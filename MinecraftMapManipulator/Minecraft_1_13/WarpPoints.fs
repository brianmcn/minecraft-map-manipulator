module WarpPoints

// a system of 'warp points' (coordinate locations) as well as a teleporter hub, for fast travel throughout the world
// you can teleport from any WP to the hub
// you can teleport from the hub to each WP


// hub is a room full of signs, e.g. WP1, WP2, WP3, ... you can warp to any by clicking the sign
// rather than any fancy way to name/identify e.g. WP8=jungle, I think just let player add extra sign as label above the tp-ing sign
// should be The Void biome
// maybe each time you arrive the book and the warp signs and sea lanterns and barriers are replaced, so in case you screw up and break one it comes back? only has text on signs for warp points you've created?
// folks can 'redecorate' by replacing stone or air, but not change sign locations, sea lanterns
let placeWallSignCmds prefix x y z facing txt1 txt2 txt3 txt4 cmd isBold =
    if facing<>"north" && facing<>"south" && facing<>"east" && facing<>"west" then failwith "bad facing wall_sign"
    let bc = sprintf """,\"bold\":\"%s\",\"color\":\"%s\" """ (if isBold then "true" else "false") (if isBold then "black" else "gray")
    let c1 = if isBold && (cmd<>null) then sprintf """,\"clickEvent\":{\"action\":\"run_command\",\"value\":\"%s\"} """ cmd else ""
    [|
        sprintf "%ssetblock %d %d %d air" prefix x y z
        sprintf """%ssetblock %d %d %d wall_sign[facing=%s]{Text1:"{\"text\":\"%s\"%s%s}",Text2:"{\"text\":\"%s\"%s}",Text3:"{\"text\":\"%s\"%s}",Text4:"{\"text\":\"%s\"%s}"}""" 
                    prefix x y z facing txt1 bc c1 txt2 bc txt3 bc txt4 bc
    |]

// corner of interior
let WP_X = 0
let WP_Y = 120
let WP_Z = 0
let WP_MAX = 40
// interior dimensions
let WP_LOBBY_LENGTH = WP_MAX/2 + 4
let WP_LOBBY_WIDTH = 5
let WP_LOBBY_HEIGHT = 4
let bookNBT = Utilities.writtenBookNBTString("Lorgon111","Welcome to Warp Zone!",[|
    "\"You can place a Warp Point anywhere in the world to create a two-way teleporter to/from here.\\n\\nThe signs in this lobby allow you to teleport to any Warp Point.\\n\\nAlso...\""
    "\"Check the sign at the end of the room for an extra challenge.\""
    |], null)
let lobby_functions = [|
    yield "init_stone_lobby", [|
        sprintf "fill %d %d %d %d %d %d air" (WP_X-1) (WP_Y-1) (WP_Z-1) (WP_X+WP_LOBBY_LENGTH+1) (WP_Y+WP_LOBBY_HEIGHT) (WP_Z+WP_LOBBY_WIDTH)
        sprintf "fill %d %d %d %d %d %d stone hollow" (WP_X-1) (WP_Y-1) (WP_Z-1) (WP_X+WP_LOBBY_LENGTH+1) (WP_Y+WP_LOBBY_HEIGHT) (WP_Z+WP_LOBBY_WIDTH)
        sprintf """summon item_frame %d %d %d {Facing:5b,Item:{id:"minecraft:written_book",Count:1b,tag:%s}}""" (WP_X) (WP_Y+1) (WP_Z+WP_LOBBY_WIDTH/2) bookNBT 
        |]
    yield "recreate_lobby_essentials", [|
        yield sprintf "fill %d %d %d %d %d %d sea_lantern" (WP_X-1) (WP_Y+1) (WP_Z-1) (WP_X+WP_LOBBY_LENGTH+1) (WP_Y+1) (WP_Z-1)
        for i = 0 to (WP_MAX-1)/2 do
            yield! placeWallSignCmds (sprintf"execute if $SCORE(wp%dy=0..) run "(i+1)) (WP_X+4+i) (WP_Y+1) (WP_Z) "south" "WARP TO" "" "Warp Point" (sprintf"%d"(i+1)) (sprintf"function wp:warp_to_wp%d"(i+1)) true
            yield! placeWallSignCmds (sprintf"execute if $SCORE(wp%dy=0..) run "(WP_MAX-i)) (WP_X+4+i) (WP_Y+1) (WP_Z+WP_LOBBY_WIDTH-1) "north" "WARP TO" "" "Warp Point" (sprintf"%d"(WP_MAX-i)) (sprintf"function wp:warp_to_wp%d"(WP_MAX-i)) true
            yield! placeWallSignCmds (sprintf"execute if $SCORE(wp%dy=-1) run "(i+1)) (WP_X+4+i) (WP_Y+1) (WP_Z) "south" "(not yet made)" "" "Warp Point" (sprintf"%d"(i+1)) (sprintf"function wp:warp_to_wp%d"(i+1)) false
            yield! placeWallSignCmds (sprintf"execute if $SCORE(wp%dy=-1) run "(WP_MAX-i)) (WP_X+4+i) (WP_Y+1) (WP_Z+WP_LOBBY_WIDTH-1) "north" "(not yet made)" "" "Warp Point" (sprintf"%d"(WP_MAX-i)) (sprintf"function wp:warp_to_wp%d"(WP_MAX-i)) false
        yield sprintf "fill %d %d %d %d %d %d sea_lantern" (WP_X-1) (WP_Y+1) (WP_Z+WP_LOBBY_WIDTH+1) (WP_X+WP_LOBBY_LENGTH+1) (WP_Y+1) (WP_Z+WP_LOBBY_WIDTH)
        yield sprintf "setblock %d %d %d sea_lantern" (WP_X-1) (WP_Y+1) (WP_Z+WP_LOBBY_WIDTH/2)
        yield sprintf "setblock %d %d %d sea_lantern" (WP_X+WP_LOBBY_LENGTH+1) (WP_Y+1) (WP_Z+WP_LOBBY_WIDTH/2)
        // TODO clickable function, qfe and wp lobby can be nearby each other in a The Void biome far in the world
        yield! placeWallSignCmds "" (WP_X+WP_LOBBY_LENGTH) (WP_Y+1) (WP_Z+WP_LOBBY_WIDTH/2) "west" "WARP TO" "Quest for" "Everything" "Lobby" "say click" true
        |]
    for i = 1 to WP_MAX do
        yield sprintf "warp_to_wp%d" i,[|
            """summon area_effect_cloud ~ ~ ~ {Duration:1,Tags:["tempaec"]}"""
            sprintf "execute store result entity @e[tag=tempaec,limit=1] Pos[0] double 1.0 run scoreboard players get $ENTITY wp%dx" i
            sprintf "execute store result entity @e[tag=tempaec,limit=1] Pos[1] double 1.0 run scoreboard players get $ENTITY wp%dy" i
            sprintf "execute store result entity @e[tag=tempaec,limit=1] Pos[2] double 1.0 run scoreboard players get $ENTITY wp%dz" i
            "teleport @s @e[tag=tempaec,limit=1]"
            sprintf "execute if $SCORE(wp%dd=-1) at @s in the_nether run tp @s ~ ~ ~" i
            sprintf "execute if $SCORE(wp%dd=0) at @s in overworld run tp @s ~ ~ ~" i
            sprintf "execute if $SCORE(wp%dd=1) at @s in the_end run tp @s ~ ~ ~" i
            |]
    |]

// WP is a location in the world.  There will be an invisible permanent armor stand there.  
// should it be end_gateway? probably not, one issue is, how to tp 'back'... without a cooldown, you just end up in an infinite tp loop... also entities can use end_gateways, so creeper could come through...
let CLOSE = "1.2"
let COUNTDOWN = 30
let nearby_behavior_functions = [|
    "proc_nearest_wp",[| // run as & at the wp armor_stand
        yield sprintf "scoreboard players set @p[distance=%s..] countdown %d" CLOSE COUNTDOWN
        yield "execute unless entity @p[distance=..5] run data merge entity @s {CustomNameVisible:0b}"
        yield sprintf "execute if entity @p[distance=%s..5] run data merge entity @s {CustomNameVisible:1b}" CLOSE
        yield sprintf "scoreboard players remove @p[distance=..%s,scores={countdown=1..}] countdown 1" CLOSE
        for i = 1 to COUNTDOWN do
            yield sprintf "execute if entity @p[distance=..%s,scores={countdown=%d}] run playsound minecraft:block.note.pling block @p ~ ~ ~ 0.1 %f 0.1" CLOSE i (float(COUNTDOWN-i)*1.5/(float COUNTDOWN)+0.5)
        yield "particle minecraft:falling_dust purpur_block ~ ~1.5 ~ 0.25 0.5 0.25 1 1 force"
        yield sprintf "execute as @p[distance=..%s,scores={countdown=0}] run function wp:do_teleport" CLOSE
        |]
    "do_teleport",[|
        sprintf "data merge entity @e[tag=wp,distance=..%s,sort=nearest,limit=1] {CustomNameVisible:0b}" CLOSE
        sprintf "scoreboard players set @p countdown %d" (COUNTDOWN*2)
        "tag @p add justSentToLobby"
        sprintf "execute in overworld run teleport @p %d %d %d 90 0" (WP_X+2) WP_Y (WP_Z+2)
        |]
    |]

let creation_functions = [|
    yield "init",[|
        yield "kill @e[tag=wp]"
        yield "scoreboard objectives add found dummy"
        yield "scoreboard objectives add spawnBat minecraft.used:minecraft.bat_spawn_egg"
        for i = 1 to WP_MAX do
            yield sprintf "scoreboard objectives add wp%dx dummy" i
            yield sprintf "scoreboard objectives add wp%dy dummy" i
            yield sprintf "scoreboard objectives add wp%dz dummy" i
            yield sprintf "scoreboard objectives add wp%dd dummy" i     // dimension: 0 overworld, -1 nether, 1 end
            yield sprintf "scoreboard players set $ENTITY wp%dy -1" i
        yield """give @p bat_spawn_egg{EntityTag:{Tags:["cwpBat"]},display:{Name:"\"Create Warp Point\"",Lore:["\"test\""]}} 1"""  // TODO add lore   TODO make this a recipe or whatnot
        // nearby_behavior
        yield "scoreboard objectives add countdown dummy"
        |]
    yield "tick",[|
        "execute at @p[scores={spawnBat=1..}] as @e[tag=cwpBat,sort=nearest,distance=..10,limit=1] at @s run function wp:create_new_warp_point"
        // nearby_behavior
        "execute at @p as @e[tag=wp,sort=nearest,distance=..7,limit=1] at @s run function wp:proc_nearest_wp"
        "execute at @p[tag=justSentToLobby] run function wp:recreate_lobby_essentials"
        "tag @p[tag=justSentToLobby] remove justSentToLobby"
        |]
    yield "create_new_warp_point",[|
        yield "scoreboard players set @a spawnBat 0"
        yield "scoreboard players set $ENTITY found 0"
        for i = 1 to WP_MAX do
            yield sprintf """execute unless $SCORE(found=1) if $SCORE(wp%dy=-1) run summon armor_stand ~ ~ ~ {Invisible:1b,NoGravity:1b,Invulnerable:1b,Small:1b,CustomName:"\"Warp %d\"",Tags:["wp","wp%d"]}""" i i i
            yield sprintf """execute unless $SCORE(found=1) if $SCORE(wp%dy=-1) as @e[tag=wp%d] run function wp:create_new_warp_point_coda%d""" i i i
        yield "tp @s ~ 250 ~"
        yield "$NTICKSLATER(2)"
        yield "kill @e[tag=cwpBat]" // note: bat is no longer @s!
        |]
    for i = 1 to WP_MAX do
        yield sprintf "create_new_warp_point_coda%d" i, [|
            "say hi"
            "execute align xyz positioned ~0.5 ~0.0 ~0.5 run teleport @s ~ ~ ~"
            sprintf "execute store result score $ENTITY wp%dx run data get entity @s Pos[0] 1.0" i
            sprintf "execute store result score $ENTITY wp%dy run data get entity @s Pos[1] 1.0" i
            sprintf "execute store result score $ENTITY wp%dz run data get entity @s Pos[2] 1.0" i
            sprintf "execute store result score $ENTITY wp%dd run data get entity @s Dimension 1.0" i
            "execute at @s run playsound minecraft:block.anvil.land block @a ~ ~ ~ 0.2 1.0"
            "execute at @s run playsound minecraft:block.portal.trigger block @a ~ ~ ~ 0.2 1.0"
            """scoreboard players set $ENTITY found 1"""
            |]
    |]

let wp_c_main() =
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "TestWarpPoints")
    let PACK_NAME = "wp_pack"

    let dummy = [|
        "say hello..."
        "$NTICKSLATER(40)"
        "say ...there"
        |]

    let pack = new Utilities.DataPackArchive(world, PACK_NAME, "warp points")

//    Utilities.writeDatapackMeta(world,PACK_NAME,"warp points")
    let compiler = new Compiler.Compiler('w','p',"wp",false)
    let all = [|
        for name,code in [|yield! creation_functions; yield! nearby_behavior_functions; yield! lobby_functions|] do
            yield! compiler.Compile("wp", name, code)
        yield! compiler.Compile("wp","dummy",dummy)
        yield! compiler.GetCompilerLoadTick()
        |]
    for ns, name, code in all do
        let compiledCode = code
//        Utilities.writeFunctionToDisk(world,PACK_NAME,ns,name,compiledCode)
        pack.WriteFunction(ns,name,compiledCode)

//    Utilities.writeFunctionTagsFileWithValues(world, PACK_NAME, "minecraft", "load", [compiler.LoadFullName;"wp:init"])
//    Utilities.writeFunctionTagsFileWithValues(world, PACK_NAME, "minecraft", "tick", [compiler.TickFullName;"wp:tick"])
    pack.WriteFunctionTagsFileWithValues("minecraft", "load", [compiler.LoadFullName;"wp:init"])  // TODO move init
    pack.WriteFunctionTagsFileWithValues("minecraft", "tick", [compiler.TickFullName;"wp:tick"])

    pack.SaveToDisk()

(*
    let pack = new Utilities.DataPackArchive(world, "zip_test", "testing zips")
    pack.WriteFunction("foo","fun1",[|"say foo:fun1 called"|])
    pack.WriteFunction("foo","fun2",[|"say foo:fun2 called"|])
    pack.SaveToDisk()
*)


// TODO is a warp egg too OP?  Kinda get-out-of-jail-free, e.g. low on hearts v enderdragon, just place it and tp home? consider if it needs more warm-up time to create, or restrictions?


(*
Now that we can get the length of strings in a score, function trees to look up items became better :stuck_out_tongue:(edited)
[11:06 AM] Pepijn: Just have to get the length and than only check the items which can have that length, instead of all the items.
*)