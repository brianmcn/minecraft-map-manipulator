module AA = ArtAssets

open MC_Constants
open NBT_Manipulation
open RegionFiles
open Utilities

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    

/////////////////////////////

let mutable signZ = 0

let placeCommandBlocksInTheWorld(fil) =
    let region = new RegionFile(fil)
#if AWESOME_CONWAY_LIFE
    let DURATION = 999999
    let entityType = "AreaEffectCloud"
    let entityDefaults = sprintf ",Duration:%d" DURATION
    let nearbys = [| "~-1 ~ ~-1"; "~0 ~ ~-1"; "~1 ~ ~-1"; "~-1 ~ ~0"; "~1 ~ ~0"; "~-1 ~ ~1"; "~0 ~ ~1"; "~1 ~ ~1" |]
    let cmds = 
        [|
            yield P ""
            for i = 0 to 7 do
                let nearby = nearbys.[i]
                if i >= 3 then
                    yield U (sprintf "execute @e[tag=live] %s fill ~ ~-1 ~ ~ ~-1 ~ wool 4 replace wool 3" nearby)
                if i >= 2 then
                    yield U (sprintf "execute @e[tag=live] %s fill ~ ~-1 ~ ~ ~-1 ~ wool 3 replace wool 2" nearby)
                if i >= 1 then
                    yield U (sprintf "execute @e[tag=live] %s fill ~ ~-1 ~ ~ ~-1 ~ wool 2 replace wool 1" nearby)
                yield U (sprintf "execute @e[tag=live] %s fill ~ ~-1 ~ ~ ~-1 ~ wool 1 replace wool 0" nearby)
            yield U (sprintf "execute @e[tag=live] ~ ~ ~ detect ~ ~-1 ~ wool 2 summon %s ~ ~ ~ {Tags:[\"keep\"]%s}" entityType entityDefaults)
            yield U (sprintf "execute @e[tag=live] ~ ~ ~ detect ~ ~-1 ~ wool 3 summon %s ~ ~ ~ {Tags:[\"keep\"]%s}" entityType entityDefaults)
            yield U "execute @e[tag=live] ~ ~ ~ detect ~ ~-1 ~ wool 0 setblock ~ ~ ~ wool 0"
            yield U "execute @e[tag=live] ~ ~ ~ detect ~ ~-1 ~ wool 1 setblock ~ ~ ~ wool 0"
            yield U "execute @e[tag=live] ~ ~ ~ detect ~ ~-1 ~ wool 4 setblock ~ ~ ~ wool 0"
            yield U "execute @e[tag=live] ~ ~ ~ setblock ~ ~-1 ~ wool 0"
            for i = 0 to 7 do
                let nearby = nearbys.[i]
                yield U (sprintf "execute @e[tag=live] %s detect ~ ~-1 ~ wool 3 summon %s ~ ~ ~ {Tags:[\"new\"]%s}" nearby entityType entityDefaults)
                yield U (sprintf "execute @e[tag=live] %s setblock ~ ~-1 ~ wool 0" nearby)
            yield U "kill @e[tag=live]"
            yield U "entitydata @e[tag=new] {Tags:[\"live\"]}"
            yield U "execute @e[tag=live] ~ ~ ~ setblock ~ ~ ~ wool 15"
            yield U "entitydata @e[tag=keep] {Tags:[\"live\"]}"
            yield U "scoreboard players set Alive Count 0"
            yield U "execute @e[tag=live] ~ ~ ~ scoreboard players add Alive Count 1"
            yield U "scoreboard players add Ticks Count 1"
        |]
    region.PlaceCommandBlocksStartingAt(20,10,20,cmds)
    let aux = [|
                P ""
                U (sprintf "execute @e[type=Sheep] ~ ~ ~ summon %s ~ ~-1 ~ {Tags:[\"live\"]%s}" entityType entityDefaults)
                U "execute @e[type=Sheep] ~ ~ ~ setblock ~ ~-1 ~ wool 15"
                U "kill @e[type=Sheep]"
              |]
    region.PlaceCommandBlocksStartingAt(24,10,20,aux)
    let aux2 = [|
                O ""
                U (sprintf "execute @e[type=%s] ~ ~ ~ setblock ~ ~ ~ wool 0" entityType)
                U (sprintf "kill @e[type=%s]" entityType)
                U "fill -160 2 -160 0 2 0 wool"
                U "fill 0 2 -160 160 2 0 wool"
                U "fill -160 2 0 0 2 160 wool"
                U "fill 0 2 0 160 2 160 wool"
                U "fill -160 3 -160 0 3 0 wool" 
                U "fill 0 3 -160 160 3 0 wool"
                U "fill -160 3 0 0 3 160 wool"
                U "fill 0 3 0 160 3 160 wool"
                U "scoreboard players reset *"
               |]
    region.PlaceCommandBlocksStartingAt(28,10,20,aux2)
#endif
#if SCRIPTED_SHEEP
    let W n = U (sprintf """summon AreaEffectCloud ~ ~ ~1 {Tags:["nTicksLater"],Age:-%d}""" n)
    let aux = 
        [|
            P ""
            U "scoreboard players tag @e[tag=nTicksLater] add nTicksLaterDone {Age:-1}"
            U "execute @e[tag=nTicksLaterDone] ~ ~ ~ blockdata ~ ~ ~ {auto:1b}"
            U "execute @e[tag=nTicksLaterDone] ~ ~ ~ blockdata ~ ~ ~ {auto:0b}"
        |]
    let cmds =
        [|
            O "kill @e[type=Sheep]"
            yield U "summon Sheep ~5 ~-0.5 ~ {NoAI:true}"
            for _i = 1 to 6 do
                yield! [| W 3; O "tp @e[type=Sheep] ~ ~ ~0.5" |]
            for _i = 1 to 3 do
                yield! [| W 3; O "tp @e[type=Sheep] ~-0.5 ~ ~ 90 0" |]
            yield! [|
                        W 3
                        O "tp @e[type=Sheep] ~ ~ ~ 110 0"
                        W 3
                        O "tp @e[type=Sheep] ~ ~ ~ 130 0"
                        W 3
                        O "tp @e[type=Sheep] ~ ~ ~ 110 0"
                        W 3
                        O "tp @e[type=Sheep] ~ ~ ~ 90 0"
                        W 3
                        O "tp @e[type=Sheep] ~ ~ ~ 70 0"
                        W 3
                        O "tp @e[type=Sheep] ~ ~ ~ 50 0"
                        W 3
                        O "tp @e[type=Sheep] ~ ~ ~ 70 0"
                        W 3
                        O "tp @e[type=Sheep] ~ ~ ~ 90 0"
                        U "say Hello there"
                        W 20
                        O "say I am the sheep"
                        W 20
                        O "say Do you have any..."
                        W 10
                        O "say HAY?"
                    |]
        |]
    region.PlaceCommandBlocksStartingAt(40,56,80,aux)
    region.PlaceCommandBlocksStartingAt(43,56,80,cmds)
#endif
    let PRNG(destPlayer, destObjective, modPlayer, modObjective) = 
        [|
            // compute next Z value with PRNG
            yield U "scoreboard players operation Z Calc *= A Calc"
            yield U "scoreboard players operation Z Calc += C Calc"
            yield U "scoreboard players operation Z Calc *= Two Calc"  // mod 2^31
            yield U "scoreboard players operation Z Calc /= Two Calc"
            yield U "scoreboard players operation K Calc = Z Calc"
            yield U "scoreboard players operation K Calc *= Two Calc"
            yield U "scoreboard players operation K Calc /= Two Calc"
            yield U "scoreboard players operation K Calc /= TwoToSixteen Calc"   // upper 16 bits most random
            // get a number in the desired range
            yield U (sprintf "scoreboard players operation %s %s = K Calc" destPlayer destObjective)
            yield U (sprintf "scoreboard players operation %s %s %%= %s %s" destPlayer destObjective modPlayer modObjective)
            yield U (sprintf "scoreboard players operation %s %s += %s %s" destPlayer destObjective modPlayer modObjective)   // ensure non-negative
            yield U (sprintf "scoreboard players operation %s %s %%= %s %s" destPlayer destObjective modPlayer modObjective)
        |]
    let nTicksLater(n) = 
        [|
#if DEBUG_NTICKSLATER
            yield U (sprintf """tellraw @a ["schedule nTickLater %d at ",{"score":{"name":"Tick","objective":"Score"}}]""" n)
#endif
            yield U "summon ArmorStand ~ ~ ~3 {Tags:[\"nTicksLaterNewArmor\"],NoGravity:1,Marker:1}"
            yield U (sprintf "scoreboard players set @e[tag=nTicksLaterNewArmor] S -%d" n)
            yield U "entitydata @e[tag=nTicksLaterNewArmor] {Tags:[\"nTicksLaterScoredArmor\"]}"
            yield O ""
        |]


    let bingoItems =
        [|
            [|  -1, "diamond", AA.diamond                   ; -1, "diamond_hoe", AA.diamond_hoe         ; -1, "diamond_axe", AA.diamond_axe         |]
            [|  -1, "bone", AA.bone                         ; -1, "bone", AA.bone                       ; -1, "bone", AA.bone                       |]
            [|  -1, "ender_pearl", AA.ender_pearl           ; -1, "ender_pearl", AA.ender_pearl         ; -1, "ender_pearl", AA.ender_pearl         |]
            [|  -1, "deadbush", AA.deadbush                 ; +2, "tallgrass", AA.fern                  ; -1, "vine", AA.vine                       |]
            [|  -1, "brick", AA.brick                       ; -1, "brick", AA.brick                     ; -1, "brick", AA.brick                     |]
            [|  -1, "glass_bottle", AA.glass_bottle         ; -1, "glass_bottle", AA.glass_bottle       ; -1, "glass_bottle", AA.glass_bottle       |]
            [|  -1, "melon", AA.melon_slice                 ; -1, "melon", AA.melon_slice               ; -1, "speckled_melon", AA.speckled_melon   |]
            [|  +0, "dye", AA.ink_sac                       ; -1, "book", AA.book                       ; -1, "writable_book", AA.book_and_quill    |]
            [|  -1, "apple", AA.apple                       ; -1, "gold_ingot", AA.gold_ingot           ; -1, "golden_apple", AA.golden_apple       |]
            [|  -1, "flint", AA.flint                       ; -1, "flint", AA.flint                     ; -1, "flint_and_steel", AA.flint_and_steel |]
            [|  -1, "cookie", AA.cookie                     ; -1, "cookie", AA.cookie                   ; -1, "cookie", AA.cookie                   |]
            [|  -1, "pumpkin_seeds", AA.pumpkin_seeds       ; -1, "pumpkin_seeds", AA.pumpkin_seeds     ; -1, "rabbit_hide", AA.rabbit_hide         |]
            [|  -1, "rail", AA.rail                         ; -1, "rail", AA.rail                       ; -1, "rail", AA.rail                       |]
            [|  -1, "mushroom_stew", AA.mushroom_stew       ; -1, "mushroom_stew", AA.mushroom_stew     ; -1, "mushroom_stew", AA.mushroom_stew     |]
            [|  -1, "sugar", AA.sugar                       ; -1, "spider_eye", AA.spider_eye           ; -1, "fermented_spider_eye", AA.fermented_spider_eye |]
            [|  +2, "dye", AA.cactus_dye                    ; +4, "dye", AA.lapis                       ; +6, "dye", AA.cyan_dye                    |]
            [|  -1, "emerald", AA.emerald                   ; -1, "emerald", AA.emerald                 ; -1, "emerald", AA.emerald                 |]
            [|  -1, "minecart", AA.minecart                 ; -1, "chest_minecart", AA.chest_minecart   ; -1, "tnt_minecart", AA.tnt_minecart       |]
            [|  -1, "gunpowder", AA.gunpowder               ; -1, "gunpowder", AA.gunpowder             ; -1, "gunpowder", AA.gunpowder             |]
            [|  -1, "compass", AA.compass                   ; -1, "compass", AA.compass                 ; -1, "compass", AA.compass                 |]
            [|  +1, "sapling", AA.spruce_sapling            ; +1, "sapling", AA.spruce_sapling          ; -1, "slime_ball", AA.slime_ball           |]
            [|  -1, "cauldron", AA.cauldron                 ; -1, "cauldron", AA.cauldron               ; -1, "cauldron", AA.cauldron               |]
            [|  -1, "name_tag", AA.name_tag                 ; -1, "saddle", AA.saddle                   ; -1, "enchanted_book", AA.enchanted_book   |]
            [|  -1, "milk_bucket", AA.milk_bucket           ; -1, "egg", AA.egg                         ; -1, "cake", AA.cake                       |]
            [|  -1, "fish", AA.fish                         ; -1, "fish", AA.fish                       ; -1, "fish", AA.fish                       |]
            [|  -1, "sign", AA.sign                         ; -1, "item_frame", AA.item_frame           ; -1, "painting", AA.painting               |]
            [|  -1, "golden_sword", AA.golden_sword         ; -1, "clock", AA.clock                     ; -1, "golden_rail", AA.golden_rail         |]
            [|  -1, "hopper", AA.hopper                     ; -1, "hopper", AA.hopper                   ; -1, "hopper", AA.hopper                   |]
        |]
    // store bingo art in the world
    let mutable x = 0
    let mutable z = 0
    let mutable uniqueArts = Map.empty 
    for i = 0 to bingoItems.Length-1 do
        for j = 0 to 2 do
            let _, _, art = bingoItems.[i].[j]
            if not(uniqueArts.ContainsKey(art)) then
                let xCoord = 3+18*x
                let yCoord = 0
                let zCoord = 3+18*z
                AA.writeZoneFromString(region, xCoord, yCoord, zCoord, art)
                uniqueArts <- uniqueArts.Add(art, (xCoord, yCoord, zCoord))
                x <- x + 1
                if x = 8 then
                    x <- 0
                    z <- z + 1
    let MAPX, MAPY, MAPZ = 0, 19, 0
    AA.writeZoneFromString(region, MAPX, MAPY, MAPZ, AA.mapTopLeft)
    AA.writeZoneFromString(region, MAPX+64, MAPY, MAPZ, AA.mapTopRight)
    AA.writeZoneFromString(region, MAPX, MAPY, MAPZ+64, AA.mapBottomLeft)
    AA.writeZoneFromString(region, MAPX+64, MAPY, MAPZ+64, AA.mapBottomRight)
    for x = 1 to 128 do
        for z = 1 to 128 do
            region.SetBlockIDAndDamage(x, MAPY-1, z, 1uy, 0uy)  // stone below it, to prevent lighting updates
    // prepare item display chests
    let anyDifficultyItems = ResizeArray()
    let otherItems = ResizeArray()
    for i = 0 to bingoItems.Length-1 do
        if bingoItems.[i].[0] = bingoItems.[i].[1] && bingoItems.[i].[0] = bingoItems.[i].[2] then
            anyDifficultyItems.Add( bingoItems.[i].[0] )
        else
            otherItems.Add( bingoItems.[i] )
    let anyDifficultyChest = 
        let sb = new System.Text.StringBuilder("""{CustomName:"Items at any difficulty",Items:[""")
        for i = 0 to anyDifficultyItems.Count-1 do
            let dmg,item,_art = anyDifficultyItems.[i]
            sb.Append(sprintf """{Slot:%db,id:"%s",Count:%db,Damage:%ds},""" i item 1 (if dmg = -1 then 0 else dmg)) |> ignore
        let s = sb.ToString()
        s.Substring(0, s.Length-1) + "]}"
    let otherChest1 =
        let sb = new System.Text.StringBuilder("""{CustomName:"Easy/Medium/Hard in row 1/2/3",Items:[""")
        for i = 0 to 8 do
            for j = 0 to 2 do
                let dmg,item,_art = otherItems.[i].[j]
                sb.Append(sprintf """{Slot:%db,id:"%s",Count:%db,Damage:%ds},""" (i+(9*j)) item 1 (if dmg = -1 then 0 else dmg)) |> ignore
        let s = sb.ToString()
        s.Substring(0, s.Length-1) + "]}"
    let otherChest2 =
        let sb = new System.Text.StringBuilder("""{CustomName:"Easy/Medium/Hard in row 1/2/3",Items:[""")
        for i = 9 to otherItems.Count-1 do
            for j = 0 to 2 do
                let dmg,item,_art = otherItems.[i].[j]
                sb.Append(sprintf """{Slot:%db,id:"%s",Count:%db,Damage:%ds},""" (i-9+(9*j)) item 1 (if dmg = -1 then 0 else dmg)) |> ignore
        let s = sb.ToString()
        s.Substring(0, s.Length-1) + "]}"

    // TODO set render distance to 32 at spawn, ensure no extra chunks gen'd, or gen them
    let MAP_UPDATE_ROOM_LOW = Coords(59,8,69)
    let MAP_UPDATE_ROOM = MAP_UPDATE_ROOM_LOW.Offset(3,2,3)
    let WAITING_ROOM_LOW = Coords(69,8,69)
    let WAITING_ROOM = WAITING_ROOM_LOW.Offset(3,2,3)

    let BINGO_ITEMS_LOW = Coords(3,6,3)
    let TIMEKEEPER_REDSTONE = Coords(3,6,44)
    let TIMEKEEPER_25MIN_REDSTONE = Coords(3,8,44)
    let ITEM_CHECKERS_REDSTONE_LOW(team) = Coords(6+6*team,6,44)
    let ITEM_CHECKERS_REDSTONE_HIGH(team) = Coords(10+6*team,10,44)
    let GOT_AN_ITEM_COMMON_LOGIC(team) = Coords(6+6*team,6,80)
    let GOT_BINGO_REDSTONE(team) = Coords(6+6*team,8,80)
    let GOT_MEGA_BINGO_REDSTONE(team) = Coords(6+6*team,10,80)
    let GOT_LOCKOUT_REDSTONE(team) = Coords(6+6*team+2,8,80)
    let SPAWN_LOCATION_COMMANDS(team) = Coords(6+6*team+2,10,80)
    let GOT_WIN_COMMON_LOGIC = Coords(4,8,80)

    let MAKE_SEEDED_CARD = Coords(7,3,11)
    let TELEPORT_PLAYERS_TO_SEEDED_SPAWN_LOW = Coords(20,3,10)

    let TPX_LOW = Coords(40,3,10)
    let TPZ_LOW = Coords(41,3,10)
    let TPY_LOW = Coords(42,3,10)

    let RESET_SCORES_LOGIC = Coords(48,3,10)
    let RANDOM_SEED_BUTTON = Coords(49,3,10)
    let CHOOSE_SEED_BUTTON = Coords(50,3,10)
    let CHOOSE_SEED_REDSTONE = Coords(51,3,9)  // 9 not 10 to offset redstone placement
    let START_GAME_PART_1 = Coords(53,3,10)
    let START_GAME_PART_2 = Coords(54,3,10)
    let SHOW_ITEMS_BUTTON = Coords(55,3,10)
    let TOGGLE_LOCKOUT_BUTTON = Coords(56,3,10)
    let END_TUTORIAL_BUTTON = Coords(57,3,10)
    let START_TUTORIAL_BUTTON = Coords(58,3,10)
    let ENSURE_CARD_UPDATED_LOGIC = Coords(59,3,10)
    let SEND_TO_WAITING_ROOM = Coords(60,3,10)

    let VANILLA_LOADOUT = Coords(70,3,10)
    let NIGHT_VISION_LOADOUT = Coords(71,3,10)
    let SADDLED_HORSE_NIGHT_VISION_LOADOUT = Coords(72,3,10)
    let STARTING_CHEST_NIGHT_VISION_LOADOUT = Coords(73,3,10)
    let SPAMMABLE_SWORD_NIGHT_VISION_LOADOUT = Coords(74,3,10)
    let ELYTRA_JUMP_BOOST_FROST_WALKER_NIGHT_VISION_LOADOUT = Coords(75,3,10)

    let PILLAR_UP_THE_ARMOR_STAND = Coords(90,3,10)
    let COMPUTE_Y_ARMOR_STAND_LOW = Coords(91,3,10)

    let NOTICE_DROPPED_MAP_CMDS = Coords(103,3,10)
    
    //////////////////////////////
    // lobby
    //////////////////////////////

    // IWIDTH/ILENGTH are interior measures, not including walls
    let LOBBYX, LOBBYY, LOBBYZ = 50, 6, 50
    let NEW_PLAYER_PLATFORM_LO = Coords(60,LOBBYY,30)
    let NEW_PLAYER_LOCATION = NEW_PLAYER_PLATFORM_LO.Offset(5,2,5)
    let NEW_MAP_PLATFORM_LO = Coords(60,LOBBYY,90)
    let NEW_MAP_LOCATION = NEW_MAP_PLATFORM_LO.Offset(5,2,5)
    let CFG_ROOM_IWIDTH = 7
    let MAIN_ROOM_IWIDTH = 7
    let INFO_ROOM_IWITDH = 7
    let TOTAL_WIDTH = 1 + CFG_ROOM_IWIDTH + 2 + MAIN_ROOM_IWIDTH + 2 + INFO_ROOM_IWITDH + 1
    let ILENGTH = 13
    let LENGTH = ILENGTH + 2
    let HEIGHT = 6
    let NUM_CONFIG_COMMANDS = 7
    let OFFERING_SPOT = Coords(LOBBYX+TOTAL_WIDTH-INFO_ROOM_IWITDH/2-2,LOBBYY+1,LOBBYZ+2)
    let LOBBY_CENTER_LOCATION = Coords(LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH/2+3, LOBBYY+2, LOBBYZ+3)
    let makeSign kind x y z dmg txt1 txt2 txt3 txt4 =
        [|
            U (sprintf "setblock %d %d %d %s %d" x y z kind dmg)
            U (sprintf """blockdata %d %d %d {x:%d,y:%d,z:%d,id:"Sign",Text1:"{\"text\":\"%s\",\"bold\":\"true\"}",Text2:"{\"text\":\"%s\",\"bold\":\"true\"}",Text3:"{\"text\":\"%s\",\"bold\":\"true\"}",Text4:"{\"text\":\"%s\",\"bold\":\"true\"}"}""" x y z x y z txt1 txt2 txt3 txt4)
        |]
    let makeWallSign x y z dmg txt1 txt2 txt3 txt4 = makeSign "wall_sign" x y z dmg txt1 txt2 txt3 txt4 
    let makeWallSignActivate x y z dmg txt1 txt2 (a:Coords) isBold color =
        let bc = sprintf """,\"bold\":\"%s\",\"color\":\"%s\" """ (if isBold then "true" else "false") color
        let c1 = if isBold then sprintf """,\"clickEvent\":{\"action\":\"run_command\",\"value\":\"blockdata %d %d %d {auto:1b}\"} """ a.X a.Y a.Z else ""
        let c2 = if isBold then sprintf """,\"clickEvent\":{\"action\":\"run_command\",\"value\":\"blockdata %d %d %d {auto:0b}\"} """ a.X a.Y a.Z else ""
        [|
            U (sprintf "setblock %d %d %d wall_sign %d" x y z dmg)
            U (sprintf """blockdata %d %d %d {x:%d,y:%d,z:%d,id:"Sign",Text1:"{\"text\":\"%s\"%s%s}",Text2:"{\"text\":\"%s\"%s%s}"}"""  x y z x y z txt1 bc c1 txt2 bc c2)
        |]
    let makeSignDoAction kind x y z dmg txt1 txt2 txt3 a1 cmd1 a2 cmd2 isBold color =
        let bc = sprintf """,\"bold\":\"%s\",\"color\":\"%s\" """ (if isBold then "true" else "false") color
        let c1 = if isBold then sprintf """,\"clickEvent\":{\"action\":\"%s\",\"value\":\"%s\"} """ a1 cmd1 else ""
        let c2 = if isBold then sprintf """,\"clickEvent\":{\"action\":\"%s\",\"value\":\"%s\"} """ a2 cmd2 else ""
        [|
            U (sprintf "setblock %d %d %d %s %d" x y z kind dmg)
            U (sprintf """blockdata %d %d %d {x:%d,y:%d,z:%d,id:"Sign",Text1:"{\"text\":\"%s\"%s%s}",Text2:"{\"text\":\"%s\"%s%s}",Text3:"{\"text\":\"%s\"%s}"}""" x y z x y z txt1 bc c1 txt2 bc c2 txt3 bc)
        |]
    let makeSignDo kind x y z dmg txt1 txt2 txt3 cmd1 cmd2 isBold color = makeSignDoAction kind x y z dmg txt1 txt2 txt3 "run_command" cmd1 "run_command" cmd2 isBold color 
    let makeWallSignDo x y z dmg txt1 txt2 txt3 cmd1 cmd2 isBold color =
        makeSignDo "wall_sign" x y z dmg txt1 txt2 txt3 cmd1 cmd2 isBold color
    let makeLobbyCmds =
        [|
            let wall = "minecraft:stained_hardened_clay 3"
            yield O ""
            // clear
            yield U (sprintf "fill %d %d %d %d %d %d air" LOBBYX LOBBYY LOBBYZ (LOBBYX+TOTAL_WIDTH-1) (LOBBYY+HEIGHT-1) (LOBBYZ+LENGTH-1))
            // z walls
            yield U (sprintf "fill %d %d %d %d %d %d %s" LOBBYX LOBBYY LOBBYZ (LOBBYX+TOTAL_WIDTH-1) (LOBBYY+HEIGHT-1) LOBBYZ wall)
            yield U (sprintf "fill %d %d %d %d %d %d %s" LOBBYX LOBBYY (LOBBYZ+LENGTH-1) (LOBBYX+TOTAL_WIDTH-1) (LOBBYY+HEIGHT-1) (LOBBYZ+LENGTH-1) wall)
            // z windows
            yield U (sprintf "fill %d %d %d %d %d %d barrier" (LOBBYX+CFG_ROOM_IWIDTH+4) (LOBBYY+1) LOBBYZ (LOBBYX+CFG_ROOM_IWIDTH+2+MAIN_ROOM_IWIDTH-1) (LOBBYY+3) LOBBYZ)
            yield U (sprintf "fill %d %d %d %d %d %d barrier" (LOBBYX+2) (LOBBYY+1) (LOBBYZ+LENGTH-1) (LOBBYX+CFG_ROOM_IWIDTH-1) (LOBBYY+3) (LOBBYZ+LENGTH-1))
            yield U (sprintf "fill %d %d %d %d %d %d barrier" (LOBBYX+CFG_ROOM_IWIDTH+2+MAIN_ROOM_IWIDTH+4) (LOBBYY+1) (LOBBYZ+LENGTH-1) (LOBBYX+TOTAL_WIDTH-3) (LOBBYY+3) (LOBBYZ+LENGTH-1))
            // x walls
            yield U (sprintf "fill %d %d %d %d %d %d %s" LOBBYX LOBBYY LOBBYZ LOBBYX (LOBBYY+HEIGHT-1) (LOBBYZ+LENGTH-1) wall)
            yield U (sprintf "fill %d %d %d %d %d %d %s" (LOBBYX+CFG_ROOM_IWIDTH+1) LOBBYY LOBBYZ (LOBBYX+CFG_ROOM_IWIDTH+2) (LOBBYY+HEIGHT-1) (LOBBYZ+LENGTH-1) wall)
            yield U (sprintf "fill %d %d %d %d %d %d air" (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+1) (LOBBYZ+LENGTH-5) (LOBBYX+CFG_ROOM_IWIDTH+2) (LOBBYY+4) (LOBBYZ+LENGTH-2))
            yield U (sprintf "fill %d %d %d %d %d %d %s" (LOBBYX+CFG_ROOM_IWIDTH+2+MAIN_ROOM_IWIDTH+1) LOBBYY LOBBYZ (LOBBYX+CFG_ROOM_IWIDTH+2+MAIN_ROOM_IWIDTH+2) (LOBBYY+HEIGHT-1) (LOBBYZ+LENGTH-1) wall)
            yield U (sprintf "fill %d %d %d %d %d %d air" (LOBBYX+CFG_ROOM_IWIDTH+2+MAIN_ROOM_IWIDTH+1) (LOBBYY+1) (LOBBYZ+LENGTH-5) (LOBBYX+CFG_ROOM_IWIDTH+2+MAIN_ROOM_IWIDTH+2) (LOBBYY+4) (LOBBYZ+LENGTH-2))
            yield U (sprintf "fill %d %d %d %d %d %d %s" (LOBBYX+TOTAL_WIDTH-1) LOBBYY LOBBYZ (LOBBYX+TOTAL_WIDTH-1) (LOBBYY+HEIGHT-1) (LOBBYZ+LENGTH-1) wall)
            // room colors
            yield U (sprintf "fill %d %d %d %d %d %d minecraft:stained_hardened_clay 6 replace minecraft:stained_hardened_clay" LOBBYX LOBBYY LOBBYZ (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+HEIGHT-1) (LOBBYZ+LENGTH-1))
            yield U (sprintf "fill %d %d %d %d %d %d minecraft:stained_hardened_clay 8 replace minecraft:stained_hardened_clay" (LOBBYX+CFG_ROOM_IWIDTH+2+MAIN_ROOM_IWIDTH+2) LOBBYY LOBBYZ (LOBBYX+TOTAL_WIDTH-1) (LOBBYY+HEIGHT-1) (LOBBYZ+LENGTH-1))
            // floor & ceiling
            yield U (sprintf "fill %d %d %d %d %d %d sea_lantern" LOBBYX LOBBYY LOBBYZ (LOBBYX+TOTAL_WIDTH-1) LOBBYY (LOBBYZ+LENGTH-1))
            yield U (sprintf "fill %d %d %d %d %d %d sea_lantern" LOBBYX (LOBBYY+HEIGHT) LOBBYZ (LOBBYX+TOTAL_WIDTH-1) (LOBBYY+HEIGHT) (LOBBYZ+LENGTH-1))
            // cfg room blocks
            yield U (sprintf "fill %d %d %d %d %d %d chain_command_block 3" (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+3) (LOBBYZ+2) (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+3) (LOBBYZ+2+NUM_CONFIG_COMMANDS-1))
            yield U (sprintf "fill %d %d %d %d %d %d chain_command_block 3" (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+1) (LOBBYZ+2) (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+1) (LOBBYZ+2+NUM_CONFIG_COMMANDS-1))
            yield U (sprintf "setblock %d %d %d chest" (LOBBYX+1) (LOBBYY+1) (LOBBYZ+1))
            // put heads
            yield U (sprintf "/summon ArmorStand %f %f %f {NoGravity:1,Marker:1,Invisible:1,ArmorItems:[{},{},{},{id:skull,Damage:3,tag:{SkullOwner:Lorgon111}}]}" (float (LOBBYX+TOTAL_WIDTH-5) + 0.5) (float (LOBBYY+2) - 0.5) (float (LOBBYZ+1) - 0.0))
            yield U (sprintf "/summon ArmorStand %f %f %f {Tags:[\"asToReverse\"],NoGravity:1,Marker:1,Invisible:1,ArmorItems:[{},{},{},{id:skull,Damage:3,tag:{SkullOwner:Lorgon111}}]}" (float (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH/2+3) + 0.5) (float (LOBBYY+2) - 0.5) (float (LOBBYZ+14) - 0.0))
            yield! nTicksLater(1)
            yield U "tp @e[tag=asToReverse] ~ ~ ~ 180 0"
            // put enabled signs
            yield U (sprintf "blockdata %d %d %d {auto:1b}" (LOBBYX-4) LOBBYY LOBBYZ)
            yield U (sprintf "blockdata %d %d %d {auto:0b}" (LOBBYX-4) LOBBYY LOBBYZ)
            // make map-update-room
            yield U (sprintf "fill %s %s sea_lantern 0 hollow" (MAP_UPDATE_ROOM_LOW.STR) (MAP_UPDATE_ROOM_LOW.Offset(6,5,6).STR))
            yield U (sprintf "fill %s %s barrier 0 hollow" (MAP_UPDATE_ROOM_LOW.Offset(2,1,2).STR) (MAP_UPDATE_ROOM_LOW.Offset(4,4,4).STR))
            yield! makeWallSign (MAP_UPDATE_ROOM_LOW.X+3) (MAP_UPDATE_ROOM_LOW.Y+3) (MAP_UPDATE_ROOM_LOW.Z+1) 3 "HOLD YOUR MAP" "(it will only" "update if you" "hold it)"
            // make waiting room
            yield U (sprintf "fill %s %s sea_lantern 0 hollow" (WAITING_ROOM_LOW.STR) (WAITING_ROOM_LOW.Offset(6,5,6).STR))
            yield U (sprintf "fill %s %s barrier 0 hollow" (WAITING_ROOM_LOW.Offset(2,1,2).STR) (WAITING_ROOM_LOW.Offset(4,4,4).STR))
            yield! makeWallSign (WAITING_ROOM_LOW.X+3) (WAITING_ROOM_LOW.Y+3) (WAITING_ROOM_LOW.Z+1) 3 "PLEASE WAIT" "(spawns are" "being" "generated)"
            // team color wall bits
            let wsx, wsy, wsz = (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+2), (LOBBYY+2), (LOBBYZ+5)
            yield U (sprintf "fill %d %d %d %d %d %d redstone_block" (wsx+1) (wsy-1) (wsz+0) (wsx+1) (wsy+1) (wsz+0))
            yield U (sprintf "fill %d %d %d %d %d %d lapis_block" (wsx+1) (wsy-1) (wsz+1) (wsx+1) (wsy+1) (wsz+1))
            yield U (sprintf "fill %d %d %d %d %d %d gold_block" (wsx+1) (wsy-1) (wsz+2) (wsx+1) (wsy+1) (wsz+2))
            yield U (sprintf "fill %d %d %d %d %d %d emerald_block" (wsx+1) (wsy-1) (wsz+3) (wsx+1) (wsy+1) (wsz+3))
        |]
    region.PlaceCommandBlocksStartingAtSelfDestruct(LOBBYX-2,LOBBYY,LOBBYZ,makeLobbyCmds,"build lobby walls")
    let bingo30testers = // TODO finish this list if more
        [|
            "Cacille"
            "ConeDodger"
            "DucksEatFree"
            "Shook50"
            "gothfaerie"
            "obesity84"
            "Insmanity"
        |] |> Array.sortBy (fun s -> s.ToLower()) 
    let bingo20testers = 
        [|
            "GrannyGamer1"
            "gothfaerie"
            "ConeDodger"
            "phedran"
            "jahg1977"
            "Zhuria"
            "Meroka"
            "Alzorath"
            "NihonTiger"
            "DireDwarf"
            "iSuchtel"
            "Blitzkriegsler"
            "IronStoneMine"
            "mod1982"
            "VanRyderLP"
            "generikb"
            "Trazlander"
            "three_two"
            "kurtjmac"
            "Bergasms"
            "FixxxerTV"
            "Grim"
            "LZmiljoona"
            "GreatScottLP"
            "LDShadowLady"
            "CthulhuToo"
            "Shook50"
            "DucksEatFree"
        |] |> Array.sortBy (fun s -> s.ToLower()) 
    let formatBingoTesters(a) =
        let sb = System.Text.StringBuilder() 
        for s:string in a do
            sb.Append(s).Append("\n") |> ignore
        let r = sb.ToString()
        r.Substring(0,r.Length-1)
    // TODO make 'BINGO Card' always be in color or something
    let gameplayBookCmd = makeCommandGivePlayerWrittenBook("Lorgon111","Gameplay", [|
            // TODO finalize prose
            """{"text":"Minecraft BINGO is a vanilla-survival scavenger hunt mini-game. Players must punch trees, craft tools, and kill monsters, as in normal Minecraft, but with a goal..."}"""
            """{"text":"Players are given a 'BINGO Card' map picturing 25 different items. The goal is to race to collect these items as fast as possible to get a 5-in-a-row BINGO..."}"""
            """{"text":"Each time you get an item on the card, a fireworks sound will play, your score will update, and a text notification will appear in the chat..."}"""
            """{"text":"You can see which items you have so far by 'updating your map': hold your maps, and then drop one copy on the ground..."}"""
            """{"text":"Any number of players is supported. On a multi-player server, 4 team colors allow players to collaborate or race against each other..."}"""
            """{"text":"In single-player, you can still compete against others by choosing 'seeds'; the same seed number always yields the same BINGO Card and spawn point."}"""
            |] )
    let gameModesBookCmd = makeCommandGivePlayerWrittenBook("Lorgon111","Major game modes", [|
            // TODO finalize prose
            """{"text":"Minecraft BINGO supports a variety of different game modes; the most basic is to play for a 'BINGO' (5 in a row, column, or diagonal) to win..."}"""
            """{"text":"For extra challenge, you can play for the 'blackout': getting all 25 items on the card..."}"""
            """{"text":"Another game mode is to gather as many items as possible within 25 minutes (1500 seconds) as a timed challenge..."}"""
            """{"text":"Each match always supports all these modes (there's nothing to configure); BINGO, blackout, and 25-minute scores are detected and printed automatically..."}"""
            """{"text":"But you're free to make your own rules; if you see a nice mountain and want to build a cabin instead, do it! Minecraft is very flexible :)"}"""
            |] )
    let customTerrainBookCmd = makeCommandGivePlayerWrittenBook("Lorgon111","Custom terrain", [|
            // TODO finalize prose
            """{"text":"Minecraft BINGO is played in a normal Minecraft world, with just 3 small changes to default world generation..."}"""
            """{"text":"First, the biome size is set to 'tiny', so that you do not need to travel for hours to find a jungle or a swamp; most biomes are close by..."}"""
            """{"text":"Second, dungeons frequency is increased to maximum, so that all players have a good chance of finding dungeon loot in the first 10 minutes..."}"""
            """{"text":"Finally, granite, diorite, and andesite are removed, so that your inventory is not filled with extra stone types while trying to collect items."}"""
            |] )
    let thanksBookCmd = makeCommandGivePlayerWrittenBook("Lorgon111","Thanks", [|
            // TODO finalize prose
            """{"text":"I've spent more than 200 hours developing MinecraftBINGO, but I got a lot of help along the way.\n\nThanks to..."}"""
            sprintf """{"text":"Version 3.0 playtesters:\n\n%s"}""" (formatBingoTesters bingo30testers)
            sprintf """{"text":"Version 2.x playtesters:\n\n%s"}""" (formatBingoTesters (Seq.append bingo20testers.[0..9] ["..."]))
            sprintf """{"text":"Version 2.x playtesters (cont'd):\n\n%s"}""" (formatBingoTesters (Seq.append bingo20testers.[10..19] ["..."]))
            sprintf """{"text":"Version 2.x playtesters (cont'd):\n\n%s"}""" (formatBingoTesters bingo20testers.[20..])
            """{"text":"Special thanks to\nAntVenom\nwho gave me the idea for Version 1.0, and\nBergasms\nwho helped me test and implement the first version."}"""
            """{"text":"And of course, to you,\n","extra":[{"selector":"@p"},{"text":"\nthanks for playing!\n\nSigned,\nDr. Brian Lorgon111"}]}"""
            |] )
    let versionInfoBookCmd = makeCommandGivePlayerWrittenBook("Lorgon111","Versions", [|
            // TODO finalize prose
            """{"text":"You're playing MinecraftBINGO\n\nVersion ","extra":[{"text":"3.0",color:"red"},{"text":"\n\nTo get the latest version, click\n\n"},{"text":"@MinecraftBINGO","clickEvent":{"action":"open_url","value":"https://twitter.com/MinecraftBINGO"},"underlined":"true"}]}"""
            """{"text":"Version History\n\n3.0 - TODO/MM/DD\n\nRewrote everything from scratch using new Minecraft 1.9 command blocks. So much more efficient!"}"""
            """{"text":"Version History\n\n2.5 - 2014/11/27\n\nUpdate for Minecraft 1.8.1, which changed map colors.\n\n2.4 - 2014/09/12\n\nAdded 'seed' game mode to specify card and spawn point via a seed number."}"""
            """{"text":"Version History\n\n2.3 - 2014/06/17\n\nAdded more items and lockout mode.\n\n2.2 - 2014/05/29\n\nAdded multiplayer team gameplay."}"""
            """{"text":"Version History\n\n2.1 - 2014/05/12\n\nCustomized terrain with tiny biomes and many dungeons.\n\n2.0 - 2014/04/09\n\nRandomize the 25 items on the card."}"""
            """{"text":"Version History\n\n1.0 - 2013/10/03\n\nOriginal Minecraft 1.6 version. Only one fixed card, dispensed buckets of water to circle items on the map. (pre-dates /setblock!)"}"""
            |] )
    // TODO
    let customModesBookCmd = makeCommandGivePlayerWrittenBook("Lorgon111","Lockout/customization", [|
            // TODO finalize prose
            """{"text":"Minecraft BINGO supports TODO..."}"""
            |] )
    // community (reddit, twitter)
    // (in custom room) about customization
    let placeSigns(enabled) =
        [|
            yield O ""
            // interior layout - cfg room
            yield! makeWallSignActivate (LOBBYX+CFG_ROOM_IWIDTH/2+1) (LOBBYY+2) (LOBBYZ+1) 3 "toggle lockout" "" TOGGLE_LOCKOUT_BUTTON enabled (if enabled then "black" else "gray")
#if DEBUG
            yield! makeWallSignDo (LOBBYX+CFG_ROOM_IWIDTH/2+2) (LOBBYY+2) (LOBBYZ+1) 3 "enable" "ticklagdebug" "" "" "scoreboard players set @p TickInfo 1" true "black" // TODO eventually remove this
            yield! makeWallSignDo (LOBBYX+CFG_ROOM_IWIDTH/2+3) (LOBBYY+2) (LOBBYZ+1) 3 "disable" "ticklagdebug" "" "" "scoreboard players set @p TickInfo 0" true "black" // TODO eventually remove this
#endif
            yield! makeWallSign (LOBBYX+CFG_ROOM_IWIDTH) (LOBBYY+4) (LOBBYZ+5) 4 "run at" "start" "" ""
            yield! makeWallSign (LOBBYX+CFG_ROOM_IWIDTH) (LOBBYY+2) (LOBBYZ+5) 4 "run at" "respawn" "" ""
            let mkLoadout x y z d txt1 txt2 txt3 (c:Coords) tellPlayers =
                makeWallSignDo x y z d txt1 txt2 txt3 (sprintf """tellraw @a [\\\"%s\\\"]""" tellPlayers) (sprintf "clone %s %s %d %d %d masked" c.STR (c.Offset(0,2,NUM_CONFIG_COMMANDS-1).STR) (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+1) (LOBBYZ+2)) enabled (if enabled then "black" else "gray")
            yield! mkLoadout (LOBBYX+1) (LOBBYY+2) (LOBBYZ+11) 5 "night vision" "" "" NIGHT_VISION_LOADOUT "Game configured: Players get night vision at start & respawn"
            yield! mkLoadout (LOBBYX+1) (LOBBYY+2) (LOBBYZ+9) 5 "vanilla" "" "" VANILLA_LOADOUT "Game configured: Vanilla gameplay (no on-start/on-respawn commands)"
            yield! mkLoadout (LOBBYX+1) (LOBBYY+2) (LOBBYZ+7) 5 "spammable" "iron sword" "+night vision" SPAMMABLE_SWORD_NIGHT_VISION_LOADOUT "Game configured: Players get night vision at start & respawn, as well as a spammable unbreakable iron sword at game start"
            yield! mkLoadout (LOBBYX+1) (LOBBYY+2) (LOBBYZ+5) 5 "saddled horse" "+night vision" "" SADDLED_HORSE_NIGHT_VISION_LOADOUT "Game configured: Players get night vision at start & respawn, as well as an invulnerable saddled horse at game start"
            yield! mkLoadout (LOBBYX+1) (LOBBYY+2) (LOBBYZ+3) 5 "elytra" "+frost walker" "+night vision" ELYTRA_JUMP_BOOST_FROST_WALKER_NIGHT_VISION_LOADOUT "Game configured: Players get night vision, frost walker, elytra, and jump boost potions at start & respawn"
            yield! mkLoadout (LOBBYX+1) (LOBBYY+2) (LOBBYZ+1) 5 "starting chest" "per team" "+night vision" STARTING_CHEST_NIGHT_VISION_LOADOUT "Game configured: Players get night vision at start & respawn, and each team starts with chest of items"
            // TODO reset everything ('circuit breaker?')
            // interior layout - main room
            // TODO need this? yield! makeWallSignDo (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH/2+3) (LOBBYY+2) (LOBBYZ+1) 3 "Go to" "TUTORIAL" "" (sprintf "tp @p %s 90 180" (NEW_PLAYER_LOCATION.STR)) "" enabled (if enabled then "black" else "gray")
            yield! makeWallSignActivate (LOBBYX+CFG_ROOM_IWIDTH+3) (LOBBYY+2) (LOBBYZ+8) 5 "Make RANDOM" "card" RANDOM_SEED_BUTTON true "black"
            if not enabled then
                yield U (sprintf """blockdata %d %d %d {Text3:"(ends any game",Text4:"in progress)"}""" (LOBBYX+CFG_ROOM_IWIDTH+3) (LOBBYY+2) (LOBBYZ+8))
            yield! makeWallSignActivate (LOBBYX+CFG_ROOM_IWIDTH+3) (LOBBYY+2) (LOBBYZ+6) 5 "Choose SEED" "for card" CHOOSE_SEED_BUTTON true "black"
            if not enabled then
                yield U (sprintf """blockdata %d %d %d {Text3:"(ends any game",Text4:"in progress)"}""" (LOBBYX+CFG_ROOM_IWIDTH+3) (LOBBYY+2) (LOBBYZ+6))
            yield! makeWallSignActivate (LOBBYX+CFG_ROOM_IWIDTH+3) (LOBBYY+2) (LOBBYZ+4) 5 "START" "the game" START_GAME_PART_1 enabled (if enabled then "black" else "gray")
            yield! makeWallSignDo (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+2) (LOBBYY+2) (LOBBYZ+5) 4 "Join team" "RED" "" "scoreboard teams join red @p" "scoreboard players set @p Score 0" enabled (if enabled then "black" else "gray")
            yield! makeWallSignDo (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+2) (LOBBYY+2) (LOBBYZ+6) 4 "Join team" "BLUE" "" "scoreboard teams join blue @p" "scoreboard players set @p Score 0" enabled (if enabled then "black" else "gray")
            yield! makeWallSignDo (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+2) (LOBBYY+2) (LOBBYZ+7) 4 "Join team" "YELLOW" "" "scoreboard teams join yellow @p" "scoreboard players set @p Score 0" enabled (if enabled then "black" else "gray")
            yield! makeWallSignDo (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+2) (LOBBYY+2) (LOBBYZ+8) 4 "Join team" "GREEN" "" "scoreboard teams join green @p" "scoreboard players set @p Score 0" enabled (if enabled then "black" else "gray")
            yield! makeWallSign (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH/2+0) (LOBBYY+2) (LOBBYZ+13) 2 "Custom" "Settings" """----->\",\"strikethrough\":\"true""" ""
            yield! makeWallSign (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH/2+3) (LOBBYY+2) (LOBBYZ+13) 2 "Welcome to" "MinecraftBINGO" "by Dr. Brian" "Lorgon111"
            yield! makeWallSign (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH/2+6) (LOBBYY+2) (LOBBYZ+13) 2 "Game" "Info" """<-----\",\"strikethrough\":\"true""" ""
            // interior layout - info room
            yield! makeWallSignDo (LOBBYX+TOTAL_WIDTH-2) (LOBBYY+2) (LOBBYZ+4) 4 "Learn about" "basic rules" "and gameplay" (escape2 gameplayBookCmd) "" true "black"
            yield! makeWallSignDo (LOBBYX+TOTAL_WIDTH-2) (LOBBYY+2) (LOBBYZ+6) 4 "Learn about" "various" "game modes" (escape2 gameModesBookCmd) "" true "black"
            yield! makeWallSignDo (LOBBYX+TOTAL_WIDTH-2) (LOBBYY+2) (LOBBYZ+8) 4 "Learn about" "this world's" "custom terrain" (escape2 customTerrainBookCmd) "" true "black"
            yield! makeWallSignDo (LOBBYX+TOTAL_WIDTH-2) (LOBBYY+2) (LOBBYZ+10) 4 "Learn about" "all the folks" "who helped" (escape2 thanksBookCmd) "" true "black"
            yield! makeWallSign (LOBBYX+TOTAL_WIDTH-5) (LOBBYY+2) (LOBBYZ+1) 3 "Thanks for" "playing!" "" ""
            yield! makeWallSignActivate (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+5) (LOBBYY+2) (LOBBYZ+8) 5 "Show all" "possible items" SHOW_ITEMS_BUTTON true "black"
            yield! makeWallSignDo (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+5) (LOBBYY+2) (LOBBYZ+6) 5 "Version" "Info" "" (escape2 versionInfoBookCmd) "" true "black"
            yield! makeWallSign (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+5) (LOBBYY+2) (LOBBYZ+4) 5 "donate" "" "" ""
            // start platform has a disable-able sign
            let GTT = NEW_PLAYER_PLATFORM_LO.Offset(7,1,6)
            yield! makeSignDo "standing_sign" GTT.X GTT.Y GTT.Z 4 "Right-click" "me to go to" "TUTORIAL" (sprintf "blockdata %s {auto:1b}" START_TUTORIAL_BUTTON.STR) (sprintf "blockdata %s {auto:0b}" START_TUTORIAL_BUTTON.STR) enabled (if enabled then "black" else "gray")
        |]
    region.PlaceCommandBlocksStartingAt(LOBBYX-3,LOBBYY,LOBBYZ,placeSigns(false),"disabled signs")
    region.PlaceCommandBlocksStartingAt(LOBBYX-4,LOBBYY,LOBBYZ,placeSigns(true),"enabled signs")

    //////////////////////////////
    // loadouts
    //////////////////////////////
    let loadout(start:_[], respawn:_[], c:Coords, comment) =
        if start.Length > NUM_CONFIG_COMMANDS || respawn .Length > NUM_CONFIG_COMMANDS then
            failwith "too many cmds"
        else
            let s = Array.init NUM_CONFIG_COMMANDS (fun i -> if i < start.Length then start.[i] else U "")
            region.PlaceCommandBlocksStartingAt(c.Offset(0,2,0),s,comment)
            let r = Array.init NUM_CONFIG_COMMANDS (fun i -> if i < respawn.Length then respawn.[i] else U "")
            region.PlaceCommandBlocksStartingAt(c.Offset(0,0,0),r,comment)
    loadout([||],[||],VANILLA_LOADOUT,"vanillaLoadout")
    let nightVisionLoadout =
        [|
            U "effect @a night_vision 9999 1 true"
        |],
        [|
            U "effect @a[tag=justRespawned] night_vision 9999 1 true"
        |], 
        NIGHT_VISION_LOADOUT, "nightVisionLoadout"
    loadout(nightVisionLoadout)
    let saddledHorseNightVisionLoadout =
        [|
            U "effect @a night_vision 9999 1 true"
            U """execute @a ~ ~ ~ summon EntityHorse ~ ~2 ~ {Tame:1b,Attributes:[0:{Base:40.0d,Name:"generic.maxHealth"},1:{Base:0.0d,Name:"generic.knockbackResistance"},2:{Base:0.3d,Name:"generic.movementSpeed"},3:{Base:0.0d,Name:"generic.armor"},4:{Base:16.0d,Name:"generic.followRange"},5:{Base:0.7d,Name:"horse.jumpStrength"}],Invulnerable:1b,Health:40.0f,SaddleItem:{id:"minecraft:saddle",Count:1b,Damage:0s}}"""
        |],
        [|
            U "effect @a[tag=justRespawned] night_vision 9999 1 true"
        |], 
        SADDLED_HORSE_NIGHT_VISION_LOADOUT, "saddledHorseNightVisionLoadout"
    loadout(saddledHorseNightVisionLoadout)
    let startingChestNightVisionLoadout =
        [|
            U "effect @a night_vision 9999 1 true"
            U (sprintf "execute @p[team=red] ~ ~ ~ clone %d %d %d %d %d %d ~ ~2 ~" (LOBBYX+1) (LOBBYY+1) (LOBBYZ+1) (LOBBYX+1) (LOBBYY+1) (LOBBYZ+1))
            U (sprintf "execute @p[team=blue] ~ ~ ~ clone %d %d %d %d %d %d ~ ~2 ~" (LOBBYX+1) (LOBBYY+1) (LOBBYZ+1) (LOBBYX+1) (LOBBYY+1) (LOBBYZ+1))
            U (sprintf "execute @p[team=yellow] ~ ~ ~ clone %d %d %d %d %d %d ~ ~2 ~" (LOBBYX+1) (LOBBYY+1) (LOBBYZ+1) (LOBBYX+1) (LOBBYY+1) (LOBBYZ+1))
            U (sprintf "execute @p[team=green] ~ ~ ~ clone %d %d %d %d %d %d ~ ~2 ~" (LOBBYX+1) (LOBBYY+1) (LOBBYZ+1) (LOBBYX+1) (LOBBYY+1) (LOBBYZ+1))
        |],
        [|
            U "effect @a[tag=justRespawned] night_vision 9999 1 true"
        |], 
        STARTING_CHEST_NIGHT_VISION_LOADOUT, "startingChestNightVisionLoadout"
    loadout(startingChestNightVisionLoadout)
    let spammableSwordNightVisionLoadout =
        [|
            U "effect @a night_vision 9999 1 true"
            U """/give @a minecraft:iron_sword 1 0 {display:{Name:"Spammable unbreakable sword"},Unbreakable:1,AttributeModifiers:[{AttributeName:"generic.attackSpeed",Name:"Speed",Slot:"mainhand",Amount:1020.0,Operation:0,UUIDLeast:111l,UUIDMost:111l},{AttributeName:"generic.attackDamage",Name:"Damage",Slot:"mainhand",Amount:4.0,Operation:0,UUIDLeast:222l,UUIDMost:222l}]}"""
        |],
        [|
            U "effect @a[tag=justRespawned] night_vision 9999 1 true"
        |], 
        SPAMMABLE_SWORD_NIGHT_VISION_LOADOUT, "spammableSwordNightVisionLoadout"
    loadout(spammableSwordNightVisionLoadout)
    let elytraJumpBoostFrostWalkerNightVisionLoadout =
        [|
            U "effect @a night_vision 9999 1 true"
            U """replaceitem entity @a slot.hotbar.7 minecraft:splash_potion 64 0 {CustomPotionEffects:[{Id:8,Amplifier:39,Duration:60}]}"""
            U """replaceitem entity @a slot.armor.chest minecraft:elytra 1 0 {Unbreakable:1}"""
            U """replaceitem entity @a slot.armor.feet minecraft:leather_boots 1 0 {Unbreakable:1,ench:[{lvl:2s,id:9s}]}"""
        |],
        [|
            U "effect @a[tag=justRespawned] night_vision 9999 1 true"
            U """replaceitem entity @a[tag=justRespawned] slot.hotbar.7 minecraft:splash_potion 64 0 {CustomPotionEffects:[{Id:8,Amplifier:39,Duration:60}]}"""
            U """replaceitem entity @a[tag=justRespawned] slot.armor.chest minecraft:elytra 1 0 {Unbreakable:1}"""
            U """replaceitem entity @a[tag=justRespawned] slot.armor.feet minecraft:leather_boots 1 0 {Unbreakable:1,ench:[{lvl:2s,id:9s}]}"""
        |], 
        ELYTRA_JUMP_BOOST_FROST_WALKER_NIGHT_VISION_LOADOUT, "elytraJumpBoostFrostWalkerNightVisionLoadout"
    loadout(elytraJumpBoostFrostWalkerNightVisionLoadout)

    //////////////////////////////
    // tutorial
    //////////////////////////////
    let TUTORIAL_LOCATION = Coords(-90, 2, -120)
    let TUTORIAL_PLAYER_START = TUTORIAL_LOCATION.Offset(-2,2,2)
    let TUTORIAL_CMDS = Coords(80,3,10)
    let makeWallSignIncZ args = 
        let r = makeWallSign args
        signZ <- signZ + 1
        r
    let makeTutorialCmds =
        [|
            let tut = TUTORIAL_LOCATION
            let signY = TUTORIAL_LOCATION.Y+2
            let signX = TUTORIAL_LOCATION.X-1
            yield O ""
            yield U (sprintf "fill %s %s stone" (tut.Offset( 0,0,-1).STR) (tut.Offset(-5,4,-1).STR))
            yield U (sprintf "fill %s %s stone" (tut.Offset( 0,0, 0).STR) (tut.Offset( 0,4,30).STR))
            yield U (sprintf "fill %s %s stone" (tut.Offset(-5,0, 0).STR) (tut.Offset(-5,4,30).STR))
            yield U (sprintf "fill %s %s sea_lantern" (tut.Offset(-5,0,0).STR) (tut.Offset(0,0,30).STR))
            signZ <- TUTORIAL_LOCATION.Z + 1
            yield! makeWallSignIncZ signX signY signZ 4 "Welcome to" "MinecraftBINGO" "by Dr. Brian" "Lorgon111"
            yield! makeWallSignIncZ signX signY signZ 4 "MinecraftBINGO" "uses" "clickable signs" ""
            yield! makeWallSignDo signX signY signZ 4 "Right-click" "this sign" "to continue" (sprintf "tp @p %s 90 180" (TUTORIAL_PLAYER_START.Offset(0,0,5).STR)) "" true "black"
            signZ <- signZ + 2
            yield U (sprintf "fill %s %s stone" (tut.Offset( 0,0,signZ-TUTORIAL_LOCATION.Z).STR) (tut.Offset(-5,4,signZ-TUTORIAL_LOCATION.Z).STR))
            signZ <- signZ + 2
            yield! makeWallSignIncZ signX signY signZ 4 "MinecraftBINGO" "plays as" "new-world" "survival Minecraft"
            yield! makeWallSignIncZ signX signY signZ 4 "You'll need to" "punch trees," "craft tools," "and eat"
            yield! makeWallSignIncZ signX signY signZ 4 "But you're" "in a race" "to complete" "a goal"
            signZ <- signZ + 1
            yield! makeWallSignIncZ signX signY signZ 4 "There are" "25 items" "pictured on" "the BINGO card"
            yield! makeWallSignIncZ signX signY signZ 4 "You want to" "get items" "as fast as" "you can"
            yield! makeWallSignIncZ signX signY signZ 4 "Goal is 'BINGO'" "5 in a row," "column, or" "diagonal"
            signZ <- signZ + 1
            yield! makeWallSignIncZ signX signY signZ 4 "Try getting" "an item now" "" ""
            yield! makeWallSignIncZ signX signY signZ 4 "Punch down some" "sugar cane" "and craft it" "into sugar"
            signZ <- signZ + 1
            yield! makeWallSignIncZ signX signY signZ 4 "When you get" "an item," "your score" "will update"
            yield! makeWallSignIncZ signX signY signZ 4 "You can see" "what items" "you've gotten" "by..."
            yield! makeWallSignIncZ signX signY signZ 4 "...holding" "your maps and" "dropping" "one copy"
            yield! makeWallSignIncZ signX signY signZ 4 "(The 'drop'" "key is 'Q'" "by default)" ""
            yield! makeWallSignIncZ signX signY signZ 4 "Try it now!" "(drop a" "BINGO Card)" ""
            signZ <- signZ + 1
            yield! makeWallSignIncZ signX signY signZ 4 "Once you get" "5 in a row," "you win!" ""
            yield! makeWallSignIncZ signX signY signZ 4 "Other game" "modes exist," "learn more" "in the lobby"
            yield! makeWallSignActivate signX signY signZ 4 "Let's play!" "Click to start" END_TUTORIAL_BUTTON true "black"  // TODO multiplayer state testing of tutorial
            signZ <- signZ + 1
            yield U (sprintf "fill %s %s stone" (tut.Offset( 0,0,signZ-TUTORIAL_LOCATION.Z).STR) (tut.Offset(-5,4,signZ-TUTORIAL_LOCATION.Z).STR))
            // first time map is loaded, players go here:
            yield U (sprintf "fill %s %s sea_lantern" NEW_MAP_PLATFORM_LO.STR (NEW_MAP_PLATFORM_LO.Offset(10,0,10).STR))
            let GTL = NEW_PLAYER_PLATFORM_LO.Offset(7,1,4)
            yield! makeSign "standing_sign" (NEW_MAP_PLATFORM_LO.X+7) (NEW_MAP_PLATFORM_LO.Y+1) (NEW_MAP_PLATFORM_LO.Z+3) 4 "Welcome to" "MinecraftBINGO" "by Dr. Brian" "Lorgon111"
            yield! makeSign "standing_sign" (NEW_MAP_PLATFORM_LO.X+7) (NEW_MAP_PLATFORM_LO.Y+1) (NEW_MAP_PLATFORM_LO.Z+4) 4 "This is version" "3.0 Beta" "of the map." ""
            yield! makeSign "standing_sign" (NEW_MAP_PLATFORM_LO.X+7) (NEW_MAP_PLATFORM_LO.Y+1) (NEW_MAP_PLATFORM_LO.Z+5) 4 "Do you have" "the latest" "version?" "Find out!"
            // TODO figure out best URL
            let downloadUrl = "https://twitter.com/MinecraftBINGO"
            let downloadCmd1 = escape2 <| sprintf """tellraw @a {"text":"Press 't' (chat), then click line below to visit the official download page for MinecraftBINGO"}"""
            let downloadCmd2 = escape2 <| sprintf """tellraw @a {"text":"%s","underlined":"true","clickEvent":{"action":"open_url","value":"%s"}}""" downloadUrl downloadUrl
            yield! makeSignDo "standing_sign" (NEW_MAP_PLATFORM_LO.X+7) (NEW_MAP_PLATFORM_LO.Y+1) (NEW_MAP_PLATFORM_LO.Z+6) 4 "Right-click this" "sign to go to" "official site" downloadCmd1 downloadCmd2 true "black"
            yield! makeSignDo "standing_sign" (NEW_MAP_PLATFORM_LO.X+7) (NEW_MAP_PLATFORM_LO.Y+1) (NEW_MAP_PLATFORM_LO.Z+7) 4 "Or right-click" "me to begin" "playing!" (sprintf "tp @p %s 90 180" NEW_PLAYER_LOCATION.STR) "" true "black"
            // new players go here:
            yield U (sprintf "fill %s %s sea_lantern" NEW_PLAYER_PLATFORM_LO.STR (NEW_PLAYER_PLATFORM_LO.Offset(10,0,10).STR))
            let GTL = NEW_PLAYER_PLATFORM_LO.Offset(7,1,4)
            yield! makeSignDo "standing_sign" GTL.X GTL.Y GTL.Z 4 "Right-click" "me to go to" "LOBBY" (sprintf "tp @p %s 0 0" LOBBY_CENTER_LOCATION.STR) "" true "black"
            // Note: there is also a 'Go to Tutorial' sign, but it's coded as part of lobby, to turn it on/off
        |]
    region.PlaceCommandBlocksStartingAtSelfDestruct(TUTORIAL_CMDS,makeTutorialCmds,"build tutorial")






    let uniqueArts = uniqueArts // make an immutable copy
    // per-item Bingo Command storage
    // Y axis - different difficulties
    // X axis - different item subsets
    // Z axis - command: item framex4, (testfor, clear)x4
    let TEAMS = [| "red"; "blue"; "green"; "yellow" |]
    for x = 0 to bingoItems.Length-1 do
        for y = 0 to 2 do
            let dmg, id, art = bingoItems.[x].[y]
            let cmds = [|
                        for team in TEAMS do
                            yield C (sprintf """testfor @a[team=%s] {Inventory:[{id:"minecraft:%s"%s}]}""" team id (if dmg <> -1 then sprintf ",Damage:%ds" dmg else ""))
                            yield C (sprintf """clear @a[team=%s] %s %d 1""" team id dmg)
                        let xx, yy, zz = uniqueArts.[art]
                        yield U (sprintf "execute @e[tag=whereToPlacePixelArt] ~ ~ ~ clone %d %d %d %d %d %d ~ ~ ~" xx yy zz (xx+16) (yy+1) (zz+16))
                       |]
            region.PlaceCommandBlocksStartingAt(BINGO_ITEMS_LOW.Offset(x,y,0),cmds,"itemframe/testfor/clear/copyPixelArt logic")

    let nTimesDo(n) = 
        assert( n > 0 && n < 60 )
        sprintf "execute @e[tag=Z,c=%d] ~ ~ ~" n   // rather than yield same command block N times, can just execute it N times

    let cmdsTickLagDebugger =
        [|
        yield O ""
        yield U "scoreboard objectives add TickInfo dummy"
        yield U "scoreboard players set @e[tag=TickLagDebug] TickInfo 1"
        yield U "stats block ~ ~ ~8 set QueryResult @e[tag=TickLagDebug] TickInfo"
        yield U "worldborder set 10000000"
        yield U "worldborder add 10000000 500000"
        yield U "scoreboard players set PrevTick TickInfo 10000000"
        yield P ""
        yield U "scoreboard players add Tick TickInfo 1"
        yield U "scoreboard players test Tick TickInfo 40 *"
        yield C "scoreboard players set Tick TickInfo 0"
        yield C "worldborder get"
        yield C "scoreboard players operation TempTick TickInfo = @e[tag=TickLagDebug] TickInfo"
        yield C "scoreboard players operation @e[tag=TickLagDebug] TickInfo -= PrevTick TickInfo"
        yield C "scoreboard players operation PrevTick TickInfo = TempTick TickInfo"
        yield C """execute @e[tag=TickLagDebug,score_TickInfo_min=41] ~ ~ ~ tellraw @a[score_TickInfo_min=1] ["ticks are lagging"]"""
        yield U "testforblock ~ ~ ~-7 chain_command_block -1 {SuccessCount:1}"
        yield C """execute @e[tag=TickLagDebug,score_TickInfo=39] ~ ~ ~ tellraw @a[score_TickInfo_min=1] ["ticks are trying to catch up"]"""
        yield U "testforblock ~ ~ ~-9 chain_command_block -1 {SuccessCount:1}"
        yield C """execute @e[tag=TickLagDebug,score_TickInfo_min=40,score_TickInfo=40] ~ ~ ~ tellraw @a[score_TickInfo_min=1] ["ticks are normal"]"""
        |]
    let cmdsnTicksLater =
        [|
        // nTicksLater
        yield P ""
        yield U "scoreboard players add Tick Score 1"
        yield U "scoreboard players add @e[tag=nTicksLaterScoredArmor] S 1"
        yield U "execute @e[tag=nTicksLaterScoredArmor,score_S_min=-1] ~ ~ ~ blockdata ~ ~ ~ {auto:1b}"
        yield U "execute @e[tag=nTicksLaterScoredArmor,score_S_min=-1] ~ ~ ~ blockdata ~ ~ ~ {auto:0b}"
#if DEBUG_NTICKSLATER
        yield U """execute @e[tag=nTicksLaterScoredArmor,score_S_min=-1] ~ ~ ~ tellraw @a ["nTickLater armor-awaken at ",{"score":{"name":"Tick","objective":"Score"}}]"""
#endif
        yield U "kill @e[tag=nTicksLaterScoredArmor,score_S_min=-1]"
        |]
    let timerCmds = 
        [|
            P "scoreboard players add Time S 1"
            U "scoreboard players operation Time S -= TIMER_CYCLE_LENGTH Calc"
            U "scoreboard players test Time S 0 *"
            C "scoreboard players set Time S 0"
            C "scoreboard players operation Time S -= TIMER_CYCLE_LENGTH Calc"
            U "scoreboard players operation Time S += TIMER_CYCLE_LENGTH Calc"
        |]
    let cmdsFindPlayerWhoDroppedMap =
        [|
        yield P "scoreboard players test Time S 0 0"
        yield C "blockdata ~ ~ ~2 {auto:1b}"
        yield C "blockdata ~ ~ ~1 {auto:0b}"
        yield O ""
        yield U "scoreboard players set SomeoneIsMapUpdating S 0"
        yield U "execute @a[tag=playerThatIsMapUpdating] ~ ~ ~ scoreboard players set SomeoneIsMapUpdating S 1"
        // if someone already updating, kill all droppedMap entities
        yield U "scoreboard players test SomeoneIsMapUpdating S 1 1"
        yield C "scoreboard players tag @e[type=Item] add droppedMap {Item:{id:\"minecraft:filled_map\",Damage:0s}}"
        yield C "kill @e[tag=droppedMap]"
        // if no one updating yet, do the main work
        yield U "scoreboard players test SomeoneIsMapUpdating S 0 0"
        yield C "blockdata ~ ~ ~2 {auto:1b}"
        yield C "blockdata ~ ~ ~1 {auto:0b}"
        yield O ""
        // mark all droppedMap entities
        yield U "scoreboard players tag @e[type=Item] add droppedMap {Item:{id:\"minecraft:filled_map\",Damage:0s}}"
        // tag all nearby players as wanting to tp
        yield U "execute @e[tag=droppedMap] ~ ~ ~ scoreboard players tag @a[r=5] add playerThatWantsToUpdate"
        // choose a random one to be the tp'er
        yield U "scoreboard players tag @r[tag=playerThatWantsToUpdate] add playerThatIsMapUpdating"
        // clear the 'wanting' flags
        yield U "scoreboard players tag @a[tag=playerThatWantsToUpdate] remove playerThatWantsToUpdate"
        // start the TP sequence for the chosen guy
        yield U "execute @p[tag=playerThatIsMapUpdating] ~ ~ ~ summon AreaEffectCloud ~ ~ ~ {Tags:[\"whereToTpBackTo\"],Duration:1000}"  // summon now, need to wait a tick to TP
        yield U "testfor @p[tag=playerThatIsMapUpdating]"
        yield C "blockdata ~ ~ ~3 {auto:1b}"
        yield C "blockdata ~ ~ ~2 {auto:0b}"
        yield U "kill @e[tag=droppedMap]"
        // at end, at most one playerThatIsMapUpdating is tagged
        yield O ""   // someone was tagged, do teleport
        yield U "execute @p[tag=playerThatIsMapUpdating] ~ ~ ~ setworldspawn ~ ~ ~"
        yield U """tellraw @a [{"selector":"@p[tag=playerThatIsMapUpdating]"}," is updating the BINGO map"]"""
        yield U "execute @p[tag=playerThatIsMapUpdating] ~ ~ ~ entitydata @e[type=!Player,r=62] {PersistenceRequired:1}"  // preserve mobs
        yield U "tp @e[tag=whereToTpBackTo] @p[tag=playerThatIsMapUpdating]"  // a tick after summoning, tp marker to player, to preserve facing direction
        yield U (sprintf "tp @p[tag=playerThatIsMapUpdating] %s 180 0" MAP_UPDATE_ROOM.STR)
        yield U "particle portal ~ ~ ~ 3 2 3 1 99 @p[tag=playerThatIsMapUpdating]"
        yield U "execute @p[tag=playerThatIsMapUpdating] ~ ~ ~ playsound entity.endermen.teleport @a"
        yield! nTicksLater(30) // TODO adjust timing?
        yield U "tp @p[tag=playerThatIsMapUpdating] @e[tag=whereToTpBackTo]"
        yield U "execute @p[tag=playerThatIsMapUpdating] ~ ~ ~ entitydata @e[type=!Player,r=72] {PersistenceRequired:0}"  // don't leak mobs
        yield U "particle portal ~ ~ ~ 3 2 3 1 99 @p[tag=playerThatIsMapUpdating]"
        yield U "execute @p[tag=playerThatIsMapUpdating] ~ ~ ~ playsound entity.endermen.teleport @a"
        yield U (sprintf "setworldspawn %s" MAP_UPDATE_ROOM.STR)
        yield U "scoreboard players tag @p[tag=playerThatIsMapUpdating] remove playerThatIsMapUpdating"
        yield U "scoreboard players set hasAnyoneUpdatedMap S 1"
        yield U "kill @e[tag=whereToTpBackTo]"
        |]
    let cmdsNoMoreMaps =
        [|
        P "scoreboard players test Time S 2 2"  // this runs at Time 2 to offset it from cmdsFindPlayerWhoDroppedMap, so 'clear' does not 'tp-update'
        C "blockdata ~ ~ ~2 {auto:1b}"
        C "blockdata ~ ~ ~1 {auto:0b}"
        O ""
        U """scoreboard players remove @a hasMaps 1"""
        U """scoreboard players set @a hasMaps 5 {Inventory:[{id:"minecraft:filled_map",Damage:0s}]}"""
        U """give @a[score_hasMaps=0] filled_map 32 0 {display:{Name:"BINGO Card"}}"""
        U """scoreboard players set @a[score_hasMaps=0] hasMaps 5"""   // just in case give it to them but inventory full, keep the delay before giving again
        |]
    let cmdsTriggerHome =
        [|
        P "scoreboard players test Time S 0 0"
        C (sprintf "tp @a[score_home_min=1] %s 180 0" OFFERING_SPOT.STR)
        C "scoreboard players set @a home 0"
        C "scoreboard players enable @a home"  // re-enable for everyone, so even if die in lobby afterward and respawn out in world again, can come back
        |]
    let cmdsTutorialState =
        [|
        P "scoreboard players test Time S 0 0"
        // if anyone in tutorial...
        C "testfor @p[tag=InTutorial]"
        C "blockdata ~ ~ ~2 {auto:1b}"
        C "blockdata ~ ~ ~1 {auto:0b}"
        // THEN...
        O ""
        //   if game not in progress...
        //    - TP any untagged players to tutorial
        //    - tag them
        U "scoreboard players test GameInProgress S 0 0"
        C (sprintf "tp @a[tag=!InTutorial] %s 90 180" TUTORIAL_PLAYER_START.STR)
        U "scoreboard players test GameInProgress S 0 0" // need to retest because above can fail
        C "scoreboard players tag @a add InTutorial"
        //   if game IS in progress...
        //    - spec the tutorial guy
        //    - tp him to lobby
        //    - tell him to wait
        //    - untag him
        U "scoreboard players test GameInProgress S 1 *"
        C "gamemode 3 @a[tag=InTutorial]"
        U "scoreboard players test GameInProgress S 1 *"
        C (sprintf "tp @a[tag=InTutorial] %s 0 0" LOBBY_CENTER_LOCATION.STR)
        U "scoreboard players test GameInProgress S 1 *"
        C """tellraw @a[tag=InTutorial] ["Please wait, a game is currently in progress"]"""
        U "scoreboard players test GameInProgress S 1 *"
        C "scoreboard players tag @a remove InTutorial"
        |]
    let cmdsFindNewPlayers =
        [|
        P "scoreboard players test Time S 0 0"
        C "blockdata ~ ~ ~2 {auto:1b}"
        C "blockdata ~ ~ ~1 {auto:0b}"
        O ""
        U "gamemode 0 @a[tag=!playerHasBeenSeen]"
        U "effect @a[tag=!playerHasBeenSeen] night_vision 9999 1 true"
        U "scoreboard players test HasTheMapEverBeenLoadedBefore Calc 1 1"
        C (sprintf "tp @a[tag=!playerHasBeenSeen] %s 90 180" NEW_PLAYER_LOCATION.STR)
        U "testforblock ~ ~ ~-2 chain_command_block -1 {SuccessCount:0}"
        C (sprintf "tp @a[tag=!playerHasBeenSeen] %s 90 180" NEW_MAP_LOCATION.STR)
        C "scoreboard players set HasTheMapEverBeenLoadedBefore Calc 1"
        U "scoreboard players tag @a[tag=!playerHasBeenSeen] add playerHasBeenSeen"
        |]
    let cmdsOnRespawn =
        [|
        yield P "scoreboard players test Time S 0 0"
        yield C "testfor @p[score_Deaths_min=1]"
        // run prelude
        yield C "BLOCKDATA ON 1"
        yield C "BLOCKDATA OFF 1"
        // run user-commands (firewalled)
        yield C "BLOCKDATA ON 2"
        yield C "BLOCKDATA OFF 2"
        // run coda
        yield C "BLOCKDATA ON 3"
        yield C "BLOCKDATA OFF 3"

        yield O "TAG 1"
        // note that @a[score_d_min=1] will target while on respawn screen, whereas @p[score_d_min=1] targets after resspawn
        // as a result, run @p multiple times
        // tag folks as justRespawned, then call the custom blocks, and people can target @a[tag=justRespawned] as desired (untag after run)
        yield U (sprintf "%s scoreboard players tag @p[score_Deaths_min=1,tag=!justRespawned] add justRespawned" (nTimesDo 8))
        // run customized on-respawn command blocks
        yield U (sprintf "clone %d %d %d %d %d %d ~ ~ ~2" (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+1) (LOBBYZ+2) (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+1) (LOBBYZ+2+NUM_CONFIG_COMMANDS-1)) // todo ensure in sync with lobby

        yield O "TAG 2"  // firewall the user-contributed CCBs, so if they screw them up, it doesn't break program logic
        for _i = 1 to NUM_CONFIG_COMMANDS do
            yield U "say SHOULD BE REPLACED"

        yield O "TAG 3"
        // untag
        yield U "scoreboard players tag @a[score_Deaths_min=1] remove justRespawned"
        // reset deaths of players who respawned
        yield U (sprintf "%s scoreboard players set @p[score_Deaths_min=1] Deaths 0" (nTimesDo 8))
        |]
#if DEBUG
    region.PlaceCommandBlocksStartingAt(100,3,6,cmdsTickLagDebugger,"tick lag debugger")
#endif
    region.PlaceCommandBlocksStartingAt(101,3,10,cmdsnTicksLater,"nTicksLater")
    region.PlaceCommandBlocksStartingAt(102,3,10,timerCmds,"clock every N ticks")
    region.PlaceCommandBlocksStartingAt(NOTICE_DROPPED_MAP_CMDS,cmdsFindPlayerWhoDroppedMap,"notice player that drops map")
    region.PlaceCommandBlocksStartingAt(104,3,10,cmdsNoMoreMaps,"give maps to players without")
    region.PlaceCommandBlocksStartingAt(105,3,10,cmdsTriggerHome,"trigger home checker")
    region.PlaceCommandBlocksStartingAt(106,3,10,cmdsTutorialState,"tutorial state checker")
    region.PlaceCommandBlocksStartingAt(107,3,10,cmdsFindNewPlayers,"cmdsFindNewPlayers")
    region.PlaceCommandBlocksStartingAt(108,3,10,cmdsOnRespawn,"cmdsOnRespawn")


    let cmdsInit1 =
        [|
        yield O ""
        yield U "effect @a night_vision 9999 0 true"
        // world init
        yield U "setworldspawn 3 4 12"
        yield U "gamerule commandBlockOutput false"
        yield U "gamerule doDaylightCycle false"
        yield U "gamerule keepInventory true"
        yield U "gamerule logAdminCommands false"
        yield U "time set 500"
        yield U "weather clear 999999"
        for t in TEAMS do
            yield U (sprintf "scoreboard teams add %s" t)
            yield U (sprintf "scoreboard teams option %s color %s" t (if t="orange" then "gold" else t))  // team color names are weird
        // kill all entities
        yield U "kill @e[type=!Player]"
        // set up scoreboard objectives & initial values
        yield U "scoreboard objectives add hasMaps dummy"
        yield U "scoreboard objectives add Score dummy"
        yield U "scoreboard objectives setdisplay sidebar Score"
        yield U "scoreboard players set @a Score 0"
        yield U "scoreboard objectives add is dummy"
        yield U "scoreboard objectives add S dummy"
        yield U "scoreboard objectives add PlayerSeed trigger"
        yield U "scoreboard objectives add home trigger"
        yield U "scoreboard objectives add Calc dummy"
        yield U "scoreboard objectives add Deaths deathCount"
        yield U "scoreboard players set A Calc 1103515245"
        yield U "scoreboard players set C Calc 12345"
        yield U "scoreboard players set Two Calc 2"
        yield U "scoreboard players set TwoToSixteen Calc 65536"
        yield U "scoreboard players set OneThousand Calc 1000"
        yield U "scoreboard players set TenMillion Calc 10000000"
        yield U "scoreboard players set Twenty Calc 20"
        yield U "scoreboard players set Sixty Calc 60"
        // TODO consider just re-hardcoding TIMER_CYCLE_LENGTH
        yield U "scoreboard players set TIMER_CYCLE_LENGTH Calc 12"  // TODO best default?  Note: lockout seems to require a value of at least 12
        yield U "scoreboard players set Tick Score 0"  // TODO eventually get rid of this, good for debugging
        yield U """summon AreaEffectCloud ~ ~ ~ {Duration:999999,Tags:["TimeKeeper"]}"""
#if DEBUG
        // start ticklagdebug // TODO eventually remove this
        yield U """summon AreaEffectCloud ~ ~ ~ {Duration:999999,Tags:["TickLagDebug"]}"""
        yield U "fill 100 4 6 100 4 16 wool"  // todo coords
        yield U "fill 100 4 6 100 4 16 redstone_block"  // todo coords
#endif
        // start major clocks
        yield U "fill 101 4 10 112 4 10 wool"  // todo coords
        yield U "fill 101 4 10 112 4 10 redstone_block"  // todo coords
        // call part 2
        yield U (sprintf "blockdata %d %d %d {auto:1b}" 4 3 10) // todo coords
        yield U (sprintf "blockdata %d %d %d {auto:0b}" 4 3 10)
        // build lobby
        yield U (sprintf "blockdata %d %d %d {auto:1b}" (LOBBYX-2) LOBBYY LOBBYZ)
        yield U (sprintf "blockdata %d %d %d {auto:0b}" (LOBBYX-2) LOBBYY LOBBYZ)
        // build tutorial
        yield U (sprintf "blockdata %s {auto:1b}" TUTORIAL_CMDS.STR)
        yield U (sprintf "blockdata %s {auto:0b}" TUTORIAL_CMDS.STR)
        // debug stuff
        yield! nTicksLater(3)
        yield U (sprintf """blockdata %d %d %d {Command:"effect @a night_vision 9999 1 true"}""" (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+3) (LOBBYZ+2))
        yield U (sprintf """blockdata %d %d %d {Command:"gamemode 1 @a"}""" (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+3) (LOBBYZ+2+1))
        yield U (sprintf """blockdata %d %d %d {Command:"tellraw @a [{\"selector\":\"@a[tag=justRespawned]\"},\" just respawned\"]"}""" (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+1) (LOBBYZ+2))
        yield U (sprintf """blockdata %d %d %d {Command:"effect @a night_vision 9999 1 true"}""" (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+1) (LOBBYZ+2+1))
        |]
    region.PlaceCommandBlocksStartingAtSelfDestruct(3,3,10,cmdsInit1,"init1 all")
    let cmdsInit2 =
        [|
        yield O ""
        // call part 3
        yield U (sprintf "blockdata %d %d %d {auto:1b}" 5 3 10) // todo coords
        yield U (sprintf "blockdata %d %d %d {auto:0b}" 5 3 10)
        // force every chunk to redraw map
        for x = 0 to 7 do
            for z = 0 to 7 do
                yield U (sprintf "setblock %d %d %d stone" (MAPX+x*16) (MAPY+17) (MAPZ+z*16))
        yield! nTicksLater(20)
        for x = 0 to 7 do
            for z = 0 to 7 do
                yield U (sprintf "setblock %d %d %d air" (MAPX+x*16) (MAPY+17) (MAPZ+z*16))
        // gen a card
        yield U (sprintf "blockdata %s {auto:1b}" RANDOM_SEED_BUTTON.STR)
        yield U (sprintf "blockdata %s {auto:0b}" RANDOM_SEED_BUTTON.STR)
        yield U (sprintf "tp @p %s 0 0" LOBBY_CENTER_LOCATION.STR)
        yield U "clear @p"
        |]
    region.PlaceCommandBlocksStartingAtSelfDestruct(4,3,10,cmdsInit2,"init2 all")
    let cmdsInit3 =
        [|
        yield O ""
        // make AECs for teleportBasedOnScore, e.g. to move N spaces with a score of N
        for i = 60 downto 1 do  // 60 is nice, not too many, divides 1-6, makes the 300 X/Z spawns manageable
            yield U (sprintf "summon AreaEffectCloud %d 1 1 {Duration:999999,Tags:[\"Z\"]}" i)
            yield U "scoreboard players add @e[tag=Z] S 1"
        yield U (sprintf "fill %d %d %d %d %d %d stone" 0 MAPY (MAPZ-1) 127 MAPY (MAPZ-1)) // stone above top row, to prevent shading on top line
        |]
    let teleportBasedOnScore(tagToTp, scorePlayer, scoreObjective, axis) =  // score must have value 0-59 to work, based on init3 code just above
        assert(axis="x" || axis="y" || axis="z")
        [|
            yield U (sprintf "scoreboard players operation @e[tag=Z] S -= %s %s" scorePlayer scoreObjective)
            if axis="x" then
                yield U (sprintf "execute @e[tag=Z,score_S=0] ~ ~ ~ tp @e[tag=%s] ~1 ~ ~" tagToTp)
            if axis="y" then
                yield U (sprintf "execute @e[tag=Z,score_S=0] ~ ~ ~ tp @e[tag=%s] ~ ~1 ~" tagToTp)
            if axis="z" then
                yield U (sprintf "execute @e[tag=Z,score_S=0] ~ ~ ~ tp @e[tag=%s] ~ ~ ~1" tagToTp)
            yield U (sprintf "scoreboard players operation @e[tag=Z] S += %s %s" scorePlayer scoreObjective)
        |]
    region.PlaceCommandBlocksStartingAtSelfDestruct(5,3,10,cmdsInit3,"init3 all")
    // ensure there is an empty command block at each SPAWN_LOCATION_COMMANDS, since it gets cloned in, and cloning air breaks the chain
    for t = 0 to 3 do
        region.PlaceCommandBlocksStartingAt(SPAWN_LOCATION_COMMANDS(t),[|U"";U"";U""|],"ensure spawn cmd blocks")


    //////////////////////////////////////////////
    // generate actual card in the sky
    //////////////////////////////////////////////
    let makeActualCardInit() =
        [|
        yield U "scoreboard players set macCol S 1"
        yield U (sprintf "summon ArmorStand %d %d %d {NoGravity:1,Tags:[\"whereToPlacePixelArt\"]}" (MAPX+7) MAPY (MAPZ+3)) 
        |]
    let makeActualCard() =
        [|
        // find one block to clone to 1 1 1 
        yield U "scoreboard players operation @e[tag=bingoItem] S -= next S"
        yield U "scoreboard players test which S 0 0"
        yield C     "execute @e[tag=bingoItem,score_S_min=0,score_S=0] ~ ~2 ~8 clone ~ ~ ~ ~ ~ ~ 1 1 1"
        yield U "scoreboard players test which S 1 1"
        yield C     "execute @e[tag=bingoItem,score_S_min=0,score_S=0] ~ ~1 ~8 clone ~ ~ ~ ~ ~ ~ 1 1 1"
        yield U "scoreboard players test which S 2 2"
        yield C     "execute @e[tag=bingoItem,score_S_min=0,score_S=0] ~ ~0 ~8 clone ~ ~ ~ ~ ~ ~ 1 1 1"
        yield U "scoreboard players operation @e[tag=bingoItem] S += next S"
        yield U "clone 1 1 1 1 1 1 ~ ~ ~1"
        yield U "say THIS SHOULD HAVE BEEN REPLACED"
        // move next spot on board
        yield U "tp @e[tag=whereToPlacePixelArt] ~24 ~ ~"
        yield U "scoreboard players add macCol S 1"
        yield U "scoreboard players test macCol S 6 *"
        yield C "scoreboard players set macCol S 1"
        yield C "tp @e[tag=whereToPlacePixelArt] ~-120 ~ ~24"
        |]
    let makeActualCardCleanup() =
        [|
        yield C "kill @e[tag=whereToPlacePixelArt]"
        |]


    /////////////////////// 
    // choose seed
    ///////////////////////
    let resetScoresLogic =
        [|
        yield O ""
        // turn off check-for-item-checkers
        for t = 0 to 3 do
            let lo = ITEM_CHECKERS_REDSTONE_LOW(t)
            yield U (sprintf "fill %d %d %d %s wool" lo.X lo.Y (lo.Z-1) (ITEM_CHECKERS_REDSTONE_HIGH t).STR)
        // turn off timekeeper
        yield U (sprintf "setblock %s wool" TIMEKEEPER_REDSTONE.STR)
        yield U (sprintf "setblock %s wool" TIMEKEEPER_25MIN_REDSTONE.STR)
        // bring everyone back to lobby in survival if game just ended
        yield U "scoreboard players test GameInProgress S 1 *"
        yield C (sprintf "tp @a %s 0 0" LOBBY_CENTER_LOCATION.STR)
        yield C "gamemode 0 @a"
        yield C "spawnpoint @a"
        // note previous game has finished
        yield U "scoreboard players set GameInProgress S 0"
        // clear player scores
        yield U "scoreboard players set @a Score 0"
        yield U "scoreboard players reset Time Score"
        yield U "scoreboard players reset Minutes Score"
        yield U "scoreboard players reset Seconds Score"
        // clear bingo information
        for t = 0 to 3 do
            let team = TEAMS.[t]
            for x = 0 to 4 do
                yield U (sprintf "scoreboard players set %s_x%d S 0" team x)
            for y = 0 to 4 do
                yield U (sprintf "scoreboard players set %s_y%d S 0" team y)
            yield U (sprintf "scoreboard players set %s_d0 S 0" team)
            yield U (sprintf "scoreboard players set %s_d1 S 0" team)
            yield U (sprintf "setblock %s wool" (GOT_BINGO_REDSTONE(t).STR))
            yield U (sprintf "setblock %s wool" (GOT_LOCKOUT_REDSTONE(t).STR))
            yield U (sprintf "setblock %s wool" (GOT_MEGA_BINGO_REDSTONE(t).STR))
        // ensure long-lived AECs stay alive
        yield U "entitydata @e[type=AreaEffectCloud] {Duration:999999}"
        |]
    region.PlaceCommandBlocksStartingAt(RESET_SCORES_LOGIC,resetScoresLogic,"reset scores")

    let randomSeedButton =
        [|
        yield O ""
        // cancel out any pending seed choice
        yield U (sprintf "setblock %s wool" CHOOSE_SEED_REDSTONE.STR)
        yield U (sprintf "blockdata %s {auto:1b}" RESET_SCORES_LOGIC.STR)
        yield U (sprintf "blockdata %s {auto:0b}" RESET_SCORES_LOGIC.STR)
        yield U "scoreboard players operation Z Calc += @r[type=AreaEffectCloud,tag=Z] S"  // this will insert some 'real' randomness by adding rand(60) before re-PRNG
        yield U """tellraw @a ["Choosing random seed..."]"""
        yield U "scoreboard players set modRandomSeed S 899"
        yield! PRNG("seed","is","modRandomSeed","S")
        yield U "scoreboard players set modRandomSeed S 999"
        yield! PRNG("tmp","is","modRandomSeed","S")
        yield U "scoreboard players operation seed is *= OneThousand Calc"
        yield U "scoreboard players operation seed is += tmp is"
        yield U "scoreboard players add seed is 100000"
        yield U (sprintf "fill %d %d %d %d %d %d clay 0 replace stained_hardened_clay" (MAPX+4) MAPY MAPZ (MAPX+122) MAPY (MAPZ+118))
        yield U (sprintf "fill %d %d %d %d %d %d clay 0 replace emerald_block" (MAPX+4) MAPY MAPZ (MAPX+122) MAPY (MAPZ+118))
        yield U "scoreboard players operation Seed Score = seed is"
        yield U "scoreboard players operation Z Calc = seed is"
        yield U "scoreboard players set seed is -2147483648"
        yield U (sprintf "blockdata %s {auto:1b}" MAKE_SEEDED_CARD.STR)
        yield U (sprintf "blockdata %s {auto:0b}" MAKE_SEEDED_CARD.STR)
        |]
    region.PlaceCommandBlocksStartingAt(RANDOM_SEED_BUTTON,randomSeedButton,"random seed")

    let chooseSeedButton =
        [|
        yield O ""
        yield U (sprintf "blockdata %s {auto:1b}" RESET_SCORES_LOGIC.STR)
        yield U (sprintf "blockdata %s {auto:0b}" RESET_SCORES_LOGIC.STR)
        // select seed and generate
        yield U "scoreboard players set seed is -2147483648"
        yield U "scoreboard players set @a PlayerSeed -2147483648"
        yield U "scoreboard players enable @a PlayerSeed"
        yield U """tellraw @a {"text":"Press 't' (chat), click below, then replace NNN with a seed number in chat"}"""
        yield U """tellraw @a {"text":"CLICK HERE","clickEvent":{"action":"suggest_command","value":"/trigger PlayerSeed set NNN"}}"""
        // clear card of colors _after_ the tellraw (as commands below can lag a few seconds)
        yield U (sprintf "fill %d %d %d %d %d %d clay 0 replace stained_hardened_clay" (MAPX+4) MAPY MAPZ (MAPX+122) MAPY (MAPZ+118))
        yield U (sprintf "fill %d %d %d %d %d %d clay 0 replace emerald_block" (MAPX+4) MAPY MAPZ (MAPX+122) MAPY (MAPZ+118))
        yield U (sprintf "setblock %s wool" CHOOSE_SEED_REDSTONE.STR)
        yield U (sprintf "setblock %s redstone_block" CHOOSE_SEED_REDSTONE.STR)
        |]
    region.PlaceCommandBlocksStartingAt(CHOOSE_SEED_BUTTON,chooseSeedButton,"choose seed")
    let chooseSeedCoda =
        [|
        yield P ""
        yield U "execute @a[score_PlayerSeed_min=-2147483647] ~ ~ ~ scoreboard players operation seed is = @p[score_PlayerSeed_min=-2147483647] PlayerSeed"
        yield U "scoreboard players test seed is -2147483647 *"
        yield C (sprintf "setblock %s wool" CHOOSE_SEED_REDSTONE.STR)
        yield C "scoreboard players set @a PlayerSeed -2147483648"
        yield C "scoreboard players operation Seed Score = seed is"
        yield C "scoreboard players operation Z Calc = seed is"
        yield C "scoreboard players set seed is -2147483648"
        yield C (sprintf "blockdata %s {auto:1b}" MAKE_SEEDED_CARD.STR)
        yield C (sprintf "blockdata %s {auto:0b}" MAKE_SEEDED_CARD.STR)
        |]
    region.PlaceCommandBlocksStartingAt(CHOOSE_SEED_REDSTONE.Offset(0,0,1),chooseSeedCoda,"choose seed coda")

    ///////////////////////
    // start game    
    ///////////////////////
    let ensureCardUpdated(whereToTpBack:Coords) =
        [|
            yield U "scoreboard players tag @p add oneGuyToEnsureBingoCardCleared"
            yield U (sprintf "blockdata %s {auto:1b}" ENSURE_CARD_UPDATED_LOGIC.STR)
            yield U (sprintf "blockdata %s {auto:0b}" ENSURE_CARD_UPDATED_LOGIC.STR)
            yield! nTicksLater(33)
            yield U (sprintf "tp @p[tag=oneGuyToEnsureBingoCardCleared] %s 90 180" whereToTpBack.STR) 
            yield U "scoreboard players tag @a remove oneGuyToEnsureBingoCardCleared"
        |]
    let startGameButtonPart1 =
        [|
        yield O ""
        // ensure that people have joined teams
        for t in TEAMS do
            yield U (sprintf "scoreboard players set %sCount S 0" t)
        yield U "scoreboard players set teamCount S 0"
        for t in TEAMS do
            yield U (sprintf "execute @a[team=%s] ~ ~ ~ scoreboard players add %sCount S 1" t t)
            yield U (sprintf "execute @p[team=%s] ~ ~ ~ scoreboard players add teamCount S 1" t)
        // if none, error out
        yield U "scoreboard players test teamCount S * 0"
        yield C """tellraw @a ["No one has joined a team - join a team color to play!"]"""
        // else continue...
        yield U "scoreboard players test teamCount S 1 *"
        yield C "blockdata ~ ~ ~2 {auto:1b}"
        yield C "blockdata ~ ~ ~1 {auto:0b}"
        yield O ""
        // turn off dropped-map checker
        yield U (sprintf "setblock %s wool" (NOTICE_DROPPED_MAP_CMDS.Offset(0,1,0).STR))
        // cancel out any pending seed choice
        yield U (sprintf "setblock %s wool" CHOOSE_SEED_REDSTONE.STR)
        // clear player scores again (in case player joined server after card gen'd)
        yield U "scoreboard players reset * Score"
        yield U "scoreboard players set @a Score 0"
        // set up lockout goal if lockout mode selected (teamCount 2/3/4 -> goal 13/9/7)
        yield U "scoreboard players test isLockoutMode S 1 *"
        yield C "scoreboard players test teamCount S 1 1"
        yield C "scoreboard players set LockoutGoal Score 25"
        yield U "scoreboard players test isLockoutMode S 1 *"
        yield C "scoreboard players test teamCount S 2 2"
        yield C "scoreboard players set LockoutGoal Score 13"
        yield U "scoreboard players test isLockoutMode S 1 *"
        yield C "scoreboard players test teamCount S 3 3"
        yield C "scoreboard players set LockoutGoal Score 9"
        yield U "scoreboard players test isLockoutMode S 1 *"
        yield C "scoreboard players test teamCount S 4 4"
        yield C "scoreboard players set LockoutGoal Score 7"
        // disable other buttons
        yield U (sprintf "blockdata %d %d %d {auto:1b}" (LOBBYX-3) LOBBYY LOBBYZ)
        yield U (sprintf "blockdata %d %d %d {auto:0b}" (LOBBYX-3) LOBBYY LOBBYZ)
        // note game in progress
        yield U "scoreboard players set GameInProgress S 1"
        // put folks in survival mode, feed & heal, remove all xp, clear inventories
        yield U "gamemode s @a"
        yield U "effect @a saturation 10 4 true"
        yield U "effect @a regeneration 10 4 true"
        yield U "xp -2147483648L @a"
        yield U "clear @a"
        // fill inv with maps to ensure cleared (map updated)
        yield! ensureCardUpdated(LOBBY_CENTER_LOCATION)
        // give player all the effects
        yield U "effect @a slowness 999 127 true"
        yield U "effect @a mining_fatigue 999 7 true"
        yield U "effect @a jump_boost 999 150 true"
        yield U "effect @a resistance 999 4 true"
        yield U "effect @a water_breathing 999 4 true"
        yield U "effect @a invisibility 999 4 true"
        // set time to day so not tp at night
        yield U "time set 0"
        yield U "scoreboard players set folksSentToWaitingRoom S 0"
        // do seeded spawn points
        yield U "scoreboard players operation Z Calc = Seed Score"
        yield U (sprintf "blockdata %s {auto:1b}" TELEPORT_PLAYERS_TO_SEEDED_SPAWN_LOW.STR)
        yield U (sprintf "blockdata %s {auto:0b}" TELEPORT_PLAYERS_TO_SEEDED_SPAWN_LOW.STR)
        |]
    let startGameButtonPart2 =
        [|
        yield O ""
        // feed & heal again
        yield U "effect @a saturation 10 4 true"
        yield U "effect @a regeneration 10 4 true"
        // clear hostile mobs
        yield U "difficulty 0"
        yield U "blockdata ~ ~ ~2 {auto:1b}"
        yield U "blockdata ~ ~ ~1 {auto:0b}"
        yield O ""
        yield! nTicksLater(40)
        yield U "difficulty 2"
        // start check-for-bingo-items checkers
        for t = 0 to 3 do
            let team = TEAMS.[t]
            yield U (sprintf "scoreboard players test %sCount S 1 *" team)
            yield C (sprintf "fill %s %s stone" (ITEM_CHECKERS_REDSTONE_LOW t).STR (ITEM_CHECKERS_REDSTONE_HIGH t).STR) // Note: 'stone' because 'wool' may fail (0 updates) after blackout
            yield C (sprintf "fill %s %s redstone_block" (ITEM_CHECKERS_REDSTONE_LOW t).STR (ITEM_CHECKERS_REDSTONE_HIGH t).STR)
            // and re-tp anyone who maybe moved, the cheaters!
            let tpxCmd = SPAWN_LOCATION_COMMANDS(t)
            let tpzCmd = SPAWN_LOCATION_COMMANDS(t).Offset(0,0,1)
            let tpyCmd = SPAWN_LOCATION_COMMANDS(t).Offset(0,0,2)
            yield U "scoreboard players tag @a[tag=oneGuyToTeleport] remove oneGuyToTeleport"
            yield U (sprintf "scoreboard players tag @a[team=%s] add oneGuyToTeleport" team)
            yield U (sprintf "clone %s %s ~ ~ ~1" tpxCmd.STR tpxCmd.STR)
            yield U "say THIS SHOULD HAVE BEEN REPLACED"
            yield U (sprintf "clone %s %s ~ ~ ~1" tpzCmd.STR tpzCmd.STR)
            yield U "say THIS SHOULD HAVE BEEN REPLACED"
            yield U (sprintf "clone %s %s ~ ~ ~1" tpyCmd.STR tpyCmd.STR)
            yield U "say THIS SHOULD HAVE BEEN REPLACED"
        yield U """tellraw @a ["Game will begin shortly... countdown commencing..."]"""
        yield! nTicksLater(20)
        yield U """tellraw @a ["3"]"""
        yield U "execute @a ~ ~ ~ playsound block.note.harp @p ~ ~ ~ 1 0.6"
        yield! nTicksLater(20)
        yield U """tellraw @a ["2"]"""
        yield U "execute @a ~ ~ ~ playsound block.note.harp @p ~ ~ ~ 1 0.6"
        yield! nTicksLater(20)
        yield U """tellraw @a ["1"]"""
        yield U "execute @a ~ ~ ~ playsound block.note.harp @p ~ ~ ~ 1 0.6"
        yield! nTicksLater(20)
        // once more, re-tp anyone who maybe moved, the cheaters!
        for t = 0 to 3 do
            let team = TEAMS.[t]
            let tpxCmd = SPAWN_LOCATION_COMMANDS(t)
            let tpzCmd = SPAWN_LOCATION_COMMANDS(t).Offset(0,0,1)
            let tpyCmd = SPAWN_LOCATION_COMMANDS(t).Offset(0,0,2)
            yield U "scoreboard players tag @a[tag=oneGuyToTeleport] remove oneGuyToTeleport"
            yield U (sprintf "scoreboard players tag @a[team=%s] add oneGuyToTeleport" team)
            yield U (sprintf "clone %s %s ~ ~ ~1" tpxCmd.STR tpxCmd.STR)
            yield U "say THIS SHOULD HAVE BEEN REPLACED"
            yield U (sprintf "clone %s %s ~ ~ ~1" tpzCmd.STR tpzCmd.STR)
            yield U "say THIS SHOULD HAVE BEEN REPLACED"
            yield U (sprintf "clone %s %s ~ ~ ~1" tpyCmd.STR tpyCmd.STR)
            yield U "say THIS SHOULD HAVE BEEN REPLACED"
        yield U "scoreboard players set hasAnyoneUpdatedMap S 0"
        yield U "time set 0"
        yield U "effect @a clear"
        yield U """tellraw @a ["Start! Go!!!"]"""
        yield U "execute @a ~ ~ ~ playsound block.note.harp @p ~ ~ ~ 1 1.2"
        // start worldborder timer
        yield U "worldborder set 10000000"
        yield U "worldborder add 10000000 500000"
        yield U (sprintf "setblock %s redstone_block" TIMEKEEPER_REDSTONE.STR)
        // enable triggers (for click-in-chat-to-tp-home stuff)
        yield U "scoreboard players set @a home 0"
        yield U "scoreboard players enable @a home"
        // option to get back
        yield U """tellraw @a ["(If you need quit before getting BINGO, you can"]"""
        yield U """tellraw @a [{"underlined":"true","text":"press 't' (chat), then click this line to return to the lobby)","clickEvent":{"action":"run_command","value":"/trigger home set 1"}}]"""
        // turn on dropped-map checker
        yield U (sprintf "setblock %s redstone_block" (NOTICE_DROPPED_MAP_CMDS.Offset(0,1,0).STR))
        // prep for customized on-respawn command blocks
        yield U "scoreboard players set @a Deaths 0"
        // run customized on-start command blocks
        yield U (sprintf "clone %d %d %d %d %d %d ~ ~ ~1" (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+3) (LOBBYZ+2) (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+3) (LOBBYZ+2+NUM_CONFIG_COMMANDS-1)) // todo ensure in sync with lobby
        for _i = 1 to NUM_CONFIG_COMMANDS do
            yield U "say SHOULD BE REPLACED"
        // NOTE, customized on-start commands must be last, to firewall them
        |]
    region.PlaceCommandBlocksStartingAt(START_GAME_PART_1,startGameButtonPart1,"start game1")
    region.PlaceCommandBlocksStartingAt(START_GAME_PART_2,startGameButtonPart2,"start game2")

    let showItemsButton =
        [|
            let x,y,z = (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+5), (LOBBYY+2), (LOBBYZ+9)
            yield O ""
            yield U (sprintf "setblock %d %d %d chest" (x+1) (y-1) z)
            yield U (sprintf "blockdata %d %d %d %s" (x+1) (y-1) z anyDifficultyChest)
            yield U (sprintf "setblock %d %d %d chest" (x+3) (y-1) z)
            yield U (sprintf "blockdata %d %d %d %s" (x+3) (y-1) z otherChest1)
            yield U (sprintf "setblock %d %d %d chest" (x+5) (y-1) z)
            yield U (sprintf "blockdata %d %d %d %s" (x+5) (y-1) z otherChest2)
        |]
    region.PlaceCommandBlocksStartingAt(SHOW_ITEMS_BUTTON,showItemsButton,"show items button")
    let toggleLockoutButton =
        [|
            O ""
            U "scoreboard players test isLockoutMode S 1 *"
            C "scoreboard players set isLockoutMode S 0"
            C "scoreboard players reset LockoutGoal Score"
            U "testforblock ~ ~ ~-2 chain_command_block -1 {SuccessCount:0}"
            C "scoreboard players set isLockoutMode S 1"
            C "scoreboard players set LockoutGoal Score -1"
        |]
    region.PlaceCommandBlocksStartingAt(TOGGLE_LOCKOUT_BUTTON,toggleLockoutButton,"toggle lockout button")
    let endTutorialButton =
        [|
            yield O ""
            // turn off check-for-item-checkers
            for t = 0 to 3 do
                let lo = ITEM_CHECKERS_REDSTONE_LOW(t)
                yield U (sprintf "fill %d %d %d %s wool" lo.X lo.Y (lo.Z-1) (ITEM_CHECKERS_REDSTONE_HIGH t).STR)
            // auto-gen a new card now; tp all to lobby, remove all InTutorial tags
            yield U (sprintf "tp @a %s 0 0" LOBBY_CENTER_LOCATION.STR)
            yield U "scoreboard players tag @a remove InTutorial"
            yield U (sprintf "blockdata %s {auto:1b}" RANDOM_SEED_BUTTON.STR)
            yield U (sprintf "blockdata %s {auto:0b}" RANDOM_SEED_BUTTON.STR)
        |]
    region.PlaceCommandBlocksStartingAt(END_TUTORIAL_BUTTON,endTutorialButton,"end tutorial button")
    let startTutorialButton =
        [|
            yield O ""
            // reset scores
            yield U """tellraw @a ["One moment, tutorial being initialized..."]"""
            yield U "time set 500"
            yield U (sprintf "blockdata %s {auto:1b}" RESET_SCORES_LOGIC.STR)
            yield U (sprintf "blockdata %s {auto:0b}" RESET_SCORES_LOGIC.STR)
            yield U "clear @a"
            yield! nTicksLater(2)
            // turn on check-for-item-checkers
            for t = 0 to 3 do
                let lo = ITEM_CHECKERS_REDSTONE_LOW(t)
                yield U (sprintf "fill %d %d %d %s redstone_block" lo.X lo.Y (lo.Z-1) (ITEM_CHECKERS_REDSTONE_HIGH t).STR)
            // do tutorial start stuff
            yield U "scoreboard teams join red @a"
            yield U "scoreboard players tag @a add InTutorial"
            yield U "gamemode 0 @a"
            yield U (sprintf "setblock %s stone" (TUTORIAL_LOCATION.Offset(1,0,17).STR))
            yield U (sprintf "setblock %s stone" (TUTORIAL_LOCATION.Offset(0,-1,17).STR))
            yield U (sprintf "setblock %s water" (TUTORIAL_LOCATION.Offset(0,0,17).STR))
            yield U (sprintf "setblock %s dirt"  (TUTORIAL_LOCATION.Offset(-1,0,17).STR))
            yield U (sprintf "setblock %s reeds" (TUTORIAL_LOCATION.Offset(-1,1,17).STR))
            yield U (sprintf "setblock %s reeds" (TUTORIAL_LOCATION.Offset(-1,2,17).STR))
            yield U "scoreboard players set Seed Score 447960"
            yield U "scoreboard players set Z Calc 447960"
            yield U (sprintf "blockdata %s {auto:1b}" MAKE_SEEDED_CARD.STR)
            yield U (sprintf "blockdata %s {auto:0b}" MAKE_SEEDED_CARD.STR)
            yield! nTicksLater(55)
            yield! ensureCardUpdated(TUTORIAL_PLAYER_START)
            yield U (sprintf "tp @a %s 90 180" TUTORIAL_PLAYER_START.STR) 
        |]
    region.PlaceCommandBlocksStartingAt(START_TUTORIAL_BUTTON,startTutorialButton,"start tutorial button")
    let ensureCardUpdatedLogic = // tp oneGuyToEnsureBingoCardCleared to center and update
        [|
            yield O ""
            yield U "clear @p[tag=oneGuyToEnsureBingoCardCleared]"
            yield U (sprintf "%s give @p[tag=oneGuyToEnsureBingoCardCleared] filled_map 64 0" (nTimesDo 9))
            yield U (sprintf "tp @p[tag=oneGuyToEnsureBingoCardCleared] %s 0 0" LOBBY_CENTER_LOCATION.STR)
            yield! nTicksLater(30)
            yield U "clear @p[tag=oneGuyToEnsureBingoCardCleared]"
        |]
    region.PlaceCommandBlocksStartingAt(ENSURE_CARD_UPDATED_LOGIC,ensureCardUpdatedLogic,"ensureCardUpdatedLogic")


    ///////////////////////
    // "constantly checking for getting bingo items" bit
    ///////////////////////
    
    // team got-item-checker framework
    for t = 0 to 3 do
        let team = TEAMS.[t]
        let color = match team with "red" -> "stained_hardened_clay 14" | "blue" -> "stained_hardened_clay 11" | "yellow" -> "stained_hardened_clay 4" | "green" -> "emerald_block 0" 
        for x = 0 to 4 do
            for y = 0 to 4 do
                let checkerCmds = 
                    [|
                    yield P (sprintf "scoreboard players test Time S %d %d" (t*3) (t*3))    // offset 4 teams checkers, both to avoid lag spikes, and to allow lockout to avoid simultaneous get
                    yield U "say REPLACE ME testfor"
                    yield U "say REPLACE ME clear"
                    yield C "blockdata ~ ~ ~2 {auto:1b}"
                    yield C "blockdata ~ ~ ~1 {auto:0b}"
                    yield O "" // separate out to have as few blocks 'under' the purples as possible
                    // call common got-an-item logic (at most once per tick)
                    yield U (sprintf "blockdata %s {auto:1b}" (GOT_AN_ITEM_COMMON_LOGIC t).STR)
                    yield U (sprintf "blockdata %s {auto:0b}" (GOT_AN_ITEM_COMMON_LOGIC t).STR)
                    // do specific-to-this-item logic (repeat for each item, even if in same tick)
                    let lo = ITEM_CHECKERS_REDSTONE_LOW(t).Offset(x,y,0)
                    yield C (sprintf "fill %d %d %d %s %s" lo.X lo.Y (lo.Z-1) lo.STR color)  // the two-consecutive-tick issue doesn't apply as a result of the purple condition if TIMER_CYCLE_LENGTH > 2
                    //  - lockout logic:  can use setblock abs coords to set the 4 wools, can set to 'my' color
                    yield U "scoreboard players test isLockoutMode S 1 *"
                    for otherTeam = 0 to 3 do
                        if otherTeam <> t then
                            let lo = ITEM_CHECKERS_REDSTONE_LOW(otherTeam).Offset(x,y,0)
                            yield C (sprintf "fill %d %d %d %s %s" lo.X lo.Y (lo.Z-1) lo.STR color)  // the two-consecutive-tick issue doesn't apply as a result of the purple condition if TIMER_CYCLE_LENGTH > 2
                    yield U (sprintf "scoreboard players add @a[team=%s] Score 1" team)
                    // if singleplayer or lockout, fill full square
                    yield U "scoreboard players test teamCount S 1 1"
                    yield C (sprintf "fill %d %d %d %d %d %d %s replace clay" (MAPX+4+x*24) (MAPY) (MAPZ+(4-y)*24) (MAPX+4+x*24+22) (MAPY) (MAPZ+(4-y)*24+22) color)
                    yield U "scoreboard players test isLockoutMode S 1 *"
                    yield C (sprintf "fill %d %d %d %d %d %d %s replace clay" (MAPX+4+x*24) (MAPY) (MAPZ+(4-y)*24) (MAPX+4+x*24+22) (MAPY) (MAPZ+(4-y)*24+22) color)
                    // else fill team corner
                    yield U "scoreboard players test isLockoutMode S 0 0"
                    yield C "scoreboard players test teamCount S 2 *"
                    yield C (match t with
                               | 0 -> sprintf "fill %d %d %d %d %d %d %s replace clay" (MAPX+4+x*24) (MAPY) (MAPZ+(4-y)*24) (MAPX+4+x*24+11) (MAPY) (MAPZ+(4-y)*24+10) color
                               | 1 -> sprintf "fill %d %d %d %d %d %d %s replace clay" (MAPX+4+x*24+12) (MAPY) (MAPZ+(4-y)*24) (MAPX+4+x*24+22) (MAPY) (MAPZ+(4-y)*24+10) color
                               | 2 -> sprintf "fill %d %d %d %d %d %d %s replace clay" (MAPX+4+x*24) (MAPY) (MAPZ+(4-y)*24+11) (MAPX+4+x*24+11) (MAPY) (MAPZ+(4-y)*24+22) color
                               | 3 -> sprintf "fill %d %d %d %d %d %d %s replace clay" (MAPX+4+x*24+12) (MAPY) (MAPZ+(4-y)*24+11) (MAPX+4+x*24+22) (MAPY) (MAPZ+(4-y)*24+22) color
                               )
                    // update win condition scores
                    yield U (sprintf "scoreboard players add %s_x%d S 1" team x)
                    yield U (sprintf "scoreboard players add %s_y%d S 1" team y)
                    if x = y then
                        yield U (sprintf "scoreboard players add %s_d0 S 1" team)
                    if x = 4-y then
                        yield U (sprintf "scoreboard players add %s_d1 S 1" team)
                    |]
                region.PlaceCommandBlocksStartingAt(ITEM_CHECKERS_REDSTONE_LOW(t).Offset(x,y,1),checkerCmds,"team 5x5 bingo play")
        let gotAnItem =
            [|
            yield O ""
            yield U (sprintf """tellraw @a [{"selector":"@a[team=%s]"}," got an item! (",{"score":{"name":"@p[team=%s]","objective":"Score"}}," in ",{"score":{"name":"Time","objective":"Score"}},"s)"]""" team team)
            yield U "scoreboard players test hasAnyoneUpdatedMap S 0 0"
            yield C """tellraw @a ["To update the BINGO map, drop one copy on the ground"]"""
            yield U "execute @a ~ ~ ~ playsound entity.firework.launch @p ~ ~ ~"
            // check for win
            for x = 0 to 4 do
                yield U (sprintf "scoreboard players test %s_x%d S 5 *" team x)
                yield C (sprintf "setblock %s redstone_block" (GOT_BINGO_REDSTONE(t).STR))
            for y = 0 to 4 do
                yield U (sprintf "scoreboard players test %s_y%d S 5 *" team y)
                yield C (sprintf "setblock %s redstone_block" (GOT_BINGO_REDSTONE(t).STR))
            yield U (sprintf "scoreboard players test %s_d0 S 5 *" team)
            yield C (sprintf "setblock %s redstone_block" (GOT_BINGO_REDSTONE(t).STR))
            yield U (sprintf "scoreboard players test %s_d1 S 5 *" team)
            yield C (sprintf "setblock %s redstone_block" (GOT_BINGO_REDSTONE(t).STR))
            yield U (sprintf "execute @p[team=%s,score_Score_min=25] ~ ~ ~ setblock %s redstone_block" team (GOT_MEGA_BINGO_REDSTONE(t).STR))
            // check for lockout
            yield U (sprintf "scoreboard players operation TempScore S = @p[team=%s] Score" team)
            yield U (sprintf "scoreboard players operation TempScore S -= LockoutGoal Score")
            yield U (sprintf "scoreboard players test TempScore S 0 *")
            yield C (sprintf "scoreboard players test isLockoutMode S 1 *")
            yield C (sprintf "setblock %s redstone_block" (GOT_LOCKOUT_REDSTONE(t).STR))
            |]
        region.PlaceCommandBlocksStartingAt(GOT_AN_ITEM_COMMON_LOGIC(t),gotAnItem,"team got-an-item")
        let gotBingo =
            [|
            yield O ""
            yield U (sprintf """tellraw @a [{"selector":"@a[team=%s]"}," got BINGO!"]""" team)
            yield U (sprintf "blockdata %s {auto:1b}" GOT_WIN_COMMON_LOGIC.STR)
            yield U (sprintf "blockdata %s {auto:0b}" GOT_WIN_COMMON_LOGIC.STR)
            |]
        region.PlaceCommandBlocksStartingAt(GOT_BINGO_REDSTONE(t).Offset(0,0,1),gotBingo,"team got bingo")
        let gotLockout =
            [|
            yield O ""
            yield U (sprintf """tellraw @a [{"selector":"@a[team=%s]"}," got the lockout goal!"]""" team)
            yield U (sprintf "blockdata %s {auto:1b}" GOT_WIN_COMMON_LOGIC.STR)
            yield U (sprintf "blockdata %s {auto:0b}" GOT_WIN_COMMON_LOGIC.STR)
            |]
        region.PlaceCommandBlocksStartingAt(GOT_LOCKOUT_REDSTONE(t).Offset(0,0,1),gotLockout,"team got lockout")
        let gotMegaBingo =
            [|
            yield O ""
            yield U (sprintf """tellraw @a [{"selector":"@a[team=%s]"}," got MEGA-BINGO!"]""" team)
            yield U (sprintf "blockdata %s {auto:1b}" GOT_WIN_COMMON_LOGIC.STR)
            yield U (sprintf "blockdata %s {auto:0b}" GOT_WIN_COMMON_LOGIC.STR)
            |]
        region.PlaceCommandBlocksStartingAt(GOT_MEGA_BINGO_REDSTONE(t).Offset(0,0,1),gotMegaBingo,"team got mega bingo")
    let gotAWinCommonLogic =
        [|
        yield O ""
        // put time on scoreboard
        yield U "scoreboard players operation Minutes S = Time Score"
        yield U "scoreboard players operation Minutes S /= Sixty Calc"
        yield U "scoreboard players operation Seconds S = Time Score"
        yield U "scoreboard players operation Seconds S %= Sixty Calc"
        yield U "scoreboard players set Minutes Score 0"
        yield U "scoreboard players set Seconds Score 0"
        yield U "scoreboard players operation Minutes Score -= Minutes S"
        yield U "scoreboard players operation Seconds Score -= Seconds S"
        // option to return to lobby
        yield U """tellraw @a ["You can keep playing, or"]"""
        yield U """tellraw @a [{"underlined":"true","text":"press 't' (chat), then click this line to return to the lobby","clickEvent":{"action":"run_command","value":"/trigger home set 1"}}]"""
        // fireworks
        yield U """execute @a ~ ~ ~ summon FireworksRocketEntity ~3 ~0 ~0 {LifeTime:20,FireworksItem:{id:401,Count:1,tag:{Fireworks:{Explosions:[{Type:0,Flicker:0,Trail:0,Colors:[16730395,1796095,5177112],FadeColors:[16777215]},]}}}}"""
        yield! nTicksLater(8)
        yield U """execute @a ~ ~ ~ summon FireworksRocketEntity ~0 ~0 ~3 {LifeTime:20,FireworksItem:{id:401,Count:1,tag:{Fireworks:{Explosions:[{Type:1,Flicker:0,Trail:1,Colors:[13172728],FadeColors:[16777215]},]}}}}"""
        yield! nTicksLater(8)
        yield U """execute @a ~ ~ ~ summon FireworksRocketEntity ~-3 ~0 ~0 {LifeTime:20,FireworksItem:{id:401,Count:1,tag:{Fireworks:{Explosions:[{Type:2,Flicker:1,Trail:0,Colors:[16777074],FadeColors:[16777215]},]}}}}"""
        yield! nTicksLater(8)
        yield U """execute @a ~ ~ ~ summon FireworksRocketEntity ~0 ~0 ~-3 {LifeTime:20,FireworksItem:{id:401,Count:1,tag:{Fireworks:{Explosions:[{Type:3,Flicker:1,Trail:1,Colors:[6160227],FadeColors:[16777215]}]}}}}"""
        |]
    region.PlaceCommandBlocksStartingAt(GOT_WIN_COMMON_LOGIC,gotAWinCommonLogic,"someone won coda")
    let timekeeperLogic =
        [|
            O "stats block ~ ~ ~6 set QueryResult @e[tag=TimeKeeper] S"
            U "say this gets replaced by redstone/wool"
            P "scoreboard players test Time S 0 0"
            C "blockdata ~ ~ ~2 {auto:1b}"
            C "blockdata ~ ~ ~1 {auto:0b}"
            O ""
            U "worldborder get"
            U "scoreboard players operation @e[tag=TimeKeeper] S -= TenMillion Calc"
            U "scoreboard players operation @e[tag=TimeKeeper] S /= Twenty Calc"
            U "scoreboard players operation Time Score = @e[tag=TimeKeeper] S"
            U "scoreboard players test Time Score 1500 *"
            C (sprintf "setblock %s redstone_block" TIMEKEEPER_25MIN_REDSTONE.STR)
        |]
    region.PlaceCommandBlocksStartingAt(TIMEKEEPER_REDSTONE.Offset(0,0,-1),timekeeperLogic,"timekeeper")
    let timekeeper25min =
        [|
            O ""
            U "execute @a ~ ~ ~ playsound block.note.harp @p ~ ~ ~ 1 0.6"
            U """execute @a ~ ~ ~ tellraw @a [{"selector":"@p"}," got ",{"score":{"name":"@p","objective":"Score"}}," in 25 mins"]"""
        |]
    region.PlaceCommandBlocksStartingAt(TIMEKEEPER_25MIN_REDSTONE.Offset(0,0,1),timekeeper25min,"timekeeper 25min")

    // clone blocks into framework
    let checkForItemsInit() =
        [|
        yield U "scoreboard players set cfiCol S 1"
        yield U (sprintf "summon ArmorStand %s {NoGravity:1,Tags:[\"whereToCloneCommandTo\"]}" (ITEM_CHECKERS_REDSTONE_LOW(0).Offset(0,4,1).STR))
        |]
    let checkForItems() =
        [|
        // find blocks to clone to 1 1 1 (testfor & clone, x4 teams)
        yield U "scoreboard players operation @e[tag=bingoItem] S -= next S"
        yield U "scoreboard players test which S 0 0"
        yield C     "execute @e[tag=bingoItem,score_S_min=0,score_S=0] ~ ~2 ~ clone ~ ~ ~ ~ ~ ~11 1 1 1"
        yield U "scoreboard players test which S 1 1"
        yield C     "execute @e[tag=bingoItem,score_S_min=0,score_S=0] ~ ~1 ~ clone ~ ~ ~ ~ ~ ~11 1 1 1"
        yield U "scoreboard players test which S 2 2"
        yield C     "execute @e[tag=bingoItem,score_S_min=0,score_S=0] ~ ~0 ~ clone ~ ~ ~ ~ ~ ~11 1 1 1"
        yield U "scoreboard players operation @e[tag=bingoItem] S += next S"
        // clone it for each team
        for t = 0 to 3 do
            yield U (sprintf "execute @e[tag=whereToCloneCommandTo] ~%d ~ ~1 clone 1 1 %d 1 1 %d ~ ~ ~" (6*t) (1+2*t) (2+2*t))
        // move next spot on board
        yield U "tp @e[tag=whereToCloneCommandTo] ~1 ~ ~"
        yield U "scoreboard players add cfiCol S 1"
        yield U "scoreboard players test cfiCol S 6 *"
        yield C "scoreboard players set cfiCol S 1"
        yield C "tp @e[tag=whereToCloneCommandTo] ~-5 ~-1 ~"
        |]
    let checkForItemsCleanup() =
        [|
            yield C "kill @e[tag=whereToCloneCommandTo]"
        |]

    
    // uses seed 'Z' to compute a bingo card (and do SOMETHING)
    let bingoCardMakerCmds(sky) =
        [|
        yield O ""
        // summon 28 AECs with score 0-27 at the bottom of the 28 item sets 
        // note, we need these short-lived AECs, because these are killed as removed from candidate set
        yield U "kill @e[tag=bingoItem]"
        for i = 1 to bingoItems.Length do
            yield U (sprintf "summon AreaEffectCloud %d %d %d {Duration:999999,Tags:[\"bingoItem\"]}" (bingoItems.Length - i + BINGO_ITEMS_LOW.X) BINGO_ITEMS_LOW.Y BINGO_ITEMS_LOW.Z)
            yield U "scoreboard players add @e[tag=bingoItem] S 1"
        yield U "scoreboard players remove @e[tag=bingoItem] S 1"
        // init other vars
        yield U (sprintf "scoreboard players set remain S %d" bingoItems.Length)
        yield U "scoreboard players set I S 25"
        if sky then
            yield U """tellraw @a ["Generating card..."]"""
            yield! makeActualCardInit()
            yield! checkForItemsInit()
        // prepare loop
        yield U "summon ArmorStand ~ ~ ~2 {NoGravity:1,Tags:[\"purple\"]}"
        yield U "execute @e[tag=purple] ~ ~ ~ blockdata ~ ~ ~ {auto:1b}"
        // loop - could be inlined, but not for now, to avoid too many blocks
        yield P ""
        if true then
            // pick which of 28 bingo sets
            yield! PRNG("next", "S", "remain", "S")
            // pick which of 3 items in that set
            yield U "scoreboard players set mod S 3"
            yield! PRNG("which", "S", "mod", "S")
            // call some procedure(s) with this implicit state flowing in
            //  - next is a subset # (1-28)
            //  - which is a difficulty # (0-2)
            if sky then
                yield! makeActualCard()
                yield! checkForItems()
            // remove used item from remaining list
            yield U "scoreboard players operation @e[tag=bingoItem] S -= next S"
            yield U "kill @e[tag=bingoItem,score_S_min=0,score_S=0]"
            yield U "scoreboard players remove @e[tag=bingoItem,score_S_min=0] S 1"
            yield U "scoreboard players operation @e[tag=bingoItem] S += next S"
            yield U "scoreboard players remove remain S 1"
            yield U "scoreboard players remove I S 1"
            yield U "scoreboard players test I S * 1"      // testing 1, which is OBO, because loop runs 1 extra time after we turn off purple
            yield C "execute @e[tag=purple] ~ ~ ~ blockdata ~ ~ ~ {auto:0b}"
            yield C "blockdata ~ ~ ~2 {auto:1b}"  //  one tick later (after loop done)
            yield C "blockdata ~ ~ ~1 {auto:0b}"
        yield O ""
        yield U "kill @e[tag=purple]"
        if sky then
            yield! makeActualCardCleanup()
            yield! checkForItemsCleanup()
            // enable other buttons
            yield U (sprintf "blockdata %d %d %d {auto:1b}" (LOBBYX-4) LOBBYY LOBBYZ)
            yield U (sprintf "blockdata %d %d %d {auto:0b}" (LOBBYX-4) LOBBYY LOBBYZ)
            yield U """tellraw @a ["...done!"]"""
        |]
    region.PlaceCommandBlocksStartingAt(MAKE_SEEDED_CARD,bingoCardMakerCmds(true),"make pixel art and checker")

    // want to have grid of 300x300 spawn points, marked -150 to 150 (skipping 0s)
    let spawnIndices = List.append [-150 .. -1] [1 .. 150] |> Array.ofList 
    for y = 0 to 4 do
        let tpXCmds =
            [|
                for z = 0 to 59 do
                    let i = 60*y + z
                    let dist = spawnIndices.[i] * 10000
                    yield U (sprintf "tp @a[tag=oneGuyToTeleport] %d ~ ~" dist)
            |]
        region.PlaceCommandBlocksStartingAt(TPX_LOW.Offset(0,y,0),tpXCmds, "tp X")
    for y = 0 to 4 do
        let tpZCmds =
            [|
                for z = 0 to 59 do
                    let i = 60*y + z
                    let dist = spawnIndices.[i] * 10000
                    yield U (sprintf "tp @a[tag=oneGuyToTeleport] ~ ~ %d" dist)
            |]
        region.PlaceCommandBlocksStartingAt(TPZ_LOW.Offset(0,y,0),tpZCmds, "tp Z")
    // once a spawn is picked, we'll also need to use falling armor stand to compute Y height of spawn point
    for y = 0 to 3 do
        let tpYCmds =
            [|
                for z = 0 to 31 do
                    yield U (sprintf "tp @a[tag=oneGuyToTeleport] ~ %d ~" (32*y + z))
            |]
        region.PlaceCommandBlocksStartingAt(TPY_LOW.Offset(0,y,0),tpYCmds, "tp Y")

    // pick spawn from seed
    let pillarUpTheArmorStand =
        [|
            yield O ""
            for _i = 0 to 30 do
                yield U "execute @e[tag=tpas] ~ ~ ~ testforblock ~ ~ ~ air 0"
                yield U "testforblock ~ ~ ~-1 chain_command_block -1 {SuccessCount:0}"
                yield C "tp @e[tag=tpas] ~ ~2 ~"
                yield C "execute @e[tag=tpas] ~ ~ ~ fill ~ ~-2 ~ ~ ~-1 ~ dirt"
        |]
    region.PlaceCommandBlocksStartingAt(PILLAR_UP_THE_ARMOR_STAND,pillarUpTheArmorStand, "pillar up the armor stand")
    // compute Y coordinate of armor stand
    let computeYCoordinateInit =
        [|
            yield O ""
            yield U """execute @e[tag=tpas] ~ ~ ~ summon AreaEffectCloud ~ ~ ~ {Duration:999999,Tags:["findASY"]}"""
            yield U (sprintf """summon AreaEffectCloud %s {Duration:999999,Tags:["findYCmd"]}""" TPY_LOW.STR)
            yield U """setblock 1 1 1 chain_command_block 3 {auto:1b,Command:"tp @a[tag=oneGuyToTeleport] ~ 68 ~"}"""  // choose a safe-ish default; if the player DCs during startup, need to at least have a CCB so chains are not broken
            yield U "blockdata ~ ~ ~2 {auto:1b}"
            yield U "blockdata ~ ~ ~1 {auto:0b}"
            yield O ""
            yield U "tp @e[tag=findASY] ~ 0 ~"
            yield U (sprintf "blockdata %d %d %d {auto:1b}" COMPUTE_Y_ARMOR_STAND_LOW.X (COMPUTE_Y_ARMOR_STAND_LOW.Y+1) COMPUTE_Y_ARMOR_STAND_LOW.Z)
            yield U (sprintf "blockdata %d %d %d {auto:0b}" COMPUTE_Y_ARMOR_STAND_LOW.X (COMPUTE_Y_ARMOR_STAND_LOW.Y+1) COMPUTE_Y_ARMOR_STAND_LOW.Z)
        |]
    region.PlaceCommandBlocksStartingAt(COMPUTE_Y_ARMOR_STAND_LOW.Offset(0,0,0),computeYCoordinateInit, "compute armor stand Y init")
    for y = 1 to 4 do
        let computeYCoordinate =
            [|
                yield O ""
                for _i = 0 to 31 do
                    //                                         if we have reached Y           then clone
                    yield U "execute @e[tag=findASY] ~ ~ ~ execute @e[tag=tpas,r=1] ~ ~ ~ execute @e[tag=findYCmd] ~ ~ ~ clone ~ ~ ~ ~ ~ ~ 1 1 1"
                    yield U "tp @e[tag=findYCmd] ~ ~ ~1"
                    yield U "tp @e[tag=findASY] ~ ~1 ~"
                yield U "tp @e[tag=findYCmd] ~ ~1 ~-32"
                yield U (sprintf "blockdata %d %d %d {auto:1b}" COMPUTE_Y_ARMOR_STAND_LOW.X (COMPUTE_Y_ARMOR_STAND_LOW.Y+y+1) COMPUTE_Y_ARMOR_STAND_LOW.Z)
                yield U (sprintf "blockdata %d %d %d {auto:0b}" COMPUTE_Y_ARMOR_STAND_LOW.X (COMPUTE_Y_ARMOR_STAND_LOW.Y+y+1) COMPUTE_Y_ARMOR_STAND_LOW.Z)
            |]
        region.PlaceCommandBlocksStartingAt(COMPUTE_Y_ARMOR_STAND_LOW.Offset(0,y,0),computeYCoordinate, "compute armor stand Y loop")
    let computeYCoordinateCoda =
        [|
            yield O ""
            yield U "kill @e[tag=findASY]"
            yield U "kill @e[tag=findYCmd]"
        |]
    region.PlaceCommandBlocksStartingAt(COMPUTE_Y_ARMOR_STAND_LOW.Offset(0,5,0),computeYCoordinateCoda, "compute armor stand Y coda")
        
    let SPAWN_START_HEIGHT = 130
    let pickSpawnThenActivate(t,team,ax,ay,az) = 
        [|
#if DEBUG_DETAIL
            yield O (sprintf "say pickSpawnThenActivate %s" team)
#else
            yield O ""
#endif
            yield U (sprintf "tp @p[tag=oneGuyToTeleport] ~ %d ~" SPAWN_START_HEIGHT)
            yield U "scoreboard players set mod S 300"
            yield! PRNG("whichX", "S", "mod", "S")
            yield U "scoreboard players set mod S 300"
            yield! PRNG("whichZ", "S", "mod", "S")
            // now whichX,whichZ have our 0-299 indices, but need to index them into actual command blocks
            yield U "scoreboard players set mod S 60"
            yield U "scoreboard players operation x1 S = whichX S"
            yield U "scoreboard players operation x1 S /= mod S"
            yield U "scoreboard players operation x2 S = whichX S"
            yield U "scoreboard players operation x2 S %= mod S"
            yield U "scoreboard players operation z1 S = whichZ S"
            yield U "scoreboard players operation z1 S /= mod S"
            yield U "scoreboard players operation z2 S = whichZ S"
            yield U "scoreboard players operation z2 S %= mod S"
            // find the tpX block to clone
            yield U (sprintf "summon ArmorStand %s {Tags:[\"findblock\"]}" TPX_LOW.STR)
            yield! teleportBasedOnScore("findblock", "x2", "S", "z")
            yield! teleportBasedOnScore("findblock", "x1", "S", "y")
            let tpxCmd = SPAWN_LOCATION_COMMANDS(t)
            let tpzCmd = SPAWN_LOCATION_COMMANDS(t).Offset(0,0,1)
            yield U (sprintf "execute @e[tag=findblock] ~ ~ ~ clone ~ ~ ~ ~ ~ ~ %s" tpxCmd.STR)
            yield U (sprintf "clone %s %s ~ ~ ~1" tpxCmd.STR tpxCmd.STR)
            yield U "say THIS SHOULD HAVE BEEN REPLACED"
            // find the tpZ block to clone
            yield U (sprintf "tp @e[tag=findblock] %s" TPZ_LOW.STR)
            yield! teleportBasedOnScore("findblock", "z2", "S", "z")
            yield! teleportBasedOnScore("findblock", "z1", "S", "y")
            yield U (sprintf "execute @e[tag=findblock] ~ ~ ~ clone ~ ~ ~ ~ ~ ~ %s" tpzCmd.STR)
            yield U (sprintf "clone %s %s ~ ~ ~1" tpzCmd.STR tpzCmd.STR)
            yield U "say THIS SHOULD HAVE BEEN REPLACED"
            yield U "kill @e[tag=findblock]"
            // guy is TP'd there, now do spawn box, drop armor stand, etc
            yield! nTicksLater(100) // get some terrain gen'd    TODO adjust timing?
            yield U (sprintf "tp @p[tag=oneGuyToTeleport] ~ %d ~" SPAWN_START_HEIGHT)
            yield! nTicksLater(5) // ensure skybox area gen'd    TODO adjust timing?
            // re-tp to exact spot to build box, in case he moved while falling
            yield U (sprintf "tp @p[tag=oneGuyToTeleport] ~ %d ~" SPAWN_START_HEIGHT)
            yield U (sprintf "clone %s %s ~ ~ ~1" tpxCmd.STR tpxCmd.STR)
            yield U "say THIS SHOULD HAVE BEEN REPLACED"
            yield U (sprintf "clone %s %s ~ ~ ~1" tpzCmd.STR tpzCmd.STR)
            yield U "say THIS SHOULD HAVE BEEN REPLACED"
            yield U (sprintf "execute @p[tag=oneGuyToTeleport] ~ ~ ~ fill ~-1 %d ~-1 ~1 %d ~1 barrier 0 hollow" (SPAWN_START_HEIGHT-2) (SPAWN_START_HEIGHT+3))
            yield U (sprintf "tp @p[tag=oneGuyToTeleport] ~ %d ~" SPAWN_START_HEIGHT)
            yield U "execute @p[tag=oneGuyToTeleport] ~ ~ ~ summon ArmorStand ~ ~-4 ~ {Invulnerable:1,Marker:1,Tags:[\"tpas\"]}"
            yield U (sprintf """tellraw @a ["Giving ",{"selector":"@p[tag=oneGuyToTeleport]"}," a birds-eye view of %s spawn as terrain generates..."]""" team)
            yield! nTicksLater(400) // TODO adjust timing?
            // call out to pillar-armor-stand (takes 1 tick to pillar, 7 ticks to compute Y)
            yield U (sprintf "blockdata %s {auto:1b}" PILLAR_UP_THE_ARMOR_STAND.STR)
            yield U (sprintf "blockdata %s {auto:0b}" PILLAR_UP_THE_ARMOR_STAND.STR)
            yield U (sprintf "blockdata %s {auto:1b}" COMPUTE_Y_ARMOR_STAND_LOW.STR)
            yield U (sprintf "blockdata %s {auto:0b}" COMPUTE_Y_ARMOR_STAND_LOW.STR)
            yield! nTicksLater(8) // ensure armor stand pillar & y compute done
            yield U (sprintf "clone 1 1 1 1 1 1 %s" (SPAWN_LOCATION_COMMANDS(t).Offset(0,0,2).STR))
            yield U "tp @p[tag=oneGuyToTeleport] @e[tag=tpas]"
            yield U "kill @e[tag=tpas]"
            yield U "execute @p[tag=oneGuyToTeleport] ~ ~ ~ fill ~ ~ ~ ~ ~20 ~ air"  // just in case findY failed and they're suffocating inside terrain
            yield U (sprintf "tp @a[team=%s,tag=!oneGuyToTeleport] @p[tag=oneGuyToTeleport]" team)
            yield U (sprintf "spawnpoint @a[team=%s]" team)
            yield U (sprintf "blockdata %d %d %d {auto:1b}" ax ay az)
            yield U (sprintf "blockdata %d %d %d {auto:0b}" ax ay az)
        |]
    for t = 0 to 3 do
        let team = TEAMS.[t]
        let coord = TELEPORT_PLAYERS_TO_SEEDED_SPAWN_LOW
        let pickSpawnCmds =
            [|
            yield O ""
            yield U "scoreboard players tag @a[tag=oneGuyToTeleport] remove oneGuyToTeleport"
            yield U (sprintf "testfor @p[team=%s]" team)
            yield U "testforblock ~ ~ ~-1 chain_command_block -1 {SuccessCount:0}"
            yield C (sprintf "blockdata %d %d %d {auto:1b}" (coord.X+t+1) coord.Y coord.Z)
            yield C (sprintf "blockdata %d %d %d {auto:0b}" (coord.X+t+1) coord.Y coord.Z)
            yield U "testforblock ~ ~ ~-3 chain_command_block -1 {SuccessCount:0}"
            yield C (sprintf "scoreboard players tag @p[team=%s] add oneGuyToTeleport" team)
            yield C (sprintf "blockdata %s {auto:1b}" SEND_TO_WAITING_ROOM.STR)
            yield C (sprintf "blockdata %s {auto:0b}" SEND_TO_WAITING_ROOM.STR)
            yield C (sprintf """tellraw @a ["Initializing spawn point for %s team..."]""" team)
            yield C "blockdata ~ ~ ~2 {auto:1b}"
            yield C "blockdata ~ ~ ~1 {auto:0b}"
            yield! pickSpawnThenActivate(t,team,coord.X+t+1,coord.Y,coord.Z)
            |]
        region.PlaceCommandBlocksStartingAt(coord.X+t,coord.Y,coord.Z,pickSpawnCmds, "spawn based on seed")
    let afterAllSpawn =
        [|
        O ""
        U (sprintf "blockdata %s {auto:1b}" START_GAME_PART_2.STR)
        U (sprintf "blockdata %s {auto:0b}" START_GAME_PART_2.STR)
        |]
    let coordTp = TELEPORT_PLAYERS_TO_SEEDED_SPAWN_LOW
    region.PlaceCommandBlocksStartingAt(coordTp.X+4,coordTp.Y,coordTp.Z,afterAllSpawn, "after spawn based on seed")
    let sendToWaitingRoom =
        [|
        O ""
        U "scoreboard players test folksSentToWaitingRoom S 0 0"
        C "scoreboard players set folksSentToWaitingRoom S 1"
        // put everyone except first TP guy in waiting room to start
        C (sprintf "tp @a[tag=!oneGuyToTeleport] %s 180 0" WAITING_ROOM.STR)
        |]
    region.PlaceCommandBlocksStartingAt(SEND_TO_WAITING_ROOM,sendToWaitingRoom, "sendToWaitingRoom")


    printfn ""
    printfn "Total commands placed: %d" region.NumCommandBlocksPlaced 

    region.Write(fil+".new")
    System.IO.File.Delete(fil)
    System.IO.File.Move(fil+".new",fil)


////////////////////////////////////////////////////

let testBackpatching(fil) =
    let r = new RegionFile(fil)
    r.PlaceCommandBlocksStartingAt(1,5,1,[|
        O ""
        U "say 1"
        U "BLOCKDATA ON 1"
        U "BLOCKDATA OFF 1"
        U "say 2"
        C "BLOCKDATA ON 2"
        C "BLOCKDATA OFF 2"
        U "say 3"
        O "TAG 2"
        U "say 6"
        U "say 7"
        O "TAG 1"
        U "say 4"
        U "say 5"
        |],"yadda")
    r.Write(fil+".new")
    System.IO.File.Delete(fil)
    System.IO.File.Move(fil+".new",fil)



////////////////////////////////////////////////////

type MapFolder(folderName) =
    let cachedRegions = new System.Collections.Generic.Dictionary<_,_>()
    let getOrCreateRegion(args) =
        if cachedRegions.ContainsKey(args) then
            cachedRegions.[args]
        else
            let rx,rz = args
            let fil = System.IO.Path.Combine(folderName, sprintf "r.%d.%d.mca" rx rz)
            let r = new RegionFile(fil)
            cachedRegions.Add(args, r)
            r
    member this.AddOrReplaceTileEntities(tes) =
        // partition by region,chunk
        let data = new System.Collections.Generic.Dictionary<_,_>()
        for te in tes do
            let x = te |> Array.pick (function Int("x",x) -> Some x | _ -> None)
            let y = te |> Array.pick (function Int("y",y) -> Some y | _ -> None)
            let z = te |> Array.pick (function Int("z",z) -> Some z | _ -> None)
            let rx = (x + 512000) / 512 - 1000
            let rz = (z + 512000) / 512 - 1000
            if not(data.ContainsKey(rx,rz)) then
                data.Add((rx,rz), Array2D.init 32 32 (fun _ _ -> ResizeArray()))
            let cx = ((x+512000)%512)/16
            let cz = ((z+512000)%512)/16
            data.[(rx,rz)].[cx,cz].Add(te)
        for (KeyValue((rx,rz),tesPerChunk)) in data do
            let r = getOrCreateRegion(rx, rz)
            // load each chunk TEs
            for cx = 0 to 31 do
                for cz = 0 to 31 do
                    if tesPerChunk.[cx,cz].Count > 0 then
                        let chunk = r.GetChunk(cx,cz)
                        let a = match chunk with Compound(_,[|Compound(_,a);_|]) | Compound(_,[|Compound(_,a);_;_|]) -> a
                        let mutable found = false
                        let mutable i = 0
                        while not found && i < a.Length-1 do
                            match a.[i] with
                            | List("TileEntities",Compounds(existingTEs)) ->
                                // there are TEs already, remove any with xyz that we'll overwrite, and add new ones
                                found <- true
                                let finalTEs = ResizeArray()
                                for ete in existingTEs do
                                    let mutable willGetOverwritten = false
                                    for nte in tesPerChunk.[cx,cz] do
                                        let x = nte |> Array.pick (function Int("x",x) -> Some x | _ -> None)
                                        let y = nte |> Array.pick (function Int("y",y) -> Some y | _ -> None)
                                        let z = nte |> Array.pick (function Int("z",z) -> Some z | _ -> None)
                                        let alreadyThere = Array.exists (fun o -> o=Int("x",x)) ete && Array.exists (fun o -> o=Int("y",y)) ete && Array.exists (fun o -> o=Int("z",z)) ete
                                        if alreadyThere then
                                            willGetOverwritten <- true
                                    if willGetOverwritten then
                                        () // TODO failwith "TODO overwriting TE, care?"
                                    else
                                        finalTEs.Add(ete)
                                for nte in tesPerChunk.[cx,cz] do
                                    finalTEs.Add(nte)
                                a.[i] <- List("TileEntities",Compounds(finalTEs |> Array.ofSeq))
                            | _ -> ()
                            i <- i + 1
                        if not found then // no TileEntities yet, write the entry
                            match chunk with 
                            | Compound(_,([|Compound(n,a);_|] as r)) 
                            | Compound(_,([|Compound(n,a);_;_|] as r)) -> 
                                r.[0] <- Compound(n, a |> Seq.append [| List("TileEntities",Compounds(tesPerChunk.[cx,cz] |> Array.ofSeq)) |] |> Array.ofSeq)
    member this.GetHeightMap(x,z) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.GetHeightMap(x,z)
    member this.SetBlockIDAndDamage(x,y,z,bid,d) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.SetBlockIDAndDamage(x,y,z,bid,d)
    member this.GetOrCreateSection(x,y,z) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.GetOrCreateSection(x,y,z)
    member this.GetBlockInfo(x,y,z) =
        let rx = (x + 512000) / 512 - 1000
        let rz = (z + 512000) / 512 - 1000
        let r = getOrCreateRegion(rx, rz)
        r.GetBlockInfo(x,y,z)
    member this.WriteAll() =
        for KeyValue(args, r) in cachedRegions do
            let rx,rz = args
            let fil = System.IO.Path.Combine(folderName, sprintf "r.%d.%d.mca" rx rz)
            r.Write(fil+".new")
            System.IO.File.Delete(fil)
            System.IO.File.Move(fil+".new",fil)

////////////////////////////////////////////

    //map.SetBlockIDAndDamage(-342, 11, 97, 52uy, 0uy) // 52 = monster spawner
type MobSpawnerInfo() =
    member val RequiredPlayerRange =  16s with get, set
    member val SpawnCount          =   4s with get, set
    member val SpawnRange          =   4s with get, set
    member val MaxNearbyEntities   =   6s with get, set
    member val Delay               =  -1s with get, set
    member val MinSpawnDelay       = 200s with get, set
    member val MaxSpawnDelay       = 800s with get, set
    member val x = 0 with get, set 
    member val y = 0 with get, set 
    member val z = 0 with get, set 
    member val BasicMob = "Zombie" with get, set  // TODO more advanced SpawnPotentials/SpawnData
    member this.AsNbtTileEntity() =
        [|
            Int("x", this.x)
            Int("y", this.y)
            Int("z", this.z)
            String("id","MobSpawner")
            Short("RequiredPlayerRange",this.RequiredPlayerRange)
            Short("SpawnCount",this.SpawnCount)
            Short("SpawnRange",this.SpawnRange)
            Short("MaxNearbyEntities",this.MaxNearbyEntities)
            Short("Delay",this.Delay)
            Short("MinSpawnDelay",this.MinSpawnDelay)
            Short("MaxSpawnDelay",this.MaxSpawnDelay)
            Compound("SpawnData",[|String("id",this.BasicMob);End|])
            List("SpawnPotentials",Compounds[|
                                                [|
                                                Compound("Entity",[|String("id",this.BasicMob);End|])
                                                Int("Weight",1)
                                                End
                                                |]
                                            |])
            End
        |]

////////////////////////////////////////////

let preciseImageToBlocks(imageFilename:string,regionFolder, baseY) =
    let image = new System.Drawing.Bitmap(imageFilename)
    let m = new MapFolder(regionFolder)
    let colorTable= new System.Collections.Generic.Dictionary<_,_>()
    let knownColors = 
        [|
            (255uy, 51uy, 102uy, 153uy),   (fun x y z -> m.SetBlockIDAndDamage(x, baseY, z, 95uy, 11uy))   // blue glass water
            (255uy, 255uy, 255uy, 255uy),  (fun x y z -> m.SetBlockIDAndDamage(x, baseY, z, 80uy, 0uy))    // white snow
            (255uy, 0uy, 102uy, 0uy),      (fun x y z -> m.SetBlockIDAndDamage(x, baseY, z, 35uy, 13uy))   // green wool tree
            (255uy, 102uy, 102uy, 102uy),  (fun x y z -> m.SetBlockIDAndDamage(x, baseY, z, 7uy, 0uy))     // dark mountain
            (255uy, 0uy, 204uy, 0uy),      (fun x y z -> m.SetBlockIDAndDamage(x, baseY, z, 2uy, 0uy))     // green grass
            (255uy, 255uy, 51uy, 0uy),     (fun x y z -> for dy in [0;1;2;3] do m.SetBlockIDAndDamage(x, baseY+dy, z, 152uy, 0uy))   // red wall
            (255uy, 153uy, 153uy, 153uy),  (fun x y z -> m.SetBlockIDAndDamage(x, baseY, z, 1uy, 0uy))     // grey stone
            (255uy, 255uy, 255uy, 0uy),    (fun x y z -> m.SetBlockIDAndDamage(x, baseY, z, 41uy, 0uy))    // gold thingy
            (255uy, 204uy, 255uy, 255uy),  (fun x y z -> m.SetBlockIDAndDamage(x, baseY, z, 174uy, 0uy))   // light blue ice
            (255uy, 153uy, 102uy, 51uy),   (fun x y z -> m.SetBlockIDAndDamage(x, baseY, z, 3uy, 2uy))     // brown podzol
            (255uy, 0uy, 0uy, 0uy),        (fun x y z -> ())                                               // black means air
            (255uy, 255uy, 102uy, 0uy),    (fun x y z -> m.SetBlockIDAndDamage(x, baseY, z, 86uy, 11uy))   // orange pumpkin
            (255uy, 153uy, 51uy, 0uy),     (fun x y z -> ()) // TODO
            (255uy, 0uy, 255uy, 0uy),      (fun x y z -> ()) // TODO
            (255uy, 255uy, 204uy, 153uy),      (fun x y z -> ()) // TODO
            (255uy, 153uy, 255uy, 102uy),      (fun x y z -> ()) // TODO
        |]
    knownColors |> Seq.iter (fun ((a,r,g,b),f) -> colorTable.Add(System.Drawing.Color.FromArgb(int a, int r, int g, int b), f))
    let mutable nextNumber = 0
    let XM = max (image.Width-1) 511
    let ZM = max (image.Height-1) 511
    for x = 0 to XM do
        for z = 0 to ZM do
            let c = image.GetPixel(x,z)
            colorTable.[c] x 10 z
            (*
            let n =
                if colorTable.ContainsKey(c) then 
                    colorTable.[c] 
                else
                    colorTable.Add(c,nextNumber)
                    nextNumber <- nextNumber + 1
                    nextNumber - 1
            r.SetBlockIDAndDamage(x, 10, z, 35uy, byte n)  // 35 = wool
    colorTable |> Seq.map (fun (KeyValue(c,n)) -> n, (c.A, c.R, c.G, c.B)) |> Seq.sortBy fst |> Seq.iter (fun (_,c) -> printfn "%A" c)
            *)
        printfn "%d of %d" x XM
    m.WriteAll()


////////////////////////////////////////////

let repopulateAsAnotherBiome() =
    //let user = "brianmcn"
    let user = "Admin1"
    let fil = """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\15w44b\region\r.0.0.mca"""
    let regionFile = new RegionFile(fil)
    //let newBiome = 32uy // mega taiga
    //let newBiome = 8uy // hell // didn't do anything interesting?
    //let newBiome = 13uy // ice plains // freezes ocean, adds snow layer
    //let newBiome = 129uy // sunflower plains, saw lakes added
    //let newBiome = 140uy // ice plains spikes (did not generate spikes) // freezes ocean, adds snow layer, re-freezes lakes that formed on/under ocean, ha
    //let newBiome = 38uy // mesa plateau f (did not change stone to clay)
    let newBiome = 6uy // swamp (did not see any witch huts, but presumably seed based?)
    for cx = 0 to 31 do
        for cz = 0 to 31 do
            match regionFile.TryGetChunk(cx,cz) with
            | None -> ()
            | Some theChunk ->
                let theChunkLevel = match theChunk with Compound(_,[|c;_|]) | Compound(_,[|c;_;_|]) -> c // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
                // replace biomes
                match theChunkLevel.["Biomes"] with
                | NBT.ByteArray(_,a) -> for i = 0 to a.Length-1 do a.[i] <- newBiome
                // replace terrain-populated
                match theChunkLevel with
                | NBT.Compound(_,a) ->
                    for i = 0 to a.Length-1 do
                        if a.[i].Name = "TerrainPopulated" then
                            a.[i] <- NBT.Byte("TerrainPopulated", 0uy)
    regionFile.Write(fil+".new")
    System.IO.File.Delete(fil)
    System.IO.File.Move(fil+".new",fil)

////////////////////////////////////////////


let debugRegion() =
    //let user = "brianmcn"
    let user = "Admin1"
    let rx = 5
    let rz = 0
    let fil = """C:\Users\"""+user+(sprintf """\AppData\Roaming\.minecraft\saves\pregenED\region\r.%d.%d.mca""" rx rz)
    let regionFile = new RegionFile(fil)
    for cx = 0 to 31 do
        for cz = 0 to 31 do
            match regionFile.TryGetChunk(cx,cz) with
            | None -> ()
            | Some theChunk ->
                printf "%5d,%5d: " (cx*16+rx*512) (cz*16+rz*512)
                let theChunkLevel = match theChunk with Compound(_,[|c;_|]) | Compound(_,[|c;_;_|]) -> c // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
                match theChunkLevel.["TerrainPopulated"] with
                | NBT.Byte(_,b) -> printf "TP=%d  " b
                match theChunkLevel.["LightPopulated"] with
                | NBT.Byte(_,b) -> printf "LP=%d  " b
                match theChunkLevel.["Entities"] with
                | NBT.List(_,Compounds(a)) -> printf "E=%d  " a.Length 
                match theChunkLevel.["TileEntities"] with
                | NBT.List(_,Compounds(a)) -> printf "TE=%d  " a.Length 
                match theChunkLevel.TryGetFromCompound("TileTicks") with
                | Some(NBT.List(_,Compounds(a))) -> printf "TT=%d  " a.Length 
                | None -> printf "TT=0  "
                printfn ""
                // replace terrain-populated
                match theChunkLevel with
                | NBT.Compound(_,a) ->
                    for i = 0 to a.Length-1 do
                        if a.[i].Name = "TerrainPopulated" then
                            a.[i] <- NBT.Byte("TerrainPopulated", 1uy)
    regionFile.Write(fil+".new")
                

////////////////////////////////////////////

type Thingy(point:int, isLeft:bool, isRight:bool) =
    let mutable isLeft = isLeft
    let mutable isRight = isRight
    member this.Point = point
    member this.IsLeft with get() = isLeft and set(x) = isLeft <- x
    member this.IsRight with get() = isRight and set(x) = isRight <- x

// A partition is a mutable set of values, where one arbitrary value in the set 
// is chosen as the canonical representative for that set. 
[<AllowNullLiteral>]
type Partition(orig : Thingy) as this =  
    [<DefaultValue(false)>] val mutable parent : Partition
    [<DefaultValue(false)>] val mutable rank : int 
    let rec FindHelper(x : Partition) = 
        if System.Object.ReferenceEquals(x.parent, x) then 
            x 
        else 
            x.parent <- FindHelper(x.parent) 
            x.parent 
    do this.parent <- this 
    // The representative element in this partition 
    member this.Find() = 
        FindHelper(this) 
    // The original value of this element 
    member this.Value = orig 
    // Merges two partitions 
    member this.Union(other : Partition) = 
        let thisRoot = this.Find() 
        let otherRoot = other.Find() 
        if thisRoot.rank < otherRoot.rank then 
            otherRoot.parent <- thisRoot
            thisRoot.Value.IsLeft <- thisRoot.Value.IsLeft || otherRoot.Value.IsLeft 
            thisRoot.Value.IsRight <- thisRoot.Value.IsRight || otherRoot.Value.IsRight
        elif thisRoot.rank > otherRoot.rank then 
            thisRoot.parent <- otherRoot 
            otherRoot.Value.IsLeft <- otherRoot.Value.IsLeft || thisRoot.Value.IsLeft 
            otherRoot.Value.IsRight <- otherRoot.Value.IsRight || thisRoot.Value.IsRight
        elif not (System.Object.ReferenceEquals(thisRoot, otherRoot)) then 
            otherRoot.parent <- thisRoot 
            thisRoot.Value.IsLeft <- thisRoot.Value.IsLeft || otherRoot.Value.IsLeft 
            thisRoot.Value.IsRight <- thisRoot.Value.IsRight || otherRoot.Value.IsRight
            thisRoot.rank <- thisRoot.rank + 1 

let findUndergroundAirSpaceConnectedComponents(map:MapFolder) =
    let LOX, LOY, LOZ = -512, 11, -512
    let MAXI, MAXJ, MAXK = 1024, 50, 1024
    let PT(i,j,k) = i*MAXJ*MAXK + k*MAXJ + j
    let a = Array3D.create (MAXI+2) (MAXJ+2) (MAXK+2) null   // +2s because we have sentinels guarding array index out of bounds
    let mutable currentSectionBlocks,curx,cury,curz = null,-1000,-1000,-1000
    let XYZ(i,j,k) =
        let x = i-1 + LOX
        let y = j-1 + LOY
        let z = k-1 + LOZ
        x,y,z
    // find all the air spaces in the underground
    printf "FIND"
    for j = 1 to MAXJ do
        printf "."
        for i = 1 to MAXI do
            for k = 1 to MAXK do
                let x,y,z = XYZ(i,j,k)
                if not(DIV(x,16) = DIV(curx,16) && DIV(y,16) = DIV(cury,16) && DIV(z,16) = DIV(curz,16)) then
                    currentSectionBlocks <- map.GetOrCreateSection(x,y,z) |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
                    curx <- x
                    cury <- y
                    curz <- z
                let dx, dy, dz = (x+51200) % 16, y % 16, (z+51200) % 16
                let bix = dy*256 + dz*16 + dx
                if currentSectionBlocks.[bix] = 0uy then // air
                    //a.[i,j,k] <- new Partition(new Thingy(PT(i,j,k),(j=1),(j=MAXJ)))
                    a.[i,j,k] <- new Partition(new Thingy(PT(i,j,k),(j=1),(y>=map.GetHeightMap(x,z))))
    printfn ""
    printf "CONNECT"
    // connected-components them
    for j = 1 to MAXJ-1 do
        printf "."
        for i = 1 to MAXI-1 do
            for k = 1 to MAXK-1 do
                if a.[i,j,k]<>null && a.[i+1,j,k]<>null then
                    a.[i,j,k].Union(a.[i+1,j,k])
                if a.[i,j,k]<>null && a.[i,j+1,k]<>null then
                    a.[i,j,k].Union(a.[i,j+1,k])
                if a.[i,j,k]<>null && a.[i,j,k+1]<>null then
                    a.[i,j,k].Union(a.[i,j,k+1])
    printfn ""
    printf "ANALYZE"
    // look for 'good' ones
    let goodCCs = new System.Collections.Generic.Dictionary<_,_>()
    for j = 1 to MAXJ do
        printf "."
        for i = 1 to MAXI do
            for k = 1 to MAXK do
                if a.[i,j,k]<>null then
                    let v = a.[i,j,k].Find().Value 
                    if v.IsLeft && v.IsRight then
                        if not(goodCCs.ContainsKey(v.Point)) then
                            goodCCs.Add(v.Point, new System.Collections.Generic.HashSet<_>())
                        else
                            goodCCs.[v.Point].Add(PT(i,j,k)) |> ignore
    printfn ""
    printfn "There are %d CCs with the desired property" goodCCs.Count 
    for hs in goodCCs.Values do
        let XYZP(pt) =
            let i = pt / (MAXJ*MAXK)
            let k = (pt % (MAXJ*MAXK)) / MAXJ
            let j = pt % MAXJ
            XYZ(i,j,k)
        let IJK(x,y,z) =
            let i = x+1 - LOX
            let j = y+1 - LOY
            let k = z+1 - LOZ
            i,j,k
        let mutable bestX,bestY,bestZ = 0,0,0
        for p in hs do
            let x,y,z = XYZP(p)
            if y > bestY then
                bestX <- x
                bestY <- y
                bestZ <- z
        // have a point at the top of the CC, now find furthest low point away (Dijkstra variant)
        let dist = Array3D.create (MAXI+2) (MAXJ+2) (MAXK+2) 999999   // +2: don't need sentinels here, but easier to keep indexes in lock-step with other array
        let prev = Array3D.create (MAXI+2) (MAXJ+2) (MAXK+2) (0,0,0)  // +2: don't need sentinels here, but easier to keep indexes in lock-step with other array
        let q = new System.Collections.Generic.Queue<_>()
        let bi,bj,bk = IJK(bestX,bestY,bestZ)
        q.Enqueue(bi,bj,bk)
        dist.[bi,bj,bk] <- 0
        let mutable besti,bestj,bestk = bi, bj, bk
        while q.Count > 0 do
            let i,j,k = q.Dequeue()
            let d = dist.[i,j,k]
            for di,dj,dk in [1,0,0; 0,1,0; 0,0,1; -1,0,0; 0,-1,0; 0,0,-1] do
                if a.[i+di,j+dj,k+dk]<>null && dist.[i+di,j+dj,k+dk] > d+1 then
                    dist.[i+di,j+dj,k+dk] <- d+1  // TODO bias to walls
                    prev.[i+di,j+dj,k+dk] <- (i,j,k)
                    q.Enqueue(i+di,j+dj,k+dk)
                    if j = 1 then  // low point
                        if dist.[besti,bestj,bestk] < d+1 then
                            besti <- i+di
                            bestj <- j+dj
                            bestk <- k+dk
        // now find shortest from that bottom to top
        let dist = Array3D.create (MAXI+2) (MAXJ+2) (MAXK+2) 999999   // +2: don't need sentinels here, but easier to keep indexes in lock-step with other array
        let prev = Array3D.create (MAXI+2) (MAXJ+2) (MAXK+2) (0,0,0,false,false,false)  // +2: don't need sentinels here, but easier to keep indexes in lock-step with other array
        let bi,bj,bk = besti,bestj,bestk
        q.Enqueue(bi,bj,bk)
        dist.[bi,bj,bk] <- 0
        let mutable besti,bestj,bestk = bi, bj, bk
        while q.Count > 0 do
            let i,j,k = q.Dequeue()
            let d = dist.[i,j,k]
            for di,dj,dk in [1,0,0; 0,1,0; 0,0,1; -1,0,0; 0,-1,0; 0,0,-1] do
                if a.[i+di,j+dj,k+dk]<>null && dist.[i+di,j+dj,k+dk] > d+1 then
                    dist.[i+di,j+dj,k+dk] <- d+1  // TODO bias to walls
                    prev.[i+di,j+dj,k+dk] <- (i,j,k,(di=0),(dj=0),(dk=0))  // booleans here help us track 'normal' to the path
                    q.Enqueue(i+di,j+dj,k+dk)
                    let x,y,z = XYZ(i,j,k)
                    if (y>=map.GetHeightMap(x,z)) then // surface
                        // found shortest
                        besti <- i+di
                        bestj <- j+dj
                        bestk <- k+dk
                        while q.Count > 0 do
                            q.Dequeue() |> ignore
        // found a path
        let sx,sy,sz = XYZ(bi,bj,bk)
        let ex,ey,ez = XYZ(besti,bestj,bestk)
        printfn "(%d,%d,%d) is %d blocks from (%d,%d,%d)" sx sy sz dist.[besti,bestj,bestk] ex ey ez
        let mutable i,j,k = besti,bestj,bestk
        let fullDist = dist.[besti,bestj,bestk]
        let mutable count = 0
        let rng = System.Random()
        let spawnerTileEntities = ResizeArray()
        while i<>bi || j<>bj || k<>bk do
            let ni,nj,nk,ii,jj,kk = prev.[i,j,k]   // ii/jj/kk track 'normal' to the path
            // maybe put mob spawner nearby
            let pct = float count / float fullDist
            if rng.NextDouble() < pct then
                let xx,yy,zz = XYZ(i,j,k)
                let mutable spread = 1   // check in outwards 'rings' around the path until we find a block we can replace
                let mutable ok = false
                while not ok do
                    let feesh = ResizeArray()
                    let xs = if ii then [xx-spread .. xx+spread] else [xx]
                    let ys = if jj then [yy-spread .. yy+spread] else [yy]
                    let zs = if kk then [zz-spread .. zz+spread] else [zz]
                    for x in xs do
                        for y in ys do
                            for z in zs do
                                if map.GetBlockInfo(x,y,z).BlockID = 97uy then // if silverfish
                                    feesh.Add(x,y,z)
                    if feesh.Count > 0 then
                        let x,y,z = feesh.[rng.Next(feesh.Count-1)]
                        map.SetBlockIDAndDamage(x, y, z, 52uy, 0uy) // 52 = monster spawner
                        let ms = MobSpawnerInfo(x=x, y=y, z=z, BasicMob="Skeleton")
                        spawnerTileEntities.Add(ms.AsNbtTileEntity())
                        ok <- true
                    spread <- spread + 1
                    if spread = 5 then  // give up if we looked a few blocks away and didn't find a suitable block to swap
                        ok <- true
            // put stripe on the ground (TODO vertical through air)
            let mutable pi,pj,pk = i,j,k
            while a.[pi,pj,pk]<>null do
                pj <- pj - 1
            let x,y,z = XYZ(pi,pj,pk)
            map.SetBlockIDAndDamage(x,y,z,73uy,0uy)  // 73 = redstone ore (lights up when things walk on it)
            i <- ni
            j <- nj
            k <- nk
            count <- count + 1
        // write out all the spawner data we just placed
        map.AddOrReplaceTileEntities(spawnerTileEntities)
        // put beacon at top end
        for x = ex-2 to ex+2 do
            for y = ey-4 to ey-1 do
                for z = ez-2 to ez+2 do
                    map.SetBlockIDAndDamage(x,y,z,166uy,0uy)  // barrier
        map.SetBlockIDAndDamage(ex,ey-2,ez,138uy,0uy) // beacon
        for x = ex-1 to ex+1 do
            for z = ez-1 to ez+1 do
                map.SetBlockIDAndDamage(x,ey-3,z,42uy,0uy)  // iron block
        // put treasure at bottom end
        for x = sx-2 to sx+2 do
            for z = sz-2 to sz+2 do
                map.SetBlockIDAndDamage(x,sy,z,22uy,0uy)  // lapis block
                map.SetBlockIDAndDamage(x,sy+3,z,22uy,0uy)  // lapis block
        map.SetBlockIDAndDamage(sx,sy,sz,89uy,0uy)  // glowstone
        for x = sx-2 to sx+2 do
            for y = sy+1 to sy+2 do
                for z = sz-2 to sz+2 do
                    map.SetBlockIDAndDamage(x,y,z,20uy,0uy)  // glass
        map.SetBlockIDAndDamage(sx,sy+1,sz,54uy,2uy)  // chest
        map.AddOrReplaceTileEntities([| [| Int("x",sx); Int("y",sy+1); Int("z",sz); String("id","Chest"); List("Items",Compounds[| |]); String("Lock",""); String("CustomName","Lootz!"); End |] |])
    // end foreach CC
    ()

////
(* MAP DEFAULTS 
ore    size tries
-----------------
dirt     33 10              3
gravel   33  8              13
granite  33 10        stone 1  1
diorite  33 10                 3
andesite 33 10                 5
coal     17 20              16
iron      9 20              15
gold      9  2              14
redstone  8  8              73 and 74
diamond   8  1              56
lapis     7  1              21
(emerald  1  3?  only extreme hills)   129
*)

let blockSubstitutionsEmpty =  // TODO want different ones, both as a function of x/z (difficulty in regions of map), biome?, and y (no spawners in wall above 63), anything else?
    [|
          3uy,0uy,    3uy,0uy;     // dirt -> 
         13uy,0uy,   13uy,0uy;     // gravel -> 
          1uy,1uy,    1uy,1uy;     // granite -> 
          1uy,3uy,    1uy,3uy;     // diorite -> 
          1uy,5uy,    1uy,5uy;     // andesite -> 
         16uy,0uy,   16uy,0uy;     // coal -> 
         15uy,0uy,   15uy,0uy;     // iron -> 
         14uy,0uy,   14uy,0uy;     // gold -> 
         73uy,0uy,   73uy,0uy;     // redstone -> 
         74uy,0uy,   74uy,0uy;     // lit_redstone -> 
         56uy,0uy,   56uy,0uy;     // diamond -> 
         21uy,0uy,   21uy,0uy;     // lapis -> 
        129uy,0uy,  129uy,0uy;     // emerald -> 
    |]

let blockSubstitutionsTrial =
    [|
          1uy,0uy,   97uy,0uy;     // stone -> silverfish
          1uy,3uy,   57uy,0uy;     // diorite -> diamond block
    |] // TODO what about tile entities like mob spawners? want to cache them per-chunk and then write them to chunks at end

let substituteBlocks(map:MapFolder) =
    let LOX, LOY, LOZ = -512, 11, -512
    let MAXI, MAXJ, MAXK = 1024, 50, 1024
    let mutable currentSectionBlocks,currentSectionBlockData,curx,cury,curz = null,null,-1000,-1000,-1000
    let XYZ(i,j,k) =
        let x = i-1 + LOX
        let y = j-1 + LOY
        let z = k-1 + LOZ
        x,y,z
    printf "SUBST"
    for j = 1 to MAXJ do
        printf "."
        for i = 1 to MAXI do
            for k = 1 to MAXK do
                let x,y,z = XYZ(i,j,k)
                if not(DIV(x,16) = DIV(curx,16) && DIV(y,16) = DIV(cury,16) && DIV(z,16) = DIV(curz,16)) then
                    let sect = map.GetOrCreateSection(x,y,z)
                    currentSectionBlocks <- sect |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
                    currentSectionBlockData <- sect |> Array.pick (function ByteArray("Data",a) -> Some a | _ -> None)
                    curx <- x
                    cury <- y
                    curz <- z
                let dx, dy, dz = (x+51200) % 16, y % 16, (z+51200) % 16
                let bix = dy*256 + dz*16 + dx
                let bid = currentSectionBlocks.[bix]
                let dmg = if bix%2=1 then currentSectionBlockData.[bix/2] >>> 4 else currentSectionBlockData.[bix/2] &&& 0xFuy
                for obid, odmg, nbid, ndmg in blockSubstitutionsTrial do
                    if bid = obid && dmg = odmg then
                        currentSectionBlocks.[bix] <- nbid
                        let mutable tmp = currentSectionBlockData.[bix/2]
                        if bix%2 = 0 then
                            tmp <- tmp &&& 0xF0uy
                            tmp <- tmp + ndmg
                        else
                            tmp <- tmp &&& 0x0Fuy
                            tmp <- tmp + (ndmg<<< 4)
                        currentSectionBlockData.[bix/2] <- tmp
    printfn ""

// mappings: should probably be to a chance set that's a function of difficulty or something...
// given that I can customize them, but want same custom settings for whole world generation, just consider as N buckets, but can e.g. customize the granite etc for more 'choice'...
// custom: dungeons at 100, probably lava/water lakes less frequent, biome size 3?

// customized preset code

// types of things
// stone -> silverfish probably
// -> spawners (multiple kinds, with some harder than others in different areas)
// -> primed tnt (and normal tnt? cue?)
// -> hidden lava pockets? (e.g. if something was like 1-40 for size-tries, can perforate area with tiny bits of X)
// -> glowstone or sea lanterns (block lights)
// -> some ore, but less and guarded
// moss stone -> netherrack in hell biome, for example
// -> coal/iron/gold/diamond _blocks_ rather than ore in some spots (coal burns!)

// set pieces (my own dungeons, persistent entities)

// in addition to block substitution, need .dat info for e.g. 'witch areas' or guardian zones'

// also need to code up basic mob spawner methods (passengers, effects, attributes, range, frequency, ...)

let makeCrazyMap() =
    let user = "Admin1"
    let map = new MapFolder("""C:\Users\"""+user+(sprintf """\AppData\Roaming\.minecraft\saves\seed31Copy\region\"""))
    substituteBlocks(map)
    findUndergroundAirSpaceConnectedComponents(map)
    printfn "saving results..."
    map.WriteAll()
    printfn "...done!"


//works:
// setblock ~ ~ ~20 mob_spawner 0 replace {SpawnPotentials:[{Entity:{id:Zombie},Weight:1,Properties:[]}],SpawnData:{id:Ghast},Delay:-1s,MaxNearbyEntities:6s,SpawnCount:4s,SpawnRange:4s,RequiredPlayerRange:16s,MinSpawnDelay:200s,MaxSpawnDelay:800s}


// {MaxNearbyEntities:6s,RequiredPlayerRange:16s,SpawnCount:4s,SpawnData:{id:"Skeleton"},MaxSpawnDelay:800s,Delay:329s,x:99977,y:39,z:-24,id:"MobSpawner",SpawnRange:4s,MinSpawnDelay:200s,SpawnPotentials:[0:{Entity:{id:"Skeleton"},Weight:1}]}
////////////////////////////////////////////

[<System.STAThread()>]  
do   
    //let user = "brianmcn"
    let user = "Admin1"
    //killAllEntities()
    //dumpChunkInfo("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\rrr\region\r.0.-3.mca""", 0, 31, 0, 31, true)
    //dumpSomeCommandBlocks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\SnakeGameByLorgon111\region\r.0.0.mca""")
    //dumpSomeCommandBlocks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\InstantReplay09\region\r.0.0.mca""")
    //dumpSomeCommandBlocks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Mandelbrot 1_9\region\r.0.0.mca""")
    //dumpSomeCommandBlocks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Learning\region\r.0.0.mca""")
    //diffRegionFiles("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmpy\region\r.0.0.mca""",
      //              """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmpy\region\r.0.0.mca.new""")
    //dumpSomeCommandBlocks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Seed9917 - Copy35e\region\r.0.0.mca""")
    //placeCertainEntitiesInTheWorld()
    // 45,000,012
    //dumpTileTicks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Purple\region\r.0.0.mca""")
    //placeCertainBlocksInTheWorld()
    //placeCommnadBlocksInTheWorld("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\BingoConcepts\region\r.0.0.mca""")
    //diffRegionFiles("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\BugRepro\region\r.0.0.mca""",
      //              """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\BugRepro\region\r.0.0.mca.new""")
    //dumpSomeCommandBlocks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\38a\region\r.0.0.mca""")
    //testing2()
    //placeCommandBlocksInTheWorldTemp("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\BugRepro\region\r.0.0.mca""")

    //mixTerrain()
    //findStrongholds()

    //printfn "%s" (makeCommandGivePlayerWrittenBook("Lorgon111", "BestTitle", [|"""["line1\n","line2"]"""; """["p2line1\n","p2line2",{"selector":"@p"}]"""|]))
    //dumpPlayerDat("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\fun with clone\playerdata\6fbefbde-67a9-4f72-ab2d-2f3ee5439bc0.dat""")
    //dumpPlayerDat("""C:\Users\"""+user+"""\Desktop\igloo_bottom.nbt""")

    
    //editMapDat("""C:\Users\"""+user+"""\Desktop\Eventide Trance v1.0.0 backup1\data\map_1.dat""")
    //testing2()
    //editMapDat("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp4\data\map_0.dat""")

    //mapDatToPng("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp9\data\map_0.dat""", """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp9\data\map_0.png""")
    //findAllLootBookItems("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\VoidLoot\region\""")
    //findAllLoot("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Seed5Normal\region\""")
    //findAllLoot("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\43aAt8200\region\""")
    //testBackpatching("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\VoidLoot\region\r.0.0.mca""")
    //makeBiomeMap()
    //repopulateAsAnotherBiome()
    //debugRegion()
    //findUndergroundAirSpaceConnectedComponents()
    //dumpPlayerDat("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\customized\level.dat""")
    //substituteBlocks()
    makeCrazyMap()
#if BINGO
    let save = "tmp9"
    //dumpTileTicks(sprintf """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\%s\region\r.0.0.mca""" save)
    //removeAllTileTicks(sprintf """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\%s\region\r.0.0.mca""" save)
    System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Void\region\r.0.0.mca""",
                        sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\r.0.0.mca""" user save, true)
    System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Void\region\r.0.-1.mca""",
                        sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\r.0.-1.mca""" user save, true)
    System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Void\region\r.-1.0.mca""",
                        sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\r.-1.0.mca""" user save, true)
    System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Void\region\r.-1.-1.mca""",
                        sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\r.-1.-1.mca""" user save, true)
    placeCommandBlocksInTheWorld(sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\r.0.0.mca""" user save)
    (*
    preciseImageToBlocks(sprintf """C:\Users\%s\Desktop\Minimap_Floor_6.png""" user, sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\""" user save, 36)
    preciseImageToBlocks(sprintf """C:\Users\%s\Desktop\Minimap_Floor_7.png""" user, sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\""" user save, 32)
    preciseImageToBlocks(sprintf """C:\Users\%s\Desktop\Minimap_Floor_8.png""" user, sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\""" user save, 28)
    *)
    System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp4\data\map_0.dat.new""",
                        sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\data\map_0.dat""" user save, true)
    System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp3\level.dat""",
                        sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\level.dat""" user save, true)
    System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp3\icon.png""",
                        sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\icon.png""" user save, true)

    //dumpSomeCommandBlocks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp9\region\r.0.0.mca""")
    //dumpSomeCommandBlocks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Seed9917 - Copy35e\region\r.0.0.mca""")
#endif

#if FUN
    placeCommandBlocksInTheWorldTemp("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\fun with clone\region\r.0.0.mca""")
#endif
    ()
