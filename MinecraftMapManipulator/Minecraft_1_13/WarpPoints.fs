module WarpPoints

// a system of 'points of interest' (coordinate locations) as well as a teleporter hub, for fast travel throughout the world
// you can teleport from any POI to the hub
// you can teleport from the hub to each POI


// hub is a room full of signs, e.g. POI1, POI2, POI3, ... you can warp to any by clicking the sign
// (need to figure out exact layout)
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


// POI is a location in the world.  There will be an invisible permanent armor stand there.  
// Behavior:
//  - once the player gets close enough... ("at @p if entity @e[tag=poi,distance=..6]")
//      - text pops up with name of location and glass box appears (always was CustomName:"POI4", but CustomNameVisible:1b and ArmorItems:[{},{},{},{id:"minecraft:purple_stained_glass",Count:1b}])
//      - if they travel further away (how to efficiently detect?) or tp away, CNV and AI should change back
//          - maybe I should uuid all the POI armor_stands, so they're "cheap" to address, then each one can have a 'tick' function with "execute as 1-1-1-0-4 run function tick_poi_4" and only does work when loaded
//  - once the player gets really close... (distance=..2?)
//      - a countdown starts, so if they stay there for 2 or 3 seconds, they get teleported to the hub (known permanently fixed coords)
//      - if they leave the 'closer' radius, the countdown is reset (this is useful to prevent accidental tps when walking by/through, as well as to prevent re-tp when tping from the hub)
// Creation: 
//  - a player places a custom spawn egg (crafted out of some rare ingredient or whatnot)
//  - this gets detected (e.g. via stats to know to check this tick, and then finding the spawned entity with @e for a location) (TODO only in overworld? how detect)
//  - the block underneath the spawned entity becomes bedrock, and the 'next free POI' invisible armor stand is spawned there (POI1, POI2, POI3, whichever hasn't been used yet)
//            summon minecraft:armor_stand ~ ~1 ~ {Invisible:1b,Marker:0b,NoGravity:1b,Small:1b,CustomName:"POI 4",CustomNameVisible:1b}
//  - the coords of the POI are stored in the scoreboard, so the TP hub sign can warp to there, e.g. 
//     - function warp_to_poi4 looks up poi4x poi4y poi4z summons temp entity, stores Pos[] to there, tp player to the entity; poiny are init to -1 for all n, to tell which are used already