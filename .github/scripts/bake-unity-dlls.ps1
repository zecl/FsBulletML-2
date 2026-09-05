#requires -Version 7
<#
.SYNOPSIS
  Unity サンプルに同梱している dll を焼き直す。

.DESCRIPTION
  Unity のサンプルは `ProjectReference` ではなく `Assets/FsBulletML2/` に置いた
  dll を見る。**Core を触ったら、ここも焼き直さないと古びる** ——
  ソースは新しい名前を指し、dll は古い名前しか持たない状態になり、
  次に誰かが Unity で開くまで誰も気づかない（実際に 2 回 やっている）。

  手順そのものは samples/FsBulletML2.Sample.Unity2D.CSharp/README.md に
  書いてあった。これはそれを 1 本 にしただけ。

  **焼く相手は「いま追跡されている dll の名前」から引く。** 増やさないし
  減らさない —— 一覧を script に持つと、置き場所が変わったとき片方だけ古びる。
  `.meta`（Unity の取り込み情報）は Unity の持ち物なので触らない。

  `netstandard2.1` を指定する。既定は `net10.0` との複数ターゲットで、
  Unity は `net10.0` を読めない。
  `CopyLocalLockFileAssemblies` を立てるのは、`FSharp.Core` と
  `FSharp.Control.R3` が PackageReference なので、それが無いと library の
  出力に落ちてこないため。
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  # 焼かずに、焼いたら変わるものだけ数える
  [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = $RepoRoot -replace '\\', '/'

# 焼き先 -> その中身を建てる代表プロジェクト。
# **ここだけが手で書いた対応。** 残り（どの dll を置くか・何を参照するか）は
# 追跡している一覧と proj から引く。
$Reps = @{
  'samples/FsBulletML2.Sample.Unity2D.CSharp/Assets/FsBulletML2' =
    'samples/FsBulletML2.Bullets.Dsl/FsBulletML2.Bullets.Dsl.fsproj'
  'samples/FsBulletML2.Sample.Unity2D.FSharp/Assets/FsBulletML2' =
    'samples/FsBulletML2.Sample.Unity2D.FSharp/FsBulletML2.Sample.Unity2D.FSharp/FsBulletML2.Sample.Unity2D.FSharp.fsproj'
}

$tracked = @(git -C $RepoRoot ls-files '*/Assets/FsBulletML2/*.dll' | ForEach-Object { $_ -replace '\\', '/' })
$dests = @($tracked | ForEach-Object { $_.Substring(0, $_.LastIndexOf('/')) } | Sort-Object -Unique)

foreach ($d in $dests) {
  if (-not $Reps.ContainsKey($d)) { throw "焼き先 $d に対応する代表プロジェクトが無い" }
}
foreach ($d in $Reps.Keys) {
  if ($dests -notcontains $d) { throw "代表プロジェクトが在るのに、焼き先 $d に dll が 1 本 も無い" }
}

$changed = 0
$same = 0

foreach ($dest in $dests) {
  $proj = $Reps[$dest]
  Write-Host "=== $dest"
  Write-Host "    建てる: $proj"

  # **-WhatIf でも建てる。** 「書かない」であって「建てない」ではない ——
  # 建てずに出力を見ると、直前に別の build が上書きした残骸と比べることになる
  # （素の `dotnet build` は CopyLocalLockFileAssemblies を立てないので、
  #  FSharp.Core.dll が出力から消える。それを「同梱の一覧が古い」と読んだ）
  Push-Location $RepoRoot
  try {
    dotnet build $proj -c Release -f netstandard2.1 -p:CopyLocalLockFileAssemblies=true --nologo -v q | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "build が落ちた: $proj" }
  } finally { Pop-Location }

  $out = Join-Path $RepoRoot ((Split-Path $proj -Parent) + '/bin/Release/netstandard2.1')
  if (-not (Test-Path -LiteralPath $out)) { throw "出力が無い: $out" }

  foreach ($rel in ($tracked | Where-Object { $_.StartsWith($dest + '/') })) {
    $name = Split-Path $rel -Leaf
    $src = Join-Path $out $name
    # **出力を glob しない。追跡している名前で引く。**
    # bin には旧実装（FsBulletML.*）の残骸が居るので、拾うと巻き込む
    if (-not (Test-Path -LiteralPath $src)) {
      throw "$name が $out に無い。焼き元がずれているか、同梱の一覧が古い"
    }
    $dstFull = Join-Path $RepoRoot $rel
    $hBefore = (Get-FileHash -LiteralPath $dstFull -Algorithm SHA256).Hash
    $hAfter = (Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash
    if ($hBefore -eq $hAfter) { $same++; continue }
    $changed++
    $sizeBefore = (Get-Item -LiteralPath $dstFull).Length
    $sizeAfter = (Get-Item -LiteralPath $src).Length
    Write-Host ("    {0,-40} {1:N0} -> {2:N0} バイト" -f $name, $sizeBefore, $sizeAfter)
    if (-not $WhatIf) { Copy-Item -LiteralPath $src -Destination $dstFull -Force }
  }
}

Write-Host ''
if ($WhatIf) {
  Write-Host "焼いたら変わる $changed 本 / 同じ $same 本（何も書いていない）"
} else {
  Write-Host "焼き直した $changed 本 / 同じ $same 本"
}
Write-Host '`.meta` は触っていない（Unity の持ち物）'
