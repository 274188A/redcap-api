namespace Redcap.Api;

/// <summary>
/// REDCap API form-encoded field name constants. These are the wire keys sent in every POST request.
/// </summary>
internal static class PayloadKey
{
    public const string Token           = "token";
    public const string Content         = "content";
    public const string Action          = "action";
    public const string Format          = "format";
    public const string ReturnFormat    = "returnFormat";
    public const string Data            = "data";
    public const string Type            = "type";
    public const string Record          = "record";
    public const string Records         = "records";
    public const string Field           = "field";
    public const string Fields          = "fields";
    public const string Event           = "event";
    public const string Events          = "events";
    public const string Forms           = "forms";
    public const string Instrument      = "instrument";
    public const string RepeatInstance  = "repeat_instance";
    public const string Arm             = "arm";
    public const string Arms            = "arms";
    public const string Override        = "override";
    public const string DeleteLogging   = "delete_logging";
    public const string LogType         = "logtype";
    public const string Name            = "name";
    public const string File            = "file";
    public const string DocId           = "doc_id";
    public const string Dag             = "dag";
    public const string ReportId        = "report_id";
    public const string RandomizationId = "randomization_id";
    public const string NewRecordName   = "new_record_name";
}
