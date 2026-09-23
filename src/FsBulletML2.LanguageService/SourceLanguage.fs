/// 表記ごとの口。次の言語を足すとき触るのは言語モジュール 1 個。
/// JSON を読むところはここに無い。形の正本はこちら。
module FsBulletML2.LanguageService.SourceLanguage

/// 雛形の骨（v2.6）。値も属性もここが持つ。表記を知らない。
/// どう字にするかは `Shape.WriteFrame`。骨は 3 表記 で 1 つ。
type Frame =
  { Element: string
    Attrs: (string * string) list
    /// 中身の字。子が在れば空（BulletML の要素は、値か子のどちらか）
    Text: string
    Children: Frame list }

/// 雛形 1 つ（v2.6）。host が渡す。
/// 骨は要素名の木。器に書くとブラウザ側に要素名を書くことになる。
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
/// 散文は持たない。同じ名前が何個 在ってもよい。正本は `Spec.ce`。
type VocabCe =
  { Name: string
    /// 作る要素
    Element: string
    /// 固定する属性。固定しないなら空
    Attr: string
    /// 固定する値。同上
    Value: string }

/// F# の CE の名前 1 つ が、どの要素の label を載せるか。
/// `VocabCe` とは別。作る要素と、名前が付く要素は違う。正本は `Spec.ceLabels`。
type VocabCeLabel =
  { Name: string
    /// label の付く要素。参照側は `Ref` の付いた要素名
    Element: string
    /// 名前が、その CE 名 のあとに続く何番目 の文字列リテラルか（0 起点）。
    /// 引数を取らないなら -1
    LabelArg: int
    /// 引数を取らないときの名前。取るなら空
    Fixed: string
    /// 根の直下 に書ける名前か。「定義を作る」がこれで選ぶ。
    /// 同じ要素を作る名前が複数 在るので、要素だけでは選べない。
    Root: bool }

/// F# の CE の名前 1 つ が、どこに置けて、何を開くか。
/// 置ける先は要素ではなく入れ物。要素で分けると `repeat` の中が空になる。
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
    /// 根から走る定義の名前の頭。綴りは Core が持つ。器に書き写すな。
    /// 空なら、意味の検査は `top` を出さない。
    TopPrefix: string }

/// 候補 1 つ。`Replace` は言語モジュールが決める。
/// Monaco の語に任せると `$rand` が `$$rand` になる。
type Completion =
  { Label: string
    /// 入れる字。`Snippet` なら Monaco の記法（`$0` がカーソル）
    Insert: string
    Snippet: bool
    /// 雛形か（v2.6）。`Snippet` と別に持つ。
    /// 属性の候補も snippet なので、あちらでは形と分けられない。
    IsFrame: bool
    /// カーソルの手前 何文字 を置き換えるか
    Replace: int }

module Completion =
  let plain (replace: int) (label: string) =
    { Label = label; Insert = label; Snippet = false; IsFrame = false; Replace = replace }

/// 同じ名前が書いてある 1 か所。指すのは名前の中身。
/// rename ではない。字を差し替えるのは言語の知識ではない。
type Usage =
  { Line: int
    Column: int
    EndColumn: int
    /// いま書いてある字。どの `Usage` でも同じ
    Text: string
    /// 名前を決めている側か。参照側なら false。
    /// 同じ本文に定義が 2 つ 在っても、1 つ に決まると思わない。
    IsDefinition: bool }

/// 直し方 1 つ。`Column = EndColumn` なら挿し込み（幅 0）。
/// 波線に紐づけない。紐づけると Apply の直後しか出ない。
type Fix =
  { /// メニューに出す字
    Title: string
    Line: int
    Column: int
    EndColumn: int
    /// そこへ書く字
    Text: string }

/// 定義の行の上に出す字 1 つ（v4.3）。字は器が組む。
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

/// 値を横に出す先 1 つ（v4.4）。値はここに入らない。
/// 器に評価器を持たせるな。同じ式が 2 通り の値になる。
type ExprSpot =
  { /// 式そのもの
    Text: string
    /// その式が書いてある要素
    Element: string
    /// 出す位置（式の終わりの次）
    Line: int
    Column: int }

/// 参照が渡す引数の形（v4.5）。`◯◯Ref` の中に居るときだけ。
/// 数だけでなく、いま何番目 かも返す。片方 だけだと隣の引数を打てる。
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
  /// エディタ側の language id。表記と 1 対 1 とは限らない。
  /// 抽象がエディタを名指ししない。
  abstract EditorLanguageId: string
  /// 打った瞬間に候補を出す字。語の文字は要らない（Monaco が自分で出す）。
  /// 表記ごとに違うので言語モジュールが持つ
  abstract TriggerCharacters: string list
  /// 本文とカーソルの位置（文字数）から候補を出す。
  /// WASM に行かない —— 語彙は起動時にもらったものを引く
  abstract Complete: source: string -> offset: int -> Completion list
  /// カーソルの下に在るものの仕様。markdown。
  /// 何の上でもなければ `None`。空の字を返すな。空でも枠が浮く。
  abstract Hover: source: string -> offset: int -> string option
  /// カーソルの下の名前が、本文のどこに書いてあるか。
  /// 名前の上でなければ空。それを「1 か所 も無い」と読まない。
  abstract Usages: source: string -> offset: int -> Usage list
  /// カーソルの下の無い参照を、どう直せるか。
  /// 候補が無ければ空。嘘の直し方を出すな。
  abstract Fixes: source: string -> offset: int -> Fix list
  /// 読めて・組めても走らないもの（v2.3）。カーソルを見ない。
  /// 字から出るので WASM へ行かない。
  abstract Findings: source: string -> Semantics.Finding list
  /// 定義の行の上に出す字（v4.3）。カーソルを見ない。
  /// 根から走る定義に「0 か所」と出すな。CE は空。
  abstract Lenses: source: string -> Lens list
  /// 値を横に出す先（v4.4）。読める、ただの数でない、`$rand` を含まない、だけ。
  /// F# の CE は空。
  abstract Hints: source: string -> ExprSpot list
  /// 参照が渡す引数の形（v4.5）。`◯◯Ref` の外なら `None`。
  /// 数が食い違っていても警告にしない。出すのは形。CE は `None`。
  abstract Signature: source: string -> offset: int -> Signature option
  /// 本文の構造（v2.4）。入れ子の知り方は表記ごと、答えは同じ。
  /// カーソルを見ない。
  abstract Outline: source: string -> Outline.Node list
  /// 木のノードを光らせる範囲。k 番目 が木の k 番目。
  /// 名前は呼ぶ側が渡す。器に表を書かない。CE は空。推定で光らせるな。
  abstract NodeSpans: source: string -> nodes: string list -> NodeSpan list
