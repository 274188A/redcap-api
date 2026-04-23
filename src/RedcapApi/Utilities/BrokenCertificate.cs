using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Redcap.Utilities
{
    /// <summary>
    /// Exposes a server-certificate validation callback that accepts ANY certificate, including
    /// expired, revoked, self-signed, or MITM-substituted ones. Intended for local development
    /// against self-signed REDCap instances. Never use this in production.
    /// </summary>
    public static class BrokenCertificate
    {
        /// <summary>
        /// Unconditional-true server certificate validator. Pass this to an <c>HttpClientHandler</c>
        /// or <c>SocketsHttpHandler</c> you construct yourself, and inject the resulting handler via
        /// <see cref="Redcap.Api.DefaultRedcapTransport"/> for per-instance TLS bypass.
        /// </summary>
        public static Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> DangerousAcceptAnyServerCertificateValidator { get; } = delegate { return true; };
    }
}
