namespace FsBulletML2

open System.IO 
open System.Text 
open FParsec

module Offside =

  type UserState = 
    { Current: int 
      Next: int 
      Depth: int list }
    with
      static member Create() = { Depth = []; Current = 0; Next = 0 }
      static member UpdateCurrent current self = { self with Current = current }
      static member UpdateNext next self = { self with Next = next }
      static member UpdateDepth depth self = { self with Depth = depth }

  let str s = pstring s
  let ws = manyChars (pchar ' ') 
  let pid = (fun _ -> Reply(()))
  let nextline = attempt (eof) <|> skipNewline >>. skipMany (regex "\s*$\n")
  let ptagName =  many1Chars (asciiLetter <|> digit)
  let pattrLabel = many1Chars (asciiLetter)
  let chr c = skipChar c
  let skipSpaces1 = skipMany (spaces1) <?> "no skip"
  let pattrValue = skipSpaces1 >>. chr '\"' >>. manyChars (noneOf "\"") .>> chr '\"'  
  let dprintPosition fmt = parse {
    let! p,_ = getPosition .>>. pid
    let dprintfn (fmt:Printf.StringFormat<_ -> _, unit>) = Printf.ksprintf System.Diagnostics.Debug.WriteLine fmt
    dprintfn fmt p
    return ()
  }

  /// attribute
  let pAttr = parse {
    let! label = pattrLabel
    do! (ws >>. str "=" >>. ws |>> ignore)
    let! value = pattrValue 
    return label,value} <?> "attribute error"

  /// attributes and body
  let pAttrsAndBody = parse{
      let pBodyValue = 
        chr '"' >>.
        manyChars (asciiLetter <|> digit <|> anyOf "()$+-*/.%" )
        .>> chr '"'
      let pSep = (str ":" >>. ws) 
      let pBody = attempt ((pSep >>. pBodyValue .>> ws) <|> str "")
      let pAttrsAndBody = (sepEndBy pAttr (many1Chars (pchar ' '))) .>>. pBody
      return! attempt (ws >>. pAttrsAndBody)
    }

  let dprintPointIndented = parse {
    let! state = getUserState
    let indent = ("".PadLeft(state.Current, ' '))
    let fmt  = Printf.StringFormat<_ -> _, unit>(indent + "%A")
    do! dprintPosition fmt
  }

  let pUpdateDepth = 
    attempt (parse {
      do! eof
      do! updateUserState (UserState.UpdateNext 0)
    }) <|>  parse {
      let! depth = ws |>> String.length
      do! (UserState.UpdateNext depth) |> updateUserState
    }

  let pSameDepth = parse {
    let! state = getUserState
    do! userStateSatisfies (fun state -> state.Next = state.Current)
    } 
  
  let pOpenIndent = parse {
    let! state = getUserState
    do! dprintPointIndented
    do! userStateSatisfies (fun state -> state.Current < state.Next)
    do! (UserState.UpdateDepth (state.Current :: state.Depth) >> UserState.UpdateCurrent (state.Next)) |> updateUserState
  }

  let pCloseIndent = parse {
    let! state = getUserState
    do! userStateSatisfies (fun state -> state.Next <= state.Current) 
    do! (UserState.UpdateDepth (List.tail state.Depth) >> UserState.UpdateCurrent (List.head state.Depth)) |> updateUserState
  }

  let pAst, pAstRef  = createParserForwardedToRef()

  let pChildren = opt <| parse {
    let! state = getUserState
    let! children = between pOpenIndent pCloseIndent (many pAst)
    return children } 

  let pElement = parse {
    do! skipMany <| pchar ' '
    let! name = ptagName
    let! attrs,body = pAttrsAndBody
    return name, attrs, body
  }

  pAstRef := parse {
    let! state = getUserState
    
    do! pSameDepth
    let! name, attrs, body = pElement
    do! nextline
    do! pUpdateDepth

    let! children = pChildren
    return children |> function
      | Some chiled -> Element(name, attrs, chiled)
      | None -> Element(name, attrs ,if body="" then [] else [PCData(body)])
  }

  [<CompiledName "Parse">]
  let parse input = runParserOnString pAst (UserState.Create()) "" input

  [<CompiledName "ParseFromFile">]
  let parseFromFile sxmlFile = 
    use sr = new StreamReader((sxmlFile:string), Encoding.GetEncoding("UTF-8"))
    let input = sr.ReadToEnd()
    parse input

  // --- 書く ----------------------------------------------------------------
  //
  // **読む口と同じファイルに置く。** 通せる字を決めているのは上の
  // `pBodyValue` / `pattrValue` / `ptagName` で、書く側が別のところに居ると、
  // 文法を直したときに置いていかれる —— **読めるものは読めるままなので気づかない。**

  /// 本文に通せる字。**上の `pBodyValue` と対。** 片方 を直したらこちらも直す
  let private bodyOk (c: char) =
    System.Char.IsLetterOrDigit c || "()$+-*/.%".IndexOf c >= 0

  /// インデント記法の受け口。
  ///
  ///     name attr="値"
  ///         子
  ///         name:"本文"
  ///
  /// **入れ子は行頭の空白だけ**（閉じる印が無いので、`End` は深さを戻すだけ）。
  ///
  /// **本文からは空白を落とす。** `pBodyValue` は空白を通さないので、
  /// XML の側が出す `90 * $rand` はそのままでは書けない。
  /// **落として語がくっつく形は、同梱カタログの本文 1,194 種類 で 0 件**
  /// （v1.4 の頭で測った）—— くっつけば意味が変わるので、門で見ている。
  type private FsbSink() =
    let sb = StringBuilder()
    let bad = ResizeArray<string>()
    let mutable depth = 0

    member _.Problems = List.ofSeq bad
    member _.Text = sb.ToString()

    interface IBulletmlSink with
      member _.Start name =
        if depth > 0 then sb.Append("\n") |> ignore
        sb.Append(String.replicate (depth * 4) " ").Append(name) |> ignore
        depth <- depth + 1

      member _.Attr(name, value) =
        if value.IndexOf '"' >= 0 then
          bad.Add(sprintf "%s の値に引用符が在る: %s" name value)
        sb.Append(" ").Append(name).Append("=\"").Append(value).Append("\"") |> ignore

      member _.Text value =
        let squashed = System.String(value.ToCharArray() |> Array.filter (fun c -> not (System.Char.IsWhiteSpace c)))
        if squashed = "" then
          // 空の本文を書くと、読み直したときに**子が 1 つ も無い形**になって消える
          bad.Add(sprintf "本文が空のものはインデント記法では書けない: %s" value)
        let ng = squashed |> Seq.filter (bodyOk >> not) |> Seq.distinct |> Seq.toList
        if not ng.IsEmpty then
          bad.Add(sprintf "本文にインデント記法で書けない字が在る（%s）: %s" (System.String(List.toArray ng)) value)
        sb.Append(":\"").Append(squashed).Append("\"") |> ignore

      member _.End() = depth <- depth - 1

  /// 弾幕をインデント記法の字にする。
  ///
  /// **書けないものは `Error`。** 黙って落とすと、読み直したときに値が変わる
  [<CompiledName "Write">]
  let write (bulletml: Bulletml) : Result<string, string> =
    let sink = FsbSink()
    BulletmlWriter.writeTo sink bulletml
    match sink.Problems with
    // **`Ok` / `Error` は FParsec の `ReplyStatus` とぶつかる。** 型名で修飾する
    | [] -> Result.Ok(sink.Text + "\n")
    | problems -> Result.Error(String.concat "\n" problems)
