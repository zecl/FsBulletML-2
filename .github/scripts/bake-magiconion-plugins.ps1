#requires -Version 7
<#
.SYNOPSIS
  Unity の MagicOnion client に、要る dll を焼き直す。

.DESCRIPTION
  **MagicOnion 7 は Unity 向けの UPM package を持っていない。**
  git URL で引こうとして 1 度 外した ——
  `src/MagicOnion.Client.Unity/Assets/Plugins/MagicOnion` は実在せず、
  repo の中 に `package.json` は 1 つ も無い（配布は NuGet）。

  openupm の NuGet 橋（`org.nuget.*`）にも居ない。
  **だから dll を置く。** この repo が F# の dll でやっているのと同じ形。

  ## 焼き元

  `samples/FsBulletML2.Sample.Unity2D.MagicOnion.Compile` を
  netstandard2.1 で建てて、その出力から**名前で引く。**

  **glob しない。** あの出力には `UnityEngine.dll` などの stub が並んでいて、
  拾うと本物を上書きする形で Assets に入る（CS7069 の嵐 になる）。

  ## 置かないもの

      FsBulletML2.Sample.Shared.MagicOnion   口。Unity は package のソースで読む
      R3 / UnityEngine.* / Unity.*           Unity 側 が持っている
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
$RepoRoot = ($RepoRoot -replace '/', '\').TrimEnd('\')

$proj = Join-Path $RepoRoot 'samples\FsBulletML2.Sample.Unity2D.MagicOnion.Compile\FsBulletML2.Sample.Unity2D.MagicOnion.Compile.csproj'
# **置き先 は口 の package の中。** Assets の下 に置くと、package 側 の asmdef
# （FsBulletML2.Sample.Shared.MagicOnion）から引けない ——
# Unity の依存は **Assets -> Packages の向きにしか流れない。**
# package の中 に置けば、package 自身 と Assets の両方 から見える
$dest = Join-Path $RepoRoot 'samples\FsBulletML2.Sample.Shared.MagicOnion\Plugins'

# **置くものを名前で列挙する。** ここが唯一 の手書き。
# 出どころは console client（FsBulletML2.Sample.Client.MagicOnion）の配り物 ——
# **あれは実際に走って 300 コマ 受けている**ので、閉じていることが確かめてある。
$wanted = @(
  'MagicOnion.Abstractions.dll'
  'MagicOnion.Client.dll'
  'MagicOnion.Serialization.MessagePack.dll'
  'MagicOnion.Shared.dll'
  'Grpc.Core.Api.dll'
  'Grpc.Net.Client.dll'
  'Grpc.Net.Common.dll'
  'MessagePack.dll'
  'MessagePack.Annotations.dll'
  'Microsoft.Extensions.DependencyInjection.Abstractions.dll'
  'Microsoft.Extensions.Logging.Abstractions.dll'
  'Microsoft.NET.StringTools.dll'
)

Push-Location $RepoRoot
try {
  dotnet build $proj -c Release -f netstandard2.1 -p:CopyLocalLockFileAssemblies=true --nologo -v q | Out-Null
  if ($LASTEXITCODE -ne 0) { throw "build が落ちた: $proj" }
}
finally { Pop-Location }

$out = Join-Path (Split-Path $proj -Parent) 'bin\Release\netstandard2.1'
if (-not (Test-Path -LiteralPath $out)) { throw "出力が無い: $out" }

if (-not $WhatIf -and -not (Test-Path -LiteralPath $dest)) {
  New-Item -ItemType Directory -Path $dest -Force | Out-Null
}

$changed = 0
$same = 0

foreach ($name in $wanted) {
  $src = Join-Path $out $name
  if (-not (Test-Path -LiteralPath $src)) {
    throw "$name が $out に無い。焼き元がずれているか、置くものの一覧が古い"
  }

  $to = Join-Path $dest $name
  if ((Test-Path -LiteralPath $to) -and
      (Get-FileHash -LiteralPath $to -Algorithm SHA256).Hash -eq (Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash) {
    $same++
    continue
  }

  $changed++
  Write-Host ("    {0,-52} {1:N0} バイト" -f $name, (Get-Item -LiteralPath $src).Length)
  if (-not $WhatIf) { Copy-Item -LiteralPath $src -Destination $to -Force }
}

# **要らないものが紛れていないか数える。** 焼き先 は手で触れる場所なので、
# 次の人が glob で放り込むと Unity の stub が混ざる
if (-not $WhatIf -and (Test-Path -LiteralPath $dest)) {
  $extra = @(Get-ChildItem -LiteralPath $dest -File -Filter *.dll |
    Where-Object { $wanted -notcontains $_.Name })
  if ($extra.Count -gt 0) {
    throw ("焼き先 に一覧に無い dll が居る:`n" +
      (($extra | ForEach-Object { "    " + $_.Name }) -join "`n"))
  }
}

Write-Host ''
if ($WhatIf) {
  Write-Host "焼いたら変わる $changed 本 / 同じ $same 本（何も書いていない）"
}
else {
  Write-Host "焼き直した $changed 本 / 同じ $same 本（全部 で $($wanted.Count) 本）"
}
Write-Host '`.meta` は触っていない（Unity の持ち物）'
