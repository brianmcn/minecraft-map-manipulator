module EandT_S11

(*
=========
E&T ideas
=========
 - QFE is in MC_Constants
 - most recipe work is in Recipes
 - QuickStack
 - WarpPoints
 - Treecapitator & Vein Miner in EandT_S11
 - throwable_light in Program


 is this interesting? https://www.reddit.com/r/Minecraft/comments/7npasa/set_a_waypoint_on_the_map_17w50a_snapshot/ 

fun: two wall signs back to back can support one another - place one on wall, then replace wall with sign via commands


// game mechanics
new mobs?  a not-too-expensive spawn-finder is at 1Hz tag all un-processed mobs with 'processed', and run some function on unprocessed ones (event from base pack)
 - then e.g. client pack can make e.g. 'prospector', if player y < 50 and on(in) rails, a newly spawned zombie at some range might hold a pickaxe and wear special armor and get a name
 - rare mob could bring book lore to progression?  (time based? time query stuff?  progression based tech tree?)
 - deep mob, e.g. special zombie, or skeleton with special arrow, that spawns rarely below y=20 or something
 - underground slimes could place/update a structure block under bedrock that counts slimes, used for finding slime chunks somehow in survival? (sound when in those chunks? only creative players see structure outlines)
 - a rare bat spawns 10 more bats - bat-cave? (ambiance)

use /locate to build some kind of homing/structure-finding thingy (temple_locator() in Program.fs)

remote redstone, e.g. quark's 'ender watcher', a block that emits redstone signal when player looks at it

potion of echolocation? gives 'glowing' to all nearby mobs, or maybe 5 nearest mobs - could be useful for building grinders to find unlit caves (modulo Glowing bugs I know about)
 - or rather than use 'glowing', can do facing math to somehow particles-point in direction of mob... can you make anything 'hud like' that tracks with player, like pumpkinblur?
      I guess e.g. a nearby AEC with a name of '.' or something kinda draws a pixel in a location, could maybe do that 16 blocks away and avoid parallax from being a tick behind player movement?
      names only render up to N blocks away radially (32?) and are subject to lighting, probably would not work well

come up with some kind of 'reward' for building mob farm (e.g. cool thing costs lost of glowstone/redstone/bone/slimeball/... structure locators above?)
    villager trade, e.g. 64 bone blocks and 64 redstone blocks -> thingy?
    /give Lorgon111 minecraft:villager_spawn_egg{EntityTag:{Profession:1,Career:1,CareerLevel:999,Willing:1b,Offers:{Recipes:[{uses:0,maxUses:999,buy:{id:"minecraft:coal",Count:33b},sell:{id:"minecraft:stone",Count:2b}}]}}} 1

increase leather drops from cows (early game armor?)




Unlocking recipes, maybe make a tech tree, and finding dungeons give ingredients to unlock portions of tree?
    gamerule doLimitedCrafting true      recipe take @p <name>     then later 'give' it back or have a knowledge book gift it
Like previous season, but with choice rather than linear (craft a knowledge book to 'choose', nametag with custom nbt found in loot chests could be good 'fragment' that gets crafted into knowledge_book? 'command block' is another 'inert material item' for survival players (cannot place/use))
possible things to need to learn (from previous) include: Furnace, Bed, Anvil, I. Armor, Bow, D. Armor, Bucket, Shield, D. Pick
others: enchant table, brewing stand (nether chest item to get recipe?), iron pick, all hoes, stone tools, chest, (certain foods? no because can't limit furnace, but see * below), charcoal, fishing_rod
others: various arbitrary ones (like colored glass, daylight sensor, rabbit stew) could be gated to 'slow down' the QFE process maybe, piston (for automated farms), firework_rocket (for flying), bone meal
maybe certain ones only in certain chests (igloo, jungle temple, stronghold, mansion) to force finding those things? need decent loot table to make probability high

no recipe for furnace, chest, hoe all make game more interesting until find a village... maybe i choose a seed without a village near spawn?
chest: also dispenser/dropper/hopper (and maybe item frame) for storage
*i also could make furnaces unobtainable until very late, and use crafting recipes, e.g. 8 pork chops around a coal yields 8 cooked pork? 8 iron ore + coal only yields a couple ingots, need furnace for full effect?

food: without hoe/farmland, only renewables are mushroom stew, apples, zombie flesh, fish; hoe/farmland leads to bread, breeding animals for raw meat, carrot, raw potato/beetrot; then cooking recipes lead to better
so maybe cooked fish is early game food, others unlock much later?

could also have QFE thresholds or achievements gift you 'knowledge fragements' (or increase world border?)... would be good to
 - make a recipe tech tree
 - check prior E&T season to see what the 'pace' of finding dungeon chests was (seems like i found like 5 in first 5 episodes, and also found a couple abandoned mineshafts with more chests (mesas help))
 - check old E&T to see pace of getting QFE items (31, 61, 95, 174, 238, (nether) 273, 303, 313 (stronghold), 325, 10 more episodes to get dragon...470, )
 - try to overlay those with right 'pace' of recipe progression

recipes to unlock
-----------------
furnace
bed
anvil
iron armor
bow
diamond armor
bucket
shield
diamond pick
enchant table
brewing stand
iron pick
hoes
stone tools?
xxx chest
charcoal?
fishing rod
various arbitrary ones (like colored glass, other colored stuff, slow down qfe? daylight sensor, rabbit stew)
piston
fireworks rocket
bone meal (from bone)
xxx dispenser
xxx dropper
xxx hopper
maybe extra furnace-less food/smelt recipes separately
throwable light
warp points
(could have different 'knowledge fragment' costs for unlocking certain knowledge books)

on order of 26 recipes there... enough to tree?  try...   hmm, is there kinda tools/combat area, food area, other game mechanics area?

tools/combat                                       food                                           game mechanics

                                                   fishing rod
stone tools?
                                                                                                  warp points (each find creates 1, so this is like 16 unlocks)
                                                                                                  throwable light
                                                   cooked fish without furnace (6 from 8?)
                                                                                                  bed
                                                                                                  anvil
bow
                                                   hoes
iron ingot without furnace (1 from 8?)
//shield
iron pick
                                                                                                  piston
                                                   bone meal (from bone)
gold ingot without furnace (4 from 8?)
                                                   //cooked meats without furnace (4 from 8?)
furnace                                            furnace
iron armor
diamond pick
                                                                                                  enchant table
																								  book
                                                                                                  colored glass, other colored stuff?
                                                                                                  bucket
                                                                                                  brewing stand
												   rabbit stew
                                                                                                  fireworks rocket
																								  daylight sensor?
diamond armor

maybe villages off, to prevent furnace/book?  i guess books also in stronghold/mansion, though, and furnace also in igloo?
aha, villages off forever.  igloo structure could be replaced in datapack, to not have furnace/brewing until such time as I unlock them; could be uncraftable, but eventually becomes findable?

will need to fix all loot tables, to get rid of things like 'buckets'... change iron/gold ingots to nuggets in loot tables, other things need to prevent, maybe give glass instead? or see below
 - could also have some completely random drops, like rather than having to use a fragment to get the recipe for a daylight sensor, just have the knowledge book randomly appear in loot chests sometimes, yeah

also consider somehow gifting/getting shulker box early, to cope with inventory management horror festival... (shulker drops could be knowledge fragments rather than nametags?)

if e.g. a nametag is a 'knowledge fragment', then e.g. "nametag + string" could be a recipe that unlocks the "fishing_rod knowledge book", 
    and using that book takes the prior nametag recipe and gives the next food one

possibly start the game with one nametag to see all starting recipes, or e.g. picking it up grants the initial recipes

recipes for   - nametag + ___
stone tools   - cobblestone
iron ingot    - iron nugget
iron tools    - iron ingot
gold ingot    - gold nugget
iron armor    - iron block
diamond armor - diamond block
diamond tools - diamond
bow           - stick
fishing rod   - string
cook fish     - cod
hoes          - cobble, iron ingot, gold ingot
bone meal     - bone
throwable light-slimeball
bed           - wool
anvil         - 3 ingot?
piston        - redstone
enchant table - book
bucket        - glass bottle (kill witch?)
consumables:
warp point    - purple dye
shulker box   - chest

In order to make these recipes discoverable, give all same prefix in name, so player can search recipe book for prefix to find all new recipes (maybe '111', is easy to type)

maybe, in addition to having structures gen rare loot recipes, also maybe husks/strays could rarely drop? incentivize fighting them? (can base spawner spawn them if exposed to sunlight? no)

todo recipe for throwable light, like torch + snowball + slimeball?

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

let FRAGMENT = "nametag"  // todo consider command block, maybe even 3 colors for different currencies, or structure block (cannot use these blocks in survival)

// TODO parent-prerequisites
let custom_survival_recipes = [|
    // recipes to unlock once, name-of-knowledge-book-and-lore 
    "iron_ingot",ShapedCrafting(PK_Enderchest("iron_ore","coal"),MC"iron_ingot",1),["Furnace-less iron";"A way to turn";"iron ore into";"ingots without";"a furnace"]
    "gold_ingot",ShapedCrafting(PK_Clock("gold_ore","coal"),MC"gold_ingot",2),["Furnace-less gold";"A way to turn";"gold ore into";"ingots without";"a furnace"]
    "cook_cod",ShapedCrafting(PK_Enderchest("cod","coal"),MC"cooked_cod",6),["Furnace-less cod";"A way to cook";"cod without";"a furnace"]
    //"cook_salmon",ShapedCrafting(PK_Enderchest("salmon","coal"),MC"cooked_salmon",6)
    //"throwable_torch",ShapedCrafting(PK_Dispenser("torch","slimeball","snowball"),MC"snowball",7) // TODO nbt

    // consumables to craft at any time
    //"warp_point",ShapelessCrafting([|MC(FRAGMENT);MC"purple_dye"|],MC"shulker_spawn_egg",1) // TODO nbt (name, loot table)
    "shulker_box",ShapelessCrafting([|MC(FRAGMENT);MC"chest"|],MC"purple_shulker_box",1),["Shulker box";"Inventory";"management";"is hard even";"in early game"]
    |]

let lootable_recipes = [|
    // TODO what are other good ones?
    [MC"daylight_sensor"]
    [for c in MC_Constants.COLORS do yield MC(c+"_stained_glass")]
    // rabbit stew
    // maybe some others where the knowledge books themselves can randomly be found in loot chests
    |] // init: takes these recipes; loot tables for dungeons etc have rare chance to give KB with the set
// TODO parent-prerequisites
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
//for recipe_name, _ingredients, knowledge_grants in custom_unlocking_recipes do
    // init: for r in knowledge_grants do /recipe take @p r
    // author advancement: trigger: recipe_unlocked, recipes: knowledge_grants.[0], rewards: function that "/recipe take @p recipe_name" and also "/recipe give @p child_recipe"
    // author recipe, once knowledge_book nbt is possible
let blah() = 
    let initCommands = ResizeArray()
    for _prereq, recipeName, _ingredients, knowledgeGrants, kbName in custom_unlocking_recipes do
        for kg in knowledgeGrants do
            initCommands.Add(sprintf "recipe @a take %s" (kg.ToString()))
            // TODO unlocking stuff 
            // can maybe fudge it sans nbt, e.g.
            // rather than craft a KB, craft N chain_command_blocks
            // a listener tries to clear 64->0 chain_command_blocks, and when succeeds at N, behaves as though KB consumed and
            //  - "recipe take @s %s" recipeName
            //  - "recipe give @s %s" kg
            //  - TODO "recipe give @s %s" <unlocked children KB recipes>


// TODO parent-prerequisites logic
// init: grant all with no parents (elsewhere advancement unlocks children)

// TODO datapack that replaces igloo, and then daapack disables once igloo pre-reqs met, except https://bugs.mojang.com/browse/MC-124167

(*
loot table design changes:

abandoned_mineshaft_chest and simple_dungeon and desert_pyramid and jungle_temple and nether_bridge:
 - iron ingots -> iron nuggets
 - gold_ingots -> gold_nuggets
 - bucket -> glass or maybe one of the random lootable_recipes?

 could basically do a search&replace in the file, and add one more to 'pools' for the rare extra

// TODO Lore below
/give @p minecraft:iron_pickaxe{VM:1,RepairCost:999999,display:{Name:"{\"color\":\"blue\",\"text\":\"Vein Miner\"}"}}
/give @p minecraft:gold_axe{TC:1,RepairCost:999999,display:{Name:"{\"color\":\"blue\",\"text\":\"Tree Feller\"}"}}

*)


// TODO tree auto-chopper thingy? gold axe plus 3 diamonds = special unrepairable : {Item:{tag:{RepairCost:2147483647}}} gold axe?  or ought it be findable rare item rather than a recipe? or require a fragment?
// TODO similarly vein-miner for mining (recipe?), or possibly 3x3 miner?
// TODO vaguely similar, 'reap' where a hoe can re-plant seeds while harvesting

// impossible to implement treecapitator 'right' (can't know what block player just mined), so find a way that's efficient approximation and not exploitable:
let LOGS = [| "acacia_log"; "oak_log"; "spruce_log"; "jungle_log"; "dark_oak_log"; "birch_log" |] |> Array.map (fun x -> x,x)
let ORES = [| "coal_ore"; "iron_ore"; "gold_ore"; "lapis_ore"; "redstone_ore"; "diamond_ore" |] |> Array.map (fun x -> x,x) 
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
            for b,_ in blocks do
                yield sprintf "scoreboard objectives add %s%s minecraft.mined:minecraft.%s" b suffixS b
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
        let ITEMSEL(bi) = sprintf """@e[type=item,distance=..7,sort=nearest,limit=1,nbt={Item:{id:"minecraft:%s"},PickupDelay:10s}]""" bi
        yield sprintf"check_mined_%s"suffixF,[|
            for b,bi in blocks do
                yield sprintf "execute if entity @s[scores={%s%s=1..}] at @s at %s run function tc:chop_start_%s_%s" b suffixS (ITEMSEL bi) suffixF bi
            |]
        for b,bi in blocks do
            yield sprintf "chop_start_%s_%s" suffixF bi,[|
                sprintf "scoreboard players set @s remain%s %d" suffixS (MAX+1)  // MAX+1 because we decrement 'remain' at start when found item entity (which will get tp'd to player), even though not breaking a block
                sprintf "execute at @s run tp %s ~ ~ ~" (ITEMSEL bi)
                sprintf "function tc:chop_check_%s_%s" suffixF bi
                sprintf "function tc:give_%s_%s" suffixF bi
                sprintf "function tc:reset_stats_%s" suffixF
                "gamerule randomTickSpeed 100"
                "$NTICKSLATER(80)"
                "gamerule randomTickSpeed 3"
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


// TODO some recipe in woodland mansion (need to heal zombie villager to get carto to find? or make mansion-finder?)

// TODO vanilla swirl all-seeing eye (FSE that grants night vision and attributes big slowness when held) and enderchest+unrepairable-silk-touch-wooden-pickaxe; fortune hoe could also be good?
// TODO alphabet banners are fun loot (craftable?)... player heads of the mjoang-mob-heads could be fun loot... stacks of blocks (rare like ice/packed/mycelium/podzol, or colorful like everything) are also fun loot to build with


// TODO desire lines?

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

// TODO do I want to make it so falling into void in The End teleports the player back to y=255 in the overworld?


