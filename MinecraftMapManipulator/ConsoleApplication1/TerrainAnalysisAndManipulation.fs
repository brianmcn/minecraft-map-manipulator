module TerrainAnalysisAndManipulation

open Algorithms
open NBT_Manipulation
open RegionFiles
open CustomizationKnobs

let HM_IGNORING_LEAVES_SKIPPABLE_DOWN_BLOCKS = 
    new System.Collections.Generic.HashSet<_>( [|0uy; 18uy; 161uy; 78uy; 31uy; 175uy; 32uy; 37uy; 38uy; 39uy; 40uy; 106uy |] ) // air, leaves, leaves2, snow_layer, tallgrass, double_plant, deadbush, yellow_flower, red_flower, brown_mushroom, red_mushroom, vine

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

let blockTE(blockEntityName,x,y,z,items,customName:Strings.TranslatableString,lootTable,lootTableSeed) =
    let te = [| yield Int("x",x); yield Int("y",y); yield Int("z",z)
                yield String("id",blockEntityName); yield String("Lock",""); 
                yield List("Items",items)
                yield String("CustomName",customName.Text)
                if lootTable <> null then
                    yield String("LootTable",lootTable)
                    yield Long("LootTableSeed",lootTableSeed)
                yield End |]
    te

let putChestCore(blockEntityName,x,y,z,chestBid,chestDmg,items,customName:Strings.TranslatableString,lootTable,lootTableSeed,map:MapFolder,tileEntities:ResizeArray<_>) =
    map.SetBlockIDAndDamage(x,y,z,chestBid,chestDmg)
    let te = blockTE(blockEntityName,x,y,z,items,customName,lootTable,lootTableSeed)
    if tileEntities <> null then
        tileEntities.Add( te )
    else
        map.AddOrReplaceTileEntities[| te |]

let putTrappedChestWithLootTableAt(x,y,z,customName,lootTable,lootTableSeed,map,tileEntities) =
    putChestCore("Chest",x,y,z,146uy,3uy,Compounds[| |],customName,lootTable,lootTableSeed,map,tileEntities)  // 146=trapped chest

let putUntrappedChestWithLootTableAt(x,y,z,customName,lootTable,lootTableSeed,map,tileEntities) =
    putChestCore("Chest",x,y,z,54uy,3uy,Compounds[| |],customName,lootTable,lootTableSeed,map,tileEntities)  // 54=(non-trapped) chest

let putTrappedChestWithItemsAt(x,y,z,customName,items,map,tileEntities) =
    putChestCore("Chest",x,y,z,146uy,3uy,items,customName,null,0L,map,tileEntities)  // 146=trapped chest

let putUntrappedChestWithItemsAndOrientationAt(x,y,z,customName,items,orientation,map,tileEntities) =
    putChestCore("Chest",x,y,z,54uy,orientation,items,customName,null,0L,map,tileEntities)  // 54=(non-trapped) chest
let putUntrappedChestWithItemsAt(x,y,z,customName,items,map,tileEntities) =
    putUntrappedChestWithItemsAndOrientationAt(x,y,z,customName,items,3uy,map,tileEntities)

let putFurnaceWithItemsAt(x,y,z,customName,items,map,tileEntities) =
    putChestCore("Furnace",x,y,z,61uy,3uy,items,customName,null,0L,map,tileEntities)  // 61,3=furnace facing south (just has slots 0/1/2)

///////////////////////////////////////////////

let runCommandBlockOnLoadCore(sx,sy,sz,map:MapFolder,cmd,futureTime) =
    map.SetBlockIDAndDamage(sx,sy,sz,137uy,0uy)  // command block
    map.AddOrReplaceTileEntities([| [| Int("x",sx); Int("y",sy); Int("z",sz); String("id","Control"); Byte("auto",0uy); String("Command",cmd); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",1uy); End |] |])
    map.AddTileTick("minecraft:command_block",futureTime,0,sx,sy,sz)
let runCommandBlockOnLoad(sx,sy,sz,map:MapFolder,cmd) =
    runCommandBlockOnLoadCore(sx,sy,sz,map,cmd,1)

let runCommandBlockOnLoadSelfDestruct(sx,sy,sz,map:MapFolder,cmd) =
    // place block here and y-1, first runs cmd, then fills both with air
    runCommandBlockOnLoadCore(sx,sy,sz,map,cmd,1)
    runCommandBlockOnLoadCore(sx,sy-1,sz,map,"fill ~ ~ ~ ~ ~1 ~ air",2)

let putTreasureBoxAtCore(map:MapFolder,sx,sy,sz,lootTableName,lootTableSeed,itemsNbt,topbid,topdmg,glassbid,glassdmg,radius) =
    let RADIUS = radius
    for x = sx-RADIUS to sx+RADIUS do
        for z = sz-RADIUS to sz+RADIUS do
            map.SetBlockIDAndDamage(x,sy,z,topbid,topdmg)  // lapis block
            map.SetBlockIDAndDamage(x,sy+3,z,topbid,topdmg)  // lapis block
    for x = sx-RADIUS to sx+RADIUS do
        for y = sy+1 to sy+2 do
            for z = sz-RADIUS to sz+RADIUS do
                map.SetBlockIDAndDamage(x,y,z,glassbid,glassdmg)  // glass
    map.SetBlockIDAndDamage(sx,sy+2,sz,198uy,1uy) // 198=end_rod  (end rods give off light level 14, don't obstruct a beacon shining through, can attach to top of chest)
    putChestCore("Chest",sx,sy+1,sz,54uy,2uy,Compounds itemsNbt,Strings.NAME_OF_GENERIC_TREASURE_BOX,lootTableName,lootTableSeed,map,null)

let putTreasureBoxAt(map:MapFolder,sx,sy,sz,lootTableName,lootTableSeed) =
    putTreasureBoxAtCore(map,sx,sy,sz,lootTableName,lootTableSeed,[| |],22uy,0uy,20uy,0uy,2) //22=lapis, 20=glass

let putTreasureBoxWithItemsAt(map:MapFolder,sx,sy,sz,itemsNbt) =
    putTreasureBoxAtCore(map,sx,sy,sz,null,0L,itemsNbt,22uy,0uy,20uy,0uy,2) //22=lapis, 20=glass

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

/////////////////////////////////////////////////////////////////

// throughout, will ignore leave decay states, wood orientations
type WoodType =
    | OAK
    | SPRUCE
    | BIRCH
    | JUNGLE
    | ACACIA
    | DARK_OAK
    member this.IsLog(bid,dmg) =
        match this with
        | OAK    -> bid=17uy && (dmg &&& 3uy)=0uy
        | SPRUCE -> bid=17uy && (dmg &&& 3uy)=1uy
        | BIRCH  -> bid=17uy && (dmg &&& 3uy)=2uy
        | JUNGLE -> bid=17uy && (dmg &&& 3uy)=3uy
        | ACACIA   -> bid=162uy && (dmg &&& 3uy)=0uy
        | DARK_OAK -> bid=162uy && (dmg &&& 3uy)=1uy
    member this.IsLeaves(bid,dmg) =
        match this with
        | OAK    -> bid=18uy && (dmg &&& 3uy)=0uy
        | SPRUCE -> bid=18uy && (dmg &&& 3uy)=1uy
        | BIRCH  -> bid=18uy && (dmg &&& 3uy)=2uy
        | JUNGLE -> bid=18uy && (dmg &&& 3uy)=3uy
        | ACACIA   -> bid=161uy && (dmg &&& 3uy)=0uy
        | DARK_OAK -> bid=161uy && (dmg &&& 3uy)=1uy
    static member AsLog(bid,dmg) =
        if bid=17uy && (dmg &&& 3uy)=0uy then Some OAK
        elif bid=17uy && (dmg &&& 3uy)=1uy then Some SPRUCE
        elif bid=17uy && (dmg &&& 3uy)=2uy then Some BIRCH
        elif bid=17uy && (dmg &&& 3uy)=3uy then Some JUNGLE
        elif bid=162uy && (dmg &&& 3uy)=0uy then Some ACACIA
        elif bid=162uy && (dmg &&& 3uy)=1uy then Some DARK_OAK
        else None
    static member AsLeaves(bid,dmg) =
        if bid=18uy && (dmg &&& 3uy)=0uy then Some OAK
        elif bid=18uy && (dmg &&& 3uy)=1uy then Some SPRUCE
        elif bid=18uy && (dmg &&& 3uy)=2uy then Some BIRCH
        elif bid=18uy && (dmg &&& 3uy)=3uy then Some JUNGLE
        elif bid=161uy && (dmg &&& 3uy)=0uy then Some ACACIA
        elif bid=161uy && (dmg &&& 3uy)=1uy then Some DARK_OAK
        else None

type MCTree(woodType) =
    let logs = ResizeArray()
    let leaves = ResizeArray()
    let mutable lly = 0
    let mutable cs = 0,0,0
    member this.WoodType = woodType
    member this.Logs = logs
    member this.Leaves = leaves
    member this.LowestLeafY with get() = lly and set(y) = lly <- y
    member this.CanonicalStump with get() = cs and set(p) = cs <- p
    member this.CouldClaimAsMyLeaf(nbi:BlockInfo) =
        // leaf matches wood, or jungle stump with oak leaves  
        let leafOption = WoodType.AsLeaves(nbi.BlockID,nbi.BlockData)
        leafOption = Some(this.WoodType) || 
            // most jungle stumps are one log, but sometimes two are right next to each other, so <= 2 is a good approximate heuristic
            (this.Logs.Count<=2 && this.WoodType=WoodType.JUNGLE && leafOption=Some(WoodType.OAK))
    member this.Replace(map:MapFolder, logbid, logdmg, leafbid, leafdmg) =
        for x,y,z in this.Logs do
            map.SetBlockIDAndDamage(x,y,z,logbid,logdmg)
        for x,y,z,_ in this.Leaves do
            map.SetBlockIDAndDamage(x,y,z,leafbid,leafdmg)
    member this.Remove(map:MapFolder) =
        let nearby(x,y,z) =
            for dx,dy,dz in [-1,0,0; 1,0,0; 0,0,-1; 0,0,1; 0,1,0] do
                let x,y,z = x+dx,y+dy,z+dz
                let bid = map.GetBlockInfo(x,y,z).BlockID 
                if dy=1 && bid=78uy || bid=39uy || bid=40uy then // snow_layer, mushrooms are possible "above-tree-block accessories"
                    map.SetBlockIDAndDamage(x,y,z,0uy,0uy)
                if bid=127uy then // cocoa beans anywhere connected
                    map.SetBlockIDAndDamage(x,y,z,0uy,0uy)
                if bid=106uy then // vine, extra attention needed, can hang down
                    let mutable y = y
                    while map.GetBlockInfo(x,y,z).BlockID=106uy do
                        map.SetBlockIDAndDamage(x,y,z,0uy,0uy)
                        y <- y - 1
        let mutable lowestY = 256
        let lowestLogs = ResizeArray()
        for x,y,z in this.Logs do
            map.SetBlockIDAndDamage(x,y,z,0uy,0uy)
            nearby(x,y,z)
            // keep track of lowest logs
            if y < lowestY then
                lowestY <- y
                lowestLogs.Clear()
                lowestLogs.Add((x,y,z))
            if y = lowestY then
                lowestLogs.Add((x,y,z))
        for x,y,z,_ in this.Leaves do
            map.SetBlockIDAndDamage(x,y,z,0uy,0uy)
            nearby(x,y,z)
        for x,y,z in lowestLogs do
            // fixup any dirt below lowest logs
            let y = y-1
            let bi = map.GetBlockInfo(x,y,z)
            // TODO, note that this will not replace snow in places where below-lowest-log was not dirt, nor in places leaves were above ground
            if bi.BlockID=3uy && bi.BlockData=0uy then // dirt
                let mutable grass,coarse,podzol,snow = false,false,false,false
                for dx = -1 to 1 do
                    for dy = -1 to 1 do
                        for dz = -1 to 1 do
                            let nbi = map.GetBlockInfo(x+dx, y+dy, z+dz)
                            if nbi.BlockID = 2uy then
                                grass <- true
                            if nbi.BlockID = 3uy && nbi.BlockData=1uy then
                                coarse <- true
                            if nbi.BlockID = 3uy && nbi.BlockData=2uy then
                                podzol <- true
                            let nbi = map.GetBlockInfo(x+dx, y+dy+1, z+dz)
                            if nbi.BlockID = 78uy then
                                snow <- true
                if grass then
                    map.SetBlockIDAndDamage(x,y,z,2uy,0uy)
                elif podzol then
                    map.SetBlockIDAndDamage(x,y,z,3uy,2uy)
                elif coarse then
                    map.SetBlockIDAndDamage(x,y,z,3uy,1uy)
                if snow then
                    map.SetBlockIDAndDamage(x,y+1,z,78uy,0uy)

[<AllowNullLiteral>]
type ContainerOfMCTrees(allTrees:MCTree seq) =
    // preprocess trees
    let treeByXZ = new System.Collections.Generic.Dictionary<_,_>()
    do
        for t in allTrees do
            let x,_,z = t.CanonicalStump
            if not(treeByXZ.ContainsKey(x,z)) then
                treeByXZ.Add((x,z),ResizeArray[t])
            else
                treeByXZ.[x,z].Add(t)
    member this.Remove(x,z,map:MapFolder) =
        if treeByXZ.ContainsKey(x,z) then
            for t in treeByXZ.[(x,z)] do
                t.Remove(map)
            treeByXZ.[(x,z)].Clear()
    member this.Replace(x,z,logbid,logdmg,leafbid,leafdmg,map:MapFolder) =
        let mutable count = 0
        if treeByXZ.ContainsKey(x,z) then
            for t in treeByXZ.[(x,z)] do
                t.Replace(map,logbid,logdmg,leafbid,leafdmg)
                count <- count + 1
        count
    member this.All() =
        seq {
            for vs in treeByXZ.Values do
                for t in vs do
                    yield t
        }

type PriorityQueue() =
    let mutable pq = Set.empty 
    let mutable count = 0
    member this.Enqueue(pri,v) = 
        pq <- pq.Add(pri,v)
        count <- count + 1
    member this.Dequeue() =
        let r = pq.MinimumElement 
        pq <- pq.Remove(r)
        count <- count - 1
        r
    member this.Count = count

let treeify(map:MapFolder, hm:_[,]) =
    let TREE_MIN_Y = 60  // why not 63? swamp trees often start below water surface, and end up removing leaves of a swamp tree but not stump, when neighbor tree above water gets assigned all leaves of tree below water.  Lower values are 'better' but 'more expensive to compute'
    let INTERIOR_WINDOW_SIZE = 126 // (so that windows fit snugly in 2048x2048 with 8 border)
    let BORDER_SIZE = 8
    let WINDOW_SIZE = INTERIOR_WINDOW_SIZE + 2*BORDER_SIZE
    let MIN_XZ = MINIMUM+BORDER_SIZE
    let MAX_XZ = MINIMUM+LENGTH-1-WINDOW_SIZE
    let allTrees = ResizeArray()
    for wx in [MIN_XZ .. INTERIOR_WINDOW_SIZE .. MAX_XZ] do
        for wz in [MIN_XZ .. INTERIOR_WINDOW_SIZE .. MAX_XZ] do
            //printfn "%d %d is corner, %d %d is int corner" wx wz (wx+BORDER_SIZE) (wz+BORDER_SIZE)
            let visitedLogs = new System.Collections.Generic.HashSet<_>()
            let treesInThisWindow = ResizeArray()
            let mutable maxh = 0
            for x = wx to wx+WINDOW_SIZE-1 do
                for z = wz to wz+WINDOW_SIZE-1 do
                    maxh <- max maxh hm.[x,z]
            for y = TREE_MIN_Y to maxh do
                for x = wx to wx+WINDOW_SIZE-1 do
                    for z = wz to wz+WINDOW_SIZE-1 do
                        let bi = map.GetBlockInfo(x,y,z)
                        match WoodType.AsLog(bi.BlockID, bi.BlockData) with
                        | None -> ()
                        | Some woodType ->
                            if not(visitedLogs.Contains(x,y,z)) then
                                // due to yxz iteration order, this is the northwest lowest stump, use it as the canonical location of the tree
                                let treeIsInInterior = x >= wx + BORDER_SIZE && x <= wx+WINDOW_SIZE-1-BORDER_SIZE && z >= wz + BORDER_SIZE && z <= wz+WINDOW_SIZE-1-BORDER_SIZE
                                visitedLogs.Add(x,y,z) |> ignore
                                let tree = MCTree(woodType)
                                tree.Logs.Add(x,y,z)
                                tree.CanonicalStump <- x,y,z
                                let q = new System.Collections.Generic.Queue<_>()
                                q.Enqueue(x,y,z)
                                while q.Count <> 0 do
                                    let cx,cy,cz = q.Dequeue()
                                    for dx in [-1;0;1] do
                                        for dz in [-1;0;1] do
                                            for dy in [0;1] do // y is iterating up in the outer loop, so we always found bottom first, only need to go up
                                                let nx,ny,nz = cx+dx, cy+dy, cz+dz
                                                // we may have wandered out of bounds, stay inside our outer window
                                                if nx >= wx && nx <= wx+WINDOW_SIZE-1 && nz >= wz && nz <= wz+WINDOW_SIZE-1 then
                                                    if not(visitedLogs.Contains(nx,ny,nz)) then
                                                        let nbi = map.GetBlockInfo(nx,ny,nz)
                                                        if WoodType.AsLog(nbi.BlockID,nbi.BlockData) = Some(woodType) then
                                                            visitedLogs.Add(nx,ny,nz) |> ignore
                                                            tree.Logs.Add(nx,ny,nz)
                                                            q.Enqueue(nx,ny,nz)
                                let mutable isTallPokingAbove = false  // does the tree start below TREE_MIN_Y and we're just seeing the top of it?
                                if y = TREE_MIN_Y then
                                    let bi = map.GetBlockInfo(x,y-1,z)
                                    match WoodType.AsLog(bi.BlockID, bi.BlockData) with
                                    | None -> ()
                                    | Some(w) -> if w = woodType then isTallPokingAbove <- true
                                if not isTallPokingAbove then
                                    if tree.Logs.Count > 2 then
                                        treesInThisWindow.Add(tree)
                                        if treeIsInInterior then
                                            allTrees.Add(tree)
                                            //printfn "found tree at %d %d %d" x y z
                                    else
                                        printfn "NOT ignoring tiny tree (jungle floor? burned? diagonal across border?) at %d %d %d" x y z
                                        treesInThisWindow.Add(tree)
                                        if treeIsInInterior then
                                            allTrees.Add(tree)
                                        ()
            // now that we have all the tree wood in the large window, determine leaf ownership
            // first find lowest point where each tree can own a leaf
            for t in treesInThisWindow do
                let ls = t.Logs.ToArray()
                Array.sortInPlaceBy (fun (x,y,z) -> y,x,z) ls
                t.LowestLeafY <- -1
                for cx,cy,cz in ls do
                    if t.LowestLeafY = -1 then
                        let mutable numAdjacentLeaves = 0
                        for dx,dz in [-1,0; 1,0; 0,-1; 0,1] do
                            let nx,ny,nz = cx+dx, cy, cz+dz
                            // we may have wandered out of bounds, stay inside our outer window
                            if nx >= wx && nx <= wx+WINDOW_SIZE-1 && nz >= wz && nz <= wz+WINDOW_SIZE-1 then
                                let nbi = map.GetBlockInfo(nx,ny,nz)
                                if t.CouldClaimAsMyLeaf(nbi) then
                                    numAdjacentLeaves <- numAdjacentLeaves + 1
                                    // large oaks can have low offshoot branches where leaves are below them in only one direction, try to kludge that case
                                    let nbi = map.GetBlockInfo(nx,ny+1,nz)
                                    if WoodType.AsLog(nbi.BlockID,nbi.BlockData) = Some(t.WoodType) then
                                        numAdjacentLeaves <- numAdjacentLeaves + 1 // if same-type log above leaf, assume branching oak and ensure LLY adjusts for this
                                    // sometimes the 'side' of one low tree crashes into the 'stem' of another, usually we can detect and prevent this thusly:
                                    if nbi.BlockID = 0uy then
                                        // was air just above the supposed 'bottom connecting leaf', but bottom connecting leaves never have air above, I think, so reject
                                        numAdjacentLeaves <- numAdjacentLeaves - 1
                                    else
                                        // another way to detect is to see if there are also lowest-leaves on the opposite side of the stem here:
                                        let nx,ny,nz = cx-dx, cy, cz-dz
                                        if nx >= wx && nx <= wx+WINDOW_SIZE-1 && nz >= wz && nz <= wz+WINDOW_SIZE-1 then
                                            let mutable nbi = map.GetBlockInfo(nx,ny,nz)
                                            if WoodType.AsLog(nbi.BlockID,nbi.BlockData) = Some(t.WoodType) then
                                                // seems to be two-wide tree, go one further back
                                                let nx,ny,nz = cx-2*dx, cy, cz-2*dz
                                                if nx >= wx && nx <= wx+WINDOW_SIZE-1 && nz >= wz && nz <= wz+WINDOW_SIZE-1 then
                                                    nbi <- map.GetBlockInfo(nx,ny,nz)
                                            if not(t.CouldClaimAsMyLeaf(nbi)) then
                                                // we didn't find lower leaves on the opposite side, suggesting this may be a crashing-stem case
                                                numAdjacentLeaves <- numAdjacentLeaves - 1
                        if numAdjacentLeaves >=2 then
                            t.LowestLeafY <- cy
            // then walk outwards from all logs over all trees, claiming ownership
            let claimedLeaves = new System.Collections.Generic.HashSet<_>()
            let claimAttemptsAtThisPriority = new System.Collections.Generic.Dictionary<_,ResizeArray<_>>()
            let computeXZsq(t:MCTree,x,z) =
                let tx,_,tz = t.CanonicalStump 
                let dx = x - tx
                let dz = z - tz
                dx*dx+dz*dz
            let pq = PriorityQueue()
            for treeIndex = 0 to treesInThisWindow.Count-1 do
                let t = treesInThisWindow.[treeIndex]
                for cx,cy,cz in t.Logs do
                    if cy >= t.LowestLeafY then
                        pq.Enqueue(0,(cx,cy,cz,treeIndex))
            let processLeaves(dirs) =
                let finishThisPriority(currentPriority) =
                    for KeyValue((x,y,z),ts) in claimAttemptsAtThisPriority do
//                        if (x,y,z)=(514,75,328) then
//                            printfn "hey"
                        claimedLeaves.Add(x,y,z) |> ignore
                        let a = ts.ToArray()
                        Array.sortInPlaceBy (fun ti -> computeXZsq(treesInThisWindow.[ti],x,z)) a
                        // TODO? could maybe mitigate 'ties' by looking at symmetry-around-trunk, e.g. if the opposite x/z around my trunk is air, then prefer to give this leaf to someone else...
                        // even non-ties, e.g. close calls still favor symmetry over distance-to-truck, actually.  but not bad just applies trunk-distance metric.
                        treesInThisWindow.[a.[0]].Leaves.Add(x,y,z,currentPriority)
                    claimAttemptsAtThisPriority.Clear()
                let mutable currentPriority = 0
                while pq.Count <> 0 do
                    let ci,(cx,cy,cz,treeIndex) = pq.Dequeue()
                    let t = treesInThisWindow.[treeIndex]
                    if ci <> currentPriority then
                        finishThisPriority(currentPriority)
                        currentPriority <- ci
                    for dx,dy,dz in dirs do
                        let nx,ny,nz = cx+dx, cy+dy, cz+dz
//                        if (nx,ny,nz)=(514,75,328) then
//                            printfn "hey"
                        // we may have wandered out of bounds, stay inside our outer window
                        if nx >= wx && nx <= wx+WINDOW_SIZE-1 && nz >= wz && nz <= wz+WINDOW_SIZE-1 && ny >= TREE_MIN_Y then
                            if not(claimedLeaves.Contains(nx,ny,nz)) then
                                let nbi = map.GetBlockInfo(nx,ny,nz)
                                if t.CouldClaimAsMyLeaf(nbi) then
                                    let ni = if dy=0 then ci+2 else ci+3  // cost more 'points' to go vertically away from logs, so we claim horizontally faster than vertically
                                    // TODO manhattan distance here means that x+3 claims before x+2,z+2 even though latter is euclidian-better
                                    let mutable alreadyEnqueued = false
                                    if claimAttemptsAtThisPriority.ContainsKey(nx,ny,nz) then
                                        if claimAttemptsAtThisPriority.[nx,ny,nz].Contains(treeIndex) then
                                            alreadyEnqueued <- true
                                        else
                                            claimAttemptsAtThisPriority.[nx,ny,nz].Add(treeIndex)
                                    else
                                        claimAttemptsAtThisPriority.Add((nx,ny,nz), ResizeArray[treeIndex])
                                    if not alreadyEnqueued then
                                        pq.Enqueue(ni,(nx,ny,nz,treeIndex)) // just keep going so long as we're claiming
                finishThisPriority(currentPriority)
            processLeaves([-1,0,0; 1,0,0; 0,0,-1; 0,0,1; 0,1,0])
            // go back and attempt to deal with unclaimed leaves that failed my original ownership heuristic
            for treeIndex = 0 to treesInThisWindow.Count-1 do
                let t = treesInThisWindow.[treeIndex]
                for x,y,z,i in t.Leaves do
                    pq.Enqueue(i,(x,y,z,treeIndex))
            processLeaves([-1,0,0; 1,0,0; 0,0,-1; 0,0,1; 0,1,0; 0,-1,0]) // note this also goes y-1, normally does not
    // done with processing...
    printfn "There were %d trees found" allTrees.Count
(*
    // debug by visualizing ownership
    let mutable color = 0uy
    for t in allTrees do
        for x,y,z in t.Logs do
            map.SetBlockIDAndDamage(x,y,z,159uy,color) // 159=stained_hardened_clay
        for x,y,z,_i in t.Leaves do
            map.SetBlockIDAndDamage(x,y,z,95uy,color) // 95=stained_glass
        color <- color + 1uy
        if color = 16uy then
            color <- 0uy
    map.WriteAll()
*)
    new ContainerOfMCTrees(allTrees)

/////////////////////////////////////////////////////////////////

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
                    currentSectionBlocks <- map.GetOrCreateSection(x,y,z) |> (fun (_sect,blocks,_bd,_bl,_sl) -> blocks)
                    curx <- x
                    cury <- y
                    curz <- z
                let dx = (x+51200) % 16
                let dy = y % 16
                let dz = (z+51200) % 16
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
                for y = bestY + 10 to bestY + 26 do
                    map.SetBlockIDAndDamage(bestX,y,bestZ,89uy,0uy)  // glowstone
                hm.[bestX,bestZ] <- bestY+27
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
        let i = x-MINIMUM
        let j = y-YMIN
        let k = z-MINIMUM
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
                    currentSectionBlocks <- map.GetOrCreateSection(x,y,z) |> (fun (_sect,blocks,_bd,_bl,_sl) -> blocks)
                    curx <- x
                    cury <- y
                    curz <- z
                let dx = (x+51200) % 16
                let dy = y % 16
                let dz = (z+51200) % 16
                let bix = dy*256 + dz*16 + dx
                if currentSectionBlocks.[bix] = 0uy || currentSectionBlocks.[bix] = 30uy then // air or cobweb // TODO or rail?
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
    let skippableDown(bid) = 
        (bid = 8uy || bid=30uy || bid=31uy || bid=37uy || bid=38uy || bid=39uy || bid=40uy || bid=66uy) // flowing_water/web/tallgrass/2flowers/2mushrooms/rail
    let replaceGroundBelowWith(x,y,z,bid,dmg) = 
        let mutable pi,pj,pk = x,y,z
        while a.[pi,pj,pk]<>null do
            pj <- pj - 1
        while skippableDown(map.GetBlockInfo(pi,pj,pk).BlockID) do
            pj <- pj - 1
        map.SetBlockIDAndDamage(pi,pj,pk,bid,dmg)
    let mutable hasDoneFinal, thisIsFinal = false, false
    for s in goodCCs.Values do
        let mutable topX,topY,topZ = 0,0,0
        let sk = System.Array.CreateInstance(typeof<sbyte>, [|LENGTH+2; YLEN+2; LENGTH+2|], [|MINIMUM; YMIN; MINIMUM|]) :?> sbyte[,,] // +2: don't need sentinels here, but easier to keep indexes in lock-step with other array
        let ones = new System.Collections.Generic.HashSet<_>()
        let atHeightMap = new System.Collections.Generic.HashSet<_>()
        for p in s do
            let x,y,z = XYZP(p)
            if y > topY then
                topX <- x
                topY <- y
                topZ <- z
            if y > YMIN && y < YMIN+YLEN && x > MINIMUM && x < MINIMUM+LENGTH && z > MINIMUM && z < MINIMUM+LENGTH then
                sk.[x,y,z] <- 1y
                ones.Add(x,y,z) |> ignore
            if y = hm.[x,z] then
                atHeightMap.Add(x,y,z) |> ignore
        let skel,endp,epwl = Algorithms.skeletonize(sk, ignore, ones) // map.SetBlockIDAndDamage(x,y,z,95uy,byte iter))) // 95 = stained_glass
        skel.UnionWith(endp)
        let MAXIMUM = MINIMUM+LENGTH-1
        let YMAX = YMIN+YLEN-1
        let canMove(x,y,z) = x>MINIMUM && z>MINIMUM && x<MAXIMUM && z < MAXIMUM && y>=YMIN && y<=YMAX && a.[x,y,z]<>null
        match Algorithms.findShortestPath(topX,topY,topZ,canMove,(fun(x,y,z)->skel.Contains(x,y,z)),DIFFERENCES) with
        | None -> printf "FAILED to get to skeleton" // TODO why ever?
        | Some((tsx,tsy,tsz),_path,_moves) ->
        printfn "there were %d endpoints" endp.Count
        match Algorithms.findLongestPath(tsx,tsy,tsz,(fun (x,y,z)->skel.Contains(x,y,z)),(fun (_x,y,_z)->y<YMIN+4),DIFFERENCES) with
        | None -> // if didn't reach low point, nothing else to do
            printfn "FAILED to get near bottom (skeleton too far away?)" // TODO ok?
        | Some((sx,sy,sz),_,_) ->
            let pointsToAddBetweenHMAndSkeleton = new System.Collections.Generic.HashSet<_>()
            for x,y,z in atHeightMap do
                match Algorithms.findShortestPath(x,y,z,canMove,(fun(x,y,z)->skel.Contains(x,y,z)),DIFFERENCES) with
                | None -> printf "FAILED to get to skeleton" // TODO why ever?
                | Some(_,path,_moves) ->
                    for x,y,z in path do
                        pointsToAddBetweenHMAndSkeleton.Add(x,y,z) |> ignore
            skel.UnionWith(pointsToAddBetweenHMAndSkeleton)
            match Algorithms.findShortestPath(sx,sy,sz,(fun (x,y,z)->skel.Contains(x,y,z)),(fun (x,y,z)->y>=hm.[x,z]),DIFFERENCES) with
            | None -> printfn "FAILED to get back up to HM at top" // TODO now impossible, right?
            | Some((ex,ey,ez), path, moves) ->
            printfn "ALL find-paths succeeded, yay"
            // ensure beacon in decent bounds
            let tooClose(x,_y,z) =
                let DB = 60
                x < MINIMUM+DB || z < MINIMUM+DB || x > MINIMUM+LENGTH-DB || z > MINIMUM+LENGTH-DB || 
                    (x > -SPAWN_PROTECTION_DISTANCE_GREEN && x < SPAWN_PROTECTION_DISTANCE_GREEN && z > -SPAWN_PROTECTION_DISTANCE_GREEN && z < SPAWN_PROTECTION_DISTANCE_GREEN)
            if tooClose(sx,sy,sz) || tooClose(ex,ey,ez) then
                () // skip if too close to 0,0 or to map bounds
            else
            let fullDist = path.Count
            log.LogInfo(sprintf "(%d,%d,%d) is %d blocks from (%d,%d,%d)" sx sy sz fullDist ex ey ez)
            if fullDist > 100 && fullDist < 500 then  // only keep mid-sized ones...
                if not hasDoneFinal && fullDist > 300 && ex*ex+ez*ez > SPAWN_PROTECTION_DISTANCE_PURPLE*SPAWN_PROTECTION_DISTANCE_PURPLE then
                    thisIsFinal <- true
                if thisIsFinal || not (ex*ex+ez*ez > SPAWN_PROTECTION_DISTANCE_PURPLE*SPAWN_PROTECTION_DISTANCE_PURPLE) then
                    // don't bother with green beacons near edge of map
                    log.LogSummary(sprintf "added %sbeacon at %d %d %d which travels %d" (if thisIsFinal then "FINAL " else "") ex ey ez fullDist)
                    decorations.Add((if thisIsFinal then 'X' else 'B'),ex,ez)
                    let mutable i,j,k = ex,ey,ez
                    let mutable count = 0
                    let spawners = SpawnerAccumulator("spawners along path")
                    let possibleSpawners = 
                        if thisIsFinal then
                            PURPLE_BEACON_CAVE_DUNGEON_SPAWNER_DATA
                        else
                            GREEN_BEACON_CAVE_DUNGEON_SPAWNER_DATA
                    moves.Reverse()
                    for m in moves do
                        let ni, nj, nk = // next points (could also use 'path' backwards, but need movement info)
                            let dx,dy,dz = DIFFERENCES.[m]
                            i-dx,j-dy,k-dz
                        let ii,jj,kk = m%3<>0, m%3<>1, m%3<>2   // ii/jj/kk track 'normal' to the path
                        makeAreaHard(map,ni,nk)
                        // maybe put mob spawner nearby
                        let pct = 
                            if float count / float fullDist > 0.95 then
                                0.0  // don't put spawners right before the loot box
                            else 
                                float count / (float fullDist * 3.0)
                        if rng.NextDouble() < pct*possibleSpawners.DensityMultiplier then
                            let xx,yy,zz = (i,j,k)
                            let mutable spread = 1   // check in outwards 'rings' around the path until we find a block we can replace
                            let mutable ok = false
                            while not ok do
                                let candidates = ResizeArray()
                                let xs = if ii then [xx-spread .. xx+spread] else [xx]
                                let ys = if jj then [yy-spread .. yy+spread-1] else [yy]  // look less in the ceiling, since ceiling spawners often can't spawn mobs
                                let zs = if kk then [zz-spread .. zz+spread] else [zz]
                                // TODO still possible to place spawners in ceiling where can't spawn mobs, if skeleton happened to be close to ceiling; improve?
                                for x in xs do
                                    for y in ys do
                                        for z in zs do
                                            let bid = map.GetBlockInfo(x,y,z).BlockID
                                            if bid <> 0uy && not(skippableDown(bid)) then // if 'solid' enough to replace with spawner
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
                        replaceGroundBelowWith(i,j,k,73uy,0uy)  // 73 = redstone ore (lights up when things walk on it)
                        i <- ni
                        j <- nj
                        k <- nk
                        count <- count + 1
                    assert(i=sx && j=sy && k=sz)
                    // write out all the spawner data we just placed
                    spawners.AddToMapAndLog(map,log)
                    putBeaconAt(map,ex,ey,ez,(if thisIsFinal then 10uy else 5uy), true) // 10=purple, 5=lime
                    map.SetBlockIDAndDamage(ex,ey+1,ez,130uy,2uy) // ender chest
                    // put treasure at bottom end
                    putTreasureBoxWithItemsAt(map,sx,sy,sz,LootTables.NEWsampleTier3Chest(rng))
                    let debugSkeleton = false
                    if debugSkeleton then
                        for x,y,z in skel do
                            map.SetBlockIDAndDamage(x,y,z,102uy,0uy) // 102 = glass_pane
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
                                             Strings.BOOK_IN_FINAL_PURPLE_DUNGEON_CHEST
                                             End |]
                                |]
                        putUntrappedChestWithItemsAt(bx,by,bz,Strings.NAME_OF_FINAL_PURPLE_DUNGEON_CHEST,chestItems,map,null)
                    else // no reason for side paths in final dungeon
                        // make side paths with extra loot
                        printfn "computing paths endpoints -> redstone"
                        let path = new System.Collections.Generic.HashSet<_>(path)
                        let sidePaths = ResizeArray()
                        for distToSkelX,_distToSkelY,distToSkelZ,(ex,ey,ez) in epwl do
                            if distToSkelX+distToSkelZ > 5 then
                                match Algorithms.findShortestPath(ex,ey,ez,(fun (x,y,z) -> skel.Contains(x,y,z)), (fun (x,y,z) -> path.Contains(x,y,z)), DIFFERENCES) with
                                | None -> ()
                                | Some((_sx,_sy,_sz), sidePath, sideMoves) -> 
                                    let mutable numVerticalMoves = 0
                                    for i in sideMoves do
                                        let _dx,dy,_dz = DIFFERENCES.[i]
                                        if dy <> 0 then
                                            numVerticalMoves <- numVerticalMoves + 1
                                    if numVerticalMoves * 3 > sidePath.Count then
                                        () // skip, the skeleton algorithm has some flaws (e.g. ravines) that have useless tall spurs, only accept side paths that are mostly x/z moves
                                    else
                                        sidePaths.Add(sidePath)
                        //sideLengths.Sort()
                        let tes = ResizeArray()
                        for sidePath in sidePaths do
                            let _,spy,_ = sidePath.[0]
                            if spy <= 56 then // an apparent endpoint at a height near 60 may just be where there was a cave opening but the analysis cut off at high y and saw it as endpoint, so ignore high-y endpoints
                                let l = sidePath.Count 
                                if l >= 15 && l <= 40 then
                                    for x,y,z in sidePath do
                                        if debugSkeleton then
                                            map.SetBlockIDAndDamage(x,y,z,160uy,5uy) // 160 = stained_glass_pane
                                        // put stripe on the ground
                                        replaceGroundBelowWith(x,y,z,73uy,0uy)  // 73 = redstone ore (lights up when things walk on it)
                                    // put chest on ground at dead end
                                    let mutable x,y,z = sidePath.[0]
                                    while a.[x,y,z]<>null do
                                        y <- y - 1
                                    y <- y + 1
                                    // TODO probably make a loot table, be more interesting
                                    // TODO sometimes be trap or troll
                                    let F = CustomizationKnobs.LOOT_FUNCTION
                                    let numEmeralds = 1 + rng.Next(F 2)
                                    let chestItems = Compounds[| [| Byte("Count",byte numEmeralds); Byte("Slot",13uy); Short("Damage",0s); String("id","minecraft:emerald"); End |] |]
                                    putTrappedChestWithItemsAt(x,y,z,Strings.NAME_OF_DEAD_END_CHEST_IN_GREEN_DUNGEON, chestItems, map, tes)
                                    log.LogInfo(sprintf "added side path length %d" l)
                        map.AddOrReplaceTileEntities(tes)
    // end foreach CC
    if finalEX = 0 && finalEZ = 0 then
        log.LogSummary("FAILED TO PLACE FINAL")
        failwith "final failed"
////
(* 

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
        "granite",   3, GRANITE_COUNT, 0,  80
        "diorite",  12,120, 0,  80
        "andesite", 33,  0, 0,  80
        "coal",     17, 20, 0, 128
        "iron",      9,  5, 0,  58
        "gold",      9,  5, 0,  62
        "redstone",  3,  REDSTONE_COUNT, 0,  32
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
    //elif map.GetBlockInfo(x,y-1,z).BlockID = 0uy then  // don't place if only visible at y-1, this means it's in the ceiling, and likely can't spawn tall mobs
    //    true
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

// TODO consider eliminating all cobwebs (if so, can change logic that checks cobwebs, and have more control over access to bows)
let substituteBlocks(rng : System.Random, map:MapFolder, log:EventAndProgressLog) =
    let LOX, LOY, LOZ = MINIMUM, 1, MINIMUM
    let HIY = 120
    let spawners1 = SpawnerAccumulator("rand spawners from granite")
    let spawners2 = SpawnerAccumulator("rand spawners from redstone")
    let chestTEs = ResizeArray()
    log.LogInfo("SUBST: substituting blocks...")
    for y = LOY to HIY do
        printf "."
        for x = LOX to LOX+LENGTH-1 do
            for z = LOZ to LOZ+LENGTH-1 do
                let bi = map.MaybeGetBlockInfo(x,y,z)
                if bi <> null then
                    let bid = bi.BlockID 
                    let dmg = bi.BlockData 
                    if bid = 1uy && dmg = 3uy then // diorite ->
                        map.SetBlockIDAndDamage(x,y,z,97uy,0uy) // silverfish monster egg
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
                        if rng.Next(18) = 0 then
                            map.SetBlockIDAndDamage(x,y,z,173uy,0uy) // coal block
                    elif bid = 54uy then // chest // assuming all chests are dungeon chests, no verification
                        chestTEs.Add( blockTE("Chest",x,y,z,Compounds(LootTables.NEWsampleTier2Chest(rng)),Strings.NAME_OF_DEFAULT_MINECRAFT_DUNGEON_CHEST,null,0L) )
                    elif bid = 17uy || bid = 162uy then // log/log2
                        map.SetBlockIDAndDamage(x,y,z,bid,dmg ||| 12uy) // setting bits 4&8 yields 6-sided bark texture, which looks cool
    log.LogSummary("added random spawners underground")
    if CustomizationKnobs.SILVERFISH_LIMITS then
        log.LogInfo("replacing silverfish pockets not exposed to air...")
        for y = LOY to HIY do
            printf "."
            let visited = new System.Collections.Generic.HashSet<_>()  // new foreach Y so set does not get too big
            for x = LOX to LOX+LENGTH-1 do
                for z = LOZ to LOZ+LENGTH-1 do
                    let bi = map.MaybeGetBlockInfo(x,y,z)
                    if bi <> null then
                        let bid = bi.BlockID 
                        if bid = 97uy then // silverfish monster egg
                            let ok(x,y,z) = 
                                x >= LOX && x <= LOX+LENGTH-1 && y >= LOY && y <= HIY && z >= LOZ && z <= LOZ+LENGTH-1 &&
                                not(visited.Contains(x*LENGTH+z))
                            let thisPatch = ResizeArray()
                            let q = new System.Collections.Generic.Queue<_>()
                            q.Enqueue( (x,y,z) )
                            thisPatch.Add( (x,y,z) )
                            visited.Add( x*LENGTH + z ) |> ignore
                            let ALLDIRS = [| 0,0,1; 0,1,0; 1,0,0; 0,0,-1; (* 0,-1,0; *) -1,0,0 |]  // outermost loop is ascending Y, so never need to go down
                            let mutable nearbyAir = false
                            while not (q.Count=0) do
                                let cx,cy,cz = q.Dequeue()
                                for dx,dy,dz in ALLDIRS do
                                    let nx,ny,nz = cx+dx, cy+dy, cz+dz
                                    if ok(nx,ny,nz) then
                                        let bi = map.MaybeGetBlockInfo(nx,ny,nz)
                                        if bi = null then
                                            nearbyAir <- true
                                        else
                                            let bid = bi.BlockID 
                                            if bid = 0uy then
                                                nearbyAir <- true
                                            elif bid = 97uy then
                                                q.Enqueue( (nx,ny,nz) )
                                                thisPatch.Add( (nx,ny,nz) )
                                                visited.Add( nx*LENGTH + nz ) |> ignore
                            if not nearbyAir then
                                for x,y,z in thisPatch do
                                    map.SetBlockIDAndDamage(x,y,z,1uy,5uy) // andesite
        log.LogInfo("done replacing airless silverfish pockets!")
    spawners1.AddToMapAndLog(map,log)
    spawners2.AddToMapAndLog(map,log)
    map.AddOrReplaceTileEntities(chestTEs)
    printfn "substituting MinecartChest loot..."
    for rx in [-2..1] do
        for rz in [-2..1] do
            let r = map.GetRegion(rx*512,rz*512)
            printfn "%d %d" rx rz
            for cx = 0 to 31 do
                for cz = 0 to 31 do
                    let chunk = r.GetChunk(cx,cz)
                    let a = match chunk with Compound(_,rsa) -> match rsa.[0] with Compound(_,a) -> a
                    let mutable found = false
                    let mutable i = 0
                    while not found && i < a.Count-1 do
                        match a.[i] with
                        | List("Entities",Compounds(existingEs)) ->
                            found <- true
                            for j = 0 to existingEs.Length-1 do
                                let ent = existingEs.[j]
                                if ent |> Array.exists (function String("id","MinecartChest") -> true | _ -> false) then
                                    for k = 0 to ent.Length-1 do
                                        match ent.[k] with
                                        | String("LootTable",_) ->
                                            ent.[k] <- List("Items", Compounds(LootTables.NEWsampleTier2Chest(rng)))
                                        | Long("LootTableSeed",_) ->
                                            ent.[k] <- String("DummyTag","unused")
                                        | _ -> ()
                                    //printfn "found MC near %d %d" (rx*512+cx*16) (rz*512+cz*16)
                        | _ -> ()
                        i <- i + 1
    printfn "...done!"

let replaceSomeBiomes(rng : System.Random, map:MapFolder, log:EventAndProgressLog, biome:_[,], allTrees:ContainerOfMCTrees) =
    let plains = Array2D.zeroCreateBased MINIMUM MINIMUM LENGTH LENGTH
    let desert = Array2D.zeroCreateBased MINIMUM MINIMUM LENGTH LENGTH
    let OKR = DAYLIGHT_RADIUS + 32  // want to give buffer to reduce chance standing in daylight and spawning ghasts in nearby hell biome
    // find plains biomes
    for x = MINIMUM to MINIMUM+LENGTH-1 do
        for z = MINIMUM to MINIMUM+LENGTH-1 do
            let b = biome.[x,z]
            if b = 1uy then // 1 = Plains
                plains.[x,z] <- new Partition(new Thingy(0,(x*x+z*z<OKR*OKR),false))
            elif b = 2uy || b = 17uy then // desert, deserthills
                desert.[x,z] <- new Partition(new Thingy(0,(x*x+z*z<OKR*OKR),false))
    // connected-components them
    for x = MINIMUM to MINIMUM+LENGTH-2 do
        for z = MINIMUM to MINIMUM+LENGTH-2 do
            if plains.[x,z] <> null && plains.[x,z+1] <> null then
                plains.[x,z].Union(plains.[x,z+1])
            if plains.[x,z] <> null && plains.[x+1,z] <> null then
                plains.[x,z].Union(plains.[x+1,z])
            if desert.[x,z] <> null && desert.[x,z+1] <> null then
                desert.[x,z].Union(desert.[x,z+1])
            if desert.[x,z] <> null && desert.[x+1,z] <> null then
                desert.[x,z].Union(desert.[x+1,z])
    let plainsCCs = new System.Collections.Generic.Dictionary<_,_>()
    let desertCCs = new System.Collections.Generic.Dictionary<_,_>()
    for x = MINIMUM to MINIMUM+LENGTH-1 do
        for z = MINIMUM to MINIMUM+LENGTH-1 do
            if plains.[x,z] <> null then
                let rep = plains.[x,z].Find()
                if not rep.Value.IsLeft then  // only find plains completely outside OKR
                    if not(plainsCCs.ContainsKey(rep)) then
                        plainsCCs.Add(rep, new System.Collections.Generic.HashSet<_>())
                    plainsCCs.[rep].Add( (x,z) ) |> ignore
            if desert.[x,z] <> null then
                let rep = desert.[x,z].Find()
                if not rep.Value.IsLeft then  // only find desert completely outside OKR
                    if not(desertCCs.ContainsKey(rep)) then
                        desertCCs.Add(rep, new System.Collections.Generic.HashSet<_>())
                    desertCCs.[rep].Add( (x,z) ) |> ignore
    let tooSmall = ResizeArray()
    for KeyValue(k,v) in plainsCCs do
        if v.Count < 1000 then
            tooSmall.Add(k)
    for k in tooSmall do
        plainsCCs.Remove(k) |> ignore
    let tooSmall = ResizeArray()
    for KeyValue(k,v) in desertCCs do
        if v.Count < 1000 then
            tooSmall.Add(k)
    for k in tooSmall do
        desertCCs.Remove(k) |> ignore
    log.LogInfo(sprintf "found %d decent-sized plains biomes outside OKR" plainsCCs.Count)
    log.LogInfo(sprintf "found %d decent-sized desert biomes outside OKR" desertCCs.Count)
    // preprocess trees
    if allTrees = null then
        printfn "allTrees WAS NULL, SKIPPING TREE REDO"
    let mutable hellBiomePlainsCount, skyBiomePlainsCount, hellPlainsTreeCount, skyPlainsTreeCount = 0,0,0,0
    let mutable hellBiomeDesertCount, hellDesertTreeCount = 0,0
    for KeyValue(_k,v) in plainsCCs do
        if rng.NextDouble() < BIOME_HELL_PERCENTAGE then
            for x,z in v do
                map.SetBiome(x,z,8uy) // 8 = Hell
                biome.[x,z] <- 8uy
                if allTrees <> null then
                    hellPlainsTreeCount <- hellPlainsTreeCount + allTrees.Replace(x,z,112uy,0uy,87uy,0uy,map)// 112=nether_brick, 87=netherrack
            hellBiomePlainsCount <- hellBiomePlainsCount + 1
        elif rng.NextDouble() < BIOME_SKY_PERCENTAGE then
            for x,z in v do
                map.SetBiome(x,z,9uy) // 9 = Sky
                biome.[x,z] <- 9uy
                if allTrees <> null then
                    skyPlainsTreeCount <- skyPlainsTreeCount + allTrees.Replace(x,z,49uy,0uy,120uy,0uy,map)// 49=obsidian, 120=end_portal_frame
            skyBiomePlainsCount <- skyBiomePlainsCount + 1
    for KeyValue(_k,v) in desertCCs do
        if rng.NextDouble() < BIOME_HELL_PERCENTAGE then
            for x,z in v do
                map.SetBiome(x,z,8uy) // 8 = Hell
                biome.[x,z] <- 8uy
                if allTrees <> null then
                    hellDesertTreeCount <- hellDesertTreeCount + allTrees.Replace(x,z,112uy,0uy,87uy,0uy,map)// 112=nether_brick, 87=netherrack
                for y = 128 downto 56 do
                    let bi = map.GetBlockInfo(x,y,z)
                    if bi.BlockID = 12uy then // 12=sand
                        map.SetBlockIDAndDamage(x,y,z,12uy,1uy) //12,1=red sand
                    elif bi.BlockID = 24uy then // 24=sandstone
                        map.SetBlockIDAndDamage(x,y,z,179uy,bi.BlockData) //179=red sandstone (keep block data, which is variant, e.g. smooth)
                    elif bi.BlockID = 44uy && bi.BlockData = 1uy then // 44,1=lower sandstone slab
                        map.SetBlockIDAndDamage(x,y,z,182uy,0uy) //182,0=lower red sandstone slab
                    elif bi.BlockID = 44uy && bi.BlockData = 9uy then // 44,9=upper sandstone slab
                        map.SetBlockIDAndDamage(x,y,z,182uy,8uy) //182,8=upper red sandstone slab
                    elif bi.BlockID = 128uy then // 128=sandstone stairs
                        map.SetBlockIDAndDamage(x,y,z,180uy,bi.BlockData) //180=red sandstone stairs (keep block data, which is orientation)
            hellBiomeDesertCount <- hellBiomeDesertCount + 1
    log.LogSummary(sprintf "Added %d Hell biomes (%d trees) and %d Sky biomes (%d trees) replacing some Plains" hellBiomePlainsCount hellPlainsTreeCount skyBiomePlainsCount skyPlainsTreeCount)
    log.LogSummary(sprintf "Added %d Hell biomes (%d trees) replacing some Desert" hellBiomeDesertCount hellDesertTreeCount)

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
    // pick highest in each CC    // TODO consider all local maxima? right now the hmDiffPerCC gives some alternatives that are ok
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
    let highPoints = highPoints |> Seq.filter (fun ((hx,hz),_,_) -> hx > MINIMUM+32 && hx < MINIMUM+LENGTH-32 && hz > MINIMUM+32 && hz < MINIMUM+LENGTH-32) // not at edge of bounds
    // find the 'best' ones based on which have lots of high ground near them
    let score(x,z) =
        let mutable s = 0
        let D = bestNearbyDist
        for a = x-D to x+D do
            for b = z-D to z+D do
                s <- s + heightMap.[a,b] - (heightMap.[x,z]-20)  // want high ground nearby, but not a huge narrow spike above moderately high ground
        // with this, higher is not better; a great hill always score higher than a very good tall mountain
        s
    let distance2(a,b,c,d) = (a-c)*(a-c)+(b-d)*(b-d)
    let bestHighPoints = ResizeArray()
    for ((hx,hz),a,b) in highPoints |> Seq.sortByDescending (fun (p,_,_) -> score p) do
        if bestHighPoints |> Seq.forall (fun ((ex,ez),_,_,_s) -> distance2(ex,ez,hx,hz) > STRUCTURE_SPACING*STRUCTURE_SPACING) then
            if decorations |> Seq.forall (fun (_,ex,ez) -> distance2(ex,ez,hx,hz) > DECORATION_SPACING*DECORATION_SPACING) then
                bestHighPoints.Add( ((hx,hz),a,b,score(hx,hz)) )
    bestHighPoints  // [(point, lo-bound-of-CC, hi-bound-of-CC, score)]

let HIDDEN_DEPTH = 10
let findHidingSpot(map:MapFolder,hmIgnoringLeaves:_[,],((highx,highz),(minx,minz),(maxx,maxz),_)) =
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
    for y = hmIgnoringLeaves.[highx,highz] downto 80 do // y is outermost loop to prioritize finding high points first
        printf "."
        if not found then
            for z = minz to maxz do
                if not found then
                    for x = minx to maxx do
                        if not found then
                            let D = HIDDEN_DEPTH
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
                                let h = hmIgnoringLeaves.[x,z]
                                if y > h-D-3 && y < h-D+1 then  // ensure not too deep, or under floating island, or whatnot... depth should be a little more than D
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

let findSomeMountainPeaks(rng : System.Random, map:MapFolder,hm:_[,],hmIgnoringLeaves, log:EventAndProgressLog, biome:_[,], decorations:ResizeArray<_>, allTrees:ContainerOfMCTrees) =
    let RADIUS = MOUNTAIN_PEAK_DANGER_RADIUS
    let isMegaTaigaOrJungle(b) =  // tall trees in these biomes confuse the 'peak' finding algorithm, so avoid them
        b = 32uy || b = 33uy || b = 160uy || b = 161uy || 
        b = 21uy || b = 22uy || b = 23uy || b = 149uy || b = 151uy
    let computeBestHighPoints(minH) =
        let bestHighPoints = findBestPeaksAlgorithm(hmIgnoringLeaves,minH,minH+20,10,6,decorations)
        let bestHighPoints = bestHighPoints |> Seq.filter (fun ((_x,_z),_,_,s) -> s > 0)  // negative scores often mean tall spike with no nearby same-height ground, get rid of them
        let bestHighPoints = bestHighPoints |> Seq.filter (fun ((x,z),_,_,_) -> x*x+z*z > SPAWN_PROTECTION_DISTANCE_PEAK*SPAWN_PROTECTION_DISTANCE_PEAK)
        let bestHighPoints = bestHighPoints |> Seq.filter (fun ((x,z),_,_,_) -> x > MINIMUM+RADIUS && z > MINIMUM + RADIUS && x < MINIMUM+LENGTH-RADIUS-1 && z < MINIMUM+LENGTH-RADIUS-1)
        let bestHighPoints = bestHighPoints |> Seq.filter (fun ((x,z),_,_,_) -> not(isMegaTaigaOrJungle(biome.[x,z])))
        bestHighPoints
    let bestHighPoints = 
        let mutable r = computeBestHighPoints(90)
        if r |> Seq.length < 8 then
            printfn "did not find enough peaks, trying again lower"
            r <- computeBestHighPoints(80)
            if r |> Seq.length < 8 then
                printfn "did not find enough peaks, trying again even lower this time"
                r <- computeBestHighPoints(70)
        r
    ////////////////////////////////////////////////
    // best hiding spot
    let timer = System.Diagnostics.Stopwatch.StartNew()
    printfn "find best hiding spot..."
    let ((bx,by,bz),(usedX,usedZ)) = bestHighPoints |> Seq.choose (fun x -> findHidingSpot(map,hmIgnoringLeaves,x)) |> Seq.maxBy (fun ((_,y,_),_) -> y)
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
                Strings.QUADRANT_NORTHWEST
            else
                Strings.QUADRANT_SOUTHWEST 
        else
            if finalEZ < 0 then 
                Strings.QUADRANT_NORTHEAST 
            else
                Strings.QUADRANT_SOUTHEAST 
    let chestItems = 
        Compounds[| 
                for slot in [12uy..14uy] do
                    yield [| Byte("Count",1uy); Byte("Slot",slot); Short("Damage",0s); String("id","minecraft:elytra"); Compound("tag",[|
                                List("ench",Compounds[|[|Short("id",2s);Short("lvl",10s);End|]|]);End|]|>ResizeArray); End |]
                // jump boost pots
                for slot in [3uy;4uy;5uy] do
                    yield [| Byte("Count",24uy); Byte("Slot",slot); Short("Damage",0s); String("id","minecraft:splash_potion"); Compound("tag",[|
                                Strings.NameAndLore.SUPER_JUMP_BOOST
                                List("CustomPotionEffects",Compounds[|[|Byte("Id",8uy);Byte("Amplifier",39uy);Int("Duration",300);End|]|]);End|]|>ResizeArray); End |]
                yield [| Byte("Count",1uy); Byte("Slot",21uy); Short("Damage",0s); String("id","minecraft:written_book"); 
                         Strings.BOOK_IN_HIDING_SPOT(quadrant); End |]
                yield [| Byte("Count",1uy); Byte("Slot",23uy); Short("Damage",0s); String("id","minecraft:written_book"); 
                         Strings.BOOK_WITH_ELYTRA; End |]
            |]
    putUntrappedChestWithItemsAt(bx,by,bz,Strings.NAME_OF_HIDDEN_TREASURE_CHEST,chestItems,map,null)
    map.SetBlockIDAndDamage(bx,by-1,bz,89uy,0uy) // 89=glowstone
    // put a tiny mark on the surface
    do
        // ..remove entirety of nearby trees (because having the orchid not placed, or atop a tree, is confusing/unhelpful)
        if allTrees = null then
            printfn "allTrees WAS NULL, SKIPPING TREE REDO"
        else
            for x = bx-10 to bx+10 do
                for z = bz-10 to bz+10 do
                    allTrees.Remove(x,z,map)
        let mutable h = hmIgnoringLeaves.[bx,bz]
        // we just removed trees, so hm may be wrong (atop tree logs), recompute
        while HM_IGNORING_LEAVES_SKIPPABLE_DOWN_BLOCKS.Contains(map.GetBlockInfo(bx,h,bz).BlockID) do
            h <- h - 1
        map.SetBlockIDAndDamage(bx,h+0,bz,3uy,0uy) // 3=dirt
        map.SetBlockIDAndDamage(bx,h+1,bz,38uy,1uy) // 38,1=blue orchid
        for y = by+2 to h-4 do
            map.SetBlockIDAndDamage(bx,y,bz,20uy,0uy)  // column of glass, to make clearer the digging is 'working'

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
                [| Byte("Slot",12uy); Byte("Count",1uy); String("id","purpur_block"); Compound("tag", [|Strings.NameAndLore.MONUMENT_BLOCK_PURPUR;End|] |> ResizeArray); End |]
                [| yield Byte("Slot",14uy); yield! LootTables.makeChestItemWithNBTItems(Strings.NAME_OF_CHEST_ITEM_CONTAINING_MOUNTAIN_PEAK_LOOT,LootTables.NEWsampleTier5Chest(rng)) |]
            |])
        for xx = x-3 to x+3 do
            for zz = z-3 to z+3 do
                map.SetBlockIDAndDamage(xx,y-1,zz,7uy,0uy) // 7=bedrock floor under to prevent cheesing
        map.SetBlockIDAndDamage(x-2,y+4,z-2,76uy,5uy) // 76=redstone_torch
        map.SetBlockIDAndDamage(x-2,y+4,z+2,76uy,5uy) // 76=redstone_torch
        map.SetBlockIDAndDamage(x+2,y+4,z-2,76uy,5uy) // 76=redstone_torch
        map.SetBlockIDAndDamage(x+2,y+4,z+2,76uy,5uy) // 76=redstone_torch
        for i = x-RADIUS to x+RADIUS do
            for j = z-RADIUS to z+RADIUS do
                makeAreaHard(map,i,j)
                if abs(x-i) > 2 || abs(z-j) > 2 then
                    let dist = abs(x-i) + abs(z-j)
                    let pct = float (2*RADIUS-dist) / float(RADIUS*30)
                    // spawners on terrain
                    if rng.NextDouble() < pct*MOUNTAIN_PEAK_DUNGEON_SPAWNER_DATA.DensityMultiplier then
                        let x = i
                        let z = j
                        let y = hm.[x,z]
                        map.SetBlockIDAndDamage(x, y, z, 52uy, 0uy) // 52 = monster spawner
                        let ms = MOUNTAIN_PEAK_DUNGEON_SPAWNER_DATA.NextSpawnerAt(x,y,z,rng)
                        spawners.Add(ms)
                    // red torches for mood lighting
                    elif rng.NextDouble() < pct then
                        let x = i
                        let z = j
                        let y = hm.[x,z]
                        map.SetBlockIDAndDamage(x,y,z,76uy,5uy) // 76=redstone_torch
                        map.SetBlockIDAndDamage(x,y-1,z,1uy,5uy) // 1,5=andesite
        let RADIUS = RADIUS + DAYLIGHT_BEDROCK_BUFFER_RADIUS
        for i = x-RADIUS to x+RADIUS do
            for j = z-RADIUS to z+RADIUS do
                // ceiling over top to prevent cheesing it
                map.SetBlockIDAndDamage(i,y+5,j,7uy,0uy) // 7=bedrock
                hm.[i,j] <- y+6
        spawners.AddToMapAndLog(map,log)
    ()

let findSomeFlatAreas(rng:System.Random, map:MapFolder,hm:_[,],hmIgnoringLeaves:_[,],log:EventAndProgressLog, decorations:ResizeArray<_>) =
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
    let RADIUS = FLAT_COBWEB_DANGER_RADIUS
    let CR = RADIUS+DAYLIGHT_BEDROCK_BUFFER_RADIUS // ceiling radius
    let BEDROCK_HEIGHT = 127
    let bestFlatPoints = bestFlatPoints |> Seq.filter (fun ((x,z),_,_,_s) -> x*x+z*z > SPAWN_PROTECTION_DISTANCE_FLAT*SPAWN_PROTECTION_DISTANCE_FLAT)
    let bestFlatPoints = bestFlatPoints |> Seq.filter (fun ((x,z),_,_,_s) -> x > MINIMUM+CR && z > MINIMUM+CR && x < MINIMUM+LENGTH-CR-1 && z < MINIMUM+LENGTH-CR-1)
    let allFlatPoints = bestFlatPoints |> Seq.toArray 
    let bestFlatPoints, nextBestFlatPoints = 
        if allFlatPoints.Length < 10 then
            allFlatPoints, [| |]
        elif allFlatPoints.Length < 15 then
            allFlatPoints.[0..9], allFlatPoints.[10..]
        else
            allFlatPoints.[0..9], allFlatPoints.[10..14]
    // decorate map with dungeon
    for (x,z),_,_,s in bestFlatPoints do
        decorations.Add('F',x,z)
        log.LogSummary(sprintf "added flat set piece (score %d) at %d %d" s x z)
        let spawners = SpawnerAccumulator("spawners around cobweb flat")
        let y = hm.[x,z]
        if y > BEDROCK_HEIGHT - 10 then
            failwith "unexpected very high flat dungeon"
        putTreasureBoxWithItemsAt(map,x,y,z,[|
                [| Byte("Slot",12uy); Byte("Count",1uy); String("id","end_bricks"); Compound("tag", [|
                        Strings.NameAndLore.MONUMENT_BLOCK_END_STONE_BRICK
                        End
                    |] |> ResizeArray); End |]
                [| yield Byte("Slot",14uy); yield! LootTables.makeChestItemWithNBTItems(Strings.NAME_OF_CHEST_ITEM_CONTAINING_RED_BEACON_WEB_LOOT,LootTables.NEWsampleTier4Chest(rng)) |]
            |])
        map.SetBlockIDAndDamage(x,y+3,z,20uy,0uy) // glass (replace roof of box so beacon works)
        putBeaconAt(map,x,y,z,14uy,false) // 14 = red
        // TODO make these spawners in CustomizationKnobs?
        // add blazes atop
        for (dx,dz) in [-3,-3; -3,3; 3,-3; 3,3] do
            let x,y,z = x+dx, y+5, z+dz
            map.SetBlockIDAndDamage(x, y, z, 52uy, 0uy) // 52 = monster spawner
            let ms = MobSpawnerInfo(x=x, y=y, z=z, BasicMob="Blaze", Delay=1s)
            spawners.Add(ms)
        // add a spider jockey too
        map.SetBlockIDAndDamage(x, y+5, z, 52uy, 0uy) // 52 = monster spawner
        let ms = MobSpawnerInfo(x=x, y=y+5, z=z, BasicMob="Spider", Delay=1s)
        ms.ExtraNbt <- [ List("Passengers",Compounds[| [|String("id","Skeleton"); List("HandItems",Compounds[| [|String("id","bow");Int("Count",1);End|]; [| End |] |]); End|] |] )]
        spawners.Add(ms)
        for dx = -3 to 3 do
            for dz = -3 to 3 do
                map.SetBlockIDAndDamage(x+dx, y+6, z+dz, 7uy, 0uy) // 7 = bedrock ceiling
                if abs(dx)=3 && abs(dz)=3 then
                    map.SetBlockIDAndDamage(x+dx, y+7, z+dz, 76uy, 5uy) // 76=redstone_torch
        // surround with danger
        for i = x-RADIUS to x+RADIUS do
            for j = z-RADIUS to z+RADIUS do
                makeAreaHard(map,i,j)
                if abs(x-i) > 2 || abs(z-j) > 2 then
                    let dist = (x-i)*(x-i) + (z-j)*(z-j) |> float |> sqrt |> int
                    let pct = float (RADIUS-dist/2) / ((float RADIUS) * 2.0)
                    let possibleSpawners = if dist < RADIUS/2 then FLAT_COBWEB_INNER_SPAWNER_DATA else FLAT_COBWEB_OUTER_SPAWNER_DATA 
                    if rng.NextDouble() < pct*possibleSpawners.DensityMultiplier then
                        let x = i
                        let z = j
                        let y = hm.[x,z] + rng.Next(2)
                        if rng.Next(12+dist/2) = 0 then
                            map.SetBlockIDAndDamage(x, y, z, 52uy, 0uy) // 52 = monster spawner
                            let ms = possibleSpawners.NextSpawnerAt(x,y,z,rng)
                            spawners.Add(ms)
                        elif rng.Next(3) = 0 then
                            map.SetBlockIDAndDamage(x, y, z, 30uy, 0uy) // 30 = cobweb
                    elif rng.Next(60) = 0 then
                        map.SetBlockIDAndDamage(i,hm.[i,j],j,76uy,5uy) // 76=redstone_torch
                        map.SetBlockIDAndDamage(i,hm.[i,j]-1,j,1uy,5uy) // 1,5=andesite
        // place some obsidian below ground to make it harder to tunnel underneath
        for i = x-RADIUS to x+RADIUS do
            for j = z-RADIUS to z+RADIUS do
                if abs(x-i) > 3 || abs(z-j) > 3 then  // don't overwrite the beacon/bedrock!
                    let dist = (x-i)*(x-i) + (z-j)*(z-j) |> float |> sqrt |> int
                    let pct = float (RADIUS-dist) / (float RADIUS)
                    let belowGround = hmIgnoringLeaves.[i,j] - 2
                    for y = belowGround downto belowGround-7 do
                        if (i+y+j)%2 = 0 then
                            if rng.NextDouble() < pct then
                                map.SetBlockIDAndDamage(i,y,j,49uy,0uy) // 49=obsdian
        spawners.AddToMapAndLog(map,log)
        for i = x-CR to x+CR do
            for j = z-CR to z+CR do
                map.SetBlockIDAndDamage(i,BEDROCK_HEIGHT,j,7uy,0uy) // 7 = bedrock
                hm.[i,j] <- BEDROCK_HEIGHT
    // decorate map with set piece
    let nextBestFlatPoints = [] // TODO these are not ready for primetime. bedrock ceiling not big enough, area not flat enough, loot/mobs not engaging enough, just not good.
    for (cx,cz),_,_,s in nextBestFlatPoints do
        // TODO alternate mob/loot loadouts
        // TODO other loot in chest?
        decorations.Add('S',cx,cz)
        log.LogSummary(sprintf "added set piece (score %d) at %d %d" s cx cz)
        let spawners = SpawnerAccumulator("spawners around set piece")
        let ROUT,RMID = 11,7
        let y = hm.[cx,cz] 
        if y > BEDROCK_HEIGHT - 18 then
            failwith "unexpected very high flat set piece"
        for x = cx-ROUT to cx+ROUT do
            for z = cz-ROUT to cz+ROUT do
                if ((x=cx-ROUT || x=cx+ROUT) && (z<=cz-RMID+3 || z>=cz+RMID-3)) ||
                   ((z=cz-ROUT || z=cz+ROUT) && (x<=cx-RMID+3 || x>=cx+RMID-3)) then
                    for y = hm.[x,z] to hm.[x,z]+7 do
                        map.SetBlockIDAndDamage(x,y,z,20uy,0uy) // 20=glass
                if ((x=cx-RMID || x=cx+RMID) && (z>=cz-RMID+3 && z<=cz+RMID-3)) ||
                   ((z=cz-RMID || z=cz+RMID) && (x>=cx-RMID+3 && x<=cx+RMID-3)) then
                    for y = hm.[x,z] to hm.[x,z]+7 do
                        map.SetBlockIDAndDamage(x,y,z,20uy,0uy) // 20=glass
                makeAreaHard(map,x,z)
        let F = CustomizationKnobs.LOOT_FUNCTION
        let numEmeralds = 1 + rng.Next(F 2)
        let chestItems = // smite V diamond sword
            [| [| Byte("Count", 1uy); Byte("Slot", 13uy); Short("Damage",0s); String("id","minecraft:diamond_sword"); Compound("tag", [|List("ench",Compounds[|[|Short("id",17s);Short("lvl",5s);End|]|]); End |] |> ResizeArray); End |]
               [| Byte("Count", byte numEmeralds); Byte("Slot", 22uy); Short("Damage",0s); String("id","minecraft:emerald"); End |] |]
        putTreasureBoxAtCore(map,cx,y+4,cz,null,0L,chestItems,49uy,0uy,20uy,0uy,1) // 49=obsidian, 20=glass
        // TODO make mobs below in CustomizationKnobs?
        for x,z in [cx-2,cz-2; cx-2,cz+2; cx+2,cz-2; cx+2,cz+2] do
            runCommandBlockOnLoadSelfDestruct(x,hm.[x,z]+1,z,map,"summon Skeleton ~ ~1 ~ {HandItems:[{id:iron_sword,Count:1},{}],SkeletonType:1b,PersistenceRequired:1b}")
        for x,z in [cx-2,cz; cx+2,cz] do
            runCommandBlockOnLoadSelfDestruct(x,hm.[x,z]+1,z,map,"summon Witch ~ ~1 ~ {PersistenceRequired:1b}")
        // TODO make spawners below in CustomizationKnobs?
        for x,z in [cx-RMID,cz-RMID; cx-RMID,cz+RMID; cx+RMID,cz-RMID; cx+RMID,cz+RMID] do
            map.SetBlockIDAndDamage(x,hm.[x,z],z,76uy,5uy) // 76=redstone_torch
            map.SetBlockIDAndDamage(x, hm.[x,z]+1, z, 52uy, 0uy) // 52 = monster spawner
            spawners.Add(FLAT_SET_PIECE_SPAWNER_DATA.NextSpawnerAt(x,hm.[x,z]+1,z,rng))
            map.SetBlockIDAndDamage(x, y+16, z, 52uy, 0uy) // 52 = monster spawner
            spawners.Add(new MobSpawnerInfo(x=x,y=y+16,z=z,BasicMob="Ghast",Delay=1s)) // "ceiling protection" to dissuade cheesing it from above
        spawners.AddToMapAndLog(map,log)
        for x = cx-ROUT to cx+ROUT do
            for z = cz-ROUT to cz+ROUT do
                map.SetBlockIDAndDamage(x,BEDROCK_HEIGHT,z,7uy,0uy) // 7 = bedrock
                hm.[x,z] <- BEDROCK_HEIGHT
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

let addRandomLootz(rng:System.Random, map:MapFolder,log:EventAndProgressLog,hm:_[,],hmIgnoringLeaves:_[,],biome:_[,],decorations:ResizeArray<_>,allTrees:ContainerOfMCTrees,colorCount:_[]) =
    printfn "add random loot chests..."
    let tileEntities = ResizeArray()
    let lootLocations = ResizeArray()
    let names = Array.create 16 ""
    let points = Array.init 16 (fun _x -> ResizeArray())
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
    let isFlowingWater(nbi:BlockInfo) = 
        nbi.BlockID = 9uy && nbi.BlockData <> 0uy || nbi.BlockID = 8uy // flowing water
    let putTrappedChestWithLoot(color,x,y,z,tier) =
        let items = if tier = 1 then LootTables.NEWaestheticTier1Chest(rng,color)
                    elif tier = 2 then LootTables.NEWaestheticTier2Chest(rng,color)
                    elif tier = 3 then LootTables.NEWaestheticTier3Chest(rng,color)
                    else failwith "bad aesthetic tier"
        putTrappedChestWithItemsAt(x,y,z,Strings.NAME_OF_GENERIC_TREASURE_BOX,Compounds(items),map,tileEntities)
        lootLocations.Add(x,y,z)
    let putFurnaceWithLoot(color,x,y,z) =
        // can hold up to 3 items
        let items = [|  yield [| String("id","minecraft:emerald"); Byte("Count", 1uy); Short("Damage",0s); End |]
                        yield LootTables.makeMultiBook(rng)
                        if CustomizationKnobs.KURT_SPECIAL then
                            if color <> -1 then
                                yield [| Byte("Count", 1uy); Short("Damage",int16(color)); String("id","minecraft:stained_glass"); Compound("tag", [|Strings.NameAndLore.BONUS_ACTUAL; End|]|>ResizeArray); End |]
                    |] |> LootTables.addSlotTags
        putFurnaceWithItemsAt(x,y,z,Strings.NAME_OF_GENERIC_TREASURE_BOX,Compounds(items),map,tileEntities)
        lootLocations.Add(x,y,z)
    let flowingWaterVisited = new System.Collections.Generic.HashSet<_>()
    let waterfallTopVisited = new System.Collections.Generic.HashSet<_>()
    // TODO consider fun names for each kind of chest (a la /help command)
    let BUFFER = 2 // to ensure we don't out-of-bounds in a number of various basic checks
    for x = MINIMUM+BUFFER to MINIMUM+LENGTH-1-BUFFER do
        if x%200 = 0 then
            printfn "%d" x
        for z = MINIMUM+BUFFER to MINIMUM+LENGTH-1-BUFFER do
            let mutable nearDecoration = false
            for _,dx,dz in decorations do
                if (x-dx)*(x-dx) + (z-dz)*(z-dz) < RANDOM_LOOT_SPACING_FROM_PRIOR_DECORATION*RANDOM_LOOT_SPACING_FROM_PRIOR_DECORATION then
                    nearDecoration <- true
            if not nearDecoration && (abs(x) > 25 || abs(z) > 25) then  // don't put near other things or right near spawn
                for y = 90 downto 59 do
                    let bi = map.GetBlockInfo(x,y,z)
                    let bid = bi.BlockID 
                    if bid = 48uy && checkForPlus(x,y,z,0uy,48uy) then // 48 = moss stone
                        // is a '+' of moss stone with air, e.g. surface boulder in mega taiga
                        if rng.Next(5) = 0 then // TODO probability, so don't place on all
                            if noneWithin(50,points.[0],x,y,z) then
                                let x = if rng.Next(2) = 0 then x-1 else x+1
                                let z = if rng.Next(2) = 0 then z-1 else z+1
                                putTrappedChestWithLoot(0,x,y,z,1)
                                points.[0].Add( (x,y,z) )
                                names.[0] <- "moss stone boulder"
                    elif bid = 18uy && checkForPlus(x,y,z,0uy,18uy) 
                         || bid = 161uy && checkForPlus(x,y,z,0uy,161uy) then // 18=leaves, 161=leaves2
                        // is a '+' of leaves with air, e.g. tree top
                        if rng.Next(20) = 0 then // TODO probability, so don't place on all
                            let x = if rng.Next(2) = 0 then x-1 else x+1
                            let z = if rng.Next(2) = 0 then z-1 else z+1
                            if map.GetBlockInfo(x,y-1,z).BlockID = 18uy || map.GetBlockInfo(x,y-1,z).BlockID = 161uy then // only if block below would be leaf
                                if noneWithin(150,points.[1],x,y,z) && noneWithin(150,points.[7],x,y,z) then
                                    putTrappedChestWithLoot(1,x,y,z,1)
                                    points.[1].Add( (x,y,z) )
                                    names.[1] <- "tree top leaves"
                    elif bid = 86uy then // 86 = pumpkin
                        let dmg = bi.BlockData
                        if rng.Next(4) = 0 then // TODO probability, so don't place on all
                            // TODO could be on hillside, and so chest under maybe exposed
                            if noneWithin(50,points.[2],x,y,z) then
                                map.SetBlockIDAndDamage(x,y,z,91uy,dmg) // 91=lit_pumpkin  // TODO found one, was not giving off light, hm
                                // chest below
                                let y = y - 1
                                putTrappedChestWithLoot(2,x,y,z,2)
                                points.[2].Add( (x,y,z) )
                                names.[2] <- "pumpkin patch"
                    elif y>63 && bid = 9uy && bi.BlockData = 0uy then  // water falling straight down has different damage value, only want sources
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
                                        putTrappedChestWithLoot(3,x,y,z,2)
                                        points.[3].Add( (x,y,z) )
                                        names.[3] <- "surface lake"
                    elif bid = 12uy then // 12=sand
                        if y >= hm.[x,z]-1 then // at top of heightmap (-1 because surface is actually just below heightmap)
                            let deserts = [| 2uy; 17uy; 130uy |]
                            if deserts |> Array.exists (fun b -> b = biome.[x,z]) then
                                if checkForPlus(x,y,z,12uy,12uy) then // flat square of sand
                                    let mutable ok = true
                                    // cacti need to not be placed next to blocks, we need some clear air here
                                    for xx = x-2 to x+2 do
                                        for zz = z-2 to z+2 do
                                            if map.GetBlockInfo(xx,y+1,zz).BlockID <> 0uy || map.GetBlockInfo(xx,y+2,zz).BlockID <> 0uy then
                                                ok <- false
                                    if ok && rng.Next(20) = 0 then // TODO probability, so don't place on all
                                        if noneWithin(150,points.[4],x,y,z) then
                                            let y = y + 1
                                            // put cactus
                                            for dy = 0 to 1 do
                                                map.SetBlockIDAndDamage(x+1,y+dy,z+1,81uy,0uy)  // cactus
                                                map.SetBlockIDAndDamage(x+1,y+dy,z-1,81uy,0uy)  // cactus
                                                map.SetBlockIDAndDamage(x-1,y+dy,z-1,81uy,0uy)  // cactus
                                                map.SetBlockIDAndDamage(x-1,y+dy,z+1,81uy,0uy)  // cactus
                                            // put chest
                                            putTrappedChestWithLoot(4,x,y,z,1)
                                            points.[4].Add( (x,y,z) )
                                            names.[4] <- "desert cactus"
                                            // TODO sometimes be a trap
                    elif y>63 && isFlowingWater(bi) then
                        if not(flowingWaterVisited.Contains(x,y,z)) then
                            flowingWaterVisited.Add(x,y,z) |> ignore
                            let q = new System.Collections.Generic.Queue<_>()
                            q.Enqueue(x,y,z)
                            while not(q.Count=0) do
                                let cx,cy,cz = q.Dequeue()
                                let isValid(coord) = coord >= MINIMUM+6 && coord <= MINIMUM+LENGTH-1-6 // the 6s are because once found we'll try to recess a torch a few blocks away, don't want to out-of-bounds with waterfall right near world edge
                                for dx,dy,dz in [1,0,0; -1,0,0; 0,0,1; 0,0,-1; 0,1,0] do
                                    let nx,ny,nz = cx+dx, cy+dy, cz+dz
                                    if isValid(nx) && isValid(nz) then
                                        if not(flowingWaterVisited.Contains(nx,ny,nz)) && not(waterfallTopVisited.Contains(nx,ny,nz)) then
                                            let nbi = map.GetBlockInfo(nx,ny,nz)
                                            if isFlowingWater(nbi) then
                                                flowingWaterVisited.Add(nx,ny,nz) |> ignore
                                                q.Enqueue(nx,ny,nz)
                                            elif nbi.BlockID = 9uy && nbi.BlockData = 0uy then  // stationary water
                                                waterfallTopVisited.Add(nx,ny,nz) |> ignore
                                                if hmIgnoringLeaves.[cx,cz] <= cy+1 then
                                                    if map.GetBlockInfo(nx,ny+1,nz).BlockID <> 0uy then
                                                        let dx,dz = 
                                                            if isFlowingWater(map.GetBlockInfo(nx+1,ny,nz)) then -1,0
                                                            elif isFlowingWater(map.GetBlockInfo(nx-1,ny,nz)) then 1,0
                                                            elif isFlowingWater(map.GetBlockInfo(nx,ny,nz+1)) then 0,-1
                                                            elif isFlowingWater(map.GetBlockInfo(nx,ny,nz-1)) then 0,1
                                                            else 99,99
                                                        if dx <> 99 then
                                                            let mutable ok = true
                                                            let isOkBlock(bi:BlockInfo) = 
                                                                bi.BlockID = 1uy || bi.BlockID = 3uy || bi.BlockID = 97uy // stone or dirt or monster_egg
                                                            for i = 1 to 5 do
                                                                if not(isOkBlock(map.GetBlockInfo(nx+i*dx,ny,nz+i*dz))) ||
                                                                    not(isOkBlock(map.GetBlockInfo(nx+i*dx,ny+1,nz+i*dz))) ||
                                                                    not(isOkBlock(map.GetBlockInfo(nx+i*dx,ny-1,nz+i*dz))) ||
                                                                    not(isOkBlock(map.GetBlockInfo(nx+i*dx+abs dz,ny,nz+i*dz+abs dx))) ||
                                                                    not(isOkBlock(map.GetBlockInfo(nx+i*dx-abs dz,ny,nz+i*dz-abs dx))) then
                                                                        ok <- false
                                                            if ok then
                                                                //printfn "found waterfall top at %d %d %d" nx ny nz
                                                                map.SetBlockIDAndDamage(nx,ny+1,nz,1uy,4uy) // 1,4 is polished diorite
                                                                for i = 1 to 3 do
                                                                    map.SetBlockIDAndDamage(nx+i*dx,ny-1,nz+i*dz,0uy,0uy) // air
                                                                map.SetBlockIDAndDamage(nx+4*dx,ny-1,nz+4*dz,50uy,5uy) // 50,5=torch attached at bottom
                                                                // chest below
                                                                putTrappedChestWithLoot(5,nx,ny-1,nz,2)
                                                                points.[5].Add( (nx,ny-1,nz) )
                                                                names.[5] <- "waterfall top"
                                                            else
                                                                ()//printfn "ignoring waterfall top at %d %d %d because couldn't recess torch" nx ny nz
                                                        else
                                                            ()//printfn "ignoring waterfall top at %d %d %d because water is weird" nx ny nz
                                                    else
                                                        ()//printfn "ignoring waterfall top at %d %d %d because air above source" nx ny nz
                                                else
                                                    ()//printfn "ignoring waterfall top at %d %d %d because underground" nx ny nz // no harm in placing chests there, but probably no one will find them, prefer to have count of findable ones; in one map, 24 of 72 waterfall tops were on surface
                    elif bid = 100uy && bi.BlockData = 5uy then  // 100=red_mushroom_block, 5=red only on top-center
                        if rng.Next(3) = 0 then // TODO probability, so don't place on all
                            if noneWithin(120,points.[6],x,y,z) then
                                putTrappedChestWithLoot(6,x,y,z,1)
                                points.[6].Add( (x,y,z) )
                                names.[6] <- "red mushroom top"
                    elif bid = 2uy && map.GetBlockInfo(x,y+1,z).BlockID = 0uy && checkForPlus(x,y,z,2uy,2uy) && checkForPlus(x,y+1,z,0uy,0uy) then
                        // 3x3 of grass with air above
                        let b = biome.[x,z]
                        if b = 4uy || b = 27uy || b = 29uy then // forest/birch/roofed
                            // TODO these probabilities can cause wild swings in variance; a better system would gather all the candidates, and try to smooth out the results over all the buckets somehow
                            // e.g. have each bucket have an xyz candidate placement and a function to run to modify the map, and then could choose a candidate from the least-populated
                            // bucket, and run it, until candidates exhausted or reach some maximum (ensure xyz are randomized across map)
                            if rng.Next(50) = 0 then // TODO probability, so don't place on all
                                if noneWithin(150,points.[7],x,y,z) && noneWithin(150,points.[1],x,y,z) then
                                    for dx in [-1..1] do
                                        for dz in [-1..1] do
                                            map.SetBlockIDAndDamage(x+dx,y,z+dz,3uy,1uy) //3,1=coarse dirt
                                    putTrappedChestWithLoot(7,x,y-1,z,1)
                                    points.[7].Add( (x,y-1,z) )
                                    names.[7] <- "forest buried"
                    elif bid = 24uy && checkForPlus(x,y,z,44uy,44uy) then // 24=sandstone, 44=slab - top of desert well
                        printfn "DESERT WELL %d %d %d" x y z
                        // do 100% of them, they're very rare
                        // 5 down chest, 4 more down torch
                        for i = 1 to 4 do
                            map.SetBlockIDAndDamage(x+1,y-5-i,z,24uy,0uy)
                            map.SetBlockIDAndDamage(x-1,y-5-i,z,24uy,0uy)
                            map.SetBlockIDAndDamage(x,y-5-i,z+1,24uy,0uy)
                            map.SetBlockIDAndDamage(x,y-5-i,z-1,24uy,0uy)
                            map.SetBlockIDAndDamage(x,y-5-i,z,0uy,0uy)
                        map.SetBlockIDAndDamage(x,y-5-4,z,50uy,5uy)
                        map.SetBlockIDAndDamage(x,y-5-5,z,24uy,0uy)
                        putTrappedChestWithLoot(8,x,y-5,z,2)
                        points.[8].Add( (x,y-5,z) )
                        names.[8] <- "rare desert well"
                    elif bid = 11uy && hm.[x,z] <= y+1 then // 11=lava at surface
                        for dx,dz in [0,1; 1,0; 0,-1; -1,0] do
                            if noneWithin(150,points.[9],x,y,z) then // in the loop so stop after placing one
                                let mutable ok = true
                                for i = 1 to 3 do
                                    // 3 more lava on one side
                                    if map.GetBlockInfo(x+i*dx, y, z+i*dz).BlockID <> 11uy then
                                        ok <- false
                                    // stone on other side
                                    if map.GetBlockInfo(x-dx, y, z-dz).BlockID <> 1uy then
                                        ok <- false
                                    // air above stone
                                    if map.GetBlockInfo(x-dx, y+1, z-dz).BlockID <> 0uy then
                                        ok <- false
                                    // non-transparent farther from lava
                                    if MC_Constants.BLOCKIDS_THAT_ARE_FULLY_TRANSPARENT_TO_LIGHT |> Array.contains(int(map.GetBlockInfo(x-2*dx, y+1, z-2*dz).BlockID)) then
                                        ok <- false
                                if ok then
                                    let x,z = x-dx, z-dz
                                    putFurnaceWithLoot(9,x,y,z)
                                    points.[9].Add( (x,y,z) )
                                    names.[9] <- "surface lava pool"
                                    printfn "lava %d %d %d" x y z
                    else
                        () // TODO other stuff
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
                    if map.GetBiome(x,z)=6uy && map.GetBlockInfo(x,y,z).BlockID=9uy && map.GetBlockInfo(x,y+1,z).BlockID=0uy then // swamp, water, air
                        if noneWithin(120,points.[14],x,y,z) then
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
                                    log.LogInfo(sprintf "putting SWAMP bit at %d %d" x z)
                                    // put "DIG" and "X" with entities so frost walker exposes
                                    let mkArmorStandAt(x,y,z) = 
                                        let y = y + 1  // place AS above the water, so no bubbles
                                        [|
                                            NBT.String("id","ArmorStand")
                                            NBT.List("Pos",Doubles([|float x + 0.5; float y + 0.9; float z + 0.5|]))
                                            NBT.List("Motion",Doubles([|0.0; 0.0; 0.0|]))
                                            NBT.List("Rotation",Doubles([|0.0; 0.0|]))
                                            NBT.Byte("Marker",1uy) // need hitbox to work with FW, so we command it instead below
                                            NBT.Byte("Invisible",1uy)
                                            NBT.Byte("NoGravity",1uy)
                                            NBT.End
                                        |]
                                    let ents = ResizeArray()
                                    for dx = 0 to DIGMAX-1 do
                                        for dz = 0 to DIGMAX-1 do
                                            if PIXELS.[dx].[DIGMAX-1-dz] = 'X' then
                                                ents.Add(mkArmorStandAt(x+dx,y,z+dz))
                                            else
                                                for ddx in [-1;0;1] do
                                                    for ddz in [-1;0;1] do
                                                        try
                                                            if PIXELS.[dx+ddx].[DIGMAX-1-(dz+ddz)] = 'X' then
                                                                // any space next to X but non-X, ensure no lily-pad by setting to air
                                                                map.SetBlockIDAndDamage(x+dx,y+1,z+dz,0uy,0uy)
                                                        with _ -> () // sloppily deal with array index out of bounds rather than using sentinels or checks
                                    map.AddEntities(ents)
                                    // place hidden trapped chest
                                    let x,y,z = x+9,y-2,z+6  // below the 'X'
                                    let mutable y = y
                                    let isWater bid = bid = 8uy || bid = 9uy
                                    while isWater(map.GetBlockInfo(x,y,z).BlockID) do
                                        y <- y - 1
                                    y <- y - 2
                                    putTrappedChestWithLoot(14,x,y,z,3)
                                    points.[14].Add( (x,y,z) )
                                    names.[14] <- "swamp hidden"
                                    // in order to make more discoverable, have nearby undeads 'help' by having FW boots they often drop
                                    let y = 2
                                    let RAD = 30 // if player this close, modify guys that close to player
                                    for dx,mob in [0,"Zombie"; 1,"Skeleton"] do
                                        let x = x + dx
                                        map.SetBlockIDAndDamage(x,y,z,210uy,0uy)  // repeating command block
                                        let command = sprintf """execute @p[r=%d,x=%d,y=65,z=%d] ~ ~ ~ entitydata @e[r=%d,type=%s,tag=!seen] {Tags:["seen"],ArmorItems:[{id:iron_boots,tag:{ench:[{id:9s,lvl:2s}]},Count:1b}],ArmorDropChances:[1.0F]}""" RAD x z RAD mob
                                        tileEntities.Add [| Int("x",x); Int("y",y); Int("z",z); String("id","Control"); Byte("auto",1uy); String("Command",command); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); End |]
                                        map.AddTileTick("minecraft:repeating_command_block",1,0,x,y,z)
                                    // use commands to undo frost-walker positioned below the AS (since AS _in_ the water caused problems)
                                    let x = x + 2
                                    map.SetBlockIDAndDamage(x,y,z,210uy,0uy)  // repeating command block
                                    let command = sprintf """execute @p[r=%d,x=%d,y=65,z=%d] ~ ~ ~ execute @e[type=ArmorStand] ~ ~-1 ~ detect ~ ~ ~ frosted_ice -1 setblock ~ ~ ~ water""" RAD x z // -1 because AS 1 above water
                                    tileEntities.Add [| Int("x",x); Int("y",y); Int("z",z); String("id","Control"); Byte("auto",1uy); String("Command",command); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); End |]
                                    map.AddTileTick("minecraft:repeating_command_block",1,0,x,y,z)
                // end for y
                if hmIgnoringLeaves.[x,z] > 100 then
                    if noneWithin(120,points.[10],x,y,z) then
                        let h = hmIgnoringLeaves.[x,z]
                        // if a local maximum
                        if h>hmIgnoringLeaves.[x-1,z-1]+1 && h>hmIgnoringLeaves.[x+0,z-1]+1 && h>hmIgnoringLeaves.[x+1,z-1]+1 && 
                            h>hmIgnoringLeaves.[x-1,z+0]+1 &&                                   h>hmIgnoringLeaves.[x+1,z+0]+1 && 
                            h>hmIgnoringLeaves.[x-1,z+1]+1 && h>hmIgnoringLeaves.[x+0,z+1]+1 && h>hmIgnoringLeaves.[x+1,z+1]+1 then
                            // if not a log
                            let y = hmIgnoringLeaves.[x,z]
                            let bid = map.GetBlockInfo(x,y,z).BlockID
                            if bid <> 17uy && bid <> 162uy then
                                // TODO ensure spire is sufficiently 'solid', sometimes chest is kinda floating above...
                                //printfn "MAYBE CRAZY SPIRE %d %d %d" x y z
                                let DIFF = 12
                                // if not too crazy a spire
                                if h<hmIgnoringLeaves.[x+0,z-1]+DIFF && h<hmIgnoringLeaves.[x-1,z+0]+DIFF && h<hmIgnoringLeaves.[x+1,z+0]+DIFF && h<hmIgnoringLeaves.[x+0,z+1]+DIFF then
                                    for nx,nz in [x,z; x+1,z; x-1,z; x,z+1; x,z-1] do
                                        let ny = hmIgnoringLeaves.[nx,nz]
                                        map.SetBlockIDAndDamage(nx, ny, nz, 87uy, 0uy) // 87=netherrack
                                        map.SetBlockIDAndDamage(nx, ny+1, nz, 51uy, 0uy) // 51=fire
                                    putTrappedChestWithLoot(10,x,y-1,z,2)
                                    points.[10].Add( (x,y-1,z) )
                                    names.[10] <- "mountain fire spire"
            // end if not near deco
        // end for z
    // end for x
    let allTrees = allTrees.All() |> Seq.toArray 
    Algorithms.shuffle(allTrees,rng)
    for t in allTrees do
        let x,y,z = t.CanonicalStump
        if biome.[x,z] = 8uy || biome.[x,z] = 9uy then
            () // ignore hell/end trees we've already replaced
        else
            // if no other super-common chests nearby
            if noneWithin(150,points.[7],x,y,z) && noneWithin(150,points.[1],x,y,z) && noneWithin(150,points.[15],x,y,z) then
                // if base would not leave chest exposed    
                let bidsAroundBase = 
                    set [| int(map.GetBlockInfo(x+1,y-1,z).BlockID)
                           int(map.GetBlockInfo(x-1,y-1,z).BlockID)
                           int(map.GetBlockInfo(x,y-1,z+1).BlockID)
                           int(map.GetBlockInfo(x,y-1,z-1).BlockID) |]
                if (Set.intersect (set MC_Constants.BLOCKIDS_THAT_ARE_FULLY_TRANSPARENT_TO_LIGHT) bidsAroundBase).IsEmpty then
                    // acacia with dark oak base, oak with birch base, all others with acacia base
                    let nbid,ndmg =
                        let bi = map.GetBlockInfo(x,y,z)
                        if bi.BlockID = 17uy && (bi.BlockData &&& 3uy = 0uy) then // oak
                            17uy, (bi.BlockData &&& 12uy) + 2uy // birch
                        elif bi.BlockID = 162uy && (bi.BlockData &&& 3uy = 0uy) then // acacia
                            162uy, (bi.BlockData &&& 12uy) + 1uy // dark oak
                        else // all others
                            162uy, (bi.BlockData &&& 12uy) + 0uy // acacia
                    map.SetBlockIDAndDamage(x,y,z,nbid,ndmg)
                    printfn "%d %d %d" x y z
                    let x,y,z = x,y-1,z
                    putTrappedChestWithLoot(15,x,y,z,1)
                    points.[15].Add( (x,y,z) )
                    names.[15] <- "tree stump"
    map.AddOrReplaceTileEntities(tileEntities)
    log.LogSummary(sprintf "added %d extra loot chests" tileEntities.Count)
    for i = 0 to names.Length-1 do
        if names.[i] <> "" then
            log.LogInfo(sprintf "%3d: %s" points.[i].Count names.[i])
            colorCount.[i] <- points.[i].Count
            if points.[i].Count > 64 then
                failwithf "more than 64 random loots of type %s added" names.[i]
        else
            colorCount.[i] <- 0
    for x,_,z in lootLocations do
        decorations.Add('*',x,z)

let placeCompassCommands(map:MapFolder, log:EventAndProgressLog) =
    // place a data block at every square of the map (y=0) and protect it (y=1)
    for x = MINIMUM to MINIMUM+LENGTH do
        for z = MINIMUM to MINIMUM+LENGTH do
            let dx = hiddenX-x
            let dz = hiddenZ-z
            let dy,dx = -dx,dz // Minecraft coords are insane
            let degrees = 180.0 * System.Math.Atan2(float dy, float dx) / System.Math.PI |> int
            let degrees = degrees + 720
            let degrees = degrees % 360
            let mutable degrees = degrees
            let mutable steps = 0
            for D in [180; 90; 45; 22; 11] do
                if degrees >= D then
                    degrees <- degrees - D
                    steps <- steps + 1
                steps <- steps * 2
            steps <- steps / 2
            if steps < 16 then
                map.SetBlockIDAndDamage(x,0,z,95uy,byte steps) // 95=stained_glass
            else
                map.SetBlockIDAndDamage(x,0,z,159uy,byte(steps-16)) // 159=stained_hardened_clay
            map.SetBlockIDAndDamage(x,1,z,7uy,0uy) // 7=bedrock
    // place the spawn chunks runner (y=2)
    let theString = """  ------->    ------->    ^^    <-------    <-------    --vv--  """
    let twice = theString + theString
    let at(n) = twice.Substring(n*2, 26)
    let i = ref 0
    let next() =
        let r = at(!i)
        incr i
        r
    let cmds = 
        [|
            // always-running loop to test for holding item
            yield P(sprintf """scoreboard players tag @a add Divining {SelectedItem:{tag:{display:{Lore:["%s"]}}}}""" Strings.NameAndLore.DIVINING_ROD_LORE)
            yield U """testfor @p[tag=Divining]"""
            yield C """blockdata ~ ~ ~2 {auto:1b}"""
            yield C """blockdata ~ ~ ~1 {auto:0b}"""
            // if item held... init score and call world-location and player rotation code
            yield O """scoreboard players set @p[tag=Divining] Rot 456"""  // where the ^^ is (96), +360
            // read world data
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 0 scoreboard players remove @p[tag=Divining] Rot 6""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 1 scoreboard players remove @p[tag=Divining] Rot 17""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 2 scoreboard players remove @p[tag=Divining] Rot 28""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 3 scoreboard players remove @p[tag=Divining] Rot 39""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 4 scoreboard players remove @p[tag=Divining] Rot 51""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 5 scoreboard players remove @p[tag=Divining] Rot 62""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 6 scoreboard players remove @p[tag=Divining] Rot 73""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 7 scoreboard players remove @p[tag=Divining] Rot 84""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 8 scoreboard players remove @p[tag=Divining] Rot 96""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 9 scoreboard players remove @p[tag=Divining] Rot 107""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 10 scoreboard players remove @p[tag=Divining] Rot 118""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 11 scoreboard players remove @p[tag=Divining] Rot 129""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 12 scoreboard players remove @p[tag=Divining] Rot 141""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 13 scoreboard players remove @p[tag=Divining] Rot 152""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 14 scoreboard players remove @p[tag=Divining] Rot 163""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_glass 15 scoreboard players remove @p[tag=Divining] Rot 174""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 0 scoreboard players remove @p[tag=Divining] Rot 186""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 1 scoreboard players remove @p[tag=Divining] Rot 197""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 2 scoreboard players remove @p[tag=Divining] Rot 208""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 3 scoreboard players remove @p[tag=Divining] Rot 219""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 4 scoreboard players remove @p[tag=Divining] Rot 231""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 5 scoreboard players remove @p[tag=Divining] Rot 242""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 6 scoreboard players remove @p[tag=Divining] Rot 253""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 7 scoreboard players remove @p[tag=Divining] Rot 264""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 8 scoreboard players remove @p[tag=Divining] Rot 276""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 9 scoreboard players remove @p[tag=Divining] Rot 287""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 10 scoreboard players remove @p[tag=Divining] Rot 298""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 11 scoreboard players remove @p[tag=Divining] Rot 309""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 12 scoreboard players remove @p[tag=Divining] Rot 321""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 13 scoreboard players remove @p[tag=Divining] Rot 332""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 14 scoreboard players remove @p[tag=Divining] Rot 343""")
            yield U("""execute @p[tag=Divining] ~ 0 ~ detect ~ ~ ~ stained_hardened_clay 15 scoreboard players remove @p[tag=Divining] Rot 354""")
            // detect player rotation 
            yield U """title @p[tag=Divining] times 0 10 0""" // is not saved with the world, so has to be re-executed to ensure run after restart client
            yield U("""summon ArmorStand ~ ~ ~ {Marker:1,Invulnerable:1,Invisible:1,Tags:["ASRot"]}""")
            yield U("""tp @e[tag=ASRot] @p[tag=Divining]""")
            for deg in [180; 90; 45; 22; 11] do
                yield U(sprintf """scoreboard players add @e[tag=ASRot,rym=%d] Rot %d""" deg deg)
                yield U(sprintf """tp @e[tag=ASRot,rym=%d] ~ ~ ~ ~-%d ~""" deg deg)
            yield U("""scoreboard players operation @p[tag=Divining] Rot += @e[tag=ASRot] Rot""")
            yield U("""kill @e[tag=ASRot]""")
            // convert score to title text
            yield U("scoreboard players operation @p[tag=Divining] Rot %= #ThreeSixty Rot")
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=0,score_Rot=11] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=12,score_Rot=22] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=23,score_Rot=33] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=34,score_Rot=45] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=46,score_Rot=56] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=57,score_Rot=67] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=68,score_Rot=78] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=79,score_Rot=90] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=91,score_Rot=101] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=102,score_Rot=112] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=113,score_Rot=123] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=124,score_Rot=135] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=136,score_Rot=146] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=147,score_Rot=157] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=158,score_Rot=168] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=169,score_Rot=180] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=181,score_Rot=191] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=192,score_Rot=202] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=203,score_Rot=213] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=214,score_Rot=225] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=226,score_Rot=236] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=237,score_Rot=247] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=248,score_Rot=258] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=259,score_Rot=270] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=271,score_Rot=281] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=282,score_Rot=292] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=293,score_Rot=303] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=304,score_Rot=315] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=316,score_Rot=326] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=327,score_Rot=337] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=338,score_Rot=348] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=349,score_Rot=359] subtitle {"text":"%s"}""" (next()))
            yield U("""title @p[tag=Divining] title {"text":""}""")
            yield U("""scoreboard players tag @a remove Divining""")
        |]
    let r = map.GetRegion(1,1)
    r.PlaceCommandBlocksStartingAt(1,2,1,cmds,"")  // placeStartingCommands will blockdata the purple at start of this guy to start him running
    log.LogInfo(sprintf "placed %d COMPASS commands" cmds.Length)

let placeStartingCommands(worldSaveFolder:string,map:MapFolder,hmIgnoringLeaves:_[,],allTrees:ContainerOfMCTrees, mapTimeInHours, debugChests, colorCount:_[]) =
    let placeCommand(x,y,z,command,bid,dmg,name,wantTileTick) =
        map.SetBlockIDAndDamage(x,y,z,bid,dmg)  // command block
        let auto = if name = "minecraft:command_block" then 0uy else 1uy
        map.AddOrReplaceTileEntities([| [| Int("x",x); Int("y",y); Int("z",z); String("id","Control"); Byte("auto",auto); String("Command",command); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); End |] |])
        if wantTileTick then
            map.AddTileTick(name,100,0,x,y,z)
    let placeImpulse(x,y,z,command,wantTileTick) = placeCommand(x,y,z,command,137uy,0uy,"minecraft:command_block",wantTileTick)
    let placeRepeating(x,y,z,command,wantTileTick) = placeCommand(x,y,z,command,210uy,0uy,"minecraft:repeating_command_block",wantTileTick)
    let placeChain(x,y,z,command,cond) = placeCommand(x,y,z,command,211uy,(if cond then 8uy else 0uy),"minecraft:chain_command_block",false)
    let h = hmIgnoringLeaves.[1,1] // 1,1 since 0,0 has commands
    let y = ref 255
    let I(c) = placeImpulse(0,!y,0,c,true); decr y
    // add diorite pillars to denote border between light and dark
    for i = 0 to 99 do
        let theta = System.Math.PI * 2.0 * float i / 100.0
        let x = cos theta * float DAYLIGHT_RADIUS |> int
        let z = sin theta * float DAYLIGHT_RADIUS |> int
        let h = hmIgnoringLeaves.[x,z] + 5
        if h > 60 then
            for y = 60 to h do
                map.SetBlockIDAndDamage(x,y,z,1uy,3uy)  // diorite
            map.SetBlockIDAndDamage(x,h+1,z,89uy,0uy) // 89=glowstone
    // compass initialization
    I "scoreboard objectives add Rot dummy"
    I "scoreboard players set #ThreeSixty Rot 360"
    I "blockdata 1 2 1 {auto:1b}"
    // other commands    
    I("worldborder set 2048")
    I("gamerule doDaylightCycle false")
    I("gamerule logAdminCommands false")
    I("gamerule commandBlockOutput false")
    I("gamerule disableElytraMovementCheck true")
    //I("gamerule keepInventory true")
    if UHC_MODE then
        I("gamerule naturalRegeneration false")  // TODO starting book info
    I(sprintf "setworldspawn 1 %d 1" h)
    I("gamerule spawnRadius 2")
    I("weather clear 999999")
    I("scoreboard objectives add hidden dummy")
    I(sprintf "scoreboard objectives add Deaths stat.deaths %s" Strings.NAME_OF_DEATHCOUNTER_SIDEBAR.Text)
    I("scoreboard objectives setdisplay sidebar Deaths")
    I(sprintf "scoreboard players set X hidden %d" hiddenX)
    I(sprintf "scoreboard players set Z hidden %d" hiddenZ)
    I(sprintf "scoreboard players set fX hidden %d" finalEX)
    I(sprintf "scoreboard players set fZ hidden %d" finalEZ)
    I("scoreboard players set CTM hidden 0")
    // repeat blocks to check for CTM completion
    I(sprintf "blockdata 0 %d 0 {auto:1b}" (h-11))
    I(sprintf "blockdata 1 %d 0 {auto:1b}" (h-11))
    I(sprintf "blockdata 2 %d 0 {auto:1b}" (h-11))
    I(sprintf "fill 0 %d 0 0 255 0 air" !y) // remove all the ICBs
    


    // clear space above spawn platform...
    // ..remove entirety of nearby trees
    if allTrees = null then
        printfn "allTrees WAS NULL, SKIPPING TREE REDO"
    else
        let TREE_REMOVE_RADIUS = 15
        for x = -TREE_REMOVE_RADIUS to TREE_REMOVE_RADIUS do
            for z = -TREE_REMOVE_RADIUS to TREE_REMOVE_RADIUS do
                allTrees.Remove(x,z,map)
    // ...clear out any other blocks
    for x = -3 to 5 do
        for z = -3 to 5 do
            for dy = 1 to 20 do
                map.SetBlockIDAndDamage(x,h+dy,z,0uy,0uy) // air
    // rest of monument
    let ENABLE_DC,DISABLE_DC = Coords(1,h-13,1), Coords(2,h-13,1)
    map.SetBlockIDAndDamage(2,h+2,4,49uy,0uy) // 49=obsidian
    map.SetBlockIDAndDamage(1,h+2,4,49uy,0uy)
    map.SetBlockIDAndDamage(0,h+2,4,49uy,0uy)
    map.SetBlockIDAndDamage(2,h+2,3,68uy,2uy) // 68=wall_sign
    map.SetBlockIDAndDamage(1,h+2,3,68uy,2uy)
    map.SetBlockIDAndDamage(0,h+2,3,68uy,2uy)
    map.AddOrReplaceTileEntities([|
                                    [| Int("x",2); Int("y",h+2); Int("z",3); String("id","Sign"); String("Text1","""{"translate":"tile.endBricks.name"}"""); String("Text2","""{"text":""}"""); String("Text3","""{"text":""}"""); String("Text4","""{"text":""}"""); End |]
                                    [| Int("x",1); Int("y",h+2); Int("z",3); String("id","Sign"); String("Text1","""{"translate":"tile.purpurBlock.name"}"""); String("Text2","""{"text":""}"""); String("Text3","""{"text":""}"""); String("Text4","""{"text":""}"""); End |]
                                    [| Int("x",0); Int("y",h+2); Int("z",3); String("id","Sign"); String("Text1","""{"translate":"tile.sponge.dry.name"}"""); String("Text2","""{"text":""}"""); String("Text3","""{"text":""}"""); String("Text4","""{"text":""}"""); End |]
                                 |])
    map.SetBlockIDAndDamage(1,h+2,5,68uy,3uy)
    map.SetBlockIDAndDamage(0,h+2,5,68uy,3uy)
    map.AddOrReplaceTileEntities([|
                                    [| Int("x",1); Int("y",h+2); Int("z",5); String("id","Sign"); String("Text1",sprintf """{"text":"%s"}""" Strings.SIGN_DC_ENABLE.[0]); String("Text2",sprintf """{"text":"%s"}""" Strings.SIGN_DC_ENABLE.[1]); String("Text3",sprintf """{"text":"%s"}""" Strings.SIGN_DC_ENABLE.[2]); String("Text4",sprintf """{"text":"%s","clickEvent":{"action":"run_command","value":"/blockdata %s {auto:1b}"}}""" Strings.SIGN_DC_ENABLE.[3] ENABLE_DC.STR); End |]
                                    [| Int("x",0); Int("y",h+2); Int("z",5); String("id","Sign"); String("Text1",sprintf """{"text":"%s"}""" Strings.SIGN_DC_DISABLE.[0]); String("Text2",sprintf """{"text":"%s"}""" Strings.SIGN_DC_DISABLE.[1]); String("Text3",sprintf """{"text":"%s"}""" Strings.SIGN_DC_DISABLE.[2]); String("Text4",sprintf """{"text":"%s","clickEvent":{"action":"run_command","value":"/blockdata %s {auto:1b}"}}""" Strings.SIGN_DC_DISABLE.[3] DISABLE_DC.STR); End |]
                                 |])

    let chestItems = 
        Compounds[| 
                let times = if CustomizationKnobs.SINGLEPLAYER then 1 else 2
                for i = 0 to times-1 do
                    yield [| Byte("Count", 1uy); Byte("Slot", byte(18*i)+0uy); Short("Damage",0s); String("id","minecraft:iron_axe"); Compound("tag", [|List("ench",Compounds[|[|Short("id",18s);Short("lvl",5s);End|]|]); End |] |> ResizeArray); End |]
                    yield [| Byte("Count", 1uy); Byte("Slot", byte(18*i)+1uy); Short("Damage",0s); String("id","minecraft:shield"); End |]
                    if CustomizationKnobs.UHC_MODE then
                        yield [| Byte("Count",12uy); Byte("Slot", byte(18*i)+2uy); Short("Damage",0s); String("id","minecraft:apple"); End |]
                    else
                        yield [| Byte("Count", 8uy); Byte("Slot", byte(18*i)+2uy); Short("Damage",0s); String("id","minecraft:bread"); End |]
                        yield [| Byte("Count",32uy); Byte("Slot", byte(18*i)+3uy); Short("Damage",0s); String("id","minecraft:cookie"); End |]
                    yield [| Byte("Count",64uy); Byte("Slot", byte(18*i)+4uy); Short("Damage",0s); String("id","minecraft:dirt"); End |]
                    if CustomizationKnobs.UHC_MODE then
                        yield [| Byte("Count", 1uy); Byte("Slot", byte(18*i)+5uy); Short("Damage",0s); String("id","minecraft:bow"); End |]
                        yield [| Byte("Count",24uy); Byte("Slot", byte(18*i)+6uy); Short("Damage",0s); String("id","minecraft:arrow"); End |]
                        yield [| Byte("Count", 3uy); Byte("Slot", byte(18*i)+7uy); Short("Damage",0s); String("id","minecraft:golden_apple"); End |]
                yield [| Byte("Count", 1uy); Byte("Slot", 9uy); Short("Damage",0s); String("id","minecraft:written_book"); Strings.STARTING_BOOK_META; End |]
                yield [| Byte("Count", 1uy); Byte("Slot",10uy); Short("Damage",0s); String("id","minecraft:written_book"); Strings.STARTING_BOOK_RULES; End |]
                yield [| Byte("Count", 1uy); Byte("Slot",11uy); Short("Damage",0s); String("id","minecraft:written_book"); Strings.STARTING_BOOK_OVERVIEW; End |]
                yield [| Byte("Count", 1uy); Byte("Slot",12uy); Short("Damage",0s); String("id","minecraft:written_book"); Strings.STARTING_BOOK_GETTING_STARTED; End |]
                yield [| Byte("Count", 1uy); Byte("Slot",13uy); Short("Damage",0s); String("id","minecraft:written_book"); Strings.STARTING_BOOK_FOOD_AND_COMBAT; End |]
                yield [| Byte("Count", 1uy); Byte("Slot",14uy); Short("Damage",0s); String("id","minecraft:written_book"); Strings.STARTING_BOOK_HINTS_AND_SPOILERS; End |]
            |]
    putUntrappedChestWithItemsAt(1,h+2,-2,Strings.NAME_OF_STARTING_CHEST,chestItems,map,null)
    map.SetBlockIDAndDamage(1,h+3,-2,130uy,3uy) // enderchest
    if debugChests then
        let chestItems = 
            Compounds[| 
                    yield [| Byte("Count",1uy); Byte("Slot",1uy); Short("Damage",0s); String("id","minecraft:written_book"); 
                             Strings.BOOK_IN_HIDING_SPOT(Strings.TranslatableString"DUMMYNW"); End |]
                    yield [| Byte("Count",1uy); Byte("Slot",2uy); Short("Damage",0s); String("id","minecraft:written_book"); 
                             Strings.BOOK_WITH_ELYTRA; End |]
                    yield [| Byte("Count", 1uy); Byte("Slot",3uy); Short("Damage",0s); String("id","minecraft:written_book"); 
                             Strings.TELEPORTER_HUB_BOOK; End |]
                    yield [| Byte("Count",1uy); Byte("Slot",4uy); Short("Damage",0s); String("id","minecraft:written_book"); 
                             Strings.BOOK_IN_FINAL_PURPLE_DUNGEON_CHEST; End |]
                    yield [| Byte("Count",1uy); Byte("Slot",4uy); Short("Damage",0s); String("id","minecraft:written_book"); 
                             Strings.BOOK_IN_FINAL_PURPLE_DUNGEON_CHEST; End |]

                    yield [| Byte("Count",1uy); Byte("Slot",10uy); Short("Damage",0s); String("id","minecraft:written_book"); 
                             Compound("tag", Utilities.makeWrittenBookTags(Strings.FISHING_DATA)|>ResizeArray); End |]
                    yield [| Byte("Count",1uy); Byte("Slot",11uy); Short("Damage",0s); String("id","minecraft:written_book"); 
                             Strings.BOOK_IN_DUNGEON_OR_MINESHAFT_CHEST; End |]
                    yield [| Byte("Count",1uy); Byte("Slot",12uy); Short("Damage",0s); String("id","minecraft:written_book"); 
                             Strings.BOOK_IN_GREEN_BEACON_CHEST; End |]
                    yield [| Byte("Count",1uy); Byte("Slot",13uy); Short("Damage",0s); String("id","minecraft:written_book"); 
                             Strings.BOOK_IN_FLAT_DUNGEON_CHEST; End |]
                    yield [| Byte("Count",1uy); Byte("Slot",14uy); Short("Damage",0s); String("id","minecraft:written_book"); 
                             Strings.BOOK_IN_MOUNTAIN_PEAK_CHEST; End |]
                |]
        putUntrappedChestWithItemsAt(3,h+2,-2,Strings.TranslatableString"DEBUG",chestItems,map,null)
    // bonus monument
    if CustomizationKnobs.KURT_SPECIAL then
        let chestItems = 
            Compounds[| 
                    for i = 1 to 8 do
                        yield [| Byte("Count", 1uy); Byte("Slot", byte(i)); Short("Damage",15s); String("id","minecraft:stained_glass_pane"); End |]
                    yield [| Byte("Count", 1uy); Byte("Slot", 0uy); Short("Damage",0s); String("id","minecraft:written_book"); Strings.BONUS_MONUMENT_BOOK; End |]
                    yield [| Byte("Count", 1uy); Byte("Slot", 9uy); Short("Damage",15s); String("id","minecraft:stained_glass_pane"); End |]
                    yield [| Byte("Count", 1uy); Byte("Slot", 18uy); Short("Damage",15s); String("id","minecraft:stained_glass_pane"); End |]
                    for i = 10 to 17 do
                        let color = i-10
                        let count = colorCount.[color]
                        if count = 0 then
                            yield [| Byte("Count", 1uy); Byte("Slot", byte(i)); Short("Damage",15s); String("id","minecraft:stained_glass_pane"); End |]
                    for i = 19 to 26 do
                        let color = i-11
                        let count = colorCount.[color]
                        if count = 0 then
                            yield [| Byte("Count", 1uy); Byte("Slot", byte(i)); Short("Damage",15s); String("id","minecraft:stained_glass_pane"); End |]
                |]
        putUntrappedChestWithItemsAndOrientationAt(-3,h+2,1,Strings.NAME_OF_BONUS_MONUMENT_CHEST,chestItems,5uy,map,null)
        let chestItems = 
            Compounds[| 
                    for i = 1 to 8 do
                        yield [| Byte("Count", 1uy); Byte("Slot", byte(i)); Short("Damage",15s); String("id","minecraft:stained_glass_pane"); End |]
                    yield [| Byte("Count", 16uy); Byte("Slot", 0uy); Short("Damage",0s); String("id","minecraft:filled_map"); Compound("tag", [|Strings.NameAndLore.WORLD_MAP; End|]|>ResizeArray); End |]
                    yield [| Byte("Count", 1uy); Byte("Slot", 9uy); Short("Damage",15s); String("id","minecraft:stained_glass_pane"); End |]
                    yield [| Byte("Count", 1uy); Byte("Slot", 18uy); Short("Damage",15s); String("id","minecraft:stained_glass_pane"); End |]
                    for i = 10 to 17 do
                        let color = i-10
                        let count = colorCount.[color]
                        if count <> 0 then
                            assert(count > 0 && count <= 64)
                            yield [| Byte("Count", byte(count)); Byte("Slot", byte(i)); Short("Damage",int16(color)); String("id","minecraft:stained_glass"); Compound("tag", [|Strings.NameAndLore.BONUS_SAMPLE; End|]|>ResizeArray); End |]
                        else
                            yield [| Byte("Count", 1uy); Byte("Slot", byte(i)); Short("Damage",15s); String("id","minecraft:stained_glass_pane"); End |]
                    for i = 19 to 26 do
                        let color = i-11
                        let count = colorCount.[color]
                        if count <> 0 then
                            assert(count > 0 && count <= 64)
                            yield [| Byte("Count", byte(count)); Byte("Slot", byte(i)); Short("Damage",int16(color)); String("id","minecraft:stained_glass"); Compound("tag", [|Strings.NameAndLore.BONUS_SAMPLE; End|]|>ResizeArray); End |]
                        else
                            yield [| Byte("Count", 1uy); Byte("Slot", byte(i)); Short("Damage",15s); String("id","minecraft:stained_glass_pane"); End |]
                |]
        putUntrappedChestWithItemsAndOrientationAt(-3,h+2,2,Strings.NAME_OF_BONUS_MONUMENT_CHEST,chestItems,5uy,map,null)
        Utilities.editMapDat(worldSaveFolder+"""\data\map_0.dat""",4uy,0,0)
    // the TP hub...
    // ...walls and room
    for x = -2 to 4 do
        for z = -2 to 4 do
            for y = h-50 to h-1 do
                if x = -2 || x = 4 || z = -2 || z = 4 then
                    map.SetBlockIDAndDamage(x,y,z,7uy,0uy)
                else
                    map.SetBlockIDAndDamage(x,y,z,0uy,0uy)
    for x = -3 to 5 do
        for z = -3 to 5 do
            for y = h-5 to h do
                if x = -3 || x = 5 || z = -3 || z = 5 then
                    map.SetBlockIDAndDamage(x,y,z,159uy,3uy) // 159,3 is light blue stained clay, on walls of tp hub in case exposed on surface
    // beacon at spawn for convenience
    putBeaconAt(map,1,h-6,1,0uy,false)
    // ...floor & ceiling
    for x = -2 to 4 do
        for z = -2 to 4 do
            map.SetBlockIDAndDamage(x,h-51,z,7uy,0uy) // floor of commands box
            map.SetBlockIDAndDamage(x,h-6,z,7uy,0uy)  // floor of teleporter hub
            map.SetBlockIDAndDamage(x,h,z,7uy,0uy)
    for x = -3 to 5 do
        for z = -3 to 5 do
            if x=1 && z=1 then
                map.SetBlockIDAndDamage(x,h+1,z,7uy,0uy)
            else
                map.SetBlockIDAndDamage(x,h+1,z,159uy,3uy) // 159,3 is light blue stained clay
    // ...stuff in the room to start
    let chestItems = Compounds[| [| Byte("Count", 1uy); Byte("Slot", 13uy); Short("Damage",0s); String("id","minecraft:written_book"); Strings.TELEPORTER_HUB_BOOK; End |] |]
    putUntrappedChestWithItemsAt(1,h-5,-1,Strings.NAME_OF_TELEPORT_ROOM_CHEST,chestItems,map,null)
    // pink sheep
    if CustomizationKnobs.KURT_SPECIAL then
        for rx in [-2 .. 1] do
            for rz in [-2 .. 1] do
                let r = map.GetRegion(512*rx, 512*rz)
                for cx = 0 to 31 do
                    for cz = 0 to 31 do
                        let theChunk = r.GetChunk(cx,cz)
                        let theChunkLevel = match theChunk with Compound(_,rsa) -> rsa.[0]  // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
                        match theChunkLevel.["Entities"] with 
                        | List(_,Compounds(cs)) ->
                            for e in cs do
                                // make all sheep pink
                                if e |> Array.exists (fun x -> x = String("id","Sheep")) then
                                    for i = 0 to e.Length-1 do
                                        match e.[i] with
                                        | Byte("Color",_) -> e.[i] <- Byte("Color",6uy)
                                        | _ -> ()
                        | _ -> ()
    // 'expose teleport area' cmd
    map.SetBlockIDAndDamage(3,h-11,0,137uy,0uy)
    map.AddOrReplaceTileEntities([| [| Int("x",3); Int("y",h-11); Int("z",0); String("id","Control"); Byte("auto",0uy); String("Command",sprintf "/fill %d %d %d %d %d %d ladder 1" 1 (h-4) 3 1 (h+1) 3); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); End |] |])
    let r = map.GetRegion(1,1)
    let cmds(x,tilename) = 
        [|
            P (sprintf "testforblock %d %d 4 %s" x (h+3) tilename)
            C "scoreboard players add CTM hidden 1"
            C Strings.TELLRAW_PLACED_A_MONUMENT_BLOCK 
            C "fill ~ ~ ~ ~ ~ ~-3 air"
        |]
    // h-11, monument block detectors
    for x,tilename in [0,"sponge"; 1,"purpur_block"; 2,"end_bricks"] do
        r.PlaceCommandBlocksStartingAt(x,h-11,0,cmds(x,tilename),"check ctm block")
    let y = ref (h-13)
    let R(c) = placeRepeating(0,!y,0,c,true); decr y
    let C(c) = placeChain(0,!y,0,c,true); decr y
    let U(c) = placeChain(0,!y,0,c,false); decr y
    let I(c) = placeImpulse(0,!y,0,c,false); decr y
    let ticks = (int64 mapTimeInHours) * 60L  *60L * 20L
    assert(ticks % 24000L = 0L)
    let dayTicks = ticks + 1000L
    let nightTicks = ticks + 14500L
    R(sprintf "time set %d" nightTicks) // night
    U("testfor @a {ActiveEffects:[{Id:26b}]}") // if anyone has luck potion
    C(sprintf "time set %d" dayTicks) // day
    U(sprintf "execute @p[r=%d,x=0,y=80,z=0] ~ ~ ~ time set %d" DAYLIGHT_RADIUS dayTicks)  // Note, in multiplayer, if any player is near spawn, stays day (could exploit)
    U("scoreboard players test CTM hidden 3 *")
    C(sprintf "blockdata ~ %d ~ {auto:0b}" (h-13)) // turn off main repeat loop
    C("blockdata ~ ~-2 ~ {auto:1b}")
    C("blockdata ~ ~-1 ~ {auto:0b}")
    // won-the-game proc
    I(sprintf "summon FireworksRocketEntity %d %d %d {LifeTime:14,FireworksItem:{id:fireworks,Count:1,tag:{Fireworks:{Explosions:[{Type:2,Flicker:1,Trail:1,Colors:[56831],FadeColors:[16715263]}]}}}}" 0 (h+3) 4)
    U(sprintf "summon FireworksRocketEntity %d %d %d {LifeTime:24,FireworksItem:{id:fireworks,Count:1,tag:{Fireworks:{Explosions:[{Type:1,Flicker:1,Trail:1,Colors:[3849770],FadeColors:[14500508]}]}}}}" 1 (h+3) 4)
    U(sprintf "summon FireworksRocketEntity %d %d %d {LifeTime:34,FireworksItem:{id:fireworks,Count:1,tag:{Fireworks:{Explosions:[{Type:4,Flicker:1,Trail:1,Colors:[8592414],FadeColors:[13942014]}]}}}}" 2 (h+3) 4)
    U(sprintf """tellraw @a [""]""")
    U(Strings.TELLRAW_FINAL_1)
    U(Strings.TELLRAW_FINAL_2)
    U(sprintf """tellraw @a [""]""")
    U(Strings.TELLRAW_FINAL_3)
    U(Strings.TELLRAW_FINAL_4)
    U("gamerule doDaylightCycle true")
    U("worldborder set 30000000")
    // TODO embed in normal world terrain
    // TODO nether still different
    // TODO loot tables still different
    // TODO message players about ability to keep playing
    if CustomizationKnobs.SILVERFISH_LIMITS then
        let y = ref (h-13)
        let R(c) = placeRepeating(1,!y,0,c,true); decr y
        let C(c) = placeChain(1,!y,0,c,true); decr y
        let U(c) = placeChain(1,!y,0,c,false); decr y
        let I(c) = placeImpulse(1,!y,0,c,false); decr y
        // cap currently off
        R("""scoreboard players set Feesh hidden 0""")
        U("""execute @e[type=Silverfish] ~ ~ ~ scoreboard players add Feesh hidden 1""")
        U(sprintf """scoreboard players test Feesh hidden %d""" CustomizationKnobs.SILVERFISH_BIG)
        C("""blockdata ~ ~3 ~ {auto:0b}""") // turn self off
        C("""blockdata ~ ~-1 ~ {auto:1b}""")
        I("""blockdata ~ ~ ~ {auto:0b}""")   // tick delay to deal with extra repeat
        U(Strings.TELLRAW_SILVERFISH_LIMIT_ON)
        U("""scoreboard players tag @e[type=Silverfish] add Old""")
        U("""blockdata ~ ~-1 ~ {auto:1b}""")
        // cap currently on
        R("""tp @e[type=Silverfish,tag=!Old] ~ ~-200 ~""") // kill all new feesh
        U("""scoreboard players set Feesh hidden 0""")
        U("""execute @e[type=Silverfish] ~ ~ ~ scoreboard players add Feesh hidden 1""")
        U(sprintf """scoreboard players test Feesh hidden 0 %d""" CustomizationKnobs.SILVERFISH_SMALL)
        C("""blockdata ~ ~4 ~ {auto:0b}""") // turn self off
        C("""blockdata ~ ~-1 ~ {auto:1b}""")
        I("""blockdata ~ ~ ~ {auto:0b}""")   // tick delay to deal with extra repeat
        U(Strings.TELLRAW_SILVERFISH_LIMIT_OFF)
        U("""blockdata ~ ~17 ~ {auto:1b}""")
    let x,y,z = ENABLE_DC.Tuple
    placeImpulse(x,y,z,"blockdata ~ ~ ~ {auto:0b}",false)
    placeChain(x,y-1,z,"scoreboard objectives setdisplay sidebar Deaths",true)
    placeChain(x,y-2,z,Strings.TELLRAW_DEATH_COUNTER_DISPLAY_ENABLED,true)
    placeChain(x,y-3,z,"scoreboard players add @a Deaths 0",true)
    let x,y,z = DISABLE_DC.Tuple
    placeImpulse(x,y,z,"blockdata ~ ~ ~ {auto:0b}",false)
    placeChain(x,y-1,z,"scoreboard objectives setdisplay sidebar",true)
    placeChain(x,y-2,z,Strings.TELLRAW_DEATH_COUNTER_DISPLAY_DISABLED,true)

let TELEPORT_PATH_OUT_DISTANCES = [|60;120;180;240;300|]
let placeTeleporters(rng:System.Random, map:MapFolder, hm:_[,], hmIgnoringLeaves:_[,], log:EventAndProgressLog, decorations:ResizeArray<_>, allTrees : ContainerOfMCTrees) =
    // preprocess trees
    if allTrees = null then
        printfn "allTrees WAS NULL, SKIPPING TREE REDO"
    // TODO code above is repeated elsewhere, is it expensive? if so, factor
    let placeCommand(x,y,z,command,bid,auto,_name) =
        map.SetBlockIDAndDamage(x,y,z,bid,0uy)  // command block
        map.AddOrReplaceTileEntities([| [| Int("x",x); Int("y",y); Int("z",z); String("id","Control"); Byte("auto",auto); String("Command",command); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); End |] |])
    let placeImpulse(x,y,z,command) = placeCommand(x,y,z,command,137uy,0uy,"minecraft:command_block")
    let placeRepeating(x,y,z,command) = placeCommand(x,y,z,command,210uy,1uy,"minecraft:repeating_command_block")
    let placeChain(x,y,z,command) = placeCommand(x,y,z,command,211uy,1uy,"minecraft:chain_command_block")
    let villagerData(i) =
        // note: Ambient:1b has potion in upper-right of HUD, whereas ShowParticles:0b does not, so I prefer ambient
        match i with
        | 0 -> sprintf """Profession:1,Career:1,CareerLevel:9999,Offers:{Recipes:[{rewardExp:0b,maxUses:99999,uses:0,buy:{Count:3b,id:"emerald"},sell:{Count:1b,id:"potion",tag:{CustomPotionEffects:[{Id:1b,Amplifier:0b,Duration:999999,Ambient:1b}],display:{Name:"%s",Lore:["%s","%s"]}}}}]}""" Strings.POTION_SPEED_NAME.Text          Strings.POTION_LORE1.Text Strings.POTION_LORE2.Text // 1=speed
        | 1 -> sprintf """Profession:2,Career:1,CareerLevel:9999,Offers:{Recipes:[{rewardExp:0b,maxUses:99999,uses:0,buy:{Count:2b,id:"emerald"},sell:{Count:1b,id:"potion",tag:{CustomPotionEffects:[{Id:3b,Amplifier:0b,Duration:999999,Ambient:1b}],display:{Name:"%s",Lore:["%s","%s"]}}}}]}""" Strings.POTION_HASTE_NAME.Text          Strings.POTION_LORE1.Text Strings.POTION_LORE2.Text // 3=haste
        | 2 -> sprintf """Profession:3,Career:1,CareerLevel:9999,Offers:{Recipes:[{rewardExp:0b,maxUses:99999,uses:0,buy:{Count:4b,id:"emerald"},sell:{Count:1b,id:"potion",tag:{CustomPotionEffects:[{Id:5b,Amplifier:1b,Duration:999999,Ambient:1b}],display:{Name:"%s",Lore:["%s","%s"]}}}}]}""" Strings.POTION_STRENGTH_NAME.Text       Strings.POTION_LORE1.Text Strings.POTION_LORE2.Text // 5=strength
        | 3 -> sprintf """Profession:4,Career:1,CareerLevel:9999,Offers:{Recipes:[{rewardExp:0b,maxUses:99999,uses:0,buy:{Count:5b,id:"emerald"},sell:{Count:1b,id:"potion",tag:{CustomPotionEffects:[{Id:21b,Amplifier:1b,Duration:999999,Ambient:1b}],display:{Name:"%s",Lore:["%s","%s"]}}}}]}""" Strings.POTION_HEALTH_BOOST_NAME.Text  Strings.POTION_LORE1.Text Strings.POTION_LORE2.Text // 21=health boost
        | _ -> failwith "bad villager #"
    let unusedVillagers = ResizeArray [| 0; 1; 2; 3 |]
    let villagers = ResizeArray()
    for i = 0 to 3 do
        let n = rng.Next(unusedVillagers.Count)
        villagers.Add(villagerData(unusedVillagers.[n]))
        unusedVillagers.RemoveAt(n)
    for xs,zs,dirName,spx,spz,tpdata,vd in [-512,-512,Strings.QUADRANT_NORTHWEST,-1,-1,"~0.5 ~0.2 ~0.5 -45 10",villagers.[0]
                                            -512,+512,Strings.QUADRANT_SOUTHWEST,-1,3,"~0.5 ~0.2 ~-0.5 -135 10",villagers.[1]
                                            +512,+512,Strings.QUADRANT_SOUTHEAST,3,3,"~-0.5 ~0.2 ~-0.5 135 10",villagers.[2]
                                            +512,-512,Strings.QUADRANT_NORTHEAST,3,-1,"~-0.5 ~0.2 ~0.5 45 10",villagers.[3]] do
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
                            decorations.Add('T',x+2,z+2)
                            for i = -1 to 5 do
                                for j = -1 to 5 do
                                    map.SetBlockIDAndDamage(x+i,h-1,z+j,7uy,0uy)  // 7=bedrock   // wider platform at base, in case generates on ocean, can 'climb up'
                                    map.SetBlockIDAndDamage(x+i,h+0,z+j,0uy,0uy)
                                    map.SetBlockIDAndDamage(x+i,h+1,z+j,0uy,0uy)
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
                            let spawnHeight = hmIgnoringLeaves.[1,1]
                            map.AddOrReplaceTileEntities([| [| Int("x",x+2); Int("y",h+2); Int("z",z+2); String("id","EndGateway"); Long("Age",180L); Byte("ExactTeleport",1uy); Compound("ExitPortal",[Int("X",1);Int("Y",spawnHeight-4);Int("Z",1);End]|>ResizeArray); End |] |])
                            putBeaconAt(map,x+2,h+14,z+2,0uy,false)
                            placeRepeating(x+2,h+24,z+2,sprintf "execute @p[r=28] ~ ~ ~ blockdata %d %d %d {auto:1b}" (x+2) (h+23) (z+2)) // absolute coords since execute-at
                            map.AddTileTick("minecraft:repeating_command_block",100,0,x+2,h+24,z+2)
                            placeImpulse(x+2,h+23,z+2,sprintf "blockdata %d %d %d {auto:1b}" 3 (spawnHeight-11) 0) // expose teleporters at spawn //note brittle coords of block
                            placeChain(x+2,h+22,z+2,sprintf "execute @a %d %d %d playsound block.portal.trigger block @a" (x+2) (h+6) (z+2))
                            placeChain(x+2,h+21,z+2,sprintf "setblock %d %d %d end_gateway 0 replace {ExactTeleport:1b,ExitPortal:{X:%d,Y:%d,Z:%d}}" spx (spawnHeight-5) spz (x+2) (h+6) (z+2))
                            placeChain(x+2,h+20,z+2,sprintf "summon ArmorStand %d %d %d {NoGravity:1,Marker:1b,Invisible:1,Passengers:[{id:Villager,Invulnerable:1,NoAI:1,Silent:1,CustomName:\"%s\",%s}]}" spx (spawnHeight-3) spz (Strings.TELEPORTER_TO_BLAH(dirName).Text) vd)
                            placeChain(x+2,h+19,z+2,Strings.TELLRAW_TELEPORTER_UNLOCKED)
                            placeChain(x+2,h+18,z+2,"""blockdata ~ ~-1 ~ {auto:1b}""") // wait a tick to teleport just-spawned entities
                            placeImpulse(x+2,h+17,z+2,sprintf "tp @e[type=ArmorStand,x=%d,y=%d,z=%d,r=1] %s" spx (spawnHeight-3) spz tpdata)
                            placeChain(x+2,h+16,z+2,sprintf "tp @e[type=Villager,x=%d,y=%d,z=%d,r=1] %s" spx (spawnHeight-3) spz tpdata)
                            placeChain(x+2,h+15,z+2,"fill ~ ~ ~ ~ ~9 ~ air") // erase us
                            // place an 8-way path out to make these more findable
                            let DIRS = [|-1,-1; -1,0; -1,+1; 0,+1; +1,+1; +1,0; +1,-1; 0,-1|]  // dx, dz
                            let TREEWIDE = [| -4; -3; -2; -1; 0; 1; 2; 3; 4 |]
                            let WIDE = [| -2; -1; 0; 1; 2 |]
                            let cx, cz = x+2, z+2
                            let TABLE = 
                                [|
                                    1uy,3uy,0uy      // stone (and variants)   -> dirt
                                    2uy,208uy,0uy    // grass                  -> grass_path
                                    3uy,110uy,0uy    // dirt (and variants)    -> mycelium
                                    7uy, 7uy,0uy     // bedrock (to keep algorithm from falling off a cliff)
                                    12uy,128uy,4uy   // sand                   -> upside-down sandstone stairs (red sand special-cased below)
                                    13uy,82uy,0uy    // gravel                 -> clay
                                    24uy,159uy,0uy   // sandstone              -> stained_hardned_clay
                                    78uy,171uy,0uy   // snow_layer             -> carpet   // TODO will never be used, since below hmIgnoringLeaves, yes?
                                    80uy,35uy,0uy    // snow                   -> wool
                                    82uy,1uy,0uy     // clay                   -> stone
                                    110uy,2uy,0uy    // mycelium               -> grass
                                    159uy,172uy,0uy  // stained_hardened_clay  -> hardened_clay
                                    172uy,159uy,0uy  // hardened_clay          -> stained_hardened_clay
                                |]
                            let subst(x,z,wideVal) =
                                let mutable y = hmIgnoringLeaves.[x,z]
                                let mutable ok = false
                                while not ok do
                                    let bid = map.GetBlockInfo(x,y,z).BlockID
                                    if bid = 9uy && map.GetBlockInfo(x,y-1,z).BlockID = 9uy then
                                        ok <- true
                                        // two-deep water, special-case it
                                        if wideVal = 0 then // only in center of path
                                            map.SetBlockIDAndDamage(x,y-1,z,79uy,0uy) // ice one below surface
                                    else
                                        match TABLE |> Array.tryFind (fun (orig,_new,_) -> orig=bid) with
                                        | None -> y <- y - 1
                                        | Some(_,nbid,ndmg) ->
                                            ok <- true
                                            if nbid = 110uy && map.GetBlockInfo(x,y+1,z).BlockID = 9uy then
                                                // mycelium placed under water will die, use sand instead
                                                map.SetBlockIDAndDamage(x,y,z,12uy,0uy)
                                            else
                                                if bid = 12uy && map.GetBlockInfo(x,y,z).BlockData = 1uy then
                                                    map.SetBlockIDAndDamage(x,y,z,180uy,4uy)  // red sand -> upside-down red sandstone stairs
                                                else
                                                    map.SetBlockIDAndDamage(x,y,z,nbid,ndmg)
                                                if map.GetBlockInfo(x,y+1,z).BlockID = 78uy then // 78=snow_layer above
                                                    map.SetBlockIDAndDamage(x,y+1,z,0uy,0uy)     // replace snow_layer with air, for visibility (hard enough to see as-is)
                            for i = 0 to 7 do
                                let dx,dz = DIRS.[i]
                                let ax,az = DIRS.[(i+2)%8]  // right angle
                                let mutable ix,iz = cx+dx*4, cz+dz*4
                                let A = TELEPORT_PATH_OUT_DISTANCES
                                let isDiagonal = abs(dx)+abs(dz) = 2 
                                for dist = 0 to A.[A.Length-1] do
                                    ix <- ix + dx
                                    iz <- iz + dz
                                    // remove trees
                                    for w in TREEWIDE do
                                        let x,z = ix+w*ax, iz+w*az
                                        if allTrees <> null then
                                            allTrees.Remove(x,z,map)
                                        // if we're on a diagonal, we only cover every other block, so kludge it thusly (x+1)
                                        if isDiagonal && allTrees <> null then
                                            allTrees.Remove(x+1,z,map)
                                    // make path
                                    let w = rng.Next(WIDE.Length)
                                    if dist > 5 && dist % A.[1] > 5 then
                                        if dist<A.[0] && dist%2=0 || dist<A.[1] && dist%3=0 || dist<A.[2] && dist%4=0 || dist<A.[3] && dist%5=0 || dist<A.[4] && dist%6=0 then
                                            subst(ix+WIDE.[w]*ax, iz+WIDE.[w]*az, WIDE.[w])
                                    // occasional lit chests to see from afar
                                    elif dist > 5 && dist % A.[1] = 2 then
                                        let mutable y = hmIgnoringLeaves.[ix,iz]
                                        // just removed some trees, so this may be atop a stump that's no longer there, so move down until ground
                                        while HM_IGNORING_LEAVES_SKIPPABLE_DOWN_BLOCKS.Contains(map.GetBlockInfo(ix,y,iz).BlockID) do
                                            y <- y - 1
                                        let chestItems = Compounds[| [| Byte("Count",1uy); Byte("Slot",13uy); Short("Damage",0s); String("id","minecraft:emerald"); End |] |]
                                        putTrappedChestWithItemsAt(ix,y,iz,Strings.NAME_OF_TELEPORTER_BREADCRUMBS_CHEST,chestItems,map,null)
                                        map.SetBlockIDAndDamage(ix,y-1,iz,0uy,0uy) // 0=air
                                        map.SetBlockIDAndDamage(ix,y-2,iz,76uy,5uy) // 76=redstone_torch
                                        map.SetBlockIDAndDamage(ix,y-3,iz,1uy,5uy) // 1,5=andesite
                                        if map.GetBlockInfo(ix,y+1,iz).BlockID = 78uy then // 78=snow_layer above
                                            map.SetBlockIDAndDamage(ix,y+1,iz,0uy,0uy)     // replace snow_layer with air, for visibility (hard enough to see as-is)
                                        decorations.Add('*',ix,iz) // TODO will this interfere with dungeon placement?
                                        // make an arrow/chevron pointing in right direction
                                        subst(ix-dx, iz-dz, 0)
                                        subst(ix+ax, iz+az, 0)
                                        subst(ix-ax, iz-az, 0)
                                        subst(ix+dx+2*ax, iz+dz+2*az, 0)
                                        subst(ix+dx-2*ax, iz+dz-2*az, 0)
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




let makeCrazyMap(worldSaveFolder, rngSeed, customTerrainGenerationOptions, mapTimeInHours) =
    let rng = ref(System.Random())
    let mainTimer = System.Diagnostics.Stopwatch.StartNew()
    let map = new MapFolder(worldSaveFolder + """\region\""")
    let log = EventAndProgressLog()
    let decorations = ResizeArray()
    let colorCount = Array.zeroCreate 16
    let hm = Array2D.zeroCreateBased MINIMUM MINIMUM LENGTH LENGTH
    let hmIgnoringLeaves = Array2D.zeroCreateBased MINIMUM MINIMUM LENGTH LENGTH
    let biome = Array2D.zeroCreateBased MINIMUM MINIMUM LENGTH LENGTH
    let origBiome = Array2D.zeroCreateBased MINIMUM MINIMUM LENGTH LENGTH
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
    // verify matches level.dat
    let levelDatFilename = System.IO.Path.Combine(worldSaveFolder, "level.dat")
    let nbt = Utilities.readDatFile(levelDatFilename)
    let f _pl nbt =
        match nbt with 
        | NBT.String("generatorOptions",oldgo) -> 
            if oldgo <> customTerrainGenerationOptions then
                let goal = customTerrainGenerationOptions.Split(',')
                let actual = oldgo.Split(',')
                Utilities.diffStringArrays(goal, actual) |> ignore
                failwith "gen options no match"
            nbt
        | _ -> nbt
    cataNBT f (fun _pl nbt -> nbt) [] nbt |> ignore
    log.LogSummary("-------------------")
    time (fun () ->
        let LOX, LOY, LOZ = MINIMUM, 1, MINIMUM
        let HIY = 255
        printf "CACHE SECT"
        // TODO this is creating every empty sky section, making chunks heavier...
        // TODO consider altering API so unrepresented sections can still return implicit block info when read (but not when written)
        map.GetOrCreateAllSections(LOX,LOX+LENGTH-1,LOY,HIY,LOZ,LOZ+LENGTH-1)
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
                origBiome.[x,z] <- biome.[x,z]
                let h = map.GetHeightMap(x,z)
                hm.[x,z] <- h
                let mutable y = h
                while HM_IGNORING_LEAVES_SKIPPABLE_DOWN_BLOCKS.Contains(map.MaybeGetBlockInfo(x,y,z).BlockID) do
                    y <- y - 1
                hmIgnoringLeaves.[x,z] <- y
        )
    let allTrees = ref null
//    xtime (fun () -> findMountainToHollowOut(map, hm, hmIgnoringLeaves, log, decorations))  // TODO eventually use?
    time (fun () -> allTrees := treeify(map, hm))
    time (fun () -> placeTeleporters(!rng, map, hm, hmIgnoringLeaves, log, decorations, !allTrees))
    time (fun () -> doubleSpawners(map, log))
    time (fun () -> substituteBlocks(!rng, map, log))
    time (fun () -> findUndergroundAirSpaceConnectedComponents(!rng, map, hm, log, decorations))
    time (fun () -> findSomeMountainPeaks(!rng, map, hm, hmIgnoringLeaves, log, biome, decorations, !allTrees))
    time (fun () -> findSomeFlatAreas(!rng, map, hm, hmIgnoringLeaves, log, decorations))
    time (fun () -> findCaveEntrancesNearSpawn(map,hm,hmIgnoringLeaves,log))
    time (fun () -> replaceSomeBiomes(!rng, map, log, biome, !allTrees)) // after treeify, so can use allTrees, after placeTeleporters so can do ground-block-substitution cleanly
    time (fun () -> addRandomLootz(!rng, map, log, hm, hmIgnoringLeaves, biome, decorations, !allTrees, colorCount))  // after others, reads decoration locations and replaced biomes
    time (fun() ->   // after hiding spots figured
        log.LogSummary("COMPASS CMDS")
        placeCompassCommands(map,log))
    time (fun() ->   // after hiding spots figured (puts on scoreboard, but not using that, so could remove and then order not matter)
        log.LogSummary("START CMDS")
        let DEBUG_CHESTS = false
        placeStartingCommands(worldSaveFolder,map,hmIgnoringLeaves,!allTrees, mapTimeInHours, DEBUG_CHESTS, colorCount))
    time (fun () -> 
        log.LogSummary("RELIGHTING THE WORLD")
        RecomputeLighting.relightTheWorldHelper(map,[-2..1],[-2..1],false)) // right before we save
    time (fun() ->
        log.LogSummary("SAVING FILES")
        map.WriteAll()
        printfn "...done!")
    time (fun() -> 
        log.LogSummary("WRITING MAP PNG IMAGES")
        let teleporterCenters = decorations |> Seq.filter (fun (c,_,_) -> c='T') |> Seq.map(fun (_,x,z) -> x,z,TELEPORT_PATH_OUT_DISTANCES.[TELEPORT_PATH_OUT_DISTANCES.Length-1])
        Utilities.makeBiomeMap(worldSaveFolder+"""\region""", map, origBiome, biome, hmIgnoringLeaves, MINIMUM, LENGTH, MINIMUM, LENGTH, 
                                [DAYLIGHT_RADIUS; SPAWN_PROTECTION_DISTANCE_GREEN; SPAWN_PROTECTION_DISTANCE_FLAT; SPAWN_PROTECTION_DISTANCE_PEAK; SPAWN_PROTECTION_DISTANCE_PURPLE], 
                                teleporterCenters, decorations)
        )
    log.LogSummary(sprintf "Took %f total minutes" mainTimer.Elapsed.TotalMinutes)
    let now = System.DateTime.Now.ToString()
    let worldSeed = Utilities.readWorldSeedFromLevelDat(System.IO.Path.Combine(worldSaveFolder, "level.dat"))
    log.LogSummary(sprintf "This map was produced with seed %d on %s" worldSeed now)
    log.LogSummary(sprintf "Customization: SINGLEPLAYER %A, UHC_MODE %A" CustomizationKnobs.SINGLEPLAYER CustomizationKnobs.UHC_MODE)

    for xc,xf in ['W', (fun x -> x < 0); 'E', (fun x -> x>=0)] do
        for zc,zf in ['N', (fun x -> x < 0); 'S', (fun x -> x>=0)] do
            let numB = decorations |> Seq.filter (fun (c,x,z) -> c='B' && xf x && zf z) |> Seq.length 
            let numF = decorations |> Seq.filter (fun (c,x,z) -> c='F' && xf x && zf z) |> Seq.length 
            let numP = decorations |> Seq.filter (fun (c,x,z) -> c='P' && xf x && zf z) |> Seq.length 
            log.LogSummary(sprintf "%c%c quadrant has %d green beacons, %d flat dungeons, and %d mountain peaks" zc xc numB numF numP)
    printfn ""
    printfn "SUMMARY"
    printfn ""
    for s in log.SummaryEvents() do
        printfn "%s" s
    System.IO.File.WriteAllLines(System.IO.Path.Combine(worldSaveFolder,"summary.txt"),log.SummaryEvents())
    System.IO.File.WriteAllLines(System.IO.Path.Combine(worldSaveFolder,"all.txt"),log.AllEvents())
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

-----

Jan 12 playthrough (seed 106)
 - 30 mins, first dungeon, 10 iron, 24 gold (too many apples, not enough cookies?)
 - 1.25 hours, found green beacon, started, backed off to get iron for lava bucket, found 2nd dungeon and good horse, prot2 gold pants drop, still short on iron
 - 1.75 hours, beat green beacon, organized (no deaths), still short on iron, but have some decent mix gold/iron armor
 - 2.25 hours, died repeatedly at red beacon door, lost everything (spawners too dense?) never saw peak, never saw set piece, very hard to recover (back to stone/gold and no horse, no good ench)
*)



(*

Jan29 (recompute my own lighting)

Debugging output for automated map generation
DON'T READ THIS UNLESS YOU WANT SPOILERS
-------------------
Terrain generation options:
{"coordinateScale":684.412,"heightScale":684.412,"lowerLimitScale":512.0,"upperLimitScale":512.0,"depthNoiseScaleX":200.0,"depthNoiseScaleZ":200.0,"depthNoiseScaleExponent":0.5,"mainNoiseScaleX":80.0,"mainNoiseScaleY":160.0,"mainNoiseScaleZ":80.0,"baseSize":8.5,"stretchY":12.0,"biomeDepthWeight":1.0,"biomeDepthOffset":0.0,"biomeScaleWeight":1.0,"biomeScaleOffset":0.0,"seaLevel":63,"useCaves":true,"useDungeons":true,"dungeonChance":35,"useStrongholds":false,"useVillages":false,"useMineShafts":true,"useTemples":false,"useMonuments":false,"useRavines":true,"useWaterLakes":true,"waterLakeChance":25,"useLavaLakes":true,"lavaLakeChance":80,"useLavaOceans":false,"fixedBiome":-1,"biomeSize":3,"riverSize":4,"dirtSize":33,"dirtCount":90,"dirtMinHeight":0,"dirtMaxHeight":256,"gravelSize":33,"gravelCount":8,"gravelMinHeight":0,"gravelMaxHeight":256,"graniteSize":3,"graniteCount":12,"graniteMinHeight":0,"graniteMaxHeight":80,"dioriteSize":12,"dioriteCount":120,"dioriteMinHeight":0,"dioriteMaxHeight":80,"andesiteSize":33,"andesiteCount":0,"andesiteMinHeight":0,"andesiteMaxHeight":80,"coalSize":17,"coalCount":20,"coalMinHeight":0,"coalMaxHeight":128,"ironSize":9,"ironCount":5,"ironMinHeight":0,"ironMaxHeight":58,"goldSize":9,"goldCount":5,"goldMinHeight":0,"goldMaxHeight":62,"redstoneSize":3,"redstoneCount":4,"redstoneMinHeight":0,"redstoneMaxHeight":32,"diamondSize":4,"diamondCount":1,"diamondMinHeight":0,"diamondMaxHeight":16,"lapisSize":7,"lapisCount":1,"lapisCenterHeight":16,"lapisSpread":16}
-------------------
(this section took 0.158097 minutes)
-----
CACHE HM AND BIOME...
(this section took 0.022957 minutes)
-----
(this section took 1.165817 minutes)
-----
SKIPPED SOMETHING
TP at -542 -500
TP at -542 482
TP at 482 492
TP at 492 -528
(this section took 0.000897 minutes)
-----
added 717 extra dungeon spawners underground
(this section took 0.292352 minutes)
-----
added random spawners underground
   rand spawners from granite:   Total:1793   Blaze: 99   Creeper: 95   Skeleton:499   Spider:558   Zombie:542
   rand spawners from redstone:   Total:740   Blaze:120   CaveSpider:128   Creeper:113   Skeleton:134   Spider:117   Zombie:128
(this section took 0.831089 minutes)
-----
added FINAL beacon at -556 59 -480 which travels 438
   spawners along path:   Total:103   CaveSpider: 10   Creeper: 21   Skeleton: 13   Witch: 10   Zombie: 49
added beacon at -596 59 314 which travels 454
   spawners along path:   Total: 74   Creeper:  8   Skeleton: 13   Zombie: 53
added beacon at -399 53 554 which travels 173
   spawners along path:   Total: 24   Skeleton:  4   Zombie: 20
added beacon at -180 59 -361 which travels 217
   spawners along path:   Total: 26   Creeper:  4   Skeleton:  3   Zombie: 19
added beacon at -152 39 512 which travels 382
   spawners along path:   Total: 63   Creeper: 10   Skeleton:  7   Zombie: 46
added beacon at 426 59 -17 which travels 147
   spawners along path:   Total: 19   Creeper:  3   Skeleton:  3   Zombie: 13
added beacon at 145 56 580 which travels 190
   spawners along path:   Total: 25   Creeper:  3   Skeleton:  7   Zombie: 15
added beacon at 294 59 -417 which travels 258
   spawners along path:   Total: 42   Creeper:  3   Skeleton:  2   Zombie: 37
added beacon at 475 58 170 which travels 198
   spawners along path:   Total: 37   Creeper:  4   Skeleton:  9   Zombie: 24
(this section took 1.352665 minutes)
-----
best hiding spot:  -32  120 -757
('find best hiding spot' sub-section took 0.152650 minutes)
added mountain peak (score 9151) at 965 123 816
   spawners around mountain peak:   Total: 64   Blaze:  2   CaveSpider: 24   Ghast:  3   Spiderextra: 13   Zombie: 22
added mountain peak (score 8366) at 73 109 834
   spawners around mountain peak:   Total: 60   Blaze:  2   CaveSpider: 24   Ghast:  3   Spiderextra: 15   Zombie: 16
added mountain peak (score 8167) at -51 116 543
   spawners around mountain peak:   Total: 47   Blaze:  2   CaveSpider: 13   Ghast:  2   Spiderextra: 17   Zombie: 13
added mountain peak (score 7620) at 849 108 117
   spawners around mountain peak:   Total: 49   Blaze:  1   CaveSpider: 20   Ghast:  3   Spiderextra:  8   Zombie: 17
added mountain peak (score 6353) at 153 116 -986
   spawners around mountain peak:   Total: 48   Blaze:  5   CaveSpider: 15   Ghast:  1   Spiderextra: 12   Zombie: 15
added mountain peak (score 5610) at 352 125 -709
   spawners around mountain peak:   Total: 56   Blaze:  6   CaveSpider: 22   Ghast:  7   Spiderextra:  6   Zombie: 15
added mountain peak (score 5338) at 737 121 620
   spawners around mountain peak:   Total: 56   Blaze:  4   CaveSpider: 16   Ghast:  3   Spiderextra: 15   Zombie: 18
added mountain peak (score 972) at 976 110 -547
   spawners around mountain peak:   Total: 55   Blaze:  5   CaveSpider: 18   Ghast:  5   Spiderextra: 12   Zombie: 15
(this section took 0.165676 minutes)
-----
added flat set piece (score 11237353) at -74 -806
   spawners around cobweb flat:   Total: 87   Blaze:  4   CaveSpider: 30   Spider: 35   Spiderextra:  3   Witch: 15
added flat set piece (score 4975962) at -698 233
   spawners around cobweb flat:   Total: 94   Blaze:  4   CaveSpider: 31   Spider: 36   Spiderextra:  7   Witch: 16
added flat set piece (score 3293543) at -449 -791
   spawners around cobweb flat:   Total: 94   Blaze:  4   CaveSpider: 39   Spider: 27   Spiderextra:  5   Witch: 19
added flat set piece (score 1743719) at -857 912
   spawners around cobweb flat:   Total: 81   Blaze:  4   CaveSpider: 35   Spider: 28   Spiderextra:  4   Witch: 10
added flat set piece (score 891410) at -762 -358
   spawners around cobweb flat:   Total: 90   Blaze:  4   CaveSpider: 27   Spider: 39   Spiderextra:  5   Witch: 15
added flat set piece (score -99441) at 626 147
   spawners around cobweb flat:   Total: 90   Blaze:  4   CaveSpider: 37   Spider: 30   Spiderextra:  3   Witch: 16
added flat set piece (score -121885) at 350 -165
   spawners around cobweb flat:   Total: 88   Blaze:  4   CaveSpider: 35   Spider: 29   Spiderextra:  5   Witch: 15
added flat set piece (score -593090) at 14 375
   spawners around cobweb flat:   Total: 84   Blaze:  4   CaveSpider: 34   Spider: 27   Spiderextra:  5   Witch: 14
added flat set piece (score -880025) at 784 581
   spawners around cobweb flat:   Total: 84   Blaze:  4   CaveSpider: 33   Spider: 32   Spiderextra:  2   Witch: 13
added flat set piece (score -894701) at 73 741
   spawners around cobweb flat:   Total: 87   Blaze:  4   CaveSpider: 30   Spider: 33   Spiderextra:  4   Witch: 16
added set piece (score -941403) at 419 870
   spawners around set piece:   Total:  8   Ghast:  4   Skeleton:  1   Zombie:  3
added set piece (score -1254021) at -920 -59
   spawners around set piece:   Total:  8   Ghast:  4   Zombie:  4
added set piece (score -2532655) at 958 92
   spawners around set piece:   Total:  8   Ghast:  4   Zombie:  4
added set piece (score -2923044) at 339 -915
   spawners around set piece:   Total:  8   Ghast:  4   Skeleton:  1   Zombie:  3
added set piece (score -3026605) at -462 19
   spawners around set piece:   Total:  8   Ghast:  4   Zombie:  4
(this section took 0.210884 minutes)
-----
highlighted 8 cave entrances near spawn
(this section took 0.012609 minutes)
-----
added 304 extra loot chests
(this section took 0.168927 minutes)
-----
Added 4 Hell biomes (10 trees) and 40 Sky biomes (155 trees) replacing some Plains
(this section took 0.011873 minutes)
-----
START CMDS
(this section took 0.000756 minutes)
-----
RELIGHTING THE WORLD
(this section took 0.817642 minutes)
-----
SAVING FILES
(this section took 0.989743 minutes)
-----
WRITING MAP PNG IMAGES
(this section took 0.531277 minutes)
-----
Took 6.734359 total minutes
NW quadrant has 1 green beacons, 3 flat dungeons, and 0 mountain peaks
SW quadrant has 3 green beacons, 2 flat dungeons, and 1 mountain peaks
NE quadrant has 2 green beacons, 1 flat dungeons, and 3 mountain peaks
SE quadrant has 2 green beacons, 4 flat dungeons, and 4 mountain peaks







Jan 11 (have learned release/optimize)

Debugging output for automated map generation
DON'T READ THIS UNLESS YOU WANT SPOILERS
-------------------
Terrain generation options:
{"coordinateScale":684.412,"heightScale":684.412,"lowerLimitScale":512.0,"upperLimitScale":512.0,"depthNoiseScaleX":200.0,"depthNoiseScaleZ":200.0,"depthNoiseScaleExponent":0.5,"mainNoiseScaleX":80.0,"mainNoiseScaleY":160.0,"mainNoiseScaleZ":80.0,"baseSize":8.5,"stretchY":12.0,"biomeDepthWeight":1.0,"biomeDepthOffset":0.0,"biomeScaleWeight":1.0,"biomeScaleOffset":0.0,"seaLevel":63,"useCaves":true,"useDungeons":true,"dungeonChance":35,"useStrongholds":false,"useVillages":false,"useMineShafts":true,"useTemples":false,"useMonuments":false,"useRavines":true,"useWaterLakes":true,"waterLakeChance":25,"useLavaLakes":true,"lavaLakeChance":80,"useLavaOceans":false,"fixedBiome":-1,"biomeSize":3,"riverSize":4,"dirtSize":33,"dirtCount":90,"dirtMinHeight":0,"dirtMaxHeight":256,"gravelSize":33,"gravelCount":8,"gravelMinHeight":0,"gravelMaxHeight":256,"graniteSize":3,"graniteCount":12,"graniteMinHeight":0,"graniteMaxHeight":80,"dioriteSize":12,"dioriteCount":120,"dioriteMinHeight":0,"dioriteMaxHeight":80,"andesiteSize":33,"andesiteCount":0,"andesiteMinHeight":0,"andesiteMaxHeight":80,"coalSize":17,"coalCount":20,"coalMinHeight":0,"coalMaxHeight":128,"ironSize":9,"ironCount":5,"ironMinHeight":0,"ironMaxHeight":58,"goldSize":9,"goldCount":5,"goldMinHeight":0,"goldMaxHeight":62,"redstoneSize":3,"redstoneCount":4,"redstoneMinHeight":0,"redstoneMaxHeight":32,"diamondSize":4,"diamondCount":1,"diamondMinHeight":0,"diamondMaxHeight":16,"lapisSize":7,"lapisCount":1,"lapisCenterHeight":16,"lapisSpread":16}
-------------------
(this section took 0.158570 minutes)
-----
CACHE HM AND BIOME...
(this section took 0.022084 minutes)
-----
(this section took 1.695067 minutes)
-----
(this section took 0.002612 minutes)
-----
TP at -542 -510
TP at -523 490
TP at 482 501
TP at 484 -515
(this section took 0.000664 minutes)
-----
added 878 extra dungeon spawners underground
(this section took 0.327069 minutes)
-----
added random spawners underground
   rand spawners from granite:   Total:1821   Blaze:116   Creeper:106   Skeleton:510   Spider:552   Zombie:537
   rand spawners from redstone:   Total:836   Blaze:139   CaveSpider:125   Creeper:148   Skeleton:132   Spider:141   Zombie:151
(this section took 0.599089 minutes)
-----
There are 36 CCs with the desired property
(-411,11,-84) is 571 blocks from (-539,52,-149)
(-720,11,-613) is 521 blocks from (-698,57,-747)
(-817,11,73) is 253 blocks from (-747,51,109)
(-297,13,916) is 211 blocks from (-273,56,945)
(-744,11,-83) is 614 blocks from (-626,57,0)
(-792,11,-603) is 287 blocks from (-761,59,-632)
(-813,11,718) is 136 blocks from (-794,32,741)
(-478,11,710) is 328 blocks from (-512,59,609)
(-618,12,-348) is 147 blocks from (-651,40,-365)
(-514,11,-621) is 152 blocks from (-534,59,-650)
(-430,11,456) is 216 blocks from (-518,47,417)
added FINAL beacon at -518 47 417 which travels 216
   spawners along path:   Total: 65   CaveSpider: 15   Creeper:  8   Skeleton: 10   Witch:  6   Zombie: 26
added side path length 31
added side path length 24
added side path length 19
(-291,13,-44) is 171 blocks from (-255,53,-24)
added beacon at -255 53 -24 which travels 171
   spawners along path:   Total: 28   Creeper:  5   Skeleton:  1   Zombie: 22
added side path length 31
(-358,13,-314) is 280 blocks from (-364,34,-292)
added beacon at -364 34 -292 which travels 280
   spawners along path:   Total: 46   Creeper:  7   Skeleton:  5   Zombie: 34
added side path length 22
added side path length 27
added side path length 33
added side path length 27
added side path length 19
added side path length 37
added side path length 18
added side path length 36
(-314,13,218) is 196 blocks from (-293,36,301)
added beacon at -293 36 301 which travels 196
   spawners along path:   Total: 27   Creeper:  1   Skeleton:  5   Zombie: 21
added side path length 34
added side path length 37
added side path length 21
added side path length 37
added side path length 34
added side path length 34
added side path length 17
added side path length 36
added side path length 38
added side path length 15
(-33,12,692) is 407 blocks from (-156,43,672)
added beacon at -156 43 672 which travels 407
   spawners along path:   Total: 67   Creeper:  6   Skeleton: 13   Zombie: 48
added side path length 30
added side path length 33
added side path length 30
added side path length 38
(-158,11,-406) is 535 blocks from (-258,59,-408)
(236,11,-6) is 183 blocks from (217,51,-59)
added beacon at 217 51 -59 which travels 183
   spawners along path:   Total: 36   Creeper:  2   Skeleton:  5   Zombie: 29
added side path length 10
added side path length 19
added side path length 39
(8,13,-611) is 134 blocks from (-16,47,-626)
added beacon at -16 47 -626 which travels 134
   spawners along path:   Total: 17   Creeper:  2   Skeleton:  3   Zombie: 12
added side path length 33
added side path length 29
added side path length 29
added side path length 38
added side path length 32
added side path length 27
(272,11,-408) is 635 blocks from (51,59,-539)
(291,11,-218) is 312 blocks from (304,56,-119)
added beacon at 304 56 -119 which travels 312
   spawners along path:   Total: 50   Creeper:  9   Skeleton: 10   Zombie: 31
added side path length 18
added side path length 39
added side path length 31
(301,11,362) is 116 blocks from (306,56,409)
added beacon at 306 56 409 which travels 116
   spawners along path:   Total: 19   Creeper:  3   Skeleton:  1   Zombie: 15
added side path length 35
added side path length 27
added side path length 19
(409,12,-604) is 431 blocks from (349,56,-618)
(533,12,17) is 511 blocks from (402,45,227)
(799,11,-697) is 181 blocks from (846,57,-754)
(653,11,283) is 206 blocks from (693,59,306)
(616,13,164) is 611 blocks from (792,37,-32)
(851,11,-449) is 324 blocks from (681,30,-447)
(779,13,-373) is 198 blocks from (878,54,-406)
(886,12,-625) is 208 blocks from (927,45,-628)
(932,11,-176) is 391 blocks from (883,59,-153)
(this section took 1.207022 minutes)
-----
added flat set piece (score 3007097) at -543 300
   spawners around cobweb flat:   Total:103   Blaze:  4   CaveSpider: 35   Spider: 27   Spiderextra: 10   Witch: 27
added flat set piece (score 1895511) at -305 482
   spawners around cobweb flat:   Total: 95   Blaze:  4   CaveSpider: 33   Spider: 33   Spiderextra:  6   Witch: 19
added flat set piece (score -258250) at -745 5
   spawners around cobweb flat:   Total:102   Blaze:  4   CaveSpider: 35   Spider: 42   Spiderextra:  8   Witch: 13
added flat set piece (score -331480) at -706 503
   spawners around cobweb flat:   Total:101   Blaze:  4   CaveSpider: 38   Spider: 35   Spiderextra:  7   Witch: 17
added flat set piece (score -428310) at -638 885
   spawners around cobweb flat:   Total:104   Blaze:  4   CaveSpider: 39   Spider: 37   Spiderextra:  3   Witch: 21
added flat set piece (score -430885) at 898 -366
   spawners around cobweb flat:   Total:102   Blaze:  4   CaveSpider: 42   Spider: 31   Spiderextra:  6   Witch: 19
added flat set piece (score -488065) at 963 838
   spawners around cobweb flat:   Total:104   Blaze:  4   CaveSpider: 34   Spider: 36   Spiderextra:  7   Witch: 23
added flat set piece (score -555861) at 109 371
   spawners around cobweb flat:   Total: 93   Blaze:  4   CaveSpider: 37   Spider: 25   Spiderextra:  6   Witch: 21
added flat set piece (score -702542) at -696 -193
   spawners around cobweb flat:   Total:101   Blaze:  4   CaveSpider: 40   Spider: 33   Spiderextra:  7   Witch: 17
added flat set piece (score -795322) at 574 -423
   spawners around cobweb flat:   Total: 98   Blaze:  4   CaveSpider: 37   Spider: 36   Spiderextra:  2   Witch: 19
added set piece (score -1786277) at 10 624
   spawners around set piece:   Total:  8   Ghast:  4   Skeleton:  1   Zombie:  3
added set piece (score -1891785) at 933 -763
   spawners around set piece:   Total:  8   Ghast:  4   Zombie:  4
added set piece (score -2316893) at -954 -462
   spawners around set piece:   Total:  8   Ghast:  4   Zombie:  4
added set piece (score -2615455) at -676 -529
   spawners around set piece:   Total:  8   Ghast:  4   Skeleton:  1   Zombie:  3
added set piece (score -2692687) at 409 -739
   spawners around set piece:   Total:  8   Ghast:  4   Skeleton:  2   Zombie:  2
added set piece (score -3011300) at -323 804
   spawners around set piece:   Total:  8   Ghast:  4   Skeleton:  2   Zombie:  2
added set piece (score -3215198) at 725 -234
   spawners around set piece:   Total:  8   Ghast:  4   Zombie:  4
added set piece (score -3801827) at 233 803
   spawners around set piece:   Total:  8   Ghast:  4   Zombie:  4
added set piece (score -3929946) at 974 -149
   spawners around set piece:   Total:  8   Ghast:  4   Skeleton:  1   Zombie:  3
added set piece (score -5129940) at 273 -909
   spawners around set piece:   Total:  8   Ghast:  4   Skeleton:  2   Zombie:  2
(this section took 0.259758 minutes)
-----
best hiding spot:  377  121  722
('find best hiding spot' sub-section took 0.147781 minutes)
added mountain peak (score 9594) at -520 105 -758
   spawners around mountain peak:   Total: 75   Blaze:  1   CaveSpider: 29   Ghast:  6   Spiderextra: 21   Zombie: 18
added mountain peak (score 8865) at -840 114 913
   spawners around mountain peak:   Total: 77   Blaze:  5   CaveSpider: 33   Ghast:  5   Spiderextra: 13   Zombie: 21
added mountain peak (score 8561) at -741 106 -848
   spawners around mountain peak:   Total: 70   Blaze:  7   CaveSpider: 26   Ghast:  2   Spiderextra: 16   Zombie: 19
added mountain peak (score 7864) at 568 106 -968
   spawners around mountain peak:   Total: 60   Blaze:  2   CaveSpider: 21   Ghast:  2   Spiderextra: 18   Zombie: 17
added mountain peak (score 7790) at 984 117 944
   spawners around mountain peak:   Total: 66   Blaze:  4   CaveSpider: 19   Ghast:  4   Spiderextra: 15   Zombie: 24
added mountain peak (score 7297) at -206 121 492
   spawners around mountain peak:   Total: 66   Blaze:  6   CaveSpider: 18   Ghast:  5   Spiderextra: 13   Zombie: 24
added mountain peak (score 6333) at -987 107 -985
   spawners around mountain peak:   Total: 61   Blaze:  4   CaveSpider: 21   Ghast:  5   Spiderextra: 15   Zombie: 16
added mountain peak (score 5784) at -827 120 -677
   spawners around mountain peak:   Total: 64   Blaze:  6   CaveSpider: 21   Ghast:  2   Spiderextra: 18   Zombie: 17
added mountain peak (score 4697) at 549 106 -593
   spawners around mountain peak:   Total: 67   Blaze:  4   CaveSpider: 24   Ghast:  8   Spiderextra: 14   Zombie: 17
added mountain peak (score 3665) at 897 113 -592
   spawners around mountain peak:   Total: 70   Blaze:  6   CaveSpider: 25   Ghast:  3   Spiderextra: 13   Zombie: 23
(this section took 0.154550 minutes)
-----
highlighted 13 cave entrances near spawn
(this section took 0.009072 minutes)
-----
added 341 extra loot chests: 47, 150, 14, 51, 7, 16, 43, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 13
(this section took 0.177114 minutes)
-----
found 45 decent-sized plains biomes outside DAYLIGHT_RADIUS
Added 1 Hell biomes (10 trees) and 31 Sky biomes (173 trees) replacing some Plains
(this section took 0.011299 minutes)
-----
START CMDS
(this section took 0.000668 minutes)
-----
SAVING FILES
(this section took 1.112201 minutes)
-----
WRITING MAP PNG IMAGES
(this section took 0.085014 minutes)
-----
Took 5.822694 total minutes




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