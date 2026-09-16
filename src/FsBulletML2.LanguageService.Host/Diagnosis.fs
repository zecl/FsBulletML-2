namespace FsBulletML2.LanguageService

open System
open System.Xml
open FsBulletML2

/// 読めなかった理由。
///
/// `Line = 0` は「位置が無い」の印。 波線を引くかどうかがこれで決まる。
/// 1 以上 なら 1 起点 の行と桁で、そこに印を付けてよい。
///
/// `EndColumn = 0` は「終わりが分からない」の印。 引く側が行末まで伸ばす。
/// 終わりが分かるのは、本文の字から数えた位置（参照の欠けなど）だけ ——
/// `XmlException` は「そこから先が読めない」しか言わない。
type Failure =
  { Line: int
    Column: int
    EndColumn: int
    Message: string }

/// Apply が落ちたときに、どこまで分かるかを分ける。
///
///     構文              XmlException / FParsec        行・桁 あり
///     BulletML でない   tryRead… が None              位置なし
///     木は組めない      BulletmlDTDViolationException 位置なし
///     式                BulletmlDTDViolationException 位置なし
///
/// 位置が在るのは 1 層 目 だけ。 3 層 目 は label 名を本文から探せるが、
/// 同じ label が 2 つ 在るときに嘘の場所を指すので、推定では引かない。
///
/// `輪` と `同じ label が 2 つ` と `top が無い` は、ここを素通りする ——
/// 落ちるのは走行のほう。Apply では何も出ないのが正しい。
module Diagnosis =

  /// 位置なしの理由
  let plain message =
    { Line = 0
      Column = 0
      EndColumn = 0
      Message = message }

  /// 例外を層に分ける。位置が在るのは `XmlException` だけ。
  ///
  /// 空文字を読ませると `XmlException` は `行 0 桁 0` を返す。
  /// 0 のまま渡すと Monaco の範囲が壊れるので 1 に丸める ——
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

  /// 読んで、載せる。載せるところは呼ぶ側が渡す ——
  /// host は `Playfield` を作り、試験は `Runner.load` だけを通す。
  /// どちらも同じ分け方を通るので、試験で当てた形が本番の形になる。
  ///
  /// 返りが `None` なら成功。
  ///
  /// XML の 1 本。 sxml は `applySxml`。束ねているのは `SourceReader`
  let apply (build: Bulletml -> unit) (xml: string) : Failure option =
    try
      match tryReadXmlString xml with
      | None -> Some(plain "BulletML として読めなかった")
      | Some bulletml ->
        build bulletml
        None
    with ex -> Some(ofException ex)

  /// FParsec の文面は 5 行 ある —— `Error in Ln: ...`、本文の写し、
  /// キャレットの絵、`Note:`、`Expecting:`。帯にも波線にも絵は要らないので、
  /// 何を待っていたかを言う行だけ足す。
  ///
  /// 飾り。 位置が本体で、この行が取れなくても波線は同じところに出る ——
  /// FParsec が文面を変えたら静かに落ちるだけ
  let private expectation (message: string) =
    message.Split('\n')
    |> Array.map (fun l -> l.Trim())
    |> Array.tryFind (fun l -> l.StartsWith("Expecting:", StringComparison.Ordinal))

  /// F# の CE を読んで、載せる。parse だけ（型検査しない）。
  ///
  /// 層の分かれ方は XML / sxml と同じ ——
  ///
  ///     F# の構文        FCS の診断              行・桁 あり
  ///     CE として読めない  知らない名前・知らない形  行・桁 あり
  ///     木は組めない      Core の例外              位置なし
  ///
  /// 2 層 目 にも位置が在るのが、ほかの表記と違うところ。
  /// 歩いている最中に「どこで詰まったか」が分かるため。
  let applyFsharp (build: Bulletml -> unit) (source: string) : Failure option =
    try
      match FsharpCe.read source with
      // 位置の無い理由（`0` は `FsharpCe` 側の印）
      | Error (0, _, message) -> Some(plain message)
      | Error (line, column, message) ->
        Some
          { Line = max 1 line
            Column = max 1 column
            // 「そこで詰まった」しか言わないので、終わりは分からない
            EndColumn = 0
            Message = message }
      | Ok bulletml ->
        build bulletml
        None
    with ex -> Some(ofException ex)

  /// sxml を読んで、載せる。`tryReadSxmlString` を使わない。
  ///
  /// あちらは `Failure (_,_,_) -> None` で、FParsec が持っている位置を捨てている。
  /// 口が在ることと、その口が要るものを返すことは別。
  let applySxml (build: Bulletml -> unit) (sxml: string) : Failure option =
    try
      match Sxml.parse sxml with
      | FParsec.CharParsers.Failure (message, error, _) ->
        let where = "S 式として読めなかった"
        Some
          { // 空文字を読ませても 1 起点 で返ってくるが、丸めておく ——
            // 0 のまま渡すと Monaco の範囲が壊れる
            Line = max 1 (int error.Position.Line)
            Column = max 1 (int error.Position.Column)
            // 「そこから先が読めない」しか言わないので、終わりは分からない
            EndColumn = 0
            Message =
              match expectation message with
              | Some e -> where + "。" + e
              | None -> where }
      | FParsec.CharParsers.Success (node, _, _) ->
        match BulletmlRead.tryBulletmlFromXmlNode node with
        | None -> Some(plain "BulletML として読めなかった")
        | Some bulletml ->
          build bulletml
          None
    with ex -> Some(ofException ex)

  /// インデント記法（fsb）を読んで、載せる。`tryReadFsbString` を使わない。
  ///
  /// あちらも `Failure (_,_,_) -> None` で位置を捨てている（sxml と同じ形）。
  let applyFsb (build: Bulletml -> unit) (fsb: string) : Failure option =
    try
      match Offside.parse fsb with
      | FParsec.CharParsers.Failure (message, error, _) ->
        let where = "インデント記法として読めなかった"
        Some
          { // 空文字を読ませても 1 起点 で返ってくるが、丸めておく
            Line = max 1 (int error.Position.Line)
            Column = max 1 (int error.Position.Column)
            // 「そこから先が読めない」しか言わないので、終わりは分からない
            EndColumn = 0
            Message =
              match expectation message with
              | Some e -> where + "。" + e
              | None -> where }
      | FParsec.CharParsers.Success (node, _, _) ->
        match BulletmlRead.tryBulletmlFromXmlNode node with
        | None -> Some(plain "BulletML として読めなかった")
        | Some bulletml ->
          build bulletml
          None
    with ex -> Some(ofException ex)
