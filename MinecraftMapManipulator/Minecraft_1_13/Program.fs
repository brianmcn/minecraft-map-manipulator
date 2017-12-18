

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

    MinecraftBINGO.cardgen_compile()
    MinecraftBINGOExtensions.Blind.main()
    //Raycast.main()
    ignore argv
    0
