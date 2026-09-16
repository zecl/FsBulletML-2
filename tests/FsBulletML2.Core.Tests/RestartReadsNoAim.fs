namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Domain

/// `Runner.restart` が aim を読まないこと。
///
/// `BulletRun.HasNoScript` の但し書きが逆のことを書いていた ——
/// 「restart は changeDirection type="aim" の term を引き直すので aim を
/// 読みうる」。引き直すのは `<term>`（数式）で、`getValue` が触るのは
/// `Rand` と `Rank` だけ（`Eval.fs`）。aim はどこにも入らない。
/// これが効くのは 1 つ。フロントが `restart` の前に aim を組むのは
/// 捨てる計算で、Atan2 4 本 と、ゲームへの問い合わせ 2 回 が
/// 弾が終わるたびに無駄になる。
[<TestFixture>]
type RestartReadsNoAim() =

  /// `restart` に渡すもの以外は全部 同じにする
  let envOf (rand: float32) (aim: float32) : Env =
    { Rand = (fun () -> rand)
      Rank = 0.5f
      Aim = { ToPlayer = aim; ToEnemy = aim * 2.0f }
      Spawn = { ToPlayer = aim * 3.0f; ToEnemy = aim * 4.0f } }

  /// 走らせ直した弾を 20 コマ 回して、軌跡を文字列にする。
  /// restart の中身は不透明なので、そのあとの振る舞いで比べる
  let traceAfterRestart (script: BulletmlScript) (restartRand: float32) (restartAim: float32) =
    let sb = System.Text.StringBuilder()
    let run0 = Runner.newRoot BulletType.Enemy script
    // 1 コマ 進めてから走らせ直す。進めないと Progress が初期のままで、
    // 引き直しても同じものになり、当てる先が消える
    let f0 = Runner.stepWith (envOf 0.5f 0.25f) run0 Motion.zero
    let mutable run = Runner.restart (envOf restartRand restartAim) f0.Run
    for _ in 1 .. 20 do
      // 比べる走行の env は両方 同じ。違うのは restart に渡したものだけ
      let f = Runner.stepWith (envOf 0.5f 0.25f) run Motion.zero
      sb.AppendLine(sprintf "%f %f %f %f %b %b %b %d"
                      f.Delta.X f.Delta.Y f.Run.Motion.Dir f.Run.Motion.Speed
                      f.Vanished f.Finished f.Retired (List.length f.Spawned)) |> ignore
      run <- f.Run
    sb.ToString()

  /// 台本を 1 本ずつ、2 通り の `restart` で回して割れた数を返す
  let compareOver (a: float32 * float32) (b: float32 * float32) =
    let files = CorpusData.uniqueSamples ()
    let mutable compared = 0
    let mutable diverged = []
    for path in files do
      let xml = System.IO.File.ReadAllText path
      try
        let script = Runner.load (fun () -> 0.5f) 0.5f (readXmlString xml)
        let x = traceAfterRestart script (fst a) (snd a)
        let y = traceAfterRestart script (fst b) (snd b)
        compared <- compared + 1
        if x <> y then diverged <- path :: diverged
      with
      // 読む段で落ちる弾幕（DTD 違反）はここでも比べられない
      | _ -> ()
    compared, List.rev diverged

  [<Test>]
  member _.``227 本 とも、aim を変えても走らせ直しの結果は同じ``() =
    let compared, diverged = compareOver (0.5f, 0.0f) (0.5f, 1.75f)
    // 当てる先が本当に在るかを数で押さえる
    compared |> should be (greaterThan 200)
    if not (List.isEmpty diverged) then
      Assert.Fail(sprintf "%d 本 で割れた。restart は aim を読んでいる。最初: %s"
                    (List.length diverged) (List.head diverged))
    TestContext.WriteLine(sprintf "比べた台本: %d 本" compared)

  /// 較正。 上の門が「そもそも restart を観測していない」ではないことを
  /// 見る。`Wait` の値だけは `getValue` の戻りが残る（`Step.fs` の
  /// `PWait (true, getValue env s)`）ので、`$rand` を変えれば動くはず。
  ///
  /// `Rank` では較正できなかった。 `ChangeDirection` / `ChangeSpeed` の
  /// term は `getValue env t |> ignore` で捨てられていて、残るのは
  /// `Wait` だけ。`$rank` を待ち時間に使う弾幕がコーパスに無かった
  [<Test>]
  member _.``較正: 乱数を変えると走らせ直しの結果は動く``() =
    let compared, diverged = compareOver (0.1f, 0.0f) (0.9f, 0.0f)
    compared |> should be (greaterThan 200)
    // 全部 動く必要は無い（`$rand` を待ちに使わない弾幕が多い）が、
    // 1 本 も動かないなら、この比べ方が restart を見ていない
    diverged |> List.length |> should be (greaterThan 0)
    TestContext.WriteLine(
      sprintf "%d 本 中 %d 本 が乱数で動いた" compared (List.length diverged))
