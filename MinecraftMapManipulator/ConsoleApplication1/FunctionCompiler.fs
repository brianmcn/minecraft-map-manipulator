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
    | AtomicCommandThunkFragment of (unit -> string) // e.g. fun () -> sprintf "something something %s" (v.AsCommandFragment())  // needs to be eval'd after whole program is visited
    | SB of ScoreboardOperationCommand
    | CALL of FunctionCommand 
    member this.AsCommand() =
        match this with
        | AtomicCommand s -> s
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
    | Program of Scope * DropInModule[] * (*one-time init*)AbstractCommand[] * (*entrypoint*)BasicBlockName * IDictionary<BasicBlockName,BasicBlock>

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
    | Program(scope,dependencyModules,init,entrypoint,origBlockDict) ->
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
        Program(scope,dependencyModules,init,entrypoint,finalBlockDict)

////////////////////////

let FUNCTION_NAMESPACE = "lorgon111" // functions folder name
let makeFunction(name,instructions) = (name,instructions|>Seq.toArray)

let functionCompilerVars = new Scope()
let YieldNow = functionCompilerVars.RegisterVar("YieldNow")   // -1 means MustNotYield, 0 is running, 1 means done with processing this tick

let TEN_MILLION = functionCompilerVars.RegisterVar("TEN_MILLION")
let WBThisTick = functionCompilerVars.RegisterVar("WBThisTick")   // for pre-emption, measure milliseconds past since end of last tick
let WBAccum = functionCompilerVars.RegisterVar("WBAccum")         // for user-space time measurement, accumulate time across ticks
// TODO rename to reflect USE_BB_COUNT, e.g. may be BBs or commands
let DesiredBBs = functionCompilerVars.RegisterVar("DesiredBBs")   // number of desired basic blocks to run (dynamically computed based on lag history)
let NumBBsRun = functionCompilerVars.RegisterVar("NumBBsRun")     // number of basic blocks left to run this tick (starts at DesiredBBs and counts down)
let WINDOW_SIZE = 10      // (should be divisible by 5) how many ticks into the past we keep track of, to see if lagging, to dynamically adjust how much we run
let DIVISOR = functionCompilerVars.RegisterVar("DIVISOR")
let LagVarArray = Array.init WINDOW_SIZE (fun i -> functionCompilerVars.RegisterVar(sprintf "LagVar%d" i))
let WindowSelect = functionCompilerVars.RegisterVar("WindowSelect")     // a number in range 0..WINDOW_SIZE-1, increments each tick, chooses LagVar to update
let Temp = functionCompilerVars.RegisterVar("Temp")
let ChooserYield = functionCompilerVars.RegisterVar("ChooserYield")
let ThrottleArray = Array.init 6 (fun i -> functionCompilerVars.RegisterVar(sprintf "Throttle%d" i))
let ThereWasWork = functionCompilerVars.RegisterVar("ThereWasWork")     // was there even any work scheduled this tick? (if not, don't go increasing DesiredBB just b/c we see extra CPU)
let CurrentRR = functionCompilerVars.RegisterVar("CurrentRR")     // the current program number (0..N-1) we are round-robining through
let NumLiveProc = functionCompilerVars.RegisterVar("NumLiveProc")   // how many procs have work left this tick

let TARGETED_MS_OVERSHOOT = 4   // how many more ms than 50ms per tick we aim for (recall, can only measure lag, not how much MC sleeps, so must overshoot)
let DISPLAY_MS_OVERSHOOT  = 8   // how many more ms than 50ms per tick we display in the chat if we overrun
let TARGETED_WB = 10000000 + 50 + TARGETED_MS_OVERSHOOT   // world border size to aim at
let DISPLAY_WB  = 10000000 + 50 + DISPLAY_MS_OVERSHOOT    // world border size to print at

let DESIRED_INIT = 3000   // number of commands to initially target

let compileToFunctions(programs, isTracing) =
    let ProcIP = Array.init (programs |> Seq.length) (fun i -> functionCompilerVars.RegisterVar(sprintf "IP%d" i))
    let ProcYieldNow = Array.init (programs |> Seq.length) (fun i -> functionCompilerVars.RegisterVar(sprintf "ProcYieldNow%d" i))  // -1 MustNotYield, 0 Running (or NoPreference), 1-or-more is MustWaitNTicks
    let allDependencyModules = ResizeArray()
    let allProgramInit = ResizeArray()

    // name-collision detection
    // TODO detect drop-in-module function names, and compiler-specific function names, in addition to BB names
    let allVariableNames = new System.Collections.Generic.HashSet<_>()
    let allBBNames = new System.Collections.Generic.HashSet<_>()
    for Program(scope,dependencyModules,programInit,_entrypoint,blockDict) in programs do
        allDependencyModules.AddRange(dependencyModules)
        allProgramInit.AddRange(programInit)
        for v in scope.All() do
            if not(allVariableNames.Add(v.Name)) then
                failwithf "two programs both use a variable named '%s'" v.Name
        for KeyValue(bbn,_) in blockDict do
            if not(allBBNames.Add(bbn.Name)) then
                failwithf "two programs both use a basic block named '%s'" bbn.Name

    let initialization = ResizeArray()
    let initialization2 = ResizeArray()  // runs a tick after initialization
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
    initialization2.Add(SB(DIVISOR .= (WINDOW_SIZE/5)))
#if USE_BB_COUNT
        // no lag     20%     40%     60%     80%    100% lag
        //    +1       +0      -1      -2      -3      -4     // works very poorly; tiny usual bits of noise slow system to a crawl
        //    +2       +1       0      -1      -2      -3     // works ok-ish, doesn't back off well
        //    +3       +0      -1      -3      -6     -12     // seems plausible with limited testing, might still need to increase backoff at 40% and higher?
    initialization2.Add(SB(ThrottleArray.[0] .= 3))
    initialization2.Add(SB(ThrottleArray.[1] .= 0))
    initialization2.Add(SB(ThrottleArray.[2] .= -1))
    initialization2.Add(SB(ThrottleArray.[3] .= -3))
    initialization2.Add(SB(ThrottleArray.[4] .= -6))
    initialization2.Add(SB(ThrottleArray.[5] .= -12))
#else
    initialization2.Add(SB(ThrottleArray.[0] .= +71))
    initialization2.Add(SB(ThrottleArray.[1] .= -3))
    initialization2.Add(SB(ThrottleArray.[2] .= -19))
    initialization2.Add(SB(ThrottleArray.[3] .= -39))
    initialization2.Add(SB(ThrottleArray.[4] .= -131))
    initialization2.Add(SB(ThrottleArray.[5] .= -357))
#endif
    let mutable avgNumCmdsOverheadInnerLoop = 0
    initialization2.Add(SB(CurrentRR .= 0))
    for i = 0 to (programs |> Seq.length)-1 do
        initialization2.Add(SB(ProcYieldNow.[i] .= 0))    // TODO some programs may want to start in a halted (99999999) state, and have command/advancement wake them later
    do // scope programNumber to this block
        let mutable programNumber = -1
        for Program(_scope,_dependencyModules,_programInit,entrypoint,blockDict) in programs do
            programNumber <- programNumber + 1
            let mutable foundEntryPointInDictionary = false
            let mutable nextBBNNumber = 1
            let bbnNumbers = new Dictionary<_,_>()
            for KeyValue(bbn,_) in blockDict do
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
#if USE_BB_COUNT
                    //instructions.Add(SB(NumBBsRun .-= 1))
                    instructions.Add(AtomicCommandThunkFragment(fun() -> sprintf "scoreboard players remove @s %s 1" NumBBsRun.Name))
#else
                    let mutable numCmdsThisBB = 0
                    instructions.Add(AtomicCommandThunkFragment(fun() -> sprintf "scoreboard players remove @s %s %d" NumBBsRun.Name (numCmdsThisBB+avgNumCmdsOverheadInnerLoop)))
#endif
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
                    numCmdsThisBB <- instructions |> Seq.length 
                    functionThunks.Add(fun() -> makeFunction(currentBBN.Name, instructions |> Seq.map (fun c -> c.AsCommand())))
            let allBBNs = new HashSet<_>(blockDict.Keys)
            allBBNs.ExceptWith(visited)
            if allBBNs.Count <> 0 then
                failwithf "there were unreferenced basic block names, including for example %s" (allBBNs |> Seq.head).Name
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
                    yield sprintf """scoreboard players set @s[score_%s=0] %s 1""" NumBBsRun.Name YieldNow.Name  // decide if time to pre-empt (done enough work already) - note this affects pump, not the proc
                    // if not yielding, run _exactly_ one block of this process
                    yield sprintf "scoreboard players operation @s %s = @s %s" Temp.Name ProcIP.[programNumber].Name // store next IP in Temp
                    for KeyValue(bbn,num) in bbnNumbers do 
                        // run only the block whose BBN# corresponds to Temp; this will update ProcIP, but not allow a subsequent BBN# to match the new value, since Temp does not change
                        // this ensures exactly one is run (well, actually _at most_ one, since may run zero if we yielded)
                        yield sprintf """execute @s[score_%s=0,score_%s=0,score_%s_min=%d,score_%s=%d] ~ ~ ~ function %s:%s""" 
                                            YieldNow.Name ProcYieldNow.[programNumber].Name Temp.Name num Temp.Name num FUNCTION_NAMESPACE bbn.Name  // find next BB to run
                |]
            avgNumCmdsOverheadInnerLoop <- avgNumCmdsOverheadInnerLoop + dispatchNormalBody.Length // currently keep running sum...
            functions.Add(makeFunction(sprintf"dispatch_normal%d"programNumber,dispatchNormalBody))
        // end foreach program

    // function runner infrastructure
    functions.Add(makeFunction("start",[|
        // make compiler live
        yield sprintf "scoreboard players set %s %s 0" ENTITY_UUID YieldNow.Name
        // make long-sleeping procs get one tick closer to waking up
        for i = 0 to (programs |> Seq.length)-1 do
            yield sprintf "execute %s ~ ~ ~ scoreboard players remove @s[score_%s_min=1] %s 1" ENTITY_UUID ProcYieldNow.[i].Name ProcYieldNow.[i].Name

        // init numbb to desired
        yield sprintf "scoreboard players operation %s %s = %s %s" ENTITY_UUID NumBBsRun.Name ENTITY_UUID DesiredBBs.Name 
        // init NumLiveProc
        yield sprintf "scoreboard players set %s %s 0" ENTITY_UUID NumLiveProc.Name
        for i = 0 to (programs |> Seq.length)-1 do
            yield sprintf "execute %s ~ ~ ~ scoreboard players add @s[score_%s=0] %s 1" ENTITY_UUID ProcYieldNow.[i].Name NumLiveProc.Name
        // init ThereWasWork
        yield sprintf "scoreboard players set %s %s 0" ENTITY_UUID ThereWasWork.Name 
        yield sprintf "execute %s ~ ~ ~ scoreboard players set @s[score_%s_min=1] %s 1" ENTITY_UUID NumLiveProc.Name ThereWasWork.Name // any live proc means there is work

        // if no work, immediately yield
        yield sprintf "execute %s ~ ~ ~ scoreboard players set @s[score_%s=0] %s 1" ENTITY_UUID ThereWasWork.Name YieldNow.Name

        // debugging
        //yield sprintf """execute %s ~ ~ ~ tellraw @a ["num live procs: ",{"score":{"name":"@e[name=%s]","objective":"%s"}}]""" ENTITY_UUID ENTITY_UUID NumLiveProc.Name 

        // pump a tick
        yield sprintf "execute %s ~ ~ ~ function %s:pump1" ENTITY_UUID FUNCTION_NAMESPACE

        // catch an error condition
        yield sprintf """execute %s ~ ~ ~ execute @s[score_%s=0] ~ ~ ~ tellraw @a ["the pump ran out but the processes never yielded; fix process bugs or recompile with a larger pump. stopping gameLoop"]""" ENTITY_UUID YieldNow.Name 
        yield sprintf """execute %s ~ ~ ~ execute @s[score_%s=0] ~ ~ ~ gamerule gameLoopFunction -""" ENTITY_UUID YieldNow.Name 

        // RECALL: 
        // we can't measure how much minecraft is Sleep()ing due to underutilized CPU
        // we can only try to overutilize until we see performance starting to tank, and then back off
        // as a result, we need to try to overrun by a small bit; need to tune to find good balance

        // measure 
        yield sprintf "execute %s ~ ~ ~ worldborder get" WBTIMER_UUID 
        yield sprintf "scoreboard players set %s %s 0" WBTIMER_UUID Temp.Name 
        yield sprintf "execute %s ~ ~ ~ scoreboard players set @s[score_%s_min=%d] %s 1" WBTIMER_UUID WBThisTick.Name TARGETED_WB Temp.Name
        // WBTIMER Temp now records 1 or 0 if we are lagging or not
        yield sprintf "scoreboard players operation %s %s = %s %s" ENTITY_UUID Temp.Name WBTIMER_UUID Temp.Name 

        // update lagvar
        for i = 0 to WINDOW_SIZE-1 do
            yield sprintf "execute %s ~ ~ ~ scoreboard players operation @s[score_%s_min=%d,score_%s=%d] %s = @s %s" 
                                 ENTITY_UUID    WindowSelect.Name i WindowSelect.Name i LagVarArray.[i].Name    Temp.Name  

        // increment windowselect (mod WINDOW_SIZE)
        yield sprintf "scoreboard players add %s %s 1" ENTITY_UUID WindowSelect.Name
        yield sprintf "execute %s ~ ~ ~ scoreboard players set @s[score_%s_min=%d] %s 0" ENTITY_UUID WindowSelect.Name WINDOW_SIZE WindowSelect.Name 

        // if there was work scheduled this tick, update DesiredBB 
        // TODO this is incorrect; in mandel+ray, Desired increases without limit after mandel finishes, because ray does a check-no-snowball-then-yield
        // the correct thing is to update Desired only if not-all-processes-chose-to-yield-by-the-end, or alternatively phrased, if we needed to force a pre-empt
        yield sprintf "execute %s ~ ~ ~ execute @s[score_%s_min=1] ~ ~ ~ function %s:update_desiredbb" ENTITY_UUID ThereWasWork.Name FUNCTION_NAMESPACE

        // accumulate time
        yield sprintf "execute %s ~ ~ ~ worldborder get" WBTIMER_UUID 
        yield sprintf "scoreboard players operation %s %s += %s %s" WBTIMER_UUID WBAccum.Name WBTIMER_UUID WBThisTick.Name  
        yield sprintf "scoreboard players operation %s %s -= %s %s" WBTIMER_UUID WBAccum.Name ENTITY_UUID TEN_MILLION.Name
        // debugging
        yield sprintf """execute %s ~ ~ ~ execute @s[score_%s_min=%d] ~ ~ ~ tellraw @a ["overrun: ",{"score":{"name":"@e[name=%s]","objective":"WBThisTick"}}]""" 
                               WBTIMER_UUID        WBThisTick.Name DISPLAY_WB                                          WBTIMER_UUID 

        // restart the timer _before_ yielding to Minecraft, so our next measurement will be after MC takes its slice of the next 50ms
        yield "worldborder set 10000000"
        yield "worldborder add 1000000 1000"
        |]))
    functions.Add(makeFunction("update_desiredbb",[|  // TODO could optimize slightly now that @s is ENTITY
        // compute delta-desired
        // let Temp be the sum of the lagvars
        yield sprintf "scoreboard players set %s %s 0" ENTITY_UUID Temp.Name 
        for i = 0 to WINDOW_SIZE-1 do
            yield sprintf "scoreboard players operation %s %s += %s %s" ENTITY_UUID Temp.Name ENTITY_UUID LagVarArray.[i].Name
        // need find right heuristic to throttle... tried various ThrottleArray values initialized at top of compiler
        yield sprintf "scoreboard players operation %s %s /= %s %s" ENTITY_UUID Temp.Name ENTITY_UUID DIVISOR.Name
        for i = 0 to 5 do
            yield sprintf "execute %s ~ ~ ~ scoreboard players operation @s[score_%s_min=%d,score_%s=%d] %s += %s %s" ENTITY_UUID Temp.Name i Temp.Name i DesiredBBs.Name ENTITY_UUID ThrottleArray.[i].Name
        // never let desired go less than 1
        yield sprintf "execute %s ~ ~ ~ scoreboard players set @s[score_%s=0] %s 1" ENTITY_UUID DesiredBBs.Name DesiredBBs.Name 
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
            for i = 0 to (programs |> Seq.length)-1 do
                // dispatch either MNY or normal for currentRR, bot not both! (hence ChooserYield temp variable, ensures exclusion)
                yield sprintf "execute @s[score_%s=-1,score_%s=%d,score_%s_min=%d] ~ ~ ~ function %s:dispatch_mny%d" ChooserYield.Name CurrentRR.Name i CurrentRR.Name i FUNCTION_NAMESPACE i
                yield sprintf "execute @s[score_%s=0,score_%s_min=0,score_%s=%d,score_%s_min=%d] ~ ~ ~ function %s:dispatch_normal%d" ChooserYield.Name ChooserYield.Name CurrentRR.Name i CurrentRR.Name i FUNCTION_NAMESPACE i
            // if NumLiveProc goes to 0, then everyone yielded, so have the pump yield
            yield sprintf "scoreboard players set @s[score_%s=0] %s 1" NumLiveProc.Name YieldNow.Name 
            // NOTE: if NumBBsRun reaches the limit and we pre-empt, that is the other condition for causing the pump to yield, but it happens in the dispatch_normal code

            // TODO guarantee each proc gets at least one slice each tick?

            // TODO can we optimize this inner loop? inspect the .mcfunction code for ideas

            // if not MustNotYield, then increment RoundRobin
            yield sprintf "scoreboard players add @s[score_%s_min=0] %s 1" YieldNow.Name CurrentRR.Name 
            yield sprintf "scoreboard players set @s[score_%s_min=%d] %s 0" CurrentRR.Name (programs |> Seq.length) CurrentRR.Name 
        |]
    avgNumCmdsOverheadInnerLoop <- avgNumCmdsOverheadInnerLoop / (programs |> Seq.length)   // ... had sum of normal dispatchers, figure average
    avgNumCmdsOverheadInnerLoop <- avgNumCmdsOverheadInnerLoop + chooserBody.Length         // ... and add constant overhead from chooser; that's approx how many commands of total overhead
    functions.Add(makeFunction(sprintf"pump%d"(MAX_PUMP_DEPTH+1),chooserBody))
    // init
    for DropInModule(_,oneTimeInit,funcs) in allDependencyModules do
        initialization2.AddRange(oneTimeInit |> Seq.map (fun cmd -> AtomicCommand(cmd)))
        functions.AddRange(funcs)
    initialization2.AddRange(allProgramInit)
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
            yield """tellraw @a ["tick2 called"]"""
            // Note: seems you cannot refer to UUID in same tick you just summoned it
            yield sprintf "execute %s ~ ~ ~ function %s:initialization2" ENTITY_UUID FUNCTION_NAMESPACE
            
            for i = 0 to WINDOW_SIZE-1 do
                yield sprintf "scoreboard players set %s %s 0" ENTITY_UUID LagVarArray.[i].Name
            yield sprintf "scoreboard players set %s %s 0" ENTITY_UUID WindowSelect.Name
            yield sprintf "scoreboard players set %s %s %d" ENTITY_UUID DesiredBBs.Name DESIRED_INIT

            yield sprintf "stats entity %s set QueryResult %s %s" WBTIMER_UUID WBTIMER_UUID WBThisTick.Name 
            yield sprintf "scoreboard players set %s %s 10000000" WBTIMER_UUID WBThisTick.Name  // need initial value before can trigger a stat
            yield "worldborder set 10000000"
            yield "worldborder add 1000000 1000"
            
            yield sprintf "gamerule gameLoopFunction %s:start" FUNCTION_NAMESPACE
        |]))
    functions.Add(makeFunction("initialization2",[|
            // This is code that runs assuming @s has been set up (compiler SB init, and user code init)
            yield """tellraw @a ["2 called"]"""
            for c in initialization2 do
                yield c.AsCommand()
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
let mandelbrotProgram = 
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
        |],cpsPrefix, dict [
        cpsPrefix,BasicBlock([|
            // color stuff
#if DIRECT16COLORTEST
#else
            yield AtomicCommand "scoreboard objectives add AS dummy"  // armor stands
            for i = 0 to 15 do
                let y,z = 4,-2
                yield AtomicCommand(sprintf "setblock %d %d %d wool %d" i y z i)
                yield AtomicCommand(sprintf "summon armor_stand %d %d %d" i y z)
                TODO ensure kill AS at end
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
            yield AtomicCommand(sprintf "scoreboard players set %s %s 0" WBTIMER_UUID WBAccum.Name) // TODO abstract this 
            // actual code
            yield SB(i .= 0)
            yield AtomicCommand(sprintf "tp %s 0 14 0" MANDEL_UUID)
            yield AtomicCommand "fill 0 14 0 127 14 127 air"
            yield AtomicCommand "fill 0 13 0 127 13 127 wool 0"
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
            // time measurement
            // TODO abstract this
            AtomicCommand(sprintf "execute %s ~ ~ ~ worldborder get" WBTIMER_UUID)
            // TODO actually want Accum + ThisTick, but not modify Accum here; only compiler should modify Accum
            AtomicCommand(sprintf "scoreboard players operation %s %s += %s %s" WBTIMER_UUID WBAccum.Name WBTIMER_UUID WBThisTick.Name)
            AtomicCommand(sprintf "scoreboard players operation %s %s -= %s %s" WBTIMER_UUID WBAccum.Name ENTITY_UUID TEN_MILLION.Name)
            AtomicCommand(sprintf """tellraw @a ["took ",{"score":{"name":"@e[name=%s]","objective":"%s"}}," milliseconds"]""" WBTIMER_UUID WBAccum.Name)
            |],Halt,mbGeneral)
        ])


    