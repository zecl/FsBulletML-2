using System;
using System.Linq;
using FsBulletML2.Sample.Server.MagicOnion.Engine;
using FsBulletML2.Sample.Server.MagicOnion.Logging;
using FsBulletML2.Sample.Server.MagicOnion.Rooms;
using MagicOnion.Server.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

// 弾幕エンジンをサーバーで走らせて、弾の並びを配る。**Console アプリ。**
//
// 眺めて分かることを 3 段 で出す。
//
//   出入り   繋がった / 部屋 へ入った / 出た / 切れた
//   呼び     口 が呼ばれた 1 行（高頻度の 2 本 は数えるだけ）
//   状況     1 秒 に 1 行。部屋・人数・コマ・弾数・当たり・帯域 の概算
//
// **h2c（暗号化しない HTTP/2）で立てる。** gRPC は HTTP/2 が要るが、
// 証明書を要求すると「動かし方 1 枚」に証明書の話が 1 段 増える。
// **本番の作法ではない。** サンプルなのでここは手数を取る。

// **立てずに数えるだけの道。** 同梱弾幕 を 1 本 ずつ回して、
// 1 コマ あたりの弾数 を出す
if (args.Contains("--measure"))
{
    int at = Array.IndexOf(args, "--measure");
    int measureFrames = at + 1 < args.Length && int.TryParse(args[at + 1], out var mf) ? mf : 600;
    return Measure.Run(measureFrames, seed: 12345, top: 10);
}

var accessLog = new AccessLogOptions
{
    AllCalls = args.Contains("--all-calls"),
    StatusInterval = StatusInterval(args),
};

var builder = WebApplication.CreateBuilder(args);

// **骨組み の log を黙らせる。** 既定では Kestrel と gRPC が
// 1 接続 につき数行 出すので、こちらの 3 段 が埋まる。
// --verbose で戻る（港 が開かない・HTTP/2 で折り合わない を割るときに要る）
builder.Logging.ClearProviders();
builder.Logging.AddConsoleFormatter<PlainConsoleFormatter, ConsoleFormatterOptions>();
builder.Logging.AddConsole(console => console.FormatterName = PlainConsoleFormatter.FormatterName);
builder.Logging.SetMinimumLevel(LogLevel.Information);
if (!args.Contains("--verbose"))
{
    builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
    builder.Logging.AddFilter("Grpc", LogLevel.Warning);
    builder.Logging.AddFilter("MagicOnion", LogLevel.Warning);
}

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.ListenAnyIP(5170, listen => listen.Protocols = HttpProtocols.Http2);
});

builder.Services.AddGrpc();
builder.Services.AddMagicOnion(options =>
{
    // **アクセスログ は filter で挟む。** 口 の実装に log を書くと、
    // 口 が増えるたびに書き忘れる。ここなら 1 か所
    options.GlobalStreamingHubFilters.Add(new StreamingHubFilterDescriptor(typeof(HubAccessLogFilter)));
    options.GlobalFilters.Add(new MagicOnionServiceFilterDescriptor(typeof(ServiceAccessLogFilter)));
});

builder.Services.AddSingleton(accessLog);
builder.Services.AddSingleton<CallCounter>();
builder.Services.AddSingleton<ConnectionCounter>();
builder.Services.AddHostedService<StatusPrinter>();

// **差し替える 1 行。**
//
//   EngineFrameSourceFactory   同梱弾幕 を走らせる（既定）
//   FixedFrameSourceFactory    エンジンを 1 度 も呼ばない。配線だけを見る
//
// 絵 が出ないときに後者へ替えれば、配線かエンジンかが割れる
bool fixedSource = args.Contains("--fixed");
if (fixedSource)
{
    builder.Services.AddSingleton<IFrameSourceFactory, FixedFrameSourceFactory>();
}
else
{
    builder.Services.AddSingleton<IFrameSourceFactory, EngineFrameSourceFactory>();
}

builder.Services.AddSingleton<RoomRegistry>();

var app = builder.Build();
app.MapMagicOnionService();

Banner(app.Services.GetRequiredService<IFrameSourceFactory>(), fixedSource, accessLog);

app.Run();
return 0;

/// <summary>
/// 立ち上がりの 1 枚。<b>log ではなく素 の出力。</b>
/// 時刻 も高さ も要らないし、**繋ぐ前 に読むもの**なので log と混ぜない。
/// </summary>
static void Banner(IFrameSourceFactory factory, bool fixedSource, AccessLogOptions accessLog)
{
    Console.WriteLine();
    Console.WriteLine("FsBulletML2 / MagicOnion サーバー");
    Console.WriteLine("  待ち受け  http://0.0.0.0:5170  （h2c。証明書 は要らない）");
    Console.WriteLine("  コマ      {0} /秒", RoomLoop.Fps);
    Console.WriteLine(
        "  弾幕      {0}{1}",
        fixedSource ? "エンジンを呼ばない固定の並び" : $"同梱 {factory.List().Length} 本",
        fixedSource ? "" : "（--fixed で配線だけを見る並びへ）");
    Console.WriteLine("  当たり    サーバーが判定する（弾は当たった側 で消える）");
    Console.WriteLine(
        "  状況      {0}",
        accessLog.StatusInterval <= TimeSpan.Zero
            ? "出さない"
            : $"{accessLog.StatusInterval.TotalSeconds:0.#} 秒 ごと（誰も居なければ黙る）");
    Console.WriteLine("  呼び      {0}", accessLog.AllCalls ? "全部 1 行 ずつ" : "出入り だけ（高頻度の 2 本 は数えて状況 へ）");
    Console.WriteLine();
    Console.WriteLine("  --fixed  --verbose  --all-calls  --status <秒>  --measure <コマ数>");
    Console.WriteLine("  止める    Ctrl+C");
    Console.WriteLine();
}

static TimeSpan StatusInterval(string[] args)
{
    int at = Array.IndexOf(args, "--status");
    if (at < 0)
    {
        return TimeSpan.FromSeconds(1);
    }

    // **--status 0 で消せる。** 数を測るときに行 が混ざると読みにくい
    if (at + 1 < args.Length && double.TryParse(args[at + 1], out var seconds) && seconds >= 0)
    {
        return TimeSpan.FromSeconds(seconds);
    }

    return TimeSpan.FromSeconds(1);
}