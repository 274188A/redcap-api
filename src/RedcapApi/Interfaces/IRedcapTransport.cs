namespace Redcap.Interfaces
{
    /// <summary>
    /// Abstraction over REDCap HTTP transport so API methods can be tested without real network calls.
    /// </summary>
    public interface IRedcapTransport
    {
        /// <summary>
        /// Sends a form-urlencoded request and returns the response stream.
        /// </summary>
        Task<Stream?> GetStreamContentAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

        /// <summary>
        /// Sends a multipart request and returns the response body.
        /// </summary>
        Task<string> SendPostRequestAsync(MultipartFormDataContent payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

        /// <summary>
        /// Sends a form-urlencoded request and returns the response body.
        /// </summary>
        Task<string> SendPostRequestAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

        /// <summary>
        /// Sends a form-urlencoded request and saves the response body to <paramref name="destinationPath"/> on disk.
        /// </summary>
        Task<string> DownloadFileAsync(Dictionary<string, string> payload, Uri uri, string destinationPath, CancellationToken cancellationToken = default, long timeOutSeconds = 100);
    }
}