namespace FsBulletML2.LanguageService

open System
open System.Collections.Generic
open System.Text

/// 開始札の中の属性 1 つ。
///
/// `Line` / `Column` / `EndColumn` は**波線を引くための 1 起点**で、
/// 指すのは値の中身（引用符の内側）。`*Start` / `*Stop` は
/// **カーソルの下を当てるための 0 起点 の文字数**で、`Stop` は含まない。
/// 2 通り 持っているのは、要る側が Monaco の行桁と本文の添字で違うため
type AttrHit =
  { AttrName: string
    Value: string
    Line: int
    Column: int
    EndColumn: int
    NameStart: int
    NameStop: int
    ValueStart: int
    ValueStop: int }

/// 要素 1 つ。**表記をまたいで同じ型。**
///
/// XML なら札 1 枚、sxml なら括弧 1 組。参照を数える側
/// （`References.missing`）が読むのは `TagName` と `Attrs` と `Closing` だけで、
/// **そこは表記に依らない。**
///
/// **`Stop` の意味は表記ごとに違う。**
///
///     XML    開始札の `>` の位置（閉じていなければ本文の末尾）
///     sxml   その括弧の `)` の位置（閉じていなければ本文の末尾）
///
/// 揃えられない —— XML は「札が閉じる」と「要素が閉じる」が別の位置に在り、
/// sxml は同じ位置に在る。**読む側が `Start` / `Stop` を要るのは
/// カーソルの居場所を出すときだけ**で、そこは表記ごとの実装の中に居る。
type TagHit =
  { TagName: string
    Attrs: AttrHit list
    /// `</foo>` か。**sxml では常に false** —— 括弧 1 組 が開きも閉じも兼ねる
    Closing: bool
    /// `<foo/>` か。**sxml では常に false**（同上）
    SelfClosing: bool
    /// 要素が始まる位置（0 起点 の文字数）。XML なら `<`、sxml なら `(`
    Start: int
    /// 上の但し書きの位置
    Stop: int
    /// 要素名の範囲。`Stop` は含まない
    NameStart: int
    NameStop: int
    /// 入れ子の深さ。根が 0（v2.4）。**閉じ札は開いていた側と同じ数**。
    ///
    /// ### なぜ `Stop` を揃えずにこれを足したか
    ///
    /// 版の頭で測った。**`Stop` の意味を揃えると `contextAt` が壊れる** ——
    /// あちらは `cursor <= t.Stop` で「開始札の中に居るか」を見ていて、
    /// `Stop` が要素の終わり（子を含む）になると、中身に居るときも
    /// 開始札の中と読むことになる。
    ///
    /// 一方、入れ子そのものは `TagHit` だけからは組めなかった ——
    ///
    ///     xml    開始/閉じ の対応で組める（深さ 6・要素 662 個・割れ 0）
    ///     sxml   Start..Stop の包含で組める（176 / 176 本）
    ///     fsb    **組めない**（`Stop` が行末なので、全部 が兄弟に見える）
    ///     F# CE  **組めない**（名前しか返さない）
    ///
    /// **知り方は表記ごとに違うが、答えは 1 つ の数。** だから数のほうを
    /// 持たせて、知り方はそれぞれの Scan に閉じる —— どの Scan も
    /// 自分の入れ子はもう知っている（XML は `Closing`、sxml は括弧、
    /// fsb は字下げ、CE は `{ }`）。
    Depth: int }

/// 木のノード 1 つ を字の上で光らせる範囲。**0 起点 の文字数で、`Stop` は含まない。**
///
/// **3 対 持っているのは、色ごとに要る幅が違うため。**
///
///     名前    走っている場所（黄）。再開点は 100% が `wait` で、葉なので
///             名前で足りる
///     開き札  撃った場所（緑）。**`fire` は撃たれる弾の一生を抱える**ので、
///             要素まるごとだと中央 5 行 / 最大 71 行 が染まり、
///             追っている弾の現在地（黄）を飲む（同梱 176 本 の 1,289 件 で測った）
///     閉じ札  上の対。**持たない表記では開き札と同じ範囲**が入る
type NodeSpan =
  { NameStart: int
    NameStop: int
    OpenStart: int
    OpenStop: int
    /// 閉じ札。**`sxml` と `fsb` は持たない**（括弧 1 組 / 字下げ）ので、
    /// そこでは開き札と同じ範囲。**呼ぶ側は同じかどうかを見て 1 つ に畳む** ——
    /// 同じ範囲を 2 枚 重ねると、下地が半透明なので色が濃くなる
    CloseStart: int
    CloseStop: int }

/// 式が書ける要素の中身（v4.1）。**`#PCDATA` を取る要素の、字のほう。**
///
/// `TagHit` は札の中身（名前と属性）しか持たない —— **式は札のあいだ に在る**
/// ので、別に拾う。
///
/// **位置は 0 起点 の文字数で、`Stop` は含まない。** 行桁へ直すのは
/// `Scan.lineColumn` で、読む側がやる（`TextHit` は表記に依らない）。
///
/// **取り方は表記ごとに違う**（XML は札のあいだ、ほかの 3 つ は引用符の中）
/// が、**答えは同じ 1 つ の字。**
type TextHit =
  { TagName: string
    Text: string
    Start: int
    Stop: int }

/// カーソルの居場所。**そこで何を打てるか。**
type Context =
  /// 本文。直近に開いている要素（無ければ根の外）
  | InContent of string option
  /// 属性名を打つところ。XML なら `<name `、sxml なら `(name (@ `
  | InStartTag of string
  /// 属性値の中。XML なら `<name attr="`、sxml なら `(name (@ (attr "`
  | InAttrValue of string * string

/// カーソルの**下**に在るもの。`Context` が「そこで何を打てるか」なのに対して、
/// こちらは「いま何の上に居るか」。hover が引く
type Token =
  /// 要素名
  | Element of string
  /// 属性名（要素名, 属性名）
  | Attribute of string * string
  /// 属性値（要素名, 属性名, 値）
  | AttrValue of string * string * string
  /// 何の上でもない。本文・空白・引用符そのもの・コメントの中
  | Nothing

/// **どの表記でも同じ「語の数え方」。**
///
/// XML の `<fire` も sxml の `(fire` も、名前に使える字は同じ。
/// **2 か所 に書くと、片方 だけ直したときに補完の置き換え幅が表記ごとに
/// ずれる** —— しかもずれるのは「入った字」であって、候補の一覧は
/// どちらも正しく見える。
module Scan =

  /// 木のノードになる札の、**光らせる範囲**を並べる（`NodeSpan`）。
  ///
  /// **字の位置は木に無い**ので、走行中のノードを字へ結ぶには順番しかない
  /// （v2.9 で測った。木の k 番目 と札の k 番目 が 3 表記 で 176 / 176 揃う）。
  ///
  /// **ノードになる名前を引数で受け取る。** ここに表を書くと、DTD に要素が
  /// 増えたときここだけが古びる —— しかも増えた側は「ノードでない」に落ちて、
  /// **黙って添字がずれる。** 正本は `Core/DTD.fs` の腕
  /// （`NodeOrder.names` が reflection で引いて渡す）。
  ///
  /// **要素まるごとの範囲は返さない。** `fire` は撃たれる弾の一生を抱えるので、
  /// 中央 5 行 / 最大 71 行 が染まる（同梱 176 本 の 1,289 件 を数えた）。
  /// 出すのは名前・開き札・閉じ札 の 3 対 で、**中身は染めない。**
  ///
  /// **1 つ ずつ引かずに並びで返す。** 呼ぶ側は窓が開いているあいだ本文が
  /// 変わらない（変わった瞬間に窓を閉じる）ので、走査は窓ごとに 1 回 で済む
  let nodeSpans (src: string) (tags: TagHit list) (nodes: string list) =
    if List.isEmpty nodes then []
    else
      let ok = Set.ofList nodes
      let all = List.toArray tags

      /// その位置を含む行の末尾（`\r\n` なら `\r` の手前）
      let lineEnd (from: int) =
        let mutable k = min from src.Length
        while k < src.Length && src.[k] <> '\n' do k <- k + 1
        if k > 0 && src.[k - 1] = '\r' then k - 1 else k

      /// 開き札の終わり。**「`Stop` の次」と「行末」の小さいほう。**
      ///
      /// `Stop` の意味は表記ごとに違う（`TagHit` の但し書き）が、
      /// **どれも開き札の終わりの上界になっている** ——
      ///
      ///     xml    開始札の `>`             -> `Stop + 1` が採られる
      ///     sxml   その括弧の `)`（要素の終わり） -> 行末 が採られる
      ///     fsb    行末                     -> 行末 が採られる
      ///
      /// **行末で切るのは、手で 1 行 に詰めて書かれたときの守り** ——
      /// `<fire><direction>...` と書かれた xml でも、緑は `<fire>` で止まる
      let openStop (t: TagHit) = min (t.Stop + 1) (lineEnd t.Start)

      // 開き札 -> 閉じ札 の対。**1 巡 で組む。**
      // `Closing` を持たない表記（sxml / fsb）では 1 件 も入らない
      let close = Dictionary<int, TagHit>()
      let stack = ResizeArray<int>()
      for k in 0 .. all.Length - 1 do
        let u = all.[k]
        if u.SelfClosing then ()
        elif u.Closing then
          // **名前が合うところまで戻す。** 閉じ忘れが在っても止まらない ——
          // 本文は打っている途中なので、閉じていない札が普通に在る
          let mutable j = stack.Count - 1
          while j >= 0 && all.[stack.[j]].TagName <> u.TagName do j <- j - 1
          if j >= 0 then
            close.[stack.[j]] <- u
            stack.RemoveRange(j, stack.Count - j)
        else stack.Add k

      [ for i in 0 .. all.Length - 1 do
          let t = all.[i]
          if not t.Closing && ok.Contains t.TagName then
            let os = openStop t
            let cs, ce =
              match close.TryGetValue i with
              | true, u -> u.Start, min (u.Stop + 1) (lineEnd u.Start)
              | _ -> t.Start, os
            { NameStart = t.NameStart
              NameStop = t.NameStop
              OpenStart = t.Start
              OpenStop = os
              CloseStart = cs
              CloseStop = ce } ]

  let isNameChar (c: char) =
    Char.IsLetterOrDigit c || c = '_' || c = '-' || c = '.' || c = ':'

  /// カーソルの手前にある「いま打っている名前」の長さ。
  /// **Monaco の語の定義に頼らない** —— 何が名前かを知っているのは言語のほう
  let nameLenBefore (src: string) (offset: int) =
    let mutable k = min offset src.Length
    while k > 0 && isNameChar src.[k - 1] do
      k <- k - 1
    (min offset src.Length) - k

  /// その位置を含む行の頭（0 起点 の文字数）。
  ///
  /// **`\r` を気にしない。** 行の頭を出すだけなので、行末が `\r\n` でも
  /// 頭の位置は変わらない
  let lineStart (src: string) (offset: int) =
    let at = max 0 (min offset src.Length)
    src.LastIndexOf('\n', max 0 (at - 1)) + 1

  /// 行頭からの桁（0 起点）。**字下げを測るのに使う**
  let columnOf (src: string) (offset: int) =
    let at = max 0 (min offset src.Length)
    at - lineStart src at

  /// 0 起点 の文字数を、**1 起点 の行と桁**にする（Monaco の行桁）。
  ///
  /// 波線や直し方の位置は表記ごとの数え方が既に持っているが、
  /// **本文へ何かを挿す位置**はそこに無い —— 札の位置（0 起点）からここで作る
  let lineColumn (src: string) (offset: int) : struct (int * int) =
    let at = max 0 (min offset src.Length)
    let mutable line = 1
    let mutable i = 0
    while i < at do
      if src.[i] = '\n' then line <- line + 1
      i <- i + 1
    struct (line, at - lineStart src at + 1)

  /// その位置より手前 が、その行では空白しか無いか。
  /// **行の頭に挿してよいか**の判定に使う
  let blankBefore (src: string) (offset: int) =
    let at = max 0 (min offset src.Length)
    let head = lineStart src at
    let mutable i = head
    let mutable ok = true
    while i < at do
      if src.[i] <> ' ' && src.[i] <> '\t' then ok <- false
      i <- i + 1
    ok

  /// 式の候補を置き換える長さ。名前のぶんに、手前の `$` が在ればそれも足す。
  /// **`$` を含めないと `$` + `$rand` で `$$rand` になる**
  let exprLenBefore (src: string) (offset: int) =
    let n = nameLenBefore src offset
    let at = (min offset src.Length) - n
    if at > 0 && src.[at - 1] = '$' then n + 1 else n

  let private describeContext (context: Context) =
    match context with
    | InContent None -> "content()"
    | InContent (Some parent) -> "content(" + parent + ")"
    | InStartTag name -> "startTag(" + name + ")"
    | InAttrValue (element, attr) -> "attrValue(" + element + "," + attr + ")"

  let private describeToken (token: Token) =
    match token with
    | Nothing -> "nothing"
    | Element name -> "element(" + name + ")"
    | Attribute (element, attr) -> "attribute(" + element + "," + attr + ")"
    | AttrValue (element, attr, value) -> "attrValue(" + element + "," + attr + "," + value + ")"

  /// 位置まわりの答えを 1 行 の字にする。**門（`guard-fable-parity.ps1`）のための口。**
  ///
  /// ここだけ別に出すのは、**`LastIndexOf(char, int)` の向き**のように
  /// runtime ごとに割れうるものが混ざっているから ——
  /// 割れても片方 では正しく動くので、走らせて突き合わせないと出ない。
  let describePosition (src: string) (offset: int) : string =
    let struct (line, column) = lineColumn src offset
    "lineStart="
    + string (lineStart src offset)
    + " columnOf="
    + string (columnOf src offset)
    + " line="
    + string line
    + " column="
    + string column
    + " blankBefore="
    + (if blankBefore src offset then "1" else "0")

  /// スキャナ 1 本 が出す答えを、全部 1 行 の字にする。
  ///
  /// **門（`guard-fable-parity.ps1`）のための口。** 同じソースが .NET と
  /// node の 2 つ で走るので、両方 でこれを呼んで突き合わせる。
  ///
  /// **字にするところは 1 本。** 突き合わせる側で組み立てると、組み方のほうが
  /// 食い違って「中身は同じなのに赤」「違うのに緑」になる。表記ごとに組んでも
  /// 同じことが起きる —— **同じ居場所を違う字で書けば、食い違いが表記の差に見える。**
  let describe (hits: TagHit list) (context: Context) (token: Token) (nameLen: int) (exprLen: int) : string =
    let sb = StringBuilder()
    let add (s: string) = sb.Append s |> ignore
    for t in hits do
      add t.TagName
      if t.Closing then add "/close"
      if t.SelfClosing then add "/self"
      add "@"
      add (string t.Start)
      add "-"
      add (string t.Stop)
      // 入れ子の深さ（v2.4）。**知り方は表記ごとに違う**（XML は閉じ札、
      // sxml は再帰、fsb は字下げ、CE は `{ }`）ので、2 つ の runtime で
      // 割れうるところが 4 本 に増えた
      add "^"
      add (string t.Depth)
      for a in t.Attrs do
        add " "
        add a.AttrName
        add "="
        add a.Value
        add "["
        add (string a.Line)
        add ":"
        add (string a.Column)
        add "-"
        add (string a.EndColumn)
        add "]"
      add ";"
    add " ctx="
    add (describeContext context)
    add " token="
    add (describeToken token)
    add " nameLen="
    add (string nameLen)
    add " exprLen="
    add (string exprLen)
    sb.ToString()
