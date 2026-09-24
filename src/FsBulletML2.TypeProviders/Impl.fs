namespace FsBulletML2.TypeProviders

type Style =
    | Xml = 0
    | Sxml = 1
    | Fsb = 2

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

    /// 生成する型のプロパティの型。ここで 1 回だけ解いて配る。
    /// `typeof<Bulletml>` がモジュール側へ解けると FS1109 で焼かれる。
    let bulletmlType = typeof<Bulletml>

    let createProvidedTypeDefinition ns =
        ProvidedTypeDefinition(asm, ns, "BulletML", Some(typeof<obj>), hideObjectMethods = true, isErased = true)

    let paramSprit strArg =
        let separators = [| ";"; "," |]

        (strArg: string).Split(separators, StringSplitOptions.None)
        |> Array.map (fun x -> x.Trim())
        |> Array.toList

    let getExtention style =
        style
        |> function
            | Style.Xml -> "xml"
            | Style.Sxml -> "sxml"
            | Style.Fsb -> "fsb"
            | _ -> failwith "err"

    let getBulletmlInfo bullets style config watch ctx =
        bullets
        |> Seq.mapi (fun i str ->
            let extention = getExtention style

            if not <| (str: string).EndsWith("." + extention) then
                str, String.Format("Bullet{0}", i)
            else
                let resolvedFileName =
                    Helper.findConfigFile (config: TypeProviderConfig).ResolutionFolder str

                try
                    let str = File.ReadAllText(resolvedFileName)

                    if watch then
                        Helper.watchFile resolvedFileName ctx

                    str, Path.GetFileNameWithoutExtension(resolvedFileName)
                with _ ->
                    failwithf "Error %s path %A" extention resolvedFileName)

    let read style bulletml =
        match style with
        | Style.Xml -> bulletml |> Bulletml.readXmlString
        | Style.Sxml -> bulletml |> Bulletml.readSxmlString
        | Style.Fsb -> bulletml |> Bulletml.readFsbString
        | _ -> failwith "err"

    let addProperties typ bullets style config watch ctx =
        let bulletmlInfos = getBulletmlInfo bullets style config watch ctx
        bulletmlInfos |> Seq.iter (fun (bulletml, _) -> read style bulletml |> ignore)

        bulletmlInfos
        |> Seq.iter (fun (bulletml, propName) ->
            (typ: ProvidedTypeDefinition)
                .AddMemberDelayed(fun () ->
                    let instanceProp =
                        ProvidedProperty(
                            propertyName = propName,
                            propertyType = typeof<Bulletml>,
                            getterCode = (fun _ -> <@@ read style bulletml @@>)
                        )

                    instanceProp.AddXmlDocDelayed(fun () ->
                        let result = read style bulletml

                        let docText =
                            let defaultDoc = @"BulletML of internal DSL."

                            match result.Name with
                            | Some bulletName ->
                                sprintf
                                    "<summary><para>%s</para><para>BulletML's name is \"%s\".</para></summary>"
                                    defaultDoc
                                    bulletName
                            | None -> sprintf "<summary><para>%s</para></summary>" defaultDoc

                        docText)

                    instanceProp))
