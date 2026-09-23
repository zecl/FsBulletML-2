/// 意味の層。読めて・組めても、走らないものが在る。
/// 字から数える。木にしない。文面を持たない。Fable.Core に依存しない。
module FsBulletML2.LanguageService.Semantics

/// 見つけたものの種類。強さはここで決めない。見せ方は呼ぶ側。
type FindingKind =
  /// 根から走る定義が 1 つ も無い。走らない
  | NoEntryPoint
  /// どこからも参照されない定義。走りには影響しない
  | UnusedDefinition
  /// 定義に無い名前を指している参照。Apply は通る。
  /// 数え方は `Refs.missing`。ここで別に数えるな。
  | MissingRef
  /// 読めない式（v4.1）。`Stops` が真なら走らない。偽なら NaN のまま走る。
  /// `$` を含まない式だけが読み込みで畳まれて落ちる。
  | BadExpr of stops: bool
  /// 同じ名前の定義が 2 つ 以上 在る（v4.6）。出すのは 2 つ 目 から ——
  /// 走るのは先に書いたほうなので、光らせるのは負けるほう
  | DuplicateDefinition

/// 意味の層の指摘 1 つ。
type Finding =
  { Kind: FindingKind
    /// 見つかった名前。`NoEntryPoint` では空
    Name: string
    /// その定義の要素名（`action` など）。`NoEntryPoint` では空。
    /// 器がこの字を作らない —— `TagHit.TagName` をそのまま渡す
    Element: string
    /// 1 起点。`NoEntryPoint` は本文の頭を指す。1 か所 を指せない。
    Line: int
    Column: int
    EndColumn: int }

/// 名前が載っている属性を引く
let private labelOf (attrName: string) (t: TagHit) =
  t.Attrs |> List.tryFind (fun a -> a.AttrName = attrName)

/// 意味の層を全部。本文に出てくる順。
/// 要素名も `top` の綴りも持たない。書き写すと Core を変えたとき黙って割れる。
/// `topPrefix` が空なら `NoEntryPoint` を出さない。
let findings
  (pairs: (string * string * string) list)
  (topPrefix: string)
  (tags: TagHit list)
  : Finding list =
  // 開始札だけ見る。 閉じ札は XML だけが返すもので、属性を持たない
  let opens = tags |> List.filter (fun t -> not t.Closing)
  let defNames = pairs |> List.map (fun (_, defName, _) -> defName) |> List.distinct

  /// その要素に付いた（名前, その属性）の並び
  let namedOf (elementName: string) (attrName: string) =
    opens
    |> List.filter (fun t -> t.TagName = elementName)
    |> List.choose (fun t -> labelOf attrName t |> Option.map (fun a -> t.TagName, a))

  let allDefs =
    [ for (_, defName, attrName) in pairs do yield! namedOf defName attrName ]

  // 空の `topPrefix` で `StartsWith` を `=` に替えるな。全部が非入口になる。
  let entryPoints =
    if topPrefix = "" then []
    else allDefs |> List.filter (fun (_, a) -> a.Value.StartsWith topPrefix)

  // 1 つ も無いときだけ 1 本。定義ごとには出せない。
  let noEntry =
    if topPrefix <> "" && List.isEmpty entryPoints && not (List.isEmpty defNames) then
      [ { Kind = NoEntryPoint
          Name = ""
          Element = ""
          Line = 1
          Column = 1
          EndColumn = 1 } ]
    else []

  let unused =
    [ for (refName, defName, attrName) in pairs do
        let used =
          opens
          |> List.filter (fun t -> t.TagName = refName)
          |> List.choose (labelOf attrName)
          |> List.map (fun a -> a.Value)
        for (_, a) in namedOf defName attrName do
          // 根から走るものは「呼ばれない」ではない。 そこは入口
          if not (topPrefix <> "" && a.Value.StartsWith topPrefix)
             && not (List.contains a.Value used) then
            yield
              { Kind = UnusedDefinition
                Name = a.Value
                Element = defName
                Line = a.Line
                Column = a.Column
                EndColumn = a.EndColumn } ]

  // 同じ名前の定義は対ごとに数える。要素が違えば別の名前。
  let duplicates =
    [ for (_, defName, attrName) in pairs do
        let seen = System.Collections.Generic.HashSet<string>()
        for (_, a) in namedOf defName attrName do
          if not (seen.Add a.Value) then
            yield
              { Kind = DuplicateDefinition
                Name = a.Value
                Element = defName
                Line = a.Line
                Column = a.Column
                EndColumn = a.EndColumn } ]

  // 定義に無い参照は `Refs.missing`。別に数えると波線と直し方がずれる。
  let missing =
    Refs.missing pairs tags
    |> List.map (fun m ->
        { Kind = MissingRef
          Name = m.Hit.Value
          Element = m.RefName
          Line = m.Hit.Line
          Column = m.Hit.Column
          EndColumn = m.Hit.EndColumn })

  // 対の順ではなく本文の順。`noEntry` だけ先頭に残す。
  noEntry @ (unused @ missing @ duplicates |> List.sortBy (fun f -> f.Line, f.Column))

/// 読めない式（v4.1）。`findings` と別。材料は `TextHit`。
/// 波線は読めた続きから。要素まるごとにはしない。行をまたがない。
let exprFindings (source: string) (texts: TextHit list) : Finding list =
  // 内包の中で `while` を回さない。Fable が enumerator の鎖に焼く。
  let out = ResizeArray<Finding>()
  for h in texts do
    if not (ExprCheck.readable h.Text) then
      let at = h.Start + ExprCheck.readTo h.Text
      let struct (line, column) = Scan.lineColumn source at
      // その行の終わりか、字の終わりか、近いほう
      let mutable stop = h.Stop
      let mutable k = at
      while k < stop && source.[k] <> '\n' do k <- k + 1
      if k < stop then stop <- k
      let endColumn = max (column + 1) (column + (stop - at))
      out.Add
        { Kind = BadExpr(not (h.Text.Contains "$"))
          Name = h.Text
          Element = h.TagName
          Line = line
          Column = column
          EndColumn = endColumn }
  List.ofSeq out

/// 2 runtime の突き合わせ口。数えるところは `findings` 1 本。
/// 対と `topPrefix` はここで決める。引数にすると表のほうが割れうる。
let describe (source: string) : string =
  let tags = XmlScan.tags
  let pairs = [ "aRef", "a", "label"; "bRef", "b", "label" ]
  let sb = System.Text.StringBuilder()
  let add (s: string) = sb.Append s |> ignore
  let render (fs: Finding list) =
    if List.isEmpty fs then add "-"
    else
      fs
      |> List.iteri (fun i f ->
           if i > 0 then add ","
           add (match f.Kind with
                | NoEntryPoint -> "entry"
                | UnusedDefinition -> "unused"
                | MissingRef -> "missing"
                | BadExpr true -> "expr!"
                | BadExpr false -> "expr?"
                | DuplicateDefinition -> "dup")
           add ":"
           add f.Element
           add ":"
           add f.Name
           add "@"
           add (string f.Line)
           add ":"
           add (string f.Column)
           add "-"
           add (string f.EndColumn))
  let hits = tags source
  add "top="
  render (findings pairs "top" hits)
  add " none="
  // `topPrefix` が空のときは `entry` を出さない。 そこも突き合わせる
  render (findings pairs "" hits)
  add " nopairs="
  render (findings [] "top" hits)
  // 要素名の表を書かない。本文に出てくる名前を全部 渡す。
  add " expr="
  let allNames = hits |> List.map (fun t -> t.TagName) |> List.distinct
  render (exprFindings source (XmlScan.texts source allNames))
  sb.ToString()
