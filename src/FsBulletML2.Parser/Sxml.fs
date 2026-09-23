namespace FsBulletML2

open System.IO 
open System.Text 
open FParsec.Primitives
open FParsec.CharParsers

module Sxml =

  type SxmlParser<'a> = Parser<'a, unit>
  type SxmlParser = Parser<XmlNode, unit>

  let chr c = skipChar c
  let skipSpaces1 : SxmlParser<unit> = skipMany (spaces1) <?> "no skip"
  let endBy p sep = many (p .>> sep)
  let pAst, pAstRef : SxmlParser * SxmlParser ref = createParserForwardedToRef()

  let parenOpen = skipSpaces1 >>. chr '('
  let parenClose = skipSpaces1 >>. chr ')'
  let parenOpenAt = skipSpaces1 >>. skipString "(@"
  let pChildOfElement = (sepEndBy pAst skipSpaces1)
  let betweenParen p = between parenOpen parenClose p
  let betweenParenAt p = between parenOpenAt parenClose p

  let pAttr = 
    let pFollowed = followedBy <| manyChars (noneOf "\"() \n\t") 
    let pLabel = manyChars asciiLetter 
    let pVal = 
      skipSpaces1 >>. chr '"' >>. 
      // `[` と `]` を通す（v1.4）。同梱カタログの 3 本がこの字をラベルに持つ。
      // 引用符で閉じた中なので、通しても曖昧にならない。
      (manyChars (asciiLetter <|> digit <|> noneOf "\"'|*`^><}{" <|> anyOf "()$+-*/.%:.~_" ))  
      .>> (skipSpaces1 >>. chr '"')
    skipSpaces1 .>>
    pFollowed >>. pLabel .>>. pVal

  let pAttrs = skipSpaces1 >>. sepEndBy (betweenParen pAttr) skipSpaces1 
  let pBody = skipSpaces1 >>. chr '\"' >>. manyChars (noneOf "\"") .>> chr '\"'  

  let pElement = 
      skipSpaces1 >>. (followedBy <| manyChars (noneOf "\" \t()\n")) >>.
      pipe4 (manyChars asciiLetter)
            (attempt (betweenParenAt pAttrs) <|>% [])
            (attempt pBody <|>% "")
            (pChildOfElement) 
            (fun name attrs body cdr -> cdr |> function
            | [] when body <> ""  -> Element(name, attrs, [PCData(body)])
            | [] -> Element(name, attrs, [])
            | cdr -> Element(name, attrs ,cdr)) 

  let ptop = parse {
      let! car = betweenParen pElement
      return car
  }

  do pAstRef := ptop

  [<CompiledName "Parse">]
  let parse input = runParserOnString pAst () "" input
  
  [<CompiledName "ParseFromFile">]
  let parseFromFile sxmlFile = 
    use sr = new StreamReader((sxmlFile:string), Encoding.GetEncoding("UTF-8"))
    let input = sr.ReadToEnd()
    parse input

  // --- 書く ----------------------------------------------------------------
  // 読む口と同じファイルに置く。書く側が別の proj に居ると文法の直しに置いていかれる。
  // 属性値に通せない字。上の `pVal` と対。片方を直したらこちらも直す。
  let private illegalInAttr = "\"'|`^><}{"

  /// 本文に通せない字。`pBody` は引用符以外 を通す
  let private illegalInBody = "\""

  /// S 式 の受け口。属性ブロックは最初の属性で開き、本文か子か閉じで閉じる。
  /// 通せない字は書かずに覚える。黙って落とすと読み直したときに値が変わる。
  type private SxmlSink() =
    let sb = StringBuilder()
    let bad = ResizeArray<string>()
    let mutable depth = 0
    let mutable attrsOpen = false

    let check (where: string) (illegal: string) (value: string) =
      let hit = value |> Seq.filter (fun c -> illegal.IndexOf c >= 0) |> Seq.distinct |> Seq.toList
      if not hit.IsEmpty then
        bad.Add(sprintf "%s に sxml で書けない字が在る（%s）: %s" where (System.String(List.toArray hit)) value)
      value

    let closeAttrs () =
      if attrsOpen then
        sb.Append ")" |> ignore
        attrsOpen <- false

    member _.Problems = List.ofSeq bad
    member _.Text = sb.ToString()

    interface IBulletmlSink with
      member _.Start name =
        closeAttrs ()
        if depth > 0 then sb.Append("\n").Append(String.replicate (depth * 2) " ") |> ignore
        sb.Append("(").Append(name) |> ignore
        depth <- depth + 1

      member _.Attr(name, value) =
        if not attrsOpen then
          sb.Append(" (@") |> ignore
          attrsOpen <- true
        sb.Append(" (").Append(name).Append(" \"").Append(check name illegalInAttr value).Append("\")") |> ignore

      member _.Text value =
        closeAttrs ()
        sb.Append(" \"").Append(check "本文" illegalInBody value).Append("\"") |> ignore

      member _.End() =
        closeAttrs ()
        depth <- depth - 1
        sb.Append(")") |> ignore

  /// 弾幕を S 式 の字にする。
  ///
  /// 書けないものは `Error`。 黙って落とすと、読み直したときに値が変わる
  [<CompiledName "Write">]
  let write (bulletml: Bulletml) : Result<string, string> =
    let sink = SxmlSink()
    BulletmlWriter.writeTo sink bulletml
    match sink.Problems with
    // `Ok` / `Error` は FParsec の `ReplyStatus` とぶつかる（このファイルは
    // `FParsec.Primitives` を開いている）。型名で修飾する
    | [] -> Result.Ok(sink.Text + "\n")
    | problems -> Result.Error(String.concat "\n" problems)
