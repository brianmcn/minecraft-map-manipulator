module Mandelbrot

let WOOL = [|
    "white_wool"
    "orange_wool"
    "magenta_wool"
    "light_blue_wool"
    "yellow_wool"
    "lime_wool"
    "pink_wool"
    "gray_wool"
    "light_gray_wool"
    "cyan_wool"
    "purple_wool"
    "blue_wool"
    "brown_wool"
    "green_wool"
    "red_wool"
    "black_wool"
    |]

let functions = [|
        "init",[|
            // objectives
            yield "scoreboard objectives add A dummy"  // variables
            yield "scoreboard objectives add B dummy"  // variables
            yield "scoreboard objectives add K dummy"  // constants
            yield "scoreboard objectives add CALL dummy"  // if-then-else
            // constants
            yield "scoreboard players set FOURISSQ K 64000000"
            yield "scoreboard players set INTSCALE K 4000"
            yield "scoreboard players set MAXH K 128"
            yield "scoreboard players set MAXW K 128"
            yield "scoreboard players set XSCALE K 96"
            yield "scoreboard players set YSCALE K 62"
            yield "scoreboard players set XMIN K -8400"
            yield "scoreboard players set YMIN K -4000"
            yield "scoreboard players set ROWS_PER_TICK K 4"
            // other
            yield "summon armor_stand 0 4 0 {Tags:[Cursor],NoGravity:1}"
            yield "gamerule maxCommandChainLength 999999"
            |]
        "go",[|
            "execute as @p run function ms:cps_i_start"
            |]
        "cps_i_start",[|
            "scoreboard players set $ENTITY CALL 0"
            // time measurement
#if PREEMPT
#else
            "worldborder set 10000000"
            "worldborder add 1000000 1000000"
#endif
            // actual code
            "scoreboard players set I A 0"
            "tp @e[tag=Cursor] 0 14 0"
            "fill 0 14 0 127 14 127 air"
            "fill 0 13 0 127 13 127 white_wool"
            "function ms:cps_j_start"
            |]
        "cps_j_start",[|
            "scoreboard players set $ENTITY CALL 0"
            "scoreboard players set J A 0"
            "execute as @e[tag=Cursor] at @s run tp @s ~ ~ 0"
            "function ms:cps_inner_start"
            |]
        "cps_inner_start",[|
            "scoreboard players set $ENTITY CALL 0"
            "scoreboard players operation x0 A = I A"
            "scoreboard players operation x0 A *= XSCALE K"
            "scoreboard players operation x0 A += XMIN K"
            "scoreboard players operation y0 A = J A"
            "scoreboard players operation y0 A *= YSCALE K"
            "scoreboard players operation y0 A += YMIN K"
            "scoreboard players set x A 0"
            "scoreboard players set y A 0"
            "scoreboard players set n A 0"
            "function ms:while_test"
            |]
        "while_test",[|
            "scoreboard players set $ENTITY CALL 0"
            "scoreboard players operation xsq A = x A"
            "scoreboard players operation xsq A *= x A"
            "scoreboard players operation ysq A = y A"
            "scoreboard players operation ysq A *= y A"
            "scoreboard players operation r1 A = xsq A"
            "scoreboard players operation r1 A += ysq A"
            "scoreboard players operation r1 A -= FOURISSQ K"
            "scoreboard players operation $ENTITY A = r1 A"
            "scoreboard players operation $ENTITY B = n A"
            "scoreboard players set $ENTITY CALL 1"
            "execute if entity $SCORE(A=..-1,B=..15) run function ms:cps_inner_inner"
            "execute if entity $SCORE(CALL=1) run function ms:cps_inner_finish"
            |]
        "cps_inner_inner",[|
            "scoreboard players set $ENTITY CALL 0"
            "scoreboard players operation xtemp A = xsq A"
            "scoreboard players operation xtemp A -= ysq A"
            "scoreboard players operation xtemp A /= INTSCALE K"
            "scoreboard players operation xtemp A += x0 A"
            "scoreboard players operation y A *= x A"
            "scoreboard players operation y A += y A"
            "scoreboard players operation y A /= INTSCALE K"
            "scoreboard players operation y A += y0 A"
            "scoreboard players operation x A = xtemp A"
            "scoreboard players add n A 1"
            "function ms:while_test"
            |]
        "put_color",[|
            yield "scoreboard players operation $ENTITY B = n A"
            for i = 0 to 15 do
                yield sprintf "execute if entity $SCORE(B=%d) run setblock ~ ~ ~ %s" (i+1) WOOL.[i]
            |]
        "cps_inner_finish",[|
            "scoreboard players set $ENTITY CALL 0"
            "execute at @e[tag=Cursor] run function ms:put_color"
            "scoreboard players add J A 1"
            "execute as @e[tag=Cursor] at @s run tp @s ~ ~ ~1"
//            "$NTICKSLATER(2)"  // TODO Yield
            "scoreboard players operation r1 A = J A"
            "scoreboard players operation r1 A -= MAXH K"
            "scoreboard players operation $ENTITY A = r1 A"
            "scoreboard players set $ENTITY CALL 1"
            "execute if entity $SCORE(A=..-1) run function ms:cps_inner_start"
            "execute if entity $SCORE(CALL=1) run function ms:cps_j_finish"
            |]
        "cps_j_finish",[|
            "scoreboard players set $ENTITY CALL 0"
            "scoreboard players add I A 1"
            "execute as @e[tag=Cursor] at @s run tp @s ~1 ~ ~"
            "scoreboard players operation r1 A = I A"
            "scoreboard players operation r1 A -= MAXW K"
            "scoreboard players operation $ENTITY A = r1 A"
            "scoreboard players operation $ENTITY B = I A"
            "scoreboard players operation $ENTITY B %= ROWS_PER_TICK K"
            "scoreboard players set $ENTITY CALL 1"
            "execute if entity $SCORE(A=..-1,B=0) run function ms:wait_then_j"
            "execute if entity $SCORE(A=..-1,B=1..) run function ms:cps_j_start"
            "execute if entity $SCORE(CALL=1) run function ms:cps_i_finish"
            |]
        "wait_then_j",[|
            "scoreboard players set $ENTITY CALL 0"
            "$NTICKSLATER(1)"  // TODO Yield
            "function ms:cps_j_start"
            |]
        "cps_i_finish",[|
            "scoreboard players set $ENTITY CALL 0"
            """tellraw @a ["done!"]"""
            // time measurement
#if PREEMPT
#else
            "execute store result score @e[tag=Cursor] A run worldborder get"
            "scoreboard players set Time A -10000000"
            "scoreboard players operation Time A += @e[tag=Cursor] A"
            """tellraw @a ["took ",{"score":{"name":"Time","objective":"A"}}," seconds"]"""
#endif
            |]
    |]

let main() =
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "Mandelbrot-")
    let PACK_NAME = "mandelbrot"
    let NS = "ms"

    let pack = new Utilities.DataPackArchive(world,PACK_NAME,"draw mandelbrot set")
    let compiler = new Compiler.Compiler('m','s',NS,1,100,1,false)
    let all = [|
        for name,code in functions do
            yield! compiler.Compile(NS, name, code)
        yield! compiler.GetCompilerLoadTick()
        |]
    for ns, name, code in all do
        pack.WriteFunction(ns,name,code)

    pack.WriteFunctionTagsFileWithValues("minecraft", "load", [compiler.LoadFullName;sprintf"%s:init"NS])
    pack.WriteFunctionTagsFileWithValues("minecraft", "tick", [compiler.TickFullName])
    pack.SaveToDisk()
