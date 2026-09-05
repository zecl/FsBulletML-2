module AssemblyInfo

open System.Resources
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

[<assembly: AssemblyVersion("0.1.0")>]
[<assembly: AssemblyFileVersion("0.1.0")>]
[<assembly: AssemblyInformationalVersion("0.1.0")>]

[<assembly: AssemblyTitle("FsBulletML2")>]
[<assembly: AssemblyDescription("FsBulletML2 front layer.")>]
[<assembly: AssemblyCompany("")>]
[<assembly: AssemblyProduct("FsBulletML2.Front")>]
[<assembly: AssemblyCopyright("Copyright (C) 2013-2014 zecl")>]
[<assembly: AssemblyTrademark("")>]
[<assembly: AssemblyCulture("")>]
[<assembly: NeutralResourcesLanguage("ja-JP")>]

[<assembly: ComVisible(false)>]
[<assembly: Guid("1FB3AC78-96BC-42BE-BDBF-B5DB7B0C8D0A")>]

// InternalsVisibleTo は置かない。Front.Tests は公開だけで書く決めごとで、
// internal が見えるとその検査（PublicSurface.fs）が効かなくなる。

#if DEBUG
[<assembly: AssemblyConfiguration("Debug")>]
#else
[<assembly: AssemblyConfiguration("Release")>]
#endif

do()
