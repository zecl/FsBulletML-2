#requires -Version 7
<#
.SYNOPSIS
  走行の記録の受け口（`NodeTrace`）が、**OFF が素に戻る形**で置かれているかを見る。

.DESCRIPTION
  v2.9 で 4 通り の形を測って、この形に決めた
  （`src/FsBulletML2.Playground/README.md` の「線を越える」）——

      Env に口を足す        OFF が素に戻らない（5way で +1.92%）
      int を走査で運ぶ      費用が inline の判断に乗るので表にできない
      module の可変な口     OFF が素に戻る。**これ**

  判定は「増分が素の 5% 以内 **かつ** OFF が素に戻る」で、**後半をここで見る。**

  ## 何を見て、何を見ていないか

      見る    既定が ignore（何も繋がなければ何もしない）
      見る    呼び先の数（通った 2 / 止まった 3）。**減ると黙って記録が欠ける**
      見る    Trace.fs が Domain.fs より前に compile される（依存の向き）
      見る    段 1 の約束 —— **消費する側をまだ作っていない**

      見ない  確保が素に戻るか。**そこは走らせないと出ない**
              （`dotnet run --project bench/FsBulletML2.Benchmarks -- --alloc` を
                素と並べて 1% の線の内側か。README の v3.1 の節に記録が在る）

  ## 呼び先の数を数える理由

  **口が減っても走行は止まらない。** `actionElm` の側を落とすと、
  上位の `action` は `command` を通らないので **1 個 も記録されない** ——
  それでも弾は同じように飛ぶ。実際に測っているとき 1 度 踏んだ
  （`action だけ` の中央値が 0 で出た）。

  ## 「消費する側」を数える理由

  段 1 の約束は「**受け口を開けるだけ。繋ぐのは次の段**」。
  繋いだ瞬間に確保と時間の予算（README の v2.9 の結論）が効き始めるので、
  **繋いだことが黙って入らない**ようにここで数える。
  次の段でここを 1 行 直す —— **直すことが段が進んだ印になる。**

  較正は guard-run-trace.Tests.ps1。
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  [string]$TraceFs,
  [string]$StepFs,
  [string]$CoreProj,
  [string]$OpsFs,
  [string]$ApiFs,
  [string[]]$ConsumerRoots,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$core = Join-Path $RepoRoot 'src/FsBulletML2.Core'
if (-not $TraceFs) { $TraceFs = Join-Path $core 'Trace.fs' }
if (-not $StepFs) { $StepFs = Join-Path $core 'Step.fs' }
if (-not $CoreProj) { $CoreProj = Join-Path $core 'FsBulletML2.Core.fsproj' }
if (-not $OpsFs) { $OpsFs = Join-Path $core 'BulletmlOps.fs' }
if (-not $ApiFs) { $ApiFs = Join-Path $core 'Api.fs' }
if (-not $ConsumerRoots) {
  $ConsumerRoots = @(
    (Join-Path $RepoRoot 'src'), (Join-Path $RepoRoot 'samples'), (Join-Path $RepoRoot 'tests')
  )
}

foreach ($f in @($TraceFs, $StepFs, $CoreProj, $OpsFs, $ApiFs)) {
  if (-not (Test-Path -LiteralPath $f)) {
    throw "読めなかった（$f）。**0 件 は違反 0 件 と同じ顔をする**ので、ここで落とす"
  }
}

$problems = [System.Collections.Generic.List[string]]::new()

# --- 既定が ignore か ---------------------------------------------------------
$trace = [IO.File]::ReadAllText($TraceFs)
$ports = @('visit', 'stop')
foreach ($p in $ports) {
  if ($trace -notmatch "let\s+mutable\s+$p\s*:\s*obj\s*->\s*unit\s*=\s*ignore") {
    $problems.Add("NodeTrace.$p が `"obj -> unit = ignore`" の形で無い（OFF が素であることの根拠が消える）")
  }
}
if (-not $Quiet) { Write-Host "受け口 $($ports.Count) 本（既定は ignore）" }

# --- 呼び先の数 ---------------------------------------------------------------
# **コメントを先に落とす。** 但し書きの中の `NodeTrace.visit` を数えない
$step = [regex]::Replace([IO.File]::ReadAllText($StepFs), '(?m)^\s*//.*$', '')
$step = [regex]::Replace($step, '(?s)\(\*.*?\*\)', ' ')
$step = [regex]::Replace($step, '(?m)^\s*///.*$', '')

$want = @{ 'visit' = 2; 'stop' = 3 }
foreach ($p in $ports) {
  $n = ([regex]::Matches($step, "NodeTrace\.$p\s+\(box")).Count
  if (-not $Quiet) { Write-Host "Step.fs の NodeTrace.$p  $n 件（$($want[$p]) 件 のはず）" }
  if ($n -ne $want[$p]) {
    $problems.Add("Step.fs の NodeTrace.$p が $n 件（$($want[$p]) 件 のはず）。減ると黙って記録が欠ける")
  }
}

# 通った側は command と actionElm の両方 に要る。
# **actionElm を落とすと、上位の action が 1 個 も記録されない**
foreach ($site in @('command', 'actionElm')) {
  $rx = "and\s+$site\s+\(rs: Resolvers\)[^=]*=\s*\r?\n\s*NodeTrace\.visit"
  $rxTop = "let\s+rec\s+$site\s+\(rs: Resolvers\)[^=]*=\s*\r?\n\s*NodeTrace\.visit"
  if (($step -notmatch $rx) -and ($step -notmatch $rxTop)) {
    $problems.Add("Step.fs の $site の入口に NodeTrace.visit が無い")
  }
}

# --- compile の順 -------------------------------------------------------------
$proj = [IO.File]::ReadAllText($CoreProj)
$order = [regex]::Matches($proj, 'Compile Include="([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
$iTrace = [array]::IndexOf($order, 'Trace.fs')
$iDomain = [array]::IndexOf($order, 'Domain.fs')
$iStep = [array]::IndexOf($order, 'Step.fs')
if (-not $Quiet) { Write-Host "compile の順  Trace $iTrace / Domain $iDomain / Step $iStep" }
if ($iTrace -lt 0) { $problems.Add('Trace.fs が Core の fsproj に無い（ファイルが在っても焼かれない）') }
elseif ($iTrace -gt $iStep) { $problems.Add('Trace.fs が Step.fs より後ろ（Step から呼べない）') }


# --- NodeOrigin の既定 ---------------------------------------------------------
# **段 2 で足した口。** こちらは「呼ぶだけ」ではなく**木を 1 周 する**ので、
# 繋がないときに歩かないための enabled が別に要る（README の「段 2 の当てる先」）。
if ($trace -notmatch 'let\s+mutable\s+enabled\s*=\s*false') {
  $problems.Add('NodeOrigin.enabled の既定が false でない（OFF で歩かないことの根拠が消える）')
}
if ($trace -notmatch 'let\s+mutable\s+pair\s*:\s*obj\s*->\s*obj\s*->\s*unit\s*=\s*fun\s+_\s+_\s*->\s*\(\)') {
  $problems.Add('NodeOrigin.pair が "obj -> obj -> unit = fun _ _ -> ()" の形で無い')
}

# --- 対を作る呼び先の数 ---------------------------------------------------------
# **新しいノードができる場所は 6 か所。** 減ると、その経路だけ字へ戻れなくなる ——
# 実際に 2 度 踏んだ（getAction の対が無くて 1.00%、FoldOrigin が根の要素を
# 対にしていなくて同じ 1.00%）。**どちらも走行は変わらないので目には出ない。**
$stripComments = {
  param($path)
  $t = [regex]::Replace([IO.File]::ReadAllText($path), '(?m)^\s*//.*$', '')
  $t = [regex]::Replace($t, '(?s)\(\*.*?\*\)', ' ')
  [regex]::Replace($t, '(?m)^\s*///.*$', '')
}
$ops = & $stripComments $OpsFs
$api = & $stripComments $ApiFs
$pairSites = ([regex]::Matches($ops, 'NodeOrigin\.pair')).Count + ([regex]::Matches($api, 'NodeOrigin\.pair')).Count
if (-not $Quiet) { Write-Host "NodeOrigin.pair の呼び先  $pairSites 件（6 件 のはず）" }
if ($pairSites -ne 6) {
  $problems.Add("NodeOrigin.pair の呼び先が $pairSites 件（6 件 のはず）。減るとその経路だけ字へ戻れない")
}

# 覆いは 4 本。**中身（*Core）を直に呼ぶ形へ戻すと、対が黙って消える**
foreach ($n in @('substCommand', 'substActionElm', 'expandCommand', 'expandActionElm')) {
  if ($ops -notmatch "and\s+private\s+$n\s[^=]*=\s*\r?\n\s*let r = ${n}Core") {
    $problems.Add("BulletmlOps.fs の $n が ${n}Core を覆う形になっていない")
  }
}

# 畳みの対は組む段で 1 回。**enabled を見ずに歩くと、繋がない人が毎回 木を 1 周 する**
if ($api -notmatch 'if NodeOrigin\.enabled then FoldOrigin\.walk') {
  $problems.Add('Api.fs の Runner.load が "if NodeOrigin.enabled then FoldOrigin.walk" の形で無い')
}
# --- 段 1 の約束 —— 消費する側をまだ作っていない ------------------------------
$hooked = [System.Collections.Generic.List[string]]::new()
foreach ($root in $ConsumerRoots) {
  if (-not (Test-Path -LiteralPath $root)) { continue }
  Get-ChildItem -LiteralPath $root -Recurse -File -Include *.fs, *.fsx, *.cs |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
    ForEach-Object {
      $t = [IO.File]::ReadAllText($_.FullName)
      foreach ($p in $ports) {
        if ($t -match "NodeTrace\.$p\s*<-") {
          $hooked.Add("$($_.FullName.Substring($RepoRoot.Length + 1)) が NodeTrace.$p を繋いでいる")
        }
      }
      # **段 2 の口も同じ約束。** 繋ぐのは段 3（Front が弾を選ぶとき）
      foreach ($p in @('pair', 'enabled')) {
        if ($t -match "NodeOrigin\.$p\s*<-") {
          $hooked.Add("$($_.FullName.Substring($RepoRoot.Length + 1)) が NodeOrigin.$p を繋いでいる")
        }
      }
    }
}
if (-not $Quiet) { Write-Host "受け口を繋いでいる場所  $($hooked.Count) 件（段 1 では 0 件）" }
if ($hooked.Count -gt 0) {
  $problems.Add("段 1 は受け口を開けるだけ。繋いだ場所が在る:`n        " + ($hooked -join "`n        "))
}

# --- 判定 --------------------------------------------------------------------
if ($problems.Count -gt 0) {
  throw ("走行の記録の受け口が、OFF が素に戻る形になっていない:`n" +
    (($problems | ForEach-Object { "    $_" }) -join "`n") +
    "`n判定は「増分が素の 5% 以内 かつ OFF が素に戻る」（v2.9 の結論）。" +
    "`n確保そのものは走らせて測る —— bench の --alloc を素と並べて 1% の線の内側か")
}

if (-not $Quiet) {
  Write-Host '受け口は 2 本 とも既定が ignore、呼び先は 通った 2 / 止まった 3、繋いだ場所は 0 件'
}
