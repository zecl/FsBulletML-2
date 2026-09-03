namespace FsBulletML2

open FsBulletML2.Domain

/// 1 コマ進めるあいだの、途中の結果。
///
/// Effects でなく Emit なのは、中身が差分リストだから。bind のたびに
/// @ で繋ぐと O(n^2) になるので、最後に 1 回だけ空リストに当てて潰す。
///
/// **[<Struct>] にしてある。** これは Sim を 1 段 進めるたびに必ず 1 個 出る。
/// homing laser は 1 走行で bind が 26,999 回 なので、参照型だとその回数だけ
/// ヒープを踏む。
///
/// Env を struct にした手で homing の確保が 4% 増えたのは、ここが参照型の
/// まま `SimResult<Env>` を作っていて、**中の Env が 8 バイトの参照から
/// 32 バイトの実体に太った**ため。箱を消せば太りごと消える。
/// Emit は関数だが、Env の Rand と同じで、関数を持つことと包みが参照型で
/// あることは別。
[<Struct>]
type internal SimResult<'a> =
  { Value : 'a
    State : BulletState
    /// 効果を積む関数。**積むものが無いときは ValueNone。**
    ///
    /// 以前は id を入れていたが、bind は合成のたびに `r2.Emit >> r1.Emit` で
    /// 新しい関数を作る。効果を積むのは emit / emitMany だけで、
    /// ret / ask / get / put は全部 id —— **大半の bind が id >> id という
    /// 何もしない関数を 1 個 作っていた。** 空を型で持てば、その合成が消える。
    Emit : (Effect list -> Effect list) voption }

/// 環境を読み、弾の状態を持ち回り、効果を書く。
/// Reader + State + Writer を 1 本に畳んだもの。
///
/// 途中の結果を 3 つ組のタプルでなくレコードにしてあるのは、bind の中で
/// 状態を取り違えないため。位置ではなく名前で受ける
///
/// BulletState / Effect を内側に持つため internal。両方とも internal
/// Progress / RecBulletml を辿って internal になっているので、それを
/// 運ぶ Sim もそこから見えない外へは出さない
type internal Sim<'a> = Sim of (Env -> BulletState -> SimResult<'a>)

module Sim =

  let inline private unwrap (Sim f) = f

  let internal ret x : Sim<'a> = Sim (fun _ st -> { Value = x; State = st; Emit = ValueNone })

  /// Emit は「残りの前に自分を足す」向きの関数なので、m の次に f を書いても
  /// 合成は r2.Emit >> r1.Emit になる（先に評価されるのが右）。これを
  /// r1.Emit >> r2.Emit に戻すと、後で積んだ効果が先頭に来る向きへ逆戻りする
  let internal bind (f: 'a -> Sim<'b>) (m: Sim<'a>) : Sim<'b> =
    Sim (fun env st ->
      let r1 = unwrap m env st
      let r2 = unwrap (f r1.Value) env r1.State
      // 片方が空なら合成しない。**向きは r2.Emit >> r1.Emit のまま**
      // （後で積んだ効果が先頭に来る向きへ戻さないこと）
      let emit =
        match r1.Emit, r2.Emit with
        | ValueNone, e -> e
        | e, ValueNone -> e
        | ValueSome a, ValueSome b -> ValueSome (b >> a)
      { r2 with Emit = emit })

  let internal ask : Sim<Env> = Sim (fun env st -> { Value = env; State = st; Emit = ValueNone })
  let internal get : Sim<BulletState> = Sim (fun _ st -> { Value = st; State = st; Emit = ValueNone })
  let internal put s : Sim<unit> = Sim (fun _ _ -> { Value = (); State = s; Emit = ValueNone })

  let internal emit (e: Effect) : Sim<unit> =
    Sim (fun _ st -> { Value = (); State = st; Emit = ValueSome (fun rest -> e :: rest) })

  /// emit の複数版。1 個ずつ Sim.bind で繋ぐと、繋ぐ数だけ bind が積み重なる
  /// （repeat の周のように手続き的なループで集めた効果を最後にまとめて
  /// 積みたい場面で、要素数ぶんスタックが伸びるのを避けるため）
  let internal emitMany (es: Effect list) : Sim<unit> =
    Sim (fun _ st -> { Value = (); State = st; Emit = ValueSome (fun rest -> es @ rest) })

  /// 走らせて、効果を並びに潰す
  let internal run env st (m: Sim<'a>) =
    let r = unwrap m env st
    let effects = match r.Emit with ValueNone -> [] | ValueSome f -> f []
    r.Value, r.State, effects

type internal SimBuilder() =
  member _.Return x = Sim.ret x
  member _.ReturnFrom (m: Sim<'a>) = m
  member _.Bind (m, f) = Sim.bind f m
  member _.Zero () = Sim.ret ()
  member _.Delay f = f
  member _.Run f = f ()
  member _.Combine (a, b) = Sim.bind (fun () -> b ()) a
  member this.For (xs: seq<'x>, f: 'x -> Sim<unit>) =
    Seq.fold (fun acc x -> Sim.bind (fun () -> f x) acc) (Sim.ret ()) xs
  member this.While (guard, body) =
    if guard () then Sim.bind (fun () -> this.While (guard, body)) (body ()) else Sim.ret ()

[<AutoOpen>]
module SimBuilderInstance =
  let internal sim = SimBuilder()
