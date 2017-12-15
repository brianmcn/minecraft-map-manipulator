module MinecraftBINGOExtensions

let BINGO_NS = MinecraftBINGO.NS

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
    // hook into events from base pack
    let hook(event) =
        let FIL = System.IO.Path.Combine(MinecraftBINGO.FOLDER,sprintf """datapacks\%s\data\%s\tags\functions\%s.json""" PACK_NAME BINGO_NS event)
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FIL)) |> ignore
        System.IO.File.WriteAllText(FIL,sprintf"""{"values": ["%s:%s"]}"""PACK_NS event)

    let hookEvents() =
        for event in ["on_new_card"; "on_finish"] do
            hook(event)
        for t in MinecraftBINGO.TEAMS do
            for s in MinecraftBINGO.SQUARES do
                hook(sprintf "on_%s_got_square_%s" t s)

    ///////////////////////////////

    let COVER_HEIGHT = MinecraftBINGO.ART_HEIGHT + 10
    let all_funcs = [|
        yield "on_new_card",[|
            sprintf """tellraw @a ["%s:on_new_card was called"]""" PACK_NS 
            sprintf "function %s:cover" PACK_NS
            |]
        yield "on_finish",[|
            sprintf """tellraw @a ["%s:on_finish was called"]""" PACK_NS 
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
        yield "uncover",[|
            sprintf "fill 0 %d -1 127 %d 118 air" COVER_HEIGHT COVER_HEIGHT
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
        hookEvents()
        let a = [|
            for name,code in all_funcs do
                yield! MinecraftBINGO.compile(code,name)
            |]
        for name,code in a do
            MinecraftBINGO.writeFunctionToDisk(PACK_NAME, PACK_NS, name,code)
