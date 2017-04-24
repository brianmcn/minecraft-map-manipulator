module Recipes

type MS = // MinecraftString
    | MC of string // e.g. "golden_axe" which will become "minecraft:golden_axe"
    | PATH of string
    member private this.Validate() =
        let v (s:string) = if s.ToLowerInvariant() <> s then failwith "path strings must be all lowercase!"
        match this with 
        | MC s -> v s
        | PATH s -> v s
    override this.ToString() =
        this.Validate()
        match this with 
        | MC s -> sprintf "minecraft:%s" s
        | PATH s -> s

type PatternKey =
    | PatternKey of string[] *          // e.g. [ "XX"; "X#"; " #" ]
                    (char*MS)[]         // e.g. [ '#',MS"stick"; 'X',MS"gold_ingot" ]
type Recipe =
    | ShapedCrafting of PatternKey * (*result*)MS
    | ShapelessCrafting of (*ingredients*)MS[] * (*result*)MS
    member this.Write(w:System.IO.TextWriter) =
        let ICH s = s |> Seq.toArray |> (fun a -> Array.init a.Length (fun i -> a.[i], if i<>a.Length-1 then "," else "")) // interspersed comma helper
        w.WriteLine("""{""")
        match this with
        | ShapelessCrafting(ingreds,result) ->
            w.WriteLine("""    "type": "crafting_shapeless",""")
            w.WriteLine("""    "ingredients": [""")
            for i,c in ICH ingreds do
                w.WriteLine("""        { "item": """+i.ToString()+""" }"""+c)
            w.WriteLine("""    ],""")
            w.WriteLine("""    "result": { "item": """+result.ToString()+""" }""")
        | ShapedCrafting(PatternKey(pattern,key),result) ->
            w.WriteLine("""    "type": "crafting_shaped",""")
            w.WriteLine("""    "pattern": [""")
            for p,c in ICH pattern do
                w.WriteLine(sprintf """        "%s"%s""" p c)
            w.WriteLine("""    ],""")
            w.WriteLine("""    "key": {""")
            for (x,s),c in ICH key do
                w.WriteLine(sprintf """        "%c": { "item": %s }%s""" x (s.ToString()) c)
            w.WriteLine("""    },""")
            w.WriteLine("""    "result": { "item": """+result.ToString()+""" }""")
        w.WriteLine("""}""")

let writeRecipes(recipes, worldSaveFolder) =
    for (name:string, recipe:Recipe) in recipes do
        if name<>name.ToLowerInvariant() then
            failwithf "bad recipe name has uppercase: %s" name
        let pathBits = name.Split [|':';'/'|]
        let wslt = System.IO.Path.Combine [| yield worldSaveFolder; yield "data"; yield "recipes" |]
        let filename = System.IO.Path.Combine [| yield wslt; yield! pathBits |]
        let filename = filename + ".json"
        if System.IO.File.Exists(filename) then
            System.IO.File.Delete(filename)
        System.IO.Directory.CreateDirectory( System.IO.Path.GetDirectoryName(filename) ) |> ignore
        use stream = new System.IO.StreamWriter( System.IO.File.OpenWrite(filename) )
        recipe.Write(stream)
