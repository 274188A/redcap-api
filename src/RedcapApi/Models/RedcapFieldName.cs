using System.Text.Json.Serialization;

namespace Redcap.Models;

/// <summary>
/// Export/import-specific field name returned by REDCap.
/// </summary>
public class RedcapFieldName
{
    /// <summary>
    /// Original project field name.
    /// </summary>
    [JsonPropertyName("original_field_name")]
    public string? OriginalFieldName { get; set; }

    /// <summary>
    /// Raw coded checkbox choice value, or blank for non-checkbox fields.
    /// </summary>
    [JsonPropertyName("choice_value")]
    public string? ChoiceValue { get; set; }

    /// <summary>
    /// Field name used when exporting or importing data.
    /// </summary>
    [JsonPropertyName("export_field_name")]
    public string? ExportFieldName { get; set; }
}
