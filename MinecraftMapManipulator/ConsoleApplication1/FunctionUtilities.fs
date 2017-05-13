module FunctionUtilities

// binaryLookup("findPhi", "look", "rym", "ry", 7, 3, -180, fs) 
// looks at @e[tag=look] and binary searches 2^7 in steps of 3 at offset -180, so e.g. [-180..-178] and [-177..-175] are bottom buckets
// and fs is a list of functions applied to the values (like -179) resulting in 
// (name,val) the name of a variable objective to write and the value to write on ENTITY in scoreboard
let binaryLookup(prefix, entityTag, minSel, maxSel, exp, k, offset, fs) =
    let mutable n = 1
    for _i = 1 to exp do
        n <- n * 2
    let functions = ResizeArray()
    let outputObjectives = new System.Collections.Generic.HashSet<_>()
    let makeName(lo,hi) = sprintf "%s/do%dto%d" prefix lo hi
    let rec go(lo,hi) =
        let name = makeName(lo,hi)
        if hi-lo < k then
            functions.Add(name,[|
                for i = lo to hi do
                    for f in fs do
                        let obj,num = f i
                        outputObjectives.Add(obj) |> ignore
                        yield sprintf "execute @e[tag=%s,%s=%d,%s=%d] ~ ~ ~ scoreboard players set %s %s %d" entityTag minSel i maxSel i FunctionCompiler.ENTITY_UUID obj num
                |])
        else
            let mid = (hi-lo)/2 + lo
            let midn = mid+1
            functions.Add(name,[|
                yield sprintf "execute @e[tag=%s,%s=%d,%s=%d] ~ ~ ~ function %s:%s" entityTag minSel lo maxSel mid FunctionCompiler.FUNCTION_NAMESPACE (makeName(lo,mid))
                yield sprintf "execute @e[tag=%s,%s=%d,%s=%d] ~ ~ ~ function %s:%s" entityTag minSel midn maxSel hi FunctionCompiler.FUNCTION_NAMESPACE (makeName(midn,hi))
                |])
            go(lo,mid)
            go(midn,hi)
    go(offset,offset+n*k)
    functions.Add(prefix,[|
        sprintf "# %s" prefix
        sprintf "# inputs: an entity has already been tagged '%s'" entityTag
        sprintf "# outputs: %A" (outputObjectives |> Seq.toList)
        sprintf "function %s:%s" FunctionCompiler.FUNCTION_NAMESPACE (makeName(offset,offset+n*k))
        |])
    functions

// Note: from an F# point-of-view, it'd be better to make these vars properties of a findPhi object, but making at global F# scope reflects fact that scoreboard objectives are global
let phiScope = new FunctionCompiler.Scope()
let vphi = phiScope.RegisterVar("phi")
let vKsinPhi = phiScope.RegisterVar("KsinPhi")
let vKcosPhi = phiScope.RegisterVar("KcosPhi")
let findPhi = 
    let funcs = binaryLookup("findPhi", "look", "rym", "ry", 7, 3, -180, 
                                [fun phi -> vphi.Name,phi
                                 fun phi -> vKsinPhi.Name,int(1000.0*sin(System.Math.PI * float phi / 180.0))
                                 fun phi -> vKcosPhi.Name,int(1000.0*cos(System.Math.PI * float phi / 180.0))
                                ]) 
    let oneTimeInit = [|
        // declare variables (objectives), initialize constants, place any long-lasting objects in the world
        for v in phiScope.All() do
            yield sprintf "scoreboard objectives add %s dummy" v.Name
        |]
    FunctionCompiler.DropInModule("findPhi",oneTimeInit,funcs.ToArray())

let thetaScope = new FunctionCompiler.Scope()
let vtheta = thetaScope.RegisterVar("theta")
let vKsinTheta = thetaScope.RegisterVar("Ksintheta")
let vKcosTheta = thetaScope.RegisterVar("Kcostheta")
let findTheta = 
    let funcs = binaryLookup("findTheta", "look", "rxm", "rx", 6, 3, -90, 
                                [fun theta -> vtheta.Name,theta
                                 fun theta -> vKsinTheta.Name,int(1000.0*sin(System.Math.PI * float theta / 180.0))
                                 fun theta -> vKcosTheta.Name,int(1000.0*cos(System.Math.PI * float theta / 180.0))
                                ]) 
    let oneTimeInit = [|
        // declare variables (objectives), initialize constants, place any long-lasting objects in the world
        for v in thetaScope.All() do
            yield sprintf "scoreboard objectives add %s dummy" v.Name
        |]
    FunctionCompiler.DropInModule("findTheta",oneTimeInit,funcs.ToArray())

//////////////////////////////////////

let profileThis(suffix,pre,cmds,post) =
    let profilerFunc = FunctionCompiler.makeFunction("prof-"+suffix,[
        yield "gamerule maxCommandChainLength 999999"
        yield "gamerule commandBlockOutput false"
        yield "gamerule sendCommandFeedback false"
        yield "gamerule logAdminCommands false"

        yield "scoreboard objectives add A dummy"
        yield "scoreboard objectives add WB dummy"

        yield "scoreboard objectives setdisplay sidebar A"

        yield "execute @p ~ ~ ~ summon armor_stand ~ ~ ~ {CustomName:Timer,NoGravity:1,Invulnerable:1}" 
        yield "scoreboard players set @e[name=Timer] WB 1" 
        yield "stats entity @e[name=Timer] set QueryResult @e[name=Timer] WB" 

        yield "worldborder set 10000000" 
        yield "worldborder add 1000000 1000" 
        
        yield! pre
        for _i = 1 to 100 do
            yield sprintf "function %s:code-%s" FunctionCompiler.FUNCTION_NAMESPACE suffix
        yield! post

        yield "tellraw @a [\"done!\"]" 
        yield "execute @e[name=Timer] ~ ~ ~ worldborder get" 
        yield "scoreboard players set Time A -10000000" 
        yield "scoreboard players operation Time A += @e[name=Timer] WB" 
        yield """tellraw @a ["took ",{"score":{"name":"Time","objective":"A"}}," milliseconds"]"""
        yield "kill @e[name=Timer]"
        ])
    let dummyFunc = FunctionCompiler.makeFunction("code-"+suffix,[|
        for _i = 1 to 1000 do 
            yield! cmds 
        |])
    [| profilerFunc; dummyFunc |]

//////////////////////////////////////

open FunctionCompiler

let init      = BBN"init"
let whiletest = BBN"whiletest"
let loopbody  = BBN"loopbody"
let coda      = BBN"coda"

let raycastVars = new Scope()
// constants
let R = raycastVars.RegisterVar("R")
let ONE_THOUSAND = raycastVars.RegisterVar("ONE_THOUSAND")
// variables
let DX = raycastVars.RegisterVar("DX")
let DY = raycastVars.RegisterVar("DY")
let DZ = raycastVars.RegisterVar("DZ")
let FLIPX = raycastVars.RegisterVar("FLIPX")
let FLIPY = raycastVars.RegisterVar("FLIPY")
let FLIPZ = raycastVars.RegisterVar("FLIPZ")
let TEMP = raycastVars.RegisterVar("TEMP")
let TDX = raycastVars.RegisterVar("TDX")
let TDY = raycastVars.RegisterVar("TDY")
let TDZ = raycastVars.RegisterVar("TDZ")
let MAJOR = raycastVars.RegisterVar("MAJOR")
let TMAJOR = raycastVars.RegisterVar("TMAJOR")
let AX = raycastVars.RegisterVar("AX")
let AY = raycastVars.RegisterVar("AY")
let AZ = raycastVars.RegisterVar("AZ")

let yOffset = 5   // attempt to put all the armor stands not-in-my-face so that I can throw snowballs

// TODO only activate when holding snowball
// uses https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
let raycastProgram = 
    Program([|findTheta;findPhi|],[|
        // dependencies
        for DropInModule(_,oneTimeInit,_) in [findPhi; findTheta] do
            yield! oneTimeInit |> Seq.map (fun cmd -> AtomicCommand(cmd))
        // SB init
        for v in raycastVars.All() do
            yield AtomicCommand(sprintf "scoreboard objectives add %s dummy" v.Name)
        // constants
        yield SB(R .= 128)
        yield SB(ONE_THOUSAND .= 1000)
        // prep code
        yield AtomicCommand "summon armor_stand 0 4 0 {CustomName:RAY,NoGravity:1,Invisible:1,Glowing:1,Invulnerable:1}"
        yield AtomicCommand "scoreboard players tag @p add look"
        |],init,dict[
        init,BasicBlock([|
            AtomicCommand(sprintf "function %s:findTheta" FunctionCompiler.FUNCTION_NAMESPACE)
            AtomicCommand(sprintf "function %s:findPhi" FunctionCompiler.FUNCTION_NAMESPACE)
            //let DX = - R cos(theta) sin(phi)
            SB(DX .= 0)
            SB(DX .-= R)
            SB(DX .*= vKcosTheta)
            SB(DX .*= vKsinPhi)
            SB(DX ./= ONE_THOUSAND)
            SB(DX ./= ONE_THOUSAND)
            //let DY = -R sin(theta)
            SB(DY .= 0)
            SB(DY .-= R)
            SB(DY .*= vKsinTheta)
            SB(DY ./= ONE_THOUSAND)
            //let DZ = R cos(theta) cos(phi)
            SB(DZ .= R)
            SB(DZ .*= vKcosTheta)
            SB(DZ .*= vKcosPhi)
            SB(DZ ./= ONE_THOUSAND)
            SB(DZ ./= ONE_THOUSAND)
(*
            // debug
            AtomicCommand(sprintf "scoreboard players operation @p %s = %s %s" DX.Name ENTITY_UUID DX.Name)
            AtomicCommand(sprintf "scoreboard players operation @p %s = %s %s" DY.Name ENTITY_UUID DY.Name)
            AtomicCommand(sprintf "scoreboard players operation @p %s = %s %s" DZ.Name ENTITY_UUID DZ.Name)
            AtomicCommand(sprintf """tellraw @a ["DX=",{"score":{"name":"@p","objective":"%s"}}," DY=",{"score":{"name":"@p","objective":"%s"}}," DZ=",{"score":{"name":"@p","objective":"%s"}}]""" DX.Name DY.Name DZ.Name)
*)
            // all D_ vars need to be positive, flip if needed and track what we flipped
            SB(FLIPX .= 0)
            SB(FLIPY .= 0)
            SB(FLIPZ .= 0)
            //if DX < 0 then
            //    DX = -DX
            //    FLIPX = true
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" DX.Name (SB(TEMP .= 0).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" DX.Name (SB(TEMP .-= DX).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" DX.Name (SB(FLIPX .= 1).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" DX.Name (SB(DX .= TEMP).AsCommand()))
            //if DY < 0 then
            //    DY = -DY
            //    FLIPY = true
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" DY.Name (SB(TEMP .= 0).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" DY.Name (SB(TEMP .-= DY).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" DY.Name (SB(FLIPY .= 1).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" DY.Name (SB(DY .= TEMP).AsCommand()))
            //if DZ < 0 then
            //    DZ = -DZ
            //    FLIPZ = true
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" DZ.Name (SB(TEMP .= 0).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" DZ.Name (SB(TEMP .-= DZ).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" DZ.Name (SB(FLIPZ .= 1).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" DZ.Name (SB(DZ .= TEMP).AsCommand()))
            //let TDX = DX + DX
            SB(TDX .= DX)
            SB(TDX .+= DX)
            //let TDY = DY + DY
            SB(TDY .= DY)
            SB(TDY .+= DY)
            //let TDZ = DZ + DZ
            SB(TDZ .= DZ)
            SB(TDZ .+= DZ)
            // major is the largest of the 3
            //let MAJOR = DX
            SB(MAJOR .= DX)
            //if DY - MAJOR > 0 then
            //    MAJOR = DY
            SB(TEMP .= DY)
            SB(TEMP .-= MAJOR)
            AtomicCommand(sprintf "execute @s[score_%s_min=0] ~ ~ ~ %s" TEMP.Name (SB(MAJOR .= DY).AsCommand()))
            //if DZ - MAJOR > 0 then
            //    MAJOR = DZ
            SB(TEMP .= DZ)
            SB(TEMP .-= MAJOR)
            AtomicCommand(sprintf "execute @s[score_%s_min=0] ~ ~ ~ %s" TEMP.Name (SB(MAJOR .= DZ).AsCommand()))
            //let TMAJOR = MAJOR + MAJOR
            SB(TMAJOR .= MAJOR)
            SB(TMAJOR .+= MAJOR)
            //let AX = TMAJOR - DX
            SB(AX .= TMAJOR)
            SB(AX .-= DX)
            //let AY = TMAJOR - DY
            SB(AY .= TMAJOR)
            SB(AY .-= DY)
            //let AZ = TMAJOR - DZ
            SB(AZ .= TMAJOR)
            SB(AZ .-= DZ)
            // put armor stand at right starting point
            AtomicCommand("tp @e[name=RAY] @p") // now RAY has my facing
            AtomicCommand(sprintf "tp @e[name=RAY] ~ ~%d ~" (1+yOffset)) // eyeball level (+offset)
(* TODO snap-to-grid
            AtomicCommand("execute @p ~ ~ ~ summon shulker ~ ~1 ~ {NoAI:1}") // snap to grid
            AtomicCommand("execute @e[type=shulker] ~ ~ ~ teleport @e[name=RAY] ~ ~ ~")
            AtomicCommand("tp @e[type=shulker] ~ ~-300 ~") // kill shulker
*)
            |],DirectTailCall(whiletest))
        whiletest,BasicBlock([|
            |],ConditionalTailCall(Conditional[| MAJOR .>= 1 |],loopbody,coda))
        loopbody,BasicBlock([|
            // remember where we are, so can back up
            AtomicCommand "execute @e[name=RAY] ~ ~ ~ summon armor_stand ~ ~ ~  {CustomName:tempAS,NoGravity:1,Invisible:1,Invulnerable:1}"
            //if AX > 0 then
            //    if FLIPX then
            //        tp RAY ~-1 ~ ~
            //    else
            //        tp RAY ~1 ~ ~
            //    AX = AX - TMAJOR
            // AX = AX + 2DX
            AtomicCommand(sprintf "execute @s[score_%s_min=1,score_%s_min=1] ~ ~ ~ tp @e[name=RAY] ~-1 ~ ~" AX.Name FLIPX.Name)
            AtomicCommand(sprintf "execute @s[score_%s_min=1,score_%s=0] ~ ~ ~ tp @e[name=RAY] ~1 ~ ~" AX.Name FLIPX.Name)
            AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ %s" AX.Name (SB(AX .-= TMAJOR).AsCommand()))
            SB(AX .+= TDX)
            // ditto for y
            AtomicCommand(sprintf "execute @s[score_%s_min=1,score_%s_min=1] ~ ~ ~ tp @e[name=RAY] ~ ~-1 ~" AY.Name FLIPY.Name)
            AtomicCommand(sprintf "execute @s[score_%s_min=1,score_%s=0] ~ ~ ~ tp @e[name=RAY] ~ ~1 ~" AY.Name FLIPY.Name)
            AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ %s" AY.Name (SB(AY .-= TMAJOR).AsCommand()))
            SB(AY .+= TDY)
            // ditto for z
            AtomicCommand(sprintf "execute @s[score_%s_min=1,score_%s_min=1] ~ ~ ~ tp @e[name=RAY] ~ ~ ~-1" AZ.Name FLIPZ.Name)
            AtomicCommand(sprintf "execute @s[score_%s_min=1,score_%s=0] ~ ~ ~ tp @e[name=RAY] ~ ~ ~1" AZ.Name FLIPZ.Name)
            AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ %s" AZ.Name (SB(AZ .-= TMAJOR).AsCommand()))
            SB(AZ .+= TDZ)
            // MAJOR = MAJOR - 1
            SB(MAJOR .-= 1)
            // detect non-air and exit loop early
            SB(TEMP .= 1)
            // TODO line below not work, because uses @s for TEMP, which is wrong under /execute... need a way to abstract this idiom
            //AtomicCommand(sprintf "execute @e[name=RAY] ~ ~ ~ detect ~ ~ ~ air 0 %s" (SB(TEMP .= 0).AsCommand()))
            AtomicCommand(sprintf "execute @e[name=RAY] ~ ~ ~ detect ~ ~%d ~ air 0 execute @e[name=RAY] ~ ~ ~ detect ~ ~%d ~ air 0 scoreboard players set %s %s 0" (0-yOffset) (-1-yOffset) ENTITY_UUID TEMP.Name)
            // line above has two E-Ds to check current block and block below, since player is 2-tall and we are at eyeball level
            SB(ScoreboardPlayersConditionalSet(Conditional[|TEMP .>= 1|],MAJOR,0))
            AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ execute @e[name=tempAS] ~ ~ ~ teleport @e[name=RAY] ~ ~ ~" TEMP.Name) // tp RAY to tempAS but preserve RAY's facing direction
            // kill tempAS
            AtomicCommand("kill @e[name=tempAS]")
            |],DirectTailCall(whiletest))
        coda,BasicBlock([|
            AtomicCommand(sprintf "tp @e[name=RAY] ~ ~%d ~" -yOffset)
            AtomicCommand("execute @e[type=snowball] ~ ~ ~ tp @p @e[name=RAY]")
            AtomicCommand("kill @e[type=snowball]")
            Yield
            |],ConditionalTailCall(Conditional[|TEMP .>= 0|], init, init))  // TODO stupid way to get past fact that my inliner incorrectly inlines DirectCalls through Yields
            //|],Halt)
        ])

//////////////////////////////////////

let prngScope = new Scope()
let prng_A = prngScope.RegisterVar("prng_A")
let prng_C = prngScope.RegisterVar("prng_C")
let prng_Two = prngScope.RegisterVar("prng_Two")
let prng_Two16 = prngScope.RegisterVar("prng_Two16")
let prng_Z = prngScope.RegisterVar("prng_Z")
let prng_Mod = prngScope.RegisterVar("prng_Mod")  // the input
let prng_K = prngScope.RegisterVar("prng_K")      // the output
let prng =
    let oneTimeInit = [|
        // declare variables
        for v in prngScope.All() do
            yield sprintf "scoreboard objectives add %s dummy" v.Name
        // initialize constants
        yield sprintf "scoreboard players set %s 1103515245" (prng_A.AsCommandFragmentWithoutEntityBoundToAtS())
        yield sprintf "scoreboard players set %s 12345" (prng_C.AsCommandFragmentWithoutEntityBoundToAtS()) 
        yield sprintf "scoreboard players set %s 2" (prng_Two.AsCommandFragmentWithoutEntityBoundToAtS()) 
        yield sprintf "scoreboard players set %s 65536" (prng_Two16.AsCommandFragmentWithoutEntityBoundToAtS()) 
        // one-time-initialize variables
        yield sprintf "scoreboard players set %s 0" (prng_Z.AsCommandFragmentWithoutEntityBoundToAtS()) 
        // place any long-lasting objects in the world
    |]
    let Z = prng_Z
    let A = prng_A
    let C = prng_C
    let K = prng_K
    let Two = prng_Two
    let Two16 = prng_Two16 
    let Mod = prng_Mod
    let cmds = [|
        // compute next Z value with PRNG
        SB(Z .*= A)
        SB(Z .+= C)
        SB(Z .*= Two)  // mod 2^31
        SB(Z ./= Two)
        SB(K .= Z)
        SB(K .*= Two)
        SB(K ./= Two)
        SB(K ./= Two16) // upper 16 bits most random
        // get a number in the desired range
        SB(K .%= Mod)
        SB(K .+= Mod)  // ensure non-negative
        SB(K .%= Mod)
    |]
    let prngBody = "prngBody",cmds|>Array.map(fun c -> c.AsCommand())
    let prngMain = "prng",[|
        "# prng"
        "# inputs: prng_Mod    e.g. if it's 20, the output will be random number in range 0-19"
        "# outputs: prng_K"
        // TODO do I need this indirection?  I wanted to test this manually by calling "function lorgon111:prng", but perhaps 
        //    I should test via "execute 1-1-1-0-1 ~ ~ ~ function lorgon111:prng", and all functions assume entity is the sender?
        sprintf "execute %s ~ ~ ~ function %s:prngBody" ENTITY_UUID FUNCTION_NAMESPACE
        |]
    DropInModule("prng",oneTimeInit,[|prngBody;prngMain|])

(*

setup objectives
init prng

not running
player drinks potion, advancement is granted, runs a function to turn on the machine:

init countdown timer for num ticks until potion runs out
while timeleft > 0 do
    execute @e[type=zombie,tag=!processed] ~ ~ ~ function process_zombie_init
    // same for spiders, etc

    // find all AS without a host
    scoreboard players set @e[tag=rider] RiderHasHost 0
    execute @e[tag=processed] ~ ~1 ~ scoreboard players set @e[type=armor_stand,tag=rider,c=1,dy=3] RiderHasHost 1
    kill @e[type=armor_stand,tag=rider,score_RiderHasHost=0]

    Yield // this loop runs once per tick
done
kill @e[type=armor_stand,tag=rider]
entitydata @e[tag=processed] {Glowing:0b}
// deathloottable remains


process_zombie_init:
@s is our guy
scoreboard players tag @s add processed
run prng
if prng.next says this zombie gets rare loot then
    choose the loot
    make zombie glowing
    give him AS picturing loot (AS tagged 'rider')
    set his deathloottable
else
    give him AS CustomName 0


// this could be part of a larger 'bestiary' system where you add mobs after you've killed enough of them or something?

*)

// incentivize (selective) fighting more than farming... how communicate that this individual mob has no drop, but others of his type may?  Maybe CustomName "X"?
//idea: foresight potion, shows what rare things mobs will drop
//  - zombie/skeleton/pigzombie wear item as head armor slot
//  - other mobs have invisible armor stand riding them that has head-armor item
//to implement it, would have to use commands:
//  - select an unprocessed-tag mob, mark to-process
//  - deceide whether it gets a rare drop at all (most don't)
//  - if so, select a 'kind' (item sans enchants) of rare drop from its table (long list of commands, PRNG to activate one)
//  - entitydata/replaceitem the mob to display it; entitydata its DeathLootTable to drop it (loot table could still e.g. apply random enchants)
//  - (requires factoring loot tables differently, so 'common' loot is in a separate table, so 'rare' could still reference 'common'; alternatively, could just be 'extra' loot atop normal drops)
// /summon minecraft:zombie ~ ~ ~ {ArmorItems:[{},{},{},{id:apple,Count:1b}]}     // zombie does not burn in sunlight, as has "helmet"
// /summon minecraft:armor_stand ~ ~ ~ {ArmorItems:[{},{},{},{id:apple,Count:1b}]}
// /summon spider ~ ~ ~ {Passengers:[{id:armor_stand,Small:1b,ArmorItems:[{},{},{},{id:apple,Count:1b}]}]}
// /summon zombie ~ ~ ~ {Passengers:[{id:armor_stand,Small:1b,Invisible:1b,CustomName:"0",CustomNameVisible:1b}]}
//  - would need to tag invisible 'riding' AS so I can remove them once they no longer have a RootVehicle https://www.reddit.com/r/Minecraft/comments/54pghm/attempting_to_detect_passenger_mobs/
// UI: glowing shows which mobs have rare drop (very engaging); greater potion could also do AS with item above head, doesn't show if under tight roof... how know if potion wear off? (use Luck?)

// to get rid of AS after host dies, can just tag all with hosts like
//        pick one host                       find its rider
// /execute @e[type=spider,c=1] ~ ~1 ~ execute @e[type=armor_stand,c=1,dy=3] ~ ~ ~ say hi
// and then kill any without hosts

// something where if you look up at the sky, like fireworks-words appear or something? chest says look up, you do, particles spell something? all mobs look up? ...


//////////////////////////////////////


//lava-dipped arrows (remote light/lava)

//weapon with very long cooldown but very strong (e.g. one hit at start of battle?)

//diamond-laying chickens under attack by zombie (how get Z to attack C?) you need to save if want to farm; 
// - or i guess chicken could be villager who produces goods with egg laying sound... 
// - could be fun recurring set piece thruout a map, find Z chasing C, save C and get a finite-farmable reward; people like to farm, this is non-standard farming


// one-way-home teleport mechanic? something time consuming you can't use in battle? drink potion, gives you nausea->blindness over 5s, then tp home

