using Redcap.Exceptions;
using Redcap.Interfaces;
using Redcap.Models;
using Xunit;

namespace RedcapApi.Tests;

public class RedcapApiTransportTests
{
    private const string Token = "token123";
    [Fact]
    public async Task ExportDagsAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportDagsAsync(RedcapFormat.csv, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("dag", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
            Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        }
    }

    [Fact]
    public async Task ExportDagsTypedAsync_UsesJsonPayloadAndDeserializesResponse()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "[{\"data_access_group_name\":\"CA Site\",\"unique_group_name\":\"ca_site\"},{\"data_access_group_name\":\"FL Site\",\"unique_group_name\":\"fl_site\"}]"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var result = await api.ExportDagsTypedAsync(RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("dag", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Equal(2, result.Count);
        Assert.Equal("CA Site", result[0].GroupName);
        Assert.Equal("ca_site", result[0].UniqueGroupName);
        Assert.Equal("FL Site", result[1].GroupName);
    }

    [Fact]
    public async Task ExportDagsTypedAsync_WithInvalidJson_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "not-json"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => api.ExportDagsTypedAsync());

        Assert.Equal("Failed to deserialize REDCap DAG response.", ex.Message);
    }

    [Fact]
    public async Task ImportDagsAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<RedcapDag> { new() { GroupName = "CA Site", UniqueGroupName = "ca_site" } };

        await api.ImportDagsAsync(RedcapFormat.json, data, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("dag", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("import", transport.LastDictionaryPayload["action"]);
            Assert.Equal("json", transport.LastDictionaryPayload["format"]);
            Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
            Assert.Contains("ca_site", transport.LastDictionaryPayload["data"]);
        }
    }

    [Fact]
    public async Task DeleteDagsAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.DeleteDagsAsync(new[] { "ca_site", "fl_site" });

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("dag", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("delete", transport.LastDictionaryPayload["action"]);
            Assert.Equal("ca_site", transport.LastDictionaryPayload["dags[0]"]);
            Assert.Equal("fl_site", transport.LastDictionaryPayload["dags[1]"]);
        }
    }

    [Fact]
    public async Task DeleteDagsAsync_WithNoItems_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
            api.DeleteDagsAsync(Array.Empty<string>()));

        Assert.Contains("No dags to delete", ex.Message);
        Assert.Null(transport.LastDictionaryPayload);
    }

    [Fact]
    public async Task SwitchDagAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.SwitchDagAsync(new RedcapDag { UniqueGroupName = "ca_site" });

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("ca_site", transport.LastDictionaryPayload!["dag"]);
            Assert.Equal("dag", transport.LastDictionaryPayload["content"]);
            Assert.Equal("switch", transport.LastDictionaryPayload["action"]);
        }
    }

    [Fact]
    public async Task ExportUserDagAssignmentAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportUserDagAssignmentAsync(RedcapFormat.csv, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("userDagMapping", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
            Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        }
    }

    [Fact]
    public async Task ExportUserDagAssignmentTypedAsync_UsesJsonPayloadAndDeserializesResponse()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "[{\"username\":\"alice\",\"redcap_data_access_group\":\"ca_site\"},{\"username\":\"bob\",\"redcap_data_access_group\":\"fl_site\"}]"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var result = await api.ExportUserDagAssignmentTypedAsync(RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("userDagMapping", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Equal(2, result.Count);
        Assert.Equal("alice", result[0].Username);
        Assert.Equal("ca_site", result[0].RedcapDataAccessGroup);
        Assert.Equal("bob", result[1].Username);
        Assert.Equal("fl_site", result[1].RedcapDataAccessGroup);
    }

    [Fact]
    public async Task ExportUserDagAssignmentTypedAsync_WithInvalidJson_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "not-json"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => api.ExportUserDagAssignmentTypedAsync());

        Assert.Equal("Failed to deserialize REDCap user-DAG assignment response.", ex.Message);
    }

    [Fact]
    public async Task ImportUserDagAssignmentAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<TestUserDagAssignment> { new() { Username = "alice", RedcapDataAccessGroup = "ca_site" } };

        await api.ImportUserDagAssignmentAsync(RedcapFormat.json, data, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("userDagMapping", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("import", transport.LastDictionaryPayload["action"]);
            Assert.Equal("json", transport.LastDictionaryPayload["format"]);
            Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
            Assert.Contains("ca_site", transport.LastDictionaryPayload["data"]);
        }
    }

    [Fact]
    public async Task ImportUserDagAssignmentAsync_WithNoData_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
            api.ImportUserDagAssignmentAsync(RedcapFormat.json, new List<TestUserDagAssignment>()));

        Assert.Contains("No data to import", ex.Message);
        Assert.Null(transport.LastDictionaryPayload);
    }

    [Fact]
    public async Task GenerateNextRecordNameAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.GenerateNextRecordNameAsync();

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal(Token, transport.LastDictionaryPayload!["token"]);
        Assert.Equal("generateNextRecordName", transport.LastDictionaryPayload["content"]);
    }

    [Fact]
    public async Task ExportRecordsAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportRecordsAsync(
            format: RedcapFormat.csv,
            redcapDataType: RedcapDataType.flat,
            records: new[] { "1", "2" },
            fields: new[] { "field_1", "field_2" },
            forms: new[] { "form_1" },
            events: new[] { "event_1_arm_1" },
            rawOrLabel: RawOrLabel.label,
            rawOrLabelHeaders: RawOrLabelHeaders.label,
            exportCheckboxLabel: true,
            exportSurveyFields: true,
            exportDataAccessGroups: true,
            filterLogic: "[age] > 30",
            dateRangeBegin: new DateTime(2024, 1, 2, 3, 4, 5),
            dateRangeEnd: new DateTime(2024, 1, 3, 3, 4, 5),
            csvDelimiter: CsvDelimiter.comma,
            decimalCharacter: DecimalCharacter.dot,
            exportBlankForGrayFormStatus: true,
            combineCheckboxOptions: true);

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("record", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
            Assert.Equal("flat", transport.LastDictionaryPayload["type"]);
            Assert.Equal("1,2", transport.LastDictionaryPayload["records"]);
            Assert.Equal("field_1,field_2", transport.LastDictionaryPayload["fields"]);
            Assert.Equal("form_1", transport.LastDictionaryPayload["forms"]);
            Assert.Equal("event_1_arm_1", transport.LastDictionaryPayload["events"]);
            Assert.Equal("label", transport.LastDictionaryPayload["rawOrLabel"]);
            Assert.Equal("label", transport.LastDictionaryPayload["rawOrLabelHeaders"]);
            Assert.Equal("True", transport.LastDictionaryPayload["exportCheckboxLabel"]);
            Assert.Equal("True", transport.LastDictionaryPayload["exportSurveyFields"]);
            Assert.Equal("True", transport.LastDictionaryPayload["exportDataAccessGroups"]);
            Assert.Equal("[age] > 30", transport.LastDictionaryPayload["filterLogic"]);
            Assert.Equal("comma", transport.LastDictionaryPayload["csvDelimiter"]);
            Assert.Equal("dot", transport.LastDictionaryPayload["decimalCharacter"]);
            Assert.Equal("True", transport.LastDictionaryPayload["exportBlankForGrayFormStatus"]);
            Assert.Equal("True", transport.LastDictionaryPayload["combineCheckboxOptions"]);
        }
    }

    [Fact]
    public async Task ExportRecordsAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportRecordsAsync(records: new[] { "5" }, rawOrLabelHeaders: RawOrLabelHeaders.label);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("record", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("5", transport.LastDictionaryPayload["records"]);
        Assert.Equal("label", transport.LastDictionaryPayload["rawOrLabelHeaders"]);
    }

    [Fact]
    public async Task ExportRecordsAsync_DefaultOverload_JsonFormat_OmitsCsvSpecificOptionalKeys()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportRecordsAsync(
            format: RedcapFormat.json,
            redcapDataType: RedcapDataType.flat,
            records: null,
            fields: null,
            forms: null,
            events: null,
            decimalCharacter: DecimalCharacter.none,
            combineCheckboxOptions: false,
            exportSurveyFields: false,
            exportDataAccessGroups: false,
            exportCheckboxLabel: false);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.False(transport.LastDictionaryPayload!.ContainsKey("csvDelimiter"));
        Assert.False(transport.LastDictionaryPayload.ContainsKey("decimalCharacter"));
        Assert.False(transport.LastDictionaryPayload.ContainsKey("combineCheckboxOptions"));
        Assert.False(transport.LastDictionaryPayload.ContainsKey("records"));
        Assert.False(transport.LastDictionaryPayload.ContainsKey("fields"));
    }

    [Fact]
    public async Task ExportRecordAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportRecordAsync(
            record: "7",
            format: RedcapFormat.csv,
            redcapDataType: RedcapDataType.flat,
            fields: new[] { "field_a" },
            forms: new[] { "form_a" },
            events: new[] { "event_a" },
            rawOrLabel: RawOrLabel.label,
            rawOrLabelHeaders: RawOrLabelHeaders.label,
            exportCheckboxLabel: true,
            onErrorFormat: RedcapReturnFormat.xml,
            exportSurveyFields: true,
            exportDataAccessGroups: true,
            filterLogic: "[id] = 7",
            dateRangeBegin: new DateTime(2024, 2, 1, 1, 1, 1),
            dateRangeEnd: new DateTime(2024, 2, 2, 1, 1, 1),
            csvDelimiter: CsvDelimiter.comma,
            decimalCharacter: DecimalCharacter.dot,
            exportBlankForGrayFormStatus: true,
            combineCheckboxOptions: true);

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("record", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
            Assert.Equal("flat", transport.LastDictionaryPayload["type"]);
            Assert.Equal("7", transport.LastDictionaryPayload["records"]);
            Assert.Equal("field_a", transport.LastDictionaryPayload["fields"]);
            Assert.Equal("form_a", transport.LastDictionaryPayload["forms"]);
            Assert.Equal("event_a", transport.LastDictionaryPayload["events"]);
            Assert.Equal("label", transport.LastDictionaryPayload["rawOrLabel"]);
            Assert.Equal("label", transport.LastDictionaryPayload["rawOrLabelHeaders"]);
            Assert.Equal("True", transport.LastDictionaryPayload["exportCheckboxLabel"]);
            Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
            Assert.Equal("True", transport.LastDictionaryPayload["exportSurveyFields"]);
            Assert.Equal("True", transport.LastDictionaryPayload["exportDataAccessGroups"]);
            Assert.Equal("[id] = 7", transport.LastDictionaryPayload["filterLogic"]);
            Assert.Equal("comma", transport.LastDictionaryPayload["csvDelimiter"]);
            Assert.Equal("dot", transport.LastDictionaryPayload["decimalCharacter"]);
            Assert.Equal("True", transport.LastDictionaryPayload["exportBlankForGrayFormStatus"]);
            Assert.Equal("True", transport.LastDictionaryPayload["combineCheckboxOptions"]);
        }
    }

    [Fact]
    public async Task ImportRecordsAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<TestRecordPayload> { new() { RecordId = "1", FirstName = "Alice" } };

        await api.ImportRecordsAsync(RedcapFormat.json, RedcapDataType.flat, OverwriteBehavior.overwrite, true, true, data, dateFormat: "YMD", csvDelimiter: CsvDelimiter.comma, returnContent: ReturnContent.ids);

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("record", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("overwrite", transport.LastDictionaryPayload["overwriteBehavior"]);
            Assert.Equal("True", transport.LastDictionaryPayload["forceAutoNumber"]);
            Assert.Equal("True", transport.LastDictionaryPayload["backgroundProcess"]);
            Assert.Equal("YMD", transport.LastDictionaryPayload["dateFormat"]);
            Assert.Equal("ids", transport.LastDictionaryPayload["returnContent"]);
            Assert.Contains("Alice", transport.LastDictionaryPayload["data"]);
        }
    }

    [Fact]
    public async Task ImportRecordsAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<TestRecordPayload> { new() { RecordId = "2", FirstName = "Bob" } };

        await api.ImportRecordsAsync(RedcapFormat.json, RedcapDataType.flat, OverwriteBehavior.normal, false, false, data);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("record", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("flat", transport.LastDictionaryPayload["type"]);
        Assert.Equal("normal", transport.LastDictionaryPayload["overwriteBehavior"]);
        Assert.Equal("False", transport.LastDictionaryPayload["forceAutoNumber"]);
        Assert.Equal("False", transport.LastDictionaryPayload["backgroundProcess"]);
        Assert.Contains("Bob", transport.LastDictionaryPayload["data"]);
    }

    [Fact]
    public async Task ExportSurveyAccessCodeAsync_UsesInjectedTransport()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var result = await api.ExportSurveyAccessCodeAsync("1", "survey_form", "event_1", 2);

        Assert.Equal("transport-response", result);
        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("surveyAccessCode", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("1", transport.LastDictionaryPayload["record"]);
        Assert.Equal("survey_form", transport.LastDictionaryPayload["instrument"]);
        Assert.Equal("event_1", transport.LastDictionaryPayload["event"]);
        Assert.Equal("2", transport.LastDictionaryPayload["repeat_instance"]);
    }

    [Fact]
    public async Task DeleteRecordsAsync_UsesInjectedTransportAndIncludesDeleteLogging()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.DeleteRecordsAsync(new[] { "10", "11" }, 1, deleteLogging: true);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("record", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("delete", transport.LastDictionaryPayload["action"]);
        Assert.Equal("10", transport.LastDictionaryPayload["records[0]"]);
        Assert.Equal("11", transport.LastDictionaryPayload["records[1]"]);
        Assert.Equal("True", transport.LastDictionaryPayload["delete_logging"]);
    }

    [Fact]
    public async Task DeleteRecordsAsync_DetailedOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.DeleteRecordsAsync(
            Content.Record,
            RedcapAction.Delete,
            new[] { "1" },
            2,
            new RedcapInstrument { InstrumentName = "demographics" },
            new RedcapEvent { EventName = "event_1_arm_1" },
            new RedcapRepeatInstance { RepeatInstance = 3 },
            deleteLogging: true);

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("record", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("delete", transport.LastDictionaryPayload["action"]);
            Assert.Equal("1", transport.LastDictionaryPayload["records[0]"]);
            Assert.Equal("2", transport.LastDictionaryPayload["arm"]);
            Assert.Equal("demographics", transport.LastDictionaryPayload["instrument"]);
            Assert.Equal("event_1_arm_1", transport.LastDictionaryPayload["event"]);
            Assert.Equal("3", transport.LastDictionaryPayload["repeat_instance"]);
            Assert.Equal("True", transport.LastDictionaryPayload["delete_logging"]);
        }
    }

    [Fact]
    public async Task DeleteRecordsAsync_ContentArmOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.DeleteRecordsAsync(new[] { "1", "2" }, 2, deleteLogging: true);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("record", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("delete", transport.LastDictionaryPayload["action"]);
        Assert.Equal("2", transport.LastDictionaryPayload["arm"]);
        Assert.Equal("True", transport.LastDictionaryPayload["delete_logging"]);
    }

    [Fact]
    public async Task DeleteRecordsAsync_WithNoRecords_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
            api.DeleteRecordsAsync(Array.Empty<string>(), 1));

        Assert.Contains("Please provide the records", ex.Message);
        Assert.Null(transport.LastDictionaryPayload);
    }

    [Fact]
    public async Task RenameRecordAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.RenameRecordAsync("old-id", "new-id", 1);

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("record", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("rename", transport.LastDictionaryPayload["action"]);
            Assert.Equal("old-id", transport.LastDictionaryPayload["record"]);
            Assert.Equal("new-id", transport.LastDictionaryPayload["new_record_name"]);
            Assert.Equal("1", transport.LastDictionaryPayload["arm"]);
        }
    }

    [Fact]
    public async Task RandomizeRecord_UsesInjectedTransport()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

#pragma warning disable CS0618
        await api.RandomizeRecord("55", "7", RedcapFormat.json, returnAlt: true);
#pragma warning restore CS0618

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("randomize", transport.LastDictionaryPayload!["action"]);
        Assert.Equal("record", transport.LastDictionaryPayload["content"]);
        Assert.Equal("55", transport.LastDictionaryPayload["record"]);
        Assert.Equal("7", transport.LastDictionaryPayload["randomization_id"]);
        Assert.Equal("True", transport.LastDictionaryPayload["returnAlt"]);
    }

    [Fact]
    public async Task RandomizeRecordAsync_UsesInjectedTransport()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.RandomizeRecordAsync("55", "7", RedcapFormat.json, returnAlt: true);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("randomize", transport.LastDictionaryPayload!["action"]);
        Assert.Equal("record", transport.LastDictionaryPayload["content"]);
        Assert.Equal("55", transport.LastDictionaryPayload["record"]);
        Assert.Equal("7", transport.LastDictionaryPayload["randomization_id"]);
        Assert.Equal("True", transport.LastDictionaryPayload["returnAlt"]);
    }

    [Fact]
    public async Task ExportRepeatingInstrumentsAndEvents_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

#pragma warning disable CS0618
        await api.ExportRepeatingInstrumentsAndEvents(RedcapFormat.odm);
#pragma warning restore CS0618

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("repeatingFormsEvents", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("odm", transport.LastDictionaryPayload["format"]);
    }

    [Fact]
    public async Task ExportRepeatingInstrumentsAndEventsAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportRepeatingInstrumentsAndEventsAsync(RedcapFormat.odm);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("repeatingFormsEvents", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("odm", transport.LastDictionaryPayload["format"]);
    }

    [Fact]
    public async Task ExportRepeatingInstrumentsAndEventsTypedAsync_UsesJsonPayloadAndDeserializesResponse()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "[{\"event_name\":\"Visit 1\",\"unique_event_name\":\"event_1_arm_1\",\"form_name\":\"demographics\",\"custom_form_label\":\"Visit Label\"},{\"event_name\":\"Visit 2\",\"unique_event_name\":\"event_2_arm_1\",\"form_name\":null,\"custom_form_label\":\"Repeat Event\"}]"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var result = await api.ExportRepeatingInstrumentsAndEventsTypedAsync();

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("repeatingFormsEvents", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal(2, result.Count);
        Assert.Equal("event_1_arm_1", result[0].UniqueEventName);
        Assert.Equal("demographics", result[0].FormName);
        Assert.Equal("Visit Label", result[0].CustomFormLabel);
        Assert.Equal("event_2_arm_1", result[1].UniqueEventName);
        Assert.Null(result[1].FormName);
        Assert.Equal("Repeat Event", result[1].CustomFormLabel);
    }

    [Fact]
    public async Task ExportRepeatingInstrumentsAndEventsTypedAsync_WithInvalidJson_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "not json"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => api.ExportRepeatingInstrumentsAndEventsTypedAsync());

        Assert.Equal("Failed to deserialize REDCap repeating instruments response.", ex.Message);
    }

    [Fact]
    public async Task ExportRepeatingInstrumentsAndEvents_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

#pragma warning disable CS0618
        await api.ExportRepeatingInstrumentsAndEvents(RedcapFormat.json);
#pragma warning restore CS0618

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("repeatingFormsEvents", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
    }

    [Fact]
    public async Task ImportRepeatingInstrumentsAndEvents_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new[] { new { instrument_name = "form_a", event_name = "event_1_arm_1" } }.ToList();

#pragma warning disable CS0618
        await api.ImportRepeatingInstrumentsAndEvents(data, format: RedcapFormat.json, returnFormat: RedcapReturnFormat.xml);
#pragma warning restore CS0618

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("repeatingFormsEvents", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Contains("form_a", transport.LastDictionaryPayload["data"]);
    }

    [Fact]
    public async Task ImportRepeatingInstrumentsAndEventsAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new[] { new { instrument_name = "form_a", event_name = "event_1_arm_1" } }.ToList();

        await api.ImportRepeatingInstrumentsAndEventsAsync(data, format: RedcapFormat.json, returnFormat: RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("repeatingFormsEvents", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Contains("form_a", transport.LastDictionaryPayload["data"]);
    }

    [Fact]
    public async Task ExportReports_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportReportsAsync(4, RedcapFormat.csv, RedcapReturnFormat.xml, RawOrLabel.label, RawOrLabelHeaders.label, exportCheckboxLabel: true, csvDelimiter: ",", decimalCharacter: ".");

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("report", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("4", transport.LastDictionaryPayload["report_id"]);
            Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
            Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
            Assert.Equal("label", transport.LastDictionaryPayload["rawOrLabel"]);
            Assert.Equal("label", transport.LastDictionaryPayload["rawOrLabelHeaders"]);
            Assert.Equal("True", transport.LastDictionaryPayload["exportCheckboxLabel"]);
            Assert.Equal(",", transport.LastDictionaryPayload["csvDelimiter"]);
            Assert.Equal(".", transport.LastDictionaryPayload["decimalCharacter"]);
        }
    }

    [Fact]
    public async Task ExportReports_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportReportsAsync(5);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("report", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("5", transport.LastDictionaryPayload["report_id"]);
    }

    [Fact]
    public async Task ExportReports_ContentOverload_WithDefaults_OmitsOptionalKeys()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportReportsAsync(7, RedcapFormat.json, RedcapReturnFormat.json, RawOrLabel.raw, RawOrLabelHeaders.raw, exportCheckboxLabel: false, csvDelimiter: null, decimalCharacter: null);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("report", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("7", transport.LastDictionaryPayload["report_id"]);
        Assert.False(transport.LastDictionaryPayload.ContainsKey("exportCheckboxLabel"));
        Assert.False(transport.LastDictionaryPayload.ContainsKey("csvDelimiter"));
        Assert.False(transport.LastDictionaryPayload.ContainsKey("decimalCharacter"));
    }

    [Fact]
    public async Task ExportUsersAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportUsersAsync(RedcapFormat.csv, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("user", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
            Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        }
    }

    [Fact]
    public async Task ExportUsersAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportUsersAsync(RedcapFormat.json, RedcapReturnFormat.json);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("user", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
    }

    [Fact]
    public async Task ExportUsersTypedAsync_UsesJsonPayloadAndDeserializesResponse()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "[{\"username\":\"alice\",\"email\":\"alice@example.com\",\"firstname\":\"Alice\",\"lastname\":\"Ng\",\"data_access_group\":\"ca_site\",\"forms\":{\"demographics\":\"1\"}}]"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var result = await api.ExportUsersTypedAsync(RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("user", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Single(result);
        Assert.Equal("alice", result[0].Username);
        Assert.Equal("alice@example.com", result[0].Email);
        Assert.Equal("Alice", result[0].FirstName);
        Assert.Equal("Ng", result[0].LastName);
        Assert.Equal("ca_site", result[0].DataAccessGroup);
        Assert.NotNull(result[0].Forms);
        Assert.Equal("1", result[0].Forms!["demographics"]);
    }

    [Fact]
    public async Task ExportUsersTypedAsync_WithInvalidJson_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "not-json"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => api.ExportUsersTypedAsync());

        Assert.Equal("Failed to deserialize REDCap user response.", ex.Message);
    }

    [Fact]
    public async Task ImportUsersAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<RedcapUser>
        {
            new()
            {
                Username = "alice",
                Design = "1",
                ApiExport = "1"
            }
        };

        await api.ImportUsersAsync(data, RedcapFormat.csv, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("user", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
            Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
            Assert.Contains("alice", transport.LastDictionaryPayload["data"]);
        }
    }

    [Fact]
    public async Task ImportUsersAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<RedcapUser>
        {
            new()
            {
                Username = "bob",
                Design = "1"
            }
        };

        await api.ImportUsersAsync<RedcapUser>(data);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("user", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Contains("bob", transport.LastDictionaryPayload["data"]);
    }

    [Fact]
    public async Task DeleteUsersAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.DeleteUsersAsync(new List<string> { "alice", "bob" });

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("user", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("delete", transport.LastDictionaryPayload["action"]);
            Assert.Equal("alice", transport.LastDictionaryPayload["users[0]"]);
            Assert.Equal("bob", transport.LastDictionaryPayload["users[1]"]);
        }
    }

    [Fact]
    public async Task ExportUserRolesAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportUserRolesAsync(Content.UserRole, RedcapFormat.csv, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("userRole", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
            Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        }
    }

    [Fact]
    public async Task ExportUserRolesTypedAsync_UsesJsonPayloadAndDeserializesResponse()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "[{\"unique_role_name\":\"U-123\",\"role_label\":\"Coordinator\",\"api_export\":\"1\",\"data_export\":\"2\",\"forms\":{\"demographics\":\"1\"}}]"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var result = await api.ExportUserRolesTypedAsync(Content.UserRole, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("userRole", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Single(result);
        Assert.Equal("U-123", result[0].UniqueRoleName);
        Assert.Equal("Coordinator", result[0].RoleLabel);
        Assert.Equal("1", result[0].ApiExport);
        Assert.Equal("2", result[0].DataExport);
        Assert.NotNull(result[0].Forms);
        Assert.Equal("1", result[0].Forms!["demographics"]);
    }

    [Fact]
    public async Task ExportUserRolesTypedAsync_WithInvalidJson_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "not json"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => api.ExportUserRolesTypedAsync());

        Assert.Equal("Failed to deserialize REDCap user role response.", ex.Message);
    }

    [Fact]
    public async Task ImportUserRolesAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<TestUserRole>
        {
            new()
            {
                UniqueRoleName = "U-123",
                RoleLabel = "Coordinator",
                ApiExport = "1"
            }
        };

        await api.ImportUserRolesAsync(data, Content.UserRole, RedcapFormat.csv, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal(Token, transport.LastDictionaryPayload!["token"]);
            Assert.Equal("userRole", transport.LastDictionaryPayload["content"]);
            Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
            Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
            Assert.Contains("Coordinator", transport.LastDictionaryPayload["data"]);
        }
    }

    [Fact]
    public async Task DeleteUserRolesAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.DeleteUserRolesAsync(new List<string> { "U-123", "U-456" });

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("userRole", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("delete", transport.LastDictionaryPayload["action"]);
            Assert.Equal("U-123", transport.LastDictionaryPayload["roles[0]"]);
            Assert.Equal("U-456", transport.LastDictionaryPayload["roles[1]"]);
        }
    }

    [Fact]
    public async Task ExportUserRoleAssignmentAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportUserRoleAssignmentAsync(Content.UserRoleMapping, RedcapFormat.csv, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal("userRoleMapping", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
            Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        }
    }

    [Fact]
    public async Task ExportUserRoleAssignmentTypedAsync_UsesJsonPayloadAndDeserializesResponse()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "[{\"username\":\"alice\",\"unique_role_name\":\"U-123\"},{\"username\":\"bob\",\"unique_role_name\":\"U-456\"}]"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var result = await api.ExportUserRoleAssignmentTypedAsync(Content.UserRoleMapping, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("userRoleMapping", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Equal(2, result.Count);
        Assert.Equal("alice", result[0].Username);
        Assert.Equal("U-123", result[0].UniqueRoleName);
        Assert.Equal("bob", result[1].Username);
        Assert.Equal("U-456", result[1].UniqueRoleName);
    }

    [Fact]
    public async Task ExportUserRoleAssignmentTypedAsync_WithInvalidJson_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "not json"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => api.ExportUserRoleAssignmentTypedAsync());

        Assert.Equal("Failed to deserialize REDCap user role assignment response.", ex.Message);
    }

    [Fact]
    public async Task ImportUserRoleAssignmentAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<TestUserRoleAssignment>
        {
            new()
            {
                Username = "alice",
                UniqueRoleName = "U-123"
            }
        };

        await api.ImportUserRoleAssignmentAsync(data, Content.UserRoleMapping, RedcapAction.Import, RedcapFormat.csv, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        {
            Assert.Equal(Token, transport.LastDictionaryPayload!["token"]);
            Assert.Equal("userRoleMapping", transport.LastDictionaryPayload["content"]);
            Assert.Equal("import", transport.LastDictionaryPayload["action"]);
            Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
            Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
            Assert.Contains("U-123", transport.LastDictionaryPayload["data"]);
        }
    }

    [Fact]
    public async Task ExportEventsAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportEventsAsync(RedcapFormat.csv, new[] { "1" }, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("event", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Equal("1", transport.LastDictionaryPayload["arms[0]"]);
    }

    [Fact]
    public async Task ExportEventsTypedAsync_UsesJsonPayloadAndDeserializesResponse()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "[{\"event_name\":\"Baseline\",\"arm_num\":\"1\",\"unique_event_name\":\"baseline_arm_1\"},{\"event_name\":\"Visit 1\",\"arm_num\":\"1\",\"unique_event_name\":\"visit_1_arm_1\"}]"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var result = await api.ExportEventsTypedAsync(new[] { "1" }, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("event", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Equal("1", transport.LastDictionaryPayload["arms[0]"]);
        Assert.Equal(2, result.Count);
        Assert.Equal("Baseline", result[0].EventName);
        Assert.Equal("1", result[0].ArmNumber);
        Assert.Equal("baseline_arm_1", result[0].UniqueEventName);
        Assert.Equal("Visit 1", result[1].EventName);
    }

    [Fact]
    public async Task ExportEventsTypedAsync_WithInvalidJson_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "not-json"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => api.ExportEventsTypedAsync(new[] { "1" }));

        Assert.Equal("Failed to deserialize REDCap event response.", ex.Message);
    }

    [Fact]
    public async Task ExportEventsAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportEventsAsync(RedcapFormat.csv, new[] { "1" }, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("event", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Equal("1", transport.LastDictionaryPayload["arms[0]"]);
    }

    [Fact]
    public async Task ImportEventsAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<RedcapEvent> { new() { EventName = "baseline", ArmNumber = "1" } };

        await api.ImportEventsAsync(false, RedcapFormat.json, data, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("event", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("import", transport.LastDictionaryPayload["action"]);
        Assert.Equal("false", transport.LastDictionaryPayload["override"]);
        Assert.Contains("baseline", transport.LastDictionaryPayload["data"]);
    }

    [Fact]
    public async Task ImportEventsAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<RedcapEvent> { new() { EventName = "baseline", ArmNumber = "1" } };

        await api.ImportEventsAsync(false, RedcapFormat.json, data, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("event", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("import", transport.LastDictionaryPayload["action"]);
        Assert.Equal("false", transport.LastDictionaryPayload["override"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Contains("baseline", transport.LastDictionaryPayload["data"]);
    }

    [Fact]
    public async Task DeleteEventsAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.DeleteEventsAsync(new[] { "event_1_arm_1" });

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("event", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("delete", transport.LastDictionaryPayload["action"]);
        Assert.Equal("event_1_arm_1", transport.LastDictionaryPayload["events[0]"]);
    }

    [Fact]
    public async Task DeleteEventsAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.DeleteEventsAsync(new[] { "event_1_arm_1" });

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("event", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("delete", transport.LastDictionaryPayload["action"]);
        Assert.Equal("event_1_arm_1", transport.LastDictionaryPayload["events[0]"]);
    }

    [Fact]
    public async Task DeleteEventsAsync_WithNoEvents_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
            api.DeleteEventsAsync(Array.Empty<string>()));

        Assert.Contains("No events to delete", ex.Message);
        Assert.Null(transport.LastDictionaryPayload);
    }

    [Fact]
    public async Task ExportInstrumentsAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportInstrumentsAsync(RedcapFormat.csv);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("instrument", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
    }

    [Fact]
    public async Task ExportInstrumentsTypedAsync_UsesJsonPayloadAndDeserializesResponse()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "[{\"instrument_name\":\"demographics\",\"instrument_label\":\"Demographics\"},{\"instrument_name\":\"follow_up\",\"instrument_label\":\"Follow Up\"}]"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var result = await api.ExportInstrumentsTypedAsync();

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("instrument", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal(2, result.Count);
        Assert.Equal("demographics", result[0].InstrumentName);
        Assert.Equal("Demographics", result[0].InstrumentLabel);
        Assert.Equal("follow_up", result[1].InstrumentName);
        Assert.Equal("Follow Up", result[1].InstrumentLabel);
    }

    [Fact]
    public async Task ExportInstrumentsTypedAsync_WithInvalidJson_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "not json"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => api.ExportInstrumentsTypedAsync());

        Assert.Equal("Failed to deserialize REDCap instrument response.", ex.Message);
    }

    [Fact]
    public async Task ExportInstrumentMappingAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportInstrumentMappingAsync(RedcapFormat.csv, new[] { "1" }, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("formEventMapping", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
        Assert.Equal("1", transport.LastDictionaryPayload["arms[0]"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
    }

    [Fact]
    public async Task ExportInstrumentMappingTypedAsync_UsesJsonPayloadAndDeserializesResponse()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "[{\"arm_num\":\"1\",\"unique_event_name\":\"event_1_arm_1\",\"form\":\"survey_a\"},{\"arm_num\":\"2\",\"unique_event_name\":\"event_2_arm_2\",\"form\":\"follow_up\"}]"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var result = await api.ExportInstrumentMappingTypedAsync(new[] { "1", "2" }, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("formEventMapping", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("1", transport.LastDictionaryPayload["arms[0]"]);
        Assert.Equal("2", transport.LastDictionaryPayload["arms[1]"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Equal(2, result.Count);
        Assert.Equal("1", result[0].arm_num);
        Assert.Equal("event_1_arm_1", result[0].unique_event_name);
        Assert.Equal("survey_a", result[0].form);
    }

    [Fact]
    public async Task ExportInstrumentMappingTypedAsync_WithInvalidJson_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "not json"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => api.ExportInstrumentMappingTypedAsync());

        Assert.Equal("Failed to deserialize REDCap instrument-event mapping response.", ex.Message);
    }

    [Fact]
    public async Task ImportInstrumentMappingAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<FormEventMapping>
        {
            new() { arm_num = "1", unique_event_name = "event_1_arm_1", form = "demographics" }
        };

        await api.ImportInstrumentMappingAsync(RedcapFormat.json, data, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("formEventMapping", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Contains("demographics", transport.LastDictionaryPayload["data"]);
    }

    [Fact]
    public async Task ExportMetaDataAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportMetaDataAsync(RedcapFormat.csv, new[] { "record_id" }, new[] { "demographics" }, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("metadata", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
        Assert.Equal("record_id", transport.LastDictionaryPayload["fields[0]"]);
        Assert.Equal("demographics", transport.LastDictionaryPayload["forms[0]"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
    }

    [Fact]
    public async Task ExportMetaDataTypedAsync_UsesJsonPayloadAndDeserializesResponse()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "[{\"field_name\":\"record_id\",\"form_name\":\"demographics\",\"field_type\":\"text\"}]"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var result = await api.ExportMetaDataTypedAsync(new[] { "record_id" }, new[] { "demographics" }, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("metadata", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("record_id", transport.LastDictionaryPayload["fields[0]"]);
        Assert.Equal("demographics", transport.LastDictionaryPayload["forms[0]"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Single(result);
        Assert.Equal("record_id", result[0].field_name);
        Assert.Equal("demographics", result[0].form_name);
        Assert.Equal("text", result[0].field_type);
    }

    [Fact]
    public async Task ExportMetaDataTypedAsync_WithInvalidJson_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "not json"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => api.ExportMetaDataTypedAsync());

        Assert.Equal("Failed to deserialize REDCap metadata response.", ex.Message);
    }

    [Fact]
    public async Task ImportMetaDataAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<RedcapMetaData> { new() { field_name = "record_id", form_name = "demographics", field_type = "text" } };

        await api.ImportMetaDataAsync(RedcapFormat.json, data, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("metadata", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Contains("record_id", transport.LastDictionaryPayload["data"]);
    }

    [Fact]
    public async Task ImportMetaDataAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<RedcapMetaData> { new() { field_name = "sex", form_name = "demographics", field_type = "radio" } };

        await api.ImportMetaDataAsync(RedcapFormat.json, data, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("metadata", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Contains("sex", transport.LastDictionaryPayload["data"]);
    }

    [Fact]
    public async Task CreateProjectAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<RedcapProject> { new() { project_title = "My Project", purpose = ProjectPurpose.Other } };

        await api.CreateProjectAsync(RedcapFormat.json, data, RedcapReturnFormat.xml, "<odm />");

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("project", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Contains("My Project", transport.LastDictionaryPayload["data"]);
        Assert.Equal("<odm />", transport.LastDictionaryPayload["odm"]);
    }

    [Fact]
    public async Task CreateProjectAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<RedcapProject> { new() { project_title = "My Project 2", purpose = ProjectPurpose.Other } };

        await api.CreateProjectAsync(RedcapFormat.json, data, RedcapReturnFormat.xml, "<odm2 />");

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("project", transport.LastDictionaryPayload!["content"]);
        Assert.Contains("My Project 2", transport.LastDictionaryPayload["data"]);
        Assert.Equal("<odm2 />", transport.LastDictionaryPayload["odm"]);
    }

    [Fact]
    public async Task ImportProjectInfoAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var info = new RedcapProjectInfo { ProjectTitle = "Updated Project", SurveysEnabled = 1 };

        await api.ImportProjectInfoAsync(RedcapFormat.json, info);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("project_settings", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Contains("Updated Project", transport.LastDictionaryPayload["data"]);
    }

    [Fact]
    public async Task ExportProjectInfoAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportProjectInfoAsync(RedcapFormat.csv, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("project", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
    }

    [Fact]
    public async Task ExportProjectXmlAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportProjectXmlAsync(
            returnMetadataOnly: true,
            records: new[] { "1", "2" },
            events: new[] { "event_1_arm_1" },
            returnFormat: RedcapReturnFormat.xml,
            exportSurveyFields: true,
            exportDataAccessGroups: true,
            filterLogic: "[record_id] = '1'",
            exportFiles: true);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("project_xml", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Equal("True", transport.LastDictionaryPayload["returnMetadataOnly"]);
        Assert.Equal("1,2", transport.LastDictionaryPayload["records"]);
        Assert.Equal("event_1_arm_1", transport.LastDictionaryPayload["events"]);
        Assert.Equal("True", transport.LastDictionaryPayload["exportSurveyFields"]);
        Assert.Equal("True", transport.LastDictionaryPayload["exportDataAccessGroups"]);
        Assert.Equal("[record_id] = '1'", transport.LastDictionaryPayload["filterLogic"]);
        Assert.Equal("True", transport.LastDictionaryPayload["exportFiles"]);
    }

    [Fact]
    public async Task ExportArmsAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportArmsAsync(RedcapFormat.csv, new[] { "1" }, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("arm", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
        Assert.Equal("1", transport.LastDictionaryPayload["arms[0]"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
    }

    [Fact]
    public async Task ExportArmsTypedAsync_UsesJsonPayloadAndDeserializesResponse()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "[{\"arm_num\":\"1\",\"name\":\"Arm A\"},{\"arm_num\":\"2\",\"name\":\"Arm B\"}]"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var result = await api.ExportArmsTypedAsync(new[] { "1", "2" }, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("arm", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Equal("1", transport.LastDictionaryPayload["arms[0]"]);
        Assert.Equal("2", transport.LastDictionaryPayload["arms[1]"]);
        Assert.Equal(2, result.Count);
        Assert.Equal("1", result[0].ArmNumber);
        Assert.Equal("Arm A", result[0].Name);
        Assert.Equal("2", result[1].ArmNumber);
    }

    [Fact]
    public async Task ExportArmsTypedAsync_WithInvalidJson_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "not-json"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => api.ExportArmsTypedAsync());

        Assert.Equal("Failed to deserialize REDCap arm response.", ex.Message);
    }

    [Fact]
    public async Task ExportArmsAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportArmsAsync(RedcapFormat.csv, new[] { "1" }, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("arm", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Equal("1", transport.LastDictionaryPayload["arms[0]"]);
    }

    [Fact]
    public async Task ImportArmsAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<TestArmPayload> { new() { arm_num = "1", name = "Arm A" } };

        await api.ImportArmsAsync(false, RedcapAction.Import, RedcapFormat.json, data, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("arm", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("import", transport.LastDictionaryPayload["action"]);
        Assert.Equal("false", transport.LastDictionaryPayload["override"]);
        Assert.Contains("Arm A", transport.LastDictionaryPayload["data"]);
    }

    [Fact]
    public async Task ImportArmsAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<RedcapArm> { new() { ArmNumber = "1", Name = "Arm B" } };

        await api.ImportArmsAsync(false, RedcapAction.Import, RedcapFormat.json, data, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("arm", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("import", transport.LastDictionaryPayload["action"]);
        Assert.Contains("Arm B", transport.LastDictionaryPayload["data"]);
    }

    [Fact]
    public async Task ImportArmsAsync_ContentOverload_ListRedcapArmVariant_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<RedcapArm> { new() { ArmNumber = "2", Name = "Arm C" } };

        await api.ImportArmsAsync<RedcapArm>(false, RedcapAction.Import, RedcapFormat.json, data, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("arm", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("import", transport.LastDictionaryPayload["action"]);
        Assert.Contains("Arm C", transport.LastDictionaryPayload["data"]);
    }

    [Fact]
    public async Task ExportFieldNamesAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportFieldNamesAsync(RedcapFormat.csv, "record_id", RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("exportFieldNames", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
        Assert.Equal("record_id", transport.LastDictionaryPayload["field"]);
    }

    [Fact]
    public async Task ExportFieldNamesAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportFieldNamesAsync(RedcapFormat.csv, "record_id", RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("exportFieldNames", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("record_id", transport.LastDictionaryPayload["field"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
    }

    [Fact]
    public async Task DeleteArmsAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.DeleteArmsAsync(new[] { "1", "2" });

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("arm", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("delete", transport.LastDictionaryPayload["action"]);
        Assert.Equal("1", transport.LastDictionaryPayload["arms[0]"]);
        Assert.Equal("2", transport.LastDictionaryPayload["arms[1]"]);
    }

    [Fact]
    public async Task DeleteArmsAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.DeleteArmsAsync(new[] { "1", "2" });

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("arm", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("delete", transport.LastDictionaryPayload["action"]);
        Assert.Equal("1", transport.LastDictionaryPayload["arms[0]"]);
        Assert.Equal("2", transport.LastDictionaryPayload["arms[1]"]);
    }

    [Fact]
    public async Task DeleteArmsAsync_WithNoArms_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
            api.DeleteArmsAsync(Array.Empty<string>()));

        Assert.Contains("No arm to delete", ex.Message);
        Assert.Null(transport.LastDictionaryPayload);
    }

    [Fact]
    public async Task ExportFileAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            var result = await api.ExportFileAsync("1", "upload", "event_1_arm_1", "2", RedcapReturnFormat.xml, tempFolder);

            Assert.Equal("transport-response", result);
            Assert.NotNull(transport.LastDictionaryPayload);
            Assert.Equal("file", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("export", transport.LastDictionaryPayload["action"]);
            Assert.False(transport.LastDictionaryPayload.ContainsKey("filePath"));
            Assert.Equal("2", transport.LastDictionaryPayload["repeat_instance"]);
        }
        finally
        {
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }
        }
    }

    [Fact]
    public async Task ExportFileAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            await api.ExportFileAsync("1", "upload", "event_1_arm_1", "2", RedcapReturnFormat.xml, tempFolder);

            Assert.NotNull(transport.LastDictionaryPayload);
            Assert.Equal("file", transport.LastDictionaryPayload!["content"]);
            Assert.Equal("export", transport.LastDictionaryPayload["action"]);
            Assert.Equal("2", transport.LastDictionaryPayload["repeat_instance"]);
            Assert.False(transport.LastDictionaryPayload.ContainsKey("filePath"));
        }
        finally
        {
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }
        }
    }

    [Fact]
    public async Task ExportFileAsync_DefaultOverload_WithMissingFilePath_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
            api.ExportFileAsync("1", "upload", "event_1_arm_1", "1", RedcapReturnFormat.xml, filePath: null));

        Assert.Contains("file path", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(transport.LastDictionaryPayload);
    }

    [Fact]
    public async Task ExportFileAsync_DefaultOverload_WithMissingRecord_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
                api.ExportFileAsync(record: null, field: "upload", eventName: "event_1_arm_1", repeatInstance: "1", returnFormat: RedcapReturnFormat.xml, filePath: tempFolder));

            Assert.Contains("No record provided", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Null(transport.LastDictionaryPayload);
        }
        finally
        {
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }
        }
    }

    [Fact]
    public async Task ExportFileAsync_ContentOverload_WithNullRepeatInstance_OmitsRepeatInstanceKey()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            await api.ExportFileAsync("1", "upload", "event_1_arm_1", null, RedcapReturnFormat.xml, tempFolder);

            Assert.NotNull(transport.LastDictionaryPayload);
            Assert.False(transport.LastDictionaryPayload!.ContainsKey("repeat_instance"));
            Assert.False(transport.LastDictionaryPayload.ContainsKey("filePath"));
        }
        finally
        {
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }
        }
    }

    [Fact]
    public async Task ImportFileAsync_DefaultOverload_UsesMultipartPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempFolder);
        var fileName = "sample.txt";
        await File.WriteAllTextAsync(Path.Combine(tempFolder, fileName), "sample-body");

        try
        {
            await api.ImportFileAsync("1", "upload", "event_1_arm_1", "1", fileName, tempFolder, RedcapReturnFormat.xml);

            Assert.NotNull(transport.LastMultipartPayload);
            var fields = await ReadMultipartFieldsAsync(transport.LastMultipartPayload!);
            Assert.Equal(Token, fields["token"]);
            Assert.Equal("file", fields["content"]);
            Assert.Equal("import", fields["action"]);
            Assert.Equal("1", fields["record"]);
            Assert.Equal("upload", fields["field"]);
            Assert.Equal("event_1_arm_1", fields["event"]);
            Assert.Equal("1", fields["repeat_instance"]);
            Assert.True(fields.ContainsKey("file"));
        }
        finally
        {
            Directory.Delete(tempFolder, true);
        }
    }

    [Fact]
    public async Task ImportFileAsync_ContentOverload_UsesMultipartPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempFolder);
        var fileName = "sample2.txt";
        await File.WriteAllTextAsync(Path.Combine(tempFolder, fileName), "sample-body-2");

        try
        {
            await api.ImportFileAsync("1", "upload", "event_1_arm_1", "2", fileName, tempFolder, RedcapReturnFormat.xml);

            Assert.NotNull(transport.LastMultipartPayload);
            var fields = await ReadMultipartFieldsAsync(transport.LastMultipartPayload!);
            Assert.Equal("file", fields["content"]);
            Assert.Equal("import", fields["action"]);
            Assert.Equal("2", fields["repeat_instance"]);
        }
        finally
        {
            Directory.Delete(tempFolder, true);
        }
    }

    [Fact]
    public async Task DeleteFileAsync_ContentOverload_UsesMultipartPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.DeleteFileAsync("1", "upload", "event_1_arm_1", "3", RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastMultipartPayload);
        var fields = await ReadMultipartFieldsAsync(transport.LastMultipartPayload!);
        Assert.Equal(Token, fields["token"]);
        Assert.Equal("file", fields["content"]);
        Assert.Equal("delete", fields["action"]);
        Assert.Equal("3", fields["repeat_instance"]);
    }

    [Fact]
    public async Task DeleteFileAsync_DefaultOverload_UsesMultipartPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.DeleteFileAsync("1", "upload", "event_1_arm_1", "4", RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastMultipartPayload);
        var fields = await ReadMultipartFieldsAsync(transport.LastMultipartPayload!);
        Assert.Equal("file", fields["content"]);
        Assert.Equal("delete", fields["action"]);
        Assert.Equal("4", fields["repeat_instance"]);
    }

    [Fact]
    public async Task DeleteFileAsync_DefaultOverload_WithNullRepeatInstance_DefaultsToOne()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.DeleteFileAsync("1", "upload", "event_1_arm_1", null, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastMultipartPayload);
        var fields = await ReadMultipartFieldsAsync(transport.LastMultipartPayload!);
        Assert.Equal("1", fields["repeat_instance"]);
    }

    [Fact]
    public async Task DeleteFileAsync_ContentOverload_WithNullRepeatInstance_DefaultsToOne()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.DeleteFileAsync("1", "upload", "event_1_arm_1", null, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastMultipartPayload);
        var fields = await ReadMultipartFieldsAsync(transport.LastMultipartPayload!);
        Assert.Equal("1", fields["repeat_instance"]);
    }

    [Fact]
    public async Task CreateFolderFileRepositoryAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.CreateFolderFileRepositoryAsync(Content.FileRepository, RedcapAction.CreateFolder, "new-folder", RedcapFormat.json, "10", "20", "30", RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("fileRepository", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("createFolder", transport.LastDictionaryPayload["action"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("10", transport.LastDictionaryPayload["folder_id"]);
        Assert.Equal("20", transport.LastDictionaryPayload["dag_id"]);
        Assert.Equal("30", transport.LastDictionaryPayload["role_id"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
    }

    [Fact]
    public async Task CreateFolderFileRepositoryAsync_WithNoName_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
            api.CreateFolderFileRepositoryAsync(Content.FileRepository, RedcapAction.CreateFolder, null!, RedcapFormat.json));

        Assert.Contains("Please provide a valid name", ex.Message);
        Assert.Null(transport.LastDictionaryPayload);
    }

    [Fact]
    public async Task ExportFilesFoldersFileRepositoryAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportFilesFoldersFileRepositoryAsync(folderId: "12", format: RedcapFormat.csv, returnFormat: RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("fileRepository", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("list", transport.LastDictionaryPayload["action"]);
        Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
        Assert.Equal("12", transport.LastDictionaryPayload["folder_id"]);
    }

    [Fact]
    public async Task ExportFileFileRepositoryAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportFileFileRepositoryAsync(Content.FileRepository, RedcapAction.Export, "55", RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("fileRepository", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("export", transport.LastDictionaryPayload["action"]);
        Assert.Equal("55", transport.LastDictionaryPayload["doc_id"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
    }

    [Fact]
    public async Task ImportFileRepositoryAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ImportFileRepositoryAsync(Content.FileRepository, RedcapAction.Import, "file-content", "9", RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("fileRepository", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("import", transport.LastDictionaryPayload["action"]);
        Assert.Equal("file-content", transport.LastDictionaryPayload["file"]);
        Assert.Equal("9", transport.LastDictionaryPayload["folder_id"]);
    }

    [Fact]
    public async Task DeleteFileRepositoryAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.DeleteFileRepositoryAsync(Content.FileRepository, RedcapAction.Delete, "55", RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("fileRepository", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("delete", transport.LastDictionaryPayload["action"]);
        Assert.Equal("55", transport.LastDictionaryPayload["doc_id"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
    }

    [Fact]
    public async Task DeleteFileRepositoryAsync_WithNoDocId_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
            api.DeleteFileRepositoryAsync());

        Assert.Contains("Please provide a document id", ex.Message);
        Assert.Null(transport.LastDictionaryPayload);
    }

    [Fact]
    public async Task ExportSurveyLinkAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportSurveyLinkAsync("1", "survey_a", "event_1_arm_1", 2, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("surveyLink", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("1", transport.LastDictionaryPayload["record"]);
        Assert.Equal("survey_a", transport.LastDictionaryPayload["instrument"]);
        Assert.Equal("event_1_arm_1", transport.LastDictionaryPayload["event"]);
        Assert.Equal("2", transport.LastDictionaryPayload["repeat_instance"]);
    }

    [Fact]
    public async Task ExportSurveyLinkAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportSurveyLinkAsync("1", "survey_a", "event_1_arm_1", 2, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("surveyLink", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("1", transport.LastDictionaryPayload["record"]);
    }

    [Fact]
    public async Task ExportSurveyParticipantsAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportSurveyParticipantsAsync("survey_a", "event_1_arm_1", RedcapFormat.csv, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("participantList", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("survey_a", transport.LastDictionaryPayload["instrument"]);
        Assert.Equal("event_1_arm_1", transport.LastDictionaryPayload["event"]);
        Assert.Equal("csv", transport.LastDictionaryPayload["format"]);
    }

    [Fact]
    public async Task ExportSurveyParticipantsAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportSurveyParticipantsAsync("survey_a", "event_1_arm_1", RedcapFormat.csv, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("participantList", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("survey_a", transport.LastDictionaryPayload["instrument"]);
    }

    [Fact]
    public async Task ExportSurveyQueueLinkAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportSurveyQueueLinkAsync("1", RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("surveyQueueLink", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("1", transport.LastDictionaryPayload["record"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
    }

    [Fact]
    public async Task ExportSurveyQueueLinkAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportSurveyQueueLinkAsync("1", RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("surveyQueueLink", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("1", transport.LastDictionaryPayload["record"]);
    }

    [Fact]
    public async Task ExportSurveyReturnCodeAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportSurveyReturnCodeAsync("1", "survey_a", "event_1_arm_1", "3", RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("surveyReturnCode", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("3", transport.LastDictionaryPayload["repeat_instance"]);
    }

    [Fact]
    public async Task ExportSurveyReturnCodeAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportSurveyReturnCodeAsync("1", "survey_a", "event_1_arm_1", "3", RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("surveyReturnCode", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("3", transport.LastDictionaryPayload["repeat_instance"]);
    }

    [Fact]
    public async Task ExportSurveyAccessCodeAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportSurveyAccessCodeAsync("1", "survey_a", "event_1_arm_1", 2, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("surveyAccessCode", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("2", transport.LastDictionaryPayload["repeat_instance"]);
    }

    [Fact]
    public async Task ExportRedcapVersionAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportRedcapVersionAsync(RedcapFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("version", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["format"]);
    }

    [Fact]
    public async Task ExportRedcapVersionAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportRedcapVersionAsync(RedcapFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("version", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["format"]);
    }

    [Fact]
    public async Task ExportRedcapVersionAsync_CachesReturnedVersion()
    {
        var transport = new FakeTransport { ResponseBody = "14.2.1" };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var result = await api.ExportRedcapVersionAsync();

        Assert.Equal("14.2.1", result);
        Assert.Equal("14.2.1", api.Version);
    }

    [Fact]
    public async Task ExportInstrumentsAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportInstrumentsAsync(RedcapFormat.json);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("instrument", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
    }

    [Fact]
    public async Task ExportPDFInstrumentsAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportPDFInstrumentsAsync("1", "event_1_arm_1", "survey_a", true, (string?)null, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("pdf", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("1", transport.LastDictionaryPayload["record"]);
        Assert.Equal("True", transport.LastDictionaryPayload["allRecords"]);
    }

    [Fact]
    public async Task ExportPDFInstrumentsAsync_WithFilePath_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        await api.ExportPDFInstrumentsAsync("1", "event_1_arm_1", "survey_a", true, tempFolder, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("pdf", transport.LastDictionaryPayload!["content"]);
        Assert.False(transport.LastDictionaryPayload.ContainsKey("filePath"));
        Assert.Equal(tempFolder, transport.LastDownloadDestinationPath);
        Assert.Equal("True", transport.LastDictionaryPayload["allRecords"]);
    }

    [Fact]
    public async Task ExportPDFInstrumentsAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportPDFInstrumentsAsync("1", "event_1_arm_1", "survey_a", true, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("pdf", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("1", transport.LastDictionaryPayload["record"]);
        Assert.Equal("True", transport.LastDictionaryPayload["allRecords"]);
    }

    [Fact]
    public async Task ExportLoggingAsync_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportLoggingAsync(RedcapFormat.csv, LogType.RecordEdit, "alice", "1", "2", "2024-01-01 10:00", "2024-01-02 10:00", RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("log", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("record_edit", transport.LastDictionaryPayload["logtype"]);
        Assert.Equal("alice", transport.LastDictionaryPayload["user"]);
        Assert.Equal("1", transport.LastDictionaryPayload["record"]);
    }

    [Fact]
    public async Task ExportInstrumentMappingAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportInstrumentMappingAsync(RedcapFormat.json, new[] { "1" }, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("formEventMapping", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("1", transport.LastDictionaryPayload["arms[0]"]);
    }

    [Fact]
    public async Task ImportInstrumentMappingAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<FormEventMapping> { new() { arm_num = "1", unique_event_name = "event_1_arm_1", form = "survey_a" } };

        await api.ImportInstrumentMappingAsync(RedcapFormat.json, data, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("formEventMapping", transport.LastDictionaryPayload!["content"]);
        Assert.Contains("survey_a", transport.LastDictionaryPayload["data"]);
    }

    [Fact]
    public async Task ExportMetaDataAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportMetaDataAsync(RedcapFormat.json, new[] { "record_id" }, new[] { "survey_a" }, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("metadata", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("record_id", transport.LastDictionaryPayload["fields[0]"]);
        Assert.Equal("survey_a", transport.LastDictionaryPayload["forms[0]"]);
    }

    [Fact]
    public async Task ExportProjectInfoAsync_ContentOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportProjectInfoAsync(RedcapFormat.json, RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("project", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
    }

    [Fact]
    public async Task ExportProjectInfoTypedAsync_UsesJsonPayloadAndDeserializesResponse()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "{\"project_id\":\"12\",\"project_title\":\"Demo Project\",\"project_notes\":\"Notes\"}"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var result = await api.ExportProjectInfoTypedAsync(RedcapReturnFormat.xml);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("project", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("xml", transport.LastDictionaryPayload["returnFormat"]);
        Assert.Equal("12", result.ProjectId);
        Assert.Equal("Demo Project", result.ProjectTitle);
        Assert.Equal("Notes", result.ProjectNotes);
    }

    [Fact]
    public async Task ExportProjectInfoTypedAsync_WithInvalidJson_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport
        {
            ResponseBody = "not-json"
        };
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() => api.ExportProjectInfoTypedAsync());

        Assert.Equal("Failed to deserialize REDCap project info response.", ex.Message);
    }

    [Fact]
    public async Task ImportProjectInfoAsync_DefaultOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var info = new RedcapProjectInfo { ProjectTitle = "Title", ProjectNotes = "Notes" };

        await api.ImportProjectInfoAsync(RedcapFormat.json, info);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("project_settings", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Contains("Title", transport.LastDictionaryPayload["data"]);
    }

    [Fact]
    public async Task ExportProjectXmlAsync_ContentlessOverload_UsesExpectedPayload()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportProjectXmlAsync(true, new[] { "1" }, null, new[] { "event_1_arm_1" }, RedcapReturnFormat.xml, true, true, "[record_id] = '1'", true);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("project_xml", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("True", transport.LastDictionaryPayload["returnMetadataOnly"]);
    }

    // ── Phase 1: DateTime format ────────────────────────────────────────────

    [Fact]
    public async Task ExportRecordsAsync_FormatsDateRangeAs24HourTime()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportRecordsAsync(dateRangeBegin: new DateTime(2024, 6, 15, 15, 4, 5), dateRangeEnd: new DateTime(2024, 6, 16, 23, 59, 0));

        Assert.Equal("2024-06-15 15:04:05", transport.LastDictionaryPayload!["dateRangeBegin"]);
        Assert.Equal("2024-06-16 23:59:00", transport.LastDictionaryPayload["dateRangeEnd"]);
    }

    [Fact]
    public async Task ExportRecordAsync_FormatsDateRangeAs24HourTime()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportRecordAsync("1", dateRangeBegin: new DateTime(2024, 6, 15, 15, 4, 5), dateRangeEnd: new DateTime(2024, 6, 16, 23, 59, 0));

        Assert.Equal("2024-06-15 15:04:05", transport.LastDictionaryPayload!["dateRangeBegin"]);
        Assert.Equal("2024-06-16 23:59:00", transport.LastDictionaryPayload["dateRangeEnd"]);
    }

    [Fact]
    public async Task ExportRecordsAsync_OmitsDateRangeKeysWhenNull()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportRecordsAsync(dateRangeBegin: null, dateRangeEnd: null);

        Assert.False(transport.LastDictionaryPayload!.ContainsKey("dateRangeBegin"));
        Assert.False(transport.LastDictionaryPayload.ContainsKey("dateRangeEnd"));
    }

    // ── Phase 2: Untested guard clauses ─────────────────────────────────────

    [Fact]
    public async Task ImportEventsAsync_WithEmptyData_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
            api.ImportEventsAsync(false, RedcapFormat.json, new List<object>()));

        Assert.NotNull(ex);
        Assert.Null(transport.LastDictionaryPayload);
    }

    [Fact]
    public async Task ImportFileRepositoryAsync_WithNoFile_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
            api.ImportFileRepositoryAsync(file: null));

        Assert.NotNull(ex);
    }

    [Fact]
    public async Task ExportFileAsync_WithMissingField_ThrowsRedcapApiException()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var ex = await Assert.ThrowsAsync<RedcapApiException>(() =>
                api.ExportFileAsync("rec1", field: "", eventName: "", filePath: tempDir));

            Assert.NotNull(ex);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Constructor_WithEmptyToken_ThrowsArgumentException()
    {
        var transport = new FakeTransport();

        Assert.Throws<ArgumentException>(() => new Redcap.RedcapApi("http://localhost/", string.Empty, transport));
    }

    // ── Phase 3: Optional parameter omission ────────────────────────────────

    [Fact]
    public async Task ExportLoggingAsync_WhenNoOptionalFilters_OmitsOptionalKeys()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportLoggingAsync();

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.False(transport.LastDictionaryPayload!.ContainsKey("user"));
        Assert.False(transport.LastDictionaryPayload.ContainsKey("record"));
        Assert.False(transport.LastDictionaryPayload.ContainsKey("dag"));
        Assert.False(transport.LastDictionaryPayload.ContainsKey("beginTime"));
        Assert.False(transport.LastDictionaryPayload.ContainsKey("endTime"));
        Assert.Equal("log", transport.LastDictionaryPayload["content"]);
    }

    [Fact]
    public async Task ExportLoggingAsync_WhenFiltersProvided_IncludesOptionalKeys()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);

        await api.ExportLoggingAsync(user: "alice", record: "42", dag: "ca_site", beginTime: "2024-01-01 00:00", endTime: "2024-12-31 23:59");

        Assert.Equal("alice", transport.LastDictionaryPayload!["user"]);
        Assert.Equal("42", transport.LastDictionaryPayload["record"]);
        Assert.Equal("ca_site", transport.LastDictionaryPayload["dag"]);
        Assert.Equal("2024-01-01 00:00", transport.LastDictionaryPayload["beginTime"]);
        Assert.Equal("2024-12-31 23:59", transport.LastDictionaryPayload["endTime"]);
    }

    [Fact]
    public async Task ImportRecordsAsync_AlwaysIncludesRequiredFields()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<TestRecordPayload> { new() { RecordId = "1", FirstName = "Alice" } };

        await api.ImportRecordsAsync(RedcapFormat.json, RedcapDataType.flat, OverwriteBehavior.normal, false, false, data);

        Assert.NotNull(transport.LastDictionaryPayload);
        Assert.Equal("record", transport.LastDictionaryPayload!["content"]);
        Assert.Equal("json", transport.LastDictionaryPayload["format"]);
        Assert.Equal("flat", transport.LastDictionaryPayload["type"]);
        Assert.Equal("normal", transport.LastDictionaryPayload["overwriteBehavior"]);
        Assert.True(transport.LastDictionaryPayload.ContainsKey("data"));
        Assert.False(transport.LastDictionaryPayload.ContainsKey("dateFormat"));
    }

    [Fact]
    public async Task ImportRecordsAsync_WhenDateFormatProvided_IncludesDateFormatKey()
    {
        var transport = new FakeTransport();
        var api = new Redcap.RedcapApi("http://localhost/", Token, transport);
        var data = new List<TestRecordPayload> { new() { RecordId = "1" } };

        await api.ImportRecordsAsync(RedcapFormat.json, RedcapDataType.flat, OverwriteBehavior.normal, false, false, data, dateFormat: "MDY");

        Assert.Equal("MDY", transport.LastDictionaryPayload!["dateFormat"]);
    }

    [Fact]
    public async Task ProtectedWrapperMethods_AreReachableViaSubclass()
    {
        var transport = new FakeTransport();
        var api = new TestableRedcapApi("http://localhost/", Token, transport);

        await api.CallMultipartWrapper(new MultipartFormDataContent());
        await api.CallDictionaryStreamWrapper(new Dictionary<string, string> { { "token", Token } });
        api.CallConvertIntArrayWrapper(new[] { 1, 2 });
        api.CallHandleFormatWrapper(RedcapFormat.json, RedcapReturnFormat.xml, RedcapDataType.flat);
        api.CallHandleReturnContentWrapper(ReturnContent.count);
        api.CallExtractBehaviorWrapper(OverwriteBehavior.normal);

        Assert.NotNull(transport.LastDictionaryPayload);
    }

    private static async Task<Dictionary<string, string>> ReadMultipartFieldsAsync(MultipartFormDataContent payload)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in payload)
        {
            var name = part.Headers.ContentDisposition?.Name?.Trim('"');
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            if (part.Headers.ContentDisposition?.FileName is not null)
            {
                fields[name] = "<binary>";
                continue;
            }

            fields[name] = await part.ReadAsStringAsync();
        }

        return fields;
    }

    private sealed class TestRecordPayload
    {
        public string RecordId { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
    }

    private sealed class TestUserDagAssignment
    {
        public string Username { get; set; } = string.Empty;

        public string RedcapDataAccessGroup { get; set; } = string.Empty;
    }

    private sealed class TestUserRole
    {
        public string UniqueRoleName { get; set; } = string.Empty;

        public string RoleLabel { get; set; } = string.Empty;

        public string ApiExport { get; set; } = string.Empty;
    }

    private sealed class TestUserRoleAssignment
    {
        public string Username { get; set; } = string.Empty;

        public string UniqueRoleName { get; set; } = string.Empty;
    }

    private sealed class TestArmPayload
    {
        public string arm_num { get; set; } = string.Empty;

        public string name { get; set; } = string.Empty;
    }

    private sealed class TestableRedcapApi : Redcap.RedcapApi
    {
        public TestableRedcapApi(string redcapApiUrl, string token, IRedcapTransport transport)
            : base(redcapApiUrl, token, transport)
        {
        }

        public Task<string> CallMultipartWrapper(MultipartFormDataContent payload)
            => base.SendPostRequestAsync(payload, new Uri("http://localhost/"));

        public Task<Stream?> CallDictionaryStreamWrapper(Dictionary<string, string> payload)
            => base.GetStreamContentAsync(payload, new Uri("http://localhost/"));

        public string CallConvertIntArrayWrapper(int[] input)
            => base.ConvertIntArraytoString(input);

        public (string format, string onErrorFormat, string redcapDataType) CallHandleFormatWrapper(RedcapFormat? format, RedcapReturnFormat? onErrorFormat, RedcapDataType? redcapDataType)
            => base.HandleFormat(format, onErrorFormat, redcapDataType);

        public string CallHandleReturnContentWrapper(ReturnContent returnContent)
            => base.HandleReturnContent(returnContent);

        public string CallExtractBehaviorWrapper(OverwriteBehavior overwriteBehavior)
            => base.ExtractBehavior(overwriteBehavior);
    }

    private sealed class FakeTransport : IRedcapTransport
    {
        public string ResponseBody { get; set; } = "transport-response";

        public Dictionary<string, string>? LastDictionaryPayload { get; private set; }

        public MultipartFormDataContent? LastMultipartPayload { get; private set; }

        public string? LastDownloadDestinationPath { get; private set; }

        public Task<Stream?> GetStreamContentAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            LastDictionaryPayload = new Dictionary<string, string>(payload);
            return Task.FromResult<Stream?>(new MemoryStream());
        }

        public Task<string> SendPostRequestAsync(MultipartFormDataContent payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            LastMultipartPayload = payload;
            return Task.FromResult(ResponseBody);
        }

        public Task<string> SendPostRequestAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            LastDictionaryPayload = new Dictionary<string, string>(payload);
            return Task.FromResult(ResponseBody);
        }

        public Task<string> DownloadFileAsync(Dictionary<string, string> payload, Uri uri, string destinationPath, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            LastDictionaryPayload = new Dictionary<string, string>(payload);
            LastDownloadDestinationPath = destinationPath;
            return Task.FromResult(ResponseBody);
        }
    }
}
