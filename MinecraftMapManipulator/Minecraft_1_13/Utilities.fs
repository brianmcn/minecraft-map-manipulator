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

let writeFunctionTagsFileWithValues(worldSaveFolder, packName, ns, funcName, values) =     
    let FIL = System.IO.Path.Combine(worldSaveFolder, "datapacks", packName, "data", ns, sprintf """tags\functions\%s.json""" funcName)
    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FIL)) |> ignore
    let quotedVals = values |> Seq.map (fun s -> sprintf "\"%s\"" s)
    System.IO.File.WriteAllText(FIL,sprintf"""{"values": [%s]}""" (String.concat ", " quotedVals))

//////////////////////////////////////////////////////////////
// config options books

// todo add radio button

type ConfigOption(scoreboardPrefix:string, description:string, defaultsOn:bool, extraCommandsWhenToggled:string[]) =
    member this.ScoreboardPrefix = scoreboardPrefix
    member this.Description = description
    member this.DefaultValue = if defaultsOn then 1 else 0
    member this.ExtraCommands = extraCommandsWhenToggled

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
let writeConfigOptionsFunctions(world,pack,ns,folder,configBook:ConfigBook,uniqueTag,compileF) =
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
                yield sprintf "scoreboard players operation $ENTITY TEMP = $ENTITY %sval" opt.ScoreboardPrefix 
                // turn off
                yield sprintf "execute if entity $SCORE(TEMP=1) run scoreboard players set $ENTITY %sval 0" opt.ScoreboardPrefix 
                yield sprintf """execute if entity $SCORE(TEMP=1) run tellraw @a ["turning off: %s"]""" opt.Description 
                // turn on
                yield sprintf "execute if entity $SCORE(TEMP=0) run scoreboard players set $ENTITY %sval 1" opt.ScoreboardPrefix
                yield sprintf """execute if entity $SCORE(TEMP=0) run tellraw @a ["turning on: %s"]""" opt.Description 
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
                                    yield sprintf """,{"text":"\n\n%s...","underlined":true,"clickEvent":{"action":"run_command","value":"/trigger %strig set 1"}},{"selector":"@e[tag=bookText,scores={%sval=1}]"}""" opt.Description opt.ScoreboardPrefix opt.ScoreboardPrefix 
                                        |]
                            + "]"
                |], sprintf "%s:1" uniqueTag))
            |]
        |]
    let funcs = [|
        for name,code in funcs do
            yield! compileF(name,code)
        |]
    for name,code in funcs do
        writeFunctionToDisk(world, pack, ns, sprintf "%s/%s" folder name, code)