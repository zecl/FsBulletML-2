namespace FsBulletML

open System
open System.Diagnostics
open System.Globalization
open System.IO
open System.Text
open System.Xml
open System.Text.RegularExpressions
open FsBulletML.Domain

[<AutoOpen>]
module Processable =

  open Microsoft.FSharp.Core.Operators.Unchecked

  type IBulletMLManager =
    abstract GetRandom : unit -> float32
    abstract GetRank : unit -> float32
    abstract GetPlayerPosX : unit -> float32
    abstract GetPlayerPosY : unit -> float32

  type BulletMLManager ()=
    static let mutable ib : IBulletMLManager = defaultof<IBulletMLManager>
    static member Init(ib1:IBulletMLManager) =
      ib <- ib1
    static member GetRandom() =  ib.GetRandom()
    static member GetRank() = ib.GetRank()
    static member GetPlayerPosX() = ib.GetPlayerPosX()
    static member GetPlayerPosY() = ib.GetPlayerPosY()

  let getValue (env: Domain.Env) (s:string) =
    let rand = env.Rand ()
    let rank = env.Rank
    let s = s.Replace("$rand", rand.ToString(CultureInfo.InvariantCulture))
             .Replace("$rank", rank.ToString(CultureInfo.InvariantCulture))
    // 置き換え残りの $N を 0 に潰す。\d が \$d* と書かれていて、
    // $ だけが 0 になり数字が残っていた（$1 が "01" = 1 になる）
    let s = System.Text.RegularExpressions.Regex.Replace(s,"\$\d*","0")
    TryParse.eval s

  type IBulletmlObject =
    abstract AccelerationX : float32 with get, set
    abstract AccelerationY : float32 with get, set
    abstract X : float32 with get,set
    abstract Y : float32 with get,set
    abstract Speed : float32 with get,set
    abstract Dir : float32 with get,set
    abstract Vanish : unit -> unit
    abstract GetNewBullet : unit -> IBulletmlObject
    abstract GetAimDir : unit -> float32
    abstract GetEnemyAimDir : unit -> float32
    abstract Init : unit -> unit
    abstract Task : BulletmlTask option with get,set
    abstract BulletType : BulletType with get,set
    abstract ShootingDirection : ShootingDirection with get,set
    abstract Used : bool with get,set
    abstract IsBullet : bool with get,set
    abstract BulletRoot : bool with get,set

  /// BulletmlTask が Init() の中で Tops を組み直すのに使う関数を、走らせる側
  /// （BulletRunner.fs）から注入してもらう入口。Step.fs / IntermediateParser.fs
  /// はこのファイルより後に compile されるので、ここから直接は呼べない
  /// （BulletRunner.calcDir が Step.calcDir を指す別名にしてあるのと同じ理由。
  /// 旧コードもここを toProcessable という同じ形で受けていた）。
  ///
  ///   resetTop     Step.resetChild。Init(env) の None 腕（既にある木を
  ///                歩き直す）が使う
  ///   rebuildRoot  bulletml から根の Tops を丸ごと組み直す経路
  ///                （BulletRunner.buildRootTops、convertBulletmlTask の
  ///                根の Tops 構築と同じもの）。Init(env) の Some 腕
  ///                （Original から作り直す）が使う
  ///
  /// 評価ごとに変わる場所は ref セル 1 つずつに閉じ込めてある。旧は木の
  /// ノードに finish / term / first などの mutable フィールドが 39 個
  /// 埋まっていたが、それらは実行位置の木（Domain.Progress、State の中）へ
  /// 移った。ここに残るのはその木そのものを差し替える口だけ
  and BulletmlTask internal (resetTop: Domain.Env -> RecBulletml -> Domain.Progress,
                              rebuildRoot: Bulletml -> (RecBulletml * Domain.Progress) list,
                              scripts: RecBulletml list,
                              initialState: Domain.BulletState) =
    let state = ref initialState
    let finish = ref false
    let shootingDirection : ShootingDirection ref = ref (defaultof<ShootingDirection>)
    let original : Bulletml option ref = ref None
    /// 輪のために展開を止めた bulletRef を、走らせる側から 1 段だけ解く入口。
    /// label と param を渡すと、その bullet を 1 段展開したものが返る
    let resolveBulletRef : (string -> string list -> RecBulletml option) ref = ref (defaultof<_>)
    /// 輪のために展開を止めた actionRef を、走らせる側から 1 段だけ解く入口。
    /// label と param を渡すと、その action を 1 段展開したものが返る
    let resolveActionRef : (string -> string list -> RecBulletml option) ref = ref (defaultof<_>)

    member internal _.ResolveBulletRef with get () = resolveBulletRef.Value
                                        and set (v) = resolveBulletRef.Value <- v
    member internal _.ResolveActionRef with get () = resolveActionRef.Value
                                        and set (v) = resolveActionRef.Value <- v
    /// 実行位置・fire の累積・弾それ自体の物理量を持つ現在の状態。
    /// BulletRunner.stateOfBullet / applyToBullet が毎フレーム読み書きする
    member internal _.State with get () = state.Value
                             and set (v) = state.Value <- v
    /// top* のスクリプト（doc 順）。State.Tops からも辿れるが、走らせる側が
    /// 弾オブジェクトを介さずに並びだけ見たいとき用に別で持つ
    member internal _.Scripts = scripts
    member internal _.ShootingDirection with get () = shootingDirection.Value
                                         and set (v) = shootingDirection.Value <- v
    member _.Finish with get () = finish.Value
                     and set (v) = finish.Value <- v
    member _.Original with get () = original.Value
                       and set (v) = original.Value <- v
    member _.BulletName
      with get () =
        match original.Value with
        | Some bulletml -> bulletml.Name
        | None -> None

    /// 旧の BulletmlTask.Init の写し。
    ///
    ///   member this.Init (env: Domain.Env) = this.Original |> function
    ///     | Some x -> this.Tasks <- toProcessable x
    ///     | None -> this.tasks |> Seq.iter (fun p -> p.Init(env))
    ///
    /// Original はエンジン自身の経路（createTask / convertBulletmlTask）では
    /// 常に None だが、Original の getter / setter も Init も public であり、
    /// 外から `t.Original <- Some otherBulletml; t.Init(env)` が組める
    /// （呼べない ResolveBulletRef 等はすぐ上で internal にしてあるので、
    /// Original が internal でないのは意図）。RefParamFreeze の
    /// 「param の中身と Original の関係」は Original を書き換えないので、
    /// この Some 腕には届かない。届く経路がある以上、写す。
    ///
    /// None（木を歩き直す）と Some（丸ごと組み直す）は引く draw が違う。
    ///   None  Step.resetChild。木の全体を歩いて wait / changeDirection /
    ///         changeSpeed の term を引き直す（Action 自身の pa.loop も
    ///         None へ戻ってから children を歩く、Init 型の draw）
    ///   Some  rebuildRoot（旧の toProcessable、いまは convertBulletmlTask の
    ///         根の Tops 構築と同じ経路）。木を組む段の draw で、wait の
    ///         term だけを document 順にまとめて引き、accel /
    ///         changeDirection / changeSpeed は引かない（rootProgress、
    ///         設計文書 5.6）。旧の Some 腕も Init の env 引数を使わず
    ///         グローバルを直に読んでいた（IntermediateParser.fs の
    ///         RecBulletml.Wait の腕）ので、rebuildRoot も env を受けない
    ///
    /// fire の累積（旧の bulletmlTask.FireData、いまは Tops の各 FireContext）は
    /// 旧はどちらの腕でも触っていない（Tasks を差し替えるだけで、別配列の
    /// FireData は変えない）。新も、Step.step が実際に読み書きする先頭の組の
    /// 共有値をそのまま引き継ぐ
    member _.Init (env: Domain.Env) =
      match original.Value with
      | Some bulletml ->
          let fc =
            match state.Value.Tops with
            | (_, _, fc0) :: _ -> fc0
            | [] -> FireContext.zero
          let tops = rebuildRoot bulletml
          state.Value <- { state.Value with Tops = tops |> List.map (fun (s, p) -> s, p, fc) }
      | None ->
          state.Value <-
            { state.Value with
                Tops = state.Value.Tops |> List.map (fun (s, _, fc) -> s, resetTop env s, fc) }
