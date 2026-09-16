namespace FsBulletML2.LanguageService

open System
open System.Collections.Generic
open System.Text

/// 開始札の中の属性 1 つ。
///
/// `Line` / `Column` / `EndColumn` は波線を引くための 1 起点で、
/// 指すのは値の中身（引用符の内側）。`*Start` / `*Stop` は
/// カーソルの下を当てるための 0 起点 の文字数で、`Stop` は含まない。
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

/// 要素 1 つ。表記をまたいで同じ型。
///
/// `Stop` の意味は表記ごとに違う（XML は開始札の `>`、sxml は括弧の `)`）。
/// 揃えられない —— XML は「札が閉じる」と「要素が閉じる」が別の位置に在る。
///
/// 読む側が `Start` / `Stop` を要るのはカーソルの居場所を出すときだけで、
/// そこは表記ごとの実装の中に居る。
type TagHit =
  { TagName: string
    Attrs: AttrHit list
    /// `</foo>` か。sxml では常に false —— 括弧 1 組 が開きも閉じも兼ねる
    Closing: bool
    /// `<foo/>` か。sxml では常に false（同上）
    SelfClosing: bool
    /// 要素が始まる位置（0 起点 の文字数）。XML なら `<`、sxml なら `(`
    Start: int
    /// 上の但し書きの位置
    Stop: int
    /// 要素名の範囲。`Stop` は含まない
    NameStart: int
    NameStop: int
    /// 入れ子の深さ。根が 0（v2.4）。閉じ札は開いていた側と同じ数。
    ///
    /// 知り方は表記ごとに違うが、答えは 1 つ の数。 だから数のほうを
    /// 持たせて、知り方はそれぞれの Scan に閉じる。
    ///
    /// `Stop` の意味を揃えて代わりにしない（揃えると `contextAt` が壊れる）
    Depth: int }

/// 木のノード 1 つ を字の上で光らせる範囲。0 起点 の文字数で、`Stop` は含まない。
///
/// 3 対 持っているのは、色ごとに要る幅が違うため（名前＝走っている場所、
/// 開き札＝撃った場所、閉じ札＝その対）。
type NodeSpan =
  { NameStart: int
    NameStop: int
    OpenStart: int
    OpenStop: int
    /// 閉じ札。`sxml` と `fsb` は持たない（括弧 1 組 / 字下げ）ので、
    /// そこでは開き札と同じ範囲。呼ぶ側は同じかどうかを見て 1 つ に畳む ——
    /// 同じ範囲を 2 枚 重ねると、下地が半透明なので色が濃くなる
    CloseStart: int
    CloseStop: int }

/// 式が書ける要素の中身（v4.1）。`#PCDATA` を取る要素の、字のほう。
///
/// `TagHit` は札の中身しか持たない —— 式は札のあいだ に在るので別に拾う。
///
/// 位置は 0 起点 の文字数で、`Stop` は含まない。
/// 取り方は表記ごとに違うが、答えは同じ 1 つ の字。
type TextHit =
  { TagName: string
    Text: string
    Start: int
    Stop: int }

/// カーソルの居場所。そこで何を打てるか。
type Context =
  /// 本文。直近に開いている要素（無ければ根の外）
  | InContent of string option
  /// 属性名を打つところ。XML なら `<name `、sxml なら `(name (@ `
  | InStartTag of string
  /// 属性値の中。XML なら `<name attr="`、sxml なら `(name (@ (attr "`
  | InAttrValue of string * string

/// カーソルの下に在るもの。`Context` が「そこで何を打てるか」なのに対して、
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

/// どの表記でも同じ「語の数え方」。
///
/// XML の `<fire` も sxml の `(fire` も、名前に使える字は同じ。
/// 2 か所 に書くと、片方 だけ直したときに補完の置き換え幅が表記ごとに
/// ずれる —— しかもずれるのは「入った字」であって、候補の一覧は
/// どちらも正しく見える。
module Scan =

  /// 木のノードになる札の、光らせる範囲を並べる（`NodeSpan`）。
  ///
  /// 字の位置は木に無いので、走行中のノードを字へ結ぶには順番しかない。
  ///
  /// ノードになる名前を引数で受け取る（ここに表を書くと、DTD に要素が
  /// 増えたとき黙って添字がずれる）。正本は `Core/DTD.fs` の腕。
  ///
  /// 要素まるごとの範囲は返さない（`fire` は弾の一生を抱えるので染まりすぎる）。
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

      /// 開き札の終わり。「`Stop` の次」と「行末」の小さいほう
      /// （`Stop` の意味は表記ごとに違うが、どれも上界になっている）。
      ///
      /// 行末で切るのは、手で 1 行 に詰めて書かれたときの守り
      let openStop (t: TagHit) = min (t.Stop + 1) (lineEnd t.Start)

      // 開き札 -> 閉じ札 の対。1 巡 で組む。
      // `Closing` を持たない表記（sxml / fsb）では 1 件 も入らない
      let close = Dictionary<int, TagHit>()
      let stack = ResizeArray<int>()
      for k in 0 .. all.Length - 1 do
        let u = all.[k]
        if u.SelfClosing then ()
        elif u.Closing then
          // 名前が合うところまで戻す。 閉じ忘れが在っても止まらない ——
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
  /// Monaco の語の定義に頼らない —— 何が名前かを知っているのは言語のほう
  let nameLenBefore (src: string) (offset: int) =
    let mutable k = min offset src.Length
    while k > 0 && isNameChar src.[k - 1] do
      k <- k - 1
    (min offset src.Length) - k

  /// その位置を含む行の頭（0 起点 の文字数）。
  ///
  /// `\r` を気にしない。 行の頭を出すだけなので、行末が `\r\n` でも
  /// 頭の位置は変わらない
  let lineStart (src: string) (offset: int) =
    let at = max 0 (min offset src.Length)
    src.LastIndexOf('\n', max 0 (at - 1)) + 1

  /// 行頭からの桁（0 起点）。字下げを測るのに使う
  let columnOf (src: string) (offset: int) =
    let at = max 0 (min offset src.Length)
    at - lineStart src at

  /// 昇順の位置を、本文 1 巡 で行桁へ直す（v4.4）。
  ///
  /// 渡す位置は昇順でなければならない。 崩れていたら頭に戻って数え直す
  /// （正しさは保つが遅くなる）—— 黙って間違えるより遅いほうを選ぶ。
  ///
  /// 内包表記で書かない（v4.9）—— Fable は内包の中の `while` を
  /// enumerator の鎖に焼き、同じソースで .NET 0.50 ms / JS 9.8 ms になる。
  /// 隣の `lineColumn` が速いのは、内包の外に在るから。
  let lineColumnsAscending (src: string) (offsets: int list) : struct (int * int) list =
    let out = ResizeArray<struct (int * int)>()
    let mutable line = 1
    let mutable lineStart = 0
    let mutable at = 0
    for raw in offsets do
      let off = max 0 (min raw src.Length)
      // 昇順が崩れていたら頭から数え直す
      if off < at then
        line <- 1
        lineStart <- 0
        at <- 0
      while at < off do
        if src.[at] = '\n' then
          line <- line + 1
          lineStart <- at + 1
        at <- at + 1
      out.Add(struct (line, off - lineStart + 1))
    List.ofSeq out

  /// 位置 -> 行桁 を何度も・行き来して引くための表（v4.9）。
  ///
  /// 改行の位置を 1 度 だけ数えて、あとは二分探索。
  ///
  /// `lineColumnsAscending` との違いは順 —— あちらは昇順でしか引けないが
  /// 表を持たない。こちらは行き来してよい代わりに、行の数だけ場所を取る。
  let lineColumnLookup (src: string) : int -> struct (int * int) =
    let n = if isNull src then 0 else src.Length
    let starts = ResizeArray<int>()
    starts.Add 0
    let mutable i = 0
    while i < n do
      if src.[i] = '\n' then starts.Add(i + 1)
      i <- i + 1
    fun offset ->
      let at = max 0 (min offset n)
      // その位置以下 でいちばん後ろ の行頭
      let mutable lo = 0
      let mutable hi = starts.Count - 1
      while lo < hi do
        let mid = (lo + hi + 1) / 2
        if starts.[mid] <= at then lo <- mid else hi <- mid - 1
      struct (lo + 1, at - starts.[lo] + 1)

  let lineColumn (src: string) (offset: int) : struct (int * int) =
    let at = max 0 (min offset src.Length)
    let mutable line = 1
    let mutable i = 0
    while i < at do
      if src.[i] = '\n' then line <- line + 1
      i <- i + 1
    struct (line, at - lineStart src at + 1)

  /// その位置より手前 が、その行では空白しか無いか。
  /// 行の頭に挿してよいかの判定に使う
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
  /// `$` を含めないと `$` + `$rand` で `$$rand` になる
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

  /// 位置まわりの答えを 1 行 の字にする。門（`guard-fable-parity.ps1`）のための口。
  ///
  /// ここだけ別に出すのは、`LastIndexOf(char, int)` の向きのように
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

  /// スキャナ 1 本 が出す答えを、全部 1 行 の字にする。門のための口。
  ///
  /// 字にするところは 1 本。 突き合わせる側で組み立てると、組み方のほうが
  /// 食い違って「中身は同じなのに赤」「違うのに緑」になる ——
  /// 同じ居場所を違う字で書けば、食い違いが表記の差に見える。
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
      // 入れ子の深さ（v2.4）。知り方が表記ごとに違うので、
      // 2 つ の runtime で割れうるところが 4 本 に増えた
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
