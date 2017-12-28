module Recipes

type MS = // MinecraftString
    | MC of string // e.g. "golden_axe" which will become "minecraft:golden_axe"
    | PATH of string
    member private this.Validate() =
        let v (s:string) = if s.ToLowerInvariant() <> s then failwith "path strings must be all lowercase!"
        match this with 
        | MC s -> v s
        | PATH s -> v s
    member this.STR = this.ToString()
    override this.ToString() =
        this.Validate()
        match this with 
        | MC s -> sprintf "minecraft:%s" s
        | PATH s -> s

type PatternKey =
    | PatternKey of string[] *          // e.g. [ "XX"; "X#"; " #" ]
                    (char*MS)[]         // e.g. [ '#',MS"stick"; 'X',MS"gold_ingot" ]
type Recipe =
    | ShapedCrafting of PatternKey * (*result*)MS * int
    | ShapelessCrafting of (*ingredients*)MS[] * (*result*)MS * int
    member this.Write(w:System.IO.TextWriter) =
        let ICH s = s |> Seq.toArray |> (fun a -> Array.init a.Length (fun i -> a.[i], if i<>a.Length-1 then "," else "")) // interspersed comma helper
        w.WriteLine("""{""")
        match this with
        | ShapelessCrafting(ingreds,result,resultCount) ->
            w.WriteLine("""    "type": "crafting_shapeless",""")
            w.WriteLine("""    "ingredients": [""")
            for i,c in ICH ingreds do
                w.WriteLine("""        { "item": """+i.ToString()+""" }"""+c)
            w.WriteLine("""    ],""")
            w.WriteLine("""    "result": { "item": """+result.ToString()+""", "count": """+resultCount.ToString()+""" }""")
        | ShapedCrafting(PatternKey(pattern,key),result,resultCount) ->
            w.WriteLine("""    "type": "crafting_shaped",""")
            w.WriteLine("""    "pattern": [""")
            for p,c in ICH pattern do
                w.WriteLine(sprintf """        "%s"%s""" p c)
            w.WriteLine("""    ],""")
            w.WriteLine("""    "key": {""")
            for (x,s),c in ICH key do
                w.WriteLine(sprintf """        "%c": { "item": %s }%s""" x (s.ToString()) c)
            w.WriteLine("""    },""")
            w.WriteLine("""    "result": { "item": """+result.ToString()+""", "count": """+resultCount.ToString()+""" }""")
        w.WriteLine("""}""")

let writeRecipes(recipes, worldSaveFolder, datapackName, ns) =
    // TODO
    ()
