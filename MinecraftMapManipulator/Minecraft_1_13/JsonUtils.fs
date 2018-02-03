module JsonUtils

type JsonValue =
    | JsonString of string
    | JsonNumber of string
    | JsonObject of (string * JsonValue)[]
    | JsonArray of JsonValue[]
    | JsonBool of bool
    | JsonNull
    static member Parse(s:string) =
        let parseString(cs:char list) =
            if cs.Head <> '"' then failwith "expected string"
            let sb = System.Text.StringBuilder()
            let mutable rest = cs.Tail
            let mutable finished = false
            while not finished do
                match rest with
                | [] -> failwith "expected \", but found end of string"
                | '"'::coda -> 
                    finished <- true
                    rest <- coda
                | '\\'::'"'::coda ->
                    sb.Append('"') |> ignore
                    rest <- coda
                | '\\'::'\\'::coda ->
                    sb.Append('\\') |> ignore
                    rest <- coda
                | '\\'::_ -> failwith "this parser does not handle string contents with anything after \\ except \\ or \", sorry"
                | c::coda ->
                    sb.Append(c) |> ignore
                    rest <- coda
            JsonString(sb.ToString()), rest
        let rec skipws(cs:char list) =
            match cs with
            | [] -> []
            | c::rest when System.Char.IsWhiteSpace(c) -> skipws(rest)
            | _ -> cs
        let rec parse(cs:char list) =
            match cs with
            | [] -> failwith "expected JsonValue, but found end of string"
            | '"'::_ -> parseString(cs)
            | c::coda when (System.Char.IsDigit(c) || c='-') ->
                let sb = System.Text.StringBuilder()
                sb.Append(c) |> ignore
                let mutable rest = coda
                while System.Char.IsDigit(rest.Head) do
                    sb.Append(rest.Head) |> ignore
                    rest <- rest.Tail 
                if rest.Head = '.' then
                    sb.Append(rest.Head) |> ignore
                    rest <- rest.Tail 
                    while System.Char.IsDigit(rest.Head) do
                        sb.Append(rest.Head) |> ignore
                        rest <- rest.Tail
                if rest.Head = 'e' || rest.Head = 'E' then
                    failwith "sorry, exponents in numbers not handled by this parser"
                JsonNumber(sb.ToString()), rest
            | '{'::coda ->
                let mutable rest = skipws(coda)
                let pairs = ResizeArray()
                while rest.Head <> '}' do
                    let key,coda = parseString(rest)
                    let key = match key with | JsonString(s) -> s | _ -> failwith "impossible string parse"
                    let coda = skipws(coda)
                    if coda.Head <> ':' then failwith "expected ':'"
                    let coda = skipws(coda.Tail)
                    let value,coda = parse(coda)
                    pairs.Add(key,value)
                    rest <- skipws(coda)
                    if rest.Head = ',' then // this allows extra trailing comma
                        rest <- skipws(rest.Tail)
                JsonObject(pairs.ToArray()), rest.Tail
            | '['::coda ->
                let mutable rest = skipws(coda)
                let values = ResizeArray()
                while rest.Head <> ']' do
                    let value,coda = parse(rest)
                    values.Add(value)
                    rest <- skipws(coda)
                    if rest.Head = ',' then // this allows extra trailing comma
                        rest <- skipws(rest.Tail)
                JsonArray(values.ToArray()), rest.Tail
            | 't'::'r'::'u'::'e'::coda -> JsonBool(true), coda
            | 'f'::'a'::'l'::'s'::'e'::coda -> JsonBool(false), coda
            | 'n'::'u'::'l'::'l'::coda -> JsonNull, coda
            | c::coda when System.Char.IsWhiteSpace(c) -> parse(skipws(coda))
            | c::_ -> failwithf "unexpected character '%c'" c
        parse(s |> Seq.toList)
    member this.ToPrettyString(spacesPerIndent,width) =
        let escape(s:string) = 
            if s.StartsWith("minecraft:") && s <> "minecraft:set_count" then  // a hack to make more loot tables 'line up'
                let r = s.Replace("\\","\\\\").Replace("\"","\\\"")
                if r.Length < 32 then r + String.replicate (32 - r.Length) " " else r
            else
                s.Replace("\\","\\\\").Replace("\"","\\\"")
        let rec maxWidth(v) =
            match v with
            | JsonString(s) -> escape(s).Length+2
            | JsonNumber(s) -> s.Length 
            | JsonObject(pairs) ->
                match pairs with
                | [||] -> 2
                | [|k,v|] -> 2 + escape(k).Length+2 + 3 + maxWidth(v) + 2
                | _ -> 
                    let mutable r = 4
                    for k,v in pairs do
                        r <- r + escape(k).Length+2 + 3 + maxWidth(v)
                    r <- r + (pairs.Length-1)*2
                    r
            | JsonArray(values) ->
                match values with
                | [||] -> 2
                | [|v|] -> 2 + maxWidth(v) + 2
                | _ -> 
                    let mutable r = 4
                    for v in values do
                        r <- r + maxWidth(v)
                    r <- r + (values.Length-1)*2
                    r
            | JsonBool(true) -> 4
            | JsonBool(false) -> 5
            | JsonNull -> 4
        let rec print(v:JsonValue, sb:System.Text.StringBuilder, curCol, numIndents) =
            match v with
            | JsonBool(true) -> 
                sb.Append("true") |> ignore
                curCol+4
            | JsonBool(false) ->
                sb.Append("false") |> ignore
                curCol+5
            | JsonNull ->
                sb.Append("null") |> ignore
                curCol+4
            | JsonNumber(s) -> 
                sb.Append(s) |> ignore
                curCol+s.Length
            | JsonString(s) ->
                sb.Append("\""+escape(s)+"\"") |> ignore
                curCol+escape(s).Length+2
            | JsonArray(values) ->
                match values with
                | [||] -> 
                    sb.Append("[]") |> ignore
                    curCol+2
                | [|x|] ->
                    sb.Append("[ ") |> ignore
                    let curCol = print(x, sb, curCol+2, numIndents)
                    sb.Append(" ]") |> ignore
                    curCol+2
                | _ -> 
                    if maxWidth(v) + curCol >= width then // use newlines
                        sb.Append("[") |> ignore
                        let numSpaces = (numIndents+1)*spacesPerIndent
                        if curCol+1 > numSpaces then
                            sb.Append("\r\n") |> ignore
                            sb.Append(String.replicate numSpaces " ") |> ignore
                        else
                            sb.Append(String.replicate (numSpaces-curCol-1) " ") |> ignore
                        let mutable cc = 0
                        for i = 0 to values.Length-1 do
                            if i <> 0 then
                                sb.Append(String.replicate numSpaces " ") |> ignore
                            cc <- print(values.[i], sb, numSpaces, numIndents+1)
                            if i < values.Length-1 then
                                sb.Append(",\r\n") |> ignore
                        sb.Append(" ]") |> ignore
                        cc+2
                    else
                        sb.Append("[ ") |> ignore
                        let mutable cc = curCol+2
                        for i = 0 to values.Length-1 do
                            cc <- print(values.[i], sb, cc, numIndents)
                            if i < values.Length-1 then
                                sb.Append(", ") |> ignore
                                cc <- cc + 2
                        sb.Append(" ]") |> ignore
                        cc+2
            | JsonObject(pairs) ->
                match pairs with
                | [||] -> 
                    sb.Append("{}") |> ignore
                    curCol+2
                | [|k,v|] ->
                    sb.Append("{ ") |> ignore
                    let curCol = print(JsonString(k), sb, curCol+2, numIndents)
                    sb.Append(" : ") |> ignore
                    let curCol = print(v, sb, curCol+3, numIndents)
                    sb.Append(" }") |> ignore
                    curCol+2
                | _ -> 
                    if maxWidth(v) + curCol >= width then // use newlines
                        sb.Append("{") |> ignore
                        let numSpaces = (numIndents+1)*spacesPerIndent
                        if curCol+1 > numSpaces then
                            sb.Append("\r\n") |> ignore
                            sb.Append(String.replicate numSpaces " ") |> ignore
                        else
                            sb.Append(String.replicate (numSpaces-curCol-1) " ") |> ignore
                        let mutable cc = 0
                        for i = 0 to pairs.Length-1 do
                            if i <> 0 then
                                sb.Append(String.replicate numSpaces " ") |> ignore
                            cc <- print(JsonString(fst pairs.[i]), sb, numSpaces, numIndents+1)
                            sb.Append(" : ") |> ignore
                            cc <- print(snd pairs.[i], sb, cc+3, numIndents+1)
                            if i < pairs.Length-1 then
                                sb.Append(",\r\n") |> ignore
                        sb.Append(" }") |> ignore
                        cc+2
                    else
                        sb.Append("{ ") |> ignore
                        let mutable cc = curCol+2
                        for i = 0 to pairs.Length-1 do
                            cc <- print(JsonString(fst pairs.[i]), sb, cc, numIndents+1)
                            sb.Append(" : ") |> ignore
                            cc <- print(snd pairs.[i], sb, cc+3, numIndents+1)
                            if i < pairs.Length-1 then
                                sb.Append(", ") |> ignore
                                cc <- cc + 2
                        sb.Append(" }") |> ignore
                        cc+2
        let sb = System.Text.StringBuilder()
        print(this, sb, 0, 0) |> ignore
        sb.ToString()

