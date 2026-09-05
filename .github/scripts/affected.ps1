#requires -Version 7
<#
.SYNOPSIS
  変更されたファイルから、走らせる試験プロジェクトを決める。

.DESCRIPTION
  決め方は 3 段。

    1. 変更ファイルを、それを含むいちばん近いプロジェクトに割り当てる
    2. 参照の逆向きに閉じる（Core を変えたら Core を参照する側が全部 入る）
    3. 閉じた集合のうち試験プロジェクトが答え

  プロジェクトの一覧と参照は .slnx と各 proj から読む。**手で書いた対応表は
  持たない。** 参照が変わったとき対応表だけが古びるのを避けるため。

  どのプロジェクトにも属さないファイルが 1 つ でもあれば全部 走らせる
  （global.json / *.slnx / tests/TestData / .github / .nuget など）。
  例外は $NoImpact に並べた形だけで、そこに落ちたファイルは無視する。

  **0 件 を黙って緑にしない。** 答えが 0 件 になってよいのは「変更が全部
  NoImpact だった」ときだけで、そうでないのに 0 件 なら道具の誤りとして落ちる。

.PARAMETER Base
  比較の起点。省略時は origin/HEAD との merge-base。

.PARAMETER ChangedFiles
  git を引かずに変更一覧を直接 渡す。校正（affected.Tests.ps1）から使う。
#>
[CmdletBinding()]
param(
  [string]$Base,
  [string]$Head = 'HEAD',
  [string[]]$ChangedFiles,
  [string]$RepoRoot,
  [switch]$ForceFull,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = $RepoRoot -replace '\\', '/'

# 変わっても何も走らせなくてよい形。**ここを広げるほど網が粗くなる**ので、
# 「その形のファイルだけの PR が来たとき、壊れうるものが本当に無いか」を
# 1 つずつ 言えるものだけ置く。
$NoImpact = @(
  '*.md'
  'LICENSE'
  '.gitignore'
  '.gitattributes'
  '.editorconfig'
  '.github/ISSUE_TEMPLATE/*'
  '.github/PULL_REQUEST_TEMPLATE*'
  '.github/CODEOWNERS'
)

# 同じく無視するが、ディレクトリの前置きで書くもの
$NoImpactDirs = @(
  'license/'        # 元実装とサンプル素材のライセンス本文
  'docs/'           # 出力済みのドキュメントとそのテンプレート
  'nuget/'          # パッケージを作る bat と nuspec。build には入らない
)

# slnx に載っていないが repo には在る化石。CI の対象外。ディレクトリの前置きで書く。
# ここに足すときは「なぜ solution に無いのか」を一緒に書く。
$OutOfScope = @(
  # MSBuild 4.0 / FSharp.Formatting 2.2.3 の頃のドキュメント生成。
  # solution から外れて久しく、いまの SDK では復元も通らない。
  'src/FsBulletML2.Docs/'
)

function ToRel([string]$p) {
  $q = $p -replace '\\', '/'
  if ($q.StartsWith('./')) { $q = $q.Substring(2) }
  $q
}

function NormalizePath([string]$p) {
  $out = [System.Collections.Generic.List[string]]::new()
  foreach ($seg in ($p -replace '\\', '/').Split('/')) {
    if ($seg -eq '' -or $seg -eq '.') { continue }
    if ($seg -eq '..') { if ($out.Count -gt 0) { $out.RemoveAt($out.Count - 1) }; continue }
    $out.Add($seg)
  }
  $out -join '/'
}

# --- プロジェクトの一覧は .slnx が正本 -------------------------------------

$slnx = Get-ChildItem -LiteralPath $RepoRoot -Filter '*.slnx' -File
if ($slnx.Count -eq 0) { throw 'slnx が 1 つ も無い。プロジェクトの一覧を引く先が無い' }

$projects = [System.Collections.Generic.HashSet[string]]::new()
foreach ($s in $slnx) {
  $doc = [xml](Get-Content -LiteralPath $s.FullName -Raw)
  foreach ($n in $doc.SelectNodes('//Project')) {
    [void]$projects.Add((NormalizePath $n.GetAttribute('Path')))
  }
}

# --- 参照を読む -------------------------------------------------------------

$deps = @{}      # proj -> 直接 参照しているもの
$isTest = @{}
foreach ($p in $projects) {
  $full = Join-Path $RepoRoot $p
  if (-not (Test-Path -LiteralPath $full)) { throw "slnx が指す proj が無い: $p" }
  $raw = Get-Content -LiteralPath $full -Raw
  $dir = Split-Path $p -Parent
  $d = [System.Collections.Generic.List[string]]::new()
  foreach ($m in [regex]::Matches($raw, 'ProjectReference\s+Include\s*=\s*"([^"]+)"')) {
    $d.Add((NormalizePath (Join-Path $dir $m.Groups[1].Value)))
  }
  $deps[$p] = $d
  # 名前でなく「試験ホストを持っているか」で判定する。tests/ の外に置いても効く
  $isTest[$p] = $raw -match 'Microsoft\.NET\.Test\.Sdk'
}

# 参照の逆向き
$rdeps = @{}
foreach ($p in $projects) { $rdeps[$p] = [System.Collections.Generic.List[string]]::new() }
foreach ($p in $projects) {
  foreach ($d in $deps[$p]) {
    if ($rdeps.ContainsKey($d)) { $rdeps[$d].Add($p) }
    else { throw "$p が slnx の外を参照している: $d" }
  }
}

# --- 変更ファイル -----------------------------------------------------------

# 空配列を渡された場合と、渡されなかった場合を分ける。**空配列は「変更 0 件」
# という答えであって、git を引き直せという指示ではない。**
if (-not $PSBoundParameters.ContainsKey('ChangedFiles')) {
  if (-not $Base) { $Base = (git merge-base origin/HEAD $Head) }
  # **core.quotepath=false が要る。** 既定だと非 ASCII のパスが
  # `"samples/.../\343\202\257.xml"` に化けて、どのプロジェクトの前置きにも
  # 当たらなくなる。弾幕の XML は日本語名なので、ここが効く
  $ChangedFiles = @(git -c core.quotepath=false diff --name-only "$Base" "$Head")
}
$changed = @($ChangedFiles | Where-Object { $_ } | ForEach-Object { ToRel $_ })

# --- 割り当て ---------------------------------------------------------------

# Split-Path は Windows で `\` を返す。前置きは `/` で比べるので自前で切る
$projDirs = @{}
foreach ($p in $projects) { $projDirs[$p] = $p.Substring(0, $p.LastIndexOf('/') + 1) }

$seed = [System.Collections.Generic.HashSet[string]]::new()
$ignored = [System.Collections.Generic.List[string]]::new()
$unowned = [System.Collections.Generic.List[string]]::new()

foreach ($f in $changed) {
  # いちばん深いプロジェクトに割り当てる（入れ子の proj があっても内側が勝つ）
  $owner = $null; $best = -1
  foreach ($p in $projects) {
    $d = $projDirs[$p]
    if ($f.StartsWith($d) -and $d.Length -gt $best) { $owner = $p; $best = $d.Length }
  }
  if ($owner) { [void]$seed.Add($owner); continue }
  $out = $false
  foreach ($o in $OutOfScope) { if ($f.StartsWith($o)) { $out = $true; break } }
  if ($out) { $ignored.Add($f); continue }
  $hit = $false
  foreach ($d in $NoImpactDirs) { if ($f.StartsWith($d)) { $hit = $true; break } }
  if (-not $hit) {
    foreach ($g in $NoImpact) { if ($f -like $g -or (Split-Path $f -Leaf) -like $g) { $hit = $true; break } }
  }
  if ($hit) { $ignored.Add($f) } else { $unowned.Add($f) }
}

$full = $ForceFull -or $unowned.Count -gt 0

# --- 閉じる -----------------------------------------------------------------

$affected = [System.Collections.Generic.HashSet[string]]::new()
if ($full) {
  foreach ($p in $projects) { [void]$affected.Add($p) }
} else {
  $queue = [System.Collections.Generic.Queue[string]]::new()
  foreach ($p in $seed) { if ($affected.Add($p)) { $queue.Enqueue($p) } }
  while ($queue.Count -gt 0) {
    $p = $queue.Dequeue()
    foreach ($r in $rdeps[$p]) { if ($affected.Add($r)) { $queue.Enqueue($r) } }
  }
}

$testProjects = @($affected | Where-Object { $isTest[$_] } | Sort-Object)

# build だけするのは、影響のある集合の**根**（その集合の中の誰からも参照されて
# いないもの）のうち試験でないもの。根でないものは誰かが build する途中で通る。
# **試験が 1 本 も触らないサンプルやベンチが壊れるのを、ここで拾う。**
$referenced = [System.Collections.Generic.HashSet[string]]::new()
foreach ($p in $affected) { foreach ($d in $deps[$p]) { if ($affected.Contains($d)) { [void]$referenced.Add($d) } } }
$buildProjects = @($affected | Where-Object { -not $isTest[$_] -and -not $referenced.Contains($_) } | Sort-Object)

# --- 0 件 を黙って通さない --------------------------------------------------

if ($testProjects.Count -eq 0 -and $buildProjects.Count -eq 0) {
  if ($changed.Count -eq 0) {
    throw '変更ファイルが 0 件。比較の起点が間違っている可能性がある'
  }
  if ($ignored.Count -ne $changed.Count) {
    throw ("走らせるものが 0 件 になったが、無視した数 ($($ignored.Count)) が" +
           " 変更の数 ($($changed.Count)) に足りない。割り当ての誤り")
  }
}

# --- 出す -------------------------------------------------------------------

function ShortName([string]$p) { [IO.Path]::GetFileNameWithoutExtension($p) }

$result = [pscustomobject]@{
  Changed = $changed
  Ignored = @($ignored)
  Unowned = @($unowned)
  Full    = $full
  Tests   = $testProjects
  Builds  = $buildProjects
}

if (-not $Quiet) {
  Write-Host "変更 $($changed.Count) 件 / 無視 $($ignored.Count) 件 / 割り当て不明 $($unowned.Count) 件"
  if ($full) {
    Write-Host '  割り当て不明が在るので全部 走らせる:'
    foreach ($f in $unowned) { Write-Host "    $f" }
  }
  Write-Host "試験 $($testProjects.Count) 本:"
  foreach ($t in $testProjects) { Write-Host "    $t" }
  Write-Host "build だけ $($buildProjects.Count) 本:"
  foreach ($b in $buildProjects) { Write-Host "    $b" }
}

# matrix にそのまま渡る形。**要素を 1 つずつ包んでから括る** ——
# 配列ごと ConvertTo-Json に渡すと、版によって 1 要素 の扱いが違う
# （5.1 はオブジェクトに畳み、7 は畳まない）。0 / 1 / 複数 で形が変わらない
# ように、括弧は自分で書く。形の校正は affected.Tests.ps1。
function ToMatrixJson($paths) {
  '[' + (($paths | ForEach-Object {
    ConvertTo-Json -Compress -InputObject ([ordered]@{ name = (ShortName $_); path = $_ })
  }) -join ',') + ']'
}

if ($env:GITHUB_OUTPUT) {
  Add-Content -LiteralPath $env:GITHUB_OUTPUT -Value "tests=$(ToMatrixJson $testProjects)"
  Add-Content -LiteralPath $env:GITHUB_OUTPUT -Value "builds=$(ToMatrixJson $buildProjects)"
  Add-Content -LiteralPath $env:GITHUB_OUTPUT -Value "full=$($full.ToString().ToLower())"
}

# 選んだ理由を走行の中に残す。**次の人が「なぜこの 1 本 だけなのか」を
# ログを掘らずに読めるように。**
if ($env:GITHUB_STEP_SUMMARY) {
  $s = [System.Collections.Generic.List[string]]::new()
  $s.Add('## 走らせるもの')
  $s.Add('')
  if ($full) {
    $reason = if ($ForceFull) { '手で全部 指定された' } else { "どのプロジェクトにも属さない変更が $($unowned.Count) 件 在る" }
    $s.Add("**全部 走らせる** —— $reason")
    if (-not $ForceFull) {
      $s.Add('')
      foreach ($f in $unowned) { $s.Add("- ``$f``") }
    }
  } else {
    $s.Add("変更 $($changed.Count) 件 のうち $($ignored.Count) 件 は影響なしとして無視。")
  }
  $s.Add('')
  $s.Add("| | |")
  $s.Add("|---|---|")
  $s.Add("| 試験 | $(if ($testProjects.Count) { ($testProjects | ForEach-Object { '`' + (ShortName $_) + '`' }) -join ' ' } else { 'なし' }) |")
  $s.Add("| build のみ | $(if ($buildProjects.Count) { ($buildProjects | ForEach-Object { '`' + (ShortName $_) + '`' }) -join ' ' } else { 'なし' }) |")
  Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value ($s -join "`n")
}

$result
