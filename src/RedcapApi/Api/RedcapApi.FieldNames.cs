using Redcap.Exceptions;
using Redcap.Models;
using Redcap.Utilities;

using Serilog;
using System.Text.Json;

namespace Redcap;

public partial class RedcapApi
{

    /// <summary>
    /// Export List of Export Field Names (i.e. variables used during exports and imports)<br/><br/>
    ///
    /// This method returns a list of the export/import-specific version of field names for all fields (or for one field, if desired) in a project.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="field">A field's variable name. By default, all fields are returned.</param>
    /// <param name="returnFormat">csv, json [default], xml - specifies the format of error messages.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Returns a list of the export/import-specific version of field names for all fields (or for one field, if desired) in a project in the format specified.</returns>
    public async Task<string> ExportFieldNamesAsync(RedcapFormat format = RedcapFormat.json, string? field = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddFormattedRequest(payload, Content.ExportFieldNames, format, returnFormat);
            AddOptional(payload, "field", field);
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// Exports field names and deserializes the JSON response into a list of <see cref="RedcapFieldName"/>.
    /// </summary>
    /// <remarks>
    /// This typed overload always requests JSON from REDCap.
    /// </remarks>
    /// <param name="field">A field's variable name. By default, all fields are returned.</param>
    /// <param name="returnFormat">json [default], xml, csv - specifies the format of error messages.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>The deserialized field-name mappings.</returns>
    public async Task<IReadOnlyList<RedcapFieldName>> ExportFieldNamesTypedAsync(string? field = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        var response = await ExportFieldNamesAsync(RedcapFormat.json, field, returnFormat, cancellationToken, timeOutSeconds);

        try
        {
            var fieldNames = JsonSerializer.Deserialize<List<RedcapFieldName>>(response, RedcapJsonOptions.Default);
            return fieldNames == null ? throw new RedcapApiException("REDCap returned an empty field-name payload.") : (IReadOnlyList<RedcapFieldName>)fieldNames;
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to deserialize REDCap field-name response.");
            throw new RedcapApiException("Failed to deserialize REDCap field-name response.", ex);
        }
    }

}
