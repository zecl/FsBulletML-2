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

type Vocab =
  { Elements: VocabElement list
    /// 式の中で使える字
    Expressions: string list
    /// F# の CE の名前。**この表記でだけ引く** ——
    /// ほかの 3 つ は要素名をそのまま打つので要らない
    Ce: VocabCe list }

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
    /// カーソルの手前 何文字 を置き換えるか
    Replace: int }

module Completion =
  let plain (replace: int) (label: string) =
    { Label = label; Insert = label; Snippet = false; Replace = replace }

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
    Text: string }

/// 直し方 1 つ。**位置は 1 起点**（Monaco の行桁と同じ）で、
/// 置き換えるのは名前の中身（引用符の内側）。
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
