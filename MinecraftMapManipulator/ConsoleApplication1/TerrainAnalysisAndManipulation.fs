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

let putTreasureBoxAt(map:MapFolder,sx,sy,sz) =
    for x = sx-2 to sx+2 do
        for z = sz-2 to sz+2 do
            map.SetBlockIDAndDamage(x,sy,z,22uy,0uy)  // lapis block
            map.SetBlockIDAndDamage(x,sy+3,z,22uy,0uy)  // lapis block
    //map.SetBlockIDAndDamage(sx,sy,sz,89uy,0uy)  // glowstone  // TODO does not glow, need to recompute light somehow
    // to have Minecraft recompute the light, use a command block and a tile tick
    map.SetBlockIDAndDamage(sx,sy,sz,137uy,0uy)  // command block
    map.AddOrReplaceTileEntities([| [| Int("x",sx); Int("y",sy); Int("z",sz); String("id","Control"); Byte("auto",0uy); String("Command","setblock ~ ~ ~ glowstone"); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",1uy); End |] |])
    map.AddTileTick("minecraft:command_block",1,0,sx,sy,sz)
    for x = sx-2 to sx+2 do
        for y = sy+1 to sy+2 do
            for z = sz-2 to sz+2 do
                map.SetBlockIDAndDamage(x,y,z,20uy,0uy)  // glass
    map.SetBlockIDAndDamage(sx,sy+1,sz,54uy,2uy)  // chest
    map.AddOrReplaceTileEntities([| [| Int("x",sx); Int("y",sy+1); Int("z",sz); String("id","Chest"); List("Items",Compounds[| |]); String("Lock",""); String("CustomName","Lootz!"); End |] |])


let findUndergroundAirSpaceConnectedComponents(map:MapFolder) =
    let LOX, LOY, LOZ = -512, 11, -512
    let MAXI, MAXJ, MAXK = 1024, 50, 1024
    let PT(i,j,k) = i*MAXJ*MAXK + k*MAXJ + j
    let a = Array3D.create (MAXI+2) (MAXJ+2) (MAXK+2) null   // +2s because we have sentinels guarding array index out of bounds
    let mutable currentSectionBlocks,curx,cury,curz = null,-1000,-1000,-1000
    let XYZ(i,j,k) =
        let x = i-1 + LOX
        let y = j-1 + LOY
        let z = k-1 + LOZ
        x,y,z
    // find all the air spaces in the underground
    printf "FIND"
    for j = 1 to MAXJ do
        printf "."
        for i = 1 to MAXI do
            for k = 1 to MAXK do
                let x,y,z = XYZ(i,j,k)
                if not(DIV(x,16) = DIV(curx,16) && DIV(y,16) = DIV(cury,16) && DIV(z,16) = DIV(curz,16)) then
                    currentSectionBlocks <- map.GetOrCreateSection(x,y,z) |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
                    curx <- x
                    cury <- y
                    curz <- z
                let dx, dy, dz = (x+51200) % 16, y % 16, (z+51200) % 16
                let bix = dy*256 + dz*16 + dx
                if currentSectionBlocks.[bix] = 0uy then // air
                    //a.[i,j,k] <- new Partition(new Thingy(PT(i,j,k),(j=1),(j=MAXJ)))
                    a.[i,j,k] <- new Partition(new Thingy(PT(i,j,k),(j=1),(y>=map.GetHeightMap(x,z))))
    printfn ""
    printf "CONNECT"
    // connected-components them
    for j = 1 to MAXJ-1 do
        printf "."
        for i = 1 to MAXI-1 do
            for k = 1 to MAXK-1 do
                if a.[i,j,k]<>null && a.[i+1,j,k]<>null then
                    a.[i,j,k].Union(a.[i+1,j,k])
                if a.[i,j,k]<>null && a.[i,j+1,k]<>null then
                    a.[i,j,k].Union(a.[i,j+1,k])
                if a.[i,j,k]<>null && a.[i,j,k+1]<>null then
                    a.[i,j,k].Union(a.[i,j,k+1])
    printfn ""
    printf "ANALYZE"
    // look for 'good' ones
    let goodCCs = new System.Collections.Generic.Dictionary<_,_>()
    for j = 1 to MAXJ do
        printf "."
        for i = 1 to MAXI do
            for k = 1 to MAXK do
                if a.[i,j,k]<>null then
                    let v = a.[i,j,k].Find().Value 
                    if v.IsLeft && v.IsRight then
                        if not(goodCCs.ContainsKey(v.Point)) then
                            goodCCs.Add(v.Point, new System.Collections.Generic.HashSet<_>())
                        else
                            goodCCs.[v.Point].Add(PT(i,j,k)) |> ignore
    printfn ""
    printfn "There are %d CCs with the desired property" goodCCs.Count 
    for hs in goodCCs.Values do
        let XYZP(pt) =
            let i = pt / (MAXJ*MAXK)
            let k = (pt % (MAXJ*MAXK)) / MAXJ
            let j = pt % MAXJ
            XYZ(i,j,k)
        let IJK(x,y,z) =
            let i = x+1 - LOX
            let j = y+1 - LOY
            let k = z+1 - LOZ
            i,j,k
        let mutable bestX,bestY,bestZ = 0,0,0
        for p in hs do
            let x,y,z = XYZP(p)
            if y > bestY then
                bestX <- x
                bestY <- y
                bestZ <- z
        // have a point at the top of the CC, now find furthest low point away (Dijkstra variant)
        let dist = Array3D.create (MAXI+2) (MAXJ+2) (MAXK+2) 999999   // +2: don't need sentinels here, but easier to keep indexes in lock-step with other array
        let prev = Array3D.create (MAXI+2) (MAXJ+2) (MAXK+2) (0,0,0)  // +2: don't need sentinels here, but easier to keep indexes in lock-step with other array
        let q = new System.Collections.Generic.Queue<_>()
        let bi,bj,bk = IJK(bestX,bestY,bestZ)
        q.Enqueue(bi,bj,bk)
        dist.[bi,bj,bk] <- 0
        let mutable besti,bestj,bestk = bi, bj, bk
        while q.Count > 0 do
            let i,j,k = q.Dequeue()
            let d = dist.[i,j,k]
            for di,dj,dk in [1,0,0; 0,1,0; 0,0,1; -1,0,0; 0,-1,0; 0,0,-1] do
                if a.[i+di,j+dj,k+dk]<>null && dist.[i+di,j+dj,k+dk] > d+1 then
                    dist.[i+di,j+dj,k+dk] <- d+1  // TODO bias to walls
                    prev.[i+di,j+dj,k+dk] <- (i,j,k)
                    q.Enqueue(i+di,j+dj,k+dk)
                    if j = 1 then  // low point
                        if dist.[besti,bestj,bestk] < d+1 then
                            besti <- i+di
                            bestj <- j+dj
                            bestk <- k+dk
        // now find shortest from that bottom to top
        let dist = Array3D.create (MAXI+2) (MAXJ+2) (MAXK+2) 999999   // +2: don't need sentinels here, but easier to keep indexes in lock-step with other array
        let prev = Array3D.create (MAXI+2) (MAXJ+2) (MAXK+2) (0,0,0,false,false,false)  // +2: don't need sentinels here, but easier to keep indexes in lock-step with other array
        let bi,bj,bk = besti,bestj,bestk
        q.Enqueue(bi,bj,bk)
        dist.[bi,bj,bk] <- 0
        let mutable besti,bestj,bestk = bi, bj, bk
        while q.Count > 0 do
            let i,j,k = q.Dequeue()
            let d = dist.[i,j,k]
            for di,dj,dk in [1,0,0; 0,1,0; 0,0,1; -1,0,0; 0,-1,0; 0,0,-1] do
                if a.[i+di,j+dj,k+dk]<>null && dist.[i+di,j+dj,k+dk] > d+1 then
                    dist.[i+di,j+dj,k+dk] <- d+1  // TODO bias to walls
                    prev.[i+di,j+dj,k+dk] <- (i,j,k,(di=0),(dj=0),(dk=0))  // booleans here help us track 'normal' to the path
                    q.Enqueue(i+di,j+dj,k+dk)
                    let x,y,z = XYZ(i,j,k)
                    if (y>=map.GetHeightMap(x,z)) then // surface
                        // found shortest
                        besti <- i+di
                        bestj <- j+dj
                        bestk <- k+dk
                        while q.Count > 0 do
                            q.Dequeue() |> ignore
        // found a path
        let sx,sy,sz = XYZ(bi,bj,bk)
        let ex,ey,ez = XYZ(besti,bestj,bestk)
        printfn "(%d,%d,%d) is %d blocks from (%d,%d,%d)" sx sy sz dist.[besti,bestj,bestk] ex ey ez
        let mutable i,j,k = besti,bestj,bestk
        let fullDist = dist.[besti,bestj,bestk]
        let mutable count = 0
        let rng = System.Random()
        let spawnerTileEntities = ResizeArray()
        while i<>bi || j<>bj || k<>bk do
            let ni,nj,nk,ii,jj,kk = prev.[i,j,k]   // ii/jj/kk track 'normal' to the path
            // maybe put mob spawner nearby
            let pct = float count / float fullDist
            if rng.NextDouble() < pct then
                let xx,yy,zz = XYZ(i,j,k)
                let mutable spread = 1   // check in outwards 'rings' around the path until we find a block we can replace
                let mutable ok = false
                while not ok do
                    let feesh = ResizeArray()
                    let xs = if ii then [xx-spread .. xx+spread] else [xx]
                    let ys = if jj then [yy-spread .. yy+spread] else [yy]
                    let zs = if kk then [zz-spread .. zz+spread] else [zz]
                    for x in xs do
                        for y in ys do
                            for z in zs do
                                if map.GetBlockInfo(x,y,z).BlockID = 97uy then // if silverfish
                                    feesh.Add(x,y,z)
                    if feesh.Count > 0 then
                        let x,y,z = feesh.[rng.Next(feesh.Count-1)]
                        map.SetBlockIDAndDamage(x, y, z, 52uy, 0uy) // 52 = monster spawner
                        let ms = MobSpawnerInfo(x=x, y=y, z=z, BasicMob="Skeleton")
                        spawnerTileEntities.Add(ms.AsNbtTileEntity())
                        ok <- true
                    spread <- spread + 1
                    if spread = 5 then  // give up if we looked a few blocks away and didn't find a suitable block to swap
                        ok <- true
            // put stripe on the ground (TODO vertical through air)
            let mutable pi,pj,pk = i,j,k
            while a.[pi,pj,pk]<>null do
                pj <- pj - 1
            let x,y,z = XYZ(pi,pj,pk)
            map.SetBlockIDAndDamage(x,y,z,73uy,0uy)  // 73 = redstone ore (lights up when things walk on it)
            i <- ni
            j <- nj
            k <- nk
            count <- count + 1
        // write out all the spawner data we just placed
        map.AddOrReplaceTileEntities(spawnerTileEntities)
        // put beacon at top end
        for x = ex-2 to ex+2 do
            for y = ey-4 to ey-1 do
                for z = ez-2 to ez+2 do
                    map.SetBlockIDAndDamage(x,y,z,166uy,0uy)  // barrier
        map.SetBlockIDAndDamage(ex,ey-2,ez,138uy,0uy) // beacon
        for x = ex-1 to ex+1 do
            for z = ez-1 to ez+1 do
                map.SetBlockIDAndDamage(x,ey-3,z,42uy,0uy)  // iron block
        // put treasure at bottom end
        putTreasureBoxAt(map,sx,sy,sz)
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
        "dirt",     33, 10, 0, 256
        "gravel",   33,  8, 0, 256
        "granite",  33,  0, 0,  80
        "diorite",  33,  0, 0,  80
        "andesite",  1, 10, 0,  80
        "coal",     17, 20, 0, 128
        "iron",      9,  3, 0,  64
        "gold",      9,  1, 0,  32
        "redstone",  8,  3, 0,  24
        "diamond",   4,  1, 0,  16
    |]
let blockSubstitutionsTrial =
    [|
          1uy,0uy,   97uy,0uy;     // stone -> silverfish
          //1uy,5uy,   uy,0uy;     // andesite -> TODO mob spawner
          //73uy,0uy,   73uy,0uy;     // redstone -> TODO mob spawner
    |] // TODO what about tile entities like mob spawners? want to cache them per-chunk and then write them to chunks at end

let substituteBlocks(map:MapFolder) =
    let LOX, LOY, LOZ = -512, 11, -512
    let MAXI, MAXJ, MAXK = 1024, 50, 1024
    let mutable currentSectionBlocks,currentSectionBlockData,curx,cury,curz = null,null,-1000,-1000,-1000
    let XYZ(i,j,k) =
        let x = i-1 + LOX
        let y = j-1 + LOY
        let z = k-1 + LOZ
        x,y,z
    printf "SUBST"
    for j = 1 to MAXJ do
        printf "."
        for i = 1 to MAXI do
            for k = 1 to MAXK do
                let x,y,z = XYZ(i,j,k)
                if not(DIV(x,16) = DIV(curx,16) && DIV(y,16) = DIV(cury,16) && DIV(z,16) = DIV(curz,16)) then
                    let sect = map.GetOrCreateSection(x,y,z)
                    currentSectionBlocks <- sect |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
                    currentSectionBlockData <- sect |> Array.pick (function ByteArray("Data",a) -> Some a | _ -> None)
                    curx <- x
                    cury <- y
                    curz <- z
                let dx, dy, dz = (x+51200) % 16, y % 16, (z+51200) % 16
                let bix = dy*256 + dz*16 + dx
                let bid = currentSectionBlocks.[bix]
                let dmg = if bix%2=1 then currentSectionBlockData.[bix/2] >>> 4 else currentSectionBlockData.[bix/2] &&& 0xFuy
                for obid, odmg, nbid, ndmg in blockSubstitutionsTrial do
                    if bid = obid && dmg = odmg then
                        currentSectionBlocks.[bix] <- nbid
                        let mutable tmp = currentSectionBlockData.[bix/2]
                        if bix%2 = 0 then
                            tmp <- tmp &&& 0xF0uy
                            tmp <- tmp + ndmg
                        else
                            tmp <- tmp &&& 0x0Fuy
                            tmp <- tmp + (ndmg<<< 4)
                        currentSectionBlockData.[bix/2] <- tmp
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
    let a = Array2D.zeroCreateBased -512 -512 1024 1024  // TODO factor constants
    printfn "PART..."
    // find all points height over threshold
    for x = -512 to 511 do
        for z = -512 to 511 do
            let h = heightMap.[x,z]
            if h > connectedThreshold then
                a.[x,z] <- new Partition(new Thingy(0 (*x*1024+z*),false,(h>goodThreshold)))
    printfn "CC..."
    // connected-components them
    for x = -512 to 511-1 do
        for z = -512 to 511-1 do
            if a.[x,z] <> null && a.[x,z+1] <> null then
                a.[x,z].Union(a.[x,z+1])
            if a.[x,z] <> null && a.[x+1,z] <> null then
                a.[x,z].Union(a.[x+1,z])
    let CCs = new System.Collections.Generic.Dictionary<_,_>()
    for x = -512 to 511 do
        for z = -512 to 511 do
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
        highPoints.Add(p)
    // find the 'best' ones based on which have lots of high ground near them
    let score(x,z) =
        try
            let mutable s = 0
            let D = bestNearbyDist
            for a = x-D to x+D do
                for b = z-D to z+D do
                    s <- s + heightMap.[a,b]
            s
        with _ -> 0  // deal with array index out of bounds
    let distance2(a,b,c,d) = (a-c)*(a-c)+(b-d)*(b-d)
    let bestHighPoints = ResizeArray()
    let bestHighPointsScores = ResizeArray()
    for hx,hz in highPoints |> Seq.sortByDescending score do
        if hx > -480 && hx < 480 && hz > -480 && hz < 480 then  // not at edge of bounds
            if bestHighPoints |> Seq.forall (fun (ex,ez) -> distance2(ex,ez,hx,hz) > 200*200) then   // spaced apart TODO factor constants
                bestHighPoints.Add( (hx,hz) )
                bestHighPointsScores.Add( score(hx,hz) )
    bestHighPoints, bestHighPointsScores

let findSomeMountainPeaks(map:MapFolder,hm) =
    let bestHighPoints, scores = findBestPeaksAlgorithm(hm,80,100,3)
    printfn "The best high points are:"
    for (x,z),s in Seq.map2 (fun x y -> x,y) bestHighPoints scores do
        printfn "  (%4d,%4d) - %d" x z s
    // decorate map with dungeon ascent
    let spawnerTileEntities = ResizeArray()
    let rng = System.Random()
    for (x,z) in bestHighPoints do
        let y = map.GetHeightMap(x,z)
        putTreasureBoxAt(map,x,y,z)   // TODO heightmap, blocklight, skylight
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
                        //let ms = MobSpawnerInfo(x=x, y=y, z=z, BasicMob="Skeleton")
                        let ms = MobSpawnerInfo(x=x, y=y, z=z, BasicMob="Spider", ExtraNbt=[ List("Passengers",Compounds[| [|String("id","Skeleton"); List("HandItems",Compounds[| [|String("id","bow");Int("Count",1);End|]; [| End |] |]); End|] |] )] )
                        spawnerTileEntities.Add(ms.AsNbtTileEntity())
    map.AddOrReplaceTileEntities(spawnerTileEntities)

let findSomeFlatAreas(map:MapFolder,hm:_[,]) =
    // convert height map to 'goodness' function that looks for similar-height blocks nearby
    // then treat 'goodness' as 'height', and the existing 'find mountain peaks' algorithm may work
    let a = Array2D.zeroCreateBased -512 -512 1024 1024
    let fScores = [| 100; 90; 75; 50; 0; -100; -999 |]
    let f(h1,h2) =
        let diff = abs(h1-h2)
        fScores.[min diff (fScores.Length-1)]
    let D = 10
    printfn "PREP FLAT MAP..."
    for x = -512+D to 511-D do
        for z = -512+D to 511-D do
            let h = if hm.[x,z] > 65 && hm.[x,z] < 90 then hm.[x,z] else 255  // only pick points above sea level but not too high
            let mutable score = 0
            for dx = -D to D do
                for dz = -D to D do
                    let ds = f(h,hm.[x+dx,z+dz])
                    score <- score + ds
            a.[x,z] <- score
    let bestFlatPoints,scores = findBestPeaksAlgorithm(a,2000,3000,D)
    printfn "The best flat points are:"
    let chosen = ResizeArray()
    for (x,z),s in Seq.map2 (fun x y -> x,y) bestFlatPoints scores do
        printfn "  (%4d,%4d) - %d" x z s
        if s > 10000000 then
            chosen.Add( (x,z) )
    let bestFlatPoints = chosen
    // decorate map with dungeon
    let spawnerTileEntities = ResizeArray()
    let rng = System.Random()
    for (x,z) in bestFlatPoints do
        let y = map.GetHeightMap(x,z)
        putTreasureBoxAt(map,x,y,z)   // TODO heightmap, blocklight, skylight
        for i = x-20 to x+20 do
            for j = z-20 to z+20 do
                if abs(x-i) > 2 || abs(z-j) > 2 then
                    let dist = abs(x-i) + abs(z-j)
                    let pct = float (40-dist) / 50.0
                    if rng.NextDouble() < pct then
                        let x = i
                        let z = j
                        let y = map.GetHeightMap(x,z) + rng.Next(2)
                        if rng.Next(5+2*dist) = 0 then
                            map.SetBlockIDAndDamage(x, y, z, 52uy, 0uy) // 52 = monster spawner   // TODO heightmap, blocklight, skylight
                            //let ms = MobSpawnerInfo(x=x, y=y, z=z, BasicMob="Skeleton")
                            let ms = MobSpawnerInfo(x=x, y=y, z=z, BasicMob="Spider", ExtraNbt=[ List("Passengers",Compounds[| [|String("id","Skeleton"); List("HandItems",Compounds[| [|String("id","bow");Int("Count",1);End|]; [| End |] |]); End|] |] )] )
                            spawnerTileEntities.Add(ms.AsNbtTileEntity())
                        else
                            map.SetBlockIDAndDamage(x, y, z, 30uy, 0uy) // 30 = cobweb
    map.AddOrReplaceTileEntities(spawnerTileEntities)

let makeCrazyMap() =
    let user = "Admin1"
    let map = new MapFolder("""C:\Users\"""+user+(sprintf """\AppData\Roaming\.minecraft\saves\seed31Copy\region\"""))
    printfn "CACHE HM & double spawners..."
    let spawnerTileEntities = ResizeArray()
    let hm = Array2D.zeroCreateBased -512 -512 1024 1024
    for x = -512 to 511 do
        printfn "%d" x
        for z = -512 to 511 do
            for y = 40 downto 12 do  // down, because will put new ones above
                let bi = map.GetBlockInfo(x,y,z) // caches height map as side effect
                // double all existing mob spawners
                if bi.BlockID = 52uy then // 52-mob spawner
                    let kind =
                        match bi.TileEntity.Value with
                        | Compound(_,cs) ->
                            match cs |> Seq.find (fun x -> x.Name = "SpawnData") with
                            | Compound(_,sd) -> sd |> Seq.find (fun x -> x.Name = "id") |> (fun (String("id",k)) -> k)
                    map.SetBlockIDAndDamage(x, y+1, z, 52uy, 0uy) // 52 = monster spawner
                    let ms = MobSpawnerInfo(x=x, y=y+1, z=z, BasicMob=(if kind = "CaveSpider" then "Skeleton" else "CaveSpider"), 
                                            ExtraNbt=[ yield String("DeathLootTable","TODO")// TODO
                                                       if kind = "CaveSpider" then 
                                                            yield List("HandItems",Compounds[| [|String("id","bow");Int("Count",1);End|]; [| End |] |]) ] ) 
                    spawnerTileEntities.Add(ms.AsNbtTileEntity())
                if y = 0 then
                    let h = map.GetHeightMap(x,z)
                    hm.[x,z] <- h
    map.AddOrReplaceTileEntities(spawnerTileEntities)
    //findSomeMountainPeaks(map,hm)
    //findSomeFlatAreas(map,hm)
    //substituteBlocks(map)
    //findUndergroundAirSpaceConnectedComponents(map)
    printfn "saving results..."
    map.WriteAll()
    printfn "...done!"


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
