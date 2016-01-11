module Utilities

module AA = ArtAssets

open MC_Constants
open NBT_Manipulation
open RegionFiles


let readDatFileStream(fs : System.IO.Stream) =
    use s = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress)
    NBT.Read(new BinaryReader2(s))

let readDatFile(filename : string) = readDatFileStream(new System.IO.FileStream(filename, System.IO.FileMode.Open))

let writeDatFile(filename : string, nbt : NBT) =
    use s = new System.IO.Compression.GZipStream(new System.IO.FileStream(filename, System.IO.FileMode.CreateNew), System.IO.Compression.CompressionMode.Compress)
    nbt.Write(new BinaryWriter2(s))


//////////////////////////////////////////////////////////////////

open System.Windows
open System.Windows.Controls
open System.Windows.Input
open System.Windows.Media

let mutable skipUnchangedChunks = true
let mutable namesToIgnore = [||]
let mutable skipUnchangedValues = true
let mutable startExpanded = false
let LIST_ITEM = "<list item>"

let rec MakeTreeDiff (Compound(_,x) as xp) (Compound(_,y) as yp) (tvp:TreeViewItem) =
    let hasDiff = ref false
    let xnames = x |> Seq.map (fun z -> z.Name) |> set
    let ynames = y |> Seq.map (fun z -> z.Name) |> set
    let names = (Set.union xnames ynames).Remove(END_NAME)
    let names = Set.difference names (set namesToIgnore)
    for n in names do
        if xnames.Contains(n) then
            let xpn = xp.[n]
            let xd = SimpleDisplay(xpn)
            if ynames.Contains(n) then
                let ypn = yp.[n]
                let yd = SimpleDisplay(ypn)
                if xd = yd then
                    if xd <> null then
                        if xpn = ypn then
                            if not skipUnchangedValues then
                                tvp.Items.Add(new TreeViewItem(Header=n+": "+xd)) |> ignore
                        else // diff byte arrays
                            tvp.Items.Add(new TreeViewItem(Header=n+": "+xd,Background=Brushes.Red)) |> ignore
                            tvp.Items.Add(new TreeViewItem(Header=n+": "+yd,Background=Brushes.Yellow)) |> ignore
                            hasDiff := true
                    else // same name compound/list
                        let tvi = new TreeViewItem(Header=n)
                        match xpn, ypn with
                        | Compound _, Compound _ ->
                            if MakeTreeDiff xpn ypn tvi then
                                tvi.Background <- Brushes.Orange
                                hasDiff := true
                        | List(_,xpay), List(_,ypay) ->
                            match xpay, ypay with
                            | Bytes xx, Bytes yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | Shorts xx, Shorts yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | Ints xx, Ints yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | Longs xx, Longs yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | Floats xx, Floats yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | Doubles xx, Doubles yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | Strings xx, Strings yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | IntArrays xx, IntArrays yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | Compounds xx, Compounds yy ->
                                // This is the hard part.  Need to be 'smart' about how to compare/merge/display
                                let ya = ResizeArray yy
                                for x in xx do
                                    let tvj = new TreeViewItem(Header=LIST_ITEM)
                                    let localDiff = ref false
                                    let BODY(i) =
                                        if i = -1 then
                                            MakeTreeDiff (Compound("",x|>ResizeArray)) (Compound("",ResizeArray())) tvj |> ignore
                                            tvj.Background <- Brushes.Red
                                            tvi.Background <- Brushes.Orange
                                            hasDiff := true
                                            localDiff := true
                                        else
                                            let y = ya.[i]
                                            ya.RemoveAt(i)
                                            if MakeTreeDiff (Compound("",x|>ResizeArray)) (Compound("",y|>ResizeArray)) tvj then
                                                tvj.Background <- Brushes.Orange
                                                tvi.Background <- Brushes.Orange
                                                hasDiff := true
                                                localDiff := true
                                    // uuid match
                                    if x |> Array.exists (function (Long("UUIDLeast",_))->true | _->false) &&
                                       x |> Array.exists (function (Long("UUIDMost",_))->true | _->false) then
                                        let xuuidl = x |> Seq.pick (function (Long("UUIDLeast",v)) -> Some v | _ -> None)
                                        let xuuidm = x |> Seq.pick (function (Long("UUIDMost",v)) -> Some v | _ -> None)
                                        let i = ya.FindIndex(fun ee -> 
                                                    ee |> Array.exists (function (Long("UUIDLeast",v)) -> v=xuuidl | _ -> false) &&
                                                    ee |> Array.exists (function (Long("UUIDMost",v)) -> v=xuuidm | _ -> false))
                                        BODY(i)
                                    // x,y,z match
                                    elif x |> Array.exists (fun e -> e.Name="x") &&
                                       x |> Array.exists (fun e -> e.Name="y") &&
                                       x |> Array.exists (fun e -> e.Name="z") then
                                        let ix = x |> Seq.pick (function (Int("x",v)) -> Some v | _ -> None)
                                        let iy = x |> Seq.pick (function (Int("y",v)) -> Some v | _ -> None)
                                        let iz = x |> Seq.pick (function (Int("z",v)) -> Some v | _ -> None)
                                        let i = ya.FindIndex(fun ee -> 
                                                    ee |> Array.exists (function (Int("x",v)) -> v=ix | _ -> false) &&
                                                    ee |> Array.exists (function (Int("y",v)) -> v=iy | _ -> false) &&
                                                    ee |> Array.exists (function (Int("z",v)) -> v=iz | _ -> false))
                                        BODY(i)
                                    // (approx) exact match
                                    elif true then
                                        let s = x.ToString()
                                        let i = ya.FindIndex(fun ee -> ee.ToString() = s)
                                        BODY(i)
                                    // TODO other kinds of heuristic matches?
                                    else
                                        MakeTreeDiff (Compound("",x|>ResizeArray)) (Compound("",ResizeArray())) tvj |> ignore
                                        tvj.Background <- Brushes.Red
                                        tvi.Background <- Brushes.Orange
                                        hasDiff := true
                                    if not skipUnchangedValues || !localDiff then
                                        tvi.Items.Add(tvj) |> ignore
                                for y in ya do
                                    let tvj = new TreeViewItem(Header=LIST_ITEM)
                                    MakeTreeDiff (Compound("",ResizeArray())) (Compound("",y|>ResizeArray)) tvj |> ignore
                                    tvj.Background <- Brushes.Yellow
                                    tvi.Background <- Brushes.Orange
                                    hasDiff := true
                                    tvi.Items.Add(tvj) |> ignore
                            | _ -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                        | _ -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                        if not skipUnchangedValues || tvi.Items.Count <> 0 then
                            tvp.Items.Add(tvi) |> ignore
                else // completely different (atoms of diff values, or diff types)
                    // TODO what best display?
                    tvp.Items.Add(new TreeViewItem(Header=n+": "+xd,Background=Brushes.Red)) |> ignore
                    tvp.Items.Add(new TreeViewItem(Header=n+": "+yd,Background=Brushes.Yellow)) |> ignore
                    hasDiff := true
            else // only x
                // TODO what best display?
                tvp.Items.Add(new TreeViewItem(Header=n+": "+xd,Background=Brushes.Red)) |> ignore
                hasDiff := true
        else // only y
            let yd = SimpleDisplay(yp.[n])
            // TODO what best display?
            tvp.Items.Add(new TreeViewItem(Header=n+": "+yd,Background=Brushes.Yellow)) |> ignore
            hasDiff := true
    !hasDiff

let diffRegions(r1:RegionFile,r2:RegionFile,regionFile1:string,regionFile2:string) =
    skipUnchangedChunks <- true
    //namesToIgnore <- [| "InhabitedTime"; "LastUpdate"; "LastOutput" |]
    namesToIgnore <- [| "InhabitedTime"; "LastUpdate"; "LastOutput"; (*"Biomes";*) "SkyLight"; "BlockLight"; "Data"; "Blocks"; "Entities" |]
    skipUnchangedValues <- true
    startExpanded <- true
    let tv = new TreeView()
    printfn "Processing chunks..."
    tv.Items.Add(new TreeViewItem(Header=regionFile1,Background=Brushes.Red)) |> ignore
    tv.Items.Add(new TreeViewItem(Header=regionFile2,Background=Brushes.Yellow)) |> ignore
    for cx = 0 to 31 do
        for cz = 0 to 31 do
            printf "(%d,%d)..." cx cz
            let n = new TreeViewItem(Header="Chunk "+cx.ToString()+","+cz.ToString())
            let c1 = r1.TryGetChunk(cx,cz)
            let c2 = r2.TryGetChunk(cx,cz)
            match c1,c2 with
            | Some c1, Some c2 ->
                if MakeTreeDiff c1 c2 n then
                    n.Background <- Brushes.Orange 
                    tv.Items.Add(n) |> ignore
                elif not skipUnchangedChunks then
                    tv.Items.Add(n) |> ignore
            | Some c1, _ ->
                if MakeTreeDiff c1 (Compound("",ResizeArray())) n then
                    n.Background <- Brushes.Red 
                    tv.Items.Add(n) |> ignore
                elif not skipUnchangedChunks then
                    tv.Items.Add(n) |> ignore
            | _, Some c2 ->
                if MakeTreeDiff (Compound("",ResizeArray())) c2 n then
                    n.Background <- Brushes.Yellow
                    tv.Items.Add(n) |> ignore
                elif not skipUnchangedChunks then
                    tv.Items.Add(n) |> ignore
            | _ -> ()
            if startExpanded then
                n.ExpandSubtree()
    printfn ""
    let window = new Window(Title="NBT Difference viewer by Dr. Brian Lorgon111", Content=tv)
    let app = new Application()
    app.Run(window) |> ignore

let diffRegionFiles(regionFile1,regionFile2) =
    let r1 = new RegionFile(regionFile1)
    let r2 = new RegionFile(regionFile2)
    diffRegions(r1,r2,regionFile1,regionFile2)

let diffDatFileStreams(datFile1,datFile2) =
    skipUnchangedChunks <- false
    namesToIgnore <- [| |]
    skipUnchangedValues <- false
    startExpanded <- false
    let tv = new TreeView()
    let r1 = readDatFileStream(datFile1)
    let r2 = readDatFileStream(datFile2)
    tv.Items.Add(new TreeViewItem(Header=datFile1,Background=Brushes.Red)) |> ignore
    tv.Items.Add(new TreeViewItem(Header=datFile2,Background=Brushes.Yellow)) |> ignore
    let n = new TreeViewItem(Header="<root>")
    if MakeTreeDiff r1 r2 n then
        n.Background <- Brushes.Orange 
        tv.Items.Add(n) |> ignore
    if startExpanded then
        n.ExpandSubtree()
    tv

let diffDatFilesGui(datFile1,datFile2) =
    let tv = diffDatFileStreams(datFile1,datFile2)
    let window = new Window(Title="NBT Difference viewer by Dr. Brian Lorgon111", Content=tv)
    let app = new Application()
    app.Run(window) |> ignore

let diffDatFilesText(datFile1,datFile2) =
    let tv = diffDatFileStreams(datFile1,datFile2)
    let a = ResizeArray()
    let (===) a b = System.Object.ReferenceEquals(a,b)
    let itemsAsSeq (c:System.Windows.Controls.ItemCollection) =
        seq {
            for x in c do
                yield x
            }
    let rec traverse(xs:seq<obj>, depth) =
        for i in xs do
            match i with
            | :? TreeViewItem as tvi ->
                let txt = tvi.Header :?> string
                let bg = tvi.Background 
                if bg === Brushes.Red then
                    a.Add((String.replicate (depth-1) "    ") + ("--- ") + txt)
                elif bg === Brushes.Yellow then
                    a.Add((String.replicate (depth-1) "    ") + ("+++ ") + txt)
                elif bg === Brushes.Orange then
                    a.Add((String.replicate (depth-1) "    ") + (">   ") + txt)
                traverse(itemsAsSeq tvi.Items, depth+1)
            | _ -> ()
    traverse(itemsAsSeq tv.Items, 1)
    for s in a do
        printfn "%s" s
    a.Count <> 0

let diffStringArrays(a1:_[],a2:_[]) =
    let mutable hasDiff = false
    // very naive diff
    let mutable i1, i2 = 0, 0
    while i1 < a1.Length && i2 < a2.Length do
        while i1 < a1.Length && i2 < a2.Length && a1.[i1] = a2.[i2] do
            i1 <- i1 + 1
            i2 <- i2 + 1
        // diff, look ahead to try to resync
        let mutable d2 = i2
        while i1 < a1.Length && d2 < a2.Length && a1.[i1] <> a2.[d2] do
            d2 <- d2 + 1
        if i1 < a1.Length && d2 < a2.Length && a1.[i1] = a2.[d2] then
            for x = i2 to d2-1 do
                printfn "+++ %s" a2.[x]
                hasDiff <- true
            i2 <- d2
        else
            let mutable d1 = i1
            while d1 < a1.Length && i2 < a2.Length && a1.[d1] <> a2.[i2] do
                d1 <- d1 + 1
            if d1 < a1.Length && i2 < a2.Length && a1.[d1] = a2.[i2] then
                for x = i1 to d1-1 do
                    printfn "--- %s" a1.[x]
                    hasDiff <- true
                i1 <- d1
            elif i1 < a1.Length && i2 < a2.Length then
                // no resync for next item of a1 or a2
                printfn "--- %s" a1.[i1]
                printfn "+++ %s" a2.[i2]
                hasDiff <- true
                i1 <- i1 + 1
                i2 <- i2 + 1
    // at least one is exhausted
    while i1 < a1.Length do
        printfn "--- %s" a1.[i1]
        hasDiff <- true
        i1 <- i1 + 1
    while i2 < a2.Length do
        printfn "+++ %s" a2.[i2]
        hasDiff <- true
        i2 <- i2 + 1
    hasDiff

///////////////////////////////////////

   
let killAllEntities() =
    let filename = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Bingo_v2_0_10\region\r.0.0.mca"""
    let regionFile = new RegionFile(filename)
    for cx = 0 to 15 do
        for cz = 0 to 15 do
            let nbt = regionFile.GetChunk(cx, cz)
            match nbt with 
            Compound(_,rsa) ->
                let theChunk = rsa.[0]
                match theChunk.TryGetFromCompound("Entities") with 
                | None -> ()
                | Some _ -> 
                    match theChunk with 
                    Compound(_cname,a) ->
                        let i = a.FindIndex (fun x -> match x with NBT.List("Entities",_) -> true | _ -> false)
                        a.[i] <- NBT.List("Entities",Compounds[||])
    regionFile.Write(filename+".new")

let makeArmorStand(x,y,z,customName,team) =
            [|
                NBT.String("id","ArmorStand")
                NBT.Byte("NoGravity",1uy)
                NBT.Byte("Invisible",1uy)
                NBT.List("Pos",Payload.Doubles [|x;y;z|])
                NBT.Byte("CustomNameVisible",1uy)
                NBT.String("CustomName",customName)
                NBT.String("Team",team)
                NBT.Byte("Marker",1uy)
                NBT.End
            |], (int x, int z)

let placeCertainEntitiesInTheWorld(entities,filename) =
    let regionFile = new RegionFile(filename)
    for cx = 0 to 15 do
        for cz = 0 to 15 do
            let nbt = regionFile.TryGetChunk(cx, cz)
            match nbt with 
            | Some( Compound(_,rsa) ) ->
                let theChunk = rsa.[0]
                match theChunk.TryGetFromCompound("Entities") with 
                | None -> ()
                | Some _ -> 
                    match theChunk with 
                    Compound(cname,a) ->
                        let i = a.FindIndex (fun x -> match x with NBT.List("Entities",_) -> true | _ -> false)
                        let es = entities |> Seq.choose (fun (e,(x,z)) -> if x/16=cx && z/16=cz then Some e else None) |> Seq.toArray 
                        a.[i] <- NBT.List("Entities",Compounds es)
            | None -> ()
    regionFile.Write(filename+".new")

let dumpSomeCommandBlocks(fil) =
    let aaa = ResizeArray()
    for filename in [fil
                     ] do
        let regionFile = new RegionFile(filename)
        
        let blockIDCounts = Array.zeroCreate 256
        for cx = 0 to 31 do
            for cz = 0 to 31 do
                try
                    let nbt = regionFile.GetChunk(cx, cz)
                    let theChunk = match nbt with Compound(_,rsa) -> rsa.[0]
                    match theChunk.TryGetFromCompound("TileEntities") with 
                    | None -> ()
                    | Some te -> 
                        match te with List(_,Compounds(tes)) ->
                        for t in tes do
                            if t |> Array.exists (function String("Command",s) -> true | _ -> false) then
                                let comm = t |> Array.pick (function String("Command",s) -> Some(string s) | _ -> None)
                                let x = t |> Array.pick (function Int("x",i) -> Some(int i) | _ -> None)
                                let y = t |> Array.pick (function Int("y",i) -> Some(int i) | _ -> None)
                                let z = t |> Array.pick (function Int("z",i) -> Some(int i) | _ -> None)
                                aaa.Add( (comm,x,y,z) )
                    let sections = match theChunk.["Sections"] with List(_,Compounds(cs)) -> cs
                    for s in sections do
                        let ySection = s |> Array.pick (function Byte("Y",y) -> Some(int y) | _ -> None)
                        let blocks = s |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
                        for i = 0 to 4095 do
                            let bid = blocks.[i]
                            blockIDCounts.[int bid] <- blockIDCounts.[int bid] + 1
                with e ->
                    if e.Message.StartsWith("chunk not") then
                        () // yeah yeah
                    else
                        printfn "%A" e.Message 

    printfn "There are %d command blocks" aaa.Count 
    let aaa = aaa.ToArray() |> Array.map (fun (s,x,y,z) -> let s = (if s.StartsWith("/") then s.Substring(1) else s) in s,x,y,z)
    let aaa = aaa |> Array.sortBy (fun (_s,x,y,z) -> y,z,x) 

    let armors = ResizeArray()
    let USE_PADDING = false   // bad usability
    let FACING_EW = false
    for (comm,x,y,z) in aaa do
        //printfn "%5d,%5d,%5d : %s" x y z comm
        // (default) facing west
        let comm = if comm.Length > 120 then comm.Substring(0,120)+"..." else comm
//        let comm = if comm = "" then "<empty>" else comm
        let comm = if comm = "" then "   " else comm
        let adjustedLen = (float comm.Length) //- (float(comm |> Seq.fold (fun x c -> x+if ",:il. []".Contains(string c) then 1 else 0) 0)/4.0)
        let deltaZ,pad = if USE_PADDING then 1.0,int(adjustedLen / 0.7) else 0.5 - (adjustedLen / 15.0),0
        let color = ((if FACING_EW then z else x)/2) % 3
        let deltaX = 0.25 + (0.25 * float color)
        let deltaX, deltaZ =
            if FACING_EW then
                deltaX, deltaZ
            else
                // facing north
                let deltaX,deltaZ = -deltaX,-deltaZ
                let deltaX,deltaZ = deltaZ,deltaX
                let deltaX = deltaX + 1.0
                let deltaZ = deltaZ + 1.0
                deltaX,deltaZ
        armors.Add(makeArmorStand(deltaX+float x, float (y+1), (float z)+deltaZ,
                                  (if USE_PADDING then (String.replicate pad " ") else "")+comm,
                                  (if color = 0 then "White" elif color = 1 then "Aqua" else "Yellow")))
    //placeCertainEntitiesInTheWorld(armors, fil)

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


// Look through statistics and achievements, discover what biomes MC thinks I have explored
let checkExploredBiomes() =
    let jsonSer = new System.Web.Script.Serialization.JavaScriptSerializer() // System.Web.Extensions.dll
    let jsonObj = jsonSer.DeserializeObject(System.IO.File.ReadAllText("""F:\.minecraft\saves\E&T Season 7\stats\6fbefbde-67a9-4f72-ab2d-2f3ee5439bc0.json"""))
    let dict : System.Collections.Generic.Dictionary<string,obj> = downcast jsonObj
    for kvp in dict do
        if kvp.Key.StartsWith("achievement.exploreAllBiomes") then
            let dict2 : System.Collections.Generic.Dictionary<string,obj> = downcast kvp.Value 
            let o = dict2.["progress"]
            let oa : obj[] = downcast o
            let sa = new ResizeArray<string>()
            for x in oa do
                sa.Add(downcast x)
            //printfn "have %d, need %d" sa.Count BIOMES_NEEDED_FOR_ADVENTURING_TIME.Length 
            let biomeSet = BIOMES_NEEDED_FOR_ADVENTURING_TIME |> set
            printfn "%d" biomeSet.Count 
            let mine = sa |> set
            let unexplored = biomeSet - mine
            printfn "%d" unexplored.Count 
            for x in biomeSet do
                printfn "%s %s" (if mine.Contains(x) then "XXX" else "   ") x
            //printfn "----"
            //for x in mine do
            //    printfn "%s %s" (if biomeSet.Contains(x) then "XXX" else "   ") x

let renamer() =
    let file = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Snakev2.0G\level.dat"""
    let nbt = readDatFile(file)
    //printfn "%s" (nbt.ToString())
    let newNbt =
        match nbt with
        | Compound("",rsa) -> 
            match rsa.[0], rsa.[1] with Compound("Data",a), End -> 
                let a = a |> Seq.filter (function String("LevelName",_) -> false | _ -> true)
                let a = a |> Seq.append [|String("LevelName","Snake Game by Lorgon111")|]
                Compound("",[|Compound("Data",a|>ResizeArray);End|]|>ResizeArray)
        | _ -> failwith "bummer"
    printfn "%s" (newNbt.ToString())
    writeDatFile(file + ".new", newNbt)

let ALPHABET5 = 
    [|
    "XXXX.XXX..XXXX.XXX..XXXX.XXXX.XXXX.X..X.XXXX....X.X..X.X....X..X.X..X.XXXX.XXXX.XXXX.XXXX.XXXX.XXXX.X..X.X..X.X..X.X..X.X..X.XXXX..X.X...XX................XXX.."
    "X..X.X..X.X....X..X.X....X....X....X..X..XX.....X.X..X.X....XXXX.XX.X.X..X.X..X.X..X.X..X.X.....XX..X..X.X..X.X..X.X..X.X..X....X..X.X....X..................X.."
    "XXXX.XXX..X....X..X.XXXX.XXXX.X.XX.XXXX..XX.....X.XXX..X....X..X.X.XX.X..X.XXXX.X..X.XXXX.XXXX..XX..X..X.X..X.X..X..XX...XX...XX.......................X.....X.."
    "X..X.X..X.X....X..X.X....X....X..X.X..X..XX..X..X.X..X.X....X..X.X..X.X..X.X....X.XX.X.X.....X..XX..X..X.XXXX.XXXX.X..X..XX..X...............XX...XX.........X.."
    "X..X.XXX..XXXX.XXXX.XXXX.X....XXXX.X..X.XXXX..XX..X..X.XXXX.X..X.X..X.XXXX.X....XXXX.X..X.XXXX..XX..XXXX..XX..X..X.X..X..XX..XXXX............XX....X.......XXXX."
    |]
let ALPHABET5INDEX(c) =
    if c >= 'A' && c <= 'Z' then
        Some(int c - int 'A')
    elif c = '"' then
        Some 26
    elif c = ''' then
        Some 27
    elif c = '.' then
        Some 28
    elif c = ',' then
        Some 29
    elif c = '*' then
        Some 30
    elif c = '1' then
        Some 31
    else 
        None

let placeVideoFramesInTheWorld() =
    let folder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\VoidTravolta\region\"""
    let map = new MapFolder(folder)
    for frame = 1 to 16 do
        let targetImage = sprintf """C:\Users\Admin1\Desktop\MC pixel art pics\CT%02d.jpg""" frame
        let _bmp = PhotoToMinecraft.computeMinecraft(128,128,targetImage)
        let maxHeight = 100 + PhotoToMinecraft.pictureBlockFilenames.GetLength(1)-1
        for x = 0 to PhotoToMinecraft.pictureBlockFilenames.GetLength(0)-1 do
            for y = 0 to PhotoToMinecraft.pictureBlockFilenames.GetLength(1)-1 do
                let filename = System.IO.Path.GetFileNameWithoutExtension(PhotoToMinecraft.pictureBlockFilenames.[x,y]).ToLower()
                if x = 0 then printfn "%3d %s" y filename
                let (_,bid,dmg) = textureFilenamesToBlockIDandDataMapping |> Array.find (fun (n,_,_) -> n = filename)
                map.EnsureSetBlockIDAndDamage( -450 + 50 * frame, maxHeight - y, 100 + x, byte bid, byte dmg)
    map.WriteAll()


let placeCertainBlocksInTheWorld() =
    let filename = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\spawnchunks\region\r.0.0.mca"""
    let regionFile = new RegionFile(filename)
    printfn "%A" (PhotoToMinecraft.finalBmp = null)
    let maxHeight = 100 + PhotoToMinecraft.pictureBlockFilenames.GetLength(1)-1
    for x = 0 to PhotoToMinecraft.pictureBlockFilenames.GetLength(0)-1 do
        for y = 0 to PhotoToMinecraft.pictureBlockFilenames.GetLength(1)-1 do
            let filename = System.IO.Path.GetFileNameWithoutExtension(PhotoToMinecraft.pictureBlockFilenames.[x,y]).ToLower()
            if x = 0 then printfn "%3d %s" y filename
            let (_,bid,dmg) = textureFilenamesToBlockIDandDataMapping |> Array.find (fun (n,_,_) -> n = filename)
            regionFile.EnsureSetBlockIDAndDamage( 1, maxHeight - y, 100 + x, byte bid, byte dmg)
    // caption (on 128-wide image, can fit about 24 letters across a line)
    //          123456789012345678901234
    let center24(s:string) = (String.replicate ((24 - s.Length)/2) " ") + s
    let top1 = center24 ""
    let top2 = center24 "" //"DUDE"
    let bot1 = center24 "" //"FAR OUT, MAN"
    let bot2 = center24 ""
    let caption(x, topy, leftz, text:string) =
        let mutable z = leftz
        for c in text do
            match ALPHABET5INDEX c with
            | Some i ->
                for j = 0 to 4 do
                    for k = 0 to 4 do
                        if ALPHABET5.[4-j].[5*i+k] = 'X' then
                            regionFile.SetBlockIDAndDamage( x, topy+j, z+k, 155uy, 0uy)  // 155 = quartz block
                        else
                            regionFile.SetBlockIDAndDamage( x, topy+j, z+k, 173uy, 0uy)  // 173 = coal block
            | None -> ()
            z <- z + 5
    caption(1, 220, 105, top1)
    caption(1, 213, 105, top2)
    caption(1, 117, 105, bot1)
    caption(1, 110, 105, bot2)
    regionFile.Write(filename+".new")
    System.IO.File.Delete(filename)
    System.IO.File.Move(filename+".new",filename)

let placeCertainBlocksInTheSky() =
    let filename = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\spawnchunks\region\r.0.0.mca"""
    let regionFile = new RegionFile(filename)
    printfn "%A" (PhotoToMinecraft.finalBmp = null)
    for x = 0 to PhotoToMinecraft.pictureBlockFilenames.GetLength(0)-1 do
        for z = 0 to PhotoToMinecraft.pictureBlockFilenames.GetLength(1)-1 do
            let filename = System.IO.Path.GetFileNameWithoutExtension(PhotoToMinecraft.pictureBlockFilenames.[x,z]).ToLower()
            let (_,bid,dmg) = textureFilenamesToBlockIDandDataMapping |> Array.find (fun (n,_,_) -> n = filename)
            regionFile.SetBlockIDAndDamage( x+64, 237, z+64, byte bid, byte dmg)
    regionFile.Write(filename+".new")

let dumpTileTicks(file) =
    let region = new RegionFile(file)
    for x = 0 to 31 do
        for z = 0 to 31 do
            try
                let theChunk = region.GetChunk(x,z)
                let theChunkLevel = match theChunk with Compound(_,rsa) -> rsa.[0] // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
                let ticks = match theChunkLevel.["TileTicks"] with List(_,Compounds(cs)) -> cs
                for t in ticks do
                    let x = (t |> Array.find(fun n -> n.Name="x") |> fun x -> x.ToString())
                    let y = (t |> Array.find(fun n -> n.Name="y") |> fun x -> x.ToString())
                    let z = (t |> Array.find(fun n -> n.Name="z") |> fun x -> x.ToString())
                    let i = (t |> Array.find(fun n -> n.Name="i") |> fun x -> x.ToString())
                    let tt = (t |> Array.find(fun n -> n.Name="t") |> fun x -> x.ToString())
                    let p = (t |> Array.find(fun n -> n.Name="p") |> fun x -> x.ToString())
                    printfn "%s %s %s     %s %s %s" x y z i tt p
            with e ->
                ()

let removeAllTileTicks(fil) =
    let region = new RegionFile(fil)
    for x = 0 to 31 do
        for z = 0 to 31 do
            try
                let theChunk = region.GetChunk(x,z)
                let theChunkLevel = match theChunk with Compound(_,rsa) -> rsa.[0] // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
                match theChunkLevel with
                | Compound(_n,a) ->
                    let i = a.FindIndex(fun x -> x.Name = "TileTicks")
                    a.[i] <- List("TileTicks",Compounds[||])
            with e ->
                ()
    region.Write(fil+".new")
    System.IO.File.Delete(fil)
    System.IO.File.Move(fil+".new",fil)

let editMapDat(file) =
    let nbt = readDatFile(file)
    printfn "%s" (nbt.ToString())
    let nbt = cataNBT (fun _pl nbt -> 
        match nbt with 
        |NBT.Compound("data",_) ->  Compound("data",[|
                                                    Byte("scale",0uy)
                                                    Byte("dimension",0uy)
                                                    Short("height",128s)
                                                    Short("width",128s)
                                                    Int("xCenter",64+128)
                                                    Int("zCenter",64)
                                                    ByteArray("colors",Array.zeroCreate 16384)
                                                    End
                                                    |] |> ResizeArray)
        | _ -> nbt) (fun _pl nbt -> nbt) [] nbt
    printfn "%s" (nbt.ToString())
    writeDatFile(file+".new", nbt)

let mapDatToPng(mapDatFile:string, newPngFilename:string) =
    let nbt = readDatFile(mapDatFile)
    let out = new System.Drawing.Bitmap(128, 128)
    let nbt = cataNBT (fun _pl nbt -> 
        match nbt with 
        | NBT.Compound("data",a) ->
            match a |> Seq.find(fun x -> x.Name = "colors") with
            | ByteArray(_,colorArray) ->
                for x = 0 to 127 do
                    for y = 0 to 127 do
                        let b = colorArray.[x+128*y]
                        let r,g,b = MAP_COLOR_TABLE |> Array.find (fun (x,y) -> x = int b) |> snd
                        out.SetPixel(x,y,System.Drawing.Color.FromArgb(r,g,b))
            id nbt
        | _ -> nbt) (fun _pl nbt -> nbt) [] nbt
    out.Save(newPngFilename, System.Drawing.Imaging.ImageFormat.Png)

let updateDat(file,f) =
    let nbt = readDatFile(file)
    printfn "%s" (nbt.ToString())
    let nbt = cataNBT f (fun _pl nbt -> nbt) [] nbt
    printfn "%s" (nbt.ToString())
    if System.IO.File.Exists(file+".new") then
        System.IO.File.Delete(file+".new")
    writeDatFile(file+".new", nbt)
    if System.IO.File.Exists(file+".old") then
        System.IO.File.Delete(file+".old")
    System.IO.File.Move(file,file+".old")
    System.IO.File.Move(file+".new",file)

let dumpPlayerDat(file) =
    let nbt = readDatFile(file)
    printfn "%s" (nbt.ToString())

    (*
    // allow commands
    match nbt with
    | Compound(_,a) ->
        match a.[0] with
        | Compound("Data",a) ->
            match a |> Array.tryFindIndex (fun x -> x.Name="allowCommands") with
            | None -> ()
            | Some i ->
                a.[i] <- Byte("allowCommands",1uy)
    (*
    // replace LevelName
    match nbt with
    | Compound(_,a) ->
        match a.[0] with
        | Compound("Data",a) ->
            match a |> Array.tryFindIndex (fun x -> x.Name="LevelName") with
            | None -> ()
            | Some i ->
                a.[i] <- String("LevelName","MinecraftBINGOv2_5")
                *)
                (*
    // mesa bryce
    match nbt with
    | Compound(_,a) ->
        match a.[0] with
        | Compound("Data",a) ->
            match a |> Array.tryFindIndex (fun x -> x.Name="generatorOptions") with
            | None -> ()
            | Some i ->
                match a.[i] with
                | String(_,s) ->
                    let newOpts = s.Substring(0,s.IndexOf("fixedBiome")) + "fixedBiome\":165" + s.Substring(s.IndexOf("fixedBiome")+14)
                    a.[i] <- String("generatorOptions",newOpts)
    *)
    writeDatFile(file+".new", nbt)
    *)
////////////////////////////////////////////////////


let findAllLoot(regionFolder:string) =
    let mutable abandonedMineshaftChestCount = 0
    let mutable desertPyramidChestCount = 0
    let mutable iglooChestCount = 0
    let mutable jungleTempleChestCount = 0
    let mutable simpleDungeonChestCount = 0
    let mutable strongholdCorridorChestCount = 0
    let mutable strongholdCrossingChestCount = 0
    let mutable strongholdLibraryChestCount = 0
    let mutable villageBlacksmithChestCount = 0

    (*
    let counts = new System.Collections.Generic.Dictionary<_,_>()
    let total = ref 0
    *)
    let files = System.IO.Directory.EnumerateFiles(regionFolder,"*.mca") |> Seq.toArray 
    for i = 0 to files.Length-1 do
        printfn "%d of %d" i files.Length 
        let fil = files.[i]
        let r = new RegionFile(fil)
        for cx = 0 to 31 do
            for cz = 0 to 31 do
                match r.TryGetChunk(cx,cz) with
                | None -> ()
                | Some c ->
                    let theChunkLevel = match c with Compound(_,rsa) -> rsa.[0] // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
                    (*
                    let lookAllItemsCore(items:NBT[][]) =
                        let ebs = ResizeArray()
                        for item in items do
                            if item |> Array.exists(function NBT.String("id","minecraft:enchanted_book") -> true | _ -> false) then
                                total := !total + 1
                                let enchs = ResizeArray()
                                match item |> Array.find(fun x -> x.Name = "tag") with
                                | Compound(_,cs) ->
                                    match cs |> Array.find(fun x -> x.Name = "StoredEnchantments") with
                                    | List(_,Compounds(xs)) ->
                                        for x in xs do
                                            let id = x |> Array.find (fun x -> x.Name = "id") |> function Short(_,v) -> v
                                            let lvl = x |> Array.find (fun x -> x.Name = "lvl") |> function Short(_,v) -> v
                                            let name,_,_ = ENCHANTS |> Array.find(fun (_,_,i) -> i = int(id)) 
                                            enchs.Add(name,lvl)
                                ebs.Add(enchs)
                        for eb in ebs do
                            for ench in eb do
                                let name,lvl = ench
                                if counts.ContainsKey(name) then
                                    counts.[name] <- counts.[name]+1
                                else
                                    counts.[name] <- 1
                        ebs
                    let lookAllItems(nbta:NBT[]) =
                        let items = match nbta |> Array.find(fun x -> x.Name = "Items") with | List(_,Compounds(cs)) -> cs
                        lookAllItemsCore(items)
                    *)
                    let tileEntities = 
                        match theChunkLevel.["TileEntities"] with 
                        | List(_,Compounds(cs)) -> cs
                        | _ -> [||]
                    for nbta in tileEntities do
                        try
                        if nbta |> Array.exists (function NBT.String("id","Chest") -> true | _ -> false) then
                            let x = nbta |> Array.find (fun x -> x.Name = "x") |> function Int(_,v) -> v
                            let y = nbta |> Array.find (fun x -> x.Name = "y") |> function Int(_,v) -> v
                            let z = nbta |> Array.find (fun x -> x.Name = "z") |> function Int(_,v) -> v
                            // determine type of chest
                            let bi = r.GetBlockInfo(x,y-1,z)
                            if bi.BlockID = 24uy then   // sandstone
                                desertPyramidChestCount <- desertPyramidChestCount + 1
                            elif bi.BlockID = 47uy then   // bookshelf
                                strongholdLibraryChestCount <- strongholdLibraryChestCount + 1
                            elif bi.BlockID = 5uy then   // wood planks
                                if r.GetBlockInfo(x+1,y,z).BlockID = 0uy && 
                                   r.GetBlockInfo(x-1,y,z).BlockID = 0uy && 
                                   r.GetBlockInfo(x,y,z+1).BlockID = 0uy && 
                                   r.GetBlockInfo(x,y,z-1).BlockID = 0uy then
                                    strongholdCrossingChestCount <- strongholdCrossingChestCount + 1  // has air all around
                                else
                                    strongholdLibraryChestCount <- strongholdLibraryChestCount + 1  // against a wall
                            elif bi.BlockID = 98uy then   // stone brick
                                if (let b = r.GetBlockInfo(x+1,y-1,z) in b.BlockID = 98uy && b.BlockData = 3uy) ||  // chiseled
                                   (let b = r.GetBlockInfo(x-1,y-1,z) in b.BlockID = 98uy && b.BlockData = 3uy) || 
                                   (let b = r.GetBlockInfo(x,y-1,z+1) in b.BlockID = 98uy && b.BlockData = 3uy) || 
                                   (let b = r.GetBlockInfo(x,y-1,z-1) in b.BlockID = 98uy && b.BlockData = 3uy) then
                                    iglooChestCount <- iglooChestCount + 1
                                else
                                    strongholdCorridorChestCount <- strongholdCorridorChestCount + 1
                            elif bi.BlockID = 48uy || bi.BlockID = 4uy then   // moss stone or cobblestone
                                if y > 60 then // once had dungeon intersect stronghold, got marked as jungle temple, this is good extra check
                                    if r.GetBlockInfo(x+1,y,z).BlockID = 5uy ||  // planks
                                       r.GetBlockInfo(x-1,y,z).BlockID = 5uy || 
                                       r.GetBlockInfo(x,y,z+1).BlockID = 5uy || 
                                       r.GetBlockInfo(x,y,z-1).BlockID = 5uy then
                                        villageBlacksmithChestCount <- villageBlacksmithChestCount + 1
                                    elif r.GetBlockInfo(x+1,y+1,z).BlockID = 23uy ||   // dispenser
                                       r.GetBlockInfo(x-1,y+1,z).BlockID = 23uy || 
                                       r.GetBlockInfo(x,y+1,z+1).BlockID = 23uy || 
                                       r.GetBlockInfo(x,y+1,z-1).BlockID = 23uy then
                                        //printfn "jung %d %d %d had %d" x y z bi.BlockID 
                                        jungleTempleChestCount <- jungleTempleChestCount + 1
                                    elif r.GetBlockInfo(x+1,y+1,z).BlockID = 98uy ||   // (chiseled) stone brick
                                         r.GetBlockInfo(x-1,y+1,z).BlockID = 98uy || 
                                         r.GetBlockInfo(x,y+1,z+1).BlockID = 98uy || 
                                         r.GetBlockInfo(x,y+1,z-1).BlockID = 98uy then
                                        //printfn "jung %d %d %d had %d" x y z bi.BlockID 
                                        jungleTempleChestCount <- jungleTempleChestCount + 1
                                    else
                                        simpleDungeonChestCount <- simpleDungeonChestCount + 1
                                else
                                    simpleDungeonChestCount <- simpleDungeonChestCount + 1
                            else
                                printfn "%d %d %d had %d" x y z bi.BlockID 
                                if bi.BlockID = 0uy then   // air is most likely a dungeon intersecting a cave
                                    simpleDungeonChestCount <- simpleDungeonChestCount + 1
                                (*
                            let ebs = lookAllItems(nbta)
                            if ebs.Count > 0 then
                                //printfn "Found Chest at (%d,%d,%d) with" x y z
                                for eb in ebs do
                                    for ench in eb do
                                        let name,lvl = ench
                                        //printf "%30s %2d   " name lvl
                                        ()
                                    //printfn ""
                                *)
                        with e ->
                            printfn "coords outside region? %s" (e.Message)  // swallow exception
                    let entities = 
                        match theChunkLevel.["Entities"] with 
                        | List(_,Compounds(cs)) -> cs
                        | _ -> [||]
                    for nbta in entities do
                        (*
                        if nbta |> Array.exists (function NBT.String("id","Item") -> true | _ -> false) then
                            let item = nbta |> Array.find (function NBT.Compound("Item",_) -> true | _ -> false) |> (function NBT.Compound("Item",tags) -> tags)
                            lookAllItemsCore [| item |] |> ignore
                            *)
                        if nbta |> Array.exists (function NBT.String("id","MinecartChest") -> true | _ -> false) then
                            abandonedMineshaftChestCount <- abandonedMineshaftChestCount + 1
                                (*
                            let ebs = lookAllItems(nbta)
                            if ebs.Count > 0 then
                                let x,y,z = nbta |> Array.find (fun x -> x.Name = "Pos") |> function List(_,Payload.Doubles([|x;y;z|])) -> x,y,z
                                //printfn "Found MinecartChest at (%4.2f,%4.2f,%4.2f) with" x y z
                                for eb in ebs do
                                    for ench in eb do
                                        let name,lvl = ench
                                        //printf "%30s %2d   " name lvl
                                        ()
                                    //printfn ""
                                *)
    (*
    for v,n in counts |> Seq.map (function KeyValue(n,v) -> v,n) |> Seq.sortBy fst do
        printfn "%4.1f%%  %-30s  (%4d/%4d)" (100.0 * float v / float (!total)) n v (!total)
    printfn "%d books" !total
    *)
    printfn "%5d   abandonedMineshaftChestCount" abandonedMineshaftChestCount
    printfn "%5d   desertPyramidChestCount" desertPyramidChestCount
    printfn "%5d   iglooChestCount" iglooChestCount 
    printfn "%5d   jungleTempleChestCount" jungleTempleChestCount
    printfn "%5d   simpleDungeonChestCount" simpleDungeonChestCount
    printfn "%5d   strongholdCorridorChestCount" strongholdCorridorChestCount 
    printfn "%5d   strongholdCrossingChestCount" strongholdCrossingChestCount 
    printfn "%5d   strongholdLibraryChestCount" strongholdLibraryChestCount 
    printfn "%5d   villageBlacksmithChestCount" villageBlacksmithChestCount
    ()
(*

    18739   abandonedMineshaftChestCount
      144   desertPyramidChestCount
       22   iglooChestCount
       42   jungleTempleChestCount
    13682   simpleDungeonChestCount
      100   strongholdCorridorChestCount
       27   strongholdCrossingChestCount
       91   strongholdLibraryChestCount
       31   villageBlacksmithChestCount


*)


(*
     0.2%  Projectile Protection           (  16/6982)
     1.9%  Feather Falling                 ( 135/6982)
     2.2%  Silk Touch                      ( 151/6982)
     2.3%  Thorns                          ( 158/6982)
     2.4%  Infinity                        ( 169/6982)
     3.5%  Blast Protection                ( 243/6982)
     4.1%  Punch                           ( 286/6982)
     4.1%  Respiration                     ( 287/6982)
     4.1%  Lure                            ( 288/6982)
     4.2%  Frost Walker                    ( 292/6982)
     4.2%  Fortune                         ( 294/6982)
     4.2%  Depth Strider                   ( 296/6982)
     4.3%  Flame                           ( 302/6982)
     4.4%  Fire Aspect                     ( 308/6982)
     4.4%  Aqua Affinity                   ( 310/6982)
     4.7%  Luck of the Sea                 ( 326/6982)
     4.8%  Looting                         ( 332/6982)
     9.0%  Bane of Arthropods              ( 628/6982)
     9.3%  Smite                           ( 649/6982)
     9.8%  Fire Protection                 ( 681/6982)
    10.8%  Knockback                       ( 754/6982)
    11.3%  Unbreaking                      ( 792/6982)
    17.8%  Sharpness                       (1244/6982)
    19.5%  Protection                      (1360/6982)
    20.3%  Efficiency                      (1414/6982)
    20.7%  Power                           (1443/6982)
*)

let testing() =
    let fil = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Seed9917 - Copy35e\region\r.0.0.mca"""
    let r = new RegionFile(fil)
    let s = AA.readZoneIntoString(r,71,208,67,16,1,16)
    printfn "%s" s
    AA.writeZoneFromString(r,71,213,67,s)

    r.Write(fil+".new")
    System.IO.File.Delete(fil)
    System.IO.File.Move(fil+".new",fil)

let testing2() =
    let fil = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Seed9917 - Copy35e\region\r.0.0.mca"""
    let r = new RegionFile(fil)
    let arr = ResizeArray()
    for i = 0 to 1 do
        for j = 0 to 1 do
            let s = AA.readZoneIntoString(r,64+i*64,208,64+j*64,63,0,63)
            //printfn "%s" s
            arr.Add(sprintf """let xxx%d%d = "%s" """ i j s)
    let writePath = """C:\Users\brianmcn\Documents\Visual Studio 2012\Projects\MinecraftMapManipulator\MinecraftMapManipulator\Tutorial.fsx"""
    System.IO.File.WriteAllLines(writePath, arr)

let mixTerrain() =
    let extremeHillsBiomeIDs = BIOMES |> Seq.filter(fun (_,n,_) -> n.StartsWith("Ex")) |> Seq.map (fun (x,_,_) -> x) |> Seq.map byte |> Set.ofSeq 
    for rx in [-1;0] do
        for rz in [-1;0] do
            let RI1 = new RegionFile(sprintf """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Seed5Normal\region\r.%d.%d.mca""" rx rz)
            let RI2 = new RegionFile(sprintf """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Seed6Normal\region\r.%d.%d.mca""" rx rz)
            let ROFile = sprintf """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Seed5Mix6\region\r.%d.%d.mca""" rx rz
            let RO  = new RegionFile(ROFile)
            for cx = 0 to 31 do
                for cz = 0 to 31 do
                    match RI1.TryGetChunk(cx,cz),RI2.TryGetChunk(cx,cz) with
                    | Some ci1, Some ci2 ->
(*
                        let chunkLevel = match ci1 with Compound(_,[|c;_|]) -> c | Compound(_,[|c;_;_|]) -> c  // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name (or two with a data version appended)
                        match chunkLevel with 
                        | Compound(n,nbts) -> 
                            let biomes = nbts |> Array.find (fun nbt -> nbt.Name = "Biomes")
                            match biomes with
                            | NBT.ByteArray(_,a) ->
                                if a |> Array.exists (fun b -> extremeHillsBiomeIDs.Contains(b)) then
                                    // there's extreme hills in this chunk
                                    printfn "XH %d, %d  ---   %d, %d"  rx rz  cx cz
                                    RO.SetChunk(cx,cz,ci2)
                                else
                                    printfn "NN %d, %d  ---   %d, %d"  rx rz  cx cz
                                    RO.SetChunk(cx,cz,ci1)
*)
                        printfn "OK %d, %d  ---   %d, %d"  rx rz  cx cz
                        if (cx + cz) % 2 = 0 then
                            RO.SetChunk(cx,cz,ci1)
                        else
                            RO.SetChunk(cx,cz,ci2)
                    | Some _, None -> 
                                    printfn "no Amp  data for %d, %d  ---   %d, %d"  rx rz  cx cz
                    | None, Some _-> 
                                    printfn "no Norm data for %d, %d  ---   %d, %d"  rx rz  cx cz
                    | None, None -> 
                                    printfn "no Any  data for %d, %d  ---   %d, %d"  rx rz  cx cz
            RO.Write(ROFile+".new")
    ()

let findStrongholds() =
    let world = "43a"
    for rx in [-16 .. 15] do
        for rz in [-16 .. 15] do
            let file = sprintf """r.%d.%d.mca""" rx rz
            let region = new RegionFile((sprintf """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\%s\region\""" world) + file)
//            printfn "scanning %s" file
            for cx = 0 to 31 do
                for cz = 0 to 31 do
                    match region.TryGetChunk(cx,cz) with
                    | None -> ()
                    | _ ->
                        let x = rx * 512 + cx * 16
                        let z = rz * 512 + cz * 16
                        for sy = 0 to 15 do
                            let y = 16 * sy
                            match region.TryGetSection(x,y,z) with
                            | None -> ()
                            | Some (_s,blocks,_bd) ->
                                if blocks |> Array.exists (fun b -> b = 120uy) then // 120 = end portal frame
                                    if y < 64 then // below ground
                                        printfn "end portal at (%6d, %3d, %6d)" x y z
    ()
    (*

RAW
end portal at ( -6944,  32,   -416)
end portal at ( -6944,  32,   -400)
end portal at ( -6752,  32,   2160)
end portal at ( -6752,  32,   2176)
end portal at ( -6736,  32,   2160)
end portal at ( -6736,  32,   2176)
end portal at ( -5376,  16,  -2544)
end portal at ( -5392,  16,   4384)
end portal at ( -5392,  16,   4400)
end portal at ( -5376,  16,   4384)
end portal at ( -5376,  16,   4400)
end portal at ( -4976,  16,  -5136)
end portal at ( -4960,  16,  -5136)
end portal at ( -3120,  16,    992)
end portal at ( -3120,  16,   1008)
end portal at ( -3344,  16,   1136)
end portal at ( -3344,  16,   1152)
end portal at ( -2608,  32,  -6304)
end portal at ( -2848,  32,   4912)
end portal at ( -1120,  32,    448)
end portal at ( -1040,  16,   6784)
end portal at ( -1024,  16,   6784)
end portal at (  -176,  16,  -5792)
end portal at (   144,  32,   -592)
end portal at (   144,  32,   -576)
end portal at (   752,  32,  -2688)
end portal at (   672,  32,  -2560)
end portal at (   672,  16,    496)
end portal at (  1280,  32,   5840)
end portal at (  2304,  32,  -6400)
end portal at (  2304,  32,  -6384)
end portal at (  2080,  32,   2048)
end portal at (  2528,  16,   2496)
end portal at (  2544,  16,   2496)
end portal at (  3248,  32,   5088)
end portal at (  4352,  16,  -5088)
end portal at (  4928,  32,   3664)
end portal at (  4928,  32,   3680)
end portal at (  4944,  32,   3664)
end portal at (  4944,  32,   3680)
end portal at (  5696,  16,  -2896)
end portal at (  5872,  16,   1664)
end portal at (  6848,  16,   -688)
end portal at (  6864,  16,   -688)
end portal at (  6896,  32,   8000)
end portal at (  6896,  32,   8016)

COOKED
    end portal at ( -6944,  32,   -416)
    end portal at ( -6752,  32,   2160)
    end portal at ( -5376,  16,  -2544)
    end portal at ( -5392,  16,   4384)
    end portal at ( -4976,  16,  -5136)
    end portal at ( -3120,  16,    992)
    end portal at ( -3344,  16,   1136)
    end portal at ( -2608,  32,  -6304)
    end portal at ( -2848,  32,   4912)
    end portal at ( -1120,  32,    448)
    end portal at ( -1040,  16,   6784)
    end portal at (  -176,  16,  -5792)
    end portal at (   144,  32,   -592)
    end portal at (   752,  32,  -2688)
    end portal at (   672,  32,  -2560)
    end portal at (   672,  16,    496)
    end portal at (  1280,  32,   5840)
    end portal at (  2304,  32,  -6400)
    end portal at (  2080,  32,   2048)
    end portal at (  2528,  16,   2496)
    end portal at (  3248,  32,   5088)
    end portal at (  4352,  16,  -5088)
    end portal at (  4928,  32,   3664)
    end portal at (  5696,  16,  -2896)
    end portal at (  5872,  16,   1664)
    end portal at (  6848,  16,   -688)
    end portal at (  6896,  32,   8000)

    *)

/////////////////////////////

let makeWrittenBookTags(author, title, pages) =
    (*
    //    /give @p minecraft:written_book 1 0 {title:"hello"}
    NBT.Byte("Count",1uy)
    NBT.Short("Damage",0s)
    NBT.String("id","minecraft:written_book")
    NBT.Compound("tag",[||])
    *)
    [|
    Byte("resolved",0uy)  // player can open book and have his name appear in it :)
    Int("generation",0)
    String("author",author)
    String("title",title)
    List("pages",Strings(pages))   //      "[\"line1\\n\",\"line2\"]", ...
    End
    |]

// TODO below does not work if '^' is in the original text
let escape(s:string) = s.Replace("\"","^").Replace("\\","\\\\").Replace("^","\\\"")    //    "  \    ->    \"   \\
let escape2(s) = escape(escape(s))

let writtenBookNBTString(author, title, pages:string[]) =
    let sb = System.Text.StringBuilder()
    sb.Append(sprintf "{resolved:0b,generation:0,author:\"%s\",title:\"%s\",pages:[" author title) |> ignore
    for i = 0 to pages.Length-2 do
        sb.Append("\"") |> ignore
        sb.Append(escape pages.[i]) |> ignore
        sb.Append("\",") |> ignore
    sb.Append("\"") |> ignore
    sb.Append(escape pages.[pages.Length-1]) |> ignore
    sb.Append("\"") |> ignore
    sb.Append("]}") |> ignore
    sb.ToString()

let makeCommandGivePlayerWrittenBook(author, title, pages:string[]) =
    sprintf "/give @p minecraft:written_book 1 0 %s" (writtenBookNBTString(author, title, pages))

let wrapInJSONText(s) = sprintf """{"text":"%s"}""" s
let wrapInJSONTextContinued(s) = sprintf """{"text":"%s\n\n(continued...)"}""" s

let placeCommandBlocksInTheWorldTemp(fil) =
    let region = new RegionFile(fil)
#if INSTANTLY_HIGHLIGHT_AN_XYZ_REGION
    let MAX = 63
    let STEPS = [32;16;8;4;2;1]
    let cmdsInit =
        [|
            yield O ""
            yield U "kill @e[type=!Player]"
            yield U "scoreboard objectives add Count dummy"
            yield U "say summon corners guy and hi"
            yield O ""
            for step in STEPS do
                yield U (sprintf "execute @e[tag=guy] ~ ~ ~ summon AreaEffectCloud ~%d ~ ~ {Duration:999999,Tags:[\"guy\"]}" step)
                yield U (sprintf "execute @e[tag=hi] ~ ~ ~ kill @e[tag=guy,dx=%d,dy=-%d,dz=-%d]" step MAX MAX)
                yield U (sprintf "execute @e[tag=guy] ~ ~ ~ summon AreaEffectCloud ~ ~%d ~ {Duration:999999,Tags:[\"guy\"]}" step)
                yield U (sprintf "execute @e[tag=hi] ~ ~ ~ kill @e[tag=guy,dx=-%d,dy=%d,dz=-%d]" MAX step MAX)
                yield U (sprintf "execute @e[tag=guy] ~ ~ ~ summon AreaEffectCloud ~ ~ ~%d {Duration:999999,Tags:[\"guy\"]}" step)
                yield U (sprintf "execute @e[tag=hi] ~ ~ ~ kill @e[tag=guy,dx=-%d,dy=-%d,dz=%d]" MAX MAX step)
            yield U "scoreboard players set @p Count 0"
            yield U "execute @e[tag=guy] ~ ~ ~ scoreboard players add @p Count 1"
            yield U """tellraw @a ["there are ",{"score":{"name":"@p","objective":"Count"}}]"""
            yield U "/execute @e[tag=guy] ~ ~ ~ particle barrier ~ ~ ~ 0 0 0 0"
        |]
    region.PlaceCommandBlocksStartingAt(3,56,3,cmdsInit,"bug repro")
    region.PlaceCommandBlocksStartingAt(4,56,3,[|
                                                 P ""
                                                 U "execute @e[type=Wolf] ~ ~ ~ summon AreaEffectCloud ~ ~ ~ {Duration:999999,Tags:[\"guy\"]}"
                                                 U "execute @e[type=Bat] ~ ~ ~ summon AreaEffectCloud ~ ~ ~ {Duration:999999,Tags:[\"hi\"]}"
                                                 U "kill @e[type=Wolf]"
                                                 U "kill @e[type=Bat]"
                                                 |],"bug repro")
#endif

    region.Write(fil+".new")
    System.IO.File.Delete(fil)
    System.IO.File.Move(fil+".new",fil)

let makeBiomeMapFromRegions(regionFolder, rxs:int list, rzs:int list, decorations) =
    let negXCount = rxs |> Seq.filter (fun x -> x<0) |> Seq.length 
    let negZCount = rzs |> Seq.filter (fun z -> z<0) |> Seq.length 
    let image = new System.Drawing.Bitmap(512*rxs.Length, 512*rzs.Length)
    for rx in rxs do
        for rz in rzs do
            let RI = new RegionFile(sprintf """%s\r.%d.%d.mca""" regionFolder rx rz)
            for cx = 0 to 31 do
                for cz = 0 to 31 do
                    match RI.TryGetChunk(cx,cz) with
                    | Some c ->
                        let chunkLevel = match c with Compound(_,rsa) -> rsa.[0]  // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name (or two with a data version appended)
                        match chunkLevel with 
                        | Compound(n,nbts) -> 
                            let biomes = nbts |> Seq.find (fun nbt -> nbt.Name = "Biomes")
                            match biomes with
                            | NBT.ByteArray(_,a) ->
                                for x = 0 to 15 do
                                    for y = 0 to 15 do
                                        let i = 16*y+x
                                        let biome = int a.[i]
                                        let mapColorIndex = BIOMES |> Array.find (fun (b,_,_) -> b=biome) |> (fun (_,_,color) -> color)
                                        let mci,(r,g,b) = MAP_COLOR_TABLE.[mapColorIndex]
                                        assert(mci = mapColorIndex)
                                        let xx = rx * 512 + cx * 16 + x + 512*negXCount
                                        let yy = rz * 512 + cz * 16 + y + 512*negZCount
                                        let r,g,b = 
                                            if (xx%512=0) || (yy%512=0) then
                                                r/2,g/2,b/2  // dark lines on region bounds
                                            else
                                                r,g,b
                                        image.SetPixel(xx, yy, System.Drawing.Color.FromArgb(r,g,b))
    let placeRedLetterAt(letter, centerX, centerZ) =
        let D = 9
        let ix = 512*negXCount + centerX - 5*(D+1)
        let iy = 512*negZCount + centerZ - 5*(D+1)
        match ALPHABET5INDEX letter with
        | Some i ->
            for j = 0 to 4 do
                for k = 0 to 4 do
                    if ALPHABET5.[j].[5*i+k] = 'X' then
                        for dx = 0 to D do
                            for dy = 0 to D do
                                try
                                    image.SetPixel(ix+k*D+dx, iy+j*D+dy, System.Drawing.Color.FromArgb(255,0,0))
                                with _ -> () // if goes off edge, just don't draw
        | None -> failwith "bad letter"
    let mapFolder = System.IO.Path.GetDirectoryName(regionFolder)
    image.Save(System.IO.Path.Combine(mapFolder,"mapOverview.png"))
    for (c,x,z) in decorations do
        placeRedLetterAt(c,x,z)
    image.Save(System.IO.Path.Combine(mapFolder,"mapOverviewWithLocationSpoilers.png"))

let makeBiomeMap(regionFolder, map:MapFolder, origBiome:byte[,], biome:byte[,], hmIgnoringLeaves:int[,], xmin, xlen:int, zmin, zlen:int, circleRadii, decorations) =
    let image = new System.Drawing.Bitmap(xlen,zlen)
    let isCircle(distSq) =
        let mutable c = false
        for r in circleRadii do
            let RM = (r-1)*(r-1)
            let R = (r+1)*(r+1)
            if distSq > RM && distSq < R then
                c <- true
        c
    let draw(biome:_[,],showHard) =
        for x = xmin to xmin+xlen-1 do
            for y = zmin to zmin+zlen-1 do
                let biome = biome.[x,y]
                let mapColorIndex = BIOMES |> Array.find (fun (b,_,_) -> b=int biome) |> (fun (_,_,color) -> color)
                let mci,(r,g,b) = MAP_COLOR_TABLE.[mapColorIndex]
                assert(mci = mapColorIndex)
                let r,g,b = 
                    if (x%512=0) || (y%512=0) then
                        r/2,g/2,b/2  // dark lines on region bounds
                    elif isCircle(x*x+y*y) then
                        r/2,g/2,b/2  // dark lines on circle
                    else
                        if x/6%2=0 && y/6%2=0 then
                            if hmIgnoringLeaves.[x,y] > 100 then // TODO named constants
                                r*1/3,g*1/3,b*1/3
                            elif hmIgnoringLeaves.[x,y] > 80 then // TODO named constants
                                r*2/3,g*2/3,b*2/3
                            else
                                r,g,b
                        else
                            r,g,b
                let r,g,b = 
                    // only in the non-spoilers...
                    if showHard && map.GetInhabitedTime(x,y) >= 3600000L then
                        r/2,g/2,b/2  // darken 'hard' areas
                    else
                        r,g,b
                image.SetPixel(x-xmin, y-zmin, System.Drawing.Color.FromArgb(r,g,b))
    draw(origBiome,false)
    let mapFolder = System.IO.Path.GetDirectoryName(regionFolder)
    image.Save(System.IO.Path.Combine(mapFolder,"mapOverview.png"))

    let placeRedLetterAt(letter, centerX, centerZ) =
        let D = 9
        let ix = centerX - 4*(D+1)/2  // letters 4 wide
        let iy = centerZ - 5*(D+1)/2  // letters 5 tall
        match ALPHABET5INDEX letter with
        | Some i ->
            for j = 0 to 4 do
                for k = 0 to 4 do
                    if ALPHABET5.[j].[5*i+k] = 'X' then
                        for dx = 0 to D do
                            for dy = 0 to D do
                                try
                                    image.SetPixel(ix+k*D+dx-xmin, iy+j*D+dy-zmin, System.Drawing.Color.FromArgb(255,0,0))
                                with _ -> () // if goes off edge, just don't draw
        | None -> failwith "bad letter"
        //image.SetPixel(centerX-xmin, centerZ-zmin, System.Drawing.Color.FromArgb(0,255,255)) // remove this, just for debugging
    draw(biome,true)
    for (c,x,z) in decorations do
        placeRedLetterAt(c,x,z)
    image.Save(System.IO.Path.Combine(mapFolder,"mapOverviewWithLocationSpoilers.png"))


