using Redcap.Models;

namespace Redcap.Interfaces;

/// <summary>
/// REDCap user roles API contract.
/// </summary>
public interface IRedcapUserRoles
{
    /// <summary>
    /// From Redcap Version 11.3.0<br/><br/>
    ///
    /// Export User Roles<br/><br/>
    /// This method allows you to export the list of user roles for a project, including their user privileges.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// <param name="content">userRole</param>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>The method will return all the attributes below with regard to user roles privileges in the format specified. Please note that the 'forms' attribute is the only attribute that contains sub-elements (one for each data collection instrument), in which each form will have its own Form Rights value (see the key below to learn what each numerical value represents).
    /// Most user role privilege attributes are boolean (0=No Access, 1=Access). Attributes returned:
    /// unique_role_name, role_label, design, user_rights, data_access_groups, data_export, reports, stats_and_charts, manage_survey_participants, calendar, data_import_tool, data_comparison_tool, logging, file_repository, data_quality_create, data_quality_execute, api_export, api_import, mobile_app, mobile_app_download_data, record_create, record_rename, record_delete, lock_records_customization, lock_records, lock_records_all_forms, forms
    /// KEY:<br/>
    /// Data Export:<br/><br/>
    /// 0=No Access,<br/>
    /// 2=De-Identified,<br/> <br/>
    /// 1=Full Data Set
    /// <br/><br/>
    /// Form Rights:<br/>
    /// 0=No Access, <br/>
    /// 2=Read Only, <br/>
    /// 1=View records/responses and edit records (survey responses are read-only), <br/>
    /// 3=Edit survey responses <br/> <br/>
    /// Other attribute values:<br/>
    /// 0=No Access,<br/>
    /// 1=Access.
    /// </returns>
    /// <example>
    ///
    /// </example>
    Task<string> ExportUserRolesAsync(Content content = Content.UserRole, RedcapFormat format = RedcapFormat.json, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// Exports user roles and deserializes the JSON response into a list of <see cref="RedcapUserRole"/>.
    /// </summary>
    /// <remarks>
    /// This typed overload always requests JSON from REDCap.
    /// </remarks>
    /// <param name="content">userRole</param>
    /// <param name="returnFormat">json [default], xml, csv - specifies the format of error messages.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>The deserialized user roles.</returns>
    Task<IReadOnlyList<RedcapUserRole>> ExportUserRolesTypedAsync(Content content = Content.UserRole, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// From Redcap Version 11.3.0<br/><br/>
    ///
    /// Import User Roles<br/><br/>
    /// This method allows you to import new user roles into a project while setting their privileges, or update the privileges of existing user roles in the project
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges *and* User Rights privileges in the project.
    /// </remarks>
    /// <param name="data">
    /// Contains the attributes of the user role to be added to the project or whose privileges in the project are being updated, in which they are provided in the specified format. All values should be numerical with the exception of unique_role_name, role_label and forms. Please note that the 'forms' attribute is the only attribute that contains sub-elements (one for each data collection instrument), in which each form will have its own Form Rights value (see the key below to learn what each numerical value represents). Most user privilege attributes are boolean (0=No Access, 1=Access).
    /// Missing attributes: If a user role is being added to a project in the API request, then any attributes not provided for a user role in the request(including form-level rights) will automatically be given the minimum privileges(typically 0=No Access) for the attribute/privilege.However, if an existing user role's privileges are being modified in the API request, then any attributes not provided will not be modified from their current value but only the attributes provided in the request will be modified.
    /// Data Export: 0=No Access, 2=De-Identified, 1=Full Data Set
    /// Form Rights: 0=No Access, 2=Read Only, 1=View records/responses and edit records(survey responses are read-only), 3=Edit survey responses
    /// Other attribute values: 0=No Access, 1=Access.
    /// All available attributes: unique_role_name, role_label, design, user_rights, data_access_groups, data_export, reports, stats_and_charts, manage_survey_participants, calendar, data_import_tool, data_comparison_tool, logging, file_repository, data_quality_create, data_quality_execute, api_export, api_import, mobile_app, mobile_app_download_data, record_create, record_rename, record_delete, lock_records_customization, lock_records, lock_records_all_forms, forms
    /// </param>
    /// <param name="content">userRole</param>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Number of user roles added or updated</returns>
    Task<string> ImportUserRolesAsync<T>(List<T> data, Content content = Content.UserRole, RedcapFormat format = RedcapFormat.json, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// From Redcap Version 11.3.0<br/><br/>
    ///
    /// Delete User Roles<br/><br/>
    /// This method allows you to delete User Roles from a project.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges *and* User Rights privileges in the project.
    /// </remarks>
    /// <param name="roles">an array of unique rolenames that you wish to delete</param>
    /// <param name="content">userRole</param>
    /// <param name="action">delete</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Number of User Roles deleted</returns>
    Task<string> DeleteUserRolesAsync(List<string> roles, Content content = Content.UserRole, RedcapAction action = RedcapAction.Delete, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// From Redcap Version 13.0.0<br/><br/>
    ///
    /// Export User Role Assignments<br/><br/>
    /// This method allows you to export user-to-role mappings for a project.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// <param name="content">userRoleMapping</param>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="onErrorFormat">csv, json, xml - specifies the format of error messages.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>User role assignments in the specified format.</returns>
    Task<string> ExportUserRoleAssignmentAsync(Content content = Content.UserRoleMapping, RedcapFormat format = RedcapFormat.json, RedcapReturnFormat onErrorFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// Exports user role assignments and deserializes the JSON response into a list of <see cref="RedcapUserRoleAssignment"/>.
    /// </summary>
    /// <remarks>
    /// This typed overload always requests JSON from REDCap.
    /// </remarks>
    /// <param name="content">userRoleMapping</param>
    /// <param name="onErrorFormat">json [default], xml, csv - specifies the format of error messages.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>The deserialized user role assignments.</returns>
    Task<IReadOnlyList<RedcapUserRoleAssignment>> ExportUserRoleAssignmentTypedAsync(Content content = Content.UserRoleMapping, RedcapReturnFormat onErrorFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// From Redcap Version 13.0.0<br/><br/>
    ///
    /// Import User Role Assignments<br/><br/>
    /// This method allows you to import user-to-role mappings for a project.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges and User Rights privileges in the project.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">A list of user-role mappings.</param>
    /// <param name="content">userRoleMapping</param>
    /// <param name="action">import</param>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="onErrorFormat">csv, json, xml - specifies the format of error messages.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Number of mappings imported or updated.</returns>
    Task<string> ImportUserRoleAssignmentAsync<T>(List<T> data, Content content = Content.UserRoleMapping, RedcapAction action = RedcapAction.Import, RedcapFormat format = RedcapFormat.json, RedcapReturnFormat onErrorFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);
}
