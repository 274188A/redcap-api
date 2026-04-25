using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RedcapApi.Tests;

internal sealed class LocalHttpServer : IDisposable
{
    private static readonly byte[] HeaderTerminator = "\r\n\r\n"u8.ToArray();

    private readonly TcpListener _listener;
    private readonly Func<CapturedRequest, TestResponse> _responseFactory;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _listenerTask;

    public LocalHttpServer(Func<CapturedRequest, TestResponse> responseFactory)
    {
        _responseFactory = responseFactory;

        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();

        var port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        Url = new Uri($"http://localhost:{port}/");

        _listenerTask = Task.Run(ListenAsync);
    }

    public Uri Url { get; }

    public ConcurrentQueue<CapturedRequest> Requests { get; } = new();

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _listener.Stop();

        try
        {
            _listenerTask.GetAwaiter().GetResult();
        }
        catch
        {
        }

        _cancellationTokenSource.Dispose();
    }

    private async Task ListenAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            TcpClient? client = null;

            try
            {
                client = await _listener.AcceptTcpClientAsync(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (SocketException)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }

                throw;
            }

            _ = Task.Run(() => HandleClientAsync(client), _cancellationTokenSource.Token);
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using var _ = client;
        using var registration = _cancellationTokenSource.Token.Register(() => client.Dispose());

        try
        {
            using var stream = client.GetStream();
            var (request, responseEncoding) = await ReadRequestAsync(stream, _cancellationTokenSource.Token);

            Requests.Enqueue(request);

            var response = _responseFactory(request);
            await WriteResponseAsync(stream, response, responseEncoding, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (IOException) when (_cancellationTokenSource.IsCancellationRequested)
        {
        }
        catch (ObjectDisposedException) when (_cancellationTokenSource.IsCancellationRequested)
        {
        }
    }

    private static async Task<(CapturedRequest Request, Encoding Encoding)> ReadRequestAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        using var requestBuffer = new MemoryStream();

        var headerEndIndex = -1;
        while (headerEndIndex < 0)
        {
            var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
            if (bytesRead == 0)
            {
                throw new IOException("Client disconnected before sending a complete request.");
            }

            requestBuffer.Write(buffer, 0, bytesRead);
            headerEndIndex = IndexOf(requestBuffer.GetBuffer().AsSpan(0, (int)requestBuffer.Length), HeaderTerminator);
        }

        var requestBytes = requestBuffer.GetBuffer().AsSpan(0, (int)requestBuffer.Length);
        var headerBytes = requestBytes[..headerEndIndex];
        var bodyOffset = headerEndIndex + HeaderTerminator.Length;

        var headerText = Encoding.ASCII.GetString(headerBytes);
        var headerLines = headerText.Split("\r\n", StringSplitOptions.None);
        var requestLine = headerLines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 1; i < headerLines.Length; i++)
        {
            var line = headerLines[i];
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
            {
                continue;
            }

            headers[line[..separatorIndex]] = line[(separatorIndex + 1)..].Trim();
        }

        var contentLength = headers.TryGetValue("Content-Length", out var contentLengthValue)
            && int.TryParse(contentLengthValue, out var parsedContentLength)
            ? parsedContentLength
            : 0;

        var bodyBytes = new byte[contentLength];
        var bufferedBodyBytes = requestBytes.Length - bodyOffset;
        if (bufferedBodyBytes > 0)
        {
            requestBytes[bodyOffset..(bodyOffset + Math.Min(contentLength, bufferedBodyBytes))].CopyTo(bodyBytes);
        }

        var remainingBytes = contentLength - Math.Max(0, bufferedBodyBytes);
        var destinationOffset = Math.Max(0, bufferedBodyBytes);
        while (remainingBytes > 0)
        {
            var bytesRead = await stream.ReadAsync(bodyBytes.AsMemory(destinationOffset, remainingBytes), cancellationToken);
            if (bytesRead == 0)
            {
                throw new IOException("Client disconnected before sending the full request body.");
            }

            destinationOffset += bytesRead;
            remainingBytes -= bytesRead;
        }

        var responseEncoding = GetEncoding(headers.TryGetValue("Content-Type", out var contentType) ? contentType : null);
        var body = responseEncoding.GetString(bodyBytes);

        var path = requestLine.Length > 1 && Uri.TryCreate(requestLine[1], UriKind.RelativeOrAbsolute, out var uri)
            ? uri.IsAbsoluteUri ? uri.AbsolutePath : requestLine[1]
            : "/";

        return (new CapturedRequest(
            requestLine.Length > 0 ? requestLine[0] : "GET",
            path,
            body,
            new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase)), responseEncoding);
    }

    private static async Task WriteResponseAsync(NetworkStream stream, TestResponse response, Encoding encoding, CancellationToken cancellationToken)
    {
        var responseBodyBytes = encoding.GetBytes(response.Body);
        var builder = new StringBuilder()
            .Append("HTTP/1.1 ")
            .Append(response.StatusCode)
            .Append(' ')
            .Append(GetReasonPhrase(response.StatusCode))
            .Append("\r\n")
            .Append("Content-Type: ")
            .Append(response.ContentType)
            .Append("\r\n")
            .Append("Content-Length: ")
            .Append(responseBodyBytes.Length)
            .Append("\r\n")
            .Append("Connection: close\r\n");

        foreach (var header in response.Headers)
        {
            builder.Append(header.Key)
                .Append(": ")
                .Append(header.Value)
                .Append("\r\n");
        }

        builder.Append("\r\n");

        var headerBytes = Encoding.ASCII.GetBytes(builder.ToString());
        await stream.WriteAsync(headerBytes, cancellationToken);

        if (responseBodyBytes.Length > 0)
        {
            await stream.WriteAsync(responseBodyBytes, cancellationToken);
        }

        await stream.FlushAsync(cancellationToken);
    }

    private static Encoding GetEncoding(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return Encoding.UTF8;
        }

        const string charsetMarker = "charset=";
        var charsetIndex = contentType.IndexOf(charsetMarker, StringComparison.OrdinalIgnoreCase);
        if (charsetIndex < 0)
        {
            return Encoding.UTF8;
        }

        var charset = contentType[(charsetIndex + charsetMarker.Length)..].Trim().TrimEnd(';');
        try
        {
            return Encoding.GetEncoding(charset);
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8;
        }
    }

    private static int IndexOf(ReadOnlySpan<byte> source, ReadOnlySpan<byte> value)
    {
        for (var i = 0; i <= source.Length - value.Length; i++)
        {
            if (source[i..(i + value.Length)].SequenceEqual(value))
            {
                return i;
            }
        }

        return -1;
    }

    private static string GetReasonPhrase(int statusCode) => statusCode switch
    {
        200 => "OK",
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        500 => "Internal Server Error",
        _ => "OK"
    };
}

internal sealed record CapturedRequest(string Method, string Path, string Body, Dictionary<string, string> Headers);

internal sealed class TestResponse
{
    public TestResponse(int statusCode, string body, string contentType = "text/plain", Dictionary<string, string>? headers = null)
    {
        StatusCode = statusCode;
        Body = body;
        ContentType = contentType;
        Headers = headers ?? new Dictionary<string, string>();
    }

    public int StatusCode { get; }

    public string Body { get; }

    public string ContentType { get; }

    public Dictionary<string, string> Headers { get; }
}
