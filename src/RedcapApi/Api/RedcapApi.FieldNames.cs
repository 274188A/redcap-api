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
                payload["token"] = _token;
                payload["content"] = Content.ExportFieldNames.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                payload["returnFormat"] = returnFormat.GetDisplayName();
                if (!IsNullOrEmpty(field))
                    payload["field"] = field!;
            }, cancellationToken, timeOutSeconds);
        }

    }
}
