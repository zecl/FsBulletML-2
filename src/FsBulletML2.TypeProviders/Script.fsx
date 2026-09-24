#r @".\bin\Debug\FsBulletML2.TypeProvider.dll"
#r @".\bin\Debug\FsBulletML2.Core.dll"
#r @".\bin\Debug\FsBulletML2.Parser.dll"
#r @".\bin\Debug\System.Xml.dll"

open FsBulletML2.TypeProviders.Xml

[<Literal>]
let err =
    """
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" type="vertical">
    <action label="top">
        <fire label="test">
            <direction>2</direction>
            <speed>4</speed>
        </fire>
    </action>
</bulletml>
"""

[<Literal>]
let xml =
    """
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" type="vertical">
    <action label="top">
        <fire label="test">
            <speed>2</speed>
            <bullet />
        </fire>
    </action>
</bulletml>
"""

[<Literal>]
let aaa =
    "<bulletml type='horizontal'><action label='top'><fire><bullet/></fire></action></bulletml>"

type BulletML = BulletMLProvider<xml>
let bulletml1 = new BulletML()
let hoge = bulletml1.Value
hoge |> printfn "%A"

//type BulletML2 = BulletMLProvider<err>
//let bulletml2 = new BulletML2()
//let hoge = bulletml1.Value
//hoge |> printfn "%A"


[<Literal>]
let sxml =
    """
(bulletml (@ (xmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml") (type "vertical"))
    (action (@ (label "top"))
        (fire
            (direction "-20")
            (bullet)
        )
        (repeat
            (times "4")
            (action
                (fire
                    (direction (@ (type "sequence")) "10")
                    (bullet)
                )
            )
        )
    )
)
"""

type SXML2 = FsBulletML2.TypeProviders.Sxml.BulletMLProvider<sxml>
let bulletml2 = new SXML2()
bulletml2.Value |> printfn "%A"

[<Literal>]
let fsb =
    """bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" type="vertical"
      action label="top"
          accel
              vertical:"2"
              term:"5"
          fire
              bullet
                  action
                      changeDirection
                          direction:"3"
                          term:"3"
  """

type FSB = FsBulletML2.TypeProviders.Fsb.BulletMLProvider<fsb>
let bulletml3 = new FSB()
bulletml3.Value |> printfn "%A"


System.Console.ReadKey() |> ignore
