#nullable enable
using System.Net;

namespace Redcap.Exceptions
{
    /// <summary>
    /// Thrown when a REDCap API call fails. Wraps transport errors, non-success HTTP responses,
    /// and any other failure that previously surfaced as a plain string return value.
    /// </summary>
    public sealed class RedcapApiException : Exception
    {
        /// <summary>
        /// HTTP status code returned by REDCap, when the failure was a non-success HTTP response.
        /// </summary>
        public HttpStatusCode? StatusCode { get; }

        /// <summary>
        /// Raw response body returned by REDCap, when available. REDCap normally returns a JSON/CSV/XML
        /// error document here even on non-success responses.
        /// </summary>
        public string? ResponseBody { get; }

        /// <summary>Creates a new <see cref="RedcapApiException"/> with the given message.</summary>
        public RedcapApiException(string message)
            : base(message) { }

        /// <summary>Creates a new <see cref="RedcapApiException"/> that wraps an underlying exception.</summary>
        public RedcapApiException(string message, Exception? innerException)
            : base(message, innerException) { }

        /// <summary>
        /// Creates a new <see cref="RedcapApiException"/> tied to a specific HTTP status code and response body.
        /// </summary>
        public RedcapApiException(string message, HttpStatusCode statusCode, string? responseBody, Exception? innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
