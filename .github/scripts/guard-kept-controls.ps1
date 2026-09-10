#requires -Version 7
<#
.SYNOPSIS
  「開き直しても覚えている口」の一覧が、画面に在る口を指しているかを見る。

.DESCRIPTION
  v3.5 は**残す口の id を 1 か所 の配列に持って、変わったら丸ごと書く**形で
  作ってある（`keptControls`）。口ごとにハンドラを足さない代わりに、
  **配列と html が食い違っても何も起きない** ——

      配列に在って html に無い    黙って飛ばす。**その口だけ覚えなくなる**
      チェックの扱いが食い違う    `value` で読むと `"on"` が入り、
                                  戻すときに `checked` にならない

  どちらも build も試験も目も出ない。開き直したときに
  **その 1 つ だけが戻っていない**という形でしか現れず、
  しかも「戻さない口」（`pattern` など）が既定で在るので、
  **忘れているのか、そういう決まりなのかが見分けられない。**

  v4.0.2 で 2 つ 目 のチェック（`marks`）を足したときに、
  `if id = "trail"` と書いてある行が 2 か所 在った ——
  **書いたほうだけが直る**形だったので、ここに門を置く。

  ## 見るもの

      1  `keptControls` の id が、html に `id="..."` として在るか
      2  `keptChecks` の id が `type="checkbox"` か
      3  `keptChecks` が `keptControls` の部分集合か
      4  html の checkbox で `keptControls` に入っていないものを名前で出す
         （**落とさない。** 覚えない決まりの口も在る）

  ## 読み方

  **コメントを先に落とす。** この repo の html はコメントが濃く、
  その中に例示として `<input …>` が書いてあることが在る
  （`guard-control-names.ps1` が同じ理由で同じことをしている）。

  校正は guard-kept-controls.Tests.ps1。
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  [string]$SourcePath,
  [string]$HtmlPath,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = ($RepoRoot -replace '\\', '/').TrimEnd('/')
if (-not $SourcePath) { $SourcePath = "$RepoRoot/src/FsBulletML2.Playground/fable/Playground.fs" }
if (-not $HtmlPath) { $HtmlPath = "$RepoRoot/src/FsBulletML2.Playground/wwwroot/index.html" }

foreach ($p in @($SourcePath, $HtmlPath)) {
  if (-not (Test-Path -LiteralPath $p)) { throw "見つからない: $p" }
}

$src = [IO.File]::ReadAllText($SourcePath)
$html = [IO.File]::ReadAllText($HtmlPath)

# --- 配列を読む。**名前で拾って、次の [| ... |] を取る**
function Read-IdArray {
  param([string]$Text, [string]$Name)
  $m = [regex]::Match($Text, "let\s+(?:private\s+)?$([regex]::Escape($Name))\s*=\s*(?:\r?\n\s*)?\[\|(?<body>[^\]]*)\|\]")
  if (-not $m.Success) { throw "$Name の配列が読めない" }
  [regex]::Matches($m.Groups['body'].Value, '"([^"]+)"') |
    ForEach-Object { $_.Groups[1].Value }
}

$kept = @(Read-IdArray -Text $src -Name 'keptControls')
$checks = @(Read-IdArray -Text $src -Name 'keptChecks')

if ($kept.Count -eq 0) { throw 'keptControls が空。読み方が壊れている' }

# --- html を読む。コメントを先に落とす
$body = [regex]::Replace($html, '(?s)<!--.*?-->', '')

# id とその札の中身を拾う。**札の中に居る id だけ** —— 字の中の `id="x"` は当たらない
$tags = @{}
foreach ($m in [regex]::Matches($body, '(?s)<(input|select|textarea|button)\b(?<attrs>[^>]*)>')) {
  $attrs = $m.Groups['attrs'].Value
  $idm = [regex]::Match($attrs, 'id="(?<id>[^"]+)"')
  if ($idm.Success) { $tags[$idm.Groups['id'].Value] = $attrs }
}

$problems = @()

# 1 —— 覚える口が画面に在るか
foreach ($id in $kept) {
  if (-not $tags.ContainsKey($id)) {
    $problems += "覚える口 '$id' が html に無い（黙って飛ばされる）"
  }
}

# 2 —— チェックの扱いが合っているか
foreach ($id in $checks) {
  if ($tags.ContainsKey($id)) {
    if ($tags[$id] -notmatch 'type="checkbox"') {
      $problems += "'$id' は checked で読み書きしているのに checkbox でない"
    }
  }
}

# 3 —— checked で読む口が、そもそも覚える口か
foreach ($id in $checks) {
  if ($kept -notcontains $id) {
    $problems += "'$id' は keptChecks に在るが keptControls に無い（読み書きされない）"
  }
}

# 4 —— **落とさない。** 覚えない決まりの checkbox も在る
$loose = @()
foreach ($id in $tags.Keys) {
  if ($tags[$id] -match 'type="checkbox"' -and $kept -notcontains $id) { $loose += $id }
}

if (-not $Quiet) {
  Write-Host ("覚える口 {0} 個 / うち checkbox {1} 個" -f $kept.Count, $checks.Count)
  if ($loose.Count -gt 0) {
    Write-Host ("  覚えない checkbox {0} 個: {1}" -f $loose.Count, (($loose | Sort-Object) -join ', '))
  }
}

if ($problems.Count -gt 0) {
  foreach ($p in $problems) { Write-Host "  $p" }
  throw ("覚える口が画面と食い違っている: {0} 件" -f $problems.Count)
}

if (-not $Quiet) { Write-Host '覚える口は、どれも画面の口を指している' }
