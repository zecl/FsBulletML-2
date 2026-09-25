module AssemblyInfo

open System.Resources
open System.Reflection
open System.Runtime.InteropServices
open Microsoft.FSharp.Core.CompilerServices

// 版・題・説明・会社・著作権は SDK が Directory.Build.props から作る。
// ここに書くと二重になって落ちる

[<assembly: NeutralResourcesLanguage("ja-JP")>]

[<assembly: ComVisible(false)>]
[<assembly: Guid("9A0CF746-36E8-4F2A-A3CD-E2B27C1E2E9D")>]

// 設計時 の dll を名前 で指す。引数 が空 だと、コンパイラ は参照した dll（NuGet なら lib/）を
// そのまま読み、typeproviders/fsharp41/<tfm>/ を探さない —— 依存 の Core / Parser / FParsec が
// 隣 に居ないので FS3033 になる。名前 を渡すと、そのフォルダ を先 に探し、無ければ隣 を読む
// （この repo の中 の試験 は隣 を読む）
[<assembly: TypeProviderAssembly("FsBulletML2.TypeProviders")>]
do ()
