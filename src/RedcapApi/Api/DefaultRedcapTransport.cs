using Redcap.Interfaces;
using Redcap.Utilities;

namespace Redcap.Api
{
    /// <summary>
    /// Default HTTP transport implementation used by RedcapApi.
    /// Owns a single <see cref="HttpClient"/> for the lifetime of the transport instance.
    /// </summary>
    public sealed class DefaultRedcapTransport : IRedcapTransport, IDisposable
    {
        private readonly HttpClient _client;

        /// <param name="handler">
        /// Optional handler. Supply one when you need custom TLS settings (e.g. self-signed certs in dev).
        /// </param>
        /// <param name="timeOutSeconds">Request timeout applied to every call made by this transport.</param>
        public DefaultRedcapTransport(HttpMessageHandler? handler = null, long timeOutSeconds = 100)
        {
            _client = new HttpClient(handler ?? new HttpClientHandler());
            _client.Timeout = TimeSpan.FromSeconds(timeOutSeconds);
        }

        /// <inheritdoc />
        public Task<Stream?> GetStreamContentAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return Utils.GetStreamContentAsync(payload, uri, _client, cancellationToken);
        }

        /// <inheritdoc />
        public Task<string> SendPostRequestAsync(MultipartFormDataContent payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return Utils.SendPostRequestAsync(payload, uri, _client, cancellationToken);
        }

        /// <inheritdoc />
        public Task<string> SendPostRequestAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return Utils.SendPostRequestAsync(payload, uri, _client, cancellationToken);
        }

        /// <inheritdoc />
        public Task<string> DownloadFileAsync(Dictionary<string, string> payload, Uri uri, string destinationPath, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return Utils.DownloadFileAsync(payload, uri, _client, destinationPath, cancellationToken);
        }

        /// <inheritdoc />
        public void Dispose() => _client.Dispose();
    }
}
