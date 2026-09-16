namespace FsBulletML2

open FsBulletML2.Domain

/// 1 コマ進めるあいだの、途中の結果。
///
/// `Effects` でなく `Emit`（関数）なのは、中身が差分リストだから。
/// bind のたびに `@` で繋ぐと O(n^2) になるので、最後に 1 回だけ空リストに当てて潰す。
///
/// [<Struct>] にしてある（Sim を 1 段 進めるたびに必ず 1 個 出る。
/// homing laser は 1 走行で bind が 26,999 回）。
[<Struct>]
type internal SimResult<'a> =
  { Value : 'a
    State : BulletState
    /// 効果を積む関数。積むものが無いときは ValueNone。
    ///
    /// 以前は `id` を入れていたが、bind は合成のたびに新しい関数を作る ——
    /// 効果を積むのは `emit` / `emitMany` だけなので、大半の bind が
    /// `id >> id` という何もしない関数を 1 個 作っていた。
    Emit : (Effect list -> Effect list) voption }

/// 環境を読み、弾の状態を持ち回り、効果を書く。
/// 包みを持たない（DU にすると InlineIfLambda が届かない）。
type internal Sim<'a> = Env -> BulletState -> SimResult<'a>

module Sim =

  let internal ret x : Sim<'a> = fun _ st -> { Value = x; State = st; Emit = ValueNone }

  /// inline + InlineIfLambda。 展開すると `f` の本体が外側の
  /// `fun env st -> ...` の中に埋まるので、bind 1 回 につき出ていた
  /// クロージャが 2 個 から 1 個 になる。
  let inline internal bind ([<InlineIfLambda>] f: 'a -> Sim<'b>) (m: Sim<'a>) : Sim<'b> =
    fun env st ->
      let r1 = m env st
      let r2 = (f r1.Value) env r1.State
      // 片方が空なら合成しない。向きは r2.Emit >> r1.Emit のまま
      // （後で積んだ効果が先頭に来る向きへ戻さないこと）
      let emit =
        match r1.Emit, r2.Emit with
        | ValueNone, e -> e
        | e, ValueNone -> e
        | ValueSome a, ValueSome b -> ValueSome (b >> a)
      { r2 with Emit = emit }

  let internal ask : Sim<Env> = fun env st -> { Value = env; State = st; Emit = ValueNone }
  let internal get : Sim<BulletState> = fun _ st -> { Value = st; State = st; Emit = ValueNone }
  let internal put s : Sim<unit> = fun _ _ -> { Value = (); State = s; Emit = ValueNone }

  let internal emit (e: Effect) : Sim<unit> =
    fun _ st -> { Value = (); State = st; Emit = ValueSome (fun rest -> e :: rest) }

  /// emit の複数版。1 個ずつ `Sim.bind` で繋ぐと繋ぐ数だけ bind が積み重なり、
  /// 要素数ぶんスタックが伸びる（repeat の周のような手続き的なループ）。
  let internal emitMany (es: Effect list) : Sim<unit> =
    fun _ st -> { Value = (); State = st; Emit = ValueSome (fun rest -> es @ rest) }

  /// 走らせて、効果を並びに潰す
  let internal run env st (m: Sim<'a>) =
    let r = m env st
    let effects = match r.Emit with ValueNone -> [] | ValueSome f -> f []
    r.Value, r.State, effects

/// メソッドを inline にしてある。 `Sim.bind` を inline にしても、CE が
/// 通るのはこのビルダのメソッドなので、ここが非 inline だとそこで展開が止まる。
type internal SimBuilder() =
  member inline _.Return x = Sim.ret x
  member inline _.ReturnFrom (m: Sim<'a>) = m
  member inline _.Bind (m: Sim<'a>, [<InlineIfLambda>] f: 'a -> Sim<'b>) = Sim.bind f m
  /// `let! x = m` の次が `return f x` のとき、F# は Bind + Return でなくこちらを選ぶ。
  member inline _.BindReturn (m: Sim<'a>, [<InlineIfLambda>] f: 'a -> 'b) : Sim<'b> =
    fun env st ->
      let r = m env st
      { Value = f r.Value; State = r.State; Emit = r.Emit }
  member inline _.Zero () = Sim.ret ()
  member _.Delay (f: unit -> Sim<'a>) = f
  member _.Run (f: unit -> Sim<'a>) = f ()
  member inline _.Combine (a: Sim<unit>, [<InlineIfLambda>] b: unit -> Sim<'a>) =
    Sim.bind (fun () -> b ()) a
  member inline _.For (xs: seq<'x>, [<InlineIfLambda>] f: 'x -> Sim<unit>) =
    Seq.fold (fun acc x -> Sim.bind (fun () -> f x) acc) (Sim.ret ()) xs
  member this.While (guard, body) =
    if guard () then Sim.bind (fun () -> this.While (guard, body)) (body ()) else Sim.ret ()

/// テストから CE を書くための入口。SimBuilder と inline 属性だけが違う。
///
/// テストのアセンブリからは inline なメンバを展開できない。
/// ここで 1 枚 包み、展開を Core の中で済ませる。
///
/// メンバは 1 つ も独自の中身を持たず、全部 `SimBuilder` へ委譲する ——
/// だから意味論は本番と同じ道を通る。
type internal SimBuilderForTests() =
  let b = SimBuilder()
  member _.Return x = b.Return x
  member _.ReturnFrom (m: Sim<'a>) = b.ReturnFrom m
  member _.Bind (m: Sim<'a>, f: 'a -> Sim<'b>) = b.Bind (m, f)
  member _.BindReturn (m: Sim<'a>, f: 'a -> 'b) = b.BindReturn (m, f)
  member _.Zero () = b.Zero ()
  member _.Delay (f: unit -> Sim<'a>) = b.Delay f
  member _.Run (f: unit -> Sim<'a>) = b.Run f
  member _.Combine (a: Sim<unit>, f: unit -> Sim<'a>) = b.Combine (a, f)
  member _.For (xs: seq<'x>, f: 'x -> Sim<unit>) = b.For (xs, f)
  member _.While (guard, body) = b.While (guard, body)

[<AutoOpen>]
module SimBuilderInstance =
  let internal sim = SimBuilder()

  /// テストが使う入口。Core の中では sim を使うこと（SimBuilderForTests の但し書き）
  let internal simForTests = SimBuilderForTests()
