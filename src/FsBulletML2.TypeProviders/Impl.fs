namespace FsBulletML2.TypeProviders

type Style =
  | Xml  = 0
  | Sxml = 1
  | Fsb  = 2

open System
open System.IO
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open FsBulletML2

[<CompilerMessage("hidden...", 13730, IsError = false, IsHidden = true)>]
module Impl =
  let asm = Assembly.GetExecutingAssembly()
  let ns = typeof<Style>.Namespace

  /// 生成する型のプロパティの型。**ここで 1 回 だけ解いて配る。**
  ///
  /// FsBulletML2.DTD には同名の型とモジュールが居て（DU の Bulletml と、
  /// readXmlString などを持つ Bulletml モジュール）、書く場所によって
  /// typeof<Bulletml> がモジュールのほうへ解ける。そうなると生成した型の
  /// プロパティ型が FsBulletML2.DTD.Bulletml.Bulletml という在りもしない名前で
  /// 焼かれ、**使う側**が FS1109 で落ちる（型プロバイダ自身のビルドは通るので、
  /// 門を通すまで気づけない）。
  let bulletmlType = typeof<Bulletml>
  let createProvidedTypeDefinition ns =
    ProvidedTypeDefinition(asm, ns, "BulletML", Some (typeof<obj>), hideObjectMethods = true, isErased = true)

  let paramSprit strArg = 
    let separators = [|";";","|]
    (strArg:string).Split(separators, StringSplitOptions.None)
    |> Array.map (fun x -> x.Trim())
    |> Array.toList  

  let getExtention style = style |> function
    | Style.Xml  -> "xml"
    | Style.Sxml -> "sxml"
    | Style.Fsb  -> "fsb"
    | _ -> failwith "err"
  
  let getBulletmlInfo bullets style config watch ctx =
    bullets |> Seq.mapi (fun i str -> 
    let extention = getExtention style
    if not <| (str:string).EndsWith("." + extention) then
      str, String.Format("Bullet{0}",i)
    else
      let resolvedFileName = Helper.findConfigFile (config:TypeProviderConfig).ResolutionFolder str
      try
        let str = File.ReadAllText(resolvedFileName)
        if watch then
          Helper.watchFile resolvedFileName ctx
        str, Path.GetFileNameWithoutExtension(resolvedFileName)
      with | _ -> failwithf "Error %s path %A" extention resolvedFileName)

  let read style bulletml = 
    match style with
    | Style.Xml  -> bulletml |> Bulletml.readXmlString
    | Style.Sxml -> bulletml |> Bulletml.readSxmlString
    | Style.Fsb  -> bulletml |> Bulletml.readFsbString
    | _ -> failwith "err"

  let addProperties typ bullets style config watch ctx =
    let bulletmlInfos = getBulletmlInfo bullets style config watch ctx
    bulletmlInfos |> Seq.iter (fun (bulletml,_) -> read style bulletml |> ignore)
    bulletmlInfos |> Seq.iter (fun (bulletml,propName) -> 
      (typ:ProvidedTypeDefinition).AddMemberDelayed(fun () -> 
        let instanceProp =
          ProvidedProperty(
            propertyName = propName,
            propertyType = typeof<Bulletml>,
            getterCode = (fun _ -> <@@ read style bulletml @@>))

        instanceProp.AddXmlDocDelayed(fun () ->
          let result = read style bulletml
          let docText = 
            let defaultDoc = @"BulletML of internal DSL."
            match result.Name with
            | Some bulletName -> sprintf "<summary><para>%s</para><para>BulletML's name is \"%s\".</para></summary>" defaultDoc bulletName
            | None -> sprintf "<summary><para>%s</para></summary>" defaultDoc
          docText)
        instanceProp))

  // Was: probe for FsBulletML2.Core/FParsec/FsBulletML2.Parser under a NuGet packages.config-style
  // "packages\<id>.<version>\lib\<tf>" layout, keyed off "net40"/"net45" via #if NET40/NET45 (an
  // ifdef that nothing defines any more - a latent, always-broken build target under this
  // project's later configurations). There is no packages.config in this repo; dependencies are
  // resolved through ProjectReference/PackageReference instead.
  //
  // What used to follow this paragraph claimed the build "already copies FsBulletML2.Core.dll,
  // FsBulletML2.Parser.dll and FParsec.dll next to FsBulletML2.TypeProviders.dll", and concluded
  // that registering a probing folder was redundant. **Both halves were wrong.** A library
  // project does not copy NuGet assemblies to its output by default, so FParsec.dll was never
  // there; and a type provider runs inside the compiler at design time, where nothing consults
  // this assembly's deps.json. Once the SDK swap made provided types resolve at all, every one
  // of them failed with "Could not load file or assembly 'FParsec'".
  //
  // The fix is in two places, and neither half works alone:
  //   FsBulletML2.TypeProviders.fsproj   CopyLocalLockFileAssemblies puts FParsec.dll there
  //   TypeProviderForNamespaces(...)     addDefaultProbingLocation makes the provider look there
  //
  // The claim was plausible, sat in a comment nothing executes, and stayed wrong until the
  // surrounding code was made to work well enough to reach it.