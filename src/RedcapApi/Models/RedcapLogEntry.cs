using System.Text.Json.Serialization;

namespace Redcap.Models;

/// <summary>
/// Logging/audit entry returned by REDCap.
/// </summary>
public class RedcapLogEntry
{
    /// <summary>
    /// REDCap log timestamp.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    /// <summary>
    /// Username associated with the log entry.
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// Logged action/event type.
    /// </summary>
    [JsonPropertyName("action")]
    public string? Action { get; set; }

    /// <summary>
    /// REDCap record associated with the entry, when applicable.
    /// </summary>
    [JsonPropertyName("record")]
    public string? Record { get; set; }

    /// <summary>
    /// REDCap instrument associated with the entry, when applicable.
    /// </summary>
    [JsonPropertyName("instrument")]
    public string? Instrument { get; set; }

    /// <summary>
    /// REDCap event associated with the entry, when applicable.
    /// </summary>
    [JsonPropertyName("event")]
    public string? Event { get; set; }

    /// <summary>
    /// Data access group associated with the entry, when applicable.
    /// </summary>
    [JsonPropertyName("dag")]
    public string? Dag { get; set; }

    /// <summary>
    /// Human-readable details for the logged action.
    /// </summary>
    [JsonPropertyName("details")]
    public string? Details { get; set; }
}
