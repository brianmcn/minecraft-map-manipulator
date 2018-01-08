module Utilities

//let MC_ROOT = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\"""
let MC_ROOT = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\"""

//////////////////////////////
// written books

let escape(s:string) = s.Replace("\"","º").Replace("\\","\\\\").Replace("º","\\\"")    //    "  \    ->    \"   \\

let writtenBookNBTString(author, title, pages:string[], extraItemNBT) =
    let sb = System.Text.StringBuilder()
    sb.Append(sprintf "{%sresolved:0b,generation:0,author:\"%s\",title:\"%s\",pages:[" (if extraItemNBT<> null then extraItemNBT+"," else "") author title ) |> ignore
    for i = 0 to pages.Length-2 do
        sb.Append("\"") |> ignore
        sb.Append(escape pages.[i]) |> ignore
        sb.Append("\",") |> ignore
    sb.Append("\"") |> ignore
    sb.Append(escape pages.[pages.Length-1]) |> ignore
    sb.Append("\"") |> ignore
    sb.Append("]}") |> ignore
    sb.ToString()

let makeCommandGivePlayerWrittenBook(author, title, pages:string[], extraItemNBT) =
    sprintf "give @s minecraft:written_book%s 1" (writtenBookNBTString(author, title, pages, extraItemNBT))

//////////////////////////////
// disk utils

let ensureDirOfFile(filename) = System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename)) |> ignore

let allDirsEnsured = new System.Collections.Generic.HashSet<_>()
let writeFunctionToDisk(worldSaveFolder, packName, packNS, name, code) =
    let DIR = System.IO.Path.Combine(worldSaveFolder, sprintf """datapacks\%s\data\%s\functions""" packName packNS)
    let FIL = System.IO.Path.Combine(DIR,sprintf "%s.mcfunction" name)
    let dir = System.IO.Path.GetDirectoryName(FIL)
    if allDirsEnsured.Add(dir) then
        System.IO.Directory.CreateDirectory(dir) |> ignore
    System.IO.File.WriteAllLines(FIL, code)

let writeDatapackMeta(worldSaveFolder, packName,description) =
    let FOLDER = packName
    let meta = sprintf """{
               "pack": {
                  "pack_format": 4,
                  "description": "%s"
               }
            }""" description
    let mcmetaFilename = System.IO.Path.Combine(worldSaveFolder, "datapacks", FOLDER, "pack.mcmeta")
    ensureDirOfFile(mcmetaFilename)
    System.IO.File.WriteAllText(mcmetaFilename, meta)
        
