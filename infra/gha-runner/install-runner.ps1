#requires -Version 7
<#
.SYNOPSIS
  この repo 用 の self-hosted runner を 1 台 建てる（Windows）。

.DESCRIPTION
  なぜ Windows なのかと、キャッシュや labels の契約は README.md が正本。
  ここに書くのは手順だけ。

  **何度 走らせても同じ形になる。** 既に登録されている runner が在れば
  `--replace` で置き換える。落とすときは `-Remove`。

  登録の token は既定で `gh` から取る（1 時間 で失効する短期のもの）。
  **PAT を置かない** —— 置くと、この機械の中に repo の管理権が残る。

  起こし方は 2 通り。**強さと、要る権限が違う。**

      既定          サービスとして登録する。サインインしていなくても走る。
                    **管理者が要る**
      -NoService    Startup に cmd を 1 枚 置いて、サインインのたびに起こす。
                    管理者は要らない。**サインアウト中は走らない**

  タスク スケジューラを使わないのは、既定で管理者が要るため。

.PARAMETER RunnerDir
  runner を展開する場所。既定は `%LOCALAPPDATA%\gha-runner\FsBulletML-2`。
  **絵に描いた drive letter を書かない**（この script は追跡されるので、
  guard-abs-paths.ps1 が絶対パスを落とす）。

.PARAMETER Token
  登録 token。省かれたら `gh api` で取る。

.PARAMETER Remove
  登録を外して、サービスと展開先を消す。

.EXAMPLE
  # サービスとして建てる（管理者で開いた pwsh から）
  ./infra/gha-runner/install-runner.ps1

.EXAMPLE
  # 管理者を使わずに建てる（Startup 経由）
  ./infra/gha-runner/install-runner.ps1 -NoService

.EXAMPLE
  # 落とす
  ./infra/gha-runner/install-runner.ps1 -Remove
#>
[CmdletBinding()]
param(
  [string]$RunnerDir,
  [string]$Token,
  # **label はここで固定する。** workflow の runs-on と対で、
  # 片方 を変えたら job が永遠に queued で待つ
  [string]$Label = 'fsbulletml2',
  [string]$Repo = 'zecl/FsBulletML-2',
  [string]$Version = '2.335.1',
  # 何台目 か。**既定 の 1 は 今まで と同じ 置き場・名前・Startup の字**。
  # 2 台目 を足しても 1 台目 は 1 文字 も動かない。
  #
  # 名前 を instance ごと に分けない と `--replace` が 1 台目 を置き換える ——
  # 2 台 建てた つもり で 1 台 のまま に なり、**GitHub 側 は online 1 台 で緑 に見える**
  [ValidateRange(1, 9)]
  [int]$Instance = 1,
  # **管理者を使わない道。** サービスにせず、Startup に置いた 1 枚 の cmd で
  # サインインのたびに起こす。runner は今この場でも立ち上げる。
  #
  # サービスのほうが強い（サインインしていなくても走る）が、admin が要る ——
  # 「CI を今日 緑にする」を admin 待ちで止めないための逃げ道
  [switch]$NoService,
  [switch]$Remove
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# 1 台目 は 尻尾 を付けない。既に建って いる ものの 置き場・名前・Startup の字 を動かさない
$suffix = if ($Instance -le 1) { '' } else { "-$Instance" }

if (-not $RunnerDir) {
  if (-not $env:LOCALAPPDATA) { throw 'LOCALAPPDATA が無い。-RunnerDir で置き場を渡すこと' }
  $RunnerDir = Join-Path $env:LOCALAPPDATA "gha-runner\FsBulletML-2$suffix"
}

# **区切り を付けて から 前方一致 する。** 付けない と `FsBulletML-2` が
# `FsBulletML-2-2\bin\...` に当たり、1 台目 を落とす つもり で 2 台目 も死ぬ
$dirPrefix = $RunnerDir.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar

function Get-MyListeners {
  Get-Process -Name 'Runner.Listener' -ErrorAction SilentlyContinue |
    Where-Object { $_.Path -and $_.Path.StartsWith($dirPrefix, [StringComparison]::OrdinalIgnoreCase) }
}

# runner の名前。**機械の名前を入れる** —— 何台か 並んだとき、どれがどこか分かる
$name = "$Label-$($env:COMPUTERNAME.ToLower())$suffix"

function Test-Admin {
  $me = [Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())
  $me.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)
}

function Assert-Admin {
  if (-not (Test-Admin)) {
    throw 'サービスとして登録するには管理者が要る。管理者で開いた pwsh から走らせるか、-NoService を付けること'
  }
}

# Startup に置く起動役。**cmd 1 枚 だけ** —— サインインのたびに runner を起こす
function Get-StartupCmdPath {
  Join-Path ([Environment]::GetFolderPath('Startup')) "gha-runner-FsBulletML-2$suffix.cmd"
}

function Get-RegistrationToken {
  if ($Token) { return $Token }
  if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw 'gh が無い。-Token に登録 token を渡すこと（GitHub の Settings -> Actions -> Runners から取る）'
  }
  # **repo scope の短期 token。** 1 時間 で失効する
  $t = (gh api -X POST "repos/$Repo/actions/runners/registration-token" --jq '.token')
  if ($LASTEXITCODE -ne 0 -or -not $t) { throw '登録 token を取れなかった（gh の権限を確認する）' }
  $t
}

# --- 落とす -------------------------------------------------------------------
if ($Remove) {
  # **走っているものを先に止める。** 掴まれたまま消すと、GitHub 側 に
  # offline の残骸が残って一覧が汚れる。
  #
  # **この instance の ぶん だけ 止める。** `actions.runner.*` と
  # `Runner.Listener` を名前 で全部 拾う と、2 台目 を落とす つもり で
  # **1 台目 も道連れ に なる**（そして GitHub 側 は offline 2 台 に見える）
  $svc = @(Get-Service -ErrorAction SilentlyContinue | Where-Object { $_.Name -like "actions.runner.*.$name" })
  if ($svc.Count -gt 0) {
    if (-not (Test-Admin)) { throw 'サービスが在るので、外すには管理者が要る' }
    $svc | Stop-Service -Force -ErrorAction SilentlyContinue
  }
  Get-MyListeners | Stop-Process -Force
  $startup = Get-StartupCmdPath
  if (Test-Path -LiteralPath $startup) {
    [IO.File]::Delete($startup)
    Write-Host "Startup から外した: $startup"
  }

  $cfg = Join-Path $RunnerDir 'config.cmd'
  if (Test-Path -LiteralPath $cfg) {
    Write-Host '登録を外す'
    $rt = Get-RegistrationToken
    # **`--token` は登録用 でも remove に使える**（remove-token でなくてよい）
    & $cfg remove --unattended --token $rt
    if ($LASTEXITCODE -ne 0) { Write-Host '  登録を外せなかった。GitHub 側 で手で消すこと' }
  } else {
    Write-Host "展開先が無い（$RunnerDir）。GitHub 側 の登録だけ残っていないか見ること"
  }
  if (Test-Path -LiteralPath $RunnerDir) {
    Remove-Item -LiteralPath $RunnerDir -Recurse -Force
    Write-Host "消した: $RunnerDir"
  }
  return
}

# --- 建てる -------------------------------------------------------------------
if (-not $NoService) { Assert-Admin }

Write-Host "repo      $Repo"
Write-Host "label     self-hosted, windows, $Label"
Write-Host "名前      $name（$Instance 台目）"
Write-Host "置き場    $RunnerDir"
Write-Host "版        $Version"
Write-Host ("起こし方  {0}" -f $(if ($NoService) { 'Startup の cmd（サインインのたび）' } else { 'サービス（サインイン前から）' }))
Write-Host ''

New-Item -ItemType Directory -Force -Path $RunnerDir | Out-Null

$zipName = "actions-runner-win-x64-$Version.zip"
$zip = Join-Path $RunnerDir $zipName

if (Test-Path -LiteralPath (Join-Path $RunnerDir 'config.cmd')) {
  Write-Host '既に展開されている。取り直さない'
} else {
  $url = "https://github.com/actions/runner/releases/download/v$Version/$zipName"
  Write-Host "取ってくる: $url"
  Invoke-WebRequest -Uri $url -OutFile $zip
  # **落とした物の指紋を出す。** GitHub の release notes の値と見比べられるように
  Write-Host ("SHA256    {0}" -f (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash)
  Expand-Archive -LiteralPath $zip -DestinationPath $RunnerDir -Force
  Remove-Item -LiteralPath $zip -Force
}

$rt = Get-RegistrationToken

Push-Location $RunnerDir
try {
  # `--replace` で、同じ名前の古い登録を置き換える（何度 走らせても同じ形になる）
  $cfgArgs = @(
    '--unattended', '--replace',
    '--url', "https://github.com/$Repo",
    '--token', $rt,
    '--name', $name,
    '--labels', $Label,
    '--work', '_work'
  )
  if (-not $NoService) { $cfgArgs += '--runasservice' }
  & ./config.cmd @cfgArgs
  if ($LASTEXITCODE -ne 0) { throw "config.cmd が落ちた（exit $LASTEXITCODE）" }
} finally {
  Pop-Location
}

if ($NoService) {
  # **Startup に 1 枚 置く。** タスク スケジューラは既定で管理者が要るので使わない
  $startup = Get-StartupCmdPath
  $cmd = @(
    '@echo off',
    "cd /d `"$RunnerDir`"",
    'start "" /min run.cmd'
  ) -join "`r`n"
  [IO.File]::WriteAllText($startup, $cmd + "`r`n", (New-Object Text.UTF8Encoding $false))
  Write-Host "Startup に置いた: $startup"

  # いまこの場でも起こす。**サインインを待たない**。
  # 止めるのは この置き場 の ぶん だけ —— 名前 で拾う と 隣 の instance が死ぬ
  Get-MyListeners | Stop-Process -Force
  Start-Process -FilePath (Join-Path $RunnerDir 'run.cmd') -WorkingDirectory $RunnerDir -WindowStyle Hidden
  Start-Sleep -Seconds 8
}

Write-Host ''
Write-Host '--- 確かめる'
$svc = @(Get-Service -ErrorAction SilentlyContinue | Where-Object { $_.Name -like 'actions.runner.*' })
foreach ($s in $svc) { "  サービス  {0}  {1}" -f $s.Status, $s.Name }
$proc = @(Get-Process -Name 'Runner.Listener' -ErrorAction SilentlyContinue)
foreach ($p in $proc) { "  プロセス  pid {0}" -f $p.Id }
if ($svc.Count -eq 0 -and $proc.Count -eq 0) { Write-Host '  ** 走っていない **' }

if (Get-Command gh -ErrorAction SilentlyContinue) {
  Write-Host '  GitHub 側:'
  gh api "repos/$Repo/actions/runners" --jq '.runners[] | "    \(.name) \(.status) busy=\(.busy) labels=\([.labels[].name] | join(","))"'
}

Write-Host ''
Write-Host "online になっていれば、workflow の runs-on: [self-hosted, windows, $Label] が拾う。"
Write-Host 'offline のまま job を投げると queued で待ち続ける（README の「queued のまま進まないとき」）。'
