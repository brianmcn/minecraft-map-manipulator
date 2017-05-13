module FunctionCompiler

open System.Collections.Generic 

////////////////////////

let ENTITY_UUID = "1-1-1-0-1"
let ENTITY_UUID_AS_FULL_GUID = "00000001-0001-0001-0000-000000000001"

////////////////////////

type Var(name:string) =
    // variables are 'globals', they are represented as objectives, and scores reside on the UUID'd entity
    do
        if name.Length > 14 then
            failwithf "Var name too long: %s" name
    member this.Name = name
    member this.AsCommandFragment() =
        sprintf "@s %s" name
    member this.AsCommandFragmentWithoutEntityBoundToAtS() =  // TODO find other places in existing code where this shoudl have been called
        sprintf "%s %s" ENTITY_UUID name
    member this.AsCommandFragment(cond:Conditional) =
        sprintf "@s%s %s" (cond.AsCommandFragment()) name
    // conditionals
    static member (.<=) (v,n) = SCMax(v,n)
    static member (.>=) (v,n) = SCMin(v,n)
    static member (.==) (v,n) = [| SCMin(v,n); SCMax(v,n) |]
    // operations
    static member (.=)  (a,b:int) = ScoreboardPlayersSet(a,b)
    static member (.=)  (a,b:Var) = ScoreboardOperationCommand(a,ASSIGN,b)
    static member (.+=) (a,b:int) = ScoreboardPlayersAdd(a,b)
    static member (.+=) (a,b:Var) = ScoreboardOperationCommand(a,PLUS_EQUALS,b)
    static member (.-=) (a,b:int) = ScoreboardPlayersRemove(a,b)
    static member (.-=) (a,b:Var) = ScoreboardOperationCommand(a,MINUS_EQUALS,b)
    static member (.*=) (a,b) = ScoreboardOperationCommand(a,TIMES_EQUALS,b)
    static member (./=) (a,b) = ScoreboardOperationCommand(a,DIVIDE_EQUALS,b)
    static member (.%=) (a,b) = ScoreboardOperationCommand(a,MOD_EQUALS,b)
    member this.Item with get(cond:Conditional) = ConditionalVar(this,cond)

and ConditionalVar = // a temporary type for operator overloading syntax   v.[cond]
    | ConditionalVar of Var * Conditional
    static member (.=)  (ConditionalVar(v,c),b:int) = ScoreboardPlayersConditionalSet(c,v,b)
and ScoreCondition =
    | SCMin of Var * int
    | SCMax of Var * int

and Conditional(conds:ScoreCondition[]) =
    do
        if conds.Length < 1 then
            failwith "bad conditional"
    member this.AsCommandFragment() =
        let sb = System.Text.StringBuilder("[")
        for i = 0 to conds.Length-1 do
            match conds.[i] with
            | SCMin(v,x) -> sb.Append(sprintf "score_%s_min=%d" v.Name x) |> ignore
            | SCMax(v,x) -> sb.Append(sprintf "score_%s=%d" v.Name x) |> ignore
            if i <> conds.Length-1 then
                sb.Append(",") |> ignore
        sb.Append("]") |> ignore
        sb.ToString()

and ScoreboardOperation = 
    | ASSIGN | PLUS_EQUALS | MINUS_EQUALS | TIMES_EQUALS | DIVIDE_EQUALS | MOD_EQUALS
    member this.AsCommandFragment() =
        match this with
        | ASSIGN         -> "="
        | PLUS_EQUALS    -> "+="
        | MINUS_EQUALS   -> "-="
        | TIMES_EQUALS   -> "*="
        | DIVIDE_EQUALS  -> "/="
        | MOD_EQUALS     -> "%="

and ScoreboardOperationCommand =
    | ScoreboardOperationCommand of Var * ScoreboardOperation * Var
    | ScoreboardPlayersConditionalSet of Conditional * Var * int
    | ScoreboardPlayersSet of Var * int
    | ScoreboardPlayersAdd of Var * int
    | ScoreboardPlayersRemove of Var * int
    member this.AsCommand() =
        match this with
        | ScoreboardPlayersAdd(v,x) -> (if x < 0 then failwith "bad x"); sprintf "scoreboard players add %s %d" (v.AsCommandFragment()) x
        | ScoreboardPlayersRemove(v,x) -> (if x < 0 then failwith "bad x"); sprintf "scoreboard players remove %s %d" (v.AsCommandFragment()) x
        | ScoreboardPlayersSet(v,x) -> sprintf "scoreboard players set %s %d" (v.AsCommandFragment()) x
        | ScoreboardPlayersConditionalSet(cond,v,x) -> sprintf "scoreboard players set %s %d" (v.AsCommandFragment(cond)) x
        | ScoreboardOperationCommand(a,op,b) -> sprintf "scoreboard players operation %s %s %s" (a.AsCommandFragment()) (op.AsCommandFragment()) (b.AsCommandFragment())

type FunctionCommand = 
    | Call of string
    | ConditionalCall of Conditional*string
    member this.AsCommand() =
        match this with
        | Call(s) -> sprintf "function %s" s
// TODO is @s always ok?
        | ConditionalCall(cond,s) -> sprintf "execute @s%s ~ ~ ~ function %s" (cond.AsCommandFragment()) s

type Scope() =
    let vars = ResizeArray()
    member this.RegisterVar(s) =
        let r = Var(s)
        vars.Add(r)
        r
    member this.All() = vars |> Seq.toArray 

////////////////////////

// TODO maybe make oneTimeInit a named function, written to disk that other modules call
// TODO move all the function code out of this assembly, can go in own, and MMM-like stuff can be in new assembly that refs both
type DropInModule = DropInModule of (*name*)string * (*oneTimeInit*) string[] * ((*name*) string * (*bodyCmds*) string[])[]

////////////////////////

type BasicBlockName = 
    | BBN of string
    member this.Name = match this with BBN(s) -> s
type FinalAbstractCommand =
    | DirectTailCall of BasicBlockName
    | ConditionalTailCall of Conditional*BasicBlockName*BasicBlockName // if-then-else
    | Halt
type AbstractCommand =
    | AtomicCommand of string // e.g. "tellraw blah", "worldborder ..."
    // TODO is this needed now that Visit() is removed?
    | AtomicCommandThunkFragment of (unit -> string) // e.g. fun () -> sprintf "something something %s" (v.AsCommandFragment())  // needs to be eval'd after whole program is visited
    | SB of ScoreboardOperationCommand
    | CALL of FunctionCommand 
    | Yield // express desire to yield CPU back to minecraft to run a tick after this block (cooperative multitasking)
    member this.AsCommand() =
        match this with
        | AtomicCommand s -> s
        | AtomicCommandThunkFragment f -> f()
        | SB soc -> soc.AsCommand()
        | CALL f -> f.AsCommand()
        | Yield -> failwith "should not get here Yield"
type BasicBlock = 
    | BasicBlock of AbstractCommand[] * FinalAbstractCommand
type Program = 
    | Program of DropInModule[] * (*one-time init*)AbstractCommand[] * (*entrypoint*)BasicBlockName * IDictionary<BasicBlockName,BasicBlock>

////////////////////////

// TODO this optimization would merge a BB that ends in a Yield & DirectTailCall into the next BB, which is incorrect semantics if the code needs MC to wait a tick
// Yield should not be an AbstractCommand, rather it should be a property of a basic block, I think maybe
let inlineAllDirectTailCallsOptimization(p) =
    match p with
    | Program(dependencyModules,init,entrypoint,origBlockDict) ->
        let finalBlockDict = new Dictionary<_,_>()
        let referencedBBNs = new HashSet<_>()
        referencedBBNs.Add(entrypoint) |> ignore
        for KeyValue(bbn,block) in origBlockDict do
            match block with
            | BasicBlock(cmds,DirectTailCall(nextBBN)) ->
                let finalCmds = ResizeArray(cmds)
                let mutable finished,nextBBN = false,nextBBN
                while not finished do
                    let nextBB = origBlockDict.[nextBBN]
                    match nextBB with
                    | BasicBlock(nextCmds,DirectTailCall(nextNextBBN)) ->
                        finalCmds.AddRange(nextCmds)
                        nextBBN <- nextNextBBN 
                    | BasicBlock(nextCmds,fac) ->
                        finalCmds.AddRange(nextCmds)
                        finalBlockDict.Add(bbn,BasicBlock(finalCmds.ToArray(),fac))
                        finished <- true
            | BasicBlock(_,fac) ->
                finalBlockDict.Add(bbn,block)
                match fac with
                | ConditionalTailCall(_conds,ifbbn,elsebbn) ->
                    referencedBBNs.UnionWith[elsebbn]
                    referencedBBNs.UnionWith[ifbbn]
                | _ -> ()
        let allBBNs = new HashSet<_>(origBlockDict.Keys)
        allBBNs.ExceptWith(referencedBBNs)
        for unusedBBN in allBBNs do
            printfn "removing unused state '%s' after optimization" unusedBBN.Name 
            finalBlockDict.Remove(unusedBBN) |> ignore
        Program(dependencyModules,init,entrypoint,finalBlockDict)

////////////////////////

let FUNCTION_NAMESPACE = "lorgon111" // functions folder name
let makeFunction(name,instructions) = (name,instructions|>Seq.toArray)

let functionCompilerVars = new Scope()
let IP = functionCompilerVars.RegisterVar("IP")
let YieldNow = functionCompilerVars.RegisterVar("YieldNow")
let Stop = functionCompilerVars.RegisterVar("Stop")
// TODO decide user-api of init/start/stop everything, and whether these vars make sense

let compileToFunctions(Program(dependencyModules,programInit,entrypoint,blockDict), isTracing) =
    let initialization = ResizeArray()
    let initialization2 = ResizeArray()  // runs a tick after initialization
    let mutable foundEntryPointInDictionary = false
(* TODO consider
    initialization.Add(AtomicCommand("gamerule maxCommandChainLength 999999")) // affects num total commands run in any function call
    initialization.Add(AtomicCommand("gamerule commandBlockOutput false"))
    initialization.Add(AtomicCommand("gamerule sendCommandFeedback false"))
    initialization.Add(AtomicCommand("gamerule logAdminCommands false"))
*)
    for v in functionCompilerVars.All() do
        initialization.Add(AtomicCommand(sprintf "scoreboard objectives add %s dummy" v.Name))
    initialization2.Add(SB(Stop .= 0))
    initialization2.Add(SB(YieldNow .= 0))
    let mutable nextBBNNumber = 1
    let bbnNumbers = new Dictionary<_,_>()
    for KeyValue(bbn,_) in blockDict do
        if bbn.Name.Length > 15 then
            failwithf "scoreboard names can only be up to 15 characters: %s" bbn.Name
        bbnNumbers.Add(bbn, nextBBNNumber)
        nextBBNNumber <- nextBBNNumber + 1
        if bbn = entrypoint then
            initialization2.Add(SB(IP .= bbnNumbers.[bbn]))
            foundEntryPointInDictionary <- true
    if not(foundEntryPointInDictionary) then
        failwith "did not find entrypoint in basic block dictionary"

    let visited = new HashSet<_>()
    let functions = ResizeArray()
    // runner infrastructure advancements at bottom of code further below
    let q = new Queue<_>()
    q.Enqueue(entrypoint)
    while q.Count <> 0 do
        let instructions = ResizeArray()
        let currentBBN = q.Dequeue()
        if not(visited.Contains(currentBBN)) then
            visited.Add(currentBBN) |> ignore
            let (BasicBlock(cmds,finish)) = blockDict.[currentBBN]
            if isTracing then
                instructions.Add(AtomicCommand(sprintf """tellraw @a ["start block: %s"]""" currentBBN.Name))
            for c in cmds do
                match c with 
                | AtomicCommand _s ->
                    instructions.Add(c)
                | AtomicCommandThunkFragment _f ->
                    instructions.Add(c)
                | SB(_soc) ->
                    instructions.Add(c)
                | CALL(_f) ->
                    instructions.Add(c)
                | Yield ->
                    instructions.Add(SB(YieldNow .= 1))
            match finish with
            | DirectTailCall(nextBBN) ->
                if not(blockDict.ContainsKey(nextBBN)) then
                    failwithf "bad DirectTailCall goto %s" nextBBN.Name
                q.Enqueue(nextBBN) |> ignore
                instructions.Add(SB(IP .= bbnNumbers.[nextBBN]))
            | ConditionalTailCall(conds,ifbbn,elsebbn) ->
                if not(blockDict.ContainsKey(elsebbn)) then
                    failwithf "bad ConditionalDirectTailCalls elsebbn %s" elsebbn.Name
                q.Enqueue(elsebbn) |> ignore
                // first set catchall
                instructions.Add(SB(IP .= bbnNumbers.[elsebbn]))
                // then do test, and if match overwrite
                if not(blockDict.ContainsKey(ifbbn)) then
                    failwithf "bad ConditionalDirectTailCalls %s" ifbbn.Name
                q.Enqueue(ifbbn) |> ignore
                instructions.Add(SB(IP.[conds] .= bbnNumbers.[ifbbn]))
            | Halt ->
                instructions.Add(SB(YieldNow .= 1))   // to get out of the pump
                instructions.Add(SB(Stop .= 1))    // to not run next tick
            functions.Add(makeFunction(currentBBN.Name, instructions |> Seq.map (fun c -> c.AsCommand())))
    let allBBNs = new HashSet<_>(blockDict.Keys)
    allBBNs.ExceptWith(visited)
    if allBBNs.Count <> 0 then
        failwithf "there were unreferenced basic block names, including for example %s" (allBBNs |> Seq.head).Name

    // function runner infrastructure
    functions.Add(makeFunction("start",[|
        sprintf "scoreboard players set %s %s 0" ENTITY_UUID YieldNow.Name
        sprintf "execute %s ~ ~ ~ execute @s[score_%s=0] ~ ~ ~ function %s:pump1" ENTITY_UUID Stop.Name FUNCTION_NAMESPACE
        |]))
    // pump loop (finite cps without deep recursion)
    let MAX_PUMP_DEPTH = 4
    let MAX_PUMP_WIDTH = 10   // (width ^ depth) is max iters
    for i = 1 to MAX_PUMP_DEPTH do
        functions.Add(makeFunction(sprintf"pump%d"i,[|
                for _x = 1 to MAX_PUMP_WIDTH do 
                    yield sprintf """execute @s[score_%s=0] ~ ~ ~ function %s:pump%d""" YieldNow.Name FUNCTION_NAMESPACE (i+1)
            |]))
    // TODO just reset gameLoopFunction as my chooser?!?
    // chooser
    functions.Add(makeFunction(sprintf"pump%d"(MAX_PUMP_DEPTH+1),[|
            for KeyValue(bbn,num) in bbnNumbers do 
                yield sprintf """execute @s[score_%s_min=%d,score_%s=%d] ~ ~ ~ function %s:%s""" 
                                    IP.Name num IP.Name num FUNCTION_NAMESPACE bbn.Name 
        |]))
    // TODO gameLoopFunction does not mix with other modules well; maybe use 'tick' and one-player guards (SMP) as a runner alternative
    // init
    for DropInModule(_,oneTimeInit,funcs) in dependencyModules do
        initialization2.AddRange(oneTimeInit |> Seq.map (fun cmd -> AtomicCommand(cmd)))
        functions.AddRange(funcs)
    initialization2.AddRange(programInit)
    let least,most = Utilities.toLeastMost(new System.Guid(ENTITY_UUID_AS_FULL_GUID))
    functions.Add(makeFunction("initialization",[|
            yield "kill @e[type=armor_stand]" // TODO too broad
            for c in initialization do
                yield c.AsCommand()
            yield sprintf "gamerule gameLoopFunction %s:inittick1" FUNCTION_NAMESPACE
        |]))
    functions.Add(makeFunction("inittick1",[|
            sprintf "gamerule gameLoopFunction %s:inittick2" FUNCTION_NAMESPACE
            // Note: cannot summon a UUID entity in same tick you killed entity with that UUID
            sprintf "summon armor_stand -3 4 -3 {CustomName:%s,NoGravity:1,UUIDMost:%dl,UUIDLeast:%dl,Invulnerable:1}" ENTITY_UUID most least
            // TODO what location is this entity? it needs to be safe in spawn chunks, but who knows where that is, hm, drop thru end portal?
            """tellraw @a ["tick1 called"]"""
        |]))
    functions.Add(makeFunction("inittick2",[|
            "gamerule gameLoopFunction -"
            """tellraw @a ["tick2 called"]"""
            // Note: seems you cannot refer to UUID in same tick you just summoned it
            sprintf "execute %s ~ ~ ~ function %s:initialization2" ENTITY_UUID FUNCTION_NAMESPACE
        |]))
    functions.Add(makeFunction("initialization2",[|
            // This is code that runs assuming @s has been set up (compiler SB init, and user code init)
            yield """tellraw @a ["2 called"]"""
            for c in initialization2 do
                yield c.AsCommand()
        |]))

    sprintf """function %s:initialization""" FUNCTION_NAMESPACE, functions

////////////////////////

// Mandelbrot

let cpsIStart = BBN"cpsistart"
let cpsJStart = BBN"cpsjstart"
let cpsInnerStart = BBN"cpsinnerstart"
let whileTest= BBN"whiletest"
let cpsInnerInner = BBN"cpsinnerinner"
let cpsInnerFinish = BBN"cpsinnerfinish"
let cpsJFinish = BBN"cpsjfinish"
let cpsIFinish = BBN"cpsifinish"

let mandelbrotVars = new Scope()

// constants
let FOURISSQ = mandelbrotVars.RegisterVar("FOURISSQ")
let INTSCALE = mandelbrotVars.RegisterVar("INTSCALE")
let MAXH = mandelbrotVars.RegisterVar("MAXH")
let MAXW = mandelbrotVars.RegisterVar("MAXW")
let XSCALE = mandelbrotVars.RegisterVar("XSCALE")
let YSCALE = mandelbrotVars.RegisterVar("YSCALE")
let XMIN = mandelbrotVars.RegisterVar("XMIN")
let YMIN = mandelbrotVars.RegisterVar("YMIN")
// variables
let i = mandelbrotVars.RegisterVar("i")
let j = mandelbrotVars.RegisterVar("j")
let x0 = mandelbrotVars.RegisterVar("x0")
let x = mandelbrotVars.RegisterVar("x")
let y0 = mandelbrotVars.RegisterVar("y0")
let y = mandelbrotVars.RegisterVar("y")
let n = mandelbrotVars.RegisterVar("n")
let xsq = mandelbrotVars.RegisterVar("xsq")
let ysq = mandelbrotVars.RegisterVar("ysq")
let r1 = mandelbrotVars.RegisterVar("r1")
let xtemp = mandelbrotVars.RegisterVar("xtemp")

let mandelbrotProgram = 
    Program([||],[|
            for v in mandelbrotVars.All() do
                yield AtomicCommand(sprintf "scoreboard objectives add %s dummy" v.Name)
            // color stuff
            yield AtomicCommand "scoreboard objectives add AS dummy"  // armor stands
#if DIRECT16COLORTEST
#else
            for i = 0 to 15 do
                let y,z = 4,-2
                yield AtomicCommand(sprintf "setblock %d %d %d wool %d" i y z i)
                yield AtomicCommand(sprintf "summon armor_stand %d %d %d" i y z)
                yield AtomicCommand(sprintf "scoreboard players set @e[type=armor_stand,x=%d,y=%d,z=%d,c=1] AS %d" i y z i)
                yield AtomicCommand(sprintf "scoreboard players tag @e[type=armor_stand,x=%d,y=%d,z=%d,c=1] add color" i y z)
#endif
            yield AtomicCommand "summon armor_stand 0 4 0 {CustomName:Cursor,NoGravity:1}"
            // constants
            yield SB(FOURISSQ .= 64000000)
            yield SB(INTSCALE .= 4000)
            yield SB(MAXH .= 128)
            yield SB(MAXW .= 128)
            yield SB(XSCALE .= 96)
            yield SB(YSCALE .= 62)
            yield SB(XMIN .= -8400)
            yield SB(YMIN .= -4000)
        |],cpsIStart, dict [
        cpsIStart,BasicBlock([|
            // time measurement
            yield AtomicCommand "worldborder set 10000000"
            yield AtomicCommand "worldborder add 1000000 1000000"
            // actual code
            yield SB(i .= 0)
            yield AtomicCommand "tp @e[name=Cursor] 0 14 0"
            yield AtomicCommand "fill 0 14 0 127 14 127 air"
            yield AtomicCommand "fill 0 13 0 127 13 127 wool 0"
            |],DirectTailCall(cpsJStart))
        cpsJStart,BasicBlock([|
            SB(j .= 0)
            AtomicCommand "tp @e[name=Cursor] ~ ~ 0"
            |],DirectTailCall(cpsInnerStart))
        cpsInnerStart,BasicBlock([|
            SB(x0 .= i)
            SB(x0 .*= XSCALE)
            SB(x0 .+= XMIN)
            SB(y0 .= j)
            SB(y0 .*= YSCALE)
            SB(y0 .+= YMIN)
            SB(x .= 0)
            SB(y .= 0)
            SB(n .= 0)
            |],DirectTailCall(whileTest))
        whileTest,BasicBlock([|
            SB(xsq .= x)
            SB(xsq .*= x)
            SB(ysq .= y)
            SB(ysq .*= y)
            SB(r1 .= xsq)
            SB(r1 .+= ysq)
            SB(r1 .-= FOURISSQ)
            |],ConditionalTailCall(Conditional[| r1 .<= -1; n .<= 15 |], cpsInnerInner, cpsInnerFinish))
        cpsInnerInner,BasicBlock([|
            SB(xtemp .= xsq)
            SB(xtemp .-= ysq)
            SB(xtemp ./= INTSCALE)
            SB(xtemp .+= x0)
            SB(y .*= x)
            SB(y .+= y)
            SB(y ./= INTSCALE)
            SB(y .+= y0)
            SB(x .= xtemp)
            SB(n .+= 1)
            |],DirectTailCall(whileTest))
        cpsInnerFinish,BasicBlock([|
#if DIRECT16COLORTEST
            yield AtomicCommandThunkFragment(fun () -> sprintf "scoreboard players operation @e[name=Cursor] %s = %s" n.Name (n.AsCommandFragment()))
            for zzz = 0 to 15 do
                yield AtomicCommand(sprintf "execute @e[name=Cursor,score_%s=%d,score_%s_min=%d] ~ ~ ~ setblock ~ ~ ~ wool %d" n.Name (zzz+1) n.Name (zzz+1) zzz)
#else
            yield AtomicCommandThunkFragment(fun () -> sprintf "scoreboard players operation @e[tag=color] AS -= %s" (n.AsCommandFragment()))
            yield AtomicCommand "execute @e[tag=color,score_AS=-1,score_AS_min=-1] ~ ~ ~ clone ~ ~ ~ ~ ~ ~ 0 4 0"
            yield AtomicCommandThunkFragment(fun() -> sprintf "scoreboard players operation @e[tag=color] AS += %s" (n.AsCommandFragment()))
            yield AtomicCommand "execute @e[name=Cursor] ~ ~ ~ clone 0 4 0 0 4 0 ~ ~ ~"
#endif
            yield SB(j .+= 1)
            yield AtomicCommand "execute @e[name=Cursor] ~ ~ ~ tp @e[c=1] ~ ~ ~1"
            //yield Yield  // inner loop yield
            yield SB(r1 .= j)
            yield SB(r1 .-= MAXH)
            |],ConditionalTailCall(Conditional[| r1 .<= -1 |], cpsInnerStart, cpsJFinish))
        cpsJFinish,BasicBlock([|
            SB(i .+= 1)
            AtomicCommand "execute @e[name=Cursor] ~ ~ ~ tp @e[c=1] ~1 ~ ~"
            Yield
            SB(r1 .= i)
            SB(r1 .-= MAXW)
            |],ConditionalTailCall(Conditional[| r1 .<= -1 |], cpsJStart, cpsIFinish))
        cpsIFinish,BasicBlock([|
            AtomicCommand """tellraw @a ["done!"]"""
            // time measurement
            AtomicCommand("stats entity @e[name=Cursor] set QueryResult @e[name=Cursor] A")
            AtomicCommand("scoreboard players set @e[name=Cursor] A 1") // need initial value before can trigger a stat
            AtomicCommand "execute @e[name=Cursor] ~ ~ ~ worldborder get"
            AtomicCommand "scoreboard players set Time A -10000000"
            AtomicCommand "scoreboard players operation Time A += @e[name=Cursor] A"
            AtomicCommand """tellraw @a ["took ",{"score":{"name":"Time","objective":"A"}}," seconds"]"""
            |],Halt)
        ])


    