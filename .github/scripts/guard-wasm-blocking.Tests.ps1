#requires -Version 7
<#
.SYNOPSIS
  guard-wasm-blocking.ps1 の較正。

.DESCRIPTION
  **通る側と落ちる側**を両方 当てる。落ちる側だけ見ていると「常に赤」の
  壊れ方が、通る側だけ見ていると「何も見ていない」壊れ方が、それぞれ
  緑のまま残る。

  材料は temp に建てた小さな proj の組。**入口 -> 引いている proj -> 引いて
  いない proj** の 3 つ を並べて、網が閉包の中だけに掛かることを見る。

  最後に**実物の repo にも当てる** —— 合成の木だけで較正すると、
  当てる先が実際には 0 本 でも全部 通る。
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$here = Split-Path -Parent $PSCommandPath
$guard = Join-Path $here 'guard-wasm-blocking.ps1'
$root = (git -C $here rev-parse --show-toplevel)
$tmp = Join-Path ([IO.Path]::GetTempPath()) ("wasm-blocking-" + [Guid]::NewGuid().ToString('N'))

$fails = 0
$count = 0

function Check {
  param([string]$Name, [hashtable]$GuardArgs, [bool]$WantPass, [string]$Expect)
  $script:count++
  $msg = ''
  $passed = $true
  try { & $guard @GuardArgs -Quiet } catch { $passed = $false; $msg = "$_" }

  if ($passed -ne $WantPass) {
    $script:fails++
    Write-Host ("  NG   {0}  期待 {1} 実際 {2}" -f $Name,
      $(if ($WantPass) { '通る' } else { '落ちる' }), $(if ($passed) { '通る' } else { '落ちる' }))
    if ($msg) { Write-Host "         $($msg -replace "`n", ' / ')" }
    return
  }
  if ($Expect -and $msg -notmatch [regex]::Escape($Expect)) {
    $script:fails++
    Write-Host "  NG   $Name  「$Expect」が出ていない"
    Write-Host "         $($msg -replace "`n", ' / ')"
    return
  }
  Write-Host ("  ok   {0}  -> {1}" -f $Name, $(if ($passed) { '通る' } else { '落ちる' }))
}

# 合成の木を建て直す。`$LibBody` が変異の入口
function Build {
  param([string]$LibBody, [string]$OtherBody = 'let ok = 1', [string]$LibRef = '..\lib\lib.fsproj')
  if (Test-Path -LiteralPath $tmp) { Remove-Item -LiteralPath $tmp -Recurse -Force }
  foreach ($d in 'app', 'lib', 'other') {
    New-Item -ItemType Directory -Path (Join-Path $tmp $d) -Force | Out-Null
  }
  Set-Content -LiteralPath (Join-Path $tmp 'app\app.fsproj') -Encoding utf8 -Value @"
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup><ProjectReference Include="$LibRef" /></ItemGroup>
</Project>
"@
  Set-Content -LiteralPath (Join-Path $tmp 'lib\lib.fsproj') -Encoding utf8 -Value '<Project Sdk="Microsoft.NET.Sdk"></Project>'
  Set-Content -LiteralPath (Join-Path $tmp 'other\other.fsproj') -Encoding utf8 -Value '<Project Sdk="Microsoft.NET.Sdk"></Project>'
  Set-Content -LiteralPath (Join-Path $tmp 'app\App.fs') -Encoding utf8 -Value 'module App
let start () = 1'
  Set-Content -LiteralPath (Join-Path $tmp 'lib\Lib.fs') -Encoding utf8 -Value "module Lib`n$LibBody"
  Set-Content -LiteralPath (Join-Path $tmp 'other\Other.fs') -Encoding utf8 -Value "module Other`n$OtherBody"
  return @{ RepoRoot = $tmp; EntryProject = (Join-Path $tmp 'app\app.fsproj'); MinFiles = 2 }
}

try {
  Write-Host '=== guard-wasm-blocking.ps1 の較正'

  # --- 通る側 ---------------------------------------------------------------
  Check '素の木' (Build 'let f () = 1') $true

  # **コメントの中は数えない。** 数えると、この形を説明した但し書きで赤になる
  Check 'doc コメントの中の名前' (Build '/// Async.RunSynchronously は使わない
let f () = 1') $true

  # 閉包の外は当てない。当てると「ブラウザへ載る側」という線が意味を失う
  Check '引いていない proj の中' (Build 'let f () = 1' 'let g () = async { return 1 } |> Async.RunSynchronously') $true

  # --- 落ちる側 -------------------------------------------------------------
  Check '引いている proj の中で待っている' `
    (Build 'let f () = async { return 1 } |> Async.RunSynchronously') $false 'Async.RunSynchronously'

  Check 'GetAwaiter で受け取っている' `
    (Build 'let f (t: System.Threading.Tasks.Task<int>) = t.GetAwaiter().GetResult()') $false 'GetAwaiter'

  Check 'Task の Result を取っている' `
    (Build 'let f (t: System.Threading.Tasks.Task<int>) = t.Result') $false '.Result'

  # **文字列を先に外している証拠。** 外す順が逆だと `"http://…"` の `//` から
  # 行コメントが始まったことになり、同じ行の後ろに在るこれを見落とす
  Check '文字列の // の後ろに在る' `
    (Build 'let f (t: System.Threading.Tasks.Task<int>) = "http://a" + string t.Result') $false '.Result'

  Check 'Thread.Sleep が在る' (Build 'let f () = System.Threading.Thread.Sleep 1') $false 'Thread.Sleep'

  # --- 0 件 を緑にしない -----------------------------------------------------
  $empty = Build 'let f () = 1'
  Remove-Item -LiteralPath (Join-Path $tmp 'app\App.fs'), (Join-Path $tmp 'lib\Lib.fs') -Force
  Check '.fs が拾えない' $empty $false '同じ顔をする'

  Check '辿れない ProjectReference' `
    (Build 'let f () = 1' 'let ok = 1' '..\nowhere\nowhere.fsproj') $false '辿れない ProjectReference'

  # --- 実物 -----------------------------------------------------------------
  #
  # **当てる先が在ることを、ここで見る。** 合成の木だけだと、実物に 1 本 も
  # 掛かっていなくても全部 通る
  Check '実物の repo' @{ RepoRoot = $root } $true

  Write-Host ''
  if ($fails -gt 0) { throw "較正が $fails / $count 点 落ちた" }
  Write-Host "較正 $count 点 すべて期待どおり"
} finally {
  if (Test-Path -LiteralPath $tmp) { Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue }
}
