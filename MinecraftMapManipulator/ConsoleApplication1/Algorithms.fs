module Algorithms

////////////////////////////

// These data structures are used in connected-components algorithms in the code

type Thingy(point:int, isLeft:bool, isRight:bool) =
    let mutable isLeft = isLeft
    let mutable isRight = isRight
    member this.Point = point
    member this.IsLeft with get() = isLeft and set(x) = isLeft <- x
    member this.IsRight with get() = isRight and set(x) = isRight <- x

// A partition is a mutable set of values, where one arbitrary value in the set 
// is chosen as the canonical representative for that set. 
[<AllowNullLiteral>]
type Partition(orig : Thingy) as this =  
    [<DefaultValue(false)>] val mutable parent : Partition
    [<DefaultValue(false)>] val mutable rank : int 
    let rec FindHelper(x : Partition) = 
        if System.Object.ReferenceEquals(x.parent, x) then 
            x 
        else 
            x.parent <- FindHelper(x.parent) 
            x.parent 
    do this.parent <- this 
    // The representative element in this partition 
    member this.Find() = 
        FindHelper(this) 
    // The original value of this element 
    member this.Value = orig 
    // Merges two partitions 
    member this.Union(other : Partition) = 
        let thisRoot = this.Find() 
        let otherRoot = other.Find() 
        if thisRoot.rank < otherRoot.rank then 
            otherRoot.parent <- thisRoot
            thisRoot.Value.IsLeft <- thisRoot.Value.IsLeft || otherRoot.Value.IsLeft 
            thisRoot.Value.IsRight <- thisRoot.Value.IsRight || otherRoot.Value.IsRight
        elif thisRoot.rank > otherRoot.rank then 
            thisRoot.parent <- otherRoot 
            otherRoot.Value.IsLeft <- otherRoot.Value.IsLeft || thisRoot.Value.IsLeft 
            otherRoot.Value.IsRight <- otherRoot.Value.IsRight || thisRoot.Value.IsRight
        elif not (System.Object.ReferenceEquals(thisRoot, otherRoot)) then 
            otherRoot.parent <- thisRoot 
            thisRoot.Value.IsLeft <- thisRoot.Value.IsLeft || otherRoot.Value.IsLeft 
            thisRoot.Value.IsRight <- thisRoot.Value.IsRight || otherRoot.Value.IsRight
            thisRoot.rank <- thisRoot.rank + 1 

////////////////////////////////////

// maximal rectangle
// http://stackoverflow.com/questions/7245/puzzle-find-largest-rectangle-maximal-rectangle-problem

let findMaximalRectangle(a:bool[,]) =
    let XMIN, XLEN = a.GetLowerBound(0), a.GetLength(0)
    let ZMIN, ZLEN = a.GetLowerBound(1), a.GetLength(1)
    let mutable bestArea = 0
    let bestLL = ref (0,0)
    let bestUR = ref (-1,-1)
    let cache = Array.zeroCreate ZLEN // how many 'true's along this x
    let stack = new System.Collections.Generic.Stack<_>()
    for x = XMIN to XMIN+XLEN-1 do
        // update cache
        for z = ZMIN to ZMIN+ZLEN-1 do
            if not a.[x,z] then
                cache.[z-ZMIN] <- 0
            else
                cache.[z-ZMIN] <- cache.[z-ZMIN] + 1
        // do alg
        let mutable curHeight = 0
        for z = ZMIN to ZMIN+ZLEN-1 do
            let height = cache.[z-ZMIN]
            if height > curHeight then
                stack.Push(z,curHeight)
                curHeight <- height
            elif height < curHeight then
                let mutable oldz, oldh = -1, -1
                while height < curHeight do
                    let tz,th = stack.Pop()
                    oldz <- tz
                    oldh <- th
                    let width = z - oldz
                    let area = curHeight * width
                    if area > bestArea then
                        bestArea <- area
                        bestLL := x-curHeight+1, oldz
                        bestUR := x, z-1
                    curHeight <- oldh
                curHeight <- height
                if curHeight <> 0 then
                    stack.Push(oldz,oldh)
    !bestLL, !bestUR, bestArea 

//////////////////////////////////////////////////////////////////////////////////////////////////

// saw various complicated 3-d skeletonization / thinning voxel algorithms
// http://dsp.stackexchange.com/questions/1154/open-source-implementation-of-3d-thinning-algorithm
// https://view.officeapps.live.com/op/view.aspx?src=http%3A%2F%2Fwww.massey.ac.nz%2F~mjjohnso%2Fnotes%2F59731%2Fpresentations%2FThinning%2520Algorithm.doc
// let's attempt my own

// values: -1 just removed, -2 removed a pass ago -3 removed 2 passes ago ... 
//          0 removed a while ago
//          1 in, no extra data
//          2 in, is a connector
//          3 in, is an endpoint

let skeletonize(a:sbyte[,,],onRemove) = // init array passed in should be all 1s and 0s, will find skeleton of 1s, boundary should be all 0 sentinels
    let TRUE_ENDPOINT = 7
    let NAIVE_ENDPOINT = 8
    let NON_ENDPOINT = 9
    let isEndpoint(x,y,z) =
        let mutable missingNeighbors = 0 // only count non-recent removals as gone
        if a.[x+1,y,z] = 0y then missingNeighbors <- missingNeighbors + 1
        if a.[x-1,y,z] = 0y then missingNeighbors <- missingNeighbors + 1
        if a.[x,y+1,z] = 0y then missingNeighbors <- missingNeighbors + 1
        if a.[x,y-1,z] = 0y then missingNeighbors <- missingNeighbors + 1
        if a.[x,y,z+1] = 0y then missingNeighbors <- missingNeighbors + 1
        if a.[x,y,z-1] = 0y then missingNeighbors <- missingNeighbors + 1
        if missingNeighbors = 5 then
            // when is an endpoint really an endpoint, as opposed to a bump on a surface? the ceiling of a 'round room' with a 2x2 ceiling bump turns into an endpoint, which is undesirable.
            // thus, let's look for a 'bigger surface', and just erode would-be endpoints that appear to actually be mere 'bumps'.
            let mutable neighborNeighbors = 0
            if a.[x+1,y,z] <> 0y then
                if a.[x+1,y+1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x+1,y-1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x+1,y,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x+1,y,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
            elif a.[x-1,y,z] <> 0y then
                if a.[x-1,y+1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x-1,y-1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x-1,y,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x-1,y,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
            elif a.[x,y+1,z] <> 0y then
                if a.[x+1,y+1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x-1,y+1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y+1,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y+1,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
            elif a.[x,y-1,z] <> 0y then
                if a.[x+1,y-1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x-1,y-1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y-1,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y-1,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
            elif a.[x,y,z+1] <> 0y then
                if a.[x+1,y,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x-1,y,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y+1,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y-1,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
            elif a.[x,y,z-1] <> 0y then
                if a.[x+1,y,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x-1,y,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y+1,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y-1,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
            if neighborNeighbors <= 0 then                // TODO ad-hoc, maybe 0?
                TRUE_ENDPOINT
            else
                NAIVE_ENDPOINT
        else
            NON_ENDPOINT
    let DIRS =
        [|
            1,0,0, [|[|0;1;0|];[|0;-1;0|];[|0;0;1|];[|0;0;-1|]|]
            0,1,0, [|[|1;0;0|];[|-1;0;0|];[|0;0;1|];[|0;0;-1|]|]
            0,0,1, [|[|1;0;0|];[|-1;0;0|];[|0;1;0|];[|0;-1;0|]|]
        |]
    let find(x,y,z,dx,dy,dz,s:string) =
        let mutable ok = true
        for i = 0 to s.Length-1 do
            if s.[i] = 'O' && (a.[x+i*dx,y+i*dy,z+i*dz] > 0y || a.[x+i*dx,y+i*dy,z+i*dz] = -1y) then  // -1 is just-removed, don't want to shave down multiple times in one pass
                ok <- false
            if s.[i] = 'X' && a.[x+i*dx,y+i*dy,z+i*dz] <= 0y then
                ok <- false
        ok
    let ones = new System.Collections.Generic.HashSet<_>()
    let decrement() =
        for x = a.GetLowerBound(0) to a.GetLowerBound(0)+a.GetLength(0)-1 do
            for y = a.GetLowerBound(1) to a.GetLowerBound(1)+a.GetLength(1)-1 do
                for z = a.GetLowerBound(2) to a.GetLowerBound(2)+a.GetLength(2)-1 do
                    if a.[x,y,z] < 0y then
                        a.[x,y,z] <- a.[x,y,z] - 1y
                        if a.[x,y,z] <= -5y then
                            a.[x,y,z] <- 0y
    for x = a.GetLowerBound(0) to a.GetLowerBound(0)+a.GetLength(0)-1 do
        for y = a.GetLowerBound(1) to a.GetLowerBound(1)+a.GetLength(1)-1 do
            for z = a.GetLowerBound(2) to a.GetLowerBound(2)+a.GetLength(2)-1 do
                if a.[x,y,z] = 1y then
                    ones.Add(x,y,z) |> ignore
    // do an initial smoothing, by pruning immediate endpoints
    let onesSnapshot = ones |> Seq.toArray 
    for x,y,z in onesSnapshot do
        if isEndpoint(x,y,z)<>NON_ENDPOINT then
            a.[x,y,z] <- 0y
            ones.Remove(x,y,z) |> ignore
    printfn "SKEL"
    let mutable iter = 0
    let mutable ok = true
    while ok do
        let mutable wereAnyRemoved  = false
        for d = 0 to 5 do
            printf "."
            let dx, dy, dz, b = 
                if d <= 2 then // + dirs
                    DIRS.[d]
                else           // - dirs
                    let dx, dy, dz, b = DIRS.[d-3]
                    let dx, dy, dz = -dx, -dy, -dz
                    dx,dy,dz,b
            let onesSnapshot = ones |> Seq.toArray 
            Array.sortInPlace onesSnapshot 
            for x,y,z in onesSnapshot do
                let e = isEndpoint(x,y,z)
                if e = TRUE_ENDPOINT then
                    a.[x,y,z] <- 3y
                    ones.Remove(x,y,z) |> ignore
                elif e = NAIVE_ENDPOINT then
                    a.[x,y,z] <- -1y
                    ones.Remove(x,y,z) |> ignore
                    wereAnyRemoved <- true
                    onRemove(x,y,z,iter)
                elif find(x-dx,y-dy,z-dz,dx,dy,dz,"OXX") then
                    // TODO don't introduce concavities
                    //    XXX                      XXX
                    //  ->XXX   should not become  OXX 
                    //    XXX                      XXX
                    // 
                    //       XO                                                                   XX
                    //      OXX                                                                    X
                    //  L-shaped connector                                                      already connected in another plane
                    if  find(x+b.[0].[0],y+b.[0].[1],z+b.[0].[2],dx,dy,dz,"XO") && not(find(x+b.[0].[0]+b.[2].[0],y+b.[0].[1]+b.[2].[1],z+b.[0].[2]+b.[2].[2],dx,dy,dz,"XX") && find(x+dx+b.[2].[0],y+dy+b.[2].[1],z+dz+b.[2].[2],dx,dy,dz,"X") || find(x+b.[0].[0]+b.[3].[0],y+b.[0].[1]+b.[3].[1],z+b.[0].[2]+b.[3].[2],dx,dy,dz,"XX") && find(x+dx+b.[3].[0],y+dy+b.[3].[1],z+dz+b.[3].[2],dx,dy,dz,"X")) ||
                        find(x+b.[1].[0],y+b.[1].[1],z+b.[1].[2],dx,dy,dz,"XO") && not(find(x+b.[1].[0]+b.[2].[0],y+b.[1].[1]+b.[2].[1],z+b.[1].[2]+b.[2].[2],dx,dy,dz,"XX") && find(x+dx+b.[2].[0],y+dy+b.[2].[1],z+dz+b.[2].[2],dx,dy,dz,"X") || find(x+b.[1].[0]+b.[3].[0],y+b.[1].[1]+b.[3].[1],z+b.[1].[2]+b.[3].[2],dx,dy,dz,"XX") && find(x+dx+b.[3].[0],y+dy+b.[3].[1],z+dz+b.[3].[2],dx,dy,dz,"X")) ||
                        find(x+b.[2].[0],y+b.[2].[1],z+b.[2].[2],dx,dy,dz,"XO") && not(find(x+b.[2].[0]+b.[0].[0],y+b.[2].[1]+b.[0].[1],z+b.[2].[2]+b.[0].[2],dx,dy,dz,"XX") && find(x+dx+b.[0].[0],y+dy+b.[0].[1],z+dz+b.[0].[2],dx,dy,dz,"X") || find(x+b.[2].[0]+b.[1].[0],y+b.[2].[1]+b.[1].[1],z+b.[2].[2]+b.[1].[2],dx,dy,dz,"XX") && find(x+dx+b.[1].[0],y+dy+b.[1].[1],z+dz+b.[1].[2],dx,dy,dz,"X")) ||
                        find(x+b.[3].[0],y+b.[3].[1],z+b.[3].[2],dx,dy,dz,"XO") && not(find(x+b.[3].[0]+b.[0].[0],y+b.[3].[1]+b.[0].[1],z+b.[3].[2]+b.[0].[2],dx,dy,dz,"XX") && find(x+dx+b.[0].[0],y+dy+b.[0].[1],z+dz+b.[0].[2],dx,dy,dz,"X") || find(x+b.[3].[0]+b.[1].[0],y+b.[3].[1]+b.[1].[1],z+b.[3].[2]+b.[1].[2],dx,dy,dz,"XX") && find(x+dx+b.[1].[0],y+dy+b.[1].[1],z+dz+b.[1].[2],dx,dy,dz,"X")) then
                            //a.[x,y,z] <- 2y
                            //ones.Remove(x,y,z) |> ignore
                            () // don't permanently save connectors, as the things they connect may be eroded away, just skip for now
                    else
                        a.[x,y,z] <- -1y
                        ones.Remove(x,y,z) |> ignore
                        wereAnyRemoved <- true
                        onRemove(x,y,z,iter)
            decrement()
        if not wereAnyRemoved then
            ok <- false
        iter <- iter + 1
    printfn ""
    () // just return, a is skeletonized, 3s are endpoints    