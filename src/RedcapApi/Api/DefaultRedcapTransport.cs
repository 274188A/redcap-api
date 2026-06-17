using Redcap.Exceptions;
using Redcap.Interfaces;
using Redcap.Utilities;

namespace Redcap.Api;

/// <summary>
/// Default HTTP transport implementation used by RedcapApi.
/// Owns a single <see cref="HttpClient"/> for the lifetime of the transport instance.
/// </summary>
public sealed class DefaultRedcapTransport : IRedcapTransport, IDisposable
{
    private readonly HttpClient _client;
    private readonly TimeSpan _defaultTimeout;
    private readonly bool _ownsClient;

    /// <param name="handler">
    /// Optional handler. Supply one when you need custom TLS settings (e.g. self-signed certs in dev).
    /// </param>
    /// <param name="timeOutSeconds">
    /// Default request timeout applied when a call does not specify a positive <c>timeOutSeconds</c> override.
    /// </param>
    public DefaultRedcapTransport(HttpMessageHandler? handler = null, long timeOutSeconds = 100)
        : this(
            new HttpClient(handler ?? new HttpClientHandler())
            {
                Timeout = Timeout.InfiniteTimeSpan
            },
            timeOutSeconds,
            ownsClient: true)
    {
    }

    /// <summary>
    /// Creates a transport around a caller-owned <see cref="HttpClient"/>, such as one created by
    /// <c>IHttpClientFactory</c>.
    /// </summary>
    /// <param name="client">Caller-owned HTTP client. Disposing this transport will not dispose it.</param>
    /// <param name="timeOutSeconds">
    /// Default request timeout applied when a call does not specify a positive <c>timeOutSeconds</c> override.
    /// </param>
    public static DefaultRedcapTransport FromHttpClient(HttpClient client, long timeOutSeconds = 100)
    {
        return new DefaultRedcapTransport(client, timeOutSeconds, ownsClient: false);
    }

    private DefaultRedcapTransport(HttpClient client, long timeOutSeconds, bool ownsClient)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _defaultTimeout = timeOutSeconds > 0
            ? TimeSpan.FromSeconds(timeOutSeconds)
            : TimeSpan.FromSeconds(100);
        _ownsClient = ownsClient;
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
    public void Dispose()
    {
        if (_ownsClient)
        {
            _client.Dispose();
        }
    }

    private async Task<T> ExecuteWithTimeoutAsync<T>(CancellationToken cancellationToken, long timeOutSeconds, Func<CancellationToken, Task<T>> action)
    {
        var timeout = ResolveTimeout(timeOutSeconds);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(timeout);
        try
        {
            return await action(linkedCts.Token);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // The linked token fired but the caller's token did not, so this is a per-call timeout,
            // not a caller-initiated cancellation. Surface it distinctly instead of as a bare
            // OperationCanceledException that callers cannot tell apart from their own cancellation.
            throw new RedcapApiException(
                $"The REDCap request timed out after {timeout.TotalSeconds:0.###} seconds.", ex);
        }
    }

    private TimeSpan ResolveTimeout(long timeOutSeconds) =>
        timeOutSeconds > 0
            ? TimeSpan.FromSeconds(timeOutSeconds)
            : _defaultTimeout;
}
