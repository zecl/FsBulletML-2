namespace FsBulletML2.LanguageService

open FsBulletML2

/// 表記ごとの書く口。`ISourceReader` の対。
/// 歩きは `BulletmlWriter.writeTo`。受け口はパーサと同じファイル。
/// 書けないものは `Error`。黙って落とすと、読み直した値が変わる。
type ISourceWriter =
  abstract Kind: SourceKind
  /// 弾幕をその表記の字にする。書けなければ理由
  abstract Write: bulletml: Bulletml -> Result<string, string>

module SourceWriter =

  let private ofParts kind write =
    { new ISourceWriter with
        member _.Kind = kind
        member _.Write bulletml = write bulletml }

  /// 定数を畳まない側を使う。畳むと `8` が `8.0000000000` になる。
  /// カタログを焼く側は畳むまま。ここへ寄せるな。
  let xml = ofParts SourceKind.Xml (fun b -> Result.Ok(BulletmlWriter.toIndentedXml 4 b))
  let sxml = ofParts SourceKind.Sxml Sxml.write
  let fsb = ofParts SourceKind.Fsb Offside.write
  let fsharp = ofParts SourceKind.FSharpDsl FsharpCe.write

  /// 並びは `SourceKind.all` と同じ順。読む口と揃えておく
  let all: ISourceWriter list = [ xml; sxml; fsb; fsharp ]

  let tryFind (kind: SourceKind) = all |> List.tryFind (fun w -> w.Kind = kind)
