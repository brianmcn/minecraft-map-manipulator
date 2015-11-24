module TerrainAnalysisAndManipulation

open NBT_Manipulation
open RegionFiles

    //map.SetBlockIDAndDamage(-342, 11, 97, 52uy, 0uy) // 52 = monster spawner
type MobSpawnerInfo() =
    member val RequiredPlayerRange =  16s with get, set
    member val SpawnCount          =   4s with get, set
    member val SpawnRange          =   4s with get, set
    member val MaxNearbyEntities   =   6s with get, set
    member val Delay               =  -1s with get, set
    member val MinSpawnDelay       = 200s with get, set
    member val MaxSpawnDelay       = 800s with get, set
    member val x = 0 with get, set 
    member val y = 0 with get, set 
    member val z = 0 with get, set 
    member val BasicMob = "Zombie" with get, set  // TODO multiple choice SpawnPotentials
    member val ExtraNbt = [] with get, set // Ex: skel jockey  Passengers:[{id:Skeleton,HandItems:[{id:bow,Count:1},{}]}]  ->    [ List("Passengers",Compounds[| [|String("id","Skeleton"); List("HandItems",Compounds[| [|String("id","bow");Int("Count",1);End|]; [| End |] |]); End|] |] )] )
    member this.AsNbtTileEntity() =
        [|
            Int("x", this.x)
            Int("y", this.y)
            Int("z", this.z)
            String("id","MobSpawner")
            Short("RequiredPlayerRange",this.RequiredPlayerRange)
            Short("SpawnCount",this.SpawnCount)
            Short("SpawnRange",this.SpawnRange)
            Short("MaxNearbyEntities",this.MaxNearbyEntities)
            Short("Delay",this.Delay)
            Short("MinSpawnDelay",this.MinSpawnDelay)
            Short("MaxSpawnDelay",this.MaxSpawnDelay)
            Compound("SpawnData",[|String("id",this.BasicMob);End|] |> ResizeArray)
            List("SpawnPotentials",Compounds[|
                                                [|
                                                Compound("Entity",[|yield String("id",this.BasicMob); yield! this.ExtraNbt; yield End|] |> ResizeArray)
                                                Int("Weight",1)
                                                End
                                                |]
                                            |])
            End
        |]

////////////////////////////////////////////

let spiderJockeyMSI(x,y,z) = MobSpawnerInfo(x=x, y=y, z=z, BasicMob="Spider", ExtraNbt=[ List("Passengers",Compounds[| [|String("id","Skeleton"); List("HandItems",Compounds[| [|String("id","bow");Int("Count",1);End|]; [| End |] |]); End|] |] )] )

let skeletonMSI(x,y,z) = MobSpawnerInfo(x=x, y=y, z=z, BasicMob="Skeleton", ExtraNbt=[ List("HandItems",Compounds[| [|String("id","bow");Int("Count",1);End|]; [| End |] |]) ] ) 

////////////////////////////////////////////


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

type Thingy(point:int, isLeft:bool, isRight:bool) =
    let mutable isLeft = isLeft
    let mutable isRight = isRight
    member this.Point = point
    member this.IsLeft with get() = isLeft and set(x) = isLeft <- x
    member this.IsRight with get() = isRight and set(x) = isRight <- x

// A partition is a mutable set of values, where one arbitrary value in the set 
// is chosen as the canonical representative for that set. 
[<AllowNullLiteral>]
type Partition(orig : Thingy) as this =  
    [<DefaultValue(false)>] val mutable parent : Partition
    [<DefaultValue(false)>] val mutable rank : int 
    let rec FindHelper(x : Partition) = 
        if System.Object.ReferenceEquals(x.parent, x) then 
            x 
        else 
            x.parent <- FindHelper(x.parent) 
            x.parent 
    do this.parent <- this 
    // The representative element in this partition 
    member this.Find() = 
        FindHelper(this) 
    // The original value of this element 
    member this.Value = orig 
    // Merges two partitions 
    member this.Union(other : Partition) = 
        let thisRoot = this.Find() 
        let otherRoot = other.Find() 
        if thisRoot.rank < otherRoot.rank then 
            otherRoot.parent <- thisRoot
            thisRoot.Value.IsLeft <- thisRoot.Value.IsLeft || otherRoot.Value.IsLeft 
            thisRoot.Value.IsRight <- thisRoot.Value.IsRight || otherRoot.Value.IsRight
        elif thisRoot.rank > otherRoot.rank then 
            thisRoot.parent <- otherRoot 
            otherRoot.Value.IsLeft <- otherRoot.Value.IsLeft || thisRoot.Value.IsLeft 
            otherRoot.Value.IsRight <- otherRoot.Value.IsRight || thisRoot.Value.IsRight
        elif not (System.Object.ReferenceEquals(thisRoot, otherRoot)) then 
            otherRoot.parent <- thisRoot 
            thisRoot.Value.IsLeft <- thisRoot.Value.IsLeft || otherRoot.Value.IsLeft 
            thisRoot.Value.IsRight <- thisRoot.Value.IsRight || otherRoot.Value.IsRight
            thisRoot.rank <- thisRoot.rank + 1 

let SPAWN_PROTECTION_DISTANCE = 200
let STRUCTURE_SPACING = 170

let putTreasureBoxAt(map:MapFolder,sx,sy,sz,lootTableName) =
    for x = sx-2 to sx+2 do
        for z = sz-2 to sz+2 do
            map.SetBlockIDAndDamage(x,sy,z,22uy,0uy)  // lapis block
            map.SetBlockIDAndDamage(x,sy+3,z,22uy,0uy)  // lapis block
    // want glowstone; to have Minecraft recompute the light, use a command block and a tile tick
    map.SetBlockIDAndDamage(sx,sy,sz,137uy,0uy)  // command block
    map.AddOrReplaceTileEntities([| [| Int("x",sx); Int("y",sy); Int("z",sz); String("id","Control"); Byte("auto",0uy); String("Command","setblock ~ ~ ~ glowstone"); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",1uy); End |] |])
    map.AddTileTick("minecraft:command_block",1,0,sx,sy,sz)
    for x = sx-2 to sx+2 do
        for y = sy+1 to sy+2 do
            for z = sz-2 to sz+2 do
                map.SetBlockIDAndDamage(x,y,z,20uy,0uy)  // glass
    map.SetBlockIDAndDamage(sx,sy+1,sz,54uy,2uy)  // chest
    map.AddOrReplaceTileEntities([| [| Int("x",sx); Int("y",sy+1); Int("z",sz); String("id","Chest"); List("Items",Compounds[| |]); String("LootTable",lootTableName); String("Lock",""); String("CustomName","Lootz!"); End |] |])

let putBeaconAt(map:MapFolder,ex,ey,ez,colorDamage) =
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

type SpawnerAccumulator() =
    let spawnerTileEntities = ResizeArray()
    let spawnerTypeCount = new System.Collections.Generic.Dictionary<_,_>()
    member this.Add(ms:MobSpawnerInfo) =
        let kind = ms.BasicMob + if ms.ExtraNbt.Length > 0 then "extra" else ""
        if spawnerTypeCount.ContainsKey(kind) then
            spawnerTypeCount.[kind] <- spawnerTypeCount.[kind] + 1
        else
            spawnerTypeCount.[kind] <- 1
        spawnerTileEntities.Add(ms.AsNbtTileEntity())
    member this.AddToMapAndLog(map:MapFolder, log:ResizeArray<_>) =
        map.AddOrReplaceTileEntities(spawnerTileEntities)
        let sb = new System.Text.StringBuilder()
        sb.Append(sprintf "   Total:%3d" (spawnerTypeCount |> Seq.sumBy (fun (KeyValue(_,v)) -> v))) |> ignore
        for KeyValue(k,v) in spawnerTypeCount |> Seq.sortBy (fun (KeyValue(k,_)) -> k)do
            sb.Append(sprintf "   %s:%3d" k v) |> ignore
        log.Add("   spawners along path:"+sb.ToString())

let MINIMUM = -1024
let LENGTH = 2048

let findUndergroundAirSpaceConnectedComponents(map:MapFolder, log:ResizeArray<_>, decorations:ResizeArray<_>) =
    let YMIN = 10
    let YLEN = 50
    let DIFFERENCES = [|1,0,0; 0,1,0; 0,0,1; -1,0,0; 0,-1,0; 0,0,-1|]
    let PT(x,y,z) = 
        let i,j,k = x-MINIMUM, y-YMIN, z-MINIMUM
        i*YLEN*LENGTH + k*YLEN + j
    let a = System.Array.CreateInstance(typeof<Partition>, [|LENGTH+2; YLEN+2; LENGTH+2|], [|MINIMUM; YMIN; MINIMUM|]) :?> Partition[,,] // +2s because we have sentinels guarding array index out of bounds
    let mutable currentSectionBlocks,curx,cury,curz = null,-1000,-1000,-1000
    // find all the air spaces in the underground
    printf "FIND"
    for y = YMIN+1 to YMIN+YLEN do
        printf "."
        for x = MINIMUM+1 to MINIMUM+LENGTH do
            for z = MINIMUM+1 to MINIMUM+LENGTH do
                if not(DIV(x,16) = DIV(curx,16) && DIV(y,16) = DIV(cury,16) && DIV(z,16) = DIV(curz,16)) then
                    currentSectionBlocks <- map.GetOrCreateSection(x,y,z) |> (fun (_sect,blocks,_bd) -> blocks)
                    curx <- x
                    cury <- y
                    curz <- z
                let dx, dy, dz = (x+51200) % 16, y % 16, (z+51200) % 16
                let bix = dy*256 + dz*16 + dx
                if currentSectionBlocks.[bix] = 0uy then // air
                    a.[x,y,z] <- new Partition(new Thingy(PT(x,y,z),(y=YMIN+1),(y>=map.GetHeightMap(x,z))))
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
                        else
                            goodCCs.[v.Point].Add(PT(x,y,z)) |> ignore
    printfn ""
    printfn "There are %d CCs with the desired property" goodCCs.Count 
    // These arrays are large enough that I think they get pinned in permanent memory, reuse them
    let dist = System.Array.CreateInstance(typeof<int>, [|LENGTH+2; YLEN+2; LENGTH+2|], [|MINIMUM; YMIN; MINIMUM|]) :?> int[,,] // +2: don't need sentinels here, but easier to keep indexes in lock-step with other array
    let prev = System.Array.CreateInstance(typeof<int>, [|LENGTH+2; YLEN+2; LENGTH+2|], [|MINIMUM; YMIN; MINIMUM|]) :?> int[,,] // +2: don't need sentinels here, but easier to keep indexes in lock-step with other array
    for hs in goodCCs.Values do
        let XYZP(pt) =
            let i = pt / (YLEN*LENGTH)
            let k = (pt % (YLEN*LENGTH)) / YLEN
            let j = pt % YLEN
            (i + MINIMUM, j + YMIN, k + MINIMUM)
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
                if (y>=map.GetHeightMap(x,z)) then // surface
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
                (ex > -SPAWN_PROTECTION_DISTANCE && ex < SPAWN_PROTECTION_DISTANCE && ez > -SPAWN_PROTECTION_DISTANCE && ez < SPAWN_PROTECTION_DISTANCE)  then
                () // skip if too close to 0,0 or to map bounds
            else
            printfn "(%d,%d,%d) is %d blocks from (%d,%d,%d)" sx sy sz dist.[besti,bestj,bestk] ex ey ez
            if dist.[besti,bestj,bestk] > 100 && dist.[besti,bestj,bestk] < 500 then  // only keep mid-sized ones...
                log.Add(sprintf "added beacon at %d %d %d which travels %d" ex ey ez dist.[besti,bestj,bestk])
                decorations.Add('B',ex,ez)
                let mutable i,j,k = besti,bestj,bestk
                let fullDist = dist.[besti,bestj,bestk]
                let mutable count = 0
                let spawners = SpawnerAccumulator()
                let rng = System.Random()
                let possibleSpawners = [|(5,"Zombie"); (1,"Skeleton"); (1,"Creeper")|] |> Array.collect (fun (n,k) -> Array.replicate n k)
                while i<>bi || j<>bj || k<>bk do
                    let ni, nj, nk = // next points (step back using info from 'prev')
                        let dx,dy,dz = DIFFERENCES.[prev.[i,j,k]]
                        i-dx,j-dy,k-dz
                    let ii,jj,kk = prev.[i,j,k]%3<>0, prev.[i,j,k]%3<>1, prev.[i,j,k]%3<>2   // ii/jj/kk track 'normal' to the path
                    // maybe put mob spawner nearby
                    let pct = float count / (float fullDist * 3.0)
                    if rng.NextDouble() < pct then
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
                                let kind = possibleSpawners.[rng.Next(possibleSpawners.Length)]
                                let ms = MobSpawnerInfo(x=x, y=y, z=z, BasicMob=kind)
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
                putBeaconAt(map,ex,ey,ez,5uy) // 5 = lime
                map.SetBlockIDAndDamage(ex,ey+1,ez,130uy,2uy) // ender chest
                // put treasure at bottom end
                putTreasureBoxAt(map,sx,sy,sz,sprintf "%s:chests/tier3" LootTables.LOOT_NS_PREFIX)
    // end foreach CC
    ()

////
(* MAP DEFAULTS 
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

let substituteBlocks(map:MapFolder, log:ResizeArray<_>) =
    let LOX, LOY, LOZ = MINIMUM, 1, MINIMUM
    let HIY = 120
    let spawners1 = SpawnerAccumulator()
    let spawners2 = SpawnerAccumulator()
    let rng = System.Random()
    let possibleSpawners1 = [|(5,"Zombie"); (5,"Skeleton"); (5,"Spider"); (1,"Blaze"); (1,"Creeper")|] |> Array.collect (fun (n,k) -> Array.replicate n k)
    let spawner1(x,y,z) =
        let kind = possibleSpawners1.[rng.Next(possibleSpawners1.Length)]
        let ms = MobSpawnerInfo(x=x, y=y, z=z, BasicMob=kind, MaxSpawnDelay=400s)
        spawners1.Add(ms)
    let possibleSpawners2 = [|(1,"Zombie"); (1,"Skeleton"); (1,"Spider"); (1,"Blaze"); (1,"Creeper"); (1,"CaveSpider")|] |> Array.collect (fun (n,k) -> Array.replicate n k)
    let spawner2(x,y,z) =
        let kind = possibleSpawners2.[rng.Next(possibleSpawners2.Length)]
        let ms = MobSpawnerInfo(x=x, y=y, z=z, BasicMob=kind, MaxSpawnDelay=400s)
        spawners2.Add(ms)
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
                            spawner1(x,y,z)
                        else
                            map.SetBlockIDAndDamage(x,y,z,1uy,5uy) // andesite
                    elif bid = 73uy && dmg = 0uy then // redstone ore ->
                        if canPlaceSpawner(map,x,y,z) then
                            map.SetBlockIDAndDamage(x,y,z,52uy,0uy) // mob spawner
                            spawner2(x,y,z)
                        else
                            map.SetBlockIDAndDamage(x,y,z,1uy,5uy) // andesite
                    elif bid = 16uy && dmg = 0uy then // coal ore ->
                        if rng.Next(20) = 0 then
                            map.SetBlockIDAndDamage(x,y,z,173uy,0uy) // coal block
    log.Add("added random spawners underground")
    spawners1.AddToMapAndLog(map,log)
    spawners2.AddToMapAndLog(map,log)
    printfn ""

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

let findBestPeaksAlgorithm(heightMap:_[,], connectedThreshold, goodThreshold, bestNearbyDist) =
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
    // pick highest in each CC
    for hs in CCs.Values do
        let p = hs |> Seq.maxBy (fun (x,z) -> heightMap.[x,z])
        let minx = hs |> Seq.minBy fst |> fst
        let maxx = hs |> Seq.maxBy fst |> fst
        let minz = hs |> Seq.minBy snd |> snd
        let maxz = hs |> Seq.maxBy snd |> snd
        highPoints.Add(p,(minx,minz),(maxx,maxz))  // retain the bounds of the CC
    // find the 'best' ones based on which have lots of high ground near them
    let score(x,z) =
        try
            let mutable s = 0
            let D = bestNearbyDist
            for a = x-D to x+D do
                for b = z-D to z+D do
                    s <- s + heightMap.[a,b] - (heightMap.[x,z]-20)  // want high ground nearby, but not a huge narrow spike above moderatly high ground // TODO will this screw up 'flat' algorithm?
            s
        with _ -> 0  // deal with array index out of bounds
    let distance2(a,b,c,d) = (a-c)*(a-c)+(b-d)*(b-d)
    let bestHighPoints = ResizeArray()
    for ((hx,hz),a,b) in highPoints |> Seq.sortByDescending (fun (p,_,_) -> score p) do
        if hx > MINIMUM+32 && hx < MINIMUM+LENGTH-32 && hz > MINIMUM+32 && hz < MINIMUM+LENGTH-32 then  // not at edge of bounds
            if bestHighPoints |> Seq.forall (fun ((ex,ez),_,_,_s) -> distance2(ex,ez,hx,hz) > STRUCTURE_SPACING*STRUCTURE_SPACING) then   // spaced apart TODO factor constants
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
    if found then
        Some(fx,fy,fz)
    else
        None

let mutable hiddenX = 0
let mutable hiddenZ = 0

let findSomeMountainPeaks(map:MapFolder,hm, log:ResizeArray<_>, decorations:ResizeArray<_>) =
    let bestHighPoints = findBestPeaksAlgorithm(hm,80,100,3)
    let unused = bestHighPoints |> Seq.filter (fun ((x,z),_,_,_s) -> x*x+z*z <= STRUCTURE_SPACING*STRUCTURE_SPACING)
    let bestHighPoints = bestHighPoints |> Seq.filter (fun ((x,z),_,_,_s) -> x*x+z*z > STRUCTURE_SPACING*STRUCTURE_SPACING)
    let otherUnused = (bestHighPoints |> Seq.toArray).[10..]
    // best hiding spot
    let timer = System.Diagnostics.Stopwatch.StartNew()
    printfn "find best hiding spot..."
    let (bx,by,bz) = Seq.append unused otherUnused |> Seq.choose (fun x -> findHidingSpot(map,hm,x)) |> Seq.maxBy (fun (_,y,_) -> y)
    printfn "best hiding spot: %4d %4d %4d" bx by bz
    decorations.Add('H',bx,bz)
    hiddenX <- bx
    hiddenZ <- bz
    log.Add(sprintf "('find best hiding spot' sub-section took %f minutes)" timer.Elapsed.TotalMinutes)
    for dx = -1 to 1 do
        for dy = -1 to 1 do
            for dz = -1 to 1 do
                map.SetBlockIDAndDamage(bx+dx,by+dy,bz+dz,20uy,0uy)  // glass
    let chestItems = 
        Compounds[| 
                [| Byte("Count",1uy); Byte("Slot",13uy); Short("Damage",0s); String("id","minecraft:elytra"); End |]
                // TODO couple mending books, what else?
                [| Byte("Count",1uy); Byte("Slot",22uy); Short("Damage",0s); String("id","minecraft:written_book"); 
                   Compound("tag", Utilities.makeWrittenBookTags("Lorgon111","You win!", 
                                                                 [| 
                                                                    """{"text":"TODO text of book"}"""
                                                                    """{"text":"hope enjoyed"}"""
                                                                    """{"text":"feedback"}"""
                                                                    """{"text":"donate"}"""  // TODO timing, they may not want to read now, may want to play with elytra? monument block? ...
                                                                 |]) |> ResizeArray
                           )
                   End |]
            |]
    map.SetBlockIDAndDamage(bx,by,bz,54uy,2uy)  // chest
    map.AddOrReplaceTileEntities([| [| Int("x",bx); Int("y",by); Int("z",bz); String("id","Chest"); List("Items",chestItems); String("Lock",""); String("CustomName","Winner!"); End |] |])
    // mountain peaks
    let bestHighPoints = try Seq.take 10 bestHighPoints with _e -> bestHighPoints
    printfn "The best high points are:"
    for (x,z),_,_,s in bestHighPoints do
        printfn "  (%4d,%4d) - %d" x z s
        decorations.Add('P',x,z)
    // decorate map with dungeon ascent
    let rng = System.Random()
    for (x,z),_,_,_s in bestHighPoints do
        log.Add(sprintf "added mountain peak at %d %d" x z)
        let spawners = SpawnerAccumulator()
        let y = map.GetHeightMap(x,z)
        putTreasureBoxAt(map,x,y,z,sprintf "%s:chests/tier5" LootTables.LOOT_NS_PREFIX)   // TODO heightmap, blocklight, skylight
        for i = x-20 to x+20 do
            for j = z-20 to z+20 do
                if abs(x-i) > 2 || abs(z-j) > 2 then
                    let dist = abs(x-i) + abs(z-j)
                    let pct = float (40-dist) / 200.0
                    if rng.NextDouble() < pct then
                        let x = i
                        let z = j
                        let y = map.GetHeightMap(x,z)
                        map.SetBlockIDAndDamage(x, y, z, 52uy, 0uy) // 52 = monster spawner   // TODO heightmap, blocklight, skylight
                        let possibleSpawners = [|(5,"Spider"); (1,"CaveSpider"); (1,"Blaze"); (1,"Ghast")|] |> Array.collect (fun (n,k) -> Array.replicate n k)
                        let kind = possibleSpawners.[rng.Next(possibleSpawners.Length)]
                        let ms = MobSpawnerInfo(x=x, y=y, z=z, BasicMob=kind, Delay=1s)
                        if ms.BasicMob = "Spider" then
                            ms.ExtraNbt <- [ List("Passengers",Compounds[| [|String("id","Skeleton"); List("HandItems",Compounds[| [|String("id","bow");Int("Count",1);End|]; [| End |] |]); End|] |] )]
                        spawners.Add(ms)
        spawners.AddToMapAndLog(map,log)
    ()

let findSomeFlatAreas(map:MapFolder,hm:_[,],log:ResizeArray<_>, decorations:ResizeArray<_>) =
    // convert height map to 'goodness' function that looks for similar-height blocks nearby
    // then treat 'goodness' as 'height', and the existing 'find mountain peaks' algorithm may work
    let a = Array2D.zeroCreateBased MINIMUM MINIMUM LENGTH LENGTH
    let fScores = [| 100; 90; 75; 50; 0; -100; -999 |]
    let f(h1,h2) =
        let diff = abs(h1-h2)
        fScores.[min diff (fScores.Length-1)]
    let D = 10
    printfn "PREP FLAT MAP..."
    for x = MINIMUM+D to MINIMUM+LENGTH-1-D do
        for z = MINIMUM+D to MINIMUM+LENGTH-1-D do
            let h = if hm.[x,z] > 65 && hm.[x,z] < 90 then hm.[x,z] else 255  // only pick points above sea level but not too high
            let mutable score = 0
            for dx = -D to D do
                for dz = -D to D do
                    let ds = f(h,hm.[x+dx,z+dz])
                    score <- score + ds
            a.[x,z] <- score
    let bestFlatPoints = findBestPeaksAlgorithm(a,2000,3000,D)
    let bestFlatPoints = bestFlatPoints |> Seq.filter (fun ((x,z),_,_,_s) -> x*x+z*z > SPAWN_PROTECTION_DISTANCE*SPAWN_PROTECTION_DISTANCE)
    let bestFlatPoints = try Seq.take 10 bestFlatPoints with _e -> bestFlatPoints
    printfn "The best flat points are:"
    let chosen = ResizeArray()
    for (x,z),_,_,s in bestFlatPoints do
        printfn "  (%4d,%4d) - %d" x z s
        chosen.Add( (x,z) )
        decorations.Add('F',x,z)
    let bestFlatPoints = chosen
    // decorate map with dungeon
    let rng = System.Random()
    for (x,z) in bestFlatPoints do
        log.Add(sprintf "added flat set piece at %d %d" x z)
        let spawners = SpawnerAccumulator()
        let y = map.GetHeightMap(x,z)
        putTreasureBoxAt(map,x,y,z,sprintf "%s:chests/tier4" LootTables.LOOT_NS_PREFIX)   // TODO heightmap, blocklight, skylight
        putBeaconAt(map,x,y,z,14uy) // 14 = red
        map.SetBlockIDAndDamage(x,y+3,z,20uy,0uy) // glass (replace roof of box so beacon works)
        // add blazes atop
        for (dx,dz) in [-3,-3; -3,3; 3,-3; 3,3; 0,0] do
            let x,y,z = x+dx, y+6, z+dz
            map.SetBlockIDAndDamage(x, y, z, 52uy, 0uy) // 52 = monster spawner   // TODO heightmap, blocklight, skylight
            let ms = MobSpawnerInfo(x=x, y=y, z=z, BasicMob="Blaze", Delay=1s)
            spawners.Add(ms)
        // surround with danger
        let RADIUS = 40
        for i = x-RADIUS to x+RADIUS do
            for j = z-RADIUS to z+RADIUS do
                if abs(x-i) > 2 || abs(z-j) > 2 then
                    let dist = (x-i)*(x-i) + (z-j)*(z-j) |> float |> sqrt |> int
                    let pct = float (RADIUS-dist/2) / ((float RADIUS) * 2.0)
                    if rng.NextDouble() < pct then
                        let x = i
                        let z = j
                        let y = map.GetHeightMap(x,z) + rng.Next(2)
                        if rng.Next(8+dist) = 0 then
                            map.SetBlockIDAndDamage(x, y, z, 52uy, 0uy) // 52 = monster spawner   // TODO heightmap, blocklight, skylight
                            let possibleSpawners = [|(1,"Spider"); (1,"Witch"); (2,"CaveSpider")|] |> Array.collect (fun (n,k) -> Array.replicate n k)
                            let ms = MobSpawnerInfo(x=x, y=y, z=z, BasicMob=possibleSpawners.[rng.Next(possibleSpawners.Length)], Delay=1s)
                            if ms.BasicMob = "Spider" && rng.Next(2) = 0 then
                                ms.ExtraNbt <- [ List("Passengers",Compounds[| [|String("id","Skeleton"); List("HandItems",Compounds[| [|String("id","bow");Int("Count",1);End|]; [| End |] |]); End|] |] )]
                            spawners.Add(ms)
                        elif rng.Next(3) > 0 then
                            map.SetBlockIDAndDamage(x, y, z, 30uy, 0uy) // 30 = cobweb
        spawners.AddToMapAndLog(map,log)
    ()

let doubleSpawners(map:MapFolder,log:ResizeArray<_>) =
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
                    let kind =
                        match bite.Value with
                        | Compound(_,cs) ->
                            match cs |> Seq.find (fun x -> x.Name = "SpawnData") with
                            | Compound(_,sd) -> sd |> Seq.find (fun x -> x.Name = "id") |> (fun (String("id",k)) -> k)
                    map.SetBlockIDAndDamage(x, y+1, z, 52uy, 0uy) // 52 = monster spawner
                    let ms = MobSpawnerInfo(x=x, y=y+1, z=z, BasicMob=(if kind = "Spider" || kind = "CaveSpider" then "Skeleton" else "CaveSpider"), 
                                            Delay=1s, // primed
                                            MinSpawnDelay=200s, MaxSpawnDelay=400s, // 10-20s, rather than 10-40s
                                            ExtraNbt=[ if kind = "CaveSpider" then 
                                                            yield List("HandItems",Compounds[| [|String("id","bow");Int("Count",1);End|]; [| End |] |]) ] ) 
                    spawnerTileEntities.Add(ms.AsNbtTileEntity())
    map.AddOrReplaceTileEntities(spawnerTileEntities)
    log.Add(sprintf "added %d extra dungeon spawners underground" spawnerTileEntities.Count)


let placeStartingCommands(map:MapFolder,hm:_[,]) =
    let placeCommand(x,y,z,command,bid,name) =
        map.SetBlockIDAndDamage(x,y,z,bid,0uy)  // command block
        map.AddOrReplaceTileEntities([| [| Int("x",x); Int("y",y); Int("z",z); String("id","Control"); Byte("auto",1uy); String("Command",command); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); End |] |])
        if bid <> 211uy then
            map.AddTileTick(name,1,0,x,y,z)  // TODO race, the gives sometimes run be4ore player online, change t to a value like 400 to run a4ter online?
    let placeImpulse(x,y,z,command) = placeCommand(x,y,z,command,137uy,"minecraft:command_block")
    let placeRepeating(x,y,z,command) = placeCommand(x,y,z,command,210uy,"minecraft:repeating_command_block")
    //let placeChain(x,y,z,command) = placeCommand(x,y,z,command,211uy,"minecraft:chain_command_block")
    let y = ref 255
    let R(c) = placeRepeating(0,!y,0,c); decr y
    let I(c) = placeImpulse(0,!y,0,c); decr y
    //let C(c) = placeChain(0,!y,0,c); decr y
    let DR = 180 // daylight radius
    for i = 0 to 99 do
        let theta = System.Math.PI * 2.0 * float i / 100.0
        let x = cos theta * float DR |> int
        let z = sin theta * float DR |> int
        let h = hm.[x,z] + 5
        if h > 60 then
            for y = 60 to h do
                map.SetBlockIDAndDamage(x,y,z,1uy,3uy)  // diorite
    R(sprintf "execute @p[r=%d,x=0,y=80,z=0] ~ ~ ~ time set 1000" DR)  // TODO multiplayer?
    R(sprintf "execute @p[rm=%d,x=0,y=80,z=0] ~ ~ ~ time set 14500" DR)
    // TODO first time enter night, give a message explaining
    I("worldborder set 2048")
    I("gamerule doDaylightCycle false")
    I("gamerule keepInventory true")  // TODO get rid of?
    I("weather clear 999999")
    I("give @a iron_axe")
    I("give @a shield")
    I("give @a cooked_beef 6")
    I("give @a dirt 64")
    I("scoreboard objectives add LavaSlimesKilled stat.killEntity.LavaSlime")
    I("scoreboard players set @a LavaSlimesKilled 0") // TODO multiplayer, what if A kills #1 and B kills #2
    I("scoreboard objectives add hidden dummy")
    I(sprintf "scoreboard players set X hidden %d" hiddenX)
    I(sprintf "scoreboard players set Z hidden %d" hiddenZ)
    let h = hm.[1,1]
    putBeaconAt(map,1,h,1,0uy)  // beacon at spawn for convenience

let makeCrazyMap(worldSaveFolder) =
    let timer = System.Diagnostics.Stopwatch.StartNew()
    let map = new MapFolder(worldSaveFolder + """\region\""")
    let log = ResizeArray()
    let decorations = ResizeArray()
    let hm = Array2D.zeroCreateBased MINIMUM MINIMUM LENGTH LENGTH
    let xtime _ = 
        printfn "SKIPPING SOMETHING"
        log.Add("SKIPPED SOMETHING")
    let time f =
        let timer = System.Diagnostics.Stopwatch.StartNew()
        f()
        log.Add(sprintf "(this section took %f minutes)" timer.Elapsed.TotalMinutes)
        log.Add("-----")
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
        printfn "CACHE HM..."
        log.Add("CACHE HM...")
        for x = MINIMUM to MINIMUM+LENGTH-1 do
            if x%200 = 0 then
                printfn "%d" x
            for z = MINIMUM to MINIMUM+LENGTH-1 do
                let bi = map.GetBlockInfo(x,0,z) // caches height map as side effect
                let h = map.GetHeightMap(x,z)
                hm.[x,z] <- h
        )
    xtime (fun () -> doubleSpawners(map, log))
    xtime (fun () -> substituteBlocks(map, log))
    xtime (fun() ->   // after substitute blocks, to keep diorite in pillars
        printfn "START CMDS"
        log.Add("START CMDS")
        placeStartingCommands(map,hm))
    xtime (fun () -> findSomeMountainPeaks(map, hm, log, decorations))
    xtime (fun () -> findSomeFlatAreas(map, hm, log, decorations))
    time (fun () -> findUndergroundAirSpaceConnectedComponents(map, log, decorations))
    printfn "saving results..."
    map.WriteAll()
    printfn "...done!"
    time (fun() -> 
        printfn "WRITING MAP PNG IMAGES"
        log.Add("WRITING MAP PNG IMAGES")
        Utilities.makeBiomeMap(worldSaveFolder+"""\region""", [-2..1], [-2..1],decorations)
        System.IO.File.WriteAllLines(System.IO.Path.Combine(worldSaveFolder,"summary.txt"),log)
        )

    printfn ""
    printfn "SUMMARY"
    printfn ""
    for s in log do
        printfn "%s" s   // TODO write to text file
    printfn "Took %f minutes" timer.Elapsed.TotalMinutes 
    printfn "press a key to end"
    System.Console.ReadKey() |> ignore


//works:
// setblock ~ ~5 ~ mob_spawner 0 replace {SpawnPotentials:[{Entity:{id:Zombie},Weight:1,Properties:[]}],SpawnData:{id:Ghast},Delay:-1s,MaxNearbyEntities:6s,SpawnCount:4s,SpawnRange:4s,RequiredPlayerRange:16s,MinSpawnDelay:200s,MaxSpawnDelay:800s}
// hmm:
// setblock ~ ~5 ~ mob_spawner 0 replace {SpawnPotentials:[{Type:{id:Spider},Weight:1,Properties:[{id:Spider,Passengers:[{"id":"Skeleton"}]}]}],SpawnData:{id:Ghast},Delay:-1s,MaxNearbyEntities:6s,SpawnCount:4s,SpawnRange:4s,RequiredPlayerRange:16s,MinSpawnDelay:200s,MaxSpawnDelay:800s}
// setblock ~ ~5 ~ mob_spawner 0 replace {SpawnPotentials:[{Type:Spider,Weight:1,Properties:[{Passengers:[{"id":"Skeleton"}]}]}],SpawnData:{id:Ghast},Delay:-1s,MaxNearbyEntities:6s,SpawnCount:4s,SpawnRange:4s,RequiredPlayerRange:16s,MinSpawnDelay:200s,MaxSpawnDelay:800s}
// setblock ~ ~5 ~ mob_spawner 0 replace {SpawnPotentials:[{Type:Spider,Weight:1,Properties:[{id:Spider,Passengers:[{"id":"Skeleton"}]}]}],SpawnData:{id:Ghast},Delay:-1s,MaxNearbyEntities:6s,SpawnCount:4s,SpawnRange:4s,RequiredPlayerRange:16s,MinSpawnDelay:200s,MaxSpawnDelay:800s}

// setblock ~ ~5 ~ mob_spawner 0 replace {SpawnPotentials:[{Entity:{id:Spider,Passengers:[{"id":"Skeleton"}]},Weight:1}],SpawnData:{id:Ghast},Delay:-1s,MaxNearbyEntities:6s,SpawnCount:4s,SpawnRange:4s,RequiredPlayerRange:16s,MinSpawnDelay:200s,MaxSpawnDelay:800s}

//works:
// setblock ~ ~5 ~ mob_spawner 0 replace {SpawnPotentials:[{Entity:{id:Spider,Passengers:[{id:Skeleton}]},Weight:1}],SpawnData:{id:Ghast},Delay:-1s,MaxNearbyEntities:6s,SpawnCount:4s,SpawnRange:4s,RequiredPlayerRange:16s,MinSpawnDelay:200s,MaxSpawnDelay:800s}
// setblock ~ ~5 ~ mob_spawner 0 replace {SpawnPotentials:[{Entity:{id:Spider,Passengers:[{id:Skeleton}]},Weight:1}],Delay:-1s,MaxNearbyEntities:6s,SpawnCount:4s,SpawnRange:4s,RequiredPlayerRange:16s,MinSpawnDelay:200s,MaxSpawnDelay:800s}
// with bow
// setblock ~ ~5 ~ mob_spawner 0 replace {SpawnPotentials:[{Entity:{id:Spider,Passengers:[{id:Skeleton,HandItems:[{id:bow,Count:1},{}]}]},Weight:1}],Delay:-1s,MaxNearbyEntities:6s,SpawnCount:4s,SpawnRange:4s,RequiredPlayerRange:16s,MinSpawnDelay:200s,MaxSpawnDelay:800s}



// {MaxNearbyEntities:6s,RequiredPlayerRange:16s,SpawnCount:4s,SpawnData:{id:"Skeleton"},MaxSpawnDelay:800s,Delay:329s,x:99977,y:39,z:-24,id:"MobSpawner",SpawnRange:4s,MinSpawnDelay:200s,SpawnPotentials:[0:{Entity:{id:"Skeleton"},Weight:1}]}





(*


SUMMARY

(this section took 0.182908 minutes)
-----
CACHE HM...
(this section took 0.027018 minutes)
-----
added 720 extra dungeon spawners underground
(this section took 1.425156 minutes)
-----
added 3359 random spawners underground
   s1:   Zombie:712   Spider:699   Skeleton:646   Creeper:133   Blaze:134
   s2:   Skeleton:202   Creeper:169   Blaze:166   CaveSpider:174   Spider:169   Zombie:155
(this section took 2.199976 minutes)
-----
added mountain peak at 400 124
added mountain peak at 830 728
added mountain peak at -368 -933
added mountain peak at 116 196
added mountain peak at 120 440
added mountain peak at -743 -371
added mountain peak at -451 680
added mountain peak at -513 -89
added mountain peak at -386 -288
added mountain peak at -9 755
(this section took 0.018029 minutes)
-----
added flat set piece at 789 -690
added flat set piece at -231 289
added flat set piece at 445 -481
added flat set piece at 147 624
added flat set piece at -499 717
added flat set piece at -977 -710
added flat set piece at -893 -152
added flat set piece at -879 192
added flat set piece at -625 -272
added flat set piece at 458 550
(this section took 1.062523 minutes)
-----
added beacon at -885 25 -559 which travels 268
   spawners along path:   Zombie:49   Skeleton:11   Creeper:19
added beacon at -556 44 -191 which travels 232
   spawners along path:   Zombie:53   Creeper:6   Skeleton:6
added beacon at -430 48 195 which travels 221
   spawners along path:   Zombie:50   Skeleton:15   Creeper:14
added beacon at -535 47 716 which travels 129
   spawners along path:   Zombie:35   Creeper:6   Skeleton:8
added beacon at -351 46 -391 which travels 189
   spawners along path:   Zombie:44   Skeleton:11   Creeper:10
added beacon at -97 44 774 which travels 190
   spawners along path:   Zombie:36   Skeleton:9   Creeper:6
added beacon at -22 44 -879 which travels 204
   spawners along path:   Zombie:53   Skeleton:9   Creeper:8
added beacon at 192 44 954 which travels 161
   spawners along path:   Zombie:26   Creeper:4   Skeleton:7
added beacon at 218 59 516 which travels 248
   spawners along path:   Zombie:56   Skeleton:15   Creeper:10
added beacon at 525 50 253 which travels 111
   spawners along path:   Zombie:33   Skeleton:4   Creeper:2
added beacon at 827 37 -692 which travels 297
   spawners along path:   Zombie:67   Creeper:15   Skeleton:14
added beacon at 885 53 170 which travels 226
   spawners along path:   Zombie:55   Creeper:10   Skeleton:7
(this section took 2.062952 minutes)
-----
Took 7.971575 minutes
press a key to end




SUMMARY

(this section took 0.183469 minutes)
-----
CACHE HM...
(this section took 0.026647 minutes)
-----
START CMDS
(this section took 0.000312 minutes)
-----
added 720 extra dungeon spawners underground
(this section took 1.454130 minutes)
-----
added 3359 random spawners underground
   s1:   Zombie:701   Blaze:143   Skeleton:684   Spider:674   Creeper:122
   s2:   Blaze:188   Skeleton:177   CaveSpider:171   Spider:161   Creeper:162   Zombie:176
(this section took 2.153262 minutes)
-----
('find best hiding spot' sub-section took 0.549599 minutes)
added mountain peak at 400 124
added mountain peak at 830 728
added mountain peak at -368 -933
added mountain peak at 116 196
added mountain peak at 120 440
added mountain peak at -743 -371
added mountain peak at -451 680
added mountain peak at -513 -89
added mountain peak at -51 465
added mountain peak at -386 -288
(this section took 0.568549 minutes)
-----
added flat set piece at 789 -690
added flat set piece at -231 289
added flat set piece at 445 -481
added flat set piece at 147 624
added flat set piece at -499 717
added flat set piece at -977 -710
added flat set piece at -893 -152
added flat set piece at -879 192
added flat set piece at -625 -272
added flat set piece at -723 -116
(this section took 1.071351 minutes)
-----
added beacon at -885 25 -559 which travels 268
   spawners along path:   Zombie:54   Skeleton:14   Creeper:12
added beacon at -556 44 -191 which travels 232
   spawners along path:   Zombie:47   Creeper:8   Skeleton:8
added beacon at -430 48 195 which travels 221
   spawners along path:   Zombie:57   Skeleton:21   Creeper:7
added beacon at -535 47 716 which travels 129
   spawners along path:   Zombie:32   Skeleton:7   Creeper:5
added beacon at -351 46 -391 which travels 189
   spawners along path:   Zombie:48   Creeper:9   Skeleton:7
added beacon at -97 44 774 which travels 190
   spawners along path:   Skeleton:8   Zombie:49   Creeper:3
added beacon at -22 44 -879 which travels 204
   spawners along path:   Creeper:11   Zombie:47   Skeleton:10
added beacon at 192 44 954 which travels 161
   spawners along path:   Zombie:24   Creeper:4   Skeleton:9
added beacon at 218 59 516 which travels 248
   spawners along path:   Zombie:55   Creeper:14   Skeleton:6
added beacon at 525 50 253 which travels 111
   spawners along path:   Zombie:26   Skeleton:6   Creeper:2
added beacon at 827 37 -692 which travels 297
   spawners along path:   Zombie:73   Creeper:16   Skeleton:20
added beacon at 885 53 170 which travels 226
   spawners along path:   Zombie:52   Skeleton:12   Creeper:14
(this section took 1.997691 minutes)
-----
WRITING MAP PNG IMAGES
(this section took 0.350523 minutes)
-----
Took 8.818133 minutes

*)