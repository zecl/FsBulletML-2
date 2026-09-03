namespace FsBulletML2
// 旧 API（IBulletmlObject）の Obsolete 警告を、**このファイルだけ**止める。
// ここは旧 API のシムそのもので、旧の型に触るのが仕事だから。
//
// プロジェクト単位（NoWarn）で止めない。止めると、**新しく書いたコードが
// うっかり旧 API を使っても警告が出なくなる**。抑制はいつも、意図して
// 旧経路を使っているファイルの中だけに置く。
#nowarn "44"


open System
open System.Diagnostics
open System.Globalization
open System.IO
open System.Text
open System.Xml
open System.Text.RegularExpressions
open FsBulletML2.Domain

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

  /// 式の値。走行中はここを通る。
  ///
  /// 木は Expr.NumExpr が読んだ時点で組んであるので、ここは評価するだけ。
  /// **乱数は式の中身によらず 1 回 だけ引く**（$rand が何個 あっても、
  /// 1 個 も無くても 1 回）。引く回数は乱数の並びを進めるので、
  /// 下の getValueByXPath と揃っていなければ全弾幕の軌跡がずれる
  let getValue (env: Domain.Env) (e: Expr.NumExpr) =
    Expr.NumExpr.eval env.Rand env.Rank e

  /// 旧実装。文字列を毎回 XPath で評価する。
  ///
  /// **走行はもうここを通らない。** 残してあるのは ExprTests が
  /// 「木が同じ値を返すか」を突き合わせる相手として要るから。消すと、
  /// 木が正しいことを確かめる基準が無くなる。
  ///
  /// この実装には穴が 2 つ ある（どちらも ExprTests が名指しで固定している）。
  ///   - $rand / $rank が 1e-4 未満だと ToString が "1E-07" を吐き、
  ///     xpathNumber の空白入れがそれを割って XPathException になる
  ///   - 読めない式で例外になる（木のほうは NaN）
  let getValueByXPath (env: Domain.Env) (s:string) =
    let rand = env.Rand ()
    let rank = env.Rank
    let s = s.Replace("$rand", rand.ToString(CultureInfo.InvariantCulture))
             .Replace("$rank", rank.ToString(CultureInfo.InvariantCulture))
    // 置き換え残りの $N を 0 に潰す。\d が \$d* と書かれていて、
    // $ だけが 0 になり数字が残っていた（$1 が "01" = 1 になる）
    let s = System.Text.RegularExpressions.Regex.Replace(s,"\$\d*","0")
    TryParse.eval s

  /// **旧 API。** エンジンがフロントを呼び返すための 19 メンバ。
  ///
  /// 実装する側が「いつ呼ばれるか」を知らないと書けなかった。新 API
  /// （Runner.step / BulletRun / Frame）は値の受け渡しだけで呼び返しが無い。
  /// 同梱フロント 2 つ は移してある。
  ///
  /// **残していた理由は無くなった。**
  ///
  /// 消せなかったのは、GetNewBullet が null を返したとき（弾プールが尽きた等）に
  /// fire の累積（SrcSpeed / SpeedInit）を巻き戻す振る舞いが旧にしか無かったため。
  /// 参照実装を 2 本 当たったところ、**その意味論はどちらにも無かった**
  /// （libbulletml は断る口そのものが無く、BulletMLLib は断れるが向きと速さは
  /// 手前で計算済み）。移植のときに入った独自の振る舞いで、しかも SrcDir は
  /// 進んで SrcSpeed だけ止まる非対称だった。**新 API へは持っていかない**と
  /// 決めた —— 根拠は Api.fs の Frame.Spawned の但し書き。
  ///
  /// **廃止すると決めた。ただし いったん保留。** 下ごしらえは 2 つ 入っている。
  ///
  /// **消すと何が失われるか**（消す前にここを読むこと）:
  ///
  ///   橋 227 本        **もう失われない。** 本物の新旧を見る唯一の門
  ///                   （凍結した旧エンジン 4077ed6 の軌跡との突き合わせ）は
  ///                   公開 API へ付け替え済み。Trace.fs は BulletRunner 層が
  ///                   Step.step と食い違わないかを見る側に残っている
  ///   ベンチの新旧比較  Harness.fs の runPrepared / allocOld。**この列は
  ///                   対照ではない**（BulletRunner は旧い口を新経路の上に
  ///                   載せたシムで、中で Step.step を通る）。それでも
  ///                   「走行間の台のドリフトを見る」役目は残っているので、
  ///                   代わりを決めてから消すこと
  ///   Fake.fs         FakeBullet は IBulletmlObject の実装で、橋の材料
  ///
  /// **消すと得られるもの**: Core テストの NonParallelizable。
  /// あれは旧 API がグローバル（BulletMLManager）から読む形だから要るもので、
  /// 新 API は Env を引数で受けるので要らなくなる。**軌跡テスト 13 本 を
  /// 公開 API へ移した時点で 23 → 10 fixture まで減っている**（この但し書きは
  /// 24 と書いていたが、実際に属性が付いていたのは 23 だった —— 24 本め は
  /// CallingConvention.fs のコメントを grep が拾っていた）。
  ///
  /// ベンチの対照を何に置き換えるかを決めてから消すこと。
  /// **「消せる」と「いま消す」は別。**
  [<System.Obsolete("新 API（Runner.step / BulletRun / Frame）へ移してください。移し方は Api.fs の Runner の但し書き。")>]
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
    /// これから GetNewBullet() で産まれる弾の位置から見た、自機への向き。
    ///
    /// <bullet><direction type="aim"> の基準は「撃った側」ではなく
    /// 「撃たれた弾」なので、撃つ側の GetAimDir では答えが違う。
    /// 産まれる弾がどこに出るかはフロントエンドが決めていて Core には
    /// 分からないため、ここで問い合わせる。
    ///
    /// 産まれた弾を実際に作ってから読む形にすると、「撃つ」という値の決定に
    /// 実体の生成が要ることになり、Spawn を値として返せなくなる。
    abstract GetSpawnAimDir : unit -> float32
    /// 同上。撃つ側が Player のときに使う敵への向き
    abstract GetSpawnEnemyAimDir : unit -> float32
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
  and [<System.Obsolete("弾幕は BulletmlScript、実行状態は BulletRun に割れました。Api.fs の Runner を見てください。")>]
      BulletmlTask internal (resetTop: Domain.Env -> RecActionElm -> Domain.Progress,
                              rebuildRoot: Bulletml -> (RecActionElm * Domain.Progress) list,
                              scripts: RecActionElm list,
                              initialState: Domain.BulletState) =
    let state = ref initialState
    let finish = ref false
    let shootingDirection : ShootingDirection ref = ref (defaultof<ShootingDirection>)
    let original : Bulletml option ref = ref None
    /// 輪のために展開を止めた bulletRef を、走らせる側から 1 段だけ解く入口。
    /// label と param を渡すと、その bullet を 1 段展開したものが返る
    let resolveBulletRef : (BulletLabel -> string list -> RecBulletElm option) ref = ref (defaultof<_>)
    /// 輪のために展開を止めた actionRef を、走らせる側から 1 段だけ解く入口。
    /// label と param を渡すと、その action を 1 段展開したものが返る
    let resolveActionRef : (ActionLabel -> string list -> RecActionElm option) ref = ref (defaultof<_>)

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
