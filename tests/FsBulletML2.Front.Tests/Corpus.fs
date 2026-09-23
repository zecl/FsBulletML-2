namespace FsBulletML2.Front.Tests

open System.IO

/// 弾幕のコーパス。Core.Tests の同名は internal なので、こちらは写し。
/// あちらを参照すると、公開だけで書けているかの検査が消える。
module internal CorpusData =

  let samplesDir =
    Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "samples"))

  /// ビルドが吐いたコピーを外す。入れないと分母がビルド状態に依存する
  let isBuildOutput (p: string) =
    let s = p.Replace('\\', '/')
    [ "/bin/"; "/obj/"; "/Library/"; "/Temp/" ] |> List.exists s.Contains

  /// 中身が同じものを 1 本に潰して返す（相対パスの辞書順で最初のものを代表にする）
  let uniqueSamples () =
    if not (Directory.Exists samplesDir) then []
    else
      Directory.EnumerateFiles(samplesDir, "*.xml", SearchOption.AllDirectories)
      |> Seq.filter (isBuildOutput >> not)
      |> Seq.map (fun p -> p, File.ReadAllText p)
      |> Seq.groupBy snd
      |> Seq.map (fun (_, g) -> g |> Seq.map fst |> Seq.sort |> Seq.head)
      |> Seq.sort
      |> List.ofSeq
