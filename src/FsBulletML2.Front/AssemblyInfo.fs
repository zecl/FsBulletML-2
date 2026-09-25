module AssemblyInfo

open System.Resources
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

// 版・題・説明・会社・著作権は SDK が Directory.Build.props から作る。
// ここに書くと二重になって落ちる

[<assembly: NeutralResourcesLanguage("ja-JP")>]

[<assembly: ComVisible(false)>]
[<assembly: Guid("1FB3AC78-96BC-42BE-BDBF-B5DB7B0C8D0A")>]

// InternalsVisibleTo は置かない。Front.Tests は公開だけで書く決めごとで、
// internal が見えるとその検査（PublicSurface.fs）が効かなくなる。

do ()
