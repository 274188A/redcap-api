using Redcap.Exceptions;
using Redcap.Models;
using Redcap.Utilities;

using Serilog;
using System.Text.Json;

namespace Redcap;

public partial class RedcapApi
{

    /// <summary>
    /// From Redcap Version 4.7.0
    ///
    /// Export Events
    /// This method allows you to export the events for a project
    /// NOTE: This only works for longitudinal projects.
    ///
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="arms"></param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the HTTP request times out.</param>
    /// <returns>Events for the project in the format specified</returns>
    public async Task<string> ExportEventsAsync(RedcapFormat format, string[] arms, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        RequireItems(arms, "Please specify the arm you wish to export the events from.");

        return await ExecuteAsync(payload =>
        {
            AddFormattedRequest(payload, Content.Event, format, returnFormat);
            AddIndexedValues(payload, "arms", arms);
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// Exports events and deserializes the JSON response into a list of <see cref="RedcapEvent"/>.
    /// </summary>
    /// <remarks>
    /// This typed overload always requests JSON from REDCap.
    /// </remarks>
    /// <param name="arms">Arm numbers to export events for.</param>
    /// <param name="returnFormat">json [default], xml, csv - specifies the format of error messages.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>The deserialized events.</returns>
    public async Task<IReadOnlyList<RedcapEvent>> ExportEventsTypedAsync(string[] arms, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        var response = await ExportEventsAsync(RedcapFormat.json, arms, returnFormat, cancellationToken, timeOutSeconds);

        try
        {
            var events = JsonSerializer.Deserialize<List<RedcapEvent>>(response, RedcapJsonOptions.Default);
            return events == null ? throw new RedcapApiException("REDCap returned an empty event payload.") : (IReadOnlyList<RedcapEvent>)events;
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to deserialize REDCap event response.");
            throw new RedcapApiException("Failed to deserialize REDCap event response.", ex);
        }
    }

    /// <summary>
    /// From Redcap Version 6.11.0<br/><br/>
    ///
    /// Import Events
    /// This method allows you to import Events into a project or to update existing Events' attributes.
    /// NOTE: This only works for longitudinal projects.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges *and* Project Design/Setup privileges in the project.
    /// </remarks>
    /// <param name="overrideBehavior">false [default] — add/modify only; true — delete all existing Events then import.</param>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">Events to import.</param>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the HTTP request times out.</param>
    /// <returns>Number of Events imported</returns>
    public async Task<string> ImportEventsAsync<T>(bool overrideBehavior, RedcapFormat format, List<T> data, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        RequireItems(data, "Events can not be empty or null");

        return await ExecuteAsync(payload =>
        {
            AddImportRequest(payload, Content.Event, format, data, returnFormat);
            payload["override"] = overrideBehavior ? "true" : "false";
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// From Redcap Version 6.11.0<br/><br/>
    ///
    /// Delete Events<br/><br/>
    /// This method allows you to delete Events from a project.
    /// NOTE: This only works for longitudinal projects.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges *and* Project Design/Setup privileges in the project.
    /// </remarks>
    /// <param name="events">Array of unique event names</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Number of Events deleted</returns>
    public async Task<string> DeleteEventsAsync(string[] events, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        RequireItems(events, "No events to delete...");

        return await ExecuteAsync(payload =>
        {
            AddActionRequest(payload, Content.Event, RedcapAction.Delete);
            AddIndexedValues(payload, "events", events);
        }, cancellationToken, timeOutSeconds);
    }

}
