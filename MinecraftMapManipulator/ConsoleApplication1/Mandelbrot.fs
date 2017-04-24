module Mandelbrot

open NoLatencyCompiler

let objectivesAndConstants = [|
    // objectives
    "scoreboard objectives add A dummy"  // variables
    "scoreboard objectives add K dummy"  // constants
    // constants
    "scoreboard players set FOURISSQ K 64000000"
    "scoreboard players set INTSCALE K 4000"
    "scoreboard players set MAXH K 128"
    "scoreboard players set MAXW K 128"
    "scoreboard players set XSCALE K 109"
    "scoreboard players set YSCALE K 62"
    "scoreboard players set XMIN K -4000"
    "scoreboard players set YMIN K -10000"
    |]

// TODO other init, e.g. armor stands

let cpsIStart = BBN"cpsIStart"
let cpsJStart = BBN"cpsJStart"
let cpsInnerStart = BBN"cpsInnerStart"
let whileTest= BBN"whileTest"
let cpsInnerInner = BBN"cpsInnerInner"
let cpsInnerFinish = BBN"cpsInnerFinish"
let cpsJFinish = BBN"cpsJFinish"
let cpsIFinish = BBN"cpsIFinish"

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
            |],ConditionalDirectTailCalls([|[|"scoreboard players test r1 A * -1"
                                              "scoreboard players test n A * 15"
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
            |],DirectTailCall(whileTest))
        cpsInnerFinish,BasicBlock([|
            AtomicCommand "scoreboard players operation @e[tag=color] AS -= n A"
            AtomicCommand "execute @e[tag=color,score_AS=-1,score_AS_min=-1] ~ ~ ~ clone ~ ~ ~ ~ ~ ~ 0 4 0"
            AtomicCommand "scoreboard players operation @e[tag=color] AS += n A"
            AtomicCommand "execute @e[name=Cursor] ~ ~ ~ clone 0 4 0 0 4 0 ~ ~ ~"
            AtomicCommand "scoreboard players add J A 1"
            AtomicCommand "execute @e[name=Cursor] ~ ~ ~ tp @e[c=1] ~ ~ ~1"
            AtomicCommand "scoreboard players set PulseICB A 0" // TODO PULSE (atomicCmd)
            AtomicCommand "scoreboard players operation r1 A = J A"
            AtomicCommand "scoreboard players operation r1 A -= MAXH K"
            |],ConditionalDirectTailCalls([|[|"scoreboard players test r1 A * -1"
                                            |],cpsInnerStart
                                          |],cpsJFinish))
        cpsJFinish,BasicBlock([|
            AtomicCommand "scoreboard players add I A 1"
            AtomicCommand "execute @e[name=Cursor] ~ ~ ~ tp @e[c=1] ~1 ~ ~"
            AtomicCommand "scoreboard players set PulseICB A 1" // TODO PULSE (atomicCmd)
            AtomicCommand "scoreboard players operation r1 A = I A"
            AtomicCommand "scoreboard players operation r1 A -= MAXW K"
            |],ConditionalDirectTailCalls([|[|"scoreboard players test r1 A * -1"
                                            |],cpsJStart
                                          |],cpsIFinish))
        cpsIFinish,BasicBlock([|
            AtomicCommand """tellraw @a ["done!"]"""
            |],Halt)
        ])