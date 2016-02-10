module Algorithms

////////////////////////////

let swap (a: _[]) x y =
    let tmp = a.[x]
    a.[x] <- a.[y]
    a.[y] <- tmp

let shuffle(a,rng:System.Random) = Array.iteri (fun i _ -> swap a i (rng.Next(i, Array.length a))) a

////////////////////////////

// choose N items from array A, with replacement, but make it increasingly unlikely to re-choose an item the more it has already been chosen
let pickNnonindependently(rng:System.Random,n,a) =
    let a = a |> Seq.toArray 
    let counts = Array.create a.Length 1
    let r = ResizeArray()
    while r.Count < n do
        let i = rng.Next(a.Length)
        if rng.Next(counts.[i]) <> 0 then
            () // choose again
        else
            r.Add(a.[i])
            counts.[i] <- counts.[i] + 1
    r.ToArray()


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

// dijkstra
let findShortestPathCore(sx,sy,sz,canMove,isEnd,differences:_[],findLongest) =
    let visited = new System.Collections.Generic.Dictionary<_,_>()  // key exists = visited, value = previous direction
    let q = new System.Collections.Generic.Queue<_>()
    let mutable e = None
    q.Enqueue(sx,sy,sz)
    visited.Add((sx,sy,sz), -1)
    while q.Count > 0 do
        let x,y,z = q.Dequeue()
        let mutable keepGoing = true
        if isEnd(x,y,z) then
            e <- Some (x,y,z)
            if not findLongest then
                while q.Count > 0 do
                    q.Dequeue() |> ignore
                keepGoing <- false
        if keepGoing then
            for diffi = 0 to differences.Length-1 do
                let dx,dy,dz = differences.[diffi]
                let nx,ny,nz = x+dx, y+dy, z+dz
                if canMove(nx,ny,nz) && not(visited.ContainsKey(nx,ny,nz)) then
                    visited.Add((nx,ny,nz), diffi)
                    q.Enqueue(nx,ny,nz)
    match e with
    | None -> None
    | Some(ex,ey,ez) ->
        let path = ResizeArray()
        let moves = ResizeArray()
        let mutable cx,cy,cz = ex,ey,ez
        while visited.[cx,cy,cz] <> -1 do
            path.Add( cx,cy,cz )
            moves.Add( visited.[cx,cy,cz] )
            let dx,dy,dz = differences.[ visited.[cx,cy,cz] ]
            cx <- cx - dx
            cy <- cy - dy
            cz <- cz - dz
        path.Add( cx,cy,cz )
        path.Reverse()
        moves.Reverse()
        Some((ex,ey,ez), path, moves)  // path is list of points from start to end, inclusive; moves is differences-indexes

let findShortestPath(sx,sy,sz,canMove,isEnd,differences:_[]) =
    findShortestPathCore(sx,sy,sz,canMove,isEnd,differences,false)
let findLongestPath(sx,sy,sz,canMove,isEnd,differences:_[]) =
    findShortestPathCore(sx,sy,sz,canMove,isEnd,differences,true)

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

let skeletonize(a:sbyte[,,],onRemove,initOnes) = // init array passed in should be all 1s/0s, will find skeleton of 1s, boundary should be all 0 sentinels; last args will be mutated if non-null
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
                if a.[x+2,y,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x+1,y+1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x+1,y-1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x+1,y,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x+1,y,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
            elif a.[x-1,y,z] <> 0y then
                if a.[x-2,y,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x-1,y+1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x-1,y-1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x-1,y,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x-1,y,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
            elif a.[x,y+1,z] <> 0y then
                if a.[x,y+2,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x+1,y+1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x-1,y+1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y+1,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y+1,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
            elif a.[x,y-1,z] <> 0y then
                if a.[x,y-2,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x+1,y-1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x-1,y-1,z] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y-1,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y-1,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
            elif a.[x,y,z+1] <> 0y then
                if a.[x,y,z+2] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x+1,y,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x-1,y,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y+1,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y-1,z+1] > 0y then neighborNeighbors <- neighborNeighbors + 1
            elif a.[x,y,z-1] <> 0y then
                if a.[x,y,z-2] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x+1,y,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x-1,y,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y+1,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
                if a.[x,y-1,z-1] > 0y then neighborNeighbors <- neighborNeighbors + 1
            if neighborNeighbors <= 1 then
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
    let endpoints = new System.Collections.Generic.HashSet<_>()
    let recentlyRemoved = new System.Collections.Generic.HashSet<_>()
    let iter = ref 0
    let decrement() =
        let rrSnapshot = recentlyRemoved|> Seq.toArray 
        for x,y,z in rrSnapshot do
            a.[x,y,z] <- a.[x,y,z] - 1y
            if a.[x,y,z] <= -5y then
                a.[x,y,z] <- 0y
                recentlyRemoved.Remove(x,y,z) |> ignore
    let ones = 
        if initOnes = null then
            let ones = new System.Collections.Generic.HashSet<_>()
            for x = a.GetLowerBound(0) to a.GetLowerBound(0)+a.GetLength(0)-1 do
                for y = a.GetLowerBound(1) to a.GetLowerBound(1)+a.GetLength(1)-1 do
                    for z = a.GetLowerBound(2) to a.GetLowerBound(2)+a.GetLength(2)-1 do
                        if a.[x,y,z] = 1y then
                            ones.Add(x,y,z) |> ignore
            ones
        else
            initOnes
    let removeCore(x,y,z) =
        a.[x,y,z] <- -1y
        onRemove(x,y,z,!iter%16)
        recentlyRemoved.Add(x,y,z) |> ignore
    let remove(x,y,z) =
        removeCore(x,y,z)
        ones.Remove(x,y,z) |> ignore
    // TODO endpoint short dist from 3-junction skeleton, can just trim that bit in post
    // do an initial smoothing, by pruning immediate endpoints
    let onesSnapshot = ones |> Seq.toArray 
    for x,y,z in onesSnapshot do
        if isEndpoint(x,y,z)<>NON_ENDPOINT then
            a.[x,y,z] <- 0y
            ones.Remove(x,y,z) |> ignore
    printfn "SKEL"
    let mutable ok = true
    // TODO note, two parallel spines diagonal from one another (touch diagonally), way to get rid of?
    while ok do
        let mutable wereAnyRemoved  = false
        for d = 5 downto 0 do  // prefer dy to be negative before positive, so that skeletons are biased to be lower
            printf "."
            let dx, dy, dz, b = 
                if d <= 2 then // + dirs
                    DIRS.[d]
                else           // - dirs
                    let dx, dy, dz, b = DIRS.[d-3]
                    let dx, dy, dz = -dx, -dy, -dz
                    dx,dy,dz,b
            // remove any naive bumps
            if ones.RemoveWhere(fun(x,y,z) ->   // RemoveWhere does not require creating a snapshot to iterate-and-remove
                let e = isEndpoint(x,y,z)
                if e = NAIVE_ENDPOINT then
                    removeCore(x,y,z)
                    true
                else
                    false                
                ) > 0 then
                    wereAnyRemoved <- true
            // main algorithm
            let onesSnapshot = ones |> Seq.toArray    // TODO cheaper way to keep track of snapshots/diffs?
            //Array.sortInPlace onesSnapshot 
            for x,y,z in onesSnapshot do
                let e = isEndpoint(x,y,z)
                if e = TRUE_ENDPOINT then
                    a.[x,y,z] <- 3y
                    ones.Remove(x,y,z) |> ignore
                    endpoints.Add(x,y,z) |> ignore
                elif e = NAIVE_ENDPOINT then
                    remove(x,y,z)
                    wereAnyRemoved <- true
                elif find(x-dx,y-dy,z-dz,dx,dy,dz,"OXX") then
                    // don't introduce concavities
                    //    XXX                      XXX
                    //  ->XXX   should not become  OXX 
                    //    XXX                      XXX
                    if  a.[x+1,y,z] > 0y && a.[x-1,y,z] > 0y ||
                        a.[x,y+1,z] > 0y && a.[x,y-1,z] > 0y ||
                        a.[x,y,z+1] > 0y && a.[x,y,z-1] > 0y then
                        () // do nothing to avoid adding concavity
                    // 
                    //       XO                                                                   XX
                    //      OXX                                                                    X
                    //  L-shaped connector                                                      already connected in another plane
                    elif find(x+b.[0].[0],y+b.[0].[1],z+b.[0].[2],dx,dy,dz,"XO") && not(find(x+b.[0].[0]+b.[2].[0],y+b.[0].[1]+b.[2].[1],z+b.[0].[2]+b.[2].[2],dx,dy,dz,"XX") && find(x+dx+b.[2].[0],y+dy+b.[2].[1],z+dz+b.[2].[2],dx,dy,dz,"X") || find(x+b.[0].[0]+b.[3].[0],y+b.[0].[1]+b.[3].[1],z+b.[0].[2]+b.[3].[2],dx,dy,dz,"XX") && find(x+dx+b.[3].[0],y+dy+b.[3].[1],z+dz+b.[3].[2],dx,dy,dz,"X")) ||
                         find(x+b.[1].[0],y+b.[1].[1],z+b.[1].[2],dx,dy,dz,"XO") && not(find(x+b.[1].[0]+b.[2].[0],y+b.[1].[1]+b.[2].[1],z+b.[1].[2]+b.[2].[2],dx,dy,dz,"XX") && find(x+dx+b.[2].[0],y+dy+b.[2].[1],z+dz+b.[2].[2],dx,dy,dz,"X") || find(x+b.[1].[0]+b.[3].[0],y+b.[1].[1]+b.[3].[1],z+b.[1].[2]+b.[3].[2],dx,dy,dz,"XX") && find(x+dx+b.[3].[0],y+dy+b.[3].[1],z+dz+b.[3].[2],dx,dy,dz,"X")) ||
                         find(x+b.[2].[0],y+b.[2].[1],z+b.[2].[2],dx,dy,dz,"XO") && not(find(x+b.[2].[0]+b.[0].[0],y+b.[2].[1]+b.[0].[1],z+b.[2].[2]+b.[0].[2],dx,dy,dz,"XX") && find(x+dx+b.[0].[0],y+dy+b.[0].[1],z+dz+b.[0].[2],dx,dy,dz,"X") || find(x+b.[2].[0]+b.[1].[0],y+b.[2].[1]+b.[1].[1],z+b.[2].[2]+b.[1].[2],dx,dy,dz,"XX") && find(x+dx+b.[1].[0],y+dy+b.[1].[1],z+dz+b.[1].[2],dx,dy,dz,"X")) ||
                         find(x+b.[3].[0],y+b.[3].[1],z+b.[3].[2],dx,dy,dz,"XO") && not(find(x+b.[3].[0]+b.[0].[0],y+b.[3].[1]+b.[0].[1],z+b.[3].[2]+b.[0].[2],dx,dy,dz,"XX") && find(x+dx+b.[0].[0],y+dy+b.[0].[1],z+dz+b.[0].[2],dx,dy,dz,"X") || find(x+b.[3].[0]+b.[1].[0],y+b.[3].[1]+b.[1].[1],z+b.[3].[2]+b.[1].[2],dx,dy,dz,"XX") && find(x+dx+b.[1].[0],y+dy+b.[1].[1],z+dz+b.[1].[2],dx,dy,dz,"X")) then
                            //a.[x,y,z] <- 2y
                            //ones.Remove(x,y,z) |> ignore
                            () // don't permanently save connectors, as the things they connect may be eroded away, just skip for now
                    else
                        remove(x,y,z)
                        wereAnyRemoved <- true
            decrement()
        if not wereAnyRemoved then
            ok <- false
        incr iter
    printfn ""
    // a is skeletonized, 3s are endpoints    
    let mutable endpointsWithLengths = ResizeArray()
    let ALL = [| (1,0,0); (0,1,0); (0,0,1); (-1,0,0); (0,-1,0); (0,0,-1) |]
    for x,y,z in endpoints do
        let mutable skip = -1
        let mutable countx, county, countz = 0, 0, 0
        let mutable ok = true
        let mutable cx,cy,cz = x,y,z
        while ok do
            let mutable next = None
            for i = 0 to 5 do
                if i <> skip then
                    let dx,dy,dz = ALL.[i]
                    if a.[cx+dx,cy+dy,cz+dz] > 0y then
                        match next with
                        | None -> 
                            next <- Some(cx+dx,cy+dy,cz+dz,(i + 3) % 6)
                            if dx <> 0 then 
                                countx <- countx + 1
                            if dy <> 0 then 
                                county <- county + 1
                            if dx <> 0 then 
                                countz <- countz + 1
                        | _ -> ok <- false
            match next with
            | Some(i,j,k,nextSkip) ->
                cx <- i
                cy <- j
                cz <- k
                skip <- nextSkip
            | _ -> 
                // a segment not connected to a more branching skeleton, just choosing to ignore
                ok <- false
                countx <- -1
                county <- -1
                countz <- -1
        endpointsWithLengths.Add(countx, county, countz, (x,y,z))
    //endpointsWithLengths.Sort()
    //for l,e in endpointsWithLengths do
    //    printfn "EPwL: %3d  %A" l e
    ones, endpoints, endpointsWithLengths 
