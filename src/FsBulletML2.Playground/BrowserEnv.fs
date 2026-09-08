namespace FsBulletML2.Playground

open FsBulletML2.Front

/// Canvas 上の自機（マウス）と乱数。**弾 1 個 につき 1 個 は要らない。**
/// 狙う相手を覚えず、`TryTarget*` は常に false。
///
/// `Rand` の関数値は 1 個。毎コマ作らない（`IFrontEnv` の但し書き）。
///
/// --- 2 つ の軸が動く（v2.0）
///
///     $rank   173 / 176 本 が使う。**0.5 に固定していた**
///     $rand   115 / 176 本 が使う。**種が無く、毎回 別の走りだった**
///
/// 版の頭で測ったら、`$rank` を 0 と 1 に振ると **176 本 中 171 本 で
/// 走りが変わった**（弾数が 3 倍 から 7 倍 になるものが在る）。
///
/// --- どちらも面を建て直さないと効かない
///
/// `Runner.load` は**木を組む段で rank と rand を引く**（`wait` の term を
/// その場で畳む）。だから `Rank` を変えても、走っている面は変わらない ——
/// 変えた側が建て直す（`Main.fs`）。
///
/// **建て直すときは並びも頭へ戻す。** 戻さないと、同じ種でも
/// 「建て直したあと」が別の走りになる（`Restart`）。
[<Sealed>]
type BrowserEnv() =

  let mutable random = SeededRandom(SeededRandom.fromClock ())
  let rand : unit -> float32 = fun () -> random.Next()

  // **既定は真ん中。** 端に寄せると、開いた人が最初に見る絵が
  // 「その弾幕の顔」ではなくなる
  let mutable rank = 0.5f

  // **面が自機の定位置を決める。** 縦と横で違うので、建て直しのたびに
  // 面から取り直す（`SetField`）。取り直さないと、横の面に切り替えても
  // 自機だけ縦の場所に残る
  let mutable field = Stage.portrait
  let mutable motion = PlayerMotion.Follow
  let mutable playerX = field.PlayerX
  let mutable playerY = field.PlayerY

  /// マウスから。**追う以外 では読み捨てる** ——
  /// 止めているのにマウスで動いたら、止めた意味が無い。
  ///
  /// **面の外は定位置へ倒す。** 2 つ のことが同じ枝で直る ——
  /// マウスが面から出たときと、**向きが変わって前の座標が外になったとき**
  /// （縦の面の y = 600 は、横の面（高さ 480）には無い）。
  /// 倒さないと、横の弾幕に切り替えた直後の自機が画面の外に居る
  member _.SetPlayer (x: float32) (y: float32) =
    if motion = PlayerMotion.Follow then
      if x < 0.0f || x > field.Width || y < 0.0f || y > field.Height then
        playerX <- field.PlayerX
        playerY <- field.PlayerY
      else
        playerX <- x
        playerY <- y

  member _.ClearPlayer () =
    if motion = PlayerMotion.Follow then
      playerX <- field.PlayerX
      playerY <- field.PlayerY

  /// 面が変わった。**自機も定位置へ戻す** —— 横の面に切り替えたとき、
  /// 縦の面での場所は面の外かもしれない
  member _.SetField (f: Field) =
    field <- f
    playerX <- f.PlayerX
    playerY <- f.PlayerY

  member _.Field = field

  /// いまの自機。**描く側はこれを見る** ——
  /// 送った座標をそのまま描くと、止めているときと回っているときに
  /// 絵と当たり判定（狙いの基準）が食い違う
  member _.PlayerX = playerX

  member _.PlayerY = playerY

  /// 自機の動かし方
  member _.Motion = motion

  member this.SetMotion (m: PlayerMotion) =
    motion <- m
    if m <> PlayerMotion.Follow then
      playerX <- field.PlayerX
      playerY <- field.PlayerY

  /// 1 コマ 分 動かす。**回すときだけ効く。**
  ///
  /// コマ数を渡すのは、時計で回すと速さを変えたときと飛んだときに
  /// 絵が変わるから（`Player.orbit` の但し書き）
  member _.AdvancePlayer (frame: int) =
    if motion = PlayerMotion.Orbit then
      let struct (x, y) = Player.orbit field frame
      playerX <- x
      playerY <- y

  /// 難易度。**0 から 1。** 外から来る値なので丸める
  member _.SetRank (v: float32) =
    rank <- if v < 0.0f then 0.0f elif v > 1.0f then 1.0f else v

  member _.RankValue = rank

  /// 乱数の種。**Share URL に乗る** —— 同じ種なら同じ走り
  member _.Seed = random.Seed

  member _.SetSeed (n: int) = random <- SeededRandom(SeededRandom.clamp n)

  /// 並びを頭へ戻す。**面を建て直す手前 で呼ぶ**
  member _.RestartRandom() = random.Restart()

  interface IFrontEnv with
    member _.Rand = rand
    member _.Rank = rank
    member _.PlayerX = playerX
    member _.PlayerY = playerY
    member _.TryTargetFrom (_, _, ex, ey) =
      ex <- 0.0f
      ey <- 0.0f
      false
    member _.TrySpawnTargetFrom (_, _, ex, ey) =
      ex <- 0.0f
      ey <- 0.0f
      false
