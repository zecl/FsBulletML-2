namespace FsBulletML.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML
open FsBulletML.DTD
open FsBulletML.Domain

/// 実行位置を木の外へ出したもの。Script と同じ形の別の木になる。
///
/// 初期化の入口が 1 本しかないことが要点。現行は入口が 2 つ在って
/// 入れる値が違い、wait が 1 フレーム短くなる不具合になっていた。
[<TestFixture>]
type ProgressTests() =

  [<Test>]
  member _.``wait は、term を評価せずに置く``() =
    // 現行も ProcessableWait を作る時点では評価していない。
    // ここで評価すると $rand を読む回数が変わって、控えが動く
    match Progress.initial (RecBulletml.Wait "3") with
    | PWait (started, left) ->
        started |> should equal false
        left |> should equal 0.0f
    | p -> Assert.Fail (sprintf "PWait のはずが %A" p)

  [<Test>]
  member _.``action は、子ぶんの Progress を並べて持つ``() =
    let script =
      RecBulletml.Action ({ actionLabel = Some "top" },
                          [ RecBulletml.Wait "1"; RecBulletml.Vanish ])
    match Progress.initial script with
    | PAction (done_, loop, children) ->
        done_ |> should equal false
        loop |> should equal None
        List.length children |> should equal 2
    | p -> Assert.Fail (sprintf "PAction のはずが %A" p)

  [<Test>]
  member _.``repeat は、子 1 つぶんを持つ``() =
    let body = RecBulletml.Action ({ actionLabel = None }, [ RecBulletml.Wait "1" ])
    match Progress.initial (RecBulletml.Repeat (Times "3", body)) with
    | PRepeat (num, done_, child) ->
        num |> should equal 0
        done_ |> should equal false
        match child with
        | PAction _ -> ()
        | p -> Assert.Fail (sprintf "子は PAction のはずが %A" p)
    | p -> Assert.Fail (sprintf "PRepeat のはずが %A" p)

  [<Test>]
  member _.``初期化の入口は 1 本。同じ Script から 2 回 作ると等しい``() =
    let script =
      RecBulletml.Action ({ actionLabel = Some "top" },
                          [ RecBulletml.Wait "3"; RecBulletml.Vanish ])
    Progress.initial script |> should equal (Progress.initial script)

  [<Test>]
  member _.``BulletState は、top ごとに script と Progress と FireContext を組で持つ``() =
    let script =
      RecBulletml.Action ({ actionLabel = Some "top" }, [ RecBulletml.Wait "1" ])
    let st =
      { Pos = { X = 0.f; Y = 0.f }
        Speed = 1.f
        Dir = 0.f
        Accel = { X = 0.f; Y = 0.f }
        Kind = BulletType.Enemy
        IsBullet = false
        HasFired = false
        Tops = [ script, Progress.initial script, FireContext.zero ]
        PendingBulletAim = false }
    List.length st.Tops |> should equal 1
    let _, _, fc = st.Tops.Head
    fc.SrcSpeed |> should equal 0.0f
    fc.SpeedInit |> should equal false
