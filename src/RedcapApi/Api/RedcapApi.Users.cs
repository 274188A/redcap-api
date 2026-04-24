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
        /// From Redcap Version 4.7.0<br/>
        /// 
        /// Export Users <br/>
        /// This method allows you to export the list of users for a project, including their user privileges and also email address, first name, and last name. Note: If the user has been assigned to a user role, it will return the user with the role's defined privileges. 
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Export privileges in the project.
        /// KEY: Data Export: 0=No Access, 2=De-Identified, 1=Full Data Set
        /// Form Rights: 0=No Access, 2=Read Only, 1=View records/responses and edit records(survey responses are read-only), 3=Edit survey responses
        /// Other attribute values: 0=No Access, 1=Access.
        /// </remarks>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>The method will return all the attributes below with regard to user privileges in the format specified. Please note that the 'forms' attribute is the only attribute that contains sub-elements (one for each data collection instrument), in which each form will have its own Form Rights value (see the key below to learn what each numerical value represents). Most user privilege attributes are boolean (0=No Access, 1=Access). Attributes returned:
        /// username, email, firstname, lastname, expiration, data_access_group, design, user_rights, data_access_groups, data_export, reports, stats_and_charts, manage_survey_participants, calendar, data_import_tool, data_comparison_tool, logging, file_repository, data_quality_create, data_quality_execute, api_export, api_import, mobile_app, mobile_app_download_data, record_create, record_rename, record_delete, lock_records_customization, lock_records, lock_records_all_forms, forms</returns>
        public async Task<string> ExportUsersAsync(string token, RedcapFormat format = RedcapFormat.json, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["content"] = Content.User.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                payload["returnFormat"] = returnFormat.GetDisplayName();
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap Version 4.7.0<br/> 
        /// 
        /// Import Users<br/>
        /// This method allows you to import new users into a project while setting their user privileges, or update the privileges of existing users in the project. 
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Import/Update privileges *and* User Rights privileges in the project.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="data">
        /// Contains the attributes of the user to be added to the project or whose privileges in the project are being updated, in which they are provided in the specified format. All values should be numerical with the exception of username, expiration, data_access_group, and forms. Please note that the 'forms' attribute is the only attribute that contains sub-elements (one for each data collection instrument), in which each form will have its own Form Rights value (see the key below to learn what each numerical value represents). Most user privilege attributes are boolean (0=No Access, 1=Access). Please notice the distinction between data_access_group (contains the unique DAG name for a user) and data_access_groups (denotes whether the user has access to the Data Access Groups page).
        /// Missing attributes: If a user is being added to a project in the API request, then any attributes not provided for a user in the request(including form-level rights) will automatically be given the minimum privileges(typically 0=No Access) for the attribute/privilege.However, if an existing user's privileges are being modified in the API request, then any attributes not provided will not be modified from their current value but only the attributes provided in the request will be modified.
        /// Data Export: 0=No Access, 2=De-Identified, 1=Full Data Set
        /// Form Rights: 0=No Access, 2=Read Only, 1=View records/responses and edit records(survey responses are read-only), 3=Edit survey responses
        /// Other attribute values: 0=No Access, 1=Access.
        /// All available attributes:
        /// username, expiration, data_access_group, design, user_rights, data_access_groups, data_export, reports, stats_and_charts, manage_survey_participants, calendar, data_import_tool, data_comparison_tool, logging, file_repository, data_quality_create, data_quality_execute, api_export, api_import, mobile_app, mobile_app_download_data, record_create, record_rename, record_delete, lock_records_customization, lock_records, lock_records_all_forms, forms
        /// </param>
        /// <example>
        /// JSON Example:
        /// [{"username":"harrispa","expiration":"","data_access_group":"","design":"1","user_rights":"1","data_access_groups":"1","data_export":"1","reports":"1","stats_and_charts":"1",
        /// "manage_survey_participants":"1","calendar":"1","data_import_tool":"1","data_comparison_tool":"1",
        /// "logging":"1","file_repository":"1","data_quality_create":"1","data_quality_execute":"1",
        /// "api_export":"1","api_import":"1","mobile_app":"1","mobile_app_download_data":"0","record_create":"1",
        /// "record_rename":"0","record_delete":"0","lock_records_all_forms":"0","lock_records":"0",
        /// "lock_records_customization":"0","forms":{"demographics":"1","day_3":"1","other":"1"}
        /// },{"username":"taylorr4","expiration":"2015-12-07","data_access_group":"","design":"0",
        /// "user_rights":"0","data_access_groups":"0","data_export":"2","reports":"1","stats_and_charts":"1",
        /// "manage_survey_participants":"1","calendar":"1","data_import_tool":"0",
        /// "data_comparison_tool":"0","logging":"0","file_repository":"1","data_quality_create":"0",
        /// "data_quality_execute":"0","api_export":"0","api_import":"0","mobile_app":"0",
        /// "mobile_app_download_data":"0","record_create":"1","record_rename":"0","record_delete":"0",
        /// "lock_records_all_forms":"0","lock_records":"0","lock_records_customization":"0",
        /// "forms":{"demographics":"1","day_3":"2","other":"0"}}]
        /// </example>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Number of users added or updated</returns>
        public async Task<string> ImportUsersAsync<T>(string token, List<T> data, RedcapFormat format = RedcapFormat.json, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            var _serializedData = JsonConvert.SerializeObject(data);
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["content"] = Content.User.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                payload["returnFormat"] = returnFormat.GetDisplayName();
                payload["data"] = _serializedData;
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap Version 11.3.0<br/><br/>
        /// 
        /// Delete Users<br/><br/>
        /// This method allows you to delete Users from a project.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Import/Update privileges *and* User Rights privileges in the project.
        /// </remarks>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="users">an array of unique usernames that you wish to delete</param>
        /// <param name="content">user</param>
        /// <param name="action">delete</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Number of Users deleted</returns>
        public async Task<string> DeleteUsersAsync(string token, List<string> users, Content content = Content.User, RedcapAction action = RedcapAction.Delete, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["content"] = content.GetDisplayName();
                payload["action"] = action.GetDisplayName();
                for (var i = 0; i < users.Count; i++)
                    payload[$"users[{i}]"] = users[i];
            }, cancellationToken, timeOutSeconds);
        }

    }
}
