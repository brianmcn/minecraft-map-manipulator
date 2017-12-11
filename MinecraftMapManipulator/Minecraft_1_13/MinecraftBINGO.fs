module MinecraftBINGO

let SKIP_WRITING_CHECK = false  // turn this on to save time if you're not modifying checker code
let USE_GAMELOOP = true         // if false, use a repeating command block instead

// TODO can I abuse eventing and enable/disable for continuations? (add/remove collection?)
// not yet: https://bugs.mojang.com/browse/MC-123032
// if that gets fixed, then we have the ability to create 'groups of functions', and add/remove known sets of functions to/from the group, and call all functions in the group
// that could be used e.g. for continuations, so only the 'active' continuations get run every tick, rather than checking all of them, which may be better if /datapack enable/disable is a fast operation (measure)
// what is behavior if #base:group calls child1:run which disables child1/child2?
// could it be used as a queue? not efficiently, since only operation is 'call all', so getting the front is still O(N); just eventing (subscribe/unsubscribe/get-invoked-on_whatever)
// could it be used as a CPS machine? maybe... each guy would disable himself and enable the continuationIP, so rather than having the trampoline have to if(IP=1)call1 elif(IP=2)call2... it could just #call:ip
// that is, we have a type of dynamic function dispatch; I can call #foo:bar, and change where that dispatches to at runtime (among a finite subset known at design-time), ok... what can we do with that?
// yes, this is pokable-command-blocks-without-a-one-tick-delay -- function indirection
// i mean, i guess I could have square11-dispatches-to-diamond do that way, but it requires 25x70=1750 datapacks, but then that's set at cardgen time...
// and i can even do that while MC-123032 exists, since i just need one.  at cardgen, call each square asking it to disable itself, then as gen card, enable the corresponding 25 packs
// and then the inventory check is just #bingo:check_square11 which will dispatch to like check_diamond or whatever...
// still not arrays, but the indirection allows what was a binary search to become an o(1), at the cost of a more expensive 'set' at game start time
// yeah, more generally, you can at design time create #array1..#arrayn, which can store fun1..funm, and then with MxN datapacks you can initialize the array in a design-time 1..n walk and then O(1) run the functions whenever
// the key is that bingo's "25" is not an array, since all I ever do is run 25 things in a row, it's just 'do a set of instructions (that happens to be 25 long)
// whereas the item-to-clear in each square is an array, i want to, at runtime, dispatch to different stuff based on a runtime index value.  right now O(logN) with binary search, and can be O(1) with datapack, because can store "function pointer"
// each bingo square is its own named 'event', whose event-subscription-dispatch can be updated at cardgen time
// but note that the event-subscription-update is linear (or logN), since I need to say if(x=1)enable datapack1, if(x=2)enable datapack2, ... whereas today it's O(1) to store an integer
// so I flip the current "O(1) set and O(logN) call" to "O(logN) set and O(1) call"   (store an int and dispatch based on it --> enable the corresponding data pack and then fire its event)
// BUT... I am also the person that argues O(logN)==O(1) in practice.  So this datapack abuse should be used only as last resort, I expect.

// TOOD learn tags - eventing? add function to group?
// TODO yes, this is how customizing should work, e.g. the "night vision + depth strider" is a data pack, and bingo has an event group for #custom:on_start and #custom:on_respawn and child packs add to that
// TODO optional announcing item could be an on_got_item001 hooks, or just a flag
// TODO 'blind' play would need like an on_new_card and on_got_square11 hooks
// TODO 128 item groups? #item001 = diamond? #set_structure001 = structure block for the art? show-all-items chest may be problematic.  may impact perf.  unsure how to do 'max value' for cardgen & checks...

// TODO could do item groups e.g. for fish like before (salmon + cod + pufferfish + clownfish) (if so, measure perf)

// TODO if re-use same seed, bedrock spawns atop old beacon - good or bad?  detect and give feedback?

// TODO suggested to fill map update room head with water IFF player's head is in water at moment of teleport :P

// TODO test non-playing players who just want to hang out in lobby

// TODO 'utility sign' hidden option?
// TODO replay-same-card sign + fake start gives 'plan marker' for 20-no-bingo... - can do now via "seed 0" to replay easily
// TODO next seed (seed+1) is a good utility sign for gothfaerie
// TODO zam idea: Have just one sign that gives the player a book with (clickable) config options in.

// TODO consider recipe book for complex crafting?

// TODO tall lobby for building? open ceiling? figure out aesthetic, maybe something that allows others to build-out? if signs are movable, pretty open-ended?

// TODO evaluate new items, other new features

// TODO 150 150 150 room

// TODO new icon, code to rename world in level.dat

// TODO history book, donation link, thank testers

// TODO decide what 'loadouts' need to ship in-game (vanilla; NV; NV+start-with-boats; NV+DS; starting chest... are ones I know are used) - so long as I can 'ship' them out-of-band with separate datapack, prefer minimal
//   - but figure how how OOB alternatives work here in terms of exclusive-selection UI from lobby...
//   - or even if not exclusive (e.g. NV on/off, DS on/off, chest on off, all stack), how to select and show options from base and 3rd party? (wubbi trick for accumulating strings with named AS is too heavy)
//   - maybe each datapack can have #on_custom_options which gives players a book of options, and main lobby can have sign that calls that? seems good? yeah, and
//   - each datapack should also have a #reset_to_default or something for 'clearing' their settings, and then there can be a master control to go back to default
//   - and then the builtin options (like NV, annc item got, ...) can be implemented as a separate datapack that turns into sample code for other datapack authors to use as examples?
//        - but book text is not dynamic, hmm... yeah, completely fixed at first-read-time... replace book in their hand at moment they click?
//          idea: "This option is selector:@e[tag=thisoption]" and there's two entities, one named "ON" and one named "OFF" and the command guts tag one and untag the other and then the text changes based on the scores or whatever
//   - OR could have config 'areas' at spawn with floating invisible named armor stands to display text and options? ...
//   - OR just clickable text in chat, and something displays options, and you can click to toggle
// /give @p sign{BlockEntityTag:{Text1:"[\"foo\",{\"score\":{\"name\":\"@p\",\"objective\":\"Score\"}}]",Text2:"[{\"score\":{\"name\":\"@p\",\"objective\":\"Score\"}}]",Text3:"[{\"selector\":\"@p\"}]",Text4:"[]",id:"minecraft:sign"}} 1

// TODO first time loading up map signs? (see HasTheMapEverBeenLoadedBefore)

// TODO maybe disable lockout each game unless set, then can be multiplayer button? hmmm... and sign can change to show current state

// TODO rather than beacon at spawn, mysterious divining rod like text on action bar that points back to spawn? can be computed with coords... would be easy if we get ^ ^ ^   https://en.wikipedia.org/wiki/Arrows_%28Unicode_block%29

// TODO 'tweet your score on seed' possible using the named-entity trick for putting together strings? ugh, yes strings, but not urls probably

// TODO multiplayer where teams spawn nearby (pvp etc)

// TODO investigate 'off' RNG in source code

// starting options (maybe both start & respawn same?)
// NV
// DS
// boats
// team chest
// OOB example elytra

// got item side effects
// announceItem012/fireworkItem01
// announceOnlyTeam  // TODO test this
// OOB reveal square in blind game
// OOB different sounds depending on square got

// other
// update reminder each game? "handHolding"
// "leadingWS" (and CustomName of scoreAS)

/////////////////////////////////////////////////////////////

(*
testers:
Ekiph
brtw
Zampone
gothfaerie
WarNev3rChanges
*)
/////////////////////////////////////////////////////////////

type Coords(x:int,y:int,z:int) = 
    member this.X = x
    member this.Y = y
    member this.Z = z
    member this.Tuple = x,y,z
    member this.STR = sprintf "%d %d %d" x y z
    member this.TPSTR = sprintf "%d %d.0 %d" x y z   // TODO seems like not specifying a decimal adds 0.5 to each value when tp'ing; we typically want y to be floor-flush and xz to be centered, hence this hack
    member this.Offset(dx,dy,dz) = new Coords(x+dx, y+dy, z+dz)

let MAP_UPDATE_ROOM = Coords(62,10,72)
let WAITING_ROOM = Coords(71,10,72)
let ART_HEIGHT = 40 // TODO

let NS = "test"
//let FOLDER = """"C:\Users\Admin1\AppData\Roaming\.minecraft\saves\BingoFor1x13"""
let FOLDER = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\testing"""
let allDirsEnsured = new System.Collections.Generic.HashSet<_>()
let writeFunctionToDisk(name,code) =
    let DIR = System.IO.Path.Combine(FOLDER,"""datapacks\BingoPack\data\"""+NS+"""\functions""")
    let FIL = System.IO.Path.Combine(DIR,sprintf "%s.mcfunction" name)
    let dir = System.IO.Path.GetDirectoryName(FIL)
    if allDirsEnsured.Add(dir) then
        System.IO.Directory.CreateDirectory(dir) |> ignore
    System.IO.File.WriteAllLines(FIL, code)

////////////////////////////

let FIL = System.IO.Path.Combine(FOLDER,"""datapacks\BingoPack\data\minecraft\tags\functions\tick.json""")
System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FIL)) |> ignore
if USE_GAMELOOP then
    System.IO.File.WriteAllText(FIL,sprintf"""{"values": ["%s:theloop"]}"""NS)
else
    System.IO.File.WriteAllText(FIL,"")

let entity_init() = [|
    yield "setworldspawn 64 64 64"
    yield "kill @e[tag=scoreAS]"
    yield """summon armor_stand 4 4 4 {CustomName:"                          ",Tags:["scoreAS"],NoGravity:1,Marker:1,Invulnerable:1,Invisible:1}"""
    |]
let ENTITY_TAG = "tag=scoreAS,x=4,y=4,z=4,distance=..1.0,limit=1"
let LEADING_WHITESPACE = sprintf """{"selector":"@e[%s,scores={leadingWS=1}]"}""" ENTITY_TAG

let allCallbackShortNames = ResizeArray()
let continuationNum = ref 1
let newName() =    // names are like 'cont6'... this is used as scoreboard objective name, and then function full name will be cont/cont6
    let r = sprintf "cont%d" !continuationNum
    incr continuationNum
    r
let gameLoopContinuationCheck() =
    [|
#if DEBUG
//        yield "say ---calling gameLoop---"
#endif    
        // first decr all cont counts (after, 0=unscheduled, 1=now, 2...=future)
        for f in allCallbackShortNames do
            yield sprintf "scoreboard players remove @e[%s,scores={%s=1..}] %s 1" ENTITY_TAG f f
        // then call all that need to go now
        for f in allCallbackShortNames do
            yield sprintf "execute if entity @e[%s,scores={%s=1}] run function %s:cont/%s" ENTITY_TAG f NS f
    |]
let compile(f,name) =
    let rec replaceScores(s:string) = 
        let i = s.IndexOf("$SCORE(")
        if i <> -1 then
            let j = s.IndexOf(')',i)
            let info = s.Substring(i+7,j-i-7)
            let s = s.Remove(i,j-i+1)
            let s = s.Insert(i,sprintf "@e[%s,scores={%s}]" ENTITY_TAG info)
            replaceScores(s)
        else
            s
    let replaceContinue(s:string) = 
        // TODO if there are many NTICKSLATER in one non-re-entrant block of code, then we could 'group' them so there is just one variable to check
        // in the main game loop each tick, rather than many... that may or may not be a useful perf optimization
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
            allCallbackShortNames.Add(nn)
            [|
                sprintf """execute if entity @e[%s,scores={%s=2..}] run tellraw @a ["error, re-entrant callback %s"]""" ENTITY_TAG nn nn
                sprintf "scoreboard players set @e[%s] %s %d" ENTITY_TAG nn (int info + 1) // +1 because we decr at start of gameloop
            |], "cont/"+nn
        else
            [|s|], null
    let a = f |> Seq.toArray 
    // $SCORE(...) is maybe e.g. "@e[tag=scoreAS,scores={...}]"
    let a = a |> Array.map replaceScores
    // $ENTITY is main scorekeeper entity (maybe e.g. "@e[tag=scoreAS]")
    let a = a |> Array.map (fun s -> s.Replace("$ENTITY",sprintf"@e[%s]"ENTITY_TAG))
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
            if name <> "theloop" then
                yield name, [| yield sprintf """tellraw @a ["calling '%s'"]""" name; yield! code |]
            else
                yield name, [| yield! code; yield sprintf """tellraw @a ["at end theloop, cont6:",{"score":{"name":"@e[%s]","objective":"cont6"}}]""" ENTITY_TAG |]
        |]
#endif    
    r

///////////////////////////////////////////////////////

let prng_init() = [|
    yield "scoreboard objectives add PRNG_MOD dummy"
    yield "scoreboard objectives add PRNG_OUT dummy"
    yield "scoreboard objectives add Calc dummy"
    yield "scoreboard players set A Calc 1103515245"
    yield "scoreboard players set C Calc 12345"
    yield "scoreboard players set Two Calc 2"
    yield "scoreboard players set TwoToSixteen Calc 65536"
    for cbn in allCallbackShortNames do
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

let LOBBY = "62 25 63 0 180"
let TEAMS = [| "red"; "blue"; "green"; "yellow" |]
let game_objectives = [|
    // GLOBALS
    yield "CALL"  // for 'else' flow control - exclusive branch; always 0, except 1 just before a switch and 0 moment branch is taken
    yield "TEMP"  // a temporary variable anyone can use locally
    // bingo main game logic
    yield "Score"
    yield "teamNum"            // on players, 1=red, 2=blue, 3=green, 4=yellow; useful for finding all teammates
    yield "fakeStart"
    yield "isLockout"
    yield "lockoutGoal"
    yield "numActiveTeams"
    yield "hasAnyoneUpdated"
    yield "handHolding"        // 1 if we need to explain how to update the card, how to click the chat, etc; 0 if not
    yield "leadingWS"          // 1 if we want leading whitespace to keep most chat away from left side; 0 if not
    yield "announceOnlyTeam"   // 1 if announce/firework only to your teammates, 0 if to everyone
    yield "announceItem"       // 2 if announce item name, 1 if just say 'got an item', 0 if no text generated
    yield "fireworkItem"       // 1 if play sound when get item, 0 if silent
    yield "gameInProgress"     // 0 if not going, 1 if startup sequence, making spawns etc, 2 if game is running
    yield "TWENTY_MIL"
    yield "SIXTY"
    yield "ONE_THOUSAND"
    yield "Seed"
    yield "minutes"
    yield "seconds"
    yield "preseconds"         // is 0 if seconds <10, else is blank (for xx:xx display)
    yield "said25mins"         // did we already display the 25-minute score?
    yield "ticksSinceGotMap"   // time since a player who had no maps in inventory got given a new set of them
    for t in TEAMS do
        yield sprintf "%sLeftHalf" t  // in a 2-team game, should this color fill left half
        yield sprintf "%sRightHalf" t // in a 2-team game, should this color fill right half
        yield sprintf "%sSpawnX" t    // a number between 1 and 999 that needs to be multipled by 10000
        yield sprintf "%sSpawnY" t    // height of surface there
        yield sprintf "%sSpawnZ" t    // a number between 1 and 999 that needs to be multipled by 10000
    |]
let game_functions = [|
    yield "game_init", [|
        for o in game_objectives do
            yield sprintf "scoreboard objectives add %s dummy" o
        for t in TEAMS do
            yield sprintf "team add %s" t
            yield sprintf "team option %s color %s" t t
        yield "scoreboard objectives setdisplay sidebar Score"
        yield "scoreboard objectives add home trigger"
        yield "scoreboard objectives add PlayerSeed trigger"
        yield "scoreboard objectives add Deaths deathCount"  // for use by on_respawn
        yield "scoreboard players set $ENTITY TWENTY_MIL 20000000"
        yield "scoreboard players set $ENTITY SIXTY 60"
        yield "scoreboard players set $ENTITY ONE_THOUSAND 1000"
        yield "scoreboard players set $ENTITY isLockout 0"
        yield "scoreboard players set $ENTITY gameInProgress 0"
        yield "scoreboard players set $ENTITY handHolding 1"
        yield "scoreboard players set $ENTITY announceItem 2"
        yield "scoreboard players set $ENTITY announceOnlyTeam 0"
        yield "scoreboard players set $ENTITY fireworkItem 1"
        yield "scoreboard players set $ENTITY leadingWS 0"
        // loop
        yield "setblock 68 25 64 air"
        if not USE_GAMELOOP then
            yield sprintf """setblock 68 25 64 repeating_command_block{auto:1b,TrackOutput:0b,Command:"function %s:theloop"}""" NS
        |]
    let placeWallSignCmds x y z facing txt1 txt2 txt3 txt4 cmd isBold onlyPlaceIfMultiplayer =
        if facing<>"north" && facing<>"south" && facing<>"east" && facing<>"west" then failwith "bad facing wall_sign"
        let bc = sprintf """,\"bold\":\"%s\",\"color\":\"%s\" """ (if isBold then "true" else "false") (if isBold then "black" else "gray")
        let c1 = if isBold && (cmd<>null) then sprintf """,\"clickEvent\":{\"action\":\"run_command\",\"value\":\"%s\"} """ cmd else ""
        [|
            sprintf "setblock %d %d %d air replace" x y z
            sprintf """%ssetblock %d %d %d wall_sign[facing=%s]{Text1:"{\"text\":\"%s\"%s%s}",Text2:"{\"text\":\"%s\"%s}",Text3:"{\"text\":\"%s\"%s}",Text4:"{\"text\":\"%s\"%s}"}""" 
                        (if onlyPlaceIfMultiplayer then "execute if entity $SCORE(TEMP=2..) run " else "") x y z facing txt1 bc c1 txt2 bc txt3 bc txt4 bc
        |]
    for gip in [0;1;2] do // gameInProgress
        yield sprintf"place_signs%d"gip, [|
            let seedSignsEnabled = gip<>1
            let otherSignsEnabled = gip=0
            // sanity check
            yield sprintf """execute unless entity $SCORE(gameInProgress=%d) run tellraw @a ["ERROR: place_signs%d was called but gameInProgress is ",{"score":{"name":"@e[%s]","objective":"gameInProgress"}}]""" gip gip ENTITY_TAG
            // count the number of players, store in TEMP
            yield "scoreboard players set $ENTITY TEMP 0"
            yield "execute as @a run scoreboard players add $ENTITY TEMP 1"
            // unbold signs while gameInProgress == 1
            yield! placeWallSignCmds 61 26 61 "south" "Make RANDOM" "card" "" "" (sprintf"function %s:choose_random_seed"NS) seedSignsEnabled false
            yield! placeWallSignCmds 62 26 61 "south" "Choose SEED" "for card" "" "" (sprintf"function %s:choose_seed"NS) seedSignsEnabled false
            // unbold signs while gameInProgress <> 0
            yield! placeWallSignCmds 63 26 61 "south" "START game" "" "" "" (sprintf"function %s:start1"NS) otherSignsEnabled false
            yield! placeWallSignCmds 65 26 61 "south" "Join team" "RED"    "" "" (sprintf "function %s:red_team_join" NS) otherSignsEnabled false
            yield! placeWallSignCmds 66 26 61 "south" "Join team" "BLUE"   "" "" (sprintf "function %s:blue_team_join" NS) otherSignsEnabled false
            yield! placeWallSignCmds 67 26 61 "south" "Join team" "GREEN"  "" "" (sprintf "function %s:green_team_join" NS) otherSignsEnabled false
            yield! placeWallSignCmds 68 26 61 "south" "Join team" "YELLOW" "" "" (sprintf "function %s:yellow_team_join" NS) otherSignsEnabled false
            //
            yield! placeWallSignCmds 61 27 61 "south" "Show all" "possible" "items" "" (sprintf"function %s:make_item_chests"NS) otherSignsEnabled false
            yield! placeWallSignCmds 62 27 61 "south" "fake START" "" "" "" (sprintf"function %s:fake_start"NS) otherSignsEnabled false
            // you kinda only want this for multiplayer, but if lockout, and others leave, you'd be stuck permanently in lockout mode unless this button appears to allow you to turn it off
            yield! placeWallSignCmds 63 27 61 "south" "toggle" "LOCKOUT" "" "" (sprintf"function %s:toggle_lockout"NS) otherSignsEnabled false 
            //
            yield! placeWallSignCmds 65 27 61 "south" "put all on" "ONE team" "" "" (sprintf"function %s:assign_1_team"NS) otherSignsEnabled true
            yield! placeWallSignCmds 66 27 61 "south" "divide into" "TWO teams" "" "" (sprintf"function %s:assign_2_team"NS) otherSignsEnabled true
            yield! placeWallSignCmds 67 27 61 "south" "divide into" "THREE teams" "" "" (sprintf"function %s:assign_3_team"NS) otherSignsEnabled true
            yield! placeWallSignCmds 68 27 61 "south" "divide into" "FOUR teams" "" "" (sprintf"function %s:assign_4_team"NS) otherSignsEnabled true
            //
            yield """kill @e[type=item,nbt={Item:{id:"minecraft:sign"}}]""" // dunno why old signs popping off when replaced by air
            |]
    yield "make_lobby", [|
        (*
        TODO
            It's hard to design a lobby without first knowing the interface (set of activation signs) to all the features.  Get features working first.
        MODES
            toggle blind-covered
        *)
        yield sprintf "teleport @a %s" LOBBY
        yield "effect give @a minecraft:night_vision 99999 1 true"
        yield "fill 60 24 60 70 28 70 air"
        yield "fill 60 24 60 70 24 70 stone"
        yield "fill 60 24 60 70 28 60 stone"
        yield "setblock 64 25 60 sea_lantern"
        yield sprintf "function %s:place_signs0" NS
        // make map-update-room
        yield sprintf "fill %s %s sea_lantern hollow" (MAP_UPDATE_ROOM.Offset(-3,-2,-3).STR) (MAP_UPDATE_ROOM.Offset(3,3,3).STR)
        yield sprintf "fill %s %s barrier hollow" (MAP_UPDATE_ROOM.Offset(-1,-1,-1).STR) (MAP_UPDATE_ROOM.Offset(1,2,1).STR)
        yield! placeWallSignCmds MAP_UPDATE_ROOM.X (MAP_UPDATE_ROOM.Y+1) (MAP_UPDATE_ROOM.Z-2) "south" "HOLD YOUR MAP" "(it will only" "update if you" "hold it)" null true false
        // make waiting room
        yield sprintf "fill %s %s sea_lantern hollow" (WAITING_ROOM.Offset(-3,-2,-3).STR) (WAITING_ROOM.Offset(3,3,3).STR)
        yield sprintf "fill %s %s barrier hollow" (WAITING_ROOM.Offset(-1,-1,-1).STR) (WAITING_ROOM.Offset(1,2,1).STR)
        yield! placeWallSignCmds WAITING_ROOM.X (WAITING_ROOM.Y+1) (WAITING_ROOM.Z-2) "south" "PLEASE WAIT" "(spawns are" "being" "generated)" null true false
        // horizontal gridlines
        yield sprintf "fill 1 %d 023 121 %d 023 stone" ART_HEIGHT ART_HEIGHT
        yield sprintf "fill 1 %d 047 121 %d 047 stone" ART_HEIGHT ART_HEIGHT
        yield sprintf "fill 1 %d 071 121 %d 071 stone" ART_HEIGHT ART_HEIGHT
        yield sprintf "fill 1 %d 095 121 %d 095 stone" ART_HEIGHT ART_HEIGHT
        yield sprintf "fill 0 %d 119 127 %d 119 stone" ART_HEIGHT ART_HEIGHT
        // vertical gridlines
        yield sprintf "fill 001 %d 0 001 %d 118 stone" ART_HEIGHT ART_HEIGHT
        yield sprintf "fill 025 %d 0 025 %d 118 stone" ART_HEIGHT ART_HEIGHT
        yield sprintf "fill 049 %d 0 049 %d 118 stone" ART_HEIGHT ART_HEIGHT
        yield sprintf "fill 073 %d 0 073 %d 118 stone" ART_HEIGHT ART_HEIGHT
        yield sprintf "fill 097 %d 0 097 %d 118 stone" ART_HEIGHT ART_HEIGHT
        yield sprintf "fill 121 %d 0 127 %d 118 stone" ART_HEIGHT ART_HEIGHT
        // put logo on card bottom
        yield sprintf "fill 0 %d 120 127 %d 127 black_wool" ART_HEIGHT ART_HEIGHT
        yield sprintf """summon area_effect_cloud 0 %d 120 {Duration:2,Tags:["logoaec"]}""" ART_HEIGHT
        for i = 1 to 4 do
            let x = 32*(i-1)
            yield sprintf """execute at @e[tag=logoaec] offset ~%d ~ ~ run setblock ~ ~ ~ minecraft:structure_block{posX:0,posY:0,posZ:0,sizeX:32,sizeY:1,sizeZ:7,mode:"LOAD",name:"test:logo%d"}""" x i
            yield sprintf """execute at @e[tag=logoaec] offset ~%d ~ ~1 run setblock ~ ~ ~ minecraft:redstone_block""" x
        |]
    yield "assign_1_team",[|
        yield "team join red @a"
        yield "scoreboard players add @a Score 0"
        yield "scoreboard players set @a[team=red] teamNum 1"
        yield sprintf "function %s:compute_lockout_goal" NS
        |]
    yield "assign_2_team",[|
        yield "team leave @a"
        for _i = 1 to 20 do
            yield "team join red @r[team=]"
            yield "team join blue @r[team=]"
        yield "scoreboard players add @a Score 0"
        yield "scoreboard players set @a[team=red] teamNum 1"
        yield "scoreboard players set @a[team=blue] teamNum 2"
        yield sprintf "function %s:compute_lockout_goal" NS
        |]
    yield "assign_3_team",[|
        yield "team leave @a"
        for _i = 1 to 13 do
            yield "team join red @r[team=]"
            yield "team join blue @r[team=]"
            yield "team join green @r[team=]"
        yield "scoreboard players add @a Score 0"
        yield "scoreboard players set @a[team=red] teamNum 1"
        yield "scoreboard players set @a[team=blue] teamNum 2"
        yield "scoreboard players set @a[team=green] teamNum 3"
        yield sprintf "function %s:compute_lockout_goal" NS
        |]
    yield "assign_4_team",[|
        yield "team leave @a"
        for _i = 1 to 10 do
            yield "team join red @r[team=]"
            yield "team join blue @r[team=]"
            yield "team join green @r[team=]"
            yield "team join yellow @r[team=]"
        yield "scoreboard players add @a Score 0"
        yield "scoreboard players set @a[team=red] teamNum 1"
        yield "scoreboard players set @a[team=blue] teamNum 2"
        yield "scoreboard players set @a[team=green] teamNum 3"
        yield "scoreboard players set @a[team=yellow] teamNum 4"
        yield sprintf "function %s:compute_lockout_goal" NS
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
            "scoreboard players set @a[team=red] teamNum 1"
            "scoreboard players set @a[team=blue] teamNum 2"
            "scoreboard players set @a[team=green] teamNum 3"
            "scoreboard players set @a[team=yellow] teamNum 4"
            sprintf "function %s:compute_lockout_goal" NS
            |]
    yield "ensure_maps",[|   // called when player @s currently has no bingo cards
        "scoreboard players add @s ticksSinceGotMap 1"
        """execute if entity @s[scores={ticksSinceGotMap=40..}] run give @s minecraft:filled_map{display:{Name:"BINGO Card"},map:0} 32"""
        "scoreboard players set @s[scores={ticksSinceGotMap=40..}] ticksSinceGotMap 0"
        |]
    yield "choose_seed",[|
        yield "scoreboard players set @a PlayerSeed -1"
        yield "scoreboard players enable @a PlayerSeed"
        yield """tellraw @a {"text":"Press 't' (chat), click below, then replace NNN with a seed number in chat"}"""
        yield """tellraw @a {"text":"CLICK HERE","clickEvent":{"action":"suggest_command","value":"/trigger PlayerSeed set NNN"}}"""
        |]
    yield "set_seed",[| // theloop listens for changes to PlayerSeed to call this as the player
        // special value of '0' means 'repeat the current seed'
        yield "execute if entity @s[scores={PlayerSeed=1..}] run scoreboard players operation Seed Score = @s PlayerSeed"
        yield "scoreboard players operation Z Calc = Seed Score"
        yield "scoreboard players set @a PlayerSeed -1"
        yield sprintf "function %s:new_card_coda" NS
        |]
    yield "choose_random_seed",[|
        // interject actual randomness, rather than deterministic pseudo
        yield "kill @e[tag=aec]"
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
        yield sprintf "execute if entity $SCORE(gameInProgress=0) run function %s:place_signs0" NS
        yield sprintf "function %s:cardgen_makecard" NS
        yield sprintf "function %s:compute_lockout_goal" NS
        |]
    let COLOR = """"color":"yellow","""
    yield "at25mins",[|
        """execute at @s run playsound block.note.harp ambient @s ~ ~ ~ 1 0.6"""
        sprintf """tellraw @a [%s,{"selector":"@s"}," got ",{"score":{"name":"@s","objective":"Score"}}," in 25 mins"]""" LEADING_WHITESPACE 
        """scoreboard players set $ENTITY said25mins 1"""
        |]
    yield "update_time",[|
        "execute store result score $ENTITY minutes run worldborder get"
        "scoreboard players operation $ENTITY minutes -= $ENTITY TWENTY_MIL"
        "scoreboard players operation Time Score = $ENTITY minutes"  // the reason to keep this is that chat text from getting items covers the actionbar, so want a way its visible
        // while 'minutes' objective has 'total seconds', do this
        sprintf """execute if entity $SCORE(said25mins=0,minutes=1500) as @a run function %s:at25mins""" NS
        // compute actual MM:SS and display
        "scoreboard players operation $ENTITY seconds = $ENTITY minutes"
        "scoreboard players operation $ENTITY minutes /= $ENTITY SIXTY"
        "scoreboard players operation $ENTITY seconds %= $ENTITY SIXTY"
        "scoreboard players reset $ENTITY preseconds"
        "execute if entity $SCORE(seconds=0..9) run scoreboard players set $ENTITY preseconds 0"
        sprintf """execute as $ENTITY run title @a actionbar [%s,%s,{%s"score":{"name":"@s","objective":"minutes"}},{%s"text":":"},{%s"score":{"name":"@s","objective":"preseconds"}},{%s"score":{"name":"@s","objective":"seconds"}}]""" LEADING_WHITESPACE LEADING_WHITESPACE COLOR COLOR COLOR COLOR
        |]
    let SKYBOX = 150 // height of skybox floor
    yield "compute_height", [|
        yield sprintf "execute as @e[tag=CurrentSpawn] at @s run teleport @s ~ %d ~" (SKYBOX-3)
        for _i = 1 to (SKYBOX-3) do
            yield "execute as @e[tag=CurrentSpawn] at @s if block ~ ~-1 ~ minecraft:air run teleport @s ~ ~-1 ~"
        yield "execute as @e[tag=CurrentSpawn] at @s run setblock ~ ~-1 ~ minecraft:bedrock"
        |]
    for t in TEAMS do
        yield sprintf "do_%s_spawn" t, [|
            yield sprintf "function %s:prng" NS
            yield sprintf "scoreboard players operation $ENTITY %sSpawnX = $ENTITY PRNG_OUT" t
            yield sprintf "scoreboard players add $ENTITY %sSpawnX 1" t
            yield sprintf "function %s:prng" NS
            yield sprintf "scoreboard players operation $ENTITY %sSpawnZ = $ENTITY PRNG_OUT" t
            yield sprintf "scoreboard players add $ENTITY %sSpawnZ 1" t
            yield sprintf """summon armor_stand 1 1 1 {Invulnerable:1b,Invisible:1b,NoGravity:1b,Tags:["%sSpawn","CurrentSpawn","SpawnLoc"]}""" t  // these entities killed at end of start4
            yield sprintf "execute as @e[tag=%sSpawn] store result entity @s Pos[0] double 10000.0 run scoreboard players get $ENTITY %sSpawnX" t t
            yield sprintf "execute as @e[tag=%sSpawn] store success entity @s Pos[1] double %d.0 run scoreboard players get $ENTITY %sSpawnX" t (SKYBOX+10) t
            yield sprintf "execute as @e[tag=%sSpawn] store result entity @s Pos[2] double 10000.0 run scoreboard players get $ENTITY %sSpawnZ" t t
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run teleport @a[team=%s] ~0.5 ~ ~0.5" t t
            // now that players are there, wait for some terrain to gen
            yield "$NTICKSLATER(20)"
            // build skybox and put players there
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run fill ~-1 %d ~-1 ~1 %d ~1 barrier hollow" t SKYBOX (SKYBOX+20)
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run teleport @a[team=%s] ~0.5 ~ ~0.5" t t
            // figure out Y height of surface
            yield sprintf "function %s:compute_height" NS
            yield sprintf "execute as @e[tag=%sSpawn] store result score $ENTITY %sSpawnY run data get entity @s Pos[1] 1.0" t t
            // give people time in skybox while terrain gens, then put them on ground and set spawns
            yield sprintf """tellraw @a ["Giving %s team a birds-eye view as terrain generates..."]""" t
            yield "$NTICKSLATER(400)"
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run teleport @a[team=%s] ~0.5 ~ ~0.5" t t
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run spawnpoint @a[team=%s] ~0.5 ~ ~0.5" t t
            // place beacon to mark spawn
            yield "execute as @e[tag=CurrentSpawn] at @s offset ~ ~10 ~ run fill ~-2 ~0 ~-2 ~2 ~3 ~2 minecraft:barrier hollow"
            yield "execute as @e[tag=CurrentSpawn] at @s offset ~ ~10 ~ run fill ~-1 ~1 ~-1 ~1 ~1 ~1 minecraft:diamond_block"
            yield "execute as @e[tag=CurrentSpawn] at @s offset ~ ~10 ~ run setblock ~ ~2 ~ minecraft:beacon"
            yield sprintf "tag @e[tag=CurrentSpawn] remove CurrentSpawn"
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
            yield sprintf "scoreboard players set $ENTITY %sLeftHalf 0" t
            yield sprintf "scoreboard players set $ENTITY %sRightHalf 0" t
        yield "scoreboard players set $ENTITY TEMP 0"   // has 'Left' been taken yet?
        for t in TEAMS do
            yield sprintf "execute if entity $SCORE(numActiveTeams=2,TEMP=1) if entity @a[team=%s] run scoreboard players set $ENTITY %sRightHalf 1" t t
            yield sprintf "execute if entity $SCORE(numActiveTeams=2,TEMP=0) if entity @a[team=%s] run scoreboard players set $ENTITY %sLeftHalf 1" t t
            yield sprintf "execute if entity $SCORE(numActiveTeams=2,TEMP=0) if entity @a[team=%s] run scoreboard players set $ENTITY TEMP 1" t
        |]
    yield "start1", [|
        // ensure folks have joined teams
        yield sprintf "function %s:compute_active_teams" NS
        yield """execute if entity $SCORE(numActiveTeams=0) run tellraw @a ["No one has joined a team - join a team color to play!"]"""
        yield sprintf "execute if entity $SCORE(numActiveTeams=1..) run function %s:start2" NS
        |]
    yield "reset_player_scores",[|
        yield "scoreboard players operation $ENTITY Seed = Seed Score"  // save seed
        yield "scoreboard players reset * Score"  // TODO fails due to bug https://bugs.mojang.com/browse/MC-122993
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
        // note game in progress
        yield "scoreboard players set $ENTITY gameInProgress 1"
        yield sprintf "function %s:place_signs1" NS
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
        // give maps in offhand for start of game
        yield """replaceitem entity @a weapon.offhand minecraft:filled_map{display:{Name:"BINGO Card"},map:0} 32""" // unused: way to test offhand non-empty - scoreboard players set @p[nbt={Inventory:[{Slot:-106b}]}] offhandFull 1
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
        // tp all to waiting room
        yield sprintf "tp @a %s 0 180" WAITING_ROOM.TPSTR
        // set up spawn points
        yield "scoreboard players set $ENTITY PRNG_MOD 998"
        yield sprintf "execute if entity @a[team=red] run function %s:do_red_spawn" NS
        yield sprintf "execute if entity @a[team=blue] unless entity @a[team=red] run function %s:do_blue_spawn" NS
        yield sprintf "execute if entity @a[team=green] unless entity @a[team=red] unless entity @a[team=blue] run function %s:do_green_spawn" NS
        yield sprintf "execute if entity @a[team=yellow] unless entity @a[team=red] unless entity @a[team=blue] unless entity @a[team=green] run function %s:do_yellow_spawn" NS
        |]
    yield "start4", [|
        yield "gamemode survival @a"
        // feed & heal again
        yield "effect give @a saturation 10 4 true"
        yield "effect give @a regeneration 10 4 true"
        // clear hostile mobs & weather
        yield "difficulty peaceful"
        yield "weather clear 99999"
        yield sprintf """tellraw @a [%s,"Game will begin shortly..."]""" LEADING_WHITESPACE
        yield "$NTICKSLATER(20)"
        yield sprintf """tellraw @a [%s,"3"]""" LEADING_WHITESPACE
        yield "execute as @a at @s run playsound block.note.harp ambient @s ~ ~ ~ 1 0.6"
        yield "$NTICKSLATER(20)"
        yield sprintf """tellraw @a [%s,"2"]""" LEADING_WHITESPACE
        yield "execute as @a at @s run playsound block.note.harp ambient @s ~ ~ ~ 1 0.6"
        yield "$NTICKSLATER(20)"
        yield sprintf """tellraw @a [%s,"1"]""" LEADING_WHITESPACE
        yield "execute as @a at @s run playsound block.note.harp ambient @s ~ ~ ~ 1 0.6"
        yield "$NTICKSLATER(20)"
        // once more, re-tp anyone who maybe moved, the cheaters!
        for t in TEAMS do
            yield sprintf "execute at @e[tag=%sSpawn] run teleport @a[team=%s] ~0.5 ~ ~0.5" t t
        yield "kill @e[tag=SpawnLoc]"
        yield "difficulty normal"
        yield sprintf "function %s:start5" NS
        |]
    yield "start5", [|
        yield "time set 0"
        yield "effect clear @a"
        // TODO custom game modes, for now, just always NV+DS
        yield "effect give @a minecraft:night_vision 99999 1 true"
        yield "replaceitem entity @a armor.feet minecraft:leather_boots{Unbreakable:1,ench:[{lvl:3s,id:8s},{lvl:1s,id:71s}]} 1"
        yield sprintf """tellraw @a [%s,"Start! Go!!!"]""" LEADING_WHITESPACE
        yield "execute as @a at @s run playsound block.note.harp ambient @s ~ ~ ~ 1 1.2"
        // enable triggers (for click-in-chat-to-tp-home stuff)
        yield "scoreboard players set @a home 0"
        yield "scoreboard players enable @a home"
        // option to get back
        yield """execute if entity $SCORE(handHolding=1) run tellraw @a ["(If you need to quit before getting BINGO, you can"]"""
        yield """execute if entity $SCORE(handHolding=1) run tellraw @a [{"underlined":"true","text":"press 't' (chat), then click this line to return to the lobby)","clickEvent":{"action":"run_command","value":"/trigger home set 1"}}]"""
        yield sprintf """execute if entity $SCORE(handHolding=0) run tellraw @a [%s,{"underlined":"true","text":"click to go to lobby","clickEvent":{"action":"run_command","value":"/trigger home set 1"}}]""" LEADING_WHITESPACE
        yield "worldborder set 20000000"          // 20 million wide is 10 million from spawn
        yield "worldborder add 10000000 10000000" // 10 million per 10 million seconds is one per second
        yield "scoreboard players set $ENTITY gameInProgress 2"
        yield sprintf "function %s:place_signs2" NS
        yield "scoreboard players set $ENTITY hasAnyoneUpdated 0"
        yield "execute if entity $SCORE(handHolding=0) run scoreboard players set $ENTITY hasAnyoneUpdated 1"
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
        sprintf "function %s:place_signs0" NS
        sprintf "teleport @a %s" LOBBY
        "gamemode survival @a"
        "clear @a"
        // feed & heal, as people get concerned in lobby about this
        "effect give @a minecraft:saturation 10 4 true"
        "effect give @a minecraft:regeneration 10 4 true"
        |]
    |]

///////////////////////////////////////////////////////

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
        "kill @e[type=item,nbt={Item:{id:\"minecraft:filled_map\",tag:{map:0}}}]"  // TODO is this super expensive? line above too... could try inverting it, e.g. only find maps near players, to cull chunks searched?
        // run progress for anyone in the update room
        sprintf "execute as @a[scores={ticksLeftMU=1}] run function %s:warp_back" NS
        "scoreboard players remove @a[scores={ticksLeftMU=1..}] ticksLeftMU 1"
        |]
    yield "warp_home", [|
        // players may have just died this tick
        "execute store result score @s TEMP run data get entity @s Health 100.0"
        sprintf "execute if entity @s[scores={TEMP=1..}] run function %s:warp_home_body" NS
        |]
    yield "warp_home_body", [|
        "scoreboard players set @s ticksLeftMU 30"  // TODO calibrate best value
        "execute store result score @s ReturnX run data get entity @s Pos[0] 128.0"   // doubles
        "execute store result score @s ReturnY run data get entity @s Pos[1] 128.0"
        "execute store result score @s ReturnZ run data get entity @s Pos[2] 128.0"
        "execute store result score @s ReturnRotX run data get entity @s Rotation[0] 8.0"   // floats
        "execute store result score @s ReturnRotY run data get entity @s Rotation[1] 8.0"
        sprintf """tellraw @a [%s,{"selector":"@s"}," is updating the BINGO map"]""" LEADING_WHITESPACE
        //"data merge entity @e[type=!player,distance=..160] {PersistenceRequired:1}"  // preserve mobs
        "execute as @e[type=!player,distance=..160] run data merge entity @s {PersistenceRequired:1}"  // preserve mobs
        // TODO ever a reason to un-persist?
        sprintf "tp @s %s 180 180" MAP_UPDATE_ROOM.TPSTR
        //TODO "execute at @s run particle portal ~ ~ ~ 3 2 3 1 99 @s"
        "execute at @s run playsound entity.endermen.teleport ambient @a"
        |]
    yield "warp_back", [|
        "kill @e[tag=return_loc]"
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
        |]
    |]

////////////////////////////////////////////////

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

let bingoItems = [|
        [|  "diamond"          ; "diamond_hoe"      ; "diamond_axe"         |]
        [|  "bone"             ; "arrow"            ; "gray_dye"            |]
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
        [|  "rail"             ; "activator_rail"   ; "detector_rail"       |]
        [|  "mushroom_stew"    ; "mushroom_stew"    ; "mushroom_stew"       |]
        [|  "sugar"            ; "spider_eye"       ; "fermented_spider_eye"|]
        [|  "cactus_green"     ; "cactus_green"     ; "lime_dye"            |]
        [|  "lapis_lazuli"     ; "purple_dye"       ; "cyan_dye"            |]
        [|  "beetroot_soup"    ; "emerald"          ; "emerald"             |]
        [|  "furnace_minecart" ; "chest_minecart"   ; "tnt_minecart"        |]
        [|  "gunpowder"        ; "firework_rocket"  ; "firework_rocket"     |]
        [|  "compass"          ; "compass"          ; "map"                 |]
        [|  "spruce_sapling"   ; "spruce_sapling"   ; "acacia_sapling"      |]
        [|  "cauldron"         ; "lava_bucket"      ; "lava_bucket"         |]
        [|  "name_tag"         ; "saddle"           ; "enchanted_book"      |]
        [|  "milk_bucket"      ; "egg"              ; "cake"                |]
        [|  "cod"              ; "cod"              ; "cod"                 |]
        [|  "sign"             ; "item_frame"       ; "painting"            |]
        [|  "golden_sword"     ; "clock"            ; "powered_rail"        |]
        [|  "hopper"           ; "hopper"           ; "hopper_minecart"     |]
        [|  "redstone_torch"   ; "repeater"         ; "repeater"            |]
    |]

let flatBingoItems = 
    let orig = [|
        for a in bingoItems do
            for x in a do
                yield x
        |]
    let trim = // remove duplicates, preserve order
        let r = ResizeArray()
        for x in orig do
            if not(r.Contains(x)) then
                r.Add(x)
        r
    trim.ToArray()

///////////////////////////////////////////////////////////////////////////////

let SQUARES = [| for i = 1 to 5 do for j = 1 to 5 do yield sprintf "%d%d" i j |]
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
                yield sprintf "execute if entity $SCORE(%sCanGet%s=1) run function %s:inv/check%s" t s NS s
                yield sprintf "execute if entity $SCORE(gotAnItem=1) run function %s:got/%s_got_square_%s" NS t s
            yield sprintf """execute if entity $SCORE(gotItems=1,hasAnyoneUpdated=0,announceItem=1..) run tellraw @s ["To update the BINGO map, drop one copy on the ground"]"""
            yield "scoreboard players operation $ENTITY teamNum = @s teamNum"
            yield sprintf "execute if entity $SCORE(gotItems=1,fireworkItem=1,announceOnlyTeam=0) as @a at @s run playsound entity.firework.launch ambient @s ~ ~ ~"
            yield sprintf "execute if entity $SCORE(gotItems=1,fireworkItem=1,announceOnlyTeam=1) as @a if score @s teamNum = $ENTITY teamNum at @s run playsound entity.firework.launch ambient @s ~ ~ ~"
            yield sprintf "execute if entity $SCORE(gotItems=1) run function %s:%s_check_for_win" NS t
            |]
        for s in SQUARES do
            yield sprintf "got/%s_got_square_%s" t s, [|       // called when player @s got square s and he is on team t
                yield sprintf "scoreboard players set $ENTITY gotItems 1"
                yield sprintf "scoreboard players add $ENTITY %sScore 1" t
                yield sprintf "scoreboard players operation @a[team=%s] Score = $ENTITY %sScore" t t
                yield sprintf "scoreboard players set $ENTITY %sCanGet%s 0" t s
                for ot in TEAMS do
                    if ot <> t then
                        yield sprintf "execute if entity $SCORE(isLockout=1) run scoreboard players set $ENTITY %sCanGet%s 0" ot s
                // TODO test actual logic to color the game board square appropriately (e.g. lockout)
                let x = 2 + 24*(int s.[0] - int '0' - 1)
                let y = ART_HEIGHT
                let z = 0 + 24*(int s.[1] - int '0' - 1)
                // determine if we should fill the whole square
                yield sprintf "scoreboard players set $ENTITY TEMP 0"
                yield sprintf "execute if entity $SCORE(numActiveTeams=1) run scoreboard players set $ENTITY TEMP 1"
                yield sprintf "execute if entity $SCORE(isLockout=1) run scoreboard players set $ENTITY TEMP 1"
                yield sprintf "execute if entity $SCORE(TEMP=1) run fill %d %d %d %d %d %d %s replace clay" x y z (x+22) y (z+22) (if t="green" then "emerald_block" else t+"_wool")
                // else if 2 active teams, fill the half
                yield sprintf "execute if entity $SCORE(numActiveTeams=2,%sLeftHalf=1) run fill %d %d %d %d %d %d %s replace clay" t (x+00) y (z+00) (x+11) y (z+22) (if t="green" then "emerald_block" else t+"_wool")
                yield sprintf "execute if entity $SCORE(numActiveTeams=2,%sRightHalf=1) run fill %d %d %d %d %d %d %s replace clay" t (x+12) y (z+00) (x+22) y (z+22) (if t="green" then "emerald_block" else t+"_wool")
                yield sprintf "execute if entity $SCORE(numActiveTeams=2) run scoreboard players set $ENTITY TEMP 1"
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
            yield sprintf """execute if entity $SCORE(TEMP=1,%sGotBingo=0) run tellraw @a [%s,{"selector":"@a[team=%s]"}," got BINGO!"]""" t LEADING_WHITESPACE t
            yield sprintf "execute if entity $SCORE(TEMP=1,%sGotBingo=0) run function %s:got_a_win_common_logic" t NS
            yield sprintf "execute if entity $SCORE(TEMP=1,%sGotBingo=0) run scoreboard players set $ENTITY %sGotBingo 1" t t
            // check for twenty-no-bingo
            yield sprintf """execute if entity $SCORE(%sScore=20,%sGotBingo=0) run tellraw @a [%s,{"selector":"@a[team=%s]"}," got TWENTY-NO-BINGO!"]""" t t LEADING_WHITESPACE t
            yield sprintf "execute if entity $SCORE(%sScore=20,%sGotBingo=0) run function %s:got_a_win_common_logic" t t NS
            // check for blackout
            yield sprintf """execute if entity $SCORE(%sScore=25) run tellraw @a [%s,{"selector":"@a[team=%s]"}," got MEGA-BINGO!"]""" t LEADING_WHITESPACE t
            yield sprintf "execute if entity $SCORE(%sScore=25) run function %s:got_a_win_common_logic" t NS
            // check for lockout
            yield sprintf "scoreboard players operation $ENTITY TEMP = $ENTITY lockoutGoal"
            yield sprintf "scoreboard players operation $ENTITY TEMP -= $ENTITY %sScore" t
            yield sprintf """execute if entity $SCORE(isLockout=1,TEMP=0) run tellraw @a [%s,{"selector":"@a[team=%s]"}," got the lockout goal!"]""" LEADING_WHITESPACE t
            yield sprintf "execute if entity $SCORE(isLockout=1,TEMP=0) run function %s:got_a_win_common_logic" NS
            |]
    yield "got_a_win_common_logic", [|
        // put time on scoreboard
        yield "scoreboard players operation Minutes Score = $ENTITY minutes"
        yield "scoreboard players operation Seconds Score = $ENTITY seconds"
        // option to return to lobby
        yield """execute if entity $SCORE(handHolding=1) run tellraw @a ["You can keep playing, or"]"""
        yield """execute if entity $SCORE(handHolding=1) run tellraw @a [{"underlined":"true","text":"press 't' (chat), then click this line to return to the lobby","clickEvent":{"action":"run_command","value":"/trigger home set 1"}}]"""
        yield sprintf """execute if entity $SCORE(handHolding=0) run tellraw @a [%s,{"underlined":"true","text":"click to go to lobby","clickEvent":{"action":"run_command","value":"/trigger home set 1"}}]""" LEADING_WHITESPACE
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
        if flatBingoItems.Length >= 128 then
            failwith "bad binary search"
        let check_and_display(prefix, n, name) = [|
            yield sprintf """%sexecute if entity $SCORE(square%s=%d) store success score $ENTITY gotAnItem run clear @s %s 1""" prefix s n name
            // Note - profiling suggests this guard does not help: if entity @s[nbt={Inventory:[{id:"minecraft:%s"}]}] 
            yield sprintf """%sexecute if entity $SCORE(square%s=%d,gotAnItem=1,announceItem=2,announceOnlyTeam=0) run tellraw @a [%s,"%s ",{"color":"gray","score":{"name":"@e[%s]","objective":"minutes"}},{"color":"gray","text":":"},{"color":"gray","score":{"name":"@e[%s]","objective":"preseconds"}},{"color":"gray","score":{"name":"@e[%s]","objective":"seconds"}}," ",{"selector":"@s"}]""" prefix s n LEADING_WHITESPACE name ENTITY_TAG ENTITY_TAG ENTITY_TAG
            yield sprintf """%sexecute if entity $SCORE(square%s=%d,gotAnItem=1,announceItem=1,announceOnlyTeam=0) run tellraw @a [%s,{"color":"gray","score":{"name":"@e[%s]","objective":"minutes"}},{"color":"gray","text":":"},{"color":"gray","score":{"name":"@e[%s]","objective":"preseconds"}},{"color":"gray","score":{"name":"@e[%s]","objective":"seconds"}}," ",{"selector":"@s"}," got an item!"]""" prefix s n LEADING_WHITESPACE ENTITY_TAG ENTITY_TAG ENTITY_TAG
            yield "scoreboard players operation $ENTITY teamNum = @s teamNum"
            yield sprintf """%sexecute if entity $SCORE(square%s=%d,gotAnItem=1,announceItem=2,announceOnlyTeam=1) run execute as @a if score @s teamNum = $ENTITY teamNum run tellraw @s [%s,"%s ",{"color":"gray","score":{"name":"@e[%s]","objective":"minutes"}},{"color":"gray","text":":"},{"color":"gray","score":{"name":"@e[%s]","objective":"preseconds"}},{"color":"gray","score":{"name":"@e[%s]","objective":"seconds"}}," ",{"selector":"@s"}]""" prefix s n LEADING_WHITESPACE name ENTITY_TAG ENTITY_TAG ENTITY_TAG
            yield sprintf """%sexecute if entity $SCORE(square%s=%d,gotAnItem=1,announceItem=1,announceOnlyTeam=1) run execute as @a if score @s teamNum = $ENTITY teamNum run tellraw @s [%s,{"color":"gray","score":{"name":"@e[%s]","objective":"minutes"}},{"color":"gray","text":":"},{"color":"gray","score":{"name":"@e[%s]","objective":"preseconds"}},{"color":"gray","score":{"name":"@e[%s]","objective":"seconds"}}," ",{"selector":"@s"}," got an item!"]""" prefix s n LEADING_WHITESPACE ENTITY_TAG ENTITY_TAG ENTITY_TAG
            |]
        let rec binary_dispatch(lo,hi) = [|
            if lo=hi-1 then
                yield sprintf "inv/check%s_%d_%d" s lo hi, [|
                    let loName,loPre = if lo < flatBingoItems.Length then flatBingoItems.[lo],"" else (sprintf "item%d" lo),"#"
                    yield! check_and_display(loPre,lo,loName)
                    let hiName,hiPre = if hi < flatBingoItems.Length then flatBingoItems.[hi],"" else (sprintf "item%d" hi),"#"
                    yield! check_and_display(hiPre,hi,hiName)
                |]
            else
                let mid = (hi-lo)/2 + lo
                yield sprintf "inv/check%s_%d_%d" s lo hi, [|
                    sprintf """execute if entity $SCORE(square%s=%d..%d) run function %s:inv/check%s_%d_%d""" s lo mid NS s lo mid
                    sprintf """execute if entity $SCORE(square%s=%d..%d) run function %s:inv/check%s_%d_%d""" s (mid+1) hi NS s (mid+1) hi
                |]
                yield! binary_dispatch(lo,mid)
                yield! binary_dispatch(mid+1,hi)
            |]
        yield! binary_dispatch(0,127)
        yield sprintf "inv/check%s" s, [|
            sprintf "function %s:inv/check%s_0_127" NS s
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
    // TODO consider different presentation, since anyDifficulty is almost becoming empty, and 'difficulty' is subjective... e.g. just show all the 'bins'
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
        yield sprintf "scoreboard players set $ENTITY PRNG_MOD %d" bingoItems.Length 
        yield sprintf "function %s:prng" NS
        yield sprintf "scoreboard players operation $ENTITY CARDGENTEMP = $ENTITY PRNG_OUT"
        // ensure exactly one call
        yield sprintf "scoreboard players set $ENTITY CALL 1"
        for i = 0 to bingoItems.Length-1 do
            yield sprintf "execute if entity $SCORE(CARDGENTEMP=%d,CALL=1) run function %s:cg/cardgen_bin%02d" i NS i
    |]
    for i = 0 to bingoItems.Length-1 do
        yield sprintf "cg/cardgen_bin%02d" i, [|
            sprintf "scoreboard players set $ENTITY CALL 0" // every exclusive-callable func needs this as first line of code
            sprintf "execute if entity $SCORE(bin%02d=1) run function %s:cardgen_choose1" i NS
            sprintf "execute unless entity $SCORE(bin%02d=1) run function %s:cg/cardgen_binbody%02d" i NS i
            |]
    for i = 0 to bingoItems.Length-1 do
        yield sprintf "cg/cardgen_binbody%02d" i, [|
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
                        let chest = if y < 4 then "59 25 63" else "59 25 62"
                        let slot = if y < 4 then (y-1)*9+x-1 else (y-4)*9+x-1
                        yield sprintf """execute if entity $SCORE(PRNG_OUT=%d,squaresPlaced=%d) run replaceitem block %s container.%d %s""" j (5*(y-1)+x) chest slot bingoItems.[i].[j]
            |]
    yield "cardgen_makecard", [|
        yield sprintf "kill @e[tag=sky]"
        yield sprintf """summon armor_stand 5 %d 3 {Tags:["sky"],NoGravity:1,Invulnerable:1,Invisible:1}""" ART_HEIGHT
        yield sprintf "scoreboard players set $ENTITY squaresPlaced 0"
        for i = 0 to bingoItems.Length-1 do
            yield sprintf "scoreboard players set $ENTITY bin%02d 0" i
        // refresh bingo art area
        yield sprintf "fill 0 %d -1 127 %d 118 clay" ART_HEIGHT ART_HEIGHT
        yield sprintf "fill 0 %d -1 127 %d 118 air" (ART_HEIGHT+1) (ART_HEIGHT+1)
        // chest of items on this card
        yield "setblock 59 25 62 air"
        yield "setblock 59 25 63 air"
        yield "setblock 59 25 62 chest[facing=east,type=left]"
        yield "setblock 59 25 63 chest[facing=east,type=right]"
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
            // players may dead
            "execute store result score @s TEMP run data get entity @s Health 100.0"
            "execute if entity @s[scores={TEMP=1..}] run effect give @s minecraft:night_vision 99999 1 true"
            "execute if entity @s[scores={TEMP=1..}] run replaceitem entity @s armor.feet minecraft:leather_boots{Unbreakable:1,ench:[{lvl:3s,id:8s},{lvl:1s,id:71s}]} 1"
            """execute if entity @s[scores={TEMP=1..}] run replaceitem entity @s weapon.offhand minecraft:filled_map{display:{Name:"BINGO Card"},map:0} 32""" // unused: way to test offhand non-empty - scoreboard players set @p[nbt={Inventory:[{Slot:-106b}]}] offhandFull 1
            "execute if entity @s[scores={TEMP=1..}] run scoreboard players set @s Deaths 0"
            |],"on_respawn")
        // TODO putting this in a separate function is a work around for https://bugs.mojang.com/browse/MC-121934
        yield! compile([|
            // players may dead
            "execute store result score @s TEMP run data get entity @s Health 100.0"
            sprintf """execute if entity @s[scores={TEMP=1..}] unless entity @s[nbt={Inventory:[{id:"minecraft:filled_map",tag:{map:0}}]}] run function %s:ensure_maps""" NS
            |],"have_no_map")
        yield! compile([|
            "tag @s add playerHasBeenSeen"
            sprintf "teleport @s %s" LOBBY
            "effect give @s minecraft:night_vision 99999 1 true"
            "recipe give @s *"
            "advancement grant @s everything"
            |],"first_time_player")
        yield! compile([|
            yield sprintf "execute if entity $SCORE(gameInProgress=2) run function %s:update_time" NS
            yield sprintf "execute if entity $SCORE(gameInProgress=2) run function %s:map_update_tick" NS
            yield sprintf "execute as @a[scores={home=1}] run function %s:go_home" NS
            yield sprintf "execute as @a[tag=!playerHasBeenSeen] run function %s:first_time_player" NS
            yield sprintf "execute unless entity $SCORE(gameInProgress=1) as @p[scores={PlayerSeed=0..}] run function %s:set_seed" NS
            yield sprintf "execute unless entity $SCORE(gameInProgress=1) as @a run function %s:have_no_map" NS
            yield sprintf "execute unless entity $SCORE(gameInProgress=1) as @a[scores={Deaths=1..}] run function %s:on_respawn" NS
            yield sprintf "execute if entity $SCORE(gameInProgress=2) as @a run function %s:check_inventory" NS
            yield! gameLoopContinuationCheck()
            |],"theloop")
        yield! compile(prng, "prng")
        yield! compile(prng_init(), "prng_init")
        yield makeItemChests()
        yield "init",[|
            yield "kill @e[type=!player]"
            yield "clear @a"
            yield "effect clear @a"
            yield! entity_init()
            yield sprintf"function %s:prng_init"NS
            yield sprintf"function %s:checker_init"NS
            yield sprintf"function %s:cardgen_init"NS
            yield sprintf"function %s:game_init"NS
            yield sprintf"function %s:map_update_init"NS
            yield sprintf"function %s:make_lobby"NS
            yield sprintf"function %s:choose_random_seed"NS
            yield """give @p minecraft:filled_map{display:{Name:"BINGO Card"},map:0} 32"""
            |]
        |]
    printfn "writing functions..."
    for name,code in r do
        if SKIP_WRITING_CHECK && System.Text.RegularExpressions.Regex.IsMatch(name,"""check\d\d_.*""") then
            () // do nothing
        else
            writeFunctionToDisk(name,code)
