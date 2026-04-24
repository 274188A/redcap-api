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
        /// From Redcap Version 6.18.0 <br/>
        /// Generate Next Record Name<br/>
        /// To be used by projects with record auto-numbering enabled, this method exports the next potential record ID for a project. It generates the next record name by determining the current maximum numerical record ID and then incrementing it by one.
        /// Note: This method does not create a new record, but merely determines what the next record name would be.
        /// If using Data Access Groups (DAGs) in the project, this method accounts for the special formatting of the record name for users in DAGs (e.g., DAG-ID); in this case, it only assigns the next value for ID for all numbers inside a DAG. For example, if a DAG has a corresponding DAG number of 223 wherein records 223-1 and 223-2 already exist, then the next record will be 223-3 if the API user belongs to the DAG that has DAG number 223. (The DAG number is auto-assigned by REDCap for each DAG when the DAG is first created.) When generating a new record name in a DAG, the method considers all records in the entire project when determining the maximum record ID, including those that might have been originally created in that DAG but then later reassigned to another DAG.
        /// Note: This method functions the same even for projects that do not have record auto-numbering enabled.
        /// </summary>
        /// 
        /// <remarks>
        /// To use this method, you must have API Export privileges in the project.
        /// </remarks>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>The maximum integer record ID + 1.</returns>
        public async Task<string> GenerateNextRecordNameAsync(string token, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["content"] = Content.GenerateNextRecordName.GetDisplayName();
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap Version 11.4.0<br/>
        /// Export Records<br/>
        /// This method allows you to export a set of records for a project.
        /// Note about export rights: Please be aware that Data Export user rights will be applied to this API request.For example, if you have 'No Access' data export rights in the project, then the API data export will fail and return an error. And if you have 'De-Identified' or 'Remove all tagged Identifier fields' data export rights, then some data fields *might* be removed and filtered out of the data set returned from the API. To make sure that no data is unnecessarily filtered out of your API request, you should have 'Full Data Set' export rights in the project.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Export privileges in the project.
        /// </remarks>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="format">csv, json [default], xml, odm ('odm' refers to CDISC ODM XML format, specifically ODM version 1.3.1)</param>
        /// <param name="redcapDataType">flat - output as one record per row [default], eav - output as one data point per row. Non-longitudinal: Will have the fields - record*, field_name, value. Longitudinal: Will have the fields - record*, field_name, value, redcap_event_name</param>
        /// <param name="records">an array of record names specifying specific records you wish to pull (by default, all records are pulled)</param>
        /// <param name="fields">an array of field names specifying specific fields you wish to pull (by default, all fields are pulled)</param>
        /// <param name="forms">an array of form names you wish to pull records for. If the form name has a space in it, replace the space with an underscore (by default, all records are pulled)</param>
        /// <param name="events">an array of unique event names that you wish to pull records for - only for longitudinal projects</param>
        /// <param name="rawOrLabel">raw [default], label - export the raw coded values or labels for the options of multiple choice fields</param>
        /// <param name="rawOrLabelHeaders">raw [default], label - (for 'csv' format 'flat' type only) for the CSV headers, export the variable/field names (raw) or the field labels (label)</param>
        /// <param name="exportCheckboxLabel">true, false [default] - specifies the format of checkbox field values specifically when exporting the data as labels (i.e., when rawOrLabel=label) in flat format (i.e., when type=flat). When exporting labels, by default (without providing the exportCheckboxLabel flag or if exportCheckboxLabel=false), all checkboxes will either have a value 'Checked' if they are checked or 'Unchecked' if not checked. But if exportCheckboxLabel is set to true, it will instead export the checkbox value as the checkbox option's label (e.g., 'Choice 1') if checked or it will be blank/empty (no value) if not checked. If rawOrLabel=false or if type=eav, then the exportCheckboxLabel flag is ignored. (The exportCheckboxLabel parameter is ignored for type=eav because 'eav' type always exports checkboxes differently anyway, in which checkboxes are exported with their true variable name (whereas the 'flat' type exports them as variable___code format), and another difference is that 'eav' type *always* exports checkbox values as the choice label for labels export, or as 0 or 1 (if unchecked or checked, respectively) for raw export.)</param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="exportSurveyFields">true, false [default] - specifies whether or not to export the survey identifier field (e.g., 'redcap_survey_identifier') or survey timestamp fields (e.g., instrument+'_timestamp') when surveys are utilized in the project. If you do not pass in this flag, it will default to 'false'. If set to 'true', it will return the redcap_survey_identifier field and also the survey timestamp field for a particular survey when at least one field from that survey is being exported. NOTE: If the survey identifier field or survey timestamp fields are imported via API data import, they will simply be ignored since they are not real fields in the project but rather are pseudo-fields.</param>
        /// <param name="exportDataAccessGroups">true, false [default] - specifies whether or not to export the 'redcap_data_access_group' field when data access groups are utilized in the project. If you do not pass in this flag, it will default to 'false'. NOTE: This flag is only viable if the user whose token is being used to make the API request is *not* in a data access group. If the user is in a group, then this flag will revert to its default value.</param>
        /// <param name="filterLogic">String of logic text (e.g., [age] > 30) for filtering the data to be returned by this API method, in which the API will only return the records (or record-events, if a longitudinal project) where the logic evaluates as TRUE. This parameter is blank/null by default unless a value is supplied. Please note that if the filter logic contains any incorrect syntax, the API will respond with an error message. </param>
        /// <param name="dateRangeBegin">To return only records that have been created or modified *after* a given date/time, provide a timestamp in the format YYYY-MM-DD HH:MM:SS (e.g., '2017-01-01 00:00:00' for January 1, 2017 at midnight server time). If not specified, it will assume no begin time. </param>
        /// <param name="dateRangeEnd">To return only records that have been created or modified *before* a given date/time, provide a timestamp in the format YYYY-MM-DD HH:MM:SS (e.g., '2017-01-01 00:00:00' for January 1, 2017 at midnight server time). If not specified, it will use the current server time. </param>
        /// <param name="csvDelimiter">Set the delimiter used to separate values in the CSV data file (for CSV format only). Options include: comma ',' (default), 'tab', semi-colon ';', pipe '|', or caret '^'. Simply provide the value in quotes for this parameter.</param>
        /// <param name="decimalCharacter">If specified, force all numbers into same decimal format. You may choose to force all data values containing a decimal to have the same decimal character, which will be applied to all calc fields and number-validated text fields. Options include comma ',' or dot/full stop '.', but if left blank/null, then it will export numbers using the fields' native decimal format. Simply provide the value of either ',' or '.' for this parameter.</param>
        /// <param name="exportBlankForGrayFormStatus">true, false [default] - specifies whether or not to export blank values for instrument complete status fields that have a gray status icon. All instrument complete status fields having a gray icon can be exported either as a blank value or as "0" (Incomplete). Blank values are recommended in a data export if the data will be re-imported into a REDCap project.</param>
        /// <param name="combineCheckboxOptions">true, false [default] - for checkbox fields, specifies whether all checked options are combined into a single delimited value when exporting labels.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Data from the project in the format and type specified ordered by the record (primary key of project) and then by event id</returns>
        public async Task<string> ExportRecordsAsync(string token, RedcapFormat format = RedcapFormat.json, RedcapDataType redcapDataType = RedcapDataType.flat, string[]? records = default, string[]? fields = default, string[]? forms = default, string[]? events = default, RawOrLabel rawOrLabel = RawOrLabel.raw, RawOrLabelHeaders rawOrLabelHeaders = RawOrLabelHeaders.raw, bool exportCheckboxLabel = false, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, bool exportSurveyFields = false, bool exportDataAccessGroups = false, string? filterLogic = null, DateTime? dateRangeBegin = default, DateTime? dateRangeEnd = default, CsvDelimiter csvDelimiter = CsvDelimiter.comma, DecimalCharacter decimalCharacter = DecimalCharacter.none, bool exportBlankForGrayFormStatus = false, bool combineCheckboxOptions = false, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            var recordsStr = records?.Length > 0 ? this.ConvertArraytoString(records) : null;
            var fieldsStr = fields?.Length > 0 ? this.ConvertArraytoString(fields) : null;
            var formsStr = forms?.Length > 0 ? this.ConvertArraytoString(forms) : null;
            var eventsStr = events?.Length > 0 ? this.ConvertArraytoString(events) : null;
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["content"] = Content.Record.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                payload["returnFormat"] = returnFormat.GetDisplayName();
                payload["type"] = redcapDataType.GetDisplayName();
                payload["exportBlankForGrayFormStatus"] = exportBlankForGrayFormStatus.ToString();
                if (recordsStr != null) payload["records"] = recordsStr;
                if (fieldsStr != null) payload["fields"] = fieldsStr;
                if (formsStr != null) payload["forms"] = formsStr;
                if (eventsStr != null) payload["events"] = eventsStr;
                var _rawOrLabel = rawOrLabel.ToString();
                if (!IsNullOrEmpty(_rawOrLabel)) payload["rawOrLabel"] = _rawOrLabel;
                var _rawOrLabelHeaders = rawOrLabelHeaders.ToString();
                if (!IsNullOrEmpty(_rawOrLabelHeaders)) payload["rawOrLabelHeaders"] = _rawOrLabelHeaders;
                if (exportCheckboxLabel) payload["exportCheckboxLabel"] = exportCheckboxLabel.ToString();
                if (exportSurveyFields) payload["exportSurveyFields"] = exportSurveyFields.ToString();
                if (exportDataAccessGroups) payload["exportDataAccessGroups"] = exportDataAccessGroups.ToString();
                if (!IsNullOrEmpty(filterLogic)) payload["filterLogic"] = filterLogic!;
                if (dateRangeBegin.HasValue) payload["dateRangeBegin"] = dateRangeBegin.Value.ToString("yyyy-MM-dd HH:mm:ss");
                if (dateRangeEnd.HasValue) payload["dateRangeEnd"] = dateRangeEnd.Value.ToString("yyyy-MM-dd HH:mm:ss");
                if (format == RedcapFormat.csv) payload["csvDelimiter"] = csvDelimiter.ToString();
                if (decimalCharacter != DecimalCharacter.none) payload["decimalCharacter"] = decimalCharacter.ToString();
                if (combineCheckboxOptions) payload["combineCheckboxOptions"] = combineCheckboxOptions.ToString();
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap Version 11.4.0<br/>
        /// Export Record<br/>
        /// This method allows you to export a single record for a project.
        /// Note about export rights: Please be aware that Data Export user rights will be applied to this API request.For example, if you have 'No Access' data export rights in the project, then the API data export will fail and return an error. And if you have 'De-Identified' or 'Remove all tagged Identifier fields' data export rights, then some data fields *might* be removed and filtered out of the data set returned from the API. To make sure that no data is unnecessarily filtered out of your API request, you should have 'Full Data Set' export rights in the project.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Export privileges in the project.
        /// </remarks>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="format">csv, json [default], xml, odm ('odm' refers to CDISC ODM XML format, specifically ODM version 1.3.1)</param>
        /// <param name="redcapDataType">flat - output as one record per row [default], eav - output as one data point per row. Non-longitudinal: Will have the fields - record*, field_name, value. Longitudinal: Will have the fields - record*, field_name, value, redcap_event_name</param>
        /// <param name="record">a single record specifying specific records you wish to pull (by default, all records are pulled)</param>
        /// <param name="fields">an array of field names specifying specific fields you wish to pull (by default, all fields are pulled)</param>
        /// <param name="forms">an array of form names you wish to pull records for. If the form name has a space in it, replace the space with an underscore (by default, all records are pulled)</param>
        /// <param name="events">an array of unique event names that you wish to pull records for - only for longitudinal projects</param>
        /// <param name="rawOrLabel">raw [default], label - export the raw coded values or labels for the options of multiple choice fields</param>
        /// <param name="rawOrLabelHeaders">raw [default], label - (for 'csv' format 'flat' type only) for the CSV headers, export the variable/field names (raw) or the field labels (label)</param>
        /// <param name="exportCheckboxLabel">true, false [default] - specifies the format of checkbox field values specifically when exporting the data as labels (i.e., when rawOrLabel=label) in flat format (i.e., when type=flat). When exporting labels, by default (without providing the exportCheckboxLabel flag or if exportCheckboxLabel=false), all checkboxes will either have a value 'Checked' if they are checked or 'Unchecked' if not checked. But if exportCheckboxLabel is set to true, it will instead export the checkbox value as the checkbox option's label (e.g., 'Choice 1') if checked or it will be blank/empty (no value) if not checked. If rawOrLabel=false or if type=eav, then the exportCheckboxLabel flag is ignored. (The exportCheckboxLabel parameter is ignored for type=eav because 'eav' type always exports checkboxes differently anyway, in which checkboxes are exported with their true variable name (whereas the 'flat' type exports them as variable___code format), and another difference is that 'eav' type *always* exports checkbox values as the choice label for labels export, or as 0 or 1 (if unchecked or checked, respectively) for raw export.)</param>
        /// <param name="onErrorFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="exportSurveyFields">true, false [default] - specifies whether or not to export the survey identifier field (e.g., 'redcap_survey_identifier') or survey timestamp fields (e.g., instrument+'_timestamp') when surveys are utilized in the project. If you do not pass in this flag, it will default to 'false'. If set to 'true', it will return the redcap_survey_identifier field and also the survey timestamp field for a particular survey when at least one field from that survey is being exported. NOTE: If the survey identifier field or survey timestamp fields are imported via API data import, they will simply be ignored since they are not real fields in the project but rather are pseudo-fields.</param>
        /// <param name="exportDataAccessGroups">true, false [default] - specifies whether or not to export the 'redcap_data_access_group' field when data access groups are utilized in the project. If you do not pass in this flag, it will default to 'false'. NOTE: This flag is only viable if the user whose token is being used to make the API request is *not* in a data access group. If the user is in a group, then this flag will revert to its default value.</param>
        /// <param name="filterLogic">String of logic text (e.g., [age] > 30) for filtering the data to be returned by this API method, in which the API will only return the records (or record-events, if a longitudinal project) where the logic evaluates as TRUE. This parameter is blank/null by default unless a value is supplied. Please note that if the filter logic contains any incorrect syntax, the API will respond with an error message. </param>
        /// <param name="dateRangeBegin">null [default] To return only records that have been created or modified *after* a given date/time, provide a timestamp in the format YYYY-MM-DD HH:MM:SS (e.g., '2017-01-01 00:00:00' for January 1, 2017 at midnight server time). If not specified, it will assume no begin time.</param>
        /// <param name="dateRangeEnd">null [default] To return only records that have been created or modified *before* a given date/time, provide a timestamp in the format YYYY-MM-DD HH:MM:SS (e.g., '2017-01-01 00:00:00' for January 1, 2017 at midnight server time). If not specified, it will use the current server time.</param>
        /// <param name="csvDelimiter">comma [default] Set the delimiter used to separate values in the CSV data file (for CSV format only). Options include: comma ',' (default), 'tab', semi-colon ';', pipe '|', or caret '^'. Simply provide the value in quotes for this parameter.</param>
        /// <param name="decimalCharacter">dot [default] If specified, force all numbers into same decimal format. You may choose to force all data values containing a decimal to have the same decimal character, which will be applied to all calc fields and number-validated text fields. Options include comma ',' or dot/full stop '.', but if left blank/null, then it will export numbers using the fields' native decimal format. Simply provide the value of either ',' or '.' for this parameter.</param>
        /// <param name="exportBlankForGrayFormStatus">true, false [default] - specifies whether or not to export blank values for instrument complete status fields that have a gray status icon. All instrument complete status fields having a gray icon can be exported either as a blank value or as "0" (Incomplete). Blank values are recommended in a data export if the data will be re-imported into a REDCap project.</param>
        /// <param name="combineCheckboxOptions">true, false [default] - for checkbox fields, specifies whether all checked options are combined into a single delimited value when exporting labels.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Data from the project in the format and type specified ordered by the record (primary key of project) and then by event id</returns>
        public async Task<string> ExportRecordAsync(string token, string record, RedcapFormat format = RedcapFormat.json, RedcapDataType redcapDataType = RedcapDataType.flat, string[]? fields = null, string[]? forms = null, string[]? events = null, RawOrLabel rawOrLabel = RawOrLabel.raw, RawOrLabelHeaders rawOrLabelHeaders = RawOrLabelHeaders.raw, bool exportCheckboxLabel = false, RedcapReturnFormat onErrorFormat = RedcapReturnFormat.json, bool exportSurveyFields = false, bool exportDataAccessGroups = false, string? filterLogic = null, DateTime? dateRangeBegin = default, DateTime? dateRangeEnd = default, CsvDelimiter csvDelimiter = CsvDelimiter.comma, DecimalCharacter decimalCharacter = DecimalCharacter.none, bool exportBlankForGrayFormStatus = false, bool combineCheckboxOptions = false, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            var fieldsStr = fields?.Length > 0 ? this.ConvertArraytoString(fields) : null;
            var formsStr = forms?.Length > 0 ? this.ConvertArraytoString(forms) : null;
            var eventsStr = events?.Length > 0 ? this.ConvertArraytoString(events) : null;
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["records"] = record;
                payload["content"] = Content.Record.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                payload["returnFormat"] = onErrorFormat.GetDisplayName();
                payload["type"] = redcapDataType.GetDisplayName();
                if (fieldsStr != null) payload["fields"] = fieldsStr;
                if (formsStr != null) payload["forms"] = formsStr;
                if (eventsStr != null) payload["events"] = eventsStr;
                var _rawOrLabel = rawOrLabel.ToString();
                if (!IsNullOrEmpty(_rawOrLabel)) payload["rawOrLabel"] = _rawOrLabel;
                var _rawOrLabelHeaders = rawOrLabelHeaders.ToString();
                if (!IsNullOrEmpty(_rawOrLabelHeaders)) payload["rawOrLabelHeaders"] = _rawOrLabelHeaders;
                if (exportCheckboxLabel) payload["exportCheckboxLabel"] = exportCheckboxLabel.ToString();
                if (exportSurveyFields) payload["exportSurveyFields"] = exportSurveyFields.ToString();
                if (exportDataAccessGroups) payload["exportDataAccessGroups"] = exportDataAccessGroups.ToString();
                if (!IsNullOrEmpty(filterLogic)) payload["filterLogic"] = filterLogic!;
                if (dateRangeBegin.HasValue) payload["dateRangeBegin"] = dateRangeBegin.Value.ToString("yyyy-MM-dd HH:mm:ss");
                if (dateRangeEnd.HasValue) payload["dateRangeEnd"] = dateRangeEnd.Value.ToString("yyyy-MM-dd HH:mm:ss");
                if (format == RedcapFormat.csv) payload["csvDelimiter"] = csvDelimiter.ToString();
                if (decimalCharacter != DecimalCharacter.none) payload["decimalCharacter"] = decimalCharacter.ToString();
                if (combineCheckboxOptions) payload["combineCheckboxOptions"] = combineCheckboxOptions.ToString();
                if (exportBlankForGrayFormStatus) payload["exportBlankForGrayFormStatus"] = exportBlankForGrayFormStatus.ToString();
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap version with version 10.3.0<br/>
        /// Import Records<br/>
        /// This method allows you to import a set of records for a project
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Import/Update privileges in the project.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="format">csv, json [default], xml, odm ('odm' refers to CDISC ODM XML format, specifically ODM version 1.3.1)</param>
        /// <param name="redcapDataType">flat - output as one record per row [default]
        /// eav - input as one data point per row
        /// Non-longitudinal: Will have the fields - record*, field_name, value
        /// Longitudinal: Will have the fields - record*, field_name, value, redcap_event_name
        /// </param>
        /// <param name="overwriteBehavior">
        /// normal - blank/empty values will be ignored [default]
        /// overwrite - blank/empty values are valid and will overwrite data</param>
        /// <param name="forceAutoNumber">If record auto-numbering has been enabled in the project, it may be desirable to import records where each record's record name is automatically determined by REDCap (just as it does in the user interface). 
        /// If this parameter is set to 'true', the record names provided in the request will not be used (although they are still required in order to associate multiple rows of data to an individual record in the request), but instead those records in the request will receive new record names during the import process. 
        /// NOTE: To see how the provided record names get translated into new auto record names, the returnContent parameter should be set to 'auto_ids', which will return a record list similar to 'ids' value, but it will have the new record name followed by the provided record name in the request, in which the two are comma-delimited. For example, if 
        /// false (or 'false') - The record names provided in the request will be used. [default]
        /// true (or 'true') - New record names will be automatically determined.</param>
        /// <param name="backgroundProcess">Specifies whether to do the import as background process.0 or 'false' for no. [default] 1 or 'true' for yes.</param>
        /// <param name="data">The formatted data to be imported. The data should be a List of Dictionary(string,string) or object that contains the fields and values.
        /// NOTE: When importing data in EAV type format, please be aware that checkbox fields must have their field_name listed as variable+'___'+optionCode and its value as either '0' or '1' (unchecked or checked, respectively). For example, for a checkbox field with variable name 'icecream', it would be imported as EAV with the field_name as 'icecream___4' having a value of '1' in order to set the option coded with '4' (which might be 'Chocolate') as 'checked'.</param>
        /// <param name="dateFormat">MDY, DMY, YMD [default] - the format of values being imported for dates or datetime fields (understood with M representing 'month', D as 'day', and Y as 'year') - NOTE: The default format is Y-M-D (with dashes), while MDY and DMY values should always be formatted as M/D/Y or D/M/Y (with slashes), respectively.</param>
        /// <param name="csvDelimiter">Set the delimiter used to separate values in the CSV data file (for CSV format only). Options include: comma ',' (default), 'tab', semi-colon ';', pipe '|', or caret '^'. Simply provide the value in quotes for this parameter.</param>
        /// <param name="returnContent">count [default] - the number of records imported, ids - a list of all record IDs that were imported, auto_ids = (used only when forceAutoNumber=true) a list of pairs of all record IDs that were imported, includes the new ID created and the ID value that was sent in the API request (e.g., 323,10). </param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>the content specified by returnContent</returns>
        public async Task<string> ImportRecordsAsync<T>(string token, RedcapFormat format, RedcapDataType redcapDataType, OverwriteBehavior overwriteBehavior, bool forceAutoNumber, bool backgroundProcess, List<T> data, string? dateFormat = default, CsvDelimiter csvDelimiter = CsvDelimiter.tab, ReturnContent returnContent = ReturnContent.count, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            var _serializedData = JsonConvert.SerializeObject(data);
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["content"] = Content.Record.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                payload["type"] = redcapDataType.GetDisplayName();
                payload["overwriteBehavior"] = overwriteBehavior.ToString();
                payload["forceAutoNumber"] = forceAutoNumber.ToString();
                payload["backgroundProcess"] = backgroundProcess.ToString();
                payload["csvDelimiter"] = csvDelimiter.ToString();
                payload["data"] = _serializedData;
                payload["returnFormat"] = returnFormat.GetDisplayName();
                if (!IsNullOrEmpty(dateFormat)) payload["dateFormat"] = dateFormat!;
                if (!IsNullOrEmpty(returnContent.ToString())) payload["returnContent"] = returnContent.ToString();
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// Delete Records<br/>
        /// This method allows you to delete one or more records from a project in a single API request.
        /// </summary>
        /// <remarks>
        /// 
        /// To use this method, you must have 'Delete Record' user privileges in the project.
        /// </remarks>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="records">an array of record names specifying specific records you wish to delete</param>
        /// <param name="arm">the arm number of the arm in which the record(s) should be deleted. 
        /// (This can only be used if the project is longitudinal with more than one arm.) NOTE: If the arm parameter is not provided, the specified records will be deleted from all arms in which they exist. Whereas, if arm is provided, they will only be deleted from the specified arm. </param>
        /// <param name="deleteLogging">true, false [default] - if true, deletes logging entries associated with the record deletion action.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>the number of records deleted or (if instrument, event, and/or instance are provided) the number of items deleted over the total records specified.</returns>
        public async Task<string> DeleteRecordsAsync(string token, string[] records, int? arm, bool deleteLogging = false, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            if (records?.Length < 1)
                throw new RedcapApiException("Please provide the records you would like to remove.");
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["content"] = Content.Record.GetDisplayName();
                payload["action"] = RedcapAction.Delete.GetDisplayName();
                for (var i = 0; i < records!.Length; i++)
                    payload[$"records[{i}]"] = records[i];
                if (arm.HasValue) payload["arm"] = arm.Value.ToString();
                if (deleteLogging) payload["delete_logging"] = deleteLogging.ToString();
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap Version 11.3.0<br/>
        /// 
        /// Delete Records<br/>
        /// This method allows you to delete one or more records from a project in a single API request, and also optionally allows you to delete parts of a record, such as a single instrument's data for one or more records or a single event's data for one or more records.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have 'Delete Record' user privileges in the project.
        /// </remarks>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="content">record</param>
        /// <param name="action">delete</param>
        /// <param name="records">an array of record names specifying specific records you wish to delete</param>
        /// <param name="arm">the arm number of the arm in which the record(s) should be deleted. 
        /// (This can only be used if the project is longitudinal with more than one arm.) NOTE: If the arm parameter is not provided, the specified records will be deleted from all arms in which they exist. Whereas, if arm is provided, they will only be deleted from the specified arm. </param>
        /// <param name="instrument">the unique instrument name (column B in the Data Dictionary) of an instrument (as a string) if you wish to delete the data for all fields on the specified instrument for the records specified.</param>
        /// <param name="redcapEvent">the unique event name - only for longitudinal projects. NOTE: If instrument is provided for a longitudinal project, the event parameter is mandatory.</param>
        ///  <param name="repeatInstance">the repeating instance number for a repeating instrument or repeating event. NOTE: If project has repeating instruments/events, it will remove only the data for that repeating instance</param>
        /// <param name="deleteLogging">true, false [default] - if true, deletes logging entries associated with the record deletion action.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>the number of records deleted or (if instrument, event, and/or instance are provided) the number of items deleted over the total records specified.</returns>
        public async Task<string> DeleteRecordsAsync(string token, Content content, RedcapAction action, string[] records, int? arm, RedcapInstrument instrument, RedcapEvent redcapEvent, RedcapRepeatInstance repeatInstance, bool deleteLogging = false, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            if (records?.Length < 1)
                throw new RedcapApiException("Please provide the records you would like to remove.");
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["content"] = content.GetDisplayName();
                payload["action"] = action.GetDisplayName();
                for (var i = 0; i < records!.Length; i++)
                    payload[$"records[{i}]"] = records[i];
                if (arm.HasValue) payload["arm"] = arm.Value.ToString();
                payload["instrument"] = instrument.InstrumentName!;
                payload["event"] = redcapEvent.EventName!;
                payload["repeat_instance"] = repeatInstance.RepeatInstance.ToString();
                if (deleteLogging) payload["delete_logging"] = deleteLogging.ToString();
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap Version 11.3.3<br/>
        /// 
        /// Rename Record<br/>
        /// This method allows you to rename a record from a project in a single API request.
        /// 
        /// </summary>
        /// <remarks>
        /// To use this method, you must have 'Rename Record' user privileges in the project.
        /// </remarks>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="record">record name of the current record which you want to rename to new name.</param> 
        /// <param name="newRecordName">new record name to which you want to rename current record.</param>
        /// <param name="arm">specific arm number in which current record exists. If null, then all records with same name across all arms on which it exists (if longitudinal with multiple arms) will be renamed to new record name, otherwise it will rename the record only in the specified arm.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Returns "1" if record is renamed or error message if any.</returns>
        public async Task<string> RenameRecordAsync(string token, string record, string newRecordName, int? arm, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["content"] = Content.Record.GetDisplayName();
                payload["action"] = RedcapAction.Rename.GetDisplayName();
                payload["record"] = record!;
                payload["new_record_name"] = newRecordName;
                if (arm.HasValue) payload["arm"] = arm.Value.ToString();
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap 14.7.0 <br/>
        /// Randomize Record <br/>
        /// This method allows the current API user to randomize a record.
        /// </summary>
        /// <remarks>
        /// To use this method you must have the Randomize privilege in the project.
        /// </remarks>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="record">The record name (id) of the record to randomize. The record must already exist and contain all necessary stratification information.</param>
        /// <param name="randomizationId">The unique id of the randomization (viewable on the Randomization page for users with Design permissions, or on the API Playground page). Corresponds to a specific target field and event.</param>
        /// <param name="format">csv, json [default], xml, odm ('odm' refers to CDISC ODM XML format, specifically ODM version 1.3.1)</param>
        /// <param name="returnFormat">csv, json [default], xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="returnAlt">false [default], true - return the value for the alternative target field, i.e. the randomization number for open allocations. Note: with concealed allocations only the value '*' will be returned, not the real allocation group (which would break the blinding).</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Performs the specified randomization for the record and returns the value for the target randomization field (plus optionally the alternative target value), or an error message on failure (such as if the record does not exist or if stratification information is missing).</returns>
        public async Task<string> RandomizeRecord(string token, string record, string randomizationId, RedcapFormat format, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, bool returnAlt = false, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["action"] = RedcapAction.Randomize.GetDisplayName();
                payload["content"] = Content.Record.GetDisplayName();
                payload["record"] = record!;
                payload["randomization_id"] = randomizationId;
                payload["format"] = format.GetDisplayName();
                payload["returnFormat"] = returnFormat.GetDisplayName();
                payload["returnAlt"] = returnAlt.ToString();
            }, cancellationToken, timeOutSeconds);
        }

    }
}
