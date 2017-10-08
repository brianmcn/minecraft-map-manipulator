module MinecraftBINGO

(*

Things to test in snapshot

    TODO verify "at @e" will loop, that is, perform the chained command for each entity
    same for 'as'
    but not for 'if/unless'

    TODO "Damage:0s" is maybe no longer the nbt of map0? check it out




check-for-items code should still be in-game command blocks like now


cut the tutorial for good
ignore lockout, custom modes, and item chests in initial version
bug: https://www.reddit.com/r/minecraftbingo/comments/74sd7m/broken_seed_spawn_in_a_waterfall_and_die_in_a_wall/
bugs & ideas from top of old file


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
 - find spawn point based on seed (maybe different logic/implementation from now? ...)
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




We need a compiler from reasonable-programming to Minecraft functions.  The compiler is mostly simple, the main things it will do include:
    $ENTITY is main scorekeeper entity (maybe e.g. "@e[tag=scoreAS]")
    $SCORE(...) is maybe e.g. "@e[tag=scoreAS,score_...]
    $NTICKSLATER(n) will
     - create a new named .mcfunction for the continuation
     - create a new scoreboard objective for it
     - set the value of e.g. @e[tag=callbackAS] in the new objective to 'n'
        - but first check the existing score was 0; this system can't register the same callback function more than once at a time, that would be an error (no re-entrancy)
     - add a hook in the gameloop that, foreach callback function in the global registry, will check the score, and
        - if the score is ..0, do nothing (unscheduled)
        - if the score is 1, call the corresponding callback function (time to continue now)
        - else subtract 1 from the score (get 1 tick closer to calling it)
    $CONTINUEASAT(entity) will
     - create a new named .mcfunction for the continuation
     - execute as entity at @s then function the new function

TODO "Damage:0s" is maybe no longer the nbt of map0? check it out
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
    TODO verify "at @e" will loop, that is, perform the chained command for each entity
    // choose a random one to be the tp'er
    "scoreboard players tag @r[tag=playerThatWantsToUpdate] add playerThatIsMapUpdating"
    // clear the 'wanting' flags
    "scoreboard players tag @a[tag=playerThatWantsToUpdate] remove playerThatWantsToUpdate"
    // kill all droppedMap entities
    "kill @e[type=Item,nbt={Item:{id:\"minecraft:filled_map\",Damage:0s}}]"
    // start the TP sequence for the chosen guy
    "execute as @p[tag=playerThatIsMapUpdating] at @s then function player_updates_map"
    |]
let player_updates_map =  // called as and at that player
    [|
    "summon area_effect_cloud ~ ~ ~ {Tags:[\"whereToTpBackTo\"],Duration:1000}"  // summon now, need to wait a tick to TP
    "$NTICKSLATER(1)"
    "$CONTINUEASAT(@p[tag=playerThatIsMapUpdating])"
    "setworldspawn ~ ~ ~"
    """tellraw @a [{"selector":"@p[tag=playerThatIsMapUpdating]"}," is updating the BINGO map"]"""
    "entitydata @e[type=!Player,r=62] {PersistenceRequired:1}"  // preserve mobs
    "tp @e[tag=whereToTpBackTo] @p[tag=playerThatIsMapUpdating]"  // a tick after summoning, tp marker to player, to preserve facing direction
    sprintf "tp @s %s 180 0" MAP_UPDATE_ROOM.STR
    "particle portal ~ ~ ~ 3 2 3 1 99 @s"
    "playsound entity.endermen.teleport ambient @a"
    "$NTICKSLATER(30)" // TODO adjust timing?
    "tp @p[tag=playerThatIsMapUpdating] @e[tag=whereToTpBackTo]"
    "$CONTINUEASAT(@p[tag=playerThatIsMapUpdating])"
    "entitydata @e[type=!Player,r=72] {PersistenceRequired:0}"  // don't leak mobs
    "particle portal ~ ~ ~ 3 2 3 1 99 @s"
    "playsound entity.endermen.teleport ambient @a"
    sprintf "setworldspawn %s" MAP_UPDATE_ROOM.STR
    "scoreboard players tag @p[tag=playerThatIsMapUpdating] remove playerThatIsMapUpdating"
    TODO keep this feature? "scoreboard players set hasAnyoneUpdatedMap S 1"
    "kill @e[tag=whereToTpBackTo]"
    |]







*)