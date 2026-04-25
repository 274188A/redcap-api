using Redcap.Exceptions;
using Redcap.Http;
using Redcap.Models;
using Redcap.Utilities;
using System.Net;
using System.Text;
using Xunit;

namespace RedcapApi.Tests;

public class UtilitiesTests
{
    private const string Token = "token123";
    private readonly Redcap.RedcapApi _api = new("http://localhost/", Token);

    [Fact]
    public void GetDisplayName_ReturnsDisplayAttributeName()
    {
        Assert.Equal("project_xml", Content.ProjectXml.GetDisplayName());
        Assert.Equal("odm", RedcapFormat.odm.GetDisplayName());
    }

    [Fact]
    public void IsNullOrEmpty_HandlesNullEmptyAndPopulatedArrays()
    {
        string[]? nullArray = null;
        var emptyArray = Array.Empty<string>();
        var values = new[] { "value" };

        Assert.True(nullArray.IsNullOrEmpty());
        Assert.True(emptyArray.IsNullOrEmpty());
        Assert.False(values.IsNullOrEmpty());
    }

    [Fact]
    public void ConvertArrayToString_FormatsValues()
    {
        var single = _api.ConvertArraytoString(new[] { "one" });
        var multiple = _api.ConvertArraytoString(new[] { "one", "two", "three" });

        Assert.Equal("one", single);
        Assert.Equal("one,two,three", multiple);
        Assert.Throws<ArgumentException>(() => _api.ConvertArraytoString<string>(null!));
        Assert.Throws<ArgumentException>(() => _api.ConvertArraytoString(Array.Empty<string>()));
    }

    [Fact]
    public void ConvertIntArrayToString_FormatsValues()
    {
        var single = _api.ConvertIntArraytoString(new[] { 1 });
        var multiple = _api.ConvertIntArraytoString(new[] { 1, 2, 3 });

        Assert.Equal("1", single);
        Assert.Equal("1,2,3", multiple);
        Assert.Throws<ArgumentException>(() => _api.ConvertIntArraytoString(null!));
        Assert.Throws<ArgumentException>(() => _api.ConvertIntArraytoString(Array.Empty<int>()));
    }

    [Fact]
    public void HandleReturnContent_ReturnsExpectedValues()
    {
        Assert.Equal("ids", _api.HandleReturnContent(ReturnContent.ids));
        Assert.Equal("count", _api.HandleReturnContent(ReturnContent.count));
        Assert.Equal("count", _api.HandleReturnContent((ReturnContent)999));
    }

    [Fact]
    public void HandleFormat_ReturnsExpectedTuple()
    {
        var result = _api.HandleFormat(RedcapFormat.odm, RedcapReturnFormat.xml, RedcapDataType.longitudinal);
        var fallback = _api.HandleFormat((RedcapFormat)999, (RedcapReturnFormat)999, (RedcapDataType)999);

        Assert.Equal("odm", result.format);
        Assert.Equal("xml", result.onErrorFormat);
        Assert.Equal("longitudinal", result.redcapDataType);

        Assert.Equal("json", fallback.format);
        Assert.Equal("json", fallback.onErrorFormat);
        Assert.Equal("flat", fallback.redcapDataType);
    }

    [Fact]
    public void ExtractBehavior_ReturnsExpectedValue()
    {
        Assert.Equal("normal", _api.ExtractBehavior(OverwriteBehavior.normal));
        Assert.Equal("overwrite", _api.ExtractBehavior(OverwriteBehavior.overwrite));
        Assert.Equal("overwrite", _api.ExtractBehavior((OverwriteBehavior)999));
    }

    [Fact]
    public void ExtractHelpers_SplitDelimitedValues()
    {
        var delimiters = new[] { ',', ';' };

        Assert.Equal(new[] { "event_1", "event_2" }, _api.ExtractEvents("event_1,event_2", delimiters));
        Assert.Equal(new[] { "field_1", "field_2" }, _api.ExtractFields("field_1;field_2", delimiters));
        Assert.Equal(new[] { "1", "2" }, _api.ExtractRecords("1,2", delimiters));
        Assert.Equal(new[] { "form_1", "form_2" }, _api.ExtractForms("form_1;form_2", delimiters));
        Assert.Equal(new[] { "1", "2" }, _api.ExtractArms<string>("1,2", delimiters));
    }

    [Fact]
    public void CheckToken_ThrowsForMissingToken()
    {
        Assert.Throws<ArgumentException>(() => _api.CheckToken(string.Empty));
        Assert.Throws<ArgumentNullException>(() => _api.CheckToken(null!));
        var ex = Xunit.Record.Exception(() => _api.CheckToken("token"));
        Assert.Null(ex);
    }

    [Fact]
    public void DefaultRedcapTransport_AcceptsCustomHandler()
    {
        // Callers needing custom TLS settings must inject a handler via DefaultRedcapTransport.
        // Verify construction succeeds — the handler is stored and forwarded to the Utils helpers.
        var customHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        var transport = new Redcap.Api.DefaultRedcapTransport(customHandler);
        Assert.NotNull(transport);
    }

    [Fact]
    public async Task CustomFormUrlEncodedContent_EncodesKeysAndValues()
    {
        using var content = new CustomFormUrlEncodedContent(new Dictionary<string, string>
        {
            ["a key"] = "value with spaces",
            ["sym&bol"] = "1+2"
        });

        var body = await content.ReadAsStringAsync();

        Assert.Equal("a+key=value+with+spaces&sym%26bol=1%2B2", body);
    }

    [Fact]
    public async Task ReadAsFileAsync_WritesContentToDisk()
    {
        using var content = new StringContent("hello world", Encoding.UTF8, "text/plain");
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            await content.ReadAsFileAsync("sample", tempDirectory, overwrite: true, fileExtension: "txt");
            var filePath = Path.Combine(tempDirectory, "sample.txt");

            Assert.True(File.Exists(filePath));
            Assert.Equal("hello world", await File.ReadAllTextAsync(filePath));
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task GetStreamContentAsync_ReturnsResponseStream()
    {
        using var server = new LocalHttpServer(_ => new TestResponse(200, "stream-body"));
        using var client = new HttpClient();
        using var stream = await Utils.GetStreamContentAsync(new Dictionary<string, string> { ["token"] = "abc" }, server.Url, client);

        Assert.NotNull(stream);
        Assert.False(stream!.CanRead);
    }

    [Fact]
    public async Task SendPostRequestAsync_WithDictionary_ReturnsResponseBody()
    {
        using var server = new LocalHttpServer(_ => new TestResponse(200, "ok"));
        using var client = new HttpClient();
        var payload = new Dictionary<string, string>
        {
            ["token"] = "abc",
            ["content"] = "record"
        };

        var response = await Utils.SendPostRequestAsync(payload, server.Url, client);

        Assert.Equal("ok", response);
        Assert.True(server.Requests.TryPeek(out var request));
        Assert.Contains("token=abc", request!.Body);
        Assert.Contains("content=record", request.Body);
    }

    [Theory]
    [InlineData(400)]
    [InlineData(500)]
    public async Task SendPostRequestAsync_WithDictionary_ThrowsRedcapApiExceptionOnError(int statusCode)
    {
        using var server = new LocalHttpServer(_ => new TestResponse(statusCode, "error-body"));
        using var client = new HttpClient();

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
            Utils.SendPostRequestAsync(new Dictionary<string, string> { ["token"] = "abc" }, server.Url, client));

        Assert.Equal((HttpStatusCode)statusCode, ex.StatusCode);
        Assert.Equal("error-body", ex.ResponseBody);
    }

    [Theory]
    [InlineData(401)]
    [InlineData(500)]
    public async Task GetStreamContentAsync_ThrowsRedcapApiExceptionOnError(int statusCode)
    {
        using var server = new LocalHttpServer(_ => new TestResponse(statusCode, "stream-error-body"));
        using var client = new HttpClient();

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
            Utils.GetStreamContentAsync(new Dictionary<string, string> { ["token"] = "abc" }, server.Url, client));

        Assert.Equal((HttpStatusCode)statusCode, ex.StatusCode);
        Assert.Equal("stream-error-body", ex.ResponseBody);
    }

    [Fact]
    public async Task ExportSurveyAccessCodeAsync_SendsExpectedPayload()
    {
        using var server = new LocalHttpServer(_ => new TestResponse(200, "access-code"));
        var api = new Redcap.RedcapApi(server.Url.ToString(), Token);

        var response = await api.ExportSurveyAccessCodeAsync("1", "survey_form", "event_1", 2);

        Assert.Equal("access-code", response);
        Assert.True(server.Requests.TryPeek(out var request));
        Assert.Contains("content=surveyAccessCode", request!.Body);
        Assert.Contains("record=1", request.Body);
        Assert.Contains("instrument=survey_form", request.Body);
        Assert.Contains("event=event_1", request.Body);
        Assert.Contains("repeat_instance=2", request.Body);
    }

    [Fact]
    public async Task DeleteRecordsAsync_IncludesDeleteLoggingFlagWhenEnabled()
    {
        using var server = new LocalHttpServer(_ => new TestResponse(200, "1"));
        var api = new Redcap.RedcapApi(server.Url.ToString(), Token);

        var response = await api.DeleteRecordsAsync(new[] { "1", "2" }, 1, deleteLogging: true);

        Assert.Equal("1", response);
        Assert.True(server.Requests.TryPeek(out var request));
        Assert.Contains("content=record", request!.Body);
        Assert.Contains("action=delete", request.Body);
        Assert.Contains("records%5B0%5D=1", request.Body);
        Assert.Contains("records%5B1%5D=2", request.Body);
        Assert.Contains("delete_logging=True", request.Body);
    }

    [Fact]
    public async Task RandomizeRecord_SendsExpectedPayload()
    {
        using var server = new LocalHttpServer(_ => new TestResponse(200, "group-a"));
        var api = new Redcap.RedcapApi(server.Url.ToString(), Token);

        var response = await api.RandomizeRecordAsync("1", "99", RedcapFormat.json, returnAlt: true);

        Assert.Equal("group-a", response);
        Assert.True(server.Requests.TryPeek(out var request));
        Assert.Contains("action=randomize", request!.Body);
        Assert.Contains("content=record", request.Body);
        Assert.Contains("randomization_id=99", request.Body);
        Assert.Contains("returnAlt=True", request.Body);
    }

    [Fact]
    public async Task ReadAsFileAsync_WritesWithinTargetDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "redcap-readasfile-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            using var content = new StringContent("payload", Encoding.UTF8);
            await content.ReadAsFileAsync("report", tempDir, overwrite: true, fileExtension: "txt");
            var expected = Path.Combine(tempDir, "report.txt");
            Assert.True(File.Exists(expected));
            Assert.Equal("payload", await File.ReadAllTextAsync(expected));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Theory]
    [InlineData("..\\..\\escape")]
    [InlineData("../../escape")]
    [InlineData("subdir/leaf")]
    public async Task ReadAsFileAsync_RejectsPathTraversal(string fileName)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "redcap-readasfile-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            using var content = new StringContent("payload", Encoding.UTF8);
            // Either the filename is stripped down to a safe leaf, or the attempt is rejected outright.
            try
            {
                await content.ReadAsFileAsync(fileName, tempDir, overwrite: true, fileExtension: "txt");
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return;
            }

            // If the call succeeded, the written file MUST live under tempDir.
            var written = Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories);
            Assert.NotEmpty(written);
            foreach (var file in written)
            {
                Assert.StartsWith(Path.GetFullPath(tempDir), Path.GetFullPath(file), StringComparison.Ordinal);
            }
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task ReadAsFileAsync_WhenOverwriteFalse_ThrowsAndPreservesExistingFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "redcap-overwrite-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var filePath = Path.Combine(tempDir, "output.txt");
            await File.WriteAllTextAsync(filePath, "original");

            using var newContent = new StringContent("replacement", Encoding.UTF8, "text/plain");
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                newContent.ReadAsFileAsync("output", tempDir, overwrite: false, fileExtension: "txt"));

            Assert.Equal("original", await File.ReadAllTextAsync(filePath));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

}
