#requires -Version 7
<#
  guard-fable-loops.ps1 の校正。

  **落ちるところを見てから、本番の軸で回す。**
#>
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$guard = Join-Path $PSScriptRoot 'guard-fable-loops.ps1'
$repo = (git rev-parse --show-toplevel)

$ok = 0
$ng = 0
function Case([string]$name, [bool]$wantPass, [scriptblock]$run) {
  $out = & $run 2>&1
  $passed = ($LASTEXITCODE -eq 0)
  if ($passed -eq $wantPass) {
    $script:ok++
    Write-Host ("  ok   {0}  -> {1}" -f $name, $(if ($wantPass) { '通る' } else { '落ちる' }))
  } else {
    $script:ng++
    Write-Host ("  NG   {0}  期待 {1} 実際 {2}" -f $name, $(if ($wantPass) { '通る' } else { '落ちる' }), $(if ($passed) { '通る' } else { '落ちる' }))
    $out | ForEach-Object { Write-Host "         $_" }
  }
}

$tmp = Join-Path ([IO.Path]::GetTempPath()) ('fl-' + [guid]::NewGuid().ToString('N'))
$js = Join-Path $tmp 'js'
New-Item -ItemType Directory -Path $js -Force | Out-Null
$enc = New-Object System.Text.UTF8Encoding($false)

$clean = @'
export function walk(src) {
    const out = [];
    let at = 0;
    while (at < src.length) {
        at = ((at + 1) | 0);
    }
    return out;
}
'@
[IO.File]::WriteAllText((Join-Path $js 'Clean.js'), $clean, $enc)

Write-Host '素の while だけなら通る'
Case '素の while' $true { & $guard -RepoRoot $repo -JsRoot $js -Quiet }

Write-Host ''
Write-Host '内包の中の while。**これが在ると、答えは合っているのに焼いた JS だけ遅い**'
$dirty = @'
export function walk(src) {
    let at = 0;
    return toList(delay(() => append(enumerateWhile(() => (at < src.length), delay(() => {
        at = ((at + 1) | 0);
        return empty();
    })), delay(() => singleton(at)))));
}
'@
[IO.File]::WriteAllText((Join-Path $js 'Dirty.js'), $dirty, $enc)
Case '内包の中の while' $false { & $guard -RepoRoot $repo -JsRoot $js -Quiet }

Write-Host ''
Write-Host '但し書きの中の綴りは数えない（この門の理由は焼いた JS のコメントにも出る）'
Remove-Item -LiteralPath (Join-Path $js 'Dirty.js') -Force
$commented = @'
/**
 * **`[ for … do … while … ]` の中の `while` を、Fable は
 * enumerateWhile( の鎖に焼く** —— 1 字 進むごとに物が 3 つ 増える。
 */
export function walk(src) {
    let at = 0;
    // enumerateWhile( を使わない形に直した
    while (at < src.length) { at = at + 1 }
    return at;
}
'@
[IO.File]::WriteAllText((Join-Path $js 'Commented.js'), $commented, $enc)
Case '但し書きの中の綴り' $true { & $guard -RepoRoot $repo -JsRoot $js -Quiet }

Write-Host ''
Write-Host 'Fable が配る現物は見ない（こちらが直す先ではない）'
$fm = Join-Path $js 'fable_modules/fable-library-js.5.16.0'
New-Item -ItemType Directory -Path $fm -Force | Out-Null
[IO.File]::WriteAllText((Join-Path $fm 'Seq.js'), $dirty, $enc)
Case 'fable_modules の中' $true { & $guard -RepoRoot $repo -JsRoot $js -Quiet }

Write-Host ''
Write-Host '材料が読めないとき。**0 件 は違反 0 件 と同じ顔をする**'
$empty = Join-Path $tmp 'empty'
New-Item -ItemType Directory -Path $empty -Force | Out-Null
Case 'JS が 1 つ も無い' $false { & $guard -RepoRoot $repo -JsRoot $empty -Quiet }
Case '置き場が無い' $false { & $guard -RepoRoot $repo -JsRoot (Join-Path $tmp 'nothere') -Quiet }

Write-Host ''
Write-Host '本番の軸で走らせる。**校正が緑でも、現物に当たらないなら意味が無い**'
Case 'repo の焼いた JS' $true { & $guard -RepoRoot $repo -Quiet }

Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ''
Write-Host "校正 $ok 点 / 外れ $ng 点"
if ($ng -gt 0) { exit 1 }
exit 0
