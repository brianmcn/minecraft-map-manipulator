module MC_Constants

let BIOMES = 
    [|0,"Ocean";1,"Plains";2,"Desert";3,"Extreme Hills";4,"Forest";5,"Taiga";6,"Swampland";7,"River";8,"Hell";9,"Sky";
      10,"Frozen Ocean";11,"FrozenRiver";12,"Ice Plains";13,"Ice Mountains";14,"MushroomIsland";15,"MushroomIslandShore";16,"Beach";17,"DesertHills";18,"ForestHills";19,"TaigaHills";
      20,"Extreme Hills Edge";21,"Jungle";22,"Jungle Hills";23,"Jungle Edge";24,"Deep Ocean";25,"Stone Beach";26,"Cold Beach";27,"Birch Forest";28,"Birch Forest Hills";29,"Roofed Forest";
      30,"Cold Taiga";31,"Cold Taiga Hills";32,"Mega Taiga";33,"Mega Taiga Hills";34,"Extreme Hills+";35,"Savanna";36,"Savanna Plateau";37,"Mesa";38,"Mesa Plateau F";39,"Mesa Plateau";
      127,"The Void";
      128,"Plains M";129,"Sunflower Plains";
      130,"Desert M";131,"Extreme Hills M";132,"Flower Forest";133,"Taiga M";134,"Swampland M";
      140,"Ice Plains Spikes";141,"Ice Mountains Spikes";149,"Jungle M";
      151,"JungleEdge M";155,"Birch Forest M";156,"Birch Forest Hills M";157,"Roofed Forest M";158,"Cold Taiga M";
      160,"Mega Spruce Taiga";161,"Mega Spruce Taiga";162,"Extreme Hills+ M";163,"Savanna M";164,"Savanna Plateau M";165,"Mesa (Bryce)";166,"Mesa Plateau F M";167,"Mesa Plateau M";
      -1,"(Uncalculated)"|]

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
      20,"Wither";21,"Health Boost";22,"Absorption";23,"Saturation"|]

let WOOL_COLORS = [|0,"White";1,"Orange";2,"Magenta";3,"Light Blue";4,"Yellow";5,"Lime";6,"Pink";7,"Gray";8,"Light Gray";9,"Cyan";10,"Purple";11,"Blue";12,"Brown";13,"Green";14,"Red";15,"Black"|]

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
        175,"minecraft:large_flowers"
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
    |]

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

