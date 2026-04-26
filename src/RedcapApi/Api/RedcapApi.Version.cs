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
        /// Export REDCap Version<br/>
        /// This method returns the current REDCap version number as plain text (e.g., 4.13.18, 5.12.2, 6.0.0).
        /// </summary>
        /// <remarks>
        ///
        /// To use this method, you must have API Export privileges in the project.
        /// </remarks>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>The current REDCap version number (three numbers delimited with two periods) as plain text - e.g., 4.13.18, 5.12.2, 6.0.0</returns>
        public async Task<string> ExportRedcapVersionAsync(RedcapFormat format = RedcapFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            var version = await ExecuteAsync(payload =>
            {
                AddFormattedRequest(payload, Content.Version, format);
            }, cancellationToken, timeOutSeconds);

            Version = version;
            return version;
        }
    }
}
