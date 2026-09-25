module FsBulletML2.Core.Tests.PartsBorrow

open NUnit.Framework
open FsUnit
open FsBulletML2

/// 同梱 の弾幕 から 仕掛け を抜く ところ。
[<TestFixture>]
type PartsBorrow() =

    static let read (xml: string) : Bulletml =
        Bulletml.ReadXmlString("<?xml version=\"1.0\" ?><bulletml type=\"vertical\">" + xml + "</bulletml>")

    static let xml (b: Bulletml) = BulletmlWriter.toIndentedXml 4 b

    static let count (needle: string) (s: string) =
        (s.Length - s.Replace(needle, "").Length) / needle.Length

    /// 弾 の中 に 動き が在る 形。同梱 の多く が これ
    static let curving =
        """<action label="top"><fire><bulletRef label="core"/></fire></action>
       <bullet label="core"><speed>2</speed><action label="turn">
         <changeDirection><direction type="aim">0</direction><term>30</term></changeDirection>
         <action label="hold"><wait>20</wait></action>
       </action></bullet>"""

    /// 撃つ 枝 を持つ 弾。抜く と 掛け算 に なる
    static let firing =
        """<action label="top"><fire><bulletRef label="core"/></fire></action>
       <bullet label="core"><speed>1</speed><action>
         <changeSpeed><speed>3</speed><term>10</term></changeSpeed>
         <fire><bullet/></fire>
       </action></bullet>"""

    [<Test>]
    member _.``仕掛け の字 が 5 通り 別 に読める``() =
        Parts.ofString "curve" |> should equal Parts.Curve
        Parts.ofString "accel" |> should equal Parts.Accelerate
        Parts.ofString "charge" |> should equal Parts.Charge
        Parts.ofString "fade" |> should equal Parts.Fade
        Parts.ofString "drift" |> should equal Parts.Drift
        // 空 も none も 知らない 字 も 借りない。曲がる に倒す と 全部 の弾幕 の終点 が曲がって いた
        Parts.ofString "none" |> should equal Parts.Idle
        Parts.ofString "" |> should equal Parts.Idle
        Parts.ofString "spin" |> should equal Parts.Idle

    [<Test>]
    member _.``借りない なら 何 が在って も 見つけない``() =
        Parts.find Parts.Idle [ read curving; read firing ] |> should equal None

    /// 弾 の中 の 動き を拾う。本物 の仕掛け は 弾 の中 に在る
    [<Test>]
    member _.``弾 の中 の 動き を抜く``() =
        let got = Parts.extract (read curving)
        got |> List.map fst |> should contain Parts.Curve

    /// `find` は 長さ が同じ なら 先 に拾った ほう を返す。拾う 順 が 外 と 内 で入れ替わる と 借りる 仕掛け が変わる
    [<Test>]
    member _.``入れ子 の動き は 外 から 順 に拾う``() =
        let nested =
            """<action label="top"><fire><bulletRef label="core"/></fire></action>
         <bullet label="core"><speed>1</speed><action>
           <action><accel><horizontal>1</horizontal><term>5</term></accel>
             <action><changeSpeed><speed>2</speed><term>5</term></changeSpeed></action>
           </action>
         </action></bullet>"""

        Parts.extract (read nested)
        |> List.map fst
        |> should equal [ Parts.Accelerate; Parts.Accelerate; Parts.Drift ]

    /// 撃つ 枝 は 入れない —— `Combine.Inside` と同じ 掛け算 に なる
    [<Test>]
    member _.``撃つ 枝 は抜かない``() =
        Parts.extract (read firing) |> should be Empty

    /// 外 を指す 参照 も 入れない。抜いた 先 に その 名前 が無く、迷子 に なる
    [<Test>]
    member _.``参照 を持つ 枝 は抜かない``() =
        let refs =
            """<action label="top"><fire><bulletRef label="core"/></fire></action>
         <action label="more"><wait>10</wait></action>
         <bullet label="core"><speed>1</speed><action>
           <changeSpeed><speed>3</speed><term>10</term></changeSpeed>
           <actionRef label="more"/>
         </action></bullet>"""

        Parts.extract (read refs) |> should be Empty

    /// 1 つ の枝 は 1 つ の種類 に落ちる。曲がる が いちばん 強い ——
    /// 曲がり ながら 加速 する 弾 は「曲がる」として 借りたい
    [<Test>]
    member _.``種類 は 1 つ に決まる``() =
        let both =
            """<action label="top"><fire><bulletRef label="core"/></fire></action>
         <bullet label="core"><speed>1</speed><action>
           <changeDirection><direction type="aim">0</direction><term>20</term></changeDirection>
           <accel><vertical>0.5</vertical><term>30</term></accel>
         </action></bullet>"""

        Parts.extract (read both) |> List.map fst |> should equal [ Parts.Curve ]

    /// 1 手 だけ の枝 は どの弾幕 にも在り、借りて も 形 が変わらない
    [<Test>]
    member _.``長い ほう を採る``() =
        let short =
            read
                """<action label="top"><fire><bulletRef label="core"/></fire></action>
              <bullet label="core"><speed>1</speed><action>
                <changeDirection><direction>10</direction><term>5</term></changeDirection>
              </action></bullet>"""

        match Parts.find Parts.Curve [ short; read curving ] with
        | None -> failwith "1 つ も無い"
        | Some body -> List.length body |> should equal 2

    /// 終点 の弾 に足す。歩き方 は `Combine` と共通
    [<Test>]
    member _.``終点 に足す``() =
        let target = read """<action label="top"><fire><bullet/></fire></action>"""

        match Parts.find Parts.Curve [ read curving ] with
        | None -> failwith "1 つ も無い"
        | Some body ->
            let got = xml (Parts.graft Parts.Leaf body target)
            got |> should haveSubstring ("<action label=\"" + Parts.MARK + "\"")
            got |> should haveSubstring "<changeDirection>"
            // 印 は 1 つ。終点 が 1 つ しか 無い 弾幕 に 2 回 足さない
            count ("label=\"" + Parts.MARK + "\"") got |> should equal 1

    /// 同じ 動き を 終点 ぜんぶ に配る ので、label を持った まま だと
    /// 同じ 名前 が 何度 も 現れる
    [<Test>]
    member _.``借りた 枝 の名前 は落ちる``() =
        let target =
            read """<action label="top"><fire><bullet/></fire><fire><bullet/></fire></action>"""

        match Parts.find Parts.Curve [ read curving ] with
        | None -> failwith "1 つ も無い"
        | Some body ->
            let got = xml (Parts.graft Parts.Leaf body target)
            got |> should not' (haveSubstring "\"turn\"")
            got |> should not' (haveSubstring "\"hold\"")
            // 終点 が 2 つ なら 印 も 2 つ
            count ("label=\"" + Parts.MARK + "\"") got |> should equal 2

    /// 1 つ も無い 種類 を頼まれた ら そのまま 返す。
    /// 「作れなかった」に しない —— 押した 手 は成立 して いる
    [<Test>]
    member _.``無い 仕掛け では 弾幕 が変わらない``() =
        let target = read """<action label="top"><fire><bullet/></fire></action>"""
        Parts.find Parts.Accelerate [ read curving ] |> should equal None
        xml (Parts.graft Parts.Leaf [] target) |> should equal (xml target)

    /// 終点 だけ に置く と、段 を持つ 弾幕 では 割れた あと の 破片 しか 動かない
    [<Test>]
    member _.``どこ に仕込む か で 当たる 弾 が変わる``() =
        // 撃つ 枝 を持つ 弾（core）と、持たない 弾（葉）の 2 段
        let two =
            read
                """<action label="top"><fire><bulletRef label="core"/></fire></action>
              <bullet label="core"><speed>2</speed><action>
                <fire><bullet/></fire>
              </action></bullet>"""

        match Parts.find Parts.Curve [ read curving ] with
        | None -> failwith "1 つ も無い"
        | Some body ->
            // 終点 だけ。撃つ 枝 を持つ 弾 は 動かない
            count ("label=\"" + Parts.MARK + "\"") (xml (Parts.graft Parts.Leaf body two))
            |> should equal 1
            // どの 弾 にも。段 の途中 も 動き出す
            count ("label=\"" + Parts.MARK + "\"") (xml (Parts.graft Parts.Every body two))
            |> should equal 2

    /// 撃たれて すぐ と、少し 飛んで から。短い と 見分け が つかない
    [<Test>]
    member _.``少し 飛んで から も選べる``() =
        let target = read """<action label="top"><fire><bullet/></fire></action>"""

        match Parts.find Parts.Curve [ read curving ] with
        | None -> failwith "1 つ も無い"
        | Some body ->
            let soon = xml (Parts.graft Parts.Leaf body target)
            let late = xml (Parts.graft Parts.Late body target)
            soon |> should not' (haveSubstring ("<wait>" + Parts.LATE_WAIT + "</wait>"))
            late |> should haveSubstring ("<wait>" + Parts.LATE_WAIT + "</wait>")

    /// 知らない 字 は 終点 に置く
    [<Test>]
    member _.``仕込む 先 の字 が 3 通り 別 に読める``() =
        Parts.whereOfString "leaf" |> should equal Parts.Leaf
        Parts.whereOfString "late" |> should equal Parts.Late
        Parts.whereOfString "every" |> should equal Parts.Every
        Parts.whereOfString "nowhere" |> should equal Parts.Leaf

    /// 参照（`actionRef`）は 弾いて いた のに、その 引数 だけ が 残って いた。
    [<Test>]
    member _.``引数 を持つ 枝 は抜かない``() =
        let parametrized =
            """<action label="top"><actionRef label="arm"><param>30</param></actionRef></action>
         <action label="arm"><fire><bulletRef label="core"/></fire></action>
         <bullet label="core"><speed>1</speed><action>
           <wait>$1</wait>
           <changeSpeed><speed>3</speed><term>10</term></changeSpeed>
         </action></bullet>"""

        Parts.extract (read parametrized) |> should be Empty

    /// `wait` だけ 見て いた のでは `term` に潜った 引数 を 見落とす
    [<Test>]
    member _.``引数 は どの欄 に在って も 弾く``() =
        let inTerm =
            """<action label="top"><fire><bulletRef label="core"/></fire></action>
         <bullet label="core"><speed>1</speed><action>
           <changeSpeed><speed>3</speed><term>$2</term></changeSpeed>
           <wait>10</wait>
         </action></bullet>"""

        Parts.extract (read inTerm) |> should be Empty

        let inRepeat =
            """<action label="top"><fire><bulletRef label="core"/></fire></action>
         <bullet label="core"><speed>1</speed><action>
           <repeat><times>$1</times><action><wait>3</wait></action></repeat>
           <changeSpeed><speed>3</speed><term>10</term></changeSpeed>
         </action></bullet>"""

        Parts.extract (read inRepeat) |> should be Empty

        // `$rand` と `$rank` は 引数 ではない。抜いた 先 でも 値 が決まる
        let seeded =
            """<action label="top"><fire><bulletRef label="core"/></fire></action>
         <bullet label="core"><speed>1</speed><action>
           <changeSpeed><speed>$rand * 3</speed><term>10 - $rank * 4</term></changeSpeed>
           <wait>20</wait>
         </action></bullet>"""

        Parts.extract (read seeded) |> should not' (be Empty)
