module MinecraftBINGO

let PROFILE = false            // turn on to log how many commands (lines) run each tick

// TODO decide lobby interface regarding choosable items (item frame wall), have sign to 'default'/all, etc (maybe wall appears when gip=0 and disappears when start game?)
// TODO deal with too few items (can keep own counter and 'give up' after n, so ensure consistent game state?)

// TODO oh yeah, nether is buggy
// TODO arrow to spawn while in nether (remove? point to entry portal?)
// TODO experiment with nether teleports, can I add nether items?
//  - glowstone dust
//  - ghast tear
//  - nether quartz
//  - comparator
//  - daylight sensor
//  - nether brick (item)

// TODO possibly-expensive things could be moved to datapacks, so turning them off will remove all the machinery (e.g. XH advancement)

// TODO may need to re-art everything? https://www.reddit.com/r/Minecraft/comments/7jr4tp/try_the_new_minecraft_java_textures/ (prob not until 1.14)

// TODO tall lobby for building? open ceiling? figure out aesthetic, maybe something that allows others to build-out? if signs are movable, pretty open-ended?

// TODO signs/books explaining gameplay/game modes/custom terrain/custom config
//  - gameplay books, like lockout & 2-for-1-mode should be linked next to options, e.g. 'click for more info'

// TODO config option to give teammates 'glowing' (only when away from lobby? is annoying there?)

// TODO evaluate new items, other new features

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

// TODO command block customization, for people on server without access to files (yeah, e.g. just an on-start and on-respawn ICB outside lobby that get triggered for each player given a tag)

// TODO refactor utility modules into a separate datapack? prng, arrow to X, warp-home/back, ...?

let NS = "test"
let PACK_NAME = "BingoPack"
let CFG = "cfg"
let bingoConfigBookTag = "BingoConfigBook"
type ConfigDescription = Utilities.ConfigDescription
type ConfigBook = Utilities.ConfigBook 
type ConfigPage = Utilities.ConfigPage 
type ConfigOption = Utilities.ConfigOption 
let bingoConfigBook = ConfigBook("Lorgon111","Standard options",[|
    ConfigPage("Starting items and effects",[|
        ConfigOption("optnv",  ConfigDescription.Toggle "Night Vision", 1, [||])
        ConfigOption("optds",  ConfigDescription.Toggle "Depth Strider boots", 0, [||])
        ConfigOption("optboat",ConfigDescription.Toggle "Starting boat", 1, [||])
        // TODO team chest
        // TODO, OOB elytra
        |])
    ConfigPage("Optional game modes",[|
        ConfigOption("optlo", ConfigDescription.Toggle "Lockout mode", 0, [|sprintf "function %s:compute_lockout_goal" NS|])
        ConfigOption("opttfo",ConfigDescription.Toggle "Two-for-one mode", 0, [||])
        |])
    ConfigPage("Other options",[|
        ConfigOption("opthh", ConfigDescription.Toggle "Novice mode (various extra help text)", 1, [||])                 // if we need to explain how to update the card, how to click the chat, etc.
        ConfigOption("optlw", ConfigDescription.Toggle "Leading whitespace", 0, [||])                                    // if we want leading whitespace to keep most chat away from left side
        ConfigOption("optsa", ConfigDescription.Toggle "Arrow points to spawn", 1, [||])                                 // if add an 'arrow' pointing at spawn on player actionbar (may be resource-intensive?)
        ConfigOption("optxh", ConfigDescription.Toggle "Show when extreme hills", 1, [||])                               // if add 'XH' on player actionbar when in extreme hills
        |])
    ConfigPage("Got-an-item effects for your team",[|
        ConfigOption("optyai", ConfigDescription.Radio [|"your teammates see <nothing> in chat"; "your teammates see 'got an item' in chat"; "your teammates see 'got bone' in chat"|], 2, [||])
        ConfigOption("optyfi",ConfigDescription.Toggle "your teammates hear firework sound", 1, [||])
        |])
    ConfigPage("Got-an-item effects for other teams",[|
        ConfigOption("optoai", ConfigDescription.Radio [|"other teams see <nothing> in chat"; "other teams see 'got an item' in chat"; "other teams see 'got bone' in chat"|], 1, [||])
        ConfigOption("optofi",ConfigDescription.Toggle "other teams hear firework sound", 1, [||])
        |])
    ConfigPage("Other options",[|
        ConfigOption("optus",ConfigDescription.Toggle "Turn on utility signs", 1, [||])  // TODO make this default 0 in release version
        |])
    |])

// TODO Maybe put the card's seed on the right margin, so a screenshot of the card without the rest of the UI includes the seed number for that card?﻿ 
// TODO display config options on card sidebar? or swap sidebar and logo (logo on side, extra on bottom?)

// TODO consider https://www.reddit.com/r/minecraftbingo/comments/7j1afe/some_ideas_for_40/

// TODO non-default option to enable another 'arrow' that points to your prior death location

/////////////////////////////////////////////////////////////

type Coords(x:int,y:int,z:int) = 
    member this.X = x
    member this.Y = y
    member this.Z = z
    member this.Tuple = x,y,z
    member this.STR = sprintf "%d %d %d" x y z
    member this.TPSTR = sprintf "%d %d.0 %d" x y z   // not specifying a decimal adds 0.5 to each value; when tp'ing, we typically want y to be floor-flush and xz to be centered, hence this
    member this.Offset(dx,dy,dz) = new Coords(x+dx, y+dy, z+dz)

let MAP_UPDATE_ROOM = Coords(62,10,72)
let WAITING_ROOM = Coords(71,10,72)
let ART_HEIGHT = 40 // TODO
let ART_HEIGHT_DAYLIGHT_BLOCKER = ART_HEIGHT-1
let ART_HEIGHT_UNDER = ART_HEIGHT-2

let CHEST_THIS_CARD_1 = Coords(59,25,62)
let CHEST_THIS_CARD_2 = Coords(59,25,63)

let SQUARES = [| for i = 1 to 5 do for j = 1 to 5 do yield sprintf "%d%d" i j |]
let TEAMS = [| "red"; "blue"; "green"; "yellow" |]


let FOLDER = System.IO.Path.Combine(Utilities.MC_ROOT, """testing""")
let pack = new Utilities.DataPackArchive(FOLDER, PACK_NAME, "MinecraftBINGO base pack")
let compiler = new Compiler.Compiler('m','b',"test",84,4,4,PROFILE)

////////////////////////////
// hook into events from base pack
pack.WriteFunctionTagsFileWithValues("minecraft", "load", [compiler.LoadFullName;sprintf"%s:init"NS])
pack.WriteFunctionTagsFileWithValues("minecraft", "tick", [compiler.TickFullName;sprintf"%s:theloop"NS])

////////////////////////////
// publish own events for child packs
let publishEvent(eventName) =
    pack.WriteFunctionTagsFileWithValues(NS, eventName, [])
for eventName in ["on_new_card"; "on_start_game"; "on_finish"; "on_get_configuration_books"] do
    publishEvent(eventName)
for t in TEAMS do
    for s in SQUARES do
        publishEvent(sprintf "on_%s_got_square_%s" t s)

////////////////////////////

let writeExtremeHillsDetection(pack:Utilities.DataPackArchive) =
    pack.WriteAdvancement(NS,"xh",sprintf """{
    "criteria": {
        "visit_xh": {"trigger": "minecraft:location","conditions": {"biome": "minecraft:extreme_hills"}},
        "visit_sxh": {"trigger": "minecraft:location","conditions": {"biome": "minecraft:smaller_extreme_hills"}},
        "visit_xht": {"trigger": "minecraft:location","conditions": {"biome": "minecraft:extreme_hills_with_trees"}},
        "visit_mxh": {"trigger": "minecraft:location","conditions": {"biome": "minecraft:mutated_extreme_hills"}},
        "visit_mxht": {"trigger": "minecraft:location","conditions": {"biome": "minecraft:mutated_extreme_hills_with_trees"}}
    },
    "requirements": [
        ["visit_xh", "visit_sxh", "visit_xht", "visit_mxh", "visit_mxht"]
    ],
    "rewards": {
        "function": "%s:on_xh_grant"
    }
}""" NS)
    pack.WriteFunction(NS,"on_xh_grant",[|
        "scoreboard players set @s inXH 21"  // location advancements are granted once per second (every 20 ticks), so a value just high enough to ensure it says above 0 if you stay in XH
        sprintf "advancement revoke @s only %s:xh" NS
        |])

////////////////////////////

let entity_init() = [|
    yield "setworldspawn 64 64 64"
    yield "kill @e[tag=XHguy]"
    yield "kill @e[tag=nonuuidguy]"
    yield "kill @e[tag=LWguy]"
    // TODO idea, for i = 1 to 50 print N spaces followed by 'click', and have folks click the line with the best alignment, and that sets the customname to that many spaces
    // TODO this unicode char '⁚' (8282, \u205A) is apparently one pixel thick, and in light grey fades to nothing? try it?
    yield """summon armor_stand 84 4 84 {CustomName:"\"                          \"",Tags:["LWguy"],NoGravity:1,Marker:1,Invulnerable:1,Invisible:1}"""
    yield """summon armor_stand 4 4 84 {CustomName:"\"XH\"",Tags:["XHguy"],NoGravity:1,Marker:1,Invulnerable:1,Invisible:1}"""
    yield """summon armor_stand 67 4 67 {CustomName:"\"nonuuidguy\"",Tags:["nonuuidguy"],NoGravity:1,Marker:1,Invulnerable:1,Invisible:1}"""
    |]
//let ENTITY_TAG = compiler.EntityTag 
let FAKE = compiler.FakePlayerName 
let NONUUID_TAG = "tag=nonuuidguy,x=67,y=4,z=67,distance=..1.0,limit=1"
let XH_TAG = "tag=XHguy,x=4,y=4,z=84,distance=..1.0,limit=1"
let LW_TAG = "tag=LWguy,x=84,y=4,z=84,distance=..1.0,limit=1"
let LEADING_WHITESPACE = sprintf """{"selector":"@e[%s,scores={optlwval=1}]"}""" LW_TAG
let XH_TEXT = sprintf """{"selector":"@e[%s,scores={inXH=1}]"}""" XH_TAG

///////////////////////////////////////////////////////

let prng_init() = [|
    yield "scoreboard objectives add PRNG_MOD dummy"
    yield "scoreboard objectives add PRNG_OUT dummy"
    yield "scoreboard objectives add Calc dummy"
    yield "scoreboard players set A Calc 1103515245"
    yield "scoreboard players set C Calc 12345"
    yield "scoreboard players set Two Calc 2"
    yield "scoreboard players set TwoToSixteen Calc 65536"
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

let LOBBY = "62 25.0 63 180 0"
let TICKS_TO_UPDATE_MAP = 30
let game_objectives = [|
    // GLOBALS
    yield "CALL"  // for 'else' flow control - exclusive branch; always 0, except 1 just before a switch and 0 moment branch is taken
    yield "TEMP"  // a temporary variable anyone can use locally
    yield "TEMP2"  // a temporary variable anyone can use locally
    // bingo main game logic
    yield "Score"
    yield "fakeStart"
    yield "lockoutGoal"
    yield "numActiveTeams"
    yield "numActivePlayers"
    yield "hasAnyoneUpdated"
    // TODO someone wants option to turn off actionbar entirely
    yield "inXH"               // on a player, has a value greater than 0 if recently in extreme hills (or variant)
    yield "gameInProgress"     // 0 if not going, 1 if startup sequence, making spawns etc, 2 if game is running
    yield "TWENTY_MIL"
    yield "SIXTY"
    yield "THREE_SIXTY"
    yield "MINUS_ONE"
    yield "ONE_THOUSAND"
    yield "TEN_THOUSAND"
    yield "spawnDir"           // a player's spawnDir holds a value 1-8 that specifies which of 8 directions most closely points back to seed's spawn point
    yield "xspawn"             // a player's xspawn is the x coordinate of his spawn point for this seed
    yield "zspawn"             // a player's zspawn is the z coordinate of his spawn point for this seed
    yield "x"                  // temp variable on player
    yield "z"                  // temp variable on player
    yield "dx"                 // temp variable on player
    yield "dz"                 // temp variable on player
    yield "playerNum"          // a value from [0..numActivePlayers) which is used to "distribute the load" in SMP so some checks only done certain ticks so lots of players don't lag the server
    yield "tickNum"            // a value from [0..numActivePlayers) which increases each tick (mod numActivePlayers), used to run certain per-player commands only in certain ticks rather than every tick
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
let placeWallSignCmds x y z facing txt1 txt2 txt3 txt4 cmd isBold onlyPlaceIfMultiplayer =
    Utilities.placeWallSignCmds x y z facing txt1 txt2 txt3 txt4 cmd isBold (if isBold then "black" else "gray") (if onlyPlaceIfMultiplayer then "execute if $SCORE(TEMP=2..) run " else "")
let game_functions = [|
    yield "game_init", [|
        for o in game_objectives do
            yield sprintf "scoreboard objectives add %s dummy" o
        yield sprintf "function %s:%s/%s" NS CFG Utilities.ConfigFunctionNames.INIT 
        for t in TEAMS do
            yield sprintf "team add %s" t
            yield sprintf "team option %s color %s" t t
        yield "scoreboard objectives setdisplay sidebar Score"
        yield "scoreboard objectives add home trigger"
        yield "scoreboard objectives add PlayerSeed trigger"
        yield "scoreboard objectives add Deaths deathCount"  // for use by on_respawn
        yield "scoreboard players set $ENTITY TWENTY_MIL 20000000"
        yield "scoreboard players set $ENTITY SIXTY 60"
        yield "scoreboard players set $ENTITY THREE_SIXTY 360"
        yield "scoreboard players set $ENTITY MINUS_ONE -1"
        yield "scoreboard players set $ENTITY ONE_THOUSAND 1000"
        yield "scoreboard players set $ENTITY TEN_THOUSAND 10000"
        yield "scoreboard players set $ENTITY gameInProgress 0"
        yield sprintf "team join green @e[%s]" XH_TAG
        yield sprintf "function %s:%s/%s" NS CFG Utilities.ConfigFunctionNames.DEFAULT 
        yield sprintf "function %s:summon_book_text_entities" NS
        yield sprintf "function %s:cardgen_init_chooseable" NS
        |]
    for gip in [0;1;2] do // gameInProgress
        yield sprintf"place_signs%d"gip, [|
            let seedSignsEnabled = gip<>1
            let otherSignsEnabled = gip=0
            // sanity check
            yield sprintf """execute unless $SCORE(gameInProgress=%d) run tellraw @a ["ERROR: place_signs%d was called but gameInProgress is ",{"score":{"name":"%s","objective":"gameInProgress"}}]""" gip gip FAKE
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
            yield! placeWallSignCmds 63 27 61 "south" "get" "CONFIGURATION" "book(s)" "" (sprintf"function %s:get_configuration_books"NS) otherSignsEnabled false 
            //
            yield! placeWallSignCmds 65 27 61 "south" "put all on" "ONE team" "" "" (sprintf"function %s:assign_1_team"NS) otherSignsEnabled true
            yield! placeWallSignCmds 66 27 61 "south" "divide into" "TWO teams" "" "" (sprintf"function %s:assign_2_team"NS) otherSignsEnabled true
            yield! placeWallSignCmds 67 27 61 "south" "divide into" "THREE teams" "" "" (sprintf"function %s:assign_3_team"NS) otherSignsEnabled true
            yield! placeWallSignCmds 68 27 61 "south" "divide into" "FOUR teams" "" "" (sprintf"function %s:assign_4_team"NS) otherSignsEnabled true
            yield! placeWallSignCmds 69 26 61 "south" "LEAVE" "team" "(to sit out" "a game)" "team leave @s" otherSignsEnabled true
            //
            yield! Utilities.placeWallSignCmds 61 28 61 "south" "previous" "SEED" "" "" (sprintf"function %s:prev_seed"NS) seedSignsEnabled "black" "execute if $SCORE(optusval=1) run "
            yield! Utilities.placeWallSignCmds 62 28 61 "south" "fake START" "" "" "" (sprintf"function %s:fake_start"NS) otherSignsEnabled (if otherSignsEnabled then "black" else "gray") "execute if $SCORE(optusval=1) run "
            yield! Utilities.placeWallSignCmds 63 28 61 "south" "next" "SEED" "" "" (sprintf"function %s:next_seed"NS) seedSignsEnabled "black" "execute if $SCORE(optusval=1) run "
            //
            yield """kill @e[type=item,nbt={Item:{id:"minecraft:sign"}}]""" // dunno why old signs popping off when replaced by air
            |]
    let bingo40testers = [|
        "Ekiph"
        "brtw"
        "Zampone"
        "gothfaerie"
        "WarNev3rChanges"
        "BananaClanana"
        "BlkR0se"  // todo spelling?
        "Krazy_Faith"
        |]
    let bingo30testers = [|
        "Bergasms"
        "Cacille"
        "ConeDodger"
        "DucksEatFree"
        "gothfaerie"
        "Insmanity"
        "obesity84"
        "Shook50"
        |]
    let bingo20testers = [|
        "GrannyGamer1"
        "gothfaerie"
        "ConeDodger"
        "phedran"
        "jahg1977"
        "Zhuria"
        "Meroka"
        "Alzorath"
        "NihonTiger"
        "DireDwarf"
        "iSuchtel"
        "Blitzkriegsler"
        "IronStoneMine"
        "mod1982"
        "VanRyderLP"
        "generikb"
        "Trazlander"
        "three_two"
        "kurtjmac"
        "Bergasms"
        "FixxxerTV"
        "Grim"
        "LZmiljoona"
        "GreatScottLP"
        "LDShadowLady"
        "CthulhuToo"
        "Shook50"
        "DucksEatFree"
        |]
    let allTesters = [| yield! bingo20testers; yield! bingo30testers; yield! bingo40testers |] |> set |> Seq.toArray |> Array.sortBy (fun s -> s.ToLower()) 
    yield "thanks_book", [| 
        Utilities.makeCommandGivePlayerWrittenBook("Lorgon111","Thanks", [|
            sprintf """{"text":"I've spent more than 300 hours developing MinecraftBINGO, but I got a lot of help along the way.\n\nThanks to many playtesters on this and prior versions, including:\n\n%s,"}"""
                (String.concat ", " allTesters.[0..1])
            sprintf """{"text":"%s,"}""" (String.concat ", " allTesters.[2..19])
            sprintf """{"text":"%s"}""" (String.concat ", " allTesters.[20..])
            """{"text":"Special thanks to\nAntVenom\nwho gave me the idea for Version 1.0, and\nBergasms\nwho helped me test and implement the first version."}"""
            """{"text":"And of course, to you,\n","extra":[{"selector":"@p"},{"text":"\nthanks for playing!\n\nSigned,\nDr. Brian Lorgon111"}]}"""
            |], null)
        |]
    yield "version_book", [|
        Utilities.makeCommandGivePlayerWrittenBook("Lorgon111","Versions", [|
            """{"text":"You're playing MinecraftBINGO\n\nVersion ","extra":[{"text":"4.0",color:"red"},{"text":"\n\nTo get the latest version, click\n\n"},{"text":"@MinecraftBINGO","clickEvent":{"action":"open_url","value":"https://twitter.com/MinecraftBINGO"},"underlined":"true"}]}"""
            // TODO add 4.0 info
            """{"text":"Version History\n\n3.1 - 2017/03/24\n\nUpdated for 1.11 - removed abandonded mineshafts & cobweb, added 6 new items"}"""
            """{"text":"Version History\n\n3.0 - 2016/02/29\n\nRewrote everything from scratch using new Minecraft 1.9 command blocks. So much more efficient!"}"""
            """{"text":"Version History\n\n2.5 - 2014/11/27\n\nUpdate for Minecraft 1.8.1, which changed map colors.\n\n2.4 - 2014/09/12\n\nAdded 'seed' game mode to specify card and spawn point via a seed number."}"""
            """{"text":"Version History\n\n2.3 - 2014/06/17\n\nAdded more items and lockout mode.\n\n2.2 - 2014/05/29\n\nAdded multiplayer team gameplay."}"""
            """{"text":"Version History\n\n2.1 - 2014/05/12\n\nCustomized terrain with tiny biomes and many dungeons.\n\n2.0 - 2014/04/09\n\nRandomize the 25 items on the card."}"""
            """{"text":"Version History\n\n1.0 - 2013/10/03\n\nOriginal Minecraft 1.6 version. Only one fixed card, dispensed buckets of water to circle items on the map. (pre-dates /setblock!)"}"""
            |], null)
        |]
    yield "donation_book", [|
        Utilities.makeCommandGivePlayerWrittenBook("Lorgon111","Donations", [|
            """{"text":"I spent more than 300 hours programming MinecraftBINGO - mapmaking is a lot of work!\nIf you enjoy the game, and are willing and able, send me a tip! Any amount is appreciated.\n\n","extra":[{"text":"Click here to tip Brian","clickEvent":{"action":"open_url","value":"https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=YFATHZXXAXRZS"},"underlined":"true"}]}"""
            |], null )
        |]
    yield "make_lobby", [|
        (*
        TODO
            It's hard to design a lobby without first knowing the interface (set of activation signs) to all the features.  Get features working first.
        *)
        yield sprintf "execute in overworld run teleport @a %s" LOBBY
        yield "effect give @a minecraft:night_vision 99999 1 true"
        yield "fill 60 24 60 70 28 70 air"
        yield "fill 60 24 60 70 24 70 stone"
        yield "fill 60 24 60 70 28 60 stone"
        yield "setblock 64 25 60 sea_lantern"
        yield sprintf "function %s:place_signs0" NS
        // thanks/donations/versions books
        yield! placeWallSignCmds 62 26 77 "north" "THANKS" "" "" "" (sprintf "function %s:thanks_book" NS) true false
        yield! placeWallSignCmds 61 26 77 "north" "DONATE" "" "" "" (sprintf "function %s:donation_book" NS) true false  // TODO update paypal button text
        yield! placeWallSignCmds 60 26 77 "north" "HISTORY" "" "" "" (sprintf "function %s:version_book" NS) true false
        // make map-update-room
        yield sprintf "fill %s %s sea_lantern hollow" (MAP_UPDATE_ROOM.Offset(-3,-2,-3).STR) (MAP_UPDATE_ROOM.Offset(3,3,3).STR)
        yield sprintf "fill %s %s barrier hollow" (MAP_UPDATE_ROOM.Offset(-1,-1,-1).STR) (MAP_UPDATE_ROOM.Offset(1,2,1).STR)
        yield! placeWallSignCmds MAP_UPDATE_ROOM.X (MAP_UPDATE_ROOM.Y+1) (MAP_UPDATE_ROOM.Z-2) "south" "HOLD YOUR MAP" "(it will only" "update if you" "hold it)" null true false
        // make waiting room
        yield sprintf "fill %s %s sea_lantern hollow" (WAITING_ROOM.Offset(-3,-2,-3).STR) (WAITING_ROOM.Offset(3,3,3).STR)
        yield sprintf "fill %s %s barrier hollow" (WAITING_ROOM.Offset(-1,-1,-1).STR) (WAITING_ROOM.Offset(1,2,1).STR)
        yield! placeWallSignCmds WAITING_ROOM.X (WAITING_ROOM.Y+1) (WAITING_ROOM.Z-2) "south" "PLEASE WAIT" "(spawns are" "being" "generated)" null true false
        // make daylight blocker under the bingo art to prevent art changes from causing lighting updates below it
        yield sprintf "fill 0 %d 0 127 %d 119 stone" ART_HEIGHT_DAYLIGHT_BLOCKER ART_HEIGHT_DAYLIGHT_BLOCKER
        // put logo on card bottom
        yield sprintf "fill 0 %d 120 127 %d 127 black_wool" ART_HEIGHT ART_HEIGHT
        yield sprintf """summon area_effect_cloud 0 %d 120 {Duration:2,Tags:["logoaec"]}""" ART_HEIGHT
        for i = 1 to 4 do
            let x = 32*(i-1)
            yield sprintf """execute at @e[tag=logoaec] positioned ~%d ~ ~ run setblock ~ ~ ~ minecraft:structure_block{posX:0,posY:0,posZ:0,sizeX:32,sizeY:1,sizeZ:7,mode:"LOAD",name:"test:logo%d"}""" x i
            yield sprintf """execute at @e[tag=logoaec] positioned ~%d ~ ~1 run setblock ~ ~ ~ minecraft:redstone_block""" x
        // make the 150 150 150 room
        yield sprintf "fill 148 149 148 152 149 152 minecraft:light_gray_stained_glass"
        yield sprintf "setblock 150 151 147 stone"
        yield! placeWallSignCmds 150 151 148 "south" "teleport" "back to" "LOBBY" "" (sprintf "teleport @s %s" LOBBY) true false
        // make area for players who respawn without valid spawn point
        yield sprintf "fill 60 %d 60 70 %d 70 minecraft:light_gray_stained_glass" (ART_HEIGHT+10) (ART_HEIGHT+10)
        yield sprintf "setblock 65 %d 60 light_gray_stained_glass" (ART_HEIGHT+12)
        yield! placeWallSignCmds 65 (ART_HEIGHT+12) 61 "south" "right click me" "to teleport" "to" "LOBBY" (sprintf "teleport @s %s" LOBBY) true false
        yield sprintf "setblock 69 %d 70 light_gray_stained_glass" (ART_HEIGHT+12)
        yield! placeWallSignCmds 69 (ART_HEIGHT+12) 69 "north" "Welcome to" "MinecraftBINGO" "by Dr. Brian" "Lorgon111" null true false
        yield sprintf "setblock 67 %d 70 light_gray_stained_glass" (ART_HEIGHT+12)
        yield! placeWallSignCmds 67 (ART_HEIGHT+12) 69 "north" "This is" "Version 4.0" "of the map." "(for MC 1.13)" null true false
        yield sprintf "setblock 66 %d 70 light_gray_stained_glass" (ART_HEIGHT+12)
        yield! placeWallSignCmds 66 (ART_HEIGHT+12) 69 "north" "Right click me" "to discover" "the latest" "version." (sprintf "function %s:latest_version" NS) true false
        yield sprintf "setblock 64 %d 70 light_gray_stained_glass" (ART_HEIGHT+12)
        yield! placeWallSignCmds 64 (ART_HEIGHT+12) 69 "north" "Turn around" "to find sign" "to go to" "the lobby." null true false
        yield sprintf "setblock 62 %d 70 light_gray_stained_glass" (ART_HEIGHT+12)
        yield! Utilities.placeWallSignCmds 62 (ART_HEIGHT+12) 69 "north" "server" "properties" "enable-command-" "block = true" null false "black" ""
        yield sprintf "setblock 61 %d 70 light_gray_stained_glass" (ART_HEIGHT+12)
        yield! Utilities.placeWallSignCmds 61 (ART_HEIGHT+12) 69 "north" "server" "properties" "spawn-" "protection=0" null false "black" ""
        |]
    yield "latest_version",[|
        let downloadUrl = "https://twitter.com/MinecraftBINGO"
        yield sprintf """tellraw @a {"text":"Press 't' (chat), then click line below to visit the official download page for MinecraftBINGO"}"""  // TODO /tellraw @a {"keybind":"key.chat"}
        yield sprintf """tellraw @a {"text":"%s","underlined":"true","clickEvent":{"action":"open_url","value":"%s"}}""" downloadUrl downloadUrl
        |]
    yield "assign_1_team",[|
        yield "team join red @a"
        yield "scoreboard players add @a Score 0"
        yield sprintf "function %s:compute_lockout_goal" NS
        |]
    yield "assign_2_team",[|
        yield "team leave @a"
        for _i = 1 to 20 do
            yield "team join red @r[team=]"
            yield "team join blue @r[team=]"
        yield "scoreboard players add @a Score 0"
        yield sprintf "function %s:compute_lockout_goal" NS
        |]
    yield "assign_3_team",[|
        yield "team leave @a"
        for _i = 1 to 13 do
            yield "team join red @r[team=]"
            yield "team join blue @r[team=]"
            yield "team join green @r[team=]"
        yield "scoreboard players add @a Score 0"
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
        yield sprintf "function %s:compute_lockout_goal" NS
        |]
    for t in TEAMS do
        yield sprintf"%s_team_join"t, [|
            sprintf "team join %s" t
            "scoreboard players add @s Score 0"
            sprintf "function %s:compute_lockout_goal" NS
            |]
    // Note: the book_text_entities are shared by child packs as well
    yield "kill_book_text_entities",[|
        "kill @e[tag=bookText]"
        |]
    yield "summon_book_text_entities",[|
        "kill @e[tag=bookText]"
        """summon armor_stand 37 1 37 {Invulnerable:1b,Invisible:1b,NoGravity:1b,Tags:["bookText","bookTextON"],CustomName:"\"ON\"",Team:green}"""
        """summon armor_stand 37 1 37 {Invulnerable:1b,Invisible:1b,NoGravity:1b,Tags:["bookText","bookTextOFF"],CustomName:"\"OFF\"",Team:red}"""
        |]
    yield "config_loop",[|
        sprintf "function %s:%s/%s" NS CFG Utilities.ConfigFunctionNames.LISTEN
        sprintf """kill @e[type=item,nbt={Item:{id:"minecraft:written_book",tag:{%s:1}}}]""" bingoConfigBookTag
        |]
    yield "get_configuration_books",[|
        // call ourselves
        sprintf "function %s:%s/%s" NS CFG Utilities.ConfigFunctionNames.GET
        // call subscribers to the event
        sprintf "function #%s:on_get_configuration_books" NS
        |]
    yield "fake_start",[| // for testing; start sequence without the spawn points
        "scoreboard players set $ENTITY fakeStart 1"
        sprintf "function %s:start1" NS
        |]
    yield "ensure_maps",[|   // called when player @s currently has no bingo cards
        """execute if entity @s[scores={ticksSinceGotMap=40..}] run give @s minecraft:filled_map{display:{Name:"\"BINGO Card\""},map:0} 32"""
        "scoreboard players set @s[scores={ticksSinceGotMap=40..}] ticksSinceGotMap 0"
        |]
    yield "choose_seed",[|
        yield "scoreboard players set @a PlayerSeed -1"
        yield "scoreboard players enable @a PlayerSeed"
        yield """tellraw @a {"text":"Press 't' (chat), click below, then replace NNN with a seed number in chat"}"""   // TODO /tellraw @a {"keybind":"key.chat"}
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
    yield "prev_seed",[|
        yield "scoreboard players remove Seed Score 1"
        yield "scoreboard players operation Z Calc = Seed Score"
        yield sprintf "function %s:new_card_coda" NS
        |]
    yield "next_seed",[|
        yield "scoreboard players add Seed Score 1"
        yield "scoreboard players operation Z Calc = Seed Score"
        yield sprintf "function %s:new_card_coda" NS
        |]
    yield "new_card_coda",[|
        yield "scoreboard players set $ENTITY fakeStart 0"
        yield sprintf "execute unless $SCORE(gameInProgress=2) run function %s:reset_player_scores" NS
        yield sprintf "execute if $SCORE(gameInProgress=2) run function %s:finish1" NS
        yield sprintf "execute if $SCORE(gameInProgress=0) run function %s:place_signs0" NS
        yield sprintf "function %s:cardgen_makecard" NS
        yield sprintf "function %s:compute_lockout_goal" NS
        yield sprintf "function #%s:on_new_card" NS
        |]
    let COLOR = """"color":"yellow","""
    let YCOLOR = """"color":"aqua","""
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
        sprintf """execute if $SCORE(said25mins=0,minutes=1500) as @a unless entity @s[team=] run function %s:at25mins""" NS
        // compute actual MM:SS and display
        "scoreboard players operation $ENTITY seconds = $ENTITY minutes"
        "scoreboard players operation $ENTITY minutes /= $ENTITY SIXTY"
        "scoreboard players operation $ENTITY seconds %= $ENTITY SIXTY"
        "scoreboard players set $ENTITY preseconds 0"
        "execute if $SCORE(seconds=10..) run scoreboard players reset $ENTITY preseconds"
        sprintf "execute as @a unless entity @s[team=] run function %s:update_time_per_player" NS
        |]
    yield "update_time_per_player",[|
        // pretty timer
        "scoreboard players operation @s minutes = $ENTITY minutes"
        "scoreboard players operation @s seconds = $ENTITY seconds"
        "scoreboard players set @s preseconds 0"
        "scoreboard players reset @s[scores={seconds=10..}] preseconds"
        // extreme hills detection
        "scoreboard players remove @s[scores={inXH=1..}] inXH 1"
        sprintf "scoreboard players set @e[%s] inXH 0" XH_TAG 
        sprintf "execute if $SCORE(optxhval=1) if entity @s[scores={inXH=1..}] run scoreboard players set @e[%s] inXH 1" XH_TAG 
        // y coordinate
        "execute store result score @s TEMP run data get entity @s Pos[1] 1.0"
        // display
        sprintf """execute if $SCORE(optsaval=0) run title @s actionbar [{%s"score":{"name":"@s","objective":"minutes"}},{%s"text":":"},{%s"score":{"name":"@s","objective":"preseconds"}},{%s"score":{"name":"@s","objective":"seconds"}},{%s"text":" Y:"},{%s"score":{"name":"@s","objective":"TEMP"}}," ",%s]""" COLOR COLOR COLOR COLOR YCOLOR YCOLOR XH_TEXT
        sprintf """execute if $SCORE(optsaval=1) run function %s:update_time_per_player_with_arrow""" NS
        |]
    yield "update_time_per_player_with_arrow",[|  // TODO "name"="*" selects 'person being displayed to' maybe? test perf of * versus execute as @a run ... @s
        sprintf "function %s:find_dir_to_spawn" NS
        sprintf """execute if entity @s[scores={spawnDir=1}] run title @s actionbar [{%s"score":{"name":"@s","objective":"minutes"}},{%s"text":":"},{%s"score":{"name":"@s","objective":"preseconds"}},{%s"score":{"name":"@s","objective":"seconds"}},{%s"text":" Y:"},{%s"score":{"name":"@s","objective":"TEMP"}}," ",%s,{%s"text":" \u2191"}]""" COLOR COLOR COLOR COLOR YCOLOR YCOLOR XH_TEXT COLOR
        sprintf """execute if entity @s[scores={spawnDir=2}] run title @s actionbar [{%s"score":{"name":"@s","objective":"minutes"}},{%s"text":":"},{%s"score":{"name":"@s","objective":"preseconds"}},{%s"score":{"name":"@s","objective":"seconds"}},{%s"text":" Y:"},{%s"score":{"name":"@s","objective":"TEMP"}}," ",%s,{%s"text":" \u2197"}]""" COLOR COLOR COLOR COLOR YCOLOR YCOLOR XH_TEXT COLOR
        sprintf """execute if entity @s[scores={spawnDir=3}] run title @s actionbar [{%s"score":{"name":"@s","objective":"minutes"}},{%s"text":":"},{%s"score":{"name":"@s","objective":"preseconds"}},{%s"score":{"name":"@s","objective":"seconds"}},{%s"text":" Y:"},{%s"score":{"name":"@s","objective":"TEMP"}}," ",%s,{%s"text":" \u2192"}]""" COLOR COLOR COLOR COLOR YCOLOR YCOLOR XH_TEXT COLOR
        sprintf """execute if entity @s[scores={spawnDir=4}] run title @s actionbar [{%s"score":{"name":"@s","objective":"minutes"}},{%s"text":":"},{%s"score":{"name":"@s","objective":"preseconds"}},{%s"score":{"name":"@s","objective":"seconds"}},{%s"text":" Y:"},{%s"score":{"name":"@s","objective":"TEMP"}}," ",%s,{%s"text":" \u2198"}]""" COLOR COLOR COLOR COLOR YCOLOR YCOLOR XH_TEXT COLOR
        sprintf """execute if entity @s[scores={spawnDir=5}] run title @s actionbar [{%s"score":{"name":"@s","objective":"minutes"}},{%s"text":":"},{%s"score":{"name":"@s","objective":"preseconds"}},{%s"score":{"name":"@s","objective":"seconds"}},{%s"text":" Y:"},{%s"score":{"name":"@s","objective":"TEMP"}}," ",%s,{%s"text":" \u2193"}]""" COLOR COLOR COLOR COLOR YCOLOR YCOLOR XH_TEXT COLOR
        sprintf """execute if entity @s[scores={spawnDir=6}] run title @s actionbar [{%s"score":{"name":"@s","objective":"minutes"}},{%s"text":":"},{%s"score":{"name":"@s","objective":"preseconds"}},{%s"score":{"name":"@s","objective":"seconds"}},{%s"text":" Y:"},{%s"score":{"name":"@s","objective":"TEMP"}}," ",%s,{%s"text":" \u2199"}]""" COLOR COLOR COLOR COLOR YCOLOR YCOLOR XH_TEXT COLOR
        sprintf """execute if entity @s[scores={spawnDir=7}] run title @s actionbar [{%s"score":{"name":"@s","objective":"minutes"}},{%s"text":":"},{%s"score":{"name":"@s","objective":"preseconds"}},{%s"score":{"name":"@s","objective":"seconds"}},{%s"text":" Y:"},{%s"score":{"name":"@s","objective":"TEMP"}}," ",%s,{%s"text":" \u2190"}]""" COLOR COLOR COLOR COLOR YCOLOR YCOLOR XH_TEXT COLOR
        sprintf """execute if entity @s[scores={spawnDir=8}] run title @s actionbar [{%s"score":{"name":"@s","objective":"minutes"}},{%s"text":":"},{%s"score":{"name":"@s","objective":"preseconds"}},{%s"score":{"name":"@s","objective":"seconds"}},{%s"text":" Y:"},{%s"score":{"name":"@s","objective":"TEMP"}}," ",%s,{%s"text":" \u2196"}]""" COLOR COLOR COLOR COLOR YCOLOR YCOLOR XH_TEXT COLOR
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
            // store xspawn/zspawn on players
            yield sprintf "execute as @a[team=%s] run scoreboard players operation @s xspawn = $ENTITY %sSpawnX" t t
            yield sprintf "execute as @a[team=%s] run scoreboard players operation @s xspawn *= $ENTITY TEN_THOUSAND" t
            yield sprintf "execute as @a[team=%s] run scoreboard players operation @s zspawn = $ENTITY %sSpawnZ" t t
            yield sprintf "execute as @a[team=%s] run scoreboard players operation @s zspawn *= $ENTITY TEN_THOUSAND" t
            // now that players are there, wait for some terrain to gen
            yield "$NTICKSLATER(20)"
            // build skybox and put players there
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run fill ~-1 %d ~-1 ~1 %d ~1 barrier hollow" t SKYBOX (SKYBOX+20)
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run setblock ~ %d ~ minecraft:light_gray_stained_glass" t (SKYBOX+1)  // so that particles not bright red when falling atop
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run teleport @a[team=%s] ~0.5 ~ ~0.5" t t
            // figure out Y height of surface
            yield sprintf "function %s:compute_height" NS
            yield sprintf "execute as @e[tag=%sSpawn] store result score $ENTITY %sSpawnY run data get entity @s Pos[1] 1.0" t t
            // give people time in skybox while terrain gens, then put them on ground and set spawns
            yield sprintf """tellraw @a ["Giving %s team a birds-eye view as terrain generates..."]""" t
            yield "$NTICKSLATER(360)"
            yield sprintf """tellraw @a ["About to drop %s team out of sky-box..."]""" t
            yield "$NTICKSLATER(40)"
            // un-build skybox
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run fill ~-1 %d ~-1 ~1 %d ~1 air hollow" t SKYBOX (SKYBOX+20)
            // give people time to fall, preserving orientation
            yield "$NTICKSLATER(100)"
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run teleport @a[team=%s] ~0.5 ~ ~0.5" t t
            yield sprintf "execute at @e[tag=%sSpawn,limit=1] run spawnpoint @a[team=%s] ~0.5 ~ ~0.5" t t
            // place beacon to mark spawn
            yield "execute as @e[tag=CurrentSpawn] at @s positioned ~ ~10 ~ run fill ~-2 ~0 ~-2 ~2 ~3 ~2 minecraft:barrier hollow"
            yield "execute as @e[tag=CurrentSpawn] at @s positioned ~ ~10 ~ run fill ~-1 ~1 ~-1 ~1 ~1 ~1 minecraft:diamond_block"
            yield "execute as @e[tag=CurrentSpawn] at @s positioned ~ ~10 ~ run setblock ~ ~2 ~ minecraft:beacon"
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
            yield sprintf "execute if $SCORE(numActiveTeams=2,TEMP=1) if entity @a[team=%s] run scoreboard players set $ENTITY %sRightHalf 1" t t
            yield sprintf "execute if $SCORE(numActiveTeams=2,TEMP=0) if entity @a[team=%s] run scoreboard players set $ENTITY %sLeftHalf 1" t t
            yield sprintf "execute if $SCORE(numActiveTeams=2,TEMP=0) if entity @a[team=%s] run scoreboard players set $ENTITY TEMP 1" t
        |]
    yield "start1", [|
        // ensure folks have joined teams
        yield sprintf "function %s:compute_active_teams" NS
        yield """execute if $SCORE(numActiveTeams=0) run tellraw @a ["No one has joined a team - join a team color to play!"]"""
        yield sprintf "execute if $SCORE(numActiveTeams=1..) run function %s:start2" NS
        |]
    yield "reset_player_scores",[|
        yield "scoreboard players operation $ENTITY Seed = Seed Score"  // save seed
        yield "scoreboard players reset * Score"
        // re-initialize
        yield "scoreboard players operation Seed Score = $ENTITY Seed"  // restore seed
        for t in TEAMS do
            yield sprintf "scoreboard players set @a[team=%s] Score 0" t
        |]
    yield "compute_lockout_goal", [|
        // set up lockout goal if lockout mode selected (teamCount 2/3/4 -> goal 13/9/7)
        yield sprintf "function %s:compute_active_teams" NS
        yield "execute if $SCORE(numActiveTeams=1) run scoreboard players set $ENTITY lockoutGoal 25"
        yield "execute if $SCORE(numActiveTeams=2) run scoreboard players set $ENTITY lockoutGoal 13"
        yield "execute if $SCORE(numActiveTeams=3) run scoreboard players set $ENTITY lockoutGoal 9"
        yield "execute if $SCORE(numActiveTeams=4) run scoreboard players set $ENTITY lockoutGoal 7"
        yield "execute if $SCORE(optloval=1) run scoreboard players operation LockoutGoal Score = $ENTITY lockoutGoal"
        yield "execute unless $SCORE(optloval=1) run scoreboard players reset LockoutGoal Score"
        |]
    yield "compute_player_numbers", [|
        yield "execute store result score $ENTITY numActivePlayers if entity @a"
        // arbitrarily assign each person a unique number in range [0..numActivePlayers)
        yield "scoreboard players set @a playerNum -1"
        for i = 0 to 39 do
            yield sprintf "scoreboard players set @p[scores={playerNum=-1}] playerNum %d" i
        |]
    yield "start2", [|
        // clear player scores again (in case player joined server after card gen'd)
        yield sprintf "function %s:reset_player_scores" NS
        yield sprintf "function %s:compute_lockout_goal" NS
        yield sprintf "function %s:compute_player_numbers" NS
        // note game in progress
        yield "scoreboard players set $ENTITY gameInProgress 1"
        yield sprintf "function %s:kill_book_text_entities" NS
        yield sprintf "function #%s:on_start_game" NS
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
        yield """give @s minecraft:filled_map{display:{Name:"\"BINGO Card\""},map:0} 640"""
        yield sprintf "$NTICKSLATER(%d)" TICKS_TO_UPDATE_MAP
        yield "clear @a"  // Note: no longer @s since NTICKSLATER
        yield sprintf "function %s:start3" NS
        |]
    yield "start3", [|
        // copy LW info to its entity
        yield sprintf "execute unless $SCORE(optlwval=1) run scoreboard players set @e[%s] optlwval 0" LW_TAG 
        yield sprintf "execute if $SCORE(optlwval=1) run scoreboard players set @e[%s] optlwval 1" LW_TAG 
        // give maps in offhand for start of game
        yield """replaceitem entity @a weapon.offhand minecraft:filled_map{display:{Name:"\"BINGO Card\""},map:0} 32""" // unused: way to test offhand non-empty - scoreboard players set @p[nbt={Inventory:[{Slot:-106b}]}] offhandFull 1
        // give player all the effects
        yield "effect give @a minecraft:slowness 999 127 true"
        yield "effect give @a minecraft:mining_fatigue 999 7 true"
        yield "effect give @a minecraft:jump_boost 999 150 true"
        yield "effect give @a minecraft:resistance 999 4 true"
        yield "effect give @a minecraft:water_breathing 999 4 true"
        yield "effect give @a minecraft:invisibility 999 4 true"
        // set time to day so not tp at night
        yield "time set 0"
        // set to peaceful so that hostiles don't spawn and are not available to interact with for players who cheat-move-in-multiplayer-startup
        yield "difficulty peaceful"
        yield sprintf "execute if $SCORE(fakeStart=1) run function %s:start5" NS
        yield sprintf "execute if $SCORE(fakeStart=0) run function %s:do_spawn_sequence" NS
        |]
    yield "do_spawn_sequence", [|
        // tp all to waiting room
        yield sprintf "execute in overworld as @a unless entity @s[team=] run teleport @s %s 180 0" WAITING_ROOM.TPSTR
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
        // clear weather
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
        // custom game modes
        yield "execute if $SCORE(optnvval=1) run effect give @a minecraft:night_vision 99999 1 true"
        yield "execute if $SCORE(optdsval=1) run replaceitem entity @a armor.feet minecraft:leather_boots{Unbreakable:1,ench:[{lvl:3s,id:8s},{lvl:1s,id:10s},{lvl:1s,id:71s}]} 1"
        yield "execute if $SCORE(optboatval=1) run give @a minecraft:oak_boat 1"
        yield sprintf """tellraw @a [%s,"Start! Go!!!"]""" LEADING_WHITESPACE
        yield "execute as @a at @s run playsound block.note.harp ambient @s ~ ~ ~ 1 1.2"
        // enable triggers (for click-in-chat-to-tp-home stuff)
        yield "scoreboard players set @a home 0"
        yield "scoreboard players enable @a home"
        // option to get back
        yield """execute if $SCORE(opthhval=1) run tellraw @a ["(If you need to quit before getting BINGO, you can"]"""
        yield """execute if $SCORE(opthhval=1) run tellraw @a [{"underlined":"true","text":"press 't' (chat), then click this line to return to the lobby)","clickEvent":{"action":"run_command","value":"/trigger home set 1"}}]"""   // TODO /tellraw @a {"keybind":"key.chat"}
        yield sprintf """execute if $SCORE(opthhval=0) run tellraw @a [%s,{"underlined":"true","text":"click to go to lobby","clickEvent":{"action":"run_command","value":"/trigger home set 1"}}]""" LEADING_WHITESPACE
        yield "worldborder set 20000000"          // 20 million wide is 10 million from spawn
        yield "worldborder add 10000000 10000000" // 10 million per 10 million seconds is one per second
        yield "scoreboard players set $ENTITY gameInProgress 2"
        yield sprintf "function %s:place_signs2" NS
        yield "scoreboard players set $ENTITY hasAnyoneUpdated 0"
        yield "execute if $SCORE(opthhval=0) run scoreboard players set $ENTITY hasAnyoneUpdated 1"
        |]
    yield "go_home", [|
        sprintf "execute in overworld run teleport @s %s" LOBBY
        "effect give @s minecraft:saturation 10 4 true"  // feed (and probably will heal some too)
        "effect give @s minecraft:night_vision 99999 1 true"
        "scoreboard players set @a home 0"
        "scoreboard players enable @a home"    // re-enable for everyone, so even if die in lobby afterward and respawn out in world again, can come back
        |]
    yield "finish1", [| // called for transition gameInProgress 2->0
        "scoreboard players set $ENTITY gameInProgress 0"
        sprintf "function %s:place_signs0" NS
        sprintf "execute in overworld run teleport @a %s" LOBBY
        "gamemode survival @a"
        "clear @a"
        // feed & heal, as people get concerned in lobby about this
        "effect give @a minecraft:saturation 10 4 true"
        "effect give @a minecraft:regeneration 10 4 true"
        sprintf "function %s:summon_book_text_entities" NS
        sprintf "function #%s:on_finish" NS
        |]
    |]

///////////////////////////////////////////////////////

let map_update_objectives = [|
    yield "ticksLeftMU"        // remaining time left for a player who dropped maps to wait in map-update room for card colors to have time to redraw
    yield "ReturnX"
    yield "ReturnY"
    yield "ReturnZ"
    yield "ReturnRotX"
    yield "ReturnRotY"
    yield "ReturnDim"  // 0 for overworld, 1 for nether
    |]
let map_update_functions = [| 
    yield "map_update_init", [|
        for o in map_update_objectives do
            yield sprintf "scoreboard objectives add %s dummy" o
        |]
    yield "map_near_player", [| // called as/at the map item
        sprintf """execute unless entity @a[scores={ticksLeftMU=1..}] as @p[distance=..5] run function %s:warp_home""" NS
        "kill @s"
        |]
    yield "map_update_tick", [| // called every tick
        // find maps near players (for efficiency, rather than looking for all items in world, only look near players)
        sprintf """execute as @a unless entity @s[team=] at @s as @e[limit=1,type=item,nbt={Item:{id:"minecraft:filled_map",tag:{map:0}}},distance=..3] run function %s:map_near_player""" NS  // TODO serializing nbt per player, lag?
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
        sprintf "scoreboard players set @s ticksLeftMU %d" TICKS_TO_UPDATE_MAP
        "execute store result score @s ReturnX run data get entity @s Pos[0] 128.0"   // doubles
        "execute store result score @s ReturnY run data get entity @s Pos[1] 128.0"
        "execute store result score @s ReturnZ run data get entity @s Pos[2] 128.0"
        "execute store result score @s ReturnRotX run data get entity @s Rotation[0] 8.0"   // floats
        "execute store result score @s ReturnRotY run data get entity @s Rotation[1] 8.0"
        "execute store result score @s ReturnDim run data get entity @s Dimension 1.0"
        sprintf """tellraw @a [%s,{"selector":"@s"}," is updating the BINGO map"]""" LEADING_WHITESPACE
        // note: only persist entities not already persisted, this way e.g. we don't later un-persist the zombie who picked up your sword
        """execute as @e[type=!player,distance=..160,nbt={PersistenceRequired:0b}] run data merge entity @s {PersistenceRequired:1b,Tags:["persisted"]}"""  // preserve mobs
        "execute at @s run particle minecraft:portal ~ ~ ~ 0.1 0.1 0.1 1 29 normal"
        sprintf "execute in overworld run teleport @s %s 180 0" MAP_UPDATE_ROOM.TPSTR
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
        "execute if entity @s[scores={ReturnDim=-1}] at @s in the_nether run teleport @s ~ ~ ~"
        "execute if entity @s[scores={ReturnDim=0}] at @s in overworld run teleport @s ~ ~ ~"
        """execute as @e[type=!player,tag=persisted] run data merge entity @s {PersistenceRequired:0b,Tags:["none"]}"""  // un-preserve mobs (note: cannot use distance=, since just teleported, so must search world)
        "execute at @s run playsound entity.endermen.teleport ambient @a"
        "scoreboard players set $ENTITY hasAnyoneUpdated 1"
        |]
    |]

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
        [|  "glowstone_dust"   ; "glowstone_dust"   ; "glowstone_dust"      |]
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

let itemIndex(name) = flatBingoItems |> Array.findIndex(fun x -> x = name)

///////////////////////////////////////////////////////////////////////////////

let CHOOSE_X = 80
let CHOOSE_Y = 25
let CHOOSE_Z = 60

let choosable_functions = [|
    yield "make_item_wall_colors", [|
        for i = 0 to bingoItems.Length-1 do
            for j = 0 to 2 do
                let item = bingoItems.[i].[j]
                let index = itemIndex(item)
                yield sprintf "execute if $SCORE(okItem%03d=1) run setblock %d %d %d emerald_block"  index CHOOSE_X (CHOOSE_Y+3-j) (CHOOSE_Z+1+i)
                yield sprintf "execute if $SCORE(okItem%03d=0) run setblock %d %d %d redstone_block" index CHOOSE_X (CHOOSE_Y+3-j) (CHOOSE_Z+1+i)
        |]
    yield "make_item_wall", [|
        yield sprintf "fill %d %d %d %d %d %d stone" CHOOSE_X CHOOSE_Y CHOOSE_Z CHOOSE_X (CHOOSE_Y+4) (CHOOSE_Z + bingoItems.Length + 1)
        yield sprintf "function %s:make_item_wall_colors" NS
        for i = 0 to bingoItems.Length-1 do
            for j = 0 to 2 do
                let item = bingoItems.[i].[j]
                let index = itemIndex(item)
                yield sprintf """summon item_frame %d %d %d {Tags:[tempIF,tempIF%02d%02d],Invulnerable:1b,Facing:4b,ItemRotation:0b,Item:{id:"minecraft:%s",Count:1b}}""" (CHOOSE_X-1) (CHOOSE_Y+3-j) (CHOOSE_Z+1+i) i j item
        |]
    yield "clear_item_wall", [|
        "kill @e[tag=tempIF]"
        sprintf "fill %d %d %d %d %d %d air" CHOOSE_X CHOOSE_Y CHOOSE_Z CHOOSE_X (CHOOSE_Y+4) (CHOOSE_Z + bingoItems.Length + 1)
        |]
    yield "item_chooser_tick", [|
        yield "scoreboard players set $ENTITY TEMP 0"  // any changes?
        for i = 0 to bingoItems.Length-1 do
            for j = 0 to 2 do
                let item = bingoItems.[i].[j]
                let index = itemIndex(item)
                yield sprintf """execute positioned %d %d %d as @e[distance=..1,tag=tempIF%02d%02d] unless entity @s[nbt={ItemRotation:0b}] run function %s:toggle_choosable%03d""" (CHOOSE_X-1) (CHOOSE_Y+3-j) (CHOOSE_Z+1+i) i j NS index 
        yield sprintf "execute if $SCORE(TEMP=1) run function %s:make_item_wall_colors" NS
        |]
    for i = 0 to flatBingoItems.Length-1 do
        yield sprintf "toggle_choosable%03d" i, [|
            // change 1->0 and 0->1
            sprintf "scoreboard players operation $ENTITY TEMP2 = $ENTITY okItem%03d" i
            sprintf "execute if $SCORE(TEMP2=1) run scoreboard players set $ENTITY okItem%03d 0" i
            sprintf "execute if $SCORE(TEMP2=0) run scoreboard players set $ENTITY okItem%03d 1" i
            // fix rotation
            "data merge entity @s {ItemRotation:0b}"
            // note that a change was made
            "scoreboard players set $ENTITY TEMP 1"
            |]
    |]

///////////////////////////////////////////////////////////////////////////////

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
            yield sprintf "%sWinSlash" t         // how many items in the bottom left to top right diagonal the team has (Slash)
            yield sprintf "%sWinBkSlh" t         // how many items in the top left to bottom right diagonal the team has ('BackSlash' is too long for objective name)
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
            yield sprintf "scoreboard players set $ENTITY %sWinBkSlh 0" t  
            yield sprintf "scoreboard players set $ENTITY %sScore 0" t         
            yield sprintf "scoreboard players set $ENTITY %sGotBingo 0" t         
        yield sprintf "scoreboard players set $ENTITY lockoutGoal 25"
        |]
    yield "check_inventory",[| // called as a player @s
        sprintf "execute if score @s playerNum = $ENTITY tickNum run function %s:check_inventory_core" NS
        |]
    yield "check_inventory_core",[| // called as a player @s
        sprintf "execute if entity @s[team=red] run function %s:red_inventory_changed" NS
        sprintf "execute if entity @s[team=blue] run function %s:blue_inventory_changed" NS
        sprintf "execute if entity @s[team=green] run function %s:green_inventory_changed" NS
        sprintf "execute if entity @s[team=yellow] run function %s:yellow_inventory_changed" NS
        |]
    for t in TEAMS do
        yield sprintf "%s_inventory_changed" t, [|        // called when player @s's inventory changed and he is on team t
            yield sprintf "scoreboard players set $ENTITY gotItems 0"
            for s in SQUARES do
                yield sprintf "scoreboard players set $ENTITY gotAnItem 0"
                yield sprintf "execute if $SCORE(%sCanGet%s=1) run function %s:inv/check%s" t s NS s
                yield sprintf "execute if $SCORE(gotAnItem=1) run function %s:got/%s_got_square_%s" NS t s
                if s <> "33" then
                    yield sprintf "execute if $SCORE(gotAnItem=1,opttfoval=1) run function %s:got/%s_got_square_%s" NS t (sprintf "%d%d" (int '6' - int s.[0]) (int '6' - int s.[1]))
            yield sprintf """execute if $SCORE(gotItems=1,hasAnyoneUpdated=0,optyaival=1..) run tellraw @s ["To update the BINGO map, drop one copy on the ground"]"""
            yield sprintf "execute if $SCORE(gotItems=1,optyfival=1) as @a[team=%s] at @s run playsound entity.firework.launch ambient @s ~ ~ ~" t
            yield sprintf "execute if $SCORE(gotItems=1,optofival=1) as @a[team=!%s] at @s run playsound entity.firework.launch ambient @s ~ ~ ~" t
            yield sprintf "execute if $SCORE(gotItems=1) run function %s:%s_check_for_win" NS t
            |]
        for s in SQUARES do
            yield sprintf "got/%s_got_square_%s" t s, [|       // called when player @s got square s and he is on team t
                yield sprintf "scoreboard players set $ENTITY gotItems 1"
                yield sprintf "scoreboard players add $ENTITY %sScore 1" t
                yield sprintf "scoreboard players operation @a[team=%s] Score = $ENTITY %sScore" t t
                yield sprintf "scoreboard players set $ENTITY %sCanGet%s 0" t s
                for ot in TEAMS do
                    if ot <> t then
                        yield sprintf "execute if $SCORE(optloval=1) run scoreboard players set $ENTITY %sCanGet%s 0" ot s
                let x = 2 + 24*(int s.[0] - int '0' - 1)
                let y = ART_HEIGHT
                let z = 0 + 24*(int s.[1] - int '0' - 1)
                // determine if we should fill the whole square
                yield sprintf "scoreboard players set $ENTITY TEMP 0"
                yield sprintf "execute if $SCORE(numActiveTeams=1) run scoreboard players set $ENTITY TEMP 1"
                yield sprintf "execute if $SCORE(optloval=1) run scoreboard players set $ENTITY TEMP 1"
                yield sprintf "execute if $SCORE(TEMP=1) run fill %d %d %d %d %d %d %s replace clay" x y z (x+22) y (z+22) (if t="green" then "emerald_block" else t+"_wool")
                // else if 2 active teams, fill the half
                yield sprintf "execute if $SCORE(numActiveTeams=2,%sLeftHalf=1) run fill %d %d %d %d %d %d %s replace clay" t (x+00) y (z+00) (x+11) y (z+22) (if t="green" then "emerald_block" else t+"_wool")
                yield sprintf "execute if $SCORE(numActiveTeams=2,%sRightHalf=1) run fill %d %d %d %d %d %d %s replace clay" t (x+12) y (z+00) (x+22) y (z+22) (if t="green" then "emerald_block" else t+"_wool")
                yield sprintf "execute if $SCORE(numActiveTeams=2) run scoreboard players set $ENTITY TEMP 1"
                // else fill the corner
                if t = "red" then
                    yield sprintf "execute unless $SCORE(TEMP=1) run fill %d %d %d %d %d %d %s replace clay" (x+00) y (z+00) (x+11) y (z+11) (if t="green" then "emerald_block" else t+"_wool")
                if t = "blue" then
                    yield sprintf "execute unless $SCORE(TEMP=1) run fill %d %d %d %d %d %d %s replace clay" (x+12) y (z+00) (x+22) y (z+11) (if t="green" then "emerald_block" else t+"_wool")
                if t = "green" then
                    yield sprintf "execute unless $SCORE(TEMP=1) run fill %d %d %d %d %d %d %s replace clay" (x+00) y (z+12) (x+11) y (z+22) (if t="green" then "emerald_block" else t+"_wool")
                if t = "yellow" then
                    yield sprintf "execute unless $SCORE(TEMP=1) run fill %d %d %d %d %d %d %s replace clay" (x+12) y (z+12) (x+22) y (z+22) (if t="green" then "emerald_block" else t+"_wool")
                // update win conditions (add to team score of row/col/diag)
                yield sprintf "scoreboard players add $ENTITY %sWinRow%c 1" t s.[1]
                yield sprintf "scoreboard players add $ENTITY %sWinCol%c 1" t s.[0]
                if s.[0] = s.[1] then
                    yield sprintf "scoreboard players add $ENTITY %sWinBkSlh 1" t
                if (int s.[0] - int '0') = 6 - (int s.[1] - int '0') then
                    yield sprintf "scoreboard players add $ENTITY %sWinSlash 1" t
                yield sprintf "function #%s:on_%s_got_square_%s" NS t s
                |]
        yield sprintf "%s_check_for_win" t, [|
            // check for bingo
            yield sprintf "scoreboard players set $ENTITY TEMP 0"
            for i = 1 to 5 do
                yield sprintf "execute if $SCORE(%sWinRow%d=5) run scoreboard players set $ENTITY TEMP 1" t i
            for i = 1 to 5 do
                yield sprintf "execute if $SCORE(%sWinCol%d=5) run scoreboard players set $ENTITY TEMP 1" t i
            yield sprintf "execute if $SCORE(%sWinSlash=5) run scoreboard players set $ENTITY TEMP 1" t
            yield sprintf "execute if $SCORE(%sWinBkSlh=5) run scoreboard players set $ENTITY TEMP 1" t
            yield sprintf """execute if $SCORE(TEMP=1,%sGotBingo=0) run tellraw @a [%s,{"selector":"@a[team=%s]"}," got BINGO!"]""" t LEADING_WHITESPACE t
            yield sprintf "execute if $SCORE(TEMP=1,%sGotBingo=0) run function %s:got_a_win_common_logic" t NS
            yield sprintf "execute if $SCORE(TEMP=1,%sGotBingo=0) run scoreboard players set $ENTITY %sGotBingo 1" t t
            // check for twenty-no-bingo
            yield sprintf """execute if $SCORE(%sScore=20,%sGotBingo=0) run tellraw @a [%s,{"selector":"@a[team=%s]"}," got TWENTY-NO-BINGO!"]""" t t LEADING_WHITESPACE t
            yield sprintf "execute if $SCORE(%sScore=20,%sGotBingo=0) run function %s:got_a_win_common_logic" t t NS
            // check for blackout
            yield sprintf """execute if $SCORE(%sScore=25) run tellraw @a [%s,{"selector":"@a[team=%s]"}," got MEGA-BINGO!"]""" t LEADING_WHITESPACE t
            yield sprintf "execute if $SCORE(%sScore=25) run function %s:got_a_win_common_logic" t NS
            // check for lockout
            yield sprintf "scoreboard players operation $ENTITY TEMP = $ENTITY lockoutGoal"
            yield sprintf "scoreboard players operation $ENTITY TEMP -= $ENTITY %sScore" t
            yield sprintf """execute if $SCORE(optloval=1,TEMP=0) run tellraw @a [%s,{"selector":"@a[team=%s]"}," got the lockout goal!"]""" LEADING_WHITESPACE t
            yield sprintf "execute if $SCORE(optloval=1,TEMP=0) run function %s:got_a_win_common_logic" NS
            |]
    yield "got_a_win_common_logic", [|
        // put time on scoreboard
        yield "scoreboard players operation Minutes Score = $ENTITY minutes"
        yield "scoreboard players operation Seconds Score = $ENTITY seconds"
        // option to return to lobby
        yield """execute if $SCORE(opthhval=1) run tellraw @a ["You can keep playing, or"]"""
        yield """execute if $SCORE(opthhval=1) run tellraw @a [{"underlined":"true","text":"press 't' (chat), then click this line to return to the lobby","clickEvent":{"action":"run_command","value":"/trigger home set 1"}}]"""   // TODO /tellraw @a {"keybind":"key.chat"}
        yield sprintf """execute if $SCORE(opthhval=0) run tellraw @a [%s,{"underlined":"true","text":"click to go to lobby","clickEvent":{"action":"run_command","value":"/trigger home set 1"}}]""" LEADING_WHITESPACE
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
    for item in flatBingoItems do
        yield sprintf "inv/got_%s" item, [|
            let itemdatum_time_playername    = sprintf """[%s,"%s ",{"color":"gray","score":{"name":"%s","objective":"minutes"}},{"color":"gray","text":":"},{"color":"gray","score":{"name":"%s","objective":"preseconds"}},{"color":"gray","score":{"name":"%s","objective":"seconds"}}," ",{"selector":"@s"}]""" LEADING_WHITESPACE item FAKE FAKE FAKE
            let time_playername_gotanitem    = sprintf """[%s,{"color":"gray","score":{"name":"%s","objective":"minutes"}},{"color":"gray","text":":"},{"color":"gray","score":{"name":"%s","objective":"preseconds"}},{"color":"gray","score":{"name":"%s","objective":"seconds"}}," ",{"selector":"@s"}," got an item!"]""" LEADING_WHITESPACE FAKE FAKE FAKE
            let time_playername_gotthe_datum = sprintf """[%s,{"color":"gray","score":{"name":"%s","objective":"minutes"}},{"color":"gray","text":":"},{"color":"gray","score":{"name":"%s","objective":"preseconds"}},{"color":"gray","score":{"name":"%s","objective":"seconds"}}," ",{"selector":"@s"}," got the %s"]""" LEADING_WHITESPACE FAKE FAKE FAKE item
            for t in TEAMS do
                // teammates
                yield sprintf """execute if $SCORE(optyaival=2,optlwval=1) if entity @s[team=%s] run tellraw @a[team=%s] %s""" t t itemdatum_time_playername
                yield sprintf """execute if $SCORE(optyaival=2,optlwval=0) if entity @s[team=%s] run tellraw @a[team=%s] %s""" t t time_playername_gotthe_datum
                yield sprintf """execute if $SCORE(optyaival=1) if entity @s[team=%s] run tellraw @a[team=%s] %s""" t t time_playername_gotanitem
                // other teams
                yield sprintf """execute if $SCORE(optoaival=2,optlwval=1) if entity @s[team=%s] run tellraw @a[team=!%s] %s""" t t itemdatum_time_playername
                yield sprintf """execute if $SCORE(optoaival=2,optlwval=0) if entity @s[team=%s] run tellraw @a[team=!%s] %s""" t t time_playername_gotthe_datum
                yield sprintf """execute if $SCORE(optoaival=1) if entity @s[team=%s] run tellraw @a[team=!%s] %s""" t t time_playername_gotanitem
            |]
    for s in SQUARES do
        if flatBingoItems.Length >= 128 then
            failwith "bad binary search"
        let check_and_display(prefix, n, name) = [|
            // Note - profiling suggests this guard does not help before 'clear': if entity @s[nbt={Inventory:[{id:"minecraft:%s"}]}] 
            yield sprintf """%sexecute if $SCORE(square%s=%d) store success score $ENTITY gotAnItem run clear @s %s 1""" prefix s n name
            yield sprintf """%sexecute if $SCORE(square%s=%d,gotAnItem=1) run function %s:inv/got_%s""" prefix s n NS name
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
                    sprintf """execute if $SCORE(square%s=%d..%d) run function %s:inv/check%s_%d_%d""" s lo mid NS s lo mid
                    sprintf """execute if $SCORE(square%s=%d..%d) run function %s:inv/check%s_%d_%d""" s (mid+1) hi NS s (mid+1) hi
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
    let otherChest1 =
        let sb = new System.Text.StringBuilder("""{CustomName:"\"Possible items (column bins)\"",Items:[""")
        for i = 0 to 8 do
            for j = 0 to 2 do
                let item = bingoItems.[i].[j]
                sb.Append(sprintf """{Slot:%db,id:"%s",Count:%db},""" (i+(9*j)) item 1 ) |> ignore
        let s = sb.ToString()
        s.Substring(0, s.Length-1) + "]}"
    let otherChest2 =
        let sb = new System.Text.StringBuilder("""{CustomName:"\"Possible items (column bins)\"",Items:[""")
        for i = 9 to 17 do
            for j = 0 to 2 do
                let item = bingoItems.[i].[j]
                sb.Append(sprintf """{Slot:%db,id:"%s",Count:%db},""" (i-9+(9*j)) item 1 ) |> ignore
        let s = sb.ToString()
        s.Substring(0, s.Length-1) + "]}"
    let otherChest3 =
        let sb = new System.Text.StringBuilder("""{CustomName:"\"Possible items (column bins)\"",Items:[""")
        for i = 18 to 26 do
            for j = 0 to 2 do
                let item = bingoItems.[i].[j]
                sb.Append(sprintf """{Slot:%db,id:"%s",Count:%db},""" (i-18+(9*j)) item 1 ) |> ignore
        let s = sb.ToString()
        s.Substring(0, s.Length-1) + "]}"
    let otherChest4 =
        let sb = new System.Text.StringBuilder("""{CustomName:"\"Possible items (column bins)\"",Items:[""")
        for i = 27 to bingoItems.Length-1 do
            for j = 0 to 2 do
                let item = bingoItems.[i].[j]
                sb.Append(sprintf """{Slot:%db,id:"%s",Count:%db},""" (i-27+(9*j)) item 1 ) |> ignore
        let s = sb.ToString()
        s.Substring(0, s.Length-1) + "]}"
    "make_item_chests",[|
        let x = 61
        let y = 25
        let z = 66
        yield sprintf "setblock %d %d %d chest" x y z
        yield sprintf "data merge block %d %d %d %s" x y z otherChest1
        let x = x + 2
        yield sprintf "setblock %d %d %d chest" x y z
        yield sprintf "data merge block %d %d %d %s" x y z otherChest2
        let x = x + 2
        yield sprintf "setblock %d %d %d chest" x y z
        yield sprintf "data merge block %d %d %d %s" x y z otherChest3
        let x = x + 2
        yield sprintf "setblock %d %d %d chest" x y z
        yield sprintf "data merge block %d %d %d %s" x y z otherChest4
    |]

///////////////////////////////////////////////////////////////////////////////

let TEMPLOC = "24 24 24"
let TEMPLOCSEL = "x=24,y=24,z=24,distance=..1"
let cardgen_objectives = [|
    yield "CARDGENTEMP"
    yield "squaresPlaced"                              // how many squares have we already completed filling on the card?
    yield "numRemainingBins"                           // how many available bins remain to choose from?
    yield "arrayIndex"                                 // for a collection of cgAEC entities, they have values [0..numRemainingBins-1]
    yield "binNumber"                                  // for a collection of cgAEC entities, they refer to a bin number [0..bingoItems.Length-1]
    yield "numItemsInBin"                              // for a collection of cgAEC entities, says how many choosables in the bin (1, 2, or 3)
    |]
let cardgen_functions = [|
    yield "cardgen_init", [|
        for o in cardgen_objectives do
            yield sprintf "scoreboard objectives add %s dummy" o
        for i = 0 to flatBingoItems.Length-1 do
            yield sprintf "scoreboard objectives add okItem%03d dummy" i   // is this item choosable? (way to enable only a subset of items to appear on card)
        |]
    yield "cardgen_init_chooseable", [|
        for i = 0 to flatBingoItems.Length-1 do
            yield sprintf "# %03d %s" i flatBingoItems.[i]
            yield sprintf "scoreboard players set $ENTITY okItem%03d 1" i
        |]
    yield "cardgen_prepare_bins",[|
        yield "scoreboard players set $ENTITY numRemainingBins 0"
        for i = 0 to bingoItems.Length-1 do
            // how many of the items in this bin are choosable?
            yield sprintf "scoreboard players set $ENTITY TEMP 0"  
            yield sprintf "execute if $SCORE(okItem%03d=1) run scoreboard players add $ENTITY TEMP 1" (itemIndex(bingoItems.[i].[0]))
            yield sprintf "execute if $SCORE(okItem%03d=1) run scoreboard players add $ENTITY TEMP 1" (itemIndex(bingoItems.[i].[1]))
            yield sprintf "execute if $SCORE(okItem%03d=1) run scoreboard players add $ENTITY TEMP 1" (itemIndex(bingoItems.[i].[2]))
            // if any, add the bin
            yield sprintf "execute if $SCORE(TEMP=1..) run summon area_effect_cloud %s {Duration:1,Tags:[newAEC,cgAEC]}" TEMPLOC
            yield sprintf "execute if $SCORE(TEMP=1..) run scoreboard players operation @e[%s,tag=newAEC] arrayIndex = $ENTITY numRemainingBins" TEMPLOCSEL
            yield sprintf "execute if $SCORE(TEMP=1..) run scoreboard players operation @e[%s,tag=newAEC] numItemsInBin = $ENTITY TEMP" TEMPLOCSEL
            yield sprintf "execute if $SCORE(TEMP=1..) run scoreboard players set @e[%s,tag=newAEC] binNumber %d" TEMPLOCSEL i
            yield sprintf "execute if $SCORE(TEMP=1..) run tag @e[%s,tag=newAEC] remove newAEC" TEMPLOCSEL
            yield sprintf "execute if $SCORE(TEMP=1..) run scoreboard players add $ENTITY numRemainingBins 1"
        |]
    yield "cardgen_do_it",[|
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
        |]
    yield "cardgen_choose1", [|
        //yield sprintf """tellraw @a ["in choose1, numRemainingBins=",%s]""" (Utilities.tellrawScoreSelectorENTITY("numRemainingBins"))
        // pick a number [0..numRemainingBins-1]
        yield sprintf "scoreboard players operation $ENTITY PRNG_MOD = $ENTITY numRemainingBins"
        yield sprintf "function %s:prng" NS
        yield sprintf "scoreboard players operation $ENTITY CARDGENTEMP = $ENTITY PRNG_OUT"
        // find that entity to execute as, run body
        yield sprintf "execute as @e[%s,tag=cgAEC] if score @s arrayIndex = $ENTITY CARDGENTEMP run function %s:cg/cardgen_choose1_body" TEMPLOCSEL NS
        // used a bin, one fewer to choose among
        yield sprintf "scoreboard players remove $ENTITY numRemainingBins 1"
        |]
    yield "cg/cardgen_choose1_body", [|
        //yield sprintf """tellraw @a ["in choose1_body, binNumber=",%s]""" (Utilities.tellrawScoreSelector("@s","binNumber"))
        // call the bin that corresponds to @s
        for i = 0 to bingoItems.Length-1 do
            yield sprintf "execute if score @s binNumber matches %d run function %s:cg/cardgen_bin%02d" i NS i
        // now that this bin is used, shift the rest of the array down
        yield sprintf "scoreboard players operation $ENTITY CARDGENTEMP = @s arrayIndex"
        yield sprintf "execute as @e[%s,tag=cgAEC] if score @s arrayIndex > $ENTITY CARDGENTEMP run scoreboard players remove @s arrayIndex 1" TEMPLOCSEL
        yield sprintf "scoreboard players set @s arrayIndex -1"  // -1 is moral equivalent of 'killing' this entity now that it's been used
        |]
    for i = 0 to bingoItems.Length-1 do
        yield sprintf "cg/cardgen_bin%02d" i, [|
            // pick a number [0..numItemsInBin-1]
            yield sprintf "scoreboard players operation $ENTITY PRNG_MOD = @s numItemsInBin"
            yield sprintf "function %s:prng" NS
            yield sprintf "scoreboard players operation $ENTITY CARDGENTEMP = $ENTITY PRNG_OUT"
            //yield sprintf """tellraw @a ["in cg...bin, choosing ",%s," of ",%s]""" (Utilities.tellrawScoreSelectorENTITY("CARDGENTEMP")) (Utilities.tellrawScoreSelector("@s","numItemsInBin"))
            // walk down the items in this bin... for each
            //  - if okItem && CARDGENTEMP==0 then pick it and CARDGENTEMP--
            //  - if okItem && CARDGENTEMP<>0 then CARDGENTEMP--
            for j = 0 to 2 do
                let index = itemIndex(bingoItems.[i].[j])
                yield sprintf """execute if $SCORE(okItem%03d=1,CARDGENTEMP=0) at @e[tag=sky] run setblock ~ ~ ~ minecraft:structure_block{posX:0,posY:0,posZ:0,sizeX:17,sizeY:2,sizeZ:17,mode:"LOAD",name:"test:%s"}""" index bingoItems.[i].[j]
                yield sprintf """execute if $SCORE(okItem%03d=1,CARDGENTEMP=0) at @e[tag=sky] run setblock ~ ~1 ~ minecraft:redstone_block""" index
                for x = 1 to 5 do
                    for y = 1 to 5 do
                        yield sprintf "execute if $SCORE(okItem%03d=1,CARDGENTEMP=0,squaresPlaced=%d) run scoreboard players set $ENTITY square%d%d %d" index (5*(y-1)+x-1) x y index
                        let chest = if y < 4 then CHEST_THIS_CARD_2.STR else CHEST_THIS_CARD_1.STR
                        let slot = if y < 4 then (y-1)*9+x-1 else (y-4)*9+x-1
                        yield sprintf """execute if $SCORE(okItem%03d=1,CARDGENTEMP=0,squaresPlaced=%d) run replaceitem block %s container.%d %s""" index (5*(y-1)+x-1) chest slot bingoItems.[i].[j]
                yield sprintf """execute if $SCORE(okItem%03d=1) run scoreboard players remove $ENTITY CARDGENTEMP 1""" index
            yield sprintf "scoreboard players add $ENTITY squaresPlaced 1"
            |]
    yield "cardgen_makecard", [|
        yield sprintf "kill @e[tag=sky]"
        yield sprintf """summon armor_stand 5 %d 3 {CustomName:"\"sky\"",Tags:["sky"],NoGravity:1,Invulnerable:1,Invisible:1}""" ART_HEIGHT
        yield sprintf "scoreboard players set $ENTITY squaresPlaced 0"
        for i = 0 to bingoItems.Length-1 do
            yield sprintf "scoreboard players set $ENTITY bin%02d 0" i
        // refresh bingo art area
        yield sprintf "fill 0 %d -1 127 %d 118 clay" ART_HEIGHT ART_HEIGHT
        yield sprintf "fill 0 %d -1 127 %d 118 air" (ART_HEIGHT+1) (ART_HEIGHT+1)
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
        // chest of items on this card
        yield sprintf "setblock %s air" CHEST_THIS_CARD_1.STR
        yield sprintf "setblock %s air" CHEST_THIS_CARD_2.STR
        yield sprintf "setblock %s chest[facing=east,type=left]" CHEST_THIS_CARD_1.STR
        yield sprintf "setblock %s chest[facing=east,type=right]" CHEST_THIS_CARD_2.STR
        // pick items and place structure art
        yield sprintf "function %s:cardgen_prepare_bins" NS
        yield sprintf """execute unless $SCORE(numRemainingBins=25..) run tellraw @a ["There are not enough bins of items to populate a full card; enable more items"]"""
        yield sprintf "execute if $SCORE(numRemainingBins=25..) run function %s:cardgen_do_it" NS
        // make a 'pretty' under-side for lobby ceiling
        yield sprintf "clone 0 %d 0 127 %d 118 0 %d 0" ART_HEIGHT ART_HEIGHT ART_HEIGHT_UNDER
        yield sprintf "clone 0 %d 0 127 %d 118 0 %d 0 masked" (ART_HEIGHT+1) (ART_HEIGHT+1) ART_HEIGHT_UNDER
        // initialize checker values for this card's items
        yield sprintf "function %s:checker_new_card" NS
        |]
    |]
let cardgen_compile() = // TODO this is really full game, naming/factoring...
    let r = [|
        yield! cardgen_functions
        yield! checker_functions
        yield! choosable_functions
        yield! game_functions
        yield! map_update_functions
        yield "on_lobby_respawn", [|
            // enable triggers (for click-in-chat-to-tp-home stuff)  TODO factor into a function
            "scoreboard players set @a home 0"
            "scoreboard players enable @a home"
            // players may dead
            "execute store result score @s TEMP run data get entity @s Health 100.0"
            // lobby respawn rules // TODO should this be effect in area of lobby?
            "execute if entity @s[scores={TEMP=1..}] run effect give @s minecraft:night_vision 99999 1 true"
            """execute if entity @s[scores={TEMP=1..}] run replaceitem entity @s weapon.offhand minecraft:filled_map{display:{Name:"\"BINGO Card\""},map:0} 32""" // unused: way to test offhand non-empty - scoreboard players set @p[nbt={Inventory:[{Slot:-106b}]}] offhandFull 1
            "execute if entity @s[scores={TEMP=1..}] run scoreboard players set @s Deaths 0"
            |]
        yield "on_respawn", [|
            // enable triggers (for click-in-chat-to-tp-home stuff)  TODO factor into a function
            "scoreboard players set @a home 0"
            "scoreboard players enable @a home"
            // players may dead
            "execute store result score @s TEMP run data get entity @s Health 100.0"
            // custom respawn equipment
            "execute if entity @s[scores={TEMP=1..}] if $SCORE(optnvval=1) run effect give @s minecraft:night_vision 99999 1 true"
            "execute if entity @s[scores={TEMP=1..}] if $SCORE(optdsval=1) run replaceitem entity @s armor.feet minecraft:leather_boots{Unbreakable:1,ench:[{lvl:3s,id:8s},{lvl:1s,id:10s},{lvl:1s,id:71s}]} 1"
            "execute if entity @s[scores={TEMP=1..}] if $SCORE(optboatval=1) run give @s minecraft:oak_boat 1"
            """execute if entity @s[scores={TEMP=1..}] run replaceitem entity @s weapon.offhand minecraft:filled_map{display:{Name:"\"BINGO Card\""},map:0} 32""" // unused: way to test offhand non-empty - scoreboard players set @p[nbt={Inventory:[{Slot:-106b}]}] offhandFull 1
            "execute if entity @s[scores={TEMP=1..}] run scoreboard players set @s Deaths 0"
            |]
        yield "have_no_map", [|
            sprintf "execute if score @s playerNum = $ENTITY tickNum run function %s:have_no_map_core" NS
            |]
        yield "have_no_map_core", [|
            // players may dead
            "execute store result score @s TEMP run data get entity @s Health 100.0"
            sprintf """execute if entity @s[scores={TEMP=1..}] unless entity @s[nbt={Inventory:[{id:"minecraft:filled_map",tag:{map:0}}]}] run function %s:ensure_maps""" NS  // NOTE may be expensive...
            |]
        yield "first_time_player", [|
            "tag @s add playerHasBeenSeen"
            sprintf "teleport @s %s" LOBBY
            "effect give @s minecraft:night_vision 99999 1 true"
            "recipe give @s *"
            "advancement grant @s everything"
            sprintf "advancement revoke @s only %s:xh" NS
            |]
        yield "find_dir_to_spawn_body", [|
            (* Compute which direction spawn is relative to the player's XZ
                    1
                  8   2
                7   x   3
                  6   4
                    5
               so that we can display an arrow on the actionbar that points at spawn.
            *)
            // called as nonuuid entity
            "teleport @s @p[tag=dirGuy]"
            // move him to spawn xz (doesn't matter if chunks loaded)
            "execute store result entity @s Pos[0] double 1.0 run scoreboard players get @p[tag=dirGuy] xspawn" 
            "execute store result entity @s Pos[2] double 1.0 run scoreboard players get @p[tag=dirGuy] zspawn" 
            // face him at player
            "execute at @s run teleport @s ~ ~ ~ facing entity @p[tag=dirGuy]"
            // move him back home now that we got our facing data
            "execute positioned 67 4 67 run teleport @s ~ ~ ~"   // TODO factor this location
            // convert Rotation to score: TEMP = entity, x = player
            "execute store result score @s TEMP run data get entity @s Rotation[0] 1.0" 
            "execute store result score @s x run data get entity @p[tag=dirGuy] Rotation[0] 1.0" 
            "scoreboard players operation @s TEMP -= @s x"
            // get number into range 0-360
            "scoreboard players operation @s TEMP += $ENTITY THREE_SIXTY"
            "scoreboard players operation @s TEMP += $ENTITY THREE_SIXTY"
            "scoreboard players operation @s TEMP += $ENTITY THREE_SIXTY"
            "scoreboard players operation @s TEMP %= $ENTITY THREE_SIXTY"
            "scoreboard players set @s spawnDir 5"
            "scoreboard players set @s[scores={TEMP=23..67}] spawnDir 6"
            "scoreboard players set @s[scores={TEMP=68..112}] spawnDir 7"
            "scoreboard players set @s[scores={TEMP=113..157}] spawnDir 8"
            "scoreboard players set @s[scores={TEMP=158..202}] spawnDir 1"
            "scoreboard players set @s[scores={TEMP=203..247}] spawnDir 2"
            "scoreboard players set @s[scores={TEMP=248..292}] spawnDir 3"
            "scoreboard players set @s[scores={TEMP=293..337}] spawnDir 4"
            // done
            "scoreboard players operation @p[tag=dirGuy] spawnDir = @s spawnDir"
            |]
        yield "find_dir_to_spawn", [|
            "tag @s add dirGuy"
            sprintf "execute as @e[%s] run function %s:find_dir_to_spawn_body" NONUUID_TAG NS
            "tag @s remove dirGuy"
            |]
        yield "theloop", [|
            yield sprintf "execute if $SCORE(gameInProgress=2) run function %s:update_time" NS
            yield sprintf "execute if $SCORE(gameInProgress=2) run function %s:map_update_tick" NS
            yield sprintf "execute as @a[scores={home=1}] run function %s:go_home" NS
            yield sprintf "execute as @a[tag=!playerHasBeenSeen] run function %s:first_time_player" NS
            yield sprintf "execute if $SCORE(gameInProgress=0) as @p[scores={PlayerSeed=0..}] run function %s:set_seed" NS
            yield sprintf "execute unless $SCORE(gameInProgress=1) as @a run function %s:have_no_map" NS
            yield sprintf "execute if $SCORE(gameInProgress=2) as @a[scores={Deaths=1..}] run function %s:on_respawn" NS
            yield sprintf "execute if $SCORE(gameInProgress=0) as @a[scores={Deaths=1..}] run function %s:on_lobby_respawn" NS
            yield sprintf "execute if $SCORE(gameInProgress=2) as @a run function %s:check_inventory" NS 
            yield sprintf "execute if $SCORE(gameInProgress=0) as @a run function %s:config_loop" NS
            yield "scoreboard players add @a ticksSinceGotMap 1"
            // throttling infrastructure (so some things don't run for every player every tick)
            yield "scoreboard players add $ENTITY tickNum 1"
            yield "execute if score $ENTITY tickNum >= $ENTITY numActivePlayers run scoreboard players set $ENTITY tickNum 0"
//            if PROFILE then
//                yield sprintf """tellraw @a [{"score":{"name":"@e[%s]","objective":"LINES"}}]""" ENTITY_TAG 
            |]
        yield "prng", prng
        yield "prng_init", prng_init()
        yield makeItemChests()
        yield "preinit",[|
            yield sprintf "execute in overworld run teleport @a %s" LOBBY
            yield "say you only need to call preinit when things have gone awry... killing all non-player entities..."
            yield "kill @e[type=!player]"
            |]
        yield "init_body",[|
            yield "gamerule doDaylightCycle true"
            yield "gamerule doWeatherCycle false"
            yield "gamerule sendCommandFeedback false"
            yield "gamerule commandBlockOutput false"
            yield "gamerule logAdminCommands false"
            yield "gamerule announceAdvancements false"
            yield "gamerule disableElytraMovementCheck true"
            yield "gamerule maxCommandChainLength 999999"
            yield "gamerule spawnRadius 1"
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
            yield """give @p minecraft:filled_map{display:{Name:"\"BINGO Card\""},map:0} 32"""
            |]
        yield "init",[|
            yield sprintf "function %s:init_body" NS
            |]
        |]
    printfn "writing functions..."
    // art structures in separate pack - TODO note that NS for them is effectively hardcoded to 'test' right now
    let artPack = """C:\Users\Admin1\Desktop\BingoArt.zip"""
    System.IO.File.Copy(artPack, System.IO.Path.Combine(FOLDER,"""datapacks\BingoArt.zip"""), true)
    // bingo pack
    writeExtremeHillsDetection(pack)
    Utilities.writeConfigOptionsFunctions(pack, NS, CFG, bingoConfigBook, bingoConfigBookTag, (fun (ns,n,c) -> compiler.Compile(ns,n,c)), sprintf "%s" FAKE)
    let r = [|
        for name,code in r do
            yield! compiler.Compile(NS, name, code)
        |]
    for ns,name,code in r do
        pack.WriteFunction(ns,name,code)
    for ns,name,code in compiler.GetCompilerLoadTick() do
        pack.WriteFunction(ns, name, code)
    pack.SaveToDisk()

//////////////////////////////////////////////////
// Possible future/OOB features

// hungry-peaceful-bingo: You could simulate a hungry peaceful mode by having a dozen repeating command blocks which do e.g. "tp @e[type=skeleton] ~ ~-250 ~" to constantly teleport each type 
// of hostile mob into the void.  I guess enderpearl, slimeball, spider eye, and fermented spider eye are the only items you can't get in peaceful now?

// triple-play mode? get a row, column and a diagonal to win? interesting strategy to plan/optimize?

// multiplayer where teams spawn nearby (pvp etc)

// speed uhc stuff, like auto-smelting, etc?

// invisibility could be a fun option for an OOB game mode

// what about lockout with steals? like, if other team has X, and you get it, you steal it (you get square, they lose it and point), they can steal back?

