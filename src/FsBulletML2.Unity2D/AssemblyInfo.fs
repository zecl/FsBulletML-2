module AssemblyInfo

open System.Resources
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

//[<assembly: AssemblyVersion("0.1.*")>]
[<assembly: AssemblyVersion("0.1.0")>]
[<assembly: AssemblyFileVersion("0.1.0")>]
[<assembly: AssemblyInformationalVersion("0.1.0")>]

[<assembly: AssemblyTitle("FsBulletML2")>]
[<assembly: AssemblyDescription("FsBulletML2 for Unity2D.")>]
[<assembly: AssemblyCompany("")>]
[<assembly: AssemblyProduct("FsBulletML2.Unity2D")>]
[<assembly: AssemblyCopyright("Copyright (C) 2014 zecl")>]
[<assembly: AssemblyTrademark("")>]
[<assembly: AssemblyCulture("")>]
[<assembly: NeutralResourcesLanguage("ja-JP")>]

[<assembly: ComVisible(false)>]
[<assembly: Guid("E3543808-0930-44D7-A8A0-7575C488C5E9")>]

#if DEBUG
[<assembly: InternalsVisibleTo("FsBulletML2.Unity2D.Tests")>]
[<assembly: InternalsVisibleTo("CreateBullets")>]
#endif

#if DEBUG
[<assembly: AssemblyConfiguration("Debug")>]
#else
[<assembly: AssemblyConfiguration("Release")>]
#endif

do ()
