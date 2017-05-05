module MouseCursorUtilties

let SCREEN_LO_X = 0
let SCREEN_HI_X = 64
let SCREEN_LO_Y = 4
let SCREEN_HI_Y = 40
let SCREEN_Z = 0

let advancements = ResizeArray()

module Objectives =
    let x = "x"
    let y = "y"
    let z = "z"
    let rx = "rx"
    let ry = "ry"
    let sqrttemp = "sqrttemp"
    let FIFTY = "FIFTY"
    let ONE_HUNDRED = "ONE_HUNDRED"
    let init = [|
        for o in [x;y;z;rx;ry;sqrttemp;FIFTY;ONE_HUNDRED] do
            yield sprintf "scoreboard objectives add %s dummy" o
        // constants
        yield sprintf "scoreboard players set @p %s %d" ONE_HUNDRED 100
        yield sprintf "scoreboard players set @p %s %d" FIFTY 50
        |]

let rec makeDiffMeasurement(selectorName,outputObjectiveName,lo,hi) =
    let name = sprintf "%s%02dto%02d" selectorName lo hi
    if lo = hi then
        advancements.Add(NoLatencyCompiler.makeAdvancement(name,[|
            sprintf "advancement revoke @s only %s:%s" NoLatencyCompiler.PREFIX name
            sprintf "scoreboard players set @s %s %d" outputObjectiveName lo
            |]))
        name
    elif lo+1 = hi then
        advancements.Add(NoLatencyCompiler.makeAdvancement(name,[|
            sprintf "advancement revoke @s only %s:%s" NoLatencyCompiler.PREFIX name
            sprintf "scoreboard players set @s[%s=%d,d%s=0] %s %d" selectorName lo selectorName outputObjectiveName lo
            sprintf "scoreboard players set @s[%s=%d,d%s=0] %s %d" selectorName hi selectorName outputObjectiveName hi
            |]))
        name
    else
        let mid = (hi+lo)/2
        let left = makeDiffMeasurement(selectorName,outputObjectiveName,lo,mid)
        let right = makeDiffMeasurement(selectorName,outputObjectiveName,mid+1,hi)
        advancements.Add(NoLatencyCompiler.makeAdvancement(name,[|
            sprintf "advancement revoke @s only %s:%s" NoLatencyCompiler.PREFIX name
            sprintf "advancement grant @s[%s=%d,d%s=%d] only %s:%s" selectorName lo selectorName (mid-lo) NoLatencyCompiler.PREFIX left
            sprintf "advancement grant @s[%s=%d,d%s=%d] only %s:%s" selectorName (mid+1) selectorName (hi-mid-1) NoLatencyCompiler.PREFIX right
            |]))
        name

let rec makeXmeasurement(lo,hi) = makeDiffMeasurement("x",Objectives.x,lo,hi)
let rec makeYmeasurement(lo,hi) = makeDiffMeasurement("y",Objectives.y,lo,hi)
let rec makeZmeasurement(lo,hi) = makeDiffMeasurement("z",Objectives.z,lo,hi)

let rec makeMinMaxMeasurement(selectorName,outputObjectiveName,f,lo,hi) =
    let name = sprintf "%s%02dto%02d" selectorName lo hi
    if lo = hi then
        advancements.Add(NoLatencyCompiler.makeAdvancement(name,[|
            sprintf "advancement revoke @s only %s:%s" NoLatencyCompiler.PREFIX name
            sprintf "scoreboard players set @s %s %d" outputObjectiveName (f lo)
            |]))
        name
    elif lo+1 = hi then
        advancements.Add(NoLatencyCompiler.makeAdvancement(name,[|
            sprintf "advancement revoke @s only %s:%s" NoLatencyCompiler.PREFIX name
            sprintf "scoreboard players set @s[%sm=%d,%s=%d] %s %d" selectorName lo selectorName lo outputObjectiveName (f lo)
            sprintf "scoreboard players set @s[%sm=%d,%s=%d] %s %d" selectorName hi selectorName hi outputObjectiveName (f hi)
            |]))
        name
    else
        let mid = (hi+lo)/2
        let left = makeMinMaxMeasurement(selectorName,outputObjectiveName,f,lo,mid)
        let right = makeMinMaxMeasurement(selectorName,outputObjectiveName,f,mid+1,hi)
        advancements.Add(NoLatencyCompiler.makeAdvancement(name,[|
            sprintf "advancement revoke @s only %s:%s" NoLatencyCompiler.PREFIX name
            sprintf "advancement grant @s[%sm=%d,%s=%d] only %s:%s" selectorName lo selectorName mid NoLatencyCompiler.PREFIX left
            sprintf "advancement grant @s[%sm=%d,%s=%d] only %s:%s" selectorName (mid+1) selectorName hi NoLatencyCompiler.PREFIX right
            |]))
        name


let deg2rad x = float x * System.Math.PI / 180.0
let makeRXmeasurement(lo,hi) = makeMinMaxMeasurement("rx",Objectives.rx,(fun x -> tan(deg2rad(0-x))*100.0 |> int),lo,hi)
let makeRYmeasurement(lo,hi) = makeMinMaxMeasurement("ry",Objectives.ry,(fun x -> tan(deg2rad(x-180))*100.0 |> int),lo,hi)


let rec makeSqrt(player,objective,lo,hi) =
    let name = sprintf "sqrt%03dto%03d" lo hi
    if lo = hi || lo+1 = hi then
        advancements.Add(NoLatencyCompiler.makeAdvancement(name,[|
            sprintf "advancement revoke @s only %s:%s" NoLatencyCompiler.PREFIX name
            sprintf "scoreboard players set @s %s %d" Objectives.sqrttemp lo
            |]))
        name
    else
        let mid = (hi+lo)/2
        let left  = makeSqrt(player,objective,lo,mid)
        let right = makeSqrt(player,objective,mid+1,hi)
        advancements.Add(NoLatencyCompiler.makeAdvancement(name,[|
            sprintf "advancement revoke @s only %s:%s" NoLatencyCompiler.PREFIX name
            sprintf "scoreboard players set @s %s %d" Objectives.sqrttemp mid
            sprintf "scoreboard players operation @s %s *= @s %s" Objectives.sqrttemp Objectives.sqrttemp
            sprintf "scoreboard players operation @s %s -= %s %s" Objectives.sqrttemp player objective 
            sprintf "advancement grant @s[score_%s_min=1] only %s:%s" Objectives.sqrttemp NoLatencyCompiler.PREFIX left
            sprintf "advancement grant @s[score_%s=0] only %s:%s" Objectives.sqrttemp NoLatencyCompiler.PREFIX right
            |]))
        name



let maketpToCursorXY(hi) =
    let name = "tptoxy"
    let mutable i = 1
    while i < hi do
        i <- i * 2
    let instructions = ResizeArray()
    while i >= 1 do
        instructions.Add(sprintf "execute @s[score_x_min=%d] ~ ~ ~ tp @e[type=armor_stand] ~%d ~ ~" i i)
        instructions.Add(sprintf "execute @s[score_y_min=%d] ~ ~ ~ tp @e[type=armor_stand] ~ ~%d ~" i i)
        //instructions.Add(sprintf "execute @s[score_z_min=%d] ~ ~ ~ tp @e[type=armor_stand] ~ ~ ~%d" i i)
        instructions.Add(sprintf "scoreboard players remove @s[score_x_min=%d] x %d" i i)
        instructions.Add(sprintf "scoreboard players remove @s[score_y_min=%d] y %d" i i)
        //instructions.Add(sprintf "scoreboard players remove @s[score_z_min=%d] z %d" i i)
        i <- i / 2
    advancements.Add(NoLatencyCompiler.makeAdvancement(name,[|
        yield sprintf "advancement revoke @s only %s:%s" NoLatencyCompiler.PREFIX name
        yield "tp @e[type=armor_stand] 0 0 0.9"
        yield! instructions
        yield "tp @e[type=armor_stand] ~ ~-1.4 ~"
        |]))
    name

    
        
let initializationCommands = [|
    yield! Objectives.init
    yield sprintf "fill %d %d %d %d %d %d wool 0" SCREEN_LO_X SCREEN_LO_Y SCREEN_Z SCREEN_HI_X SCREEN_HI_Y SCREEN_Z 
    |]
let rootX = makeXmeasurement(0,63)
let rootY = makeYmeasurement(0,63)
let rootZ = makeZmeasurement(0,99)
let rootRX = makeRXmeasurement(-89,89)
let rootRY = makeRYmeasurement(91,269)
let tpToCursorXY = maketpToCursorXY(63)
let computeSqrtDyZ = makeSqrt("dy","z",0,128)

open RegionFiles
open Advancements

let fauxroot = "functions/fauxroot",Advancements.Advancement(None,NoDisplay,Reward([||],[||],0,[|
                |]),[|Criterion("cx",Recipes.MC"impossible",[||])|],[|[|"cx"|]|])
advancements.Add(fauxroot)

let putItAllInTheWorld(worldFolder:string) =
    let map = new MapFolder(worldFolder+"""\region""")
    let region = map.GetRegion(0,0)
    region.PlaceCommandBlocksStartingAt(3,4,3,[|
        yield O ""
        for c in initializationCommands do
            yield U c
        yield U "kill @e[type=armor_stand]"
        yield U "summon armor_stand ~ ~ ~ {Invisible:1b,NoGravity:1b,ArmorItems:[{},{},{},{id:skull,Count:1b,Damage:1b}]}"
        // big brush template for cloning
        yield U "fill -10 4 1 -6 8 1 wool 14"
        yield U "setblock -10 4 1 air"
        yield U "setblock -10 8 1 air"
        yield U "setblock -6 4 1 air"
        yield U "setblock -6 8 1 air"
        // tick counter
        yield U "scoreboard players set tick z 0"
        |],"init",false,true)
    region.PlaceCommandBlocksStartingAt(5,4,3,[|
        yield P ""
        yield U(sprintf "advancement grant @p only %s:%s" NoLatencyCompiler.PREFIX rootX)
        yield U(sprintf "advancement grant @p only %s:%s" NoLatencyCompiler.PREFIX rootY)
        yield U(sprintf "advancement grant @p only %s:%s" NoLatencyCompiler.PREFIX rootZ)
        yield U(sprintf "advancement grant @p only %s:%s" NoLatencyCompiler.PREFIX rootRX)
        yield U(sprintf "advancement grant @p only %s:%s" NoLatencyCompiler.PREFIX rootRY)

        // cursorX = z * tan theta
        yield U(sprintf "scoreboard players operation dx z = @p z")
        yield U(sprintf "scoreboard players operation dx z *= @p ry")
        yield U(sprintf "scoreboard players operation dx z -= @p %s" Objectives.FIFTY)  // computation is off by half-block (aims at corner rather than center)
        yield U(sprintf "scoreboard players operation hundreddx z = dx z")
        yield U(sprintf "scoreboard players operation dx z /= @p %s" Objectives.ONE_HUNDRED)
        yield U(sprintf "scoreboard players operation cursorX z = @p x")
        yield U(sprintf "scoreboard players operation cursorX z += dx z")

        // cursorY = sqrt(z^2+dx^2) * tan theta
        yield U(sprintf "scoreboard players operation hundreddx z *= hundreddx z")
        yield U(sprintf "scoreboard players operation dy z = @p z")
        yield U(sprintf "scoreboard players operation dy z *= @p %s" Objectives.ONE_HUNDRED)
        yield U(sprintf "scoreboard players operation dy z *= dy z")
        yield U(sprintf "scoreboard players operation dy z += hundreddx z")  // (100dx)^2 + (100z)^2
        yield U(sprintf "scoreboard players operation dy z /= @p %s" Objectives.ONE_HUNDRED)
        yield U(sprintf "scoreboard players operation dy z /= @p %s" Objectives.ONE_HUNDRED) // dx^2 + z^2 (preserving decimals while squaring dx)
        yield U(sprintf "advancement grant @p only %s:%s" NoLatencyCompiler.PREFIX computeSqrtDyZ)
        yield U(sprintf "scoreboard players operation dy z = @p %s" Objectives.sqrttemp)
        yield U(sprintf "scoreboard players operation dy z *= @p rx")
        yield U(sprintf "scoreboard players operation dy z /= @p %s" Objectives.ONE_HUNDRED)
        yield U(sprintf "scoreboard players operation cursorY z = @p y")
        yield U(sprintf "scoreboard players operation cursorY z += dy z")

        yield U(sprintf "scoreboard players operation @p x = cursorX z")
        yield U(sprintf "scoreboard players operation @p y = cursorY z")
        yield U(sprintf "advancement grant @p only %s:%s" NoLatencyCompiler.PREFIX tpToCursorXY)

        //yield U(sprintf """tellraw @a ["looking at ",{"score":{"name":"cursorX","objective":"z"}},",",{"score":{"name":"cursorY","objective":"z"}}]""")
        yield C("scoreboard players set @e[type=armor_stand] z 0")
        yield U("""testfor @p {SelectedItem:{id:"minecraft:carpet"}}""")
        yield C("scoreboard players set @e[type=armor_stand] z 1")
        yield U("""testfor @p {SelectedItem:{id:"minecraft:wool"}}""")
        yield C("scoreboard players set @e[type=armor_stand] z 2")
        yield U("execute @e[type=armor_stand,score_z_min=1] ~ ~2 ~ setblock ~ ~ ~ wool 14")
        yield U("execute @e[type=armor_stand,score_z_min=2] ~-2 ~ ~ clone -10 4 1 -6 8 1 ~ ~ ~ masked")

        // basic fruitninja ideas
        yield U("scoreboard players add tick z 1")
        yield U("scoreboard players operation ticktmp z = tick z")
        yield U(sprintf "scoreboard players operation ticktmp z %%= @p %s" Objectives.ONE_HUNDRED)
        yield U("scoreboard players test ticktmp z 0 0")
        yield C("""summon minecraft:item 10 4 1 {Item:{id:"minecraft:apple",Count:1b},Age:5920s,Motion:[0.2,1.0,0.0]}""")
        yield U("""execute @e[type=armor_stand] ~ ~ ~ execute @e[type=item,r=1] ~ ~ ~ summon minecraft:fireworks_rocket ~ ~ ~ {LifeTime:0,FireworksItem:{id:"minecraft:fireworks",Count:1,tag:{Fireworks:{Explosions:[{Type:0,Flicker:0,Trail:0,Colors:[16730395,1796095,5177112],FadeColors:[16777215]},]}}}}""")
        yield U("""execute @e[type=armor_stand] ~ ~ ~ execute @e[type=item,r=1] ~ ~ ~ kill @e[type=item]""")
        |],"run",false,true)
    writeAdvancements(advancements,worldFolder)
    map.WriteAll()
