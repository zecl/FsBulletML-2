namespace FsBulletML2.Parser.Tests

open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

/// 試験が言語モジュールへ渡す語彙。1 本。
/// 表記ごとに写すと、片方だけ古びる。落とし方はどの表記でも同じ。
[<AutoOpen>]
module VocabForTests =

    let vocab: Vocab =
        { // 雛形（v2.6）。host の並びをそのまま借りる ——
            // 試験のために別の表を作ると、そちらだけが古びる
            Frames = Frames.all
            Elements =
                Vocabulary.elements
                |> Array.toList
                |> List.map (fun e ->
                    {
                        Name = e.Name
                        Children = List.ofArray e.Children
                        Text = e.Text
                        Dtd = e.Dtd
                        Spec = e.Spec
                        Attrs =
                            e.Attrs
                            |> Array.toList
                            |> List.map (fun a ->
                                {
                                    Name = a.Name
                                    Values = List.ofArray a.Values
                                    Defaults = List.ofArray a.Defaults
                                    Dtd = a.Dtd
                                    Spec = a.Spec
                                    ValueSpecs = List.ofArray a.ValueSpecs
                                })
                    })
            Expressions = List.ofArray Vocabulary.expressions
            // F# の CE の名前。 本番も同じ表（`Spec.ce`）が JSON を通って届く
            Ce =
                Spec.ce
                |> List.map (fun (name, element, attr, value) ->
                    {
                        Name = name
                        Element = element
                        Attr = attr
                        Value = value
                    })
            // CE の名前が載せる label。上と別の表（あちらは「作る要素」）
            CeLabels =
                Spec.ceLabels
                |> List.map (fun (name, element, labelArg, fixedName, root) ->
                    {
                        Name = name
                        Element = element
                        LabelArg = labelArg
                        Fixed = fixedName
                        Root = root
                    })
            // どこに置けて、何を開くか。表ではなく `Dsl` から reflection で引く
            CePlaces =
                Vocabulary.cePlaces
                |> Array.toList
                |> List.map (fun (name, place, opens) ->
                    {
                        Name = name
                        In = place
                        Opens = opens
                    })
            // 根から走る定義の名前の頭。本番と同じ 1 本 を引く
            TopPrefix = Vocabulary.topPrefix
        }
