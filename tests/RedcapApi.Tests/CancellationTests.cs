using Redcap.Api;
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

    [Fact]
    public async Task ExportRecordsAsync_WhenTransportRespectsCancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var transport = new CancellationRespectingTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            api.ExportRecordsAsync(cancellationToken: cts.Token));
    }

    [Fact]
    public async Task ExportUsersAsync_WhenPerCallTimeoutExceeded_ThrowsTaskCanceledException()
    {
        using var server = new LocalHttpServer(_ =>
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
            return new TestResponse(200, "ok");
        });
        using var transport = new DefaultRedcapTransport(timeOutSeconds: 10);
        var api = new Redcap.RedcapApi(server.Url.ToString(), Token, transport);

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            api.ExportUsersAsync(timeOutSeconds: 1));
    }

    [Fact]
    public void Dispose_WhenTransportIsInjected_DoesNotDisposeTransport()
    {
        var transport = new DisposableTrackingTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        api.Dispose();

        Assert.False(transport.DisposeCalled);
    }

    [Fact]
    public void DefaultTransport_WhenHandlerIsInjected_DisposesOwnedHttpClient()
    {
        var handler = new TrackingHandler();
        var transport = new DefaultRedcapTransport(handler);

        transport.Dispose();

        Assert.True(handler.DisposeCalled);
    }

    [Fact]
    public async Task DefaultTransport_WhenHttpClientIsInjected_LeavesClientOwnedByCaller()
    {
        var handler = new TrackingHandler();
        using var client = new HttpClient(handler);
        var transport = DefaultRedcapTransport.FromHttpClient(client);

        var response = await transport.SendPostRequestAsync(
            new Dictionary<string, string> { ["token"] = Token },
            new Uri("http://localhost/api"));
        transport.Dispose();

        Assert.Equal("ok", response);
        Assert.False(handler.DisposeCalled);
    }

    [Fact]
    public async Task Dispose_WhenClientOwnsTransport_PreventsFurtherCalls()
    {
        using var server = new LocalHttpServer(_ => new TestResponse(200, "ok"));
        var api = new Redcap.RedcapApi(server.Url.ToString(), Token);
        api.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => api.ExportUsersAsync());
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

    private sealed class DisposableTrackingTransport : IRedcapTransport, IDisposable
    {
        public bool DisposeCalled { get; private set; }

        public Task<Stream?> GetStreamContentAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
            => Task.FromResult<Stream?>(new MemoryStream());

        public Task<string> SendPostRequestAsync(MultipartFormDataContent payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
            => Task.FromResult("transport-response");

        public Task<string> SendPostRequestAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
            => Task.FromResult("transport-response");

        public Task<string> DownloadFileAsync(Dictionary<string, string> payload, Uri uri, string destinationPath, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
            => Task.FromResult("transport-response");

        public void Dispose()
        {
            DisposeCalled = true;
        }
    }

    private sealed class TrackingHandler : HttpMessageHandler
    {
        public bool DisposeCalled { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("ok")
            });
        }

        protected override void Dispose(bool disposing)
        {
            DisposeCalled = true;
            base.Dispose(disposing);
        }
    }
}
