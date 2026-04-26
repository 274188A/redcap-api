using Newtonsoft.Json;
using Redcap.Exceptions;
using Redcap.Models;
using Redcap.Utilities;

using Serilog;

namespace Redcap;

public partial class RedcapApi
{

    /// <summary>
    /// From Redcap Version 10.8 <br/>
    /// Export Logging  <br/>
    /// This method allows you to export the logging (audit trail) of all changes made to this project, including data exports, data changes, project metadata changes, modification of user rights, etc.
    /// <br/>KEY:<br/>Filter by event (logtype):<br/>export = Data export,<br/>manage = Manage/Design,<br/>user = User or role created-updated-deleted,<br/>record = Record created-updated-deleted,<br/>record_add = Record created (only),<br/>record_edit = Record updated(only),<br/>record_delete = Record deleted(only),<br/>lock_record = Record locking and e-signatures,<br/>page_view = Page View
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges *and* Logging privileges in the project.
    /// </remarks>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="logType">You may choose event type to fetch result for specific event type</param>
    /// <param name="user">To return only the events belong to specific user (referring to existing username), provide a user. If not specified, it will assume all users</param>
    /// <param name="record">To return only the events belong to specific record (referring to existing record name), provide a record. If not specified, it will assume all records. This parameter is available only when event is related to record.</param>
    /// <param name="dag">To return only the events belong to specific DAG (referring to group_id), provide a dag. If not specified, it will assume all dags.</param>
    /// <param name="beginTime">To return only the events that have been logged *after* a given date/time, provide a timestamp in the format YYYY-MM-DD HH:MM (e.g., '2017-01-01 17:00' for January 1, 2017 at 5:00 PM server time). If not specified, it will assume no begin time.</param>
    /// <param name="endTime">To return only records that have been logged *before* a given date/time, provide a timestamp in the format YYYY-MM-DD HH:MM (e.g., '2017-01-01 17:00' for January 1, 2017 at 5:00 PM server time). If not specified, it will use the current server time.</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>List of all changes made to this project, including data exports, data changes, and the creation or deletion of users.</returns>
    public async Task<string> ExportLoggingAsync(RedcapFormat format = RedcapFormat.json, LogType logType = LogType.All, string? user = default, string? record = default, string? dag = default, string? beginTime = default, string? endTime = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddFormattedRequest(payload, Content.Log, format, returnFormat);
            payload["logtype"] = logType.GetDisplayName();
            AddOptional(payload, "user", user);
            AddOptional(payload, "record", record);
            AddOptional(payload, "dag", dag);
            AddOptional(payload, "beginTime", beginTime);
            AddOptional(payload, "endTime", endTime);
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// Exports logging entries and deserializes the JSON response into a list of <see cref="RedcapLogEntry"/>.
    /// </summary>
    /// <remarks>
    /// This typed overload always requests JSON from REDCap.
    /// </remarks>
    /// <param name="logType">You may choose event type to fetch result for specific event type.</param>
    /// <param name="user">To return only the events belonging to a specific user, provide a username.</param>
    /// <param name="record">To return only the events belonging to a specific record, provide a record name.</param>
    /// <param name="dag">To return only the events belonging to a specific DAG, provide a group_id.</param>
    /// <param name="beginTime">Lower timestamp bound in REDCap server time.</param>
    /// <param name="endTime">Upper timestamp bound in REDCap server time.</param>
    /// <param name="returnFormat">json [default], xml, csv - specifies the format of error messages.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>The deserialized logging entries.</returns>
    public async Task<IReadOnlyList<RedcapLogEntry>> ExportLoggingTypedAsync(LogType logType = LogType.All, string? user = default, string? record = default, string? dag = default, string? beginTime = default, string? endTime = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        var response = await ExportLoggingAsync(RedcapFormat.json, logType, user, record, dag, beginTime, endTime, returnFormat, cancellationToken, timeOutSeconds);

        try
        {
            var entries = JsonConvert.DeserializeObject<List<RedcapLogEntry>>(response);
            return entries == null ? throw new RedcapApiException("REDCap returned an empty logging payload.") : (IReadOnlyList<RedcapLogEntry>)entries;
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to deserialize REDCap logging response.");
            throw new RedcapApiException("Failed to deserialize REDCap logging response.", ex);
        }
    }
}
