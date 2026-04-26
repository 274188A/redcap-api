using System.Text.Json.Serialization;

namespace Redcap.Models;

/// <summary>
/// Arms are only available for longitudinal projects.
/// </summary>
public class RedcapArm
{
    /// <summary>
    /// Number associated with the event, e.g "1"
    /// </summary>
    /// 
    [JsonPropertyName("arm_num")]
    public string? ArmNumber { get; set; }
    /// <summary>
    /// Name of the event. e.g "event1_arm_1"
    /// </summary>
    /// 
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
