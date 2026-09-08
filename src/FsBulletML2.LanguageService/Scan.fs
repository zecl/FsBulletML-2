namespace FsBulletML2.LanguageService

open System
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
    NameStop: int }

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
