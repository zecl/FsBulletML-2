namespace FsBulletML2.LanguageService

open System

/// 参照の欠けを、本文の字から全部 数える（Core は最初の 1 件 で止まり、
/// しかも位置を持たない）。
///
/// ここは Core より広い（到達を見ないので、誰からも参照されていない枝の
/// 壊れた参照も挙げる）—— だから単独では使わない。
///
/// 二重化 なのでコーパス で突き合わせている（`Parser.Tests/ReferenceScan.fs`）
module References =

  // --- 走る先は語彙から引く -------------------------------------------------

  /// 参照する要素 -> 参照される要素と、label の属性名。表を持たない。
  ///
  /// 規則は `Refs.pairs`（器の側）に 1 本 —— 2 か所 に書くと
  /// 「波線は出るのに rename は当たらない」が作れて、
  /// どちらも単独では正しく見える。
  ///
  /// 空なら呼ぶ側が何も挙げない（`Parser.Tests` が 0 件 を赤にする）
  let pairs: (string * string * string)[] =
    Vocabulary.elements
    |> Array.toList
    |> List.map (fun e -> e.Name, (e.Attrs |> Array.toList |> List.map (fun a -> a.Name)))
    |> Refs.pairs
    |> List.toArray

  // --- 定義に無い参照 -------------------------------------------------------

  /// 定義に無い参照を全部。本文に出てくる順で返す。
  ///
  /// 字を数えるところは受け取る（v0.9）—— 表記ごとの 1 本 は
  /// `ISourceReader.Tags` に束ねてある。
  ///
  /// 数えるところは `Refs.missing`（器の側）に 1 本。
  /// ここが持つのは人へ見せる文面だけ。
  let missing (scan: string -> TagHit list) (src: string) : Failure list =
    if pairs.Length = 0 then []
    else
      Refs.missing (List.ofArray pairs) (scan src)
      |> List.map (fun m ->
           { Line = m.Hit.Line
             Column = m.Hit.Column
             EndColumn = m.Hit.EndColumn
             Message = sprintf "%s が指す %s が無い: %s" m.RefName m.DefName m.Hit.Value })

  /// Apply の答えを、波線にできる形へ。Core の判定が真。
  ///
  /// 位置が在る層（XML の構文）は本文が読めていないので、そのまま 1 本。
  /// 位置が無い層のときだけ本文の字から数え直して位置を足す。
  /// 0 件 なら理由をそのまま位置なしで出す —— 参照以外の層（式、輪、
  /// `top` が無い）はここでは何も足せない。
  ///
  /// `Main.fs` と `Parser.Tests` が同じこれを通る。本番と別の組み合わせを
  /// 試験の側で組まない —— 組むとそちらだけが緑になる。
  let explain (scan: string -> TagHit list) (failure: Failure option) (src: string) : Failure list =
    match failure with
    | None -> []
    | Some f when f.Line > 0 -> [ f ]
    | Some f ->
      match missing scan src with
      | [] -> [ f ]
      | ms -> ms
