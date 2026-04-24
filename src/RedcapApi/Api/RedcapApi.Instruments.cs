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
        /// Export Instruments (Data Entry Forms)<br/><br/>
        /// This method allows you to export a list of the data collection instruments for a project. 
        /// This includes their unique instrument name as seen in the second column of the Data Dictionary, as well as each instrument's corresponding instrument label, which is seen on a project's left-hand menu when entering data. The instruments will be ordered according to their order in the project.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Export privileges in the project.
        /// </remarks>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Instruments for the project in the format specified and will be ordered according to their order in the project.</returns>
        public async Task<string> ExportInstrumentsAsync(string token, RedcapFormat format = RedcapFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["content"] = Content.Instrument.GetDisplayName();
                payload["format"] = format.GetDisplayName();
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap Version 6.4.0 <br/><br/>
        /// Export PDF file of Data Collection Instruments (either as blank or with data)  <br/><br/>
        /// This method allows you to export a PDF file for any of the following: 1) a single data collection instrument (blank), 2) all instruments (blank), 3) a single instrument (with data from a single record), 4) all instruments (with data from a single record), or 5) all instruments (with data from ALL records). 
        /// This is the exact same PDF file that is downloadable from a project's data entry form in the web interface, and additionally, the user's privileges with regard to data exports will be applied here just like they are when downloading the PDF in the web interface (e.g., if they have de-identified data export rights, then it will remove data from certain fields in the PDF). 
        /// If the user has 'No Access' data export rights, they will not be able to use this method, and an error will be returned.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Export privileges in the project.
        /// </remarks>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="recordId">the record ID. The value is blank by default. If record is blank, it will return the PDF as blank (i.e. with no data). If record is provided, it will return a single instrument or all instruments containing data from that record only.</param>
        /// <param name="eventName">the unique event name - only for longitudinal projects. For a longitudinal project, if record is not blank and event is blank, it will return data for all events from that record. If record is not blank and event is not blank, it will return data only for the specified event from that record.</param>
        /// <param name="instrument">the unique instrument name as seen in the second column of the Data Dictionary. The value is blank by default, which returns all instruments. If record is not blank and instrument is blank, it will return all instruments for that record.</param>
        /// <param name="allRecord">[The value of this parameter does not matter and is ignored.] If this parameter is passed with any value, it will export all instruments (and all events, if longitudinal) with data from all records. Note: If this parameter is passed, the parameters record, event, and instrument will be ignored.</param>
        /// <param name="returnFormat">csv, json [default] , xml- The returnFormat is only used with regard to the format of any error messages that might be returned.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>A PDF file containing one or all data collection instruments from the project, in which the instruments will be blank (no data), contain data from a single record, or contain data from all records in the project, depending on the parameters passed in the API request.</returns>
        public async Task<string> ExportPDFInstrumentsAsync(string token, string? recordId = default, string? eventName = default, string? instrument = default, bool allRecord = false, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["content"] = Content.Pdf.GetDisplayName();
                payload["returnFormat"] = returnFormat.GetDisplayName();
                if (!IsNullOrEmpty(recordId)) payload["record"] = recordId!;
                if (!IsNullOrEmpty(eventName)) payload["event"] = eventName!;
                if (!IsNullOrEmpty(instrument)) payload["instrument"] = instrument!;
                if (allRecord) payload["allRecords"] = allRecord.ToString();
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap Version 6.4.0<br/>
        /// **Allows for file download to a path.**
        /// Export PDF file of Data Collection Instruments (either as blank or with data)<br/>
        /// This method allows you to export a PDF file for any of the following: 1) a single data collection instrument (blank), 2) all instruments (blank), 3) a single instrument (with data from a single record), 4) all instruments (with data from a single record), or 5) all instruments (with data from ALL records). 
        /// This is the exact same PDF file that is downloadable from a project's data entry form in the web interface, and additionally, the user's privileges with regard to data exports will be applied here just like they are when downloading the PDF in the web interface (e.g., if they have de-identified data export rights, then it will remove data from certain fields in the PDF). 
        /// If the user has 'No Access' data export rights, they will not be able to use this method, and an error will be returned.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Export privileges in the project.
        /// </remarks>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="recordId">the record ID. The value is blank by default. If record is blank, it will return the PDF as blank (i.e. with no data). If record is provided, it will return a single instrument or all instruments containing data from that record only.</param>
        /// <param name="eventName">the unique event name - only for longitudinal projects. For a longitudinal project, if record is not blank and event is blank, it will return data for all events from that record. If record is not blank and event is not blank, it will return data only for the specified event from that record.</param>
        /// <param name="instrument">the unique instrument name as seen in the second column of the Data Dictionary. The value is blank by default, which returns all instruments. If record is not blank and instrument is blank, it will return all instruments for that record.</param>
        /// <param name="allRecord">[The value of this parameter does not matter and is ignored.] If this parameter is passed with any value, it will export all instruments (and all events, if longitudinal) with data from all records. Note: If this parameter is passed, the parameters record, event, and instrument will be ignored.</param>
        /// <param name="filePath">the path where the file is located</param>
        /// <param name="returnFormat">csv, json [default] , xml- The returnFormat is only used with regard to the format of any error messages that might be returned.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>A PDF file containing one or all data collection instruments from the project, in which the instruments will be blank (no data), contain data from a single record, or contain data from all records in the project, depending on the parameters passed in the API request.</returns>
        public async Task<string> ExportPDFInstrumentsAsync(string token, string? recordId = default, string? eventName = default, string? instrument = default, bool allRecord = false, string? filePath = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            if (!Directory.Exists(filePath) && !IsNullOrEmpty(filePath))
            {
                Log.Warning("The directory provided does not exist! Creating a folder for you.");
                Directory.CreateDirectory(filePath!);
            }
            return await ExecuteDownloadAsync(token, filePath!, payload =>
            {
                payload["token"] = token;
                payload["content"] = Content.Pdf.GetDisplayName();
                payload["returnFormat"] = returnFormat.GetDisplayName();
                if (!IsNullOrEmpty(recordId)) payload["record"] = recordId!;
                if (!IsNullOrEmpty(eventName)) payload["event"] = eventName!;
                if (!IsNullOrEmpty(instrument)) payload["instrument"] = instrument!;
                if (allRecord) payload["allRecords"] = allRecord.ToString();
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap Version 4.7.0 <br/><br/>
        /// 
        /// Export Instrument-Event Mappings<br/><br/>
        /// This method allows you to export the instrument-event mappings for a project (i.e., how the data collection instruments are designated for certain events in a longitudinal project).
        /// NOTE: This only works for longitudinal projects.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Export privileges in the project.
        /// </remarks>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="arms">an array of arm numbers that you wish to pull events for (by default, all events are pulled)</param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Instrument-event mappings for the project in the format specified</returns>
        public async Task<string> ExportInstrumentMappingAsync(string token, RedcapFormat format = RedcapFormat.json, string[]? arms = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["content"] = Content.FormEventMapping.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                payload["returnFormat"] = returnFormat.GetDisplayName();
                if (arms?.Length > 0)
                    for (var i = 0; i < arms.Length; i++)
                        payload[$"arms[{i}]"] = arms[i];
            }, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// From Redcap Version 4.7.0 <br/><br/>
        /// 
        /// Import Instrument-Event Mappings<br/><br/>
        /// This method allows you to import Instrument-Event Mappings into a project (this corresponds to the 'Designate Instruments for My Events' page in the project). 
        /// NOTE: This only works for longitudinal projects.
        /// </summary>
        /// <remarks>
        /// To use this method, you must have API Import/Update privileges *and* Project Design/Setup privileges in the project.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="token">The API token specific to your REDCap project and username (each token is unique to each user for each project). See the section on the left-hand menu for obtaining a token for a given project.</param>
        /// <param name="format">csv, json [default], xml</param>
        /// <param name="data">Contains the attributes 'arm_num' (referring to the arm number), 'unique_event_name' (referring to the auto-generated unique event name of the given event), and 'form' (referring to the unique form name of the given data collection instrument), in which they are provided in the specified format. 
        /// JSON Example:[{"arm_num":"1","unique_event_name":"baseline_arm_1","form":"demographics"},
        /// {"arm_num":"1","unique_event_name":"visit_1_arm_1","form":"day_3"},
        /// {"arm_num":"1","unique_event_name":"visit_1_arm_1","form":"other"},
        /// {"arm_num":"1","unique_event_name":"visit_2_arm_1","form":"other"}]
        /// </param>
        /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
        /// <returns>Number of Instrument-Event Mappings imported</returns>
        public async Task<string> ImportInstrumentMappingAsync<T>(string token, RedcapFormat format, List<T> data, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return await ExecuteAsync(token, payload =>
            {
                payload["token"] = token;
                payload["content"] = Content.FormEventMapping.GetDisplayName();
                payload["format"] = format.GetDisplayName();
                payload["returnFormat"] = returnFormat.GetDisplayName();
                payload["data"] = JsonConvert.SerializeObject(data);
            }, cancellationToken, timeOutSeconds);
        }

    }
}
