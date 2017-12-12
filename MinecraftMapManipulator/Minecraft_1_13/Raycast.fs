module Raycast

let STEP = 1.0 / 1024.0
let MAX_STEPS = 65536
let STEP_ARRAY = [| 65536; 32768; 16384; 8192; 4096; 2048; 1024; 512; 256; 128; 64; 32; 16; 8; 4; 2; 1 |]
let MAX_DIST = float MAX_STEPS * STEP * 2.0

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
            yield sprintf "execute at @s run teleport @s ^ ^ ^%f" (float dx * STEP)
            yield sprintf "execute store result score @s temp run data get entity @s Pos[0] 1.0"
//            yield sprintf """tellraw @a ["%f: ",{"score":{"name":"@s","objective":"temp"}}," ",{"score":{"name":"@s","objective":"curX"}}," ",{"score":{"name":"@s","objective":"steps"}}]""" (float dx * STEP)
            yield sprintf "execute if score @s temp = @s curX run scoreboard players add @s steps %d" dx
            yield sprintf "execute at @s unless score @s temp = @s curX run teleport @s ^ ^ ^-%f" (float dx * STEP)
        yield sprintf "execute at @s run teleport @s ^ ^ ^%f" STEP
        |]
    // called as&at an entity, discovers how many steps until 'z' reaches next integer, moves the entity to just past
    "steps_to_next_y",[|
        yield "execute store result score @s curY run data get entity @s Pos[1] 1.0"
        yield "scoreboard players set @s steps 1"  // loop below stops at step before we cross threshold, so this adds 1 at end
        for dx in STEP_ARRAY do
            yield sprintf "execute at @s run teleport @s ^ ^ ^%f" (float dx * STEP)
            yield sprintf "execute store result score @s temp run data get entity @s Pos[1] 1.0"
            yield sprintf "execute if score @s temp = @s curY run scoreboard players add @s steps %d" dx
            yield sprintf "execute at @s unless score @s temp = @s curY run teleport @s ^ ^ ^-%f" (float dx * STEP)
        yield sprintf "execute at @s run teleport @s ^ ^ ^%f" STEP
        |]
    // called as&at an entity, discovers how many steps until 'z' reaches next integer, moves the entity to just past
    "steps_to_next_z",[|
        yield "execute store result score @s curZ run data get entity @s Pos[2] 1.0"
        yield "scoreboard players set @s steps 1"  // loop below stops at step before we cross threshold, so this adds 1 at end
        for dx in STEP_ARRAY do
            yield sprintf "execute at @s run teleport @s ^ ^ ^%f" (float dx * STEP)
            yield sprintf "execute store result score @s temp run data get entity @s Pos[2] 1.0"
            yield sprintf "execute if score @s temp = @s curZ run scoreboard players add @s steps %d" dx
            yield sprintf "execute at @s unless score @s temp = @s curZ run teleport @s ^ ^ ^-%f" (float dx * STEP)
        yield sprintf "execute at @s run teleport @s ^ ^ ^%f" STEP
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
        "scoreboard players operation @e[tag=AS] xuntil = @s steps"
        sprintf "execute as @s at @s run function %s:steps_to_next_x" NS
        "scoreboard players operation @e[tag=AS] xspc = @s steps"
        |]
    "find_y_init_and_spc", [|
        // called on tempAS starting at origin
        sprintf "execute as @s at @s run function %s:steps_to_next_y" NS
        "scoreboard players operation @e[tag=AS] yuntil = @s steps"
        sprintf "execute as @s at @s run function %s:steps_to_next_y" NS
        "scoreboard players operation @e[tag=AS] yspc = @s steps"
        |]
    "find_z_init_and_spc", [|
        // called on tempAS starting at origin
        sprintf "execute as @s at @s run function %s:steps_to_next_z" NS
        "scoreboard players operation @e[tag=AS] zuntil = @s steps"
        sprintf "execute as @s at @s run function %s:steps_to_next_z" NS
        "scoreboard players operation @e[tag=AS] zspc = @s steps"
        |]
    "raycast", [| 
        // called as AS or whatever at the player
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
        """execute unless entity @e[tag=tempAS] run summon armor_stand ~ ~ ~ {NoGravity:1b,Invulnerable:1b,Small:1b,Tags:["tempAS"]}"""  
        "teleport @e[tag=tempAS] @s"
        sprintf "execute as @e[tag=tempAS] at @s run function %s:find_x_init_and_spc" NS
        "teleport @e[tag=tempAS] @s"
        sprintf "execute as @e[tag=tempAS] at @s run function %s:find_y_init_and_spc" NS
        "teleport @e[tag=tempAS] @s"
        sprintf "execute as @e[tag=tempAS] at @s run function %s:find_z_init_and_spc" NS
        // run the loop
        "scoreboard players set @s curstep 0"
        sprintf "scoreboard players set @s maxstep %d" MAX_STEPS 
        sprintf "execute at @s run function %s:loop" NS
        |]
    "loop",[|
        sprintf "execute if score @s curstep < @s maxstep if block ~ ~ ~ air run function %s:loop_try_x" NS
        |]
    "loop_try_x",[|
        //sprintf """tellraw @a ["xuntil: ",{"score":{"name":"@s","objective":"xuntil"}},"  zuntil:",{"score":{"name":"@s","objective":"zuntil"}}]"""
        sprintf "scoreboard players set @s which 2"
        sprintf "execute if score @s xuntil < @s zuntil if score @s xuntil < @s yuntil run scoreboard players set @s which 0"
        sprintf "execute if score @s yuntil < @s zuntil run scoreboard players set @s which 1"
        sprintf "execute if entity @s[scores={which=0}] run function %s:step_x" NS
        sprintf "execute if entity @s[scores={which=1}] run function %s:step_y" NS
        sprintf "execute if entity @s[scores={which=2}] run function %s:step_z" NS
        sprintf "execute at @s run function %s:loop" NS
        "execute as @e[tag=AS] at @s align xyz run teleport @s ~0.5 ~ ~0.5" // snap to grid at end
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
        // Note: if just summoned, can't tp, so fails first time, dunno any workaround
        """execute unless entity @e[tag=AS] run summon armor_stand ~ ~ ~ {Glowing:1b,NoGravity:1b,Invulnerable:1b,Tags:["AS"]}"""
        "teleport @e[tag=AS] @p"
        // TODO start at eye level
        sprintf "execute as @e[tag=AS] run function %s:raycast" NS
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
            for o in allObjectives do
                yield sprintf "scoreboard objectives add %s dummy" o
            |]
        yield! compute_steps 
        yield! raycast 
        |]
    for name,code in allFuncs do
        writeFunctionToDisk(name,code)
