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

type AdvancementGrant = 
    | AdvancementGrant of string
    | ConditionalAdvancementGrant of Conditional*string

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
type BasicBlock = BasicBlock of AbstractCommand[] * FinalAbstractCommand
type Program = Program of (*entrypoint*)BasicBlockName * IDictionary<BasicBlockName,BasicBlock>

////////////////////////

let inlineAllDirectTailCallsOptimization(p) =
    match p with
    | Program(entrypoint,origBlockDict) ->
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
        Program(entrypoint,finalBlockDict)

////////////////////////

module ScoreboardNameConstants =
    let IP = "IP"  // objective name where basic block names say a value, and CurrentIP says which block is now
    //let CurrentIP = "CurrentIP"  // currentIP says which is active now
    //let PulseICB = "PulseICB"  // player name in objective IP saying whether to pulse starter ICB next tick (let a game tick run)
    //let Halt = "Halt"  // player name in objective IP saying whether to halt the machine
    let Stop = "Stop" // name of an objective where @p has 1 or 0 depending on if time to stop the loop runner

#if NEEDS_REWORK

let advancementize(Program(entrypoint,blockDict), isTracing, 
            x, y, z) =   // x,y,z is where the repumpAfterTick (PulseICB) is located
    // TODO consider abstracting 'who gets the advancement', currently also @p
    let ENTITY_IP = "@p"
    let initialization = ResizeArray()
    let mutable foundEntryPointInDictionary = false
    initialization.Add(sprintf "scoreboard objectives add %s dummy" ScoreboardNameConstants.IP)
    initialization.Add(sprintf "scoreboard objectives add %s dummy" ScoreboardNameConstants.Stop)
    initialization.Add(sprintf "scoreboard players set %s %s %d" ENTITY_IP ScoreboardNameConstants.Stop 0)
    let mutable nextBBNNumber = 1
    let bbnNumbers = new Dictionary<_,_>()
    for KeyValue(bbn,_) in blockDict do
        if bbn.Name.Length > 15 then
            failwithf "scoreboard names can only be up to 15 characters: %s" bbn.Name
        bbnNumbers.Add(bbn, nextBBNNumber)
        nextBBNNumber <- nextBBNNumber + 1
        if bbn = entrypoint then
            initialization.Add(sprintf "scoreboard players set %s %s %d" ENTITY_IP ScoreboardNameConstants.IP bbnNumbers.[bbn])
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
            instructions.Add(sprintf "advancement revoke @s only %s:%s" PREFIX currentBBN.Name)
            if isTracing then
                instructions.Add(sprintf """tellraw @a ["start block: %s"]""" currentBBN.Name)
            for c in cmds do
                match c with 
                | AtomicCommand s ->
                    instructions.Add(s)
                | Yield ->
                    instructions.Add(sprintf "scoreboard players set @s %s %d" ScoreboardNameConstants.Stop 1)
                    instructions.Add(sprintf "blockdata %d %d %d {auto:1b}" x y z)
            match finish with
            | DirectTailCall(nextBBN) ->
                if not(blockDict.ContainsKey(nextBBN)) then
                    failwithf "bad DirectTailCall goto %s" nextBBN.Name
                q.Enqueue(nextBBN) |> ignore
                instructions.Add(sprintf "scoreboard players set @s %s %d" ScoreboardNameConstants.IP bbnNumbers.[nextBBN])
            | ConditionalDirectTailCalls((conds,bbn),catchAllBBN) ->
                if not(blockDict.ContainsKey(catchAllBBN)) then
                    failwithf "bad ConditionalDirectTailCalls catchall %s" catchAllBBN.Name
                q.Enqueue(catchAllBBN) |> ignore
                // first set catchall
                instructions.Add(sprintf "scoreboard players set @s %s %d" ScoreboardNameConstants.IP bbnNumbers.[catchAllBBN])
                // then do test, and if match overwrite
                if not(blockDict.ContainsKey(bbn)) then
                    failwithf "bad ConditionalDirectTailCalls %s" bbn.Name
                q.Enqueue(bbn) |> ignore
                match conds with
                | [|selector|] ->
                    instructions.Add(sprintf "scoreboard players set %s %s %d" selector ScoreboardNameConstants.IP bbnNumbers.[bbn])
                | _ -> failwith "there should be exactly one conditional selector in advancements' ConditionalDirectTailCalls"
            | Halt ->
                instructions.Add(sprintf "scoreboard players set @s %s %d" ScoreboardNameConstants.Stop 1)
                // TODO line below is a hack to fix duplicate 'done'
                instructions.Add(sprintf "scoreboard players set @s %s %d" ScoreboardNameConstants.IP -1)
            advancements.Add(makeAdvancement(currentBBN.Name,instructions))
    let allBBNs = new HashSet<_>(blockDict.Keys)
    allBBNs.ExceptWith(visited)
    if allBBNs.Count <> 0 then
        failwithf "there were unreferenced basic block names, including for example %s" (allBBNs |> Seq.head).Name

    // advancement runner infrastructure
    // root
    let root = "functions/root",Advancement(None,NoDisplay,Reward([||],[||],0,[|
                    |]),[|Criterion("cx",MC"impossible",[||])|],[|[|"cx"|]|])
    advancements.Add(root)
    let root2 = "functions/root2",Advancement(Some(PATH"functions:root"),NoDisplay,Reward([||],[||],0,[|
                    |]),[|Criterion("cx",MC"impossible",[||])|],[|[|"cx"|]|])
    advancements.Add(root2)
    let fauxroot = "functions/fauxroot",Advancement(Some(PATH"functions:root2"),NoDisplay,Reward([||],[||],0,[|
                    |]),[|Criterion("cx",MC"impossible",[||])|],[|[|"cx"|]|])
    advancements.Add(fauxroot)
    // pump loop (finite cps without deep recursion)
    let MAX_PUMP_DEPTH = 4
    let MAX_PUMP_WIDTH = 10   // (width ^ depth) is max iters
    for i = 1 to MAX_PUMP_DEPTH do
        advancements.Add(makeAdvancement(sprintf"pump%d"i,[|
                yield sprintf """advancement revoke @s only %s:pump%d""" PREFIX i
                for _x = 1 to MAX_PUMP_WIDTH do yield sprintf """advancement grant @s[score_%s=0] only %s:pump%d""" ScoreboardNameConstants.Stop PREFIX (i+1)
            |]))
    // chooser
    advancements.Add(makeAdvancement(sprintf"pump%d"(MAX_PUMP_DEPTH+1),[|
            yield sprintf """advancement revoke @s only %s:pump%d""" PREFIX (MAX_PUMP_DEPTH+1)
            for KeyValue(bbn,num) in bbnNumbers do 
                yield sprintf """advancement grant @s[score_%s_min=%d,score_%s=%d] only %s:%s""" 
                                    ScoreboardNameConstants.IP num ScoreboardNameConstants.IP num PREFIX bbn.Name 
        |]))

    let repumpAfterTick = [|
        RegionFiles.CommandBlock.O "blockdata ~ ~ ~ {auto:0b}"
        RegionFiles.CommandBlock.U (sprintf "scoreboard players set %s %s %d" ENTITY_IP ScoreboardNameConstants.Stop 0)
        RegionFiles.CommandBlock.U (sprintf "advancement grant %s only %s:pump1" ENTITY_IP PREFIX)
        |]

    initialization, repumpAfterTick, advancements

#endif

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

// constants
let FOURISSQ = Var("FOURISSQ")
let INTSCALE = Var("INTSCALE")
let MAXH = Var("MAXH")
let MAXW = Var("MAXW")
let XSCALE = Var("XSCALE")
let YSCALE = Var("YSCALE")
let XMIN = Var("XMIN")
let YMIN = Var("YMIN")
// variables
let i = Var("i")
let j = Var("j")
let x0 = Var("x0")
let x = Var("x")
let y0 = Var("y0")
let y = Var("y")
let n = Var("n")
let xsq = Var("xsq")
let ysq = Var("ysq")
let r1 = Var("r1")
let xtemp = Var("xtemp")

let program = 
    Program(cpsIStart, dict [
        cpsIStart,BasicBlock([|
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


    