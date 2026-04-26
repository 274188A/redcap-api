using Redcap.Exceptions;
using Redcap.Models;
using Redcap.Utilities;

using Serilog;
using System.Text.Json;

using static System.String;

namespace Redcap;

public partial class RedcapApi
{

    /// <summary>
    /// Export Instruments (Data Entry Forms)<br/><br/>
    /// This method allows you to export a list of the data collection instruments for a project.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Instruments for the project in the format specified and will be ordered according to their order in the project.</returns>
    public async Task<string> ExportInstrumentsAsync(RedcapFormat format = RedcapFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddFormattedRequest(payload, Content.Instrument, format);
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// Exports instruments and deserializes the JSON response into a list of <see cref="RedcapInstrument"/>.
    /// </summary>
    /// <remarks>
    /// This typed overload always requests JSON from REDCap.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>The deserialized instruments.</returns>
    public async Task<IReadOnlyList<RedcapInstrument>> ExportInstrumentsTypedAsync(CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        var response = await ExportInstrumentsAsync(RedcapFormat.json, cancellationToken, timeOutSeconds);

        try
        {
            var instruments = JsonSerializer.Deserialize<List<RedcapInstrument>>(response, RedcapJsonOptions.Default);
            return instruments == null ? throw new RedcapApiException("REDCap returned an empty instrument payload.") : (IReadOnlyList<RedcapInstrument>)instruments;
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to deserialize REDCap instrument response.");
            throw new RedcapApiException("Failed to deserialize REDCap instrument response.", ex);
        }
    }

    /// <summary>
    /// From Redcap Version 6.4.0 <br/><br/>
    /// Export PDF file of Data Collection Instruments (either as blank or with data)  <br/><br/>
    /// This method allows you to export a PDF file for any of the following: 1) a single data collection instrument (blank), 2) all instruments (blank), 3) a single instrument (with data from a single record), 4) all instruments (with data from a single record), or 5) all instruments (with data from ALL records).
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// <param name="recordId">the record ID. The value is blank by default.</param>
    /// <param name="eventName">the unique event name - only for longitudinal projects.</param>
    /// <param name="instrument">the unique instrument name as seen in the second column of the Data Dictionary.</param>
    /// <param name="allRecord">If this parameter is passed with any value, it will export all instruments with data from all records.</param>
    /// <param name="returnFormat">csv, json [default] , xml - The returnFormat is only used with regard to the format of any error messages that might be returned.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>A PDF file containing one or all data collection instruments from the project.</returns>
    public async Task<string> ExportPDFInstrumentsAsync(string? recordId = default, string? eventName = default, string? instrument = default, bool allRecord = false, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddContent(payload, Content.Pdf);
            AddReturnFormat(payload, returnFormat);
            AddOptional(payload, "record", recordId);
            AddOptional(payload, "event", eventName);
            AddOptional(payload, "instrument", instrument);
            if (allRecord)
            {
                payload["allRecords"] = allRecord.ToString();
            }
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// From Redcap Version 6.4.0<br/>
    /// **Allows for file download to a path.**
    /// Export PDF file of Data Collection Instruments (either as blank or with data)<br/>
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// <param name="recordId">the record ID. The value is blank by default.</param>
    /// <param name="eventName">the unique event name - only for longitudinal projects.</param>
    /// <param name="instrument">the unique instrument name as seen in the second column of the Data Dictionary.</param>
    /// <param name="allRecord">If this parameter is passed with any value, it will export all instruments with data from all records.</param>
    /// <param name="filePath">the path where the file is located</param>
    /// <param name="returnFormat">csv, json [default] , xml - The returnFormat is only used with regard to the format of any error messages that might be returned.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>A PDF file containing one or all data collection instruments from the project.</returns>
    public async Task<string> ExportPDFInstrumentsAsync(string? recordId = default, string? eventName = default, string? instrument = default, bool allRecord = false, string? filePath = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        if (!Directory.Exists(filePath) && !IsNullOrEmpty(filePath))
        {
            Log.Warning("The directory provided does not exist! Creating a folder for you.");
            Directory.CreateDirectory(filePath!);
        }
        return await ExecuteDownloadAsync(filePath!, payload =>
        {
            AddContent(payload, Content.Pdf);
            AddReturnFormat(payload, returnFormat);
            AddOptional(payload, "record", recordId);
            AddOptional(payload, "event", eventName);
            AddOptional(payload, "instrument", instrument);
            if (allRecord)
            {
                payload["allRecords"] = allRecord.ToString();
            }
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// From Redcap Version 4.7.0 <br/><br/>
    ///
    /// Export Instrument-Event Mappings<br/><br/>
    /// This method allows you to export the instrument-event mappings for a project.
    /// NOTE: This only works for longitudinal projects.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="arms">an array of arm numbers that you wish to pull events for (by default, all events are pulled)</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Instrument-event mappings for the project in the format specified</returns>
    public async Task<string> ExportInstrumentMappingAsync(RedcapFormat format = RedcapFormat.json, string[]? arms = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddFormattedRequest(payload, Content.FormEventMapping, format, returnFormat);
            AddIndexedValues(payload, "arms", arms);
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// Exports instrument-event mappings and deserializes the JSON response into a list of <see cref="FormEventMapping"/>.
    /// </summary>
    /// <remarks>
    /// This typed overload always requests JSON from REDCap.
    /// </remarks>
    /// <param name="arms">Arm numbers to export mappings for. When omitted, all mappings are returned.</param>
    /// <param name="returnFormat">json [default], xml, csv - specifies the format of error messages.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>The deserialized instrument-event mappings.</returns>
    public async Task<IReadOnlyList<FormEventMapping>> ExportInstrumentMappingTypedAsync(string[]? arms = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        var response = await ExportInstrumentMappingAsync(RedcapFormat.json, arms, returnFormat, cancellationToken, timeOutSeconds);

        try
        {
            var mappings = JsonSerializer.Deserialize<List<FormEventMapping>>(response, RedcapJsonOptions.Default);
            return mappings == null ? throw new RedcapApiException("REDCap returned an empty instrument-event mapping payload.") : (IReadOnlyList<FormEventMapping>)mappings;
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to deserialize REDCap instrument-event mapping response.");
            throw new RedcapApiException("Failed to deserialize REDCap instrument-event mapping response.", ex);
        }
    }

    /// <summary>
    /// From Redcap Version 4.7.0 <br/><br/>
    ///
    /// Import Instrument-Event Mappings<br/><br/>
    /// This method allows you to import Instrument-Event Mappings into a project.
    /// NOTE: This only works for longitudinal projects.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges *and* Project Design/Setup privileges in the project.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="data">Contains the attributes 'arm_num', 'unique_event_name', and 'form' of each mapping.</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Number of Instrument-Event Mappings imported</returns>
    public async Task<string> ImportInstrumentMappingAsync<T>(RedcapFormat format, List<T> data, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddFormattedRequest(payload, Content.FormEventMapping, format, returnFormat);
            AddData(payload, data);
        }, cancellationToken, timeOutSeconds);
    }

}
