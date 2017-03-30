module Advancements

open Recipes // for MS

type Condition =
    | NumInventorySlotsFull of int
    | HasItems of MS[]
    | HasRecipe of MS
type Criterion =
    | Criterion of (*name*) string * (*trigger*) MS * Condition[]
type Advancement =
    | Reward of (*recipes*) MS[] * Criterion[] * (*requirements in DNF*) string[][]
    | Display of (*icon*) MS * (*title*) string * (*parent*) MS * Criterion[]
    member this.Write(w:System.IO.TextWriter) =
        let ICH s = s |> Seq.toArray |> (fun a -> Array.init a.Length (fun i -> a.[i], if i<>a.Length-1 then ", " else "")) // interspersed comma helper
        let intersperse s = 
            let sb = new System.Text.StringBuilder()
            for x,c in ICH s do
                sb.Append(x.ToString()+c)  |> ignore
            sb.ToString()
        let doCriteria criteria endComma=
            w.WriteLine("""    "criteria": {""")
            for Criterion(name,trigger,conditions),c in ICH criteria do
                w.WriteLine(sprintf """        "%s": {""" name)
                w.WriteLine(sprintf """            "trigger": %s""" (trigger.ToString()))
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
                        w.WriteLine(sprintf """                "recipe": %s%s""" (r.ToString()) c)
                w.WriteLine("""            }""")
                w.WriteLine(sprintf """        }%s""" c)
            w.WriteLine(sprintf """    }%s""" (if endComma then "," else ""))
        w.WriteLine("""{""")
        match this with
        | Reward(recipes,criteria,reqsDNF) ->
            w.WriteLine(sprintf """    "rewards": { "recipes": [%s] },""" (intersperse recipes))
            doCriteria criteria true
            w.WriteLine(sprintf """    "requirements": [%s]""" (
                let sb = new System.Text.StringBuilder()
                for a,c in ICH reqsDNF do
                    sb.Append("[") |> ignore
                    for s,c in ICH a do
                        sb.Append("\""+s+"\""+c)  |> ignore
                    sb.Append("]").Append(c)  |> ignore
                sb.ToString()
                ))
        | Display(icon,title,parent,criteria) ->
            w.WriteLine("""    "display": {""")
            w.WriteLine(sprintf """        "icon": %s""" (icon.ToString()))
            w.WriteLine(sprintf """        "title": %s""" (sprintf "\"%s\"" title))
            w.WriteLine("""    },""")
            w.WriteLine(sprintf """    "parent": %s,""" (parent.ToString()))
            doCriteria criteria false
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





                

            

