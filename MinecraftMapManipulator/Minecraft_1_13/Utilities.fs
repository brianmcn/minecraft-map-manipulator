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
                  "pack_format": 1,
                  "description": "%s"
               }
            }""" description
    let mcmetaFilename = System.IO.Path.Combine(worldSaveFolder, "datapacks", FOLDER, "pack.mcmeta")
    ensureDirOfFile(mcmetaFilename)
    System.IO.File.WriteAllText(mcmetaFilename, meta)

let writeFunctionTagsFileWithValues(worldSaveFolder, packName, ns, funcName, values) =     
    let FIL = System.IO.Path.Combine(worldSaveFolder, "datapacks", packName, "data", ns, sprintf """tags\functions\%s.json""" funcName)
    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FIL)) |> ignore
    let quotedVals = values |> Seq.map (fun s -> sprintf "\"%s\"" s)
    System.IO.File.WriteAllText(FIL,sprintf"""{"values": [%s]}""" (String.concat ", " quotedVals))

type DataPackArchive(worldSaveFolder, packName, description) =
    let zipFileName = System.IO.Path.Combine(worldSaveFolder, "datapacks", packName+".zip")
    let zipFileStream = new System.IO.FileStream(zipFileName, System.IO.FileMode.Create)  // will overwrite existing
    let zipArchive = new System.IO.Compression.ZipArchive(zipFileStream, System.IO.Compression.ZipArchiveMode.Create) //, false, System.Text.Encoding.ASCII)
    let mutable funcCount = 0
    do
        let mcmetaEntry = zipArchive.CreateEntry("pack.mcmeta", System.IO.Compression.CompressionLevel.NoCompression)
        use w = new System.IO.StreamWriter(mcmetaEntry.Open())
        let meta = sprintf """{ "pack": { "pack_format": 1, "description": "%s" } }""" description
        w.WriteLine(meta)
    member this.WriteFunction(ns,name,code:seq<string>) =
        funcCount <- funcCount + 1
        // NOTE: do not use System.IO.Path.Combine(), as it uses '\' as a separator, but zip-compliance requires '/'
        let path = sprintf "data/%s/functions/%s.mcfunction" ns name
        let entry = zipArchive.CreateEntry(path, System.IO.Compression.CompressionLevel.NoCompression)
        use w = new System.IO.StreamWriter(entry.Open())
        for s in code do
            w.WriteLine(s)
    member this.WriteFunctionTagsFileWithValues(ns,name,values:seq<string>) =
        let path = sprintf "data/%s/tags/functions/%s.json" ns name
        let entry = zipArchive.CreateEntry(path, System.IO.Compression.CompressionLevel.NoCompression)
        use w = new System.IO.StreamWriter(entry.Open())
        let quotedVals = values |> Seq.map (fun s -> sprintf "\"%s\"" s)
        w.WriteLine(sprintf"""{"values": [%s]}""" (String.concat ", " quotedVals))
    member this.WriteAdvancement(ns,name,fileContents:string) =
        let path = sprintf "data/%s/advancements/%s.json" ns name
        let entry = zipArchive.CreateEntry(path, System.IO.Compression.CompressionLevel.NoCompression)
        use w = new System.IO.StreamWriter(entry.Open())
        w.WriteLine(fileContents)
    member this.SaveToDisk() =
//        for e in zipArchive.Entries do
//            printfn "%s" e.FullName 
        zipArchive.Dispose()
        zipFileStream.Close()
        zipFileStream.Dispose()
//        printfn "%d functions written" funcCount

//////////////////////////////////////////////////////////////
// uuid stuff

let toLeastMost(uuid:System.Guid) =
    let bytes = uuid.ToByteArray()
    let i,j,k,a = bytes.[0..3], bytes.[4..5], bytes.[6..7], bytes.[8..15]
    let least = System.BitConverter.ToInt64(a |> Array.rev, 0)
    let most = System.BitConverter.ToInt64(Array.concat [i |> Array.rev; j |> Array.rev; k |> Array.rev] |> Array.rev, 0)
    //printfn "%d    %d" least most
    least,most

let ENTITY_UUID = "1-1-1-0-1"
let ENTITY_UUID_AS_FULL_GUID = "00000001-0001-0001-0000-000000000001"
let least,most = toLeastMost(new System.Guid(ENTITY_UUID_AS_FULL_GUID))

(*
let entity_init() = [|
    // Note: cannot summon a UUID entity in same tick you killed entity with that UUID
    yield sprintf """summon armor_stand 64 4 64 {CustomName:"\"%s\"",UUIDMost:%dl,UUIDLeast:%dl,Tags:["uuidguy"],NoGravity:1,Marker:1,Invulnerable:1,Invisible:1}""" ENTITY_UUID most least
*)

//////////////////////////////////////////////////////////////
// config options books

type ConfigDescription =
    | Toggle of string     // on-off toggleable option (value = 1 if on, 0 if off)
    | Radio of string[]    // radio button group where one choice is selected (value = index of one active)

type ConfigOption(scoreboardPrefix:string, description:ConfigDescription, defaultValue:int, extraCommandsWhenSwitched:string[]) =
    member this.ScoreboardPrefix = scoreboardPrefix
    member this.Description = description
    member this.DefaultValue = defaultValue 
    member this.ExtraCommands = extraCommandsWhenSwitched

type ConfigPage(header:string, options:ConfigOption[]) =
    member this.Header = header
    member this.Options = options

type ConfigBook(author:string, title:string, pages:ConfigPage[]) =
    member this.Author = author
    member this.Title = title
    member this.Pages = pages
    member this.FlatOptions = [|
        for page in this.Pages do
            for opt in page.Options do
                yield opt
        |]

module ConfigFunctionNames =
    let INIT = "init"
    let DEFAULT = "set_options_to_defaults"
    let LISTEN = "listen_for_triggers"
    let GET = "on_get_configuration_books"
// assumes existence of ON/OFF booktext entities, $ENTITY/$SCORE compilation entities
let writeConfigOptionsFunctions(pack:DataPackArchive,ns,folder,configBook:ConfigBook,uniqueTag,compileF,entity_selector) =
    let funcs = [|
        yield ConfigFunctionNames.INIT,[|
            for opt in configBook.FlatOptions do
                yield sprintf "scoreboard objectives add %sval dummy" opt.ScoreboardPrefix 
                yield sprintf "scoreboard objectives add %strig trigger" opt.ScoreboardPrefix 
        |]
        yield ConfigFunctionNames.DEFAULT,[|
            for opt in configBook.FlatOptions do
                yield sprintf "scoreboard players set $ENTITY %sval %d" opt.ScoreboardPrefix opt.DefaultValue
            |]
        yield ConfigFunctionNames.LISTEN,[|
            for opt in configBook.FlatOptions do
                yield sprintf "execute as @a[scores={%strig=1}] run function %s:%s/toggle_%s" opt.ScoreboardPrefix ns folder opt.ScoreboardPrefix
            |]
        for opt in configBook.FlatOptions do
            yield sprintf "toggle_%s" opt.ScoreboardPrefix, [|
                match opt.Description with
                | Toggle(desc) ->
                    yield sprintf "scoreboard players operation $ENTITY TEMP = $ENTITY %sval" opt.ScoreboardPrefix 
                    // turn off
                    yield sprintf "execute if entity $SCORE(TEMP=1) run scoreboard players set $ENTITY %sval 0" opt.ScoreboardPrefix 
                    yield sprintf """execute if entity $SCORE(TEMP=1) run tellraw @a ["turning off: %s"]""" desc
                    // turn on
                    yield sprintf "execute if entity $SCORE(TEMP=0) run scoreboard players set $ENTITY %sval 1" opt.ScoreboardPrefix
                    yield sprintf """execute if entity $SCORE(TEMP=0) run tellraw @a ["turning on: %s"]""" desc
                | Radio(descs) ->
                    yield sprintf "scoreboard players add $ENTITY %sval 1" opt.ScoreboardPrefix 
                    yield sprintf "execute if entity $SCORE(%sval=%d..) run scoreboard players set $ENTITY %sval 0" opt.ScoreboardPrefix descs.Length opt.ScoreboardPrefix 
                    for i = 0 to descs.Length-1 do
                        yield sprintf """execute if entity $SCORE(%sval=%d) run tellraw @a ["turning on: %s"]""" opt.ScoreboardPrefix i descs.[i]
                // boilerplate
                yield sprintf "scoreboard players set @s %strig 0" opt.ScoreboardPrefix 
                yield sprintf "scoreboard players enable @s %strig" opt.ScoreboardPrefix 
                // special cases
                yield! opt.ExtraCommands 
                // get a new book
                yield sprintf "function %s:%s/%s" ns folder ConfigFunctionNames.GET
                |]
        yield ConfigFunctionNames.GET,[|
            for opt in configBook.FlatOptions do
                yield sprintf "scoreboard players enable @s %strig" opt.ScoreboardPrefix 
                yield sprintf "execute if entity $SCORE(%sval=1) run scoreboard players set @e[tag=bookTextON] %sval 1" opt.ScoreboardPrefix opt.ScoreboardPrefix 
                yield sprintf "execute if entity $SCORE(%sval=0) run scoreboard players set @e[tag=bookTextON] %sval 0" opt.ScoreboardPrefix opt.ScoreboardPrefix 
                yield sprintf "execute if entity $SCORE(%sval=1) run scoreboard players set @e[tag=bookTextOFF] %sval 0" opt.ScoreboardPrefix opt.ScoreboardPrefix 
                yield sprintf "execute if entity $SCORE(%sval=0) run scoreboard players set @e[tag=bookTextOFF] %sval 1" opt.ScoreboardPrefix opt.ScoreboardPrefix 
            // Note: only one person in the world can have the config book, as we cannot keep multiple copies 'in sync'
            // TODO they could store it in a chest or item frame or something in the lobby...
            yield sprintf "clear @a minecraft:written_book{%s:1}" uniqueTag
            yield sprintf "%s" (makeCommandGivePlayerWrittenBook(configBook.Author,configBook.Title,[|
                for page in configBook.Pages do
                    yield sprintf """[{"text":"%s"}""" page.Header 
                            + String.concat "" [| for opt in page.Options do 
                                    match opt.Description  with
                                    | Toggle(desc) ->
                                        yield sprintf """,{"text":"\n\n%s...","underlined":true,"clickEvent":{"action":"run_command","value":"/trigger %strig set 1"}},{"selector":"@e[tag=bookText,scores={%sval=1}]"}""" desc opt.ScoreboardPrefix opt.ScoreboardPrefix 
                                    | Radio(descs) ->
                                        yield sprintf """,{"text":"\n\n#"},{"score":{"name":"%s","objective":"%sval"}},{"text":" below "},{"text":"(click here)","underlined":true,"clickEvent":{"action":"run_command","value":"/trigger %strig set 1"}}""" entity_selector opt.ScoreboardPrefix opt.ScoreboardPrefix
                                        for i = 0 to descs.Length-1 do
                                            yield sprintf """,{"text":"\n%d...%s"}""" i descs.[i]
                                        |]
                            + "]"
                |], sprintf "%s:1" uniqueTag))
            |]
        |]
    let funcs = [|
        for name,code in funcs do
            yield! compileF(ns,sprintf "%s/%s" folder name,code)
        |]
    for ns,name,code in funcs do
        pack.WriteFunction(ns,name,code)