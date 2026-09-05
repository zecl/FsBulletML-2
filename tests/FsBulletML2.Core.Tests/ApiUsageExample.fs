namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Domain

/// **Api.fs の Runner の但し書きに書いてある最小例を、そのまま動かす門。**
///
/// あの例はコメントなのでコンパイルされない。実際、段階 4 で Runner.load に
/// rootEnv が増えたとき、**例だけが古い形（引数 1 つ）のまま残っていた。**
/// 誰も呼ばないコードは、周りの API が動いても古びたまま座る。
///
/// ここに同じ形を置いておけば、次に API が動いたときはここが赤くなる。
/// **例を直したらここも、ここが赤くなったら例も。**
///
/// この門は「値が正しいか」ではなく「**その形で書けるか**」を見ている。
/// 軌跡の正しさは橋（Equivalence）の仕事。
///
/// **「他が緑のまま自分だけ赤くなる」形にはならない。** API の形が変われば、
/// ここも他の呼び出し側も一緒にコンパイルエラーになる。狙いは網ではなく
/// **同期の強制** —— 他を直すときにここも直すことになり、そのとき Api.fs の
/// 例も一緒に直る（ここと例は同じ形・同じ順で書いてある）。
/// 門を足すときに「何を捕まえるか」と同じくらい「何は捕まえないか」が要る。
[<TestFixture>]
type ApiUsageExample() =

  [<Literal>]
  let Xml = """<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <fire><direction type="absolute">0</direction><speed>2</speed><bullet/></fire>
    <wait>1</wait>
    <vanish/>
  </action>
</bulletml>"""

  /// 但し書きの例と同じ順・同じ呼び方で 3 コマ 回す
  [<Test>]
  member _.``但し書きの最小例が、その形のまま書けて動く``() =
    let rand () = 0.5f
    let rank = 0.5f

    // 読む段（弾幕 1 本 につき 1 回）。**Env は取らない** ——
    // この段が読むのは乱数とランクだけで、aim は撃つ弾ごとの位置が
    // まだ無いので読まれない
    let script = Runner.load rand rank (readXmlString Xml)
    let mutable run = Runner.newRoot BulletType.Enemy script

    let mutable myPos = { X = 0.0f; Y = 0.0f }
    let spawned = ResizeArray<BulletRun>()
    let mutable vanished = false

    // 毎コマ
    for _ in 1 .. 3 do
      let env =
        { Rand = rand; Rank = rank
          Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }
          Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f } }
      let f = Runner.stepWith env run { run.Motion with Pos = myPos }
      myPos <- { X = myPos.X + f.Delta.X; Y = myPos.Y + f.Delta.Y }
      run <- f.Run
      for child in f.Spawned do spawned.Add child
      if f.Vanished then vanished <- true

    // 台本のとおりに撃って、消えている。**回ったことの証拠**で、
    // これが無いと「例外を投げずに 3 周 した」だけで緑になる
    spawned.Count |> should equal 1
    vanished |> should equal true

  /// 撃たれた弾も同じ script で回せる（親から引き継ぐ形）
  [<Test>]
  member _.``撃たれた弾は、親と同じ script で次のコマから回せる``() =
    let rand () = 0.5f
    let rank = 0.5f
    let script = Runner.load rand rank (readXmlString Xml)
    let root = Runner.newRoot BulletType.Enemy script

    // aim を読まないと分かっているコマの Env
    let noAim =
      { Rand = rand; Rank = rank
        Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }
        Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f } }

    let f = Runner.stepWith noAim root root.Motion
    f.Spawned |> should not' (be Empty)

    // 撃たれた弾を 1 コマ 回す。**同じ script を渡す** —— 弾の中に残った
    // bulletRef / actionRef は、その script の入口でしか解けない
    let child = f.Spawned |> List.head
    let cf = Runner.stepWith noAim child child.Motion
    // 素の bullet なので台本を持たない。HasNoScript が立つ
    cf.Run.HasNoScript |> should equal true
