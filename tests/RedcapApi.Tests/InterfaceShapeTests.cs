using Redcap.Interfaces;
using Xunit;

namespace RedcapApi.Tests;

public class InterfaceShapeTests
{
    private const string Token = "token123";

    [Fact]
    public void RedcapApi_CanBeUsedThroughFocusedDomainInterfaces()
    {
        var api = new Redcap.RedcapApi("http://localhost/", Token, new FakeTransport());

        Assert.IsAssignableFrom<IRedcap>(api);
        Assert.IsAssignableFrom<IRedcapRecords>(api);
        Assert.IsAssignableFrom<IRedcapProjects>(api);
        Assert.IsAssignableFrom<IRedcapUsers>(api);
        Assert.IsAssignableFrom<IRedcapFiles>(api);
    }

    private sealed class FakeTransport : IRedcapTransport
    {
        public Task<Stream?> GetStreamContentAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
            => Task.FromResult<Stream?>(new MemoryStream());

        public Task<string> SendPostRequestAsync(MultipartFormDataContent payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
            => Task.FromResult("transport-response");

        public Task<string> SendPostRequestAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
            => Task.FromResult("transport-response");

        public Task<string> DownloadFileAsync(Dictionary<string, string> payload, Uri uri, string destinationPath, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
            => Task.FromResult("transport-response");
    }
}
