using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// COMPILE-ONLY stub of YetAnotherHttpHandler. Not a substitute for the real package.
//
// **本物は Unity 向けにしか配られていない**（Cysharp/YetAnotherHttpHandler。
// UPM で入れる）。Unity の素 の HttpClient は HTTP/2 を喋らないので、
// gRPC を通すのにこれが要る。
//
// ここは compile を通すためだけの空実装で、走らせるものではない。
// **アセンブリ名が本物と一致していることが要**（詳しくは
// src/UnityEngine.Stub/UnityEngine.cs の頭）。
namespace Cysharp.Net.Http
{
    /// <summary>Unity で HTTP/2 を喋る handler。<b>ここは空</b></summary>
    public sealed class YetAnotherHttpHandler : HttpMessageHandler
    {
        /// <summary>平文 の HTTP/2（h2c）で繋ぐ。<b>証明書 を要求しない</b></summary>
        public bool Http2Only { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new NotSupportedException("compile-only stub");
    }
}
