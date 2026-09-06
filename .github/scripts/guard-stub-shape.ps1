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
$ver = ((Get-Content $verFile -TotalCount 1) -replace '^m_EditorVersion:\s*', '').Trim()
$editor = "C:\Program Files\Unity\Hub\Editor\$ver\Editor\Data\Managed"
$sa = Join-Path $proj 'Library\ScriptAssemblies'

foreach ($need in @($editor, $sa)) {
  if (-not (Test-Path $need)) {
    Write-Host "測っていない: $need が無い"
    Write-Host '  Unity をインストールし、一度 このプロジェクトを開くこと'
    Write-Host '  （ECS はソースで配られるので、Unity が建てるまで dll が無い）'
    exit 1
  }
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
dotnet run --project $tool -c Release -- $RepoRoot
exit $LASTEXITCODE
