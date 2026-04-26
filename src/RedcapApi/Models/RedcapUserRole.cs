using System.Text.Json.Serialization;

using System.ComponentModel.DataAnnotations;

namespace Redcap.Models;

/// <summary>
/// REDCap user role privileges.
/// </summary>
public class RedcapUserRole
{
    /// <summary>
    /// unique role name
    /// </summary>
    [Display(Name = "unique_role_name")]
    [JsonPropertyName("unique_role_name")]
    public string? UniqueRoleName { get; set; }

    /// <summary>
    /// role label
    /// </summary>
    [Display(Name = "role_label")]
    [JsonPropertyName("role_label")]
    public string? RoleLabel { get; set; }

    /// <summary>
    /// design
    /// </summary>
    [Display(Name = "design")]
    [JsonPropertyName("design")]
    public string? Design { get; set; }

    /// <summary>
    /// user rights
    /// </summary>
    [Display(Name = "user_rights")]
    [JsonPropertyName("user_rights")]
    public string? UserRights { get; set; }

    /// <summary>
    /// data access groups
    /// </summary>
    [Display(Name = "data_access_groups")]
    [JsonPropertyName("data_access_groups")]
    public string? DataAccessGroups { get; set; }

    /// <summary>
    /// data export
    /// </summary>
    [Display(Name = "data_export")]
    [JsonPropertyName("data_export")]
    public string? DataExport { get; set; }

    /// <summary>
    /// reports
    /// </summary>
    [Display(Name = "reports")]
    [JsonPropertyName("reports")]
    public string? Reports { get; set; }

    /// <summary>
    /// stats and charts
    /// </summary>
    [Display(Name = "stats_and_charts")]
    [JsonPropertyName("stats_and_charts")]
    public string? StatsAndCharts { get; set; }

    /// <summary>
    /// manage survey participants
    /// </summary>
    [Display(Name = "manage_survey_participants")]
    [JsonPropertyName("manage_survey_participants")]
    public string? ManageSurveyParticipants { get; set; }

    /// <summary>
    /// calendar
    /// </summary>
    [Display(Name = "calendar")]
    [JsonPropertyName("calendar")]
    public string? Calendar { get; set; }

    /// <summary>
    /// data import tool
    /// </summary>
    [Display(Name = "data_import_tool")]
    [JsonPropertyName("data_import_tool")]
    public string? DataImportTool { get; set; }

    /// <summary>
    /// data comparison tool
    /// </summary>
    [Display(Name = "data_comparison_tool")]
    [JsonPropertyName("data_comparison_tool")]
    public string? DataComparisonTool { get; set; }

    /// <summary>
    /// logging
    /// </summary>
    [Display(Name = "logging")]
    [JsonPropertyName("logging")]
    public string? Logging { get; set; }

    /// <summary>
    /// file repository
    /// </summary>
    [Display(Name = "file_repository")]
    [JsonPropertyName("file_repository")]
    public string? FileRepository { get; set; }

    /// <summary>
    /// data quality create
    /// </summary>
    [Display(Name = "data_quality_create")]
    [JsonPropertyName("data_quality_create")]
    public string? DataQualityCreate { get; set; }

    /// <summary>
    /// data quality execute
    /// </summary>
    [Display(Name = "data_quality_execute")]
    [JsonPropertyName("data_quality_execute")]
    public string? DataQualityExecute { get; set; }

    /// <summary>
    /// api export
    /// </summary>
    [Display(Name = "api_export")]
    [JsonPropertyName("api_export")]
    public string? ApiExport { get; set; }

    /// <summary>
    /// api import
    /// </summary>
    [Display(Name = "api_import")]
    [JsonPropertyName("api_import")]
    public string? ApiImport { get; set; }

    /// <summary>
    /// mobile app
    /// </summary>
    [Display(Name = "mobile_app")]
    [JsonPropertyName("mobile_app")]
    public string? MobileApp { get; set; }

    /// <summary>
    /// mobile app download data
    /// </summary>
    [Display(Name = "mobile_app_download_data")]
    [JsonPropertyName("mobile_app_download_data")]
    public string? MobileAppDownloadData { get; set; }

    /// <summary>
    /// record create
    /// </summary>
    [Display(Name = "record_create")]
    [JsonPropertyName("record_create")]
    public string? RecordCreate { get; set; }

    /// <summary>
    /// record rename
    /// </summary>
    [Display(Name = "record_rename")]
    [JsonPropertyName("record_rename")]
    public string? RecordRename { get; set; }

    /// <summary>
    /// record delete
    /// </summary>
    [Display(Name = "record_delete")]
    [JsonPropertyName("record_delete")]
    public string? RecordDelete { get; set; }

    /// <summary>
    /// lock records customization
    /// </summary>
    [Display(Name = "lock_records_customization")]
    [JsonPropertyName("lock_records_customization")]
    public string? LockRecordsCustomization { get; set; }

    /// <summary>
    /// lock records
    /// </summary>
    [Display(Name = "lock_records")]
    [JsonPropertyName("lock_records")]
    public string? LockRecords { get; set; }

    /// <summary>
    /// lock records all forms
    /// </summary>
    [Display(Name = "lock_records_all_forms")]
    [JsonPropertyName("lock_records_all_forms")]
    public string? LockRecordsAllForms { get; set; }

    /// <summary>
    /// form-level rights for each instrument
    /// </summary>
    [Display(Name = "forms")]
    [JsonPropertyName("forms")]
    public Dictionary<string, string>? Forms { get; set; }
}
