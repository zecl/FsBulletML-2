#requires -Version 7
<#
.SYNOPSIS
  焼いた JS に、内包表記の中の `while` が残っていないかを見る。

.DESCRIPTION
  F# の `[ for … do … while … ]` を、Fable は **enumerator の鎖**に焼く ——

      enumerateWhile(() => (at < off), delay(() => append(…)))

  1 歩 進むごとに物が 3 つ 増える。**.NET では素の `while` に落ちる**ので、
  同じソースで .NET だけ速い。

  ## なぜ build も試験も目も見ないか

  答えは合っている。速さだけが違う。しかも**測るのが .NET だと出ない** ——
  v4.9 で実機に当てて初めて出た。

      Scan.lineColumnsAscending   .NET 0.50 ms   焼いた JS 9.8 ms
      Outline.build               .NET 速い       焼いた JS 56 ms

  打鍵 1 回 の合計 18.8 ms のうち、**この形が 13.4 ms** だった。

  ## 何に当てるか

  ブラウザへ載る JS（`$JsRoot`）。**`fable_modules` は見ない** ——
  あちらは Fable が配る現物で、こちらが直す先ではない。

  ## 0 件 は緑にしない

  JS が 1 つ も読めなければ落とす。**焼く前に回すと 0 件 で通ってしまう。**
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  # 既定がこの repo の本番の置き場。渡せるのは校正のため
  [string]$JsRoot,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = ($RepoRoot -replace '\\', '/').TrimEnd('/')
if (-not $JsRoot) { $JsRoot = "$RepoRoot/src/FsBulletML2.LanguageService.Js/js" }

# **指紋は 1 つ。** `enumerateWhile` は Fable が内包の中の while にだけ使う
$Fingerprint = 'enumerateWhile('

if (-not (Test-Path -LiteralPath $JsRoot)) {
  Write-Host "焼いた JS の置き場が無い: $JsRoot"
  Write-Host '**焼いてから回す。** 0 件 で通すと、この門は何も見ていない'
  exit 1
}

$files = @(
  Get-ChildItem -LiteralPath $JsRoot -Recurse -Filter '*.js' -File -ErrorAction SilentlyContinue |
    Where-Object { ($_.FullName -replace '\\', '/') -notmatch '/fable_modules/' }
)

if ($files.Count -eq 0) {
  Write-Host "焼いた JS が 1 つ も無い: $JsRoot"
  Write-Host '**0 件 は緑にしない** —— 焼く前に回すと違反 0 件 と同じ顔をする'
  exit 1
}

$hits = @()
foreach ($f in $files) {
  $rel = ($f.FullName -replace '\\', '/') -replace [regex]::Escape($RepoRoot + '/'), ''
  $n = 0
  foreach ($line in [IO.File]::ReadAllLines($f.FullName)) {
    $n++
    # **字の中の `enumerateWhile` は数えない** —— 但し書きに綴りが出る
    if ($line -like "*$Fingerprint*" -and $line -notmatch '^\s*(\*|//)') {
      $hits += [pscustomobject]@{ Path = $rel; Line = $n }
    }
  }
}

if (-not $Quiet) {
  Write-Host "見た JS $($files.Count) 本"
}

if ($hits.Count -gt 0) {
  Write-Host ''
  Write-Host "内包表記の中の while が $($hits.Count) 件 焼かれている:"
  foreach ($h in $hits) { Write-Host "    $($h.Path):$($h.Line)" }
  Write-Host ''
  Write-Host '素の `while` と `ResizeArray` に書き直す —— 元の F# は'
  Write-Host '`[ for … do … while … ]` の形になっている（`Scan.fs` に直した例が在る）'
  exit 1
}

if (-not $Quiet) { Write-Host '内包表記の中の while は 0 件' }
exit 0
