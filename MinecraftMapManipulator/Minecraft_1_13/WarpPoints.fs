module WarpPoints

// a system of 'warp points' (coordinate locations) as well as a teleporter hub, for fast travel throughout the world
// you can teleport from any WP to the hub
// you can teleport from the hub to each WP


// hub is a room full of signs, e.g. WP1, WP2, WP3, ... you can warp to any by clicking the sign
// rather than any fancy way to name/identify e.g. WP8=jungle, I think just let player add extra sign as label above the tp-ing sign
// should be The Void biome
(* signs call this...
for i = 1 to WP_MAX do
    warp_to_wp%d:
        summon area_effect_cloud ~ ~ ~ {Duration:1,Tags:["temp"]}
        execute store result entity @e[tag=temp] Pos[0] double 1.0 run scoreboard players get $ENTITY wp%dx
        execute store result entity @e[tag=temp] Pos[1] double 1.0 run scoreboard players get $ENTITY wp%dy
        execute store result entity @e[tag=temp] Pos[2] double 1.0 run scoreboard players get $ENTITY wp%dz
        execute at @e[tag=temp] run teleport @s ~ ~ ~
*)
// maybe each time you arrive the book and the warp signs and sea lanterns and barriers are replaced, so in case you screw up and break one it comes back? only has text on signs for warp points you've created?
// folks can 'redecorate' by replacing stone or air, but not change sign locations, sea lanterns

let placeWallSignCmds x y z facing txt1 txt2 txt3 txt4 cmd isBold =
    if facing<>"north" && facing<>"south" && facing<>"east" && facing<>"west" then failwith "bad facing wall_sign"
    let bc = sprintf """,\"bold\":\"%s\",\"color\":\"%s\" """ (if isBold then "true" else "false") (if isBold then "black" else "gray")
    let c1 = if isBold && (cmd<>null) then sprintf """,\"clickEvent\":{\"action\":\"run_command\",\"value\":\"%s\"} """ cmd else ""
    [|
        sprintf "setblock %d %d %d air" x y z
        sprintf """setblock %d %d %d wall_sign[facing=%s]{Text1:"{\"text\":\"%s\"%s%s}",Text2:"{\"text\":\"%s\"%s}",Text3:"{\"text\":\"%s\"%s}",Text4:"{\"text\":\"%s\"%s}"}""" 
                    x y z facing txt1 bc c1 txt2 bc txt3 bc txt4 bc
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
let lobby_functions = [|
    "init_stone_lobby", [|
        sprintf "fill %d %d %d %d %d %d air" (WP_X-1) (WP_Y-1) (WP_Z-1) (WP_X+WP_LOBBY_LENGTH+1) (WP_Y+WP_LOBBY_HEIGHT) (WP_Z+WP_LOBBY_WIDTH)
        sprintf "fill %d %d %d %d %d %d stone hollow" (WP_X-1) (WP_Y-1) (WP_Z-1) (WP_X+WP_LOBBY_LENGTH+1) (WP_Y+WP_LOBBY_HEIGHT) (WP_Z+WP_LOBBY_WIDTH)
        |]
    "recreate_lobby_essentials", [|
        yield sprintf "fill %d %d %d %d %d %d sea_lantern" (WP_X-1) (WP_Y+1) (WP_Z-1) (WP_X+WP_LOBBY_LENGTH+1) (WP_Y+1) (WP_Z-1)
        for i = 0 to (WP_MAX-1)/2 do
            // TODO conditional text on signs, TODO clickable function
            yield! placeWallSignCmds (WP_X+4+i) (WP_Y+1) (WP_Z) "south" "WARP TO" "" "Warp Point" (sprintf"%d"(i+1)) "say click" true
            yield! placeWallSignCmds (WP_X+4+i) (WP_Y+1) (WP_Z+WP_LOBBY_WIDTH-1) "north" "WARP TO" "" "Warp Point" (sprintf"%d"(WP_MAX-i)) "say click" true
        yield sprintf "fill %d %d %d %d %d %d sea_lantern" (WP_X-1) (WP_Y+1) (WP_Z+WP_LOBBY_WIDTH+1) (WP_X+WP_LOBBY_LENGTH+1) (WP_Y+1) (WP_Z+WP_LOBBY_WIDTH)
        yield sprintf "setblock %d %d %d sea_lantern" (WP_X-1) (WP_Y+1) (WP_Z+WP_LOBBY_WIDTH/2)
        yield sprintf "setblock %d %d %d sea_lantern" (WP_X+WP_LOBBY_LENGTH+1) (WP_Y+1) (WP_Z+WP_LOBBY_WIDTH/2)
        // TODO clickable function
        yield! placeWallSignCmds (WP_X+WP_LOBBY_LENGTH) (WP_Y+1) (WP_Z+WP_LOBBY_WIDTH/2) "west" "WARP TO" "Quest for" "Everything" "Lobby" "say click" true
        // TODO item frame with written book explaining it
        |]
    |]

let main() =
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "TestSize")
    Utilities.writeDatapackMeta(world,"wp_pack","warp points")
    for name,code in lobby_functions do
        Utilities.writeFunctionToDisk(world,"wp_pack","wp",name,code)


// WP is a location in the world.  There will be an invisible permanent armor stand there.  
// TODO should it be end_gateway? probably not, one issue is, how to tp 'back'... without a cooldown, you just end up in an infinite tp loop... also entities can use end_gateways, so creeper could come through...
// Behavior:
//  - once the player gets close enough... ("at @p if entity @e[tag=wp,distance=..6]")
//      - text pops up with name of location and glass box appears (always was CustomName:"WP4", but CustomNameVisible:1b and ArmorItems:[{},{},{},{id:"minecraft:purple_stained_glass",Count:1b}])
//      - if they travel further away (how to efficiently detect and poke?  i guess another larger distance range, like 6.1..8.0) or tp away, CNV and AI should change back
//          - maybe I should uuid all the WP armor_stands, so they're "cheap" to address, then each one can have a 'tick' function with "execute as 1-1-1-0-4 run function tick_wp_4" and only does work when loaded
//  - once the player gets really close... (distance=..2?)
//      - a countdown starts, so if they stay there for 2 or 3 seconds, they get teleported to the hub (known permanently fixed coords)
//      - if they leave the 'closer' radius, the countdown is reset (this is useful to prevent accidental tps when walking by/through, as well as to prevent re-tp when tping from the hub)
(*
state machine: 0=faraway, 1=closer, 2=very close

note: some of this logic fails if two wps are placed within a few blocks of each other

tick:
execute if @p[scores={state=0}] at @p as @e[tag=wp,distance=..5] run function getting_close
execute if @p[scores={state=1}] at @p as @e[tag=wp,distance=5..7] run function getting_far
execute if @p[scores={state=0..1}] at @p as @e[tag=wp,distance=..2] run function getting_very_close
execute if @p[scores={state=2}] at @p as @e[tag=wp,distance=2..] run function leaving_very_close
scoreboard players remove @p[scores={state=2,countdown=1..}] countdown 1
exeute as @p[scores={state=2,countdown=0}] run function do_teleport

getting_close:
data merge entity @s {CustomNameVisible:1b,ArmorItems:[{},{},{},{id:"minecraft:purple_stained_glass",Count:1b}]}
scoreboard players set @p state 1

getting_far:
data merge entity @s {CustomNameVisible:0b,ArmorItems:[{},{},{},{}]}
scoreboard players set @p state 0

getting_very_close:
scoreboard players set @p countdown 60
scoreboard players set @p state 2

leaving_very_close:
scoreboard players set @p countdown 60
scoreboard players set @p state 1

do_teleport:
data merge entity @e[tag=wp,distance=..2,sort-nearest,limit=1] {CustomNameVisible:0b,ArmorItems:[{},{},{},{}]}
tp to wherever

OR

I can get rid of the state machine, and do something like

tick:
execute at @p as @e[tag=wp,sort=nearest,distance=..7,limit=1] at @s run function proc_nearest_wp

proc_nearest_wp:
scoreboard players set @p[distance=2..] countdown 60
execute if entity @p[distance=5..] run data merge entity @s {CustomNameVisible:0b,ArmorItems:[{},{},{},{}]}
execute if entity @p[distance=2..5] run data merge entity @s {CustomNameVisible:1b,ArmorItems:[{},{},{},{id:"minecraft:purple_stained_glass",Count:1b}]}
scoreboard players remove @p[distance=..2,scores={countdown=1..}] countdown 1
// consider optionally playing a build-up sound while counting down for audio feedback
// consider optionally making particles then too, this looks good on repeat:  particle minecraft:falling_dust purpur_block ~ ~1.5 ~4 0.25 0.5 0.25 1 1 force
execute as @p[distance=..2,scores={countdown=0}] run function do_teleport

do_teleport:
data merge entity @e[tag=wp,distance=..2,sort-nearest,limit=1] {CustomNameVisible:0b,ArmorItems:[{},{},{},{}]}
tp to wherever

that is, find the nearest wp, only looking nearby, and if found, do whatever... this does constantly data merge the AS when nearby, but otherwise is cheap and simple
*)
let CLOSE = "1.2"
let COUNTDOWN = 30
let nearby_behavior_functions = [|
    "proc_nearest_wp",[| // run as & at the wp armor_stand
        yield sprintf "scoreboard players set @p[distance=%s..] countdown %d" CLOSE COUNTDOWN
        yield "execute unless entity @p[distance=..5] run data merge entity @s {CustomNameVisible:0b}" //,ArmorItems:[{},{},{},{}]}"
        yield sprintf "execute if entity @p[distance=%s..5] run data merge entity @s {CustomNameVisible:1b}" CLOSE //,ArmorItems:[{},{},{},{id:"minecraft:purple_stained_glass",Count:1b}]}
        yield sprintf "scoreboard players remove @p[distance=..%s,scores={countdown=1..}] countdown 1" CLOSE
        for i = 1 to COUNTDOWN do
            yield sprintf "execute if entity @p[distance=..%s,scores={countdown=%d}] run playsound minecraft:block.note.pling block @p ~ ~ ~ 0.1 %f 0.1" CLOSE i (float(COUNTDOWN-i)*1.5/(float COUNTDOWN)+0.5)
        yield "particle minecraft:falling_dust purpur_block ~ ~1.5 ~ 0.25 0.5 0.25 1 1 force"
        yield sprintf "execute as @p[distance=..%s,scores={countdown=0}] run function wp:do_teleport" CLOSE
        |]
    "do_teleport",[|
        sprintf "data merge entity @e[tag=wp,distance=..%s,sort=nearest,limit=1] {CustomNameVisible:0b}" CLOSE //,ArmorItems:[{},{},{},{}]}
        // TODO
        "teleport @p ~ ~5 ~5"
        |]
    |]

// Creation: 
//  - a player places a custom spawn egg (crafted out of some rare ingredient or whatnot) or maybe an enchanted bedrock item (stat placed "/scoreboard objectives add fdjkhds minecraft.used:minecraft.bedrock", SelectedItem to ensure was magic one... but then how find location? raycast to it? prob a tick later after stat...)
//  - this gets detected (e.g. via stats or SelectedItem to know to check this tick, and then finding the spawned entity with @e for a location) (TODO only in overworld? how detect? guess could put a cmd block at 0,0,0 in overworld and then 'execute if block 0 0 0 command_block run say in overworld')
//      - or just check very nearby entities, probably fine
//  - the block underneath the spawned entity becomes bedrock (should it?), and the 'next free WP' invisible armor stand is spawned there (WP1, WP2, WP3, whichever hasn't been used yet)
//            summon minecraft:armor_stand ~ ~1 ~ {Invisible:1b,Marker:0b,NoGravity:1b,Small:1b,CustomName:"WP 4",CustomNameVisible:1b}
//  - the coords of the WP are stored in the scoreboard, so the TP hub sign can warp to there, e.g. 
//     - function warp_to_wp4 looks up wp4x wp4y wp4z summons temp entity, stores Pos[] to there, tp player to the entity; wpny are init to -1 for all n, to tell which are used already
//  - some kind of sound plays, probably text in chat or title screen announcing name of created WP
let creation_functions = [|
    yield "init",[|
        yield "kill @e[tag=wp]"
        yield "scoreboard objectives add found dummy"
        yield "scoreboard objectives add spawnBat minecraft.used:minecraft.bat_spawn_egg"
        for i = 1 to WP_MAX do
            yield sprintf "scoreboard objectives add wp%dx dummy" i
            yield sprintf "scoreboard objectives add wp%dy dummy" i
            yield sprintf "scoreboard objectives add wp%dz dummy" i
            yield sprintf "scoreboard players set $ENTITY wp%dy -1" i
        yield """give @p bat_spawn_egg{EntityTag:{Tags:["cwpBat"]}} 1"""  // TODO remove
        // nearby_behavior
        yield "scoreboard objectives add countdown dummy"
        |]
    yield "tick",[|
        "execute at @p[scores={spawnBat=1..}] as @e[tag=cwpBat,sort=nearest,distance=..10,limit=1] at @s run function wp:create_new_warp_point"
        // nearby_behavior
        "execute at @p as @e[tag=wp,sort=nearest,distance=..7,limit=1] at @s run function wp:proc_nearest_wp"
        |]
    yield "create_new_warp_point",[|
        yield "scoreboard players set @a spawnBat 0"
        yield "scoreboard players set $ENTITY found 0"
        for i = 1 to WP_MAX do
            yield sprintf """execute unless entity $SCORE(found=1) if entity $SCORE(wp%dy=-1) run summon armor_stand ~ ~ ~ {Invisible:1b,NoGravity:1b,Invulnerable:1b,Small:1b,CustomName:"Warp",Tags:["wp","wp%d"]}""" i i
            yield sprintf """execute unless entity $SCORE(found=1) if entity $SCORE(wp%dy=-1) as @e[tag=wp%d] run function wp:create_new_warp_point_coda%d""" i i i
        yield "say @s"
        yield "kill @s"
        |]
    for i = 1 to WP_MAX do
        yield sprintf "create_new_warp_point_coda%d" i, [|
            "execute align xyz offset ~0.5 ~0.0 ~0.5 run teleport @s ~ ~ ~"
            sprintf "execute store result score $ENTITY wp%dx run data get entity @s Pos[0] 1.0" i
            sprintf "execute store result score $ENTITY wp%dy run data get entity @s Pos[1] 1.0" i
            sprintf "execute store result score $ENTITY wp%dz run data get entity @s Pos[2] 1.0" i
            // TODO play a sound
            // TODO display a title
            """scoreboard players set $ENTITY found 1"""
            |]
    |]

let wp_c_main() =
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "TestSize")
    Utilities.writeDatapackMeta(world,"wp_pack","warp points")
    for name,code in creation_functions do
        Utilities.writeFunctionToDisk(world,"wp_pack","wp",name,code |> Array.map MC_Constants.compile)  // TODO real compile, ENTITY
    for name,code in nearby_behavior_functions do
        Utilities.writeFunctionToDisk(world,"wp_pack","wp",name,code |> Array.map MC_Constants.compile)  // TODO real compile, ENTITY
