

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

(Also, could change snowball entity to NoGravity and constantly teleport it if I wanted it to have 'laser' rather than 'gravity' behavior)

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
    let c1 = Compiler.Compiler('z','x',"zx",false)
    let c2 = Compiler.Compiler('z','y',"zy",false)
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

let test_conditional_reentrancy_in_compiled_code() =
    let c1 = Compiler.Compiler('z','w',"zw",false)
    let NS = "zw"
    let FOLDER = System.IO.Path.Combine(Utilities.MC_ROOT, """TestCompiler""")
    let pack1 = new Utilities.DataPackArchive(FOLDER, "packzw", "zw blah")
    let functions = [|
        "bar", [|
            "say calling before"
            "$NTICKSLATER(100)"
            "say called back after"
            |]
        "foo", [|
            "say will maybe call"
            sprintf "$CALL_ONLY_IF_NOT_REENTRANT(%s:bar)" NS
            |]
        |]
    for ns,name,code in [for n,c in functions do yield! c1.Compile(NS,n,c) ] do
        pack1.WriteFunction(ns,name,code)
    for ns,name,code in c1.GetCompilerLoadTick() do
        pack1.WriteFunction(ns,name,code)
    pack1.WriteFunctionTagsFileWithValues("minecraft","load",[c1.LoadFullName])
    pack1.WriteFunctionTagsFileWithValues("minecraft","tick",[c1.TickFullName])
    pack1.SaveToDisk()

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
        // TODO new /locate targets
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
    

let dump_context() =
    let FOLDER = System.IO.Path.Combine(Utilities.MC_ROOT, """work""")
    let pack = Utilities.DataPackArchive(FOLDER,"dump","dump context info to chat")
    let NS = "context"
    let functions =[|
        "init", [|
            "scoreboard objectives add X dummy"
            "scoreboard objectives add Y dummy"
            "scoreboard objectives add Z dummy"
            "scoreboard objectives add Dim dummy"
            "scoreboard objectives add XRot dummy"
            "scoreboard objectives add YRot dummy"
            "scoreboard objectives add ActionBarDump dummy"
            |]
        "dump", [|
            sprintf """execute if entity @e[tag=DEBUG] run function %s:dump_body""" NS
            sprintf """execute unless entity @e[tag=DEBUG] run tellraw @a ["DEBUG entity was not present, summoning a new one now so future invocations work"]"""
            sprintf """execute unless entity @e[tag=DEBUG] run summon area_effect_cloud ~ ~ ~ {Duration:9999999,Tags:[DEBUG]}"""
            |]
        "dump_body", [|
            sprintf "tp @e[tag=DEBUG] ~ ~ ~ ~ ~"
            sprintf """tellraw @a ["WHO: @s is '",{"selector":"@s"},"'"]"""
            sprintf "execute store result score FAKE X run data get entity @e[tag=DEBUG,limit=1] Pos[0]"
            sprintf "execute store result score FAKE Y run data get entity @e[tag=DEBUG,limit=1] Pos[1]"
            sprintf "execute store result score FAKE Z run data get entity @e[tag=DEBUG,limit=1] Pos[2]"
            sprintf "execute store result score FAKE Dim run data get entity @e[tag=DEBUG,limit=1] Dimension"
            sprintf """tellraw @a ["WHERE: (x,y,z,dim) is (",%s,", ",%s,", ",%s,", ",%s,")"]""" (Utilities.tellrawScoreSelector("FAKE","X")) (Utilities.tellrawScoreSelector("FAKE","Y")) (Utilities.tellrawScoreSelector("FAKE","Z")) (Utilities.tellrawScoreSelector("FAKE","Dim"))
            sprintf "execute store result score FAKE YRot run data get entity @e[tag=DEBUG,limit=1] Rotation[0]"
            sprintf "execute store result score FAKE XRot run data get entity @e[tag=DEBUG,limit=1] Rotation[1]"
            sprintf "kill @e[tag=TEMPAEC]"  // even with Duration:1, it won't die if e.g. it was tp'd to spawn chunks where no players are, because entities away from players don't get ticked
            sprintf "summon area_effect_cloud ^ ^ ^ {Duration:1,Tags:[TEMPAEC]}"
            sprintf """execute if entity @e[tag=TEMPAEC,distance=..0.0001] run tellraw @a ["HOW: (yrot,xrot,anchor) is (",%s,", ",%s,", feet)"]""" (Utilities.tellrawScoreSelector("FAKE","YRot")) (Utilities.tellrawScoreSelector("FAKE","XRot")) 
            sprintf """execute unless entity @e[tag=TEMPAEC,distance=..0.0001] run tellraw @a ["HOW: (yrot,xrot,anchor) is (",%s,", ",%s,", not feet)"]""" (Utilities.tellrawScoreSelector("FAKE","YRot")) (Utilities.tellrawScoreSelector("FAKE","XRot")) 
            |]
        "tick", [|
            sprintf "execute as @a[scores={ActionBarDump=1..}] at @s run function %s:display_actionbar" NS
            |]
        "display_actionbar", [|
            yield sprintf "execute store result score @s X run data get entity @s Pos[0]"
            yield sprintf "execute store result score @s Y run data get entity @s Pos[1]"
            yield sprintf "execute store result score @s Z run data get entity @s Pos[2]"
            yield sprintf "execute store result score @s Dim run data get entity @s Dimension"
            yield sprintf "execute store result score @s YRot run data get entity @s Rotation[0]"
            yield sprintf "execute store result score @s XRot run data get entity @s Rotation[1]"
            let TRS(obj) = Utilities.tellrawScoreSelector("@s",obj)
            for sel, dir in Utilities.CARDINALS do
                yield sprintf """execute if entity %s run title @s actionbar {"text":"","color":"red","extra":["XYZD: ",%s,", ",%s,", ",%s,", ",%s,"  %s  Rot(",%s,", ",%s,")"]}""" 
                                                        sel (TRS"X") (TRS"Y") (TRS"Z") (TRS"Dim") (dir.ToUpper()) (TRS"YRot") (TRS"XRot")
            |]
        |]
    for name, code in functions do
        pack.WriteFunction(NS, name, code)
    pack.WriteFunctionTagsFileWithValues("minecraft","load",[sprintf"%s:init"NS])
    pack.WriteFunctionTagsFileWithValues("minecraft","tick",[sprintf"%s:tick"NS])
    pack.SaveToDisk()

let fun_whip() =
    // attempt something like https://gfycat.com/EvergreenFoolhardyHeterodontosaurus
    let FOLDER = System.IO.Path.Combine(Utilities.MC_ROOT, """TestLocate""")
    let pack = Utilities.DataPackArchive(FOLDER,"fun","fun misc")
    let NS = "fun"
    let functions =[|
        "init", [|
            "scoreboard objectives add This dummy"
            "scoreboard objectives add Next dummy"
            "scoreboard objectives add A dummy"
            |]
        "summon", [|
            "kill @e[tag=AS]"
            "scoreboard players set Count A 0"
            "function fun:summon_body"
            |]
        "summon_body", [|
            "tag @e[tag=new] remove new"
            "summon armor_stand ~ ~ ~ {Tags:[AS,new],NoGravity:1b}"
            "scoreboard players operation @e[tag=new] This = Count A"
            "scoreboard players add Count A 1"
            "scoreboard players operation @e[tag=new] Next = Count A"
            "execute as @e[tag=new] at @s rotated ~10 ~ positioned ^ ^ ^1 if score Count A matches ..10 run function fun:summon_body"
            |]
        "turn", [|
            "scoreboard players set Count A 0"
            "execute as @e[tag=AS] if score @s This = Count A at @s run function fun:turn_body"
            |]
        "turn_body", [|
//            "teleport @s ~ ~ ~ ~ ~"  // TODO https://bugs.mojang.com/browse/MC-124686
            "teleport @s ~ ~ ~"
            "execute as @e[tag=AS] if score @s This > Count A at @s run teleport @s ~ ~ ~ ~10 ~"
            "scoreboard players add Count A 1"
//            "execute rotated ~-30 ~ positioned ^ ^ ^1 as @e[tag=AS] if score @s This = Count A run function fun:turn_body"
            "execute rotated as @s positioned ^ ^ ^1 as @e[tag=AS] if score @s This = Count A run function fun:turn_body"
            |]
        |]
    for name, code in functions do
        pack.WriteFunction(NS, name, code)
    pack.WriteFunctionTagsFileWithValues("minecraft","load",[sprintf"%s:init"NS])
    pack.SaveToDisk()


let mob_replacement() =
    // TODO maybe also biome tracker, keep player's most recent biome in scoreboard, use it to do stuff
    let FOLDER = System.IO.Path.Combine(Utilities.MC_ROOT, """New World""")
    let pack = Utilities.DataPackArchive(FOLDER,"mob","mob replacement")
    let NS = "mob"
    let functions =[|
        "tick",[|
            // regardless of whether run in command block or in tick, can still see the spider on the screen for a tick when it spawns
            "execute at @e[type=spider] run summon creeper"
            "execute as @e[type=spider] at @s run teleport @s ~ ~-250 ~"
            |]
        |]
    for name, code in functions do
        pack.WriteFunction(NS, name, code)
    pack.WriteFunctionTagsFileWithValues("minecraft","tick",[sprintf"%s:tick"NS])
    pack.SaveToDisk()

let track_general_biome(NS:string,pack:Utilities.DataPackArchive) =
    let sample_display_commands = ResizeArray()
    printfn "%d biomes" MC_Constants.BIOMES.Length 
    let mutable kind,count = 0,0
    for a in MC_Constants.BIOME_COLLECTIONS do
        printfn "%s" (String.concat "," a)
        count <- count + a.Count
        kind <- kind + 1
        // track via scoreboard in pack
        let general_biome_name = a.[0]
        let sb = System.Text.StringBuilder()
        sb.Append("""{"criteria":{""") |> ignore  // NOTE: criteria names must be unique across (advancements in a namespace?)
        for i = 0 to a.Count-1 do
            let comma = if i=a.Count-1 then "\r\n" else ",\r\n"
            sb.Append(sprintf """ "visit_%s": {"trigger": "minecraft:location","conditions": {"biome": "minecraft:%s"}}%s""" a.[i] a.[i] comma) |> ignore
        sb.Append("""},"requirements": [[""" + (String.concat ", " [for i = 0 to a.Count-1 do yield sprintf "\"visit_%s\"" a.[i]]) + sprintf """]],
            "rewards": { "function": "%s:on_%s_grant" } }""" NS general_biome_name) |> ignore
        pack.WriteAdvancement(NS,general_biome_name,sb.ToString())
        pack.WriteFunction(NS,sprintf "on_%s_grant" general_biome_name,[|
            sprintf "scoreboard players set @s BIOME %d" kind
            sprintf "advancement revoke @s only %s:%s" NS general_biome_name
            // TODO could call e.g. location_tick here, and move my announce code from tick to location_tick so it only runs once per second (per player)
            |])
        sample_display_commands.Add(sprintf """execute if score @s BIOME matches %d run tellraw @s ["You are now in a %s biome"]""" kind general_biome_name)
    printfn "%d   %d" kind count
    // sample tracking
    pack.WriteFunction(NS,"init",[|
        "scoreboard objectives add BIOME dummy"
        "scoreboard objectives add OLD_BIOME dummy"
        |])
    pack.WriteFunction(NS,"tick",[|
        sprintf "execute as @a unless score @s OLD_BIOME = @s BIOME run function %s:announce_biome" NS
        sprintf "execute as @a run scoreboard players operation @s OLD_BIOME = @s BIOME"
        |])
    pack.WriteFunction(NS,"announce_biome",sample_display_commands)
    pack.WriteFunctionTagsFileWithValues("minecraft","load",[sprintf"%s:init"NS])
    pack.WriteFunctionTagsFileWithValues("minecraft","tick",[sprintf"%s:tick"NS])

let show_biomes() =
    let FOLDER = System.IO.Path.Combine(Utilities.MC_ROOT, """New World""")
    let pack = Utilities.DataPackArchive(FOLDER,"biome","biome tracker")
    let NS = "biome"
    track_general_biome(NS,pack)
    pack.SaveToDisk()

let test_json() =
//    let text = System.IO.File.ReadAllText("""C:\Users\Admin1\Desktop\chests\abandoned_mineshaft.json""")
//    let text = System.IO.File.ReadAllText("""C:\Users\Admin1\Desktop\chests\desert_pyramid.json""")
//    let text = System.IO.File.ReadAllText("""C:\Users\Admin1\Desktop\chests\end_city_treasure.json""")
//    let text = System.IO.File.ReadAllText("""C:\Users\Admin1\Desktop\chests\igloo_chest.json""")
//    let text = System.IO.File.ReadAllText("""C:\Users\Admin1\Desktop\chests\jungle_temple.json""")
//    let text = System.IO.File.ReadAllText("""C:\Users\Admin1\Desktop\chests\nether_bridge.json""")
//    let text = System.IO.File.ReadAllText("""C:\Users\Admin1\Desktop\chests\simple_dungeon.json""")
//    let text = System.IO.File.ReadAllText("""C:\Users\Admin1\Desktop\chests\stronghold_corridor.json""")
//    let text = System.IO.File.ReadAllText("""C:\Users\Admin1\Desktop\chests\stronghold_crossing.json""")
//    let text = System.IO.File.ReadAllText("""C:\Users\Admin1\Desktop\chests\stronghold_library.json""")
//    let text = System.IO.File.ReadAllText("""C:\Users\Admin1\Desktop\chests\village_blacksmith.json""")
    let text = System.IO.File.ReadAllText("""C:\Users\Admin1\Desktop\chests\woodland_mansion.json""")
    let jv, _r = JsonUtils.JsonValue.Parse(text)
    printfn "%s" (jv.ToPrettyString(2,210))

let see_if_loot_tables_changed() =
    let version = "18w05a"
    let jar = """C:\Users\Admin1\AppData\Roaming\.minecraft\versions\"""+version+"""\"""+version+""".jar"""
    let zipFileStream = new System.IO.FileStream(jar, System.IO.FileMode.Open)
    let zipArchive = new System.IO.Compression.ZipArchive(zipFileStream, System.IO.Compression.ZipArchiveMode.Read)
    for entry in zipArchive.Entries do
        for chestName,prevContents in MC_Default_Loot.chest_loot_tables do
            if entry.FullName = """data/minecraft/loot_tables/"""+chestName then
                let sr = new System.IO.StreamReader(entry.Open())
                let allText = sr.ReadToEnd()
                let newJson,_ = JsonUtils.JsonValue.Parse(allText)
                let oldJson,_ = JsonUtils.JsonValue.Parse(prevContents)
                if newJson = oldJson then
                    printfn "unchanged: %s" chestName
                else
                    printfn "CHANGE DETECTED!!! : %s" chestName
                    printfn "old:"
                    printfn "%s" (oldJson.ToPrettyString(2,210))
                    printfn "new:"
                    printfn "%s" (newJson.ToPrettyString(2,210))
                    newJson.FailOnDiff(oldJson)


let hover() =
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "work")
    let pack = new Utilities.DataPackArchive(world,"hover_boots","hover boots")
    let compiler = new Compiler.Compiler('h','b',"hb",false)
    
    let hover_funcs = [|
        yield "init", [|
            "scoreboard objectives add jump minecraft.custom:minecraft.jump"
            "scoreboard objectives add inair dummy"
            "scoreboard objectives add cooldown dummy"
            "scoreboard objectives add temp dummy"
            "scoreboard players set TWO temp 2"
            "scoreboard objectives add state dummy"
            |]
        yield "tick", [|
            "scoreboard players set @a[nbt={OnGround:1b},scores={state=..-1}] state 0"
            "scoreboard players set @a[nbt={OnGround:1b},scores={state=1}] state -1"
            "scoreboard players set @a[nbt={OnGround:1b},scores={state=2}] state -2"
            "scoreboard players set @a[nbt={OnGround:0b},scores={state=0,jump=1}] state 1"
            "scoreboard players set @a[nbt={OnGround:0b},scores={state=-1}] state 1"
            "scoreboard players set @a[nbt={OnGround:0b},scores={state=-2}] state 2"
            "execute as @a[nbt={OnGround:0b},scores={state=0,jump=0}] run function hb:begin"
            "scoreboard players set @a jump 0"

(*
            """execute as @p run tellraw @a [{"score":{"name":"@s","objective":"jump"}},{"score":{"name":"@s","objective":"inair"}}]"""
            "execute as @a[scores={inair=0,jump=0,cooldown=..0},nbt={OnGround:0b}] run function hb:begin"
            "execute as @a[nbt={ActiveEffects:[{Id:25s,Amplifier:-1b}]}] at @s run particle cloud ~ ~-0.5 ~ 0.5 0 0.5 0.01 2 force"
            "scoreboard players set @a[nbt={OnGround:0b}] inair 1"
            "scoreboard players set @a[nbt={OnGround:1b}] inair 0"
            "scoreboard players set @a jump 0"
            "scoreboard players remove @a[nbt={OnGround:1b}] cooldown 1"
            "scoreboard players operation @p temp = @p cooldown"
            "scoreboard players operation @p temp %= TWO temp"
            "execute as @p[scores={cooldown=1..,temp=0}] run function hb:levitate"
            "execute as @p[scores={cooldown=1..,temp=1}] run function hb:slowfall"
*)
            |]
        yield "begin", [|
            "scoreboard players set @s state 2"
            "say effect give @s levitation 2 255"
            "scoreboard players set @s cooldown 140"
            //"$NTICKSLATER(1)"
            //"execute as @a[nbt={ActiveEffects:[{Id:25s,Amplifier:-1b}]}] at @s positioned ~ ~0.92 ~ align y run teleport @s ~ ~ ~"
            |]
(*
        yield "slowfall", [|
            "effect clear @s levitation"
            "effect give @s slow_falling 1 1"
            "scoreboard players remove @s cooldown 1"
            |]
        yield "levitate", [|
            "effect clear @s slow_falling"
            "effect give @s levitation 1 5"
            "scoreboard players remove @s cooldown 1"
            |]
*)
        |]
    
    for ns,name,code in [for name,code in hover_funcs do yield! compiler.Compile("hb",name,code)] do
        pack.WriteFunction(ns,name,code)
    for ns,name,code in compiler.GetCompilerLoadTick() do
        pack.WriteFunction(ns,name,code)
    pack.WriteFunctionTagsFileWithValues("minecraft","load",[compiler.LoadFullName;"hb:init"])
    pack.WriteFunctionTagsFileWithValues("minecraft","tick",[compiler.TickFullName;"hb:tick"])
    pack.SaveToDisk()

let hero_bow() =
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "work")
    let pack = new Utilities.DataPackArchive(world,"hero_bow","hero bow")
    let compiler = new Compiler.Compiler('h','a',"ha",false)
    
    let hero_bow_funcs = [|
        yield "init", [|
            "scoreboard objectives add temp dummy"
            """give @p bow{ench:[{lvl:8s,id:48s}],Unbreakable:1b,HeroBow:1b,display:{Name:"{\"text\":\"Hero\\u0027s Bow\"}"}}"""
            "scoreboard objectives add shoot minecraft.used:minecraft.bow"
            |]
        yield "tick", [|
            """execute as @a[scores={shoot=1},nbt={SelectedItem:{id:"minecraft:bow",tag:{display:{Name:"{\"text\":\"Hero\\u0027s Bow\"}"}}}}] at @s as @e[type=arrow,sort=nearest,limit=1] run tag @s add HeroArrow"""
            """execute as @e[type=arrow,tag=HeroArrow,nbt={inBlockState:{Name:"minecraft:chiseled_stone_bricks"}}] at @s run function ha:proc_arrow"""
            "scoreboard players add @e[type=armor_stand,tag=HeroArrowAS] temp 1"
            "execute as @e[type=armor_stand,tag=HeroArrowAS,scores={temp=1}] at @s run function ha:find_wool"
            "execute as @e[type=armor_stand,tag=HeroArrowAS,scores={temp=201}] at @s run function ha:finish_wool"
            "scoreboard players set @a shoot 0"
            |]
        yield "proc_arrow", [|
            "tag @s remove HeroArrow"
            "summon armor_stand ~ ~ ~ {Invisible:1b,Invulnerable:1b,Marker:1b,Tags:[HeroArrowAS]}"
            |]
        yield "find_wool", [|
            yield "say expensive wool find"
            for x = -5 to 5 do
                for y = -5 to 5 do
                    for z = -5 to 5 do
                        yield sprintf "execute positioned ~%d ~%d ~%d if block ~ ~ ~ minecraft:red_wool run summon armor_stand ~ ~ ~ {Invisible:1b,Invulnerable:1b,Marker:1b,Tags:[RedWoolAS]}" x y z
            yield "execute at @e[type=armor_stand,tag=RedWoolAS,distance=..9] run setblock ~ ~ ~ redstone_block"
            |]
        yield "finish_wool", [|
            "say finish wool"
            "execute at @e[type=armor_stand,tag=RedWoolAS,distance=..9] run setblock ~ ~ ~ red_wool"
            "kill @e[type=armor_stand,tag=RedWoolAS,distance=..9]"  // TODO problems if multiple chiseled nearby that both modify?
            "kill @s"
            |]
        |]
    
    for ns,name,code in [for name,code in hero_bow_funcs do yield! compiler.Compile("ha",name,code)] do
        pack.WriteFunction(ns,name,code)
    for ns,name,code in compiler.GetCompilerLoadTick() do
        pack.WriteFunction(ns,name,code)
    pack.WriteFunctionTagsFileWithValues("minecraft","load",[compiler.LoadFullName;"ha:init"])
    pack.WriteFunctionTagsFileWithValues("minecraft","tick",[compiler.TickFullName;"ha:tick"])
    pack.SaveToDisk()

let tnt_bomb() =
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "work")
    let pack = new Utilities.DataPackArchive(world,"tnt_bomb","tnt bomb")
    let compiler = new Compiler.Compiler('t','b',"tb",false)
    
    let hero_bow_funcs = [|
        yield "init", [|
            "scoreboard objectives add temp dummy"
            """give @p tnt{IsBomb:1b,display:{Name:"{\"text\":\"Bomb\"}"}} 64"""
            |]
        yield "tick", [|
            """execute at @a as @e[distance=..3,tag=!TNTBombItem,type=item,nbt={Item:{id:"minecraft:tnt",tag:{IsBomb:1b}}}] run tag @s add TNTBombItem"""
            "execute as @e[tag=TNTBombItem,nbt={OnGround:1b}] at @s run function tb:ignite"
            "execute as @e[type=tnt,tag=TNTBomb,nbt={Fuse:1s}] at @s run function tb:detonate"
            |]
        yield "ignite", [|
            "summon tnt ~ ~ ~ {Fuse:61s,Tags:[TNTBomb]}"
            "playsound minecraft:entity.tnt.primed block @a ~ ~ ~ 1 1"
            "kill @s"
            |]
        yield "detonate", [|
            // sound/visual
            "particle explosion ~ ~ ~ 0.3 0.3 0.3 0.2 2500 force"
            "playsound minecraft:entity.generic.explode block @a ~ ~ ~ 1 1"
            // damage mobs and players
            "execute as @e[distance=..5,type=!zombie,type=!skeleton,type=!zombie_pigman,type=!wither_skeleton,type=!stray,type=!zombie_villager,type=!husk,type=!drowned] run effect give @s instant_damage 1 0 true"
            "execute as @e[distance=..5,type=zombie] run effect give @s instant_health 1 0 true"
            "execute as @e[distance=..5,type=skeleton] run effect give @s instant_health 1 0 true"
            "execute as @e[distance=..5,type=zombie_pigman] run effect give @s instant_health 1 0 true"
            "execute as @e[distance=..5,type=wither_skeleton] run effect give @s instant_health 1 0 true"
            "execute as @e[distance=..5,type=stray] run effect give @s instant_health 1 0 true"
            "execute as @e[distance=..5,type=zombie_villager] run effect give @s instant_health 1 0 true"
            "execute as @e[distance=..5,type=husk] run effect give @s instant_health 1 0 true"
            "execute as @e[distance=..5,type=drowned] run effect give @s instant_health 1 0 true"
            // kill infested blocks
            "fill ~-3 ~-3 ~-3 ~3 ~3 ~3 air replace minecraft:infested_chiseled_stone_bricks"
            "fill ~-3 ~-3 ~-3 ~3 ~3 ~3 air replace minecraft:infested_cobblestone"
            "fill ~-3 ~-3 ~-3 ~3 ~3 ~3 air replace minecraft:infested_cracked_stone_bricks"
            "fill ~-3 ~-3 ~-3 ~3 ~3 ~3 air replace minecraft:infested_mossy_stone_bricks"
            "fill ~-3 ~-3 ~-3 ~3 ~3 ~3 air replace minecraft:infested_stone"
            "fill ~-3 ~-3 ~-3 ~3 ~3 ~3 air replace minecraft:infested_stone_bricks"
            // remove tnt           
            "kill @s"
            |]
        |]
    
    for ns,name,code in [for name,code in hero_bow_funcs do yield! compiler.Compile("tb",name,code)] do
        pack.WriteFunction(ns,name,code)
    for ns,name,code in compiler.GetCompilerLoadTick() do
        pack.WriteFunction(ns,name,code)
    pack.WriteFunctionTagsFileWithValues("minecraft","load",[compiler.LoadFullName;"tb:init"])
    pack.WriteFunctionTagsFileWithValues("minecraft","tick",[compiler.TickFullName;"tb:tick"])
    pack.SaveToDisk()

let magic_fire_and_compass() =
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "work")
    let pack = new Utilities.DataPackArchive(world,"magic_fire","magic fire")
    let compiler = new Compiler.Compiler('m','f',"mf",false)
    
    let magic_fire_funcs = [|
        yield "init", [|
            "scoreboard objectives add coas minecraft.used:minecraft.carrot_on_a_stick"
            "scoreboard objectives add temp dummy"
            """give @p carrot_on_a_stick{Unbreakable:1b,Damage:1,MagicFire:1b,display:{Name:"{\"text\":\"Magic Fire\"}"}} 1"""
            """give @p carrot_on_a_stick{Unbreakable:1b,Damage:2,CompassLost:1b,display:{Name:"{\"text\":\"Compass of the Lost\"}"}} 1"""
            |]
        yield "tick", [|
            "execute as @a[scores={coas=1},nbt={SelectedItem:{tag:{MagicFire:1b}}}] at @s run function mf:fire"
            "execute as @a[scores={coas=1},nbt={SelectedItem:{tag:{CompassLost:1b}}}] at @s run teleport @s 100 100 100"
            "scoreboard players set @a coas 0"
            |]
        yield "fire", [|
            "scoreboard players set @s temp 200"
            "execute anchored eyes positioned ^ ^ ^0.2 run function mf:step"
            "execute if entity @s[scores={temp=-1}] run say hit non-air"
            "execute if entity @s[scores={temp=0}] run say missed"
            |]
        yield "step", [|
            //"particle minecraft:flame ~ ~ ~ 0.01 0.01 0.01 0.001 1 force"
            // TODO saying if my hitbox intersects his, i think?
            "execute positioned ^-0.5 ^ ^ if entity @e[type=!area_effect_cloud,type=!player,dx=0.01,dy=0.01,dz=0.01] run function mf:hit"
            "execute unless block ~ ~ ~ #mf:collisionless run scoreboard players set @s temp -1"
            "scoreboard players remove @s[scores={temp=1..}] temp 1"
            "execute if entity @s[scores={temp=-1..0}] run particle minecraft:cloud ~ ~ ~ 0.1 0.1 0.1 0.001 5 force"
            "execute if entity @s[scores={temp=1..}] anchored feet positioned ^ ^ ^0.1 run function mf:step"
            |]
        yield "hit", [|
            "scoreboard players set @s temp -2"
            "execute as @e[type=!area_effect_cloud,type=!player,dx=0.01,dy=0.01,dz=0.01,sort=nearest,limit=1] run function mf:hit_coda"
            |]
        yield "hit_coda", [|
            "say hit @s"
            "data merge entity @s {Fire:160s}"
            |]
        |]
    
    for ns,name,code in [for name,code in magic_fire_funcs do yield! compiler.Compile("mf",name,code)] do
        pack.WriteFunction(ns,name,code)
    for ns,name,code in compiler.GetCompilerLoadTick() do
        pack.WriteFunction(ns,name,code)
    pack.WriteFunctionTagsFileWithValues("minecraft","load",[compiler.LoadFullName;"mf:init"])
    pack.WriteFunctionTagsFileWithValues("minecraft","tick",[compiler.TickFullName;"mf:tick"])
    pack.WriteBlocksTagsFileWithValues("mf","collisionless",[
        for s in MC_Constants.collisionless_blocks do
            if s.Contains(":") then 
                yield s
            else
                yield "minecraft:"+s
        ])
    pack.SaveToDisk()

let various_items() =
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "work")
    let pack = new Utilities.DataPackArchive(world,"hero_items","various items")
    let compiler = new Compiler.Compiler('h','i',"hi",false)
    let funcs = [|
        yield "init", [|
            """give @p minecraft:diamond_sword{ench:[{id:16s,lvl:10s},{id:21s,lvl:4s},{id:22s,lvl:6s}],Unbreakable:1b,display:{Name:"{\"text\":\"Master Sword\"}"}} 1"""
            """give @p minecraft:leather_helmet{ench:[{id:0s,lvl:8s}],display:{color:6192150,Name:"{\"text\":\"Hero's Cap\"}"},HeroCap:1b,Unbreakable:1b} 1"""
            """give @p minecraft:leather_chestplate{ench:[{id:0s,lvl:10s}],display:{color:6192150,Name:"{\"text\":\"Hero's Tunic\"}"},HeroTunic:1b,Unbreakable:1b} 1"""
            """give @p minecraft:leather_chestplate{ench:[{id:0s,lvl:5s}],display:{color:11546150,Name:"{\"text\":\"Fire Tunic\"}"},FireTunic:1b,Unbreakable:1b} 1"""
            """give @p minecraft:leather_chestplate{ench:[{id:0s,lvl:5s}],display:{color:3949738,Name:"{\"text\":\"Zora's Tunic\"}"},ZoraTunic:1b,Unbreakable:1b} 1"""
            """give @p minecraft:leather_leggings{ench:[{id:0s,lvl:7s}],display:{color:16383998,Name:"{\"text\":\"Hero's Leggings\"}"},Unbreakable:1b,AttributeModifiers:[{AttributeName:"generic.knockbackResistance",Name:"generic.knockbackResistance",Amount:1,Operation:0,Slot:"legs",UUIDMost:82829,UUIDLeast:167220}]} 1"""
            """give @p minecraft:leather_boots{ench:[{id:0s,lvl:6s}],display:{color:8606770,Name:"{\"text\":\"Hero's Boots\"}"},HeroBoots:1b,Unbreakable:1b} 1"""
            """give @p minecraft:leather_boots{ench:[{id:0s,lvl:3s},{id:8s,lvl:6s}],display:{color:3847130,Name:"{\"text\":\"Flippers\"}"},Flippers:1b,Unbreakable:1b} 1"""
            |]
        yield "tick", [|
            "execute as @a[nbt={Inventory:[{Slot:103b,tag:{HeroCap:1b}}]}] run effect give @s minecraft:strength 1 0"
            "execute as @a[nbt={Inventory:[{Slot:102b,tag:{HeroTunic:1b}}]}] run effect give @s minecraft:resistance 1 0"
            "execute as @a[nbt={Inventory:[{Slot:102b,tag:{FireTunic:1b}}]}] run effect give @s minecraft:fire_resistance 1 0"
            "execute as @a[nbt={Inventory:[{Slot:102b,tag:{ZoraTunic:1b}}]}] run effect give @s minecraft:water_breathing 1 0"
            "execute as @a[nbt={Inventory:[{Slot:100b,tag:{HeroBoots:1b}}]}] run effect give @s minecraft:speed 1 1"
            "execute as @a[nbt={Inventory:[{Slot:100b,tag:{Flippers:1b}}]}] run effect give @s minecraft:night_vision 1 0"
            |]
        |]
    for ns,name,code in [for name,code in funcs do yield! compiler.Compile("hi",name,code)] do
        pack.WriteFunction(ns,name,code)
    for ns,name,code in compiler.GetCompilerLoadTick() do
        pack.WriteFunction(ns,name,code)
    pack.WriteFunctionTagsFileWithValues("minecraft","load",[compiler.LoadFullName;"hi:init"])
    pack.WriteFunctionTagsFileWithValues("minecraft","tick",[compiler.TickFullName;"hi:tick"])
    pack.SaveToDisk()

let crystal_wand() =
    let world = System.IO.Path.Combine(Utilities.MC_ROOT, "work")
    let pack = new Utilities.DataPackArchive(world,"crystal_wand","crystal wand")
    let compiler = new Compiler.Compiler('c','w',"cw",false)
    let funcs = [|
        yield "init", [|
            "scoreboard objectives add temp dummy"  // used for 'found'
            "scoreboard objectives add pe minecraft.used:minecraft.emerald_block"
            // TODO fix
            """give @p minecraft:golden_pickaxe{Unbreakable:1b,CanDestroy:[emerald_block],display:{Name:"{\"text\":\"Crystal Wand\"}"}}"""
            """give @p minecraft:emerald_block{CanPlaceOn:[iron_block],display:{Name:"{\"text\":\"Crystal Key\"}"}}"""
            |]
        yield "tick", [|
            // check for pickup
            "scoreboard players set PICKUP temp 0"  // ensure summon at most one in multiplayer
            "execute at @a as @e[type=armor_stand,tag=CrystalKey,distance=..7] at @s if block ~ ~ ~ air run function cw:pickup"
            // check for placement
            "execute as @a[scores={pe=1}] at @s positioned ^ ^ ^1 align xyz positioned ~0.5 ~ ~0.5 run function cw:find"
            "scoreboard players set @a pe 0"
            |]
        yield "pickup", [|
            """execute if score PICKUP temp matches 0 run summon item ~ ~ ~ {Item:{id:"minecraft:emerald_block",Count:1b,tag:{CanPlaceOn:[iron_block],display:{Name:"{\"text\":\"Crystal Key\"}"}}}}"""
            "scoreboard players set PICKUP temp 1"
            "kill @s"
            |]
        yield "find", [|
            yield "scoreboard players set @s temp 0"  // found
            for diff = 0 to 9 do  // look nearby first, increase radius until found
                for x = -3 to 3 do
                    for y = -3 to 3 do
                        for z = -3 to 3 do
                            if (abs x + abs y + abs z) = diff then
                                yield sprintf "execute if score @s temp matches 0 if block ~%d ~%d ~%d emerald_block if block ~%d ~%d ~%d iron_block positioned ~%d ~%d ~%d run function cw:found" x y z x (y-1) z x y z
            yield "execute if score @s temp matches 0 run say error did not find placed emerald"
            |]
        yield "found", [|
            "scoreboard players set @s temp 1"
            "execute if block ~ ~-2 ~ air run scoreboard players set @s temp 2"
            "execute if score @s temp matches 2 run setblock ~ ~-2 ~ redstone_block"
            """execute if score @s temp matches 1 run summon armor_stand ~ ~ ~ {NoGravity:1b,Marker:1b,Invisible:0b,Tags:[CrystalKey],CustomName:"{\"text\":\"Crystal Key\"}",CustomNameVisible:1b}"""
            |]
        |]
    for ns,name,code in [for name,code in funcs do yield! compiler.Compile("cw",name,code)] do
        pack.WriteFunction(ns,name,code)
    for ns,name,code in compiler.GetCompilerLoadTick() do
        pack.WriteFunction(ns,name,code)
    pack.WriteFunctionTagsFileWithValues("minecraft","load",[compiler.LoadFullName;"cw:init"])
    pack.WriteFunctionTagsFileWithValues("minecraft","tick",[compiler.TickFullName;"cw:tick"])
    pack.SaveToDisk()

[<EntryPoint>]
let main argv = 
    //MinecraftBINGO.cardgen_compile()
    //MinecraftBINGOExtensions.Blind.main()   // TODO was crashing the game on reload (something with its sign, or looking at items frames, or who knows) maybe https://bugs.mojang.com/browse/MC-123363
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
    //test_conditional_reentrancy_in_compiled_code()
    //temple_locator()
    //local_v_relative()
    //dump_context()
    //fun_whip()
    //mob_replacement()
    //show_biomes()
    //test_json()
    //see_if_loot_tables_changed()
    //hover()
    //hero_bow()
    //tnt_bomb()
    //magic_fire_and_compass()
    //various_items()
    crystal_wand()
    //dump_context()
    ignore argv
    0


