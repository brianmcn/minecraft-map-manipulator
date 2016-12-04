module EandT

let COINS_PER_RAIL = 1
let COINS_PER_EMERALD = 20

let RECIPE_PRICE = 300
let CHEST_PRICE = 240
let SPEED_PRICE = 200
let HASTE_PRICE = 200
let STRENGTH_PRICE = 200

open LootTables
// TODO if keep mineshafts, probably give them same emerald table as dungeons, and not make dungeon rate as high...
//dungeon loot: what books would I want? (sample of 15 dungeons: 10 had 2 chests, 5 had 1 chest, so a 60% rate of upgrades is appropriate)
// desert temples off, which also controls igloos and jungle temples, village chests? (mansions on)

// world suggestions: biome 4 (default), water lake rarity up, dungeon rate up to 28, temples off - actually, can leave temples on, no huge issue, right? recipes still block progress?

let dungeonLoot = Pools [Pool(Roll(1,1),[Item("minecraft:emerald",[SetCount(7,13)]),1,0,[]])]

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
                    //POW[2]
                    PUNCH[2]
                    INF[1]
                ] do
                    yield Item("minecraft:enchanted_book",     [SetNbt(sprintf "{StoredEnchantments:[{id:%ds,lvl:%ds}]}" id lvl)]), 1, 0, []
            ])
        Pool(Roll(1,1),[Item("minecraft:gold_ingot",[SetCount(10,15)]),1,0,[]])
        Pool(Roll(1,1),[Item("minecraft:iron_ingot",[SetCount(4,8)]),1,0,[]])
        Pool(Roll(1,1),[Item("minecraft:diamond",[SetCount(0,2)]),1,0,[]])
        Pool(Roll(1,1),[Item("minecraft:ender_chest",[SetCount(0,1)]),1,0,[]])
        Pool(Roll(1,1),[Item("minecraft:stone_pickaxe",[SetCount(0,1);SetNbt("{ench:[{id:33s,lvl:1s}]}")]),1,0,[]])  // silk touch, to help with enderchests
    ]

let LOOT_TABLES =
    [|
        "minecraft:chests/simple_dungeon", dungeonLoot
//        "minecraft:chests/abandoned_mineshaft", dungeonLoot    // leave as normal, to have normal loot avail - ok to find diamonds, still can't craft much with them unless unlocked
        "minecraft:chests/village_blacksmith", dungeonLoot   // no obsdian
        "brianloot:boon", boonLoot
    |]

//bows offset the weakness... arrow ingedients or arrows? (will there be animals and feathers?)


open RegionFiles

// SCOREBOARD INIT

let initCmds = [|
    O ""
    U "gamerule logAdminCommands false"
    U "gamerule commandBlockOutput false"
    U "gamerule sendCommandFeedback false"
    U "gamerule disableElytraMovementCheck true"
    U "gamerule keepInventory true"

    U """scoreboard objectives add statSpeed dummy"""
    U """scoreboard objectives add statHaste dummy"""
    U """scoreboard objectives add statStrength dummy"""
    U """scoreboard objectives add coins dummy"""
    U """scoreboard objectives add recipe dummy"""
    U """scoreboard objectives add chest dummy"""
    U """scoreboard objectives add numBought dummy"""

    U """scoreboard objectives add Deaths stat.deaths"""
    U """scoreboard objectives add Rails stat.craftItem.minecraft.rail"""

    U """scoreboard objectives add display trigger"""
    U """scoreboard objectives add buyRecipe trigger"""
    U """scoreboard objectives add buyChest trigger"""
    U """scoreboard objectives add buySpeed trigger"""
    U """scoreboard objectives add buyHaste trigger"""
    U """scoreboard objectives add buyStrength trigger"""
    
    U """scoreboard objectives add craftFurnace stat.craftItem.minecraft.furnace"""
    U """scoreboard objectives add craftBed stat.craftItem.minecraft.bed"""
    U """scoreboard objectives add craftAnvil stat.craftItem.minecraft.anvil"""
    U """scoreboard objectives add craftIHelmet stat.craftItem.minecraft.iron_helmet"""
    U """scoreboard objectives add craftIChestplate stat.craftItem.minecraft.iron_chestplate"""
    U """scoreboard objectives add craftILeggings stat.craftItem.minecraft.iron_leggings"""
    U """scoreboard objectives add craftIBoots stat.craftItem.minecraft.iron_boots"""
    U """scoreboard objectives add craftBow stat.craftItem.minecraft.bow"""
    U """scoreboard objectives add craftDHelmet stat.craftItem.minecraft.diamond_helmet"""
    U """scoreboard objectives add craftDChestplate stat.craftItem.minecraft.diamond_chestplate"""
    U """scoreboard objectives add craftDLeggings stat.craftItem.minecraft.diamond_leggings"""
    U """scoreboard objectives add craftDBoots stat.craftItem.minecraft.diamond_boots"""
    U """scoreboard objectives add craftBucket stat.craftItem.minecraft.bucket"""
    U """scoreboard objectives add craftShield stat.craftItem.minecraft.shield"""
    U """scoreboard objectives add craftDPick stat.craftItem.minecraft.diamond_pickaxe"""

    U """scoreboard players set @a statSpeed -2"""
    U """scoreboard players set @a statHaste -2"""
    U """scoreboard players set @a statStrength -2"""
    U """scoreboard players set @a coins 0"""
    U """scoreboard players set @a recipe 0"""
    U """scoreboard players set @a chest 0"""
    U """scoreboard players set @a Deaths 0"""
    U """scoreboard players set @a Rails 0"""

    U """scoreboard players set @a display 0"""
    U """scoreboard players set @a buyRecipe 0"""
    U """scoreboard players set @a buyChest 0"""
    U """scoreboard players set @a buySpeed 0"""
    U """scoreboard players set @a buyHaste 0"""
    U """scoreboard players set @a buyStrength 0"""
    U """scoreboard players set @a numBought 0"""
    
    U """scoreboard players set @a craftFurnace 0"""
    U """scoreboard players set @a craftBed 0"""
    U """scoreboard players set @a craftAnvil 0"""
    U """scoreboard players set @a craftIHelmet 0"""
    U """scoreboard players set @a craftIChestplate 0"""
    U """scoreboard players set @a craftILeggings 0"""
    U """scoreboard players set @a craftIBoots 0"""
    U """scoreboard players set @a craftBow 0"""
    U """scoreboard players set @a craftDHelmet 0"""
    U """scoreboard players set @a craftDChestplate 0"""
    U """scoreboard players set @a craftDLeggings 0"""
    U """scoreboard players set @a craftDBoots 0"""
    U """scoreboard players set @a craftBucket 0"""
    U """scoreboard players set @a craftShield 0"""
    U """scoreboard players set @a craftDPick 0"""
    |]

// tag a buyer first
let computePRICE(basePrice) = [|
    U(sprintf """scoreboard players set PRICE coins %d""" basePrice)
    U(sprintf """scoreboard players set TAX coins 10""")
    U(sprintf """scoreboard players operation TAX coins *= @p[tag=buyer] numBought""")
    U(sprintf """scoreboard players operation PRICE coins += TAX coins""")
    |]

//DISPLAY

let displayCmds = [|
    yield P ""
    yield U "scoreboard players test @a display 1 *"
    yield C "scoreboard players set @a display 0"
    yield C "blockdata ~ ~ ~2 {auto:1b}"
    yield C "blockdata ~ ~ ~1 {auto:0b}"
    yield O ""
    yield U """tellraw @a [""]"""

//    yield U("""execute @a[score_statSpeed_min=-2,score_statSpeed=-2] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
//    yield U("""execute @a[score_statSpeed_min=-1,score_statSpeed=-1] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"green","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
//    yield U("""execute @a[score_statSpeed_min=0,score_statSpeed=0] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
//    yield U("""execute @a[score_statSpeed_min=1,score_statSpeed=1] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"+1"},""]""")
//
//    yield U("""execute @a[score_statHaste_min=-2,score_statHaste=-2] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
//    yield U("""execute @a[score_statHaste_min=-1,score_statHaste=-1] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"green","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
//    yield U("""execute @a[score_statHaste_min=0,score_statHaste=0] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
//    yield U("""execute @a[score_statHaste_min=1,score_statHaste=1] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"+1"},""]""")
//
//    yield U("""execute @a[score_statStrength_min=-2,score_statStrength=-2] ~ ~ ~ tellraw @a ["Strength:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
//    yield U("""execute @a[score_statStrength_min=-1,score_statStrength=-1] ~ ~ ~ tellraw @a ["Strength:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"green","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
//    yield U("""execute @a[score_statStrength_min=0,score_statStrength=0] ~ ~ ~ tellraw @a ["Strength:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
//    yield U("""execute @a[score_statStrength_min=1,score_statStrength=1] ~ ~ ~ tellraw @a ["Strength:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"+1"},""]""")

    yield U("""execute @a[score_statSpeed_min=-2,score_statSpeed=-2] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},""]""")
    yield U("""execute @a[score_statSpeed_min=-1,score_statSpeed=-1] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"green","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},""]""")
    yield U("""execute @a[score_statSpeed_min=0,score_statSpeed=0] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"0"},""]""")
    yield U("""execute @a[score_statSpeed_min=1,score_statSpeed=1] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},""]""")

    yield U("""execute @a[score_statHaste_min=-2,score_statHaste=-2] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},""]""")
    yield U("""execute @a[score_statHaste_min=-1,score_statHaste=-1] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"green","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},""]""")
    yield U("""execute @a[score_statHaste_min=0,score_statHaste=0] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"0"},""]""")
    yield U("""execute @a[score_statHaste_min=1,score_statHaste=1] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},""]""")

    yield U("""execute @a[score_statStrength_min=-2,score_statStrength=-2] ~ ~ ~ tellraw @a ["Strength:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},""]""")
    yield U("""execute @a[score_statStrength_min=-1,score_statStrength=-1] ~ ~ ~ tellraw @a ["Strength:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"green","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},""]""")
    yield U("""execute @a[score_statStrength_min=0,score_statStrength=0] ~ ~ ~ tellraw @a ["Strength:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"0"},""]""")
    yield U("""execute @a[score_statStrength_min=1,score_statStrength=1] ~ ~ ~ tellraw @a ["Strength:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},""]""")

    yield U("""execute @a[score_recipe_min=0,score_recipe=0] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":""},{"color":"gray","text":"Furnace, Bed, Anvil, I. Armor, Bow"}]""")
    yield U("""execute @a[score_recipe_min=0,score_recipe=0] ~ ~ ~ tellraw @a ["\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":""},{"color":"gray","text":"D. Armor, Bucket, Shield, D. Pick"}]""")

    yield U("""execute @a[score_recipe_min=1,score_recipe=1] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace"},{"color":"gray","text":", Bed, Anvil, I. Armor, Bow"}]""")
    yield U("""execute @a[score_recipe_min=1,score_recipe=1] ~ ~ ~ tellraw @a ["\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":""},{"color":"gray","text":"D. Armor, Bucket, Shield, D. Pick"}]""")

    yield U("""execute @a[score_recipe_min=2,score_recipe=2] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, Bed"},{"color":"gray","text":", Anvil, I. Armor, Bow"}]""")
    yield U("""execute @a[score_recipe_min=2,score_recipe=2] ~ ~ ~ tellraw @a ["\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":""},{"color":"gray","text":"D. Armor, Bucket, Shield, D. Pick"}]""")

    yield U("""execute @a[score_recipe_min=3,score_recipe=3] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, Bed, Anvil"},{"color":"gray","text":", I. Armor, Bow"}]""")
    yield U("""execute @a[score_recipe_min=3,score_recipe=3] ~ ~ ~ tellraw @a ["\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":""},{"color":"gray","text":"D. Armor, Bucket, Shield, D. Pick"}]""")

    yield U("""execute @a[score_recipe_min=4,score_recipe=4] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, Bed, Anvil, I. Armor"},{"color":"gray","text":", Bow"}]""")
    yield U("""execute @a[score_recipe_min=4,score_recipe=4] ~ ~ ~ tellraw @a ["\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":""},{"color":"gray","text":"D. Armor, Bucket, Shield, D. Pick"}]""")

    yield U("""execute @a[score_recipe_min=5,score_recipe=5] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, Bed, Anvil, I. Armor, Bow"},{"color":"gray","text":""}]""")
    yield U("""execute @a[score_recipe_min=5,score_recipe=5] ~ ~ ~ tellraw @a ["\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":""},{"color":"gray","text":"D. Armor, Bucket, Shield, D. Pick"}]""")

    yield U("""execute @a[score_recipe_min=6,score_recipe=6] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, Bed, Anvil, I. Armor, Bow"},{"color":"gray","text":""}]""")
    yield U("""execute @a[score_recipe_min=6,score_recipe=6] ~ ~ ~ tellraw @a ["\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"D. Armor"},{"color":"gray","text":", Bucket, Shield, D. Pick"}]""")

    yield U("""execute @a[score_recipe_min=7,score_recipe=7] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, Bed, Anvil, I. Armor, Bow"},{"color":"gray","text":""}]""")
    yield U("""execute @a[score_recipe_min=7,score_recipe=7] ~ ~ ~ tellraw @a ["\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"D. Armor, Bucket"},{"color":"gray","text":", Shield, D. Pick"}]""")

    yield U("""execute @a[score_recipe_min=8,score_recipe=8] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, Bed, Anvil, I. Armor, Bow"},{"color":"gray","text":""}]""")
    yield U("""execute @a[score_recipe_min=8,score_recipe=8] ~ ~ ~ tellraw @a ["\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"D. Armor, Bucket, Shield"},{"color":"gray","text":", D. Pick"}]""")

    yield U("""execute @a[score_recipe_min=9,score_recipe=9] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, Bed, Anvil, I. Armor, Bow"},{"color":"gray","text":""}]""")
    yield U("""execute @a[score_recipe_min=9,score_recipe=9] ~ ~ ~ tellraw @a ["\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"D. Armor, Bucket, Shield, D. Pick"},{"color":"gray","text":""}]""")

    yield U("""tellraw @a ["You have ",{"color":"yellow","score":{"name":"@a","objective":"coins"}}," coins"]""")

    yield U("""scoreboard players tag @p add buyer""")
    yield! computePRICE(SPEED_PRICE)
    yield U("""scoreboard players operation SPEED_PRICE coins = PRICE coins""")
    yield! computePRICE(HASTE_PRICE)
    yield U("""scoreboard players operation HASTE_PRICE coins = PRICE coins""")
    yield! computePRICE(STRENGTH_PRICE)
    yield U("""scoreboard players operation STRENGTH_PRICE coins = PRICE coins""")
    yield! computePRICE(RECIPE_PRICE)
    yield U("""scoreboard players operation RECIPE_PRICE coins = PRICE coins""")
    yield! computePRICE(CHEST_PRICE)
    yield U("""scoreboard players operation CHEST_PRICE coins = PRICE coins""")
    yield U("""scoreboard players tag @p[tag=buyer] remove buyer""")

    yield U("""tellraw @a [{"text":"[Buy Speed (","extra":[{"score":{"name":"SPEED_PRICE","objective":"coins"}},")]"],"clickEvent":{"action":"run_command","value":"/trigger buySpeed set 1"}},"\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"text":"[Buy Next Recipe (","extra":[{"score":{"name":"RECIPE_PRICE","objective":"coins"}},")]"],"clickEvent":{"action":"run_command","value":"/trigger buyRecipe set 1"}}]""")
    yield U("""tellraw @a [{"text":"[Buy Haste (","extra":[{"score":{"name":"HASTE_PRICE","objective":"coins"}},")]"],"clickEvent":{"action":"run_command","value":"/trigger buyHaste set 1"}},"\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"text":"[Buy Loot Chest (","extra":[{"score":{"name":"CHEST_PRICE","objective":"coins"}},")]"],"clickEvent":{"action":"run_command","value":"/trigger buyChest set 1"}}]""")
    yield U("""tellraw @a [{"text":"[Buy Strength (","extra":[{"score":{"name":"STRENGTH_PRICE","objective":"coins"}},")]"],"clickEvent":{"action":"run_command","value":"/trigger buyStrength set 1"}}]""")
    |]

//BUY

let cmdsToBuy(buyObjectiveName,basePrice,displayName,objectiveName,maxValue) = [|
    yield P ""
    yield U(sprintf """testfor @p[score_%s_min=1]""" buyObjectiveName)
    yield C "blockdata ~ ~ ~2 {auto:1b}"
    yield C "blockdata ~ ~ ~1 {auto:0b}"
    yield O ""

    yield U(sprintf """scoreboard players tag @p[score_%s_min=1] add buyer""" buyObjectiveName)
    yield U(sprintf """scoreboard players set @p[tag=buyer] %s 0""" buyObjectiveName)

    yield! computePRICE(basePrice)

    yield U(sprintf """scoreboard players operation DIFF coins = @p[tag=buyer] coins""")
    yield U(sprintf """scoreboard players operation DIFF coins -= PRICE coins""")
    yield U(sprintf """scoreboard players test DIFF coins * -1""")
    yield C(sprintf """tellraw @p[tag=buyer] ["You can't afford this, you only have ",{"score":{"name":"@p[tag=buyer]","objective":"coins"}}," coins"]""")

    yield U(sprintf """scoreboard players test DIFF coins 0 *""")
    yield C(sprintf """scoreboard players test @p[tag=buyer] %s %d *""" objectiveName maxValue)
    yield C(sprintf """tellraw @p[tag=buyer] ["You can't buy more %s, it's already maxxed out"]""" displayName)

    yield U(sprintf """scoreboard players test DIFF coins 0 *""")
    yield C(sprintf """scoreboard players test @p[tag=buyer] %s * %d""" objectiveName (maxValue-1))
    yield C(sprintf """scoreboard players operation @p[tag=buyer] coins -= PRICE coins""")
    yield C(sprintf """scoreboard players add @p[tag=buyer] %s 1""" objectiveName)
    yield C(sprintf """scoreboard players add @p[tag=buyer] numBought 1""")
    yield C(sprintf """scoreboard players set @p[tag=buyer] display 1""")

    yield U(sprintf """scoreboard players tag @p[tag=buyer] remove buyer""")
    |]

//EFFECTS

let ongoingStatusEffects = [|
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

    // strength -2
    U """effect @a[score_statStrength_min=-2,score_statStrength=-2] weakness 5 0 true"""    // -4
    // strength -1
    U """effect @a[score_statStrength_min=-1,score_statStrength=-1] weakness 5 1 true"""    // -2
    U """effect @a[score_statStrength_min=-1,score_statStrength=-1] strength 5 1 true"""
    // strength +1
    U """effect @a[score_statStrength_min=1,score_statStrength=1] strength 5 1 true"""      // +3
    |]

//GAIN COINS
let ongoingCoinsRules(coinsPerRailSetCrafted, coinsPerEmerald) = [|
    P ""
    // (for lack of a better place to put this code)
    U """scoreboard players enable @a display"""
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
    U """scoreboard players set @a[score_Rails_min=1] display 1"""
    U """scoreboard players remove @a[score_Rails_min=1] Rails 1"""

    U """clear @a minecraft:emerald 0 1"""
    C(sprintf """scoreboard players add @a coins %d""" coinsPerEmerald)
    C """scoreboard players set @a display 1"""
    // todo sounds/text
    |]

//DEATH CHECK

let ongoingDeathCheck = [|
    P ""
    U """scoreboard players tag @a[score_Deaths_min=1] add justdied"""
    U """scoreboard players set @a[tag=justdied] Deaths 0"""
    U """scoreboard players set @a[tag=justdied] coins 0"""
    U """tellraw @p[tag=justdied] ["Upon respawning, you discover you've lost all your coins"]"""
    U """scoreboard players tag @p[tag=justdied] remove justdied"""
    |]

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

    yield! recipeHelp(1,"craftBed","bed","wool",3)

    yield! recipeHelp(2,"craftAnvil","anvil","iron_ingot",31)

    yield! recipeHelp(3,"craftIHelmet","iron_helmet","iron_ingot",5)
    yield! recipeHelp(3,"craftIChestplate","iron_chestplate","iron_ingot",8)
    yield! recipeHelp(3,"craftILeggings","iron_leggings","iron_ingot",7)
    yield! recipeHelp(3,"craftIBoots","iron_boots","iron_ingot",4)

    yield! recipeHelp(4,"craftBow","bow","string",3)

    yield! recipeHelp(5,"craftDHelmet","diamond_helmet","diamond",5)
    yield! recipeHelp(5,"craftDChestplate","diamond_chestplate","diamond",8)
    yield! recipeHelp(5,"craftDLeggings","diamond_leggings","diamond",7)
    yield! recipeHelp(5,"craftDBoots","diamond_boots","diamond",4)

    yield! recipeHelp(6,"craftBucket","bucket","iron_ingot",3)

    yield! recipeHelp(7,"craftShield","shield","iron_ingot",1)

    yield! recipeHelp(8,"craftDPick","diamond_pickaxe","diamond",3)
    |]

//////////////////////////////////////

// TODO balance loot

let populateWorld(regionDir) =
    let map = new MapFolder(regionDir)
    let r = map.GetRegion(0,0)
    let bi = r.GetBlockInfo(0,200,1)
    printfn "%A" bi.BlockID 
    r.PlaceCommandBlocksStartingAtSelfDestruct(0,202,0,[|O"fill 1 201 0 1 191 0 air";U"fill 1 201 0 1 191 0 redstone_block"|],"kickoff",false,true)
    r.PlaceCommandBlocksStartingAtSelfDestruct(0,201,0,initCmds,"init",false,true)
    r.PlaceCommandBlocksStartingAt(0,200,0,ongoingStatusEffects,"status effects",false,true)
    r.PlaceCommandBlocksStartingAt(0,199,0,ongoingRecipeEnforcement,"recipes",false,true)
    r.PlaceCommandBlocksStartingAt(0,198,0,ongoingCoinsRules(COINS_PER_RAIL,COINS_PER_EMERALD),"coins",false,true)
    r.PlaceCommandBlocksStartingAt(0,197,0,displayCmds,"display",false,true)
    r.PlaceCommandBlocksStartingAt(0,196,0,cmdsToBuy("buyRecipe",RECIPE_PRICE,"next recipe","recipe",9),"buy recipe",false,true)
    r.PlaceCommandBlocksStartingAt(0,195,0,cmdsToBuy("buyChest",CHEST_PRICE,"loot chest","chest",999),"buy boon",false,true)
    r.PlaceCommandBlocksStartingAt(0,194,0,cmdsToBuy("buySpeed",SPEED_PRICE,"speed upgrade","statSpeed",0),"buy speed",false,true)
    r.PlaceCommandBlocksStartingAt(0,193,0,cmdsToBuy("buyHaste",HASTE_PRICE,"haste upgrade","statHaste",0),"buy haste",false,true)
    r.PlaceCommandBlocksStartingAt(0,192,0,cmdsToBuy("buyStrength",STRENGTH_PRICE,"strength upgrade","statStrength",0),"buy strength",false,true)
    r.PlaceCommandBlocksStartingAt(0,191,0,ongoingDeathCheck,"death check",false,true)
    map.WriteAll()
    let worldSaveFolder = System.IO.Path.GetDirectoryName(regionDir)
    writeLootTables(LOOT_TABLES, worldSaveFolder)

// TODO playtest ideas


///////////////////////

// VS bonus monument chests related to death penalty (heal-you-back for finding or something?)
// VS losing coins, getting from bonus, ...
// VS idea of breaking N% of spawners in an area earns something
// breaking spawner earns coins?
