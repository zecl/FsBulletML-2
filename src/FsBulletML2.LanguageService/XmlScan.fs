namespace FsBulletML2.LanguageService

open System
open System.Text

/// 開始タグの中の属性 1 つ。
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

/// タグ 1 つ。
///
/// **閉じ札も拾う。** 参照の欠けを数えるだけなら開始札で足りるが、
/// カーソルの居場所を出すには「いま開いている要素」が要る。
///
/// 飛ばすもの（そもそも出てこない）: コメント / 宣言 / CDATA。
type TagHit =
  { TagName: string
    Attrs: AttrHit list
    /// `</foo>` か
    Closing: bool
    /// `<foo/>` か。**開始札として出したうえで旗を立てる** ——
    /// 積まないことは使う側が決める
    SelfClosing: bool
    /// `<` の位置（0 起点 の文字数）
    Start: int
    /// `>` の位置。閉じていなければ本文の末尾
    Stop: int
    /// 要素名の範囲。`Stop` は含まない
    NameStart: int
    NameStop: int }

/// カーソルの居場所
type Context =
  /// 本文。直近に開いている要素（無ければ根の外）
  | InContent of string option
  /// `<name ` の中。属性名を出す
  | InStartTag of string
  /// `<name attr="` の中。属性値を出す
  | InAttrValue of string * string

/// カーソルの**下**に在るもの。`Context` が「そこで何を打てるか」なのに対して、
/// こちらは「いま何の上に居るか」。hover が引く
type Token =
  /// 要素名。開き札でも閉じ札でも同じ
  | Element of string
  /// 属性名（要素名, 属性名）
  | Attribute of string * string
  /// 属性値（要素名, 属性名, 値）
  | AttrValue of string * string * string
  /// 何の上でもない。本文・空白・引用符そのもの・コメントの中
  | Nothing

/// **XML の字を数える 1 本。**
///
/// 前は 2 本 在った —— 補完のための `contextAt`（Fable 側、カーソルの手前だけ）と、
/// 波線のための `tags`（host 側、全タグを位置つき）。同じ罠
/// （引用符の中の `>` / コメント / CDATA / 閉じ引用符の欠け）を
/// 2 か所 で踏むので 1 本 にした。
///
/// **パーサは使わない。** 打っている途中の XML は必ず壊れているので、
/// 頭から数えるだけにする。壊れた本文でも最後まで走る。
///
/// このファイルは `fable/` に在るが**中身は純 F#**（`Fable.Core` に触らない）。
/// Fable が焼き、host と `Parser.Tests` が `Link` で借りる。
/// **`fable/` の外へ出すと、Fable の出力が `wwwroot/js/` の外へ落ちる**
/// （`../XmlScan.fs` を引くと `wwwroot/XmlScan.js` になる。実測）。
///
/// **internal にしない。** 門（`guard-fable-parity.ps1`）が .NET と node の
/// 両方 から呼んで答えを突き合わせるので、外から見える必要がある
module XmlScan =

  let isNameChar (c: char) =
    Char.IsLetterOrDigit c || c = '_' || c = '-' || c = '.' || c = ':'

  let private isSpace c = c = ' ' || c = '\t' || c = '\r' || c = '\n'

  /// タグを頭から全部 拾う。**木は組まない。**
  ///
  /// **引用符の中は名前も `>` も数えない** —— 属性値に `>` を書ける。
  let tags (src: string) : TagHit list =
    let hits = ResizeArray<TagHit>()
    let mutable i = 0
    let mutable line = 1
    let mutable lineStart = 0
    // 1 起点 の桁。行頭からの差に 1 を足す
    let col p = p - lineStart + 1
    let advance () =
      if src.[i] = '\n' then
        line <- line + 1
        lineStart <- i + 1
      i <- i + 1
    /// `s` が来るまで進める（`s` の分も食う）
    let skipUntil (s: string) =
      while i < src.Length && not (i + s.Length <= src.Length && src.Substring(i, s.Length) = s) do
        advance ()
      for _ in 1 .. min s.Length (src.Length - i) do
        advance ()

    while i < src.Length do
      if src.[i] <> '<' then advance ()
      elif i + 4 <= src.Length && src.Substring(i, 4) = "<!--" then skipUntil "-->"
      elif i + 9 <= src.Length && src.Substring(i, 9) = "<![CDATA[" then skipUntil "]]>"
      elif i + 2 <= src.Length && (src.[i + 1] = '!' || src.[i + 1] = '?') then skipUntil ">"
      else
        let start = i
        advance () // '<'
        let closing = i < src.Length && src.[i] = '/'
        if closing then advance ()
        let nameFrom = i
        while i < src.Length && isNameChar src.[i] do advance ()
        let name = src.Substring(nameFrom, i - nameFrom)
        let nameStop = i
        let attrs = ResizeArray<AttrHit>()
        let mutable selfClosing = false
        let mutable stop = -1
        while stop < 0 && i < src.Length do
          if src.[i] = '>' then
            selfClosing <- i > start && src.[i - 1] = '/'
            stop <- i
            advance ()
          elif Char.IsWhiteSpace src.[i] || src.[i] = '/' then advance ()
          elif isNameChar src.[i] then
            let aFrom = i
            while i < src.Length && isNameChar src.[i] do advance ()
            let aName = src.Substring(aFrom, i - aFrom)
            let aNameStop = i
            while i < src.Length && Char.IsWhiteSpace src.[i] do advance ()
            if i < src.Length && src.[i] = '=' then
              advance ()
              while i < src.Length && Char.IsWhiteSpace src.[i] do advance ()
              if i < src.Length && (src.[i] = '"' || src.[i] = '\'') then
                let quote = src.[i]
                advance ()
                let vFrom = i
                let vLine = line
                let vCol = col i
                while i < src.Length && src.[i] <> quote do advance ()
                // 閉じ引用符が無いまま本文が尽きた。**この属性は捨てて、
                // タグはそこで打ち切る** —— 手前まで拾った属性は位置が
                // 本文と合っているので残す
                if i >= src.Length then stop <- src.Length
                else
                  attrs.Add
                    { AttrName = aName
                      Value = src.Substring(vFrom, i - vFrom)
                      Line = vLine
                      Column = vCol
                      // 中身が空でも 1 文字 ぶんは指せるようにする
                      EndColumn = max (vCol + 1) (col i)
                      NameStart = aFrom
                      NameStop = aNameStop
                      ValueStart = vFrom
                      ValueStop = i }
                  advance () // 閉じ引用符
          else advance ()
        if name <> "" then
          hits.Add
            { TagName = name
              Attrs = List.ofSeq attrs
              Closing = closing
              SelfClosing = selfClosing
              Start = start
              Stop = if stop < 0 then src.Length else stop
              NameStart = nameFrom
              NameStop = nameStop }
    List.ofSeq hits

  /// タグの中にカーソルが居るときの居場所。
  /// `text` は `<` からカーソルまで
  let private inTag (text: string) : Context option =
    if text.StartsWith("</", StringComparison.Ordinal)
       || text.StartsWith("<!", StringComparison.Ordinal)
       || text.StartsWith("<?", StringComparison.Ordinal) then None
    else
      let mutable k = 1
      while k < text.Length && not (isSpace text.[k] || text.[k] = '/' || text.[k] = '>') do
        k <- k + 1
      let name = text.Substring(1, k - 1)
      // まだ要素名を打っている（`<act|`）。**要素の候補を出したいので content 扱い**
      if k >= text.Length then None
      else
        // 属性の領域。引用符が開いたままならその中に居る
        let mutable m = k
        let mutable quote = '\000'
        let mutable lastAttr = ""
        let pending = StringBuilder()
        while m < text.Length do
          let c = text.[m]
          if quote <> '\000' then
            (if c = quote then quote <- '\000')
          elif c = '"' || c = '\'' then quote <- c
          elif c = '=' then
            lastAttr <- pending.ToString().Trim()
            pending.Clear() |> ignore
          elif isSpace c then pending.Clear() |> ignore
          else pending.Append c |> ignore
          m <- m + 1
        if quote <> '\000' then Some(InAttrValue(name, lastAttr)) else Some(InStartTag name)

  /// カーソルの居場所を出す。
  ///
  /// **カーソルより後ろのタグは札に効かせない。** 効かせると後ろの閉じ札まで
  /// 札を下ろしてしまい、どこに居ても「根」に見える（属性の補完は当たり、
  /// 本文の補完だけ静かに外れる）
  let contextAt (src: string) (offset: int) : Context =
    let cursor = max 0 (min offset src.Length)
    let all = tags src
    let stack = ResizeArray<string>()
    let mutable inside = None
    for t in all do
      if inside.IsNone then
        if cursor > t.Start && cursor <= t.Stop then
          inside <- Some t
        elif t.Stop < cursor then
          if t.Closing then
            (if stack.Count > 0 then stack.RemoveAt(stack.Count - 1))
          // **自己閉じは積まない。** 積むと `<bullet/>` から先が
          // ずっと bullet の中に居ることになる
          elif not t.SelfClosing then stack.Add t.TagName
    let content () = InContent(if stack.Count > 0 then Some stack.[stack.Count - 1] else None)
    match inside with
    | None -> content ()
    | Some t ->
      match inTag (src.Substring(t.Start, cursor - t.Start)) with
      | Some c -> c
      | None -> content ()

  /// カーソルの**下**に在るものを出す。hover が引く。
  ///
  /// **`contextAt` とは向きが違う。** あちらは「そこで何を打てるか」なので
  /// 手前だけを見て、名前を打っている途中なら本文扱いにする。こちらは
  /// 「いま何の上に居るか」なので、語の途中でもその語を返す。
  ///
  /// 指すのは**カーソルの位置に在る 1 文字**。だから
  ///
  ///   `<` の上 / `>` の上 / 引用符そのものの上   何でもない
  ///   要素名の最初の字と最後の字               その要素
  ///   タグの中の空白                           何でもない
  ///   コメント・CDATA・宣言・本文の字          何でもない
  ///
  /// 閉じ札の名前も要素として返す（`</fire>` に触っても fire の仕様が出る）
  let tokenAt (src: string) (offset: int) : Token =
    let p = max 0 (min offset (src.Length - 1))
    if src.Length = 0 then Nothing
    else
      match tags src |> List.tryFind (fun t -> p >= t.Start && p <= t.Stop) with
      | None -> Nothing
      | Some t ->
        if p >= t.NameStart && p < t.NameStop then Element t.TagName
        else
          match t.Attrs |> List.tryFind (fun a -> p >= a.NameStart && p < a.NameStop) with
          | Some a -> Attribute(t.TagName, a.AttrName)
          | None ->
            match t.Attrs |> List.tryFind (fun a -> p >= a.ValueStart && p < a.ValueStop) with
            | Some a -> AttrValue(t.TagName, a.AttrName, a.Value)
            | None -> Nothing

  /// カーソルの手前にある「いま打っている名前」の長さ。
  /// **Monaco の語の定義に頼らない** —— 何が名前かを知っているのは言語のほう
  let nameLenBefore (src: string) (offset: int) =
    let mutable k = min offset src.Length
    while k > 0 && isNameChar src.[k - 1] do
      k <- k - 1
    (min offset src.Length) - k

  /// 式の候補を置き換える長さ。名前のぶんに、手前の `$` が在ればそれも足す。
  /// **`$` を含めないと `$` + `$rand` で `$$rand` になる**
  let exprLenBefore (src: string) (offset: int) =
    let n = nameLenBefore src offset
    let at = (min offset src.Length) - n
    if at > 0 && src.[at - 1] = '$' then n + 1 else n

  /// この 1 本 が出す答えを、全部 1 行 の字にする。
  ///
  /// **門（`guard-fable-parity.ps1`）のための口。** 同じソースが .NET と
  /// node の 2 つ で走るので、両方 でこれを呼んで突き合わせる。
  ///
  /// **字にするところも 1 本 にしてある。** 突き合わせる側で組み立てると、
  /// 組み方のほうが食い違って「中身は同じなのに赤」「違うのに緑」になる。
  let describe (src: string) (cursor: int) : string =
    let sb = StringBuilder()
    let add (s: string) = sb.Append s |> ignore
    for t in tags src do
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
    match contextAt src cursor with
    | InContent None -> add "content()"
    | InContent (Some p) ->
      add "content("
      add p
      add ")"
    | InStartTag n ->
      add "startTag("
      add n
      add ")"
    | InAttrValue (e, a) ->
      add "attrValue("
      add e
      add ","
      add a
      add ")"
    add " token="
    match tokenAt src cursor with
    | Nothing -> add "nothing"
    | Element n ->
      add "element("
      add n
      add ")"
    | Attribute (e, a) ->
      add "attribute("
      add e
      add ","
      add a
      add ")"
    | AttrValue (e, a, v) ->
      add "attrValue("
      add e
      add ","
      add a
      add ","
      add v
      add ")"
    add " nameLen="
    add (string (nameLenBefore src cursor))
    add " exprLen="
    add (string (exprLenBefore src cursor))
    sb.ToString()
