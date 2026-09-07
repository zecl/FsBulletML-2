namespace FsBulletML2.LanguageService

/// F# の CE の字を数える 1 本。**ほかの 3 本 とは返すものが違う。**
///
/// `XmlScan` / `SxmlScan` / `FsbScan` は「要素・属性・属性値」を返すが、
/// **CE にはその形が無い** —— 打つのは要素名ではなく DSL の名前で、
/// 属性に当たるものは引数と CustomOperation に散っている。
///
/// だからここが返すのは**名前 1 つ だけ**。それが BulletML の何を作るかは
/// 語彙の側（`Vocab.Ce`。正本は host の `Spec.ce`）が持つ。
///
/// ## `Tags` を持たない
///
/// 参照を数える側（`Refs.missing`）が探すのは「要素名 + label 属性」で、
/// CE はそこが DSL の名前（`defAction "x"` / `actionRef "x" []`）。
/// **別の語彙なので当てない** —— rename も Quick Fix もこの表記では空のまま。
///
/// ## 名前の字を ASCII で書く
///
/// F# の識別子は Unicode を取れるが、CE の名前は全部 ASCII。
/// `Char.IsLetterOrDigit` に任せると **2 つ の runtime で境界が動きうる**
/// （本文には日本語が入る）ので、当てる字をここに書く。
/// 答えが一致することは `guard-fable-parity.ps1` が走行で見ている。
module FsharpScan =

  let private isIdent (c: char) =
    (c >= 'a' && c <= 'z')
    || (c >= 'A' && c <= 'Z')
    || (c >= '0' && c <= '9')
    || c = '_'
    || c = '\''

  /// 字の位置ごとに「そこが本文か」。**文字列とコメントの中は false。**
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
        // **`@"..."` は逃がしを持たない。** 手前 の 1 文字 で見分ける
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
            // 閉じていない行文字列は行末で切る。**残り全部 を飲まない** ——
            // 飲むと、打っている途中に本文の後ろが丸ごと消える
            elif src.[i] = '\n' then fin <- true
            else i <- i + 1
      else
        ok.[i] <- true
        i <- i + 1
    ok

  /// カーソルの下に在る名前。**何の上でもなければ `None`。**
  ///
  /// 数から始まる語は名前ではない（`1` や `2u` は識別子にならない）。
  ///
  /// **本文の外は丸めない。** 末尾へ寄せると、範囲外の位置でも最後の語が
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

  /// 2 つ の runtime で同じ答えが返ることを見る口。
  /// **組み立てはここ 1 か所**（`Scan.describe` と同じ理由）。
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
