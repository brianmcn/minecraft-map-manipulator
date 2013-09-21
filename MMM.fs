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
    override __.Write(x:uint32) = 
        let a = System.BitConverter.GetBytes(x)
        System.Array.Reverse(a)
        base.Write(a)

type Name = string

type Payload =
    // | Ends
    | Bytes of byte[]
    | Shorts of int16[]
    | Ints of int[]
    | Longs of int64[]
    | Floats of single[]
    | Doubles of double[]
//    | ByteArrays of byte[][]
//    | Strings of string[]
    //| Lists
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
    | Compound of Name * NBT[]
    | IntArray of Name * int[] // int32 before data shows num elements
    member this.Name =
        match this with
        | End -> "End"
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
    member this.Item(s:string) =
        match this with
        | Compound(_n,a) ->
            match a |> Array.tryFind (fun x -> x.Name = s) with
            | Some x -> x
            | None -> failwithf "tag %s not found" s
        | _ -> failwith "try to name-index into a non-compound"
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
            if n = "TileTicks" then prefix + n + " : [] (NOTE: skipping data)" else
            let sb = new System.Text.StringBuilder(prefix + n + " : [] ")
            let p = "    " + prefix
            match pay with
            | Bytes(a) -> sb.Append(a.Length.ToString() + " bytes") |> ignore
            | Shorts(a) -> sb.Append(a.Length.ToString() + " shorts") |> ignore
            | Ints(a) -> sb.Append(a.Length.ToString() + " ints") |> ignore
            | Longs(a) -> sb.Append(a.Length.ToString() + " longs") |> ignore
            | Floats(a) -> sb.Append(a.Length.ToString() + " floats") |> ignore
            | Doubles(a) -> sb.Append(a.Length.ToString() + " doubles") |> ignore
            | Compounds(a) -> 
                let mutable first = true
                for c in a do 
                    if first then
                        first <- false
                    else
                        sb.Append("\n" + p + "----") |> ignore
                    for x in c do 
                        if x <> End then sb.Append("\n" + x.ToString(p)) |> ignore
            | IntArrays(a) -> sb.Append(a.Length.ToString() + " int arrays") |> ignore
            sb.ToString()
        | Compound(n,a) -> 
            let sb = new System.Text.StringBuilder(prefix + n + " :\n")
            let p = "    " + prefix
            for x in a do
                sb.Append(x.ToString(p) + "\n") |> ignore
            sb.ToString()
        | IntArray(n,_a) -> prefix + n + " : <ints>"
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
                | 1uy -> Bytes(Array.init len (fun _ -> s.ReadByte()))
                | 2uy -> Shorts(Array.init len (fun _ -> s.ReadInt16()))
                | 3uy -> Ints(Array.init len (fun _ -> s.ReadInt32()))
                | 4uy -> Longs(Array.init len (fun _ -> s.ReadInt64()))
                | 5uy -> Floats(Array.init len (fun _ -> s.ReadSingle()))
                | 6uy -> Doubles(Array.init len (fun _ -> s.ReadDouble()))
                | 10uy ->Compounds(Array.init len (fun _ -> readCompoundPayload()))
                | 11uy ->IntArrays(Array.init len (fun _ -> let innerLen = s.ReadInt32() in Array.init innerLen (fun _ -> s.ReadInt32())))
                | _ -> failwith "unimplemented list kind"
            List(n,payload)
        | 10uy ->
            let n = NBT.ReadName(s)
            Compound(n, readCompoundPayload())
        | 11uy -> let n = NBT.ReadName(s) in let len = s.ReadInt32() in let a = Array.init len (fun _ -> s.ReadInt32()) in IntArray(n,a)
        | _ -> failwith "bad NBT tag"
    static member WriteName(bw : BinaryWriter2, n : string) =
        bw.Write(int16 n.Length)
        let a = System.Text.Encoding.UTF8.GetBytes(n)
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
        | ByteArray(n,a) -> bw.Write(7uy); NBT.WriteName(bw,n); bw.Write(a)
        | String(n,x) -> bw.Write(8uy); NBT.WriteName(bw,n); NBT.WriteName(bw,x)
        | List(n,pay) -> 
            bw.Write(9uy); 
            NBT.WriteName(bw,n)
            match pay with
            | Bytes(a) -> bw.Write(1uy); bw.Write(a.Length); bw.Write(a)
            | Shorts(a) -> bw.Write(2uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Ints(a) -> bw.Write(3uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Longs(a) -> bw.Write(4uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Floats(a) -> bw.Write(5uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Doubles(a) -> bw.Write(6uy); bw.Write(a.Length); for x in a do bw.Write(x)
            //| ByteArrays(a) -> bw.Write(7uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Compounds(a) -> bw.Write(10uy); bw.Write(a.Length); for x in a do for y in x do y.Write(bw)
            | IntArrays(a) -> bw.Write(11uy); bw.Write(a.Length); for x in a do for y in x do bw.Write(y)
        | Compound(n,xs) -> bw.Write(10uy); NBT.WriteName(bw,n); for x in xs do x.Write(bw) // end is in-memory represented, so written out too
        | IntArray(n,xs) -> bw.Write(11uy); NBT.WriteName(bw,n); bw.Write(xs.Length); for x in xs do bw.Write(x)

type RegionFile(filename) =
    let chunks : NBT[,] = Array2D.create 32 32 End  // End represents a blank (unrepresented) chunk
    let chunkTimestampInfos : int[,] = Array2D.zeroCreate 32 32
    do
        // a set of 4KB sectors
        let chunkOffsetTable = Array.zeroCreate 1024 : int[]
        let timeStampInfo = Array.zeroCreate 1024 : int[]
        use file = new System.IO.FileStream(filename, System.IO.FileMode.Open)
        (*
         The chunk offset for a chunk (x, z) begins at byte 4*(x+z*32) in the
         file. The bottom byte of the chunk offset indicates the number of sectors the
         chunk takes up, and the top 3 bytes represent the sector number of the chunk.
         Given a chunk offset o, the chunk data begins at byte 4096*(o/256) and takes up
         at most 4096*(o%256) bytes. A chunk cannot exceed 1MB in size. If a chunk
         offset is 0, the corresponding chunk is not stored in the region file.

         Chunk data begins with a 4-byte big-endian integer representing the chunk data
         length in bytes, not counting the length field. The length must be smaller than
         4096 times the number of sectors. The next byte is a version field, to allow
         backwards-compatible updates to how chunks are encoded.
         *)
        use br = new BinaryReader2(file)
        for i = 0 to 1023 do
            chunkOffsetTable.[i] <- br.ReadInt32()
        for i = 0 to 1023 do
            timeStampInfo.[i] <- br.ReadInt32()
        for cx = 0 to 31 do
            for cz = 0 to 31 do
                let offset = chunkOffsetTable.[cx+32*cz]
                if offset <> 0 then
                    chunkTimestampInfos.[cx,cz] <- timeStampInfo.[cx+32*cz]
                    let sectorNumber = offset >>> 8
                    let _numSectors = offset &&& 0xFF
                    br.BaseStream.Seek(int64 (sectorNumber * 4096), System.IO.SeekOrigin.Begin) |> ignore
                    let length = br.ReadInt32()
                    let _version = br.ReadByte()
                    // If you prefer to read Zlib-compressed chunk data with Deflate (RFC1951), just skip the first two bytes and leave off the last 4 bytes before decompressing.
                    let _dummy1 = br.ReadByte()
                    let _dummy2 = br.ReadByte()          // CMF and FLG are first two bytes, could remember these and just rewrite them
                    let bytes = br.ReadBytes(length - 6) // ADLER32 is last 4 bytes checksum, not hard to compute a new one to write back out
                    use s = new System.IO.Compression.DeflateStream(new System.IO.MemoryStream(bytes) , System.IO.Compression.CompressionMode.Decompress)
                    let nbt = NBT.Read(new BinaryReader2(s))
                    chunks.[cx,cz] <- nbt
    member this.Write(outputFilename) =
        let chunkOffsetTable = Array.zeroCreate 1024 : int[]
        let timeStampInfo = Array.zeroCreate 1024 : int[]
        let mutable nextFreeSection = 2  // sections 0 and 1 are chunk offset table and timestamp info table
        use file = new System.IO.FileStream(outputFilename, System.IO.FileMode.CreateNew)
        use bw = new BinaryWriter2(file)
        for cx = 0 to 31 do
            for cz = 0 to 31 do
                if chunks.[cx,cz] <> End then
                    let temp = Array.zeroCreate (1024*1024)
                    use s = new System.IO.Compression.DeflateStream(new System.IO.MemoryStream(temp) , System.IO.Compression.CompressionMode.Compress)
                    // TODO NBT.Write(new BinaryWriter2(s), chunks.[cx,cz])
                    let numBytes = int s.BaseStream.Length
                    s.Close()
                    let numSectionsNeeded = (numBytes + 6) / 4096
                    chunkOffsetTable.[cx+cz*32] <- (nextFreeSection <<< 8) + numSectionsNeeded
                    timeStampInfo.[cx+cz*32] <- chunkTimestampInfos.[cx,cz]  // no-op, not updating them
                    bw.Seek(nextFreeSection * 4096, System.IO.SeekOrigin.Begin) |> ignore
                    nextFreeSection <- nextFreeSection + numSectionsNeeded 
                    bw.Write(120uy) // CMF
                    bw.Write(156uy) // FLG
                    bw.Write(temp, 0, numBytes)  // stream
                    bw.Write(RegionFile.ComputeAdler(temp)) // adler checksum
        // TODO pad to 4096
        ()
    static member ComputeAdler(bytes : byte[]) : int =
        (*
                Adler-32 is composed of two sums accumulated per byte: s1 is
                the sum of all bytes, s2 is the sum of all s1 values. Both sums
                are done modulo 65521. s1 is initialized to 1, s2 to zero.  The
                Adler-32 checksum is stored as s2*65536 + s1 in most-
                significant-byte first (network) order.
        *)
        let mutable s1 = 1
        let mutable s2 = 0
        for i = 0 to bytes.Length - 1 do
            s1 <- (s1 + int(bytes.[i])) % 65521
            s2 <- (s2 + s1) % 65521
        s2*65536 + s1
    member this.GetChunk(cx, cz) =
        match chunks.[cx,cz] with
        | End -> failwith "chunk not represented, NYI"
        | c -> c
    member this.GetBlockIDAndBlockDataAndTileEntityOpt(x, y, z) =
        let theChunk = 
            match chunks.[x/16,z/16] with
            | End -> failwith "chunk not represented, NYI"
            | c -> c
        let theChunk = match theChunk with Compound(_,[|c;_|]) -> c // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
        let sections = match theChunk.["Sections"] with List(_,Compounds(cs)) -> cs
        let theSection = sections |> Array.find (Array.exists (function Byte("Y",n) when n=byte(y/16) -> true | _ -> false))
        let dx, dy, dz = x % 16, y % 16, z % 16
        let i = dy*256 + dz*16 + dx
        // BlockID
        let blocks = theSection |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
        // BlockData
        let blockData = theSection |> Array.pick (function ByteArray("Data",a) -> Some a | _ -> None)
        // expand 2048 half-bytes into 4096 for convenience of same indexing
        let blockData = Array.init 4096 (fun x -> if x%2=1 then blockData.[x/2] >>> 4 else blockData.[x/2] &&& 0xFuy)
        // TileEntities
        let tileEntity = 
            match theChunk.["TileEntities"] with 
            | List(_,Compounds(cs)) ->
                cs |> Array.tryPick (fun te -> 
                    let te = Compound("dummy",te)
                    if te.["x"]=Int("x",x%512) && te.["y"]=Int("y",y) && te.["z"]=Int("z",z%512) then Some te else None)
            | _ -> None
        blocks.[i], blockData.[i], tileEntity

let main2() =
    let filename = """F:\.minecraft\saves\FindHut\region\r.0.0.mca"""
    // I want to read chunk (10,13), which is coords (165,211) and is in the witch hut
    let regionFile = new RegionFile(filename)
    let nbt = regionFile.GetChunk(10, 13)
    printfn "%s" (nbt.ToString())
    let theChunk = match nbt with Compound(_,[|c;_|]) -> c
    let sections = match theChunk.["Sections"] with List(_,Compounds(cs)) -> cs
    // height of map I want to look at is 65, which is section 4
    let s = sections |> Array.find (Array.exists (function Byte("Y",4uy) -> true | _ -> false))
    for x in s do printfn "%s\n" (x.ToString())
    let blockID, blockData, tileEntityOpt = regionFile.GetBlockIDAndBlockDataAndTileEntityOpt(165,65,211)
    printfn "%A %A %A" blockID blockData tileEntityOpt  // planks at floor of hut

    let structures = [ //"VILLAGES", """F:\.minecraft\saves\FindHut\data\villages.dat"""
                       "TEMPLE", """F:\.minecraft\saves\FindHut\data\Temple.dat"""
                       //"MINESHAFT", """F:\.minecraft\saves\FindHut\data\Mineshaft.dat"""
                     ]
    for name, file in structures do
        printfn ""
        printfn "%s" name
        use s = new System.IO.Compression.GZipStream(new System.IO.FileStream(file, System.IO.FileMode.Open), System.IO.Compression.CompressionMode.Decompress)
        let vnbt = NBT.Read(new BinaryReader2(s))
        printfn "%s" (vnbt.ToString())

    (* Aha, yes, temples stores the witch hut
TEMPLE
 :
    data :
        Features :
            [10,13] :
                id : Temple
                ChunkX : 10
                BB : <ints>
                ChunkZ : 13
                Children : []
                    id : TeSH
                    GD : 0
                    HPos : 64
                    BB : <ints>
                    Height : 5
                    Witch : 1
                    Width : 7
                    O : 2
                    Depth : 9
    *)
    use s = new System.IO.Compression.GZipStream(new System.IO.FileStream("""F:\.minecraft\saves\FindHut\data\Temple.dat""", System.IO.FileMode.Open), System.IO.Compression.CompressionMode.Decompress)
    let temp = Array.zeroCreate 100000
    let mutable N = 0
    let mutable dun = false
    while not dun do
        let x = s.ReadByte()
        if x = -1 then
            dun <- true
        else
            temp.[N] <- byte x
            N <- N + 1
    s.Close()
    let decompressedBytes = temp.[0..N-1]
    let s = new System.IO.MemoryStream(decompressedBytes)
    let nbt = NBT.Read(new BinaryReader2(s))
    let theHut = nbt.["data"].["Features"].["[10,13]"]  // swamp hut
    //let theHut = nbt.["data"].["Features"].["[-60,-25]"]  // desert pyramid
    printfn "%s" (theHut.ToString())
    let (IntArray(_n,a)) = theHut.["BB"]
    for x in a do printf "%d  " x
    printfn ""
    (* 160  64  208  166  68  216 *)
    // Aha, BB is the bounding box
    (*
    let (List(_n,Compounds([|c|]))) = theHut.["Children"]
    let childBB = c |> Array.find (fun x -> x.Name = "BB")
    let (IntArray(_n,a)) = childBB
    for x in a do printf "%d  " x
    printfn ""
    (* 160  64  208  166  68  216 *)
    *)
    let ms = new System.IO.MemoryStream(temp)
    nbt.Write(new BinaryWriter2(ms))
    let p = int ms.Position 
    ms.Close()
    let writtenBytes = temp.[0..p-1]
    printfn "%d %d" decompressedBytes.Length writtenBytes.Length 
(*
    let k =
        let mutable i = 0
        let mutable dun = false
        while not dun do
            if decompressedBytes.[i] = writtenBytes.[i] then
                i <- i + 1
            else
                dun <- true
        i
    for b in decompressedBytes.[k-5 .. k+25] do
        printf "%3d " b
    printfn ""
    printfn "-------"
    for b in writtenBytes.[k-5 .. k+25] do
        printf "%3d " b
    printfn ""
*)
    //use s = new System.IO.Compression.GZipStream(new System.IO.FileStream("""F:\.minecraft\saves\FindHut\data\BriTemple.dat""", System.IO.FileMode.CreateNew), System.IO.Compression.CompressionMode.Compress)
    //nbt.Write(new BinaryWriter2(s))
    //s.Close()
(*
TEMPLE
 :
    data :
        Features :
            [10,13] :
                id : Temple
                ChunkX : 10
                BB : <ints>
                ChunkZ : 13
                Children : []
                    id : TeSH
                    GD : 0
                    HPos : 64
                    BB : <ints>
                    Height : 5
                    Witch : 1
                    Width : 7
                    O : 2
                    Depth : 9
*)
    let synthetic = 
        Compound("", [|
            Compound("data", [|
                Compound("Features", [|
                    Compound("[0,0]", [|
                        String("id", "Temple")
                        Int("ChunkX", 0)
                        Int("ChunkZ", 0)
                        IntArray("BB", [| 0; 1; 0; 400; 255; 400 |])
                        Compound("Children", [|
                            String("id", "TeSH")
                            Int("GD", 0) //?
                            Int("HPos", 1)
                            IntArray("BB", [| 0; 1; 0; 400; 255; 400 |])
                            Int("Height", 255)
                            Int("Width", 401)
                            Int("Depth", 401)
                            Int("Witch", 1) //?
                            Int("O", 2)  //?
                            |])
                        |])
                    |])
                |])
            |])
    printfn "%s" (synthetic.ToString())
    use s = new System.IO.Compression.GZipStream(new System.IO.FileStream("""F:\.minecraft\saves\FindHut\data\AllWitchesTemple.dat""", System.IO.FileMode.CreateNew), System.IO.Compression.CompressionMode.Compress)
    synthetic.Write(new BinaryWriter2(s))
    s.Close()


    

let main() =
    let filename = """F:\.minecraft\saves\BingoGood\region\r.0.0.mca"""
    // I want to read chunk (12,12), which is coords (192,192) and is the top left of my bingo map
    let regionFile = new RegionFile(filename)
    let nbt = regionFile.GetChunk(12, 12)
    printfn "%s" (nbt.ToString())
    let theChunk = match nbt with Compound(_,[|c;_|]) -> c
    printfn "%s" theChunk.Name 
    let sections = match theChunk.["Sections"] with List(_,Compounds(cs)) -> cs
    printfn "%d" sections.Length 
    // height of map I want to look at is just below 208, which is section 12
    let s = sections |> Array.find (Array.exists (function Byte("Y",12uy) -> true | _ -> false))
    for x in s do printfn "%s\n" (x.ToString())
    let blocks = s |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
    // 76 is active redstone torch blockID; 23 is dispenser
    printfn "red torch? %A" (blocks |> Array.exists (fun b -> b = 76uy))
    // dispenser in next higher section (13)... i could see its contents already in the TileEntities of the chunk
    let s = sections |> Array.find (Array.exists (function Byte("Y",13uy) -> true | _ -> false))
    let blocks = s |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
    printfn "dispenser? %A" (blocks |> Array.exists (fun b -> b = 23uy))
    let dispIndex = blocks |> Array.findIndex (fun b -> b = 23uy)
    let blockData = s |> Array.pick (function ByteArray("Data",a) -> Some a | _ -> None)
    // expand 2048 half-bytes into 4096 for convenience of same indexing without having to decode YZX
    let blockData = Array.init 4096 (fun x -> if x%2=1 then blockData.[x/2] >>> 4 else blockData.[x/2] &&& 0xFuy)
    printfn "disp orientation %d" blockData.[dispIndex]
(*
    for i = 0 to 4095 do
        printf "%d " blocks.[i]
        if i%16 = 15 then printfn ""
*)
    let blockID, blockData, tileEntityOpt = regionFile.GetBlockIDAndBlockDataAndTileEntityOpt(204,208,199)
    printfn "should be dispenser (23) here: %d" blockID
    printfn "should be facing up and powered (9) here: %d" blockData
    printfn "should have tile entity here: %s" (match tileEntityOpt with None -> "" | Some nbt -> nbt.ToString())
    ()

printfn "hi"
//main()
main2()

