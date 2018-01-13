module PerformanceMicroBenchmarks

let PACK_NAME = "PerfPack"
let FOLDER = System.IO.Path.Combine(Utilities.MC_ROOT, """TestPerf""")

let allProfilerFunctions = ResizeArray()

let profileThis(suffix,outer,inner,innerInner,pre,cmds,post) =
    let profName = "run/run-"+suffix
    let all = [|
        yield profName,[|
            "execute as @e[tag=scoreAS] at @s run function test:prof-" + suffix
            |]
        yield "prof-"+suffix,[|
            yield "worldborder set 10000000" 
            yield "worldborder add 1000000 1000" 
        
            yield! pre
            for _i = 1 to outer do
                yield sprintf "function %s:code/code-%s" "test" suffix
            yield! post

            yield "execute store result score DATA WB run worldborder get" 
            yield "scoreboard players set Time A -10000000" 
            yield "scoreboard players operation Time A += DATA WB" 
            yield "scoreboard players operation @p WB = Time A"
            yield sprintf """tellraw @a ["took ",{"score":{"name":"@p","objective":"WB"}}," milliseconds to run %d iterations of"]""" (outer*inner*innerInner)
            for cmd in cmds do
                yield sprintf """tellraw @a ["    %s"]""" (Utilities.escape cmd)
            |]
        if innerInner=1 then
            yield "code/code-"+suffix,[|
                for _i = 1 to inner do 
                    yield! cmds 
                |]
        else
            yield "code/code-"+suffix,[|
                for _i = 1 to inner do 
                    yield sprintf "function %s:inner/inner-%s" "test" suffix
                |]
            yield "inner/inner-"+suffix,[|
                for _i = 1 to innerInner do 
                    yield! cmds 
                |]
        |]
    for name,code in all do
        Utilities.writeFunctionToDisk(FOLDER, PACK_NAME, "test", name, code)
    allProfilerFunctions.Add(profName)

(*
execute pseudocode (imagined)

let instr = instructions.pop_front()
decode(instr)
    case: "execute as @e run FOO"
        let ents = <list of entities>
        let ctxt = new Context(CURRENT_SENDER, CURRENT_AT)   // context actually has:  sender, x y z, yrot xrot, dimension, anchor (feet/eyes), perm., output-suppression, callback (where to 'store' results)
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


// regarding "tagtype" below
// someone suggests that @e[tag=X,type=armor_stand] might be more efficient than @e[tag=X] if limited to a specific chunk/section, so test that
// AjaxGb: Essentially, each subchunk (16x16x16) stores a hashmap of type -> list of entities. These hashmaps only get used if you have r or dx/dy/dz in play, so without them type will have no special efficiency.
// MrPingouin: r and dx/dy/dz are the same, they both define a bounding box
// AjaxGb: Regarding ClassInheritanceMultiMap (entity-by-type) By default, the map only maps the base Entity class. Scanning for entities of a specific class will add a mapping for that class, which will 
// be kept up-to-date. Entity selectors only scan for Entity and then do their own filtering afterwards, so no subdivision (or related efficiency improvement/hit) will occur.
// Many other systems do cause such subdivisions. For example, hoppers scan for items, so any subchunk with a hopper will have an EntityItem subdivision.
// me: since a common place for a hopper is e.g. at a mob farm, so there are lots of entities in a small location, so type filtering the small location is actually a win for that scenario

let SELECTORS = [|
    "p",      "@p"
    "s",      "@s"
    "tag",    "@e[tag=scoreAS]"
    "name",    "@e[name=scoreAS]"
    "tagtype","@e[tag=scoreAS,type=armor_stand]"
    "tagdist","@e[tag=scoreAS,distance=..1]"
    "namedist","@e[name=scoreAS,distance=..1]"
    |]
let SELECTORS_WITH_FAKE = [|
    yield! SELECTORS
    yield "fake", "x"
    |]
let SELECTORS_WITH_UUID = [|
    yield! SELECTORS
    yield "u", "1-1-1-0-1"
    |]

let main() =
    Utilities.writeDatapackMeta(FOLDER, PACK_NAME, "Performance testing")

    Utilities.writeFunctionTagsFileWithValues(FOLDER, PACK_NAME, "minecraft", "load", ["test:on_load"])
    Utilities.writeFunctionTagsFileWithValues(FOLDER, PACK_NAME, "minecraft", "tick", ["test:on_tick"])

    Utilities.writeFunctionToDisk(FOLDER, PACK_NAME, "test", "on_load", [|
        "kill @e[type=armor_stand]"
        """summon armor_stand ~ ~ ~ {CustomName:"\"scoreAS\"",Tags:["scoreAS"],NoGravity:1,Marker:1,Invulnerable:1,Invisible:1}"""  // ~ ~ ~ because #load now runs at world spawn

        "gamerule maxCommandChainLength 999999"
        "gamerule commandBlockOutput false"
        "gamerule sendCommandFeedback false"
        "gamerule logAdminCommands false"

        "scoreboard objectives add A dummy"
        "scoreboard objectives add WB dummy"
        "scoreboard objectives add NF dummy"
        "scoreboard objectives add FailureCount dummy"

        "scoreboard objectives setdisplay sidebar A"

        "scoreboard players set DATA WB 1" 
        "scoreboard players set @p NF -1"
        "scoreboard players set @p FailureCount 0"

        // convenience for various tests to use:
        "scoreboard objectives add TEMP_OBJ dummy"

        "say datapack loaded, initialization complete"
        "say after terrain all loaded and settled, run"
        """tellraw @a ["    scoreboard players set @p NF 0"]"""
        "say to run various performance tests begun as&at"
        "say an invisible armor_stand at world spawn"
        |])

    let SEED = "seed"
    let ADD = "scoreboard players add @s A 1"
    Utilities.writeFunctionToDisk(FOLDER, PACK_NAME, "test", "call_seed", [|SEED|])
    Utilities.writeFunctionToDisk(FOLDER, PACK_NAME, "test", "call_add", [|ADD|])
    Utilities.writeFunctionToDisk(FOLDER, PACK_NAME, "test", "fail", [|
        "execute if entity @p[scores={FailureCount=0}] run say FIRST FAILURE!!! DEBUG THIS!!!"
        "execute if entity @p[scores={FailureCount=0}] run say FIRST FAILURE!!! DEBUG THIS!!!"
        "execute if entity @p[scores={FailureCount=0}] run say FIRST FAILURE!!! DEBUG THIS!!!"
        "scoreboard players add @p FailureCount 1"
        |])

    let OUTER = 200
    let INNER = 1000

    ///////////
    // good baseline stuff to keep

    // trivial calls, functions
    profileThis("seed",       OUTER,INNER,1,[],[SEED],[]) 
    profileThis("callseed",   OUTER,INNER,1,[],["function test:call_seed"],[])    // TODO 500,1000 just fails silently and immediately, why?  past maxCommandChainLength?
    profileThis("add",        OUTER,INNER,1,[],[ADD],[])
    profileThis("calladd",    OUTER,INNER,1,[],["function test:call_add"],[])

    // some bits that show off execute overhead
    for name,sel in ["s","@s"] do
        profileThis("eif"+name,OUTER,INNER,1,[],[sprintf "execute if entity %s" sel],[])  // the new 'testfor'
        profileThis("eatif"+name,OUTER,INNER,1,[],[sprintf "execute at %s if entity @s" sel],[])
        profileThis("eatifseed"+name,OUTER,INNER,1,[],[sprintf "execute at %s if entity @s" sel; SEED],[])
        profileThis("eatifrunseed"+name,OUTER,INNER,1,[],[sprintf "execute at %s if entity @s run %s" sel SEED],[])
        profileThis("eatifadd"+name,OUTER,INNER,1,[],[sprintf "execute at %s if entity @s" sel; ADD],[])
        profileThis("eatifrunadd"+name,OUTER,INNER,1,[],[sprintf "execute at %s if entity @s run %s" sel ADD],[])
    profileThis("erunadd",OUTER,INNER,1,[],[sprintf "execute run %s" ADD],[])
    profileThis("erunseed",OUTER,INNER,1,[],[sprintf "execute run %s" SEED],[])
    profileThis("erunseed3",OUTER,INNER,1,[],[sprintf "execute run execute run execute run %s" SEED],[])
    profileThis("erunseed4",OUTER,INNER,1,[],[sprintf "execute run execute run execute run execute run %s" SEED],[])

    // NBT serialization versus alternatives
    profileThis("readtagtag",OUTER,INNER,1,["tag @p add SomeTag"],["execute unless entity @p[tag=SomeTag] run function test:fail"],["tag @p remove SomeTag"])
    profileThis("readtagnbt",OUTER,INNER,1,["tag @p add SomeTag"],["""execute unless entity @p[nbt={Tags:["SomeTag"]}] run function test:fail"""],["tag @p remove SomeTag"])
    let MAX = 10
    profileThis("readtagstag",OUTER,INNER,1,[for i=1 to MAX do yield sprintf "tag @p add SomeTag%d" i],["execute unless entity @p[tag=SomeTag1] run function test:fail"],[for i=1 to MAX do yield sprintf "tag @p remove SomeTag%d" i])
    profileThis("readtagsnbt",OUTER,INNER,1,[for i=1 to MAX do yield sprintf "tag @p add SomeTag%d" i],["""execute unless entity @p[nbt={Tags:["SomeTag1"]}] run function test:fail"""],[for i=1 to MAX do yield sprintf "tag @p remove SomeTag%d" i])
    profileThis("readtagstagmax",OUTER,INNER,1,[for i=1 to MAX do yield sprintf "tag @p add SomeTag%d" i],[sprintf"execute unless entity @p[tag=SomeTag%d] run function test:fail"MAX],[for i=1 to MAX do yield sprintf "tag @p remove SomeTag%d" i])
    profileThis("readtagsnbtmax",OUTER,INNER,1,[for i=1 to MAX do yield sprintf "tag @p add SomeTag%d" i],[sprintf"""execute unless entity @p[nbt={Tags:["SomeTag%d"]}] run function test:fail"""MAX],[for i=1 to MAX do yield sprintf "tag @p remove SomeTag%d" i])
    profileThis("readtagstagtwo",OUTER,INNER,1,[for i=1 to MAX do yield sprintf "tag @p add SomeTag%d" i],[sprintf"execute unless entity @p[tag=SomeTag%d,tag=SomeTag%d] run function test:fail"2 (MAX-1)],[for i=1 to MAX do yield sprintf "tag @p remove SomeTag%d" i])
    profileThis("readtagsnbttwo",OUTER,INNER,1,[for i=1 to MAX do yield sprintf "tag @p add SomeTag%d" i],[sprintf"""execute unless entity @p[nbt={Tags:["SomeTag%d","SomeTag%d"]}] run function test:fail"""2 (MAX-1)],[for i=1 to MAX do yield sprintf "tag @p remove SomeTag%d" i])

    // various selectors
    for name,sel in SELECTORS_WITH_UUID do
        profileThis("ei"+name,OUTER,INNER,1,[],[sprintf "execute unless entity %s run function %s:fail" sel "test"],[])  // the new 'testfor'

    // carets versus tildes (will move player to world spawn)
    profileThis("tpcarrot",   OUTER,INNER,1,[],["tp @p ^ ^ ^1";"tp @p ^ ^ ^-1"],[])
    profileThis("tptilde",   OUTER,INNER,1,[],["tp @p ~ ~ ~1";"tp @p ~ ~ ~-1"],[])

    // selector score literals versus 'matches'
    profileThis("scorelitsel",OUTER,INNER,1,["scoreboard players set @s TEMP_OBJ 1"],["execute unless entity @s[scores={TEMP_OBJ=1}] run function test:fail"],[])
    profileThis("scorelitmat",OUTER,INNER,1,["scoreboard players set @s TEMP_OBJ 1"],["execute unless score @s TEMP_OBJ matches 1 run function test:fail"],[])

    // type=player versus @a
    profileThis("ei-ep",OUTER,INNER,1,[],[sprintf "execute unless entity @e[type=player] run function test:fail"],[])
    profileThis("ei-a",OUTER,INNER,1,[],[sprintf "execute unless entity @a run function test:fail"],[])


(*
    profileThis("stoneairstoneif",OUTER,INNER,1,[],["execute if block ~ ~ ~ stone run setblock ~ ~ ~ air"; "setblock ~ ~ ~ stone"],[])
    profileThis("stoneairstonefill",OUTER,INNER,1,[],["fill ~ ~ ~ ~ ~ ~ air replace stone"; "setblock ~ ~ ~ stone"],[])
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


    // TODO @e[distance=0..] will ensure you only search executor's current dimension (e.g. overworld) and avoid nether/end

    (*
setblock on edge of loaded chunks seems to create next chunk over:
Tested it
[6:22 PM] tryashtar: Seems to be correct -- placing one at the border opens up the adjacent chunk for placing.
[6:27 PM] Lorgon111: Can you test for blocks (e.g. does the terrain generator run?)
Did it work in 1.12?
[6:30 PM] Lorgon111: (if the former, then exploiting this would create a way to do automated tests of terrain generator performance)

Also

if you LOAD a structure block here in loaded chunks, but the structure has e.g. offsetX = 10000, it will 
  load that chunk right now (this tick) so you can setblock into that (previously unloaded) chunk right now
    *)


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
//    profileThis("nearby1",       50,1000,1,[],["execute at @p as @e[type=pig,sort=nearest,distance=..7,limit=1] run scoreboard players add @p A 1"],[])
//    profileThis("nearby2",       50,1000,1,[],["execute at @p as @e[type=pig,distance=..7,limit=1] run scoreboard players add @p A 1"],[])
//    profileThis("nearby3",       50,1000,1,[],["execute at @p as @e[type=pig,distance=..57,limit=1] run scoreboard players add @p A 1"],[])

    

    // TODO could author tests to dump a bunch of text/numbers, but also have some expected ratios, and print e.g. UNEXPECTED if something is out-of-tolerance, to highlight problems in the text dump

    // TODO 
    (*What has less impact on performance:
Execute as @s[scores={test=1}] run ...
Execute if @s[scores={test=1}] run ...
Or wouldnt there be a difference?
*)




    (*

took 49 milliseconds to run 200000 iterations of
    execute unless entity @p[tag=SomeTag] run function test:fail
took 1104 milliseconds to run 200000 iterations of
    execute unless entity @p[nbt={Tags:["SomeTag"]}] run function test:fail

took 1196 milliseconds to run 200000 iterations of
    execute unless entity @p[nbt={Tags:["SomeTag1"]}] run function test:fail
took 52 milliseconds to run 200000 iterations of
    execute unless entity @p[tag=SomeTag1] run function test:fail

took 52 milliseconds to run 200000 iterations of
    execute unless entity @p[tag=SomeTag10] run function test:fail
took 1226 milliseconds to run 200000 iterations of
    execute unless entity @p[nbt={Tags:["SomeTag10"]}] run function test:fail

took 53 milliseconds to run 200000 iterations of
    execute unless entity @p[tag=SomeTag2,tag=SomeTag9] run function test:fail
took 1257 milliseconds to run 200000 iterations of
    execute unless entity @p[nbt={Tags:["SomeTag2","SomeTag9"]}] run function test:fail

    *)


(*

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
*)
// note: these do not have much effect... was trying to test if the 'number of instructions remaining in the continuation' affects the cost of invoking a function (assuming it 'inserts instructions to front of current list')
//        profileThis("eiseed"+name,5000,100,1,[],[sprintf "execute if entity %s run seed" sel],[])
//        profileThis("easeed"+name,5000,100,1,[],[sprintf "execute as %s run seed" sel],[])
//        profileThis("eiseed"+name,50,100,100,[],[sprintf "execute if entity %s run seed" sel],[])
//        profileThis("easeed"+name,50,100,100,[],[sprintf "execute as %s run seed" sel],[])


    ///////////
    // current experiments


    (*
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
        // Note: we summon the uuidguy here, rather than in #load, because you cannot kill and summon same uuid in same tick
        yield sprintf """execute if entity @p[scores={NF=0}] unless entity @e[tag=uuidguy] at @e[tag=scoreAS] run summon armor_stand ~ ~ ~80 {CustomName:"\"%s\"",Tags:["uuidguy"],NoGravity:1,Marker:1,Invulnerable:1,Invisible:1,UUIDMost:%dl,UUIDLeast:%dl}""" 
            MinecraftBINGO.ENTITY_UUID MinecraftBINGO.most MinecraftBINGO.least  // ~ ~ ~80 to have him out of scoreAS's chunks but within spawn chunks

        for i = 0 to allProfilerFunctions.Count-1 do
            let funcName = allProfilerFunctions.[i]
            yield sprintf "execute if entity @p[scores={NF=%d}] run function test:%s" i funcName
        yield sprintf """execute if entity @p[scores={NF=%d}] run tellraw @a ["done with all tests!"]""" allProfilerFunctions.Count
        yield sprintf """execute if entity @p[scores={NF=%d}] run tellraw @a ["There were ",{"score":{"name":"@p","objective":"FailureCount"}}," failures that need debugging.  Run"]""" allProfilerFunctions.Count
        yield sprintf """execute if entity @p[scores={NF=%d}] run tellraw @a ["    scoreboard players set @p NF 0"]""" allProfilerFunctions.Count
        yield sprintf """execute if entity @p[scores={NF=%d}] run tellraw @a ["to restart"]""" allProfilerFunctions.Count
        yield "execute if entity @p[scores={NF=0..}] run scoreboard players add @p NF 1"
        |]
    Utilities.writeFunctionToDisk(FOLDER, PACK_NAME, "test", "on_tick", next)

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





18w01a
took 32 milliseconds to run 200000 iterations of
    seed
took 103 milliseconds to run 200000 iterations of
    function test:call_seed
took 485 milliseconds to run 200000 iterations of
    scoreboard players add @s A 1
took 962 milliseconds to run 200000 iterations of
    function test:call_add
took 19 milliseconds to run 200000 iterations of
    execute if entity @s
took 517 milliseconds to run 200000 iterations of
    execute at @s if entity @s
took 643 milliseconds to run 200000 iterations of
    execute at @s if entity @s
    seed
took 3685 milliseconds to run 200000 iterations of
    execute at @s if entity @s run seed
took 2450 milliseconds to run 200000 iterations of
    execute at @s if entity @s
    scoreboard players add @s A 1
took 8020 milliseconds to run 200000 iterations of
    execute at @s if entity @s run scoreboard players add @s A 1
took 2692 milliseconds to run 200000 iterations of
    execute run seed
took 6703 milliseconds to run 200000 iterations of
    execute run scoreboard players add @s A 1



18w02a
took 29 milliseconds to run 200000 iterations of
    seed
took 101 milliseconds to run 200000 iterations of
    function test:call_seed
took 597 milliseconds to run 200000 iterations of
    scoreboard players add @s A 1
took 957 milliseconds to run 200000 iterations of
    function test:call_add
took 24 milliseconds to run 200000 iterations of
    execute if entity @s
took 688 milliseconds to run 200000 iterations of
    execute at @s if entity @s
took 723 milliseconds to run 200000 iterations of
    execute at @s if entity @s
    seed
took 4116 milliseconds to run 200000 iterations of
    execute at @s if entity @s run seed
took 2394 milliseconds to run 200000 iterations of
    execute at @s if entity @s
    scoreboard players add @s A 1
took 8541 milliseconds to run 200000 iterations of
    execute at @s if entity @s run scoreboard players add @s A 1
took 6969 milliseconds to run 200000 iterations of
    execute run scoreboard players add @s A 1
took 2815 milliseconds to run 200000 iterations of
    execute run seed
took 9076 milliseconds to run 200000 iterations of
    execute run execute run execute run seed
took 12264 milliseconds to run 200000 iterations of
    execute run execute run execute run execute run seed
took 48 milliseconds to run 200000 iterations of
    execute unless entity @p[tag=SomeTag] run function test:fail
took 1065 milliseconds to run 200000 iterations of
    execute unless entity @p[nbt={Tags:["SomeTag"]}] run function test:fail
took 47 milliseconds to run 200000 iterations of
    execute unless entity @p[tag=SomeTag1] run function test:fail
took 1097 milliseconds to run 200000 iterations of
    execute unless entity @p[nbt={Tags:["SomeTag1"]}] run function test:fail
took 48 milliseconds to run 200000 iterations of
    execute unless entity @p[tag=SomeTag10] run function test:fail
took 1098 milliseconds to run 200000 iterations of
    execute unless entity @p[nbt={Tags:["SomeTag10"]}] run function test:fail
took 53 milliseconds to run 200000 iterations of
    execute unless entity @p[tag=SomeTag2,tag=SomeTag9] run function test:fail
took 1125 milliseconds to run 200000 iterations of
    execute unless entity @p[nbt={Tags:["SomeTag2","SomeTag9"]}] run function test:fail
took 50 milliseconds to run 200000 iterations of
    execute unless entity @p run function test:fail
took 39 milliseconds to run 200000 iterations of
    execute unless entity @s run function test:fail
took 2648 milliseconds to run 200000 iterations of
    execute unless entity @e[tag=scoreAS] run function test:fail
took 20253 milliseconds to run 200000 iterations of
    execute unless entity @e[name=scoreAS] run function test:fail
took 2775 milliseconds to run 200000 iterations of
    execute unless entity @e[tag=scoreAS,type=armor_stand] run function test:fail
took 170 milliseconds to run 200000 iterations of
    execute unless entity @e[tag=scoreAS,distance=..1] run function test:fail
took 170 milliseconds to run 200000 iterations of
    execute unless entity @e[name=scoreAS,distance=..1] run function test:fail
took 41 milliseconds to run 200000 iterations of
    execute unless entity 1-1-1-0-1 run function test:fail
took 3927 milliseconds to run 200000 iterations of
    tp @p ^ ^ ^1
    tp @p ^ ^ ^-1
took 4142 milliseconds to run 200000 iterations of
    tp @p ~ ~ ~1
    tp @p ~ ~ ~-1
took 67 milliseconds to run 200000 iterations of
    execute unless entity @s[scores={TEMP_OBJ=1}] run function test:fail
took 73 milliseconds to run 200000 iterations of
    execute unless score @s TEMP_OBJ matches 1 run function test:fail
took 85 milliseconds to run 200000 iterations of
    execute unless entity @e[type=player] run function test:fail
took 44 milliseconds to run 200000 iterations of
    execute unless entity @a run function test:fail

*)