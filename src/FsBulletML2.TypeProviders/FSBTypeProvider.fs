// 他の 4 本 と同じ扱い。Impl / BulletMLTypeProvider が付けている
// [<CompilerMessage(... IsHidden = true)>] に触るので出る
#nowarn "13730"
namespace FsBulletML2.TypeProviders

open System.IO
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes

[<TypeProvider>]
type FSBTypeProvider(config: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces(config, addDefaultProbingLocation = true)

    let asm = Assembly.GetExecutingAssembly()
    let ns = "FsBulletML2.TypeProviders"

    let typ =
        ProvidedTypeDefinition(asm, ns, "SXML", Some(typeof<obj>), hideObjectMethods = true)

    do
        typ.DefineStaticParameters(
            [ ProvidedStaticParameter("source", typeof<string>) ],
            fun typeName parameters ->
                let source =
                    let str = string parameters.[0]

                    if not <| str.EndsWith(".fsb") then
                        str
                    else
                        let path = Path.Combine(config.ResolutionFolder, str)

                        try
                            File.ReadAllText(path)
                        with _ ->
                            failwithf "Error fsb path %A" path

                let typ =
                    ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>, hideObjectMethods = true)

                let ctor =
                    ProvidedConstructor(parameters = [], invokeCode = (fun args -> <@@ source :> obj @@>))

                typ.AddMember ctor

                typ.AddMemberDelayed(fun () ->
                    let instanceProp =
                        ProvidedProperty(
                            propertyName = "Value",
                            propertyType = Impl.bulletmlType,
                            // XMLTypeProvider.fs と同じ理由で Impl.read を通す
                            getterCode = (fun _ -> <@@ Impl.read Style.Fsb source @@>)
                        )

                    instanceProp.AddXmlDocDelayed(fun () -> System.String.Format(@"BulletMLを取得します。"))
                    instanceProp)

                typ
        )

    do
        let myAssembly = Assembly.GetAssembly(this.GetType())
        let path = System.IO.Path.GetDirectoryName(myAssembly.Location)
        this.RegisterProbingFolder(path)
        this.AddNamespace(ns, [ typ ])

    override this.ResolveAssembly(args) =
        let name = System.Reflection.AssemblyName(args.Name)

        let existingAssembly =
            let loaded = System.AppDomain.CurrentDomain.GetAssemblies()
            loaded |> Seq.tryFind (fun a -> name = a.GetName())

        match existingAssembly with
        | Some a -> a
        | None -> base.ResolveAssembly(args)
