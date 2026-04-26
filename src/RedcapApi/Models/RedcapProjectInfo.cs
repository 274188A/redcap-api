using System.Text.Json.Serialization;

namespace Redcap.Models;

/// <summary>
/// Minimum redcap project information when creating a project
/// 1.  Project Title
/// 2.  Purpose
/// 
/// </summary>
public class RedcapProjectInfo
{
    /// <summary>
    /// Project Identifier
    /// </summary>
    /// 
    [JsonPropertyName("project_id")]
    public string? ProjectId { get; set; }
    /// <summary>
    /// Title of project
    /// </summary>
    /// 
    [JsonPropertyName("project_title")]
    public string? ProjectTitle { get; set; }
    /// <summary>
    /// Purpose, i.e. 0, 1, 2, 3
    /// 0 = Pratice For Fun
    /// 1 = Other
    /// 2 = Research
    /// 3 = Quality Improvement
    /// 4 = Other
    /// </summary>
    /// 
    [JsonPropertyName("purpose")]
    public ProjectPurpose Purpose { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// 
    [JsonPropertyName("purpose_other")]
    public string? PurposeOther { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// 
    [JsonPropertyName("project_notes")]
    public string? ProjectNotes { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// 
    [JsonPropertyName("project_language")]
    public string? ProjectLanguage { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// 
    [JsonPropertyName("custom_record_label")]
    public string? CustomRecordLabel { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// 
    [JsonPropertyName("secondary_unique_field")]
    public string? SecondaryUniqueField { get; set; }
    /// <summary>
    /// 1: True, 0: False
    /// </summary>
    /// 
    [JsonPropertyName("is_longitudinal")]
    public int IsLongitudinal { get; set; }
    /// <summary>
    /// 1: True, 0: False
    /// </summary>
    /// 
    [JsonPropertyName("surveys_enabled")]
    public int SurveysEnabled { get; set; }
    /// <summary>
    /// 1: True, 0: False
    /// </summary>
    /// 
    [JsonPropertyName("scheduling_enabled")]
    public int SchedulingEnabled { get; set; }
    /// <summary>
    /// 1: True, 0: False
    /// </summary>
    /// 
    [JsonPropertyName("record_autonumbering_enabled")]
    public int RecordAutonumberingEnabled { get; set; }
    /// <summary>
    /// 1: True, 0: False
    /// </summary>
    /// 
    [JsonPropertyName("randomization_enabled")]
    public int RandomizationEnabled { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// 
    [JsonPropertyName("project_irb_number")]
    public string? ProjectIrbNumber { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// 
    [JsonPropertyName("project_grant_number")]
    public string? ProjectGrantNumber { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// 
    [JsonPropertyName("project_pi_firstname")]
    public string? ProjectPiFirstName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// 
    [JsonPropertyName("project_pi_lastname")]
    public string? ProjectPiLastName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// 
    [JsonPropertyName("display_today_now_button")]
    public bool DisplayTodayNowButton { get; set; }
}
