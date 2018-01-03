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

    profileThis("stoneairstoneif",OUTER,INNER,1,[],["execute if block ~ ~ ~ stone run setblock ~ ~ ~ air"; "setblock ~ ~ ~ stone"],[])
    profileThis("stoneairstonefill",OUTER,INNER,1,[],["fill ~ ~ ~ ~ ~ ~ air replace stone"; "setblock ~ ~ ~ stone"],[])
(*
took 13796 milliseconds to run 200000 iterations of
    execute if block ~ ~ ~ stone run setblock ~ ~ ~ air
    setblock ~ ~ ~ stone
took 7441 milliseconds to run 200000 iterations of
    fill ~ ~ ~ ~ ~ ~ air replace stone
    setblock ~ ~ ~ stone
*)

    // TODO how to profile 'summon'?  can't just summon 100,000 entities in a tick...
    // Maybe use an RCB and a tick counter, and do like 1000 at a time? but then wall-clock time would include latency between ticks (unless game was fully lagged)
    //profileThis("summon",       10,1000,1,[],["summon area_effect_cloud ~ ~ ~ {Duration:1}"],[])   // was about 300ms to summon 10,000 aecs (but their death costs not measured); would be 6000ms for 200k if linear scale

    (*
FAR ANIMALS
took 417 milliseconds to run 50000 iterations of
    execute at @p as @e[type=pig,sort=nearest,distance=..7,limit=1] run scoreboard players add @p A 1
took 340 milliseconds to run 50000 iterations of
    execute at @p as @e[type=pig,distance=..7,limit=1] run scoreboard players add @p A 1
took 4234 milliseconds to run 50000 iterations of
    execute at @p as @e[type=pig,distance=..57,limit=1] run scoreboard players add @p A 1
NEAR PIG
took 2843 milliseconds to run 50000 iterations of
    execute at @p as @e[type=pig,sort=nearest,distance=..7,limit=1] run scoreboard players add @p A 1
took 2776 milliseconds to run 50000 iterations of
    execute at @p as @e[type=pig,distance=..7,limit=1] run scoreboard players add @p A 1
took 7501 milliseconds to run 50000 iterations of
    execute at @p as @e[type=pig,distance=..57,limit=1] run scoreboard players add @p A 1
    *)
    profileThis("nearby1",       50,1000,1,[],["execute at @p as @e[type=pig,sort=nearest,distance=..7,limit=1] run scoreboard players add @p A 1"],[])
    profileThis("nearby2",       50,1000,1,[],["execute at @p as @e[type=pig,distance=..7,limit=1] run scoreboard players add @p A 1"],[])
    profileThis("nearby3",       50,1000,1,[],["execute at @p as @e[type=pig,distance=..57,limit=1] run scoreboard players add @p A 1"],[])

    
    // TODO someone suggests that @e[tag=X,type=armor_stand] might be more efficient than @e[tag=X] if limited to a specific chunk/section, so test that
    // AjaxGb: Essentially, each subchunk (16x16x16) stores a hashmap of type -> list of entities. These hashmaps only get used if you have r or dx/dy/dz in play, so without them type will have no special efficiency.
    // MrPingouin: r and dx/dy/dz are the same, they both define a bounding box
    // AjaxGb: Regarding ClassInheritanceMultiMap (entity-by-type) By default, the map only maps the base Entity class. Scanning for entities of a specific class will add a mapping for that class, which will 
    // be kept up-to-date. Entity selectors only scan for Entity and then do their own filtering afterwards, so no subdivision (or related efficiency improvement/hit) will occur.
    // Many other systems do cause such subdivisions. For example, hoppers scan for items, so any subchunk with a hopper will have an EntityItem subdivision.
    // me: since a common place for a hopper is e.g. at a mob farm, so there are lots of entities in a small location, so type filtering the small location is actually a win for that scenario

    // TODO could author tests to dump a bunch of text/numbers, but also have some expected ratios, and print e.g. UNEXPECTED if something is out-of-tolerance, to highlight problems in the text dump

    // TODO 
    (*What has less impact on performance:
Execute as @s[scores={test=1}] run ...
Execute if @s[scores={test=1}] run ...
Or wouldnt there be a difference?
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