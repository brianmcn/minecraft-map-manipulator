module Raycast

let STEP = 1.0 / 128.0
// TODO fix back
//let MAX_STEPS = 65536
//let STEP_ARRAY = [| 65536; 32768; 16384; 8192; 4096; 2048; 1024; 512; 256; 128; 64; 32; 16; 8; 4; 2; 1 |]
let MAX_STEPS = 16384
let STEP_ARRAY = [| 16384; 8192; 4096; 2048; 1024; 512; 256; 128; 64; 32; 16; 8; 4; 2; 1 |]
let MAX_DIST = float MAX_STEPS * STEP * 2.0

//////////////////////////////////////

//   /tp Lorgon111 1.909 56.0 -7.635 -53.1 26.9
let deg2rad(d) = d * System.Math.PI / 180.0
let x,y,z = 1.909, 56.0 + 1.62, -7.635
let rotx, roty = deg2rad(26.7), deg2rad(-53.1)
let mutable curx, cury, curz = x, y, z
let reset() =
    curx <- x
    cury <- y
    curz <- z
let step(n) =
    curx <- curx - (float n) * STEP * cos(roty) * sin(rotx)
    cury <- cury - (float n) * STEP * sin(roty)
    curz <- curz + (float n) * STEP * cos(roty) * cos(rotx)
let simulate() =
    printfn "%f, %f, %f" curx cury curz
    printfn "%f, %f, %f" (floor curx) (floor cury) (floor curz)
    step(10*128)
    printfn "%f, %f, %f" curx cury curz
    printfn "%f, %f, %f" (floor curx) (floor cury) (floor curz)

    reset()
    let temp = floor curx
    let mutable steps = 1
    for s in STEP_ARRAY do
        step(s)
        if floor(curx) = temp then
            steps <- steps + s
        else
            step(-s)
    step(1)
    let initx = steps

    let temp = floor curx
    let mutable steps = 1
    for s in STEP_ARRAY do
        step(s)
        if floor(curx) = temp then
            steps <- steps + s
        else
            step(-s)
    let xspc = steps

    reset()
    let temp = floor cury
    let mutable steps = 1
    for s in STEP_ARRAY do
        step(s)
        if floor(cury) = temp then
            steps <- steps + s
        else
            step(-s)
    step(1)
    let inity = steps

    let temp = floor cury
    let mutable steps = 1
    for s in STEP_ARRAY do
        step(s)
        if floor(cury) = temp then
            steps <- steps + s
        else
            step(-s)
    let yspc = steps

    reset()
    let temp = floor curz
    let mutable steps = 1
    for s in STEP_ARRAY do
        step(s)
        if floor(curz) = temp then
            steps <- steps + s
        else
            step(-s)
    step(1)
    let initz = steps

    let temp = floor curz
    let mutable steps = 1
    for s in STEP_ARRAY do
        step(s)
        if floor(curz) = temp then
            steps <- steps + s
        else
            step(-s)
    let zspc = steps

    // MC says        init x: 432 init y:  61 init z: 152    xspc: 471   yspc: 160   zspc: 239
    // F# says                432          61         152          474         160         239
    printfn "%d %d %d   %d %d %d" initx inity initz xspc yspc zspc

    reset()
    let mutable xuntil, yuntil, zuntil = initx, inity, initz
    let mutable cursteps = 0
    while cursteps < MAX_STEPS do
        printfn "%6d   %f, %f, %f      %d %d %d" cursteps (floor curx) (floor cury) (floor curz) xuntil yuntil zuntil
        if xuntil < yuntil && xuntil < zuntil then
            cursteps <- cursteps + xuntil
            yuntil <- yuntil - xuntil
            zuntil <- zuntil - xuntil
            xuntil <- xspc
            curx <- curx - 1.0
        elif yuntil < zuntil then
            cursteps <- cursteps + yuntil
            xuntil <- xuntil - yuntil
            zuntil <- zuntil - yuntil
            yuntil <- yspc
            cury <- cury + 1.0
        else
            cursteps <- cursteps + zuntil
            xuntil <- xuntil - zuntil
            yuntil <- yuntil - zuntil
            zuntil <- zspc
            curz <- curz + 1.0

//////////////////////////////////////

let NS = "raycast"

let FOLDER = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\raycast"""
let allDirsEnsured = new System.Collections.Generic.HashSet<_>()
let writeFunctionToDisk(name,code) =
    let DPDIR = System.IO.Path.Combine(FOLDER,"""datapacks\RaycastPack""")
    let DIR = System.IO.Path.Combine(FOLDER,DPDIR+"""\data\"""+NS+"""\functions""")
    let FIL = System.IO.Path.Combine(DIR,sprintf "%s.mcfunction" name)
    let dir = System.IO.Path.GetDirectoryName(FIL)
    if allDirsEnsured.Add(dir) then
        System.IO.Directory.CreateDirectory(dir) |> ignore
    System.IO.File.WriteAllLines(FIL, code)
    let mcmetatext = """{
   "pack": {
      "pack_format": 4,
      "description": "raycast stuff"
   }
}"""
    let MCMFIL = System.IO.Path.Combine(DPDIR,"pack.mcmeta")
    System.IO.File.WriteAllText(MCMFIL, mcmetatext)

///////////////////////////////

let global_objectives = [|
    "temp"
    |]

let cs_objectives = [|
    "temp"
    "curX"     // Pos[0]
    "curY"     // Pos[1]
    "curZ"     // Pos[2]
    "steps"    // computed output
    "xspc"     // x steps per cross
    "yspc"     // y steps per cross
    "zspc"     // z steps per cross
    |]
let compute_steps = [|
    // called as&at an entity, discovers how many steps until 'x' reaches next integer, moves the entity to just past
    "steps_to_next_x",[|
        yield "execute store result score @s curX run data get entity @s Pos[0] 1.0"
        yield "scoreboard players set @s steps 1"  // loop below stops at step before we cross threshold, so this adds 1 at end
        for dx in STEP_ARRAY do
            yield sprintf "execute at @s run teleport @s ^ ^ ^%3.10f" (float dx * STEP)
            yield sprintf "execute store result score @s temp run data get entity @s Pos[0] 1.0"
//            yield sprintf """tellraw @a ["%3.10f: ",{"score":{"name":"@s","objective":"temp"}}," ",{"score":{"name":"@s","objective":"curX"}}," ",{"score":{"name":"@s","objective":"steps"}}]""" (float dx * STEP)
            yield sprintf "execute if score @s temp = @s curX run scoreboard players add @s steps %d" dx
            yield sprintf "execute at @s unless score @s temp = @s curX run teleport @s ^ ^ ^-%3.10f" (float dx * STEP)
        yield sprintf "execute at @s run teleport @s ^ ^ ^%3.10f" STEP
        |]
    // called as&at an entity, discovers how many steps until 'y' reaches next integer, moves the entity to just past
    "steps_to_next_y",[|
        yield "execute store result score @s curY run data get entity @s Pos[1] 1.0"
        yield "scoreboard players set @s steps 1"  // loop below stops at step before we cross threshold, so this adds 1 at end
        for dx in STEP_ARRAY do
            yield sprintf "execute at @s run teleport @s ^ ^ ^%3.10f" (float dx * STEP)
            yield sprintf "execute store result score @s temp run data get entity @s Pos[1] 1.0"
            yield sprintf "execute if score @s temp = @s curY run scoreboard players add @s steps %d" dx
            yield sprintf "execute at @s unless score @s temp = @s curY run teleport @s ^ ^ ^-%3.10f" (float dx * STEP)
        yield sprintf "execute at @s run teleport @s ^ ^ ^%3.10f" STEP
        |]
    // called as&at an entity, discovers how many steps until 'z' reaches next integer, moves the entity to just past
    "steps_to_next_z",[|
        yield "execute store result score @s curZ run data get entity @s Pos[2] 1.0"
        yield "scoreboard players set @s steps 1"  // loop below stops at step before we cross threshold, so this adds 1 at end
        for dx in STEP_ARRAY do
            yield sprintf "execute at @s run teleport @s ^ ^ ^%3.10f" (float dx * STEP)
            yield sprintf "execute store result score @s temp run data get entity @s Pos[2] 1.0"
            yield sprintf "execute if score @s temp = @s curZ run scoreboard players add @s steps %d" dx
            yield sprintf "execute at @s unless score @s temp = @s curZ run teleport @s ^ ^ ^-%3.10f" (float dx * STEP)
        yield sprintf "execute at @s run teleport @s ^ ^ ^%3.10f" STEP
        |]
    |]

let rc_objectives = [|
    "flipx"    // whether looking in negative x direction
    "flipy"    // whether looking in negative y direction
    "flipz"    // whether looking in negative z direction
    "xuntil"   // current x steps until next cross
    "yuntil"   // current y steps until next cross
    "zuntil"   // current z steps until next cross
    "xspc"     // x steps per cross
    "yspc"     // y steps per cross
    "zspc"     // z steps per cross
    "curstep"  // loop counter
    "maxstep"  // loop bound
    "which"    // which step to take next, x=0,y=1,z=2
    |]
let raycast = [|
    "find_x_init_and_spc", [|
        // called on tempAS starting at origin
        sprintf "execute as @s at @s run function %s:steps_to_next_x" NS
        "scoreboard players operation @e[tag=markAS] xuntil = @s steps"
        sprintf "execute as @s at @s run function %s:steps_to_next_x" NS
        "scoreboard players operation @e[tag=markAS] xspc = @s steps"
        |]
    "find_y_init_and_spc", [|
        // called on tempAS starting at origin
        sprintf "execute as @s at @s run function %s:steps_to_next_y" NS
        "scoreboard players operation @e[tag=markAS] yuntil = @s steps"
        sprintf "execute as @s at @s run function %s:steps_to_next_y" NS
        "scoreboard players operation @e[tag=markAS] yspc = @s steps"
        |]
    "find_z_init_and_spc", [|
        // called on tempAS starting at origin
        sprintf "execute as @s at @s run function %s:steps_to_next_z" NS
        "scoreboard players operation @e[tag=markAS] zuntil = @s steps"
        sprintf "execute as @s at @s run function %s:steps_to_next_z" NS
        "scoreboard players operation @e[tag=markAS] zspc = @s steps"
        |]
    "raycast", [| 
        // called as markAS or whatever at the player's eyes
        // figure out flip values
        "execute store result score @s temp run data get entity @s Rotation[0] 1.0"
        "scoreboard players set @s flipx 0"
        "scoreboard players set @s[scores={temp=1..}] flipx 1"
        "scoreboard players set @s flipz 1"
        "scoreboard players set @s[scores={temp=-90..90}] flipz 0"
        "execute store result score @s temp run data get entity @s Rotation[1] 1.0"
        "scoreboard players set @s flipy 0"
        "scoreboard players set @s[scores={temp=1..}] flipy 1"
        // init until/spc
        "teleport @e[tag=tempAS] @s"
        sprintf "execute as @e[tag=tempAS] at @s run function %s:find_x_init_and_spc" NS
        "teleport @e[tag=tempAS] @s"
        sprintf "execute as @e[tag=tempAS] at @s run function %s:find_y_init_and_spc" NS
        "teleport @e[tag=tempAS] @s"
        sprintf "execute as @e[tag=tempAS] at @s run function %s:find_z_init_and_spc" NS
        //sprintf """tellraw @a ["init x: ",{"score":{"name":"@s","objective":"xuntil"}}," init y: ",{"score":{"name":"@s","objective":"yuntil"}}," init z: ",{"score":{"name":"@s","objective":"zuntil"}}]""" // TODO
        //sprintf """tellraw @a ["xspc: ",{"score":{"name":"@s","objective":"xspc"}}," yspc: ",{"score":{"name":"@s","objective":"yspc"}}," zspc: ",{"score":{"name":"@s","objective":"zspc"}}]""" // TODO
        // run the loop
        "scoreboard players set @s curstep 0"
        sprintf "scoreboard players set @s maxstep %d" MAX_STEPS 
        sprintf "execute at @s run function %s:loop" NS
        // markAS is currently inside the first collision box, and 'which' has the direction of the last step
        "teleport @e[tag=collidemagma] @s"
        // step back 1
        "execute if entity @s[scores={which=0,flipx=0}] at @s run teleport @s ~-1 ~ ~"
        "execute if entity @s[scores={which=0,flipx=1}] at @s run teleport @s ~1 ~ ~"
        "execute if entity @s[scores={which=1,flipy=0}] at @s run teleport @s ~ ~-1 ~"
        "execute if entity @s[scores={which=1,flipy=1}] at @s run teleport @s ~ ~1 ~"
        "execute if entity @s[scores={which=2,flipz=0}] at @s run teleport @s ~ ~ ~-1"
        "execute if entity @s[scores={which=2,flipz=1}] at @s run teleport @s ~ ~ ~1"
        "teleport @e[tag=raymagma] @s"
        |]
    "loop",[|
        // TODO debugging
        "execute store result score @s curX run data get entity @s Pos[0] 1.0"
        "execute store result score @s curY run data get entity @s Pos[1] 1.0"
        "execute store result score @s curZ run data get entity @s Pos[2] 1.0"
        //sprintf """tellraw @a ["cursteps:",{"score":{"name":"@s","objective":"curstep"}}," x: ",{"score":{"name":"@s","objective":"curX"}}," y:",{"score":{"name":"@s","objective":"curY"}}," z:",{"score":{"name":"@s","objective":"curZ"}}]"""
        //sprintf """tellraw @a ["init x: ",{"score":{"name":"@s","objective":"xuntil"}}," init y: ",{"score":{"name":"@s","objective":"yuntil"}}," init z: ",{"score":{"name":"@s","objective":"zuntil"}}]""" // TODO
        // test if continue loop
        sprintf "execute if score @s curstep < @s maxstep if block ~ ~ ~ air run function %s:loop_try_x" NS
        |]
    "workaround",[|
        "execute if score @s xuntil < @s yuntil run scoreboard players set @s which 0"
        |]
    "loop_try_x",[|
        // TODO debugging
        "execute if entity @p[scores={flipx=1}] at @s run setblock ~ ~ ~ diorite"
        //sprintf """tellraw @a ["xuntil: ",{"score":{"name":"@s","objective":"xuntil"}},"  zuntil:",{"score":{"name":"@s","objective":"zuntil"}}]"""
        sprintf "scoreboard players set @s which 2"
        
        //TODO // argh MC-121934 hitting the double-if here?
        //sprintf "execute if score @s xuntil < @s zuntil if score @s xuntil < @s yuntil run scoreboard players set @s which 0"
        sprintf "execute if score @s xuntil < @s zuntil run function %s:workaround" NS

        // ARGH, no 'else', only do this if didn't pick 'x'
        // sprintf "execute if score @s yuntil < @s zuntil run scoreboard players set @s which 1"
        sprintf "execute if entity @s[scores={which=2}] if score @s yuntil < @s zuntil run scoreboard players set @s which 1"

        sprintf "execute if entity @s[scores={which=0}] run function %s:step_x" NS
        sprintf "execute if entity @s[scores={which=1}] run function %s:step_y" NS
        sprintf "execute if entity @s[scores={which=2}] run function %s:step_z" NS
        sprintf "execute at @s run function %s:loop" NS
        "execute as @e[tag=markAS] at @s align xyz run teleport @s ~0.5 ~ ~0.5" // snap to grid at end
        |]
    "step_x",[|
        "scoreboard players operation @s curstep += @s xuntil"
        "scoreboard players operation @s yuntil -= @s xuntil"
        "scoreboard players operation @s zuntil -= @s xuntil"
        "scoreboard players operation @s xuntil = @s xspc"
        "execute if entity @s[scores={flipx=0}] at @s run teleport @s ~1 ~ ~"
        "execute if entity @s[scores={flipx=1}] at @s run teleport @s ~-1 ~ ~"
        |]
    "step_y",[|
        "scoreboard players operation @s curstep += @s yuntil"
        "scoreboard players operation @s xuntil -= @s yuntil"
        "scoreboard players operation @s zuntil -= @s yuntil"
        "scoreboard players operation @s yuntil = @s yspc"
        "execute if entity @s[scores={flipy=0}] at @s run teleport @s ~ ~1 ~"
        "execute if entity @s[scores={flipy=1}] at @s run teleport @s ~ ~-1 ~"
        |]
    "step_z",[|
        "scoreboard players operation @s curstep += @s zuntil"
        "scoreboard players operation @s xuntil -= @s zuntil"
        "scoreboard players operation @s yuntil -= @s zuntil"
        "scoreboard players operation @s zuntil = @s zspc"
        "execute if entity @s[scores={flipz=0}] at @s run teleport @s ~ ~ ~1"
        "execute if entity @s[scores={flipz=1}] at @s run teleport @s ~ ~ ~-1"
        |]
    "run", [| 
        "teleport @e[tag=markAS] @p"
        // start at eye level (move feet of markAS to player eye level)
        "teleport @e[tag=markAS] ~ ~1.62 ~"
        sprintf "execute as @e[tag=markAS] at @s run function %s:raycast" NS
        |]
    |]

////////////////////////////

let main() =
    let allObjectives = [|
        yield! global_objectives
        yield! cs_objectives
        yield! rc_objectives
        |]
    let allFuncs = [|
        yield "init",[|
            yield "data merge entity @e[tag=raymagma,limit=1] {Health:0}"  // kill invincible guy
            yield "data merge entity @e[tag=collidemagma,limit=1] {Health:0}"  // kill invincible guy
            yield "kill @e[type=!player]"
            yield """summon armor_stand ~ ~ ~ {Invisible:0b,Glowing:1b,NoGravity:1b,Invulnerable:1b,Small:0b,Tags:["markAS"],CustomName:markAS}"""
            //yield """summon armor_stand ~ ~ ~ {Invisible:1b,Glowing:0b,NoGravity:1b,Invulnerable:1b,Small:1b,Tags:["tempAS"],CustomName:tempAS}"""  
            yield """summon magma_cube ~ ~ ~ {Team:RayTeam,Size:0,Silent:1,NoAI:1,DeathLootTable:"minecraft:empty",Glowing:1,Invulnerable:1,Tags:["raymagma","magma"],CustomName:raymagma}"""
            // AbsorptionAmount:3.5e38f makes it invincible even to void, so we can teleport it below y=0; must kill by setting Health=0
            yield """summon magma_cube ~ ~ ~ {Team:CollideTeam,Size:0,Silent:1,NoAI:1,DeathLootTable:"minecraft:empty",Glowing:1,Invulnerable:1,Tags:["tempAS","collidemagma","magma"],CustomName:collidemagma,AbsorptionAmount:3.5e38f}"""
            yield "effect give @e[tag=magma] invisibility 999999 1 true"
            for o in allObjectives do
                yield sprintf "scoreboard objectives add %s dummy" o
            yield "team add RayTeam"
            yield "team option RayTeam collisionRule never"  // magma_cube should not collide with player
            yield "team option RayTeam color green"  // may change to red on the fly
            yield "team add CollideTeam"  // shows non-air collision box
            yield "team option CollideTeam collisionRule never"  // magma_cube should not collide with player
            yield "team option CollideTeam color blue"
            |]
        yield! compute_steps 
        yield! raycast 
        |]
    for name,code in allFuncs do
        writeFunctionToDisk(name,code)
