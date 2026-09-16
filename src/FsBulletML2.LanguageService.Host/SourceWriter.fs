namespace FsBulletML2.LanguageService

open FsBulletML2

/// 表記ごとの「書く口」。`ISourceReader` の対。
///
/// 木を歩くのは `Core` の `BulletmlWriter.writeTo` 1 本 で、違うのは受け口だけ
/// —— それはその文法を読むパーサと同じファイルに置いてある
/// （離すと、文法を直したときに書く側が置いていかれる）。
///
/// 書けないことが在るので返りは `Result`（fsb は本文に空白を入れられない、
/// sxml は属性値に通せない字が在る、CE は `description` を書く口 が無い）——
/// 黙って落とさない。 落とすと、読み直したときに値が変わる。
type ISourceWriter =
  abstract Kind: SourceKind
  /// 弾幕をその表記の字にする。書けなければ理由
  abstract Write: bulletml: Bulletml -> Result<string, string>

module SourceWriter =

  let private ofParts kind write =
    { new ISourceWriter with
        member _.Kind = kind
        member _.Write bulletml = write bulletml }

  /// 定数を畳まない側を使う（畳む側 は `8` が `8.0000000000` になり、
  /// 表記を行き来する用途では人が書いた字が化ける）。
  ///
  /// 同梱カタログを焼くところ（`SelectPattern`）は畳む側のままにしてある
  let xml = ofParts SourceKind.Xml (fun b -> Result.Ok(BulletmlWriter.toIndentedXml 4 b))
  let sxml = ofParts SourceKind.Sxml Sxml.write
  let fsb = ofParts SourceKind.Fsb Offside.write
  let fsharp = ofParts SourceKind.FSharpDsl FsharpCe.write

  /// 並びは `SourceKind.all` と同じ順。読む口と揃えておく
  let all: ISourceWriter list = [ xml; sxml; fsb; fsharp ]

  let tryFind (kind: SourceKind) = all |> List.tryFind (fun w -> w.Kind = kind)
