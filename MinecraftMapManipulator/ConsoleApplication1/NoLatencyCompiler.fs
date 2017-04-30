module NoLatencyCompiler

open System.Collections.Generic 

////////////////////////////////////////////////

module ScoreboardNameConstants =
    // for the command-block compiler
//    let IP = "IP"  // objective name where basic block names say 1 or 0 for whether they are current instruction pointer
    let IP = "IP"  // objective name where basic block names say a value, and CurrentIP says which block is now
    let CurrentIP = "CurrentIP"  // currentIP says which is active now
    let PulseICB = "PulseICB"  // player name in objective IP saying whether to pulse starter ICB next tick (let a game tick run)
    let Halt = "Halt"  // player name in objective IP saying whether to halt the machine
    
    // for the advancements compiler
    let Stop = "Stop" // name of an objective where @p has 1 or 0 depending on if time to stop the loop runner

open Recipes 
open Advancements
let PREFIX = "functions" // advancements folder name
let makeAdvancement(name,instructions) =
    sprintf "%s/%s" PREFIX name, Advancement(Some(PATH(sprintf"%s:root"PREFIX)),NoDisplay,Reward([||],[||],0,[|
        yield! instructions
        |]),[|Criterion("cx",MC"impossible",[||])|],[|[|"cx"|]|])

////////////////////////////////////////////////

type BasicBlockName = 
    | BBN of string
    member this.Name = match this with BBN(s) -> s
type FinalAbstractCommand =
    | DirectTailCall of BasicBlockName
    | ConditionalDirectTailCalls of ((*andedTestCommands*)string[]*(*if-all-then*)BasicBlockName) * (*catch-all-else*)BasicBlockName
    | Halt
type AbstractCommand =
    | AtomicCommand of string // e.g. "say blah", "scoreboard players ..."
    | Yield // express desire to yield CPU back to minecraft to run a tick after this block (cooperative multitasking)
type BasicBlock = BasicBlock of AbstractCommand[] * FinalAbstractCommand
type Program = Program of (*entrypoint*)BasicBlockName * IDictionary<BasicBlockName,BasicBlock>

////////////////////////////////////////////////

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
                | ConditionalDirectTailCalls((_conds,ifbbn),cabbn) ->
                    referencedBBNs.UnionWith[cabbn]
                    referencedBBNs.UnionWith[ifbbn]
                | _ -> ()
        let allBBNs = new HashSet<_>(origBlockDict.Keys)
        allBBNs.ExceptWith(referencedBBNs)
        for unusedBBN in allBBNs do
            printfn "removing unused state '%s' after optimization" unusedBBN.Name 
            finalBlockDict.Remove(unusedBBN) |> ignore
        Program(entrypoint,finalBlockDict)

////////////////////////////////////////////////

type ChainCommandBlockType = U | C

let linearize(Program(entrypoint,blockDict), isTracing,
              x,y,z) = // x,y,z is ICB with BD{auto:0b}
                       // z+1 is CCB with set PulseICB to 0
                       // z+2 is CCB with setblock(z+4) to CCB (not stone)
                       // z+3 is CCB with blockdata(z+4) to ULE:false (can't setblock that way)
                       // z+4 is empty instruction that is first in loop and may get stoned
    let initialization = ResizeArray()
    let mutable foundEntryPointInDictionary = false
    initialization.Add(sprintf "scoreboard objectives add %s dummy" ScoreboardNameConstants.IP)
    initialization.Add(sprintf "scoreboard objectives add %s dummy" ScoreboardNameConstants.Stop)
    let mutable nextBBNNumber = 1
    let bbnNumbers = new Dictionary<_,_>()
    for KeyValue(bbn,_) in blockDict do
        if bbn.Name.Length > 15 then
            failwithf "scoreboard names can only be up to 15 characters: %s" bbn.Name
        bbnNumbers.Add(bbn, nextBBNNumber)
        nextBBNNumber <- nextBBNNumber + 1
        if bbn = entrypoint then
            initialization.Add(sprintf "scoreboard players set %s %s %d" ScoreboardNameConstants.CurrentIP ScoreboardNameConstants.IP bbnNumbers.[bbn])
            foundEntryPointInDictionary <- true
        initialization.Add(sprintf "scoreboard players set %s %s %d" bbn.Name ScoreboardNameConstants.IP bbnNumbers.[bbn])
    if not(foundEntryPointInDictionary) then
        failwith "did not find entrypoint in basic block dictionary"

#if HYBRID
    let advancementBBs = new Dictionary<_,_>()
#endif
    let visited = new HashSet<_>()
    let instructions = ResizeArray()
    let q = new Queue<_>()
    q.Enqueue(entrypoint)
    while q.Count <> 0 do
        let currentBBN = q.Dequeue()
        if not(visited.Contains(currentBBN)) then
            visited.Add(currentBBN) |> ignore
            let (BasicBlock(cmds,finish)) = blockDict.[currentBBN]
            if isTracing then
                instructions.Add(U,sprintf "scoreboard players test %s %s 1 1" currentBBN.Name ScoreboardNameConstants.IP)
                instructions.Add(C,sprintf """tellraw @a ["start block: %s"]""" currentBBN.Name)
#if HYBRID
            advancementBBs.Add(currentBBN,ResizeArray())
            for c in cmds do
                match c with 
                | AtomicCommand s ->
                    advancementBBs.[currentBBN].Add(s)
                | Yield ->
                    advancementBBs.[currentBBN].Add(sprintf "scoreboard players set %s %s 1" ScoreboardNameConstants.PulseICB ScoreboardNameConstants.IP)
#else
            for c in cmds do
                match c with 
                | AtomicCommand s ->
                    instructions.Add(U,sprintf "scoreboard players test %s %s 1 1" currentBBN.Name ScoreboardNameConstants.IP)
                    instructions.Add(C,s)
                | Yield ->
                    instructions.Add(U,sprintf "scoreboard players test %s %s 1 1" currentBBN.Name ScoreboardNameConstants.IP)
                    instructions.Add(C,sprintf "scoreboard players set %s %s 1" ScoreboardNameConstants.PulseICB ScoreboardNameConstants.IP)
#endif
            match finish with
            | DirectTailCall(nextBBN) ->
                if not(blockDict.ContainsKey(nextBBN)) then
                    failwithf "bad DirectTailCall goto %s" nextBBN.Name
                q.Enqueue(nextBBN) |> ignore
                instructions.Add(U,sprintf "scoreboard players test %s %s %d %d" ScoreboardNameConstants.CurrentIP ScoreboardNameConstants.IP bbnNumbers.[currentBBN] bbnNumbers.[currentBBN])
#if HYBRID
                instructions.Add(C,sprintf "advancement grant @p only %s:%s" PREFIX currentBBN.Name) // codegen to invoke AtomicCommands without needlessly replicating the IP test
#endif
                instructions.Add(C,sprintf "scoreboard players set %s %s %d" ScoreboardNameConstants.CurrentIP ScoreboardNameConstants.IP bbnNumbers.[nextBBN])
            | ConditionalDirectTailCalls((conds,ifbbn),catchAllBBN) ->
                if not(blockDict.ContainsKey(catchAllBBN)) then
                    failwithf "bad ConditionalDirectTailCalls catchall %s" catchAllBBN.Name
                q.Enqueue(catchAllBBN) |> ignore
                instructions.Add(U,sprintf "scoreboard players test %s %s %d %d" ScoreboardNameConstants.CurrentIP ScoreboardNameConstants.IP bbnNumbers.[currentBBN] bbnNumbers.[currentBBN])
#if HYBRID
                instructions.Add(C,sprintf "advancement grant @p only %s:%s" PREFIX currentBBN.Name)
                // code below is technically control flow, but can be lifted into 'effect' code without conditionals, so may as well do it
                advancementBBs.[currentBBN].Add(sprintf "scoreboard players set %s %s %d" ScoreboardNameConstants.CurrentIP ScoreboardNameConstants.IP bbnNumbers.[catchAllBBN])
#else
                instructions.Add(C,sprintf "scoreboard players set %s %s %d" ScoreboardNameConstants.CurrentIP ScoreboardNameConstants.IP bbnNumbers.[catchAllBBN])
#endif
                if not(blockDict.ContainsKey(ifbbn)) then
                    failwithf "bad ConditionalDirectTailCalls %s" ifbbn.Name
                q.Enqueue(ifbbn) |> ignore
                for c in conds do
                    instructions.Add(C,c)
                instructions.Add(C,sprintf "scoreboard players set %s %s %d" ScoreboardNameConstants.CurrentIP ScoreboardNameConstants.IP bbnNumbers.[ifbbn])
            | Halt ->
                instructions.Add(U,sprintf "scoreboard players test %s %s %d %d" ScoreboardNameConstants.CurrentIP ScoreboardNameConstants.IP bbnNumbers.[currentBBN] bbnNumbers.[currentBBN])
#if HYBRID
                instructions.Add(C,sprintf "advancement grant @p only %s:%s" PREFIX currentBBN.Name)
#endif
                instructions.Add(C,sprintf "scoreboard players set %s %s 0" ScoreboardNameConstants.PulseICB ScoreboardNameConstants.IP)
                instructions.Add(C,sprintf "scoreboard players set %s %s 0" currentBBN.Name ScoreboardNameConstants.IP)
                instructions.Add(C,sprintf "setblock %d %d %d stone" x y (z+4))
    // instructions below aren't part of any basic block, but must be executed every loop, so put unguarded at end
    instructions.Add(U,sprintf "scoreboard players test %s %s 1 1" ScoreboardNameConstants.PulseICB ScoreboardNameConstants.IP)
    instructions.Add(C,sprintf "setblock %d %d %d stone" x y (z+4))
    instructions.Add(C,sprintf "blockdata %d %d %d {auto:1b}" x y z)
    let allBBNs = new HashSet<_>(blockDict.Keys)
    allBBNs.ExceptWith(visited)
    if allBBNs.Count <> 0 then
        failwithf "there were unreferenced basic block names, including for example %s" (allBBNs |> Seq.head).Name
#if HYBRID
    let advancements = [|
        for KeyValue(bbn,cmds) in advancementBBs do
            yield makeAdvancement(bbn.Name,[|
                yield sprintf "advancement revoke @p only %s:%s" PREFIX bbn.Name
                yield! cmds
                |])
        |]
    initialization, instructions, advancements
#else
    initialization, instructions
#endif


////////////////////////////////////////////////


// TODO compile a program into advancements
(*
e.g. 16x loop1/2/3/4 that conditionally-call-down if not halted to get 65k loop without recursion
root module to switch based on currently active function and call it
writing advancement for each function, ungranting itself at start
*)

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
            advancements.Add(makeAdvancement(currentBBN.Name,instructions))
    let allBBNs = new HashSet<_>(blockDict.Keys)
    allBBNs.ExceptWith(visited)
    if allBBNs.Count <> 0 then
        failwithf "there were unreferenced basic block names, including for example %s" (allBBNs |> Seq.head).Name

    // advancement runner infrastructure
    // root
    let root = "functions/root",Advancement(None,NoDisplay,Reward([||],[||],0,[|
                    """advancement revoke @s only functions:root"""
                    (* TODO remove
                    """stats entity @e[name=Cursor] set QueryResult @e[name=Cursor] A"""
                    """scoreboard player set @e[name=Cursor] A 1""" // need initial value before can trigger a stat
                    """worldborder set 10000000"""
                    """worldborder add 1000000 1000"""
                    *)
                    |]),[|Criterion("cx",MC"impossible",[||])|],[|[|"cx"|]|])
    advancements.Add(root)
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
