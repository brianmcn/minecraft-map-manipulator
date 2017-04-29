module Mandelbrot

open NoLatencyCompiler

let objectivesAndConstants = [|
    // objectives
    yield "scoreboard objectives add A dummy"  // variables
#if ADVANCEMENTS
    yield "scoreboard objectives add B dummy"  // variables
#endif
    yield "scoreboard objectives add K dummy"  // constants
    // constants
    yield "scoreboard players set FOURISSQ K 64000000"
    yield "scoreboard players set INTSCALE K 4000"
    yield "scoreboard players set MAXH K 128"
    yield "scoreboard players set MAXW K 128"
    yield "scoreboard players set XSCALE K 96"
    yield "scoreboard players set YSCALE K 62"
    yield "scoreboard players set XMIN K -8400"
    yield "scoreboard players set YMIN K -4000"
    // color stuff
    yield "scoreboard objectives add AS dummy"  // armor stands
    yield "kill @e[type=armor_stand]"  // armor stands
    for i = 0 to 15 do
        let y,z = 4,-2
        yield sprintf "setblock %d %d %d wool %d" i y z i
        yield sprintf "summon armor_stand %d %d %d" i y z
        yield sprintf "scoreboard players set @e[type=armor_stand,x=%d,y=%d,z=%d,c=1] AS %d" i y z i
    yield "scoreboard players tag @e[type=armor_stand] add color"
    yield "summon armor_stand 0 4 0 {CustomName:Cursor}"
    |]

let cpsIStart = BBN"cpsistart"
let cpsJStart = BBN"cpsjstart"
let cpsInnerStart = BBN"cpsinnerstart"
let whileTest= BBN"whiletest"
let cpsInnerInner = BBN"cpsinnerinner"
let cpsInnerFinish = BBN"cpsinnerfinish"
let cpsJFinish = BBN"cpsjfinish"
let cpsIFinish = BBN"cpsifinish"

let program = 
    Program(cpsIStart, dict [
        cpsIStart,BasicBlock([|
            AtomicCommand "scoreboard players set I A 0"
            AtomicCommand "tp @e[name=Cursor] 0 14 0"
            AtomicCommand "fill 0 14 0 127 14 127 air"
            AtomicCommand "fill 0 13 0 127 13 127 wool 0"
            |],DirectTailCall(cpsJStart))
        cpsJStart,BasicBlock([|
            AtomicCommand "scoreboard players set J A 0"
            AtomicCommand "tp @e[name=Cursor] ~ ~ 0"
            |],DirectTailCall(cpsInnerStart))
        cpsInnerStart,BasicBlock([|
            AtomicCommand "scoreboard players operation x0 A = I A"
            AtomicCommand "scoreboard players operation x0 A *= XSCALE K"
            AtomicCommand "scoreboard players operation x0 A += XMIN K"
            AtomicCommand "scoreboard players operation y0 A = J A"
            AtomicCommand "scoreboard players operation y0 A *= YSCALE K"
            AtomicCommand "scoreboard players operation y0 A += YMIN K"
            AtomicCommand "scoreboard players set x A 0"
            AtomicCommand "scoreboard players set y A 0"
            AtomicCommand "scoreboard players set n A 0"
            |],DirectTailCall(whileTest))
        whileTest,BasicBlock([|
            AtomicCommand "scoreboard players operation xsq A = x A"
            AtomicCommand "scoreboard players operation xsq A *= x A"
            AtomicCommand "scoreboard players operation ysq A = y A"
            AtomicCommand "scoreboard players operation ysq A *= y A"
            AtomicCommand "scoreboard players operation r1 A = xsq A"
            AtomicCommand "scoreboard players operation r1 A += ysq A"
            AtomicCommand "scoreboard players operation r1 A -= FOURISSQ K"
#if ADVANCEMENTS
            AtomicCommand "scoreboard players operation @p A = r1 A"
            AtomicCommand "scoreboard players operation @p B = n A"
            |],ConditionalDirectTailCalls([|[|"execute @p[score_A=-1,score_B=15] ~ ~ ~"
#else

            |],ConditionalDirectTailCalls([|[|"scoreboard players test r1 A * -1"
                                              "scoreboard players test n A * 15"
#endif
                                            |],cpsInnerInner
                                          |],cpsInnerFinish))
        cpsInnerInner,BasicBlock([|
            AtomicCommand "scoreboard players operation xtemp A = xsq A"
            AtomicCommand "scoreboard players operation xtemp A -= ysq A"
            AtomicCommand "scoreboard players operation xtemp A /= INTSCALE K"
            AtomicCommand "scoreboard players operation xtemp A += x0 A"
            AtomicCommand "scoreboard players operation y A *= x A"
            AtomicCommand "scoreboard players operation y A += y A"
            AtomicCommand "scoreboard players operation y A /= INTSCALE K"
            AtomicCommand "scoreboard players operation y A += y0 A"
            AtomicCommand "scoreboard players operation x A = xtemp A"
            AtomicCommand "scoreboard players add n A 1"
            |],DirectTailCall(whileTest)) // TODO it may be an optimization (or pessimization) to re-unroll/inline whileTest here, measure
        cpsInnerFinish,BasicBlock([|
            AtomicCommand "scoreboard players operation @e[tag=color] AS -= n A"
            AtomicCommand "execute @e[tag=color,score_AS=-1,score_AS_min=-1] ~ ~ ~ clone ~ ~ ~ ~ ~ ~ 0 4 0"
            AtomicCommand "scoreboard players operation @e[tag=color] AS += n A"
            AtomicCommand "execute @e[name=Cursor] ~ ~ ~ clone 0 4 0 0 4 0 ~ ~ ~"
            AtomicCommand "scoreboard players add J A 1"
            AtomicCommand "execute @e[name=Cursor] ~ ~ ~ tp @e[c=1] ~ ~ ~1"
            //Yield
            AtomicCommand "scoreboard players operation r1 A = J A"
            AtomicCommand "scoreboard players operation r1 A -= MAXH K"
#if ADVANCEMENTS
            AtomicCommand "scoreboard players operation @p A = r1 A"
            |],ConditionalDirectTailCalls([|[|"execute @p[score_A=-1] ~ ~ ~"
#else
            |],ConditionalDirectTailCalls([|[|"scoreboard players test r1 A * -1"
#endif
                                            |],cpsInnerStart
                                          |],cpsJFinish))
        cpsJFinish,BasicBlock([|
            AtomicCommand "scoreboard players add I A 1"
            AtomicCommand "execute @e[name=Cursor] ~ ~ ~ tp @e[c=1] ~1 ~ ~"
            Yield
            AtomicCommand "scoreboard players operation r1 A = I A"
            AtomicCommand "scoreboard players operation r1 A -= MAXW K"
#if ADVANCEMENTS
            AtomicCommand "scoreboard players operation @p A = r1 A"
            |],ConditionalDirectTailCalls([|[|"execute @p[score_A=-1] ~ ~ ~"
#else
            |],ConditionalDirectTailCalls([|[|"scoreboard players test r1 A * -1"
#endif
                                            |],cpsJStart
                                          |],cpsIFinish))
        cpsIFinish,BasicBlock([|
            AtomicCommand """tellraw @a ["done!"]"""
            |],Halt)
        ])