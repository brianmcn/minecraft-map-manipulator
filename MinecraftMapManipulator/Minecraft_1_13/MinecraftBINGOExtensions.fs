module MinecraftBINGOExtensions

let BINGO_NS = MinecraftBINGO.NS

//////////////////////////////
// utilities

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
    sprintf "give @s minecraft:written_book%s 1" (writtenBookNBTString(author, title, pages))

//////////////////////////////

(* 
TODO how will a player enable/disable datapacks?
seems the only thing a player can do is interact with signs
so probably a datapack author needs hand out a command that an OP'd player can run to get a sign to toggle the pack
the sign, when clicked, would have to... ONLY if gameInProgress=0...
 - enable its pack (if it's not enabled)
 - see if the current state of the whole pack is ON or OFF
 - if ON, set scoreboard to OFF, run any cleanup commands, then disable pack
 - if OFF, set scoreboard to ON, and then run any startup/init for the pack

so the sign has 2 commands
 - datapack enable file/pack-name
 - function packns:toggle_pack
and its text should say toggle (so it can be fixed text; data merge Text1 does not update client)

a common sign in the lobby (part of base bingo) should broadcast #on_give_config_books or something when players want your config options

*)

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
        for event in ["on_new_card"; "on_start_game"; "on_finish"; "on_get_configuration_books"] do
            hook(event)
        for t in MinecraftBINGO.TEAMS do
            for s in MinecraftBINGO.SQUARES do
                hook(sprintf "on_%s_got_square_%s" t s)

    ///////////////////////////////

    let COVER_HEIGHT = MinecraftBINGO.ART_HEIGHT + 4
    let toggleableOptions = [|
        "thing5", "Some option"
        "thing6", "Another option"
        "thing7", "Great option"
        |]
    let toggleables = toggleableOptions |> Array.map fst
    let all_objectives = [|
        yield "blindPackInited"   // has the pack ever been initialized
        yield "blindPackEnabled"  // is the pack currently enabled
        for t in toggleables do
            yield "val"+t
        |]
    let all_funcs = [|
        yield "get_sign_for_lobby", [|
            let prefix0 = sprintf "execute if entity @e[%s,scores={gameInProgress=0}] run " MinecraftBINGO.ENTITY_TAG 
            let prefix1 = sprintf "execute unless entity @e[%s,scores={gameInProgress=0}] run " MinecraftBINGO.ENTITY_TAG 
            yield sprintf 
                """give @s sign{BlockEntityTag:{Text1:"[%s]",Text2:"[%s]",Text3:"[%s]",Text4:"[%s]"}} 1"""
                (escape(sprintf     """{"text":"toggle","clickEvent":{"action":"run_command","value":"%stellraw @a [\"(game will lag as datapack is toggled, please wait)\"]"}}""" prefix0))
                (escape(sprintf """{"text":"blind pack","clickEvent":{"action":"run_command","value":"%sdatapack enable \"file/%s\""}}""" prefix0 PACK_NAME))
                (escape(sprintf           """{"text":"","clickEvent":{"action":"run_command","value":"%sfunction %s:toggle_pack"}}""" prefix0 PACK_NS))
                (escape(sprintf           """{"text":"","clickEvent":{"action":"run_command","value":"%stellraw @a [\"(this sign cannot be run while there is a game in progress)\"]"}}""" prefix1))
            |]
        yield "toggle_pack", [|
            // deal with first-time initialization
            sprintf "execute unless entity $SCORE(blindPackInited=1) run function %s:init" PACK_NS    // TODO during development, will need to manually run init or unset this flag
            sprintf "scoreboard players set $ENTITY blindPackInited 1"
            // cache value
            sprintf "scoreboard players operation $ENTITY TEMP = $ENTITY blindPackEnabled"
            // turn on
            sprintf "execute if entity $SCORE(TEMP=0) run scoreboard players set $ENTITY blindPackEnabled 1"
            sprintf "execute if entity $SCORE(TEMP=0) run function %s:startup" PACK_NS 
            sprintf """execute if entity $SCORE(TEMP=0) run tellraw @a ["enabled covered gameplay from %s"]""" PACK_NAME
            // turn off
            sprintf "execute if entity $SCORE(TEMP=1) run scoreboard players set $ENTITY blindPackEnabled 0"
            sprintf "execute if entity $SCORE(TEMP=1) run function %s:teardown" PACK_NS 
            sprintf """execute if entity $SCORE(TEMP=1) run tellraw @a ["disabled covered gameplay from %s"]""" PACK_NAME
            |]
        yield "startup",[|
            sprintf "function %s:cover" PACK_NS
            sprintf "function %s:summon_book_text_entities" PACK_NS
            |]
        yield "teardown",[|
            // clear any blocks in the world
            sprintf "function %s:uncover" PACK_NS
            // clear any inventory in the world
            "clear @a minecraft:written_book{ConfigBook:1}"
            // clear any entities in the world
            sprintf "function %s:kill_book_text_entities" PACK_NS
            sprintf """datapack disable "file/%s" """ PACK_NAME
            |]
        yield "on_get_configuration_books",[|
            for t in toggleables do
                yield sprintf "scoreboard players enable @s trig%s" t
            yield sprintf "function %s:clear_and_give_book" PACK_NS
            |]
        yield "init",[|
            for o in all_objectives do
                yield sprintf "scoreboard objectives add %s dummy" o
            for t in toggleables do
                yield sprintf "scoreboard objectives add trig%s trigger" t
            // set any default values
            yield "scoreboard players set $ENTITY blindPackEnabled 0"
            |]
        yield "tick",[|
            sprintf "execute if entity $SCORE(gameInProgress=0) run function %s:config_loop" PACK_NS 
            |]
        yield "config_loop",[|
            for t in toggleables do
                yield sprintf "execute as @a[scores={trig%s=1}] run function %s:toggle_%s" t PACK_NS t
            yield """kill @e[type=item,nbt={Item:{id:"minecraft:written_book",tag:{ConfigBook:1}}}]"""
            |]
        for t,name in toggleableOptions do
            yield sprintf "toggle_%s" t, [|
                sprintf "scoreboard players operation $ENTITY TEMP = $ENTITY val%s" t
                // turn off
                sprintf "execute if entity $SCORE(TEMP=1) run scoreboard players set @e[tag=bookText,name=ON] val%s 0" t
                sprintf "execute if entity $SCORE(TEMP=1) run scoreboard players set @e[tag=bookText,name=OFF] val%s 1" t
                sprintf "execute if entity $SCORE(TEMP=1) run scoreboard players set $ENTITY val%s 0" t
                sprintf """execute if entity $SCORE(TEMP=1) run tellraw @a ["turning off: %s"]""" name
                // turn on
                sprintf "execute if entity $SCORE(TEMP=0) run scoreboard players set @e[tag=bookText,name=ON] val%s 1" t
                sprintf "execute if entity $SCORE(TEMP=0) run scoreboard players set @e[tag=bookText,name=OFF] val%s 0" t
                sprintf "execute if entity $SCORE(TEMP=0) run scoreboard players set $ENTITY val%s 1" t
                sprintf """execute if entity $SCORE(TEMP=0) run tellraw @a ["turning on: %s"]""" name
                // boilerplate
                sprintf "scoreboard players set @s trig%s 0" t
                sprintf "scoreboard players enable @s trig%s" t
                sprintf "function %s:clear_and_give_book" PACK_NS
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
            |]
        yield "kill_book_text_entities",[|
            "kill @e[tag=bookText]"
            |]
        yield "summon_book_text_entities",[|
            """summon armor_stand 37 1 37 {Invulnerable:1b,Invisible:1b,NoGravity:1b,Tags:["bookText"],CustomName:ON,Team:green}"""
            """summon armor_stand 37 1 37 {Invulnerable:1b,Invisible:1b,NoGravity:1b,Tags:["bookText"],CustomName:OFF,Team:red}"""
            |]
        // TODO cover and uncover lag a lot due to lighting updates; change bingo art to non-transparent, with full bottom-y thickness? 
        // leaves are only non-opaque block, there is no replacement for them, so... hm, put green wool under? hm...
        // something like clone bottom layer, clone top layer non-air, replace leaves with green wool, profit?
        // TODO problem is my leaves are decayable:true in the structures, need to fix them all
        (*
clone 0 40 0 127 40 118 0 39 0
fill 0 39 0 127 39 118 minecraft:green_wool replace minecraft:oak_leaves
clone 0 41 0 127 41 118 0 39 0 masked
fill 0 39 0 127 39 118 minecraft:green_wool replace minecraft:oak_leaves
        *)
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
            // Note: only one person in the world can have the config book, as we cannot keep multiple copies 'in sync'
            "clear @a minecraft:written_book{ConfigBook:1}"
            sprintf "%s" (makeCommandGivePlayerWrittenBook("Lorgon111","The title",[|
                """[{"text":"Some descriptive header"}"""
                    + String.concat "" [| for t,name in toggleableOptions do 
                            yield sprintf """,{"text":"\n\n%s...","underlined":true,"clickEvent":{"action":"run_command","value":"/trigger trig%s set 1"}},{"selector":"@e[tag=bookText,scores={val%s=1}]"}""" name t t
                        |]
                    + "]"
                |]))
            |]
        yield "uncover",[|
            yield sprintf "fill 0 %d -1 127 %d 118 air" COVER_HEIGHT COVER_HEIGHT
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
