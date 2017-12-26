module WarpPoints

// a system of 'points of interest' (coordinate locations) as well as a teleporter hub, for fast travel throughout the world
// you can teleport from any POI to the hub
// you can teleport from the hub to each POI


// hub is a room full of signs, e.g. POI1, POI2, POI3, ... you can warp to any by clicking the sign
// (need to figure out exact layout; maybe when you TP in you're in front of an item frame holding a named book that explains it)
// need to figure out how many (max) POIs to have...
//  - for E&T, I imagine 
//     - my base (near world spawn)
//     - some farm outside spawn chunks and base
//     - stronghold or two
//     - underground mine or two
//     - nether portal by a fortress or two
//     - jungle, mesa, ice spikes, handful of rare biomes
//     - woodland mansion or two
//     - ocean monument
//     - total of maybe 16 is good?
// rather than any fancy way to name/identify e.g. POI8=jungle, I think just let player add extra sign as label above the tp-ing sign
// should be The Void biome
(* signs call this...
for i = 1 to POI_MAX do
    warp_to_poi%d:
        summon area_effect_cloud ~ ~ ~ {Duration:1,Tags:["temp"]}
        execute store result entity @e[tag=temp] Pos[0] double 1.0 run scoreboard players get $ENTITY poi%dx
        execute store result entity @e[tag=temp] Pos[1] double 1.0 run scoreboard players get $ENTITY poi%dy
        execute store result entity @e[tag=temp] Pos[2] double 1.0 run scoreboard players get $ENTITY poi%dz
        execute at @e[tag=temp] run teleport @s ~ ~ ~
*)


// POI is a location in the world.  There will be an invisible permanent armor stand there.  
// TODO should it be end_gateway? probably not, one issue is, how to tp 'back'... without a cooldown, you just end up in an infinite tp loop... also entities can use end_gateways, so creeper could come through...
// Behavior:
//  - once the player gets close enough... ("at @p if entity @e[tag=poi,distance=..6]")
//      - text pops up with name of location and glass box appears (always was CustomName:"POI4", but CustomNameVisible:1b and ArmorItems:[{},{},{},{id:"minecraft:purple_stained_glass",Count:1b}])
//      - if they travel further away (how to efficiently detect and poke?  i guess another larger distance range, like 6.1..8.0) or tp away, CNV and AI should change back
//          - maybe I should uuid all the POI armor_stands, so they're "cheap" to address, then each one can have a 'tick' function with "execute as 1-1-1-0-4 run function tick_poi_4" and only does work when loaded
//  - once the player gets really close... (distance=..2?)
//      - a countdown starts, so if they stay there for 2 or 3 seconds, they get teleported to the hub (known permanently fixed coords)
//      - if they leave the 'closer' radius, the countdown is reset (this is useful to prevent accidental tps when walking by/through, as well as to prevent re-tp when tping from the hub)
(*
state machine: 0=faraway, 1=closer, 2=very close

note: some of this logic fails if two pois are placed within a few blocks of each other

tick:
execute if @p[scores={state=0}] at @p as @e[tag=poi,distance=..5] run function getting_close
execute if @p[scores={state=1}] at @p as @e[tag=poi,distance=5..7] run function getting_far
execute if @p[scores={state=0..1}] at @p as @e[tag=poi,distance=..2] run function getting_very_close
execute if @p[scores={state=2}] at @p as @e[tag=poi,distance=2..] run function leaving_very_close
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
data merge entity @e[tag=poi,distance=..2,sort-nearest,limit=1] {CustomNameVisible:0b,ArmorItems:[{},{},{},{}]}
tp to wherever

OR

I can get rid of the state machine, and do something like

tick:
execute at @p as @e[tag=poi,sort=nearest,distance=..7,limit=1] at @s run function proc_nearest_poi

proc_nearest_poi:
scoreboard players set @p[distance=2..] countdown 60
execute if entity @p[distance=5..] run data merge entity @s {CustomNameVisible:0b,ArmorItems:[{},{},{},{}]}
execute if entity @p[distance=2..5] run data merge entity @s {CustomNameVisible:1b,ArmorItems:[{},{},{},{id:"minecraft:purple_stained_glass",Count:1b}]}
scoreboard players remove @p[distance=..2,scores={countdown=1..}] countdown 1
// consider optionally playing a build-up sound while counting down for audio feedback
// consider optionally making particles then too, this looks good on repeat:  particle minecraft:falling_dust purpur_block ~ ~1.5 ~4 0.25 0.5 0.25 1 1 force
execute as @p[distance=..2,scores={countdown=0}] run function do_teleport

do_teleport:
data merge entity @e[tag=poi,distance=..2,sort-nearest,limit=1] {CustomNameVisible:0b,ArmorItems:[{},{},{},{}]}
tp to wherever

that is, find the nearest poi, only looking nearby, and if found, do whatever... this does constantly data merge the AS when nearby, but otherwise is cheap and simple
*)
// Creation: 
//  - a player places a custom spawn egg (crafted out of some rare ingredient or whatnot) or maybe an enchanted bedrock item (stat placed "/scoreboard objectives add fdjkhds minecraft.used:minecraft.bedrock", SelectedItem to ensure was magic one... but then how find location? raycast to it? prob a tick later after stat...)
//  - this gets detected (e.g. via stats or SelectedItem to know to check this tick, and then finding the spawned entity with @e for a location) (TODO only in overworld? how detect? guess could put a cmd block at 0,0,0 in overworld and then 'execute if block 0 0 0 command_block run say in overworld')
//      - or just check very nearby entities, probably fine
//  - the block underneath the spawned entity becomes bedrock (should it?), and the 'next free POI' invisible armor stand is spawned there (POI1, POI2, POI3, whichever hasn't been used yet)
//            summon minecraft:armor_stand ~ ~1 ~ {Invisible:1b,Marker:0b,NoGravity:1b,Small:1b,CustomName:"POI 4",CustomNameVisible:1b}
//  - the coords of the POI are stored in the scoreboard, so the TP hub sign can warp to there, e.g. 
//     - function warp_to_poi4 looks up poi4x poi4y poi4z summons temp entity, stores Pos[] to there, tp player to the entity; poiny are init to -1 for all n, to tell which are used already
//  - some kind of sound plays, probably text in chat or title screen announcing name of created POI
(*

tick:
// could stats detect e.g. /scoreboard objectives add fjkdh minecraft.used:minecraft.bat_spawn_egg
execute at @p as @e[tag=summonedViaEggGuy,sort=nearest,distance=..7,limit=1] at @s run function create_new_warp_point

create_new_warp_point:
scoreboard players set $ENTITY found 0
for i = 1 to POI_MAX do
    execute unless entity $SCORE(found=1) unless entity $SCORE(poi%dy=-1) run summon armor_stand ~ ~ ~ {... Tags:["poi","poi%d","newpoi"]}
    execute unless entity $SCORE(found=1) unless entity $SCORE(poi%dy=-1) run scoreboard players set $ENTITY found 1
    execute as @e[tag=newpoi] run function create_new_warp_point_coda%d

    create_new_warp_point_coda%d:
        tag @s remove newpoi
        execute align xyz offset ~0.5 ~0.0 ~0.5 run teleport @s ~ ~ ~
        execute store result score $ENTITY poi%dx 1.0 run data get entity @s Pos[0] 1.0
        execute store result score $ENTITY poi%dy 1.0 run data get entity @s Pos[1] 1.0
        execute store result score $ENTITY poi%dz 1.0 run data get entity @s Pos[2] 1.0
        // TODO play a sound
        // TODO display a title
*)