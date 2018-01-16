module Compiler

// TODO consider if/how to author two separate datapacks that each use compiler with $NTICKSLATER
// could maybe dump conts in userns/compiler/contNN and call them there, as well as put /userns/compiler/{load,tick,...}?
// no, because cont4 as a score objective means two different things, one for each project...
// we want to reuse the scoreAS entity, but firewall the functions for load/tick/contNN, as well as the objectives contNN & numPendingCont (LINES?)

(*
me: so I recall in some prior version, they changed how r= worked, and there was lots of debate and hulabaloo...
I never really paid much attention
does distance= work the same way in 1.13 as r= in 1.12?  if so, does someone have a decent write-up of how it actually behaves?  if not, any description of changes?

(also recall how summon ~ ~ ~ versus ~0.0 ~0.0 ~0.0 works, align, 0.5s, etc)

*)

let NS = "compiler"

// a basic abstraction over a single canonical entity ($ENTITY) for scores ($SCORE), as well as execution over time ($NTICKLATER)
type Compiler(scoreASx,scoreASy,scoreASz,doProfiling) =
    // TODO why does distance=..0.1 not work?
    let ENTITY_TAG = sprintf "tag=scoreAS,x=%d,y=%d,z=%d,distance=..1.0,limit=1" scoreASx scoreASy scoreASz   // consider UUIDing it? no, because UUIDs do not accept selectors
    let allCallbackShortNames = ResizeArray()
    let continuationNum = ref 1
    let newName() =    // names are like 'cont6'... this is used as scoreboard objective name, and then function full name will be compiler/cont6
        let r = sprintf "cont%d" !continuationNum
        incr continuationNum
        r
    let compile(f,ns,name) =
        let rec replaceScores(s:string) = 
            let i = s.IndexOf("$SCORE(")
            if i <> -1 then
                let j = s.IndexOf(')',i)
                let info = s.Substring(i+7,j-i-7)
                let s = s.Remove(i,j-i+1)
                let s = s.Insert(i,sprintf "@e[%s,scores={%s}]" ENTITY_TAG info)
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
                    sprintf "scoreboard players add @e[%s] numPendingCont 1" ENTITY_TAG
                |], nn
            else
                [|s|], null
        let a = f |> Seq.toArray 
        // $SCORE(...) is maybe e.g. "@e[tag=scoreAS,scores={...}]"
        let a = a |> Array.map replaceScores
        // $ENTITY is main scorekeeper entity (maybe e.g. "@e[tag=scoreAS]")
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
                    cur.Add(sprintf "scoreboard players remove @e[%s] numPendingCont 1" ENTITY_TAG)
                    curNS := NS
                    curName := nn
                incr i
            yield !curNS, !curName, cur.ToArray()
        |]
    #if DEBUG
        let r = [|
            for name,code in r do
                if name <> "theloop" then
                    yield name, [| yield sprintf """tellraw @a ["calling '%s'"]""" name; yield! code |]
                else
                    yield name, [| yield! code; yield sprintf """tellraw @a ["at end theloop, cont6:",{"score":{"name":"@e[%s]","objective":"cont6"}}]""" ENTITY_TAG |]
            |]
    #endif    
        // try to catch common errors in post-processing
        let okChars = [|'a'..'z'|] |> Array.append [|'_';'-'|] |> Array.append [|'0'..'9'|] |> Array.append [|'/'|]  // '.' is also technically allowed, but I do not use it.
        // TODO add more validation, e.g. find all "function foo:bar" and ensure such a function exists, e.g. to check for spelling errors
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
                        yield ns, name, Array.append code [|sprintf "scoreboard players add @e[%s] LINES %d" ENTITY_TAG code.Length|] // TODO note that tick/tick_body are un-profiled; should add that manually
                    |]
            else
                r
        r

    let get_final_compiler_funcs() = [| // Note: an F# function because capturing mutable globals; must be called at end of F# execution after all compile() calls are done
        NS, "load", [|
            if doProfiling then
                yield sprintf "scoreboard objectives add LINES dummy"
            yield sprintf "scoreboard objectives add numPendingCont dummy"
            for cbn in allCallbackShortNames do
                yield sprintf "scoreboard objectives add %s dummy" cbn
            yield sprintf "execute as @p run function %s:load_entity_init" NS
            yield "scoreboard players set @e[tag=scoreAS] numPendingCont 0"
            |]
        // Note: I think #load-initializations must be commutative and idempotent, and must handle both first-time and already-existing world state.
        NS, "load_entity_init", [|
            // use numPendingCont as local temp variable, since only objective we know exists
            
            // look up location of world spawn, assuming this func called from #load at spawn, and then ensure scoreAS x y z is close enough (dx and dz each <160), else warn/fail
            yield sprintf "summon area_effect_cloud ~ ~ ~ {Duration:1,Tags:[loadaec]}"
            yield sprintf "execute store result score @s numPendingCont run data get entity @e[tag=loadaec,limit=1] Pos[0] 1.0"
            if scoreASx >= 0 then
                yield sprintf "scoreboard players add @s numPendingCont %d" scoreASx 
            else
                yield sprintf "scoreboard players remove @s numPendingCont %d" -scoreASx
            yield """execute unless entity @s[scores={numPendingCont=-160..160}] run tellraw @a ["Load failure - Compiler scoreAS x coordinate too far from world spawn, dx:",{"score":{"name":"@s","objective":"numPendingCont"}}]"""
            yield sprintf "execute store result score @s numPendingCont run data get entity @e[tag=loadaec,limit=1] Pos[2] 1.0"
            if scoreASz >= 0 then
                yield sprintf "scoreboard players add @s numPendingCont %d" scoreASz 
            else
                yield sprintf "scoreboard players remove @s numPendingCont %d" -scoreASz
            yield """execute unless entity @s[scores={numPendingCont=-160..160}] run tellraw @a ["Load failure - Compiler scoreAS z coordinate too far from world spawn, dz:",{"score":{"name":"@s","objective":"numPendingCont"}}]"""

            // count the number of scoreAS in the world
            yield sprintf "execute store result score @s numPendingCont if entity @e[%s]" ENTITY_TAG
            // if was zero, summon one
            yield sprintf """execute if entity @s[scores={numPendingCont=0}] run say summoning scoreAS for first time"""
            yield sprintf """execute if entity @s[scores={numPendingCont=0}] run summon armor_stand %d %d %d {CustomName:"\"scoreAS\"",Tags:["scoreAS"],NoGravity:1,Marker:1,Invulnerable:1,Invisible:1}""" scoreASx scoreASy scoreASz
            // if was two or more, kill them all and summon one
            yield sprintf """execute if entity @s[scores={numPendingCont=2..}] run say found multiple scoreAS, killing them and resummoning one"""
            yield sprintf """execute if entity @s[scores={numPendingCont=2..}] run kill @e[%s]""" ENTITY_TAG
            yield sprintf """execute if entity @s[scores={numPendingCont=2..}] run summon armor_stand %d %d %d {CustomName:"\"scoreAS\"",Tags:["scoreAS"],NoGravity:1,Marker:1,Invulnerable:1,Invisible:1}""" scoreASx scoreASy scoreASz
            |]
        NS, "tick", [|  // Note: Must be called as $ENTITY
            if doProfiling then
                yield "scoreboard players set $ENTITY LINES 0"
            //yield "say ---calling compiler:tick---"
            yield sprintf "execute if entity @s[scores={numPendingCont=1..}] run function %s:tick_body" NS
            |]
        NS, "tick_body", [|  // Note: Must be called as $ENTITY
            // first decr all cont counts (after, 0=unscheduled, 1=now, 2...=future)
            for f in allCallbackShortNames do
                yield sprintf "scoreboard players remove @s[scores={%s=1..}] %s 1" f f
            // then call all that need to go now
            for f in allCallbackShortNames do
                yield sprintf "execute if entity @s[scores={%s=1}] run function %s:%s" f NS f
            |]
        |]

    member this.Compile(ns,name,code) = compile(code,ns,name)
    // user should ensure NS:load called to init objectives and summon entities (e.g. during #minecraft:load)
    // user should ensure NS:tick called as $ENTITY at start of each tick (e.g. at top of #minecraft:tick)
    member this.GetCompilerLoadTick() = get_final_compiler_funcs()
    // if want profiling, use e.g.
    //            if PROFILE then
    //                yield sprintf """tellraw @a [{"score":{"name":"@e[%s]","objective":"LINES"}}]""" ENTITY_TAG 
    // at end of tick
    member this.EntityTag = ENTITY_TAG