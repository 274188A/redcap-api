using Redcap.Models;
using Redcap.Utilities;
using System.Text.Json;
using Xunit;

namespace RedcapApi.Tests;

public class JsonSerializationTests
{
    // ── Export (deserialize) ─────────────────────────────────────────────────

    [Fact]
    public void RedcapArm_DeserializesFromWireNames()
    {
        var json = """[{"arm_num":"2","name":"Drug B"}]""";
        var arms = JsonSerializer.Deserialize<List<RedcapArm>>(json, RedcapJsonOptions.Default);
        Assert.NotNull(arms);
        Assert.Single(arms);
        Assert.Equal("2", arms[0].ArmNumber);
        Assert.Equal("Drug B", arms[0].Name);
    }

    [Fact]
    public void RedcapEvent_DeserializesFromWireNames()
    {
        var json = """[{"event_name":"Baseline","arm_num":"1","unique_event_name":"baseline_arm_1","day_offset":"0","offset_min":"0","offset_max":"0","custom_event_label":null}]""";
        var events = JsonSerializer.Deserialize<List<RedcapEvent>>(json, RedcapJsonOptions.Default);
        Assert.NotNull(events);
        Assert.Single(events);
        Assert.Equal("Baseline", events[0].EventName);
        Assert.Equal("1", events[0].ArmNumber);
        Assert.Equal("baseline_arm_1", events[0].UniqueEventName);
    }

    [Fact]
    public void RedcapUser_DeserializesFromWireNames()
    {
        var json = """[{"username":"jdoe","email":"jdoe@example.com","firstname":"John","lastname":"Doe","expiration":""}]""";
        var users = JsonSerializer.Deserialize<List<RedcapUser>>(json, RedcapJsonOptions.Default);
        Assert.NotNull(users);
        Assert.Single(users);
        Assert.Equal("jdoe", users[0].Username);
        Assert.Equal("jdoe@example.com", users[0].Email);
        Assert.Equal("John", users[0].FirstName);
        Assert.Equal("Doe", users[0].LastName);
    }

    [Fact]
    public void RedcapProjectInfo_DeserializesFromWireNames()
    {
        var json = """{"project_id":"42","project_title":"My Study"}""";
        var info = JsonSerializer.Deserialize<RedcapProjectInfo>(json, RedcapJsonOptions.Default);
        Assert.NotNull(info);
        Assert.Equal("42", info.ProjectId);
        Assert.Equal("My Study", info.ProjectTitle);
    }

    [Fact]
    public void RedcapMetaData_DeserializesFromSnakeCaseProperties()
    {
        // RedcapMetaData uses snake_case property names directly rather than [JsonPropertyName]
        var json = """[{"field_name":"age","form_name":"demographics","field_type":"text","field_label":"Age"}]""";
        var metadata = JsonSerializer.Deserialize<List<RedcapMetaData>>(json, RedcapJsonOptions.Default);
        Assert.NotNull(metadata);
        Assert.Single(metadata);
        Assert.Equal("age", metadata[0].field_name);
        Assert.Equal("demographics", metadata[0].form_name);
        Assert.Equal("text", metadata[0].field_type);
        Assert.Equal("Age", metadata[0].field_label);
    }

    [Fact]
    public void FormEventMapping_DeserializesFromSnakeCaseProperties()
    {
        var json = """[{"arm_num":"1","form":"demographics","unique_event_name":"event_1_arm_1"}]""";
        var mappings = JsonSerializer.Deserialize<List<FormEventMapping>>(json, RedcapJsonOptions.Default);
        Assert.NotNull(mappings);
        Assert.Single(mappings);
        Assert.Equal("1", mappings[0].arm_num);
        Assert.Equal("demographics", mappings[0].form);
        Assert.Equal("event_1_arm_1", mappings[0].unique_event_name);
    }

    // ── Import (serialize) ───────────────────────────────────────────────────

    [Fact]
    public void RedcapArm_SerializesToWireNames()
    {
        var arm = new RedcapArm { ArmNumber = "1", Name = "Drug A" };
        var json = JsonSerializer.Serialize(arm, RedcapJsonOptions.Default);
        Assert.Contains("\"arm_num\"", json);
        Assert.Contains("\"name\"", json);
        Assert.DoesNotContain("\"ArmNumber\"", json);
        Assert.DoesNotContain("\"Name\"", json);
    }

    [Fact]
    public void ImportArmList_SerializesToWireNames()
    {
        var arms = new List<RedcapArm>
        {
            new() { ArmNumber = "1", Name = "Drug A" },
            new() { ArmNumber = "2", Name = "Drug B" },
        };
        var json = JsonSerializer.Serialize(arms, RedcapJsonOptions.Default);
        Assert.Contains("\"arm_num\"", json);
        Assert.Contains("\"Drug A\"", json);
        Assert.Contains("\"Drug B\"", json);
        Assert.DoesNotContain("\"ArmNumber\"", json);
    }

    [Fact]
    public void RedcapMetaData_SerializesToSnakeCaseKeys()
    {
        var field = new RedcapMetaData { field_name = "age", form_name = "demographics", field_type = "text" };
        var json = JsonSerializer.Serialize(field, RedcapJsonOptions.Default);
        Assert.Contains("\"field_name\"", json);
        Assert.Contains("\"form_name\"", json);
        Assert.Contains("\"field_type\"", json);
    }

    // ── Options ──────────────────────────────────────────────────────────────

    [Fact]
    public void RedcapJsonOptions_Default_IsCaseInsensitive()
    {
        Assert.True(RedcapJsonOptions.Default.PropertyNameCaseInsensitive);
    }

    [Fact]
    public void RedcapArm_CaseInsensitiveDeserialization_PopulatesProperties()
    {
        var json = """[{"ARM_NUM":"3","NAME":"Drug C"}]""";
        var arms = JsonSerializer.Deserialize<List<RedcapArm>>(json, RedcapJsonOptions.Default);
        Assert.NotNull(arms);
        Assert.Single(arms);
        Assert.Equal("3", arms[0].ArmNumber);
        Assert.Equal("Drug C", arms[0].Name);
    }
}
