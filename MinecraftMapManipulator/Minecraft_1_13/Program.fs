
(*
: I notice that i I /give @p minecraft:snowball{Foo:1} 1, and then throw it, the thrown snowball /data get entity @e[type=snowball,limit=1,sort=nearest] loses the tag:{Foo:1} metadata... 
is there any easy way to gift a player 'magic snowballs' that are distinctly detectable from normal ones?
[10:13 AM] SirBenet: Don't think any data from item gets copied over to snowball unfortunately
👆1
[10:13 AM] SirBenet: Need to do something awkward like tag the nearest snowball when players used stat for snowball increases(edited)
[10:15 AM] Maxaxik: It doesn't get copied because there's no NBT on the snowball to hold that information
[10:29 AM] Lorgon111: Hm, and if I want to limit access to the 'magical' snowballs but still allow usual normal snowballs... 
I guess I would have to do something crazy like track how many magic snowballs the player has in their inventory, and look again after a snowball is thrown, and if the count went down by 1, 
then assume it's a magical one that just got thrown and tag the snowball entity nearest the player to get 'magic' processing?  Any other clever strategies?  
I guess maybe also, rather than tracking 'inventory count', I could track 'SelectedItem', and if magic snowball was SelectedItem in the tick (before?) stat detected a throw, 
then tag it? (and possibly also track offhand, since can throw from there)  Other thoughts?
SliceThePi: @Lorgon111 What I do for "magic" snowballs is have a tag for when the player is holding an item. 
I set it at the end of the tick so that even if the player runs out of snowballs, they'll still have the tag after throwing.
[10:39 AM] SliceThePi: So essentially what you said.
*)

//       /give @p minecraft:snowball{Foo:1} 1
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
    throwable_light()
    //PerformanceMicroBenchmarks.main()
    ignore argv
    0
