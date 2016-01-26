﻿module RecomputeLighting

// recompute the BlockLight/SkyLight/HeightMap/LightPopulated values after making changes

// TODO how to do large sections that don't all fit in memory at once?
//  - compute e.g. chunk 0,0 alone, then compute chunk 0,1 using the boundary values from 0,0 as sources
//      - but this won't find light that 'goes around the corner from 0,1 into 0,0 and then back into 0,1', will it? could repeat entire chunk (16 blocks far enough can't propagate)
//      - had a discussion with codewarrior; for now i think i will just redo one chunk at the boundary. this lends towards large scale work (more volume/less surface means less re-work at boundaries), but beware of tension that cache/locality get poorer at large scale (e.g. my hashset of 15-sources is walking all over memory)
//      - can't just 'redo from scratch' tho, or will miss light coming in from completed side.  need to use known-correct wall as outside sources too...
//  - then can test how long it takes to compute light of whole region at different 'chunking' sizes
//  - and can test for correctness against a saved 'good' copy

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

let recomputeLightCore(map:MapFolder, canChange, allSources, isSky) =
    // bucket all to-propogate light sources by light level (1-15)
    let sourcesByLevel = Array.init 16 (fun _ -> new System.Collections.Generic.HashSet<_>())
    for x,y,z in allSources do
        let _nbt,_bids,_blockData,blockLight,skyLight = map.GetSection(x,y,z)
        let light = if isSky then skyLight else blockLight
        let bl = NibbleArray.get(light,x,y,z)
        if bl <> 0uy then
            sourcesByLevel.[int bl].Add(x,y,z) |> ignore
    // propogate light at each level
    for level = 15 downto 1 do
        printfn "There are %d sources at level %d" sourcesByLevel.[level].Count level
        for x,y,z in sourcesByLevel.[level] do
            let _nbt,_bids,_blockData,blockLight,skyLight = map.GetSection(x,y,z)
            let light = if isSky then skyLight else blockLight
            let curLight = NibbleArray.get(light,x,y,z)
            assert(curLight = byte level)
            for dx,dy,dz in [| 0,0,1; 0,1,0; 1,0,0; 0,0,-1; 0,-1,0; -1,0,0 |] do
                if canChange(x+dx,y+dy,z+dz) then
                    let neighborBID = map.GetBlockInfo(x+dx,y+dy,z+dz).BlockID 
                    let neighborLevelBasedOnMySpread = max 0 (level - OPACITY.[int neighborBID])
                    assert(neighborLevelBasedOnMySpread < level)
                    if neighborLevelBasedOnMySpread > 0 then
                        let _,_,_,neighborBlockLight,neighborSkyLight = map.GetSection(x+dx,y+dy,z+dz)
                        let neighborLight = if isSky then neighborSkyLight else neighborBlockLight
                        let curNeighborLevel = NibbleArray.get(neighborLight,x+dx,y+dy,z+dz)
                        if curNeighborLevel < byte neighborLevelBasedOnMySpread then
                            NibbleArray.set(neighborLight,x+dx,y+dy,z+dz,byte neighborLevelBasedOnMySpread)
                            sourcesByLevel.[neighborLevelBasedOnMySpread].Add(x+dx,y+dy,z+dz) |> ignore
    ()
let recomputeBlockLightHelper(map:MapFolder, canChange, blockLightSources) = // blockLightSources is coords of all light-emitting blocks
    // TODO assert all blockLightSources have correct non-zero value
    recomputeLightCore(map, canChange, blockLightSources, false)
let recomputeSkyLightHelper(map:MapFolder, canChange, skyLightSources) = // skyLightSources is coords of all blocks from ceiling down to last _fully_ transparent block
    // TODO assert all skyLightSources have value 15
    // TODO wiki suggests leaves/cobweb/ice are different, but I cannot find anything that suggests I need to handle them differently
    recomputeLightCore(map, canChange, skyLightSources, true)

let LIGHTBIDS = new System.Collections.Generic.Dictionary<_,_>()
for bid, lvl in MC_Constants.BLOCKIDS_THAT_EMIT_LIGHT do
    LIGHTBIDS.Add(byte bid, byte lvl)

let recomputeBlockLight(map:MapFolder,minx,minz,maxx,maxz) =
    let lightSources = ResizeArray()
    printf "finding light sources and initting light to 0..."
    // for every represented block
    for x = minx to maxx do
        printf "."
        for z = minz to maxz do
            for y = 0 to 255 do
                let bi = map.MaybeGetBlockInfo(x,y,z)
                if bi <> null then
                    let _,_,_,blockLight,_ = map.GetSection(x,y,z)
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
    recomputeBlockLightHelper(map, canChange, lightSources)

let recomputeSkyLight(map:MapFolder,cachedHeightMap:_[,],minx,minz,maxx,maxz) =
    let lightSources = ResizeArray()
    printf "finding light sources and initting light to 0..."
    // for every represented block
    for x = minx to maxx do
        printf "."
        for z = minz to maxz do
            let mutable y = 255
            let hm = cachedHeightMap.[x,z]
            // everything above heightmap is a source
            while y >= hm && y >= 0 do
                let bi = 
                    if y = hm then map.GetBlockInfo(x,y,z)  // if y=hm, need to force-create the section above topmost block (might be unrepresented), so that we can put the lowest source there
                    // TODO line above will create some sky with light 0 we've already gone past the coords of, bug...
                    // ...not sure how to deal with; one way is to create all sections, then cull any 'all sky' ones before writing to disk
                    else map.MaybeGetBlockInfo(x,y,z)
                if bi <> null then
                    let _,_,_,_,skyLight = map.GetSection(x,y,z)
                    NibbleArray.set(skyLight,x,y,z,15uy)
                    // we don't need to set _every_ block in the sky as a light source; there's tons of sky that's all sources
                    // as a result, only start setting sources as we reach terrain, e.g. if there's a nearby block above the heightmap
                    let mutable terrainNearby = false
                    for dx,dz in [| -1,-1; -1,0; -1,1; 0,-1; 0,0; 0,1; 1,-1; 1,0; 1,1 |] do
                        let x,z = x+dx, z+dz
                        if x >= minx && x <= maxx && z >= minz && z <= maxz then
                            if y <= cachedHeightMap.[x,z]+1 then
                                terrainNearby <- true
                    if terrainNearby then
                        lightSources.Add(x,y,z)
                y <- y - 1
            // from there down, everything _fully_ transparent is still a source
            let fullyTrans = new System.Collections.Generic.HashSet<_>(MC_Constants.BLOCKIDS_THAT_ARE_FULLY_TRANSPARENT_TO_LIGHT |> Seq.map byte)
            while y >= 0 && fullyTrans.Contains(map.GetBlockInfo(x,y,z).BlockID) do
                let _,_,_,_,skyLight = map.GetSection(x,y,z)
                lightSources.Add(x,y,z)
                NibbleArray.set(skyLight,x,y,z,15uy)
                y <- y - 1
            // done with sources, now init the rest
            while y >= 0 do
                let _,_,_,_,skyLight = map.GetSection(x,y,z)
                // set its skyLight to 0
                NibbleArray.set(skyLight,x,y,z,0uy)
                y <- y - 1
    printfn "done!"
    printfn "recomputing light..."
    let canChange(x,y,z) = x >= minx && x <= maxx && y >= 0 && y <= 255 && z >= minz && z <= maxz
    // recompute sky light
    recomputeSkyLightHelper(map, canChange, lightSources)

(*
let comparePerformanceAtDifferentChunkingSizes() =
    let sampleRegionFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RCTM109\region\"""
    //let F = testBlockLightByComparingToMinecraft
    let F = testSkyLightByComparingToMinecraft
    let elapsedAtOnce = F(sampleRegionFolder,0,0,511,511)
    let mutable elapsedInPieces3 = 0L
    let mutable elapsedInPieces7 = 0L
    for x = 0 to 7 do
        for z = 0 to 7 do
            // TODO more boundaries -> more incorrect
            let r = F(sampleRegionFolder,x*64,z*64,x*64+63,z*64+63)
            elapsedInPieces7 <- elapsedInPieces7 + r
    for x = 0 to 3 do
        for z = 0 to 3 do
            // TODO more boundaries -> more incorrect
            let r = F(sampleRegionFolder,x*128,z*128,x*128+127,z*128+127)
            elapsedInPieces3 <- elapsedInPieces3 + r
    printfn "Time when one: %d" elapsedAtOnce 
    printfn "Time when 0-7: %d" elapsedInPieces7 
    printfn "Time when 0-3: %d" elapsedInPieces3
    // skylight: very similar times at different chunking sizes (25-27s)
    // blocklight: slightly favors larger chunking sizes (8-10s)
*)

let fixLighting(minx, minz, maxx, maxz) =
    let sampleRegionFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RCTM109\region\"""
    let originalRegionFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RCTM109OriginalLighting\region\"""
    let fixedRegionFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RCTM109CorrectedLighting\region\"""
    System.IO.Directory.CreateDirectory(originalRegionFolder) |> ignore
    System.IO.Directory.CreateDirectory(fixedRegionFolder) |> ignore
    System.IO.File.Copy(sampleRegionFolder+"r.0.0.mca", originalRegionFolder+"r.0.0.mca", true)
    System.IO.File.Copy(sampleRegionFolder+"r.0.0.mca", fixedRegionFolder+"r.0.0.mca", true)

    let mapToChange = new MapFolder(fixedRegionFolder)
    printfn "loading map..."
    mapToChange.GetOrCreateAllSections(minx,maxx,0,255,minz,maxz)
    printfn "recomputing block light..."
    recomputeBlockLight(mapToChange, minx, minz, maxx, maxz)
    // cache height map
    let hm = Array2D.zeroCreateBased minx minz (maxx-minx+1) (maxz-minz+1)
    for x = minx to maxx do
        for z = minz to maxz do
            mapToChange.GetBlockInfo(x,0,z) |> ignore // originally caches HeightMap
            hm.[x,z] <- mapToChange.GetHeightMap(x,z)
    printfn "recomputing sky light..."
    recomputeSkyLight(mapToChange, hm, minx, minz, maxx, maxz)
    printfn "comparing results..."
    // compare
    let origMap = new MapFolder(originalRegionFolder)
    let mutable numBlockDiff,numSkyDiff = 0,0
    for x = minx to maxx do
        printf "."
        for z = minz to maxz do
            for y = 0 to 255 do
                let _,_,_,origBlockLight,origSkyLight = origMap.GetSection(x,y,z)
                if origBlockLight <> null then
                    let _,_,_,newBlockLight,newSkyLight = mapToChange.GetSection(x,y,z)
                    let origValue = NibbleArray.get(origBlockLight,x,y,z)
                    let testValue = NibbleArray.get(newBlockLight,x,y,z)
                    if origValue <> testValue then
                        //printfn "%3d %3d %3d differ, orig %2d test %2d" x y z origValue testValue 
                        numBlockDiff <- numBlockDiff + 1
                    let origValue = NibbleArray.get(origSkyLight,x,y,z)
                    let testValue = NibbleArray.get(newSkyLight,x,y,z)
                    if origValue <> testValue then
                        //printfn "%3d %3d %3d differ, orig %2d test %2d" x y z origValue testValue 
                        numSkyDiff <- numSkyDiff + 1
    printfn "done!"
    printfn "There were %d block and %d sky differences" numBlockDiff numSkyDiff
    printfn "saving results..."
    mapToChange.WriteAll()


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

skylight is similar to blocklight
any time a block is placed above HM, need to fix HM, and also ensure sections exist from bottom up to that block, recompute
any time a block is removed at HM, need to fix HM, recompute

*)


