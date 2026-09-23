using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// COMPILE-ONLY stub。走らせるものではない。
// 本物は Unity 向けにしか配られていない。gRPC の HTTP/2 に要る。
namespace Cysharp.Net.Http
{
    /// <summary>Unity で HTTP/2 を喋る handler。ここは空</summary>
    public sealed class YetAnotherHttpHandler : HttpMessageHandler
    {
        /// <summary>平文 の HTTP/2（h2c）で繋ぐ。証明書 を要求しない</summary>
        public bool Http2Only { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new NotSupportedException("compile-only stub");
    }
}
