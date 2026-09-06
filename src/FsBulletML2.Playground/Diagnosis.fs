namespace FsBulletML2.Playground

open System.Xml
open FsBulletML2

/// 読めなかった理由。
///
/// **`Line = 0` は「位置が無い」の印。** 波線を引くかどうかがこれで決まる。
/// 1 以上 なら 1 起点 の行と桁で、そこに印を付けてよい。
///
/// **`EndColumn = 0` は「終わりが分からない」の印。** 引く側が行末まで伸ばす。
/// 終わりが分かるのは、本文の字から数えた位置（参照の欠けなど）だけ ——
/// `XmlException` は「そこから先が読めない」しか言わない。
type Failure =
  { Line: int
    Column: int
    EndColumn: int
    Message: string }

/// Apply が落ちたときに、**どこまで分かるか**を分ける。
///
/// 実際に流して数えた 4 層。
///
///     XML の構文        XmlException                  行・桁 あり
///     BulletML でない   tryReadXmlString が None      位置なし
///     木は組めない      BulletmlDTDViolationException 位置なし（label 名は在る）
///     式                XPathException                位置なし
///
/// **位置が在るのは 1 層 目 だけ。** 3 層 目 は label 名を持っているので
/// 本文を探せば当てられるが、**同じ label が 2 つ 在るときに嘘の場所を指す。**
/// 外れた波線は「そこが悪い」と言い切るので、推定では引かない。
///
/// `輪` と `同じ label が 2 つ` と `top が無い` は、ここを素通りする ——
/// 落ちるのは走行のほう。**Apply では何も出ないのが正しい。**
///
/// このファイルが Playground の下に在るのに Core と Parser しか使わないのは、
/// `Parser.Tests` が `Link` で借りて .NET で走らせるため
/// （`ApplySource` は Blazor WASM のアプリの中に在って、試験から触れない）。
module Diagnosis =

  /// 位置なしの理由
  let plain message =
    { Line = 0
      Column = 0
      EndColumn = 0
      Message = message }

  /// 例外を層に分ける。**位置が在るのは `XmlException` だけ。**
  ///
  /// 空文字を読ませると `XmlException` は `行 0 桁 0` を返す。
  /// **0 のまま渡すと Monaco の範囲が壊れる**ので 1 に丸める ——
  /// 位置なし（`Line = 0`）と混ざらないように、ここで分けきる。
  let ofException (ex: exn) : Failure =
    match ex with
    | :? XmlException as x ->
      { Line = max 1 x.LineNumber
        Column = max 1 x.LinePosition
        // 「そこから先が読めない」しか言わないので、終わりは分からない
        EndColumn = 0
        Message = x.Message }
    | _ -> plain ex.Message

  /// 読んで、載せる。**載せるところは呼ぶ側が渡す** ——
  /// host は `Playfield` を作り、試験は `Runner.load` だけを通す。
  /// どちらも同じ分け方を通るので、**試験で当てた形が本番の形になる。**
  ///
  /// 返りが `None` なら成功。
  let apply (build: Bulletml -> unit) (xml: string) : Failure option =
    try
      match tryReadXmlString xml with
      | None -> Some(plain "BulletML として読めなかった")
      | Some bulletml ->
        build bulletml
        None
    with ex -> Some(ofException ex)
