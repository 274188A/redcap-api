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
        /// <param name="payload">Form-encoded payload to send.</param>
        /// <param name="uri">REDCap API endpoint URI.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <param name="timeOutSeconds">
        /// Per-call timeout in seconds. Non-positive values should fall back to the transport's default timeout behavior.
        /// </param>
        Task<Stream?> GetStreamContentAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

        /// <summary>
        /// Sends a multipart request and returns the response body.
        /// </summary>
        /// <param name="payload">Multipart form payload to send.</param>
        /// <param name="uri">REDCap API endpoint URI.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <param name="timeOutSeconds">
        /// Per-call timeout in seconds. Non-positive values should fall back to the transport's default timeout behavior.
        /// </param>
        Task<string> SendPostRequestAsync(MultipartFormDataContent payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

        /// <summary>
        /// Sends a form-urlencoded request and returns the response body.
        /// </summary>
        /// <param name="payload">Form-encoded payload to send.</param>
        /// <param name="uri">REDCap API endpoint URI.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <param name="timeOutSeconds">
        /// Per-call timeout in seconds. Non-positive values should fall back to the transport's default timeout behavior.
        /// </param>
        Task<string> SendPostRequestAsync(Dictionary<string, string> payload, Uri uri, CancellationToken cancellationToken = default, long timeOutSeconds = 100);

        /// <summary>
        /// Sends a form-urlencoded request and saves the response body to <paramref name="destinationPath"/> on disk.
        /// </summary>
        /// <param name="payload">Form-encoded payload to send.</param>
        /// <param name="uri">REDCap API endpoint URI.</param>
        /// <param name="destinationPath">Directory path where the downloaded file should be saved.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <param name="timeOutSeconds">
        /// Per-call timeout in seconds. Non-positive values should fall back to the transport's default timeout behavior.
        /// </param>
        Task<string> DownloadFileAsync(Dictionary<string, string> payload, Uri uri, string destinationPath, CancellationToken cancellationToken = default, long timeOutSeconds = 100);
    }
}
