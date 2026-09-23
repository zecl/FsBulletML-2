namespace FsBulletML2.LanguageService

open System
// `Attribute` が `System.Attribute` に負ける。理由は `XmlScan.fs` の同じ行
open FsBulletML2.LanguageService

/// S 式の字を数える 1 本。XML 側と同じ型を返す。
/// 数える形は `(name (@ (attr "値") ...) "本文" 子...)` だけ。
module SxmlScan =

  /// 括弧 1 組。属性ブロックの位置はカーソルの居場所用。
  /// 参照を数える側には `TagHit` だけ渡す。
  type Form =
    { Hit: TagHit
      /// `(@` の `(` の位置。属性ブロックが無ければ -1
      AttrsStart: int
      /// 属性ブロックを閉じる `)` の位置。閉じていなければ本文の末尾。無ければ -1
      AttrsStop: int }

  /// 括弧を頭から全部 拾う。`Start` / `Stop` が子を覆う。
  /// 閉じ引用符が無い属性も捨てるな。こちらは別走査を持たない。
  let forms (src: string) : Form list =
    let out = ResizeArray<Form>()
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
    let skipSpace () =
      while i < src.Length && Char.IsWhiteSpace src.[i] do advance ()
    let readName () =
      let from = i
      while i < src.Length && Scan.isNameChar src.[i] do advance ()
      (from, i)
    /// `i` は開き引用符の上。読み終えると閉じ引用符の次（無ければ末尾）
    let readString () =
      advance () // 開き引用符
      let from = i
      let l = line
      let c = col i
      while i < src.Length && src.[i] <> '"' do advance ()
      let stop = i
      // 中身が空でも 1 文字 ぶんは指せるようにする
      let endCol = max (c + 1) (col i)
      if i < src.Length then advance () // 閉じ引用符
      (from, stop, l, c, endCol)

    /// 属性ブロック。`i` は `(@` の `(` の上。返すのは (属性, 閉じ括弧の位置)
    let readAttrs () =
      let start = i
      advance () // '('
      advance () // '@'
      let attrs = ResizeArray<AttrHit>()
      let mutable stop = -1
      while stop < 0 && i < src.Length do
        skipSpace ()
        if i >= src.Length then ()
        elif src.[i] = ')' then
          stop <- i
          advance ()
        elif src.[i] = '(' then
          advance ()
          skipSpace ()
          let (nameFrom, nameStop) = readName ()
          skipSpace ()
          if i < src.Length && src.[i] = '"' then
            let (vFrom, vStop, vLine, vCol, vEndCol) = readString ()
            if nameStop > nameFrom then
              attrs.Add
                { AttrName = src.Substring(nameFrom, nameStop - nameFrom)
                  Value = src.Substring(vFrom, vStop - vFrom)
                  Line = vLine
                  Column = vCol
                  EndColumn = vEndCol
                  NameStart = nameFrom
                  NameStop = nameStop
                  ValueStart = vFrom
                  ValueStop = vStop }
          skipSpace ()
          if i < src.Length && src.[i] = ')' then advance ()
        else advance ()
      (List.ofSeq attrs, (if stop < 0 then src.Length else stop), start)

    /// 括弧 1 組。`i` は `(` の上。読み終えると `)` の次。
    /// 深さは再帰の段。sxml は包含で木になる。
    let rec readForm (depth: int) =
      let start = i
      advance () // '('
      skipSpace ()
      let (nameFrom, nameStop) = readName ()
      let mutable attrs = []
      let mutable attrsStart = -1
      let mutable attrsStop = -1
      let mutable stop = -1
      while stop < 0 && i < src.Length do
        if Char.IsWhiteSpace src.[i] then advance ()
        elif src.[i] = ')' then
          stop <- i
          advance ()
        elif src.[i] = '(' then
          if i + 1 < src.Length && src.[i + 1] = '@' && attrsStart < 0 then
            let (a, s, st) = readAttrs ()
            attrs <- a
            attrsStart <- st
            attrsStop <- s
          else readForm (depth + 1)
        elif src.[i] = '"' then readString () |> ignore
        else advance ()
      // 名前が空なら足さない。効くのは `tags` の並び。居場所は変わらない。
      if nameStop > nameFrom then
        out.Add
          { Hit =
              { TagName = src.Substring(nameFrom, nameStop - nameFrom)
                Attrs = attrs
                // 括弧 1 組 が開きも閉じも兼ねる。どちらも常に false
                Closing = false
                SelfClosing = false
                Start = start
                Stop = if stop < 0 then src.Length else stop
                NameStart = nameFrom
                NameStop = nameStop
                Depth = depth }
            AttrsStart = attrsStart
            AttrsStop = attrsStop }

    while i < src.Length do
      if src.[i] = '(' then readForm 0 else advance ()
    // 前順に並べ直す。後順のままだと「いちばん内側 = 最後」が逆になる。
    out |> Seq.sortBy (fun f -> f.Hit.Start) |> List.ofSeq

  /// 参照を数える側が読む形。表記を知らない側へ渡すのはこちら。
  let tags (src: string) : TagHit list = forms src |> List.map (fun f -> f.Hit)

  /// カーソルを覆う括弧のうち、いちばん内側。前順なので最後
  let private innermost (all: Form list) (pick: TagHit -> bool) =
    all |> List.filter (fun f -> pick f.Hit) |> List.tryLast

  /// カーソルの居場所を出す。名前の途中も `(` の直後も本文扱い。
  let contextAt (src: string) (offset: int) : Context =
    let cursor = max 0 (min offset src.Length)
    let all = forms src
    match innermost all (fun h -> cursor > h.Start && cursor <= h.Stop) with
    | None -> InContent None
    | Some f ->
      let h = f.Hit
      if cursor <= h.NameStop then
        // 名前を打っている途中。候補を出す先は外側の中身
        InContent(
          innermost all (fun g -> g.Start < h.Start && h.Stop <= g.Stop)
          |> Option.map (fun g -> g.Hit.TagName))
      elif f.AttrsStart >= 0 && cursor > f.AttrsStart && cursor <= f.AttrsStop then
        match h.Attrs |> List.tryFind (fun a -> cursor >= a.ValueStart && cursor <= a.ValueStop) with
        | Some a -> InAttrValue(h.TagName, a.AttrName)
        | None -> InStartTag h.TagName
      else InContent(Some h.TagName)

  /// カーソルの下に在るものを出す。hover が引く。
  /// 指すのはその 1 文字。括弧も引用符も空白も本文も何でもない。
  let tokenAt (src: string) (offset: int) : Token =
    if src.Length = 0 then Nothing
    else
      let p = max 0 (min offset (src.Length - 1))
      match innermost (forms src) (fun h -> p >= h.Start && p <= h.Stop) with
      | None -> Nothing
      | Some f ->
        let h = f.Hit
        if p >= h.NameStart && p < h.NameStop then Element h.TagName
        else
          match h.Attrs |> List.tryFind (fun a -> p >= a.NameStart && p < a.NameStop) with
          | Some a -> Attribute(h.TagName, a.AttrName)
          | None ->
            match h.Attrs |> List.tryFind (fun a -> p >= a.ValueStart && p < a.ValueStop) with
            | Some a -> AttrValue(h.TagName, a.AttrName, a.Value)
            | None -> Nothing

  /// この 1 本 が出す答えを、全部 1 行 の字にする。門のための口。
  /// 字にするところは `Scan.describe`（XML 側と共通）
  let describe (src: string) (cursor: int) : string =
    Scan.describe (tags src) (contextAt src cursor) (tokenAt src cursor)
                  (Scan.nameLenBefore src cursor) (Scan.exprLenBefore src cursor)

  /// 式が書ける要素の中身（v4.1）。括弧の中の引用符。
  /// 深さ 0 だけ拾う。属性値は `(@ ...)` の中なので取り違えない。
  let texts (src: string) (names: string list) : TextHit list =
    tags src
    |> List.choose (fun t ->
         if not (List.contains t.TagName names) then None
         else
           let stop = min t.Stop (src.Length - 1)
           let mutable i = t.NameStop
           let mutable depth = 0
           let mutable found = None
           while found.IsNone && i <= stop do
             let c = src.[i]
             if c = '(' then
               depth <- depth + 1
               i <- i + 1
             elif c = ')' then
               depth <- depth - 1
               i <- i + 1
             elif c = '"' && depth = 0 then
               let from = i + 1
               let mutable j = from
               while j < src.Length && src.[j] <> '"' do j <- j + 1
               let text = src.Substring(from, j - from)
               found <- (if text.Trim() = "" then None
                         else Some { TagName = t.TagName; Text = text; Start = from; Stop = j })
               i <- j + 1
             else i <- i + 1
           found)