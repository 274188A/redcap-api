using Redcap.Models;

namespace Redcap.Interfaces;

/// <summary>
/// REDCap record file API contract.
/// </summary>
public interface IRedcapFiles
{
    /// <summary>
    /// Export a File<br/><br/>
    /// This method allows you to download a document that has been attached to an individual record for a File Upload field. Please note that this method may also be used for Signature fields (i.e. File Upload fields with 'signature' validation type).
    /// Note about export rights: Please be aware that Data Export user rights will be applied to this API request.For example, if you have 'No Access' data export rights in the project, then the API file export will fail and return an error. And if you have 'De-Identified' or 'Remove all tagged Identifier fields' data export rights, then the API file export will fail and return an error *only if* the File Upload field has been tagged as an Identifier field.To make sure that your API request does not return an error, you should have 'Full Data Set' export rights in the project.
    /// <br/><br/>
    /// How to obtain the filename of the file:
    /// The MIME type of the file, along with the name of the file and its extension, can be found in the header of the returned response.Thus in order to determine these attributes of the file being exported, you will need to parse the response header. Example: content-type = application / vnd.openxmlformats - officedocument.wordprocessingml.document; name='FILE_NAME.docx'
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges in the project.<br/>
    ///
    /// </remarks>
    /// <example>
    /// The MIME type of the file, along with the name of the file and its extension, can be found in the header of the returned response. Thus in order to determine these attributes of the file being exported, you will need to parse the response header. Example: content-type = application/vnd.openxmlformats-officedocument.wordprocessingml.document; name='FILE_NAME.docx'
    /// </example>
    /// <param name="record">the record ID</param>
    /// <param name="field">the name of the field that contains the file</param>
    /// <param name="eventName">the unique event name - only for longitudinal projects</param>
    /// <param name="repeatInstance">(only for projects with repeating instruments/events) The repeat instance number of the repeating event (if longitudinal) or the repeating instrument (if classic or longitudinal). Default value is '1'.</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'xml'.</param>
    /// <param name="filePath">File path which the file will be saved.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>the contents of the file</returns>
    Task<string> ExportFileAsync(string? record, string field, string eventName, string? repeatInstance = "1", RedcapReturnFormat returnFormat = RedcapReturnFormat.json, string? filePath = null, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// Import a File <br/><br/>
    /// This method allows you to upload a document that will be attached to an individual record for a File Upload field. Please note that this method may NOT be used for Signature fields (i.e. File Upload fields with 'signature' validation type) because a signature can only be captured and stored using the web interface.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges in the project.
    /// If you pass in a record parameter that does not exist, Redcap will create it for you.
    /// </remarks>
    /// <param name="record">the record ID</param>
    /// <param name="field">the name of the field that contains the file</param>
    /// <param name="eventName">the unique event name - only for longitudinal projects</param>
    /// <param name="repeatInstance">(only for projects with repeating instruments/events) The repeat instance number of the repeating event (if longitudinal) or the repeating instrument (if classic or longitudinal). Default value is '1'.</param>
    /// <param name="fileName">The File you be imported, contents of the file</param>
    /// <param name="filePath">the path where the file is located</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'xml'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</returns>
    Task<string> ImportFileAsync(string record, string field, string eventName, string? repeatInstance, string fileName, string filePath, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// Delete a File <br/><br/>
    /// This method allows you to remove a document that has been attached to an individual record for a File Upload field. Please note that this method may also be used for Signature fields (i.e. File Upload fields with 'signature' validation type).
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges in the project.
    /// </remarks>
    /// <param name="record">the record ID</param>
    /// <param name="field">the name of the field that contains the file</param>
    /// <param name="eventName">the unique event name - only for longitudinal projects</param>
    /// <param name="repeatInstance">(only for projects with repeating instruments/events) The repeat instance number of the repeating event (if longitudinal) or the repeating instrument (if classic or longitudinal). Default value is '1'.</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>String</returns>
    Task<string> DeleteFileAsync(string record, string field, string eventName, string? repeatInstance, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);
}
