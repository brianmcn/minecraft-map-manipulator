module MinecraftBINGO

(*

check-for-items code should still be in-game command blocks like now


cut the tutorial for good
ignore lockout, custom modes, and item chests in initial version



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







*)