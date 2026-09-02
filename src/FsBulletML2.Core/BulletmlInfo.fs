namespace FsBulletML2
// 旧 API の Obsolete 警告を、**このファイルだけ**止める。
// BulletmlTask / BulletmlTaskOption は旧 API のシムそのもので、
// 中で旧を呼ぶのが仕事だから。
//
// プロジェクト単位（NoWarn）で止めない。効きがファイル単位であることは
// 較正済み —— nowarn を置いていない Api.fs で旧 API に触ると FS0044 が出る。
#nowarn "44"

[<StructAttribute>]
type BulletmlInfo =
  val Name: string
  val Bulletml: Bulletml
  new(bulletml: Bulletml) = 
    { Name = match bulletml.Name with | Some x -> x | None -> ""
      Bulletml = bulletml }

  /// 新 API。走らせる材料を組む。
  ///
  /// rootEnv を受け取るのは、木を組む段が wait の term をその場で引くため
  /// （Runner.load の但し書き）。省くとグローバルから引くことになる。
  member this.Script (rootEnv: Domain.Env) =
    Runner.load rootEnv this.Bulletml

  [<System.Obsolete("Script(rootEnv) を使ってください。")>]
  member this.BulletmlTask () =
    BulletRunner.convertBulletmlTask this.Bulletml

  [<System.Obsolete("Script(rootEnv) を使ってください。")>]
  member this.BulletmlTaskOption () =
    BulletRunner.convertBulletmlTask this.Bulletml |> Some

[<AutoOpen>]
module BulletmlInfoModule =
  let createBulletmlInfo bulletml = new BulletmlInfo(bulletml)