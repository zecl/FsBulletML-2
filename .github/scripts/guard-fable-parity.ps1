# **1 本 のソースが 2 つ の runtime で走る。片方 しか測らないのは穴。**
#
# `fable/XmlScan.fs` は .NET（波線）と Fable が焼いた JS（補完・hover）の
# 両方 で走る。焼いた出力を誰も走らせていないと、Fable でだけ落ちる書き方が
# 素通りする（`Build · Playground.Js` は build するだけ）。
#
# 同じ入力を 2 つ に通して、答えを 1 文字 ずつ突き合わせる。
#
#   入力の表    .github/scripts/fable-parity-cases.json   **1 か所**
#   答えの形    XmlScan.describe                          **1 か所**
#
# どちらも 2 か所 に書くと、そちらが食い違って
# 「中身は同じなのに赤」「違うのに緑」になる。
#
# **0 件 を緑にしない。** 焼いた JS が無い / 表が空 / 走行が 0 件 は全部 赤。
# fable が黙って何も出さなかった状態は「違反 0 件」と同じ顔をする。
#
# 焼くのは `dotnet build src/FsBulletML2.Playground`（BeforeBuild で fable が走る）。
# **焼き直す前にここを回すと、.NET だけ直して JS が古い状態が赤で出る** ——
# それが狙いなので、赤が出たらまず焼き直す。

[CmdletBinding()]
param(
  [string]$RepoRoot,
  [string]$CasesPath,
  [string]$JsPath,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path

if (-not $CasesPath) { $CasesPath = Join-Path $RepoRoot '.github/scripts/fable-parity-cases.json' }
if (-not $JsPath) { $JsPath = Join-Path $RepoRoot 'src/FsBulletML2.Playground/wwwroot/js/XmlScan.js' }
$mjs = Join-Path $RepoRoot '.github/scripts/fable-parity.mjs'
$fsx = Join-Path $RepoRoot '.github/scripts/fable-parity.fsx'

if (-not (Test-Path -LiteralPath $JsPath)) {
  throw "焼いた JS が無い（$JsPath）。**違反 0 件 と同じ顔をする**ので、ここで落とす。" +
        " dotnet build src/FsBulletML2.Playground で焼ける"
}
if (-not (Test-Path -LiteralPath $CasesPath)) { throw "入力の表が無い（$CasesPath）" }
if (-not (Test-Path -LiteralPath $mjs)) { throw "node 側の口が無い（$mjs）" }
if (-not (Test-Path -LiteralPath $fsx)) { throw ".NET 側の口が無い（$fsx）" }

$cases = Get-Content -LiteralPath $CasesPath -Raw | ConvertFrom-Json
# **1 要素 でも配列のまま数えられるように包む。** ConvertFrom-Json は
# 1 要素 の配列を素の値へ畳む
$caseCount = @($cases).Count
if ($caseCount -eq 0) { throw "入力の表が空（$CasesPath）。突き合わせが 0 件 で緑になる" }

function Read-Lines([string]$what, [string[]]$raw) {
  $lines = @($raw | Where-Object { $_ -match "^\d+`t" })
  if ($lines.Count -ne $caseCount) {
    $head = ($raw | Select-Object -First 12) -join "`n"
    throw "$what の答えが $($lines.Count) 行（表は $caseCount 件）。`n$head"
  }
  $map = @{}
  foreach ($l in $lines) {
    $i, $v = $l -split "`t", 2
    $map[[int]$i] = $v
  }
  $map
}

$nodeRaw = & node $mjs $CasesPath $JsPath 2>&1 | ForEach-Object { [string]$_ }
$node = Read-Lines 'node（焼いた JS）' $nodeRaw

$netRaw = & dotnet fsi $fsx $CasesPath 2>&1 | ForEach-Object { [string]$_ }
$net = Read-Lines '.NET（ソースを load）' $netRaw

$bad = New-Object System.Collections.Generic.List[string]
for ($i = 0; $i -lt $caseCount; $i++) {
  # **`-cne`。** 素の `-ne` は大小を無視するので、`ctx` と `CTX` が同じに見える
  # （較正でそれを踏んだ）。要素名も属性値も大小で意味が変わる
  if ($node[$i] -cne $net[$i]) {
    $note = @($cases)[$i].note
    $bad.Add("  [$i] $note")
    $bad.Add("      .NET: $($net[$i])")
    $bad.Add("      node: $($node[$i])")
  }
}

if ($bad.Count -gt 0) {
  throw ("同じソースが 2 つ の runtime で違う答えを出した:`n" + ($bad -join "`n"))
}

if (-not $Quiet) {
  Write-Host "$caseCount 件 を .NET と node で突き合わせた / 食い違い 0 件"
}
