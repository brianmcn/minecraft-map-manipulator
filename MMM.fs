open NBT_Manipulation
open RegionFiles
open Utilities

#if AWESOME_CONWAY_LIFE
let placeCommandBlocksInTheWorld(fil) =
    let region = new RegionFile(fil)
    let DURATION = 999999
    let entityType = "AreaEffectCloud"
    let entityDefaults = sprintf ",Duration:%d" DURATION
    let nearbys = [| "~-1 ~ ~-1"; "~0 ~ ~-1"; "~1 ~ ~-1"; "~-1 ~ ~0"; "~1 ~ ~0"; "~-1 ~ ~1"; "~0 ~ ~1"; "~1 ~ ~1" |]
    let cmds = 
        [|
            yield P ""
            for i = 0 to 7 do
                let nearby = nearbys.[i]
                if i >= 3 then
                    yield U (sprintf "execute @e[tag=live] %s fill ~ ~-1 ~ ~ ~-1 ~ wool 4 replace wool 3" nearby)
                if i >= 2 then
                    yield U (sprintf "execute @e[tag=live] %s fill ~ ~-1 ~ ~ ~-1 ~ wool 3 replace wool 2" nearby)
                if i >= 1 then
                    yield U (sprintf "execute @e[tag=live] %s fill ~ ~-1 ~ ~ ~-1 ~ wool 2 replace wool 1" nearby)
                yield U (sprintf "execute @e[tag=live] %s fill ~ ~-1 ~ ~ ~-1 ~ wool 1 replace wool 0" nearby)
            yield U (sprintf "execute @e[tag=live] ~ ~ ~ detect ~ ~-1 ~ wool 2 summon %s ~ ~ ~ {Tags:[\"keep\"]%s}" entityType entityDefaults)
            yield U (sprintf "execute @e[tag=live] ~ ~ ~ detect ~ ~-1 ~ wool 3 summon %s ~ ~ ~ {Tags:[\"keep\"]%s}" entityType entityDefaults)
            yield U "execute @e[tag=live] ~ ~ ~ detect ~ ~-1 ~ wool 0 setblock ~ ~ ~ wool 0"
            yield U "execute @e[tag=live] ~ ~ ~ detect ~ ~-1 ~ wool 1 setblock ~ ~ ~ wool 0"
            yield U "execute @e[tag=live] ~ ~ ~ detect ~ ~-1 ~ wool 4 setblock ~ ~ ~ wool 0"
            yield U "execute @e[tag=live] ~ ~ ~ setblock ~ ~-1 ~ wool 0"
            for i = 0 to 7 do
                let nearby = nearbys.[i]
                yield U (sprintf "execute @e[tag=live] %s detect ~ ~-1 ~ wool 3 summon %s ~ ~ ~ {Tags:[\"new\"]%s}" nearby entityType entityDefaults)
                yield U (sprintf "execute @e[tag=live] %s setblock ~ ~-1 ~ wool 0" nearby)
            yield U "kill @e[tag=live]"
            yield U "entitydata @e[tag=new] {Tags:[\"live\"]}"
            yield U "execute @e[tag=live] ~ ~ ~ setblock ~ ~ ~ wool 15"
            yield U "entitydata @e[tag=keep] {Tags:[\"live\"]}"
            yield U "scoreboard players set Alive Count 0"
            yield U "execute @e[tag=live] ~ ~ ~ scoreboard players add Alive Count 1"
            yield U "scoreboard players add Ticks Count 1"
        |]
    region.PlaceCommandBlocksStartingAt(20,10,20,cmds)
    let aux = [|
                P ""
                U (sprintf "execute @e[type=Sheep] ~ ~ ~ summon %s ~ ~-1 ~ {Tags:[\"live\"]%s}" entityType entityDefaults)
                U "execute @e[type=Sheep] ~ ~ ~ setblock ~ ~-1 ~ wool 15"
                U "kill @e[type=Sheep]"
              |]
    region.PlaceCommandBlocksStartingAt(24,10,20,aux)
    let aux2 = [|
                O ""
                U (sprintf "execute @e[type=%s] ~ ~ ~ setblock ~ ~ ~ wool 0" entityType)
                U (sprintf "kill @e[type=%s]" entityType)
                U "fill -160 2 -160 0 2 0 wool"
                U "fill 0 2 -160 160 2 0 wool"
                U "fill -160 2 0 0 2 160 wool"
                U "fill 0 2 0 160 2 160 wool"
                U "fill -160 3 -160 0 3 0 wool" 
                U "fill 0 3 -160 160 3 0 wool"
                U "fill -160 3 0 0 3 160 wool"
                U "fill 0 3 0 160 3 160 wool"
                U "scoreboard players reset *"
               |]
    region.PlaceCommandBlocksStartingAt(28,10,20,aux2)
#endif


let testBackpatching(fil) =
    let r = new RegionFile(fil)
    r.PlaceCommandBlocksStartingAt(1,5,1,[|
        O ""
        U "say 1"
        U "BLOCKDATA ON 1"
        U "BLOCKDATA OFF 1"
        U "say 2"
        C "BLOCKDATA ON 2"
        C "BLOCKDATA OFF 2"
        U "say 3"
        O "TAG 2"
        U "say 6"
        U "say 7"
        O "TAG 1"
        U "say 4"
        U "say 5"
        |],"yadda")
    r.Write(fil+".new")
    System.IO.File.Delete(fil)
    System.IO.File.Move(fil+".new",fil)

////////////////////////////////////////////

let preciseImageToBlocks(imageFilename:string,regionFolder, baseY) =
    let image = new System.Drawing.Bitmap(imageFilename)
    let m = new MapFolder(regionFolder)
    let colorTable= new System.Collections.Generic.Dictionary<_,_>()
    let knownColors = 
        [|
            (255uy, 51uy, 102uy, 153uy),   (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 95uy, 11uy))   // blue glass water
            (255uy, 255uy, 255uy, 255uy),  (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 80uy, 0uy))    // white snow
            (255uy, 0uy, 102uy, 0uy),      (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 35uy, 13uy))   // green wool tree
            (255uy, 102uy, 102uy, 102uy),  (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 7uy, 0uy))     // dark mountain
            (255uy, 0uy, 204uy, 0uy),      (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 2uy, 0uy))     // green grass
            (255uy, 255uy, 51uy, 0uy),     (fun x y z -> for dy in [0;1;2;3] do m.EnsureSetBlockIDAndDamage(x, baseY+dy, z, 152uy, 0uy))   // red wall
            (255uy, 153uy, 153uy, 153uy),  (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 1uy, 0uy))     // grey stone
            (255uy, 255uy, 255uy, 0uy),    (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 41uy, 0uy))    // gold thingy
            (255uy, 204uy, 255uy, 255uy),  (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 174uy, 0uy))   // light blue ice
            (255uy, 153uy, 102uy, 51uy),   (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 3uy, 2uy))     // brown podzol
            (255uy, 0uy, 0uy, 0uy),        (fun x y z -> ())                                               // black means air
            (255uy, 255uy, 102uy, 0uy),    (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 86uy, 11uy))   // orange pumpkin
            (255uy, 153uy, 51uy, 0uy),     (fun x y z -> ()) // TODO
            (255uy, 0uy, 255uy, 0uy),      (fun x y z -> ()) // TODO
            (255uy, 255uy, 204uy, 153uy),      (fun x y z -> ()) // TODO
            (255uy, 153uy, 255uy, 102uy),      (fun x y z -> ()) // TODO
        |]
    knownColors |> Seq.iter (fun ((a,r,g,b),f) -> colorTable.Add(System.Drawing.Color.FromArgb(int a, int r, int g, int b), f))
    let mutable nextNumber = 0
    let XM = max (image.Width-1) 511
    let ZM = max (image.Height-1) 511
    for x = 0 to XM do
        for z = 0 to ZM do
            let c = image.GetPixel(x,z)
            colorTable.[c] x 10 z
            (*
            let n =
                if colorTable.ContainsKey(c) then 
                    colorTable.[c] 
                else
                    colorTable.Add(c,nextNumber)
                    nextNumber <- nextNumber + 1
                    nextNumber - 1
            r.SetBlockIDAndDamage(x, 10, z, 35uy, byte n)  // 35 = wool
    colorTable |> Seq.map (fun (KeyValue(c,n)) -> n, (c.A, c.R, c.G, c.B)) |> Seq.sortBy fst |> Seq.iter (fun (_,c) -> printfn "%A" c)
            *)
        printfn "%d of %d" x XM
    m.WriteAll()


////////////////////////////////////////////

open System.IO.Compression

let compareMinecraftAssets(jar1, jar2) =
    use archive1 = ZipFile.OpenRead(jar1)
    use archive2 = ZipFile.OpenRead(jar2)
    let a1 = ResizeArray()
    let a2 = ResizeArray()
    for e in archive1.Entries do
        if e.FullName.StartsWith("assets/minecraft/loot_tables") ||
                e.FullName.StartsWith("assets/minecraft/structures") ||
                e.FullName.StartsWith("assets/minecraft/texts") then
            a1.Add(e.FullName)
    for e in archive2.Entries do
        if e.FullName.StartsWith("assets/minecraft/loot_tables") ||
                e.FullName.StartsWith("assets/minecraft/structures") ||
                e.FullName.StartsWith("assets/minecraft/texts") then
            a2.Add(e.FullName)
    a1.Sort()
    a2.Sort()
    let mutable diffCount = 0
    printfn "FILE LIST DIFF"
    if diffStringArrays(a1.ToArray(), a2.ToArray()) then
        diffCount <- diffCount + 1
    printfn "=============="
    for name in a1 do
        let entry1 = archive1.GetEntry(name)
        let entry2 = archive2.GetEntry(name)
        if entry1 <> null && entry2 <> null then
            if System.IO.Path.GetExtension(name).ToLowerInvariant() = ".nbt" then
                printfn "%s" (name.ToUpper())
                if diffDatFilesText(entry1.Open(), entry2.Open(),false) then
                    printfn "CHANGED!"
                    diffCount <- diffCount + 1
                printfn "=============="
            else
                printfn "%s" (name.ToUpper())
                let a1 = ResizeArray()
                let s1 = new System.IO.StreamReader(entry1.Open())
                while not s1.EndOfStream do
                    a1.Add(s1.ReadLine())
                let a2 = ResizeArray()
                let s2 = new System.IO.StreamReader(entry2.Open())
                while not s2.EndOfStream do
                    a2.Add(s2.ReadLine())
                if diffStringArrays(a1.ToArray(), a2.ToArray()) then
                    diffCount <- diffCount + 1
                printfn "=============="
    printfn ""
    printfn "Total diffs found: %d" diffCount
////////////////////////////////////////////

open System.Diagnostics 

let genTerrainWithMCServer(seed, customizedPreset) =
    let serverFolder = """C:\Users\Admin1\Desktop\MC SERVER\"""
    let jar = """minecraft_server.15w49a.jar"""
    let psi = new ProcessStartInfo(UseShellExecute=false, RedirectStandardInput=true, RedirectStandardOutput=true) 
    psi.WorkingDirectory <- serverFolder
    psi.FileName <- "java" 
    psi.Arguments <- sprintf "-Xms1024M -Xmx1024M -d64 -jar %s nogui" jar
    // TODO
    // server prop before level.dat (delete whole world folder)
    // setworldspawn 0 80 0, stop server, restart
    if false then
        System.IO.File.WriteAllLines(serverFolder+"server.properties",
            [|
                sprintf "generator-settings=%s" customizedPreset
                "level-type=CUSTOMIZED"
                sprintf "level-seed=%d" seed
                "enable-command-block=true"
                "gamemode=1"
                "force-gamemode=true"
            |])
    //System.Threading.Thread.Sleep(8000)
    let proc = new Process(StartInfo=psi) 
    proc.Start() |> ignore 
    let rec rcvloop() = 
        let data = proc.StandardOutput.ReadLine() 
        if data <> null then 
            printfn "MC: %s" data
            rcvloop() 
    let t = new System.Threading.Thread(rcvloop) 
    t.Start() 
    System.Threading.Thread.Sleep(5000)
    //proc.StandardInput.WriteLine("/stop")
    //System.Threading.Thread.Sleep(500000)
    let userInput = false
    if userInput then
        let mutable s = stdin.ReadLine()
        while s <> "" do
            //printfn "USER: %s" s
            proc.StandardInput.WriteLine(s)
            s <- stdin.ReadLine()
    else
        let sw = Stopwatch.StartNew()
        proc.StandardInput.WriteLine("""/summon LavaSlime 0 255 0 {Invulnerable:1,Tags:["AA"]}""")
        //for cx = -32 to 32 do
        for cx = -5 to 5 do
            for cz = -32 to 32 do
                let x = cx*16+8
                let z = cz*16+8
                proc.StandardInput.WriteLine(sprintf """/spreadplayers %d %d 2 7 false @e[tag=AA]""" x z)
                System.Threading.Thread.Sleep(170)  // TODO tune this, but overall, seems to take a lot longer than my player version
        printfn "finished in %f minutes" sw.Elapsed.TotalMinutes 
    proc.StandardInput.WriteLine("""/kill @e[tag=AA]""")
    System.Threading.Thread.Sleep(200)
    proc.StandardInput.WriteLine("""/stop""")
    System.Threading.Thread.Sleep(2000)
    proc.Close()
    printfn "press enter to quit"
    stdin.ReadLine() |> ignore

////////////////////////////////////////////

[<AllowNullLiteral>]
type TrieNode(parent,finalLetter) =
    let data = Array.zeroCreate 26
    let mutable isWord = false
    let mutable x,y,z = 0,0,0
    member this.Add(letter) =
        if data.[letter] = null then
            data.[letter] <- new TrieNode(this,letter)
        data.[letter]
    member this.FinishWord() = isWord <- true
    member this.Data = data
    member this.IsWord = isWord
    member this.SetXYZ(xx,yy,zz) = x <- xx; y <- yy; z <- zz
    member this.X = x
    member this.Y = y
    member this.Z = z
    member this.Parent = parent
    member this.FinalLetter = finalLetter

let makeTrie() =
    let words = System.IO.File.ReadAllLines("""C:\Users\Admin1\Documents\GitHubVisualStudio\minecraft-map-manipulator\MinecraftMapManipulator\ConsoleApplication1\ENABLE.txt""")
    let root = new TrieNode(null,-1)
    let mutable count, letters = 0,0
    for w in words do
        let mutable i = root
        for c in w do
            i <- i.Add(int(c) - int('a'))
        i.FinishWord()
        count <- count + 1
        letters <- letters + w.Length 
    printfn "made %d words, %d letters" count letters
    root

let mutable words = 0
let mutable nonWords = 0
let mutable cmdBlocks = 0
let mutable firstTime = true
let mutable nodesVisited = 0

type Placer() =
    let mutable x,y,z = 0,255,0
    member this.Place(n) =
        if z+n > 170 then
            z <- 0
            x <- x + 1
            if x > 170 then
                x <- 0
                y <- y - 3
                if y < 10 then
                    failwith "out of room"
        let r = x,y,z
        z <- z + n
        r

let rec postfix(n:TrieNode,parent:TrieNode,placer:Placer,r:RegionFile) =
    nodesVisited <- nodesVisited + 1
    if nodesVisited % 10000 = 0 then
        printfn "    visited %d" nodesVisited
    let mutable numFwd = 0
    for x in n.Data do
        if x <> null then
            postfix(x,n,placer,r)
            numFwd <- numFwd + 1
    if n.IsWord then
        words <- words + 1
    else
        nonWords <- nonWords + 1
    let thisCmdBlocks = 1 + numFwd + 1
    cmdBlocks <- cmdBlocks + thisCmdBlocks
    let x,y,z =     
        if firstTime then
            let x,y,z = placer.Place(thisCmdBlocks+1)  // +1 for air in between, PlaceCommandBlocksStartingAt puts air after
            n.SetXYZ(x,y,z)
            x,y,z
        else
            n.X, n.Y, n.Z
    if firstTime then
        // could place commands, record self location
        // second pass to record parent locations for backspace
        // O fill blah wool n replace wool  // n = green or red if word
        // U tp @e[type=LavaSlime,score_L_min=0,score_L=0] x y z // xyz for 'A' (0)
        // U tp @e[type=LavaSlime,score_L_min=1,score_L=1] x y z // xyz for 'B' (1)
        // ...
        // U tp @e[type=LavaSlime,score_L_min=26,score_L=26] x y z // xyz for 'backspace' (26)
        let cmds = 
            [|
                yield O (sprintf "fill 49 0 0 49 3 10 wool %d replace wool" (if n.IsWord then 5 else 14))
                for i = 0 to 25 do
                    let next = n.Data.[i]
                    if next <> null then
                        assert(next.Y <> 0)
                        yield U (sprintf "tp @e[type=LavaSlime,score_L_min=%d,score_L=%d] %d %d %d" i i next.X next.Y next.Z)
                yield U "say never get here"  // replace in pass 2
            |]
        ()//r.PlaceCommandBlocksStartingAt(x,y,z,cmds,"",false)
    else
        ()//r.PlaceCommandBlocksStartingAt(x,y,z+thisCmdBlocks-1,[|U (sprintf "tp @e[type=LavaSlime,score_L_min=26,score_L=26] %d %d %d" parent.X parent.Y parent.Z)|],"",false)

let rec findTrie(n:TrieNode) =
    let mutable numFwd = 0
    for x in n.Data do
        if x <> null then
            numFwd <- numFwd + 1
            findTrie(x)
    if numFwd < 5 && n.Data.[0] <> null && n.Data.[1] <> null then
        let mutable c,a = n,[]
        while c <> null do
            a <- c.FinalLetter :: a
            c <- c.Parent 
        let s = new string(a |> List.map (fun i -> i+65 |> char) |> List.toArray)
        printfn "%s" s

let doTrie(r:RegionFile) =
    let t = makeTrie()
    findTrie(t)
    let placer = new Placer()
    postfix(t,null,placer,r)
    printfn "%d word nodes and %d nonword nodes, need %d commands" words nonWords cmdBlocks
    firstTime <- false
    nodesVisited <- 0
    postfix(t,t,placer,r)
    // glowing entity can be the 'PC', can use all Os and have it BD them
    for x = 0 to 26 do
        let cmds = 
            [|
                yield O "blockdata ~ ~ ~ {auto:0b}"
                //yield U (sprintf """tellraw @a [{"text":"you pressed %c"}]""" (char (65+x)))
                yield U (sprintf "scoreboard players set @e[type=LavaSlime] L %d" x)
                yield U "execute @e[type=LavaSlime] ~ ~ ~ blockdata ~ ~ ~ {auto:1b}"  // run cur to jump to next
                yield U "blockdata ~ ~ ~2 {auto:1b}"
                yield U "blockdata ~ ~ ~1 {auto:0b}"
                yield O "scoreboard players set @e[type=LavaSlime] L -1"
                yield U "execute @e[type=LavaSlime] ~ ~ ~ blockdata ~ ~ ~ {auto:1b}"  // run next's fill, also see if looped
                if x = 26 then
                    yield C "tp @e[type=ArmorStand] ~ ~ ~-1"
                    yield C "execute @e[type=ArmorStand] ~ ~ ~ setblock 48 2 ~ air"
                else
                    yield C (sprintf "execute @e[type=ArmorStand] ~ ~ ~ clone 68 2 %d 68 2 %d 48 2 ~" x x)
                    yield C "tp @e[type=ArmorStand] ~ ~ ~1"
                yield U "execute @e[type=Slime] ~ ~ ~ blockdata ~ ~ ~ {auto:0b}"  // slime stayed at old place
                yield U "tp @e[type=Slime] @e[type=LavaSlime]"
                yield U "execute @e[type=Slime] ~ ~ ~ blockdata ~ ~ ~ {auto:0b}"  // slime at new place
            |]
        r.PlaceCommandBlocksStartingAt(x,0,0,cmds,"on key",false,true)
    let keys = 
        [|
            for i = 0 to 25 do
                yield sprintf """{"text":"[%c] ","clickEvent":{"action":"run_command","value":"/blockdata %d 0 0 {auto:1b}"}}""" (char (65+i)) i
            yield """{"text":"[BKSP]","clickEvent":{"action":"run_command","value":"/blockdata 26 0 0 {auto:1b}"}}"""
        |]
    let initCmds =
        [|
            O ""
            U "scoreboard objectives add L dummy"
            U "kill @e[type=!Player]"
            U "effect @p 16 9999 1 true"
            U "gamerule commandBlockOutput false"
            U "gamerule sendCommandFeedback false"
            U "clear @a"
            (*
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ts,Color:0},{Pattern:mr,Color:15},{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:ms,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:0,Patterns:[{Pattern:mr,Color:15},{Pattern:ls,Color:0},{Pattern:ms,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:0,Patterns:[{Pattern:mr,Color:15},{Pattern:ms,Color:15},{Pattern:ls,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:rs,Color:0},{Pattern:ts,Color:0},{Pattern:bs,Color:0},{Pattern:cbo,Color:15},{Pattern:ls,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ms,Color:0},{Pattern:hhb,Color:15},{Pattern:rs,Color:15},{Pattern:ls,Color:0},{Pattern:ts,Color:0},{Pattern:bs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ms,Color:0},{Pattern:hhb,Color:15},{Pattern:rs,Color:15},{Pattern:ls,Color:0},{Pattern:ts,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ms,Color:0},{Pattern:vh,Color:15},{Pattern:rs,Color:0},{Pattern:hh,Color:15},{Pattern:bs,Color:0},{Pattern:ls,Color:0},{Pattern:ts,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:ms,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ts,Color:0},{Pattern:bs,Color:0},{Pattern:cs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:bs,Color:0},{Pattern:mr,Color:15},{Pattern:rs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:drs,Color:0},{Pattern:vh,Color:15},{Pattern:hh,Color:15},{Pattern:dls,Color:0},{Pattern:ls,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:vh,Color:0},{Pattern:cs,Color:15},{Pattern:bs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:tt,Color:0},{Pattern:tts,Color:15},{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ls,Color:0},{Pattern:rud,Color:15},{Pattern:drs,Color:0},{Pattern:rs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ts,Color:0},{Pattern:bs,Color:0},{Pattern:mr,Color:15},{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:rs,Color:0},{Pattern:hhb,Color:0},{Pattern:bs,Color:15},{Pattern:ts,Color:0},{Pattern:ls,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ts,Color:0},{Pattern:bs,Color:0},{Pattern:mr,Color:15},{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:br,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag: {Base:15,Patterns:[{Pattern:br,Color:0},{Pattern:rud,Color:15},{Pattern:ms,Color:0},{Pattern:hh,Color:0},{Pattern:cs,Color:15},{Pattern:drs,Color:0},{Pattern:tt,Color:15},{Pattern:ls,Color:0},{Pattern:ms,Color:0},{Pattern:ts,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ts,Color:0},{Pattern:bs,Color:0},{Pattern:mr,Color:15},{Pattern:drs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ts,Color:0},{Pattern:cs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:bs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:bs,Color:0},{Pattern:mr,Color:15},{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:tt,Color:15},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:bt,Color:0},{Pattern:bts,Color:15},{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:dls,Color:0},{Pattern:drs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:drs,Color:0},{Pattern:vhr,Color:15},{Pattern:dls,Color:0},{Pattern:cbo,Color:15},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ts,Color:0},{Pattern:bs,Color:0},{Pattern:dls,Color:0},{Pattern:bo,Color:15}]}}"
            *)
            U (sprintf "summon LavaSlime %d %d %d {Glowing:1,Invulnerable:1,Silent:1,NoAI:1,Size:1,Invisible:1}" t.X t.Y t.Z)
            U (sprintf "summon Slime %d %d %d {Glowing:1,Invulnerable:1,Silent:1,NoAI:1,Size:1,Invisible:1}" t.X t.Y t.Z)
            U "summon ArmorStand 50 2 0 {Invisible:1,Marker:1,NoGravity:1}"
            U "fill 49 0 0 49 3 10 wool 0"
            U "fill 69 0 0 69 3 30 wool 0"
            U (sprintf "tellraw @a [%s]" (System.String.Join(",",keys)))
        |]
    r.PlaceCommandBlocksStartingAt(30,0,0,initCmds,"init",false,true)
////////////////////////////////////////////

let musicStuff() =
    let MUSIC_DIR = """C:\Users\Admin1\Desktop\Music\"""
    let FFMPEG_DIR = """C:\Users\Admin1\Desktop\ffmpeg-20160105-git-68eb208-win64-static\bin\"""
    let OUT_DIR = """C:\Users\Admin1\AppData\Roaming\.minecraft\resourcepacks\BrianResourcePack\assets\minecraft\sounds\"""
    let DEFAULT_FILTER = """-filter_complex afade=t=in:st=0:d=0.02:c=tri,afade=t=out:st=4.98:d=0.02:c=tri"""
    let FADEOUT_FILTER = """-filter_complex afade=t=in:st=0:d=0.02:c=tri,afade=t=out:st=0:d=5:c=tri"""
    let FADEIN_FILTER  = """-filter_complex afade=t=in:st=0:d=5:c=tri,afade=t=out:st=4.98:d=0.02:c=tri"""
    let SECONDS_PER_TRACK = 5
    let SEGMENTS = 60
    let MAX_TICK = SEGMENTS * SECONDS_PER_TRACK * 20 // 20 = ticks per second
    let TRACKS = [|"cat"; "else"; "far"|]
    let breakUpOggFiles = false
    if breakUpOggFiles then
        for track in TRACKS do
            for i = 0 to SEGMENTS-1 do
                let startTime = i * SECONDS_PER_TRACK
                let run(args) = 
                    let psi = ProcessStartInfo(UseShellExecute=true, RedirectStandardInput=false, RedirectStandardOutput=false)
                    psi.WorkingDirectory <- FFMPEG_DIR
                    psi.FileName <- "ffmpeg.exe"
                    psi.Arguments <- args
                    let proc = new Process(StartInfo=psi)
                    proc.Start() |> ignore
                let file = sprintf "%s%02d" track i
                let args = sprintf "-y -ss %d -i %s%s.ogg -t %d %s %s%s.ogg" startTime MUSIC_DIR track SECONDS_PER_TRACK DEFAULT_FILTER OUT_DIR file
                run(args)
                printfn """  "brian.%s": { "category": "record", "sounds": [ {"name":"%s","stream":true} ] },""" file file
                let file = sprintf "%s%02dfadeout" track i
                let args = sprintf "-y -ss %d -i %s%s.ogg -t %d %s %s%s.ogg" startTime MUSIC_DIR track SECONDS_PER_TRACK FADEOUT_FILTER OUT_DIR file
                run(args)
                printfn """  "brian.%s": { "category": "record", "sounds": [ {"name":"%s","stream":true} ] },""" file file
                let file = sprintf "%s%02dfadein" track i
                let args = sprintf "-y -ss %d -i %s%s.ogg -t %d %s %s%s.ogg" startTime MUSIC_DIR track SECONDS_PER_TRACK FADEIN_FILTER OUT_DIR file
                run(args)
                printfn """  "brian.%s": { "category": "record", "sounds": [ {"name":"%s","stream":true} ] },""" file file
        printfn ""
        printfn "Now delete all the 'extra' files from the output directory, since ffmpeg can leave bad bits when starting past EOF (anything less than 10KB file size)"
    let fil = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\MusicTestRR\region\r.0.0.mca"""
    let r = new RegionFile(fil)
    let cmds = 
        [|
            O "scoreboard objectives add Prev dummy"
            U "scoreboard objectives add Curr dummy"
            U "scoreboard objectives add Tick dummy"
            U "gamerule commandBlockOutput false"
            U "gamerule doDaylightCycle false"
            U "time set 500"
            U "scoreboard players set Tick Tick -1"
            U "scoreboard players set N Tick 100"
            U "scoreboard players set @a Prev 0"
            U "fill 10 55 10 19 55 19 stone"
            U "fill 10 55 20 19 55 29 lapis_block"
        |]
    r.PlaceCommandBlocksStartingAtSelfDestruct(1,60,1,cmds,"")
    let cmds = 
        [|
            yield O "fill 20 60 1 100 60 100 air"
            yield U "fill ~ ~ ~-1 ~ ~ ~100 air"
            yield P "scoreboard players add Tick Tick 1"
            yield U "scoreboard players operation Test Tick = Tick Tick"
            yield U "scoreboard players operation Test Tick %= N Tick"
            yield U (sprintf "scoreboard players test Tick Tick %d" (MAX_TICK+5))  // supposed to be silent at end of loop anyway, just ensure we get all segments even after the delay to run code below
            yield C "scoreboard players set Tick Tick -1"
            yield U "scoreboard players test Test Tick * 0"
            yield C "blockdata ~ ~ ~2 {auto:1b}"
            yield C "blockdata ~ ~ ~1 {auto:0b}"
            yield O "scoreboard players operation @a Tick = Tick Tick"
            yield U "scoreboard players operation @a Tick /= N Tick"  // @p Tick is which segment
            yield U """tellraw @a ["segment #",{"score":{"name":"@p","objective":"Tick"}}]"""
            yield U "execute @a ~ ~ ~ scoreboard players operation @p[r=0,c=1] Prev = @p[r=0,c=1] Curr"
            yield U "scoreboard players set @a Curr 0"  // default
            yield U "scoreboard players set @a[x=10,y=1,z=10,dx=10,dy=200,dz=10] Curr 1"  // region score
            yield U "scoreboard players set @a[x=10,y=1,z=20,dx=10,dy=200,dz=10] Curr 2"  // region score
            for k = 0 to SEGMENTS-1 do
                yield U (sprintf "execute @a[score_Tick=%d,score_Tick_min=%d] ~ ~ ~ blockdata %d 60 1 {auto:1b}" k k (20+k))
                //yield U (sprintf "execute @a[score_Tick=%d,score_Tick_min=%d] ~ ~ ~ blockdata %d 60 1 {auto:0b}" k k (20+k))
        |]
    r.PlaceCommandBlocksStartingAt(2,60,1,cmds,"")
    for k = 0 to SEGMENTS-1 do
        let cmds = 
            [|
                yield O "blockdata ~ ~ ~ {auto:0b}"
                for i = 0 to TRACKS.Length-1 do
                    for j = 0 to TRACKS.Length-1 do
                        if i=j then
                            yield U (sprintf "execute @a[score_Prev=%d,score_Prev_min=%d,score_Curr=%d,score_Curr_min=%d] ~ ~ ~ playsound %s @p[r=0,c=1] ~ 255 ~ 64 1 0" i i j j (sprintf "brian.%s%02d" TRACKS.[i] k))
                        else
                            yield U (sprintf "execute @a[score_Prev=%d,score_Prev_min=%d,score_Curr=%d,score_Curr_min=%d] ~ ~ ~ playsound %s @p[r=0,c=1] ~ 255 ~ 64 1 0" i i j j (sprintf "brian.%s%02dfadeout" TRACKS.[i] k))
                            yield U (sprintf "execute @a[score_Prev=%d,score_Prev_min=%d,score_Curr=%d,score_Curr_min=%d] ~ ~ ~ playsound %s @p[r=0,c=1] ~ 255 ~ 64 1 0" i i j j (sprintf "brian.%s%02dfadein" TRACKS.[j] k))
            |]
        r.PlaceCommandBlocksStartingAt(20+k,60,1,cmds,"")
    // TODO have each track have its own repeat length, have Tick mode segment # based on biome track?
    // TODO use the 'play 46 null sounds at once' strategy to end all sound, and then have continual tracks that fade in at each 5s start point, and only stop at transitions
    // TODO can make computation more efficient using a armor stand to track segment number with a tp each 5s segment

    r.Write(fil+".new")
    System.IO.File.Delete(fil)
    System.IO.File.Move(fil+".new",fil)


///////////////////////////////////////////////////////
type ImageWPFWindow(img) as this =  
    inherit System.Windows.Window()    
    do 
        this.SizeToContent <- System.Windows.SizeToContent.WidthAndHeight 
        this.Content <- img

let plotRegionalDifficulty() =
    let difficulty = 2 // normal
    let moonPhase = 1.0 // full
    let N = 100
    let image = new System.Drawing.Bitmap(N+1,N+1)
    for totalPlayTimeN = 0 to N do
        let totalPlayTimePct = float totalPlayTimeN / float N
        let totalPlayTimeHours = 21.0 * totalPlayTimePct 
        for chunkInhabitedTimeN = 0 to N do
            let chunkInhabitedTimePct = float chunkInhabitedTimeN / float N
            let chunhInhabitedTimeHours = 50.0 * chunkInhabitedTimePct 

            // wiki formula
            let TotalTimeFactor = (min 20.0 (totalPlayTimeHours - 1.0)) / 80.0        // range: 0.0 - 0.25
            let mutable ChunkFactor = (min 1.0 (chunhInhabitedTimeHours / 50.0))      // init range: 0.0 - 1.0
            if difficulty <> 3 then
                ChunkFactor <- ChunkFactor * 0.75
            ChunkFactor <- ChunkFactor + (min (moonPhase/4.0) TotalTimeFactor)
            if difficulty = 1 then
                ChunkFactor <- ChunkFactor / 2.0
            let mutable RegionalDifficulty = 0.75 + TotalTimeFactor + ChunkFactor
            if difficulty = 2 then
                RegionalDifficulty <- RegionalDifficulty * 2.0
            if difficulty = 3 then
                RegionalDifficulty <- RegionalDifficulty * 3.0
            // wiki says The regional difficulty ranges from 0.75–1.5 on easy, 1.5–4.0 on normal, and 2.25–6.75 on hard.

            let gameUsedValue = min 1.0 (max 0.0 ((RegionalDifficulty - 2.0) / 2.0))
            if totalPlayTimeN = 50 then
                printf "%1.2f " gameUsedValue
            let r,g,b = 
                if gameUsedValue = 0.0 then
                    255, 0, 0
                elif gameUsedValue = 1.0 then
                    0, 150, 0
                else
                    int(255.0 * gameUsedValue), int(255.0 * gameUsedValue), int(255.0 * gameUsedValue)
            image.SetPixel(chunkInhabitedTimeN, totalPlayTimeN, System.Drawing.Color.FromArgb(r,g,b))
        if totalPlayTimeN = 50 then
            printfn ""
    let img = PhotoToMinecraft.bmpToImage(image,8.0)
    let app =  new System.Windows.Application()  
    app.Run(new ImageWPFWindow(img)) |> ignore 
    // neat, this demos that if TotalPlayTime is 11 hours, at normal diff, full moon, ChunkInhabitedTime knob changes game value smoothly from 0.0 to 0.75, which is very useful.

////////////////////////////////////////

type InputEvent = 
    | CONSOLE of string     // stuff typed into the keyboard console of this program 
    | MINECRAFT of string   // the stdout of the Minecraft process 

let chatToVoiceDemo() =
    use inputEvents = new System.Collections.Concurrent.BlockingCollection<_>()
    // SETUP MINECRAFT 
    let minecraftStdin = 
        let psi = new ProcessStartInfo(UseShellExecute=false, RedirectStandardInput=true, RedirectStandardOutput=true) 
        psi.WorkingDirectory <- """C:\Users\Admin1\Desktop\Server""" 
        psi.FileName <- "java" 
        psi.Arguments <- "-Xms1024M -Xmx1024M -d64 -jar minecraft_server.16w02a.jar nogui" 
        let proc = new Process(StartInfo=psi) 
        // START MINECRAFT 
        do
            proc.Start() |> ignore 
            let rec rcvloop() = 
                let data = proc.StandardOutput.ReadLine() 
                if data <> null then 
                    inputEvents.Add(MINECRAFT data) 
                    rcvloop() 
            let t = new System.Threading.Thread(rcvloop) 
            t.Start() 
        proc.StandardInput 
    // SETUP & START CONSOLE 
    do 
        printfn "press q <enter> to quit" 
        let rec sendloop() = 
            let i = System.Console.ReadLine() 
            if i = "go" then
                for i = 1 to 9 do
                    minecraftStdin.WriteLine("execute @p ~ ~ ~ tp @e[type=Villager] ~ ~ ~ ~10 ~")
                    minecraftStdin.WriteLine("execute @p ~ ~ ~ tp @e[type=Villager] ~ ~ ~0.11 ~ ~")
                    minecraftStdin.Flush()
                    System.Threading.Thread.Sleep(100)
            if i <> "q" then 
                inputEvents.Add(CONSOLE i) 
                sendloop() 
            else 
                inputEvents.CompleteAdding() 
        let t = new System.Threading.Thread(sendloop) 
        t.Start() 
    use ss = new System.Speech.Synthesis.SpeechSynthesizer()
    // MAIN LOOP 
    for e in inputEvents.GetConsumingEnumerable() do 
        match e with 
        | MINECRAFT data -> 
                try 
                    printfn "MINECRAFT> %s" data 
                    match data.IndexOf("Lorgon111") with 
                    | -1 -> () 
                    | n ->  
                    let data = data.Substring(n+"Lorgon111".Length) 
                    let PROMPT = "> !"
                    let PROMPT = "> "
                    match data.LastIndexOf(PROMPT) with      // may be color reset code between name and text, match separately 
                    | -1 -> () 
                    | n ->  
                        let text = data.Substring(n+PROMPT.Length).ToLowerInvariant()
                        let words = text.Split([|" "|], System.StringSplitOptions.RemoveEmptyEntries) 
                        for w in words do
                            printfn "M: %s" w
                        ss.Speak(text)
                with e ->  
                    printfn "MINECRAFT FAULT> %s" (e.ToString()) 
                    reraise() 
        | CONSOLE data -> 
            printfn "C: %s" data

////////////////////////////////////////

// BUG LIST
// xTODO bed registered twice, two points? (26 and 355, both named "bed", only 355 should be there)
// xTODO (white) banner did not register (the issue is Damage)
// xTODO (damaged) bow (skel drop) did not register (the issue is Damage)
// xTODO deadbush as 31 and 32, are both obtainable? (31 is not)
// xTODO dragon head obtainable? 397,5
// xTODO should chat the display name rather than item name, should add display names currently only in comments
// obe can beta test

// xTODO between the gold carrot and the carrot on stick, there should be 5 skulls, but they are not displaying properly (I think fixed by moving itemName before name)

// xTODO /gamerule logAdminCommands false
// xTODO nicer dye names (351)

// TODO got red mush, said in chat, but no sound nor count++
// TODO skylinerw suggests /clear 0 may be cheaper than /testfor

// could be nice to print name of player who got the item (but needs more cmds/lag, yes?)

open MC_Constants
let makeGetAllItemsGame(map:MapFolder, minxRoom, minyRoom, minzRoom, minxCmds, minyCmds, minzCmds) =    
    // absolute to relative coords
    let AR goal curr = goal-curr
    // discover when damage value matters
    let hasNonZeroDamage = Array.zeroCreate 3000
    for bid,dmg,_ in survivalObtainableItems do
        if dmg <> 0 then hasNonZeroDamage.[bid] <- true
    // main loop
    let tes = ResizeArray()
    let YMIN = minyRoom
    let L = (survivalObtainableItems.Length+4)/5
    let Q = (L+3)/4
    let mutable count = 0
    for oz = 1 to 4*Q do
        for dy = 4 downto 0 do
            let y = YMIN+dy
            let i = (YMIN+4-y)*L + oz - 1
            let x,z,dx,dz,facing =
                if oz <= Q then
                    1,Q+2-oz,1,0,3
                elif oz <= 2*Q then
                    oz-Q+3,1,0,-1,0
                elif oz <= 3*Q then
                    Q+6,2+oz-2*Q,-1,0,1
                else
                    4*Q-oz+4,Q+2,0,1,2
            let x,y,z = x-2+minxRoom, y, z+minzRoom
            // x : [-1,-1]    z : [Q+1,2]
            // x : [2,Q+1]    z : [1,1]
            // x : [Q+4,Q+4]  z : [3,Q+2]
            // x : [Q+1,2]    z : [Q+2,Q+2]
            map.EnsureSetBlockIDAndDamage(x+dx,y,z+dz,1uy,0uy) // 1,0 = stone
            count <- count + 1
            let cx, cy, cz = minxCmds, minyCmds+dy, minzCmds+oz
            map.EnsureSetBlockIDAndDamage(cx,cy,cz,211uy,3uy)
            if i < survivalObtainableItems.Length then
                let bid,dmg,name = survivalObtainableItems.[i]
                let itemName = if bid <= 255 then blockIdToMinecraftName |> Array.find (fun (x,_y) -> x=bid) |> snd else sprintf "minecraft:%s" name
                // minor name fixup stuff
                let name = System.Text.RegularExpressions.Regex.Replace(name, """\s+""", " ")  // condense multiple spaces to one space
                let name = match bid,dmg with
                           | 263,1 -> "charcoal"
                           | 322,1 -> "enchanted golden apple"
                           | 349,1 -> "salmon"
                           | 349,2 -> "clownfish"
                           | 349,3 -> "pufferfish"
                           | 350,1 -> "cooked salmon"
                           | 351,15 -> "bone meal"
                           | 351,14 -> "orange dye"
                           | 351,13 -> "magenta dye"
                           | 351,12 -> "light blue dye"
                           | 351,11 -> "dandelion yellow"
                           | 351,10 -> "lime dye"
                           | 351,9 -> "pink dye"
                           | 351,8 -> "dark gray dye"
                           | 351,7 -> "light gray dye"
                           | 351,6 -> "cyan dye"
                           | 351,5 -> "purple dye"
                           | 351,4 -> "lapis lazuli"
                           | 351,3 -> "cocoa bean"
                           | 351,2 -> "cactus green"
                           | 351,1 -> "rose red"
                           | 351,0 -> "ink sac"
                           | 397,0 -> "skeleton head"
                           | 397,1 -> "wither skeleton head"
                           | 397,2 -> "zombie head"
                           | 397,4 -> "creeper head"
                           | 397,5 -> "dragon head"
                           | _ -> name
                let cmd = sprintf """summon ItemFrame ~%d ~%d ~%d {Facing:%db,Item:{id:"%s",Count:1b,Damage:%ds,tag:{display:{Name:"%s"}}}}""" (AR (x+2*dx) cx) (AR y cy) (AR (z+0*dz) cz) facing itemName dmg name
                tes.Add [|Int("x",cx); Int("y",cy); Int("z",cz); String("id","minecraft:command_block"); 
                            Byte("auto",1uy); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); 
                            String("Command",cmd); End |]
                // backing commands
                let cmdFacing = [| 2uy; 5uy; 3uy; 4uy |].[facing]  // convert item-frame-facing to opposite command-block-facing
                let cx,cy,cz = x+0*dx, y, z+2*dz
                map.EnsureSetBlockIDAndDamage(cx,cy,cz,210uy,cmdFacing) // 210=repeating
                tes.Add [|Int("x",cx); Int("y",cy); Int("z",cz); String("id","minecraft:command_block"); 
                            Byte("auto",0uy); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); 
                            String("Command",sprintf """testfor @a {Inventory:[{id:"%s"%s}]}""" itemName (if hasNonZeroDamage.[bid] then sprintf ",Damage:%ds" dmg else "")); End |]
                // setblock emerald
                let cx,cy,cz = x+ -1*dx, y, z+3*dz
                map.EnsureSetBlockIDAndDamage(cx,cy,cz,211uy,cmdFacing+8uy) // 211=chain (conditional)
                tes.Add [|Int("x",cx); Int("y",cy); Int("z",cz); String("id","minecraft:command_block"); 
                            Byte("auto",1uy); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); 
                            String("Command",sprintf """setblock ~%d ~ ~%d emerald_block""" (2*dx) (-2*dz)); End |]
                // chat announce name
                let cx,cy,cz = x+ -2*dx, y, z+4*dz
                map.EnsureSetBlockIDAndDamage(cx,cy,cz,211uy,cmdFacing+8uy) // 211=chain (conditional)
                tes.Add [|Int("x",cx); Int("y",cy); Int("z",cz); String("id","minecraft:command_block"); 
                            Byte("auto",1uy); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); 
                            String("Command",sprintf """tellraw @a ["Got %s"]""" name); End |]
                // score++
                let cx,cy,cz = x+ -3*dx, y, z+5*dz
                map.EnsureSetBlockIDAndDamage(cx,cy,cz,211uy,cmdFacing+8uy) // 211=chain (conditional)
                tes.Add [|Int("x",cx); Int("y",cy); Int("z",cz); String("id","minecraft:command_block"); 
                            Byte("auto",1uy); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); 
                            String("Command","""scoreboard players add Have Items 1"""); End |]
                // sound
                let cx,cy,cz = x+ -4*dx, y, z+6*dz
                map.EnsureSetBlockIDAndDamage(cx,cy,cz,211uy,cmdFacing+8uy) // 211=chain (conditional)
                tes.Add [|Int("x",cx); Int("y",cy); Int("z",cz); String("id","minecraft:command_block"); 
                            Byte("auto",1uy); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); 
                            String("Command","""execute @a ~ ~ ~ playsound entity.firework.launch voice @p ~ ~ ~"""); End |]
            else 
                // empty command, just to ensure overwriting blocks
                tes.Add [|Int("x",cx); Int("y",cy); Int("z",cz); String("id","minecraft:command_block"); 
                            Byte("auto",1uy); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); 
                            String("Command",""); End |]
                // TODO filled_map looks weird
    // ICBs to init the item frames at each Y
    for dy = 4 downto 0 do
        let x = minxCmds
        let z = minzCmds
        let y = minyCmds + dy
        map.SetBlockIDAndDamage(x,y,z,137uy,3uy)
        let cmd = ""
        tes.Add [|Int("x",x); Int("y",y); Int("z",z); String("id","minecraft:command_block"); 
                    Byte("auto",0uy); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); 
                    String("Command",cmd); End |]
    let cx,cy,cz = minxRoom+10, minyRoom, minzRoom+5
    let r = map.GetRegion(cx,cz)
    r.PlaceCommandBlocksStartingAt(cx,cy,cz,
        [|
        // put redstone to start the checkers
        O(sprintf "fill ~%d ~%d ~%d ~%d ~%d ~%d redstone_block" (AR minxRoom cx)       (AR minyRoom cy) (AR (minzRoom+Q+1) (cz+0)) (AR minxRoom cx)       (AR (minyRoom+4) cy) (AR (minzRoom+2)   (cz+0)))
        U(sprintf "fill ~%d ~%d ~%d ~%d ~%d ~%d redstone_block" (AR (minxRoom+2) cx)   (AR minyRoom cy) (AR (minzRoom)     (cz+1)) (AR (minxRoom+Q+1) cx) (AR (minyRoom+4) cy) (AR (minzRoom)     (cz+1)))
        U(sprintf "fill ~%d ~%d ~%d ~%d ~%d ~%d redstone_block" (AR (minxRoom+Q+3) cx) (AR minyRoom cy) (AR (minzRoom+3)   (cz+2)) (AR (minxRoom+Q+3) cx) (AR (minyRoom+4) cy) (AR (minzRoom+Q+2) (cz+2)))
        U(sprintf "fill ~%d ~%d ~%d ~%d ~%d ~%d redstone_block" (AR (minxRoom+Q+1) cx) (AR minyRoom cy) (AR (minzRoom+Q+3) (cz+3)) (AR (minxRoom+2) cx)   (AR (minyRoom+4) cy) (AR (minzRoom+Q+3) (cz+3)))
        // put barriers to enclose room
        U(sprintf "fill ~%d ~%d ~%d ~%d ~%d ~%d barrier" (AR (minxRoom+2)   cx) (AR minyRoom cy) (AR (minzRoom+Q+1) (cz+4)) (AR (minxRoom+2)   cx) (AR (minyRoom+4) cy) (AR (minzRoom+2)   (cz+4)))
        U(sprintf "fill ~%d ~%d ~%d ~%d ~%d ~%d barrier" (AR (minxRoom+2)   cx) (AR minyRoom cy) (AR (minzRoom+2)   (cz+5)) (AR (minxRoom+Q+1) cx) (AR (minyRoom+4) cy) (AR (minzRoom+2)   (cz+5)))
        U(sprintf "fill ~%d ~%d ~%d ~%d ~%d ~%d barrier" (AR (minxRoom+Q+1) cx) (AR minyRoom cy) (AR (minzRoom+3)   (cz+6)) (AR (minxRoom+Q+1) cx) (AR (minyRoom+4) cy) (AR (minzRoom+Q+1) (cz+6)))
        U(sprintf "fill ~%d ~%d ~%d ~%d ~%d ~%d barrier" (AR (minxRoom+Q+1) cx) (AR minyRoom cy) (AR (minzRoom+Q+1) (cz+7)) (AR (minxRoom+2)   cx) (AR (minyRoom+4) cy) (AR (minzRoom+Q+1) (cz+7)))
        // activate item frames
        U(sprintf "fill ~%d ~%d ~%d ~%d ~%d ~%d redstone_block" (AR minxCmds cx) (AR minyCmds cy) (AR (minzCmds-1) (cz+8)) (AR minxCmds cx) (AR (minyCmds+4) cy) (AR (minzCmds-1) (cz+8)))
        // set world spawn here
        U("setworldspawn ~ ~ ~")
        // set up scoreboard
        U("scoreboard objectives add Items dummy")
        U(sprintf "scoreboard players set Goal Items %d" survivalObtainableItems.Length)
        U("scoreboard players set Have Items 0")
        U("scoreboard objectives setdisplay sidebar Items")
        U("gamerule logAdminCommands false")
        U("gamerule commandBlockOutput false")
        U("gamerule disableElytraMovementCheck true")
        U("blockdata ~ ~ ~1 {auto:1b}")
        // erase item frame makers
        O(sprintf "fill ~%d ~%d ~%d ~%d ~%d ~%d air" (AR minxCmds cx) (AR minyCmds cy) (AR (minzCmds-1) (cz+18)) (AR minxCmds cx) (AR (minyCmds+4) cy) (AR (minzCmds+4*Q+2) (cz+18)))
        U("fill ~ ~ ~ ~ ~ ~-19 air")
        |],"",false,false)
    // write it all out
    map.AddOrReplaceTileEntities(tes)
    for x = minxRoom-5 to minxRoom+Q+8  do
        for z = minzRoom-5 to minzRoom+Q+8 do
            map.EnsureSetBlockIDAndDamage(x,minyRoom-1,z,20uy,0uy) // glass floor
            map.EnsureSetBlockIDAndDamage(x,minyRoom+5,z,20uy,0uy) // glass ceiling
    let x = minxRoom+(Q+5)/2
    let z = minzRoom+(Q+4)/2
    let mutable y = minyRoom-1
    map.SetBlockIDAndDamage(x,y,z,65uy,2uy) // 65=ladder
    map.SetBlockIDAndDamage(x,y,z+1,1uy,0uy) // 1=stone
    map.SetBlockIDAndDamage(x,y+1,z+1,50uy,5uy) // 50=torch
    y <- y - 1
    while map.GetBlockInfo(x,y,z).BlockID = 0uy do
        map.SetBlockIDAndDamage(x,y,z,65uy,2uy) // 65=ladder
        map.SetBlockIDAndDamage(x,y,z+1,1uy,0uy) // 1=stone
        y <- y - 1
    printfn "%d wall spots, %d items" count survivalObtainableItems.Length 

////////////////////////////////////////

let testCompass() =
    let map = new MapFolder("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\Superflat\region\""")
    //let theString = """ x - S - x - W - x - N - x - E -"""
    //let theString = """ --->  --->  ^  <---  <---  -v- """
    let theString = """  ------->    ------->    ^^    <-------    <-------    --vv--  """
    let twice = theString + theString
    //let at(n) = twice.Substring(n, 13)
    let at(n) = twice.Substring(n*2, 26)
    let i = ref 0
    let next() =
        let r = at(!i)
        incr i
        r
    let cmds = 
        [|
            O """fill ~ ~ ~ ~ ~ ~120 air"""
            O """title @p times 0 10 0"""
            P """testfor @p {Inventory:[{Slot:0b,id:"minecraft:compass"}]}"""
            C """blockdata ~ ~ ~2 {auto:1b}"""
            C """blockdata ~ ~ ~1 {auto:0b}"""
            O ""
            U(sprintf """title @p[rym=1,ry=11] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=12,ry=22] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=23,ry=33] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=34,ry=45] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=46,ry=56] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=57,ry=67] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=68,ry=78] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=79,ry=90] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=91,ry=101] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=102,ry=112] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=113,ry=123] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=124,ry=135] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=136,ry=146] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=147,ry=157] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=158,ry=168] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=169,ry=180] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=181,ry=191] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=192,ry=202] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=203,ry=213] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=214,ry=225] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=226,ry=236] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=237,ry=247] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=248,ry=258] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=259,ry=270] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=271,ry=281] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=282,ry=292] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=293,ry=303] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=304,ry=315] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=316,ry=326] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=327,ry=337] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=338,ry=348] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[rym=349,ry=360] subtitle {"text":"%s"}""" (next()))
            U("""title @p title {"text":""}""")
        |]
    let r = map.GetRegion(1,1)
    r.PlaceCommandBlocksStartingAt(1,4,1,cmds,"")
    map.WriteAll()

let testCompass2() =
    let map = new MapFolder("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\Superflat\region\""")
    let theString = """  ------->    ------->    ^^    <-------    <-------    --vv--  """
    let twice = theString + theString
    let at(n) = twice.Substring(n*2, 26)
    let i = ref 0
    let next() =
        let r = at(!i)
        incr i
        r
    let cmds = 
        [|
            O """fill 10 3 10 90 3 90 grass"""
            U """fill ~ ~ ~-1 ~ ~ ~120 air"""
            O """title @p times 0 10 0"""
            U """scoreboard objectives add Rot dummy"""
            U """scoreboard players set #ThreeSixty Rot 360"""
            P """testfor @p {Inventory:[{Slot:0b,id:"minecraft:compass"}]}"""
            C """scoreboard players set @p Rot 456"""  // where the ^^ is (96), +360
            C """execute @p ~ ~-1 ~ blockdata ~ ~ ~ {auto:1b}"""
            C """execute @p ~ ~-1 ~ blockdata ~ ~ ~ {auto:0b}"""
            C """blockdata ~ ~ ~2 {auto:1b}"""
            C """blockdata ~ ~ ~1 {auto:0b}"""
            O ""

            // TODO can binary search this by tp-ing an armor stand to player to get original rotation, and then twisting the armor stand to 'subtract' degrees
            U("""scoreboard players add @p[rym=1,ry=11] Rot 6""")
            U("""scoreboard players add @p[rym=12,ry=22] Rot 17""")
            U("""scoreboard players add @p[rym=23,ry=33] Rot 28""")
            U("""scoreboard players add @p[rym=34,ry=45] Rot 39""")
            U("""scoreboard players add @p[rym=46,ry=56] Rot 51""")
            U("""scoreboard players add @p[rym=57,ry=67] Rot 62""")
            U("""scoreboard players add @p[rym=68,ry=78] Rot 73""")
            U("""scoreboard players add @p[rym=79,ry=90] Rot 84""")
            U("""scoreboard players add @p[rym=91,ry=101] Rot 96""")
            U("""scoreboard players add @p[rym=102,ry=112] Rot 107""")
            U("""scoreboard players add @p[rym=113,ry=123] Rot 118""")
            U("""scoreboard players add @p[rym=124,ry=135] Rot 129""")
            U("""scoreboard players add @p[rym=136,ry=146] Rot 141""")
            U("""scoreboard players add @p[rym=147,ry=157] Rot 152""")
            U("""scoreboard players add @p[rym=158,ry=168] Rot 163""")
            U("""scoreboard players add @p[rym=169,ry=180] Rot 174""")
            U("""scoreboard players add @p[rym=181,ry=191] Rot 186""")
            U("""scoreboard players add @p[rym=192,ry=202] Rot 197""")
            U("""scoreboard players add @p[rym=203,ry=213] Rot 208""")
            U("""scoreboard players add @p[rym=214,ry=225] Rot 219""")
            U("""scoreboard players add @p[rym=226,ry=236] Rot 231""")
            U("""scoreboard players add @p[rym=237,ry=247] Rot 242""")
            U("""scoreboard players add @p[rym=248,ry=258] Rot 253""")
            U("""scoreboard players add @p[rym=259,ry=270] Rot 264""")
            U("""scoreboard players add @p[rym=271,ry=281] Rot 276""")
            U("""scoreboard players add @p[rym=282,ry=292] Rot 287""")
            U("""scoreboard players add @p[rym=293,ry=303] Rot 298""")
            U("""scoreboard players add @p[rym=304,ry=315] Rot 309""")
            U("""scoreboard players add @p[rym=316,ry=326] Rot 321""")
            U("""scoreboard players add @p[rym=327,ry=337] Rot 332""")
            U("""scoreboard players add @p[rym=338,ry=348] Rot 343""")
            U("""scoreboard players add @p[rym=349,ry=360] Rot 354""")

            U("scoreboard players operation @p Rot %= #ThreeSixty Rot")

            // TODO should be 0-359 now, yes?
            // TODO could array-index this with armor stands for instant lookup, a la teleportBasedOnScore in MinecraftBINGO
            U(sprintf """title @p[score_Rot_min=1,score_Rot=11] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=12,score_Rot=22] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=23,score_Rot=33] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=34,score_Rot=45] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=46,score_Rot=56] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=57,score_Rot=67] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=68,score_Rot=78] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=79,score_Rot=90] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=91,score_Rot=101] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=102,score_Rot=112] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=113,score_Rot=123] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=124,score_Rot=135] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=136,score_Rot=146] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=147,score_Rot=157] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=158,score_Rot=168] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=169,score_Rot=180] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=181,score_Rot=191] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=192,score_Rot=202] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=203,score_Rot=213] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=214,score_Rot=225] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=226,score_Rot=236] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=237,score_Rot=247] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=248,score_Rot=258] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=259,score_Rot=270] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=271,score_Rot=281] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=282,score_Rot=292] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=293,score_Rot=303] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=304,score_Rot=315] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=316,score_Rot=326] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=327,score_Rot=337] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=338,score_Rot=348] subtitle {"text":"%s"}""" (next()))
            U(sprintf """title @p[score_Rot_min=349,score_Rot=360] subtitle {"text":"%s"}""" (next()))

            U("""title @p title {"text":""}""")
        |]
    let r = map.GetRegion(1,1)
    r.PlaceCommandBlocksStartingAt(1,4,1,cmds,"")

    let goalX, goalZ = 60, 60
    for x = 10 to 90 do
        let cmds = 
            [|
                for z = 10 to 90 do
                    let dx = goalX-x
                    let dz = goalZ-z
                    let dy,dx = -dx,dz // Minecraft coords are insane
                    let degrees = 180.0 * System.Math.Atan2(float dy, float dx) / System.Math.PI |> int
                    let degrees = degrees + 360 // scoreboard remove only handles positive numbers (?!?)
                    yield O(sprintf "scoreboard players remove @p Rot %d" degrees)
            |]
        r.PlaceCommandBlocksStartingAt(x,3,10,cmds,"")
    map.WriteAll()

let testCompass3() =
    let map = new MapFolder("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\Superflat\region\""")
    let theString = """  ------->    ------->    ^^    <-------    <-------    --vv--  """
    let twice = theString + theString
    let at(n) = twice.Substring(n*2, 26)
    let i = ref 0
    let next() =
        let r = at(!i)
        incr i
        r
    let cmds = 
        [|
            // clean up the world
            yield O """fill 10 3 10 90 3 90 grass"""
            yield U """fill ~ ~ ~-1 ~ ~ ~120 air"""
            // init world
            yield O """scoreboard objectives add Rot dummy"""
            yield U """scoreboard players set #ThreeSixty Rot 360"""
            // always-running loop to test for holding item
            yield P(sprintf """scoreboard players tag @p add Divining {SelectedItem:{tag:{display:{Lore:["%s"]}}}}""" Strings.NameAndLore.DIVINING_ROD_LORE)
            yield U """testfor @p[tag=Divining]"""
            yield C """blockdata ~ ~ ~2 {auto:1b}"""
            yield C """blockdata ~ ~ ~1 {auto:0b}"""
            // if item held... init score and call world-location and player rotation code
            yield O """scoreboard players set @p[tag=Divining] Rot 456"""  // where the ^^ is (96), +360
            yield U """execute @p[tag=Divining] ~ ~-1 ~ blockdata ~ ~ ~ {auto:1b}"""
            yield U """execute @p[tag=Divining] ~ ~-1 ~ blockdata ~ ~ ~ {auto:0b}"""
            yield U """blockdata ~ ~ ~2 {auto:1b}"""
            yield U """blockdata ~ ~ ~1 {auto:0b}"""
            // detect player rotation 
            yield O """title @p[tag=Divining] times 0 10 0""" // is not saved with the world, so has to be re-executed to ensure run after restart client
            yield U("""summon ArmorStand ~ ~ ~ {Marker:1,Invulnerable:1,Invisible:1,Tags:["ASRot"]}""")
            yield U("""tp @e[tag=ASRot] @p[tag=Divining]""")
            for deg in [180; 90; 45; 22; 11] do
                yield U(sprintf """scoreboard players add @e[tag=ASRot,rym=%d] Rot %d""" deg deg)
                yield U(sprintf """tp @e[tag=ASRot,rym=%d] ~ ~ ~ ~-%d ~""" deg deg)
            yield U("""scoreboard players operation @p[tag=Divining] Rot += @e[tag=ASRot] Rot""")
            yield U("""kill @e[tag=ASRot]""")
            // convert score to title text
            yield U("scoreboard players operation @p[tag=Divining] Rot %= #ThreeSixty Rot")
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=0,score_Rot=11] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=12,score_Rot=22] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=23,score_Rot=33] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=34,score_Rot=45] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=46,score_Rot=56] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=57,score_Rot=67] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=68,score_Rot=78] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=79,score_Rot=90] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=91,score_Rot=101] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=102,score_Rot=112] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=113,score_Rot=123] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=124,score_Rot=135] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=136,score_Rot=146] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=147,score_Rot=157] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=158,score_Rot=168] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=169,score_Rot=180] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=181,score_Rot=191] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=192,score_Rot=202] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=203,score_Rot=213] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=214,score_Rot=225] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=226,score_Rot=236] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=237,score_Rot=247] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=248,score_Rot=258] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=259,score_Rot=270] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=271,score_Rot=281] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=282,score_Rot=292] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=293,score_Rot=303] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=304,score_Rot=315] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=316,score_Rot=326] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=327,score_Rot=337] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=338,score_Rot=348] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=349,score_Rot=359] subtitle {"text":"%s"}""" (next()))
            yield U("""title @p[tag=Divining] title {"text":""}""")
            yield U("""scoreboard players tag @a remove Divining""")
        |]
    let r = map.GetRegion(1,1)
    r.PlaceCommandBlocksStartingAt(1,4,1,cmds,"")

    let goalX, goalZ = 60, 60
    for x = 10 to 90 do
        let cmds = 
            [|
                for z = 10 to 90 do
                    let dx = goalX-x
                    let dz = goalZ-z
                    let dy,dx = -dx,dz // Minecraft coords are insane
                    let degrees = 180.0 * System.Math.Atan2(float dy, float dx) / System.Math.PI |> int
                    let degrees = degrees + 360 // scoreboard remove only handles positive numbers (?!?)
                    yield O(sprintf "scoreboard players remove @p[tag=Divining] Rot %d" degrees)
            |]
        r.PlaceCommandBlocksStartingAt(x,3,10,cmds,"")
    map.WriteAll()

let testCompass4() =
    let map = new MapFolder("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\Superflat\region\""")
    let theString = """  ------->    ------->    ^^    <-------    <-------    --vv--  """
    let twice = theString + theString
    let at(n) = twice.Substring(n*2, 26)
    let i = ref 0
    let next() =
        let r = at(!i)
        incr i
        r
    let cmds = 
        [|
            // clean up the world
            yield O """fill 10 3 10 90 3 90 grass"""
            yield U """fill ~ ~ ~-1 ~ ~ ~120 air"""
            // init world
            yield O """scoreboard objectives add Rot dummy"""
            yield U """scoreboard players set #ThreeSixty Rot 360"""
            // always-running loop to test for holding item
            yield P(sprintf """scoreboard players tag @p add Divining {SelectedItem:{tag:{display:{Lore:["%s"]}}}}""" Strings.NameAndLore.DIVINING_ROD_LORE)
            yield U """testfor @p[tag=Divining]"""
            yield C """blockdata ~ ~ ~2 {auto:1b}"""
            yield C """blockdata ~ ~ ~1 {auto:0b}"""
            // if item held... init score and call world-location and player rotation code
            yield O """scoreboard players set @p[tag=Divining] Rot 456"""  // where the ^^ is (96), +360
            // read world data
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 0 scoreboard players remove @p[tag=Divining] Rot 6""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 1 scoreboard players remove @p[tag=Divining] Rot 17""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 2 scoreboard players remove @p[tag=Divining] Rot 28""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 3 scoreboard players remove @p[tag=Divining] Rot 39""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 4 scoreboard players remove @p[tag=Divining] Rot 51""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 5 scoreboard players remove @p[tag=Divining] Rot 62""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 6 scoreboard players remove @p[tag=Divining] Rot 73""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 7 scoreboard players remove @p[tag=Divining] Rot 84""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 8 scoreboard players remove @p[tag=Divining] Rot 96""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 9 scoreboard players remove @p[tag=Divining] Rot 107""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 10 scoreboard players remove @p[tag=Divining] Rot 118""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 11 scoreboard players remove @p[tag=Divining] Rot 129""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 12 scoreboard players remove @p[tag=Divining] Rot 141""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 13 scoreboard players remove @p[tag=Divining] Rot 152""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 14 scoreboard players remove @p[tag=Divining] Rot 163""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_glass 15 scoreboard players remove @p[tag=Divining] Rot 174""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 0 scoreboard players remove @p[tag=Divining] Rot 186""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 1 scoreboard players remove @p[tag=Divining] Rot 197""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 2 scoreboard players remove @p[tag=Divining] Rot 208""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 3 scoreboard players remove @p[tag=Divining] Rot 219""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 4 scoreboard players remove @p[tag=Divining] Rot 231""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 5 scoreboard players remove @p[tag=Divining] Rot 242""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 6 scoreboard players remove @p[tag=Divining] Rot 253""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 7 scoreboard players remove @p[tag=Divining] Rot 264""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 8 scoreboard players remove @p[tag=Divining] Rot 276""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 9 scoreboard players remove @p[tag=Divining] Rot 287""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 10 scoreboard players remove @p[tag=Divining] Rot 298""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 11 scoreboard players remove @p[tag=Divining] Rot 309""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 12 scoreboard players remove @p[tag=Divining] Rot 321""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 13 scoreboard players remove @p[tag=Divining] Rot 332""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 14 scoreboard players remove @p[tag=Divining] Rot 343""")
            yield U("""execute @p[tag=Divining] ~ ~-1 ~ detect ~ ~ ~ stained_hardened_clay 15 scoreboard players remove @p[tag=Divining] Rot 354""")
            // detect player rotation 
            yield U """title @p[tag=Divining] times 0 10 0""" // is not saved with the world, so has to be re-executed to ensure run after restart client
            yield U("""summon ArmorStand ~ ~ ~ {Marker:1,Invulnerable:1,Invisible:1,Tags:["ASRot"]}""")
            yield U("""tp @e[tag=ASRot] @p[tag=Divining]""")
            for deg in [180; 90; 45; 22; 11] do
                yield U(sprintf """scoreboard players add @e[tag=ASRot,rym=%d] Rot %d""" deg deg)
                yield U(sprintf """tp @e[tag=ASRot,rym=%d] ~ ~ ~ ~-%d ~""" deg deg)
            yield U("""scoreboard players operation @p[tag=Divining] Rot += @e[tag=ASRot] Rot""")
            yield U("""kill @e[tag=ASRot]""")
            // convert score to title text
            yield U("scoreboard players operation @p[tag=Divining] Rot %= #ThreeSixty Rot")
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=0,score_Rot=11] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=12,score_Rot=22] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=23,score_Rot=33] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=34,score_Rot=45] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=46,score_Rot=56] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=57,score_Rot=67] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=68,score_Rot=78] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=79,score_Rot=90] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=91,score_Rot=101] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=102,score_Rot=112] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=113,score_Rot=123] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=124,score_Rot=135] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=136,score_Rot=146] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=147,score_Rot=157] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=158,score_Rot=168] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=169,score_Rot=180] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=181,score_Rot=191] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=192,score_Rot=202] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=203,score_Rot=213] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=214,score_Rot=225] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=226,score_Rot=236] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=237,score_Rot=247] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=248,score_Rot=258] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=259,score_Rot=270] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=271,score_Rot=281] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=282,score_Rot=292] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=293,score_Rot=303] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=304,score_Rot=315] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=316,score_Rot=326] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=327,score_Rot=337] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=338,score_Rot=348] subtitle {"text":"%s"}""" (next()))
            yield U(sprintf """title @p[tag=Divining,score_Rot_min=349,score_Rot=359] subtitle {"text":"%s"}""" (next()))
            yield U("""title @p[tag=Divining] title {"text":""}""")
            yield U("""scoreboard players tag @a remove Divining""")
        |]
    let r = map.GetRegion(1,1)
    r.PlaceCommandBlocksStartingAt(1,4,1,cmds,"")

    let goalX, goalZ = 60, 60
    for x = 10 to 90 do
        for z = 10 to 90 do
            let dx = goalX-x
            let dz = goalZ-z
            let dy,dx = -dx,dz // Minecraft coords are insane
            let degrees = 180.0 * System.Math.Atan2(float dy, float dx) / System.Math.PI |> int
            let degrees = degrees + 720
            let degrees = degrees % 360
            let mutable degrees = degrees
            let mutable steps = 0
            for D in [180; 90; 45; 22; 11] do
                if degrees >= D then
                    degrees <- degrees - D
                    steps <- steps + 1
                steps <- steps * 2
            steps <- steps / 2
            if steps < 16 then
                map.SetBlockIDAndDamage(x,3,z,95uy,byte steps) // 95=stained_glass
            else
                map.SetBlockIDAndDamage(x,3,z,159uy,byte(steps-16)) // 159=stained_hardened_clay
    map.WriteAll()

////////////////////////////////////////

let uuidToTwoLongsThingy() =
    let uuid = "CB3F55D3-645C-4F38-A497-9C13A33DB5CF"
    let hexDigits = uuid.Replace("-","")
    let a = Array.create 16 0uy
    let HEX = "0123456789ABCDEF"
    for i in [0 .. 2 .. 31] do
        let x = HEX.IndexOf(hexDigits.[i])
        let y = HEX.IndexOf(hexDigits.[i+1])
        let v = x*16 + y
        a.[i/2] <- byte v
    System.Array.Reverse(a)
    let lo = System.BitConverter.ToInt64(a,0)
    let hi = System.BitConverter.ToInt64(a,8)
    printfn "%d   %d" hi lo

////////////////////////////////////////

let generateSuperLongSnakingCommandBlockChain() =
    let worldSaveFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\ReproCmds\region\"""
    let map = new MapFolder(worldSaveFolder)
    let tes = ResizeArray()
    let x,y,z = 100,100,100
    let command = "scoreboard players add Long Score 1"
    for dx = 0 to 11 do
        let x = x + dx
        if x % 2 = 0 then
            for dz = 0 to 99 do
                let z = z + dz
                map.EnsureSetBlockIDAndDamage(x,y,z,211uy,3uy)
                tes.Add [| Int("x",x); Int("y",y); Int("z",z); String("id","minecraft:command_block"); Byte("auto",1uy); String("Command",command); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); End |]
            let z = z + 100
            map.EnsureSetBlockIDAndDamage(x,y,z,211uy,5uy)
            tes.Add [| Int("x",x); Int("y",y); Int("z",z); String("id","minecraft:command_block"); Byte("auto",1uy); String("Command",command); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); End |]
        else
            for dz = 100 downto 1 do
                let z = z + dz
                map.EnsureSetBlockIDAndDamage(x,y,z,211uy,2uy)
                tes.Add [| Int("x",x); Int("y",y); Int("z",z); String("id","minecraft:command_block"); Byte("auto",1uy); String("Command",command); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); End |]
            let z = z + 0
            map.EnsureSetBlockIDAndDamage(x,y,z,211uy,5uy)
            tes.Add [| Int("x",x); Int("y",y); Int("z",z); String("id","minecraft:command_block"); Byte("auto",1uy); String("Command",command); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); End |]
    map.AddOrReplaceTileEntities(tes)
    map.WriteAll()

////////////////////////////////////////

let SERVER_DIRECTORY = """C:\Users\Admin1\Desktop\Server""" 
let COMMAND_FILE = System.IO.Path.Combine(SERVER_DIRECTORY,"commands_to_run.txt")
let putCommandBlocks() =
    let cmds = [|
        yield P """execute @e[type=Fireball,c=1] ~ ~-0.1 ~ summon Snowball {Motion:[0.0,1.0,0.0]}"""
        for x in [-2.0; -1.5; -1.0; -0.5; 0.5; 1.0; 1.5; 2.0] do
            for z in [-2.0; -1.5; -1.0; -0.5; 0.5; 1.0; 1.5; 2.0] do
                yield U(sprintf """execute @e[type=Fireball,c=1] ~%3f ~-0.1 ~%3f summon Snowball {Motion:[0.0,1.0,0.0]}""" x z)
        yield U """blockdata ~ ~ ~2 {auto:1b}"""
        yield U """blockdata ~ ~ ~1 {auto:0b}"""
        yield O """entitydata @e[type=Snowball] {ownerName:"Lorgon111"}"""
        |]
    let mutable x,y,z = 1211, 27, -122 
    let strings = ResizeArray()
    for c in cmds do
        match c with
        | P cmd -> strings.Add(sprintf """setblock %d %d %d repeating_command_block 3 replace {Command:"%s"}""" x y z (escape cmd)); z <- z + 1
        | O cmd -> strings.Add(sprintf """setblock %d %d %d command_block 3 replace {Command:"%s"}""" x y z (escape cmd)); z <- z + 1
        | U cmd -> strings.Add(sprintf """setblock %d %d %d chain_command_block 3 replace {Command:"%s",auto:1b}""" x y z (escape cmd)); z <- z + 1
        | C cmd -> strings.Add(sprintf """setblock %d %d %d chain_command_block 11 replace {Command:"%s",auto:1b}""" x y z (escape cmd)); z <- z + 1
    strings.Add(sprintf """setblock %d %d %d air""" x y z)
    System.IO.File.WriteAllLines(COMMAND_FILE,strings)

////////////////////////////////////////

let interpolate(start, stop, n, steps) = start + (stop-start)*(float(n)/float(steps))

let carveSphere(map:MapFolder, xc, yc, zc, r) = 
    let sq x = x*x
    for x = int(xc-r-1.0) to int(xc+r+1.0) do
        for y = int(yc-r-1.0) to int(yc+r+1.0) do
            for z = int(zc-r-1.0) to int(zc+r+1.0) do
                if sq(float(x)-xc)+sq(float(y)-yc)+sq(float(z)-zc) < sq(r) then
                    map.EnsureSetBlockIDAndDamage(x,y,z,0uy,0uy)

let MAX_INTERPOLATE = 2
let carvePassage(map:MapFolder, xc1, yc1, zc1, r1, xc2, yc2, zc2, r2) =
    let MAX = MAX_INTERPOLATE
    for n in 0..MAX do
        let xc = interpolate(xc1,xc2,n,MAX)
        let yc = interpolate(yc1,yc2,n,MAX)
        let zc = interpolate(zc1,zc2,n,MAX)
        let r  = interpolate(r1, r2, n,MAX)
        carveSphere(map, xc, yc, zc, r)

let makeRandomCave(map:MapFolder, xs, ys, zs, rs, initPhi, initTheta, desiredLength, rng:System.Random) =
    // spherical coordinates (phi=0 is up) to minecraft axes:
    // x = sin phi * cos theta
    // z = sin phi * sin theta
    // y = cos phi
    let inBounds(x,y,z,r) =
        x-r > -400.0 && x+r < 400.0 &&
        y-r >   10.0 && y+r < 130.0 &&
        z-r > -400.0 && z+r < 400.0
    let computeNextXYZ(curX, curY, curZ, nextPhi, nextTheta, nextLength) =
        let nextX = curX + (sin(nextPhi)*cos(nextTheta)*nextLength)
        let nextZ = curZ + (sin(nextPhi)*sin(nextTheta)*nextLength)
        let nextY = curY + (cos(nextPhi)*nextLength)
        nextX, nextY, nextZ
    let mutable curX, curY, curZ, curR, curPhi, curTheta, totalLength = xs, ys, zs, rs, initPhi, initTheta, 0.0
    let mutable failCount = 0
    while totalLength < desiredLength do
        // choose a next heading, r, & segment length
        let nextPhi = curPhi + (rng.NextDouble()-0.5)*1.0 // +/- half radian
        let nextTheta = curTheta + (rng.NextDouble()-0.5)*2.0
        let nextR = curR + (rng.NextDouble()-0.5)*1.0
        let nextLength = rng.NextDouble()*7.0 + 5.0 // 5 to 12
        // (ensure connected)
        let minR = min nextR curR
        let maxLen = minR * 2.0 * float(MAX_INTERPOLATE)
        let nextLength = min nextLength maxLen
        // validate
        let mutable ok = true
        if nextR < 1.5 || nextR > 5.0 then
            ok <- false
            printfn "R out of bounds"
        let nextX,nextY,nextZ = computeNextXYZ(curX, curY, curZ, nextPhi, nextTheta, nextLength)
        if not(inBounds(nextX,nextY,nextZ,nextR)) then
            ok <- false
            printfn "out of bounds: (%3.0f, %3.0f, %3.0f) (%2.1f, %1.2f, %1.2f)" nextX nextY nextZ nextR nextPhi nextTheta
        // heuristic to avoid steering at a wall we later can't avoid
        for tries = 8 downto 1 do
            if ok then
                let nextX,nextY,nextZ = computeNextXYZ(curX, curY, curZ, nextPhi, nextTheta, nextLength * float(tries))
                if not(inBounds(nextX,nextY,nextZ,nextR)) then
                    if rng.Next(tries+4) <= 4 then
                        ok <- false
                        printfn "steering towards bounds %d: (%3.0f, %3.0f, %3.0f) (%2.1f, %1.2f, %1.2f)" tries nextX nextY nextZ nextR nextPhi nextTheta
                    else
                        printfn "steering towards bounds %d but ignoring" tries
        if ok then
            // make next segment
            carvePassage(map, curX, curY, curZ, curR, nextX, nextY, nextZ, nextR)
            curX <- nextX
            curY <- nextY
            curZ <- nextZ
            curR <- nextR
            curPhi <- nextPhi
            curTheta <- nextTheta
            totalLength <- totalLength + nextLength
            printfn "making segment: (%3.0f, %3.0f, %3.0f) (%2.1f, %1.2f, %1.2f)" nextX nextY nextZ nextR nextPhi nextTheta
            failCount <- 0
        else
            failCount <- failCount + 1
        if failCount = 16 then
            printfn "failed 16 times in a row, picking a new random heading to escape"
            curPhi <- rng.NextDouble()*2.0*System.Math.PI 
            curTheta <- rng.NextDouble()*2.0*System.Math.PI 
            failCount <- 0
        
////////////////////////////////////////


open LootTables
open Recipes
open Advancements

[<System.STAThread()>]  
do   
    let user = "Admin1"


#if BINGO

(*
TO TEST

prep map, e.g. 

click button to set night vision config
ensure version info/date all updated
/scoreboard players set HasTheMapEverBeenLoadedBefore Calc 0   
/scoreboard players tag @a remove playerHasBeenSeen
/scoreboard players reset Lorgon111
/scoreboard teams leave
clear unnecessary files out of directories

--------------

with release, pin tweet with video link, map link, subreddit
update twitter profile to have link to map, donate, etc

3.0 http://www.minecraftworldmap.com/worlds/MgmI_
3.1 https://www.minecraftworldmap.com/worlds/k0c6d
http://www.minecraftforum.net/forums/mapping-and-modding/maps/1557706-14w11b-mini-game-surv-minecraft-bingo-vanilla
now http://www.minecraftforum.net/forums/mapping-and-modding/maps/1557706-minecraft-bingo-vanilla-survival-scavenger-hunt
https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=YFATHZXXAXRZS
https://youtu.be/3V3nDkAuMQ0
https://youtu.be/x-udqNYXBJw

MB 3.1 now available, DL link in description
In this video, i'll discuss what's new/different in 3.1.  If new to bingo and want overview please see the 3.0 video linked in desc.
--
The 3.1 features fall in 3 main categories: what's different in MC, new items on the card, and other usability features
--
Minecraft - MB 3.1 now runs in MC 1.11.2.  
That means you may encounter new mobs like llamas or polar bears, and are more likely to encounter villages which can now span across biomes in Minecraft.
However abandoned mineshafts are now disabled, because otherwise they could appear on the surface of Mesa biomes, 
  offering a very unfair advantage to players who spawned near these in multi-player.
--
Items - There are 6 new items that can appear on MB 3.1 cards.
fireworks rocket and fern, cocoa bean and beetroot stew, furnace minecart and hopper minecart
Two items were also removed: the normal plain minecart (which now has more variants) and the cobweb (which is removed since mineshafts are removed).
598194 fern/fwr, 169499 c/b, 466363 f/hmc
--
Features - There are a few minor feature upgrades in MB 3.1
A minutes&seconds timer now appears on the statusbar, making it easier to see the current game time at a glance
Maps now start in your offhand at the start of the game, so you can see the card while you explore
The floor of the lobby is now double-thick, to make it less likely you'll glitch through the floor when horsing around in boats
Frost Walker has been changed to Depth Strider in the custom configuration options, and the elytra option now starts you with fireworks rockets
--
That's the summary of what's new & different in MB 3.1; download the map and have fun! 
If you're looking to compete against others, join us on the minecraftbingo subreddit (linked below) for weekly seed competitions.
--
This is the 9th release of MB I've published in 3.5 years of development.  If you enjoy the game and would like to leave me a tip for my programming work, you can find a donation link
in the description below, or in-game by clicking this sign on the wall of the lobby.  Thanks!
--
(probably outro of me in lobby - music throughout?)


https://gfycat.com/EcstaticTerribleAngelwingmussel


(Fifteen seconds doesn't quite do it justice, so here's a little more info.)

MinecraftBINGO is the vanilla survival scavenger hunt mini-game for Minecraft 1.9.  You get a BINGO Card (map) picturing 25 items, spawn out in the world, and play vanilla survival with the goal of collecting/crafting items on the card in order to get 5 in a row/column/diagonal as fast as possible.  Your progress is tracked on your BINGO card map.

Multi-player supports any number of players on up to 4 teams to race against each other for BINGO (or to collaborate for the best time).  Single-player has 'seeded' cards, so you can race a friend as they play the same card in their own single-player world.

There are other game modes and customization options; if you want to learn more, check out this (ad-free) [4-minute video](https://youtu.be/3V3nDkAuMQ0).

You can [download the map here](http://www.minecraftworldmap.com/worlds/MgmI_) - MinecraftBINGO runs on vanilla 1.9 command blocks (no mods or resource packs required).

--------------

feature list:
vanilla survival mini game
random card generation of 25 of 61 items 
each item get: sound, score update, chat msg
throw map on ground to update card viz
auto-detect bingo/blackout wins, keeps time
show 25-min score
tiny biomes, many dungeons, no -ite
up to 4 teams in SMP, collab or compete
lockout mode
seeded cards & spawns (90000 possible)
automatic game start configs (night vision, starting items), customizable
3.1 changes: https://www.reddit.com/r/minecraftbingo/comments/5vimhp/bingo_31_beta_for_minecraft_1112/dexozib/ (and maybe new timer)
*)

    let readInSomeArt = false
    if readInSomeArt then
        let fil = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\BingoArt\region\r.0.0.mca"""
        //let fil = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\bingo111\region\r.0.0.mca"""
        let r = new RegionFile(fil)
        let arr = ResizeArray()
        let s = ArtAssets.readZoneIntoString(r,183,3,39,16,1,16)
        //let s = ArtAssets.readZoneIntoString(r,64,19,0,63,1,63)
        arr.Add(sprintf """let cw = "%s" """ s)
        let writePath = """C:\Users\Admin1\Documents\GitHubVisualStudio\minecraft-map-manipulator\MinecraftMapManipulator\ConsoleApplication1\Temp.txt"""
        System.IO.File.WriteAllLines(writePath, arr)
        failwith "done"

    printfn "bingo seed is 8126031 preset to clipboard..."
    System.Windows.Clipboard.SetText(MC_Constants.defaultWorldWithCustomOreSpawns(1,100,4,80,true,true,false,true,true,true,MC_Constants.oreSpawnBingo))

(*
    let tmp3LevelDat = sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\tmp3_111\level.dat""" user
    //dumpPlayerDat("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\Eventide Trance v1.0.2\level.dat""")
    //dumpPlayerDat(tmp3LevelDat)
    Utilities.renamer(tmp3LevelDat,"\u00A7l\u00A76Minecraft\u00A7dBINGO \u00A79v3.1 \u00A7aby \u00A7eLorgon111\u00A7r ")
    failwith "bang"
*)

    let onlyArt = false
    let save = if onlyArt then "BingoArt" else "bingo111"
    //dumpTileTicks(sprintf """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\%s\region\r.0.0.mca""" save)
    //removeAllTileTicks(sprintf """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\%s\region\r.0.0.mca""" save)
    for x in [-2..1] do
        for z in [-2..1] do
            System.IO.File.Copy(sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\Void\region\r.%d.%d.mca""" user x z,
                                sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\r.%d.%d.mca""" user save x z, true)
    try 
        MinecraftBingo.placeCommandBlocksInTheWorld(sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\r.0.0.mca""" user save, onlyArt) 
    with e -> 
        printfn "caught exception: %s" (e.Message)
    let bingoFolder = sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\""" user save
    let bingoMap = new MapFolder(bingoFolder)
    RecomputeLighting.relightTheWorldHelper(bingoMap,[-1;0],[-1;0],false)
    (*
    preciseImageToBlocks(sprintf """C:\Users\%s\Desktop\Minimap_Floor_6.png""" user, sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\""" user save, 36)
    preciseImageToBlocks(sprintf """C:\Users\%s\Desktop\Minimap_Floor_7.png""" user, sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\""" user save, 32)
    preciseImageToBlocks(sprintf """C:\Users\%s\Desktop\Minimap_Floor_8.png""" user, sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\""" user save, 28)
    *)
    System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp4\data\map_0.dat.new""",
                        sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\data\map_0.dat""" user save, true)
    System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp4\data\map_1.dat.new""",
                        sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\data\map_1.dat""" user save, true)
    if not onlyArt then
        System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp3_111\level.dat""",
                            sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\level.dat""" user save, true)
        System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp3_111\icon.png""",
                            sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\icon.png""" user save, true)
    //dumpSomeCommandBlocks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp9\region\r.0.0.mca""")
    //dumpSomeCommandBlocks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Seed9917 - Copy35e\region\r.0.0.mca""")


#else


    //killAllEntities()
    //dumpChunkInfo("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\rrr\region\r.0.-3.mca""", 0, 31, 0, 31, true)
    //dumpSomeCommandBlocks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\SnakeGameByLorgon111\region\r.0.0.mca""")
    //diffRegionFiles("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmpy\region\r.0.0.mca""",
      //              """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmpy\region\r.0.0.mca.new""")
    //placeCertainEntitiesInTheWorld()
    //mixTerrain()
    //findStrongholds()
    //dumpPlayerDat("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\fun with clone\playerdata\6fbefbde-67a9-4f72-ab2d-2f3ee5439bc0.dat""")
    //editMapDat("""C:\Users\"""+user+"""\Desktop\Eventide Trance v1.0.0 backup1\data\map_1.dat""")
    //editMapDat("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp4\data\map_1.dat""")
    //mapDatToPng("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp9\data\map_0.dat""", """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp9\data\map_0.png""")
    //findAllLootBookItems("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\VoidLoot\region\""")
    //findAllLoot("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Seed5Normal\region\""")
    //testBackpatching("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\VoidLoot\region\r.0.0.mca""")
    //repopulateAsAnotherBiome()
    //debugRegion()
    //dumpTileTicks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\RandomCTM\region\r.0.0.mca""")
    //diffDatFilesGui("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\tmp3\level.dat""","""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\tmp9\level.dat""")
    //diffDatFilesText("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\tmp3\level.dat""","""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\tmp9\level.dat""")
    //placeCertainBlocksInTheWorld()
    //placeVideoFramesInTheWorld()
    //dumpPlayerDat("""C:\Users\"""+user+"""\Desktop\igloo45a\igloo_bottom.nbt""")
    //musicStuff()
    //plotRegionalDifficulty()
    //chatToVoiceDemo()


    //dumpPlayerDat("""C:\Users\Admin1\Desktop\ship.nbt""")
    //dumpPlayerDat """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\tmp9\stats\6fbefbde-67a9-4f72-ab2d-2f3ee5439bc0.dat"""

#if ADVANCEMENTS
    let recipes = 
        ["book",ShapelessCrafting([|MC"paper";MC"paper";MC"paper";MC"leather"|],MC"book")
         "golden_axe",ShapedCrafting(PatternKey([|"XX";"#X";" #"|],[|'#',MC"stick";'X',MC"gold_ingot"|]),MC"golden_axe")
         ]
    writeRecipes(recipes,"""C:\Users\Admin1\Desktop\RecipeSamples""")
    let dummyCriteria = [|Criterion("cx",MC"recipe_unlocked",[|HasRecipe(MC"chest")|])|]
    let advancements =
        ["pink_wool/root",Advancement(None,Display("Winterbound","What does a desc do here?",MC"wool",Task,Some(MC"textures/blocks/wool_colored_pink.png")),NoReward,dummyCriteria,[|[|"cx"|]|])
         "pink_wool/emerald_1",Advancement(Some(PATH"pink_wool:root"),Display("Emerald #1","",MC"emerald",Task,None),NoReward,dummyCriteria,[|[|"cx"|]|])
         "pink_wool/music",Advancement(Some(PATH"pink_wool:root"),Display("Tune of Gale","",MC"record_strad",Task,None),NoReward,dummyCriteria,[|[|"cx"|]|])
         "pink_wool/wool",Advancement(Some(PATH"pink_wool:root"),Display("Pink Wool","",MC"wool",Task,None),NoReward,dummyCriteria,[|[|"cx"|]|])
         "magenta_wool/root",Advancement(None,Display("Aurora Valley","",MC"wool",Task,Some(MC"textures/blocks/wool_colored_magenta.png")),NoReward,dummyCriteria,[|[|"cx"|]|])
         "magenta_wool/emerald_3",Advancement(Some(PATH"magenta_wool:root"),Display("Emerald #3","",MC"emerald",Task,None),NoReward,dummyCriteria,[|[|"cx"|]|])
         "magenta_wool/emerald_4",Advancement(Some(PATH"magenta_wool:emerald_3"),Display("Emerald #4","",MC"emerald",Task,None),NoReward,dummyCriteria,[|[|"cx"|]|])
         "magenta_wool/emerald_5",Advancement(Some(PATH"magenta_wool:emerald_4"),Display("Emerald #5","",MC"emerald",Task,None),NoReward,dummyCriteria,[|[|"cx"|]|])
         "magenta_wool/dummy",Advancement(Some(PATH"magenta_wool:emerald_5"),NoDisplay,NoReward,dummyCriteria,[|[|"cx"|]|])
         "magenta_wool/music",Advancement(Some(PATH"magenta_wool:root"),Display("Tune of Blessing","",MC"record_far",Task,None),NoReward,dummyCriteria,[|[|"cx"|]|])
         "magenta_wool/wool",Advancement(Some(PATH"magenta_wool:root"),Display("Magenta Wool","This is what happens if you type a ridiculously long description that is probably going to fill the screen with tons of text and maybe wrap poorly or look weird or something, who knows, right?",MC"wool",Task,None),NoReward,dummyCriteria,[|[|"cx"|]|])
(*        
        Reward([|MS"chest"|],
                               [|Criterion("slightly_full_inventory",MS"inventory_changed",[|NumInventorySlotsFull 9|])
                                 Criterion("already_has_recipe",MS"recipe_unlocked",[|HasRecipe(MS"chest")|])|],
                                 [|[|"slightly_full_inventory"|];[|"already_has_recipe"|]|])
         "upgrade_tools",Display(MS"stone_pickaxe","Upgrade tools",MS"story/mine_stone",
                                 [|Criterion("stone_pickaxe",MS"inventory_changed",[|HasItems[|MS"stone_pickaxe"|]|])|])                        
*)
         ]
    writeAdvancements(advancements,"""C:\Users\Admin1\Desktop\RecipeSamples""")
#endif


#if DO_MOUSECURSOR_STUFF
    let folder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\MouseCursor"""
    MouseCursorUtilties.putItAllInTheWorld(folder)
    let mf = new MapFolder(folder+"""\region\""")
    Utilities.placePhotoAtConstantZ(0,4,-1,mf)
    mf.WriteAll()
#endif


#if YADDA
    //let worldFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\Prerelease1test"""
    let worldFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\pre1world"""

    let advancements =
        ["testing/drank_luck",Advancement(None,NoDisplay,Reward([||],[||],0,sprintf"%s:drank_luck"FunctionCompiler.FUNCTION_NAMESPACE),
            [|Criterion("did",MC"consume_item",[|Item(MC"potion",1,MC"luck")|])|],[|[|"did"|]|])
         ]
    writeAdvancements(advancements,worldFolder)
    let drank_luck = ("drank_luck",[|
        """tellraw @a ["drank luck potion"]"""
        """advancement revoke @a only testing:drank_luck"""
        |])

    //let p = FunctionCompiler.mandelbrotProgram
    let p = FunctionUtilities.raycastProgram
    let p = FunctionCompiler.inlineAllDirectTailCallsOptimization(p)
    let _init, funcs = FunctionCompiler.compileToFunctions(p,(*isTracing*)false)
    let mutable commandCount = 0
    let uuid = System.Guid.NewGuid()
    let least,most = Utilities.toLeastMost(uuid)
    let summonCmd = sprintf "summon armor_stand ~ ~ ~ {NoGravity:1,UUIDMost:%dl,UUIDLeast:%dl,Invulnerable:1}" most least
    let killCmd = sprintf "kill %s" (uuid.ToString())
    let allFuncs = [|
        yield! funcs
        yield! FunctionUtilities.profileThis("p",[],["scoreboard players add @p A 1"],[])
        yield! FunctionUtilities.profileThis("x",[],["scoreboard players add x A 1"],[])
        yield! FunctionUtilities.profileThis("uuide",[summonCmd],[sprintf "scoreboard players add %s A 1" (uuid.ToString())],[killCmd])
        yield drank_luck
        let (FunctionCompiler.DropInModule(_,init,fs)) = FunctionUtilities.prng 
        yield "prng_init",init
        yield! fs
        |]
    for name,cmds in allFuncs do
        let path = worldFolder + (sprintf """\data\functions\%s\%s.mcfunction""" FunctionCompiler.FUNCTION_NAMESPACE name)
        let dir = System.IO.Path.GetDirectoryName(path)
        System.IO.Directory.CreateDirectory(dir) |> ignore
        System.IO.File.WriteAllLines(path,cmds)
        commandCount <- commandCount + cmds.Length 
    printfn "%d commands were written to %d functions" commandCount allFuncs.Length 

    let zombieWithDiamond = 
            Pools [Pool(Roll(1,1), [LootTables.Item("minecraft:diamond", []), 1, 0, []])
                   Pool(Roll(1,1), [LootTable("minecraft:entities/zombie"), 1, 0, []])]
    LootTables.writeLootTables(["lorgon111:zombie_with_diamond",zombieWithDiamond],worldFolder)

#endif

    let worldFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\life"""
    let lifeFuncs = [|
        let (FunctionCompiler.DropInModule(_,init,fs)) = FunctionUtilities.conwayLife  
        yield "life_init",init
        yield! fs
        |]
    for name,cmds in lifeFuncs do
        //let path = worldFolder + (sprintf """\data\functions\%s\%s.mcfunction""" "conway" name)
        let path = worldFolder + (sprintf """\data\functions\%s\%s.txt""" "conway" name)
        let dir = System.IO.Path.GetDirectoryName(path)
        System.IO.Directory.CreateDirectory(dir) |> ignore
        System.IO.File.WriteAllLines(path,cmds)



#if DO_NOLATENCY_COMPILER_STUFF
    let worldFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\M2"""
    let map = new MapFolder(worldFolder+"""\region""")
    let region = map.GetRegion(0,0)
    let CMDICBX,CMDICBY,CMDICBZ = 20,4,2
    let p = AdvancementCompiler.program
    let p = AdvancementCompiler.inlineAllDirectTailCallsOptimization(p)
    let getLOP() = AdvancementCompiler.mandelbrotVars.All() |> Seq.filter (fun v -> v.LivesOnPlayer) |> Seq.length
    let mutable lop = getLOP()
    let mutable prevlop = -1
    printfn "init on-player: %d" lop
    while prevlop <> lop do
        p.Visit()
        prevlop <- lop
        lop <- getLOP()
        printfn "... now on-player: %d" lop
    printfn "final on-player: %d   of %d total vars" lop (AdvancementCompiler.mandelbrotVars.All().Length)
    let init, repump, advancements = AdvancementCompiler.advancementize(p,(*isTracing*)false,CMDICBX,CMDICBY,CMDICBZ)
    region.PlaceCommandBlocksStartingAt(5,4,2,[|
        yield O ""
        yield U "kill @e[type=armor_stand]"  // both compiler and code summon some, want to start fresh, but can't have code killing compiler or vice versa...
        yield U init
        |],"init",false,true)
    region.PlaceCommandBlocksStartingAt(CMDICBX,CMDICBY,CMDICBZ,repump,"repump",false,true)
    writeAdvancements(advancements,worldFolder)
    map.WriteAll()




    let worldFolder = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\M2"""
    let map = new MapFolder(worldFolder+"""\region""")
    let region = map.GetRegion(0,0)

#if ADVANCEMENTS
    let CMDICBX,CMDICBY,CMDICBZ = 20,4,2
    let p = Mandelbrot.program
    let p = NoLatencyCompiler.inlineAllDirectTailCallsOptimization(p)
    let init, repump, advancements = NoLatencyCompiler.advancementize(p,(*isTracing*)false,CMDICBX,CMDICBY,CMDICBZ)
#else
#if HYBRID
    let CMDICBX,CMDICBY,CMDICBZ = 8,4,2
    let p = Mandelbrot.program
    let p = NoLatencyCompiler.inlineAllDirectTailCallsOptimization(p)
    let init, cmds, advancements = NoLatencyCompiler.linearize(p,(*isTracing*)false,CMDICBX,CMDICBY,CMDICBZ)
#else
#if CLONEMACHINE
    let CMDICBX,CMDICBY,CMDICBZ = 30,4,2
    let p = Mandelbrot.program
    let p = NoLatencyCompiler.inlineAllDirectTailCallsOptimization(p)
    let init, advancements = NoLatencyCompiler.makeCloneMachineInTheWorld(p,(*isTracing*)false,region,CMDICBX,CMDICBY,CMDICBZ)
#else    
    let CMDICBX,CMDICBY,CMDICBZ = 8,4,2
    let init, cmds = NoLatencyCompiler.linearize(Mandelbrot.program,(*isTracing*)false,CMDICBX,CMDICBY,CMDICBZ)
#endif
#endif
#endif
    printfn "MAND %d" Mandelbrot.objectivesAndConstants.Length
    for s in Mandelbrot.objectivesAndConstants do
        printfn "%s" s
    printfn "INIT %d" init.Count 
    for s in init do
        printfn "%s" s
#if ADVANCEMENTS
#else
#if CLONEMACHINE
#else
    printfn "CMDS %d" cmds.Count 
    for t,s in cmds do
        printfn "%s %s" (match t with NoLatencyCompiler.U -> " " | _ -> "C" ) s
#endif
#endif

    region.PlaceCommandBlocksStartingAt(2,4,2,[|
        yield O ""
        yield! (Mandelbrot.objectivesAndConstants |> Seq.map (fun s -> U s))
        |],"m startup",false,true)
    region.PlaceCommandBlocksStartingAt(5,4,2,[|
        yield O ""
        yield! (init |> Seq.map (fun s -> U s))
        |],"init",false,true)
    let linearAndCloneMachineStartupSequence = [|
        yield O "blockdata ~ ~ ~ {auto:0b}"
#if PREEMPT
        yield U "worldborder set 10000000"
        yield U "worldborder add 1000000 1000"
#else
        yield U ""
        yield U ""
#endif
        yield U (sprintf "scoreboard players set %s %s 0" NoLatencyCompiler.ScoreboardNameConstants.PulseICB NoLatencyCompiler.ScoreboardNameConstants.IP)
        yield U "setblock ~ ~ ~2 chain_command_block 3 {auto:1b}"
        yield U "blockdata ~ ~ ~1 {UpdateLastExecution:0b}"
        yield U ""  // may get stoned
        |]
#if ADVANCEMENTS
    region.PlaceCommandBlocksStartingAt(CMDICBX,CMDICBY,CMDICBZ,repump,"repump",false,true)
    let summonzombies = NoLatencyCompiler.makeAdvancement("summonzombies",[|yield "advancement revoke @p only functions:summonzombies"; for x = 1 to 40 do for z = 1 to 40 do yield sprintf "execute @s ~%d ~ ~%d summon zombie" (10+x) (10+z)|])
    let advancements = [| yield! advancements; yield summonzombies |]
    writeAdvancements(advancements,worldFolder)
#else
#if CLONEMACHINE
    region.PlaceCommandBlocksStartingAt(CMDICBX,CMDICBY,CMDICBZ,linearAndCloneMachineStartupSequence,"startup prefix",false,false)
    writeAdvancements(advancements,worldFolder)
#else
    let allcmds = cmds |> Seq.map (fun (t,s) -> match t with NoLatencyCompiler.U -> U s | _ -> C s) |> Array.ofSeq 
    let mutable i = allcmds.Length / 2
    while allcmds.[i].IsConditional do
        i <- i + 1
    // i is now an index it's safe to 'break' right before, without changing conditional logic
    let part1    = allcmds.[0..i-1]
    let part2rev = allcmds.[i..] |> Array.rev 
    let firstHalf = [|
        yield! linearAndCloneMachineStartupSequence
        yield! part1
        |]
    region.PlaceCommandBlocksStartingAt(CMDICBX,CMDICBY,CMDICBZ,firstHalf,"part1",false,true)
    region.PlaceCommandBlocksStartingAt(CMDICBX+1,CMDICBY,CMDICBZ+7,[|
        yield O ""  // dummy to manually break and link up
        yield! part2rev
        |],"part2",false,true,2uy)
#if HYBRID
    writeAdvancements(advancements,worldFolder)
#endif
#endif
#endif
    map.WriteAll()


#endif



    (*
    compareMinecraftAssets("""C:\Users\Admin1\Desktop\1.9.4.zip""","""C:\Users\Admin1\Desktop\16w20a.zip""")
    // compare sounds.json
    let currentSoundsJson = System.IO.File.ReadAllLines("""C:\Users\Admin1\AppData\Roaming\.minecraft\assets\objects\54\54511a168f5960dd36ff46ef7a9fd1d4b1edee4a""")
    let oldSoundsJson = System.IO.File.ReadAllLines("""C:\Users\Admin1\Desktop\54511a168f5960dd36ff46ef7a9fd1d4b1edee4a""")
    let normalize(a:string[]) =
        for i = 0 to a.Length-1 do
            a.[i] <- a.[i].Replace("\r","")
    normalize(currentSoundsJson)
    normalize(oldSoundsJson) 
    if not(diffStringArrays(oldSoundsJson, currentSoundsJson)) then
        printfn "no sound json diff"
    *)

#if VANILLA_SWIRL_STUFF
    let worldSaveFolder = """C:\Users\""" + user + """\AppData\Roaming\.minecraft\saves\RandomCTM"""
    let levelDat = System.IO.Path.Combine(worldSaveFolder, "level.dat")
//    Utilities.renamer(levelDat,"\u00A7l\u00A7fVanilla \u00A7aS\u00A79w\u00A7ci\u00A7dr\u00A7el \u00A7bCTM\u00A77 - May 2016 \u00A7aX\u00A7r ")
    let biomeSize = 3
    let custom = MC_Constants.defaultWorldWithCustomOreSpawns(biomeSize,50,25,80,false,false,false,false,(*ravine*)true,TerrainAnalysisAndManipulation.oreSpawnCustom)
    //let almostDefault = MC_Constants.defaultWorldWithCustomOreSpawns(biomeSize,8,4,80,true,true,true,true,true,MC_Constants.oreSpawnDefaults) // biome size kept, but otherwise default
    let brianRngSeed = 0
    //dumpPlayerDat(System.IO.Path.Combine(worldSaveFolder, "level.dat"))

//    TerrainAnalysisAndManipulation.makeCrazyMap(worldSaveFolder,brianRngSeed,custom,11)
    LootTables.writeAllLootTables(worldSaveFolder)
    // TODO below crashes game to embed world in one with diff level.dat ... but what does work is, gen world with options below, then copy the region files from my custom world to it
    // updateDat(System.IO.Path.Combine(worldSaveFolder, "level.dat"), (fun _pl nbt -> match nbt with |NBT.String("generatorOptions",_oldgo) -> NBT.String("generatorOptions",almostDefault) | _ -> nbt))
    System.IO.Directory.CreateDirectory(sprintf """%s\DIM-1\region\""" worldSaveFolder) |> ignore
    for x in [-1..0] do 
        for z in [-1..0] do 
            System.IO.File.Copy(sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\LargeVoid\region\r.%d.%d.mca""" user x z,sprintf """%s\DIM-1\region\r.%d.%d.mca""" worldSaveFolder x z, true)
    for x in [-3..2] do 
        for z in [-3..2] do 
            if -2 <= x && x <= 1 &&
               -2 <= z && z <= 1 then
                () // do nothing
            else
                // out ring, make void regions instead
                System.IO.File.Copy(sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\LargeVoid\region\r.%d.%d.mca""" user x z,sprintf """%s\region\r.%d.%d.mca""" worldSaveFolder x z, true)
    //dumpPlayerDat("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\RandomCTM\data\scoreboard.dat""")
    //dumpTileTicks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\RandomCTM\region\r.0.-1.mca""")


//    System.Windows.Clipboard.SetText(custom)   // AFK pregen: http://pastebin.com/uq09kFNW
    //let worldSeed = 14 
    //genTerrainWithMCServer(worldSeed,custom)
#endif

#if MAKE_RANDOM_CAVES
    let map = new MapFolder("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\solidCopy\region""")
    //carveSphere(map, 20.0, 115.0, 20.0, 15.0)
    //carvePassage(map, 40.0, 115.0, 40.0, 15.0, 60.0, 105.0, 50.0, 8.0)
    let rng = new System.Random(0)
    makeRandomCave(map, 40.0, 120.0, 40.0, 5.0, 1.8, 1.0, 300.0, rng)
    makeRandomCave(map, -40.0, 120.0, 40.0, 5.0, 1.8, 1.0, 300.0, rng)
    makeRandomCave(map, 40.0, 120.0, -40.0, 5.0, 1.8, 1.0, 300.0, rng)
    // TODO noise?
    // TODO track walls
    // TODO occasional round rooms?
    //RecomputeLighting.relightTheWorld(map)
    map.WriteAll()
#endif


    //EandT.populateWorld("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\Like E&T 10\region""")
    //EandT.populateWorld("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\E&T Season 9\region""")
    //EandT.populateWorld("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\testdung30\region""")
    //EandT.populateWorld("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\dung30alpha1seed17\region""")
    

    //putCommandBlocks()
    (*
    let map = new MapFolder("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\testing\region""")
    let colors = [|
                    System.Drawing.Color.Aqua 
                    System.Drawing.Color.Black
                    System.Drawing.Color.Gold 
                    System.Drawing.Color.Cyan
                    System.Drawing.Color.Orange  
                    System.Drawing.Color.Purple 
                    System.Drawing.Color.Red
                    System.Drawing.Color.Green
                 |]
    map.GetRegion(1,1).PlaceCommandBlocksStartingAt(1,6,1,[|
        yield P ""
        for i = 0 to colors.Length-1 do
            let c = colors.[i]
            let nonZero x = if x = 0.0 then 0.001 else x
            let r,g,b = nonZero((float c.R)/255.0), nonZero((float c.G)/255.0), nonZero((float c.B)/255.0)
            let x,y,z = 10.2, 6.0, 10.0+0.2*(float i)
            printfn "particle reddust %f %f %f %f %f %f 1 0" x y z r g b
            yield U (sprintf "particle reddust %f %f %f %f %f %f 1 0" x y z r g b)
        |],"",false,true)
    map.WriteAll()
    do
        let map = new MapFolder("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\QFETest\region\""")
        let X,Y,Z = -300,80,60
        makeGetAllItemsGame(map,X,Y,Z,X-10,Y,Z-10)
        RecomputeLighting.relightTheWorld(map)
        map.WriteAll()
    *)
    //testCompass4()

    (*
    let map = new MapFolder("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\VanillaSwirlCTMApr2016J - Copy\region""")
    TerrainAnalysisAndManipulation.discoverAndFixTileEntityErrors(map)
    *)
#endif


    printfn "press a key to end"
    System.Console.Beep()
    System.Console.ReadKey() |> ignore

    

#if FUN
    placeCommandBlocksInTheWorldTemp("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\fun with clone\region\r.0.0.mca""")
#endif
    ()