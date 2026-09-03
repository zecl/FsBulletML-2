namespace FsBulletML2
open System
open System.Globalization
// DTD は AutoOpen だが、open System が先に来るので Action が System.Action に
// 取られる。DU のほうを指すよう明示で開き直す
open FsBulletML2.DTD
open FsBulletML2.Processable

/// **XML を読んで公開の木にし、走らせる木（Rec*）へ写すところまで。**
/// Rec* の上の操作は RecOps.fs へ切り出した。
///
/// 二重木（公開の Bulletml と、エンジンが歩く Rec*）を畳むときは、
/// この地図を先に読むこと ——「畳む」が何を消して何を残すのかが、
/// 役目ごとに違う。
///
/// 区切りは関数名で書いてある。**行番号を書くとこのコメント自身でずれる。**
///
///     役目                          先頭 〜 末尾の関数              畳むと
///     ---------------------------- ------------------------------ ----------
///     1. XML を読んで公開の木にする  existsAttribute 〜             残る
///                                   tryBulletmlFromXmlNode         （読み取り本体）
///     2. 公開 → Rec*（定数を畳む）   convertDirectionOption 〜      **消える**
///                                   convertRecBulletmlForTest      （約 125 行）
///     （RecOps.fs）Rec* の上の操作   collect 〜 expandActionRefOnceRec 残る
///
/// **畳んで消えるのは 2 ＝ 約 125 行だけ。** 残りは木が 1 つ になっても要る。
///
/// **役目の分けかたと、実装の依存は一致していない。** 役目 2 の小さい関数
/// （convertDirection / convertTerm / convertParam など）は、RecOps の
/// 「param を差し込む」からも呼ばれる。切り出した側が
/// `open FsBulletML2.IntermediateParser` しているのはそのため。
/// **畳むときは「役目 2 が丸ごと消える」ではなく、この共有部分がどちらに
/// 残るかを先に決めること。**
///
/// **かつて「畳むと消える」に数えていた 2 つ は、畳むより先に死んでいた。**
/// どちらも定義以外に呼び出しが無く、internal なので外からも呼べなかった。
///
///     Rec* → 公開へ戻す   commandToPublic / actionElmToPublic /
///                        bulletElmToPublic / topElmToPublic /
///                        convertBulletml                        45 行
///     平ら ⇔ 位置の糊     bulletElmToBulletml 〜
///                        bulletmlToBulletmlElm                   62 行
///
/// 戻す向きが死んでいたのは、XML を書く経路が公開 → Rec のほうを通っていた
/// から。糊のほうは、公開の木を位置ごとの型へ入れ直す必要が無くなった時点で
/// 使われなくなっていた。**「畳めば消える」と書いてあるものが、実は
/// もう誰にも呼ばれていないことがある。** 畳む前に呼び出しを数えること。
///
/// ## ロード時と実行時の境界
///
/// ここが計画書の言う「先に固定する」もの。**いまは既に分かれている。**
///
///     ロード時（Runner.load で 1 回）
///       XML → 公開の木 → Rec*（定数を畳む）
///       Resolvers を組む（expandBulletRefOnceRec / expandActionRefOnceRec を
///       部分適用しただけの、まだ何も解いていない関数 2 本）
///
///     実行時（Step.action / Step.fire が踏むたび）
///       Resolvers を呼んで**輪を 1 段 だけ**解く
///
/// **1 段 なのは輪があるから。** 自己参照する actionRef は解いた先にまた
/// 同じ actionRef が現れるので、ロード時に解き切ろうとすると止まらない。
/// Step.action の走査がそこで Stop する形と対になっている。
///
/// **畳んでもこの境界は動かない。** 動かすなら別の話（そちらのほうが
/// 指紋を動かす）。
///
/// ## 「読めなかった」は値ではなく戻り値の型で言う
///
/// かつて公開の Bulletml には NotCommand という腕があり、**読めなかったを
/// 値で返していた**。tryBulletmlFromXmlNode は例外だけを option に畳んで
/// いたので、読めなかったときも None ではなく Some NotCommand が返り、
/// 「成功したが中身が無い」と見分けが付かなかった。
///
/// いまはこう:
///
///     (|Command|)                  命令でない節（direction / speed / …）は None
///                                  —— これは**捨てる印**で、エラーではない
///     convertBulletmlFromXmlNode   読めなければ BulletmlDTDViolationException
///     tryBulletmlFromXmlNode       読めなければ None
///
/// **1 つ 上の階（readXmlString / readSxmlString / readFsb …）が、もう
/// この形で書かれていた** —— read は失敗で上げ、tryRead は None を返す。
/// 型が意図を宣言していたので、それに揃えただけ。
///
/// 押さえは tests/FsBulletML2.Core.Tests/PublicParseBoundary.fs。
module IntermediateParser =
  let internal existsAttribute attrs f = attrs |> List.exists (fun (label, v) -> if f label v then true else false)
  let internal tryFindPCData children =  children |> List.tryPick (function | PCData x -> Some x | _ -> None)

  let internal getElement children parentElementName elementName factory =
    let termXml = children |> List.tryFind (fun xml -> 
      match xml with
      | Element(name, attrs, _) -> 
        if name.ToLower() = elementName then 
          if existsAttribute attrs (fun _ _ -> true) then
            new BulletmlDTDViolationException (sprintf "this element has no attributes.:[%s]" elementName) |> raise
          true 
        else false 
      | _ -> false) 

    let pcdata = function
      | Element(_, _, children) -> tryFindPCData children 
      | _ -> None
          
    match termXml with
    | Some term ->
      match pcdata term with
      | Some text -> factory(text)
      | None -> new BulletmlDTDViolationException(sprintf "[%s] element should have #PCDATA." elementName) |> raise
    | None -> new BulletmlDTDViolationException(sprintf "[%s] element should have [%s] element." parentElementName elementName) |> raise

  let internal createTerm children parentElementName = getElement children parentElementName "term" (numExpr >> Term)
  let internal createTimes children parentElementName = getElement children parentElementName "times" (numExpr >> Times)

  let internal getParam xml =
    match xml with
    | PCData x -> [] 
    | Element(_, _, children) -> 
      let rec f x = 
        match x with
        | PCData x -> x
        | Element(elementName, attrs, children) -> 
          if elementName = "param" then 
            if existsAttribute attrs (fun _ _ -> true) then
               new BulletmlDTDViolationException(sprintf "this element has no attributes.:[%s]" elementName) |> raise
            else     
              if children |> List.length = 0 then
                new BulletmlDTDViolationException(sprintf "[%s] element should have #PCDATA." elementName) |> raise
              f (children.[0]) 
          else 
            new BulletmlDTDViolationException(sprintf "not support element.:[%s]" elementName) |> raise
      List.map f children
   
  let internal getTextDefault children defaultText = 
    children |> List.tryPick (function | PCData x -> Some x | _ -> None)
    |> function | Some x -> x | _ -> defaultText

  let internal tryFindAttrValue attrs attrName = attrs |> List.tryPick (fun (label, v) -> if label = attrName && v <> "" then Some v else None)
  let internal getAttrValue attrs attrName = attrs |> List.pick (fun (label, v) -> if label = attrName && v <> "" then Some v else None)
  let internal tryFindLabelValue attrs = tryFindAttrValue attrs "label" 
  let internal getLabelValue attrs = getAttrValue attrs "label" 

  let internal toSpeedType (s:string) = 
    match s.ToLower () with
    | "absolute"  -> SpeedType.Absolute   
    | "relative"   -> SpeedType.Relative 
    | "sequence" -> SpeedType.Sequence 
    | x -> new BulletmlDTDViolationException(sprintf "not support SpeedType.:[%s]" x) |> raise 

  let internal tryFindDirection (children:XmlNode list) = 
    let f = function 
      | Element(elementName, attrs, children) -> 
        match elementName.ToLower() with
        | "direction" ->
          let attr = attrs |> List.tryPick (fun (_, x) -> if x <> "" then Some x else None)
          match attr with
          | Some attr -> 
            let toDirectionType (s:string) = 
              match s.ToLower () with
              | "aim"      -> DirectionType.Aim 
              | "absolute" -> DirectionType.Absolute  
              | "relative" -> DirectionType.Relative 
              | "sequence" -> DirectionType.Sequence 
              | x -> new BulletmlDTDViolationException(sprintf "not support DirectionType.:[%s]" x) |> raise 
            let attr = { DirectionAttrs.directionType = attr |> toDirectionType }
            match tryFindPCData children with
            | Some text -> Direction(Some attr, numExpr text) |> Some
            | None -> new BulletmlDTDViolationException(sprintf "[%s] element should have #PCDATA." elementName) |> raise
          | None -> 
            match tryFindPCData children with
            | Some text -> Direction(None, numExpr text) |> Some
            | None -> new BulletmlDTDViolationException(sprintf "[%s] element should have #PCDATA." elementName) |> raise
        | _ -> None
      | _ -> new BulletmlDTDViolationException("not support element.") |> raise
    children |> List.tryPick f

  let internal tryFindSpeed (children:XmlNode list) = 
    let f = function 
      | Element(elementName, attrs, children) -> 
        match elementName.ToLower() with
        | "speed" ->
          let attr = attrs |> List.tryPick (fun (_,x) -> if x <> "" then Some x else None)
          match attr with
          | Some attr -> 
            let attr = { SpeedAttrs.speedType = attr |> toSpeedType }
            match tryFindPCData children with
            | Some text -> Speed(Some attr, numExpr text) |> Some
            | None -> new BulletmlDTDViolationException(sprintf "[%s] element should have #PCDATA." elementName) |> raise
          | None -> 
            match tryFindPCData children with
            | Some text -> Speed(None, numExpr text) |> Some
            | None -> new BulletmlDTDViolationException(sprintf "[%s] element should have #PCDATA." elementName) |> raise
        | _ -> None
      | _ -> new BulletmlDTDViolationException("not support element.") |> raise
    children |> List.tryPick f

  let internal tryFindHorizontal (children:XmlNode list) = 
    let f = function 
      | Element(elementName, attrs, children) -> 
        match elementName.ToLower() with
        | "horizontal" ->
          let attr = attrs |> List.tryPick (fun (_,x) -> if x <> "" then Some x else None)
          match attr with
          | Some attr -> 
            let toHorizontalType (s:string) = 
              match s.ToLower () with
              | "absolute" -> HorizontalType.Absolute   
              | "relative" -> HorizontalType.Relative  
              | "sequence" -> HorizontalType.Sequence 
              | x -> new BulletmlDTDViolationException(sprintf "not support HorizontalType.:[%s]" x) |> raise 
            let attr = { HorizontalAttrs.horizontalType = attr |> toHorizontalType }
            match tryFindPCData children with
            | Some text -> 
              Horizontal(Some attr, numExpr text) |> Some
            | None -> new BulletmlDTDViolationException(sprintf "[%s] element should have #PCDATA." elementName) |> raise   
          | None -> 
            match tryFindPCData children with
            | Some text -> 
              Horizontal(None, numExpr text) |> Some
            | None -> new BulletmlDTDViolationException(sprintf "[%s] element should have #PCDATA." elementName) |> raise   
        | _ -> None
      | _ -> new BulletmlDTDViolationException("not support element.") |> raise
    children |> List.tryPick f

  let internal tryFindVertical (children:XmlNode list) = 
    let f = function 
      | Element(elementName, attrs, children) -> 
        match elementName.ToLower() with 
        | "vertical" ->
          let attr = attrs |> List.tryPick (fun (_, x) -> if x <> "" then Some x else None)
          match attr with
          | Some attr -> 
            let toVerticalType (s:string) = 
              match s.ToLower () with
              | "absolute" -> VerticalType.Absolute   
              | "relative" -> VerticalType.Relative  
              | "sequence" -> VerticalType.Sequence 
              | x -> new BulletmlDTDViolationException(sprintf "not support VerticalType.:[%s]" x) |> raise 
            let attr = { VerticalAttrs.verticalType = attr |> toVerticalType }
            match tryFindPCData children with
            | Some text -> 
              Vertical(Some attr, numExpr text) |> Some
            | None -> new BulletmlDTDViolationException(sprintf "[%s] element should have #PCDATA." elementName) |> raise   
          | None ->
            match tryFindPCData children with
            | Some text -> 
              Vertical(None, numExpr text) |> Some
            | None -> new BulletmlDTDViolationException(sprintf "[%s] element should have #PCDATA." elementName) |> raise   
        | _ -> None
      | _ -> new BulletmlDTDViolationException("not support element.") |> raise
    children |> List.tryPick f

  /// XmlNode to Bulletml.Bulletml
  ///
  /// DTD :
  /// <!ELEMENT bulletml (bullet | fire | action)*>
  /// <!ATTLIST bulletml xmlns CDATA #IMPLIED>
  /// <!ATTLIST bulletml type (none|vertical|horizontal) "none">
  let internal createBulletml xml readTopElms =
    match xml with
    | Element(name, attrs, children) ->
      let tryFindBulletmlAttrs = maybe {
        let toShootingDirection (s:string) = 
          match s.ToLower () with
          | "none"       -> ShootingDirection.BulletNone 
          | "vertical"   -> ShootingDirection.BulletVertical 
          | "horizontal" -> ShootingDirection.BulletHorizontal 
          | x -> new BulletmlDTDViolationException(sprintf "not support ShootingDirection.：[%s]" x) |> raise
        let xmlns = tryFindAttrValue attrs "xmlns"
        let name = tryFindAttrValue attrs "name"
        // description は BulletML公式の属性ではない。BulletMLの名前/説明文を格納するための属性として追加した。
        let description = tryFindAttrValue attrs "description"
        match tryFindAttrValue attrs "type" with
        | Some shootingDirection ->
          return { bulletmlXmlns = xmlns; bulletmlType = shootingDirection |> toShootingDirection |> Some; bulletmlName = name; bulletmlDescription = description }
        | None ->
          return { bulletmlXmlns = xmlns; bulletmlType = None; bulletmlName = name; bulletmlDescription = description }}

      match tryFindBulletmlAttrs with
      | Some attrs ->
        // 以前はここで、平らな Bulletml で受けた子を filter して BulletmlElm へ
        // 入れ直していた。入れ直しの `| _ -> raise "convert error"` は
        // **型が防げるはずの検査**で、読む段が位置の型で返せば要らない
        Bulletml.Bulletml (attrs, readTopElms children)
      // 条件は type 属性の有無ではなく、attrs レコードそのものが取れなかったとき。
      // 上の maybe には let! が 1 つも無いので必ず return に着き、いまは届かない
      | None -> new BulletmlDTDViolationException("bulletml element attributes could not be read.") |> raise
    | _ -> new BulletmlDTDViolationException("not support element.") |> raise

  /// XmlNode to Bulletml.Action
  ///
  /// DTD :
  /// <!ELEMENT action (changeDirection | accel | vanish | changeSpeed | repeat | wait | (fire | fireRef) | (action | actionRef))*>
  /// <!ATTLIST action label CDATA #IMPLIED>
  let internal createAction factory xml readCommands =
    match xml with
    | Element(name, attrs, children) ->
      let attrs = { actionLabel = tryFindLabelValue attrs |> Option.map ActionLabel }
      // 命令でない子（bullet / bulletRef / direction …）は readCommands が
      // 黙って落とす。**以前ここに 10 腕 の filter と 10 腕 の map が
      // 並んでいたぶん。落とし方は変えていない**
      factory(attrs, readCommands children)
    | _ -> new BulletmlDTDViolationException("not support element.") |> raise

  /// XmlNode to Bulletml.ActionRef
  ///
  /// DTD :
  /// <!ELEMENT actionRef (param* )>
  /// <!ATTLIST actionRef label CDATA #REQUIRED>
  let internal createActionRef factory xml = 
    match xml with
    | Element(_ , attrs, _) ->
      let tryFindActionRefAtts = maybe {
        let! label = tryFindLabelValue attrs
        return { actionRefLabel = ActionLabel label } }

      match tryFindActionRefAtts with
      | Some attrs -> factory(attrs, getParam xml)
      | _ -> new BulletmlDTDViolationException("ActionRef element should have label attribute.") |> raise 
    | _ -> new BulletmlDTDViolationException("not support element.") |> raise 

  let internal tryFindActionOrActionRef (children:XmlNode list) readCommands =
    let f xml =
      match xml with
      | Element(name, attrs, children) ->
        match name.ToLower() with
        | "action" -> createAction (ActionElm.Action) xml readCommands |> Some
        | "actionref" -> createActionRef (ActionElm.ActionRef) xml |> Some
        | _ -> None
      | _ -> new BulletmlDTDViolationException("not support element.") |> raise

    let result = children |> List.filter (function
                                          | Element(name, attrs, children) -> 
                                            match name.ToLower() with
                                            | "action"  | "actionref" -> true
                                            | _ -> false
                                          | _ -> false)

    if result |> List.length > 1 then
      new BulletmlDTDViolationException("repeat element cannot have multiple elements of (Action|ActionRef).") |> raise
    elif result |> List.length = 0 then
      new BulletmlDTDViolationException("repeat element should have Action or ActionRef.") |> raise
    result.[0] |> f

  /// XmlNode to Bulletml.Bullet
  ///
  /// DTD :
  /// <!ELEMENT bullet (direction?, speed?, (action | actionRef)* )>
  /// <!ATTLIST bullet label CDATA #IMPLIED>
  let internal createBullet factory xml readActionElms =
    match xml with
    | Element(_ , attrs, children) ->
      let attr = { bulletLabel = tryFindLabelValue attrs |> Option.map BulletLabel }
      // action / actionRef 以外は readActionElms が黙って落とす（getActions と同じ）
      factory(attr, tryFindDirection children, tryFindSpeed children, readActionElms children)
    | _ -> new BulletmlDTDViolationException("not support element.") |> raise

  /// XmlNode to Bulletml.BulletRef
  ///
  /// DTD :
  /// <!ELEMENT bulletRef (param* )>
  /// <!ATTLIST bulletRef label CDATA #REQUIRED>
  let internal createBulletRef factory xml = 
    match xml with
    | Element(_ , attrs, _) ->
      let tryFindBulletRefAttrs = maybe {
        let! label = tryFindLabelValue attrs
        let bulletRefAttrs = { bulletRefLabel = BulletLabel label }
        return bulletRefAttrs
        }
      match tryFindBulletRefAttrs with
      | Some attrs ->
        factory(attrs, getParam xml)
      | None -> new BulletmlDTDViolationException("BulletRef element should have label attribute.") |> raise 
    | _ -> new BulletmlDTDViolationException("not support element.") |> raise

  let internal tryFindBulletOrBulletRef (children:XmlNode list) readActionElms =
    let f xml =
      match xml with
      | Element(name, attrs, children) ->
        match name.ToLower()  with
        | "bullet"    -> createBullet (BulletElm.Bullet) xml readActionElms |> Some
        | "bulletref" -> createBulletRef (BulletElm.BulletRef) xml  |> Some
        | _ -> None
      | _ -> new BulletmlDTDViolationException("not support element.") |> raise
    children |> List.tryPick f

  /// XmlNode to Bulletml.Fire
  ///
  /// DTD :
  /// <!ELEMENT fire (direction?, speed?, (bullet | bulletRef))>
  /// <!ATTLIST fire label CDATA #IMPLIED>
  /// fire は action の子にも bulletml の子にもなれる。**位置が違えば型が違う**
  /// ので、どちらの腕を作るかは呼び側が factory で渡す
  let internal createFire factory xml readActionElms =
    match xml with
    | Element(_ , attrs, children) ->
      let fireattrs = { FireAttrs.fireLabel = tryFindLabelValue attrs |> Option.map FireLabel }
      match tryFindBulletOrBulletRef children readActionElms with
      | Some bullet ->
        factory(fireattrs, tryFindDirection children, tryFindSpeed children, bullet)
      | None -> new BulletmlDTDViolationException("Fire element should have Bullet or BulletRef element.") |> raise
    | _ -> new BulletmlDTDViolationException ("not support element.") |> raise

  /// XmlNode to Bulletml.FireRef
  ///
  /// DTD :
  /// <!ELEMENT fireRef (param* )>
  /// <!ATTLIST fireRef label CDATA #REQUIRED>
  let internal createFireRef xml = 
    match xml with
    | Element(_ , attrs, _) ->
      let tryFindFireAttrs = maybe {
        let! label = tryFindLabelValue attrs
        return { fireRefLabel = FireLabel label } }
      
      match tryFindFireAttrs with
      | Some attrs ->
        Action.FireRef(attrs, getParam xml)
      | _ -> new BulletmlDTDViolationException("FireRef element should have label attribute.") |> raise 
    | _ -> new BulletmlDTDViolationException("not support element.") |> raise 

  /// XmlNode to Bulletml.Accel
  ///
  /// DTD :
  /// <!ELEMENT accel (horizontal?, vertical?, term)>  
  let internal createAccel = function
    | Element(elementName , attrs, children) ->
      match existsAttribute attrs (fun _ _ -> true) with
      | true ->
        new BulletmlDTDViolationException (sprintf "this element has no attributes.:[%s]" elementName) |> raise
      | false -> 
        let horizontal = tryFindHorizontal children
        let vertical = tryFindVertical children
        Action.Accel(horizontal, vertical , createTerm children "accel")
    | _ -> new BulletmlDTDViolationException ("not support element.") |> raise

  /// XmlNode to ChangeSpeed
  ///
  /// DTD :
  /// <!ELEMENT changeSpeed (speed, term)>
  let internal createChangeSpeed = function
    | Element(elementName , attrs, children) ->
      match existsAttribute attrs (fun _ _ -> true) with
      | true ->
        new BulletmlDTDViolationException (sprintf "this element has no attributes.:[%s]" elementName) |> raise
      | false -> 
        let speed = 
          match tryFindSpeed children with
          | Some speed -> speed
          | None -> new BulletmlDTDViolationException(sprintf "this element should have Speed element.:[%s]" elementName) |> raise

        Action.ChangeSpeed(speed, createTerm children "changeSpeed")
    | _ -> new BulletmlDTDViolationException ("not support element.") |> raise

  /// XmlNode to ChangeDirection
  ///
  /// DTD :
  /// <!ELEMENT changeDirection (direction, term)>
  let internal createChangeDirection = function
    | Element(elementName , attrs, children) ->
      match existsAttribute attrs (fun _ _ -> true) with
      | true ->
        new BulletmlDTDViolationException (sprintf "this element has no attributes.:[%s]" elementName) |> raise
      | false -> 
        let direction = 
          match tryFindDirection children with
          | Some direction -> direction
          | None -> new BulletmlDTDViolationException(sprintf "this element should have Direction element.:[%s]" elementName) |> raise
        Action.ChangeDirection(direction, createTerm children "changeDirection")
    | _ -> new BulletmlDTDViolationException ("not support element.") |> raise

  /// XmlNode to Bulletml.Wait
  ///
  /// DTD :
  /// <!ELEMENT wait (#PCDATA)>
  let internal createWait = function 
    | Element(elementName, attrs, children) ->
      match existsAttribute attrs (fun _ _ -> true) with
      | true ->
        new BulletmlDTDViolationException (sprintf "this element has no attributes.:[%s]" elementName) |> raise
      | false -> 
        match tryFindPCData children with
        | Some text -> Action.Wait(numExpr text)
        | None -> new BulletmlDTDViolationException (sprintf "[%s] element should have #PCDATA." elementName) |> raise
    | _ -> new BulletmlDTDViolationException ("not support element.") |> raise 

  /// XmlNode to Bulletml.Vanish
  ///
  /// DTD :
  /// <!ELEMENT vanish (#PCDATA)>
  let internal createVanish = function 
    | Element (elementName, attrs, children) -> 
      match existsAttribute attrs (fun _ _ -> true) with
      | true ->
        new BulletmlDTDViolationException (sprintf "this element has no attributes.:[%s]" elementName) |> raise
      | false -> 
        match tryFindPCData children with
        | Some text -> new BulletmlDTDViolationException (sprintf "this element cannot have #PCDATA.:[%s]" elementName) |> raise
        | None -> Action.Vanish
    | _ -> new BulletmlDTDViolationException ("not support element.") |> raise 

  /// XmlNode to Bulletml.Repeat
  ///
  /// DTD :
  /// <!ELEMENT repeat (times, (action | actionRef))>
  let internal createRepeat xml getChildren = 
    match xml with
    | Element(elementName, attrs, children) ->
      match existsAttribute attrs (fun _ _ -> true) with
      | true ->
        new BulletmlDTDViolationException (sprintf "this element has no attributes.:[%s]" elementName) |> raise
      | false -> 
        let actionOrActionRef =
          match tryFindActionOrActionRef children getChildren with
          | Some actionOrActionRef -> actionOrActionRef
          | None -> new BulletmlDTDViolationException("repeat element should have Action or ActionRef.") |> raise
        Action.Repeat(createTimes children "repeat", actionOrActionRef)
    | _ -> new BulletmlDTDViolationException ("not support element.") |> raise

  /// 要素の名前から命令を作る。**None は「命令の位置に来ない節」**
  /// —— direction / speed / term / times / param など、親が自分で読む子。
  /// 呼び側（xmlToCommandList）はここで落ちたものを捨てる。
  ///
  /// **エラーではない。** 名前が命令なのに中身が DTD と違うときは、
  /// create* の中で BulletmlDTDViolationException が上がる。
  /// 子を、**その位置の型で**読む 3 本。
  ///
  /// 以前は 1 本の xmlToCommandList が全位置ぶんを平らな Bulletml（13 腕）で
  /// 返し、親がそれを filter + map で自分の位置の型へ入れ直していた。
  /// 入れ直しには `| _ -> raise "convert error"` が付いていた ——
  /// **型が防げるはずの検査を、実行時に置いていた。**
  ///
  /// 落とし方は位置ごとに違う。**ここは変えていない。**
  ///
  ///     action の子    命令でないものは黙って落とす（bullet / direction など）
  ///     bullet の子    action / actionRef 以外は黙って落とす
  ///     bulletml の子  bullet / fire / action 以外の**命令**は上げる。
  ///                    命令ですらないもの（direction など）は黙って落とす
  ///
  /// **fire だけが 2 つ の位置に来る**ので、どちらの腕を作るかは factory で渡す。
  let rec internal readCommands (children: XmlNode list) : Action list =
    children |> List.choose (fun child ->
      match child with
      | PCData _ -> None
      | Element (name, _, _) ->
        match name.ToLower() with
        | "changedirection" -> createChangeDirection child |> Some
        | "changespeed"     -> createChangeSpeed child |> Some
        | "accel"           -> createAccel child |> Some
        | "wait"            -> createWait child |> Some
        | "vanish"          -> createVanish child |> Some
        | "repeat"          -> createRepeat child readCommands |> Some
        | "fire"            -> createFire (Action.Fire) child readActionElms |> Some
        | "fireref"         -> createFireRef child |> Some
        | "action"          -> createAction (Action.Action) child readCommands |> Some
        | "actionref"       -> createActionRef (Action.ActionRef) child |> Some
        | _ -> None)

  and internal readActionElms (children: XmlNode list) : ActionElm list =
    children |> List.choose (fun child ->
      match child with
      | PCData _ -> None
      | Element (name, _, _) ->
        match name.ToLower() with
        | "action"    -> createAction (ActionElm.Action) child readCommands |> Some
        | "actionref" -> createActionRef (ActionElm.ActionRef) child |> Some
        | _ -> None)

  and internal readTopElms (children: XmlNode list) : BulletmlElm list =
    children |> List.choose (fun child ->
      match child with
      | PCData _ -> None
      | Element (name, _, _) ->
        match name.ToLower() with
        | "bullet" -> createBullet (BulletmlElm.Bullet) child readActionElms |> Some
        | "fire"   -> createFire (BulletmlElm.Fire) child readActionElms |> Some
        | "action" -> createAction (BulletmlElm.Action) child readCommands |> Some
        // 命令ではあるが bulletml の子になれないもの。**以前と同じく上げる**
        // （以前は平らな DU に一度組んでから filter で弾いていたので、
        //   例文に組んだ中身が入っていた。いまは要素の名前を出す）
        | "bulletml" | "actionref" | "fireref" | "changespeed" | "changedirection"
        | "accel" | "wait" | "vanish" | "bulletref" | "repeat" ->
          new BulletmlDTDViolationException (sprintf "not support child element：[%s]" name) |> raise
        | _ -> None)

  /// XML の木を BulletML の木にする。**読めなければ上げる。**
  ///
  /// 以前は「読めなかった」を NotCommand という値で返していた。
  /// 値で返すと、呼び側が受け取ったものを検査しないかぎり
  /// **空の弾幕がそのまま走る**（何も撃たない弾として）。
  /// 上の階（readXmlString / readSxmlString …）はどれも読めなければ
  /// 上げる形で書いてあるので、ここもそれに揃えた。
  /// **読めなかったかを値で受けたいときは tryBulletmlFromXmlNode。**
  [<CompiledName("ConvertBulletmlFromXmlNode")>]
  let convertBulletmlFromXmlNode xml : Bulletml =
    match xml with
    | PCData _ ->
      new BulletmlDTDViolationException ("root should be a bulletml element, not text.") |> raise
    | Element(name, _, _) ->
      if name.ToLower() = "bulletml" then
        createBulletml xml readTopElms
      else
        new BulletmlDTDViolationException (sprintf "root should be a bulletml element, not <%s>." name) |> raise

  /// 読めなければ None。**「読めなかった」と「読めたが中身が無い」を
  /// 見分けられなかったのはここ** —— 以前は例外だけを畳んでいたので、
  /// 読めなかったときも Some NotCommand が返っていた。
  [<CompiledName("TryBulletmlFromXmlNode")>]
  let tryBulletmlFromXmlNode xml : Bulletml option =
    try
      xml |> convertBulletmlFromXmlNode |> Some
    with | _ -> None

  let internal convertDirectionOption  = fun prams -> function
    | Some (Direction(attrs,s)) -> Direction(attrs, Param.replaceIn prams s) |> Some
    | None -> None

  let internal convertDirection  = fun prams -> function Direction(attrs,s) -> Direction(attrs, Param.replaceIn prams s) 

  let internal convertSpeedOption = fun prams -> function
    | Some (Speed(attrs,s)) -> Speed(attrs, Param.replaceIn prams s) |> Some
    | None -> None

  let internal convertSpeed = fun prams -> function Speed(attrs,s) -> Speed(attrs, Param.replaceIn prams s) 
  let internal convertTerm = fun prams -> function Term(s) -> Term(Param.replaceIn prams s)
  let internal convertTimes = fun prams -> function | Times(s) -> Times(Param.replaceIn prams s)
  let internal convertParam = fun prams -> List.map (fun s -> Param.replace s prams) 
  let internal convertWait = fun prams -> function | s -> Param.replaceIn prams s

  let internal convertHorizontalOption = fun prams -> function 
    | Some(Horizontal(attrs,s)) -> Horizontal(attrs, Param.replaceIn prams s) |> Some
    | None -> None
  let internal convertVerticalOption = fun prams -> function 
    | Some(Vertical(attrs,s)) -> Vertical(attrs, Param.replaceIn prams s) |> Some
    | None -> None
 
  let private convertRecBulletml' bulletml test = 
    // 値は BulletML の文書と同じ書き方（小数点は . ）で持ち回る。
    // F10 を既定カルチャで作ると , が混ざり、XPath が引数区切りと読んで落ちる。
    // test の側は元から不変（F# の string 演算子）で、明示に揃えただけ
    let toStr (single: float32) =
      if test then single.ToString(CultureInfo.InvariantCulture)
      else single.ToString("F10", CultureInfo.InvariantCulture)
    let rep (s: Expr.NumExpr) x (y:Lazy<'T>) = if (Expr.NumExpr.text s).Contains("$") then x else y.Force()
    let repDir direction = direction |> function
      | Some d -> d |> function 
        | Direction(a,x) -> rep x direction (lazy (Some (Direction(a,TryParse.eval (Expr.NumExpr.text x) |> toStr |> numExpr))))
      | None -> direction

    let repSpd speed = speed |> function
      | Some s -> s |> function 
        | Speed(a,x) -> rep x speed (lazy (Some (Speed(a,TryParse.eval (Expr.NumExpr.text x) |> toStr |> numExpr))))
      | None -> speed

    let repTimes times = times |> function 
      | Times(x) -> rep x times (lazy (Times(TryParse.eval (Expr.NumExpr.text x) |> toStr |> numExpr)))

    let repTerm term = term |> function 
      | Term(x) -> rep x term (lazy (Term(TryParse.eval (Expr.NumExpr.text x) |> toStr |> numExpr)))

    let repHorizontal horizontal = horizontal |> function
      | Some h -> h |> function 
        | Horizontal(a,x) -> rep x horizontal (lazy (Some (Horizontal(a,TryParse.eval (Expr.NumExpr.text x) |> toStr |> numExpr))))
      | None -> horizontal

    let repVertical vertical = vertical |> function
      | Some v -> v |> function 
        | Vertical(a,x) -> rep x vertical (lazy (Some (Vertical(a,TryParse.eval (Expr.NumExpr.text x) |> toStr |> numExpr))))
      | None -> vertical

    let repDirOne direction = direction |> function
      | Direction(a,x) -> rep x direction (lazy (Direction(a,TryParse.eval (Expr.NumExpr.text x) |> toStr |> numExpr)))
    let repSpdOne speed = speed |> function
      | Speed(a,x) -> rep x speed (lazy (Speed(a,TryParse.eval (Expr.NumExpr.text x) |> toStr |> numExpr)))
    let repWait times =
      rep times times (lazy (TryParse.eval (Expr.NumExpr.text times) |> toStr |> numExpr))

    // 位置ごとに変換する。公開の Bulletml ファミリと走らせる木が同じ形を
    // しているので、腕が 1 対 1 に並ぶ。以前は平らな DU 同士だったので
    // 「どの位置に来たか」を型が持たず、bulletElmToBulletml のような
    // 位置を潰す変換を挟んでから 1 つの match で受けていた
    let rec convertCommand (c: Action) : RecCommand =
      match c with
      | ChangeDirection (direction, term) ->
        RecCommand.ChangeDirection (repDirOne direction, repTerm term)
      | ChangeSpeed (speed, term) ->
        RecCommand.ChangeSpeed (repSpdOne speed, repTerm term)
      | Accel (horizontal, vertical, term) ->
        RecCommand.Accel (repHorizontal horizontal, repVertical vertical, repTerm term)
      | Vanish -> RecCommand.Vanish
      | Wait times -> RecCommand.Wait (repWait times)
      | Repeat (times, actionElm) ->
        RecCommand.Repeat (repTimes times, convertActionElm actionElm)
      | Fire (attrs, direction, speed, bulletElm) ->
        RecCommand.Fire (attrs, repDir direction, repSpd speed, convertBulletElm bulletElm)
      | FireRef (attrs, prams) -> RecCommand.FireRef (attrs, prams)
      | Action.Action (attrs, commands) ->
        RecCommand.Action (attrs, commands |> List.map convertCommand)
      | Action.ActionRef (attrs, prams) -> RecCommand.ActionRef (attrs, prams)

    and convertActionElm (a: ActionElm) : RecActionElm =
      match a with
      | ActionElm.Action (attrs, commands) ->
        RecActionElm.Action (attrs, commands |> List.map convertCommand)
      | ActionElm.ActionRef (attrs, prams) -> RecActionElm.ActionRef (attrs, prams)

    and convertBulletElm (b: BulletElm) : RecBulletElm =
      match b with
      | BulletElm.Bullet (attrs, direction, speed, actionElms) ->
        RecBulletElm.Bullet (attrs, repDir direction, repSpd speed,
                             actionElms |> List.map convertActionElm)
      | BulletElm.BulletRef (attrs, prams) -> RecBulletElm.BulletRef (attrs, prams)

    let convertTopElm (t: BulletmlElm) : RecTopElm =
      match t with
      | BulletmlElm.Bullet (attrs, direction, speed, actionElms) ->
        RecTopElm.Bullet (attrs, repDir direction, repSpd speed,
                          actionElms |> List.map convertActionElm)
      | BulletmlElm.Fire (attrs, direction, speed, bulletElm) ->
        RecTopElm.Fire (attrs, repDir direction, repSpd speed, convertBulletElm bulletElm)
      | BulletmlElm.Action (attrs, commands) ->
        RecTopElm.Action (attrs, commands |> List.map convertCommand)

    // 根は bulletml だけ。**公開の Bulletml も腕が 1 つ になったので、
    // ここで確かめる必要が無くなった** ——「どの要素でも表せる型」だった
    // 頃は `| _ -> raise` が要った
    match bulletml with
    | Bulletml.Bulletml (attrs, elms) ->
      RecBulletml.Bulletml (attrs, elms |> List.map convertTopElm)

  let internal convertRecBulletml bulletml= 
    convertRecBulletml' bulletml false

  let internal convertRecBulletmlForTest bulletml = 
    convertRecBulletml' bulletml true
