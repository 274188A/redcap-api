using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Redcap.Interfaces;
using Redcap.Models;
using Redcap.Utilities;

using Serilog;

using static System.String;

namespace Redcap
{
    /// <summary>
    /// This api interacts with redcap instances. https://project-redcap.org
    /// Go to your http://redcap_instance/api/help for Redcap Api documentations
    /// Author: Michael Tran tranpl@outlook.com, tranpl@vcu.edu
    /// </summary>
    public partial class RedcapApi : IRedcap
    {
       

        /// <summary>
        /// Constructor requires an api token and a valid url.
        /// </summary>
        /// <remarks>
        /// Token is required only for older APIs. As of version 1.0.0, token is required for each
        /// method used.
        /// </remarks>
        /// <param name="apiToken">Redcap Api Token can be obtained from redcap project or redcap administrators</param>
        /// <param name="redcapApiUrl">Redcap instance URI</param>
        /// 
        [Obsolete("Use constructor without token.")]
        public RedcapApi(string apiToken, string redcapApiUrl)
        {
            _token = apiToken?.ToString();
            _uri = new Uri(redcapApiUrl);
        }
      
               

        #region deprecated methods < version 1.0.0
        /// <summary>
        /// Export Arms
        /// </summary>
        /// <param name="inputFormat">test</param>
        /// <param name="returnFormat">test</param>
        /// 
        [Obsolete("Please use ExportArmsAsync with token param")]
        public async Task<string> ExportArmsAsync(ReturnFormat inputFormat, OnErrorFormat returnFormat)
        {
            try
            {
                string _responseMessage;
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat);

                var payload = new Dictionary<string, string>
                    {
                        { "token", _token },
                        { "content", "arm" },
                        { "format", _inputFormat },
                        { "returnFormat", _returnFormat },
                        { "arms", null}
                    };
                // Execute send request
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use DeleteArmsAsync with token param")]
        public async Task<string> DeleteArmsAsync<T>(T data)
        {
            try
            {
                string _responseMessage;
                var _serializedData = JsonConvert.SerializeObject(data);
                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", "arm" },
                    { "action", "delete" },
                    { "arms", _serializedData }
                };
                // Execute request
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFormat"></param>
        /// <param name="returnFormat"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ExportMetaDataAsync with token param")]
        public async Task<string> GetMetaDataAsync(ReturnFormat? inputFormat, OnErrorFormat? returnFormat)
        {
            try
            {
                string _responseMessage;
                // Handle optional parameters
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat);
                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", "metadata" },
                    { "format", _inputFormat },
                    { "returnFormat", _returnFormat }
                };
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFormat"></param>
        /// <param name="returnFormat"></param>
        /// <param name="delimiters"></param>
        /// <param name="fields"></param>
        /// <param name="forms"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ExportMetaDataAsync")]
        public async Task<string> GetMetaDataAsync(ReturnFormat? inputFormat, OnErrorFormat? returnFormat, char[] delimiters, string fields = "", string forms = "")
        {
            try
            {
                string _responseMessage;
                var _fields = "";
                var _forms = "";
                var _response = String.Empty;
                if (delimiters.Length == 0)
                {
                    // Provide some default delimiters, mostly comma and spaces for redcap
                    delimiters = new char[] { ',', ' ' };
                }

                var fieldsResult = await this.ExtractFieldsAsync(fields, delimiters);
                var formsResult = await this.ExtractFormsAsync(forms, delimiters);

                // Handle optional parameters
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat);

                if (!String.IsNullOrEmpty(fields))
                {
                    // Convert Array List into string array
                    string[] fieldsArray = fieldsResult.ToArray();
                    // Convert string array into String
                    _fields = await this.ConvertArraytoString(fieldsArray);
                }
                if (!String.IsNullOrEmpty(forms))
                {
                    string[] formsArray = formsResult.ToArray();
                    // Convert string array into String
                    _forms = await this.ConvertArraytoString(formsArray);
                }
                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", "metadata" },
                    { "format", _inputFormat },
                    { "returnFormat", _returnFormat }
                };
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <param name="inputFormat"></param>
        /// <param name="returnFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <param name="delimiters"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ExportRecordsAsync with token param")]
        public async Task<string> GetRecordAsync(string record, ReturnFormat inputFormat, OnErrorFormat returnFormat, RedcapDataType redcapDataType, char[] delimiters)
        {
            try
            {
                string _responseMessage;
                var _records = String.Empty;
                if (delimiters.Length == 0)
                {
                    // Provide some default delimiters, mostly comma and spaces for redcap
                    delimiters = new char[] { ',', ' ' };
                }
                var recordResults = await this.ExtractRecordsAsync(record, delimiters);
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat, redcapDataType);
                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", "record" },
                    { "format", _inputFormat },
                    { "returnFormat", _returnFormat },
                    { "type", _redcapDataType }
                };
                if (recordResults.Count == 0)
                {
                    Log.Error($"Missing required informaion.");
                    throw new InvalidOperationException($"Missing required informaion.");
                }
                else
                {
                    // Convert Array List into string array
                    var inputRecords = recordResults.ToArray();
                    // Convert string array into String
                    _records = await this.ConvertArraytoString(inputRecords);
                    payload.Add("records", _records);
                }
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <param name="inputFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <param name="returnFormat"></param>
        /// <param name="delimiters"></param>
        /// <param name="forms"></param>
        /// <param name="events"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ExportRecordsAsync with token param")]
        public async Task<string> GetRecordAsync(string record, ReturnFormat inputFormat, RedcapDataType redcapDataType, OnErrorFormat returnFormat = OnErrorFormat.json, char[] delimiters = null, string forms = null, string events = null, string fields = null)
        {
            try
            {
                var _records = String.Empty;
                if (delimiters == null)
                {
                    // Provide some default delimiters, mostly comma and spaces for redcap
                    delimiters = new char[] { ',', ' ' };
                }
                var recordItems = await this.ExtractRecordsAsync(records: record, delimiters: delimiters);
                var fieldItems = await this.ExtractFieldsAsync(fields: fields, delimiters: delimiters);
                var formItems = await this.ExtractFormsAsync(forms: forms, delimiters: delimiters);
                var eventItems = await this.ExtractEventsAsync(events: events, delimiters: delimiters);

                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat, redcapDataType);

                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", "record" },
                    { "format", _inputFormat },
                    { "returnFormat", _returnFormat },
                    { "type", _redcapDataType }
                };
                // Required
                if (recordItems.Count == 0)
                {
                    Log.Error($"Missing required informaion.");
                    throw new InvalidOperationException($"Missing required informaion.");
                }
                else
                {
                    // Convert Array List into string array
                    var _inputRecords = recordItems.ToArray();
                    payload.Add("records", await this.ConvertArraytoString(_inputRecords));
                }
                // Optional
                if (fieldItems.Count > 0)
                {
                    var _fields = fieldItems.ToArray();
                    payload.Add("fields", await this.ConvertArraytoString(_fields));
                }

                // Optional
                if (formItems.Count > 0)
                {
                    var _forms = formItems.ToArray();
                    payload.Add("forms", await this.ConvertArraytoString(_forms));
                }

                // Optional
                if (eventItems.Count > 0)
                {
                    var _events = eventItems.ToArray();
                    payload.Add("events", await this.ConvertArraytoString(_events));
                }

                return await this.SendPostRequestAsync(payload, _uri);
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFormat"></param>
        /// <param name="returnFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ExportRecordsAsync with token param")]
        public async Task<string> GetRecordsAsync(ReturnFormat inputFormat, OnErrorFormat returnFormat, RedcapDataType redcapDataType)
        {
            string _responseMessage;
            try
            {
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat, redcapDataType);
                var response = String.Empty;
                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", "record" },
                    { "format", _inputFormat },
                    { "returnFormat", _returnFormat },
                    { "type", _redcapDataType }
                };
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ExportRedcapVersionAsync with token param")]
        public async Task<string> GetRedcapVersionAsync(ReturnFormat inputFormat, RedcapDataType redcapDataType)
        {
            try
            {
                string _responseMessage;
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, OnErrorFormat.json, redcapDataType);
                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", "version" },
                    { "format", _inputFormat },
                    { "type", _redcapDataType }
                };
                // Execute send request
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="returnContent"></param>
        /// <param name="overwriteBehavior"></param>
        /// <param name="inputFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <param name="returnFormat"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ImportRecordsAsync with token param")]
        public async Task<string> SaveRecordsAsync(object data, ReturnContent returnContent, OverwriteBehavior overwriteBehavior, ReturnFormat? inputFormat, RedcapDataType? redcapDataType, OnErrorFormat? returnFormat)
        {
            try
            {
                string _responseMessage;
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat, redcapDataType);
                if (data != null)
                {
                    List<object> dataList = new List<object>
                    {
                        data
                    };
                    var _serializedData = JsonConvert.SerializeObject(dataList);
                    var _overWriteBehavior = await this.ExtractBehaviorAsync(overwriteBehavior);
                    var payload = new Dictionary<string, string>
                    {
                        { "token", _token },
                        { "content", Content.Record.GetDisplayName() },
                        { "format", _inputFormat },
                        { "type", _redcapDataType },
                        { "overwriteBehavior", _overWriteBehavior },
                        { "dateFormat", "MDY" },
                        { "returnFormat", _inputFormat },
                        { "returnContent", "count" },
                        { "data", _serializedData }
                    };

                    // Execute send request
                    _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                    return _responseMessage;
                }
                return null;
            }
            catch (Exception Ex)
            {
                Log.Error($"Could not save records into redcap.");
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="returnContent"></param>
        /// <param name="overwriteBehavior"></param>
        /// <param name="inputFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <param name="returnFormat"></param>
        /// <param name="dateFormat"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ImportRecordsAsync with token param")]
        public async Task<string> SaveRecordsAsync(object data, ReturnContent returnContent, OverwriteBehavior overwriteBehavior, ReturnFormat? inputFormat, RedcapDataType? redcapDataType, OnErrorFormat? returnFormat, string dateFormat = "MDY")
        {
            try
            {
                string _responseMessage;
                string _dateFormat = dateFormat;
                // Handle optional parameters
                if (String.IsNullOrEmpty(_dateFormat))
                {
                    _dateFormat = "MDY";
                }
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat, redcapDataType);
                var _returnContent = await this.HandleReturnContent(returnContent);
                var _overWriteBehavior = await this.ExtractBehaviorAsync(overwriteBehavior);

                // Extract properties from object provided
                if (data != null)
                {
                    List<object> list = new List<object>
                    {
                        data
                    };
                    var formattedData = JsonConvert.SerializeObject(list);
                    var payload = new Dictionary<string, string>
                    {
                        { "token", _token },
                        { "content", Content.Record.GetDisplayName() },
                        { "format", _inputFormat },
                        { "type", _redcapDataType },
                        { "overwriteBehavior", _overWriteBehavior },
                        { "dateFormat", _dateFormat },
                        { "returnFormat", _inputFormat },
                        { "returnContent", _returnContent },
                        { "data", formattedData }
                    };

                    // Execute send request
                    _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                    return _responseMessage;
                }
                return string.Empty;

            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return await Task.FromResult(string.Empty);
            }
        }
        /// <summary>
        /// Saves record to redcap
        /// </summary>
        /// <param name="data"></param>
        /// <param name="returnContent"></param>
        /// <param name="overwriteBehavior"></param>
        /// <param name="inputFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <param name="returnFormat"></param>
        /// <param name="dateFormat"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ImportRecordsAsync with token param")]
        public async Task<string> SaveRecordsAsync(List<string> data, ReturnContent returnContent, OverwriteBehavior overwriteBehavior, ReturnFormat? inputFormat, RedcapDataType? redcapDataType, OnErrorFormat? returnFormat, string dateFormat = "MDY")
        {
            try
            {
                string _responseMessage;
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat, redcapDataType);
                var _returnContent = await this.HandleReturnContent(returnContent);
                var _overWriteBehavior = await this.ExtractBehaviorAsync(overwriteBehavior);

                var _response = String.Empty;
                var _dateFormat = dateFormat;
                // Handle optional parameters
                if (string.IsNullOrEmpty((string)_dateFormat))
                {
                    _dateFormat = "MDY";
                }
                // Extract properties from object provided
                if (data != null)
                {
                    var _serializedData = JsonConvert.SerializeObject(data);
                    var payload = new Dictionary<string, string>
                    {
                        { "token", _token },
                        { "content",  Content.Record.GetDisplayName() },
                        { "format", _inputFormat },
                        { "type", _redcapDataType },
                        { "overwriteBehavior", _overWriteBehavior },
                        { "dateFormat", _dateFormat },
                        { "returnFormat", _returnFormat },
                        { "returnContent", _returnContent },
                        { "data", _serializedData }
                    };

                    // Execute send request
                    _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                    return _responseMessage;
                }
                return string.Empty;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return await Task.FromResult(string.Empty);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="returnContent"></param>
        /// <param name="overwriteBehavior"></param>
        /// <param name="inputFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <param name="returnFormat"></param>
        /// <param name="dateFormat"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ImportRecordsAsync with token param")]
        public async Task<string> ImportRecordsAsync(object data, ReturnContent returnContent, OverwriteBehavior overwriteBehavior, ReturnFormat? inputFormat, RedcapDataType? redcapDataType, OnErrorFormat? returnFormat, string dateFormat = "MDY")
        {
            try
            {
                string _responseMessage;
                string _dateFormat = dateFormat;
                // Handle optional parameters
                if (String.IsNullOrEmpty(_dateFormat))
                {
                    _dateFormat = "MDY";
                }
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat, redcapDataType);
                var _returnContent = await this.HandleReturnContent(returnContent);
                var _overWriteBehavior = await this.ExtractBehaviorAsync(overwriteBehavior);

                // Extract properties from object provided
                if (data != null)
                {
                    List<object> list = new List<object>
                    {
                        data
                    };
                    var formattedData = JsonConvert.SerializeObject(list);
                    var payload = new Dictionary<string, string>
                    {
                        { "token", _token },
                        { "content",  Content.Record.GetDisplayName() },
                        { "format", _inputFormat },
                        { "type", _redcapDataType },
                        { "overwriteBehavior", _overWriteBehavior },
                        { "dateFormat", _dateFormat },
                        { "returnFormat", _inputFormat },
                        { "returnContent", _returnContent },
                        { "data", formattedData }
                    };

                    // Execute send request
                    _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                    return _responseMessage;
                }
                return string.Empty;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return await Task.FromResult(string.Empty);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="returnContent"></param>
        /// <param name="overwriteBehavior"></param>
        /// <param name="inputFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <param name="returnFormat"></param>
        /// <param name="apiToken"></param>
        /// <param name="dateFormat"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ImportRecordsAsync with token param")]
        public async Task<string> ImportRecordsAsync(object data, ReturnContent returnContent, OverwriteBehavior overwriteBehavior, ReturnFormat? inputFormat, RedcapDataType? redcapDataType, OnErrorFormat? returnFormat, string apiToken, string dateFormat = "MDY")
        {
            try
            {
                string _apiToken = apiToken;
                string _responseMessage;
                string _dateFormat = dateFormat;
                // Handle optional parameters
                if (String.IsNullOrEmpty(_dateFormat))
                {
                    _dateFormat = "MDY";
                }
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat, redcapDataType);
                var _returnContent = await this.HandleReturnContent(returnContent);
                var _overWriteBehavior = await this.ExtractBehaviorAsync(overwriteBehavior);

                // Extract properties from object provided
                if (data != null)
                {
                    //List<object> list = new List<object>
                    //{
                    //    data
                    //};
                    var formattedData = JsonConvert.SerializeObject(data);
                    var payload = new Dictionary<string, string>
                    {
                        { "token", _apiToken },
                        { "content",  Content.Record.GetDisplayName()},
                        { "format", _inputFormat },
                        { "type", _redcapDataType },
                        { "overwriteBehavior", _overWriteBehavior },
                        { "dateFormat", _dateFormat },
                        { "returnFormat", _inputFormat },
                        { "returnContent", _returnContent },
                        { "data", formattedData }
                    };

                    // Execute send request
                    _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                    return _responseMessage;
                }
                return string.Empty;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return await Task.FromResult(string.Empty);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFormat"></param>
        /// <param name="returnFormat"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ExportMetaDataAsync with token param")]

        public async Task<string> ExportMetaDataAsync(ReturnFormat? inputFormat, OnErrorFormat? returnFormat)
        {
            try
            {
                string _responseMessage;
                // Handle optional parameters
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat);
                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content",  Content.MetaData.GetDisplayName() },
                    { "format", _inputFormat },
                    { "returnFormat", _returnFormat }
                };
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFormat"></param>
        /// <param name="returnFormat"></param>
        /// <param name="delimiters"></param>
        /// <param name="fields"></param>
        /// <param name="forms"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ExportMetaDataAsync with token param")]
        public async Task<string> ExportMetaDataAsync(ReturnFormat? inputFormat, OnErrorFormat? returnFormat, char[] delimiters, string fields = "", string forms = "")
        {
            try
            {
                string _responseMessage;
                var _fields = "";
                var _forms = "";
                if (delimiters.Length == 0)
                {
                    // Provide some default delimiters, mostly comma and spaces for redcap
                    delimiters = new char[] { ',', ' ' };
                }

                var fieldsResult = await this.ExtractFieldsAsync(fields, delimiters);
                var formsResult = await this.ExtractFormsAsync(forms, delimiters);

                // Handle optional parameters
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat);

                if (!String.IsNullOrEmpty(fields))
                {
                    // Convert Array List into string array
                    string[] fieldsArray = fieldsResult.ToArray();
                    // Convert string array into String
                    _fields = await this.ConvertArraytoString(fieldsArray);
                }
                if (!String.IsNullOrEmpty(forms))
                {
                    string[] formsArray = formsResult.ToArray();
                    // Convert string array into String
                    _forms = await this.ConvertArraytoString(formsArray);
                }
                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", Content.MetaData.GetDisplayName()  },
                    { "format", _inputFormat },
                    { "returnFormat", _returnFormat }
                };
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFormat"></param>
        /// <param name="returnFormat"></param>
        /// <param name="arms"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ExportEventsAsync with token param")]
        public async Task<string> ExportEventsAsync(ReturnFormat inputFormat, OnErrorFormat returnFormat = OnErrorFormat.json, int[] arms = null)
        {
            try
            {
                string _responseMessage;
                var _arms = "";
                // Handle optional parameters
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat);
                if (arms.Length > 0)
                {
                    // Convert string array into String
                    _arms = await this.ConvertIntArraytoString(arms);
                }
                var payload = new Dictionary<string, string>
                {
                    {"arms", _arms },
                    { "token", _token },
                    { "content", Content.Event.GetDisplayName()  },
                    { "format", _inputFormat },
                    { "returnFormat", _returnFormat }
                };
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// Alias /test/compatibility
        /// </summary>
        /// <param name="inputFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <returns>string</returns>
        public delegate Task<string> GetRedcapVersion(ReturnFormat inputFormat, RedcapDataType redcapDataType);
        /// <summary>
        /// Alias /test/compatibility
        /// </summary>
        /// <param name="record"></param>
        /// <param name="inputFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <param name="returnFormat"></param>
        /// <param name="delimiters"></param>
        /// <param name="forms"></param>
        /// <param name="events"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public delegate Task<string> ExportRecord(string record, ReturnFormat inputFormat, RedcapDataType redcapDataType, OnErrorFormat returnFormat = OnErrorFormat.json, char[] delimiters = null, string forms = null, string events = null, string fields = null);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <param name="inputFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <param name="returnFormat"></param>
        /// <param name="delimiters"></param>
        /// <param name="forms"></param>
        /// <param name="events"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public delegate Task<string> ExportRecords(string record, ReturnFormat inputFormat, RedcapDataType redcapDataType, OnErrorFormat returnFormat = OnErrorFormat.json, char[] delimiters = null, string forms = null, string events = null, string fields = null);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="overRide"></param>
        /// <param name="inputFormat"></param>
        /// <param name="returnFormat"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ImportArmsAsync with token param")]
        public async Task<string> ImportArmsAsync<T>(List<T> data, Override overRide, ReturnFormat inputFormat, OnErrorFormat returnFormat)
        {
            try
            {
                string _responseMessage;
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat);
                var _override = overRide.ToString();
                var _serializedData = JsonConvert.SerializeObject(data);
                var payload = new Dictionary<string, string>
                    {
                        { "token", _token },
                        { "content", Content.Arm.GetDisplayName() },
                        { "action", "import" },
                        { "format", _inputFormat },
                        { "type", _redcapDataType },
                        { "override", _override },
                        { "returnFormat", _returnFormat },
                        { "data", _serializedData }
                    };
                // Execute request
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <param name="inputFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <param name="returnFormat"></param>
        /// <param name="delimiters"></param>
        /// <param name="forms"></param>
        /// <param name="events"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        [Obsolete("ExportRecordAsync is deprecated, please use ExportRecordsAsync version 1.0+", true)]
        public async Task<string> ExportRecordAsync(string record, ReturnFormat inputFormat, RedcapDataType redcapDataType, OnErrorFormat returnFormat = OnErrorFormat.json, char[] delimiters = null, string forms = null, string events = null, string fields = null)
        {
            try
            {
                if (delimiters == null)
                {
                    // Provide some default delimiters, mostly comma and spaces for redcap
                    delimiters = new char[] { ',', ' ' };
                }
                var recordItems = await this.ExtractRecordsAsync(records: record, delimiters: delimiters);
                var fieldItems = await this.ExtractFieldsAsync(fields: fields, delimiters: delimiters);
                var formItems = await this.ExtractFormsAsync(forms: forms, delimiters: delimiters);
                var eventItems = await this.ExtractEventsAsync(events: events, delimiters: delimiters);

                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat, redcapDataType);

                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", Content.Record.GetDisplayName()  },
                    { "format", _inputFormat },
                    { "returnFormat", _returnFormat },
                    { "type", _redcapDataType }
                };
                // Required
                if (recordItems.Count == 0)
                {
                    Log.Error($"Missing required informaion.");
                    throw new InvalidOperationException($"Missing required informaion.");
                }
                else
                {
                    // Convert Array List into string array
                    var _inputRecords = recordItems.ToArray();
                    payload.Add("records", await this.ConvertArraytoString(_inputRecords));
                }
                // Optional
                if (fieldItems.Count > 0)
                {
                    var _fields = fieldItems.ToArray();
                    payload.Add("fields", await this.ConvertArraytoString(_fields));
                }

                // Optional
                if (formItems.Count > 0)
                {
                    var _forms = formItems.ToArray();
                    payload.Add("forms", await this.ConvertArraytoString(_forms));
                }

                // Optional
                if (eventItems.Count > 0)
                {
                    var _events = eventItems.ToArray();
                    payload.Add("events", await this.ConvertArraytoString(_events));
                }

                return await this.SendPostRequestAsync(payload, _uri);
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="records"></param>
        /// <param name="inputFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <param name="returnFormat"></param>
        /// <param name="delimiters"></param>
        /// <param name="forms"></param>
        /// <param name="events"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        /// 
        [Obsolete("ExportRecordAsync is deprecated, please use ExportRecordsAsync version 1.0+", true)]
        public async Task<string> ExportRecordsAsync(string records, ReturnFormat inputFormat, RedcapDataType redcapDataType, OnErrorFormat returnFormat = OnErrorFormat.json, char[] delimiters = null, string forms = null, string events = null, string fields = null)
        {
            try
            {
                string _responseMessage;
                var _records = String.Empty;
                if (delimiters == null)
                {
                    // Provide some default delimiters, mostly comma and spaces for redcap
                    delimiters = new char[] { ',', ' ' };
                }
                var recordItems = await this.ExtractRecordsAsync(records: records, delimiters: delimiters);
                var fieldItems = await this.ExtractFieldsAsync(fields: fields, delimiters: delimiters);
                var formItems = await this.ExtractFormsAsync(forms: forms, delimiters: delimiters);
                var eventItems = await this.ExtractEventsAsync(events: events, delimiters: delimiters);

                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat, redcapDataType);

                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", Content.Record.GetDisplayName()  },
                    { "format", _inputFormat },
                    { "returnFormat", _returnFormat },
                    { "type", _redcapDataType }
                };
                // Required
                if (recordItems.Count == 0)
                {
                    Log.Error($"Missing required informaion.");
                    throw new InvalidOperationException($"Missing required informaion.");
                }
                else
                {
                    // Convert Array List into string array
                    var _inputRecords = recordItems.ToArray();
                    payload.Add("records", await this.ConvertArraytoString(_inputRecords));
                }
                // Optional
                if (fieldItems.Count > 0)
                {
                    var _fields = fieldItems.ToArray();
                    payload.Add("fields", await this.ConvertArraytoString(_fields));
                }

                // Optional
                if (formItems.Count > 0)
                {
                    var _forms = formItems.ToArray();
                    payload.Add("forms", await this.ConvertArraytoString(_forms));
                }

                // Optional
                if (eventItems.Count > 0)
                {
                    var _events = eventItems.ToArray();
                    payload.Add("events", await this.ConvertArraytoString(_events));
                }

                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <param name="returnFormat"></param>
        /// <param name="delimiters"></param>
        /// <param name="forms"></param>
        /// <param name="events"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        /// 
        [Obsolete("ExportRecordAsync is deprecated, please use ExportRecordsAsync version 1.0+", true)]
        public async Task<string> ExportRecordsAsync(ReturnFormat inputFormat, RedcapDataType redcapDataType, OnErrorFormat returnFormat = OnErrorFormat.json, char[] delimiters = null, string forms = null, string events = null, string fields = null)
        {
            try
            {
                string _responseMessage;
                var _records = String.Empty;
                if (delimiters == null)
                {
                    // Provide some default delimiters, mostly comma and spaces for redcap
                    delimiters = new char[] { ',', ' ' };
                }
                var fieldItems = await this.ExtractFieldsAsync(fields: fields, delimiters: delimiters);
                var formItems = await this.ExtractFormsAsync(forms: forms, delimiters: delimiters);
                var eventItems = await this.ExtractEventsAsync(events: events, delimiters: delimiters);

                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat, redcapDataType);

                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", Content.Record.GetDisplayName()  },
                    { "format", _inputFormat },
                    { "returnFormat", _returnFormat },
                    { "type", _redcapDataType }
                };
                // Optional
                if (fieldItems.Count > 0)
                {
                    var _fields = fieldItems.ToArray();
                    payload.Add("fields", await this.ConvertArraytoString(_fields));
                }

                // Optional
                if (formItems.Count > 0)
                {
                    var _forms = formItems.ToArray();
                    payload.Add("forms", await this.ConvertArraytoString(_forms));
                }

                // Optional
                if (eventItems.Count > 0)
                {
                    var _events = eventItems.ToArray();
                    payload.Add("events", await this.ConvertArraytoString(_events));
                }

                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ExportRedcapVersionAsync with token param ")]
        public async Task<string> ExportRedcapVersionAsync(ReturnFormat inputFormat, RedcapDataType redcapDataType)
        {
            try
            {
                string _responseMessage;
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, OnErrorFormat.json, redcapDataType);
                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", Content.Version.GetDisplayName() },
                    { "format", _inputFormat },
                    { "type", _redcapDataType }
                };
                // Execute send request
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);

                return await Task.FromResult(_responseMessage);
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFormat"></param>
        /// <param name="returnFormat"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ExportUsersAsync with token param ")]
        public async Task<string> ExportUsersAsync(ReturnFormat inputFormat, OnErrorFormat returnFormat = OnErrorFormat.json)
        {
            try
            {
                string _responseMessage;
                var _inputFormat = inputFormat.ToString();
                var _returnFormat = returnFormat.ToString();
                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", Content.User.GetDisplayName()  },
                    { "format", _inputFormat },
                    { "returnFormat", _returnFormat }
                };

                // Execute send request
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFormat"></param>
        /// <param name="returnFormat"></param>
        /// <param name="redcapDataType"></param>
        /// <param name="delimiters"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ExportRecordsAsync with token param ")]
        public async Task<string> GetRecordsAsync(ReturnFormat inputFormat, OnErrorFormat returnFormat, RedcapDataType redcapDataType, char[] delimiters)
        {
            try
            {
                string _responseMessage;
                var _records = String.Empty;
                if (delimiters == null)
                {
                    // Provide some default delimiters, mostly comma and spaces for redcap
                    delimiters = new char[] { ',', ' ' };
                }
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat);
                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", Content.Record.GetDisplayName() },
                    { "format", _inputFormat },
                    { "returnFormat", _returnFormat },
                    { "type", _redcapDataType }
                };
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="overRide"></param>
        /// <param name="inputFormat"></param>
        /// <param name="returnFormat"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ImportEventsAsync with token param ")]
        public async Task<string> ImportEventsAsync<T>(List<T> data, Override overRide, ReturnFormat inputFormat, OnErrorFormat returnFormat = OnErrorFormat.json)
        {
            try
            {
                string _responseMessage;
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat);
                var _override = overRide.ToString();
                var _serializedData = JsonConvert.SerializeObject(data);
                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", Content.Event.GetDisplayName() },
                    { "action", "import" },
                    { "format", _inputFormat },
                    { "type", _redcapDataType },
                    { "override", _override },
                    { "returnFormat", _returnFormat },
                    { "data", _serializedData }
                };
                // Execute request
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <param name="field"></param>
        /// <param name="eventName"></param>
        /// <param name="repeatInstance"></param>
        /// <param name="returnFormat"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ExportFileAsync with token param ")]
        public async Task<string> ExportFileAsync(string record, string field, string eventName, string repeatInstance, OnErrorFormat returnFormat = OnErrorFormat.json)
        {
            try
            {
                string _responseMessage;
                var _returnFormat = returnFormat.ToString();
                var _eventName = eventName;
                var _repeatInstance = repeatInstance;
                var _record = record;
                var _field = field;
                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", Content.File.GetDisplayName() },
                    { "action", "export" },
                    { "record", _record },
                    { "field", _field },
                    { "event", _eventName },
                    { "returnFormat", _returnFormat }
                };
                if (!string.IsNullOrEmpty(_repeatInstance))
                {
                    payload.Add("repeat_instance", _repeatInstance);
                }
                // Execute request
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error(Ex.Message);
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <param name="field"></param>
        /// <param name="eventName"></param>
        /// <param name="repeatInstance"></param>
        /// <param name="filePath"></param>
        /// <param name="returnFormat"></param>
        /// <returns></returns>
        /// 
        [Obsolete("ExportFileAsync is deprecated, please use ExportFileAsync version 1.0+", true)]
        public async Task<string> ExportFileAsync(string record, string field, string eventName, string repeatInstance, string filePath = null, OnErrorFormat returnFormat = OnErrorFormat.json)
        {
            try
            {
                string _responseMessage;
                var _filePath = filePath;
                if (!string.IsNullOrEmpty(filePath))
                {
                    if (!Directory.Exists(_filePath))
                    {
                        Log.Information($"The directory does not exist!");
                        Directory.CreateDirectory(_filePath);
                    }
                }
                var _returnFormat = returnFormat.ToString();
                var _eventName = eventName;
                var _repeatInstance = repeatInstance;
                var _record = record;
                var _field = field;
                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", Content.File.GetDisplayName() },
                    { "action", "export" },
                    { "record", _record },
                    { "field", _field },
                    { "event", _eventName },
                    { "returnFormat", _returnFormat },
                    { "filePath", $@"{_filePath}" }
                };
                if (!string.IsNullOrEmpty(_repeatInstance))
                {
                    payload.Add("repeat_instance", _repeatInstance);
                }
                // Execute request
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error(Ex.Message);
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <param name="field"></param>
        /// <param name="eventName"></param>
        /// <param name="repeatInstance"></param>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        /// <param name="returnFormat"></param>
        /// <returns></returns>
        /// 
        [Obsolete("ImportFileAsync is deprecated, please use ImportFileAsync version 1.0+", true)]
        public async Task<string> ImportFileAsync(string record, string field, string eventName, string repeatInstance, string fileName, string filePath, OnErrorFormat returnFormat = OnErrorFormat.json)
        {

            try
            {
                string _responseMessage;
                var _fileName = fileName;
                var _filePath = filePath;
                var _binaryFile = Path.Combine(_filePath, _fileName);
                ByteArrayContent _fileContent;
                var _returnFormat = returnFormat.ToString();
                var _eventName = eventName;
                var _repeatInstance = repeatInstance;
                var _record = record;
                var _field = field;
                var payload = new MultipartFormDataContent()
                {
                        {new StringContent(_token), "token" },
                        {new StringContent("file") ,"content" },
                        {new StringContent("import"), "action" },
                        {new StringContent(_record), "record" },
                        {new StringContent(_field), "field" },
                        {new StringContent(_eventName),  "event" },
                        {new StringContent(_returnFormat), "returnFormat" }
                };
                if (!string.IsNullOrEmpty(_repeatInstance))
                {
                    // add repeat instrument params if available
                    payload.Add(new StringContent(_repeatInstance), "repeat_instance");
                }
                if (string.IsNullOrEmpty(_fileName) || string.IsNullOrEmpty(_filePath))
                {

                    throw new InvalidOperationException($"file can not be empty or null");
                }
                else
                {
                    // add the binary file in specific content type
                    _fileContent = new ByteArrayContent(File.ReadAllBytes(_binaryFile));
                    _fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    payload.Add(_fileContent, "file", _fileName);
                }
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error(Ex.Message);
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <param name="field"></param>
        /// <param name="eventName"></param>
        /// <param name="repeatInstance"></param>
        /// <param name="returnFormat"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use DeleteFileAsync with token param ")]
        public async Task<string> DeleteFileAsync(string record, string field, string eventName, string repeatInstance, OnErrorFormat returnFormat = OnErrorFormat.json)
        {
            try
            {
                string _responseMessage;
                var _returnFormat = returnFormat.ToString();
                var _eventName = eventName;
                var _repeatInstance = repeatInstance;
                var _record = record;
                var _field = field;
                var payload = new MultipartFormDataContent()
                {
                        {new StringContent(_token), "token" },
                        {new StringContent("file") ,"content" },
                        {new StringContent("delete"), "action" },
                        {new StringContent(_record), "record" },
                        {new StringContent(_field), "field" },
                        {new StringContent(_eventName),  "event" },
                        {new StringContent(_returnFormat), "returnFormat" }
                };
                if (!string.IsNullOrEmpty(_repeatInstance))
                {
                    // add repeat instrument params if available
                    payload.Add(new StringContent(_repeatInstance), "repeat_instance");
                }
                _responseMessage = await this.SendPostRequestAsync(payload, _uri);
                return _responseMessage;
            }
            catch (Exception Ex)
            {
                Log.Error(Ex.Message);
                return string.Empty;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFormat"></param>
        /// <param name="returnFormat"></param>
        /// <returns></returns>
        /// 
        [Obsolete("Please use ExportEventsAsync with token param ")]
        public async Task<string> ExportEventsAsync(ReturnFormat inputFormat, OnErrorFormat returnFormat = OnErrorFormat.json)
        {
            try
            {
                // Handle optional parameters
                var (_inputFormat, _returnFormat, _redcapDataType) = await this.HandleFormat(inputFormat, returnFormat);
                var payload = new Dictionary<string, string>
                {
                    { "token", _token },
                    { "content", Content.Event.GetDisplayName() },
                    { "format", _inputFormat },
                    { "returnFormat", _returnFormat }
                };
                return await this.SendPostRequestAsync(payload, _uri);
            }
            catch (Exception Ex)
            {
                Log.Error($"{Ex.Message}");
                return string.Empty;
            }
        }
        #endregion deprecated
    }
}
