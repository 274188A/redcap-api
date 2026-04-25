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
        /// From Redcap Version 4.7.0<br/><br/>
        /// Export Arms<br/><br/>
        /// This method allows you to export the Arms for a project
        /// NOTE: This only works for longitudinal projects.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Export privileges in the project.
        /// </remarks>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="arms">an array of arm numbers that you wish to pull events for (by default, all events are pulled)</param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Arms for the project in the format specified(only ones with Events available)</returns>
        public async Task<string> ExportArmsAsync(RedcapFormat format = RedcapFormat.json, string[]? arms = null, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(payload =>
            {
                payload["token"] = _token;
                payload["content"] = Content.Arm.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                if (arms?.Length > 0)
                    for (var i = 0; i < arms.Length; i++)
                        payload[$"arms[{i}]"] = arms[i];
                payload["returnFormat"] = returnFormat.GetDisplayName();
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap Version 4.7.0<br/><br/>
        ///
        /// Import Arms<br/><br/>
        /// This method allows you to import Arms into a project or to rename existing Arms in a project.
        /// You may use the parameter override=1 as a 'delete all + import' action in order to erase all existing Arms in the project while importing new Arms.
        /// Notice: Because of the 'override' parameter's destructive nature, this method may only use override=1 for projects in Development status.
        /// NOTE: This only works for longitudinal projects.
        ///
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Import/Update privileges *and* Project Design/Setup privileges in the project.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="overrideBehavior">false [default] — add/rename only; true — delete all existing Arms then import.</param>
        /// <param name="action">import</param>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="data">Contains the attributes 'arm_num' (referring to the arm number) and 'name' (referring to the arm's name) of each arm to be created/modified, in which they are provided in the specified format.
        /// [{"arm_num":"1","name":"Drug A"},
        /// {"arm_num":"2","name":"Drug B"},
        /// {"arm_num":"3","name":"Drug C"}]
        /// </param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'xml'.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">The number of seconds beore the http request times out.</param>
        /// <returns>Number of Arms imported</returns>
        public async Task<string> ImportArmsAsync<T>(bool overrideBehavior, RedcapAction action, RedcapFormat format, List<T> data, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(payload =>
            {
                payload["token"] = _token;
                payload["content"] = Content.Arm.GetDisplayName();
                payload["action"] = action.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                payload["override"] = overrideBehavior ? "true" : "false";
                payload["returnFormat"] = returnFormat.GetDisplayName();
                payload["data"] = JsonConvert.SerializeObject(data);
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap Version 4.7.0<br/><br/>
        ///
        /// Delete Arms<br/><br/>
        /// This method allows you to delete Arms from a project.
        /// Notice: Because of this method's destructive nature, it is only available for use for projects in Development status. Additionally, please be aware that deleting an arm also automatically deletes all events that belong to that arm, and will also automatically delete any records/data that have been collected under that arm (this is non-reversible data loss).
        /// NOTE: This only works for longitudinal projects.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Import/Update privileges *and* Project Design/Setup privileges in the project.
        /// </remarks>
        /// <param name="arms">an array of arm numbers that you wish to delete</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">The number of seconds beore the http request times out.</param>
        /// <returns>Number of Arms deleted</returns>
        public async Task<string> DeleteArmsAsync(string[] arms, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(payload =>
            {
                if (arms == null || arms.Length < 1)
                    throw new InvalidOperationException("No arm to delete, please specify arm");
                payload["token"] = _token;
                payload["content"] = Content.Arm.GetDisplayName();
                payload["action"] = RedcapAction.Delete.GetDisplayName();
                for (var i = 0; i < arms.Length; i++)
                    payload[$"arms[{i}]"] = arms[i];
            }, cancellationToken, timeOutSeconds);
        }

    }
}
