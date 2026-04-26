using Newtonsoft.Json;
using Redcap.Exceptions;
using Redcap.Models;

using Serilog;

namespace Redcap;

public partial class RedcapApi
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
    /// <param name="timeOutSeconds">The number of seconds beore the http request times out.</param>
    /// <returns>DAGs for the project in the format specified</returns>
    public async Task<string> ExportDagsAsync(RedcapFormat format = RedcapFormat.json, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddFormattedRequest(payload, Content.Dag, format, returnFormat);
        }, cancellationToken, timeOutSeconds);
    }

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
    public async Task<IReadOnlyList<RedcapDag>> ExportDagsTypedAsync(RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        var response = await ExportDagsAsync(RedcapFormat.json, returnFormat, cancellationToken, timeOutSeconds);

        try
        {
            var dags = JsonConvert.DeserializeObject<List<RedcapDag>>(response);
            return dags == null ? throw new RedcapApiException("REDCap returned an empty DAG payload.") : (IReadOnlyList<RedcapDag>)dags;
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to deserialize REDCap DAG response.");
            throw new RedcapApiException("Failed to deserialize REDCap DAG response.", ex);
        }
    }

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
    /// <param name="data">Contains the attributes 'data_access_group_name' (referring to the group name) and 'unique_group_name' (referring to the auto-generated unique group name) of each DAG to be created/modified, in which they are provided in the specified format.</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Number of DAGs added or updated</returns>
    public async Task<string> ImportDagsAsync<T>(RedcapFormat format, List<T> data, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddImportRequest(payload, Content.Dag, format, data, returnFormat);
        }, cancellationToken, timeOutSeconds);
    }

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
    public async Task<string> DeleteDagsAsync(string[] dags, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        RequireItems(dags, "No dags to delete.");

        return await ExecuteAsync(payload =>
        {
            AddActionRequest(payload, Content.Dag, RedcapAction.Delete);
            AddIndexedValues(payload, "dags", dags);
        }, cancellationToken, timeOutSeconds);
    }

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
    /// <param name="timeOutSeconds">Number of seconds before the HTTP request times out.</param>
    /// <returns>Returns "1" when the current API user is switched to the specified Data Access Group, otherwise it returns an error message.</returns>
    public async Task<string> SwitchDagAsync(RedcapDag dag, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            payload["dag"] = dag.UniqueGroupName!;
            AddActionRequest(payload, Content.Dag, RedcapAction.Switch);
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// Export User-DAG Assignments<br/>
    /// This method allows you to export existing User-DAG assignments for a project.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the HTTP request times out.</param>
    /// <returns>User-DAG assignments for the project in the format specified</returns>
    public async Task<string> ExportUserDagAssignmentAsync(RedcapFormat format = RedcapFormat.json, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddFormattedRequest(payload, Content.UserDagMapping, format, returnFormat);
        }, cancellationToken, timeOutSeconds);
    }

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
    public async Task<IReadOnlyList<RedcapUserDagAssignment>> ExportUserDagAssignmentTypedAsync(RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        var response = await ExportUserDagAssignmentAsync(RedcapFormat.json, returnFormat, cancellationToken, timeOutSeconds);

        try
        {
            var assignments = JsonConvert.DeserializeObject<List<RedcapUserDagAssignment>>(response);
            return assignments == null ? throw new RedcapApiException("REDCap returned an empty user-DAG assignment payload.") : (IReadOnlyList<RedcapUserDagAssignment>)assignments;
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to deserialize REDCap user-DAG assignment response.");
            throw new RedcapApiException("Failed to deserialize REDCap user-DAG assignment response.", ex);
        }
    }

    /// <summary>
    /// Import User-DAG Assignments<br/><br/>
    /// This method allows you to assign users to any data access group.
    /// NOTE: If you wish to modify an existing mapping, you *must* provide its unique username and group name. If the 'redcap_data_access_group' column is not provided, user will not assigned to any group. There should be only one record per username.
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges in the project.
    /// </remarks>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="data">Contains the attributes 'username' and 'redcap_data_access_group' of each User-DAG assignment to be modified.</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the HTTP request times out.</param>
    /// <returns>Number of User-DAG assignments added or updated</returns>
    public async Task<string> ImportUserDagAssignmentAsync<T>(RedcapFormat format, List<T> data, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        RequireItems(data, "No data to import, please specify data to import.");

        return await ExecuteAsync(payload =>
        {
            AddImportRequest(payload, Content.UserDagMapping, format, data, returnFormat);
        }, cancellationToken, timeOutSeconds);
    }

}
