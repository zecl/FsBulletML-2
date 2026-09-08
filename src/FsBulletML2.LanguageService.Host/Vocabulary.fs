namespace FsBulletML2.LanguageService

open System
open System.Collections.Generic
open System.Reflection
open System.Text
open Microsoft.FSharp.Reflection
open FsBulletML2

/// 属性 1 つ。`Values` が空なら自由記述（label など）。
type VocabAttr =
  { Name: string
    Values: string[]
    /// 書かなかったときに走る値。`Core` の腕に付いた `[<BulletmlDefault>]` から。
    ///
    /// **1 個 に絞らず並びで持つ。** reflection は 2 個 でも返せるので、
    /// 型で 1 個 に潰すとその壊れが「先頭を採る」で消える。
    /// `Values` が空でないとき 1 個 であることは門が見る
    Defaults: string[]
    /// この属性の `<!ATTLIST ...>` 行。**表を持たない** —— 型から組む
    Dtd: string
    /// hover に出す散文（`Spec.fs`）
    Spec: string
    /// 値ごとの散文。`Values` と同じ綴りを鍵に持つ
    ValueSpecs: (string * string)[] }

/// 要素 1 つ。`Children` は「この中に置ける要素」、`Text` は #PCDATA を取るか。
type VocabElement =
  { Name: string
    Children: string[]
    Attrs: VocabAttr[]
    Text: bool
    /// この要素の `<!ELEMENT ...>` 行。腕の並びから組む
    Dtd: string
    /// hover に出す散文（`Spec.fs`）
    Spec: string }

/// BulletML の語彙。**正本は `Core/DTD.fs` の型だけ。**
///
/// あちらは DTD をそのまま DU にしたもので、「どの要素の中に何が置けるか」は
/// 腕の並びそのもの。ここはそれを reflection で読むだけで、**表を持たない** ——
/// 表を書くと、DTD 側を直したとき表だけが古びる（しかも赤くならない）。
///
///     BulletmlElm = Bullet | Fire | Action          <bulletml> の子
///     Action      = ChangeDirection | Accel | ...   <action> の子
///     BulletElm   = Bullet | BulletRef              <fire> が取る
///     ActionElm   = Action | ActionRef              <repeat> が取る
///
/// **この木のすべての腕が、そのまま 1 要素になる。** 多腕（置ける物の束）と
/// 単腕（要素そのもの）を区別しなくてよいのは、腕の名前が要素の名前だから。
///
/// 名前の付け方は 3 つ。**どれも当てずっぽうなので、突き合わせの試験で見る**
/// （`FsBulletML2.Dsl.Tests`。同梱 176 本 を書き出して、出てくる名前が
/// 全部 ここに在るかを当てる）。
///
///   要素   腕の名前の頭を小文字に（`ChangeDirection` -> `changeDirection`）
///   属性   レコードの field 名から要素名を剥がす（`directionType` -> `type`）
///   属性値 腕の名前を小文字に。**全部の腕が同じ語で始まるならその語を落とす**
///          （`BulletNone` / `BulletVertical` -> `none` / `vertical`）
///
/// reflection なので `PublishTrimmed` は false のまま。true にすると
/// 腕が消えて語彙が空になる。**空は呼ぶ側が赤にする。**
module Vocabulary =

  /// 散文の引き先。**無ければ空**で、足りていないことは門が見る
  /// （`SpecCoverage`。ここで落とすと、表を直す前に何も動かなくなる）
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
    if String.IsNullOrEmpty s then s
    else string (Char.ToLowerInvariant s.[0]) + s.Substring 1

  /// option / list を剥がす
  let rec private unwrap (t: Type) =
    if t.IsGenericType then
      let d = t.GetGenericTypeDefinition()
      if d = typedefof<option<_>> || d = typedefof<list<_>> then unwrap (t.GetGenericArguments().[0])
      else t
    else t

  /// `Params = string list`。DTD では `<param>` の並び
  let private isParams (t: Type) =
    t.IsGenericType
    && t.GetGenericTypeDefinition() = typedefof<list<_>>
    && t.GetGenericArguments().[0] = typeof<string>

  let private isAttrs (t: Type) = FSharpType.IsRecord t && t.Name.EndsWith "Attrs"

  /// 全部の腕が同じ語で始まっていたら、その語を落とす。
  /// **切れ目は大文字**（語の途中で切ると `ertical` のような字が出る）。
  /// 1 つでも残りが空になる並びなら落とさない
  let private stripCommonPrefix (names: string[]) =
    if names.Length < 2 then names
    else
      let shortest = names |> Array.map String.length |> Array.min
      let mutable n = 0
      while n < shortest - 1 && names |> Array.forall (fun s -> s.[n] = names.[0].[n]) do
        n <- n + 1
      let mutable cut = n
      while cut > 0 && not (Char.IsUpper names.[0].[cut]) do
        cut <- cut - 1
      if cut > 0 && names |> Array.forall (fun s -> s.Length > cut && Char.IsUpper s.[cut])
      then names |> Array.map (fun s -> s.Substring cut)
      else names

  /// option / list を **1 段 だけ** 剥がして、多重度の印を返す。
  /// `unwrap` は全部 剥がすので、多重度がここで消える
  let private occurs (t: Type) =
    if t.IsGenericType then
      let d = t.GetGenericTypeDefinition()
      if d = typedefof<option<_>> then Some("?", t.GetGenericArguments().[0])
      elif d = typedefof<list<_>> then Some("*", t.GetGenericArguments().[0])
      else None
    else None

  /// 「そこに置ける物」1 つ ぶんの字。多腕 DU は選択に、単腕 DU は要素名に
  let private slot (t: Type) =
    if FSharpType.IsUnion t then
      let cs = FSharpType.GetUnionCases t
      if cs.Length > 1
      then "(" + (cs |> Array.map (fun c -> camel c.Name) |> String.concat " | ") + ")"
      else camel t.Name
    else camel t.Name

  /// `<!ELEMENT ...>` の中身。**腕の field の並びがそのまま順序**、
  /// option / list がそのまま多重度、多腕 DU がそのまま選択になる
  let private contentModel (fields: Type[]) =
    let parts =
      [ for f in fields do
          if isParams f then yield "param*"
          else
            let occ, inner =
              match occurs f with
              | Some(o, i) -> o, i
              | None -> "", f
            if isAttrs inner then ()
            elif inner = typeof<Expr.NumExpr> then yield "#PCDATA"
            else yield slot inner + occ ]
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
          if p.Name.StartsWith(elementName, StringComparison.OrdinalIgnoreCase)
          then p.Name.Substring elementName.Length
          else p.Name
        let vt = unwrap p.PropertyType
        let cases = if FSharpType.IsUnion vt then FSharpType.GetUnionCases vt else [||]
        // 腕が 1 本 の DU は「値の並び」ではない（ActionLabel など）。
        // **札は並びでない相手にも読む** —— 読まないと「自由記述に既定が付いた」を
        // 見る門が、当たる先を持たない
        let values =
          if cases.Length > 1
          then cases |> Array.map (fun c -> c.Name) |> stripCommonPrefix |> Array.map camel
          else [||]
        // 既定は「腕の位置」で引く。名前をもう一度 変換すると
        // stripCommonPrefix の結果とずれる余地ができる
        let defaults =
          cases
          |> Array.mapi (fun i c -> i, c)
          |> Array.filter (fun (_, c) ->
               (c.GetCustomAttributes typeof<BulletmlDefaultAttribute>).Length > 0)
          |> Array.map (fun (i, c) -> if values.Length > 0 then values.[i] else camel c.Name)
        let name = camel bare
        // option でない field は、書かないと読めない属性（actionRef/@label）
        let required =
          not (p.PropertyType.IsGenericType
               && p.PropertyType.GetGenericTypeDefinition() = typedefof<option<_>>)
        let decl = if values.Length > 0 then "(" + String.concat "|" values + ")" else "CDATA"
        let def =
          match Array.tryHead defaults with
          | Some d -> "\"" + d + "\""
          | None -> if required then "#REQUIRED" else "#IMPLIED"
        { Name = name
          Values = values
          Defaults = defaults
          Dtd = sprintf "<!ATTLIST %s %s %s %s>" elementName name decl def
          Spec = attributeSpec (sprintf "%s/@%s" elementName name)
          ValueSpecs =
            values
            |> Array.map (fun v -> v, attrValueSpec (sprintf "%s/@%s=%s" elementName name v)) })

  /// 木を歩いて、腕ごとに「要素名 -> 腕の持ち物」を集める。
  /// **同じ腕が 2 か所 に出る**（`Action` は BulletmlElm / Action / ActionElm に居る）
  /// が、持ち物は同じなので上書きしても変わらない
  let rec private collect (t: Type) (seen: HashSet<Type>) (acc: Dictionary<string, Type[]>) =
    if FSharpType.IsUnion t && seen.Add t then
      for c in FSharpType.GetUnionCases t do
        let fields = c.GetFields() |> Array.map (fun p -> p.PropertyType)
        acc.[camel c.Name] <- fields
        for f in fields do
          if not (isParams f) then collect (unwrap f) seen acc

  let private describe (name: string) (fields: Type[]) =
    let children = ResizeArray<string>()
    let attrs = ResizeArray<VocabAttr>()
    let mutable text = false
    for f in fields do
      if isParams f then children.Add "param"
      else
        let u = unwrap f
        if u = typeof<Expr.NumExpr> then text <- true
        elif isAttrs u then attrs.AddRange(attrsOf name u)
        elif FSharpType.IsUnion u then
          for c in FSharpType.GetUnionCases u do
            children.Add(camel c.Name)
    { Name = name
      Children = children |> Seq.distinct |> Seq.toArray
      Attrs = attrs.ToArray()
      Text = text
      Dtd = sprintf "<!ELEMENT %s %s>" name (contentModel fields)
      Spec = elementSpec name }

  /// 語彙。**空なら呼ぶ側が赤にすること** —— reflection が効いていない印
  let elements: VocabElement[] =
    let acc = Dictionary<string, Type[]>()
    collect typeof<Bulletml> (HashSet<Type>()) acc
    acc
    |> Seq.map (fun kv -> describe kv.Key kv.Value)
    // `Params = string list` は腕を持たないので木から出てこない。
    // **DTD は `<!ELEMENT param (#PCDATA)>`** なので、中身を取る要素として足す
    |> Seq.append
         [ { Name = "param"
             Children = [||]
             Attrs = [||]
             Text = true
             Dtd = "<!ELEMENT param (#PCDATA)>"
             Spec = elementSpec "param" } ]
    |> Seq.sortBy (fun e -> e.Name)
    |> Seq.toArray

  /// 式の中で使える字。**DU からは引けない** —— `$rand` / `$rank` という綴りは
  /// 読む側（`Expr` の parser）が文字で持っていて、木の腕の名前
  /// （`Rand` / `Rank`）とは別物。ここは短い表にする。
  ///
  /// **代わりに「Parser が本当に読める字か」を試験で当てる**
  /// （`Expr.NumExpr.ofString` に通して、`Invalid` でなく
  /// `NeedRand` / `NeedRank` が立つこと）。綴りが動けば赤になる。
  let expressions = [| "$rand"; "$rank" |]

  // --- F# の CE を「どこに置けるか」 -----------------------------------------

  /// 入れ物の種類。**要素ではない。**
  ///
  /// v1.9 の頭で数えて分かったこと —— `repeat` の中に置けるものは
  /// `action` の中と**同じ**。どちらも `ActionBuilder` だから。
  /// 要素（`<repeat>` と `<action>`）で分けると、`repeat` の中で
  /// 候補が 1 つ も出なくなる。
  ///
  /// 綴りは要素名と重ならないものを使う（**器へ渡すので、要素名を
  /// そのまま流すと線を越える**）。
  /// いちばん外（まだ `{ }` の中に居ない）。**根の builder はここに置く**
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

  /// 型の名前 -> 入れ物。**表はこの 1 か所 だけ**（残りは reflection）。
  ///
  /// `ActionBuilder<'T>` のような builder は「`{ }` を開く」側で、
  /// **置ける先は `'T` のほう** —— `body` は `ActionElm` を返すので
  /// `bullet` の中、`nest` は `Action` を返すので `action` の中。
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
    if t.Name.StartsWith "FSharpFunc" then resultOf (t.GetGenericArguments().[1]) else t

  /// 根から走る定義の名前の頭。
  ///
  /// **Core の `Api.fs` が持っている綴り。** あちらは top* の並びを
  /// `label.StartsWith "top"` で選んでいて、そこが真。
  /// **reflection では引けない**（字の比較であって型ではない）ので、
  /// Core を見ているこの層が写す —— 器（`Semantics`）に書くと、
  /// Core を変えたときに黙って割れる。
  ///
  /// 変えるときは `Core/Api.fs` の `StartsWith` と対で。割れたら
  /// `Parser.Tests` の意味の検査が赤くなる（同梱 176 本 が 1 本 も
  /// 入口を持たないことになるので）
  [<Literal>]
  let topPrefix = "top"

  /// CE の名前が「どこに置けて」「何を開くか」。**表ではなく `Dsl` から引く。**
  ///
  /// v1.9 の頭で reflection を測ったら、`ActionBuilder` と `BulletmlBuilder` の
  /// CustomOperation は **0 個** だった —— あの 2 つ の中身は module の
  /// 公開 `let` で、`[<CustomOperation>]` では引けない。
  /// **だから両方 を舐める。**
  ///
  ///     (CE の名前, 置ける入れ物, 開く `{ }` の種類。開かないなら空)
  let cePlaces : (string * string * string)[] =
    let asm = typeof<FsBulletML2.Dsl.BulletmlBuilder>.Assembly
    let dslModule = asm.GetTypes() |> Array.find (fun t -> t.FullName = "FsBulletML2.Dsl")
    // module の公開 let。**戻り値の型が置ける先を決める**
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
             // **根の builder はまだ `{ }` の中に居ない。** いちばん外に置く
             | Some s when s = SlotRoot -> Some SlotOuter
             // accel は `{ }` を開くが、それ自身は action の中に置く
             | Some s when s = SlotAccel -> Some SlotAction
             // ほかの builder は `'T` の中に置ける ——
             // `body` は `ActionElm` を返すので bullet の中、
             // `nest` は `Action` を返すので action の中
             | Some _ when r.IsGenericType ->
               r.GetGenericArguments() |> Array.tryHead |> Option.bind (fun a -> slotOfType a.Name)
             | Some _ -> None
             | None -> slotOfType r.Name
           match place with
           | None -> None
           | Some p -> Some(name, p, defaultArg opens ""))
    // builder の CustomOperation。**置ける先はその builder が開く `{ }`**
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

  /// 起動時に 1 回 だけ JS へ渡す形。**毎キー呼ばない。**
  /// 手で組むのは、F# のレコードを serializer に任せると版で形が動くから
  let toJson () =
    let sb = StringBuilder()
    let str (s: string) = sb.Append('"').Append(escape s).Append('"') |> ignore
    let arr (xs: string[]) =
      sb.Append '[' |> ignore
      xs |> Array.iteri (fun i x ->
        if i > 0 then sb.Append ',' |> ignore
        str x)
      sb.Append ']' |> ignore
    sb.Append "{\"elements\":[" |> ignore
    elements
    |> Array.iteri (fun i e ->
        if i > 0 then sb.Append ',' |> ignore
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
            if j > 0 then sb.Append ',' |> ignore
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
                if k > 0 then sb.Append ',' |> ignore
                sb.Append "{\"value\":" |> ignore
                str v
                sb.Append ",\"spec\":" |> ignore
                str s
                sb.Append '}' |> ignore)
            sb.Append "]}" |> ignore)
        sb.Append "]}" |> ignore)
    sb.Append "],\"expressions\":" |> ignore
    arr expressions
    // **F# の CE の名前。** ここは reflection ではなく手で書いた表（`Spec.ce`）。
    // 名前が `Dsl` を過不足なく覆うことは `SpecCoverage` が見ている
    sb.Append ",\"ce\":[" |> ignore
    Spec.ce
    |> List.iteri (fun i (name, element, attr, value) ->
        if i > 0 then sb.Append ',' |> ignore
        sb.Append "{\"name\":" |> ignore
        str name
        sb.Append ",\"element\":" |> ignore
        str element
        sb.Append ",\"attr\":" |> ignore
        str attr
        sb.Append ",\"value\":" |> ignore
        str value
        sb.Append '}' |> ignore)
    // **CE の名前が載せる label。** 上の `ce` と別の表（`Spec.ceLabels`）——
    // あちらは「作る要素」で、こちらは「名前を決めるか、使うか」
    sb.Append "],\"ceLabels\":[" |> ignore
    Spec.ceLabels
    |> List.iteri (fun i (name, element, labelArg, fixedName, root) ->
        if i > 0 then sb.Append ',' |> ignore
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
    // **どこに置けて、何を開くか。** 表ではなく `Dsl` から reflection で引いたもの
    sb.Append "],\"cePlaces\":[" |> ignore
    cePlaces
    |> Array.iteri (fun i (name, place, opens) ->
        if i > 0 then sb.Append ',' |> ignore
        sb.Append "{\"name\":" |> ignore
        str name
        sb.Append ",\"in\":" |> ignore
        str place
        sb.Append ",\"opens\":" |> ignore
        str opens
        sb.Append '}' |> ignore)
    // 根から走る定義の名前の頭。**綴りは Core が持っている** ——
    // 器に書き写すと、あちらを変えたときに黙って割れる（Semantics の但し書き）
    // 根から走る定義の名前の頭。**綴りは Core が持っている**
    sb.Append "],\"topPrefix\":" |> ignore
    str topPrefix
    // 雛形（v2.6）。**骨は要素名の木**なので、器に書くと
    // 「ブラウザ側に要素名を書かない」線を越える —— 語彙と同じ経路で渡す
    sb.Append ",\"frames\":[" |> ignore
    let rec writeFrame (f: SourceLanguage.Frame) =
      sb.Append "{\"element\":" |> ignore
      str f.Element
      sb.Append ",\"text\":" |> ignore
      str f.Text
      sb.Append ",\"attrs\":[" |> ignore
      f.Attrs
      |> List.iteri (fun i (k, v) ->
          if i > 0 then sb.Append ',' |> ignore
          sb.Append "{\"name\":" |> ignore
          str k
          sb.Append ",\"value\":" |> ignore
          str v
          sb.Append '}' |> ignore)
      sb.Append "],\"children\":[" |> ignore
      f.Children
      |> List.iteri (fun i c ->
          if i > 0 then sb.Append ',' |> ignore
          writeFrame c)
      sb.Append "]}" |> ignore
    Frames.all
    |> List.iteri (fun i s ->
        if i > 0 then sb.Append ',' |> ignore
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
