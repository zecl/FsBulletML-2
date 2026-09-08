namespace FsBulletML2.Playground

open System
open System.Collections.Generic
open System.Runtime.InteropServices
open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.Front

/// 参照型。struct だと `let it = live.[i]` がコピーになって書き戻せない。
[<Sealed>]
type Live(run: BulletRun, x: float32, y: float32, isRoot: bool) =
  member val Run = run with get, set
  member val X = x with get, set
  member val Y = y with get, set
  member _.IsRoot = isRoot

/// 生きている弾の一覧。Bolero を知らない。
///
/// 1 コマの順は MonoGame の `RunTask` と同じ
/// （`Driver.step` → Delta を足す → Spawn は次コマ → Finished なら restart）。
/// **回している最中に `live.Add` しない。** 今コマの Spawn は溜めて、消しのあとで足す。
[<Sealed>]
type Playfield private (front: IFrontEnv, live: ResizeArray<Live>, field: Field) =

  let spawned = ResizeArray<Live>()
  let mutable frame = 0
  let mutable xs = Array.zeroCreate<float32> 256
  let mutable pin = Unchecked.defaultof<GCHandle>

  let rePin () =
    if pin.IsAllocated then pin.Free()
    pin <- GCHandle.Alloc(xs, GCHandleType.Pinned)

  static member Create (front: IFrontEnv) (bulletml: Bulletml) =
    let script = Runner.load front.Rand front.Rank bulletml
    let run = Runner.newRoot BulletType.Enemy script
    // **面の形は弾幕が決める。** 横画面と名乗る弾幕は、縦の面に置くと
    // 弾が横へ抜けていく（`Stage.landscape` の但し書き）
    let field = Stage.ofDirection script.ShootingDirection
    let live = ResizeArray<Live>()
    live.Add(Live(run, field.EnemyX, field.EnemyY, true))
    Playfield(front, live, field)

  member _.Count = live.Count

  /// この面の形。**建てたあとは変わらない** —— 向きは弾幕が決めるので、
  /// 変わるときは弾幕が変わったとき、つまり建て直すとき
  member _.Field = field

  /// 進めたコマ数。**この面を建ててから何回 `Tick` したか**であって、
  /// 弾幕の中の時間ではない。建て直せば 0 に戻る（Reset / Apply / 選び直し）。
  ///
  /// 面が持つのは、飛ぶ側（`Seek`）が「いまどこか」を要るから ——
  /// 呼ぶ側で数えると、`Tick` を呼ぶ道が増えたときに数え漏れる。
  member _.Frame = frame

  member _.Tick() =
    spawned.Clear()
    let mutable i = 0
    while i < live.Count do
      let it = live.[i]
      let m0 = it.Run.Motion
      let motion =
        { Pos = { X = it.X; Y = it.Y }
          Speed = m0.Speed
          Dir = m0.Dir
          Accel = m0.Accel }
      let f = Driver.step front Space.YDown SpawnOrigin.AtShooter it.Run motion
      if not it.IsRoot then
        it.X <- it.X + f.Delta.X
        it.Y <- it.Y + f.Delta.Y
      it.Run <- if f.Finished then Driver.restart front f.Run else f.Run
      for child in f.Spawned do
        let p = child.Motion.Pos
        spawned.Add(Live(child, p.X, p.Y, false))
      let dead =
        not it.IsRoot && (
          f.Vanished || f.Retired
          || it.X < 0.0f || it.X > field.Width
          || it.Y < 0.0f || it.Y > field.Height)
      if dead then
        let last = live.Count - 1
        if i <> last then live.[i] <- live.[last]
        live.RemoveAt last
      else i <- i + 1
    live.AddRange spawned
    frame <- frame + 1

  /// WASM ヒープ上の `float32[]`。JS は JSON せず `localHeapViewF32` で読む。
  member _.Pack() =
    let n = live.Count
    let need = n * 2
    if xs.Length < need then
      xs <- Array.zeroCreate (max (need * 2) 256)
      rePin ()
    elif not pin.IsAllocated then
      rePin ()
    for i in 0 .. n - 1 do
      xs.[i * 2] <- live.[i].X
      xs.[i * 2 + 1] <- live.[i].Y
    n

  member _.PackedPtr =
    if not pin.IsAllocated then rePin ()
    float (pin.AddrOfPinnedObject().ToInt64())
