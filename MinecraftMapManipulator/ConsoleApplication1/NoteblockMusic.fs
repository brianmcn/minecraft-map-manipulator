module NoteblockMusic

let pitchArray = [|
    0.5
    0.53
    0.56
    0.59
    0.63
    0.67
    0.71
    0.75
    0.79
    0.84
    0.89
    0.94    // F
    1.0     //          13
    1.06    // G
    1.12
    1.19    // A
    1.26
    1.33    // B
    1.41    // C        19
    1.5
    1.59
    1.68
    1.78
    1.89
    2.0
    |]

type Notes = 
    | F | G | A | B | C | Rest
    member this.Pitch =
        let K = 1
        match this with
        | F -> pitchArray.[12-K]
        | G -> pitchArray.[14-K]
        | A -> pitchArray.[16-K]
        | B -> pitchArray.[18-K]
        | C -> pitchArray.[19-K]
        | Rest -> failwith "no pitch for Rest"

// Sonic Green Hill Zone
// https://musescore.com/user/15992/scores/138675

let sonicTop = [|
    C; B; A; G;
    C; B; A; G;
    C; B; A; G;
    C; B; A; G;

    C; B; A; G;
    C; B; A; G;
    C; B; A; G;
    C; B; A; G;

    C; B; A; G;
    C; B; A; G;
    C; B; A; G;
    C; B; A; G;

    C; B; A; G;
    C; B; A; G;
    C; B; A; G;
    C; B; A; G;
    |]

let sonicMid = [|
    B; Rest; Rest
    A; Rest; Rest
    B; Rest; Rest
    A; Rest; Rest
    B; Rest
    A; Rest

    C; Rest; Rest
    B; Rest; Rest
    A; Rest; Rest; Rest; Rest; Rest; Rest; Rest; Rest; Rest

    A; Rest; Rest
    B; Rest; Rest
    C; Rest
    A; Rest; Rest
    B; Rest; Rest
    C; Rest

    C; Rest; Rest
    B; Rest; Rest; Rest; Rest; Rest; Rest; Rest; Rest
    Rest; Rest; Rest; Rest
    |]

open FunctionCompiler 

let mutable ticksPerEighth = 4

let bbns = Array.init 100 (fun i -> BBN(sprintf "music%02d" i))
let convert() =
    let mutable bbn = 0
    let mutable i = 0
    let basicBlocks = ResizeArray()
    while i < 64 do
        // get notes
        let cmds = ResizeArray()
        if sonicTop.[i] <> Rest then
            cmds.Add(sprintf "execute @p ~ ~ ~ playsound block.note.harp master @p ~ ~ ~ 0.6 %1.2f" sonicTop.[i].Pitch)
        if sonicMid.[i] <> Rest then
            cmds.Add(sprintf "execute @p ~ ~ ~ playsound block.note.guitar master @p ~ ~ ~ 1.0 %1.2f" sonicMid.[i].Pitch)
        // also consider chime/bell/flute
        // get rests
        i <- i + 1
        let mutable rests = 1
        while i < 64 && sonicTop.[i] = Rest && sonicMid.[i] = Rest do
            i <- i + 1
            rests <- rests + 1
        // make bb
        basicBlocks.Add(bbns.[bbn],BasicBlock(cmds.ToArray()|>Array.map(fun s -> AtomicCommand(s)),
            (if i=64 then DirectTailCall(bbns.[0]) else DirectTailCall(bbns.[bbn+1])),
            MustWaitNTicks(rests*ticksPerEighth)))
        bbn <- bbn + 1
    Program(new Scope(),[||],[||],[||],bbns.[0],dict basicBlocks)