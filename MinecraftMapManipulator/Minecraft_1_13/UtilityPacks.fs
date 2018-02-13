module UtilityPacks

let writePRNGto(worldSaveFolder) = 
    let dpa = new Utilities.DataPackArchive(worldSaveFolder, "prng", "pseudo-random number generator")
    let prng_init = [|
        "scoreboard objectives add PRNG_MOD dummy"
        "scoreboard objectives add PRNG_OUT dummy"
        "scoreboard objectives add Calc dummy"
        "scoreboard players set A Calc 1103515245"
        "scoreboard players set C Calc 12345"
        "scoreboard players set Two Calc 2"
        "scoreboard players set TwoToSixteen Calc 65536"
        |]
    let prng = [|
        // compute next Z value with PRNG
        "scoreboard players operation Z Calc *= A Calc"
        "scoreboard players operation Z Calc += C Calc"
        "scoreboard players operation Z Calc *= Two Calc"  // mod 2^31
        "scoreboard players operation Z Calc /= Two Calc"
        "scoreboard players operation K Calc = Z Calc"
        "scoreboard players operation K Calc *= Two Calc"
        "scoreboard players operation K Calc /= Two Calc"
        "scoreboard players operation K Calc /= TwoToSixteen Calc"   // upper 16 bits most random
        // get a number in the desired range
        "scoreboard players operation $ENTITY PRNG_OUT = K Calc"
        "scoreboard players operation $ENTITY PRNG_OUT %= $ENTITY PRNG_MOD"
        "scoreboard players operation $ENTITY PRNG_OUT += $ENTITY PRNG_MOD" // ensure non-negative
        "scoreboard players operation $ENTITY PRNG_OUT %= $ENTITY PRNG_MOD"
        |]
    let compiler = Compiler.Compiler('p','r',"prng",false)
    for ns,name,code in [yield! compiler.Compile("prng", "next", prng)
                         yield! compiler.Compile("prng", "init", prng_init)] do
        dpa.WriteFunction(ns, name, code)
    dpa.WriteFunctionTagsFileWithValues("minecraft","load",["prng:init"])
    dpa.SaveToDisk()