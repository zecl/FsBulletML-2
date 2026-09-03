namespace FsBulletML2.Core.Tests

/// 公開 API で 1 本 走らせる入口。**軌跡を文字列で返すのは Trace.run と同じ。**
///
/// 互換の口を落とす前、テストの多くは fixture の SetUp で
/// `BulletMLManager.Init(FixedManager(rand, rank, px, py))` を呼び、
/// `Trace.run xml frames` で走らせていた。**グローバル可変（static mutable）を
/// 触るので fixture を NonParallelizable にする必要があり**、しかも
/// 「その走行がどの乱数・どの自機位置で回っているか」は SetUp まで
/// 遡らないと読めなかった。
///
/// ここは同じ 4 値 を引数で受け取る。**グローバルに触らないので並列に走る。**
/// 値の組は旧い SetUp に出ていたものをそのまま名前にしてある —— 移すときに
/// 「同じ値で走っている」ことを 1 行 で確かめられるようにするため。
module TraceRun =

  /// 既定。旧い SetUp のうち最も多かった FixedManager(0.5f, 0.5f, 30.0f, 100.0f)。
  /// 自機は (30, 100) なので aim = atan2(30, -100) = 2.850
  let std (xml: string) (frames: int) : string =
    TraceApi.run (fun () -> 0.5f) 0.5f 30.0f 100.0f xml frames

  /// 自機を真下に置く形（旧 SetUp の FixedManager(0.5f, 0.5f, 0.0f, 100.0f)）。
  /// aim = atan2(0, -100) = π なので、狙いが真下に固定される
  let atOrigin (xml: string) (frames: int) : string =
    TraceApi.run (fun () -> 0.5f) 0.5f 0.0f 100.0f xml frames

  /// 乱数と rank を指定する。自機は std と同じ (30, 100)
  let withRandRank (rand: float32) (rank: float32) (xml: string) (frames: int) : string =
    TraceApi.run (fun () -> rand) rank 30.0f 100.0f xml frames

  /// 自機の位置を指定する。乱数と rank は std と同じ 0.5
  let atPlayer (px: float32) (py: float32) (xml: string) (frames: int) : string =
    TraceApi.run (fun () -> 0.5f) 0.5f px py xml frames
