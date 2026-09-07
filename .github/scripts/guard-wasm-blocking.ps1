#requires -Version 7
<#
.SYNOPSIS
  ブラウザ（wasm）へ載る側に、待って止まる書き方が無いかを見る。

.DESCRIPTION
  **build も、.NET の試験も、これでは落ちない。**

  Blazor WASM はスレッドが 1 本 しか無い。`Async.RunSynchronously` や
  `.GetAwaiter().GetResult()` で待つと、**続きを流す相手がそのスレッドで
  塞がれている** —— 押した瞬間に画面ごと止まって、二度と返ってこない。

  v1.0 で FCS の `ParseFile` をこれで待って踏んだ。そのとき

      dotnet test        530 本 緑
      Playground の build 警告 0
      ブラウザで Apply    renderer が返らない

  で、**落ちたのはいちばん最後だけ。** 待たない書き方は
  `LanguageService.Host/FsharpCe.fs` の `runHere`。

  ## 当てる先は、入口の proj が引いているものだけ

  `ProjectReference` を辿って集める。**門の中に proj の表を持たない** ——
  参照が増えたときに、ここを直し忘れても網が伸びる。

  Fable が焼く側（`fable/` の下は別の proj）は閉包に入らない。
  あちらは JS になるので、この形の待ち方がそもそも書けない。

  ## コメントと文字列を外してから当てる

  外さないと、**この形を説明している但し書きがそのまま違反として出る**
  （`FsharpCe.fs` の doc コメントが `Async.RunSynchronously` と書いている）。

  文字列も外す —— `"http://…"` の `//` を行コメントと読むと、
  **その行の後ろに在る違反を見落とす。** 見落としは赤にならないので、
  外し方の順は「文字列 -> コメント」で固定する。

  ## 0 件 を緑にしない

  proj が辿れない・`.fs` が拾えない状態は、「違反 0 件」と同じ顔をする。
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  # 閉包の入口。ここから `ProjectReference` を辿る
  [string]$EntryProject,
  # 拾えた `.fs` の下限。**下回ったら落とす**（拾えていない状態は 0 件 と同じ顔）
  [int]$MinFiles = 20,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) {
  $RepoRoot = (git -C (Split-Path -Parent $PSCommandPath) rev-parse --show-toplevel)
}
$RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
if (-not $EntryProject) {
  $EntryProject = Join-Path $RepoRoot 'src/FsBulletML2.Playground/FsBulletML2.Playground.fsproj'
}
if (-not (Test-Path -LiteralPath $EntryProject)) {
  throw "入口の proj が無い（$EntryProject）。**違反 0 件 と同じ顔をする**ので、ここで落とす"
}

$bad = [System.Collections.Generic.List[string]]::new()

# --- ProjectReference の閉包 -------------------------------------------------
$closure = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$queue = [System.Collections.Generic.Queue[string]]::new()
$queue.Enqueue((Resolve-Path -LiteralPath $EntryProject).Path)
while ($queue.Count -gt 0) {
  $proj = $queue.Dequeue()
  if (-not $closure.Add($proj)) { continue }
  $projDir = Split-Path -Parent $proj
  $xml = [xml][IO.File]::ReadAllText($proj)
  foreach ($node in $xml.SelectNodes('//ProjectReference')) {
    $include = $node.GetAttribute('Include')
    if ([string]::IsNullOrWhiteSpace($include)) { continue }
    $rel = $include -replace '[\\/]', [IO.Path]::DirectorySeparatorChar
    $full = [IO.Path]::GetFullPath((Join-Path $projDir $rel))
    if (Test-Path -LiteralPath $full) { $queue.Enqueue($full) }
    else {
      # **辿れない参照は赤。** 黙って閉包を狭めると、その先が丸ごと網から外れる
      $bad.Add("  辿れない ProjectReference: $include`n      引いた元: $proj")
    }
  }
}

# **閉包が欠けた状態で先へ進まない。** 辿れなかった先は丸ごと網から外れるので、
# そのまま数えると「違反 0 件」と同じ顔で通る
if ($bad.Count -gt 0) {
  throw ("ブラウザへ載る proj を辿りきれなかった（$($bad.Count) 本）:`n" + ($bad -join "`n"))
}

# --- どの proj が持つ `.fs` かを、いちばん近い fsproj で決める ---------------
$ownerOf = {
  param([string]$path)
  $dir = Split-Path -Parent $path
  while ($dir -and $dir.StartsWith($RepoRoot, [StringComparison]::OrdinalIgnoreCase)) {
    $here = @(Get-ChildItem -LiteralPath $dir -Filter *.fsproj -File -ErrorAction SilentlyContinue)
    if ($here.Count -gt 0) { return $here[0].FullName }
    $dir = Split-Path -Parent $dir
  }
  return $null
}

$files = [System.Collections.Generic.List[string]]::new()
foreach ($proj in $closure) {
  $projDir = Split-Path -Parent $proj
  Get-ChildItem -LiteralPath $projDir -Recurse -Filter *.fs -File -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
    ForEach-Object {
      # **入れ子の proj は数えない。** `Playground/fable/` は別の proj で、
      # 閉包に入っていない（Fable が焼くので、この形の待ち方が書けない）
      $owner = & $ownerOf $_.FullName
      if ($owner -and $closure.Contains($owner)) { $files.Add($_.FullName) }
    }
}
$files = @($files | Sort-Object -Unique)

if ($files.Count -lt $MinFiles) {
  throw ("ブラウザへ載る `.fs` が $($files.Count) 本 しか拾えなかった（下限 $MinFiles）。" +
         "**違反 0 件 と同じ顔をする**ので、ここで落とす")
}

# --- 当てる形 ---------------------------------------------------------------
#
# どれも「そのスレッドで待つ」書き方。**wasm には待つ相手が居ない**
$patterns = @(
  @{ Rx = 'Async\.RunSynchronously'; Why = '続きを流す相手が、待っているそのスレッドで塞がれている' }
  @{ Rx = '\.GetAwaiter\s*\(\s*\)'; Why = 'Task を同期で受け取る形。上と同じ' }
  @{ Rx = '\.Wait\s*\(\s*\)'; Why = 'Task を待つ形。上と同じ' }
  @{ Rx = 'Task\.Wait(All|Any)'; Why = 'Task を待つ形。上と同じ' }
  @{ Rx = '\.Result\b'; Why = 'Task の結果を同期で取る形。上と同じ' }
  @{ Rx = 'Thread\.Sleep'; Why = 'wasm では画面ごと止まる（進める相手が同じスレッド）' }
)

foreach ($file in $files) {
  $text = [IO.File]::ReadAllText($file)
  # **順は 文字列 -> コメント。** 逆にすると `"http://…"` の中で行コメントが
  # 始まったことになり、その行の後ろが読まれない
  $blank = { param($m) $m.Value -replace '[^\r\n]', '' }
  $text = [regex]::Replace($text, '"""[\s\S]*?"""', $blank)
  $text = [regex]::Replace($text, '@"(?:[^"]|"")*"', $blank)
  $text = [regex]::Replace($text, '"(?:\\.|[^"\\\r\n])*"', $blank)
  $text = [regex]::Replace($text, '\(\*[\s\S]*?\*\)', $blank)
  $text = [regex]::Replace($text, '//[^\r\n]*', $blank)

  $lines = $text -split "`r?`n"
  for ($i = 0; $i -lt $lines.Count; $i++) {
    foreach ($p in $patterns) {
      if ($lines[$i] -cmatch $p.Rx) {
        $rel = $file.Substring($RepoRoot.Length).TrimStart('\', '/')
        $bad.Add("  $($rel):$($i + 1)  $($lines[$i].Trim())`n      $($p.Why)")
      }
    }
  }
}

if (-not $Quiet) {
  Write-Host ("ブラウザへ載る側を見た: proj {0} 個 / .fs {1} 本" -f $closure.Count, $files.Count)
}

if ($bad.Count -gt 0) {
  throw ("ブラウザ（wasm）で止まる書き方が在る（$($bad.Count) 箇所）:`n" + ($bad -join "`n") +
         "`n  待たない形は LanguageService.Host/FsharpCe.fs の runHere")
}

if (-not $Quiet) { Write-Host 'ブラウザへ載る側に、待って止まる書き方は無い' }
