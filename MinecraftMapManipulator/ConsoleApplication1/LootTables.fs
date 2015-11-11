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
type EntryDatum =
    | Item of string * Function list // name, functions
    | LootTable of string // name
    | Empty
type Entries = (EntryDatum * int * int)list // weight, quality
type Rolls =
    | Roll of int * int
type Pool =
    | Pool of Rolls * Entries // TODO * Condition[]
type LootTable =
    | Pools of Pool list
    member this.Write(w:System.IO.TextWriter) =
        let ICH s = s |> Seq.toArray |> (fun a -> Array.init a.Length (fun i -> a.[i], if i<>0 then "," else "")) // interspersed comma helper
        w.WriteLine("""{"pools":[""")
        for (Pool(Roll(ra,rb),entries)),c in ICH(match this with Pools x -> x) do
            w.WriteLine(sprintf """    %s{"rolls":{"min":%d,"max":%d}, "entries":[""" c ra rb)
            for (datum,weight,quality),c in ICH entries do
                match datum with
                | Item(name, fs) ->
                    w.Write(sprintf """        %s{"weight":%4d, "quality":%4d, "type":"item", "name":"%s" """ c weight quality name)
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
                    w.Write(sprintf """        %s{"weight":%4d, "quality":%4d, "type":"loot_table", "name":"%s"} """ c weight quality name)
                    w.WriteLine()
                | Empty ->
                    w.Write(sprintf """        %s{"weight":%4d, "quality":%4d, "type":"empty"} """ c weight quality)
                    w.WriteLine()
            w.WriteLine("""    ]}""")
        w.WriteLine("""]}""")

let simple_dungeon =
    Pools [
            Pool(Roll(1,3), [
                    Item("minecraft:saddle",[]), 20, 0
                    Item("minecraft:golden_apple",[]), 15, 0
                    Item("minecraft:golden_apple",[SetData 1]), 2, 0
                    Item("minecraft:record_13",[]), 15, 0
                    Item("minecraft:record_cat",[]), 15, 0
                    Item("minecraft:name_tag",[]), 20, 0
                    Item("minecraft:golden_horse_armor",[]), 10, 0
                    Item("minecraft:iron_horse_armor",[]), 15, 0
                    Item("minecraft:diamond_horse_armor",[]), 5, 0
                    Item("minecraft:book",[EnchantRandomly[]]), 5, 0
                ])
            Pool(Roll(1,4), [
                    Item("minecraft:iron_ingot",[SetCount(1,4)]), 10, 0
                    Item("minecraft:gold_ingot",[SetCount(1,4)]), 5, 0
                    Item("minecraft:bread",[]), 20, 0
                    Item("minecraft:wheat",[SetCount(1,4)]), 20, 0
                    Item("minecraft:bucket",[]), 10, 0
                    Item("minecraft:redstone",[SetCount(1,4)]), 15, 0
                    Item("minecraft:coal",[SetCount(1,4)]), 15, 0
                    Item("minecraft:melon_seeds",[SetCount(2,4)]), 10, 0
                    Item("minecraft:pumpkin_seeds",[SetCount(2,4)]), 10, 0
                    Item("minecraft:beetroot_seeds",[SetCount(2,4)]), 10, 0
                ])
            Pool(Roll(3,3), [
                    Item("minecraft:bone",[SetCount(1,8)]), 10, 0
                    Item("minecraft:gunpowder",[SetCount(1,8)]), 10, 0
                    Item("minecraft:rotten_flesh",[SetCount(1,8)]), 10, 0
                    Item("minecraft:string",[SetCount(1,8)]), 10, 0
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
                        Item("minecraft:leather_helmet",     [EnchantWithLevels(1,15,false)]), 1, 0
                        Item("minecraft:leather_chestplate", [EnchantWithLevels(1,15,false)]), 1, 0
                        Item("minecraft:leather_leggings",   [EnchantWithLevels(1,15,false)]), 1, 0
                        Item("minecraft:leather_boots",      [EnchantWithLevels(1,15,false)]), 1, 0
                               ])]
        // tier 2
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:gold_helmet",     [EnchantWithLevels(1,15,false)]), 1, 0
                        Item("minecraft:gold_chestplate", [EnchantWithLevels(1,15,false)]), 1, 0
                        Item("minecraft:gold_leggings",   [EnchantWithLevels(1,15,false)]), 1, 0
                        Item("minecraft:gold_boots",      [EnchantWithLevels(1,15,false)]), 1, 0
                               ])]
        // tier 3
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:iron_helmet",     [EnchantWithLevels(1,15,false)]), 1, 0
                        Item("minecraft:iron_chestplate", [EnchantWithLevels(1,15,false)]), 1, 0
                        Item("minecraft:iron_leggings",   [EnchantWithLevels(1,15,false)]), 1, 0
                        Item("minecraft:iron_boots",      [EnchantWithLevels(1,15,false)]), 1, 0
                               ])]
        // tier 4
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:iron_helmet",     [EnchantWithLevels(16,30,false)]), 1, 0
                        Item("minecraft:iron_chestplate", [EnchantWithLevels(16,30,false)]), 1, 0
                        Item("minecraft:iron_leggings",   [EnchantWithLevels(16,30,false)]), 1, 0
                        Item("minecraft:iron_boots",      [EnchantWithLevels(16,30,false)]), 1, 0
                               ])]
    |]

let LOOT_TOOLS =
    [|
        // tier 1
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:wooden_sword",   [EnchantWithLevels(16,30,false)]), 1, 0
                        Item("minecraft:wooden_pickaxe", [EnchantWithLevels(16,30,false)]), 1, 0
                        Item("minecraft:wooden_shovel",  [EnchantWithLevels(16,30,false)]), 1, 0
                        Item("minecraft:wooden_axe",     [EnchantWithLevels(16,30,false)]), 1, 0
                               ])]
        // tier 2
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:stone_sword",   [EnchantWithLevels(16,30,false)]), 1, 0
                        Item("minecraft:stone_pickaxe", [EnchantWithLevels(16,30,false)]), 1, 0
                        Item("minecraft:stone_shovel",  [EnchantWithLevels(16,30,false)]), 1, 0
                        Item("minecraft:stone_axe",     [EnchantWithLevels(16,30,false)]), 1, 0
                               ])]
        // tier 3
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:iron_sword",   []), 1, 0
                        Item("minecraft:iron_pickaxe", []), 1, 0
                        Item("minecraft:iron_shovel",  []), 1, 0
                        Item("minecraft:iron_axe",     []), 1, 0
                        Item("minecraft:iron_sword",   [EnchantWithLevels(1,15,false)]), 1, 0
                        Item("minecraft:iron_pickaxe", [EnchantWithLevels(1,15,false)]), 1, 0
                        Item("minecraft:iron_shovel",  [EnchantWithLevels(1,15,false)]), 1, 0
                        Item("minecraft:iron_axe",     [EnchantWithLevels(1,15,false)]), 1, 0
                               ])]
        // tier 4
        Pools [Pool(Roll(1,1), [
                        Item("minecraft:iron_sword",   [EnchantWithLevels(16,30,false)]), 1, 0
                        Item("minecraft:iron_pickaxe", [EnchantWithLevels(16,30,false)]), 1, 0
                        Item("minecraft:iron_shovel",  [EnchantWithLevels(16,30,false)]), 1, 0
                        Item("minecraft:iron_axe",     [EnchantWithLevels(16,30,false)]), 1, 0
                               ])]
    |]

let LOOT_FOOD =
    [|
        // tier 1
        Pools [Pool(Roll(1,1), [Item("minecraft:cookie",   [SetCount(3,8)]), 1, 0])]
        // tier 2
        Pools [Pool(Roll(1,1), [Item("minecraft:bread",   [SetCount(3,8)]), 1, 0])]
        // tier 3
        Pools [Pool(Roll(1,1), [Item("minecraft:cooked_beef",   [SetCount(3,8)]), 1, 0])]
        // tier 4
        Pools [Pool(Roll(1,1), [Item("minecraft:golden_apple",   [SetCount(3,6)]), 1, 0])]
    |]

// TODO cobblestone

let LOOT_NS_PREFIX = "BrianLoot"
let LOOT_FORMAT s n = sprintf "%s:%s%d" LOOT_NS_PREFIX s n
type LOOT_KIND = | ARMOR | TOOLS | FOOD //| TODO 
let P11 x = Pool(Roll(1,1),x)
let OneOfAtNPercent(entryData, n) = 
    assert(n>=0 && n <=100)
    let weight = (entryData |> Seq.length)*(100-n)
    P11[yield (Empty, weight, 0); for ed in entryData do yield (ed, n, 0)]
let tierNLootData n kinds = 
    [ for k in kinds do match k with | ARMOR -> yield LootTable(LOOT_FORMAT"armor"n) | FOOD -> yield LootTable(LOOT_FORMAT"food"n) | TOOLS -> yield LootTable(LOOT_FORMAT"tools"n) ]
let tierxyLootPct x y kinds n = // tier x at n%, but instead tier y at n/10%.... so n=10 give 10%x, 1%y, and 89% nothing
    assert(n>=0 && n <=100)
    let weight = (kinds|>Seq.length) * (1000-10*n-n)
    P11[yield (Empty, weight, 0)
        for ed in tierNLootData x kinds do 
            yield (ed, 10*n, 0)
        for ed in tierNLootData y kinds do 
            yield (ed, n, 0)
       ]
let cobblePile = [Item("minecraft:cobblestone", [SetCount(4,9)])]
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

//        "minecraft:entities/blaze", Pools [P11 [LT ARMOR 1 10; LT ARMOR 2 1]; P11 [LT TOOLS 1 1]]
//        "minecraft:entities/cave_spider
//        "minecraft:entities/creeper
//        "minecraft:entities/elder_guardian
//        "minecraft:entities/enderman
//        "minecraft:entities/ghast
//        "minecraft:entities/guardian
//        "minecraft:entities/magma_cube
//        "minecraft:entities/shulker
//        "minecraft:entities/silverfish
        "minecraft:entities/skeleton", Pools [tierxyLootPct 1 2 [ARMOR;TOOLS] 16; tierxyLootPct 1 2 [FOOD] 16; OneOfAtNPercent(cobblePile,10)]
//        "minecraft:entities/skeleton_horse
//        "minecraft:entities/slime
        "minecraft:entities/spider", Pools [tierxyLootPct 1 2 [ARMOR;TOOLS] 10; tierxyLootPct 1 2 [FOOD] 10; OneOfAtNPercent(cobblePile,10)]
//        "minecraft:entities/witch
//        "minecraft:entities/wither_skeleton
        "minecraft:entities/zombie", Pools [tierxyLootPct 1 2 [ARMOR;TOOLS] 10; tierxyLootPct 1 2 [FOOD] 10; OneOfAtNPercent(cobblePile,10)]
//        "minecraft:entities/zombie_horse
//        "minecraft:entities/zombie_pigman
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
let writeAllLootTables() =
    writeLootTables(LOOT_FROM_DEFAULT_MOBS, """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\spawnchunks""")
    let otherTables = [|
            for i = 1 to 4 do
                yield (LOOT_FORMAT"armor"i, LOOT_ARMOR.[i-1])
                yield (LOOT_FORMAT"tools"i, LOOT_TOOLS.[i-1])
                yield (LOOT_FORMAT"food"i,  LOOT_FOOD.[i-1])
        |]
    writeLootTables(otherTables, """C:\Users\Admin1\AppData\Roaming\.minecraft\saves\spawnchunks""")



