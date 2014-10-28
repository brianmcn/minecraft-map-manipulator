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
                | 10uy ->Compounds(Array.init len (fun _ -> readCompoundPayload()))
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
            | Compounds(a) -> bw.Write(10uy); bw.Write(a.Length); for x in a do (for y in x do y.Write(bw); assert(x.[x.Length-1] = End))
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

type BlockInfo(blockID:byte, blockData:byte, tileEntity:NBT option) =
    member this.BlockID = blockID
    member this.BlockData = blockData
    member this.TileEntity = tileEntity

type RegionFile(filename) =
    let rx, rz =
        let m = System.Text.RegularExpressions.Regex.Match(filename, """.*r\.(.*)\.(.*)\.mca$""")
        int m.Groups.[1].Value, int m.Groups.[2].Value
    let chunks : NBT[,] = Array2D.create 32 32 End  // End represents a blank (unrepresented) chunk
    let chunkTimestampInfos : int[,] = Array2D.zeroCreate 32 32
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
    member this.GetChunk(cx, cz) =
        match chunks.[cx,cz] with
        | End -> failwith "chunk not represented, NYI"
        | c -> c
    member this.GetBlockInfo(x, y, z) =
        let xxxx = if x < 0 then x - 512 else x
        let zzzz = if z < 0 then z - 512 else z
        if xxxx/512 <> rx || zzzz/512 <> rz then failwith "coords outside this region"
        let theChunk = 
            match chunks.[((x+51200)%512)/16,((z+51200)%512)/16] with
            | End -> failwith "chunk not represented, NYI"
            | c -> c
        let theChunk = match theChunk with Compound(_,[|c;_|]) -> c // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
        let sections = match theChunk.["Sections"] with List(_,Compounds(cs)) -> cs
        let theSection = sections |> Array.find (Array.exists (function Byte("Y",n) when n=byte(y/16) -> true | _ -> false))  // TODO cope with missing sections (air)
        let dx, dy, dz = (x+51200) % 16, y % 16, (z+51200) % 16
        let i = dy*256 + dz*16 + dx
        // BlockID
        let blocks = theSection |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
        // BlockData
        let blockData = theSection |> Array.pick (function ByteArray("Data",a) -> Some a | _ -> None)
        // expand 2048 half-bytes into 4096 for convenience of same indexing
        let blockData = Array.init 4096 (fun x -> if (x+51200)%2=1 then blockData.[x/2] >>> 4 else blockData.[x/2] &&& 0xFuy)
        // TileEntities
        let tileEntity = 
            match theChunk.["TileEntities"] with 
            | List(_,Compounds(cs)) ->
                let tes = cs |> Array.choose (fun te -> 
                    let te = Compound("unnamedDummyToCarryAPayload",te)
                    if te.["x"]=Int("x",x) && te.["y"]=Int("y",y) && te.["z"]=Int("z",z) then Some te else None)
                if tes.Length = 0 then None
                elif tes.Length = 1 then Some tes.[0]
                else failwith "unexpected: multiple TileEntities with same xyz coords"
            | _ -> None
        new BlockInfo(blocks.[i], blockData.[i], tileEntity)
    member this.SetBlockIDAndDamage(x, y, z, blockID, damage) =
        if (x+5120)/512 <> rx+10 || (z+5120)/512 <> rz+10 then failwith "coords outside this region"
        if damage > 15uy then failwith "invalid blockData"
        let theChunk = 
            let xx = ((x+5120)%512)/16
            let zz = ((z+5120)%512)/16
            match chunks.[xx,zz] with
            | End -> failwith "chunk not represented, NYI"
            | c -> c
        let theChunk = match theChunk with Compound(_,[|c;_|]) -> c // unwrap: almost every root tag has an empty name string and encapsulates only one Compound tag with the actual data and a name
        let sections = match theChunk.["Sections"] with List(_,Compounds(cs)) -> cs
        let theSection = sections |> Array.find (Array.exists (function Byte("Y",n) when n=byte(y/16) -> true | _ -> false))  // TODO cope with missing sections (air)
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

let readDatFile(filename : string) =
    use s = new System.IO.Compression.GZipStream(new System.IO.FileStream(filename, System.IO.FileMode.Open), System.IO.Compression.CompressionMode.Decompress)
    NBT.Read(new BinaryReader2(s))

let writeDatFile(filename : string, nbt : NBT) =
    use s = new System.IO.Compression.GZipStream(new System.IO.FileStream(filename, System.IO.FileMode.CreateNew), System.IO.Compression.CompressionMode.Compress)
    nbt.Write(new BinaryWriter2(s))

let main2() =
    let filename = """F:\.minecraft\saves\FindHut\region\r.0.0.mca"""
    // I want to read chunk (10,13), which is coords (165,211) and is in the witch hut
    let regionFile = new RegionFile(filename)
    let nbt = regionFile.GetChunk(10, 13)
    printfn "%s" (nbt.ToString())
    let theChunk = match nbt with Compound(_,[|c;_|]) -> c
    let sections = match theChunk.["Sections"] with List(_,Compounds(cs)) -> cs
    // height of map I want to look at is 65, which is section 4
    let s = sections |> Array.find (Array.exists (function Byte("Y",4uy) -> true | _ -> false))
    for x in s do printfn "%s\n" (x.ToString())
    let bi = regionFile.GetBlockInfo(165,65,211)
    printfn "%A %A %A" bi.BlockID bi.BlockData bi.TileEntity  // planks at floor of hut

    let structures = [ //"VILLAGES", """F:\.minecraft\saves\FindHut\data\villages.dat"""
                       //"TEMPLE", """F:\.minecraft\saves\E&T 1_3\data\Temple.dat"""
                       "TEMPLE", """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\E&T 1_3later\data\Temple.dat"""
                       //"TEMPLE", """F:\.minecraft\saves\FindHut\data\Temple.dat"""
                       //"MINESHAFT", """F:\.minecraft\saves\FindHut\data\Mineshaft.dat"""
                     ]
    printfn "E&Tafterupdate"
    for name, file in structures do
        printfn ""
        printfn "%s" name
        try
            let vnbt = readDatFile(file)
            printfn "%s" (vnbt.ToString())
        with e -> printfn "error %A" e
    printfn "======================="

    let MakeSyntheticWitchArea(lowX, lowY, lowZ, hiX, hiY, hiZ) = 
        let bb = [| lowX; lowY; lowZ; hiX; hiY; hiZ |]
        let chunkX = lowX / 16
        let chunkZ = lowZ / 16
        Compound("", [|
            Compound("data", [|
                Compound("Features", [|
                    Compound(sprintf "[%d,%d]" chunkX chunkZ, [|
                        String("id", "Temple")
                        Int("ChunkX", chunkX)
                        Int("ChunkZ", chunkZ)
                        IntArray("BB", bb)
                        List("Children", Compounds[|[|
                            String("id", "TeSH")
                            Int("GD", 0) //?
                            Int("HPos", -1)
                            IntArray("BB", bb)
                            Int("Height", hiY - lowY + 1)
                            Int("Width", hiX - lowX + 1)
                            Int("Depth", hiZ - lowZ + 1)
                            Int("Witch", 0) //?
                            Int("O", 1)  //?
                            End
                            |]|])
                        End
                        |])
                    End
                    |])
                End
                |])
            End
            |])

    let synthetic = MakeSyntheticWitchArea(651, 54, 651, 750, 83, 750)
    printfn "%s" (synthetic.ToString())
    use s = new System.IO.Compression.GZipStream(new System.IO.FileStream("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\E&T 1_3later\data\Temple.dat""", System.IO.FileMode.CreateNew), System.IO.Compression.CompressionMode.Compress)
    synthetic.Write(new BinaryWriter2(s))
    s.Close()
   
let dumpChunkInfo(regionFileName,cx,cX,cz,cZ) =
    let regionFile = new RegionFile(regionFileName)
    for x = cx to cX do
        for z = cz to cZ do
            let nbt = regionFile.GetChunk(x, z)
            let theChunk = match nbt with Compound(_,[|c;_|]) -> c
            printfn "%s" (theChunk.ToString())
            //System.Console.ReadKey() |> ignore
    System.Console.ReadKey() |> ignore

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
                    Compound(cname,a) ->
                        let i = a |> Array.findIndex (fun x -> match x with NBT.List("Entities",_) -> true | _ -> false)
                        a.[i] <- NBT.List("Entities",Compounds[||])
    regionFile.Write(filename+".new")

let dumpSomeCommandBlocks() =
    let aaa = ResizeArray()
    let hop = ResizeArray()
    let uuidStuff = ResizeArray()
//    for filename in ["""C:\Users\brianmcn\Desktop\bugged.r.0.0.mca"""
    for filename in ["""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\MinecraftBINGOv2_4update47\region\r.0.0.mca"""
//    for filename in ["""C:\Users\brianmcn\Desktop\pre3failure\pre3failure\region\r.0.0.mca"""
//    for filename in ["""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\MinecraftBINGOv2_4test01\region\r.0.0.mca"""
//    for filename in ["""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\PaintBling34\region\r.-2.2.mca"""
                     ] do
        let regionFile = new RegionFile(filename)
        let blockIDCounts = Array.zeroCreate 256
        for cx = 0 to 31 do
            for cz = 0 to 31 do
                try
                    let nbt = regionFile.GetChunk(cx, cz)
                    //printfn "%s" (nbt.ToString())
                    let theChunk = match nbt with Compound(_,[|c;_|]) -> c
        //            printfn "%s" theChunk.Name 
                    let biomeData = match theChunk.["Biomes"] with ByteArray(_n,a) -> a
        //            for i = 0 to 255 do biomeData.[i] <- 14uy // Moo
                    match theChunk.TryGetFromCompound("Entities") with 
                    | None -> ()
                    | Some e -> 
                        match e with
                        | NBT.List(_,Compounds(nbtarrarr)) ->
                            for ent in nbtarrarr do
                                let mutable l = 0L
                                let mutable m = 0L
                                let mutable s = ""
                                for nv in ent do
                                    match nv with
                                    | Long("UUIDLeast",x) -> l <- x
                                    | Long("UUIDMost",x) -> m <- x
                                    | String("id",n) -> s <- n
                                    | _ -> ()
                                //if l <> 0L then
                                if s="Ozelot" then
                                    uuidStuff.Add( (s,l,m) )   
                        //printfn "%s" (e.ToString())
                    match theChunk.TryGetFromCompound("TileEntities") with 
                    | None -> ()
                    | Some te -> 
                        //printfn "%s" (te.ToString())
                        match te with List(_,Compounds(tes)) ->
                        for t in tes do
                            if t |> Array.exists (function String("id","Hopper") -> true | _ -> false) then
                                let x = t |> Array.pick (function Int("x",i) -> Some(int i) | _ -> None)
                                let y = t |> Array.pick (function Int("y",i) -> Some(int i) | _ -> None)
                                let z = t |> Array.pick (function Int("z",i) -> Some(int i) | _ -> None)
                                hop.Add( (x,y,z) )
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
                            (*
                            if bid = 116uy then
                                // let i = dy*256 + dz*16 + dx
                                printfn "ench tab at (%d,%d,%d)" (regionFile.RX*512 + cx*16 + (i%16)) (ySection * 16 + (i/256)) (regionFile.RZ*512 + cz*16 + ((i%256)/16))
                            if bid = 137uy then
                                // let i = dy*256 + dz*16 + dx
                                printfn "comm at (%d,%d,%d)" (regionFile.RX*512 + cx*16 + (i%16)) (ySection * 16 + (i/256)) (regionFile.RZ*512 + cz*16 + ((i%256)/16))
                            *)
                with e ->
                    if e.Message.StartsWith("chunk not") then
                        () // yeah yeah
                    else
                        printfn "%A" e.Message 

    let uuidStuff = uuidStuff (* |> Seq.filter (fun (s,l,m) -> s = "MinecartRideable") *) |> Seq.toArray
    uuidStuff |> Array.sortInPlaceBy (fun (s,l,m) -> s,m)
    printfn "%A" uuidStuff

    printfn "There are %d hoppers" hop.Count 
    for (x,y,z) in hop do
        printfn "%5d,%5d,%5d" x y z

        
    printfn "There are %d command blocks" aaa.Count 
    let aaa = aaa.ToArray() |> Array.map (fun (s,x,y,z) -> let s = (if s.StartsWith("/") then s.Substring(1) else s) in s,x,y,z)
    //let aaa = aaa.ToArray() |> Array.filter (fun (comm,_,_,_) -> comm.StartsWith("say") || comm.StartsWith("tellraw"))
    //let aaa = aaa.ToArray() |> Array.filter (fun (comm,_,_,_) -> comm.StartsWith("say"))
    let aaa = aaa |> Array.filter (fun (comm,_,_,_) -> comm.ToLower().Contains("stats"))
    //let aaa = aaa |> Array.filter (fun (comm,_,_,_) -> comm.ToLower().Contains("kill"))
    //let aaa = aaa |> Array.filter (fun (comm,_,_,_) -> comm.ToLower().Contains("0001"))
    //let aaa = aaa |> Array.filter (fun (_,x,y,z) -> x>= -846 && x<= -802 && y=67 && z>=1400 && z<=1480)
    //let aaa = aaa |> Array.sortBy (fun (s,x,y,z) -> s.Split([|' '|]).[0],y,z,x) 
    let aaa = aaa |> Array.sortBy (fun (_s,x,y,z) -> y,z,x) 
    for (comm,x,y,z) in aaa do
        printfn "%5d,%5d,%5d : %s" x y z comm

//    printfn "============="
//    let bi = regionFile.GetBlockInfo(761,65,767)  // ench table
//    printfn "%A %A %A" bi.BlockID bi.BlockData bi.TileEntity  

//    printfn "============="
//    printfn "Block counts"
//    for i = 0 to blockIDCounts.Length-1 do
//        if blockIDCounts.[i] <> 0 then
//            printfn "%7d - %s" blockIDCounts.[i] (BLOCK_IDS |> Array.find (fun (n,_) -> n=i) |> snd)
//    printfn "============="

(*
    let copy = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\E&T 1_3later\region\copy.r.1.1.mca"""
    regionFile.Write(copy)
    let copyRegionFile = new RegionFile(copy)
    printfn "DIFFS:"
    for cx = 0 to 31 do
        for cz = 0 to 31 do
            let a = regionFile.GetChunk(cx,cz)
            let b = copyRegionFile.GetChunk(cx,cz)
            match a.Diff(b) with
            | None -> ()
            | Some(path,m,n) ->
                printfn "chunk (%d,%d) differs at %s" cx cz path
                printfn "%s" (m.ToString())
                printfn "----"
                printfn "%s" (n.ToString())
    printfn "(end of DIFFS)"
*)

    (*
    //let filename = """F:\.minecraft\saves\BingoGood\region\r.0.0.mca"""
    // I want to read chunk (12,12), which is coords (192,192) and is the top left of my bingo map
    let regionFile = new RegionFile(filename)
    let nbt = regionFile.GetChunk(12, 12)
    printfn "%s" (nbt.ToString())
    let theChunk = match nbt with Compound(_,[|c;_|]) -> c
    printfn "%s" theChunk.Name 
    let sections = match theChunk.["Sections"] with List(_,Compounds(cs)) -> cs
    printfn "%d" sections.Length 
    // height of map I want to look at is just below 208, which is section 12
    let s = sections |> Array.find (Array.exists (function Byte("Y",12uy) -> true | _ -> false))
    for x in s do printfn "%s\n" (x.ToString())
    let blocks = s |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
    // 76 is active redstone torch blockID; 23 is dispenser
    printfn "red torch? %A" (blocks |> Array.exists (fun b -> b = 76uy))
    // dispenser in next higher section (13)... i could see its contents already in the TileEntities of the chunk
    let s = sections |> Array.find (Array.exists (function Byte("Y",13uy) -> true | _ -> false))
    let blocks = s |> Array.pick (function ByteArray("Blocks",a) -> Some a | _ -> None)
    printfn "dispenser? %A" (blocks |> Array.exists (fun b -> b = 23uy))
    let dispIndex = blocks |> Array.findIndex (fun b -> b = 23uy)
    let blockData = s |> Array.pick (function ByteArray("Data",a) -> Some a | _ -> None)
    // expand 2048 half-bytes into 4096 for convenience of same indexing without having to decode YZX
    let blockData = Array.init 4096 (fun x -> if x%2=1 then blockData.[x/2] >>> 4 else blockData.[x/2] &&& 0xFuy)
    printfn "disp orientation %d" blockData.[dispIndex]
(*
    for i = 0 to 4095 do
        printf "%d " blocks.[i]
        if i%16 = 15 then printfn ""
*)
    let blockID, blockData, tileEntityOpt = regionFile.GetBlockIDAndBlockDataAndTileEntityOpt(204,208,199)
    printfn "should be dispenser (23) here: %d" blockID
    printfn "should be facing up and powered (9) here: %d" blockData
    printfn "should have tile entity here: %s" (match tileEntityOpt with None -> "" | Some nbt -> nbt.ToString())
    *)
    ()

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
    ()


// Look through statistics and achievements, discover what biomes MC thinks I have explored
let checkExploredBiomes() =
    let jsonSer = new System.Web.Script.Serialization.JavaScriptSerializer() // System.Web.Extensions.dll
    let jsonObj = jsonSer.DeserializeObject(System.IO.File.ReadAllText("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\Advtime\stats\6fbefbde-67a9-4f72-ab2d-2f3ee5439bc0.json"""))
    let dict : System.Collections.Generic.Dictionary<string,obj> = downcast jsonObj
    for kvp in dict do
        if kvp.Key.StartsWith("achievement.exploreAllBiomes") then
            let dict2 : System.Collections.Generic.Dictionary<string,obj> = downcast kvp.Value 
            let o = dict2.["progress"]
            let oa : obj[] = downcast o
            let sa = new ResizeArray<string>()
            for x in oa do
                sa.Add(downcast x)
            printfn "have %d, need %d" sa.Count BIOMES.Length 
            let biomeSet = BIOMES |> Array.map snd |> set
            printfn "%d" biomeSet.Count 
            let mine = sa |> set
            let unexplored = biomeSet - mine
            printfn "%d" unexplored.Count 
            for x in biomeSet do
                printfn "%s %s" (if mine.Contains(x) then "XXX" else "   ") x
            printfn "----"
            for x in mine do
                printfn "%s %s" (if biomeSet.Contains(x) then "XXX" else "   ") x

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


(*    
    | None -> printfn "hmmm"
    | Some data -> 
        match data.TryGetFromCompound("LevelName") with 
        | None -> printfn "yuck"
        | Some x -> printfn "%s" (x.ToString())
        *)


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
    //let bi = regionFile.GetBlockInfo(1, 50, 1)
    //printfn "%s" (bi.ToString())
    regionFile.Write(filename+".new")

let writeCommandBlocksFromATextFileIntoARegionFile(regionFilename,commandTextFilename) =
    let lines = System.IO.File.ReadAllLines(commandTextFilename)
    let regionFile = new RegionFile(regionFilename)
    let regex = System.Text.RegularExpressions.Regex(" *(-?\d+), *(-?\d+), *(-?\d+) : (.*)")
    for line in lines do
        let matches = regex.Match(line)
        //for m in matches.Groups do
        //    printfn "|%s|" m.Value
        //printfn ""
        let x = System.Int32.Parse(matches.Groups.[1].Value)
        let y = System.Int32.Parse(matches.Groups.[2].Value)
        let z = System.Int32.Parse(matches.Groups.[3].Value)
        let comm = matches.Groups.[4].Value
                    //let te = Compound("unnamedDummyToCarryAPayload",te)
                    //if te.["x"]=Int("x",x) && te.["y"]=Int("y",y) && te.["z"]=Int("z",z) then Some te else None)
        let bi = regionFile.GetBlockInfo(x,y,z)
        match bi.TileEntity with
        | Some(Compound(_,nbts)) ->
            //for nbt in nbts do
            //    printfn "%A" nbt
            let i = nbts |> Array.findIndex(function String("Command",_) -> true | _ -> false)
            nbts.[i] <- String("Command",comm)
        ()
    regionFile.Write(regionFilename+".new")

let dumpPlayerDat() =
    //let file = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\14w25a\data\Monument.dat"""
    //let file = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\New World\playerdata\6fbefbde-67a9-4f72-ab2d-2f3ee5439bc0.dat"""
    let file = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\w26b1\playerdata\6fbefbde-67a9-4f72-ab2d-2f3ee5439bc0.dat"""
    let nbt = readDatFile(file)
    printfn "%s" (nbt.ToString())

let makeArmorStand(x,y,z,headBlock,headDamage) =
            [|
                NBT.String("id","ArmorStand")
                NBT.Byte("NoGravity",1uy)
                NBT.Byte("Invisible",1uy)
                NBT.List("Pos",Payload.Doubles [|x;y;z|])
                NBT.List("Equipment",Payload.Compounds [|
                                [|NBT.End|];[|NBT.End|];[|NBT.End|];[|NBT.End|];
                                [|
                                    NBT.String("id",headBlock)
                                    NBT.Short("Damage",headDamage)
                                    NBT.End
                                |]
                            |])
                NBT.End
            |], (int x, int z)

let placeCertainEntitiesInTheWorld() =
    let filename = """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\MovingBlocks05\region\r.0.0.mca"""
    let regionFile = new RegionFile(filename)
    let HEADSIZE = 0.625
    
    (*
    let entities = [|
        makeArmorStand(105.0, 64.50, 84.0, "minecraft:stained_hardened_clay", 14s)
        makeArmorStand(105.65, 64.50, 84.0, "minecraft:stained_hardened_clay", 13s)
        makeArmorStand(119.0, 64.50, 84.0, "minecraft:stained_hardened_clay", 14s)
        makeArmorStand(119.65, 64.50, 84.0, "minecraft:stained_hardened_clay", 13s)
        makeArmorStand(119.0, 64.50, 104.0, "minecraft:stained_hardened_clay", 14s)
        makeArmorStand(119.65, 64.50, 104.0, "minecraft:stained_hardened_clay", 13s)
        |]
    *)

    let entities = ResizeArray()
    let maxHeight = 60.0 + float(PhotoToMinecraft.pictureBlockFilenames.GetLength(1)-1)*HEADSIZE
    for x = 0 to PhotoToMinecraft.pictureBlockFilenames.GetLength(0)-1 do
        for y = 0 to PhotoToMinecraft.pictureBlockFilenames.GetLength(1)-1 do
            let filename = System.IO.Path.GetFileNameWithoutExtension(PhotoToMinecraft.pictureBlockFilenames.[x,y]).ToLower()
            let (_,bid,dmg) = textureFilenamesToBlockIDandDataMappingForHeads |> Array.find (fun (n,_,_) -> n = filename)
            let (_,mid) = blockIdToMinecraftName |> Array.find (fun (n,s) -> n = bid)
            if y=21 && x=19 then
                printfn ""
            entities.Add( makeArmorStand(105.0 + HEADSIZE*(float x), maxHeight - (float y)*HEADSIZE, 84.0, mid, int16 dmg))

    for cx = 0 to 15 do
        for cz = 0 to 15 do
            let nbt = regionFile.GetChunk(cx, cz)
            match nbt with 
            Compound(_,[|theChunk;_|]) ->
                match theChunk.TryGetFromCompound("Entities") with 
                | None -> ()
                | Some _ -> 
                    match theChunk with 
                    Compound(cname,a) ->
                        let i = a |> Array.findIndex (fun x -> match x with NBT.List("Entities",_) -> true | _ -> false)
                        let es = entities |> Seq.choose (fun (e,(x,z)) -> if x/16=cx && z/16=cz then Some e else None) |> Seq.toArray 
                        a.[i] <- NBT.List("Entities",Compounds es)
    regionFile.Write(filename+".new")

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
   

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Input
open System.Windows.Media

// each node is either
//  - same in both (white)
//  - added (yellow)
//  - removed (red)
//  - same name, diff value (...)
// will need good canonical sort for list of compounds (e.g. id/uuids for entities, x/y/z for tileentities, etc)

let rec MakeTreeDiff (Compound(_,x) as xp) (Compound(_,y) as yp) (tvp:TreeViewItem) =
    let hasDiff = ref false
    let xnames = x |> Array.map (fun z -> z.Name) |> set
    let ynames = y |> Array.map (fun z -> z.Name) |> set
    let names = (Set.union xnames ynames).Remove(END_NAME)
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
                                    let tvj = new TreeViewItem(Header=".")
                                    let BODY(i) =
                                        if i = -1 then
                                            MakeTreeDiff (Compound("",x)) (Compound("",[||])) tvj |> ignore
                                            tvj.Background <- Brushes.Red
                                            tvi.Background <- Brushes.Orange
                                            hasDiff := true
                                        else
                                            let y = ya.[i]
                                            ya.RemoveAt(i)
                                            if MakeTreeDiff (Compound("",x)) (Compound("",y)) tvj then
                                                tvj.Background <- Brushes.Orange
                                                tvi.Background <- Brushes.Orange
                                                hasDiff := true
                                    if x |> Array.exists (fun e -> e.Name="UUIDLeast") &&
                                       x |> Array.exists (fun e -> e.Name="UUIDMost") then
                                        let xuuidl = x |> Seq.pick (function (Long("UUIDLeast",v)) -> Some v | _ -> None)
                                        let xuuidm = x |> Seq.pick (function (Long("UUIDMost",v)) -> Some v | _ -> None)
                                        let i = ya.FindIndex(fun ee -> 
                                                    ee |> Array.exists (function (Long("UUIDLeast",v)) -> v=xuuidl | _ -> false) &&
                                                    ee |> Array.exists (function (Long("UUIDMost",v)) -> v=xuuidm | _ -> false))
                                        BODY(i)
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
                                    // TODO other kinds of heuristic matches?
                                    else
                                        // TODO exact matches?
                                        MakeTreeDiff (Compound("",x)) (Compound("",[||])) tvj |> ignore
                                        tvj.Background <- Brushes.Red
                                        tvi.Background <- Brushes.Orange
                                        hasDiff := true
                                    tvi.Items.Add(tvj) |> ignore
                                for y in ya do
                                    let tvj = new TreeViewItem(Header=".")
                                    MakeTreeDiff (Compound("",[||])) (Compound("",y)) tvj |> ignore
                                    tvj.Background <- Brushes.Yellow
                                    tvi.Background <- Brushes.Orange
                                    hasDiff := true
                                    tvi.Items.Add(tvj) |> ignore
                            | _ -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
                        | _ -> tvi.Items.Add(new TreeViewItem(Header="TODO",Background=Brushes.Blue)) |> ignore
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

let TestTreeView() =
    let tv = new TreeView()
    let n = new TreeViewItem(Header="Top")
    n.Background <- Brushes.Blue 
    n.Items.Add(new TreeViewItem(Header="Mid")) |> ignore
    tv.Items.Add(n) |> ignore
    let window = new Window(Title="NBT Difference viewer by Dr. Brian Lorgon111", Content=tv)
    let app = new Application()
    app.Run(window) |> ignore

let diffChunk(regionFile1,regionFile2,cx,cz) =
    let tv = new TreeView()
    let n = new TreeViewItem(Header="Chunk "+cx.ToString()+","+cz.ToString())
    tv.Items.Add(n) |> ignore
    let r1 = new RegionFile(regionFile1)
    let r2 = new RegionFile(regionFile2)
    let c1 = r1.GetChunk(cx,cz)
    let c2 = r2.GetChunk(cx,cz)
    if MakeTreeDiff c1 c2 n then
        n.Background <- Brushes.Orange 
    let window = new Window(Title="NBT Difference viewer by Dr. Brian Lorgon111", Content=tv)
    let app = new Application()
    app.Run(window) |> ignore


////////////////////////////////////////////////////

[<System.STAThread()>]  
do   
    //printfn "hi"
    //placeCertainBlocksInTheWorld()
    //renamer()
    //dumpSomeCommandBlocks()
    //TestTreeView()
(*
    diffChunk("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\MinecraftBINGOv2_4update46\region\r.0.0.mca""",
              """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\MinecraftBINGOv2_4update47\region\r.0.0.mca""",
              8, 8)
*)
    diffChunk("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\TryToReproduceBug4\region\r.0.0.mca""",
              """C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\TryToReproduceBug5\region\r.0.0.mca""",
              8, 8)

    //killAllEntities()
    //main2()
    //testReadWriteRegionFile()
    //checkExploredBiomes()
    //dumpChunkInfo("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\MovingBlocks03\region\r.0.0.mca""", 6, 6, 6, 6)
    //placeCertainEntitiesInTheWorld()
    //dumpPlayerDat()
    //writeCommandBlocksFromATextFileIntoARegionFile("""C:\Users\brianmcn\AppData\Roaming\.minecraft\saves\PaintBling19\region\r.-2.2.mca""", """C:\Users\brianmcn\Desktop\pbcomm.txt""")
    ()
