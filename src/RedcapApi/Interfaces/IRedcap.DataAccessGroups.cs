using Redcap.Models;

namespace Redcap.Interfaces;

/// <summary>
/// REDCap data access groups API contract.
/// </summary>
public interface IRedcapDataAccessGroups
{
    /// <summary>
    /// Export DAGs<br/><br/>
    /// This method allows you to export the Data Access Groups for a project
    /// <remarks>
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// </summary>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>DAGs for the project in the format specified</returns>
    Task<string> ExportDagsAsync(RedcapFormat format = RedcapFormat.json, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// Exports data access groups and deserializes the JSON response into a list of <see cref="RedcapDag"/>.
    /// </summary>
    /// <remarks>
    /// This typed overload always requests JSON from REDCap.
    /// </remarks>
    /// <param name="returnFormat">json [default], xml, csv - specifies the format of error messages.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>The deserialized data access groups.</returns>
    Task<IReadOnlyList<RedcapDag>> ExportDagsTypedAsync(RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// Import DAGs
    /// This method allows you to import new DAGs (Data Access Groups) into a project or update the group name of any existing DAGs.
    /// NOTE: DAGs can be renamed by simply changing the group name(data_access_group_name).
    /// DAG can be created by providing group name value while unique group name should be set to blank.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges in the project.
    /// </remarks>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="data">Contains the attributes 'data_access_group_name' (referring to the group name) and 'unique_group_name' (referring to the auto-generated unique group name) of each DAG to be created/modified, in which they are provided in the specified format.
    /// Refer to the API documenations for additional examples.
    /// JSON Example:
    /// [{"data_access_group_name":"CA Site","unique_group_name":"ca_site"}
    /// {"data_access_group_name":"FL Site","unique_group_name":"fl_site"},
    /// { "data_access_group_name":"New Site","unique_group_name":""}]
    /// CSV Example:
    /// data_access_group_name,unique_group_name
    /// "CA Site",ca_site
    /// "FL Site",fl_site
    /// "New Site",
    /// </param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Number of DAGs added or updated</returns>
    Task<string> ImportDagsAsync<T>(RedcapFormat format, List<T> data, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// Delete DAGs <br/><br/>
    /// This method allows you to delete DAGs from a project.
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges in the project.
    /// </remarks>
    /// </summary>
    /// <param name="dags">an array of unique group names that you wish to delete</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Number of DAGs deleted</returns>
    Task<string> DeleteDagsAsync(string[] dags, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// From Redcap Version 11.3.1<br/><br/>
    ///
    /// Switch DAG<br/><br/>
    /// This method allows the current API user to switch (assign/reassign/unassign) their current Data Access Group assignment if they have been assigned to multiple DAGs via the DAG Switcher page in the project.
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges in the project.
    /// </remarks>
    /// </summary>
    /// <param name="dag">The unique group name of the Data Access Group to which you wish to switch.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Returns "1" when the current API user is switched to the specified Data Access Group, otherwise it returns an error message.</returns>
    Task<string> SwitchDagAsync(RedcapDag dag, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// Export User-DAG Assignments<br/>
    /// This method allows you to export existing User-DAG assignments for a project.
    ///
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>User-DAG assignments for the project in the format specified</returns>
    Task<string> ExportUserDagAssignmentAsync(RedcapFormat format = RedcapFormat.json, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// Exports user-DAG assignments and deserializes the JSON response into a list of <see cref="RedcapUserDagAssignment"/>.
    /// </summary>
    /// <remarks>
    /// This typed overload always requests JSON from REDCap.
    /// </remarks>
    /// <param name="returnFormat">json [default], xml, csv - specifies the format of error messages.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>The deserialized user-DAG assignments.</returns>
    Task<IReadOnlyList<RedcapUserDagAssignment>> ExportUserDagAssignmentTypedAsync(RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// Import User-DAG Assignments<br/><br/>
    /// This method allows you to assign users to any data access group.
    /// NOTE: If you wish to modify an existing mapping, you *must* provide its unique username and group name.If the 'redcap_data_access_group' column is not provided, user will not assigned to any group.There should be only one record per username.
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges in the project.
    /// </remarks>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="data">
    /// Contains the attributes 'username' (referring to the existing unique username) and 'redcap_data_access_group' (referring to existing unique group name) of each User-DAG assignments to be modified, in which they are provided in the specified format.
    /// JSON Example:
    /// [{"username":"ca_dt_person","redcap_data_access_group":"ca_site"},
    /// {"username":"fl_dt_person","redcap_data_access_group":"fl_site"},
    /// { "username":"global_user","redcap_data_access_group":""}]
    /// CSV Example:
    /// username,redcap_data_access_group
    /// ca_dt_person, ca_site
    /// fl_dt_person, fl_site
    /// global_user,
    /// </param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Number of User-DAG assignments added or updated</returns>
    Task<string> ImportUserDagAssignmentAsync<T>(RedcapFormat format, List<T> data, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);
}
