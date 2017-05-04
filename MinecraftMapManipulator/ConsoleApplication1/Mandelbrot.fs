module Mandelbrot

open NoLatencyCompiler

#if SCORESONPLAYER

let objectivesAndConstants = [|
    // objectives
    yield "scoreboard objectives add A dummy"  // variables
    yield "scoreboard objectives add B dummy"  // variables
    yield "scoreboard objectives add K dummy"  // constants
    // constant objectives
    yield "scoreboard objectives add FOURISSQ dummy"
    yield "scoreboard objectives add INTSCALE dummy"
    yield "scoreboard objectives add MAXH dummy"
    yield "scoreboard objectives add MAXW dummy"
    yield "scoreboard objectives add XSCALE dummy"
    yield "scoreboard objectives add YSCALE dummy"
    yield "scoreboard objectives add XMIN dummy"
    yield "scoreboard objectives add YMIN dummy"
    // variable objectives
    yield "scoreboard objectives add i dummy"
    yield "scoreboard objectives add j dummy"
    yield "scoreboard objectives add x0 dummy"
    yield "scoreboard objectives add y0 dummy"
    yield "scoreboard objectives add x dummy"
    yield "scoreboard objectives add y dummy"
    yield "scoreboard objectives add n dummy"
    yield "scoreboard objectives add xsq dummy"
    yield "scoreboard objectives add ysq dummy"
    yield "scoreboard objectives add r1 dummy"
    yield "scoreboard objectives add xtemp dummy"
    // constants
    yield "scoreboard players set @p FOURISSQ 64000000"
    yield "scoreboard players set @p INTSCALE 4000"
    yield "scoreboard players set @p MAXH 128"
    yield "scoreboard players set @p MAXW 128"
    yield "scoreboard players set @p XSCALE 96"
    yield "scoreboard players set @p YSCALE 62"
    yield "scoreboard players set @p XMIN -8400"
    yield "scoreboard players set @p YMIN -4000"
    // color stuff
    yield "scoreboard objectives add AS dummy"  // armor stands
    yield "kill @e[type=armor_stand]"  // armor stands
    for i = 0 to 15 do
        let y,z = 4,-2
        yield sprintf "setblock %d %d %d wool %d" i y z i
        yield sprintf "summon armor_stand %d %d %d" i y z
        yield sprintf "scoreboard players set @e[type=armor_stand,x=%d,y=%d,z=%d,c=1] AS %d" i y z i
    yield "scoreboard players tag @e[type=armor_stand] add color"
    yield "summon armor_stand 0 4 0 {CustomName:Cursor,NoGravity:1}"
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
            // time measurement
#if PREEMPT
#else
            AtomicCommand "worldborder set 10000000"
            AtomicCommand "worldborder add 1000000 1000000"
#endif
            // actual code
            AtomicCommand "scoreboard players set @s i 0"
            AtomicCommand "tp @e[name=Cursor] 0 14 0"
            AtomicCommand "fill 0 14 0 127 14 127 air"
            AtomicCommand "fill 0 13 0 127 13 127 wool 0"
            |],DirectTailCall(cpsJStart))
        cpsJStart,BasicBlock([|
            AtomicCommand "scoreboard players set @s j 0"
            AtomicCommand "tp @e[name=Cursor] ~ ~ 0"
            |],DirectTailCall(cpsInnerStart))
        cpsInnerStart,BasicBlock([|
            AtomicCommand "scoreboard players operation @s x0 = @s i"
            AtomicCommand "scoreboard players operation @s x0 *= @s XSCALE"
            AtomicCommand "scoreboard players operation @s x0 += @s XMIN"
            AtomicCommand "scoreboard players operation @s y0 = @s j"
            AtomicCommand "scoreboard players operation @s y0 *= @s YSCALE"
            AtomicCommand "scoreboard players operation @s y0 += @s YMIN"
            AtomicCommand "scoreboard players set @s x 0"
            AtomicCommand "scoreboard players set @s y 0"
            AtomicCommand "scoreboard players set @s n 0"
            |],DirectTailCall(whileTest))
        whileTest,BasicBlock([|
            AtomicCommand "scoreboard players operation @s xsq = @s x"
            AtomicCommand "scoreboard players operation @s xsq *= @s x"
            AtomicCommand "scoreboard players operation @s ysq = @s y"
            AtomicCommand "scoreboard players operation @s ysq *= @s y"
            AtomicCommand "scoreboard players operation @s r1 = @s xsq"
            AtomicCommand "scoreboard players operation @s r1 += @s ysq"
            AtomicCommand "scoreboard players operation @s r1 -= @s FOURISSQ"
#if ADVANCEMENTS
            |],ConditionalDirectTailCalls(([|"@s[score_r1=-1,score_n=15]"
#else
#if CLONEMACHINE
#if USEEXECUTEIF
            |],ConditionalDirectTailCalls(([|"@p[score_r1=-1,score_n=15]"
#else
            |],ConditionalDirectTailCalls(([|"testfor @p[score_r1=-1,score_n=15]"
#endif
#else
            |],ConditionalDirectTailCalls(([|"scoreboard players test @p r1 * -1"
                                             "scoreboard players test @p n * 15"
#endif
#endif
                                            |],cpsInnerInner
                                          ),cpsInnerFinish))
        cpsInnerInner,BasicBlock([|
            AtomicCommand "scoreboard players operation @s xtemp = @s xsq"
            AtomicCommand "scoreboard players operation @s xtemp -= @s ysq"
            AtomicCommand "scoreboard players operation @s xtemp /= @s INTSCALE"
            AtomicCommand "scoreboard players operation @s xtemp += @s x0"
            AtomicCommand "scoreboard players operation @s y *= @s x"
            AtomicCommand "scoreboard players operation @s y += @s y"
            AtomicCommand "scoreboard players operation @s y /= @s INTSCALE"
            AtomicCommand "scoreboard players operation @s y += @s y0"
            AtomicCommand "scoreboard players operation @s x = @s xtemp"
            AtomicCommand "scoreboard players add @s n 1"
            |],DirectTailCall(whileTest))
        cpsInnerFinish,BasicBlock([|
#if DIRECT16COLORTEST
            yield AtomicCommand "scoreboard players operation @e[name=Cursor] n = @s n"
            for zzz = 0 to 15 do
                yield AtomicCommand(sprintf "execute @e[name=Cursor,score_n=%d,score_n_min=%d] ~ ~ ~ setblock ~ ~ ~ wool %d" (zzz+1) (zzz+1) zzz)
#else
            yield AtomicCommand "scoreboard players operation @e[tag=color] AS -= @s n"
            yield AtomicCommand "execute @e[tag=color,score_AS=-1,score_AS_min=-1] ~ ~ ~ clone ~ ~ ~ ~ ~ ~ 0 4 0"
            yield AtomicCommand "scoreboard players operation @e[tag=color] AS += @s n"
            yield AtomicCommand "execute @e[name=Cursor] ~ ~ ~ clone 0 4 0 0 4 0 ~ ~ ~"
#endif
            yield AtomicCommand "scoreboard players add @s j 1"
            yield AtomicCommand "execute @e[name=Cursor] ~ ~ ~ tp @e[c=1] ~ ~ ~1"
            //yield Yield  // inner loop yield
            yield AtomicCommand "scoreboard players operation @s r1 = @s j"
            yield AtomicCommand "scoreboard players operation @s r1 -= @s MAXH"
#if ADVANCEMENTS
            |],ConditionalDirectTailCalls(([|"@s[score_r1=-1]"
#else
#if USEEXECUTEIF
            |],ConditionalDirectTailCalls(([|"@p[score_r1=-1]"
#else
            |],ConditionalDirectTailCalls(([|"scoreboard players test @p r1 * -1"
#endif
#endif
                                            |],cpsInnerStart
                                          ),cpsJFinish))
        cpsJFinish,BasicBlock([|
            AtomicCommand "scoreboard players add @s i 1"
            AtomicCommand "execute @e[name=Cursor] ~ ~ ~ tp @e[c=1] ~1 ~ ~"
            Yield
            AtomicCommand "scoreboard players operation @s r1 = @s i"
            AtomicCommand "scoreboard players operation @s r1 -= @s MAXW"
#if ADVANCEMENTS
            |],ConditionalDirectTailCalls(([|"@s[score_r1=-1]"
#else
#if USEEXECUTEIF
            |],ConditionalDirectTailCalls(([|"@p[score_r1=-1]"
#else
            |],ConditionalDirectTailCalls(([|"scoreboard players test @p r1 * -1"
#endif
#endif
                                            |],cpsJStart
                                          ),cpsIFinish))
        cpsIFinish,BasicBlock([|
            AtomicCommand """tellraw @a ["done!"]"""
            // time measurement
#if PREEMPT
#else
            AtomicCommand("stats entity @e[name=Cursor] set QueryResult @e[name=Cursor] A")
            AtomicCommand("scoreboard players set @e[name=Cursor] A 1") // need initial value before can trigger a stat
            AtomicCommand "execute @e[name=Cursor] ~ ~ ~ worldborder get"
            AtomicCommand "scoreboard players set Time A -10000000"
            AtomicCommand "scoreboard players operation Time A += @e[name=Cursor] A"
            AtomicCommand """tellraw @a ["took ",{"score":{"name":"Time","objective":"A"}}," seconds"]"""
#endif
            |],Halt)
        ])


#else

let objectivesAndConstants = [|
    // objectives
    yield "scoreboard objectives add A dummy"  // variables
    yield "scoreboard objectives add B dummy"  // variables
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
    yield "summon armor_stand 0 4 0 {CustomName:Cursor,NoGravity:1}"
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
            // time measurement
#if PREEMPT
#else
            AtomicCommand "worldborder set 10000000"
            AtomicCommand "worldborder add 1000000 1000000"
#endif
            // actual code
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
            AtomicCommand "scoreboard players operation @s A = r1 A"
            AtomicCommand "scoreboard players operation @s B = n A"
            |],ConditionalDirectTailCalls(([|"@s[score_A=-1,score_B=15]"
#else
#if CLONEMACHINE
#if USEEXECUTEIF
            AtomicCommand "scoreboard players operation @s A = r1 A"
            AtomicCommand "scoreboard players operation @s B = n A"
            |],ConditionalDirectTailCalls(([|"@p[score_A=-1,score_B=15]"
#else
            AtomicCommand "scoreboard players operation @s A = r1 A"
            AtomicCommand "scoreboard players operation @s B = n A"
            |],ConditionalDirectTailCalls(([|"testfor @p[score_A=-1,score_B=15]"
#endif
#else
            |],ConditionalDirectTailCalls(([|"scoreboard players test r1 A * -1"
                                             "scoreboard players test n A * 15"
#endif
#endif
                                            |],cpsInnerInner
                                          ),cpsInnerFinish))
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
            //Yield
            AtomicCommand "scoreboard players operation r1 A = J A"
            AtomicCommand "scoreboard players operation r1 A -= MAXH K"
#if ADVANCEMENTS
            AtomicCommand "scoreboard players operation @s A = r1 A"
            |],ConditionalDirectTailCalls(([|"@s[score_A=-1]"
#else
#if USEEXECUTEIF
            AtomicCommand "scoreboard players operation @s A = r1 A"
            |],ConditionalDirectTailCalls(([|"@p[score_A=-1]"
#else
            |],ConditionalDirectTailCalls(([|"scoreboard players test r1 A * -1"
#endif
#endif
                                            |],cpsInnerStart
                                          ),cpsJFinish))
        cpsJFinish,BasicBlock([|
            AtomicCommand "scoreboard players add I A 1"
            AtomicCommand "execute @e[name=Cursor] ~ ~ ~ tp @e[c=1] ~1 ~ ~"
            Yield
            AtomicCommand "scoreboard players operation r1 A = I A"
            AtomicCommand "scoreboard players operation r1 A -= MAXW K"
#if ADVANCEMENTS
            AtomicCommand "scoreboard players operation @s A = r1 A"
            |],ConditionalDirectTailCalls(([|"@s[score_A=-1]"
#else
#if USEEXECUTEIF
            AtomicCommand "scoreboard players operation @s A = r1 A"
            |],ConditionalDirectTailCalls(([|"@p[score_A=-1]"
#else
            |],ConditionalDirectTailCalls(([|"scoreboard players test r1 A * -1"
#endif
#endif
                                            |],cpsJStart
                                          ),cpsIFinish))
        cpsIFinish,BasicBlock([|
            AtomicCommand """tellraw @a ["done!"]"""
            // time measurement
#if PREEMPT
#else
            AtomicCommand("stats entity @e[name=Cursor] set QueryResult @e[name=Cursor] A")
            AtomicCommand("scoreboard players set @e[name=Cursor] A 1") // need initial value before can trigger a stat
            AtomicCommand "execute @e[name=Cursor] ~ ~ ~ worldborder get"
            AtomicCommand "scoreboard players set Time A -10000000"
            AtomicCommand "scoreboard players operation Time A += @e[name=Cursor] A"
            AtomicCommand """tellraw @a ["took ",{"score":{"name":"Time","objective":"A"}}," seconds"]"""
#endif
            |],Halt)
        ])

#endif