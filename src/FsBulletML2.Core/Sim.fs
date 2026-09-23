namespace FsBulletML2

open FsBulletML2.Domain

/// 1 コマの途中結果。効果をリストで繋ぐと二乗になる。参照型に戻すと bind の回数だけヒープを踏む。
[<Struct>]
type internal SimResult<'a> =
  { Value : 'a
    State : BulletState
    /// 積むものが無いときは ValueNone。id を入れると bind のたびに空の関数ができる。
    Emit : (Effect list -> Effect list) voption }

/// 環境を読み、状態を持ち回り、効果を書く。単一ケースの DU にすると InlineIfLambda が届かない。
type internal Sim<'a> = Env -> BulletState -> SimResult<'a>

module Sim =

  let internal ret x : Sim<'a> = fun _ st -> { Value = x; State = st; Emit = ValueNone }

  /// inline + InlineIfLambda。外すと bind 1 回 につきクロージャが 1 個 増える。
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

  /// まとめて積む。1 個ずつ bind すると repeat の周のぶんスタックが伸びる。
  let internal emitMany (es: Effect list) : Sim<unit> =
    fun _ st -> { Value = (); State = st; Emit = ValueSome (fun rest -> es @ rest) }

  /// 走らせて、効果を並びに潰す
  let internal run env st (m: Sim<'a>) =
    let r = m env st
    let effects = match r.Emit with ValueNone -> [] | ValueSome f -> f []
    r.Value, r.State, effects

/// メソッドを inline にしてある。非 inline だと Sim.bind の展開がここで止まる。
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

/// テスト用。inline を展開できないテスト側のため、中身は SimBuilder へ委譲するだけ。
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
