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
//    | IntArrays of int[][]
    
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
    member this.ToString(prefix) =
        match this with
        | End -> ""
        | Byte(n,x) -> prefix + n + " : " + (x.ToString())
        | Short(n,x) -> prefix + n + " : " + (x.ToString())
        | Int(n,x) -> prefix + n + " : " + (x.ToString())
        | Long(n,x) -> prefix + n + " : " + (x.ToString())
        | Float(n,x) -> prefix + n + " : " + (x.ToString())
        | Double(n,x) -> prefix + n + " : " + (x.ToString())
        | ByteArray(n,_a) -> prefix + n + " : <bytes>"
        | String(n,s) -> prefix + n + " : " + s
        | List(n,pay) -> 
            let sb = new System.Text.StringBuilder(prefix + n + " : []\n")
            let p = "    " + prefix
            match pay with
            | Bytes(a) -> for x in a do sb.Append(x.ToString(p) + "\n") |> ignore
            | Shorts(a) -> for x in a do sb.Append(x.ToString(p) + "\n") |> ignore
            | Ints(a) -> for x in a do sb.Append(x.ToString(p) + "\n") |> ignore
            | Longs(a) -> for x in a do sb.Append(x.ToString(p) + "\n") |> ignore
            | Floats(a) -> for x in a do sb.Append(x.ToString(p) + "\n") |> ignore
            | Doubles(a) -> for x in a do sb.Append(x.ToString(p) + "\n") |> ignore
            | Compounds(a) -> for c in a do for x in c do sb.Append(x.ToString(p) + "\n") |> ignore
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
                | _ -> failwith "unimplemented list kind"
            List(n,payload)
        | 10uy ->
            let n = NBT.ReadName(s)
            Compound(n, readCompoundPayload())
        | 11uy -> let n = NBT.ReadName(s) in let len = s.ReadInt32() in let a = Array.init len (fun _ -> s.ReadInt32()) in IntArray(n,a)
        | _ -> failwith "bad NBT tag"

type RegionFile private () =
    static member ReadChunk(filename, cx, cz) =
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
        // a set of 4KB sectors
        let chunkOffsetTable = Array.zeroCreate 1024 : int[]
        //let timeStampInfo = Array.zeroCreate 1024 : int[]
        use br = new BinaryReader2(file)
        for i = 0 to 1023 do
            chunkOffsetTable.[i] <- br.ReadInt32()
        let offset = chunkOffsetTable.[cx+32*cz]
        let sectorNumber = offset >>> 8
        let _numSectors = offset &&& 0xFF
        br.BaseStream.Seek(int64 (sectorNumber * 4096), System.IO.SeekOrigin.Begin) |> ignore
        let length = br.ReadInt32()
        let _version = br.ReadByte()
        // If you prefer to read Zlib-compressed chunk data with Deflate (RFC1951), just skip the first two bytes and leave off the last 4 bytes before decompressing.
        let _dummy1 = br.ReadByte()
        let _dummy2 = br.ReadByte()
        let bytes = br.ReadBytes(length - 6)
        use s = new System.IO.Compression.DeflateStream(new System.IO.MemoryStream(bytes) , System.IO.Compression.CompressionMode.Decompress)
        let nbt = NBT.Read(new BinaryReader2(s))
        nbt

let main() =
    let filename = """F:\.minecraft\saves\BingoGood\region\r.0.0.mca"""
    // I want to read chunk (11,11), which is coords (192,192) and is the top left of my bingo map
    let nbt = RegionFile.ReadChunk(filename, 11, 11)
    printfn "%s" (nbt.ToString())
    ()

printfn "hi"
main()

