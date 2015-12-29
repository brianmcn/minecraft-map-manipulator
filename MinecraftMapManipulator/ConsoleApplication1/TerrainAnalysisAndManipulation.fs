module TerrainAnalysisAndManipulation

open Algorithms
open NBT_Manipulation
open RegionFiles
open CustomizationKnobs

let repopulateAsAnotherBiome() =
    //let user = "brianmcn"
    let user = "Admin1"
    let fil = """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\15w44b\region\r.0.0.mca"""
    let regionFile = new RegionFile(fil)
    //let newBiome = 32uy // mega taiga
    //let newBiome = 8uy // hell // didn't do anything interesting?
    //let newBiome = 13uy // ice plains // freezes ocean, adds snow layer
    //let newBiome = 129uy // sunflower plains, saw lakes added
    //let newBiome = 140uy // ice plains spikes (did not generate spikes) // freezes ocean, adds snow layer, re-freezes lakes that formed on/under ocean, ha
    //let newBiome = 38uy // mesa plateau f (did not change stone to clay)
    let newBiome = 6uy // swamp (did not see any witch huts, but presumably seed based?)
    for cx = 0 to 31 do
        for cz = 0 to 31 do
            match regionFile.TryGetChunk(cx,cz) with
            | None -> ()
            | Some theChunk ->
                let theChunkLevel = match theChunk with Compound(_,rsa) -> rsa.[0] // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
                // replace biomes
                match theChunkLevel.["Biomes"] with
                | NBT.ByteArray(_,a) -> for i = 0 to a.Length-1 do a.[i] <- newBiome
                // replace terrain-populated
                match theChunkLevel with
                | NBT.Compound(_,a) ->
                    for i = 0 to a.Count-1 do
                        if a.[i].Name = "TerrainPopulated" then
                            a.[i] <- NBT.Byte("TerrainPopulated", 0uy)
    regionFile.Write(fil+".new")
    System.IO.File.Delete(fil)
    System.IO.File.Move(fil+".new",fil)

////////////////////////////////////////////

let debugRegion() =
    //let user = "brianmcn"
    let user = "Admin1"
    let rx = 5
    let rz = 0
    let fil = """C:\Users\"""+user+(sprintf """\AppData\Roaming\.minecraft\saves\pregenED\region\r.%d.%d.mca""" rx rz)
    let regionFile = new RegionFile(fil)
    for cx = 0 to 31 do
        for cz = 0 to 31 do
            match regionFile.TryGetChunk(cx,cz) with
            | None -> ()
            | Some theChunk ->
                printf "%5d,%5d: " (cx*16+rx*512) (cz*16+rz*512)
                let theChunkLevel = match theChunk with Compound(_,rsa) -> rsa.[0] // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
                match theChunkLevel.["TerrainPopulated"] with
                | NBT.Byte(_,b) -> printf "TP=%d  " b
                match theChunkLevel.["LightPopulated"] with
                | NBT.Byte(_,b) -> printf "LP=%d  " b
                match theChunkLevel.["Entities"] with
                | NBT.List(_,Compounds(a)) -> printf "E=%d  " a.Length 
                match theChunkLevel.["TileEntities"] with
                | NBT.List(_,Compounds(a)) -> printf "TE=%d  " a.Length 
                match theChunkLevel.TryGetFromCompound("TileTicks") with
                | Some(NBT.List(_,Compounds(a))) -> printf "TT=%d  " a.Length 
                | None -> printf "TT=0  "
                printfn ""
                // replace terrain-populated
                match theChunkLevel with
                | NBT.Compound(_,a) ->
                    for i = 0 to a.Count-1 do
                        if a.[i].Name = "TerrainPopulated" then
                            a.[i] <- NBT.Byte("TerrainPopulated", 1uy)
    regionFile.Write(fil+".new")
                
////////////////////////////////////////////

let putThingRecomputeLight(sx,sy,sz,map:MapFolder,thing,dmg) =
    // for lighted blocks (e.g. thing="glowstone"), to have Minecraft recompute the light, use a command block and a tile tick
    map.SetBlockIDAndDamage(sx,sy,sz,137uy,0uy)  // command block
    map.AddOrReplaceTileEntities([| [| Int("x",sx); Int("y",sy); Int("z",sz); String("id","Control"); Byte("auto",0uy); String("Command",sprintf "setblock ~ ~ ~ %s %d" thing dmg); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",1uy); End |] |])
    map.AddTileTick("minecraft:command_block",1,0,sx,sy,sz)
let putGlowstoneRecomputeLight(sx,sy,sz,map:MapFolder) = putThingRecomputeLight(sx,sy,sz,map,"glowstone",0)

let putTreasureBoxAtCore(map:MapFolder,sx,sy,sz,lootTableName,itemsNbt) =
    for x = sx-2 to sx+2 do
        for z = sz-2 to sz+2 do
            map.SetBlockIDAndDamage(x,sy,z,22uy,0uy)  // lapis block
            map.SetBlockIDAndDamage(x,sy+3,z,22uy,0uy)  // lapis block
    putGlowstoneRecomputeLight(sx,sy,sz,map)
    for x = sx-2 to sx+2 do
        for y = sy+1 to sy+2 do
            for z = sz-2 to sz+2 do
                map.SetBlockIDAndDamage(x,y,z,20uy,0uy)  // glass
    map.SetBlockIDAndDamage(sx,sy+1,sz,54uy,2uy)  // chest
    map.AddOrReplaceTileEntities([| [| yield Int("x",sx)
                                       yield Int("y",sy+1)
                                       yield Int("z",sz)
                                       yield String("id","Chest")
                                       yield List("Items",Compounds itemsNbt)
                                       if lootTableName <> null then
                                            yield String("LootTable",lootTableName)
                                       yield String("Lock","")
                                       yield String("CustomName","Lootz!")
                                       yield End |] |])

let putTreasureBoxAt(map:MapFolder,sx,sy,sz,lootTableName) =
    putTreasureBoxAtCore(map,sx,sy,sz,lootTableName,[| |])

let putTreasureBoxWithItemsAt(map:MapFolder,sx,sy,sz,itemsNbt) =
    putTreasureBoxAtCore(map,sx,sy,sz,null,itemsNbt)

let putBeaconAt(map:MapFolder,ex,ey,ez,colorDamage,addAirSpace) =
    if addAirSpace then
        for x = ex-3 to ex+3 do
            for y = ey-5 to ey do
                for z = ez-3 to ez+3 do
                    map.SetBlockIDAndDamage(x,y,z,0uy,0uy)  // air
    for x = ex-2 to ex+2 do
        for y = ey-4 to ey-1 do
            for z = ez-2 to ez+2 do
                map.SetBlockIDAndDamage(x,y,z,7uy,0uy)  // bedrock
    for x = ex-1 to ex+1 do
        for z = ez-1 to ez+1 do
            map.SetBlockIDAndDamage(x,ey-3,z,133uy,0uy)  // emerald block
    map.SetBlockIDAndDamage(ex,ey-2,ez,138uy,0uy) // beacon
    map.SetBlockIDAndDamage(ex,ey-1,ez, 95uy,colorDamage) // stained glass
    map.SetBlockIDAndDamage(ex,ey+0,ez,120uy,0uy) // end portal frame

// use printf for console progress indiciation
// use info for non-summary info
// use summary for summary info
type EventAndProgressLog() =
    let log = ResizeArray()
    member this.LogInfo(s) = 
        log.Add( (1,s) )
        printfn "%s" s
    member this.LogSummary(s) = 
        log.Add( (2,s) )
        printfn "%s" s
    member this.SummaryEvents() = log |> Seq.choose (fun (i,s) -> if i=2 then Some s else None)
    member this.AllEvents() = log |> Seq.map snd

type SpawnerAccumulator(description) =
    let spawnerTileEntities = ResizeArray()
    let spawnerTypeCount = new System.Collections.Generic.Dictionary<_,_>()
    member this.Add(ms:MobSpawnerInfo) =
        let kind = ms.BasicMob + if ms.ExtraNbt.Length > 0 then "extra" else ""
        if spawnerTypeCount.ContainsKey(kind) then
            spawnerTypeCount.[kind] <- spawnerTypeCount.[kind] + 1
        else
            spawnerTypeCount.[kind] <- 1
        spawnerTileEntities.Add(ms.AsNbtTileEntity())
    member this.AddToMapAndLog(map:MapFolder, log:EventAndProgressLog) =
        map.AddOrReplaceTileEntities(spawnerTileEntities)
        let sb = new System.Text.StringBuilder()
        sb.Append(sprintf "   Total:%3d" (spawnerTypeCount |> Seq.sumBy (fun (KeyValue(_,v)) -> v))) |> ignore
        for KeyValue(k,v) in spawnerTypeCount |> Seq.sortBy (fun (KeyValue(k,_)) -> k)do
            sb.Append(sprintf "   %s:%3d" k v) |> ignore
        log.LogSummary(sprintf "   %s:%s" description (sb.ToString()))

let findCaveEntrancesNearSpawn(map:MapFolder, hm:_[,], hmIgnoringLeaves:_[,], log:EventAndProgressLog) =
    let MINIMUM = -DAYLIGHT_RADIUS
    let LENGTH = 2*DAYLIGHT_RADIUS
    let YMIN = 50
    let YLEN = 30
    let PT(x,y,z) = 
        let i,j,k = x-MINIMUM, y-YMIN, z-MINIMUM
        i*YLEN*LENGTH + k*YLEN + j
    let XYZP(pt) =
        let i = pt / (YLEN*LENGTH)
        let k = (pt % (YLEN*LENGTH)) / YLEN
        let j = pt % YLEN
        (i + MINIMUM, j + YMIN, k + MINIMUM)
    let a = System.Array.CreateInstance(typeof<Partition>, [|LENGTH+2; YLEN+2; LENGTH+2|], [|MINIMUM; YMIN; MINIMUM|]) :?> Partition[,,] // +2s because we have sentinels guarding array index out of bounds
    let mutable currentSectionBlocks,curx,cury,curz = null,-1000,-1000,-1000
    // find all the air spaces
    printf "FIND"
    for y = YMIN+1 to YMIN+YLEN do
        printf "."
        for x = MINIMUM+1 to MINIMUM+LENGTH-1 do
            for z = MINIMUM+1 to MINIMUM+LENGTH-1 do
                if not(DIV(x,16) = DIV(curx,16) && DIV(y,16) = DIV(cury,16) && DIV(z,16) = DIV(curz,16)) then
                    currentSectionBlocks <- map.GetOrCreateSection(x,y,z) |> (fun (_sect,blocks,_bd) -> blocks)
                    curx <- x
                    cury <- y
                    curz <- z
                let dx, dy, dz = (x+51200) % 16, y % 16, (z+51200) % 16
                let bix = dy*256 + dz*16 + dx
                if currentSectionBlocks.[bix] = 0uy then
                    a.[x,y,z] <- new Partition(new Thingy(PT(x,y,z),(y=YMIN+1),(y>=hmIgnoringLeaves.[x,z])))
    printfn ""
    printf "CONNECT"
    // connected-components them
    for y = YMIN+1 to YMIN+YLEN-1 do
        printf "."
        for x = MINIMUM+1 to MINIMUM+LENGTH-1 do
            for z = MINIMUM+1 to MINIMUM+LENGTH-1 do
                if a.[x,y,z]<>null && a.[x+1,y,z]<>null && (y < hmIgnoringLeaves.[x,z] || y < hmIgnoringLeaves.[x+1,z]) then
                    a.[x,y,z].Union(a.[x+1,y,z])
                if a.[x,y,z]<>null && a.[x,y+1,z]<>null && (y < hmIgnoringLeaves.[x,z]) then
                    a.[x,y,z].Union(a.[x,y+1,z])
                if a.[x,y,z]<>null && a.[x,y,z+1]<>null && (y < hmIgnoringLeaves.[x,z] || y < hmIgnoringLeaves.[x,z+1]) then
                    a.[x,y,z].Union(a.[x,y,z+1])
    printfn ""
    printf "ANALYZE"
    // look for 'good' ones
    let nearSpawnCaveEntranceCCs = new System.Collections.Generic.Dictionary<_,_>()
    for y = YMIN+1 to YMIN+YLEN do
        printf "."
        for x = MINIMUM+1 to MINIMUM+LENGTH do
            for z = MINIMUM+1 to MINIMUM+LENGTH do
                if a.[x,y,z]<>null then
                    let v = a.[x,y,z].Find().Value 
                    if v.IsLeft && v.IsRight then
                        if not(nearSpawnCaveEntranceCCs.ContainsKey(v.Point)) then
                            nearSpawnCaveEntranceCCs.Add(v.Point, new System.Collections.Generic.HashSet<_>())
                        nearSpawnCaveEntranceCCs.[v.Point].Add(PT(x,y,z)) |> ignore
    printfn ""
    // highlight cave entrances near spawn
    let mutable caveCount = 0
    for hs in nearSpawnCaveEntranceCCs.Values do
        if hs.Count > 200 then
            // only consider "caves" of some min size
            let mutable bestX,bestY,bestZ = 9999,0,9999
            for p in hs do
                let x,y,z = XYZP(p)
                if y >= hmIgnoringLeaves.[x,z] && (x*x+z*z < bestX*bestX+bestZ*bestZ) then
                    bestX <- x
                    bestY <- y
                    bestZ <- z
            if bestY <> 0 then
                // found highest point in this cave exposed to surface
                for y = bestY + 10 to bestY + 25 do
                    map.SetBlockIDAndDamage(bestX,y,bestZ,89uy,0uy)  // glowstone
                putGlowstoneRecomputeLight(bestX,bestY+26,bestZ,map)
                hm.[bestX,bestZ] <- bestY+27
                map.SetHeightMap(bestX, bestZ, bestY+27)
                caveCount <- caveCount + 1
                (*
                for p in hs do
                    let x,y,z = XYZP(p)
                    map.SetBlockIDAndDamage(x,y,z,20uy,0uy)  // glass (debug viz of CC)
                *)
    log.LogSummary(sprintf "highlighted %d cave entrances near spawn" caveCount)

let mutable finalEX = 0
let mutable finalEZ = 0

let findUndergroundAirSpaceConnectedComponents(rng : System.Random, map:MapFolder, hm:_[,], log:EventAndProgressLog, decorations:ResizeArray<_>) =
    let YMIN = 10
    let YLEN = 50
    let DIFFERENCES = [|1,0,0; 0,1,0; 0,0,1; -1,0,0; 0,-1,0; 0,0,-1|]
    let PT(x,y,z) = 
        let i,j,k = x-MINIMUM, y-YMIN, z-MINIMUM
        i*YLEN*LENGTH + k*YLEN + j
    let XYZP(pt) =
        let i = pt / (YLEN*LENGTH)
        let k = (pt % (YLEN*LENGTH)) / YLEN
        let j = pt % YLEN
        (i + MINIMUM, j + YMIN, k + MINIMUM)
    let a = System.Array.CreateInstance(typeof<Partition>, [|LENGTH+2; YLEN+2; LENGTH+2|], [|MINIMUM; YMIN; MINIMUM|]) :?> Partition[,,] // +2s because we have sentinels guarding array index out of bounds
    let mutable currentSectionBlocks,curx,cury,curz = null,-1000,-1000,-1000
    // find all the air spaces in the underground
    printf "FIND"
    for y = YMIN+1 to YMIN+YLEN do
        printf "."
        for x = MINIMUM+1 to MINIMUM+LENGTH-1 do
            for z = MINIMUM+1 to MINIMUM+LENGTH-1 do
                if not(DIV(x,16) = DIV(curx,16) && DIV(y,16) = DIV(cury,16) && DIV(z,16) = DIV(curz,16)) then
                    currentSectionBlocks <- map.GetOrCreateSection(x,y,z) |> (fun (_sect,blocks,_bd) -> blocks)
                    curx <- x
                    cury <- y
                    curz <- z
                let dx, dy, dz = (x+51200) % 16, y % 16, (z+51200) % 16
                let bix = dy*256 + dz*16 + dx
                if currentSectionBlocks.[bix] = 0uy then // air
                    a.[x,y,z] <- new Partition(new Thingy(PT(x,y,z),(y=YMIN+1),(y>=hm.[x,z])))
    printfn ""
    printf "CONNECT"
    // connected-components them
    for y = YMIN+1 to YMIN+YLEN-1 do
        printf "."
        for x = MINIMUM+1 to MINIMUM+LENGTH-1 do
            for z = MINIMUM+1 to MINIMUM+LENGTH-1 do
                if a.[x,y,z]<>null && a.[x+1,y,z]<>null then
                    a.[x,y,z].Union(a.[x+1,y,z])
                if a.[x,y,z]<>null && a.[x,y+1,z]<>null then
                    a.[x,y,z].Union(a.[x,y+1,z])
                if a.[x,y,z]<>null && a.[x,y,z+1]<>null then
                    a.[x,y,z].Union(a.[x,y,z+1])
    printfn ""
    printf "ANALYZE"
    // look for 'good' ones
    let goodCCs = new System.Collections.Generic.Dictionary<_,_>()
    for y = YMIN+1 to YMIN+YLEN do
        printf "."
        for x = MINIMUM+1 to MINIMUM+LENGTH do
            for z = MINIMUM+1 to MINIMUM+LENGTH do
                if a.[x,y,z]<>null then
                    let v = a.[x,y,z].Find().Value 
                    if v.IsLeft && v.IsRight then
                        if not(goodCCs.ContainsKey(v.Point)) then
                            goodCCs.Add(v.Point, new System.Collections.Generic.HashSet<_>())
                        goodCCs.[v.Point].Add(PT(x,y,z)) |> ignore
    printfn ""
    log.LogInfo(sprintf "There are %d CCs with the desired property" goodCCs.Count)
    let sk = System.Array.CreateInstance(typeof<sbyte>, [|LENGTH+2; YLEN+2; LENGTH+2|], [|MINIMUM; YMIN; MINIMUM|]) :?> sbyte[,,] // +2: don't need sentinels here, but easier to keep indexes in lock-step with other array
    for p in goodCCs.Values |> Seq.head do
        let x,y,z = XYZP(p)
        if y > YMIN && y < YMIN+YLEN && x > MINIMUM && x < MINIMUM+LENGTH && z > MINIMUM && z < MINIMUM+LENGTH then
            if x > -1000 && x < -750 && z > -800 && z < -500 then // TODO artificial to reduce space
                sk.[x,y,z] <- 1y
    Algorithms.skeletonize(sk, (fun (x,y,z,iter) -> map.SetBlockIDAndDamage(x,y,z,95uy,byte iter))) // 95 = stained_glass
    let mutable numEndpoints = 0
    for y = YMIN+1 to YMIN+YLEN do
        for x = MINIMUM+1 to MINIMUM+LENGTH do
            for z = MINIMUM+1 to MINIMUM+LENGTH do
                if sk.[x,y,z]>0y then
                    map.SetBlockIDAndDamage(x,y,z,102uy,0uy) // 102 = glass_pane
                if sk.[x,y,z]=3y then
                    printfn "EP %d %d %d" x y z
                    numEndpoints <- numEndpoints + 1
    printfn "there were %d endpoints" numEndpoints 
    if false then // TODO remove
    // These arrays are large enough that I think they get pinned in permanent memory, reuse them
    let dist = System.Array.CreateInstance(typeof<int>, [|LENGTH+2; YLEN+2; LENGTH+2|], [|MINIMUM; YMIN; MINIMUM|]) :?> int[,,] // +2: don't need sentinels here, but easier to keep indexes in lock-step with other array
    let prev = System.Array.CreateInstance(typeof<int>, [|LENGTH+2; YLEN+2; LENGTH+2|], [|MINIMUM; YMIN; MINIMUM|]) :?> int[,,] // +2: don't need sentinels here, but easier to keep indexes in lock-step with other array
    let mutable hasDoneFinal, thisIsFinal = false, false
    for hs in goodCCs.Values do
        let mutable bestX,bestY,bestZ = 0,0,0
        for p in hs do
            let x,y,z = XYZP(p)
            if y > bestY then
                bestX <- x
                bestY <- y
                bestZ <- z
        // have a point at the top of the CC, now find furthest low point away (Dijkstra variant)
        // re-init the array:
        for ii = MINIMUM to MINIMUM+LENGTH+1 do
            for jj = YMIN to YMIN+YLEN+1 do
                for kk = MINIMUM to MINIMUM+LENGTH+1 do
                    dist.[ii,jj,kk] <- 999999
        let q = new System.Collections.Generic.Queue<_>()
        let bi,bj,bk = (bestX,bestY,bestZ)
        q.Enqueue(bi,bj,bk)
        dist.[bi,bj,bk] <- 0
        let mutable besti,bestj,bestk = bi, bj, bk
        while q.Count > 0 do
            let i,j,k = q.Dequeue()
            let d = dist.[i,j,k]
            for diffi = 0 to DIFFERENCES.Length-1 do
                let di,dj,dk = DIFFERENCES.[diffi]
                if a.[i+di,j+dj,k+dk]<>null && dist.[i+di,j+dj,k+dk] > d+1 then
                    dist.[i+di,j+dj,k+dk] <- d+1  // TODO bias to walls
                    q.Enqueue(i+di,j+dj,k+dk)
                    if j = YMIN+1 then  // low point
                        if dist.[besti,bestj,bestk] < d+1 then
                            besti <- i+di
                            bestj <- j+dj
                            bestk <- k+dk
        if bj <> bestj then  // we actually reached a low point; if not, nothing else to do
            // now find shortest from that bottom to top
            // re-init the arrays:
            for ii = MINIMUM to MINIMUM+LENGTH+1 do
                for jj = YMIN to YMIN+YLEN+1 do
                    for kk = MINIMUM to MINIMUM+LENGTH+1 do
                        dist.[ii,jj,kk] <- 999999
                        prev.[ii,jj,kk] <- -1
            let bi,bj,bk = besti,bestj,bestk
            q.Enqueue(bi,bj,bk)
            dist.[bi,bj,bk] <- 0
            let mutable besti,bestj,bestk = bi, bj, bk
            while q.Count > 0 do
                let i,j,k = q.Dequeue()
                let d = dist.[i,j,k]
                let x,y,z = (i,j,k)
                if (y>=hm.[x,z]) then // surface
                    // found shortest
                    besti <- i
                    bestj <- j
                    bestk <- k
                    while q.Count > 0 do
                        q.Dequeue() |> ignore
                else
                    for diffi = 0 to DIFFERENCES.Length-1 do
                        let di,dj,dk = DIFFERENCES.[diffi]
                        if a.[i+di,j+dj,k+dk]<>null && dist.[i+di,j+dj,k+dk] > d+1 then
                            dist.[i+di,j+dj,k+dk] <- d+1  // TODO bias to walls
                            prev.[i+di,j+dj,k+dk] <- diffi
                            q.Enqueue(i+di,j+dj,k+dk)
            // found a path
            let sx,sy,sz = (bi,bj,bk)
            let ex,ey,ez = (besti,bestj,bestk)
            // ensure beacon in decent bounds
            let DB = 60
            if ex < MINIMUM+DB || ez < MINIMUM+DB || ex > MINIMUM+LENGTH-DB || ez > MINIMUM+LENGTH-DB || 
                (ex > -SPAWN_PROTECTION_DISTANCE_GREEN && ex < SPAWN_PROTECTION_DISTANCE_GREEN && ez > -SPAWN_PROTECTION_DISTANCE_GREEN && ez < SPAWN_PROTECTION_DISTANCE_GREEN)  then
                () // skip if too close to 0,0 or to map bounds
            else
            log.LogInfo(sprintf "(%d,%d,%d) is %d blocks from (%d,%d,%d)" sx sy sz dist.[besti,bestj,bestk] ex ey ez)
            if dist.[besti,bestj,bestk] > 100 && dist.[besti,bestj,bestk] < 500 then  // only keep mid-sized ones...
                if not hasDoneFinal && dist.[besti,bestj,bestk] > 300 && ex*ex+ez*ez > SPAWN_PROTECTION_DISTANCE_PURPLE*SPAWN_PROTECTION_DISTANCE_PURPLE then
                    thisIsFinal <- true
                log.LogSummary(sprintf "added %sbeacon at %d %d %d which travels %d" (if thisIsFinal then "FINAL " else "") ex ey ez dist.[besti,bestj,bestk])
                decorations.Add((if thisIsFinal then 'X' else 'B'),ex,ez)
                let mutable i,j,k = besti,bestj,bestk
                let fullDist = dist.[besti,bestj,bestk]
                let mutable count = 0
                let spawners = SpawnerAccumulator("spawners along path")
                let possibleSpawners = 
                    if thisIsFinal then
                        PURPLE_BEACON_CAVE_DUNGEON_SPAWNER_DATA
                    else
                        GREEN_BEACON_CAVE_DUNGEON_SPAWNER_DATA
                while i<>bi || j<>bj || k<>bk do
                    let ni, nj, nk = // next points (step back using info from 'prev')
                        let dx,dy,dz = DIFFERENCES.[prev.[i,j,k]]
                        i-dx,j-dy,k-dz
                    let ii,jj,kk = prev.[i,j,k]%3<>0, prev.[i,j,k]%3<>1, prev.[i,j,k]%3<>2   // ii/jj/kk track 'normal' to the path
                    // maybe put mob spawner nearby
                    let pct = float count / (float fullDist * 3.0)
                    if rng.NextDouble() < pct*possibleSpawners.DensityMultiplier then
                        let xx,yy,zz = (i,j,k)
                        let mutable spread = 1   // check in outwards 'rings' around the path until we find a block we can replace
                        let mutable ok = false
                        while not ok do
                            let candidates = ResizeArray()
                            let xs = if ii then [xx-spread .. xx+spread] else [xx]
                            let ys = if jj then [yy-spread .. yy+spread] else [yy]
                            let zs = if kk then [zz-spread .. zz+spread] else [zz]
                            for x in xs do
                                for y in ys do
                                    for z in zs do
                                        if map.GetBlockInfo(x,y,z).BlockID <> 0uy then // if not air
                                            candidates.Add(x,y,z)
                            if candidates.Count > 0 then
                                let x,y,z = candidates.[rng.Next(candidates.Count-1)]
                                map.SetBlockIDAndDamage(x, y, z, 52uy, 0uy) // 52 = monster spawner
                                let ms = possibleSpawners.NextSpawnerAt(x,y,z,rng)
                                spawners.Add(ms)
                                ok <- true
                            spread <- spread + 1
                            if spread = 5 then  // give up if we looked a few blocks away and didn't find a suitable block to swap
                                ok <- true
                    // put stripe on the ground
                    let mutable pi,pj,pk = i,j,k
                    while a.[pi,pj,pk]<>null do
                        pj <- pj - 1
                    map.SetBlockIDAndDamage(pi,pj,pk,73uy,0uy)  // 73 = redstone ore (lights up when things walk on it)
                    i <- ni
                    j <- nj
                    k <- nk
                    count <- count + 1
                // write out all the spawner data we just placed
                spawners.AddToMapAndLog(map,log)
                putBeaconAt(map,ex,ey,ez,(if thisIsFinal then 10uy else 5uy), true) // 10=purple, 5=lime
                map.SetBlockIDAndDamage(ex,ey+1,ez,130uy,2uy) // ender chest
                // put treasure at bottom end
                putTreasureBoxAt(map,sx,sy,sz,sprintf "%s:chests/tier3" LootTables.LOOT_NS_PREFIX)
                if thisIsFinal then
                    thisIsFinal <- false
                    hasDoneFinal <- true
                    finalEX <- ex
                    finalEZ <- ez
                    // replace final treasure
                    let bx,by,bz = sx,sy+1,sz // chest location, will overwrite it inside treasure box
                    let chestItems = 
                        Compounds[| 
                                yield [| Byte("Count",1uy); Byte("Slot",12uy); Short("Damage",0s); String("id","minecraft:sponge"); Compound("tag", [|
                                            Compound("display",[|String("Name","Monument Block: Sponge");End|] |> ResizeArray); End |] |> ResizeArray); End |]
                                yield [| Byte("Count",1uy); Byte("Slot",14uy); Short("Damage",0s); String("id","minecraft:written_book"); 
                                         Compound("tag", Utilities.makeWrittenBookTags("Lorgon111","Congratulations!", 
                                                                                     [| 
                                                                                        """{"text":"Once all monument blocks are placed on the monument, you win! ..."}"""
                                                                                        """{"text":"I hope enjoyed playing the map.  I am happy to hear your feedback, you can contact me at TODO..."}"""
                                                                                        """{"text":"If you enjoyed and would like to leave me a donation, I'd very much appreciate that! TODO donation link"}"""
                                                                                     |]) |> ResizeArray
                                                  )
                                         End |]
                            |]
                    map.SetBlockIDAndDamage(bx,by,bz,54uy,2uy)  // chest
                    map.AddOrReplaceTileEntities([| [| Int("x",bx); Int("y",by); Int("z",bz); String("id","Chest"); List("Items",chestItems); String("Lock",""); String("CustomName","Winner!"); End |] |])
    // end foreach CC
    if finalEX = 0 && finalEZ = 0 then
        log.LogSummary("FAILED TO PLACE FINAL")
        failwith "final failed"
    (*
    This section is not done.  There is no loot, and also the glowstone at entrance can block the beacon from other structures.

    // find completely flat floors underground
    printfn ""
    printf "FLAT FLOOR ANALYSIS"
    let MAXR, THRESHHOLDR, YWEIGHT = 18, 12, 3
    let YDIFF = (float MAXR) / sqrt(float YWEIGHT) |> int
    let D = 6 // length of side of square we're looking for
    let ff = System.Array.CreateInstance(typeof<System.Collections.Generic.HashSet<int> >, [|LENGTH+2; YLEN+2|], [|MINIMUM; YMIN|]) :?> System.Collections.Generic.HashSet<int>[,]  // +2: don't need sentinels here, but easier to keep indexes in lock-step with other array
    for y = YMIN to YMIN+YLEN-1-YDIFF do
        printf "."
        for x = MINIMUM to MINIMUM+LENGTH-1 do
            ff.[x,y] <- new System.Collections.Generic.HashSet<int>()
            let mutable r = 0 // count of how many air-above-non-air we've found in a row along z
            for z = MINIMUM to MINIMUM+LENGTH-1 do
                if a.[x,y,z]=null && a.[x,y+1,z]<>null && a.[x,y+2,z]<>null then  // TODO lava/water pools
                    r <- r + 1
                else
                    r <- 0
                if r >= D then
                    ff.[x,y].Add(z) |> ignore  // ff.[x,y] contains z iff (x,y,z-6) to (x,y,z) are all non-air-with-air-above blocks
    printfn ""
    let flatCorners = ResizeArray()
    for y = YMIN+1 to YMIN+YLEN-1-YDIFF do
        printf "."
        for x = MINIMUM+D-1 to MINIMUM+LENGTH-1 do
            for z in ff.[x,y] do
                let mutable ok = true
                for d = 1 to D-1 do
                    if not(ff.[x-d,y].Contains(z)) then
                        ok <- false
                if ok then
                    flatCorners.Add( (x,y,z) )
    printfn ""
    // find good 'cul-de-sac rooms'
    let UNVISITED = (-9999,-9999,-9999)
    let VISITED_NEAR = (7777,7777,7777)
    let culDeSacRooms = ResizeArray()
    for x,y,z in flatCorners do
        let x,y,z = x-D/2, y+1, z-D/2  // center air space
        let boundaryPoints = ResizeArray()
        let visited = System.Array.CreateInstance(typeof<int*int*int>, [|MAXR*2+3; MAXR*2+3; MAXR*2+3|], [|x-MAXR-1; y-MAXR-1; z-MAXR-1|]) :?> (int*int*int)[,,]
        for i = x-MAXR-1 to x+MAXR+1 do
            for j = y-MAXR-1 to y+MAXR+1 do
                for k = z-MAXR-1 to z+MAXR+1 do
                    visited.[i,j,k] <- UNVISITED
        visited.[x,y,z] <- VISITED_NEAR
        // 3 states, unvisited, visited within THRESHHOLDR, or visited beyond THRESHHOLDR with a record of first point past threshold
        // explore contiguous air until reach MAXR away from center
        let q = new System.Collections.Generic.Queue<_>()
        q.Enqueue(x,y,z)
        let distSq(i,j,k) = (x-i)*(x-i) + YWEIGHT*(y-j)*(y-j) + (z-k)*(z-k)  // make diff y seem farther away
        while not(q.Count=0) do
            let i,j,k = q.Dequeue()
            let prevVisit = visited.[i,j,k]
            for di,dj,dk in DIFFERENCES do
                let i,j,k = i+di,j+dj,k+dk
                if visited.[i,j,k] = UNVISITED && a.[i,j,k]<>null then // if new and air
                    if prevVisit = VISITED_NEAR then
                        if distSq(i,j,k) < THRESHHOLDR*THRESHHOLDR then
                            visited.[i,j,k] <- VISITED_NEAR
                            //map.SetBlockIDAndDamage(i,j,k,95uy,0uy) // TODO debug
                        else
                            visited.[i,j,k] <- i,j,k // first point past threshhold
                            //map.SetBlockIDAndDamage(i,j,k,95uy,2uy) // TODO debug
                    else
                        visited.[i,j,k] <- prevVisit
                        //map.SetBlockIDAndDamage(i,j,k,95uy,8uy) // TODO debug
                    if distSq(i,j,k) < MAXR*MAXR then
                        q.Enqueue(i,j,k)
                    else
                        boundaryPoints.Add( (i,j,k) )
                        //map.SetBlockIDAndDamage(i,j,k,95uy,15uy) // TODO debug
        if boundaryPoints.Count > 0 then
            let i,j,k = boundaryPoints.[0]
            let v = visited.[i,j,k]
            let mutable n, ok = 1, true
            while ok && n < boundaryPoints.Count do
                let i,j,k = boundaryPoints.[n]
                if visited.[i,j,k] <> v then
                    ok <- false
                    //printfn "rejected, as %A escapes via %A but %A escapes via %A" boundaryPoints.[n] visited.[i,j,k] boundaryPoints.[0] v
                n <- n + 1
            if ok then
                let mutable dup = false
                for cx,cy,cz in culDeSacRooms do
                    if distSq(cx,cy,cz) < MAXR*MAXR then
                        dup <- true  // we find a lot of overlapping flat areas, ignore duplicates
                if not dup then
                    log.LogInfo(sprintf "ACCEPTED, all exits of %A go through %A" (x,y,z) v)
                    culDeSacRooms.Add( (x,y,z) )
        ()
    log.LogInfo(sprintf "There are %d cul-de-sac rooms" culDeSacRooms.Count)
    let mutable reachableCount = 0
    for x,y,z in culDeSacRooms do
            let q = new System.Collections.Generic.Queue<_>()
            // now find shortest from that point to top
            // re-init the arrays:
            for ii = MINIMUM to MINIMUM+LENGTH+1 do
                for jj = YMIN to YMIN+YLEN+1 do
                    for kk = MINIMUM to MINIMUM+LENGTH+1 do
                        dist.[ii,jj,kk] <- 999999
                        prev.[ii,jj,kk] <- -1
            let bi,bj,bk = x,y,z
            q.Enqueue(bi,bj,bk)
            dist.[bi,bj,bk] <- 0
            let mutable besti,bestj,bestk = -99999,-99999,-99999
            while q.Count > 0 do
                let i,j,k = q.Dequeue()
                let d = dist.[i,j,k]
                let x,y,z = (i,j,k)
                if (y>=hm.[x,z]) then // surface
                    // found shortest
                    besti <- i
                    bestj <- j
                    bestk <- k
                    while q.Count > 0 do
                        q.Dequeue() |> ignore
                else
                    for diffi = 0 to DIFFERENCES.Length-1 do
                        let di,dj,dk = DIFFERENCES.[diffi]
                        if a.[i+di,j+dj,k+dk]<>null && dist.[i+di,j+dj,k+dk] > d+1 then
                            dist.[i+di,j+dj,k+dk] <- d+1  // TODO bias to walls
                            prev.[i+di,j+dj,k+dk] <- diffi
                            q.Enqueue(i+di,j+dj,k+dk)
            if besti <> -99999 then
                // found an exit
                log.LogInfo(sprintf "Can get to cul-de-sec %A from surface %A" (x,y,z) (besti,bestj,bestk))
                reachableCount <- reachableCount + 1
                // TODO put some loot or something, have a purpose
                let mutable i,j,k = besti,bestj,bestk
                let mutable count = 0
                putGlowstoneRecomputeLight(i,80,k,map)  // TODO arbitrary height constant
                while i<>x || j<>y || k<>z do
                    let ni, nj, nk = // next points (step back using info from 'prev')
                        let dx,dy,dz = DIFFERENCES.[prev.[i,j,k]]
                        i-dx,j-dy,k-dz
                    i <- ni
                    j <- nj
                    k <- nk
                    count <- count + 1
                    if count%10 = 0 then
                        // find nearest wall/floor, place red torch
                        let numAirBeforeNonAir = Array.zeroCreate 6
                        for n = 0 to 5 do
                            let mutable ii, jj, kk = i, j, k
                            let di,dj,dk = DIFFERENCES.[n]
                            if dj = 1 then
                                numAirBeforeNonAir.[n] <- 99999 // don't look up
                            else
                                while a.[ii,jj,kk] <> null do
                                    ii <- ii + di
                                    jj <- jj + dj
                                    kk <- kk + dk
                                    numAirBeforeNonAir.[n] <- numAirBeforeNonAir.[n] + 1
                        let nearestDist = Array.min numAirBeforeNonAir
                        let dirIdx = numAirBeforeNonAir |> Array.findIndex(fun x -> x = nearestDist)
                        let di,dj,dk = DIFFERENCES.[dirIdx]
                        map.SetBlockIDAndDamage(i+di*nearestDist, j+dj*nearestDist, k+dk*nearestDist, 3uy, 0uy)  // dirt, in case was liquid or something
                        let nearestDist = nearestDist - 1
                        map.SetBlockIDAndDamage(i+di*nearestDist, j+dj*nearestDist, k+dk*nearestDist, 76uy,   // lit red torch
                                                if di = 1 then 2uy elif dk = 1 then 4uy elif di = -1 then 1uy elif dk = -1 then 3uy elif dj = -1 then 5uy else failwith "unexpected") // attached to dirt block in right orientation
    printfn ""
    log.LogSummary(sprintf "found %d reachable cul-de-sac rooms" reachableCount)
    *)
////
(* 

Can get to cul-de-sec (436, 18, 362) from surface (402, 36, 227)
Can get to cul-de-sec (-691, 21, -636) from surface (-695, 55, -744)
Can get to cul-de-sec (-728, 30, -24) from surface (-627, 55, 1)

MAP DEFAULTS 
ore    size tries
-----------------
dirt     33 10              3
gravel   33  8              13
granite  33 10        stone 1  1
diorite  33 10                 3
andesite 33 10                 5
coal     17 20              16
iron      9 20              15
gold      9  2              14
redstone  8  8              73 and 74
diamond   8  1              56
lapis     7  1              21
(emerald  1  3?  only extreme hills)   129
*)

let blockSubstitutionsEmpty =  // TODO want different ones, both as a function of x/z (difficulty in regions of map), biome?, and y (no spawners in wall above 63), anything else?
    [|
          3uy,0uy,    3uy,0uy;     // dirt -> 
         13uy,0uy,   13uy,0uy;     // gravel -> 
          1uy,1uy,    1uy,1uy;     // granite -> 
          1uy,3uy,    1uy,3uy;     // diorite -> 
          1uy,5uy,    1uy,5uy;     // andesite -> 
         16uy,0uy,   16uy,0uy;     // coal -> 
         15uy,0uy,   15uy,0uy;     // iron -> 
         14uy,0uy,   14uy,0uy;     // gold -> 
         73uy,0uy,   73uy,0uy;     // redstone -> 
         74uy,0uy,   74uy,0uy;     // lit_redstone -> 
         56uy,0uy,   56uy,0uy;     // diamond -> 
         21uy,0uy,   21uy,0uy;     // lapis -> 
        129uy,0uy,  129uy,0uy;     // emerald -> 
    |]

let oreSpawnCustom =
    [|
        // block, Size, Count, MinHeight, MaxHeight
        "dirt",     33, 90, 0, 256
        "gravel",   33,  8, 0, 256
        "granite",   3, 12, 0,  80
        "diorite",  12,120, 0,  80
        "andesite", 33,  0, 0,  80
        "coal",     17, 20, 0, 128
        "iron",      9,  6, 0,  64
        "gold",      9,  4, 0,  48
        "redstone",  3,  4, 0,  32
        "diamond",   4,  1, 0,  16
    |]

// only place if visible
let canPlaceSpawner(map:MapFolder,x,y,z) =
    // avoid placing multiple in a cluster
    if map.GetBlockInfo(x-1,y,z).BlockID = 52uy then
        false
    elif map.GetBlockInfo(x-1,y-1,z).BlockID = 52uy then
        false
    elif map.GetBlockInfo(x,y-1,z).BlockID = 52uy then
        false
    elif map.GetBlockInfo(x,y-1,z-1).BlockID = 52uy then
        false
    elif map.GetBlockInfo(x,y,z-1).BlockID = 52uy then
        false
    elif map.GetBlockInfo(x-1,y,z-1).BlockID = 52uy then
        false
    // only place if air nearby (can see spawner, or see particles up through blocks)
    elif map.GetBlockInfo(x+1,y,z).BlockID = 0uy then
        true
    elif map.GetBlockInfo(x-1,y,z).BlockID = 0uy then
        true
    elif map.GetBlockInfo(x,y,z+1).BlockID = 0uy then
        true
    elif map.GetBlockInfo(x,y,z-1).BlockID = 0uy then
        true
    elif map.GetBlockInfo(x,y+1,z).BlockID = 0uy then
        true
    elif map.GetBlockInfo(x,y-1,z).BlockID = 0uy then
        true
    elif map.GetBlockInfo(x+1,y+1,z).BlockID = 0uy then
        true
    elif map.GetBlockInfo(x-1,y+1,z).BlockID = 0uy then
        true
    elif map.GetBlockInfo(x,y+1,z+1).BlockID = 0uy then
        true
    elif map.GetBlockInfo(x,y+1,z-1).BlockID = 0uy then
        true
    elif map.GetBlockInfo(x,y+2,z).BlockID = 0uy then
        true
    else
        false

let substituteBlocks(rng : System.Random, map:MapFolder, log:EventAndProgressLog) =
    let LOX, LOY, LOZ = MINIMUM, 1, MINIMUM
    let HIY = 120
    let spawners1 = SpawnerAccumulator("rand spawners from granite")
    let spawners2 = SpawnerAccumulator("rand spawners from redstone")
    printf "SUBST"
    for y = LOY to HIY do
        printf "."
        for x = LOX to LOX+LENGTH-1 do
            for z = LOZ to LOZ+LENGTH-1 do
                let bi = map.MaybeGetBlockInfo(x,y,z)
                if bi <> null then
                    let bid = bi.BlockID 
                    let dmg = bi.BlockData 
                    if bid = 1uy && dmg = 3uy then // diorite ->
                        map.SetBlockIDAndDamage(x,y,z,97uy,0uy) // silverfish
                    elif bid = 1uy && dmg = 0uy then // stone ->
                        map.SetBlockIDAndDamage(x,y,z,1uy,5uy) // andesite
                    elif bid = 1uy && dmg = 1uy then // granite ->
                        if canPlaceSpawner(map,x,y,z) then
                            map.SetBlockIDAndDamage(x,y,z,52uy,0uy) // mob spawner
                            let ms = GRANITE_SPAWNER_DATA.NextSpawnerAt(x,y,z,rng)
                            spawners1.Add(ms)
                        else
                            map.SetBlockIDAndDamage(x,y,z,1uy,5uy) // andesite
                    elif bid = 73uy && dmg = 0uy then // redstone ore ->
                        if canPlaceSpawner(map,x,y,z) then
                            map.SetBlockIDAndDamage(x,y,z,52uy,0uy) // mob spawner
                            let ms = REDTSONE_SPAWNER_DATA.NextSpawnerAt(x,y,z,rng)
                            spawners2.Add(ms)
                        else
                            map.SetBlockIDAndDamage(x,y,z,1uy,5uy) // andesite
                    elif bid = 16uy && dmg = 0uy then // coal ore ->
                        if rng.Next(15) = 0 then
                            map.SetBlockIDAndDamage(x,y,z,173uy,0uy) // coal block
    log.LogSummary("added random spawners underground")
    spawners1.AddToMapAndLog(map,log)
    spawners2.AddToMapAndLog(map,log)
    printfn ""

let replaceSomeBiomes(rng : System.Random, map:MapFolder, log:EventAndProgressLog, biome:_[,]) =
    let a = Array2D.zeroCreateBased MINIMUM MINIMUM LENGTH LENGTH
    // find plains biomes
    for x = MINIMUM to MINIMUM+LENGTH-1 do
        for z = MINIMUM to MINIMUM+LENGTH-1 do
            let b = biome.[x,z]
            if b = 1uy then // 1 = Plains
                a.[x,z] <- new Partition(new Thingy(0,(x*x+z*z<DAYLIGHT_RADIUS*DAYLIGHT_RADIUS),false))
    // connected-components them
    for x = MINIMUM to MINIMUM+LENGTH-2 do
        for z = MINIMUM to MINIMUM+LENGTH-2 do
            if a.[x,z] <> null && a.[x,z+1] <> null then
                a.[x,z].Union(a.[x,z+1])
            if a.[x,z] <> null && a.[x+1,z] <> null then
                a.[x,z].Union(a.[x+1,z])
    let CCs = new System.Collections.Generic.Dictionary<_,_>()
    for x = MINIMUM to MINIMUM+LENGTH-1 do
        for z = MINIMUM to MINIMUM+LENGTH-1 do
            if a.[x,z] <> null then
                let rep = a.[x,z].Find()
                if not rep.Value.IsLeft then  // only find plains completely outside DAYLIGHT_RADIUS
                    if not(CCs.ContainsKey(rep)) then
                        CCs.Add(rep, new System.Collections.Generic.HashSet<_>())
                    CCs.[rep].Add( (x,z) ) |> ignore
    let tooSmall = ResizeArray()
    for KeyValue(k,v) in CCs do
        if v.Count < 1000 then
            tooSmall.Add(k)
    for k in tooSmall do
        CCs.Remove(k) |> ignore
    log.LogInfo(sprintf "found %d decent-sized plains biomes outside DAYLIGHT_RADIUS" CCs.Count)
    let mutable hellCount, skyCount = 0,0
    for KeyValue(_k,v) in CCs do
        if rng.NextDouble() < BIOME_HELL_PERCENTAGE then
            for x,z in v do
                map.SetBiome(x,z,8uy) // 8 = Hell
                biome.[x,z] <- 8uy
            hellCount <- hellCount + 1
        elif rng.NextDouble() < BIOME_SKY_PERCENTAGE then
            for x,z in v do
                map.SetBiome(x,z,9uy) // 9 = Sky
                biome.[x,z] <- 9uy
            skyCount <- skyCount + 1
    log.LogSummary(sprintf "Added %d Hell biomes and %d Sky biomes (replacing Plains)" hellCount skyCount)

// mappings: should probably be to a chance set that's a function of difficulty or something...
// given that I can customize them, but want same custom settings for whole world generation, just consider as N buckets, but can e.g. customize the granite etc for more 'choice'...
// custom: dungeons at 100, probably lava/water lakes less frequent, biome size 3?

// customized preset code

// types of things
// stone -> silverfish probably
// -> spawners (multiple kinds, with some harder than others in different areas)
// -> primed tnt (and normal tnt? cue?)
// -> hidden lava pockets? (e.g. if something was like 1-40 for size-tries, can perforate area with tiny bits of X)
// -> glowstone or sea lanterns (block lights)
// -> some ore, but less and guarded
// moss stone -> netherrack in hell biome, for example
// -> coal/iron/gold/diamond _blocks_ rather than ore in some spots (coal burns!)

// set pieces (my own dungeons, persistent entities)

// in addition to block substitution, need .dat info for e.g. 'witch areas' or guardian zones'

// also need to code up basic mob spawner methods (passengers, effects, attributes, range, frequency, ...)

let findBestPeaksAlgorithm(heightMap:_[,], connectedThreshold, goodThreshold, bestNearbyDist, hmDiffPerCC, decorations:ResizeArray<_>) =
    let a = Array2D.zeroCreateBased MINIMUM MINIMUM LENGTH LENGTH
    printfn "PART..."
    // find all points height over threshold
    for x = MINIMUM to MINIMUM+LENGTH-1 do
        for z = MINIMUM to MINIMUM+LENGTH-1 do
            let h = heightMap.[x,z]
            if h > connectedThreshold then
                a.[x,z] <- new Partition(new Thingy(0 (*x*1024+z*),false,(h>goodThreshold)))
    printfn "CC..."
    // connected-components them
    for x = MINIMUM to MINIMUM+LENGTH-2 do
        for z = MINIMUM to MINIMUM+LENGTH-2 do
            if a.[x,z] <> null && a.[x,z+1] <> null then
                a.[x,z].Union(a.[x,z+1])
            if a.[x,z] <> null && a.[x+1,z] <> null then
                a.[x,z].Union(a.[x+1,z])
    let CCs = new System.Collections.Generic.Dictionary<_,_>()
    for x = MINIMUM to MINIMUM+LENGTH-1 do
        for z = MINIMUM to MINIMUM+LENGTH-1 do
            if a.[x,z] <> null then
                let rep = a.[x,z].Find()
                if rep.Value.IsRight then
                    if not(CCs.ContainsKey(rep)) then
                        CCs.Add(rep, new System.Collections.Generic.HashSet<_>())
                    CCs.[rep].Add( (x,z) ) |> ignore
    printfn "ANALYZE..."
    let highPoints = ResizeArray()
    // pick highest in each CC    // TODO consider all local maxima? or make cutoff for CC be topY-constant, to disconnect high ranges?
    for hs in CCs.Values do
        let hix,hiz = hs |> Seq.maxBy (fun (x,z) -> heightMap.[x,z])
        let hihm = heightMap.[hix,hiz]
        let minx = hs |> Seq.minBy fst |> fst
        let maxx = hs |> Seq.maxBy fst |> fst
        let minz = hs |> Seq.minBy snd |> snd
        let maxz = hs |> Seq.maxBy snd |> snd
        if hmDiffPerCC > 0 then
            // rather than only pick one 'highest' point from each CC, instead consider all points near the top (withing hmDiffPerCC) of the CC
            for p in hs |> Seq.filter (fun (x,z) -> hihm - heightMap.[x,z] < hmDiffPerCC) do
                highPoints.Add(p,(minx,minz),(maxx,maxz))  // retain the bounds of the CC
        else
            // just choose one representative to try
            highPoints.Add((hix,hiz),(minx,minz),(maxx,maxz))  // retain the bounds of the CC
    // find the 'best' ones based on which have lots of high ground near them
    let score(x,z) =
        try
            let mutable s = 0
            let D = bestNearbyDist
            for a = x-D to x+D do
                for b = z-D to z+D do
                    s <- s + heightMap.[a,b] - (heightMap.[x,z]-20)  // want high ground nearby, but not a huge narrow spike above moderately high ground
            // TODO XXX with this, higher is not better; a great hill always score higher than a very good tall mountain
            s
        with _ -> 0  // deal with array index out of bounds
    let distance2(a,b,c,d) = (a-c)*(a-c)+(b-d)*(b-d)
    let bestHighPoints = ResizeArray()
    for ((hx,hz),a,b) in highPoints |> Seq.sortByDescending (fun (p,_,_) -> score p) do
        if hx > MINIMUM+32 && hx < MINIMUM+LENGTH-32 && hz > MINIMUM+32 && hz < MINIMUM+LENGTH-32 then  // not at edge of bounds
            if bestHighPoints |> Seq.forall (fun ((ex,ez),_,_,_s) -> distance2(ex,ez,hx,hz) > STRUCTURE_SPACING*STRUCTURE_SPACING) then
                if decorations |> Seq.forall (fun (_,ex,ez) -> distance2(ex,ez,hx,hz) > DECORATION_SPACING*DECORATION_SPACING) then
                    bestHighPoints.Add( ((hx,hz),a,b,score(hx,hz)) )
    bestHighPoints  // [(point, lo-bound-of-CC, hi-bound-of-CC, score)]

let findHidingSpot(map:MapFolder,hm:_[,],((highx,highz),(minx,minz),(maxx,maxz),_)) =
    // protect it from other structures
    // walk map looking for highest point where no air/lava withing N (20?) blocks
    // can just traverse, each time find bad block, skip N? add to exclusion zone...
    // or could maybe brute-force the mountain CCs I'm already computing?
    // ...
    // related problem: http://stackoverflow.com/questions/7245/puzzle-find-largest-rectangle-maximal-rectangle-problem
    // ...
    // ok, among mountain connected components, just mostly brute force them
    let mutable found = false
    let mutable fx,fy,fz = 0,0,0
    for y = hm.[highx,highz] downto 80 do // y is outermost loop to prioritize finding high points first
        printf "."
        if not found then
            for z = minz to maxz do
                if not found then
                    for x = minx to maxx do
                        if not found then
                            let D = 10
                            let mutable ok = true
                            for dx = -D to D do
                                if ok then
                                    for dy = -D to D do
                                        if ok then
                                            for dz = -D to D do
                                                if ok && (abs dx + abs dy + abs dz < D) then  // make a 'round radius'
                                                    let bi = map.MaybeGetBlockInfo(x+dx,y+dy,z+dz)
                                                    if bi = null then // out of bounds
                                                        ok <- false
                                                    else
                                                        let bid = bi.BlockID 
                                                        if bid = 0uy || (bid>=8uy && bid<11uy) then  // if air or water/lava
                                                            ok <- false
                            if ok then
                                found <- true
                                fx <- x
                                fy <- y
                                fz <- z
    printfn ""
    if found then
        Some((fx,fy,fz),(highx,highz))
    else
        None

let mutable hiddenX = 0
let mutable hiddenZ = 0

let findSomeMountainPeaks(rng : System.Random, map:MapFolder,hm,hmIgnoringLeaves, log:EventAndProgressLog, decorations:ResizeArray<_>) =
    let bestHighPoints = findBestPeaksAlgorithm(hmIgnoringLeaves,90,110,10,8,decorations) // TODO XXX // 8 may be too high
    let RADIUS = 20
    let bestHighPoints = bestHighPoints |> Seq.filter (fun ((_x,_z),_,_,s) -> s > 0)  // negative scores often mean tall spike with no nearby same-height ground, get rid of them
    let bestHighPoints = bestHighPoints |> Seq.filter (fun ((x,z),_,_,_) -> x*x+z*z > SPAWN_PROTECTION_DISTANCE_PEAK*SPAWN_PROTECTION_DISTANCE_PEAK)
    let bestHighPoints = bestHighPoints |> Seq.filter (fun ((x,z),_,_,_) -> x > MINIMUM+RADIUS && z > MINIMUM + RADIUS && x < MINIMUM+LENGTH-RADIUS-1 && z < MINIMUM+LENGTH-RADIUS-1)
    ////////////////////////////////////////////////
    // best hiding spot
    let timer = System.Diagnostics.Stopwatch.StartNew()
    printfn "find best hiding spot..."
    let ((bx,by,bz),(usedX,usedZ)) = bestHighPoints |> Seq.choose (fun x -> findHidingSpot(map,hm,x)) |> Seq.maxBy (fun ((_,y,_),_) -> y)
    let bestHighPoints = bestHighPoints |> Seq.filter (fun ((x,z),_,_,_) -> not(x=usedX && z=usedZ)) // rest are for mountain peaks
    log.LogSummary(sprintf "best hiding spot: %4d %4d %4d" bx by bz)
    decorations.Add('H',bx,bz)
    hiddenX <- bx
    hiddenZ <- bz
    log.LogSummary(sprintf "('find best hiding spot' sub-section took %f minutes)" timer.Elapsed.TotalMinutes)
    for dx = -1 to 1 do
        for dy = -1 to 1 do
            for dz = -1 to 1 do
                map.SetBlockIDAndDamage(bx+dx,by+dy,bz+dz,20uy,0uy)  // glass
    let quadrant = 
        if finalEX < 0 then
            if finalEZ < 0 then 
                "NorthWest (-X,-Z)"
            else
                "SouthWest (-X,+Z)"
        else
            if finalEZ < 0 then 
                "NorthEast (+X,-Z)"
            else
                "SouthEast (+X,+Z)"
    let chestItems = 
        Compounds[| 
                yield [| Byte("Count",1uy); Byte("Slot",13uy); Short("Damage",0s); String("id","minecraft:elytra"); End |]
                // jump boost pots
                for slot in [2uy;3uy;4uy;5uy;6uy;11uy;12uy;14uy;15uy] do
                    yield [| Byte("Count",1uy); Byte("Slot",slot); Short("Damage",0s); String("id","minecraft:splash_potion"); Compound("tag",[|List("CustomPotionEffects",Compounds[|[|Byte("Id",8uy);Byte("Amplifier",39uy);Int("Duration",100);End|]|]);End|]|>ResizeArray); End |]
                yield [| Byte("Slot",21uy); Byte("Count",1uy); String("id","purpur_block"); Compound("tag", [|
                            Compound("display",[|String("Name","Monument Block: Purpur Block");End|] |> ResizeArray)
                            End
                      |] |> ResizeArray); End |]
                yield [| Byte("Count",1uy); Byte("Slot",23uy); Short("Damage",0s); String("id","minecraft:written_book"); 
                         Compound("tag", Utilities.makeWrittenBookTags("Lorgon111","5. Final dungeon...", 
                                        [| 
                                        sprintf """{"text":"The final dungeon entrance is marked by a PURPLE beacon found somewhere in the %s quadrant of the map! The other items from this chest should make traveling easier :)"}""" quadrant
                                        |]) |> ResizeArray
                                  )
                         End |]
            |]
    map.SetBlockIDAndDamage(bx,by,bz,54uy,2uy)  // chest
    map.AddOrReplaceTileEntities([| [| Int("x",bx); Int("y",by); Int("z",bz); String("id","Chest"); List("Items",chestItems); String("Lock",""); String("CustomName","Winner!"); End |] |])
    putGlowstoneRecomputeLight(bx,by-1,bz,map)
    /////////////////////////////////////////////////////////////////
    // mountain peaks
    let bestHighPoints = try Seq.take 10 bestHighPoints |> ResizeArray with _e -> bestHighPoints |> ResizeArray
    // decorate map with dungeon ascent
    for (x,z),_,_,s in bestHighPoints do
        decorations.Add('P',x,z)
        let y = hmIgnoringLeaves.[x,z]
        log.LogSummary(sprintf "added mountain peak (score %d) at %d %d %d" s x y z)
        let spawners = SpawnerAccumulator("spawners around mountain peak")
        putTreasureBoxWithItemsAt(map,x,y,z,[|
                [| Byte("Slot",12uy); Byte("Count",1uy); String("id","minecraft:written_book"); Compound("tag",
                        Utilities.makeWrittenBookTags("Lorgon111","4. Secret Treasure", 
                            [| 
                               """{"text":"The secret treasure is buried at\nX : ","extra":[{"score":{"name":"X","objective":"hidden"}},{"text":"\nZ : "},{"score":{"name":"Z","objective":"hidden"}}]}"""
                            |]) |> ResizeArray); End |]
                [| Byte("Slot",14uy); Byte("Count",1uy); String("id","chest"); Compound("tag", [|
                        Compound("display",[|String("Name","Mountain Peak Loot");End|] |> ResizeArray)
                        Compound("BlockEntityTag",[|String("LootTable",sprintf "%s:chests/tier5" LootTables.LOOT_NS_PREFIX);End|] |> ResizeArray)
                        End
                    |] |> ResizeArray); End |]
            |]) // TODO heightmap, blocklight, skylight
        putThingRecomputeLight(x-2,y+4,z-2,map,"redstone_torch",5) 
        putThingRecomputeLight(x-2,y+4,z+2,map,"redstone_torch",5) 
        putThingRecomputeLight(x+2,y+4,z-2,map,"redstone_torch",5) 
        putThingRecomputeLight(x+2,y+4,z+2,map,"redstone_torch",5) 
        for i = x-RADIUS to x+RADIUS do
            for j = z-RADIUS to z+RADIUS do
                if abs(x-i) > 2 || abs(z-j) > 2 then
                    let dist = abs(x-i) + abs(z-j)
                    let pct = float (2*RADIUS-dist) / float(RADIUS*25)
                    // spawners on terrain
                    if rng.NextDouble() < pct*MOUNTAIN_PEAK_DUNGEON_SPAWNER_DATA.DensityMultiplier then
                        let x = i
                        let z = j
                        let y = hm.[x,z]
                        map.SetBlockIDAndDamage(x, y, z, 52uy, 0uy) // 52 = monster spawner   // TODO heightmap, blocklight, skylight
                        let ms = MOUNTAIN_PEAK_DUNGEON_SPAWNER_DATA.NextSpawnerAt(x,y,z,rng)
                        spawners.Add(ms)
                    // red torches for mood lighting
                    elif rng.NextDouble() < pct then
                        let x = i
                        let z = j
                        let y = hm.[x,z]
                        putThingRecomputeLight(x,y,z,map,"redstone_torch",5) 
                // ceiling over top to prevent cheesing it
                map.SetBlockIDAndDamage(i,y+5,j,7uy,0uy) // 7=bedrock
                map.SetHeightMap(i,j,y+6)
                hm.[i,j] <- y+6
        spawners.AddToMapAndLog(map,log)
    ()

let findSomeFlatAreas(rng:System.Random, map:MapFolder,hm:_[,],log:EventAndProgressLog, decorations:ResizeArray<_>) =
    // convert height map to 'goodness' function that looks for similar-height blocks nearby
    // then treat 'goodness' as 'height', and the existing 'find mountain peaks' algorithm may work
    let a = Array2D.zeroCreateBased MINIMUM MINIMUM LENGTH LENGTH
    let fScores = [| 100; 90; 75; 50; 0; -100; -999 |]
    let f(h1,h2) =
        let diff = abs(h1-h2)
        fScores.[min diff (fScores.Length-1)]
    let D = 10
    printf "PREP FLAT MAP..."
    for x = MINIMUM+D to MINIMUM+LENGTH-1-D do
        if x % 100 = 0 then printf "."
        for z = MINIMUM+D to MINIMUM+LENGTH-1-D do
            let h = if hm.[x,z] > 65 && hm.[x,z] < 90 then hm.[x,z] else 255  // only pick points above sea level but not too high
            let mutable score = 0
            for dx = -D to D do
                for dz = -D to D do
                    let ds = f(h,hm.[x+dx,z+dz])
                    score <- score + ds
            a.[x,z] <- score
    printfn ""
    let bestFlatPoints = findBestPeaksAlgorithm(a,2000,3000,D,0,decorations)
    let RADIUS = 40
    let bestFlatPoints = bestFlatPoints |> Seq.filter (fun ((x,z),_,_,_s) -> x*x+z*z > SPAWN_PROTECTION_DISTANCE_FLAT*SPAWN_PROTECTION_DISTANCE_FLAT)
    let bestFlatPoints = bestFlatPoints |> Seq.filter (fun ((x,z),_,_,_s) -> x > MINIMUM+RADIUS && z > MINIMUM + RADIUS && x < MINIMUM+LENGTH-RADIUS-1 && z < MINIMUM+LENGTH-RADIUS-1)
    let bestFlatPoints = try Seq.take 10 bestFlatPoints with _e -> bestFlatPoints
    // decorate map with dungeon
    for (x,z),_,_,s in bestFlatPoints do
        decorations.Add('F',x,z)
        log.LogSummary(sprintf "added flat set piece (score %d) at %d %d" s x z)
        let spawners = SpawnerAccumulator("spawners around cobweb flat")
        let y = hm.[x,z]
        putTreasureBoxWithItemsAt(map,x,y,z,[|
                [| Byte("Slot",12uy); Byte("Count",1uy); String("id","end_bricks"); Compound("tag", [|
                        Compound("display",[|String("Name","Monument Block: End Stone Brick");End|] |> ResizeArray)
                        End
                    |] |> ResizeArray); End |]
                [| Byte("Slot",14uy); Byte("Count",1uy); String("id","chest"); Compound("tag", [|
                        Compound("display",[|String("Name","Red Beacon Web Loot");End|] |> ResizeArray)
                        Compound("BlockEntityTag",[|String("LootTable",sprintf "%s:chests/tier4" LootTables.LOOT_NS_PREFIX);End|] |> ResizeArray)
                        End
                    |] |> ResizeArray); End |]
            |]) // TODO heightmap, blocklight, skylight
        putBeaconAt(map,x,y,z,14uy,false) // 14 = red
        map.SetBlockIDAndDamage(x,y+3,z,20uy,0uy) // glass (replace roof of box so beacon works)
        // add blazes atop
        for (dx,dz) in [-3,-3; -3,3; 3,-3; 3,3] do
            let x,y,z = x+dx, y+6, z+dz
            map.SetBlockIDAndDamage(x, y, z, 52uy, 0uy) // 52 = monster spawner   // TODO heightmap, blocklight, skylight
            let ms = MobSpawnerInfo(x=x, y=y, z=z, BasicMob="Blaze", Delay=1s)
            spawners.Add(ms)
        // add a spider jockey too
        map.SetBlockIDAndDamage(x, y+6, z, 52uy, 0uy) // 52 = monster spawner   // TODO heightmap, blocklight, skylight
        let ms = MobSpawnerInfo(x=x, y=y+6, z=z, BasicMob="Spider", Delay=1s)
        ms.ExtraNbt <- [ List("Passengers",Compounds[| [|String("id","Skeleton"); List("HandItems",Compounds[| [|String("id","bow");Int("Count",1);End|]; [| End |] |]); End|] |] )]
        spawners.Add(ms)
        // surround with danger
        for i = x-RADIUS to x+RADIUS do
            for j = z-RADIUS to z+RADIUS do
                if abs(x-i) > 2 || abs(z-j) > 2 then
                    let dist = (x-i)*(x-i) + (z-j)*(z-j) |> float |> sqrt |> int
                    let pct = float (RADIUS-dist/2) / ((float RADIUS) * 2.0)
                    let possibleSpawners = if dist < RADIUS/2 then FLAT_COBWEB_INNER_SPAWNER_DATA else FLAT_COBWEB_OUTER_SPAWNER_DATA 
                    if rng.NextDouble() < pct*possibleSpawners.DensityMultiplier then
                        let x = i
                        let z = j
                        let y = hm.[x,z] + rng.Next(2)
                        if rng.Next(10+dist/2) = 0 then
                            map.SetBlockIDAndDamage(x, y, z, 52uy, 0uy) // 52 = monster spawner   // TODO heightmap, blocklight, skylight
                            let ms = possibleSpawners.NextSpawnerAt(x,y,z,rng)
                            spawners.Add(ms)
                        elif rng.Next(3) = 0 then
                            map.SetBlockIDAndDamage(x, y, z, 30uy, 0uy) // 30 = cobweb
        spawners.AddToMapAndLog(map,log)
    ()

let doubleSpawners(map:MapFolder,log:EventAndProgressLog) =
    printfn "double spawners..."
    let spawnerTileEntities = ResizeArray()
    for x = MINIMUM to MINIMUM+LENGTH-1 do
        if x%200 = 0 then
            printfn "%d" x
        for z = MINIMUM to MINIMUM+LENGTH-1 do
            for y = 79 downto 0 do  // down, because will put new ones above
                let bid = map.GetBlockInfo(x,y,z).BlockID 
                // double all existing mob spawners
                if bid = 52uy then // 52-mob spawner
                    let bite = map.GetTileEntity(x,y,z) // caches height map as side effect
                    let originalKind =
                        match bite.Value with
                        | Compound(_,cs) ->
                            match cs |> Seq.find (fun x -> x.Name = "SpawnData") with
                            | Compound(_,sd) -> sd |> Seq.find (fun x -> x.Name = "id") |> (fun (String("id",k)) -> k)
                    map.SetBlockIDAndDamage(x, y+1, z, 52uy, 0uy) // 52 = monster spawner
                    let ms = VANILLA_DUNGEON_EXTRA(x,y+1,z,originalKind)
                    spawnerTileEntities.Add(ms.AsNbtTileEntity())
    map.AddOrReplaceTileEntities(spawnerTileEntities)
    log.LogSummary(sprintf "added %d extra dungeon spawners underground" spawnerTileEntities.Count)

let addRandomLootz(rng:System.Random, map:MapFolder,log:EventAndProgressLog,hm:_[,],biome:_[,],decorations:ResizeArray<_>) =
    printfn "add random loot chests..."
    let tileEntities = ResizeArray()
    let points = Array.init 20 (fun x -> ResizeArray())
    let noneWithin(r,points,x,_y,z) =
        let mutable ok = true
        for px,_,pz in points do
            if (x-px)*(x-px) + (z-pz)*(z-pz) < r*r then
                ok <- false
        ok
    let checkForPlus(x,y,z,corner,plus) =
        map.GetBlockInfo(x+1,y,z+1).BlockID = corner &&
        map.GetBlockInfo(x-1,y,z+1).BlockID = corner &&
        map.GetBlockInfo(x-1,y,z-1).BlockID = corner &&
        map.GetBlockInfo(x+1,y,z-1).BlockID = corner &&
        map.GetBlockInfo(x+1,y,z).BlockID = plus &&
        map.GetBlockInfo(x-1,y,z).BlockID = plus &&
        map.GetBlockInfo(x,y,z+1).BlockID = plus &&
        map.GetBlockInfo(x,y,z-1).BlockID = plus
    let putTrappedChestWithLoot(x,y,z,chestName) =
        let lootTableName = sprintf "%s:chests/%s" LootTables.LOOT_NS_PREFIX chestName
        map.SetBlockIDAndDamage(x,y,z,146uy,2uy)  // trapped chest
        tileEntities.Add [| Int("x",x); Int("y",y); Int("z",z); String("id","Chest"); List("Items",Compounds[| |]); String("LootTable",lootTableName); String("Lock",""); String("CustomName","Lootz!"); End |]
    for x = MINIMUM to MINIMUM+LENGTH-1 do
        if x%200 = 0 then
            printfn "%d" x
        for z = MINIMUM to MINIMUM+LENGTH-1 do
            let mutable nearDecoration = false
            for _,dx,dz in decorations do
                if (x-dx)*(x-dx) + (z-dz)*(z-dz) < 50*50 then // TODO distance constant
                    nearDecoration <- true
            if not nearDecoration then
                for y = 90 downto 64 do
                    let bi = map.GetBlockInfo(x,y,z)
                    let bid = bi.BlockID 
                    if bid = 48uy && checkForPlus(x,y,z,0uy,48uy) then // 48 = moss stone
                        // is a '+' of moss stone with air, e.g. surface boulder in mega taiga
                        if rng.Next(5) = 0 then // TODO probability, so don't place on all
                            if noneWithin(50,points.[0],x,y,z) then
                                let x = if rng.Next(2) = 0 then x-1 else x+1
                                let z = if rng.Next(2) = 0 then z-1 else z+1
                                putTrappedChestWithLoot(x,y,z,"aesthetic1")
                                points.[0].Add( (x,y,z) )
                    elif bid = 18uy && checkForPlus(x,y,z,0uy,18uy) 
                         || bid = 161uy && checkForPlus(x,y,z,0uy,161uy) then // 18=leaves, 161=leaves2
                        // is a '+' of leaves with air, e.g. tree top
                        if rng.Next(20) = 0 then // TODO probability, so don't place on all
                            let x = if rng.Next(2) = 0 then x-1 else x+1
                            let z = if rng.Next(2) = 0 then z-1 else z+1
                            if map.GetBlockInfo(x,y-1,z).BlockID = 18uy || map.GetBlockInfo(x,y-1,z).BlockID = 161uy then // only if block below would be leaf
                                if noneWithin(120,points.[1],x,y,z) then
                                    putTrappedChestWithLoot(x,y,z,"aesthetic1")
                                    points.[1].Add( (x,y,z) )
                    elif bid = 86uy then // 86 = pumpkin
                        let dmg = map.GetBlockInfo(x,y,z).BlockData
                        if rng.Next(4) = 0 then // TODO probability, so don't place on all
                            // TODO could be on hillside, and so chest under maybe exposed
                            if noneWithin(50,points.[2],x,y,z) then
                                putThingRecomputeLight(x,y,z,map,"lit_pumpkin",int dmg) // replace with jack'o'lantern  // TODO found one, was not giving off light, hm
                                // chest below
                                let y = y - 1
                                putTrappedChestWithLoot(x,y,z,"aesthetic2")
                                points.[2].Add( (x,y,z) )
                    elif bid = 9uy && bi.BlockData = 0uy then  // water falling straight down has different damage value, only want sources
                        if y >= hm.[x,z]-1 then // 9=water, at top of heightmap (-1 because lake surface is actually just below heightmap)
                            let b = biome.[x,z]
                            // not one of these
                            let excludedBiomes = [|0uy; 10uy; 24uy   // oceans
                                                   7uy; 11uy         // rivers
                                                   16uy; 25uy; 26uy  // beaches
                                                   6uy; 134uy        // swamp
                                                 |]
                            if not(excludedBiomes |> Array.exists (fun x -> x = b)) then
                                // probably a surface lake
                                if rng.Next(20) = 0 then
                                    if noneWithin(50,points.[3],x,y,z) then
                                        // TODO where put? bottom? any light cue? ...
                                        // for now just under water
                                        let y = y - 1
                                        putTrappedChestWithLoot(x,y,z,"aesthetic2")
                                        points.[3].Add( (x,y,z) )
                    elif bid = 12uy then // 12=sand
                        if y >= hm.[x,z]-1 then // at top of heightmap (-1 because surface is actually just below heightmap)
                            let deserts = [| 2uy; 17uy; 130uy |]
                            if deserts |> Array.exists (fun b -> b = biome.[x,z]) then
                                if checkForPlus(x,y,z,12uy,12uy) && checkForPlus(x,y+1,z,0uy,0uy) && checkForPlus(x,y+2,z,0uy,0uy) then // flat square of sand with air above
                                    if rng.Next(20) = 0 then // TODO probability, so don't place on all
                                        if noneWithin(120,points.[4],x,y,z) then
                                            let y = y + 1
                                            // put cactus
                                            for dy = 0 to 1 do
                                                map.SetBlockIDAndDamage(x+1,y+dy,z+1,81uy,0uy)  // cactus
                                                map.SetBlockIDAndDamage(x+1,y+dy,z-1,81uy,0uy)  // cactus
                                                map.SetBlockIDAndDamage(x-1,y+dy,z-1,81uy,0uy)  // cactus
                                                map.SetBlockIDAndDamage(x-1,y+dy,z+1,81uy,0uy)  // cactus
                                            // put chest
                                            putTrappedChestWithLoot(x,y,z,"aesthetic1")
                                            points.[4].Add( (x,y,z) )
                                            // TODO sometimes be a trap
                    else
                        () // TODO other stuff
                        // 56, 205, 20, 65,
                // end for y
                let y = 62
                let PIXELS = 
                    [|
                        "............."
                        ".XXX..X.XXXX."
                        ".X..X.X.X...."
                        ".X..X.X.X.XX."
                        ".X..X.X.X..X."
                        ".XXX..X.XXXX."
                        "............."
                        "....X...X...."
                        ".....X.X....."
                        "......X......"
                        ".....X.X....."
                        "....X...X...."
                        "............."
                    |]
                let DIGMAX = PIXELS.Length 
                assert(PIXELS.Length = PIXELS.[0].Length)
                if x < MINIMUM+LENGTH-1 - DIGMAX && z < MINIMUM+LENGTH-1 - DIGMAX then
                    if map.GetBiome(x,z)=6uy && map.GetBlockInfo(x,y,z).BlockID=9uy then // swamp, water
                        if noneWithin(120,points.[19],x,y,z) then
                            if rng.Next(40) = 0 then // TODO probability, so don't place on all, or all NE corners, or whatnot (data point: at rng(40), 13 of 17 swamps got covered)
                                let mutable ok,i = true,0
                                while ok && i < DIGMAX*DIGMAX do
                                    i <- i + 1
                                    let dx = i % DIGMAX
                                    let dz = i / DIGMAX
                                    let x,z = x+dx, z+dz
                                    if map.GetBiome(x,z)<>6uy || map.GetBlockInfo(x,y,z).BlockID<>9uy then // swamp, water
                                        ok <- false
                                if ok then
                                    printfn "FOUND SWAMP %d %d" x z
                                    // put "DIG" and "X" with entities so frost walker exposes
                                    let mkArmorStandAt(x,y,z) = 
                                        [|
                                            // ArmorStand versus mob - players can move through AS without collision, though both block attacks
                                            NBT.String("id","ArmorStand")
                                            //NBT.String("id","Silverfish")
                                            NBT.List("Pos",Doubles([|float x + 0.5; float y + 0.9; float z + 0.5|]))  // high Y to try to prevent them from preventing people from digging...
                                            NBT.List("Motion",Doubles([|0.0; 0.0; 0.0|]))
                                            NBT.List("Rotation",Doubles([|0.0; 0.0|]))
                                            NBT.Byte("Marker",0uy) // need hitbox to work with FW
                                            NBT.Byte("Small",1uy) // small hitbox to avoid interfering much with world
                                            NBT.Byte("Invisible",1uy)
                                            NBT.Byte("NoGravity",1uy)
                                            //NBT.Byte("Silent",1uy)
                                            //NBT.Byte("Invulnerable",1uy)
                                            //NBT.Byte("NoAI",1uy)
                                            //NBT.Byte("PersistenceRequired",1uy)
                                            //NBT.List("ActiveEffects",Compounds([|[|Byte("Id",14uy);Byte("Amplifier",0uy);Int("Duration",999999);Byte("ShowParticles",0uy);End|]|]))
                                            NBT.End
                                        |]
                                    let ents = ResizeArray()
                                    for dx = 0 to DIGMAX-1 do
                                        for dz = 0 to DIGMAX-1 do
                                            if PIXELS.[dx].[DIGMAX-1-dz] = 'X' then
                                                ents.Add(mkArmorStandAt(x+dx,y,z+dz))
                                    map.AddEntities(ents)
                                    // place hidden trapped chest
                                    let x,y,z = x+9,y-5,z+6  // below the 'X'
                                    putTrappedChestWithLoot(x,y,z,"aesthetic3")
                                    points.[19].Add( (x,y,z) )
            // end if not near deco
        // end for z
    // end for x
    map.AddOrReplaceTileEntities(tileEntities)
    log.LogSummary(sprintf "added %d extra loot chests: %s" tileEntities.Count (points |> Array.map (fun ps -> sprintf "%d" ps.Count) |> String.concat(", ")))

let placeStartingCommands(map:MapFolder,hm:_[,]) =
    let placeCommand(x,y,z,command,bid,name) =
        map.SetBlockIDAndDamage(x,y,z,bid,0uy)  // command block
        map.AddOrReplaceTileEntities([| [| Int("x",x); Int("y",y); Int("z",z); String("id","Control"); Byte("auto",1uy); String("Command",command); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); End |] |])
        if bid <> 211uy then
            map.AddTileTick(name,100,0,x,y,z)
    let placeImpulse(x,y,z,command) = placeCommand(x,y,z,command,137uy,"minecraft:command_block")
    let placeRepeating(x,y,z,command) = placeCommand(x,y,z,command,210uy,"minecraft:repeating_command_block")
    //let placeChain(x,y,z,command) = placeCommand(x,y,z,command,211uy,"minecraft:chain_command_block")
    let h = hm.[1,1] // 1,1 since 0,0 has commands
    let y = ref 255
    let R(c) = placeRepeating(0,!y,0,c); decr y
    let I(c) = placeImpulse(0,!y,0,c); decr y
    //let C(c) = placeChain(0,!y,0,c); decr y
    // add diorite pillars to denote border between light and dark
    for i = 0 to 99 do
        let theta = System.Math.PI * 2.0 * float i / 100.0
        let x = cos theta * float DAYLIGHT_RADIUS |> int
        let z = sin theta * float DAYLIGHT_RADIUS |> int
        let h = hm.[x,z] + 5
        if h > 60 then
            for y = 60 to h do
                map.SetBlockIDAndDamage(x,y,z,1uy,3uy)  // diorite
            putGlowstoneRecomputeLight(x,h+1,z,map)
    R(sprintf "execute @p[r=%d,x=0,y=80,z=0] ~ ~ ~ time set 1000" DAYLIGHT_RADIUS)  // TODO multiplayer?
    R(sprintf "execute @p[rm=%d,x=0,y=80,z=0] ~ ~ ~ time set 14500" DAYLIGHT_RADIUS)
    // TODO first time enter night, give a message explaining?
    I("worldborder set 2048")
    I("gamerule doDaylightCycle false")
    //I("gamerule keepInventory true")
    I("weather clear 999999")
    I("give @a iron_axe 1 0 {ench:[{id:18s,lvl:5s}]}")
    I("give @a shield")
    I("give @a cooked_beef 6")  // TODO too op? maybe more bread is better
    I("give @a dirt 64")
    I("scoreboard objectives add hidden dummy")
    I("scoreboard objectives add Deaths stat.deaths")
    I("scoreboard objectives setdisplay sidebar Deaths")
    I(sprintf "scoreboard players set X hidden %d" hiddenX)
    I(sprintf "scoreboard players set Z hidden %d" hiddenZ)
    I(sprintf "scoreboard players set fX hidden %d" finalEX)
    I(sprintf "scoreboard players set fZ hidden %d" finalEZ)
    I("scoreboard players set CTM hidden 0")
    // repeat blocks to check for CTM completion
    I(sprintf "blockdata 0 %d 3 {auto:1b}" (h-2))
    I(sprintf "blockdata 1 %d 3 {auto:1b}" (h-2))
    I(sprintf "blockdata 2 %d 3 {auto:1b}" (h-2))
    I(Utilities.makeCommandGivePlayerWrittenBook("Lorgon111","Rules",
                                                 [|
                                                    Utilities.wrapInJSONTextContinued "RULES\n\nMy personal belief is that in Minecraft there are no rules; you should play whatever way you find most fun."
                                                    Utilities.wrapInJSONTextContinued "That said, I have created a map designed to help you have fun, and the next pages have some suggestions that I think will make it the most fun for most players."
                                                    Utilities.wrapInJSONTextContinued "Suggestions\n\nThis map has only been tested in single-player.\n\nSurvive in any way you can think of, and try to find the 3 monument blocks to place atop the monument at spawn."
                                                    Utilities.wrapInJSONText "Use normal difficulty.\n\nYou CAN use/move/craft enderchests.\n\nDon't go to Nether or leave the worldborder.\n\nYou can use beds to set spawn, but they don't affect the daylight cycle."
                                                 |]))
    I(Utilities.makeCommandGivePlayerWrittenBook("Lorgon111","Map Overview",
                                                 [|
                                                    Utilities.wrapInJSONTextContinued "OVERVIEW\n\nCTM Maps are often most fun to play blind, but there are a few things you ought to know about this map before getting started."
                                                    Utilities.wrapInJSONTextContinued "Note: if you have not played Minecraft 1.9 before, I do not recommend playing this map as your first 1.9 map. Get used to 1.9 first."
                                                    Utilities.wrapInJSONTextContinued "CTM\n\nThis is a three objective Complete The Monument (CTM) map.  The goal/objective blocks you need are hidden in chests in various dungeons in the world."
                                                    Utilities.wrapInJSONTextContinued "OPEN WORLD\n\nThis is an open-world map that takes place on a 2048x2048 piece of (heavily modified) Minecraft terrain. Spawn is 0,0 and there's a worldborder 1024 blocks out."
                                                    Utilities.wrapInJSONTextContinued "...\nThere are multiple versions of most dungeons, and you'll find many just by wandering around. You'll find more books that suggest the best thing to do next after completing each dungeon."
                                                    Utilities.wrapInJSONTextContinued "DAYLIGHT CYCLE\n\nThe sun is not moving in the sky. Near spawn it's permanently daytime, and the rest you can discover for yourself."
                                                    Utilities.wrapInJSONTextContinued "MOBS\n\nMob loot drops are heavily modified in this map, but the mobs themselves are completely vanilla. There are many spawners in the map; both to guard loot, and to surprise you."
                                                    Utilities.wrapInJSONTextContinued "TECH PROGRESSION\n\nThere's no netherwart in the map and no potions given in chests.\n\nYou'll probably spend a little time with wood tools before managing to acquire some stone/gold/iron upgrades."
                                                    Utilities.wrapInJSONTextContinued "...\nThere will be lots of anvils and enchanted books; you don't have to farm xp/drops, or mine for diamonds/enchanting, in order to progress, but you can if you want."
                                                    Utilities.wrapInJSONTextContinued "RANDOMLY GENERATED\n\nThis map was created entirely via algorithms. The Minecraft terrain generator made the original terrain, and my program added dungeons, loot, monument, & secrets automatically."
                                                    Utilities.wrapInJSONText "...\nIf you encounter something especially weird, don't over-think the map-maker rationale; it's possible my code had a bug and did something silly."
                                                 |]))
    I(Utilities.makeCommandGivePlayerWrittenBook("Lorgon111","Getting started",
                                                 [|
                                                    Utilities.wrapInJSONTextContinued "You have some starting items, but you'll still want to gather some wood and do some caving near spawn to get more supplies."
                                                    Utilities.wrapInJSONText "Explore! If you travel too far from spawn, things will get scarier, so I recommend caving near spawn to improve your gear until you are strong enough to venture further and you discover suggestions of what to try next."
                                                 |]))
    I(Utilities.makeCommandGivePlayerWrittenBook("Lorgon111","Hints and Spoilers",
                                                 [|
                                                    Utilities.wrapInJSONTextContinued "The following pages outline the simplest 'progression order' of the map. You can refer to this if you get stuck, but DON'T READ THIS UNLESS YOU NEED TO BECAUSE YOU'RE STUCK."
                                                    Utilities.wrapInJSONTextContinued "Note: In the map folder on disk, there are two pictures of the terrain, one has locations of major dungeons labeled (spoilers), the other does not."
                                                    Utilities.wrapInJSONTextContinued "1. GEARING UP\n\nGetting your first cobblestone is not so easy, though there are at least 5 different ways you can obtain it."
                                                    Utilities.wrapInJSONTextContinued "...\nCaving near spawn to find dungeons (which are somewhat common) or abandoned mineshafts is the best way to find initial loot to gear up. You can also mine iron and gold (or even diamonds) for early gear."
                                                    Utilities.wrapInJSONTextContinued "2. GREEN BEACONS\n\nNext explore the world for GREEN beacons ('B' on spoiler map image), which lead to underground dungeons. You'll find a marked path through a cave and spawners guarding a good loot box."
                                                    Utilities.wrapInJSONTextContinued "3. RED BEACONS\n\nNext explore the world for RED beacons ('F' on spoiler map image): cobwebbed dungeons on the surface.  The loot box at the center has the first monument block and more gear upgrades."
                                                    Utilities.wrapInJSONTextContinued "4. MOUNTAIN PEAKS\n\nNext explore the world for dangerous looking mountain peaks ('P' on spoiler map image), illuminated with redstone torches. Buried treasure directions, and more loot!"
                                                    Utilities.wrapInJSONTextContinued "5. SECRET TREASURE\n\nNext dig for treasure at the given coordinates ('H' on the spoiler map image). You'll find the second monument block, and faster travel/exploration."
                                                    Utilities.wrapInJSONText "6. FINAL DUNGEON\n\nFinally explore one quadrant of the world for a PURPLE beacon ('X' on spoiler map image), the final dungeon. It's like the first dungeon, but harder, and has the final monument block."
                                                 |]))
    I(sprintf "fill 0 %d 0 0 253 0 air" !y) // remove all the ICBs, just leave the RCBs
    putBeaconAt(map,1,h,1,0uy,false)  // beacon at spawn for convenience
    // clear space above beacon
    for x = -1 to 3 do
        for z = -1 to 3 do
            if x<>1 || z<>1 then
                map.SetBlockIDAndDamage(x,h+0,z,0uy,0uy) // air
            map.SetBlockIDAndDamage(x,h+1,z,0uy,0uy) // air
            map.SetBlockIDAndDamage(x,h+2,z,0uy,0uy) // air
            map.SetBlockIDAndDamage(x,h+3,z,0uy,0uy) // air
    // for teleport area...
    for x = -2 to 4 do
        for z = 2 to 8 do
            for dh = 9 to 15 do
                map.SetBlockIDAndDamage(x,h+dh,z,166uy,0uy) // 166=barrier  // TODO barrier is bad design, do something else
    // put monument
    for x = -1 to 3 do
        for z = 3 to 7 do
            map.SetBlockIDAndDamage(x,h-1,z,7uy,0uy) // bedrock
            for dh = 0 to 8 do
                map.SetBlockIDAndDamage(x,h+dh,z,0uy,0uy) // air
            // skip dh=9, has barrier
            for dh = 10 to 14 do
                map.SetBlockIDAndDamage(x,h+dh,z,20uy,0uy) // 20=glass (will be teleport area)
    // remove glass cmd
    map.SetBlockIDAndDamage(0,h-2,0,137uy,0uy)
    map.AddOrReplaceTileEntities([| [| Int("x",0); Int("y",h-2); Int("z",0); String("id","Control"); Byte("auto",0uy); String("Command",sprintf "/fill %d %d %d %d %d %d air" -2 (h+11) 2 4 (h+15) 8); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); End |] |])
    // rest of monument
    map.SetBlockIDAndDamage(2,h,6,7uy,0uy)
    map.SetBlockIDAndDamage(1,h,6,7uy,0uy)
    map.SetBlockIDAndDamage(0,h,6,7uy,0uy)
    map.SetBlockIDAndDamage(2,h,5,68uy,2uy) // wall_sign
    map.SetBlockIDAndDamage(1,h,5,68uy,2uy)
    map.SetBlockIDAndDamage(0,h,5,68uy,2uy)
    map.AddOrReplaceTileEntities([|
                                    [| Int("x",2); Int("y",h); Int("z",5); String("id","Sign"); String("Text1","""{"text":"End Stone Brick"}"""); String("Text2","""{"text":""}"""); String("Text3","""{"text":""}"""); String("Text4","""{"text":""}"""); End |]
                                    [| Int("x",1); Int("y",h); Int("z",5); String("id","Sign"); String("Text1","""{"text":"Purpur Block"}"""); String("Text2","""{"text":""}"""); String("Text3","""{"text":""}"""); String("Text4","""{"text":""}"""); End |]
                                    [| Int("x",0); Int("y",h); Int("z",5); String("id","Sign"); String("Text1","""{"text":"Sponge"}"""); String("Text2","""{"text":""}"""); String("Text3","""{"text":""}"""); String("Text4","""{"text":""}"""); End |]
                                 |])
    let r = map.GetRegion(1,1)
    let cmds(x,tilename) = 
        [|
            P (sprintf "testforblock %d %d 6 %s" x (h+1) tilename)
            C "scoreboard players add CTM hidden 1"
            C """tellraw @a ["You placed ",{"score":{"name":"CTM","objective":"hidden"}}," of 3 objective blocks so far!"]"""
            C "fill ~ ~ ~ ~ ~ ~-3 air"
        |]
    for x,tilename in [0,"sponge"; 1,"purpur_block"; 2,"end_bricks"] do
        r.PlaceCommandBlocksStartingAt(x,h-2,3,cmds(x,tilename),"check ctm block")
    let finalCmds = 
        [|
            O "scoreboard players test CTM hidden 3 *"
            C """tellraw @a ["You win the map! Daylight cycle restored! World border removed! Feel free to continue playing normal Minecraft now; terrain generation becomes normal after about 1300 blocks from spawn."]"""
            // TODO nether still different
            // TODO loot tables still different
            C "worldborder set 30000000"
            C "gamerule doDaylightCycle true"
            C "fill 0 254 0 0 255 0 air"  // remove day/night blocks
        |]
    r.PlaceCommandBlocksStartingAt(0,h-3,3,finalCmds,"check ctm win")

let placeTeleporters(map:MapFolder, hm:_[,], log:EventAndProgressLog, decorations:ResizeArray<_>) =
    let placeCommand(x,y,z,command,bid,auto,_name) =
        map.SetBlockIDAndDamage(x,y,z,bid,0uy)  // command block
        map.AddOrReplaceTileEntities([| [| Int("x",x); Int("y",y); Int("z",z); String("id","Control"); Byte("auto",auto); String("Command",command); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); End |] |])
    let placeImpulse(x,y,z,command) = placeCommand(x,y,z,command,137uy,0uy,"minecraft:command_block")
    let placeRepeating(x,y,z,command) = placeCommand(x,y,z,command,210uy,1uy,"minecraft:repeating_command_block")
    let placeChain(x,y,z,command) = placeCommand(x,y,z,command,211uy,1uy,"minecraft:chain_command_block")
    for xs,zs,dirName,spx,spz in [-512,-512,"NorthWest (-X,-Z)",-1,3; -512,512,"SouthWest (-X,+Z)",-1,7; 512,512,"SouthEast (+X,+Z)",3,7; 512,-512,"NorthEast (+X,-Z)",3,3] do
        let mutable found = false
        for dx = -30 to 30 do
            if not found then
                for dz = -30 to 30 do
                    if not found then
                        let x = xs + dx
                        let z = zs + dz
                        let h = hm.[x,z]
                        let mutable ok = true
                        for i = 0 to 4 do
                            for j = 0 to 4 do
                                if hm.[x+i,z+j] <> h then
                                    ok <- false
                        if ok then
                            found <- true
                            log.LogSummary(sprintf "TP at %d %d" x z)
                            decorations.Add('T',x,z)
                            for i = 0 to 4 do
                                for j = 0 to 4 do
                                    map.SetBlockIDAndDamage(x+i,h+0,z+j,7uy,0uy)  // 7=bedrock
                                    map.SetBlockIDAndDamage(x+i,h+1,z+j,0uy,0uy)  // 0=air
                                    map.SetBlockIDAndDamage(x+i,h+2,z+j,0uy,0uy)  // 0=air
                                    map.SetBlockIDAndDamage(x+i,h+3,z+j,0uy,0uy)  // 0=air
                                    map.SetBlockIDAndDamage(x+i,h+4,z+j,7uy,0uy)  // 7=bedrock
                                    map.SetBlockIDAndDamage(x+i,h+5,z+j,0uy,0uy)  // 0=air
                                    map.SetBlockIDAndDamage(x+i,h+6,z+j,0uy,0uy)  // 0=air
                                    map.SetBlockIDAndDamage(x+i,h+7,z+j,0uy,0uy)  // 0=air
                            map.SetBlockIDAndDamage(x+2,h+2,z+2,209uy,0uy) // 209=end_gateway
                            map.AddOrReplaceTileEntities([| [| Int("x",x+2); Int("y",h+2); Int("z",z+2); String("id","EndGateway"); Long("Age",180L); Byte("ExactTeleport",1uy); Compound("ExitPortal",[Int("X",1);Int("Y",hm.[1,1]+12);Int("Z",5);End]|>ResizeArray); End |] |])
                            putBeaconAt(map,x+2,h+12,z+2,0uy,false)
                            placeRepeating(x+2,h+19,z+2,sprintf "execute @p[r=25] ~ ~ ~ blockdata %d %d %d {auto:1b}" (x+2) (h+18) (z+2)) // absolute coords since execute-at
                            map.AddTileTick("minecraft:repeating_command_block",100,0,x+2,h+19,z+2)
                            placeImpulse(x+2,h+18,z+2,sprintf "blockdata %d %d %d {auto:1b}" 0 (hm.[1,1]-2) 0) // remove glass at spawn //note brittle coords of block
                            placeChain(x+2,h+17,z+2,"blockdata ~ ~-1 ~ {auto:1b}") // run rest after that
                            placeImpulse(x+2,h+16,z+2,sprintf "setblock %d %d %d end_gateway 0 replace {ExactTeleport:1b,ExitPortal:{X:%d,Y:%d,Z:%d}}" spx (hm.[1,1]+12) spz (x+2) (h+6) (z+2))
                            placeChain(x+2,h+15,z+2,sprintf "setblock %d %d %d chest 2 replace {CustomName:\"Teleporter to %s\",Items:[{Slot:13b,id:water_bucket,Count:1}]}" spx (hm.[1,1]+11) spz dirName)
                            placeChain(x+2,h+14,z+2,"""tellraw @a [{"text":"A two-way teleporter to/from spawn has been unlocked nearby"}]""")
                            placeChain(x+2,h+13,z+2,"fill ~ ~ ~ ~ ~6 ~ air") // erase us
        if not found then
            log.LogSummary(sprintf "FAILED TO FIND TELEPORTER LOCATION NEAR %d %d" xs zs)
            failwith "no teleporters"
    ()

let findMountainToHollowOut(map : MapFolder, hm, hmIgnoringLeaves :_[,], log, decorations) =
    let YMAX = 100
    let (xmin,zmin),(xmax,zmax),area = findMaximalRectangle(Array2D.initBased MINIMUM MINIMUM LENGTH LENGTH (fun x z -> hmIgnoringLeaves.[x,z] > YMAX))
    printfn "%A %A %d" (xmin,zmin) (xmax,zmax) area
    let midx = xmin + (xmax-xmin)/2
    let midz = zmin + (zmax-zmin)/2
    let D = 100
    let XMIN = midx - D/2
    let ZMIN = midz - D/2
    let YMIN = 60
    let data = Array2D.initBased (XMIN-1) (ZMIN-1) (D+2) (D+2) (fun x z -> // data.[x,z].[y] = my temp block stuff
        Array.init 256 (fun y -> 
            if x = XMIN-1 || x = XMIN+D || z = ZMIN-1 || z = ZMIN+D then 999 // sentinels at array edges
            else if y <= hmIgnoringLeaves.[x,z] then 0 else 999))   // don't touch any blocks above HM
    // find existing block shell
    let q = System.Collections.Generic.Queue<_>()
    for x = XMIN to XMIN+D-1 do
        for z = ZMIN to ZMIN+D-1 do
            let h = hmIgnoringLeaves.[x,z]
            let mutable y = h  // topmost block
            data.[x,z].[y] <- 1  // 1 = existing shell
            // make bedrock shell "below"
            y <- y - 1
            data.[x,z].[y] <- 1  // 1 = existing shell
            while y > hmIgnoringLeaves.[x-1,z] || y > hmIgnoringLeaves.[x,z-1] || y > hmIgnoringLeaves.[x+1,z] || y > hmIgnoringLeaves.[x,z+1] do
                y <- y - 1
                data.[x,z].[y] <- 1  // 1 = existing shell
                q.Enqueue(x,y,z)
            data.[x,z].[y] <- 2  // 2 = bedrock inner shell
            q.Enqueue(x,y,z)
    let GOALX, GOALY, GOALZ = midx, YMIN, midz
    let compute(x,y,z) =
        if data.[x,z].[y+1] = 1 || data.[x-1,z].[y] = 1 || data.[x,z-1].[y] = 1 || data.[x+1,z].[y] = 1 || data.[x,z+1].[y] = 1 then
            data.[x,z].[y] <- 2 // bedrock if next to outer shell
        else
            data.[x,z].[y] <- 3 // air if otherwise in interior
        q.Enqueue(x,y,z)
    // find rest of bedrock/air space
    while not(q.Count=0) do
        let x,y,z = q.Dequeue()
        if x > GOALX && data.[x-1,z].[y]=0 then
            compute(x-1,y,z)
        elif x < GOALX && data.[x+1,z].[y]=0 then
            compute(x+1,y,z)
        if z > GOALZ && data.[x,z-1].[y]=0 then
            compute(x,y,z-1)
        elif z < GOALZ && data.[x,z+1].[y]=0 then
            compute(x,y,z+1)
        if y > GOALY && data.[x,z].[y-1]=0 then
            compute(x,y-1,z)
    for x = XMIN to XMIN+D-1 do
        for z = ZMIN to ZMIN+D-1 do
            for y = YMIN to 255 do
                if data.[x,z].[y] = 2 then
                    map.SetBlockIDAndDamage(x,y,z,7uy,0uy) // 7=bedrock
                elif data.[x,z].[y] = 3 then
                    map.SetBlockIDAndDamage(x,y,z,0uy,0uy) // 0 = air
    // TODO deal with overhangs showing exposed bedrock?
    // TODO entrance, floor, populate
    // TODO log, decorations, etc




let makeCrazyMap(worldSaveFolder, rngSeed, customTerrainGenerationOptions) =
    let rng = ref(System.Random())
    let mainTimer = System.Diagnostics.Stopwatch.StartNew()
    let map = new MapFolder(worldSaveFolder + """\region\""")
    let log = EventAndProgressLog()
    let decorations = ResizeArray()
    let hm = Array2D.zeroCreateBased MINIMUM MINIMUM LENGTH LENGTH
    let hmIgnoringLeaves = Array2D.zeroCreateBased MINIMUM MINIMUM LENGTH LENGTH
    let biome = Array2D.zeroCreateBased MINIMUM MINIMUM LENGTH LENGTH
    let xtime _ = 
        printfn "SKIPPING SOMETHING"
        log.LogSummary("SKIPPED SOMETHING")
    let time f =
        rng := new System.Random(rngSeed)  // each section re-seeds, to avoid effects bleeding across sections
        let timer = System.Diagnostics.Stopwatch.StartNew()
        f()
        printfn "Time so far: %f minutes" mainTimer.Elapsed.TotalMinutes
        log.LogSummary(sprintf "(this section took %f minutes)" timer.Elapsed.TotalMinutes)
        log.LogSummary("-----")
    log.LogSummary("Debugging output for automated map generation")
    log.LogSummary("DON'T READ THIS UNLESS YOU WANT SPOILERS")
    log.LogSummary("-------------------")
    log.LogSummary("Terrain generation options:")
    log.LogSummary(customTerrainGenerationOptions)
    log.LogSummary("-------------------")
    time (fun () ->
        let LOX, LOY, LOZ = MINIMUM, 1, MINIMUM
        let HIY = 255
        printf "CACHE SECT"
        for y in [LOY .. 16 .. HIY] do
            printf "."
            for x in [LOX .. 16 .. LOX+LENGTH-1] do
                for z in [LOZ .. 16 .. LOZ+LENGTH-1] do
                    ignore <| map.GetOrCreateSection(x,y,z)  // cache each section
        printfn ""
        )
    time (fun () ->
        log.LogSummary("CACHE HM AND BIOME...")
        let wm = "LORGON111"
        for x = MINIMUM to MINIMUM+LENGTH-1 do
            if x%200 = 0 then
                printfn "%d" x
            for z = MINIMUM to MINIMUM+LENGTH-1 do
                let _bi = map.GetBlockInfo(x,0,z) // caches height map as side effect
                let xx,zz = (x+51200)%512, (z+51200)%512
                if xx >= 256-22 && xx <= 256+22 && zz >= 256-2 && zz <= 256+2 then
                    // watermark
                    let i,ix = (xx - (256-22)) / 5, (xx - (256-22)) % 5
                    let j = zz - (256-2)
                    match Utilities.ALPHABET5INDEX wm.[i] with
                    | Some i ->
                         if Utilities.ALPHABET5.[j].[5*i+ix] = 'X' then
                            map.SetBiome(x,z,15uy)  // 15 = MushroomIslandShore
                    | None -> failwith "unexpected alpha wm"
                biome.[x,z] <- map.GetBiome(x,z)
                let h = map.GetHeightMap(x,z)
                hm.[x,z] <- h
                let mutable y = h
                while (let bid = map.MaybeGetBlockInfo(x,y,z).BlockID in bid = 0uy || bid = 18uy || bid = 161uy || bid = 78uy || bid = 31uy || bid = 175uy || bid = 32uy || bid = 37uy || bid = 38uy || bid = 39uy || bid = 40uy) do // air, leaves, leaves2, snow_layer, tallgrass, double_plant, deadbush, yellow_flower, red_flower, brown_mushroom, red_mushroom
                    y <- y - 1
                hmIgnoringLeaves.[x,z] <- y
        )
    xtime (fun () -> findMountainToHollowOut(map, hm, hmIgnoringLeaves, log, decorations))
    xtime (fun () -> placeTeleporters(map, hm, log, decorations))
    xtime (fun () -> doubleSpawners(map, log))
    xtime (fun () -> substituteBlocks(!rng, map, log))
    time (fun () -> findUndergroundAirSpaceConnectedComponents(!rng, map, hm, log, decorations))
    xtime (fun () -> findSomeFlatAreas(!rng, map, hm, log, decorations))
    xtime (fun () -> findSomeMountainPeaks(!rng, map, hm, hmIgnoringLeaves, log, decorations))
    xtime (fun () -> findCaveEntrancesNearSpawn(map,hm,hmIgnoringLeaves,log))
    xtime (fun () -> addRandomLootz(!rng, map, log, hm, biome, decorations))  // after others, reads decoration locations
    xtime (fun () -> replaceSomeBiomes(!rng, map, log, biome))
    xtime (fun() ->   // after hiding spots figured
        log.LogSummary("START CMDS")
        placeStartingCommands(map,hm))
    time (fun() ->
        log.LogSummary("SAVING FILES")
        map.WriteAll()
        printfn "...done!")
    xtime (fun() -> 
        log.LogSummary("WRITING MAP PNG IMAGES")
        Utilities.makeBiomeMap(worldSaveFolder+"""\region""", biome, MINIMUM, LENGTH, MINIMUM, LENGTH, 
                                [DAYLIGHT_RADIUS; SPAWN_PROTECTION_DISTANCE_GREEN; SPAWN_PROTECTION_DISTANCE_FLAT; SPAWN_PROTECTION_DISTANCE_PEAK], decorations)
        )
    log.LogSummary(sprintf "Took %f total minutes" mainTimer.Elapsed.TotalMinutes)

    printfn ""
    printfn "SUMMARY"
    printfn ""
    for s in log.SummaryEvents() do
        printfn "%s" s
    System.IO.File.WriteAllLines(System.IO.Path.Combine(worldSaveFolder,"summary.txt"),log.SummaryEvents())
    System.IO.File.WriteAllLines(System.IO.Path.Combine(worldSaveFolder,"all.txt"),log.AllEvents())
    printfn "press a key to end"
    System.Console.ReadKey() |> ignore
    // TODO automate world creation...



(*

After 1.5 hours, I had P2 iron armor, inf Pow5 box, lousy sword, ok pick, ~20 steak, able to take on flat cobwebs
Can roughly speed past that by gifting yourself a tier 4 chest, then almost immediately can take on mountains

Previous runs took 2 hours to get to start of 1st beacon, have I gotten better/inured, or has it gotten too easy at start?

-----

first full playthrough (Dec 20, seed 27):
 - 2 hours to finish first cave dungeon
 - 1 hour conquer flat (block 1)
 - 30 mins second flat for more loot
 - 30 mins mountain (nearby, easyish)
 - 30 mins for secret (block 2)
 - 1 hour final dungeon (block 3)
5.5 hours to complete, knowing route and just speedrunning it

*)



(*

Dec 19
SUMMARY

(this section took 0.185253 minutes)
-----
CACHE HM AND BIOME...
(this section took 0.093584 minutes)
-----
TP at -542 -510
TP at -523 490
TP at 482 501
TP at 484 -515
(this section took 0.000596 minutes)
-----
added 878 extra dungeon spawners underground
(this section took 1.516802 minutes)
-----
added random spawners underground
   spawners along path:   Total:1996   Blaze:136   Creeper:126   Skeleton:575   Spider:565   Zombie:594
   spawners along path:   Total:897   Blaze:146   CaveSpider:148   Creeper:140   Skeleton:141   Spider:179   Zombie:143
(this section took 2.207281 minutes)
-----
added flat set piece at -543 300
   spawners along path:   Total: 72   Blaze:  4   CaveSpider: 32   Spider:  9   Spiderextra:  9   Witch: 18
added flat set piece at -282 -107
   spawners along path:   Total: 66   Blaze:  4   CaveSpider: 33   Spider:  4   Spiderextra: 11   Witch: 14
added flat set piece at -305 482
   spawners along path:   Total: 65   Blaze:  4   CaveSpider: 34   Spider:  8   Spiderextra:  7   Witch: 12
added flat set piece at -745 5
   spawners along path:   Total: 89   Blaze:  4   CaveSpider: 44   Spider:  9   Spiderextra: 15   Witch: 17
added flat set piece at -706 503
   spawners along path:   Total: 63   Blaze:  4   CaveSpider: 27   Spider: 15   Spiderextra:  6   Witch: 11
added flat set piece at -638 885
   spawners along path:   Total: 61   Blaze:  4   CaveSpider: 32   Spider:  6   Spiderextra:  4   Witch: 15
added flat set piece at 898 -366
   spawners along path:   Total: 70   Blaze:  4   CaveSpider: 35   Spider:  9   Spiderextra: 10   Witch: 12
added flat set piece at 963 838
   spawners along path:   Total: 69   Blaze:  4   CaveSpider: 28   Spider:  7   Spiderextra: 10   Witch: 20
added flat set piece at 109 371
   spawners along path:   Total: 59   Blaze:  4   CaveSpider: 33   Spider:  7   Spiderextra:  6   Witch:  9
added flat set piece at -696 -193
   spawners along path:   Total: 63   Blaze:  4   CaveSpider: 25   Spider:  9   Spiderextra:  7   Witch: 18
(this section took 1.097675 minutes)
-----
added  beacon at -539 52 -149 which travels 498
   spawners along path:   Total: 90   Creeper: 13   Skeleton: 15   Zombie: 62
added FINAL beacon at -695 55 -744 which travels 478
   spawners along path:   Total:156   CaveSpider: 18   Creeper: 22   Skeleton: 24   Witch: 22   Zombie: 70
added  beacon at -747 51 109 which travels 268
   spawners along path:   Total: 41   Creeper:  5   Skeleton:  5   Zombie: 31
added  beacon at -273 54 945 which travels 183
   spawners along path:   Total: 32   Creeper:  7   Skeleton:  5   Zombie: 20
added  beacon at -761 59 -632 which travels 265
   spawners along path:   Total: 38   Creeper:  7   Skeleton:  3   Zombie: 28
added  beacon at -794 32 741 which travels 122
   spawners along path:   Total: 18   Creeper:  2   Skeleton:  5   Zombie: 11
added  beacon at -514 60 665 which travels 241
   spawners along path:   Total: 40   Creeper:  4   Skeleton:  6   Zombie: 30
added  beacon at -534 59 -650 which travels 141
   spawners along path:   Total: 26   Creeper:  4   Skeleton:  1   Zombie: 21
added  beacon at -518 47 417 which travels 202
   spawners along path:   Total: 26   Creeper:  7   Skeleton:  2   Zombie: 17
added  beacon at -255 53 -24 which travels 175
   spawners along path:   Total: 25   Skeleton:  5   Zombie: 20
added  beacon at -362 20 -293 which travels 176
   spawners along path:   Total: 31   Creeper:  7   Skeleton:  2   Zombie: 22
added  beacon at -291 32 301 which travels 158
   spawners along path:   Total: 24   Skeleton:  3   Zombie: 21
added  beacon at -122 60 622 which travels 335
   spawners along path:   Total: 55   Creeper:  6   Skeleton:  4   Zombie: 45
added  beacon at -258 59 -408 which travels 482
   spawners along path:   Total: 76   Creeper: 10   Skeleton: 18   Zombie: 48
added  beacon at 217 51 -59 which travels 172
   spawners along path:   Total: 33   Creeper:  7   Skeleton:  3   Zombie: 23
added  beacon at 104 56 916 which travels 413
   spawners along path:   Total: 80   Creeper: 10   Skeleton: 12   Zombie: 58
added  beacon at -16 47 -626 which travels 116
   spawners along path:   Total: 21   Creeper:  1   Skeleton:  7   Zombie: 13
added  beacon at 304 53 -119 which travels 280
   spawners along path:   Total: 31   Creeper:  6   Skeleton:  8   Zombie: 17
added  beacon at 306 56 409 which travels 107
   spawners along path:   Total: 17   Creeper:  2   Skeleton:  2   Zombie: 13
added  beacon at 349 55 -618 which travels 286
   spawners along path:   Total: 42   Creeper:  3   Skeleton:  4   Zombie: 35
added  beacon at 402 36 227 which travels 470
   spawners along path:   Total: 79   Creeper:  9   Skeleton:  9   Zombie: 61
added  beacon at 846 57 -754 which travels 172
   spawners along path:   Total: 22   Creeper:  5   Skeleton:  2   Zombie: 15
added  beacon at 693 59 306 which travels 191
   spawners along path:   Total: 30   Creeper:  5   Skeleton:  4   Zombie: 21
added  beacon at 681 30 -447 which travels 315
   spawners along path:   Total: 55   Creeper:  6   Skeleton:  7   Zombie: 42
added  beacon at 878 53 -406 which travels 425
   spawners along path:   Total: 59   Creeper:  6   Skeleton:  8   Zombie: 45
added  beacon at 927 33 -628 which travels 174
   spawners along path:   Total: 24   Creeper:  3   Skeleton:  3   Zombie: 18
added  beacon at 864 60 -132 which travels 342
   spawners along path:   Total: 57   Creeper:  8   Skeleton:  9   Zombie: 40
found 3 reachable cul-de-sac rooms
(this section took 2.843439 minutes)
-----
('find best hiding spot' sub-section took 0.728736 minutes)
added mountain peak at -450 -168
   spawners along path:   Total: 66   Blaze:  5   CaveSpider: 16   Ghast:  9   Spiderextra: 13   Zombie: 23
added mountain peak at 388 732
   spawners along path:   Total: 59   Blaze:  5   CaveSpider: 23   Ghast:  4   Spiderextra: 11   Zombie: 16
added mountain peak at 988 938
   spawners along path:   Total: 68   Blaze:  4   CaveSpider: 26   Ghast:  2   Spiderextra: 15   Zombie: 21
added mountain peak at -984 984
   spawners along path:   Total: 68   Blaze:  6   CaveSpider: 23   Ghast:  7   Spiderextra: 13   Zombie: 19
added mountain peak at -516 -757
   spawners along path:   Total: 65   Blaze:  4   CaveSpider: 23   Ghast:  4   Spiderextra: 14   Zombie: 20
added mountain peak at -204 488
   spawners along path:   Total: 45   Blaze:  2   CaveSpider: 20   Ghast:  3   Spiderextra:  9   Zombie: 11
added mountain peak at -68 -581
   spawners along path:   Total: 55   Blaze:  3   CaveSpider: 17   Ghast:  4   Spiderextra: 15   Zombie: 16
added mountain peak at -820 -275
   spawners along path:   Total: 74   Blaze:  4   CaveSpider: 32   Ghast:  6   Spiderextra: 14   Zombie: 18
added mountain peak at 550 -595
   spawners along path:   Total: 62   Blaze:  1   CaveSpider: 24   Ghast:  2   Spiderextra: 16   Zombie: 19
added mountain peak at 514 551
   spawners along path:   Total: 67   Blaze:  3   CaveSpider: 21   Ghast:  2   Spiderextra: 19   Zombie: 22
(this section took 0.747477 minutes)
-----
highlighted 13 cave entrances near spawn
(this section took 0.019540 minutes)
-----
added 290 extra loot chests: 51, 142, 16, 60, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 14
(this section took 0.578063 minutes)
-----
Added 8 Hell biomes and 6 Sky biomes (replacing Plains)
(this section took 0.014112 minutes)
-----
START CMDS
(this section took 0.000838 minutes)
-----
SAVING FILES
(this section took 1.220618 minutes)
-----
WRITING MAP PNG IMAGES
(this section took 0.125847 minutes)
-----
Took 10.651594 total minutes



*)