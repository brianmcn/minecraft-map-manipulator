module EandT

open LootTables
//dungeon loot: what books would I want? (sample of 15 dungeons: 10 had 2 chests, 5 had 1 chest, so a 60% rate of upgrades is appropriate)

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
                    yield Item("minecraft:enchanted_book",     [SetNbt(sprintf "{tag:{StoredEnchantments:[{id:%ds,lvl:%ds}]}}" id lvl)]), 1, 0, []
            ])
        Pool(Roll(1,1),[Item("minecraft:anvil",[]),1,0,[]])
        // TODO - loot (??? 24 gold, 12 iron, 4 diamonds, food?, other? books?)
        // TODO enderchest?
        // TODO saddles/horsearmor/nametags?

    ]

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

//DISPLAY

let displayCmds = [
    U("""execute @a[score_statSpeed_min=-2,score_statSpeed=-2] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statSpeed_min=-1,score_statSpeed=-1] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"green","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statSpeed_min=0,score_statSpeed=0] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statSpeed_min=1,score_statSpeed=1] ~ ~ ~ tellraw @a ["Speed:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"+1"},""]""")

    U("""execute @a[score_statHaste_min=-2,score_statHaste=-2] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statHaste_min=-1,score_statHaste=-1] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"green","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statHaste_min=0,score_statHaste=0] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statHaste_min=1,score_statHaste=1] ~ ~ ~ tellraw @a ["Haste:\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-2"},"\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"+1"},""]""")

    U("""execute @a[score_statStrength_min=-1,score_statStrength=-1] ~ ~ ~ tellraw @a ["Strength:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"green","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statStrength_min=0,score_statStrength=0] ~ ~ ~ tellraw @a ["Strength:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"+1"},""]""")
    U("""execute @a[score_statStrength_min=1,score_statStrength=1] ~ ~ ~ tellraw @a ["Strength:\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020\u0020",{"color":"gray","text":"-1"},"\u0020\u0020\u0020\u0020",{"color":"gray","text":"0"},"\u0020\u0020\u0020\u0020",{"color":"green","text":"+1"},""]""")

    U("""execute @a[score_statRecipe_min=0,score_statRecipe=0] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":""},{"color":"gray","text":"Furnace, I. Armor, D. Armor, Bucket, D. Pick"}]""")
    U("""execute @a[score_statRecipe_min=1,score_statRecipe=1] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace"},{"color":"gray","text":", I. Armor, D. Armor, Bucket, D. Pick"}]""")
    U("""execute @a[score_statRecipe_min=2,score_statRecipe=2] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, I. Armor"},{"color":"gray","text":", D. Armor, Bucket, D. Pick"}]""")
    U("""execute @a[score_statRecipe_min=3,score_statRecipe=3] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, I. Armor, D. Armor"},{"color":"gray","text":", Bucket, D. Pick"}]""")
    U("""execute @a[score_statRecipe_min=4,score_statRecipe=4] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, I. Armor, D. Armor, Bucket"},{"color":"gray","text":", D. Pick"}]""")
    U("""execute @a[score_statRecipe_min=5,score_statRecipe=5] ~ ~ ~ tellraw @a ["Recipe: ",{"color":"green","text":"Furnace, I. Armor, D. Armor, Bucket, D. Pick"},{"color":"gray","text":""}]""")
    ]


//BUY

// tag a buyer, then call this with desired thingy
let cmdsToBuy(price,displayName,objectiveName,maxValue) = [
    U(sprintf """scoreboard players test @p[tag=buyer] coins * %d""" (price-1))
    C(sprintf """tellraw @a ["You can't afford this, you only have ",{"score":{"name":"@p","objective":"coins"}}," coins"]""")
    U(sprintf """scoreboard players test @p[tag=buyer] coins %d *""" price)
    C(sprintf """scoreboard players test @p[tag=buyer] %s %d *""" objectiveName maxValue)
    C(sprintf """tellraw @a ["You can't buy more %s, it's already maxxed out"]""" displayName)
    U(sprintf """scoreboard players test @p[tag=buyer] coins %d *""" price)
    C(sprintf """scoreboard players test @p[tag=buyer] %s * %d""" objectiveName (maxValue-1))
    C(sprintf """scoreboard players remove @p[tag=buyer] coins %d""" price)
    C(sprintf """scoreboard players add @p[tag=buyer] %s 1""" objectiveName)
    U(sprintf """scoreboard players tag @p[tag=buyer] remove buyer""")
    ]

//EFFECTS

let ongoingStatusEffects = [
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
    ]

(*

GAIN COINS
(see loot tables below)
scoreboard objectives add Rails stat.craftItem.minecraft.rail
scoreboard players tag @a[score_Rails_min=1] add justrail
scoreboard players remove @a[tag=justrail] Rails 1
scoreboard players add @a[tag=justrail] coins 10
scoreboard players tag @a[tag=justrail] remove justrail
// todo sounds/text

todo emeralds

LOOT TABLES
// dung loot: placeable command block, command summons invisible AS inside it tagged loot
execute @e[tag=loot] ~ ~ ~ scoreboard players add @a coins 100  // ? fixed number?
execute @e[tag=loot] ~ ~ ~ setblock ~ ~ ~ chest 2 {LootTable:blah}  // what loot?
kill @e[tag=loot]

what loot in dung chests? gold? anvil? book?
what loot in 'buy loot bag'? (??? 24 gold, 12 iron, 4 diamonds, food?, other? books? anvils?)

DEATH CHECK

scoreboard objectives add Deaths stat.deaths
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
