
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
    let world = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\testPackEventing\"""
    for name,code in functions do
        Utilities.writeFunctionToDisk(world,"BasePack","snow",name,code)


let profileThis(suffix,outer,inner,pre,cmds,post) =
    let profilerFunc = ("prof/prof-"+suffix,[|
        yield "gamerule maxCommandChainLength 999999"
        yield "gamerule commandBlockOutput false"
        yield "gamerule sendCommandFeedback false"
        yield "gamerule logAdminCommands false"

        yield "scoreboard objectives add A dummy"
        yield "scoreboard objectives add WB dummy"

        yield "scoreboard objectives setdisplay sidebar A"

        yield "scoreboard players set DATA WB 1" 

        yield "worldborder set 10000000" 
        yield "worldborder add 1000000 1000" 
        
        yield! pre
        for _i = 1 to outer do
            yield sprintf "function %s:prof/code-%s" "test" suffix
        yield! post

        //yield "tellraw @p [\"done!\"]" 
        yield "execute store result score DATA WB run worldborder get" 
        yield "scoreboard players set Time A -10000000" 
        yield "scoreboard players operation Time A += DATA WB" 
        //yield """tellraw @p ["took ",{"score":{"name":"Time","objective":"A"}}," milliseconds"]"""
        yield "kill @e[name=Timer]"
        |])
    let dummyFunc = ("prof/code-"+suffix,[|
        for _i = 1 to inner do 
            yield! cmds 
        |])
    for name,code in [| profilerFunc; dummyFunc |] do
        MinecraftBINGO.writeFunctionToDisk(MinecraftBINGO.PACK_NAME, MinecraftBINGO.NS, name,code)



[<EntryPoint>]
let main argv = 
    profileThis("p",      500,1000,[],["scoreboard players add @p A 1"],[])                                              //  1100
    profileThis("x",      500,1000,[],["scoreboard players add x A 1"],[])                                               //   800
    profileThis("s",      500,1000,[],["scoreboard players add @s A 1"],[])                                              //   900
    profileThis("u",      500,1000,[],["scoreboard players add 1-1-1-0-1 A 1"],[])                                       //   800
    profileThis("tag",    500,1000,[],["scoreboard players add @e[tag=scoreAS] A 1"],[])                                 // 19000
    profileThis("tagdist",500,1000,[],[sprintf"scoreboard players add @e[%s] A 1"MinecraftBINGO.ENTITY_TAG],[])          //  1600
    profileThis("tagtype",500,1000,[],["scoreboard players add @e[type=armor_stand,tag=scoreAS] A 1"],[])                // 19000 
    profileThis("tagtypelimit",500,1000,[],["scoreboard players add @e[type=armor_stand,tag=scoreAS,limit=1] A 1"],[])   // 19000 

    profileThis("ix",2,500,[],["""execute store success score @p FOO run clear @p diamond 1"""],[])
    profileThis("ic",2,500,[],["""execute if entity @p[nbt={Inventory:[{id:"minecraft:diamond"}]}] store success score @p FOO run clear @p diamond 1"""],[])
    profileThis("ig",2,500,[],["""execute store success score @p FOO run clear @p #test:item001 1"""],[])

    //MinecraftBINGO.cardgen_compile()
    //MinecraftBINGOExtensions.Blind.main()
    //Raycast.main()
    throwable_light()
    ignore argv
    0
