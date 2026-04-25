using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;

namespace Redcap.Models
{
    /// <summary>
    /// REDCap user role privileges.
    /// </summary>
    public class RedcapUserRole
    {
        /// <summary>
        /// unique role name
        /// </summary>
        [Display(Name = "unique_role_name")]
        [JsonProperty("unique_role_name")]
        public string? UniqueRoleName { get; set; }

        /// <summary>
        /// role label
        /// </summary>
        [Display(Name = "role_label")]
        [JsonProperty("role_label")]
        public string? RoleLabel { get; set; }

        /// <summary>
        /// design
        /// </summary>
        [Display(Name = "design")]
        [JsonProperty("design")]
        public string? Design { get; set; }

        /// <summary>
        /// user rights
        /// </summary>
        [Display(Name = "user_rights")]
        [JsonProperty("user_rights")]
        public string? UserRights { get; set; }

        /// <summary>
        /// data access groups
        /// </summary>
        [Display(Name = "data_access_groups")]
        [JsonProperty("data_access_groups")]
        public string? DataAccessGroups { get; set; }

        /// <summary>
        /// data export
        /// </summary>
        [Display(Name = "data_export")]
        [JsonProperty("data_export")]
        public string? DataExport { get; set; }

        /// <summary>
        /// reports
        /// </summary>
        [Display(Name = "reports")]
        [JsonProperty("reports")]
        public string? Reports { get; set; }

        /// <summary>
        /// stats and charts
        /// </summary>
        [Display(Name = "stats_and_charts")]
        [JsonProperty("stats_and_charts")]
        public string? StatsAndCharts { get; set; }

        /// <summary>
        /// manage survey participants
        /// </summary>
        [Display(Name = "manage_survey_participants")]
        [JsonProperty("manage_survey_participants")]
        public string? ManageSurveyParticipants { get; set; }

        /// <summary>
        /// calendar
        /// </summary>
        [Display(Name = "calendar")]
        [JsonProperty("calendar")]
        public string? Calendar { get; set; }

        /// <summary>
        /// data import tool
        /// </summary>
        [Display(Name = "data_import_tool")]
        [JsonProperty("data_import_tool")]
        public string? DataImportTool { get; set; }

        /// <summary>
        /// data comparison tool
        /// </summary>
        [Display(Name = "data_comparison_tool")]
        [JsonProperty("data_comparison_tool")]
        public string? DataComparisonTool { get; set; }

        /// <summary>
        /// logging
        /// </summary>
        [Display(Name = "logging")]
        [JsonProperty("logging")]
        public string? Logging { get; set; }

        /// <summary>
        /// file repository
        /// </summary>
        [Display(Name = "file_repository")]
        [JsonProperty("file_repository")]
        public string? FileRepository { get; set; }

        /// <summary>
        /// data quality create
        /// </summary>
        [Display(Name = "data_quality_create")]
        [JsonProperty("data_quality_create")]
        public string? DataQualityCreate { get; set; }

        /// <summary>
        /// data quality execute
        /// </summary>
        [Display(Name = "data_quality_execute")]
        [JsonProperty("data_quality_execute")]
        public string? DataQualityExecute { get; set; }

        /// <summary>
        /// api export
        /// </summary>
        [Display(Name = "api_export")]
        [JsonProperty("api_export")]
        public string? ApiExport { get; set; }

        /// <summary>
        /// api import
        /// </summary>
        [Display(Name = "api_import")]
        [JsonProperty("api_import")]
        public string? ApiImport { get; set; }

        /// <summary>
        /// mobile app
        /// </summary>
        [Display(Name = "mobile_app")]
        [JsonProperty("mobile_app")]
        public string? MobileApp { get; set; }

        /// <summary>
        /// mobile app download data
        /// </summary>
        [Display(Name = "mobile_app_download_data")]
        [JsonProperty("mobile_app_download_data")]
        public string? MobileAppDownloadData { get; set; }

        /// <summary>
        /// record create
        /// </summary>
        [Display(Name = "record_create")]
        [JsonProperty("record_create")]
        public string? RecordCreate { get; set; }

        /// <summary>
        /// record rename
        /// </summary>
        [Display(Name = "record_rename")]
        [JsonProperty("record_rename")]
        public string? RecordRename { get; set; }

        /// <summary>
        /// record delete
        /// </summary>
        [Display(Name = "record_delete")]
        [JsonProperty("record_delete")]
        public string? RecordDelete { get; set; }

        /// <summary>
        /// lock records customization
        /// </summary>
        [Display(Name = "lock_records_customization")]
        [JsonProperty("lock_records_customization")]
        public string? LockRecordsCustomization { get; set; }

        /// <summary>
        /// lock records
        /// </summary>
        [Display(Name = "lock_records")]
        [JsonProperty("lock_records")]
        public string? LockRecords { get; set; }

        /// <summary>
        /// lock records all forms
        /// </summary>
        [Display(Name = "lock_records_all_forms")]
        [JsonProperty("lock_records_all_forms")]
        public string? LockRecordsAllForms { get; set; }

        /// <summary>
        /// form-level rights for each instrument
        /// </summary>
        [Display(Name = "forms")]
        [JsonProperty("forms")]
        public Dictionary<string, string>? Forms { get; set; }
    }
}
