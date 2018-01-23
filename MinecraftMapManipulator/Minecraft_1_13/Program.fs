

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

// TODO consider rather than sea_lantern, could do lava, "flowing_lava 7" does not seem to catch flammable things on fire, gives light for 1s and drops/disappears, can overlap entities (lava laser?), may be fun
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
        "init",[|
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
//            "execute unless entity @p[scores={remain=1..}] as @p[scores={snowy=0..}] run say state"
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
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "TestWarpPoints")
    let pack = new Utilities.DataPackArchive(world,"ThrowableLightPack","snowballs place temp light sources")
    for name,code in functions do
        pack.WriteFunction("snow",name,code)
    pack.SaveToDisk()

(*
let area_highlight() =
    let functions = [|
        "init",[|
            "scoreboard objectives add X dummy"
            "scoreboard objectives add Y dummy"
            "scoreboard objectives add Z dummy"
            "scoreboard objectives add curX dummy"
            "scoreboard objectives add curY dummy"
            "scoreboard objectives add curZ dummy"
            |]
        "do",[|
            "scoreboard players set $ENTITY curX 0"
            "function we:loop_x"
            |]
        "loop_x",[|
            "scoreboard players set $ENTITY curY 0"
            "function we:loop_y"
            "scoreboard players add $ENTITY curX 1"
            "execute if score $ENTITY curX < $ENTITY X positioned ~1 ~ ~ run function we:loop_x"
            |]
        "loop_y",[|
            "scoreboard players set $ENTITY curZ 0"
            "function we:loop_z"
            "scoreboard players add $ENTITY curY 1"
            "execute if score $ENTITY curY < $ENTITY Y positioned ~ ~1 ~ run function we:loop_y"
            |]
        "loop_z",[|
            //"setblock ~ ~ ~ stone"
            //"particle minecraft:dust 0.8 0.8 0.8 0.5 ~ ~ ~ 0.2 0.2 0.2 1 3 normal"
            //"particle minecraft:block stone ~ ~ ~ 0.2 0.2 0.2 1 3 normal"
            "particle minecraft:end_rod ~ ~ ~ 0.1 0.1 0.1 0.01 1 normal"
            "scoreboard players add $ENTITY curZ 1"
            "execute if score $ENTITY curZ < $ENTITY Z positioned ~ ~ ~1 run function we:loop_z"
            |]
        |]
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "TestWE")
    let pack = new Utilities.DataPackArchive(world,"WE","we")
    for name,code in functions do
        pack.WriteFunction("we",name,code |> Array.map MC_Constants.compile)  // TODO real compile, ENTITY
    pack.SaveToDisk()
*)


(*
A single sumofsqares dispatchntable can make it so can fill arbitrary region size with dx dy dz using fill ^ ^ ^ ^ ^ ^N and a facing entity.

Also clone

Fun with particles like Shane's corners too, easy to make something cool

Yeah clone is fill at rest with ICBs of clone ^^^M ^^^M ^^^ maybe, almost

Preview using particle block, or armorstand with head block... Raytrace to dest to project in front where to place

Nudge with eg selected item and W/S

See also https://www.youtube.com/watch?v=SOOvommDpUA for ideas

*)


let find_slime_chunks() =
    let PACK_NAME = "SlimePack"
    let FOLDER = System.IO.Path.Combine(Utilities.MC_ROOT, """TestSlime""")
    let pack = new Utilities.DataPackArchive(FOLDER, PACK_NAME, "Slime finder")

    pack.WriteFunctionTagsFileWithValues("minecraft", "load", ["test:on_load"])
    pack.WriteFunctionTagsFileWithValues("minecraft", "tick", ["test:on_tick"])

    pack.WriteFunction("test", "on_load", [|
        "scoreboard objectives add Tick dummy"
        |])
    pack.WriteFunction("test", "on_tick", [|
        "scoreboard players add @p Tick 1"
        "scoreboard players set @p[scores={Tick=4}] Tick 0"
        "execute if entity @p[scores={Tick=0}] run difficulty peaceful"
        "execute if entity @p[scores={Tick=1}] run difficulty normal"
        //"execute if entity @p[scores={Tick=3}] at @e[type=slime] run setblock ~ 127 ~ emerald_block"
        //"execute if entity @p[scores={Tick=3}] at @e[type=zombie] run setblock ~ 127 ~ emerald_block"
        |])
    pack.SaveToDisk()


(*

seed: -2788760844838102142

minecraft:igloo/igloo_top      7x5x8
minecraft:igloo/igloo_middle   3x3x3
minecraft:igloo/igloo_bottom   7x6x9

datapack replaces furnace in top with a sign

let igloo_replacement() =
    let PACK_NAME = "IglooPack"
    let FOLDER = System.IO.Path.Combine(Utilities.MC_ROOT, """TestIgloo""")
    Utilities.writeDatapackMeta(FOLDER, PACK_NAME, "Replace furnace in igloo")
    // copy in the structure manually, have an example in TestIgloo world now
    // TODO bug https://bugs.mojang.com/browse/MC-124167 means disabling pack does not disable this structure override
*)


let shoulder_cam() =
    let PACK_NAME = "ShoulderCamPack"
    let FOLDER = System.IO.Path.Combine(Utilities.MC_ROOT, """TestCam""")
    let pack = new Utilities.DataPackArchive(FOLDER, PACK_NAME, "spectator watches player in F5-like mode")
    let functions = [|
        "init", [|
            "scoreboard objectives add Count dummy"
            |]
        "tick", [|
            // OK, below works decent.  Have ideas for rotation and raytracing to get around terrain, too
            "execute at @p[tag=subject] run teleport @e[type=pig,tag=camera] ~ ~2 ~"
            "execute as @e[type=pig,tag=camera] at @s run teleport @s ^ ^ ^-5"
            // spectate through it, don't do like below
            //"execute as @e[type=pig,tag=camera] at @s run teleport @p[tag=camera] ~ ~2 ~"  // +2 to avoid entity collision
            |]
        // TODO ideas for player to control it:
        // Camera sits at a location defined by XRot/YRot/R from the player
        // Player could control it via a single item in inventory, along lines of
        //  - if item in upper left slot, then scroll-wheeling (SelectedItem change) changes RotX by 10
        //  - if item in next slot, then scroll-wheeling (SelectedItem change) changes RotY by 5
        //  - if item in next slot, then scroll-wheeling (SelectedItem change) changes R by 0.2 (min 0.2, max 20?)
        //  - if item upper right, stationary mode, camera does not move
        //  - if item next slot, 'chase' mode, if camera in front 2/3 of player facing for a certain threshold of time, then it starts RotX+10 each tick until it gets behind (back third)
        //  - if item next slot, toggles camera visibility (is pig invisible or not)
        //  - any other slot, follow mode, just stays relative position
        |]
    pack.WriteFunctionTagsFileWithValues("minecraft", "load", ["sc:init"])
    pack.WriteFunctionTagsFileWithValues("minecraft", "tick", ["sc:tick"])
    for name, code in functions do
        pack.WriteFunction("sc", name, code)
    pack.SaveToDisk()

let test2compilers() =
    let c1 = Compiler.Compiler('z','x',"zx",1,100,1,false)
    let c2 = Compiler.Compiler('z','y',"zy",1,100,1,false)
    let FOLDER = System.IO.Path.Combine(Utilities.MC_ROOT, """TestCompiler""")
    let pack1 = new Utilities.DataPackArchive(FOLDER, "pack1", "zx blah")
    let pack2 = new Utilities.DataPackArchive(FOLDER, "pack2", "zy blah")
    for ns,name,code in [yield! c1.Compile("zx","foo",["say hi...";"$NTICKSLATER(100)";"say ...there"])] do
        pack1.WriteFunction(ns,name,code)
    for ns,name,code in [yield! c2.Compile("zy","foo",["say zyhi...";"$NTICKSLATER(20)";"say zy...there"])] do
        pack2.WriteFunction(ns,name,code)
    for ns,name,code in c1.GetCompilerLoadTick() do
        pack1.WriteFunction(ns,name,code)
    for ns,name,code in c2.GetCompilerLoadTick() do
        pack2.WriteFunction(ns,name,code)
    pack1.WriteFunctionTagsFileWithValues("minecraft","load",[c1.LoadFullName])
    pack2.WriteFunctionTagsFileWithValues("minecraft","load",[c2.LoadFullName])
    pack1.WriteFunctionTagsFileWithValues("minecraft","tick",[c1.TickFullName])
    pack2.WriteFunctionTagsFileWithValues("minecraft","tick",[c2.TickFullName])
    pack1.SaveToDisk()
    pack2.SaveToDisk()

let temple_locator() =
    let code = [|
        yield "scoreboard objectives add Dir dummy"
        yield "scoreboard objectives add Best dummy"
        yield "scoreboard objectives add TEMP dummy"
        yield "scoreboard players set @p Dir 1"
        yield "execute as @p at @s rotated ~ 0 positioned ^ ^ ^20 store result score @s Best run locate Temple"
        for rel,dir in ["^-14 ^ ^14",2
                        "^-20 ^ ^",3
                        "^-14 ^ ^-14",4
                        "^ ^ ^-20",5
                        "^14 ^ ^-14",6
                        "^20 ^ ^",7
                        "^14 ^ ^14",8
                       ] do
            yield sprintf "execute as @p at @s rotated ~ 0 positioned %s store result score @s TEMP run locate Temple" rel
            yield sprintf "execute if score @p TEMP < @p Best run scoreboard players set @p Dir %d" dir
            yield sprintf "execute if score @p TEMP < @p Best run scoreboard players operation @p Best = @p TEMP"
        yield """execute if entity @p[scores={Dir=1}] run title @p actionbar [{"color":"gold","score":{"name":"@p","objective":"Best"}},{"color":"gold","text":" \u2191"}]"""
        yield """execute if entity @p[scores={Dir=2}] run title @p actionbar [{"color":"gold","score":{"name":"@p","objective":"Best"}},{"color":"gold","text":" \u2197"}]"""
        yield """execute if entity @p[scores={Dir=3}] run title @p actionbar [{"color":"gold","score":{"name":"@p","objective":"Best"}},{"color":"gold","text":" \u2192"}]"""
        yield """execute if entity @p[scores={Dir=4}] run title @p actionbar [{"color":"gold","score":{"name":"@p","objective":"Best"}},{"color":"gold","text":" \u2198"}]"""
        yield """execute if entity @p[scores={Dir=5}] run title @p actionbar [{"color":"gold","score":{"name":"@p","objective":"Best"}},{"color":"gold","text":" \u2193"}]"""
        yield """execute if entity @p[scores={Dir=6}] run title @p actionbar [{"color":"gold","score":{"name":"@p","objective":"Best"}},{"color":"gold","text":" \u2199"}]"""
        yield """execute if entity @p[scores={Dir=7}] run title @p actionbar [{"color":"gold","score":{"name":"@p","objective":"Best"}},{"color":"gold","text":" \u2190"}]"""
        yield """execute if entity @p[scores={Dir=8}] run title @p actionbar [{"color":"gold","score":{"name":"@p","objective":"Best"}},{"color":"gold","text":" \u2196"}]"""
        |]
    let FOLDER = System.IO.Path.Combine(Utilities.MC_ROOT, """TestLocate""")
    let pack = Utilities.DataPackArchive(FOLDER,"locator","find structures")
    pack.WriteFunction("loc","tick",code)
    pack.WriteFunctionTagsFileWithValues("minecraft","tick",["loc:tick"])
    pack.SaveToDisk()

let local_v_relative() =
    let FOLDER = System.IO.Path.Combine(Utilities.MC_ROOT, """TestLocate""")
    let pack = Utilities.DataPackArchive(FOLDER,"explain","explain local v relative coords")
    let show(block,name,color) =
        if name = null then
            sprintf """summon minecraft:armor_stand ~ ~-0.9 ~ {Small:1b,Invisible:1b,NoGravity:1b,ArmorItems:[{},{},{},{id:"minecraft:%s",Count:1b}]}""" block
        else
            sprintf """summon minecraft:armor_stand ~ ~-0.9 ~ {Small:1b,Invisible:1b,NoGravity:1b,CustomNameVisible:1b,CustomName:"{\"color\":\"%s\",\"text\":\"%s\"}",ArmorItems:[{},{},{},{id:"minecraft:%s",Count:1b}]}""" color name block 
    let functions = [|
        "local", [|
            let block, color = "blue_stained_glass", "blue"
            for i = 1 to 4 do
                yield sprintf """execute as @p at @s positioned ^ ^ ^%d run %s""" i (show(block,null,null))
            yield sprintf """execute as @p at @s positioned ^ ^ ^%d run %s""" 5 (show(block,"^ ^ ^5",color))
            for i = 1 to 4 do
                yield sprintf """execute as @p at @s positioned ^ ^%d ^ run %s""" i (show(block,null,null))
            yield sprintf """execute as @p at @s positioned ^ ^%d ^ run %s""" 5 (show(block,"^ ^5 ^",color))
            for i = 1 to 4 do
                yield sprintf """execute as @p at @s positioned ^%d ^ ^ run %s""" i (show(block,null,null))
            yield sprintf """execute as @p at @s positioned ^%d ^ ^ run %s""" 5 (show(block,"^5 ^ ^",color))
            |]
        "relative", [|
            let block, color = "red_stained_glass", "red"
            for i = 1 to 4 do
                yield sprintf """execute as @p at @s positioned ~ ~ ~%d run %s""" i (show(block,null,null))
            yield sprintf """execute as @p at @s positioned ~ ~ ~%d run %s""" 5 (show(block,"~ ~ ~5",color))
            for i = 1 to 4 do
                yield sprintf """execute as @p at @s positioned ~ ~%d ~ run %s""" i (show(block,null,null))
            yield sprintf """execute as @p at @s positioned ~ ~%d ~ run %s""" 5 (show(block,"~ ~5 ~",color))
            for i = 1 to 4 do
                yield sprintf """execute as @p at @s positioned ~%d ~ ~ run %s""" i (show(block,null,null))
            yield sprintf """execute as @p at @s positioned ~%d ~ ~ run %s""" 5 (show(block,"~5 ~ ~",color))
            |]
        "killall", [|
            "kill @e[type=armor_stand]"
            |]
        |]
    for name, code in functions do
        pack.WriteFunction("show", name, code)
    pack.SaveToDisk()
    


[<EntryPoint>]
let main argv = 
    MinecraftBINGO.cardgen_compile()
    //MinecraftBINGOExtensions.Blind.main()
    //Raycast.main()
    //throwable_light()
    //PerformanceMicroBenchmarks.main()
    //QFE.main()
    //WarpPoints.wp_c_main()
    //EandT_S11.tc_main()
    //QuickStack.main()
    //area_highlight()
    //find_slime_chunks()
    //igloo_replacement()
    //Recipes.test_recipe()
    //shoulder_cam()
    //test_selection_execution_order()
    //Mandelbrot.main()
    //test2compilers()
    //temple_locator()
    //local_v_relative()
    ignore argv
    0


