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
        /// From Redcap Version 8.2.0 <br/>
        ///
        /// Export Repeating Instruments and Events <br/>
        ///
        /// This method allows you to export a list of the repeated instruments and repeating events for a project. This includes their unique instrument name as seen in the second column of the Data Dictionary, as well as each repeating instrument's corresponding custom repeating instrument label. For longitudinal projects, the unique event name is also returned for each repeating instrument. Additionally, repeating events are returned as separate items, in which the instrument name will be blank/null to indicate that it is a repeating event (rather than a repeating instrument).
        /// </summary>
        /// <param name="format">csv, json [default], xml odm ('odm' refers to CDISC ODM XML format, specifically ODM version 1.3.1)</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Repeated instruments and events for the project in the format specified and will be ordered according to their order in the project.</returns>
        public async Task<string> ExportRepeatingInstrumentsAndEventsAsync(RedcapFormat format = RedcapFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(payload =>
            {
                payload["token"] = _token;
                payload["content"] = Content.RepeatingFormsEvents.GetDisplayName();
                payload["format"] = format.GetDisplayName();
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// Exports repeating instruments and events and deserializes the JSON response into a list of <see cref="RedcapRepeatInstrument"/>.
        /// </summary>
        /// <remarks>
        /// This typed overload always requests JSON from REDCap.
        /// </remarks>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>The deserialized repeating instruments and events.</returns>
        public async Task<IReadOnlyList<RedcapRepeatInstrument>> ExportRepeatingInstrumentsAndEventsTypedAsync(CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            var response = await ExportRepeatingInstrumentsAndEventsAsync(RedcapFormat.json, cancellationToken, timeOutSeconds);

            try
            {
                var repeatingInstruments = JsonConvert.DeserializeObject<List<RedcapRepeatInstrument>>(response);
                if (repeatingInstruments == null)
                {
                    throw new RedcapApiException("REDCap returned an empty repeating instruments payload.");
                }

                return repeatingInstruments;
            }
            catch (JsonException ex)
            {
                Log.Error(ex, "Failed to deserialize REDCap repeating instruments response.");
                throw new RedcapApiException("Failed to deserialize REDCap repeating instruments response.", ex);
            }
        }

        /// <summary>
        /// Obsolete compatibility shim for <see cref="ExportRepeatingInstrumentsAndEventsAsync(RedcapFormat, CancellationToken, long)"/>.
        /// </summary>
        [Obsolete("Use ExportRepeatingInstrumentsAndEventsAsync instead.")]
        public Task<string> ExportRepeatingInstrumentsAndEvents(RedcapFormat format = RedcapFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return ExportRepeatingInstrumentsAndEventsAsync(format, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap Version 8.10.0 <br/>
        ///
        /// Import Repeating Instruments and Events<br/>
        /// This method allows you to import a list of the repeated instruments and repeating events for a project. This includes their unique instrument name as seen in the second column of the Data Dictionary, as well as each repeating instrument's corresponding custom repeating instrument label. For longitudinal projects, the unique event name is also needed for each repeating instrument. Additionally, repeating events must be submitted as separate items, in which the instrument name will be blank/null to indicate that it is a repeating event (rather than a repeating instrument).
        /// </summary>
        /// <param name="data">Note: Super API Tokens can also be utilized for this method instead of a project-level API token. Users can only be granted a super token by a REDCap administrator (using the API Tokens page in the REDCap Control Center).</param>
        /// <param name="content">repeatingFormsEvents</param>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Repeated instruments and events for the project in the format specified and will be ordered according to their order in the project.</returns>
        public async Task<string> ImportRepeatingInstrumentsAndEventsAsync<T>(List<T> data, Content content = Content.RepeatingFormsEvents, RedcapFormat format = RedcapFormat.json, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(payload =>
            {
                payload["token"] = _token;
                payload["content"] = content.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                payload["returnFormat"] = returnFormat.GetDisplayName();
                payload["data"] = JsonConvert.SerializeObject(data);
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// Obsolete compatibility shim for <see cref="ImportRepeatingInstrumentsAndEventsAsync{T}(List{T}, Content, RedcapFormat, RedcapReturnFormat, CancellationToken, long)"/>.
        /// </summary>
        [Obsolete("Use ImportRepeatingInstrumentsAndEventsAsync instead.")]
        public Task<string> ImportRepeatingInstrumentsAndEvents<T>(List<T> data, Content content = Content.RepeatingFormsEvents, RedcapFormat format = RedcapFormat.json, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return ImportRepeatingInstrumentsAndEventsAsync(data, content, format, returnFormat, cancellationToken, timeOutSeconds);
        }

    }
}
