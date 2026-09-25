namespace FsBulletML2.MonoGame

open System.Collections.Generic
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

module Manager =

    [<CompiledName("Enemies")>]
    let enemies: List<IBullet> = new List<IBullet>()

    [<CompiledName("EnemyBullets")>]
    let enemyBullets: List<IBullet> = new List<IBullet>()

    [<CompiledName("PlayerBullets")>]
    let playerBullets: List<IBullet> = new List<IBullet>()

    // 画面 を区画 の格子 に切る。画面 の外 の弾 は端 の区画 に入れる
    let private columns = int (ceil (Settings.Display.Width / Settings.Space.Width))
    let private rows = int (ceil (Settings.Display.Height / Settings.Space.Height))

    [<CompiledName("SpaceMax")>]
    let spaceMax = columns * rows

    // 区画 ごと に別 の List。`Array.create` だと全部 の区画 が同じ 1 本 を指す
    [<CompiledName("EnemySpaces")>]
    let enemySpaces: List<IBullet> array =
        Array.init spaceMax (fun _ -> new List<IBullet>())

    [<CompiledName("EnemyBulletSpaces")>]
    let enemyBulletSpaces: List<IBullet> array =
        Array.init spaceMax (fun _ -> new List<IBullet>())

    [<CompiledName("PlayerBulletSpaces")>]
    let playerBulletSpaces: List<IBullet> array =
        Array.init spaceMax (fun _ -> new List<IBullet>())

    // 区画 に入れた弾 の最大 の半径。当たり を探す範囲 をこれ だけ 広げる
    let mutable private enemyBulletReach = 0.f
    let mutable private playerBulletReach = 0.f

    [<CompiledName("AddEnemy")>]
    let addEnemy (enemy: IBullet) = enemies.Add(enemy)

    [<CompiledName("AddEnemyPos")>]
    let addEnemyPos (enemy: IBullet, original: Vector2) =
        enemies.Add(enemy)
        enemy.X <- original.X
        enemy.Y <- original.Y

    [<CompiledName("AddEnemyBullet")>]
    let addEnemyBullet (bullet: IBullet) = enemyBullets.Add(bullet)

    [<CompiledName("AddEnemyBulletPos")>]
    let addEnemyBulletPos (bullet: IBullet, original: Vector2) =
        enemyBullets.Add(bullet)
        bullet.X <- original.X
        bullet.Y <- original.Y

    [<CompiledName("AddPlayerBullet")>]
    let addPlayerBullet (bullet: IBullet) = playerBullets.Add(bullet)

    [<CompiledName("AddPlayerBulletPos")>]
    let addPlayerBulletPos (bullet: IBullet, original: Vector2) =
        playerBullets.Add(bullet)
        bullet.X <- original.X
        bullet.Y <- original.Y

    [<CompiledName("Update")>]
    let update () =
        let update (source: List<IBullet>) =
            for i = 0 to source.Count - 1 do
                source.[i].Update()

        [ enemies; enemyBullets; playerBullets ] |> List.iter update

    [<CompiledName("Free")>]
    let free () =
        let free (source: List<IBullet>) =
            let mutable i = 0

            for j = 0 to source.Count - 1 do
                if not source.[i].Used then
                    source.Remove(source.[i]) |> ignore
                    i <- i - 1

                i <- i + 1

        [ enemies; enemyBullets; playerBullets ] |> List.iter free

    [<CompiledName("RemoveAll")>]
    let removeAll () =
        [ enemies; enemyBullets; playerBullets ] |> Seq.iter (fun x -> x.Clear())
        enemyBulletSpaces |> Seq.iter (fun x -> x.Clear())
        playerBulletSpaces |> Array.iter (fun x -> x.Clear())
        enemyBulletReach <- 0.f
        playerBulletReach <- 0.f

    [<CompiledName("GetDrawPos")>]
    let getDrawPos (pos: Vector2) (texture: Texture2D) =
        new Vector2(pos.X - (float32 (texture.Width / 2)), pos.Y - (float32 (texture.Height / 2)))

    /// 列 か行 の番号。外 は端 へ寄せる（NaN は 0）
    let private cellOf (v: float32) (size: float32) (count: int) =
        let c = floor (v / size)

        if System.Single.IsNaN c || c < 0.f then 0
        elif c >= float32 count then count - 1
        else int c

    [<CompiledName("GetSpaceIndex")>]
    let getSpaceIndex (pos: Vector2) =
        cellOf pos.Y Settings.Space.Height rows * columns
        + cellOf pos.X Settings.Space.Width columns

    [<CompiledName("UpdateSpace")>]
    let updateSpace () =
        enemyBulletSpaces |> Array.iter (fun x -> x.Clear())
        playerBulletSpaces |> Array.iter (fun x -> x.Clear())

        let add (spaces: List<IBullet> array) (source: List<IBullet>) =
            let mutable reach = 0.f

            for target in source do
                spaces.[getSpaceIndex target.Pos].Add(target)
                reach <- max reach target.Radius

            reach

        enemyBulletReach <- add enemyBulletSpaces enemyBullets
        playerBulletReach <- add playerBulletSpaces playerBullets

    /// 半径 の和 より近い弾 は、`pos` から「半径 ＋ 入れた弾 の最大 の半径」の中 の区画 に必ず在る。
    /// 端 へ寄せても寄せ方 は単調 なので、その区画 の範囲 に入る
    [<CompiledName("CheckCollision")>]
    let private checkCollision (pos: Vector2) (radius: float32) (targetSpaces: List<IBullet> array) reach free cont =
        let r = radius + reach
        let x0 = cellOf (pos.X - r) Settings.Space.Width columns
        let x1 = cellOf (pos.X + r) Settings.Space.Width columns
        let y0 = cellOf (pos.Y - r) Settings.Space.Height rows
        let y1 = cellOf (pos.Y + r) Settings.Space.Height rows

        for cy in y0..y1 do
            for cx in x0..x1 do
                for target in targetSpaces.[cy * columns + cx] do
                    let distance = Vector2.Distance(target.Pos, pos)

                    if (distance < target.Radius + radius) then
                        free (target)
                        cont ()

    [<CompiledName("CheckPlayerCollision")>]
    let checkPlayerCollision playerPos radius cont =
        checkCollision playerPos radius enemyBulletSpaces enemyBulletReach (fun target -> target.Used <- false) cont

    [<CompiledName("CheckEnemyCollision")>]
    let checkEnemyCollision enemyPos radius cont =
        checkCollision enemyPos radius playerBulletSpaces playerBulletReach (fun target -> target.Used <- false) cont
