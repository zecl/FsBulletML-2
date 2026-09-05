namespace FsBulletML2.Core.Tests

open System.IO
open System.Text.RegularExpressions
open NUnit.Framework

/// 1 コマ進める呼び出しが返すものを、呼ぶ側がどう使っているかを固める。
///
/// 返るのは **差分**（そのフレームの移動量）で、呼ぶ側が座標に足す。
/// 3 で見つけたとおり、絶対値を返す枝に届くと座標が膨らむ。膨らむ量は
/// 呼ぶ側の係数しだいで、同梱では MonoGame が 1 倍、Unity2D が 1/100。
///
/// **フロントは Core.Tests から呼べない**（MonoGame と UnityEngine が要る）。
/// なのでソースを読む門にする。壊れ方は 3 通りあって、どれもここに出る。
///
///   1  足すのをやめて代入にした（差分を絶対値として扱う）
///   2  係数を 1 か所だけ変えた（front-end の間でずれる）
///   3  Y の符号を 1 か所だけ変えた（Unity は下が正なので引いている）
///
/// **古びやすい門**（行を動かすと赤くなる）ことは承知のうえ。
/// Core を直したときにフロントで壊れることを、他に測る手が無い。
///
/// **網が前提にしている書き方**（いまの 6 ファイルは全部この形）。
///
///   let result = ...step...              返り値の名前が `result`
///   self.X <- self.X + (x / 100)         代入の左が `.X` / `.Y` で終わる
///
/// **この形から外れると、係数の行だけが控えから消える。**
///
///   var d = Runner.step(...)                              step の行は載る
///   self.Position = self.Position + new Vector2(d.X, d.Y) **この行は載らない**
///
/// 変数名が `result` でなく、代入の左が `.Position` なのでどちらの網にも当たらない。
/// **ファイル自体は run を呼ぶので控えに出る**（だから「新しいフロントが増えても
/// 何も起きない」ではない）。**出ないのは「どう座標へ入れたか」の行のほう。**
///
/// 網を広げると自機や背景の移動を拾い直すほうへ戻るので、
/// **前提を書いて残すほうを採った。** 控えにファイル名だけの行が出ていたら、
/// その形になっていないかを見ること。
///
/// **`NonParallelizable` を付けていない。** ここはソースを読むだけで走らせない。
/// 以前は「他の fixture はグローバル可変（`BulletMLManager`）を触るので
/// 逐次でないと混ざるが、ここは違う」と書いてあったが、**この試験
/// プロジェクトにグローバルを触る fixture はもう 1 つ も無い**
/// （口ごと `FsBulletML2.Front` へ移した）。
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
  ///
  ///   Runner.step / stepWith    → Frame.Delta.X / .Y
  ///
  /// **入口の名前で当てる網は、入口が増えるたびに漏れる。** 三度 踏んだ ——
  /// 新 API を足したとき、Obsolete の説明文に当たったとき、そして
  /// `Runner.step` → `Runner.stepWith` に移したとき（`\b` が効いて
  /// stepWith に当たらず、控えから 2 行 消えた）。
  ///
  /// だから `Runner\.[Ss]tep` で始まりだけを見る。名前の続きは問わない。
  /// **控えの行数が減ったら、まず網が漏れていないかを疑うこと**
  /// —— 減った行は「消えた呼び出し」ではなく「見えなくなった呼び出し」で
  /// あることが、ここでは 3 回中 3 回 だった。
  ///
  /// **旧の腕（`BulletRunner\.run\b`）は外した。** `BulletRunner` を消したので
  /// 当てる先が 0 になった。**4 回目 は「消えた呼び出し」のほうだった** ——
  /// 控えから `src/FsBulletML2.Core/BulletRunner.fs` の 1 行 が落ちるのが正しい。
  ///
  /// **`samples/.../Unity2D.CSharp` も新 API へ移した。** あそこは
  /// `FsBulletML2.sln` に入っていないので**この門でしか見えない** ——
  /// 実際、旧 API を落とすより前から壊れていた（`BulletMLManager` を
  /// namespace 直下へ出したとき、`.cs` は直したのに同梱 dll を焼き直して
  /// いなかった）。**sln の外は、控えの行が消えたときにしか気づけない。**
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

    // コメントだけの行は外す。**この門が見たいのは呼び出しで、散文の中の
    // 言及ではない。** 網は文字で当てるので、「Runner.step に渡す」と
    // 書いた doc コメントにも当たり、説明を書き足しただけで控えが割れる
    // （実際に割れた）。
    //
    // 外して安全なのは、`//` で始まる行が F# でも C# でも定義上 呼び出しに
    // ならないため。**行の途中から始まるコメントは外していない** ——
    // そこは同じ行に呼び出しが在りうるので、狭めると本物を落とす。
    let isCommentOnly (l: string) = l.Trim().StartsWith "//"

    // 属性だけの行も外す。**同じ理由**（呼び出しではなく、書かれた説明）。
    //
    // 二度 踏んだ。1 度目 は doc コメント、2 度目 は
    // [<System.Obsolete("新 API（Runner.step）へ移してください…")>] の
    // 説明文が網に当たった。**非推奨の案内には、移り先の名前を書くのが
    // 当たり前**なので、移り先を網にした瞬間に必ずぶつかる。
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
