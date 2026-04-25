using Newtonsoft.Json;

using Redcap.Api;
using Redcap.Exceptions;
using Redcap.Interfaces;
using Redcap.Models;
using Redcap.Utilities;

using Serilog;

using System.Net.Http.Headers;

using static System.String;

namespace Redcap
{
    public partial class RedcapApi
    {

        /// <summary>
        /// From Redcap Version 6.11.0<br/><br/>
        ///
        /// Create A New Project<br/>
        ///
        /// This method allows you to create a new REDCap project. A 64-character Super API Token is required for this method (as opposed to project-level API methods that require a regular 32-character token associated with the project-user). In the API request, you must minimally provide the project attributes 'project_title' and 'purpose' (with numerical value 0=Practice/Just for fun, 1=Other, 2=Research, 3=Quality Improvement, 4=Operational Support) when creating a project.
        /// When a project is created with this method, the project will automatically be given all the project-level defaults just as if you created a new empty project via the web user interface, such as a automatically creating a single data collection instrument seeded with a single Record ID field and Form Status field, as well as (for longitudinal projects) one arm with one event. And if you intend to create your own arms or events immediately after creating the project, it is recommended that you utilize the override=1 parameter in the 'Import Arms' or 'Import Events' method, respectively, so that the default arm and event are removed when you add your own.Also, the user creating the project will automatically be added to the project as a user with full user privileges and a project-level API token, which could then be used for subsequent project-level API requests.
        /// NOTE: Only users with Super API Tokens can utilize this method.Users can only be granted a super token by a REDCap administrator(using the API Tokens page in the REDCap Control Center). Please be advised that users with a Super API Token can create new REDCap projects via the API without any approval needed by a REDCap administrator.If you are interested in obtaining a super token, please contact your local REDCap administrator.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have a Super API Token.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="data">
        ///     Contains the attributes of the project to be created, in which they are provided in the specified format. While the only required attributes are 'project_title' and 'purpose', the fields listed below are all the possible attributes that can be provided in the 'data' parameter. The 'purpose' attribute must have a numerical value (0=Practice/Just for fun, 1=Other, 2=Research, 3=Quality Improvement, 4=Operational Support), in which 'purpose_other' is only required to have a value (as a text string) if purpose=1. The attributes is_longitudinal (0=False, 1=True; Default=0), surveys_enabled (0=False, 1=True; Default=0), and record_autonumbering_enabled (0=False, 1=True; Default=1) are all boolean. Please note that either is_longitudinal=1 or surveys_enabled=1 does not add arms/events or surveys to the project, respectively, but it merely enables those settings which are seen at the top of the project's Project Setup page.
        ///     All available attributes:
        ///     project_title, purpose, purpose_other, project_notes, is_longitudinal, surveys_enabled, record_autonumbering_enabled
        ///     JSON Example:
        ///     [{"project_title":"My New REDCap Project","purpose":"0"}]
        /// </param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="odm">default: NULL - The 'odm' parameter must be an XML string in CDISC ODM XML format that contains project metadata (fields, forms, events, arms) and might optionally contain data to be imported as well. The XML contained in this parameter can come from a REDCap Project XML export file from REDCap itself, or may come from another system that is capable of exporting projects and data in CDISC ODM format. If the 'odm' parameter is included in the API request, it will use the XML to import its contents into the newly created project. This will allow you not only to create the project with the API request, but also to import all fields, forms, and project attributes (and events and arms, if longitudinal) as well as record data all at the same time.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>When a project is created, a 32-character project-level API Token is returned (associated with both the project and user creating the project). This token could then ostensibly be used to make subsequent API calls to this project, such as for adding new events, fields, records, etc.</returns>
        public async Task<string> CreateProjectAsync<T>(RedcapFormat format, List<T> data,
            RedcapReturnFormat returnFormat = RedcapReturnFormat.json, string? odm = null,
            CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(payload =>
            {
                payload["token"] = _token;
                payload["content"] = Content.Project.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                payload["returnFormat"] = returnFormat.GetDisplayName();
                payload["data"] = JsonConvert.SerializeObject(data);
                if (!IsNullOrEmpty(odm)) payload["odm"] = odm;
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// Import Project Information<br/><br/>
        /// This method allows you to update some of the basic attributes of a given REDCap project, such as the project's title, if it is longitudinal, if surveys are enabled, etc. Its data format corresponds to the format in the API method Export Project Information.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Import/Update privileges in the project.
        /// </remarks>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="projectInfo">Contains some or all of the attributes from Export Project Information in the same data format as in the export. These attributes will change the project information.
        /// Attributes for the project in the format specified. For any values that are boolean, they should be represented as either a '0' (no/false) or '1' (yes/true). The following project attributes can be udpated:
        /// project_title, project_language, purpose, purpose_other, project_notes, custom_record_label, secondary_unique_field, is_longitudinal, surveys_enabled, scheduling_enabled, record_autonumbering_enabled, randomization_enabled, project_irb_number, project_grant_number, project_pi_firstname, project_pi_lastname, display_today_now_button
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Returns the number of values accepted to be updated in the project settings (including values which remained the same before and after the import).</returns>
        public async Task<string> ImportProjectInfoAsync(RedcapFormat format, RedcapProjectInfo projectInfo, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(payload =>
            {
                payload["token"] = _token;
                payload["content"] = Content.ProjectSettings.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                payload["data"] = JsonConvert.SerializeObject(projectInfo);
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// Export Project Information <br/><br/>
        /// This method allows you to export some of the basic attributes of a given REDCap project, such as the project's title, if it is longitudinal, if surveys are enabled, the time the project was created and moved to production, etc.
        /// </summary>
        /// <remarks>
        ///
        /// To use this method, you must have API Export privileges in the project.
        /// </remarks>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>
        /// Attributes for the project in the format specified. For any values that are boolean, they will be represented as either a '0' (no/false) or '1' (yes/true). Also, all date/time values will be returned in Y-M-D H:M:S format. The following attributes will be returned:
        /// project_id, project_title, creation_time, production_time, in_production, project_language, purpose, purpose_other, project_notes, custom_record_label, secondary_unique_field, is_longitudinal, surveys_enabled, scheduling_enabled, record_autonumbering_enabled, randomization_enabled, ddp_enabled, project_irb_number, project_grant_number, project_pi_firstname, project_pi_lastname, display_today_now_button
        /// </returns>
        public async Task<string> ExportProjectInfoAsync(RedcapFormat format = RedcapFormat.json, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(payload =>
            {
                payload["token"] = _token;
                payload["content"] = Content.Project.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                payload["returnFormat"] = returnFormat.GetDisplayName();
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap Version 6.12.0 <br/>
        ///
        /// Export Entire Project as REDCap XML File (containing metadata and data) <br/>
        /// The entire project(all records, events, arms, instruments, fields, and project attributes) can be downloaded as a single XML file, which is in CDISC ODM format(ODM version 1.3.1). This XML file can be used to create a clone of the project(including its data, optionally) on this REDCap server or on another REDCap server (it can be uploaded on the Create New Project page). Because it is in CDISC ODM format, it can also be used to import the project into another ODM-compatible system. NOTE: All the option paramters listed below ONLY apply to data returned if the 'returnMetadataOnly' parameter is set to FALSE (default). For this API method, ALL metadata (all fields, forms, events, and arms) will always be exported.Only the data returned can be filtered using the optional parameters.
        /// Note about export rights: If the 'returnMetadataOnly' parameter is set to FALSE, then please be aware that Data Export user rights will be applied to any data returned from this API request. For example, if you have 'De-Identified' or 'Remove all tagged Identifier fields' data export rights, then some data fields *might* be removed and filtered out of the data set returned from the API. To make sure that no data is unnecessarily filtered out of your API request, you should have 'Full Data Set' export rights in the project.
        /// </summary>
        /// <remarks>
        ///
        /// To use this method, you must have API Export privileges in the project.
        /// </remarks>
        /// <param name="returnMetadataOnly">true, false [default] - TRUE returns only metadata (all fields, forms, events, and arms), whereas FALSE returns all metadata and also data (and optionally filters the data according to any of the optional parameters provided in the request) </param>
        /// <param name="records">an array of record names specifying specific records you wish to pull (by default, all records are pulled)</param>
        /// <param name="fields">an array of field names specifying specific fields you wish to pull (by default, all fields are pulled)</param>
        /// <param name="events">an array of unique event names that you wish to pull records for - only for longitudinal projects</param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="exportSurveyFields">true, false [default] - specifies whether or not to export the survey identifier field (e.g., 'redcap_survey_identifier') or survey timestamp fields (e.g., instrument+'_timestamp') when surveys are utilized in the project. If you do not pass in this flag, it will default to 'false'. If set to 'true', it will return the redcap_survey_identifier field and also the survey timestamp field for a particular survey when at least one field from that survey is being exported. NOTE: If the survey identifier field or survey timestamp fields are imported via API data import, they will simply be ignored since they are not real fields in the project but rather are pseudo-fields.</param>
        /// <param name="exportDataAccessGroups">true, false [default] - specifies whether or not to export the 'redcap_data_access_group' field when data access groups are utilized in the project. If you do not pass in this flag, it will default to 'false'. NOTE: This flag is only viable if the user whose token is being used to make the API request is *not* in a data access group. If the user is in a group, then this flag will revert to its default value.</param>
        /// <param name="filterLogic">String of logic text (e.g., [age] > 30) for filtering the data to be returned by this API method, in which the API will only return the records (or record-events, if a longitudinal project) where the logic evaluates as TRUE. This parameter is blank/null by default unless a value is supplied. Please note that if the filter logic contains any incorrect syntax, the API will respond with an error message. </param>
        /// <param name="exportFiles">true, false [default] - TRUE will cause the XML returned to include all files uploaded for File Upload and Signature fields for all records in the project, whereas FALSE will cause all such fields not to be included. NOTE: Setting this option to TRUE can make the export very large and may prevent it from completing if the project contains many files or very large files. </param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>The entire REDCap project's metadata (and data, if specified) will be returned in CDISC ODM format as a single XML string.</returns>
        public async Task<string> ExportProjectXmlAsync(bool returnMetadataOnly = false, string[]? records = default, string[]? fields = default, string[]? events = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, bool exportSurveyFields = false, bool exportDataAccessGroups = false, string? filterLogic = default, bool exportFiles = false, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            var recordsStr = records?.Length > 0 ? this.ConvertArraytoString(records) : null;
            var eventsStr = events?.Length > 0 ? this.ConvertArraytoString(events) : null;
            return await ExecuteAsync(payload =>
            {
                payload["token"] = _token;
                payload["content"] = Content.ProjectXml.GetDisplayName();
                payload["returnFormat"] = returnFormat.GetDisplayName();
                if (returnMetadataOnly) payload["returnMetadataOnly"] = returnMetadataOnly.ToString();
                if (recordsStr != null) payload["records"] = recordsStr;
                if (eventsStr != null) payload["events"] = eventsStr;
                if (exportSurveyFields) payload["exportSurveyFields"] = exportSurveyFields.ToString();
                if (exportDataAccessGroups) payload["exportDataAccessGroups"] = exportDataAccessGroups.ToString();
                if (!IsNullOrEmpty(filterLogic)) payload["filterLogic"] = filterLogic;
                if (exportFiles) payload["exportFiles"] = exportFiles.ToString();
            }, cancellationToken, timeOutSeconds);
        }

    }
}
