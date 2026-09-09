#requires -Version 7
<#
.SYNOPSIS
  使い方の窓が、章に割れたまま保てているかを見る。

.DESCRIPTION
  章は 3 か所 に書かれている ——

      html の目次    <button id="help-tab-write" class="help-tab">書く</button>
      html の本文    <section id="help-ch-write" class="help-ch" hidden>
      Fable の並び   let private helpChapters = [ "start"; "write"; ... ]

  **どれか 1 つ を足し忘れても、走行は止まらない。** 目次に出ない章、
  押しても開かない札、配線されない札 —— どれも黙って通る。ここで数える。

  **並びまで見る。** 集合が合っていても順が違うと、目次の並びと
  切り替えの並びがずれる（Fable の並びは配線の順でもある）。

  絵も見る。**参照と実物を両方向 で数える** ——

      片方向 だけ    参照した絵が無ければ落ちるが、誰も参照していない絵は残り続ける
      両方向         消し忘れも出る

  css も 1 つ 見る。**素の `.help` に `display` を書くと、閉じた窓が生き残る** ——
  ブラウザが素で持っている `dialog:not([open]) { display: none }` は
  作者の指定に負ける（origin で決まるので詳細度は関係ない）。生き残った窓は
  中の口が Tab の輪に入り、縦に長い機械では画面にも出る。実際に踏んだ。

  **絵の中身が古びたことは、この門も見ていない。** 撮り直す手順は
  docs/help-shots.md に在る。ここで測れるのは「在る / 参照されている」まで。

  較正は guard-help-chapters.Tests.ps1。
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  [string]$IndexHtml,
  [string]$PlaygroundFs,
  # 絵の置き場。**html の src からの相対の根**（wwwroot）
  [string]$WwwRoot,
  [string]$Css,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$pg = Join-Path $RepoRoot 'src/FsBulletML2.Playground'
if (-not $IndexHtml) { $IndexHtml = Join-Path $pg 'wwwroot/index.html' }
if (-not $PlaygroundFs) { $PlaygroundFs = Join-Path $pg 'fable/Playground.fs' }
if (-not $WwwRoot) { $WwwRoot = Join-Path $pg 'wwwroot' }
if (-not $Css) { $Css = Join-Path $pg 'wwwroot/css/playground.css' }

foreach ($f in @($IndexHtml, $PlaygroundFs)) {
  if (-not (Test-Path -LiteralPath $f)) {
    throw "読めなかった（$f）。**章 0 件 は違反 0 件 と同じ顔をする**ので、ここで落とす"
  }
}

# **コメントを先に落とす。** どちらの側 も、但し書きの中に `help-tab-` と
# `helpChapters` の字が在る（実際に自分で書いた）。字数と行は保つ
function Strip-Comments {
  param([string]$Text, [string]$Kind)
  switch ($Kind) {
    'html' { [regex]::Replace($Text, '(?s)<!--.*?-->', { param($m) $m.Value -replace '[^\r\n]', ' ' }) }
    'fs' { ($Text -split "\r?\n" | ForEach-Object { $_ -replace '^\s*///.*$', '' } ) -join "`n" }
  }
}

$html = Strip-Comments -Text ([IO.File]::ReadAllText($IndexHtml)) -Kind 'html'
$fs = Strip-Comments -Text ([IO.File]::ReadAllText($PlaygroundFs)) -Kind 'fs'

# --- 3 つ の並びを引く --------------------------------------------------------
$tabs = @([regex]::Matches($html, 'id="help-tab-([a-z][a-z0-9-]*)"') | ForEach-Object { $_.Groups[1].Value })
$sections = @([regex]::Matches($html, 'id="help-ch-([a-z][a-z0-9-]*)"') | ForEach-Object { $_.Groups[1].Value })

$listMatch = [regex]::Match($fs, 'let\s+private\s+helpChapters\s*=\s*\[(?<body>[^\]]*)\]')
if (-not $listMatch.Success) {
  throw "Playground.fs に helpChapters の並びが見つからない。**引けなければ 0 件 になり、0 件 は違反 0 件 と同じ顔をする**"
}
$chapters = @([regex]::Matches($listMatch.Groups['body'].Value, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value })

if (-not $Quiet) {
  Write-Host ("章 {0} / 目次の札 {1} / 本文の節 {2}" -f $chapters.Count, $tabs.Count, $sections.Count)
}

# **0 件 で落とす。** 引けなかったのと、違反が無いのを混ぜない
if ($chapters.Count -eq 0) { throw 'helpChapters が空。章を 1 つ も引けなかった' }
if ($tabs.Count -eq 0) { throw '目次の札を 1 つ も引けなかった（id="help-tab-…"）' }
if ($sections.Count -eq 0) { throw '本文の節を 1 つ も引けなかった（id="help-ch-…"）' }

# --- 並びが 3 つ とも同じか ---------------------------------------------------
# **集合ではなく並びで比べる。** 集合が合っていても順が違うと、
# 目次の並びと切り替えの並びがずれる
$problems = [System.Collections.Generic.List[string]]::new()

function Compare-Order {
  param([string]$What, [string[]]$Expected, [string[]]$Actual)
  if (($Expected -join ',') -eq ($Actual -join ',')) { return }
  $missing = @($Expected | Where-Object { $_ -notin $Actual })
  $extra = @($Actual | Where-Object { $_ -notin $Expected })
  if ($missing.Count -gt 0) { $script:problems.Add("$What に無い章: $($missing -join ', ')") }
  if ($extra.Count -gt 0) { $script:problems.Add("$What にだけ在る章: $($extra -join ', ')") }
  if ($missing.Count -eq 0 -and $extra.Count -eq 0) {
    $script:problems.Add("$What の並びが違う: Fable は [$($Expected -join ', ')] / こちらは [$($Actual -join ', ')]")
  }
}

Compare-Order -What '目次の札' -Expected $chapters -Actual $tabs
Compare-Order -What '本文の節' -Expected $chapters -Actual $sections

# --- 出ている章がちょうど 1 つ か ---------------------------------------------
# **`hidden` の付いていない節が 1 つ**。0 なら開いた瞬間 空、2 つ 以上 なら重なる
$openSections = @()
foreach ($c in $sections) {
  $m = [regex]::Match($html, 'id="help-ch-' + [regex]::Escape($c) + '"(?<attrs>[^>]*)>')
  if ($m.Success -and $m.Groups['attrs'].Value -notmatch '\bhidden\b') { $openSections += $c }
}
if ($openSections.Count -ne 1) {
  $problems.Add("開いた瞬間 に出る節が $($openSections.Count) 個（1 つ でないと、空か重なる）: $($openSections -join ', ')")
}

# **`aria-current` の付いた札もちょうど 1 つ**、しかも出ている節と同じ章
$currentTabs = @()
foreach ($c in $tabs) {
  $m = [regex]::Match($html, 'id="help-tab-' + [regex]::Escape($c) + '"(?<attrs>[^>]*)>')
  if ($m.Success -and $m.Groups['attrs'].Value -match 'aria-current="true"') { $currentTabs += $c }
}
if ($currentTabs.Count -ne 1) {
  $problems.Add("aria-current の付いた札が $($currentTabs.Count) 個（1 つ でないと、いまどこかが読めない）: $($currentTabs -join ', ')")
} elseif ($openSections.Count -eq 1 -and $currentTabs[0] -ne $openSections[0]) {
  $problems.Add("出ている節は $($openSections[0]) なのに、印が付いている札は $($currentTabs[0])")
}

# --- 絵。参照と実物を両方向 で -------------------------------------------------
$refs = @([regex]::Matches($html, 'src="(img/help/[^"]+)"') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
$imgDir = Join-Path $WwwRoot 'img/help'

if (-not $Quiet) { Write-Host ("絵の参照 {0} 本" -f $refs.Count) }

foreach ($r in $refs) {
  $onDisk = Join-Path $WwwRoot ($r -replace '/', [IO.Path]::DirectorySeparatorChar)
  if (-not (Test-Path -LiteralPath $onDisk)) { $problems.Add("参照している絵が無い: $r") }
}

if (Test-Path -LiteralPath $imgDir) {
  $files = @(Get-ChildItem -LiteralPath $imgDir -File | ForEach-Object { 'img/help/' + $_.Name })
  foreach ($f in $files) {
    if ($f -notin $refs) { $problems.Add("誰も参照していない絵が在る: $f") }
  }
} elseif ($refs.Count -gt 0) {
  $problems.Add("絵の置き場が無い（$imgDir）のに、参照が $($refs.Count) 本 在る")
}

# --- 閉じた窓が生き残らないか -------------------------------------------------
#
# ブラウザは素で `dialog:not([open]) { display: none }` を持っているが、
# **作者の指定は origin で勝つ**（詳細度は関係ない）。だから素の `.help` に
# `display` を書くと、**閉じた窓が生きたまま残る** ——
# 中の口が Tab の輪に入り、窓が縦に長い機械では画面にも出る。
#
# 実際に踏んだ。`display: flex` を素の `.help` に書いて、
# **閉じているのに輪へ 9 個 入っていた**（窓は画面の下 に外れていて目には出ず、
# 輪を数えて初めて出た）。`display` は `[open]` の側 に書く。
#
# **見るのは「窓自身を指す並び」だけ。** `.help figure` のような子孫や
# `.help-tab` のような別の名前は、閉じていれば親ごと消えるので関係無い
# **css も材料。** 読めなければ落とす —— 見張りを黙って外すと、
# 「規則 0 件」が「違反 0 件」と同じ顔をする
if (-not (Test-Path -LiteralPath $Css)) {
  throw "css を読めなかった（$Css）。閉じた窓の見張りが掛からないので、ここで落とす"
}

$cssText = [regex]::Replace([IO.File]::ReadAllText($Css), '(?s)/\*.*?\*/', ' ')
$cssRules = [regex]::Matches($cssText, '(?s)(?<sel>[^{}]+)\{(?<body>[^{}]*)\}')
if ($cssRules.Count -eq 0) {
  throw "css から規則を 1 つ も引けなかった（$Css）。**0 件 は違反 0 件 と同じ顔をする**"
}
foreach ($rule in $cssRules) {
  if ($rule.Groups['body'].Value -notmatch '(?m)(^|;)\s*display\s*:') { continue }
  foreach ($sel in ($rule.Groups['sel'].Value -split ',')) {
    $s = $sel.Trim()
    # 窓自身 を指していて、`[open]` が付いていない並び
    if ($s -match '^(\.help|dialog)(:[a-z-]+(\([^)]*\))?)*$') {
      $problems.Add("素の $s に display を書いている（閉じた窓が生き残る。[open] の側 に書く）")
    }
  }
}

if (-not $Quiet) { Write-Host ("css の規則 {0} 件 を見た" -f $cssRules.Count) }

# --- 判定 --------------------------------------------------------------------
if ($problems.Count -gt 0) {
  throw ("使い方の窓の章が揃っていない:`n" +
    (($problems | ForEach-Object { "    $_" }) -join "`n") +
    "`n札（html）・節（html）・並び（Playground.fs の helpChapters）の 3 つ を合わせること。" +
    "`n絵は wwwroot/img/help/ に置いて、必ず 1 か所 から参照する（撮り直しは docs/help-shots.md）")
}

if (-not $Quiet) {
  Write-Host ("章は 3 か所 とも同じ並び（{0}）で、絵は参照と実物が 1 対 1" -f ($chapters -join ' -> '))
}
