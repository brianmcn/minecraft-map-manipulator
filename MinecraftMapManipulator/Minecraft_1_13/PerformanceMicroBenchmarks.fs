module PerformanceMicroBenchmarks

let PACK_NAME = "PerfPack"
// TODO move to separate world, do setup to create initial objectives and entities
let FOLDER = System.IO.Path.Combine(Utilities.MC_ROOT, """testing""")

let allProfilerFunctions = ResizeArray()

let profileThis(suffix,outer,inner,innerInner,pre,cmds,post) =
    let profName = "prof-"+suffix
    let all = [|
        yield profName,[|
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
                yield sprintf "function %s:code-%s" "test" suffix
            yield! post

            //yield "tellraw @p [\"done!\"]" 
            yield "execute store result score DATA WB run worldborder get" 
            yield "scoreboard players set Time A -10000000" 
            yield "scoreboard players operation Time A += DATA WB" 
            yield "scoreboard players operation @p WB = Time A"
            yield sprintf """tellraw @a ["took ",{"score":{"name":"@p","objective":"WB"}}," milliseconds to run %d iterations of"]""" (outer*inner*innerInner)
            for cmd in cmds do
                yield sprintf """tellraw @a ["    %s"]""" (Utilities.escape cmd)
            yield "kill @e[name=Timer]"
            |]
        if innerInner=1 then
            yield "code-"+suffix,[|
                for _i = 1 to inner do 
                    yield! cmds 
                |]
        else
            yield "code-"+suffix,[|
                for _i = 1 to inner do 
                    yield sprintf "function %s:inner-%s" "test" suffix
                |]
            yield "inner-"+suffix,[|
                for _i = 1 to innerInner do 
                    yield! cmds 
                |]
        |]
    for name,code in all do
        Utilities.writeFunctionToDisk(FOLDER, PACK_NAME, "test", name, code)
    allProfilerFunctions.Add(profName)


let SELECTORS = [|
    "p",      "@p"
    "s",      "@s"
    "tag",    "@e[tag=scoreAS]"
    "tagtype","@e[tag=scoreAS,type=armor_stand]"
    "tagdist",sprintf "@e[%s]" MinecraftBINGO.ENTITY_TAG 
    |]
let SELECTORS_WITH_FAKE = [|
    yield! SELECTORS
    yield "fake", "x"
    |]
let SELECTORS_WITH_UUID = [|
    yield! SELECTORS
    yield "u", "1-1-1-0-1"
    |]

(*
execute pseudocode (imagined)


let instr = instructions.pop_front()
decode(instr)
    case: "execute as @e run FOO"
        let ents = <list of entities>
        let ctxt = new Context(CURRENT_SENDER, CURRENT_AT)
        contextStack.Push(ctxt)
        instructions.push_front(QUOTE(contextStack.Pop()))
        instructions.push_front(QUOTE(CURRENT_AT = contextStack.Top().At))
        instructions.push_front(QUOTE(CURRENT_SENDER = contextStack.Top().Sender))
        for each e in ents do
            instructions.push_front(QUOTE(FOO))
            instructions.push_front(QUOTE(CURRENT_SENDER = e))
            instructions.push_front(QUOTE(CURRENT_AT = ctxt.AT))
        TODO - what is 'result' and 'success' values?

*)


let main() =
    Utilities.writeDatapackMeta(FOLDER, PACK_NAME, "MinecraftBINGO base pack")

    Utilities.writeFunctionToDisk(FOLDER, PACK_NAME, "test", "call_seed", [|"seed"|])
    Utilities.writeFunctionToDisk(FOLDER, PACK_NAME, "test", "call_add", [|"scoreboard players add @s A 1"|])

    let OUTER = 200
    let INNER = 1000

    ///////////
    // good baseline stuff to keep


    profileThis("seed",       OUTER,INNER,1,[],["seed"],[]) 
    profileThis("callseed",   OUTER,INNER,1,[],["function test:call_seed"],[])    // TODO 500,1000 just fails silently and immediately, why?

    profileThis("calladd",   OUTER,INNER,1,[],["function test:call_add"],[])
    profileThis("eradd",OUTER,INNER,1,[],[sprintf "execute run scoreboard players add @s A 1"],[])
    (*
took 29 milliseconds to run 200000 iterations of
    seed
took 102 milliseconds to run 200000 iterations of
    function test:call_seed
    *)

    for name,sel in SELECTORS_WITH_UUID do
        profileThis("ei"+name,OUTER,INNER,1,[],[sprintf "execute if entity %s" sel],[])  // the new 'testfor'
        profileThis("eiat1"+name,OUTER,INNER,1,[],[sprintf "execute if entity %s at @s run seed" sel],[])
        profileThis("eiat2"+name,OUTER,INNER,1,[],[sprintf "execute if entity %s at %s run seed" sel sel],[])

//    profileThis("ei-ep",OUTER,INNER,1,[],[sprintf "execute if entity @e[type=player]"],[])
//    profileThis("ei-a",OUTER,INNER,1,[],[sprintf "execute if entity @a"],[])

    for name,sel in SELECTORS_WITH_FAKE do
        // TODO note that "scoreboard players add 1-1-1-0-1 A 1" will add to a fake player with that name, and not to a UUID'd entity with that uuid
        profileThis("sb"+name,OUTER,INNER,1,[],[sprintf "scoreboard players add %s A 1" sel],[])

    for name,sel in SELECTORS_WITH_UUID do
        profileThis("ea"+name,OUTER,INNER,1,[],[sprintf "execute as %s run scoreboard players add @s A 1" sel],[])

    profileThis("eseed",OUTER,INNER,1,[],[sprintf "execute run seed"],[])
    for name,sel in SELECTORS_WITH_UUID do
        profileThis("eiseed"+name,OUTER,INNER,1,[],[sprintf "execute if entity %s run seed" sel],[])
        profileThis("easeed"+name,OUTER,INNER,1,[],[sprintf "execute as %s run seed" sel],[])
        profileThis("eaaseed"+name,OUTER,INNER,1,[],[sprintf "execute as %s at @s run seed" sel],[])
// note: these do not have much effect... was trying to test if the 'number of instructions remaining in the continuation' affects the cost of invoking a function (assuming it 'inserts instructions to front of current list')
//        profileThis("eiseed"+name,5000,100,1,[],[sprintf "execute if entity %s run seed" sel],[])
//        profileThis("easeed"+name,5000,100,1,[],[sprintf "execute as %s run seed" sel],[])
//        profileThis("eiseed"+name,50,100,100,[],[sprintf "execute if entity %s run seed" sel],[])
//        profileThis("easeed"+name,50,100,100,[],[sprintf "execute as %s run seed" sel],[])


    ///////////
    // current experiments


    // TODO stuff in email notes

    (*

took 7501 milliseconds to run 500000 iterations of
    execute run seed
took 75 milliseconds to run 500000 iterations of
    seed

took 9017 milliseconds to run 500000 iterations of
    execute as @p run seed
took 8912 milliseconds to run 500000 iterations of
    execute if entity @p run seed

    for pre,suf in ["","run scoreboard players add FAKE A 1"; "tp","at @s run tp @s ~ ~ ~"] do
        profileThis(sprintf "%seip"pre,      500,1000,[],[sprintf"execute as @p %s"suf],[])                                
        profileThis(sprintf "%seis"pre,      500,1000,[],[sprintf"execute as @s %s"suf],[])                                
        profileThis(sprintf "%seiu"pre,      500,1000,[],[sprintf"execute as 1-1-1-0-1 %s"suf],[])                         
        profileThis(sprintf "%seineu"pre,    500,1000,[],[sprintf"execute as 3-3-3-0-3 %s"suf],[])                         
        profileThis(sprintf "%seitag"pre,    500,1000,[],[sprintf"execute as @e[tag=scoreAS] %s"suf],[])                   
        profileThis(sprintf "%seitagdist"pre,500,1000,[],[sprintf"execute as @e[%s] %s"MinecraftBINGO.ENTITY_TAG suf],[])  
    
    
    profileThis("ix",2,500,[],["""execute store success score @p FOO run clear @p diamond 1"""],[])
    profileThis("ic",2,500,[],["""execute if entity @p[nbt={Inventory:[{id:"minecraft:diamond"}]}] store success score @p FOO run clear @p diamond 1"""],[])
    profileThis("ig",2,500,[],["""execute store success score @p FOO run clear @p #test:item001 1"""],[])
    *)

    // NF = 'next function'
    let next = [|
        for i = 0 to allProfilerFunctions.Count-1 do
            let funcName = allProfilerFunctions.[i]
            yield sprintf "execute if entity @p[scores={NF=%d}] run function test:%s" i funcName
        yield sprintf "execute if entity @p[scores={NF=%d..}] run say all done, do scoreboard players set @p NF 0 to restart" allProfilerFunctions.Count
        yield "scoreboard players add @p NF 1"
        |]
    Utilities.writeFunctionToDisk(FOLDER, PACK_NAME, "test", "next", next)


(*

took 83 milliseconds to run 500000 iterations of
    seed
took 79 milliseconds to run 500000 iterations of
    execute if entity @p
took 53 milliseconds to run 500000 iterations of
    execute if entity @s
took 19546 milliseconds to run 500000 iterations of
    execute if entity @e[tag=scoreAS]
took 19755 milliseconds to run 500000 iterations of
    execute if entity @e[tag=scoreAS,type=armor_stand]
took 320 milliseconds to run 500000 iterations of
    execute if entity @e[tag=scoreAS,x=84,y=4,z=4,distance=..1.0,limit=1]
took 60 milliseconds to run 500000 iterations of
    execute if entity 1-1-1-0-1
took 1229 milliseconds to run 500000 iterations of
    scoreboard players add @p A 1
took 1186 milliseconds to run 500000 iterations of
    scoreboard players add @s A 1
took 21468 milliseconds to run 500000 iterations of
    scoreboard players add @e[tag=scoreAS] A 1
took 21588 milliseconds to run 500000 iterations of
    scoreboard players add @e[tag=scoreAS,type=armor_stand] A 1
took 1857 milliseconds to run 500000 iterations of
    scoreboard players add @e[tag=scoreAS,x=84,y=4,z=4,distance=..1.0,limit=1] A 1
took 605 milliseconds to run 500000 iterations of
    scoreboard players add x A 1
took 11651 milliseconds to run 500000 iterations of
    execute as @p run scoreboard players add @s A 1
took 11401 milliseconds to run 500000 iterations of
    execute as @s run scoreboard players add @s A 1
took 34943 milliseconds to run 500000 iterations of
    execute as @e[tag=scoreAS] run scoreboard players add @s A 1
took 35460 milliseconds to run 500000 iterations of
    execute as @e[tag=scoreAS,type=armor_stand] run scoreboard players add @s A 1
took 14980 milliseconds to run 500000 iterations of
    execute as @e[tag=scoreAS,x=84,y=4,z=4,distance=..1.0,limit=1] run scoreboard players add @s A 1
took 13354 milliseconds to run 500000 iterations of
    execute as 1-1-1-0-1 run scoreboard players add @s A 1

*)