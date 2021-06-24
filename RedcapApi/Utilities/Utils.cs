using Redcap.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Redcap.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class TestPriorityAttribute : Attribute
    {

       
        /// <param name="priority"></param>
        public TestPriorityAttribute(int priority)
        {
            var _priority = priority;

            
        }

    }

 
    /// <summary>
    /// Utilities
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Controls how strictly a certificate's credentials are observed, for example over ssh tunnels.
        /// </summary>
        internal static bool UseInsecureCertificate = false;

        /// <summary>
        /// Method gets the display string for an enum
        /// </summary>
        /// <param name="enumString"></param>
        /// <returns></returns>
        public static string GetDisplayName(this Enum enumString)
        {
            var returnvalue = enumString.GetType()
                .GetMember(enumString.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                ?.GetName();

            return returnvalue;
        }

        /// <summary>
        /// Extension method reads a stream and saves content to a local file.
        /// </summary>
        /// <param name="httpContent"></param>
        /// <param name="fileName"></param>
        /// <param name="path"></param>
        /// <param name="overwrite"></param>
        /// <param name="fileExtension"></param>
        /// <returns>HttpContent</returns>
        public static Task ReadAsFileAsync(this HttpContent httpContent, string fileName, string path, bool overwrite,
            string fileExtension = "pdf")
        {
            if (!overwrite && File.Exists(Path.Combine(fileName + fileExtension?.SingleOrDefault(), path)))
            {
                throw new InvalidOperationException($"File {fileName} already exists.");
            }

            FileStream filestream = null;
            try
            {
                fileName = fileName.Replace("\"", "");
                /*
                 * Add extension
                 */
                if (!string.IsNullOrEmpty(fileExtension))
                {
                    fileName = fileName + "." + fileExtension;
                }

                filestream = new FileStream(Path.Combine(path, fileName), FileMode.Create, FileAccess.Write,
                    FileShare.None);
                return httpContent.CopyToAsync(filestream).ContinueWith(
                    _ =>
                    {
                        filestream.Flush();
                        filestream.Dispose();
                    }
                );
            }
            catch (Exception Ex)
            {
                Log.Error(Ex.Message);
                if (filestream != null)
                {
                    filestream.Flush();
                }

                throw new InvalidOperationException($"{Ex.Message}");
            }
        }

        /// https://stackoverflow.com/questions/8560106/isnullorempty-equivalent-for-array-c-sharp
        /// <summary>Indicates whether the specified array is null or has a length of zero.</summary>
        /// <param name="array">The array to test.</param>
        /// <returns>true if the array parameter is null or has a length of zero; otherwise, false.</returns>
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return (array == null || array.Length == 0);
        }

        /// <summary>
        /// This method converts string[] into string. For example, given string of "firstName, lastName, age"
        /// gets converted to "["firstName","lastName","age"]" 
        /// This is used as optional arguments for the Redcap Api
        /// </summary>
        /// <param name="redcap"></param>
        /// <param name="inputArray"></param>
        /// <returns>string[]</returns>
        public static async Task<string> ConvertArraytoString<T>(this Api.Redcap redcap, T[] inputArray)
        {
            try
            {
                if (inputArray.IsNullOrEmpty())
                {
                    throw new ArgumentNullException(nameof(inputArray));
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
                Log.Error($"{Ex.Message}");
                return await Task.FromResult(string.Empty);
            }
        }

        /// <summary>
        /// This method converts int[] into a string. For example, given int[] of "[1,2,3]"
        /// gets converted to "["1","2","3"]" 
        /// This is used as optional arguments for the Redcap Api
        /// </summary>
        /// <param name="redcap"></param>
        /// <param name="inputArray"></param>
        /// <returns>string</returns>
        public static async Task<string> ConvertIntArraytoString(this Api.Redcap redcap, int[] inputArray)
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
                Log.Error($"{Ex.Message}");
                return await Task.FromResult(string.Empty);
            }
        }

        /// <summary>
        ///The method hands the return content from a request, the response.
        /// The method allows the calling method to choose a return type.
        /// </summary>
        /// <param name="redcap"></param>
        /// <param name="returnContent"></param>
        /// <returns>string</returns>
        public static async Task<string> HandleReturnContent(this Api.Redcap redcap,
            ReturnContent returnContent = ReturnContent.count)
        {
            try
            {
                string _returnContent;
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
                Log.Error(Ex.Message);
                return await Task.FromResult(string.Empty);
            }
        }

        /// <summary>
        /// Tuple that returns both inputFormat and redcap returnFormat
        /// </summary>
        /// <param name="redcap"></param>
        /// <param name="format">csv, json[default], xml , odm ('odm' refers to CDISC ODM XML format, specifically ODM version 1.3.1)</param>
        /// <param name="onErrorFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <returns>tuple, string, string, string</returns>
        public static async Task<(string format, string onErrorFormat, string redcapDataType)> HandleFormat(
            this Api.Redcap redcap, ReturnFormat? format = ReturnFormat.json,
            OnErrorFormat? onErrorFormat = OnErrorFormat.json, RedcapDataType? redcapDataType = RedcapDataType.flat)
        {
            // default
            var _format = ReturnFormat.json.ToString();
            var _onErrorFormat = OnErrorFormat.json.ToString();
            var _redcapDataType = RedcapDataType.flat.ToString();

            try
            {
                switch (format)
                {
                    case ReturnFormat.json:
                        _format = ReturnFormat.json.ToString();
                        break;
                    case ReturnFormat.csv:
                        _format = ReturnFormat.csv.ToString();
                        break;
                    case ReturnFormat.xml:
                        _format = ReturnFormat.xml.ToString();
                        break;
                    default:
                        _format = ReturnFormat.json.ToString();
                        break;
                }

                switch (onErrorFormat)
                {
                    case OnErrorFormat.json:
                        _onErrorFormat = OnErrorFormat.json.ToString();
                        break;
                    case OnErrorFormat.csv:
                        _onErrorFormat = OnErrorFormat.csv.ToString();
                        break;
                    case OnErrorFormat.xml:
                        _onErrorFormat = OnErrorFormat.xml.ToString();
                        break;
                    default:
                        _onErrorFormat = OnErrorFormat.json.ToString();
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
                Log.Error(Ex.Message);
                return await Task.FromResult((_format, _onErrorFormat, _redcapDataType));
            }
        }

        /// <summary>
        /// Method gets the overwrite behavior type and converts into string
        /// </summary>
        /// <param name="redcap"></param>
        /// <param name="overwriteBehavior"></param>
        /// <returns>string</returns>
        public static async Task<string> ExtractBehaviorAsync(this Api.Redcap redcap,
            OverwriteBehavior overwriteBehavior)
        {
            try
            {
                string _overwriteBehavior;
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
                Log.Error($"{Ex.Message}");
                return await Task.FromResult(string.Empty);
            }
        }

        /// <summary>
        /// This method extracts and converts an object's properties and associated values to redcap type and values.
        /// </summary>
        /// <param name="redcap"></param>
        /// <param name="input">Object</param>
        /// <returns>Dictionary of key value pair.</returns>
        public static async Task<Dictionary<string, string>> GetProperties(this Api.Redcap redcap, object input)
        {
            try
            {
                if (input != null)
                {
                    // Get the type
                    var type = input.GetType();
                    var obj = new Dictionary<string, string>();
                    // Get the properties
                    //var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    PropertyInfo[] properties = input.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
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
                                    propValue = dt.ToString(CultureInfo.CurrentCulture);
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

                return await Task.FromResult(new Dictionary<string, string>());
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return await Task.FromResult(new Dictionary<string, string>());
            }
        }

        /// <summary>
        /// Method extracts events into list from string
        /// </summary>
        /// <param name="redcap"></param>
        /// <param name="events"></param>
        /// <param name="delimiters">char[] e.g [';',',']</param>
        /// <returns>List of string</returns>
        public static async Task<List<string>> ExtractEventsAsync(this Api.Redcap redcap, string events,
            char[] delimiters)
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
                    Log.Error($"{Ex.Message}");
                    return await Task.FromResult(new List<string>());
                }
            }

            return await Task.FromResult(new List<string>());
        }

        /// <summary>
        /// Method gets / extracts fields into list from string
        /// </summary>
        /// <param name="redcap"></param>
        /// <param name="fields"></param>
        /// <param name="delimiters">char[] e.g [';',',']</param>
        /// <returns>List of string</returns>
        public static async Task<List<string>> ExtractFieldsAsync(this Api.Redcap redcap, string fields,
            char[] delimiters)
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
                    Log.Error($"{Ex.Message}");
                    return await Task.FromResult(new List<string>());
                }
            }

            return await Task.FromResult(new List<string>());
        }

        /// <summary>
        /// Method gets / extract records into list from string
        /// </summary>
        /// <param name="redcap"></param>
        /// <param name="records"></param>
        /// <param name="delimiters">char[] e.g [';',',']</param>
        /// <returns>List of string</returns>
        public static async Task<List<string>> ExtractRecordsAsync(this Api.Redcap redcap, string records,
            char[] delimiters)
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
                    Log.Error($"{Ex.Message}");
                    return await Task.FromResult(new List<string>());
                }
            }

            return await Task.FromResult(new List<string>());
        }

        /// <summary>
        /// Method gets / extracts forms into list from string
        /// </summary>
        /// <param name="redcap"></param>
        /// <param name="forms"></param>
        /// <param name="delimiters">char[] e.g [';',',']</param>
        /// <returns>A list of string</returns>
        public static async Task<List<string>> ExtractFormsAsync(this Api.Redcap redcap, string forms,
            char[] delimiters)
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
                    Log.Error($"{Ex.Message}");
                    return await Task.FromResult(new List<string>());
                }
            }

            return await Task.FromResult(new List<string>());
        }

        /// <summary>
        /// Returns a HttpClientHandler dependent on the value of the UseInsecureCertificate boolean
        /// </summary>
        /// <returns>HttpClientHandler</returns>
        public static HttpClientHandler GetHttpHandler()
        {
            return UseInsecureCertificate
                ? new HttpClientHandler
                {
                    UseProxy = false,
                    ServerCertificateCustomValidationCallback =
                        BrokenCertificate.DangerousAcceptAnyServerCertificateValidator
                }
                : new HttpClientHandler();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="redcap"></param>
        /// <param name="payload">data</param>
        /// <param name="uri">URI of the api instance</param>
        /// <returns>Stream</returns>
        public static async Task<Stream> GetStreamContentAsync(this Api.Redcap redcap,
            Dictionary<string, string> payload, Uri uri)
        {
            try
            {
                using (var handler = GetHttpHandler())
                using (var client = new HttpClient(handler))
                {
                    // Encode the values for payload
                    var content = new FormUrlEncodedContent(payload);
                    using (var response = await client.PostAsync(uri, content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            Stream stream = await response.Content.ReadAsStreamAsync();
                            return stream;
                        }
                    }
                }

                return null;
            }
            catch (Exception Ex)
            {
                Log.Error(Ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Method to send http request using MultipartFormDataContent
        /// Requests with attachments
        /// </summary>
        /// <param name="redcap"></param>
        /// <param name="payload">data</param>
        /// <param name="uri">URI of the api instance</param>
        /// <returns>string</returns>
        public static async Task<string> SendPostRequestAsync(this Api.Redcap redcap,
            MultipartFormDataContent payload, Uri uri)
        {
            try
            {
                using (var handler = GetHttpHandler())
                using (var client = new HttpClient(handler))
                {
                    using (var response = await client.PostAsync(uri, payload))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return await response.Content.ReadAsStringAsync();
                        }
                    }
                }

                return string.Empty;
            }
            catch (Exception Ex)
            {
                Log.Error(Ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Sends request using http
        /// </summary>
        /// <param name="redcap"></param>
        /// <param name="payload">data</param>
        /// <param name="uri">URI of the api instance</param>
        /// <returns></returns>
        public static async Task<string> SendPostRequestAsync(this Api.Redcap redcap,
            Dictionary<string, string> payload, Uri uri)
        {
            try
            {
                using var handler = GetHttpHandler();
                using var client = new HttpClient(handler);
                // extract the filepath
                var pathValue = payload.FirstOrDefault(x => x.Key == "filePath").Value;
                var pathkey = payload.FirstOrDefault(x => x.Key == "filePath").Key;

                if (!string.IsNullOrEmpty(pathkey))
                {
                    // the actual payload does not contain a 'filePath' key
                    payload.Remove(pathkey);
                }

                string _responseMessage;
                using (var content = new CustomFormUrlEncodedContent(payload))
                {
                    using (var response = await client.PostAsync(uri, content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            // Get the filename so we can save with the name
                            var headers = response.Content.Headers;
                            var fileName = headers.ContentType?.Parameters.Select(x => x.Value).FirstOrDefault();
                            //if (!string.IsNullOrEmpty(fileName))
                            //{
                            //    var contentDisposition = response.Content.Headers.ContentDisposition =
                            //        new ContentDispositionHeaderValue("attachment")
                            //        {
                            //            FileName = fileName
                            //        };
                            //}


                            if (!string.IsNullOrEmpty(pathValue))
                            {
                                var fileExtension = payload
                                    .SingleOrDefault(x => x.Key == "content" && x.Value == "pdf").Value;
                                if (!string.IsNullOrEmpty(fileExtension))
                                {
                                    // pdf 
                                    fileName = payload.SingleOrDefault(x => x.Key == "instrument").Value;
                                    // to do , make extensions for various types
                                    // save the file to a specified location using an extension method
                                    await response.Content.ReadAsFileAsync(fileName, pathValue, true,
                                        fileExtension);
                                }
                                else
                                {
                                    await response.Content.ReadAsFileAsync(fileName, pathValue, true,
                                        fileExtension);
                                }

                                _responseMessage = fileName;
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
                    }
                }

                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Sends http request to api
        /// </summary>
        /// <param name="redcap"></param>
        /// <param name="payload">data </param>
        /// <param name="uri">URI of the api instance</param>
        /// <returns>string</returns>
        public static async Task<string> SendPostRequest(this Api.Redcap redcap,
            Dictionary<string, string> payload, Uri uri)
        {
            string responseString;
            using (var handler = GetHttpHandler())
            using (var client = new HttpClient(handler))
            {
                // Encode the values for payload
                using (var content = new FormUrlEncodedContent(payload))
                {
                    using (var response = await client.PostAsync(uri, content))
                    {
                        // check the response and make sure its successful
                        response.EnsureSuccessStatusCode();
                        responseString = await response.Content.ReadAsStringAsync();
                    }
                }
            }

            return responseString;
        }
        
        /// <summary>
        /// Checks if the string passed is null or empty.
        /// </summary>
        /// <param name="redcap"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static void CheckToken(this Api.Redcap redcap, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }
        }
    }
}