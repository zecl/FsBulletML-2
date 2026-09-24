namespace FsBulletML2.LanguageService

open System
open System.Collections.Generic
open System.Reflection
open System.Text
open Microsoft.FSharp.Reflection
open FsBulletML2

/// 属性 1 つ。`Values` が空なら自由記述（label など）。
type VocabAttr =
    {
        Name: string
        Values: string[]
        /// 書かなかったときに走る値。`[<BulletmlDefault>]` から。
        /// 1 個 に絞らず並びで持つ。型で潰すと「先頭を採る」で壊れが消える。
        Defaults: string[]
        /// この属性の `<!ATTLIST ...>` 行。表を持たない —— 型から組む
        Dtd: string
        /// hover に出す散文（`Spec.fs`）
        Spec: string
        /// 値ごとの散文。`Values` と同じ綴りを鍵に持つ
        ValueSpecs: (string * string)[]
    }

/// 要素 1 つ。`Children` は「この中に置ける要素」、`Text` は #PCDATA を取るか。
type VocabElement =
    {
        Name: string
        Children: string[]
        Attrs: VocabAttr[]
        Text: bool
        /// この要素の `<!ELEMENT ...>` 行。腕の並びから組む
        Dtd: string
        /// hover に出す散文（`Spec.fs`）
        Spec: string
    }

/// BulletML の語彙。正本は `Core/DTD.fs` の型だけ。表を持たない。
/// 腕が消えて空になったら、呼ぶ側が赤にする。
module Vocabulary =

    /// 散文の引き先。無ければ空。足りないことは門が見る。
    /// ここで落とすと、表を直す前に何も動かなくなる。
    let private specOf (table: (string * string) list) =
        let d = dict table

        fun (key: string) ->
            match d.TryGetValue key with
            | true, v -> v
            | _ -> ""

    let private elementSpec = specOf Spec.elements
    let private attributeSpec = specOf Spec.attributes
    let private attrValueSpec = specOf Spec.attrValues

    let private camel (s: string) =
        if String.IsNullOrEmpty s then
            s
        else
            string (Char.ToLowerInvariant s.[0]) + s.Substring 1

    /// option / list を剥がす
    let rec private unwrap (t: Type) =
        if t.IsGenericType then
            let d = t.GetGenericTypeDefinition()

            if d = typedefof<option<_>> || d = typedefof<list<_>> then
                unwrap (t.GetGenericArguments().[0])
            else
                t
        else
            t

    /// `Params = string list`。DTD では `<param>` の並び
    let private isParams (t: Type) =
        t.IsGenericType
        && t.GetGenericTypeDefinition() = typedefof<list<_>>
        && t.GetGenericArguments().[0] = typeof<string>

    let private isAttrs (t: Type) =
        FSharpType.IsRecord t && t.Name.EndsWith "Attrs"

    /// 全部の腕が同じ語で始まっていたら、その語を落とす。
    /// 切れ目は大文字。語の途中で切ると `ertical` のような字が出る。
    let private stripCommonPrefix (names: string[]) =
        if names.Length < 2 then
            names
        else
            let shortest = names |> Array.map String.length |> Array.min
            let mutable n = 0

            while n < shortest - 1 && names |> Array.forall (fun s -> s.[n] = names.[0].[n]) do
                n <- n + 1

            let mutable cut = n

            while cut > 0 && not (Char.IsUpper names.[0].[cut]) do
                cut <- cut - 1

            if
                cut > 0
                && names |> Array.forall (fun s -> s.Length > cut && Char.IsUpper s.[cut])
            then
                names |> Array.map (fun s -> s.Substring cut)
            else
                names

    /// option / list を 1 段 だけ剥がして、多重度の印を返す。
    /// `unwrap` で全部 剥がすな。多重度が消える。
    let private occurs (t: Type) =
        if t.IsGenericType then
            let d = t.GetGenericTypeDefinition()

            if d = typedefof<option<_>> then
                Some("?", t.GetGenericArguments().[0])
            elif d = typedefof<list<_>> then
                Some("*", t.GetGenericArguments().[0])
            else
                None
        else
            None

    /// 「そこに置ける物」1 つ ぶんの字。多腕 DU は選択に、単腕 DU は要素名に
    let private slot (t: Type) =
        if FSharpType.IsUnion t then
            let cs = FSharpType.GetUnionCases t

            if cs.Length > 1 then
                "(" + (cs |> Array.map (fun c -> camel c.Name) |> String.concat " | ") + ")"
            else
                camel t.Name
        else
            camel t.Name

    /// `<!ELEMENT ...>` の中身。腕の並びが順序、option / list が多重度。
    let private contentModel (fields: Type[]) =
        let parts =
            [
                for f in fields do
                    if isParams f then
                        yield "param*"
                    else
                        let occ, inner =
                            match occurs f with
                            | Some(o, i) -> o, i
                            | None -> "", f

                        if isAttrs inner then ()
                        elif inner = typeof<Expr.NumExpr> then yield "#PCDATA"
                        else yield slot inner + occ
            ]

        match parts with
        | [] -> "EMPTY"
        // 1 つ だけで、それ自体が括弧で括られた組なら外側を足さない
        // （`((bullet | fire | action)*)` にしない）
        | [ p ] when p.StartsWith("(", StringComparison.Ordinal) -> p
        | ps -> "(" + String.concat ", " ps + ")"

    let private attrsOf (elementName: string) (t: Type) =
        FSharpType.GetRecordFields t
        |> Array.map (fun p ->
            // field 名は要素名で始まる（directionType / actionRefLabel）。剥がす
            let bare =
                if p.Name.StartsWith(elementName, StringComparison.OrdinalIgnoreCase) then
                    p.Name.Substring elementName.Length
                else
                    p.Name

            let vt = unwrap p.PropertyType

            let cases =
                if FSharpType.IsUnion vt then
                    FSharpType.GetUnionCases vt
                else
                    [||]
            // 腕が 1 本 の DU にも札を読む。読まないと既定の門が当たる先を持たない。
            let values =
                if cases.Length > 1 then
                    cases |> Array.map (fun c -> c.Name) |> stripCommonPrefix |> Array.map camel
                else
                    [||]
            // 既定は腕の位置で引く。名前をもう一度変換するな。
            let defaults =
                cases
                |> Array.mapi (fun i c -> i, c)
                |> Array.filter (fun (_, c) -> (c.GetCustomAttributes typeof<BulletmlDefaultAttribute>).Length > 0)
                |> Array.map (fun (i, c) -> if values.Length > 0 then values.[i] else camel c.Name)

            let name = camel bare
            // option でない field は、書かないと読めない属性（actionRef/@label）
            let required =
                not (
                    p.PropertyType.IsGenericType
                    && p.PropertyType.GetGenericTypeDefinition() = typedefof<option<_>>
                )

            let decl =
                if values.Length > 0 then
                    "(" + String.concat "|" values + ")"
                else
                    "CDATA"

            let def =
                match Array.tryHead defaults with
                | Some d -> "\"" + d + "\""
                | None -> if required then "#REQUIRED" else "#IMPLIED"

            {
                Name = name
                Values = values
                Defaults = defaults
                Dtd = sprintf "<!ATTLIST %s %s %s %s>" elementName name decl def
                Spec = attributeSpec (sprintf "%s/@%s" elementName name)
                ValueSpecs =
                    values
                    |> Array.map (fun v -> v, attrValueSpec (sprintf "%s/@%s=%s" elementName name v))
            })

    /// 木を歩いて、腕ごとに要素名から持ち物を集める。
    /// 同じ腕が 2 か所 に出ても持ち物は同じ。上書きしてよい。
    let rec private collect (t: Type) (seen: HashSet<Type>) (acc: Dictionary<string, Type[]>) =
        if FSharpType.IsUnion t && seen.Add t then
            for c in FSharpType.GetUnionCases t do
                let fields = c.GetFields() |> Array.map (fun p -> p.PropertyType)
                acc.[camel c.Name] <- fields

                for f in fields do
                    if not (isParams f) then
                        collect (unwrap f) seen acc

    let private describe (name: string) (fields: Type[]) =
        let children = ResizeArray<string>()
        let attrs = ResizeArray<VocabAttr>()
        let mutable text = false

        for f in fields do
            if isParams f then
                children.Add "param"
            else
                let u = unwrap f

                if u = typeof<Expr.NumExpr> then
                    text <- true
                elif isAttrs u then
                    attrs.AddRange(attrsOf name u)
                elif FSharpType.IsUnion u then
                    for c in FSharpType.GetUnionCases u do
                        children.Add(camel c.Name)

        {
            Name = name
            Children = children |> Seq.distinct |> Seq.toArray
            Attrs = attrs.ToArray()
            Text = text
            Dtd = sprintf "<!ELEMENT %s %s>" name (contentModel fields)
            Spec = elementSpec name
        }

    /// 語彙。空なら呼ぶ側が赤にすること —— reflection が効いていない印
    let elements: VocabElement[] =
        let acc = Dictionary<string, Type[]>()
        collect typeof<Bulletml> (HashSet<Type>()) acc

        acc
        |> Seq.map (fun kv -> describe kv.Key kv.Value)
        // `param` は腕を持たないので木から出てこない。中身を取る要素として足す。
        |> Seq.append
            [
                {
                    Name = "param"
                    Children = [||]
                    Attrs = [||]
                    Text = true
                    Dtd = "<!ELEMENT param (#PCDATA)>"
                    Spec = elementSpec "param"
                }
            ]
        |> Seq.sortBy (fun e -> e.Name)
        |> Seq.toArray

    /// 式の中で使える字。DU からは引けない。`$rand` は腕の名前 `Rand` とは別。
    /// 短い表にする。綴りが動いたら試験が赤になる。
    let expressions = [| "$rand"; "$rank" |]

    // 入れ物は要素ではない。綴りを要素名と重ねるな。器へ流すと線を越える。
    [<Literal>]
    let private SlotOuter = "outer"

    /// 根の `{ }` の中
    [<Literal>]
    let private SlotRoot = "root"

    [<Literal>]
    let private SlotAction = "acts"

    [<Literal>]
    let private SlotFire = "shot"

    [<Literal>]
    let private SlotBullet = "ammo"

    [<Literal>]
    let private SlotAccel = "push"

    /// 型の名前から入れ物。表はこの 1 か所 だけ。
    /// builder の置ける先は `'T`。開く側と置く側を取り違えるな。
    let private slotOfType (name: string) =
        match name with
        | "BulletmlElm" -> Some SlotRoot
        | "Action" -> Some SlotAction
        | "ActionElm" -> Some SlotBullet
        | "BulletElm" -> Some SlotFire
        | "Horizontal"
        | "Vertical" -> Some SlotAccel
        | _ -> None

    /// builder の型 -> それが開く `{ }` の種類
    let private slotOfBuilder (name: string) =
        if name.StartsWith "ActionBuilder" then Some SlotAction
        elif name.StartsWith "FireBuilder" then Some SlotFire
        elif name.StartsWith "BulletBuilder" then Some SlotBullet
        elif name.StartsWith "AccelBuilder" then Some SlotAccel
        elif name.StartsWith "BulletmlBuilder" then Some SlotRoot
        else None

    let rec private resultOf (t: Type) : Type =
        if t.Name.StartsWith "FSharpFunc" then
            resultOf (t.GetGenericArguments().[1])
        else
            t

    /// 根から走る定義の名前の頭。綴りは Core の `Api.fs` が真。
    /// 器へ書き写すな。Core を変えたときに黙って割れる。
    [<Literal>]
    let topPrefix = "top"

    /// CE の名前がどこに置けて何を開くか。表ではなく `Dsl` から引く。
    /// `ActionBuilder` の中身は `[<CustomOperation>]` では引けない。公開 `let` も舐める。
    let cePlaces: (string * string * string)[] =
        let asm = typeof<FsBulletML2.Dsl.BulletmlBuilder>.Assembly

        let dslModule =
            asm.GetTypes() |> Array.find (fun t -> t.FullName = "FsBulletML2.Dsl")
        // module の公開 let。戻り値の型が置ける先を決める
        let lets =
            dslModule.GetMembers(BindingFlags.Public ||| BindingFlags.Static)
            |> Array.choose (fun m ->
                match m with
                | :? MethodInfo as mi when not mi.IsSpecialName -> Some(mi.Name, mi.ReturnType)
                | :? PropertyInfo as pi -> Some(pi.Name, pi.PropertyType)
                | _ -> None)
            |> Array.choose (fun (name, t) ->
                let r = resultOf t
                let opens = slotOfBuilder r.Name

                let place =
                    match opens with
                    // 根の builder はまだ `{ }` の中に居ない。 いちばん外に置く
                    | Some s when s = SlotRoot -> Some SlotOuter
                    // accel は `{ }` を開くが、それ自身は action の中に置く
                    | Some s when s = SlotAccel -> Some SlotAction
                    // ほかの builder は `'T` の中に置ける。開く側と置く側は別。
                    | Some _ when r.IsGenericType ->
                        r.GetGenericArguments()
                        |> Array.tryHead
                        |> Option.bind (fun a -> slotOfType a.Name)
                    | Some _ -> None
                    | None -> slotOfType r.Name

                match place with
                | None -> None
                | Some p -> Some(name, p, defaultArg opens ""))
        // builder の CustomOperation。置ける先はその builder が開く `{ }`
        let ops =
            asm.GetTypes()
            |> Array.collect (fun t ->
                match slotOfBuilder t.Name with
                | None -> [||]
                | Some slot ->
                    t.GetMethods(BindingFlags.Public ||| BindingFlags.Instance)
                    |> Array.choose (fun m ->
                        m.GetCustomAttributes(typeof<CustomOperationAttribute>, false)
                        |> Array.tryHead
                        |> Option.map (fun a -> (a :?> CustomOperationAttribute).Name, slot, "")))

        Array.append lets ops
        |> Array.distinctBy (fun (n, p, o) -> n, p, o)
        |> Array.sortBy (fun (n, _, _) -> n)

    let private escape (s: string) =
        let sb = StringBuilder()

        for ch in s do
            match ch with
            | '"' -> sb.Append "\\\"" |> ignore
            | '\\' -> sb.Append "\\\\" |> ignore
            | c when c < ' ' -> sb.AppendFormat("\\u{0:x4}", int c) |> ignore
            | c -> sb.Append c |> ignore

        sb.ToString()

    /// 起動時に 1 回 だけ JS へ渡す形。毎キー呼ばない。
    /// 手で組む。serializer に任せると版で形が動く。
    let toJson () =
        let sb = StringBuilder()

        let str (s: string) =
            sb.Append('"').Append(escape s).Append('"') |> ignore

        let arr (xs: string[]) =
            sb.Append '[' |> ignore

            xs
            |> Array.iteri (fun i x ->
                if i > 0 then
                    sb.Append ',' |> ignore

                str x)

            sb.Append ']' |> ignore

        sb.Append "{\"elements\":[" |> ignore

        elements
        |> Array.iteri (fun i e ->
            if i > 0 then
                sb.Append ',' |> ignore

            sb.Append "{\"name\":" |> ignore
            str e.Name
            sb.Append ",\"text\":" |> ignore
            sb.Append(if e.Text then "true" else "false") |> ignore
            sb.Append ",\"children\":" |> ignore
            arr e.Children
            sb.Append ",\"dtd\":" |> ignore
            str e.Dtd
            sb.Append ",\"spec\":" |> ignore
            str e.Spec
            sb.Append ",\"attrs\":[" |> ignore

            e.Attrs
            |> Array.iteri (fun j a ->
                if j > 0 then
                    sb.Append ',' |> ignore

                sb.Append "{\"name\":" |> ignore
                str a.Name
                sb.Append ",\"values\":" |> ignore
                arr a.Values
                sb.Append ",\"defaults\":" |> ignore
                arr a.Defaults
                sb.Append ",\"dtd\":" |> ignore
                str a.Dtd
                sb.Append ",\"spec\":" |> ignore
                str a.Spec
                sb.Append ",\"valueSpecs\":[" |> ignore

                a.ValueSpecs
                |> Array.iteri (fun k (v, s) ->
                    if k > 0 then
                        sb.Append ',' |> ignore

                    sb.Append "{\"value\":" |> ignore
                    str v
                    sb.Append ",\"spec\":" |> ignore
                    str s
                    sb.Append '}' |> ignore)

                sb.Append "]}" |> ignore)

            sb.Append "]}" |> ignore)

        sb.Append "],\"expressions\":" |> ignore
        arr expressions
        // F# の CE の名前。 ここは reflection ではなく手で書いた表（`Spec.ce`）。
        // 名前が `Dsl` を過不足なく覆うことは `SpecCoverage` が見ている
        sb.Append ",\"ce\":[" |> ignore

        Spec.ce
        |> List.iteri (fun i (name, element, attr, value) ->
            if i > 0 then
                sb.Append ',' |> ignore

            sb.Append "{\"name\":" |> ignore
            str name
            sb.Append ",\"element\":" |> ignore
            str element
            sb.Append ",\"attr\":" |> ignore
            str attr
            sb.Append ",\"value\":" |> ignore
            str value
            sb.Append '}' |> ignore)
        // label は `ce` と別の表。作る要素と、名前が付く要素は違う。
        sb.Append "],\"ceLabels\":[" |> ignore

        Spec.ceLabels
        |> List.iteri (fun i (name, element, labelArg, fixedName, root) ->
            if i > 0 then
                sb.Append ',' |> ignore

            sb.Append "{\"name\":" |> ignore
            str name
            sb.Append ",\"element\":" |> ignore
            str element
            sb.Append ",\"labelArg\":" |> ignore
            sb.Append(string labelArg) |> ignore
            sb.Append ",\"fixed\":" |> ignore
            str fixedName
            sb.Append ",\"root\":" |> ignore
            sb.Append(if root then "true" else "false") |> ignore
            sb.Append '}' |> ignore)
        // どこに置けて、何を開くか。 表ではなく `Dsl` から reflection で引いたもの
        sb.Append "],\"cePlaces\":[" |> ignore

        cePlaces
        |> Array.iteri (fun i (name, place, opens) ->
            if i > 0 then
                sb.Append ',' |> ignore

            sb.Append "{\"name\":" |> ignore
            str name
            sb.Append ",\"in\":" |> ignore
            str place
            sb.Append ",\"opens\":" |> ignore
            str opens
            sb.Append '}' |> ignore)
        // 根の名前の頭。綴りは Core。器に書き写すな。
        sb.Append "],\"topPrefix\":" |> ignore
        str topPrefix
        // 雛形。骨は要素名の木。器に書かず、語彙と同じ経路で渡す。
        sb.Append ",\"frames\":[" |> ignore

        let rec writeFrame (f: SourceLanguage.Frame) =
            sb.Append "{\"element\":" |> ignore
            str f.Element
            sb.Append ",\"text\":" |> ignore
            str f.Text
            sb.Append ",\"attrs\":[" |> ignore

            f.Attrs
            |> List.iteri (fun i (k, v) ->
                if i > 0 then
                    sb.Append ',' |> ignore

                sb.Append "{\"name\":" |> ignore
                str k
                sb.Append ",\"value\":" |> ignore
                str v
                sb.Append '}' |> ignore)

            sb.Append "],\"children\":[" |> ignore

            f.Children
            |> List.iteri (fun i c ->
                if i > 0 then
                    sb.Append ',' |> ignore

                writeFrame c)

            sb.Append "]}" |> ignore

        Frames.all
        |> List.iteri (fun i s ->
            if i > 0 then
                sb.Append ',' |> ignore

            sb.Append "{\"label\":" |> ignore
            str s.Label
            sb.Append ",\"detail\":" |> ignore
            str s.Detail
            sb.Append ",\"in\":" |> ignore
            arr (List.toArray s.In)
            sb.Append ",\"frame\":" |> ignore
            writeFrame s.Frame
            sb.Append '}' |> ignore)

        sb.Append "]}" |> ignore
        sb.ToString()
