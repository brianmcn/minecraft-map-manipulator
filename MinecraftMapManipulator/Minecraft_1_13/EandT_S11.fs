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


// TODO with facing, is 'item magnet' interesting?


Unlocking recipes, maybe make a tech tree, and finding dungeons give ingredients to unlock portions of tree?
    gamerule doLimitedCrafting true      recipe take @p <name>     then later 'give' it back or have a knowledge book gift it
Like previous season, but with choice rather than linear (craft a knowledge book to 'choose', nametag with custom nbt found in loot chests could be good 'fragment' that gets crafted into knowledge_book?)
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

	terrain changes: depthbasesize up (to like 13) and sea level up (to like 100) gives more underground (also bring ore distributions up higher), but
	 - connectedness of caves/mineshafts below ground is still a big issue... maybe i carve out more air? idea to compute skeleton, and then have 'ends' search spherically for a 
	   different airspace CC within like 10 blocks or something, and if found, connect with an air tunnel? also need a way to open up more tunnels to surface...
	 - or maybe can get more with other settings, e.g. increase lower limit scale to 850? 3000? even upper height limit 3000? ...
		 - lower limit down to 70 makes amplified-like crazy overhangy tall bits; upper limit down to 170, similar, not as many caves? both down to 212, not much land above sea level
	 - or maybe can overlay cave air from a separate world with even larger cave-height, which creates more holes to surface and adds more caves?
		 - yeah, depthbase 20, sea level 150, turn off mineshafts/lakes/dungeons/ravines, just grab cave air, overlay, seems plausible
	(could replace large pockets of dirt/gravel/ore with glass/air, as another possible way to 'connect' caves?)
	render's idea for advancements as 'tome of knowledge', e.g. tab for 'yellow wool', names that area, shows how many emeralds or other side objectives there are, etc.
	render's sprint mechanic - if player not near any hostile mobs (could mark all hostiles as spawn in? and check 1Hz? dist>16?) for some time (10s?) then give sprint back?
	render's consumable idea - craft tulip into food, then e.g. can give non-renewable food 'in world' rather than in a chest; also e.g. shrubs drop sticks, starve of wood, have craft table in environment
	using glass/panes tactically is fun


	Sea Level: 120
	River Size: 5
	DepthBaseSize: 17
	LowerLimit: 512
	UpperLimit: 512

	caves always go up to max of 128, other params dont' seem to affcet caves
	mineshafts seem to generate anywhere below sea level
	ravines only go up to max 68

	it is not clear that custom terrain will make game any more 'fun'



Figure out custom terrain to use

QFE? need master list of all items, then curate for obtainable; rest should be straighforward?
also the qfe could be an existing teleporter once i unlock first teleporter; first visit to QFE room could actually start the qfe game?


*)

open Recipes

let PK_Enderchest(outsideItem,insideItem) =
    PatternKey([|"XXX";"XOX";"XXX"|],[|'X',MC(outsideItem); 'O',MC(insideItem)|])
let PK_Clock(outsideItem,insideItem) =
    PatternKey([|" X ";"XOX";" X "|],[|'X',MC(outsideItem); 'O',MC(insideItem)|])
let PK_Dispenser(outsideItem,insideItem,bottomItem) =
    PatternKey([|"XXX";"XOX";"X$X"|],[|'X',MC(outsideItem); 'O',MC(insideItem); '$',MC(bottomItem)|])

let FRAGMENT = "nametag"

// TODO parent-prerequisites
let custom_survival_recipes = [|
    // recipes to unlock once
    "iron_ingot",ShapedCrafting(PK_Enderchest("iron_ore","coal"),MC"iron_ingot",1)
    "gold_ingot",ShapedCrafting(PK_Clock("gold_ore","coal"),MC"gold_ingot",2)
    "cook_cod",ShapedCrafting(PK_Enderchest("cod","coal"),MC"cooked_cod",6)
    "cook_salmon",ShapedCrafting(PK_Enderchest("salmon","coal"),MC"cooked_salmon",6)
    //"throwable_torch",ShapedCrafting(PK_Dispenser("torch","slimeball","snowball"),MC"snowball",7) // TODO nbt

    // consumables to craft at any time
    //"warp_point",ShapelessCrafting([|MC(FRAGMENT);MC"purple_dye"|],MC"shulker_spawn_egg",1) // TODO nbt (name, loot table)
    "shulker_box",ShapelessCrafting([|MC(FRAGMENT);MC"chest"|],MC"purple_shulker_box",1)
    |]

let lootable_recipes = [|
    // TODO what are other good ones?
    [MC"daylight_sensor"]
    [for c in MC_Constants.COLORS do yield MC(c+"_stained_glass")]
    |] // init: takes these recipes; loot tables for dungeons etc have rare chance to give KB with the set
// TODO parent-prerequisites
let custom_unlocking_recipes = [|
    // recipe name, ingredients in addition to fragment, recipes granted, TODO-name-of-knowledge-book
    "unlock_stone_tools", [MC"cobblestone"], [MC"stone_pickaxe";MC"stone_axe";MC"stone_shovel";MC"stone_sword"]
    "unlock_iron_tools", [MC"iron_ingot"], [MC"iron_pickaxe";MC"iron_axe";MC"iron_shovel";MC"iron_sword"]
    "unlock_diamond_tools", [MC"diamond"], [MC"diamond_pickaxe";MC"diamond_axe";MC"diamond_shovel";MC"diamond_sword"]
    "unlock_iron_armor", [MC"iron_block"], [MC"iron_helmet";MC"iron_chestplate";MC"iron_leggings";MC"iron_boots"]
    "unlock_diamond_armor", [MC"diamond_block"], [MC"diamond_helmet";MC"diamond_chestplate";MC"diamond_leggings";MC"diamond_boots"]
    "unlock_bow", [MC"stick"], [MC"bow"]
    "unlock_fishing_rod", [MC"string"], [MC"fishing_rod"]
    "unlock_hoes", [MC"cobblestone";MC"iron_ingot";MC"gold_ingot"], [MC"wooden_hoe";MC"stone_hoe";MC"iron_hoe";MC"gold_hoe";MC"diamond_hoe"]
    "unlock_bone_meal", [MC"bone"], [MC"bone_meal"]
    "unlock_bed", [MC"white_wool"], [for c in MC_Constants.COLORS do yield MC(c+"_bed")]
    "unlock_anvil", [MC"iron_ingot";MC"iron_ingot";MC"iron_ingot"], [MC"anvil"]
    "unlock_piston", [MC"redstone"], [MC"piston"]
    "unlock_enchanting_table", [MC"book"], [MC"enchanting_table"]
    "unlock_bucket", [MC"glass_bottle"], [MC"bucket"]
    |]
//for recipe_name, _ingredients, knowledge_grants in custom_unlocking_recipes do
    // init: for r in knowledge_grants do /recipe take @p r
    // author advancement: trigger: recipe_unlocked, recipes: knowledge_grants.[0], rewards: function that "/recipe take @p recipe_name" and also "/recipe give @p child_recipe"
    // author recipe, once knowledge_book nbt is possible


// TODO parent-prerequisites logic
// init: grant all with no parents (elsewhere advancement unlocks children)

// TODO datapack that replaces igloo, and then daapack disables once igloo pre-reqs met

(*
loot table design changes:

abandoned_mineshaft_chest and simple_dungeon and desert_pyramid and jungle_temple and nether_bridge:
 - iron ingots -> iron nuggets
 - gold_ingots -> gold_nuggets
 - bucket -> glass or maybe one of the random lootable_recipes?

 could basically do a search&replace in the file, and add one more to 'pools' for the rare extra

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
        yield sprintf"check_mined_%s"suffixF,[|
            for b,bi in blocks do
                yield sprintf """execute if entity @s[scores={%s%s=1..}] at @s at @e[type=item,distance=..7,sort=nearest,limit=1,nbt={Item:{id:"minecraft:%s"}}] run function tc:chop_start_%s_%s""" b suffixS bi suffixF bi
                // TODO consider PickupDelay:10s as a filter on item-just-broken maybe
            |]
        for b,bi in blocks do
            yield sprintf "chop_start_%s_%s" suffixF bi,[|
                sprintf "scoreboard players set @s remain%s 500" suffixS  // TODO appropriate limit?
                sprintf "function tc:chop_check_%s_%s" suffixF bi
                sprintf "function tc:reset_stats_%s" suffixF
                |]
            yield sprintf "chop_check_%s_%s" suffixF bi,[|
                sprintf "execute if entity @s[scores={remain%s=1..}] run function tc:chop_body_%s_%s" suffixS suffixF bi
                |]
            yield sprintf "chop_body_%s_%s" suffixF bi,[|
                yield sprintf "setblock ~ ~ ~ air destroy"
                yield sprintf "scoreboard players remove @s remain%s 1" suffixS
                for dir in dirs do
                    yield sprintf "execute offset %s if block ~ ~ ~ %s run function tc:chop_check_%s_%s" dir b suffixF bi
                |]
    |]
let tc_main() =
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "TestSize")
    Utilities.writeDatapackMeta(world,"multi_miner","cut down entire trees or mine entire ore veins")
    for name,code in connected_mining_functions do
        Utilities.writeFunctionToDisk(world,"multi_miner","tc",name,code)


// TODO some recipe in woodland mansion (need to heal zombie villager to get carto to find?)

// TODO vanilla swirl all-seeing eye (any others?)

// TODO desire lines?

// TODO how to communicate initial (and permanent, e.g. furnace craft) limitations to player at start or along way?  
// what is right way to communicate deviations from vanilla without giving eveything away and taking away mystery?




