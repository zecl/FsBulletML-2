/// **意味の層。読めて・組めても、走らないものが在る。**
///
/// v2.2 までの `Diagnosis` は 4 層 とも構文と構造だった ——
///
///     構文              XmlException / FParsec        行・桁 あり
///     BulletML でない   tryRead… が None              位置なし
///     木は組めない      BulletmlDTDViolationException 位置なし
///     式                XPathException                位置なし
///
/// どれも通るのに走らない弾幕が在る。`top` で始まる定義が 1 つ も無い本は、
/// **読めて・組めて・黙って何も起きない**（走らせる側が `StartsWith` で
/// 選ぶだけで、誰も警告しない）。
///
/// ## 字から数える。**木にしない。**
///
/// 材料は `TagHit` だけ —— つまり `Refs.missing` と同じ形で、
/// **WASM への往復が要らない。** だから打鍵ごとに数え直せる
/// （v1.8 で未決にした「打鍵ごとの波線」がここで実現する）。
///
/// 版の頭で測った値段は、いちばん長い本（29,190 字）で **0.488 ms / 回**。
/// 既存の `Refs.missing` が 0.291 ms なので、同じ桁。
///
/// ## 出さないと決めたもの
///
/// 候補は 4 つ 在って、同梱 176 本 に当てたらこうなった ——
///
///     top が無い            当たり 0 / 176 本   偽陽性なし
///     呼ばれない定義         当たり 6 / 176 本   **正しい弾幕でも光る**
///     参照の循環            当たり 2 / 176 本   **正しい弾幕に在る**
///     repeat の times が 0   当たり 0 / 176 本   当てる先が無い
///
/// **参照の循環は出さない。** 当たった 2 件 は同梱、つまり正しく動く弾幕で、
/// BulletML では `actionRef` の輪は正常（無限に繰り返す弾幕がそう書く）。
/// Core が「1 段 だけ解く」入口を持っているのは、まさにそれを前提にしている。
///
/// **`repeat` の times は出せない。** `TagHit` は要素の中身（#PCDATA）を
/// 持たないので、字からは値が取れない。当たりも 0 なので、
/// 取れる形にする理由が測れていない。
///
/// **呼ばれない定義は強さを下げる。** 6 件 とも正しい弾幕に在る ——
/// 走りには影響しないので、これは警告ではなく知らせ。
///
/// ## 文面を持たない
///
/// `Refs.Missing` と同じ理由。持たせると、波線と直し方で文面が割れて、
/// **どちらも単独では正しく見える。** 文面は呼ぶ側が組む。
///
/// ## `Fable.Core` に依存しない
///
/// host（.NET）と ブラウザ側（Fable が焼いた JS）の**両方 で走る。**
module FsBulletML2.LanguageService.Semantics

/// 見つけたものの種類。**強さはここで決めない** ——
/// 見せ方は呼ぶ側（Monaco の severity は Monaco 固有の数）
type FindingKind =
  /// 根から走る定義が 1 つ も無い。**走らない**
  | NoEntryPoint
  /// どこからも参照されない定義。走りには影響しない
  | UnusedDefinition
  /// 定義に無い名前を指している参照。
  ///
  /// **Apply は通る。** 実測 —— 参照が解けないと、その `Ref` は黙って
  /// 無視されるだけで、Core は落ちない（`expandActionRefOnce` は
  /// 1 段 だけ解いて、解けなければそのまま）。
  /// つまりこれも「読めて・組めて・書いたものが出ない」側。
  ///
  /// 数え方は `Refs.missing` の 1 本。**ここでは持たない** ——
  /// 波線と Quick Fix が同じ数え方を見る
  | MissingRef

/// 意味の層の指摘 1 つ。
type Finding =
  { Kind: FindingKind
    /// 見つかった名前。`NoEntryPoint` では空
    Name: string
    /// その定義の要素名（`action` など）。`NoEntryPoint` では空。
    /// **器がこの字を作らない** —— `TagHit.TagName` をそのまま渡す
    Element: string
    /// 1 起点（Monaco の行桁と同じ）。
    /// **`NoEntryPoint` は本文の頭を指す** —— 「どこにも無い」の位置は
    /// 本文全体で、1 か所 を指せない
    Line: int
    Column: int
    EndColumn: int }

/// 名前が載っている属性を引く
let private labelOf (attrName: string) (t: TagHit) =
  t.Attrs |> List.tryFind (fun a -> a.AttrName = attrName)

/// 意味の層を全部。**本文に出てくる順**で返す。
///
/// 受け取るのは
///
///     pairs       `Refs.pairs` の返り（参照する要素, される要素, 属性名）
///     topPrefix   根から走る定義の名前の頭。**空なら `NoEntryPoint` を出さない**
///     tags        表記ごとの 1 本（`XxxScan.tags`）の返り
///
/// **要素名も `top` の綴りも、この 1 本 は持たない。** どちらも
/// 受け取るものの中に在る —— 書き写すと、Core を変えたときに黙って割れる。
///
/// ### `NoEntryPoint` は要素を絞らない
///
/// 走らせる側が見るのは `action` だけだが、**器に要素名を書けない**ので、
/// 「`topPrefix` で始まる名前がどの定義にも 1 つ も無い」で見る。
/// **緩い側に振ってある** —— `bullet label="top…"` しか無い本は
/// 見逃すが、正しい本を光らせることは無い。
/// 同梱 176 本 では当たりも偽陽性も 0 で、そこは測れていない。
let findings
  (pairs: (string * string * string) list)
  (topPrefix: string)
  (tags: TagHit list)
  : Finding list =
  // **開始札だけ見る。** 閉じ札は XML だけが返すもので、属性を持たない
  let opens = tags |> List.filter (fun t -> not t.Closing)
  let defNames = pairs |> List.map (fun (_, defName, _) -> defName) |> List.distinct

  /// その要素に付いた（名前, その属性）の並び
  let namedOf (elementName: string) (attrName: string) =
    opens
    |> List.filter (fun t -> t.TagName = elementName)
    |> List.choose (fun t -> labelOf attrName t |> Option.map (fun a -> t.TagName, a))

  let allDefs =
    [ for (_, defName, attrName) in pairs do yield! namedOf defName attrName ]

  // **この守りは冗長で、外しても答えが変わらない。** `StartsWith ""` は
  // 常に true なので、`topPrefix` が空なら全部 が入口に数えられ、
  // 下の `noEntry` は結局 何も出さない —— 較正で外してみて、
  // **1 点 も赤くならないことを確かめてある。**
  //
  // それでも残すのは、**意図が字に出るのはここだけ**だから。
  // `StartsWith` を `=` に替えたら、空のときに全部 が非入口になって
  // 「走らない」と言い出す。そのときこの行が効く
  let entryPoints =
    if topPrefix = "" then []
    else allDefs |> List.filter (fun (_, a) -> a.Value.StartsWith topPrefix)

  // **1 つ も無いときだけ、1 本 だけ出す。** 定義ごとには出せない ——
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
          // **根から走るものは「呼ばれない」ではない。** そこは入口
          if not (topPrefix <> "" && a.Value.StartsWith topPrefix)
             && not (List.contains a.Value used) then
            yield
              { Kind = UnusedDefinition
                Name = a.Value
                Element = defName
                Line = a.Line
                Column = a.Column
                EndColumn = a.EndColumn } ]

  // 定義に無い参照。**数え方は `Refs.missing` の 1 本** ——
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

  // **並べ直す。** 上は対ごとに走るので、対の順に並んでいる ——
  // 人へ見せる側は本文の順で読む（`Refs.missing` と同じ理由）。
  // `noEntry` だけは本文全体の話なので、位置に関わらず先頭
  noEntry @ (unused @ missing |> List.sortBy (fun f -> f.Line, f.Column))

/// 2 つ の runtime で同じ答えが返ることを見る口（`guard-fable-parity`）。
///
/// **数えるところは `findings` 1 本。** node 側 と .NET 側 で別々に
/// 組むと、組み方のほうが食い違って「中身は同じなのに赤」になる。
///
/// 渡された字を本文として読ませる。対と `topPrefix` は**この関数が決める**
/// —— 表に並べると、そちらが 2 つ 目 の表になる。
///
/// **ここだけ表記を名指しする**（`XmlScan`）。門は字を 1 本 渡して
/// 答えを比べるので、どの表記で数えるかを引数にすると
/// **表のほうが 2 runtime で割れうる**ものになる
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
                | MissingRef -> "missing")
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
  // **`topPrefix` が空のときは `entry` を出さない。** そこも突き合わせる
  render (findings pairs "" hits)
  add " nopairs="
  render (findings [] "top" hits)
  sb.ToString()
