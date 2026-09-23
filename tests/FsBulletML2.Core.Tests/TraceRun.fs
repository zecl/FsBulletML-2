namespace FsBulletML2.Core.Tests

/// 公開 API で 1 本 走らせる入口。
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
