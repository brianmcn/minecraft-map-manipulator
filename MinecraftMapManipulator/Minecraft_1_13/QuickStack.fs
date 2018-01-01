module QuickStack

// TODO inventory sorter stuff for quality-of-life (would need to gift enderchest and silk touch early to utilize well in E&T)
// question: can one use /replaceitem on a shulker box in your inventory? (no)
// if not, a couple nice systems are
//  - quick stack to enderchest (invoked by moving enderchest to upper-right slot of inventory)
//               execute if entity @p[nbt={Inventory:[{Slot:17b,id:"minecraft:ender_chest"}]}] run say ec
//               execute store result score @p wp4y run data get entity Lorgon111 EnderItems[0].Count  // reads first occupied slot in enderchest, not Slot:0b
//                 master outside loop of all stackable-to-64-items (cobblestone...bone...) can do e.g.
//                   does ender have any X? if so, does it have space for any X (loop unfull stacks or empty slots)? If so, does player have any X in non-hotbar? 
//                     if so, compute amount to transfer from invslotN to enderslotM with /replaceitem and repeat until player is empty or enderchest is full
//  - quick stack to nearby chests (particle animations) - how to invoke? clickable sign?
// algorithms are complicated and may be expensive, work them out

let quickstack_functions = [|
    yield "init", [|
        for i = 9 to 35 do
            yield sprintf "scoreboard objectives add invslot%d dummy" i       // invslot4=60 means there is a count of 60 items in Inventory Slot:4b
        for i = 0 to 26 do
            yield sprintf "scoreboard objectives add enderslot%d dummy" i     // enderslot7=50 means there is a count of 50 items in EnderItems Slot:7b
        yield "scoreboard objectives add succ dummy"
        yield "scoreboard objectives add slot dummy"
        yield "scoreboard objectives add count dummy"
        //
        yield "scoreboard objectives add hasItem dummy"
        yield "scoreboard objectives add toFill dummy"
        yield "scoreboard objectives add space dummy"
        yield "scoreboard objectives add numMoved dummy"
        yield "scoreboard objectives add toSet dummy"
        |]
    for name,nbt,range in ["ender","EnderItems", [0..26]; "inv","Inventory",[0..35]] do  // note - must be 0..35 because e.g. Inventory[5] might be Slot:22b if lots of empy hotbar/inventory
        yield sprintf "compute_%s_counts" name,[|
            for i in range do
                yield sprintf "scoreboard players set $ENTITY %sslot%d 0" name i
            for i in range do
                yield sprintf "execute store success score $ENTITY succ run data get entity @p %s[%d].Slot 1" nbt i
                yield sprintf "execute store result score $ENTITY slot run data get entity @p %s[%d].Slot 1" nbt i
                yield sprintf "execute store result score $ENTITY count run data get entity @p %s[%d].Count 1" nbt i
                yield sprintf "function qs:%s_helper" name
            |]
        yield sprintf "%s_helper" name,[|
            for i in range do
                yield sprintf "execute if entity $SCORE(succ=1,slot=%d) run scoreboard players operation $ENTITY %sslot%d = $ENTITY count" i name i
            |]
        (*
        yield sprintf "debug_%s" name, [|
            for i in range do
                yield sprintf """tellraw @a ["slot %d has ",{"score":{"name":"$ENTITY","objective":"%sslot%d"}}," items"]""" i name i
            |]
        *)
        yield "check_cobblestone", [|
                """execute if entity @p[nbt={EnderItems:[{id:"minecraft:cobblestone"}]}] run function qs:check_cobblestone_body"""
            |]
        yield "check_cobblestone_body", [|
            // TODO this won't stack X into empty spaces before the first X - bug or feature?
            yield "scoreboard players set $ENTITY hasItem 0"
            for i = 0 to 26 do
                yield "scoreboard players set $ENTITY space 0"
                yield sprintf """execute if entity @p[nbt={EnderItems:[{Slot:%db,id:"minecraft:cobblestone"}]}] run scoreboard players set $ENTITY hasItem 1""" i
                yield sprintf """execute if entity @p[nbt={EnderItems:[{Slot:%db,id:"minecraft:cobblestone"}]}] run scoreboard players set $ENTITY space 64""" i
                yield sprintf """execute if entity @p[nbt={EnderItems:[{Slot:%db,id:"minecraft:cobblestone"}]}] run scoreboard players operation $ENTITY space -= $ENTITY enderslot%d""" i i
                yield sprintf """execute if entity $SCORE(hasItem=1,enderslot%d=0) run scoreboard players set $ENTITY space 64""" i
                yield sprintf """execute if entity $SCORE(space=1..) run scoreboard players set $ENTITY toFill %d""" i
                yield sprintf """execute if entity $SCORE(space=1..) run function qs:move_cobblestone"""

            |]
        yield "move_cobblestone", [|  // given a <toFill> (EC slot #) and a <space> (max count that can fit there yet, 64-current), finds some cobble in non-hotbar inv to move to EC slot ToFill
            for i = 9 to 35 do
                yield sprintf """execute if entity $SCORE(space=1..) if entity @p[nbt={Inventory:[{Slot:%db,id:"minecraft:cobblestone"}]}] run function qs:mv/move_cobblestone_from_%d""" i i
            |]
        for i = 9 to 35 do
            yield sprintf "mv/move_cobblestone_from_%d" i, [|
                // numMoved = min(space,invslot)
                sprintf "scoreboard players operation $ENTITY numMoved = $ENTITY space"
                sprintf "scoreboard players operation $ENTITY numMoved < $ENTITY invslot%d" i
                // update enderchest & data
                sprintf "function qs:incr_enderslot"
                // update invdata & inventory
                sprintf "scoreboard players operation $ENTITY invslot%d -= $ENTITY numMoved" i
                sprintf "scoreboard players operation $ENTITY toSet = $ENTITY invslot%d" i
                sprintf "function qs:inv/set_inv%d" i
                // update remaining space
                sprintf "scoreboard players operation $ENTITY space -= $ENTITY numMoved"
                // debug
                sprintf """tellraw @a ["moved ",{"score":{"name":"$ENTITY","objective":"numMoved"}}," items from inv slot %d to EC slot ",{"score":{"name":"$ENTITY","objective":"toFill"}}]""" i
                |]
        yield "incr_enderslot", [| // sets ES[toFill] <- ES[toFill] + numMoved
            for n = 0 to 64 do
                yield sprintf "execute if entity $SCORE(numMoved=%d) run function qs:incr/incr_enderslot_by_%d" n n
            |]
        for n = 1 to 64 do
            yield sprintf "incr/incr_enderslot_by_%d" n, [| // sets ES[toFill] <- ES[toFill] + %d
                for i = 0 to 26 do
                    yield sprintf "execute if entity $SCORE(toFill=%d) run scoreboard players add $ENTITY enderslot%d %d" i i n
                    yield sprintf "execute if entity $SCORE(toFill=%d) run scoreboard players operation $ENTITY toSet = $ENTITY enderslot%d" i i
                    yield sprintf "execute if entity $SCORE(toFill=%d) run function qs:end/set_end%d" i i
                |]
        for i = 9 to 35 do
            yield sprintf "inv/set_inv%d" i, [| // inv[%d] <- toSet
                yield sprintf "execute if entity $SCORE(toSet=0) run replaceitem entity @p inventory.%d air 1" (i-9)
                for n = 1 to 63 do
                    yield sprintf "execute if entity $SCORE(toSet=%d) run replaceitem entity @p inventory.%d cobblestone %d" n (i-9) n
                |]
        for i = 0 to 26 do
            yield sprintf "end/set_end%d" i, [| // end[%d] <- toSet
                for n = 1 to 64 do
                    yield sprintf "execute if entity $SCORE(toSet=%d) run replaceitem entity @p enderchest.%d cobblestone %d" n i n
                |]
        yield "quick_stack", [|
            "function qs:compute_ender_counts"
            "function qs:compute_inv_counts"
            "function qs:check_cobblestone"
            |]
    |]

let main() =
    printfn "how many items can safely stack into 64s: %d" MC_Constants.STACKABLE_TO_64_ITEM_IDS.Length  
    for s in MC_Constants.STACKABLE_TO_64_ITEM_IDS do
        printfn "%s" s
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "TestSize")
    Utilities.writeDatapackMeta(world,"qspack","quick stack")
    for name,code in quickstack_functions do
        Utilities.writeFunctionToDisk(world,"qspack","qs",name,code |> Array.map MC_Constants.compile)


