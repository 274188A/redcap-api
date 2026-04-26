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
        /// Export a File<br/><br/>
        /// This method allows you to download a document that has been attached to an individual record for a File Upload field.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Export privileges in the project.
        /// </remarks>
        /// <param name="record">the record ID</param>
        /// <param name="field">the name of the field that contains the file</param>
        /// <param name="eventName">the unique event name - only for longitudinal projects</param>
        /// <param name="repeatInstance">The repeat instance number of the repeating event or instrument. Default value is '1'.</param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages.</param>
        /// <param name="filePath">File path which the file will be saved.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>the contents of the file</returns>
        public async Task<string> ExportFileAsync(string? record, string field, string eventName, string? repeatInstance = "1", RedcapReturnFormat returnFormat = RedcapReturnFormat.json, string? filePath = null, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            if (IsNullOrEmpty(filePath))
                throw new RedcapApiException("Must contain a file path to save the file.");
            if (!Directory.Exists(filePath))
            {
                Log.Warning("The directory provided does not exist! Creating a folder for you.");
                Directory.CreateDirectory(filePath!);
            }
            if (IsNullOrEmpty(record))
                throw new RedcapApiException("No record provided to export");
            if (IsNullOrEmpty(field) || IsNullOrEmpty(eventName))
                throw new RedcapApiException("No field provided to export");
            return await ExecuteDownloadAsync(filePath!, payload =>
            {
                AddActionRequest(payload, Content.File, RedcapAction.Export, returnFormat);
                payload["record"] = record!;
                payload["field"] = field;
                payload["event"] = eventName;
                AddOptional(payload, "repeat_instance", repeatInstance);
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// Import a File <br/><br/>
        /// This method allows you to upload a document that will be attached to an individual record for a File Upload field.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Import/Update privileges in the project.
        /// </remarks>
        /// <param name="record">the record ID</param>
        /// <param name="field">the name of the field that contains the file</param>
        /// <param name="eventName">the unique event name - only for longitudinal projects</param>
        /// <param name="repeatInstance">The repeat instance number of the repeating event or instrument. Default value is '1'.</param>
        /// <param name="fileName">The File you be imported, contents of the file</param>
        /// <param name="filePath">the path where the file is located</param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>csv, json, xml - specifies the format of error messages.</returns>
        public async Task<string> ImportFileAsync(string record, string field, string eventName, string? repeatInstance, string fileName, string filePath, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            var _binaryFile = Path.Combine(filePath, fileName);
            var _fileBytes = System.IO.File.ReadAllBytes(_binaryFile);
            return await ExecuteMultipartAsync(payload =>
            {
                payload.Add(new StringContent(_token), "token");
                payload.Add(new StringContent(Content.File.GetDisplayName()), "content");
                payload.Add(new StringContent(RedcapAction.Import.GetDisplayName()), "action");
                payload.Add(new StringContent(record), "record");
                payload.Add(new StringContent(field), "field");
                payload.Add(new StringContent(eventName), "event");
                payload.Add(new StringContent(returnFormat.GetDisplayName()), "returnFormat");
                payload.Add(new StringContent(IsNullOrEmpty(repeatInstance) ? "1" : repeatInstance!), "repeat_instance");
                var fileContent = new ByteArrayContent(_fileBytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                payload.Add(fileContent, "file", fileName);
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// Delete a File <br/><br/>
        /// This method allows you to remove a document that has been attached to an individual record for a File Upload field.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Import/Update privileges in the project.
        /// </remarks>
        /// <param name="record">the record ID</param>
        /// <param name="field">the name of the field that contains the file</param>
        /// <param name="eventName">the unique event name - only for longitudinal projects</param>
        /// <param name="repeatInstance">The repeat instance number of the repeating event or instrument. Default value is '1'.</param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>String</returns>
        public async Task<string> DeleteFileAsync(string record, string field, string eventName, string? repeatInstance, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteMultipartAsync(payload =>
            {
                payload.Add(new StringContent(_token), "token");
                payload.Add(new StringContent(Content.File.GetDisplayName()), "content");
                payload.Add(new StringContent(RedcapAction.Delete.GetDisplayName()), "action");
                payload.Add(new StringContent(record), "record");
                payload.Add(new StringContent(field), "field");
                payload.Add(new StringContent(eventName), "event");
                payload.Add(new StringContent(returnFormat.GetDisplayName()), "returnFormat");
                payload.Add(new StringContent(IsNullOrEmpty(repeatInstance) ? "1" : repeatInstance!), "repeat_instance");
            }, cancellationToken, timeOutSeconds);
        }

    }
}
