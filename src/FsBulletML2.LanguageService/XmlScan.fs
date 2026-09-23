namespace FsBulletML2.LanguageService

open System
open System.Text
// 自分の名前空間を開き直す。 `Token` を `Scan.fs` へ移した時点で、
// `Attribute` が名前空間の側に落ちて `System.Attribute` に負ける ——
// 型が同じファイルに在るうちは勝っていたので、移して初めて出る形
open FsBulletML2.LanguageService

/// XML の字を数える 1 本。補完と波線で分けない。
module XmlScan =

  let private isSpace c = c = ' ' || c = '\t' || c = '\r' || c = '\n'

  /// タグを頭から全部 拾う。木は組まない。
  /// 引用符の中は名前も `>` も数えるな。閉じ札も拾う。居場所に要る。
  let tags (src: string) : TagHit list =
    let hits = ResizeArray<TagHit>()
    let mutable i = 0
    let mutable line = 1
    let mutable lineStart = 0
    // 深さは開始札と閉じ札。`Stop` は開始札の `>` なので、包含では子を覆わない。
    let mutable depth = 0
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
        while i < src.Length && Scan.isNameChar src.[i] do advance ()
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
          elif Scan.isNameChar src.[i] then
            let aFrom = i
            while i < src.Length && Scan.isNameChar src.[i] do advance ()
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
                // 閉じ引用符が無い属性は捨てる。手前まで拾った属性は残す。
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
          // 閉じ札は開いていた側と同じ深さ。閉じすぎても負にしない。
          if closing then depth <- max 0 (depth - 1)
          hits.Add
            { TagName = name
              Attrs = List.ofSeq attrs
              Closing = closing
              SelfClosing = selfClosing
              Start = start
              Stop = if stop < 0 then src.Length else stop
              NameStart = nameFrom
              NameStop = nameStop
              Depth = depth }
          if not closing && not selfClosing then depth <- depth + 1
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
      // まだ要素名を打っている（`<act|`）。要素の候補を出したいので content 扱い
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
  /// カーソルより後ろのタグは札に効かせるな。どこに居ても根に見える。
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
          // 自己閉じは積まない。積むとそこから先がずっと中に居る。
          elif not t.SelfClosing then stack.Add t.TagName
    let content () = InContent(if stack.Count > 0 then Some stack.[stack.Count - 1] else None)
    match inside with
    | None -> content ()
    | Some t ->
      match inTag (src.Substring(t.Start, cursor - t.Start)) with
      | Some c -> c
      | None -> content ()

  /// カーソルの下に在るものを出す。hover が引く。
  /// `contextAt` と逆。語の途中でもその語を返す。
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

  /// この 1 本 が出す答えを、全部 1 行 の字にする。門のための口。
  /// 字にするところは `Scan.describe`（sxml の 2 本 目 と共通）
  let describe (src: string) (cursor: int) : string =
    Scan.describe (tags src) (contextAt src cursor) (tokenAt src cursor)
                  (Scan.nameLenBefore src cursor) (Scan.exprLenBefore src cursor)

  /// 式が書ける要素の中身（v4.1）。札のあいだ の字。
  /// 拾う要素名は呼ぶ側が渡す。器に表を書かない。
  let texts (src: string) (names: string list) : TextHit list =
    tags src
    |> List.choose (fun t ->
         if t.Closing || t.SelfClosing || not (List.contains t.TagName names) then None
         else
           let from = t.Stop + 1
           if from >= src.Length then None
           else
             let mutable j = from
             while j < src.Length && src.[j] <> '<' do j <- j + 1
             let text = src.Substring(from, j - from)
             if text.Trim() = "" then None
             else Some { TagName = t.TagName; Text = text; Start = from; Stop = j })