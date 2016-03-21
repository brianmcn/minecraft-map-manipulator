module RegionFiles

open NBT_Manipulation

[<AllowNullLiteral>]
type BlockInfo(blockID:byte, blockData:byte) =
    member this.BlockID = blockID
    member this.BlockData = blockData

type CommandBlock =     // the useful subset I plan to map anything into
    | P of string  // purple (pointing positive Z, unconditional, auto:0)
    | O of string  // orange (pointing positive Z, unconditional, auto:0)
    | U of string  // green (pointing positive Z, unconditional, auto:1)
    | C of string  // green (pointing positive Z, conditional, auto:1)

type Coords(x:int,y:int,z:int) = 
    member this.X = x
    member this.Y = y
    member this.Z = z
    member this.Tuple = x,y,z
    member this.STR = sprintf "%d %d %d" x y z
    member this.Offset(dx,dy,dz) = new Coords(x+dx, y+dy, z+dz)

[<AllowNullLiteral>]
type TwoDArrayFacadeOverOneDArray<'T>(flatArray:'T[]) =
    let K = 16 // Minecraft uses z*16+x for chunk array indexing 
    member this.Item 
        with get(x,z) = flatArray.[z*K+x]
        and set(x,z) v = flatArray.[z*K+x] <- v

module NibbleArray = // deal with various 2048-length arrays of 4-bit entries (blockdata, blocklight, skylight)
    let get(a:_[], x, y, z) =
        let dx, dy, dz = (x+51200) % 16, y % 16, (z+51200) % 16
        let i = dy*256 + dz*16 + dx
        let r = if i%2=1 then a.[i/2] >>> 4 else a.[i/2] &&& 0xFuy
        r
    let set(a:_[], x, y, z, v) =
        let dx, dy, dz = (x+51200) % 16, y % 16, (z+51200) % 16
        let i = dy*256 + dz*16 + dx
        let mutable tmp = a.[i/2]
        if i%2 = 0 then
            tmp <- tmp &&& 0xF0uy
            tmp <- tmp + v
        else
            tmp <- tmp &&& 0x0Fuy
            tmp <- tmp + (v <<< 4)
        a.[i/2] <- tmp

[<Literal>]
let DUMMY = "say dummy"

let DIV(x:int,m:int) = (x+10000*m)/m - 10000
let MOD(x:int,m:int) = (x+10000*m)%m

type ChunkLightPopulated = | DIRTY | CLEAN | UNCHANGED

[<AllowNullLiteral>]
type RegionFile(filename) =
    let rx, rz =
        let m = System.Text.RegularExpressions.Regex.Match(filename, """.*r\.(.*)\.(.*)\.mca(\.new|\.old)?$""")
        int m.Groups.[1].Value, int m.Groups.[2].Value
    let isChunkDirty = Array2D.create 32 32 UNCHANGED
    let chunkHeightMapCache : TwoDArrayFacadeOverOneDArray<int>[,] = Array2D.create 32 32 null   // chunkHeightMapCache.[cx,cz].[x,z]
    let chunkBiomeCache : TwoDArrayFacadeOverOneDArray<byte>[,] = Array2D.create 32 32 null   // chunkBiomeCache.[cx,cz].[x,z]
    let chunkInhabitedTimeCache = Array2D.create 32 32 -1L // -1 means unrepesented/unknown
    let chunkSectionsCache : (NBT[]*byte[]*byte[]*byte[]*byte[])[][,] = // chunkSectionsCache.[cx,cz].[sy] -> nbt, bid, blockdata, blocklight, skylight
        Array2D.init 32 32 (fun _ _ -> Array.create 16 (null,null,null,null,null))
    let chunks : NBT[,] = Array2D.create 32 32 End  // End represents a blank (unrepresented) chunk
    let chunkTimestampInfos : int[,] = Array2D.zeroCreate 32 32
    let mutable firstSeenDataVersion = -1
#if DEBUG
    let ensureCorrectRegion(x,z) =
        let xxxx = if x < 0 then x - 512 else x
        let zzzz = if z < 0 then z - 512 else z
        let xxxx = if xxxx < 0 then xxxx+1 else xxxx
        let zzzz = if zzzz < 0 then zzzz+1 else zzzz
        if xxxx/512 <> rx || zzzz/512 <> rz then 
            failwith "coords outside this region"
#else
    let ensureCorrectRegion(_x,_z) = ()
#endif
    let getOrCreateChunk(cx,cz,wcx,wcz) =  // cx/cz in 0..31, whereas wcx/wcz are world x/z div 16
        let chunk = 
            match chunks.[cx,cz] with
            | End ->
                let newChunk = Compound("",[|NBT.Compound("Level", [|
                                                                    NBT.Int("xPos",wcx); NBT.Int("zPos",wcz); NBT.Long("LastUpdate", 0L);
                                                                    NBT.Byte("LightPopulated",0uy); NBT.Byte("TerrainPopulated",1uy); // TODO best TerrainPopulated value?
                                                                    NBT.Byte("V",1uy); NBT.Long("InhabitedTime",0L);
                                                                    NBT.IntArray("HeightMap", Array.zeroCreate 256)
                                                                    NBT.ByteArray("Biomes", Array.zeroCreate 256)
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
            | Compound(_n,nbts) -> 
                let i = nbts.FindIndex (fun nbt -> nbt.Name = "HeightMap")
                match nbts.[i] with
                | NBT.IntArray(_,flatArray) ->
                    chunkHeightMapCache.[cx,cz] <- TwoDArrayFacadeOverOneDArray(flatArray)
        if chunkBiomeCache.[cx,cz] = null then
            let chunkLevel = match chunk with Compound(_,rsa) -> rsa.[0]  // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name (or two with a data version appended)
            match chunkLevel with 
            | Compound(_n,nbts) -> 
                let i = nbts.FindIndex (fun nbt -> nbt.Name = "Biomes")
                match nbts.[i] with
                | NBT.ByteArray(_,flatArray) ->
                    chunkBiomeCache.[cx,cz] <- TwoDArrayFacadeOverOneDArray(flatArray)
        if chunkInhabitedTimeCache.[cx,cz] = -1L then
            let chunkLevel = match chunk with Compound(_,rsa) -> rsa.[0]  // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name (or two with a data version appended)
            match chunkLevel with 
            | Compound(_n,nbts) -> 
                let i = nbts.FindIndex (fun nbt -> nbt.Name = "InhabitedTime")
                match nbts.[i] with
                | NBT.Long(_,v) -> chunkInhabitedTimeCache.[cx,cz] <- v
        chunk
    let updateChunksWithSideData() =
        // Some data (like InhabitedTime) is stored 'off to the side' for efficient read/write, with values that may differ from what's in the NBT.  
        // Throughout, we treat the side data as canonical and the NBT data as stale.
        // This function updates the NBT to be current with the side data, e.g. so we can write NBT out to disk.
        for cx = 0 to 31 do
            for cz = 0 to 31 do
                if chunks.[cx,cz] <> End then
                    if chunkInhabitedTimeCache.[cx,cz] <> -1L then
                        let chunkLevel = match chunks.[cx,cz] with Compound(_,rsa) -> rsa.[0] // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name (or two with a data version appended)
                        match chunkLevel with 
                        | Compound(n,nbts) -> 
                            let i = nbts.FindIndex (fun nbt -> nbt.Name = "InhabitedTime")
                            nbts.[i] <- NBT.Long("InhabitedTime", chunkInhabitedTimeCache.[cx,cz])
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
        updateChunksWithSideData()
        let zeros = Array.zeroCreate 4096 : byte[]
        let chunkOffsetTable = Array.zeroCreate 1024 : int[]
        let timeStampInfo = Array.zeroCreate 1024 : int[]
        let mutable nextFreeSection = 2  // sections 0 and 1 are chunk offset table and timestamp info table
        use file = new System.IO.FileStream(outputFilename, System.IO.FileMode.CreateNew)
        use bw = new BinaryWriter2(file)
        for cx = 0 to 31 do
            for cz = 0 to 31 do
                if chunks.[cx,cz] <> End then
                    if isChunkDirty.[cx,cz] = DIRTY then
                        let chunkLevel = match chunks.[cx,cz] with Compound(_,rsa) -> rsa.[0] // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name (or two with a data version appended)
                        match chunkLevel with 
                        | Compound(n,nbts) -> 
                            let i = nbts.FindIndex (fun nbt -> nbt.Name = "LightPopulated")
                            nbts.[i] <- NBT.Byte("LightPopulated", 0uy)
                    elif isChunkDirty.[cx,cz] = CLEAN then
                        let chunkLevel = match chunks.[cx,cz] with Compound(_,rsa) -> rsa.[0] // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name (or two with a data version appended)
                        match chunkLevel with 
                        | Compound(n,nbts) -> 
                            let i = nbts.FindIndex (fun nbt -> nbt.Name = "LightPopulated")
                            nbts.[i] <- NBT.Byte("LightPopulated", 1uy)
                    else 
                        assert( isChunkDirty.[cx,cz] = UNCHANGED )
                    let uncompressedStream = new System.IO.MemoryStream()
                    chunks.[cx,cz].Write(new BinaryWriter2(uncompressedStream))
                    uncompressedStream.Close()
                    let uncompressedBytes = uncompressedStream.ToArray()
                    let ms = new System.IO.MemoryStream()
                    //let fLevel, compressionLevel = 3, System.IO.Compression.CompressionLevel.Optimal
                    let fLevel, compressionLevel = 0, System.IO.Compression.CompressionLevel.Fastest   // This is about 2.2x faster, but files are about 15% larger. (MC opens them fine.)
                    use s = new System.IO.Compression.DeflateStream(ms, compressionLevel, true)
                    s.Write(uncompressedBytes, 0, uncompressedBytes.Length)
                    s.Close()
                    let numBytes = int ms.Length
                    let numSectionsNeeded = 1 + ((numBytes + 11) / 4096)
                    chunkOffsetTable.[cx+cz*32] <- (nextFreeSection <<< 8) + numSectionsNeeded
                    timeStampInfo.[cx+cz*32] <- chunkTimestampInfos.[cx,cz]  // no-op, not updating them
                    bw.Seek(nextFreeSection * 4096, System.IO.SeekOrigin.Begin) |> ignore
                    nextFreeSection <- nextFreeSection + numSectionsNeeded 
                    bw.Write(numBytes + 7) // length (1 byte ver, 1 byte cmf, 1 byte flg, data, 4 bytes adler)
                    bw.Write(2uy) // version (must be 2)
                    // CM = 8 = deflate
                    // CMINFO = ?? = log2(window size)-8   (7 may be good?)
                    // CMF = CINFO <<< 4 + CM
                    let cmf = 120uy
                    bw.Write(cmf) // CMF
                    // FCHECK = N such that CMF*256+FLG is a multiple of 31
                    // FDICT = 0 (no preset dict)
                    // FLEVEL = compress level (0=fastest, 1=fast, 2=default, 3=max compress)
                    // FLG = FLEVEL <<< 6 + FDICT <<< 5 + FCHECK
                    let mutable flg = (fLevel <<< 6) + (0 <<< 5)
                    while (256 * (int cmf) + flg) % 31 <> 0 do
                        flg <- flg + 1
                    bw.Write(byte flg) // FLG
                    let temp = ms.ToArray()
                    assert(temp.Length = numBytes)
                    bw.Write(temp, 0, numBytes)  // stream
                    bw.Write(RegionFile.ComputeAdler(uncompressedBytes)) // adler checksum
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
        // we note chunks where we want to recompute light as 'dirty'
        let xx = ((x+51200)%512)/16   // TODO start using DIV and MOD
        let zz = ((z+51200)%512)/16
        isChunkDirty.[xx,zz] <- DIRTY
    member this.SetChunkClean(x,z) = // x,z are world coordinates
        // we note chunks where we recomputed the light offline as 'clean'
        let xx = ((x+51200)%512)/16   // TODO start using DIV and MOD
        let zz = ((z+51200)%512)/16
        isChunkDirty.[xx,zz] <- CLEAN
    member this.GetOrCreateChunk(x,z) =  // x,z are world coordinates
        let xx = ((x+51200)%512)/16
        let zz = ((z+51200)%512)/16
        let theChunk = getOrCreateChunk(xx,zz,x/16,z/16)
        theChunk
    member this.GetSection(x,y,z) =  // x,y,z are world coordinates - will return (null,null,null,null,null) for unrepresented sections
        this.GetOrCreateSectionCore(x,y,z,false)
    member private this.GetOrCreateSectionCore(x,y,z,createIfUnrepresented) =  // x,y,z are world coordinates
        let xx = ((x+51200)%512)/16
        let zz = ((z+51200)%512)/16
        let theChunk = 
            if createIfUnrepresented then
                getOrCreateChunk(xx,zz,x/16,z/16) // do this even though we don't "need" it, to avoid having sections without parent chunks
            else
                chunks.[xx,zz]
        match theChunk with
        | End ->
            assert(not createIfUnrepresented)
            null,null,null,null,null
        | _ ->
        let sy = y/16
        match chunkSectionsCache.[xx,zz].[sy] with
        | nbtCacheProxy,null,null,null,null -> 
            if nbtCacheProxy <> null && not(createIfUnrepresented) then
                // we've already tried to look up this section, and it wasn't there. we're not trying to make a new one, so quick-return.
                null,null,null,null,null
            else
                let theChunkLevel = match theChunk with Compound(_,rsa) -> rsa.[0] // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
                let sections = match theChunkLevel.["Sections"] with List(_,Compounds(cs)) -> cs
                match sections |> Array.tryFind (Array.exists (function Byte("Y",n) when n=byte(y/16) -> true | _ -> false)) with
                | Some x -> 
                    let blocks = x |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
                    let blockData = x |> Array.pick (function ByteArray("Data",a) -> Some a | _ -> None)
                    let blockLight = x |> Array.pick (function ByteArray("BlockLight",a) -> Some a | _ -> None)
                    let skyLight = x |> Array.pick (function ByteArray("SkyLight",a) -> Some a | _ -> None)
                    let r = x, blocks, blockData, blockLight, skyLight
                    chunkSectionsCache.[xx,zz].[sy] <- r
                    r
                | None ->
                    if createIfUnrepresented then
                        let blocks = Array.zeroCreate 4096
                        let blockData = Array.zeroCreate 2048
                        let blockLight = Array.create 2048 0uy
                        let skyLight = Array.create 2048 0uy
                        let newSection = [| NBT.Byte("Y",byte(y/16)); NBT.ByteArray("Blocks", blocks); NBT.ByteArray("Data", blockData); 
                                            NBT.ByteArray("BlockLight", blockLight); NBT.ByteArray("SkyLight", skyLight); NBT.End |]  // TODO relight chunk instead of fill with dummy light values?
                                            // TODO at very least, should populate skylight based on heightmap? oh but that's non-trivial (but doable) if half this section is above HM but other half below an overhang...
                        match theChunkLevel with
                        | Compound(_,a) ->
                            let i = a.FindIndex (fun x -> x.Name="Sections")
                            a.[i] <- List("Sections",Compounds( sections |> Seq.append [| newSection |] |> Seq.toArray ))
                            isChunkDirty.[xx,zz] <- DIRTY
                            let r = newSection, blocks, blockData, blockLight, skyLight
                            chunkSectionsCache.[xx,zz].[sy] <- r
                            r
                    else
                        chunkSectionsCache.[xx,zz].[sy] <- [||],null,null,null,null
                        null,null,null,null,null
        | x -> x
    member this.GetOrCreateSection(x,y,z) =  // x,y,z are world coordinates
        this.GetOrCreateSectionCore(x,y,z,true)
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
    member private this.GetBlockInfoCore(x,y,z,blocks:_[],blockData:_[]) =
        let dx, dy, dz = (x+51200) % 16, y % 16, (z+51200) % 16
        let i = dy*256 + dz*16 + dx
        let blockDataAtI = if i%2=1 then blockData.[i/2] >>> 4 else blockData.[i/2] &&& 0xFuy
#if DEBUG
        let bdai = NibbleArray.get(blockData, dx, dy, dz)
        assert(bdai = blockDataAtI)
#endif
        new BlockInfo(blocks.[i], blockDataAtI)
    member this.MaybeGetBlockInfo(x, y, z) = // will return null if unrepresented
        ensureCorrectRegion(x,z)
        let _theSection, blocks, blockData, _bl, _sl = this.GetSection(x,y,z)
        if blocks = null then null
        else this.GetBlockInfoCore(x,y,z,blocks,blockData)
    member this.GetBlockInfo(x, y, z) = // will create unrepresented chunk/section
        ensureCorrectRegion(x,z)
        let _theSection, blocks, blockData, _bl, _sl = this.GetOrCreateSection(x,y,z)
        this.GetBlockInfoCore(x,y,z,blocks,blockData)
    member this.GetTileEntity(x, y, z) = // will create unrepresented chunk/section
        ensureCorrectRegion(x,z)
        this.GetOrCreateSection(x,y,z) |> ignore
        // TileEntities
        let tileEntity =
            let theChunk = this.GetOrCreateChunk(x,z)
            let theChunkLevel = match theChunk with Compound(_,rsa) -> rsa.[0]  // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
            match theChunkLevel.["TileEntities"] with 
            | List(_,Compounds(cs)) ->
                let tes = cs |> Array.choose (fun te -> 
                    let te = Compound("unnamedDummyToCarryAPayload",te |> ResizeArray)
                    if te.["x"]=Int("x",x) && te.["y"]=Int("y",y) && te.["z"]=Int("z",z) then Some te else None)
                if tes.Length = 0 then None
                elif tes.Length = 1 then Some tes.[0]
                else failwith "unexpected: multiple TileEntities with same xyz coords"
            | _ -> None
        tileEntity
    member this.EnsureSetBlockIDAndDamage(x, y, z, blockID, damage) =
        this.GetOrCreateSection(x,y,z) |> ignore
        this.SetBlockIDAndDamage(x, y, z, blockID, damage)
    member this.SetBlockIDAndDamage(x, y, z, blockID, damage) =
        if (x+51200)/512 <> rx+100 || (z+51200)/512 <> rz+100 then failwith "coords outside this region"
        if damage > 15uy then failwith "invalid blockData"
        let _theSection, blocks, blockData, _bl, _sl = this.GetSection(x,y,z)
        if blocks = null then 
            failwith "unrepresented section"
        let dx, dy, dz = (x+51200) % 16, y % 16, (z+51200) % 16
        let i = dy*256 + dz*16 + dx
        blocks.[i] <- blockID
        let mutable tmp = blockData.[i/2]
        if i%2 = 0 then
            tmp <- tmp &&& 0xF0uy
            tmp <- tmp + damage
        else
            tmp <- tmp &&& 0x0Fuy
            tmp <- tmp + (damage <<< 4)
        blockData.[i/2] <- tmp
        this.SetChunkDirty(x,z)
    member this.GetHeightMap(x, z) = // This method can be very slow if called repeatedly, clients may want to track an outside cache
        ensureCorrectRegion(x,z)
        let cx = ((x+51200)%512)/16
        let cz = ((z+51200)%512)/16
        let heightMap = chunkHeightMapCache.[cx,cz]
        if heightMap = null then
            failwith "no HeightMap cached"
        heightMap.[MOD(x,16),MOD(z,16)]
    member this.SetHeightMap(x, z, h) =
        ensureCorrectRegion(x,z)
        let cx = ((x+51200)%512)/16
        let cz = ((z+51200)%512)/16
        let heightMap = chunkHeightMapCache.[cx,cz]
        if heightMap = null then
            failwith "no HeightMap cached"
        heightMap.[MOD(x,16),MOD(z,16)] <- h
    member this.GetBiome(x, z) = // This method can be very slow if called repeatedly, clients may want to track an outside cache
        ensureCorrectRegion(x,z)
        let cx = ((x+51200)%512)/16
        let cz = ((z+51200)%512)/16
        let biome = chunkBiomeCache.[cx,cz]
        if biome = null then
            failwith "no Biome cached"
        biome.[MOD(x,16),MOD(z,16)]
    member this.SetBiome(x, z, bio) =
        ensureCorrectRegion(x,z)
        let cx = ((x+51200)%512)/16
        let cz = ((z+51200)%512)/16
        let biome = chunkBiomeCache.[cx,cz]
        if biome = null then
            failwith "no Biome cached"
        biome.[MOD(x,16),MOD(z,16)] <- bio
    member this.GetInhabitedTime(x, z) =      // note, reads value from entire chunk
        ensureCorrectRegion(x,z)
        let cx = ((x+51200)%512)/16
        let cz = ((z+51200)%512)/16
        let inhabitedTime = chunkInhabitedTimeCache.[cx,cz]
        if inhabitedTime = -1L then
            failwith "no InhabitedTime cached"
        inhabitedTime
    member this.SetInhabitedTime(x, z, it) =  // note, sets value for whole chunk
        ensureCorrectRegion(x,z)
        let cx = ((x+51200)%512)/16
        let cz = ((z+51200)%512)/16
        chunkInhabitedTimeCache.[cx,cz] <- it
    member this.PlaceCommandBlocksStartingAtSelfDestruct(c:Coords,cmds:_[],comment) =
        this.PlaceCommandBlocksStartingAtSelfDestruct(c.X,c.Y,c.Z,cmds,comment)
    member this.PlaceCommandBlocksStartingAt(c:Coords,cmds:_[],comment) =
        this.PlaceCommandBlocksStartingAt(c.X,c.Y,c.Z,cmds,comment)
    member this.PlaceCommandBlocksStartingAtSelfDestruct(x,y,startz,cmds:_[],comment) =
        let cmds = [| yield! cmds; yield U (sprintf "fill ~ ~ ~-%d ~ ~ ~ air" cmds.Length) |]
        this.PlaceCommandBlocksStartingAt(x,y,startz,cmds,comment)
    member this.PlaceCommandBlocksStartingAt(x,y,startz,cmds:_[],comment) =
        this.PlaceCommandBlocksStartingAt(x,y,startz,cmds,comment,true,true)
    member this.PlaceCommandBlocksStartingAt(x,y,startz,cmds:_[],comment,checkForOverwrites,putDummyAirAtEnd) =
        if comment <> "" then
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
        let cmds = 
            if putDummyAirAtEnd then
                Seq.append cmds [| O DUMMY |] |> Array.ofSeq  // dummy will put air at end - ensure not chain into existing blocks from world
            else
                cmds
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
        let mutable prevcx,prevcz,prevwcx,prevwcz = -1, -1, -1, -1
        let mutable tepayload = ResizeArray<NBT[]>()
        let storeIt(prevcx,prevcz,prevwcx,prevwcz,tepayload) =
            let a = match getOrCreateChunk(prevcx, prevcz, prevwcx, prevwcz) with Compound(_,rsa) -> match rsa.[0] with Compound(_,a) -> a
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
            this.EnsureSetBlockIDAndDamage(x,y,z,bid,bd)
            let nbts = if s = DUMMY then [||] else [|mkCmd(x,y,z,au,s,txt)|]
            if (x+51200)/512 <> rx+100 || (z+51200)/512 <> rz+100 then failwith "coords outside this region"
            let xx = ((x+51200)%512)/16
            let zz = ((z+51200)%512)/16
            if xx <> prevcx || zz <> prevcz then
                // store out old TE as we move out of this chunk
                if prevcx <> -1 then
                    storeIt(prevcx,prevcz,prevwcx,prevwcz,tepayload)
                // load in initial TE as we move into next chunk
                let newChunk = match getOrCreateChunk(xx,zz,x/16,z/16) with 
                               | Compound(_,rsa) -> rsa.[0]
                tepayload <- match newChunk.TryGetFromCompound("TileEntities") with | Some (List(_,Compounds(cs))) -> ResizeArray(cs) | None -> ResizeArray()
                prevcx <- xx
                prevcz <- zz
                prevwcx <- x/16
                prevwcz <- z/16
            // accumulate payload in this chunk
            let thisz = z
            if checkForOverwrites then
                tepayload <-  
                    tepayload 
                    |> Seq.filter (fun te -> 
                        let alreadyThere = Array.exists (fun o -> o=Int("x",x)) te && Array.exists (fun o -> o=Int("y",y)) te && Array.exists (fun o -> o=Int("z",thisz)) te
                        if alreadyThere then
                            failwith "uh-oh, overwriting blocks"
                            //printfn "******WARN***** overwriting blocks"
                        not(alreadyThere) )
                    |> Seq.append nbts |> (fun x -> ResizeArray x)
            else
                tepayload.AddRange(nbts)
            z <- z + 1
        storeIt(prevcx,prevcz,prevwcx,prevwcz,tepayload)
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
                        let s,blocks,data, _bl, _sl = chunkSectionsCache.[cx,cz].[sy]
                        if s <> null then
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

////////////////////////////////////////////////////

type MapFolder(folderName) =
    let MMM = 100
    let cachedRegions = Array2D.zeroCreateBased -MMM -MMM (2*MMM) (2*MMM)
    let checkRegionBounds(rx,rz) =
        if rx < -MMM || rx > MMM-1 || rz < -MMM || rz > MMM-1 then
            failwith "MapFolder can currently only do a fixed num regions"
    let getOrLoadRegion(rx,rz,failIfNotThere) =
        checkRegionBounds(rx,rz)
        if cachedRegions.[rx,rz] <> null then
            cachedRegions.[rx,rz]
        else
            let fil = System.IO.Path.Combine(folderName, sprintf "r.%d.%d.mca" rx rz)
            if not(System.IO.File.Exists(fil)) && not(failIfNotThere) then
                null
            else
                let r = new RegionFile(fil)
                cachedRegions.[rx,rz] <- r
                r
    let getOrCreateRegion(rx,rz) = getOrLoadRegion(rx,rz,true) // TODO 'create' is a bad name choice, it loads off disk into cache, but does not create a new one if nonexistent
    member this.AddEntities(es) =
        // partition by region,chunk
        let data = new System.Collections.Generic.Dictionary<_,_>()
        for e in es do
            let pos = e |> Array.pick (function List("Pos",Doubles(da)) -> Some da | _ -> None)
            let [|fx;_fy;fz|] = pos
            let rx = int(floor(fx + 512000.0)) / 512 - 1000
            let rz = int(floor(fz + 512000.0)) / 512 - 1000
            if not(data.ContainsKey(rx,rz)) then
                data.Add((rx,rz), Array2D.init 32 32 (fun _ _ -> ResizeArray()))
            let cx = (int(floor(fx+512000.0))%512)/16
            let cz = (int(floor(fz+512000.0))%512)/16
            data.[(rx,rz)].[cx,cz].Add(e)
        for (KeyValue((rx,rz),esPerChunk)) in data do
            let r = getOrCreateRegion(rx, rz)
            // load each chunk Es
            for cx = 0 to 31 do
                for cz = 0 to 31 do
                    if esPerChunk.[cx,cz].Count > 0 then
                        let chunk = r.GetChunk(cx,cz)
                        let a = match chunk with Compound(_,rsa) -> match rsa.[0] with Compound(_,a) -> a
                        let mutable found = false
                        let mutable i = 0
                        while not found && i < a.Count-1 do
                            match a.[i] with
                            | List("Entities",Compounds(existingEs)) ->
                                found <- true
                                a.[i] <- List("Entities",Compounds(Seq.append existingEs esPerChunk.[cx,cz] |> Array.ofSeq))
                            | _ -> ()
                            i <- i + 1
                        if not found then // no Entities yet, write the entry
                            match chunk with 
                            | Compound(_,rsa) -> 
                                match rsa.[0] with 
                                | Compound(n,a) -> rsa.[0] <- Compound(n, a |> Seq.append [| List("Entities",Compounds(esPerChunk.[cx,cz] |> Array.ofSeq)) |] |> ResizeArray)
    member this.AddOrReplaceTileEntities(tes) =
        // partition by region,chunk
        let data = new System.Collections.Generic.Dictionary<_,_>()
        for te in tes do
            let x = te |> Array.pick (function Int("x",x) -> Some x | _ -> None)
            let y = te |> Array.pick (function Int("y",y) -> Some y | _ -> None)
            let z = te |> Array.pick (function Int("z",z) -> Some z | _ -> None)
            let rx = (x + 512000) / 512 - 1000
            let rz = (z + 512000) / 512 - 1000
            if not(data.ContainsKey(rx,rz)) then
                data.Add((rx,rz), Array2D.init 32 32 (fun _ _ -> ResizeArray()))
            let cx = ((x+512000)%512)/16
            let cz = ((z+512000)%512)/16
            data.[(rx,rz)].[cx,cz].Add(te)
        for (KeyValue((rx,rz),tesPerChunk)) in data do
            let r = getOrCreateRegion(rx, rz)
            // load each chunk TEs
            for cx = 0 to 31 do
                for cz = 0 to 31 do
                    if tesPerChunk.[cx,cz].Count > 0 then
                        let chunk = r.GetChunk(cx,cz)
                        let a = match chunk with Compound(_,rsa) -> match rsa.[0] with Compound(_,a) -> a
                        let mutable found = false
                        let mutable i = 0
                        while not found && i < a.Count-1 do
                            match a.[i] with
                            | List("TileEntities",Compounds(existingTEs)) ->
                                // there are TEs already, remove any with xyz that we'll overwrite, and add new ones
                                found <- true
                                let finalTEs = ResizeArray()
                                for ete in existingTEs do
                                    let mutable willGetOverwritten = false
                                    for nte in tesPerChunk.[cx,cz] do
                                        let x = nte |> Array.pick (function Int("x",x) -> Some x | _ -> None)
                                        let y = nte |> Array.pick (function Int("y",y) -> Some y | _ -> None)
                                        let z = nte |> Array.pick (function Int("z",z) -> Some z | _ -> None)
                                        let alreadyThere = Array.exists (fun o -> o=Int("x",x)) ete && Array.exists (fun o -> o=Int("y",y)) ete && Array.exists (fun o -> o=Int("z",z)) ete
                                        if alreadyThere then
                                            willGetOverwritten <- true
                                    if willGetOverwritten then
                                        () // TODO failwith "TODO overwriting TE, care?"
                                    else
                                        finalTEs.Add(ete)
                                for nte in tesPerChunk.[cx,cz] do
                                    finalTEs.Add(nte)
                                a.[i] <- List("TileEntities",Compounds(finalTEs |> Array.ofSeq))
                            | _ -> ()
                            i <- i + 1
                        if not found then // no TileEntities yet, write the entry
                            match chunk with 
                            | Compound(_,rsa) -> 
                                match rsa.[0] with 
                                | Compound(n,a) -> rsa.[0] <- Compound(n, a |> Seq.append [| List("TileEntities",Compounds(tesPerChunk.[cx,cz] |> Array.ofSeq)) |] |> ResizeArray)
    member this.GetHeightMap(x,z) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.GetHeightMap(x,z)
    member this.SetHeightMap(x,z,h) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.SetHeightMap(x,z,h)
    member this.GetBiome(x,z) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.GetBiome(x,z)
    member this.SetBiome(x,z,b) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.SetBiome(x,z,b)
    member this.GetInhabitedTime(x,z) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.GetInhabitedTime(x,z)
    member this.SetInhabitedTime(x,z,it) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.SetInhabitedTime(x,z,it)
    member this.GetRegion(x,z) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r
    member this.MaybeGetRegion(x,z) =  // may return null if no file on disk
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        checkRegionBounds(rx,rz)
        let r = getOrLoadRegion(rx,rz,false)
        r
    member this.EnsureSetBlockIDAndDamage(x,y,z,bid,d) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.EnsureSetBlockIDAndDamage(x,y,z,bid,d)
    member this.SetBlockIDAndDamage(x,y,z,bid,d) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.SetBlockIDAndDamage(x,y,z,bid,d)
    member this.GetSection(x,y,z) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.GetSection(x,y,z)
    member this.MaybeMaybeGetSection(x,y,z) = // if region exists, if chunk exists, if section exists, return it, else nulls - create nothing
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrLoadRegion(rx,rz,false)
        if r <> null then
            let cx = ((x+51200)%512)/16
            let cz = ((z+51200)%512)/16
            match r.TryGetChunk(cx,cz) with
            | Some _ -> r.GetSection(x,y,z)
            | _ -> null,null,null,null,null
        else null,null,null,null,null
    member this.GetOrCreateSection(x,y,z) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.GetOrCreateSection(x,y,z)
    member this.GetBlockInfo(x,y,z) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.GetBlockInfo(x,y,z)
    member this.MaybeGetBlockInfo(x,y,z) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.MaybeGetBlockInfo(x,y,z)
    member this.GetTileEntity(x,y,z) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.GetTileEntity(x,y,z)
    member this.AddTileTick(id,t,p,x,y,z) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.AddTileTick(id,t,p,x,y,z)
    member this.WriteAll() =
        for rx = -MMM to MMM-1 do
            for rz = -MMM to MMM-1 do
                let r = cachedRegions.[rx,rz]
                if r <> null then
                    let fil = System.IO.Path.Combine(folderName, sprintf "r.%d.%d.mca" rx rz)
                    r.Write(fil+".new")
                    System.IO.File.Delete(fil)
                    System.IO.File.Move(fil+".new",fil)
    member this.GetOrCreateAllSections(minx,maxx,miny,maxy,minz,maxz) =  // to force-load everything into memory cache up front
        for y in [miny .. 16 .. maxy] do
            printf "."
            for x in [minx .. 16 .. maxx] do
                for z in [minz .. 16 .. maxz] do
                    ignore <| this.GetOrCreateSection(x,y,z)  // cache each section
