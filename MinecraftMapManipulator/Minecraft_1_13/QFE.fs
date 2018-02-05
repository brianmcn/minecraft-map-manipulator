module QFE

let SURVIVAL_OBTAINABLE_ITEM_IDS = MC_Constants.SURVIVAL_OBTAINABLE_ITEM_IDS

let ITEM_GROUP_PREFIXES = [|
    "birch_"
    "yellow_"
    "wooden_"
    "leather_"
    "music_disc_"
    |]

let ITEM_GROUP_SUFFIXES = [|
    "_ore"
    "_slab"
    "rail"
    "_stairs"
    "_block"
    "sandstone"
    "stone"
    "bucket"
    |]

let partitionItemList(items) =
    let partitionItemListCore(items:seq<string>, pres) =
        let partitions = ResizeArray()
        let remaining = ResizeArray(items)
        for pre in pres do
            let group = ResizeArray()
            let name = remaining.Find(fun s -> s.StartsWith(pre))
            let suffix = name.Substring(pre.Length)
            let all = remaining |> Seq.filter (fun s -> s.EndsWith(suffix)) |> Seq.toArray 
            let prefixes = all |> Array.map (fun s -> s.Substring(0,s.Length-suffix.Length))
            for candidate in remaining |> Seq.filter (fun s -> s.StartsWith(pre)) |> Seq.toArray do
                let cand_suf = candidate.Substring(pre.Length)
                let all = prefixes |> Array.map (fun s -> s + cand_suf)
                if all |> Seq.forall (fun s -> remaining.Contains(s)) then
                    group.AddRange(all)
                    for x in all do
                        remaining.Remove(x) |> ignore
            partitions.Add(prefixes.Length, group.ToArray())
        partitions.Add(1, remaining.ToArray())
        partitions.ToArray()
    let after_pre = partitionItemListCore(items,ITEM_GROUP_PREFIXES) |> ResizeArray
    let mutable remaining = after_pre.[after_pre.Count-1] |> snd
    after_pre.RemoveAt(after_pre.Count-1)
    (*
    let rev(s:string) = new string(s |> Seq.rev |> Seq.toArray)
    let reverse_remaining = remaining |> Array.map rev
    let reverse_parts = partitionItemListCore(reverse_remaining, ITEM_GROUP_SUFFIXES |> Array.map rev)
    let after_suf = reverse_parts |> Array.map (Array.map rev)
    *)
    let after_suf = ResizeArray()
    for suf in ITEM_GROUP_SUFFIXES do
        let those,others = remaining |> Array.partition (fun s -> s.EndsWith(suf))
        after_suf.Add(1, those)
        remaining <- others
    after_suf.Add(1, remaining)
    Array.append (after_pre.ToArray()) (after_suf.ToArray())

let main() =
    let r = partitionItemList(SURVIVAL_OBTAINABLE_ITEM_IDS)
    let show = false
    for i = 0 to r.Length-1 do
        let n,a = r.[i]
        if i = r.Length-1 then
            printfn "%4d = %3dx%3d = 4x%3d" a.Length n (a.Length/n) ((a.Length+3)/4)
            if show then
                for x in a do
                    printfn "%s" x
        else
            if show then 
                printfn "%4d = %3dx%3d = 4x%3d %A" a.Length n (a.Length/n) ((a.Length+3)/4) a
            else
                printfn "%4d = %3dx%3d = 4x%3d" a.Length n (a.Length/n) ((a.Length+3)/4)
    printfn "%d total items" SURVIVAL_OBTAINABLE_ITEM_IDS.Length 

    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "TestQFE")
    let compiler = Compiler.Compiler('q','f',"qfe",1,100,1,false)
    let pack = new Utilities.DataPackArchive(world,"QFE","qfe")
    let functions = ResizeArray()
    functions.Add("init_objectives_and_scores",[|
        yield sprintf "scoreboard objectives add gotAnItem dummy"
        yield sprintf "scoreboard objectives add Items dummy"
        for i = 1 to SURVIVAL_OBTAINABLE_ITEM_IDS.Length do
            yield sprintf "scoreboard objectives add notYet%03d dummy" i
        yield sprintf "scoreboard players set @p Items 0"
        for i = 1 to SURVIVAL_OBTAINABLE_ITEM_IDS.Length do
            yield sprintf "scoreboard players set $ENTITY notYet%03d 1" i
        |])
    functions.Add("main",[|
        for i = 1 to SURVIVAL_OBTAINABLE_ITEM_IDS.Length do
            yield sprintf "execute if $SCORE(notYet%03d=1) run function qfe:check/check%03d" i i
        |])
    let itemNum = ref 1
    let check(itemName, x, y, z, ix, iy, iz) =
//        if ((string)itemName).StartsWith("dia") then
//            printfn "%3d: %s" !itemNum itemName
        functions.Add(sprintf"check/check%03d"!itemNum,[|
            sprintf "scoreboard players set $ENTITY gotAnItem 0"
            sprintf "execute store success score $ENTITY gotAnItem run clear @p %s 0" itemName // todo multiplayer @p thruout
            sprintf "execute if $SCORE(gotAnItem=1) run function qfe:check/got%03d" !itemNum
        |])
        functions.Add(sprintf"check/got%03d"!itemNum,[|
            sprintf "scoreboard players set $ENTITY notYet%03d 0" !itemNum
            sprintf "scoreboard players add @p Items 1"
            sprintf "setblock %d %d %d emerald_block" x y z
            // todo consider /particle minecraft:dust 1.0 0.5 0.5 1 ^ ^2 ^2 0 0 0 0 20 force as it's more 'confineable' (r g b 1 x y z dx dy dz spd ct)
            // portal particles can move in a straight line.  
            // SirBenet: I think for a couple of other particles (flame, smoke) you can make them move in a straight line if count is 0 and dx/dy/dz are specified
            sprintf "particle minecraft:firework %d %d.8 %d 1.0 1.0 1.0 0.01 20" ix iy iz
            sprintf "particle minecraft:firework %d %d.8 %d 0.1 0.1 0.1 0.01 20" ix iy iz
            sprintf "execute at @p run playsound entity.firework.launch ambient @p ~ ~ ~"
            sprintf "$NTICKSLATER(25)"
            sprintf "particle minecraft:firework %d %d.8 %d 1.0 1.0 1.0 0.01 10" ix iy iz
            sprintf "particle minecraft:firework %d %d.8 %d 0.1 0.1 0.1 0.01 10" ix iy iz
            sprintf "execute at @p run playsound entity.firework.twinkle ambient @p ~ ~ ~"
            sprintf "$NTICKSLATER(25)"
            sprintf "particle minecraft:firework %d %d.8 %d 1.0 1.0 1.0 0.01 10" ix iy iz
            sprintf "particle minecraft:firework %d %d.8 %d 0.1 0.1 0.1 0.01 10" ix iy iz
        |])
        itemNum := !itemNum + 1

    let groups = r |> Array.map snd
    let X = 20
    let Y = 120
    let Z = 20
    
    let data = groups |> Array.find (fun a -> a |> Array.contains "yellow_wool") |> Array.rev 
    let groups = groups |> Seq.filter (fun a -> not(a |> Array.contains "yellow_wool")) |> Seq.toArray 
    let color_wall_item_count = data.Length 
    let wall1commands = ResizeArray()
    let x = X
    let FACING = 5
    let mutable z = Z
    let mutable y = Y + 3
    let mutable i = 0
    while i < data.Length do
        let item = data.[i]
        wall1commands.Add(sprintf "setblock %d %d %d redstone_block" x y z)
        wall1commands.Add(sprintf """summon item_frame %d %d %d {Invulnerable:1b,Facing:%db,Item:{id:"minecraft:%s",Count:1b,tag:{display:{Name:"%s"}}}}""" (x+1) y z FACING item item)
        check(item,x,y,z,x+1,y,z)
        i <- i + 1
        y <- y - 1
        if y<Y then 
            y <- Y+3
            z <- z+1

    let pred a = a |> Array.contains "oak_planks"
               ||a |> Array.contains "leather_helmet"
               ||a |> Array.contains "gold_ore"
               ||a |> Array.contains "rail"
               ||a |> Array.contains "purpur_stairs"
               ||a |> Array.contains "sandstone"
               ||a |> Array.contains "music_disc_cat"
               ||a |> Array.contains "bucket"
    let data = groups |> Array.filter pred |> Array.map (fun a -> if a |> Array.contains "leather_helmet" then Array.sort a else a) |> Array.collect id
    let groups = groups |> Array.filter (fun a -> not(pred a))
    let wall2commands = ResizeArray()
    let mutable x = X+2
    let FACING = 3
    let z = Z-2
    let mutable y = Y + 3
    let mutable i = 0
    while i < data.Length do
        let item = data.[i]
        wall2commands.Add(sprintf "setblock %d %d %d redstone_block" x y z)
        wall2commands.Add(sprintf """summon item_frame %d %d %d {Invulnerable:1b,Facing:%db,Item:{id:"minecraft:%s",Count:1b,tag:{display:{Name:"%s"}}}}""" x y (z+1) FACING item item)
        check(item,x,y,z,x,y,z+1)
        i <- i + 1
        y <- y - 1
        if y<Y then 
            y <- Y+3
            x <- x+1

    let remain = groups |> Array.collect id
    let wall3Items = remain.[0..color_wall_item_count-1]
    let wall4Items = remain.[color_wall_item_count..]

    let data = wall3Items
    let wall3commands = ResizeArray()
    let x = x+1
    let FACING = 4
    let mutable z = Z
    let mutable y = Y + 3
    let mutable i = 0
    while i < data.Length do
        let item = data.[i]
        wall3commands.Add(sprintf "setblock %d %d %d redstone_block" x y z)
        wall3commands.Add(sprintf """summon item_frame %d %d %d {Invulnerable:1b,Facing:%db,Item:{id:"minecraft:%s",Count:1b,tag:{display:{Name:"%s"}}}}""" (x-1) y z FACING item item)
        check(item,x,y,z,x-1,y,z)
        i <- i + 1
        y <- y - 1
        if y<Y then 
            y <- Y+3
            z <- z+1
    let biggest_x = x
    let biggest_z = z

    let data = wall4Items
    let wall4commands = ResizeArray()
    let mutable x = x-2
    let FACING = 2
    let z = z+1
    let mutable y = Y + 3
    let mutable i = 0
    while i < data.Length do
        let item = data.[i]
        wall4commands.Add(sprintf "setblock %d %d %d redstone_block" x y z)
        wall4commands.Add(sprintf """summon item_frame %d %d %d {Invulnerable:1b,Facing:%db,Item:{id:"minecraft:%s",Count:1b,tag:{display:{Name:"\"%s\""}}}}""" x y (z-1) FACING item item)
        check(item,x,y,z,x,y,z-1)
        i <- i + 1
        y <- y - 1
        if y<Y then 
            y <- Y+3
            x <- x-1


    functions.Add("wall1",wall1commands.ToArray())
    functions.Add("wall2",wall2commands.ToArray())
    functions.Add("wall3",wall3commands.ToArray())
    functions.Add("wall4",wall4commands.ToArray())
    functions.Add("floor",[|
        yield sprintf "fill %d %d %d %d %d %d light_gray_stained_glass" X (Y-1) (Z-2) biggest_x (Y-1) (biggest_z+1)
        yield sprintf "fill %d %d %d %d %d %d barrier" X (Y-2) (Z-2) biggest_x (Y-2) (biggest_z+1)
        let midx,midz = (X+(biggest_x-X)/2), (Z+(biggest_z-Z-2)/2)
        yield sprintf "setblock %d %d %d barrier" midx Y (midz-1)
        yield! Utilities.placeWallSignCmds midx Y midz "south" "click to" "CHECK" "inventory" "for items" "function qfe:main" true "black" ""
        |])
    functions.Add("clear",[|
        for y = Y-2 to Y+3 do
            yield sprintf "fill %d %d %d %d %d %d air" X y (Z-2) biggest_x y (biggest_z+1)
        |])
    functions.Add("init_all",[|
        "function qfe:init_objectives_and_scores"
        "function qfe:clear"
        "function qfe:wall1"
        "function qfe:wall2"
        "function qfe:wall3"
        "function qfe:wall4"
        "function qfe:floor"
        |])

    for ns,name,code in [for name,code in functions do yield! compiler.Compile("qfe",name,code)] do
        pack.WriteFunction(ns,name,code)
    for ns,name,code in compiler.GetCompilerLoadTick() do
        pack.WriteFunction(ns,name,code)
    pack.WriteFunctionTagsFileWithValues("minecraft","load",[compiler.LoadFullName])
    pack.WriteFunctionTagsFileWithValues("minecraft","tick",[compiler.TickFullName])

    pack.SaveToDisk()
        // TODO come up with a decent way to test it (spot-checking shows it works, but how try every single item?)

    //for i in SURVIVAL_OBTAINABLE_ITEM_IDS do
    //    printfn "%s" i


// invulnerable item frame can hold name item, survival player cannot break or retrieve (unless break block behind)




