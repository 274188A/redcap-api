using Redcap.Models;
using Xunit;

namespace RedcapApi.Tests;

// The DefaultRedcapTransport swallows HTTP errors: non-2xx responses return the
// server's response body as a string rather than throwing. These tests document
// that contract so a future change to the transport behaviour is caught.
public class HttpErrorTests
{
    private const string Token = "token123";

    [Theory]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(500)]
    public async Task ExportRecordsAsync_WhenServerReturnsError_ReturnsResponseBody(int statusCode)
    {
        using var server = new LocalHttpServer(_ => new TestResponse(statusCode, "server-error-body"));
        var api = new Redcap.RedcapApi(server.Url.ToString());

        var result = await api.ExportRecordsAsync(Token);

        Assert.Equal("server-error-body", result);
    }

    [Theory]
    [InlineData(400)]
    [InlineData(500)]
    public async Task ImportRecordsAsync_WhenServerReturnsError_ReturnsResponseBody(int statusCode)
    {
        using var server = new LocalHttpServer(_ => new TestResponse(statusCode, "import-error-body"));
        var api = new Redcap.RedcapApi(server.Url.ToString());
        var data = new List<object> { new { record_id = "1" } };

        var result = await api.ImportRecordsAsync(Token, RedcapFormat.json, RedcapDataType.flat, OverwriteBehavior.normal, false, false, data);

        Assert.Equal("import-error-body", result);
    }

    [Theory]
    [InlineData(401)]
    [InlineData(500)]
    public async Task ExportUsersAsync_WhenServerReturnsError_ReturnsResponseBody(int statusCode)
    {
        using var server = new LocalHttpServer(_ => new TestResponse(statusCode, "users-error-body"));
        var api = new Redcap.RedcapApi(server.Url.ToString());

        var result = await api.ExportUsersAsync(Token);

        Assert.Equal("users-error-body", result);
    }
}
