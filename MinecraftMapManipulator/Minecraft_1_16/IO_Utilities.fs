module IO_Utilities

//////////////////////////////
// disk utils

let ensureDirOfFile(filename) = System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename)) |> ignore

let allDirsEnsured = new System.Collections.Generic.HashSet<_>()
let private writeFunctionToDisk(worldSaveFolder, packName, packNS, name, code) =
    let DIR = System.IO.Path.Combine(worldSaveFolder, sprintf """datapacks\%s\data\%s\functions""" packName packNS)
    let FIL = System.IO.Path.Combine(DIR,sprintf "%s.mcfunction" name)
    let dir = System.IO.Path.GetDirectoryName(FIL)
    if allDirsEnsured.Add(dir) then
        System.IO.Directory.CreateDirectory(dir) |> ignore
    System.IO.File.WriteAllLines(FIL, code)

let private writeDatapackMeta(worldSaveFolder, packName, description) =
    let FOLDER = packName
    let meta = sprintf """{
               "pack": {
                  "pack_format": 3,
                  "description": "%s"
               }
            }""" description
    let mcmetaFilename = System.IO.Path.Combine(worldSaveFolder, "datapacks", FOLDER, "pack.mcmeta")
    ensureDirOfFile(mcmetaFilename)
    System.IO.File.WriteAllText(mcmetaFilename, meta)

let private writeFunctionTagsFileWithValues(worldSaveFolder, packName, ns, funcName, values) =     
    let FIL = System.IO.Path.Combine(worldSaveFolder, "datapacks", packName, "data", ns, sprintf """tags\functions\%s.json""" funcName)
    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FIL)) |> ignore
    let quotedVals = values |> Seq.map (fun s -> sprintf "\"%s\"" s)
    System.IO.File.WriteAllText(FIL,sprintf"""{"values": [%s]}""" (String.concat ", " quotedVals))

type DataPackArchive(worldSaveFolder, packName, description) =
    let zipFileName = System.IO.Path.Combine(worldSaveFolder, "datapacks", packName+".zip")
    let zipFileStream = new System.IO.FileStream(zipFileName, System.IO.FileMode.Create)  // will overwrite existing
    let zipArchive = new System.IO.Compression.ZipArchive(zipFileStream, System.IO.Compression.ZipArchiveMode.Create) //, false, System.Text.Encoding.ASCII)
    let paths = new System.Collections.Generic.HashSet<string>()
    let mutable funcCount = 0
    let mutable lineCount = 0
    let validate(ns:string,name:string) =
        if ns.ToLowerInvariant() <> ns then failwithf "bad ns: %s" ns
        if name.ToLowerInvariant() <> name then failwithf "bad name: %s" name
    do
        let mcmetaEntry = zipArchive.CreateEntry("pack.mcmeta", System.IO.Compression.CompressionLevel.NoCompression)
        use w = new System.IO.StreamWriter(mcmetaEntry.Open())
        let meta = sprintf """{ "pack": { "pack_format": 1, "description": "%s" } }""" description
        w.WriteLine(meta)
    member this.WriteFunction(ns,name,code:seq<string>) =
        validate(ns,name)
        funcCount <- funcCount + 1
        lineCount <- lineCount + Seq.length code
        // NOTE: do not use System.IO.Path.Combine(), as it uses '\' as a separator, but zip-compliance requires '/'
        let path = sprintf "data/%s/functions/%s.mcfunction" ns name
        if not(paths.Add(path)) then failwithf "already added entry to ZipArchive: %s" path
        let entry = zipArchive.CreateEntry(path, System.IO.Compression.CompressionLevel.NoCompression)
        use w = new System.IO.StreamWriter(entry.Open())
        for s in code do
            w.WriteLine(s)
    member this.WriteBlocksTagsFileWithValues(ns,name,values:seq<string>) =
        validate(ns,name)
        let path = sprintf "data/%s/tags/blocks/%s.json" ns name
        if not(paths.Add(path)) then failwithf "already added entry to ZipArchive: %s" path
        let entry = zipArchive.CreateEntry(path, System.IO.Compression.CompressionLevel.NoCompression)
        use w = new System.IO.StreamWriter(entry.Open())
        let quotedVals = values |> Seq.map (fun s -> sprintf "\"%s\"" s)
        w.WriteLine(sprintf"""{"values": [%s]}""" (String.concat ", " quotedVals))
    member this.WriteFunctionTagsFileWithValues(ns,name,values:seq<string>) =
        validate(ns,name)
        let path = sprintf "data/%s/tags/functions/%s.json" ns name
        if not(paths.Add(path)) then failwithf "already added entry to ZipArchive: %s" path
        let entry = zipArchive.CreateEntry(path, System.IO.Compression.CompressionLevel.NoCompression)
        use w = new System.IO.StreamWriter(entry.Open())
        let quotedVals = values |> Seq.map (fun s -> sprintf "\"%s\"" s)
        w.WriteLine(sprintf"""{"values": [%s]}""" (String.concat ", " quotedVals))
    member this.WriteAdvancement(ns,name,fileContents:string) =
        validate(ns,name)
        let path = sprintf "data/%s/advancements/%s.json" ns name
        if not(paths.Add(path)) then failwithf "already added entry to ZipArchive: %s" path
        let entry = zipArchive.CreateEntry(path, System.IO.Compression.CompressionLevel.NoCompression)
        use w = new System.IO.StreamWriter(entry.Open())
        w.WriteLine(fileContents)
    member this.WriteRecipe(ns,name,fileContents:string) =
        validate(ns,name)
        let path = sprintf "data/%s/recipes/%s.json" ns name
        if not(paths.Add(path)) then failwithf "already added entry to ZipArchive: %s" path
        let entry = zipArchive.CreateEntry(path, System.IO.Compression.CompressionLevel.NoCompression)
        use w = new System.IO.StreamWriter(entry.Open())
        w.WriteLine(fileContents)
    member this.SaveToDisk() =
//        for e in zipArchive.Entries do
//            printfn "%s" e.FullName 
        zipArchive.Dispose()
        zipFileStream.Close()
        zipFileStream.Dispose()
        printfn "%d functions written, %d lines of code" funcCount lineCount
