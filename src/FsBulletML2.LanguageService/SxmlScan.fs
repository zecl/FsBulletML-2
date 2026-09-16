namespace FsBulletML2.LanguageService

open System
// `Attribute` が `System.Attribute` に負ける。理由は `XmlScan.fs` の同じ行
open FsBulletML2.LanguageService

/// S 式の字を数える 1 本。 XML 側と同じ型を返す。
///
/// 数える形はこれだけ ——
///
///     (name (@ (attr "値") ...) "本文" 子...)
module SxmlScan =

  /// 括弧 1 組。`TagHit` に、括弧の中の区切りを足したもの。
  ///
  /// 参照を数える側は `TagHit` だけで足りる（表記を知らない）。
  /// カーソルの居場所を出すには「属性ブロックの中に居るか」が要るので、
  /// そこだけこちらが持つ。
  type Form =
    { Hit: TagHit
      /// `(@` の `(` の位置。属性ブロックが無ければ -1
      AttrsStart: int
      /// 属性ブロックを閉じる `)` の位置。閉じていなければ本文の末尾。無ければ -1
      AttrsStop: int }

  /// 括弧を頭から全部 拾う。木は組まないが、`Start` / `Stop` が
  /// 子を丸ごと覆うので、入れ子は位置から読める（いちばん内側 = 前順で最後）。
  ///
  /// 閉じ引用符が無い属性も捨てない。 XML 側は捨てているが、あちらは
  /// カーソルの居場所を別の走査（`inTag`）で出しているので困らない。
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

    /// 括弧 1 組。`i` は `(` の上。読み終えると `)` の次（無ければ末尾）。
    ///
    /// 入れ子の深さは再帰の段そのもの（v2.4）—— sxml は包含で
    /// 木になる（測った。176 / 176 本）ので、ここで数えれば足りる
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
      // 名前が空なら足さない。 `(` を打った直後がこれ。
      // XML 側も `<` だけの札を足していない —— そこで揃う。
      //
      // 効くのは参照を数える側（`tags`）。 カーソルの居場所のほうは
      // 足しても変わらない（名前が空なら `cursor <= NameStop` の枝を通って
      // 外側を返す）—— 足さない理由を「候補が消えるから」と書いていたが、
      // 変異を当てたら緑のままだった。 当たるのは `tags` の並びのほう
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
    // 前順に並べ直す。 上は子を読み終えてから自分を足すので後順になる
    // —— 「いちばん内側 = 最後」で引く側が、そのままだと逆を引く
    out |> Seq.sortBy (fun f -> f.Hit.Start) |> List.ofSeq

  /// 参照を数える側が読む形。表記を知らない側へ渡すのはこちら。
  let tags (src: string) : TagHit list = forms src |> List.map (fun f -> f.Hit)

  /// カーソルを覆う括弧のうち、いちばん内側。前順なので最後
  let private innermost (all: Form list) (pick: TagHit -> bool) =
    all |> List.filter (fun f -> pick f.Hit) |> List.tryLast

  /// カーソルの居場所を出す。
  ///
  /// 名前を打っている途中は本文扱い（`(act|`）。XML 側の `<act|` と同じ ——
  /// そこで出したいのは要素の候補で、`(` の直後もそれに含まれる。
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
  ///
  /// 指すのはカーソルの位置に在る 1 文字。だから
  ///
  ///   `(` の上 / `)` の上 / 引用符そのものの上   何でもない
  ///   要素名の最初の字と最後の字               その要素
  ///   括弧の中の空白 / 本文の字                何でもない
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
  ///
  ///     (wait "30-$rank*8")
  ///     (direction (@ (type "absolute")) "180+$rand*30")
  ///
  /// 括弧の深さを数える。 属性は `(@ (type "absolute"))` の中に在るので、
  /// 深さ 0 の引用符だけを拾えば、属性値と取り違えない
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