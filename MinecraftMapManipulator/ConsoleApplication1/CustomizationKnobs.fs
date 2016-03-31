module CustomizationKnobs

open NBT_Manipulation

///////////////////////////////////////////////////
// Try to put all the twistable knobs here, and have the rest of the code below be fixed.

let UHC_MODE = false
let SINGLEPLAYER = false


// TODO kind/freq of armor/weapon/food drops can affect difficulty
// TODO kind/cost of villager trades can affect difficulty or offer crutches (e.g. resistance pot, buy gapples, ...)
// TODO freq of random loot chests? (may interact with trades by having emeralds, may contain gapples or occasional OP weapon/armor? ...)
// TODO (eventually maybe) number of monumnet blocks / major dungeon types?
// TODO options to armor mobs in the spawners

// TODO initial terrain biome/etc stuff (is a kind of aesthetic customization)

///////////////////////////////////////////////////

let UHC_MULT_A = if UHC_MODE then 0.5 else 1.0   // most of map is much harder, make easier
let UHC_MULT_B = if UHC_MODE then 0.8 else 1.0   // end of map with great armor doesn't need as much buffer

let LOOT_FUNCTION(n) =
    if SINGLEPLAYER then
        n
    else
        match n with
        | 0 -> 0
        | 1 -> 2
        | 2 -> 3
        | 3 -> 5
        | _ -> int (float n * 1.5)

///////////////////////////////////////////////////

type MobSpawnerInfo() =
    member val RequiredPlayerRange =  16s with get, set
    member val SpawnCount          =   4s with get, set
    member val SpawnRange          =   4s with get, set
    member val MaxNearbyEntities   =   6s with get, set
    member val Delay               =  -1s with get, set
    member val MinSpawnDelay       = 200s with get, set
    member val MaxSpawnDelay       = 800s with get, set
    member val x = 0 with get, set 
    member val y = 0 with get, set 
    member val z = 0 with get, set 
    member val BasicMob = "Zombie" with get, set  // TODO multiple choice SpawnPotentials
    member val ExtraNbt = [] with get, set // Ex: skel jockey  Passengers:[{id:Skeleton,HandItems:[{id:bow,Count:1},{}]}]  ->    [ List("Passengers",Compounds[| [|String("id","Skeleton"); List("HandItems",Compounds[| [|String("id","bow");Int("Count",1);End|]; [| End |] |]); End|] |] )] )
    member this.AsNbtTileEntity() =
        [|
            Int("x", this.x)
            Int("y", this.y)
            Int("z", this.z)
            String("id","MobSpawner")
            Short("RequiredPlayerRange",this.RequiredPlayerRange)
            Short("SpawnCount",this.SpawnCount)
            Short("SpawnRange",this.SpawnRange)
            Short("MaxNearbyEntities",this.MaxNearbyEntities)
            Short("Delay",this.Delay)
            Short("MinSpawnDelay",this.MinSpawnDelay)
            Short("MaxSpawnDelay",this.MaxSpawnDelay)
            Compound("SpawnData",[|yield String("id",this.BasicMob);yield! this.ExtraNbt;yield End|] |> ResizeArray)
            List("SpawnPotentials",Compounds[|
                                                [|
                                                Compound("Entity",[|yield String("id",this.BasicMob); yield! this.ExtraNbt; yield End|] |> ResizeArray)
                                                Int("Weight",1)
                                                End
                                                |]
                                            |])
            End
        |]

////////////////////////////////////////////

type SpawnerData(distributionInfo, densityMultiplier) =
    let distribution = distributionInfo |> Array.collect (fun (n,k) -> Array.replicate n k)
    let mutable delayF = fun (_ms : MobSpawnerInfo, _rng : System.Random) -> ()
    let mutable spiderJockeyPercentage = 0.0
    member this.DensityMultiplier = densityMultiplier
    member this.DelayF with get() = delayF and set(f) = delayF <- f
    member this.SpiderJockeyPercentage with get() = spiderJockeyPercentage and set(x) = spiderJockeyPercentage <- x // 0.0 to 1.0
    member this.NextSpawnerAt(x,y,z,rng:System.Random) = 
        let ms = MobSpawnerInfo(x=x, y=y, z=z, BasicMob=distribution.[rng.Next(distribution.Length)])
        delayF(ms, rng)
        if ms.BasicMob = "Spider" then
            if rng.NextDouble() < spiderJockeyPercentage then
                ms.ExtraNbt <- [ List("Passengers",Compounds[| [|String("id","Skeleton"); List("HandItems",Compounds[| [|String("id","bow");Int("Count",1);End|]; [| End |] |]); End|] |] )]
        ms



// mega-dungeons
let GREEN_BEACON_CAVE_DUNGEON_SPAWNER_DATA = 
    SpawnerData([|(5,"Zombie"); (1,"Skeleton"); (1,"Creeper")|],                                UHC_MULT_A*1.0, DelayF = (fun (ms,rng) -> if rng.Next(10)=0 then ms.Delay <- 1s))
let PURPLE_BEACON_CAVE_DUNGEON_SPAWNER_DATA = 
    SpawnerData([|(6,"Zombie"); (1,"CaveSpider"); (1,"Witch"); (2,"Skeleton"); (2,"Creeper")|], UHC_MULT_B*1.5, DelayF = (fun (ms,rng) -> ms.MaxSpawnDelay <- 400s; ms.Delay <- int16(rng.Next(100))))
let MOUNTAIN_PEAK_DUNGEON_SPAWNER_DATA = 
    SpawnerData([|(3,"Zombie"); (3,"Spider"); (5,"CaveSpider"); (1,"Blaze"); (2,"Ghast")|],     UHC_MULT_B*1.0, DelayF = (fun (ms,_rng) -> ms.Delay <- 1s), SpiderJockeyPercentage = 1.0)
let FLAT_COBWEB_OUTER_SPAWNER_DATA = 
    SpawnerData([|(2,"Spider"); (1,"Witch"); (2,"CaveSpider")|],                                UHC_MULT_A*1.0, DelayF = (fun (ms,_rng) -> ms.Delay <- 1s), SpiderJockeyPercentage = 0.0)
let FLAT_COBWEB_INNER_SPAWNER_DATA = 
    SpawnerData([|(2,"Spider"); (1,"Witch"); (2,"CaveSpider")|],                                UHC_MULT_A*1.0, DelayF = (fun (ms,_rng) -> ms.Delay <- 1s), SpiderJockeyPercentage = 0.333)
let FLAT_SET_PIECE_SPAWNER_DATA = 
    SpawnerData([|(4,"Zombie"); (1,"Skeleton") |],                                              UHC_MULT_A*1.0, DelayF = (fun (ms,_rng) -> ms.Delay <- 1s))

// terrain ore substitutes
let GRANITE_COUNT = int(UHC_MULT_A*12.0)
let GRANITE_SPAWNER_DATA = 
    SpawnerData([|(5,"Zombie"); (5,"Skeleton"); (5,"Spider"); (2,"Creeper")|],     0.0, DelayF = (fun (ms,_rng) -> ms.MaxSpawnDelay <- 400s))
let REDSTONE_COUNT = int(UHC_MULT_A*4.0)
let REDTSONE_SPAWNER_DATA = 
    SpawnerData([|(1,"Zombie"); (1,"Skeleton"); (1,"Spider"); (1,"Blaze"); (1,"Creeper"); (1,"CaveSpider")|], 0.0, DelayF = (fun (ms,_rng) -> ms.MaxSpawnDelay <- 400s))

// vanilla dungeon additions
let VANILLA_DUNGEON_EXTRA(x,y,z,originalKind) = 
    MobSpawnerInfo(x=x, y=y, z=z, BasicMob=(if originalKind = "Spider" || originalKind = "CaveSpider" then "Skeleton" else "CaveSpider"), 
                                            Delay=1s, // primed
                                            MinSpawnDelay=200s, MaxSpawnDelay=400s) // 10-20s, rather than 10-40s



let BIOME_HELL_PERCENTAGE = 0.1
let BIOME_SKY_PERCENTAGE = 0.2


let DAYLIGHT_BEDROCK_BUFFER_RADIUS = 7
let MOUNTAIN_PEAK_DANGER_RADIUS = 20
let FLAT_COBWEB_DANGER_RADIUS = 40

let SPAWN_PROTECTION_DISTANCE_GREEN = 200
let SPAWN_PROTECTION_DISTANCE_FLAT = 350
let SPAWN_PROTECTION_DISTANCE_PEAK = 500
let SPAWN_PROTECTION_DISTANCE_PURPLE = 700
let STRUCTURE_SPACING = 250  // no two of same structure within this dist of each other (currently used by peaks and flats)
let DECORATION_SPACING = float(MOUNTAIN_PEAK_DANGER_RADIUS+FLAT_COBWEB_DANGER_RADIUS+2*DAYLIGHT_BEDROCK_BUFFER_RADIUS)*sqrt(2.0)|>int  // no two decos this close together (used by peaks and flats)
let RANDOM_LOOT_SPACING_FROM_PRIOR_DECORATION = 50 // no rand loot chests near dungeons, for example
let DAYLIGHT_RADIUS = 180

/////////////////////////////////
// regional difficulty settings
// (also influenced by easy/medium/hard and phase of moon)

let makeAreaHard(map:RegionFiles.MapFolder,x,z) =
    map.SetInhabitedTime(x,z,3600000L)
(*
// this is not useful, because (A) it's DayTime, not Time, that influences difficulty and (B) /time set will reset it.  So use /time set to influence instead
let makeMapTimeNhours(levelDat, n) =
    let ticks = (int64 n) * 60L  *60L *20L
    let replaceLevelDatTime pl nbt =
        match pl, nbt with 
        | Compound("Data",_)::_, NBT.Long("Time",_old) -> NBT.Long("Time",ticks) // DayTime!
        | _ -> nbt
    Utilities.updateDat(levelDat, replaceLevelDatTime)
*)

/////////////////////////////////

// map size; probably would require other changes to change these
let MINIMUM = -1024
let LENGTH = 2048

