module MC_Constants

let BIOMES = // id, name, good-map-color-Brian-chose
    [|0,"Ocean",48;1,"Plains",6;2,"Desert",10;3,"Extreme Hills",26;4,"Forest",5;5,"Taiga",31;6,"Swampland",4;7,"River",50;8,"Hell",142;9,"Sky",73;
      10,"Frozen Ocean",20;11,"FrozenRiver",22;12,"Ice Plains",34;13,"Ice Mountains",32;14,"MushroomIsland",64;15,"MushroomIslandShore",66;16,"Beach",9;17,"DesertHills",8;18,"ForestHills",7;19,"TaigaHills",28;
      20,"Extreme Hills Edge",25;21,"Jungle",78;22,"Jungle Hills",77;23,"Jungle Edge",76;24,"Deep Ocean",51;25,"Stone Beach",46;26,"Cold Beach",44;27,"Birch Forest",110;28,"Birch Forest Hills",109;29,"Roofed Forest",111;
      30,"Cold Taiga",132;31,"Cold Taiga Hills",135;32,"Mega Taiga",106;33,"Mega Taiga Hills",105;34,"Extreme Hills+",26;35,"Savanna",120;36,"Savanna Plateau",123;37,"Mesa",114;38,"Mesa Plateau F",113;39,"Mesa Plateau",112;
      127,"The Void",0;
      128,"Plains M",6;129,"Sunflower Plains",6;
      130,"Desert M",10;131,"Extreme Hills M",26;132,"Flower Forest",5;133,"Taiga M",31;134,"Swampland M",4;
      140,"Ice Plains Spikes",21;141,"Ice Mountains Spikes",20;149,"Jungle M",78;
      151,"JungleEdge M",76;155,"Birch Forest M",110;156,"Birch Forest Hills M",109;157,"Roofed Forest M",111;158,"Cold Taiga M",132;
      160,"Mega Spruce Taiga",106;161,"Mega Spruce Taiga",106;162,"Extreme Hills+ M",26;163,"Savanna M",120;164,"Savanna Plateau M",123;165,"Mesa (Bryce)",114;166,"Mesa Plateau F M",113;167,"Mesa Plateau M",113;
      -1,"(Uncalculated)",0|]

let BIOMES_NEEDED_FOR_ADVENTURING_TIME = 
    [|
        "Beach"
        "Birch Forest"
        "Birch Forest Hills"
        "Cold Beach"
        "Cold Taiga"
        "Cold Taiga Hills"
        "Deep Ocean"
        "Desert"
        "DesertHills"
        "Extreme Hills"
        "Extreme Hills+"
        "Forest"
        "ForestHills"
        "FrozenRiver"
        "Ice Plains"
        "Jungle"
        "JungleEdge"
        "JungleHills"
        "Mega Taiga"
        "Mega Taiga Hills"
        "Mesa"
        "Mesa Plateau"
        "Mesa Plateau F"
        "MushroomIsland"
        "MushroomIslandShore"
        "Ice Mountains"
        "Ocean"
        "Plains"
        "Savanna Plateau"
        "River"
        "Roofed Forest"
        "Savanna"
        "Swampland"
        "Stone Beach"
        "Taiga"
        "TaigaHills"
    |]



let ENCHANTS =
    [|
        "Protection","protection", 0 
        "Fire Protection","fire_protection", 1 
        "Feather Falling","feather_falling", 2 
        "Blast Protection","blast_protection", 3 
        "Projectile Protection","projectile_protection", 4 
        "Respiration","respiration", 5 
        "Aqua Affinity","aqua_affinity", 6 
        "Thorns","thorns", 7 
        "Depth Strider","depth_strider", 8 
        "Frost Walker","frost_walker", 9 
        "Sharpness","sharpness", 16 
        "Smite","smite", 17 
        "Bane of Arthropods","bane_of_arthropods", 18 
        "Knockback","knockback", 19 
        "Fire Aspect","fire_aspect", 20 
        "Looting","looting", 21 
        "Efficiency","efficiency", 32 
        "Silk Touch","silk_touch", 33 
        "Unbreaking","unbreaking", 34 
        "Fortune","fortune", 35 
        "Power","power", 48 
        "Punch","punch", 49 
        "Flame","flame", 50 
        "Infinity","infinity", 51 
        "Luck of the Sea","luck_of_the_sea", 61 
        "Lure","lure", 62 
        "Mending","mending", 70 
    |]

let POTION_EFFECTS =
    [|1,"Speed";2,"Slowness";3,"Haste";4,"Mining Fatigue";5,"Strength";6,"Instant Health";7,"Instant Damage";8,"Jump Boost";9,"Nausea";
      10,"Regeneration";11,"Resistance";12,"Fire Resistance";13,"Water Breathing";14,"Invisibility";15,"Blindness";16,"Night vision";17,"Hunger";18,"Weakness";19,"Poison";
      20,"Wither";21,"Health Boost";22,"Absorption";23,"Saturation";24,"Glowing";25,"Levitation";26,"Luck";27,"Bad Luck"|]

let WOOL_COLORS = [|0,"White";1,"Orange";2,"Magenta";3,"Light Blue";4,"Yellow";5,"Lime";6,"Pink";7,"Gray";8,"Light Gray";9,"Cyan";10,"Purple";11,"Blue";12,"Brown";13,"Green";14,"Red";15,"Black"|]

// map top-blocks to map colors here:
// https://gist.githubusercontent.com/codewarrior0/6754728d68e3f28241ce/raw/17469d3c209b105b03c319ff197b8c6c80ee12fe/color_indexes.json
// map block descriptions to damage value here:
// https://github.com/mcedit/mcedit2/blob/master/src/mceditlib/blocktypes/idmapping_raw.json

let BLOCK_IDS =
    [|0,"Air";1,"Stone";2,"Grass Block";3,"Dirt";4,"Cobblestone";5,"Wood Planks";6,"Saplings";7,"Bedrock";8,"Water";9,"Stationary water";
      10,"Lava";11,"Stationary lava";12,"Sand";13,"Gravel";14,"Gold Ore";15,"Iron Ore";16,"Coal Ore";17,"Wood";18,"Leaves";19,"Sponge";
      20,"Glass";21,"Lapis Lazuli Ore";22,"Lapis Lazuli Block";23,"Dispenser";24,"Sandstone";25,"Note Block";26,"Bed";27,"Powered Rail";28,"Detector Rail";29,"Sticky Piston";
      30,"Cobweb";31,"Grass";32,"Dead Bush";33,"Piston";34,"Piston Extension";35,"Wool";36,"Block moved by Piston";37,"Dandelion";38,"Poppy";39,"Brown Mushroom";
      40,"Red Mushroom";41,"Block of Gold";42,"Block of Iron";43,"Double Stone Slab";44,"Stone Slab";45,"Bricks";46,"TNT";47,"Bookshelf";48,"Moss Stone";49,"Obsidian";
      50,"Torch";51,"Fire";52,"Monster Spawner";53,"Oak Wood Stairs";54,"Chest";55,"Redstone Wire";56,"Diamond Ore";57,"Block of Diamond";58,"Crafting Table";59,"Wheat";
      60,"Farmland";61,"Furnace";62,"Burning Furnace";63,"Sign Post";64,"Wooden Door";65,"Ladders";66,"Rail";67,"Cobblestone Stairs";68,"Wall Sign";69,"Lever";
      70,"Stone Pressure Plate";71,"Iron Door";72,"Wooden Pressure Plate";73,"Redstone Ore";74,"Glowing Redstone Ore";75,"Redstone Torch (inactive)";76,"Redstone Torch (active)";77,"Stone Button";78,"Snow";79,"Ice";
      80,"Snow Block";81,"Cactus";82,"Clay";83,"Sugar Cane";84,"Jukebox";85,"Fence";86,"Pumpkin";87,"Netherrack";88,"Soul Sand";89,"Glowstone";
      90,"Nether Portal";91,"Jack 'o' Lantern";92,"Cake Block";93,"Redstone Repeater (inactive)";94,"Redstone Repeater (active)";95,"Locked Chest";96,"Trapdoor";97,"Monster Egg";98,"Stone Bricks";99,"Huge Brown Mushroom";
      100,"Huge Red Mushroom";101,"Iron Bars";102,"Glass Pane";103,"Melon";104,"Pumpkin Stem";105,"Melon Stem";106,"Vines";107,"Fence Gate";108,"Brick Stairs";109,"Stone Brick Stairs";
      110,"Mycelium";111,"Lily Pad";112,"Nether Brick";113,"Nether Brick Fence";114,"Nether Brick Stairs";115,"Nether Wart";116,"Enchantment Table";117,"Brewing Stand";118,"Cauldron";119,"End Portal";
      120,"End Portal Block";121,"End Stone";122,"Dragon Egg";123,"Redstone Lamp (inactive)";124,"Redstone Lamp (active)";125,"Wooden Double Slab";126,"Wooden Slab";127,"Cocoa";128,"Sandstone Stairs";129,"Emerald Ore";
      130,"Ender Chest";131,"Tripwire Hook";132,"Tripwire";133,"Block of Emerald";134,"Spruce Wood Stairs";135,"Birch Wood Stairs";136,"Jungle Wood Stairs";137,"Command Block";138,"Beacon";139,"Cobblestone Wall";
      140,"Flower Pot";141,"Carrots";142,"Potatoes";143,"Wooden Button";144,"Mob Head";145,"Anvil";146,"Trapped Chest";147,"Weighted Pressure Plate (Light)";148,"Weighted Pressure Plate (Heavy)";149,"Redstone Comparator (inactive)";
      150,"Redstone Comparator (active)";151,"Daylight Sensor";152,"Block of Redstone";153,"Nether Quartz Ore";154,"Hopper";155,"Block of Quartz";156,"Quartz Stairs";157,"Activator Rail";158,"Dropper";159,"Stained Clay";
      170,"Hay Block";171,"Carpet";172,"Hardened Clay";173,"Block of Coal";174,"Packed Ice";175,"Large Flowers"|]

let blockIdToMinecraftName =
    [|
        1,"minecraft:stone"
        2,"minecraft:grass"
        3,"minecraft:dirt"
        4,"minecraft:cobblestone"
        5,"minecraft:planks"
        6,"minecraft:sapling"
        7,"minecraft:bedrock"
        8,"minecraft:flowing_water"
        9,"minecraft:water"
        10,"minecraft:flowing_lava"
        11,"minecraft:lava"
        12,"minecraft:sand"
        13,"minecraft:gravel"
        14,"minecraft:gold_ore"
        15,"minecraft:iron_ore"
        16,"minecraft:coal_ore"
        17,"minecraft:log"
        18,"minecraft:leaves"
        19,"minecraft:sponge"
        20,"minecraft:glass"
        21,"minecraft:lapis_ore"
        22,"minecraft:lapis_block"
        23,"minecraft:dispenser"
        24,"minecraft:sandstone"
        25,"minecraft:noteblock"
        26,"minecraft:bed"
        27,"minecraft:golden_rail"
        28,"minecraft:detector_rail"
        29,"minecraft:sticky_piston"
        30,"minecraft:web"
        31,"minecraft:tallgrass"
        32,"minecraft:deadbush"
        33,"minecraft:piston"
        34,"minecraft:piston_head"
        35,"minecraft:wool"
        36,"minecraft:piston_extension"
        37,"minecraft:yellow_flower"
        38,"minecraft:red_flower"
        39,"minecraft:brown_mushroom"
        40,"minecraft:red_mushroom"
        41,"minecraft:gold_block"
        42,"minecraft:iron_block"
        43,"minecraft:double_stone_slab"
        44,"minecraft:stone_slab"
        45,"minecraft:brick_block"
        46,"minecraft:tnt"
        47,"minecraft:bookshelf"
        48,"minecraft:mossy_cobblestone"
        49,"minecraft:obsidian"
        50,"minecraft:torch"
        51,"minecraft:fire"
        52,"minecraft:mob_spawner"
        53,"minecraft:oak_stairs"
        54,"minecraft:chest"
        55,"minecraft:redstone_wire"
        56,"minecraft:diamond_ore"
        57,"minecraft:diamond_block"
        58,"minecraft:crafting_table"
        59,"minecraft:wheat"
        60,"minecraft:farmland"
        61,"minecraft:furnace"
        62,"minecraft:lit_furnace"
        63,"minecraft:standing_sign"
        64,"minecraft:wooden_door"
        65,"minecraft:ladder"
        66,"minecraft:rail"
        67,"minecraft:stone_stairs"
        68,"minecraft:wall_sign"
        69,"minecraft:lever"
        70,"minecraft:stone_pressure_plate"
        71,"minecraft:iron_door"
        72,"minecraft:wooden_pressure_plate"
        73,"minecraft:redstone_ore"
        74,"minecraft:lit_redstone_ore"
        75,"minecraft:unlit_redstone_torch"
        76,"minecraft:redstone_torch"
        77,"minecraft:stone_button"
        78,"minecraft:snow_layer"
        79,"minecraft:ice"
        80,"minecraft:snow"
        81,"minecraft:cactus"
        82,"minecraft:clay"
        83,"minecraft:reeds"
        84,"minecraft:jukebox"
        85,"minecraft:fence"
        86,"minecraft:pumpkin"
        87,"minecraft:netherrack"
        88,"minecraft:soul_sand"
        89,"minecraft:glowstone"
        90,"minecraft:portal"
        91,"minecraft:lit_pumpkin"
        92,"minecraft:cake"
        93,"minecraft:unpowered_repeater"
        94,"minecraft:powered_repeater"
        95,"minecraft:stained_glass"
        96,"minecraft:trapdoor"
        97,"minecraft:monster_egg"
        98,"minecraft:stonebrick"
        99,"minecraft:brown_mushroom_block"
        100,"minecraft:red_mushroom_block"
        101,"minecraft:iron_bars"
        102,"minecraft:glass_pane"
        103,"minecraft:melon_block"
        104,"minecraft:pumpkin_stem"
        105,"minecraft:melon_stem"
        106,"minecraft:vine"
        107,"minecraft:fence_gate"
        108,"minecraft:brick_stairs"
        109,"minecraft:stone_brick_stairs"
        110,"minecraft:mycelium"
        111,"minecraft:waterlily"
        112,"minecraft:nether_brick"
        113,"minecraft:nether_brick_fence"
        114,"minecraft:nether_brick_stairs"
        115,"minecraft:nether_wart"
        116,"minecraft:enchanting_table"
        117,"minecraft:brewing_stand"
        118,"minecraft:cauldron"
        119,"minecraft:end_portal"
        120,"minecraft:end_portal_frame"
        121,"minecraft:end_stone"
        122,"minecraft:dragon_egg"
        123,"minecraft:redstone_lamp"
        124,"minecraft:lit_redstone_lamp"
        125,"minecraft:double_wooden_slab"
        126,"minecraft:wooden_slab"
        127,"minecraft:cocoa"
        128,"minecraft:sandstone_stairs"
        129,"minecraft:emerald_ore"
        130,"minecraft:ender_chest"
        131,"minecraft:tripwire_hook"
        132,"minecraft:tripwire"
        133,"minecraft:emerald_block"
        134,"minecraft:spruce_stairs"
        135,"minecraft:birch_stairs"
        136,"minecraft:jungle_stairs"
        137,"minecraft:command_block"
        138,"minecraft:beacon"
        139,"minecraft:cobblestone_wall"
        140,"minecraft:flower_pot"
        141,"minecraft:carrots"
        142,"minecraft:potatoes"
        143,"minecraft:wooden_button"
        144,"minecraft:skull"
        145,"minecraft:anvil"
        146,"minecraft:trapped_chest"
        147,"minecraft:light_weighted_pressure_plate"
        148,"minecraft:heavy_weighted_pressure_plate"
        149,"minecraft:unpowered_comparator"
        150,"minecraft:powered_comparator"
        151,"minecraft:daylight_detector"
        152,"minecraft:redstone_block"
        153,"minecraft:quartz_ore"
        154,"minecraft:hopper"
        155,"minecraft:quartz_block"
        156,"minecraft:quartz_stairs"
        157,"minecraft:activator_rail"
        158,"minecraft:dropper"
        159,"minecraft:stained_hardened_clay"
        160,"minecraft:stained_glass_pane"
        161,"minecraft:leaves2"
        162,"minecraft:log2"
        163,"minecraft:acacia_stairs"
        164,"minecraft:dark_oak_stairs"
        165,"minecraft:slime"
        166,"minecraft:barrier"
        167,"minecraft:iron_trapdoor"
        168,"minecraft:prismarine"
        169,"minecraft:sea_lantern"
        170,"minecraft:hay_block"
        171,"minecraft:carpet"
        172,"minecraft:hardened_clay"
        173,"minecraft:coal_block"
        174,"minecraft:packed_ice"
        175,"minecraft:double_plant"
        176,"minecraft:standing_banner"
        177,"minecraft:wall_banner"
        178,"minecraft:daylight_detector_inverted"
        179,"minecraft:red_sandstone"
        180,"minecraft:red_sandstone_stairs"
        181,"minecraft:double_stone_slab2"
        182,"minecraft:stone_slab2"
        183,"minecraft:spruce_fence_gate"
        184,"minecraft:birch_fence_gate"
        185,"minecraft:jungle_fence_gate"
        186,"minecraft:dark_oak_fence_gate"
        187,"minecraft:acacia_fence_gate"
        188,"minecraft:spruce_fence"
        189,"minecraft:birch_fence"
        190,"minecraft:jungle_fence"
        191,"minecraft:dark_oak_fence"
        192,"minecraft:acacia_fence"
        193,"minecraft:spruce_door"
        194,"minecraft:birch_door"
        195,"minecraft:jungle_door"
        196,"minecraft:acacia_door"
        197,"minecraft:dark_oak_door"
        198,"minecraft:end_rod"
        199,"minecraft:chorus_plant"
        200,"minecraft:chorus_flower"
        201,"minecraft:purpur_block"
        202,"minecraft:purpur_pillar"
        203,"minecraft:purpur_stairs"
        204,"minecraft:purpur_double_slab"
        205,"minecraft:purpur_slab"
        206,"minecraft:end_bricks"
        207,"minecraft:beetroot_seeds"
        208,"minecraft:grass_path"
        209,"minecraft:end_gateway"
        210,"minecraft:repeating_command_block"
        211,"minecraft:chain_command_block"
        212,"minecraft:frosted_ice"
        255,"minecraft:structure_block"
    |]

let BLOCKIDS_THAT_EMIT_LIGHT =
    [|  // block id, light level
        138,15 // beacon
        119,15 // end portal block
        51, 15 // fire
        89, 15 // glowstone
        91, 15 // jack o' lantern
        10, 15 // flowing lava
        11, 15 // lava source
        124,15 // powered redstone lamp
        169,15 // sea lantern
        198,14 // end rod 
        50, 14 // torch
        62, 13 // lit furnace
        90, 11 // nether portal block
        74,  9  // lit redstone ore
        130, 7  // ender chest
        76,  7  // restone torch (on)
        117, 1  // brewing stand
        39,  1  // brown mushroom
        122, 1  // dragon egg
        120, 1  // end portal frame
    |]

let BLOCKIDS_THAT_FILTER_SKYLIGHT = [|18;30;161|]  // cobweb and leaves
let BLOCKIDS_THAT_LOWER_LIGHT_BY_TWO = [|8;9;79;212|]  // water and ice
let BLOCKIDS_THAT_ARE_FULLY_TRANSPARENT_TO_LIGHT = 
    [|0;6;10;11;20;26;27;28;29;31;32;33;34;36;37;38;39;40;50;51;52;54;55;59;63;64;65;66;68;69;70;71;72;75;76;77;78;81;83;85;90;92;93;94;95;96;
      101;102;104;105;106;107;111;113;115;116;117;118;119;120;122;127;130;131;132;138;139;140;141;142;143;144;145;146;147;148;149;150;
      151;154;157;160;165;166;167;171;175;176;177;178;183;184;185;186;187;188;189;190;191;192;193;194;195;196;197;
      198;199;200;207;
      |]
(*  // above was computed using some code from mcedit raw data repository:

    let mcdata = System.IO.File.ReadAllText("""C:\Users\Admin1\Desktop\minecraft_raw.txt""")
    let jss = new System.Web.Script.Serialization.JavaScriptSerializer()
    let o = jss.DeserializeObject(mcdata)
    let bso = new System.Collections.Generic.Dictionary<string,int>()
    for d in (o:?>obj[]) do
        let dict = d :?> System.Collections.Generic.Dictionary<string,obj>
        let s = dict.["blockState"] :?> string
        let d = dict.["opacity"] :?> int
        bso.Add(s,d)
    let o = jss.DeserializeObject(System.IO.File.ReadAllText("""C:\Users\Admin1\Desktop\idmapping_raw.txt"""))
    let bsi = new System.Collections.Generic.Dictionary<string,int>()
    for d in (o:?>obj[]) do
        let a = d :?> obj[]
        let bid = a.[0] :?> int
        let bs = a.[2] :?> string
        bsi.Add(bs, bid)
    let opacity = Array.create 256 -1
    for KeyValue(bs,bid) in bsi do
        let op = bso.[bs]
        if opacity.[bid] <> -1 && opacity.[bid] <> op then
            failwith "two values"
        else
            opacity.[bid] <- op
    printf "[|"
    for i = 0 to 255 do
        if opacity.[i] = 0 then
            printf "%d;" i
    printfn "|]"
*)

let MAP_COLOR_TABLE =
    [|
        0,(0,0,0)  // Transparent Not explored 
        1,(0,0,0)
        2,(0,0,0)
        3,(0,0,0)
        4,(88,124,39)
        5,(108,151,47)
        6,(125,176,55)
        7,(66,93,29)
        8,(172,162,114)
        9,(210,199,138)
        10,(244,230,161)
        11,(128,122,85)
        12,(138,138,138)
        13,(169,169,169)
        14,(197,197,197)
        15,(104,104,104)
        16,(178,0,0)
        17,(217,0,0)
        18,(252,0,0)
        19,(133,0,0)
        20,(111,111,178)
        21,(136,136,217)
        22,(158,158,252)
        23,(83,83,133)
        24,(116,116,116)
        25,(142,142,142)
        26,(165,165,165)
        27,(87,87,87)
        28,(0,86,0)
        29,(0,105,0)
        30,(0,123,0)
        31,(0,64,0)
        32,(178,178,178)
        33,(217,217,217)
        34,(252,252,252)
        35,(133,133,133)
        36,(114,117,127)
        37,(139,142,156)
        38,(162,166,182)
        39,(85,87,96)
        40,(105,75,53)
        41,(128,93,65)
        42,(149,108,76)
        43,(78,56,39)
        44,(78,78,78)
        45,(95,95,95)
        46,(111,111,111)
        47,(58,58,58)
        48,(44,44,178)
        49,(54,54,217)
        50,(63,63,252)
        51,(33,33,133)
        52,(99,83,49)
        53,(122,101,61)
        54,(141,118,71)
        55,(74,62,38)
        56,(178,175,170)
        57,(217,214,208)
        58,(252,249,242)
        59,(133,131,127)
        60,(150,88,36)
        61,(184,108,43)
        62,(213,125,50)
        63,(113,66,27)
        64,(124,52,150)
        65,(151,64,184)
        66,(176,75,213)
        67,(93,39,113)
        68,(71,107,150)
        69,(87,130,184)
        70,(101,151,213)
        71,(53,80,113)
        72,(159,159,36)
        73,(195,195,43)
        74,(226,226,50)
        75,(120,120,27)
        76,(88,142,17)
        77,(108,174,21)
        78,(125,202,25)
        79,(66,107,13)
        80,(168,88,115)
        81,(206,108,140)
        82,(239,125,163)
        83,(126,66,86)
        84,(52,52,52)
        85,(64,64,64)
        86,(75,75,75)
        87,(39,39,39)
        88,(107,107,107)
        89,(130,130,130)
        90,(151,151,151)
        91,(80,80,80)
        92,(52,88,107)
        93,(64,108,130)
        94,(75,125,151)
        95,(39,66,80)
        96,(88,43,124)
        97,(108,53,151)
        98,(125,62,176)
        99,(66,33,93)
        100,(36,52,124)
        101,(43,64,151)
        102,(50,75,176)
        103,(27,39,93)
        104,(71,52,36)
        105,(87,64,43)
        106,(101,75,50)
        107,(53,39,27)
        108,(71,88,36)
        109,(87,108,43)
        110,(101,125,50)
        111,(53,66,27)
        112,(107,36,36)
        113,(130,43,43)
        114,(151,50,50)
        115,(80,27,27)
        116,(17,17,17)
        117,(21,21,21)
        118,(25,25,25)
        119,(13,13,13)
        120,(174,166,53)
        121,(212,203,65)
        122,(247,235,76)
        123,(130,125,39)
        124,(63,152,148)
        125,(78,186,181)
        126,(91,216,210)
        127,(47,114,111)
        128,(51,89,178)
        129,(62,109,217)
        130,(73,129,252)
        131,(39,66,133)
        132,(0,151,39)
        133,(0,185,49)
        134,(0,214,57)
        135,(0,113,30)
        136,(90,59,34)
        137,(110,73,41)
        138,(127,85,48)
        139,(67,44,25)
        140,(78,1,0)
        141,(95,1,0)
        142,(111,2,0)
        143,(58,1,0)
    |]

////////////////////////////////////////////////////////////////////////////////////


let textureFilenamesToBlockIDandDataMapping =
    [|
        "bedrock",7,0
        "bookshelf",47,0
        "brick",45,0
        "clay",82,0
        "coal_block",173,0
        "coal_ore",16,0
        "cobblestone",4,0
        "cobblestone_mossy",48,0
        "command_block",137,0
        "crafting_table_front",58,0
        "crafting_table_side",58,0  // TODO
        "diamond_block",57,0
        "diamond_ore",56,0
        "dirt",3,0
        "dirt_podzol_side",3,2
        "dispenser_front_horizontal",23,3  // TODO
        "dispenser_front_vertical",23,3  // TODO
        "dropper_front_horizontal",158,3  // TODO
        "dropper_front_vertical",158,3  // TODO
        "emerald_block",133,0
        "emerald_ore",129,0
        "end_stone",121,0
        "furnace_front_off",61,4
        "furnace_front_on",62,4
        "furnace_side",61,3
        "glowstone",89,0
        "gold_block",41,0
        "gold_ore",14,0
        "gravel",13,0
        "hardened_clay",172,0
        "hardened_clay_stained_black",159,15
        "hardened_clay_stained_blue",159,11
        "hardened_clay_stained_brown",159,12
        "hardened_clay_stained_cyan",159,9
        "hardened_clay_stained_gray",159,7
        "hardened_clay_stained_green",159,13
        "hardened_clay_stained_light_blue",159,3
        "hardened_clay_stained_lime",159,5
        "hardened_clay_stained_magenta",159,2
        "hardened_clay_stained_orange",159,1
        "hardened_clay_stained_pink",159,6
        "hardened_clay_stained_purple",159,10
        "hardened_clay_stained_red",159,14
        "hardened_clay_stained_silver",159,8
        "hardened_clay_stained_white",159,0
        "hardened_clay_stained_yellow",159,4
        "hay_block_side",170,0
        "hay_block_top",170,0  // TODO
        "ice",79,0
        "ice_packed",174,0
        "iron_block",42,0
        "iron_ore",15,0
        "jukebox_side",84,0
        "lapis_block",22,0
        "lapis_ore",21,0
        "log_acacia",162,0
        "log_acacia_top",162,8
        "log_big_oak",162,1
        "log_big_oak_top",162,9
        "log_birch",17,2
        "log_birch_top",17,10
        "log_jungle",17,3
        "log_jungle_top",17,11
        "log_oak",17,0
        "log_oak_top",17,8
        "log_spruce",17,1
        "log_spruce_top",17,9
        "melon_side",103,0  
        "melon_top",103,0  // TODO
        "mushroom_block_inside",99,0  // TODO
        "mushroom_block_skin_brown",99,14
        "mushroom_block_skin_red",100,14
        "mushroom_block_skin_stem",99,15  // TODO
        "mycelium_side",110,0
        "netherrack",87,0
        "nether_brick",112,0
        "noteblock",25,0
        "obsidian",49,0
        "piston_bottom",33,2  // TODO
        "piston_side",33,0
        "piston_top_normal",33,3 // TODO
        "piston_top_sticky",29,3 // TODO
        "planks_acacia",5,4
        "planks_big_oak",5,5
        "planks_birch",5,2
        "planks_jungle",5,3
        "planks_oak",5,0
        "planks_spruce",5,1
        "prismarine",168,0
        "prismarine_bricks",168,1
        "prismarine_dark",168,2
        "pumpkin_face_off",86,0
        "pumpkin_face_on",91,0
        "pumpkin_side",86,1
        "quartz_block_chiseled",155,1
        "quartz_block_lines",155,2
        "quartz_block_side",155,3
        "quartz_ore",153,0
        "redstone_block",152,0
        "redstone_lamp_off",123,0
        "redstone_lamp_on",124,0
        "redstone_ore",73,0
        "red_sand",12,1
        "sand",12,0
        "sandstone_carved",24,1
        "sandstone_normal",24,0
        "sandstone_smooth",24,2
        "slime",165,0
        "snow",80,0
        "soul_sand",88,0
        "sponge",19,0
        "stone",1,0
        "stonebrick",98,0
        "stonebrick_carved",98,3
        "stonebrick_cracked",98,2
        "stonebrick_mossy",98,1
        "stone_andesite",1,5
        "stone_andesite_smooth",1,6
        "stone_diorite",1,3
        "stone_diorite_smooth",1,4
        "stone_granite",1,1
        "stone_granite_smooth",1,2
        "stone_slab_side",43,0
        "stone_slab_top",43,8
        "tnt_side",46,0
        "wool_colored_black",35,15
        "wool_colored_blue",35,11
        "wool_colored_brown",35,12
        "wool_colored_cyan",35,9
        "wool_colored_gray",35,7
        "wool_colored_green",35,13
        "wool_colored_light_blue",35,3
        "wool_colored_lime",35,5
        "wool_colored_magenta",35,2
        "wool_colored_orange",35,1
        "wool_colored_pink",35,6
        "wool_colored_purple",35,10
        "wool_colored_red",35,14
        "wool_colored_silver",35,8
        "wool_colored_white",35,0
        "wool_colored_yellow",35,4
        "000124000",18,4
        "000217058",133,0
        "025025025",35,15
        "051076178",35,11
        "074128255",22,0
        "076076076",35,7
        "076127153",35,9
        "092219213",57,0
        "102076051",35,12
        "102127051",35,13
        "102153216",35,3
        "104083050",5,0
        "112002000",87,0
        "112112112",1,0
        "127063178",35,10
        "127178056",2,0
        "127204025",35,5
        "153051051",35,14
        "153153153",35,8
        "160160255",174,0
        "164168184",82,0
        "178076216",35,2
        "183106047",3,0
        "216127051",35,1
        "229229051",35,4
        "242127165",35,6
        "247233163",24,0
        "250238077",41,0
        "255000000",46,0
        "255252245",35,0
    |]

let textureFilenamesToBlockIDandDataMappingForHeads =
    [|
        "bedrock",7,0
        "bookshelf",47,0
        "brick",45,0
        "clay",82,0
        "coal_block",173,0
        "coal_ore",16,0
        "cobblestone",4,0
        "cobblestone_mossy",48,0
        "command_block",137,0
        "crafting_table_front",58,0
        "crafting_table_side",58,0  // TODO
        "diamond_block",57,0
        "diamond_ore",56,0
        "dirt",3,0
        "dirt_podzol_side",3,2
        "dispenser_front_horizontal",23,3  // TODO
        "dispenser_front_vertical",23,3  // TODO
        "dropper_front_horizontal",158,3  // TODO
        "dropper_front_vertical",158,3  // TODO
        "emerald_block",133,0
        "emerald_ore",129,0
        "end_stone",121,0
        "furnace_front_off",61,0
        "furnace_front_on",62,0
        "furnace_side",61,0 // TODO
        "glowstone",89,0
        "gold_block",41,0
        "gold_ore",14,0
        "gravel",13,0
        "hardened_clay",172,0
        "hardened_clay_stained_black",159,15
        "hardened_clay_stained_blue",159,11
        "hardened_clay_stained_brown",159,12
        "hardened_clay_stained_cyan",159,9
        "hardened_clay_stained_gray",159,7
        "hardened_clay_stained_green",159,13
        "hardened_clay_stained_light_blue",159,3
        "hardened_clay_stained_lime",159,5
        "hardened_clay_stained_magenta",159,2
        "hardened_clay_stained_orange",159,1
        "hardened_clay_stained_pink",159,6
        "hardened_clay_stained_purple",159,10
        "hardened_clay_stained_red",159,14
        "hardened_clay_stained_silver",159,8
        "hardened_clay_stained_white",159,0
        "hardened_clay_stained_yellow",159,4
        "hay_block_side",170,0
        "hay_block_top",170,0  // TODO
        "ice",79,0
        "ice_packed",174,0
        "iron_block",42,0
        "iron_ore",15,0
        "jukebox_side",84,0
        "lapis_block",22,0
        "lapis_ore",21,0
        "log_acacia",162,0
        "log_acacia_top",162,8
        "log_big_oak",162,1
        "log_big_oak_top",162,9
        "log_birch",17,2
        "log_birch_top",17,10
        "log_jungle",17,3
        "log_jungle_top",17,11
        "log_oak",17,0
        "log_oak_top",17,8
        "log_spruce",17,1
        "log_spruce_top",17,9
        "melon_side",103,0  
        "melon_top",103,0  // TODO
        "mushroom_block_inside",99,0  // TODO
        "mushroom_block_skin_brown",99,0
        "mushroom_block_skin_red",100,0
        "mushroom_block_skin_stem",99,0  // TODO
        "mycelium_side",110,0
        "netherrack",87,0
        "nether_brick",112,0
        "noteblock",25,0
        "obsidian",49,0
        "piston_bottom",33,0  // TODO
        "piston_side",33,0
        "piston_top_normal",33,0 // TODO
        "piston_top_sticky",29,0 // TODO
        "planks_acacia",5,4
        "planks_big_oak",5,5
        "planks_birch",5,2
        "planks_jungle",5,3
        "planks_oak",5,0
        "planks_spruce",5,1
        "pumpkin_face_off",86,0
        "pumpkin_face_on",91,0
        "pumpkin_side",86,1
        "quartz_block_chiseled",155,1
        "quartz_block_lines",155,2
        "quartz_block_side",155,3
        "quartz_ore",153,0
        "redstone_block",152,0
        "redstone_lamp_off",123,0
        "redstone_lamp_on",124,0
        "redstone_ore",73,0
        "red_sand",12,1
        "sand",12,0
        "sandstone_carved",24,1
        "sandstone_normal",24,0
        "sandstone_smooth",24,2
        "slime",165,0
        "snow",80,0
        "soul_sand",88,0
        "sponge",19,0
        "stone",1,0
        "stonebrick",98,0
        "stonebrick_carved",98,3
        "stonebrick_cracked",98,2
        "stonebrick_mossy",98,1
        "stone_andesite",1,5
        "stone_andesite_smooth",1,6
        "stone_diorite",1,3
        "stone_diorite_smooth",1,4
        "stone_granite",1,1
        "stone_granite_smooth",1,2
        "stone_slab_side",43,0
        "stone_slab_top",43,8
        "wool_colored_black",35,15
        "wool_colored_blue",35,11
        "wool_colored_brown",35,12
        "wool_colored_cyan",35,9
        "wool_colored_gray",35,7
        "wool_colored_green",35,13
        "wool_colored_light_blue",35,3
        "wool_colored_lime",35,5
        "wool_colored_magenta",35,2
        "wool_colored_orange",35,1
        "wool_colored_pink",35,6
        "wool_colored_purple",35,10
        "wool_colored_red",35,14
        "wool_colored_silver",35,8
        "wool_colored_white",35,0
        "wool_colored_yellow",35,4
    |]


///////////////////////////////////////

// blockIdToMinecraftName for first 255 to get MC name
let survivalObtainableItems =
    [|
        1,0,"stone"
        1,1,"granite"
        1,2,"smooth granite"
        1,3,"diorite"
        1,4,"smooth diorite"
        1,5,"andesite"
        1,6,"smooth andesite"
        2,0,"grass"
        3,0,"dirt"
        3,1,"coarse dirt"
        3,2,"podzol"
        4,0,"cobblestone"
        5,0,"oak      planks"
        5,1,"spruce   planks"
        5,2,"birch    planks"
        5,3,"jungle   planks"
        5,4,"acacia   planks"
        5,5,"dark oak planks"
        6,0,"oak      sapling"
        6,1,"spruce   sapling"
        6,2,"birch    sapling"
        6,3,"jungle   sapling"
        6,4,"acacia   sapling"
        6,5,"dark oak sapling"
        12,0,"sand"
        12,1,"red sand"
        13,0,"gravel"
        14,0,"gold ore"
        15,0,"iron ore"
        16,0,"coal ore"
        17,0,"oak      log"
        17,1,"spruce   log"
        17,2,"birch    log"
        17,3,"jungle   log"
        18,0,"oak      leaves"
        18,1,"spruce   leaves"
        18,2,"birch    leaves"
        18,3,"jungle   leaves"
        19,0,"sponge"
        19,1,"wet sponge"
        20,0,"glass"
        21,0,"lapis ore"
        22,0,"lapis block"
        23,0,"dispenser"
        24,0,"sandstone"
        24,1,"chiseled sandstone"
        24,2,"smooth sandstone"
        25,0,"note block"
        27,0,"powered rail"
        28,0,"detector rail"
        29,0,"sticky piston"
        30,0,"web"
        31,1,"tall grass"
        31,2,"fern"
        32,0,"dead bush"
        33,0,"piston"
        35, 0,"white      wool"
        35, 1,"orange     wool"
        35, 2,"magenta    wool"
        35, 3,"light blue wool"
        35, 4,"yellow     wool"
        35, 5,"lime       wool"
        35, 6,"pink       wool"
        35, 7,"light gray wool"
        35, 8,"dark gray  wool"
        35, 9,"cyan       wool"
        35,10,"purple     wool"
        35,11,"blue       wool"
        35,12,"brown      wool"
        35,13,"green      wool"
        35,14,"red        wool"
        35,15,"black      wool"
        37,0,"dandelion"
        38,0,"poppy"
        38,1,"blue orchid"
        38,2,"allium"
        38,3,"azure bluet"
        38,4,"red tulip"
        38,5,"orange tulip"
        38,6,"pink tulip"
        38,7,"white tulip"
        38,8,"oxeye daisy"
        39,0,"brown mushroom"
        40,0,"red mushroom"
        41,0,"gold block"
        42,0,"iron block"
        44,0,"stone slab"
        44,1,"sandstone slab"
        44,3,"cobblestone slab"
        44,4,"brick slab"
        44,5,"stone brick slab"
        44,6,"nether brick slab"
        44,7,"quartz slab"
        45,0,"brick block"
        46,0,"tnt"
        47,0,"bookshelf"
        48,0,"moss stone"
        49,0,"obsidian"
        50,0,"torch"
        53,0,"oak_stairs"
        54,0,"chest"
        56,0,"diamond ore"
        57,0,"diamond block"
        58,0,"crafting table"
        61,0,"furnace"
        65,0,"ladder"
        66,0,"rail"
        67,0,"cobblestone stairs"
        69,0,"lever"
        70,0,"stone pressure plate"
        72,0,"wooden pressure plate"
        73,0,"redstone ore"
        76,0,"redstone torch"
        77,0,"stone button"
        78,0,"snow layer"
        79,0,"ice"
        80,0,"snow block"
        81,0,"cactus"
        82,0,"clay block"
        84,0,"jukebox"
        85,0,"wooden fence"
        86,0,"pumpkin"
        87,0,"netherrack"
        88,0,"soul sand"
        89,0,"glowstone"
        91,0,"jack o' lantern"
        95, 0,"white      stained glass"
        95, 1,"orange     stained glass"
        95, 2,"magenta    stained glass"
        95, 3,"light blue stained glass"
        95, 4,"yellow     stained glass"
        95, 5,"lime       stained glass"
        95, 6,"pink       stained glass"
        95, 7,"light gray stained glass"
        95, 8,"dark gray  stained glass"
        95, 9,"cyan       stained glass"
        95,10,"purple     stained glass"
        95,11,"blue       stained glass"
        95,12,"brown      stained glass"
        95,13,"green      stained glass"
        95,14,"red        stained glass"
        95,15,"black      stained glass"
        96,0,"trapdoor"
        98,0,"stone brick"
        98,1,"mossy stone brick"
        98,2,"cracked stone brick"
        98,3,"chiseled stone brick"
        99,0,"brown mushroom block"
        100,0,"red mushroom block"
        101,0,"iron bars"
        102,0,"glass pane"
        103,0,"melon block"
        106,0,"vine"
        107,0,"fence gate"
        108,0,"brick stairs"
        109,0,"stone brick stairs"
        110,0,"mycelium"
        111,0,"lily pad"
        112,0,"nether brick block"
        113,0,"nether brick fence"
        114,0,"nether brick stairs"
        116,0,"enchanting table"
        121,0,"end stone"
        122,0,"dragon egg"
        123,0,"redstone lamp"
        126,0,"oak      wooden slab"
        126,1,"spruce   wooden slab"
        126,2,"birch    wooden slab"
        126,3,"jungle   wooden slab"
        126,4,"acacia   wooden slab"
        126,5,"dark oak wooden slab"
        128,0,"sandstone stairs"
        129,0,"emerald ore"
        130,0,"ender chest"
        131,0,"tripwire hook"
        133,0,"emerald block"
        134,0,"spruce stairs"
        135,0,"birch stairs"
        136,0,"jungle stairs"
        138,0,"beacon"
        139,0,"cobblestone wall"
        139,1,"mossy cobblestone wall"
        143,0,"wooden button"
        145,0,"anvil"
        145,1,"slightly damaged anvil"
        145,2,"very damaged anvil"
        146,0,"trapped chest"
        147,0,"gold pressure plate"
        148,0,"iron pressure plate"
        151,0,"daylight sensor"
        152,0,"redstone block"
        153,0,"quartz ore"
        154,0,"hopper"
        155,0,"quartz block"
        155,1,"chiseled quartz block"
        155,2,"pillar quartz block"
        156,0,"quartz stairs"
        157,0,"activator rail"
        158,0,"dropper"
        159, 0,"white      stained hardened clay"
        159, 1,"orange     stained hardened clay"
        159, 2,"magenta    stained hardened clay"
        159, 3,"light blue stained hardened clay"
        159, 4,"yellow     stained hardened clay"
        159, 5,"lime       stained hardened clay"
        159, 6,"pink       stained hardened clay"
        159, 7,"light gray stained hardened clay"
        159, 8,"dark gray  stained hardened clay"
        159, 9,"cyan       stained hardened clay"
        159,10,"purple     stained hardened clay"
        159,11,"blue       stained hardened clay"
        159,12,"brown      stained hardened clay"
        159,13,"green      stained hardened clay"
        159,14,"red        stained hardened clay"
        159,15,"black      stained hardened clay"
        160, 0,"white      stained glass pane"
        160, 1,"orange     stained glass pane"
        160, 2,"magenta    stained glass pane"
        160, 3,"light blue stained glass pane"
        160, 4,"yellow     stained glass pane"
        160, 5,"lime       stained glass pane"
        160, 6,"pink       stained glass pane"
        160, 7,"light gray stained glass pane"
        160, 8,"dark gray  stained glass pane"
        160, 9,"cyan       stained glass pane"
        160,10,"purple     stained glass pane"
        160,11,"blue       stained glass pane"
        160,12,"brown      stained glass pane"
        160,13,"green      stained glass pane"
        160,14,"red        stained glass pane"
        160,15,"black      stained glass pane"
        161,0,"acacia leaves"
        161,1,"dark oak leaves"
        162,0,"acacia log"
        162,1,"dark oak log"
        163,0,"acacia stairs"
        164,0,"dark oak stairs"
        165,0,"slime block"
        167,0,"iron trapdoor"
        168,0,"prismarine"
        168,1,"prismarine brick"
        168,2,"dark prismarine"
        169,0,"sea lantern"
        170,0,"hay block"
        171, 0,"white      carpet"
        171, 1,"orange     carpet"
        171, 2,"magenta    carpet"
        171, 3,"light blue carpet"
        171, 4,"yellow     carpet"
        171, 5,"lime       carpet"
        171, 6,"pink       carpet"
        171, 7,"light gray carpet"
        171, 8,"dark gray  carpet"
        171, 9,"cyan       carpet"
        171,10,"purple     carpet"
        171,11,"blue       carpet"
        171,12,"brown      carpet"
        171,13,"green      carpet"
        171,14,"red        carpet"
        171,15,"black      carpet"
        172,0,"hardened clay"
        173,0,"coal block"
        174,0,"packed ice"
        175,0,"sunflower"
        175,1,"lilac"
        175,4,"rose bush"
        175,5,"peony"
        179,0,"red sandstone"
        180,0,"red sandstone stairs"
        182,0,"red sandstone slab"
        183,0,"spruce fence gate"
        184,0,"birch fence gate"
        185,0,"jungle fence gate"
        186,0,"dark oak fence gate"
        187,0,"acacia fence gate"
        188,0,"spruce fence"
        189,0,"birch fence"
        190,0,"jungle fence"
        191,0,"dark oak fence"
        192,0,"acacia fence"
        198,0,"end rod"
        200,0,"chorus flower"
        201,0,"purpur block"
        202,0,"purpur pillar"
        203,0,"purpur stairs"
        205,0,"purpur slab"
        206,0,"end stone bricks"
        // end of blocks, on to items...
        256,0,"iron_shovel"
        257,0,"iron_pickaxe"
        258,0,"iron_axe"
        259,0,"flint_and_steel"
        260,0,"apple"
        261,0,"bow"
        262,0,"arrow"
        263,0,"coal"
        263,1,"coal" // charcoal
        264,0,"diamond"
        265,0,"iron_ingot"
        266,0,"gold_ingot"
        267,0,"iron_sword"
        268,0,"wooden_sword"
        269,0,"wooden_shovel"
        270,0,"wooden_pickaxe"
        271,0,"wooden_axe"
        272,0,"stone_sword"
        273,0,"stone_shovel"
        274,0,"stone_pickaxe"
        275,0,"stone_axe"
        276,0,"diamond_sword"
        277,0,"diamond_shovel"
        278,0,"diamond_pickaxe"
        279,0,"diamond_axe"
        280,0,"stick"
        281,0,"bowl"
        282,0,"mushroom_stew"
        283,0,"golden_sword"
        284,0,"golden_shovel"
        285,0,"golden_pickaxe"
        286,0,"golden_axe"
        287,0,"string"
        288,0,"feather"
        289,0,"gunpowder"
        290,0,"wooden_hoe"
        291,0,"stone_hoe"
        292,0,"iron_hoe"
        293,0,"diamond_hoe"
        294,0,"golden_hoe"
        295,0,"wheat_seeds"
        296,0,"wheat"
        297,0,"bread"
        298,0,"leather_helmet"  // millions of dye colors, ignore
        299,0,"leather_chestplate"
        300,0,"leather_leggings"
        301,0,"leather_boots"
        302,0,"chainmail_helmet"
        303,0,"chainmail_chestplate"
        304,0,"chainmail_leggings"
        305,0,"chainmail_boots"
        306,0,"iron_helmet"
        307,0,"iron_chestplate"
        308,0,"iron_leggings"
        309,0,"iron_boots"
        310,0,"diamond_helmet"
        311,0,"diamond_chestplate"
        312,0,"diamond_leggings"
        313,0,"diamond_boots"
        314,0,"golden_helmet"
        315,0,"golden_chestplate"
        316,0,"golden_leggings"
        317,0,"golden_boots"
        318,0,"flint"
        319,0,"porkchop"
        320,0,"cooked_porkchop"
        321,0,"painting"
        322,0,"golden_apple"
        322,1,"golden_apple"  // enchanted
        323,0,"sign"
        324,0,"wooden_door"
        325,0,"bucket"
        326,0,"water_bucket"
        327,0,"lava_bucket"
        328,0,"minecart"
        329,0,"saddle"
        330,0,"iron_door"
        331,0,"redstone"
        332,0,"snowball"
        333,0,"boat"
        334,0,"leather"
        335,0,"milk_bucket"
        336,0,"brick"
        337,0,"clay_ball"
        338,0,"reeds"  // sugar cane
        339,0,"paper"
        340,0,"book"
        341,0,"slime_ball"
        342,0,"chest_minecart"
        343,0,"furnace_minecart"
        344,0,"egg"
        345,0,"compass"
        346,0,"fishing_rod"
        347,0,"clock"
        348,0,"glowstone_dust"
        349,0,"fish"
        349,1,"fish" // salmon
        349,2,"fish" // clownfish
        349,3,"fish" // pufferfish
        350,0,"cooked_fish"
        350,1,"cooked_fish" // salmon
        351,0,"dye" // nicer names (backwards color order)
        351,1,"dye"
        351,2,"dye"
        351,3,"dye"
        351,4,"dye"
        351,5,"dye"
        351,6,"dye"
        351,7,"dye"
        351,8,"dye"
        351,9,"dye"
        351,10,"dye"
        351,11,"dye"
        351,12,"dye"
        351,13,"dye"
        351,14,"dye"
        351,15,"dye"
        352,0,"bone"
        353,0,"sugar"
        354,0,"cake"
        355,0,"bed"
        356,0,"repeater"
        357,0,"cookie"
        358,0,"filled_map"
        359,0,"shears"
        360,0,"melon"
        361,0,"pumpkin_seeds"
        362,0,"melon_seeds"
        363,0,"beef"
        364,0,"cooked_beef"
        365,0,"chicken"
        366,0,"cooked_chicken"
        367,0,"rotten_flesh"
        368,0,"ender_pearl"
        369,0,"blaze_rod"
        370,0,"ghast_tear"
        371,0,"gold_nugget"
        372,0,"nether_wart"
        373,0,"potion"       // TODO 30+ obtainable versions
        374,0,"glass_bottle"
        375,0,"spider_eye"
        376,0,"fermented_spider_eye"
        377,0,"blaze_powder"
        378,0,"magma_cream"
        379,0,"brewing_stand"
        380,0,"cauldron"
        381,0,"ender_eye"
        382,0,"speckled_melon"
        384,0,"experience_bottle"
        385,0,"fire_charge"
        386,0,"writable_book"
        387,0,"written_book"
        388,0,"emerald"
        389,0,"item_frame"
        390,0,"flower_pot"
        391,0,"carrot"
        392,0,"potato"
        393,0,"baked_potato"
        394,0,"poisonous_potato"
        395,0,"map"
        396,0,"golden_carrot"
        397,0,"skull" // skeleton head
        397,1,"skull" // wither skeleton head
        397,2,"skull" // zombie head
        397,4,"skull" // creeper head
        397,5,"skull" // dragon head
        398,0,"carrot_on_a_stick"
        399,0,"nether_star"
        400,0,"pumpkin_pie"
        401,0,"fireworks" // TODO tons
        402,0,"firework_charge" // TODO tons
        403,0,"enchanted_book" // TODO 26 variations
        404,0,"comparator"
        405,0,"netherbrick"
        406,0,"quartz"
        407,0,"tnt_minecart"
        408,0,"hopper_minecart"
        409,0,"prismarine_shard"
        410,0,"prismarine_crystals"
        411,0,"rabbit"
        412,0,"cooked_rabbit"
        413,0,"rabbit_stew"
        414,0,"rabbit_foot"
        415,0,"rabbit_hide"
        416,0,"armor_stand"
        417,0,"iron_horse_armor"
        418,0,"golden_horse_armor"
        419,0,"diamond_horse_armor"
        420,0,"lead"
        421,0,"name_tag"
        423,0,"mutton"
        424,0,"cooked_mutton"
        425,0,"banner" // TODO millions of kinds
        426,0,"end_crystal"
        427,0,"spruce_door"
        428,0,"birch_door"
        429,0,"jungle_door"
        430,0,"acacia_door"
        431,0,"dark_oak_door"
        432,0,"chorus_fruit"
        433,0,"chorus_fruit_popped"
        434,0,"beetroot"
        435,0,"beetroot_seeds"
        436,0,"beetroot_soup"
        437,0,"dragon_breath"
        438,0,"splash_potion" // TODO 30+ kinds
        439,0,"spectral_arrow"
        440,0,"tipped_arrow" // TODO 30+ kinds
        441,0,"lingering_potion" // TODO 30+ kinds
        442,0,"shield" // TODO millions of banners to add
        443,0,"elytra"
        444,0,"spruce_boat"
        445,0,"birch_boat"
        446,0,"jungle_boat"
        447,0,"acacia_boat"
        448,0,"dark_oak_boat"
        2256,0,"record_13"
        2257,0,"record_cat"
        2258,0,"record_blocks"
        2259,0,"record_chirp"
        2260,0,"record_far"
        2261,0,"record_mall"
        2262,0,"record_mellohi"
        2263,0,"record_stal"
        2264,0,"record_strad"
        2265,0,"record_ward"
        2266,0,"record_11"
        2267,0,"record_wait"
    |]

///////////////////////////////////////


// level.dat's "generatorOptions" (for {generatorName : customized})
let DTInormal    = """{"coordinateScale":676.94366,"heightScale":676.94366,"lowerLimitScale":458.6549,"upperLimitScale":845.90137,"depthNoiseScaleX":240.31691,"depthNoiseScaleZ":240.31691,"depthNoiseScaleExponent":1.5585212,"mainNoiseScaleX":106.61267,"mainNoiseScaleY":141.8169,"mainNoiseScaleZ":106.61267,"baseSize":8.5,"stretchY":8.459015,"biomeDepthWeight":1.0,"biomeDepthOffset":0.0,"biomeScaleWeight":1.0,"biomeScaleOffset":0.0,"seaLevel":63,"useCaves":true,"useDungeons":true,"dungeonChance":8,"useStrongholds":true,"useVillages":true,"useMineShafts":true,"useTemples":true,"useMonuments":true,"useRavines":true,"useWaterLakes":true,"waterLakeChance":4,"useLavaLakes":true,"lavaLakeChance":80,"useLavaOceans":false,"fixedBiome":-1,"biomeSize":4,"riverSize":4,"dirtSize":33,"dirtCount":10,"dirtMinHeight":0,"dirtMaxHeight":256,"gravelSize":33,"gravelCount":8,"gravelMinHeight":0,"gravelMaxHeight":256,"graniteSize":33,"graniteCount":10,"graniteMinHeight":0,"graniteMaxHeight":80,"dioriteSize":33,"dioriteCount":10,"dioriteMinHeight":0,"dioriteMaxHeight":80,"andesiteSize":33,"andesiteCount":10,"andesiteMinHeight":0,"andesiteMaxHeight":80,"coalSize":17,"coalCount":20,"coalMinHeight":0,"coalMaxHeight":128,"ironSize":9,"ironCount":20,"ironMinHeight":0,"ironMaxHeight":64,"goldSize":9,"goldCount":2,"goldMinHeight":0,"goldMaxHeight":32,"redstoneSize":8,"redstoneCount":8,"redstoneMinHeight":0,"redstoneMaxHeight":16,"diamondSize":8,"diamondCount":1,"diamondMinHeight":0,"diamondMaxHeight":16,"lapisSize":7,"lapisCount":1,"lapisCenterHeight":16,"lapisSpread":16}"""
let defaultWorld = """{"coordinateScale":684.412,"heightScale":684.412,"lowerLimitScale":512.0,"upperLimitScale":512.0,"depthNoiseScaleX":200.0,"depthNoiseScaleZ":200.0,"depthNoiseScaleExponent":0.5,"mainNoiseScaleX":80.0,"mainNoiseScaleY":160.0,"mainNoiseScaleZ":80.0,"baseSize":8.5,"stretchY":12.0,"biomeDepthWeight":1.0,"biomeDepthOffset":0.0,"biomeScaleWeight":1.0,"biomeScaleOffset":0.0,"seaLevel":63,"useCaves":true,"useDungeons":true,"dungeonChance":8,"useStrongholds":true,"useVillages":true,"useMineShafts":true,"useTemples":true,"useMonuments":true,"useRavines":true,"useWaterLakes":true,"waterLakeChance":4,"useLavaLakes":true,"lavaLakeChance":80,"useLavaOceans":false,"fixedBiome":-1,"biomeSize":4,"riverSize":4,"dirtSize":33,"dirtCount":10,"dirtMinHeight":0,"dirtMaxHeight":256,"gravelSize":33,"gravelCount":8,"gravelMinHeight":0,"gravelMaxHeight":256,"graniteSize":33,"graniteCount":10,"graniteMinHeight":0,"graniteMaxHeight":80,"dioriteSize":33,"dioriteCount":10,"dioriteMinHeight":0,"dioriteMaxHeight":80,"andesiteSize":33,"andesiteCount":10,"andesiteMinHeight":0,"andesiteMaxHeight":80,"coalSize":17,"coalCount":20,"coalMinHeight":0,"coalMaxHeight":128,"ironSize":9,"ironCount":20,"ironMinHeight":0,"ironMaxHeight":64,"goldSize":9,"goldCount":2,"goldMinHeight":0,"goldMaxHeight":32,"redstoneSize":8,"redstoneCount":8,"redstoneMinHeight":0,"redstoneMaxHeight":16,"diamondSize":8,"diamondCount":1,"diamondMinHeight":0,"diamondMaxHeight":16,"lapisSize":7,"lapisCount":1,"lapisCenterHeight":16,"lapisSpread":16}"""

let oreSpawnDefaults =
    [|
        // block, Size, Count, MinHeight, MaxHeight
        "dirt",     33, 10, 0, 256
        "gravel",   33,  8, 0, 256
        "granite",  33, 10, 0,  80
        "diorite",  33, 10, 0,  80
        "andesite", 33, 10, 0,  80
        "coal",     17, 20, 0, 128
        "iron",      9, 20, 0,  64
        "gold",      9,  2, 0,  32
        "redstone",  8,  8, 0,  16
        "diamond",   8,  1, 0,  16
    |]
let oreSpawnBingo =
    [|
        // block, Size, Count, MinHeight, MaxHeight
        "dirt",     33, 10, 0, 256
        "gravel",   33,  8, 0, 256
        "granite",  33,  0, 0,  80
        "diorite",  33,  0, 0,  80
        "andesite", 33,  0, 0,  80
        "coal",     17, 20, 0, 128
        "iron",      9, 20, 0,  64
        "gold",      9,  2, 0,  32
        "redstone",  8,  8, 0,  16
        "diamond",   8,  1, 0,  16
    |]
let defaultWorldWithCustomOreSpawns(biomeSize,dungeonChance,waterLakeRarity,lavaLakeRarity,   // defaults: 4, 8, 80, 4
                                    useStrongholds,useVillages,useTemples,useMonuments,       // defaults: all true
                                    oreSpawnCustom) =                                         // defaults: "oreSpawnDefaults" above
    let TF b = if b then "true" else "false"
    let part1 : string = sprintf """{"coordinateScale":684.412,"heightScale":684.412,"lowerLimitScale":512.0,"upperLimitScale":512.0,"depthNoiseScaleX":200.0,"depthNoiseScaleZ":200.0,"depthNoiseScaleExponent":0.5,"mainNoiseScaleX":80.0,"mainNoiseScaleY":160.0,"mainNoiseScaleZ":80.0,"baseSize":8.5,"stretchY":12.0,"biomeDepthWeight":1.0,"biomeDepthOffset":0.0,"biomeScaleWeight":1.0,"biomeScaleOffset":0.0,"seaLevel":63,"useCaves":true,"useDungeons":true,"dungeonChance":%d,"useStrongholds":%s,"useVillages":%s,"useMineShafts":true,"useTemples":%s,"useMonuments":%s,"useRavines":true,"useWaterLakes":true,"waterLakeChance":%d,"useLavaLakes":true,"lavaLakeChance":%d,"useLavaOceans":false,"fixedBiome":-1,"biomeSize":%d,"riverSize":4,""" dungeonChance (TF useStrongholds) (TF useVillages) (TF useTemples) (TF useMonuments) waterLakeRarity lavaLakeRarity biomeSize
    let part4 =
        let sb = new System.Text.StringBuilder()
        for kind, size, count, min, max in oreSpawnCustom do
            sb.Append(sprintf """"%sSize":%d,"%sCount":%d,"%sMinHeight":%d,"%sMaxHeight":%d,""" kind size kind count kind min kind max) |> ignore
        sb.ToString()
    let part5 = """"lapisSize":7,"lapisCount":1,"lapisCenterHeight":16,"lapisSpread":16}"""
    part1+part4+part5
