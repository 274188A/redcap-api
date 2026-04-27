namespace Redcap.Models;

/// <summary>
/// Options for <see cref="Redcap.RedcapApi.ImportRecordsAsync{T}(RecordImportOptions{T}, CancellationToken)"/>.
/// </summary>
/// <typeparam name="T">The record payload item type.</typeparam>
public sealed class RecordImportOptions<T>
{
    /// <summary>Requested REDCap import format.</summary>
    public RedcapFormat Format { get; init; } = RedcapFormat.json;

    /// <summary>Requested REDCap record import layout.</summary>
    public RedcapDataType RedcapDataType { get; init; } = RedcapDataType.flat;

    /// <summary>Overwrite behavior to apply during import.</summary>
    public OverwriteBehavior OverwriteBehavior { get; init; } = OverwriteBehavior.normal;

    /// <summary>Whether REDCap should auto-number imported records.</summary>
    public bool ForceAutoNumber { get; init; }

    /// <summary>Whether REDCap should process the import in the background.</summary>
    public bool BackgroundProcess { get; init; }

    /// <summary>Formatted record payload to import.</summary>
    public List<T> Data { get; init; } = [];

    /// <summary>Optional date format hint for imported date values.</summary>
    public string? DateFormat { get; init; }

    /// <summary>CSV delimiter to use when importing CSV data.</summary>
    public CsvDelimiter CsvDelimiter { get; init; } = CsvDelimiter.tab;

    /// <summary>Content REDCap should return after the import completes.</summary>
    public ReturnContent ReturnContent { get; init; } = ReturnContent.count;

    /// <summary>Error response format requested from REDCap.</summary>
    public RedcapReturnFormat ReturnFormat { get; init; } = RedcapReturnFormat.json;

    /// <summary>Per-request timeout in seconds.</summary>
    public long TimeOutSeconds { get; init; } = 100;
}
