#requires -Version 7
<#
.SYNOPSIS
  guard-client-has-no-fsharp.ps1 の較正。

.DESCRIPTION
  **門 は「書いた」でなく「落ちるところを見た」で初めて働く。**
  ここは、わざと壊した写しを作って、門 がそれぞれ赤くなることを見る。

  見るのは 14 の壊し方と、壊しても緑 のままであるべき 2 つ。

      素 のまま                              緑
      Assets に FSharp.Core.dll を 1 本       赤   <- 在来サンプルの姿
      Assets に FsBulletML2.Core.dll を       赤
      .cs が using FsBulletML2; を持つ         赤
      .cs が Runner.Load を呼ぶ                赤
      .cs の**コメント**に Runner.Load         緑   <- 消した経緯 は参照ではない
      口 が F# の csproj を参照                赤
      Assets が空（.cs 0 本）                  赤   <- 0 件 を緑にしない
      Assets そのものが無い                    赤
      **口 の package** に FSharp.Core.dll     赤   <- Unity は Assets だけを読むのではない
      **packages.config** が F# を宣言         赤   <- 写した先 に旧 repo の残骸 が在った
      口 の package そのものが無い             赤

  **絵 が出る client は 2 本 在る**（Unity / Godot）。片方 だけ 見ていると、
  もう片方 に F# が戻っても緑 のまま —— どちら も
  **dll を消し忘れても絵 は出てしまう。**

      Godot の .cs が using FsBulletML2       赤
      Godot に FSharp.Core.dll                赤
      Godot の csproj が F# を ProjectReference 赤  <- 素 の .NET なので 1 行 で入る
      Godot そのものが無い                     赤
      **`.godot/` の中** に FSharp.Core.dll    緑  <- インポート の中間物。配り物 ではない

  **写しの上 で壊す。** 本物を書き換えて戻す形にすると、
  途中で落ちたときに壊れたまま残る。
#>
[CmdletBinding()]
param([switch]$Quiet)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$here = Split-Path -Parent $PSCommandPath
$guard = Join-Path $here 'guard-client-has-no-fsharp.ps1'
$root = (git rev-parse --show-toplevel)

$srcAssets = Join-Path $root 'samples/FsBulletML2.Sample.Unity2D.MagicOnion/Assets'
$srcShared = Join-Path $root 'samples/FsBulletML2.Sample.Shared.MagicOnion.Compile/FsBulletML2.Sample.Shared.MagicOnion.Compile.csproj'
$srcGodot = Join-Path $root 'samples/FsBulletML2.Sample.Godot.MagicOnion'

$tmp = Join-Path ([IO.Path]::GetTempPath()) ("client-fsharp-cal-" + [Guid]::NewGuid().ToString('N'))

function New-Sandbox {
  $dir = Join-Path $tmp ([Guid]::NewGuid().ToString('N'))
  $assets = Join-Path $dir 'Assets'
  New-Item -ItemType Directory -Path $assets -Force | Out-Null
  # **ソースだけ写す。** dll は元 に 0 本 なので、写しても 0 本
  Copy-Item -LiteralPath (Join-Path $srcAssets 'Scripts') -Destination $assets -Recurse
  $shared = Join-Path $dir 'shared.csproj'
  Copy-Item -LiteralPath $srcShared -Destination $shared
  # 口 の package。**dll を置く場所**（MagicOnion 一式 が居るところ）
  $pkg = Join-Path $dir 'pkg'
  New-Item -ItemType Directory -Path (Join-Path $pkg 'Plugins') -Force | Out-Null
  Set-Content -LiteralPath (Join-Path $pkg 'package.json') -Value '{}' -NoNewline
  # Godot の client。**ソース と csproj だけ 写す** —— `.godot/` と `Sprites/` は
  # 見ていないので要らない（dll は元 に 0 本 なので、写しても 0 本）
  $godot = Join-Path $dir 'godot'
  New-Item -ItemType Directory -Path $godot -Force | Out-Null
  Copy-Item -LiteralPath (Join-Path $srcGodot 'Scripts') -Destination $godot -Recurse
  Copy-Item -LiteralPath (Join-Path $srcGodot 'FsBulletML2.Sample.Godot.MagicOnion.csproj') -Destination $godot
  [pscustomobject]@{ Assets = $assets; Shared = $shared; Pkg = $pkg; Godot = $godot }
}

function Run($assets, $shared, $pkg, $godot) {
  try {
    & $guard -RepoRoot $root -UnityAssets $assets -SharedProj $shared -SharedPkg $pkg -GodotProj $godot -Quiet
    return $null
  }
  catch {
    return $_.Exception.Message
  }
}

$failures = [System.Collections.Generic.List[string]]::new()
# **`$got` に型を付けない。** `[string]` にすると `$null` が空文字へ潰れて、
# 緑（null）が赤に見える
function Expect([string]$name, $got, [bool]$wantRed) {
  $red = $null -ne $got
  if ($red -eq $wantRed) {
    if (-not $Quiet) { Write-Host ("  OK   {0} -> {1}" -f $name, $(if ($red) { '赤' } else { '緑' })) }
  }
  else {
    $want = if ($wantRed) { '赤' } else { '緑' }
    $act = if ($red) { '赤' } else { '緑' }
    $failures.Add(("{0}: 期待 {1} / 実際 {2}" -f $name, $want, $act))
  }
}

# 素 のまま
$s = New-Sandbox
Expect '素 のまま' (Run $s.Assets $s.Shared $s.Pkg $s.Godot) $false

# **在来サンプルの姿。** FSharp.Core を 1 本 置いたら赤
$s = New-Sandbox
New-Item -ItemType Directory -Path (Join-Path $s.Assets 'FsBulletML2') -Force | Out-Null
Set-Content -LiteralPath (Join-Path $s.Assets 'FsBulletML2\FSharp.Core.dll') -Value 'MZ' -NoNewline
Expect 'FSharp.Core.dll を 1 本 置いた' (Run $s.Assets $s.Shared $s.Pkg $s.Godot) $true

$s = New-Sandbox
New-Item -ItemType Directory -Path (Join-Path $s.Assets 'FsBulletML2') -Force | Out-Null
Set-Content -LiteralPath (Join-Path $s.Assets 'FsBulletML2\FsBulletML2.Core.dll') -Value 'MZ' -NoNewline
Expect 'FsBulletML2.Core.dll を 1 本 置いた' (Run $s.Assets $s.Shared $s.Pkg $s.Godot) $true

# ソースが F# を指す
$s = New-Sandbox
Add-Content -LiteralPath (Join-Path $s.Assets 'Scripts\ECS\BulletSim.cs') -Value 'using FsBulletML2;'
Expect 'ソースが using FsBulletML2 を持つ' (Run $s.Assets $s.Shared $s.Pkg $s.Godot) $true

$s = New-Sandbox
Add-Content -LiteralPath (Join-Path $s.Assets 'Scripts\ECS\BulletSim.cs') -Value 'class X { void M() { Runner.Load(null, 0f, null); } }'
Expect 'ソースが Runner.Load を呼ぶ' (Run $s.Assets $s.Shared $s.Pkg $s.Godot) $true

# **コメントは参照ではない。** ここが赤くなると、消した経緯 を書いた面 ほど赤くなる
$s = New-Sandbox
Add-Content -LiteralPath (Join-Path $s.Assets 'Scripts\ECS\BulletSim.cs') -Value '// 元 は Runner.Load を呼んで BulletmlScript を持っていた'
Expect 'コメントに Runner.Load / BulletmlScript' (Run $s.Assets $s.Shared $s.Pkg $s.Godot) $false

# 口 が F# を参照する
$s = New-Sandbox
$proj = Get-Content -LiteralPath $s.Shared -Raw
$proj = $proj.Replace('</Project>', "  <ItemGroup>`r`n    <ProjectReference Include=`"..\..\src\FsBulletML2.Core\FsBulletML2.Core.fsproj`" />`r`n  </ItemGroup>`r`n</Project>")
[IO.File]::WriteAllText($s.Shared, $proj, (New-Object Text.UTF8Encoding($false)))
Expect '口 が F# を参照した' (Run $s.Assets $s.Shared $s.Pkg $s.Godot) $true

# **0 件 を緑にしない**
$s = New-Sandbox
Remove-Item -LiteralPath (Join-Path $s.Assets 'Scripts') -Recurse -Force
Expect 'Assets の .cs が 0 本' (Run $s.Assets $s.Shared $s.Pkg $s.Godot) $true

$s = New-Sandbox
Expect 'Assets そのものが無い' (Run (Join-Path $s.Assets 'nai-mono') $s.Shared $s.Pkg $s.Godot) $true

# **口 の package に置いた dll も配り物。** Assets しか見ない門 は、ここが緑 のまま
$s = New-Sandbox
Set-Content -LiteralPath (Join-Path $s.Pkg 'Plugins\FSharp.Core.dll') -Value 'MZ' -NoNewline
Expect '口 の package に FSharp.Core.dll' (Run $s.Assets $s.Shared $s.Pkg $s.Godot) $true

# **packages.config の宣言も配り物。** 写した先 に旧 プロジェクトの残骸 が在った
$s = New-Sandbox
Set-Content -LiteralPath (Join-Path $s.Assets 'packages.config') `
  -Value '<packages><package id="FsBulletML2.Core" version="0.8.5" /></packages>' -NoNewline
Expect 'packages.config が FsBulletML2.Core を宣言' (Run $s.Assets $s.Shared $s.Pkg $s.Godot) $true

$s = New-Sandbox
Expect '口 の package そのものが無い' (Run $s.Assets $s.Shared (Join-Path $s.Pkg 'nai-mono') $s.Godot) $true

# --- Godot の client ---------------------------------------------------------
# **絵 が出る client は 2 本 在る。** Unity だけ を見ていると、
# Godot に F# が戻っても緑 のまま —— どちら も「dll を消し忘れても絵 は出る」

$s = New-Sandbox
Add-Content -LiteralPath (Join-Path $s.Godot 'Scripts\Player.cs') -Value 'using FsBulletML2;'
Expect 'Godot のソースが using FsBulletML2 を持つ' (Run $s.Assets $s.Shared $s.Pkg $s.Godot) $true

$s = New-Sandbox
Set-Content -LiteralPath (Join-Path $s.Godot 'FSharp.Core.dll') -Value 'MZ' -NoNewline
Expect 'Godot に FSharp.Core.dll' (Run $s.Assets $s.Shared $s.Pkg $s.Godot) $true

# **Godot は素 の .NET なので、`ProjectReference` を 1 行 足すだけ で F# が入る**
# （Unity より簡単 に壊れる）
$s = New-Sandbox
$gproj = Join-Path $s.Godot 'FsBulletML2.Sample.Godot.MagicOnion.csproj'
$text = Get-Content -LiteralPath $gproj -Raw
$text = $text.Replace('</Project>', "  <ItemGroup>`r`n    <ProjectReference Include=`"..\..\src\FsBulletML2.Core\FsBulletML2.Core.fsproj`" />`r`n  </ItemGroup>`r`n</Project>")
[IO.File]::WriteAllText($gproj, $text, (New-Object Text.UTF8Encoding($false)))
Expect 'Godot の csproj が F# を参照した' (Run $s.Assets $s.Shared $s.Pkg $s.Godot) $true

$s = New-Sandbox
Expect 'Godot そのものが無い' (Run $s.Assets $s.Shared $s.Pkg (Join-Path $s.Godot 'nai-mono')) $true

# **`.godot/` の中 は見ない。** あれ はインポート の中間物 で配り物 ではない ——
# ここ が赤 になると、**1 度 でも build した人 の手元 でだけ 落ちる門**になる
$s = New-Sandbox
New-Item -ItemType Directory -Path (Join-Path $s.Godot '.godot\mono\temp\bin\Debug') -Force | Out-Null
Set-Content -LiteralPath (Join-Path $s.Godot '.godot\mono\temp\bin\Debug\FSharp.Core.dll') -Value 'MZ' -NoNewline
Expect '.godot の中 の FSharp.Core.dll' (Run $s.Assets $s.Shared $s.Pkg $s.Godot) $false

# **後始末は全部 の較正の後。** 1 度 手前 に置いていて、後 から足した 3 つ の
# 写しが temp に残り続けた
Remove-Item -LiteralPath $tmp -Recurse -Force

if ($failures.Count -gt 0) {
  throw ("門 の較正が合わない:`n" + (($failures | ForEach-Object { "    $_" }) -join "`n"))
}

if (-not $Quiet) { Write-Host '17 の形（素 1 / 壊し 14 / 壊さない 2）で、門 が期待どおり緑赤になった' }
