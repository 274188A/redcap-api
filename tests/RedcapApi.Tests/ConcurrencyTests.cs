using System.Globalization;
using System.Net;

using Redcap.Api;

using Xunit;

namespace RedcapApi.Tests;

public class ConcurrencyTests
{
    [Fact]
    public async Task SharedApiInstance_HandlesParallelCallsWithoutDataMixing()
    {
        using var server = new LocalHttpServer(request =>
        {
            var form = ParseFormBody(request.Body);
            var record = form["record"];

            Thread.Sleep(TimeSpan.FromMilliseconds(int.Parse(record, CultureInfo.InvariantCulture) % 5));

            return new TestResponse(200, record);
        });
        using var transport = new DefaultRedcapTransport(timeOutSeconds: 10);
        var api = new Redcap.RedcapApi(server.Url.ToString(), TestConstants.Token, transport);
        var recordIds = Enumerable.Range(1, 32)
            .Select(id => id.ToString(CultureInfo.InvariantCulture))
            .ToArray();

        var results = await Task.WhenAll(recordIds.Select(recordId =>
            api.ExportSurveyQueueLinkAsync(recordId)));

        Assert.Equal(recordIds, results);
        Assert.Equal(recordIds.Length, server.Requests.Count);

        var requestedRecords = server.Requests
            .Select(request => ParseFormBody(request.Body))
            .Select(form => (Content: form["content"], Record: form["record"], Token: form["token"]))
            .OrderBy(item => int.Parse(item.Record, CultureInfo.InvariantCulture))
            .ToArray();

        Assert.All(requestedRecords, request =>
        {
            Assert.Equal("surveyQueueLink", request.Content);
            Assert.Equal(TestConstants.Token, request.Token);
        });
        Assert.Equal(recordIds, requestedRecords.Select(item => item.Record).ToArray());
    }

    private static Dictionary<string, string> ParseFormBody(string body)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = part.IndexOf('=');
            var key = separatorIndex >= 0 ? part[..separatorIndex] : part;
            var value = separatorIndex >= 0 ? part[(separatorIndex + 1)..] : string.Empty;

            fields[WebUtility.UrlDecode(key)] = WebUtility.UrlDecode(value);
        }

        return fields;
    }
}
