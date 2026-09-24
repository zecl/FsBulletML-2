/// どの要素がどの要素をどの属性で参照するか。1 本。表を持たない。
/// 規則を 2 か所 に書かない。書くと波線と rename がずれる。
module FsBulletML2.LanguageService.Refs

/// 名前が `Ref` で終わるか。
/// `EndsWith` は使うな。引数 1 つの版は現在のカルチャで比べる。
let private endsWithRef (name: string) =
    name.Length > 3 && name.Substring(name.Length - 3) = "Ref"

/// (参照する要素, 参照される要素, label の属性名) の並び。
/// 空なら呼ぶ側は何も挙げない。語彙が引けていない印。
let pairs (elements: (string * string list) list) : (string * string * string) list =
    let attrsOf name =
        elements
        |> List.tryPick (fun (n, attrs) -> if n = name then Some attrs else None)

    elements
    |> List.choose (fun (name, attrs) ->
        if not (endsWithRef name) then
            None
        else
            match attrsOf (name.Substring(0, name.Length - 3)) with
            | None -> None
            | Some defAttrs ->
                attrs
                |> List.tryPick (fun a ->
                    if List.contains a defAttrs then
                        Some(name, name.Substring(0, name.Length - 3), a)
                    else
                        None))

/// 定義に無い参照 1 つ。人へ見せる文面は持たない。
/// 持たせると host の波線とブラウザの Quick Fix で文面が割れる。
type Missing =
    {
        /// 参照している要素（`actionRef`）
        RefName: string
        /// 参照されるはずの要素（`action`）
        DefName: string
        /// 名前が載っている属性（`label`）
        AttrName: string
        /// その参照の属性そのもの。位置も値もここに在る
        Hit: AttrHit
        /// その本文に在る、その種類の定義の名前。「近い名前」の材料 ——
        /// 空なら直し方は作れない（作る側がそう読む）
        Defined: string list
    }

/// 定義に無い参照を全部。本文に出てくる順。
/// 同じ名前を 2 回 参照していたら 2 本。どちらも直す先。
let missing (pairs: (string * string * string) list) (tags: TagHit list) : Missing list =
    // 開始札だけ見る。 閉じ札は XML だけが返すもので、属性を持たない
    let opens = tags |> List.filter (fun t -> not t.Closing)

    [
        for (refName, defName, attrName) in pairs do
            let defined =
                opens
                |> List.filter (fun t -> t.TagName = defName)
                |> List.choose (fun t -> t.Attrs |> List.tryFind (fun a -> a.AttrName = attrName))
                |> List.map (fun a -> a.Value)
                |> List.distinct

            for t in opens do
                if t.TagName = refName then
                    match t.Attrs |> List.tryFind (fun a -> a.AttrName = attrName) with
                    | Some a when not (List.contains a.Value defined) ->
                        yield
                            {
                                RefName = refName
                                DefName = defName
                                AttrName = attrName
                                Hit = a
                                Defined = defined
                            }
                    | _ -> ()
    ]
    // 対の順ではなく本文の順に並べ直す。
    |> List.sortBy (fun m -> m.Hit.Line, m.Hit.Column)

/// 定義 1 つ と、それを指している参照の数（v4.3）。`missing` の裏返し。
/// 字を持たない。表記ごとに書き方が違う。
type DefUse =
    {
        /// 定義の要素（`action`）
        DefName: string
        /// 参照する側の要素（`actionRef`）。呼ぶ側が文面に使う
        RefName: string
        /// その名前
        Name: string
        /// 名前が載っている属性そのもの。位置はここに在る
        Hit: AttrHit
        /// その名前を指している参照の数。定義そのものは数えない
        Uses: int
    }

/// 定義を全部、参照の数つきで。本文に出てくる順。
/// 同じ名前が 2 つ あっても、どちらを指しているかは言わない。
let uses (pairs: (string * string * string) list) (tags: TagHit list) : DefUse list =
    let opens = tags |> List.filter (fun t -> not t.Closing)

    [
        for (refName, defName, attrName) in pairs do
            // 参照側の名前を数える。1 対 につき 1 回 走る
            let used =
                opens
                |> List.filter (fun t -> t.TagName = refName)
                |> List.choose (fun t -> t.Attrs |> List.tryFind (fun a -> a.AttrName = attrName))
                |> List.map (fun a -> a.Value)

            for t in opens do
                if t.TagName = defName then
                    match t.Attrs |> List.tryFind (fun a -> a.AttrName = attrName) with
                    | Some a ->
                        yield
                            {
                                DefName = defName
                                RefName = refName
                                Name = a.Value
                                Hit = a
                                Uses = used |> List.filter (fun v -> v = a.Value) |> List.length
                            }
                    | None -> ()
    ]
    |> List.sortBy (fun d -> d.Hit.Line, d.Hit.Column)

/// 定義が取る引数の数と、参照が渡している数（v4.5）。
/// 数えるのは最大の `$n`。`$1` が飛んでも減らない。
type Arity =
    {
        /// 定義の要素（`action`）
        DefName: string
        /// その名前
        Name: string
        /// 名前が載っている属性そのもの
        Hit: AttrHit
        /// 本文で使われているいちばん大きい `$n`。1 つ も無ければ 0 ——
        /// `$rand` / `$rank` は数えない（`$` のあとが数字のものだけ）
        Takes: int
    }

/// 本文の一部 のいちばん大きい `$n`。`$rand` と `$rank` は数えない。
/// 数字チェックは冗長だが残す。外すと `$rank` の `r` を跨ぎかねない。
let private maxParamIn (src: string) (from: int) (until: int) =
    let stop = min until src.Length
    let mutable i = max 0 from
    let mutable best = 0

    while i < stop do
        if src.[i] = '$' && i + 1 < stop && src.[i + 1] >= '0' && src.[i + 1] <= '9' then
            let mutable j = i + 1
            let mutable v = 0

            while j < stop && src.[j] >= '0' && src.[j] <= '9' do
                v <- v * 10 + int src.[j] - int '0'
                j <- j + 1

            if v > best then
                best <- v

            i <- j
        else
            i <- i + 1

    best

/// 定義を全部、取る引数の数つきで。本文に出てくる順。
/// 範囲は深さ。最後の定義は本文の終わりまで。
let arity (pairs: (string * string * string) list) (src: string) (tags: TagHit list) : Arity list =
    let all = tags |> List.toArray
    let defNames = pairs |> List.map (fun (_, defName, attrName) -> defName, attrName)
    // 内包の中で `while` を回さない。Fable が enumerator の鎖に焼く。
    let out = ResizeArray<Arity>()

    for i in 0 .. all.Length - 1 do
        let t = all.[i]

        if not t.Closing then
            match defNames |> List.tryFind (fun (n, _) -> n = t.TagName) with
            | None -> ()
            | Some(_, attrName) ->
                match t.Attrs |> List.tryFind (fun a -> a.AttrName = attrName) with
                | None -> ()
                | Some a ->
                    let mutable k = i + 1

                    while k < all.Length && all.[k].Depth > t.Depth do
                        k <- k + 1

                    let until = if k < all.Length then all.[k].Start else src.Length

                    out.Add
                        {
                            DefName = t.TagName
                            Name = a.Value
                            Hit = a
                            Takes = maxParamIn src t.Stop until
                        }

    List.ofSeq out
