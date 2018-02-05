module EandT_S11

(*
=========
E&T ideas
=========
 - QFE is in MC_Constants
 - most recipe work is here or in Recipes
 - QuickStack
 - WarpPoints
 - Treecapitator & Vein Miner end of this file
 - throwable_light in Program


// game mechanics
new mobs?  a not-too-expensive spawn-finder is at 1Hz tag all un-processed mobs with 'processed', and run some function on unprocessed ones (event from base pack)
 - then e.g. client pack can make e.g. 'prospector', if player y < 50 and on(in) rails, a newly spawned zombie at some range might hold a pickaxe and wear special armor and get a name
 - rare mob could bring book lore to progression?  (time based? time query stuff?  progression based tech tree?)
 - deep mob, e.g. special zombie, or skeleton with special arrow, that spawns rarely below y=20 or something
 - underground slimes could place/update a structure block under bedrock that counts slimes, used for finding slime chunks somehow in survival? (sound when in those chunks? only creative players see structure outlines)
      - actually, simpler thing is e.g. slime on stone/-ite/coal has chance to convert stone to mossy cobble, or slimeblock, or something visible, so slimes leave a trail which helps you find slime chunks (maybe only during original spawning tick)
 - a rare bat spawns 10 more bats - bat-cave? (ambiance)
 - biome specific spawns (e.g. turn 50% zombies to spiders in a forest or something? ...)

use /locate to build some kind of homing/structure-finding thingy (temple_locator() in Program.fs) (Temple, or Mansion/EndCity/Fortress depending on dimension?)

map-filler-outer?  some mechanic that breifly tps you across a region in the sky (looking up), while you hold a map, to fill it out?  no way to know the map bounds, (not in nbt), but could just tp across -512..512 in steps of 128 and then return you to start.
 - a knowledge book is a nice consumable for this perhaps   - might take too long to run given how maps update?

come up with some kind of 'reward' for building mob farm (e.g. cool thing costs lots of glowstone/redstone/bone/slimeball/gunpowder... structure locators above?)
    villager trade, e.g. 64 bone blocks and 64 redstone blocks -> thingy?
    /give Lorgon111 minecraft:villager_spawn_egg{EntityTag:{Profession:1,Career:1,CareerLevel:999,Willing:1b,Offers:{Recipes:[{uses:0,maxUses:999,buy:{id:"minecraft:coal",Count:33b},sell:{id:"minecraft:stone",Count:2b}}]}}} 1

increase leather drops from cows (early game armor?)

something with moon phase (midnight full moon, all mobs look up, you look up, and, something?)

something xray-ish? (can check set of blocks in front of you, put glowing magma if certain block there, e.g. xray? or have make a sound originating there?)




food: without hoe/farmland, only renewables are mushroom stew, apples, zombie flesh, fish; hoe/farmland leads to bread, breeding animals for raw meat, carrot, raw potato/beetrot; then cooking recipes lead to better
so maybe cooked fish is early game food, others unlock much later?

could also have QFE thresholds or achievements gift you 'knowledge fragements' (or increase world border?)... would be good to
 - make a recipe tech tree
 - check prior E&T season to see what the 'pace' of finding dungeon chests was (seems like i found like 5 in first 5 episodes, and also found a couple abandoned mineshafts with more chests (mesas help))
 - check old E&T to see pace of getting QFE items (31, 61, 95, 174, 238, (nether) 273, 303, 313 (stronghold), 325, 10 more episodes to get dragon...470, )
 - try to overlay those with right 'pace' of recipe progression

(could have different 'knowledge fragment' costs for unlocking certain knowledge books)

maybe villages off, to prevent furnace/book?  i guess books also in stronghold/mansion, though, and furnace also in igloo?
aha, villages off forever.  igloo structure could be replaced in datapack, to not have furnace/brewing until such time as I unlock them; could be uncraftable, but eventually becomes findable?

will need to fix all loot tables, to get rid of things like 'buckets'... change iron/gold ingots to nuggets in loot tables, other things need to prevent, maybe give glass instead? or see below
 - could also have some completely random drops, like rather than having to use a fragment to get the recipe for a daylight sensor, just have the knowledge book randomly appear in loot chests sometimes, yeah

possibly start the game with one fragment to see all starting recipes, or e.g. picking it up grants the initial recipes

In order to make these recipes discoverable, give all same prefix in name, so player can search recipe book for prefix to find all new recipes (maybe '111', is easy to type)

maybe, in addition to having structures gen rare loot recipes, also maybe husks/strays could rarely drop? incentivize fighting them? (can base spawner spawn them if exposed to sunlight? no)

todo recipe for throwable light, like torch + snowball + slimeball?

could e.g. start with 'heavy shield' (slowness) and need to upgrade to learn to build light shield recipe?

from old swirl notes:
	tech tree: no early pickaxe? f&s rather than sword/axe? no torch recipe? villagers sell 3 choice of recipe? late bow/shield? early tnt? what foods? armor logic?
	recipes: iron nuggets on tip stone pick -> weak iron pick, can mine gold; iron/gold ore smelt furance -> nuggets; iron/gold nuggets in recipe for chain mail

	render's sprint mechanic - if player not near any hostile mobs (could mark all hostiles as spawn in? and check 1Hz? dist>16?) for some time (10s?) then give sprint back?
	render's consumable idea - craft tulip into food, then e.g. can give non-renewable food 'in world' rather than in a chest; also e.g. shrubs drop sticks, starve of wood, have craft table in environment
	using glass/panes tactically is fun

	caves always go up to max of 128, other params dont' seem to affcet caves
	mineshafts seem to generate anywhere below sea level
	ravines only go up to max 68
	it is not clear that custom terrain (apart from usual 'fewer lakes', 'more dungeons') will make game any more 'fun'
*)

open Recipes

let PK_Enderchest(outsideItem,insideItem) =
    PatternKey([|"XXX";"XOX";"XXX"|],[|'X',MC(outsideItem); 'O',MC(insideItem)|])
let PK_Clock(outsideItem,insideItem) =
    PatternKey([|" X ";"XOX";" X "|],[|'X',MC(outsideItem); 'O',MC(insideItem)|])
let PK_Dispenser(outsideItem,insideItem,bottomItem) =
    PatternKey([|"XXX";"XOX";"X$X"|],[|'X',MC(outsideItem); 'O',MC(insideItem); '$',MC(bottomItem)|])

let FRAGMENT = MC"nametag"  // TODO consider command block, maybe even 3 colors for different currencies, or structure block (cannot use these blocks in survival)

// TODO parent-prerequisites
let custom_survival_recipes = [|
    // recipes to unlock once, name-of-knowledge-book-and-lore 
    "iron_ingot",ShapedCrafting(PK_Enderchest("iron_ore","coal"),MC"iron_ingot",1),["Furnace-less iron";"A way to turn";"iron ore into";"ingots without";"a furnace"]
    "gold_ingot",ShapedCrafting(PK_Clock("gold_ore","coal"),MC"gold_ingot",2),["Furnace-less gold";"A way to turn";"gold ore into";"ingots without";"a furnace"]
    "cook_cod",ShapedCrafting(PK_Enderchest("cod","coal"),MC"cooked_cod",6),["Furnace-less cooked cod";"A way to cook";"cod without";"a furnace"]
    //"cook_salmon",ShapedCrafting(PK_Enderchest("salmon","coal"),MC"cooked_salmon",6)
    //"throwable_torch",ShapedCrafting(PK_Dispenser("torch","slimeball","snowball"),MC"snowball",7) // TODO nbt

    // consumables to craft at any time
    //"warp_point",ShapelessCrafting([|FRAGMENT;MC"purple_dye"|],MC"shulker_spawn_egg",1) // TODO nbt (name, loot table)
    "shulker_box",ShapelessCrafting([|FRAGMENT;MC"chest"|],MC"purple_shulker_box",1),["Shulker box";"Inventory";"management";"is hard even";"in early game"] // TODO want this? or prefer enderchest and quickstack?
    |]

let lootable_recipes = [|
    // TODO what are other good ones?
    [MC"daylight_sensor"]
    [for c in MC_Constants.COLORS do yield MC(c+"_stained_glass")]
    // rabbit stew
    // maybe some others where the knowledge books themselves can randomly be found in loot chests
    |] // init: takes these recipes; loot tables for dungeons etc have rare chance to give KB with the set

let custom_unlocking_recipes = [|
    // pre-req, recipe name, ingredients in addition to fragment, recipes granted, name-of-knowledge-book-and-lore 
    null,                 "unlock_stone_tools",     [MC"cobblestone"], [MC"stone_pickaxe";MC"stone_axe";MC"stone_shovel";MC"stone_sword"],["Stone tools"]
    "unlock_stone_tools", "unlock_iron_tools",      [MC"iron_ingot"], [MC"iron_pickaxe";MC"iron_axe";MC"iron_shovel";MC"iron_sword"],["Iron tools"]
    "unlock_iron_tools",  "unlock_diamond_tools",   [MC"diamond"], [MC"diamond_pickaxe";MC"diamond_axe";MC"diamond_shovel";MC"diamond_sword"],["Diamond tools"]
    null,                 "unlock_iron_armor",      [MC"iron_block"], [MC"iron_helmet";MC"iron_chestplate";MC"iron_leggings";MC"iron_boots"],["Iron armor"]
    "unlock_iron_armor",  "unlock_diamond_armor",   [MC"diamond_block"], [MC"diamond_helmet";MC"diamond_chestplate";MC"diamond_leggings";MC"diamond_boots"],["Diamond armor"]
    null,                 "unlock_bow",             [MC"stick"], [MC"bow"],["Bow"]
    null,                 "unlock_fishing_rod",     [MC"string"], [MC"fishing_rod"],["Fishing Rod"]
    "unlock_fishing_rod", "unlock_cook_fish",       [MC"cod"], [PATH"TODO:cook_cod"],["Furnace-less cod";"A way to cook";"cod without";"a furnace"]
    "unlock_cook_fish",   "unlock_hoes",            [MC"cobblestone";MC"iron_ingot";MC"gold_ingot"], [MC"wooden_hoe";MC"stone_hoe";MC"iron_hoe";MC"gold_hoe";MC"diamond_hoe"],["Hoes";"All five";"materials"]
    "unlock_hoes",        "unlock_bone_meal",       [MC"bone"], [MC"bone_meal"],["Bone meal";"from bones"]
    null,                 "unlock_bed",             [MC"white_wool"], [for c in MC_Constants.COLORS do yield MC(c+"_bed")],["Beds";"all colors"]
    null,                 "unlock_anvil",           [MC"iron_ingot";MC"iron_ingot";MC"iron_ingot"], [MC"anvil"],["Anvil"]
    null,                 "unlock_book",            [MC"paper"], [MC"book"],["Book"]
    "unlock_anvil",       "unlock_piston",          [MC"redstone"], [MC"piston"],["Piston"]
    null,                 "unlock_enchanting_table",[MC"book"], [MC"enchanting_table"],["Enchanting Table"]
    null,                 "unlock_bucket",          [MC"glass_bottle"], [MC"bucket"],["Bucket"]
    |]
let childrenOf(recipeName) = [|
    for prereq,r,_,_,_ in custom_unlocking_recipes do
        if prereq = recipeName then
            yield r
    |]
let NS = "TODO" // TODO
let author_tech_tree_logic(pack:Utilities.DataPackArchive) = 
    let initCommands = ResizeArray()
    for _prereq, recipeName, ingredients, knowledgeGrants, kbName in custom_unlocking_recipes do
        // init
        for kg in knowledgeGrants do
            initCommands.Add(sprintf "recipe @a take %s" (kg.ToString()))
        // recipe
        pack.WriteRecipe(NS,recipeName,ShapelessCrafting([|yield FRAGMENT;yield! ingredients|],MC"knowledge_book",  // TODO KB needs name, lore, and recipes attached via NBT; name must be searchable (see https://bugs.mojang.com/browse/MC-124553 )
                            1).AsJsonString())
        // advancement
        pack.WriteAdvancement(NS,recipeName,sprintf """{
            "criteria": { "xxx": {"trigger": "minecraft:recipe_unlocked","conditions": {"recipe": "%s"}} },
            "requirements": [ ["xxx"] ],
            "rewards": { "function": "%s:on_%s" } }""" knowledgeGrants.[0].STR NS recipeName)
        // reward function
        pack.WriteFunction(NS,sprintf"on_%s"recipeName,[|
            yield sprintf "recipe take @s %s" recipeName
            for child in childrenOf(recipeName) do
                yield sprintf "recipe give @s %s" child
            |])
    pack.WriteFunction(NS,"init_player_recipes",[|
        yield "gamerule doLimitedCrafting true"
        yield "recipe give @p *"
        yield! initCommands
        |])


// TODO datapack that replaces igloo, and then datapack disables once igloo pre-reqs met, except https://bugs.mojang.com/browse/MC-124167

// TODO some recipe in woodland mansion (need to heal zombie villager to get carto to find? or make mansion-finder?)

// TODO vanilla swirl all-seeing eye (FSE that grants night vision and attributes big slowness when held) 
//    /give @p minecraft:fermented_spider_eye{ench:[{id:51s,lvl:1s}],FSE:1b,display:{Name:"\"Infinitely seeing eye\"",Lore:["TODO"]}} 1
//    effect give @a[nbt={SelectedItem:{tag:{FSE:1b}}}] night_vision 1 0 true
//    effect give @a[nbt={SelectedItem:{tag:{FSE:1b}}}] slowness 1 4 true
// and enderchest+unrepairable-silk-touch-wooden-pickaxe
//    /give @p minecraft:wooden_pickaxe{ench:[{id:33s,lvl:1s}],RepairCost:99999} 1
//    or, alternatively, no silk touch, and e.g. just   /give @p minecraft:ender_chest 64
// fortune hoe could also be good?
// TODO alphabet banners are fun loot (craftable?)... 
//    /give @p chest{BlockEntityTag:{Items:[{Slot:0b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"ls",Color:15},{Pattern:"rs",Color:15},{Pattern:"ts",Color:15},{Pattern:"ms",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:1b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{id:"white_banner",Patterns:[{Pattern:"ms",Color:15},{Pattern:"ts",Color:15},{Pattern:"bs",Color:15},{Pattern:"rs",Color:15},{Pattern:"cbo",Color:0},{Pattern:"ls",Color:15},{Pattern:"bl",Color:15},{Pattern:"tl",Color:15},{Pattern:"bo",Color:0}],Base:15},display:{Lore:["(+NBT)"]}},Damage:0s},{Slot:2b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"ls",Color:15},{Pattern:"ts",Color:15},{Pattern:"bs",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:3b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"rs",Color:15},{Pattern:"ts",Color:15},{Pattern:"bs",Color:15},{Pattern:"cbo",Color:0},{Pattern:"ls",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:4b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"ls",Color:15},{Pattern:"bs",Color:15},{Pattern:"ms",Color:15},{Pattern:"ts",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:5b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"ls",Color:15},{Pattern:"ts",Color:15},{Pattern:"ms",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:6b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"rud",Color:15},{Pattern:"hh",Color:0},{Pattern:"bs",Color:15},{Pattern:"ls",Color:15},{Pattern:"ts",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:7b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"ls",Color:15},{Pattern:"rs",Color:15},{Pattern:"ms",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:8b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"ts",Color:15},{Pattern:"cs",Color:15},{Pattern:"bs",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:9b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"ls",Color:15},{Pattern:"hh",Color:0},{Pattern:"bs",Color:15},{Pattern:"rs",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:10b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"drs",Color:15},{Pattern:"hh",Color:0},{Pattern:"ls",Color:15},{Pattern:"dls",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:11b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"ls",Color:15},{Pattern:"bs",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:12b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"tt",Color:15},{Pattern:"tts",Color:0},{Pattern:"ls",Color:15},{Pattern:"rs",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:13b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"ls",Color:15},{Pattern:"drs",Color:15},{Pattern:"rs",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:14b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"mr",Color:0},{Pattern:"ts",Color:15},{Pattern:"bs",Color:15},{Pattern:"ls",Color:15},{Pattern:"rs",Color:15},{Pattern:"bo",Color:0}]}},Damage:0s},{Slot:15b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"hh",Color:15},{Pattern:"cs",Color:0},{Pattern:"ts",Color:15},{Pattern:"ms",Color:15},{Pattern:"ls",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:16b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"mr",Color:0},{Pattern:"ts",Color:15},{Pattern:"bs",Color:15},{Pattern:"ls",Color:15},{Pattern:"rs",Color:15},{Pattern:"bo",Color:0},{Pattern:"br",Color:15}]}},Damage:0s},{Slot:17b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{id:"white_banner",Patterns:[{Pattern:"rs",Color:15},{Pattern:"rud",Color:0},{Pattern:"ms",Color:15},{Pattern:"hh",Color:15},{Pattern:"cs",Color:0},{Pattern:"drs",Color:15},{Pattern:"mc",Color:15},{Pattern:"tt",Color:0},{Pattern:"ls",Color:15},{Pattern:"ts",Color:15},{Pattern:"ms",Color:15},{Pattern:"bo",Color:0}],Base:15},display:{Lore:["(+NBT)"]}},Damage:0s},{Slot:18b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{id:"white_banner",Patterns:[{Pattern:"bs",Color:15},{Pattern:"rud",Color:0},{Pattern:"ts",Color:15},{Pattern:"mr",Color:0},{Pattern:"drs",Color:15},{Pattern:"bo",Color:0}],Base:15},display:{Lore:["(+NBT)"]}},Damage:0s},{Slot:19b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"ts",Color:15},{Pattern:"cs",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:20b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"ls",Color:15},{Pattern:"bs",Color:15},{Pattern:"rs",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:21b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"ls",Color:15},{Pattern:"bl",Color:0},{Pattern:"dls",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:22b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"bt",Color:15},{Pattern:"bts",Color:0},{Pattern:"ls",Color:15},{Pattern:"rs",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:23b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"cr",Color:15},{Pattern:"bo",Color:0},{Pattern:"cbo",Color:0}]}}},{Slot:24b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"drs",Color:15},{Pattern:"br",Color:0},{Pattern:"rs",Color:0},{Pattern:"vhr",Color:0},{Pattern:"dls",Color:15},{Pattern:"cbo",Color:0}]}}},{Slot:25b,id:"minecraft:white_banner",Count:16b,tag:{BlockEntityTag:{Patterns:[{Pattern:"ts",Color:15},{Pattern:"dls",Color:15},{Pattern:"bs",Color:15},{Pattern:"bo",Color:0}]}}},{Slot:26b,id:"minecraft:white_banner",Count:16b}]}}
//player heads of the mjoang-mob-heads could be fun loot... stacks of blocks (rare like ice/packed/mycelium/podzol, or colorful like everything) are also fun loot to build with


// TODO craftable Ragecraft-like frost trap, 
//    just e.g. lingering potion of long-slowness (vanilla item) is decent, but make it craftable/findable to use pre-dragon... 
//    hm, but RC one makes thrower immune, players get hit by own potion, hm... guess could detect when player is near AEC and cancel it with /effect...
//    yeah and RC one was like slowness 4, for 1s, re-applied every second, or something, and also damaged for 0 (so mobs make hit sound), can I conjure that?
//    no on the damage sound, but e.g. could make glowing for 10 ticks
// here's a cloud for flash-glowing and slowness 6:   (Radius:1 means 2x2 square of blocks)
// /summon area_effect_cloud ~ ~ ~ {Duration:600,Radius:1.5,ReapplicationDelay:20,Effects:[{Id:24b,Amplifier:0b,Duration:10,Ambient:1b,ShowParticles:0b},{Id:2b,Amplifier:5b,Duration:20,Ambient:1b,ShowParticles:0b}],Particle:"falling_dust ice"}
// don't think i can /give a lingering potion that does that, but could have a custom unique pot for duration 2 or something, detect that, and summon my own AEC?

// TODO how to communicate initial (and permanent, e.g. furnace craft) limitations to player at start or along way?  
// what is right way to communicate deviations from vanilla without giving eveything away and taking away mystery?
//  - is there a good way to have mobs deliver books of lore as loot drops after some time-amount/progression-gate? or consuming a knowledge book (detect by advancement) could /give the player extra lore when needed?

// TODO do I want to make it so falling into void in The End teleports the player back to y=255 in the overworld?



(*
loot table design changes:

abandoned_mineshaft_chest and simple_dungeon and desert_pyramid and jungle_temple and nether_bridge:
 - iron ingots -> iron nuggets
 - gold_ingots -> gold_nuggets
 - bucket -> glass or maybe one of the random lootable_recipes?

could basically do a search&replace in the file, and add one more to 'pools' for the rare extra
seems like mechanically, it's easier to spec the changes I want to make and then just make them by hand
see also test_json() in Program.fs

// TODO Lore below
/give @p minecraft:iron_pickaxe{VM:1,RepairCost:999999,display:{Name:"{\"color\":\"blue\",\"text\":\"Vein Miner\"}"}}
/give @p minecraft:gold_axe{TC:1,RepairCost:999999,display:{Name:"{\"color\":\"blue\",\"text\":\"Tree Feller\"}"}}

*)

(*
discarded ideas

remote redstone, e.g. quark's 'ender watcher', a block that emits redstone signal when player looks at it

potion of echolocation? gives 'glowing' to all nearby mobs, or maybe 5 nearest mobs - could be useful for building grinders to find unlit caves (modulo Glowing bugs I know about)
 - or rather than use 'glowing', can do facing math to somehow particles-point in direction of mob... can you make anything 'hud like' that tracks with player, like pumpkinblur?
      I guess e.g. a nearby AEC with a name of '.' or something kinda draws a pixel in a location, could maybe do that 16 blocks away and avoid parallax from being a tick behind player movement?
      names only render up to N blocks away radially (32?) and are subject to lighting, probably would not work well


*)




// TODO tree auto-chopper thingy? gold axe plus 3 diamonds = special unrepairable : {Item:{tag:{RepairCost:2147483647}}} gold axe?  or ought it be findable rare item rather than a recipe? or require a fragment?
// TODO similarly vein-miner for mining (recipe?), or possibly 3x3 miner?
// TODO vaguely similar, 'reap' where a hoe can re-plant seeds while harvesting

// impossible to implement treecapitator 'right' (can't know what block player just mined), so find a way that's efficient approximation and not exploitable:
let LOGS = [| "acacia_log"; "oak_log"; "spruce_log"; "jungle_log"; "dark_oak_log"; "birch_log" |] |> Array.map (fun x -> x,x)
let ORES = [| "coal_ore"; "iron_ore"; "gold_ore"; "lapis_ore"; "redstone_ore"; "diamond_ore"; "obsidian" |] |> Array.map (fun x -> x,x)   // Note: obsidian means diamond pick can quick-mine that too
            |> Array.append [| "coal_ore", "coal"; "lapis_ore", "lapis_lazuli"; "redstone_ore", "redstone"; "diamond_ore", "diamond"|]
let TREE_DIRS = [|
    // 8 dirs on this level
    "~00 ~00 ~01"
    "~01 ~00 ~01"
    "~01 ~00 ~00"
    "~01 ~00 ~-1"
    "~00 ~00 ~-1"
    "~-1 ~00 ~-1"
    "~-1 ~00 ~00"
    "~-1 ~00 ~01"
    // 8 dirs on next level
    "~00 ~01 ~01"
    "~01 ~01 ~01"
    "~01 ~01 ~00"
    "~01 ~01 ~-1"
    "~00 ~01 ~-1"
    "~-1 ~01 ~-1"
    "~-1 ~01 ~00"
    "~-1 ~01 ~01"
    // above
    "~00 ~01 ~00"
    |]
let ORE_DIRS = [|
    // 6 neighbors
    "~00 ~00 ~01"
    "~01 ~00 ~00"
    "~00 ~01 ~00"
    "~00 ~00 ~-1"
    "~-1 ~00 ~00"
    "~00 ~-1 ~00"
    |]
let MAX = 150
let connected_mining_functions = [|
    for suffixF,suffixS,tag,blocks,dirs in ["tc","TC","TC",LOGS,TREE_DIRS;
                                            "vm","VM","VM",ORES,ORE_DIRS] do
        yield sprintf"init_%s"suffixF,[|
            yield sprintf "scoreboard objectives add isHolding%s dummy" suffixS
            yield sprintf "scoreboard objectives add wasHolding%s dummy" suffixS
            yield sprintf "scoreboard objectives add remain%s dummy" suffixS
            yield sprintf "scoreboard objectives add TEMP dummy"
            for b,_ in blocks do
                yield sprintf "scoreboard objectives add %s%s minecraft.mined:minecraft.%s" b suffixS b
            yield sprintf "scoreboard players set randomTickSpeed TEMP -1"
            |]
        yield sprintf"tick_%s"suffixF,[|
            sprintf "scoreboard players set @a isHolding%s 0" suffixS
            sprintf "scoreboard players set @a[nbt={SelectedItem:{tag:{%s:1}}}] isHolding%s 1" tag suffixS  // TODO pick a real item tag
            sprintf "execute as @a[scores={isHolding%s=1,wasHolding%s=0}] run function tc:reset_stats_%s" suffixS suffixS suffixF
            sprintf "execute as @a run scoreboard players operation @s wasHolding%s = @s isHolding%s" suffixS suffixS
            sprintf "execute as @a[scores={isHolding%s=1}] run function tc:check_mined_%s" suffixS suffixF
            |]
        yield sprintf"reset_stats_%s"suffixF,[|
            for b,_ in blocks do
                yield sprintf "scoreboard players set @s %s%s 0" b suffixS
            |]
        let ITEMSEL(bi) = sprintf """@e[type=item,distance=..7,sort=nearest,limit=1,nbt={Item:{id:"minecraft:%s"},PickupDelay:10s}]""" bi  // todo could also look for Age:0s in addition to PickupDelay
        yield sprintf"check_mined_%s"suffixF,[|
            for b,bi in blocks do
                yield sprintf "execute if entity @s[scores={%s%s=1..}] at @s at %s run function tc:chop_start_%s_%s" b suffixS (ITEMSEL bi) suffixF bi
            |]
        for b,bi in blocks do
            yield sprintf "chop_start_%s_%s" suffixF bi,[|
                yield sprintf "scoreboard players set @s remain%s %d" suffixS (MAX+1)  // MAX+1 because we decrement 'remain' at start when found item entity (which will get tp'd to player), even though not breaking a block
                yield sprintf "execute at @s run tp %s ~ ~ ~" (ITEMSEL bi)
                yield sprintf "function tc:chop_check_%s_%s" suffixF bi
                yield sprintf "function tc:give_%s_%s" suffixF bi
                yield sprintf "function tc:reset_stats_%s" suffixF
                if suffixF = "tc" then
                    yield "execute if score randomTickSpeed TEMP matches -1 store result score randomTickSpeed TEMP run gamerule randomTickSpeed"
                    yield "gamerule randomTickSpeed 100"
                    yield "$NTICKSLATER(80)"
                    yield "function tc:restore_random_tick_speed"
                    yield "scoreboard players set randomTickSpeed TEMP -1"
                |]
            yield sprintf "chop_check_%s_%s" suffixF bi,[|
                sprintf "execute if entity @s[scores={remain%s=1..}] run function tc:chop_body_%s_%s" suffixS suffixF bi
                |]
            yield sprintf "chop_body_%s_%s" suffixF bi,[|
                yield sprintf "setblock ~ ~ ~ air"
                yield sprintf "scoreboard players remove @s remain%s 1" suffixS
                for dir in dirs do
                    yield sprintf "execute positioned %s if block ~ ~ ~ %s run function tc:chop_check_%s_%s" dir b suffixF bi
                |]
            yield sprintf "give_%s_%s" suffixF bi,[|
                sprintf "execute if entity @s[scores={remain%s=..%d}] run give @s %s 1" suffixS (MAX-1) bi // MAX-1 because we want to test <MAX
                sprintf "scoreboard players add @s remain%s 1" suffixS 
                sprintf "execute if entity @s[scores={remain%s=..%d}] run function tc:give_%s_%s" suffixS (MAX-1) suffixF bi
                |]
    yield "restore_random_tick_speed", [|
        for i = 0 to 100 do
            yield sprintf "execute if score randomTickSpeed TEMP matches %d run gamerule randomTickSpeed %d" i i
        |]
    |]
let tc_main() =
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "TestWarpPoints")
    let pack = new Utilities.DataPackArchive(world,"multi_miner","cut down entire trees or mine entire ore veins")
    let compiler = new Compiler.Compiler('v','m',"tc",1,100,1,false)
    for ns,name,code in [for name,code in connected_mining_functions do yield! compiler.Compile("tc",name,code)] do
        pack.WriteFunction(ns,name,code)
    for ns,name,code in compiler.GetCompilerLoadTick() do
        pack.WriteFunction(ns,name,code)
    pack.WriteFunctionTagsFileWithValues("minecraft","load",[compiler.LoadFullName;"tc:init_tc";"tc:init_vm"])
    pack.WriteFunctionTagsFileWithValues("minecraft","tick",[compiler.TickFullName;"tc:tick_tc";"tc:tick_vm"])
    pack.SaveToDisk()


