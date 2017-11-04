module MinecraftBINGO

(*

Things to test in snapshot

    TODO verify "at @e" will loop, that is, perform the chained command for each entity
    same for 'as'
    but not for 'if/unless'

    TODO "Damage:0s" is maybe no longer the nbt of map0? check it out

    item ids for e.g. dyes?

    understand data packs, how to turn off e.g. vanilla advancements/crafting



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


let allCallbackFunctions = ResizeArray()  // TODO for now, the name is both .mcfunction name and scoreboard objective name
let continuationNum = ref 1
let newName() = 
    let r = sprintf "cont%d" !continuationNum
    incr continuationNum
    r
let gameLoopContinuationCheck() =
    [|
        for f in allCallbackFunctions do
            yield sprintf "execute if @e[tag=callbackAS,score_%s=1] then function %s" f f
            yield sprintf "scoreboard players remove @e[tag=callbackAS,score_%s=1..] %s 1" f f
    |]
let compile(f,name) =
    let rec replaceScores(s:string) = 
        let i = s.IndexOf("$SCORE(")
        if i <> -1 then
            let j = s.IndexOf(')',i)
            let info = s.Substring(i+7,j-i-7)
            let s = s.Remove(i,j-i+1)
            let s = s.Insert(i,sprintf "@e[tag=scoreAS,score_%s]" info)
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
            //  - execute as entity at @s then function the new function
            let nn = newName()
            [|sprintf "execute as %s at @s then function %s" info nn|], nn
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
                    sprintf """execute if @e[tag=callbackAS,score_%s=1..] then tellraw @a ["error, re-entrant callback %s"]""" nn nn
                    sprintf "scoreboard players set @e[tag=callbackAS] %s %s" nn info
                |], nn
            else
                [|s|], null
    let a = f |> Seq.toArray 
    // $SCORE(...) is maybe e.g. "@e[tag=scoreAS,score_...]"
    let a = a |> Array.map replaceScores
    // $ENTITY is main scorekeeper entity (maybe e.g. "@e[tag=scoreAS]")
    let a = a |> Array.map (fun s -> s.Replace("$ENTITY","@e[tag=scoreAS]"))
    [|
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

//TODO "Damage:0s" is maybe no longer the nbt of map0? check it out
let find_player_who_dropped_map =
    [|
    "scoreboard players set $ENTITY SomeoneIsMapUpdating 0"
    "execute as @a[tag=playerThatIsMapUpdating] then scoreboard players set $ENTITY SomeoneIsMapUpdating 1"
    // if someone already updating, kill all droppedMap entities
    "execute if $SCORE(SomeoneIsMapUpdating=1) then kill @e[type=Item,nbt={Item:{id:\"minecraft:filled_map\",Damage:0s}}]"
    // if no one updating yet, do the main work
    "execute if $SCORE(SomeoneIsMapUpdating=0) then function TODO:find_player_who_dropped_map_core"
    |]
let find_player_who_dropped_map_core =
    [|
    // tag all players near dropped maps as wanting to tp
    "execute at @e[type=Item,nbt={Item:{id:\"minecraft:filled_map\",Damage:0s}}] then scoreboard players tag @a[r=5] add playerThatWantsToUpdate"
    //TODO verify "at @e" will loop, that is, perform the chained command for each entity
    // choose a random one to be the tp'er
    "scoreboard players tag @r[tag=playerThatWantsToUpdate] add playerThatIsMapUpdating"
    // clear the 'wanting' flags
    "scoreboard players tag @a[tag=playerThatWantsToUpdate] remove playerThatWantsToUpdate"
    // kill all droppedMap entities
    "kill @e[type=Item,nbt={Item:{id:\"minecraft:filled_map\",Damage:0s}}]"
    // start the TP sequence for the chosen guy
    "execute as @p[tag=playerThatIsMapUpdating] at @s then function player_updates_map"
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
            [|  -1, "diamond"          ,"" ; -1, "diamond_hoe"      ,"" ; -1, "diamond_axe"         ,"" |]
            [|  -1, "bone"             ,"" ; +8, "dye"              ,"gray_dye" ; +8, "dye"                 ,"gray_dye" |]
            [|  -1, "ender_pearl"      ,"" ; -1, "ender_pearl"      ,"" ; -1, "slime_ball"          ,"" |]
            [|  +2, "tallgrass"        ,"fern" ; -1, "vine"             ,"" ; -1, "deadbush"            ,"" |]
            [|  -1, "brick"            ,"" ; -1, "flower_pot"       ,"" ; -1, "flower_pot"          ,"" |]
            [|  -1, "glass_bottle"     ,"" ; -1, "glass_bottle"     ,"" ; -1, "glass_bottle"        ,"" |]
            [|  -1, "melon"            ,"" ; -1, "melon"            ,"" ; -1, "speckled_melon"      ,"" |]
            [|  +0, "dye"              ,"ink_sac" ; -1, "book"             ,"" ; -1, "writable_book"       ,"" |]
            [|  -1, "apple"            ,"" ; -1, "golden_shovel"    ,"" ; -1, "golden_apple"        ,"" |]
            [|  -1, "flint"            ,"" ; -1, "flint"            ,"" ; -1, "flint_and_steel"     ,"" |]
            [|  +3, "dye"              ,"cocoa_bean" ; -1, "cookie"           ,"" ; -1, "cookie"              ,"" |]
            [|  -1, "pumpkin_seeds"    ,"" ; -1, "pumpkin_seeds"    ,"" ; -1, "pumpkin_pie"         ,"" |]
            [|  -1, "rail"             ,"" ; -1, "rail"             ,"" ; -1, "rail"                ,"" |]
            [|  -1, "mushroom_stew"    ,"" ; -1, "mushroom_stew"    ,"" ; -1, "mushroom_stew"       ,"" |]
            [|  -1, "sugar"            ,"" ; -1, "spider_eye"       ,"" ; -1, "fermented_spider_eye","" |]
            [|  +2, "dye"              ,"cactus_green" ; +2, "dye"              ,"cactus_green" ;+10, "dye"                 ,"lime_dye" |]
            [|  +4, "dye"              ,"lapis" ; +5, "dye"              ,"purple_dye" ; +6, "dye"                 ,"cyan_dye" |]
            [|  -1, "beetroot_soup"    ,"" ; -1, "emerald"          ,"" ; -1, "emerald"             ,"" |]
            [|  -1, "furnace_minecart" ,"" ; -1, "chest_minecart"   ,"" ; -1, "tnt_minecart"        ,"" |]
            [|  -1, "gunpowder"        ,"" ; -1, "fireworks"        ,"" ; -1, "fireworks"           ,"" |]
            [|  -1, "compass"          ,"" ; -1, "compass"          ,"" ; -1, "map"                 ,"" |]
            [|  +1, "sapling"          ,"spruce_sapling" ; +1, "sapling"          ,"spruce_sapling" ; +4, "sapling"             ,"acacia_sapling" |]
            [|  -1, "cauldron"         ,"" ; -1, "cauldron"         ,"" ; -1, "cauldron"            ,"" |]
            [|  -1, "name_tag"         ,"" ; -1, "saddle"           ,"" ; -1, "enchanted_book"      ,"" |]
            [|  -1, "milk_bucket"      ,"" ; -1, "egg"              ,"" ; -1, "cake"                ,"" |]
            [|  -1, "fish"             ,"" ; -1, "fish"             ,"" ; -1, "fish"                ,"" |]
            [|  -1, "sign"             ,"" ; -1, "item_frame"       ,"" ; -1, "painting"            ,"" |]
            [|  -1, "golden_sword"     ,"" ; -1, "clock"            ,"" ; -1, "golden_rail"         ,"" |]
            [|  -1, "hopper"           ,"" ; -1, "hopper"           ,"" ; -1, "hopper_minecart"     ,"" |]
            [|  -1, "repeater"         ,"" ; -1, "repeater"         ,"" ; -1, "repeater"            ,"" |]
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
        let _dmg,itemid,desc = flatBingoItems.[i]
        let name = if desc="" then itemid else desc
        cmds.Add(sprintf "setblock %d 2 %d minecraft:structure_block 0" x z)
        cmds.Add(sprintf """blockdata %d 2 %d {metadata:"",mirror:"NONE",ignoreEntities:1b,mode:"SAVE",rotation:"NONE",posX:0,posY:-2,posZ:0,sizeX:17,sizeY:2,sizeZ:17,integrity:1.0f,showair:1b,powered:0b,seed:0L,author:"Lorgon111",name:"%s",id:"minecraft:structure_block",showboundingbox:1b}""" x z name)
        x <- x + 18
        i <- i + 1
        if i%8=0 then
            x <- 3
            z <- z + 18
    writeFunctionsToResourcePack("testing",[|"autobingo",cmds.ToArray()|])
    ()