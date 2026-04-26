using Redcap.Exceptions;
using Redcap.Models;

using static System.String;

namespace Redcap;

public partial class RedcapApi
{

    /// <summary>
    /// From Redcap Version 13.1 <br/><br/>
    ///
    /// Create a New Folder in the File Repository <br/><br/>
    ///
    /// This method allows you to create a new folder in the File Repository.
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges and File Repository privileges in the project.
    /// </remarks>
    /// <param name="content">fileRepository</param>
    /// <param name="action">createFolder</param>
    /// <param name="name">The desired name of the folder to be created (max length = 150 characters)</param>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="folderId">the folder_id of a specific folder in the File Repository for which you wish to create this sub-folder.</param>
    /// <param name="dagId">the dag_id of the DAG to which you wish to restrict access for this folder.</param>
    /// <param name="roleId">the role_id of the User Role to which you wish to restrict access for this folder.</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>The folder_id of the new folder created in the specified format.</returns>
    public async Task<string> CreateFolderFileRepositoryAsync(Content content, RedcapAction action, string? name, RedcapFormat format, string? folderId = default, string? dagId = default, string? roleId = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return IsNullOrEmpty(name)
            ? throw new RedcapApiException("Please provide a valid name for the folder to create in the Repository.")
            : await ExecuteAsync(payload =>
        {
            AddActionRequest(payload, content, action, returnFormat);
            AddFormat(payload, format);
            payload["name"] = name!;
            AddOptional(payload, "folder_id", folderId);
            AddOptional(payload, "dag_id", dagId);
            AddOptional(payload, "role_id", roleId);
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// From Redcap Version 13.1<br/>
    ///
    /// Export a List of Files/Folders from the File Repository<br/><br/>
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges and File Repository privileges in the project.
    /// </remarks>
    /// <param name="content">fileRepository</param>
    /// <param name="action">list</param>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="folderId">the folder_id of a specific folder in the File Repository for which you wish to export a list of its files and sub-folders.</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>The list of all files and folders within a given sub-folder in the File Repository in the format specified.</returns>
    public async Task<string> ExportFilesFoldersFileRepositoryAsync(Content content = Content.FileRepository, RedcapAction action = RedcapAction.List, RedcapFormat format = RedcapFormat.json, string? folderId = null, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddActionRequest(payload, content, action, returnFormat);
            AddFormat(payload, format);
            AddOptional(payload, "folder_id", folderId);
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// From Redcap Version 13.1<br/>
    /// Export a File from the File Repository<br/>
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges and File Repository privileges in the project.
    /// </remarks>
    /// <param name="content">fileRepository</param>
    /// <param name="action">export</param>
    /// <param name="docId">the doc_id of the file in the File Repository</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>the contents of the file</returns>
    public async Task<string> ExportFileFileRepositoryAsync(Content content, RedcapAction action, string? docId = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return await ExecuteAsync(payload =>
        {
            AddActionRequest(payload, content, action, returnFormat);
            AddOptional(payload, "doc_id", docId);
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// From Redcap Version 13.1<br/>
    /// Import a File into the File Repository<br/>
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges and File Repository privileges in the project.
    /// </remarks>
    /// <param name="content">fileRepository</param>
    /// <param name="action">import</param>
    /// <param name="file">the contents of the file</param>
    /// <param name="folderId">the folder_id of a specific folder in the File Repository where you wish to store the file.</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>string</returns>
    public async Task<string> ImportFileRepositoryAsync(Content content = Content.FileRepository, RedcapAction action = RedcapAction.Import, string? file = null, string? folderId = null, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return IsNullOrEmpty(file)
            ? throw new RedcapApiException("Please provide a file to import.")
            : await ExecuteAsync(payload =>
        {
            AddActionRequest(payload, content, action, returnFormat);
            payload["file"] = file!;
            AddOptional(payload, "folder_id", folderId);
        }, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// From Redcap Version 13.1<br/>
    /// Delete a File from the File Repository<br/>
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges and File Repository privileges in the project.
    /// </remarks>
    /// <param name="content">fileRepository</param>
    /// <param name="action">delete</param>
    /// <param name="docId">the doc_id of the file in the File Repository</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>string</returns>
    public async Task<string> DeleteFileRepositoryAsync(Content content = Content.FileRepository, RedcapAction action = RedcapAction.Delete, string? docId = null, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        return IsNullOrEmpty(docId)
            ? throw new RedcapApiException("Please provide a document id to delete.")
            : await ExecuteAsync(payload =>
        {
            AddActionRequest(payload, content, action, returnFormat);
            payload["doc_id"] = docId!;
        }, cancellationToken, timeOutSeconds);
    }
}
