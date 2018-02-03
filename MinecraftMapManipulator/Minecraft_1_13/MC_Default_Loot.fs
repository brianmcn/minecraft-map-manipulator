﻿module MC_Default_Loot

// last updated 18w01a

let abandoned_mineshaft = """
{ "pools" : [
   {  "rolls" : 1,
      "entries" : [
         { "type" : "item", "name" : "minecraft:golden_apple"          , "weight" : 20 },
         { "type" : "item", "name" : "minecraft:enchanted_golden_apple", "weight" : 1 },
         { "type" : "item", "name" : "minecraft:name_tag"              , "weight" : 30 },
         { "type" : "item", "name" : "minecraft:book"                  , "weight" : 10, "functions" : [ { "function" : "enchant_randomly" } ] },
         { "type" : "item", "name" : "minecraft:iron_pickaxe"          , "weight" : 5 },
         { "type" : "empty", "weight" : 5 } ] },
   {  "rolls" : { "min" : 2, "max" : 4 },
      "entries" : [
         { "type" : "item", "name" : "minecraft:iron_ingot"            , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 5 } } ], "weight" : 10 },
         { "type" : "item", "name" : "minecraft:gold_ingot"            , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 5 },
         { "type" : "item", "name" : "minecraft:redstone"              , "functions" : [ { "function" : "set_count", "count" : { "min" : 4, "max" : 9 } } ], "weight" : 5 },
         { "type" : "item", "name" : "minecraft:lapis_lazuli"          , "functions" : [ { "function" : "set_count", "count" : { "min" : 4, "max" : 9 } } ], "weight" : 5 },
         { "type" : "item", "name" : "minecraft:diamond"               , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 2 } } ], "weight" : 3 },
         { "type" : "item", "name" : "minecraft:coal"                  , "functions" : [ { "function" : "set_count", "count" : { "min" : 3, "max" : 8 } } ], "weight" : 10 },
         { "type" : "item", "name" : "minecraft:bread"                 , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 15 },
         { "type" : "item", "name" : "minecraft:melon_seeds"           , "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 4 } } ], "weight" : 10 },
         { "type" : "item", "name" : "minecraft:pumpkin_seeds"         , "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 4 } } ], "weight" : 10 },
         { "type" : "item", "name" : "minecraft:beetroot_seeds"        , "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 4 } } ], "weight" : 10 } ] },
   {  "rolls" : 3,
      "entries" : [
         { "type" : "item", "name" : "minecraft:rail"                  , "functions" : [ { "function" : "set_count", "count" : { "min" : 4, "max" : 8 } } ], "weight" : 20 },
         { "type" : "item", "name" : "minecraft:powered_rail"          , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 4 } } ], "weight" : 5 },
         { "type" : "item", "name" : "minecraft:detector_rail"         , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 4 } } ], "weight" : 5 },
         { "type" : "item", "name" : "minecraft:activator_rail"        , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 4 } } ], "weight" : 5 },
         { "type" : "item", "name" : "minecraft:torch"                 , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 16 } } ], "weight" : 15 } ] } ] }
"""

let desert_pyramid = """
{ "pools" : [
   {  "rolls" : { "min" : 2, "max" : 4 },
      "entries" : [
         { "type" : "item", "name" : "minecraft:diamond"               , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 5 },
         { "type" : "item", "name" : "minecraft:iron_ingot"            , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 5 } } ], "weight" : 15 },
         { "type" : "item", "name" : "minecraft:gold_ingot"            , "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 7 } } ], "weight" : 15 },
         { "type" : "item", "name" : "minecraft:emerald"               , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 15 },
         { "type" : "item", "name" : "minecraft:bone"                  , "functions" : [ { "function" : "set_count", "count" : { "min" : 4, "max" : 6 } } ], "weight" : 25 },
         { "type" : "item", "name" : "minecraft:spider_eye"            , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 25 },
         { "type" : "item", "name" : "minecraft:rotten_flesh"          , "functions" : [ { "function" : "set_count", "count" : { "min" : 3, "max" : 7 } } ], "weight" : 25 },
         { "type" : "item", "name" : "minecraft:saddle"                , "weight" : 20 },
         { "type" : "item", "name" : "minecraft:iron_horse_armor"      , "weight" : 15 },
         { "type" : "item", "name" : "minecraft:golden_horse_armor"    , "weight" : 10 },
         { "type" : "item", "name" : "minecraft:diamond_horse_armor"   , "weight" : 5 },
         { "type" : "item", "name" : "minecraft:book"                  , "weight" : 20, "functions" : [ { "function" : "enchant_randomly" } ] },
         { "type" : "item", "name" : "minecraft:golden_apple"          , "weight" : 20 },
         { "type" : "item", "name" : "minecraft:enchanted_golden_apple", "weight" : 2 },
         { "type" : "empty", "weight" : 15 } ] },
   {  "rolls" : 4,
      "entries" : [
         { "type" : "item", "name" : "minecraft:bone"                  , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 8 } } ] },
         { "type" : "item", "name" : "minecraft:gunpowder"             , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 8 } } ] },
         { "type" : "item", "name" : "minecraft:rotten_flesh"          , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 8 } } ] },
         { "type" : "item", "name" : "minecraft:string"                , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 8 } } ] },
         { "type" : "item", "name" : "minecraft:sand"                  , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 8 } } ] } ] } ] }
"""

let end_city_treasure = """
{ "pools" : [ {
  "rolls" : { "min" : 2, "max" : 6 },
  "entries" : [
    { "type" : "item", "name" : "minecraft:diamond"               , "weight" : 5, "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 7 } } ] },
    { "type" : "item", "name" : "minecraft:iron_ingot"            , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 4, "max" : 8 } } ] },
    { "type" : "item", "name" : "minecraft:gold_ingot"            , "weight" : 15, "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 7 } } ] },
    { "type" : "item", "name" : "minecraft:emerald"               , "weight" : 2, "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 6 } } ] },
    { "type" : "item", "name" : "minecraft:beetroot_seeds"        , "weight" : 5, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 10 } } ] },
    { "type" : "item", "name" : "minecraft:saddle"                , "weight" : 3 },
    { "type" : "item", "name" : "minecraft:iron_horse_armor"      , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:golden_horse_armor"    , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:diamond_horse_armor"   , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:diamond_sword"         , "weight" : 3, "functions" : [ { "function" : "enchant_with_levels", "treasure" : true, "levels" : { "min" : 20, "max" : 39 } } ] },
    { "type" : "item", "name" : "minecraft:diamond_boots"         , "weight" : 3, "functions" : [ { "function" : "enchant_with_levels", "treasure" : true, "levels" : { "min" : 20, "max" : 39 } } ] },
    { "type" : "item", "name" : "minecraft:diamond_chestplate"    , "weight" : 3, "functions" : [ { "function" : "enchant_with_levels", "treasure" : true, "levels" : { "min" : 20, "max" : 39 } } ] },
    { "type" : "item", "name" : "minecraft:diamond_leggings"      , "weight" : 3, "functions" : [ { "function" : "enchant_with_levels", "treasure" : true, "levels" : { "min" : 20, "max" : 39 } } ] },
    { "type" : "item", "name" : "minecraft:diamond_helmet"        , "weight" : 3, "functions" : [ { "function" : "enchant_with_levels", "treasure" : true, "levels" : { "min" : 20, "max" : 39 } } ] },
    { "type" : "item", "name" : "minecraft:diamond_pickaxe"       , "weight" : 3, "functions" : [ { "function" : "enchant_with_levels", "treasure" : true, "levels" : { "min" : 20, "max" : 39 } } ] },
    { "type" : "item", "name" : "minecraft:diamond_shovel"        , "weight" : 3, "functions" : [ { "function" : "enchant_with_levels", "treasure" : true, "levels" : { "min" : 20, "max" : 39 } } ] },
    { "type" : "item", "name" : "minecraft:iron_sword"            , "weight" : 3, "functions" : [ { "function" : "enchant_with_levels", "treasure" : true, "levels" : { "min" : 20, "max" : 39 } } ] },
    { "type" : "item", "name" : "minecraft:iron_boots"            , "weight" : 3, "functions" : [ { "function" : "enchant_with_levels", "treasure" : true, "levels" : { "min" : 20, "max" : 39 } } ] },
    { "type" : "item", "name" : "minecraft:iron_chestplate"       , "weight" : 3, "functions" : [ { "function" : "enchant_with_levels", "treasure" : true, "levels" : { "min" : 20, "max" : 39 } } ] },
    { "type" : "item", "name" : "minecraft:iron_leggings"         , "weight" : 3, "functions" : [ { "function" : "enchant_with_levels", "treasure" : true, "levels" : { "min" : 20, "max" : 39 } } ] },
    { "type" : "item", "name" : "minecraft:iron_helmet"           , "weight" : 3, "functions" : [ { "function" : "enchant_with_levels", "treasure" : true, "levels" : { "min" : 20, "max" : 39 } } ] },
    { "type" : "item", "name" : "minecraft:iron_pickaxe"          , "weight" : 3, "functions" : [ { "function" : "enchant_with_levels", "treasure" : true, "levels" : { "min" : 20, "max" : 39 } } ] },
    { "type" : "item", "name" : "minecraft:iron_shovel"           , "weight" : 3, "functions" : [ { "function" : "enchant_with_levels", "treasure" : true, "levels" : { "min" : 20, "max" : 39 } } ] } ] } ] }
"""

let igloo_chest = """
{ "pools" : [
  { "rolls" : { "min" : 2, "max" : 8 },
    "entries" : [
      { "type" : "item", "name" : "minecraft:apple"                 , "weight" : 15, "functions" : [ { "function" : "minecraft:set_count", "count" : { "min" : 1, "max" : 3 } } ] },
      { "type" : "item", "name" : "minecraft:coal"                  , "weight" : 15, "functions" : [ { "function" : "minecraft:set_count", "count" : { "min" : 1, "max" : 4 } } ] },
      { "type" : "item", "name" : "minecraft:gold_nugget"           , "weight" : 10, "functions" : [ { "function" : "minecraft:set_count", "count" : { "min" : 1, "max" : 3 } } ] },
      { "type" : "item", "name" : "minecraft:stone_axe"             , "weight" : 2 },
      { "type" : "item", "name" : "minecraft:rotten_flesh"          , "weight" : 10 },
      { "type" : "item", "name" : "minecraft:emerald"               , "weight" : 1 },
      { "type" : "item", "name" : "minecraft:wheat"                 , "weight" : 10, "functions" : [ { "function" : "minecraft:set_count", "count" : { "min" : 2, "max" : 3 } } ] } ] },
  { "rolls" : 1, "entries" : [ { "type" : "item", "name" : "minecraft:golden_apple"          , "weight" : 1 } ] } ] }
"""

let jungle_temple = """
{ "pools" : [ {
  "rolls" : { "min" : 2, "max" : 6 },
  "entries" : [
    { "type" : "item", "name" : "minecraft:diamond"               , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 3 },
    { "type" : "item", "name" : "minecraft:iron_ingot"            , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 5 } } ], "weight" : 10 },
    { "type" : "item", "name" : "minecraft:gold_ingot"            , "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 7 } } ], "weight" : 15 },
    { "type" : "item", "name" : "minecraft:emerald"               , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 2 },
    { "type" : "item", "name" : "minecraft:bone"                  , "functions" : [ { "function" : "set_count", "count" : { "min" : 4, "max" : 6 } } ], "weight" : 20 },
    { "type" : "item", "name" : "minecraft:rotten_flesh"          , "functions" : [ { "function" : "set_count", "count" : { "min" : 3, "max" : 7 } } ], "weight" : 16 },
    { "type" : "item", "name" : "minecraft:saddle"                , "weight" : 3 },
    { "type" : "item", "name" : "minecraft:iron_horse_armor"      , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:golden_horse_armor"    , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:diamond_horse_armor"   , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:book"                  , "weight" : 1, "functions" : [ { "function" : "enchant_with_levels", "levels" : 30, "treasure" : true } ] } ] } ] }
"""

let nether_bridge = """
{ "pools" : [ {
  "rolls" : { "min" : 2, "max" : 4 },
  "entries" : [
    { "type" : "item", "name" : "minecraft:diamond"               , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 5 },
    { "type" : "item", "name" : "minecraft:iron_ingot"            , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 5 } } ], "weight" : 5 },
    { "type" : "item", "name" : "minecraft:gold_ingot"            , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 15 },
    { "type" : "item", "name" : "minecraft:golden_sword"          , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:golden_chestplate"     , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:flint_and_steel"       , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:nether_wart"           , "functions" : [ { "function" : "set_count", "count" : { "min" : 3, "max" : 7 } } ], "weight" : 5 },
    { "type" : "item", "name" : "minecraft:saddle"                , "weight" : 10 },
    { "type" : "item", "name" : "minecraft:golden_horse_armor"    , "weight" : 8 },
    { "type" : "item", "name" : "minecraft:iron_horse_armor"      , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:diamond_horse_armor"   , "weight" : 3 },
    { "type" : "item", "name" : "minecraft:obsidian"              , "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 4 } } ], "weight" : 2 } ] } ] }
"""

let simple_dungeon = """
{ "pools" : [
  { "rolls" : { "min" : 1, "max" : 3 },
    "entries" : [
      { "type" : "item", "name" : "minecraft:saddle"                , "weight" : 20 },
      { "type" : "item", "name" : "minecraft:golden_apple"          , "weight" : 15 },
      { "type" : "item", "name" : "minecraft:enchanted_golden_apple", "weight" : 2 },
      { "type" : "item", "name" : "minecraft:music_disc_13"         , "weight" : 15 },
      { "type" : "item", "name" : "minecraft:music_disc_cat"        , "weight" : 15 },
      { "type" : "item", "name" : "minecraft:name_tag"              , "weight" : 20 },
      { "type" : "item", "name" : "minecraft:golden_horse_armor"    , "weight" : 10 },
      { "type" : "item", "name" : "minecraft:iron_horse_armor"      , "weight" : 15 },
      { "type" : "item", "name" : "minecraft:diamond_horse_armor"   , "weight" : 5 },
      { "type" : "item", "name" : "minecraft:book"                  , "weight" : 10, "functions" : [ { "function" : "enchant_randomly" } ] } ] },
  { "rolls" : { "min" : 1, "max" : 4 },
    "entries" : [
      { "type" : "item", "name" : "minecraft:iron_ingot"            , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 4 } } ] },
      { "type" : "item", "name" : "minecraft:gold_ingot"            , "weight" : 5, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 4 } } ] },
      { "type" : "item", "name" : "minecraft:bread"                 , "weight" : 20 },
      { "type" : "item", "name" : "minecraft:wheat"                 , "weight" : 20, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 4 } } ] },
      { "type" : "item", "name" : "minecraft:bucket"                , "weight" : 10 },
      { "type" : "item", "name" : "minecraft:redstone"              , "weight" : 15, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 4 } } ] },
      { "type" : "item", "name" : "minecraft:coal"                  , "weight" : 15, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 4 } } ] },
      { "type" : "item", "name" : "minecraft:melon_seeds"           , "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 4 } } ], "weight" : 10 },
      { "type" : "item", "name" : "minecraft:pumpkin_seeds"         , "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 4 } } ], "weight" : 10 },
      { "type" : "item", "name" : "minecraft:beetroot_seeds"        , "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 4 } } ], "weight" : 10 } ] },
  { "rolls" : 3,
    "entries" : [
      { "type" : "item", "name" : "minecraft:bone"                  , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 8 } } ] },
      { "type" : "item", "name" : "minecraft:gunpowder"             , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 8 } } ] },
      { "type" : "item", "name" : "minecraft:rotten_flesh"          , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 8 } } ] },
      { "type" : "item", "name" : "minecraft:string"                , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 8 } } ] } ] } ] }
"""

let stronghold_corridor = """
{ "pools" : [ {
  "rolls" : { "min" : 2, "max" : 3 },
  "entries" : [
    { "type" : "item", "name" : "minecraft:ender_pearl"           , "weight" : 10 },
    { "type" : "item", "name" : "minecraft:diamond"               , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 3 },
    { "type" : "item", "name" : "minecraft:iron_ingot"            , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 5 } } ], "weight" : 10 },
    { "type" : "item", "name" : "minecraft:gold_ingot"            , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 5 },
    { "type" : "item", "name" : "minecraft:redstone"              , "functions" : [ { "function" : "set_count", "count" : { "min" : 4, "max" : 9 } } ], "weight" : 5 },
    { "type" : "item", "name" : "minecraft:bread"                 , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 15 },
    { "type" : "item", "name" : "minecraft:apple"                 , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 15 },
    { "type" : "item", "name" : "minecraft:iron_pickaxe"          , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:iron_sword"            , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:iron_chestplate"       , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:iron_helmet"           , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:iron_leggings"         , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:iron_boots"            , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:golden_apple"          , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:saddle"                , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:iron_horse_armor"      , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:golden_horse_armor"    , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:diamond_horse_armor"   , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:book"                  , "weight" : 1, "functions" : [ { "function" : "enchant_with_levels", "levels" : 30, "treasure" : true } ] } ] } ] }
"""

let stronghold_crossing = """
{ "pools" : [ {
  "rolls" : { "min" : 1, "max" : 4 },
  "entries" : [
    { "type" : "item", "name" : "minecraft:iron_ingot"            , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 5 } } ], "weight" : 10 },
    { "type" : "item", "name" : "minecraft:gold_ingot"            , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 5 },
    { "type" : "item", "name" : "minecraft:redstone"              , "functions" : [ { "function" : "set_count", "count" : { "min" : 4, "max" : 9 } } ], "weight" : 5 },
    { "type" : "item", "name" : "minecraft:coal"                  , "functions" : [ { "function" : "set_count", "count" : { "min" : 3, "max" : 8 } } ], "weight" : 10 },
    { "type" : "item", "name" : "minecraft:bread"                 , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 15 },
    { "type" : "item", "name" : "minecraft:apple"                 , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 15 },
    { "type" : "item", "name" : "minecraft:iron_pickaxe"          , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:book"                  , "weight" : 1, "functions" : [ { "function" : "enchant_with_levels", "levels" : 30, "treasure" : true } ] } ] } ] }
"""

let stronghold_library = """
{ "pools" : [ {
  "rolls" : { "min" : 2, "max" : 10 },
  "entries" : [
    { "type" : "item", "name" : "minecraft:book"                  , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 20 },
    { "type" : "item", "name" : "minecraft:paper"                 , "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 7 } } ], "weight" : 20 },
    { "type" : "item", "name" : "minecraft:map"                   , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:compass"               , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:book"                  , "weight" : 10, "functions" : [ { "function" : "enchant_with_levels", "levels" : 30, "treasure" : true } ] } ] } ] }
"""

let village_blacksmith = """
{ "pools" : [ {
  "rolls" : { "min" : 3, "max" : 8 },
  "entries" : [
    { "type" : "item", "name" : "minecraft:diamond"               , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 3 },
    { "type" : "item", "name" : "minecraft:iron_ingot"            , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 5 } } ], "weight" : 10 },
    { "type" : "item", "name" : "minecraft:gold_ingot"            , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 5 },
    { "type" : "item", "name" : "minecraft:bread"                 , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 15 },
    { "type" : "item", "name" : "minecraft:apple"                 , "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 3 } } ], "weight" : 15 },
    { "type" : "item", "name" : "minecraft:iron_pickaxe"          , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:iron_sword"            , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:iron_chestplate"       , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:iron_helmet"           , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:iron_leggings"         , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:iron_boots"            , "weight" : 5 },
    { "type" : "item", "name" : "minecraft:obsidian"              , "functions" : [ { "function" : "set_count", "count" : { "min" : 3, "max" : 7 } } ], "weight" : 5 },
    { "type" : "item", "name" : "minecraft:oak_sapling"           , "functions" : [ { "function" : "set_count", "count" : { "min" : 3, "max" : 7 } } ], "weight" : 5 },
    { "type" : "item", "name" : "minecraft:saddle"                , "weight" : 3 },
    { "type" : "item", "name" : "minecraft:iron_horse_armor"      , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:golden_horse_armor"    , "weight" : 1 },
    { "type" : "item", "name" : "minecraft:diamond_horse_armor"   , "weight" : 1 } ] } ] }
"""

let woodland_mansion = """
{ "pools" : [
  { "rolls" : { "min" : 1, "max" : 3 },
    "entries" : [
      { "type" : "item", "name" : "minecraft:lead"                  , "weight" : 20 },
      { "type" : "item", "name" : "minecraft:golden_apple"          , "weight" : 15 },
      { "type" : "item", "name" : "minecraft:enchanted_golden_apple", "weight" : 2 },
      { "type" : "item", "name" : "minecraft:music_disc_13"         , "weight" : 15 },
      { "type" : "item", "name" : "minecraft:music_disc_cat"        , "weight" : 15 },
      { "type" : "item", "name" : "minecraft:name_tag"              , "weight" : 20 },
      { "type" : "item", "name" : "minecraft:chainmail_chestplate"  , "weight" : 10 },
      { "type" : "item", "name" : "minecraft:diamond_hoe"           , "weight" : 15 },
      { "type" : "item", "name" : "minecraft:diamond_chestplate"    , "weight" : 5 },
      { "type" : "item", "name" : "minecraft:book"                  , "weight" : 10, "functions" : [ { "function" : "enchant_randomly" } ] } ] },
  { "rolls" : { "min" : 1, "max" : 4 },
    "entries" : [
      { "type" : "item", "name" : "minecraft:iron_ingot"            , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 4 } } ] },
      { "type" : "item", "name" : "minecraft:gold_ingot"            , "weight" : 5, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 4 } } ] },
      { "type" : "item", "name" : "minecraft:bread"                 , "weight" : 20 },
      { "type" : "item", "name" : "minecraft:wheat"                 , "weight" : 20, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 4 } } ] },
      { "type" : "item", "name" : "minecraft:bucket"                , "weight" : 10 },
      { "type" : "item", "name" : "minecraft:redstone"              , "weight" : 15, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 4 } } ] },
      { "type" : "item", "name" : "minecraft:coal"                  , "weight" : 15, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 4 } } ] },
      { "type" : "item", "name" : "minecraft:melon_seeds"           , "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 4 } } ], "weight" : 10 },
      { "type" : "item", "name" : "minecraft:pumpkin_seeds"         , "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 4 } } ], "weight" : 10 },
      { "type" : "item", "name" : "minecraft:beetroot_seeds"        , "functions" : [ { "function" : "set_count", "count" : { "min" : 2, "max" : 4 } } ], "weight" : 10 } ] },
  { "rolls" : 3,
    "entries" : [
      { "type" : "item", "name" : "minecraft:bone"                  , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 8 } } ] },
      { "type" : "item", "name" : "minecraft:gunpowder"             , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 8 } } ] },
      { "type" : "item", "name" : "minecraft:rotten_flesh"          , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 8 } } ] },
      { "type" : "item", "name" : "minecraft:string"                , "weight" : 10, "functions" : [ { "function" : "set_count", "count" : { "min" : 1, "max" : 8 } } ] } ] } ] }
"""

let chest_loot_tables = [|
    """chests/abandoned_mineshaft.json""" , abandoned_mineshaft
    """chests/desert_pyramid.json"""      , desert_pyramid
    """chests/end_city_treasure.json"""   , end_city_treasure
    """chests/igloo_chest.json"""         , igloo_chest
    """chests/jungle_temple.json"""       , jungle_temple
    """chests/nether_bridge.json"""       , nether_bridge
    """chests/simple_dungeon.json"""      , simple_dungeon
    """chests/stronghold_corridor.json""" , stronghold_corridor
    """chests/stronghold_crossing.json""" , stronghold_crossing
    """chests/stronghold_library.json"""  , stronghold_library
    """chests/village_blacksmith.json"""  , village_blacksmith
    """chests/woodland_mansion.json"""    , woodland_mansion
    |]

