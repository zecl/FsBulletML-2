#requires -Version 7
<#
.SYNOPSIS
  Playground が守ると決めた線を、機械で見る。

.DESCRIPTION
  実装計画の「完成条件」のうち、目で確かめると書いてあったものを門にする。
  **どれも いまは守れている**ので、これは網ではなく現状固定 ——
  「いま守れている形を、次の PR で崩さない」ためだけに置く。
  例外は 3 つ目 で、当てる名前は DTD から毎回 引き直すので、
  そちらは要素が増えれば網も広がる。

  ## 見るもの

  1. 追跡された `.js` が Playground の下に無い
     ブラウザ側は Fable で書く。glue を手で書き始めると、そこだけ型も
     検査も掛からなくなる（v0.2 で一度 捨てている）

  2. Core が Bolero / Monaco / Blazor を参照していない
     Core は表示を知らない。参照が付くと、Unity も MonoGame も
     ブラウザの物を引きずる

  3. Fable 側に BulletML の要素名が書かれていない
     語彙は host が `Core/DTD.fs` から焼いて渡す。**当てる名前は
     `WriteStartElement(...)` から引く** —— 門の中に表を持たない

  4. Monaco を叩くのは `fable/Monaco.fs` 1 本 だけ
     v0.7 の Language Service が Monaco を直に叩き始めると、**Monaco 以外 の
     エディタへ載せられなくなる。** build でも試験でも出ず、載せ替えようと
     した版で初めて分かる。当てるのは `globalThis.monaco`（叩いているか）で、
     `Monaco` という語（言及しているか）ではない

  5. `index.html` に起動のロジックが無い
     html に書くと、そこだけ型も検査も掛からない

  6. Monaco の版が **正確な版で固定**されている（`monaco-editor@x.y.z`）
     CDN から読むので、固定されていなければ黙って上がる。上がったこと自体は
     走行にも試験にも出ず、補完や hover の形が変わったときに初めて分かる。
     `@latest` も `@^0.56` も、読むたび別のものが来る

  7. Monaco の src に SRI（`integrity` と `crossorigin`）が在り、CSP が在り、
     **中身の在るインライン script が無い**
     どれも外されても走るので、走行でも試験でも出ない。integrity が守るのは
     loader.js 1 本 だけで、ローダが後から取りに行く分は CSP の origin でしか
     閉じていない。インライン script は CSP に弾かれて「起動待ち」で止まる
     （実際に踏んだ）

  8. Playground の csproj で `UseSystemResourceKeys` が `false`
     読めなかった理由をそのまま人へ見せるので、例外の文面が字である必要が
     ある。Blazor WASM の SDK は Release でこれを立てるので、放っておくと
     `Xml_TagMismatchEx` のような鍵が波線に載る。**動きは変わらないので、
     外れても誰も気づかない**

  ## 0 件 を緑にしない

  当てる材料が読めなければ赤にする —— 3 つ目 の要素名、Fable のソース、
  4 つ目 の Monaco.fs 以外 のソース、6 つ目 の html と Monaco の src、8 つ目 の csproj。
  拾えなくなった状態は、「違反 0 件」と同じ顔をする。
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  # 追跡ファイルの一覧（repo からの相対）。渡さなければ git から引く
  [string[]]$Files,
  # 以下は較正で差し替えるためだけに開けてある
  [string]$PlaygroundDir,
  [string]$CoreProj,
  [string]$DtdSource,
  [string]$IndexHtml,
  [string]$PlaygroundProj,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = ($RepoRoot -replace '\\', '/').TrimEnd('/')

if (-not $PlaygroundDir) { $PlaygroundDir = 'src/FsBulletML2.Playground' }
if (-not $CoreProj) { $CoreProj = "$RepoRoot/src/FsBulletML2.Core/FsBulletML2.Core.fsproj" }
if (-not $DtdSource) { $DtdSource = "$RepoRoot/src/FsBulletML2.Core/DTD.fs" }
if (-not $IndexHtml) { $IndexHtml = "$RepoRoot/$PlaygroundDir/wwwroot/index.html" }
if (-not $PlaygroundProj) { $PlaygroundProj = "$RepoRoot/$PlaygroundDir/FsBulletML2.Playground.fsproj" }
if (-not $PSBoundParameters.ContainsKey('Files')) {
  $Files = @(git -C $RepoRoot -c core.quotepath=false ls-files)
}

$prefix = ($PlaygroundDir -replace '\\', '/').TrimEnd('/') + '/'
$bad = [System.Collections.Generic.List[string]]::new()

# --- 1. 手書きの .js -------------------------------------------------------
$js = @($Files | Where-Object { $_.StartsWith($prefix) -and $_.ToLower().EndsWith('.js') })
if ($js.Count -gt 0) {
  $bad.Add("  追跡された .js が $($js.Count) 件:`n      " + ($js -join "`n      ") +
           "`n      ブラウザ側は Fable で書く。焼いた js は .gitignore の下に置く")
}

# --- 2. Core の参照 --------------------------------------------------------
if (Test-Path -LiteralPath $CoreProj) {
  $refs = @(Select-String -LiteralPath $CoreProj -Pattern 'Bolero|Monaco|Blazor' |
            ForEach-Object { "{0} 行  {1}" -f $_.LineNumber, $_.Line.Trim() })
  if ($refs.Count -gt 0) {
    $bad.Add("  Core が表示側を参照している:`n      " + ($refs -join "`n      "))
  }
}

# --- 3. Fable 側に要素名 ---------------------------------------------------
#
# **当てる名前は DTD から引く。** 門の中に表を持つと、要素が増えたとき
# 表だけが古びる
$names = @()
if (Test-Path -LiteralPath $DtdSource) {
  $names = @(Select-String -LiteralPath $DtdSource -Pattern 'WriteStartElement\("([^"]+)"' -AllMatches |
             ForEach-Object { $_.Matches } | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
}
if ($names.Count -eq 0) {
  throw "当てる要素名を 1 つ も引けなかった（$DtdSource）。" +
        "**違反 0 件 と同じ顔をする**ので、ここで落とす"
}

# 文字列の足し算で組む。**`"$prefix`fable"` と書くと `` `f `` が改ページになり**、
# ディレクトリが見つからず「走査 0 本 で違反 0 件」になる（実際にやった）
$fableDir = Join-Path $RepoRoot ($prefix + 'fable')
$fableFiles = @()
if (Test-Path -LiteralPath $fableDir) {
  $fableFiles = @(Get-ChildItem -LiteralPath $fableDir -Recurse -Filter '*.fs' -File |
                  ForEach-Object { $_.FullName })
}
if ($fableFiles.Count -eq 0) {
  throw "Fable のソースを 1 本 も読めなかった（$fableDir）。" +
        "**違反 0 件 と同じ顔をする**ので、ここで落とす"
}
$hardcoded = [System.Collections.Generic.List[string]]::new()
foreach ($n in $names) {
  $hits = Select-String -LiteralPath $fableFiles -Pattern ('"' + [regex]::Escape($n) + '"') -CaseSensitive
  foreach ($h in $hits) {
    $hardcoded.Add(("{0} {1} 行  {2}" -f (Split-Path $h.Path -Leaf), $h.LineNumber, $h.Line.Trim()))
  }
}
if ($hardcoded.Count -gt 0) {
  $bad.Add("  Fable 側に要素名が $($hardcoded.Count) 件:`n      " + ($hardcoded -join "`n      ") +
           "`n      語彙は host が Core/DTD.fs から焼いて渡す。ここに表を持たない")
}

# --- 4. Monaco を叩く場所 --------------------------------------------------
#
# **Monaco を知るのは `fable/Monaco.fs` 1 本 だけ。**
#
# v0.7 で「他の DSL でも使える Language Service」を建てる計画が在る。
# 中身が Monaco を直に叩き始めると、**Monaco 以外 のエディタへ載せられなくなる** ——
# しかもそれは build でも試験でも出ず、載せ替えようとした版で初めて分かる。
#
# 見るのは `globalThis.monaco`（`[<Emit>]` の中の字）。`Monaco` という語そのものは
# doc コメントにも `MonacoLanguage`（language id を持つ抽象の名前）にも出るので
# 当てない。**当てる先は「叩いているか」であって「言及しているか」ではない。**
$monacoOwner = Join-Path $fableDir 'Monaco.fs'
$others = @($fableFiles | Where-Object { $_ -ne $monacoOwner })
if ($others.Count -eq 0) {
  throw "Monaco.fs 以外 の Fable のソースが 1 本 も無い（$fableDir）。" +
        "**違反 0 件 と同じ顔をする**ので、ここで落とす"
}
$callers = @(Select-String -LiteralPath $others -Pattern 'globalThis\.monaco' -CaseSensitive |
             ForEach-Object { "{0} {1} 行  {2}" -f (Split-Path $_.Path -Leaf), $_.LineNumber, $_.Line.Trim() })
if ($callers.Count -gt 0) {
  $bad.Add("  Monaco.fs の外から Monaco を叩いている $($callers.Count) 件:`n      " +
           ($callers -join "`n      ") +
           "`n      叩くのは Monaco.fs 1 本。ほかは Monaco を知らない形で書く")
}

# --- html を 1 回 だけ読む -------------------------------------------------
#
# **材料が読めないときは落とす。** 下の 3 つ（ロジック / 版 / SRI）は
# どれも「読めなければ違反 0 件」になる。
#
# **コメントを外してから当てる。** 外さないと、但し書きに書いた例文が
# そのまま違反として出る（実際に踏んだ —— 「前はこう書いていた」の例に
# インライン script の点が当たった）。コメントの中の script はブラウザも
# 読まないので、外すのが正しい
if (-not (Test-Path -LiteralPath $IndexHtml)) {
  throw "index.html を読めなかった（$IndexHtml）。**違反 0 件 と同じ顔をする**ので、ここで落とす"
}
$html = [regex]::Replace((Get-Content -LiteralPath $IndexHtml -Raw), '<!--[\s\S]*?-->', '')

# --- 5. html のロジック ----------------------------------------------------
if ($html -match 'Blazor\.start') {
  $bad.Add("  html に起動のロジックが在る:`n      $IndexHtml" +
           "`n      起こすのは Fable の側。html は読み込むだけ")
}

# --- 6. Monaco の版 --------------------------------------------------------
#
# **CDN から読むので、固定されていなければ黙って上がる。** 上がったこと自体は
# 走行にも試験にも出ず、補完や hover の形が変わったときに初めて分かる。
#
# 見るのは「そこに版が在る」ではなく「**正確な版**で書いてある」——
# `@latest` も `@^0.56` も、読むたび別のものが来る。
#
# **Monaco の src が 1 本 も無いのも落とす。** 違反 0 件 と同じ顔をする
$monaco = @([regex]::Matches($html, 'monaco-editor@([^/"]+)') |
            ForEach-Object { $_.Groups[1].Value })
if ($monaco.Count -eq 0) {
  $bad.Add("  Monaco の src が index.html に無い:`n      $IndexHtml" +
           "`n      版はこの 1 行 だけに在る。無いと Monaco.fs が読む先も消える")
} else {
  $floating = @($monaco | Where-Object { $_ -notmatch '^\d+\.\d+\.\d+$' })
  if ($floating.Count -gt 0) {
    $bad.Add("  Monaco の版が固定されていない: " + ($floating -join ', ') +
             "`n      CDN は読むたび取りに行く。x.y.z で書くこと")
  }
}

# --- 7. SRI と CSP ---------------------------------------------------------
#
# **どちらも外されても走る。** integrity を消しても Monaco は読めるし、
# CSP を消しても画は出る。**走行でも試験でも出ない**ので、字で見るしかない。
#
# integrity が守るのは loader.js 1 本 だけ。ローダが後から取りに行く分は
# CSP の origin でしか閉じていない —— **その但し書きは html に書いてある。**
if ($html -notmatch 'integrity="sha(256|384|512)-') {
  $bad.Add("  Monaco の src に integrity が無い:`n      $IndexHtml" +
           "`n      CDN が別の物を返しても気づけない。sha384 を付けること")
}
if ($html -notmatch 'crossorigin=') {
  $bad.Add("  Monaco の src に crossorigin が無い:`n      $IndexHtml" +
           "`n      付けないとブラウザが integrity を当てられない（黙って素通りする）")
}
if ($html -notmatch 'http-equiv="Content-Security-Policy"') {
  $bad.Add("  CSP が index.html に無い:`n      $IndexHtml" +
           "`n      integrity は loader.js 1 本 にしか掛からない。残りは origin で閉じる")
}
# **インラインの script を置かない。** CSP に 'unsafe-inline' を出していないので
# 黙って弾かれる（実際に踏んだ。エラーは出ず「起動待ち」のまま止まる）。
# 空の importmap は中身が無いので数えない
$inline = @([regex]::Matches($html, '<script(?![^>]*\ssrc=)[^>]*>(?<body>[\s\S]*?)</script>') |
            Where-Object { $_.Groups['body'].Value.Trim() -ne '' } |
            ForEach-Object { $_.Value.Split("`n")[0].Trim() })
if ($inline.Count -gt 0) {
  $bad.Add("  中身の在るインライン script が $($inline.Count) 件:`n      " + ($inline -join "`n      ") +
           "`n      CSP に 'unsafe-inline' を出していないので黙って弾かれる。src で読むこと")
}

# --- 8. 例外の文面 ---------------------------------------------------------
#
# **現状固定。** 外すと Release で resource key に戻り、波線に載る文面が
# 鍵になる。動きは変わらないので、外れても走行では気づけない
if (-not (Test-Path -LiteralPath $PlaygroundProj)) {
  throw "Playground の csproj を読めなかった（$PlaygroundProj）。" +
        "**違反 0 件 と同じ顔をする**ので、ここで落とす"
}
$resourceKeys = @(Select-String -LiteralPath $PlaygroundProj `
                    -Pattern '<UseSystemResourceKeys>([^<]*)</UseSystemResourceKeys>' -AllMatches |
                  ForEach-Object { $_.Matches } | ForEach-Object { $_.Groups[1].Value.Trim() })
if ($resourceKeys.Count -eq 0) {
  $bad.Add("  UseSystemResourceKeys が csproj に無い:`n      $PlaygroundProj" +
           "`n      Blazor WASM の SDK が Release で立てるので、書かないと鍵に戻る")
} elseif ($resourceKeys.Count -ne 1 -or $resourceKeys[0] -ne 'false') {
  $bad.Add("  UseSystemResourceKeys が false でない: " + ($resourceKeys -join ', ') +
           "`n      読めなかった理由をそのまま人へ見せる。文面は字で出す")
}

if (-not $Quiet) {
  Write-Host ("走査 {0} 件 / 当てる要素名 {1} 個 / Fable のファイル {2} 本" -f
              $Files.Count, $names.Count, $fableFiles.Count)
}

if ($bad.Count -gt 0) {
  throw ("Playground の線を越えているものが $($bad.Count) 形:`n" + ($bad -join "`n"))
}

if (-not $Quiet) { Write-Host 'Playground は線の内側に居る' }
