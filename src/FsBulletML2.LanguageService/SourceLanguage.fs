/// 表記ごとの口。**v0.3 の実装は XML 1 本 だけ。**
///
/// ここに口を置くのは、次の言語（sxml / fsb / F# CE）を足すときに
/// **触るのを言語モジュール 1 個 に閉じるため。** Monaco の配線も rAF も
/// `Playfield` も触らせない。
///
/// **`Diagnose` は入れない。** 設計書の口には在るが、v0.3 の診断は Apply 時に
/// host（WASM）が返すので、Fable 側の `Diagnose` を呼ぶ経路が無い。
/// 入れると XML モジュールが「`None` を返すだけの実装」を抱える ——
/// 設計書自身がリスクに挙げている形。呼ぶ経路ができる版で足す。
///
/// `Complete` が返すのは文字列の並び。**`{ Label; InsertText; Detail }` にしない** ——
/// v0.3 で使うのは Label だけで、残り 2 つ は誰も読まない。要る言語が来たら広げる。
///
/// **JSON を読むところはここに無い。** `[<Emit>]` を使うので Fable でしか
/// 走らず、器に置くと片方 の runtime で当てられなくなる。読む側は
/// `Playground/fable/VocabularyJson.fs`。**形の正本はこちら。**
module FsBulletML2.LanguageService.SourceLanguage

/// 語彙の写し。**正本は Core の DTD.fs**（host が JSON にして渡す）。
/// ここは受け取った形をそのまま持つだけで、表を書かない
/// 雛形の骨（v2.6）。**値も属性もここが持つ。**
///
/// 値は Monaco の snippet の穴（`$1` `$2` …。`$0` が最後にカーソルを置く場所）。
///
/// **表記を知らない。** どう字にするかは `Shape.WriteFrame` の側で、
/// 骨は 3 表記 で 1 つ —— 雛形を増やしても表記ごとの手は増えない。
type Frame =
  { Element: string
    Attrs: (string * string) list
    /// 中身の字。**子が在れば空**（BulletML の要素は、値か子のどちらか）
    Text: string
    Children: Frame list }

/// 雛形 1 つ（v2.6）。**host が渡す** ——
/// 骨は要素名の木なので、器に書くと「ブラウザ側に要素名を書かない」線を越える
/// （`guard-playground-boundaries` が実際に落とした）。
/// 語彙と同じ経路で、`Core/DTD.fs` を見ている側から渡ってくる
type FrameSnippet =
  { /// 候補に出す名前。**要素名は既に候補に出ている**ので、ここに並ぶのは形の名前
    Label: string
    /// どの要素の中で出すか。**空 は出さない**
    In: string list
    /// 何本 が使っているか。候補の脇に出す
    Detail: string
    Frame: Frame }

type VocabAttr =
  { Name: string
    Values: string list
    /// 書かなかったときに走る値。**並びで持つ** —— host 側が 1 個 に
    /// 絞れているかを門で見ているので、ここは受け取った形のまま
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
/// **散文を持たない。** 出す字は `Elements` の側から引く ——
/// CE で書いていても読んでいるのは BulletML で、`fire` の意味は
/// 表記が変わっても変わらない。持たせると同じ説明が 2 か所 に在る。
///
/// **同じ名前が何個 在ってもよい** —— `changeDirectionAbs` は要素と
/// 属性値の 2 つ を、`vertical` は根の型と accel の中の要素という
/// **別のもの 2 つ** を指す。正本は host の `Spec.ce`
type VocabCe =
  { Name: string
    /// 作る要素
    Element: string
    /// 固定する属性。**固定しないなら空**
    Attr: string
    /// 固定する値。同上
    Value: string }

/// F# の CE の名前 1 つ が、どの要素の label を載せるか。
///
/// **`VocabCe` と別に持つ。** あちらは「その名前が何を作るか」で、
/// こちらは「その名前が名前を決めるか、使うか」——
/// `repeatAs` は `<repeat>` を作るが、**名前が付くのは中の `<action>`**。
///
/// 正本は host の `Spec.ceLabels`
type VocabCeLabel =
  { Name: string
    /// label の付く要素。**参照側は `Ref` の付いた要素名**
    /// （`Refs.pairs` が対にするのと同じ綴り）
    Element: string
    /// 名前が、その CE 名 のあとに続く**何番目 の文字列リテラル**か（0 起点）。
    /// **引数を取らないなら -1**
    LabelArg: int
    /// 引数を取らないときの名前。取るなら空
    Fixed: string
    /// 根の直下 に書ける名前か。**「定義を作る」がこれで選ぶ** ——
    /// 同じ要素を作る名前が複数 在り、どこに置けるかで使い分けるので、
    /// 要素だけでは選べない
    Root: bool }

/// F# の CE の名前 1 つ が、**どこに置けて、何を開くか。**
///
/// v1.9 の頭で測って決めた形 —— **置ける先は要素ではなく「入れ物の種類」。**
/// `repeat` の中に置けるものは `action` の中と同じ（どちらも `ActionBuilder`）で、
/// 要素（`<repeat>` と `<action>`）で分けると `repeat` の中で候補が 0 個 になる。
///
/// 正本は host の `Vocabulary.cePlaces`。**表ではなく `Dsl` から reflection で
/// 引いている** —— 手で書いた表は 107 行 になり、DSL が動くと黙って古びる。
type VocabCePlace =
  { Name: string
    /// 置ける入れ物の種類。**空 は「どこにも置けない」**
    In: string
    /// この名前が開く `{ }` の種類。**開かないなら空**
    Opens: string }

type Vocab =
  { Elements: VocabElement list
    /// 式の中で使える字
    Expressions: string list
      /// 雛形（v2.6）。**host が焼いて渡す** —— 骨は要素名の木なので、
    /// 器に書くと「ブラウザ側に要素名を書かない」線を越える
    Frames: FrameSnippet list
  /// F# の CE の名前。**この表記でだけ引く** ——
    /// ほかの 3 つ は要素名をそのまま打つので要らない
    Ce: VocabCe list
    /// CE の名前が載せる label。同上
    CeLabels: VocabCeLabel list
    /// CE の名前を、どこに置けるか。**候補（`Complete`）がこれで決まる**
    CePlaces: VocabCePlace list
    /// 根から走る定義の名前の頭（`top`）。**この綴りは Core が持っている** ——
    /// 走らせる側が `label.StartsWith` で選ぶだけなので、器に書き写すと
    /// あちらを変えたときに黙って割れる。host が Core を見て埋める。
    ///
    /// **空なら、意味の検査は `top` の話を何も出さない**（`Semantics`）——
    /// 語彙が引けていないときに「走らない」と言うほうが害が大きい
    TopPrefix: string }

/// 候補 1 つ。
///
/// **`Replace` を言語モジュールが決める。** Monaco の「語」に任せると、
/// `$rand` の `$` が語に入らない版で `$$rand` になる。手前 何文字 を
/// 置き換えるかはこちらが数える —— 何が語かを知っているのは言語のほう
type Completion =
  { Label: string
    /// 入れる字。`Snippet` なら Monaco の記法（`$0` がカーソル）
    Insert: string
    Snippet: bool
    /// 雛形か（v2.6）。**`Snippet` と別に持つ** —— 属性の候補も
    /// snippet 記法で入るので、あちらでは「形の候補」と分けられない。
    ///
    /// 分ける先は 2 つ ある —— エディタ側の見た目（Monaco の kind）と、
    /// **「その場所に置ける要素」を数える試験**
    IsFrame: bool
    /// カーソルの手前 何文字 を置き換えるか
    Replace: int }

module Completion =
  let plain (replace: int) (label: string) =
    { Label = label; Insert = label; Snippet = false; IsFrame = false; Replace = replace }

/// 同じ名前が書いてある 1 か所。**位置は 1 起点**（Monaco の行桁と同じ）で、
/// 指すのは名前の中身（引用符の内側）。
///
/// **rename ではなく「どこに書いてあるか」を返す。** 新しい名前をここへ
/// 渡さないのは、**字を差し替えるのが言語の知識ではない**から ——
/// 言語が知っているのは「同じ名前がここに在る」までで、
/// そこへ何を書くかはエディタ側の話。
///
/// そのぶん、同じ並びを rename 以外（今いる名前を光らせる・参照を一覧する）
/// にも使える。
type Usage =
  { Line: int
    Column: int
    EndColumn: int
    /// いま書いてある字。**どの `Usage` でも同じ** ——
    /// rename の入力欄の初期値に要るので、1 つ 拾えば済む形にしてある
    Text: string
    /// 名前を**決めている**側か（`action label="a"`）。
    /// 参照している側（`actionRef label="a"`）なら false。
    ///
    /// **口を足さずに 1 欄 で足りる。** 語彙から引いた対
    /// （`Refs.pairs`）の定義側と参照側は、要素名が 1 つ も重ならない ——
    /// 測ってから決めた（3 組 / 重なり 0）。重なっていたら、
    /// 札の名前だけではどちら側か言えないので口が要っていた。
    ///
    /// **同じ本文に定義が 2 つ 在ることは在る**（別々の要素が同じ label）。
    /// 読む側は「1 つ に決まる」と思ってはいけない
    IsDefinition: bool }

/// 直し方 1 つ。**位置は 1 起点**（Monaco の行桁と同じ）。
///
/// 置き換えるのは名前の中身（引用符の内側）。
/// **`Column = EndColumn` なら置き換えではなく挿し込み**（幅 0 の範囲）——
/// 「定義を作る」がそちら。同じ型で足りるので口を分けない。
///
/// **波線に紐づけない。** 波線は 1 文字 打った時点で消える（印は文字に
/// 追随しないので、そこで下ろすのが正しい）—— 紐づけると、直し方が
/// **Apply の直後の窓でしか出ない。** 本文から数え直せば、いつでも出る。
type Fix =
  { /// メニューに出す字
    Title: string
    Line: int
    Column: int
    EndColumn: int
    /// そこへ書く字
    Text: string }

type ISourceLanguage =
  abstract Kind: SourceKind
  /// エディタ側の language id。表記と 1 対 1 とは限らないので別に持つ。
  ///
  /// **`MonacoLanguage` という名前だった。** 中身は Monaco 固有の値ではなく
  /// 「エディタに渡す language id」で、名前だけが唯一 Monaco を名指ししていた ——
  /// 抽象がエディタを名指しすると、載せ替えるときに抽象ごと直すことになる
  abstract EditorLanguageId: string
  /// 打った瞬間に候補を出す字。**語の文字は要らない** —— そちらは Monaco が
  /// 自分で出す。ここに置くのは「語ではないが、その直後に必ず候補が要る」字。
  /// **表記ごとに違う**（sxml なら括弧）ので言語モジュールが持つ
  abstract TriggerCharacters: string list
  /// 本文とカーソルの位置（文字数）から候補を出す。
  /// **WASM に行かない** —— 語彙は起動時にもらったものを引く
  abstract Complete: source: string -> offset: int -> Completion list
  /// カーソルの下に在るものの仕様。**返すのは markdown の字**で、
  /// Monaco 側の形（`contents` の配列）を組むのは呼ぶ側。
  /// 何の上でもなければ `None`（**空の字を返さない** ——
  /// 空でも枠が浮くので、出ていないことと見分けがつかなくなる）
  abstract Hover: source: string -> offset: int -> string option
  /// カーソルの下に在る名前が、本文のどこに書いてあるか。**全部。**
  ///
  /// **定義側からも参照側からも同じ並びが返る**（`action label="a"` の上でも
  /// `actionRef label="a"` の上でも、その 2 つ が並ぶ）。
  ///
  /// 名前の上でなければ空。**空を「その名前が 1 か所 も無い」と読まない** ——
  /// カーソルが名前の上に無いだけのことがある（呼ぶ側がそこを分ける）
  abstract Usages: source: string -> offset: int -> Usage list
  /// カーソルの下に在る「無い参照」を、どう直せるか。
  ///
  /// **候補が無ければ空。** 嘘の直し方を出さない —— 出すと、押した人は
  /// 直ったと思って、別の名前に化けた本文を持つことになる
  abstract Fixes: source: string -> offset: int -> Fix list
  /// 読めて・組めても走らないもの（v2.3）。**カーソルを見ない** ——
  /// 本文ぜんぶ の話なので、位置ではなく本文だけを受け取る。
  ///
  /// **字から出るので WASM へ行かない。** 打鍵ごとに引き直せる
  /// （いちばん長い本 29,190 字 で 0.488 ms / 回。実測）
  abstract Findings: source: string -> Semantics.Finding list
  /// 本文の構造（v2.4）。アウトラインと折りたたみが載る。
  ///
  /// **入れ子の知り方は表記ごとに違うが、答えは同じ** ——
  /// 同梱 176 本 を 3 表記 に通して (名前, 深さ) が 176 / 176 本 一致した
  /// （F# の CE だけは `{ }` の段なので、要素の入れ子とは別物）。
  ///
  /// **カーソルを見ない。** 本文ぜんぶ の話
  abstract Outline: source: string -> Outline.Node list
