namespace FsBulletML2.Sample.Unity2D.FSharp

open System
open R3
open UnityEngine

/// 毎コマ 1 個 流す発火源。**このサンプルの R3 の入口はここ 1 つ。**
///
/// --- なぜ `Observable.EveryUpdate` を使わないか
///
/// R3 には Unity 向けの面（`Observable.EveryUpdate`、`AddTo(Component)`、
/// `R3.Triggers`）があり、同梱の C# サンプルはそれを使っている。
/// **こちらは使えない。** それらは `R3.Unity`（`com.cysharp.r3`）に居て、
/// Unity がビルドしたアセンブリでしか手に入らない。
///
/// このサンプルは F# を Unity の外でビルドして dll を Assets へ置く形で、
/// sln のビルドには `UnityEngine.Stub`（コンパイル専用の偽 UnityEngine）を通す。
/// 偽物に `R3.Unity` の面を足すには `namespace R3` に `static class Observable`
/// をもう 1 つ 置くことになり、**NuGet の本物と衝突する**（CS0433）。
/// C# なら `extern alias` で割れるが、**F# にその構文は無い。**
///
/// そこで毎コマ の発火だけ自分で持つ。**使うのは R3 本体だけ**になり、
/// 偽物に足すのは `MonoBehaviour.destroyCancellationToken` の 1 つ で済む。
///
/// --- 実行順
///
/// **誰より先に流す。** 入力を取って撃つ購読はここから走るので、
/// 弾を動かす `BulletEcsDriver`（`DefaultExecutionOrder(-100)`）より前に置く。
/// 撃った弾がそのコマのうちに動き出す。
///
/// **`BulletEcsDriver` は R3 にしていない。** あれは毎コマ の固定処理で、
/// 実行順そのものに意味がある —— 購読の順に流れる形にすると、
/// 誰がいつ購読したかで順番が変わる。C# サンプルも同じ判断で、
/// あちらの弾の駆動は R3 ではなく `SystemBase` に置かれている。
[<DefaultExecutionOrder(-10000)>]
type FrameTicker () =
  inherit MonoBehaviour ()

  /// シーンに 1 つ。**毎コマ 探さないための控え。**
  /// F# の型に `null` は入れられないので defaultof で置く
  static let mutable instance : FrameTicker = Unchecked.defaultof<FrameTicker>

  /// **F# は let 束縛を member より前に置く**ので、ここに居る
  let subject = new Subject<R3.Unit>()

  member private this.Stream = subject

  /// Unity 流の「生きているか」。
  ///
  /// **`isNull (box o)` では足りない。** Unity は壊した component を
  /// **null のように振る舞う非 null 参照**にするので、素の参照比較では
  /// 「生きている」と読んでしまう。判定は `op_Equality`（Unity が
  /// その振る舞いを入れている演算子）に任せる。
  ///
  /// **これを見落として実際に踏んだ。** 壊れた発火源を控えに残したまま
  /// 配り続け、新しい発火源を回しても購読へ届かない
  /// —— 弾が 1 発 も出なくなる。`Assets/Editor/EnemyShootCheck.cs` が
  /// 捕まえた（門を 2 本 続けて回したときだけ出る）。
  ///
  /// Play を止めたあとも同じ形になりうる。Enter Play Mode Options で
  /// Domain Reload を切っていると static が持ち越されるため。
  static member private Alive (o: UnityEngine.Object) =
    not (UnityEngine.Object.op_Equality(o, null))

  /// 毎コマ の流れ。**呼ばれた時点で発火源が居なければ作る** ——
  /// シーンに置き忘れても動くようにするため。
  ///
  /// **Play 中でなければ作らない。** `Informations` は
  /// `[<ExecuteInEditMode()>]` なので Editor でも購読しにきうる。
  /// そこで GameObject を作ると Editor のシーンを汚すし、
  /// `DontDestroyOnLoad` は Play 中でないと例外になる。
  /// （シーンに既に居れば、Play 中でなくても配る）
  static member Frames : Observable<R3.Unit> =
    if not (FrameTicker.Alive instance) then
      let found = UnityEngine.Object.FindAnyObjectByType<FrameTicker>()
      if FrameTicker.Alive found then
        instance <- found
      elif Application.isPlaying then
        let go = new GameObject("FrameTicker")
        UnityEngine.Object.DontDestroyOnLoad go
        instance <- go.AddComponent<FrameTicker>()

    if not (FrameTicker.Alive instance) then R3.Observable.Empty<R3.Unit>()
    else instance.Stream :> Observable<R3.Unit>

  member this.Update () = subject.OnNext R3.Unit.Default

  member this.OnDestroy () =
    subject.Dispose()
    // **自分が控えなら外す。** 残すと、シーンを組み直したあとに
    // 壊れた component の subject を配り続ける
    if System.Object.ReferenceEquals(instance, this) then
      instance <- Unchecked.defaultof<FrameTicker>

/// 購読の締め方。**このサンプルでは必ずこれを通す。**
///
/// R3.Unity なら `.Subscribe(...).AddTo(this)` と書くところ。
/// あちらが使えない事情は `FrameTicker` の但し書き。
/// **`RegisterTo` は R3 本体**なので、偽 UnityEngine でビルドする経路でも通る。
[<AutoOpen>]
module ObservableSubscription =

  /// 購読して、**その component が壊れたときに切る。**
  ///
  /// 切り忘れると、シーンを離れたあとも購読が残って
  /// 壊れた component を触りにいく（Unity は壊れた参照を null 相当に
  /// 見せるので、`MissingReferenceException` で初めて分かる）。
  ///
  /// パイプの終端に置く形にしてあるので、`Observable.filter` などと
  /// そのまま繋がる ——
  ///
  ///     FrameTicker.Frames
  ///     |> Observable.filter (fun _ -> Input.GetKey KeyCode.Z)
  ///     |> subscribeUntilDestroy this (fun _ -> this.Shoot ())
  let subscribeUntilDestroy (owner: MonoBehaviour) (action: 'T -> unit) (source: Observable<'T>) =
    source.Subscribe(Action<'T>(action)).RegisterTo(owner.destroyCancellationToken) |> ignore
