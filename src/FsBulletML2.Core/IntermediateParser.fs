namespace FsBulletML2
open System
open System.Globalization
// DTD は AutoOpen だが、open System が先に来るので Action が System.Action に
// 取られる。DU のほうを指すよう明示で開き直す
open FsBulletML2.DTD
open FsBulletML2.Processable

/// **995 行 ある。中身は 4 つ の役目で、境界はここに書いてある。**
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
///     3. Rec* の上の操作             collect 〜 expandActionRefOnceRec
///        ├ 集める / 探す             collect / getAction / tryFind* 残る
///        ├ param を差し込む          substCommand / refAction 系    残る
///        └ 輪を 1 段 解く            resolveActionRef / expand* 系  残る
///
/// **畳んで消えるのは 2 ＝ 約 125 行だけ。** 残りは木が 1 つ になっても要る。
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
/// ## NotCommand は公開 API から漏れる
///
/// 「BulletML の命令でない節」を表す腕で、Rec* からは消えたが公開の
/// Bulletml には残っている。作るのは 4 か所、捨てるのは 1 か所 だが、
/// **捨て切れていない。**
///
///     convertBulletmlFromXmlNode（公開）が
///       PCData を渡されたら NotCommand
///       根が bulletml でなければ NotCommand
///       中身が空なら NotCommand（xmlToBulletml 経由）
///
/// つまり**「読めなかった」を値で返している**。しかも
/// tryBulletmlFromXmlNode は例外だけを option に畳むので、
/// 読めなかったときは None ではなく **Some NotCommand** が返る
/// ——「成功したが中身が無い」と見分けが付かない。
///
/// 消すなら戻り値の型を変えることになる（option か Result）。
/// **公開 API の破壊的変更**なので、畳む話と一緒に決めること。
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

  let tryFindAttrValue attrs attrName = attrs |> List.tryPick (fun (label, v) -> if label = attrName && v <> "" then Some v else None)
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

  let internal getActions commands = 
    commands 
    |> List.map(fun command -> command |> function 
      | Bulletml.Action (attr, commands) -> ActionElm.Action(attr, commands) |> Some 
      | Bulletml.ActionRef (attr, commands) -> ActionElm.ActionRef(attr, commands) |> Some 
      | _ -> None)
    |> List.filter (fun x -> match x with | Some x -> true | _ -> false )
    |> List.map (fun x -> match x with | Some x -> x | _ -> new BulletmlDTDViolationException("not support action.") |> raise )

  /// XmlNode to Bulletml.Bulletml
  ///
  /// DTD :
  /// <!ELEMENT bulletml (bullet | fire | action)*>
  /// <!ATTLIST bulletml xmlns CDATA #IMPLIED>
  /// <!ATTLIST bulletml type (none|vertical|horizontal) "none">
  let internal createBulletml xml getChildren = 
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
        let commands : Bulletml list= getChildren xml children
        let bulletmlElements = 
          commands |> List.filter(function
            | Bulletml.Bullet _ -> true
            | Bulletml.Fire _ -> true
            | Bulletml.Action _ -> true
            | command -> new BulletmlDTDViolationException(sprintf "not support child element：[%s]" <| string command) |> raise)
          |> List.map (function
            | Bulletml.Bullet (a,b,c,d) -> BulletmlElm.Bullet (a,b,c,d)
            | Bulletml.Fire (a,b,c,d) -> BulletmlElm.Fire(a,b,c,d)
            | Bulletml.Action (a,b) -> BulletmlElm.Action(a,b)
            | _ -> new BulletmlDTDViolationException("convert error") |> raise)
        Bulletml.Bulletml(attrs, bulletmlElements ) 
      // 条件は type 属性の有無ではなく、attrs レコードそのものが取れなかったとき。
      // 上の maybe には let! が 1 つも無いので必ず return に着き、いまは届かない
      | None -> new BulletmlDTDViolationException("bulletml element attributes could not be read.") |> raise
    | _ -> new BulletmlDTDViolationException("not support element.") |> raise

  /// XmlNode to Bulletml.Action
  ///
  /// DTD :
  /// <!ELEMENT action (changeDirection | accel | vanish | changeSpeed | repeat | wait | (fire | fireRef) | (action | actionRef))*>
  /// <!ATTLIST action label CDATA #IMPLIED>
  let internal createAction factory xml getChildren = 
    match xml with
    | Element(name, attrs, children) ->
      let attrs = { actionLabel = tryFindLabelValue attrs |> Option.map ActionLabel }
      let commands = getChildren xml children

      let actionElements = 
        commands |> List.filter(function
          | Bulletml.ChangeDirection _ -> true
          | Bulletml.Accel _ -> true
          | Bulletml.Vanish -> true
          | Bulletml.ChangeSpeed _ -> true
          | Bulletml.Repeat _ -> true
          | Bulletml.Wait _ -> true
          | Bulletml.Fire _ -> true
          | Bulletml.FireRef _ -> true
          | Bulletml.Action _ -> true
          | Bulletml.ActionRef _ -> true
          | _ -> false)
        |> List.map (function 
          | Bulletml.ChangeDirection (a,b) -> Action.ChangeDirection (a,b)
          | Bulletml.Accel (a,b,c) -> Action.Accel(a,b,c)
          | Bulletml.Vanish -> Action.Vanish 
          | Bulletml.ChangeSpeed (a,b) -> Action.ChangeSpeed (a,b)
          | Bulletml.Repeat (a,b) -> Action.Repeat (a,b)
          | Bulletml.Wait (a) -> Action.Wait (a)
          | Bulletml.Fire (a,b,c,d) -> Action.Fire(a,b,c,d)
          | Bulletml.FireRef (a,b) -> Action.FireRef (a,b)
          | Bulletml.Action (a,b) -> Action.Action(a,b)
          | Bulletml.ActionRef (a,b) -> Action.ActionRef (a,b)
          | _ -> new BulletmlDTDViolationException("convert error") |> raise)

      factory(attrs, actionElements) 
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

  let internal tryFindActionOrActionRef (children:XmlNode list) getChildren = 
    let f xml = 
      match xml with 
      | Element(name, attrs, children) -> 
        match name.ToLower() with
        | "action" -> createAction (ActionElm.Action) xml getChildren |> Some
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
  let internal createBullet factory xml getChildren = 
    match xml with 
    | Element(_ , attrs, children) -> 
      let text = getTextDefault children "0"
      let attr = { bulletLabel = tryFindLabelValue attrs |> Option.map BulletLabel }
      let commands = getChildren xml children

      let actions = getActions commands
      factory(attr, tryFindDirection children, tryFindSpeed children, actions)
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

  let internal tryFindBulletOrBulletRef (children:XmlNode list) getChildren = 
    let f xml = 
      match xml with 
      | Element(name, attrs, children) -> 
        let text = getTextDefault children "0"
        match name.ToLower()  with
        | "bullet"    -> createBullet (BulletElm.Bullet) xml getChildren |> Some
        | "bulletref" -> createBulletRef (BulletElm.BulletRef) xml  |> Some
        | _ -> None
      | _ -> new BulletmlDTDViolationException("not support element.") |> raise
    children |> List.tryPick f

  /// XmlNode to Bulletml.Fire
  ///
  /// DTD :
  /// <!ELEMENT fire (direction?, speed?, (bullet | bulletRef))>
  /// <!ATTLIST fire label CDATA #IMPLIED>
  let internal createFire xml getChildren = 
    match xml with
    | Element(_ , attrs, children) ->
      let fireattrs = { FireAttrs.fireLabel = tryFindLabelValue attrs |> Option.map FireLabel }
      match tryFindBulletOrBulletRef children getChildren with
      | Some bullet ->
        Bulletml.Fire(fireattrs, tryFindDirection children, tryFindSpeed children, bullet )
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
        Bulletml.FireRef(attrs, getParam xml)
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
        Bulletml.Accel(horizontal, vertical , createTerm children "accel")
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

        Bulletml.ChangeSpeed(speed, createTerm children "changeSpeed")
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
        Bulletml.ChangeDirection(direction, createTerm children "changeDirection")
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
        | Some text -> Bulletml.Wait(numExpr text)
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
        | None -> Bulletml.Vanish
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
        Bulletml.Repeat(createTimes children "repeat", actionOrActionRef)
    | _ -> new BulletmlDTDViolationException ("not support element.") |> raise

  let internal (|Command|) command xml (getChildren:XmlNode -> XmlNode list -> Bulletml list) : Bulletml =
    match  (command:string).ToLower() with
    | "bulletml"        -> createBulletml xml getChildren
    | "action"          -> createAction (fun x -> Bulletml.Action(x)) xml getChildren
    | "actionref"       -> createActionRef (fun x -> Bulletml.ActionRef(x)) xml
    | "fire"            -> createFire xml getChildren
    | "fireref"         -> createFireRef xml
    | "changespeed"     -> createChangeSpeed xml
    | "changedirection" -> createChangeDirection xml 
    | "accel"           -> createAccel xml
    | "wait"            -> createWait xml
    | "vanish"          -> createVanish xml
    | "bullet"          -> createBullet (Bulletml.Bullet) xml getChildren
    | "bulletref"       -> createBulletRef (Bulletml.BulletRef) xml
    | "repeat"          -> createRepeat xml getChildren 
    | _ -> NotCommand

  let rec internal xmlToCommandList topXml xml = 
    let rec xmlToCommandList' topXml xml (list:Bulletml list) = 
      let getChildren topXml children = List.fold (fun tl child -> tl@xmlToCommandList topXml child) [] children 
      match xml with
      | PCData s -> list 
      | Element (name, attrs, children) -> 
        let command = name |> function Command func -> func xml getChildren 
        match command  with
        | Bulletml _  
        | Bulletml.ActionRef _ 
        | Bulletml.Repeat _ 
        | Bulletml.FireRef _ 
        | Bulletml.ChangeDirection _ 
        | Bulletml.ChangeSpeed _ 
        | Bulletml.Accel _ 
        | Bulletml.Wait _ 
        | Bulletml.Vanish 
        | Bulletml.BulletRef _ 
        | Bulletml.Action _ 
        | Bulletml.Fire _ 
        | Bulletml.Bullet _ ->
          command::list 
        | NotCommand -> list 
    xmlToCommandList' topXml xml []

  let internal xmlToBulletml topXml xml = 
    xmlToCommandList topXml xml 
    |> function
    | [] -> NotCommand
    | lst -> lst |> List.head

  [<CompiledName("ConvertBulletmlFromXmlNode")>]
  let convertBulletmlFromXmlNode xml =
    match xml with
    | PCData s -> NotCommand 
    | Element(name, _, _) ->
      if name.ToLower() = "bulletml" then
        xmlToBulletml xml xml
      else
        NotCommand 

  [<CompiledName("TryBulletmlFromXmlNode")>]
  let tryBulletmlFromXmlNode xml = 
    try
      xml |> convertBulletmlFromXmlNode |> Some
    with | ex -> None

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

    // 走らせる木の根は bulletml だけ。公開の Bulletml は「どの要素でも」
    // 表せる型なので、ここで根であることを確かめる。DTD の
    // <!ELEMENT bulletml ...> が根であるという決めが、ここに 1 回だけ出る
    match bulletml with
    | Bulletml.Bulletml (attrs, elms) ->
      RecBulletml.Bulletml (attrs, elms |> List.map convertTopElm)
    | _ ->
      new BulletmlDTDViolationException("走らせる木の根は bulletml でなければなりません。") |> raise

  let internal convertRecBulletml bulletml= 
    convertRecBulletml' bulletml false

  let internal convertRecBulletmlForTest bulletml = 
    convertRecBulletml' bulletml true

  /// 木を隅々まで歩いて、名前の付いた要素を集める。
  ///
  /// 以前は同じ形の走査を getAction / getFire / getBullet で 3 回 書いていた
  /// （どれも 14 の腕を並べ、拾う 1 腕だけが違った）。位置ごとに型が
  /// 分かれたので、歩き方を 1 本 にして「拾うもの」だけを差し替える。
  ///
  /// 拾う順は変えていない —— 自分を先に入れてから子へ降りる
  let private collect
      (fromAction: ActionAttrs * RecCommand list -> 'a list)
      (fromFire: FireAttrs * Direction option * Speed option * RecBulletElm -> 'a list)
      (fromBullet: BulletAttrs * Direction option * Speed option * RecActionElm list -> 'a list)
      (root: RecBulletml) : 'a list =
    let rec command (c: RecCommand) =
      match c with
      | RecCommand.Action (attrs, children) ->
        fromAction (attrs, children) @ (children |> List.collect command)
      | RecCommand.Fire (attrs, d, s, child) ->
        fromFire (attrs, d, s, child) @ bulletElm child
      | RecCommand.Repeat (_, child) -> actionElm child
      // 要素の子を持たない腕。以前はここに NotCommand と、命令の位置には
      // 来られない Bulletml / Bullet / BulletRef も並んでいた
      | RecCommand.ActionRef _ | RecCommand.FireRef _
      | RecCommand.ChangeDirection _ | RecCommand.ChangeSpeed _
      | RecCommand.Accel _ | RecCommand.Wait _ | RecCommand.Vanish -> []
    and actionElm (a: RecActionElm) =
      match a with
      | RecActionElm.Action (attrs, children) ->
        fromAction (attrs, children) @ (children |> List.collect command)
      | RecActionElm.ActionRef _ -> []
    and bulletElm (b: RecBulletElm) =
      match b with
      | RecBulletElm.Bullet (attrs, d, s, children) ->
        fromBullet (attrs, d, s, children) @ (children |> List.collect actionElm)
      | RecBulletElm.BulletRef _ -> []
    let topElm (t: RecTopElm) =
      match t with
      | RecTopElm.Bullet (attrs, d, s, children) ->
        fromBullet (attrs, d, s, children) @ (children |> List.collect actionElm)
      | RecTopElm.Fire (attrs, d, s, child) ->
        fromFire (attrs, d, s, child) @ bulletElm child
      | RecTopElm.Action (attrs, children) ->
        fromAction (attrs, children) @ (children |> List.collect command)
    match root with
    | RecBulletml.Bulletml (_, elms) -> elms |> List.collect topElm

  /// 名前の付いた action。actionRef が指す先になれるので RecActionElm で返す
  let internal getAction (recBulletml: RecBulletml) : RecActionElm list =
    recBulletml |> collect
      (fun (attrs, children) ->
        match attrs.actionLabel with
        | Some _ -> [ RecActionElm.Action (attrs, children) ]
        | None -> [])
      (fun _ -> [])
      (fun _ -> [])

  let internal tryFindAction recBulletml (targetLabel: ActionLabel) =
    getAction recBulletml |> List.tryFind (function
      | RecActionElm.Action (attrs, _) ->
        // 以前は tryFindLabelValue [("label", v)] を通していたが、
        // 1 要素の連想リストから同じキーを引くだけで、常に Some v を返す
        // 空回りだった。型が付いたのでそのまま比べる
        (match attrs.actionLabel with Some v -> v = targetLabel | None -> false)
      | RecActionElm.ActionRef _ -> false)

  /// 名前の付いた fire。fireRef が指す先は命令の位置へ差し込まれるので
  /// RecCommand.Fire で返す（根の直下にある fire も同じ形にして返す）
  let internal getFire (recBulletml: RecBulletml) : RecCommand list =
    recBulletml |> collect
      (fun _ -> [])
      (fun (attrs, d, s, child) ->
        match attrs.fireLabel with
        | Some _ -> [ RecCommand.Fire (attrs, d, s, child) ]
        | None -> [])
      (fun _ -> [])

  let internal tryFindFire recBulletml (targetLabel: FireLabel) =
    getFire recBulletml |> List.tryFind (function
      | RecCommand.Fire (attrs, _, _, _) ->
        (match attrs.fireLabel with Some v -> v = targetLabel | None -> false)
      | _ -> false)

  /// 名前の付いた bullet。bulletRef が指す先になれるので RecBulletElm で返す
  let internal getBullet (recBulletml: RecBulletml) : RecBulletElm list =
    recBulletml |> collect
      (fun _ -> [])
      (fun _ -> [])
      (fun (attrs, d, s, children) ->
        match attrs.bulletLabel with
        | Some _ -> [ RecBulletElm.Bullet (attrs, d, s, children) ]
        | None -> [])

  let internal tryFindBullet recBulletml (targetLabel: BulletLabel) =
    getBullet recBulletml |> List.tryFind (function
      | RecBulletElm.Bullet (attrs, _, _, _) ->
        (match attrs.bulletLabel with Some v -> v = targetLabel | None -> false)
      | RecBulletElm.BulletRef _ -> false)

  /// 実引数を差し込む走査。位置ごとに分ける。
  ///
  /// 以前は 1 本の convert が平らな DU を歩き、最後に `| x -> x` で
  /// 「触らない腕」をまとめて受けていた。その `x` には
  /// Vanish（触らなくてよい）と Bulletml / NotCommand（そもそも
  /// ここへ来ない）が混ざっていた
  let rec private substCommand prams (c: RecCommand) : RecCommand =
    match c with
    | RecCommand.ChangeDirection (direction, term) ->
      RecCommand.ChangeDirection (convertDirection prams direction, convertTerm prams term)
    | RecCommand.ChangeSpeed (speed, term) ->
      RecCommand.ChangeSpeed (convertSpeed prams speed, convertTerm prams term)
    | RecCommand.Accel (horizontal, vertical, term) ->
      RecCommand.Accel (convertHorizontalOption prams horizontal,
                        convertVerticalOption prams vertical, convertTerm prams term)
    | RecCommand.Wait s -> RecCommand.Wait (convertWait prams s)
    | RecCommand.Vanish -> RecCommand.Vanish
    | RecCommand.Repeat (times, child) ->
      RecCommand.Repeat (convertTimes prams times, substActionElm prams child)
    | RecCommand.Fire (attrs, direction, speed, child) ->
      RecCommand.Fire (attrs, convertDirectionOption prams direction,
                       convertSpeedOption prams speed, substBulletElm prams child)
    | RecCommand.FireRef (attrs, param) -> RecCommand.FireRef (attrs, convertParam prams param)
    | RecCommand.Action (attrs, children) ->
      RecCommand.Action (attrs, children |> List.map (substCommand prams))
    | RecCommand.ActionRef (attrs, param) -> RecCommand.ActionRef (attrs, convertParam prams param)

  and private substActionElm prams (a: RecActionElm) : RecActionElm =
    match a with
    | RecActionElm.Action (attrs, children) ->
      RecActionElm.Action (attrs, children |> List.map (substCommand prams))
    | RecActionElm.ActionRef (attrs, param) -> RecActionElm.ActionRef (attrs, convertParam prams param)

  and private substBulletElm prams (b: RecBulletElm) : RecBulletElm =
    match b with
    | RecBulletElm.Bullet (attrs, direction, speed, children) ->
      RecBulletElm.Bullet (attrs, convertDirectionOption prams direction,
                           convertSpeedOption prams speed,
                           children |> List.map (substActionElm prams))
    | RecBulletElm.BulletRef (attrs, param) -> RecBulletElm.BulletRef (attrs, convertParam prams param)

  /// 参照先の要素へ実引数を差し込む。**種別ごとに 1 本 ずつ。**
  ///
  /// 以前は target を 1 つの平らな型で受け、target と label の種別が
  /// 揃っていることを型で言えなかった（前の段で RefKey を入れて
  /// 「揃っている腕だけ」を書ける形にしたが、まだ 1 本 の関数だった）。
  /// 位置ごとに型が分かれたので、関数そのものが 3 本 に割れて、
  /// 揃わない呼び方が書けなくなる。
  ///
  /// 名前の一致を確かめてから差し込むのは以前と同じ。呼ぶ側は
  /// tryFind* が返したものを渡すので必ず一致するが、確認は残す
  let internal refAction (target: RecActionElm) (label: ActionLabel) prams : RecActionElm =
    let prams = prams |> Param.ofList
    match target with
    | RecActionElm.Action (attrs, _) when attrs.actionLabel = Some label ->
      substActionElm prams target
    | _ -> target

  let internal refFire (target: RecCommand) (label: FireLabel) prams : RecCommand =
    let prams = prams |> Param.ofList
    match target with
    | RecCommand.Fire (attrs, _, _, _) when attrs.fireLabel = Some label ->
      substCommand prams target
    | _ -> target

  let internal refBullet (target: RecBulletElm) (label: BulletLabel) prams : RecBulletElm =
    let prams = prams |> Param.ofList
    match target with
    | RecBulletElm.Bullet (attrs, _, _, _) when attrs.bulletLabel = Some label ->
      substBulletElm prams target
    | _ -> target

  /// 展開中の参照は DTD.RefKey が表す（action:foo と bullet:foo は別物）。
  /// 以前はここに refKey kind label = kind + ":" + label があり、種別を
  /// 文字で足していた。型にしたので、足し忘れも綴り違いも起きない

  /// 参照を解いて木へ展開する。
  ///
  /// lastAction は「直近に展開した action の label」。
  /// action の輪を残してよいのは、その輪が直近に展開した action 自身へ戻るときだけ。
  /// 別の action を経由する輪は、解いた結果の中に action が挟まるので、
  /// 走らせる側が 1 段ずつ解くと呼び出しがフレームごとに深くなる。
  ///
  /// 位置ごとに関数が分かれた。actionRef は「命令の位置」と
  /// 「repeat / bullet の子の位置」の両方に出るので、解く判断だけを
  /// resolveActionRef に出して両方から使う
  let rec private resolveActionRef visiting lastAction top (attrs: ActionRefAttrs) prams
      : RecActionElm option =
    // None は「輪なので、そのまま残す」
    let key = ActionKey attrs.actionRefLabel
    if Set.contains key visiting then
      if lastAction = Some attrs.actionRefLabel then None
      else
        new BulletmlDTDViolationException(
              sprintf "circular reference detected:[%s] 参照が輪になっているため展開できません" (RefKey.text key)) |> raise
    else
      match tryFindAction top attrs.actionRefLabel with
      | Some action ->
        let newAction = refAction action attrs.actionRefLabel prams
        Some (expandActionElm (Set.add key visiting) (Some attrs.actionRefLabel) top newAction)
      | None ->
        new BulletmlDTDViolationException(
              sprintf "not found target Action element:%s" (ActionLabel.text attrs.actionRefLabel)) |> raise

  and private expandCommand visiting lastAction top (c: RecCommand) : RecCommand =
    match c with
    | RecCommand.ActionRef (attrs, prams) ->
      match resolveActionRef visiting lastAction top attrs prams with
      | None -> c
      // 解いた結果は action。ActionRef が居たのは命令の位置なので、命令として置く
      | Some (RecActionElm.Action (a, cs)) -> RecCommand.Action (a, cs)
      | Some (RecActionElm.ActionRef (a, p)) -> RecCommand.ActionRef (a, p)
    | RecCommand.FireRef (attrs, prams) ->
      let key = FireKey attrs.fireRefLabel
      if Set.contains key visiting then
        new BulletmlDTDViolationException(
              sprintf "circular reference detected:[%s] 参照が輪になっているため展開できません" (RefKey.text key)) |> raise
      let visiting = Set.add key visiting
      match tryFindFire top attrs.fireRefLabel with
      | Some fire ->
        let newFire = refFire fire attrs.fireRefLabel prams
        expandCommand visiting None top newFire
      | None ->
        new BulletmlDTDViolationException(
              sprintf "not found target Fire element:%s" (FireLabel.text attrs.fireRefLabel)) |> raise
    | RecCommand.Action (attrs, children) ->
      RecCommand.Action (attrs, children |> List.map (expandCommand visiting lastAction top))
    | RecCommand.Fire (attrs, d, s, child) ->
      RecCommand.Fire (attrs, d, s, expandBulletElm visiting lastAction top child)
    | RecCommand.Repeat (times, child) ->
      RecCommand.Repeat (times, expandActionElm visiting lastAction top child)
    | RecCommand.ChangeDirection _ | RecCommand.ChangeSpeed _
    | RecCommand.Accel _ | RecCommand.Wait _ | RecCommand.Vanish -> c

  and private expandActionElm visiting lastAction top (a: RecActionElm) : RecActionElm =
    match a with
    | RecActionElm.Action (attrs, children) ->
      RecActionElm.Action (attrs, children |> List.map (expandCommand visiting lastAction top))
    | RecActionElm.ActionRef (attrs, prams) ->
      match resolveActionRef visiting lastAction top attrs prams with
      | None -> a
      | Some expanded -> expanded

  and private expandBulletElm visiting lastAction top (b: RecBulletElm) : RecBulletElm =
    match b with
    | RecBulletElm.Bullet (attrs, d, s, children) ->
      RecBulletElm.Bullet (attrs, d, s, children |> List.map (expandActionElm visiting lastAction top))
    | RecBulletElm.BulletRef (attrs, prams) ->
      let key = BulletKey attrs.bulletRefLabel
      if Set.contains key visiting then
        // 輪。展開せず残し、走らせる側が 1 段ずつ解く
        b
      else
        match tryFindBullet top attrs.bulletRefLabel with
        | Some bullet ->
          let newBullet = refBullet bullet attrs.bulletRefLabel prams
          expandBulletElm (Set.add key visiting) None top newBullet
        | None ->
          new BulletmlDTDViolationException(
                sprintf "not foun target Bullet element:%s" (BulletLabel.text attrs.bulletRefLabel)) |> raise

  let private expandTopElm visiting lastAction top (t: RecTopElm) : RecTopElm =
    match t with
    | RecTopElm.Bullet (attrs, d, s, children) ->
      RecTopElm.Bullet (attrs, d, s, children |> List.map (expandActionElm visiting lastAction top))
    | RecTopElm.Fire (attrs, d, s, child) ->
      RecTopElm.Fire (attrs, d, s, expandBulletElm visiting lastAction top child)
    | RecTopElm.Action (attrs, children) ->
      RecTopElm.Action (attrs, children |> List.map (expandCommand visiting lastAction top))

  /// 木を丸ごと展開する。根から入る唯一の入口
  let internal convertRefBulletml (top: RecBulletml) (recBulletml: RecBulletml) : RecBulletml =
    match recBulletml with
    | RecBulletml.Bulletml (attrs, elms) ->
      RecBulletml.Bulletml (attrs, elms |> List.map (expandTopElm Set.empty None top))

  /// top* の台本 1 本 を展開する。BulletRunner.buildRootTops が使う
  let internal convertRefActionElm (top: RecBulletml) (a: RecActionElm) : RecActionElm =
    expandActionElm Set.empty None top a

  /// 輪のために展開を止めた bulletRef を、走らせる側から 1 段だけ解く。
  /// 中にまた同じ参照が残るので、次に撃たれたときに次の 1 段が解かれる。
  ///
  /// 解く前から自分の key を visiting に入れておくこと。空から始めると
  /// 解いた中身の同じ参照がもう 1 段 展開され、1 段のつもりが 2 段になる。
  /// 新経路（Step.Resolvers）は木を組まないのでこちらを直に使う
  let internal expandBulletRefOnceRec top (label: BulletLabel) prams : RecBulletElm option =
    match tryFindBullet top label with
    | Some bullet ->
      // param は文字のまま渡す（Params は string list で、Param.replace も
      // 文字の置き換えなので、ここで数へ潰すと $rank / $rand が凍る）
      refBullet bullet label prams
      |> expandBulletElm (Set.singleton (BulletKey label)) None top
      |> Some
    | None -> None

  /// 輪のために展開を止めた actionRef を、走らせる側から 1 段だけ解く。
  /// 中にまた同じ参照が残るので、そこへ届いたときに次の 1 段が解かれる。
  ///
  /// bulletRef と違って fire を挟まないので、2 段 解くと走らせる側の
  /// 呼び出しが 1 フレームごとに深くなり、スタックを使い切る
  let internal expandActionRefOnceRec top (label: ActionLabel) prams : RecActionElm option =
    match tryFindAction top label with
    | Some action ->
      refAction action label prams
      |> expandActionElm (Set.singleton (ActionKey label)) (Some label) top
      |> Some
    | None -> None
