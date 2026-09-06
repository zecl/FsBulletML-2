namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.Playground

/// **属性を書かなかったときに走る値が、Core のマーカーと合っているか。**
///
/// 既定は AST に残らない。読む段は属性が無ければ Attrs ごと None にするので、
/// 値が決まるのは Step / Api の fall-through（`| None -> aim` の形）。
/// `[<BulletmlDefault>]` はそれを型の側へ写した札で、**写し間違えても
/// コンパイルは通る。** ここが唯一 当たる場所。
///
/// --- 字でなく走りで当てる
///
/// 「腕に札が付いている」だけを見ると、実装のほうが動いたときに気づけない。
/// だから走らせて比べる ——
///
///   属性を省いた走り  ==  札の値を書いた走り
///
/// --- 0 件 を緑にしない
///
/// 上の一致は、**その属性が何にも効いていなくても緑になる。** 効いていなければ
/// どの値を書いても同じだから。だから逆向きも置く ——
///
///   札でない値のどれかを書くと  !=  省いた走り
///
/// --- 表と、その覆い
///
/// 弾幕として成り立つ形は機械では組めないので、下の `templates` だけは表。
/// **語彙と過不足なく一致すること**を別の点で見る（両向き）。
/// 語彙は `Core/DTD.fs` から reflection で出るので、Core に並びが増えて
/// ここに書き忘れれば赤くなる。
///
/// --- 借りているファイル
///
/// `Vocabulary.fs` は `Link` で借りている。**そのファイルだけを触った PR では
/// この試験が選ばれない**（`VocabularyCoverage` と同じ穴）。
/// Core を触ると全部 走るので、守りたい向きは塞がっている。
///
/// --- 較正（当てた変異と、赤くなった点）
///
/// 変異 1 つ につき赤くなるのは 1 点 だけ。素はその前後とも緑。
///
///   direction の札を Absolute にも足す        既定は ちょうど 1 個
///   speed の札を外す                          既定は ちょうど 1 個
///   direction の札を aim から relative へ     省いた走りは既定と同じ
///   action/@label（自由記述）に札を付ける     自由記述に既定は付いていない
///   vertical の形を表から落とす               形が過不足なく一致する
///   accel を撃った弾から根の top へ戻す       既定でない値で走りが変わる
///
/// 最後の 1 つ は最初に書いた形そのもの。**根の top に置いた accel は
/// 2 コマ の no-op になる**ので、どの値を書いても走りが変わらず、
/// 「省いた == 既定」だけなら緑のまま通っていた。
///
/// 変異を戻すときは mtime も進めること。**元の時刻ごと書き戻すと
/// MSBuild が焼き直さず、前の変異が dll に残ったまま次の測定が走る**
/// （実際にそれで 1 巡 汚れた）。
[<TestFixture>]
type AttributeDefaults() =

  [<Literal>]
  static let Ns = "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"

  /// 値の並びを持つ属性。**表を書かない** —— 語彙から拾う
  static let enumerated =
    [| for e in Vocabulary.elements do
         for a in e.Attrs do
           if a.Values.Length > 0 then yield e.Name, a |]

  static let wrap (body: string) =
    sprintf "<bulletml xmlns=\"%s\">%s</bulletml>" Ns body

  /// accel は**撃った弾の中に置く。** 根の top に直に置くと 2 コマ の no-op に
  /// なる（`Step.rootProgress` の但し書き。根は Init を通らないので、
  /// accel の placeholder が「もう評価済み」のまま最初のコマへ入る）。
  /// 根に置いたままだと、どの値を書いても走りが変わらない
  static let accelIn (axis: string) (attr: string) =
    sprintf "<action label=\"top\"><fire><speed>1</speed><bullet><action><accel><%s%s>2</%s><term>4</term></accel><wait>8</wait></action></bullet></fire><wait>9</wait></action>"
      axis attr axis

  /// 属性のところだけ差し替えられる弾幕。渡すのは ` type=\"aim\"` のような字か、空。
  ///
  /// **どの形も「その属性が効く」ように組んである。** 効かない形にすると
  /// 逆向きの点（札でない値で走りが変わる）が空振りする ——
  ///
  ///   speed      根の速さが 0 なので relative と absolute が同じ値になる。
  ///              先に 1 本 撃って sequence の latch を立てておく
  ///   horizontal 加速度が 0 から始まるので absolute と relative が同じ。
  ///              sequence だけが違う値になる
  ///   direction  aim と absolute が同じにならないよう、Env の aim を 0 にしない
  static let templates : ((string * string) * (string -> string)) list =
    [ ("bulletml", "type"),
        (fun attr ->
          sprintf "<bulletml%s xmlns=\"%s\"><action label=\"top\"><fire><bullet/></fire><wait>2</wait></action></bulletml>" attr Ns)

      ("direction", "type"),
        (fun attr ->
          wrap (sprintf "<action label=\"top\"><fire><direction%s>30</direction><speed>2</speed><bullet/></fire><wait>3</wait></action>" attr))

      ("speed", "type"),
        (fun attr ->
          wrap (sprintf "<action label=\"top\"><fire><speed>3</speed><bullet/></fire><fire><speed%s>2</speed><bullet/></fire><wait>3</wait></action>" attr))

      ("horizontal", "type"),
        (fun attr -> wrap (accelIn "horizontal" attr))

      ("vertical", "type"),
        (fun attr -> wrap (accelIn "vertical" attr)) ]

  /// 走らせて、外から見えるものを字にする。
  ///
  /// **`ShootingDirection` も混ぜる。** bulletml/@type は弾の動きに出ない ——
  /// フロントへ渡すだけの値なので、動きだけ見ていると常に一致して緑になる
  static let observe (src: string) =
    match tryReadXmlString src with
    | None -> failwithf "読めなかった: %s" src
    | Some bulletml ->
      let rand () = 0.5f
      let script = Runner.load rand 0.5f bulletml
      let env =
        { Rand = rand
          Rank = 0.5f
          Aim = { ToPlayer = 1.0f; ToEnemy = 1.0f }
          Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f } }
      let sb = System.Text.StringBuilder()
      sb.Append(sprintf "shooting=%A;" script.ShootingDirection) |> ignore
      let mutable runs = [ Runner.newRoot BulletType.Enemy script ]
      for frame in 1 .. 8 do
        let next = ResizeArray<BulletRun>()
        runs
        |> List.iteri (fun i run ->
            let f = Runner.stepWith env run run.Motion
            sb.Append(sprintf "%d/%d=%.4f,%.4f;" frame i (float f.Delta.X) (float f.Delta.Y)) |> ignore
            next.Add f.Run
            for c in f.Spawned do next.Add c)
        runs <- List.ofSeq next
      sb.ToString()

  static let attrText (name: string) (value: string) = sprintf " %s=\"%s\"" name value

  [<Test>]
  member _.``値の並びを持つ属性が在る``() =
    // reflection が効いていない印。ここが 0 だと下の点は全部 空振りする
    enumerated.Length |> should greaterThan 0

  [<Test>]
  member _.``並びを持つ属性の既定は ちょうど 1 個 で、その並びの中に在る``() =
    let bad =
      enumerated
      |> Array.choose (fun (el, a) ->
          if a.Defaults.Length <> 1 then
            Some(sprintf "%s/@%s の既定が %d 個" el a.Name a.Defaults.Length)
          elif not (Array.contains a.Defaults.[0] a.Values) then
            Some(sprintf "%s/@%s の既定 %s が並び %s に無い"
                   el a.Name a.Defaults.[0] (String.concat "|" a.Values))
          else None)
      |> Array.toList
    bad |> should be Empty

  [<Test>]
  member _.``自由記述の属性に既定は付いていない``() =
    let bad =
      [ for e in Vocabulary.elements do
          for a in e.Attrs do
            if a.Values.Length = 0 && a.Defaults.Length > 0 then
              yield sprintf "%s/@%s" e.Name a.Name ]
    bad |> should be Empty

  [<Test>]
  member _.``並びを持つ属性と、走らせる形が 過不足なく 一致する``() =
    let fromVocab = enumerated |> Array.map (fun (el, a) -> el, a.Name) |> Set.ofArray
    let fromTable = templates |> List.map fst |> Set.ofList
    // 語彙に在って形が無い（Core に並びが増えて書き忘れ）
    Set.difference fromVocab fromTable |> Set.toList |> should be Empty
    // 形が在って語彙に無い（Core から消えて形だけ残った）
    Set.difference fromTable fromVocab |> Set.toList |> should be Empty

  [<Test>]
  member _.``属性を省いた走りは、既定の値を書いた走りと同じ``() =
    let bad =
      [ for (el, a) in enumerated do
          match templates |> List.tryFind (fun (k, _) -> k = (el, a.Name)) with
          | Some(_, build) when a.Defaults.Length = 1 ->
            let omitted = observe (build "")
            let written = observe (build (attrText a.Name a.Defaults.[0]))
            if omitted <> written then
              yield sprintf "%s/@%s: 省いた走りが 既定 %s と違う" el a.Name a.Defaults.[0]
          | _ -> () ]
    bad |> should be Empty

  [<Test>]
  member _.``既定でない値を書くと 走りが変わる``() =
    // これが無いと上の一致は「その属性が何にも効いていない」でも緑になる
    let bad =
      [ for (el, a) in enumerated do
          match templates |> List.tryFind (fun (k, _) -> k = (el, a.Name)) with
          | Some(_, build) when a.Defaults.Length = 1 ->
            let omitted = observe (build "")
            let others = a.Values |> Array.filter (fun v -> v <> a.Defaults.[0])
            let differs =
              others |> Array.exists (fun v -> observe (build (attrText a.Name v)) <> omitted)
            if not differs then
              yield sprintf "%s/@%s: %s のどれを書いても走りが変わらない"
                      el a.Name (String.concat "|" others)
          | _ -> () ]
    bad |> should be Empty
