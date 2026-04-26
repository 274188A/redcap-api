using System.Text.Json.Serialization;

using System.ComponentModel.DataAnnotations;

namespace Redcap.Models;

/// <summary>
/// REDCap user-to-role mapping.
/// </summary>
public class RedcapUserRoleAssignment
{
    /// <summary>
    /// username
    /// </summary>
    [Display(Name = "username")]
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// unique role name
    /// </summary>
    [Display(Name = "unique_role_name")]
    [JsonPropertyName("unique_role_name")]
    public string? UniqueRoleName { get; set; }
}
