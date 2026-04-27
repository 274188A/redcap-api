using Redcap.Exceptions;
using Redcap.Interfaces;
using Redcap.Models;
using Xunit;

namespace RedcapApi.Tests;

public class ValidationTests
{
    public static TheoryData<string, Func<Redcap.RedcapApi, Task>, string> EmptyRequiredCollectionCases => new()
    {
        { "DeleteArms", api => api.DeleteArmsAsync(Array.Empty<string>()), "No arm to delete" },
        { "DeleteDags", api => api.DeleteDagsAsync(Array.Empty<string>()), "No dags to delete" },
        { "DeleteEvents", api => api.DeleteEventsAsync(Array.Empty<string>()), "No events to delete" },
        { "ImportEvents", api => api.ImportEventsAsync(false, RedcapFormat.json, new List<object>()), "Events can not be empty or null" },
        { "ImportUserDagAssignment", api => api.ImportUserDagAssignmentAsync(RedcapFormat.json, new List<object>()), "No data to import" },
    };

    [Fact]
    public void Constructor_WithNullToken_ThrowsArgumentNullException()
    {
        var transport = new FakeTransport();

        Assert.Throws<ArgumentNullException>(() => new Redcap.RedcapApi(TestConstants.BaseUrl, null!, transport));
    }

    [Fact]
    public void Constructor_WithEmptyToken_ThrowsArgumentException()
    {
        var transport = new FakeTransport();

        Assert.Throws<ArgumentException>(() => new Redcap.RedcapApi(TestConstants.BaseUrl, string.Empty, transport));
    }

    [Theory]
    [MemberData(nameof(EmptyRequiredCollectionCases))]
    public async Task ApiMethods_WithEmptyRequiredCollections_ThrowWithoutCallingTransport(
        string _,
        Func<Redcap.RedcapApi, Task> apiCall,
        string expectedMessage)
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi(TestConstants.BaseUrl, TestConstants.Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => apiCall(api));

        Assert.Contains(expectedMessage, ex.Message);
        Assert.Null(transport.LastDictionaryPayload);
        Assert.Null(transport.LastMultipartPayload);
    }

    [Fact]
    public async Task ImportFileRepositoryAsync_WithNoFile_ThrowsWithoutCallingTransport()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi(TestConstants.BaseUrl, TestConstants.Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => api.ImportFileRepositoryAsync(file: null));

        Assert.Contains("Please provide a file to import.", ex.Message);
        Assert.Null(transport.LastDictionaryPayload);
        Assert.Null(transport.LastMultipartPayload);
    }

    [Fact]
    public async Task ExportFileAsync_WithMissingField_ThrowsWithoutCallingTransport()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi(TestConstants.BaseUrl, TestConstants.Token, transport);
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
                api.ExportFileAsync("rec1", field: "", eventName: "", filePath: tempDir));

            Assert.Contains("No field provided to export", ex.Message);
            Assert.Null(transport.LastDictionaryPayload);
            Assert.Null(transport.LastMultipartPayload);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private sealed class FakeTransport : IRedcapTransport
    {
        public Dictionary<string, string>? LastDictionaryPayload { get; private set; }

        public MultipartFormDataContent? LastMultipartPayload { get; private set; }

        public Task<Stream?> GetStreamContentAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            LastDictionaryPayload = new Dictionary<string, string>(payload);
            return Task.FromResult<Stream?>(new MemoryStream());
        }

        public Task<string> SendPostRequestAsync(MultipartFormDataContent payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            LastMultipartPayload = payload;
            return Task.FromResult("transport-response");
        }

        public Task<string> SendPostRequestAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            LastDictionaryPayload = new Dictionary<string, string>(payload);
            return Task.FromResult("transport-response");
        }

        public Task<string> DownloadFileAsync(Dictionary<string, string> payload, Uri uri, string destinationPath, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            LastDictionaryPayload = new Dictionary<string, string>(payload);
            return Task.FromResult("transport-response");
        }
    }
}
