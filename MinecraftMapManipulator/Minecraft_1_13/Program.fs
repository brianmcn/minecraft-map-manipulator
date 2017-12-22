
let throwable_light() =
    let objectives = [|
        // position of most recent unprocessed airborne snowball (snowy = -1 to "null out" after processing)
        "snowx"
        "snowy"
        "snowz"
        // time remaining on the one light placed
        "remain"
        |]
    let functions = [|
        "snowinit",[|
            for o in objectives do
                yield sprintf "scoreboard objectives add %s dummy" o
            |]
        "snowloop",[|
            // TODO can get rid of continual @e by reading stats and only turning this on when the player just threw a snowball
            "execute unless entity @p[scores={remain=1..}] as @e[type=snowball] at @s if block ~ ~ ~ air run function snow:track"
            "execute unless entity @p[scores={remain=1..}] unless entity @p[scores={snowy=-1}] unless entity @e[type=snowball] run function snow:place"
            "execute if entity @p[scores={remain=1}] run function snow:remove"
            "scoreboard players remove @p[scores={remain=1..}] remain 1"  // run timer
            |]
        "track",[|
            // TODO don't store on @p
            "execute store result score @p snowx run data get entity @s Pos[0] 1.0"
            "execute store result score @p snowy run data get entity @s Pos[1] 1.0"
            "execute store result score @p snowz run data get entity @s Pos[2] 1.0"
            "execute if entity @e[type=!snowball,distance=..2] run scoreboard players set @p snowy -1"  // if too close to an entity, null it out, so don't suffocate anything
            |]
        "place",[|
            """summon area_effect_cloud 1 1 1 {Duration:1,Tags:["snowaec"]}"""
            "execute store result entity @e[tag=snowaec,limit=1] Pos[0] double 1.0 run scoreboard players get @p snowx"
            "execute store result entity @e[tag=snowaec,limit=1] Pos[1] double 1.0 run scoreboard players get @p snowy"
            "execute store result entity @e[tag=snowaec,limit=1] Pos[2] double 1.0 run scoreboard players get @p snowz"
            "execute at @e[tag=snowaec] run setblock ~ ~ ~ sea_lantern"
            "scoreboard players set @p remain 40" // TODO store other than on player
            // no need to kill, Duration wil do it
            |]
        "remove",[|
            """summon area_effect_cloud 1 1 1 {Duration:1,Tags:["snowaec"]}"""
            "execute store result entity @e[tag=snowaec,limit=1] Pos[0] double 1.0 run scoreboard players get @p snowx"
            "execute store result entity @e[tag=snowaec,limit=1] Pos[1] double 1.0 run scoreboard players get @p snowy"
            "execute store result entity @e[tag=snowaec,limit=1] Pos[2] double 1.0 run scoreboard players get @p snowz"
            "execute at @e[tag=snowaec] run setblock ~ ~ ~ air"
            "scoreboard players set @p snowy -1" // TODO store other than on player
            // no need to kill, Duration wil do it
            |]
        |]
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, """testPackEventing""")
    for name,code in functions do
        Utilities.writeFunctionToDisk(world,"BasePack","snow",name,code)


[<EntryPoint>]
let main argv = 
    MinecraftBINGO.cardgen_compile()
    MinecraftBINGOExtensions.Blind.main()
    //Raycast.main()
    //throwable_light()
    PerformanceMicroBenchmarks.main()
    ignore argv
    0
