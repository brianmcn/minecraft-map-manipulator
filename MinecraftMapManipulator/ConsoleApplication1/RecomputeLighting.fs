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

let LIGHTBIDS = Array.zeroCreate 256 // block ids just go 0-255
for bid, lvl in MC_Constants.BLOCKIDS_THAT_EMIT_LIGHT do
    LIGHTBIDS.[int bid] <- byte lvl

let compareLighting(map1:MapFolder, map2:MapFolder, minx, minz, maxx, maxz) =
    if MOD(minx,16) <> 0 || MOD(maxx,16) <> 15 || MOD(minz,16) <> 0 || MOD(maxz,16) <> 15 then
        failwith "this algorithm only works on full chunks"
    printfn "comparing results..."
    // compare
    let mutable numBlockDiff,numSkyDiff,numSectDiff,numHMDiff = 0,0,0,0
    for xs in [ minx .. 16 .. maxx ] do
        for zs in [ minz .. 16 .. maxz ] do
            printf "."
            if map1.MaybeGetBlockInfo(xs,0,zs)<> null && map2.MaybeGetBlockInfo(xs,0,zs)<> null then
                let hm1 = map1.GetHeightMap(xs,zs)
                let hm2 = map2.GetHeightMap(xs,zs)
                if hm1 <> hm2 then
                    printfn "heightmap differ at %d %d, values of %d versus %d" xs zs hm1 hm2
                    numHMDiff <- numHMDiff + 1
            for ys in [ 0 .. 16 .. 255 ] do
                let _,_,_,origBlockLight,origSkyLight = map1.GetSection(xs,ys,zs)
                if origBlockLight <> null then
                    let _,_,_,newBlockLight,newSkyLight = map2.GetSection(xs,ys,zs)
                    if newBlockLight = null then
                        printfn "%3d %3d %3d differ, orig populated section, test no section" xs ys zs
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
                                        printfn "SKY %3d %3d %3d differ, orig %2d test %2d" x y z origValue testValue 
                                        numSkyDiff <- numSkyDiff + 1
                else
                    let _,_,_,newBlockLight,_newSkyLight = map2.GetSection(xs,ys,zs)
                    if newBlockLight <> null then
                        printfn "%3d %3d %3d differ, orig no section, test had section" xs ys zs
                        numSectDiff <- numSectDiff + 1
    printfn "done!"
    printfn "There were %d block, %d sky, %d section and %d HM differences" numBlockDiff numSkyDiff numSectDiff numHMDiff

/////////////////////////////////////////

let accumulateBlockLightSourcesAndMaybeInitialize(sourcesByLevel:System.Collections.Generic.HashSet<_>[],shouldInitialize,xs,ys,zs,blocks:_[],blockLight:_[]) =
    if shouldInitialize then
        System.Array.Clear(blockLight, 0, 2048)
    for dx = 0 to 15 do
        for dy = 0 to 15 do
            for dz = 0 to 15 do
                let i = dy*256 + dz*16 + dx
                let bid = blocks.[i]
                // if it emits light, add it to light sources
                let level = LIGHTBIDS.[int bid]
                if level <> 0uy then
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

let relightTheWorldHelper(map:MapFolder, rxs, rzs, trustTheHeightMap) =
    let isFully = Array.zeroCreate 256
    MC_Constants.BLOCKIDS_THAT_ARE_FULLY_TRANSPARENT_TO_LIGHT |> Array.iter (fun bid -> isFully.[bid] <- true)
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
                                r.GetOrCreateChunk(x,z) |> ignore // create underlying heightmapcache structure // TODO should GetHeightMap/SetHeightMap do this?
                                if trustTheHeightMap then
                                    heightMapCache.[x,z] <- r.GetHeightMap(x,z)
                                else
                                    // calculate the height map (don't trust the contents from disk)
                                    let dx = MOD(x,16)
                                    let dz = MOD(z,16)
                                    let mutable y = 255  // TODO how does minecraft represent an opaque block at the build height in the HM? does it use 256? is that why int and not byte?
                                    let mutable _,curSectionBlocks,_,_,_ = r.GetSection(x,y,z)
                                    heightMapCache.[x,z] <- 0
                                    while y >= 0 do
                                        if curSectionBlocks = null then
                                            // This section is not represented, jump down to section below
                                            assert(y%16 = 15)
                                            y <- y - 16
                                            let _,blocks,_,_,_ = r.GetSection(x,y,z)
                                            curSectionBlocks <- blocks
                                        elif not(isFully.[int curSectionBlocks.[(y%16)*256+dz*16+dx]]) then
                                            heightMapCache.[x,z] <- y+1
                                            r.SetHeightMap(x,z,y+1)
                                            // now also ensure every section below here is represented, as unrepresented sections below HM are incorrectly lit by MC (e.g. air just above flat plane of blocks at section boundary looks wrong)
                                            y <- y - 16
                                            while y >= 0 do
                                                r.GetOrCreateSection(x,y,z) |> ignore
                                                y <- y - 16
                                        else
                                            // we're in terrain, but at a fully transparent block
                                            y <- y - 1
                                            if y%16 = 15 then
                                                let _,blocks,_,_,_ = r.GetSection(x,y,z)
                                                curSectionBlocks <- blocks
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
    // update LightPopulated
    printf "updating LightPopulated..."
    for rx in rxs do
        for rz in rzs do
            let r = map.MaybeGetRegion(rx*512,rz*512)
            if r <> null then
                for cx = 0 to 31 do
                    for cz = 0 to 31 do
                        let x,z = rx*512+cx*16, rz*512+cz*16
                        let bi = r.MaybeGetBlockInfo(x,0,z)
                        if bi <> null then // chunk is represented
                            r.SetChunkClean(x,z)
    printfn "...done!"

let relightTheWorld(map:MapFolder) = relightTheWorldHelper(map, [-99..99], [-99..99], false)
// TODO known oddities
//  - RandomCTM (seed 109) at -72 80 -809, top surface of flat peak mini-bedrock is dark (unrepresented air?)
//  - RandomCTM (seed 109) at 618 66 671, jack-o-lantern does not give off light
// In both cases I appear to be writing the correct data, so maybe MC eats it, e.g.
//  - maybe MC culls empty sections, throwing away block light?
//  - maybe MC is intolerant to jack-o-lanterns sitting atop non-opaque blocks (you can't place them there)?

let demoFixTheWorld() =
    //let originalRegionFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RCTM109OriginalLighting\region\"""
    //let fixedRegionFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RCTM109CorrectedLighting\region\"""
    let originalRegionFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RandomCTM\region\"""
    let fixedRegionFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RandomCTM109WithNearlyFixedLighting\region\"""
    let testMap = new MapFolder(originalRegionFolder)
    let sw = System.Diagnostics.Stopwatch.StartNew()
    relightTheWorldHelper(testMap,[-2..1],[-2..1],false)
    printfn "took %dms" sw.ElapsedMilliseconds 
    let goodMap = new MapFolder(fixedRegionFolder)
    compareLighting(goodMap, testMap, -2*512, -2*512, 1*512+511, 1*512+511)
