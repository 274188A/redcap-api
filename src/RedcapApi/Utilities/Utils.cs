using Redcap.Http;
using Redcap.Models;

using Serilog;

using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

using static System.String;

namespace Redcap.Utilities
{
    /// <summary>
    /// Utilities
    /// </summary>
    public static class Utils
    {

        /// <summary>
        /// Method gets the display string for an enum. Falls back to the enum name when no [Display] attribute is present.
        /// </summary>
        public static string GetDisplayName(this Enum enumString)
        {
            var member = enumString.GetType().GetMember(enumString.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? enumString.ToString();
        }

        /// <summary>
        /// Writes the response body to a file under <paramref name="path"/>, sanitising the filename so it cannot escape that directory.
        /// </summary>
        public static async Task ReadAsFileAsync(this HttpContent httpContent, string fileName, string path, bool overwrite, string fileExtension = "pdf", CancellationToken cancellationToken = default)
        {
            if (httpContent == null) throw new ArgumentNullException(nameof(httpContent));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path must be provided.", nameof(path));

            // Strip directory separators from caller- or server-supplied names.
            var rawName = (fileName ?? string.Empty).Replace("\"", "");
            var safeName = Path.GetFileName(rawName);
            if (string.IsNullOrWhiteSpace(safeName))
            {
                throw new ArgumentException("fileName did not contain a usable file name.", nameof(fileName));
            }

            if (!string.IsNullOrEmpty(fileExtension))
            {
                safeName = safeName + "." + fileExtension;
            }

            var rootPath = Path.GetFullPath(path);
            var targetPath = Path.GetFullPath(Path.Combine(rootPath, safeName));
            // Residual-traversal guard: Path.GetFileName above should already block '..', but normalise and recheck.
            var sep = Path.DirectorySeparatorChar.ToString();
            if (!string.Equals(targetPath, rootPath, StringComparison.Ordinal) &&
                !targetPath.StartsWith(rootPath.EndsWith(sep) ? rootPath : rootPath + sep, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Resolved file path escapes the target directory.");
            }

            if (!overwrite && File.Exists(targetPath))
            {
                throw new InvalidOperationException($"File {safeName} already exists.");
            }

            FileStream? filestream = null;
            try
            {
                filestream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await httpContent.CopyToAsync(filestream, cancellationToken);
                await filestream.FlushAsync(cancellationToken);
            }
            catch (Exception Ex)
            {
                Log.Error(Ex, "Failed writing response to {TargetPath}", targetPath);
                throw new InvalidOperationException(Ex.Message, Ex);
            }
            finally
            {
                if (filestream != null)
                {
                    await filestream.DisposeAsync();
                }
            }
        }
        /// https://stackoverflow.com/questions/8560106/isnullorempty-equivalent-for-array-c-sharp
        /// <summary>Indicates whether the specified array is null or has a length of zero.</summary>
        /// <param name="array">The array to test.</param>
        /// <returns>true if the array parameter is null or has a length of zero; otherwise, false.</returns>
        public static bool IsNullOrEmpty<T>(this T[]? array)
        {
            return (array == null || array.Length == 0);
        }
        /// <summary>
        /// This method converts string[] into string. For example, given string of "firstName, lastName, age"
        /// gets converted to "["firstName","lastName","age"]" 
        /// This is used as optional arguments for the Redcap Api
        /// </summary>
        /// <param name="redcapApi"></param>
        /// <param name="inputArray"></param>
        /// <returns>string[]</returns>
        public static async Task<string> ConvertArraytoString<T>(this RedcapApi redcapApi, T[] inputArray)
        {
            try
            {
                if (inputArray.IsNullOrEmpty())
                {
                    throw new ArgumentNullException("Please provide a valid array.");
                }
                StringBuilder builder = new StringBuilder();
                foreach (T v in inputArray)
                {

                    builder.Append(v);
                    // We do not need to append the , if less than or equal to a single string
                    if (inputArray.Length <= 1)
                    {
                        return await Task.FromResult(builder.ToString());
                    }
                    builder.Append(",");
                }
                // We trim the comma from the string for clarity
                return await Task.FromResult(builder.ToString().TrimEnd(','));

            }
            catch (Exception Ex)
            {
                Log.Error(Ex, "REDCap API call failed");
                return await Task.FromResult(String.Empty);
            }
        }
        /// <summary>
        /// This method converts int[] into a string. For example, given int[] of "[1,2,3]"
        /// gets converted to "["1","2","3"]" 
        /// This is used as optional arguments for the Redcap Api
        /// </summary>
        /// <param name="redcapApi"></param>
        /// <param name="inputArray"></param>
        /// <returns>string</returns>
        public static async Task<string> ConvertIntArraytoString(this RedcapApi redcapApi, int[] inputArray)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                foreach (var intValue in inputArray)
                {

                    builder.Append(intValue);
                    // We do not need to append the , if less than or equal to a single string
                    if (inputArray.Length <= 1)
                    {
                        return await Task.FromResult(builder.ToString());
                    }
                    builder.Append(",");
                }
                // We trim the comma from the string for clarity
                return await Task.FromResult(builder.ToString().TrimEnd(','));

            }
            catch (Exception Ex)
            {
                Log.Error(Ex, "REDCap API call failed");
                return await Task.FromResult(String.Empty);
            }
        }
        /// <summary>
        ///The method hands the return content from a request, the response.
        /// The method allows the calling method to choose a return type.
        /// </summary>
        /// <param name="redcapApi"></param>
        /// <param name="returnContent"></param>
        /// <returns>string</returns>
        public static async Task<string> HandleReturnContent(this RedcapApi redcapApi, ReturnContent returnContent = ReturnContent.count)
        {
            try
            {
                var _returnContent = returnContent.ToString();
                switch (returnContent)
                {
                    case ReturnContent.ids:
                        _returnContent = ReturnContent.ids.ToString();
                        break;
                    case ReturnContent.count:
                        _returnContent = ReturnContent.count.ToString();
                        break;
                    default:
                        _returnContent = ReturnContent.count.ToString();
                        break;
                }
                return await Task.FromResult(_returnContent);
            }
            catch (Exception Ex)
            {
                Log.Error(Ex, "REDCap API call failed");
                return await Task.FromResult(String.Empty);
            }
        }
        /// <summary>
        /// Tuple that returns both inputFormat and redcap returnFormat
        /// </summary>
        /// <param name="redcapApi"></param>
        /// <param name="format">csv, json[default], xml , odm ('odm' refers to CDISC ODM XML format, specifically ODM version 1.3.1)</param>
        /// <param name="onErrorFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <returns>tuple, string, string, string</returns>
        public static async Task<(string format, string onErrorFormat, string redcapDataType)> HandleFormat(this RedcapApi redcapApi, RedcapFormat? format = RedcapFormat.json, RedcapReturnFormat? onErrorFormat = RedcapReturnFormat.json, RedcapDataType? redcapDataType = RedcapDataType.flat)
        {
            // default
            var _format = RedcapFormat.json.ToString();
            var _onErrorFormat = RedcapReturnFormat.json.ToString();
            var _redcapDataType = RedcapDataType.flat.ToString();

            try
            {

                switch (format)
                {
                    case RedcapFormat.json:
                        _format = RedcapFormat.json.ToString();
                        break;
                    case RedcapFormat.csv:
                        _format = RedcapFormat.csv.ToString();
                        break;
                    case RedcapFormat.xml:
                        _format = RedcapFormat.xml.ToString();
                        break;
                    case RedcapFormat.odm:
                        _format = RedcapFormat.odm.ToString();
                        break;
                    default:
                        _format = RedcapFormat.json.ToString();
                        break;
                }

                switch (onErrorFormat)
                {
                    case RedcapReturnFormat.json:
                        _onErrorFormat = RedcapReturnFormat.json.ToString();
                        break;
                    case RedcapReturnFormat.csv:
                        _onErrorFormat = RedcapReturnFormat.csv.ToString();
                        break;
                    case RedcapReturnFormat.xml:
                        _onErrorFormat = RedcapReturnFormat.xml.ToString();
                        break;
                    default:
                        _onErrorFormat = RedcapReturnFormat.json.ToString();
                        break;
                }

                switch (redcapDataType)
                {
                    case RedcapDataType.flat:
                        _redcapDataType = RedcapDataType.flat.ToString();
                        break;
                    case RedcapDataType.eav:
                        _redcapDataType = RedcapDataType.eav.ToString();
                        break;
                    case RedcapDataType.longitudinal:
                        _redcapDataType = RedcapDataType.longitudinal.ToString();
                        break;
                    case RedcapDataType.nonlongitudinal:
                        _redcapDataType = RedcapDataType.nonlongitudinal.ToString();
                        break;
                    default:
                        _redcapDataType = RedcapDataType.flat.ToString();
                        break;
                }

                return await Task.FromResult((_format, _onErrorFormat, _redcapDataType));
            }
            catch (Exception Ex)
            {
                Log.Error(Ex, "REDCap API call failed");
                return await Task.FromResult((_format, _onErrorFormat, _redcapDataType));
            }
        }

        /// <summary>
        /// Method gets the overwrite behavior type and converts into string
        /// </summary>
        /// <param name="redcapApi"></param>
        /// <param name="overwriteBehavior"></param>
        /// <returns>string</returns>
        public static async Task<string> ExtractBehaviorAsync(this RedcapApi redcapApi, OverwriteBehavior overwriteBehavior)
        {
            try
            {
                var _overwriteBehavior = OverwriteBehavior.normal.ToString();
                switch (overwriteBehavior)
                {
                    case OverwriteBehavior.overwrite:
                        _overwriteBehavior = OverwriteBehavior.overwrite.ToString();
                        break;
                    case OverwriteBehavior.normal:
                        _overwriteBehavior = OverwriteBehavior.normal.ToString();
                        break;
                    default:
                        _overwriteBehavior = OverwriteBehavior.overwrite.ToString();
                        break;
                }
                return await Task.FromResult(_overwriteBehavior);
            }
            catch (Exception Ex)
            {
                Log.Error(Ex, "REDCap API call failed");
                return await Task.FromResult(String.Empty);
            }
        }
        /// <summary>
        /// This method extracts and converts an object's properties and associated values to redcap type and values.
        /// </summary>
        /// <param name="redcapApi"></param>
        /// <param name="input">Object</param>
        /// <returns>Dictionary of key value pair.</returns>
        public static async Task<Dictionary<string, string?>> GetProperties(this RedcapApi redcapApi, object input)
        {
            try
            {
                if (input != null)
                {
                    // Get the type
                    var type = input.GetType();
                    var obj = new Dictionary<string, string?>();
                    // Get the properties
                    var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    PropertyInfo[] properties = input.GetType().GetProperties();
                    foreach (var prop in properties)
                    {
                        // get type of column
                        // The the type of the property
                        Type columnType = prop.PropertyType;

                        // We need to set lower case for REDCap's variable nameing convention (lower casing)
                        string propName = prop.Name.ToLower();
                        // We check for null values
                        var propValue = type.GetProperty(prop.Name)?.GetValue(input, null)?.ToString();
                        if (propValue != null)
                        {
                            var t = columnType.GetGenericArguments();
                            if (t.Length > 0)
                            {
                                if (columnType.GenericTypeArguments[0].FullName == "System.DateTime")
                                {
                                    var dt = DateTime.Parse(propValue);
                                    propValue = dt.ToString();
                                }
                                if (columnType.GenericTypeArguments[0].FullName == "System.Boolean")
                                {
                                    if (propValue == "True")
                                    {
                                        propValue = "1";
                                    }
                                    else
                                    {
                                        propValue = "0";
                                    }
                                }
                            }
                            obj.Add(propName, propValue);
                        }
                        else
                        {
                            // We have to make sure we handle for null values.
                            obj.Add(propName, null);
                        }
                    }
                    return await Task.FromResult(obj);
                }
                return await Task.FromResult(new Dictionary<string, string?> { });
            }
            catch (Exception Ex)
            {
                Log.Error(Ex, "REDCap API call failed");
                return await Task.FromResult(new Dictionary<string, string?> { });
            }
        }
        /// <summary>
        /// Method extracts events into list from string
        /// </summary>
        /// <param name="redcapApi"></param>
        /// <param name="events"></param>
        /// <param name="delimiters">char[] e.g [';',',']</param>
        /// <returns>List of string</returns>
        public static async Task<List<string>> ExtractEventsAsync(this RedcapApi redcapApi, string events, char[] delimiters)
        {
            if (!String.IsNullOrEmpty(events))
            {
                try
                {
                    var formItems = events.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    List<string> eventsResult = new List<string>();
                    foreach (var form in formItems)
                    {
                        eventsResult.Add(form);
                    }
                    return await Task.FromResult(eventsResult);
                }
                catch (Exception Ex)
                {
                    Log.Error(Ex, "REDCap API call failed");
                    return await Task.FromResult(new List<string> { });
                }
            }
            return await Task.FromResult(new List<string> { });
        }
        /// <summary>
        /// Method gets / extracts fields into list from string
        /// </summary>
        /// <param name="redcapApi"></param>
        /// <param name="fields"></param>
        /// <param name="delimiters">char[] e.g [';',',']</param>
        /// <returns>List of string</returns>
        public static async Task<List<string>> ExtractFieldsAsync(this RedcapApi redcapApi, string fields, char[] delimiters)
        {
            if (!String.IsNullOrEmpty(fields))
            {
                try
                {
                    var fieldItems = fields.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    List<string> fieldsResult = new List<string>();
                    foreach (var field in fieldItems)
                    {
                        fieldsResult.Add(field);
                    }
                    return await Task.FromResult(fieldsResult);
                }
                catch (Exception Ex)
                {
                    Log.Error(Ex, "REDCap API call failed");
                    return await Task.FromResult(new List<string> { });
                }
            }
            return await Task.FromResult(new List<string> { });
        }
        /// <summary>
        /// Method gets / extract records into list from string
        /// </summary>
        /// <param name="redcapApi"></param>
        /// <param name="records"></param>
        /// <param name="delimiters">char[] e.g [';',',']</param>
        /// <returns>List of string</returns>
        public static async Task<List<string>> ExtractRecordsAsync(this RedcapApi redcapApi, string records, char[] delimiters)
        {
            if (!String.IsNullOrEmpty(records))
            {
                try
                {
                    var recordItems = records.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    List<string> recordResults = new List<string>();
                    foreach (var record in recordItems)
                    {
                        recordResults.Add(record);
                    }
                    return await Task.FromResult(recordResults);
                }
                catch (Exception Ex)
                {
                    Log.Error(Ex, "REDCap API call failed");
                    return await Task.FromResult(new List<string> { });
                }
            }
            return await Task.FromResult(new List<string> { });
        }
        /// <summary>
        /// Method gets / extracts forms into list from string
        /// </summary>
        /// <param name="redcapApi"></param>
        /// <param name="forms"></param>
        /// <param name="delimiters">char[] e.g [';',',']</param>
        /// <returns>A list of string</returns>
        public static async Task<List<string>> ExtractFormsAsync(this RedcapApi redcapApi, string forms, char[] delimiters)
        {
            if (!String.IsNullOrEmpty(forms))
            {
                try
                {
                    var formItems = forms.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    List<string> formsResult = new List<string>();
                    foreach (var form in formItems)
                    {
                        formsResult.Add(form);
                    }
                    return await Task.FromResult(formsResult);
                }
                catch (Exception Ex)
                {
                    Log.Error(Ex, "REDCap API call failed");
                    return await Task.FromResult(new List<string> { });
                }
            }
            return await Task.FromResult(new List<string> { });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="redcapApi"></param>
        /// <param name="payload">data</param>
        /// <param name="uri">URI of the api instance</param>
        /// <param name="client">Caller-owned <see cref="HttpClient"/>.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Stream</returns>
        public static async Task<Stream?> GetStreamContentAsync(this RedcapApi redcapApi, Dictionary<string, string> payload, Uri uri, HttpClient client, CancellationToken cancellationToken = default)
        {
            try
            {
                var content = new FormUrlEncodedContent(payload);
                using var response = await client.PostAsync(uri, content, cancellationToken);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStreamAsync();
                return null;
            }
            catch (Exception Ex)
            {
                Log.Error(Ex, "REDCap API call failed");
                return null;
            }
        }

        /// <summary>
        /// Method to send http request using MultipartFormDataContent
        /// Requests with attachments
        /// </summary>
        /// <param name="redcapApi"></param>
        /// <param name="payload">data</param>
        /// <param name="uri">URI of the api instance</param>
        /// <param name="client">Caller-owned <see cref="HttpClient"/>.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>string</returns>
        public static async Task<string> SendPostRequestAsync(this RedcapApi redcapApi, MultipartFormDataContent payload, Uri uri, HttpClient client, CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await client.PostAsync(uri, payload, cancellationToken);
                return response.IsSuccessStatusCode
                    ? await response.Content.ReadAsStringAsync()
                    : Empty;
            }
            catch (Exception Ex)
            {
                Log.Error(Ex, "REDCap API call failed");
                return Empty;
            }
        }

        /// <summary>
        /// Sends request using http
        /// </summary>
        /// <param name="redcapApi"></param>
        /// <param name="payload">data</param>
        /// <param name="uri">URI of the api instance</param>
        /// <param name="client">Caller-owned <see cref="HttpClient"/>.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<string> SendPostRequestAsync(this RedcapApi redcapApi, Dictionary<string, string> payload, Uri uri, HttpClient client, CancellationToken cancellationToken = default)
        {
            try
            {
                string _responseMessage = string.Empty;

                // extract the filepath
                var pathValue = payload.FirstOrDefault(x => x.Key == "filePath").Value;
                var pathkey = payload.FirstOrDefault(x => x.Key == "filePath").Key;

                if (!string.IsNullOrEmpty(pathkey))
                {
                    // the actual payload does not contain a 'filePath' key
                    payload.Remove(pathkey);
                }

                using var content = new CustomFormUrlEncodedContent(payload);
                using var response = await client.PostAsync(uri, content, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    // Get the filename so we can save with the name
                    var headers = response.Content.Headers;
                    var fileName = headers.ContentType?.Parameters.Select(x => x.Value).FirstOrDefault();
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var contentDisposition = response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = fileName
                        };
                    }


                    if (!string.IsNullOrEmpty(pathValue))
                    {
                        var fileExtension = payload.SingleOrDefault(x => x.Key == "content" && x.Value == "pdf").Value;
                        if (!string.IsNullOrEmpty(fileExtension))
                        {
                            // pdf 
                            fileName = payload.SingleOrDefault(x => x.Key == "instrument").Value;
                            // to do , make extensions for various types
                            // save the file to a specified location using an extension method
                            await response.Content.ReadAsFileAsync(fileName!, pathValue, true, fileExtension, cancellationToken: cancellationToken);

                        }
                        else
                        {
                            await response.Content.ReadAsFileAsync(fileName!, pathValue, true, cancellationToken: cancellationToken);

                        }
                        _responseMessage = fileName ?? string.Empty;
                    }
                    else
                    {
                        _responseMessage = await response.Content.ReadAsStringAsync();
                    }
                }
                else
                {
                    _responseMessage = await response.Content.ReadAsStringAsync();
                }

                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error(Ex, "REDCap API call failed");
                return Empty;
            }
        }
        /// <summary>
        /// Sends http request to api
        /// </summary>
        /// <param name="redcapApi"></param>
        /// <param name="payload">data </param>
        /// <param name="uri">URI of the api instance</param>
        /// <param name="client">Caller-owned <see cref="HttpClient"/>.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>string</returns>
        public static async Task<string> SendPostRequest(this RedcapApi redcapApi, Dictionary<string, string> payload, Uri uri, HttpClient client, CancellationToken cancellationToken = default)
        {
            using var content = new FormUrlEncodedContent(payload);
            using var response = await client.PostAsync(uri, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// Method obtains list of string from comma seperated strings
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="redcapApi"></param>
        /// <param name="arms"></param>
        /// <param name="delimiters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of string</returns>
        public static async Task<List<string>> ExtractArmsAsync<T>(this RedcapApi redcapApi, string arms, char[] delimiters, CancellationToken cancellationToken = default)
        {
            if (!String.IsNullOrEmpty(arms))
            {
                try
                {
                    var _arms = arms.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    List<string> armsResult = new List<string>();
                    foreach (var arm in _arms)
                    {
                        armsResult.Add(arm);
                    }
                    return await Task.FromResult(armsResult);
                }
                catch (Exception Ex)
                {
                    Log.Error(Ex, "REDCap API call failed");
                    return await Task.FromResult(new List<string> { });
                }
            }
            return await Task.FromResult(new List<string> { });

        }
        /// <summary>
        /// Checks if the string passed is null or empty.
        /// </summary>
        /// <param name="redcapApi"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static void CheckToken(this RedcapApi redcapApi, string token)
        {
            /*
             * Check the required parameters for empty or null
             */
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("Please provide a valid Redcap token.");
            }
        }

    }
}
