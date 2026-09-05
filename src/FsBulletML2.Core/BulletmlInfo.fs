namespace FsBulletML2

[<StructAttribute>]
type BulletmlInfo =
  val Name: string
  val Bulletml: Bulletml
  new(bulletml: Bulletml) = 
    { Name = match bulletml.Name with | Some x -> x | None -> ""
      Bulletml = bulletml }

  /// 新 API。走らせる材料を組む。
  ///
  /// 乱数とランクを受け取るのは、木を組む段が wait の term をその場で引くため
  /// （Runner.load の但し書き）。省くとグローバルから引くことになる。
  /// **旧 API の BulletmlTask() / BulletmlTaskOption() は落とした。**
  /// どちらも BulletRunner.convertBulletmlTask を呼ぶだけの包みで、
  /// 呼び手は 0 だった（型プロバイダも sln の中のサンプルも Script を使う）。
  member this.Script (rand: unit -> float32, rank: float32) =
    Runner.load rand rank this.Bulletml

[<AutoOpen>]
module BulletmlInfoModule =
  let createBulletmlInfo bulletml = new BulletmlInfo(bulletml)