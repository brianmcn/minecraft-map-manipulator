module MinecraftBINGO

(*
think about skylinerw's "else feature"
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
can have arrays using e.g. array of structure blocks in the world and poking their PosX nbt data or something.


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



cut the tutorial for good
ignore lockout, custom modes, and item chests in initial version
bug: https://www.reddit.com/r/minecraftbingo/comments/74sd7m/broken_seed_spawn_in_a_waterfall_and_die_in_a_wall/
bugs & ideas from top of old file



feature ideas:
 - beacon at spawn
 - randomly put people on 1/2/3/4 teams
 - 'blind' covered play
 - use achievement toasts rather than chat for got-item notifications?
 - arrow/activator-/detector-rail
 - enable-able datapacks mean e.g. alternate loot tables could be turned on, etc

architecture

helper functions
 x PRNG
 x make new card (clone art, setup checker command blocks)
 - finalize prior game (clear inv, feed/heal, tp all to lobby, ...)
 - make new seeded card
 x make new random card
 - ensure card updated (player holding map at spawn)
 - begin a game (lots of logic here...)
 x check for bingo (5-in-a-row logic)
 - team-got-an-item (announce, add score, check for win/lockout)
 - various 'win' announcements/fireworks/scoreboard
 - worldborder timekeeper logic (compute actual seconds)
 - find spawn point based on seed (maybe different logic/implementation from now? yes, binary search a list of larger choices...)
 - compute lockout goal

blocks
 x art assets
 - ?lobby? (or code that write it?)

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
let writeFunctionToDisk(name,code) =
    //let DIR = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\BingoFor1x13\data\functions\test\"""
    let DIR = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\BingoFor1x13\datapacks\BingoPack\data\"""+NS+"""\functions"""
    let FIL = System.IO.Path.Combine(DIR,sprintf "%s.mcfunction" name)
    System.IO.File.WriteAllLines(FIL, code)



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
    yield "kill $ENTITY"
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
        "scoreboard players operation $ENTITY PRNG_OUT = K Calc"
        "scoreboard players operation $ENTITY PRNG_OUT %= $ENTITY PRNG_MOD"
        "scoreboard players operation $ENTITY PRNG_OUT += $ENTITY PRNG_MOD" // ensure non-negative
        "scoreboard players operation $ENTITY PRNG_OUT %= $ENTITY PRNG_MOD"
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
            [|  "gunpowder"        ; "firework_rocket"  ; "firework_rocket"     |]
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
        cmds.Add(sprintf "setblock %d 2 %d minecraft:structure_block" x z)
        cmds.Add(sprintf """data merge block %d 2 %d {metadata:"",mirror:"NONE",ignoreEntities:1b,mode:"SAVE",rotation:"NONE",posX:0,posY:-2,posZ:0,sizeX:17,sizeY:2,sizeZ:17,integrity:1.0f,showair:1b,powered:0b,seed:0L,author:"Lorgon111",name:"%s",id:"minecraft:structure_block",showboundingbox:1b}""" x z name)
        x <- x + 18
        i <- i + 1
        if i%8=0 then
            x <- 3
            z <- z + 18
    //writeFunctionsToResourcePack("testing",[|"autobingo",cmds.ToArray()|])
    // saves to C:\Users\Admin1\AppData\Roaming\.minecraft\saves\BingoFor1x13\generated\minecraft\structures
    writeFunctionToDisk("make_saving_structure_blocks",cmds.ToArray())
    ()

///////////////////////////////////////////////////////////////////////////////

let writeInventoryChangedHandler() =
    let advancementText = """
{
    "criteria": {
        "ic": {
            "trigger": "minecraft:inventory_changed"
        }
    },
    "requirements": [
        ["ic"]
    ],
    "rewards": {
        "function": "test:inventory_changed"
    }
}"""
    System.IO.File.WriteAllText("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\BingoFor1x13\datapacks\BingoPack\data\test\advancements\on_inventory_changed.json""",advancementText)
    let functionText = """
say INVENTORY TODO
function test:red_inventory_changed
advancement revoke @s only test:on_inventory_changed"""
    System.IO.File.WriteAllText("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\BingoFor1x13\datapacks\BingoPack\data\test\functions\inventory_changed.mcfunction""",functionText)

///////////////////////////////////////////////////////////////////////////////

let TEAMS = [| "red"; "blue"; "green"; "yellow" |]
let SQUARES = [| for i = 1 to 5 do for j = 1 to 5 do yield sprintf "%d%d" i j |]
// TODO init code to set up teams
// TODO init code to reset all scores
let checker_objectives = [|
        for s in SQUARES do
            yield sprintf "square%s" s           // square11 contains the index number of the flatBingoItems[] of the item in the top-left square of this card
            for t in TEAMS do
                yield sprintf "%sCanGet%s" t s   // redCanGet11 is 1 or 0 depending on whether the top-left square is an item it could score in the future (not yet gotten or locked out)
        yield "gotAnItem"                        // gotAnItem is 1 or 0 depending on if a gettable item was found and removed during the scoring check for a certain square this tick
        yield "gotItems"                         // gotItems is 1 or 0 depending on if one or more gettable items was found and removed during the scoring check for a certain player this tick
        for t in TEAMS do
            for i = 1 to 5 do
                yield sprintf "%sWinRow%d" t i   // how many items in the row the team has
                yield sprintf "%sWinCol%d" t i   // how many items in the col the team has
            yield sprintf "%sWinSlash" t         // how many items in the bottom left to top right diagonal the team has
            yield sprintf "%sWinBackslash" t     // how many items in the top left to bottom right diagonal the team has
            yield sprintf "%sScore" t            // how many total items the team has
            yield sprintf "%sGotBingo" t         // 1 if they already got a bingo (don't repeatedly announce)
        yield "lockoutGoal"                      // how many needed to win lockout (if this is a lockout game)
        yield "TEMP"
    |]
let checker_functions = [|
    yield "checker_init", [|
        for o in checker_objectives do
            yield sprintf "scoreboard objectives add %s dummy" o
        for s in SQUARES do
            for t in TEAMS do
                yield sprintf "scoreboard players set $ENTITY %sCanGet%s 1" t s
        for t in TEAMS do
            for i = 1 to 5 do
                yield sprintf "scoreboard players set $ENTITY %sWinRow%d 0" t i
                yield sprintf "scoreboard players set $ENTITY %sWinCol%d 0" t i
            yield sprintf "scoreboard players set $ENTITY %sWinSlash 0" t      
            yield sprintf "scoreboard players set $ENTITY %sWinBackslash 0" t  
            yield sprintf "scoreboard players set $ENTITY %sScore 0" t         
            yield sprintf "scoreboard players set $ENTITY %sGotBingo 0" t         
        yield sprintf "scoreboard players set $ENTITY lockoutGoal 25"
        |]
    for t in TEAMS do
        yield sprintf "%s_inventory_changed" t, [|        // called when player @s's inventory changed and he is on team t
            yield sprintf "scoreboard players set $ENTITY gotItems 0"
            for s in SQUARES do
                yield sprintf "scoreboard players set $ENTITY gotAnItem 0"
                yield sprintf "execute if entity $SCORE(%sCanGet%s=1) run function %s:check%s" t s NS s
                yield sprintf "execute if entity $SCORE(gotAnItem=1) run function %s:%s_got_square_%s" NS t s
            yield sprintf """execute if entity $SCORE(gotItems=1) run tellraw @a [{"selector":"@s]"}," got an item! (",{"score":{"name":"@s","objective":"Score"}}," in ",{"score":{"name":"Time","objective":"Score"}},"s)"]"""
            (* TODO
            yield U "scoreboard players test hasAnyoneUpdatedMap S 0 0"
            yield C """tellraw @a ["To update the BINGO map, drop one copy on the ground"]"""
            *)
            yield sprintf "execute if entity $SCORE(gotItems=1) as @a at @s run playsound entity.firework.launch ambient @s ~ ~ ~"
            yield sprintf "execute if entity $SCORE(gotItems=1) run function %s:%s_check_for_win" NS t
            |]
        for s in SQUARES do
            yield sprintf "%s_got_square_%s" t s, [|       // called when player @s got square s and he is on team t
                yield sprintf "say got square %s" s
                yield sprintf "scoreboard players set $ENTITY gotItems 1"
                yield sprintf "scoreboard players add $ENTITY %sScore 1" t
                yield sprintf "scoreboard players operation @a[team=%s] Score = $ENTITY %sScore" t t
                yield sprintf "scoreboard players set $ENTITY %sCanGet%s 0" t s
                for ot in TEAMS do
                    if ot <> t then
                        yield sprintf "execute if entity $SCORE(isLockout=1) run scoreboard players set $ENTITY %sCanGet%s 0" ot s
                // TODO actual logic to color the game board square appropriately
                let x = 4 + 24*(int s.[0] - int '0' - 1)
                let y = 30
                let z = 0 + 24*(int s.[1] - int '0' - 1)
                yield sprintf "fill %d %d %d %d %d %d red_terracotta replace clay" x y z (x+22) y (z+22)
                // update win conditions (add to team score of row/col/diag)
                yield sprintf "scoreboard players add $ENTITY %sWinRow%c 1" t s.[1]
                yield sprintf "scoreboard players add $ENTITY %sWinCol%c 1" t s.[0]
                if s.[0] = s.[1] then
                    yield sprintf "scoreboard players add $ENTITY %sWinBackslash 1" t
                if (int s.[0] - int '0') = 6 - (int s.[1] - int '0') then
                    yield sprintf "scoreboard players add $ENTITY %sWinSlash 1" t
                |]
        yield sprintf "%s_check_for_win" t, [|
            // check for bingo
            yield sprintf "scoreboard players set $ENTITY TEMP 0"
            for i = 1 to 5 do
                yield sprintf "execute if entity $SCORE(%sWinRow%d=5) run scoreboard players set $ENTITY TEMP 1" t i
            for i = 1 to 5 do
                yield sprintf "execute if entity $SCORE(%sWinCol%d=5) run scoreboard players set $ENTITY TEMP 1" t i
            yield sprintf "execute if entity $SCORE(%sWinSlash=5) run scoreboard players set $ENTITY TEMP 1" t
            yield sprintf "execute if entity $SCORE(%sWinBackslash=5) run scoreboard players set $ENTITY TEMP 1" t
            yield sprintf """execute if entity $SCORE(TEMP=1,%sGotBingo=0) run tellraw @a [{"selector":"@a[team=%s]"}," got BINGO!"]""" t t
            yield sprintf "execute if entity $SCORE(TEMP=1,%sGotBingo=0) run function %s:got_a_win_common_logic" t NS
            yield sprintf "execute if entity $SCORE(TEMP=1,%sGotBingo=0) run scoreboard players set $ENTITY %sGotBingo 1" t t
            // check for blackout
            yield sprintf """execute if entity $SCORE(%sScore=25) run tellraw @a [{"selector":"@a[team=%s]"}," got MEGA-BINGO!"]""" t t
            yield sprintf "execute if entity $SCORE(%sScore=25) run function %s:got_a_win_common_logic" t NS
            // check for lockout
            yield sprintf "scoreboard players operation $ENTITY TEMP = $ENTITY lockoutGoal"
            yield sprintf "scoreboard players operation $ENTITY TEMP -= $ENTITY %sScore" t
            yield sprintf """execute if entity $SCORE(isLockout=1,TEMP=0) run tellraw @a [{"selector":"@a[team=%s]"}," got the lockout goal!"]""" t
            yield sprintf "execute if entity $SCORE(isLockout=1,TEMP=0) run function %s:got_a_win_common_logic" NS
            |]
    yield "got_a_win_common_logic", [|
        (* TODO
        // put time on scoreboard
        yield "scoreboard players operation Minutes S = Time Score"
        yield "scoreboard players operation Minutes S /= Sixty Calc"
        yield "scoreboard players operation Seconds S = Time Score"
        yield "scoreboard players operation Seconds S %= Sixty Calc"
        yield "scoreboard players set Minutes Score 0"
        yield "scoreboard players set Seconds Score 0"
        yield "scoreboard players operation Minutes Score -= Minutes S"
        yield "scoreboard players operation Seconds Score -= Seconds S"
        *)
        // option to return to lobby
        yield """tellraw @a ["You can keep playing, or"]"""
        yield """tellraw @a [{"underlined":"true","text":"press 't' (chat), then click this line to return to the lobby","clickEvent":{"action":"run_command","value":"/trigger home set 1"}}]"""
        // fireworks
        yield """execute as @a at @s run summon fireworks_rocket ~3 ~0 ~0 {LifeTime:20}"""
        yield "$NTICKSLATER(8)"    // TODO note that this could be a re-entrant callback, e.g. you get bingo, and two ticks later you get the lockout goal, and so there are two schedulings of the same callback active
        yield """execute as @a at @s run summon fireworks_rocket ~0 ~0 ~3 {LifeTime:20}"""
        yield "$NTICKSLATER(8)"
        yield """execute as @a at @s run summon fireworks_rocket ~-3 ~0 ~0 {LifeTime:20}"""
        yield "$NTICKSLATER(8)"
        yield """execute as @a at @s run summon fireworks_rocket ~0 ~0 ~-3 {LifeTime:20}"""
        yield "$NTICKSLATER(8)"
        yield """execute as @a at @s run playsound entity.firework.blast ambient @s ~ ~ ~"""
        yield "$NTICKSLATER(8)"
        yield """execute as @a at @s run playsound entity.firework.twinkle ambient @s ~ ~ ~"""

    |]
    for s in SQUARES do
        yield sprintf "check%s" s, [|
            for i=0 to flatBingoItems.Length-1 do
                // TODO more efficient binary search?
                yield sprintf "execute if entity $SCORE(square%s=%d) store success score $ENTITY gotAnItem run clear @s %s 1" s i flatBingoItems.[i]
                // TODO remove this
                yield sprintf """execute if entity $SCORE(square%s=%d,gotAnItem=1) run tellraw @a [{"selector":"@s"}," got %s"]""" s i flatBingoItems.[i]
        |]
    |]

///////////////////////////////////////////////////////////////////////////////

let makeItemChests() =
    // prepare item display chests
    let anyDifficultyItems = ResizeArray()
    let otherItems = ResizeArray()
    for i = 0 to bingoItems.Length-1 do
        if bingoItems.[i].[0] = bingoItems.[i].[1] && bingoItems.[i].[0] = bingoItems.[i].[2] then
            anyDifficultyItems.Add( bingoItems.[i].[0] )
        else
            otherItems.Add( bingoItems.[i] )
    let anyDifficultyChest = 
        let sb = new System.Text.StringBuilder("""{CustomName:"Items at any difficulty",Items:[""")
        for i = 0 to anyDifficultyItems.Count-1 do
            let item = anyDifficultyItems.[i]
            sb.Append(sprintf """{Slot:%db,id:"%s",Count:%db},""" i item 1 ) |> ignore
        let s = sb.ToString()
        s.Substring(0, s.Length-1) + "]}"
    let otherChest1 =
        let sb = new System.Text.StringBuilder("""{CustomName:"Easy/Medium/Hard in row 1/2/3",Items:[""")
        for i = 0 to 8 do
            for j = 0 to 2 do
                let item = otherItems.[i].[j]
                sb.Append(sprintf """{Slot:%db,id:"%s",Count:%db},""" (i+(9*j)) item 1 ) |> ignore
        let s = sb.ToString()
        s.Substring(0, s.Length-1) + "]}"
    let otherChest2 =
        let sb = new System.Text.StringBuilder("""{CustomName:"Easy/Medium/Hard in row 1/2/3",Items:[""")
        for i = 9 to 17 do
            for j = 0 to 2 do
                let item = otherItems.[i].[j]
                sb.Append(sprintf """{Slot:%db,id:"%s",Count:%db},""" (i-9+(9*j)) item 1 ) |> ignore
        let s = sb.ToString()
        s.Substring(0, s.Length-1) + "]}"
    let otherChest3 =
        let sb = new System.Text.StringBuilder("""{CustomName:"Easy/Medium/Hard in row 1/2/3",Items:[""")
        for i = 18 to otherItems.Count-1 do
            for j = 0 to 2 do
                let item = otherItems.[i].[j]
                sb.Append(sprintf """{Slot:%db,id:"%s",Count:%db},""" (i-18+(9*j)) item 1 ) |> ignore
        let s = sb.ToString()
        s.Substring(0, s.Length-1) + "]}"
    "make_item_chests",[|
        let x = 60
        let y = 50
        let z = 100
        yield sprintf "setblock %d %d %d chest" x y z
        yield sprintf "data merge block %d %d %d %s" x y z anyDifficultyChest
        let x = x + 2
        yield sprintf "setblock %d %d %d chest" x y z
        yield sprintf "data merge block %d %d %d %s" x y z otherChest1
        let x = x + 2
        yield sprintf "setblock %d %d %d chest" x y z
        yield sprintf "data merge block %d %d %d %s" x y z otherChest2
        let x = x + 2
        yield sprintf "setblock %d %d %d chest" x y z
        yield sprintf "data merge block %d %d %d %s" x y z otherChest3
    |]

///////////////////////////////////////////////////////////////////////////////

let cardgen_objectives = [|
    yield "CARDGENTEMP"
    yield "squaresPlaced"
    for i = 0 to bingoItems.Length-1 do
        yield sprintf "bin%02d" i
    |]
let cardgen_functions = [|
    yield "cardgen_init", [|
        for o in cardgen_objectives do
            yield sprintf "scoreboard objectives add %s dummy" o
        for i = 0 to bingoItems.Length-1 do
            yield sprintf "scoreboard players set $ENTITY bin%02d 0" i
        // TODO put this somewhere better, fix
        yield sprintf "fill 4 30 0 131 30 128 clay"
        yield sprintf "fill 4 31 0 131 31 128 air"
        |]
    yield "cardgen_choose1", [|
        yield sprintf "scoreboard players set $ENTITY PRNG_MOD 28"
        yield sprintf "function %s:prng" NS
        yield sprintf "scoreboard players operation $ENTITY CARDGENTEMP = $ENTITY PRNG_OUT"
        // ensure exactly one call
        yield sprintf "scoreboard players set $ENTITY CALL 1"
        for i = 0 to bingoItems.Length-1 do
            yield sprintf "execute if entity $SCORE(CARDGENTEMP=%d,CALL=1) run function %s:cardgen_bin%02d" i NS i  // TODO binary dispatch?
    |]
    for i = 0 to bingoItems.Length-1 do
        yield sprintf "cardgen_bin%02d" i, [|
            sprintf "scoreboard players set $ENTITY CALL 0" // every exclusive-callable func needs this as first line of code
            sprintf "execute if entity $SCORE(bin%02d=1) run function %s:cardgen_choose1" i NS
            sprintf "execute unless entity $SCORE(bin%02d=1) run function %s:cardgen_binbody%02d" i NS i
            |]
    for i = 0 to bingoItems.Length-1 do
        yield sprintf "cardgen_binbody%02d" i, [|
            yield sprintf "scoreboard players add $ENTITY squaresPlaced 1"
            yield sprintf "scoreboard players set $ENTITY bin%02d 1" i
            yield sprintf "scoreboard players set $ENTITY PRNG_MOD 3"
            yield sprintf "function %s:prng" NS
            for j = 0 to 2 do
                yield sprintf """execute if entity $SCORE(PRNG_OUT=%d) at @e[tag=sky] run setblock ~ ~ ~ minecraft:structure_block{posX:0,posY:0,posZ:0,sizeX:17,sizeY:2,sizeZ:17,mode:"LOAD",name:"test:%s"}""" j bingoItems.[i].[j]
                yield sprintf """execute if entity $SCORE(PRNG_OUT=%d) at @e[tag=sky] run setblock ~ ~1 ~ minecraft:redstone_block""" j
                let index = flatBingoItems |> Array.findIndex(fun x -> x = bingoItems.[i].[j])
                for x = 1 to 5 do
                    for y = 1 to 5 do
                        yield sprintf "execute if entity $SCORE(PRNG_OUT=%d,squaresPlaced=%d) run scoreboard players set $ENTITY square%d%d %d" j (5*(y-1)+x) x y index
            |]
    yield "cardgen_makecard", [|
        yield sprintf """summon armor_stand 7 30 3 {Tags:["sky"],NoGravity:1}"""
        yield sprintf "scoreboard players set $ENTITY squaresPlaced 0"
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
let cardgen_compile() =
    let r = [|
        for name,code in cardgen_functions do
            yield! compile(code, name)
        for name,code in checker_functions do
            yield! compile(code, name)
        yield "theloop",gameLoopContinuationCheck()
        yield! compile(prng, "prng")
        yield! compile(prng_init(), "prng_init")
        yield makeItemChests()
        yield "init",[|
            "kill @e[type=!player]"
            "clear @a"
            "give @a minecraft:filled_map"
            sprintf "function %s:make_item_chests" NS
            sprintf"function %s:prng_init"NS
            sprintf"function %s:checker_init"NS
            sprintf"function %s:cardgen_init"NS
            sprintf"function %s:cardgen_makecard"NS
            |]
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

let choose_spawn_point_functions() = [|
    // prng -> x 4000, -2000, x10000
    // prng -> z 4000, -2000, x10000
    // y 130
    // tp AS & player there (probably need to wait, currently 100 ticks)
    // have AS function do like
    //     execute as AS at @s if block ~ ~ ~ air run teleport ~ ~-1 ~
    //     execute as AS at @s if block ~ ~ ~ air run function recurse
    // then    
    //     execute as AS at @s run setblock ~ ~ ~ obsidian
    //     execute as @p at AS run spawnpoint ~ ~1 ~
    //     // don't kill AS yet
    // build skybox, put players there
    // wait another 400 ticks for terrain or whatnot
    // tp players to AS ~ ~1 ~
    |]


