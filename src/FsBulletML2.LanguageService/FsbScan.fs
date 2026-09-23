namespace FsBulletML2.LanguageService

open System
// `Attribute` が `System.Attribute` に負ける。理由は `XmlScan.fs` の同じ行
open FsBulletML2.LanguageService

/// インデント記法（fsb）の字を数える 1 本。XML 側と同じ型を返す。
/// 数えるのは `name attr="値"` まで。残りを黙って捨てる。
module FsbScan =

  /// 要素 1 つ ＝ 行 1 本。行と字下げはカーソルの居場所用。
  /// 参照を数える側には `TagHit` だけ渡す。
  type Row =
    { Hit: TagHit
      /// 行頭の空白の数。入れ子はこれだけで決まる
      Indent: int
      /// 行の頭（字下げの空白を含む）
      LineStart: int
      /// 行の末。改行と `\r` を含まない
      LineStop: int
      /// `:"…"` の中身。無ければ -1。ここは要素の #PCDATA ——
      /// XML の `<wait>3</wait>` に当たるので、カーソルが入れば式の候補が出る
      BodyStart: int
      BodyStop: int }

  /// 名前に使える字。`Scan.isNameChar` より狭い。
  /// `:` を通すと `direction:` が 1 つの名前になる。置き換え幅はあちらと別。
  let private isName (c: char) = Char.IsLetterOrDigit c || c = '_'

  /// 行を頭から全部 拾う。木は組まない。
  /// 閉じ引用符が無い値も捨てるな。候補が静かに消える。
  let rows (src: string) : Row list =
    let out = ResizeArray<Row>()
    let n = src.Length
    let mutable i = 0
    let mutable line = 1
    // 字下げの幅は決め打たない。`Stop` は行末なので、包含では木にならない。
    let indents = ResizeArray<int>()
    while i <= n do
      let lineStart = i
      let mutable j = i
      while j < n && src.[j] <> '\n' do j <- j + 1
      // `\r` は行に入れない。 入れると、末尾の属性値の桁が 1 ずれる
      let lineStop = if j > lineStart && src.[j - 1] = '\r' then j - 1 else j

      let mutable p = lineStart
      while p < lineStop && src.[p] = ' ' do p <- p + 1
      let indent = p - lineStart
      let nameFrom = p
      while p < lineStop && isName src.[p] do p <- p + 1
      let nameStop = p

      // 名前が空なら足さない。 空行と、字下げだけの行がこれ。
      // XML の `<` だけ / sxml の `(` だけ と揃える
      if nameStop > nameFrom then
        let attrs = ResizeArray<AttrHit>()
        let mutable bodyStart = -1
        let mutable bodyStop = -1
        while p < lineStop do
          if src.[p] = ' ' then p <- p + 1
          elif src.[p] = ':' then
            // 本文。属性より後ろに来る（`Offside.fs` の `pAttrsAndBody`）
            p <- p + 1
            while p < lineStop && src.[p] = ' ' do p <- p + 1
            if p < lineStop && src.[p] = '"' then
              p <- p + 1
              bodyStart <- p
              while p < lineStop && src.[p] <> '"' do p <- p + 1
              bodyStop <- p
              if p < lineStop then p <- p + 1
          elif isName src.[p] then
            let aFrom = p
            while p < lineStop && isName src.[p] do p <- p + 1
            let aStop = p
            while p < lineStop && src.[p] = ' ' do p <- p + 1
            if p < lineStop && src.[p] = '=' then
              p <- p + 1
              while p < lineStop && src.[p] = ' ' do p <- p + 1
              if p < lineStop && src.[p] = '"' then
                p <- p + 1
                let vFrom = p
                let vCol = vFrom - lineStart + 1
                while p < lineStop && src.[p] <> '"' do p <- p + 1
                let vStop = p
                // 中身が空でも 1 文字 ぶんは指せるようにする
                let endCol = max (vCol + 1) (p - lineStart + 1)
                if p < lineStop then p <- p + 1
                attrs.Add
                  { AttrName = src.Substring(aFrom, aStop - aFrom)
                    Value = src.Substring(vFrom, vStop - vFrom)
                    Line = line
                    Column = vCol
                    EndColumn = endCol
                    NameStart = aFrom
                    NameStop = aStop
                    ValueStart = vFrom
                    ValueStop = vStop }
          else p <- p + 1

        // 同じか浅い段を落としてから数える。 残った段の数が深さ
        while indents.Count > 0 && indents.[indents.Count - 1] >= indent do
          indents.RemoveAt(indents.Count - 1)
        let depth = indents.Count
        indents.Add indent
        out.Add
          { Hit =
              { TagName = src.Substring(nameFrom, nameStop - nameFrom)
                Attrs = List.ofSeq attrs
                // 閉じ札も自己閉じも無い。どちらも常に false
                Closing = false
                SelfClosing = false
                // 要素は名前から始まる（XML の `<`、sxml の `(` に当たる字が無い）
                Start = nameFrom
                // 行末。 子は覆わない
                Stop = lineStop
                NameStart = nameFrom
                NameStop = nameStop
                Depth = depth }
            Indent = indent
            LineStart = lineStart
            LineStop = lineStop
            BodyStart = bodyStart
            BodyStop = bodyStop }

      i <- j + 1
      line <- line + 1
    List.ofSeq out

  /// 参照を数える側が読む形。表記を知らない側へ渡すのはこちら。
  let tags (src: string) : TagHit list = rows src |> List.map (fun r -> r.Hit)

  /// カーソルの居る行の頭
  let private lineStartOf (src: string) (cursor: int) =
    let mutable k = cursor
    while k > 0 && src.[k - 1] <> '\n' do k <- k - 1
    k

  /// カーソルの居場所を出す。名前の途中は本文扱い。
  /// 親は、自分より浅い最後の行。
  let contextAt (src: string) (offset: int) : Context =
    let cursor = max 0 (min offset src.Length)
    let all = rows src
    let lineStart = lineStartOf src cursor
    let here = all |> List.tryFind (fun r -> r.LineStart = lineStart)
    match here with
    | Some r when cursor > r.Hit.NameStop ->
      match r.Hit.Attrs |> List.tryFind (fun a -> cursor >= a.ValueStart && cursor <= a.ValueStop) with
      | Some a -> InAttrValue(r.Hit.TagName, a.AttrName)
      | None ->
        // 本文の中は、その要素の #PCDATA
        if r.BodyStart >= 0 && cursor >= r.BodyStart && cursor <= r.BodyStop
        then InContent(Some r.Hit.TagName)
        else InStartTag r.Hit.TagName
    | _ ->
      let indent =
        match here with
        | Some r -> r.Indent
        | None ->
          // 空行の字下げは空白の数。カーソルまでの文字数で数えるな。タブが深くなる。
          let mutable k = lineStart
          while k < cursor && src.[k] = ' ' do k <- k + 1
          k - lineStart
      all
      |> List.filter (fun r -> r.LineStart < lineStart && r.Indent < indent)
      |> List.tryLast
      |> Option.map (fun r -> r.Hit.TagName)
      |> InContent

  /// カーソルの下に在るものを出す。hover が引く。
  /// 指すのはその 1 文字。空白も `=` も引用符も本文も何でもない。
  let tokenAt (src: string) (offset: int) : Token =
    if src.Length = 0 then Nothing
    else
      let p = max 0 (min offset (src.Length - 1))
      match rows src |> List.tryFind (fun r -> p >= r.LineStart && p <= r.LineStop) with
      | None -> Nothing
      | Some r ->
        let h = r.Hit
        if p >= h.NameStart && p < h.NameStop then Element h.TagName
        else
          match h.Attrs |> List.tryFind (fun a -> p >= a.NameStart && p < a.NameStop) with
          | Some a -> Attribute(h.TagName, a.AttrName)
          | None ->
            match h.Attrs |> List.tryFind (fun a -> p >= a.ValueStart && p < a.ValueStop) with
            | Some a -> AttrValue(h.TagName, a.AttrName, a.Value)
            | None -> Nothing

  /// この 1 本 が出す答えを、全部 1 行 の字にする。門のための口。
  /// 字にするところは `Scan.describe`（XML / sxml と共通）
  let describe (src: string) (cursor: int) : string =
    Scan.describe (tags src) (contextAt src cursor) (tokenAt src cursor)
                  (Scan.nameLenBefore src cursor) (Scan.exprLenBefore src cursor)

  /// 式が書ける要素の中身（v4.1）。`:` のあとの引用符。
  /// 属性は `=`、中身は `:`。引用符の中ではどちらも数えるな。
  let texts (src: string) (names: string list) : TextHit list =
    tags src
    |> List.choose (fun t ->
         if not (List.contains t.TagName names) then None
         else
           let stop = min t.Stop (src.Length - 1)
           let mutable i = t.NameStop
           let mutable inQuote = false
           let mutable found = None
           while found.IsNone && i <= stop do
             let c = src.[i]
             if inQuote then
               if c = '"' then inQuote <- false
               i <- i + 1
             elif c = '"' then
               inQuote <- true
               i <- i + 1
             elif c = ':' then
               let mutable k = i + 1
               while k < src.Length && (src.[k] = ' ' || src.[k] = '\t') do k <- k + 1
               if k < src.Length && src.[k] = '"' then
                 let from = k + 1
                 let mutable j = from
                 while j < src.Length && src.[j] <> '"' do j <- j + 1
                 let text = src.Substring(from, j - from)
                 found <- (if text.Trim() = "" then None
                           else Some { TagName = t.TagName; Text = text; Start = from; Stop = j })
                 i <- j + 1
               else i <- i + 1
             else i <- i + 1
           found)