﻿module Utilities

//let MC_ROOT = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\"""
//let MC_ROOT = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\"""
let MC_ROOT = """C:\Users\Admin1\AppData\Roaming\.minecraft-for-c2\saves\"""

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
// signs
let placeWallSignCmds x y z facing txt1 txt2 txt3 txt4 cmd isBold color executePrefix =
    if facing<>"north" && facing<>"south" && facing<>"east" && facing<>"west" then failwith "bad facing wall_sign"
    let bc = sprintf """,\"bold\":\"%s\",\"color\":\"%s\" """ (if isBold then "true" else "false") color
    let c1 = if isBold && (cmd<>null) then sprintf """,\"clickEvent\":{\"action\":\"run_command\",\"value\":\"%s\"} """ cmd else ""
    [|
        sprintf "setblock %d %d %d air replace" x y z
        sprintf """%ssetblock %d %d %d wall_sign[facing=%s]{Text1:"{\"text\":\"%s\"%s%s}",Text2:"{\"text\":\"%s\"%s}",Text3:"{\"text\":\"%s\"%s}",Text4:"{\"text\":\"%s\"%s}"}""" 
                    executePrefix x y z facing txt1 bc c1 txt2 bc txt3 bc txt4 bc
    |]

//////////////////////////////
// tellraw

let tellrawScoreSelectorENTITY(objective) = sprintf """{"score":{"name":"FAKE","objective":"%s"}}""" objective           // TODO factor FAKE constant from compiler
let tellrawScoreSelector(name,objective) = sprintf """{"score":{"name":"%s","objective":"%s"}}""" name objective

let CARDINALS = [|
    "@s[y_rotation=-157..-112]","ne"
    "@s[y_rotation=-112..-67]","e"
    "@s[y_rotation=-67..-22]","se"
    "@s[y_rotation=-22..22]","s"
    "@s[y_rotation=22..67]","sw"
    "@s[y_rotation=67..112]","w"
    "@s[y_rotation=112..157]","nw"
    "@s[y_rotation=157..179]","n"
    "@s[y_rotation=-180..-157]","n"
    |]

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
                  "pack_format": 1,
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
    | Clickable of string  // not a stateful option, just clickable ConfigOption text that runs ExtraCommands

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
let writeConfigOptionsFunctions(pack:DataPackArchive,ns,folder,configBook:ConfigBook,uniqueTag,compileF,tellrawScoreName) =
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
                    yield sprintf "execute if $SCORE(TEMP=1) run scoreboard players set $ENTITY %sval 0" opt.ScoreboardPrefix 
                    yield sprintf """execute if $SCORE(TEMP=1) run tellraw @a ["turning off: %s"]""" desc
                    // turn on
                    yield sprintf "execute if $SCORE(TEMP=0) run scoreboard players set $ENTITY %sval 1" opt.ScoreboardPrefix
                    yield sprintf """execute if $SCORE(TEMP=0) run tellraw @a ["turning on: %s"]""" desc
                | Radio(descs) ->
                    yield sprintf "scoreboard players add $ENTITY %sval 1" opt.ScoreboardPrefix 
                    yield sprintf "execute if $SCORE(%sval=%d..) run scoreboard players set $ENTITY %sval 0" opt.ScoreboardPrefix descs.Length opt.ScoreboardPrefix 
                    for i = 0 to descs.Length-1 do
                        yield sprintf """execute if $SCORE(%sval=%d) run tellraw @a ["turning on: %s"]""" opt.ScoreboardPrefix i descs.[i]
                | Clickable(_desc) -> ()
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
                yield sprintf "execute if $SCORE(%sval=1) run scoreboard players set @e[tag=bookTextON] %sval 1" opt.ScoreboardPrefix opt.ScoreboardPrefix 
                yield sprintf "execute if $SCORE(%sval=0) run scoreboard players set @e[tag=bookTextON] %sval 0" opt.ScoreboardPrefix opt.ScoreboardPrefix 
                yield sprintf "execute if $SCORE(%sval=1) run scoreboard players set @e[tag=bookTextOFF] %sval 0" opt.ScoreboardPrefix opt.ScoreboardPrefix 
                yield sprintf "execute if $SCORE(%sval=0) run scoreboard players set @e[tag=bookTextOFF] %sval 1" opt.ScoreboardPrefix opt.ScoreboardPrefix 
            // Note: only one person in the world can have the config book, as we cannot keep multiple copies 'in sync'
            // todo they could store it in a chest or item frame or something in the lobby...
            yield sprintf "clear @a minecraft:written_book{%s:1}" uniqueTag
            yield sprintf "%s" (makeCommandGivePlayerWrittenBook(configBook.Author,configBook.Title,[|
                for page in configBook.Pages do
                    yield sprintf """[{"text":"%s"}""" page.Header 
                            + String.concat "" [| for opt in page.Options do 
                                    match opt.Description  with
                                    | Toggle(desc) ->
                                        yield sprintf """,{"text":"\n\n%s...","underlined":true,"clickEvent":{"action":"run_command","value":"/trigger %strig set 1"}},{"selector":"@e[tag=bookText,scores={%sval=1}]"}""" desc opt.ScoreboardPrefix opt.ScoreboardPrefix 
                                    | Radio(descs) ->
                                        yield sprintf """,{"text":"\n\n#"},{"score":{"name":"%s","objective":"%sval"}},{"text":" below "},{"text":"(click here)","underlined":true,"clickEvent":{"action":"run_command","value":"/trigger %strig set 1"}}""" tellrawScoreName opt.ScoreboardPrefix opt.ScoreboardPrefix
                                        for i = 0 to descs.Length-1 do
                                            yield sprintf """,{"text":"\n%d...%s"}""" i descs.[i]
                                    | Clickable(desc) ->
                                        yield sprintf """,{"text":"\n\n%s","underlined":true,"clickEvent":{"action":"run_command","value":"/trigger %strig set 1"}}""" desc opt.ScoreboardPrefix
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

// binaryLookup("ma", "findPhi", "look", 7, 3, -180) 
// looks at @e[tag=look] and binary searches 2^7 in steps of 3 at offset -180, so e.g. [-180..-178] and [-177..-175] are bottom buckets
// cmf is a func of a value (like 173) that returns a command string to run if score was that value
let binaryLookup(ns, prefix, objective, exp, k, offset, cmdf) =
    let mutable n = 1
    for _i = 1 to exp do
        n <- n * 2
    let functions = ResizeArray()
    let makeName(lo,hi) = sprintf "%s/do%dto%d" prefix lo hi
    let rec go(lo,hi) =
        let name = makeName(lo,hi)
        if hi-lo < k then
            functions.Add(name,[|
                for i = lo to hi do
                    yield cmdf i
                |])
        else
            let mid = (hi-lo)/2 + lo
            let midn = mid+1
            functions.Add(name,[|
                yield sprintf "execute if entity @s[scores={%s=%d..%d}] run function %s:%s" objective lo mid ns (makeName(lo,mid))
                yield sprintf "execute if entity @s[scores={%s=%d..%d}] run function %s:%s" objective midn hi ns (makeName(midn,hi))
                |])
            go(lo,mid)
            go(midn,hi)
    go(offset,offset+n*k)
    functions.Add(prefix,[|
        sprintf "function %s:%s" ns (makeName(offset,offset+n*k))
        |])
    functions
