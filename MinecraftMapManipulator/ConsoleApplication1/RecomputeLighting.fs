module RecomputeLighting

// recompute the BlockLight/SkyLight/HeightMap/LightPopulated values after making changes

// need to be careful with leaves/webs & skylight
// need to be careful that slabs/stairs/farmland/?path? are opaque but have self light-level of neighbor

let INTRINSIC_BRIGHTNESS =
    let a = Array.zeroCreate 256
    for bid, ib in MC_Constants.BLOCKIDS_THAT_EMIT_LIGHT do
        a.[bid] <- ib
    a

let brightness(bid) = INTRINSIC_BRIGHTNESS.[bid]

let OPACITY =
    let a = Array.create 256 15
    for bid in MC_Constants.BLOCKIDS_THAT_ARE_FULLY_TRANSPARENT_TO_LIGHT do
        a.[bid] <- 1
    for bid in MC_Constants.BLOCKIDS_THAT_FILTER_SKYLIGHT do
        a.[bid] <- 1
    for bid in MC_Constants.BLOCKIDS_THAT_LOWER_LIGHT_BY_TWO do
        a.[bid] <- 3
    a

let opacity(bid) = OPACITY.[bid]

open RegionFiles

let recomputeBlockLight(map:MapFolder, canChange, allSources) =
    // bucket all to-propogate light sources by light level (1-15)
    let sourcesByLevel = Array.init 16 (fun _ -> new System.Collections.Generic.HashSet<_>())
    for x,y,z in allSources do
        let _nbt,_bids,_blockData,blockLight,_skyLight = map.GetSection(x,y,z)
        let bl = NibbleArray.get(blockLight,x,y,z)
        if bl <> 0uy then
            sourcesByLevel.[int bl].Add(x,y,z) |> ignore
    // propogate light at each level
    for level = 15 downto 1 do
        printfn "There are %d sources at level %d" sourcesByLevel.[level].Count level
        for x,y,z in sourcesByLevel.[level] do
            let _nbt,_bids,_blockData,blockLight,_skyLight = map.GetSection(x,y,z)
            let curLight = NibbleArray.get(blockLight,x,y,z)
            assert(curLight = byte level)
            for dx,dy,dz in [| 0,0,1; 0,1,0; 1,0,0; 0,0,-1; 0,-1,0; -1,0,0 |] do
                if canChange(x+dx,y+dy,z+dz) then
                    let neighborBID = map.GetBlockInfo(x+dx,y+dy,z+dz).BlockID 
                    let neighborLevelBasedOnMySpread = max 0 (level - OPACITY.[int neighborBID])
                    assert(neighborLevelBasedOnMySpread < level)
                    if neighborLevelBasedOnMySpread > 0 then
                        let _,_,_,neighborBlockLight,_ = map.GetSection(x+dx,y+dy,z+dz)
                        let curNeighborLevel = NibbleArray.get(neighborBlockLight,x+dx,y+dy,z+dz)
                        if curNeighborLevel < byte neighborLevelBasedOnMySpread then
                            NibbleArray.set(neighborBlockLight,x+dx,y+dy,z+dz,byte neighborLevelBasedOnMySpread)
                            sourcesByLevel.[neighborLevelBasedOnMySpread].Add(x+dx,y+dy,z+dz) |> ignore
    ()

let testByComparingToMinecraft(mapFolderPath,minx,minz,maxx,maxz) =
    printfn "loading map..."
    let origMap = new MapFolder(mapFolderPath)
    // force every region file to get read in
    for x in [minx .. 512 .. maxx] do
        for z in [minz .. 512 .. maxz] do
            origMap.GetBlockInfo(x,0,z) |> ignore
    let testMap = new MapFolder(mapFolderPath)
    // force every region file to get read in
    for x in [minx .. 512 .. maxx] do
        for z in [minz .. 512 .. maxz] do
            testMap.GetBlockInfo(x,0,z) |> ignore
    // now do computations on testMap, and compare results to origMap
    let LIGHTBIDS = new System.Collections.Generic.Dictionary<_,_>()
    for bid, lvl in MC_Constants.BLOCKIDS_THAT_EMIT_LIGHT do
        LIGHTBIDS.Add(byte bid, byte lvl)
    let lightSources = ResizeArray()
    printf "finding light sources and initting light to 0..."
    // for every represented block
    for x = minx to maxx do
        printf "."
        for z = minz to maxz do
            for y = 0 to 255 do
                let bi = testMap.MaybeGetBlockInfo(x,y,z)
                if x=86 && y=26 && z=77 then
                    printfn "should be lava"
                if bi <> null then
                    let _,_,_,blockLight,_ = testMap.GetSection(x,y,z)
                    // if it emits light, add it to light sources
                    if LIGHTBIDS.ContainsKey(bi.BlockID) then
                        lightSources.Add(x,y,z)
                        NibbleArray.set(blockLight,x,y,z,LIGHTBIDS.[bi.BlockID])
                    else
                        // set its blockLight to 0   // TODO would be faster memory-wise to just do this in bulk to every section, and process x/y/z by sections
                        NibbleArray.set(blockLight,x,y,z,0uy)
    printfn "done!"
    printfn "recomputing light..."
    let canChange(x,y,z) = x >= minx && x <= maxx && y >= 0 && y <= 255 && z >= minz && z <= maxz
    // recompute block light
    recomputeBlockLight(testMap, canChange, lightSources)
    printfn "comparing results..."
    // compare
    let mutable numDiff = 0
    for x = minx to maxx do
        for z = minz to maxz do
            for y = 0 to 255 do
                let _,_,_,origBlockLight,_ = origMap.GetSection(x,y,z)
                if origBlockLight <> null then
                    let _,_,_,testBlockLight,_ = testMap.GetSection(x,y,z)
                    let origValue = NibbleArray.get(origBlockLight,x,y,z)
                    let testValue = NibbleArray.get(testBlockLight,x,y,z)
                    if origValue <> testValue then
                        printfn "%3d %3d %3d differ, orig %2d test %2d" x y z origValue testValue 
                        numDiff <- numDiff + 1
    printfn "There were %d differences" numDiff


(*

blocklight:

have set of 'could be changed by update' (easy to compute at start).  
all blocks with intrinsic light values there are noted, as well as all points on the surface boundary, as light 'sources' with strength N

then for N = 15 downto 0 do

forall sources with strength N, set block light to N (unless already higher, in which case stop), 
then add all neighbors to lists of N-minus-their-opacity sources (unless they were already brighter)

thus we have the 15 wavefront, then the 14 wavefront, ... no work is done for e.g. red torches until we reach iter 7

each block is only ever set once

-----------------------

the 'could be changed by update' set is useful for a single change or a set of changes
to recompute entire chunk/region at once, probably best to just do everything, e.g. the 'could be changed' is 'everything'
hm, do we need the 'could be changed' set? does the wavefront implicitly do this?
aha, yes, we need if for dimming - when light is dimmed or removed (includes e.g. putting stone to replace air), we need to
 - compute the changed set (the N-radius around the N-light-removed block)
 - note its frontier (set on the boundary/edge of the region)
 - after unioning all the changed sets from all the changes, union the frontiers and then for each guy on the frontier,
     check all his neighbors for blocks outside the change scope, these become the new 'sources' that will bleed in

-----------------------

we can address all the light arrays relatively quickly (O(1)) via the region files (need to code up the direct access)
each section is 2048 bytes of BlockLight
each chunk is up to 16*2048 bytes
each region is up to 32*32*16*2048 = 33,554,432 bytes = 32MB of memory just for the BlockLight info (but we've been holding 16 regions in memory fine with other stuff)

-----------------------

skylight is similar to blocklight
everything above HM is a source of 15
some special work for leaves/cobwebs
then basic lighting computation
any time a block is placed above HM, need to fix HM, and also ensure sections exist from bottom up to that block, recompute
any time a block is removed at HM, need to fix HM, recompute

*)


