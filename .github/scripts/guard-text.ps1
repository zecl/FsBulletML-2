#requires -Version 7
<#
.SYNOPSIS
  追跡されているテキストに、制御文字が生で混ざっていないかを見る。

.DESCRIPTION
  許すのは TAB / LF / CR だけ。それ以外の C0 と DEL が 1 バイト でもあれば落とす。

  入口は 2 つ ある。

    PowerShell の二重引用符でバッククォート ＋ 英字が化ける（`a → BEL）
    制御文字について書くときに、その文字を生で置く（`'[^\x00-\x7F]'` のつもりで
    NUL と DEL のバイトを打つ）

  **どちらも走りはする。** 出るのは git の側で、NUL が 1 個 入るとそのファイルは
  binary 扱いになって numstat が `-` になり、差分が読めなくなる。
  読み返しても字が繋がって見えるだけなので、目では出ない。

  除くのは $BinaryExt だけ。**含める側でなく除く側を並べる** —— 新しい種類の
  テキストが増えたとき、含める並びを足し忘れると黙って対象外になるので。
  逆に新しい binary が増えたときは赤くなるが、そちらは目に見える。

  見つけた文字は**字でそのまま出さない**。名前とコードで出す（出力のほうが壊れる）。
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  [string[]]$Files,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = $RepoRoot -replace '\\', '/'

$BinaryExt = @('.xnb', '.png', '.dll', '.wav', '.mp3', '.psd')

$C0 = @(
  'NUL', 'SOH', 'STX', 'ETX', 'EOT', 'ENQ', 'ACK', 'BEL',
  'BS',  'TAB', 'LF',  'VT',  'FF',  'CR',  'SO',  'SI',
  'DLE', 'DC1', 'DC2', 'DC3', 'DC4', 'NAK', 'SYN', 'ETB',
  'CAN', 'EM',  'SUB', 'ESC', 'FS',  'GS',  'RS',  'US')

function CharName([byte]$b) {
  $n = if ($b -eq 127) { 'DEL' } else { $C0[$b] }
  '{0}（U+{1:X4}）' -f $n, $b
}

if (-not $PSBoundParameters.ContainsKey('Files')) {
  # 非 ASCII のパスが化けると開けなくなるので quotepath を切る
  $Files = @(git -C $RepoRoot -c core.quotepath=false ls-files |
             ForEach-Object { Join-Path $RepoRoot $_ })
}

$scanned = 0
$skipped = 0
$bad = [System.Collections.Generic.List[string]]::new()

foreach ($f in $Files) {
  if ($BinaryExt -contains [IO.Path]::GetExtension($f).ToLower()) { $skipped++; continue }
  if (-not (Test-Path -LiteralPath $f)) { continue }
  $scanned++

  $b = [IO.File]::ReadAllBytes($f)
  $line = 1
  $col = 1
  $found = [System.Collections.Generic.List[string]]::new()
  for ($i = 0; $i -lt $b.Length; $i++) {
    $x = $b[$i]
    if ($x -eq 10) { $line++; $col = 1; continue }
    if (($x -lt 32 -and $x -ne 9 -and $x -ne 13) -or $x -eq 127) {
      $found.Add(("{0} 行 {1} 桁 {2}" -f $line, $col, (CharName $x)))
    }
    $col++
  }
  if ($found.Count -gt 0) {
    $rel = ($f -replace '\\', '/').Replace("$RepoRoot/", '')
    $bad.Add(("  {0}`n      {1}" -f $rel, ($found -join "`n      ")))
  }
}

if (-not $Quiet) {
  Write-Host "走査 $scanned 件 / binary として除いた $skipped 件"
}

if ($bad.Count -gt 0) {
  $msg = "制御文字が生で入っているファイルが $($bad.Count) 件:`n" + ($bad -join "`n") +
         "`n`n名前で書くこと（`\x00` のようなエスケープ）。字で置くと git が binary と見て差分が読めなくなる"
  throw $msg
}

if (-not $Quiet) { Write-Host '生の制御文字は無い' }
