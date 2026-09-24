module FsBulletML2.Benchmarks.Program

open BenchmarkDotNet.Running
open FsBulletML2.Benchmarks.Harness

/// Debug では BenchmarkDotNet が止める。測るときは Release。
let private scenarios =
    [
        "move", "Content/xml/Enemy/move.xml"
        "5way", "Content/xml/EnemyBullet/5way.xml"
        "10Way", "Content/xml/EnemyBullet/10Way.xml"
        "homing", "Content/xml/EnemyBullet/[G_DARIUS]_homing_laser.xml"
        "wide", "Content/xml/EnemyBullet/sdmkun/bosses.d/[OtakuTwo]_dis_bee_1.xml"
    ]

/// 時間の物差しは先頭 4 本。wide は確保だけ。効きが出てから時間へ載せる。
let private onBdn = 4

/// 数を比べる前にここを見る。死が 0 の台本は、死んだコマの変更の対照。
let private counts () =
    fixManager ()
    printfn "台本ごとの step 呼び出し。60 コマ。"
    printfn "%-8s %10s %10s %10s %10s %10s   %s" "台本" "生" "死" "死の割合" "Env 構築" "aim を組む" "この台本が使えるもの"

    for name, suffix in scenarios do
        match Corpus.findBySuffix suffix with
        | None -> printfn "%-8s %s が見つかりません" name suffix
        | Some path ->
            let doc = parseXml (System.IO.File.ReadAllText path)
            let live, dead = countLiveDead doc 60
            let envs = countEnvBuilds doc 60
            let total = live + dead
            let ratio = if total = 0 then 0.0 else float dead / float total * 100.0

            printfn
                "%-8s %10d %10d %9.1f%% %10d %10d   %s"
                name
                live
                dead
                ratio
                envs
                live
                (if dead = 0 then "対照（死んだコマの変更は届かない）" else "対象")

    printfn ""
    printfn "「Env 構築」は Env を組む回数（step の前と、走らせ直しの前）。**掛けてはいけない。**"
    printfn "この列は HasNoScript を通さずに数えている。実際に計時している経路は"
    printfn "HasNoScript で aim 4 本 を 0 に置き換えるので、Atan2 を回すのは「aim を組む」の側だけ。"
    printfn ""
    printfn "**Env を遅延にしたときの天井 = SetupBenchmarks の「Env を 1 回 組む」×「aim を組む」。**"
    printfn "遅延にしても実際に読まれるぶんは残るので、その積は上界。"
    printfn ""
    printfn "以前ここは「Env 構築」を掛けろと書いてあり、5way で 60 倍 の見積もりを出していた。"
    printfn "死んだコマが 0 の台本（move / homing）では 2 つの列が一致するので、"
    printfn "**対照の側だけが合っていて、対象の側だけが外れる**という形で隠れていた。"

/// 新 API の絶対値。走行のたびに記録からの差を出す。
let private baselineAt = "572086a（旧 API 廃止。BulletRunner を消し、FakeBullet を小さくした版）"

let private baseline =
    dict
        [
            "move", 313_320L
            "5way", 3_593_552L
            "10Way", 7_465_968L
            "homing", 4_120_840L
            "wide", 3_157_808L
        ]

/// BenchmarkDotNet を通さない。時間は測れない。物差しは `baseline` からの差。
let private alloc () =
    fixManager ()
    printfn "1 走行（60 コマ）の確保。BenchmarkDotNet を通さない即席の物差し。"
    printfn ""
    // 1 巡 空けて 2 巡目 を出す。空けないと軽い台本が段階的 JIT で動く。
    let measure () =
        [
            for name, suffix in scenarios do
                match Corpus.findBySuffix suffix with
                | None -> ()
                | Some path ->
                    let doc = parseXml (System.IO.File.ReadAllText path)
                    yield name, allocApi doc 60, bulletCount doc 60
        ]

    measure () |> ignore
    let rows = measure ()

    for name, suffix in scenarios do
        if (Corpus.findBySuffix suffix).IsNone then
            printfn "%-8s %s が見つかりません" name suffix

    printfn "%-8s %14s %14s %10s %9s %7s %9s" "台本" "新 API" "記録" "差" "差(%)" "弾" "差/弾"

    for name, a, n in rows do
        match baseline.TryGetValue name with
        | true, b ->
            let d = a - b
            let perBullet = if n = 0 then "—" else sprintf "%+.1f B" (float d / float n)
            printfn "%-8s %12d B %12d B %+9d B %+8.2f%% %7d %9s" name a b d (float d / float b * 100.0) n perBullet
        | _ ->
            // 記録の無い台本を 0 や「一致」で埋めない。 埋めると、
            // 控えていない列を控えたものと読む
            printfn "%-8s %12d B %14s %10s %9s %7d %9s" name a "—" "—" "—" n "—"

    printfn ""
    // 0 件 を緑にしない。台本が 1 本 も見つからなければ表は空のまま通る
    printfn "測った台本 %d 本（%d 本 中）。0 なら測れていない。" (List.length rows) (List.length scenarios)
    printfn "うち %d 本 は BDN の物差しにも載っている（%d 本 目 以降は確保だけ）。" onBdn (onBdn + 1)
    printfn ""
    printfn "**「差/弾」が 5 本 とも同じ値なら、動いたのは測定器のほう。**"
    printfn "この口の確保には、測定器自身が作る FakeBullet のぶんも入っている。"
    printfn "実際に踏んだ —— FakeBullet から旧 API の面を落としただけで 5 本 とも下がった。"
    printfn ""
    printfn "**1 巡 空けて 2 巡目 を出している。** それでも再現性は 0 にならない ——"
    printfn "段階的 JIT の段が走行ごとに変わりうるため（実測: 1 巡だと move が 2.0%%、"
    printfn "2 巡だと 0.1%% 未満）。**0.1%% 程度の差は雑音。1%% 未満は読まないこと。**"
    printfn ""
    printfn "記録は %s で、この口自身が出した値。" baselineAt
    printfn "**正しさの門ではない。** Core を触れば動くのが正しく、動いたときに"
    printfn "それが手の効きなのか、無関係な変更に伴うドリフト（既知の幅 4,120 B）"
    printfn "なのかを読むための目印。**手を打ったら Program.fs の baseline と"
    printfn "baselineAt を更新する。**"
    printfn ""
    printfn "**絶対値は BenchmarkDotNet の Allocated と一致しない。** あちらは"
    printfn "ウォームアップ後の定常状態を測り、こちらは下ごしらえ（木を組む段）を"
    printfn "1 回ぶん 含む。使うのは差だけ。"
    printfn ""
    printfn "**旧 API の列は落とした。** 対照に見えて対照ではなかった —— 旧い口を"
    printfn "新経路の上に載せたシムで、中では同じ Step.step を通っていた。"
    printfn "軸（この口の差 vs BDN の Allocated の差）は 3 回 確かめてあり、"
    printfn "どれも旧の列に依っていない。README の「測るときの約束」を見ること。"

[<EntryPoint>]
let main argv =
    if argv |> Array.contains "--counts" then
        counts ()
        0
    elif argv |> Array.contains "--alloc" then
        alloc ()
        0
    else
        BenchmarkSwitcher.FromAssembly(typeof<StepBenchmarks>.Assembly).Run(argv)
        |> ignore

        0
