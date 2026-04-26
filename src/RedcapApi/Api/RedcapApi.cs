using System.Text.Json;

using Redcap.Api;
using Redcap.Exceptions;
using Redcap.Interfaces;
using Redcap.Models;
using Redcap.Utilities;

using Serilog;

using static System.String;

namespace Redcap;

/// <summary>
/// Client for interacting with a REDCap API endpoint.
/// </summary>
/// <remarks>
/// Construct the client with the API endpoint URL and a project token, then reuse the instance for multiple calls.
/// Dispose the client when you are done if it owns its transport.
/// </remarks>
public partial class RedcapApi : IRedcap, IDisposable
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
    private readonly bool _ownsTransport;

    private readonly string _token;
    private bool _disposed;

    /// <summary>
    /// Gets the last REDCap version value returned by <see cref="ExportRedcapVersionAsync(Redcap.Models.RedcapFormat, CancellationToken, long)"/>.
    /// </summary>
    public string? Version { get; private set; }

    /// <summary>
    /// Creates a new <see cref="RedcapApi"/> with the given URL and project token.
    /// </summary>
    /// <param name="redcapApiUrl">REDCap API endpoint URI.</param>
    /// <param name="token">API token for the REDCap project.</param>
    /// <remarks>
    /// This overload creates and owns the default transport. Dispose the client when you are done with it.
    /// </remarks>
    public RedcapApi(string redcapApiUrl, string token)
        : this(redcapApiUrl, token, new DefaultRedcapTransport(), ownsTransport: true)
    {
    }

    /// <summary>
    /// Creates a new <see cref="RedcapApi"/> with a caller-supplied transport.
    /// </summary>
    /// <param name="redcapApiUrl">REDCap API endpoint URI.</param>
    /// <param name="token">API token for the REDCap project.</param>
    /// <param name="transport">Transport abstraction used to execute requests.</param>
    /// <remarks>
    /// The caller retains ownership of <paramref name="transport"/>. Disposing this client will not dispose the injected transport.
    /// </remarks>
    public RedcapApi(string redcapApiUrl, string token, IRedcapTransport transport)
        : this(redcapApiUrl, token, transport, ownsTransport: false)
    {
    }

    private RedcapApi(string redcapApiUrl, string token, IRedcapTransport transport, bool ownsTransport)
    {
        _uri = new Uri(redcapApiUrl);
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _ownsTransport = ownsTransport;
        Utils.CheckToken(this, token);
        _token = token;
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
    /// Disposes the client and, when owned by this instance, its underlying transport.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
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
        ThrowIfDisposed();
        return _transport.SendPostRequestAsync(payload, uri, cancellationToken, timeOutSeconds);
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
        ThrowIfDisposed();
        return _transport.SendPostRequestAsync(payload, uri, cancellationToken, timeOutSeconds);
    }

    private async Task<string> ExecuteAsync(
        Action<Dictionary<string, string>> buildPayload,
        CancellationToken cancellationToken = default,
        long timeOutSeconds = 100)
    {
        ThrowIfDisposed();

        try
        {
            var payload = CreatePayload();
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
        Action<MultipartFormDataContent> buildPayload,
        CancellationToken cancellationToken = default,
        long timeOutSeconds = 100)
    {
        ThrowIfDisposed();

        try
        {
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
        ThrowIfDisposed();
        return _transport.GetStreamContentAsync(payload, uri, cancellationToken, timeOutSeconds);
    }

    /// <summary>
    /// Posts payload to the transport's download path and saves the response to <paramref name="destinationPath"/> on disk.
    /// </summary>
    protected virtual Task<string> DownloadFileAsync(Dictionary<string, string> payload, Uri uri, string destinationPath, CancellationToken cancellationToken = default, long timeOutSeconds = 100)
    {
        ThrowIfDisposed();
        return _transport.DownloadFileAsync(payload, uri, destinationPath, cancellationToken, timeOutSeconds);
    }

    private async Task<string> ExecuteDownloadAsync(
        string destinationPath,
        Action<Dictionary<string, string>> buildPayload,
        CancellationToken cancellationToken = default,
        long timeOutSeconds = 100)
    {
        ThrowIfDisposed();

        try
        {
            var payload = CreatePayload();
            buildPayload(payload);
            return await this.DownloadFileAsync(payload, _uri, destinationPath,
                cancellationToken: cancellationToken, timeOutSeconds: timeOutSeconds);
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

    private Dictionary<string, string> CreatePayload()
    {
        return new Dictionary<string, string>
        {
            ["token"] = _token
        };
    }

    private static void AddContent(Dictionary<string, string> payload, Content content)
    {
        payload["content"] = content.GetDisplayName();
    }

    private static void AddAction(Dictionary<string, string> payload, RedcapAction action)
    {
        payload["action"] = action.GetDisplayName();
    }

    private static void AddFormat(Dictionary<string, string> payload, RedcapFormat format)
    {
        payload["format"] = format.GetDisplayName();
    }

    private static void AddReturnFormat(Dictionary<string, string> payload, RedcapReturnFormat returnFormat)
    {
        payload["returnFormat"] = returnFormat.GetDisplayName();
    }

    private static void AddFormattedRequest(
        Dictionary<string, string> payload,
        Content content,
        RedcapFormat format,
        RedcapReturnFormat? returnFormat = null)
    {
        AddContent(payload, content);
        AddFormat(payload, format);

        if (returnFormat.HasValue)
        {
            AddReturnFormat(payload, returnFormat.Value);
        }
    }

    private static void AddActionRequest(
        Dictionary<string, string> payload,
        Content content,
        RedcapAction action,
        RedcapReturnFormat? returnFormat = null)
    {
        AddContent(payload, content);
        AddAction(payload, action);

        if (returnFormat.HasValue)
        {
            AddReturnFormat(payload, returnFormat.Value);
        }
    }

    private static void AddImportRequest<T>(
        Dictionary<string, string> payload,
        Content content,
        RedcapFormat format,
        T data,
        RedcapReturnFormat? returnFormat = null,
        RedcapAction action = RedcapAction.Import)
    {
        AddContent(payload, content);
        AddAction(payload, action);
        AddFormat(payload, format);

        if (returnFormat.HasValue)
        {
            AddReturnFormat(payload, returnFormat.Value);
        }

        AddData(payload, data);
    }

    private static void AddData<T>(Dictionary<string, string> payload, T data)
    {
        payload["data"] = JsonSerializer.Serialize(data, RedcapJsonOptions.Default);
    }

    private static T DeserializeResponse<T>(string response, string payloadName) where T : class
    {
        try
        {
            var result = JsonSerializer.Deserialize<T>(response, RedcapJsonOptions.Default);
            return result ?? throw new RedcapApiException($"REDCap returned an empty {payloadName} payload.");
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to deserialize REDCap {PayloadName} response.", payloadName);
            throw new RedcapApiException($"Failed to deserialize REDCap {payloadName} response.", ex);
        }
    }

    private static void AddIndexedValues(Dictionary<string, string> payload, string key, IReadOnlyList<string>? values)
    {
        if (values == null || values.Count == 0)
        {
            return;
        }

        for (var i = 0; i < values.Count; i++)
        {
            payload[$"{key}[{i}]"] = values[i];
        }
    }

    private static void AddOptional(Dictionary<string, string> payload, string key, string? value)
    {
        if (!IsNullOrEmpty(value))
        {
            payload[key] = value!;
        }
    }

    private static void RequireItems<T>(IReadOnlyCollection<T>? values, string message)
    {
        if (values == null || values.Count == 0)
        {
            throw new RedcapApiException(message);
        }
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing && _ownsTransport && _transport is IDisposable disposableTransport)
        {
            disposableTransport.Dispose();
        }

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
