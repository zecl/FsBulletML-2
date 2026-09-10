namespace FsBulletML2.Playground

open System.Text.Json
open System.Threading.Tasks
open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open FsBulletML2
open FsBulletML2.Bullets.Dsl
// `SourceKind`。**ブラウザ側（Fable）が同じ 1 本 を引く** ——
// 前は こちらが `match kind with | "xml"` で受け、あちらが
// `SourceKind.Xml.Id` で作っていて、片方 だけ変えても落ちなかった
open FsBulletML2.LanguageService

/// 起動時に載せる弾幕。**同梱カタログの CE が正本。**
///
/// 欄に出す XML は `BulletmlWriter.toIndentedXml` で焼く —— html に直書きすると、
/// 弾幕を差し替えたとき字だけが古びる（走るのは CE、見えるのは古い XML）。
///
/// **一覧の番号で指さない。** 並びが動くと黙って別の弾幕になる。
/// 番号のほうを値から引く（`InitialIndex`）。
///
/// 起動時に XML を読まないのは前と同じ理由 —— WASM で `XmlReader` に
/// 入る経路をひとつ減らす。CE なら木がもう建っている。
module private Initial =
  let pattern = EnemyBullet.Sdmkun.EspRade.round_123_boss_izuna_hakkyou

/// JSInvokable の実体。アセンブリ名経由だと WASM 起動直後に見つからない。
type PlaygroundHost() =

  let env = BrowserEnv()
  /// 2 つ 目 の面の env。**乱数を分ける。**
  ///
  /// 1 個 を共有すると、2 つ の面が並びを取り合って**どちらも決定的でなくなる**
  /// —— 片方 の弾数が変われば、もう片方 が引く値がずれる。
  /// 「同じ種なら同じ走り」の契約は 1 面 で見たときと 2 面 で見たときの
  /// あいだにも要る（並べたら絵が変わる、では比べる道具にならない）。
  ///
  /// 難度・種・自機の動かし方は同じ値を配る（比べるのは弾幕であって走らせ方ではない）
  let env2 = BrowserEnv()
  let mutable current = Initial.pattern.Bulletml
  /// 2 つ 目 の弾幕。**無ければ 1 面。**
  let mutable second : Bulletml option = None

  /// 面を建てる。**乱数の並びを頭へ戻してから。**
  ///
  /// 戻さないと、同じ種でも「建て直したあと」が別の走りになる ——
  /// Reset も Apply も飛ぶのも、すべてここを通る。
  ///
  /// **`Runner.load` は木を組む段で rank と rand を引く**（`wait` の term を
  /// その場で畳む）ので、rank や種を変えたら建て直すしかない
  /// **面の形も建て直しで決まる。** 向きは弾幕が持っているので、
  /// 弾幕が変われば面も変わりうる —— 自機の定位置ごと env へ渡す
  let build (bulletml: Bulletml) =
    env.RestartRandom()
    let pf = Playfield.Create env bulletml
    env.SetField pf.Field
    pf

  /// 2 つ 目 の面。**同じ手順を別の env で。** 1 本 にまとめられないのは
  /// `env` が引数でなく閉包に居るからで、そこは 2 つ 目 の面の意味そのもの
  let buildSecond (bulletml: Bulletml) =
    env2.RestartRandom()
    let pf = Playfield.Create env2 bulletml
    env2.SetField pf.Field
    pf

  let mutable field = build current
  let mutable field2 : Playfield option = None

  /// 走らせ方の軸が動いたときの建て直し。**両方 の面を、同じところで。**
  ///
  /// 片方 だけ建て直すと、並べている 2 つ が別のコマを指す ——
  /// 「同じ時刻の 2 つ」でなくなった時点で、比べる道具ではなくなる
  let rebuildAll () =
    field <- build current
    field2 <- second |> Option.map buildSecond
  // **開いた時点で走っている。** Play を押すまで止まっていると、
  // 弾幕を見に来た人が最初に見るのが静止画になる。止めたい人は Pause を
  // 押せばよく、そちらは 1 手 で戻せる。
  //
  // rAF は `onReady` で回り始めるので、ここが true でも起こす順は変わらない
  let mutable playing = true
  // 進め方は `Pacing` が持つ。**ここで判断しない** ——
  // 判断をここへ書くと、Bolero を参照するこのファイルの中にしか無くなって
  // .NET で当てられなくなる
  let mutable pacing = Pacing.normal
  // 飛び先。**進め方（Pacing）と別に持つ** —— 飛んでいるあいだは速さを見ない
  // （速さは「見ながら進める」ための道具で、飛ぶのは着くまでの手段）
  let mutable seek = Seek.idle
  // 1 フレームの予算を測る。**1 個 だけ作って持ち回る**
  let watch = System.Diagnostics.Stopwatch()
  let elapsed = fun () -> watch.Elapsed.TotalMilliseconds
  // **1 個 だけ作って持ち回る。** 毎コマ `field.Tick` を関数値にすると、
  // 1 フレーム につき 1 個 の閉包がヒープに乗る。
  // `field` は Apply で差し替わるが、その都度 読み直すのでこれで足りる。
  //
  // **自機を動かすのもここ 1 か所。** 進める道は 3 本 ある（走る・1 コマ
  // 送り・飛ぶ）ので、呼ぶ側に置くと必ずどれかで動かし忘れる
  let tick =
    fun () ->
      env.AdvancePlayer field.Frame
      field.Tick()
      // **2 つ 目 も同じコマで進める。** 別々に進めると、並べている意味が
      // 「同じ時刻の 2 つ」ではなくなる
      match field2 with
      | Some f2 ->
        env2.AdvancePlayer f2.Frame
        f2.Tick()
      | None -> ()
  // `[n; ptr; frame; playerX; playerY; width; height]` に、
  // 2 つ 目 の `[n2; ptr2; width2; height2]` と、追っている弾の
  // `[pick; stops; depth; serial; paths; order; from]` を足した 18 数。
  // **2 つ 目 が無ければ `n2` は -1**（0 は「弾が 1 つ も無い面」で別の意味）
  let ret = Array.zeroCreate<float> 18
  /// プルダウンに出す並び。**同梱のあとに公式配布のサンプルを繋ぐ**（v2.4.1）。
  ///
  /// 番号で引く口（`SelectPattern` / `InitialIndex`）が在るので、
  /// **公式は後ろに足す** —— 前や間に入れると、同梱の番号が全部 動く。
  /// 境目は `OfficialFrom` が返す
  let catalog = lazy (List.append All.bullets All.official |> List.toArray)

  /// `[n; ptr; frame; playerX; playerY; width; height]`。
  /// Apply で配列が差し替わるので ptr は毎コマ返す。
  ///
  /// **自機と面の大きさも毎コマ返す。** どちらも「建て直したときに変わる」
  /// もので、JS から引きに行く形にすると建て直す道の数だけ呼び忘れる口が
  /// できる（走る・1 コマ 送り・飛ぶ・Apply・Reset・選び直し・難度・種・
  /// リンクを開く）。返してしまえば呼び忘れが起きない。
  ///
  /// 自機を返すのは、**送った座標がそのまま自機とは限らない**から ——
  /// 止めているときと回っているときは、こちらが決める。
  ///
  /// **飛んでいるあいだは Pacing を通さない。** 速さは「見ながら進める」ための
  /// 道具で、飛ぶのは着くまでの手段 —— 混ぜると 1/4 速で飛べなくなる。
  [<JSInvokable>]
  member _.StepFrame(_now: float, playerX: float, playerY: float) : float[] =
    env.SetPlayer (float32 playerX) (float32 playerY)
    // **自機は 2 つ の面で同じ。** 比べるのは弾幕であって走らせ方ではない
    env2.SetPlayer (float32 playerX) (float32 playerY)
    if Seek.isRunning seek then
      // 戻るには建て直すしかない（面は逆再生できない）。
      // **建ててから差し替える** —— ほかの差し替えと同じ理由
      if Seek.needsRestart field.Frame seek then rebuildAll ()
      watch.Restart()
      let struct (_, next) = Seek.step field.Frame tick elapsed seek
      seek <- next
    elif playing then pacing <- Pacing.step tick pacing
    ret.[0] <- float (field.Pack())
    ret.[1] <- field.PackedPtr
    ret.[2] <- float field.Frame
    ret.[3] <- float env.PlayerX
    ret.[4] <- float env.PlayerY
    ret.[5] <- float field.Field.Width
    ret.[6] <- float field.Field.Height
    match field2 with
    | Some f2 ->
      ret.[7] <- float (f2.Pack())
      ret.[8] <- f2.PackedPtr
      ret.[9] <- float f2.Field.Width
      ret.[10] <- float f2.Field.Height
    | None ->
      // **-1 は「2 つ 目 が無い」。** 0 は「弾が 1 つ も無い面」で別の意味
      ret.[7] <- -1.0
      ret.[8] <- 0.0
      ret.[9] <- 0.0
      ret.[10] <- 0.0
    // 追っている弾（v3.1 の段 3）。**添字は毎コマ 引き直す** ——
    // 消しで詰めたコマに黙って別の弾を指さないため（`Playfield.PickedIndex`）。
    //
    // 数を 4 つ 返すのは、**「戻れた」と「正しい所へ戻れた」が別**だから ——
    // `stops` は 0 か 2 以上（1 は出ない）、`depth` は鎖の段数、
    // `serial` は再開点そのもの、`paths` は道の本数。
    // 名前だけでは、どれが効いたか読めない。
    //
    // **`paths` が 2 以上 のコマでは、出しているのは 1 本 目 だけ。**
    // 黙って落とさないために数で出す（同梱では wait 全体 の 1.31%）
    ret.[11] <- float field.PickedIndex
    ret.[12] <- float field.Focus.Stops
    ret.[13] <- float field.Focus.Depth
    ret.[14] <- float field.Focus.Serial
    ret.[15] <- float field.Focus.Paths
    // 光らせる先（v3.1 の段 4）。**読んだ木を書いてある順に歩いた添字。**
    //
    // **字の位置は渡らない。** 位置は木に無く（`XmlNode` は 名前・属性・子 だけ）、
    // 字はあちら（JS）にしか無いので、あいだを渡るのは順番だけ。
    // 決まらないときは -1（再開点が無いときと、並びに 2 度 出るとき）
    ret.[16] <- float field.Focus.OrderIndex
    // 追っている弾を撃った場所（v3.2）。**こちらも書いてある順の添字。**
    //
    // 再開点と違って**コマごとに動かない** —— 撃たれた時点で決まる。
    // だから呼ぶ側は、これが変わったときだけ字へ飛ぶ
    ret.[17] <- float field.PickedFrom
    ret

  /// 面の弾を押したときの受け口（v3.1 の段 3）。**`Pack` の並びの添字。**
  ///
  /// **いちばん近い弾を探すのは呼ぶ側。** 座標は WASM ヒープの `float32[]` を
  /// JS がそのまま読んでいる（`Playfield.Pack`）ので、あちらには全部 の座標が
  /// もう在る —— こちらへ座標を送り返すと、同じ配列を 2 度 運ぶことになる。
  ///
  /// 範囲の外なら追うのをやめる（`Playfield.Pick`）
  [<JSInvokable>]
  member _.PickBullet(index: int) = field.Pick index

  /// 追うのをやめる。**面の何も無いところを押したとき**
  [<JSInvokable>]
  member _.UnpickBullet() = field.Unpick()

  /// 木のノードになる要素名（v3.1 の段 4）。**起動時に 1 回 だけ。**
  ///
  /// 字の側は札を数えて k 番目 を取るが、**そのとき落とすものが要る** ——
  /// `times` / `direction` などは値になって親へ畳まれ、木のノードにならない。
  /// **落とす側の表を持たない** —— 増えたとき黙って添字がずれるので、
  /// 引くのは逆側（ノードになる腕）にする。正本は `Core/DTD.fs` の腕。
  ///
  /// 空で返ってきたら黙って進まない（`Vocabulary` と同じ理由）
  [<JSInvokable>]
  member _.NodeElementNames() : string[] = NodeOrder.names

  /// 追っている弾の再開点の要素名。**`serial` が変わったときだけ引く口** ——
  /// 毎コマ 引くと、選んだだけで境界越しに文字列が 60 個/秒 できる。
  /// 名前の表は持たない（腕の名前の頭を小文字に。正本は `Core/DTD.fs`）
  [<JSInvokable>]
  member _.ResumeName() : string = field.Focus.Name

  /// どの撃つ腕が何発 撃ったか、多い順に（v3.3 の段 1）。
  ///
  /// 並びは `[添字; 数; 添字; 数; ...]` で、**先頭に 総数 と 腕の数**を置く ——
  /// 上位が全体の何割 かは、その 2 つ が無いと出せない。
  ///
  ///     ret.[0]   数えた総数
  ///     ret.[1]   数えた腕の数
  ///     ret.[2k+2] / ret.[2k+3]   k 番目 の (書いてある順の添字, 撃った数)
  ///
  /// **毎コマ 呼ばない口。** 並べ替えが腕の数ぶん要るので、呼ぶ側が間引く
  /// （字の上の印は 0.5 秒 ごとで足りる）。
  ///
  /// 上位いくつ で足りるかは測ってある —— 同梱 176 本 で**上位 3 つ が 93%**
  [<JSInvokable>]
  member _.TallyTop(n: int) : float[] =
    let top = field.Focus.TallyTop n
    let out = Array.zeroCreate<float> (2 + top.Length * 2)
    out.[0] <- float field.Focus.TallyTotal
    out.[1] <- float field.Focus.TallyCount
    for k in 0 .. top.Length - 1 do
      let struct (idx, count) = top.[k]
      out.[2 + k * 2] <- float idx
      out.[3 + k * 2] <- float count
    out

  /// **飛ぶのをやめる。** 飛んでいる最中の Play は「もう待たない」なので、
  /// 目的のコマを捨てていまの場所から走らせる
  [<JSInvokable>]
  member _.Play() =
    seek <- Seek.idle
    playing <- true

  [<JSInvokable>]
  member _.Pause() =
    seek <- Seek.idle
    playing <- false

  /// 1 コマ だけ進める。**押した時点で止まる** ——
  /// 走っているまま 1 コマ 足しても、次のコマで流れてしまって見えない
  [<JSInvokable>]
  member _.StepOnce() =
    seek <- Seek.idle
    playing <- false
    tick ()

  /// そのコマへ飛ぶ。**着くまで何フレームか かかる**（`Seek` の但し書き）。
  ///
  /// 着いたら止まる —— 1 コマ送りと同じで、見たいコマで止まっていないと
  /// 流れてしまって見えない
  [<JSInvokable>]
  member _.SeekTo(n: int) =
    playing <- false
    seek <- Seek.toFrame n

  /// 正なら倍速（1 フレームに n 回）、負ならスロー（-n フレームに 1 回）。
  ///
  /// **Apply / Reset / プルダウンでは戻さない。** 速さは見る側の都合で、
  /// 載っている弾幕の一部ではない（拡大率と同じ扱い）
  [<JSInvokable>]
  member _.SetRate(n: int) = pacing <- Pacing.withRate n

  /// 難易度。**0 から 1。**
  ///
  /// 版の頭で測った —— 0 と 1 に振ると **176 本 中 171 本 で走りが変わる**
  /// （弾数が 3 倍 から 7 倍 になるものが在る）。v1.9 まで 0.5 に固定していた。
  ///
  /// **建て直す。** `Runner.load` は木を組む段で rank を引く（`wait` の term を
  /// その場で畳む）ので、走っている面には効かない。
  ///
  /// **速さや配色と違って、これは弾幕の走りを変える。** 見る側の都合ではなく
  /// 「どの難度の絵を見ているか」なので、Reset でも Apply でも保つ
  [<JSInvokable>]
  member _.SetRank(v: float) =
    env.SetRank(float32 v)
    env2.SetRank(float32 v)
    seek <- Seek.idle
    rebuildAll ()

  /// 乱数の種。**同じ種なら同じ走り。**
  ///
  /// v1.9 まで `System.Random()` に種が無く、毎回 別の走りだった ——
  /// 「いま見た絵をもう一度」が出せない。
  ///
  /// **並びを決めるのは `SeededRandom`**（`System.Random` ではない）——
  /// 種は Share URL に乗るので、走らせる runtime が変わっても
  /// 同じ絵でなければならない。
  [<JSInvokable>]
  member _.SetSeed(n: int) =
    env.SetSeed n
    env2.SetSeed n
    seek <- Seek.idle
    rebuildAll ()

  /// いまの難度と種。**Share が読む。** `[rank; seed]`
  [<JSInvokable>]
  member _.ViewAxes() : float[] = [| float env.RankValue; float env.Seed |]

  /// 自機の動かし方。**面が変わっても保つ** ——
  /// 難度や種と違って走りの一部ではないが、見る人の手の置き方なので、
  /// 弾幕を替えるたびに追う側へ戻されると邪魔になる
  [<JSInvokable>]
  member _.SetPlayerMotion(n: int) =
    let m = Player.ofInt n
    env.SetMotion m
    env2.SetMotion m

  /// 面の大きさ。**JS が canvas をこの大きさにする。**
  ///
  /// 向きは弾幕が持っているので、建て直すたびに変わりうる ——
  /// 呼ぶのは建て直した側（Apply / Reset / 選び直し / 難度 / 種）。
  ///
  /// `[width; height; playerX; playerY; enemyX; enemyY]`。置き場所も返すのは、
  /// マウスが面の外に出たときの戻り先と、弾が 1 つ も無いときに描く敵が
  /// 要るため。**JS 側に数を書き写さない**
  [<JSInvokable>]
  member _.FieldSize() : float[] =
    let f = field.Field
    // 2 つ 目 の敵も返す。**無ければ -1**（面の中に -1 は無い）
    let struct (ex2, ey2) =
      match field2 with
      | Some f2 -> struct (float f2.Field.EnemyX, float f2.Field.EnemyY)
      | None -> struct (-1.0, -1.0)
    [| float f.Width; float f.Height; float f.PlayerX; float f.PlayerY
       float f.EnemyX; float f.EnemyY; ex2; ey2 |]

  /// **飛んでいる最中でも頭へ戻す。** 面を建て直すと `Frame` も 0 に戻るので、
  /// 飛び先を持ったままだとそこへ向かって走り直してしまう
  [<JSInvokable>]
  member _.Reset() =
    seek <- Seek.idle
    rebuildAll ()

  /// 起動時に欄へ出す XML。**html に直書きしない。**
  ///
  /// **定数を畳まない側で焼く（v1.4）。** `Parser` の `ToIndentedXmlString` は
  /// `foldConstants` を通すので `8` が `8.0000000000` になる ——
  /// 表記を切り替えると、その字が sxml にも fsb にも CE にも伝播する。
  /// **人が書いた式のまま見せる**ほうが正しい。
  [<JSInvokable>]
  member _.InitialSource() : string = BulletmlWriter.toIndentedXml 4 current

  /// 補完の語彙。**起動時に 1 回 だけ。** 正本は Core の DTD.fs で、
  /// ここは reflection で読んだものを JSON にして渡すだけ。
  /// 毎キー呼ばない —— 引くのは Fable 側でやる
  [<JSInvokable>]
  member _.Vocabulary() : string = Vocabulary.toJson ()

  /// 起動時の弾幕が一覧の何番目か。**無ければ -1。**
  ///
  /// 中身の `Bulletml` で引く —— `BulletmlInfo` は struct なので、
  /// 一覧に入るとき写される。参照が同じなのは中の木のほう。
  ///
  /// -1 は「走っている弾幕がプルダウンに出ていない」なので、
  /// 呼ぶ側は黙って空にせず理由を出す
  [<JSInvokable>]
  member _.InitialIndex() : int =
    catalog.Value
    |> Array.tryFindIndex (fun i -> System.Object.ReferenceEquals(i.Bulletml, Initial.pattern.Bulletml))
    |> Option.defaultValue -1

  /// 同梱 CE の名前。初回だけ木を組む。
  ///
  /// **同梱（176 本）のあとに公式配布のサンプル（17 本）が続く。**
  /// 境目は `OfficialFrom` で、呼ぶ側がそこで並びを 2 つ に割って出す
  [<JSInvokable>]
  member _.ListPatterns() : string[] =
    catalog.Value
    |> Array.mapi (fun i info ->
         if System.String.IsNullOrEmpty info.Name then sprintf "#%d" i else info.Name)

  /// 一覧の何番目から公式配布のサンプルか（v2.4.1）。
  ///
  /// **`All.bullets` と `All.official` は別の集合。** あちらは白い弾幕くん
  /// 由来で、測った数（`$rank` を使う 173 本 / 狙いを使う 103 本 /
  /// 横画面 9 本 …）が全部 その集合に紐づいている。混ぜないまま
  /// **プルダウンだけ 1 本 に繋いで、境目をここで教える。**
  ///
  /// **数を JS 側に書かない** —— 本数が変わったときに片方 だけ古びる
  [<JSInvokable>]
  member _.OfficialFrom() : int = List.length All.bullets

  /// 一覧の番号で差し替える。**どの表記で欄に出すかを受け取る。**
  /// 成功ならその表記の本文。失敗は `ERROR:` で始まる。
  ///
  /// v1.5 まで XML を返し、呼ぶ側が表記も XML に戻していた。
  /// **選ぶのは弾幕であって書き方ではない** —— sxml で読んでいる人が
  /// 別の弾幕を見たいだけで XML に戻されるのは、選んでいないものが動く。
  ///
  /// **読んでから書き直すのではない。** 木はもう在るので、その表記の
  /// 書き手へ直に渡す（`Transcode` は本文の字から始まるので別の口）。
  ///
  /// 書けない表記が在れば `ERROR:`。**そのとき弾幕も差し替えない** ——
  /// 欄と走っているものが食い違うほうが読み解けない
  /// （同梱 176 本 が 4 表記 とも書けることは `Parser.Tests/Transcode.fs`）。
  [<JSInvokable>]
  member _.SelectPattern(index: int, kind: string) : string =
    try
      let items = catalog.Value
      if index < 0 || index >= items.Length then "ERROR:範囲外"
      else
        match SourceKind.tryParse kind |> Option.bind SourceWriter.tryFind with
        // 知らない字。`ApplySource` と同じ文面
        | None -> "ERROR:未対応: " + kind
        | Some writer ->
          let info = items.[index]
          // **字を先に作る。** 書けなかったときに走っているものを触らない
          match writer.Write info.Bulletml with
          | Result.Error why -> "ERROR:" + why
          | Result.Ok text ->
            // 建ててから差し替える（`ApplySource` と同じ理由。落ちたあとの
            // `Reset` が `current` から建て直すので、進めてはいけない）
            let next = build info.Bulletml
            current <- info.Bulletml
            field <- next
            // 2 つ 目 も頭から。**同じコマで並べる**
            field2 <- second |> Option.map buildSecond
            // 別の弾幕に飛び先は引き継がない（`Reset` と同じ理由）
            seek <- Seek.idle
            text
    with ex -> "ERROR:" + ex.Message

  /// いま走っている弾幕を、指定の表記で焼く。**「編集前」の正本はこちら。**
  ///
  /// 並べて見る側（v2.1）が、左に置く本文としてこれを引く。
  /// **Fable 側に写しを持たない** —— 弾幕が差し替わる道は 8 本 以上 あり
  /// （Apply / Reset / 選び直し / Open / リンク / 難度 / 種 / 起動）、
  /// 写しを持てばそのどれかで更新し忘れて、**古い本文と比べていることに
  /// 誰も気づけない**（差分が出るのが正常な道具なので、嘘の差分が嘘に見えない）。
  ///
  /// 失敗は `SelectPattern` と同じく `ERROR:` で始まる
  [<JSInvokable>]
  member _.AppliedSource(kind: string) : string =
    try
      match SourceKind.tryParse kind |> Option.bind SourceWriter.tryFind with
      | None -> "ERROR:未対応: " + kind
      | Some writer ->
        match writer.Write current with
        | Result.Error why -> "ERROR:" + why
        | Result.Ok text -> text
    with ex -> "ERROR:" + ex.Message

  /// 2 つ 目 の面に、同梱の一覧から弾幕を載せる（v2.1）。
  ///
  /// **1 つ 目 も頭から建て直す。** 並べた 2 つ が別のコマを指していたら、
  /// それは「同じ時刻の 2 つ」ではないので比べられない。
  ///
  /// **env を分けてある**（`env2`）—— 乱数の並びを共有すると、
  /// 片方 の弾数が変わるたびにもう片方 の引く値がずれて、
  /// **1 面 で見たときと 2 面 で見たときの絵が違う**ことになる。
  ///
  /// 失敗は `SelectPattern` と同じく `ERROR:` で始まる。空なら成功
  [<JSInvokable>]
  member _.SetSecond(index: int) : string =
    try
      let items = catalog.Value
      if index < 0 || index >= items.Length then "ERROR:範囲外"
      else
        let info = items.[index]
        second <- Some info.Bulletml
        seek <- Seek.idle
        rebuildAll ()
        ""
    with ex -> "ERROR:" + ex.Message

  /// 2 つ 目 の面をやめる。**1 つ 目 も頭から**（`SetSecond` と同じ理由）
  [<JSInvokable>]
  member _.ClearSecond() =
    second <- None
    seek <- Seek.idle
    rebuildAll ()

  /// 右側の本文を読んで弾幕を差し替える。
  ///
  /// **どの表記かを受け取る。** v0.9 で読めるのは xml と sxml。
  /// テキストだけ受け取る形にすると、呼ぶ側にも XML が焼き込まれて、
  /// 次の表記で両方 直すことになる。
  ///
  /// **表記ごとの腕はここに無い。** `SourceReader` が束ねている ——
  /// fsb を足すのはあちらに 1 行、`Diagnosis` に読む筋 1 本。
  ///
  /// **戻りは JSON。空文字を成功の印にしない** ——
  /// 「読めない理由が空文字」と見分けられない。
  ///
  ///     {"ok":true}
  ///     {"ok":false,"message":"…","marks":[{"line":14,"column":11,"endColumn":15,"message":"…"}]}
  ///
  /// `message` は帯に出す代表で、`marks` が波線。**`marks` が空なら
  /// 位置が無い層**（呼ぶ側は波線を引かない）。`endColumn` が 0 なら行末まで。
  /// 分け方は `Diagnosis`（`Parser.Tests` が同じ道を通って当てている）。
  [<JSInvokable>]
  member _.ApplySource(kind: string, text: string) : string =
    let jstr (s: string) = JsonSerializer.Serialize s
    /// 帯に出す代表。**何本 引いたかは帯にしか出ない** ——
    /// 波線は 1 本 ずつ別のところに在るので、まとめて数えられない
    let banner (fs: Failure list) =
      match fs with
      | [] -> ""
      | [ f ] -> f.Message
      | f :: rest -> sprintf "%s（ほか %d 件）" f.Message rest.Length
    let failed (fs: Failure list) =
      let marks =
        fs
        |> List.filter (fun f -> f.Line > 0)
        |> List.map (fun f ->
            sprintf
              "{\"line\":%d,\"column\":%d,\"endColumn\":%d,\"message\":%s}"
              f.Line f.Column f.EndColumn (jstr f.Message))
        |> String.concat ","
      sprintf "{\"ok\":false,\"message\":%s,\"marks\":[%s]}" (jstr (banner fs)) marks
    // **建ててから差し替える。** 木は読めるが組めない層が在るので、
    // 先に `current` を書くと、落ちたあとの Reset がその弾幕で作り直して
    // また落ちる（`Reset` は `current` から建てる）
    let put bulletml =
      let next = build bulletml
      current <- bulletml
      field <- next
      // 2 つ 目 も頭から。**同じコマで並べる**（`SetSecond` の但し書き）
      field2 <- second |> Option.map buildSecond
      // 別の本文に飛び先は引き継がない（`Reset` と同じ理由）
      seek <- Seek.idle
    // **字を直に書かない。** 送ってくるのは Fable 側の `SourceKind.Id` で、
    // どちらも `LanguageService` の 1 本 を引く。
    //
    // **表記ごとの腕をここに書かない（v0.9）。** 読む口は `SourceReader` に
    // 束ねてある —— 書くと、表記を足すたびにここも直すことになる
    match SourceKind.tryParse kind |> Option.bind SourceReader.tryFind with
    // **組み合わせは `References.explain` 1 本。** 試験も同じそれを通る
    | Some reader ->
      match References.explain reader.Tags (reader.Apply put text) text with
      | [] -> "{\"ok\":true}"
      | fs -> failed fs
    // 知らない字と、まだ読めない表記を分けない。**どちらも人には同じ** ——
    // 分けると「口は在るが読めない」を人に見せることになる
    | None -> failed [ Diagnosis.plain ("未対応: " + kind) ]

  /// **本文を別の表記に書き直す。** 弾幕は差し替えない ——
  /// 読んで書くだけで、走っているものは触らない。
  ///
  /// v1.4 まで「表記を変えても本文はそのまま」だった。**決めたのではなく、
  /// XML 以外 を書く口が repo に無かった。**
  ///
  /// **いまの本文を変換する**（プルダウンの弾幕を読み直すのではない）——
  /// 人が足した字が消えないし、Open したファイルや編集後でも効く。
  /// 読めない本文のときは触らない（呼ぶ側が `ok:false` を見て何もしない）。
  ///
  ///     {"ok":true,"text":"…"}
  ///     {"ok":false,"message":"…"}
  [<JSInvokable>]
  member _.Transcode(fromKind: string, toKind: string, text: string) : string =
    let jstr (s: string) = JsonSerializer.Serialize s
    let ng (message: string) = sprintf "{\"ok\":false,\"message\":%s}" (jstr message)
    match SourceKind.tryParse fromKind |> Option.bind SourceReader.tryFind,
          SourceKind.tryParse toKind |> Option.bind SourceWriter.tryFind with
    | Some reader, Some writer ->
      // **載せない。** `ApplySource` と違って、ここは字を作るだけ
      let mutable got = None
      match reader.Apply (fun b -> got <- Some b) text with
      | Some failure -> ng failure.Message
      | None ->
        match got with
        | None -> ng "読めたが弾幕が取れなかった"
        | Some bulletml ->
          match writer.Write bulletml with
          | Result.Ok written -> sprintf "{\"ok\":true,\"text\":%s}" (jstr written)
          | Result.Error why -> ng why
    | None, _ -> ng ("未対応: " + fromKind)
    | _, None -> ng ("未対応: " + toKind)

/// 空の根。描画のあとで host を JS に渡す。
type MyApp() =
  inherit Component()

  let host = PlaygroundHost()
  let mutable started = false
  let mutable dotNetRef : DotNetObjectReference<PlaygroundHost> = null

  [<Inject>]
  member val JS : IJSRuntime = Unchecked.defaultof<_> with get, set

  override _.Render() = text ""

  override this.OnAfterRenderAsync first =
    if first && not started then
      started <- true
      dotNetRef <- DotNetObjectReference.Create(host)
      this.JS.InvokeVoidAsync("playground.onReady", dotNetRef).AsTask()
    else
      Task.CompletedTask
