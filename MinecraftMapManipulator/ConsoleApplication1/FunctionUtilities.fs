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
    FunctionCompiler.DropInModule("findPhi",oneTimeInit,[||],funcs.ToArray())

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
    FunctionCompiler.DropInModule("findTheta",oneTimeInit,[||],funcs.ToArray())

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

// uses https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
let raycastProgram = 
    let testsnow   = BBN"testsnow"
    let raybegin   = BBN"raybegin"
    let rayskip    = BBN"rayskip"
    let rwhiletest = BBN"rwhiletest"
    let loopbody   = BBN"loopbody"
    let coda       = BBN"coda"

    let raycastVars = new Scope()
    // constants
    let R = raycastVars.RegisterVar("R")
    let ONE_THOUSAND = raycastVars.RegisterVar("ONE_THOUSAND")
    // variables
    let HOLDSNOW = raycastVars.RegisterVar("HOLDSNOW")
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

    Program(raycastVars,[|findTheta;findPhi|],[|
        yield AtomicCommand("kill @e[type=armor_stand,name=RAY]")
        // SB init
        for v in raycastVars.All() do
            yield AtomicCommand(sprintf "scoreboard objectives add %s dummy" v.Name)
        // constants
        yield SB(R .= 128)
        yield SB(ONE_THOUSAND .= 1000)
        // prep code
        yield AtomicCommand "scoreboard players tag @p add look"
        |],[||],testsnow,dict[
        testsnow,BasicBlock([|
            // test for if we're holding snowball
            SB(HOLDSNOW .= 0)
            AtomicCommand(sprintf """scoreboard players tag @p remove holdsnow""")
            AtomicCommand(sprintf """scoreboard players tag @p add holdsnow {SelectedItem:{id:"minecraft:snowball"}}""")
            AtomicCommand(sprintf """execute @p[tag=holdsnow] ~ ~ ~ scoreboard players set %s %s 1""" ENTITY_UUID HOLDSNOW.Name)
            // see if we need to summon the AS
            AtomicCommand(sprintf """scoreboard players tag @p add needsray""")
            AtomicCommand(sprintf """execute @e[type=armor_stand,name=RAY] ~ ~ ~ scoreboard players tag @p remove needsray""")
            // TODO would be more efficient to UUID RAY
            AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ execute @p[tag=needsray] ~ ~ ~ summon armor_stand ~ ~10 ~ {CustomName:RAY,NoGravity:1,Invisible:0,Glowing:1,Invulnerable:1}" HOLDSNOW.Name)
            AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ execute @p[tag=needsray] ~ ~ ~ summon armor_stand ~ ~10 ~ {CustomName:tempAS,NoGravity:1,Invisible:1,Glowing:0,Invulnerable:1}" HOLDSNOW.Name)
            |],ConditionalTailCall(Conditional[| HOLDSNOW .>= 1 |],raybegin,rayskip),MustNotYield)
        rayskip,BasicBlock([|
            AtomicCommand("kill @e[type=armor_stand,name=RAY]")
            AtomicCommand("kill @e[type=armor_stand,name=tempAS]")
            |],DirectTailCall(testsnow),MustWaitNTicks 1)
        raybegin,BasicBlock([|
            AtomicCommandWithExtraCost(sprintf "function %s:findTheta" FunctionCompiler.FUNCTION_NAMESPACE, 24)
            AtomicCommandWithExtraCost(sprintf "function %s:findPhi" FunctionCompiler.FUNCTION_NAMESPACE, 24)
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
            //let AX = TDX - MAJOR
            SB(AX .= TDX)
            SB(AX .-= MAJOR)
            //let AY = TDY - MAJOR
            SB(AY .= TDY)
            SB(AY .-= MAJOR)
            //let AZ = TDZ - MAJOR
            SB(AZ .= TDZ)
            SB(AZ .-= MAJOR)
            // put armor stand at right starting point
            AtomicCommand("tp @e[type=armor_stand,name=RAY] @p") // now RAY has my facing
            AtomicCommand(sprintf "tp @e[type=armor_stand,name=RAY] ~ ~%2f ~" (1.625 + float yOffset)) // RAY position (its feet) are at my eyeball level (+offset)
            |],DirectTailCall(rwhiletest),MustNotYield)
        rwhiletest,BasicBlock([|
            |],ConditionalTailCall(Conditional[| MAJOR .>= 1 |],loopbody,coda),MustNotYield)
        loopbody,BasicBlock([|
            // TODO could tease apart all 8 octants into separate functions, as an optimization
            // remember where we are, so can back up
            AtomicCommand "tp @e[type=armor_stand,name=tempAS] @e[type=armor_stand,name=RAY]"
            //if AX > 0 then
            //    if FLIPX then
            //        tp RAY ~-1 ~ ~
            //    else
            //        tp RAY ~1 ~ ~
            //    AX = AX - TMAJOR
            // AX = AX + 2DX
            AtomicCommand(sprintf "execute @s[score_%s_min=1,score_%s_min=1] ~ ~ ~ tp @e[type=armor_stand,name=RAY] ~-1 ~ ~" AX.Name FLIPX.Name)
            AtomicCommand(sprintf "execute @s[score_%s_min=1,score_%s=0] ~ ~ ~ tp @e[type=armor_stand,name=RAY] ~1 ~ ~" AX.Name FLIPX.Name)
            AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ %s" AX.Name (SB(AX .-= TMAJOR).AsCommand()))
            SB(AX .+= TDX)
            // ditto for y
            // TODO could I change all the '1'/'-1' I am TPing to M, where M could be e.g. 0.5 and get more collision-detection-precision (at extra computation cost to get same max radius)?
            // more than that, see http://www.cse.yorku.ca/~amana/research/grid.pdf
            // would be cool visual to highlight all blocks ray passes through, and have a second player look from another perspective to see it.
            // might be best to implement an integer version in F# first to make sure understand alg perfectly?
            AtomicCommand(sprintf "execute @s[score_%s_min=1,score_%s_min=1] ~ ~ ~ tp @e[type=armor_stand,name=RAY] ~ ~-1 ~" AY.Name FLIPY.Name)
            AtomicCommand(sprintf "execute @s[score_%s_min=1,score_%s=0] ~ ~ ~ tp @e[type=armor_stand,name=RAY] ~ ~1 ~" AY.Name FLIPY.Name)
            AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ %s" AY.Name (SB(AY .-= TMAJOR).AsCommand()))
            SB(AY .+= TDY)
            // ditto for z
            AtomicCommand(sprintf "execute @s[score_%s_min=1,score_%s_min=1] ~ ~ ~ tp @e[type=armor_stand,name=RAY] ~ ~ ~-1" AZ.Name FLIPZ.Name)
            AtomicCommand(sprintf "execute @s[score_%s_min=1,score_%s=0] ~ ~ ~ tp @e[type=armor_stand,name=RAY] ~ ~ ~1" AZ.Name FLIPZ.Name)
            AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ %s" AZ.Name (SB(AZ .-= TMAJOR).AsCommand()))
            SB(AZ .+= TDZ)
            // MAJOR = MAJOR - 1
            SB(MAJOR .-= 1)
            
            // detect non-air and exit loop early
            SB(TEMP .= 1)
            // for my snowball TP program, may make sense to 'look from eyes to feet' when looking downwards and 'look from feet to head' when looking upwards
            // line below has two E-Ds to check current block and block above, since player is 2-tall and we are at looking downwards to check for feet-collision
            AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ execute @e[type=armor_stand,name=RAY] ~ ~ ~ detect ~ ~%.2f ~ air 0 execute @e[type=armor_stand,name=RAY] ~ ~ ~ detect ~ ~%.2f ~ air 0 scoreboard players set %s %s 0" FLIPY.Name (-0.0 - float yOffset) (+1.0 - float yOffset) ENTITY_UUID TEMP.Name)
            // line below has two E-Ds to check current block and block below, since player is 2-tall and we are at looking up and checking for head-collision
            AtomicCommand(sprintf "execute @s[score_%s=0] ~ ~ ~ execute @e[type=armor_stand,name=RAY] ~ ~ ~ detect ~ ~%.2f ~ air 0 execute @e[type=armor_stand,name=RAY] ~ ~ ~ detect ~ ~%.2f ~ air 0 scoreboard players set %s %s 0" FLIPY.Name (-0.0 - float yOffset) (-1.0 - float yOffset) ENTITY_UUID TEMP.Name)
            // TODO should not detect 2 air in abstracted module, should be just bresenham one-block-thick ray...
            SB(ScoreboardPlayersConditionalSet(Conditional[|TEMP .>= 1|],MAJOR,0))
            // TEMP==1 means collision, TEMP==0 means still in air

            AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ execute @e[type=armor_stand,name=tempAS] ~ ~ ~ teleport @e[type=armor_stand,name=RAY] ~ ~ ~" TEMP.Name) // tp RAY back to tempAS if we collided, but preserve RAY's facing direction
            |],DirectTailCall(rwhiletest),MustNotYield)
        coda,BasicBlock([|
            AtomicCommand("""execute @e[type=armor_stand,name=RAY] ~ ~ ~ summon leash_knot ~ ~-500 ~ {Tags:["snapToGrid"]}""")   // -500 so as to not be visible to players
            AtomicCommand("""execute @e[type=leash_knot,tag=snapToGrid] ~ ~ ~ teleport @e[type=armor_stand,name=RAY] ~ ~ ~""")   // snap RAY to grid, but preserve facing
            AtomicCommand("""tp @e[type=armor_stand,name=RAY] ~ ~500 ~""")  // +500 to offset leash_knot visibility offset 
            AtomicCommand("""kill @e[type=leash_knot,tag=snapToGrid]""")
            AtomicCommand(sprintf "tp @e[type=armor_stand,name=RAY] ~ ~%.2f ~" (-0.5 - float yOffset))
            AtomicCommand(sprintf "execute @s[score_%s=0] ~ ~ ~ tp @e[type=armor_stand,name=RAY] ~ ~-1 ~" FLIPY.Name)  // if were looking up, then move down 1 because AS head is in collision now
            AtomicCommand("execute @e[type=snowball] ~ ~ ~ tp @p @e[type=armor_stand,name=RAY]")
            AtomicCommand("kill @e[type=snowball]")
            |],DirectTailCall(testsnow),MustWaitNTicks 1)
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
        //    Is a trade-off; place I do intend to call it next does not have 1-1-1-0-1 as the sender, so not a slam dunk... maybe always have 2 versions of utils?
        sprintf "execute %s ~ ~ ~ function %s:prngBody" ENTITY_UUID FUNCTION_NAMESPACE
        |]
    DropInModule("prng",oneTimeInit,[||],[|prngBody;prngMain|])

//////////////////////////////////////

let coordsScope = new Scope()
let coordsX = coordsScope.RegisterVar("coordsX")
let coordsZ = coordsScope.RegisterVar("coordsZ")
let coordsTemp = coordsScope.RegisterVar("coordsTemp")
// entity who is the stats'd executor
let COORDS_UUID = "1-1-1-0-8"
let COORDS_UUID_AS_FULL_GUID = "00000001-0001-0001-0000-000000000008"
let gcleast,gcmost = Utilities.toLeastMost(new System.Guid(COORDS_UUID_AS_FULL_GUID))
// TODO abstract the whole UUID'd armor stand thing, have a general registry
let getCoords =
    let oneTimeInit = [|
        // declare variables
        for v in coordsScope.All() do
            yield sprintf "scoreboard objectives add %s dummy" v.Name
        // place any long-lasting objects in the world
        // TODO what location is this entity? it needs to be safe in spawn chunks, but who knows where that is, hm, drop thru end portal?
        yield sprintf """summon armor_stand -5 4 -5 {CustomName:%s,NoGravity:1,UUIDMost:%dl,UUIDLeast:%dl,Invulnerable:1,Tags:["compiler"]}""" COORDS_UUID gcmost gcleast
        // TODO break init here into two ticks where kill any old version of this entity
        yield sprintf """stats entity @e[name=%s] set SuccessCount @e[name=%s] %s""" COORDS_UUID ENTITY_UUID coordsTemp.Name
        yield sprintf """scoreboard players set %s %s 1""" ENTITY_UUID coordsTemp.Name // stats'd entity must be initialized before can update
    |]
    let coords_body = "coords_body",([|
        yield SB(coordsX .= 30000000)
        let cur = ref 33554432
        while !cur > 0 do
            // tp AS ~cur ~ ~
            yield AtomicCommand(sprintf "execute %s ~ ~ ~ tp @e[type=armor_stand,tag=GC_AS] ~%d ~ ~" COORDS_UUID !cur)
            // if SuccessCount=1 then
            //    x = x-cur
            yield AtomicCommand(sprintf """execute @s[score_%s_min=1] ~ ~ ~ scoreboard players remove %s %s %d""" coordsTemp.Name ENTITY_UUID coordsX.Name !cur)
            // cur = cur / 2
            cur := !cur / 2

        yield SB(coordsZ .= 30000000)
        let cur = ref 33554432
        while !cur > 0 do
            // tp AS ~ ~ ~cur
            yield AtomicCommand(sprintf "execute %s ~ ~ ~ tp @e[type=armor_stand,tag=GC_AS] ~ ~ ~%d" COORDS_UUID !cur)
            // if SuccessCount=1 then
            //    z = z-cur
            yield AtomicCommand(sprintf """execute @s[score_%s_min=1] ~ ~ ~ scoreboard players remove %s %s %d""" coordsTemp.Name ENTITY_UUID coordsZ.Name !cur)
            // cur = cur / 2
            cur := !cur / 2

        yield AtomicCommand("kill @e[type=armor_stand,tag=GC_AS]")
        |]|>Array.map(fun c -> c.AsCommand()))
    let get_coords = "get_coords",[|
        "# get_coords"
        "# inputs: one entity is tagged with 'GetCoords'"
        "# outputs: coordsX, coordsZ"
        // summoned 0.5 less in x/z because it measures e.g. anything above 37 (like 37.1) as 38, want the rounded version
        sprintf """execute @e[tag=GetCoords,c=1] ~ ~ ~ summon armor_stand ~-0.5 ~ ~-0.5 {NoGravity:1b,Invulnerable:1b,Invisible:1b,Marker:1b,Tags:["GC_AS"]}"""
        sprintf "execute %s ~ ~ ~ function %s:coords_body" ENTITY_UUID FUNCTION_NAMESPACE
        |]
    DropInModule("get_coords",oneTimeInit,[||],[|coords_body;get_coords|])

//////////////////////////////////////

let makeUUIDas(digit1,digit2) =
    let UUID = sprintf "1-1-1-%c-%c" digit1 digit2 
    let UUID_AS_FULL_GUID = sprintf "00000001-0001-0001-000%c-00000000000%c" digit1 digit2 
    let least,most = Utilities.toLeastMost(new System.Guid(UUID_AS_FULL_GUID))
    let nbt = sprintf "CustomName:%s,UUIDMost:%dl,UUIDLeast:%dl" UUID most least
    UUID, nbt

let fracScope = new Scope()
let fracX = fracScope.RegisterVar("fracX")
let fracY = fracScope.RegisterVar("fracY")
let fracZ = fracScope.RegisterVar("fracZ")
let fracCoords =
    let FRAC_AS_UUID,FRAC_AS_NBT = makeUUIDas('2','0')
    let FRAC_GRID_UUID,FRAC_GRID_NBT = makeUUIDas('2','1')
    let oneTimeInit1 = [|
        // declare variables
        for v in fracScope.All() do
            yield sprintf "scoreboard objectives add %s dummy" v.Name
        // kill prior permanent entities
        yield sprintf "kill %s" FRAC_AS_UUID 
        yield sprintf "kill %s" FRAC_GRID_UUID 
    |]
    let oneTimeInit2 = [|
        // permanent entities
        yield sprintf """summon armor_stand ~ ~ ~ {Invulnerable:1b,NoGravity:1b,Invisible:1b,Marker:1b,%s}""" FRAC_AS_NBT 
        yield sprintf """summon armor_stand ~ ~ ~ {Invulnerable:1b,NoGravity:1b,Invisible:1b,Marker:1b,%s}""" FRAC_GRID_NBT 
    |]
    let frac_coords = "frac_coords",[|
        yield "# frac_coords"
        yield "# inputs: calling entity (@s) is who we'll get coords for"
        yield "# outputs: fracX, fracY, fracZ"

        yield """summon leash_knot ~ ~-500 ~ {Tags:["frac_lk"],Invulnerable:1b}"""
        yield sprintf """execute @e[tag=frac_lk] ~ ~ ~ teleport %s ~ ~500 ~""" FRAC_GRID_UUID 
        yield "kill @e[tag=frac_lk]"

        yield sprintf "scoreboard players set %s %s 0" FRAC_GRID_UUID fracX.Name 
        yield sprintf "scoreboard players set %s %s 0" FRAC_GRID_UUID fracY.Name 
        yield sprintf "scoreboard players set %s %s 0" FRAC_GRID_UUID fracZ.Name 

        yield sprintf "teleport %s ~ ~ ~" FRAC_AS_UUID  // reset to start

        for d in [1024; 512; 256; 128; 64; 32; 16; 8; 4; 2; 1] do
            let f = float d / 1000.0
            yield sprintf "scoreboard players tag %s add frac_toofar" FRAC_GRID_UUID 
            // TODO put next 3 lines in function with frac_as as @s caller
            yield sprintf "tp %s ~%.3f ~ ~" FRAC_AS_UUID f
            yield sprintf "execute %s ~ ~ ~ scoreboard players remove @e[name=%s,dx=1] %s %d" FRAC_AS_UUID FRAC_GRID_UUID fracX.Name d
            yield sprintf "execute %s ~ ~ ~ scoreboard players tag @e[name=%s,dx=1] remove frac_toofar" FRAC_AS_UUID FRAC_GRID_UUID 
            yield sprintf "execute %s ~ ~ ~ execute @s[tag=frac_toofar] ~ ~ ~ tp %s ~-%.3f ~ ~" FRAC_GRID_UUID FRAC_AS_UUID f
        yield sprintf "scoreboard players set %s %s 999" ENTITY_UUID fracX.Name // account for intersection overlap of AS and LK
        yield sprintf "scoreboard players operation %s %s += %s %s" ENTITY_UUID fracX.Name FRAC_GRID_UUID fracX.Name 

        yield sprintf "teleport %s ~ ~ ~" FRAC_AS_UUID  // reset to start

        for d in [1024; 512; 256; 128; 64; 32; 16; 8; 4; 2; 1] do
            let f = float d / 1000.0
            yield sprintf "scoreboard players tag %s add frac_toofar" FRAC_GRID_UUID
            // TODO put next 3 lines in function with frac_as as @s caller
            yield sprintf "tp %s ~ ~-%.3f ~" FRAC_AS_UUID f
            yield sprintf "execute %s ~ ~ ~ scoreboard players add @e[name=%s,dy=1] %s %d" FRAC_AS_UUID FRAC_GRID_UUID fracY.Name d
            yield sprintf "execute %s ~ ~ ~ scoreboard players tag @e[name=%s,dy=1] remove frac_toofar" FRAC_AS_UUID FRAC_GRID_UUID
            yield sprintf "execute %s ~ ~ ~ execute @s[tag=frac_toofar] ~ ~ ~ tp %s ~ ~%.3f ~" FRAC_GRID_UUID FRAC_AS_UUID f
        yield sprintf "scoreboard players set %s %s -1000" ENTITY_UUID fracY.Name // account for intersection overlap of AS and LK
        yield sprintf "scoreboard players operation %s %s += %s %s" ENTITY_UUID fracY.Name FRAC_GRID_UUID fracY.Name 

        yield sprintf "teleport %s ~ ~ ~" FRAC_AS_UUID  // reset to start

        for d in [1024; 512; 256; 128; 64; 32; 16; 8; 4; 2; 1] do
            let f = float d / 1000.0
            yield sprintf "scoreboard players tag %s add frac_toofar" FRAC_GRID_UUID 
            // TODO put next 3 lines in function with frac_as as @s caller
            yield sprintf "tp %s ~ ~ ~%.3f" FRAC_AS_UUID f
            yield sprintf "execute %s ~ ~ ~ scoreboard players remove @e[name=%s,dz=1] %s %d" FRAC_AS_UUID FRAC_GRID_UUID fracZ.Name d
            yield sprintf "execute %s ~ ~ ~ scoreboard players tag @e[name=%s,dz=1] remove frac_toofar" FRAC_AS_UUID FRAC_GRID_UUID
            yield sprintf "execute %s ~ ~ ~ execute @s[tag=frac_toofar] ~ ~ ~ tp %s ~ ~ ~-%.3f" FRAC_GRID_UUID FRAC_AS_UUID f
        yield sprintf "scoreboard players set %s %s 999" ENTITY_UUID fracZ.Name // account for intersection overlap of AS and LK
        yield sprintf "scoreboard players operation %s %s += %s %s" ENTITY_UUID fracZ.Name FRAC_GRID_UUID fracZ.Name 
        |]
    DropInModule("frac_coords",oneTimeInit1,oneTimeInit2,[|frac_coords|])

//////////////////////////////////////

let perfectRaycastProgram = 
    let ptestsnow   = BBN"ptestsnow"
    let praybegin   = BBN"praybegin"
    let prayskip    = BBN"prayskip"
    let prwhiletest = BBN"prwhiletest"
    let ploopbody   = BBN"ploopbody"
    let pylessx     = BBN"pylessx"
    let pxlessy     = BBN"pxlessy"
    let pxleast     = BBN"pxleast"
    let pyleast     = BBN"pyleast"
    let pzleast     = BBN"pzleast"
    let ploopfinish = BBN"ploopfinish"
    let pcoda       = BBN"pcoda"

    let praycastVars = new Scope()
    let ONE_THOUSAND = praycastVars.RegisterVar("ONE_THOUSAND")
    let HOLDSNOW = praycastVars.RegisterVar("HOLDSNOW")
    let RDX = praycastVars.RegisterVar("RDX")
    let RDY = praycastVars.RegisterVar("RDY")
    let RDZ = praycastVars.RegisterVar("RDZ")
    let TEMP = praycastVars.RegisterVar("TEMP")
    let FLIPX = praycastVars.RegisterVar("FLIPX")
    let FLIPY = praycastVars.RegisterVar("FLIPY")
    let FLIPZ = praycastVars.RegisterVar("FLIPZ")
    let DONE = praycastVars.RegisterVar("DONE")
    let CURX = praycastVars.RegisterVar("CURX")
    let CURY = praycastVars.RegisterVar("CURY")
    let CURZ = praycastVars.RegisterVar("CURZ")
    let FOUND_BELOW = praycastVars.RegisterVar("FOUND_BELOW")

    let yOffset = 5   // attempt to put all the armor stands not-in-my-face so that I can throw snowballs
    let eyesAboveFeet = 1.625  // how much higher camera (eyes) is than y coord showed on f3 screen as player coord (feet)
    let FRACY_OFFSET = int(eyesAboveFeet * 1000.0) - 1000
    let MAX = 64000   // travel at most 64 blocks

    // TODO show collision box
    let RAY_UUID,RAY_NBT = makeUUIDas('3','0')
    let TEMPAS_UUID,TEMPAS_NBT = makeUUIDas('3','1')
    // magma_cubes of Size:1 are a good size, but killing them splits them into Size:0 guys, and those guys have duplicate UUIDs, ruining everything
    // instead, use eight Size:0 guys to mark a cube
    let MAGMA_UUID,MAGMA_NBT = 
        let a = [| for i = '2' to '9' do yield makeUUIDas('3',i) |]
        Array.init 8 (fun i -> fst a.[i]), Array.init 8 (fun i -> snd a.[i])
    let KILL_MAGMAS = [|
        for i = 0 to 7 do
            yield AtomicCommand(sprintf "tp %s ~ -1000 ~" MAGMA_UUID.[i])
            yield AtomicCommand(sprintf "kill %s" MAGMA_UUID.[i])
            yield AtomicCommand(sprintf "entitydata %s {DeathTime:19}" MAGMA_UUID.[i]) // UUIDs will hang around during death animation, ruining everything, so skip it
        |]
    Program(praycastVars,[|findTheta;findPhi;fracCoords|],[|
        yield AtomicCommand(sprintf "kill %s" RAY_UUID)
        yield AtomicCommand(sprintf "kill %s" TEMPAS_UUID)
        yield! KILL_MAGMAS
        yield AtomicCommand("kill @e[type=armor_stand,name=looker]")
        // SB init
        for v in praycastVars.All() do
            yield AtomicCommand(sprintf "scoreboard objectives add %s dummy" v.Name)
        yield AtomicCommand(sprintf "scoreboard teams add RayTeam")
        yield AtomicCommand(sprintf "scoreboard teams option RayTeam collisionRule never")  // magma_cube should not collide with player
        // constants
        yield SB(ONE_THOUSAND .= 1000)
        |],[|
        // summoning permanent entities and other prep code
        for i = 0 to 7 do
            yield AtomicCommand(sprintf """summon magma_cube ~ ~ ~ {Team:RayTeam,Size:0,Silent:1,NoAI:1,DeathLootTable:"minecraft:empty",Glowing:0,Invulnerable:1,%s}""" MAGMA_NBT.[i])
            yield AtomicCommand(sprintf "effect %s invisibility 999999 1 true" MAGMA_UUID.[i])
            //yield AtomicCommand(sprintf "effect %s weakness 999999 5 true" HOLDSNOW.Name MAGMA_UUID.[i]) // sadly, they hurt and knock back survival players regardless
#if DEBUG_WITH_LOOKER
        yield AtomicCommand "scoreboard players tag @p remove look"
        yield AtomicCommand """summon armor_stand 0 0 0 {CustomName:looker,NoGravity:1,Invulnerable:1,ArmorItems:[{},{},{},{id:"minecraft:iron_helmet",Count:1b}],Tags:["look"]}"""
#else
        yield AtomicCommand "scoreboard players tag @p add look"
#endif
        |],ptestsnow,dict[
        ptestsnow,BasicBlock([|
            // test for if we're holding snowball
            yield SB(HOLDSNOW .= 0)
            yield AtomicCommand(sprintf """scoreboard players tag @p remove holdsnow""")
            yield AtomicCommand(sprintf """scoreboard players tag @p add holdsnow {SelectedItem:{id:"minecraft:snowball"}}""")
            yield AtomicCommand(sprintf """execute @p[tag=holdsnow] ~ ~ ~ scoreboard players set %s %s 1""" ENTITY_UUID HOLDSNOW.Name)
            // see if we need to summon the AS, if so HOLDSNOW becomes 2
            yield AtomicCommand(sprintf """scoreboard players add @s[score_%s_min=1] %s 1""" HOLDSNOW.Name HOLDSNOW.Name)
            yield AtomicCommand(sprintf """execute %s ~ ~ ~ scoreboard players remove %s %s 1""" RAY_UUID ENTITY_UUID HOLDSNOW.Name)
            yield AtomicCommand(sprintf "execute @s[score_%s_min=2] ~ ~ ~ execute @p[tag=holdsnow] ~ ~ ~ summon armor_stand ~ ~10 ~ {Team:RayTeam,NoGravity:1,Invisible:0,Glowing:1,Invulnerable:1,%s}" HOLDSNOW.Name RAY_NBT)
            yield AtomicCommand(sprintf "execute @s[score_%s_min=2] ~ ~ ~ execute @p[tag=holdsnow] ~ ~ ~ summon armor_stand ~ ~10 ~ {NoGravity:1,Invisible:1,Glowing:0,Invulnerable:1,%s}" HOLDSNOW.Name TEMPAS_NBT)
            for i = 0 to 7 do
                yield AtomicCommand(sprintf """execute @s[score_%s_min=2] ~ ~ ~ entitydata %s {Glowing:1b}""" HOLDSNOW.Name MAGMA_UUID.[i])
            |],ConditionalTailCall(Conditional[| HOLDSNOW .>= 1 |],praybegin,prayskip),MustNotYield)
        prayskip,BasicBlock([|
            yield AtomicCommand(sprintf "kill %s" RAY_UUID)
            yield AtomicCommand(sprintf "kill %s" TEMPAS_UUID)
            for i = 0 to 7 do
                yield AtomicCommand(sprintf """entitydata %s {Glowing:0b}""" MAGMA_UUID.[i])
                yield AtomicCommand(sprintf """tp %s %s""" MAGMA_UUID.[i] ENTITY_UUID)
            |],DirectTailCall(ptestsnow),MustWaitNTicks 1)
        praybegin,BasicBlock([|
            AtomicCommandWithExtraCost(sprintf "function %s:findTheta" FunctionCompiler.FUNCTION_NAMESPACE, 24)
            AtomicCommandWithExtraCost(sprintf "function %s:findPhi" FunctionCompiler.FUNCTION_NAMESPACE, 24)
            AtomicCommandWithExtraCost(sprintf "execute @e[tag=look] ~ ~ ~ function %s:frac_coords" FunctionCompiler.FUNCTION_NAMESPACE, 180)

            // fracY is location of feet, but we need location of eyes, so fix it
            SB(fracY .+= FRACY_OFFSET)  // add offset
            AtomicCommand(sprintf "scoreboard players remove @s[score_%s_min=1000] %s 1000" fracY.Name fracY.Name)  // get it back in 000-999 range

            // RDX: radial distance needed to travel in order to cross 1.0 distance along x-axis
            SB(TEMP .= vKcosTheta)
            SB(TEMP .*= vKsinPhi)
            SB(RDX .= -1000000000)
            SB(RDX ./= TEMP)
            // if TEMP was 0, division fails, need to clamp RDX at a value bigger than next degree, but small enough that multiplying by 1000 does not overflow
            AtomicCommand(sprintf "scoreboard players set @s[score_%s_min=0,score_%s=0] %s 1000000" TEMP.Name TEMP.Name RDX.Name) 
            // now RDX of 1000 means 1.0, 2437 means 2.437, etc.

            // RDY: radial distance needed to travel in order to cross 1.0 distance along y-axis
            SB(RDY .= -1000000)
            SB(RDY ./= vKsinTheta)
            // if vKsinTheta was 0, division fails, need to clamp RDY at a value bigger than next degree, but small enough that multiplying by 1000 does not overflow
            AtomicCommand(sprintf "scoreboard players set @s[score_%s_min=0,score_%s=0] %s 1000000" vKsinTheta.Name vKsinTheta.Name RDY.Name) 

            // RDZ: radial distance needed to travel in order to cross 1.0 distance along z-axis
            SB(TEMP .= vKcosTheta)
            SB(TEMP .*= vKcosPhi)
            SB(RDZ .= 1000000000)
            SB(RDZ ./= TEMP)
            // if TEMP was 0, division fails, need to clamp RDZ at a value bigger than next degree, but small enough that multiplying by 1000 does not overflow
            AtomicCommand(sprintf "scoreboard players set @s[score_%s_min=0,score_%s=0] %s 1000000" TEMP.Name TEMP.Name RDZ.Name) 

            // all RD_ vars need to be positive, flip if needed and track what we flipped
            SB(FLIPX .= 0)
            SB(FLIPY .= 0)
            SB(FLIPZ .= 0)
            //if RDX < 0 then
            //    RDX = -RDX
            //    FLIPX = true
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" RDX.Name (SB(TEMP .= 0).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" RDX.Name (SB(TEMP .-= RDX).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" RDX.Name (SB(FLIPX .= 1).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" RDX.Name (SB(RDX .= TEMP).AsCommand()))
            //if RDY < 0 then
            //    RDY = -RDY
            //    FLIPY = true
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" RDY.Name (SB(TEMP .= 0).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" RDY.Name (SB(TEMP .-= RDY).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" RDY.Name (SB(FLIPY .= 1).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" RDY.Name (SB(RDY .= TEMP).AsCommand()))
            //if RDZ < 0 then
            //    RDZ = -RDZ
            //    FLIPZ = true
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" RDZ.Name (SB(TEMP .= 0).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" RDZ.Name (SB(TEMP .-= RDZ).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" RDZ.Name (SB(FLIPZ .= 1).AsCommand()))
            AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ %s" RDZ.Name (SB(RDZ .= TEMP).AsCommand()))

            // put armor stand at right starting point
            AtomicCommand(sprintf "execute @e[tag=look] ~ ~ ~ teleport %s ~ ~%2f ~ ~ ~" RAY_UUID (eyesAboveFeet + float yOffset)) // RAY position (its feet) are at looker eyeball level (+offset)

            // initial values
            //if FLIPX
            //    CURX .= fracX
            //    CURX .*= RDX
            //    CURX ./= ONE_THOUSAND
            //else
            //    CURX .= ONE_THOUSAND
            //    CURX .-= fracX
            //    CURX .*= RDX
            //    CURX ./= ONE_THOUSAND
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s_min=1] %s = @s %s" FLIPX.Name CURX.Name fracX.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s_min=1] %s *= @s %s" FLIPX.Name CURX.Name RDX.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s_min=1] %s /= @s %s" FLIPX.Name CURX.Name ONE_THOUSAND.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s=0] %s = @s %s" FLIPX.Name CURX.Name ONE_THOUSAND.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s=0] %s -= @s %s" FLIPX.Name CURX.Name fracX.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s=0] %s *= @s %s" FLIPX.Name CURX.Name RDX.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s=0] %s /= @s %s" FLIPX.Name CURX.Name ONE_THOUSAND.Name)
            //same for y
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s_min=1] %s = @s %s" FLIPY.Name CURY.Name fracY.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s_min=1] %s *= @s %s" FLIPY.Name CURY.Name RDY.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s_min=1] %s /= @s %s" FLIPY.Name CURY.Name ONE_THOUSAND.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s=0] %s = @s %s" FLIPY.Name CURY.Name ONE_THOUSAND.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s=0] %s -= @s %s" FLIPY.Name CURY.Name fracY.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s=0] %s *= @s %s" FLIPY.Name CURY.Name RDY.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s=0] %s /= @s %s" FLIPY.Name CURY.Name ONE_THOUSAND.Name)
            //same for z
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s_min=1] %s = @s %s" FLIPZ.Name CURZ.Name fracZ.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s_min=1] %s *= @s %s" FLIPZ.Name CURZ.Name RDZ.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s_min=1] %s /= @s %s" FLIPZ.Name CURZ.Name ONE_THOUSAND.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s=0] %s = @s %s" FLIPZ.Name CURZ.Name ONE_THOUSAND.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s=0] %s -= @s %s" FLIPZ.Name CURZ.Name fracZ.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s=0] %s *= @s %s" FLIPZ.Name CURZ.Name RDZ.Name)
            AtomicCommand(sprintf "scoreboard players operation @s[score_%s=0] %s /= @s %s" FLIPZ.Name CURZ.Name ONE_THOUSAND.Name)
            // loop prep
            SB(DONE .= 0)
//            AtomicCommand(sprintf """tellraw @a ["init - CURX:",{"score":{"name":"@e[name=%s]","objective":"%s"}}," CURY:",{"score":{"name":"@e[name=%s]","objective":"%s"}}," CURZ:",{"score":{"name":"@e[name=%s]","objective":"%s"}}]""" ENTITY_UUID CURX.Name ENTITY_UUID CURY.Name ENTITY_UUID CURZ.Name)
            |],DirectTailCall(prwhiletest),MustNotYield)
        prwhiletest,BasicBlock([|
            |],ConditionalTailCall(Conditional[| DONE .<= 0 |],ploopbody,pcoda),MustNotYield)
        ploopbody,BasicBlock([|
            // remember where we are, so can back up
            AtomicCommand(sprintf "tp %s %s" TEMPAS_UUID RAY_UUID)
            // find the smallest, step in its direction, increment, test for done
            SB(TEMP .= CURX)
            SB(TEMP .-= CURY)
            |],ConditionalTailCall(Conditional[| TEMP .<= 0 |],pxlessy,pylessx),MustNotYield)
        pylessx,BasicBlock([|
            SB(TEMP .= CURZ)
            SB(TEMP .-= CURY)
            |],ConditionalTailCall(Conditional[| TEMP .<= 0 |],pzleast,pyleast),MustNotYield)
        pzleast,BasicBlock([|
            // z was least
            // step 1 in z direction (using FLIPZ to determine + or -)
            AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ tp %s ~ ~ ~-1" FLIPZ.Name RAY_UUID)
            AtomicCommand(sprintf "execute @s[score_%s=0] ~ ~ ~ tp %s ~ ~ ~1" FLIPZ.Name RAY_UUID)
            // see if air here, if not stop
            SB(DONE .= 1)
            AtomicCommand(sprintf "execute %s ~ ~ ~ detect ~ ~-%d ~ air 0 scoreboard players set %s %s 0" RAY_UUID yOffset ENTITY_UUID DONE.Name)
            // see if traveled far enough to stop
            AtomicCommand(sprintf "scoreboard players set @s[score_%s_min=%d] %s 1" CURZ.Name MAX DONE.Name)
//            AtomicCommand(sprintf """tellraw @a ["step z - CURX:",{"score":{"name":"@e[name=%s]","objective":"%s"}}," CURY:",{"score":{"name":"@e[name=%s]","objective":"%s"}}," CURZ:",{"score":{"name":"@e[name=%s]","objective":"%s"}}]""" ENTITY_UUID CURX.Name ENTITY_UUID CURY.Name ENTITY_UUID CURZ.Name)
            SB(CURZ .+= RDZ)
            |],DirectTailCall(ploopfinish),MustNotYield)
        pyleast,BasicBlock([|
            // y was least
            // step 1 in y direction (using FLIPY to determine + or -)
            AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ tp %s ~ ~-1 ~" FLIPY.Name RAY_UUID)
            AtomicCommand(sprintf "execute @s[score_%s=0] ~ ~ ~ tp %s ~ ~1 ~" FLIPY.Name RAY_UUID)
            // see if air here, if not stop
            SB(DONE .= 1)
            AtomicCommand(sprintf "execute %s ~ ~ ~ detect ~ ~-%d ~ air 0 scoreboard players set %s %s 0" RAY_UUID yOffset ENTITY_UUID DONE.Name)
            // see if traveled far enough to stop
            AtomicCommand(sprintf "scoreboard players set @s[score_%s_min=%d] %s 1" CURY.Name MAX DONE.Name)
//            AtomicCommand(sprintf """tellraw @a ["step y - CURX:",{"score":{"name":"@e[name=%s]","objective":"%s"}}," CURY:",{"score":{"name":"@e[name=%s]","objective":"%s"}}," CURZ:",{"score":{"name":"@e[name=%s]","objective":"%s"}}]""" ENTITY_UUID CURX.Name ENTITY_UUID CURY.Name ENTITY_UUID CURZ.Name)
            SB(CURY .+= RDY)
            |],DirectTailCall(ploopfinish),MustNotYield)
        pxlessy,BasicBlock([|
            SB(TEMP .= CURZ)
            SB(TEMP .-= CURX)
            |],ConditionalTailCall(Conditional[| TEMP .<= 0 |],pzleast,pxleast),MustNotYield)
        pxleast,BasicBlock([|
            // x was least
            // step 1 in x direction (using FLIPX to determine + or -)
            AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ tp %s ~-1 ~ ~" FLIPX.Name RAY_UUID)
            AtomicCommand(sprintf "execute @s[score_%s=0] ~ ~ ~ tp %s ~1 ~ ~" FLIPX.Name RAY_UUID)
            // see if air here, if not stop
            SB(DONE .= 1)
            AtomicCommand(sprintf "execute %s ~ ~ ~ detect ~ ~-%d ~ air 0 scoreboard players set %s %s 0" RAY_UUID yOffset ENTITY_UUID DONE.Name)
            // see if traveled far enough to stop
            AtomicCommand(sprintf "scoreboard players set @s[score_%s_min=%d] %s 1" CURX.Name MAX DONE.Name)
//            AtomicCommand(sprintf """tellraw @a ["step x - CURX:",{"score":{"name":"@e[name=%s]","objective":"%s"}}," CURY:",{"score":{"name":"@e[name=%s]","objective":"%s"}}," CURZ:",{"score":{"name":"@e[name=%s]","objective":"%s"}}]""" ENTITY_UUID CURX.Name ENTITY_UUID CURY.Name ENTITY_UUID CURZ.Name)
            SB(CURX .+= RDX)
            |],DirectTailCall(ploopfinish),MustNotYield)
        ploopfinish,BasicBlock([|
#if DEBUG_WITH_LOOKER
            AtomicCommand(sprintf "execute %s ~ ~-%d ~ particle happyVillager ~ ~ ~ 0 0 0 0 0" RAY_UUID yOffset)
#endif
//            AtomicCommand(sprintf """tellraw @a ["loop - CURX:",{"score":{"name":"@e[name=%s]","objective":"%s"}}," CURY:",{"score":{"name":"@e[name=%s]","objective":"%s"}}," CURZ:",{"score":{"name":"@e[name=%s]","objective":"%s"}}]""" ENTITY_UUID CURX.Name ENTITY_UUID CURY.Name ENTITY_UUID CURZ.Name)
            |],DirectTailCall(prwhiletest),MustNotYield)
        pcoda,BasicBlock([|
//            AtomicCommand(sprintf """tellraw @a ["done!"]""")
            yield AtomicCommand(sprintf "execute %s ~ ~ ~ teleport %s ~ ~ ~" TEMPAS_UUID RAY_UUID) // tp RAY back to tempAS, but preserve RAY's facing direction
            yield AtomicCommand(sprintf """execute %s ~ ~ ~ summon leash_knot ~ ~-500 ~ {Tags:["snapToGrid"]}""" RAY_UUID)   // -500 so as to not be visible to players
            yield AtomicCommand(sprintf """execute @e[type=leash_knot,tag=snapToGrid] ~ ~ ~ teleport %s ~ ~ ~""" RAY_UUID)   // snap RAY to grid, but preserve facing
            yield AtomicCommand(sprintf """tp %s ~ ~500 ~""" RAY_UUID)  // +500 to offset leash_knot visibility offset 
            yield AtomicCommand(sprintf """kill @e[type=leash_knot,tag=snapToGrid]""")
            yield AtomicCommand(sprintf "tp %s ~ ~%.2f ~" RAY_UUID (-0.5 - float yOffset))
            // red/green logic: 
            yield SB(TEMP .= 0) // TEMP==0 means here RED, TEMP==1 means here GREEN
            yield SB(FOUND_BELOW .= 0) // like TEMP but for spot one below RAY
            // if was looking down, first check square above for air...
            yield AtomicCommand(sprintf "execute @s[score_%s_min=0] ~ ~ ~ execute %s ~ ~ ~ detect ~ ~1 ~ air 0 scoreboard players set %s %s 1" vtheta.Name RAY_UUID ENTITY_UUID TEMP.Name)
            // ...else check square below for air
            yield AtomicCommand(sprintf "execute @s[score_%s=0,score_%s_min=0] ~ ~ ~ execute %s ~ ~ ~ detect ~ ~-1 ~ air 0 scoreboard players set %s %s 1" TEMP.Name vtheta.Name RAY_UUID ENTITY_UUID FOUND_BELOW.Name)
            // if was looking up, first check square above for air...
            yield AtomicCommand(sprintf "execute @s[score_%s=-1] ~ ~ ~ execute %s ~ ~ ~ detect ~ ~1 ~ air 0 scoreboard players set %s %s 1" vtheta.Name RAY_UUID ENTITY_UUID TEMP.Name)
            // ...else check square below for air
            yield AtomicCommand(sprintf "execute @s[score_%s=0,score_%s=-1] ~ ~ ~ execute %s ~ ~ ~ detect ~ ~-1 ~ air 0 scoreboard players set %s %s 1" TEMP.Name vtheta.Name RAY_UUID ENTITY_UUID FOUND_BELOW.Name)
            // Now, either TEMP is 1 (RAY is in right spot, GREEN), or FOUND_BELOW is 1 (RAY is one above right spot, GREEN), or neither (RED)
            // display a highlighted cube where the player cast the ray
            for i = 0 to 7 do
                yield AtomicCommand(sprintf "tp %s %s" MAGMA_UUID.[i] RAY_UUID)
            yield AtomicCommand(sprintf "tp %s ~-0.25 ~ ~-0.25 0 0" MAGMA_UUID.[0])
            yield AtomicCommand(sprintf "tp %s ~+0.25 ~ ~-0.25 0 0" MAGMA_UUID.[1])
            yield AtomicCommand(sprintf "tp %s ~-0.25 ~ ~+0.25 0 0" MAGMA_UUID.[2])
            yield AtomicCommand(sprintf "tp %s ~+0.25 ~ ~+0.25 0 0" MAGMA_UUID.[3])
            yield AtomicCommand(sprintf "tp %s ~-0.25 ~+0.5 ~-0.25 0 0" MAGMA_UUID.[4])
            yield AtomicCommand(sprintf "tp %s ~+0.25 ~+0.5 ~-0.25 0 0" MAGMA_UUID.[5])
            yield AtomicCommand(sprintf "tp %s ~-0.25 ~+0.5 ~+0.25 0 0" MAGMA_UUID.[6])
            yield AtomicCommand(sprintf "tp %s ~+0.25 ~+0.5 ~+0.25 0 0" MAGMA_UUID.[7])
            // set up colors and move armor stand if needed
            yield AtomicCommand("scoreboard teams option RayTeam color Red")
            yield AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ tp %s ~ ~-1 ~" FOUND_BELOW.Name RAY_UUID)
            yield AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ scoreboard players set @s %s 1" FOUND_BELOW.Name TEMP.Name)
            yield AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ scoreboard teams option RayTeam color Green" TEMP.Name)
            // do teleport if GREEN
            yield AtomicCommand(sprintf "execute @s[score_%s_min=1] ~ ~ ~ execute @e[type=snowball] ~ ~ ~ tp @p %s" TEMP.Name RAY_UUID)
            yield AtomicCommand("kill @e[type=snowball]")
            |],DirectTailCall(ptestsnow),MustWaitNTicks 1)
        ])

//////////////////////////////////////

// conway life

// one level cmd blocks
let conwayLife = 
    let oneTimeInit = [|
        "scoreboard objectives add A dummy"  // 2-state iteration
        "scoreboard objectives add N dummy"  // neightbor count
        "scoreboard objectives add R dummy"  // is it running?
        "gamerule maxCommandChainLength 999999"
        "summon armor_stand ~ 0 ~ {Invisible:1b,NoGravity:1b}"
        "scoreboard players set @p A 1"
        "scoreboard players set @e[type=armor_stand] A 1"
        |]

    let count_neighbors = "count_neighbors",[|
        "scoreboard players set @s N 0"
        "execute @s ~ ~ ~ detect ~-1 4 ~-1 wool 15 scoreboard players add @s N 1"
        "execute @s ~ ~ ~ detect ~-1 4 ~-0 wool 15 scoreboard players add @s N 1"
        "execute @s ~ ~ ~ detect ~-1 4 ~+1 wool 15 scoreboard players add @s N 1"
        "execute @s ~ ~ ~ detect ~-0 4 ~-1 wool 15 scoreboard players add @s N 1"
        "execute @s ~ ~ ~ detect ~-0 4 ~+1 wool 15 scoreboard players add @s N 1"
        "execute @s ~ ~ ~ detect ~+1 4 ~-1 wool 15 scoreboard players add @s N 1"
        "execute @s ~ ~ ~ detect ~+1 4 ~-0 wool 15 scoreboard players add @s N 1"
        "execute @s ~ ~ ~ detect ~+1 4 ~+1 wool 15 scoreboard players add @s N 1"
        |]
    let has_buffer_neighbor = "has_buffer_neighbor",[|
        "scoreboard players set @s N 0"
        "execute @s ~ ~ ~ detect ~-1 3 ~-1 wool 15 scoreboard players add @s N 1"
        "execute @s[score_N=0] ~ ~ ~ detect ~-1 3 ~-0 wool 15 scoreboard players add @s N 1"
        "execute @s[score_N=0] ~ ~ ~ detect ~-1 3 ~+1 wool 15 scoreboard players add @s N 1"
        "execute @s[score_N=0] ~ ~ ~ detect ~-0 3 ~-1 wool 15 scoreboard players add @s N 1"
        "execute @s[score_N=0] ~ ~ ~ detect ~-0 3 ~-0 wool 15 scoreboard players add @s N 1"
        "execute @s[score_N=0] ~ ~ ~ detect ~-0 3 ~+1 wool 15 scoreboard players add @s N 1"
        "execute @s[score_N=0] ~ ~ ~ detect ~+1 3 ~-1 wool 15 scoreboard players add @s N 1"
        "execute @s[score_N=0] ~ ~ ~ detect ~+1 3 ~-0 wool 15 scoreboard players add @s N 1"
        "execute @s[score_N=0] ~ ~ ~ detect ~+1 3 ~+1 wool 15 scoreboard players add @s N 1"
        |]
    // note that the 2-cycles is
    // @p A = 0 -> run all the check1, compute buffer, do no scheduling
    // @p A = 1 -> double-buffer the check1 results, schedule blocks around all (buffer) blacks, delete all check1 command blocks that are (buffer) white and have no (buffer) black neighbors
    let check1 = "check1",[|
        "teleport @e[type=armor_stand,score_R_min=1] ~ ~ ~"
        "execute @e[type=armor_stand,score_A=0,score_A_min=0,score_R_min=1] ~ ~ ~ function conway:check1body"
        "execute @e[type=armor_stand,score_A=1,score_A_min=1,score_R_min=1] ~ ~ ~ function conway:check1part2"
        |]
    let check1body = "check1body",[|
        "function conway:count_neighbors"
        "clone ~ 4 ~ ~ 4 ~ ~ 3 ~"  // assume nothing changes to start
        "execute @s[score_N=1] ~ ~ ~ detect ~ 4 ~ wool 15 setblock ~ 3 ~ wool 0"
        "execute @s[score_N_min=4] ~ ~ ~ detect ~ 4 ~ wool 15 setblock ~ 3 ~ wool 0"
        "execute @s[score_N_min=3,score_N=3] ~ ~ ~ detect ~ 4 ~ wool 0 setblock ~ 3 ~ wool 15"
        |]
    let check1part2 = "check1part2",[|
        // double-buffer this cell
        "clone ~ 3 ~ ~ 3 ~ ~ 4 ~"
        // schedule blocks for next tick if (buffer) me is alive
        """execute @s ~ ~ ~ detect ~ 3 ~ wool 15 fill ~-1 1 ~-1 ~1 1 ~1 repeating_command_block 0 replace {auto:1b,Command:"function conway:check1"}"""
        // delete command block if (buffer) me is dead, and so are all my neighbors
        "function conway:has_buffer_neighbor"
        "execute @s[score_N=0] ~ ~ ~ setblock ~ 1 ~ air"
        |]
    let life = "life",[|
        "execute @e[type=armor_stand,score_A_min=1] ~ ~ ~ scoreboard players set @p[type=armor_stand] R 0"
        """execute @e[type=armor_stand,score_A_min=1] ~ ~ ~ scoreboard players add @p R 1 {SelectedItem:{id:"minecraft:redstone_block"}}"""
        "scoreboard players operation @e[type=armor_stand] R = @p R"
        "scoreboard players add @e[type=armor_stand,score_R_min=1] A 1"
        "scoreboard players set @e[type=armor_stand,score_R_min=1,score_A_min=2] A 0"
        """execute @e[type=bat] ~ ~ ~ fill ~-1 1 ~-1 ~1 1 ~1 repeating_command_block 0 replace {auto:1b,Command:"function conway:check1"}"""
        // todo don't run so much in the 'always' loop, when this is really only 'setup' stuff - modal?
        "execute @e[type=bat] ~ ~ ~ setblock ~ 4 ~ wool 15"
        "tp @e[type=bat] ~ ~-200 ~"
        "execute @e[type=skeleton] ~ ~ ~ setblock ~ 4 ~ wool 0"
        "tp @e[type=skeleton] ~ ~-200 ~"
        // todo add methusalah/glider eggs?
        |]
    DropInModule("life",oneTimeInit,[||],[|
        count_neighbors
        has_buffer_neighbor
        check1
        check1body
        check1part2
        life
        |])

//////////////////////////////////////

let potionOfForesight =
    let least,most = Utilities.toLeastMost(new System.Guid(ENTITY_UUID_AS_FULL_GUID))
    let oneTimeInit = [|
        // prng too
        "scoreboard objectives add nPotionActive dummy"  // n=next
        "scoreboard objectives add potionActive dummy"
        "scoreboard objectives add riderHasHost dummy"
        "scoreboard objectives add nextNum dummy"
        sprintf "summon armor_stand -3 4 -3 {CustomName:%s,NoGravity:1,UUIDMost:%dl,UUIDLeast:%dl,Invulnerable:1}" ENTITY_UUID most least  // TODO lots of UUID fragility
        // TODO call prng_init, so only need one init call
        // TODO abstract notion of dependency to deal with this
        // TODO abstract notion of init also then installing gameLoopFunction
        |]
    let foresight_loop = "foresight_loop",[|
        "execute @p[score_nPotionActive_min=1,score_potionActive=0] ~ ~ ~ function lorgon111:turn_on"
        "execute @p[score_nPotionActive=0,score_potionActive_min=1] ~ ~ ~ function lorgon111:turn_off"
        "scoreboard players operation @p potionActive = @p nPotionActive"
        "scoreboard players remove @p nPotionActive 1"   // was init'd to numTicks, counts down here
        
        "function lorgon111:process_zombies"
        "function lorgon111:process_spiders"
        // same for spiders, etc
        
        "execute @e[tag=rider] ~ ~ ~ function lorgon111:run_rider"
        |]
    let process_zombies = "process_zombies",[|
        sprintf "scoreboard players set %s prng_Mod 100" FunctionCompiler.ENTITY_UUID 
        "execute @e[type=zombie,tag=!processed] ~ ~ ~ function lorgon111:process_zombie"
        |]
    let process_zombie = "process_zombie",[|
        "scoreboard players tag @s add processed"
        "function lorgon111:prng" // TODO namepsace
        sprintf "scoreboard players operation @s prng_K = %s prng_K" FunctionCompiler.ENTITY_UUID 
        
        // 95/100 zombie -> 0
        "scoreboard players tag @s[score_prng_K=94] add noRareLoot"
        // 4/100 zombie -> apple
        "scoreboard players tag @s[score_prng_K_min=95] add hasRareLoot"
        "scoreboard players tag @s[score_prng_K_min=95,score_prng_K=98] add hasApple"
        """entitydata @s[score_prng_K_min=95,score_prng_K=98] {DeathLootTable:"lorgon111:zombie_with_apple"}"""
        // 1/100 zombie -> diamond
        "scoreboard players tag @s[score_prng_K_min=99] add hasDiamond"
        """entitydata @s[score_prng_K_min=99] {DeathLootTable:"lorgon111:zombie_with_diamond"}"""
        
        "scoreboard players operation @s potionActive = @p potionActive"
        "execute @s[score_potionActive_min=1] ~ ~ ~ function lorgon111:turn_on_mob"
        |]
    let process_spiders = "process_spiders",[|
        sprintf "scoreboard players set %s prng_Mod 100" FunctionCompiler.ENTITY_UUID 
        "execute @e[type=spider,tag=!processed] ~ ~ ~ function lorgon111:process_spider"
        |]
    let process_spider = "process_spider",[|
        "scoreboard players tag @s add processed"
        "function lorgon111:prng" // TODO namepsace
        sprintf "scoreboard players operation @s prng_K = %s prng_K" FunctionCompiler.ENTITY_UUID 
        
        // 95/100 spider -> 0
        "scoreboard players tag @s[score_prng_K=94] add noRareLoot"
        // 4/100 spider -> flint
        "scoreboard players tag @s[score_prng_K_min=95] add hasRareLoot"
        "scoreboard players tag @s[score_prng_K_min=95,score_prng_K=98] add hasFlint"
        """entitydata @s[score_prng_K_min=95,score_prng_K=98] {DeathLootTable:"lorgon111:spider_with_flint"}"""
        // 1/100 spider -> cake
        "scoreboard players tag @s[score_prng_K_min=99] add hasCake"
        """entitydata @s[score_prng_K_min=99] {DeathLootTable:"lorgon111:spider_with_cake"}"""
        
        "scoreboard players operation @s potionActive = @p potionActive"
        "execute @s[score_potionActive_min=1] ~ ~ ~ function lorgon111:turn_on_mob"
        |]
    let run_rider = "run_rider",[|
        // @s is a rider AS, we need to tp it to its entity, or kill if its host is gone
        sprintf "scoreboard players set %s riderHasHost 0" FunctionCompiler.ENTITY_UUID
        "scoreboard players tag @s add curRider"

        // using r=3, since mob may have moved last tick (assumes mob not move more than 60 blocks per second)
        "scoreboard players operation @e[r=3,tag=!rider] nextNum -= @s nextNum"
        "execute @e[r=3,tag=!rider,score_nextNum_min=0,score_nextNum=0] ~ ~ ~ teleport @e[r=3,tag=curRider] ~ ~ ~"
        sprintf "execute @e[r=3,tag=!rider,score_nextNum_min=0,score_nextNum=0] ~ ~ ~ scoreboard players set %s riderHasHost 1" FunctionCompiler.ENTITY_UUID 
        "scoreboard players operation @e[r=3,tag=!rider] nextNum += @s nextNum"

        "scoreboard players tag @s remove curRider"
        sprintf "scoreboard players operation @s riderHasHost = %s riderHasHost" FunctionCompiler.ENTITY_UUID 
        "kill @s[score_riderHasHost=0]"
        |]
    let turn_on = "turn_on",[|
        """tellraw @a["foresight turned on"]"""
        "execute @e[tag=processed] ~ ~ ~ function lorgon111:turn_on_mob"
        |]
    let turn_on_mob = "turn_on_mob",[|
        "execute @s[tag=hasRareLoot] ~ ~ ~ function lorgon111:turn_on_rare_mob"
        """entitydata @s[tag=noRareLoot] {CustomName:"0",CustomNameVisible:1b}"""
        |]
    let turn_on_rare_mob = "turn_on_rare_mob",[|
        """summon armor_stand ~ ~ ~ {Small:0b,NoGravity:1b,Invisible:1b,Invulnerable:1b,Marker:1b,Tags:["rider","newAS"]}"""
        sprintf "scoreboard players add %s nextNum 1" FunctionCompiler.ENTITY_UUID
        sprintf "scoreboard players operation @s nextNum = %s nextNum" FunctionCompiler.ENTITY_UUID
        sprintf "scoreboard players operation @e[r=1,tag=newAS] nextNum = %s nextNum" FunctionCompiler.ENTITY_UUID
        "entitydata @s[tag=hasRareLoot] {Glowing:1b}"
        "execute @s[tag=hasDiamond] ~ ~ ~ entitydata @e[r=1,type=armor_stand,tag=newAS] {ArmorItems:[{},{},{},{id:diamond,Count:1b}]}"
        "execute @s[tag=hasApple] ~ ~ ~ entitydata @e[r=1,type=armor_stand,tag=newAS] {ArmorItems:[{},{},{},{id:apple,Count:1b}]}"
        "execute @s[tag=hasFlint] ~ ~ ~ entitydata @e[r=1,type=armor_stand,tag=newAS] {ArmorItems:[{},{},{},{id:flint,Count:1b}]}"
        "execute @s[tag=hasCake] ~ ~ ~ entitydata @e[r=1,type=armor_stand,tag=newAS] {ArmorItems:[{},{},{},{id:cake,Count:1b}]}"
        "scoreboard players tag @e[r=1,tag=newAS] remove newAS"
        |]
    let turn_off = "turn_off",[|
        """tellraw @a["foresight turned off"]"""
        """entitydata @e[tag=processed] {Glowing:0b,CustomName:"",CustomNameVisible:0b}"""
        "kill @e[type=armor_stand,tag=rider]"
        |]
    let summon25zombies = "summon25zombies",[|
        for i = 1 to 5 do
        for j = 1 to 5 do
        yield sprintf "execute @p ~%d ~ ~%d summon zombie" (i+2) (j+2)
        |]
    let summon400zombies = "summon400zombies",[|
        for i = 1 to 20 do
        for j = 1 to 20 do
        yield sprintf "execute @p ~%d ~ ~%d summon zombie" (i+2) (j+2)
        |]
    let summon100spiders = "summon100spiders",[|
        for i = 1 to 10 do
        for j = 1 to 10 do
        yield sprintf "execute @p ~%d ~ ~%d summon spider" (2*i+2) (2*j+2)
        |]
    let restart = "restart",[|
        "gamerule gameLoopFunction lorgon111:restart2"
        |]
    let restart2 = "restart2",[|
        "kill @e[type=!player]"
        "kill @e[type=!player]"
        "gamerule gameLoopFunction lorgon111:restart3"
        |]
    let restart3 = "restart3",[|
        "function lorgon111:foresight_init"
        "function lorgon111:prng_init"
        "gamerule gameLoopFunction lorgon111:foresight_loop"
        |]
    DropInModule("foresight_loop",oneTimeInit,[||],[|
        foresight_loop
        process_zombies 
        process_zombie 
        process_spiders
        process_spider
        run_rider
        turn_on 
        turn_on_mob
        turn_on_rare_mob
        turn_off 
        summon25zombies
        summon400zombies
        summon100spiders
        restart
        restart2
        restart3
        |])

// Hm, an ench book as the loot could be good if you need to 'get close' to 'read its name' before kill maybe... requires toggling CustomNameVisible via commands though, ugh, weird in SMP
// actually, glowing guy with no item on his head can just be CustomNamed, and requires getting close to read name, e.g. "Feather Falling IV"

//////////////////////////////////////

// 'wait' drop-in module, to schedule something in the future (how work? queuing order? idempotency? ...)

(*
best idea: to schedule function F to run nTicks in future, summon armor_stand tagged 'F' and 'countdown' with score N
general runner SB removes 1 all [tag=countdown] guys
if score=0, then calls dispatch on those guys to read tags and call functions (1 place in system that known universe of functions; only run when time to callback)
-----
implement something simple first, atop those, e.g. a short music piece
-----
insta-enderpearl can become 'tractor beam' or 'grappling hook', put 10 AS along bresenham long axis at tenths, and every 2 ticks, TP the player to next AS
this would use delay alongside (within) CPS machine
-----
   - probably best to do RR, and maybe also somehow let the processes 'inquire' about recent CPU utilization so they can choose to yield or do work

Actually, "run one block" is "run K blocks" where K is a constant chosen to minimize scheduler overhead while preserving system liveness.  
Hmm, but also need to ensure e.g. that a system that alternates between MustNotYield blocks and CanYield blocks with K=2 doesn't get stuck in exclusion, so only run one block 
at a time once past K, or alternatively only run one block at a time in the MustNotYield loop.

When a process has no work left to do this tick, it MUST set MustWaitNTicks to non-zero, else it will busy wait in the scheduler.

If a process has no work for the foreseeable future, it can set MustWaitNTicks to an extremely large number.  An advancement or command block could then set it to 0 to wake it back up.
These are the moral equivalents of STOP and START; e.g. an implicit loop wrapped through entrypoint and exits of the program.

If there are programs that must run every tick and don't need the help of a pump (like ForesightPotion), then the gameLoopFunction can just be a function like
    function ForesightPotionLoop
    # any other of those style programs
    function pump1    # calls all programs requiring pumps for within-tick arbitrary loops/control, or programs that span ticks

It might be a good policy to ensure every process gets at least one time slice each tick, if desired, hmm

Consider instancing; if I want two mandels running at different locations/zooms, what kind of variable mechanism do I need for separate instances? 
 - Namespaces for variables, summoned entity names/tags? ...
 - Namespaces for BBN names, yikes... compiler should check uniqueness!

Can have e.g. terrain gen put command blocks in chunks that detect idle cpu to wake up and do work a la gm4 (each chunk 1/10 change build struct, unless #ticks>K, then not)
-----
can I just have a chunkloader, and store all my entities in a loaded chunk out in the middle of nowhere? no more need spawn chunks
 - what chunkloader needs nothing to start? nothing
 - in worst case, can i summon AS at @p and spreadplayers it? (no, what if player in wrong dimension)
 - so i guess must have player in overworld to start a thingy? (also spreadplayers chunkloader fails if over void, hm)
 - ah, but execute-detect might work? seems to not any more... https://www.reddit.com/r/MinecraftCommands/comments/3r07sw/19_chunk_load_generator/
 - i guess can tp the player out there briefly? but then no way to tp him back (even if leave AS, it might unload)
 - ah, can just tp an entity out there, seems to load it.  so
    - summon entity at overworld player (or fail with error message)
    - tp entity far off
    - ... then, keep tping it there? have to try see what works to keep selection of the entity (@e[...]) alive, don't need other processing of chunk apart from selection
       - seems just constant tp not enough, nor constant spreadplayers... maybe need 5x5 chunks around to entity process? (ugh)
       - and then once working, actually do it at 0,0 so likely to be at spawn in practice and not loading extra useless chunks (though adds risk of not noticing chunk loader breaking)
(also, lookup all @e entities there using [x,y,z,dx,dy,dz] to ensure only that chunk searched)
*)

//////////////////////////////////////

// location of spawn chunks (for a drop-in, where to place entity to make work anywhere in world)
// singleplayer
// change scheduler code to write out (and call) functions in separate folders for each program and drop-in (like I did with one-tick compiler)


// first TODO is to make a non-Bresenham raycast algorithm and check that it no longer has precision errors I see from Bresenham:
//  - target no exact when I an near edge of block (more exact when my eyes near center block)
//  - target sometimes not next to collision (diagonally away)
//  - can clip through corners
// Note that simplest/best 'selection' technique is a single raycast out from eyeballs of player until air just before collision, then (start GREEN)
//  - if was looking down, first check square above for air, else check square below for air, else RED
//  - else (if was looking up), first check square below for air, else check square above for air, else RED
//  - then if GREEN, can tp to it, but if RED cannot because selected square without 'room'; the double-ray-casts are too complicated/expensive
//  - can use glowing magma cube with team color to display red/green and selected-block raycast (and have glowing AS when green but no displayed AS when red)


// portal gun: raycast to shoot them out, particles or something to mark spot, and can use world-coord-checker to tp you there (a la wubbi)?  would work even to unloaded places!
//  - can't do particles anywhere in world from 0,0,0 AS (too many commands/functions even with binary), either need entity or relative to player
//  - entity has problem that if player goes away, entity may remain and be unloaded, many GC issues, so
//  - relative to player is best, can see if portal location <= N distance radially from player, and if so, display particles relative to player, but
//  - then if player move 0.3 block, the particles will also move 0.3 blocks, so need a way to snap-to-grid in coordinate-math
//  - summon leash_knot works, within-tick can summon another entity from l_k and is centered
//  - note that we want to snap-to-grid at the _end_ of the raycast; probably want to use actual eyeball location at start and while raycast, for better 'feel' of results
// so next TODO is to abstract my raycast thingy and make it more efficient
//  - make it a module; input is tagged player, output is rayx/rayy/rayz as well as the RAY AS entity, all work is grid-snapped coords
//     - yes, it can be a module, not a program, by having its own dedicated CPS pump, since it's all MustNotYield (would be faster than program scheduler overhead anyway)
//     - disadvantage of module v program is that we can no longer give the scheduler an accurate measure to load-balance other programs, hmm (AtomicCommandWithExtraCost)
//     - advantage of module is can publish it separately for people other than me to consume
//  - make a compiler that can compile a program to a standalone module, with own CPS pump, but no scheduler
//     - program runs in one tick; "MustWaitNTicks 1" is like "Halt", anything other than MWNT1/MNY is a compilation error
//        - may want an EveryTick abstraction ideally, where an EveryTick program compiles 'Halt' to 'wait 1' rather than 'wait 9999999' in the scheduler-compiler
//        - EveryTick programs do not admit NoPreference or MWNT, in either scheduler-compiler or module-compiler; both produce compile errors for those
//     - as standalone module, needs option to also summon ENTITY if not part of one of my scheduled programs which sets up ENTITY (and help for users to modify coords to their spawn chunks)
//  - ensure program still work as part of old muti-task, while get new module working from same program code
//  - then make new client for portal gun to display particles or something
//  - will need a constantly running teleporter, e.g. if player's coords exactly equal blue portal coords, then tp to orange portal coords, and vice versa
//     - and need some cooldown (5s?) so not constantly tp back and forth, hmm, that will require across-tick logic, I guess just a score on the player that constantly decrements is fine, no biggie

// online terrain-gen that uses chunkloaders to create (and modify?) terrain outside the player's viewdistance?
//  - can test using spectators-gen-chunks=false, and spectate to see what i have wrought?

// survey advancements for ideas of fun things (biomes?)

// snowball teleporter could turn into a grappling-hook that takes time to move over distance

//lava-dipped arrows (remote light/lava) "flowing_lava 7" does not seem to catch flammable things on fire, gives light for 1s and drops/disappears, or lava laser

// what would a UI for accessories/upgrades/trinkets/carrying-capacity/buy/sell look like? what are interesting non-armor/weapon upgrades? (arcane strike, various 'rush' buffs after kill/streak, ...)

//weapon with very long cooldown but very strong (e.g. one hit at start of battle?)

// something where if you look up at the sky, like fireworks-words appear or something? chest says look up, you do, particles spell something? all mobs look up? ...

//diamond-laying chickens under attack by zombie (how get Z to attack C?) you need to save if want to farm; 
// - or i guess chicken could be villager who produces goods with egg laying sound... 
// - could be fun recurring set piece thruout a map, find Z chasing C, save C and get a finite-farmable reward; people like to farm, this is non-standard farming


// one-way-home teleport mechanic? something time consuming you can't use in battle? drink potion, gives you nausea->blindness over 5s, then tp home

// 'desire lines' grass repeatedly walked on changes to dirt, etc (see gm4)

// 'enderman support class' from gm4 is cool idea (endermen give buffs to nearby mobs, to incentivize player to get rid of them)

// idea of 'flat villages', e.g. detect flattish spaces to add villages, rather than minecrafty way on sides of mountains ridiculously?

// ideas: use randomness/misc from environmnt http://gamemode4.wikia.com/wiki/Dangerous_Dungeon_Structure_Pack

// ideas: now with biomes, change underground http://gamemode4.wikia.com/wiki/Cooler_Caves_Expansion_Pack
//  - start in one cell of a biome, and radiate armor-stand outwards flood-fill-like to either reach edge of biome, or reach some other limit of existence/player-nearness to stop?
//  - need chunk-finding algorithm
//  - need to eval an area is reasonablyh big before we start
//  - need to find a space-filling al to expand perimiter with minimum num armor stands or cmd blocks
//  - need to stop if age reaches a certain value, or if player underground/within certain range
//  - should make surface-visible something for ease of debugging