module Strings

// All the english text strings for the RCTM

type TranslatableString(s:string) =
    member this.Text = s

open NBT_Manipulation

let private displayNameAndLore(name, lore:_[]) = // lore can be null
    Compound("display",[|yield String("Name",name);
                         if lore <> null && lore.Length > 0 then
                             yield List("Lore",Strings(lore));
                         yield End|] |> ResizeArray)

let donationLink = """https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=457JUZA5FV924"""

let BOOK_IN_DUNGEON_OR_MINESHAFT_CHEST = 
    Compound("tag", Utilities.makeWrittenBookTags("Lorgon111","1. After gearing up",[|
         id """{"text":"Once you've geared up and are wearing metal armor, you should venture out into the night looking for GREEN beacon light. A challenging path will lead to riches!"}"""
     |]) |> ResizeArray)

let BOOK_IN_GREEN_BEACON_CHEST = 
    Compound("tag", Utilities.makeWrittenBookTags("Lorgon111","2. After green beacon cave",[|
         id """{"text":"If you feel protected enough, look for a RED beacon and try attacking a surface area filled with cobwebs... terrific rewards await you!"}"""
     |]) |> ResizeArray)

let BOOK_IN_FLAT_DUNGEON_CHEST = 
    Compound("tag", Utilities.makeWrittenBookTags("Lorgon111","3. After red beacon webs",[|
         id """{"text":"Once strong enough, attack dangerous-looking mountain peaks to get a map to the best treasure!"}"""
     |]) |> ResizeArray)

let BOOK_IN_MOUNTAIN_PEAK_CHEST =
    Compound("tag",Utilities.makeWrittenBookTags("Lorgon111","4. Secret Treasure",[| 
        id """{"text":"The secret treasure is buried at\nX : ","extra":[{"score":{"name":"X","objective":"hidden"}},{"text":"\nZ : "},{"score":{"name":"Z","objective":"hidden"}}]}"""
     |])|> ResizeArray)

// TODO quadrant stuff is sloppy, ideally should be 4 strings in both places
let BOOK_IN_HIDING_SPOT(quadrant:TranslatableString) =
    Compound("tag",Utilities.makeWrittenBookTags("Lorgon111","5. Final dungeon...",[| 
        sprintf """{"text":"The final dungeon entrance is marked by a PURPLE beacon found somewhere in the %s quadrant of the map! The other items from this chest should make traveling easier :)"}""" quadrant.Text 
     |])|> ResizeArray)
let QUADRANT_NORTHWEST = TranslatableString "NorthWest (-X,-Z)"
let QUADRANT_SOUTHWEST = TranslatableString "SouthWest (-X,+Z)"
let QUADRANT_NORTHEAST = TranslatableString "NorthEast (+X,-Z)"
let QUADRANT_SOUTHEAST = TranslatableString "SouthEast (+X,+Z)"
let TELEPORTER_TO_BLAH(quadrant:TranslatableString) = TranslatableString(sprintf "Teleporter to %s" quadrant.Text)
let BOOK_IN_FINAL_PURPLE_DUNGEON_CHEST =
    Compound("tag",Utilities.makeWrittenBookTags("Lorgon111","Congratulations!",[| 
        id """{"text":"Once all monument blocks are placed on the monument, you win! ..."}"""
        id """{"text":"I hope you enjoyed playing the map.  I am happy to hear your feedback, you can contact me at TODO..."}"""
        sprintf """[{"text":"If you enjoyed the map and would like to leave me a donation, I'd very much appreciate that!\n\n"},{"text":"Click to donate","underlined":true,"clickEvent":{"action":"open_url","value":"%s"}}]""" donationLink
     |])|> ResizeArray)

let STARTING_BOOK_RULES =
    Compound("tag", Utilities.makeWrittenBookTags(
                            "Lorgon111","Rules",
                            [|
                                Utilities.wrapInJSONTextContinued "RULES\n\nMy personal belief is that in Minecraft there are no rules; you should play whatever way you find most fun."
                                Utilities.wrapInJSONTextContinued "That said, I have created a map designed to help you have fun, and the next pages have some suggestions that I think will make it the most fun for most players."
                                Utilities.wrapInJSONTextContinued (sprintf "Suggestions\n\n%s\n\nSurvive in any way you can think of, and try to find the 3 monument blocks to place atop the monument at spawn." (if CustomizationKnobs.SINGLEPLAYER then "This map's loot is suitable for single-player." else "This map's loot is suitable for multi-player."))
                                Utilities.wrapInJSONText "Use normal difficulty.\n\nYou CAN use/move/craft enderchests.\n\nDon't go to Nether or leave the worldborder.\n\nYou can use beds to set spawn, but they don't affect the daylight cycle."
                            |]) |> ResizeArray)
let STARTING_BOOK_OVERVIEW =
    Compound("tag", Utilities.makeWrittenBookTags(
                            "Lorgon111","Map Overview",
                            [|
                                Utilities.wrapInJSONTextContinued "OVERVIEW\n\nCTM Maps are often most fun to play blind, but there are a few things you ought to know about this map before getting started."
                                Utilities.wrapInJSONTextContinued "Note: if you have not played Minecraft 1.9 before, I do not recommend playing this map as your first 1.9 map. Get used to 1.9 first."
                                Utilities.wrapInJSONTextContinued "CTM\n\nThis is a three objective Complete The Monument (CTM) map.  The goal/objective blocks you need are hidden in chests in various dungeons in the world."
                                Utilities.wrapInJSONTextContinued "OPEN WORLD\n\nThis is an open-world map that takes place on a 2048x2048 piece of (heavily modified) Minecraft terrain. Spawn is 0,0 and there's a worldborder 1024 blocks out."
                                Utilities.wrapInJSONTextContinued "...\nThere are multiple versions of most dungeons, and you'll find many just by wandering around. You'll find more books that suggest the best thing to do next after completing each dungeon."
                                Utilities.wrapInJSONTextContinued "DAYLIGHT CYCLE\n\nThe sun is not moving in the sky. Near spawn it's permanently daytime, and the rest you can discover for yourself."
                                Utilities.wrapInJSONTextContinued "MOBS\n\nMob loot drops are heavily modified in this map, but the mobs themselves are completely vanilla. There are many spawners in the map; both to guard loot, and to surprise you."
                                Utilities.wrapInJSONTextContinued "TECH PROGRESSION\n\nThere's no netherwart in the map and no potions given in chests.\n\nYou'll probably spend a little time with wood tools before managing to acquire some stone/gold/iron upgrades."
                                Utilities.wrapInJSONTextContinued "...\nThere will be lots of anvils and enchanted books. To progress, you do not have to farm xp/drops, mine for diamonds, or make an enchanting table, but you can if you want."
                                Utilities.wrapInJSONTextContinued "...\nThere will be emeralds in some loot chests. You should save them, as eventually you may unlock the ability to trade emeralds for some very useful buffs."
                                Utilities.wrapInJSONTextContinued "RANDOMLY GENERATED\n\nThis map was created entirely via algorithms. The Minecraft terrain generator made the original terrain, and my program added dungeons, loot, monument, & secrets automatically."
                                Utilities.wrapInJSONText "...\nIf you encounter something especially weird, don't over-think the map-maker rationale; it's possible my code had a bug and did something silly."
                            |]) |> ResizeArray)
let STARTING_BOOK_GETTING_STARTED = 
    Compound("tag", Utilities.makeWrittenBookTags(
                            "Lorgon111","Getting started",
                            [|
                                Utilities.wrapInJSONTextContinued "You have some starting items, but you'll still want to gather some wood and do some caving near spawn to get more supplies."
                                Utilities.wrapInJSONText "Explore! If you travel too far from spawn, things will get scarier, so I recommend caving near spawn to improve your gear until you are strong enough to venture further and you discover suggestions of what to try next."
                            |]) |> ResizeArray)

let STARTING_BOOK_FOOD_AND_COMBAT =
    Compound("tag", Utilities.makeWrittenBookTags(
                            "Lorgon111","1.9 Food and Combat",
                            [|
                                Utilities.wrapInJSONTextContinued "For those new to 1.9: Minecraft 1.9 changed the food and combat systems a lot. Here are some quick tips."
                                Utilities.wrapInJSONTextContinued "FOOD\n\nFood is no longer merely a survival mechanism. It's now also a combat mechanic, as over-feeding with high-saturation food will replenish life very quickly."
                                Utilities.wrapInJSONTextContinued "It helps to manage food carefully, preferring lower saturation foods (cookies, apples, ...) when you're 'safe', and save higher saturation foods (bread, steak, ...) for combat where rapid healing is valuable."
                                Utilities.wrapInJSONTextContinued "COMBAT\n\nClicking your weapons too quickly (to use them repeatedly) will cause them to deal less damage. Weapons now have cooldowns, which means you need to wait a half-second or more between attacks for maximal damage."
                                Utilities.wrapInJSONText "Stone, iron, and diamond axes are strong weapons, but they have long cooldowns. Swords deal less damage, but have shorter cooldowns, so you can swing them more often with no damage decrease."
                            |]) |> ResizeArray)

let STARTING_BOOK_HINTS_AND_SPOILERS = 
    Compound("tag", Utilities.makeWrittenBookTags(
                            "Lorgon111","Hints and Spoilers",
                            [|
                                Utilities.wrapInJSONTextContinued "The following pages outline the simplest 'progression order' of the map. You can refer to this if you get stuck, but DON'T READ THIS UNLESS YOU NEED TO BECAUSE YOU'RE STUCK."
                                Utilities.wrapInJSONTextContinued "Note: In the map folder on disk, there are two pictures of the terrain, one has locations of major dungeons labeled (spoilers), the other does not."
                                Utilities.wrapInJSONTextContinued "1. GEARING UP\n\nGetting your first cobblestone is not so easy, though there are at least 5 different ways you can obtain it."
                                Utilities.wrapInJSONTextContinued "...\nCaving near spawn to find dungeons (which are somewhat common) or abandoned mineshafts is the best way to find initial loot to gear up. You can also mine iron and gold (or even diamonds) for early gear."
                                Utilities.wrapInJSONTextContinued "2. GREEN BEACONS\n\nNext explore the world for GREEN beacons ('B' on spoiler map image), which lead to underground dungeons. You'll find a marked path through a cave and spawners guarding a good loot box."
                                Utilities.wrapInJSONTextContinued "3. RED BEACONS\n\nNext explore the world for RED beacons ('F' on spoiler map image): cobwebbed dungeons on the surface.  The loot box at the center has the first monument block and more gear upgrades."
                                Utilities.wrapInJSONTextContinued "4. MOUNTAIN PEAKS\n\nNext explore the world for dangerous looking mountain peaks ('P' on spoiler map image), illuminated with redstone torches. Buried treasure directions, and more loot!"
                                Utilities.wrapInJSONTextContinued "5. SECRET TREASURE\n\nNext dig for treasure at the given coordinates ('H' on the spoiler map image). You'll find the second monument block, and faster travel/exploration."
                                Utilities.wrapInJSONText "6. FINAL DUNGEON\n\nFinally explore one quadrant of the world for a PURPLE beacon ('X' on spoiler map image), the final dungeon. It's like the first dungeon, but harder, and has the final monument block."
                            |]) |> ResizeArray)
let TELEPORTER_HUB_BOOK =
    Compound("tag", Utilities.makeWrittenBookTags(
                            "Lorgon111","Teleport hub",
                            [|
                                Utilities.wrapInJSONTextContinued "This teleporter hub allows for faster travel to unlocked corners of the map. Each unlocked teleporter also provides a villager who trades emeralds for potion buffs."
                                Utilities.wrapInJSONText "If you can't see a villager in this room, exit/disconnect from the world, then reload/rejoin, and the villager should appear above the teleporter (Minecraft rendering bug)."
                            |]) |> ResizeArray)

let NAME_OF_FINAL_PURPLE_DUNGEON_CHEST = TranslatableString "Winner!"
let NAME_OF_GENERIC_TREASURE_BOX = TranslatableString "Lootz!"
let NAME_OF_DEAD_END_CHEST_IN_GREEN_DUNGEON = TranslatableString "Dead end, turn back & try again"
let NAME_OF_DEFAULT_MINECRAFT_DUNGEON_CHEST = TranslatableString "Spooky dungeon loot"
let NAME_OF_HIDDEN_TREASURE_CHEST = TranslatableString "Hidden treasure!"
let NAME_OF_STARTING_CHEST = TranslatableString "Welcome!"
let NAME_OF_TELEPORT_ROOM_CHEST = TranslatableString "Teleporter hub"
let NAME_OF_TELEPORTER_BREADCRUMBS_CHEST = TranslatableString "Keep your eyes open"
let NAME_OF_CHEST_ITEM_CONTAINING_MOUNTAIN_PEAK_LOOT = TranslatableString "Mountain Peak Loot"
let NAME_OF_CHEST_ITEM_CONTAINING_RED_BEACON_WEB_LOOT = TranslatableString "Red Beacon Web Loot"
let NAME_OF_CHEST_ITEM_CONTAINING_DUNGEON_LOOT = TranslatableString "Dungeon Loot"
let NAME_OF_CHEST_ITEM_CONTAINING_GREEN_BEACON_LOOT = TranslatableString "Green Beacon Cave Loot"
let NAME_OF_CHEST_ITEM_CONTAINING_AESTHETIC_BASIC_BLOCKS = TranslatableString "Basic Blocks"
let NAME_OF_CHEST_ITEM_CONTAINING_AESTHETIC_NICER_BLOCKS = TranslatableString "Nicer Blocks and Fun"

let POTION_SPEED_NAME = TranslatableString "Enduring Speed"
let POTION_HASTE_NAME = TranslatableString "Enduring Haste"
let POTION_STRENGTH_NAME = TranslatableString "Enduring Strength"
let POTION_HEALTH_BOOST_NAME = TranslatableString "Enduring Health Boost"
let POTION_LORE1 = TranslatableString "Lasts until you die"
let POTION_LORE2 = TranslatableString "or drink milk"

let TELLRAW_PLACED_A_MONUMENT_BLOCK = """tellraw @a ["You placed ",{"score":{"name":"CTM","objective":"hidden"}}," of 3 objective blocks so far!"]"""
let TELLRAW_TELEPORTER_UNLOCKED = """tellraw @a [{"text":"A two-way teleporter to/from spawn has been unlocked nearby"}]"""

let NBT_LUCKY_GAPPLE = """{display:{Name:\"Lucky Golden Apple\",Lore:[\"Extremely rare drop\",\"See? Bats are useful :)\"]}}"""
let NBT_FISHING = 
    Utilities.escape <| Utilities.writtenBookNBTString("Lorgon111","Nope!",[|
        sprintf """[{"text":"Fishing is over-powered, so I have disabled it.\n\nYour map-maker,\nDr. Brian Lorgon111\n\nP.S. If you like the map, feel free to "},{"text":"donate!","underlined":true,"clickEvent":{"action":"open_url","value":"%s"}}]""" donationLink
    |])

module NameAndLore =
    let SUPER_JUMP_BOOST = displayNameAndLore("Super jump boost",[|"Don't use without";"your elytra wings on!"|]) // TODO note 'elytra' here too
    let MONUMENT_BLOCK_PURPUR = displayNameAndLore("Monument Block: Purpur Block",null) // TODO ideally would use MC's item.whatever.name, but this is not a 1.9 translatable context
    let MONUMENT_BLOCK_END_STONE_BRICK = displayNameAndLore("Monument Block: End Stone Brick",null) // TODO ideally would use MC's item.whatever.name, but this is not a 1.9 translatable context
    let INNER_CHEST_WITH_NAME(name:TranslatableString) = displayNameAndLore(name.Text, [|"Place this chest"; "and open it"; "for more loot"|])
    let LUCK_POTION_DISPLAY = [String("Name","Your lucky DAY"); List("Lore", Strings[|"While this luck"; "potion is active"; "it will remain"; "daylight, even"; "away from spawn"|]); End]
    let DIVINING_ROD_LORE = "XXX" //"Hold me to locate loot!" //must be one-line