using System.Net;

using Redcap.Exceptions;
using Redcap.Models;

using Xunit;

namespace RedcapApi.Tests;

public class HttpErrorTests
{
    private const string Token = "token123";

    [Theory]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(500)]
    public async Task ExportRecordsAsync_WhenServerReturnsError_ThrowsRedcapApiException(int statusCode)
    {
        using var server = new LocalHttpServer(_ => new TestResponse(statusCode, "server-error-body"));
        var api = new Redcap.RedcapApi(server.Url.ToString());

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => api.ExportRecordsAsync(Token));

        Assert.Equal((HttpStatusCode)statusCode, ex.StatusCode);
        Assert.Equal("server-error-body", ex.ResponseBody);
    }

    [Theory]
    [InlineData(400)]
    [InlineData(500)]
    public async Task ImportRecordsAsync_WhenServerReturnsError_ThrowsRedcapApiException(int statusCode)
    {
        using var server = new LocalHttpServer(_ => new TestResponse(statusCode, "import-error-body"));
        var api = new Redcap.RedcapApi(server.Url.ToString());
        var data = new List<object> { new { record_id = "1" } };

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
            api.ImportRecordsAsync(Token, RedcapFormat.json, RedcapDataType.flat, OverwriteBehavior.normal, false, false, data));

        Assert.Equal((HttpStatusCode)statusCode, ex.StatusCode);
        Assert.Equal("import-error-body", ex.ResponseBody);
    }

    [Theory]
    [InlineData(401)]
    [InlineData(500)]
    public async Task ExportUsersAsync_WhenServerReturnsError_ThrowsRedcapApiException(int statusCode)
    {
        using var server = new LocalHttpServer(_ => new TestResponse(statusCode, "users-error-body"));
        var api = new Redcap.RedcapApi(server.Url.ToString());

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => api.ExportUsersAsync(Token));

        Assert.Equal((HttpStatusCode)statusCode, ex.StatusCode);
        Assert.Equal("users-error-body", ex.ResponseBody);
    }
}
