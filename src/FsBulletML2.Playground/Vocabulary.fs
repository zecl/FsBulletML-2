namespace FsBulletML2.Playground

open System
open System.Collections.Generic
open System.Text
open Microsoft.FSharp.Reflection
open FsBulletML2

/// 属性 1 つ。`Values` が空なら自由記述（label など）。
type VocabAttr =
  { Name: string
    Values: string[] }

/// 要素 1 つ。`Children` は「この中に置ける要素」、`Text` は #PCDATA を取るか。
type VocabElement =
  { Name: string
    Children: string[]
    Attrs: VocabAttr[]
    Text: bool }

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

  let private attrsOf (elementName: string) (t: Type) =
    FSharpType.GetRecordFields t
    |> Array.map (fun p ->
        // field 名は要素名で始まる（directionType / actionRefLabel）。剥がす
        let bare =
          if p.Name.StartsWith(elementName, StringComparison.OrdinalIgnoreCase)
          then p.Name.Substring elementName.Length
          else p.Name
        let vt = unwrap p.PropertyType
        let values =
          if FSharpType.IsUnion vt && (FSharpType.GetUnionCases vt).Length > 1 then
            FSharpType.GetUnionCases vt
            |> Array.map (fun c -> c.Name)
            |> stripCommonPrefix
            |> Array.map camel
          else [||]
        { Name = camel bare; Values = values })

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
      Text = text }

  /// 語彙。**空なら呼ぶ側が赤にすること** —— reflection が効いていない印
  let elements: VocabElement[] =
    let acc = Dictionary<string, Type[]>()
    collect typeof<Bulletml> (HashSet<Type>()) acc
    acc
    |> Seq.map (fun kv -> describe kv.Key kv.Value)
    // `Params = string list` は腕を持たないので木から出てこない。
    // **DTD は `<!ELEMENT param (#PCDATA)>`** なので、中身を取る要素として足す
    |> Seq.append [ { Name = "param"; Children = [||]; Attrs = [||]; Text = true } ]
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
        sb.Append ",\"attrs\":[" |> ignore
        e.Attrs
        |> Array.iteri (fun j a ->
            if j > 0 then sb.Append ',' |> ignore
            sb.Append "{\"name\":" |> ignore
            str a.Name
            sb.Append ",\"values\":" |> ignore
            arr a.Values
            sb.Append '}' |> ignore)
        sb.Append "]}" |> ignore)
    sb.Append "],\"expressions\":" |> ignore
    arr expressions
    sb.Append "}" |> ignore
    sb.ToString()
