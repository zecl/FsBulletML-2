namespace FsBulletML2.Sample.Unity2D.FSharp

open System
open R3
open UnityEngine

/// このサンプルの R3 の入口はここ 1 つ。`EveryUpdate` は `R3.Unity` に居て、ここでは使えない。
[<DefaultExecutionOrder(-10000)>]
type FrameTicker() =
    inherit MonoBehaviour()

    /// 毎コマ 探さない控え。F# の型に null は入れられないので defaultof。
    static let mutable instance: FrameTicker = Unchecked.defaultof<FrameTicker>

    /// F# は let 束縛を member より前に置くので、ここに居る
    let subject = new Subject<R3.Unit>()

    member private this.Stream = subject

    /// `isNull` では足りない。壊した component は非 null のまま null のように振る舞う。
    static member private Alive(o: UnityEngine.Object) =
        not (UnityEngine.Object.op_Equality (o, null))

    /// 居なければ作る。Play 中でなければ作らない。Editor で作るとシーンを汚す。
    static member Frames: Observable<R3.Unit> =
        if not (FrameTicker.Alive instance) then
            let found = UnityEngine.Object.FindAnyObjectByType<FrameTicker>()

            if FrameTicker.Alive found then
                instance <- found
            elif Application.isPlaying then
                let go = new GameObject("FrameTicker")
                UnityEngine.Object.DontDestroyOnLoad go
                instance <- go.AddComponent<FrameTicker>()

        if not (FrameTicker.Alive instance) then
            R3.Observable.Empty<R3.Unit>()
        else
            instance.Stream :> Observable<R3.Unit>

    member this.Update() = subject.OnNext R3.Unit.Default

    member this.OnDestroy() =
        subject.Dispose()
        // 自分が控えなら外す。 残すと、シーンを組み直したあとに
        // 壊れた component の subject を配り続ける
        if System.Object.ReferenceEquals(instance, this) then
            instance <- Unchecked.defaultof<FrameTicker>

/// 購読は必ずここを通す。`AddTo` は使えず、`RegisterTo` は偽 UnityEngine でも通る。
[<AutoOpen>]
module ObservableSubscription =

    /// 壊れたときに切る。切り忘れると、離れたあとも壊れた component を触りにいく。
    let subscribeUntilDestroy (owner: MonoBehaviour) (action: 'T -> unit) (source: Observable<'T>) =
        source.Subscribe(Action<'T>(action)).RegisterTo(owner.destroyCancellationToken)
        |> ignore
