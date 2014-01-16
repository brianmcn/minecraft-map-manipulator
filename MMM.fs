let BIOMES = 
    [|0,"Ocean";1,"Plains";2,"Desert";3,"Extreme Hills";4,"Forest";5,"Taiga";6,"Swampland";7,"River";8,"Hell";9,"Sky";
      10,"Frozen Ocean";11,"FrozenRiver";12,"Ice Plains";13,"Ice Mountains";14,"MushroomIsland";15,"MushroomIslandShore";16,"Beach";17,"DesertHills";18,"ForestHills";19,"TaigaHills";
      20,"Extreme Hills Edge";21,"Jungle";22,"Jungle Hills";23,"Jungle Edge";24,"Deep Ocean";25,"Stone Beach";26,"Cold Beach";27,"Birch Forest";28,"Birch Forest Hills";29,"Roofed Forest";
      30,"Cold Taiga";31,"Cold Taiga Hills";32,"Mega Taiga";33,"Mega Taiga Hills";34,"Extreme Hills+";35,"Savanna";36,"Savanna Plateau";37,"Mesa";38,"Mesa Plateau F";39,"Mesa Plateau";
      129,"Sunflower Plains";
      130,"Desert M";131,"Extreme Hills M";132,"Flower Forest";133,"Taiga M";134,"Swampland M";
      140,"Ice Plains Spikes";141,"Ice Mountains Spikes";149,"Jungle M";
      151,"JungleEdge M";155,"Birch Forest M";156,"Birch Forest Hills M";157,"Roofed Forest M";158,"Cold Taiga M";
      160,"Mega Spruce Taiga";161,"Mega Spruce Taiga";162,"Extreme Hills+ M";163,"Savanna M";164,"Savanna Plateau M";165,"Mesa (Bryce)";166,"Mesa Plateau F M";167,"Mesa Plateau M";
      -1,"(Uncalculated)"|]

let ENCHANTS =
    [|0,"Protection";1,"Fire Protection";2,"Feather Falling";3,"Blast Protection";4,"Projectile Protection";5,"Respiration";6,"Aqua Affinity";7,"Thorns";
      16,"Sharpness";17,"Smite";18,"Bane of Arthropods";19,"Knockback";20,"Fire Aspect";21,"Looting";
      32,"Efficiency";33,"Silk Touch";34,"Unbreaking";35,"Fortune";
      48,"Power";49,"Punch";50,"Flame";51,"Infinity";
      61,"Luck of the Sea";62,"Lure"|]

let POTION_EFFECTS =
    [|1,"Speed";2,"Slowness";3,"Haste";4,"Mining Fatigue";5,"Strength";6,"Instant Health";7,"Instant Damage";8,"Jump Boost";9,"Nausea";
      10,"Regeneration";11,"Resistance";12,"Fire Resistance";13,"Water Breathing";14,"Invisibility";15,"Blindness";16,"Night vision";17,"Hunger";18,"Weakness";19,"Poison";
      20,"Wither";21,"Health Boost";22,"Absorption";23,"Saturation"|]

let WOOL_COLORS = [|0,"White";1,"Orange";2,"Magenta";3,"Light Blue";4,"Yellow";5,"Lime";6,"Pink";7,"Gray";8,"Light Gray";9,"Cyan";10,"Purple";11,"Blue";12,"Brown";13,"Green";14,"Red";15,"Black"|]

let BLOCK_IDS =
    [|0,"Air";1,"Stone";2,"Grass Block";3,"Dirt";4,"Cobblestone";5,"Wood Planks";6,"Saplings";7,"Bedrock";8,"Water";9,"Stationary water";
      10,"Lava";11,"Stationary lava";12,"Sand";13,"Gravel";14,"Gold Ore";15,"Iron Ore";16,"Coal Ore";17,"Wood";18,"Leaves";19,"Sponge";
      20,"Glass";21,"Lapis Lazuli Ore";22,"Lapis Lazuli Block";23,"Dispenser";24,"Sandstone";25,"Note Block";26,"Bed";27,"Powered Rail";28,"Detector Rail";29,"Sticky Piston";
      30,"Cobweb";31,"Grass";32,"Dead Bush";33,"Piston";34,"Piston Extension";35,"Wool";36,"Block moved by Piston";37,"Dandelion";38,"Poppy";39,"Brown Mushroom";
      40,"Red Mushroom";41,"Block of Gold";42,"Block of Iron";43,"Double Stone Slab";44,"Stone Slab";45,"Bricks";46,"TNT";47,"Bookshelf";48,"Moss Stone";49,"Obsidian";
      50,"Torch";51,"Fire";52,"Monster Spawner";53,"Oak Wood Stairs";54,"Chest";55,"Redstone Wire";56,"Diamond Ore";57,"Block of Diamond";58,"Crafting Table";59,"Wheat";
      60,"Farmland";61,"Furnace";62,"Burning Furnace";63,"Sign Post";64,"Wooden Door";65,"Ladders";66,"Rail";67,"Cobblestone Stairs";68,"Wall Sign";69,"Lever";
      70,"Stone Pressure Plate";71,"Iron Door";72,"Wooden Pressure Plate";73,"Redstone Ore";74,"Glowing Redstone Ore";75,"Redstone Torch (inactive)";76,"Redstone Torch (active)";77,"Stone Button";78,"Snow";79,"Ice";
      80,"Snow Block";81,"Cactus";82,"Clay";83,"Sugar Cane";84,"Jukebox";85,"Fence";86,"Pumpkin";87,"Netherrack";88,"Soul Sand";89,"Glowstone";
      90,"Nether Portal";91,"Jack 'o' Lantern";92,"Cake Block";93,"Redstone Repeater (inactive)";94,"Redstone Repeater (active)";95,"Locked Chest";96,"Trapdoor";97,"Monster Egg";98,"Stone Bricks";99,"Huge Brown Mushroom";
      100,"Huge Red Mushroom";101,"Iron Bars";102,"Glass Pane";103,"Melon";104,"Pumpkin Stem";105,"Melon Stem";106,"Vines";107,"Fence Gate";108,"Brick Stairs";109,"Stone Brick Stairs";
      110,"Mycelium";111,"Lily Pad";112,"Nether Brick";113,"Nether Brick Fence";114,"Nether Brick Stairs";115,"Nether Wart";116,"Enchantment Table";117,"Brewing Stand";118,"Cauldron";119,"End Portal";
      120,"End Portal Block";121,"End Stone";122,"Dragon Egg";123,"Redstone Lamp (inactive)";124,"Redstone Lamp (active)";125,"Wooden Double Slab";126,"Wooden Slab";127,"Cocoa";128,"Sandstone Stairs";129,"Emerald Ore";
      130,"Ender Chest";131,"Tripwire Hook";132,"Tripwire";133,"Block of Emerald";134,"Spruce Wood Stairs";135,"Birch Wood Stairs";136,"Jungle Wood Stairs";137,"Command Block";138,"Beacon";139,"Cobblestone Wall";
      140,"Flower Pot";141,"Carrots";142,"Potatoes";143,"Wooden Button";144,"Mob Head";145,"Anvil";146,"Trapped Chest";147,"Weighted Pressure Plate (Light)";148,"Weighted Pressure Plate (Heavy)";149,"Redstone Comparator (inactive)";
      150,"Redstone Comparator (active)";151,"Daylight Sensor";152,"Block of Redstone";153,"Nether Quartz Ore";154,"Hopper";155,"Block of Quartz";156,"Quartz Stairs";157,"Activator Rail";158,"Dropper";159,"Stained Clay";
      170,"Hay Block";171,"Carpet";172,"Hardened Clay";173,"Block of Coal";174,"Packed Ice";175,"Large Flowers"|]

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
    | Strings of string[]
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
    member this.TryGetFromCompound(s:string) =
        match this with
        | Compound(_n,a) -> a |> Array.tryFind (fun x -> x.Name = s)
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
            if n = "TileTicks" then prefix + n + " : [] (NOTE: skipping data)" else
            let sb = new System.Text.StringBuilder(prefix + n + " : [] ")
            let p = "    " + prefix
            match pay with
            | Bytes(a) -> sb.Append(a.Length.ToString() + " bytes") |> ignore
            | Shorts(a) -> sb.Append(a.Length.ToString() + " shorts") |> ignore
            | Ints(a) -> sb.Append(a.Length.ToString() + " ints") |> ignore
            | Longs(a) -> sb.Append(a.Length.ToString() + " longs") |> ignore
            | Floats(a) -> 
                if a.Length > 2 then 
                    sb.Append(a.Length.ToString() + " floats") |> ignore 
                else 
                    sb.Append((a |> Array.fold (fun s x -> s + (x.ToString()) + " ") " : [ ") + "]") |> ignore
            | Doubles(a) -> sb.Append(a.Length.ToString() + " doubles") |> ignore
            | Strings(a) -> for s in a do sb.Append("\""+s+"\"  ") |> ignore
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
                | 10uy ->Compounds(Array.init len (fun _ -> readCompoundPayload()))
                | 11uy ->IntArrays(Array.init len (fun _ -> let innerLen = s.ReadInt32() in Array.init innerLen (fun _ -> s.ReadInt32())))
                | _ -> failwith "unimplemented list kind"
            List(n,payload)
        | 10uy ->
            let n = NBT.ReadName(s)
            Compound(n, readCompoundPayload())
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
            //| ByteArrays(a) -> bw.Write(7uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Compounds(a) -> bw.Write(10uy); bw.Write(a.Length); for x in a do (for y in x do y.Write(bw); assert(x.[x.Length-1] = End))
            | IntArrays(a) -> bw.Write(11uy); bw.Write(a.Length); for x in a do for y in x do bw.Write(y)
        | Compound(n,xs) -> bw.Write(10uy); NBT.WriteName(bw,n); for x in xs do x.Write(bw); assert(xs.[xs.Length-1] = End)
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
                    if xn=yn && xs.Length=ys.Length then 
                        (None,xs,ys) |||> Array.fold2 (fun s xx yy -> match s with None -> diff(xx,yy,xn::path) | s -> s) 
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

type BlockInfo(blockID:byte, blockData:byte, tileEntity:NBT option) =
    member this.BlockID = blockID
    member this.BlockData = blockData
    member this.TileEntity = tileEntity

type RegionFile(filename) =
    let rx, rz =
        let m = System.Text.RegularExpressions.Regex.Match(filename, """.*r\.(.*)\.(.*)\.mca$""")
        int m.Groups.[1].Value, int m.Groups.[2].Value
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
    member this.GetChunk(cx, cz) =
        match chunks.[cx,cz] with
        | End -> failwith "chunk not represented, NYI"
        | c -> c
    member this.GetBlockInfo(x, y, z) =
        if x/512 <> rx || z/512 <> rz then failwith "coords outside this region"
        let theChunk = 
            match chunks.[(x%512)/16,(z%512)/16] with
            | End -> failwith "chunk not represented, NYI"
            | c -> c
        let theChunk = match theChunk with Compound(_,[|c;_|]) -> c // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
        let sections = match theChunk.["Sections"] with List(_,Compounds(cs)) -> cs
        let theSection = sections |> Array.find (Array.exists (function Byte("Y",n) when n=byte(y/16) -> true | _ -> false))  // TODO cope with missing sections (air)
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
                let tes = cs |> Array.choose (fun te -> 
                    let te = Compound("unnamedDummyToCarryAPayload",te)
                    if te.["x"]=Int("x",x) && te.["y"]=Int("y",y) && te.["z"]=Int("z",z) then Some te else None)
                if tes.Length = 0 then None
                elif tes.Length = 1 then Some tes.[0]
                else failwith "unexpected: multiple TileEntities with same xyz coords"
            | _ -> None
        new BlockInfo(blocks.[i], blockData.[i], tileEntity)

let readDatFile(filename : string) =
    use s = new System.IO.Compression.GZipStream(new System.IO.FileStream(filename, System.IO.FileMode.Open), System.IO.Compression.CompressionMode.Decompress)
    NBT.Read(new BinaryReader2(s))

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
    let bi = regionFile.GetBlockInfo(165,65,211)
    printfn "%A %A %A" bi.BlockID bi.BlockData bi.TileEntity  // planks at floor of hut

    let structures = [ //"VILLAGES", """F:\.minecraft\saves\FindHut\data\villages.dat"""
                       //"TEMPLE", """F:\.minecraft\saves\E&T 1_3\data\Temple.dat"""
                       "TEMPLE", """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\E&T 1_3later\data\Temple.dat"""
                       //"TEMPLE", """F:\.minecraft\saves\FindHut\data\Temple.dat"""
                       //"MINESHAFT", """F:\.minecraft\saves\FindHut\data\Mineshaft.dat"""
                     ]
    printfn "E&Tafterupdate"
    for name, file in structures do
        printfn ""
        printfn "%s" name
        try
            let vnbt = readDatFile(file)
            printfn "%s" (vnbt.ToString())
        with e -> printfn "error %A" e
    printfn "======================="

    let MakeSyntheticWitchArea(lowX, lowY, lowZ, hiX, hiY, hiZ) = 
        let bb = [| lowX; lowY; lowZ; hiX; hiY; hiZ |]
        let chunkX = lowX / 16
        let chunkZ = lowZ / 16
        Compound("", [|
            Compound("data", [|
                Compound("Features", [|
                    Compound(sprintf "[%d,%d]" chunkX chunkZ, [|
                        String("id", "Temple")
                        Int("ChunkX", chunkX)
                        Int("ChunkZ", chunkZ)
                        IntArray("BB", bb)
                        List("Children", Compounds[|[|
                            String("id", "TeSH")
                            Int("GD", 0) //?
                            Int("HPos", -1)
                            IntArray("BB", bb)
                            Int("Height", hiY - lowY + 1)
                            Int("Width", hiX - lowX + 1)
                            Int("Depth", hiZ - lowZ + 1)
                            Int("Witch", 0) //?
                            Int("O", 1)  //?
                            End
                            |]|])
                        End
                        |])
                    End
                    |])
                End
                |])
            End
            |])

    let synthetic = MakeSyntheticWitchArea(651, 54, 651, 750, 83, 750)
    printfn "%s" (synthetic.ToString())
    use s = new System.IO.Compression.GZipStream(new System.IO.FileStream("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\E&T 1_3later\data\Temple.dat""", System.IO.FileMode.CreateNew), System.IO.Compression.CompressionMode.Compress)
    synthetic.Write(new BinaryWriter2(s))
    s.Close()
   

let main() =
    let filename = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\E&T 1_3later\region\r.1.1.mca"""
    // I want to read chunk (12,12), which is coords (704,704) and is near the WWF board
    let regionFile = new RegionFile(filename)

    let blockIDCounts = Array.zeroCreate 256
    for cx = 0 to 15 do
        for cz = 0 to 15 do
            let nbt = regionFile.GetChunk(cx, cz)
//            printfn "%s" (nbt.ToString())
            let theChunk = match nbt with Compound(_,[|c;_|]) -> c
//            printfn "%s" theChunk.Name 
            let biomeData = match theChunk.["Biomes"] with ByteArray(_n,a) -> a
            for i = 0 to 255 do biomeData.[i] <- 14uy // Moo
            match theChunk.TryGetFromCompound("TileEntities") with 
            | Some te -> printfn "%s" (te.ToString())
            | None -> ()
            let sections = match theChunk.["Sections"] with List(_,Compounds(cs)) -> cs
            for s in sections do
                let ySection = s |> Array.pick (function Byte("Y",y) -> Some(int y) | _ -> None)
                let blocks = s |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
                for i = 0 to 4095 do
                    let bid = blocks.[i]
                    blockIDCounts.[int bid] <- blockIDCounts.[int bid] + 1
                    if bid = 116uy then
                        // let i = dy*256 + dz*16 + dx
                        printfn "ench tab at (%d,%d,%d)" (regionFile.RX*512 + cx*16 + (i%16)) (ySection * 16 + (i/256)) (regionFile.RZ*512 + cz*16 + ((i%256)/16))

    printfn "============="
    //let bi = regionFile.GetBlockInfo(686,63,648) // chest in my area
    let bi = regionFile.GetBlockInfo(761,65,767)  // ench table
    printfn "%A %A %A" bi.BlockID bi.BlockData bi.TileEntity  

//    printfn "============="
//    printfn "Block counts"
//    for i = 0 to blockIDCounts.Length-1 do
//        if blockIDCounts.[i] <> 0 then
//            printfn "%7d - %s" blockIDCounts.[i] (BLOCK_IDS |> Array.find (fun (n,_) -> n=i) |> snd)
//    printfn "============="

    let copy = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\E&T 1_3later\region\copy.r.1.1.mca"""
    regionFile.Write(copy)
    let copyRegionFile = new RegionFile(copy)
    printfn "DIFFS:"
    for cx = 0 to 31 do
        for cz = 0 to 31 do
            let a = regionFile.GetChunk(cx,cz)
            let b = copyRegionFile.GetChunk(cx,cz)
            match a.Diff(b) with
            | None -> ()
            | Some(path,m,n) ->
                printfn "chunk (%d,%d) differs at %s" cx cz path
                printfn "%s" (m.ToString())
                printfn "----"
                printfn "%s" (n.ToString())
    printfn "(end of DIFFS)"


    (*
    //let filename = """F:\.minecraft\saves\BingoGood\region\r.0.0.mca"""
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
    *)
    ()

let diagnoseStringDiff(s1 : string, s2 : string) =
    if s1 = s2 then printfn "same!" else
    let mutable i = 0
    while i < s1.Length && i < s2.Length && s1.[i] = s2.[i] do
        i <- i + 1
    if i = s1.Length then 
        printfn "first ended at pos %d whereas second still has more %s" s1.Length (s2.Substring(s1.Length))
    elif i = s2.Length then 
        printfn "second ended at pos %d whereas first still has more %s" s2.Length (s1.Substring(s2.Length))
    else
        let j = i - 20
        let j = if j < 0 then 0 else j
        printfn "first diff at position %d" i
        printfn ">>>>>"
        printfn "%s" (s1.Substring(j, 40))
        printfn "<<<<<"
        printfn "%s" (s2.Substring(j, 40))

let testReadWriteRegionFile() =
    let filename = """F:\.minecraft\saves\BingoGood\region\r.0.0.mca"""
    let out1 = """F:\.minecraft\saves\BingoGood\region\out1.r.0.0.mca"""
    let out2 = """F:\.minecraft\saves\BingoGood\region\out2.r.0.0.mca"""
    // load up orig file, show some data
    let origFile = new RegionFile(filename)
    let nbt = origFile.GetChunk(12, 12)
    let origString = nbt.ToString()
    // write out a copy
    origFile.Write(out1)
    // try to read in the copy, see if data same
    let out1File = new RegionFile(out1)
    let nbt = out1File.GetChunk(12, 12)
    let out1String = nbt.ToString()
    diagnoseStringDiff(origString, out1String)
    // write out a copy
    out1File.Write(out2)
    // try to read in the copy, see if data same
    let out2File = new RegionFile(out2)
    let nbt = out2File.GetChunk(12, 12)
    let out2String = nbt.ToString()
    diagnoseStringDiff(origString, out2String)
    

(*
    // finding diff in byte arrays
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
    ()


// Look through statistics and achievements, discover what biomes MC thinks I have explored
let main3() =
    let jsonSer = new System.Web.Script.Serialization.JavaScriptSerializer() // System.Web.Extensions.dll
    let jsonObj = jsonSer.DeserializeObject(System.IO.File.ReadAllText("""F:\.minecraft\saves\E&T Season 6\stats\brianmcn.json"""))
    let dict : System.Collections.Generic.Dictionary<string,obj> = downcast jsonObj
    for kvp in dict do
        if kvp.Key.StartsWith("achievement.exploreAllBiomes") then
            let dict2 : System.Collections.Generic.Dictionary<string,obj> = downcast kvp.Value 
            let o = dict2.["progress"]
            let oa : obj[] = downcast o
            let sa = new ResizeArray<string>()
            for x in oa do
                sa.Add(downcast x)
            printfn "%d, %d" sa.Count BIOMES.Length 
            let biomeSet = BIOMES |> Array.map snd |> set
            printfn "%d" biomeSet.Count 
            let mine = sa |> set
            let unexplored = biomeSet - mine
            printfn "%d" unexplored.Count 
            for x in biomeSet do
                printfn "%s %s" (if mine.Contains(x) then "XXX" else "   ") x
            printfn "----"
            for x in mine do
                printfn "%s %s" (if biomeSet.Contains(x) then "XXX" else "   ") x


printfn "hi"
//main()
//main2()
//testReadWriteRegionFile()
main3()
