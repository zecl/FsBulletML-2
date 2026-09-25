namespace FsBulletML2

open System
open System.Globalization
// DTD は AutoOpen だが、open System が先に来るので Action が System.Action に
// 取られる。DU のほうを指すよう明示で開き直す
open FsBulletML2.DTD

/// XML を読んで木にし、定数を畳むところまで。
/// 木の上の操作は BulletmlOps.fs。読めなかったは戻り値の型で言う。
module BulletmlRead =
    let internal existsAttribute attrs f =
        attrs |> List.exists (fun (label, v) -> if f label v then true else false)

    let internal tryFindPCData children =
        children
        |> List.tryPick (function
            | PCData x -> Some x
            | _ -> None)

    /// 属性 を持たない要素 が属性 を持っていたら上げる
    let internal refuseAttributes (elementName: string) attrs =
        if existsAttribute attrs (fun _ _ -> true) then
            new BulletmlDTDViolationException(sprintf "this element has no attributes.:[%s]" elementName)
            |> raise

    let internal notSupported () : 'T =
        new BulletmlDTDViolationException("not support element.") |> raise

    let internal getElement children parentElementName elementName factory =
        let termXml =
            children
            |> List.tryFind (fun xml ->
                match xml with
                | Element(name, attrs, _) ->
                    if name.ToLower() = elementName then
                        refuseAttributes elementName attrs
                        true
                    else
                        false
                | _ -> false)

        let pcdata =
            function
            | Element(_, _, children) -> tryFindPCData children
            | _ -> None

        match termXml with
        | Some term ->
            match pcdata term with
            | Some text -> factory (text)
            | None ->
                new BulletmlDTDViolationException(sprintf "[%s] element should have #PCDATA." elementName)
                |> raise
        | None ->
            new BulletmlDTDViolationException(
                sprintf "[%s] element should have [%s] element." parentElementName elementName
            )
            |> raise

    let internal createTerm children parentElementName =
        getElement children parentElementName "term" (numExpr >> Term)

    let internal createTimes children parentElementName =
        getElement children parentElementName "times" (numExpr >> Times)

    let internal getParam xml =
        match xml with
        | PCData _ -> []
        | Element(_, _, children) ->
            let rec f x =
                match x with
                | PCData x -> x
                | Element(elementName, attrs, children) ->
                    if elementName = "param" then
                        refuseAttributes elementName attrs

                        if children |> List.length = 0 then
                            new BulletmlDTDViolationException(sprintf "[%s] element should have #PCDATA." elementName)
                            |> raise

                        f (children.[0])
                    else
                        new BulletmlDTDViolationException(sprintf "not support element.:[%s]" elementName)
                        |> raise

            List.map f children

    let internal tryFindAttrValue attrs attrName =
        attrs
        |> List.tryPick (fun (label, v) -> if label = attrName && v <> "" then Some v else None)

    let internal tryFindLabelValue attrs = tryFindAttrValue attrs "label"

    let internal toSpeedType (s: string) =
        match s.ToLower() with
        | "absolute" -> SpeedType.Absolute
        | "relative" -> SpeedType.Relative
        | "sequence" -> SpeedType.Sequence
        | x ->
            new BulletmlDTDViolationException(sprintf "not support SpeedType.:[%s]" x)
            |> raise

    let internal toDirectionType (s: string) =
        match s.ToLower() with
        | "aim" -> DirectionType.Aim
        | "absolute" -> DirectionType.Absolute
        | "relative" -> DirectionType.Relative
        | "sequence" -> DirectionType.Sequence
        | x ->
            new BulletmlDTDViolationException(sprintf "not support DirectionType.:[%s]" x)
            |> raise

    let internal toHorizontalType (s: string) =
        match s.ToLower() with
        | "absolute" -> HorizontalType.Absolute
        | "relative" -> HorizontalType.Relative
        | "sequence" -> HorizontalType.Sequence
        | x ->
            new BulletmlDTDViolationException(sprintf "not support HorizontalType.:[%s]" x)
            |> raise

    let internal toVerticalType (s: string) =
        match s.ToLower() with
        | "absolute" -> VerticalType.Absolute
        | "relative" -> VerticalType.Relative
        | "sequence" -> VerticalType.Sequence
        | x ->
            new BulletmlDTDViolationException(sprintf "not support VerticalType.:[%s]" x)
            |> raise

    /// direction / speed / horizontal / vertical の 4 つ。型 の属性 が 1 つ と、式 の #PCDATA を持つ。
    /// 属性 の字 を型 にする（読めなければ上げる）のが、#PCDATA が無い のを上げるより先
    let private tryFindTyped (tag: string) (toAttrs: string -> 'A) (make: 'A option * Expr.NumExpr -> 'T) children =
        children
        |> List.tryPick (function
            | Element(elementName, attrs, children) ->
                if elementName.ToLower() = tag then
                    let attr =
                        attrs
                        |> List.tryPick (fun (_, x) -> if x <> "" then Some x else None)
                        |> Option.map toAttrs

                    match tryFindPCData children with
                    | Some text -> make (attr, numExpr text) |> Some
                    | None ->
                        new BulletmlDTDViolationException(sprintf "[%s] element should have #PCDATA." elementName)
                        |> raise
                else
                    None
            | _ -> notSupported ())

    let internal tryFindDirection (children: XmlNode list) =
        tryFindTyped
            "direction"
            (fun s ->
                {
                    DirectionAttrs.directionType = toDirectionType s
                })
            Direction
            children

    let internal tryFindSpeed (children: XmlNode list) =
        tryFindTyped "speed" (fun s -> { SpeedAttrs.speedType = toSpeedType s }) Speed children

    let internal tryFindHorizontal (children: XmlNode list) =
        tryFindTyped
            "horizontal"
            (fun s ->
                {
                    HorizontalAttrs.horizontalType = toHorizontalType s
                })
            Horizontal
            children

    let internal tryFindVertical (children: XmlNode list) =
        tryFindTyped
            "vertical"
            (fun s ->
                {
                    VerticalAttrs.verticalType = toVerticalType s
                })
            Vertical
            children

    /// XmlNode to Bulletml.Bulletml
    ///
    /// DTD :
    /// <!ELEMENT bulletml (bullet | fire | action)*>
    /// <!ATTLIST bulletml xmlns CDATA #IMPLIED>
    /// <!ATTLIST bulletml type (none|vertical|horizontal) "none">
    let internal createBulletml xml readTopElms =
        match xml with
        | Element(_, attrs, children) ->
            let tryFindBulletmlAttrs =
                maybe {
                    let toShootingDirection (s: string) =
                        match s.ToLower() with
                        | "none" -> ShootingDirection.BulletNone
                        | "vertical" -> ShootingDirection.BulletVertical
                        | "horizontal" -> ShootingDirection.BulletHorizontal
                        | x ->
                            new BulletmlDTDViolationException(sprintf "not support ShootingDirection.：[%s]" x)
                            |> raise

                    let xmlns = tryFindAttrValue attrs "xmlns"
                    let name = tryFindAttrValue attrs "name"
                    // description は BulletML 公式の属性ではない（名前 / 説明文を入れるために足した）
                    let description = tryFindAttrValue attrs "description"

                    match tryFindAttrValue attrs "type" with
                    | Some shootingDirection ->
                        return
                            {
                                bulletmlXmlns = xmlns
                                bulletmlType = shootingDirection |> toShootingDirection |> Some
                                bulletmlName = name
                                bulletmlDescription = description
                            }
                    | None ->
                        return
                            {
                                bulletmlXmlns = xmlns
                                bulletmlType = None
                                bulletmlName = name
                                bulletmlDescription = description
                            }
                }

            match tryFindBulletmlAttrs with
            | Some attrs -> Bulletml.Bulletml(attrs, readTopElms children)
            // いまは届かない（上の maybe には let! が 1 つも無いので必ず return に着く）
            | None ->
                new BulletmlDTDViolationException("bulletml element attributes could not be read.")
                |> raise
        | _ -> notSupported ()

    /// XmlNode to action。どの位置の腕を作るかは factory が決める
    ///
    /// DTD :
    /// <!ELEMENT action (changeDirection | accel | vanish | changeSpeed | repeat | wait | (fire | fireRef) | (action | actionRef))*>
    /// <!ATTLIST action label CDATA #IMPLIED>
    let internal createAction factory xml readCommands =
        match xml with
        | Element(_, attrs, children) ->
            let attrs =
                {
                    actionLabel = tryFindLabelValue attrs |> Option.map ActionLabel
                }
            // 命令でない子（bullet / bulletRef / direction …）は readCommands が黙って落とす
            factory (attrs, readCommands children)
        | _ -> notSupported ()

    /// XmlNode to actionRef。どの位置の腕を作るかは factory が決める
    ///
    /// DTD :
    /// <!ELEMENT actionRef (param* )>
    /// <!ATTLIST actionRef label CDATA #REQUIRED>
    /// actionRef / bulletRef / fireRef の 3 つ。label が要り、子 は param だけ
    let private createRef (kind: string) (toAttrs: string -> 'A) (make: 'A * string list -> 'R) xml =
        match xml with
        | Element(_, attrs, _) ->
            match tryFindLabelValue attrs with
            | Some label -> make (toAttrs label, getParam xml)
            | None ->
                new BulletmlDTDViolationException(sprintf "%s element should have label attribute." kind)
                |> raise
        | _ -> notSupported ()

    let internal createActionRef factory xml =
        createRef "ActionRef" (fun label -> { actionRefLabel = ActionLabel label }) factory xml

    let internal tryFindActionOrActionRef (children: XmlNode list) readCommands =
        let f xml =
            match xml with
            | Element(name, _, _) ->
                match name.ToLower() with
                | "action" -> createAction (ActionElm.Action) xml readCommands |> Some
                | "actionref" -> createActionRef (ActionElm.ActionRef) xml |> Some
                | _ -> None
            | _ -> notSupported ()

        let result =
            children
            |> List.filter (function
                | Element(name, _, _) ->
                    match name.ToLower() with
                    | "action"
                    | "actionref" -> true
                    | _ -> false
                | _ -> false)

        if result |> List.length > 1 then
            new BulletmlDTDViolationException("repeat element cannot have multiple elements of (Action|ActionRef).")
            |> raise
        elif result |> List.length = 0 then
            new BulletmlDTDViolationException("repeat element should have Action or ActionRef.")
            |> raise

        result.[0] |> f

    /// XmlNode to bullet。どの位置の腕を作るかは factory が決める
    ///
    /// DTD :
    /// <!ELEMENT bullet (direction?, speed?, (action | actionRef)* )>
    /// <!ATTLIST bullet label CDATA #IMPLIED>
    let internal createBullet factory xml readActionElms =
        match xml with
        | Element(_, attrs, children) ->
            let attr =
                {
                    bulletLabel = tryFindLabelValue attrs |> Option.map BulletLabel
                }
            // action / actionRef 以外は readActionElms が黙って落とす（getActions と同じ）
            factory (attr, tryFindDirection children, tryFindSpeed children, readActionElms children)
        | _ -> notSupported ()

    /// XmlNode to bulletRef。どの位置の腕を作るかは factory が決める
    ///
    /// DTD :
    /// <!ELEMENT bulletRef (param* )>
    /// <!ATTLIST bulletRef label CDATA #REQUIRED>
    let internal createBulletRef factory xml =
        createRef "BulletRef" (fun label -> { bulletRefLabel = BulletLabel label }) factory xml

    let internal tryFindBulletOrBulletRef (children: XmlNode list) readActionElms =
        let f xml =
            match xml with
            | Element(name, _, _) ->
                match name.ToLower() with
                | "bullet" -> createBullet (BulletElm.Bullet) xml readActionElms |> Some
                | "bulletref" -> createBulletRef (BulletElm.BulletRef) xml |> Some
                | _ -> None
            | _ -> notSupported ()

        children |> List.tryPick f

    /// XmlNode to fire。どの位置の腕を作るかは factory が決める
    ///
    /// DTD :
    /// <!ELEMENT fire (direction?, speed?, (bullet | bulletRef))>
    /// <!ATTLIST fire label CDATA #IMPLIED>
    /// fire は action の子にも bulletml の子にもなれる。位置が違えば型が違う
    /// ので、どちらの腕を作るかは呼び側が factory で渡す
    let internal createFire factory xml readActionElms =
        match xml with
        | Element(_, attrs, children) ->
            let fireattrs =
                {
                    FireAttrs.fireLabel = tryFindLabelValue attrs |> Option.map FireLabel
                }

            match tryFindBulletOrBulletRef children readActionElms with
            | Some bullet -> factory (fireattrs, tryFindDirection children, tryFindSpeed children, bullet)
            | None ->
                new BulletmlDTDViolationException("Fire element should have Bullet or BulletRef element.")
                |> raise
        | _ -> notSupported ()

    /// XmlNode to Action.FireRef
    ///
    /// DTD :
    /// <!ELEMENT fireRef (param* )>
    /// <!ATTLIST fireRef label CDATA #REQUIRED>
    let internal createFireRef xml =
        createRef "FireRef" (fun label -> { fireRefLabel = FireLabel label }) Action.FireRef xml

    /// XmlNode to Action.Accel
    ///
    /// DTD :
    /// <!ELEMENT accel (horizontal?, vertical?, term)>
    let internal createAccel =
        function
        | Element(elementName, attrs, children) ->
            refuseAttributes elementName attrs

            let horizontal = tryFindHorizontal children
            let vertical = tryFindVertical children
            Action.Accel(horizontal, vertical, createTerm children "accel")
        | _ -> notSupported ()

    /// XmlNode to ChangeSpeed
    ///
    /// DTD :
    /// <!ELEMENT changeSpeed (speed, term)>
    let internal createChangeSpeed =
        function
        | Element(elementName, attrs, children) ->
            refuseAttributes elementName attrs

            let speed =
                match tryFindSpeed children with
                | Some speed -> speed
                | None ->
                    new BulletmlDTDViolationException(
                        sprintf "this element should have Speed element.:[%s]" elementName
                    )
                    |> raise

            Action.ChangeSpeed(speed, createTerm children "changeSpeed")
        | _ -> notSupported ()

    /// XmlNode to ChangeDirection
    ///
    /// DTD :
    /// <!ELEMENT changeDirection (direction, term)>
    let internal createChangeDirection =
        function
        | Element(elementName, attrs, children) ->
            refuseAttributes elementName attrs

            let direction =
                match tryFindDirection children with
                | Some direction -> direction
                | None ->
                    new BulletmlDTDViolationException(
                        sprintf "this element should have Direction element.:[%s]" elementName
                    )
                    |> raise

            Action.ChangeDirection(direction, createTerm children "changeDirection")
        | _ -> notSupported ()

    /// XmlNode to Action.Wait
    ///
    /// DTD :
    /// <!ELEMENT wait (#PCDATA)>
    let internal createWait =
        function
        | Element(elementName, attrs, children) ->
            refuseAttributes elementName attrs

            match tryFindPCData children with
            | Some text -> Action.Wait(numExpr text)
            | None ->
                new BulletmlDTDViolationException(sprintf "[%s] element should have #PCDATA." elementName)
                |> raise
        | _ -> notSupported ()

    /// XmlNode to Action.Vanish
    ///
    /// DTD :
    /// <!ELEMENT vanish (#PCDATA)>
    let internal createVanish =
        function
        | Element(elementName, attrs, children) ->
            refuseAttributes elementName attrs

            match tryFindPCData children with
            | Some _ ->
                new BulletmlDTDViolationException(sprintf "this element cannot have #PCDATA.:[%s]" elementName)
                |> raise
            | None -> Action.Vanish
        | _ -> notSupported ()

    /// XmlNode to Action.Repeat
    ///
    /// DTD :
    /// <!ELEMENT repeat (times, (action | actionRef))>
    let internal createRepeat xml getChildren =
        match xml with
        | Element(elementName, attrs, children) ->
            refuseAttributes elementName attrs

            let actionOrActionRef =
                match tryFindActionOrActionRef children getChildren with
                | Some actionOrActionRef -> actionOrActionRef
                | None ->
                    new BulletmlDTDViolationException("repeat element should have Action or ActionRef.")
                    |> raise

            Action.Repeat(createTimes children "repeat", actionOrActionRef)
        | _ -> notSupported ()

    /// 子を位置の型で読む 3 本。落とし方は位置ごとに違う。ここは変えていない。
    /// fire だけ 2 位置に来るので、腕は factory で渡す。
    let rec internal readCommands (children: XmlNode list) : Action list =
        children
        |> List.choose (fun child ->
            match child with
            | PCData _ -> None
            | Element(name, _, _) ->
                match name.ToLower() with
                | "changedirection" -> createChangeDirection child |> Some
                | "changespeed" -> createChangeSpeed child |> Some
                | "accel" -> createAccel child |> Some
                | "wait" -> createWait child |> Some
                | "vanish" -> createVanish child |> Some
                | "repeat" -> createRepeat child readCommands |> Some
                | "fire" -> createFire (Action.Fire) child readActionElms |> Some
                | "fireref" -> createFireRef child |> Some
                | "action" -> createAction (Action.Action) child readCommands |> Some
                | "actionref" -> createActionRef (Action.ActionRef) child |> Some
                | _ -> None)

    and internal readActionElms (children: XmlNode list) : ActionElm list =
        children
        |> List.choose (fun child ->
            match child with
            | PCData _ -> None
            | Element(name, _, _) ->
                match name.ToLower() with
                | "action" -> createAction (ActionElm.Action) child readCommands |> Some
                | "actionref" -> createActionRef (ActionElm.ActionRef) child |> Some
                | _ -> None)

    and internal readTopElms (children: XmlNode list) : BulletmlElm list =
        children
        |> List.choose (fun child ->
            match child with
            | PCData _ -> None
            | Element(name, _, _) ->
                match name.ToLower() with
                | "bullet" -> createBullet (BulletmlElm.Bullet) child readActionElms |> Some
                | "fire" -> createFire (BulletmlElm.Fire) child readActionElms |> Some
                | "action" -> createAction (BulletmlElm.Action) child readCommands |> Some
                // 命令ではあるが bulletml の子になれないもの。上げる
                | "bulletml"
                | "actionref"
                | "fireref"
                | "changespeed"
                | "changedirection"
                | "accel"
                | "wait"
                | "vanish"
                | "bulletref"
                | "repeat" ->
                    new BulletmlDTDViolationException(sprintf "not support child element：[%s]" name)
                    |> raise
                | _ -> None)

    /// XML の木を BulletML の木にする。読めなければ上げる。
    ///
    /// 読めなかったかを値で受けたいときは `tryBulletmlFromXmlNode`。
    [<CompiledName("ConvertBulletmlFromXmlNode")>]
    let convertBulletmlFromXmlNode xml : Bulletml =
        match xml with
        | PCData _ ->
            new BulletmlDTDViolationException("root should be a bulletml element, not text.")
            |> raise
        | Element(name, _, _) ->
            if name.ToLower() = "bulletml" then
                createBulletml xml readTopElms
            else
                new BulletmlDTDViolationException(sprintf "root should be a bulletml element, not <%s>." name)
                |> raise

    /// 読めなければ None
    [<CompiledName("TryBulletmlFromXmlNode")>]
    let tryBulletmlFromXmlNode xml : Bulletml option =
        try
            xml |> convertBulletmlFromXmlNode |> Some
        with _ ->
            None


    /// 定数を畳む。$ を含まない式だけ数へ潰す。型が同じでも走査は残る。test は小数の書き方だけを変える。
    let private foldConstants' bulletml test =
        // 値は小数点 `.` で持ち回る。 F10 を既定カルチャで作ると `,` が混ざり、
        // 読み直す側が桁区切りと読んで落ちる
        let toStr (single: float32) =
            if test then
                single.ToString(CultureInfo.InvariantCulture)
            else
                single.ToString("F10", CultureInfo.InvariantCulture)

        /// $ を含まない式だけ畳む。乱数も難度も読まないので 0。読めない式で落ちるのは畳む側だけ。
        let foldEval (x: Expr.NumExpr) =
            if not (Expr.NumExpr.isReadable x) then
                new BulletmlDTDViolationException(sprintf "式として読めない:[%s]" (Expr.NumExpr.text x))
                |> raise

            Expr.NumExpr.evalWithValues 0.0f 0.0f x

        let rep (s: Expr.NumExpr) x (y: Lazy<'T>) =
            if (Expr.NumExpr.text s).Contains("$") then x else y.Force()

        let repDir direction =
            direction
            |> function
                | Some d ->
                    d
                    |> function
                        | Direction(a, x) -> rep x direction (lazy (Some(Direction(a, foldEval x |> toStr |> numExpr))))
                | None -> direction

        let repSpd speed =
            speed
            |> function
                | Some s ->
                    s
                    |> function
                        | Speed(a, x) -> rep x speed (lazy (Some(Speed(a, foldEval x |> toStr |> numExpr))))
                | None -> speed

        let repTimes times =
            times
            |> function
                | Times(x) -> rep x times (lazy (Times(foldEval x |> toStr |> numExpr)))

        let repTerm term =
            term
            |> function
                | Term(x) -> rep x term (lazy (Term(foldEval x |> toStr |> numExpr)))

        let repHorizontal horizontal =
            horizontal
            |> function
                | Some h ->
                    h
                    |> function
                        | Horizontal(a, x) ->
                            rep x horizontal (lazy (Some(Horizontal(a, foldEval x |> toStr |> numExpr))))
                | None -> horizontal

        let repVertical vertical =
            vertical
            |> function
                | Some v ->
                    v
                    |> function
                        | Vertical(a, x) -> rep x vertical (lazy (Some(Vertical(a, foldEval x |> toStr |> numExpr))))
                | None -> vertical

        let repDirOne direction =
            direction
            |> function
                | Direction(a, x) -> rep x direction (lazy (Direction(a, foldEval x |> toStr |> numExpr)))

        let repSpdOne speed =
            speed
            |> function
                | Speed(a, x) -> rep x speed (lazy (Speed(a, foldEval x |> toStr |> numExpr)))

        let repWait times =
            rep times times (lazy (foldEval times |> toStr |> numExpr))

        // 位置ごとに変換する。公開の Bulletml ファミリと走らせる木が同じ形を
        // しているので、腕が 1 対 1 に並ぶ
        let rec convertCommand (c: Action) : Action =
            match c with
            | ChangeDirection(direction, term) -> Action.ChangeDirection(repDirOne direction, repTerm term)
            | ChangeSpeed(speed, term) -> Action.ChangeSpeed(repSpdOne speed, repTerm term)
            | Accel(horizontal, vertical, term) ->
                Action.Accel(repHorizontal horizontal, repVertical vertical, repTerm term)
            | Vanish -> Action.Vanish
            | Wait times -> Action.Wait(repWait times)
            | Repeat(times, actionElm) -> Action.Repeat(repTimes times, convertActionElm actionElm)
            | Fire(attrs, direction, speed, bulletElm) ->
                Action.Fire(attrs, repDir direction, repSpd speed, convertBulletElm bulletElm)
            | FireRef(attrs, prams) -> Action.FireRef(attrs, prams)
            | Action.Action(attrs, commands) -> Action.Action(attrs, commands |> List.map convertCommand)
            | Action.ActionRef(attrs, prams) -> Action.ActionRef(attrs, prams)

        and convertActionElm (a: ActionElm) : ActionElm =
            match a with
            | ActionElm.Action(attrs, commands) -> ActionElm.Action(attrs, commands |> List.map convertCommand)
            | ActionElm.ActionRef(attrs, prams) -> ActionElm.ActionRef(attrs, prams)

        and convertBulletElm (b: BulletElm) : BulletElm =
            match b with
            | BulletElm.Bullet(attrs, direction, speed, actionElms) ->
                BulletElm.Bullet(attrs, repDir direction, repSpd speed, actionElms |> List.map convertActionElm)
            | BulletElm.BulletRef(attrs, prams) -> BulletElm.BulletRef(attrs, prams)

        let convertTopElm (t: BulletmlElm) : BulletmlElm =
            match t with
            | BulletmlElm.Bullet(attrs, direction, speed, actionElms) ->
                BulletmlElm.Bullet(attrs, repDir direction, repSpd speed, actionElms |> List.map convertActionElm)
            | BulletmlElm.Fire(attrs, direction, speed, bulletElm) ->
                BulletmlElm.Fire(attrs, repDir direction, repSpd speed, convertBulletElm bulletElm)
            | BulletmlElm.Action(attrs, commands) -> BulletmlElm.Action(attrs, commands |> List.map convertCommand)

        // 根は bulletml だけ（腕が 1 つ なので `| _ -> raise` が要らない）
        match bulletml with
        | Bulletml.Bulletml(attrs, elms) -> Bulletml.Bulletml(attrs, elms |> List.map convertTopElm)

    let internal foldConstants bulletml = foldConstants' bulletml false

    let internal foldConstantsForTest bulletml = foldConstants' bulletml true
