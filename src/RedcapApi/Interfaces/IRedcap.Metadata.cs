using Redcap.Models;

namespace Redcap.Interfaces
{
    /// <summary>
    /// REDCap metadata API contract.
    /// </summary>
    public interface IRedcapMetadata
    {
        /// <summary>
        /// From Redcap Version 3.4.0+<br/><br/>
        /// Export Metadata (Data Dictionary)
        /// This method allows you to export the metadata for a project
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Export privileges in the project.
        /// </remarks>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="fields">an array of field names specifying specific fields you wish to pull (by default, all metadata is pulled)</param>
        /// <param name="forms">an array of form names specifying specific data collection instruments for which you wish to pull metadata (by default, all metadata is pulled). NOTE: These 'forms' are not the form label values that are seen on the webpages, but instead they are the unique form names seen in Column B of the data dictionary.</param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Metadata from the project (i.e. Data Dictionary values) in the format specified ordered by the field order</returns>
        Task<string> ExportMetaDataAsync(RedcapFormat format, string[]? fields = default, string[]? forms = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

        /// <summary>
        /// Exports metadata and deserializes the JSON response into a list of <see cref="RedcapMetaData"/>.
        /// </summary>
        /// <remarks>
        /// This typed overload always requests JSON from REDCap.
        /// </remarks>
        /// <param name="fields">Specific field names to export. When omitted, all fields are returned.</param>
        /// <param name="forms">Specific instrument names to export. When omitted, all forms are returned.</param>
        /// <param name="returnFormat">json [default], xml, csv - specifies the format of error messages.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>The deserialized metadata rows.</returns>
        Task<IReadOnlyList<RedcapMetaData>> ExportMetaDataTypedAsync(string[]? fields = default, string[]? forms = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

        /// <summary>
        /// From Redcap Version 6.11.0<br/><br/>
        ///
        /// Import Metadata (Data Dictionary)<br/><br/>
        ///
        /// This method allows you to import metadata (i.e., Data Dictionary) into a project. Notice: Because of this method's destructive nature, it is only available for use for projects in Development status.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Import/Update privileges *and* Project Design/Setup privileges in the project.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="data">The formatted data to be imported.</param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Number of fields imported</returns>
        Task<string> ImportMetaDataAsync<T>(RedcapFormat format, List<T> data, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);
    }
}
