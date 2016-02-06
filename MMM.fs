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
            if System.IO.Path.GetExtension(name) = "nbt" then
                printfn "%s" (name.ToUpper())
                if diffDatFilesText(entry1.Open(), entry2.Open()) then
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
        r.PlaceCommandBlocksStartingAt(x,0,0,cmds,"on key",false)
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
    r.PlaceCommandBlocksStartingAt(30,0,0,initCmds,"init",false)
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
    let ss = System.Speech.Synthesis.SpeechSynthesizer()
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

open MC_Constants
let makeGetAllItemsGame() =    
    // TODO encase in barrier
    let map = new MapFolder("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\flattest\region""")
    let tes = ResizeArray()
    let YMIN = 5
    let L = (survivalObtainableItems.Length+4)/5
    let Q = (L+3)/4
    let ox = 1
    let mutable count = 0
    for oz = 1 to 4*Q do
        for y = YMIN+4 downto YMIN do
            let i = (YMIN+4-y)*L + oz - 1
            let x,z,dx,dz,facing =
                if oz <= Q then
                    ox,Q+2-oz,1,0,3
                elif oz <= 2*Q then
                    oz-Q+3,1,0,-1,0
                elif oz <= 3*Q then
                    Q+6,2+oz-2*Q,-1,0,1
                else
                    4*Q-oz+4,Q+2,0,1,2
            map.EnsureSetBlockIDAndDamage(x+dx,y,z+dz,1uy,0uy)
            count <- count + 1
            map.EnsureSetBlockIDAndDamage(ox,y,oz,211uy,3uy)
            if i < survivalObtainableItems.Length then
                let bid,dmg,name = survivalObtainableItems.[i]
                let itemName = if bid <= 255 then blockIdToMinecraftName |> Array.find (fun (x,_y) -> x=bid) |> snd else sprintf "minecraft:%s" name
                let cmd = sprintf """summon ItemFrame %d %d %d {Facing:%db,Item:{id:"%s",Count:1b,Damage:%ds,tag:{display:{Name:"%d"}}}}""" (x+2*dx) y (z+0*dz) facing itemName dmg i
                tes.Add [|Int("x",ox); Int("y",y); Int("z",oz); String("id","Control"); 
                            Byte("auto",1uy); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); 
                            String("Command",cmd); End |]
            else // empty command, just to ensure overwriting blocks
                tes.Add [|Int("x",ox); Int("y",y); Int("z",oz); String("id","Control"); 
                            Byte("auto",1uy); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); 
                            String("Command",""); End |]
                // TODO filled_map looks weird
                // TODOs from the item list, e.g. potions, ench books, etc.. figure out where draw line, maybe if more than 17 variations of X?
    for y = YMIN+4 downto YMIN do
        let x = 1
        let z = 0
        map.SetBlockIDAndDamage(x,y,z,137uy,3uy)
        let cmd = ""
        tes.Add [|Int("x",x); Int("y",y); Int("z",z); String("id","Control"); 
                    Byte("auto",0uy); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); 
                    String("Command",cmd); End |]
    map.AddOrReplaceTileEntities(tes)
    printfn "%d wall spots, %d items" count survivalObtainableItems.Length 
    map.WriteAll()

////////////////////////////////////////


[<System.STAThread()>]  
do   
    let user = "Admin1"
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


    (*
    compareMinecraftAssets("""C:\Users\Admin1\Desktop\16w05a.zip""","""C:\Users\Admin1\Desktop\16w05b.zip""")
    // compare sounds.json
    let currentSoundsJson = System.IO.File.ReadAllLines("""C:\Users\Admin1\AppData\Roaming\.minecraft\assets\objects\d1\d154dfa7a66bda3c07ac3e40cb967aa7ae0b84a0""")
    let oldSoundsJson = System.IO.File.ReadAllLines("""C:\Users\Admin1\Desktop\d154dfa7a66bda3c07ac3e40cb967aa7ae0b84a0""")
    if not(diffStringArrays(oldSoundsJson, currentSoundsJson)) then
        printfn "no sound json diff"
    *)


    let biomeSize = 3
    let custom = MC_Constants.defaultWorldWithCustomOreSpawns(biomeSize,35,25,80,false,false,false,false,TerrainAnalysisAndManipulation.oreSpawnCustom)
    //let almostDefault = MC_Constants.defaultWorldWithCustomOreSpawns(biomeSize,8,80,4,true,true,true,true,MC_Constants.oreSpawnDefaults) // biome size kept, but otherwise default
    let worldSaveFolder = """C:\Users\""" + user + """\AppData\Roaming\.minecraft\saves\RandomCTM"""
    let brianRngSeed = 0
    //dumpPlayerDat(System.IO.Path.Combine(worldSaveFolder, "level.dat"))
    CustomizationKnobs.makeMapTimeNhours(System.IO.Path.Combine(worldSaveFolder, "level.dat"), 11)
    TerrainAnalysisAndManipulation.makeCrazyMap(worldSaveFolder,brianRngSeed,custom)
    LootTables.writeAllLootTables(worldSaveFolder)
    // TODO below crashes game to embed world in one with diff level.dat ... but what does work is, gen world with options below, then copy the region files from my custom world to it
    // updateDat(System.IO.Path.Combine(worldSaveFolder, "level.dat"), (fun _pl nbt -> match nbt with |NBT.String("generatorOptions",_oldgo) -> NBT.String("generatorOptions",almostDefault) | _ -> nbt))
    System.IO.Directory.CreateDirectory(sprintf """%s\DIM-1\region\""" worldSaveFolder) |> ignore
    for x in [-1..0] do for z in [-1..0] do System.IO.File.Copy(sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\Void\region\r.%d.%d.mca""" user x z,sprintf """%s\DIM-1\region\r.%d.%d.mca""" worldSaveFolder x z, true)

    //printfn "%d" survivalObtainableItems.Length  // 516
    //makeGetAllItemsGame()

    printfn "press a key to end"
    System.Console.Beep()
    System.Console.ReadKey() |> ignore


    let worldSeed = 14 
    //System.Windows.Clipboard.SetText(custom)
    //genTerrainWithMCServer(worldSeed,custom)

    

    let readInSomeArt = false
    if readInSomeArt then
        let fil = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\BingoArt\region\r.0.0.mca"""
        let r = new RegionFile(fil)
        let arr = ResizeArray()
        let s = ArtAssets.readZoneIntoString(r,165,3,39,16,1,16)
        arr.Add(sprintf """let cw = "%s" """ s)
        let writePath = """C:\Users\Admin1\Documents\GitHubVisualStudio\minecraft-map-manipulator\MinecraftMapManipulator\ConsoleApplication1\Temp.txt"""
        System.IO.File.WriteAllLines(writePath, arr)


#if BINGO
    printfn "bingo seed is 8126030 preset to clipboard..."
    System.Windows.Clipboard.SetText(MC_Constants.defaultWorldWithCustomOreSpawns(1,100,4,80,true,true,true,true,MC_Constants.oreSpawnBingo))

    let onlyArt = false
    let save = if onlyArt then "BingoArt" else "tmp9"
    //dumpTileTicks(sprintf """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\%s\region\r.0.0.mca""" save)
    //removeAllTileTicks(sprintf """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\%s\region\r.0.0.mca""" save)
    System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Void\region\r.0.0.mca""",
                        sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\r.0.0.mca""" user save, true)
    System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Void\region\r.0.-1.mca""",
                        sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\r.0.-1.mca""" user save, true)
    System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Void\region\r.-1.0.mca""",
                        sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\r.-1.0.mca""" user save, true)
    System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Void\region\r.-1.-1.mca""",
                        sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\r.-1.-1.mca""" user save, true)
    try 
        placeCommandBlocksInTheWorld(sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\r.0.0.mca""" user save, onlyArt) 
    with e -> 
        printfn "caught exception: %s" (e.Message)
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
        System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp3\level.dat""",
                            sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\level.dat""" user save, true)
        System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp3\icon.png""",
                            sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\icon.png""" user save, true)
    //dumpSomeCommandBlocks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp9\region\r.0.0.mca""")
    //dumpSomeCommandBlocks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Seed9917 - Copy35e\region\r.0.0.mca""")
#endif

#if FUN
    placeCommandBlocksInTheWorldTemp("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\fun with clone\region\r.0.0.mca""")
#endif
    ()