using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MagicOnion.Server;
using MagicOnion.Server.Filters;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Logging;

namespace FsBulletML2.Sample.Server.MagicOnion.Logging
{
    /// <summary>
    /// Hub の口 が呼ばれたことを出す。<b>MagicOnion の filter。</b>
    ///
    /// <b>降りるほう（<c>OnFrame</c>）はここを通らない。</b>
    /// filter が挟まるのは client から呼ばれた口 だけで、
    /// サーバーから配るぶんは素通り ——
    /// 配れているかは状況 の行（<see cref="StatusPrinter"/>）で見る。
    /// </summary>
    public sealed class HubAccessLogFilter : IStreamingHubFilter
    {
        readonly ILogger logger;
        readonly CallCounter counter;
        readonly AccessLogOptions options;

        public HubAccessLogFilter(ILoggerFactory loggers, CallCounter counter, AccessLogOptions options)
        {
            logger = loggers.CreateLogger("call");
            this.counter = counter;
            this.options = options;
        }

        public async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
        {
            string method = MethodOf(context.Path);
            counter.Hit(method);

            long began = Stopwatch.GetTimestamp();
            try
            {
                await next(context).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                // **落ちたものは、間引きの対象にしない。** 数えるだけにすると
                // 「呼ばれた回数は出るのに、なぜ効かないか分からない」になる
                logger.LogError(ex, "{Path} が落ちた  #{Conn}", context.Path, Conn(context));
                throw;
            }

            if (options.AllCalls || !AccessLogOptions.IsNoisy(method))
            {
                logger.LogInformation(
                    "{Path}  {Elapsed:0.0}ms  #{Conn}", context.Path, Ms(began), Conn(context));
            }
        }

        static string MethodOf(string path)
        {
            int at = path.LastIndexOf('/');
            return at < 0 ? path : path.Substring(at + 1);
        }

        static string Conn(StreamingHubContext context)
            => Short(context.ServiceContext.ContextId);

        internal static string Short(Guid id) => id.ToString("N").Substring(0, 8);

        internal static double Ms(long began)
            => (Stopwatch.GetTimestamp() - began) * 1000.0 / Stopwatch.Frequency;
    }

    /// <summary>
    /// 1 往復 の口（<c>IDanmakuService</c>）を出す。
    /// <b>こちらは間引かない</b> —— 部屋 に入る前 に 1 度 呼ばれるだけなので、
    /// 出しても流れない。
    /// </summary>
    public sealed class ServiceAccessLogFilter : IMagicOnionServiceFilter
    {
        readonly ILogger logger;
        readonly CallCounter counter;

        public ServiceAccessLogFilter(ILoggerFactory loggers, CallCounter counter)
        {
            logger = loggers.CreateLogger("call");
            this.counter = counter;
        }

        public async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            string path = context.CallContext.Method.TrimStart('/');
            counter.Hit(path);

            long began = Stopwatch.GetTimestamp();
            try
            {
                await next(context).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "{Path} が落ちた", path);
                throw;
            }

            logger.LogInformation("{Path}  {Elapsed:0.0}ms", path, HubAccessLogFilter.Ms(began));
        }
    }
}