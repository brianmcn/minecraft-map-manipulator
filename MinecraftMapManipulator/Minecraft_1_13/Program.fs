

//       /give @p minecraft:snowball{Foo:1} 1
// TODO make a recipe for it (e.g. 16 torches + 16 snowballs + 1 slimeball)
(*
I am imagining that in the near future we will be able to specify NBT in recipes.  
One thing I imagined doing was adding a recipe so that e.g. you could take a stack of snowballs, and add a slimeball to get a stack of 'magic snowballs' 
(which have some extra NBT and behave magically).  However looking at @￰￰Skylinerw 's recipe tutorial, I see

The optional count number specifies the number of items in the stack, defaulting to 1 when not specified. This cannot be used in a key or ingredient, only in a result.

so it sounds like you can't create the recipe I'm imagining (e.g. 16 snowballs + 1 slimeball = 16 magic snowballs), as inputs must be single items.
Assuming that's the case, is there any clever way to enable a single 'rare ingredient' to be combined with a large number of 'common consumables' to craft new 'rare consumables', 
using the recipe system (and not e.g. using 'floor crafting' or 'dispenser crafting')?


I guess in the worst case I can do like 7 torches, a snowball, and a slimeball as the 9 items

*)
let throwable_light() =
    let objectives = [|
        // position of most recent unprocessed airborne snowball (snowy = -1 to "null out" after processing)
        "snowx"
        "snowy"
        "snowz"
        // time remaining on the one light placed
        "remain"
        // 1 if holding the special magic snowball, 0 otherwise
        "holdingMagic"   
        // 1 if running loop, 0 if work done and no snowballs
        "loopRunning"
        |]
    let functions = [|
        "snowinit",[|
            for o in objectives do
                yield sprintf "scoreboard objectives add %s dummy" o
            yield "scoreboard players set @p snowy -1" // TODO store other than on player
            |]
        // always running
        "tick",[|  // TODO tested running in cmd block, also test as 'minecraft:tick'
            "execute at @p[scores={holdingMagic=1}] as @e[type=snowball,distance=..2] run scoreboard players set @p loopRunning 1"
            "execute at @p[scores={holdingMagic=1}] as @e[type=snowball,distance=..2] run tag @s add magicSnow"
            "execute if entity @p[scores={loopRunning=1}] run function snow:snowloop"
            "scoreboard players set @p holdingMagic 0"
            "scoreboard players set @p[nbt={SelectedItem:{tag:{Foo:1}}}] holdingMagic 1"  // TODO pick a real item tag
            |]
        // only running while there's either a magic snowball in the air, or the remain timer is still counting down
        "snowloop",[|
            "execute unless entity @p[scores={remain=1..}] as @e[type=snowball,tag=magicSnow] at @s if block ~ ~ ~ air run function snow:track"
            "execute unless entity @p[scores={remain=1..}] as @p[scores={snowy=0..}] run say state"
            "execute unless entity @p[scores={remain=1..}] as @p[scores={snowy=0..}] unless entity @e[type=snowball,tag=magicSnow] at @s run function snow:place"
            "execute at @p[scores={remain=1}] run function snow:remove"
            "scoreboard players remove @p[scores={remain=1..}] remain 1"  // run timer
            "execute unless entity @e[type=snowball,tag=magicSnow] unless entity @p[scores={remain=1..}] run scoreboard players set @p loopRunning 0" 
            |]
        "track",[|
            // TODO don't store on @p
            "execute store result score @p snowx run data get entity @s Pos[0] 1.0"
            "execute store result score @p snowy run data get entity @s Pos[1] 1.0"
            "execute store result score @p snowz run data get entity @s Pos[2] 1.0"
            "execute if entity @e[type=!snowball,distance=..2] run scoreboard players set @p snowy -1"  // if too close to an entity, null it out, so don't suffocate anything
            |]
        "place",[|
            """summon area_effect_cloud ~ ~ ~ {Duration:1,Tags:["snowaec"]}"""
            "execute store result entity @e[tag=snowaec,limit=1] Pos[0] double 1.0 run scoreboard players get @p snowx"
            "execute store result entity @e[tag=snowaec,limit=1] Pos[1] double 1.0 run scoreboard players get @p snowy"
            "execute store result entity @e[tag=snowaec,limit=1] Pos[2] double 1.0 run scoreboard players get @p snowz"
            "execute at @e[tag=snowaec] run setblock ~ ~ ~ sea_lantern"
            "scoreboard players set @p remain 40" // TODO store other than on player
            // no need to kill, Duration wil do it
            |]
        "remove",[|
            """summon area_effect_cloud ~ ~ ~ {Duration:1,Tags:["snowaec"]}"""
            "execute store result entity @e[tag=snowaec,limit=1] Pos[0] double 1.0 run scoreboard players get @p snowx"
            "execute store result entity @e[tag=snowaec,limit=1] Pos[1] double 1.0 run scoreboard players get @p snowy"
            "execute store result entity @e[tag=snowaec,limit=1] Pos[2] double 1.0 run scoreboard players get @p snowz"
            "execute at @e[tag=snowaec] run setblock ~ ~ ~ air"
            "scoreboard players set @p snowy -1" // TODO store other than on player
            // no need to kill, Duration wil do it
            |]
        |]
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "testflattening")
    Utilities.writeDatapackMeta(world,"ThrowableLightPack","snowballs place temp light sources")
    for name,code in functions do
        Utilities.writeFunctionToDisk(world,"ThrowableLightPack","snow",name,code)


[<EntryPoint>]
let main argv = 
    //MinecraftBINGO.cardgen_compile()
    //MinecraftBINGOExtensions.Blind.main()
    //Raycast.main()
    //throwable_light()
    //PerformanceMicroBenchmarks.main()
    //MC_Constants.main()
    //WarpPoints.main()
    //WarpPoints.wp_c_main()
    EandT_S11.tc_main()
    //QuickStack.main()
    ignore argv
    0
