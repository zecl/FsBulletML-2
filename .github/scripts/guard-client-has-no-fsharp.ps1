#requires -Version 7
<#
.SYNOPSIS
  MagicOnion の client が、F# を 1 本 も抱えていないことを数える。

.DESCRIPTION
  帯 1（E1.x）が示そうとしているのは 1 つ だけ ——
  **弾幕エンジンをサーバーで走らせれば、client は F# を知らなくて済む。**

  在来の Unity サンプルは、`Assets/FsBulletML2/` に dll を 5 本 抱えている。

      FsBulletML2.Core.dll / Front.dll / Dsl.dll / Bullets.Dsl.dll
      FSharp.Core.dll          <- これが居ることが代償

  **消し忘れても絵 は出てしまう。** 参照が残っているだけで動くので、
  目 では出ない。だから数える。

  ## 見るのは 3 つ

  1. **配り物 に F# の dll が 0 本**（`Assets/` と、console client の bin）
  2. **ソースが F# の型 を 1 つ も指していない**（`using` と型名。
     **コメントは外す** —— 「元 は Runner.Load を呼んでいた」と
     書いてあるのは、消したことの記録であって参照ではない）
  3. **口（Shared）が F# を参照していない。** ここが 1 本 でも参照したら、
     client 側 に FSharp.Core が戻ってくる

  ## 0 件 を緑にしない

  探す先 が空 のときも赤にする。**当たる先 が消えた門 は、
  違反 0 件 と同じ顔をする。**

  較正は guard-client-has-no-fsharp.Tests.ps1。
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  # 較正で差し替える
  [string]$UnityAssets,
  [string]$SharedProj,
  [string]$SharedPkg,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = ($RepoRoot -replace '/', '\').TrimEnd('\')

if (-not $UnityAssets) {
  $UnityAssets = Join-Path $RepoRoot 'samples\FsBulletML2.Sample.Unity2D.MagicOnion\Assets'
}
if (-not $SharedPkg) {
  $SharedPkg = Join-Path $RepoRoot 'samples\FsBulletML2.Sample.Shared.MagicOnion'
}
if (-not $SharedProj) {
  $SharedProj = Join-Path $RepoRoot 'samples\FsBulletML2.Sample.Shared.MagicOnion.Compile\FsBulletML2.Sample.Shared.MagicOnion.Compile.csproj'
}

$problems = [System.Collections.Generic.List[string]]::new()

# F# の持ち物 の見分け方。**FSharp.Core が居ることがいちばんの印**
$fsharpDll = '^(FSharp\.Core|FsBulletML2\.(Core|Front|Dsl|Parser|Bullets|Bullets\.Dsl|MonoGame|Unity2D))\.dll$'

# --- 1. 配り物 --------------------------------------------------------------
if (-not (Test-Path -LiteralPath $UnityAssets)) {
  $problems.Add("Unity の Assets が無い: $UnityAssets（当たる先 が 0 の門 は緑にしない）")
}
else {
  $dlls = @(Get-ChildItem -LiteralPath $UnityAssets -Recurse -File -Filter *.dll -ErrorAction SilentlyContinue)
  foreach ($d in $dlls) {
    if ($d.Name -match $fsharpDll) {
      $rel = $d.FullName.Substring($RepoRoot.Length + 1)
      $problems.Add("Assets の下 に F# の dll が居る: $rel")
    }
  }

  if (-not $Quiet) { Write-Host ("Assets の dll {0} 本 を見た" -f $dlls.Count) }
}

# --- 1.5 口 の package -------------------------------------------------------
# **Unity が読むのは Assets だけではない。** 口 は UPM package として
# file: で引いていて、**その中にも dll を置いている**（MagicOnion 一式）。
# Assets しか見ない門 は、ここに F# が紛れても緑 のまま
if (-not (Test-Path -LiteralPath $SharedPkg)) {
  $problems.Add("口 の package が無い: $SharedPkg（当たる先 が 0 の門 は緑にしない）")
}
else {
  $pkgDlls = @(Get-ChildItem -LiteralPath $SharedPkg -Recurse -File -Filter *.dll -ErrorAction SilentlyContinue)
  foreach ($d in $pkgDlls) {
    if ($d.Name -match $fsharpDll) {
      $problems.Add("口 の package に F# の dll が居る: " + $d.FullName.Substring($RepoRoot.Length + 1))
    }
  }

  if (-not $Quiet) { Write-Host ("口 の package の dll {0} 本 を見た" -f $pkgDlls.Count) }
}

# --- 1.6 packages.config -----------------------------------------------------
# **NuGetForUnity の宣言も配り物 のうち。** 写した先 に
# 旧 プロジェクトの packages.config が残っていて、そこに
# `FsBulletML2.Core` と `FSharp.Core.3` が宣言されていた（実際に踏んだ）。
# NuGetForUnity が入っていなければ死んでいるが、**入れた瞬間に引かれる**
foreach ($root in @($UnityAssets, $SharedPkg)) {
  if (-not (Test-Path -LiteralPath $root)) { continue }
  foreach ($cfg in @(Get-ChildItem -LiteralPath $root -Recurse -File -Filter 'packages.config' -ErrorAction SilentlyContinue)) {
    foreach ($m in [regex]::Matches((Get-Content -LiteralPath $cfg.FullName -Raw), 'id="([^"]+)"')) {
      if ($m.Groups[1].Value -match '^(FSharp\.Core|FsBulletML2\.(Core|Front|Dsl|Parser|Bullets))') {
        $rel = $cfg.FullName.Substring($RepoRoot.Length + 1)
        $problems.Add("packages.config が F# を宣言している: " + $m.Groups[1].Value + "（" + $rel + "）")
      }
    }
  }
}

# --- 2. ソース --------------------------------------------------------------
$sources = @(Get-ChildItem -LiteralPath $UnityAssets -Recurse -File -Filter *.cs -ErrorAction SilentlyContinue)

if ($sources.Count -eq 0) {
  $problems.Add("Assets の下 に .cs が 1 本 も無い（当たる先 が 0 の門 は緑にしない）")
}

# 引いたら F# が要るもの。**`FsBulletML2.Sample.` は口 なので外す** ——
# あれは C# で、名前が似ているだけ
$fsharpType = @(
  'using\s+FsBulletML2\s*;'
  'using\s+FsBulletML2\.(Core|Front|DTD|Dsl|Bullets)'
  'using\s+Microsoft\.FSharp'
  '\bBulletmlScript\b'
  '\bBulletmlInfo\b'
  '\bBulletRun\b'
  '\bBulletMLManager\b'
  '\bIFrontEnv\b'
  '\bRunner\.(Load|StepWith|NewRoot|NewShot)\b'
  '\bDriver\.(Step|Restart)\b'
  '\bFSharpFunc\b'
) -join '|'

$hits = 0
foreach ($f in $sources) {
  $lines = [IO.File]::ReadAllLines($f.FullName)
  for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]

    # **コメントを外す。** 「元 は Runner.Load を呼んでいた」は、
    # 消したことの記録であって参照ではない。外さないと、
    # **消した経緯を書いた面 ほど赤くなる**
    $code = $line -replace '//.*$', '' -replace '^\s*\*.*$', ''
    if ($code.Trim().Length -eq 0) { continue }

    if ($code -match $fsharpType) {
      $rel = $f.FullName.Substring($RepoRoot.Length + 1)
      $problems.Add("client のソースが F# を指している: ${rel}:$($i + 1): $($line.Trim())")
      $hits++
    }
  }
}

if (-not $Quiet) { Write-Host ("client の .cs {0} 本 を見た" -f $sources.Count) }

# --- 3. 口（Shared）---------------------------------------------------------
if (-not (Test-Path -LiteralPath $SharedProj)) {
  $problems.Add("口 の csproj が無い: $SharedProj（当たる先 が 0 の門 は緑にしない）")
}
else {
  $proj = Get-Content -LiteralPath $SharedProj -Raw
  $refs = [regex]::Matches($proj, '<(ProjectReference|PackageReference|Reference)\s+Include="([^"]+)"')
  if ($refs.Count -eq 0) {
    $problems.Add("口 の csproj から参照を 1 つ も引けなかった（$SharedProj）")
  }

  foreach ($m in $refs) {
    $inc = $m.Groups[2].Value
    if ($inc -match 'FSharp\.Core|FsBulletML2\.(Core|Front|Dsl|Parser|Bullets)') {
      $problems.Add("口 が F# を参照している: $inc（ここが参照すると client に FSharp.Core が戻る）")
    }
  }

  if (-not $Quiet) { Write-Host ("口 の参照 {0} 本 を見た" -f $refs.Count) }
}

# --- 判定 --------------------------------------------------------------------
if ($problems.Count -gt 0) {
  throw ("client が F# を抱えている:`n" +
    (($problems | ForEach-Object { "    $_" }) -join "`n") +
    "`nエンジンはサーバーで走らせる。client が知ってよいのは口 と BulletDto だけ。")
}

if (-not $Quiet) {
  Write-Host ("client に F# は 0 本（.cs {0} 本 / dll 0 本 / 口 の参照に F# 0 本）" -f $sources.Count)
}
