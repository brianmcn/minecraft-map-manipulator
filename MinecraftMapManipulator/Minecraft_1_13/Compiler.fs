module Compiler

// Note: two different datapack can each use their own compiler

// each compiler will generate its own functions like compiler/tick and compiler/cont1xy in a user-specified namespace userNS
// each compiler will generate its own scoreboard objectives suffixed with e.g. xy, two user-specified characters objChar1/objChar2
// each compiler will share the same fake player named FAKE for scores

let FOLDER = "compiler"

// a basic abstraction over a single canonical entity or fake-player for scoreboards ($ENTITY) and for score conditions (if $SCORE/unless $SCORE), as well as execution over time ($NTICKLATER)
type Compiler(objChar1,objChar2,userNS,doProfiling) =
    let objectiveSuffix = sprintf "%c%c" objChar1 objChar2
    let NUM_PENDING_CONT = sprintf "numPendingCont%s" objectiveSuffix
    let LINES = sprintf "LINES%s" objectiveSuffix 
    let FAKE = "FAKE"  // the name of the 'fake' player on the scoreboard    // TODO consider changing to #FAKE, so can't conflict with a player name
    let allCallbackShortNames = ResizeArray()
    let continuationNum = ref 1
    let newName() =    // names are like 'cont6xy'... this is used as scoreboard objective name, and then function full name will be compiler/cont6xy
        let r = sprintf "cont%d%s" !continuationNum objectiveSuffix
        incr continuationNum
        r
    let compile(f,ns,name) =
        let rec replaceIfScores(s:string) = 
            let TEXT = "if $SCORE("
            let i = s.IndexOf(TEXT)
            if i <> -1 then
                let j = s.IndexOf(')',i)
                let info = s.Substring(i+TEXT.Length,j-i-TEXT.Length)
                let s = s.Remove(i,j-i+1)
                // for convenience of syntax and to be able to swap compiler implementations, 'info' is a comma separated list of form OBJ=match
                let scores = info.Split(',')
                let replacements = ResizeArray()
                for omp in scores do
                    let [|o;m|] = omp.Split('=')   // o is objective, m is 'matches' spec (e.g. "5..7")
                    replacements.Add(sprintf "if score %s %s matches %s" FAKE o m) |> ignore
                let s = s.Insert(i,String.concat " " replacements)
                replaceIfScores(s)
            else
                s
        let rec replaceUnlessScores(s:string) = 
            let TEXT = "unless $SCORE("
            let i = s.IndexOf(TEXT)
            if i <> -1 then
                let j = s.IndexOf(')',i)
                let info = s.Substring(i+TEXT.Length,j-i-TEXT.Length)
                let s = s.Remove(i,j-i+1)
(*
I found the one thing that entity scores can handle easily that fake player scores cannot handle as easily:
    unless entity @e[tag=FAKE,scores={X=0..,Y=0..}]             succeeds if at least one of X or Y is negative
you cannot simply write
    unless score FAKE X matches 0.. unless score FAKE Y matches 0..
because that succeeds only if both X and Y are negative.  You can 'and' entity score conditions under an 'unless', but you cannot do that with fake player scores.
*)
                // for convenience of syntax and to be able to swap compiler implementations, 'info' is a comma separated list of form OBJ=match
                if info.Contains(",") then
                    failwith "compiler cannot handle 'unless $SCORE(...,...)' where multiple items in SCORE"
                let [|o;m|] = info.Split('=')   // o is objective, m is 'matches' spec (e.g. "5..7")
                let s = s.Insert(i,sprintf "unless score %s %s matches %s" FAKE o m)
                replaceIfScores(s)
            else
                s
        let replaceContinue(s:string) = 
            let i = s.IndexOf("$NTICKSLATER(")
            if i <> -1 then
                if i <> 0 then failwith "$NTICKSLATER must be only thing on the line"
                let j = s.IndexOf(')',i)
                if j <> s.Length-1 then failwith "$NTICKSLATER must be only thing on the line"
                let info = s.Substring(i+13,j-i-13)
                // $NTICKSLATER(n) will
                //  - create a new named .mcfunction for the continuation
                //  - create a new scoreboard objective for it
                //  - set the value of e.g. @e[tag=callbackAS] in the new objective to 'n'
                //     - but first check the existing score was 0; this system can't register the same callback function more than once at a time, that would be an error (no re-entrancy)
                //  - add a hook in the gameloop that, foreach callback function in the global registry, will check the score, and
                //     - if the score is ..0, do nothing (unscheduled)
                //     - if the score is 1, call the corresponding callback function (time to continue now)
                //     - else subtract 1 from the score (get 1 tick closer to calling it)
                let nn = newName()
                allCallbackShortNames.Add(nn)
                [|
                    sprintf """execute if score %s %s matches 2.. run tellraw @a ["error, re-entrant callback %s is being dropped on the floor"]""" FAKE nn nn  
                    // Note: after error above, compiler would busy-wait with no continuations queued, unless we guard with 'unless' as below
                    // This means a subsequent re-entrant callback gets dropped on the floor (only the first outstanding one is kept)
                    // TODO look for all $NTICKSLATER calls and see if that's ok behavior, or if it would break invariants
                    sprintf "execute unless score %s %s matches 2.. run scoreboard players add %s %s 1" FAKE nn FAKE NUM_PENDING_CONT
                    sprintf "execute unless score %s %s matches 2.. run scoreboard players set %s %s %d" FAKE nn FAKE nn (int info + 1) // +1 because we decr at start of gameloop
                |], nn
            else
                [|s|], null
        let a = f |> Seq.toArray 
        // $SCORE(...) is maybe e.g. "entity @e[tag=scoreAS,scores={...}]" or "score FAKE OBJ matches ..."
        let a = a |> Array.map replaceIfScores
        let a = a |> Array.map replaceUnlessScores
        // $ENTITY is main scorekeeper entity (maybe e.g. "@e[tag=scoreAS]") or fake player ("FAKE")
        let a = a |> Array.map (fun s -> s.Replace("$ENTITY",FAKE))
        let r = [|
            let cur = ResizeArray()
            let curNS = ref ns
            let curName = ref name
            let i = ref 0
            while !i < a.Length do
                let b,nn = replaceContinue(a.[!i])
                cur.AddRange(b)
                if nn<>null then
                    yield !curNS, !curName, cur.ToArray()
                    cur.Clear()
                    cur.Add(sprintf "scoreboard players remove %s %s 1" FAKE NUM_PENDING_CONT) 
                    curNS := userNS
                    curName := FOLDER+"/"+nn
                incr i
            yield !curNS, !curName, cur.ToArray()
        |]
        // try to catch common errors in post-processing
        let okChars = [|'a'..'z'|] |> Array.append [|'_';'-'|] |> Array.append [|'0'..'9'|] |> Array.append [|'/'|]  // '.' is also technically allowed, but I do not use it.
        // todo add more validation, e.g. find all "function foo:bar" and ensure such a function exists, e.g. to check for spelling errors
        for _ns,name,code in r do
            for c in name do
                if not(okChars |> Array.contains c) then
                    failwithf "bad function name (char '%c'): %s" c name
            for cmd in code do
                let soa = "scoreboard objectives add "
                if cmd.StartsWith(soa) then
                    let i = soa.Length 
                    let j = cmd.IndexOf(" ",i)
                    let objName = cmd.Substring(i,j-i)
                    if objName.Length > 16 then
                        failwithf "bad objective name (too long): '%s'" objName
        let r = 
            if doProfiling then
                [|  for ns,name,code in r do
                        yield ns, name, Array.append code [|sprintf "scoreboard players add %s %s %d" FAKE LINES code.Length|] // todo note that tick/tick_body are un-profiled; should add that manually
                    |]
            else
                r
        r

    let get_final_compiler_funcs() = [| // Note: an F# function because capturing mutable globals; must be called at end of F# execution after all compile() calls are done
        userNS, FOLDER+"/one_time_init", [|
            for cbn in allCallbackShortNames do
                yield sprintf "scoreboard players set %s %s 0" FAKE cbn
            yield sprintf "scoreboard players set %s %s 0" FAKE NUM_PENDING_CONT
            |]
        userNS, FOLDER+"/load", [|
            if doProfiling then
                yield sprintf "scoreboard objectives add %s dummy" LINES
            yield sprintf "scoreboard objectives add %s dummy" NUM_PENDING_CONT
            for cbn in allCallbackShortNames do
                yield sprintf "scoreboard objectives add %s dummy" cbn
            |]
        // Note: I think #load-initializations must be commutative and idempotent, and must handle both first-time and already-existing world state.
        userNS, FOLDER+"/tick", [|
            sprintf "function %s:%s/tick_prelude" userNS FOLDER
            |]
        userNS, FOLDER+"/tick_prelude", [|  // Note: Must be called as $ENTITY
            if doProfiling then
                yield sprintf "scoreboard players set %s %s 0" FAKE LINES
            //yield "say ---calling compiler:tick---"
            yield sprintf "execute if score %s %s matches 1.. run function %s:%s/tick_body" FAKE NUM_PENDING_CONT userNS FOLDER
            |]
        userNS, FOLDER+"/tick_body", [|  // Note: Must be called as $ENTITY
            // first decr all cont counts (after, 0=unscheduled, 1=now, 2...=future)
            for f in allCallbackShortNames do
                yield sprintf "execute if score %s %s matches 1.. run scoreboard players remove %s %s 1" FAKE f FAKE f
            // then call all that need to go now
            for f in allCallbackShortNames do
                yield sprintf "execute if score %s %s matches 1 run function %s:%s/%s" FAKE f userNS FOLDER f
            |]
        |]

    member this.Compile(ns,name,code) = compile(code,ns,name)
    // user should ensure userNS:FOLDER/load called FROM WORLD SPAWN to init objectives and summon entities (e.g. during #minecraft:load)
    // user should ensure userNS:FOLDER/tick called at start of each tick (e.g. at top of #minecraft:tick)
    member this.GetCompilerLoadTick() = get_final_compiler_funcs()
    // if want profiling, use e.g.
    //            if PROFILE then
    //                yield sprintf """tellraw @a [{"score":{"name":"@e[%s]","objective":"%s"}}]""" ENTITY_TAG LINES
    // at end of tick
    member this.FakePlayerName = FAKE   // e.g. for folks who want to tellraw a score
//    member this.Lines = LINES
    member this.LoadFullName = sprintf "%s:%s/load" userNS FOLDER
    member this.TickFullName = sprintf "%s:%s/tick" userNS FOLDER

(*
cheap-o version for singleplayer without NTICKSLATER:

let rec compile(s:string) =
    let i = s.IndexOf("$SCORE(")
    if i <> -1 then
        let j = s.IndexOf(')',i)
        let info = s.Substring(i+7,j-i-7)
        let s = s.Remove(i,j-i+1)
        let s = s.Insert(i,sprintf "entity @p[scores={%s}]" info)
        compile(s)
    else
        s.Replace("$ENTITY","@p")
*)