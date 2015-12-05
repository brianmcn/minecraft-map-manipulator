module NBT_Manipulation

type BinaryReader2(stream : System.IO.Stream) = // Big Endian
    inherit System.IO.BinaryReader(stream)
    override __.ReadInt32() = 
        let a32 = base.ReadBytes(4)
        System.Array.Reverse(a32)
        System.BitConverter.ToInt32(a32,0)
    override __.ReadInt16() = 
        let a16 = base.ReadBytes(2)
        System.Array.Reverse(a16)
        System.BitConverter.ToInt16(a16,0)
    override __.ReadInt64() = 
        let a64 = base.ReadBytes(8)
        System.Array.Reverse(a64)
        System.BitConverter.ToInt64(a64,0)
    override __.ReadDouble() = 
        let a64 = base.ReadBytes(8)
        System.Array.Reverse(a64)
        System.BitConverter.ToDouble(a64,0)
    override __.ReadUInt32() = 
        let a32 = base.ReadBytes(4)
        System.Array.Reverse(a32)
        System.BitConverter.ToUInt32(a32,0)

type BinaryWriter2(stream : System.IO.Stream) = // Big Endian
    inherit System.IO.BinaryWriter(stream)
    override __.Write(x:int) = 
        let a = System.BitConverter.GetBytes(x)
        System.Array.Reverse(a)
        base.Write(a)
    override __.Write(x:int16) = 
        let a = System.BitConverter.GetBytes(x)
        System.Array.Reverse(a)
        base.Write(a)
    override __.Write(x:int64) = 
        let a = System.BitConverter.GetBytes(x)
        System.Array.Reverse(a)
        base.Write(a)
    override __.Write(x:double) = 
        let a = System.BitConverter.GetBytes(x)
        System.Array.Reverse(a)
        base.Write(a)
    override __.Write(x:uint32) = 
        let a = System.BitConverter.GetBytes(x)
        System.Array.Reverse(a)
        base.Write(a)

let END_NAME = "--End--"

type Name = string

type Payload =
    | Bytes of byte[]
    | Shorts of int16[]
    | Ints of int[]
    | Longs of int64[]
    | Floats of single[]
    | Doubles of double[]
    | Strings of string[]
    | Compounds of NBT[][]
    | IntArrays of int[][]
    
and NBT =
    | End
    | Byte of Name * byte
    | Short of Name * int16
    | Int of Name * int
    | Long of Name * int64
    | Float of Name * single
    | Double of Name * double
    | ByteArray of Name * byte[]
    | String of Name * string // int16 length beforehand
    | List of Name * Payload // (name,kind,num,a)
    | Compound of Name * ResizeArray<NBT>
    | IntArray of Name * int[] // int32 before data shows num elements
    member this.Name =
        match this with
        | End -> END_NAME
        | Byte(n,_) -> n
        | Short(n,_) -> n
        | Int(n,_) -> n
        | Long(n,_) -> n
        | Float(n,_) -> n
        | Double(n,_) -> n
        | ByteArray(n,_) -> n
        | String(n,_) -> n
        | List(n,_) -> n
        | Compound(n,_) -> n
        | IntArray(n,_) -> n
    member this.TryGetFromCompound(s:string) =
        match this with
        | Compound(_n,a) -> a |> Seq.tryFind (fun x -> x.Name = s)
        | _ -> failwith "try to name-index into a non-compound"
    member this.Item(s:string) =
        match this.TryGetFromCompound(s) with
        | Some x -> x
        | None -> failwithf "tag %s not found" s
    member this.ToString(prefix) =
        match this with
        | End -> ""
        | Byte(n,x) -> prefix + n + " : " + (x.ToString())
        | Short(n,x) -> prefix + n + " : " + (x.ToString())
        | Int(n,x) -> prefix + n + " : " + (x.ToString())
        | Long(n,x) -> prefix + n + " : " + (x.ToString())
        | Float(n,x) -> prefix + n + " : " + (x.ToString())
        | Double(n,x) -> prefix + n + " : " + (x.ToString())
        | ByteArray(n,a) -> prefix + n + " : <" + (if a |> Array.exists (fun b -> b <> 0uy) then "bytes>" else "all zero bytes>")
        | String(n,s) -> prefix + n + " : " + s
        | List(n,pay) -> 
            //if n = "TileTicks" then prefix + n + " : [] (NOTE: skipping data)" else
            let sb = new System.Text.StringBuilder(prefix + n + " : [")
            let p = "    " + prefix
            match pay with
            | Bytes(a) -> sb.Append(a.Length.ToString() + " bytes]") |> ignore
            | Shorts(a) -> sb.Append(a.Length.ToString() + " shorts]") |> ignore
            | Ints(a) -> 
                if a.Length < 5 then
                    sb.Append((a |> Array.map (sprintf "%d") |> String.concat ", ") + "]") |> ignore
                else
                    sb.Append(a.Length.ToString() + " ints]") |> ignore
            | Longs(a) -> sb.Append(a.Length.ToString() + " longs]") |> ignore
            | Floats(a) -> 
                if a.Length < 5 then
                    sb.Append((a |> Array.map (sprintf "%g") |> String.concat ", ") + "]") |> ignore
                else
                    sb.Append(a.Length.ToString() + " floats]") |> ignore 
            | Doubles(a) -> 
                if a.Length < 5 then
                    sb.Append((a |> Array.map (sprintf "%g") |> String.concat ", ") + "]") |> ignore
                else
                    sb.Append(a.Length.ToString() + " doubles]") |> ignore
            | Strings(a) -> 
                sb.Append("] ") |> ignore
                for s in a do sb.Append("\""+s+"\"  ") |> ignore
            | Compounds(a) -> 
                sb.Append("] ") |> ignore
                let mutable first = true
                for c in a do 
                    if first then
                        first <- false
                    else
                        sb.Append("\n" + p + "----") |> ignore
                    for x in c do 
                        if x <> End then sb.Append("\n" + x.ToString(p)) |> ignore
            | IntArrays(a) -> sb.Append(a.Length.ToString() + " int arrays]") |> ignore
            sb.ToString()
        | Compound(n,a) -> 
            let sb = new System.Text.StringBuilder(prefix + n + " :\n")
            let p = "    " + prefix
            for x in a do
                sb.Append(x.ToString(p) + "\n") |> ignore
            sb.ToString()
        | IntArray(n,a) -> prefix + n + if a.Length > 6 then " : <ints>" else (a |> Array.fold (fun s x -> s + (x.ToString()) + " ") " : [ ") + "]"
    override this.ToString() =
        this.ToString("")
    static member ReadName(s : BinaryReader2) =
        let length = s.ReadInt16()
        let utf8bytes = s.ReadBytes(int length)
        System.Text.Encoding.UTF8.GetString(utf8bytes)
    static member Read(s : BinaryReader2) =
        let readCompoundPayload() =
            let nbts = new ResizeArray<NBT>()
            let mutable dun = false
            while not dun do
                let x = NBT.Read(s)
                nbts.Add(x)
                if x = End then
                    dun <- true
            nbts.ToArray()
        match s.ReadByte() with
        | 0uy -> End
        | 1uy -> let n = NBT.ReadName(s) in let x = s.ReadByte() in Byte(n,x)
        | 2uy -> let n = NBT.ReadName(s) in let x = s.ReadInt16() in Short(n,x)
        | 3uy -> let n = NBT.ReadName(s) in let x = s.ReadInt32() in Int(n,x)
        | 4uy -> let n = NBT.ReadName(s) in let x = s.ReadInt64() in Long(n,x)
        | 5uy -> let n = NBT.ReadName(s) in let x = s.ReadSingle() in Float(n,x)
        | 6uy -> let n = NBT.ReadName(s) in let x = s.ReadDouble() in Double(n,x)
        | 7uy -> let n = NBT.ReadName(s) in let len = s.ReadInt32() in let a = s.ReadBytes(len) in ByteArray(n,a)
        | 8uy -> let n = NBT.ReadName(s) in let x = NBT.ReadName(s) in String(n,x)
        | 9uy -> 
            let n = NBT.ReadName(s) 
            let kind = s.ReadByte() 
            let len = s.ReadInt32() 
            let payload =
                match kind with
                | 0uy -> assert(len=0); Compounds [||]
                | 1uy -> Bytes(Array.init len (fun _ -> s.ReadByte()))
                | 2uy -> Shorts(Array.init len (fun _ -> s.ReadInt16()))
                | 3uy -> Ints(Array.init len (fun _ -> s.ReadInt32()))
                | 4uy -> Longs(Array.init len (fun _ -> s.ReadInt64()))
                | 5uy -> Floats(Array.init len (fun _ -> s.ReadSingle()))
                | 6uy -> Doubles(Array.init len (fun _ -> s.ReadDouble()))
                | 8uy -> Strings(Array.init len (fun _ -> NBT.ReadName(s)))
                | 10uy ->let r = Compounds(Array.init len (fun _ -> readCompoundPayload()))
                         //if n = "TileEntities" then printfn "read %d TEs" (match r with Compounds(a) -> a.Length)
                         r
                | 11uy ->IntArrays(Array.init len (fun _ -> let innerLen = s.ReadInt32() in Array.init innerLen (fun _ -> s.ReadInt32())))
                | _ -> failwith "unimplemented list kind"
            List(n,payload)
        | 10uy ->
            let n = NBT.ReadName(s)
            Compound(n, readCompoundPayload() |> ResizeArray)
        | 11uy -> let n = NBT.ReadName(s) in let len = s.ReadInt32() in let a = Array.init len (fun _ -> s.ReadInt32()) in IntArray(n,a)
        | bb -> failwithf "bad NBT tag: %d" bb
    static member WriteName(bw : BinaryWriter2, n : string) =
        let a = System.Text.Encoding.UTF8.GetBytes(n)
        bw.Write(int16 a.Length)  // not n.Length! utf encoding may change it, we need byte count!
        bw.Write(a)
    member this.Write(bw : BinaryWriter2) =
        match this with
        | End -> bw.Write(0uy)
        | Byte(n,x) -> bw.Write(1uy); NBT.WriteName(bw,n); bw.Write(x)
        | Short(n,x) -> bw.Write(2uy); NBT.WriteName(bw,n); bw.Write(x)
        | Int(n,x) -> bw.Write(3uy); NBT.WriteName(bw,n); bw.Write(x)
        | Long(n,x) -> bw.Write(4uy); NBT.WriteName(bw,n); bw.Write(x)
        | Float(n,x) -> bw.Write(5uy); NBT.WriteName(bw,n); bw.Write(x)
        | Double(n,x) -> bw.Write(6uy); NBT.WriteName(bw,n); bw.Write(x)
        | ByteArray(n,a) -> bw.Write(7uy); NBT.WriteName(bw,n); bw.Write(a.Length); bw.Write(a)
        | String(n,x) -> bw.Write(8uy); NBT.WriteName(bw,n); NBT.WriteName(bw,x)
        | List(n,pay) -> 
            bw.Write(9uy)
            NBT.WriteName(bw,n)
            match pay with
            | Bytes(a) -> bw.Write(1uy); bw.Write(a.Length); bw.Write(a)
            | Shorts(a) -> bw.Write(2uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Ints(a) -> bw.Write(3uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Longs(a) -> bw.Write(4uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Floats(a) -> bw.Write(5uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Doubles(a) -> bw.Write(6uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Strings(a) -> bw.Write(8uy); bw.Write(a.Length); for x in a do NBT.WriteName(bw,x)
            | Compounds(a) -> bw.Write(10uy); bw.Write(a.Length); 
                              //if n = "TileEntities" then printfn "%d" a.Length
                              for x in a do (for y in x do y.Write(bw); assert(x.[x.Length-1] = End))
            | IntArrays(a) -> bw.Write(11uy); bw.Write(a.Length); for x in a do for y in x do bw.Write(y)
        | Compound(n,xs) -> bw.Write(10uy); NBT.WriteName(bw,n); for x in xs do x.Write(bw); assert(xs.[xs.Count-1] = End)
        | IntArray(n,xs) -> bw.Write(11uy); NBT.WriteName(bw,n); bw.Write(xs.Length); for x in xs do bw.Write(x)
    member this.Diff(other : NBT) =
        let rec diff(x,y,path) =
            match x with
            | End | Byte _ | Short _ | Int _ | Long _ | Float _ | Double _ | ByteArray _ | String _ | IntArray _ ->
                 if LanguagePrimitives.GenericEqualityER x y then None else Some(path,x,y)  // all structural leaf nodes
            | List(xn,_) ->
                if x=y then None else 
                match y with 
                | List(yn,_) -> if xn=yn then paydiff(x,y,xn::path) else Some(path,x,y)
                | _ -> Some(path,x,y)
            | Compound(xn,xs) ->
                if x=y then None else 
                match y with 
                | Compound(yn,ys) -> 
                    if xn=yn && xs.Count=ys.Count then 
                        (None,xs,ys) |||> Seq.fold2 (fun s xx yy -> match s with None -> diff(xx,yy,xn::path) | s -> s) 
                    else Some(path,x,y)
                | _ -> Some(path,x,y)
        and paydiff((List(_,xs) as x), (List(_,ys) as y), path) =
            match xs with
            | Bytes _ | Shorts _ | Ints _ | Longs _ | Floats _ | Doubles _ | Strings _ | IntArrays _ ->
                 if LanguagePrimitives.GenericEqualityER xs ys then None else Some(path,x,y)  // all structural leaf nodes
            | Compounds(xss) ->
                if xs=ys then None else 
                match ys with 
                | Compounds(yss) -> 
                    if xss.Length=yss.Length then 
                        (None,xss,yss) |||> Array.fold2 (fun s xx yy -> 
                            match s with 
                            | None -> (None,xx,yy) |||> Array.fold2 (fun s xxx yyy -> match s with None -> diff(xxx,yyy,path) | s -> s)
                            | s -> s) 
                    else Some(path,x,y)
                | _ -> Some(path,x,y)
        match diff(this,other,[]) with
        | None -> None
        | Some(path,a,b) -> Some(path |> List.fold (fun s x -> ":" + x + s) "", a, b)

/////////////////////////////////////////////////////////////////////////////

let rec cataNBT f g nbt =
    match nbt with
    | End
    | Byte _
    | Short _
    | Int _
    | Long _
    | Float _
    | Double _
    | ByteArray _
    | String _
    | IntArray _ -> f nbt
    | List(n,pay) -> f (List(n, cataPayload f g pay))
    | Compound(n,a) -> f (Compound(n, a |> Seq.map (cataNBT f g) |> ResizeArray))
and cataPayload f g pay =
    match pay with
    | Bytes _
    | Shorts _
    | Ints _
    | Longs _
    | Floats _
    | Doubles _
    | Strings _
    | IntArrays _ -> g pay
    | Compounds(aa) ->
        g(Compounds(aa |> Array.map (Array.map (cataNBT f g))))

/////////////////////////////////////////////////////////////////////////////

let SimpleDisplay nbt =
    match nbt with
    | End -> failwith "bad SimpleDisplay"
    | Byte(_,x) -> "<byte> " + x.ToString()
    | Short(_,x) -> "<short> " + x.ToString()
    | Int(_,x) -> "<int> " + x.ToString()
    | Long(_,x) -> "<long> " + x.ToString()
    | Float(_,x) -> "<float> " + x.ToString()
    | Double(_,x) -> "<double> " + x.ToString()
    | ByteArray(_,_) -> "<byte array>"
    | String(_,x) -> x
    | List(_,_) -> null
    | Compound(_,_) -> null
    | IntArray(_,_) -> "<int array>"
   
