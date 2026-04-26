using Redcap.Exceptions;
using Redcap.Http;
using Redcap.Models;

using Serilog;

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Reflection;

namespace Redcap.Utilities;

/// <summary>
/// Utilities
/// </summary>
public static class Utils
{

    private static readonly ConcurrentDictionary<(Type, string), string> _displayNameCache = new();

    /// <summary>
    /// Method gets the display string for an enum. Falls back to the enum name when no [Display] attribute is present.
    /// </summary>
    public static string GetDisplayName(this Enum enumString)
    {
        var type = enumString.GetType();
        var name = enumString.ToString();
        return _displayNameCache.GetOrAdd((type, name), static key =>
        {
            var member = key.Item1.GetMember(key.Item2).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? key.Item2;
        });
    }

    /// <summary>
    /// Writes the response body to a file under <paramref name="path"/>, sanitising the filename so it cannot escape that directory.
    /// </summary>
    public static async Task ReadAsFileAsync(this HttpContent httpContent, string fileName, string path, bool overwrite, string fileExtension = "pdf", CancellationToken cancellationToken = default)
    {
        if (httpContent == null)
        {
            throw new ArgumentNullException(nameof(httpContent));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must be provided.", nameof(path));
        }

        // Strip directory separators from caller- or server-supplied names.
        var rawName = (fileName ?? string.Empty).Replace("\"", "");
        var safeName = Path.GetFileName(rawName);
        if (string.IsNullOrWhiteSpace(safeName))
        {
            throw new ArgumentException("fileName did not contain a usable file name.", nameof(fileName));
        }

        var normalizedExtension = fileExtension.TrimStart('.');
        if (!string.IsNullOrEmpty(normalizedExtension) &&
            !safeName.EndsWith("." + normalizedExtension, StringComparison.OrdinalIgnoreCase))
        {
            safeName = safeName + "." + normalizedExtension;
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
    public static string ConvertArraytoString<T>(this RedcapApi redcapApi, T[] inputArray)
    {
        if (inputArray.IsNullOrEmpty())
        {
            throw new ArgumentException("A non-empty array is required.", nameof(inputArray));
        }

        return string.Join(",", inputArray);
    }

    /// <summary>
    /// Delegates to <see cref="ConvertArraytoString{T}"/> for integer arrays.
    /// </summary>
    /// <param name="redcapApi"></param>
    /// <param name="inputArray"></param>
    /// <returns>string</returns>
    public static string ConvertIntArraytoString(this RedcapApi redcapApi, int[] inputArray)
    {
        return ConvertArraytoString<int>(redcapApi, inputArray);
    }

    /// <summary>
    ///The method hands the return content from a request, the response.
    /// The method allows the calling method to choose a return type.
    /// </summary>
    /// <param name="redcapApi"></param>
    /// <param name="returnContent"></param>
    /// <returns>string</returns>
    public static string HandleReturnContent(this RedcapApi redcapApi, ReturnContent returnContent = ReturnContent.count)
    {
        return Enum.IsDefined(returnContent) ? returnContent.ToString() : ReturnContent.count.ToString();
    }

    /// <summary>
    /// Tuple that returns both inputFormat and redcap returnFormat
    /// </summary>
    /// <param name="redcapApi"></param>
    /// <param name="format">csv, json[default], xml , odm ('odm' refers to CDISC ODM XML format, specifically ODM version 1.3.1)</param>
    /// <param name="onErrorFormat"></param>
    /// <param name="redcapDataType"></param>
    /// <returns>tuple, string, string, string</returns>
    public static (string format, string onErrorFormat, string redcapDataType) HandleFormat(this RedcapApi redcapApi, RedcapFormat? format = RedcapFormat.json, RedcapReturnFormat? onErrorFormat = RedcapReturnFormat.json, RedcapDataType? redcapDataType = RedcapDataType.flat)
    {
        var f = format ?? RedcapFormat.json;
        var e = onErrorFormat ?? RedcapReturnFormat.json;
        var d = redcapDataType ?? RedcapDataType.flat;
        var _format = Enum.IsDefined(f) ? f.ToString() : RedcapFormat.json.ToString();
        var _onErrorFormat = Enum.IsDefined(e) ? e.ToString() : RedcapReturnFormat.json.ToString();
        var _redcapDataType = Enum.IsDefined(d) ? d.ToString() : RedcapDataType.flat.ToString();
        return (_format, _onErrorFormat, _redcapDataType);
    }

    /// <summary>
    /// Method gets the overwrite behavior type and converts into string
    /// </summary>
    /// <param name="redcapApi"></param>
    /// <param name="overwriteBehavior"></param>
    /// <returns>string</returns>
    public static string ExtractBehavior(this RedcapApi redcapApi, OverwriteBehavior overwriteBehavior)
    {
        return Enum.IsDefined(overwriteBehavior) ? overwriteBehavior.ToString() : OverwriteBehavior.overwrite.ToString();
    }

    private static List<string> SplitToList(string input, char[] delimiters) =>
        string.IsNullOrEmpty(input)
            ? new List<string>()
            : input.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).ToList();

    /// <summary>
    /// Method extracts events into list from string
    /// </summary>
    /// <param name="redcapApi"></param>
    /// <param name="events"></param>
    /// <param name="delimiters">char[] e.g [';',',']</param>
    /// <returns>List of string</returns>
    public static List<string> ExtractEvents(this RedcapApi redcapApi, string events, char[] delimiters)
    {
        return SplitToList(events, delimiters);
    }

    /// <summary>
    /// Method gets / extracts fields into list from string
    /// </summary>
    /// <param name="redcapApi"></param>
    /// <param name="fields"></param>
    /// <param name="delimiters">char[] e.g [';',',']</param>
    /// <returns>List of string</returns>
    public static List<string> ExtractFields(this RedcapApi redcapApi, string fields, char[] delimiters)
    {
        return SplitToList(fields, delimiters);
    }

    /// <summary>
    /// Method gets / extract records into list from string
    /// </summary>
    /// <param name="redcapApi"></param>
    /// <param name="records"></param>
    /// <param name="delimiters">char[] e.g [';',',']</param>
    /// <returns>List of string</returns>
    public static List<string> ExtractRecords(this RedcapApi redcapApi, string records, char[] delimiters)
    {
        return SplitToList(records, delimiters);
    }

    /// <summary>
    /// Method gets / extracts forms into list from string
    /// </summary>
    /// <param name="redcapApi"></param>
    /// <param name="forms"></param>
    /// <param name="delimiters">char[] e.g [';',',']</param>
    /// <returns>A list of string</returns>
    public static List<string> ExtractForms(this RedcapApi redcapApi, string forms, char[] delimiters)
    {
        return SplitToList(forms, delimiters);
    }

    /// <summary>
    /// </summary>
    /// <param name="payload">data</param>
    /// <param name="uri">URI of the api instance</param>
    /// <param name="client">Caller-owned <see cref="HttpClient"/>.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Stream</returns>
    public static async Task<Stream?> GetStreamContentAsync(Dictionary<string, string> payload, Uri uri, HttpClient client, CancellationToken cancellationToken = default)
    {
        using var content = new CustomFormUrlEncodedContent(payload);
        using var response = await client.PostAsync(uri, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new RedcapApiException($"REDCap returned {(int)response.StatusCode} {response.ReasonPhrase}", response.StatusCode, body);
        }

        var stream = new MemoryStream();
        await response.Content.CopyToAsync(stream, cancellationToken);
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// Method to send http request using MultipartFormDataContent
    /// Requests with attachments
    /// </summary>
    /// <param name="payload">data</param>
    /// <param name="uri">URI of the api instance</param>
    /// <param name="client">Caller-owned <see cref="HttpClient"/>.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>string</returns>
    public static async Task<string> SendPostRequestAsync(MultipartFormDataContent payload, Uri uri, HttpClient client, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(uri, payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new RedcapApiException($"REDCap returned {(int)response.StatusCode} {response.ReasonPhrase}", response.StatusCode, body);
        }
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Sends request using http
    /// </summary>
    /// <param name="payload">data</param>
    /// <param name="uri">URI of the api instance</param>
    /// <param name="client">Caller-owned <see cref="HttpClient"/>.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string> SendPostRequestAsync(Dictionary<string, string> payload, Uri uri, HttpClient client, CancellationToken cancellationToken = default)
    {
        using var content = new CustomFormUrlEncodedContent(payload);
        using var response = await client.PostAsync(uri, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new RedcapApiException($"REDCap returned {(int)response.StatusCode} {response.ReasonPhrase}", response.StatusCode, body);
        }
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Posts <paramref name="payload"/> and saves the response body to <paramref name="destinationPath"/> on disk.
    /// </summary>
    /// <param name="payload">data</param>
    /// <param name="uri">URI of the api instance</param>
    /// <param name="client">Caller-owned <see cref="HttpClient"/>.</param>
    /// <param name="destinationPath">Directory on disk where the downloaded file is saved.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The saved filename, or empty string.</returns>
    public static async Task<string> DownloadFileAsync(Dictionary<string, string> payload, Uri uri, HttpClient client, string destinationPath, CancellationToken cancellationToken = default)
    {
        using var content = new CustomFormUrlEncodedContent(payload);
        using var response = await client.PostAsync(uri, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new RedcapApiException($"REDCap returned {(int)response.StatusCode} {response.ReasonPhrase}", response.StatusCode, body);
        }

        var fileName = GetDownloadFileName(response.Content.Headers);
        var isPdf = payload.TryGetValue(Redcap.Api.PayloadKey.Content, out var contentVal) && contentVal == "pdf";
        var fileExtension = string.Empty;
        if (isPdf)
        {
            fileName ??= GetPdfFallbackFileName(payload);
            if (!string.IsNullOrEmpty(fileName) && !Path.HasExtension(fileName))
            {
                fileExtension = "pdf";
            }
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidOperationException("Response did not include a download filename.");
        }

        await response.Content.ReadAsFileAsync(fileName, destinationPath, true, fileExtension, cancellationToken: cancellationToken);
        return GetSavedFileName(fileName, fileExtension);
    }

    private static string? GetDownloadFileName(HttpContentHeaders headers)
    {
        var contentDisposition = headers.ContentDisposition;
        return CleanHeaderFileName(contentDisposition?.FileNameStar)
            ?? CleanHeaderFileName(contentDisposition?.FileName)
            ?? CleanHeaderFileName(headers.ContentType?.Parameters.Select(x => x.Value).FirstOrDefault());
    }

    private static string? CleanHeaderFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var cleaned = fileName.Trim().Trim('"');
        return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
    }

    private static string GetSavedFileName(string fileName, string fileExtension)
    {
        var savedFileName = Path.GetFileName(CleanHeaderFileName(fileName) ?? fileName);
        var normalizedExtension = fileExtension.TrimStart('.');
        if (!string.IsNullOrEmpty(normalizedExtension) &&
            !savedFileName.EndsWith("." + normalizedExtension, StringComparison.OrdinalIgnoreCase))
        {
            savedFileName += "." + normalizedExtension;
        }

        return savedFileName;
    }

    private static string GetPdfFallbackFileName(Dictionary<string, string> payload)
    {
        return payload.TryGetValue(Redcap.Api.PayloadKey.Instrument, out var instrument) &&
            !string.IsNullOrWhiteSpace(instrument)
            ? instrument
            : "redcap-pdf-instruments";
    }

    /// <summary>
    /// Method obtains list of string from comma seperated strings
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="redcapApi"></param>
    /// <param name="arms"></param>
    /// <param name="delimiters"></param>
    /// <returns>List of string</returns>
    public static List<string> ExtractArms<T>(this RedcapApi redcapApi, string arms, char[] delimiters)
    {
        return SplitToList(arms, delimiters);
    }

    /// <summary>
    /// Checks if the string passed is null or empty.
    /// </summary>
    /// <param name="redcapApi"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static void CheckToken(this RedcapApi redcapApi, string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token, nameof(token));
    }

}
