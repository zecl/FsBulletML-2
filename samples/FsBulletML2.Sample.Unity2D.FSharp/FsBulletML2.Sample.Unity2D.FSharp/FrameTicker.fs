namespace FsBulletML2.Sample.Unity2D.FSharp

open System
open R3
open UnityEngine

/// 毎コマ 1 個 流す発火源。このサンプルの R3 の入口はここ 1 つ。
///
/// --- なぜ `Observable.EveryUpdate` を使わないか
///
/// R3 には Unity 向けの面（`Observable.EveryUpdate`、`AddTo(Component)`、
/// `R3.Triggers`）があり、同梱の C# サンプルはそれを使っている。
[<DefaultExecutionOrder(-10000)>]
type FrameTicker () =
  inherit MonoBehaviour ()

  /// シーンに 1 つ。毎コマ 探さないための控え。
  /// F# の型に `null` は入れられないので defaultof で置く
  static let mutable instance : FrameTicker = Unchecked.defaultof<FrameTicker>

  /// F# は let 束縛を member より前に置くので、ここに居る
  let subject = new Subject<R3.Unit>()

  member private this.Stream = subject

  /// Unity 流の「生きているか」。
  ///
  /// `isNull (box o)` では足りない。 Unity は壊した component を
  /// null のように振る舞う非 null 参照にするので、素の参照比較では
  /// 「生きている」と読んでしまう。判定は `op_Equality`（Unity が
  /// その振る舞いを入れている演算子）に任せる。
  static member private Alive (o: UnityEngine.Object) =
    not (UnityEngine.Object.op_Equality(o, null))

  /// 毎コマ の流れ。呼ばれた時点で発火源が居なければ作る ——
  /// シーンに置き忘れても動くようにするため。
  ///
  /// Play 中でなければ作らない。 `Informations` は
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
    // 自分が控えなら外す。 残すと、シーンを組み直したあとに
    // 壊れた component の subject を配り続ける
    if System.Object.ReferenceEquals(instance, this) then
      instance <- Unchecked.defaultof<FrameTicker>

/// 購読の締め方。このサンプルでは必ずこれを通す。
///
/// R3.Unity なら `.Subscribe(...).AddTo(this)` と書くところ。
/// あちらが使えない事情は `FrameTicker` の但し書き。
/// `RegisterTo` は R3 本体なので、偽 UnityEngine でビルドする経路でも通る。
[<AutoOpen>]
module ObservableSubscription =

  /// 購読して、その component が壊れたときに切る。
  ///
  /// 切り忘れると、シーンを離れたあとも購読が残って
  /// 壊れた component を触りにいく（Unity は壊れた参照を null 相当に
  /// 見せるので、`MissingReferenceException` で初めて分かる）。
  let subscribeUntilDestroy (owner: MonoBehaviour) (action: 'T -> unit) (source: Observable<'T>) =
    source.Subscribe(Action<'T>(action)).RegisterTo(owner.destroyCancellationToken) |> ignore
