namespace FsBulletML2.Core.Tests

open System.IO
open System.Text.RegularExpressions
open NUnit.Framework

/// 1 コマ進める呼び出しが返すものを、呼ぶ側がどう使っているかを固める。
[<TestFixture>]
type CallingConvention() =

  let repoRoot =
    Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, "..", ".."))

  let isBuildOutput (p: string) =
    let s = p.Replace('\\', '/')
    [ "/bin/"; "/obj/"; "/Library/"; "/Temp/"; "/.git/" ] |> List.exists s.Contains

  let sources () =
    [ "src"; "samples" ]
    |> List.collect (fun d ->
        let dir = Path.Combine(repoRoot, d)
        if not (Directory.Exists dir) then []
        else
          Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
          |> Seq.filter (fun p -> p.EndsWith ".fs" || p.EndsWith ".cs")
          |> Seq.filter (isBuildOutput >> not)
          |> List.ofSeq)
    |> List.sort

  let relative (p: string) =
    p.Substring(repoRoot.Length).Replace('\\', '/').TrimStart('/')

  /// 1 コマ進める呼び出しと、返り値を座標へ入れる行。
  /// `\b` で `Runner.step` を止めると stepWith に当たらず、控えから行が消える。
  let callsRun = Regex(@"Runner\.[Ss]tep")
  let usesResult = Regex(@"result\.[XY]\b|\.Delta\.[XY]\b")
  /// `self.X <- self.X + ...` / `self.X = self.X + ...` の形（足しているか代入か）
  let movesPos = Regex(@"\.[XY]\s*(<-|=)\s*[^;]*\.[XY]\s*[+\-]")

  [<Test>]
  member _.``run の返り値の使われ方を固める``() =
    let files = sources ()
    // 当てる先が本当に在るかを先に見る。0 本なら緑にしない
    if List.length files < 10 then
      Assert.Fail(sprintf "src と samples のソースが %d 本しか見つかりません。走査が壊れています" (List.length files))

    // run の経路に居るファイルだけを本体にする。座標を足す行は自機や背景にもあり、
    // 混ぜると自機の速度を変えただけで赤くなる
    let inPipeline (text: string) =
      callsRun.IsMatch text || usesResult.IsMatch text || text.Contains "RunTask"

    let read = files |> List.map (fun f -> relative f, File.ReadAllLines f)

    // コメントだけの行は外す。見たいのは呼び出しで、散文ではない。
    // 行の途中から始まるコメントは外していない。狭めると、同じ行の呼び出しを落とす。
    let isCommentOnly (l: string) = l.Trim().StartsWith "//"

    // 属性だけの行も外す。
    let isAttributeOnly (l: string) =
      let t = l.Trim()
      t.StartsWith "[<" && t.EndsWith ">]"

    let pick (lines: string[]) =
      lines
      |> Array.filter (fun l -> not (isCommentOnly l) && not (isAttributeOnly l))
      |> Array.filter (fun l -> callsRun.IsMatch l || usesResult.IsMatch l || movesPos.IsMatch l)
      // 行番号は入れない。行を動かしただけで赤くしても意味が無い
      |> Array.map (fun l -> Regex.Replace(l.Trim(), @"\s+", " "))
      |> List.ofArray

    let core, others =
      read
      |> List.map (fun (name, lines) -> name, lines, pick lines)
      |> List.filter (fun (_, _, hit) -> not (List.isEmpty hit))
      |> List.partition (fun (_, lines, _) -> inPipeline (String.concat "\n" lines))

    let lines =
      [ yield "step が返すのは差分。呼ぶ側が座標に足す。"
        yield "係数と Y の符号はフロントごとに違う（MonoGame は 1 倍、Unity2D は 1/100 で Y を反転）。"
        yield "MagicOnion の サーバーは Unity2D と同じ —— **空間 を決めて配るのがサーバー**なので、換算 もそちら側 に在る。"
        yield ""
        yield "**面（Playground）は Danmaku Lab へ出た**（v5.4）。あちらの Playfield.fs も"
        yield "同じ規約で足すが、**この網はこの repo しか見ない** ——"
        yield "向こうが規約を破っても、ここは緑のまま。"
        yield ""
        for (file, _, hit) in core |> List.sortBy (fun (n, _, _) -> n) do
          yield file
          for l in hit do yield sprintf "    %s" l
          yield ""
        // 絞った外も出す。件数だけにしておくと、新しいフロントが増えたときは
        // ファイル名が増えて赤くなり、自機の速度をいじっただけでは動かない
        yield "run の経路に居ないが、座標を足している所（件数だけ）:"
        if List.isEmpty others then yield "  なし"
        else
          for (file, _, hit) in others |> List.sortBy (fun (n, _, _) -> n) do
            yield sprintf "  %-64s %d 行" file (List.length hit)
        yield ""
        // 0 件を緑にしないための締め
        yield sprintf "―― 本体 %d ファイル / %d 行（0 なら走査か網が壊れている）"
                (List.length core) (core |> List.sumBy (fun (_, _, h) -> List.length h)) ]

    Assert.That(core |> List.sumBy (fun (_, _, h) -> List.length h), Is.GreaterThan 5,
                "当てる先が少なすぎます。網が壊れています")
    String.concat "\n" lines |> Golden.check "calling-convention"
