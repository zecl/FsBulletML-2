#requires -Version 7
<#
.SYNOPSIS
  `FsBulletML2.Core` が Fable で焼けて、焼いた JS が .NET と同じ答えを出すか。

.DESCRIPTION
  **build も `dotnet test` もこれを見ない。** .NET では全部 通るから。

  Fable は proj まるごとしか焼けない（中のどのファイルを焼くか選ぶ口が無い）ので、
  `Core` に **`System.Xml` や F# の reflection が 1 か所 でも入ると
  `Core` ごと焼けなくなる。** v5.5 で外すまで、実際に 3 か所 在った ——

      Xml.fs        XmlReader / XmlTextWriter   -> Parser へ移した
      DTD.fs        XmlSink（XmlWriter を包む） -> Parser へ移した
      Util.fs       XPathDocument（旧い式の評価）-> 消した（木を評価する）

  外した後も、**次に足した人は何も赤くならない。** だからここで焼く。

  ## 「焼けた」と「走る」は別

  焼いて終わりにしない。**焼いた JS を node で走らせて、同じソースを .NET で
  走らせた答えと突き合わせる。** build が通ったことは「引けた」の証拠ではない。

  ## 突き合わせるのは整数だけ

  **JS に単精度が無い。** Fable は `float32` を倍精度のまま持つので、
  座標や速さは単精度の桁数を超えたところから必ず割れる（実測で 7 桁 目）——
  そこを見ると、この門は毎回 赤くなって誰も読まなくなる。

  撃った数・撃ったフレーム・終わった数は丸めを跨がないので一致する。

  ## 0 件 を緑にしない

  焼けていない / JS が出ていない / 撃った数が 0 は全部 赤。
  **fable が黙って何も出さなかった状態は「違反 0 件」と同じ顔をする。**
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  # 焼く proj の置き場。**校正はここを差し替える**（System.Xml を混ぜた写しへ向ける）
  [string]$ProbeRoot,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
if (-not $ProbeRoot) { $ProbeRoot = Join-Path $RepoRoot 'tools/CoreFableProbe' }

# **材料の欠けは `exit 1` で言う。`throw` にしない** ——
# 呼ぶ側（校正）は `$LASTEXITCODE` で赤を数えていて、`throw` だと
# そこが立たないまま走行ごと止まる（校正を書いて初めて出た）
foreach ($n in 'CoreFableProbe.fsproj', 'Probe.fs', 'run.mjs', 'run.fsx') {
  $p = Join-Path $ProbeRoot $n
  if (-not (Test-Path -LiteralPath $p)) {
    Write-Host "門の材料が無い（$p）"
    exit 1
  }
}

# **fable の版は 1 か所。** 器の側の tool manifest をそのまま使う ——
# 2 か所 に書くと、片方 だけ上げたときに「同じ版で焼いている」が嘘になる
$toolDir = Join-Path $RepoRoot 'src/FsBulletML2.LanguageService.Js'
if (-not (Test-Path -LiteralPath (Join-Path $toolDir '.config/dotnet-tools.json'))) {
  Write-Host "fable の tool manifest が無い（$toolDir/.config）"
  exit 1
}

$out = Join-Path ([IO.Path]::GetTempPath()) ('core-fable-' + [guid]::NewGuid().ToString('N'))

try {
  # ---- 焼く ----------------------------------------------------------------
  Push-Location $toolDir
  try {
    & dotnet tool restore 2>&1 | Out-Null
    $bake = & dotnet tool run fable -- (Join-Path $ProbeRoot 'CoreFableProbe.fsproj') -o $out --noCache 2>&1 |
      ForEach-Object { [string]$_ }
    $baked = ($LASTEXITCODE -eq 0)
  } finally { Pop-Location }

  if (-not $baked) {
    Write-Host 'Core が Fable で焼けなくなっている:'
    # FABLE の行だけ出す。fable は通した FSHARP の情報行も混ぜる
    $why = @($bake | Where-Object { $_ -match 'error' })
    if ($why.Count -eq 0) { $why = @($bake | Select-Object -Last 12) }
    foreach ($l in ($why | Select-Object -First 20)) { Write-Host "    $l" }
    Write-Host ''
    Write-Host '**Fable は proj まるごとしか焼けない。** `System.Xml` や'
    Write-Host 'F# の reflection を `Core` に足すと、`Core` ごと焼けなくなる ——'
    Write-Host '置き場は `Parser`（xml を読む側）。式は `Expr.NumExpr` の木を評価する'
    exit 1
  }

  # ---- 焼けた物を数える（0 件 を緑にしない）--------------------------------
  $js = @(Get-ChildItem -LiteralPath $out -Recurse -Filter '*.js' -File -ErrorAction SilentlyContinue |
    Where-Object { ($_.FullName -replace '\\', '/') -notmatch '/fable_modules/' })
  if ($js.Count -eq 0) {
    Write-Host "焼いた JS が 1 つ も無い: $out"
    Write-Host '**fable が黙って何も出さなかった状態は「違反 0 件」と同じ顔をする**'
    exit 1
  }
  # エンジンの芯が焼けているか。**Probe.js だけ出て Core が落ちていても
  # 「焼けた」の顔をする**ので、名指しで見る
  foreach ($need in 'Step.js', 'Api.js', 'BulletmlRead.js', 'Expr.js') {
    if (-not ($js | Where-Object { $_.Name -eq $need })) {
      Write-Host "エンジンの $need が焼けていない（$out）"
      exit 1
    }
  }

  # ---- 走らせる ------------------------------------------------------------
  $nodeOut = (& node (Join-Path $ProbeRoot 'run.mjs') $out 2>&1 | ForEach-Object { [string]$_ }) -join "`n"
  if ($LASTEXITCODE -ne 0) {
    Write-Host '焼いた JS が node で走らなかった:'
    Write-Host $nodeOut
    exit 1
  }

  $core = @(Get-ChildItem -LiteralPath (Join-Path $RepoRoot 'src/FsBulletML2.Core/bin') -Recurse `
    -Filter 'FsBulletML2.Core.dll' -File -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending)
  $dsl = @(Get-ChildItem -LiteralPath (Join-Path $RepoRoot 'src/FsBulletML2.Dsl/bin') -Recurse `
    -Filter 'FsBulletML2.Dsl.dll' -File -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending)
  if ($core.Count -eq 0 -or $dsl.Count -eq 0) {
    Write-Host 'Core / Dsl の dll が無い。**焼く前に build する** ——'
    Write-Host '片方 しか走らせないと突き合わせが 0 件 で緑になる'
    exit 1
  }

  Push-Location $ProbeRoot
  try {
    $netOut = (& dotnet fsi "-r:$($core[0].FullName)" "-r:$($dsl[0].FullName)" 'run.fsx' 2>&1 |
      ForEach-Object { [string]$_ }) -join "`n"
    $ranNet = ($LASTEXITCODE -eq 0)
  } finally { Pop-Location }
  if (-not $ranNet) {
    Write-Host '同じソースが .NET で走らなかった:'
    Write-Host $netOut
    exit 1
  }

  # ---- 突き合わせる --------------------------------------------------------
  $nodeLine = ($nodeOut -split "`n" | Where-Object { $_ -match '^spawned=' } | Select-Object -Last 1)
  $netLine = ($netOut -split "`n" | Where-Object { $_ -match '^spawned=' } | Select-Object -Last 1)
  if (-not $nodeLine -or -not $netLine) {
    Write-Host '答えの行が取れなかった（片方 でも空なら赤）:'
    Write-Host "    node: $nodeOut"
    Write-Host "    .NET: $netOut"
    exit 1
  }

  # **撃った数が 0 なら緑にしない。** 何も撃たない弾幕でも両方 一致するので、
  # 突き合わせは通るのに何も確かめていない
  if ($nodeLine -notmatch 'spawned=(\d+)' -or [int]$Matches[1] -eq 0) {
    Write-Host "撃った数が 0（$nodeLine）。**一致するが何も確かめていない**"
    exit 1
  }

  # `-cne`。素の `-ne` は大小を無視する
  if ($nodeLine -cne $netLine) {
    Write-Host '同じソースが 2 つ の runtime で違う答えを出した:'
    Write-Host "    .NET: $netLine"
    Write-Host "    node: $nodeLine"
    Write-Host ''
    Write-Host '整数だけを見ている（座標は JS に単精度が無いので必ず割れる）——'
    Write-Host 'ここが割れたなら、**焼いた JS だけ違う道を通っている**'
    exit 1
  }

  if (-not $Quiet) {
    Write-Host "Core を焼いた（JS $($js.Count) 本）/ node と .NET で一致: $nodeLine"
  }
  exit 0
} finally {
  Remove-Item -LiteralPath $out -Recurse -Force -ErrorAction SilentlyContinue
}
