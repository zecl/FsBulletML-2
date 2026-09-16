/// 表記ごとの口。次の言語を足すとき触るのは言語モジュール 1 個。
/// JSON を読むところはここに無い。形の正本はこちら。
module FsBulletML2.LanguageService.SourceLanguage

/// 雛形の骨（v2.6）。値も属性もここが持つ。
///
/// 値は Monaco の snippet の穴（`$1` `$2` …。`$0` が最後にカーソルを置く場所）。
///
/// 表記を知らない。 どう字にするかは `Shape.WriteFrame` の側で、
/// 骨は 3 表記 で 1 つ —— 雛形を増やしても表記ごとの手は増えない。
type Frame =
  { Element: string
    Attrs: (string * string) list
    /// 中身の字。子が在れば空（BulletML の要素は、値か子のどちらか）
    Text: string
    Children: Frame list }

/// 雛形 1 つ（v2.6）。host が渡す ——
/// 骨は要素名の木なので、器に書くと「ブラウザ側に要素名を書かない」線を越える
type FrameSnippet =
  { /// 候補に出す名前。要素名は既に候補に出ているので、ここに並ぶのは形の名前
    Label: string
    /// どの要素の中で出すか。空 は出さない
    In: string list
    /// 何本 が使っているか。候補の脇に出す
    Detail: string
    Frame: Frame }

type VocabAttr =
  { Name: string
    Values: string list
    /// 書かなかったときに走る値。並びで持つ（1 個 に絞れているかは host 側 の門）
    Defaults: string list
    /// `<!ATTLIST ...>` 行
    Dtd: string
    /// hover に出す散文
    Spec: string
    /// 値ごとの散文
    ValueSpecs: (string * string) list }

type VocabElement =
  { Name: string
    Children: string list
    Attrs: VocabAttr list
    /// #PCDATA を取るか。取る要素の中では式（$rand / $rank）も候補になる
    Text: bool
    /// `<!ELEMENT ...>` 行
    Dtd: string
    /// hover に出す散文
    Spec: string }

/// F# の CE の名前 1 つ が、BulletML の何を作るか。
///
/// 散文を持たない（出す字は `Elements` の側から引く。持たせると
/// 同じ説明が 2 か所 に在る）。
/// 同じ名前が何個 在ってもよい。 正本は host の `Spec.ce`
type VocabCe =
  { Name: string
    /// 作る要素
    Element: string
    /// 固定する属性。固定しないなら空
    Attr: string
    /// 固定する値。同上
    Value: string }

/// F# の CE の名前 1 つ が、どの要素の label を載せるか。
///
/// `VocabCe` と別に持つ —— あちらは「何を作るか」で、こちらは
/// 「名前を決めるか、使うか」（`repeatAs` は `<repeat>` を作るが、
/// 名前が付くのは中の `<action>`）。正本は host の `Spec.ceLabels`
type VocabCeLabel =
  { Name: string
    /// label の付く要素。参照側は `Ref` の付いた要素名
    Element: string
    /// 名前が、その CE 名 のあとに続く何番目 の文字列リテラルか（0 起点）。
    /// 引数を取らないなら -1
    LabelArg: int
    /// 引数を取らないときの名前。取るなら空
    Fixed: string
    /// 根の直下 に書ける名前か。「定義を作る」がこれで選ぶ
    /// （同じ要素を作る名前が複数 在るので、要素だけでは選べない）
    Root: bool }

/// F# の CE の名前 1 つ が、どこに置けて、何を開くか。
///
/// 置ける先は要素ではなく「入れ物の種類」 —— `repeat` の中に置けるものは
/// `action` の中と同じなので、要素で分けると `repeat` の中で候補が 0 個 になる。
///
/// 正本は host の `Vocabulary.cePlaces`。表ではなく `Dsl` から reflection で引く
type VocabCePlace =
  { Name: string
    /// 置ける入れ物の種類。空 は「どこにも置けない」
    In: string
    /// この名前が開く `{ }` の種類。開かないなら空
    Opens: string }

type Vocab =
  { Elements: VocabElement list
    /// 式の中で使える字
    Expressions: string list
      /// 雛形（v2.6）。host が焼いて渡す
    Frames: FrameSnippet list
  /// F# の CE の名前。この表記でだけ引く
    Ce: VocabCe list
    /// CE の名前が載せる label。同上
    CeLabels: VocabCeLabel list
    /// CE の名前を、どこに置けるか。候補（`Complete`）がこれで決まる
    CePlaces: VocabCePlace list
    /// 根から走る定義の名前の頭（`top`）。この綴りは Core が持っている ——
    /// 器に書き写すと、あちらを変えたときに黙って割れる。
    ///
    /// 空なら、意味の検査は `top` の話を何も出さない（`Semantics`）
    TopPrefix: string }

/// 候補 1 つ。
///
/// `Replace` を言語モジュールが決める。 Monaco の「語」に任せると、
/// `$rand` の `$` が語に入らない版で `$$rand` になる ——
/// 何が語かを知っているのは言語のほう
type Completion =
  { Label: string
    /// 入れる字。`Snippet` なら Monaco の記法（`$0` がカーソル）
    Insert: string
    Snippet: bool
    /// 雛形か（v2.6）。`Snippet` と別に持つ（属性の候補も snippet 記法で
    /// 入るので、あちらでは「形の候補」と分けられない）
    IsFrame: bool
    /// カーソルの手前 何文字 を置き換えるか
    Replace: int }

module Completion =
  let plain (replace: int) (label: string) =
    { Label = label; Insert = label; Snippet = false; IsFrame = false; Replace = replace }

/// 同じ名前が書いてある 1 か所。位置は 1 起点（Monaco の行桁と同じ）で、
/// 指すのは名前の中身（引用符の内側）。
///
/// rename ではなく「どこに書いてあるか」を返す ——
/// 字を差し替えるのは言語の知識ではない。そのぶん rename 以外
/// （今いる名前を光らせる・参照を一覧する）にも使える。
type Usage =
  { Line: int
    Column: int
    EndColumn: int
    /// いま書いてある字。どの `Usage` でも同じ
    Text: string
    /// 名前を決めている側か（`action label="a"`）。
    /// 参照している側（`actionRef label="a"`）なら false。
    ///
    /// 1 欄 で足りることは測ってある（定義側と参照側で要素名が
    /// 1 つ も重ならない。3 組 / 重なり 0）。
    ///
    /// 同じ本文に定義が 2 つ 在ることは在る —— 読む側は
    /// 「1 つ に決まる」と思ってはいけない
    IsDefinition: bool }

/// 直し方 1 つ。位置は 1 起点（Monaco の行桁と同じ）。
///
/// `Column = EndColumn` なら置き換えではなく挿し込み（幅 0 の範囲）。
///
/// 波線に紐づけない。 波線は 1 文字 打った時点で消えるので、
/// 紐づけると直し方が Apply の直後の窓でしか出ない
type Fix =
  { /// メニューに出す字
    Title: string
    Line: int
    Column: int
    EndColumn: int
    /// そこへ書く字
    Text: string }

/// 定義の行の上に出す字 1 つ（v4.3）。位置は 1 起点。
///
/// 字は器が組む（「3 か所 から参照」と「根から走る」のどちらを出すかを
/// 決めているのが器の側）。
type Lens =
  { /// 出す字
    Title: string
    /// 定義の名前。押したときに飛ぶ先を決めるのに要る
    Name: string
    /// 1 起点。名前が載っている属性の位置
    Line: int
    Column: int
    /// 押せるか。 参照が 0 なら開く先が無いので押せない
    Clickable: bool }

/// 値を横に出す先 1 つ（v4.4）。位置は 1 起点。
///
/// 値はここに入らない。 評価するのは面（`$rank` を持っている側）で、
/// 器は評価器を持たない —— 持つと同じ式が 2 通り の値になる
type ExprSpot =
  { /// 式そのもの
    Text: string
    /// その式が書いてある要素
    Element: string
    /// 出す位置（式の終わりの次）
    Line: int
    Column: int }

/// 参照が渡す引数の形（v4.5）。カーソルが `◯◯Ref` の中に居るときだけ。
///
/// 数だけでなく、いま何番目 に居るかも返す。 どちらか片方 だと
/// 「3 つ 取る」を見ながら 4 つ 目 を打てる。
type Signature =
  { /// 見出し（`shot($1, $2)`）
    Label: string
    /// 見出しの中の引数の範囲（0 起点、`Stop` は含まない）。並びの順
    Params: (int * int) list
    /// いま何番目 の引数に居るか（0 起点）。どれにも居なければ -1
    Active: int
    /// 補足の 1 行。呼ぶ側が組み直せる材料が無いので器が組む
    Detail: string }

type ISourceLanguage =
  abstract Kind: SourceKind
  /// エディタ側の language id。表記と 1 対 1 とは限らないので別に持つ。
  ///
  /// 抽象がエディタを名指ししない（`MonacoLanguage` という名前だった）
  abstract EditorLanguageId: string
  /// 打った瞬間に候補を出す字。語の文字は要らない（Monaco が自分で出す）。
  /// 表記ごとに違うので言語モジュールが持つ
  abstract TriggerCharacters: string list
  /// 本文とカーソルの位置（文字数）から候補を出す。
  /// WASM に行かない —— 語彙は起動時にもらったものを引く
  abstract Complete: source: string -> offset: int -> Completion list
  /// カーソルの下に在るものの仕様。返すのは markdown の字。
  /// 何の上でもなければ `None`（空の字を返さない ——
  /// 空でも枠が浮くので、出ていないことと見分けがつかなくなる）
  abstract Hover: source: string -> offset: int -> string option
  /// カーソルの下に在る名前が、本文のどこに書いてあるか。全部。
  ///
  /// 定義側からも参照側からも同じ並びが返る。
  /// 名前の上でなければ空 —— それを「その名前が 1 か所 も無い」と読まない
  abstract Usages: source: string -> offset: int -> Usage list
  /// カーソルの下に在る「無い参照」を、どう直せるか。
  ///
  /// 候補が無ければ空。嘘の直し方を出さない —— 出すと、押した人は
  /// 直ったと思って、別の名前に化けた本文を持つことになる
  abstract Fixes: source: string -> offset: int -> Fix list
  /// 読めて・組めても走らないもの（v2.3）。カーソルを見ない。
  ///
  /// 字から出るので WASM へ行かない（いちばん長い本 29,190 字 で 0.488 ms / 回）
  abstract Findings: source: string -> Semantics.Finding list
  /// 定義の行の上に出す字（v4.3）。カーソルを見ない。
  ///
  /// 根から走る定義には「0 か所 から参照」と出さない
  /// （参照 0 の定義 215 件 のうち 209 件 が根から走る。出すと 9 割 の本に嘘が並ぶ）。
  ///
  /// F# の CE は空（要素名で書かないので、定義の行を字から決められない）
  abstract Lenses: source: string -> Lens list
  /// 値を横に出す先（v4.4）。本文ぜんぶ の話。
  ///
  /// 出すのは 3 つ を通ったものだけ ——
  ///
  ///     読める            読めない式は波線の担当（v4.1）
  ///     ただの数でない     `<wait>30</wait>` に `= 30` は要らない
  ///     `$rand` を含まない 毎回 変わるので、字の横に固定の数を出すと嘘になる
  ///
  /// F# の CE は空
  abstract Hints: source: string -> ExprSpot list
  /// 参照が渡す引数の形（v4.5）。カーソルを見る。
  ///
  /// `◯◯Ref` の中に居なければ `None`（空を返さない。`Hover` と同じ）。
  ///
  /// 数が食い違っていても警告にしない（同梱 176 本 に 9 件 在る）——
  /// 出すのは形であって、正しさの判定ではない。
  ///
  /// F# の CE は `None`
  abstract Signature: source: string -> offset: int -> Signature option
  /// 本文の構造（v2.4）。アウトラインと折りたたみが載る。
  ///
  /// 入れ子の知り方は表記ごとに違うが、答えは同じ
  /// （同梱 176 本 を 3 表記 に通して (名前, 深さ) が 176 / 176 本 一致）。
  ///
  /// カーソルを見ない
  abstract Outline: source: string -> Outline.Node list
  /// 木のノードになる札の、光らせる範囲の並び（v3.1 の段 4）。
  /// k 番目 が木の k 番目 のノード —— 木は字の位置を持たないので
  /// 位置ではなく順番で結ぶ。
  ///
  /// F# の CE は空（v2.4.5 で 176 本 中 145 本 割れた。
  /// 「結べない」は測ってあるので、推定で光らせない）。
  ///
  /// ノードになる名前は呼ぶ側が渡す。正本は `Core/DTD.fs` の腕で、
  /// 器に表を書かない
  abstract NodeSpans: source: string -> nodes: string list -> NodeSpan list
