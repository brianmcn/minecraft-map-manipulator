module MinecraftBINGOExtensions

(*

blind play: min req's

#on_new_card      - if blind is enabled, covers the board
#on_got_squareNM  - if blind is enabled, uncovers the square, perhaps also plays own sound based on know config options? (fireworkItem, announceOnlyTeam, ...)
#on_finish        - uncovers rest of card, which also means back to 'default' state if user disables the mode (mode only configurable (ideally) in gameInProgress=0)

option: rather than just reveal item got, could reveal it _and_ another? game evolves dynamically as you learn more about what is needed?
    also could start with N revealed?   in general, getting a revealed item does not increase number of choices, whereas getting unrevealed item is twice as much progress to revealing whole card and creates more known choices

undecided: how to toggle it, how to display its current state

*)

let BINGO_NS = MinecraftBINGO.NS

module Blind =
    let PACK_NAME = "blind-bingo"
    let PACK_NS   = "blind"

    ///////////////////////////////
    // hook into events from base pack
    let FIL = System.IO.Path.Combine(MinecraftBINGO.FOLDER,sprintf """datapacks\%s\data\%s\tags\functions\on_new_card.json""" PACK_NAME BINGO_NS)
    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FIL)) |> ignore
    System.IO.File.WriteAllText(FIL,sprintf"""{"values": ["%s:on_new_card"]}"""PACK_NS)

    ///////////////////////////////

    let on_new_card = "on_new_card",[|
        sprintf """tellraw @a ["%s:on_new_card was called"]""" PACK_NS 
        |]

    ///////////////////////////////

    let main() =
        MinecraftBINGO.writeDatapackMeta(PACK_NAME, "MinecraftBINGO extension pack for blind play")
        let a = [|
            for name,code in [| on_new_card |] do
                yield! MinecraftBINGO.compile(code,name)
            |]
        for name,code in a do
            MinecraftBINGO.writeFunctionToDisk(PACK_NAME, PACK_NS, name,code)
