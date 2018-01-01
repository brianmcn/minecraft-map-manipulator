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
        yield "scoreboard objectives add toFill dummy"
        yield "scoreboard objectives add space dummy"
        yield "scoreboard objectives add numMoved dummy"
        yield "scoreboard objectives add toSet dummy"
        |]
    for name,nbt,range in ["ender","EnderItems", [0..26]; "inv","Inventory",[9..35]] do
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
        yield sprintf "debug_%s" name, [|
            for i in range do
                yield sprintf """tellraw @a ["slot %d has ",{"score":{"name":"$ENTITY","objective":"%sslot%d"}}," items"]""" i name i
            |]
(*
is there any cobblestone? where is the next slot to store cobblestone, and how much fits?
set has 0
for i = 0 to 26 do
    set space 0
    if {Slot:%db,id:"minecraft:cobblestone"} then
        has <- 1
        if enderslot%d < 64 then
            space <- 64 - enderslot%d
    if has && enderslot%d == 0 then
        space <- 64
    if space > 0 then
        toFill <- %d
        try_to_fill slot %d with <space> more cobble

*)
        yield "move_cobblestone", [|  // given a <toFill> (EC slot #) and a <space> (max count that can fit there yet, 64-current), finds some cobble in non-hotbar inv to move to EC slot ToFill
            for i = 9 to 35 do
                yield sprintf """execute if entity $SCORE(space=1..) if entity @p[nbt={Inventory:[{Slot:%db,id:"minecraft:cobblestone"}]}] run function qs:mv/move_cobblestone_from_%d""" i i
            |]
        for i = 9 to 35 do
            yield sprintf "mv/move_cobblestone_from_%d" i, [|
                sprintf "say mcf%d" i
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
                // debug
                yield sprintf """tellraw @a ["set_end%d, toSet=",{"score":{"name":"$ENTITY","objective":"toSet"}}]""" i
                for n = 1 to 64 do
                    yield sprintf "execute if entity $SCORE(toSet=%d) run replaceitem entity @p enderchest.%d cobblestone %d" n i n
                |]
    |]

let main() =
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "TestSize")
    Utilities.writeDatapackMeta(world,"qspack","quick stack")
    for name,code in quickstack_functions do
        Utilities.writeFunctionToDisk(world,"qspack","qs",name,code |> Array.map MC_Constants.compile)


