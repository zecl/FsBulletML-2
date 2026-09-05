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
/// Progress / Bulletml を辿って internal になっているので、それを
/// 運ぶ Sim もそこから見えない外へは出さない
/// **包みを持たない。型の別名そのもの。**
///
/// Sim は「関数を 1 本 くるんだだけ」の型。単一ケースの DU にすると
/// `Sim (fun env st -> ...)` の中のラムダが独立したクロージャになり、
/// **bind の InlineIfLambda がそこまで届かない。**
///
/// **包み自体は確保を生んでいなかった。展開を止めていた。**
/// homing の 1 走行（確保、バイト）で測った 2 x 2:
///
///                      ビルダ非 inline    ビルダ inline
///     struct の DU        6,200,672        5,552,696
///     型の別名            6,199,088        4,372,632
///
/// inline を外すと 2 つ の形はほぼ同じ（差 1,584 B）。inline を入れると
/// 差が 1,180,064 B に開く。**「struct にした時点で包みはヒープを踏まない
/// のだから、別名にしても効かない」と読んでいちど 見送った。** 見ていたのは
/// 包みの確保だけで、包みが展開を妨げることを見ていなかった。
///
/// 中身の関数（クロージャ）の確保は、この手でも消えない。
///
/// **失うもの: 型としての区別。** 別名なので、`Env -> BulletState ->
/// SimResult<'a>` の形をした関数は何でも Sim として通る。単一ケースの DU が
/// 与えていた「別の関数を間違って渡せない」という守りは無くなる。
/// **Sim は internal で、Core の外へは出ない**（BulletState / Effect を
/// 内側に持つため）ので、その範囲で引き合いに見合うと判断した。
type internal Sim<'a> = Env -> BulletState -> SimResult<'a>

module Sim =

  let internal ret x : Sim<'a> = fun _ st -> { Value = x; State = st; Emit = ValueNone }

  /// Emit は「残りの前に自分を足す」向きの関数なので、m の次に f を書いても
  /// 合成は r2.Emit >> r1.Emit になる（先に評価されるのが右）。これを
  /// r1.Emit >> r2.Emit に戻すと、後で積んだ効果が先頭に来る向きへ逆戻りする
  ///
  /// **inline + InlineIfLambda。** f には CE が組んだラムダが渡る。展開すると
  /// その本体が外側の `fun env st -> ...` の中に埋まるので、**bind 1 回 につき
  /// 出ていたクロージャが 2 個 から 1 個 になる。**
  ///
  let inline internal bind ([<InlineIfLambda>] f: 'a -> Sim<'b>) (m: Sim<'a>) : Sim<'b> =
    fun env st ->
      let r1 = m env st
      let r2 = (f r1.Value) env r1.State
      // 片方が空なら合成しない。**向きは r2.Emit >> r1.Emit のまま**
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

  /// emit の複数版。1 個ずつ Sim.bind で繋ぐと、繋ぐ数だけ bind が積み重なる
  /// （repeat の周のように手続き的なループで集めた効果を最後にまとめて
  /// 積みたい場面で、要素数ぶんスタックが伸びるのを避けるため）
  let internal emitMany (es: Effect list) : Sim<unit> =
    fun _ st -> { Value = (); State = st; Emit = ValueSome (fun rest -> es @ rest) }

  /// 走らせて、効果を並びに潰す
  let internal run env st (m: Sim<'a>) =
    let r = m env st
    let effects = match r.Emit with ValueNone -> [] | ValueSome f -> f []
    r.Value, r.State, effects

/// **メソッドを inline にしてある。** Sim.bind を inline にしても、CE が
/// 通るのはこのビルダのメソッドなので、ここが非 inline だとそこで展開が止まる。
///
/// While だけは inline にできない（自分を呼ぶ。付けると FS3890 ——
/// F# は再帰的な inline を持たない）。
type internal SimBuilder() =
  member inline _.Return x = Sim.ret x
  member inline _.ReturnFrom (m: Sim<'a>) = m
  member inline _.Bind (m: Sim<'a>, [<InlineIfLambda>] f: 'a -> Sim<'b>) = Sim.bind f m
  member inline _.Zero () = Sim.ret ()
  // Delay と Run は inline にしない。**FS1118 で落ちる**（Run の展開で
  // 型変数が解決できない）。どちらも CE 1 個 につき 1 回 しか通らないので、
  // 効きは薄い。**Bind の展開は Delay が受け取るラムダの中で起きるので、
  // ここが非 inline でも止まらない**
  member _.Delay (f: unit -> Sim<'a>) = f
  member _.Run (f: unit -> Sim<'a>) = f ()
  member inline _.Combine (a: Sim<unit>, [<InlineIfLambda>] b: unit -> Sim<'a>) =
    Sim.bind (fun () -> b ()) a
  member inline _.For (xs: seq<'x>, [<InlineIfLambda>] f: 'x -> Sim<unit>) =
    Seq.fold (fun acc x -> Sim.bind (fun () -> f x) acc) (Sim.ret ()) xs
  member this.While (guard, body) =
    if guard () then Sim.bind (fun () -> this.While (guard, body)) (body ()) else Sim.ret ()

/// **テストから CE を書くための入口。SimBuilder と inline 属性だけが違う。**
///
/// テストのアセンブリからは
/// inline なメンバを展開できない。ここで 1 枚 包み、展開を Core の中で済ませる。
///
/// **メンバは 1 つ も独自の中身を持たず、全部 SimBuilder へ委譲する。**
/// だから意味論は本番と同じ道（Sim.bind の本体は 1 つ）を通る。inline は
/// F# の保証として意味論を変えないので、ここでテストしたことは本番にも効く。
///
/// **本番の道（Step.fs の sim { }）は inline 版を通り、その軌跡は
/// 橋 227 本（Equivalence）が押さえている。**
type internal SimBuilderForTests() =
  let b = SimBuilder()
  member _.Return x = b.Return x
  member _.ReturnFrom (m: Sim<'a>) = b.ReturnFrom m
  member _.Bind (m: Sim<'a>, f: 'a -> Sim<'b>) = b.Bind (m, f)
  member _.Zero () = b.Zero ()
  member _.Delay (f: unit -> Sim<'a>) = b.Delay f
  member _.Run (f: unit -> Sim<'a>) = b.Run f
  member _.Combine (a: Sim<unit>, f: unit -> Sim<'a>) = b.Combine (a, f)
  member _.For (xs: seq<'x>, f: 'x -> Sim<unit>) = b.For (xs, f)
  member _.While (guard, body) = b.While (guard, body)

[<AutoOpen>]
module SimBuilderInstance =
  let internal sim = SimBuilder()

  /// テストが使う入口。**Core の中では sim を使うこと**（SimBuilderForTests の但し書き）
  let internal simForTests = SimBuilderForTests()
