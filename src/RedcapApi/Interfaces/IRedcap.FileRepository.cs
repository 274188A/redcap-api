using Redcap.Models;

namespace Redcap.Interfaces;

/// <summary>
/// REDCap file repository API contract.
/// </summary>
public interface IRedcapFileRepository
{
    /// <summary>
    /// From Redcap Version 13.1 <br/><br/>
    ///
    /// Create a New Folder in the File Repository <br/><br/>
    ///
    /// This method allows you to create a new folder in the File Repository.<br/>
    /// You may optionally provide the folder_id of the parent folder under which you wish this folder to be created.<br/>
    /// Providing a dag_id and/or role_id will allow you to restrict access to only users within a specific DAG (Data Access Group) or User Role, respectively.
    ///
    /// </summary>
    ///
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges and File Repository privileges in the project.
    /// </remarks>
    /// <param name="content">fileRepository</param>
    /// <param name="action">createFolder</param>
    /// <param name="name">The desired name of the folder to be created (max length = 150 characters)</param>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="folderId">the folder_id of a specific folder in the File Repository for which you wish to create this sub-folder. If none is provided, the folder will be created in the top-level directory of the File Repository.</param>
    /// <param name="dagId">the dag_id of the DAG (Data Access Group) to which you wish to restrict access for this folder. If none is provided, the folder will accessible to users in all DAGs and users in no DAGs.</param>
    /// <param name="roleId">the role_id of the User Role to which you wish to restrict access for this folder. If none is provided, the folder will accessible to users in all User Roles and users in no User Roles.</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>The folder_id of the new folder created in the specified format. <br/>For example, if using format=json, the output would look similar to this: [{folder_id:45}].</returns>
    Task<string> CreateFolderFileRepositoryAsync(Content content = Content.FileRepository, RedcapAction action = RedcapAction.CreateFolder, string? name = null, RedcapFormat format = RedcapFormat.json, string? folderId = null, string? dagId = null, string? roleId = null, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// From Redcap Version 13.1<br/>
    ///
    /// Export a List of Files/Folders from the File Repository<br/><br/>
    ///
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Export privileges and File Repository privileges in the project.
    /// </remarks>
    /// <param name="content">fileRepository</param>
    /// <param name="action">list</param>
    /// <param name="format">csv, json [default], xml</param>
    /// <param name="folderId">the folder_id of a specific folder in the File Repository for which you wish to export a list of its files and sub-folders. If none is provided, the top-level directory of the File Repository will be used.</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>The list of all files and folders within a given sub-folder in the File Repository in the format specified.</returns>
    Task<string> ExportFilesFoldersFileRepositoryAsync(Content content = Content.FileRepository, RedcapAction action = RedcapAction.List, RedcapFormat format = RedcapFormat.json, string? folderId = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

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
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>the contents of the file</returns>
    Task<string> ExportFileFileRepositoryAsync(Content content = Content.FileRepository, RedcapAction action = RedcapAction.Export, string? docId = null, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// From Redcap Version 13.1<br/>
    /// Import a File into the File Repository<br/>
    ///
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges and File Repository privileges in the project.
    /// </remarks>
    /// <param name="content">fileRepository</param>
    /// <param name="action">import</param>
    /// <param name="file">the contents of the file</param>
    /// <param name="folderId">the folder_id of a specific folder in the File Repository where you wish to store the file. If none is provided, the file will be stored in the top-level directory of the File Repository.</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>string</returns>
    Task<string> ImportFileRepositoryAsync(Content content = Content.FileRepository, RedcapAction action = RedcapAction.Import, string? file = null, string? folderId = default, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

    /// <summary>
    /// From Redcap Version 13.1<br/>
    /// Delete a File from the File Repository<br/>
    ///
    /// </summary>
    /// <remarks>
    /// To use this method, you must have API Import/Update privileges and File Repository privileges in the project.
    /// </remarks>
    /// <param name="content">fileRepository</param>
    /// <param name="action">delete</param>
    /// <param name="docId">the doc_id of the file in the File Repository</param>
    /// <param name="returnFormat">csv, json, xml - specifies the format of error messages. If you do not pass in this flag, it will select the default format for you passed based on the 'format' flag you passed in or if no format flag was passed in, it will default to 'json'.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeOutSeconds">Number of seconds before the http request times out.</param>
    /// <returns>string</returns>
    Task<string> DeleteFileRepositoryAsync(Content content = Content.FileRepository, RedcapAction action = RedcapAction.Delete, string? docId = null, RedcapReturnFormat returnFormat = RedcapReturnFormat.json, CancellationToken cancellationToken = default, long timeOutSeconds = 100);
}
