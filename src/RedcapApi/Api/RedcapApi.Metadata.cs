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
        /// From Redcap Version 3.4.0+<br/><br/>
        /// Export Metadata (Data Dictionary)
        /// This method allows you to export the metadata for a project
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Export privileges in the project.
        /// </remarks>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="fields">an array of field names specifying specific fields you wish to pull (by default, all metadata is pulled)</param>
        /// <param name="forms">an array of form names specifying specific data collection instruments for which you wish to pull metadata (by default, all metadata is pulled). NOTE: These 'forms' are not the form label values that are seen on the webpages, but instead they are the unique form names seen in Column B of the data dictionary.</param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Metadata from the project (i.e. Data Dictionary values) in the format specified ordered by the field order</returns>
        public async Task<string> ExportMetaDataAsync(string token, RedcapFormat format = RedcapFormat.json, string[]? fields = default, string[]? forms = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["content"] = Content.MetaData.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                payload["returnFormat"] = returnFormat.GetDisplayName();
                if (fields?.Length > 0)
                    for (var i = 0; i < fields.Length; i++)
                        payload[$"fields[{i}]"] = fields[i];
                if (forms?.Length > 0)
                    for (var i = 0; i < forms.Length; i++)
                        payload[$"forms[{i}]"] = forms[i];
            }, cancellationToken, timeOutSeconds);
        }

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
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="data">The formatted data to be imported.</param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Number of fields imported</returns>
        public async Task<string> ImportMetaDataAsync<T>(string token, RedcapFormat format, List<T> data, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["content"] = Content.MetaData.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                payload["returnFormat"] = returnFormat.GetDisplayName();
                payload["data"] = JsonConvert.SerializeObject(data);
            }, cancellationToken, timeOutSeconds);
        }

    }
}
