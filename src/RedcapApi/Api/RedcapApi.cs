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
    /// <summary>
    /// This api interacts with redcap instances. https://project-redcap.org
    /// Go to your http://redcap_instance/api/help for Redcap Api documentations
    /// Author: John Barrett 274188A@curtin.edu.au
    /// </summary>
    public partial class RedcapApi : IRedcap
    {
        /// <summary>
        /// Redcap API Uri
        /// Location of your redcap instance
        /// </summary>
        /// <example>
        /// https://localhost/redcap/api
        /// </example>
        private Uri _uri = default!;

        private readonly IRedcapTransport _transport;

        /// <summary>
        /// The version of redcap that the api is currently interacting with.
        /// </summary>
        public string? Version = default!;
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="redcapApiUrl"></param>
        public RedcapApi(string redcapApiUrl)
            : this(redcapApiUrl, new DefaultRedcapTransport())
        {
        }

        /// <summary>
        /// Constructor that accepts a transport abstraction for testing and customization.
        /// </summary>
        /// <param name="redcapApiUrl">Redcap instance URI</param>
        /// <param name="transport">Transport abstraction used to execute requests.</param>
        public RedcapApi(string redcapApiUrl, IRedcapTransport transport)
        {
            _uri = new Uri(redcapApiUrl);
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }
        /// <summary>
        /// Validates the provided API token is not null or empty.
        /// </summary>
        /// <param name="token">The API token to validate.</param>
        protected virtual void CheckToken(string token)
        {
            Utils.CheckToken(this, token);
        }

        /// <summary>
        /// Sends a POST request with form-encoded payload to the specified URI.
        /// </summary>
        /// <param name="payload">Dictionary of form data to send.</param>
        /// <param name="uri">The URI to send the request to.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <param name="timeOutSeconds">Number of seconds before the HTTP request times out.</param>
        /// <returns>The response content as a string.</returns>
        protected virtual Task<string> SendPostRequestAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return _transport.SendPostRequestAsync(this, payload, uri, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// Sends a POST request with multipart form data payload to the specified URI.
        /// </summary>
        /// <param name="payload">Multipart form data content to send.</param>
        /// <param name="uri">The URI to send the request to.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <param name="timeOutSeconds">Number of seconds before the HTTP request times out.</param>
        /// <returns>The response content as a string.</returns>
        protected virtual Task<string> SendPostRequestAsync(MultipartFormDataContent payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return _transport.SendPostRequestAsync(this, payload, uri, cancellationToken, timeOutSeconds);
        }

        private async Task<string> ExecuteAsync(
            string token,
            Action<Dictionary<string, string>> buildPayload,
            CancellationToken cancellationToken = default,
            long timeOutSeconds = 100)
        {
            try
            {
                this.CheckToken(token);
                var payload = new Dictionary<string, string>();
                buildPayload(payload);
                return await this.SendPostRequestAsync(payload, _uri,
                    cancellationToken: cancellationToken, timeOutSeconds);
            }
            catch (RedcapApiException Ex)
            {
                Log.Error(Ex, "REDCap API call failed");
                throw;
            }
            catch (Exception Ex)
            {
                Log.Error(Ex, "REDCap API call failed");
                throw new RedcapApiException(Ex.Message, Ex);
            }
        }

        private async Task<string> ExecuteMultipartAsync(
            string token,
            Action<MultipartFormDataContent> buildPayload,
            CancellationToken cancellationToken = default,
            long timeOutSeconds = 100)
        {
            try
            {
                this.CheckToken(token);
                var payload = new MultipartFormDataContent();
                buildPayload(payload);
                return await this.SendPostRequestAsync(payload, _uri,
                    cancellationToken: cancellationToken, timeOutSeconds);
            }
            catch (RedcapApiException Ex)
            {
                Log.Error(Ex, "REDCap API call failed");
                throw;
            }
            catch (Exception Ex)
            {
                Log.Error(Ex, "REDCap API call failed");
                throw new RedcapApiException(Ex.Message, Ex);
            }
        }

        /// <summary>
        /// Gets stream content from the specified URI with form-encoded payload.
        /// </summary>
        /// <param name="payload">Dictionary of form data to send.</param>
        /// <param name="uri">The URI to send the request to.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <param name="timeOutSeconds">Number of seconds before the HTTP request times out.</param>
        /// <returns>The response content as a stream.</returns>
        protected virtual Task<Stream?> GetStreamContentAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
        {
            return _transport.GetStreamContentAsync(this, payload, uri, cancellationToken, timeOutSeconds);
        }

        /// <summary>
        /// Converts an array to a comma-separated string.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="inputArray">The array to convert.</param>
        /// <returns>A comma-separated string representation of the array elements.</returns>
        protected virtual string ConvertArraytoString<T>(T[] inputArray)
        {
            return Utils.ConvertArraytoString(this, inputArray);
        }

        /// <summary>
        /// Converts an integer array to a comma-separated string.
        /// </summary>
        /// <param name="inputArray">The integer array to convert.</param>
        /// <returns>A comma-separated string representation of the array elements.</returns>
        protected virtual string ConvertIntArraytoString(int[] inputArray)
        {
            return Utils.ConvertIntArraytoString(this, inputArray);
        }

        /// <summary>
        /// Processes and validates format parameters for API requests.
        /// </summary>
        /// <param name="format">The data format (csv, json, xml).</param>
        /// <param name="onErrorFormat">The format for error messages.</param>
        /// <param name="redcapDataType">The REDCap data type.</param>
        /// <returns>A tuple containing the processed format, error format, and data type strings.</returns>
        protected virtual (string format, string onErrorFormat, string redcapDataType) HandleFormat(RedcapFormat? format = RedcapFormat.json, RedcapReturnFormat? onErrorFormat = RedcapReturnFormat.json, RedcapDataType? redcapDataType = RedcapDataType.flat)
        {
            return Utils.HandleFormat(this, format, onErrorFormat, redcapDataType);
        }

        /// <summary>
        /// Processes the return content parameter for API requests.
        /// </summary>
        /// <param name="returnContent">The type of content to return in the response.</param>
        /// <returns>The processed return content string value.</returns>
        protected virtual string HandleReturnContent(ReturnContent returnContent = ReturnContent.count)
        {
            return Utils.HandleReturnContent(this, returnContent);
        }

        /// <summary>
        /// Extracts and processes the overwrite behavior parameter.
        /// </summary>
        /// <param name="overwriteBehavior">The behavior to use when overwriting existing data.</param>
        /// <returns>The processed overwrite behavior string value.</returns>
        protected virtual string ExtractBehavior(OverwriteBehavior overwriteBehavior)
        {
            return Utils.ExtractBehavior(this, overwriteBehavior);
        }
    }
}
