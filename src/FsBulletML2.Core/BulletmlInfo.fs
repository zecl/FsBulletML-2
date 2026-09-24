namespace FsBulletML2

[<StructAttribute>]
type BulletmlInfo =
    val Name: string
    val Bulletml: Bulletml

    new(bulletml: Bulletml) =
        {
            Name =
                match bulletml.Name with
                | Some x -> x
                | None -> ""
            Bulletml = bulletml
        }

    /// 走らせる材料を組む。乱数とランクを省くと、wait の term がグローバルから引かれる。
    member this.Script(rand: unit -> float32, rank: float32) = Runner.load rand rank this.Bulletml

[<AutoOpen>]
module BulletmlInfoModule =
    let createBulletmlInfo bulletml = new BulletmlInfo(bulletml)
