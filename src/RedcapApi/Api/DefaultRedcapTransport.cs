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
        private readonly TimeSpan _defaultTimeout;

        /// <param name="handler">
        /// Optional handler. Supply one when you need custom TLS settings (e.g. self-signed certs in dev).
        /// </param>
        /// <param name="timeOutSeconds">
        /// Default request timeout applied when a call does not specify a positive <c>timeOutSeconds</c> override.
        /// </param>
        public DefaultRedcapTransport(HttpMessageHandler? handler = null, long timeOutSeconds = 100)
        {
            _defaultTimeout = timeOutSeconds > 0
                ? TimeSpan.FromSeconds(timeOutSeconds)
                : TimeSpan.FromSeconds(100);
            _client = new HttpClient(handler ?? new HttpClientHandler());
            _client.Timeout = Timeout.InfiniteTimeSpan;
        }

        /// <inheritdoc />
        public Task<Stream?> GetStreamContentAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return ExecuteWithTimeoutAsync(
                cancellationToken,
                timeOutSeconds,
                token => Utils.GetStreamContentAsync(payload, uri, _client, token));
        }

        /// <inheritdoc />
        public Task<string> SendPostRequestAsync(MultipartFormDataContent payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return ExecuteWithTimeoutAsync(
                cancellationToken,
                timeOutSeconds,
                token => Utils.SendPostRequestAsync(payload, uri, _client, token));
        }

        /// <inheritdoc />
        public Task<string> SendPostRequestAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return ExecuteWithTimeoutAsync(
                cancellationToken,
                timeOutSeconds,
                token => Utils.SendPostRequestAsync(payload, uri, _client, token));
        }

        /// <inheritdoc />
        public Task<string> DownloadFileAsync(Dictionary<string, string> payload, Uri uri, string destinationPath, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return ExecuteWithTimeoutAsync(
                cancellationToken,
                timeOutSeconds,
                token => Utils.DownloadFileAsync(payload, uri, _client, destinationPath, token));
        }

        /// <inheritdoc />
        public void Dispose() => _client.Dispose();

        private async Task<T> ExecuteWithTimeoutAsync<T>(CancellationToken cancellationToken, long timeOutSeconds, Func<CancellationToken, Task<T>> action)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedCts.CancelAfter(ResolveTimeout(timeOutSeconds));
            return await action(linkedCts.Token);
        }

        private TimeSpan ResolveTimeout(long timeOutSeconds) =>
            timeOutSeconds > 0
                ? TimeSpan.FromSeconds(timeOutSeconds)
                : _defaultTimeout;
    }
}
