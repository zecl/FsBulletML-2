namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.LanguageService

/// 属性を書かなかったときに走る値が、Core のマーカーと合っているか。
/// 既定は AST に残らない。「省いた == 既定」だけなら緑のまま通る。
[<TestFixture>]
type AttributeDefaults() =

  [<Literal>]
  static let Ns = "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"

  /// 値の並びを持つ属性。表を書かない —— 語彙から拾う
  static let enumerated =
    [| for e in Vocabulary.elements do
         for a in e.Attrs do
           if a.Values.Length > 0 then yield e.Name, a |]

  static let wrap (body: string) =
    sprintf "<bulletml xmlns=\"%s\">%s</bulletml>" Ns body

  /// accel は撃った弾の中に置く。根の top に直に置くと 2 コマ の no-op になる。
  /// 根に置いたままだと、どの値を書いても走りが変わらない。
  static let accelIn (axis: string) (attr: string) =
    sprintf "<action label=\"top\"><fire><speed>1</speed><bullet><action><accel><%s%s>2</%s><term>4</term></accel><wait>8</wait></action></bullet></fire><wait>9</wait></action>"
      axis attr axis

  /// 属性のところだけ差し替えられる弾幕。渡すのは ` type=\"aim\"` のような字か、空。
  /// 効かない形にすると、札でない値で走りが変わる点が空振りする。
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
  /// bulletml/@type は弾の動きに出ない。動きだけ見ていると常に一致して緑になる。
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
