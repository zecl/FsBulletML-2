#requires -Version 7
<#
.SYNOPSIS
  guard-run-trace.ps1 の較正。

.DESCRIPTION
  この門が守っているのは 4 つ ——

      既定        NodeTrace の口が `obj -> unit = ignore`（OFF が素）
      呼び先      通った 2（command / actionElm）/ 止まった 3
      compile の順 Trace.fs が Step.fs より前
      段の約束    受け口を繋いだ場所が 0 件

  **いちばん当てたいのは呼び先の数。** 口が減っても走行は止まらないので、
  目でも試験でも出ない。実際に測っているとき 1 度 踏んだ ——
  `actionElm` の側を落とすと、上位の `action` が **1 個 も記録されない**のに
  弾は同じように飛ぶ（`action だけ` の中央値が 0 で出た）。

  **通る側 も当てる。** 落ちるほうだけ見ていると「常に赤」の門が緑に見える。

  最後に repo の現物へ当てる。**較正が緑でも、本番の軸で走らないなら意味が無い。**
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$here = Split-Path -Parent $PSCommandPath
$guard = Join-Path $here 'guard-run-trace.ps1'
$root = (git -C $here rev-parse --show-toplevel)

$fails = 0
$count = 0

$goodTrace = @'
namespace FsBulletML2

module NodeTrace =
  /// そのコマに通ったノード
  let mutable visit : obj -> unit = ignore
  /// そのコマに止まったノード
  let mutable stop : obj -> unit = ignore
'@

$goodStep = @'
namespace FsBulletML2

module internal Step =

  // コメントの中の NodeTrace.visit (box script) は数えない
  /// 但し書きの中の NodeTrace.stop (box script) も数えない
  let rec command (rs: Resolvers) (script: Action) (p: Progress) (fc: FireContext)
      : Sim<RunState * Progress * FireContext> =
    NodeTrace.visit (box script)
    match script with
    | Action.Wait s -> sim { return Ended, p, fc }

  and actionElm (rs: Resolvers) (script: ActionElm) (p: Progress) (fc: FireContext)
      : Sim<RunState * Progress * FireContext> =
    NodeTrace.visit (box script)
    match script with
    | ActionElm.Action (attrs, children) -> action rs attrs children p fc

  and action (rs: Resolvers) (attrs: ActionAttrs) (children: Action list) =
    sim {
      let! r, p', fc' = command rs childScript cur curFc
      if r = Stopped then NodeTrace.stop (box childScript)
      return r, p', fc'
    }

  and repeat (rs: Resolvers) (Times t) (body: ActionElm) =
    let (r, child', fc'), st', w = Sim.run env st (actionElm rs body child curFc)
    if r = Stopped then NodeTrace.stop (box body)
    ()

  let step (rs: Resolvers) (env: Env) (self: BulletState) : StepResult =
    let (r, prog', fc'), st', w = Sim.run env st (actionElm rs script prog sharedFc)
    if r = Stopped then NodeTrace.stop (box script)
    ()
'@

$goodProj = @'
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <Compile Include="DTD.fs" />
    <Compile Include="Trace.fs" />
    <Compile Include="Domain.fs" />
    <Compile Include="Step.fs" />
  </ItemGroup>
</Project>
'@

function Check {
  param(
    [string]$Name, [string]$Trace, [string]$Step, [string]$Proj,
    [bool]$WantPass, [string]$Expect, [string]$Consumer
  )
  $script:count++
  $tmp = Join-Path ([IO.Path]::GetTempPath()) ("guard-run-trace-" + [guid]::NewGuid().ToString('N'))
  New-Item -ItemType Directory -Path $tmp | Out-Null
  $tp = Join-Path $tmp 'Trace.fs'
  $sp = Join-Path $tmp 'Step.fs'
  $pp = Join-Path $tmp 'Core.fsproj'
  [IO.File]::WriteAllText($tp, $Trace)
  [IO.File]::WriteAllText($sp, $Step)
  [IO.File]::WriteAllText($pp, $Proj)
  $consumerRoot = Join-Path $tmp 'consumer'
  New-Item -ItemType Directory -Path $consumerRoot | Out-Null
  if ($Consumer) { [IO.File]::WriteAllText((Join-Path $consumerRoot 'Front.fs'), $Consumer) }

  $msg = ''
  $passed = $true
  try { & $guard -TraceFs $tp -StepFs $sp -CoreProj $pp -ConsumerRoots @($consumerRoot) -Quiet }
  catch { $passed = $false; $msg = "$_" }
  Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue

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

Write-Host '揃っていれば通る。**ここが赤いと、以下 の落ちは「常に赤」の可能性が在る**'
Check '受け口・呼び先・順・繋いでいない' $goodTrace $goodStep $goodProj $true '' ''

Write-Host ''
Write-Host '既定。**ignore でなくなると、OFF が素であることの根拠が消える**'
Check 'visit の既定が ignore でない' ($goodTrace -replace 'let mutable visit : obj -> unit = ignore',
  'let mutable visit : obj -> unit = fun o -> printfn "%A" o') $goodStep $goodProj $false 'ignore'
Check 'stop の口ごと無い' ($goodTrace -replace '(?m)^\s*let mutable stop.*$', '') $goodStep $goodProj $false 'NodeTrace.stop'

Write-Host ''
Write-Host '呼び先の数。**減っても走行は止まらないので、ここでしか出ない**'
Check 'actionElm の visit を落とす' $goodTrace `
  ($goodStep -replace "(?m)^    NodeTrace\.visit \(box script\)\r?\n(?=    match script with\r?\n    \| ActionElm)", '') `
  $goodProj $false 'NodeTrace.visit が 1 件'
Check 'repeat の stop を落とす' $goodTrace `
  ($goodStep -replace '(?m)^\s*if r = Stopped then NodeTrace\.stop \(box body\)\r?\n', '') `
  $goodProj $false 'NodeTrace.stop が 2 件'
Check 'stop を 1 本 増やす' $goodTrace `
  ($goodStep + "`r`n    if r = Stopped then NodeTrace.stop (box extra)`r`n") `
  $goodProj $false 'NodeTrace.stop が 4 件'

Write-Host ''
Write-Host 'コメントは数えない。**但し書きの中の呼び名を数えると、増やすだけで緑になる**'
Check 'コメントに呼び名を足しても数は変わらない' $goodTrace `
  ($goodStep -replace '(?m)^  // コメントの中の.*$', '  // NodeTrace.visit (box a) NodeTrace.stop (box b) NodeTrace.stop (box c)') `
  $goodProj $true '' ''

Write-Host ''
Write-Host 'compile の順。**後ろに在ると Step から呼べない**'
$afterProj = $goodProj -replace '<Compile Include="Trace.fs" />\r?\n\s*', ''
$stepLine = '<Compile Include="Step.fs" />'
$traceLine = '<Compile Include="Trace.fs" />'
$afterProj = $afterProj.Replace($stepLine, $stepLine + "`r`n    " + $traceLine)
Check 'Trace.fs が Step.fs より後ろ' $goodTrace $goodStep $afterProj $false 'より後ろ'
Check 'Trace.fs が fsproj に無い' $goodTrace $goodStep `
  ($goodProj -replace '\s*<Compile Include="Trace.fs" />', '') $false 'fsproj に無い'

Write-Host ''
Write-Host '段の約束。**繋いだことが黙って入らないように数える**'
Check '受け口を繋いでいる場所が在る' $goodTrace $goodStep $goodProj $false '繋いだ場所が在る' `
  "module Front =`r`n  let wire () = NodeTrace.visit <- (fun o -> ())`r`n"

Write-Host ''
Write-Host '材料が読めないとき。**0 件 は違反 0 件 と同じ顔をする**'
$script:count++
$missing = Join-Path ([IO.Path]::GetTempPath()) 'no-such-trace.fs'
try { & $guard -TraceFs $missing -Quiet; $script:fails++; Write-Host '  NG   ファイルが無い  期待 落ちる 実際 通る' }
catch { Write-Host '  ok   ファイルが無い  -> 落ちる' }

Write-Host ''
Write-Host '本番の軸で走らせる。**較正が緑でも、現物に当たらないなら意味が無い**'
$script:count++
try {
  & $guard -RepoRoot $root -Quiet
  Write-Host '  ok   repo の現物  -> 通る'
} catch {
  $script:fails++
  Write-Host "  NG   repo の現物  -> 落ちた: $($_ -replace "`n", ' / ')"
}

Write-Host ''
Write-Host "較正 $count 件 / 食い違い $fails 件"
if ($fails -gt 0) { exit 1 }
