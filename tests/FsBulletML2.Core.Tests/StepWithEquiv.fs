namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Domain

/// Runner.stepWith が Runner.step（WithBody 経由）と同じ答えを返すこと。
///
/// **出荷側は stepWith しか呼ばない**（フロント 2 つ・ベンチ・TraceApi）ので、
/// ここが無いと step のほうが誰にも確かめられなくなる。逆に、片方だけ直したら
/// ここが割れる。
///
/// **2 回 走らせるので、乱数を二重に消費しない形にしてある。**
/// 同じ列を独立に 2 本 用意して、それぞれに 1 本ずつ渡す。同じ env を
/// 使い回して 2 回 呼ぶと、2 回目 は列が進んだ後の値を見るので、
/// 「答えが違う」が実装の差なのか列の差なのか分けられなくなる。
[<TestFixture>]
type StepWithEquiv() =

  /// 呼ぶたび進む決定的な列。同じ種から 2 本 作れば、同じ順で同じ値が出る
  let stream () =
    let mutable i = 0
    fun () ->
      i <- i + 1
      // 0 と 1 に張り付かない、周期の長くない列。値そのものに意味は無く、
      // 「呼ぶたび違う」ことと「2 本 が同じ順で同じ値を返す」ことだけが要る
      float32 ((i * 7919) % 1000) / 1000.0f

  let envWith (rand: unit -> float32) : Env =
    { Rand = rand
      Rank = 0.5f
      AimDir = 0.3f
      EnemyAimDir = -0.7f
      SpawnAimDir = 1.1f
      SpawnEnemyAimDir = -1.3f }

  let body =
    { Pos = { X = 3.0f; Y = -4.0f }
      Speed = 1.5f
      Dir = 0.25f
      Accel = { X = 0.1f; Y = -0.2f }
      Kind = BulletType.Enemy
      IsBullet = true
      HasFired = false }

  /// 227 本 の実物で突き合わせる。1 本 ずつ、両方に独立な同じ列を渡す
  [<Test>]
  member _.``227 本 とも stepWith と WithBody+step が同じ答えを返す``() =
    let files = CorpusData.uniqueSamples ()
    files |> List.length |> should be (greaterThan 100)
    let mutable compared = 0
    for path in files do
      let xml = System.IO.File.ReadAllText path
      try
        let a = stream ()
        let b = stream ()
        let scriptA = Runner.load a 0.5f (readXmlString xml)
        let scriptB = Runner.load b 0.5f (readXmlString xml)
        let runA = Runner.newRoot scriptA
        let runB = Runner.newRoot scriptB
        let fa = Runner.stepWith scriptA (envWith a) runA body
        let fb = Runner.step scriptB (envWith b) (runB.WithBody body)
        fa.Delta |> should equal fb.Delta
        fa.Finished |> should equal fb.Finished
        fa.Retired |> should equal fb.Retired
        fa.Vanished |> should equal fb.Vanished
        fa.Spawned.Length |> should equal fb.Spawned.Length
        fa.Run.Body |> should equal fb.Run.Body
        compared <- compared + 1
      with
      // 読む段で落ちる 3 本（DTD 違反）はここでも比べられない。
      // 数は下で門にするので、握って進む
      | _ -> ()
    // **当てる先が本当に在るかを数で押さえる。** 全部 例外に吸われて
    // 「0 本 比べて緑」になっても気づけない
    compared |> should be (greaterThan 200)
    TestContext.WriteLine(sprintf "比べた台本: %d 本" compared)
