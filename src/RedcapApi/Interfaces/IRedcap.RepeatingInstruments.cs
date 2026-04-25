using Redcap.Models;

namespace Redcap.Interfaces
{
    public partial interface IRedcap
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
        Task<string> ExportRepeatingInstrumentsAndEventsAsync(RedcapFormat format = RedcapFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

        /// <summary>
        /// Obsolete compatibility shim for <see cref="ExportRepeatingInstrumentsAndEventsAsync(RedcapFormat, CancellationToken, long)"/>.
        /// </summary>
        [Obsolete("Use ExportRepeatingInstrumentsAndEventsAsync instead.")]
        Task<string> ExportRepeatingInstrumentsAndEvents(RedcapFormat format = RedcapFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

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
        /// <returns>Number of repeated isntruments or repeated events that have been imported</returns>
        Task<string> ImportRepeatingInstrumentsAndEventsAsync<T>(List<T> data, Content content = Content.RepeatingFormsEvents, RedcapFormat format = RedcapFormat.json, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

        /// <summary>
        /// Obsolete compatibility shim for <see cref="ImportRepeatingInstrumentsAndEventsAsync{T}(List{T}, Content, RedcapFormat, RedcapReturnFormat, CancellationToken, long)"/>.
        /// </summary>
        [Obsolete("Use ImportRepeatingInstrumentsAndEventsAsync instead.")]
        Task<string> ImportRepeatingInstrumentsAndEvents<T>(List<T> data, Content content = Content.RepeatingFormsEvents, RedcapFormat format = RedcapFormat.json, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);
    }
}
