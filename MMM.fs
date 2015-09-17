module AA = ArtAssets

let BIOMES = 
    [|0,"Ocean";1,"Plains";2,"Desert";3,"Extreme Hills";4,"Forest";5,"Taiga";6,"Swampland";7,"River";8,"Hell";9,"Sky";
      10,"Frozen Ocean";11,"FrozenRiver";12,"Ice Plains";13,"Ice Mountains";14,"MushroomIsland";15,"MushroomIslandShore";16,"Beach";17,"DesertHills";18,"ForestHills";19,"TaigaHills";
      20,"Extreme Hills Edge";21,"Jungle";22,"Jungle Hills";23,"Jungle Edge";24,"Deep Ocean";25,"Stone Beach";26,"Cold Beach";27,"Birch Forest";28,"Birch Forest Hills";29,"Roofed Forest";
      30,"Cold Taiga";31,"Cold Taiga Hills";32,"Mega Taiga";33,"Mega Taiga Hills";34,"Extreme Hills+";35,"Savanna";36,"Savanna Plateau";37,"Mesa";38,"Mesa Plateau F";39,"Mesa Plateau";
      129,"Sunflower Plains";
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
    [|0,"Protection";1,"Fire Protection";2,"Feather Falling";3,"Blast Protection";4,"Projectile Protection";5,"Respiration";6,"Aqua Affinity";7,"Thorns";
      16,"Sharpness";17,"Smite";18,"Bane of Arthropods";19,"Knockback";20,"Fire Aspect";21,"Looting";
      32,"Efficiency";33,"Silk Touch";34,"Unbreaking";35,"Fortune";
      48,"Power";49,"Punch";50,"Flame";51,"Infinity";
      61,"Luck of the Sea";62,"Lure"|]

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

let rand = new System.Random()
let swap (a: _[]) x y =
    let tmp = a.[x]
    a.[x] <- a.[y]
    a.[y] <- tmp
let shuffle a = Array.iteri (fun i _ -> swap a i (rand.Next(i, Array.length a))) a


type BinaryReader2(stream : System.IO.Stream) = // Big Endian
    inherit System.IO.BinaryReader(stream)
    override __.ReadInt32() = 
        let a32 = base.ReadBytes(4)
        System.Array.Reverse(a32)
        System.BitConverter.ToInt32(a32,0)
    override __.ReadInt16() = 
        let a16 = base.ReadBytes(2)
        System.Array.Reverse(a16)
        System.BitConverter.ToInt16(a16,0)
    override __.ReadInt64() = 
        let a64 = base.ReadBytes(8)
        System.Array.Reverse(a64)
        System.BitConverter.ToInt64(a64,0)
    override __.ReadDouble() = 
        let a64 = base.ReadBytes(8)
        System.Array.Reverse(a64)
        System.BitConverter.ToDouble(a64,0)
    override __.ReadUInt32() = 
        let a32 = base.ReadBytes(4)
        System.Array.Reverse(a32)
        System.BitConverter.ToUInt32(a32,0)

type BinaryWriter2(stream : System.IO.Stream) = // Big Endian
    inherit System.IO.BinaryWriter(stream)
    override __.Write(x:int) = 
        let a = System.BitConverter.GetBytes(x)
        System.Array.Reverse(a)
        base.Write(a)
    override __.Write(x:int16) = 
        let a = System.BitConverter.GetBytes(x)
        System.Array.Reverse(a)
        base.Write(a)
    override __.Write(x:int64) = 
        let a = System.BitConverter.GetBytes(x)
        System.Array.Reverse(a)
        base.Write(a)
    override __.Write(x:double) = 
        let a = System.BitConverter.GetBytes(x)
        System.Array.Reverse(a)
        base.Write(a)
    override __.Write(x:uint32) = 
        let a = System.BitConverter.GetBytes(x)
        System.Array.Reverse(a)
        base.Write(a)

let END_NAME = "--End--"

type Name = string

type Payload =
    // | Ends
    | Bytes of byte[]
    | Shorts of int16[]
    | Ints of int[]
    | Longs of int64[]
    | Floats of single[]
    | Doubles of double[]
//    | ByteArrays of byte[][]
    | Strings of string[]
    //| Lists
    | Compounds of NBT[][]
    | IntArrays of int[][]
    
and NBT =
    | End
    | Byte of Name * byte
    | Short of Name * int16
    | Int of Name * int
    | Long of Name * int64
    | Float of Name * single
    | Double of Name * double
    | ByteArray of Name * byte[]
    | String of Name * string // int16 length beforehand
    | List of Name * Payload // (name,kind,num,a)
    | Compound of Name * NBT[]
    | IntArray of Name * int[] // int32 before data shows num elements
    member this.Name =
        match this with
        | End -> END_NAME
        | Byte(n,_) -> n
        | Short(n,_) -> n
        | Int(n,_) -> n
        | Long(n,_) -> n
        | Float(n,_) -> n
        | Double(n,_) -> n
        | ByteArray(n,_) -> n
        | String(n,_) -> n
        | List(n,_) -> n
        | Compound(n,_) -> n
        | IntArray(n,_) -> n
    member this.TryGetFromCompound(s:string) =
        match this with
        | Compound(_n,a) -> a |> Array.tryFind (fun x -> x.Name = s)
        | _ -> failwith "try to name-index into a non-compound"
    member this.Item(s:string) =
        match this.TryGetFromCompound(s) with
        | Some x -> x
        | None -> failwithf "tag %s not found" s
    member this.ToString(prefix) =
        match this with
        | End -> ""
        | Byte(n,x) -> prefix + n + " : " + (x.ToString())
        | Short(n,x) -> prefix + n + " : " + (x.ToString())
        | Int(n,x) -> prefix + n + " : " + (x.ToString())
        | Long(n,x) -> prefix + n + " : " + (x.ToString())
        | Float(n,x) -> prefix + n + " : " + (x.ToString())
        | Double(n,x) -> prefix + n + " : " + (x.ToString())
        | ByteArray(n,a) -> prefix + n + " : <" + (if a |> Array.exists (fun b -> b <> 0uy) then "bytes>" else "all zero bytes>")
        | String(n,s) -> prefix + n + " : " + s
        | List(n,pay) -> 
            if n = "TileTicks" then prefix + n + " : [] (NOTE: skipping data)" else
            let sb = new System.Text.StringBuilder(prefix + n + " : [] ")
            let p = "    " + prefix
            match pay with
            | Bytes(a) -> sb.Append(a.Length.ToString() + " bytes") |> ignore
            | Shorts(a) -> sb.Append(a.Length.ToString() + " shorts") |> ignore
            | Ints(a) -> sb.Append(a.Length.ToString() + " ints") |> ignore
            | Longs(a) -> sb.Append(a.Length.ToString() + " longs") |> ignore
            | Floats(a) -> 
                if a.Length > 2 then 
                    sb.Append(a.Length.ToString() + " floats") |> ignore 
                else 
                    sb.Append((a |> Array.fold (fun s x -> s + (x.ToString()) + " ") " : [ ") + "]") |> ignore
            | Doubles(a) -> 
                if n = "Pos" && a.Length = 3 then
                    sb.Append(a.[0].ToString("F") + "," + a.[1].ToString("F") + "," + a.[2].ToString("F") ) |> ignore
                else
                    sb.Append(a.Length.ToString() + " doubles") |> ignore
            | Strings(a) -> for s in a do sb.Append("\""+s+"\"  ") |> ignore
            | Compounds(a) -> 
                let mutable first = true
                for c in a do 
                    if first then
                        first <- false
                    else
                        sb.Append("\n" + p + "----") |> ignore
                    for x in c do 
                        if x <> End then sb.Append("\n" + x.ToString(p)) |> ignore
            | IntArrays(a) -> sb.Append(a.Length.ToString() + " int arrays") |> ignore
            sb.ToString()
        | Compound(n,a) -> 
            let sb = new System.Text.StringBuilder(prefix + n + " :\n")
            let p = "    " + prefix
            for x in a do
                sb.Append(x.ToString(p) + "\n") |> ignore
            sb.ToString()
        | IntArray(n,a) -> prefix + n + if a.Length > 6 then " : <ints>" else (a |> Array.fold (fun s x -> s + (x.ToString()) + " ") " : [ ") + "]"
    override this.ToString() =
        this.ToString("")
    static member ReadName(s : BinaryReader2) =
        let length = s.ReadInt16()
        let utf8bytes = s.ReadBytes(int length)
        System.Text.Encoding.UTF8.GetString(utf8bytes)
    static member Read(s : BinaryReader2) =
        let readCompoundPayload() =
            let nbts = new ResizeArray<NBT>()
            let mutable dun = false
            while not dun do
                let x = NBT.Read(s)
                nbts.Add(x)
                if x = End then
                    dun <- true
            nbts.ToArray()
        match s.ReadByte() with
        | 0uy -> End
        | 1uy -> let n = NBT.ReadName(s) in let x = s.ReadByte() in Byte(n,x)
        | 2uy -> let n = NBT.ReadName(s) in let x = s.ReadInt16() in Short(n,x)
        | 3uy -> let n = NBT.ReadName(s) in let x = s.ReadInt32() in Int(n,x)
        | 4uy -> let n = NBT.ReadName(s) in let x = s.ReadInt64() in Long(n,x)
        | 5uy -> let n = NBT.ReadName(s) in let x = s.ReadSingle() in Float(n,x)
        | 6uy -> let n = NBT.ReadName(s) in let x = s.ReadDouble() in Double(n,x)
        | 7uy -> let n = NBT.ReadName(s) in let len = s.ReadInt32() in let a = s.ReadBytes(len) in ByteArray(n,a)
        | 8uy -> let n = NBT.ReadName(s) in let x = NBT.ReadName(s) in String(n,x)
        | 9uy -> 
            let n = NBT.ReadName(s) 
            let kind = s.ReadByte() 
            let len = s.ReadInt32() 
            let payload =
                match kind with
                | 0uy -> assert(len=0); Compounds [||]
                | 1uy -> Bytes(Array.init len (fun _ -> s.ReadByte()))
                | 2uy -> Shorts(Array.init len (fun _ -> s.ReadInt16()))
                | 3uy -> Ints(Array.init len (fun _ -> s.ReadInt32()))
                | 4uy -> Longs(Array.init len (fun _ -> s.ReadInt64()))
                | 5uy -> Floats(Array.init len (fun _ -> s.ReadSingle()))
                | 6uy -> Doubles(Array.init len (fun _ -> s.ReadDouble()))
                | 8uy -> Strings(Array.init len (fun _ -> NBT.ReadName(s)))
                | 10uy ->let r = Compounds(Array.init len (fun _ -> readCompoundPayload()))
                         //if n = "TileEntities" then printfn "read %d TEs" (match r with Compounds(a) -> a.Length)
                         r
                | 11uy ->IntArrays(Array.init len (fun _ -> let innerLen = s.ReadInt32() in Array.init innerLen (fun _ -> s.ReadInt32())))
                | _ -> failwith "unimplemented list kind"
            List(n,payload)
        | 10uy ->
            let n = NBT.ReadName(s)
            Compound(n, readCompoundPayload())
        | 11uy -> let n = NBT.ReadName(s) in let len = s.ReadInt32() in let a = Array.init len (fun _ -> s.ReadInt32()) in IntArray(n,a)
        | bb -> failwithf "bad NBT tag: %d" bb
    static member WriteName(bw : BinaryWriter2, n : string) =
        let a = System.Text.Encoding.UTF8.GetBytes(n)
        bw.Write(int16 a.Length)  // not n.Length! utf encoding may change it, we need byte count!
        bw.Write(a)
    member this.Write(bw : BinaryWriter2) =
        match this with
        | End -> bw.Write(0uy)
        | Byte(n,x) -> bw.Write(1uy); NBT.WriteName(bw,n); bw.Write(x)
        | Short(n,x) -> bw.Write(2uy); NBT.WriteName(bw,n); bw.Write(x)
        | Int(n,x) -> bw.Write(3uy); NBT.WriteName(bw,n); bw.Write(x)
        | Long(n,x) -> bw.Write(4uy); NBT.WriteName(bw,n); bw.Write(x)
        | Float(n,x) -> bw.Write(5uy); NBT.WriteName(bw,n); bw.Write(x)
        | Double(n,x) -> bw.Write(6uy); NBT.WriteName(bw,n); bw.Write(x)
        | ByteArray(n,a) -> bw.Write(7uy); NBT.WriteName(bw,n); bw.Write(a.Length); bw.Write(a)
        | String(n,x) -> bw.Write(8uy); NBT.WriteName(bw,n); NBT.WriteName(bw,x)
        | List(n,pay) -> 
            bw.Write(9uy)
            NBT.WriteName(bw,n)
            match pay with
            | Bytes(a) -> bw.Write(1uy); bw.Write(a.Length); bw.Write(a)
            | Shorts(a) -> bw.Write(2uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Ints(a) -> bw.Write(3uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Longs(a) -> bw.Write(4uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Floats(a) -> bw.Write(5uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Doubles(a) -> bw.Write(6uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Strings(a) -> bw.Write(8uy); bw.Write(a.Length); for x in a do NBT.WriteName(bw,x)
            //| ByteArrays(a) -> bw.Write(7uy); bw.Write(a.Length); for x in a do bw.Write(x)
            | Compounds(a) -> bw.Write(10uy); bw.Write(a.Length); 
                              //if n = "TileEntities" then printfn "%d" a.Length
                              for x in a do (for y in x do y.Write(bw); assert(x.[x.Length-1] = End))
            | IntArrays(a) -> bw.Write(11uy); bw.Write(a.Length); for x in a do for y in x do bw.Write(y)
        | Compound(n,xs) -> bw.Write(10uy); NBT.WriteName(bw,n); for x in xs do x.Write(bw); assert(xs.[xs.Length-1] = End)
        | IntArray(n,xs) -> bw.Write(11uy); NBT.WriteName(bw,n); bw.Write(xs.Length); for x in xs do bw.Write(x)
    member this.Diff(other : NBT) =
        let rec diff(x,y,path) =
            match x with
            | End | Byte _ | Short _ | Int _ | Long _ | Float _ | Double _ | ByteArray _ | String _ | IntArray _ ->
                 if LanguagePrimitives.GenericEqualityER x y then None else Some(path,x,y)  // all structural leaf nodes
            | List(xn,_) ->
                if x=y then None else 
                match y with 
                | List(yn,_) -> if xn=yn then paydiff(x,y,xn::path) else Some(path,x,y)
                | _ -> Some(path,x,y)
            | Compound(xn,xs) ->
                if x=y then None else 
                match y with 
                | Compound(yn,ys) -> 
                    if xn=yn && xs.Length=ys.Length then 
                        (None,xs,ys) |||> Array.fold2 (fun s xx yy -> match s with None -> diff(xx,yy,xn::path) | s -> s) 
                    else Some(path,x,y)
                | _ -> Some(path,x,y)
        and paydiff((List(_,xs) as x), (List(_,ys) as y), path) =
            match xs with
            | Bytes _ | Shorts _ | Ints _ | Longs _ | Floats _ | Doubles _ | Strings _ | IntArrays _ ->
                 if LanguagePrimitives.GenericEqualityER xs ys then None else Some(path,x,y)  // all structural leaf nodes
            | Compounds(xss) ->
                if xs=ys then None else 
                match ys with 
                | Compounds(yss) -> 
                    if xss.Length=yss.Length then 
                        (None,xss,yss) |||> Array.fold2 (fun s xx yy -> 
                            match s with 
                            | None -> (None,xx,yy) |||> Array.fold2 (fun s xxx yyy -> match s with None -> diff(xxx,yyy,path) | s -> s)
                            | s -> s) 
                    else Some(path,x,y)
                | _ -> Some(path,x,y)
        match diff(this,other,[]) with
        | None -> None
        | Some(path,a,b) -> Some(path |> List.fold (fun s x -> ":" + x + s) "", a, b)

type BlockInfo(blockID:byte, blockData:Lazy<byte>, tileEntity:NBT option) =
    member this.BlockID = blockID
    member this.BlockData = blockData
    member this.TileEntity = tileEntity

type CommandBlock =     // the useful subset I plan to map anything into
    | P of string  // purple (pointing positive Z, unconditional, auto:0)
    | O of string  // orange (pointing positive Z, unconditional, auto:0)
    | U of string  // green (pointing positive Z, unconditional, auto:1)
    | C of string  // green (pointing positive Z, conditional, auto:1)
    | S of string[]  // sign, for commenting start of line

type RegionFile(filename) =
    let rx, rz =
        let m = System.Text.RegularExpressions.Regex.Match(filename, """.*r\.(.*)\.(.*)\.mca(\.new|\.old)?$""")
        int m.Groups.[1].Value, int m.Groups.[2].Value
    let isChunkDirty = Array2D.create 32 32 false
    let chunks : NBT[,] = Array2D.create 32 32 End  // End represents a blank (unrepresented) chunk
    let chunkTimestampInfos : int[,] = Array2D.zeroCreate 32 32
    let mutable firstSeenDataVersion = -1
    let getOrCreateChunk(xx,zz) =
            match chunks.[xx,zz] with
            | End ->
                let newChunk = Compound("",[|NBT.Compound("Level", [|
                                                                    NBT.Int("xPos",xx); NBT.Int("zPos",zz); NBT.Long("LastUpdate", 0L);
                                                                    NBT.Byte("LightPopulated",0uy); NBT.Byte("TerrainPopulated",1uy); // TODO best TerrainPopulated value?
                                                                    NBT.Byte("V",1uy); NBT.Long("InhabitedTime",0L);
                                                                    NBT.IntArray("HeightMap", Array.zeroCreate 256)
                                                                    // a number of things can be absent
                                                                    NBT.List("Sections", Compounds([||]))
                                                                    NBT.End
                                                                   |]
                                                         )
                                             NBT.Int("DataVersion",firstSeenDataVersion)
                                             NBT.End|])
                chunks.[xx,zz] <- newChunk
                newChunk
            | c -> c
    do
        // a set of 4KB sectors
        let chunkOffsetTable = Array.zeroCreate 1024 : int[]
        let timeStampInfo = Array.zeroCreate 1024 : int[]
        use file = new System.IO.FileStream(filename, System.IO.FileMode.Open)
        (*
         The chunk offset for a chunk (x, z) begins at byte 4*(x+z*32) in the
         file. The bottom byte of the chunk offset indicates the number of sectors the
         chunk takes up, and the top 3 bytes represent the sector number of the chunk.
         Given a chunk offset o, the chunk data begins at byte 4096*(o/256) and takes up
         at most 4096*(o%256) bytes. A chunk cannot exceed 1MB in size. If a chunk
         offset is 0, the corresponding chunk is not stored in the region file.

         Chunk data begins with a 4-byte big-endian integer representing the chunk data
         length in bytes, not counting the length field. The length must be smaller than
         4096 times the number of sectors. The next byte is a version field, to allow
         backwards-compatible updates to how chunks are encoded.
         *)
        use br = new BinaryReader2(file)
        for i = 0 to 1023 do
            chunkOffsetTable.[i] <- br.ReadInt32()
        for i = 0 to 1023 do
            timeStampInfo.[i] <- br.ReadInt32()
        for cx = 0 to 31 do
            for cz = 0 to 31 do
                let offset = chunkOffsetTable.[cx+32*cz]
                if offset <> 0 then
                    chunkTimestampInfos.[cx,cz] <- timeStampInfo.[cx+32*cz]
                    let sectorNumber = offset >>> 8
                    let _numSectors = offset &&& 0xFF
                    br.BaseStream.Seek(int64 (sectorNumber * 4096), System.IO.SeekOrigin.Begin) |> ignore
                    let length = br.ReadInt32()
                    let _version = br.ReadByte()
                    // If you prefer to read Zlib-compressed chunk data with Deflate (RFC1951), just skip the first two bytes and leave off the last 4 bytes before decompressing.
                    let _dummy1 = br.ReadByte()
                    let _dummy2 = br.ReadByte()          // CMF and FLG are first two bytes, could remember these and just rewrite them
                    let bytes = br.ReadBytes(length - 6) // ADLER32 is last 4 bytes checksum, not hard to compute a new one to write back out
                    use s = new System.IO.Compression.DeflateStream(new System.IO.MemoryStream(bytes) , System.IO.Compression.CompressionMode.Decompress)
                    let nbt = NBT.Read(new BinaryReader2(s))
                    chunks.[cx,cz] <- nbt
                    if firstSeenDataVersion = -1 then
                        firstSeenDataVersion <- match nbt.["DataVersion"] with NBT.Int(_,i) -> i   // TODO make not fail on Minecraft 1.8
    member this.RX = rx  // e.g. 1 means starts at x coord 512
    member this.RZ = rz
    member this.Write(outputFilename) =
        let zeros = Array.zeroCreate 4096 : byte[]
        let chunkOffsetTable = Array.zeroCreate 1024 : int[]
        let timeStampInfo = Array.zeroCreate 1024 : int[]
        let mutable nextFreeSection = 2  // sections 0 and 1 are chunk offset table and timestamp info table
        use file = new System.IO.FileStream(outputFilename, System.IO.FileMode.CreateNew)
        use bw = new BinaryWriter2(file)
        for cx = 0 to 31 do
            for cz = 0 to 31 do
                if chunks.[cx,cz] <> End then
                    if isChunkDirty.[cx,cz] then
                        let chunkLevel = match chunks.[cx,cz] with Compound(_,[|c;_|]) -> c | Compound(_,[|c;_;_|]) -> c  // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name (or two with a data version appended)
                        match chunkLevel with 
                        | Compound(n,nbts) -> 
                            let i = nbts |> Array.findIndex (fun nbt -> nbt.Name = "LightPopulated")
                            nbts.[i] <- NBT.Byte("LightPopulated", 0uy)
                    let ms = new System.IO.MemoryStream()
                    use s = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Compress, true)
                    chunks.[cx,cz].Write(new BinaryWriter2(s))
                    s.Close()
                    let numBytes = int ms.Length
                    let numSectionsNeeded = 1 + ((numBytes + 11) / 4096)
                    chunkOffsetTable.[cx+cz*32] <- (nextFreeSection <<< 8) + numSectionsNeeded
                    timeStampInfo.[cx+cz*32] <- chunkTimestampInfos.[cx,cz]  // no-op, not updating them
                    bw.Seek(nextFreeSection * 4096, System.IO.SeekOrigin.Begin) |> ignore
                    nextFreeSection <- nextFreeSection + numSectionsNeeded 
                    bw.Write(numBytes + 6) // length
                    bw.Write(2uy) // version (must be 2)
                    bw.Write(120uy) // CMF
                    bw.Write(156uy) // FLG
                    let temp = ms.ToArray()
                    bw.Write(temp, 0, numBytes)  // stream
                    bw.Write(RegionFile.ComputeAdler(temp)) // adler checksum
                    let paddingLengthNeeded = 4096 - ((numBytes+11)%4096)
                    bw.Write(zeros, 0, paddingLengthNeeded) // zero padding out to 4K
        bw.Seek(0, System.IO.SeekOrigin.Begin) |> ignore
        for i = 0 to 1023 do
            bw.Write(chunkOffsetTable.[i])
        for i = 0 to 1023 do
            bw.Write(timeStampInfo.[i])
    static member ComputeAdler(bytes : byte[]) : int =
        (*
                Adler-32 is composed of two sums accumulated per byte: s1 is
                the sum of all bytes, s2 is the sum of all s1 values. Both sums
                are done modulo 65521. s1 is initialized to 1, s2 to zero.  The
                Adler-32 checksum is stored as s2*65536 + s1 in most-
                significant-byte first (network) order.
        *)
        let mutable s1 = 1
        let mutable s2 = 0
        for i = 0 to bytes.Length - 1 do
            s1 <- (s1 + int(bytes.[i])) % 65521
            s2 <- (s2 + s1) % 65521
        s2*65536 + s1
    member private this.SetChunkDirty(x,z) = // x,z are world coordinates
        let xx = ((x+5120)%512)/16
        let zz = ((z+5120)%512)/16
        isChunkDirty.[xx,zz] <- true
    member this.GetOrCreateChunk(x,z) =  // x,z are world coordinates
        let xx = ((x+5120)%512)/16
        let zz = ((z+5120)%512)/16
        let theChunk = getOrCreateChunk(xx,zz)
        theChunk
    member this.GetOrCreateSection(x,y,z) =  // x,y,z are world coordinates
        let xx = ((x+5120)%512)/16
        let zz = ((z+5120)%512)/16
        let theChunk = getOrCreateChunk(xx,zz)
        let theChunkLevel = match theChunk with Compound(_,[|c;_|]) | Compound(_,[|c;_;_|]) -> c // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
        let sections = match theChunkLevel.["Sections"] with List(_,Compounds(cs)) -> cs
        let theSection = 
            match sections |> Array.tryFind (Array.exists (function Byte("Y",n) when n=byte(y/16) -> true | _ -> false)) with
            | Some x -> x
            | None ->
                let newSection = [| NBT.Byte("Y",byte(y/16)); NBT.ByteArray("Blocks", Array.zeroCreate 4096); NBT.ByteArray("Data", Array.zeroCreate 2048); 
                                    NBT.ByteArray("BlockLight",Array.create 2048 0uy); NBT.ByteArray("SkyLight",Array.create 2048 0uy); NBT.End |]  // TODO relight chunk instead of pop with 255uy?
                match theChunkLevel with
                | Compound(_,a) ->
                    let i = a |> Array.findIndex (fun x -> x.Name="Sections")
                    a.[i] <- List("Sections",Compounds( sections |> Seq.append [| newSection |] |> Seq.toArray ))
                    isChunkDirty.[xx,zz] <- true
                    newSection
        theSection
    member this.GetChunk(cx, cz) =
        match chunks.[cx,cz] with
        | End -> failwith "chunk not represented, NYI"
        | c -> c
    member this.TryGetChunk(cx, cz) =
        match chunks.[cx,cz] with
        | End -> None
        | c -> Some c
    member this.GetBlockInfo(x, y, z) =
        match this.TryGetBlockInfo(x,y,z) with
        | Some r -> r
        | None -> failwith "chunk not represented, NYI"
    member this.TryGetBlockInfo(x, y, z) =
        let xxxx = if x < 0 then x - 512 else x
        let zzzz = if z < 0 then z - 512 else z
        let xxxx = if xxxx < 0 then xxxx+1 else xxxx
        let zzzz = if zzzz < 0 then zzzz+1 else zzzz
        if xxxx/512 <> rx || zzzz/512 <> rz then failwith "coords outside this region"
        let theSection = this.GetOrCreateSection(x,y,z)
        let dx, dy, dz = (x+51200) % 16, y % 16, (z+51200) % 16
        let i = dy*256 + dz*16 + dx
        // BlockID
        let blocks = theSection |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
        // BlockData
        let blockData = theSection |> Array.pick (function ByteArray("Data",a) -> Some a | _ -> None)
        let blockDataAtI = Lazy.Create(fun() ->
            // expand 2048 half-bytes into 4096 for convenience of same indexing
            let blockData = Array.init 4096 (fun x -> if (x+51200)%2=1 then blockData.[x/2] >>> 4 else blockData.[x/2] &&& 0xFuy)
            blockData.[i])
        let theChunk = this.GetOrCreateChunk(x,z)
        let theChunkLevel = match theChunk with Compound(_,[|c;_|]) | Compound(_,[|c;_;_|]) -> c // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
        // TileEntities
        let tileEntity = 
            match theChunkLevel.["TileEntities"] with 
            | List(_,Compounds(cs)) ->
                let tes = cs |> Array.choose (fun te -> 
                    let te = Compound("unnamedDummyToCarryAPayload",te)
                    if te.["x"]=Int("x",x) && te.["y"]=Int("y",y) && te.["z"]=Int("z",z) then Some te else None)
                if tes.Length = 0 then None
                elif tes.Length = 1 then Some tes.[0]
                else failwith "unexpected: multiple TileEntities with same xyz coords"
            | _ -> None
        Some(new BlockInfo(blocks.[i], blockDataAtI, tileEntity))
    member this.SetBlockIDAndDamage(x, y, z, blockID, damage) =
        if (x+5120)/512 <> rx+10 || (z+5120)/512 <> rz+10 then failwith "coords outside this region"
        if damage > 15uy then failwith "invalid blockData"
        let theSection = this.GetOrCreateSection(x,y,z)
        let dx, dy, dz = (x+5120) % 16, y % 16, (z+5120) % 16
        let i = dy*256 + dz*16 + dx
        // BlockID
        let blocks = theSection |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
        blocks.[i] <- blockID
        // BlockData
        let blockData = theSection |> Array.pick (function ByteArray("Data",a) -> Some a | _ -> None)
        let mutable tmp = blockData.[i/2]
        if i%2 = 0 then
            tmp <- tmp &&& 0xF0uy
            tmp <- tmp + damage
        else
            tmp <- tmp &&& 0x0Fuy
            tmp <- tmp + (damage <<< 4)
        blockData.[i/2] <- tmp
        this.SetChunkDirty(x,z)
    member this.PlaceCommandBlocksWithLeadingSignStartingAt(x,y,startz,cmds:_[],signText:string[]) =
        let newCmds = [| yield S signText; yield! cmds |]
        this.PlaceCommandBlocksStartingAt(x,y,startz,newCmds)
    member this.PlaceCommandBlocksStartingAt(x,y,startz,cmds:_[]) =
        let cmds = Seq.append cmds [| O "say dummy command at end" |]
        let mutable z = startz
        let mkCmd(x,y,z,auto,s,txt:_[]) = 
            if txt = null then
                [| NBT.Int("x",x); NBT.Int("y",y); NBT.Int("z",z); NBT.Byte("auto",auto); NBT.String("Command",s); 
                   NBT.Byte("conditionMet",1uy); NBT.String("CustomName","@"); NBT.Byte("powered",0uy);   // TODO defaulting conditionMet to 1, which is not like MC, but avoids an MC bug
                   NBT.String("id","Control"); NBT.Int("SuccessCount",0); NBT.Byte("TrackOutput",1uy);
                   NBT.End |]
            else
                [| yield NBT.Int("x",x); yield NBT.Int("y",y); yield NBT.Int("z",z); yield NBT.String("id","Sign")
                   for i = 0 to txt.Length-1 do
                       yield NBT.String(sprintf "Text%d" (i+1), sprintf "{\"text\":\"%s\"}" txt.[i])
                   yield NBT.End |]
        let mutable prevcx = -1
        let mutable prevcz = -1
        let mutable tepayload = ResizeArray<NBT[]>()
        let storeIt(prevcx,prevcz,tepayload) =
            let a = match getOrCreateChunk(prevcx, prevcz) with Compound(_,[|Compound(_,a);_|]) | Compound(_,[|Compound(_,a);_;_|]) -> a
            let mutable found = false
            let mutable i = 0
            for te in tepayload do
                let sb = new System.Text.StringBuilder()
                for nbt in te do
                    sb.Append(nbt.ToString()).Append("   ") |> ignore
                //printfn "%s" (let s = sb.ToString() in s.Substring(0,min 90 s.Length))
            //printfn "-----"
            while not found && i < a.Length-1 do
                if a.[i].Name = "TileEntities" then
                    found <- true
                    a.[i] <- List("TileEntities",Compounds(tepayload |> Array.ofSeq))
                i <- i + 1
            if not found then
                match chunks.[prevcx, prevcz] with 
                | Compound(_,([|Compound(n,a);_|] as r)) 
                | Compound(_,([|Compound(n,a);_;_|] as r)) -> 
                    r.[0] <- Compound(n, a |> Seq.append [| List("TileEntities",Compounds(tepayload |> Array.ofSeq)) |] |> Array.ofSeq)
        for c in cmds do
            let bid, bd, au, s,txt = 
                match c with
                | P s -> 210uy,3uy,0uy,s,null
                | O s -> 137uy,3uy,0uy,s,null
                | U s -> 211uy,3uy,1uy,s,null
                | C s -> 211uy,11uy,1uy,s,null
                | S txt -> 63uy,8uy,0uy,"",txt
            this.SetBlockIDAndDamage(x,y,z,bid,bd)
            let nbt = mkCmd(x,y,z,au,s,txt)
            if (x+5120)/512 <> rx+10 || (z+5120)/512 <> rz+10 then failwith "coords outside this region"
            let xx = ((x+5120)%512)/16
            let zz = ((z+5120)%512)/16
            if xx <> prevcx || zz <> prevcz then
                // store out old TE as we move out of this chunk
                if prevcx <> -1 then
                    storeIt(prevcx,prevcz,tepayload)
                // load in initial TE as we move into next chunk
                let newChunk = match getOrCreateChunk(xx,zz) with 
                               | Compound(_,[|Compound(_,_) as c;_|]) 
                               | Compound(_,[|Compound(_,_) as c;_;_|]) -> c
                tepayload <- match newChunk.TryGetFromCompound("TileEntities") with | Some (List(_,Compounds(cs))) -> ResizeArray(cs) | None -> ResizeArray()
                prevcx <- xx
                prevcz <- zz
            // accumulate payload in this chunk
            let thisz = z
            tepayload <-  
                tepayload 
                |> Seq.filter (fun te -> not(Array.exists (fun o -> o=Int("x",x)) te && Array.exists (fun o -> o=Int("y",y)) te && Array.exists (fun o -> o=Int("z",thisz)) te))
                |> Seq.append [|nbt|] |> (fun x -> ResizeArray x)
            z <- z + 1
        storeIt(prevcx,prevcz,tepayload)
    member this.DumpChunkDebug(cx,cz) =
        let nbt = chunks.[cx,cz]
        let s = nbt.ToString()
        printfn "%s" s

let readDatFile(filename : string) =
    use s = new System.IO.Compression.GZipStream(new System.IO.FileStream(filename, System.IO.FileMode.Open), System.IO.Compression.CompressionMode.Decompress)
    NBT.Read(new BinaryReader2(s))

let writeDatFile(filename : string, nbt : NBT) =
    use s = new System.IO.Compression.GZipStream(new System.IO.FileStream(filename, System.IO.FileMode.CreateNew), System.IO.Compression.CompressionMode.Compress)
    nbt.Write(new BinaryWriter2(s))


//////////////////////////////////////////////////////////////////

(*
type Payload =
    | Bytes of byte[]
    | Shorts of int16[]
    | Ints of int[]
    | Longs of int64[]
    | Floats of single[]
    | Doubles of double[]
    | Strings of string[]
    | Compounds of NBT[][]
    | IntArrays of int[][]
and NBT =
    | End
    | Byte of Name * byte
    | Short of Name * int16
    | Int of Name * int
    | Long of Name * int64
    | Float of Name * single
    | Double of Name * double
    | ByteArray of Name * byte[]
    | String of Name * string // int16 length beforehand
    | List of Name * Payload // (name,kind,num,a)
    | Compound of Name * NBT[]
    | IntArray of Name * int[] // int32 before data shows num elements
*)

let SimpleDisplay nbt =
    match nbt with
    | End -> failwith "bad SimpleDisplay"
    | Byte(_,x) -> "<byte> " + x.ToString()
    | Short(_,x) -> "<short> " + x.ToString()
    | Int(_,x) -> "<int> " + x.ToString()
    | Long(_,x) -> "<long> " + x.ToString()
    | Float(_,x) -> "<float> " + x.ToString()
    | Double(_,x) -> "<double> " + x.ToString()
    | ByteArray(_,_) -> "<byte array>"
    | String(_,x) -> x
    | List(_,_) -> null
    | Compound(_,_) -> null
    | IntArray(_,_) -> "<int array>"
   
open System.Windows
open System.Windows.Controls
open System.Windows.Input
open System.Windows.Media

let mutable skipUnchangedChunks = true
let mutable namesToIgnore = [||]
let mutable skipUnchangedValues = true
let mutable startExpanded = false
let LIST_ITEM = "<list item>"

let rec MakeTreeDiff (Compound(_,x) as xp) (Compound(_,y) as yp) (tvp:TreeViewItem) =
    let hasDiff = ref false
    let xnames = x |> Array.map (fun z -> z.Name) |> set
    let ynames = y |> Array.map (fun z -> z.Name) |> set
    let names = (Set.union xnames ynames).Remove(END_NAME)
    let names = Set.difference names (set namesToIgnore)
    for n in names do
        if xnames.Contains(n) then
            let xpn = xp.[n]
            let xd = SimpleDisplay(xpn)
            if ynames.Contains(n) then
                let ypn = yp.[n]
                let yd = SimpleDisplay(ypn)
                if xd = yd then
                    if xd <> null then
                        if xpn = ypn then
                            if not skipUnchangedValues then
                                tvp.Items.Add(new TreeViewItem(Header=n+": "+xd)) |> ignore
                        else // diff byte arrays
                            tvp.Items.Add(new TreeViewItem(Header=n+": "+xd,Background=Brushes.Red)) |> ignore
                            tvp.Items.Add(new TreeViewItem(Header=n+": "+yd,Background=Brushes.Yellow)) |> ignore
                            hasDiff := true
                    else // same name compound/list
                        let tvi = new TreeViewItem(Header=n)
                        match xpn, ypn with
                        | Compound _, Compound _ ->
                            if MakeTreeDiff xpn ypn tvi then
                                tvi.Background <- Brushes.Orange
                                hasDiff := true
                        | List(_,xpay), List(_,ypay) ->
                            match xpay, ypay with
                            | Bytes xx, Bytes yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | Shorts xx, Shorts yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | Ints xx, Ints yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | Longs xx, Longs yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | Floats xx, Floats yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | Doubles xx, Doubles yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | Strings xx, Strings yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | IntArrays xx, IntArrays yy -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                            | Compounds xx, Compounds yy ->
                                // This is the hard part.  Need to be 'smart' about how to compare/merge/display
                                let ya = ResizeArray yy
                                for x in xx do
                                    let tvj = new TreeViewItem(Header=LIST_ITEM)
                                    let localDiff = ref false
                                    let BODY(i) =
                                        if i = -1 then
                                            MakeTreeDiff (Compound("",x)) (Compound("",[||])) tvj |> ignore
                                            tvj.Background <- Brushes.Red
                                            tvi.Background <- Brushes.Orange
                                            hasDiff := true
                                            localDiff := true
                                        else
                                            let y = ya.[i]
                                            ya.RemoveAt(i)
                                            if MakeTreeDiff (Compound("",x)) (Compound("",y)) tvj then
                                                tvj.Background <- Brushes.Orange
                                                tvi.Background <- Brushes.Orange
                                                hasDiff := true
                                                localDiff := true
                                    // uuid match
                                    if x |> Array.exists (function (Long("UUIDLeast",_))->true | _->false) &&
                                       x |> Array.exists (function (Long("UUIDMost",_))->true | _->false) then
                                        let xuuidl = x |> Seq.pick (function (Long("UUIDLeast",v)) -> Some v | _ -> None)
                                        let xuuidm = x |> Seq.pick (function (Long("UUIDMost",v)) -> Some v | _ -> None)
                                        let i = ya.FindIndex(fun ee -> 
                                                    ee |> Array.exists (function (Long("UUIDLeast",v)) -> v=xuuidl | _ -> false) &&
                                                    ee |> Array.exists (function (Long("UUIDMost",v)) -> v=xuuidm | _ -> false))
                                        BODY(i)
                                    // x,y,z match
                                    elif x |> Array.exists (fun e -> e.Name="x") &&
                                       x |> Array.exists (fun e -> e.Name="y") &&
                                       x |> Array.exists (fun e -> e.Name="z") then
                                        let ix = x |> Seq.pick (function (Int("x",v)) -> Some v | _ -> None)
                                        let iy = x |> Seq.pick (function (Int("y",v)) -> Some v | _ -> None)
                                        let iz = x |> Seq.pick (function (Int("z",v)) -> Some v | _ -> None)
                                        let i = ya.FindIndex(fun ee -> 
                                                    ee |> Array.exists (function (Int("x",v)) -> v=ix | _ -> false) &&
                                                    ee |> Array.exists (function (Int("y",v)) -> v=iy | _ -> false) &&
                                                    ee |> Array.exists (function (Int("z",v)) -> v=iz | _ -> false))
                                        BODY(i)
                                    // (approx) exact match
                                    elif true then
                                        let s = x.ToString()
                                        let i = ya.FindIndex(fun ee -> ee.ToString() = s)
                                        BODY(i)
                                    // TODO other kinds of heuristic matches?
                                    else
                                        MakeTreeDiff (Compound("",x)) (Compound("",[||])) tvj |> ignore
                                        tvj.Background <- Brushes.Red
                                        tvi.Background <- Brushes.Orange
                                        hasDiff := true
                                    if not skipUnchangedValues || !localDiff then
                                        tvi.Items.Add(tvj) |> ignore
                                for y in ya do
                                    let tvj = new TreeViewItem(Header=LIST_ITEM)
                                    MakeTreeDiff (Compound("",[||])) (Compound("",y)) tvj |> ignore
                                    tvj.Background <- Brushes.Yellow
                                    tvi.Background <- Brushes.Orange
                                    hasDiff := true
                                    tvi.Items.Add(tvj) |> ignore
                            | _ -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                        | _ -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                        if not skipUnchangedValues || tvi.Items.Count <> 0 then
                            tvp.Items.Add(tvi) |> ignore
                else // completely different (atoms of diff values, or diff types)
                    // TODO what best display?
                    tvp.Items.Add(new TreeViewItem(Header=n+": "+xd,Background=Brushes.Red)) |> ignore
                    tvp.Items.Add(new TreeViewItem(Header=n+": "+yd,Background=Brushes.Yellow)) |> ignore
                    hasDiff := true
            else // only x
                // TODO what best display?
                tvp.Items.Add(new TreeViewItem(Header=n+": "+xd,Background=Brushes.Red)) |> ignore
                hasDiff := true
        else // only y
            let yd = SimpleDisplay(yp.[n])
            // TODO what best display?
            tvp.Items.Add(new TreeViewItem(Header=n+": "+yd,Background=Brushes.Yellow)) |> ignore
            hasDiff := true
    !hasDiff

let diffRegions(r1:RegionFile,r2:RegionFile,regionFile1:string,regionFile2:string) =
    skipUnchangedChunks <- true
    //namesToIgnore <- [| "InhabitedTime"; "LastUpdate"; "LastOutput" |]
    namesToIgnore <- [| "InhabitedTime"; "LastUpdate"; "LastOutput"; (*"Biomes";*) "SkyLight"; "BlockLight"; "Data"; "Blocks"; "Entities" |]
    skipUnchangedValues <- true
    startExpanded <- true
    let tv = new TreeView()
    printfn "Processing chunks..."
    tv.Items.Add(new TreeViewItem(Header=regionFile1,Background=Brushes.Red)) |> ignore
    tv.Items.Add(new TreeViewItem(Header=regionFile2,Background=Brushes.Yellow)) |> ignore
    for cx = 0 to 31 do
        for cz = 0 to 31 do
            printf "(%d,%d)..." cx cz
            let n = new TreeViewItem(Header="Chunk "+cx.ToString()+","+cz.ToString())
            let c1 = r1.TryGetChunk(cx,cz)
            let c2 = r2.TryGetChunk(cx,cz)
            match c1,c2 with
            | Some c1, Some c2 ->
                if MakeTreeDiff c1 c2 n then
                    n.Background <- Brushes.Orange 
                    tv.Items.Add(n) |> ignore
                elif not skipUnchangedChunks then
                    tv.Items.Add(n) |> ignore
            | Some c1, _ ->
                if MakeTreeDiff c1 (Compound("",[||])) n then
                    n.Background <- Brushes.Red 
                    tv.Items.Add(n) |> ignore
                elif not skipUnchangedChunks then
                    tv.Items.Add(n) |> ignore
            | _, Some c2 ->
                if MakeTreeDiff (Compound("",[||])) c2 n then
                    n.Background <- Brushes.Yellow
                    tv.Items.Add(n) |> ignore
                elif not skipUnchangedChunks then
                    tv.Items.Add(n) |> ignore
            | _ -> ()
            if startExpanded then
                n.ExpandSubtree()
    printfn ""
    let window = new Window(Title="NBT Difference viewer by Dr. Brian Lorgon111", Content=tv)
    let app = new Application()
    app.Run(window) |> ignore

let diffRegionFiles(regionFile1,regionFile2) =
    let r1 = new RegionFile(regionFile1)
    let r2 = new RegionFile(regionFile2)
    diffRegions(r1,r2,regionFile1,regionFile2)

let diffDatFiles(datFile1,datFile2) =
    skipUnchangedChunks <- false
    namesToIgnore <- [| |]
    skipUnchangedValues <- false
    startExpanded <- false
    let tv = new TreeView()
    let r1 = readDatFile(datFile1)
    let r2 = readDatFile(datFile2)
    tv.Items.Add(new TreeViewItem(Header=datFile1,Background=Brushes.Red)) |> ignore
    tv.Items.Add(new TreeViewItem(Header=datFile2,Background=Brushes.Yellow)) |> ignore
    let n = new TreeViewItem(Header="<root>")
    if MakeTreeDiff r1 r2 n then
        n.Background <- Brushes.Orange 
        tv.Items.Add(n) |> ignore
    if startExpanded then
        n.ExpandSubtree()
    let window = new Window(Title="NBT Difference viewer by Dr. Brian Lorgon111", Content=tv)
    let app = new Application()
    app.Run(window) |> ignore

///////////////////////////////////////

   
let killAllEntities() =
    let filename = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Bingo_v2_0_10\region\r.0.0.mca"""
    let regionFile = new RegionFile(filename)
    for cx = 0 to 15 do
        for cz = 0 to 15 do
            let nbt = regionFile.GetChunk(cx, cz)
            match nbt with 
            Compound(_,[|theChunk;_|]) ->
                match theChunk.TryGetFromCompound("Entities") with 
                | None -> ()
                | Some _ -> 
                    match theChunk with 
                    Compound(_cname,a) ->
                        let i = a |> Array.findIndex (fun x -> match x with NBT.List("Entities",_) -> true | _ -> false)
                        a.[i] <- NBT.List("Entities",Compounds[||])
    regionFile.Write(filename+".new")

let makeArmorStand(x,y,z,customName,team) =
            [|
                NBT.String("id","ArmorStand")
                NBT.Byte("NoGravity",1uy)
                NBT.Byte("Invisible",1uy)
                NBT.List("Pos",Payload.Doubles [|x;y;z|])
                NBT.Byte("CustomNameVisible",1uy)
                NBT.String("CustomName",customName)
                NBT.String("Team",team)
                NBT.Byte("Marker",1uy)
                NBT.End
            |], (int x, int z)

let placeCertainEntitiesInTheWorld(entities,filename) =
    let regionFile = new RegionFile(filename)
    for cx = 0 to 15 do
        for cz = 0 to 15 do
            let nbt = regionFile.TryGetChunk(cx, cz)
            match nbt with 
            | Some( Compound(_,[|theChunk;_;_|]) ) | Some( Compound(_,[|theChunk;_|]) ) ->
                match theChunk.TryGetFromCompound("Entities") with 
                | None -> ()
                | Some _ -> 
                    match theChunk with 
                    Compound(cname,a) ->
                        let i = a |> Array.findIndex (fun x -> match x with NBT.List("Entities",_) -> true | _ -> false)
                        let es = entities |> Seq.choose (fun (e,(x,z)) -> if x/16=cx && z/16=cz then Some e else None) |> Seq.toArray 
                        a.[i] <- NBT.List("Entities",Compounds es)
            | None -> ()
    regionFile.Write(filename+".new")

let dumpSomeCommandBlocks(fil) =
    let aaa = ResizeArray()
    for filename in [fil
                     ] do
        let regionFile = new RegionFile(filename)
        
        let blockIDCounts = Array.zeroCreate 256
        for cx = 0 to 31 do
            for cz = 0 to 31 do
                try
                    let nbt = regionFile.GetChunk(cx, cz)
                    let theChunk = match nbt with Compound(_,[|c;_|]) -> c 
                                                | Compound(_,[|c;_;_|]) -> c 
                                                | _ -> failwith "unexpected cpdf"
                    match theChunk.TryGetFromCompound("TileEntities") with 
                    | None -> ()
                    | Some te -> 
                        match te with List(_,Compounds(tes)) ->
                        for t in tes do
                            if t |> Array.exists (function String("Command",s) -> true | _ -> false) then
                                let comm = t |> Array.pick (function String("Command",s) -> Some(string s) | _ -> None)
                                let x = t |> Array.pick (function Int("x",i) -> Some(int i) | _ -> None)
                                let y = t |> Array.pick (function Int("y",i) -> Some(int i) | _ -> None)
                                let z = t |> Array.pick (function Int("z",i) -> Some(int i) | _ -> None)
                                aaa.Add( (comm,x,y,z) )
                    let sections = match theChunk.["Sections"] with List(_,Compounds(cs)) -> cs
                    for s in sections do
                        let ySection = s |> Array.pick (function Byte("Y",y) -> Some(int y) | _ -> None)
                        let blocks = s |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
                        for i = 0 to 4095 do
                            let bid = blocks.[i]
                            blockIDCounts.[int bid] <- blockIDCounts.[int bid] + 1
                with e ->
                    if e.Message.StartsWith("chunk not") then
                        () // yeah yeah
                    else
                        printfn "%A" e.Message 

    printfn "There are %d command blocks" aaa.Count 
    let aaa = aaa.ToArray() |> Array.map (fun (s,x,y,z) -> let s = (if s.StartsWith("/") then s.Substring(1) else s) in s,x,y,z)
    let aaa = aaa |> Array.sortBy (fun (_s,x,y,z) -> y,z,x) 

    let armors = ResizeArray()
    let USE_PADDING = false   // bad usability
    let FACING_EW = false
    for (comm,x,y,z) in aaa do
        printfn "%5d,%5d,%5d : %s" x y z comm
        // (default) facing west
        let comm = if comm.Length > 120 then comm.Substring(0,120)+"..." else comm
//        let comm = if comm = "" then "<empty>" else comm
        let comm = if comm = "" then "   " else comm
        let adjustedLen = (float comm.Length) //- (float(comm |> Seq.fold (fun x c -> x+if ",:il. []".Contains(string c) then 1 else 0) 0)/4.0)
        let deltaZ,pad = if USE_PADDING then 1.0,int(adjustedLen / 0.7) else 0.5 - (adjustedLen / 15.0),0
        let color = ((if FACING_EW then z else x)/2) % 3
        let deltaX = 0.25 + (0.25 * float color)
        let deltaX, deltaZ =
            if FACING_EW then
                deltaX, deltaZ
            else
                // facing north
                let deltaX,deltaZ = -deltaX,-deltaZ
                let deltaX,deltaZ = deltaZ,deltaX
                let deltaX = deltaX + 1.0
                let deltaZ = deltaZ + 1.0
                deltaX,deltaZ
        armors.Add(makeArmorStand(deltaX+float x, float (y+1), (float z)+deltaZ,
                                  (if USE_PADDING then (String.replicate pad " ") else "")+comm,
                                  (if color = 0 then "White" elif color = 1 then "Aqua" else "Yellow")))
    placeCertainEntitiesInTheWorld(armors, fil)

let diagnoseStringDiff(s1 : string, s2 : string) =
    if s1 = s2 then printfn "same!" else
    let mutable i = 0
    while i < s1.Length && i < s2.Length && s1.[i] = s2.[i] do
        i <- i + 1
    if i = s1.Length then 
        printfn "first ended at pos %d whereas second still has more %s" s1.Length (s2.Substring(s1.Length))
    elif i = s2.Length then 
        printfn "second ended at pos %d whereas first still has more %s" s2.Length (s1.Substring(s2.Length))
    else
        let j = i - 20
        let j = if j < 0 then 0 else j
        printfn "first diff at position %d" i
        printfn ">>>>>"
        printfn "%s" (s1.Substring(j, 40))
        printfn "<<<<<"
        printfn "%s" (s2.Substring(j, 40))

let testReadWriteRegionFile() =
    let filename = """F:\.minecraft\saves\BingoGood\region\r.0.0.mca"""
    let out1 = """F:\.minecraft\saves\BingoGood\region\out1.r.0.0.mca"""
    let out2 = """F:\.minecraft\saves\BingoGood\region\out2.r.0.0.mca"""
    // load up orig file, show some data
    let origFile = new RegionFile(filename)
    let nbt = origFile.GetChunk(12, 12)
    let origString = nbt.ToString()
    // write out a copy
    origFile.Write(out1)
    // try to read in the copy, see if data same
    let out1File = new RegionFile(out1)
    let nbt = out1File.GetChunk(12, 12)
    let out1String = nbt.ToString()
    diagnoseStringDiff(origString, out1String)
    // write out a copy
    out1File.Write(out2)
    // try to read in the copy, see if data same
    let out2File = new RegionFile(out2)
    let nbt = out2File.GetChunk(12, 12)
    let out2String = nbt.ToString()
    diagnoseStringDiff(origString, out2String)
(*
    // finding diff in byte arrays
    let k =
        let mutable i = 0
        let mutable dun = false
        while not dun do
            if decompressedBytes.[i] = writtenBytes.[i] then
                i <- i + 1
            else
                dun <- true
        i
    for b in decompressedBytes.[k-5 .. k+25] do
        printf "%3d " b
    printfn ""
    printfn "-------"
    for b in writtenBytes.[k-5 .. k+25] do
        printf "%3d " b
    printfn ""
*)


// Look through statistics and achievements, discover what biomes MC thinks I have explored
let checkExploredBiomes() =
    let jsonSer = new System.Web.Script.Serialization.JavaScriptSerializer() // System.Web.Extensions.dll
    let jsonObj = jsonSer.DeserializeObject(System.IO.File.ReadAllText("""F:\.minecraft\saves\E&T Season 7\stats\6fbefbde-67a9-4f72-ab2d-2f3ee5439bc0.json"""))
    let dict : System.Collections.Generic.Dictionary<string,obj> = downcast jsonObj
    for kvp in dict do
        if kvp.Key.StartsWith("achievement.exploreAllBiomes") then
            let dict2 : System.Collections.Generic.Dictionary<string,obj> = downcast kvp.Value 
            let o = dict2.["progress"]
            let oa : obj[] = downcast o
            let sa = new ResizeArray<string>()
            for x in oa do
                sa.Add(downcast x)
            //printfn "have %d, need %d" sa.Count BIOMES_NEEDED_FOR_ADVENTURING_TIME.Length 
            let biomeSet = BIOMES_NEEDED_FOR_ADVENTURING_TIME |> set
            printfn "%d" biomeSet.Count 
            let mine = sa |> set
            let unexplored = biomeSet - mine
            printfn "%d" unexplored.Count 
            for x in biomeSet do
                printfn "%s %s" (if mine.Contains(x) then "XXX" else "   ") x
            //printfn "----"
            //for x in mine do
            //    printfn "%s %s" (if biomeSet.Contains(x) then "XXX" else "   ") x

let renamer() =
    let file = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Snakev2.0G\level.dat"""
    let nbt = readDatFile(file)
    //printfn "%s" (nbt.ToString())
    let newNbt =
        match nbt with
        | Compound("",[|Compound("Data",a);End|]) -> 
            let a = a |> Array.filter (function String("LevelName",_) -> false | _ -> true)
            let a = a |> Array.append [|String("LevelName","Snake Game by Lorgon111")|]
            Compound("",[|Compound("Data",a);End|])
        | _ -> failwith "bummer"
    printfn "%s" (newNbt.ToString())
    writeDatFile(file + ".new", newNbt)


let placeCertainBlocksInTheWorld() =
    let filename = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Photo\region\r.0.-2.mca"""
    let regionFile = new RegionFile(filename)
    printfn "%A" (PhotoToMinecraft.finalBmp = null)
    let maxHeight = 60 + PhotoToMinecraft.pictureBlockFilenames.GetLength(1)-1
    for x = 0 to PhotoToMinecraft.pictureBlockFilenames.GetLength(0)-1 do
        for y = 0 to PhotoToMinecraft.pictureBlockFilenames.GetLength(1)-1 do
            let filename = System.IO.Path.GetFileNameWithoutExtension(PhotoToMinecraft.pictureBlockFilenames.[x,y]).ToLower()
            let (_,bid,dmg) = textureFilenamesToBlockIDandDataMapping |> Array.find (fun (n,_,_) -> n = filename)
            regionFile.SetBlockIDAndDamage( 400, maxHeight - y, -670 + x, byte bid, byte dmg)
    regionFile.Write(filename+".new")

let placeCertainBlocksInTheSky() =
    let filename = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\MinecraftBINGOv2_4update53\region\r.0.0.mca"""
    let regionFile = new RegionFile(filename)
    printfn "%A" (PhotoToMinecraft.finalBmp = null)
    for x = 0 to PhotoToMinecraft.pictureBlockFilenames.GetLength(0)-1 do
        for z = 0 to PhotoToMinecraft.pictureBlockFilenames.GetLength(1)-1 do
            let filename = System.IO.Path.GetFileNameWithoutExtension(PhotoToMinecraft.pictureBlockFilenames.[x,z]).ToLower()
            let (_,bid,dmg) = textureFilenamesToBlockIDandDataMapping |> Array.find (fun (n,_,_) -> n = filename)
            regionFile.SetBlockIDAndDamage( x+64, 237, z+64, byte bid, byte dmg)
    regionFile.Write(filename+".new")

let dumpPlayerDat() =
    let file = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\New World\data\map_6.dat"""
    let nbt = readDatFile(file)
    printfn "%s" (nbt.ToString())
    (*
    // allow commands
    match nbt with
    | Compound(_,a) ->
        match a.[0] with
        | Compound("Data",a) ->
            match a |> Array.tryFindIndex (fun x -> x.Name="allowCommands") with
            | None -> ()
            | Some i ->
                a.[i] <- Byte("allowCommands",1uy)
    (*
    // replace LevelName
    match nbt with
    | Compound(_,a) ->
        match a.[0] with
        | Compound("Data",a) ->
            match a |> Array.tryFindIndex (fun x -> x.Name="LevelName") with
            | None -> ()
            | Some i ->
                a.[i] <- String("LevelName","MinecraftBINGOv2_5")
                *)
                (*
    // mesa bryce
    match nbt with
    | Compound(_,a) ->
        match a.[0] with
        | Compound("Data",a) ->
            match a |> Array.tryFindIndex (fun x -> x.Name="generatorOptions") with
            | None -> ()
            | Some i ->
                match a.[i] with
                | String(_,s) ->
                    let newOpts = s.Substring(0,s.IndexOf("fixedBiome")) + "fixedBiome\":165" + s.Substring(s.IndexOf("fixedBiome")+14)
                    a.[i] <- String("generatorOptions",newOpts)
    *)
    writeDatFile(file+".new", nbt)
    *)
////////////////////////////////////////////////////

let BrianEncodeKey = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!." |> Seq.toArray 

let encodeBlock(blockId:byte, data:byte) =
    assert(data <= 15uy)
    let lo6 = blockId &&& 63uy
    let hi2 = blockId >>> 6
    let x = (hi2 <<< 4) + data
    BrianEncodeKey.[int lo6], BrianEncodeKey.[int x]

let decodeBlock(c1, c2) =
    match BrianEncodeKey |> Array.tryFindIndex (fun x -> x=c1) with
    | Some lo6 ->
        match BrianEncodeKey |> Array.tryFindIndex (fun x -> x=c2) with
        | Some x ->
            let data = (byte x) &&& 15uy
            let hi2 = (byte x) >>> 4
            let blockId = (hi2 <<< 6) + (byte lo6)
            blockId, data
        | _ -> failwith "bad decode data"
    | _ -> failwith "bad decode data"

let readZoneIntoString(r:RegionFile,x,y,z,dx,dy,dz) =
    assert(dx>=0)
    assert(dx>=0)
    assert(dx>=0)
    let arr = ResizeArray()
    arr.Add(BrianEncodeKey.[dx])
    arr.Add(BrianEncodeKey.[dy])
    arr.Add(BrianEncodeKey.[dz])
    for i = 0 to dx do
        for j = 0 to dy do
            for k = 0 to dz do
                let bi = r.GetBlockInfo(x+i, y+j, z+k)
                let a,b = encodeBlock(bi.BlockID, bi.BlockData.Force())
                arr.Add(a)
                arr.Add(b)
    new System.String(arr |> Seq.toArray)

let writeZoneFromString(r:RegionFile,x,y,z,s:string) =
    assert(s.Length>=3)
    let dx = BrianEncodeKey |> Array.findIndex(fun c -> c=s.[0])
    let dy = BrianEncodeKey |> Array.findIndex(fun c -> c=s.[1])
    let dz = BrianEncodeKey |> Array.findIndex(fun c -> c=s.[2])
    let mutable strIndex = 3
    for i = 0 to dx do
        for j = 0 to dy do
            for k = 0 to dz do
                let bid, data = decodeBlock(s.[strIndex], s.[strIndex+1])
                r.SetBlockIDAndDamage(x+i, y+j, z+k, bid, data)
                strIndex <- strIndex + 2

let testing() =
    let fil = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Seed9917 - Copy35e\region\r.0.0.mca"""
    let r = new RegionFile(fil)
    let s = readZoneIntoString(r,71,208,67,16,1,16)
    printfn "%s" s
    writeZoneFromString(r,71,213,67,s)

    r.Write(fil+".new")
    System.IO.File.Delete(fil)
    System.IO.File.Move(fil+".new",fil)

let testing2() =
    let fil = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Seed9917 - Copy35e\region\r.0.0.mca"""
    let r = new RegionFile(fil)
    let arr = ResizeArray()
    for i = 0 to 1 do
        for j = 0 to 1 do
            let s = readZoneIntoString(r,64+i*64,208,64+j*64,63,0,63)
            //printfn "%s" s
            arr.Add(sprintf """let xxx%d%d = "%s" """ i j s)
    let writePath = """C:\Users\brianmcn\Documents\Visual Studio 2012\Projects\MinecraftMapManipulator\MinecraftMapManipulator\Tutorial.fsx"""
    System.IO.File.WriteAllLines(writePath, arr)


/////////////////////////////

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
    let bingoItems =
        [|
            [|  -1, "diamond", AA.diamond                   ; -1, "diamond_hoe", AA.diamond_hoe         ; -1, "diamond_axe", AA.diamond_axe         |]
            [|  -1, "bone", AA.bone                         ; -1, "bone", AA.bone                       ; -1, "bone", AA.bone                       |]
            [|  -1, "ender_pearl", AA.ender_pearl           ; -1, "ender_pearl", AA.ender_pearl         ; -1, "ender_pearl", AA.ender_pearl         |]
            [|  +0, "tallgrass", AA.deadbush                ; +2, "tallgrass", AA.fern                  ; -1, "vine", AA.vine                       |]
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
                let yCoord = 1
                let zCoord = 3+18*z
                writeZoneFromString(region, xCoord, yCoord, zCoord, art)
                uniqueArts <- uniqueArts.Add(art, (xCoord, yCoord, zCoord))
                x <- x + 1
                if x = 8 then
                    x <- 0
                    z <- z + 1
    let MAPX, MAPY, MAPZ = 1, 28, 1
    writeZoneFromString(region, MAPX, MAPY, MAPZ, AA.mapTopLeft)
    writeZoneFromString(region, MAPX+64, MAPY, MAPZ, AA.mapTopRight)
    writeZoneFromString(region, MAPX, MAPY, MAPZ+64, AA.mapBottomLeft)
    writeZoneFromString(region, MAPX+64, MAPY, MAPZ+64, AA.mapBottomRight)
    let uniqueArts = uniqueArts // make an immutable copy
    // per-item Bingo Command storage
    // Y axis - different difficulties
    // X axis - different item subsets
    // Z axis - command: item framex4, (testfor, clear)x4
    for x = 0 to bingoItems.Length-1 do
        for y = 0 to 2 do
            let dmg, id, art = bingoItems.[x].[y]
            let cmds = [|
                        for i = 0 to 3 do
                            yield U (sprintf "execute @e[tag=whereToPlaceItemFrame] ~ ~ ~ summon ItemFrame ~ ~ ~ {ItemRotation:0,Facing:%d,Item:{Count:1,id:minecraft:%s%s}}" i id (if dmg <> -1 then sprintf ",Damage:%d" dmg else ""))
                        for team in [| "Red"; "Blue"; "Green"; "Yellow" |] do
                            yield C (sprintf """testfor @a[team=%s] {Inventory:[{id:"minecraft:%s"%s}]}""" team id (if dmg <> -1 then sprintf ",Damage:%ds" dmg else ""))
                            yield C (sprintf """clear @a[team=%s] %s %d 1""" team id dmg)
                        let xx, yy, zz = uniqueArts.[art]
                        yield U (sprintf "execute @e[tag=whereToPlacePixelArt] ~ ~ ~ clone %d %d %d %d %d %d ~ ~ ~" xx yy zz (xx+16) (yy+1) (zz+16))
                       |]
            region.PlaceCommandBlocksStartingAt(3+x,10+y,3,cmds)

    // TODO snow blocks in art assets (e.g. glass bottle) is melting - Mojang bug? how fix?
    let cmdsInit =
        [|
        yield O ""
        // world init
        yield U "setworldspawn 3 4 12"
        yield U "gamerule commandBlockOutput false"
        yield U "gamerule doDaylightCycle false"
        yield U "gamerule keepInventory true"
        yield U "time set 500"
        yield U "weather clear 999999"
        yield U "scoreboard teams add Red"
        yield U "scoreboard teams option Red color red"
        yield U "scoreboard teams join Red @a"
        // make display walls for temp work
        yield U "fill 3 3 3 7 7 3 stone"
        yield U "fill 3 10 -4 30 12 -4 stone"
        // kill all entities
        yield U "kill @e[type=!Player]"
        // set up scoreboard objectives & initial values
        yield U "scoreboard objectives add Score dummy"
        yield U "scoreboard objectives setdisplay sidebar Score"
        yield U "scoreboard players set @a Score 0"
        yield U "scoreboard objectives add S dummy"
        yield U "scoreboard objectives add Calc dummy"
        yield U "scoreboard players set A Calc 1103515245"
        yield U "scoreboard players set C Calc 12345"
        yield U "scoreboard players set Two Calc 2"
        yield U "scoreboard players set TwoToSixteen Calc 65536"
        // ask for initial seed
        yield U """tellraw @a {"text":"CLICK HERE","clickEvent":{"action":"suggest_command","value":"/scoreboard players set Z Calc NNN"}}"""
        |]
    region.PlaceCommandBlocksWithLeadingSignStartingAt(3,3,10,cmdsInit,[|"init AECs";"and scores"|])

    //////////////////////////////////////////////
    // generate a preview of card with items frames on a wall
    //////////////////////////////////////////////
    let makePreviewWallInit() =
        [|
        yield U "scoreboard players set mpwCol S 1"
        yield U "summon ArmorStand 3 7 4 {NoGravity:1,Tags:[\"whereToPlaceItemFrame\"]}"
        |]
    let makePreviewWall() =
        [|
        // find one block to clone to 1 1 1 
        yield U "scoreboard players operation @e[tag=item] S -= next S"
        yield U "scoreboard players test which S 0 0"
        yield C     "execute @e[tag=item,score_S_min=0,score_S=0] ~ ~2 ~ clone ~ ~ ~ ~ ~ ~ 1 1 1"
        yield U "scoreboard players test which S 1 1"
        yield C     "execute @e[tag=item,score_S_min=0,score_S=0] ~ ~1 ~ clone ~ ~ ~ ~ ~ ~ 1 1 1"
        yield U "scoreboard players test which S 2 2"
        yield C     "execute @e[tag=item,score_S_min=0,score_S=0] ~ ~0 ~ clone ~ ~ ~ ~ ~ ~ 1 1 1"
        yield U "scoreboard players operation @e[tag=item] S += next S"
        // clone it and run it
        yield U "summon ArmorStand ~ ~ ~2 {Tags:[\"replaceme\"]}"
        yield U "execute @e[tag=replaceme] ~ ~ ~ clone 1 1 1 1 1 1 ~ ~ ~"
        yield U "say THIS SHOULD HAVE BEEN REPLACED"
        yield U "kill @e[tag=replaceme]"
        // move next spot on board
        yield U "tp @e[tag=whereToPlaceItemFrame] ~1 ~ ~"
        yield U "scoreboard players add mpwCol S 1"
        yield U "scoreboard players test mpwCol S 6 *"
        yield C "scoreboard players set mpwCol S 1"
        yield C "tp @e[tag=whereToPlaceItemFrame] ~-5 ~-1 ~"
        |]
    let makePreviewWallCleanup() =
        [|
            yield C "kill @e[tag=whereToPlaceItemFrame]"
        |]

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
        yield U "scoreboard players operation @e[tag=item] S -= next S"
        yield U "scoreboard players test which S 0 0"
        yield C     "execute @e[tag=item,score_S_min=0,score_S=0] ~ ~2 ~12 clone ~ ~ ~ ~ ~ ~ 1 1 1"
        yield U "scoreboard players test which S 1 1"
        yield C     "execute @e[tag=item,score_S_min=0,score_S=0] ~ ~1 ~12 clone ~ ~ ~ ~ ~ ~ 1 1 1"
        yield U "scoreboard players test which S 2 2"
        yield C     "execute @e[tag=item,score_S_min=0,score_S=0] ~ ~0 ~12 clone ~ ~ ~ ~ ~ ~ 1 1 1"
        yield U "scoreboard players operation @e[tag=item] S += next S"
        // clone it and run it
        yield U "summon ArmorStand ~ ~ ~2 {Tags:[\"replaceme\"]}"
        yield U "execute @e[tag=replaceme] ~ ~ ~ clone 1 1 1 1 1 1 ~ ~ ~"
        yield U "say THIS SHOULD HAVE BEEN REPLACED"
        yield U "kill @e[tag=replaceme]"
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
    // "constantly checking for getting bingo items" bit
    ///////////////////////
    let TIMER_CYCLE_LENGTH = 5  // TODO decide loop time length
    assert(TIMER_CYCLE_LENGTH > 1)
    let timerCmds = 
        [|
            O "fill 6 10 44 10 14 44 minecraft:redstone_block"
            P "scoreboard players add Time S 1"
            U (sprintf "scoreboard players test Time S %d %d" TIMER_CYCLE_LENGTH TIMER_CYCLE_LENGTH)
            C "scoreboard players set Time S 0"
        |]
    region.PlaceCommandBlocksStartingAt(3,10,45,timerCmds)

    // red team got-item-checker framework   // TODO 3 other team colors
    for x = 0 to 4 do
        for y = 0 to 4 do
            let checkerCmds = 
                [|
                    P "scoreboard players test Time S 0 0"   // might need to spread load across 0 to N-1 to avoid lag spikes?
                    U "say REPLACE ME testfor"
                    U "say REPLACE ME clear"
                    // TODO call on-item-get (fireworks sound, chat msg)
                    // TODO color
                    // TODO lockout logic
                    C "scoreboard players add @a[team=Red] Score 1"  // TODO hardcoded red
                    C "setblock ~ ~ ~-5 wool"  // the two-consecutive-tick issue doesn't apply as a result of the purple condition if N > 1
                |]
            region.PlaceCommandBlocksStartingAt(6+x,10+y,45,checkerCmds)

    let checkForItemsInit() =
        [|
        yield U "scoreboard players set cfiCol S 1"
        yield U "summon ArmorStand 6 14 45 {NoGravity:1,Tags:[\"whereToCloneCommandTo\"]}"
        |]
    let checkForItems() =
        [|
        // find one block to clone to 1 1 1 (testfor)
        yield U "scoreboard players operation @e[tag=item] S -= next S"
        yield U "scoreboard players test which S 0 0"
        yield C     "execute @e[tag=item,score_S_min=0,score_S=0] ~ ~2 ~ clone ~ ~ ~4 ~ ~ ~4 1 1 1"
        yield U "scoreboard players test which S 1 1"
        yield C     "execute @e[tag=item,score_S_min=0,score_S=0] ~ ~1 ~ clone ~ ~ ~4 ~ ~ ~4 1 1 1"
        yield U "scoreboard players test which S 2 2"
        yield C     "execute @e[tag=item,score_S_min=0,score_S=0] ~ ~0 ~ clone ~ ~ ~4 ~ ~ ~4 1 1 1"
        yield U "scoreboard players operation @e[tag=item] S += next S"
        // clone it
        yield U "execute @e[tag=whereToCloneCommandTo] ~ ~ ~1 clone 1 1 1 1 1 1 ~ ~ ~"
        // find one block to clone to 1 1 1 (clear)
        yield U "scoreboard players operation @e[tag=item] S -= next S"
        yield U "scoreboard players test which S 0 0"
        yield C     "execute @e[tag=item,score_S_min=0,score_S=0] ~ ~2 ~ clone ~ ~ ~5 ~ ~ ~5 1 1 1"
        yield U "scoreboard players test which S 1 1"
        yield C     "execute @e[tag=item,score_S_min=0,score_S=0] ~ ~1 ~ clone ~ ~ ~5 ~ ~ ~5 1 1 1"
        yield U "scoreboard players test which S 2 2"
        yield C     "execute @e[tag=item,score_S_min=0,score_S=0] ~ ~0 ~ clone ~ ~ ~5 ~ ~ ~5 1 1 1"
        yield U "scoreboard players operation @e[tag=item] S += next S"
        // clone it
        yield U "execute @e[tag=whereToCloneCommandTo] ~ ~ ~2 clone 1 1 1 1 1 1 ~ ~ ~"
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
        yield U "kill @e[tag=item]"
        for _i = 1 to bingoItems.Length do
            yield U "tp @e[tag=item] ~1 ~ ~"
            yield U "summon AreaEffectCloud 3 10 3 {Duration:999999,Tags:[\"item\"]}"
            yield U "scoreboard players add @e[tag=item] S 1"
        yield U "scoreboard players remove @e[tag=item] S 1"
        // init other vars
        yield U (sprintf "scoreboard players set remain S %d" bingoItems.Length)
        yield U "scoreboard players set I S 25"
        if sky then
            yield! makeActualCardInit()
            yield U "say generating card..."
        else
            yield! makePreviewWallInit()
            yield! checkForItemsInit()
        // prepare loop
        yield U "summon ArmorStand ~ ~ ~2 {NoGravity:1,Tags:[\"purple\"]}"
        yield U "execute @e[tag=purple] ~ ~ ~ blockdata ~ ~ ~ {auto:1b}"
        // loop - could be inlined, but not for now, to avoid too many blocks
        yield P ""
        if true then
            // pick which of 28 bingo sets
            // INLINE PRNG HERE
            yield U "scoreboard players operation Z Calc *= A Calc"
            yield U "scoreboard players operation Z Calc += C Calc"
            yield U "scoreboard players operation Z Calc *= Two Calc"  // mod 2^31
            yield U "scoreboard players operation Z Calc /= Two Calc"
            yield U "scoreboard players set K Calc 0"
            yield U "scoreboard players operation K Calc += Z Calc"
            yield U "scoreboard players operation K Calc *= Two Calc"
            yield U "scoreboard players operation K Calc /= Two Calc"
            yield U "scoreboard players operation K Calc /= TwoToSixteen Calc"   // upper 16 bits most random
            // mod remain
            yield U "scoreboard players set next S 0"
            yield U "scoreboard players operation next S += K Calc"
            yield U "scoreboard players operation next S %= remain S"
            yield U "scoreboard players operation next S += remain S"   // ensure non-negative
            yield U "scoreboard players operation next S %= remain S"
            // pick which of 3 items in that set
            // INLINE PRNG HERE
            yield U "scoreboard players operation Z Calc *= A Calc"
            yield U "scoreboard players operation Z Calc += C Calc"
            yield U "scoreboard players operation Z Calc *= Two Calc"  // mod 2^31
            yield U "scoreboard players operation Z Calc /= Two Calc"
            yield U "scoreboard players set K Calc 0"
            yield U "scoreboard players operation K Calc += Z Calc"
            yield U "scoreboard players operation K Calc *= Two Calc"
            yield U "scoreboard players operation K Calc /= Two Calc"
            yield U "scoreboard players operation K Calc /= TwoToSixteen Calc"   // upper 16 bits most random
            // mod 3
            yield U "scoreboard players set which S 0"
            yield U "scoreboard players set mod S 3"
            yield U "scoreboard players operation which S += K Calc"
            yield U "scoreboard players operation which S %= mod S"
            yield U "scoreboard players operation which S += mod S"   // ensure non-negative
            yield U "scoreboard players operation which S %= mod S"
            // ************* array-indexed calls within tick - clone a CCB further into this chain, scheduling is per-x-y-z
            // call some procedure(s) with this implicit state flowing in
            //  - next is a subset # (1-28)
            //  - which is a difficulty # (0-2)
            if sky then
                yield! makeActualCard()
            else
                yield! makePreviewWall()
                yield! checkForItems()
            // remove used item from remaining list
            yield U "scoreboard players operation @e[tag=item] S -= next S"
            yield U "kill @e[tag=item,score_S_min=0,score_S=0]"
            yield U "scoreboard players remove @e[tag=item,score_S_min=0] S 1"
            yield U "scoreboard players operation @e[tag=item] S += next S"
            yield U "scoreboard players remove remain S 1"
            yield U "scoreboard players remove I S 1"
            yield U "scoreboard players test I S * 1"      // testing 1, which is OBO, because loop runs 1 extra time after we turn off purple
            yield C "execute @e[tag=purple] ~ ~ ~ blockdata ~ ~ ~ {auto:0b}"
            yield C "blockdata ~ ~ ~1 {auto:1b}"  //  one tick later (after loop done)
            yield O "blockdata ~ ~ ~ {auto:0b}"
            yield C "kill @e[tag=purple]"
            if sky then
                yield! makeActualCardCleanup()
                yield U "say ...done!"
            else
                yield! makePreviewWallCleanup()
                yield! checkForItemsCleanup()
        |]
    region.PlaceCommandBlocksWithLeadingSignStartingAt(6,3,10,bingoCardMakerCmds(false),[|"set Z Calc";"to seed"|])
    region.PlaceCommandBlocksWithLeadingSignStartingAt(7,3,10,bingoCardMakerCmds(true),[|"set Z Calc";"to seed"|])
    let cloneIntoDisplayWallCmds =
        [|
            yield O ""
            for i = 1 to 28 do
                yield U (sprintf "clone %d 12 3 %d 12 3 12 3 %d" (2+i) (2+i) (2*i+11))
            for i = 1 to 28 do
                yield U (sprintf "clone %d 11 3 %d 11 3 12 3 %d" (2+i) (2+i) (2*i+11+56))
            for i = 1 to 28 do
                yield U (sprintf "clone %d 10 3 %d 10 3 12 3 %d" (2+i) (2+i) (2*i+11+56+56))
        |]
    region.PlaceCommandBlocksStartingAt(9,3,10,cloneIntoDisplayWallCmds)
    let displayWallCmds = 
        [|
            yield O ""
            yield U "summon ArmorStand ~ ~ ~ {NoGravity:1,Tags:[\"whereToPlaceItemFrame\"]}"
            for i = 1 to 28 do
                yield U (sprintf "tp @e[tag=whereToPlaceItemFrame] %d 12 -3" (2+i))
                yield U "say REPLACE"
            for i = 1 to 28 do
                yield U (sprintf "tp @e[tag=whereToPlaceItemFrame] %d 11 -3" (2+i))
                yield U "say REPLACE"
            for i = 1 to 28 do
                yield U (sprintf "tp @e[tag=whereToPlaceItemFrame] %d 10 -3" (2+i))
                yield U "say REPLACE"
        |]
    region.PlaceCommandBlocksStartingAt(12,3,10,displayWallCmds)

    region.Write(fil+".new")
    System.IO.File.Delete(fil)
    System.IO.File.Move(fil+".new",fil)




let placeCommandBlocksInTheWorldTemp(fil) =
    let region = new RegionFile(fil)
    let cmdsInit =
        [|
        yield P ""
        for _i = 0 to 199 do
            yield U "scoreboard players add Score S 1"
        |]
    region.PlaceCommandBlocksStartingAt(3,3,10,cmdsInit)

    region.Write(fil+".new")
    System.IO.File.Delete(fil)
    System.IO.File.Move(fil+".new",fil)

////////////////////////////////////////////////////


[<System.STAThread()>]  
do   
    //killAllEntities()
    //dumpChunkInfo("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\rrr\region\r.0.-3.mca""", 0, 31, 0, 31, true)
    //dumpSomeCommandBlocks("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\SnakeGameByLorgon111\region\r.0.0.mca""")
    //dumpSomeCommandBlocks("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\InstantReplay09\region\r.0.0.mca""")
    //dumpSomeCommandBlocks("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Mandelbrot 1_9\region\r.0.0.mca""")
    //dumpSomeCommandBlocks("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Learning\region\r.0.0.mca""")
    //diffRegionFiles("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\tmpy\region\r.0.0.mca""",
      //              """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\tmpy\region\r.0.0.mca.new""")
    //dumpSomeCommandBlocks("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Seed9917 - Copy35e\region\r.0.0.mca""")
    //placeCertainEntitiesInTheWorld()
    //dumpPlayerDat()
    //placeCertainBlocksInTheSky()
    //placeCommandBlocksInTheWorld("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\BingoConcepts\region\r.0.0.mca""")
    //placeCommandBlocksInTheWorld("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\tmp1\region\r.0.0.mca""")
    diffRegionFiles("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\BugRepro\region\r.0.0.mca""",
                    """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\BugRepro\region\r.0.0.mca.new""")
    //dumpSomeCommandBlocks("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\38a\region\r.0.0.mca""")
    //testing2()
    //placeCommandBlocksInTheWorldTemp("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\BugRepro\region\r.0.0.mca""")
