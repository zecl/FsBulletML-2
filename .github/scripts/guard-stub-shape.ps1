#requires -Version 7
<#
.SYNOPSIS
  COMPILE-ONLY stub の型の形が、本物の Unity と合っているかを見る。

.DESCRIPTION
  **CI では走らない。** 本物の Unity のインストールと、Unity が一度 開いた
  プロジェクト（`Library/ScriptAssemblies` と `Library/PackageCache`）が要る。
  焼き直す人が手元で走らせるための門。

  ## 何を見るのか

  `src/*.Stub` は Unity を持っていない機械でも建つように置いてある。
  **stub の形は、焼いた dll の中身としてそのまま Unity へ渡る。**

      型が値型か参照型か        違うと TypeLoadException
      field か property か      違うと MissingFieldException
      method の引数の型と数     違うと MissingMethodException
      省いた引数の既定値        違うと、黙って別の値で呼ばれる
      その面がどの型に在るか     基底に在るものを派生に置くと、
                                呼び手が本物に無い口を指す

  どれも `dotnet build` では出ない。`-p:UseShippedDlls=true` の門も
  **stub を参照するので最後まで緑になる。** ここだけが本物と突き合わせる。

  実際に 4 段 出た（アセンブリ名 / 値型か参照型か / field か property か /
  signature）。1 段 直すと次が出るので、**まとめて数えるための道具**。

  ## 走らせ方

      pwsh .github/scripts/guard-stub-shape.ps1

  Unity が見つからなければ「測っていない」と言って落ちる。
  **緑にはしない** —— 測れなかったことと、合っていることは別。
#>
[CmdletBinding()]
param([string]$RepoRoot)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = ($RepoRoot -replace '/', '\').TrimEnd('\')

$proj = Join-Path $RepoRoot 'samples\FsBulletML2.Sample.Unity2D.FSharp'
$verFile = Join-Path $proj 'ProjectSettings\ProjectVersion.txt'
if (-not (Test-Path $verFile)) {
  Write-Host "測っていない: $verFile が無い"
  exit 1
}
$sa = Join-Path $proj 'Library\ScriptAssemblies'

if (-not (Test-Path $sa)) {
  Write-Host "測っていない: $sa が無い"
  Write-Host '  Unity で一度 このプロジェクトを開くこと'
  Write-Host '  （ECS はソースで配られるので、Unity が建てるまで dll が無い）'
  exit 1
}

# stub の dll が要る。建っていなければ建てる
$anyStub = Get-ChildItem -Path (Join-Path $RepoRoot 'src') -Directory |
  Where-Object { $_.Name -like '*.Stub' } |
  ForEach-Object { Get-ChildItem -Path $_.FullName -Filter '*.dll' -File -Recurse -ErrorAction SilentlyContinue } |
  Where-Object { $_.FullName -match '\\bin\\Release\\' } | Select-Object -First 1
if (-not $anyStub) {
  Write-Host 'stub の dll が無いので建てる'
  dotnet build (Join-Path $RepoRoot 'FsBulletML2.slnx') -c Release --nologo -v q | Out-Null
  if ($LASTEXITCODE -ne 0) { Write-Host 'build が落ちた'; exit 1 }
}

$tool = Join-Path $RepoRoot 'tools\StubShapeCheck\StubShapeCheck.csproj'
$out = & dotnet run --project $tool -c Release -- $RepoRoot 2>&1
$shape = $LASTEXITCODE
$out | ForEach-Object { Write-Host $_ }

# **Unity の置き場を探すのは道具の側 1 か所 だけ。** ここで同じ順番を書くと、
# 片方だけ直したときに黙ってずれる。使った先を道具に言わせて受け取る
$m = $out | Select-String -Pattern '^Unity Managed: (.+)$' | Select-Object -First 1
if (-not $m) {
  Write-Host ''
  Write-Host '測っていない: Unity の Managed が見つからなかった（上を見ること）'
  exit 1
}
$editor = $m.Matches[0].Groups[1].Value.Trim()

# --- 名前の衝突 ---------------------------------------------------------
#
# **stub が本物より狭いことも壊れの原因になる。** 本物に在る型を stub が
# 持たないと、C# サンプルの `using` で起きる名前の衝突（CS0104）が
# `.Compile` では出ず、Unity でだけ出る。実際に 2 回 踏んだ（Space / Motion）。
#
# ここで数えるのは「こちらの型と Unity の型で単純名がかぶり、かつ
# C# サンプルが両方 の namespace を using しているもの」。
# かぶっている型が stub にも在れば `.Compile` が同じ CS0104 を出すので、
# CI が捕まえる。**stub に無いものだけが危ない。**

Add-Type -AssemblyName System.Reflection.Metadata

function Get-PublicTypes ($path) {
  $fs = [IO.File]::OpenRead($path)
  try {
    $pe = New-Object System.Reflection.PortableExecutable.PEReader($fs)
    try {
      if (-not $pe.HasMetadata) { return @() }
      $md = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
      $out = @()
      foreach ($h in $md.TypeDefinitions) {
        $td = $md.GetTypeDefinition($h)
        if (($td.Attributes -band [System.Reflection.TypeAttributes]::VisibilityMask) -ne
            [System.Reflection.TypeAttributes]::Public) { continue }
        $ns = $md.GetString($td.Namespace)
        if ($ns) { $out += [pscustomobject]@{ Ns = $ns; Name = $md.GetString($td.Name) } }
      }
      foreach ($h in $md.ExportedTypes) {
        $et = $md.GetExportedType($h)
        $ns = $md.GetString($et.Namespace)
        if ($ns) { $out += [pscustomobject]@{ Ns = $ns; Name = $md.GetString($et.Name) } }
      }
      $out
    } finally { $pe.Dispose() }
  } finally { $fs.Dispose() }
}

$csProj = Join-Path $RepoRoot 'samples\FsBulletML2.Sample.Unity2D.CSharp'
$usings = @{}
foreach ($cs in Get-ChildItem -Path (Join-Path $csProj 'Assets') -Filter *.cs -File -Recurse) {
  foreach ($m in [regex]::Matches([IO.File]::ReadAllText($cs.FullName), '(?m)^\s*using\s+(?!static)([A-Za-z0-9_.]+)\s*;')) {
    $usings[$m.Groups[1].Value] = $true
  }
}

$ours = @{}
foreach ($f in Get-ChildItem (Join-Path $csProj 'Assets\FsBulletML2') -Filter 'FsBulletML2*.dll' -File) {
  foreach ($t in Get-PublicTypes $f.FullName) {
    if ($usings.ContainsKey($t.Ns)) { $ours[$t.Name] = $t.Ns }
  }
}

$dirs = @($sa, "$editor\UnityEngine", $editor)
$pc = Join-Path $proj 'Library\PackageCache'
if (Test-Path $pc) {
  $dirs += (Get-ChildItem $pc -Directory -Recurse -Depth 3 |
            Where-Object { $_.Name -in 'Runtime', 'lib' } | ForEach-Object { $_.FullName })
}
$theirs = @{}
foreach ($d in $dirs) {
  foreach ($f in (Get-ChildItem $d -Filter *.dll -File -ErrorAction SilentlyContinue)) {
    $ts = @(); try { $ts = Get-PublicTypes $f.FullName } catch { continue }
    foreach ($t in $ts) { if ($usings.ContainsKey($t.Ns)) { $theirs[$t.Name] = $t.Ns } }
  }
}

# stub が持っている型の単純名
$inStub = @{}
foreach ($sd in (Get-ChildItem (Join-Path $RepoRoot 'src') -Directory | Where-Object { $_.Name -like '*.Stub' })) {
  $dll = Get-ChildItem -Path $sd.FullName -Filter '*.dll' -File -Recurse -ErrorAction SilentlyContinue |
         Where-Object { $_.FullName -match '\\bin\\Release\\' -and $_.BaseName -eq ($sd.Name -replace '\.Stub$', '') } |
         Select-Object -First 1
  if ($dll) { foreach ($t in Get-PublicTypes $dll.FullName) { $inStub[$t.Name] = $true } }
}

$hidden = @()
$seen = @()
foreach ($n in ($ours.Keys | Where-Object { $theirs.ContainsKey($_) } | Sort-Object)) {
  if ($inStub.ContainsKey($n)) { $seen += $n } else { $hidden += $n }
}

''
'名前がかぶる型 {0}: {1}' -f ($seen.Count + $hidden.Count), ((($seen + $hidden) | Sort-Object) -join ', ')
if ($hidden.Count -gt 0) {
  ''
  '** stub に無いので .Compile をすり抜ける型が {0} 件 **' -f $hidden.Count
  foreach ($n in $hidden) { '    {0,-24} こちら {1} / Unity {2}' -f $n, $ours[$n], $theirs[$n] }
  ''
  'src/UnityEngine.Stub などに足すこと。足せば .Compile が同じ CS0104 を出す'
  exit 1
}
'  どれも stub に在る（.Compile が同じ CS0104 を出す）'

exit $shape
