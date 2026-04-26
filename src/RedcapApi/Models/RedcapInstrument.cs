using System.Text.Json.Serialization;

namespace Redcap.Models;

/// <summary>
/// Instrument in a redcap project
/// </summary>
public class RedcapInstrument
{
    /// <summary>
    /// Name of the instrument
    /// </summary>
    [JsonPropertyName("instrument_name")]
    public string? InstrumentName { get; set; }

    /// <summary>
    /// Label (display)
    /// </summary>
    [JsonPropertyName("instrument_label")]
    public string? InstrumentLabel { get; set; }

}
