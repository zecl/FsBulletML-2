#requires -Version 7
<#
.SYNOPSIS
  guard-shipped-refs.ps1 の校正。

.DESCRIPTION
  **落ちるところを見ておく。** この門が守るのは
  「同梱 dll が、名乗りどおりのアセンブリから型を引いていること」で、
  それが崩れた実物が git に残っている ——
  stub が `AssemblyName=UnityEngine` 1 本 だった頃の dll。

  その版を取り出して当て、**20 型 で赤くなる**ことを見る。
  いまの dll では緑。この 2 点 で目盛りが合う。
#>
[CmdletBinding()]
param([string]$RepoRoot)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = ($RepoRoot -replace '/', '\').TrimEnd('\')
$guard = Join-Path $RepoRoot '.github/scripts/guard-shipped-refs.ps1'

# stub を割る前の同梱 dll。ここに ECS と URP の型が
# 「UnityEngine に在る」として焼き込まれている
$BrokenAt = '75fcf1f'
$BrokenDll = 'samples/FsBulletML2.Sample.Unity2D.FSharp/Assets/FsBulletML2/FsBulletML2.Sample.Unity2D.FSharp.dll'

$ok = 0
$ng = 0
function Check ($name, $cond, $detail) {
  if ($cond) { '  ok   ' + $name; $script:ok++ }
  else { '  NG   ' + $name; if ($detail) { '         ' + $detail }; $script:ng++ }
}

''
'=== いまの同梱 dll'
$out = & pwsh -NoProfile -File $guard -RepoRoot $RepoRoot 2>&1 | Out-String
Check 'いまの同梱 dll は緑' ($LASTEXITCODE -eq 0) ($out.Trim() -split "`n" | Select-Object -Last 1)

''
'=== stub を割る前の dll（' + $BrokenAt + '）'
$tmp = Join-Path ([IO.Path]::GetTempPath()) ('shipped-refs-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tmp -Force | Out-Null
try {
  # binary をそのまま取り出す（PowerShell のリダイレクトはテキストに壊す）
  $sha = (git -C $RepoRoot rev-parse "${BrokenAt}:${BrokenDll}").Trim()
  $dst = Join-Path $tmp 'FsBulletML2.Sample.Unity2D.FSharp.dll'
  $p = Start-Process -FilePath 'git' -ArgumentList @('-C', $RepoRoot, 'cat-file', 'blob', $sha) `
                     -RedirectStandardOutput $dst -NoNewWindow -Wait -PassThru
  if ($p.ExitCode -ne 0) { throw "取り出せない: $sha" }

  $out2 = & pwsh -NoProfile -File $guard -RepoRoot $RepoRoot -ShippedDir $tmp 2>&1 | Out-String
  $ec2 = $LASTEXITCODE
  $n = 0
  if ($out2 -match '型が (\d+) 件') { $n = [int]$Matches[1] }

  Check '割る前の dll は赤くなる' ($ec2 -ne 0) "exit=$ec2"
  # UnityEngine から引いていた 20 型 のうち、Allocator と NativeArray<> の 2 型 は
  # **本物でも UnityEngine に在る**（パッケージのほうではない）ので、
  # あの dll の引き方で正しかった。残る 18 型 が名乗り違い
  Check '赤くなる型は 18 件' ($n -eq 18) "実際 $n 件"
  foreach ($want in 'Unity.Entities', 'Unity.Mathematics',
                    'Unity.Transforms', 'Unity.Rendering', 'UnityEngine.Rendering.Universal') {
    Check ("$want を UnityEngine から引いていると出る") `
          ($out2 -match ([regex]::Escape($want) + '\s+<- UnityEngine')) ''
  }
} finally {
  Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue
}

''
'=== 網が外れたときは緑にしない'
$empty = Join-Path ([IO.Path]::GetTempPath()) ('shipped-refs-empty-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path (Join-Path $empty 'src') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $empty 'samples') -Force | Out-Null
try {
  & pwsh -NoProfile -File $guard -RepoRoot $empty 2>&1 | Out-Null
  Check '表が空なら落ちる（stub を読めていない）' ($LASTEXITCODE -ne 0) ''
} finally {
  Remove-Item -LiteralPath $empty -Recurse -Force -ErrorAction SilentlyContinue
}

''
if ($ng -eq 0) { '校正 {0} 点 すべて一致' -f $ok; exit 0 }
'校正 {0} 点 中 {1} 点 が外れた' -f ($ok + $ng), $ng
exit 1
