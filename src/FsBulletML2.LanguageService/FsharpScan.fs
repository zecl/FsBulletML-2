namespace FsBulletML2.LanguageService

/// F# の CE の字を数える 1 本。ほかの 3 本 とは返すものが違う。
///
/// `XmlScan` / `SxmlScan` / `FsbScan` は「要素・属性・属性値」を返すが、
/// CE にはその形が無い —— 打つのは要素名ではなく DSL の名前で、
/// 属性に当たるものは引数と CustomOperation に散っている。
module FsharpScan =

  let private isIdent (c: char) =
    (c >= 'a' && c <= 'z')
    || (c >= 'A' && c <= 'Z')
    || (c >= '0' && c <= '9')
    || c = '_'
    || c = '\''

  /// 字の位置ごとに「そこが本文か」。文字列とコメントの中は false。
  ///
  /// `wait "aim"` の `aim` は値であって CE の名前ではない ——
  /// 数えないと、弾幕の名前に入った語で hover が浮く。
  ///
  /// 見るのは 4 つ —— `//` 行コメント、`(* *)`（入れ子を数える）、
  /// `"..."`（`\` で逃がす。閉じていなければ行末で切る）、`"""..."""`。
  let private outside (src: string) : bool[] =
    let n = src.Length
    let ok = Array.zeroCreate<bool> n
    let mutable i = 0
    let mutable depth = 0
    while i < n do
      let c = src.[i]
      let c1 = if i + 1 < n then src.[i + 1] else ' '
      if depth > 0 then
        if c = '(' && c1 = '*' then
          depth <- depth + 1
          i <- i + 2
        elif c = '*' && c1 = ')' then
          depth <- depth - 1
          i <- i + 2
        else i <- i + 1
      elif c = '/' && c1 = '/' then
        while i < n && src.[i] <> '\n' do
          i <- i + 1
      elif c = '(' && c1 = '*' then
        depth <- 1
        i <- i + 2
      elif c = '"' then
        // `@"..."` は逃がしを持たない。 手前 の 1 文字 で見分ける
        let verbatim = i > 0 && src.[i - 1] = '@'
        if i + 2 < n && c1 = '"' && src.[i + 2] = '"' then
          i <- i + 3
          while i + 2 < n && not (src.[i] = '"' && src.[i + 1] = '"' && src.[i + 2] = '"') do
            i <- i + 1
          i <- if i + 2 < n then i + 3 else n
        else
          i <- i + 1
          let mutable fin = false
          while not fin && i < n do
            if not verbatim && src.[i] = '\\' then i <- min n (i + 2)
            elif src.[i] = '"' then
              i <- i + 1
              fin <- true
            // 閉じていない行文字列は行末で切る。残り全部 を飲まない ——
            // 飲むと、打っている途中に本文の後ろが丸ごと消える
            elif src.[i] = '\n' then fin <- true
            else i <- i + 1
      else
        ok.[i] <- true
        i <- i + 1
    ok

  /// カーソルの下に在る名前。何の上でもなければ `None`。
  ///
  /// 数から始まる語は名前ではない（`1` や `2u` は識別子にならない）。
  ///
  /// 本文の外は丸めない。 末尾へ寄せると、範囲外の位置でも最後の語が
  /// 返って「そこに在る」ことになる —— そこには字が無い
  let wordAt (src: string) (offset: int) : string option =
    if isNull src || src.Length = 0 then None
    elif offset < 0 || offset >= src.Length then None
    else
      let n = src.Length
      let at = offset
      let ok = outside src
      if not (isIdent src.[at]) || not ok.[at] then None
      else
        let mutable s = at
        while s > 0 && isIdent src.[s - 1] && ok.[s - 1] do
          s <- s - 1
        let mutable e = at
        while e + 1 < n && isIdent src.[e + 1] && ok.[e + 1] do
          e <- e + 1
        if src.[s] >= '0' && src.[s] <= '9' then None
        else Some(src.Substring(s, e - s + 1))

  /// その位置を含む行と桁（1 起点）。`Scan.lineColumn` の 1 本 を引く
  let private lineColumn (src: string) (offset: int) = Scan.lineColumn src offset

  /// 名前を載せている CE の並び。ほかの 3 表記 と同じ `TagHit` を返す。
  ///
  /// v1.6 まで、この表記だけ `Tags` が空だった。「CE には要素名が無いから」
  /// と書いてあったが、無いのは要素名であって名前ではない ——
  /// `defAction "x"` の `x` は `<action label="x">` の `x` そのもので、
  /// 数え方が違うだけだった。
  let tags
    (labels: (string * string * int * string)[])
    (attrName: string)
    (src: string)
    : TagHit list =
    if isNull src || src.Length = 0 then []
    else
      let n = src.Length
      let ok = outside src
      // `outside` は引用符ごと false にする（コメントも同じ側に居る）。
      // だから塊の頭の字で見分けて、両端の引用符を落とす。
      // 返すのは (値, 値の始まり, 値の終わり, 塊の次)
      let readString (from: int) =
        let mutable e = from
        while e < n && not ok.[e] do
          e <- e + 1
        let vs = from + 1
        let ve = max vs (e - 1)
        src.Substring(vs, ve - vs), vs, ve, e
      let hits = ResizeArray<TagHit>()
      let mutable i = 0
      // 入れ子の深さ（v2.4）。`{ }` の積み —— blockAt と同じ数え方で、
      // あちらは名前を積み、こちらは数だけ数える。
      // `}` が多すぎる本文（打っている途中）では 0 で止める
      let mutable depth = 0
      while i < n do
        if ok.[i] && src.[i] = '{' then depth <- depth + 1
        elif ok.[i] && src.[i] = '}' then depth <- max 0 (depth - 1)
        if ok.[i] && isIdent src.[i] && (i = 0 || not (ok.[i - 1] && isIdent src.[i - 1])) then
          let s = i
          let mutable e = i
          while e + 1 < n && ok.[e + 1] && isIdent src.[e + 1] do
            e <- e + 1
          let name = src.Substring(s, e - s + 1)
          match labels |> Array.tryFind (fun (nm, _, _, _) -> nm = name) with
          | Some (_, element, labelArg, fixedName) ->
            let attrs =
              if labelArg < 0 then
                // 引数を取らない（`top`）。位置は名前そのもの ——
                // 名前を書き換える先が無いので、rename はここを指す
                let struct (line, col) = lineColumn src s
                [ { AttrName = attrName
                    Value = fixedName
                    Line = line
                    Column = col
                    EndColumn = col + (e - s + 1)
                    NameStart = s
                    NameStop = e + 1
                    ValueStart = s
                    ValueStop = e + 1 } ]
              else
                // 名前のあとから、`{` か行の終わりまでの文字列を数える
                let mutable j = e + 1
                let mutable seen = 0
                let mutable got = None
                let mutable stop = false
                while not stop && j < n do
                  if ok.[j] && (src.[j] = '{' || src.[j] = '\n') then stop <- true
                  // ok の false は文字列かコメント。頭の字で見分ける
                  elif not ok.[j] && src.[j] = '"' then
                    let (value, vs, ve, next) = readString j
                    if seen = labelArg then
                      got <- Some(value, vs, ve)
                      stop <- true
                    seen <- seen + 1
                    j <- next
                  elif not ok.[j] then
                    // コメント。飛ばす
                    let mutable e2 = j
                    while e2 < n && not ok.[e2] do
                      e2 <- e2 + 1
                    j <- e2
                  else j <- j + 1
                match got with
                | None -> []
                | Some (value, vs, ve) ->
                  let struct (line, col) = lineColumn src vs
                  [ { AttrName = attrName
                      Value = value
                      Line = line
                      Column = col
                      EndColumn = col + (ve - vs)
                      NameStart = s
                      NameStop = e + 1
                      ValueStart = vs
                      ValueStop = ve } ]
            if not attrs.IsEmpty then
              hits.Add
                { TagName = element
                  Attrs = attrs
                  // 閉じ札も自己閉じも無い（fsb と同じ）
                  Closing = false
                  SelfClosing = false
                  Start = s
                  Stop = e + 1
                  NameStart = s
                  NameStop = e + 1
                  // 名前は自分が開く `{` の手前 に在る。 だから
                  // その名前の深さは、いま開いている段そのもの
                  Depth = depth }
          | None -> ()
          i <- e + 1
        else i <- i + 1
      List.ofSeq hits

  /// いちばん外の `{ }` が閉じる位置（0 起点）。閉じていなければ `None`。
  ///
  /// ほかの 3 表記 の「根の閉じ札 / 閉じ括弧」に当たるもの ——
  /// 打っている途中の本文はふつうに閉じていないので、そこで場所を
  /// 決め打つと本文の外に出る。
  let blockEnd (src: string) : int option =
    if isNull src || src.Length = 0 then None
    else
      let ok = outside src
      let mutable depth = 0
      let mutable at = None
      let mutable i = 0
      while at.IsNone && i < src.Length do
        if ok.[i] && src.[i] = '{' then depth <- depth + 1
        elif ok.[i] && src.[i] = '}' then
          depth <- depth - 1
          if depth = 0 then at <- Some i
        i <- i + 1
      at

  /// カーソルがどの `{ }` の中に居るか。返すのはその `{` の手前 の名前。
  ///
  /// まだどこにも入っていなければ `None`（根の builder を打つところ）。
  ///
  /// v1.9 の頭で測った —— 同梱 176 本 を焼いて `{` を 3259 個 数え、
  /// 手前 に名前が無かったものは 0 個。 入れ子はいちばん深いもので 12 段。
  /// FCS は要らない（あちらは host にしか無く、補完はブラウザ側）。
  ///
  /// `}` が多すぎる本文（打っている途中）では、積みが空になったところで
  /// `None` に戻る —— 負に潜らせない。
  let blockAt (src: string) (offset: int) : string option =
    if isNull src || src.Length = 0 then None
    else
      let ok = outside src
      let at = max 0 (min offset src.Length)
      // `{` の手前 の名前を積む
      let stack = ResizeArray<string>()
      let mutable last = ""
      let mutable i = 0
      while i < at do
        if ok.[i] && isIdent src.[i] && (i = 0 || not (ok.[i - 1] && isIdent src.[i - 1])) then
          let s = i
          let mutable e = i
          while e + 1 < src.Length && ok.[e + 1] && isIdent src.[e + 1] do
            e <- e + 1
          // カーソルの下の語は数えない。 打っている途中の名前が
          // 「いま開いている入れ物」になってしまう
          if e + 1 <= at then last <- src.Substring(s, e - s + 1)
          i <- min at (e + 1)
        else
          if ok.[i] && src.[i] = '{' then stack.Add last
          elif ok.[i] && src.[i] = '}' then
            if stack.Count > 0 then stack.RemoveAt(stack.Count - 1)
          i <- i + 1
      if stack.Count = 0 then None else Some stack.[stack.Count - 1]

  /// 根のブロックの直下 に在る行の字下げ。無ければ書き手と同じ 4。
  ///
  /// 本文から測るのはほかの 3 表記 と同じ理由 ——
  /// 書き手が 4 で焼いても、人が 2 で書き直していることは在る。
  let rootChildIndent (src: string) : int =
    if isNull src || src.Length = 0 then 4
    else
      let ok = outside src
      let mutable depth = 0
      let mutable found = None
      let mutable i = 0
      while found.IsNone && i < src.Length do
        if ok.[i] && src.[i] = '{' then depth <- depth + 1
        elif ok.[i] && src.[i] = '}' then depth <- depth - 1
        elif depth = 1 && ok.[i] && isIdent src.[i] && Scan.blankBefore src i then
          found <- Some(Scan.columnOf src i)
        i <- i + 1
      match found with
      | Some n when n > 0 -> n
      | _ -> 4

  /// 2 つ の runtime で同じ答えが返ることを見る口。
  /// 組み立てはここ 1 か所（`Scan.describe` と同じ理由）。
  ///
  /// 本文の何文字 が「文字列・コメントの外」かも出す ——
  /// カーソル 1 点 だけだと、数え方がずれても当たった点でしか出ない
  let describe (src: string) (cursor: int) : string =
    let outsideCount =
      if isNull src || src.Length = 0 then 0
      else outside src |> Array.filter id |> Array.length
    let word =
      match wordAt src cursor with
      | Some w -> "word(" + w + ")"
      | None -> "nothing"
    word + " outside=" + string outsideCount + " len=" + string (if isNull src then 0 else src.Length)

  /// 名前の数え方を突き合わせる口。表は引数で来る ——
  /// 器に要素名を書けないので、ここで表を作ることはできない（門が見ている）。
  ///
  /// 配列で受ける。 F# の list は焼くと連結リストになり、
  /// 表から素の配列を渡す道が無くなる
  let describeTags
    (labels: (string * string * int * string)[])
    (attrName: string)
    (src: string)
    : string =
    let sb = System.Text.StringBuilder()
    let add (s: string) = sb.Append s |> ignore
    for t in tags labels attrName src do
      add t.TagName
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
        add "/"
        add (string a.ValueStart)
        add "-"
        add (string a.ValueStop)
        add "]"
      add ";"
    add " end="
    add (match blockEnd src with Some at -> string at | None -> "none")
    add " indent="
    add (string (rootChildIndent src))
    sb.ToString()

  /// 「いまどの `{ }` の中に居るか」を突き合わせる口。
  /// 本文の全部 の位置で数える —— カーソル 1 点 だけだと、
  /// 数え方がずれても当たった点でしか出ない
  let describeBlocks (src: string) : string =
    if isNull src then "len=0"
    else
      let sb = System.Text.StringBuilder()
      let mutable last = ""
      for i in 0 .. src.Length do
        let now = match blockAt src i with Some n -> n | None -> "-"
        if now <> last then
          sb.Append(string i).Append(':').Append(now).Append(' ') |> ignore
          last <- now
      sb.Append("len=").Append(string src.Length).ToString()
