

let profileThis(suffix,pre,cmds,post) =
    let profilerFunc = ("prof-"+suffix,[|
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
        for _i = 1 to 100 do
            yield sprintf "function %s:code-%s" "test" suffix
        yield! post

        //yield "tellraw @p [\"done!\"]" 
        yield "execute store result score DATA WB run worldborder get" 
        yield "scoreboard players set Time A -10000000" 
        yield "scoreboard players operation Time A += DATA WB" 
        //yield """tellraw @p ["took ",{"score":{"name":"Time","objective":"A"}}," milliseconds"]"""
        yield "kill @e[name=Timer]"
        |])
    let dummyFunc = ("code-"+suffix,[|
        for _i = 1 to 1000 do 
            yield! cmds 
        |])
    for name,code in [| profilerFunc; dummyFunc |] do
        MinecraftBINGO.writeFunctionToDisk(name,code)



[<EntryPoint>]
let main argv = 
    //profileThis("p",[],["scoreboard players add @p A 1"],[])
    //profileThis("x",[],["scoreboard players add x A 1"],[])

    //printfn "hello world"
    //MinecraftBINGO.test()
    //MinecraftBINGO.testWrite()
    //MinecraftBINGO.makeSavingStructureBlocks()
    //MinecraftBINGO.cardgen_compile()
    MinecraftBINGO.magic_mirror_compile()
    ignore argv
    0
