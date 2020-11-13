module QuickStack

(*

quick stack design issues:
 - how to select which chest(s) to stack to (first pass, single nearest one they are looking at?)
 - gesture to invoke it
 - which player inventory items to stack (non hotbar/armor/shield I presume; assume hotbar and armor and shield are 'favorites')
 - how to visualize it


random notes:
 - scoreboard objectives add chest minecraft.custom:minecraft.open_chest




assume for a moment we know a player and a chest and have an invocation (so this is running one time, and performance is not critical).  let's sketch implementation...

for each item in the chest
    if its stackable and the current count is less than the max stack
        let N = how many more will fit
        for each player inv slot 9-35
            if N>0 && same item      // TODO how to detect e.g. enchanted magma cream "same item"
                let M = min(how many player has, N)
                reduce player inv in that slot by M  // item
                increase chest count by M            // data modify block
                N <- N - M

that would not fill any empty slots in the best, but it's not bad as a first pass


for "same item", I think we need complex logic along the lines of
    let ok = true
    let C = chest item nbt
    let P = player item nbt
    execute store result R run data modify C with P
    if R <> 0 then ok <- false
    let C = chest item nbt
    let P = player item nbt
    execute store result R run data modify P with C
    if R <> 0 then ok <- false
    ok

*)

let x = 0