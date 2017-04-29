module Advancements

open Recipes // for MS

type DisplayFrame = Task | Challenge | Goal
type Condition =
    | NumInventorySlotsFull of int
    | HasItems of MS[]
    | HasRecipe of MS
type Criterion =
    | Criterion of (*name*) string * (*trigger*) MS * Condition[]
type Reward =
    | Reward of (*recipes*) MS[] * (*loottables*) MS[] * (*experience*) int * (*commands*) string[]
    | NoReward
type Display =
    | NoDisplay
    | Display of (*title*) string * (*description*) string * (*icon*) MS * DisplayFrame * (*backgroundImageIfRoot*) MS option
type Advancement =
    | Advancement of (*parent*) MS option * Display * Reward * Criterion[] * (*requirements in DNF*) string[][]
    member this.Write(w:System.IO.TextWriter) =
        let ICH s = s |> Seq.toArray |> (fun a -> Array.init a.Length (fun i -> a.[i], if i<>a.Length-1 then ", " else "")) // interspersed comma helper
        let intersperse s = 
            let sb = new System.Text.StringBuilder()
            for x,c in ICH s do
                sb.Append(x.ToString()+c)  |> ignore
            sb.ToString()
        w.WriteLine("""{""")
        match this with
        | Advancement(parentOpt,display,reward,criteria,reqsDNF) ->
            // parent
            match parentOpt with
            | None -> ()
            | Some parent -> w.WriteLine(sprintf """    "parent": "%s",""" (parent.ToString()))
            // display
            match display with
            | NoDisplay -> ()
            | Display(title, description, icon, frame, backgroundImageOpt) ->
                w.WriteLine("""    "display": {""")
                match backgroundImageOpt with 
                | None -> ()
                | Some background -> w.WriteLine(sprintf """        "background": "%s",""" (background.ToString()))
                w.WriteLine(sprintf """        "title": "%s",""" title)
                w.WriteLine(sprintf """        "description": "%s",""" description)
                w.WriteLine(sprintf """        "icon": "%s",""" (icon.ToString()))
                w.WriteLine(sprintf """        "frame": "%s" """ (match frame with Task -> "task" | Challenge -> "challenge" | Goal -> "goal"))
                w.WriteLine("""    },""")
            // reward
            match reward with
            | NoReward -> ()
            | Reward(recipes, loottables, experience, commands) ->
                w.WriteLine(sprintf """    "rewards": {""")
                if recipes.Length<>0 then w.WriteLine(sprintf """        "recipes": [%s],""" (intersperse recipes))
                if loottables.Length<>0 then w.WriteLine(sprintf """        "loot": [%s],""" (intersperse loottables))
                if experience<>0 then w.WriteLine(sprintf """        "experience": %d,""" experience)
                if commands.Length<>0 then 
                    w.WriteLine(sprintf """        "commands": [""")
                    let escape (s:string) =
                        s.Replace("\"","\\\"") // TODO what-all needs to be escaped in JSON strings?
                    for i,c in ICH commands do
                        w.WriteLine(sprintf """                    "%s"%s""" (escape i) c)
                    w.WriteLine(sprintf """        ],""")
                w.WriteLine("""        "comma": 0 },""")
            // criteria
            w.WriteLine("""    "criteria": {""")
            for Criterion(name,trigger,conditions),c in ICH criteria do
                w.WriteLine(sprintf """        "%s": {""" name)
                w.WriteLine(sprintf """            "trigger": "%s",""" (trigger.ToString()))
                w.WriteLine("""            "conditions": {""")
                for cond,c in ICH conditions do
                    match cond with
                    | NumInventorySlotsFull n ->
                        w.WriteLine(sprintf """                "slots": { "occupied": %d }%s""" n c)
                    | HasItems items ->
                        w.WriteLine(sprintf """                "items": [""")
                        for i,c in ICH items do
                            w.WriteLine(sprintf """                    { "item": %s }%s""" (i.ToString()) c)
                        w.WriteLine(sprintf """                ]""")
                    | HasRecipe r ->
                        w.WriteLine(sprintf """                "recipe": "%s"%s""" (r.ToString()) c)
                w.WriteLine("""            }""")
                w.WriteLine(sprintf """        }%s""" c)
            w.WriteLine(sprintf """    },""")
            // requirements
            w.WriteLine(sprintf """    "requirements": [%s]""" (
                let sb = new System.Text.StringBuilder()
                for a,c in ICH reqsDNF do
                    sb.Append("[") |> ignore
                    for s,c in ICH a do
                        sb.Append("\""+s+"\""+c)  |> ignore
                    sb.Append("]").Append(c)  |> ignore
                sb.ToString()
                ))
        w.WriteLine("""}""")

let writeAdvancements(advancements, worldSaveFolder) =
    for (name:string, a:Advancement) in advancements do
        if name<>name.ToLowerInvariant() then
            failwithf "bad recipe name has uppercase: %s" name
        let pathBits = name.Split [|':';'/'|]
        let wslt = System.IO.Path.Combine [| yield worldSaveFolder; yield "data"; yield "advancements" |]
        let filename = System.IO.Path.Combine [| yield wslt; yield! pathBits |]
        let filename = filename + ".json"
        if System.IO.File.Exists(filename) then
            System.IO.File.Delete(filename)
        System.IO.Directory.CreateDirectory( System.IO.Path.GetDirectoryName(filename) ) |> ignore
        use stream = new System.IO.StreamWriter( System.IO.File.OpenWrite(filename) )
        a.Write(stream)





                

            

