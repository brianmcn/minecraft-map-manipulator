module AdvancementCompiler

open System.Collections.Generic 


// TODO fastest score lookup is by uuid, so make one entity with a short uuid like below to use for ALL scores
// summon area_effect_cloud ~ ~ ~ {UUIDMost:12884967424l,UUIDLeast:844424930131969l}
// teleport 3-1-0-3-1 ~ ~ ~ ~ ~
// Note: without stack frames, need a naming convention for temp & permanent vars to avoid conflict; also function args and return values
//       temps are easy, can assume will get wiped each time you grant an advancement... but permanent ones? (and deal with recursion if needed!)
// OR: could summon a uuid'd entity to use as a stack frame or permanent namespace, but not too many, as each creates more work
// note that the AS need names as well, so their scores can be subject of conditions, e.g. @e[name=3-1-0-3-1,score_x=45]
// in any case, useful to factor in my programming model a way to abstract over 'variable storage entity' and 'variable name'
// let x = TEMP.MakeVar("x")
// x.Gets(CONSTANTS(42))   // entity for storing all int constants needed?   
// x.PlusEquals(y)
// ARGS.[0].Assign(x)
// Call("advFunc")
// x.Assign(RETURN.[0])
// IF(Selector(x.GREATER(45),y.EQUAL(0)),x.PlusEquals(CONSTANTS(1))))
// NOTE! conditional function call (advancement grant @s[score...] ...) can only run on the _player_ score, though, hmmm... PLAYER needs some globals as well as some registers
// TODO Rewrite Mandelbrot and MouseCursor (tan func, if-then-else near bottom) with this programming model to get a feel for it and find issues
// Well, if we assume singleplayer, seems one 'locality' strategy is all scores on player (@p/@s), 
//   - may not be quite as fast as UUID entity, but no 'register transfer' for conditional calls
//   - maybe analysis can discover some variables can 'live' on entity and not need transfer
// TODO how are 'modules' factored? what does 'tan' look like? how is json folder structure?
// I guess best is first to have a good programming model to do this manually, and then can do register allocator and peephole optimizer in future later?
// note that 'scoreboard add' on UUID takes 400(300-800ms), whereas execute @e[name=blah,score_x_min=1] ~ ~ ~ scoreboard add takes 1300(1200-1700ms)
// so for just a few commands, a redundant execute prefix is ok overhead, but for more than a few, it may make sense to break THEN and ELSE into advancement subroutines


////////////////////////

type Var(name:string) =
    // variables are 'globals', they are represented as objectives, and scores either reside on the player (@s) if used in conditionals, or on a UUID'd entity
    let mutable livesOnPlayer = true // TODO should assume false, but for now simply just putting all scores on the player
    do
        if name.Length > 14 then
            failwithf "Var name too long: %s" name
    member this.Name = name
    member this.UseInConditional() =
        livesOnPlayer <- true
    member this.AsCommandFragment() =
        if livesOnPlayer then
            sprintf "@s %s" name
        else
            failwith "not yet implemented"
    // conditionals
    static member (.<=) (v,n) = SCMax(v,n)
    static member (.>=) (v,n) = SCMin(v,n)
    static member (.==) (v,n) = [| SCMin(v,n); SCMax(v,n) |]
    // operations
    static member (.=)  (a,b:int) = ScoreboardPlayersSet(a,b)
    static member (.=)  (a,b:Var) = ScoreboardOperationCommand(a,ASSIGN,b)
    static member (.+=) (a,b:int) = ScoreboardPlayersAdd(a,b)
    static member (.+=) (a,b:Var) = ScoreboardOperationCommand(a,PLUS_EQUALS,b)
    static member (.-=) (a,b) = ScoreboardOperationCommand(a,MINUS_EQUALS,b)
    static member (.*=) (a,b) = ScoreboardOperationCommand(a,TIMES_EQUALS,b)
    static member (./=) (a,b) = ScoreboardOperationCommand(a,DIVIDE_EQUALS,b)

and ScoreCondition =
    | SCMin of Var * int
    | SCMax of Var * int

and Conditional(conds:ScoreCondition[]) =
    do
        if conds.Length < 1 then
            failwith "bad conditional"
    member this.Visit() =
        for c in conds do
            match c with
            | SCMin(v,_) -> v.UseInConditional()
            | SCMax(v,_) -> v.UseInConditional()
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
    | ASSIGN | PLUS_EQUALS | MINUS_EQUALS | TIMES_EQUALS | DIVIDE_EQUALS
    member this.AsCommandFragment() =
        match this with
        | ASSIGN         -> "="
        | PLUS_EQUALS    -> "+="
        | MINUS_EQUALS   -> "-="
        | TIMES_EQUALS   -> "*="
        | DIVIDE_EQUALS  -> "/="

and ScoreboardOperationCommand =
    | ScoreboardOperationCommand of Var * ScoreboardOperation * Var
    | ScoreboardPlayersSet of Var * int
    | ScoreboardPlayersAdd of Var * int
    member this.AsCommand() =
        match this with
        | ScoreboardPlayersAdd(v,x) -> sprintf "scoreboard players add %s %d" (v.AsCommandFragment()) x
        | ScoreboardPlayersSet(v,x) -> sprintf "scoreboard players set %s %d" (v.AsCommandFragment()) x
        | ScoreboardOperationCommand(a,op,b) -> sprintf "scoreboard players operation %s %s %s" (a.AsCommandFragment()) (op.AsCommandFragment()) (b.AsCommandFragment())

//type AdvancementGrant = 
//    | AdvancementGrant of string
//    | ConditionalAdvancementGrant of Conditional*string

type Scope() =
    let vars = ResizeArray()
    member this.RegisterVar(s) =
        let r = Var(s)
        vars.Add(r)
        r
    member this.All() = vars |> Seq.toArray 

////////////////////////

type BasicBlockName = 
    | BBN of string
    member this.Name = match this with BBN(s) -> s
type FinalAbstractCommand =
    | DirectTailCall of BasicBlockName
    | ConditionalTailCall of Conditional*BasicBlockName*BasicBlockName // if-then-else
    | Halt
type AbstractCommand =
    | AtomicCommand of string // e.g. "say blah", "scoreboard players ..."
    | SB of ScoreboardOperationCommand
//    | AG of AdvancementGrant 
    | Yield // express desire to yield CPU back to minecraft to run a tick after this block (cooperative multitasking)
    member this.AsCommand() =
        match this with
        | AtomicCommand s -> s
        | SB soc -> soc.AsCommand()
        | Yield -> failwith "should not get here Yield"
type BasicBlock = BasicBlock of AbstractCommand[] * FinalAbstractCommand
type Program = Program of (*one-time init*)AbstractCommand[] * (*entrypoint*)BasicBlockName * IDictionary<BasicBlockName,BasicBlock>

////////////////////////

let inlineAllDirectTailCallsOptimization(p) =
    match p with
    | Program(init,entrypoint,origBlockDict) ->
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
        Program(init,entrypoint,finalBlockDict)

////////////////////////

let PREFIX = "functions" // advancements folder name
let makeAdvancement(name,instructions) = // TODO ensure root, clean this up?
    sprintf "%s/%s" PREFIX name, Advancements.Advancement(Some(Recipes.PATH(sprintf"%s:root"PREFIX)),Advancements.NoDisplay,Advancements.Reward([||],[||],0,[|
        yield! instructions
        |]),[|Advancements.Criterion("cx",Recipes.MC"impossible",[||])|],[|[|"cx"|]|])

let advancementizeVars = new Scope()
let IP = advancementizeVars.RegisterVar("IP")
let Stop = advancementizeVars.RegisterVar("Stop")

let advancementize(Program(programInit,entrypoint,blockDict), isTracing, 
            x, y, z) =   // x,y,z is where the repumpAfterTick (PulseICB) is located
    let initialization = ResizeArray()
    let mutable foundEntryPointInDictionary = false
    for v in advancementizeVars.All() do
        initialization.Add(AtomicCommand(sprintf "scoreboard objectives add %s dummy" v.Name))
    initialization.Add(SB(Stop .= 0))
    let mutable nextBBNNumber = 1
    let bbnNumbers = new Dictionary<_,_>()
    for KeyValue(bbn,_) in blockDict do
        if bbn.Name.Length > 15 then
            failwithf "scoreboard names can only be up to 15 characters: %s" bbn.Name
        bbnNumbers.Add(bbn, nextBBNNumber)
        nextBBNNumber <- nextBBNNumber + 1
        if bbn = entrypoint then
            initialization.Add(SB(IP .= bbnNumbers.[bbn]))
            foundEntryPointInDictionary <- true
    if not(foundEntryPointInDictionary) then
        failwith "did not find entrypoint in basic block dictionary"

    let visited = new HashSet<_>()
    let advancements = ResizeArray()
    // runner infrastructure advancements at bottom of code further below
    let q = new Queue<_>()
    q.Enqueue(entrypoint)
    while q.Count <> 0 do
        let instructions = ResizeArray()
        let currentBBN = q.Dequeue()
        if not(visited.Contains(currentBBN)) then
            visited.Add(currentBBN) |> ignore
            let (BasicBlock(cmds,finish)) = blockDict.[currentBBN]
            // TODO better way
            instructions.Add(AtomicCommand(sprintf "advancement revoke @s only %s:%s" PREFIX currentBBN.Name))
            if isTracing then
                instructions.Add(AtomicCommand(sprintf """tellraw @a ["start block: %s"]""" currentBBN.Name))
            for c in cmds do
                match c with 
                | AtomicCommand _s ->
                    instructions.Add(c)
                | SB(_soc) ->
                    instructions.Add(c)
                | Yield ->
                    instructions.Add(SB(Stop .= 1))
                    // TODO replace with 'tick' advancement
                    instructions.Add(AtomicCommand(sprintf "blockdata %d %d %d {auto:1b}" x y z))
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
                // TODO abstract this
                instructions.Add(AtomicCommand(sprintf "scoreboard players set @s%s %s %d" (conds.AsCommandFragment()) IP.Name bbnNumbers.[ifbbn]))
            | Halt ->
                instructions.Add(SB(Stop .= 1))
                // TODO line below is a hack to fix duplicate 'done'
                instructions.Add(SB(IP .= -1))
            advancements.Add(makeAdvancement(currentBBN.Name, instructions |> Seq.map (fun c -> c.AsCommand())))
    let allBBNs = new HashSet<_>(blockDict.Keys)
    allBBNs.ExceptWith(visited)
    if allBBNs.Count <> 0 then
        failwithf "there were unreferenced basic block names, including for example %s" (allBBNs |> Seq.head).Name

    // advancement runner infrastructure
    // root
    let root = "functions/root",Advancements.Advancement(None,Advancements.NoDisplay,Advancements.Reward([||],[||],0,[|
                    |]),[|Advancements.Criterion("cx",Recipes.MC"impossible",[||])|],[|[|"cx"|]|])
    advancements.Add(root)
    // pump loop (finite cps without deep recursion)
    let MAX_PUMP_DEPTH = 4
    let MAX_PUMP_WIDTH = 10   // (width ^ depth) is max iters
    for i = 1 to MAX_PUMP_DEPTH do
        advancements.Add(makeAdvancement(sprintf"pump%d"i,[|
                // TODO
                yield sprintf """advancement revoke @s only %s:pump%d""" PREFIX i
                // TODO
                for _x = 1 to MAX_PUMP_WIDTH do yield sprintf """advancement grant @s[score_%s=0] only %s:pump%d""" Stop.Name PREFIX (i+1)
            |]))
    // chooser
    advancements.Add(makeAdvancement(sprintf"pump%d"(MAX_PUMP_DEPTH+1),[|
            // TODO
            yield sprintf """advancement revoke @s only %s:pump%d""" PREFIX (MAX_PUMP_DEPTH+1)
            for KeyValue(bbn,num) in bbnNumbers do 
                // TODO
                yield sprintf """advancement grant @s[score_%s_min=%d,score_%s=%d] only %s:%s""" 
                                    IP.Name num IP.Name num PREFIX bbn.Name 
        |]))
    // init
    initialization.AddRange(programInit)
    advancements.Add(makeAdvancement("initialization",[|
            yield sprintf """advancement revoke @s only %s:initialization""" PREFIX
            for c in initialization do
                yield c.AsCommand()
        |]))

    // TODO set this up with 'tick'
    let repumpAfterTick = [|
        RegionFiles.CommandBlock.O "blockdata ~ ~ ~ {auto:0b}"
        RegionFiles.CommandBlock.U (sprintf "scoreboard players set @p %s %d" Stop.Name 0)
        RegionFiles.CommandBlock.U (sprintf "advancement grant @p only %s:pump1" PREFIX)
        |]

    sprintf """advancement grant @p only %s:initialization""" PREFIX, repumpAfterTick, advancements

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

let program = 
    Program([|
            for v in mandelbrotVars.All() do
                yield AtomicCommand(sprintf "scoreboard objectives add %s dummy" v.Name)
            // color stuff
            yield AtomicCommand "scoreboard objectives add AS dummy"  // armor stands
            yield AtomicCommand "kill @e[type=armor_stand]"  // armor stands
            for i = 0 to 15 do
                let y,z = 4,-2
                yield AtomicCommand(sprintf "setblock %d %d %d wool %d" i y z i)
                yield AtomicCommand(sprintf "summon armor_stand %d %d %d" i y z)
                yield AtomicCommand(sprintf "scoreboard players set @e[type=armor_stand,x=%d,y=%d,z=%d,c=1] AS %d" i y z i)
            yield AtomicCommand "scoreboard players tag @e[type=armor_stand] add color"
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
            yield AtomicCommand "scoreboard players operation @e[name=Cursor] n = @s n"
            for zzz = 0 to 15 do
                yield AtomicCommand(sprintf "execute @e[name=Cursor,score_n=%d,score_n_min=%d] ~ ~ ~ setblock ~ ~ ~ wool %d" (zzz+1) (zzz+1) zzz)
#else
            yield AtomicCommand "scoreboard players operation @e[tag=color] AS -= @s n"
            yield AtomicCommand "execute @e[tag=color,score_AS=-1,score_AS_min=-1] ~ ~ ~ clone ~ ~ ~ ~ ~ ~ 0 4 0"
            yield AtomicCommand "scoreboard players operation @e[tag=color] AS += @s n"
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


    