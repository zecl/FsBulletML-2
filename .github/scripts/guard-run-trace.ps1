#requires -Version 7
<#
.SYNOPSIS
  走行の記録の受け口（`NodeTrace`）が、**OFF が素に戻る形**で置かれているかを見る。

.DESCRIPTION
  v2.9 で 4 通り の形を測って、この形に決めた
  （「線を越える」の測定）——

      Env に口を足す        OFF が素に戻らない（5way で +1.92%）
      int を走査で運ぶ      費用が inline の判断に乗るので表にできない
      module の可変な口     OFF が素に戻る。**これ**

  判定は「増分が素の 5% 以内 **かつ** OFF が素に戻る」で、**後半をここで見る。**

  ## 何を見て、何を見ていないか

      見る    既定が ignore（何も繋がなければ何もしない）
      見る    呼び先の数（通った 2 / 止まった 3）。**減ると黙って記録が欠ける**
      見る    Trace.fs が Domain.fs より前に compile される（依存の向き）
      見る    約束 —— **4 つ の口を繋ぐのは Focus.fs の 1 か所 だけ**

      見ない  確保が素に戻るか。**そこは走らせないと出ない**
              （`dotnet run --project bench/FsBulletML2.Benchmarks -- --alloc` を
                素と並べて 1% の線の内側か。README の v3.1 の節に記録が在る）

  ## 呼び先の数を数える理由

  **口が減っても走行は止まらない。** `actionElm` の側を落とすと、
  上位の `action` は `command` を通らないので **1 個 も記録されない** ——
  それでも弾は同じように飛ぶ。実際に測っているとき 1 度 踏んだ
  （`action だけ` の中央値が 0 で出た）。

  ## 「繋ぐ場所」を数える理由

  段 1 と段 2 の約束は「**受け口を開けるだけ。繋ぐのは次の段**」だった。
  段 3 でそこを直した —— いまの約束は「**繋ぐのは Focus.fs の 1 か所 だけ**」。

  繋いだ瞬間に確保と時間の予算（README の v2.9 の結論）が効き始めるので、
  **どこが繋いでいるかを数える。** 数えるのは 2 方向 で、片方 しか見ないと
  黙って壊れる ——

      増える  ほかの場所が繋ぐと、選んでいない人も費用を払う
      減る    Focus.fs が繋がなくなると、**印が出なくなるだけで走行は変わらない**
              （目にも試験にも出ない。**0 件 を緑にしない**）

  **`visit` も Focus.fs だけ。** v3.2（弾から字へ）で繋いだ ——
  撃った `fire` を拾うのに要る。**押す前に撃たれている**ので、
  段 3 のように「選んだ弾の 1 コマ だけ」では足りない。

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
  # 受け口を繋ぐ 1 か所。**面（Playground）は Danmaku Lab へ出たので、
  # この repo には無い** —— 既定は空で、渡されたときだけ 4 つ 目 を見る。
  # Lab からは `vendor/FsBulletML2/.github/scripts/guard-run-trace.ps1
  # -FocusFs client/Focus.fs` と呼ぶ（**門の写しを 2 か所 に置かない**）。
  # 渡されなかったことは走行の頭に出す —— 黙って飛ばすと 3 項目 が 4 項目 に見える
  [string]$FocusFs = '',
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
# Fantomas は `then` のあとで改行する。意味は「enabled の中でしか歩かない」。
if ($api -notmatch 'if NodeOrigin\.enabled then\s+FoldOrigin\.walk') {
  $problems.Add('Api.fs の Runner.load が "if NodeOrigin.enabled then FoldOrigin.walk" の形で無い')
}
# --- 約束 —— 繋ぐのは Focus.fs の 1 か所 だけ ---------------------------------
# 4 つ の口を全部 あそこで繋ぐ —— ほかが繋げば費用が増え、
# あそこが繋がなくなれば**印が消えるだけで走行は変わらない**
$hooked = [System.Collections.Generic.List[string]]::new()
$inFocus = @{ 'NodeTrace.visit' = 0; 'NodeTrace.stop' = 0; 'NodeOrigin.pair' = 0; 'NodeOrigin.enabled' = 0 }
$focusSeen = $false
$skipFocus = [string]::IsNullOrWhiteSpace($FocusFs)
if ($skipFocus -and -not $Quiet) {
  Write-Host '受け口を繋ぐ側は見ない（-FocusFs が無い）。**見たのは 3 項目**'
}
# **繋ぐ側が別の repo に在ることが在る。** 面（Playground）は Danmaku Lab へ出た。
# `-FocusFs` に**絶対パス**を渡すと、この repo の走査とは別に、その 1 本 を読む ——
#
#     vendor/FsBulletML2/.github/scripts/guard-run-trace.ps1 `
#       -RepoRoot vendor/FsBulletML2 -FocusFs "$PWD/client/Focus.fs"
#
# **そのとき「ほかの場所が繋いでいないか」は、この repo の中でしか言えない。**
# 向こうの `client/` が別の所で繋いでも、こちらの走査には出ない ——
# 網が repo をまたがないことを承知で置いている
$focusAbs = if (-not $skipFocus -and [IO.Path]::IsPathRooted($FocusFs)) { $FocusFs } else { '' }
foreach ($root in $ConsumerRoots) {
  if ($skipFocus) { break }
  if (-not (Test-Path -LiteralPath $root)) { continue }
  Get-ChildItem -LiteralPath $root -Recurse -File -Include *.fs, *.fsx, *.cs |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
    ForEach-Object {
      $rel = $_.FullName.Substring($RepoRoot.Length + 1).Replace('\', '/')
      $isFocus = ($rel -eq $FocusFs)
      if ($isFocus) { $focusSeen = $true }
      $t = [IO.File]::ReadAllText($_.FullName)
      foreach ($port in @('NodeTrace.visit', 'NodeTrace.stop', 'NodeOrigin.pair', 'NodeOrigin.enabled')) {
        $c = ([regex]::Matches($t, ($port -replace '\.', '\.') + '\s*<-')).Count
        if ($c -gt 0) {
          if ($isFocus) { $inFocus[$port] += $c }
          else { $hooked.Add("$rel が $port を繋いでいる（繋ぐのは $FocusFs だけ）") }
        }
      }
    }
}
if ($focusAbs -and (Test-Path -LiteralPath $focusAbs)) {
  $focusSeen = $true
  $t = [IO.File]::ReadAllText($focusAbs)
  foreach ($port in @('NodeTrace.visit', 'NodeTrace.stop', 'NodeOrigin.pair', 'NodeOrigin.enabled')) {
    $inFocus[$port] += ([regex]::Matches($t, ($port -replace '\.', '\.') + '\s*<-')).Count
  }
}
if (-not $skipFocus -and -not $focusSeen) {
  $problems.Add("$FocusFs が読めなかった。**0 件 は違反 0 件 と同じ顔をする**ので、ここで落とす")
}
# **繋ぐ側と素へ戻す側を、別々に数える。**
#
# 合わせて数えると、**繋ぐのをやめても「戻す」1 件 で緑のまま**になる ——
# 較正で 3 件 踏んだ。素へ戻す形は `Trace.fs` の既定と同じ形で書く
# （`ignore` / `false` / その場で作らない `noPair`）ので、
# **戻す側を字で決められる。残りが繋ぐ側。**
$restore = @{
  'NodeTrace.visit'    = 'NodeTrace\.visit\s*<-\s*ignore'
  'NodeTrace.stop'     = 'NodeTrace\.stop\s*<-\s*ignore'
  'NodeOrigin.enabled' = 'NodeOrigin\.enabled\s*<-\s*false'
  'NodeOrigin.pair'    = 'NodeOrigin\.pair\s*<-\s*noPair'
}
$focusPath =
  if ($skipFocus) { '' }
  elseif ($focusAbs) { $focusAbs }
  else { Join-Path $RepoRoot $FocusFs }
$focusText = if (-not $skipFocus -and (Test-Path -LiteralPath $focusPath)) { [IO.File]::ReadAllText($focusPath) } else { '' }
foreach ($port in @('NodeTrace.visit', 'NodeTrace.stop', 'NodeOrigin.pair', 'NodeOrigin.enabled')) {
  if ($skipFocus) { break }
  $off = ([regex]::Matches($focusText, $restore[$port])).Count
  $on = $inFocus[$port] - $off
  if (-not $Quiet) { Write-Host "$FocusFs の $port  繋ぐ $on 件 / 戻す $off 件" }
  if ($on -lt 1) {
    $problems.Add("$FocusFs が $port を繋いでいない。**印が出なくなるだけで走行は変わらない**ので、目にも試験にも出ない")
  }
  if ($off -lt 1) {
    $problems.Add("$FocusFs に $port を素へ戻す側が無い（$($restore[$port])）。繋ぎっぱなしだと、選んでいない弾も受け口を通る")
  }
}
if (-not $Quiet -and -not $skipFocus) { Write-Host "ほかの場所が繋いでいる件数  $($hooked.Count) 件（0 件 のはず）" }
if ($hooked.Count -gt 0) {
  $problems.Add("繋ぐのは $FocusFs だけ。ほかに繋いだ場所が在る:`n        " + ($hooked -join "`n        "))
}

# --- 判定 --------------------------------------------------------------------
if ($problems.Count -gt 0) {
  throw ("走行の記録の受け口が、OFF が素に戻る形になっていない:`n" +
    (($problems | ForEach-Object { "    $_" }) -join "`n") +
    "`n判定は「増分が素の 5% 以内 かつ OFF が素に戻る」（v2.9 の結論）。" +
    "`n確保そのものは走らせて測る —— bench の --alloc を素と並べて 1% の線の内側か")
}

if (-not $Quiet) {
  Write-Host ("受け口は 2 本 とも既定が ignore、呼び先は 通った 2 / 止まった 3" +
    $(if ($skipFocus) { "（繋ぐ側は見ていない —— **3 項目**）" }
      else { "、繋ぐのは $FocusFs だけ（口は 4 つ）" }))
}
