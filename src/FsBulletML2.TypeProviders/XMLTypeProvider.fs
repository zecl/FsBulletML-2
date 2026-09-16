// 他の 4 本 と同じ扱い。Impl / BulletMLTypeProvider が付けている
// [<CompilerMessage(... IsHidden = true)>] に触るので出る
#nowarn "13730"
namespace FsBulletML2.TypeProviders

open System.IO
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open FsBulletML2

[<TypeProvider>]
type BulletmlTypeProvider(config: TypeProviderConfig) as this =
  inherit TypeProviderForNamespaces(config, addDefaultProbingLocation = true)

  let asm = Assembly.GetExecutingAssembly()
  let ns = "FsBulletML2.TypeProviders"

  let typ = ProvidedTypeDefinition(asm, ns, "XML", Some (typeof<obj>), hideObjectMethods = true)
  do typ.DefineStaticParameters(
        [ProvidedStaticParameter("source", typeof<string>)],
        fun typeName parameters ->
          let xml = 
            let str = string parameters.[0]
            if not <| str.EndsWith(".xml") then
              str
            else
              let path = Path.Combine(config.ResolutionFolder, str)
              try
                File.ReadAllText(path)
              with
              | _ -> 
                  failwithf "Error xml path %A" path

          let typ = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>, hideObjectMethods = true)
          let ctor = ProvidedConstructor(parameters = [ ], 
                                          invokeCode = (fun args -> <@@ xml :> obj @@>))
          typ.AddMember ctor

          // ビルドに入っていなかったあいだに、読む側の API がここだけ古いまま
          // 残っていた（FsBulletML2.Xml.Bulletml.readXmlString に (xml, None) を
          // 渡す形）。いまは Bulletml.readXmlString が文字列 1 つ を受ける。
          // 隣の束縛（Bulletml(...) を組んで捨てるだけの bulletml2）も消した ——
          // 読まれないので、レコードにフィールドが増えても気づけなかった
          let bulletml = xml |> Bulletml.readXmlString
          let instanceProp = 
            ProvidedProperty(propertyName = "Value", 
                             propertyType = Impl.bulletmlType, 
                             // 値を quotation へ直に埋めない。 Bulletml は
                             // FsBulletML2.DTD で型とモジュールが同名なので、値を埋めると
                             // プロパティの型が FsBulletML2.DTD.Bulletml.Bulletml という
                             // 在りもしない名前で焼かれ、使う側が FS1109 で落ちる
                             // （型プロバイダ自身のビルドは通るので門を通すまで見えない）。
                             // Impl.read を呼ぶ形にすると、焼かれるのは呼び出しだけになる。
                             // 上の bulletml は「設計時に読めるか」を確かめる役目で残す
                             getterCode = (fun _ -> <@@ Impl.read Style.Xml xml @@>))
          instanceProp.AddXmlDoc(System.String.Format(@"BulletMLを取得します。"))

          typ.AddMember(instanceProp)

          typ
  )
  do 
    let myAssembly = Assembly.GetAssembly(this.GetType())
    let path = System.IO.Path.GetDirectoryName(myAssembly.Location)
    this.RegisterProbingFolder(path)
    this.AddNamespace(ns, [typ])

  override this.ResolveAssembly(args) = 
    let name = System.Reflection.AssemblyName(args.Name)
    let existingAssembly = 
      let hoge = System.AppDomain.CurrentDomain.GetAssemblies()
      hoge |> Seq.tryFind(fun a -> name = a.GetName())
    match existingAssembly with
    | Some a -> a
    | None -> base.ResolveAssembly(args)
  
