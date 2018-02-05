module Compiler

// Note: two different datapack can each use their own compiler

// each compiler will generate its own functions like compiler/tick and compiler/cont1xy in a user-specified namespace userNS
// each compiler will generate its own scoreboard objectives suffixed with e.g. xy, two user-specified characters objChar1/objChar2
#if USE_ENTITY_FOR_SCORES
// each compiler will share the same score entity scoreAS located near spawn
#else
// each compiler will share the same fake player named FAKE for scores
#endif

// TODO clean up this code

let FOLDER = "compiler"

// a basic abstraction over a single canonical entity or fake-player for scoreboards ($ENTITY) and for score conditions (if $SCORE/unless $SCORE), as well as execution over time ($NTICKLATER)
type Compiler(objChar1,objChar2,userNS,scoreASx,scoreASy,scoreASz,doProfiling) =
    let objectiveSuffix = sprintf "%c%c" objChar1 objChar2
    let NUM_PENDING_CONT = sprintf "numPendingCont%s" objectiveSuffix
    let LINES = sprintf "LINES%s" objectiveSuffix 
#if USE_ENTITY_FOR_SCORES
    (*
    summon 1 2 3 puts center-feet at 1.5 2 3.5 (adds .5, e.g. -2 becomes -1.5)
    distance=0..N   - is target entity feet-center point in a sphere of radius N centerd at command origin?
    dx/dy/dz        - does this box intersection target's hitbox?
    *)
    let summonLoc(x) =
        let poin(x) = if x < 0 then x+1 else x // plus one if negative
        match x with
        | -1 -> "-0.5"
        | _ -> sprintf "%d.5" (poin x)
    let ENTITY_TAG = sprintf "tag=scoreAS,x=%s,y=%d,z=%s,distance=..0.1,limit=1" (summonLoc scoreASx) scoreASy (summonLoc scoreASz)   // consider UUIDing it? no, because UUIDs do not accept selectors
#else
    let FAKE = "FAKE"  // the name of the 'fake' player on the scoreboard    // TODO consider changing to #FAKE, so can't conflict with a player name
#endif
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
#if USE_ENTITY_FOR_SCORES
                let s = s.Insert(i,sprintf "if entity @e[%s,scores={%s}]" ENTITY_TAG info)
#else
                // for convenience of syntax and to be able to swap compiler implementations, 'info' is a comma separated list of form OBJ=match
                let scores = info.Split(',')
                let replacements = ResizeArray()
                for omp in scores do
                    let [|o;m|] = omp.Split('=')   // o is objective, m is 'matches' spec (e.g. "5..7")
                    replacements.Add(sprintf "if score %s %s matches %s" FAKE o m) |> ignore
                let s = s.Insert(i,String.concat " " replacements)
#endif
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
#if USE_ENTITY_FOR_SCORES
                let s = s.Insert(i,sprintf "if entity @e[%s,scores={%s}]" ENTITY_TAG info)
#else
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
#endif
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
#if USE_ENTITY_FOR_SCORES
                    sprintf """execute if entity @e[%s,scores={%s=2..}] run tellraw @a ["error, re-entrant callback %s"]""" ENTITY_TAG nn nn
                    sprintf "scoreboard players set @e[%s] %s %d" ENTITY_TAG nn (int info + 1) // +1 because we decr at start of gameloop
                    sprintf "scoreboard players add @e[%s] %s 1" ENTITY_TAG NUM_PENDING_CONT
#else
                    sprintf """execute if score %s %s matches 2.. run tellraw @a ["error, re-entrant callback %s"]""" FAKE nn nn  
                    // Note: after error above, compiler would busy-wait with no continuations queued, unless we guard with 'unless' as below
                    // This means a subsequent re-entrant callback gets dropped on the floor (only the first outstanding one is kept)
                    // TODO look for all $NTICKSLATER calls and see if that's ok behavior, or if it would break invariants
                    sprintf "execute unless score %s %s matches 2.. run scoreboard players add %s %s 1" FAKE nn FAKE NUM_PENDING_CONT
                    sprintf "execute unless score %s %s matches 2.. run scoreboard players set %s %s %d" FAKE nn FAKE nn (int info + 1) // +1 because we decr at start of gameloop
#endif
                |], nn
            else
                [|s|], null
        let a = f |> Seq.toArray 
        // $SCORE(...) is maybe e.g. "entity @e[tag=scoreAS,scores={...}]" or "score FAKE OBJ matches ..."
        let a = a |> Array.map replaceIfScores
        let a = a |> Array.map replaceUnlessScores
        // $ENTITY is main scorekeeper entity (maybe e.g. "@e[tag=scoreAS]") or fake player ("FAKE")
#if USE_ENTITY_FOR_SCORES
        let a = a |> Array.map (fun s -> s.Replace("$ENTITY",sprintf"@e[%s]"ENTITY_TAG))
#else
        let a = a |> Array.map (fun s -> s.Replace("$ENTITY",FAKE))
#endif
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
#if USE_ENTITY_FOR_SCORES
                    cur.Add(sprintf "scoreboard players remove @e[%s] %s 1" ENTITY_TAG NUM_PENDING_CONT) 
#else
                    cur.Add(sprintf "scoreboard players remove %s %s 1" FAKE NUM_PENDING_CONT) 
#endif
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
#if USE_ENTITY_FOR_SCORES
                        yield ns, name, Array.append code [|sprintf "scoreboard players add @e[%s] %s %d" ENTITY_TAG LINES code.Length|] // todo note that tick/tick_body are un-profiled; should add that manually
#else
                        yield ns, name, Array.append code [|sprintf "scoreboard players add %s %s %d" FAKE LINES code.Length|] // todo note that tick/tick_body are un-profiled; should add that manually
#endif
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
            yield sprintf "execute as @p run function %s:%s/load_entity_init" userNS FOLDER
#if USE_ENTITY_FOR_SCORES
            yield sprintf "scoreboard players set @e[tag=scoreAS] %s 0" NUM_PENDING_CONT  // TODO setting this at /reload time means in-flight continuations are lost at save&exit, this should be a one-time init
#else
#endif
            |]
        // Note: I think #load-initializations must be commutative and idempotent, and must handle both first-time and already-existing world state.
        userNS, FOLDER+"/load_entity_init", [|
#if USE_ENTITY_FOR_SCORES
            // use NUM_PENDING_CONT as local temp variable, since only objective we know exists  // TODO no, this stomps world state at save&exit
            
            // look up location of world spawn, assuming this func called from #load at spawn, and then ensure scoreAS x y z is close enough (dx and dz each <160), else warn/fail
            yield sprintf "summon area_effect_cloud ~ ~ ~ {Duration:1,Tags:[loadaec]}"
            yield sprintf "execute store result score @s %s run data get entity @e[tag=loadaec,limit=1] Pos[0] 1.0" NUM_PENDING_CONT
            if scoreASx >= 0 then
                yield sprintf "scoreboard players add @s %s %d" NUM_PENDING_CONT scoreASx 
            else
                yield sprintf "scoreboard players remove @s %s %d" NUM_PENDING_CONT -scoreASx
            yield sprintf """execute unless entity @s[scores={%s=-160..160}] run tellraw @a ["Load failure - Compiler scoreAS x coordinate too far from world spawn, dx:",{"score":{"name":"@s","objective":"%s"}}]""" NUM_PENDING_CONT NUM_PENDING_CONT
            yield sprintf "execute store result score @s %s run data get entity @e[tag=loadaec,limit=1] Pos[2] 1.0" NUM_PENDING_CONT
            if scoreASz >= 0 then
                yield sprintf "scoreboard players add @s %s %d" NUM_PENDING_CONT scoreASz 
            else
                yield sprintf "scoreboard players remove @s %s %d" NUM_PENDING_CONT -scoreASz
            yield sprintf """execute unless entity @s[scores={%s=-160..160}] run tellraw @a ["Load failure - Compiler scoreAS z coordinate too far from world spawn, dz:",{"score":{"name":"@s","objective":"%s"}}]""" NUM_PENDING_CONT NUM_PENDING_CONT

            // count the number of scoreAS in the world
            yield sprintf "execute store result score @s %s if entity @e[%s]" NUM_PENDING_CONT ENTITY_TAG
            // if was zero, summon one
            yield sprintf """execute if entity @s[scores={%s=0}] run say summoning scoreAS for first time""" NUM_PENDING_CONT
            yield sprintf """execute if entity @s[scores={%s=0}] run summon armor_stand %d %d %d {CustomName:"\"scoreAS\"",Tags:["scoreAS"],NoGravity:1,Marker:1,Invulnerable:1,Invisible:1}""" NUM_PENDING_CONT scoreASx scoreASy scoreASz
            // if was two or more, kill them all and summon one
            yield sprintf """execute if entity @s[scores={%s=2..}] run say found multiple scoreAS, killing them and resummoning one""" NUM_PENDING_CONT
            yield sprintf """execute if entity @s[scores={%s=2..}] run kill @e[%s]""" NUM_PENDING_CONT ENTITY_TAG
            yield sprintf """execute if entity @s[scores={%s=2..}] run summon armor_stand %d %d %d {CustomName:"\"scoreAS\"",Tags:["scoreAS"],NoGravity:1,Marker:1,Invulnerable:1,Invisible:1}""" NUM_PENDING_CONT scoreASx scoreASy scoreASz
#endif
            |]
        userNS, FOLDER+"/tick", [|
#if USE_ENTITY_FOR_SCORES
            sprintf "execute as @e[%s] run function %s:%s/tick_prelude" ENTITY_TAG userNS FOLDER
#else
            sprintf "function %s:%s/tick_prelude" userNS FOLDER
#endif
            |]
        userNS, FOLDER+"/tick_prelude", [|  // Note: Must be called as $ENTITY
            if doProfiling then
#if USE_ENTITY_FOR_SCORES
                yield sprintf "scoreboard players set @s %s 0" LINES
#else
                yield sprintf "scoreboard players set %s %s 0" FAKE LINES
#endif
            //yield "say ---calling compiler:tick---"
#if USE_ENTITY_FOR_SCORES
            yield sprintf "execute if entity @s[scores={%s=1..}] run function %s:%s/tick_body" NUM_PENDING_CONT userNS FOLDER
#else
            yield sprintf "execute if score %s %s matches 1.. run function %s:%s/tick_body" FAKE NUM_PENDING_CONT userNS FOLDER
#endif
            |]
        userNS, FOLDER+"/tick_body", [|  // Note: Must be called as $ENTITY
            // first decr all cont counts (after, 0=unscheduled, 1=now, 2...=future)
            for f in allCallbackShortNames do
#if USE_ENTITY_FOR_SCORES
                yield sprintf "scoreboard players remove @s[scores={%s=1..}] %s 1" f f
#else
                yield sprintf "execute if score %s %s matches 1.. run scoreboard players remove %s %s 1" FAKE f FAKE f
#endif
            // then call all that need to go now
            for f in allCallbackShortNames do
#if USE_ENTITY_FOR_SCORES
                yield sprintf "execute if entity @s[scores={%s=1}] run function %s:%s/%s" f userNS FOLDER f
#else
                yield sprintf "execute if score %s %s matches 1 run function %s:%s/%s" FAKE f userNS FOLDER f
#endif
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
#if USE_ENTITY_FOR_SCORES
    member this.EntityTag = ENTITY_TAG
#else
    member this.FakePlayerName = FAKE   // e.g. for folks who want to tellraw a score
#endif
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