module PiggyBag

open IO_Utilities

let NS = "lorgon111"

let makePiggyBag() =   // Note: multi-player is just too tough/tedious/expensive to implement 

    // Note: uses https://github.com/rx-modules/PlayerDB for storage
    // TODO temp scoreboard id tags to associate chest and owner perhaps

    let pack = new DataPackArchive("""C:\Users\Admin1\AppData\Roaming\.minecraft\saves\test2""", "piggy_bag", "Piggy Bag by Dr. Brian Lorgon111")

    pack.WriteAdvancement(NS, "inv_changed", """
    {
        "criteria": {
            "inv_changed": {
                "trigger": "minecraft:inventory_changed"
            }
        },
        "rewards": {
            "function": "lorgon111:inv_changed"
        }
    }""")

    pack.WriteFunction(NS, "inv_changed", 
        [|
        // keep tracking inv
        """advancement revoke @s only lorgon111:inv_changed"""
        // detect if Piggy summoning item in upper right of current player inv
        """data remove storage lorgon111:inv Temp"""
        """execute if data entity @s Inventory[{Slot:17b,tag:{display:{Name:'{"text":"Piggy Bag"}'}}}] run data modify storage lorgon111:inv Temp set value 1"""
        // load up per-player data
        """function rx.playerdb:api/get_self"""
        // if just moved to upper right, summon fresh one
        """execute unless data storage rx:io lorgon111.UpperRight run execute if data storage lorgon111:inv Temp run execute at @s run function lorgon111:upper_right"""
        // if just moved away from upper right, kill any
        """execute if data storage rx:io lorgon111.UpperRight run execute unless data storage lorgon111:inv Temp run function lorgon111:not_upper_right"""
        |])

    // TODO
    pack.WriteFunction(NS, "kill",
        [|
        // kill would loot the chest items, which is not desirable.
        // tp into the void kills with no items dropped nearby
        """tp @e[type=armor_stand,tag=piggybag] ~ -999 ~"""
        """tp @e[type=chest_minecart,tag=piggybag] ~ -999 ~"""
        |])

    pack.WriteFunction(NS, "master",
        [|
        """execute as @e[type=chest_minecart,tag=piggybag] at @s run function lorgon111:per_piggy"""
        |])

    pack.WriteFunction(NS, "per_piggy",
        [|
        // TODO multiplayer
        // if piggybag is not near the player (single player), kill them all to ensure don't duplicate 
        // (e.g. if tp to new chunks & unload/reload old entity with older chest contents)
        """execute unless entity @a[distance=..9] run function lorgon111:kill"""
        // if piggybag exists near the player (single player), copy chest contents to storage Inv
        """execute if entity @a[distance=..9] run function lorgon111:copy_to_storage"""
        |])

    pack.WriteFunction(NS, "copy_to_storage",
        [|
        // while the piggy is still @s, copy its data to temp storage
        """data modify storage lorgon111:inv Inv set from entity @s Items"""
        // TODO find owning player
        // copy temp storage to player storage
        """execute as @p run function lorgon111:copy_to_player_storage"""
        |])

    pack.WriteFunction(NS, "copy_to_player_storage",
        [|
        // load up per-player data
        """function rx.playerdb:api/get_self"""
        // copy storage
        """data modify storage rx:io lorgon111.Inv set from storage lorgon111:inv Inv"""
        |])

    pack.WriteFunction(NS, "not_upper_right",
        [|
        """data remove storage rx:io lorgon111.UpperRight"""
        """function lorgon111:kill"""
        |])

    // TODO
    pack.WriteFunction(NS, "upper_right",
        [|
        """data modify storage rx:io lorgon111.UpperRight set value 1"""
        """function lorgon111:kill"""
        // summon a new piggybag
        """execute positioned ~ ~-0.7 ~ anchored eyes run summon armor_stand ^ ^ ^1.3 {Invisible:1b,NoGravity:1b,Marker:1b,Invulnerable:1b,Tags:["piggybag"],Passengers:[{id:"chest_minecart",CustomName:'{"text":"Piggy Bag"}',Invulnerable:1b,Tags:["piggybag"]}]}"""
        """playsound minecraft:entity.pig.ambient neutral @p ~ ~ ~ 1"""
        // copy data from storage Inv to chest contents
        """data modify entity @e[type=chest_minecart,limit=1,sort=nearest,tag=piggybag] Items set from storage rx:io lorgon111.Inv"""
        |])

    // TODO other stuff from minecraft/recipes and tags/items/log? or in a separate pack?

    pack.WriteFunctionTagsFileWithValues("minecraft", "tick", ["""lorgon111:master"""])

    pack.SaveToDisk()
