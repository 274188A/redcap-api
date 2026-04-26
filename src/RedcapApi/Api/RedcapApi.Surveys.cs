using Redcap.Models;

namespace Redcap;

public partial class RedcapApi
{

    /// <summary>
    /// From Redcap Version 6.4.0<br/>
    /// Export a Survey Link for a Participant
    /// This method returns a unique survey link (i.e., a URL) in plain text format for a specified record and data collection instrument (and event, if longitudinal) in a project. If the user does not have 'Manage Survey Participants' privileges, they will not be able to use this method, and an error will be returned. If the specified data collection instrument has not been enabled as a survey in the project, an error will be returned.
    /// </summary>
    /// <remarks>
    ///
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// <param name="record">the record ID. The name of the record in the project.</param>
    /// <param name="instrument">the unique instrument name as seen in the second column of the Data Dictionary. This instrument must be enabled as a survey in the project.</param>
    /// <param name="eventName">the unique event name (for longitudinal projects only).</param>
    /// <param name="repeatInstance">(only for projects with repeating instruments/events) The repeat instance number of the repeating event (if longitudinal) or the repeating instrument (if classic or longitudinal). Default value is '1'.</param>
    /// <param name="returnFormat">csv, json [default], xml - The returnFormat is only used with regard to the format of any error messages that might be returned.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Returns a unique survey link (i.e., a URL) in plain text format for the specified record and instrument (and event, if longitudinal).</returns>
    public async Task<string> ExportSurveyLinkAsync(string record, string instrument, string eventName, int repeatInstance, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddContent(payload, Content.SurveyLink);
            payload["record"] = record;
            payload["instrument"] = instrument;
            payload["event"] = eventName;
            payload["repeat_instance"] = repeatInstance.ToString();
            AddReturnFormat(payload, returnFormat);
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// From Redcap Version 14.0.0<br/>
    /// Export a Survey Access Code for a Participant<br/>
    /// This method returns a unique survey participant identifier in plain text format for a specified record and data collection instrument (and event, if longitudinal) in a project.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// <param name="record">the record ID. The name of the record in the project.</param>
    /// <param name="instrument">the unique instrument name as seen in the second column of the Data Dictionary. This instrument must be enabled as a survey in the project.</param>
    /// <param name="eventName">the unique event name (for longitudinal projects only).</param>
    /// <param name="repeatInstance">(only for projects with repeating instruments/events) The repeat instance number of the repeating event (if longitudinal) or the repeating instrument (if classic or longitudinal). Default value is '1'.</param>
    /// <param name="returnFormat">csv, json [default], xml - The returnFormat is only used with regard to the format of any error messages that might be returned.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Returns a unique survey participant identifier in plain text format for the specified record and instrument (and event, if longitudinal).</returns>
    public async Task<string> ExportSurveyAccessCodeAsync(string record, string instrument, string eventName, int repeatInstance, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddContent(payload, Content.SurveyAccessCode);
            payload["record"] = record;
            payload["instrument"] = instrument;
            payload["event"] = eventName;
            payload["repeat_instance"] = repeatInstance.ToString();
            AddReturnFormat(payload, returnFormat);
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// Export a Survey Participant List<br/>
    /// This method returns the list of all participants for a specific survey instrument (and for a specific event, if a longitudinal project). If the user does not have 'Manage Survey Participants' privileges, they will not be able to use this method, and an error will be returned. If the specified data collection instrument has not been enabled as a survey in the project, an error will be returned.
    /// </summary>
    /// <remarks>
    ///
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// <param name="instrument">the unique instrument name as seen in the second column of the Data Dictionary. This instrument must be enabled as a survey in the project.</param>
    /// <param name="eventName">the unique event name (for longitudinal projects only).</param>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Returns the list of all participants for the specified survey instrument [and event] in the desired format. The following fields are returned: email, email_occurrence, identifier, invitation_sent_status, invitation_send_time, response_status, survey_access_code, survey_link. The attribute 'email_occurrence' represents the current count that the email address has appeared in the list (because emails can be used more than once), thus email + email_occurrence represent a unique value pair. 'invitation_sent_status' is '0' if an invitation has not yet been sent to the participant, and is '1' if it has. 'invitation_send_time' is the date/time in which the next invitation will be sent, and is blank if there is no invitation that is scheduled to be sent. 'response_status' represents whether the participant has responded to the survey, in which its value is 0, 1, or 2 for 'No response', 'Partial', or 'Completed', respectively. Note: If an incorrect event_id or instrument name is used or if the instrument has not been enabled as a survey, then an error will be returned.</returns>
    public async Task<string> ExportSurveyParticipantsAsync(string instrument, string eventName, RedcapFormat format = RedcapFormat.json, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddFormattedRequest(payload, Content.ParticipantList, format, returnFormat);
            payload["instrument"] = instrument;
            payload["event"] = eventName;
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// From Redcap Version 6.4.0<br/>
    ///
    /// Export a Survey Queue Link for a Participant <br/>
    /// This method returns a unique Survey Queue link (i.e., a URL) in plain text format for the specified record in a project that is utilizing the Survey Queue feature. If the user does not have 'Manage Survey Participants' privileges, they will not be able to use this method, and an error will be returned. If the Survey Queue feature has not been enabled in the project, an error will be
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// <param name="record">the record ID. The name of the record in the project.</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Returns a unique Survey Queue link (i.e., a URL) in plain text format for the specified record in the project.</returns>
    public async Task<string> ExportSurveyQueueLinkAsync(string record, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddContent(payload, Content.SurveyQueueLink);
            payload["record"] = record;
            AddReturnFormat(payload, returnFormat);
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// From Redcap Version 6.4.0<br/>
    /// Export a Survey Return Code for a Participant<br/>
    /// This method returns a unique Return Code in plain text format for a specified record and data collection instrument (and event, if longitudinal) in a project. If the user does not have 'Manage Survey Participants' privileges, they will not be able to use this method, and an error will be returned. If the specified data collection instrument has not been enabled as a survey in the project or does not have the 'Save and Return Later' feature enabled, an error will be returned.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges in the project.
    /// </remarks>
    /// <param name="record">the record ID. The name of the record in the project.</param>
    /// <param name="instrument">the unique instrument name as seen in the second column of the Data Dictionary. This instrument must be enabled as a survey in the project.</param>
    /// <param name="eventName">the unique event name (for longitudinal projects only).</param>
    /// <param name="repeatInstance">(only for projects with repeating instruments/events) The repeat instance number of the repeating event (if longitudinal) or the repeating instrument (if classic or longitudinal). Default value is '1'.</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>Returns a unique Return Code in plain text format for the specified record and instrument (and event, if longitudinal).</returns>
    public async Task<string> ExportSurveyReturnCodeAsync(string record, string instrument, string eventName, string? repeatInstance, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddContent(payload, Content.SurveyReturnCode);
            payload["record"] = record;
            payload["instrument"] = instrument;
            payload["event"] = eventName;
            payload["repeat_instance"] = repeatInstance!;
            AddReturnFormat(payload, returnFormat);
        }, cancellationToken, timeOutSeconds);
    }

}
