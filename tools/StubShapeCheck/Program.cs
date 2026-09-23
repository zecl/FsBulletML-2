// COMPILE-ONLY stub と本物の Unity dll を、呼び手の IL に焼き込まれる面だけで突き合わせる。
using System.Reflection;
using System.Text.Json;

var repoRoot = args.Length > 0 ? args[0] : FindRepoRoot();
if (repoRoot is null)
{
    Console.WriteLine("測っていない: FsBulletML2.slnx が見つからない");
    Console.WriteLine("  repo の場所を第 1 引数 で渡すこと");
    return 2;
}

var proj = Path.Combine(repoRoot, "samples", "FsBulletML2.Sample.Unity2D.FSharp");
var verFile = Path.Combine(proj, "ProjectSettings", "ProjectVersion.txt");
if (!File.Exists(verFile))
{
    Console.WriteLine($"測っていない: {verFile} が無い");
    return 2;
}

var ver = File.ReadLines(verFile).First().Replace("m_EditorVersion:", "").Trim();
Console.WriteLine($"Unity {ver}");

var editor = args.Length > 1 ? args[1] : UnityManaged(ver).FirstOrDefault(Directory.Exists);
if (editor is null || !Directory.Exists(editor))
{
    Console.WriteLine($"測っていない: Unity {ver} の Managed が見つからない");
    foreach (var c in UnityManaged(ver)) Console.WriteLine($"    探した: {c}");
    Console.WriteLine("  UNITY_EDITOR_DATA_MANAGED（直指し）か");
    Console.WriteLine("  UNITY_HUB_EDITOR_PATH（Hub の Editor 置き場）で渡せる");
    return 2;
}
// この行は guard-stub-shape.ps1 が読む。探すところを 2 つ に増やさないため
Console.WriteLine($"Unity Managed: {editor}");

// slnx を自分の居場所から上る。dotnet run でも焼いた exe でも同じところに着く。
static string FindRepoRoot()
{
    foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        for (var d = new DirectoryInfo(start); d is not null; d = d.Parent)
            if (File.Exists(Path.Combine(d.FullName, "FsBulletML2.slnx")))
                return d.FullName;
    return null;
}

// Unity の置き場の候補。先に在ったものを使う
static IEnumerable<string> UnityManaged(string ver)
{
    var direct = Environment.GetEnvironmentVariable("UNITY_EDITOR_DATA_MANAGED");
    if (!string.IsNullOrWhiteSpace(direct)) yield return direct;

    foreach (var hub in HubRoots())
    {
        yield return Path.Combine(hub, ver, "Editor", "Data", "Managed");   // Windows / Linux
        yield return Path.Combine(hub, ver, "Unity.app", "Contents", "Managed");  // macOS
    }
}

static IEnumerable<string> HubRoots()
{
    var env = Environment.GetEnvironmentVariable("UNITY_HUB_EDITOR_PATH");
    if (!string.IsNullOrWhiteSpace(env)) yield return env;

    // Hub が「既定と別の場所に入れる」設定を持っていればそこ。
    // 中身は JSON の文字列 1 つ
    foreach (var cfg in new[]
    {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                     "UnityHub", "secondaryInstallPath.json"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                     "Library", "Application Support", "UnityHub", "secondaryInstallPath.json"),
    })
    {
        string p = null;
        try { if (File.Exists(cfg)) p = JsonSerializer.Deserialize<string>(File.ReadAllText(cfg)); }
        catch { }
        if (!string.IsNullOrWhiteSpace(p)) yield return p;
    }

    var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
    if (!string.IsNullOrWhiteSpace(pf)) yield return Path.Combine(pf, "Unity", "Hub", "Editor");
    yield return Path.Combine("/", "Applications", "Unity", "Hub", "Editor");
    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    if (!string.IsNullOrWhiteSpace(home)) yield return Path.Combine(home, "Unity", "Hub", "Editor");
}

// --- 本物の dll をかき集める --------------------------------------------
var realPaths = new List<string>();
void AddDir(string d)
{
    if (!Directory.Exists(d)) return;
    foreach (var f in Directory.GetFiles(d, "*.dll")) realPaths.Add(f);
}
AddDir(Path.Combine(proj, "Library", "ScriptAssemblies"));
AddDir(Path.Combine(editor, "UnityEngine"));
AddDir(editor);
var pc = Path.Combine(proj, "Library", "PackageCache");
if (Directory.Exists(pc))
    foreach (var d in Directory.GetDirectories(pc, "*", SearchOption.AllDirectories))
        if (Path.GetFileName(d) is "Runtime" or "lib") AddDir(d);

// 参照解決のために BCL も足す
var bcl = Path.GetDirectoryName(typeof(object).Assembly.Location);
var bclPaths = Directory.GetFiles(bcl, "*.dll").ToList();
realPaths.AddRange(bclPaths);

// --- stub の dll --------------------------------------------------------
var stubPaths = new List<string>();
foreach (var d in Directory.GetDirectories(Path.Combine(repoRoot, "src"), "*.Stub"))
{
    var bin = Path.Combine(d, "bin", "Release");
    if (!Directory.Exists(bin)) continue;
    var name = Path.GetFileName(d).Replace(".Stub", "");
    foreach (var f in Directory.GetFiles(bin, name + ".dll", SearchOption.AllDirectories))
    { stubPaths.Add(f); break; }
}
Console.WriteLine($"stub {stubPaths.Count} 本 / 本物を探す dll {realPaths.Count} 本");

string Sig(MethodBase m)
{
    var ret = m is MethodInfo mi ? Name(mi.ReturnType) : "void";
    var ps = string.Join(", ", m.GetParameters().Select(p => Name(p.ParameterType) + (p.HasDefaultValue ? " = " + (p.RawDefaultValue?.ToString() ?? "null") : "")));
    return $"{ret} ({ps})";
}
// アセンブリ修飾を落として比べる。FullName はジェネリック引数に
// 「, Assembly, Version=…」を含むので、facade 越しの同じ型が別物に見える
string Name(Type t)
{
    try
    {
        if (t.IsArray) return Name(t.GetElementType()) + "[]";
        if (t.IsByRef) return Name(t.GetElementType()) + "&";
        if (t.IsPointer) return Name(t.GetElementType()) + "*";
        if (t.IsConstructedGenericType)
            return t.GetGenericTypeDefinition().FullName + "<" +
                   string.Join(",", t.GenericTypeArguments.Select(Name)) + ">";
        return t.FullName ?? t.Name;
    }
    catch { return t.Name; }
}
string Kind(Type t) => t.IsEnum ? "enum" : t.IsValueType ? "struct" : t.IsInterface ? "interface" : "class";
bool Shown(MethodBase m) => m.IsPublic || m.IsFamily || m.IsFamilyOrAssembly;
bool ShownF(FieldInfo f) => f.IsPublic || f.IsFamily || f.IsFamilyOrAssembly;

// 本物: 型のフル名 -> Type（最初に見つかったもの）
var realResolver = new PathAssemblyResolver(realPaths.Distinct());
using var realCtx = new MetadataLoadContext(realResolver, Path.GetFileNameWithoutExtension(
    realPaths.First(p => Path.GetFileName(p) == "System.Private.CoreLib.dll")));
var real = new Dictionary<string, Type>();
foreach (var p in realPaths.Distinct())
{
    Assembly a;
    try { a = realCtx.LoadFromAssemblyPath(p); } catch { continue; }
    Type[] ts;
    try { ts = a.GetTypes(); } catch { continue; }
    foreach (var t in ts)
    {
        var n = Name(t);
        if (n == null) continue;
        if (!real.ContainsKey(n)) real[n] = t;
    }
}
Console.WriteLine($"本物: {real.Count} 型");

var bad = new List<string>();
var checkedTypes = 0;

var stubResolver = new PathAssemblyResolver(stubPaths.Concat(bclPaths).Distinct());
using var stubCtx = new MetadataLoadContext(stubResolver, Path.GetFileNameWithoutExtension(
    bclPaths.First(p => Path.GetFileName(p) == "System.Private.CoreLib.dll")));

foreach (var p in stubPaths)
{
    var a = stubCtx.LoadFromAssemblyPath(p);
    Type[] ts;
    try { ts = a.GetTypes(); } catch (ReflectionTypeLoadException e) { ts = e.Types.Where(t => t != null).ToArray(); }
    foreach (var st in ts)
    {
        if (!st.IsPublic && !st.IsNestedPublic) continue;
        var n = Name(st);
        if (n == null || n.StartsWith("System.Runtime.CompilerServices")) continue;
        if (!real.TryGetValue(n, out var rt)) { bad.Add($"  {n,-52} 本物に無い"); continue; }
        checkedTypes++;

        if (Kind(st) != Kind(rt)) bad.Add($"  {n,-52} kind  stub={Kind(st)} 本物={Kind(rt)}");

        const BindingFlags BF = BindingFlags.Public | BindingFlags.NonPublic |
                                BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (var sf in st.GetFields(BF).Where(ShownF))
        {
            var rf = rt.GetField(sf.Name, BF);
            if (rf == null)
            {
                var asProp = rt.GetProperty(sf.Name, BF) != null ? "property" : null;
                bad.Add($"  {n,-52} field {sf.Name}  本物では {(asProp ?? "無い")}");
            }
            else if (Name(rf.FieldType) != Name(sf.FieldType))
                bad.Add($"  {n,-52} field {sf.Name}  型 stub={Name(sf.FieldType)} 本物={Name(rf.FieldType)}");
        }

        foreach (var sp in st.GetProperties(BF))
        {
            var rp = rt.GetProperty(sp.Name, BF);
            if (rp == null)
            {
                var asField = rt.GetField(sp.Name, BF) != null ? "field" : null;
                bad.Add($"  {n,-52} prop  {sp.Name}  本物では {(asField ?? "無い")}");
            }
            else if (Name(rp.PropertyType) != Name(sp.PropertyType))
                bad.Add($"  {n,-52} prop  {sp.Name}  型 stub={Name(sp.PropertyType)} 本物={Name(rp.PropertyType)}");
        }

        foreach (var sm in st.GetMethods(BF).Where(Shown))
        {
            if (sm.IsSpecialName) continue;   // get_/set_/op_ は property 側で見る
            var sig = Sig(sm);
            var cands = rt.GetMethods(BF).Where(m => m.Name == sm.Name).ToArray();
            if (cands.Length == 0) { bad.Add($"  {n,-52} method {sm.Name}  本物に無い"); continue; }
            if (!cands.Any(m => Sig(m) == sig))
                bad.Add($"  {n,-52} method {sm.Name}{sig}  本物は {string.Join(" / ", cands.Select(Sig))}");
        }

        foreach (var sc in st.GetConstructors(BF).Where(Shown))
        {
            var sig = Sig(sc);
            var cands = rt.GetConstructors(BF).ToArray();
            if (cands.Length == 0) { bad.Add($"  {n,-52} ctor  本物に無い"); continue; }
            if (!cands.Any(m => Sig(m) == sig))
                bad.Add($"  {n,-52} ctor {sig}  本物は {string.Join(" / ", cands.Select(Sig))}");
        }
    }
}

Console.WriteLine();
Console.WriteLine($"突き合わせた型: {checkedTypes}");
Console.WriteLine("=== 食い違い");
if (bad.Count == 0) { Console.WriteLine("  無し"); return 0; }
foreach (var b in bad.Distinct().OrderBy(x => x)) Console.WriteLine(b);
Console.WriteLine();
Console.WriteLine($"{bad.Distinct().Count()} 件");
return 1;
