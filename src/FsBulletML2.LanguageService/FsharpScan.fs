namespace FsBulletML2.LanguageService

/// F# の CE の字を数える 1 本。ほかの 3 本 とは返すものが違う。
/// 打つのは要素名ではなく DSL の名前。
module FsharpScan =

  let private isIdent (c: char) =
    (c >= 'a' && c <= 'z')
    || (c >= 'A' && c <= 'Z')
    || (c >= '0' && c <= '9')
    || c = '_'
    || c = '\''

  /// 字の位置ごとに「そこが本文か」。文字列とコメントの中は false。
  /// 文字列の中を名前に数えるな。弾幕の名前で hover が浮く。
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
            // 閉じていない行文字列は行末で切る。残り全部 を飲むな。
            elif src.[i] = '\n' then fin <- true
            else i <- i + 1
      else
        ok.[i] <- true
        i <- i + 1
    ok

  /// カーソルの下に在る名前。何の上でもなければ `None`。
  /// 範囲外を末尾へ寄せるな。字が無い場所で最後の語が返る。
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

  /// 名前を載せている CE の並び。ほかの 3 表記 と同じ `TagHit`。
  /// 空にするな。無いのは要素名であって名前ではない。
  let tags
    (labels: (string * string * int * string)[])
    (attrName: string)
    (src: string)
    : TagHit list =
    if isNull src || src.Length = 0 then []
    else
      let n = src.Length
      let ok = outside src
      // `outside` は引用符ごと false。頭の字で見分けて、両端の引用符を落とす。
      let readString (from: int) =
        let mutable e = from
        while e < n && not ok.[e] do
          e <- e + 1
        let vs = from + 1
        let ve = max vs (e - 1)
        src.Substring(vs, ve - vs), vs, ve, e
      let hits = ResizeArray<TagHit>()
      let mutable i = 0
      // 深さは `{ }` の数。`}` が多すぎても 0 で止める。負にしない。
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
                // 引数を取らない名前。位置は名前そのもの。書き換え先が無い。
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
                  // 名前は自分が開く `{` の手前。深さはいま開いている段。
                  Depth = depth }
          | None -> ()
          i <- e + 1
        else i <- i + 1
      List.ofSeq hits

  /// いちばん外の `{ }` が閉じる位置（0 起点）。閉じていなければ `None`。
  /// 閉じていない本文に場所を決め打つと、本文の外に出る。
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
  /// どこにも入っていなければ `None`。`}` が多すぎても負にしない。
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
          // カーソルの下の語は数えない。打っている途中が入れ物になる。
          if e + 1 <= at then last <- src.Substring(s, e - s + 1)
          i <- min at (e + 1)
        else
          if ok.[i] && src.[i] = '{' then stack.Add last
          elif ok.[i] && src.[i] = '}' then
            if stack.Count > 0 then stack.RemoveAt(stack.Count - 1)
          i <- i + 1
      if stack.Count = 0 then None else Some stack.[stack.Count - 1]

  /// 根のブロックの直下 の字下げ。無ければ 4。
  /// 本文から測る。書き手の 4 を人が 2 にしていることは在る。
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

  /// 2 runtime の突き合わせ口。組み立てはここ 1 か所。
  /// 文字列の外の文字数も出す。1 点 だけだとずれが隠れる。
  let describe (src: string) (cursor: int) : string =
    let outsideCount =
      if isNull src || src.Length = 0 then 0
      else outside src |> Array.filter id |> Array.length
    let word =
      match wordAt src cursor with
      | Some w -> "word(" + w + ")"
      | None -> "nothing"
    word + " outside=" + string outsideCount + " len=" + string (if isNull src then 0 else src.Length)

  /// 名前の数え方を突き合わせる口。表は引数。ここで表を作るな。
  /// 配列で受ける。list は焼くと連結リストになり、素の配列を渡せなくなる。
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

  /// どの `{ }` の中かを突き合わせる口。
  /// 全部 の位置で数える。1 点 だけだとずれが隠れる。
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
