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

let LOOT_NS_PREFIX = "BrianLoot"
let LOOT_FORMAT s n = sprintf "%s:%s%d" LOOT_NS_PREFIX s n
type LOOT_KIND = | ARMOR | TOOLS | FOOD | BOOKS //| TODO 
let P11 x = Pool(Roll(1,1),x)
let OneOfAtNPercent(entryData, n, conds) = 
    assert(n>=0 && n <=100)
    let weight = (entryData |> Seq.length)*(100-n)
    P11[yield (Empty, weight, 0, []); for ed in entryData do yield (ed, n, 0, conds)]

// TODO was noting that RNG is not always kind; may get lots of armor and never get leggings.  I could flatten it out if I did something like:
//        - instead of armor pieces, drop an 'armor token' item
//        - have a 20Hz cmd that adds a score/tag to the token
//        - if any score/tag items exist, call a routine that converts it to armor; that routine implements 'fairness'
//        - the 'token' swapping is basically how eventide trance did drops, so it can't be too awful for overhead

// TODO make a customization knob to buff/nerf these drops/chances, perhaps?
let LOOT_ARMOR =
    [|
        // tier 1
        Pools [Pool(Roll(1,1), [
                        //Item("minecraft:leather_helmet",     [EnchantWithLevels(1,15,false)]), 1, 0, []
                        //Item("minecraft:leather_chestplate", [EnchantWithLevels(1,15,false)]), 1, 0, []
                        //Item("minecraft:leather_leggings",   [EnchantWithLevels(1,15,false)]), 1, 0, []
                        //Item("minecraft:leather_boots",      [EnchantWithLevels(1,15,false)]), 1, 0, []
                        Item("minecraft:golden_helmet",     []), 1, 0, []
                        Item("minecraft:golden_chestplate", []), 1, 0, []
                        Item("minecraft:golden_leggings",   []), 1, 0, []
                        Item("minecraft:golden_boots",      []), 1, 0, []
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

// TODO should I somehow stop mob-dropping useless tools late in the game? after first monument block, they might no longer be desirable?
//       - or maybe even player-configurable? ('stop dropping wood/stone tools' toggle, sets score, kills the drops?)
//       - or maybe the drops get better but rarer (e.g. iron stuff after first block?)
// NOTE: this also suggest a way to make the game 'revert to normal' after you win, could have all the 'normal' drops conditioned on a win score.  
// But would incur a perf penalty for eternity, just to save people from deleting some files, in the rare case people want to continue playing survival after won.

let LOOT_TOOLS =
    [|
        // tier 1
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:stone_sword",    [EnchantWithLevels( 1,15,false)]), 1, 0, []
                        Item("minecraft:wooden_pickaxe", [EnchantWithLevels(16,30,false)]), 1, 0, []
                        Item("minecraft:wooden_shovel",  [EnchantWithLevels(16,30,false)]), 1, 0, []
                        Item("minecraft:stone_axe",      [EnchantWithLevels( 1,15,false)]), 1, 0, []
                        Item("minecraft:bucket",         []), 1, 0, []
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

// TODO make a customization knob to buff/nerf these drops/chances, perhaps?
let LOOT_FOOD =
    [|
        // tier 1
        Pools [Pool(Roll(1,1), [Item("minecraft:cookie",   [SetCount(6,10)]), 1, 0, []])]
        // tier 2
        Pools [Pool(Roll(1,1), [Item("minecraft:apple",   [SetCount(4,6)]), 1, 0, []])]
        // tier 3
        Pools [Pool(Roll(1,1), [Item("minecraft:bread",   [SetCount(3,5)]), 1, 0, []])]
        // tier 4
        Pools [Pool(Roll(1,1), [Item("minecraft:cooked_beef",   [SetCount(3,8)]), 1, 0, []])]
        // tier 5
        Pools [Pool(Roll(1,1), [Item("minecraft:golden_apple",   [SetCount(3,6)]), 1, 0, []])]
    |]

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
let defaultMobDrop(itemName, countMin, countMax, lootingMin, lootingMax) = 
    Pool(Roll(1,1),[Item(sprintf "minecraft:%s" itemName,[SetCount(countMin,countMax);LootingEnchant(lootingMin,lootingMax)]),0,1, []])
let cobblePile = Item("minecraft:cobblestone", [SetCount(3,7)])
let ironPile = Item("minecraft:iron_ingot", [SetCount(1,3)])  // TODO gold pile?
let arrows = Item("minecraft:arrow", [SetCount(6,9)])
let luckyGapple = Item("minecraft:golden_apple", [SetCount(1,1);SetNbt("""{display:{Name:\"Lucky Golden Apple\",Lore:[\"Extremely rare drop\",\"See? Bats are useful :)\"]}}""")])
let MOB = [KilledByPlayer]
let LOOT_FROM_DEFAULT_MOBS =
    [|
        // PASSIVE
        "minecraft:entities/bat", Pools [OneOfAtNPercent([luckyGapple],2,MOB)]
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

        // HOSTILE
        "minecraft:entities/blaze", Pools [tierxyLootPct MOB 2 2 [ARMOR;TOOLS] 33; tierxyLootPct MOB 3 4 [FOOD] 33; OneOfAtNPercent([ironPile],10,MOB)]
        "minecraft:entities/cave_spider", Pools [tierxyLootPct MOB 2 2 [ARMOR;TOOLS] 16; tierxyLootPct MOB 3 3 [FOOD] 16; OneOfAtNPercent([ironPile],10,MOB)]
        "minecraft:entities/creeper", Pools [defaultMobDrop("gunpowder",0,1,0,1)
                                             tierxyLootPct MOB 1 2 [ARMOR;TOOLS] 8; tierxyLootPct MOB 1 2 [FOOD] 16; OneOfAtNPercent([cobblePile],10,MOB)]
//        "minecraft:entities/elder_guardian
        "minecraft:entities/enderman", Pools [defaultMobDrop("ender_pearl",0,1,0,1)
                                              tierxyLootPct MOB 2 3 [ARMOR;TOOLS] 16; tierxyLootPct MOB 2 3 [FOOD] 16; OneOfAtNPercent([arrows],16,MOB)  // extra loot
                                             ]
        "minecraft:entities/ghast", Pools [tierxyLootPct MOB 2 2 [ARMOR;TOOLS] 33; tierxyLootPct MOB 3 3 [FOOD] 33; OneOfAtNPercent([ironPile],10,MOB)]
//        "minecraft:entities/guardian
//        "minecraft:entities/magma_cube"
//        "minecraft:entities/shulker
        "minecraft:entities/silverfish", Pools [tierxyLootPct MOB 2 3 [FOOD] 20] // TODO what ought silverfish drop?
        "minecraft:entities/skeleton", Pools [defaultMobDrop("bone",0,2,0,1)
                                              tierxyLootPct MOB 1 2 [ARMOR;TOOLS] 10; tierxyLootPct MOB 2 2 [FOOD] 16; OneOfAtNPercent([arrows],16,MOB)]
//        "minecraft:entities/skeleton_horse
//        "minecraft:entities/slime
        "minecraft:entities/spider", Pools [tierxyLootPct MOB 1 2 [ARMOR;TOOLS] 8; tierxyLootPct MOB 1 2 [FOOD] 12; OneOfAtNPercent([cobblePile],8,MOB)]
        "minecraft:entities/witch", Pools [tierxyLootPct MOB 2 3 [ARMOR;TOOLS] 10; tierxyLootPct MOB 2 3 [FOOD] 16; OneOfAtNPercent([arrows],10,MOB)]
        "minecraft:entities/wither_skeleton", Pools [tierxyLootPct MOB 2 2 [ARMOR;TOOLS] 33; tierxyLootPct MOB 2 2 [FOOD] 33; OneOfAtNPercent([ironPile],10,MOB)]
        "minecraft:entities/zombie", Pools [tierxyLootPct MOB 1 2 [ARMOR;TOOLS] 6; tierxyLootPct MOB 1 2 [FOOD] 12; OneOfAtNPercent([cobblePile],8,MOB)]
//        "minecraft:entities/zombie_horse
        "minecraft:entities/zombie_pigman", Pools [Pool(Roll(1,1),[Item("minecraft:gold_ingot",[SetCount(0,1)]),1,0,[]]);tierxyLootPct MOB 2 3 [ARMOR;TOOLS] 10; tierxyLootPct MOB 3 3 [FOOD] 16; OneOfAtNPercent([arrows],10,MOB)]
    |]

let noFishingForYou =
        Pools[ Pool(Roll(1,1),[Item("minecraft:written_book",[SetNbt(Utilities.escape <| Utilities.writtenBookNBTString("Lorgon111","Nope!",[|
                                            """{"text":"Fishing is over-powered, so I have disabled it.\n\nYour map-maker,\nDr. Brian Lorgon111\n\nP.S. If you like the map, feel free to donate!"}""" // TODO donate link?
                                        |]))]),1,0, []]) ]

let LOOT_FROM_DEFAULT_CHESTS =
    [|
        "minecraft:gameplay/fishing", noFishingForYou
        // hack to get mine there
    |]

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
            let i = 5
            yield (LOOT_FORMAT"food"i,  LOOT_FOOD.[i-1])
        |]
    writeLootTables(otherTables, worldSaveFolder)

//////////////////////////////////////////////////////////

// non-loot-table chests

// ARMOR
let PROT(lvls) = 0, Seq.toArray lvls
let FF(lvls) = 2, Seq.toArray lvls
let BP(lvls) = 3, Seq.toArray lvls
let PROJ(lvls) = 4, Seq.toArray lvls
// fire, resp, aqua, thorns, depth unused
// MELEE
let SHARP(lvls) = 16, Seq.toArray lvls
let SMITE(lvls) = 17, Seq.toArray lvls
let BOA(lvls) = 18, Seq.toArray lvls
let KNOCK(lvls) = 19, Seq.toArray lvls
// looting, fire aspect unused
// UTILITY
let EFF(lvls) = 32, Seq.toArray lvls
let SILK(lvls) = 33, Seq.toArray lvls
let UNBR(lvls) = 34, Seq.toArray lvls
let FORT(lvls) = 35, Seq.toArray lvls
let FW(lvls) = 9, Seq.toArray lvls
let MEND(lvls) = 70, Seq.toArray lvls
// lure, luck unused
// BOW
let POW(lvls) = 48, Seq.toArray lvls
let PUNCH(lvls) = 49, Seq.toArray lvls
let INF(lvls) = 51, Seq.toArray lvls
// flame unused

open NBT_Manipulation

let makeItem(rng:System.Random,name,min,max,dmg) =
    [| String("id","minecraft:"+name); Byte("Count", byte(min+rng.Next(max-min+1))); Short("Damage",dmg); End |]
let INNER_CHEST_LORE = [|"Place this chest"; "and open it"; "for more loot"|]
let makeChestItemWithNBTItems(name,items) =
    [| Byte("Count", 1uy); Short("Damage",0s); String("id","minecraft:chest"); Compound("tag", [
                Compound("display",[String("Name",name);List("Lore",Strings INNER_CHEST_LORE);End]|>ResizeArray);
                Compound("BlockEntityTag", [List("Items",Compounds items);End] |> ResizeArray); End] |> ResizeArray); End |]
let makeBookWithIdLvl(id, lvl) =
    [| Byte("Count", 1uy); Short("Damage",0s); String("id","minecraft:enchanted_book"); Compound("tag", [|List("StoredEnchantments",Compounds[|[|Short("id",int16 id);Short("lvl",int16 lvl);End|]|]); End |] |> ResizeArray); End |]
let chooseNbooks(rng:System.Random,n,a) =
    let r = Algorithms.pickNnonindependently(rng, n, a)
    r |> Array.map (fun (id,lvls:_[]) -> makeBookWithIdLvl(id, lvls.[rng.Next(lvls.Length)]))
let addSlotTags(items) =
    let slot = ref 0uy
    [|
        for item in items do
            if !slot > 26uy then
                failwith "too much loot for chest"
            yield Seq.append [Byte("Slot",!slot)] item |> Seq.toArray 
            slot := !slot + 1uy
    |]

let NEWsampleTier2Chest(rng:System.Random) = // dungeons and mineshafts
    let F = CustomizationKnobs.LOOT_FUNCTION
    let tier2ArmorBooks = [PROT[1]; FF[1..4]; BP[1..3]; PROJ[1..3]]
    let tier2MeleeBooks = [SHARP[1]; SMITE[1..3]; BOA[1..3]; KNOCK[2]]
    let tier2UtilBooks = [EFF[1..3]; SILK[1]; FORT[1..3]; FW[2]]
    let tier2BowBooks = [POW[1..2]; PUNCH[1]]
    let tier2Items =
        [|
            yield! chooseNbooks(rng,F 2,tier2ArmorBooks)
            yield! chooseNbooks(rng,F 1,tier2MeleeBooks)
            yield! chooseNbooks(rng,F 2,tier2UtilBooks)
            yield! chooseNbooks(rng,F 1,tier2BowBooks)
            yield makeItem(rng,"anvil",F 3,F 5,2s)
            yield makeItem(rng,"arrow",F 10,F 20,0s)
            yield makeItem(rng,"apple",F 4,F 6,0s)
            yield makeItem(rng,"bread",F 2,F 2,0s)
            yield! Algorithms.pickNnonindependently(rng,F 1,[makeItem(rng,"iron_pickaxe",1,1,0s);makeItem(rng,"iron_sword",1,1,0s);makeItem(rng,"iron_axe",1,1,0s);makeItem(rng,"iron_ingot",2,9,0s)])
            yield makeItem(rng,"saddle",1,1,0s)
            yield makeItem(rng,"iron_horse_armor",1,1,0s)
            yield [| Byte("Count", 1uy); Short("Damage",0s); String("id","minecraft:written_book"); Compound("tag", Utilities.makeWrittenBookTags("Lorgon111","1. After gearing up",[|
                                            """{"text":"Once you've geared up and are wearing metal armor, you should venture out into the night looking for GREEN beacon light. A challenging path will lead to riches!"}"""
                                        |]) |> ResizeArray); End |]
        |]
    addSlotTags tier2Items 

let NEWsampleTier3Chest(rng:System.Random) = // green beacon
    let F = CustomizationKnobs.LOOT_FUNCTION
    let tier3ArmorBooks = [PROT[1..3]; FF[1..4]; BP[1..3]; PROJ[1..3]]
    let tier3MeleeBooks = [SHARP[1..3]; SMITE[2..4]; BOA[2..4]; KNOCK[2]]
    let tier3UtilBooks = [EFF[3..5]; UNBR[1..3]]
    let tier3BowBooks = [POW[2..4]; PUNCH[1..2]; INF[1]]
    let tier3Items =
        [|
            yield makeBookWithIdLvl(0,4)   // prot 4 book
            yield makeItem(rng,"anvil",F 3,F 5,2s)
            yield! chooseNbooks(rng,F 3,tier3ArmorBooks)
            yield! chooseNbooks(rng,F 3,tier3MeleeBooks)
            yield! chooseNbooks(rng,F 2,tier3UtilBooks)
            yield! chooseNbooks(rng,F 2,tier3BowBooks)
            yield makeChestItemWithNBTItems("Dungeon Loot",NEWsampleTier2Chest(rng))
            yield makeChestItemWithNBTItems("Dungeon Loot",NEWsampleTier2Chest(rng))
            yield makeItem(rng,"experience_bottle",64,64,0s)
            yield makeItem(rng,"diamond_pickaxe",1,1,0s)
            yield makeItem(rng,"diamond_sword",1,1,0s)
            yield makeItem(rng,"iron_ingot",F 20,F 30,0s)
            yield makeItem(rng,"gold_ingot",F 20,F 30,0s)
            yield makeItem(rng,"cooked_beef",F 10,F 20,0s)
            yield [| Byte("Count", 1uy); Short("Damage",0s); String("id","minecraft:written_book"); Compound("tag", Utilities.makeWrittenBookTags("Lorgon111","2. After green beacon cave",[|
                                            """{"text":"If you feel protected enough, look for a RED beacon and try attacking a surface area filled with cobwebs... terrific rewards await you!"}"""
                                        |]) |> ResizeArray); End |]
        |]
    addSlotTags tier3Items 

let NEWsampleTier4Chest(rng:System.Random) = // flat dungeon
    let F = CustomizationKnobs.LOOT_FUNCTION
    let tier4ArmorBooks = [PROT[3..4]; BP[3..4]; PROJ[3..4]]
    let tier4MeleeBooks = [SHARP[4..5]; SMITE[5]; BOA[5]]
    let tier4UtilBooks = [EFF[5]; UNBR[3]; MEND[1]]
    let tier4BowBooks = [POW[4..5]; PUNCH[2]; INF[1]]
    let tier4Items =
        [|
            yield! chooseNbooks(rng,F 2,tier4ArmorBooks)
            yield! chooseNbooks(rng,F 2,tier4MeleeBooks)
            yield! chooseNbooks(rng,F 3,tier4UtilBooks)
            yield! chooseNbooks(rng,F 2,tier4BowBooks)
            yield makeItem(rng,"anvil",F 3,F 5,2s)
            yield makeChestItemWithNBTItems("Green Beacon Cave Loot",NEWsampleTier3Chest(rng))
            yield makeItem(rng,"diamond",F 20,F 30,0s)
            yield makeItem(rng,"golden_apple",F 9,F 14,0s)
            yield [| Byte("Count", 1uy); Short("Damage",0s); String("id","minecraft:written_book"); Compound("tag", Utilities.makeWrittenBookTags("Lorgon111","3. After red beacon webs",[|
                                            """{"text":"Once strong enough, attack dangerous-looking mountain peaks with glassed loot boxes to get a map to the best treasure!"}"""
                                        |]) |> ResizeArray); End |]
        |]
    addSlotTags tier4Items 

let NEWsampleTier5Chest(rng:System.Random) = // mountain peak
    let F = CustomizationKnobs.LOOT_FUNCTION
    let tier5Items =
        [|
            yield makeChestItemWithNBTItems("Red Beacon Web Loot",NEWsampleTier4Chest(rng))
            yield makeChestItemWithNBTItems("Red Beacon Web Loot",NEWsampleTier4Chest(rng))
            for _i = 1 to F 2 do
                yield makeItem(rng,"experience_bottle",64,64,0s)
        |]
    addSlotTags tier5Items 

let NEWaestheticTier1Chest(rng:System.Random) =
    let F = CustomizationKnobs.LOOT_FUNCTION
    let items =
        [|  // blocks
            yield! Algorithms.pickNnonindependently(rng,3,[
                makeItem(rng,"stone",64,64,1s) // granite
                makeItem(rng,"stone",64,64,3s) // diorite
                makeItem(rng,"brick_block",64,64,0s)
                makeItem(rng,"hardened_clay",64,64,0s)
                makeItem(rng,"netherrack",64,64,0s)
                ])
            // fun
            yield makeItem(rng,"name_tag",3,10,0s)
            // utility blocks
            yield! Algorithms.pickNnonindependently(rng,3,[
                makeItem(rng,"log",64,64,0s) // oak
                makeItem(rng,"log",64,64,1s) // spruce
                makeItem(rng,"log",64,64,2s) // birch
                makeItem(rng,"log",64,64,3s) // jungle
                makeItem(rng,"log2",64,64,0s) // acacia
                makeItem(rng,"log2",64,64,1s) // dark oak
                ])
            // tradeable
            yield makeItem(rng,"emerald",1,F 1,0s)
            // useful (dungeon-chest book list)
            yield! chooseNbooks(rng,F 2,[PROT[1]; FF[1..4]; BP[1..3]; PROJ[1..3]; SHARP[1]; SMITE[1..3]; BOA[1..3]; KNOCK[2]; EFF[1..3]; SILK[1]; FORT[1..3]; FW[2]; POW[1..2]; PUNCH[1]])
            yield makeItem(rng,"anvil",F 2,F 2,2s)
        |]
    addSlotTags items 

let NEWaestheticTier2Chest(rng:System.Random) =
    let F = CustomizationKnobs.LOOT_FUNCTION
    let items =
        [|  // blocks
            yield! Algorithms.pickNnonindependently(rng,4,[
                makeItem(rng,"bookshelf",64,64,0s)
                makeItem(rng,"glass",64,64,0s)
                makeItem(rng,"glowstone",64,64,0s)
                ])
            // fun
            yield makeItem(rng,"jukebox",1,1,0s)
            yield! Algorithms.pickNnonindependently(rng,4,[
                    makeItem(rng,"record_13",1,1,0s)
                    makeItem(rng,"record_cat",1,1,0s)
                    makeItem(rng,"record_blocks",1,1,0s)
                    makeItem(rng,"record_chirp",1,1,0s)
                    makeItem(rng,"record_far",1,1,0s)
                    makeItem(rng,"record_mall",1,1,0s)
                    makeItem(rng,"record_mellohi",1,1,0s)
                    makeItem(rng,"record_stal",1,1,0s)
                    makeItem(rng,"record_strad",1,1,0s)
                    makeItem(rng,"record_ward",1,1,0s)
                    makeItem(rng,"record_11",1,1,0s)
                    makeItem(rng,"record_wait",1,1,0s)
                    ])
            // rail & redstone
            yield makeItem(rng,"rail",64,64,0s)
            yield makeItem(rng,"rail",64,64,0s)
            yield makeItem(rng,"golden_rail",64,64,0s)
            yield makeItem(rng,"redstone_block",64,64,0s)
            yield! Algorithms.pickNnonindependently(rng,2,[
                    makeItem(rng,"comparator",64,64,0s)
                    makeItem(rng,"piston",64,64,0s)
                    makeItem(rng,"slime_block",64,64,0s)
                    ])
            // tradeable
            yield makeItem(rng,"emerald",1,F 1,0s)
            // useful (dungeon-chest book list)
            yield! chooseNbooks(rng,F 3,[PROT[1]; FF[1..4]; BP[1..3]; PROJ[1..3]; SHARP[1]; SMITE[1..3]; BOA[1..3]; KNOCK[2]; EFF[1..3]; SILK[1]; FORT[1..3]; FW[2]; POW[1..2]; PUNCH[1]])
            yield makeItem(rng,"anvil",F 2,F 2,2s)
        |]
    addSlotTags items 

// TODO 16 dyes? (would also be lapis)
let NEWaestheticTier3Chest(rng:System.Random) =
    let items =
        [|  // blocks
            yield! Algorithms.pickNnonindependently(rng,3,[
                makeItem(rng,"quartz_block",64,64,0s)
                makeItem(rng,"prismarine",64,64,0s)
                makeItem(rng,"sea_lantern",64,64,0s)
                makeItem(rng,"hay_block",64,64,0s)
                ])
            // other chests
            yield makeChestItemWithNBTItems("Basic Blocks",NEWaestheticTier1Chest(rng))
            yield makeChestItemWithNBTItems("Nicer blocks and fun",NEWaestheticTier2Chest(rng))
        |]
    addSlotTags items 
