module FunctionCompiler

open System.Collections.Generic 

////////////////////////

let ENTITY_UUID = "1-1-1-0-1"
let ENTITY_UUID_AS_FULL_GUID = "00000001-0001-0001-0000-000000000001"

let WBTIMER_UUID = "1-1-1-0-2"
let WBTIMER_UUID_AS_FULL_GUID = "00000001-0001-0001-0000-000000000002"

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
type DropInModule = DropInModule of (*name*)string * (*oneTimeInit*) string[] * (*oneTimeInit a tick later*) string[] * ((*name*) string * (*bodyCmds*) string[])[]

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
    // TODO is line below best programming model?
    | AtomicCommandWithExtraCost of string * int     // e.g. ("function foo:bar",50) declares that we expect this statement to cost about 50x as much as other statements, for both lag prediction & lag attribution
    | AtomicCommandThunkFragment of (unit -> string) // e.g. fun () -> sprintf "something something %s" (v.AsCommandFragment())  // needs to be eval'd after whole program is visited
    | SB of ScoreboardOperationCommand
    | CALL of FunctionCommand 
    member this.AsCommand() =
        match this with
        | AtomicCommand s -> s
        | AtomicCommandWithExtraCost(s,_) -> s
        | AtomicCommandThunkFragment f -> f()
        | SB soc -> soc.AsCommand()
        | CALL f -> f.AsCommand()
type WaitPolicy =
    | MustNotYield  // next block must run after this one without interleaving or tick
    | NoPreference
    | MustWaitNTicks of int  // next block should not be run until N ticks in the future (N must be greater than 0)
type BasicBlock = 
    | BasicBlock of AbstractCommand[] * FinalAbstractCommand * WaitPolicy
type Program = 
    // TODO Scope used only for variable-uniqueness-checking by compiler, consider also having compiler be responsibile for creating objectives for each var in the scope?
    | Program of Scope * DropInModule[] * (*one-time init*)AbstractCommand[] * (*one-time init a tick later*)AbstractCommand[] * (*entrypoint*)BasicBlockName * IDictionary<BasicBlockName,BasicBlock>

////////////////////////

// TODO this code needs more correctness testing
let inlineDirectTailCallsOptimization(p) =
    let merge(incomingWP, outgoingWP) =
        match incomingWP, outgoingWP with
        | MustNotYield, MustNotYield -> Some MustNotYield 
        | NoPreference, NoPreference -> Some NoPreference 
        | NoPreference, MustWaitNTicks n -> Some(MustWaitNTicks n)
        | _ -> None
    let canMerge(i,o) = merge(i,o) |> Option.isSome 
    match p with
    | Program(scope,dependencyModules,init1,init2,entrypoint,origBlockDict) ->
        let finalBlockDict = new Dictionary<_,_>()
        let referencedBBNs = new HashSet<_>()
        referencedBBNs.Add(entrypoint) |> ignore
        for KeyValue(bbn,block) in origBlockDict do
            match block with
            | BasicBlock(cmds,(DirectTailCall(nextBBN) as incomingFac),incomingWP) ->
                let mutable curWP = incomingWP
                let mutable curFac = incomingFac 
                let finalCmds = ResizeArray(cmds)
                let mutable finished,nextBBN = false,nextBBN
                while not finished do
                    let nextBB = origBlockDict.[nextBBN]
                    match nextBB with
                    | BasicBlock(nextCmds,(DirectTailCall(nextNextBBN) as nextFac),nextWP) when canMerge(curWP,nextWP) ->
                        curFac <- nextFac
                        curWP <- merge(curWP,nextWP).Value
                        finalCmds.AddRange(nextCmds)
                        nextBBN <- nextNextBBN 
                    | BasicBlock(_nextCmds,_fac,_) ->
                        finalBlockDict.Add(bbn,BasicBlock(finalCmds.ToArray(),curFac,curWP))
                        finished <- true
                        referencedBBNs.UnionWith[nextBBN]
            | BasicBlock(_,fac,_) ->
                finalBlockDict.Add(bbn,block)
                match fac with
                | ConditionalTailCall(_conds,ifbbn,elsebbn) ->
                    referencedBBNs.UnionWith[elsebbn]
                    referencedBBNs.UnionWith[ifbbn]
                | DirectTailCall(bbn) ->
                    referencedBBNs.UnionWith[bbn]
                | _ -> ()
        let allBBNs = new HashSet<_>(origBlockDict.Keys)
        allBBNs.ExceptWith(referencedBBNs)
        for unusedBBN in allBBNs do
            printfn "removing unused state '%s' after optimization" unusedBBN.Name 
            finalBlockDict.Remove(unusedBBN) |> ignore
        Program(scope,dependencyModules,init1,init2,entrypoint,finalBlockDict)

////////////////////////

let FUNCTION_NAMESPACE = "lorgon111" // functions folder name
let makeFunction(name,instructions) = (name,instructions|>Seq.toArray)

////////////////////////
// compile to one-tick

let compileToOneTick(program, isTracing) =
    let ONE_TICK_DIRECTORY = "one_tick"
    let otName(s) = sprintf "%s/%s" ONE_TICK_DIRECTORY s

    let onetickCompilerVars = new Scope()
    let otcIP = onetickCompilerVars.RegisterVar("otcIP")
    let StopPump = onetickCompilerVars.RegisterVar("StopPump")  // done processing this tick

    // TODO any name-collision detection?

    let initialization = ResizeArray()
    let initialization2 = ResizeArray()  // runs a tick after initialization
    let initialization3 = ResizeArray()  // runs 2 ticks after initialization
    let functions = ResizeArray()

    let transformBBN(bbn:BasicBlockName) = BBN(sprintf "%s/%s" "raycast" bbn.Name)  // TODO - let program choose name

    for v in onetickCompilerVars.All() do
        initialization.Add(AtomicCommand(sprintf "scoreboard objectives add %s dummy" v.Name))
    let (Program(_scope,dependencyModules,programInit1,programInit2,entrypoint,blockDict)) = program
    let mutable foundEntryPointInDictionary = false
    let mutable nextBBNNumber = 1
    let bbnNumbers = new Dictionary<_,_>()
    for KeyValue(bbn,_) in blockDict do
        bbnNumbers.Add(bbn, nextBBNNumber)
        nextBBNNumber <- nextBBNNumber + 1
        if bbn = entrypoint then
            initialization2.Add(SB(otcIP .= bbnNumbers.[bbn]))
            foundEntryPointInDictionary <- true
    if not(foundEntryPointInDictionary) then
        failwith "did not find entrypoint in basic block dictionary"

    let visited = new HashSet<_>()
    // runner infrastructure functions at bottom of code further below
    let q = new Queue<_>()
    q.Enqueue(entrypoint)
    while q.Count <> 0 do
        let instructions = ResizeArray()
        let currentBBN = q.Dequeue()
        if not(visited.Contains(currentBBN)) then
            visited.Add(currentBBN) |> ignore
            let (BasicBlock(cmds,finish,waitPolicy)) = blockDict.[currentBBN]
            if isTracing then
                instructions.Add(AtomicCommand(sprintf """tellraw @a ["start block: %s"]""" currentBBN.Name))
            for c in cmds do
                match c with 
                | AtomicCommand _s ->
                    instructions.Add(c)
                | AtomicCommandWithExtraCost(_s,_cost) ->
                    instructions.Add(c)
                | AtomicCommandThunkFragment _f ->
                    instructions.Add(c)
                | SB(_soc) ->
                    instructions.Add(c)
                | CALL(_f) ->
                    instructions.Add(c)
            match waitPolicy with
            | MustWaitNTicks 1 -> 
                instructions.Add(SB(StopPump .= 1)) // ok, this is same as 'halt'
            | MustWaitNTicks _ ->
                failwithf "bad MustWaitNTicks for %s, can only be 1 in one-tick compiler" currentBBN.Name
            | MustNotYield -> () // ok, this is typical for one-tick
            | NoPreference ->
                failwithf "bad NoPreference for %s, should only be MustNotYield in one-tick compiler" currentBBN.Name
            match finish with
            | DirectTailCall(nextBBN) ->
                if not(blockDict.ContainsKey(nextBBN)) then
                    failwithf "bad DirectTailCall goto %s" nextBBN.Name
                q.Enqueue(nextBBN) |> ignore
                instructions.Add(SB(otcIP .= bbnNumbers.[nextBBN]))
            | ConditionalTailCall(conds,ifbbn,elsebbn) ->
                if not(blockDict.ContainsKey(elsebbn)) then
                    failwithf "bad ConditionalDirectTailCalls elsebbn %s" elsebbn.Name
                q.Enqueue(elsebbn) |> ignore
                // first set catchall
                instructions.Add(SB(otcIP .= bbnNumbers.[elsebbn]))
                // then do test, and if match overwrite
                if not(blockDict.ContainsKey(ifbbn)) then
                    failwithf "bad ConditionalDirectTailCalls %s" ifbbn.Name
                q.Enqueue(ifbbn) |> ignore
                instructions.Add(SB(otcIP.[conds] .= bbnNumbers.[ifbbn]))
            | Halt ->
                instructions.Add(SB(StopPump .= 1))
            functions.Add(makeFunction(transformBBN(currentBBN).Name, instructions |> Seq.map (fun c -> c.AsCommand())))
    let allBBNs = new HashSet<_>(blockDict.Keys)
    allBBNs.ExceptWith(visited)
    if allBBNs.Count <> 0 then
        failwithf "there were unreferenced basic block names, including for example %s" (allBBNs |> Seq.head).Name
    // TODO consider giving programs names, and putting code for each program in name\bbn rather than just lumping all code in functions namespace folder
    // dispatchers for this program
    functions.Add(makeFunction(otName"otc_dispatch",[|
            // run one (or more, if subsequent block has higher BBN# than orig) block of this process
            for KeyValue(bbn,num) in bbnNumbers do 
                yield sprintf """execute @s[score_%s_min=%d,score_%s=%d] ~ ~ ~ function %s:%s""" 
                                    otcIP.Name num otcIP.Name num FUNCTION_NAMESPACE (transformBBN(bbn).Name)           // find next BB to run
        |]))

    // function runner infrastructure
    functions.Add(makeFunction(otName"start",[|
        // make compiler live
        yield sprintf "scoreboard players set %s %s 0" ENTITY_UUID StopPump.Name

        // PUMP A TICK
        yield sprintf "execute %s ~ ~ ~ function %s:%s/pump1" ENTITY_UUID FUNCTION_NAMESPACE ONE_TICK_DIRECTORY

        // catch an error condition
        yield sprintf """execute %s ~ ~ ~ execute @s[score_%s=0] ~ ~ ~ tellraw @a ["the otc pump ran out but the process never yielded; fix process bugs or recompile with a larger pump."]""" ENTITY_UUID StopPump.Name 
        |]))
    // pump loop (finite cps without deep recursion)
    let MAX_PUMP_DEPTH = 4
    let MAX_PUMP_WIDTH = 10   // (width ^ depth) is max iters
    for i = 1 to MAX_PUMP_DEPTH do
        functions.Add(makeFunction(otName(sprintf"pump%d"i),[|
                for _x = 1 to MAX_PUMP_WIDTH do 
                    yield sprintf """execute @s[score_%s=0] ~ ~ ~ function %s:%s/pump%d""" StopPump.Name FUNCTION_NAMESPACE ONE_TICK_DIRECTORY (i+1)
            |]))
    functions.Add(makeFunction(otName(sprintf"pump%d"(MAX_PUMP_DEPTH+1)),[|sprintf "function %s:%s/otc_dispatch" FUNCTION_NAMESPACE ONE_TICK_DIRECTORY|]))
    // init
    for DropInModule(_,oneTimeInit1,oneTimeInit2,funcs) in dependencyModules do
        initialization2.AddRange(oneTimeInit1 |> Seq.map (fun cmd -> AtomicCommand(cmd)))
        // TODO is there ever a case where a program's first-tick init depends on a drop-in's second tick init? for now, assume no.
        initialization3.AddRange(oneTimeInit2 |> Seq.map (fun cmd -> AtomicCommand(cmd)))
        functions.AddRange(funcs)
    initialization2.AddRange(programInit1)
    initialization3.AddRange(programInit2)
    // TODO conditionally do this if there's another compiler live
    let least,most = Utilities.toLeastMost(new System.Guid(ENTITY_UUID_AS_FULL_GUID))
    functions.Add(makeFunction(otName"initialization",[|
            yield "kill @e[type=armor_stand,tag=compiler]" // TODO possibly use x,y,z to limit chunk
            for c in initialization do
                yield c.AsCommand()
            yield sprintf "gamerule gameLoopFunction %s:%s/inittick1" FUNCTION_NAMESPACE ONE_TICK_DIRECTORY
        |]))
    functions.Add(makeFunction(otName"inittick1",[|
            sprintf "gamerule gameLoopFunction %s:%s/inittick2" FUNCTION_NAMESPACE ONE_TICK_DIRECTORY
            // Note: cannot summon a UUID entity in same tick you killed entity with that UUID
            sprintf """summon armor_stand -3 4 -3 {CustomName:%s,NoGravity:1,UUIDMost:%dl,UUIDLeast:%dl,Invulnerable:1,Tags:["compiler"]}""" ENTITY_UUID most least
            // TODO what location is this entity? it needs to be safe in spawn chunks, but who knows where that is, hm, drop thru end portal?
        |]))
    functions.Add(makeFunction(otName"inittick2",[|
            yield sprintf "gamerule gameLoopFunction %s:%s/inittick3" FUNCTION_NAMESPACE ONE_TICK_DIRECTORY
            yield sprintf "execute %s ~ ~ ~ function %s:%s/initialization2" ENTITY_UUID FUNCTION_NAMESPACE ONE_TICK_DIRECTORY
        |]))
    functions.Add(makeFunction(otName"initialization2",[|
            // This is code that runs assuming @s has been set up (compiler SB init, and user code init)
            for c in initialization2 do
                yield c.AsCommand()
        |]))
    functions.Add(makeFunction(otName"inittick3",[|
            // This is code that runs assuming @s has been set up (compiler SB init, and user code init)
            for c in initialization3 do
                yield c.AsCommand()
            yield sprintf "gamerule gameLoopFunction %s:%s/start" FUNCTION_NAMESPACE ONE_TICK_DIRECTORY
        |]))
    sprintf """function %s:%s/initialization""" FUNCTION_NAMESPACE ONE_TICK_DIRECTORY, functions

////////////////////////
// multi-program compiler/scheduler

let functionCompilerVars = new Scope()
let YieldNow = functionCompilerVars.RegisterVar("YieldNow")   // -1 means MustNotYield, 0 is running, 1 means done with processing this tick because every proc yielded, 2 means done because overran desired

let TEN_MILLION = functionCompilerVars.RegisterVar("TEN_MILLION")
let WBThisTick = functionCompilerVars.RegisterVar("WBThisTick")   // for pre-emption, measure milliseconds past since end of last tick
let CmdThisTick = functionCompilerVars.RegisterVar("CmdThisTick") // measure milliseconds that 'we ate doing computation' this tick
let WBAccum = functionCompilerVars.RegisterVar("WBAccum")         // for user-space time measurement, accumulate time across ticks
let Desired = functionCompilerVars.RegisterVar("Desired")         // number of desired commands to run (dynamically computed based on lag history)
let NumRun = functionCompilerVars.RegisterVar("NumRun")           // number of commands left to run this tick (starts at Desired and counts down)
let WINDOW_SIZE = 59      // how many ticks into the past we keep track of, to see if lagging, to dynamically adjust how much we run
let WS_VAR = functionCompilerVars.RegisterVar("WS_VAR")  // WINDOW_SIZE as a constant
let LagExactA = Array.init WINDOW_SIZE (fun i -> functionCompilerVars.RegisterVar(sprintf "LagExactA%d" i))
let WindowSelect = functionCompilerVars.RegisterVar("WindowSelect")     // a number in range 0..WINDOW_SIZE-1, increments each tick, chooses LagExactA to update
let Temp = functionCompilerVars.RegisterVar("Temp")
let LagThisTick = functionCompilerVars.RegisterVar("LagThisTick")
let AvgOverrun = functionCompilerVars.RegisterVar("AvgOverrun")
let ChooserYield = functionCompilerVars.RegisterVar("ChooserYield")
let CurrentRR = functionCompilerVars.RegisterVar("CurrentRR")        // the current program number (0..N-1) we are round-robining through
let NumLiveProc = functionCompilerVars.RegisterVar("NumLiveProc")    // how many procs have work left this tick
let NumProcInNeed = functionCompilerVars.RegisterVar("NumProcInNeed")// how many procs have work left this tick, but have not yet had a time slice

let DESIRED_INIT = 4000
let DISPLAY_MS_OVERSHOOT    = 51   // threshold we display in the chat if we overrun
#if LAG_ATTRIBUTION
// TODO attribution doesn't do very well, because it samples only one tick, really need a window history to do it well, probably, expensive
let DISPLAY_LAG_ATTRIBUTION = 980  // how many more ms than 50ms per tick we display lag attribution info
let DISPLAY_LAG = 10000000 + 50 + DISPLAY_LAG_ATTRIBUTION // world border size to print at
#endif

let DEBUG_THROTTLE = true
let compileToFunctions(programs, isTracing) =
    let ProcIP = Array.init (programs |> Seq.length) (fun i -> functionCompilerVars.RegisterVar(sprintf "IP%d" i))
    let ProcYieldNow = Array.init (programs |> Seq.length) (fun i -> functionCompilerVars.RegisterVar(sprintf "ProcYieldNow%d" i))  // -1 MustNotYield, 0 Running (or NoPreference), 1-or-more is MustWaitNTicks
    let ProcGotSlice = Array.init (programs |> Seq.length) (fun i -> functionCompilerVars.RegisterVar(sprintf "ProcGotSlice%d" i))  // 0 if not yet got a slice this tick, 1 otherwise
#if LAG_ATTRIBUTION
    let ProcRun = Array.init (programs |> Seq.length) (fun i -> functionCompilerVars.RegisterVar(sprintf "ProcRun%d" i))  // approx num statements run by this process this tick (used for lag-attribution)
#endif
    let allDependencyModules = ResizeArray()
    let allProgramInit1 = ResizeArray()
    let allProgramInit2 = ResizeArray()

    // name-collision detection
    // TODO detect drop-in-module function names, and compiler-specific function names, in addition to BB names
    let allVariableNames = new System.Collections.Generic.HashSet<_>()
    let allBBNames = new System.Collections.Generic.HashSet<_>()
    for Program(scope,dependencyModules,programInit1,programInit2,_entrypoint,blockDict) in programs do
        allDependencyModules.AddRange(dependencyModules)
        allProgramInit1.AddRange(programInit1)
        allProgramInit2.AddRange(programInit2)
        for v in scope.All() do
            if not(allVariableNames.Add(v.Name)) then
                failwithf "two programs both use a variable named '%s'" v.Name
        for KeyValue(bbn,_) in blockDict do
            if not(allBBNames.Add(bbn.Name)) then
                failwithf "two programs both use a basic block named '%s'" bbn.Name

    let initialization = ResizeArray()
    let initialization2 = ResizeArray()  // runs a tick after initialization
    let initialization3 = ResizeArray()  // runs 2 ticks after initialization
    let functions = ResizeArray()
    let functionThunks = ResizeArray()
(* TODO consider
    initialization.Add(AtomicCommand("gamerule maxCommandChainLength 999999")) // affects num total commands run in any function call
    initialization.Add(AtomicCommand("gamerule commandBlockOutput false"))
    initialization.Add(AtomicCommand("gamerule sendCommandFeedback false"))
    initialization.Add(AtomicCommand("gamerule logAdminCommands false"))
*)
    for v in functionCompilerVars.All() do
        initialization.Add(AtomicCommand(sprintf "scoreboard objectives add %s dummy" v.Name))
    initialization2.Add(SB(YieldNow .= 0))
    initialization2.Add(SB(TEN_MILLION .= 10000000))
    initialization2.Add(SB(WS_VAR .= WINDOW_SIZE))
    let mutable avgNumCmdsOverheadInnerLoop = 0
    initialization2.Add(SB(CurrentRR .= 0))
    for i = 0 to (programs |> Seq.length)-1 do
        initialization2.Add(SB(ProcYieldNow.[i] .= 0))    // TODO some programs may want to start in a halted (99999999) state, and have command/advancement wake them later
    do // scope programNumber to this block
        let mutable programNumber = -1
        for Program(_scope,_dependencyModules,_programInit1,_programInit2,entrypoint,blockDict) in programs do
            programNumber <- programNumber + 1
            let mutable foundEntryPointInDictionary = false
            let mutable nextBBNNumber = 1
            let bbnNumbers = new Dictionary<_,_>()
            for KeyValue(bbn,_) in blockDict do
                // TODO this check is nonsense, yes? the name is not in the scoreboard?
                if bbn.Name.Length > 15 then
                    failwithf "scoreboard names can only be up to 15 characters: %s" bbn.Name
                bbnNumbers.Add(bbn, nextBBNNumber)
                nextBBNNumber <- nextBBNNumber + 1
                if bbn = entrypoint then
                    initialization2.Add(SB(ProcIP.[programNumber] .= bbnNumbers.[bbn]))
                    foundEntryPointInDictionary <- true
            if not(foundEntryPointInDictionary) then
                failwith "did not find entrypoint in basic block dictionary"

            let visited = new HashSet<_>()
            // runner infrastructure functions at bottom of code further below
            let q = new Queue<_>()
            q.Enqueue(entrypoint)
            while q.Count <> 0 do
                let instructions = ResizeArray()
                let currentBBN = q.Dequeue()
                if not(visited.Contains(currentBBN)) then
                    visited.Add(currentBBN) |> ignore
                    let (BasicBlock(cmds,finish,waitPolicy)) = blockDict.[currentBBN]
                    if isTracing then
                        instructions.Add(AtomicCommand(sprintf """tellraw @a ["start block: %s"]""" currentBBN.Name))
                    let mutable numCmdsThisBB = 0
#if LAG_ATTRIBUTION
                    // keep track of approx statements run per-process, so can attribute lag to the offender
                    let pn = programNumber // don't thunk this mutable var, need current value!
                    instructions.Add(AtomicCommandThunkFragment(fun() -> sprintf "scoreboard players add @s %s %d" ProcRun.[pn].Name (numCmdsThisBB+avgNumCmdsOverheadInnerLoop)))
#endif
                    // keep countdown of approx statements being run now, to know when to pre-empt
                    instructions.Add(AtomicCommandThunkFragment(fun() -> sprintf "scoreboard players remove @s %s %d" NumRun.Name (numCmdsThisBB+avgNumCmdsOverheadInnerLoop)))
                    for c in cmds do
                        match c with 
                        | AtomicCommand _s ->
                            instructions.Add(c)
                        | AtomicCommandWithExtraCost(_s,cost) ->
                            numCmdsThisBB <- numCmdsThisBB + cost
                            instructions.Add(c)
                        | AtomicCommandThunkFragment _f ->
                            instructions.Add(c)
                        | SB(_soc) ->
                            instructions.Add(c)
                        | CALL(_f) ->
                            instructions.Add(c)
                    instructions.Add(AtomicCommand(sprintf "scoreboard players remove @s[score_%s=0] %s 1" ProcGotSlice.[programNumber].Name NumProcInNeed.Name))
                    instructions.Add(SB(ProcGotSlice.[programNumber] .= 1))
                    match finish,waitPolicy with
                    | Halt,_ -> ()  // if Halt, then don't do any of this, waitPolicy is meaningless
                    | _,MustWaitNTicks n ->
                        if not(n>0) then
                            failwithf "bad MustWaitNTicks for %s" currentBBN.Name
                        instructions.Add(SB(ProcYieldNow.[programNumber] .= n))
                        instructions.Add(SB(NumLiveProc .-= 1))
                    | _,MustNotYield ->
                        instructions.Add(SB(ProcYieldNow.[programNumber] .= -1))
                        instructions.Add(SB(YieldNow .= -1))
                    | _,NoPreference ->
                        instructions.Add(SB(ProcYieldNow.[programNumber] .= 0))
                    match finish with
                    | DirectTailCall(nextBBN) ->
                        if not(blockDict.ContainsKey(nextBBN)) then
                            failwithf "bad DirectTailCall goto %s" nextBBN.Name
                        q.Enqueue(nextBBN) |> ignore
                        instructions.Add(SB(ProcIP.[programNumber] .= bbnNumbers.[nextBBN]))
                    | ConditionalTailCall(conds,ifbbn,elsebbn) ->
                        if not(blockDict.ContainsKey(elsebbn)) then
                            failwithf "bad ConditionalDirectTailCalls elsebbn %s" elsebbn.Name
                        q.Enqueue(elsebbn) |> ignore
                        // first set catchall
                        instructions.Add(SB(ProcIP.[programNumber] .= bbnNumbers.[elsebbn]))
                        // then do test, and if match overwrite
                        if not(blockDict.ContainsKey(ifbbn)) then
                            failwithf "bad ConditionalDirectTailCalls %s" ifbbn.Name
                        q.Enqueue(ifbbn) |> ignore
                        instructions.Add(SB(ProcIP.[programNumber].[conds] .= bbnNumbers.[ifbbn]))
                    | Halt ->
                        instructions.Add(SB(ProcYieldNow.[programNumber] .= 999999999))   // to get out of the pump, and sleep 'for good' (over 500 days)
                        instructions.Add(SB(NumLiveProc .-= 1))
                    numCmdsThisBB <- numCmdsThisBB + (instructions |> Seq.length)
                    functionThunks.Add(fun() -> makeFunction(currentBBN.Name, instructions |> Seq.map (fun c -> c.AsCommand())))
            let allBBNs = new HashSet<_>(blockDict.Keys)
            allBBNs.ExceptWith(visited)
            if allBBNs.Count <> 0 then
                failwithf "there were unreferenced basic block names, including for example %s" (allBBNs |> Seq.head).Name
            // TODO consider giving programs names, and putting code for each program in name\bbn rather than just lumping all code in functions namespace folder
            // dispatchers for this program
            functions.Add(makeFunction(sprintf"dispatch_mny%d"programNumber,[|
                    // run one (or more, if subsequent block has higher BBN# than orig) block of this process
                    for KeyValue(bbn,num) in bbnNumbers do 
                        yield sprintf """execute @s[score_%s=-1,score_%s_min=%d,score_%s=%d] ~ ~ ~ function %s:%s""" 
                                            ProcYieldNow.[programNumber].Name ProcIP.[programNumber].Name num ProcIP.[programNumber].Name num FUNCTION_NAMESPACE bbn.Name           // find next BB to run
                    // if we are no longer MNY, notify the pump of that change by setting YieldNow to 0
                    yield sprintf "scoreboard players set @s[score_%s_min=0] %s 0" ProcYieldNow.[programNumber].Name YieldNow.Name 
                |]))
            let dispatchNormalBody = [|
                    // decide if time to pre-empt (all procs had at least one slice; overall done enough work already) - note this affects pump, not the proc
                    yield sprintf """scoreboard players set @s[score_%s=0,score_%s=0] %s 2""" NumProcInNeed.Name NumRun.Name YieldNow.Name  
                    // if not yielding, run _exactly_ one block of this process
                    yield sprintf "scoreboard players operation @s %s = @s %s" Temp.Name ProcIP.[programNumber].Name // store next IP in Temp
                    // TODO binary search is less overhead, esp. when many BBs, but must adjust avgNumCmdsOverheadInnerLoop appropriately (also apply to MNY above)
                    // TODO could do even better if can predict frequencies and put common BBs at the top of the tree and rare at bottom (Huffman coding)
                    for KeyValue(bbn,num) in bbnNumbers do 
                        // run only the block whose BBN# corresponds to Temp; this will update ProcIP, but not allow a subsequent BBN# to match the new value, since Temp does not change
                        // this ensures exactly one is run (well, actually _at most_ one, since may run zero if we got pre-empted)
                        yield sprintf """execute @s[score_%s=0,score_%s_min=%d,score_%s=%d] ~ ~ ~ function %s:%s""" 
                                            YieldNow.Name Temp.Name num Temp.Name num FUNCTION_NAMESPACE bbn.Name  // find next BB to run
                |]
            avgNumCmdsOverheadInnerLoop <- avgNumCmdsOverheadInnerLoop + dispatchNormalBody.Length // currently keep running sum...
            functions.Add(makeFunction(sprintf"dispatch_normal%d"programNumber,dispatchNormalBody))
        // end foreach program

    // function runner infrastructure
    functions.Add(makeFunction("start",[|
        // measure time when we got control...
        yield sprintf "execute %s ~ ~ ~ worldborder get" WBTIMER_UUID 
        // copy measurement onto entity as CmdThisTick
        yield sprintf "scoreboard players operation %s %s = %s %s" ENTITY_UUID CmdThisTick.Name WBTIMER_UUID WBThisTick.Name 

        // make compiler live
        yield sprintf "scoreboard players set %s %s 0" ENTITY_UUID YieldNow.Name
        // make long-sleeping procs get one tick closer to waking up
        for i = 0 to (programs |> Seq.length)-1 do
            yield sprintf "execute %s ~ ~ ~ scoreboard players remove @s[score_%s_min=1] %s 1" ENTITY_UUID ProcYieldNow.[i].Name ProcYieldNow.[i].Name

        // init NumRun to desired
        yield sprintf "scoreboard players operation %s %s = %s %s" ENTITY_UUID NumRun.Name ENTITY_UUID Desired.Name 
        // init NumLiveProc & ProcGotSlice
        yield sprintf "scoreboard players set %s %s 0" ENTITY_UUID NumLiveProc.Name
        for i = 0 to (programs |> Seq.length)-1 do
            yield sprintf "execute %s ~ ~ ~ scoreboard players add @s[score_%s=0] %s 1" ENTITY_UUID ProcYieldNow.[i].Name NumLiveProc.Name
            yield sprintf "scoreboard players set %s %s 0" ENTITY_UUID ProcGotSlice.[i].Name
        // init NumProcInNeed 
        yield sprintf "scoreboard players operation %s %s = %s %s" ENTITY_UUID NumProcInNeed.Name ENTITY_UUID NumLiveProc.Name 
#if LAG_ATTRIBUTION
        // init ProcRun
        for i = 0 to (programs |> Seq.length)-1 do
            yield sprintf "scoreboard players set %s %s 0" ENTITY_UUID ProcRun.[i].Name
#endif

        // if no work, immediately yield
        yield sprintf "execute %s ~ ~ ~ scoreboard players set @s[score_%s=0] %s 1" ENTITY_UUID NumLiveProc.Name YieldNow.Name

        // debugging
        //yield sprintf """execute %s ~ ~ ~ tellraw @a ["num live procs: ",{"score":{"name":"@e[name=%s]","objective":"%s"}}]""" ENTITY_UUID ENTITY_UUID NumLiveProc.Name 

        // PUMP A TICK
        yield sprintf "execute %s ~ ~ ~ function %s:pump1" ENTITY_UUID FUNCTION_NAMESPACE

        // catch an error condition
        yield sprintf """execute %s ~ ~ ~ execute @s[score_%s=0] ~ ~ ~ tellraw @a ["the pump ran out but the processes never yielded; fix process bugs or recompile with a larger pump. stopping gameLoop"]""" ENTITY_UUID YieldNow.Name 
        yield sprintf """execute %s ~ ~ ~ execute @s[score_%s=0] ~ ~ ~ gamerule gameLoopFunction -""" ENTITY_UUID YieldNow.Name 

        // measure 
        yield sprintf "execute %s ~ ~ ~ worldborder get" WBTIMER_UUID 
        // immediately restart the timer, so we measure a full cycle with fewest instructions between measurement and restart
        yield "worldborder set 10000000"
        yield "worldborder add 1000000 1000"
        // copy measurement onto entity as WBThisTick
        yield sprintf "scoreboard players operation %s %s = %s %s" ENTITY_UUID WBThisTick.Name WBTIMER_UUID WBThisTick.Name 
        // compute how much time we spent running commands since the gameLoopFunction began, store in CmdThisTick
        yield sprintf "scoreboard players operation %s %s = %s %s" ENTITY_UUID Temp.Name ENTITY_UUID WBThisTick.Name 
        yield sprintf "scoreboard players operation %s %s -= %s %s" ENTITY_UUID Temp.Name ENTITY_UUID CmdThisTick.Name 
        yield sprintf "scoreboard players operation %s %s = %s %s" ENTITY_UUID CmdThisTick.Name ENTITY_UUID Temp.Name 

        // convert WBThisTick measurement to milliseconds (ms)
        yield sprintf "scoreboard players operation %s %s -= %s %s" ENTITY_UUID WBThisTick.Name ENTITY_UUID TEN_MILLION.Name 

        // update lag/window info (regardless of whether we've been doing work, after all MC could be lagging on its own, and we need to know)
        yield sprintf "execute %s ~ ~ ~ function %s:update_lag" ENTITY_UUID FUNCTION_NAMESPACE
        // if work got pre-empted this tick, update Desired
        yield sprintf "execute %s ~ ~ ~ execute @s[score_%s_min=2] ~ ~ ~ function %s:update_desired" ENTITY_UUID YieldNow.Name FUNCTION_NAMESPACE

        // accumulate time
        yield sprintf "scoreboard players operation %s %s += %s %s" ENTITY_UUID WBAccum.Name ENTITY_UUID WBThisTick.Name  
        // debugging
        if DEBUG_THROTTLE then
//            yield sprintf """execute %s ~ ~ ~ execute @s[score_%s_min=%d] ~ ~ ~ tellraw @a ["overrun: ",{"score":{"name":"@e[name=%s]","objective":"WBThisTick"}},"  avg:",{"score":{"name":"@e[name=%s]","objective":"AvgOverrun"}}]""" 
//                                   ENTITY_UUID        WBThisTick.Name DISPLAY_MS_OVERSHOOT                                   ENTITY_UUID                                                    ENTITY_UUID 
            yield sprintf """execute %s ~ ~ ~ execute @s[score_%s=0] ~ ~ ~ tellraw @a ["avg ms per tick:",{"score":{"name":"@e[name=%s]","objective":"AvgOverrun"}},"  CmdThisTick:",{"score":{"name":"@e[name=%s]","objective":"CmdThisTick"}}]""" 
                                    ENTITY_UUID      WindowSelect.Name                                                       ENTITY_UUID                                                      ENTITY_UUID    

#if LAG_ATTRIBUTION
        // lag attribution
        yield sprintf """execute %s ~ ~ ~ execute @s[score_%s_min=%d] ~ ~ ~ function %s:lag_attribution""" ENTITY_UUID WBThisTick.Name DISPLAY_LAG FUNCTION_NAMESPACE
#endif

        |]))
#if LAG_ATTRIBUTION
    functions.Add(makeFunction("lag_attribution",[|
        let sb = new System.Text.StringBuilder()
        for i = 0 to (programs |> Seq.length)-1 do
            sb.Append(sprintf """," Proc#%d:",{"score":{"name":"@e[name=%s]","objective":"%s"}}""" i ENTITY_UUID ProcRun.[i].Name) |> ignore
        yield sprintf """tellraw @a ["Major lag detected, here is a current process summary... "%s]""" (sb.ToString())
        |]))
#endif
    functions.Add(makeFunction("update_lag",[|
        // RECALL: 
        // we can't measure how much minecraft is Sleep()ing due to underutilized CPU
        // we can only try to overutilize until we see performance starting to tank, and then back off
        // as a result, we need to try to overrun by a small bit; need to tune to find good balance

        // Ironically, if we get a time <= 48ms, this mean Minecraft is 'playing catch up', which means we may need to throttle back.
        // This means we can't just use an 'average' of the wall-clock measurements.  We need to adjust e.g. a recorded measurement of '47' to
        // mean what it really means, namely that somewhere in the past there was a '53' that Minecraft is trying to catch back up with.
        // Thus, LagExact will be populated only with values >= 49 (I'm assuming some minor measurement error).
        // Any measurements <= 48 could be recorded as LagExacts of 100-X.  We'll use LagThisTick to store this modified value.

        // Furthermore, when MC falls behind more than 2s, it starts discarding ticks, and we can't see/measure that.
        // As a result, we need an extra penalty for when we see measurements well below 48, to help ensure we don't invade the 'seriously lagging' vicinity.
        // We'll say if we see a measurement lower than, say, 35, we create an extra penalty of say 25.
        // Also if we see a measurement lower than 48, we create a small nominal penalty of say 2; we don't want noise to interfere, but we also want to ensure we're backing off if needed.

        // TODO the problem with this strategy is that if 60-40-60-40 is how MC wants to do things, there may still be plenty of mc-sleep, but I can't see it because I just see 60-60-60-60...
        // I need to measure TPS, and increase while it's >=20, and decrease when it goes below.
        // And I also need to be careful of overcorrecting, as with a long window, each tick sees mostly the same average as the previous tick.

        // set LagThisTick to measured value
        yield sprintf "scoreboard players operation @s %s = @s %s" LagThisTick.Name WBThisTick.Name
(*
        // if LagThisTick <= 48, set Temp to 102-LagThisTick, then assign back to LagThisTick
        // but if LagThisTick <= 35, set Temp to 125-LagThisTick, then assign back to LagThisTick
        yield sprintf "scoreboard players set @s[score_%s=48] %s 102" LagThisTick.Name Temp.Name
        yield sprintf "scoreboard players set @s[score_%s=35] %s 125" LagThisTick.Name Temp.Name
        yield sprintf "scoreboard players operation @s[score_%s=48] %s -= @s %s" LagThisTick.Name Temp.Name LagThisTick.Name 
        yield sprintf "scoreboard players operation @s[score_%s=48] %s = @s %s" LagThisTick.Name LagThisTick.Name Temp.Name 
*)

        // update lagexact
        for i = 0 to WINDOW_SIZE-1 do
            yield sprintf "scoreboard players operation @s[score_%s_min=%d,score_%s=%d] %s = @s %s" 
                                    WindowSelect.Name i WindowSelect.Name i LagExactA.[i].Name  LagThisTick.Name     

        // compute AvgOverrun
        yield sprintf "scoreboard players set @s %s 0" AvgOverrun.Name 
        for i = 0 to WINDOW_SIZE-1 do
            yield sprintf "scoreboard players operation @s %s += @s %s" AvgOverrun.Name LagExactA.[i].Name
        yield sprintf "scoreboard players operation @s %s /= @s %s" AvgOverrun.Name WS_VAR.Name 

        // increment windowselect (mod WINDOW_SIZE)
        yield sprintf "scoreboard players add %s %s 1" ENTITY_UUID WindowSelect.Name
        yield sprintf "scoreboard players set @s[score_%s_min=%d] %s 0" WindowSelect.Name WINDOW_SIZE WindowSelect.Name 
        |]))
    functions.Add(makeFunction("update_desired",[|
        // compute delta-desired
(*
        // AvgOverrun can never be <= 48 (see above for why) 
        // increase the throttle
        for lag,inc in [49,15
                        50,19
                        51,15
                        52,12
                        53,10
                        54,8
                        55,7
                        56,6
                        57,5
                        58,4
                        59,2
                        60,1] do
            yield sprintf "scoreboard players add @s[score_%s_min=%d,score_%s=%d] %s %d" AvgOverrun.Name lag AvgOverrun.Name lag Desired.Name inc
        // decrease the throttle
        for lag,inc in [61,1
                        62,2
                        63,3
                        64,5
                        65,8
                        66,13] do
            yield sprintf "scoreboard players remove @s[score_%s_min=%d,score_%s=%d] %s %d" AvgOverrun.Name lag AvgOverrun.Name lag Desired.Name inc
        // final decrease
        yield sprintf "scoreboard players remove @s[score_%s_min=67] %s 21" AvgOverrun.Name Desired.Name
*)
        
        yield sprintf "scoreboard players add @s[score_%s=49] %s 6" AvgOverrun.Name Desired.Name
        yield sprintf "scoreboard players remove @s[score_%s_min=51] %s 1" AvgOverrun.Name Desired.Name
        yield sprintf "scoreboard players remove @s[score_%s_min=52] %s 2" AvgOverrun.Name Desired.Name
        yield sprintf "scoreboard players remove @s[score_%s_min=53] %s 3" AvgOverrun.Name Desired.Name
        yield sprintf "scoreboard players remove @s[score_%s_min=58] %s 5" AvgOverrun.Name Desired.Name
        yield sprintf "scoreboard players remove @s[score_%s_min=65] %s 15" AvgOverrun.Name Desired.Name
        // if WE ate more that 48ms this tick, throttle back a lot - this helps get out of state where MC is constantly losing ticks
        // TODO 48 is arbitrary, how decide best number for e.g. multiplayer server? need some typical cpu utilization baseline, hm
        // TODO average this over window, so less noise-prone
        yield sprintf "scoreboard players remove @s[score_%s_min=48] %s 60" CmdThisTick.Name Desired.Name
        // Note: an alternate strategy for above is that if we see multiple ticks in a row come in under 50, MC must be doing the 'play faster ticks to catch up' thing, though that also needs a 'cap' on our side to ensure we're not doing more than 50ms (all of it)
        // Note: another strategy would be external monitoring of CPU utilization (e.g. task manager) that somehow auto-send commands to the server console

        // ensure desired never below 1
        yield sprintf "scoreboard players set @s[score_%s=0] %s 1" Desired.Name Desired.Name
        |]))
    // pump loop (finite cps without deep recursion)
    let MAX_PUMP_DEPTH = 4
    let MAX_PUMP_WIDTH = 10   // (width ^ depth) is max iters
    for i = 1 to MAX_PUMP_DEPTH do
        functions.Add(makeFunction(sprintf"pump%d"i,[|
                for _x = 1 to MAX_PUMP_WIDTH do 
                    yield sprintf """execute @s[score_%s=0] ~ ~ ~ function %s:pump%d""" YieldNow.Name FUNCTION_NAMESPACE (i+1)
            |]))
    // chooser
    let chooserBody = [|
            yield sprintf "scoreboard players operation @s %s = @s %s" ChooserYield.Name YieldNow.Name 
            // TODO an optimization is to notice if some programs have 0 MNY blocks, or 0 non-MNY blocks, and eliminate the unneeded calls below
            for i = 0 to (programs |> Seq.length)-1 do
                // dispatch either MNY or normal for currentRR, bot not both! (hence ChooserYield temp variable, ensures exclusion)
                yield sprintf "execute @s[score_%s=-1,score_%s=%d,score_%s_min=%d] ~ ~ ~ function %s:dispatch_mny%d" ChooserYield.Name CurrentRR.Name i CurrentRR.Name i FUNCTION_NAMESPACE i
                yield sprintf "execute @s[score_%s=0,score_%s_min=0,score_%s=0,score_%s=%d,score_%s_min=%d] ~ ~ ~ function %s:dispatch_normal%d" ChooserYield.Name ChooserYield.Name ProcYieldNow.[i].Name CurrentRR.Name i CurrentRR.Name i FUNCTION_NAMESPACE i
            // if NumLiveProc goes to 0, then everyone yielded, so have the pump yield
            yield sprintf "scoreboard players set @s[score_%s=0] %s 1" NumLiveProc.Name YieldNow.Name 
            // NOTE: if NumRun reaches the limit and we pre-empt, that is the other condition for causing the pump to yield, but it happens in the dispatch_normal code

            // TODO can we optimize this inner loop? inspect the .mcfunction code for ideas

            // TODO when NumLiveProc becomes 1, maybe I can avoid dispatch loop and process-swapping?

            // if not MustNotYield, then increment RoundRobin
            yield sprintf "scoreboard players add @s[score_%s_min=0] %s 1" YieldNow.Name CurrentRR.Name 
            yield sprintf "scoreboard players set @s[score_%s_min=%d] %s 0" CurrentRR.Name (programs |> Seq.length) CurrentRR.Name 
        |]
    avgNumCmdsOverheadInnerLoop <- avgNumCmdsOverheadInnerLoop / (programs |> Seq.length)   // ... had sum of normal dispatchers, figure average
    avgNumCmdsOverheadInnerLoop <- avgNumCmdsOverheadInnerLoop + chooserBody.Length         // ... and add constant overhead from chooser; that's approx how many commands of total overhead
    functions.Add(makeFunction(sprintf"pump%d"(MAX_PUMP_DEPTH+1),chooserBody))
    // init
    for DropInModule(_,oneTimeInit1,oneTimeInit2,funcs) in allDependencyModules do
        initialization2.AddRange(oneTimeInit1 |> Seq.map (fun cmd -> AtomicCommand(cmd)))
        // TODO is there ever a case where a program's first-tick init depends on a drop-in's second tick init? for now, assume no.
        initialization3.AddRange(oneTimeInit2 |> Seq.map (fun cmd -> AtomicCommand(cmd)))
        functions.AddRange(funcs)
    initialization2.AddRange(allProgramInit1)
    initialization3.AddRange(allProgramInit2)
    let least,most = Utilities.toLeastMost(new System.Guid(ENTITY_UUID_AS_FULL_GUID))
    let wbleast,wbmost = Utilities.toLeastMost(new System.Guid(WBTIMER_UUID_AS_FULL_GUID))
    functions.Add(makeFunction("initialization",[|
            yield "kill @e[type=armor_stand,tag=compiler]" // TODO possibly use x,y,z to limit chunk
            for c in initialization do
                yield c.AsCommand()
            yield sprintf "gamerule gameLoopFunction %s:inittick1" FUNCTION_NAMESPACE
        |]))
    functions.Add(makeFunction("inittick1",[|
            sprintf "gamerule gameLoopFunction %s:inittick2" FUNCTION_NAMESPACE
            // Note: cannot summon a UUID entity in same tick you killed entity with that UUID
            sprintf """summon armor_stand -3 4 -3 {CustomName:%s,NoGravity:1,UUIDMost:%dl,UUIDLeast:%dl,Invulnerable:1,Tags:["compiler"]}""" ENTITY_UUID most least
            // TODO what location is this entity? it needs to be safe in spawn chunks, but who knows where that is, hm, drop thru end portal?
            sprintf """summon armor_stand -4 4 -4 {CustomName:%s,NoGravity:1,UUIDMost:%dl,UUIDLeast:%dl,Invulnerable:1,Tags:["compiler"]}""" WBTIMER_UUID wbmost wbleast
            """tellraw @a ["tick1 called"]"""
        |]))
    functions.Add(makeFunction("inittick2",[|
            yield sprintf "gamerule gameLoopFunction %s:inittick3" FUNCTION_NAMESPACE
            yield """tellraw @a ["tick2 called"]"""
            yield sprintf "execute %s ~ ~ ~ function %s:initialization2" ENTITY_UUID FUNCTION_NAMESPACE
            
            for i = 0 to WINDOW_SIZE-1 do
                yield sprintf "scoreboard players set %s %s 50" ENTITY_UUID LagExactA.[i].Name
            yield sprintf "scoreboard players set %s %s 0" ENTITY_UUID WindowSelect.Name
            yield sprintf "scoreboard players set %s %s %d" ENTITY_UUID Desired.Name DESIRED_INIT

            yield sprintf "stats entity %s set QueryResult %s %s" WBTIMER_UUID WBTIMER_UUID WBThisTick.Name 
            yield sprintf "scoreboard players set %s %s 10000000" WBTIMER_UUID WBThisTick.Name  // need initial value before can trigger a stat
            yield "worldborder set 10000000"
            yield "worldborder add 1000000 1000"
        |]))
    functions.Add(makeFunction("initialization2",[|
            // This is code that runs assuming @s has been set up (compiler SB init, and user code init)
            yield """tellraw @a ["2 called"]"""
            for c in initialization2 do
                yield c.AsCommand()
        |]))
    functions.Add(makeFunction("inittick3",[|
            // This is code that runs assuming @s has been set up (compiler SB init, and user code init)
            yield """tellraw @a ["3 called"]"""
            for c in initialization3 do
                yield c.AsCommand()
            yield sprintf "gamerule gameLoopFunction %s:start" FUNCTION_NAMESPACE
        |]))

    for thunk in functionThunks do
        functions.Add(thunk())

    sprintf """function %s:initialization""" FUNCTION_NAMESPACE, functions

////////////////////////

// Mandelbrot

let cpsPrefix = BBN"cpsprefix"
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

let mbGeneral = NoPreference
//let mbGeneral = MustNotYield
let MANDEL_UUID = "1-1-1-0-9"
let MANDEL_UUID_AS_FULL_GUID = "00000001-0001-0001-0000-000000000009"
let mbleast,mbmost = Utilities.toLeastMost(new System.Guid(MANDEL_UUID_AS_FULL_GUID))
let mandelbrotProgram(yLevel) = 
    Program(mandelbrotVars,[||],[|
            yield AtomicCommand "kill @e[type=armor_stand,tag=mandel]"
            for v in mandelbrotVars.All() do
                yield AtomicCommand(sprintf "scoreboard objectives add %s dummy" v.Name)
            // constants
            yield SB(FOURISSQ .= 64000000)
            yield SB(INTSCALE .= 4000)
            yield SB(MAXH .= 128)
            yield SB(MAXW .= 128)
            yield SB(XSCALE .= 96)
            yield SB(YSCALE .= 62)
            yield SB(XMIN .= -8400)
            yield SB(YMIN .= -4000)
(*
            yield SB(FOURISSQ .= 64000000)
            yield SB(INTSCALE .= 4000)
            yield SB(MAXH .= 128)
            yield SB(MAXW .= 128)
            yield SB(XSCALE .= 12)
            yield SB(YSCALE .= 12)
            yield SB(XMIN .= -1120)
            yield SB(YMIN .= 3200)
*)
        |],[||],cpsPrefix, dict [
        cpsPrefix,BasicBlock([|
            // color stuff
#if DIRECT16COLORTEST
#else
            yield AtomicCommand "scoreboard objectives add AS dummy"  // armor stands
            for i = 0 to 15 do
                let y,z = 4,-2
                yield AtomicCommand(sprintf "setblock %d %d %d wool %d" i y z i)
                yield AtomicCommand(sprintf "summon armor_stand %d %d %d" i y z)
//                TODO ensure kill AS at end
                yield AtomicCommand(sprintf "scoreboard players set @e[type=armor_stand,x=%d,y=%d,z=%d,c=1] AS %d" i y z i)
                yield AtomicCommand(sprintf "scoreboard players tag @e[type=armor_stand,x=%d,y=%d,z=%d,c=1] add color" i y z)
#endif
            // note: cannot summon uuid'd entity in same tick it was killed
            yield AtomicCommand(sprintf """summon armor_stand 0 4 0 {CustomName:Cursor,NoGravity:1,UUIDMost:%dl,UUIDLeast:%dl,Invulnerable:1,Tags:["mandel"]}""" mbmost mbleast)
            |],DirectTailCall(cpsIStart),mbGeneral)
        cpsIStart,BasicBlock([|
            // time measurement
//            yield AtomicCommand "worldborder set 10000000"
//            yield AtomicCommand "worldborder add 1000000 1000000"
            yield AtomicCommand(sprintf "scoreboard players set %s %s 0" ENTITY_UUID WBAccum.Name) // TODO abstract this 
            // actual code
            yield SB(i .= 0)
            yield AtomicCommand(sprintf "tp %s 0 %d 0" MANDEL_UUID yLevel)
            yield AtomicCommand(sprintf "fill 0 %d 0 127 %d 127 air" yLevel yLevel)
            yield AtomicCommand(sprintf "fill 0 %d 0 127 %d 127 wool 0" (yLevel-1) (yLevel-1))
            |],DirectTailCall(cpsJStart),mbGeneral)
        cpsJStart,BasicBlock([|
            SB(j .= 0)
            AtomicCommand(sprintf "tp %s ~ ~ 0" MANDEL_UUID)
            |],DirectTailCall(cpsInnerStart),mbGeneral)
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
            |],DirectTailCall(whileTest),mbGeneral)
        whileTest,BasicBlock([|
            SB(xsq .= x)
            SB(xsq .*= x)
            SB(ysq .= y)
            SB(ysq .*= y)
            SB(r1 .= xsq)
            SB(r1 .+= ysq)
            SB(r1 .-= FOURISSQ)
            |],ConditionalTailCall(Conditional[| r1 .<= -1; n .<= 15 |], cpsInnerInner, cpsInnerFinish),mbGeneral)
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
            |],DirectTailCall(whileTest),mbGeneral)
        cpsInnerFinish,BasicBlock([|
#if DIRECT16COLORTEST
            yield AtomicCommandThunkFragment(fun () -> sprintf "scoreboard players operation %s %s = %s" MANDEL_UUID n.Name (n.AsCommandFragment()))
            for zzz = 0 to 15 do
                yield AtomicCommand(sprintf "execute %s ~ ~ ~ execute @s[score_%s=%d,score_%s_min=%d] ~ ~ ~ setblock ~ ~ ~ wool %d" MANDEL_UUID n.Name (zzz+1) n.Name (zzz+1) zzz)
#else
            yield AtomicCommandThunkFragment(fun () -> sprintf "scoreboard players operation @e[tag=color] AS -= %s" (n.AsCommandFragment()))
            yield AtomicCommand "execute @e[tag=color,score_AS=-1,score_AS_min=-1] ~ ~ ~ clone ~ ~ ~ ~ ~ ~ 0 4 0"
            yield AtomicCommandThunkFragment(fun() -> sprintf "scoreboard players operation @e[tag=color] AS += %s" (n.AsCommandFragment()))
            yield AtomicCommand "execute @e[name=Cursor] ~ ~ ~ clone 0 4 0 0 4 0 ~ ~ ~"
#endif
            yield SB(j .+= 1)
            yield AtomicCommand(sprintf "execute %s ~ ~ ~ tp @e[c=1] ~ ~ ~1" MANDEL_UUID)
            yield SB(r1 .= j)
            yield SB(r1 .-= MAXH)
            |],ConditionalTailCall(Conditional[| r1 .<= -1 |], cpsInnerStart, cpsJFinish),mbGeneral)  // inner loop yield
        cpsJFinish,BasicBlock([|
            SB(i .+= 1)
            AtomicCommand(sprintf "execute %s ~ ~ ~ tp @e[c=1] ~1 ~ ~" MANDEL_UUID)
            SB(r1 .= i)
            SB(r1 .-= MAXW)
            |],ConditionalTailCall(Conditional[| r1 .<= -1 |], cpsJStart, cpsIFinish), MustWaitNTicks 1)
        cpsIFinish,BasicBlock([|
            AtomicCommand """tellraw @a ["done!"]"""
            // time measurement (todo abstract reading accum)
            AtomicCommand(sprintf """tellraw @a ["took ",{"score":{"name":"@e[name=%s]","objective":"%s"}}," milliseconds"]""" ENTITY_UUID WBAccum.Name)
            |],Halt,mbGeneral)
        ])


    