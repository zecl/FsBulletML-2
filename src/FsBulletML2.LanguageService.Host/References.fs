namespace FsBulletML2.LanguageService

open System

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
///
/// **`internal` を外した（v0.8）。** 前は Playground の中に在り、
/// `Parser.Tests` が `Link` で借りて**同じ assembly として** compile して
/// いたので `internal` でも見えていた。器へ移して assembly が分かれた時点で
/// 見えなくなる —— **移して初めて出る形**で、build が赤くなって分かった。
module References =

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

  // --- 定義に無い参照 -------------------------------------------------------

  /// 定義に無い参照を全部。**本文に出てくる順**で返す。
  ///
  /// 同じ名前を 2 回 参照していたら 2 本 引く —— どちらも直す先なので。
  let missing (src: string) : Failure list =
    if pairs.Length = 0 then []
    else
      // **開始札だけ見る。** 欲しいのは「どの名前でどの label を書いたか」で、
      // 木は要らない（`XmlScan` は閉じ札も返すが、それはカーソルの居場所を
      // 出す側が使う）
      let tags = XmlScan.tags src |> List.filter (fun t -> not t.Closing)
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
