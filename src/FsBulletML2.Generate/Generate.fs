/// 仕様 から BulletML を組む。骨 を N 種類 持って 選ばせず、軸 ごと の値 の掛け算 で組む
module FsBulletML2.Generate.Generate

open FsBulletML2
open FsBulletML2.Dsl
open FsBulletML2.Generate
open FsBulletML2.Generate.Exprs
open FsBulletML2.Generate.Bound

/// `Depth` の段数 だけ `repeat` を入れ子 にする
let rec private nestRepeat (n: int) (times: string) (inner: Action list) : Action list =
    if n <= 0 then
        inner
    else
        [ repeat times { yield! nestRepeat (n - 1) times inner } ]

/// 撃つ 向き。`direction` の型 4 つ（`DTD.fs`）と 同じ 形
/// CE の操作 は 単独 では 値 に ならない ので、向き の規則 を ここ に落として 撃つ 形 は 1 か所 で書く
type private Dir =
    | Seq of string
    | Abs of string
    | At of string

/// 腕 の 1 発 の向き。腕 は どれ も 頭 から の `sequence`
/// `absolute` / `relative` / `aim` は 前 の弾 を見ない ので、腕 に置く と n 発 が 1 つ の角 に重なる
let private armDir (d: PatternSpec) : Dir =
    match d.Kind with
    | Spiral -> Seq(armStep d)
    | Radial -> Seq(ringStep d)
    | Aimed -> Seq(aimStep d)
    | Spread -> Seq(spreadStep d)
    // 幕 は 帯 を等間隔 に掃く。頭 が毎波 左端 へ戻る ので回り出さない
    | Curtain -> Seq(curtainStep d)

/// 頭 の 1 発 の向き。`Spread` と `Radial` は 名指し（前 / 後ろ / 横）が 最優先、次 が 狙い、最後 が 型 の既定
/// 「後方 へ」と頼まれた のに 自機 を狙う と、頼み の逆 を向く
let private headDir (d: PatternSpec) : Dir =
    match d.Kind with
    // 毎波 少しずつ 回す。これ が渦 の正体 —— 名指し より 先 に見る
    | Spiral -> Seq(spinExpr d)
    | Aimed -> At(fanHead "0" (aimSpan d) d)
    // 幕 の頭 は帯 の左端。ここ が毎波 の起点 になる
    | Curtain -> Abs(curtainHead d)
    | Spread ->
        if facingGiven d then
            Abs(fanHead (facingExpr d) (spreadSpan d) d)
        elif d.Aiming then
            At(fanHead "0" (spreadSpan d) d)
        // 撃つ側 の向き（`relative`）を中心 に すると、敵 は 0 度 なので 真上 へ開く
        else
            Abs(fanHead "180" (spreadSpan d) d)
    | Radial ->
        if facingGiven d then
            Abs(facingExpr d)
        elif d.Aiming then
            At "0"
        // 回らない。毎波 同じ 向き から 撒く
        else
            Abs "0"

/// 腕 を撃つ 最内側
let private arms (d: PatternSpec) : Action list =
    // 腕 は 速度 を持たない。`sequence` の 0 で 頭 の速さ を引き継ぐ
    let one () =
        match armDir d with
        | Seq e ->
            fire {
                sequence e
                speedSeq "0"
                refBullet "core" []
            }
        | Abs e ->
            fire {
                absolute e
                speedSeq "0"
                refBullet "core" []
            }
        | At e ->
            fire {
                aim e
                speedSeq "0"
                refBullet "core" []
            }

    // 頭 の 1 発 が速度 の起点。`sequence` は差分 なので、無い と `Speed` 軸 が効かない
    let head =
        match headDir d with
        | Seq e ->
            fire {
                sequence e
                speed (speedExpr d)
                refBullet "core" []
            }
        | Abs e ->
            fire {
                absolute e
                speed (speedExpr d)
                refBullet "core" []
            }
        | At e ->
            fire {
                aim e
                speed (speedExpr d)
                refBullet "core" []
            }

    // 幕 は `Parametrized` を通さない。`arm` の 0 度 と 180 度 の対称 で 帯 が上下 に割れる
    if d.Parametrized && d.Kind <> Curtain then
        [ head; actionRef "arm" [ "0"; "1" ]; actionRef "arm" [ "180"; "-1" ] ]
    elif headIsArm d then
        if d.Ways = 1 then
            [ head ]
        else
            [ head; repeat (restArms d) { one () } ]
    else
        [ head; repeat (armsExpr d) { one () } ]

/// 段 ごと の弾（`core` -> `core1` -> `core2` -> `core3`）
/// 自己再帰 に しない。BulletML に深さ を止める 機構 が無い ので上界 が効かなくなる
let private bullets (d: PatternSpec) : BulletmlElm list =
    [
        for lv in 0 .. step d.Cascade do
            let isLast = lv = step d.Cascade

            defBullet (levelName lv) {
                doActs (
                    body {
                        if d.Breathe && lv = 0 then
                            changeSpeed "0.25" "26"
                            wait "34"
                            changeSpeed (speedExpr d) "40"

                        // 自機 を追う。頭 の向き で効かせる と `Spiral` の渦 が止まる ので、弾 の側 で持つ
                        // 段 が無くて も 効く ように、撒く 枝 の外 に置く
                        if d.Aiming then
                            changeDirectionAim "0" "60"

                        // 1 発 が どう 飛ぶ か。頭 の 1 段 目 に だけ 置く
                        // 段 の先 まで 引き継ぐ と、割れた 破片 まで レーザー に なって 形 が消える
                        if lv = 0 then
                            match d.Motion with
                            | Laser ->
                                // 撃った 直後 に 一気 に伸びる。短い term が 線 に見せる
                                changeSpeed (laserSpeed d) "8"
                            | Missile ->
                                // 曲がり ながら 加速。狙い は `Aiming` と別 に持つ ——
                                // ミサイル と 言われた なら 狙う のが 本体
                                changeDirectionAim "0" "30"
                                accel "40" { vertical (missileAccel d) }
                            | Plain -> ()

                        if not isLast then
                            repeat (scatterExpr d lv) {
                                fire {
                                    sequence (scatterStep d lv)
                                    speed (subSpeedExpr d lv)
                                    refBullet (levelName (lv + 1)) []
                                }
                            }

                        if d.Vanishing then
                            wait "180"
                            vanish
                    }
                )
            }
    ]

/// 並行 の層。`topN` という口 は無い —— `top` は名前 が "top" 固定 なので
/// 2 本目 以降 は `defAction "top2"`。
let private layers (d: PatternSpec) : BulletmlElm list =
    [
        for lv in 2 .. step d.Layers + 1 do
            defAction (sprintf "top%d" lv) {
                // 同時 に出す と 重なって 1 層 に見える
                wait (string (lv * 20))

                repeat (wavesExpr d) {
                    repeat (armsExpr d) {
                        // 偶数 の層 は逆 に回る（干渉縞）
                        fire {
                            sequence (sprintf "%d * (%s)" (if lv % 2 = 0 then -1 else 1) (armStep d))
                            speedSeq "0"
                            refBullet (layerBullet lv) []
                        }
                    }

                    wait (waitExpr d 1)
                }
            }

        for lv in 2 .. step d.Layers + 1 do
            defBullet (layerBullet lv) {
                speed (subSpeedExpr d (lv - 1))

                doActs (
                    body {
                        wait "180"
                        vanish
                    }
                )
            }
    ]

/// `Parametrized` が真 のとき の部品。`$1` が開始角、`$2` が向き
let private armPart (d: PatternSpec) : BulletmlElm list =
    // 幕 は `arm` を呼ばない（`arms` が外す）ので、定義 だけ 残さない ——
    // 誰 も引かない `action` が字 に出ると、読む人 が形 を誤解 する
    if not d.Parametrized || d.Kind = Curtain then
        []
    else
        [
            defAction "arm" {
                fire {
                    absolute "$1"
                    speed (speedExpr d)
                    refBullet "core" []
                }

                repeat (armsExpr d) {
                    fire {
                        sequence (sprintf "$2 * (%s)" (armStep d))
                        speedSeq "0"
                        refBullet "core" []
                    }
                }
            }
        ]

/// 中 で `fit` を通す —— 呼ぶ側 に任せる と 上限 が守られない 経路 が生まれる。
/// 上限 そのもの は 引数（混ぜ と 散らし は 1 面 を分け合う）
let generateTo (budget: float) (spec: PatternSpec) : BulletmlInfo =
    let d = fitTo budget spec

    createBulletmlInfo
    <| vertical "AI" {
        top {
            repeat (wavesExpr d) {
                // 外 が波、中間 が `Depth` 段、内 が腕
                yield! nestRepeat (step d.Depth) (midExpr d) (arms d @ [ wait (waitExpr d 0) ])

                if d.Pause then
                    wait (pauseExpr d)
            }
        }

        yield! layers d
        yield! armPart d
        yield! bullets d
    }

let generate (spec: PatternSpec) : BulletmlInfo =
    generateTo (float Consts.MAX_ALIVE) spec
