namespace FsBulletML2.Playground

open System

/// 開始タグの中の属性 1 つ。位置は値の中身（引用符の内側）で、1 起点。
type internal AttrHit =
  { AttrName: string
    Value: string
    Line: int
    Column: int
    EndColumn: int }

/// 開始タグ 1 つ。`<foo/>` も `<foo>` も同じに見る（閉じ札は要らない）。
type internal TagHit =
  { TagName: string
    Attrs: AttrHit list }

/// **参照の欠けを、本文の字から全部 数える。**
///
/// Core は最初の 1 件 で `raise` して止まり、しかも位置を持たない
/// （`Bulletml` の DU に行番号が無い）。だから「無い label」を全部 波線に
/// するには、本文の側から数えるしかない。
///
/// **ここは Core より広い。** Core は `top` から到達した要素しか展開しないので、
/// 誰からも参照されていない枝の中の壊れた参照は素通りする（コーパスの
/// `readTest.xml` が実際にそう）。ここは到達を見ないので、そういう参照も挙げる。
/// **だから単独では使わない** —— `explain` が Core の判定を真として使う。
///
/// 判定を作り直している以上は二重化なので、コーパスで突き合わせる
/// （`Parser.Tests/ReferenceScan.fs`）——
///
///   - **本番と同じ道**（`explain`）で、Apply が通る弾幕には 1 本 も引かないこと
///   - 参照を 1 つ 壊して Core が落ちたとき、同じ名前をここも挙げること
module internal References =

  // --- 走る先は語彙から引く -------------------------------------------------

  /// 参照する要素 -> 参照される要素と、label の属性名。**表を持たない** ——
  /// 語彙の中で名前が `Ref` で終わり、`Ref` を落とした名前も語彙に在り、
  /// 両方 が同じ属性を持つものだけを対にする。
  ///
  /// **空なら呼ぶ側が何も挙げない**（reflection が効いていない印。
  /// `Parser.Tests` が 0 件 を赤にする）
  let pairs: (string * string * string)[] =
    let byName =
      Vocabulary.elements
      |> Array.map (fun e -> e.Name, (e.Attrs |> Array.map (fun a -> a.Name)))
      |> dict
    Vocabulary.elements
    |> Array.choose (fun e ->
        if not (e.Name.EndsWith("Ref", StringComparison.Ordinal)) then None
        else
          let def = e.Name.Substring(0, e.Name.Length - 3)
          match byName.TryGetValue def with
          | false, _ -> None
          | true, defAttrs ->
            e.Attrs
            |> Array.tryPick (fun a -> if Array.contains a.Name defAttrs then Some(e.Name, def, a.Name) else None))

  // --- 開始タグを数える -----------------------------------------------------

  let private isNameChar (c: char) =
    Char.IsLetterOrDigit c || c = '_' || c = '-' || c = '.' || c = ':'

  /// 開始タグを頭から全部 拾う。
  ///
  /// **閉じ札も入れ子も見ない。** 欲しいのは「どの名前でどの label を書いたか」
  /// だけなので、木を組む必要が無い。組まないぶん、壊れた本文でも走れる。
  ///
  /// 飛ばすもの: コメント / 宣言 / CDATA / 閉じ札。
  /// **引用符の中は名前も `>` も数えない** —— 属性値に `>` を書ける。
  let internal tags (src: string) : TagHit list =
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
      elif i + 2 <= src.Length && (src.[i + 1] = '!' || src.[i + 1] = '?' || src.[i + 1] = '/') then skipUntil ">"
      else
        advance () // '<'
        let nameFrom = i
        while i < src.Length && isNameChar src.[i] do advance ()
        let name = src.Substring(nameFrom, i - nameFrom)
        let attrs = ResizeArray<AttrHit>()
        let mutable closed = false
        while not closed && i < src.Length do
          if src.[i] = '>' then
            advance ()
            closed <- true
          elif Char.IsWhiteSpace src.[i] || src.[i] = '/' then advance ()
          elif isNameChar src.[i] then
            let aFrom = i
            while i < src.Length && isNameChar src.[i] do advance ()
            let aName = src.Substring(aFrom, i - aFrom)
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
                // 閉じ引用符が無いまま終わったら、そのタグは読めていない。
                // **拾った属性ごと捨てる** —— 位置が本文とずれる
                if i >= src.Length then
                  closed <- true
                else
                  attrs.Add
                    { AttrName = aName
                      Value = src.Substring(vFrom, i - vFrom)
                      Line = vLine
                      Column = vCol
                      // 中身が空でも 1 文字 ぶんは指せるようにする
                      EndColumn = max (vCol + 1) (col i) }
                  advance () // 閉じ引用符
          else advance ()
        if name <> "" then hits.Add { TagName = name; Attrs = List.ofSeq attrs }
    List.ofSeq hits

  // --- 定義に無い参照 -------------------------------------------------------

  /// 定義に無い参照を全部。**本文に出てくる順**で返す。
  ///
  /// 同じ名前を 2 回 参照していたら 2 本 引く —— どちらも直す先なので。
  let missing (src: string) : Failure list =
    if pairs.Length = 0 then []
    else
      let tags = tags src
      let value (t: TagHit) (attr: string) =
        t.Attrs |> List.tryFind (fun a -> a.AttrName = attr)
      [ for (refName, defName, attr) in pairs do
          let defined =
            tags
            |> List.choose (fun t -> if t.TagName = defName then value t attr else None)
            |> List.map (fun a -> a.Value)
            |> Set.ofList
          for t in tags do
            if t.TagName = refName then
              match value t attr with
              | Some a when not (defined.Contains a.Value) ->
                yield
                  { Line = a.Line
                    Column = a.Column
                    EndColumn = a.EndColumn
                    Message = sprintf "%s が指す %s が無い: %s" refName defName a.Value }
              | _ -> () ]
      |> List.sortBy (fun f -> f.Line, f.Column)

  /// Apply の答えを、波線にできる形へ。**Core の判定が真。**
  ///
  /// 位置が在る層（XML の構文）は本文が読めていないので、そのまま 1 本。
  /// 位置が無い層のときだけ本文の字から数え直して位置を足す。
  /// **0 件 なら理由をそのまま位置なしで出す** —— 参照以外の層（式、輪、
  /// `top` が無い）はここでは何も足せない。
  ///
  /// `Main.fs` と `Parser.Tests` が同じこれを通る。**本番と別の組み合わせを
  /// 試験の側で組まない** —— 組むとそちらだけが緑になる。
  let explain (failure: Failure option) (src: string) : Failure list =
    match failure with
    | None -> []
    | Some f when f.Line > 0 -> [ f ]
    | Some f ->
      match missing src with
      | [] -> [ f ]
      | ms -> ms
