#requires -Version 7
<#
.SYNOPSIS
  guard-playground-boundaries.ps1 の較正。

.DESCRIPTION
  線を 1 つ ずつ、**通る側と落ちる側**で当てる。
  落ちる側だけ見ていると「常に赤」の壊れ方が、通る側だけ見ていると
  「何も見ていない」壊れ方が、それぞれ緑のまま残る。

  **「材料が読めない」も落ちること**を見る。これがいちばん怖い形 ——
  当てる名前が 0 個、走査するファイルが 0 本 は、違反 0 件 と同じ顔をする。
  実際に一度 やった（`"$prefix`fable"` の `` `f `` が改ページになり、
  Fable のソースを 1 本 も読まずに緑を出した）。

  最後に repo の現物へ当てる。**較正が緑でも、本番の軸で走らないなら意味が無い。**
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$here = Split-Path -Parent $PSCommandPath
$guard = Join-Path $here 'guard-playground-boundaries.ps1'
$root = (git -C $here rev-parse --show-toplevel)
$tmp = Join-Path ([IO.Path]::GetTempPath()) ("pg-bound-" + [Guid]::NewGuid().ToString('N'))

$fails = 0
$count = 0

function Make {
  param([string]$Rel, [string]$Text)
  $p = Join-Path $tmp $Rel
  New-Item -ItemType Directory -Path (Split-Path -Parent $p) -Force | Out-Null
  [IO.File]::WriteAllText($p, $Text)
  $p
}

# 既定に上書きを重ねた新しいハッシュ。**+ は重複キーで落ちる**
function With {
  param([hashtable]$Base, [hashtable]$Over)
  $h = @{}
  foreach ($k in $Base.Keys) { $h[$k] = $Base[$k] }
  foreach ($k in $Over.Keys) { $h[$k] = $Over[$k] }
  $h
}

function Check {
  param([string]$Name, [hashtable]$Opt, [bool]$WantPass, [string]$Expect)
  $script:count++
  $msg = ''
  $passed = $true
  try { & $guard @Opt -Quiet } catch { $passed = $false; $msg = "$_" }

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

try {
  # 差し替え用の材料。既定はどれも「線の内側」
  New-Item -ItemType Directory -Path $tmp -Force | Out-Null
  $dtd = Make 'core/DTD.fs' @'
writer.WriteStartElement("bulletml")
writer.WriteStartElement("action")
writer.WriteStartElement("vanish")
'@
  $proj = Make 'core/Core.fsproj' '<Project><ItemGroup><Compile Include="DTD.fs" /></ItemGroup></Project>'
  # 線の内側の html。**SRI と CSP が在り、中身の在るインライン script は無い**
  $okHead = '<meta http-equiv="Content-Security-Policy" content="default-src ''self''">'
  $okMonaco = '<script src="https://cdn.jsdelivr.net/npm/monaco-editor@0.56.0/min/vs/loader.js" ' +
              'integrity="sha384-AAAA" crossorigin="anonymous"></script>'
  $okBoot = '<script type="module" src="js/Playground.js"></script>'
  $html = Make 'pg/wwwroot/index.html' ($okHead + $okMonaco + $okBoot)
  Make 'pg/fable/Playground.fs' 'let el (id: string) = document.getElementById id' | Out-Null
  $pgProj = Make 'pg/Playground.fsproj' `
    '<Project><PropertyGroup><UseSystemResourceKeys>false</UseSystemResourceKeys></PropertyGroup></Project>'
  # LanguageService の側。**線の内側** —— 表示側を参照せず、Monaco も叩かない。
  # 要素名（`vanish` など）はここに在ってよい：あちらへ移す散文の表は
  # 要素名で引くので、当てる先から外してある
  Make 'svc/FsBulletML2.LanguageService.fsproj' '<Project><ItemGroup /></Project>' | Out-Null
  Make 'svc/SourceKind.fs' 'let ids = [ "xml"; "sxml" ]' | Out-Null
  # `.Host` の側。**線が 1 本 違う** —— 要素名はあちらに在ってよい
  Make 'host/FsBulletML2.LanguageService.Host.fsproj' '<Project><ItemGroup /></Project>' | Out-Null
  Make 'host/Spec.fs' 'let elements = [ "vanish", "自分を消す" ]' | Out-Null

  $ok = @{ RepoRoot = $tmp; PlaygroundDir = 'pg'; CoreProj = $proj; DtdSource = $dtd; IndexHtml = $html
           ServiceDir = (Join-Path $tmp 'svc'); HostDir = (Join-Path $tmp 'host')
           PlaygroundProj = $pgProj; Files = @('pg/fable/Playground.fs') }

  Write-Host '=== 通る側'
  Check '線の内側' $ok $true ''

  Write-Host '=== 落ちる側'

  Check '追跡された .js が在る' (With $ok @{ Files = @('pg/wwwroot/js/glue.js') }) $false '追跡された .js'

  $badProj = Make 'core/Bad.fsproj' '<Project><ItemGroup><PackageReference Include="Bolero" /></ItemGroup></Project>'
  Check 'Core が Bolero を参照している' (With $ok @{ CoreProj = $badProj }) `
    $false 'Bad.fsproj が表示側を参照している'

  # **LanguageService も同じ線の内側。** 器がエディタを参照した瞬間に、
  # そのエディタ以外 へ載せられなくなる
  Make 'svc-bad/FsBulletML2.LanguageService.fsproj' `
    '<Project><ItemGroup><PackageReference Include="Bolero" /></ItemGroup></Project>' | Out-Null
  Make 'svc-bad/SourceKind.fs' 'let ids = [ "xml" ]' | Out-Null
  Check 'LanguageService が Bolero を参照している' `
    (With $ok @{ ServiceDir = (Join-Path $tmp 'svc-bad') }) `
    $false 'FsBulletML2.LanguageService.fsproj が表示側を参照している'

  # **`.Host` も同じ線の内側。** 片方 だけ当てても、もう片方 は素通りする
  Make 'host-bad/FsBulletML2.LanguageService.Host.fsproj' `
    '<Project><ItemGroup><PackageReference Include="Blazor" /></ItemGroup></Project>' | Out-Null
  Make 'host-bad/Spec.fs' 'let elements = []' | Out-Null
  Check 'LanguageService.Host が Blazor を参照している' `
    (With $ok @{ HostDir = (Join-Path $tmp 'host-bad') }) `
    $false 'FsBulletML2.LanguageService.Host.fsproj が表示側を参照している'

  # **コメントの中の但し書きは数えない。** 数えると「Bolero を参照しない」と
  # proj に書けなくなる（実際に踏んだ）
  Make 'svc-note/FsBulletML2.LanguageService.fsproj' `
    '<Project><!-- Monaco も Bolero も Blazor も参照しない --><ItemGroup /></Project>' | Out-Null
  Make 'svc-note/SourceKind.fs' 'let ids = [ "xml" ]' | Out-Null
  Check 'proj のコメントで Bolero に触れているだけ' `
    (With $ok @{ ServiceDir = (Join-Path $tmp 'svc-note') }) $true ''

  # **要素名は DTD から引く。** ここでは vanish を Fable 側に書いてみる
  $badFable = Join-Path $tmp 'bad/fable'
  Make 'bad/fable/Xml.fs' 'let root = "vanish"' | Out-Null
  Make 'bad/wwwroot/index.html' `
    ($okHead + $okMonaco + $okBoot) | Out-Null
  Check 'Fable 側に要素名が在る' `
    (With $ok @{ PlaygroundDir = 'bad'; IndexHtml = (Join-Path $tmp 'bad/wwwroot/index.html'); Files = @() }) `
    $false 'ブラウザ側に要素名'

  # **器へ移した先でも赤くなること。** v0.8 で `languages/Xml.fs` が
  # `$fableDir` から器へ移った —— 走査先を伸ばさないと、移した先で書いても
  # 赤くならない（守る対象が消えたのに緑を出し続ける形）
  Make 'svc-name/FsBulletML2.LanguageService.fsproj' '<Project><ItemGroup /></Project>' | Out-Null
  Make 'svc-name/Languages/Xml.fs' 'let root = "vanish"' | Out-Null
  Check '器に要素名が在る' (With $ok @{ ServiceDir = (Join-Path $tmp 'svc-name') }) `
    $false 'ブラウザ側に要素名'

  # **`.Host` に在るのは正しい。** `Spec.fs` は要素名で引く散文の表。
  # ここが赤になると、散文が書けなくなる（既定の `host/Spec.fs` が
  # `"vanish"` を持っているので、通る側の点がそのまま証拠）
  Check 'Host に要素名が在っても通る' $ok $true ''

  # **`Monaco` の語ではなく、叩いている字を当てる。** 語で当てると
  # doc コメントや `MonacoLanguage` まで赤になる
  $badFable = Join-Path $tmp 'monaco/fable'
  Make 'monaco/fable/Playground.fs' 'let ed = globalThis.monaco.editor.getEditors()' | Out-Null
  Make 'monaco/fable/Monaco.fs' '[<Emit("globalThis.monaco.editor.create($0)")>] let create = jsNative' | Out-Null
  Make 'monaco/wwwroot/index.html' `
    ($okHead + $okMonaco + $okBoot) | Out-Null
  Check 'Monaco.fs の外から Monaco を叩いている' `
    (With $ok @{ PlaygroundDir = 'monaco'; IndexHtml = (Join-Path $tmp 'monaco/wwwroot/index.html'); Files = @() }) `
    $false 'Monaco.fs の外から Monaco を叩いている'

  # **言及だけなら通る。** ここが赤になると、doc コメントが書けなくなる
  Make 'monaco2/fable/Monaco.fs' '[<Emit("globalThis.monaco.editor.create($0)")>] let create = jsNative' | Out-Null
  Make 'monaco2/fable/Note.fs' '/// Monaco の語の定義に頼らない。member _.MonacoLanguage = "text"' | Out-Null
  Make 'monaco2/wwwroot/index.html' `
    ($okHead + $okMonaco + $okBoot) | Out-Null
  Check 'doc コメントで Monaco に触れているだけ' `
    (With $ok @{ PlaygroundDir = 'monaco2'; IndexHtml = (Join-Path $tmp 'monaco2/wwwroot/index.html'); Files = @() }) `
    $true ''

  # **LanguageService も走査の中に居る。** あちらのソースは Fable が
  # ProjectReference を辿って焼くので、ブラウザ側でも走る = 叩けてしまう位置
  Make 'svc-monaco/FsBulletML2.LanguageService.fsproj' '<Project><ItemGroup /></Project>' | Out-Null
  Make 'svc-monaco/Hover.fs' 'let ed = globalThis.monaco.editor.getEditors()' | Out-Null
  Check 'LanguageService から Monaco を叩いている' `
    (With $ok @{ ServiceDir = (Join-Path $tmp 'svc-monaco') }) `
    $false 'Monaco.fs の外から Monaco を叩いている'

  Make 'host-monaco/FsBulletML2.LanguageService.Host.fsproj' '<Project><ItemGroup /></Project>' | Out-Null
  Make 'host-monaco/Hover.fs' 'let ed = globalThis.monaco.editor.getEditors()' | Out-Null
  Check 'LanguageService.Host から Monaco を叩いている' `
    (With $ok @{ HostDir = (Join-Path $tmp 'host-monaco') }) `
    $false 'Monaco.fs の外から Monaco を叩いている'

  # `src` 付きにして、**インラインの点でなく起動のロジックの点で赤くする**
  $badHtml = Make 'pg/wwwroot/bad.html' `
    ($okHead + $okMonaco + '<script src="boot.js">Blazor.start().catch(function (e) {});</script>')
  Check 'html に起動のロジックが在る' (With $ok @{ IndexHtml = $badHtml }) $false 'html に起動のロジック'

  # **固定していない形を 2 通り 当てる。** `latest` だけ見ていると、
  # `^0.56` のような範囲が通る。**版のところだけ差し替える**
  $latest = Make 'pg/wwwroot/latest.html' `
    ($okHead + $okMonaco.Replace('@0.56.0', '@latest') + $okBoot)
  Check 'Monaco の版が latest' (With $ok @{ IndexHtml = $latest }) $false '版が固定されていない'

  $range = Make 'pg/wwwroot/range.html' `
    ($okHead + $okMonaco.Replace('@0.56.0', '@^0.56') + $okBoot)
  Check 'Monaco の版が範囲' (With $ok @{ IndexHtml = $range }) $false '版が固定されていない'

  # --- SRI と CSP。**どれも 1 か所 だけ欠けた形で当てる**
  $noSri = Make 'pg/wwwroot/nosri.html' `
    ($okHead + '<script src="https://cdn.jsdelivr.net/npm/monaco-editor@0.56.0/min/vs/loader.js"></script>' + $okBoot)
  Check 'integrity が無い' (With $ok @{ IndexHtml = $noSri }) $false 'integrity が無い'

  $noCors = Make 'pg/wwwroot/nocors.html' `
    ($okHead + $okMonaco.Replace(' crossorigin="anonymous"', '') + $okBoot)
  Check 'crossorigin が無い' (With $ok @{ IndexHtml = $noCors }) $false 'crossorigin が無い'

  $noCsp = Make 'pg/wwwroot/nocsp.html' ($okMonaco + $okBoot)
  Check 'CSP が無い' (With $ok @{ IndexHtml = $noCsp }) $false 'CSP が index.html に無い'

  # **中身の在るインライン script。** CSP に 'unsafe-inline' を出していないので
  # 黙って弾かれ、「起動待ち」で止まる（実際に踏んだ）
  $inline = Make 'pg/wwwroot/inline.html' `
    ($okHead + $okMonaco + '<script type="module">import "./js/Playground.js";</script>')
  Check '中身の在るインライン script が在る' (With $ok @{ IndexHtml = $inline }) $false 'インライン script'

  # **空の importmap は数えない。** 数えると ESM へ移すときの受け皿が置けなくなる
  $emptyMap = Make 'pg/wwwroot/emptymap.html' `
    ($okHead + '<script type="importmap"></script>' + $okMonaco + $okBoot)
  Check '空の importmap は通る' (With $ok @{ IndexHtml = $emptyMap }) $true ''

  # **コメントの中の例文は数えない。** 数えると但し書きが書けなくなる
  # （実際に踏んだ —— 「前はこう書いていた」の例に当たって本番の軸が赤になった）
  $commented = Make 'pg/wwwroot/commented.html' `
    ($okHead + '<!-- 前は <script type="module">import "./x.js";</script> と書いていた。' +
     'monaco-editor@latest も Blazor.start() もここでは字であって markup ではない -->' +
     $okMonaco + $okBoot)
  Check 'コメントの中の例文は通る' (With $ok @{ IndexHtml = $commented }) $true ''

  # **書き忘れと、書いてあるが逆を、別々に当てる。** 既定は SDK 側が立てるので、
  # 「無い」は「true と書いた」と同じ結果になる —— 門の側では別の壊れ方
  $noKeys = Make 'pg/NoKeys.fsproj' '<Project><PropertyGroup /></Project>'
  Check 'UseSystemResourceKeys が無い' (With $ok @{ PlaygroundProj = $noKeys }) `
    $false 'UseSystemResourceKeys が csproj に無い'

  $trueKeys = Make 'pg/TrueKeys.fsproj' `
    '<Project><PropertyGroup><UseSystemResourceKeys>true</UseSystemResourceKeys></PropertyGroup></Project>'
  Check 'UseSystemResourceKeys が true' (With $ok @{ PlaygroundProj = $trueKeys }) `
    $false 'UseSystemResourceKeys が false でない'

  Write-Host '=== 材料が読めないときも落ちる'
  # **ここが本体。** 0 件 は違反 0 件 と同じ顔をする
  Check '当てる要素名が 0 個' (With $ok @{ DtdSource = (Make 'core/Empty.fs' 'なにも書いていない') }) `
    $false '当てる要素名を 1 つ も引けなかった'
  Check 'Fable のソースが 0 本' (With $ok @{ PlaygroundDir = 'nowhere' }) `
    $false 'Fable のソースを 1 本 も読めなかった'
  Check 'LanguageService のソースが 0 本' (With $ok @{ ServiceDir = (Join-Path $tmp 'nowhere') }) `
    $false 'LanguageService のソースを 1 本 も読めなかった'
  # **`obj/` の下しか無いのも 0 本。** 数えると、本数が手元（build 済み）と
  # CI（clean）で変わり、生成物だけで「1 本 も無い」が満たされなくなる
  Make 'svc-obj/FsBulletML2.LanguageService.fsproj' '<Project><ItemGroup /></Project>' | Out-Null
  Make 'svc-obj/obj/Debug/net10.0/AssemblyInfo.fs' 'namespace Generated' | Out-Null
  Check 'obj の下の生成物しか無い' (With $ok @{ ServiceDir = (Join-Path $tmp 'svc-obj') }) `
    $false 'LanguageService のソースを 1 本 も読めなかった'
  Check 'LanguageService.Host のソースが 0 本' (With $ok @{ HostDir = (Join-Path $tmp 'nowhere') }) `
    $false 'LanguageService.Host のソースを 1 本 も読めなかった'
  Check 'csproj が読めない' (With $ok @{ PlaygroundProj = (Join-Path $tmp 'pg/Nope.fsproj') }) `
    $false 'Playground の csproj を読めなかった'
  Check 'html が読めない' (With $ok @{ IndexHtml = (Join-Path $tmp 'pg/wwwroot/nope.html') }) `
    $false 'index.html を読めなかった'
  $noMonaco = Make 'pg/wwwroot/nomonaco.html' ($okHead + $okBoot)
  Check 'Monaco の src が無い' (With $ok @{ IndexHtml = $noMonaco }) `
    $false 'Monaco の src が index.html に無い'

  Write-Host '=== 本番の軸'
  # 差し替えを 1 つ も渡さないで呼ぶ。渡した数点が緑でも、
  # git から一覧を引く経路（本番で通る側）を通らない
  $count++
  try {
    & $guard -RepoRoot $root -Quiet
    Write-Host '  ok   repo の現物  -> 通る'
  } catch {
    $fails++
    Write-Host '  NG   repo の現物  -> 落ちた'
    Write-Host "         $_"
  }
} finally {
  Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host ''
if ($fails -gt 0) {
  Write-Host "校正 $count 点 中 $fails 点 が外れた"
  exit 1
}
Write-Host "校正 $count 点 すべて一致"
