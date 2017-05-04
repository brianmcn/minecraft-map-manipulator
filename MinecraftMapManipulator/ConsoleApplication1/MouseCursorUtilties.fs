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
    let ONE_THOUSAND = "ONE_THOUSAND"
    let init = [|
        for o in [x;y;z;rx;ry;ONE_THOUSAND] do
            yield sprintf "scoreboard objectives add %s dummy" o
        // constants
        yield sprintf "scoreboard players set @p %s %d" ONE_THOUSAND 1000
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
let makeRXmeasurement(lo,hi) = makeMinMaxMeasurement("rx",Objectives.rx,(fun x -> tan(deg2rad(0-x))*1000.0 |> int),lo,hi)
let makeRYmeasurement(lo,hi) = makeMinMaxMeasurement("ry",Objectives.ry,(fun x -> tan(deg2rad(x-180))*1000.0 |> int),lo,hi)
        
let initializationCommands = [|
    yield! Objectives.init
    yield sprintf "fill %d %d %d %d %d %d wool 0" SCREEN_LO_X SCREEN_LO_Y SCREEN_Z SCREEN_HI_X SCREEN_HI_Y SCREEN_Z 
    |]
let rootX = makeXmeasurement(0,63)
let rootY = makeYmeasurement(0,63)
let rootZ = makeZmeasurement(0,99)
let rootRX = makeRXmeasurement(-89,89)
let rootRY = makeRYmeasurement(91,269)

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
        |],"init",false,true)
    region.PlaceCommandBlocksStartingAt(5,4,3,[|
        yield P ""
        yield U(sprintf "advancement grant @p only %s:%s" NoLatencyCompiler.PREFIX rootX)
        yield U(sprintf "advancement grant @p only %s:%s" NoLatencyCompiler.PREFIX rootY)
        yield U(sprintf "advancement grant @p only %s:%s" NoLatencyCompiler.PREFIX rootZ)
        yield U(sprintf "advancement grant @p only %s:%s" NoLatencyCompiler.PREFIX rootRX)
        yield U(sprintf "advancement grant @p only %s:%s" NoLatencyCompiler.PREFIX rootRY)

        yield U(sprintf "scoreboard players operation dx z = @p z")
        yield U(sprintf "scoreboard players operation dx z *= @p ry")
        yield U(sprintf "scoreboard players operation dx z /= @p %s" Objectives.ONE_THOUSAND)
        yield U(sprintf "scoreboard players operation cursorX z = @p x")
        yield U(sprintf "scoreboard players operation cursorX z += dx z")

        yield U(sprintf "scoreboard players operation dy z = @p z")
        yield U(sprintf "scoreboard players operation dy z *= @p rx")
        yield U(sprintf "scoreboard players operation dy z /= @p %s" Objectives.ONE_THOUSAND)
        yield U(sprintf "scoreboard players operation cursorY z = @p y")
        yield U(sprintf "scoreboard players operation cursorY z += dy z")

        yield U(sprintf """tellraw @a ["looking at ",{"score":{"name":"cursorX","objective":"z"}},",",{"score":{"name":"cursorY","objective":"z"}}]""")

        |],"run",false,true)
    writeAdvancements(advancements,worldFolder)
    map.WriteAll()
