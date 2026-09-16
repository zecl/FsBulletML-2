/// 型プロバイダで引いた弾幕を、エンジンに渡して走らせる。
///
/// 下の型宣言がコンパイルできた時点で「型が出ている」ことは分かる。
/// 出た型が走るかは別なので、main で回して、撃った数を弾幕の字と突き合わせる。
module FsBulletML2.Sample.TypeProviders.Debug.Program

open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.TypeProviders

// 入口 6 本 が出す 8 通り。ファイル名の綴りを間違えれば、ここで落ちる
type ViaStyleXml  = BulletML<"5way.xml">
type ViaStyleSxml = BulletML<"5way.sxml", Style.Sxml>
type ViaStyleFsb  = BulletML<"5way.fsb", Style.Fsb>
type ViaXml       = Xml.BulletML<"5way.xml;homing.xml;2wayRight.xml">
type ViaSxml      = Sxml.BulletML<"5way.sxml">
type ViaFsb       = Fsb.BulletML<"5way.fsb">
type ViaXmlSingle = XML<"5way.xml">
type ViaFsbSingle = SXML<"5way.fsb">

/// ファイルでなく弾幕そのものを渡す形。拡張子で終わらない字は中身と見なされる
[<Literal>]
let private Inline = """
2wayLeft.xml;
<bulletml type='horizontal'>
    <action label='top'><fire><bullet/></fire></action>
</bulletml>"""

type ViaInline = BulletML<Inline>

[<Literal>]
let private Frames = 60

/// 1 コマ 進める。座標を持つのはこちら側で、エンジンが返すのは差分。
///
/// 撃たれた弾はこのコマでは回さない（産まれた弾は次のコマから回る）。
let private stepAll (env: Env) (bullets: ResizeArray<BulletRun * Vec2>) =
  let next = ResizeArray<BulletRun * Vec2> ()
  let mutable born = 0
  for (bullet, pos) in bullets do
    let f = Runner.stepWith env bullet { bullet.Motion with Pos = pos }
    if not f.Vanished && not f.Retired then
      next.Add (f.Run, { X = pos.X + f.Delta.X; Y = pos.Y + f.Delta.Y })
    for child in f.Spawned do
      born <- born + 1
      next.Add (child, child.Motion.Pos)
  next, born

/// 走らせて、撃った数を返す
let private fired (bulletml: Bulletml) =
  let rnd = System.Random 42
  let rand () = float32 (rnd.NextDouble ())
  let rank = 0.5f

  // 読む段。弾幕 1 本 につき 1 回
  let script = Runner.load rand rank bulletml

  let bullets =
    ResizeArray<BulletRun * Vec2> [ Runner.newRoot BulletType.Enemy script, { X = 0.0f; Y = 0.0f } ]
  let mutable total = 0

  for _ in 1 .. Frames do
    // 自機を狙う角度はフロントが出す。ここは固定
    let env =
      { Rand = rand; Rank = rank
        Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }
        Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f } }
    let next, born = stepAll env bullets
    total <- total + born
    bullets.Clear ()
    bullets.AddRange next

  total

/// 撃った数は弾幕の字から数えられる。合わなければ 1 で落ちる ——
/// 「例外を投げずに 60 周 した」だけで緑にしないため
let private check name bulletml expected note =
  let actual = fired bulletml
  let ok = actual = expected
  printfn "%-10s 撃った %5d  弾幕から数えて %5d  %s   %s"
    name actual expected (if ok then "ok" else "NG") note
  ok

[<EntryPoint>]
let main _ =
  let b = ViaXml ()
  printfn "%d コマ 回す。撃たれた弾も回す（vanish が無い弾幕なので誰も消えない）" Frames

  let results =
    [ check "5way"      b.``5way``             778 "fire 1 + repeat 777"
      check "homing"    b.homing                22 "repeat 2 × (fire 1 + repeat 10)"
      check "2wayRight" b.``2wayRight``          2 "fire 2"
      check "inline"    (ViaInline ()).Bullet1   1 "fire 1" ]

  if List.forall id results then 0 else 1
