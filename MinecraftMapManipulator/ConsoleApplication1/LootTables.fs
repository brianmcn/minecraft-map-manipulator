module LootTables

////////////////////////////////////////////

type Function =
    | SetCount of int * int
    | SetData of int
    | EnchantWithLevels of int * int * bool // min, max, treasure
    | SetDamage of float * float
    | SetNbt of string
    | EnchantRandomly of string list
    | LootingEnchant of int * int
    | FurnaceSmelt
    //| SetAttributes of TODO
type Condition =
    | KilledByPlayer
    | EntityScoresKillerPlayer of string * int * int
    // TODO others
type EntryDatum =
    | Item of string * Function list // name, functions
    | LootTable of string // name
    | Empty
type Entries = (EntryDatum * int * int * Condition list)list // weight, quality
type Rolls =
    | Roll of int * int
type Pool =
    | Pool of Rolls * Entries
type LootTable =
    | Pools of Pool list
    member this.Write(w:System.IO.TextWriter) =
        let ICH s = s |> Seq.toArray |> (fun a -> Array.init a.Length (fun i -> a.[i], if i<>0 then "," else "")) // interspersed comma helper
        w.WriteLine("""{"pools":[""")
        for (Pool(Roll(ra,rb),entries)),c in ICH(match this with Pools x -> x) do
            w.WriteLine(sprintf """    %s{"rolls":{"min":%d,"max":%d}, "entries":[""" c ra rb)
            for (datum,weight,quality,conds),c in ICH entries do
                let conditionsString = 
                    match conds with
                    | [] -> ""
                    | [KilledByPlayer] -> """, "conditions":[{"condition":"killed_by_player"}]"""
                    | [EntityScoresKillerPlayer(obj,n,x)] -> sprintf """, "conditions":[{"condition":"entity_scores","entity":"killer_player","scores":{"%s":{"min":%d,"max":%d}}}]""" obj n x
                    | _ -> failwith "NYI"
                match datum with
                | Item(name, fs) ->
                    w.Write(sprintf """        %s{"weight":%4d, "quality":%4d%s, "type":"item", "name":"%s" """ c weight quality conditionsString name)
                    if fs.Length = 0 then
                        w.WriteLine(sprintf """}""")
                    else
                        w.WriteLine()
                        w.WriteLine(sprintf """         ,"functions": [""")
                        for f,c in ICH fs do
                            match f with
                            | SetCount(a,b) -> w.WriteLine(sprintf """             %s{"function":"set_count","count":{"min":%d,"max":%d}}""" c a b)
                            | SetData d -> w.WriteLine(sprintf """             %s{"function":"set_data","data":%d}""" c d)
                            | EnchantWithLevels(a,b,t) -> w.WriteLine(sprintf """             %s{"function":"enchant_with_levels","levels":{"min":%d,"max":%d},"treasure":%A}""" c a b t)
                            | SetDamage(a,b) -> w.WriteLine(sprintf """             %s{"function":"set_damage","damage":{"min":%f,"max":%f}}""" c a b)
                            | SetNbt(s) -> w.WriteLine(sprintf """             %s{"function":"set_nbt","tag":"%s"}""" c s)
                            | EnchantRandomly(a) -> 
                                if a.Length = 0 then
                                    w.WriteLine(sprintf """             %s{"function":"enchant_randomly"}""" c)
                                else
                                    w.WriteLine(sprintf """             %s{"function":"enchant_randomly","enchantments":[""" c)
                                    for e,cc in ICH a do
                                        w.WriteLine(sprintf """                 %s"%s" """ cc e)
                                    w.WriteLine(sprintf """             ]}""")
                            | LootingEnchant(a,b) -> w.WriteLine(sprintf """             %s{"function":"looting_enchant","count":{"min":%d,"max":%d}}""" c a b)
                            | FurnaceSmelt -> w.WriteLine(sprintf """             %s{"function":"furnace_smelt"}""" c)
                        w.WriteLine("""        ]}""")
                | LootTable(name) ->
                    w.Write(sprintf """        %s{"weight":%4d, "quality":%4d%s, "type":"loot_table", "name":"%s"} """ c weight quality conditionsString name)
                    w.WriteLine()
                | Empty ->
                    w.Write(sprintf """        %s{"weight":%4d, "quality":%4d%s, "type":"empty"} """ c weight quality conditionsString)
                    w.WriteLine()
            w.WriteLine("""    ]}""")
        w.WriteLine("""]}""")

let simple_dungeon =
    Pools [
            Pool(Roll(1,3), [
                    Item("minecraft:saddle",[]), 20, 0, []
                    Item("minecraft:golden_apple",[]), 15, 0, []
                    Item("minecraft:golden_apple",[SetData 1]), 2, 0, []
                    Item("minecraft:record_13",[]), 15, 0, []
                    Item("minecraft:record_cat",[]), 15, 0, []
                    Item("minecraft:name_tag",[]), 20, 0, []
                    Item("minecraft:golden_horse_armor",[]), 10, 0, []
                    Item("minecraft:iron_horse_armor",[]), 15, 0, []
                    Item("minecraft:diamond_horse_armor",[]), 5, 0, []
                    Item("minecraft:book",[EnchantRandomly[]]), 5, 0, []
                ])
            Pool(Roll(1,4), [
                    Item("minecraft:iron_ingot",[SetCount(1,4)]), 10, 0, []
                    Item("minecraft:gold_ingot",[SetCount(1,4)]), 5, 0, []
                    Item("minecraft:bread",[]), 20, 0, []
                    Item("minecraft:wheat",[SetCount(1,4)]), 20, 0, []
                    Item("minecraft:bucket",[]), 10, 0, []
                    Item("minecraft:redstone",[SetCount(1,4)]), 15, 0, []
                    Item("minecraft:coal",[SetCount(1,4)]), 15, 0, []
                    Item("minecraft:melon_seeds",[SetCount(2,4)]), 10, 0, []
                    Item("minecraft:pumpkin_seeds",[SetCount(2,4)]), 10, 0, []
                    Item("minecraft:beetroot_seeds",[SetCount(2,4)]), 10, 0, []
                ])
            Pool(Roll(3,3), [
                    Item("minecraft:bone",[SetCount(1,8)]), 10, 0, []
                    Item("minecraft:gunpowder",[SetCount(1,8)]), 10, 0, []
                    Item("minecraft:rotten_flesh",[SetCount(1,8)]), 10, 0, []
                    Item("minecraft:string",[SetCount(1,8)]), 10, 0, []
                ])
        ]

// ench books, anvils, bottles
// blocks
// heals? IH1/2, R1/2, gapple
// aesthetic blocks

// wood tier: player acquires on own on surface, can grind for better
// stone tier: dungeons & mineshafts
// gold tier: surface dungeons I make
// iron tier: best loot from ribbons and mountains

// loot tables: e.g. armor.[1], tool.[3], etc

// mobs: drop stuff at their tier and rarely a next tier thing

let LOOT_ARMOR =
    [|
        // tier 1
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:leather_helmet",     [EnchantWithLevels(1,15,false)]), 1, 0, []
                        //Item("minecraft:leather_chestplate", [EnchantWithLevels(1,15,false)]), 1, 0
                        //Item("minecraft:leather_leggings",   [EnchantWithLevels(1,15,false)]), 1, 0
                        Item("minecraft:leather_boots",      [EnchantWithLevels(1,15,false)]), 1, 0, []
                        Item("minecraft:golden_chestplate", [EnchantWithLevels(1,15,false)]), 1, 0, []
                        Item("minecraft:golden_leggings",   [EnchantWithLevels(1,15,false)]), 1, 0, []
                               ])]
        // tier 2
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:golden_helmet",     [EnchantWithLevels(1,15,false)]), 1, 0, []
                        Item("minecraft:golden_chestplate", [EnchantWithLevels(1,15,false)]), 1, 0, []
                        Item("minecraft:golden_leggings",   [EnchantWithLevels(1,15,false)]), 1, 0, []
                        Item("minecraft:golden_boots",      [EnchantWithLevels(1,15,false)]), 1, 0, []
                               ])]
        // tier 3
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:iron_helmet",     [EnchantWithLevels(1,15,false)]), 1, 0, []
                        Item("minecraft:iron_chestplate", [EnchantWithLevels(1,15,false)]), 1, 0, []
                        Item("minecraft:iron_leggings",   [EnchantWithLevels(1,15,false)]), 1, 0, []
                        Item("minecraft:iron_boots",      [EnchantWithLevels(1,15,false)]), 1, 0, []
                               ])]
        // tier 4
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:iron_helmet",     [EnchantWithLevels(16,30,false)]), 1, 0, []
                        Item("minecraft:iron_chestplate", [EnchantWithLevels(16,30,false)]), 1, 0, []
                        Item("minecraft:iron_leggings",   [EnchantWithLevels(16,30,false)]), 1, 0, []
                        Item("minecraft:iron_boots",      [EnchantWithLevels(16,30,false)]), 1, 0, []
                               ])]
    |]

let LOOT_TOOLS =
    [|
        // tier 1
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:stone_sword",    [EnchantWithLevels( 1,15,false)]), 1, 0, []
                        Item("minecraft:wooden_pickaxe", [EnchantWithLevels(16,30,false)]), 1, 0, []
                        Item("minecraft:wooden_shovel",  [EnchantWithLevels(16,30,false)]), 1, 0, []
                        Item("minecraft:stone_axe",      [EnchantWithLevels( 1,15,false)]), 1, 0, []
                               ])]
        // tier 2
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:stone_sword",   [EnchantWithLevels(16,30,false)]), 1, 0, []
                        Item("minecraft:stone_pickaxe", [EnchantWithLevels(16,30,false)]), 1, 0, []
                        Item("minecraft:stone_shovel",  [EnchantWithLevels(16,30,false)]), 1, 0, []
                        Item("minecraft:stone_axe",     [EnchantWithLevels(16,30,false)]), 1, 0, []
                               ])]
        // tier 3
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:iron_sword",   []), 1, 0, []
                        Item("minecraft:iron_pickaxe", []), 1, 0, []
                        Item("minecraft:iron_shovel",  []), 1, 0, []
                        Item("minecraft:iron_axe",     []), 1, 0, []
                        Item("minecraft:iron_sword",   [EnchantWithLevels(1,15,false)]), 1, 0, []
                        Item("minecraft:iron_pickaxe", [EnchantWithLevels(1,15,false)]), 1, 0, []
                        Item("minecraft:iron_shovel",  [EnchantWithLevels(1,15,false)]), 1, 0, []
                        Item("minecraft:iron_axe",     [EnchantWithLevels(1,15,false)]), 1, 0, []
                               ])]
        // tier 4
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:iron_sword",   [EnchantWithLevels(16,30,false)]), 1, 0, []
                        Item("minecraft:iron_pickaxe", [EnchantWithLevels(16,30,false)]), 1, 0, []
                        Item("minecraft:iron_shovel",  [EnchantWithLevels(16,30,false)]), 1, 0, []
                        Item("minecraft:iron_axe",     [EnchantWithLevels(16,30,false)]), 1, 0, []
                               ])]
    |]

let LOOT_FOOD =
    [|
        // tier 1
        Pools [Pool(Roll(1,1), [Item("minecraft:cookie",   [SetCount(5,10)]), 1, 0, []])]
        // tier 2
        Pools [Pool(Roll(1,1), [Item("minecraft:bread",   [SetCount(3,8)]), 1, 0, []])]
        // tier 3
        Pools [Pool(Roll(1,1), [Item("minecraft:cooked_beef",   [SetCount(3,8)]), 1, 0, []])]
        // tier 4
        Pools [Pool(Roll(1,1), [Item("minecraft:golden_apple",   [SetCount(3,6)]), 1, 0, []])]
    |]

let enchantmentsInTiers =
    [|
        [
            // tier 1 has none
        ]
        [
            //"fire_protection"
            "feather_falling"
            "blast_protection"
            "projectile_protection"
            //"respiration"
            //"aqua_affinity"
            "thorns"
            "smite"
            "bane_of_arthropods"
            "knockback"
            "fire_aspect"
            "efficiency"
            "silk_touch"
            "fortune"
            "power"
            "punch"
            "flame"
            "depth_strider"
        ]
        [
            "efficiency"
            "protection"
            "frost_walker"
            "sharpness"
//            "looting"  // TODO figure out if/what looting does
            "unbreaking"
            "infinity"
        ]
        [
            "protection"
            "sharpness"
//            "looting"  // TODO figure out if/what looting does
            "infinity"
            "mending"
        ]
    |]
// unused
//            "luck_of_the_sea"
//            "lure"

let LOOT_NS_PREFIX = "BrianLoot"
let LOOT_FORMAT s n = sprintf "%s:%s%d" LOOT_NS_PREFIX s n
type LOOT_KIND = | ARMOR | TOOLS | FOOD | BOOKS //| TODO 
let P11 x = Pool(Roll(1,1),x)
let OneOfAtNPercent(entryData, n, conds) = 
    assert(n>=0 && n <=100)
    let weight = (entryData |> Seq.length)*(100-n)
    P11[yield (Empty, weight, 0, []); for ed in entryData do yield (ed, n, 0, conds)]
let tierNLootData n kinds = 
    [ for k in kinds do match k with | ARMOR -> yield LootTable(LOOT_FORMAT"armor"n) | FOOD -> yield LootTable(LOOT_FORMAT"food"n) | TOOLS -> yield LootTable(LOOT_FORMAT"tools"n) | BOOKS -> yield LootTable(LOOT_FORMAT"books"n) ]
let tierxyLootPct conds x y kinds n = // tier x at n%, but instead tier y at n/10%.... so n=10 give 10%x, 1%y, and 89% nothing
    assert(n>=0 && n <=100)
    let weight = (kinds|>Seq.length) * (1000-10*n-n)
    P11[yield (Empty, weight, 0, conds)
        for ed in tierNLootData x kinds do 
            yield (ed, 10*n, 0, conds)
        for ed in tierNLootData y kinds do 
            yield (ed, n, 0, conds)
       ]
let cobblePile = Item("minecraft:cobblestone", [SetCount(3,7)])
let ironPile = Item("minecraft:iron_ingot", [SetCount(1,3)])
let arrows = Item("minecraft:arrow", [SetCount(6,9)])
let MOB = [KilledByPlayer]
let LOOT_FROM_DEFAULT_MOBS =
    [|
//        "minecraft:entities/bat"
//        "minecraft:entities/chicken
//        "minecraft:entities/cow
//        "minecraft:entities/endermite
//        "minecraft:entities/giant
//        "minecraft:entities/horse
//        "minecraft:entities/iron_golem
//        "minecraft:entities/mushroom_cow
//        "minecraft:entities/ocelot
//        "minecraft:entities/pig
//        "minecraft:entities/rabbit
//        "minecraft:entities/sheep
//        "minecraft:entities/snowman
//        "minecraft:entities/squid
//        "minecraft:entities/wolf

        "minecraft:entities/blaze", Pools [tierxyLootPct MOB 2 2 [ARMOR;TOOLS] 33; tierxyLootPct MOB 2 2 [FOOD] 33; OneOfAtNPercent([ironPile],10,MOB)]
        "minecraft:entities/cave_spider", Pools [tierxyLootPct MOB 2 2 [ARMOR;TOOLS] 16; tierxyLootPct MOB 2 2 [FOOD] 16; OneOfAtNPercent([ironPile],10,MOB)]
        "minecraft:entities/creeper", Pools [tierxyLootPct MOB 1 2 [ARMOR;TOOLS] 10; tierxyLootPct MOB 1 2 [FOOD] 16; OneOfAtNPercent([cobblePile],10,MOB)]
//        "minecraft:entities/elder_guardian
        "minecraft:entities/enderman", Pools [Pool(Roll(1,1),[Item("minecraft:ender_pearl",[SetCount(0,1);LootingEnchant(0,1)]),0,1, []]) // usual default drop
                                              tierxyLootPct MOB 2 3 [ARMOR;TOOLS] 16; tierxyLootPct MOB 2 3 [FOOD] 16; OneOfAtNPercent([arrows],16,MOB)  // extra loot
                                             ]
        "minecraft:entities/ghast", Pools [tierxyLootPct MOB 2 2 [ARMOR;TOOLS] 33; tierxyLootPct MOB 2 2 [FOOD] 33; OneOfAtNPercent([ironPile],10,MOB)]
//        "minecraft:entities/guardian
// TODO change to command signs? https://www.reddit.com/r/MinecraftCommands/comments/3twe3w/same_chest_different_loot/
// TODO or just something 'better'... how best do? monument? 2 endstone, 1 sponge?
// TODO end stone brick with X coord, purpur brick with Z coord
        "minecraft:entities/magma_cube", Pools [
            Pool(Roll(1,1),[Item("minecraft:written_book",[SetNbt(Utilities.escape <| Utilities.writtenBookNBTString("Lorgon111","Secret Treasure",[|
                   """{"text":"The secret treasure is buried at X=","extra":[{"score":{"name":"X","objective":"hidden"}},{"text":". You'll need to pair this information with another clue, found in a similar location!"}]}"""
                   |]))]),1,0,[EntityScoresKillerPlayer("LavaSlimesKilled",1,1)]])
            Pool(Roll(1,1),[Item("minecraft:written_book",[SetNbt(Utilities.escape <| Utilities.writtenBookNBTString("Lorgon111","Secret Treasure",[|
                   """{"text":"The secret treasure is buried at Z=","extra":[{"score":{"name":"Z","objective":"hidden"}},{"text":". You'll need to pair this information with another clue, found in a similar location!"}]}"""
                   |]))]),1,0,[EntityScoresKillerPlayer("LavaSlimesKilled",2,2)]])  // Note: entity_scores evals after stat.kills updated but before next commandtick
            Pool(Roll(1,1),[Item("minecraft:written_book",[SetNbt(Utilities.escape <| Utilities.writtenBookNBTString("Lorgon111","Secret Treasure",[|
                   """{"text":"The secret treasure is buried at X=","extra":[{"score":{"name":"X","objective":"hidden"}},{"text":". You'll need to pair this information with another clue, found in a similar location!"}]}"""
                   |]))]),1,0,[EntityScoresKillerPlayer("LavaSlimesKilled",3,3)]])  // in case they somehow lose the first one?
            Pool(Roll(1,1),[Item("minecraft:written_book",[SetNbt(Utilities.escape <| Utilities.writtenBookNBTString("Lorgon111","Secret Treasure",[|
                   """{"text":"The secret treasure is buried at Z=","extra":[{"score":{"name":"Z","objective":"hidden"}},{"text":". You'll need to pair this information with another clue, found in a similar location!"}]}"""
                   |]))]),1,0,[EntityScoresKillerPlayer("LavaSlimesKilled",4,9999)]])
            ]
//        "minecraft:entities/shulker
//        "minecraft:entities/silverfish
        "minecraft:entities/skeleton", Pools [tierxyLootPct MOB 1 2 [ARMOR;TOOLS] 12; tierxyLootPct MOB 1 2 [FOOD] 16; OneOfAtNPercent([arrows],16,MOB)]
//        "minecraft:entities/skeleton_horse
//        "minecraft:entities/slime
        "minecraft:entities/spider", Pools [tierxyLootPct MOB 1 2 [ARMOR;TOOLS] 8; tierxyLootPct MOB 1 2 [FOOD] 12; OneOfAtNPercent([cobblePile],8,MOB)]
        "minecraft:entities/witch", Pools [tierxyLootPct MOB 2 3 [ARMOR;TOOLS] 10; tierxyLootPct MOB 2 3 [FOOD] 16; OneOfAtNPercent([arrows],10,MOB)]
//        "minecraft:entities/wither_skeleton
        "minecraft:entities/zombie", Pools [tierxyLootPct MOB 1 2 [ARMOR;TOOLS] 8; tierxyLootPct MOB 1 2 [FOOD] 12; OneOfAtNPercent([cobblePile],8,MOB)]
//        "minecraft:entities/zombie_horse
//        "minecraft:entities/zombie_pigman
    |]

let tierNBookItem(n) = Item("minecraft:book", [EnchantRandomly enchantmentsInTiers.[n-1]])
let veryDamagedAnvils(min,max) = Item("minecraft:anvil", [SetData 2; SetCount(min,max)])

let sampleTier1Chest =
        Pools[ Pool(Roll(1,1),[veryDamagedAnvils(2,4),1,0, []]) // TODO proper loot
             ]
let sampleTier2Chest =
        Pools[ Pool(Roll(5,5),[tierNBookItem(2),1,0, []])
               Pool(Roll(0,1),[tierNBookItem(3),1,0, []])
               Pool(Roll(1,1),[veryDamagedAnvils(2,4),1,0, []])
               Pool(Roll(0,1),[Item("minecraft:written_book",[SetNbt(Utilities.escape <| Utilities.writtenBookNBTString("Lorgon111","'What Next' hints",[|
                                            """{"text":"Once you've geared up and are wearing metal armor, you should venture out into the night looking for GREEN beacon light. A challenging path will lead to riches!"}"""
                                        |]))]),1,0, []])  // TODO now a good moment to suggest a donation?
               Pool(Roll(1,3),[arrows,1,0, []])
               tierxyLootPct [] 2 3 [FOOD] 16 
               tierxyLootPct [] 2 3 [FOOD] 16 
               OneOfAtNPercent([Item("minecraft:iron_pickaxe",[]);Item("minecraft:iron_sword",[]);Item("minecraft:iron_axe",[]);Item("minecraft:iron_ingot",[SetCount(2,9)])],50,[])
               Pool(Roll(0,1),
                    [LootTable("empty"), 20, 0, []
                     Item("minecraft:saddle",[]), 20, 0, []
                     Item("minecraft:iron_horse_armor",[]), 15, 0, []
                     Item("minecraft:diamond_horse_armor",[]), 5, 0, []])
             ]
let sampleTier3Chest =
        Pools[ Pool(Roll(12,12),[tierNBookItem(3),1,0, []])
               Pool(Roll(1,1),[veryDamagedAnvils(3,5),1,0, []])
               Pool(Roll(2,2),[Item("minecraft:chest",[SetNbt("""{display:{Name:\"Dungeon Loot\"},BlockEntityTag:{LootTable:\"minecraft:chests/simple_dungeon\"}}""")]),1,0, []])
               Pool(Roll(1,1),[Item("minecraft:experience_bottle",[SetCount(64,64)]),1,0, []])
               Pool(Roll(1,1),[Item("minecraft:diamond_pickaxe",[]),1,0, []])
               Pool(Roll(1,1),[Item("minecraft:diamond_sword",[]),1,0, []])
               Pool(Roll(1,1),[Item("minecraft:written_book",[SetNbt(Utilities.escape <| Utilities.writtenBookNBTString("Lorgon111","'What Next' hints",[|
                                            """{"text":"If you feel strong enough, look for a RED beacon and try attacking a surface area filled with cobwebs... terrific rewards await you!"}"""
                                        |]))]),1,0, []])
               Pool(Roll(1,1),[Item("minecraft:iron_ingot",[SetCount(20,30)]),1,0, []])
               Pool(Roll(1,1),[Item("minecraft:gold_ingot",[SetCount(20,30)]),1,0, []])
               Pool(Roll(1,1),[LootTable(LOOT_FORMAT"armor"4),1,0, []])
               Pool(Roll(3,3),[LootTable(LOOT_FORMAT"food"3),1,0, []])
             ]
let sampleTier4Chest =
        Pools[ Pool(Roll(5,5),[tierNBookItem(3),1,0, []])
               Pool(Roll(5,5),[tierNBookItem(4),1,0, []])
               Pool(Roll(1,1),[Item("minecraft:enchanted_book",[SetNbt("""{StoredEnchantments:[{id:0s,lvl:4s}]}""")]),1,0, []]) // protection IV book
               Pool(Roll(1,1),[veryDamagedAnvils(3,4),1,0, []])
               Pool(Roll(2,2),[Item("minecraft:chest",[SetNbt(sprintf """{display:{Name:\"Green Beacon Cave Loot\"},BlockEntityTag:{LootTable:\"%s:chests/tier3\"}}""" LOOT_NS_PREFIX)]),1,0, []])
               Pool(Roll(1,1),[Item("minecraft:diamond",[SetCount(20,30)]),1,0, []])
               Pool(Roll(3,3),[LootTable(LOOT_FORMAT"armor"4),1,0, []])
               Pool(Roll(3,3),[LootTable(LOOT_FORMAT"tools"4),1,0, []])
               Pool(Roll(3,3),[LootTable(LOOT_FORMAT"food"4),1,0, []])
               Pool(Roll(1,1),[Item("minecraft:written_book",[SetNbt(Utilities.escape <| Utilities.writtenBookNBTString("Lorgon111","'What Next' hints",[|
                                            """{"text":"Once strong enough, attack dangerous-looking mountain peaks with glassed loot boxes to get a map to the best treasure!"}"""
                                        |]))]),1,0, []])
             ]
let sampleTier5Chest =
        Pools[ Pool(Roll(5,5),[tierNBookItem(4),1,0, []])
               Pool(Roll(1,1),[veryDamagedAnvils(3,4),1,0, []])
               Pool(Roll(2,2),[Item("minecraft:chest",[SetNbt(sprintf """{display:{Name:\"Red Beacon Web Loot\"},BlockEntityTag:{LootTable:\"%s:chests/tier4\"}}""" LOOT_NS_PREFIX)]),1,0, []])
               Pool(Roll(1,1),[Item("minecraft:diamond",[SetCount(20,30)]),1,0, []])
               Pool(Roll(3,3),[LootTable(LOOT_FORMAT"armor"4),1,0, []])
               Pool(Roll(3,3),[LootTable(LOOT_FORMAT"tools"4),1,0, []])
               Pool(Roll(3,3),[LootTable(LOOT_FORMAT"food"4),1,0, []])
               // Note: alternative is give player NBT command sign 'click me' which runs and increase score and also changes own text to clue
               Pool(Roll(1,1),[Item("minecraft:spawn_egg",[SetNbt("""{EntityTag:{id:LavaSlime,Size:0,DeathLootTable:\"minecraft:entities/magma_cube\"},display:{Name:\"Kill me with a sword to learn a secret!\"}}""")]),1,0,[]])
               // TODO easy to 'miss' the egg in the chest... could make only thing, have give actual loot chest as well? hmm
             ]


let LOOT_FROM_DEFAULT_CHESTS =
    [|
        "minecraft:chests/simple_dungeon", sampleTier2Chest
        "minecraft:chests/abandoned_mineshaft", sampleTier2Chest
        // TODO all the others
        // hack to get mine there
        sprintf "%s:chests/tier1" LOOT_NS_PREFIX, sampleTier1Chest 
        sprintf "%s:chests/tier3" LOOT_NS_PREFIX, sampleTier3Chest 
        sprintf "%s:chests/tier4" LOOT_NS_PREFIX, sampleTier4Chest 
        sprintf "%s:chests/tier5" LOOT_NS_PREFIX, sampleTier5Chest 
    |]
// TODO fix fishing


let writeLootTables(tables, worldSaveFolder) =
    for (name:string, table:LootTable) in tables do
        let pathBits = name.Split [|':';'/'|]
        let wslt = System.IO.Path.Combine [| yield worldSaveFolder; yield "data"; yield "loot_tables" |]
        let filename = System.IO.Path.Combine [| yield wslt; yield! pathBits |]
        let filename = filename + ".json"
        if System.IO.File.Exists(filename) then
            System.IO.File.Delete(filename)
        System.IO.Directory.CreateDirectory( System.IO.Path.GetDirectoryName(filename) ) |> ignore
        use stream = new System.IO.StreamWriter( System.IO.File.OpenWrite(filename) )
        table.Write(stream)

let writeAllLootTables(worldSaveFolder) =
    writeLootTables(LOOT_FROM_DEFAULT_MOBS, worldSaveFolder)
    writeLootTables(LOOT_FROM_DEFAULT_CHESTS, worldSaveFolder)
    let otherTables = [|
            for i = 1 to 4 do
                yield (LOOT_FORMAT"armor"i, LOOT_ARMOR.[i-1])
                yield (LOOT_FORMAT"tools"i, LOOT_TOOLS.[i-1])
                yield (LOOT_FORMAT"food"i,  LOOT_FOOD.[i-1])
        |]
    writeLootTables(otherTables, worldSaveFolder)



