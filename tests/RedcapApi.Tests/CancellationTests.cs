using Redcap.Exceptions;
using Redcap.Interfaces;
using Redcap.Models;
using Xunit;

namespace RedcapApi.Tests;

public class CancellationTests
{
    private const string Token = "token123";

    [Fact]
    public async Task ExportRecordsAsync_ForwardsCancellationTokenToTransport()
    {
        using var cts = new CancellationTokenSource();
        var transport = new TokenCapturingTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportRecordsAsync(cancellationToken: cts.Token);

        Assert.Equal(cts.Token, transport.LastCancellationToken);
    }

    [Fact]
    public async Task ImportRecordsAsync_ForwardsCancellationTokenToTransport()
    {
        using var cts = new CancellationTokenSource();
        var transport = new TokenCapturingTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<object> { new { record_id = "1" } };

        await api.ImportRecordsAsync(RedcapFormat.json, RedcapDataType.flat, OverwriteBehavior.normal, false, false, data, cancellationToken: cts.Token);

        Assert.Equal(cts.Token, transport.LastCancellationToken);
    }

    [Fact]
    public async Task ExportFileAsync_ForwardsCancellationTokenToTransport()
    {
        using var cts = new CancellationTokenSource();
        var transport = new TokenCapturingTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            await api.ExportFileAsync("rec1", "file_field", "event_1_arm_1", filePath: tempDir, cancellationToken: cts.Token);

            Assert.Equal(cts.Token, transport.LastCancellationToken);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    // When the transport itself respects the cancellation token, ExecuteAsync
    // wraps the resulting OperationCanceledException in RedcapApiException.
    [Fact]
    public async Task ExportRecordsAsync_WhenTransportRespectsCancelledToken_ThrowsRedcapApiException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var transport = new CancellationRespectingTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await Assert.ThrowsAsync<RedcapApiException>(() =>
            api.ExportRecordsAsync(cancellationToken: cts.Token));
    }

    private sealed class CancellationRespectingTransport : IRedcapTransport
    {
        public Task<Stream?> GetStreamContentAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<Stream?>(new MemoryStream());
        }

        public Task<string> SendPostRequestAsync(MultipartFormDataContent payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult("transport-response");
        }

        public Task<string> SendPostRequestAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult("transport-response");
        }

        public Task<string> DownloadFileAsync(Dictionary<string, string> payload, Uri uri, string destinationPath, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult("transport-response");
        }
    }

    private sealed class TokenCapturingTransport : IRedcapTransport
    {
        public CancellationToken LastCancellationToken { get; private set; }

        public Task<Stream?> GetStreamContentAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            LastCancellationToken = cancellationToken;
            return Task.FromResult<Stream?>(new MemoryStream());
        }

        public Task<string> SendPostRequestAsync(MultipartFormDataContent payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            LastCancellationToken = cancellationToken;
            return Task.FromResult("transport-response");
        }

        public Task<string> SendPostRequestAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            LastCancellationToken = cancellationToken;
            return Task.FromResult("transport-response");
        }

        public Task<string> DownloadFileAsync(Dictionary<string, string> payload, Uri uri, string destinationPath, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            LastCancellationToken = cancellationToken;
            return Task.FromResult("transport-response");
        }
    }
}
