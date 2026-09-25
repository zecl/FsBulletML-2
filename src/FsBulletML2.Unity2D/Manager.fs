namespace FsBulletML2.Unity2D

open System
open System.Collections.Generic
open UnityEngine

module Manager =

    [<CompiledName("Enemies")>]
    let enemies: List<IDefaultBullet> = new List<IDefaultBullet>()

    [<CompiledName("RootBullets")>]
    let rootBullets: List<IDefaultBullet> = new List<IDefaultBullet>()

    [<CompiledName("EnemyBullets")>]
    let enemyBullets: List<IDefaultBullet> = new List<IDefaultBullet>()

    [<CompiledName("PlayerBullets")>]
    let playerBullets: List<IDefaultBullet> = new List<IDefaultBullet>()

    // 画面 を区画 の格子 に切る。画面 の外 の弾 は端 の区画 に入れる
    let private columns = int (ceil (Settings.Display.Width / Settings.Space.Width))
    let private rows = int (ceil (Settings.Display.Height / Settings.Space.Height))

    [<CompiledName("SpaceMax")>]
    let spaceMax = columns * rows

    // 区画 ごと に別 の List。`Array.create` だと全部 の区画 が同じ 1 本 を指す
    [<CompiledName("EnemySpaces")>]
    let enemySpaces: List<IDefaultBullet> array =
        Array.init spaceMax (fun _ -> new List<IDefaultBullet>())

    [<CompiledName("EnemyBulletSpaces")>]
    let enemyBulletSpaces: List<IDefaultBullet> array =
        Array.init spaceMax (fun _ -> new List<IDefaultBullet>())

    [<CompiledName("PlayerBulletSpaces")>]
    let playerBulletSpaces: List<IDefaultBullet> array =
        Array.init spaceMax (fun _ -> new List<IDefaultBullet>())

    // 区画 に入れた弾 の最大 の半径。当たり を探す範囲 をこれ だけ 広げる
    let mutable private enemyBulletReach = 0.f
    let mutable private playerBulletReach = 0.f

    [<CompiledName("AddEnemy")>]
    let addEnemy (enemy: IDefaultBullet) = enemies.Add(enemy)

    [<CompiledName("AddRootBullet")>]
    let addRootBullet (bullet: IDefaultBullet) = rootBullets.Add(bullet)

    [<CompiledName("AddRootBulletPos")>]
    let addRootBulletPos (bullet: IDefaultBullet, original: Vector2) =
        rootBullets.Add(bullet)
        bullet.X <- original.x
        bullet.Y <- original.y

    [<CompiledName("AddEnemyPos")>]
    let addEnemyPos (enemy: IDefaultBullet, original: Vector2) =
        enemies.Add(enemy)
        enemy.X <- original.x
        enemy.Y <- original.y

    [<CompiledName("AddEnemyBullet")>]
    let addEnemyBullet (bullet: IDefaultBullet) = enemyBullets.Add(bullet)

    [<CompiledName("AddEnemyBulletPos")>]
    let addEnemyBulletPos (bullet: IDefaultBullet, original: Vector2) =
        enemyBullets.Add(bullet)
        bullet.X <- original.x
        bullet.Y <- original.y

    [<CompiledName("AddPlayerBullet")>]
    let addPlayerBullet (bullet: IDefaultBullet) = playerBullets.Add(bullet)

    [<CompiledName("AddPlayerBulletPos")>]
    let addPlayerBulletPos (bullet: IDefaultBullet, original: Vector2) =
        playerBullets.Add(bullet)
        bullet.X <- original.x
        bullet.Y <- original.y

    [<CompiledName("Free")>]
    let free () =
        let free (source: List<IDefaultBullet>) =
            let mutable i = 0

            for j = 0 to source.Count - 1 do
                if not source.[i].Used then
                    let mb = source.[i] :?> UnityEngine.MonoBehaviour

                    if mb <> null then
                        mb.enabled <- false
                        source.Remove(source.[i]) |> ignore
                        UnityEngine.Object.Destroy(mb.gameObject)
                        i <- i - 1

                i <- i + 1

        [ enemies; rootBullets; enemyBullets; playerBullets ] |> List.iter free

    [<CompiledName("RemoveNonEnemy")>]
    let removeNonEnemy () =
        [ rootBullets; enemyBullets; playerBullets ]
        |> Seq.iter (fun x -> x |> Seq.iter (fun b -> b.Used <- false))

    [<CompiledName("RemoveAll")>]
    let removeAll () =
        [ enemies; rootBullets; enemyBullets; playerBullets ]
        |> Seq.iter (fun x -> x |> Seq.iter (fun b -> b.Used <- false))

    /// 列 か行 の番号。外 は端 へ寄せる（NaN は 0）
    let private cellOf (v: float32) (size: float32) (count: int) =
        let c = floor (v / size)

        if Single.IsNaN c || c < 0.f then 0
        elif c >= float32 count then count - 1
        else int c

    [<CompiledName("GetSpaceIndex")>]
    let getSpaceIndex (pos: Vector2) =
        cellOf pos.y Settings.Space.Height rows * columns
        + cellOf pos.x Settings.Space.Width columns

    [<CompiledName("UpdateSpace")>]
    let updateSpace () =
        enemyBulletSpaces |> Array.iter (fun x -> x.Clear())
        playerBulletSpaces |> Array.iter (fun x -> x.Clear())

        let add (spaces: List<IDefaultBullet> array) (source: List<IDefaultBullet>) =
            let mutable reach = 0.f

            for target in source do
                spaces.[getSpaceIndex (Vector2(target.Pos.x, target.Pos.y))].Add(target)
                reach <- max reach target.Radius

            reach

        enemyBulletReach <- add enemyBulletSpaces enemyBullets
        playerBulletReach <- add playerBulletSpaces playerBullets

    /// 半径 の和 より近い弾 は、`pos` から「半径 ＋ 入れた弾 の最大 の半径」の中 の区画 に必ず在る。
    /// 端 へ寄せても寄せ方 は単調 なので、その区画 の範囲 に入る
    [<CompiledName("CheckCollision")>]
    let private checkCollision
        (pos: Vector2)
        (radius: float32)
        (targetSpaces: List<IDefaultBullet> array)
        reach
        free
        cont
        =
        let r = radius + reach
        let x0 = cellOf (pos.x - r) Settings.Space.Width columns
        let x1 = cellOf (pos.x + r) Settings.Space.Width columns
        let y0 = cellOf (pos.y - r) Settings.Space.Height rows
        let y1 = cellOf (pos.y + r) Settings.Space.Height rows

        for cy in y0..y1 do
            for cx in x0..x1 do
                for target in targetSpaces.[cy * columns + cx] do
                    let distance = Vector2.Distance(Vector2(target.Pos.x, target.Pos.y), pos)

                    if (distance < target.Radius + radius) then
                        free (target)
                        cont ()

    [<CompiledName("CheckPlayerCollision")>]
    let checkPlayerCollision playerPos radius cont =
        checkCollision playerPos radius enemyBulletSpaces enemyBulletReach (fun target -> target.Used <- false) cont

    [<CompiledName("CheckEnemyCollision")>]
    let checkEnemyCollision enemyPos radius cont =
        checkCollision enemyPos radius playerBulletSpaces playerBulletReach (fun target -> target.Used <- false) cont
