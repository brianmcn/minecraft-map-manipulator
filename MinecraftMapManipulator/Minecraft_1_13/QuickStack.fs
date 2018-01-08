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

//let STACKABLES = [|"cobblestone";"diorite"|]  // TODO run out of RAM with more, test when launcher works to allocate RAM
//let STACKABLES = MC_Constants.STACKABLE_TO_64_ITEM_IDS
let STACKABLES = MC_Constants.PRAGMATIC_64_STACKABLES
(*
     Total Files Listed:
            8201 File(s)     46,004,791 bytes
*)

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
        for itemType in STACKABLES do
            yield sprintf "check_%s" itemType, [|
                    sprintf """execute if entity @p[nbt={EnderItems:[{id:"minecraft:%s"}]}] run function qs:check_%s_body""" itemType itemType
                |]
            yield sprintf "check_%s_body" itemType, [|
                // TODO this won't stack X into empty spaces before the first X - bug or feature?
                yield "scoreboard players set $ENTITY hasItem 0"
                for i = 0 to 26 do
                    yield "scoreboard players set $ENTITY space 0"
                    yield sprintf """execute if entity @p[nbt={EnderItems:[{Slot:%db,id:"minecraft:%s"}]}] run scoreboard players set $ENTITY hasItem 1""" i itemType
                    yield sprintf """execute if entity @p[nbt={EnderItems:[{Slot:%db,id:"minecraft:%s"}]}] run scoreboard players set $ENTITY space 64""" i itemType
                    yield sprintf """execute if entity @p[nbt={EnderItems:[{Slot:%db,id:"minecraft:%s"}]}] run scoreboard players operation $ENTITY space -= $ENTITY enderslot%d""" i itemType i
                    yield sprintf """execute if entity $SCORE(hasItem=1,enderslot%d=0) run scoreboard players set $ENTITY space 64""" i
                    yield sprintf """execute if entity $SCORE(space=1..) run scoreboard players set $ENTITY toFill %d""" i
                    yield sprintf """execute if entity $SCORE(space=1..) run function qs:move_%s""" itemType

                |]
            yield sprintf "move_%s" itemType, [|  // given a <toFill> (EC slot #) and a <space> (max count that can fit there yet, 64-current), finds some itemType in non-hotbar inv to move to EC slot ToFill
                for i = 9 to 35 do
                    yield sprintf """execute if entity $SCORE(space=1..) if entity @p[nbt={Inventory:[{Slot:%db,id:"minecraft:%s"}]}] run function qs:mv/move_%s_from_%d""" i itemType itemType i
                |]
            for i = 9 to 35 do
                yield sprintf "mv/move_%s_from_%d" itemType i, [|
                    // numMoved = min(space,invslot)
                    sprintf "scoreboard players operation $ENTITY numMoved = $ENTITY space"
                    sprintf "scoreboard players operation $ENTITY numMoved < $ENTITY invslot%d" i
                    // update enderchest & data
                    sprintf "function qs:incr_enderslot_%s" itemType
                    // update invdata & inventory
                    sprintf "scoreboard players operation $ENTITY invslot%d -= $ENTITY numMoved" i
                    sprintf "scoreboard players operation $ENTITY toSet = $ENTITY invslot%d" i
                    sprintf "function qs:inv/set_inv%d_%s" i itemType
                    // update remaining space
                    sprintf "scoreboard players operation $ENTITY space -= $ENTITY numMoved"
                    // debug
                    sprintf """tellraw @a ["moved ",{"score":{"name":"$ENTITY","objective":"numMoved"}}," items from inv slot %d to EC slot ",{"score":{"name":"$ENTITY","objective":"toFill"}}]""" i
                    |]
            yield sprintf "incr_enderslot_%s" itemType, [| // sets ES[toFill] <- ES[toFill] + numMoved
                for n = 0 to 64 do
                    yield sprintf "execute if entity $SCORE(numMoved=%d) run function qs:incr/incr_enderslot_%s_by_%d" n itemType n
                |]
            for n = 1 to 64 do
                yield sprintf "incr/incr_enderslot_%s_by_%d" itemType n, [| // sets ES[toFill] <- ES[toFill] + %d
                    for i = 0 to 26 do
                        yield sprintf "execute if entity $SCORE(toFill=%d) run scoreboard players add $ENTITY enderslot%d %d" i i n
                        yield sprintf "execute if entity $SCORE(toFill=%d) run scoreboard players operation $ENTITY toSet = $ENTITY enderslot%d" i i
                        yield sprintf "execute if entity $SCORE(toFill=%d) run function qs:end/set_end%d_%s" i i itemType
                    |]
            for i = 9 to 35 do
                yield sprintf "inv/set_inv%d_%s" i itemType, [| // inv[%d] <- toSet
                    yield sprintf "execute if entity $SCORE(toSet=0) run replaceitem entity @p inventory.%d air 1" (i-9)
                    for n = 1 to 63 do
                        yield sprintf "execute if entity $SCORE(toSet=%d) run replaceitem entity @p inventory.%d %s %d" n (i-9) itemType n
                    |]
            for i = 0 to 26 do
                yield sprintf "end/set_end%d_%s" i itemType, [| // end[%d] <- toSet
                    for n = 1 to 64 do
                        yield sprintf "execute if entity $SCORE(toSet=%d) run replaceitem entity @p enderchest.%d %s %d" n i itemType n
                    |]
        yield "quick_stack", [|
            yield "function qs:compute_ender_counts"
            yield "function qs:compute_inv_counts"
            for itemType in STACKABLES do
                yield sprintf "function qs:check_%s" itemType
            |]
    |]

let main() =
    printfn "stackables into 64s: %d" STACKABLES.Length  
    for s in STACKABLES do
        printfn "%s" s
    // issue: for /replaceitem we need every slot * every stack size * every item, which is 27 * 64 * ~500 ~= 864000 which is probably too many commands...
    // and there's no way to encode/decode item type as scores, since one line of code (/replaceitem) needs all 3 bits explicitly specified (no variables)
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "TestQuickStack")
    Utilities.writeDatapackMeta(world,"qspack","quick stack")
    printfn "%d functions" quickstack_functions.Length 
    for name,code in quickstack_functions do
        Utilities.writeFunctionToDisk(world,"qspack","qs",name,code |> Array.map MC_Constants.compile)


(*
TODO

'replenish' from a container
Container to inv
Goes to hotbar or to inv
Tops off stacks, but does not start new stacks
Always leaves 1 in each stack of container
move e.g. enderchest to bottom-right inventory to activate (as opposed to top-rick to quick-stack)

both quick-stack and replenish could be done terraria-AOE style to nearby chests 5x5x4 around player
could have particles between player and chest to help viz
*)