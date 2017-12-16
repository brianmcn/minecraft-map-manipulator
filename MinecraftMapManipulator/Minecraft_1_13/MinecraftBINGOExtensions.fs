module MinecraftBINGOExtensions

let BINGO_NS = MinecraftBINGO.NS

let escape(s:string) = s.Replace("\"","^").Replace("\\","\\\\").Replace("^","\\\"")    //    "  \    ->    \"   \\

let writtenBookNBTString(author, title, pages:string[]) =
    let sb = System.Text.StringBuilder()
    sb.Append(sprintf "{ConfigBook:1,resolved:0b,generation:0,author:\"%s\",title:\"%s\",pages:[" author title) |> ignore
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
    sprintf "give @p minecraft:written_book%s 1" (writtenBookNBTString(author, title, pages))

//////////////////////////////

module Blind =
    let PACK_NAME = "blind-bingo"
    let PACK_NS   = "blind"

    (*

    blind play: min req's

    #on_new_card      - if blind is enabled, covers the board
    #on_got_squareNM  - if blind is enabled, uncovers the square, perhaps also plays own sound based on know config options? (fireworkItem, announceOnlyTeam, ...)
    #on_finish        - uncovers rest of card, which also means back to 'default' state if user disables the mode (mode only configurable (ideally) in gameInProgress=0)
                        except that finish is immediately followed by a new card, so, need a separate action, or the 'turn off blind' mode trigger also needs to 'uncover' everything

    option: rather than just reveal item got, could reveal it _and_ another? game evolves dynamically as you learn more about what is needed?
        also could start with N revealed?   in general, getting a revealed item does not increase number of choices, whereas getting unrevealed item is twice as much progress to revealing whole card and creates more known choices

    undecided: how to toggle it, how to display its current state

    *)

    ///////////////////////////////
    // hook into events from base packs
    let hookTick() =
        let FIL = System.IO.Path.Combine(MinecraftBINGO.FOLDER,sprintf """datapacks\%s\data\minecraft\tags\functions\tick.json""" PACK_NAME)
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FIL)) |> ignore
        System.IO.File.WriteAllText(FIL, sprintf """{"values": ["%s:tick"]}""" PACK_NS)

    let hook(event) =
        let FIL = System.IO.Path.Combine(MinecraftBINGO.FOLDER,sprintf """datapacks\%s\data\%s\tags\functions\%s.json""" PACK_NAME BINGO_NS event)
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FIL)) |> ignore
        System.IO.File.WriteAllText(FIL, sprintf """{"values": ["%s:%s"]}""" PACK_NS event)

    let hookEvents() =
        for event in ["on_new_card"; "on_start_game"; "on_finish"] do
            hook(event)
        for t in MinecraftBINGO.TEAMS do
            for s in MinecraftBINGO.SQUARES do
                hook(sprintf "on_%s_got_square_%s" t s)

    ///////////////////////////////

    let COVER_HEIGHT = MinecraftBINGO.ART_HEIGHT + 10
    let all_objectives = [|
        "thing1val"
        "thing2val"
        |]
    let all_funcs = [|
        yield "init",[|    // TODO who will run "init"? I guess the datapack enabler, ... ugh
            for o in all_objectives do
                yield sprintf "scoreboard objectives add %s dummy" o
            yield "scoreboard objectives add doThing1 trigger"
            yield "scoreboard objectives add toggleThing2 trigger"
            |]
        yield "tick",[|
            sprintf "execute as @a[scores={doThing1=1}] run function %s:do_thing1" PACK_NS
            sprintf "execute as @a[scores={toggleThing2=1}] run function %s:toggle_thing2" PACK_NS
            // TODO only run cfg checks when gameInProgress=0
            |]
        yield "toggle_thing2",[|
            "scoreboard players operation $ENTITY TEMP = $ENTITY thing2val"
            // turn off
            "execute if entity $SCORE(TEMP=1) run scoreboard players set @e[tag=bookText,name=ON] thing2val 0"
            "execute if entity $SCORE(TEMP=1) run scoreboard players set @e[tag=bookText,name=OFF] thing2val 1"
            "execute if entity $SCORE(TEMP=1) run scoreboard players set $ENTITY thing2val 0"
            "execute if entity $SCORE(TEMP=1) run say turning off thing2"
            // turn on
            "execute if entity $SCORE(TEMP=0) run scoreboard players set @e[tag=bookText,name=ON] thing2val 1"
            "execute if entity $SCORE(TEMP=0) run scoreboard players set @e[tag=bookText,name=OFF] thing2val 0"
            "execute if entity $SCORE(TEMP=0) run scoreboard players set $ENTITY thing2val 1"
            "execute if entity $SCORE(TEMP=0) run say turning on thing2"
            // boilerplate
            "scoreboard players set @s toggleThing2 0"
            "scoreboard players enable @s toggleThing2"
            sprintf "function %s:clear_and_give_book" PACK_NS
            |]
        yield "do_thing1",[|
            "scoreboard players add $ENTITY thing1val 1"
            "scoreboard players set @s doThing1 0"
            "scoreboard players enable @s doThing1"
            sprintf "function %s:clear_and_give_book" PACK_NS
            "say doing thing 1"
            |]
        yield "on_new_card",[|
            sprintf """tellraw @a ["%s:on_new_card was called"]""" PACK_NS 
            sprintf "function %s:cover" PACK_NS
            |]
        yield "on_start_game",[|
            sprintf "function %s:kill_book_text_entities" PACK_NS
            |]
        yield "on_finish",[|
            sprintf """tellraw @a ["%s:on_finish was called"]""" PACK_NS 
            sprintf "function %s:summon_book_text_entities" PACK_NS
            // TODO would also need to summon them in pack-enable and kill in pack-disable
            |]
        yield "kill_book_text_entities",[|
            "kill @e[tag=bookText]"
            |]
        yield "summon_book_text_entities",[|
            """summon armor_stand 37 1 37 {Invulnerable:1b,Invisible:1b,NoGravity:1b,Tags:["bookText"],CustomName:ON,Team:green}"""
            """summon armor_stand 37 1 37 {Invulnerable:1b,Invisible:1b,NoGravity:1b,Tags:["bookText"],CustomName:OFF,Team:red}"""
            |]
        yield "cover",[|
            yield sprintf "fill 0 %d -1 127 %d 118 white_wool" COVER_HEIGHT COVER_HEIGHT
            // horizontal gridlines
            yield sprintf "fill 1 %d 023 121 %d 023 stone" COVER_HEIGHT COVER_HEIGHT
            yield sprintf "fill 1 %d 047 121 %d 047 stone" COVER_HEIGHT COVER_HEIGHT
            yield sprintf "fill 1 %d 071 121 %d 071 stone" COVER_HEIGHT COVER_HEIGHT
            yield sprintf "fill 1 %d 095 121 %d 095 stone" COVER_HEIGHT COVER_HEIGHT
            yield sprintf "fill 0 %d 119 127 %d 119 stone" COVER_HEIGHT COVER_HEIGHT
            // vertical gridlines
            yield sprintf "fill 001 %d 0 001 %d 118 stone" COVER_HEIGHT COVER_HEIGHT
            yield sprintf "fill 025 %d 0 025 %d 118 stone" COVER_HEIGHT COVER_HEIGHT
            yield sprintf "fill 049 %d 0 049 %d 118 stone" COVER_HEIGHT COVER_HEIGHT
            yield sprintf "fill 073 %d 0 073 %d 118 stone" COVER_HEIGHT COVER_HEIGHT
            yield sprintf "fill 097 %d 0 097 %d 118 stone" COVER_HEIGHT COVER_HEIGHT
            yield sprintf "fill 121 %d 0 127 %d 118 stone" COVER_HEIGHT COVER_HEIGHT
            |]
        yield "clear_and_give_book",[|
            "clear @a minecraft:written_book{ConfigBook:1}"  // TODO kill any book items that were dropped on the floor, so no stale copies anywhere?
            sprintf "%s" (makeCommandGivePlayerWrittenBook("Lorgon111","The title",[|
                "["
                    + sprintf """{"score":{"name":"@e[%s]","objective":"thing1val"}},{"text":" click me","underlined":true,"clickEvent":{"action":"run_command","value":"/trigger doThing1 set 1"}}""" MinecraftBINGO.ENTITY_TAG
                    + sprintf """,{"text":"\n\ntoggle thing2...","underlined":true,"clickEvent":{"action":"run_command","value":"/trigger toggleThing2 set 1"}},{"selector":"@e[tag=bookText,scores={thing2val=1}]"}"""
                    + "]"
                |]))
            |]
        yield "uncover",[|
            sprintf "fill 0 %d -1 127 %d 118 air" COVER_HEIGHT COVER_HEIGHT
            "scoreboard players enable @a doThing1"
            "scoreboard players enable @a toggleThing2"
            sprintf "function %s:clear_and_give_book" PACK_NS
            |]
        for t in MinecraftBINGO.TEAMS do
            for s in MinecraftBINGO.SQUARES do
                let x = 2 + 24*(int s.[0] - int '0' - 1)
                let y = COVER_HEIGHT
                let z = 0 + 24*(int s.[1] - int '0' - 1)
                yield sprintf "on_%s_got_square_%s" t s, [|
                    sprintf """tellraw @a ["%s:on_%s_got_square_%s was called"]""" PACK_NS t s
                    sprintf "fill %d %d %d %d %d %d air" x y z (x+22) y (z+22)
                    |]
        |]

    ///////////////////////////////

    let main() =
        MinecraftBINGO.writeDatapackMeta(PACK_NAME, "MinecraftBINGO extension pack for blind play")
        hookTick()
        hookEvents()
        let a = [|
            for name,code in all_funcs do
                yield! MinecraftBINGO.compile(code,name)
            |]
        for name,code in a do
            MinecraftBINGO.writeFunctionToDisk(PACK_NAME, PACK_NS, name,code)
