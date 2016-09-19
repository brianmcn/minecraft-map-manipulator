module EandT

open LootTables
//dungeon loot: what books would I want? (sample of 15 dungeons: 10 had 2 chests, 5 had 1 chest, so a 60% rate of upgrades is appropriate)

let dungeonLoot = Pools [Pool(Roll(1,1),[Item("minecraft:emerald",[SetCount(8,12)]),1,0,[]])]

let boonLoot = 
    Pools [
        Pool(Roll(2,2), [
            for id,[|lvl|] in [
                    PROT[2]
                    MEND[1]
                    DS[3]
                    SHARP[2]
                    FF[3]
                    BP[3]
                    PROJ[3]
                    KNOCK[2]
                    EFF[2]
                    FORT[2]
                    UNBR[3]
                    POW[2]
                    PUNCH[2]
                    INF[1]
                ] do
                    yield Item("minecraft:enchanted_book",     [SetNbt(sprintf "{StoredEnchantments:[{id:%ds,lvl:%ds}]}" id lvl)]), 1, 0, []
            ])
        Pool(Roll(1,1),[Item("minecraft:anvil",[]),1,0,[]])
        Pool(Roll(1,1),[Item("minecraft:gold_ingot",[SetCount(10,15)]),1,0,[]])
        Pool(Roll(1,1),[Item("minecraft:iron_ingot",[SetCount(4,8)]),1,0,[]])
        Pool(Roll(1,1),[Item("minecraft:diamond",[SetCount(0,2)]),1,0,[]])
        Pool(Roll(1,1),[Item("minecraft:ender_chest",[SetCount(0,1)]),1,0,[]])
    ]

let LOOT_TABLES =
    [|
        "minecraft:chests/simple_dungeon", dungeonLoot
        "brianloot:boon", boonLoot
    |]




(*
bows offset the weakness... arrow ingedients or arrows? (will there be animals and feathers?)

book allows you to pick one of 3:
	recipe tree:
	 - furnace
	 - iron armor -> diamond armor
	 - bucket -> diamond pickaxe  // until one, no nether; until DP, no enchant table
	upgrade tree:                                             can go past default, reset on death?
	 - speed, speed     // slow 2 -> slow 1 -> none           --?--> speed1
	 - strength         // w2+s2 -> none                      --?--> str2
	 - haste, haste     // mf1 -> mf1+h2 -> none              --?--> haste2
	 boon tree:
	  - loot (??? 24 gold, 12 iron, 4 diamonds, food?, other? books?)

For book to show current stats, since text not conditional, we need 4x3x4x5 = 240 separate books to display stats... how to 'distribute'? 
May be better to have book that you can click to bring up stats text on the screen? How much text fits in your chat? ... about 9 lines of 50 chars on my screen, hmm
(or 20 lines when bring up chat with 't')
Speed:       -2   -1    0    +1
Strength:         -1    0    +1
Haste:       -2   -1    0    +1
Recipe: furnace -> iron armor -> 
  diamond armor -> bucket -> diamond pickaxe
Click to buy upgrade:
SPEED(100) STRENGTH(100) HASTE(100) RECIPE(150)   
Coins available: XXX
Can use \u0020 to display multiple spaces without them converting down to one space.

*)

open RegionFiles

// SCOREBOARD INIT

let initCmds = [|
    //O "AUTO "
    O ""
    U "gamerule logAdminCommands false"
    U "gamerule commandBlockOutput false"
    U "gamerule disableElytraMovementCheck true"

    U """scoreboard objectives add statSpeed dummy"""
    U """scoreboard objectives add statHaste dummy"""
    U """scoreboard objectives add statStrength dummy"""
    U """scoreboard objectives add coins dummy"""
    U """scoreboard objectives add recipe dummy"""
    U """scoreboard objectives add chest dummy"""

    U """scoreboard objectives add Deaths stat.deaths"""
    U """scoreboard objectives add Rails stat.craftItem.minecraft.rail"""

    U """scoreboard objectives add buyRecipe trigger"""
    U """scoreboard objectives add buyChest trigger"""
    U """scoreboard objectives add buySpeed trigger"""
    U """scoreboard objectives add buyHaste trigger"""
    U """scoreboard objectives add buyStrength trigger"""
    
    U """scoreboard objectives add craftFurnace stat.craftItem.minecraft.furnace"""
    U """scoreboard objectives add craftIHelmet stat.craftItem.minecraft.iron_helmet"""
    U """scoreboard objectives add craftIChestplate stat.craftItem.minecraft.iron_chestplate"""
    U """scoreboard objectives add craftILeggings stat.craftItem.minecraft.iron_leggings"""
    U """scoreboard objectives add craftIBoots stat.craftItem.minecraft.iron_boots"""
    U """scoreboard objectives add craftDHelmet stat.craftItem.minecraft.diamond_helmet"""
    U """scoreboard objectives add craftDChestplate stat.craftItem.minecraft.diamond_chestplate"""
    U """scoreboard objectives add craftDLeggings stat.craftItem.minecraft.diamond_leggings"""
    U """scoreboard objectives add craftDBoots stat.craftItem.minecraft.diamond_boots"""
    U """scoreboard objectives add craftBucket stat.craftItem.minecraft.bucket"""
    U """scoreboard objectives add craftDPick stat.craftItem.minecraft.diamond_pickaxe"""

    U """scoreboard players set @a statSpeed -2"""
    U """scoreboard players set @a statHaste -2"""
    U """scoreboard players set @a statStrength -1"""
    U """scoreboard players set @a coins 0"""
    U """scoreboard players set @a recipe 0"""
    U """scoreboard players set @a chest 0"""
    U """scoreboard players set @a Deaths 0"""
    U """scoreboard players set @a Rails 0"""

    U """scoreboard players set @a buyRecipe 0"""
    U """scoreboard players set @a buyChest 0"""
    U """scoreboard players set @a buySpeed 0"""
    U """scoreboard players set @a buyHaste 0"""
    U """scoreboard players set @a buyStrength 0"""
    
    U """scoreboard players set @a craftFurnace 0"""
    U """scoreboard players set @a craftIHelmet 0"""
    U """scoreboard players set @a craftIChestplate 0"""
    U """scoreboard players set @a craftILeggings 0"""
    U """scoreboard players set @a craftIBoots 0"""
    U """scoreboard players set @a craftDHelmet 0"""
    U """scoreboard players set @a craftDChestplate 0"""
    U """scoreboard players set @a craftDLeggings 0"""
    U """scoreboard players set @a craftDBoots 0"""
    U """scoreboard players set @a craftBucket 0"""
    U """scoreboard players set @a craftDPick 0"""
    |]


//DISPLAY

let displayCmds = [|
    O ""
    U """tellraw @a [""]"""

    U("""execute @a[score_statSpeed_min=-2,score_statSpeed=-2] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statSpeed_min=-1,score_statSpeed=-1] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"green","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statSpeed_min=0,score_statSpeed=0] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statSpeed_min=1,score_statSpeed=1] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"+1"},""]""")

    U("""execute @a[score_statHaste_min=-2,score_statHaste=-2] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statHaste_min=-1,score_statHaste=-1] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"green","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statHaste_min=0,score_statHaste=0] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statHaste_min=1,score_statHaste=1] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"+1"},""]""")

    U("""execute @a[score_statStrength_min=-1,score_statStrength=-1] ~ ~ ~ tellraw @a ["Strength:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statStrength_min=0,score_statStrength=0] ~ ~ ~ tellraw @a ["Strength:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statStrength_min=1,score_statStrength=1] ~ ~ ~ tellraw @a ["Strength:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"+1"},""]""")

    U("""execute @a[score_recipe_min=0,score_recipe=0] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":""},{"color":"gray","text":"Furnace, I. Armor, D. Armor, Bucket, D. Pick"}]""")
    U("""execute @a[score_recipe_min=1,score_recipe=1] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace"},{"color":"gray","text":", I. Armor, D. Armor, Bucket, D. Pick"}]""")
    U("""execute @a[score_recipe_min=2,score_recipe=2] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, I. Armor"},{"color":"gray","text":", D. Armor, Bucket, D. Pick"}]""")
    U("""execute @a[score_recipe_min=3,score_recipe=3] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, I. Armor, D. Armor"},{"color":"gray","text":", Bucket, D. Pick"}]""")
    U("""execute @a[score_recipe_min=4,score_recipe=4] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, I. Armor, D. Armor, Bucket"},{"color":"gray","text":", D. Pick"}]""")
    U("""execute @a[score_recipe_min=5,score_recipe=5] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, I. Armor, D. Armor, Bucket, D. Pick"},{"color":"gray","text":""}]""")
    
    U("""tellraw @a ["You have ",{"color":"yellow","score":{"name":"@a","objective":"coins"}}," coins"]""")

    U("""tellraw @a [{"text":"[Buy Speed]","clickEvent":{"action":"run_command","value":"/trigger buySpeed set 1"}}]""")
    U("""tellraw @a [{"text":"[Buy Loot Chest]","clickEvent":{"action":"run_command","value":"/trigger buyChest set 1"}}]""")
    |]


//BUY

let cmdsToBuy(buyObjectiveName,price,displayName,objectiveName,maxValue) = [|
    P ""
    U(sprintf """scoreboard players tag @p[score_%s_min=1] add buyer""" buyObjectiveName)
    U(sprintf """scoreboard players set @p[tag=buyer] %s 0""" buyObjectiveName)
    U(sprintf """scoreboard players test @p[tag=buyer] coins * %d""" (price-1))
    C(sprintf """tellraw @p[tag=buyer] ["You can't afford this, you only have ",{"score":{"name":"@p[tag=buyer]","objective":"coins"}}," coins"]""")
    U(sprintf """scoreboard players test @p[tag=buyer] coins %d *""" price)
    C(sprintf """scoreboard players test @p[tag=buyer] %s %d *""" objectiveName maxValue)
    C(sprintf """tellraw @p[tag=buyer] ["You can't buy more %s, it's already maxxed out"]""" displayName)
    U(sprintf """scoreboard players test @p[tag=buyer] coins %d *""" price)
    C(sprintf """scoreboard players test @p[tag=buyer] %s * %d""" objectiveName (maxValue-1))
    C(sprintf """scoreboard players remove @p[tag=buyer] coins %d""" price)
    C(sprintf """scoreboard players add @p[tag=buyer] %s 1""" objectiveName)
    U(sprintf """scoreboard players tag @p[tag=buyer] remove buyer""")
    |]

//EFFECTS

let ongoingStatusEffects = [|
    //P "AUTO "
    P ""
    // speed -2
    U """effect @a[score_statSpeed_min=-2,score_statSpeed=-2] slowness 5 1 true"""
    // speed -1
    U """effect @a[score_statSpeed_min=-1,score_statSpeed=-1] slowness 5 0 true"""
    // speed +1
    U """effect @a[score_statSpeed_min=1,score_statSpeed=1] speed 5 0 true"""

    // haste -2
    U """effect @a[score_statHaste_min=-2,score_statHaste=-2] mining_fatigue 5 0 true"""
    // haste -1
    U """effect @a[score_statHaste_min=-1,score_statHaste=-1] haste 5 1 true"""
    U """effect @a[score_statHaste_min=-1,score_statHaste=-1] mining_fatigue 5 0 true"""
    // haste +1
    U """effect @a[score_statHaste_min=1,score_statHaste=1] haste 5 1 true"""

    // strength -1
    U """effect @a[score_statStrength_min=-1,score_statStrength=-1] weakness 5 1 true"""
    U """effect @a[score_statStrength_min=-1,score_statStrength=-1] strength 5 1 true"""
    // strength +1
    U """effect @a[score_statStrength_min=1,score_statStrength=1] strength 5 1 true"""
    |]

//GAIN COINS
let ongoingCoinsRules(coinsPerRailSetCrafted, coinsPerEmerald) = [|
    P ""
    // (for lack of a better place to put this code)
    U """scoreboard players enable @a buyRecipe"""
    U """scoreboard players enable @a buyChest"""
    U """scoreboard players enable @a buySpeed"""
    U """scoreboard players enable @a buyHaste"""
    U """scoreboard players enable @a buyStrength"""
    // (for lack of a better place to put this code)
    U """give @p[score_chest_min=1] chest 1 0 {BlockEntityTag:{LootTable:"brianloot:boon",CustomName:"Lootz!"},display:{Name:"Lootz!"}}"""
    U """scoreboard players remove @p[score_chest_min=1] chest 1"""
    
    U(sprintf """scoreboard players add @a[score_Rails_min=1] coins %d""" coinsPerRailSetCrafted)
    // todo sounds/text
    U """scoreboard players remove @a[score_Rails_min=1] Rails 1"""

    // TODO loot tables
    
    // TODO emeralds   
    U """clear @a minecraft:emerald 0 1"""
    C(sprintf """scoreboard players add @a coins %d""" coinsPerEmerald)
    // todo sounds/text
    |]
(*

LOOT TABLES
// dung loot: placeable command block, command summons invisible AS inside it tagged loot
execute @e[tag=loot] ~ ~ ~ scoreboard players add @a coins 100  // ? fixed number?  or put emeralds in chest below? in which case, just have dung loot table with misc crap and a bunch of emeralds?
execute @e[tag=loot] ~ ~ ~ setblock ~ ~ ~ chest 2 {LootTable:blah}  // what loot?
kill @e[tag=loot]

what loot in dung chests? gold? anvil? book?
what loot in 'buy loot bag'? (??? 24 gold, 12 iron, 4 diamonds, food?, other? books? anvils?)

DEATH CHECK

scoreboard players tag @a[score_Deaths_min=1] add justdied
scoreboard players set @a[tag=justdied] Deaths 0
scoreboard players set @a[tag=justdied] coins 0
scoreboard players tag @a[tag=justdied] remove justdied
// todo sounds/text


custom terrain: no village/mineshaft/pyramid/oceanmonument, turn up dungeon rate to 25 or 30 (turn down lakes as usual, biome size as usual)


keepInventory=true?
goals, constraints, and penalties - what penalty for death?
 - lose 'points' where points gotten from kills, digging, xp, emeralds, and lots from dungeon chests, and points redeemable for upgrades?
 - make it for me and E&T - points for dungeons, crafting rails, and sure, emeralds
*)

// RECIPE ENFORCEMENT

let recipeHelp(maxScore,objective,item,ingredient,count) = [|
    U(sprintf "clear @a[score_recipe=%d,score_%s_min=1] minecraft:%s 0 1" maxScore objective item)
    C(sprintf "give @a[score_recipe=%d,score_%s_min=1] minecraft:%s %d" maxScore objective ingredient count)
    C(sprintf """tellraw @a[score_recipe=%d,score_%s_min=1] ["You haven't unlocked the recipe for %s yet"]""" maxScore objective item)
    C(sprintf "scoreboard players remove @a[score_recipe=%d,score_%s_min=1] %s 1" maxScore objective objective)
    |]

let ongoingRecipeEnforcement = [|
    yield P ""
    yield! recipeHelp(0,"craftFurnace","furnace","cobblestone",8)

    yield! recipeHelp(1,"craftIHelmet","iron_helmet","iron_ingot",5)
    yield! recipeHelp(1,"craftIChestplate","iron_chestplate","iron_ingot",8)
    yield! recipeHelp(1,"craftILeggings","iron_leggings","iron_ingot",7)
    yield! recipeHelp(1,"craftIBoots","iron_boots","iron_ingot",4)

    yield! recipeHelp(2,"craftDHelmet","diamond_helmet","diamond",5)
    yield! recipeHelp(2,"craftDChestplate","diamond_chestplate","diamond",8)
    yield! recipeHelp(2,"craftDLeggings","diamond_leggings","diamond",7)
    yield! recipeHelp(2,"craftDBoots","diamond_boots","diamond",4)

    yield! recipeHelp(3,"craftBucket","bucket","iron_ingot",3)

    yield! recipeHelp(4,"craftDPick","diamond_pickaxe","diamond",3)
    |]

//////////////////////////////////////

let populateWorld(regionDir) =
    let map = new MapFolder(regionDir)
    let r = map.GetRegion(0,0)
    r.PlaceCommandBlocksStartingAtSelfDestruct(0,202,0,[|O"fill 1 201 0 1 192 0 redstone_block"|],"kickoff")
    r.PlaceCommandBlocksStartingAtSelfDestruct(0,201,0,initCmds,"init")
    //r.AddTileTick("minecraft:command_block",100,0,0,201,0)
    r.PlaceCommandBlocksStartingAt(0,200,0,ongoingStatusEffects,"status effects",false,true)
    //r.AddTileTick("minecraft:repeating_command_block",100,0,0,200,0)
    r.PlaceCommandBlocksStartingAt(0,199,0,ongoingRecipeEnforcement,"recipes",false,true)
    r.PlaceCommandBlocksStartingAt(0,198,0,ongoingCoinsRules(10,10),"coins",false,true)
    r.PlaceCommandBlocksStartingAt(0,197,0,displayCmds,"display",false,true)
    r.PlaceCommandBlocksStartingAt(0,196,0,cmdsToBuy("buyRecipe",150,"next recipe","recipe",4),"buy recipe",false,true)
    r.PlaceCommandBlocksStartingAt(0,195,0,cmdsToBuy("buyChest",120,"loot chest","chest",999),"buy boon",false,true)
    r.PlaceCommandBlocksStartingAt(0,194,0,cmdsToBuy("buySpeed",100,"speed upgrade","statSpeed",1),"buy speed",false,true)
    r.PlaceCommandBlocksStartingAt(0,193,0,cmdsToBuy("buyHaste",100,"haste upgrade","statHaste",1),"buy haste",false,true)
    r.PlaceCommandBlocksStartingAt(0,192,0,cmdsToBuy("buyStrength",100,"strength upgrade","statStrength",1),"buy strength",false,true)
    map.WriteAll()
    let worldSaveFolder = System.IO.Path.GetDirectoryName(regionDir)
    writeLootTables(LOOT_TABLES, worldSaveFolder)
