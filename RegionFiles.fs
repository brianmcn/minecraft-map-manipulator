module RegionFiles

open NBT_Manipulation

type BlockInfo(blockID:byte, blockData:Lazy<byte>, tileEntity:NBT option) =
    member this.BlockID = blockID
    member this.BlockData = blockData
    member this.TileEntity = tileEntity

type CommandBlock =     // the useful subset I plan to map anything into
    | P of string  // purple (pointing positive Z, unconditional, auto:0)
    | O of string  // orange (pointing positive Z, unconditional, auto:0)
    | U of string  // green (pointing positive Z, unconditional, auto:1)
    | C of string  // green (pointing positive Z, conditional, auto:1)

type Coords(x:int,y:int,z:int) = 
    member this.X = x
    member this.Y = y
    member this.Z = z
    member this.STR = sprintf "%d %d %d" x y z
    member this.Offset(dx,dy,dz) = new Coords(x+dx, y+dy, z+dz)

[<Literal>]
let DUMMY = "say dummy"

let DIV(x,m) = (x+10000*m)/m - 10000
let MOD(x,m) = (x+10000*m)%m

type RegionFile(filename) =
    let rx, rz =
        let m = System.Text.RegularExpressions.Regex.Match(filename, """.*r\.(.*)\.(.*)\.mca(\.new|\.old)?$""")
        int m.Groups.[1].Value, int m.Groups.[2].Value
    let isChunkDirty = Array2D.create 32 32 false
    let chunkHeightMapCache : int[,][,] = Array2D.create 32 32 null   // chunkHeightMapCache.[cx,cz].[x,z], (TODO currently just read once, never updated)
    let chunkSectionsCache : NBT[][][,] = Array2D.init 32 32 (fun _ _ -> Array.zeroCreate 16)   // chunkSectionsCache.[cx,cz].[sy]
    let chunks : NBT[,] = Array2D.create 32 32 End  // End represents a blank (unrepresented) chunk
    let chunkTimestampInfos : int[,] = Array2D.zeroCreate 32 32
    let mutable firstSeenDataVersion = -1
    let getOrCreateChunk(cx,cz) =
        let chunk = 
            match chunks.[cx,cz] with
            | End ->
                let newChunk = Compound("",[|NBT.Compound("Level", [|
                                                                    NBT.Int("xPos",cx); NBT.Int("zPos",cz); NBT.Long("LastUpdate", 0L);
                                                                    NBT.Byte("LightPopulated",0uy); NBT.Byte("TerrainPopulated",1uy); // TODO best TerrainPopulated value?
                                                                    NBT.Byte("V",1uy); NBT.Long("InhabitedTime",0L);
                                                                    NBT.IntArray("HeightMap", Array.zeroCreate 256)
                                                                    // a number of things can be absent
                                                                    NBT.List("Sections", Compounds([||]))
                                                                    NBT.End
                                                                   |] |> ResizeArray
                                                         )
                                             NBT.Int("DataVersion",firstSeenDataVersion)
                                             NBT.End|] |> ResizeArray)
                chunks.[cx,cz] <- newChunk
                newChunk
            | c -> c
        if chunkHeightMapCache.[cx,cz] = null then
            let chunkLevel = match chunk with Compound(_,rsa) -> rsa.[0]  // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name (or two with a data version appended)
            match chunkLevel with 
            | Compound(n,nbts) -> 
                let i = nbts.FindIndex (fun nbt -> nbt.Name = "HeightMap")
                match nbts.[i] with
                | NBT.IntArray(_,flatArray) ->
                    let squareArray = Array2D.init 16 16 (fun x z -> flatArray.[z*16+x])
                    chunkHeightMapCache.[cx,cz] <- squareArray
        chunk
    let mutable numCommandBlocksPlaced = 0
    do
        if not(System.IO.File.Exists(filename)) then
            // create file if does not exist
            use file = new System.IO.FileStream(filename, System.IO.FileMode.CreateNew)
            use bw = new BinaryWriter2(file)
            bw.Seek(0, System.IO.SeekOrigin.Begin) |> ignore
            for i = 0 to 1023 do
                bw.Write(0)
            for i = 0 to 1023 do
                bw.Write(0)
            bw.Close()
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
                    if firstSeenDataVersion = -1 then
                        try
                            firstSeenDataVersion <- match nbt.["DataVersion"] with NBT.Int(_,i) -> i   // TODO make not fail on Minecraft 1.8, see line below
                        with e -> () // ignore failure, sloppy
    member this.RX = rx  // e.g. 1 means starts at x coord 512
    member this.RZ = rz
    member this.Write(outputFilename) =
        let zeros = Array.zeroCreate 4096 : byte[]
        let chunkOffsetTable = Array.zeroCreate 1024 : int[]
        let timeStampInfo = Array.zeroCreate 1024 : int[]
        let mutable nextFreeSection = 2  // sections 0 and 1 are chunk offset table and timestamp info table
        use file = new System.IO.FileStream(outputFilename, System.IO.FileMode.CreateNew)
        use bw = new BinaryWriter2(file)
        for cx = 0 to 31 do
            for cz = 0 to 31 do
                if chunks.[cx,cz] <> End then
                    if isChunkDirty.[cx,cz] then
                        let chunkLevel = match chunks.[cx,cz] with Compound(_,rsa) -> rsa.[0] // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name (or two with a data version appended)
                        match chunkLevel with 
                        | Compound(n,nbts) -> 
                            let i = nbts.FindIndex (fun nbt -> nbt.Name = "LightPopulated")
                            nbts.[i] <- NBT.Byte("LightPopulated", 0uy)
                    let ms = new System.IO.MemoryStream()
                    use s = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Compress, true)
                    chunks.[cx,cz].Write(new BinaryWriter2(s))
                    s.Close()
                    let numBytes = int ms.Length
                    let numSectionsNeeded = 1 + ((numBytes + 11) / 4096)
                    chunkOffsetTable.[cx+cz*32] <- (nextFreeSection <<< 8) + numSectionsNeeded
                    timeStampInfo.[cx+cz*32] <- chunkTimestampInfos.[cx,cz]  // no-op, not updating them
                    bw.Seek(nextFreeSection * 4096, System.IO.SeekOrigin.Begin) |> ignore
                    nextFreeSection <- nextFreeSection + numSectionsNeeded 
                    bw.Write(numBytes + 6) // length
                    bw.Write(2uy) // version (must be 2)
                    bw.Write(120uy) // CMF
                    bw.Write(156uy) // FLG
                    let temp = ms.ToArray()
                    bw.Write(temp, 0, numBytes)  // stream
                    bw.Write(RegionFile.ComputeAdler(temp)) // adler checksum
                    let paddingLengthNeeded = 4096 - ((numBytes+11)%4096)
                    bw.Write(zeros, 0, paddingLengthNeeded) // zero padding out to 4K
        bw.Seek(0, System.IO.SeekOrigin.Begin) |> ignore
        for i = 0 to 1023 do
            bw.Write(chunkOffsetTable.[i])
        for i = 0 to 1023 do
            bw.Write(timeStampInfo.[i])
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
    member private this.SetChunkDirty(x,z) = // x,z are world coordinates
        let xx = ((x+51200)%512)/16   // TODO start using DIV and MOD
        let zz = ((z+51200)%512)/16
        isChunkDirty.[xx,zz] <- true
    member this.GetOrCreateChunk(x,z) =  // x,z are world coordinates
        let xx = ((x+51200)%512)/16
        let zz = ((z+51200)%512)/16
        let theChunk = getOrCreateChunk(xx,zz)
        theChunk
    member this.TryGetSection(x,y,z) =  // x,y,z are world coordinates
        let xx = ((x+51200)%512)/16
        let zz = ((z+51200)%512)/16
        let sy = y/16
        match chunkSectionsCache.[xx,zz].[sy] with
        | null -> None
        | x -> Some x
    member this.GetOrCreateSection(x,y,z) =  // x,y,z are world coordinates
        let xx = ((x+51200)%512)/16
        let zz = ((z+51200)%512)/16
        let theChunk = getOrCreateChunk(xx,zz) // do this even though we don't "need" it, to avoid having sections without parent chunks
        let sy = y/16
        match chunkSectionsCache.[xx,zz].[sy] with
        | null -> 
            let theChunkLevel = match theChunk with Compound(_,rsa) -> rsa.[0] // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
            let sections = match theChunkLevel.["Sections"] with List(_,Compounds(cs)) -> cs
            let theSection = 
                match sections |> Array.tryFind (Array.exists (function Byte("Y",n) when n=byte(y/16) -> true | _ -> false)) with
                | Some x -> x
                | None ->
                    let newSection = [| NBT.Byte("Y",byte(y/16)); NBT.ByteArray("Blocks", Array.zeroCreate 4096); NBT.ByteArray("Data", Array.zeroCreate 2048); 
                                        NBT.ByteArray("BlockLight",Array.create 2048 0uy); NBT.ByteArray("SkyLight",Array.create 2048 0uy); NBT.End |]  // TODO relight chunk instead of fill with dummy light values?
                    match theChunkLevel with
                    | Compound(_,a) ->
                        let i = a.FindIndex (fun x -> x.Name="Sections")
                        a.[i] <- List("Sections",Compounds( sections |> Seq.append [| newSection |] |> Seq.toArray ))
                        isChunkDirty.[xx,zz] <- true
                        newSection
            chunkSectionsCache.[xx,zz].[sy] <- theSection
            theSection
        | x -> x
    member this.GetChunk(cx, cz) =
        match chunks.[cx,cz] with
        | End -> failwith "chunk not represented, NYI"
        | c -> c
    member this.TryGetChunk(cx, cz) =
        match chunks.[cx,cz] with
        | End -> None
        | c -> Some c
    member this.SetChunk(cx, cz, newChunk) =
        chunks.[cx,cz] <- newChunk
    member this.GetBlockInfo(x, y, z) =
        match this.TryGetBlockInfo(x,y,z) with
        | Some r -> r
        | None -> failwith "chunk not represented, NYI"
    member this.TryGetBlockInfo(x, y, z) =
        let xxxx = if x < 0 then x - 512 else x
        let zzzz = if z < 0 then z - 512 else z
        let xxxx = if xxxx < 0 then xxxx+1 else xxxx
        let zzzz = if zzzz < 0 then zzzz+1 else zzzz
        if xxxx/512 <> rx || zzzz/512 <> rz then failwith "coords outside this region"
        let theSection = this.GetOrCreateSection(x,y,z)
        let dx, dy, dz = (x+51200) % 16, y % 16, (z+51200) % 16
        let i = dy*256 + dz*16 + dx
        // BlockID
        let blocks = theSection |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
        // BlockData
        let blockData = theSection |> Array.pick (function ByteArray("Data",a) -> Some a | _ -> None)
        let blockDataAtI = Lazy.Create(fun() ->
            // expand 2048 half-bytes into 4096 for convenience of same indexing
            let blockData = Array.init 4096 (fun x -> if (x+51200)%2=1 then blockData.[x/2] >>> 4 else blockData.[x/2] &&& 0xFuy)
            blockData.[i])
        let theChunk = this.GetOrCreateChunk(x,z)
        let theChunkLevel = match theChunk with Compound(_,rsa) -> rsa.[0]  // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
        // TileEntities
        let tileEntity = 
            match theChunkLevel.["TileEntities"] with 
            | List(_,Compounds(cs)) ->
                let tes = cs |> Array.choose (fun te -> 
                    let te = Compound("unnamedDummyToCarryAPayload",te |> ResizeArray)
                    if te.["x"]=Int("x",x) && te.["y"]=Int("y",y) && te.["z"]=Int("z",z) then Some te else None)
                if tes.Length = 0 then None
                elif tes.Length = 1 then Some tes.[0]
                else failwith "unexpected: multiple TileEntities with same xyz coords"
            | _ -> None
        Some(new BlockInfo(blocks.[i], blockDataAtI, tileEntity))
    member this.SetBlockIDAndDamage(x, y, z, blockID, damage) =
        if (x+51200)/512 <> rx+100 || (z+51200)/512 <> rz+100 then failwith "coords outside this region"
        if damage > 15uy then failwith "invalid blockData"
        let theSection = this.GetOrCreateSection(x,y,z)
        let dx, dy, dz = (x+51200) % 16, y % 16, (z+51200) % 16
        let i = dy*256 + dz*16 + dx
        // BlockID
        let blocks = theSection |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
        blocks.[i] <- blockID
        // BlockData
        let blockData = theSection |> Array.pick (function ByteArray("Data",a) -> Some a | _ -> None)
        let mutable tmp = blockData.[i/2]
        if i%2 = 0 then
            tmp <- tmp &&& 0xF0uy
            tmp <- tmp + damage
        else
            tmp <- tmp &&& 0x0Fuy
            tmp <- tmp + (damage <<< 4)
        blockData.[i/2] <- tmp
        this.SetChunkDirty(x,z)
    member this.GetHeightMap(x, z) =
        if (x+51200)/512 <> rx+100 || (z+51200)/512 <> rz+100 then failwith "coords outside this region"
        let cx = ((x+51200)%512)/16
        let cz = ((z+51200)%512)/16
        let heightMap = chunkHeightMapCache.[cx,cz]
        if heightMap = null then
            failwith "no HeightMap cached"
        heightMap.[MOD(x,16),MOD(z,16)]
    member this.PlaceCommandBlocksStartingAtSelfDestruct(c:Coords,cmds:_[],comment) =
        this.PlaceCommandBlocksStartingAtSelfDestruct(c.X,c.Y,c.Z,cmds,comment)
    member this.PlaceCommandBlocksStartingAt(c:Coords,cmds:_[],comment) =
        this.PlaceCommandBlocksStartingAt(c.X,c.Y,c.Z,cmds,comment)
    member this.PlaceCommandBlocksStartingAtSelfDestruct(x,y,startz,cmds:_[],comment) =
        let cmds = [| yield! cmds; yield U (sprintf "fill ~ ~ ~-%d ~ ~ ~ air" cmds.Length) |]
        this.PlaceCommandBlocksStartingAt(x,y,startz,cmds,comment)
    member this.PlaceCommandBlocksStartingAt(x,y,startz,cmds:_[],comment) =
        printfn "%s%d commands being placed - %s" (if startz + cmds.Length > 180 then "***WARN*** - " else "") cmds.Length comment
        let preprocessForBackpatching(a:_[]) =
            // n is a single character
            // C/U BLOCKDATA ON n
            // C/U BLOCKDATA OFF n
            // O   TAG n
            let tagIndexes = new System.Collections.Generic.Dictionary<_,_>()
            for i = 0 to a.Length-1 do
                match a.[i] with
                | O s ->
                    if s.StartsWith("TAG ") then
                        if s.Length <> 5 then
                            failwith "bad TAG string length"
                        let n = s.Substring(4)
                        if tagIndexes.ContainsKey(n) then
                            failwith "duplicate TAG"
                        tagIndexes.Add(n, i)
                        a.[i] <- O ""
                | _ -> ()
            for i = 0 to a.Length-1 do
                match a.[i] with
                | U s | C s ->
                    if s.StartsWith("BLOCKDATA ON ") then
                        if s.Length <> 14 then
                            failwith "bad BLOCKDATA string length"
                        let n = s.Substring(13)
                        match a.[i+1] with
                        | U s | C s ->
                            if not( s.StartsWith("BLOCKDATA OFF "+n) ) then
                                failwith "BLOCKDATA ON must be followed by BLOCKDATA OFF"
                        | _ -> failwith "BLOCKDATA ON must be followed by BLOCKDATA OFF"
                        if not( tagIndexes.ContainsKey(n) ) then
                            failwith "try to BLOCKDATA a non-existent TAG"
                        let tagI = tagIndexes.[n]
                        let diff = tagI - i
                        match a.[i] with
                        | U _ -> a.[i] <- U (sprintf "blockdata ~ ~ ~%d {auto:1b}" diff)
                        | C _ -> a.[i] <- C (sprintf "blockdata ~ ~ ~%d {auto:1b}" diff)
                        | _ -> failwith "impossible"
                        match a.[i+1] with
                        | U _ -> a.[i+1] <- U (sprintf "blockdata ~ ~ ~%d {auto:0b}" (diff-1))
                        | C _ -> a.[i+1] <- C (sprintf "blockdata ~ ~ ~%d {auto:0b}" (diff-1))
                        | _ -> failwith "impossible"
                | _ -> ()
        preprocessForBackpatching(cmds)
        numCommandBlocksPlaced <- numCommandBlocksPlaced + cmds.Length 
        let cmds = Seq.append cmds [| O DUMMY |] |> Array.ofSeq  // dummy will put air at end - ensure not chain into existing blocks from world
#if DEBUG_DETAIL
        if cmds.[0] = O "" then
            cmds.[0] <- O (sprintf "say %s" comment)
#endif
        let mutable z = startz
        let mkCmd(x,y,z,auto,s,txt:_[]) = 
            if txt = null then
                [| NBT.Int("x",x); NBT.Int("y",y); NBT.Int("z",z); NBT.Byte("auto",auto); NBT.String("Command",s); 
                   NBT.Byte("conditionMet",0uy); NBT.String("CustomName","@"); NBT.Byte("powered",0uy);
                   NBT.String("id","Control"); NBT.Int("SuccessCount",0); 
#if DEBUG
                   NBT.Byte("TrackOutput",1uy);   // TODO for release, use release mode
#else
                   NBT.Byte("TrackOutput",0uy);
#endif
                   NBT.End |]
            else
                [| yield NBT.Int("x",x); yield NBT.Int("y",y); yield NBT.Int("z",z); yield NBT.String("id","Sign")
                   for i = 0 to txt.Length-1 do
                       yield NBT.String(sprintf "Text%d" (i+1), sprintf "{\"text\":\"%s\"}" txt.[i])
                   yield NBT.End |]
        let mutable prevcx = -1
        let mutable prevcz = -1
        let mutable tepayload = ResizeArray<NBT[]>()
        let storeIt(prevcx,prevcz,tepayload) =
            let a = match getOrCreateChunk(prevcx, prevcz) with Compound(_,rsa) -> match rsa.[0] with Compound(_,a) -> a
            let mutable found = false
            let mutable i = 0
            while not found && i < a.Count-1 do
                if a.[i].Name = "TileEntities" then
                    found <- true
                    a.[i] <- List("TileEntities",Compounds(tepayload |> Array.ofSeq))
                i <- i + 1
            if not found then
                match chunks.[prevcx, prevcz] with 
                | Compound(_,rsa) ->
                    match rsa.[0] with 
                    | Compound(n,a) -> rsa.[0] <- Compound(n, a |> Seq.append [| List("TileEntities",Compounds(tepayload |> Array.ofSeq)) |] |> ResizeArray)
        for c in cmds do
            let bid, bd, au, s,txt = 
                match c with
                | P s -> 210uy,3uy,0uy,s,null
                | O DUMMY -> 0uy,0uy,0uy,DUMMY,null
                | O s -> 137uy,3uy,0uy,s,null
                | U s -> 211uy,3uy,1uy,s,null
                | C s -> 211uy,11uy,1uy,s,null
            this.SetBlockIDAndDamage(x,y,z,bid,bd)
            let nbts = if s = DUMMY then [||] else [|mkCmd(x,y,z,au,s,txt)|]
            if (x+51200)/512 <> rx+100 || (z+51200)/512 <> rz+100 then failwith "coords outside this region"
            let xx = ((x+51200)%512)/16
            let zz = ((z+51200)%512)/16
            if xx <> prevcx || zz <> prevcz then
                // store out old TE as we move out of this chunk
                if prevcx <> -1 then
                    storeIt(prevcx,prevcz,tepayload)
                // load in initial TE as we move into next chunk
                let newChunk = match getOrCreateChunk(xx,zz) with 
                               | Compound(_,rsa) -> rsa.[0]
                tepayload <- match newChunk.TryGetFromCompound("TileEntities") with | Some (List(_,Compounds(cs))) -> ResizeArray(cs) | None -> ResizeArray()
                prevcx <- xx
                prevcz <- zz
            // accumulate payload in this chunk
            let thisz = z
            tepayload <-  
                tepayload 
                |> Seq.filter (fun te -> 
                    let alreadyThere = Array.exists (fun o -> o=Int("x",x)) te && Array.exists (fun o -> o=Int("y",y)) te && Array.exists (fun o -> o=Int("z",thisz)) te
                    if alreadyThere then
                        failwith "uh-oh, overwriting blocks"
                        //printfn "******WARN***** overwriting blocks"
                    not(alreadyThere) )
                |> Seq.append nbts |> (fun x -> ResizeArray x)
            z <- z + 1
        storeIt(prevcx,prevcz,tepayload)
    member this.NumCommandBlocksPlaced = numCommandBlocksPlaced
    member this.DumpChunkDebug(cx,cz) =
        let nbt = chunks.[cx,cz]
        let s = nbt.ToString()
        printfn "%s" s
    member this.ReplaceBlocks(f) =
        for cx = 0 to 31 do
            for cz = 0 to 31 do
                if chunks.[cx,cz] <> End then
                    for sy = 0 to 15 do
                        let s = chunkSectionsCache.[cx,cz].[sy]
                        if s <> null then
                            let blocks = s |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
                            let data = s |> Array.pick (function ByteArray("Data",a) -> Some a | _ -> None)
                            f blocks data
    member this.AddTileTick(id,t,p,x,y,z) =
        let cx = ((x+51200)%512)/16
        let cz = ((z+51200)%512)/16
        let theChunk = this.GetChunk(cx,cz)
        let tick = [| String("i",id); Int("t",t); Int("p",p); Int("x",x); Int("y",y); Int("z",z); End |]
        let theChunkLevel = match theChunk with Compound(_,rsa) -> rsa.[0] // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
        match theChunkLevel with
        | Compound(level,a) ->
            match a.FindIndex (fun x -> x.Name = "TileTicks") with
            | -1 -> 
                a.Insert(0, List("TileTicks",Compounds[|tick|]))
            | i ->
                match a.[i] with
                | List(n,Compounds(cs)) ->
                   a.[i] <-  List(n,Compounds(Seq.append cs [tick] |> Seq.toArray))
