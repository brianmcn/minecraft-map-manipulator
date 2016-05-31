module CaveGeneration

open RegionFiles

//////////////////////////////

(*
!testforblock -55 124 28 air 0
C: !testforblock -55 124 28 air 0
MINECRAFT> [17:55:13] [Server thread/INFO]: The block at -55,124,28 is Grass Block (expected: tile.air.name).
!testforblock -55 124 28 grass 0
C: !testforblock -55 124 28 grass 0
MINECRAFT> [17:55:42] [Server thread/INFO]: Successfully found the block at -55,124,28.
*)


//////////////////////////////

// TODO dummy
let map = new MapFolder("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\solidCopy\region""")

let rng = new System.Random(0)

//////////////////////////////

let newHS() = new System.Collections.Generic.HashSet<_>()

let computeSphere(xc, yc, zc, r) = 
    let sphere = newHS()
    let sq x = x*x
    for x = int(xc-r-1.0) to int(xc+r+1.0) do
        for y = int(yc-r-1.0) to int(yc+r+1.0) do
            for z = int(zc-r-1.0) to int(zc+r+1.0) do
                if sq(float(x)-xc)+sq(float(y)-yc)+sq(float(z)-zc) < sq(r) then
                    sphere.Add((x, y, z)) |> ignore
    sphere

let interpolate(start, stop, n, steps) = start + (stop-start)*(float(n)/float(steps))

let MAX_INTERPOLATE = 2
let computePassage(xc1, yc1, zc1, r1, xc2, yc2, zc2, r2) =
    let MAX = MAX_INTERPOLATE
    let result = newHS()
    for n in 0..MAX do
        let xc = interpolate(xc1,xc2,n,MAX)
        let yc = interpolate(yc1,yc2,n,MAX)
        let zc = interpolate(zc1,zc2,n,MAX)
        let r  = interpolate(r1, r2, n,MAX)
        result.UnionWith(computeSphere(xc, yc, zc, r))
    result

type CaveState(existingAirSpace,existingWalls,recentAirSpace,recentWalls) =
    let WALL_THICKNESS = 3.0
    let mutable outOfBounds = fun(x,y,z) -> false
    let collisions       = newHS()
    new() = CaveState(newHS(),newHS(),newHS(),newHS())
    member this.SetBoundaryFunction(f) = outOfBounds <- f
    member this.AsCommands() =
        let r = ResizeArray()
        let air = new System.Collections.Generic.HashSet<_>(existingAirSpace)
        air.UnionWith(recentAirSpace)
        for x,y,z in air do
            r.Add(sprintf "setblock %d %d %d air" x y z)
        let walls = new System.Collections.Generic.HashSet<_>(existingWalls)
        walls.UnionWith(recentWalls)
        for x,y,z in walls do
            r.Add(sprintf "setblock %d %d %d stone" x y z)
        for x,y,z in collisions do
            r.Add(sprintf "setblock %d %d %d stained_glass 14" x y z)
        r
    member this.TryAddAirRegion(airRegion : System.Collections.Generic.HashSet<_>) =
        let collisionWithExistingWalls = new System.Collections.Generic.HashSet<_>(airRegion)
        collisionWithExistingWalls.IntersectWith(existingWalls)
        for x,y,z in airRegion do
            if outOfBounds(x,y,z) then
                collisionWithExistingWalls.Add((x,y,z)) |> ignore
        if collisionWithExistingWalls.Count = 0 then
            // ok to add
            existingAirSpace.UnionWith(recentAirSpace)

            recentWalls.ExceptWith(airRegion) // we've carved some out
            existingWalls.UnionWith(recentWalls)

            recentAirSpace.Clear()
            recentAirSpace.UnionWith(airRegion)

            recentWalls.Clear()
            let allAirSpace = new System.Collections.Generic.HashSet<_>(existingAirSpace)
            allAirSpace.UnionWith(recentAirSpace)
            for xc, yc, zc in recentAirSpace do
                let wallSpace = computeSphere(float xc, float yc, float zc, WALL_THICKNESS)
                for x,y,z in wallSpace do
                    if not(allAirSpace.Contains(x,y,z)) then
                        recentWalls.Add((x,y,z)) |> ignore
            true
        else
            collisions.UnionWith(collisionWithExistingWalls)
            // not ok, but make changes for a visualization
            existingAirSpace.UnionWith(recentAirSpace)

            recentWalls.ExceptWith(airRegion) // we've carved some out
            existingWalls.UnionWith(recentWalls)

            recentAirSpace.Clear()
            recentAirSpace.UnionWith(airRegion)

            recentWalls.Clear()
            let allAirSpace = new System.Collections.Generic.HashSet<_>(existingAirSpace)
            allAirSpace.UnionWith(recentAirSpace)
            for xc, yc, zc in recentAirSpace do
                let wallSpace = computeSphere(float xc, float yc, float zc, WALL_THICKNESS)
                for x,y,z in wallSpace do
                    if not(allAirSpace.Contains(x,y,z)) then
                        recentWalls.Add((x,y,z)) |> ignore
            false

let makeRandomCave(xs, ys, zs, rs, initPhi, initTheta, desiredLength, rng:System.Random) =
    // spherical coordinates (phi=0 is up) to minecraft axes:
    // x = sin phi * cos theta
    // z = sin phi * sin theta
    // y = cos phi
    // setup boundaries
    let hardBoundaryWalls = newHS()
    let softBoundaryWalls   = newHS()
    for x = -400 to 400 do
        for y = 10 to 130 do
            hardBoundaryWalls.Add((x,y,-400)) |> ignore
            hardBoundaryWalls.Add((x,y,400)) |> ignore
    for z = -400 to 400 do
        for y = 10 to 130 do
            hardBoundaryWalls.Add((-400,y,z)) |> ignore
            hardBoundaryWalls.Add((400,y,z)) |> ignore
    for x = -400 to 400 do
        for z = -400 to 400 do
            hardBoundaryWalls.Add((x,10,z)) |> ignore
            softBoundaryWalls.Add((x,130,z)) |> ignore
    let caveState = new CaveState()
    caveState.SetBoundaryFunction(fun(x,y,z) -> hardBoundaryWalls.Contains(x,y,z))
    let computeNextXYZ(curX, curY, curZ, nextPhi, nextTheta, nextLength) =
        let nextX = curX + (sin(nextPhi)*cos(nextTheta)*nextLength)
        let nextZ = curZ + (sin(nextPhi)*sin(nextTheta)*nextLength)
        let nextY = curY + (cos(nextPhi)*nextLength)
        nextX, nextY, nextZ
    let mutable curX, curY, curZ, curR, curPhi, curTheta, totalLength = xs, ys, zs, rs, initPhi, initTheta, 0.0
    let mutable ok, firstTime = true, true
    while ok && totalLength < desiredLength do
        // choose a next heading, r, & segment length
        let nextPhi = curPhi + (rng.NextDouble()-0.5)*1.0 // +/- half radian
        let nextTheta = curTheta + (rng.NextDouble()-0.5)*2.0
        let nextR = curR + (rng.NextDouble()-0.5)*1.0
        let nextLength = rng.NextDouble()*7.0 + 5.0 // 5 to 12
        // (ensure connected)
        let minR = min nextR curR
        let maxLen = minR * 2.0 * float(MAX_INTERPOLATE)
        let nextLength = min nextLength maxLen
        let nextX,nextY,nextZ = computeNextXYZ(curX, curY, curZ, nextPhi, nextTheta, nextLength)
        // make next segment
        let passage = computePassage(curX, curY, curZ, curR, nextX, nextY, nextZ, nextR)
        if caveState.TryAddAirRegion(passage) then
            curX <- nextX
            curY <- nextY
            curZ <- nextZ
            curR <- nextR
            curPhi <- nextPhi
            curTheta <- nextTheta
            totalLength <- totalLength + nextLength
            printfn "making segment: (%3.0f, %3.0f, %3.0f) (%2.1f, %1.2f, %1.2f)" nextX nextY nextZ nextR nextPhi nextTheta
        else
            ok <- false
            printfn "FAILED WHILE making segment: (%3.0f, %3.0f, %3.0f) (%2.1f, %1.2f, %1.2f)" nextX nextY nextZ nextR nextPhi nextTheta
        if firstTime then
            // after we've plunged into the earth, make top earth surface become a new boundary
            caveState.SetBoundaryFunction(fun(x,y,z) -> hardBoundaryWalls.Contains(x,y,z) || softBoundaryWalls.Contains(x,y,z))
            firstTime <- false
    // affect the world
    let cmds = caveState.AsCommands()
    let SERVER_DIRECTORY = """C:\Users\Admin1\Desktop\Server""" 
    let COMMAND_FILE = System.IO.Path.Combine(SERVER_DIRECTORY,"commands_to_run.txt")
    System.IO.File.WriteAllLines(COMMAND_FILE, cmds)
    printfn "CGLS wrote %d commands to file" cmds.Count 

(*

let carveSphere(map:MapFolder, xc, yc, zc, r) = 
    let sq x = x*x
    for x = int(xc-r-1.0) to int(xc+r+1.0) do
        for y = int(yc-r-1.0) to int(yc+r+1.0) do
            for z = int(zc-r-1.0) to int(zc+r+1.0) do
                if sq(float(x)-xc)+sq(float(y)-yc)+sq(float(z)-zc) < sq(r) then
                    //map.EnsureSetBlockIDAndDamage(x,y,z,0uy,0uy)
                    inputEvents.Add(COMMAND (sprintf "setblock %d %d %d air" x y z))

let MAX_INTERPOLATE = 2
let carvePassage(map:MapFolder, xc1, yc1, zc1, r1, xc2, yc2, zc2, r2) =
    let MAX = MAX_INTERPOLATE
    for n in 0..MAX do
        let xc = interpolate(xc1,xc2,n,MAX)
        let yc = interpolate(yc1,yc2,n,MAX)
        let zc = interpolate(zc1,zc2,n,MAX)
        let r  = interpolate(r1, r2, n,MAX)
        carveSphere(map, xc, yc, zc, r)

let makeRandomCave(map:MapFolder, xs, ys, zs, rs, initPhi, initTheta, desiredLength, rng:System.Random) =
    // spherical coordinates (phi=0 is up) to minecraft axes:
    // x = sin phi * cos theta
    // z = sin phi * sin theta
    // y = cos phi
    let inBounds(x,y,z,r) =
        x-r > -400.0 && x+r < 400.0 &&
        y-r >   10.0 && y+r < 130.0 &&
        z-r > -400.0 && z+r < 400.0
    let computeNextXYZ(curX, curY, curZ, nextPhi, nextTheta, nextLength) =
        let nextX = curX + (sin(nextPhi)*cos(nextTheta)*nextLength)
        let nextZ = curZ + (sin(nextPhi)*sin(nextTheta)*nextLength)
        let nextY = curY + (cos(nextPhi)*nextLength)
        nextX, nextY, nextZ
    let mutable curX, curY, curZ, curR, curPhi, curTheta, totalLength = xs, ys, zs, rs, initPhi, initTheta, 0.0
    let mutable failCount = 0
    while totalLength < desiredLength do
        // choose a next heading, r, & segment length
        let nextPhi = curPhi + (rng.NextDouble()-0.5)*1.0 // +/- half radian
        let nextTheta = curTheta + (rng.NextDouble()-0.5)*2.0
        let nextR = curR + (rng.NextDouble()-0.5)*1.0
        let nextLength = rng.NextDouble()*7.0 + 5.0 // 5 to 12
        // (ensure connected)
        let minR = min nextR curR
        let maxLen = minR * 2.0 * float(MAX_INTERPOLATE)
        let nextLength = min nextLength maxLen
        // validate
        let mutable ok = true
        if nextR < 1.5 || nextR > 5.0 then
            ok <- false
            printfn "R out of bounds"
        let nextX,nextY,nextZ = computeNextXYZ(curX, curY, curZ, nextPhi, nextTheta, nextLength)
        if not(inBounds(nextX,nextY,nextZ,nextR)) then
            ok <- false
            printfn "out of bounds: (%3.0f, %3.0f, %3.0f) (%2.1f, %1.2f, %1.2f)" nextX nextY nextZ nextR nextPhi nextTheta
        // heuristic to avoid steering at a wall we later can't avoid
        for tries = 8 downto 1 do
            if ok then
                let nextX,nextY,nextZ = computeNextXYZ(curX, curY, curZ, nextPhi, nextTheta, nextLength * float(tries))
                if not(inBounds(nextX,nextY,nextZ,nextR)) then
                    if rng.Next(tries+4) <= 4 then
                        ok <- false
                        printfn "steering towards bounds %d: (%3.0f, %3.0f, %3.0f) (%2.1f, %1.2f, %1.2f)" tries nextX nextY nextZ nextR nextPhi nextTheta
                    else
                        printfn "steering towards bounds %d but ignoring" tries
        if ok then
            // make next segment
            carvePassage(map, curX, curY, curZ, curR, nextX, nextY, nextZ, nextR)
            curX <- nextX
            curY <- nextY
            curZ <- nextZ
            curR <- nextR
            curPhi <- nextPhi
            curTheta <- nextTheta
            totalLength <- totalLength + nextLength
            printfn "making segment: (%3.0f, %3.0f, %3.0f) (%2.1f, %1.2f, %1.2f)" nextX nextY nextZ nextR nextPhi nextTheta
            failCount <- 0
        else
            failCount <- failCount + 1
        if failCount = 16 then
            printfn "failed 16 times in a row, picking a new random heading to escape"
            curPhi <- rng.NextDouble()*2.0*System.Math.PI 
            curTheta <- rng.NextDouble()*2.0*System.Math.PI 
            failCount <- 0
*)