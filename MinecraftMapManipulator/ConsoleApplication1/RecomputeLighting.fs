module RecomputeLighting

// recompute the BlockLight/SkyLight/HeightMap/LightPopulated values after making changes

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

// this is an additive, batch-repainting-of-sources
let recomputeLightCore(map:MapFolder, canChange, sourcesByLevel:System.Collections.Generic.HashSet<_>[], isSky) =
    // propogate light at each level
    for level = 15 downto 1 do
        //printfn "There are %d sources at level %d" sourcesByLevel.[level].Count level
        for x,y,z in sourcesByLevel.[level] do
            let _nbt,_bids,_blockData,blockLight,skyLight = map.GetSection(x,y,z)
            let light = if isSky then skyLight else blockLight
            let curLight = NibbleArray.get(light,x,y,z)
            assert(curLight = byte level)
            for dx,dy,dz in [| 0,0,1; 0,1,0; 1,0,0; 0,0,-1; 0,-1,0; -1,0,0 |] do
                // TODO have canChange guarding all logic here, but probably want it guarding only the writing-to-chunk, so I can still correctly deal with loopback outside canChange region
                // though this does beg the question of what about chunks that have not been generated yet? if i'm at the true edge of the world, how do I reason about light traveling around corners?
                // no answer is right, so I think it's fine (and more efficient) to say that light does not travel outside of the world.  however this means I need a way to distinguish canChange from canRead.
                // and does this interact with any of the other algorithms?
                // TODO also, the canChange guard here was originally for local changes and trimming the algorithm, which I'm now kinda abandoning it seems? 
                // like, then it actually meant 'do we need to bother', so you wouldn't prop light at one corner of the chunk when the only thing to account for was a dimming at the other corner.
                // but then possibly economy of scale and algorithmic considerations may make 'sections' the smallest reasonable amount of granularity, in which case
                // confounding 'canWrite' and 'doWeNeedToBother' may be pragmatic anyway? so e.g. if you were removing one glowstone, you would mark all the sections it could touch,
                // make them 'canWrite', and then run the repainting algorithm across those sections and all their 27-neighborhoods ('canRead's), yes?
                // in my benchmark, I could light an entire region (16k sections) in 4s. this suggests I can relight a section (process a 27-neighborhood) in 7ms.
                // (actually typically  1-block change can touch 2x2x2 sections, so I'd need a 64-section-neighborhood, but that's still under 20ms, that's fine with me.)
                // that seems plenty 'good enough' to not feel bad about section-sized granularity for containing small block-change-transaction-sets we want to relight.
                if canChange(x+dx,y+dy,z+dz) then // TODO if level=1 can stop now
                    let neighborBID = map.GetBlockInfo(x+dx,y+dy,z+dz).BlockID // TODO surely an out-of-bounds bug here in y? oh, canChange was guarding it...
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
    // TODO wiki suggests leaves/cobweb/ice are different, but I cannot find anything that suggests I need to handle them differently
    recomputeLightCore(map, canChange, skyLightSources, true)

let LIGHTBIDS = new System.Collections.Generic.Dictionary<_,_>()
for bid, lvl in MC_Constants.BLOCKIDS_THAT_EMIT_LIGHT do
    LIGHTBIDS.Add(byte bid, byte lvl)

// TODO do I need the incoming sourcesByLevel argument any more? not being used...
let recomputeBlockLight(map:MapFolder,sourcesByLevel:System.Collections.Generic.HashSet<_>[],minx,minz,maxx,maxz,canChange,shouldZero) =
    if MOD(minx,16) <> 0 || MOD(maxx,16) <> 15 || MOD(minz,16) <> 0 || MOD(maxz,16) <> 15 then
        failwith "this algorithm only works on full chunks"
    printf "finding light sources and initting light to 0..."
    // loop over every represented block, section-wise (section-wise was more than 3x faster when I benchmarked it)
    for xs in [ minx .. 16 .. maxx ] do
        for zs in [ minz .. 16 .. maxz ] do
            for ys in [ 0 .. 16 .. 255 ] do
                printf "."
                let bi = map.MaybeGetBlockInfo(xs,ys,zs)
                if bi <> null then
                    let _,blocks,_,blockLight,_ = map.GetSection(xs,ys,zs)
                    if shouldZero then
                        // TODO what if not canChange?
                        // zero out the light values to begin
                        System.Array.Clear(blockLight, 0, 2048)
                    for dx = 0 to 15 do
                        for dy = 0 to 15 do
                            for dz = 0 to 15 do
                                //let bid = map.GetBlockInfo(xs+dx,ys+dy,zs+dz).BlockID   // could use this API, but below is faster since we already have section
                                let i = dy*256 + dz*16 + dx
                                let bid = blocks.[i]
                                // if it emits light, add it to light sources
                                if LIGHTBIDS.ContainsKey(bid) then
                                    let level = LIGHTBIDS.[bid]
                                    // we may have already painted a higher level over this source (from a source outside this chunk/section), in which case this source can be ignored, so check it
                                    let existingLevel = NibbleArray.get(blockLight,xs+dx,ys+dy,zs+dz)
                                    if existingLevel < level then
                                        sourcesByLevel.[int level].Add(xs+dx,ys+dy,zs+dz) |> ignore
                                        // TODO what if not canChange?
                                        NibbleArray.set(blockLight,xs+dx,ys+dy,zs+dz,level)
    printfn "done!"
    printfn "recomputing light..."
    // recompute block light
    recomputeBlockLightHelper(map, canChange, sourcesByLevel)

// TODO do I need the incoming sourcesByLevel argument any more? not being used...
let recomputeSkyLight(map:MapFolder,sourcesByLevel:System.Collections.Generic.HashSet<_>[],cachedHeightMap:_[,],minx,minz,maxx,maxz,canChange,shouldZero) =
    let chmminx, chmminz = cachedHeightMap.GetLowerBound(0), cachedHeightMap.GetLowerBound(1)
    let chmmaxx, chmmaxz = chmminx+cachedHeightMap.GetLength(0)-1, chmminz+cachedHeightMap.GetLength(1)-1
    printf "finding light sources and initting light to 0..."
    let NINE_NEIGHBORS = [| -1,-1; -1,0; -1,1; 0,-1; 0,0; 0,1; 1,-1; 1,0; 1,1 |]
    // for every represented block
    for xs in [ minx .. 16 .. maxx ] do
        for zs in [ minz .. 16 .. maxz ] do
            printf "."
            let mutable highestHMInThisChunk,lowestHMInThisChunk = 0,255
            for dx = 0 to 15 do
                for dz = 0 to 15 do
                    let x,z = xs+dx,zs+dz
                    if cachedHeightMap.[x,z] > highestHMInThisChunk then
                        highestHMInThisChunk <- cachedHeightMap.[x,z]
                    if cachedHeightMap.[x,z] < lowestHMInThisChunk then
                        lowestHMInThisChunk <- cachedHeightMap.[x,z]
            let mutable ys = 240
            while highestHMInThisChunk < ys-1 do  // -1 because I need to represent skylight source above the highest block
                // this section is entirely above the heightmap; it could be unrepresented, or if represented, its skyLight is just all 15s
                let _,_,_,_,skyLight = map.GetSection(xs,ys,zs)
                if skyLight <> null then
                    for i = 0 to skyLight.Length-1 do
                        // TODO what if not canChange? (may not matter here if cached HM is correct)
                        skyLight.[i] <- 255uy  // 255 is (15 <<< 4) + 15
                ys <- ys - 16
            if ys = highestHMInThisChunk then // special-case this for efficiency - all sky, but with terrain adjacent below; just want to ensure is represented
                let _,_,_,_,skyLight = map.GetOrCreateSection(xs,ys,zs)
                for i = 0 to skyLight.Length-1 do
                    // TODO what if not canChange? (may not matter here if cached HM is correct)
                    skyLight.[i] <- 255uy  // 255 is (15 <<< 4) + 15
                ys <- ys - 16
            // now the meat, we're into sections with some bits below the heightmap
            while ys >=0 do
                let _,_,_,_,skyLight = map.GetOrCreateSection(xs,ys,zs) // represent all sections below heightmap
                for dx = 0 to 15 do
                    for dz = 0 to 15 do
                        let x,z = xs+dx,zs+dz
                        let curHM = cachedHeightMap.[x,z]
                        for dy = 0 to 15 do
                            let y = ys+dy
                            if y >= curHM then
                                // TODO what if not canChange?
                                NibbleArray.set(skyLight,x,y,z,15uy)
                                // we don't need to set _every_ block in the sky as a light source; there's tons of sky that's all sources
                                // as a result, only start setting sources as we reach terrain
                                let mutable terrainNearby = false
                                for dx,dz in NINE_NEIGHBORS do
                                    let x,z = x+dx, z+dz
                                    if x >= chmminx && x <= chmmaxx && z >= chmminz && z <= chmmaxz then // if we can't read at the edge, then some overlapped processing will be needed for correctness when e.g. skylight is here and overhang is just outside readable data.
                                        if y <= cachedHeightMap.[x,z]+1 then
                                            terrainNearby <- true
                                if terrainNearby then
                                    sourcesByLevel.[15].Add(x,y,z) |> ignore
                            else
                                if shouldZero then
                                    // TODO what if not canChange?
                                    NibbleArray.set(skyLight,x,y,z,0uy) // init all non-sources to 0
                ys <- ys - 16
    printfn "done!"
    printfn "recomputing light..."
    // recompute sky light
    recomputeSkyLightHelper(map, canChange, sourcesByLevel)

let compareLighting(map1:MapFolder, map2:MapFolder, minx, minz, maxx, maxz) =
    if MOD(minx,16) <> 0 || MOD(maxx,16) <> 15 || MOD(minz,16) <> 0 || MOD(maxz,16) <> 15 then
        failwith "this algorithm only works on full chunks"
    printfn "comparing results..."
    // compare
    let mutable numBlockDiff,numSkyDiff,numSectDiff = 0,0,0
    for xs in [ minx .. 16 .. maxx ] do
        for zs in [ minz .. 16 .. maxz ] do
            for ys in [ 0 .. 16 .. 255 ] do
                printf "."
                let _,_,_,origBlockLight,origSkyLight = map1.GetSection(xs,ys,zs)
                if origBlockLight <> null then
                    let _,_,_,newBlockLight,newSkyLight = map2.GetSection(xs,ys,zs)
                    if newBlockLight = null then
                        //printfn "%3d %3d %3d differ, orig populated section, test no section" xs ys zs
                        numSectDiff <- numSectDiff + 1
                    else
                        for dx = 0 to 15 do
                            for dy = 0 to 15 do
                                for dz = 0 to 15 do
                                    let x = xs+dx
                                    let y = ys+dy
                                    let z = zs+dz
                                    let origValue = NibbleArray.get(origBlockLight,x,y,z)
                                    let testValue = NibbleArray.get(newBlockLight,x,y,z)
                                    if origValue <> testValue then
                                        printfn "BLOCK %3d %3d %3d differ, orig %2d test %2d" x y z origValue testValue 
                                        numBlockDiff <- numBlockDiff + 1
                                    let origValue = NibbleArray.get(origSkyLight,x,y,z)
                                    let testValue = NibbleArray.get(newSkyLight,x,y,z)
                                    if origValue <> testValue then
                                        //printfn "SKY %3d %3d %3d differ, orig %2d test %2d" x y z origValue testValue 
                                        numSkyDiff <- numSkyDiff + 1
    printfn "done!"
    printfn "There were %d block, %d sky, and %d section differences" numBlockDiff numSkyDiff numSectDiff

// TODO do I need the incoming xxxSourcesByLevel arguments any more? not being used... rather I'd just run the alg on a big enough min/max to encompass them, but not make them writable
let fixLighting(mapToChange:MapFolder, 
                blockLightSourcesByLevel:System.Collections.Generic.HashSet<_>[], 
                skyLightSourcesByLevel:System.Collections.Generic.HashSet<_>[], 
                minx, minz, maxx, maxz, hm, canChange,
                shouldZero) =
    // blockLightSourcesByLevel, skyLightSourcesByLevel : any sources outside [(minx,minz) - (maxx,maxz)] that should shine light in
    // minx,minz,maxx,maxz                              : range of blocks to (possibly zero and) scan for sources and shine light from
    // hm                                               : a cached heightmap of the area, at least the size of, and ideally at least one cell wider around than, [(minx,minz) - (maxx,maxz)]
    // canChange                                        : function saying which cells we can write new light values to (could e.g. be one-chunk-border-bigger-than [(minx,minz) - (maxx,maxz)])
    // shouldZero                                       : whether [(minx,minz) - (maxx,maxz)] light should be zeroed out at the start (e.g. because computing from scratch or removing/dimming, as opposed to incrementally adding)
    if MOD(minx,16) <> 0 || MOD(maxx,16) <> 15 || MOD(minz,16) <> 0 || MOD(maxz,16) <> 15 then
        failwith "this algorithm only works on full chunks"
    printfn "loading map..."
    mapToChange.GetOrCreateAllSections(minx,maxx,0,255,minz,maxz)
    printfn "recomputing block light..."
    recomputeBlockLight(mapToChange, blockLightSourcesByLevel, minx, minz, maxx, maxz, canChange, shouldZero)
    printfn "recomputing sky light..."
    recomputeSkyLight(mapToChange, skyLightSourcesByLevel, hm, minx, minz, maxx, maxz, canChange, shouldZero)

let lightingTestSetup() =
    let sampleRegionFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RCTM109\region\"""
    let originalRegionFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RCTM109OriginalLighting\region\"""
    let fixedRegionFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RCTM109CorrectedLighting\region\"""
    System.IO.Directory.CreateDirectory(originalRegionFolder) |> ignore
    System.IO.Directory.CreateDirectory(fixedRegionFolder) |> ignore
    System.IO.File.Copy(sampleRegionFolder+"r.0.0.mca", originalRegionFolder+"r.0.0.mca", true)
    System.IO.File.Copy(sampleRegionFolder+"r.0.0.mca", fixedRegionFolder+"r.0.0.mca", true)

    let minx, minz, maxx, maxz = 0, 0, 511, 511

    let mapToChange = new MapFolder(fixedRegionFolder)
    // cache height map
    let hm = Array2D.zeroCreateBased minx minz (maxx-minx+1) (maxz-minz+1)
    for x = minx to maxx do
        for z = minz to maxz do
            if MOD(x,16)=0 && MOD(z,16)=0 then
                mapToChange.GetBlockInfo(x,0,z) |> ignore // originally caches HeightMap, which is stored as arrays per-chunk
            hm.[x,z] <- mapToChange.GetHeightMap(x,z)
    let canChange(x,y,z) = x >= minx && x <= maxx && y >= 0 && y <= 255 && z >= minz && z <= maxz
    let blockLightSourcesByLevel = Array.init 16 (fun _ -> new System.Collections.Generic.HashSet<_>())
    let skyLightSourcesByLevel = Array.init 16 (fun _ -> new System.Collections.Generic.HashSet<_>())
    fixLighting(mapToChange, blockLightSourcesByLevel, skyLightSourcesByLevel, minx, minz, maxx, maxz, hm, canChange, true)
    printfn "saving results..."
    mapToChange.WriteAll()
    // compare
    let origMap = new MapFolder(originalRegionFolder)
    compareLighting(origMap, mapToChange, minx, minz, maxx, maxz)

let demoCorrectBoundaries() =
    let originalRegionFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RCTM109OriginalLighting\region\"""
    let fixedRegionFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RCTM109CorrectedLighting\region\"""

    let minx, minz, maxx, maxz = 0, 0, 511, 511

    let mapToChange = new MapFolder(originalRegionFolder)
    // cache height map
    let hm = Array2D.zeroCreateBased minx minz (maxx-minx+1) (maxz-minz+1)
    for x = minx to maxx do
        for z = minz to maxz do
            if MOD(x,16)=0 && MOD(z,16)=0 then
                mapToChange.GetBlockInfo(x,0,z) |> ignore // originally caches HeightMap, which is stored as arrays per-chunk
            hm.[x,z] <- mapToChange.GetHeightMap(x,z)
    let canChange(x,y,z) = x >= minx && x <= maxx && y >= 0 && y <= 255 && z >= minz && z <= maxz

    let sw = System.Diagnostics.Stopwatch.StartNew()

    (*
    let blockLightSourcesByLevel = Array.init 16 (fun _ -> new System.Collections.Generic.HashSet<_>())
    let skyLightSourcesByLevel = Array.init 16 (fun _ -> new System.Collections.Generic.HashSet<_>())
    fixLighting(mapToChange, blockLightSourcesByLevel, skyLightSourcesByLevel, minx, minz, maxx, maxz, hm, canChange, true)
    *)

// TODO test different size chunking for performance and correctness
    let MAX = 512
    let PARTS = 8
    let LEN = MAX / PARTS
    let hasBeenZeroed = new System.Collections.Generic.HashSet<_>()
    for x = 0 to PARTS-1 do
        for z = 0 to PARTS-1 do
            let blockLightSourcesByLevel = Array.init 16 (fun _ -> new System.Collections.Generic.HashSet<_>())
            let skyLightSourcesByLevel = Array.init 16 (fun _ -> new System.Collections.Generic.HashSet<_>())
            // zero out all lighting in this chunking-area... and also one chunk in the forward X/Z directions (the +16)
            for xs in [x*LEN .. 16 .. x*LEN+LEN-1+16] do
                for zs in [z*LEN .. 16 .. z*LEN+LEN-1+16] do
                    if xs<MAX && zs<MAX then
                        for ys in [0 .. 16 .. 255] do
                            if not(hasBeenZeroed.Contains(xs,ys,zs)) then
                                let _,_,_,blockLight,skyLight = mapToChange.GetSection(xs,ys,zs)
                                if blockLight <> null then
                                    System.Array.Clear(blockLight, 0, 2048)
                                    System.Array.Clear(skyLight, 0, 2048)
                                    hasBeenZeroed.Add(xs,ys,zs) |> ignore
            fixLighting(mapToChange,blockLightSourcesByLevel,skyLightSourcesByLevel,x*LEN,z*LEN,x*LEN+LEN-1,z*LEN+LEN-1,hm,canChange,false)

    printfn "took %dms" sw.ElapsedMilliseconds 
    // compare
    let fixedMap = new MapFolder(fixedRegionFolder)
    compareLighting(fixedMap, mapToChange, minx, minz, maxx, maxz)


let fixMyMap() =
    let map = new MapFolder("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RandomCTM147CopyWithFixxer - Copy\region\""")

    let minx, minz, maxx, maxz = 0, 0, 511, 511

    // compute and cache height map
    let isFully = new System.Collections.Generic.HashSet<_>()
    MC_Constants.BLOCKIDS_THAT_ARE_FULLY_TRANSPARENT_TO_LIGHT |> Array.iter (fun bid -> isFully.Add(byte bid) |> ignore)
    let hm = Array2D.zeroCreateBased minx minz (maxx-minx+1) (maxz-minz+1)
    for x = minx to maxx do
        printf "*"
        for z = minz to maxz do
            let mutable y,finished = 255,false
            while y>=0 && not finished do
                let bi = map.MaybeGetBlockInfo(x,y,z)
                if bi <> null then
                    if not(isFully.Contains(bi.BlockID)) then
                        finished <- true
                y <- y - 1
            y <- y + 1
            hm.[x,z] <- y
            map.SetHeightMap(x,z,y)
    printfn ""

    let canChange(x,y,z) = x >= minx && x <= maxx && y >= 0 && y <= 255 && z >= minz && z <= maxz
    
    let blockLightSourcesByLevel = Array.init 16 (fun _ -> new System.Collections.Generic.HashSet<_>())
    let skyLightSourcesByLevel = Array.init 16 (fun _ -> new System.Collections.Generic.HashSet<_>())
    fixLighting(map, blockLightSourcesByLevel, skyLightSourcesByLevel, minx, minz, maxx, maxz, hm, canChange, true)
    map.WriteAll()


    // TODO what if not canChange? should canChange be per-section? if so, how best to author that (do i want section objects? what data structure is best?)
    // basically I want to source block/light sources just outside my map to get the 'edges' right, but I don't want to modify anything outside my 16 regions

    (*
    so i want sections that know
     - most of their data (block, blockLight, skyLight)
     - can they be read
     - can they be written
     - are they currently represented (?)
     - HM info
    If I assume main goal is 'relight entire map, a region at a time' then the real data structure I need is a RegionAndALittle:
     - has cx and cz range from [-1..33], canRead(cx,cz) says if chunk exists
     - the canWrite would be cx/cz in the [0..32] range
     - the HM would be populated up front in all the readable chunks
     - the sections can be accessed directly out of the map?
    My overall algorithm still seems to not be factored right.  There's
     - locating sources
     - zeroing stuff out
     - painting light
    and they're currently all intertwined.  can I tease them apart without losing loop-fusion benefits? 
    once we have a section
     - finding all BL sources is easy (iter over all cells)
     - zeroing BL is of course easy
     - 'zeroing' SL is also easy (above/below HM are 15/0)
     - finding all SL sources is non-trivial... even with the HM, I have two issues, 
                one is peeking nearby cells outside sect for neighbors (but it just HM!), 
                other is when terrain abuts top of sect and need to represent empty sect above for sources
            I think I could pre-process HM to speed neighbor computation perhaps, e.g. windowed computation of lo and hi, 3 across, then 3 down kinda thing...
     - painting light walks all over memory per-region
    If I could really make those all independent per-section, then I think I get good factoring and can retain loop fusion benefits...

    // unrepresented:
    MaybeGetRegion // may return null
    TryGetChunk // may return None
    GetSection // may return null


    so we need a per-section
    accumulateBlockLightSourcesAndMaybeInitialize()
    accumulateSkyLightSourcesAndMaybeInitialize()

    let hasBeenZeroed = new System.Collections.Generic.HashSet<_>()  // cx and cz
    for rx/rz in -99 to +99 do
        if MaybeGetRegion then
            make a heightmapcache (that is one chunk and one cell greater around), init to all 255s
            for cx/cz in -2..33 do
                if tryGetChunk then
                    update heightmapcache
            for cx/cz in 0..31 do
                if tryGetChunk then
                    for ys = 0 to 15 do
                        findSources, and initialize if not not HasBeenZeroed
                        (if HasBeenZeroed, can remove from set, won't ask again)
            for all the chunks just outside the region, attempt to get them with MaybeGetRegion/TryGetChunk, and
                findSources them too, and if they are in +X or +Z direction initialize (and add to set)
            now we have all sources for our RegionAndALittle, so run the repaint light core, saying to only write to our region
    *)

let accumulateBlockLightSourcesAndMaybeInitialize(sourcesByLevel:System.Collections.Generic.HashSet<_>[],shouldInitialize,xs,ys,zs,blocks:_[],blockLight:_[]) =
    if shouldInitialize then
        System.Array.Clear(blockLight, 0, 2048)
    for dx = 0 to 15 do
        for dy = 0 to 15 do
            for dz = 0 to 15 do
                let i = dy*256 + dz*16 + dx
                let bid = blocks.[i]
                // if it emits light, add it to light sources
                if LIGHTBIDS.ContainsKey(bid) then
                    let level = LIGHTBIDS.[bid]
                    // we may have already painted a higher level over this source (from a source outside this chunk/section), in which case this source can be ignored, so check it
                    let existingLevel = NibbleArray.get(blockLight,dx,dy,dz)
                    if existingLevel < level then
                        sourcesByLevel.[int level].Add(xs+dx,ys+dy,zs+dz) |> ignore
                        if shouldInitialize then
                            NibbleArray.set(blockLight,dx,dy,dz,level)

let FIVE_NEIGHBORS = [| 0,0,1; 1,0,0; 0,0,-1; 0,1,0; -1,0,0 |] // 4 beside and 1 above, places where we may find direct sunshine
let accumulateSkyLightSourcesAndMaybeInitialize(sourcesByLevel:System.Collections.Generic.HashSet<_>[],shouldInitialize,xs,ys,zs,blocks:_[],skyLight:_[],cachedHeightMap:_[,]) =
    for dx = 0 to 15 do
        for dz = 0 to 15 do
            let x,z = xs+dx,zs+dz
            let curHM = cachedHeightMap.[x,z]
            for dy = 0 to 15 do
                let y = ys+dy
                if y >= curHM then
                    if shouldInitialize then
                        NibbleArray.set(skyLight,x,y,z,15uy)
                else
                    // rather than treat any of the above-HM cells as sources (there are tons!), instead we'll find blocks that are below the HM but adjacent to skylight sources,
                    // and treat those terrain blocks as the sources for the relighting algorithm.
                    // This has the nice side-effect of not having to deal with 'sources' in unrepresented sections in the sky; all 'sources' are in represented sections.
                    let mutable pureSkyNearby = false
                    for dx,dy,dz in FIVE_NEIGHBORS do
                        let x,y,z = x+dx,y+dy,z+dz
                        if y >= cachedHeightMap.[x,z] then
                            pureSkyNearby <- true
                    let mutable initValue = 0uy
                    if pureSkyNearby then
                        // see if we may be a source
                        let i = dy*256 + dz*16 + dx
                        let bid = blocks.[i]
                        let myLightLevel = max 0 (15 - OPACITY.[int bid])
                        if myLightLevel > 0 then
                            sourcesByLevel.[myLightLevel].Add(x,y,z) |> ignore
                            initValue <- byte myLightLevel
                    if shouldInitialize then
                        NibbleArray.set(skyLight,x,y,z,initValue)
    ()

// this is an additive, batch-repainting-of-sources
let ADJACENCIES = [| 0,0,1; 0,1,0; 1,0,0; 0,0,-1; 0,-1,0; -1,0,0 |]
let newRecomputeLightCore(map:MapFolder, sourcesByLevel:System.Collections.Generic.HashSet<_>[], isSky) =
    // propogate light at each level
    for level = 15 downto 1 do
        //printfn "There are %d sources at level %d" sourcesByLevel.[level].Count level
        for x,y,z in sourcesByLevel.[level] do
            let _nbt,_bids,_blockData,blockLight,skyLight = map.MaybeMaybeGetSection(x,y,z)
            let light = if isSky then skyLight else blockLight
            let curLight = NibbleArray.get(light,x,y,z)
            // Note: curLight typically will equal level, but may be greater, e.g. when there is a brown mushroom (source level 1) which was found as a source, but then later painted over with a brighter source
            //assert( curLight >= byte level ) // TODO this assert sometimes fails - how/why?
            if level > 1 then
                for dx,dy,dz in ADJACENCIES do
                    let bi = if y+dy>=0 && y+dy<=255 then map.MaybeGetBlockInfo(x+dx,y+dy,z+dz) else null
                    if bi <> null then // null means outside the map (outside y, or outside represented chunks)
                        let neighborLevelBasedOnMySpread = max 0 (level - OPACITY.[int bi.BlockID])
                        assert(neighborLevelBasedOnMySpread < level)
                        if neighborLevelBasedOnMySpread > 0 then
                            let _,_,_,neighborBlockLight,neighborSkyLight = map.MaybeMaybeGetSection(x+dx,y+dy,z+dz)
                            let neighborLight = if isSky then neighborSkyLight else neighborBlockLight
                            assert(neighborLight <> null) // 'bi' existed, so this must too
                            let curNeighborLevel = NibbleArray.get(neighborLight,x+dx,y+dy,z+dz)
                            if curNeighborLevel < byte neighborLevelBasedOnMySpread then
                                sourcesByLevel.[neighborLevelBasedOnMySpread].Add(x+dx,y+dy,z+dz) |> ignore
                                NibbleArray.set(neighborLight,x+dx,y+dy,z+dz,byte neighborLevelBasedOnMySpread)

let relightTheWorldHelper(map:MapFolder, rxs, rzs) =
    let isFully = new System.Collections.Generic.HashSet<_>()
    MC_Constants.BLOCKIDS_THAT_ARE_FULLY_TRANSPARENT_TO_LIGHT |> Array.iter (fun bid -> isFully.Add(byte bid) |> ignore)
    let hasBeenInitialized = new System.Collections.Generic.HashSet<_>()
    for rx in rxs do
        for rz in rzs do
            let r = map.MaybeGetRegion(rx*512,rz*512)
            if r <> null then
                printf "r.%3d.%3d... calc HM..." rx rz
                let sw = System.Diagnostics.Stopwatch.StartNew()
                // make a heightmapcache (that is one chunk plus one cell greater around), init to all 255s
                let minx = rx * 512  // bottom coords of region
                let minz = rz * 512
                let heightMapCache = Array2D.createBased (minx-17) (minz-17) (512+17+17) (512+17+17) 255
                // populate the heightmap cache for all represented chunks
                for x = minx-17 to minx+512+17-1 do
                    for z = minz-17 to minz+512+17-1 do
                        let r = map.MaybeGetRegion(x,z)
                        if r <> null then
                            let bi = r.MaybeGetBlockInfo(x,0,z)
                            if bi <> null then
                                // calculate the height map (don't trust the contents from disk)
                                // TODO an option to 'trust the HM' would probably make this run twice as fast or something
                                let mutable y = 255  // TODO how does minecraft represent an opaque block at the build height in the HM? does it use 256? is that why int and not byte?
                                heightMapCache.[x,z] <- 0
                                while y >= 0 do
                                    let bi = r.MaybeGetBlockInfo(x,y,z)
                                    if bi <> null && not(isFully.Contains(bi.BlockID)) then
                                        heightMapCache.[x,z] <- y+1
                                        r.SetHeightMap(x,z,y+1)
                                        // now also ensure every section below here is represented, as unrepresented sections below HM are incorrectly lit by MC (e.g. air just above flat plane of blocks at section boundary looks wrong)
                                        y <- y - 16
                                        while y >= 0 do
                                            r.GetOrCreateSection(x,y,z) |> ignore
                                            y <- y - 16
                                    else 
                                        y <- y - 1
                printf "...relight..."
                // accumulate all light sources (and initialize any uninitalized chunks light arrays)
                let blockLightSourcesByLevel = Array.init 16 (fun _ -> new System.Collections.Generic.HashSet<_>())
                let skyLightSourcesByLevel = Array.init 16 (fun _ -> new System.Collections.Generic.HashSet<_>())
                for x in [minx-16 .. 16 .. minx+512+16-1] do
                    for z in [minz-16 .. 16 .. minz+512+16-1] do
                        let r = map.MaybeGetRegion(x,z)
                        if r <> null then
                            let bi = r.MaybeGetBlockInfo(x,0,z)
                            if bi <> null then
                                let cx = ((x+51200)%512)/16
                                let cz = ((z+51200)%512)/16
                                let chunkId = (rx*32+cx)*100000 + (rz*32+cz)
                                let needsInit = hasBeenInitialized.Add(chunkId)
                                for y in [0 .. 16 .. 255] do
                                    let _nbt,blocks,_blockData,blockLight,skyLight = r.GetSection(x,y,z)
                                    if blocks <> null then
                                        accumulateBlockLightSourcesAndMaybeInitialize(blockLightSourcesByLevel,needsInit,x,y,z,blocks,blockLight)
                                        accumulateSkyLightSourcesAndMaybeInitialize(skyLightSourcesByLevel,needsInit,x,y,z,blocks,skyLight,heightMapCache)
                // light it up!
                newRecomputeLightCore(map, blockLightSourcesByLevel, false)
                newRecomputeLightCore(map, skyLightSourcesByLevel, true)
                printfn " ...took %dms" sw.ElapsedMilliseconds 

let relightTheWorld(map:MapFolder) = relightTheWorldHelper(map, [-99..99], [-99..99])
// TODO known oddities
//  - RandomCTM (seed 109) at -72 80 -809, top surface of flat peak mini-bedrock is dark (unrepresented air?)
//  - RandomCTM (seed 109) at 618 66 671, jack-o-lantern does not give off light
// In both cases I appear to be writing the correct data, so maybe MC eats it, e.g.
//  - maybe MC culls empty sections, throwing away block light?
//  - maybe MC is intolerant to jack-o-lanterns sitting atop non-opaque blocks (you can't place them there)?

let demoFixTheWorld() =
    let originalRegionFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RCTM109OriginalLighting\region\"""
    let fixedRegionFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RCTM109CorrectedLighting\region\"""
    let testMap = new MapFolder(originalRegionFolder)
    testMap.GetRegion(0,0) |> ignore
    let sw = System.Diagnostics.Stopwatch.StartNew()
    relightTheWorld(testMap)
    printfn "took %dms" sw.ElapsedMilliseconds 
    let goodMap = new MapFolder(fixedRegionFolder)
    compareLighting(goodMap, testMap, 0, 0, 511, 511)
