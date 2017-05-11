module FunctionUtilities

// binaryLookup("findPhi", "look", "rym", "ry", 7, 3, -180, fs) 
// looks at @e[tag=look] and binary searches 2^7 in steps of 8 at offset -180, so e.g. [-180..-178] and [-177..-175] are bottom buckets
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

let phiScope = new FunctionCompiler.Scope()
let vphi = phiScope.RegisterVar("phi")
let vKsinPhi = phiScope.RegisterVar("KsinPhi")
let vKcosPhi = phiScope.RegisterVar("KcosPhi")
let findPhi = binaryLookup("findPhi", "look", "rym", "ry", 7, 3, -180, 
                                [fun phi -> vphi.Name,phi
                                 fun phi -> vKsinPhi.Name,int(1000.0*sin(System.Math.PI * float phi / 180.0))
                                 fun phi -> vKcosPhi.Name,int(1000.0*cos(System.Math.PI * float phi / 180.0))
                                ]) 

let thetaScope = new FunctionCompiler.Scope()
let vtheta = thetaScope.RegisterVar("theta")
let vKsinTheta = thetaScope.RegisterVar("Ksintheta")
let vKcosTheta = thetaScope.RegisterVar("Kcostheta")
let findTheta = binaryLookup("findTheta", "look", "rxm", "rx", 6, 3, -90, 
                                [fun theta -> vtheta.Name,theta
                                 fun theta -> vKsinTheta.Name,int(1000.0*sin(System.Math.PI * float theta / 180.0))
                                 fun theta -> vKcosTheta.Name,int(1000.0*cos(System.Math.PI * float theta / 180.0))
                                ]) 

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

let raycastProgram = 
    Program([|
        // SB init
        for v in [yield! raycastVars.All(); yield! phiScope.All(); yield! thetaScope.All()] do
            yield AtomicCommand(sprintf "scoreboard objectives add %s dummy" v.Name)
        // constants
        yield SB(R .= 128)
        yield SB(ONE_THOUSAND .= 1000)
        // prep code
        yield AtomicCommand "summon armor_stand 0 4 0 {CustomName:RAY,NoGravity:1,Invisible:1,Glowing:1,Invulnerable:1}"
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
            AtomicCommand("tp @e[name=RAY] @p")
            AtomicCommand("tp @e[name=RAY] ~ ~1 ~")
            // TODO keepgoing
            |],DirectTailCall(whiletest))
        whiletest,BasicBlock([|
            |],ConditionalTailCall(Conditional[| MAJOR .>= 1 |],loopbody,coda))
        loopbody,BasicBlock([|
            // TODO do stuff at RAY
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
            |],DirectTailCall(whiletest))
        coda,BasicBlock([|
            |],Halt)
        ])
        // TODO throw snowball to activate
        // first test, just 'see' AS