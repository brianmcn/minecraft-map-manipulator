module AA = ArtAssets

open MC_Constants
open NBT_Manipulation
open RegionFiles
open Utilities

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    

/////////////////////////////

let mutable signZ = 0

let placeCommandBlocksInTheWorld(fil,onlyPlaceArtThenFail) =
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
            [|  -1, "bone", AA.bone                         ; +8, "dye", AA.gray_dye                    ; +8, "dye", AA.gray_dye                    |]
            [|  -1, "ender_pearl", AA.ender_pearl           ; -1, "ender_pearl", AA.ender_pearl         ; -1, "slime_ball", AA.slime_ball           |]
            [|  -1, "vine", AA.vine                         ; -1, "deadbush", AA.deadbush               ; -1, "web", AA.web                         |]
            [|  -1, "brick", AA.brick                       ; -1, "flower_pot", AA.flower_pot           ; -1, "flower_pot", AA.flower_pot           |]
            [|  -1, "glass_bottle", AA.glass_bottle         ; -1, "glass_bottle", AA.glass_bottle       ; -1, "glass_bottle", AA.glass_bottle       |]
            [|  -1, "melon", AA.melon_slice                 ; -1, "melon", AA.melon_slice               ; -1, "speckled_melon", AA.speckled_melon   |]
            [|  +0, "dye", AA.ink_sac                       ; -1, "book", AA.book                       ; -1, "writable_book", AA.book_and_quill    |]
            [|  -1, "apple", AA.apple                       ; -1, "golden_shovel", AA.golden_shovel     ; -1, "golden_apple", AA.golden_apple       |]
            [|  -1, "flint", AA.flint                       ; -1, "flint", AA.flint                     ; -1, "flint_and_steel", AA.flint_and_steel |]
            [|  -1, "cookie", AA.cookie                     ; -1, "cookie", AA.cookie                   ; -1, "cookie", AA.cookie                   |]
            [|  -1, "pumpkin_seeds", AA.pumpkin_seeds       ; -1, "pumpkin_seeds", AA.pumpkin_seeds     ; -1, "pumpkin_pie", AA.pumpkin_pie         |]
            [|  -1, "rail", AA.rail                         ; -1, "rail", AA.rail                       ; -1, "rail", AA.rail                       |]
            [|  -1, "mushroom_stew", AA.mushroom_stew       ; -1, "mushroom_stew", AA.mushroom_stew     ; -1, "mushroom_stew", AA.mushroom_stew     |]
            [|  -1, "sugar", AA.sugar                       ; -1, "spider_eye", AA.spider_eye           ; -1, "fermented_spider_eye", AA.fermented_spider_eye |]
            [|  +2, "dye", AA.cactus_dye                    ; +2, "dye", AA.cactus_dye                  ;+10, "dye", AA.lime_dye                    |]
            [|  +4, "dye", AA.lapis                         ; +5, "dye", AA.purple_dye                  ; +6, "dye", AA.cyan_dye                    |]
            [|  -1, "emerald", AA.emerald                   ; -1, "emerald", AA.emerald                 ; -1, "emerald", AA.emerald                 |]
            [|  -1, "minecart", AA.minecart                 ; -1, "chest_minecart", AA.chest_minecart   ; -1, "tnt_minecart", AA.tnt_minecart       |]
            [|  -1, "gunpowder", AA.gunpowder               ; -1, "gunpowder", AA.gunpowder             ; -1, "gunpowder", AA.gunpowder             |]
            [|  -1, "compass", AA.compass                   ; -1, "compass", AA.compass                 ; -1, "map", AA.empty_map                   |]
            [|  +1, "sapling", AA.spruce_sapling            ; +1, "sapling", AA.spruce_sapling          ; +4, "sapling", AA.acacia_sapling          |]
            [|  -1, "cauldron", AA.cauldron                 ; -1, "cauldron", AA.cauldron               ; -1, "cauldron", AA.cauldron               |]
            [|  -1, "name_tag", AA.name_tag                 ; -1, "saddle", AA.saddle                   ; -1, "enchanted_book", AA.enchanted_book   |]
            [|  -1, "milk_bucket", AA.milk_bucket           ; -1, "egg", AA.egg                         ; -1, "cake", AA.cake                       |]
            [|  -1, "fish", AA.fish                         ; -1, "fish", AA.fish                       ; -1, "fish", AA.fish                       |]
            [|  -1, "sign", AA.sign                         ; -1, "item_frame", AA.item_frame           ; -1, "painting", AA.painting               |]
            [|  -1, "golden_sword", AA.golden_sword         ; -1, "clock", AA.clock                     ; -1, "golden_rail", AA.golden_rail         |]
            [|  -1, "hopper", AA.hopper                     ; -1, "hopper", AA.hopper                   ; -1, "hopper", AA.hopper                   |]
            [|  -1, "repeater", AA.repeater                 ; -1, "repeater", AA.repeater               ; -1, "repeater", AA.repeater               |]
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
    if onlyPlaceArtThenFail then 
        region.PlaceCommandBlocksStartingAtSelfDestruct(0,0,0,[| O "clone 1 0 1 150 0 150 1 3 1"; U "clone 1 1 1 150 1 150 1 4 1"; U "give @p filled_map 64 0"; U "give @p filled_map 64 1" |],"relight/heightmap and give map")
        region.Write(fil+".new")
        System.IO.File.Delete(fil)
        System.IO.File.Move(fil+".new",fil)
        failwith "throwing"
    let MAPX, MAPY, MAPZ = 0, 19, 0
    AA.writeZoneFromString(region, MAPX, MAPY, MAPZ, AA.mapTopLeft)
    AA.writeZoneFromString(region, MAPX+64, MAPY, MAPZ, AA.mapTopRight)
    AA.writeZoneFromString(region, MAPX, MAPY, MAPZ+64, AA.mapBottomLeft)
    AA.writeZoneFromString(region, MAPX+64, MAPY, MAPZ+64, AA.mapBottomRight)
    for x = 1 to 128 do
        for z = 1 to 128 do
            region.EnsureSetBlockIDAndDamage(x, MAPY-1, z, 1uy, 0uy)  // stone below it, to prevent lighting updates
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
        for i = 9 to 17 do
            for j = 0 to 2 do
                let dmg,item,_art = otherItems.[i].[j]
                sb.Append(sprintf """{Slot:%db,id:"%s",Count:%db,Damage:%ds},""" (i-9+(9*j)) item 1 (if dmg = -1 then 0 else dmg)) |> ignore
        let s = sb.ToString()
        s.Substring(0, s.Length-1) + "]}"
    let otherChest3 =
        let sb = new System.Text.StringBuilder("""{CustomName:"Easy/Medium/Hard in row 1/2/3",Items:[""")
        for i = 18 to otherItems.Count-1 do
            for j = 0 to 2 do
                let dmg,item,_art = otherItems.[i].[j]
                sb.Append(sprintf """{Slot:%db,id:"%s",Count:%db,Damage:%ds},""" (i-18+(9*j)) item 1 (if dmg = -1 then 0 else dmg)) |> ignore
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

    let FINALIZE_PRIOR_GAME_LOGIC = Coords(47,3,10)
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
    let COMPUTE_LOCKOUT_GOAL = Coords(61,3,10)

    let VANILLA_LOADOUT = Coords(70,3,10), "Vanilla gameplay (no on-start/on-respawn commands)"
    let NIGHT_VISION_LOADOUT = Coords(71,3,10), "Players get night vision at start & respawn"
    let SADDLED_HORSE_NIGHT_VISION_LOADOUT = Coords(72,3,10), "Players get night vision and frost walker at start & respawn, as well as an invulnerable saddled horse at game start"
    let STARTING_CHEST_NIGHT_VISION_LOADOUT = Coords(73,3,10), "Players get night vision at start & respawn, and each team starts with chest of items"
    let SPAMMABLE_SWORD_NIGHT_VISION_LOADOUT = Coords(74,3,10), "Players get night vision at start & respawn, as well as a spammable unbreakable iron sword at game start"
    let ELYTRA_JUMP_BOOST_FROST_WALKER_NIGHT_VISION_LOADOUT = Coords(75,3,10), "Players get night vision, frost walker, elytra, and jump boost potions at start & respawn"
    let ALL_LOADOUTS = [VANILLA_LOADOUT; NIGHT_VISION_LOADOUT; SADDLED_HORSE_NIGHT_VISION_LOADOUT; STARTING_CHEST_NIGHT_VISION_LOADOUT; SPAMMABLE_SWORD_NIGHT_VISION_LOADOUT; ELYTRA_JUMP_BOOST_FROST_WALKER_NIGHT_VISION_LOADOUT]

    let PILLAR_UP_THE_ARMOR_STAND = Coords(90,3,10)
    let COMPUTE_Y_ARMOR_STAND_LOW = Coords(91,3,10)

    let NOTICE_DROPPED_MAP_CMDS = Coords(103,3,10)
    
    //////////////////////////////
    // lobby
    //////////////////////////////

    // IWIDTH/ILENGTH are interior measures, not including walls
    let LOBBYX, LOBBYY, LOBBYZ = 50, 6, 50  // alcove goes to Z-1
    let NEW_PLAYER_PLATFORM_LO = Coords(60,LOBBYY,30)
    let NEW_PLAYER_LOCATION = NEW_PLAYER_PLATFORM_LO.Offset(5,2,5)
    let NEW_MAP_PLATFORM_LO = Coords(60,LOBBYY,90)
    let NEW_MAP_LOCATION = NEW_MAP_PLATFORM_LO.Offset(3,2,5)
    let CFG_ROOM_IWIDTH = 7
    let MAIN_ROOM_IWIDTH = 7
    let INFO_ROOM_IWITDH = 7
    let TOTAL_WIDTH = 1 + CFG_ROOM_IWIDTH + 2 + MAIN_ROOM_IWIDTH + 2 + INFO_ROOM_IWITDH + 1
    let ILENGTH = 13
    let LENGTH = ILENGTH + 2
    let HEIGHT = 6
    let NUM_CONFIG_COMMANDS = 7
    let OFFERING_SPOT = Coords(LOBBYX+TOTAL_WIDTH-INFO_ROOM_IWITDH/2-2,LOBBYY+1,LOBBYZ+4)
    let LOBBY_CENTER_LOCATION = Coords(LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH/2+3, LOBBYY+2, LOBBYZ+3)
    let makeSignBoldness kind x y z dmg txt1 b1 txt2 b2 txt3 b3 txt4 b4 =
        [|
            U (sprintf "setblock %d %d %d %s %d" x y z kind dmg)
            U (sprintf """blockdata %d %d %d {x:%d,y:%d,z:%d,id:"Sign",Text1:"{\"text\":\"%s\",\"bold\":\"%s\"}",Text2:"{\"text\":\"%s\",\"bold\":\"%s\"}",Text3:"{\"text\":\"%s\",\"bold\":\"%s\"}",Text4:"{\"text\":\"%s\",\"bold\":\"%s\"}"}""" x y z x y z txt1 b1 txt2 b2 txt3 b3 txt4 b4)
        |]
    let makeSign kind x y z dmg txt1 txt2 txt3 txt4 = makeSignBoldness kind x y z dmg txt1 "true" txt2 "true" txt3 "true" txt4 "true"
    let makeWallSign x y z dmg txt1 txt2 txt3 txt4 = makeSign "wall_sign" x y z dmg txt1 txt2 txt3 txt4 
    let makeWallSignActivate x y z dmg txt1 txt2 (a:Coords) isBold color =
        let bc = sprintf """,\"bold\":\"%s\",\"color\":\"%s\" """ (if isBold then "true" else "false") color
        let c1 = if isBold then sprintf """,\"clickEvent\":{\"action\":\"run_command\",\"value\":\"blockdata %d %d %d {auto:1b}\"} """ a.X a.Y a.Z else ""
        let c2 = if isBold then sprintf """,\"clickEvent\":{\"action\":\"run_command\",\"value\":\"blockdata %d %d %d {auto:0b}\"} """ a.X a.Y a.Z else ""
        [|
            U (sprintf "setblock %d %d %d wall_sign %d" x y z dmg)
            U (sprintf """blockdata %d %d %d {x:%d,y:%d,z:%d,id:"Sign",Text1:"{\"text\":\"%s\"%s%s}",Text2:"{\"text\":\"%s\"%s%s}"}"""  x y z x y z txt1 bc c1 txt2 bc c2)
        |]
    let makeSignDoAction kind x y z dmg txt1 txt2 txt3 txt4 a1 cmd1 a2 cmd2 a3 cmd3 a4 cmd4 isBold color =
        let bc = sprintf """,\"bold\":\"%s\",\"color\":\"%s\" """ (if isBold then "true" else "false") color
        let c1 = if isBold then sprintf """,\"clickEvent\":{\"action\":\"%s\",\"value\":\"%s\"} """ a1 cmd1 else ""
        let c2 = if isBold then sprintf """,\"clickEvent\":{\"action\":\"%s\",\"value\":\"%s\"} """ a2 cmd2 else ""
        let c3 = if isBold then sprintf """,\"clickEvent\":{\"action\":\"%s\",\"value\":\"%s\"} """ a3 cmd3 else ""
        let c4 = if isBold then sprintf """,\"clickEvent\":{\"action\":\"%s\",\"value\":\"%s\"} """ a4 cmd4 else ""
        [|
            U (sprintf "setblock %d %d %d %s %d" x y z kind dmg)
            U (sprintf """blockdata %d %d %d {x:%d,y:%d,z:%d,id:"Sign",Text1:"{\"text\":\"%s\"%s%s}",Text2:"{\"text\":\"%s\"%s%s}",Text3:"{\"text\":\"%s\"%s%s}",Text4:"{\"text\":\"%s\"%s%s}"}""" x y z x y z txt1 bc c1 txt2 bc c2 txt3 bc c3 txt4 bc c4)
        |]
    let makeSignDo kind x y z dmg txt1 txt2 txt3 txt4 cmd1 cmd2 isBold color = makeSignDoAction kind x y z dmg txt1 txt2 txt3 txt4 "run_command" cmd1 "run_command" cmd2 "run_command" "" "run_command" "" isBold color 
    let makeWallSignDo x y z dmg txt1 txt2 txt3 txt4 cmd1 cmd2 isBold color =
        makeSignDo "wall_sign" x y z dmg txt1 txt2 txt3 txt4 cmd1 cmd2 isBold color
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
            // light up what's visible through the windows
            yield U "fill 1 0 1 128 0 128 sea_lantern 0 replace air"
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
            // lockout alcove
            yield U (sprintf "fill %d %d %d %d %d %d air" (LOBBYX+CFG_ROOM_IWIDTH/2+0) (LOBBYY+1) (LOBBYZ) (LOBBYX+CFG_ROOM_IWIDTH/2+2) (LOBBYY+3) (LOBBYZ))
            yield U (sprintf "fill %d %d %d %d %d %d minecraft:stained_hardened_clay 10" (LOBBYX+CFG_ROOM_IWIDTH/2+0) (LOBBYY+1) (LOBBYZ-1) (LOBBYX+CFG_ROOM_IWIDTH/2+2) (LOBBYY+3) (LOBBYZ-1))
            yield U (sprintf "fill %d %d %d %d %d %d wool 13" (LOBBYX+CFG_ROOM_IWIDTH/2+0) (LOBBYY) (LOBBYZ-1) (LOBBYX+CFG_ROOM_IWIDTH/2+2) (LOBBYY) (LOBBYZ-1))
            yield U (sprintf "setblock %d %d %d purpur_slab 8" (LOBBYX+CFG_ROOM_IWIDTH) (LOBBYY+2) (LOBBYZ+1)) // for standing sign to rest atop
            // floor & ceiling
            yield U (sprintf "fill %d %d %d %d %d %d sea_lantern" LOBBYX LOBBYY LOBBYZ (LOBBYX+TOTAL_WIDTH-1) LOBBYY (LOBBYZ+LENGTH-1))
            yield U (sprintf "fill %d %d %d %d %d %d sea_lantern" LOBBYX (LOBBYY+HEIGHT) LOBBYZ (LOBBYX+TOTAL_WIDTH-1) (LOBBYY+HEIGHT) (LOBBYZ+LENGTH-1))
            // carpet
            yield U (sprintf "fill %d %d %d %d %d %d carpet 13 replace air" LOBBYX (LOBBYY+1) LOBBYZ (LOBBYX+TOTAL_WIDTH-1) (LOBBYY+1) (LOBBYZ+LENGTH-1))
            yield U (sprintf "fill %d %d %d %d %d %d carpet 8 replace carpet" (LOBBYX+3) (LOBBYY+1) (LOBBYZ+3) (LOBBYX+5) (LOBBYY+1) (LOBBYZ+LENGTH-3))
            yield U (sprintf "fill %d %d %d %d %d %d carpet 8 replace carpet" (LOBBYX+CFG_ROOM_IWIDTH+5) (LOBBYY+1) (LOBBYZ+3) (LOBBYX+CFG_ROOM_IWIDTH+7) (LOBBYY+1) (LOBBYZ+LENGTH-3))
            yield U (sprintf "fill %d %d %d %d %d %d carpet 8 replace carpet" (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+7) (LOBBYY+1) (LOBBYZ+3) (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+9) (LOBBYY+1) (LOBBYZ+LENGTH-3))
            yield U (sprintf "fill %d %d %d %d %d %d carpet 8 replace carpet" (LOBBYX+3) (LOBBYY+1) (LOBBYZ+LENGTH-4) (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+8) (LOBBYY+1) (LOBBYZ+LENGTH-3))
            // cfg room blocks
            yield U (sprintf "fill %d %d %d %d %d %d chain_command_block 3" (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+3) (LOBBYZ+2) (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+3) (LOBBYZ+2+NUM_CONFIG_COMMANDS-1))
            yield U (sprintf "fill %d %d %d %d %d %d chain_command_block 3" (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+1) (LOBBYZ+2) (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+1) (LOBBYZ+2+NUM_CONFIG_COMMANDS-1))
            yield U (sprintf "setblock %d %d %d chest 5" (LOBBYX+1) (LOBBYY+1) (LOBBYZ+1))
            yield U (sprintf "setblock %d %d %d wool 13" (LOBBYX+1) (LOBBYY) (LOBBYZ+1)) // wool under chest
            // put heads
            yield U (sprintf "/summon ArmorStand %f %f %f {NoGravity:1,Marker:1,Invisible:1,ArmorItems:[{},{},{},{id:skull,Damage:3,tag:{SkullOwner:Lorgon111}}]}" (float (LOBBYX+TOTAL_WIDTH-5) + 0.5) (float (LOBBYY+2) - 1.1) (float (LOBBYZ+1) - 0.0))
            yield U (sprintf "/summon ArmorStand %f %f %f {Tags:[\"asToReverse\"],NoGravity:1,Marker:1,Invisible:1,ArmorItems:[{},{},{},{id:skull,Damage:3,tag:{SkullOwner:Lorgon111}}]}" (float (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH/2+3) + 0.5) (float (LOBBYY+2) - 1.1) (float (LOBBYZ+14) - 0.0))
            yield! nTicksLater(1)
            yield U "tp @e[tag=asToReverse] ~ ~ ~-0.01 180 0"
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
    let customConfigBookCmd = makeCommandGivePlayerWrittenBook("Lorgon111","Custom game config", [|
            // TODO finalize prose
            """{"text":"MinecraftBINGO allows you to customize the gameplay with extra command blocks. There are two banks of command blocks you can customize."}"""
            """{"text":"The first bank of command blocks contains commands that run at the start of the game.\n\nThe second bank runs when a player respawns after death."}"""
            """{"text":"You can modify these command blocks (in creative mode) to do a variety of things. But also..."}"""
            """{"text":"There are various right-clickable signs labelled 'Load config' which you click to populate the command blocks with various fun pre-set configs."}"""
            """{"text":"So you can choose one of the 'pre-set' configs just by clicking a sign, or if you know command blocks, you can create your own."}"""
            |] )
    // TODO community (reddit, twitter)
    let placeSigns(enabled) =
        [|
            yield O ""
            // interior layout - cfg room
            yield! makeSign "standing_sign" (LOBBYX+CFG_ROOM_IWIDTH/2+0) (LOBBYY+1) (LOBBYZ) 0 "Lockout mode:" "Once one team" "gets a square," "no other"
            yield! makeSign "standing_sign" (LOBBYX+CFG_ROOM_IWIDTH/2+1) (LOBBYY+1) (LOBBYZ) 0 "team can get" "that square." "Every square" "is a race!"
            yield! makeSign "standing_sign" (LOBBYX+CFG_ROOM_IWIDTH/2+2) (LOBBYY+1) (LOBBYZ) 0 "First to BINGO" "or get score" "'LockoutGoal'" "wins the game!"
            yield! makeWallSignActivate (LOBBYX+CFG_ROOM_IWIDTH/2+1) (LOBBYY+2) (LOBBYZ) 3 "Toggle" "Lockout Mode" TOGGLE_LOCKOUT_BUTTON enabled (if enabled then "black" else "gray")
#if DEBUG
            yield! makeWallSignDo (LOBBYX+CFG_ROOM_IWIDTH/2+3) (LOBBYY+3) (LOBBYZ+1) 3 "enable" "ticklagdebug" "" "" "" "scoreboard players set @p TickInfo 1" true "black" // TODO eventually remove this
            yield! makeWallSignDo (LOBBYX+CFG_ROOM_IWIDTH/2+3) (LOBBYY+2) (LOBBYZ+1) 3 "disable" "ticklagdebug" "" "" "" "scoreboard players set @p TickInfo 0" true "black" // TODO eventually remove this
#endif
            yield! makeSign "standing_sign" (LOBBYX+CFG_ROOM_IWIDTH) (LOBBYY+3) (LOBBYZ+1) 4 "Commands run" "at game" "start" "-->"
            yield! makeSign "standing_sign" (LOBBYX+CFG_ROOM_IWIDTH) (LOBBYY+1) (LOBBYZ+1) 4 "Commands run" "at each death" "(respawn)" "-->"
            yield U (sprintf "setblock %d %d %d wool 13" (LOBBYX+CFG_ROOM_IWIDTH) (LOBBYY) (LOBBYZ+1)) // wool under sign
            let mkLoadout x y z d txt1 txt2 txt3 ((c:Coords),tellPlayers) =
                makeWallSignDo x y z d "Load config:" txt1 txt2 txt3 (sprintf """tellraw @a [{\\\"text\\\":\\\"Configuration loaded: %s\\\",\\\"color\\\":\\\"green\\\"}]""" tellPlayers) (sprintf "clone %s %s %d %d %d masked" c.STR (c.Offset(0,2,NUM_CONFIG_COMMANDS-1).STR) (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+1) (LOBBYZ+2)) enabled (if enabled then "black" else "gray")
            yield! makeWallSignDo (LOBBYX+1) (LOBBYY+2) (LOBBYZ+13) 5 "Learn about" "custom game" "configs" "" (escape2 customConfigBookCmd) "" true "black"
            yield! mkLoadout (LOBBYX+1) (LOBBYY+2) (LOBBYZ+11) 5 "Vanilla" "(no extra" "commands)" VANILLA_LOADOUT
            yield! mkLoadout (LOBBYX+1) (LOBBYY+2) (LOBBYZ+9) 5 "Night Vision" "" "" NIGHT_VISION_LOADOUT
            yield! mkLoadout (LOBBYX+1) (LOBBYY+2) (LOBBYZ+7) 5 "Spammable" "Iron Sword" "+Night Vision" SPAMMABLE_SWORD_NIGHT_VISION_LOADOUT
            yield! mkLoadout (LOBBYX+1) (LOBBYY+2) (LOBBYZ+5) 5 "Saddled Horse" "+Frost Wlker" "+Night Vision" SADDLED_HORSE_NIGHT_VISION_LOADOUT
            yield! mkLoadout (LOBBYX+1) (LOBBYY+2) (LOBBYZ+3) 5 "Elytra" "+Frost Walker" "+Night Vision" ELYTRA_JUMP_BOOST_FROST_WALKER_NIGHT_VISION_LOADOUT
            yield! mkLoadout (LOBBYX+1) (LOBBYY+2) (LOBBYZ+1) 5 "Starting Chest" "Per Team" "+Night Vision" STARTING_CHEST_NIGHT_VISION_LOADOUT
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
            yield! makeSignDoAction "wall_sign" (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+2) (LOBBYY+2) (LOBBYZ+5) 4 "Join team" "RED"    "" "" "run_command" "scoreboard teams join red @p"    "run_command" "scoreboard players set @p Score 0" "run_command" (sprintf "blockdata %s {auto:1b}" COMPUTE_LOCKOUT_GOAL.STR) "run_command" (sprintf "blockdata %s {auto:0b}" COMPUTE_LOCKOUT_GOAL.STR) enabled (if enabled then "black" else "gray")
            yield! makeSignDoAction "wall_sign" (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+2) (LOBBYY+2) (LOBBYZ+6) 4 "Join team" "BLUE"   "" "" "run_command" "scoreboard teams join blue @p"   "run_command" "scoreboard players set @p Score 0" "run_command" (sprintf "blockdata %s {auto:1b}" COMPUTE_LOCKOUT_GOAL.STR) "run_command" (sprintf "blockdata %s {auto:0b}" COMPUTE_LOCKOUT_GOAL.STR) enabled (if enabled then "black" else "gray")
            yield! makeSignDoAction "wall_sign" (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+2) (LOBBYY+2) (LOBBYZ+7) 4 "Join team" "YELLOW" "" "" "run_command" "scoreboard teams join yellow @p" "run_command" "scoreboard players set @p Score 0" "run_command" (sprintf "blockdata %s {auto:1b}" COMPUTE_LOCKOUT_GOAL.STR) "run_command" (sprintf "blockdata %s {auto:0b}" COMPUTE_LOCKOUT_GOAL.STR) enabled (if enabled then "black" else "gray")
            yield! makeSignDoAction "wall_sign" (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+2) (LOBBYY+2) (LOBBYZ+8) 4 "Join team" "GREEN"  "" "" "run_command" "scoreboard teams join green @p"  "run_command" "scoreboard players set @p Score 0" "run_command" (sprintf "blockdata %s {auto:1b}" COMPUTE_LOCKOUT_GOAL.STR) "run_command" (sprintf "blockdata %s {auto:0b}" COMPUTE_LOCKOUT_GOAL.STR) enabled (if enabled then "black" else "gray")
            yield! makeSign "standing_sign" (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH/2+0) (LOBBYY+1) (LOBBYZ+13) 8 "Custom" "Settings" """----->\",\"strikethrough\":\"true""" ""
            yield! makeSign "standing_sign" (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH/2+3) (LOBBYY+1) (LOBBYZ+13) 8 "Welcome to" "MinecraftBINGO" "by Dr. Brian" "Lorgon111"
            yield! makeSign "standing_sign" (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH/2+6) (LOBBYY+1) (LOBBYZ+13) 8 "Game" "Info" """<-----\",\"strikethrough\":\"true""" ""
            yield U (sprintf "setblock %d %d %d wool 13" (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH/2+0) (LOBBYY) (LOBBYZ+13)) // wool under sign
            yield U (sprintf "setblock %d %d %d wool 13" (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH/2+3) (LOBBYY) (LOBBYZ+13)) // wool under sign
            yield U (sprintf "setblock %d %d %d wool 13" (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH/2+6) (LOBBYY) (LOBBYZ+13)) // wool under sign
            // interior layout - info room
            yield! makeWallSignDo (LOBBYX+TOTAL_WIDTH-2) (LOBBYY+2) (LOBBYZ+4) 4 "Learn about" "basic rules" "and gameplay" "" (escape2 gameplayBookCmd) "" true "black"
            yield! makeWallSignDo (LOBBYX+TOTAL_WIDTH-2) (LOBBYY+2) (LOBBYZ+6) 4 "Learn about" "various" "game modes" "" (escape2 gameModesBookCmd) "" true "black"
            yield! makeWallSignDo (LOBBYX+TOTAL_WIDTH-2) (LOBBYY+2) (LOBBYZ+8) 4 "Learn about" "this world's" "custom terrain" "" (escape2 customTerrainBookCmd) "" true "black"
            yield! makeWallSignDo (LOBBYX+TOTAL_WIDTH-2) (LOBBYY+2) (LOBBYZ+10) 4 "Learn about" "all the folks" "who helped" "" (escape2 thanksBookCmd) "" true "black"
            yield! makeSign "standing_sign" (LOBBYX+TOTAL_WIDTH-5) (LOBBYY+1) (LOBBYZ+1) 0 "Thanks for" "playing!" "" ""
            yield U (sprintf "setblock %d %d %d wool 13" (LOBBYX+TOTAL_WIDTH-5) (LOBBYY) (LOBBYZ+1)) // wool under sign
            yield! makeWallSignActivate (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+5) (LOBBYY+2) (LOBBYZ+8) 5 "Show all" "possible items" SHOW_ITEMS_BUTTON true "black"
            yield U (sprintf "setblock %d %d %d wool 8" (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+7) (LOBBYY) (LOBBYZ+8)) // wool under chests
            yield U (sprintf "setblock %d %d %d wool 8" (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+9) (LOBBYY) (LOBBYZ+8)) // wool under chests
            yield U (sprintf "setblock %d %d %d wool 8" (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+7) (LOBBYY) (LOBBYZ+10)) // wool under chests
            yield U (sprintf "setblock %d %d %d wool 8" (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+9) (LOBBYY) (LOBBYZ+10)) // wool under chests
            yield! makeWallSignDo (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+5) (LOBBYY+2) (LOBBYZ+6) 5 "Version" "Info" "" "" (escape2 versionInfoBookCmd) "" true "black"
            yield! makeWallSign (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+5) (LOBBYY+2) (LOBBYZ+4) 5 "donate" "" "" ""
            // start platform has a disable-able sign
            let GTT = NEW_PLAYER_PLATFORM_LO.Offset(7,2,6)
            yield U (sprintf "setblock %s stone" (GTT.Offset(1,0,0).STR))
            yield! makeSignDo "wall_sign" GTT.X GTT.Y GTT.Z 4 "Right-click" "me to go to" "TUTORIAL" "" (sprintf "blockdata %s {auto:1b}" START_TUTORIAL_BUTTON.STR) (sprintf "blockdata %s {auto:0b}" START_TUTORIAL_BUTTON.STR) enabled (if enabled then "black" else "gray")
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
    loadout([||],[||],fst VANILLA_LOADOUT,"vanillaLoadout")
    let nightVisionLoadout =
        [|
            U "effect @a night_vision 9999 1 true"
        |],
        [|
            U "effect @a[tag=justRespawned] night_vision 9999 1 true"
        |], 
        fst NIGHT_VISION_LOADOUT, "nightVisionLoadout"
    loadout(nightVisionLoadout)
    let saddledHorseNightVisionLoadout =
        [|
            U "effect @a night_vision 9999 1 true"
            U """replaceitem entity @a slot.armor.feet minecraft:leather_boots 1 0 {Unbreakable:1,ench:[{lvl:2s,id:9s}]}"""
            U """execute @a ~ ~ ~ summon EntityHorse ~ ~2 ~ {Tame:1b,Attributes:[0:{Base:40.0d,Name:"generic.maxHealth"},1:{Base:0.0d,Name:"generic.knockbackResistance"},2:{Base:0.3d,Name:"generic.movementSpeed"},3:{Base:0.0d,Name:"generic.armor"},4:{Base:16.0d,Name:"generic.followRange"},5:{Base:0.7d,Name:"horse.jumpStrength"}],Invulnerable:1b,Health:40.0f,SaddleItem:{id:"minecraft:saddle",Count:1b,Damage:0s}}"""
        |],
        [|
            U "effect @a[tag=justRespawned] night_vision 9999 1 true"
            U """replaceitem entity @a[tag=justRespawned] slot.armor.feet minecraft:leather_boots 1 0 {Unbreakable:1,ench:[{lvl:2s,id:9s}]}"""
        |], 
        fst SADDLED_HORSE_NIGHT_VISION_LOADOUT, "saddledHorseNightVisionLoadout"
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
        fst STARTING_CHEST_NIGHT_VISION_LOADOUT, "startingChestNightVisionLoadout"
    loadout(startingChestNightVisionLoadout)
    let spammableSwordNightVisionLoadout =
        [|
            U "effect @a night_vision 9999 1 true"
            U """/give @a minecraft:iron_sword 1 0 {display:{Name:"Spammable unbreakable sword"},Unbreakable:1,AttributeModifiers:[{AttributeName:"generic.attackSpeed",Name:"Speed",Slot:"mainhand",Amount:1020.0,Operation:0,UUIDLeast:111l,UUIDMost:111l},{AttributeName:"generic.attackDamage",Name:"Damage",Slot:"mainhand",Amount:4.0,Operation:0,UUIDLeast:222l,UUIDMost:222l}]}"""
        |],
        [|
            U "effect @a[tag=justRespawned] night_vision 9999 1 true"
        |], 
        fst SPAMMABLE_SWORD_NIGHT_VISION_LOADOUT, "spammableSwordNightVisionLoadout"
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
        fst ELYTRA_JUMP_BOOST_FROST_WALKER_NIGHT_VISION_LOADOUT, "elytraJumpBoostFrostWalkerNightVisionLoadout"
    loadout(elytraJumpBoostFrostWalkerNightVisionLoadout)

    //////////////////////////////
    // tutorial
    //////////////////////////////
    let TUTORIAL_LOCATION = Coords(-90, 2, -120)
    let TUTORIAL_PLAYER_START = TUTORIAL_LOCATION.Offset(-3,2,2)
    let TUTORIAL_CMDS = Coords(80,3,10)
    let makeStandingSignIncZ args = 
        let r = makeSign "standing_sign" args
        signZ <- signZ + 1
        r
    let makeTutorialCmds =
        [|
            let tut = TUTORIAL_LOCATION
            let signY = TUTORIAL_LOCATION.Y+1
            let signX = TUTORIAL_LOCATION.X-1
            yield O ""
            yield U (sprintf "fill %s %s stone" (tut.Offset( 0,0,-1).STR) (tut.Offset(-5,4,-1).STR))
            yield U (sprintf "fill %s %s stone" (tut.Offset( 0,0, 0).STR) (tut.Offset( 0,4,30).STR))
            yield U (sprintf "fill %s %s stone" (tut.Offset(-5,0, 0).STR) (tut.Offset(-5,4,30).STR))
            yield U (sprintf "fill %s %s sea_lantern" (tut.Offset(-5,0,0).STR) (tut.Offset(0,0,30).STR))
            signZ <- TUTORIAL_LOCATION.Z + 1
            yield! makeStandingSignIncZ signX signY signZ 4  "(In this map," "_wall_ signs" "can be" "right-clicked)"
            yield! makeStandingSignIncZ signX signY signZ 4 "Welcome to" "MinecraftBINGO" "by Dr. Brian" "Lorgon111"
            yield! makeStandingSignIncZ signX signY signZ 4 "MinecraftBINGO" "uses" "clickable signs" ""
            yield! makeWallSignDo signX (signY+1) signZ 4 "Right-click" "this sign" "to continue" "" (sprintf "tp @p %s 90 180" (TUTORIAL_PLAYER_START.Offset(0,0,5).STR)) "" true "black"
            signZ <- signZ + 2
            yield U (sprintf "fill %s %s stone" (tut.Offset( 0,0,signZ-TUTORIAL_LOCATION.Z).STR) (tut.Offset(-5,4,signZ-TUTORIAL_LOCATION.Z).STR))
            signZ <- signZ + 2
            yield! makeStandingSignIncZ signX signY signZ 4 "MinecraftBINGO" "plays as" "new-world" "survival Minecraft"
            yield! makeStandingSignIncZ signX signY signZ 4 "You'll need to" "punch trees," "craft tools," "and eat"
            yield! makeStandingSignIncZ signX signY signZ 4 "But you're" "in a race" "to complete" "a goal"
            signZ <- signZ + 1
            yield! makeStandingSignIncZ signX signY signZ 4 "There are" "25 items" "pictured on" "the BINGO card"
            yield! makeStandingSignIncZ signX signY signZ 4 "You want to" "get items" "as fast as" "you can"
            yield! makeStandingSignIncZ signX signY signZ 4 "Goal is 'BINGO'" "5 in a row," "column, or" "diagonal"
            signZ <- signZ + 1
            yield! makeStandingSignIncZ signX signY signZ 4 "Try getting" "an item now" "" ""
            yield! makeStandingSignIncZ signX signY signZ 4 "Punch down some" "sugar cane" "and craft it" "into sugar"
            signZ <- signZ + 1
            yield! makeStandingSignIncZ signX signY signZ 4 "When you get" "an item," "your score" "will update"
            yield! makeStandingSignIncZ signX signY signZ 4 "You can see" "what items" "you've gotten" "by..."
            yield! makeStandingSignIncZ signX signY signZ 4 "...holding" "your maps and" "dropping" "one copy"
            yield! makeStandingSignIncZ signX signY signZ 4 "(The 'drop'" "key is 'Q'" "by default)" ""
            yield! makeStandingSignIncZ signX signY signZ 4 "Try it now!" "(drop a" "BINGO Card)" ""
            signZ <- signZ + 1
            yield! makeStandingSignIncZ signX signY signZ 4 "Once you get" "5 in a row," "you win!" ""
            yield! makeStandingSignIncZ signX signY signZ 4 "Other game" "modes exist," "learn more" "in the lobby"
            yield! makeWallSignActivate signX (signY+1) signZ 4 "Let's play!" "Click to start" END_TUTORIAL_BUTTON true "black"  // TODO multiplayer state testing of tutorial
            signZ <- signZ + 1
            yield U (sprintf "fill %s %s stone" (tut.Offset( 0,0,signZ-TUTORIAL_LOCATION.Z).STR) (tut.Offset(-5,4,signZ-TUTORIAL_LOCATION.Z).STR))
            // first time map is loaded, players go here:
            yield U (sprintf "fill %s %s sea_lantern" NEW_MAP_PLATFORM_LO.STR (NEW_MAP_PLATFORM_LO.Offset(10,0,10).STR))
            let GTL = NEW_PLAYER_PLATFORM_LO.Offset(7,1,4)
            yield! makeSign "standing_sign" (NEW_MAP_PLATFORM_LO.X+7) (NEW_MAP_PLATFORM_LO.Y+1) (NEW_MAP_PLATFORM_LO.Z+2) 4 "Welcome to" "MinecraftBINGO" "by Dr. Brian" "Lorgon111"
            yield! makeSign "standing_sign" (NEW_MAP_PLATFORM_LO.X+7) (NEW_MAP_PLATFORM_LO.Y+1) (NEW_MAP_PLATFORM_LO.Z+3) 4 "This is version" "3.0 Beta" "of the map." ""
            yield! makeSign "standing_sign" (NEW_MAP_PLATFORM_LO.X+7) (NEW_MAP_PLATFORM_LO.Y+1) (NEW_MAP_PLATFORM_LO.Z+4) 4 "Do you have" "the latest" "version?" "Find out!"
            // TODO figure out best URL
            let downloadUrl = "https://twitter.com/MinecraftBINGO"
            let downloadCmd1 = escape2 <| sprintf """tellraw @a {"text":"Press 't' (chat), then click line below to visit the official download page for MinecraftBINGO"}"""
            let downloadCmd2 = escape2 <| sprintf """tellraw @a {"text":"%s","underlined":"true","clickEvent":{"action":"open_url","value":"%s"}}""" downloadUrl downloadUrl
            yield U (sprintf "fill %d %d %d %d %d %d stone" (NEW_MAP_PLATFORM_LO.X+8) (NEW_MAP_PLATFORM_LO.Y+2) (NEW_MAP_PLATFORM_LO.Z+5) (NEW_MAP_PLATFORM_LO.X+8) (NEW_MAP_PLATFORM_LO.Y+2) (NEW_MAP_PLATFORM_LO.Z+6))
            yield! makeSignDo "wall_sign" (NEW_MAP_PLATFORM_LO.X+7) (NEW_MAP_PLATFORM_LO.Y+2) (NEW_MAP_PLATFORM_LO.Z+5) 4 "Right-click this" "sign to go to" "official site" "" downloadCmd1 downloadCmd2 true "black"
            yield! makeSignDo "wall_sign" (NEW_MAP_PLATFORM_LO.X+7) (NEW_MAP_PLATFORM_LO.Y+2) (NEW_MAP_PLATFORM_LO.Z+6) 4 "Or right-click" "me to begin" "playing!" "" (sprintf "tp @p %s 90 180" NEW_PLAYER_LOCATION.STR) "" true "black"
            yield! makeSign "standing_sign" (NEW_MAP_PLATFORM_LO.X+7) (NEW_MAP_PLATFORM_LO.Y+1) (NEW_MAP_PLATFORM_LO.Z+7) 4 "(In this map," "_wall_ signs" "can be" "right-clicked)"
            yield! makeSignBoldness "standing_sign" (NEW_MAP_PLATFORM_LO.X+7) (NEW_MAP_PLATFORM_LO.Y+1) (NEW_MAP_PLATFORM_LO.Z+8) 4 "server" "true" "properties" "true" "enable-command-" "false" "block = true" "false"
            // new players go here:
            yield U (sprintf "fill %s %s sea_lantern" NEW_PLAYER_PLATFORM_LO.STR (NEW_PLAYER_PLATFORM_LO.Offset(10,0,10).STR))
            let GTL = NEW_PLAYER_PLATFORM_LO.Offset(7,2,4)
            yield U (sprintf "setblock %s stone" (GTL.Offset(1,0,0).STR))
            yield! makeSignDo "wall_sign" GTL.X GTL.Y GTL.Z 4 "Right-click" "me to go to" "LOBBY" "" (sprintf "tp @p %s 0 0" LOBBY_CENTER_LOCATION.STR) "" true "black"
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

#if DEBUG
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
#endif
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
        U """tellraw @a[63,7,57,rm=30,score_hasMaps=0] ["(If you need to quit before getting BINGO, you can"]"""
        U """tellraw @a[63,7,57,rm=30,score_hasMaps=0] [{"underlined":"true","text":"press 't' (chat), then click this line to return to the lobby)","clickEvent":{"action":"run_command","value":"/trigger home set 1"}}]"""
        U """scoreboard players set @a[score_hasMaps=0] hasMaps 5"""   // just in case give it to them but inventory full, keep the delay before giving again
        |]
    let cmdsTriggerHome =
        [|
        P "scoreboard players test Time S 0 0"
        C (sprintf "tp @a[score_home_min=1] %s 180 0" OFFERING_SPOT.STR)
        C (sprintf """tellraw @a[score_home_min=1] ["Teleporting back to %s"]""" OFFERING_SPOT.STR)
        C "effect @a[score_home_min=1] saturation 10 4 true"  // feed (and probably will heal some too, but don't want to chain many commands here on clock)
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
        C "testfor @a[tag=!playerHasBeenSeen]"
        C "blockdata ~ ~ ~2 {auto:1b}"
        C "blockdata ~ ~ ~1 {auto:0b}"
        O ""
        U "gamemode 0 @a[tag=!playerHasBeenSeen]"
        U (sprintf "tp @a[tag=!playerHasBeenSeen] %s 0 0" LOBBY_CENTER_LOCATION.STR)
        U "spawnpoint @a[tag=!playerHasBeenSeen]"
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
        // run customized on-respawn loadout command blocks (that were cloned at game start to just below lobby)
        yield U (sprintf "clone %d %d %d %d %d %d ~ ~ ~2" (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY-1) (LOBBYZ+2) (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY-1) (LOBBYZ+2+NUM_CONFIG_COMMANDS-1)) // todo ensure in sync with lobby
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
        yield U "gamerule sendCommandFeedback false"
        yield U "gamerule doDaylightCycle true"
        yield U "gamerule keepInventory false"
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
    let finalizePriorGameLogic =
        [|
            yield O ""
            // bring everyone back to lobby in survival, clear inventories... 
            yield U (sprintf "tp @a %s 0 0" LOBBY_CENTER_LOCATION.STR)
            yield U "gamemode 0 @a"
            yield U "spawnpoint @a"
            yield U "clear @a"
            // feed & heal, as people get concerned in lobby about this
            yield U "effect @a saturation 10 4 true"
            yield U "effect @a regeneration 10 4 true"
            // remind game mode, if game just ended
            yield U """tellraw @a ["Reminder: current game is configured as"]"""
            yield U "scoreboard players set LoadoutTestForBlocksFound S 0"
            for (c,desc) in ALL_LOADOUTS do
                yield U "scoreboard players test LoadoutTestForBlocksFound S 0 0"
                yield C (sprintf "testforblocks %s %s %d %d %d masked" c.STR (c.Offset(0,2,NUM_CONFIG_COMMANDS-1).STR) (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+1) (LOBBYZ+2))
                yield C (sprintf """tellraw @a [{"text":"%s","color":"green"}]""" desc)
                yield C "scoreboard players set LoadoutTestForBlocksFound S 1"
            yield U "scoreboard players test LoadoutTestForBlocksFound S 0 0"
            yield C (sprintf """tellraw @a ["Some custom configuration (command blocks programmed manually)"]""")
        |]
    region.PlaceCommandBlocksStartingAt(FINALIZE_PRIOR_GAME_LOGIC,finalizePriorGameLogic,"end prior game")

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
        // if game just ended, do some housekeeping/usability
        yield U "scoreboard players test GameInProgress S 1 *"
        yield C (sprintf "blockdata %s {auto:1b}" FINALIZE_PRIOR_GAME_LOGIC.STR)
        yield C (sprintf "blockdata %s {auto:0b}" FINALIZE_PRIOR_GAME_LOGIC.STR)
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
        yield! nTicksLater(3)  // let reset score print some text before we print ours
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
        yield! nTicksLater(3)  // let reset score print some text before we print ours
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
        yield U "scoreboard players operation Seed Calc = Seed Score"  // save seed
        yield U "scoreboard players reset * Score"
        yield U "scoreboard players set @a Score 0"
        yield U "scoreboard players operation Seed Score = Seed Calc"  // restore seed
        // set up lockout goal if lockout mode selected (teamCount 2/3/4 -> goal 13/9/7)
        yield U (sprintf "blockdata %s {auto:1b}" (COMPUTE_LOCKOUT_GOAL.STR))
        yield U (sprintf "blockdata %s {auto:0b}" (COMPUTE_LOCKOUT_GOAL.STR))
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
        yield U """tellraw @a ["(If you need to quit before getting BINGO, you can"]"""
        yield U """tellraw @a [{"underlined":"true","text":"press 't' (chat), then click this line to return to the lobby)","clickEvent":{"action":"run_command","value":"/trigger home set 1"}}]"""
        // turn on dropped-map checker
        yield U (sprintf "setblock %s redstone_block" (NOTICE_DROPPED_MAP_CMDS.Offset(0,1,0).STR))
        // prep for customized on-respawn command blocks
        yield U "scoreboard players set @a Deaths 0"
        // clone them to a temp location two below
        yield U (sprintf "clone %d %d %d %d %d %d %d %d %d" (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+1) (LOBBYZ+2) (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+1) (LOBBYZ+2+NUM_CONFIG_COMMANDS-1) (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY-1) (LOBBYZ+2))
        // run customized on-start loadout command blocks
        yield U (sprintf "clone %d %d %d %d %d %d ~ ~ ~1" (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+3) (LOBBYZ+2) (LOBBYX+CFG_ROOM_IWIDTH+1) (LOBBYY+3) (LOBBYZ+2+NUM_CONFIG_COMMANDS-1))
        for _i = 1 to NUM_CONFIG_COMMANDS do
            yield U "say SHOULD BE REPLACED"
        // NOTE, customized on-start commands must be last, to firewall them
        |]
    region.PlaceCommandBlocksStartingAt(START_GAME_PART_1,startGameButtonPart1,"start game1")
    region.PlaceCommandBlocksStartingAt(START_GAME_PART_2,startGameButtonPart2,"start game2")

    let showItemsButton =
        [|
            let x,y,z = (LOBBYX+CFG_ROOM_IWIDTH+MAIN_ROOM_IWIDTH+6), (LOBBYY+2), (LOBBYZ+8)
            yield O ""
            yield U (sprintf "setblock %d %d %d chest" (x+1) (y-1) z)
            yield U (sprintf "blockdata %d %d %d %s" (x+1) (y-1) z anyDifficultyChest)
            yield U (sprintf "setblock %d %d %d chest" (x+3) (y-1) z)
            yield U (sprintf "blockdata %d %d %d %s" (x+3) (y-1) z otherChest1)
            yield U (sprintf "setblock %d %d %d chest" (x+1) (y-1) (z+2))
            yield U (sprintf "blockdata %d %d %d %s" (x+1) (y-1) (z+2) otherChest2)
            yield U (sprintf "setblock %d %d %d chest" (x+3) (y-1) (z+2))
            yield U (sprintf "blockdata %d %d %d %s" (x+3) (y-1) (z+2) otherChest3)
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
            C (sprintf "blockdata %s {auto:1b}" (COMPUTE_LOCKOUT_GOAL.STR))
            C (sprintf "blockdata %s {auto:0b}" (COMPUTE_LOCKOUT_GOAL.STR))
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
            yield U (sprintf "setblock %s stone" (TUTORIAL_LOCATION.Offset(1,0,18).STR))
            yield U (sprintf "setblock %s stone" (TUTORIAL_LOCATION.Offset(0,-1,18).STR))
            yield U (sprintf "setblock %s water" (TUTORIAL_LOCATION.Offset(0,0,18).STR))
            yield U (sprintf "setblock %s dirt"  (TUTORIAL_LOCATION.Offset(-1,0,18).STR))
            yield U (sprintf "setblock %s reeds" (TUTORIAL_LOCATION.Offset(-1,1,18).STR))
            yield U (sprintf "setblock %s reeds" (TUTORIAL_LOCATION.Offset(-1,2,18).STR))
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
        yield U """execute @a ~ ~ ~ summon FireworksRocketEntity ~3 ~0 ~0 {LifeTime:20,FireworksItem:{id:"minecraft:fireworks",Count:1,tag:{Fireworks:{Explosions:[{Type:0,Flicker:0,Trail:0,Colors:[16730395,1796095,5177112],FadeColors:[16777215]},]}}}}"""
        yield! nTicksLater(8)
        yield U """execute @a ~ ~ ~ summon FireworksRocketEntity ~0 ~0 ~3 {LifeTime:20,FireworksItem:{id:"minecraft:fireworks",Count:1,tag:{Fireworks:{Explosions:[{Type:1,Flicker:0,Trail:1,Colors:[13172728],FadeColors:[16777215]},]}}}}"""
        yield! nTicksLater(8)
        yield U """execute @a ~ ~ ~ summon FireworksRocketEntity ~-3 ~0 ~0 {LifeTime:20,FireworksItem:{id:"minecraft:fireworks",Count:1,tag:{Fireworks:{Explosions:[{Type:2,Flicker:1,Trail:0,Colors:[16777074],FadeColors:[16777215]},]}}}}"""
        yield! nTicksLater(8)
        yield U """execute @a ~ ~ ~ summon FireworksRocketEntity ~0 ~0 ~-3 {LifeTime:20,FireworksItem:{id:"minecraft:fireworks",Count:1,tag:{Fireworks:{Explosions:[{Type:3,Flicker:1,Trail:1,Colors:[6160227],FadeColors:[16777215]}]}}}}"""
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
    let computeLockoutGoal =
        [|
        yield O ""
        yield U "scoreboard players set teamCount S 0"
        for t in TEAMS do
            yield U (sprintf "execute @p[team=%s] ~ ~ ~ scoreboard players add teamCount S 1" t)
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
        |]
    region.PlaceCommandBlocksStartingAt(COMPUTE_LOCKOUT_GOAL,computeLockoutGoal, "computeLockoutGoal")


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



////////////////////////////////////////////

////////////////////////////////////////////

let preciseImageToBlocks(imageFilename:string,regionFolder, baseY) =
    let image = new System.Drawing.Bitmap(imageFilename)
    let m = new MapFolder(regionFolder)
    let colorTable= new System.Collections.Generic.Dictionary<_,_>()
    let knownColors = 
        [|
            (255uy, 51uy, 102uy, 153uy),   (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 95uy, 11uy))   // blue glass water
            (255uy, 255uy, 255uy, 255uy),  (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 80uy, 0uy))    // white snow
            (255uy, 0uy, 102uy, 0uy),      (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 35uy, 13uy))   // green wool tree
            (255uy, 102uy, 102uy, 102uy),  (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 7uy, 0uy))     // dark mountain
            (255uy, 0uy, 204uy, 0uy),      (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 2uy, 0uy))     // green grass
            (255uy, 255uy, 51uy, 0uy),     (fun x y z -> for dy in [0;1;2;3] do m.EnsureSetBlockIDAndDamage(x, baseY+dy, z, 152uy, 0uy))   // red wall
            (255uy, 153uy, 153uy, 153uy),  (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 1uy, 0uy))     // grey stone
            (255uy, 255uy, 255uy, 0uy),    (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 41uy, 0uy))    // gold thingy
            (255uy, 204uy, 255uy, 255uy),  (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 174uy, 0uy))   // light blue ice
            (255uy, 153uy, 102uy, 51uy),   (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 3uy, 2uy))     // brown podzol
            (255uy, 0uy, 0uy, 0uy),        (fun x y z -> ())                                               // black means air
            (255uy, 255uy, 102uy, 0uy),    (fun x y z -> m.EnsureSetBlockIDAndDamage(x, baseY, z, 86uy, 11uy))   // orange pumpkin
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

open System.IO.Compression

let compareMinecraftAssets(jar1, jar2) =
    use archive1 = ZipFile.OpenRead(jar1)
    use archive2 = ZipFile.OpenRead(jar2)
    let a1 = ResizeArray()
    let a2 = ResizeArray()
    for e in archive1.Entries do
        if e.FullName.StartsWith("assets/minecraft/loot_tables") ||
                e.FullName.StartsWith("assets/minecraft/structures") ||
                e.FullName.StartsWith("assets/minecraft/texts") then
            a1.Add(e.FullName)
    for e in archive2.Entries do
        if e.FullName.StartsWith("assets/minecraft/loot_tables") ||
                e.FullName.StartsWith("assets/minecraft/structures") ||
                e.FullName.StartsWith("assets/minecraft/texts") then
            a2.Add(e.FullName)
    a1.Sort()
    a2.Sort()
    let mutable diffCount = 0
    printfn "FILE LIST DIFF"
    if diffStringArrays(a1.ToArray(), a2.ToArray()) then
        diffCount <- diffCount + 1
    printfn "=============="
    for name in a1 do
        let entry1 = archive1.GetEntry(name)
        let entry2 = archive2.GetEntry(name)
        if entry1 <> null && entry2 <> null then
            if System.IO.Path.GetExtension(name) = "nbt" then
                printfn "%s" (name.ToUpper())
                if diffDatFilesText(entry1.Open(), entry2.Open()) then
                    diffCount <- diffCount + 1
                printfn "=============="
            else
                printfn "%s" (name.ToUpper())
                let a1 = ResizeArray()
                let s1 = new System.IO.StreamReader(entry1.Open())
                while not s1.EndOfStream do
                    a1.Add(s1.ReadLine())
                let a2 = ResizeArray()
                let s2 = new System.IO.StreamReader(entry2.Open())
                while not s2.EndOfStream do
                    a2.Add(s2.ReadLine())
                if diffStringArrays(a1.ToArray(), a2.ToArray()) then
                    diffCount <- diffCount + 1
                printfn "=============="
    printfn ""
    printfn "Total diffs found: %d" diffCount
////////////////////////////////////////////

open System.Diagnostics 

let genTerrainWithMCServer(seed, customizedPreset) =
    let serverFolder = """C:\Users\Admin1\Desktop\MC SERVER\"""
    let jar = """minecraft_server.15w49a.jar"""
    let psi = new ProcessStartInfo(UseShellExecute=false, RedirectStandardInput=true, RedirectStandardOutput=true) 
    psi.WorkingDirectory <- serverFolder
    psi.FileName <- "java" 
    psi.Arguments <- sprintf "-Xms1024M -Xmx1024M -d64 -jar %s nogui" jar
    // TODO
    // server prop before level.dat (delete whole world folder)
    // setworldspawn 0 80 0, stop server, restart
    if false then
        System.IO.File.WriteAllLines(serverFolder+"server.properties",
            [|
                sprintf "generator-settings=%s" customizedPreset
                "level-type=CUSTOMIZED"
                sprintf "level-seed=%d" seed
                "enable-command-block=true"
                "gamemode=1"
                "force-gamemode=true"
            |])
    //System.Threading.Thread.Sleep(8000)
    let proc = new Process(StartInfo=psi) 
    proc.Start() |> ignore 
    let rec rcvloop() = 
        let data = proc.StandardOutput.ReadLine() 
        if data <> null then 
            printfn "MC: %s" data
            rcvloop() 
    let t = new System.Threading.Thread(rcvloop) 
    t.Start() 
    System.Threading.Thread.Sleep(5000)
    //proc.StandardInput.WriteLine("/stop")
    //System.Threading.Thread.Sleep(500000)
    let userInput = false
    if userInput then
        let mutable s = stdin.ReadLine()
        while s <> "" do
            //printfn "USER: %s" s
            proc.StandardInput.WriteLine(s)
            s <- stdin.ReadLine()
    else
        let sw = Stopwatch.StartNew()
        proc.StandardInput.WriteLine("""/summon LavaSlime 0 255 0 {Invulnerable:1,Tags:["AA"]}""")
        //for cx = -32 to 32 do
        for cx = -5 to 5 do
            for cz = -32 to 32 do
                let x = cx*16+8
                let z = cz*16+8
                proc.StandardInput.WriteLine(sprintf """/spreadplayers %d %d 2 7 false @e[tag=AA]""" x z)
                System.Threading.Thread.Sleep(170)  // TODO tune this, but overall, seems to take a lot longer than my player version
        printfn "finished in %f minutes" sw.Elapsed.TotalMinutes 
    proc.StandardInput.WriteLine("""/kill @e[tag=AA]""")
    System.Threading.Thread.Sleep(200)
    proc.StandardInput.WriteLine("""/stop""")
    System.Threading.Thread.Sleep(2000)
    proc.Close()
    printfn "press enter to quit"
    stdin.ReadLine() |> ignore

////////////////////////////////////////////

[<AllowNullLiteral>]
type TrieNode(parent,finalLetter) =
    let data = Array.zeroCreate 26
    let mutable isWord = false
    let mutable x,y,z = 0,0,0
    member this.Add(letter) =
        if data.[letter] = null then
            data.[letter] <- new TrieNode(this,letter)
        data.[letter]
    member this.FinishWord() = isWord <- true
    member this.Data = data
    member this.IsWord = isWord
    member this.SetXYZ(xx,yy,zz) = x <- xx; y <- yy; z <- zz
    member this.X = x
    member this.Y = y
    member this.Z = z
    member this.Parent = parent
    member this.FinalLetter = finalLetter

let makeTrie() =
    let words = System.IO.File.ReadAllLines("""C:\Users\Admin1\Documents\GitHubVisualStudio\minecraft-map-manipulator\MinecraftMapManipulator\ConsoleApplication1\ENABLE.txt""")
    let root = new TrieNode(null,-1)
    let mutable count, letters = 0,0
    for w in words do
        let mutable i = root
        for c in w do
            i <- i.Add(int(c) - int('a'))
        i.FinishWord()
        count <- count + 1
        letters <- letters + w.Length 
    printfn "made %d words, %d letters" count letters
    root

let mutable words = 0
let mutable nonWords = 0
let mutable cmdBlocks = 0
let mutable firstTime = true
let mutable nodesVisited = 0

type Placer() =
    let mutable x,y,z = 0,255,0
    member this.Place(n) =
        if z+n > 170 then
            z <- 0
            x <- x + 1
            if x > 170 then
                x <- 0
                y <- y - 3
                if y < 10 then
                    failwith "out of room"
        let r = x,y,z
        z <- z + n
        r

let rec postfix(n:TrieNode,parent:TrieNode,placer:Placer,r:RegionFile) =
    nodesVisited <- nodesVisited + 1
    if nodesVisited % 10000 = 0 then
        printfn "    visited %d" nodesVisited
    let mutable numFwd = 0
    for x in n.Data do
        if x <> null then
            postfix(x,n,placer,r)
            numFwd <- numFwd + 1
    if n.IsWord then
        words <- words + 1
    else
        nonWords <- nonWords + 1
    let thisCmdBlocks = 1 + numFwd + 1
    cmdBlocks <- cmdBlocks + thisCmdBlocks
    let x,y,z =     
        if firstTime then
            let x,y,z = placer.Place(thisCmdBlocks+1)  // +1 for air in between, PlaceCommandBlocksStartingAt puts air after
            n.SetXYZ(x,y,z)
            x,y,z
        else
            n.X, n.Y, n.Z
    if firstTime then
        // could place commands, record self location
        // second pass to record parent locations for backspace
        // O fill blah wool n replace wool  // n = green or red if word
        // U tp @e[type=LavaSlime,score_L_min=0,score_L=0] x y z // xyz for 'A' (0)
        // U tp @e[type=LavaSlime,score_L_min=1,score_L=1] x y z // xyz for 'B' (1)
        // ...
        // U tp @e[type=LavaSlime,score_L_min=26,score_L=26] x y z // xyz for 'backspace' (26)
        let cmds = 
            [|
                yield O (sprintf "fill 49 0 0 49 3 10 wool %d replace wool" (if n.IsWord then 5 else 14))
                for i = 0 to 25 do
                    let next = n.Data.[i]
                    if next <> null then
                        assert(next.Y <> 0)
                        yield U (sprintf "tp @e[type=LavaSlime,score_L_min=%d,score_L=%d] %d %d %d" i i next.X next.Y next.Z)
                yield U "say never get here"  // replace in pass 2
            |]
        ()//r.PlaceCommandBlocksStartingAt(x,y,z,cmds,"",false)
    else
        ()//r.PlaceCommandBlocksStartingAt(x,y,z+thisCmdBlocks-1,[|U (sprintf "tp @e[type=LavaSlime,score_L_min=26,score_L=26] %d %d %d" parent.X parent.Y parent.Z)|],"",false)

let rec findTrie(n:TrieNode) =
    let mutable numFwd = 0
    for x in n.Data do
        if x <> null then
            numFwd <- numFwd + 1
            findTrie(x)
    if numFwd < 5 && n.Data.[0] <> null && n.Data.[1] <> null then
        let mutable c,a = n,[]
        while c <> null do
            a <- c.FinalLetter :: a
            c <- c.Parent 
        let s = new string(a |> List.map (fun i -> i+65 |> char) |> List.toArray)
        printfn "%s" s
(*
@ACER
@AFE
@AGRI
@BELLY
@CELI
@CHUB
@CLIM
@CRUCI
@DECUM
@DIB
@EXOR
@FRAM
@HEREIN
@IMPARTI
@INTERTRI
@KEB
@KLE
@OUTRE
@POSTOR
@RAB
@REIM
@RETRI
@RUR
@SCRIB
@SHIB
@UNIM
@VESTI
@WOM
*)

let doTrie(r:RegionFile) =
    let t = makeTrie()
    findTrie(t)
    let placer = new Placer()
    postfix(t,null,placer,r)
    printfn "%d word nodes and %d nonword nodes, need %d commands" words nonWords cmdBlocks
    firstTime <- false
    nodesVisited <- 0
    postfix(t,t,placer,r)
    // glowing entity can be the 'PC', can use all Os and have it BD them
    for x = 0 to 26 do
        let cmds = 
            [|
                yield O "blockdata ~ ~ ~ {auto:0b}"
                //yield U (sprintf """tellraw @a [{"text":"you pressed %c"}]""" (char (65+x)))
                yield U (sprintf "scoreboard players set @e[type=LavaSlime] L %d" x)
                yield U "execute @e[type=LavaSlime] ~ ~ ~ blockdata ~ ~ ~ {auto:1b}"  // run cur to jump to next
                yield U "blockdata ~ ~ ~2 {auto:1b}"
                yield U "blockdata ~ ~ ~1 {auto:0b}"
                yield O "scoreboard players set @e[type=LavaSlime] L -1"
                yield U "execute @e[type=LavaSlime] ~ ~ ~ blockdata ~ ~ ~ {auto:1b}"  // run next's fill, also see if looped
                if x = 26 then
                    yield C "tp @e[type=ArmorStand] ~ ~ ~-1"
                    yield C "execute @e[type=ArmorStand] ~ ~ ~ setblock 48 2 ~ air"
                else
                    yield C (sprintf "execute @e[type=ArmorStand] ~ ~ ~ clone 68 2 %d 68 2 %d 48 2 ~" x x)
                    yield C "tp @e[type=ArmorStand] ~ ~ ~1"
                yield U "execute @e[type=Slime] ~ ~ ~ blockdata ~ ~ ~ {auto:0b}"  // slime stayed at old place
                yield U "tp @e[type=Slime] @e[type=LavaSlime]"
                yield U "execute @e[type=Slime] ~ ~ ~ blockdata ~ ~ ~ {auto:0b}"  // slime at new place
            |]
        r.PlaceCommandBlocksStartingAt(x,0,0,cmds,"on key",false)
    let keys = 
        [|
            for i = 0 to 25 do
                yield sprintf """{"text":"[%c] ","clickEvent":{"action":"run_command","value":"/blockdata %d 0 0 {auto:1b}"}}""" (char (65+i)) i
            yield """{"text":"[BKSP]","clickEvent":{"action":"run_command","value":"/blockdata 26 0 0 {auto:1b}"}}"""
        |]
    let initCmds =
        [|
            O ""
            U "scoreboard objectives add L dummy"
            U "kill @e[type=!Player]"
            U "effect @p 16 9999 1 true"
            U "gamerule commandBlockOutput false"
            U "gamerule sendCommandFeedback false"
            U "clear @a"
            (*
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ts,Color:0},{Pattern:mr,Color:15},{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:ms,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:0,Patterns:[{Pattern:mr,Color:15},{Pattern:ls,Color:0},{Pattern:ms,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:0,Patterns:[{Pattern:mr,Color:15},{Pattern:ms,Color:15},{Pattern:ls,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:rs,Color:0},{Pattern:ts,Color:0},{Pattern:bs,Color:0},{Pattern:cbo,Color:15},{Pattern:ls,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ms,Color:0},{Pattern:hhb,Color:15},{Pattern:rs,Color:15},{Pattern:ls,Color:0},{Pattern:ts,Color:0},{Pattern:bs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ms,Color:0},{Pattern:hhb,Color:15},{Pattern:rs,Color:15},{Pattern:ls,Color:0},{Pattern:ts,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ms,Color:0},{Pattern:vh,Color:15},{Pattern:rs,Color:0},{Pattern:hh,Color:15},{Pattern:bs,Color:0},{Pattern:ls,Color:0},{Pattern:ts,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:ms,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ts,Color:0},{Pattern:bs,Color:0},{Pattern:cs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:bs,Color:0},{Pattern:mr,Color:15},{Pattern:rs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:drs,Color:0},{Pattern:vh,Color:15},{Pattern:hh,Color:15},{Pattern:dls,Color:0},{Pattern:ls,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:vh,Color:0},{Pattern:cs,Color:15},{Pattern:bs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:tt,Color:0},{Pattern:tts,Color:15},{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ls,Color:0},{Pattern:rud,Color:15},{Pattern:drs,Color:0},{Pattern:rs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ts,Color:0},{Pattern:bs,Color:0},{Pattern:mr,Color:15},{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:rs,Color:0},{Pattern:hhb,Color:0},{Pattern:bs,Color:15},{Pattern:ts,Color:0},{Pattern:ls,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ts,Color:0},{Pattern:bs,Color:0},{Pattern:mr,Color:15},{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:br,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag: {Base:15,Patterns:[{Pattern:br,Color:0},{Pattern:rud,Color:15},{Pattern:ms,Color:0},{Pattern:hh,Color:0},{Pattern:cs,Color:15},{Pattern:drs,Color:0},{Pattern:tt,Color:15},{Pattern:ls,Color:0},{Pattern:ms,Color:0},{Pattern:ts,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ts,Color:0},{Pattern:bs,Color:0},{Pattern:mr,Color:15},{Pattern:drs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ts,Color:0},{Pattern:cs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:bs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:bs,Color:0},{Pattern:mr,Color:15},{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:tt,Color:15},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:bt,Color:0},{Pattern:bts,Color:15},{Pattern:ls,Color:0},{Pattern:rs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:dls,Color:0},{Pattern:drs,Color:0},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:drs,Color:0},{Pattern:vhr,Color:15},{Pattern:dls,Color:0},{Pattern:cbo,Color:15},{Pattern:bo,Color:15}]}}"
            U "/give @p minecraft:banner 1 0 {BlockEntityTag:{Base:15,Patterns:[{Pattern:ts,Color:0},{Pattern:bs,Color:0},{Pattern:dls,Color:0},{Pattern:bo,Color:15}]}}"
            *)
            U (sprintf "summon LavaSlime %d %d %d {Glowing:1,Invulnerable:1,Silent:1,NoAI:1,Size:1,Invisible:1}" t.X t.Y t.Z)
            U (sprintf "summon Slime %d %d %d {Glowing:1,Invulnerable:1,Silent:1,NoAI:1,Size:1,Invisible:1}" t.X t.Y t.Z)
            U "summon ArmorStand 50 2 0 {Invisible:1,Marker:1,NoGravity:1}"
            U "fill 49 0 0 49 3 10 wool 0"
            U "fill 69 0 0 69 3 30 wool 0"
            U (sprintf "tellraw @a [%s]" (System.String.Join(",",keys)))
        |]
    r.PlaceCommandBlocksStartingAt(30,0,0,initCmds,"init",false)
////////////////////////////////////////////

let musicStuff() =
    let MUSIC_DIR = """C:\Users\Admin1\Desktop\Music\"""
    let FFMPEG_DIR = """C:\Users\Admin1\Desktop\ffmpeg-20160105-git-68eb208-win64-static\bin\"""
    let OUT_DIR = """C:\Users\Admin1\AppData\Roaming\.minecraft\resourcepacks\BrianResourcePack\assets\minecraft\sounds\"""
    let DEFAULT_FILTER = """-filter_complex afade=t=in:st=0:d=0.02:c=tri,afade=t=out:st=4.98:d=0.02:c=tri"""
    let FADEOUT_FILTER = """-filter_complex afade=t=in:st=0:d=0.02:c=tri,afade=t=out:st=0:d=5:c=tri"""
    let FADEIN_FILTER  = """-filter_complex afade=t=in:st=0:d=5:c=tri,afade=t=out:st=4.98:d=0.02:c=tri"""
    let SECONDS_PER_TRACK = 5
    let SEGMENTS = 60
    let MAX_TICK = SEGMENTS * SECONDS_PER_TRACK * 20 // 20 = ticks per second
    let TRACKS = [|"cat"; "else"; "far"|]
    let breakUpOggFiles = false
    if breakUpOggFiles then
        for track in TRACKS do
            for i = 0 to SEGMENTS-1 do
                let startTime = i * SECONDS_PER_TRACK
                let run(args) = 
                    let psi = ProcessStartInfo(UseShellExecute=true, RedirectStandardInput=false, RedirectStandardOutput=false)
                    psi.WorkingDirectory <- FFMPEG_DIR
                    psi.FileName <- "ffmpeg.exe"
                    psi.Arguments <- args
                    let proc = new Process(StartInfo=psi)
                    proc.Start() |> ignore
                let file = sprintf "%s%02d" track i
                let args = sprintf "-y -ss %d -i %s%s.ogg -t %d %s %s%s.ogg" startTime MUSIC_DIR track SECONDS_PER_TRACK DEFAULT_FILTER OUT_DIR file
                run(args)
                printfn """  "brian.%s": { "category": "record", "sounds": [ {"name":"%s","stream":true} ] },""" file file
                let file = sprintf "%s%02dfadeout" track i
                let args = sprintf "-y -ss %d -i %s%s.ogg -t %d %s %s%s.ogg" startTime MUSIC_DIR track SECONDS_PER_TRACK FADEOUT_FILTER OUT_DIR file
                run(args)
                printfn """  "brian.%s": { "category": "record", "sounds": [ {"name":"%s","stream":true} ] },""" file file
                let file = sprintf "%s%02dfadein" track i
                let args = sprintf "-y -ss %d -i %s%s.ogg -t %d %s %s%s.ogg" startTime MUSIC_DIR track SECONDS_PER_TRACK FADEIN_FILTER OUT_DIR file
                run(args)
                printfn """  "brian.%s": { "category": "record", "sounds": [ {"name":"%s","stream":true} ] },""" file file
        printfn ""
        printfn "Now delete all the 'extra' files from the output directory, since ffmpeg can leave bad bits when starting past EOF (anything less than 10KB file size)"
    let fil = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\MusicTestRR\region\r.0.0.mca"""
    let r = new RegionFile(fil)
    let cmds = 
        [|
            O "scoreboard objectives add Prev dummy"
            U "scoreboard objectives add Curr dummy"
            U "scoreboard objectives add Tick dummy"
            U "gamerule commandBlockOutput false"
            U "gamerule doDaylightCycle false"
            U "time set 500"
            U "scoreboard players set Tick Tick -1"
            U "scoreboard players set N Tick 100"
            U "scoreboard players set @a Prev 0"
            U "fill 10 55 10 19 55 19 stone"
            U "fill 10 55 20 19 55 29 lapis_block"
        |]
    r.PlaceCommandBlocksStartingAtSelfDestruct(1,60,1,cmds,"")
    let cmds = 
        [|
            yield O "fill 20 60 1 100 60 100 air"
            yield U "fill ~ ~ ~-1 ~ ~ ~100 air"
            yield P "scoreboard players add Tick Tick 1"
            yield U "scoreboard players operation Test Tick = Tick Tick"
            yield U "scoreboard players operation Test Tick %= N Tick"
            yield U (sprintf "scoreboard players test Tick Tick %d" (MAX_TICK+5))  // supposed to be silent at end of loop anyway, just ensure we get all segments even after the delay to run code below
            yield C "scoreboard players set Tick Tick -1"
            yield U "scoreboard players test Test Tick * 0"
            yield C "blockdata ~ ~ ~2 {auto:1b}"
            yield C "blockdata ~ ~ ~1 {auto:0b}"
            yield O "scoreboard players operation @a Tick = Tick Tick"
            yield U "scoreboard players operation @a Tick /= N Tick"  // @p Tick is which segment
            yield U """tellraw @a ["segment #",{"score":{"name":"@p","objective":"Tick"}}]"""
            yield U "execute @a ~ ~ ~ scoreboard players operation @p[r=0,c=1] Prev = @p[r=0,c=1] Curr"
            yield U "scoreboard players set @a Curr 0"  // default
            yield U "scoreboard players set @a[x=10,y=1,z=10,dx=10,dy=200,dz=10] Curr 1"  // region score
            yield U "scoreboard players set @a[x=10,y=1,z=20,dx=10,dy=200,dz=10] Curr 2"  // region score
            for k = 0 to SEGMENTS-1 do
                yield U (sprintf "execute @a[score_Tick=%d,score_Tick_min=%d] ~ ~ ~ blockdata %d 60 1 {auto:1b}" k k (20+k))
                //yield U (sprintf "execute @a[score_Tick=%d,score_Tick_min=%d] ~ ~ ~ blockdata %d 60 1 {auto:0b}" k k (20+k))
        |]
    r.PlaceCommandBlocksStartingAt(2,60,1,cmds,"")
    for k = 0 to SEGMENTS-1 do
        let cmds = 
            [|
                yield O "blockdata ~ ~ ~ {auto:0b}"
                for i = 0 to TRACKS.Length-1 do
                    for j = 0 to TRACKS.Length-1 do
                        if i=j then
                            yield U (sprintf "execute @a[score_Prev=%d,score_Prev_min=%d,score_Curr=%d,score_Curr_min=%d] ~ ~ ~ playsound %s @p[r=0,c=1] ~ 255 ~ 64 1 0" i i j j (sprintf "brian.%s%02d" TRACKS.[i] k))
                        else
                            yield U (sprintf "execute @a[score_Prev=%d,score_Prev_min=%d,score_Curr=%d,score_Curr_min=%d] ~ ~ ~ playsound %s @p[r=0,c=1] ~ 255 ~ 64 1 0" i i j j (sprintf "brian.%s%02dfadeout" TRACKS.[i] k))
                            yield U (sprintf "execute @a[score_Prev=%d,score_Prev_min=%d,score_Curr=%d,score_Curr_min=%d] ~ ~ ~ playsound %s @p[r=0,c=1] ~ 255 ~ 64 1 0" i i j j (sprintf "brian.%s%02dfadein" TRACKS.[j] k))
            |]
        r.PlaceCommandBlocksStartingAt(20+k,60,1,cmds,"")
    // TODO have each track have its own repeat length, have Tick mode segment # based on biome track?
    // TODO use the 'play 46 null sounds at once' strategy to end all sound, and then have continual tracks that fade in at each 5s start point, and only stop at transitions
    // TODO can make computation more efficient using a armor stand to track segment number with a tp each 5s segment

    r.Write(fil+".new")
    System.IO.File.Delete(fil)
    System.IO.File.Move(fil+".new",fil)


///////////////////////////////////////////////////////
type ImageWPFWindow(img) as this =  
    inherit System.Windows.Window()    
    do 
        this.SizeToContent <- System.Windows.SizeToContent.WidthAndHeight 
        this.Content <- img

let plotRegionalDifficulty() =
    let difficulty = 2 // normal
    let moonPhase = 1.0 // full
    let N = 100
    let image = new System.Drawing.Bitmap(N+1,N+1)
    for totalPlayTimeN = 0 to N do
        let totalPlayTimePct = float totalPlayTimeN / float N
        let totalPlayTimeHours = 21.0 * totalPlayTimePct 
        for chunkInhabitedTimeN = 0 to N do
            let chunkInhabitedTimePct = float chunkInhabitedTimeN / float N
            let chunhInhabitedTimeHours = 50.0 * chunkInhabitedTimePct 

            // wiki formula
            let TotalTimeFactor = (min 20.0 (totalPlayTimeHours - 1.0)) / 80.0        // range: 0.0 - 0.25
            let mutable ChunkFactor = (min 1.0 (chunhInhabitedTimeHours / 50.0))      // init range: 0.0 - 1.0
            if difficulty <> 3 then
                ChunkFactor <- ChunkFactor * 0.75
            ChunkFactor <- ChunkFactor + (min (moonPhase/4.0) TotalTimeFactor)
            if difficulty = 1 then
                ChunkFactor <- ChunkFactor / 2.0
            let mutable RegionalDifficulty = 0.75 + TotalTimeFactor + ChunkFactor
            if difficulty = 2 then
                RegionalDifficulty <- RegionalDifficulty * 2.0
            if difficulty = 3 then
                RegionalDifficulty <- RegionalDifficulty * 3.0
            // wiki says The regional difficulty ranges from 0.75–1.5 on easy, 1.5–4.0 on normal, and 2.25–6.75 on hard.

            let gameUsedValue = min 1.0 (max 0.0 ((RegionalDifficulty - 2.0) / 2.0))
            if totalPlayTimeN = 50 then
                printf "%1.2f " gameUsedValue
            let r,g,b = 
                if gameUsedValue = 0.0 then
                    255, 0, 0
                elif gameUsedValue = 1.0 then
                    0, 150, 0
                else
                    int(255.0 * gameUsedValue), int(255.0 * gameUsedValue), int(255.0 * gameUsedValue)
            image.SetPixel(chunkInhabitedTimeN, totalPlayTimeN, System.Drawing.Color.FromArgb(r,g,b))
        if totalPlayTimeN = 50 then
            printfn ""
    let img = PhotoToMinecraft.bmpToImage(image,8.0)
    let app =  new System.Windows.Application()  
    app.Run(new ImageWPFWindow(img)) |> ignore 
    // neat, this demos that if TotalPlayTime is 11 hours, at normal diff, full moon, ChunkInhabitedTime knob changes game value smoothly from 0.0 to 0.75, which is very useful.

////////////////////////////////////////

type InputEvent = 
    | CONSOLE of string     // stuff typed into the keyboard console of this program 
    | MINECRAFT of string   // the stdout of the Minecraft process 

let chatToVoiceDemo() =
    use inputEvents = new System.Collections.Concurrent.BlockingCollection<_>()
    // SETUP MINECRAFT 
    let minecraftStdin = 
        let psi = new ProcessStartInfo(UseShellExecute=false, RedirectStandardInput=true, RedirectStandardOutput=true) 
        psi.WorkingDirectory <- """C:\Users\Admin1\Desktop\Server""" 
        psi.FileName <- "java" 
        psi.Arguments <- "-Xms1024M -Xmx1024M -d64 -jar minecraft_server.16w02a.jar nogui" 
        let proc = new Process(StartInfo=psi) 
        // START MINECRAFT 
        do
            proc.Start() |> ignore 
            let rec rcvloop() = 
                let data = proc.StandardOutput.ReadLine() 
                if data <> null then 
                    inputEvents.Add(MINECRAFT data) 
                    rcvloop() 
            let t = new System.Threading.Thread(rcvloop) 
            t.Start() 
        proc.StandardInput 
    // SETUP & START CONSOLE 
    do 
        printfn "press q <enter> to quit" 
        let rec sendloop() = 
            let i = System.Console.ReadLine() 
            if i = "go" then
                for i = 1 to 9 do
                    minecraftStdin.WriteLine("execute @p ~ ~ ~ tp @e[type=Villager] ~ ~ ~ ~10 ~")
                    minecraftStdin.WriteLine("execute @p ~ ~ ~ tp @e[type=Villager] ~ ~ ~0.11 ~ ~")
                    minecraftStdin.Flush()
                    System.Threading.Thread.Sleep(100)
            if i <> "q" then 
                inputEvents.Add(CONSOLE i) 
                sendloop() 
            else 
                inputEvents.CompleteAdding() 
        let t = new System.Threading.Thread(sendloop) 
        t.Start() 
    let ss = System.Speech.Synthesis.SpeechSynthesizer()
    // MAIN LOOP 
    for e in inputEvents.GetConsumingEnumerable() do 
        match e with 
        | MINECRAFT data -> 
                try 
                    printfn "MINECRAFT> %s" data 
                    match data.IndexOf("Lorgon111") with 
                    | -1 -> () 
                    | n ->  
                    let data = data.Substring(n+"Lorgon111".Length) 
                    let PROMPT = "> !"
                    let PROMPT = "> "
                    match data.LastIndexOf(PROMPT) with      // may be color reset code between name and text, match separately 
                    | -1 -> () 
                    | n ->  
                        let text = data.Substring(n+PROMPT.Length).ToLowerInvariant()
                        let words = text.Split([|" "|], System.StringSplitOptions.RemoveEmptyEntries) 
                        for w in words do
                            printfn "M: %s" w
                        ss.Speak(text)
                with e ->  
                    printfn "MINECRAFT FAULT> %s" (e.ToString()) 
                    reraise() 

        | CONSOLE data -> 
            printfn "C: %s" data

////////////////////////////////////////
let makeGetAllItemsGame() =    
    // TODO encase in barrier
    let map = new MapFolder("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\flattest\region""")
    let tes = ResizeArray()
    let YMIN = 5
    let L = (survivalObtainableItems.Length+4)/5
    let Q = (L+3)/4
    let ox = 1
    for oz = 1 to 4*Q do
        for y = YMIN to YMIN+4 do
            let i = (y-YMIN)*L + oz
            let x,z,dx,dz,facing =
                if oz <= Q then
                    ox,oz+1,1,0,3
                elif oz <= 2*Q then
                    oz-Q+3,Q+2,0,1,2
                elif oz <= 3*Q then
                    Q+6,3*Q+2-oz,-1,0,1
                else
                    oz-3*Q+3,1,0,-1,0
            map.EnsureSetBlockIDAndDamage(x+dx,y,z+dz,1uy,0uy)
            if i < survivalObtainableItems.Length then
                map.EnsureSetBlockIDAndDamage(ox,y,oz,211uy,3uy)
                let bid,dmg,name = survivalObtainableItems.[i]
                let itemName = if bid <= 255 then blockIdToMinecraftName |> Array.find (fun (x,_y) -> x=bid) |> snd else sprintf "minecraft:%s" name
                let cmd = sprintf """summon ItemFrame %d %d %d {Facing:%db,Item:{id:"%s",Count:1b,Damage:%ds}}""" (x+2*dx) y (z+0*dz) facing itemName dmg
                tes.Add [|Int("x",ox); Int("y",y); Int("z",oz); String("id","Control"); 
                            Byte("auto",1uy); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); 
                            String("Command",cmd); End |]
                // TODO filled_map is weird
                // TODOs from the item list, e.g. potions, ench books, etc
    for y = YMIN to YMIN+4 do
        let x = 1
        let z = 0
        map.SetBlockIDAndDamage(x,y,z,137uy,3uy)
        let cmd = ""
        tes.Add [|Int("x",x); Int("y",y); Int("z",z); String("id","Control"); 
                    Byte("auto",0uy); Byte("conditionMet",1uy); String("CustomName","@"); Byte("powered",0uy); Int("SuccessCount",1); Byte("TrackOutput",0uy); 
                    String("Command",cmd); End |]
    map.AddOrReplaceTileEntities(tes)
    // blockIdToMinecraftName
    printfn "%d" survivalObtainableItems.Length 
    map.WriteAll()

////////////////////////////////////////


[<System.STAThread()>]  
do   
    //makeGetAllItemsGame()
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
    //diffRegionFiles("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\BugRepro\region\r.0.0.mca""",
      //              """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\BugRepro\region\r.0.0.mca.new""")
    //dumpSomeCommandBlocks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\38a\region\r.0.0.mca""")
    //testing2()
    //placeCommandBlocksInTheWorldTemp("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\BugRepro\region\r.0.0.mca""")

    //mixTerrain()
    //findStrongholds()

    //printfn "%s" (makeCommandGivePlayerWrittenBook("Lorgon111", "BestTitle", [|"""["line1\n","line2"]"""; """["p2line1\n","p2line2",{"selector":"@p"}]"""|]))
    //dumpPlayerDat("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\fun with clone\playerdata\6fbefbde-67a9-4f72-ab2d-2f3ee5439bc0.dat""")

    
    //editMapDat("""C:\Users\"""+user+"""\Desktop\Eventide Trance v1.0.0 backup1\data\map_1.dat""")
    //testing2()
    //editMapDat("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp4\data\map_1.dat""")

    //mapDatToPng("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp9\data\map_0.dat""", """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp9\data\map_0.png""")
    //findAllLootBookItems("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\VoidLoot\region\""")
    //findAllLoot("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\Seed5Normal\region\""")
    //findAllLoot("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\43aAt8200\region\""")
    //testBackpatching("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\VoidLoot\region\r.0.0.mca""")
    //repopulateAsAnotherBiome()
    //debugRegion()
    //findUndergroundAirSpaceConnectedComponents()
    //substituteBlocks()
    (*
    let sb = new System.Text.StringBuilder()
    let sw = new System.IO.StringWriter(sb)
    simple_dungeon.Write(sw)
    sw.Close()
    printfn "%s" (sb.ToString())
    *)

    (*
    let fil = """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\15w45a\region\r.0.0.mca"""
    let r = RegionFile(fil)
    r.AddTileTick("minecraft:command_block",1,0,27,67,33)
    r.Write(fil+".new")
    *)
    //dumpTileTicks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\RandomCTM\region\r.0.0.mca""")
    //dumpTileTicks("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\seed31Copy\region\r.0.0.mca""")
    //diffDatFilesGui("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\tmp3\level.dat""","""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\tmp9\level.dat""")
    //diffDatFilesText("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\tmp3\level.dat""","""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\tmp9\level.dat""")
    //placeCertainBlocksInTheWorld()
    //placeVideoFramesInTheWorld()
    //dumpPlayerDat("""C:\Users\Admin1\AppData\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds\AhceAMzyAAA=\level.dat""")
    //dumpPlayerDat("""C:\Users\"""+user+"""\Desktop\igloo45a\igloo_bottom.nbt""")

    //musicStuff()
    //plotRegionalDifficulty()
    //chatToVoiceDemo()
    (*
    let map = new MapFolder(worldSaveFolder+"""\region\""")
    let rng = new System.Random()
    map.EnsureSetBlockIDAndDamage(1,100,1,0uy,0uy)
    TerrainAnalysisAndManipulation.putChestCore(1,100,1,54uy,2uy,Compounds(LootTables.NEWsampleTier4Chest(rng)),"",null,0L,map,null)
    TerrainAnalysisAndManipulation.putChestCore(1,100,3,54uy,2uy,Compounds(LootTables.NEWsampleTier4Chest(rng)),"",null,0L,map,null)
    TerrainAnalysisAndManipulation.putChestCore(1,100,5,54uy,2uy,Compounds(LootTables.NEWsampleTier4Chest(rng)),"",null,0L,map,null)
    TerrainAnalysisAndManipulation.putChestCore(1,100,7,54uy,2uy,Compounds(LootTables.NEWsampleTier4Chest(rng)),"",null,0L,map,null)
    TerrainAnalysisAndManipulation.putChestCore(1,100,9,54uy,2uy,Compounds(LootTables.NEWsampleTier4Chest(rng)),"",null,0L,map,null)
    map.WriteAll()
    *)

    (*
    compareMinecraftAssets("""C:\Users\Admin1\Desktop\16w02a.zip""","""C:\Users\Admin1\Desktop\16w03a.zip""")
    // compare sounds.json
    let currentSoundsJson = System.IO.File.ReadAllLines("""C:\Users\Admin1\AppData\Roaming\.minecraft\assets\objects\30\30b0989b97f6c10183de1ddf47f428b5d4edc895""")
    let oldSoundsJson = System.IO.File.ReadAllLines("""C:\Users\Admin1\Desktop\30b0989b97f6c10183de1ddf47f428b5d4edc895""")
    if not(diffStringArrays(oldSoundsJson, currentSoundsJson)) then
        printfn "no sound json diff"
    *)


    let biomeSize = 3
    let custom = MC_Constants.defaultWorldWithCustomOreSpawns(biomeSize,35,25,80,false,false,false,false,TerrainAnalysisAndManipulation.oreSpawnCustom)
    //let almostDefault = MC_Constants.defaultWorldWithCustomOreSpawns(biomeSize,8,80,4,true,true,true,true,MC_Constants.oreSpawnDefaults) // biome size kept, but otherwise default
    let worldSaveFolder = """C:\Users\""" + user + """\AppData\Roaming\.minecraft\saves\RandomCTM"""
    let brianRngSeed = 0
    //dumpPlayerDat(System.IO.Path.Combine(worldSaveFolder, "level.dat"))
    CustomizationKnobs.makeMapTimeNhours(System.IO.Path.Combine(worldSaveFolder, "level.dat"), 11)
    //TerrainAnalysisAndManipulation.makeCrazyMap(worldSaveFolder,brianRngSeed,custom)
    LootTables.writeAllLootTables(worldSaveFolder)
    // TODO below crashes game to embed world in one with diff level.dat ... but what does work is, gen world with options below, then copy the region files from my custom world to it
    // updateDat(System.IO.Path.Combine(worldSaveFolder, "level.dat"), (fun _pl nbt -> match nbt with |NBT.String("generatorOptions",_oldgo) -> NBT.String("generatorOptions",almostDefault) | _ -> nbt))
    System.IO.Directory.CreateDirectory(sprintf """%s\DIM-1\region\""" worldSaveFolder) |> ignore
    for x in [-1..0] do for z in [-1..0] do System.IO.File.Copy(sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\Void\region\r.%d.%d.mca""" user x z,sprintf """%s\DIM-1\region\r.%d.%d.mca""" worldSaveFolder x z, true)

    

    RecomputeLighting.demoBrokenBoundaries()

    printfn "press a key to end"
    System.Console.Beep()
    System.Console.ReadKey() |> ignore


    let worldSeed = 14 
    //System.Windows.Clipboard.SetText(custom)
    //genTerrainWithMCServer(worldSeed,custom)



    // to test
    // made some changes to cave-side-paths, see if ok... they are decent, main issue is still that not always at dead ends, due to imperfect skeleton
    // test new food balance
    // test new flat set piece...
    // test new iron/gold distr.
    // test new dungeon chance (seems ok)
    // test new distances to main dungeons...
    // test if teleporters get discovered (barely so far, now tried making farther and less spread out marks)
    // test if peaks (hmDiffPerCC value) look ok
    // get use to clicking for invincibility frames, think about mob balance with viable spam clicking
    // test SMP loot balance
    // (changed logAdmindCmds) a little a4ter set piece, huge game lag (and some 'block placed' notes), hm... logAdminCmds is true, should turn o44 to get rid o4 all set time? lighting updates?
    // SMP testing is super-useful!
    // playtest note: Fixxxer didn't use silk touch for feesh->cobble, nor think of picking up enderchest with it
    // playtest note: Fixxxer didn't think to mine cobble walls of dungeon
    // playtest note: Fixxxer went for red before green (forgot book info), got close
    // playtest note: can cheat the day/night boundary in SMP (but no known fix)
    // playtest note: Fix and I completed red after 3.2 hours; (another red and) mountain after 4.7 hours, whole map after 6 hours (did purple before secret)
    // playtest note: Fix & I did a second red, got more loot, was much easier 2nd time (better gear)
    // playtest note: Fix missed 2 desert chests before seeing one, missed a tree chest before seeing one
    // playtest note: Fix rarely read the name of chests the first time
    // playtest note: Fix found teleporter only after all monument blocks and flying elytra
    // playtest note: Obe used elytra to zoom in & out of 2nd mountain peak for extra loot.  i don't mind.

    // TODO: bugs & good ideas
    // obe notes that random-drop axes never have weapon enchants
    // need to have thank yous to obe/fix, and others who help (codewarrior?)
    // obe did not know he could walk into black cube of teleporter, thought he needed enderpearl
    // to make teleporters more discoverable, have any out-of-place light source (red lamp? torch? glow?) along the path.  'light' = 'come look', and then will see path, etc. ...
    // Fixxer died, daylight went even under bedrock ceiling, also lag, so really need to fix lighting, and SMP is good way to test
    // glass set piece had cave below it, fixer went into it.  set piece was kinda blah
    // vary food per starting biome?
    // purple beacon dungeon - witches infighting caused a feesh-pocalypse, I nearly died... not sure how to deal with, but un-ideal
    // since dungeons can be done out of order, last book cannot be written to assume it is the last book.  (also has typo on 2nd page, and is unfinished)
    // consider not turning on purple beacon until the book describing it has been found? (how to implement, and be sure book says it's been unlocked, so if people mapped they don't assume not there b/c didn't see before)
    // elytra and jump pots need to be completely reworked/rethought:
    //  - fixxer died crashing from elytra, would have lost all stuff in unknown area, including a monument block (keepInventory with elytra?)
    //  - had never used elytra before, didn't know how to fly, looked straight down, died of fall damage (FF X boots? but maybe can still die of velocity...)
    //  - jump pots unstack when dropped, inventory nightmare (other takeoff mechanic?)
    //  - maybe move monument block to mountain peak, make secret treasure book part of mountain peak loot, elytra becomes optional? but then need to figure out where purple beacon book goes
    //  - also could have a 'how to fly with elytra' book
    // teleporter command blocks look like a bug, consider moving cmds
    // value of emeralds unknown at start of map, have starter book say they can eventually be traded for useful 'buffs'
    // teleporter does not work in SMP, got glitched into bedrock.  figure that out.
    // fix and I blitzed the green dungeon easy by moving fast, maybe have each spawner have a 1/20 chance of insta-spawning?
    // both Fix and Obe doubted secret treasure coords because there was no 'mark'; neither initially dug.  So consider something (one small out of place flower? coarse dirt is great (no snow atop)) can also say 'unmarked treasure' in the book
    // more varied terrain (like the end/hell trees) makes exploring more fun; vanilla is vanilla)
    // there may be too many red dungeons (we saw like 5); ok if folks have to look a little, as I need more exploration anyway
    // spider jockey spawner above the bedrock of mountain peak was kinda 'useful' (spawned mobs, made reason to go atop), consider intentionally putting stuff above it
    // lag again at flat cobwebs, think skylight calc is an issue (unrepresented sections?) can probably hack something to ensure sections represented... maybe also LightPopulated=0 and ceiling is the issue? could try LP=1, though then other bits may break? hm...
    // speed bu44 is quite nice (emeralds in short supply - buff? did now, see how works out); strength seemed ok
    // 4inal book TODOs, some kind o4 better acknowledgement when complete map
    // diorite pillars at day/night go down into some caves, was kind a cool effect, other ways to use?
    // getting chest in glass set piece spawns 4 ghasts - good or bad?
    // spawners inside tree leaves are nice simple 'traps', maybe by the tree chests? (how look on fast graphics if inside?)
    // villager at end of green beacon who has no trades but suggests you look for his friends?
    // consider directly give some fw2 boots with other good something, to make more likely to use? no, has downsides too (waterfalls, farms, ...), hm
    // some other block substitutions to make things interesting? six-sided oak/shroom texture (set bits 4 & 8 of log dmg)? cobble stairs/double-slabs to feign stone in caves?
    // glowstone 'ore veins' are fun to find (instead of lava? creepers can wreck? ...)
    // summary and maybe starting book need summary customization info (seed, # player loot, difficulty, ...)
    // remove bingo's spammable iron sword
    // ice near spawners in cave, to knock out torches?
    // snow golems and creepers something something
    // still hard to get init bow
    // note to self: craft gapples next time
    // still some skeleton bugs (screenshots from jan 20)
    // good horse spawn egg as loot? (encourage travel/exploration?)
    // ***witch zones / guardian zones (could be small zone, but when you stand at loot chest, they spawn?)
    // zisteau-like firelands biome (netherrack trees on fire, lava rivers/lakes, ...)? aesthetic biomes with block changes? ... swapping out grass for X (mycelium, red mush top?) can be good; randomizing the trees?
    // ***'themed' mobs, e.g. lots of undead, so smite/IH works, etc, make 'gear options' valuable
    // ***configurable difficulty (# spawners, amount of loot, limit good foods, mob stats? map size? local difficulty/inhabitedtime? weather? uhc mode?) (somewhat in-progress already)
    // ***look of dungeons customized, e.g. moss -> netherrack in hell biome, more spawners per dungeon has better loot
    // spawner underneath dead-end chest?
    // ***hyper-spawner (dispenser with eggs)
    // ***places where gravel floor falls
    //   - desert trap, can rig sand to fall like 10 blocks to obsidian line hole with a low-range spawner underneath, or have TNT, etc, can find flat areas, preserve cacti, etc.
    //   - key is getting player to set off trap, need a hook: gold block, chest, etc. and a few of them so not every one is a trap
    // ***skeleton trap, electrified creeper
    // ***primed tnt buried in walls
    // "retro"/"throwback"?
    // more variety of random-chest-loot (have some good weapons/armor that will break quickly (e.g. smite V diamond sword with only 50 durability), or other 'collectables'); loot increases with distance from spawn? some traps necessary
    // legendary: seeing x from afar leads to seeing y up close...
    // have a way to 'go to normal', e.g. turn off world border, (world embedded in normal terrain generator, ores, dungeons, with structures on, 
    //    small biomes, same seed, seamless?), turn off night stuff, how fix nether? ...
    //    what about drops after game is over? conditionally change all mob drops back to normal based on scoreboard? or? (like nether, have people delete files?)
    // glowstone behind stairs in wall (like Eventide Trance) highlights part of cave/dungeon without giving light
    // TODOs and refactorings...
    // return to spawn from teleporter, villagers don't immediately appear on client?
    // noisemaker noteblock troll underground? fireworks spawner
    // a finite set of collectable items could be good long-term for completionists


    // other ideas
    // floating structures? lava/water pillars fall down? sky has 'advantage' of being open to build without overlap... eventide trance parkour up the creeper platforms stuff
    // could put more than one spawner in wall in green/purple cave as progress... could extinguish lava -> obsidian for less light...
    // teleporters could be color-coded (easier to remember?)
    // tnt exploding a morse code explosion-sound something?!?
    // infinite loot chest puzzle (need a certain tree/path through 27 chests in each to win, or something)
    // fix heightmap issues? blocklight/skylight?
    // hollow out mountain, for top-down, multi-tier, encased in bedrock dungeon? ghast spawner atop mtn when broken could lead to ladder down to hollowed mountain dungeon
    // forest or ice spikes are good places to 'hide spawners' (trees, spikes) in an area, but how/what to put in middle to find?
    // desert falling sand could also open hole into giant underground cavern of some sort?
    // consider strongly how hard it would be for a good biome-specific music implementation (transitions etc) to e.g. help alert of nether biomes, day/night area, etc
    // more random chest locations:
    //   - bottom of surface ravine (low heightmap)
    //   - flower forest, near poeny/rosebush, put '+' of tall flowers with chest in middle
    //   - just anywhere on a plain with a mob-set-piece guarding it makes for interesting bits
    // more variants of set pieces (persistence-required mobs placed in map)
    //   - land guardians may make a good set piece (protection books?)
    //   - something to protect a good bow? close to spawn? kinda hard to get good bow early
    // rand cave wall spawners guarding iron/gold blocks?
    



    (*
    let scrabbleRegion = """C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\ScrabbleBackup\region\r.0.0.mca"""
    let r = new RegionFile(scrabbleRegion)
    doTrie(r)
    //r.Write(scrabbleRegion+".new")
    //System.IO.File.Delete(scrabbleRegion)
    //System.IO.File.Move(scrabbleRegion+".new",scrabbleRegion)
    *)

    let readInSomeArt = false
    if readInSomeArt then
        let fil = """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\BingoArt\region\r.0.0.mca"""
        let r = new RegionFile(fil)
        let arr = ResizeArray()
        let s = AA.readZoneIntoString(r,165,3,39,16,1,16)
        arr.Add(sprintf """let cw = "%s" """ s)
        let writePath = """C:\Users\Admin1\Documents\GitHubVisualStudio\minecraft-map-manipulator\MinecraftMapManipulator\ConsoleApplication1\Temp.txt"""
        System.IO.File.WriteAllLines(writePath, arr)


#if BINGO
    printfn "bingo seed is 8126030 preset to clipboard..."
    System.Windows.Clipboard.SetText(MC_Constants.defaultWorldWithCustomOreSpawns(1,100,4,80,true,true,true,true,MC_Constants.oreSpawnBingo))

    let onlyArt = false
    let save = if onlyArt then "BingoArt" else "tmp9"
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
    try 
        placeCommandBlocksInTheWorld(sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\r.0.0.mca""" user save, onlyArt) 
    with e -> 
        printfn "caught exception: %s" (e.Message)
    (*
    preciseImageToBlocks(sprintf """C:\Users\%s\Desktop\Minimap_Floor_6.png""" user, sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\""" user save, 36)
    preciseImageToBlocks(sprintf """C:\Users\%s\Desktop\Minimap_Floor_7.png""" user, sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\""" user save, 32)
    preciseImageToBlocks(sprintf """C:\Users\%s\Desktop\Minimap_Floor_8.png""" user, sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\region\""" user save, 28)
    *)
    System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp4\data\map_0.dat.new""",
                        sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\data\map_0.dat""" user save, true)
    System.IO.File.Copy("""C:\Users\"""+user+"""\AppData\Roaming\.minecraft\saves\tmp4\data\map_1.dat.new""",
                        sprintf """C:\Users\%s\AppData\Roaming\.minecraft\saves\%s\data\map_1.dat""" user save, true)
    if not onlyArt then
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