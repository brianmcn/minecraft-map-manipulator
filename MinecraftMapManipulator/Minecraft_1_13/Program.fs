

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
        MinecraftBINGO.writeFunctionToDisk(name,code)



[<EntryPoint>]
let main argv = 
    profileThis("p",100,1000,[],["scoreboard players add @p A 1"],[])
    profileThis("x",100,1000,[],["scoreboard players add x A 1"],[])
    profileThis("lu",100,1000,[],["execute if entity @e[tag=scoreAS,scores={gameInProgress=2}] run scoreboard players add x A 1"],[])
    profileThis("lud",100,1000,[],["execute if entity @e[tag=scoreAS,x=1,y=1,z=1,distance=..1.0,scores={gameInProgress=2}] run scoreboard players add x A 1"],[])
    profileThis("luf",100,1000,[],["execute if score FOO A <= FOO A run scoreboard players add x A 1"],[])

    profileThis("ix",2,500,[],["""execute store success score @p FOO run clear @p diamond 1"""],[])
    profileThis("ic",2,500,[],["""execute if entity @p[nbt={Inventory:[{id:"minecraft:diamond"}]}] store success score @p FOO run clear @p diamond 1"""],[])
    profileThis("ig",2,500,[],["""execute store success score @p FOO run clear @p #test:item001 1"""],[])


    //printfn "hello world"
    //MinecraftBINGO.test()
    //MinecraftBINGO.testWrite()
    MinecraftBINGO.makeSavingStructureBlocks()
    MinecraftBINGO.writeInventoryChangedHandler()
    MinecraftBINGO.cardgen_compile()
    ignore argv
    0
