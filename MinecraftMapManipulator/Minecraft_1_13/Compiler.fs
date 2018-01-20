module Compiler

// Note: two different datapack can each use their own compiler

// each compiler will generate its own functions like compiler/tick and compiler/cont1xy in a user-specified namespace userNS
// each compiler will generate its own scoreboard objectives suffixed with e.g. xy, two user-specified characters objChar1/objChar2
// each compiler will share the same score entity scoreAS located near spawn

// TODO it seems like fake-player scores are better than entity scores now, any reason not to switch to fake players? no, do it. (tellraw works with fake players, execute-store to fake players works, ...)

let FOLDER = "compiler"

// a basic abstraction over a single canonical entity ($ENTITY) for scores ($SCORE), as well as execution over time ($NTICKLATER)
type Compiler(objChar1,objChar2,userNS,scoreASx,scoreASy,scoreASz,doProfiling) =
    let objectiveSuffix = sprintf "%c%c" objChar1 objChar2
    let NUM_PENDING_CONT = sprintf "numPendingCont%s" objectiveSuffix
    let LINES = sprintf "LINES%s" objectiveSuffix 
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
    let allCallbackShortNames = ResizeArray()
    let continuationNum = ref 1
    let newName() =    // names are like 'cont6'... this is used as scoreboard objective name, and then function full name will be compiler/cont6
        let r = sprintf "cont%d%s" !continuationNum objectiveSuffix
        incr continuationNum
        r
    let compile(f,ns,name) =
        let rec replaceScores(s:string) = 
            let i = s.IndexOf("$SCORE(")
            if i <> -1 then
                let j = s.IndexOf(')',i)
                let info = s.Substring(i+7,j-i-7)
                let s = s.Remove(i,j-i+1)
                let s = s.Insert(i,sprintf "entity @e[%s,scores={%s}]" ENTITY_TAG info)
                replaceScores(s)
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
                    sprintf """execute if entity @e[%s,scores={%s=2..}] run tellraw @a ["error, re-entrant callback %s"]""" ENTITY_TAG nn nn
                    sprintf "scoreboard players set @e[%s] %s %d" ENTITY_TAG nn (int info + 1) // +1 because we decr at start of gameloop
                    sprintf "scoreboard players add @e[%s] %s 1" ENTITY_TAG NUM_PENDING_CONT
                |], nn
            else
                [|s|], null
        let a = f |> Seq.toArray 
        // $SCORE(...) is maybe e.g. "entity @e[tag=scoreAS,scores={...}]" or "score FAKE OBJ matches ..."
        let a = a |> Array.map replaceScores
        // $ENTITY is main scorekeeper entity (maybe e.g. "@e[tag=scoreAS]") or fake player ("FAKE")
        let a = a |> Array.map (fun s -> s.Replace("$ENTITY",sprintf"@e[%s]"ENTITY_TAG))
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
                    cur.Add(sprintf "scoreboard players remove @e[%s] %s 1" ENTITY_TAG NUM_PENDING_CONT) 
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
                        yield ns, name, Array.append code [|sprintf "scoreboard players add @e[%s] %s %d" ENTITY_TAG LINES code.Length|] // todo note that tick/tick_body are un-profiled; should add that manually
                    |]
            else
                r
        r

    let get_final_compiler_funcs() = [| // Note: an F# function because capturing mutable globals; must be called at end of F# execution after all compile() calls are done
        userNS, FOLDER+"/load", [|
            if doProfiling then
                yield sprintf "scoreboard objectives add %s dummy" LINES
            yield sprintf "scoreboard objectives add %s dummy" NUM_PENDING_CONT
            for cbn in allCallbackShortNames do
                yield sprintf "scoreboard objectives add %s dummy" cbn
            yield sprintf "execute as @p run function %s:%s/load_entity_init" userNS FOLDER
            yield sprintf "scoreboard players set @e[tag=scoreAS] %s 0" NUM_PENDING_CONT  // TODO setting this at /reload time means in-flight continuations are lost at save&exit, this should be a one-time init
            |]
        // Note: I think #load-initializations must be commutative and idempotent, and must handle both first-time and already-existing world state.
        userNS, FOLDER+"/load_entity_init", [|
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
            |]
        userNS, FOLDER+"/tick", [|
            sprintf "execute as @e[%s] run function %s:%s/tick_prelude" ENTITY_TAG userNS FOLDER
            |]
        userNS, FOLDER+"/tick_prelude", [|  // Note: Must be called as $ENTITY
            if doProfiling then
                yield sprintf "scoreboard players set @s %s 0" LINES
            //yield "say ---calling compiler:tick---"
            yield sprintf "execute if entity @s[scores={%s=1..}] run function %s:%s/tick_body" NUM_PENDING_CONT userNS FOLDER
            |]
        userNS, FOLDER+"/tick_body", [|  // Note: Must be called as $ENTITY
            // first decr all cont counts (after, 0=unscheduled, 1=now, 2...=future)
            for f in allCallbackShortNames do
                yield sprintf "scoreboard players remove @s[scores={%s=1..}] %s 1" f f
            // then call all that need to go now
            for f in allCallbackShortNames do
                yield sprintf "execute if entity @s[scores={%s=1}] run function %s:%s/%s" f userNS FOLDER f
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
    member this.EntityTag = ENTITY_TAG
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