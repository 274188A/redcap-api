namespace Redcap.Models;

/// <summary>
/// Options for <see cref="Redcap.RedcapApi.ExportRecordsAsync(RecordExportOptions, CancellationToken)"/>.
/// </summary>
public sealed class RecordExportOptions
{
    /// <summary>Requested REDCap export format.</summary>
    public RedcapFormat Format { get; init; } = RedcapFormat.json;

    /// <summary>Requested REDCap record export layout.</summary>
    public RedcapDataType RedcapDataType { get; init; } = RedcapDataType.flat;

    /// <summary>Optional record ids to export.</summary>
    public string[]? Records { get; init; }

    /// <summary>Optional field names to export.</summary>
    public string[]? Fields { get; init; }

    /// <summary>Optional instrument names to export.</summary>
    public string[]? Forms { get; init; }

    /// <summary>Optional event names to export.</summary>
    public string[]? Events { get; init; }

    /// <summary>Whether to export raw values or labels.</summary>
    public RawOrLabel RawOrLabel { get; init; } = RawOrLabel.raw;

    /// <summary>Whether CSV headers should use raw names or labels.</summary>
    public RawOrLabelHeaders RawOrLabelHeaders { get; init; } = RawOrLabelHeaders.raw;

    /// <summary>Whether checkbox labels should be exported when labels mode is used.</summary>
    public bool ExportCheckboxLabel { get; init; }

    /// <summary>Error response format requested from REDCap.</summary>
    public RedcapReturnFormat ReturnFormat { get; init; } = RedcapReturnFormat.json;

    /// <summary>Whether survey pseudo-fields should be included.</summary>
    public bool ExportSurveyFields { get; init; }

    /// <summary>Whether the DAG pseudo-field should be included.</summary>
    public bool ExportDataAccessGroups { get; init; }

    /// <summary>Optional REDCap filter logic expression.</summary>
    public string? FilterLogic { get; init; }

    /// <summary>Optional lower bound for created or modified timestamps.</summary>
    public DateTime? DateRangeBegin { get; init; }

    /// <summary>Optional upper bound for created or modified timestamps.</summary>
    public DateTime? DateRangeEnd { get; init; }

    /// <summary>CSV delimiter to request when exporting CSV.</summary>
    public CsvDelimiter CsvDelimiter { get; init; } = CsvDelimiter.comma;

    /// <summary>Decimal character override for numeric output.</summary>
    public DecimalCharacter DecimalCharacter { get; init; } = DecimalCharacter.none;

    /// <summary>Whether gray-status completion fields should export as blank values.</summary>
    public bool ExportBlankForGrayFormStatus { get; init; }

    /// <summary>Whether checkbox labels should be combined into a single delimited value.</summary>
    public bool CombineCheckboxOptions { get; init; }

    /// <summary>Per-request timeout in seconds.</summary>
    public long TimeOutSeconds { get; init; } = 100;
}
