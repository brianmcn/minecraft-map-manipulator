﻿module MC_Constants

let COLORS = [|
    "white"
    "orange"
    "magenta"
    "light_blue"
    "yellow"
    "lime"
    "pink"
    "gray"
    "light_gray"
    "cyan"
    "purple"
    "blue"
    "brown"
    "green"
    "red"
    "black"
    |]

// todo see if this is still up to date at release
let ALL_GIVEABLE_ITEM_IDS = [|
    "stone"
    "granite"
    "polished_granite"
    "diorite"
    "polished_diorite"
    "andesite"
    "polished_andesite"
    "grass_block"
    "dirt"
    "coarse_dirt"
    "podzol"
    "cobblestone"
    "oak_planks"
    "spruce_planks"
    "birch_planks"
    "jungle_planks"
    "acacia_planks"
    "dark_oak_planks"
    "oak_sapling"
    "spruce_sapling"
    "birch_sapling"
    "jungle_sapling"
    "acacia_sapling"
    "dark_oak_sapling"
    "bedrock"
    "sand"
    "red_sand"
    "gravel"
    "gold_ore"
    "iron_ore"
    "coal_ore"
    "oak_log"
    "spruce_log"
    "birch_log"
    "jungle_log"
    "acacia_log"
    "dark_oak_log"
    "oak_bark"
    "spruce_bark"
    "birch_bark"
    "jungle_bark"
    "acacia_bark"
    "dark_oak_bark"
    "oak_leaves"
    "spruce_leaves"
    "birch_leaves"
    "jungle_leaves"
    "acacia_leaves"
    "dark_oak_leaves"
    "sponge"
    "wet_sponge"
    "glass"
    "lapis_ore"
    "lapis_block"
    "dispenser"
    "sandstone"
    "chiseled_sandstone"
    "cut_sandstone"
    "note_block"
    "powered_rail"
    "detector_rail"
    "sticky_piston"
    "cobweb"
    "grass"
    "fern"
    "dead_bush"
    "piston"
    "white_wool"
    "orange_wool"
    "magenta_wool"
    "light_blue_wool"
    "yellow_wool"
    "lime_wool"
    "pink_wool"
    "gray_wool"
    "light_gray_wool"
    "cyan_wool"
    "purple_wool"
    "blue_wool"
    "brown_wool"
    "green_wool"
    "red_wool"
    "black_wool"
    "dandelion"
    "poppy"
    "blue_orchid"
    "allium"
    "azure_bluet"
    "red_tulip"
    "orange_tulip"
    "white_tulip"
    "pink_tulip"
    "oxeye_daisy"
    "brown_mushroom"
    "red_mushroom"
    "gold_block"
    "iron_block"
    "oak_slab"
    "spruce_slab"
    "birch_slab"
    "jungle_slab"
    "acacia_slab"
    "dark_oak_slab"
    "stone_slab"
    "sandstone_slab"
    "petrified_oak_slab"
    "cobblestone_slab"
    "brick_slab"
    "stone_brick_slab"
    "nether_brick_slab"
    "quartz_slab"
    "red_sandstone_slab"
    "purpur_slab"
    "smooth_quartz"
    "smooth_red_sandstone"
    "smooth_sandstone"
    "smooth_stone"
    "bricks"
    "tnt"
    "bookshelf"
    "mossy_cobblestone"
    "obsidian"
    "torch"
    "end_rod"
    "chorus_plant"
    "chorus_flower"
    "purpur_block"
    "purpur_pillar"
    "purpur_stairs"
    "mob_spawner"
    "oak_stairs"
    "chest"
    "diamond_ore"
    "diamond_block"
    "crafting_table"
    "farmland"
    "furnace"
    "ladder"
    "rail"
    "cobblestone_stairs"
    "lever"
    "stone_pressure_plate"
    "oak_pressure_plate"
    "spruce_pressure_plate"
    "birch_pressure_plate"
    "jungle_pressure_plate"
    "acacia_pressure_plate"
    "dark_oak_pressure_plate"
    "redstone_ore"
    "redstone_torch"
    "stone_button"
    "snow"
    "ice"
    "snow_block"
    "cactus"
    "clay"
    "jukebox"
    "oak_fence"
    "spruce_fence"
    "birch_fence"
    "jungle_fence"
    "acacia_fence"
    "dark_oak_fence"
    "pumpkin"
    "carved_pumpkin"
    "netherrack"
    "soul_sand"
    "glowstone"
    "jack_o_lantern"
    "oak_trapdoor"
    "spruce_trapdoor"
    "birch_trapdoor"
    "jungle_trapdoor"
    "acacia_trapdoor"
    "dark_oak_trapdoor"
    "infested_stone"
    "infested_cobblestone"
    "infested_stone_bricks"
    "infested_mossy_stone_bricks"
    "infested_cracked_stone_bricks"
    "infested_chiseled_stone_bricks"
    "stone_bricks"
    "mossy_stone_bricks"
    "cracked_stone_bricks"
    "chiseled_stone_bricks"
    "brown_mushroom_block"
    "red_mushroom_block"
    "mushroom_stem"
    "iron_bars"
    "glass_pane"
    "melon_block"
    "vine"
    "oak_fence_gate"
    "spruce_fence_gate"
    "birch_fence_gate"
    "jungle_fence_gate"
    "acacia_fence_gate"
    "dark_oak_fence_gate"
    "brick_stairs"
    "stone_brick_stairs"
    "mycelium"
    "lily_pad"
    "nether_bricks"
    "nether_brick_fence"
    "nether_brick_stairs"
    "enchanting_table"
    "end_portal_frame"
    "end_stone"
    "end_stone_bricks"
    "dragon_egg"
    "redstone_lamp"
    "sandstone_stairs"
    "emerald_ore"
    "ender_chest"
    "tripwire_hook"
    "emerald_block"
    "spruce_stairs"
    "birch_stairs"
    "jungle_stairs"
    "command_block"
    "beacon"
    "cobblestone_wall"
    "mossy_cobblestone_wall"
    "oak_button"
    "spruce_button"
    "birch_button"
    "jungle_button"
    "acacia_button"
    "dark_oak_button"
    "anvil"
    "chipped_anvil"
    "damaged_anvil"
    "trapped_chest"
    "light_weighted_pressure_plate"
    "heavy_weighted_pressure_plate"
    "daylight_detector"
    "redstone_block"
    "nether_quartz_ore"
    "hopper"
    "chiseled_quartz_block"
    "quartz_block"
    "quartz_pillar"
    "quartz_stairs"
    "activator_rail"
    "dropper"
    "white_terracotta"
    "orange_terracotta"
    "magenta_terracotta"
    "light_blue_terracotta"
    "yellow_terracotta"
    "lime_terracotta"
    "pink_terracotta"
    "gray_terracotta"
    "light_gray_terracotta"
    "cyan_terracotta"
    "purple_terracotta"
    "blue_terracotta"
    "brown_terracotta"
    "green_terracotta"
    "red_terracotta"
    "black_terracotta"
    "barrier"
    "iron_trapdoor"
    "hay_block"
    "white_carpet"
    "orange_carpet"
    "magenta_carpet"
    "light_blue_carpet"
    "yellow_carpet"
    "lime_carpet"
    "pink_carpet"
    "gray_carpet"
    "light_gray_carpet"
    "cyan_carpet"
    "purple_carpet"
    "blue_carpet"
    "brown_carpet"
    "green_carpet"
    "red_carpet"
    "black_carpet"
    "terracotta"
    "coal_block"
    "packed_ice"
    "acacia_stairs"
    "dark_oak_stairs"
    "slime_block"
    "grass_path"
    "sunflower"
    "lilac"
    "rose_bush"
    "peony"
    "tall_grass"
    "large_fern"
    "white_stained_glass"
    "orange_stained_glass"
    "magenta_stained_glass"
    "light_blue_stained_glass"
    "yellow_stained_glass"
    "lime_stained_glass"
    "pink_stained_glass"
    "gray_stained_glass"
    "light_gray_stained_glass"
    "cyan_stained_glass"
    "purple_stained_glass"
    "blue_stained_glass"
    "brown_stained_glass"
    "green_stained_glass"
    "red_stained_glass"
    "black_stained_glass"
    "white_stained_glass_pane"
    "orange_stained_glass_pane"
    "magenta_stained_glass_pane"
    "light_blue_stained_glass_pane"
    "yellow_stained_glass_pane"
    "lime_stained_glass_pane"
    "pink_stained_glass_pane"
    "gray_stained_glass_pane"
    "light_gray_stained_glass_pane"
    "cyan_stained_glass_pane"
    "purple_stained_glass_pane"
    "blue_stained_glass_pane"
    "brown_stained_glass_pane"
    "green_stained_glass_pane"
    "red_stained_glass_pane"
    "black_stained_glass_pane"
    "prismarine"
    "prismarine_bricks"
    "dark_prismarine"
    "sea_lantern"
    "red_sandstone"
    "chiseled_red_sandstone"
    "cut_red_sandstone"
    "red_sandstone_stairs"
    "repeating_command_block"
    "chain_command_block"
    "magma_block"
    "nether_wart_block"
    "red_nether_bricks"
    "bone_block"
    "structure_void"
    "observer"
    "white_shulker_box"
    "orange_shulker_box"
    "magenta_shulker_box"
    "light_blue_shulker_box"
    "yellow_shulker_box"
    "lime_shulker_box"
    "pink_shulker_box"
    "gray_shulker_box"
    "light_gray_shulker_box"
    "cyan_shulker_box"
    "purple_shulker_box"
    "blue_shulker_box"
    "brown_shulker_box"
    "green_shulker_box"
    "red_shulker_box"
    "black_shulker_box"
    "white_glazed_terracotta"
    "orange_glazed_terracotta"
    "magenta_glazed_terracotta"
    "light_blue_glazed_terracotta"
    "yellow_glazed_terracotta"
    "lime_glazed_terracotta"
    "pink_glazed_terracotta"
    "gray_glazed_terracotta"
    "light_gray_glazed_terracotta"
    "cyan_glazed_terracotta"
    "purple_glazed_terracotta"
    "blue_glazed_terracotta"
    "brown_glazed_terracotta"
    "green_glazed_terracotta"
    "red_glazed_terracotta"
    "black_glazed_terracotta"
    "white_concrete"
    "orange_concrete"
    "magenta_concrete"
    "light_blue_concrete"
    "yellow_concrete"
    "lime_concrete"
    "pink_concrete"
    "gray_concrete"
    "light_gray_concrete"
    "cyan_concrete"
    "purple_concrete"
    "blue_concrete"
    "brown_concrete"
    "green_concrete"
    "red_concrete"
    "black_concrete"
    "white_concrete_powder"
    "orange_concrete_powder"
    "magenta_concrete_powder"
    "light_blue_concrete_powder"
    "yellow_concrete_powder"
    "lime_concrete_powder"
    "pink_concrete_powder"
    "gray_concrete_powder"
    "light_gray_concrete_powder"
    "cyan_concrete_powder"
    "purple_concrete_powder"
    "blue_concrete_powder"
    "brown_concrete_powder"
    "green_concrete_powder"
    "red_concrete_powder"
    "black_concrete_powder"
    "iron_door"
    "oak_door"
    "spruce_door"
    "birch_door"
    "jungle_door"
    "acacia_door"
    "dark_oak_door"
    "repeater"
    "comparator"
    "structure_block"
    "iron_shovel"
    "iron_pickaxe"
    "iron_axe"
    "flint_and_steel"
    "apple"
    "bow"
    "arrow"
    "coal"
    "charcoal"
    "diamond"
    "iron_ingot"
    "gold_ingot"
    "iron_sword"
    "wooden_sword"
    "wooden_shovel"
    "wooden_pickaxe"
    "wooden_axe"
    "stone_sword"
    "stone_shovel"
    "stone_pickaxe"
    "stone_axe"
    "diamond_sword"
    "diamond_shovel"
    "diamond_pickaxe"
    "diamond_axe"
    "stick"
    "bowl"
    "mushroom_stew"
    "golden_sword"
    "golden_shovel"
    "golden_pickaxe"
    "golden_axe"
    "string"
    "feather"
    "gunpowder"
    "wooden_hoe"
    "stone_hoe"
    "iron_hoe"
    "diamond_hoe"
    "golden_hoe"
    "wheat_seeds"
    "wheat"
    "bread"
    "leather_helmet"
    "leather_chestplate"
    "leather_leggings"
    "leather_boots"
    "chainmail_helmet"
    "chainmail_chestplate"
    "chainmail_leggings"
    "chainmail_boots"
    "iron_helmet"
    "iron_chestplate"
    "iron_leggings"
    "iron_boots"
    "diamond_helmet"
    "diamond_chestplate"
    "diamond_leggings"
    "diamond_boots"
    "golden_helmet"
    "golden_chestplate"
    "golden_leggings"
    "golden_boots"
    "flint"
    "porkchop"
    "cooked_porkchop"
    "painting"
    "golden_apple"
    "enchanted_golden_apple"
    "sign"
    "bucket"
    "water_bucket"
    "lava_bucket"
    "minecart"
    "saddle"
    "redstone"
    "snowball"
    "oak_boat"
    "leather"
    "milk_bucket"
    "brick"
    "clay_ball"
    "sugar_cane"
    "paper"
    "book"
    "slime_ball"
    "chest_minecart"
    "furnace_minecart"
    "egg"
    "compass"
    "fishing_rod"
    "clock"
    "glowstone_dust"
    "cod"
    "salmon"
    "clownfish"
    "pufferfish"
    "cooked_cod"
    "cooked_salmon"
    "ink_sac"
    "rose_red"
    "cactus_green"
    "cocoa_beans"
    "lapis_lazuli"
    "purple_dye"
    "cyan_dye"
    "light_gray_dye"
    "gray_dye"
    "pink_dye"
    "lime_dye"
    "dandelion_yellow"
    "light_blue_dye"
    "magenta_dye"
    "orange_dye"
    "bone_meal"
    "bone"
    "sugar"
    "cake"
    "white_bed"
    "orange_bed"
    "magenta_bed"
    "light_blue_bed"
    "yellow_bed"
    "lime_bed"
    "pink_bed"
    "gray_bed"
    "light_gray_bed"
    "cyan_bed"
    "purple_bed"
    "blue_bed"
    "brown_bed"
    "green_bed"
    "red_bed"
    "black_bed"
    "cookie"
    "filled_map"
    "shears"
    "melon"
    "pumpkin_seeds"
    "melon_seeds"
    "beef"
    "cooked_beef"
    "chicken"
    "cooked_chicken"
    "rotten_flesh"
    "ender_pearl"
    "blaze_rod"
    "ghast_tear"
    "gold_nugget"
    "nether_wart"
    "potion"
    "glass_bottle"
    "spider_eye"
    "fermented_spider_eye"
    "blaze_powder"
    "magma_cream"
    "brewing_stand"
    "cauldron"
    "ender_eye"
    "speckled_melon"
    "bat_spawn_egg"
    "blaze_spawn_egg"
    "cave_spider_spawn_egg"
    "chicken_spawn_egg"
    "cow_spawn_egg"
    "creeper_spawn_egg"
    "donkey_spawn_egg"
    "elder_guardian_spawn_egg"
    "enderman_spawn_egg"
    "endermite_spawn_egg"
    "evocation_illager_spawn_egg"
    "ghast_spawn_egg"
    "guardian_spawn_egg"
    "horse_spawn_egg"
    "husk_spawn_egg"
    "llama_spawn_egg"
    "magma_cube_spawn_egg"
    "mooshroom_spawn_egg"
    "mule_spawn_egg"
    "ocelot_spawn_egg"
    "parrot_spawn_egg"
    "pig_spawn_egg"
    "polar_bear_spawn_egg"
    "rabbit_spawn_egg"
    "sheep_spawn_egg"
    "shulker_spawn_egg"
    "silverfish_spawn_egg"
    "skeleton_spawn_egg"
    "skeleton_horse_spawn_egg"
    "slime_spawn_egg"
    "spider_spawn_egg"
    "squid_spawn_egg"
    "stray_spawn_egg"
    "vex_spawn_egg"
    "villager_spawn_egg"
    "vindication_illager_spawn_egg"
    "witch_spawn_egg"
    "wither_skeleton_spawn_egg"
    "wolf_spawn_egg"
    "zombie_spawn_egg"
    "zombie_horse_spawn_egg"
    "zombie_pigman_spawn_egg"
    "zombie_villager_spawn_egg"
    "experience_bottle"
    "fire_charge"
    "writable_book"
    "written_book"
    "emerald"
    "item_frame"
    "flower_pot"
    "carrot"
    "potato"
    "baked_potato"
    "poisonous_potato"
    "map"
    "golden_carrot"
    "skeleton_skull"
    "wither_skeleton_skull"
    "player_head"
    "zombie_head"
    "creeper_head"
    "dragon_head"
    "carrot_on_a_stick"
    "nether_star"
    "pumpkin_pie"
    "firework_rocket"
    "firework_star"
    "enchanted_book"
    "nether_brick"
    "quartz"
    "tnt_minecart"
    "hopper_minecart"
    "prismarine_shard"
    "prismarine_crystals"
    "rabbit"
    "cooked_rabbit"
    "rabbit_stew"
    "rabbit_foot"
    "rabbit_hide"
    "armor_stand"
    "iron_horse_armor"
    "golden_horse_armor"
    "diamond_horse_armor"
    "lead"
    "name_tag"
    "command_block_minecart"
    "mutton"
    "cooked_mutton"
    "white_banner"
    "orange_banner"
    "magenta_banner"
    "light_blue_banner"
    "yellow_banner"
    "lime_banner"
    "pink_banner"
    "gray_banner"
    "light_gray_banner"
    "cyan_banner"
    "purple_banner"
    "blue_banner"
    "brown_banner"
    "green_banner"
    "red_banner"
    "black_banner"
    "end_crystal"
    "chorus_fruit"
    "chorus_fruit_popped"
    "beetroot"
    "beetroot_seeds"
    "beetroot_soup"
    "dragon_breath"
    "splash_potion"
    "spectral_arrow"
    "tipped_arrow"
    "lingering_potion"
    "shield"
    "elytra"
    "spruce_boat"
    "birch_boat"
    "jungle_boat"
    "acacia_boat"
    "dark_oak_boat"
    "totem_of_undying"
    "shulker_shell"
    "iron_nugget"
    "knowledge_book"
    "debug_stick"
    "music_disc_13"
    "music_disc_cat"
    "music_disc_blocks"
    "music_disc_chirp"
    "music_disc_far"
    "music_disc_mall"
    "music_disc_mellohi"
    "music_disc_stal"
    "music_disc_strad"
    "music_disc_ward"
    "music_disc_11"
    "music_disc_wait"
    |]

// TODO re-curate this list neat 1.13 release
let SURVIVAL_UNOBTAINABLE_ITEM_IDS = [|
    "bedrock"
    "petrified_oak_slab"
    "chorus_plant"
    "mob_spawner"
    "farmland"
    "infested_stone"
    "infested_cobblestone"
    "infested_stone_bricks"
    "infested_mossy_stone_bricks"
    "infested_cracked_stone_bricks"
    "infested_chiseled_stone_bricks"
    "end_portal_frame"
    "command_block"
    "barrier"
    "grass_path"
    "repeating_command_block"
    "chain_command_block"
    "structure_void"
    "structure_block"
    "bat_spawn_egg"
    "blaze_spawn_egg"
    "cave_spider_spawn_egg"
    "chicken_spawn_egg"
    "cow_spawn_egg"
    "creeper_spawn_egg"
    "donkey_spawn_egg"
    "elder_guardian_spawn_egg"
    "enderman_spawn_egg"
    "endermite_spawn_egg"
    "evocation_illager_spawn_egg"
    "ghast_spawn_egg"
    "guardian_spawn_egg"
    "horse_spawn_egg"
    "husk_spawn_egg"
    "llama_spawn_egg"
    "magma_cube_spawn_egg"
    "mooshroom_spawn_egg"
    "mule_spawn_egg"
    "ocelot_spawn_egg"
    "parrot_spawn_egg"
    "pig_spawn_egg"
    "polar_bear_spawn_egg"
    "rabbit_spawn_egg"
    "sheep_spawn_egg"
    "shulker_spawn_egg"
    "silverfish_spawn_egg"
    "skeleton_spawn_egg"
    "skeleton_horse_spawn_egg"
    "slime_spawn_egg"
    "spider_spawn_egg"
    "squid_spawn_egg"
    "stray_spawn_egg"
    "vex_spawn_egg"
    "villager_spawn_egg"
    "vindication_illager_spawn_egg"
    "witch_spawn_egg"
    "wither_skeleton_spawn_egg"
    "wolf_spawn_egg"
    "zombie_spawn_egg"
    "zombie_horse_spawn_egg"
    "zombie_pigman_spawn_egg"
    "zombie_villager_spawn_egg"
    "player_head"
    "command_block_minecart"
    "knowledge_book"
    "debug_stick"
    |]

let SURVIVAL_OBTAINABLE_ITEM_IDS = ALL_GIVEABLE_ITEM_IDS |> Array.filter (fun x -> not(SURVIVAL_UNOBTAINABLE_ITEM_IDS |> Array.contains(x)))


let ITEMS_THAT_DO_NOT_STACK_TO_64_OR_MIGHT_HAVE_METADATA = [|
    "mob_spawner"
    "chest"
    "furnace"
    "jukebox"
    "dragon_egg"
    "command_block"
    "trapped_chest"
    "hopper"
    "dropper"
    "repeating_command_block"
    "chain_command_block"
    "white_shulker_box"
    "orange_shulker_box"
    "magenta_shulker_box"
    "light_blue_shulker_box"
    "yellow_shulker_box"
    "lime_shulker_box"
    "pink_shulker_box"
    "gray_shulker_box"
    "light_gray_shulker_box"
    "cyan_shulker_box"
    "purple_shulker_box"
    "blue_shulker_box"
    "brown_shulker_box"
    "green_shulker_box"
    "red_shulker_box"
    "black_shulker_box"
    "structure_block"
    "iron_shovel"
    "iron_pickaxe"
    "iron_axe"
    "flint_and_steel"
    "bow"
    "iron_sword"
    "wooden_sword"
    "wooden_shovel"
    "wooden_pickaxe"
    "wooden_axe"
    "stone_sword"
    "stone_shovel"
    "stone_pickaxe"
    "stone_axe"
    "diamond_sword"
    "diamond_shovel"
    "diamond_pickaxe"
    "diamond_axe"
    "bowl"
    "mushroom_stew"
    "golden_sword"
    "golden_shovel"
    "golden_pickaxe"
    "golden_axe"
    "wooden_hoe"
    "stone_hoe"
    "iron_hoe"
    "diamond_hoe"
    "golden_hoe"
    "leather_helmet"
    "leather_chestplate"
    "leather_leggings"
    "leather_boots"
    "chainmail_helmet"
    "chainmail_chestplate"
    "chainmail_leggings"
    "chainmail_boots"
    "iron_helmet"
    "iron_chestplate"
    "iron_leggings"
    "iron_boots"
    "diamond_helmet"
    "diamond_chestplate"
    "diamond_leggings"
    "diamond_boots"
    "golden_helmet"
    "golden_chestplate"
    "golden_leggings"
    "golden_boots"
    "sign"
    "bucket"
    "water_bucket"
    "lava_bucket"
    "minecart"
    "saddle"
    "snowball"
    "oak_boat"
    "milk_bucket"
    "chest_minecart"
    "furnace_minecart"
    "egg"
    "fishing_rod"
    "cake"
    "white_bed"
    "orange_bed"
    "magenta_bed"
    "light_blue_bed"
    "yellow_bed"
    "lime_bed"
    "pink_bed"
    "gray_bed"
    "light_gray_bed"
    "cyan_bed"
    "purple_bed"
    "blue_bed"
    "brown_bed"
    "green_bed"
    "red_bed"
    "black_bed"
    "cookie"
    "filled_map"
    "shears"
    "ender_pearl"
    "potion"
    "brewing_stand"
    "bat_spawn_egg"
    "blaze_spawn_egg"
    "cave_spider_spawn_egg"
    "chicken_spawn_egg"
    "cow_spawn_egg"
    "creeper_spawn_egg"
    "donkey_spawn_egg"
    "elder_guardian_spawn_egg"
    "enderman_spawn_egg"
    "endermite_spawn_egg"
    "evocation_illager_spawn_egg"
    "ghast_spawn_egg"
    "guardian_spawn_egg"
    "horse_spawn_egg"
    "husk_spawn_egg"
    "llama_spawn_egg"
    "magma_cube_spawn_egg"
    "mooshroom_spawn_egg"
    "mule_spawn_egg"
    "ocelot_spawn_egg"
    "parrot_spawn_egg"
    "pig_spawn_egg"
    "polar_bear_spawn_egg"
    "rabbit_spawn_egg"
    "sheep_spawn_egg"
    "shulker_spawn_egg"
    "silverfish_spawn_egg"
    "skeleton_spawn_egg"
    "skeleton_horse_spawn_egg"
    "slime_spawn_egg"
    "spider_spawn_egg"
    "squid_spawn_egg"
    "stray_spawn_egg"
    "vex_spawn_egg"
    "villager_spawn_egg"
    "vindication_illager_spawn_egg"
    "witch_spawn_egg"
    "wither_skeleton_spawn_egg"
    "wolf_spawn_egg"
    "zombie_spawn_egg"
    "zombie_horse_spawn_egg"
    "zombie_pigman_spawn_egg"
    "zombie_villager_spawn_egg"
    "writable_book"
    "written_book"
    "player_head"
    "carrot_on_a_stick"
    "firework_rocket"
    "firework_star"
    "enchanted_book"
    "tnt_minecart"
    "hopper_minecart"
    "rabbit_stew"
    "armor_stand"
    "iron_horse_armor"
    "golden_horse_armor"
    "diamond_horse_armor"
    "name_tag"
    "command_block_minecart"
    "white_banner"
    "orange_banner"
    "magenta_banner"
    "light_blue_banner"
    "yellow_banner"
    "lime_banner"
    "pink_banner"
    "gray_banner"
    "light_gray_banner"
    "cyan_banner"
    "purple_banner"
    "blue_banner"
    "brown_banner"
    "green_banner"
    "red_banner"
    "black_banner"
    "beetroot_soup"
    "splash_potion"
    "tipped_arrow"
    "lingering_potion"
    "shield"
    "elytra"
    "spruce_boat"
    "birch_boat"
    "jungle_boat"
    "acacia_boat"
    "dark_oak_boat"
    "totem_of_undying"
    "knowledge_book"
    "debug_stick"
    "music_disc_13"
    "music_disc_cat"
    "music_disc_blocks"
    "music_disc_chirp"
    "music_disc_far"
    "music_disc_mall"
    "music_disc_mellohi"
    "music_disc_stal"
    "music_disc_strad"
    "music_disc_ward"
    "music_disc_11"
    "music_disc_wait"
    "iron_door"
    "oak_door"
    "spruce_door"
    "birch_door"
    "jungle_door"
    "acacia_door"
    "dark_oak_door"
    |]

let STACKABLE_TO_64_ITEM_IDS = ALL_GIVEABLE_ITEM_IDS |> Array.filter (fun x -> not(ITEMS_THAT_DO_NOT_STACK_TO_64_OR_MIGHT_HAVE_METADATA |> Array.contains(x)))

// things you may want to quick stack to enderchest in a mining expedition
let PRAGMATIC_64_STACKABLES = [|
    "stone"
    "granite"
    "diorite"
    "andesite"
    "dirt"
    "cobblestone"
    "gravel"

    "coal_ore"
    "iron_ore"
    "gold_ore"
    "lapis_ore"
    "redstone_ore"
    "diamond_ore"
    "emerald_ore"

    "coal_block"
    "iron_block"
    "gold_block"
    "lapis_block"
    "redstone_block"
    "diamond_block"
    "emerald_block"

    "coal"
    "iron_ingot"
    "gold_ingot"
    "lapis_lazuli"
    "redstone"
    "diamond"
    "emerald"

    "sand"
    "red_sand"
    "sandstone"

    "cobweb"
    "mossy_cobblestone"

    "white_terracotta"
    "orange_terracotta"
    "magenta_terracotta"
    "light_blue_terracotta"
    "yellow_terracotta"
    "lime_terracotta"
    "pink_terracotta"
    "gray_terracotta"
    "light_gray_terracotta"
    "cyan_terracotta"
    "purple_terracotta"
    "blue_terracotta"
    "brown_terracotta"
    "green_terracotta"
    "red_terracotta"
    "black_terracotta"
    "terracotta"

    "string"
    "gunpowder"
    "flint"
    "bone"
    "spider_eye"
    |]

let BIOMES_AND_CORE = [|
    "ocean", null
    "plains", null
    "desert", null
    "extreme_hills", null
    "forest", null
    "taiga", null
    "swampland", null
    "river", null
    "hell", null
    "sky", null
    "frozen_ocean", "ocean"
    "frozen_river", "river"
    "ice_flats", null
    "ice_mountains", null
    "mushroom_island", null
    "mushroom_island_shore", "mushroom_island"
    "beaches", null
    "desert_hills", "desert"
    "forest_hills", "forest"
    "taiga_hills", "taiga"
    "smaller_extreme_hills", "extreme_hills"
    "jungle", null
    "jungle_hills", "jungle"
    "jungle_edge", "jungle"
    "deep_ocean", "ocean"
    "stone_beach", null
    "cold_beach", null
    "birch_forest", null
    "birch_forest_hills", "birch_forest"
    "roofed_forest", null
    "taiga_cold", null
    "taiga_cold_hills", "taiga_cold"
    "redwood_taiga", null
    "redwood_taiga_hills", "redwood_taiga"
    "extreme_hills_with_trees", "extreme_hills"
    "savanna", null
    "savanna_rock", "savanna"
    "mesa", null
    "mesa_rock", "mesa"
    "mesa_clear_rock", "mesa"
    "void", null
    "mutated_plains", "plains"
    "mutated_desert", "desert"
    "mutated_extreme_hills", "extreme_hills"
    "mutated_forest", "forest"
    "mutated_taiga", "taiga"
    "mutated_swampland", "swampland"
    "mutated_ice_flats", "ice_flats"
    "mutated_jungle", "jungle"
    "mutated_jungle_edge", "jungle"
    "mutated_birch_forest", "birch_forest"
    "mutated_birch_forest_hills", "birch_forest"
    "mutated_roofed_forest", "roofed_forest"
    "mutated_taiga_cold", "taiga_cold"
    "mutated_redwood_taiga", "redwood_taiga"
    "mutated_redwood_taiga_hills", "redwood_taiga"
    "mutated_extreme_hills_with_trees", "extreme_hills"
    "mutated_savanna", "savanna"
    "mutated_savanna_rock", "savanna"
    "mutated_mesa", "mesa"
    "mutated_mesa_rock", "mesa"
    "mutated_mesa_clear_rock", "mesa"
    |]

let BIOMES = BIOMES_AND_CORE |> Array.map fst

let BIOME_COLLECTIONS =
    let r = ResizeArray()
    for biome,core in BIOMES_AND_CORE do
        if core = null then
            r.Add(ResizeArray[|biome|])
        else
            let i = r.FindIndex(fun a -> a.[0] = core)
            r.[i].Add(biome)
    r