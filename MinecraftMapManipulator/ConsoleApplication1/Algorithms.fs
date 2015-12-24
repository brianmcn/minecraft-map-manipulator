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