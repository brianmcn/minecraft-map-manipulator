module MinecraftBINGO


let CHECK_EVERY_TICK = true

(*
think about skylinerw's "else feature"
before a 'switch', set a global flag, condition each branch on the flag, and first instruction of each called branch function unsets the flag
it's a transaction, yes? safe?
annoying that caller and callee have to coordinate, but seems simple and workable?


think about survival waypoints idea...
wubbi https://www.youtube.com/watch?v=WCJRTd7Otq8 has a nice one
basic idea:
 - have a room with lots of clickable signs (go to waypoint N)
 - player can add own signs for names if desired
 - spawn egg or whatnot to place new waypoint (crafting/found materials?), will have AS with particles or whatnot, will store coords in scoreboard in fixed (non-entity?) slots (WayX01, WayY01, ... WayX99...)
 - when player stays in particles for more than 3s or whatever, brings to tp room
 - could 'delete' waypoint by having e.g. WayYnn==-1 to mark as 'unused', and clickable sign to 'forget' it? hm, but how delete the entity there... i guess it could delete self after check if exist based on scores



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

    understand data packs, how to turn off e.g. vanilla advancements/crafting


cut the tutorial for good
ignore custom modes, and item chests in initial version
bug: https://www.reddit.com/r/minecraftbingo/comments/74sd7m/broken_seed_spawn_in_a_waterfall_and_die_in_a_wall/
bugs & ideas from top of old file



feature ideas:
 - beacon at spawn
 - randomly put people on 1/2/3/4 teams
 - 'blind' covered play
 - use achievement toasts rather than chat for got-item notifications?
 - arrow/activator-/detector-rail
 - enable-able datapacks mean e.g. alternate loot tables could be turned on, etc?
 - custom configs done as separate data packs? out-of-band changes/extensions?
    - on-start/on-respawn is one type of simple customization that maybe could be in datapack
    - lockout, 'blind-covered', other bingo-game mechanics updates are more baked in, hm...
 - call out 'sniper bingo' (score exactly 5)
 - 20-no-bingo?

architecture

helper functions
 x PRNG
 x make new card (clone art, setup checker command blocks)
 - finalize prior game (clear inv, feed/heal, tp all to lobby, ...)
 x make new seeded card
 x make new random card
 x ensure card updated (player holding map at spawn)
 - begin a game (lots of logic here...)
 x check for bingo (5-in-a-row logic)
 - team-got-an-item (announce, add score, check for win/lockout)
 x various 'win' announcements/fireworks/scoreboard
 x worldborder timekeeper logic (compute actual seconds)
 x find spawn point based on seed (maybe different logic/implementation from now? yes, binary search a list of larger choices...)
 x compute lockout goal

blocks
 x art assets
 - ?lobby? (or code that write it?)

ongoing per-tick code
 x updating game time when game in progress (seconds on scoreboard, MM:SS on statusbar)
 x check for players who drop map when game in progress (kill map, tag player, invoke TP sequence)
 x check for players with no more maps to give more
 x check for anyone with a trigger home score (to tp back to lobby)
 - check for on-respawn when game in progress (test for live player with death count, run on-respawn code, reset deaths)
 x check for 25-mins passed when game in progress

setup
 - gamerules
 x scoreboard objectives created
 x constants initialized
 - ?build lobby?
 x any permanent entities
*)

let NS = "test"
let writeFunctionToDisk(name,code) =
    let DIR = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\BingoFor1x13\datapacks\BingoPack\data\"""+NS+"""\functions"""
    let FIL = System.IO.Path.Combine(DIR,sprintf "%s.mcfunction" name)
    System.IO.File.WriteAllLines(FIL, code)


let entity_init() = [|
    yield "kill @e[tag=scoreAS]"
    yield "summon armor_stand 4 4 4 {Tags:[\"scoreAS\"],NoGravity:1,Marker:1,Invulnerable:1,Invisible:1}"    
    |]
let SCOREAS_TAG = "tag=scoreAS,x=4,y=4,z=4,distance=..1.0,limit=1"

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
            yield sprintf "scoreboard players remove @e[%s,scores={%s=1..}] %s 1" SCOREAS_TAG f f
        // then call all that need to go now
        for f in allCallbackFunctions do
            yield sprintf "execute if entity @e[%s,scores={%s=1}] run function %s:%s" SCOREAS_TAG f NS f
    |]
let compile(f,name) =
    let rec replaceScores(s:string) = 
        let i = s.IndexOf("$SCORE(")
        if i <> -1 then
            let j = s.IndexOf(')',i)
            let info = s.Substring(i+7,j-i-7)
            let s = s.Remove(i,j-i+1)
            let s = s.Insert(i,sprintf "@e[%s,scores={%s}]" SCOREAS_TAG info)
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
                    sprintf """execute if entity @e[%s,scores={%s=2..}] run tellraw @a ["error, re-entrant callback %s"]""" SCOREAS_TAG nn nn
                    sprintf "scoreboard players set @e[%s] %s %d" SCOREAS_TAG nn (int info + 1) // +1 because we decr at start of gameloop
                |], nn
            else
                [|s|], null
    let a = f |> Seq.toArray 
    // $SCORE(...) is maybe e.g. "@e[tag=scoreAS,scores={...}]"
    let a = a |> Array.map replaceScores
    // $ENTITY is main scorekeeper entity (maybe e.g. "@e[tag=scoreAS]")
    let a = a |> Array.map (fun s -> s.Replace("$ENTITY",sprintf"@e[%s]"SCOREAS_TAG))
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
            yield name, [| yield sprintf """tellraw @a ["calling '%s'"]""" name; yield! code |]
//            yield name, [| yield sprintf """say ---calling '%s'---""" name; yield! code |]
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

let placeWallSignCmds x y z facing txt1 txt2 txt3 txt4 cmd isBold color =
    if facing<>"north" && facing<>"south" && facing<>"east" && facing<>"west" then failwith "bad facing wall_sign"
    let bc = sprintf """,\"bold\":\"%s\",\"color\":\"%s\" """ (if isBold then "true" else "false") color
    let c1 = if isBold then sprintf """,\"clickEvent\":{\"action\":\"run_command\",\"value\":\"%s\"} """ cmd else ""
    [|
        sprintf "setblock %d %d %d air replace" x y z
        sprintf """setblock %d %d %d wall_sign[facing=%s]{Text1:"{\"text\":\"%s\"%s%s}",Text2:"{\"text\":\"%s\"%s}",Text3:"{\"text\":\"%s\"%s}",Text4:"{\"text\":\"%s\"%s}"}""" x y z facing txt1 bc c1 txt2 bc txt3 bc txt4 bc
    |]

///////////////////////////////////////////////////////

let LOBBY = "62 25 63 0 180"
let TEAMS = [| "red"; "blue"; "green"; "yellow" |]
let game_objectives = [|
    yield "fakeStart"
    yield "isLockout"
    yield "lockoutGoal"
    yield "numActiveTeams"
    yield "hasAnyoneUpdated"
    yield "gameInProgress"     // 0 if not going, 1 if startup sequence, making spawns etc, 2 if game is running
    yield "TWENTY_MIL"
    yield "SIXTY"
    yield "ONE_THOUSAND"
    yield "Seed"
    yield "minutes"
    yield "seconds"
    yield "said25mins"         // did we already display the 25-minute score?
    yield "ticksSinceGotMap"   // time since a player who had no maps in inventory got given a new set of them
    for t in TEAMS do
        yield sprintf "%sSpawnX" t   // a number between 1 and 999 that needs to be multipled by 10000
        yield sprintf "%sSpawnY" t   // height of surface there
        yield sprintf "%sSpawnZ" t   // a number between 1 and 999 that needs to be multipled by 10000
    |]
let game_functions = [|
    yield "game_init", [|
        for o in game_objectives do
            yield sprintf "scoreboard objectives add %s dummy" o
        yield "scoreboard objectives add home trigger"
        yield "scoreboard objectives add PlayerSeed trigger"
        yield "scoreboard players set $ENTITY TWENTY_MIL 20000000"
        yield "scoreboard players set $ENTITY SIXTY 60"
        yield "scoreboard players set $ENTITY ONE_THOUSAND 1000"
        yield sprintf "gamerule gameLoopFunction %s:theloop" NS
        // TODO
        yield "scoreboard players set $ENTITY isLockout 0"
        yield "scoreboard players set $ENTITY gameInProgress 0"
        |]
    yield "make_lobby", [|
        (*
        TODO
            It's hard to design a lobby without first knowing the interface (set of activation signs) to all the features.  Get features working first.
        CARDS
            random card
            seeded card
            start game
        TEAMS
            join each color
            everyone on one team
            random into 2 teams
            random into 3 teams
            random into 4 teams
        MODES
            toggle lockout
            toggle blind-covered
        *)
        yield sprintf "teleport @a %s" LOBBY
        yield "effect give @a minecraft:night_vision 99999 1 true"
        yield "fill 60 24 60 70 28 70 air"
        yield "fill 60 24 60 70 24 70 stone"
        yield "fill 60 24 60 70 28 60 stone"
        // TODO unbold signs while gameInProgress == 1
        yield! placeWallSignCmds 61 26 61 "south" "Make RANDOM" "card" "" "" (sprintf"function %s:choose_random_seed"NS) true "black"
        yield! placeWallSignCmds 62 26 61 "south" "Choose SEED" "for card" "" "" (sprintf"function %s:choose_seed"NS) true "black"
        // TODO unbold signs while gameInProgress <> 0
        yield! placeWallSignCmds 63 26 61 "south" "START game" "" "" "" (sprintf"function %s:start1"NS) true "black"
        yield! placeWallSignCmds 65 26 61 "south" "Join team" "RED"    "" "" (sprintf "function %s:red_team_join" NS) true "black"
        yield! placeWallSignCmds 66 26 61 "south" "Join team" "BLUE"   "" "" (sprintf "function %s:blue_team_join" NS) true "black"
        yield! placeWallSignCmds 67 26 61 "south" "Join team" "GREEN"  "" "" (sprintf "function %s:green_team_join" NS) true "black"
        yield! placeWallSignCmds 68 26 61 "south" "Join team" "YELLOW" "" "" (sprintf "function %s:yellow_team_join" NS) true "black"
        //
        yield! placeWallSignCmds 61 27 61 "south" "Show all" "possible" "items" "" (sprintf"function %s:make_item_chests"NS) true "black"
        yield! placeWallSignCmds 62 27 61 "south" "fake START" "" "" "" (sprintf"function %s:fake_start"NS) true "black"
        yield! placeWallSignCmds 63 27 61 "south" "toggle" "LOCKOUT" "" "" (sprintf"function %s:toggle_lockout"NS) true "black"
        yield """kill @e[type=item,nbt={Item:{id:"minecraft:sign"}}]""" // dunno why old signs popping off when replaced by air
        |]
    yield "fake_start",[| // for testing; start sequence without the spawn points
        "scoreboard players set $ENTITY fakeStart 1"
        sprintf "function %s:start1" NS
        |]
    yield "toggle_lockout",[|
        "scoreboard players operation $ENTITY TEMP = $ENTITY isLockout"
        "execute if entity $SCORE(TEMP=1) run scoreboard players set $ENTITY isLockout 0"
        "execute unless entity $SCORE(TEMP=1) run scoreboard players set $ENTITY isLockout 1"
        sprintf "function %s:compute_lockout_goal" NS
        |]
    for t in TEAMS do
        yield sprintf"%s_team_join"t, [|
            sprintf "team join %s" t
            "scoreboard players add @s Score 0"
            sprintf "function %s:compute_lockout_goal" NS
            |]
    yield "ensure_maps",[|   // called when player @s currently has no bingo cards
        "scoreboard players add @s ticksSinceGotMap 1"
        """execute if entity @s[scores={ticksSinceGotMap=40..}] run give @s minecraft:filled_map{display:{Name:"BINGO Card"},map:0} 32"""
        "scoreboard players set @s[scores={ticksSinceGotMap=40..}] ticksSinceGotMap 0"
        |]
    yield "choose_seed",[|
        yield "scoreboard players set @a PlayerSeed 0"
        yield "scoreboard players enable @a PlayerSeed"
        yield """tellraw @a {"text":"Press 't' (chat), click below, then replace NNN with a seed number in chat"}"""
        yield """tellraw @a {"text":"CLICK HERE","clickEvent":{"action":"suggest_command","value":"/trigger PlayerSeed set NNN"}}"""
        |]
    yield "set_seed",[| // theloop listens for changes to PlayerSeed to call this as the player
        yield "scoreboard players operation Seed Score = @s PlayerSeed"
        yield "scoreboard players operation Z Calc = Seed Score"
        yield "scoreboard players set @a PlayerSeed 0"
        yield sprintf "function %s:new_card_coda" NS
        |]
    yield "choose_random_seed",[|
        // interject actual randomness, rather than deterministic pseudo
        for _i = 1 to 10 do
            yield """summon area_effect_cloud 4 4 4 {Duration:2,Tags:["aec"]}"""
            yield "scoreboard players add @e[tag=aec] TEMP 1"
        yield "scoreboard players operation Z Calc += @e[tag=aec,sort=random,limit=1] TEMP"
        yield "scoreboard players operation @e[tag=aec] TEMP *= $ENTITY SIXTY"
        yield "scoreboard players operation Z Calc += @e[tag=aec,sort=random,limit=1] TEMP"
        // compute a seed number between 100,000 and 999,999
        yield "scoreboard players set $ENTITY PRNG_MOD 899"
        yield sprintf "function %s:prng" NS
        yield "scoreboard players operation Seed Score = $ENTITY PRNG_OUT"
        yield "scoreboard players operation Seed Score *= $ENTITY ONE_THOUSAND"
        yield "scoreboard players set $ENTITY PRNG_MOD 999"
        yield sprintf "function %s:prng" NS
        yield "scoreboard players operation Seed Score += $ENTITY PRNG_OUT"
        // re-seed the PRNG with that seed number
        yield "scoreboard players operation Z Calc = Seed Score"
        yield sprintf "function %s:new_card_coda" NS
        |]
    yield "new_card_coda",[|
        yield "scoreboard players set $ENTITY fakeStart 0"
        yield sprintf "execute unless entity $SCORE(gameInProgress=2) run function %s:reset_player_scores" NS
        yield sprintf "execute if entity $SCORE(gameInProgress=2) run function %s:finish1" NS
        yield sprintf "function %s:cardgen_makecard" NS
        yield sprintf "function %s:compute_lockout_goal" NS
        |]
    yield "update_time",[|
        "execute store result score $ENTITY minutes run worldborder get"
        "scoreboard players operation $ENTITY minutes -= $ENTITY TWENTY_MIL"
        "scoreboard players operation Time Score = $ENTITY minutes"
        // while 'minutes' objective has 'total seconds', do this
        """execute if entity $SCORE(said25mins=0,minutes=1500) as @a at @s run playsound block.note.harp ambient @s ~ ~ ~ 1 0.6"""
        """execute if entity $SCORE(said25mins=0,minutes=1500) run tellraw @a [{"selector":"@p"}," got ",{"score":{"name":"@p","objective":"Score"}}," in 25 mins"]"""
        """execute if entity $SCORE(said25mins=0,minutes=1500) run scoreboard players set $ENTITY said25mins 1"""
        // compute actual MM:SS and display
        "scoreboard players operation $ENTITY seconds = $ENTITY minutes"
        "scoreboard players operation $ENTITY minutes /= $ENTITY SIXTY"
        "scoreboard players operation $ENTITY seconds %= $ENTITY SIXTY"
        """execute as $ENTITY if entity $SCORE(seconds=0..9) run title @a actionbar ["",{"score":{"name":"@s","objective":"minutes"}},":0",{"score":{"name":"@s","objective":"seconds"}}]"""
        """execute as $ENTITY if entity $SCORE(seconds=10..) run title @a actionbar ["",{"score":{"name":"@s","objective":"minutes"}},":",{"score":{"name":"@s","objective":"seconds"}}]"""
        |]
    yield "compute_height", [|
        yield "execute as @e[tag=CurrentSpawn] at @s run teleport @s ~ 230 ~"
        for _i = 1 to 230 do
            yield "execute as @e[tag=CurrentSpawn] at @s if block ~ ~-1 ~ minecraft:air run teleport @s ~ ~-1 ~"
        yield "execute as @e[tag=CurrentSpawn] at @s run setblock ~ ~-1 ~ minecraft:obsidian"
        |]
    for t in TEAMS do
        yield sprintf "do_%s_spawn" t, [|
            yield sprintf "function %s:prng" NS
            yield sprintf "scoreboard players operation $ENTITY %sSpawnX = $ENTITY PRNG_OUT" t
            yield sprintf "scoreboard players add $ENTITY %sSpawnX 1" t
            yield sprintf "function %s:prng" NS
            yield sprintf "scoreboard players operation $ENTITY %sSpawnZ = $ENTITY PRNG_OUT" t
            yield sprintf "scoreboard players add $ENTITY %sSpawnZ 1" t
            yield sprintf """summon armor_stand 1 1 1 {Invulnerable:1b,Invisible:1b,NoGravity:1b,Tags:["%sSpawn","CurrentSpawn"]}""" t
            yield sprintf "execute as @e[tag=%sSpawn] store result entity @s Pos[0] double 10000.0 run scoreboard players get $ENTITY %sSpawnX" t t
            yield sprintf "execute as @e[tag=%sSpawn] store success entity @s Pos[1] double 250.0 run scoreboard players get $ENTITY %sSpawnX" t t
            yield sprintf "execute as @e[tag=%sSpawn] store result entity @s Pos[2] double 10000.0 run scoreboard players get $ENTITY %sSpawnZ" t t
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run teleport @a[team=%s] ~0.5 ~ ~0.5" t t
            // now that players are there, wait for some terrain to gen
            yield """tellraw @a ["at a location, gen some terrain"]"""
            yield "$NTICKSLATER(20)"
            // build skybox and put players there
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run fill ~-1 235 ~-1 ~1 254 ~1 barrier hollow" t
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run teleport @a[team=%s] ~0.5 ~ ~0.5" t t
            // figure out Y height of surface
            yield sprintf "function %s:compute_height" NS
            yield sprintf "tag @e[tag=CurrentSpawn] remove CurrentSpawn"
            yield sprintf "execute as @e[tag=%sSpawn] store result score $ENTITY %sSpawnY run data get entity @s Pos[1] 1.0" t t
            // give people time in skybox while terrain gens, then put them on ground and set spawns
            yield "$NTICKSLATER(400)"
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run teleport @a[team=%s] ~0.5 ~ ~0.5" t t
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run spawnpoint @a[team=%s] ~0.5 ~ ~0.5" t t
            yield sprintf "kill @e[tag=%sSpawn]" t
            // call next continuation
            if t = "red" then
                yield sprintf "execute if entity @a[team=blue] run function %s:do_blue_spawn" NS
                yield sprintf "execute if entity @a[team=green] unless entity @a[team=blue] run function %s:do_green_spawn" NS
                yield sprintf "execute if entity @a[team=yellow] unless entity @a[team=blue] unless entity @a[team=green] run function %s:do_yellow_spawn" NS
                yield sprintf "execute unless entity @a[team=blue] unless entity @a[team=green] unless entity @a[team=yellow] run function %s:start4" NS
            elif t = "blue" then
                yield sprintf "execute if entity @a[team=green] run function %s:do_green_spawn" NS
                yield sprintf "execute if entity @a[team=yellow] unless entity @a[team=green] run function %s:do_yellow_spawn" NS
                yield sprintf "execute unless entity @a[team=green] unless entity @a[team=yellow] run function %s:start4" NS
            elif t = "green" then
                yield sprintf "execute if entity @a[team=yellow] run function %s:do_yellow_spawn" NS
                yield sprintf "execute unless entity @a[team=yellow] run function %s:start4" NS
            elif t = "yellow" then
                yield sprintf "function %s:start4" NS
            |]
    yield "compute_active_teams", [|
        yield "scoreboard players set $ENTITY numActiveTeams 0"
        for t in TEAMS do
            yield sprintf "execute if entity @a[team=%s] run scoreboard players add $ENTITY numActiveTeams 1" t
        |]
    yield "start1", [|
        // ensure folks have joined teams
        yield sprintf "function %s:compute_active_teams" NS
        yield """execute if entity $SCORE(numActiveTeams=0) run tellraw @a ["No one has joined a team - join a team color to play!"]"""
        yield sprintf "execute if entity $SCORE(numActiveTeams=1..) run function %s:start2" NS
        |]
    yield "reset_player_scores",[|
        yield "scoreboard players operation $ENTITY Seed = Seed Score"  // save seed
        yield "scoreboard players reset * Score"
        yield "scoreboard players operation Seed Score = $ENTITY Seed"  // restore seed
        for t in TEAMS do
            yield sprintf "scoreboard players set @a[team=%s] Score 0" t
        |]
    yield "compute_lockout_goal", [|
        // set up lockout goal if lockout mode selected (teamCount 2/3/4 -> goal 13/9/7)
        yield sprintf "function %s:compute_active_teams" NS
        yield "execute if entity $SCORE(numActiveTeams=1) run scoreboard players set $ENTITY lockoutGoal 25"
        yield "execute if entity $SCORE(numActiveTeams=2) run scoreboard players set $ENTITY lockoutGoal 13"
        yield "execute if entity $SCORE(numActiveTeams=3) run scoreboard players set $ENTITY lockoutGoal 9"
        yield "execute if entity $SCORE(numActiveTeams=4) run scoreboard players set $ENTITY lockoutGoal 7"
        yield "execute if entity $SCORE(isLockout=1) run scoreboard players operation LockoutGoal Score = $ENTITY lockoutGoal"
        yield "execute unless entity $SCORE(isLockout=1) run scoreboard players reset LockoutGoal Score"
        |]
    yield "start2", [|
        // clear player scores again (in case player joined server after card gen'd)
        yield sprintf "function %s:reset_player_scores" NS
        yield sprintf "function %s:compute_lockout_goal" NS
        // TODO disable other lobby buttons
        // note game in progress
        yield "scoreboard players set $ENTITY gameInProgress 1"
        yield "scoreboard players set $ENTITY said25mins 0"
        // put folks in survival mode, feed & heal, remove all xp, clear inventories
        yield "gamemode survival @a"
        yield "effect give @a minecraft:saturation 10 4 true"
        yield "effect give @a minecraft:regeneration 10 4 true"
        yield "experience set @a 0 points"
        yield "experience set @a 0 levels"
        yield "clear @a"
        yield sprintf "execute as @r run function %s:ensure_card_updated" NS
        |]
    yield "ensure_card_updated", [|  // called on one player with a cleared inventory
        yield sprintf "teleport @s %s" LOBBY
        yield """give @s minecraft:filled_map{display:{Name:"BINGO Card"},map:0} 640"""
        yield "$NTICKSLATER(20)"
        yield "clear @a"  // Note: no longer @s since NTICKSLATER
        yield sprintf "function %s:start3" NS
        |]
    yield "start3", [|
        yield """give @a minecraft:filled_map{display:{Name:"BINGO Card"},map:0} 32"""
        // give player all the effects
        yield "effect give @a minecraft:slowness 999 127 true"
        yield "effect give @a minecraft:mining_fatigue 999 7 true"
        yield "effect give @a minecraft:jump_boost 999 150 true"
        yield "effect give @a minecraft:resistance 999 4 true"
        yield "effect give @a minecraft:water_breathing 999 4 true"
        yield "effect give @a minecraft:invisibility 999 4 true"
        // set time to day so not tp at night
        yield "time set 0"
        yield sprintf "execute if entity $SCORE(fakeStart=1) run function %s:start5" NS
        yield sprintf "execute if entity $SCORE(fakeStart=0) run function %s:do_spawn_sequence" NS
        |]
    yield "do_spawn_sequence", [|
        // TODO tp all to waiting room
        // set up spawn points
        yield "scoreboard players set $ENTITY PRNG_MOD 998"
        yield sprintf "execute if entity @a[team=red] run function %s:do_red_spawn" NS
        yield sprintf "execute if entity @a[team=blue] unless entity @a[team=red] run function %s:do_blue_spawn" NS
        yield sprintf "execute if entity @a[team=green] unless entity @a[team=red] unless entity @a[team=blue] run function %s:do_green_spawn" NS
        yield sprintf "execute if entity @a[team=yellow] unless entity @a[team=red] unless entity @a[team=blue] unless entity @a[team=green] run function %s:do_yellow_spawn" NS
        |]
    yield "start4", [|
        yield "gamemode creative @a"  // TODO
        // feed & heal again
        yield "effect give @a saturation 10 4 true"
        yield "effect give @a regeneration 10 4 true"
        // clear hostile mobs
        yield "difficulty peaceful"
        yield "$NTICKSLATER(2)"
        yield "difficulty normal"
        yield """tellraw @a ["Game will begin shortly... countdown commencing..."]"""
        yield "$NTICKSLATER(20)"
        yield """tellraw @a ["3"]"""
        yield "execute as @a at @s run playsound block.note.harp ambient @s ~ ~ ~ 1 0.6"
        yield "$NTICKSLATER(20)"
        yield """tellraw @a ["2"]"""
        yield "execute as @a at @s run playsound block.note.harp ambient @s ~ ~ ~ 1 0.6"
        yield "$NTICKSLATER(20)"
        yield """tellraw @a ["1"]"""
        yield "execute as @a at @s run playsound block.note.harp ambient @s ~ ~ ~ 1 0.6"
        yield "$NTICKSLATER(20)"
        // TODO once more, re-tp anyone who maybe moved, the cheaters! (wait to kill armor_stands?)
        yield sprintf "function %s:start5" NS
        |]
    yield "start5", [|
        yield "time set 0"
        yield "effect clear @a"
        // TODO custom game modes, for now, just always NV
        yield "effect give @a minecraft:night_vision 99999 1 true"
        yield """tellraw @a ["Start! Go!!!"]"""
        yield "execute as @a at @s run playsound block.note.harp ambient @s ~ ~ ~ 1 1.2"
        // enable triggers (for click-in-chat-to-tp-home stuff)
        yield "scoreboard players set @a home 0"
        yield "scoreboard players enable @a home"
        // option to get back
        yield """tellraw @a ["(If you need to quit before getting BINGO, you can"]"""
        yield """tellraw @a [{"underlined":"true","text":"press 't' (chat), then click this line to return to the lobby)","clickEvent":{"action":"run_command","value":"/trigger home set 1"}}]"""
        yield "worldborder set 20000000"          // 20 million wide is 10 million from spawn
        yield "worldborder add 10000000 10000000" // 10 million per 10 million seconds is one per second
        yield "scoreboard players set $ENTITY gameInProgress 2"
        yield "scoreboard players set $ENTITY hasAnyoneUpdated 0"
        |]
    yield "go_home", [|
        sprintf "teleport @s %s" LOBBY
        "effect give @s minecraft:saturation 10 4 true"  // feed (and probably will heal some too)
        "effect give @s minecraft:night_vision 99999 1 true"
        "scoreboard players set @a home 0"
        "scoreboard players enable @a home"    // re-enable for everyone, so even if die in lobby afterward and respawn out in world again, can come back
        |]
    yield "finish1", [| // called for transition gameInProgress 2->0
        "scoreboard players set $ENTITY gameInProgress 0"
        sprintf "teleport @a %s" LOBBY
        // TODO "gamemode survival @a"
        "clear @a"
        // feed & heal, as people get concerned in lobby about this
        "effect give @a minecraft:saturation 10 4 true"
        "effect give @a minecraft:regeneration 10 4 true"
        // TODO consider separate 'end game' sign? end game tps back but preserves scoreboard, whereas 'new card' resets scores? right now first 'new game' is 'end game'...
        (*
        // clear player scores
        "scoreboard players set @a Score 0"
        "scoreboard players reset Time Score"
        "scoreboard players reset Minutes Score"
        "scoreboard players reset Seconds Score"
        *)
        |]
    |]

///////////////////////////////////////////////////////

let MAP_UPDATE_ROOM = "62 10 72"
let map_update_objectives = [|
    yield "ticksLeftMU"        // remaining time left for a player who dropped maps to wait in map-update room for card colors to have time to redraw
    yield sprintf "ReturnX"
    yield sprintf "ReturnY"
    yield sprintf "ReturnZ"
    yield sprintf "ReturnRotX"
    yield sprintf "ReturnRotY"
    |]
let map_update_functions = [| 
    yield "map_update_init", [|
        for o in map_update_objectives do
            yield sprintf "scoreboard objectives add %s dummy" o
        |]
    yield "map_update_tick", [| // called every tick
        // find player who dropped map
        sprintf """execute unless entity @a[scores={ticksLeftMU=1..}] at @e[limit=1,type=item,nbt={Item:{id:"minecraft:filled_map",tag:{map:0}}}] as @p[distance=..5] run function %s:warp_home""" NS
        "kill @e[type=item,nbt={Item:{id:\"minecraft:filled_map\",tag:{map:0}}}]"  // TODO is this super expensive?
        // run progress for anyone in the update room
        sprintf "execute as @a[scores={ticksLeftMU=1}] run function %s:warp_back" NS
        "scoreboard players remove @a[scores={ticksLeftMU=1..}] ticksLeftMU 1"
        |]
    yield "warp_home", [|
        "scoreboard players set @s ticksLeftMU 30"  // TODO calibrate best value
        "execute store result score @s ReturnX run data get entity @s Pos[0] 128.0"   // doubles
        "execute store result score @s ReturnY run data get entity @s Pos[1] 128.0"
        "execute store result score @s ReturnZ run data get entity @s Pos[2] 128.0"
        "execute store result score @s ReturnRotX run data get entity @s Rotation[0] 8.0"   // floats
        "execute store result score @s ReturnRotY run data get entity @s Rotation[1] 8.0"
        """tellraw @a [{"selector":"@s"}," is updating the BINGO map"]"""
        //"data merge entity @e[type=!player,distance=..160] {PersistenceRequired:1}"  // preserve mobs
        "execute as @e[type=!player,distance=..160] run data merge entity @s {PersistenceRequired:1}"  // preserve mobs
        // TODO ever a reason to un-persist?
        sprintf "tp @s %s 180 0" MAP_UPDATE_ROOM
        //TODO "execute at @s run particle portal ~ ~ ~ 3 2 3 1 99 @s"
        "execute at @s run playsound entity.endermen.teleport ambient @a"
        |]
    yield "warp_back", [|
        """summon area_effect_cloud 4 4 4 {Duration:1,Tags:["return_loc"],Rotation:[20f,20f]}""" // needs some rotation to be able to store to it later
        "execute store result entity @e[limit=1,tag=return_loc] Pos[0] double 0.0078125 run scoreboard players get @s ReturnX"
        "execute store result entity @e[limit=1,tag=return_loc] Pos[1] double 0.0078125 run scoreboard players get @s ReturnY"
        "execute store result entity @e[limit=1,tag=return_loc] Pos[2] double 0.0078125 run scoreboard players get @s ReturnZ"
        "execute store result entity @e[limit=1,tag=return_loc] Rotation[0] float 0.125 run scoreboard players get @s ReturnRotX"
        "execute store result entity @e[limit=1,tag=return_loc] Rotation[1] float 0.125 run scoreboard players get @s ReturnRotY"
        "teleport @s @e[limit=1,tag=return_loc]"
        //TODO "execute at @s run particle portal ~ ~ ~ 3 2 3 1 99 @s"
        "execute at @s run playsound entity.endermen.teleport ambient @a"
        "scoreboard players set $ENTITY hasAnyoneUpdated 1"
        // don't kill a_e_c, as can't kill in same tick as summon, and also its Duration will expire it
        |]
    |]


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
    // https://bugs.mojang.com/browse/MC-117653
    // the 'changed' notification does not happen while you have a container GUI open
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
    let functionText = 
        if CHECK_EVERY_TICK then "" else sprintf """
execute if entity @e[%s,scores={gameInProgress=2}] if entity @s[team=red] run function test:red_inventory_changed
execute if entity @e[%s,scores={gameInProgress=2}] if entity @s[team=blue] run function test:blue_inventory_changed
execute if entity @e[%s,scores={gameInProgress=2}] if entity @s[team=green] run function test:green_inventory_changed
execute if entity @e[%s,scores={gameInProgress=2}] if entity @s[team=yellow] run function test:yellow_inventory_changed
advancement revoke @s only test:on_inventory_changed""" SCOREAS_TAG SCOREAS_TAG SCOREAS_TAG SCOREAS_TAG
    System.IO.File.WriteAllText("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\BingoFor1x13\datapacks\BingoPack\data\test\functions\inventory_changed.mcfunction""",functionText)

///////////////////////////////////////////////////////////////////////////////

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
        |]
    yield "checker_new_card",[|
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
    yield "check_inventory",[| // called as a player @s
        "execute if entity @s[team=red] run function test:red_inventory_changed"
        "execute if entity @s[team=blue] run function test:blue_inventory_changed"
        "execute if entity @s[team=green] run function test:green_inventory_changed"
        "execute if entity @s[team=yellow] run function test:yellow_inventory_changed"
        |]
    for t in TEAMS do
        yield sprintf "%s_inventory_changed" t, [|        // called when player @s's inventory changed and he is on team t
            yield sprintf "scoreboard players set $ENTITY gotItems 0"
            for s in SQUARES do
                yield sprintf "scoreboard players set $ENTITY gotAnItem 0"
                yield sprintf "execute if entity $SCORE(%sCanGet%s=1) run function %s:check%s" t s NS s
                yield sprintf "execute if entity $SCORE(gotAnItem=1) run function %s:%s_got_square_%s" NS t s
            yield sprintf """execute if entity $SCORE(gotItems=1) run tellraw @a [{"selector":"@s]"}," got an item! (",{"score":{"name":"@s","objective":"Score"}}," in ",{"score":{"name":"Time","objective":"Score"}},"s)"]"""
            yield sprintf """execute if entity $SCORE(gotItems=1,hasAnyoneUpdated=0) run tellraw @a ["To update the BINGO map, drop one copy on the ground"]"""
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
                // TODO test actual logic to color the game board square appropriately
                // TODO maybe improve 2-player to halves?
                let x = 4 + 24*(int s.[0] - int '0' - 1)
                let y = 30
                let z = 0 + 24*(int s.[1] - int '0' - 1)
                // determine if we should fill the whole square
                yield sprintf "scoreboard players set $ENTITY TEMP 0"
                yield sprintf "execute if entity $SCORE(numActiveTeams=1) run scoreboard players set $ENTITY TEMP 1"
                yield sprintf "execute if entity $SCORE(isLockout=1) run scoreboard players set $ENTITY TEMP 1"
                yield sprintf "execute if entity $SCORE(TEMP=1) run fill %d %d %d %d %d %d %s replace clay" x y z (x+22) y (z+22) (if t="green" then "emerald_block" else t+"_wool")
                // else fill the corner
                if t = "red" then
                    yield sprintf "execute unless entity $SCORE(TEMP=1) run fill %d %d %d %d %d %d %s replace clay" (x+00) y (z+00) (x+11) y (z+11) (if t="green" then "emerald_block" else t+"_wool")
                if t = "blue" then
                    yield sprintf "execute unless entity $SCORE(TEMP=1) run fill %d %d %d %d %d %d %s replace clay" (x+12) y (z+00) (x+22) y (z+11) (if t="green" then "emerald_block" else t+"_wool")
                if t = "green" then
                    yield sprintf "execute unless entity $SCORE(TEMP=1) run fill %d %d %d %d %d %d %s replace clay" (x+00) y (z+12) (x+11) y (z+22) (if t="green" then "emerald_block" else t+"_wool")
                if t = "yellow" then
                    yield sprintf "execute unless entity $SCORE(TEMP=1) run fill %d %d %d %d %d %d %s replace clay" (x+12) y (z+12) (x+22) y (z+22) (if t="green" then "emerald_block" else t+"_wool")
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
        // put time on scoreboard
        yield "scoreboard players operation Minutes Score = $ENTITY minutes"
        yield "scoreboard players operation Seconds Score = $ENTITY seconds"
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
(*
        if flatBingoItems.Length > 128 then
            failwith "bad binary search"
*)
        yield sprintf "check%s" s, [|
            for i=0 to flatBingoItems.Length-1 do
                // TODO more efficient binary search?
                yield sprintf """execute if entity $SCORE(square%s=%d) store success score $ENTITY gotAnItem run clear @s %s 1""" s i flatBingoItems.[i]
                // Note - profiling suggests this guard does not help: if entity @s[nbt={Inventory:[{id:"minecraft:%s"}]}] 
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
        let x = 61
        let y = 25
        let z = 66
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
        |]
    yield "cardgen_choose1", [|
        yield sprintf "scoreboard players set $ENTITY PRNG_MOD 28"
        yield sprintf "function %s:prng" NS
        yield sprintf "scoreboard players operation $ENTITY CARDGENTEMP = $ENTITY PRNG_OUT"
        // ensure exactly one call
        yield sprintf "scoreboard players set $ENTITY CALL 1"
        for i = 0 to bingoItems.Length-1 do
            yield sprintf "execute if entity $SCORE(CARDGENTEMP=%d,CALL=1) run function %s:cardgen_bin%02d" i NS i
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
        yield sprintf "kill @e[tag=sky]"
        yield sprintf """summon armor_stand 7 30 3 {Tags:["sky"],NoGravity:1,Invulnerable:1,Invisible:1}"""
        yield sprintf "scoreboard players set $ENTITY squaresPlaced 0"
        for i = 0 to bingoItems.Length-1 do
            yield sprintf "scoreboard players set $ENTITY bin%02d 0" i
        yield sprintf "fill 4 30 0 131 30 128 clay"
        yield sprintf "fill 4 31 0 131 31 128 air"
        for _x = 1 to 5 do
            yield sprintf "function %s:cardgen_choose1" NS
            yield sprintf "execute at @e[tag=sky] run teleport @e[tag=sky] ~24 ~ ~"
            yield sprintf "function %s:cardgen_choose1" NS
            yield sprintf "execute at @e[tag=sky] run teleport @e[tag=sky] ~24 ~ ~"
            yield sprintf "function %s:cardgen_choose1" NS
            yield sprintf "execute at @e[tag=sky] run teleport @e[tag=sky] ~24 ~ ~"
            yield sprintf "function %s:cardgen_choose1" NS
            yield sprintf "execute at @e[tag=sky] run teleport @e[tag=sky] ~24 ~ ~"
            yield sprintf "function %s:cardgen_choose1" NS
            yield sprintf "execute at @e[tag=sky] run teleport @e[tag=sky] ~-96 ~ ~24"
        yield sprintf "function %s:checker_new_card" NS
        |]
    |]
let cardgen_compile() = // TODO this is really full game, naming/factoring...
    let r = [|
        for name,code in cardgen_functions do
            yield! compile(code, name)
        for name,code in checker_functions do
            yield! compile(code, name)
        for name,code in game_functions do
            yield! compile(code, name)
        for name,code in map_update_functions do
            yield! compile(code, name)
        yield! compile([|
            yield sprintf "execute if entity $SCORE(gameInProgress=2) run function %s:update_time" NS
            yield sprintf "execute if entity $SCORE(gameInProgress=2) run function %s:map_update_tick" NS
            yield sprintf "execute as @a[scores={home=1}] run function %s:go_home" NS
            yield sprintf "execute if entity $SCORE(gameInProgress=0) as @p[scores={PlayerSeed=1..}] run function %s:set_seed" NS
            yield sprintf """execute unless entity $SCORE(gameInProgress=1) as @a unless entity @s[nbt={Inventory:[{id:"minecraft:filled_map",tag:{map:0}}]}] run function %s:ensure_maps""" NS
            if CHECK_EVERY_TICK then
                yield sprintf "execute if entity $SCORE(gameInProgress=2) as @a run function %s:check_inventory" NS
            yield! gameLoopContinuationCheck()
            |],"theloop")
        yield! compile(prng, "prng")
        yield! compile(prng_init(), "prng_init")
        yield makeItemChests()
        yield "init",[|
            yield "kill @e[type=!player]"
            yield "clear @a"
            yield! entity_init()
            yield sprintf"function %s:make_lobby"NS
            yield sprintf"function %s:prng_init"NS
            yield sprintf"function %s:checker_init"NS
            yield sprintf"function %s:cardgen_init"NS
            yield sprintf"function %s:game_init"NS
            yield sprintf"function %s:map_update_init"NS
            yield sprintf"function %s:choose_random_seed"NS
            |]
        |]
    printfn "%A" r
    for name,code in r do
        writeFunctionToDisk(name,code)
