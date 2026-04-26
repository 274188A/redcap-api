using System.Text.Json;

namespace Redcap.Utilities;

/// <summary>Shared <see cref="JsonSerializerOptions"/> used for all REDCap JSON serialization and deserialization.</summary>
public static class RedcapJsonOptions
{
    /// <summary>Case-insensitive options instance shared across all typed API methods.</summary>
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
