/// 意味の層。読めて・組めても、走らないものが在る。
/// 字から数える。木にしない。文面を持たない。Fable.Core に依存しない。
module FsBulletML2.LanguageService.Semantics

/// 見つけたものの種類。強さはここで決めない ——
/// 見せ方は呼ぶ側（Monaco の severity は Monaco 固有の数）
type FindingKind =
  /// 根から走る定義が 1 つ も無い。走らない
  | NoEntryPoint
  /// どこからも参照されない定義。走りには影響しない
  | UnusedDefinition
  /// 定義に無い名前を指している参照。Apply は通る（解けない `Ref` は
  /// 黙って無視されるだけで Core は落ちない）。
  ///
  /// 数え方は `Refs.missing` の 1 本。ここでは持たない ——
  /// 波線と Quick Fix が同じ数え方を見る
  | MissingRef
  /// 読めない式（v4.1）。壊れ方が 2 通り あるので、腕が値を持つ。
  ///
  ///     Stops = true    走らない。Apply が落ちる（`#loop-error` に出る）
  ///     Stops = false   走る。値が NaN になるだけ
  ///
  /// 割れ目は Core の畳みに在る —— `$` を含まない式だけが読み込みの段で
  /// 畳まれ、そこで落ちる
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
    /// 1 起点（Monaco の行桁と同じ）。
    /// `NoEntryPoint` は本文の頭を指す —— 「どこにも無い」の位置は
    /// 本文全体で、1 か所 を指せない
    Line: int
    Column: int
    EndColumn: int }

/// 名前が載っている属性を引く
let private labelOf (attrName: string) (t: TagHit) =
  t.Attrs |> List.tryFind (fun a -> a.AttrName = attrName)

/// 意味の層を全部。本文に出てくる順で返す。
///
/// 受け取るのは
///
///     pairs       `Refs.pairs` の返り（参照する要素, される要素, 属性名）
///     topPrefix   根から走る定義の名前の頭。空なら `NoEntryPoint` を出さない
///     tags        表記ごとの 1 本（`XxxScan.tags`）の返り
///
/// 要素名も `top` の綴りも、この 1 本 は持たない。 どちらも
/// 受け取るものの中に在る —— 書き写すと、Core を変えたときに黙って割れる。
///
/// `NoEntryPoint` は緩い側に振ってある（`top` 始まり の定義 が要素を問わず
/// 1 つ も無いときだけ 出す。見逃すことは在るが、正しい本を光らせない）
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

  // この守りは冗長で、外しても 1 点 も赤くならない（較正済み）。
  // それでも残すのは、意図が字に出るのはここだけだから ——
  // `StartsWith` を `=` に替えたら、空のときに全部 が非入口になって
  // 「走らない」と言い出す。そのときこの行が効く
  let entryPoints =
    if topPrefix = "" then []
    else allDefs |> List.filter (fun (_, a) -> a.Value.StartsWith topPrefix)

  // 1 つ も無いときだけ、1 本 だけ出す。 定義ごとには出せない ——
  // 「どこにも無い」の位置は本文全体で、1 か所 を指せない
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

  // 同じ名前の定義（v4.6）。対ごとに数える ——
  // `action` の `top` と `bullet` の `top` は別の名前空間
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

  // 定義に無い参照。数え方は `Refs.missing` の 1 本 ——
  // 波線と Quick Fix が同じものを見る（別に数えると、
  // 「波線は出るのに直し方が出ない」が作れてしまう）
  let missing =
    Refs.missing pairs tags
    |> List.map (fun m ->
        { Kind = MissingRef
          Name = m.Hit.Value
          Element = m.RefName
          Line = m.Hit.Line
          Column = m.Hit.Column
          EndColumn = m.Hit.EndColumn })

  // 並べ直す。 上は対ごとに走るので、対の順に並んでいる ——
  // 人へ見せる側は本文の順で読む（`Refs.missing` と同じ理由）。
  // `noEntry` だけは本文全体の話なので、位置に関わらず先頭
  noEntry @ (unused @ missing @ duplicates |> List.sortBy (fun f -> f.Line, f.Column))

/// 読めない式（v4.1）。`findings` と別の 1 本（材料が違う ——
/// あちらは `TagHit`、こちらは `TextHit`。押し込むと
/// 「出さない」と「取れない」が混ざる）。
///
/// 位置は「読めたところの続き」から、その字の終わりまで
/// （要素まるごとに引くと `180+$rand*30` が全部 赤くなって、どこが悪いか読めない）。
///
/// 行をまたがない —— またいで桁を出すと、2 行 目 の桁が 1 行 目 の続きになる
let exprFindings (source: string) (texts: TextHit list) : Finding list =
  // 内包表記の中で `while` を回さない（v4.9）——
  // Fable は内包の中の `while` を enumerator の鎖に焼く（`Scan.fs` に書いた）
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

/// 2 つ の runtime で同じ答えが返ることを見る口（`guard-fable-parity`）。
///
/// 数えるところは `findings` 1 本（別々に組むと、組み方のほうが食い違って
/// 「中身は同じなのに赤」になる）。
///
/// 対と `topPrefix` はこの関数が決める。ここだけ表記を名指しする
/// （引数にすると表のほうが 2 runtime で割れうるものになる）
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
  // 式（v4.1）。要素名の表を書かない —— 本文に出てくる名前を全部 渡す
  // （突き合わせに要るのは「2 つ の runtime が同じ答えを出すこと」だけ）
  add " expr="
  let allNames = hits |> List.map (fun t -> t.TagName) |> List.distinct
  render (exprFindings source (XmlScan.texts source allNames))
  sb.ToString()
