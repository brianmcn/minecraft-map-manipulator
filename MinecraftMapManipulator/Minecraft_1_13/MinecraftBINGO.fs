module MinecraftBINGO

(*

this about skylinerw's "else feature"
before a 'switch', set a global flag, condition each branch on the flag, and first instruction of each called branch function unsets the flag
it's a transaction, yes? safe?
annoying that caller and callee have to coordinate, but seems simple and workable?



magic mirror - save coords to scoreboard, then e.g.

summon pig ~ ~1 ~
tag @e[type=pig,limit=1] add foo
execute as @e[tag=foo] run teleport @s 100000 90 100000   // actually use execute-store to faux-tp the pig based on scores
execute as @p at @e[tag=foo] run teleport @s ~ ~ ~

works (chunk is temp loaded or whatever)









https://minecraft.gamepedia.com/1.13/Flattening

https://www.reddit.com/user/Dinnerbone/comments/6l6e3d/a_completely_incomplete_super_early_preview_of/

can we get arrays now by reading/writing? no because paths (e.g. Pos[2]) are still hardcoded...




can pose armor stand angles dynamically now, e.g. 3 player axes control 3 pose angles could be fun...
    // first manually merge in some pose data [0f,0f,0f]
    execute store result entity @e[type=armor_stand,limit=1] Pose.LeftArm[0] float 8.0 run data get entity @p Pos[0] 1.0



Things to test in snapshot

    TODO verify "at @e" will loop, that is, perform the chained command for each entity
    same for 'as'
    but not for 'if/unless'

    TODO "Damage:0s" is maybe no longer the nbt of map0? check it out

    item ids for e.g. dyes?

    understand data packs, how to turn off e.g. vanilla advancements/crafting


almost floating point math...
/execute store result entity @e[type=item,limit=1] Pos[2] double 0.001 run data get entity @e[type=item,limit=1] Pos[0] 1000.0
/data get entity @e[type=item,limit=1]



check-for-items code should still be in-game command blocks like now


cut the tutorial for good
ignore lockout, custom modes, and item chests in initial version
bug: https://www.reddit.com/r/minecraftbingo/comments/74sd7m/broken_seed_spawn_in_a_waterfall_and_die_in_a_wall/
bugs & ideas from top of old file

art assets should now be saved as structures (could find way to write my old data, but no way to read new arts without binary reader)
cardgen is an easy module to work on first?


feature ideas:
 - beacon at spawn
 - randomly put people on 1/2/3/4 teams
 - 'blind' covered play
 - use achievement toasts rather than chat for got-item notifications?
 - arrow/activator-/detector-rail
 - enable-able datapacks mean e.g. alternate loot tables could be turned on, etc

architecture

helper functions
 - PRNG
 - make new card (clone art, setup checker command blocks)
 - finalize prior game (clear inv, feed/heal, tp all to lobby, ...)
 - make new seeded card
 - make new random card
 - ensure card updated (player holding map at spawn)
 - begin a game (lots of logic here...)
 - check for bingo (5-in-a-row logic)
 - team-got-an-item (announce, add score, check for win/lockout)
 - various 'win' announcements/fireworks/scoreboard
 - worldborder timekeeper logic (compute actual seconds)
 - find spawn point based on seed (maybe different logic/implementation from now? yes, binary search a list of larger choices...)
 - compute lockout goal

blocks
 - art assets
 - ?lobby? (or code that write it?)
 - ?commands-per-item? (testfor/clear, and also clone-art-to-location)
 - fixed commands-per-item (add score, color card, lockout, ...)

ongoing per-tick code
 - updating game time when game in progress (seconds on scoreboard, MM:SS on statusbar)
 - check for players who drop map when game in progress (kill map, tag player, invoke TP sequence)
 - check for players with no more maps to give more
 - check for anyone with a trigger home score (to tp back to lobby)
 - check for on-respawn when game in progress (test for live player with death count, run on-respawn code, reset deaths)
 - check for 25-mins passed when game in progress

setup
 - gamerules
 - scoreboard objectives created
 - constants initialized
 - ?build lobby?
 - any permanent entities
*)


let NS = "test"
let allCallbackFunctions = ResizeArray()  // TODO for now, the name is both .mcfunction name and scoreboard objective name
let continuationNum = ref 1
let newName() = 
    let r = sprintf "cont%d" !continuationNum
    incr continuationNum
    r
let gameLoopContinuationCheck() =
    [|
#if DEBUG
//        yield "say ---calling gameLoop---"
#endif    
        // first decr all cont counts (after, 0=unscheduled, 1=now, 2...=future)
        for f in allCallbackFunctions do
            yield sprintf "scoreboard players remove @e[tag=callbackAS,scores={%s=1..}] %s 1" f f
        // then call all that need to go now
        for f in allCallbackFunctions do
            yield sprintf "execute if entity @e[tag=callbackAS,scores={%s=1}] run function %s:%s" f NS f
    |]
let compile(f,name) =
    let rec replaceScores(s:string) = 
        let i = s.IndexOf("$SCORE(")
        if i <> -1 then
            let j = s.IndexOf(')',i)
            let info = s.Substring(i+7,j-i-7)
            let s = s.Remove(i,j-i+1)
            let s = s.Insert(i,sprintf "@e[tag=scoreAS,scores={%s}]" info)
            replaceScores(s)
        else
            s
    let replaceContinue(s:string) = 
        let i = s.IndexOf("$CONTINUEASAT(")
        if i <> -1 then
            if i <> 0 then failwith "$CONTINUEASAT must be only thing on the line"
            let j = s.IndexOf(')',i)
            if j <> s.Length-1 then failwith "$CONTINUEASAT must be only thing on the line"
            let info = s.Substring(i+14,j-i-14)
            // $CONTINUEASAT(entity) will
            //  - create a new named .mcfunction for the continuation
            //  - execute as entity at @s run function the new function
            let nn = newName()
            [|sprintf "execute as %s at @s run function %s" info nn|], nn
        else
            let i = s.IndexOf("$NTICKSLATER(")
            if i <> -1 then
                if i <> 0 then failwith "$NTICKSLATER must be only thing on the line"
                let j = s.IndexOf(')',i)
                if j <> s.Length-1 then failwith "$NTICKSLATER must be only thing on the line"
                let info = s.Substring(i+13,j-i-13)
                // $NTICKSLATER(n) will
                //  - create a new named .mcfunction for the continuation
                //  - create a new scoreboard objective for it
                //  - set the value of e.g. @e[tag=callbackAS] in the new objective to 'n'
                //     - but first check the existing score was 0; this system can't register the same callback function more than once at a time, that would be an error (no re-entrancy)
                //  - add a hook in the gameloop that, foreach callback function in the global registry, will check the score, and
                //     - if the score is ..0, do nothing (unscheduled)
                //     - if the score is 1, call the corresponding callback function (time to continue now)
                //     - else subtract 1 from the score (get 1 tick closer to calling it)
                let nn = newName()
                allCallbackFunctions.Add(nn)
                [|
                    sprintf """execute if entity @e[tag=callbackAS,scores={%s=2..}] run tellraw @a ["error, re-entrant callback %s"]""" nn nn
                    sprintf "scoreboard players set @e[tag=callbackAS] %s %d" nn (int info + 1) // +1 because we decr at start of gameloop
                |], nn
            else
                [|s|], null
    let a = f |> Seq.toArray 
    // $SCORE(...) is maybe e.g. "@e[tag=scoreAS,scores={...}]"
    let a = a |> Array.map replaceScores
    // $ENTITY is main scorekeeper entity (maybe e.g. "@e[tag=scoreAS]")
    let a = a |> Array.map (fun s -> s.Replace("$ENTITY","@e[tag=scoreAS]"))
    let r = [|
        let cur = ResizeArray()
        let curName = ref name
        let i = ref 0
        while !i < a.Length do
            let b,nn = replaceContinue(a.[!i])
            cur.AddRange(b)
            if nn<>null then
                yield !curName, cur.ToArray()
                cur.Clear()
                curName := nn
            incr i
        yield !curName, cur.ToArray()
    |]
#if DEBUG
    let r = [|
        for name,code in r do
//            yield name, [| yield sprintf """tellraw @a ["calling '%s'"]""" name; yield! code |]
            yield name, [| yield sprintf """say ---calling '%s'---""" name; yield! code |]
        |]
#endif    
    r

///////////////////////////////////////////////////////

let prng_init() = [|
    yield "scoreboard objectives add CALL dummy"  // for 'else' flow control - exclusive branch; always 0, except 1 just before a switch and 0 moment branch is taken
    yield "scoreboard objectives add PRNG_MOD dummy"
    yield "scoreboard objectives add PRNG_OUT dummy"
    yield "scoreboard objectives add Calc dummy"
    yield "scoreboard players set A Calc 1103515245"
    yield "scoreboard players set C Calc 12345"
    yield "scoreboard players set Two Calc 2"
    yield "scoreboard players set TwoToSixteen Calc 65536"
    yield "kill @e[tag=scoreAS]"
    yield "summon armor_stand 1 1 1 {Tags:[\"scoreAS\"],NoGravity:1,Marker:1}"    
    // TODO move this to right spot
    yield "kill @e[tag=callbackAS]"
    yield "summon armor_stand 1 1 1 {Tags:[\"callbackAS\"],NoGravity:1,Marker:1}"    
    for cbn in allCallbackFunctions do
        yield sprintf "scoreboard objectives add %s dummy" cbn
    |]
let prng = [|
        // compute next Z value with PRNG
        "scoreboard players operation Z Calc *= A Calc"
        "scoreboard players operation Z Calc += C Calc"
        "scoreboard players operation Z Calc *= Two Calc"  // mod 2^31
        "scoreboard players operation Z Calc /= Two Calc"
        "scoreboard players operation K Calc = Z Calc"
        "scoreboard players operation K Calc *= Two Calc"
        "scoreboard players operation K Calc /= Two Calc"
        "scoreboard players operation K Calc /= TwoToSixteen Calc"   // upper 16 bits most random
        // get a number in the desired range
        "scoreboard players operation @e[tag=scoreAS] PRNG_OUT = K Calc"
        "scoreboard players operation @e[tag=scoreAS] PRNG_OUT %= @e[tag=scoreAS] PRNG_MOD"
        "scoreboard players operation @e[tag=scoreAS] PRNG_OUT += @e[tag=scoreAS] PRNG_MOD" // ensure non-negative
        "scoreboard players operation @e[tag=scoreAS] PRNG_OUT %= @e[tag=scoreAS] PRNG_MOD"
    |]

///////////////////////////////////////////////////////







//TODO "Damage:0s" is maybe no longer the nbt of map0? check it out
let find_player_who_dropped_map =
    [|
    "scoreboard players set $ENTITY SomeoneIsMapUpdating 0"
    "execute as @a[tag=playerThatIsMapUpdating] run scoreboard players set $ENTITY SomeoneIsMapUpdating 1"
    // if someone already updating, kill all droppedMap entities
    "execute if entity $SCORE(SomeoneIsMapUpdating=1) run kill @e[type=Item,nbt={Item:{id:\"minecraft:filled_map\",Damage:0s}}]"
    // if no one updating yet, do the main work
    "execute if entity $SCORE(SomeoneIsMapUpdating=0) run function TODO:find_player_who_dropped_map_core"
    |]
let find_player_who_dropped_map_core =
    [|
    // tag all players near dropped maps as wanting to tp
    "execute at @e[type=Item,nbt={Item:{id:\"minecraft:filled_map\",Damage:0s}}] run scoreboard players tag @a[r=5] add playerThatWantsToUpdate"
    //TODO verify "at @e" will loop, that is, perform the chained command for each entity
    // choose a random one to be the tp'er
    "scoreboard players tag @r[tag=playerThatWantsToUpdate] add playerThatIsMapUpdating"
    // clear the 'wanting' flags
    "scoreboard players tag @a[tag=playerThatWantsToUpdate] remove playerThatWantsToUpdate"
    // kill all droppedMap entities
    "kill @e[type=Item,nbt={Item:{id:\"minecraft:filled_map\",Damage:0s}}]"
    // start the TP sequence for the chosen guy
    "execute as @p[tag=playerThatIsMapUpdating] at @s run function player_updates_map"
    |]
// TODO
let MAP_UPDATE_ROOM = "62 10 72"
let player_updates_map =  // called as and at that player
    [|
    "summon area_effect_cloud ~ ~ ~ {Tags:[\"whereToTpBackTo\"],Duration:1000}"  // summon now, need to wait a tick to TP
    "$NTICKSLATER(1)"
    "$CONTINUEASAT(@p[tag=playerThatIsMapUpdating])"
    "setworldspawn ~ ~ ~"
    """tellraw @a [{"selector":"@p[tag=playerThatIsMapUpdating]"}," is updating the BINGO map"]"""
    "entitydata @e[type=!Player,r=62] {PersistenceRequired:1}"  // preserve mobs
    "tp @e[tag=whereToTpBackTo] @p[tag=playerThatIsMapUpdating]"  // a tick after summoning, tp marker to player, to preserve facing direction
    sprintf "tp @s %s 180 0" MAP_UPDATE_ROOM
    "particle portal ~ ~ ~ 3 2 3 1 99 @s"
    "playsound entity.endermen.teleport ambient @a"
    "$NTICKSLATER(30)" // TODO adjust timing?
    "tp @p[tag=playerThatIsMapUpdating] @e[tag=whereToTpBackTo]"
    "$CONTINUEASAT(@p[tag=playerThatIsMapUpdating])"
    "entitydata @e[type=!Player,r=72] {PersistenceRequired:0}"  // don't leak mobs
    "particle portal ~ ~ ~ 3 2 3 1 99 @s"
    "playsound entity.endermen.teleport ambient @a"
    sprintf "setworldspawn %s" MAP_UPDATE_ROOM
    "scoreboard players tag @p[tag=playerThatIsMapUpdating] remove playerThatIsMapUpdating"
    //TODO keep this feature? "scoreboard players set hasAnyoneUpdatedMap S 1"
    "kill @e[tag=whereToTpBackTo]"
    |]
let test() = 
    //let r = compile(find_player_who_dropped_map,"find_player_who_dropped_map")
    let r = compile(player_updates_map,"player_updates_map")
    printfn "%A" r
    printfn ""
    printfn "callbacks: %A" (allCallbackFunctions.ToArray())
    printfn "gameLoopContinuationCheck: %A" (gameLoopContinuationCheck())
let ensureDirOfFile(filename) = 
    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename)) |> ignore
let writeFunctionsToResourcePack(packName, funcs) =
    let ROOT = """C:\Users\Admin1\AppData\Roaming\.minecraft\resourcepacks"""
    let FOLDER = packName
    let meta = 
            """
            {
               "pack": {
                  "pack_format": 3,
                  "description": "BINGO Data Pack"
               }
            }
            """
    let mcmetaFilename = System.IO.Path.Combine(ROOT, FOLDER, "pack.mcmeta")
    ensureDirOfFile(mcmetaFilename)
    System.IO.File.WriteAllText(mcmetaFilename, meta)
    let FUNCTIONSDIR = System.IO.Path.Combine(ROOT, FOLDER, """data\BINGO\functions""")
    for name,code in funcs do
        let fn = System.IO.Path.Combine(FUNCTIONSDIR, name+".mcfunction")
        ensureDirOfFile(fn)
        System.IO.File.WriteAllLines(fn, code)
let testWrite() = 
    //let r = compile(find_player_who_dropped_map,"find_player_who_dropped_map")
    let r = compile(player_updates_map,"player_updates_map")
    writeFunctionsToResourcePack("BINGO", r)
        
        



////////////////////////////////////////////////

let bingoItems =
        [|
            [|  "diamond"          ; "diamond_hoe"      ; "diamond_axe"         |]
            [|  "bone"             ; "gray_dye"         ; "gray_dye"            |]
            [|  "ender_pearl"      ; "ender_pearl"      ; "slime_ball"          |]
            [|  "fern"             ; "vine"             ; "dead_bush"           |]
            [|  "brick"            ; "flower_pot"       ; "flower_pot"          |]
            [|  "glass_bottle"     ; "glass_bottle"     ; "glass_bottle"        |]
            [|  "melon"            ; "melon"            ; "speckled_melon"      |]
            [|  "ink_sac"          ; "book"             ; "writable_book"       |]
            [|  "apple"            ; "golden_shovel"    ; "golden_apple"        |]
            [|  "flint"            ; "flint"            ; "flint_and_steel"     |]
            [|  "cocoa_beans"      ; "cookie"           ; "cookie"              |]
            [|  "pumpkin_seeds"    ; "pumpkin_seeds"    ; "pumpkin_pie"         |]
            [|  "rail"             ; "rail"             ; "rail"                |]
            [|  "mushroom_stew"    ; "mushroom_stew"    ; "mushroom_stew"       |]
            [|  "sugar"            ; "spider_eye"       ; "fermented_spider_eye"|]
            [|  "cactus_green"     ; "cactus_green"     ; "lime_dye"            |]
            [|  "lapis_lazuli"     ; "purple_dye"       ; "cyan_dye"            |]
            [|  "beetroot_soup"    ; "emerald"          ; "emerald"             |]
            [|  "furnace_minecart" ; "chest_minecart"   ; "tnt_minecart"        |]
            [|  "gunpowder"        ; "fireworks"        ; "fireworks"           |]
            [|  "compass"          ; "compass"          ; "map"                 |]
            [|  "spruce_sapling"   ; "spruce_sapling"   ; "acacia_sapling"      |]
            [|  "cauldron"         ; "cauldron"         ; "cauldron"            |]
            [|  "name_tag"         ; "saddle"           ; "enchanted_book"      |]
            [|  "milk_bucket"      ; "egg"              ; "cake"                |]
            [|  "cod"              ; "cod"              ; "cod"                 |]
            [|  "sign"             ; "item_frame"       ; "painting"            |]
            [|  "golden_sword"     ; "clock"            ; "powered_rail"        |]
            [|  "hopper"           ; "hopper"           ; "hopper_minecart"     |]
            [|  "repeater"         ; "repeater"         ; "repeater"            |]
        |]

let flatBingoItems = 
    let orig = [|
        for a in bingoItems do
            for x in a do
                yield x
        |]
    let trim = // remove duplicates
        let r = ResizeArray()
        for x in orig do
            if not(r.Contains(x)) then
                r.Add(x)
        r
    trim.ToArray()

let makeSavingStructureBlocks() =
    let cmds = ResizeArray()
    let mutable i = 0
    let mutable x,z = 3,3
    while i < flatBingoItems.Length do
        let name = flatBingoItems.[i]
        cmds.Add(sprintf "setblock %d 2 %d minecraft:structure_block 0" x z)
        cmds.Add(sprintf """blockdata %d 2 %d {metadata:"",mirror:"NONE",ignoreEntities:1b,mode:"SAVE",rotation:"NONE",posX:0,posY:-2,posZ:0,sizeX:17,sizeY:2,sizeZ:17,integrity:1.0f,showair:1b,powered:0b,seed:0L,author:"Lorgon111",name:"%s",id:"minecraft:structure_block",showboundingbox:1b}""" x z name)
        x <- x + 18
        i <- i + 1
        if i%8=0 then
            x <- 3
            z <- z + 18
    writeFunctionsToResourcePack("testing",[|"autobingo",cmds.ToArray()|])
    ()

///////////////////////////////////////////////////////////////////////////////

// card gen sketch
(*
have 28 temp AS numbered 0-27, sitting inside cmd blocks that function choose_bin_NN
scoreboard players set BINS X 28
prng(BINS)
scoreboard players operation @e[tag=chooser] X -= prng X
execute at @e[tag=chooser,score_X=0] run blockdata ~ ~ ~ {auto:1b}
kill @e[tag=chooser,score_X=0]
scoreboard players remove @e[tag=chooser,score_X=0..] X 1
scoreboard players operation @e[tag=chooser] X += prng X
scoreboard players remove BINS X 1

BLEAH

simpler implementation - just use scoreboard
prng(28)
run that guy (will set flag to say already run)
if it was new, decrement the countdown
repeat until done
will waste a lot of computation choosing duplicates, but _so_ much simpler to implement, and good enough

init:
    SB set bin00 0
    ...
    SB set bin27 0
choose1:
    prng(28)
    call that guy (binary dispatch)
call_bin_xx:
    execute if entity $SCORE(binxx=1) run function choose1
    execute unless $SCORE(binxx=1) run function call_bin_xx_body
call_bin_xx_body:
    SB set binxx 1
    do structure cloning
    do checker setup
*)

let cardgen_objectives = [|
    yield "CARDGENTEMP"
    for i = 0 to bingoItems.Length-1 do
        yield sprintf "bin%02d" i
    |]
let cardgen_functions = [|
    yield "cardgen_init", [|
        for o in cardgen_objectives do
            yield sprintf "scoreboard objectives add %s dummy" o
        for i = 0 to bingoItems.Length-1 do
            yield sprintf "scoreboard players set @e[tag=scoreAS] bin%02d 0" i
        |]
    yield "cardgen_choose1", [|
        yield "scoreboard players set @e[tag=scoreAS] PRNG_MOD 28"
        yield sprintf "function %s:prng" NS
        yield "scoreboard players operation @e[tag=scoreAS] CARDGENTEMP = @e[tag=scoreAS] PRNG_OUT" 
        // ensure exactly one call
        yield "scoreboard players set @e[tag=scoreAS] CALL 1" 
        for i = 0 to bingoItems.Length-1 do
            yield sprintf "execute if entity $SCORE(CARDGENTEMP=%d,CALL=1) run function %s:cardgen_bin%02d" i NS i  // TODO binary dispatch?
    |]
    for i = 0 to bingoItems.Length-1 do
        yield sprintf "cardgen_bin%02d" i, [|
            "scoreboard players set @e[tag=scoreAS] CALL 0" // every exclusive-callable func needs this as first line of code
            sprintf "execute if entity $SCORE(bin%02d=1) run function %s:cardgen_choose1" i NS
            sprintf "execute unless entity $SCORE(bin%02d=1) run function %s:cardgen_binbody%02d" i NS i
            |]
    for i = 0 to bingoItems.Length-1 do
        yield sprintf "cardgen_binbody%02d" i, [|
            yield sprintf "scoreboard players set @e[tag=scoreAS] bin%02d 1" i
            yield sprintf "scoreboard players set @e[tag=scoreAS] PRNG_MOD 3"
            yield sprintf "function %s:prng" NS
            for j = 0 to 2 do
                yield sprintf """execute if entity $SCORE(PRNG_OUT=%d) at @e[tag=sky] run setblock ~ ~ ~ minecraft:structure_block{posX:0,posY:0,posZ:0,sizeX:17,sizeY:2,sizeZ:17,mode:"LOAD",name:"test:%s"}""" j bingoItems.[i].[j]
                yield sprintf """execute if entity $SCORE(PRNG_OUT=%d) at @e[tag=sky] run setblock ~ ~1 ~ minecraft:redstone_block""" j
                yield sprintf "execute if entity $SCORE(PRNG_OUT=%d) run say TODO checker setup %s" j bingoItems.[i].[j]
            |]
    yield "cardgen_makecard", [|
        yield sprintf """summon armor_stand 7 30 3 {Tags:["sky"],NoGravity:1}"""
        yield sprintf "say TODO init AS for location first checker"
        for _x = 1 to 5 do
            yield sprintf "$NTICKSLATER(1)"
            yield sprintf "function %s:cardgen_choose1" NS
            yield sprintf "execute at @e[tag=sky] run teleport @e[tag=sky] ~24 ~ ~"
            yield sprintf "$NTICKSLATER(1)"
            yield sprintf "function %s:cardgen_choose1" NS
            yield sprintf "execute at @e[tag=sky] run teleport @e[tag=sky] ~24 ~ ~"
            yield sprintf "$NTICKSLATER(1)"
            yield sprintf "function %s:cardgen_choose1" NS
            yield sprintf "execute at @e[tag=sky] run teleport @e[tag=sky] ~24 ~ ~"
            yield sprintf "$NTICKSLATER(1)"
            yield sprintf "function %s:cardgen_choose1" NS
            yield sprintf "execute at @e[tag=sky] run teleport @e[tag=sky] ~24 ~ ~"
            yield sprintf "$NTICKSLATER(1)"
            yield sprintf "function %s:cardgen_choose1" NS
            yield sprintf "execute at @e[tag=sky] run teleport @e[tag=sky] ~-96 ~ ~24"
        yield sprintf "kill @e[tag=sky]"
        |]
    |]
let writeFunctionToDisk(name,code) =
    //let DIR = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\BingoFor1x13\data\functions\test\"""
    let DIR = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\BingoFor1x13\datapacks\BingoPack\data\test\functions"""
    let FIL = System.IO.Path.Combine(DIR,sprintf "%s.mcfunction" name)
    System.IO.File.WriteAllLines(FIL, code)
let cardgen_compile() =
    let r = [|
        for name,code in cardgen_functions do
            yield! compile(code, name)
        yield "theloop",gameLoopContinuationCheck()
        yield! compile(prng, "prng")
        yield! compile(prng_init(), "prng_init")
        yield "init",[|"kill @e[type=!player]";sprintf"function %s:prng_init"NS;sprintf"function %s:cardgen_init"NS|]
        |]
    printfn "%A" r
    for name,code in r do
        writeFunctionToDisk(name,code)

let magic_mirror_funcs = [|
    "magic1", [|
        "summon minecraft:armor_stand 10 32 10 {Rotation:[20f,20f]}" // needs some rotation to be able to store to it later
        "scoreboard objectives add X dummy"
        "scoreboard objectives add Y dummy"
        "scoreboard objectives add Z dummy"
        "scoreboard objectives add RotX dummy"
        "scoreboard objectives add RotY dummy"
        "execute store result score @e[type=armor_stand,limit=1] X run data get entity @p Pos[0] 128.0"
        "execute store result score @e[type=armor_stand,limit=1] Y run data get entity @p Pos[1] 128.0"
        "execute store result score @e[type=armor_stand,limit=1] Z run data get entity @p Pos[2] 128.0"
        "execute store result score @e[type=armor_stand,limit=1] RotX run data get entity @p Rotation[0] 8.0"
        "execute store result score @e[type=armor_stand,limit=1] RotY run data get entity @p Rotation[1] 8.0"
        "tp @p 10 31 10" 
        |]
    "magic2", [|
        "execute as @e[type=armor_stand,limit=1] store result entity @s Pos[0] double 0.0078125 run scoreboard players get @s X"
        "execute as @e[type=armor_stand,limit=1] store result entity @s Pos[1] double 0.0078125 run scoreboard players get @s Y"
        "execute as @e[type=armor_stand,limit=1] store result entity @s Pos[2] double 0.0078125 run scoreboard players get @s Z"
        "execute as @e[type=armor_stand,limit=1] store result entity @s Rotation[0] float 0.125 run scoreboard players get @s RotX"
        "execute as @e[type=armor_stand,limit=1] store result entity @s Rotation[1] float 0.125 run scoreboard players get @s RotY"
        "teleport @p @e[type=armor_stand,limit=1]"
        "kill @e[type=armor_stand]"  // TODO tag it
        |]
    |]
let magic_mirror_compile() =
    for name,code in magic_mirror_funcs do
        writeFunctionToDisk(name,code)


(*
    init sky AS location
    NTICKSLATER(1)
    inline loop 5x do
        inline loop 5x do
            //choose1
            found = -1
            loop
                r = prng(28)
                if not binsUsed[r] then   // inlined array
                    binsUsed[r] = true
                    found = r
                exitif found>=0
            done
            r = prng(3)
            //
            structure at sky AS inline-decoded with values of (found,r)
            // TODO command block checkers
            tp right
        done
        tp downleft
    done
    kill sky AS    
*)

(*
    // or if we had arrays of ints
    A = array(28)
    inline loop i = 0 to 27 do
        A[i] = i
    done
    inline loop i = 27 downto 0 do
        let r = prng(i)
        temp = A[r]
        A[r] = A[i]
        A[i] = temp
    done
    init sky AS location
    NTICKSLATER(1)
    i = 0
    inline loop 5x do
        inline loop 5x do
            found = A[i]
            r = prng(3)
            structure at sky AS inline-decoded with values of (found,r)
            // TODO command block checkers
            tp right
            i = i + 1
        done
        tp downleft
    done
*)

(*
    even in the array case, the 'inline-decode' of an int into specific code is a tedious long list of instructions
    command blocks in the world an an entity to activate them make O(1) array decoding of instructions (but at most once per tick)
    ...
    if I did do a CPS compiler, the main IP-decoder could be an entity addressing a row of command blocks? no, the one/tick limit, argh
*)

(*
there are too many implementation choices... I need a decision-making framework
for example "can use entities for 'array data', but no command blocks for array/dispatch" or something
*)