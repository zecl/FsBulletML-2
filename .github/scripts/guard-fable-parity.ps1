# **1 本 のソースが 2 つ の runtime で走る。片方 しか測らないのは穴。**
#
# `fable/XmlScan.fs` と `LanguageService/SourceKind.fs` は、.NET（host）と
# Fable が焼いた JS（ブラウザ側）の両方 で走る。焼いた出力を誰も走らせて
# いないと、Fable でだけ落ちる書き方が素通りする
# （`Build · Playground.Js` は build するだけ）。
#
# 同じ入力を 2 つ に通して、答えを 1 文字 ずつ突き合わせる。
#
#   入力の表    .github/scripts/fable-parity-cases.json     **1 か所**
#   当てる先    .github/scripts/fable-parity-targets.json   **1 か所**
#   答えの形    各 target の describe                        **1 か所**
#
# どれも 2 か所 に書くと、そちらが食い違って
# 「中身は同じなのに赤」「違うのに緑」になる。
#
# ## proj をまたぐ
#
# `SourceKind` は `FsBulletML2.LanguageService` に在り、Fable は
# `ProjectReference` を辿って焼く。**「引ける」を build だけで見ない** ——
# build が通っても、焼いた JS を誰も走らせていなければ引けたことにならない。
#
# ## 0 件 を緑にしない
#
# 焼いた JS が無い / 表が空 / **どれかの target に 1 件 も無い** /
# 走行が 0 件 は全部 赤。fable が黙って何も出さなかった状態は
# 「違反 0 件」と同じ顔をする。target を表に足しただけで 1 件 も
# 当てていない状態も同じ顔をするので、そちらも数える。
#
# 焼くのは `dotnet build src/FsBulletML2.Playground`（BeforeBuild で fable が走る）。
# **焼き直す前にここを回すと、.NET だけ直して JS が古い状態が赤で出る** ——
# それが狙いなので、赤が出たらまず焼き直す。

[CmdletBinding()]
param(
  [string]$RepoRoot,
  [string]$CasesPath,
  # 当てる先の表。**較正はここを差し替える**（焼いた JS を変異させた写しへ向ける）
  [string]$TargetsPath,
  # 表の `js` を解く根。既定は `$RepoRoot`
  [string]$JsRoot,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path

if (-not $CasesPath) { $CasesPath = Join-Path $RepoRoot '.github/scripts/fable-parity-cases.json' }
if (-not $TargetsPath) { $TargetsPath = Join-Path $RepoRoot '.github/scripts/fable-parity-targets.json' }
if (-not $JsRoot) { $JsRoot = $RepoRoot }
$mjs = Join-Path $RepoRoot '.github/scripts/fable-parity.mjs'
$fsx = Join-Path $RepoRoot '.github/scripts/fable-parity.fsx'

if (-not (Test-Path -LiteralPath $CasesPath)) { throw "入力の表が無い（$CasesPath）" }
if (-not (Test-Path -LiteralPath $TargetsPath)) { throw "当てる先の表が無い（$TargetsPath）" }
if (-not (Test-Path -LiteralPath $mjs)) { throw "node 側の口が無い（$mjs）" }
if (-not (Test-Path -LiteralPath $fsx)) { throw ".NET 側の口が無い（$fsx）" }

# **1 要素 でも配列のまま数えられるように包む。** ConvertFrom-Json は
# 1 要素 の配列を素の値へ畳む
$cases = @(Get-Content -LiteralPath $CasesPath -Raw | ConvertFrom-Json)
$targets = @(Get-Content -LiteralPath $TargetsPath -Raw | ConvertFrom-Json)
$caseCount = $cases.Count
if ($caseCount -eq 0) { throw "入力の表が空（$CasesPath）。突き合わせが 0 件 で緑になる" }
if ($targets.Count -eq 0) { throw "当てる先の表が空（$TargetsPath）。突き合わせが 0 件 で緑になる" }

$perTarget = @{}
foreach ($t in $targets) {
  $js = Join-Path $JsRoot $t.js
  if (-not (Test-Path -LiteralPath $js)) {
    throw "焼いた JS が無い（$js）。**違反 0 件 と同じ顔をする**ので、ここで落とす。" +
          " dotnet build src/FsBulletML2.Playground で焼ける"
  }
  # **表に足しただけで 1 件 も当てていない target を通さない。**
  # 「引ける形にした」と「引けることを見た」は別
  $name = $t.target
  $n = @($cases | Where-Object { $_.target -ceq $name }).Count
  if ($n -eq 0) {
    throw "target『$name』に当てる入力が 1 件 も無い（$CasesPath）。" +
          "**違反 0 件 と同じ顔をする**ので、ここで落とす"
  }
  $perTarget[$name] = $n
}

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

$nodeRaw = & node $mjs $CasesPath $TargetsPath $JsRoot 2>&1 | ForEach-Object { [string]$_ }
$node = Read-Lines 'node（焼いた JS）' $nodeRaw

$netRaw = & dotnet fsi $fsx $CasesPath 2>&1 | ForEach-Object { [string]$_ }
$net = Read-Lines '.NET（ソースを load）' $netRaw

$bad = New-Object System.Collections.Generic.List[string]
for ($i = 0; $i -lt $caseCount; $i++) {
  # **`-cne`。** 素の `-ne` は大小を無視するので、`ctx` と `CTX` が同じに見える
  # （較正でそれを踏んだ）。要素名も属性値も大小で意味が変わる
  if ($node[$i] -cne $net[$i]) {
    $c = $cases[$i]
    $bad.Add("  [$i] $($c.target) / $($c.note)")
    $bad.Add("      .NET: $($net[$i])")
    $bad.Add("      node: $($node[$i])")
  }
}

if ($bad.Count -gt 0) {
  throw ("同じソースが 2 つ の runtime で違う答えを出した:`n" + ($bad -join "`n"))
}

if (-not $Quiet) {
  $per = ($targets | ForEach-Object { "{0} {1} 件" -f $_.target, $perTarget[$_.target] }) -join ' / '
  Write-Host "$caseCount 件 を .NET と node で突き合わせた（$per）/ 食い違い 0 件"
}
