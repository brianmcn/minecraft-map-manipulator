module MinecraftBINGOExtensions

let BINGO_NS = MinecraftBINGO.NS

//////////////////////////////

// TODO uncover card at end game (in book? link in chat that only works in lobby? ...)

//////////////////////////////

module Blind =
    let PACK_NAME = "blind-bingo"
    let PACK_NS   = "blind"

    let theSign() =
        // use a separate data pack (always loaded) for processing the sign to toggle the game mode
        let functions = [|
            yield "init",[|
                "scoreboard objectives add blindEnabled dummy"
                "scoreboard players add $ENTITY blindEnabled 0"
                |]
            yield "toggle_pack",[|
                sprintf "execute if $SCORE(gameInProgress=0) run function blindsign:toggle_pack_body"
                sprintf "execute unless $SCORE(gameInProgress=0) run tellraw @a [\"(this sign cannot be run while there is a game in progress)\"]"
                |]
            yield "toggle_pack_body",[|
                // prelude
                sprintf "tellraw @a [\"(game will lag as datapack is toggled, please wait)\"]"
                // body
                sprintf "scoreboard players operation $ENTITY TEMP = $ENTITY blindEnabled"
                sprintf "execute if $SCORE(blindEnabled=0) run datapack enable \"file/%s.zip\"" PACK_NAME
                sprintf "execute if $SCORE(blindEnabled=0) run scoreboard players set $ENTITY blindEnabled 1"
                sprintf "execute if $SCORE(TEMP=1) run function %s:teardown" PACK_NS
                sprintf "execute if $SCORE(TEMP=1) run scoreboard players set $ENTITY blindEnabled 0"
                // coda
                sprintf "execute if $SCORE(TEMP=0) run tellraw @a [\"Blind bingo has been enabled\"]"
                sprintf "execute if $SCORE(TEMP=1) run tellraw @a [\"Blind bingo has been disabled\"]"
                |]
            yield "put_sign_in_lobby", [|
                yield! MinecraftBINGO.placeWallSignCmds 70 26 77 "north" "toggle" "blind pack" "" "" "function blindsign:toggle_pack" true false
                |]
            |]
        let signPack = new Utilities.DataPackArchive(MinecraftBINGO.FOLDER, "blind-bingo-sign", "sign to toggle blind mode on-off")
        signPack.WriteFunctionTagsFileWithValues("minecraft","load",["blindsign:init"])
        let a = [|
            for name,code in functions do
                yield! MinecraftBINGO.compiler.Compile("blindsign",name,code)
            |]
        for ns,name,code in a do
            signPack.WriteFunction(ns,name,code)
        signPack.SaveToDisk()



    let pack = new Utilities.DataPackArchive(MinecraftBINGO.FOLDER, PACK_NAME, "MinecraftBINGO extension pack for blind play")

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
        pack.WriteFunctionTagsFileWithValues("minecraft", "tick", [sprintf "%s:tick" PACK_NS])
    let hookLoad() =
        pack.WriteFunctionTagsFileWithValues("minecraft", "load", [MinecraftBINGO.compiler.LoadFullName; sprintf "%s:init" PACK_NS])

    let hook(event) =
        pack.WriteFunctionTagsFileWithValues(BINGO_NS, event, [sprintf "%s:%s" PACK_NS event])
    let hookEvents() =
        for event in ["on_new_card"; "on_start_game"; "on_finish"; "on_get_configuration_books"] do
            hook(event)
        for t in MinecraftBINGO.TEAMS do
            for s in MinecraftBINGO.SQUARES do
                hook(sprintf "on_%s_got_square_%s" t s)

    ///////////////////////////////

    // TODO mode where an extra square is revealed after a first-time-square-got in blind game
    // TODO different sounds depending on square got, different arts per square, pixel art, etc

    let COVER_HEIGHT = MinecraftBINGO.ART_HEIGHT + 2
    let COVER_HEIGHT_UNDER = MinecraftBINGO.ART_HEIGHT_UNDER - 1

    let CFG = "cfg"
    let blindConfigBookTag = "BlindConfigBook"
    type ConfigDescription = Utilities.ConfigDescription
    type ConfigBook = Utilities.ConfigBook 
    type ConfigPage = Utilities.ConfigPage 
    type ConfigOption = Utilities.ConfigOption 
    let blindConfigBook = ConfigBook("Lorgon111","Blind options",[|
        ConfigPage("Not an option, just a clickable action",[|
            // TODO this is a nice idea, but can't actually get a config book to use it after-a-game-but-before-reset-to-start-next-game
            ConfigOption("optunc", ConfigDescription.Toggle "Uncover card now", 1, [|sprintf "function %s:uncover" PACK_NS|])
            |])
        |])
    let all_funcs = [|
        yield "teardown",[|
            // clear any blocks in the world
            sprintf "function %s:uncover" PACK_NS
            // clear any inventory in the world
            sprintf "clear @a minecraft:written_book{%s:1}" blindConfigBookTag
            // clear any entities in the world
            // disable pack
            sprintf "datapack disable \"file/%s.zip\"" PACK_NAME
            sprintf """tellraw @a ["%s:teardown was called"]""" PACK_NS 
            |]
        yield "init",[|
            sprintf "function %s:%s/%s" PACK_NS CFG Utilities.ConfigFunctionNames.INIT 
            sprintf "function %s:%s/%s" PACK_NS CFG Utilities.ConfigFunctionNames.DEFAULT
            sprintf "function %s:cover" PACK_NS
            |]
        yield "tick",[|
            sprintf "execute if $SCORE(gameInProgress=0) run function %s:config_loop" PACK_NS 
            |]
        yield "config_loop",[|
            yield sprintf "function %s:%s/%s" PACK_NS CFG Utilities.ConfigFunctionNames.LISTEN
            yield sprintf """kill @e[type=item,nbt={Item:{id:"minecraft:written_book",tag:{%s:1}}}]""" blindConfigBookTag
            |]
        yield "on_get_configuration_books",[| // base pack calls us outside CFG folder
            sprintf "function %s:%s/%s" PACK_NS CFG Utilities.ConfigFunctionNames.GET  // call our code inside CFG folder
            |]
        yield "on_new_card",[|
            //sprintf """tellraw @a ["%s:on_new_card was called"]""" PACK_NS 
            sprintf "function %s:cover" PACK_NS
            |]
        yield "on_start_game",[|
            |]
        yield "on_finish",[|
            //sprintf """tellraw @a ["%s:on_finish was called"]""" PACK_NS 
            |]
        yield "cover",[|
            // cover on top so map doesn't show
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
            // cover on bottom so can't see items on ceiling
            yield sprintf "fill 0 %d -1 127 %d 118 white_wool" COVER_HEIGHT_UNDER COVER_HEIGHT_UNDER
            // cover the chest of items, to help prevent peeking
            yield sprintf "setblock %s stone" (MinecraftBINGO.CHEST_THIS_CARD_1.Offset(0,1,0).STR)
            yield sprintf "setblock %s stone" (MinecraftBINGO.CHEST_THIS_CARD_2.Offset(0,1,0).STR)
            |]
        yield "uncover",[|
            // uncover top, bottom, and chest
            yield sprintf "fill 0 %d -1 127 %d 118 air" COVER_HEIGHT COVER_HEIGHT
            yield sprintf "fill 0 %d -1 127 %d 118 air" COVER_HEIGHT_UNDER COVER_HEIGHT_UNDER
            yield sprintf "setblock %s air" (MinecraftBINGO.CHEST_THIS_CARD_1.Offset(0,1,0).STR)
            yield sprintf "setblock %s air" (MinecraftBINGO.CHEST_THIS_CARD_2.Offset(0,1,0).STR)
            |]
        for t in MinecraftBINGO.TEAMS do
            for s in MinecraftBINGO.SQUARES do
                let x = 2 + 24*(int s.[0] - int '0' - 1)
                let y = COVER_HEIGHT
                let z = 0 + 24*(int s.[1] - int '0' - 1)
                yield sprintf "on_%s_got_square_%s" t s, [|
                    //sprintf """tellraw @a ["%s:on_%s_got_square_%s was called"]""" PACK_NS t s
                    sprintf "fill %d %d %d %d %d %d air" x y z (x+22) y (z+22)
                    |]
        |]

    ///////////////////////////////

    let main() =
        theSign()
        hookTick()
        hookEvents()
        let a = [|
            for name,code in all_funcs do
                yield! MinecraftBINGO.compiler.Compile(PACK_NS,name,code)   // TODO note that this is a way to re-use bingo's $SCORE and $ENTITY, but cannot participate in $NTICKSLATER since all continuation callbacks were already written
            |]
        for ns,name,code in a do
            pack.WriteFunction(ns,name,code)
        Utilities.writeConfigOptionsFunctions(pack, PACK_NS, CFG, blindConfigBook, blindConfigBookTag, (fun (ns,n,c) -> MinecraftBINGO.compiler.Compile(ns,n,c)), sprintf "@e[%s]" MinecraftBINGO.ENTITY_TAG)
        pack.SaveToDisk()

