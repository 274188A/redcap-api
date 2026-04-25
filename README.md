<div align="center">

# redcap-api

A .NET 10 library for the [REDCap](https://www.project-redcap.org/) REST API.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE.md)
[![Last Commit](https://img.shields.io/github/last-commit/274188A/redcap-api)](https://github.com/274188A/redcap-api/commits/master)

</div>

---

## Requirements

- .NET 10 SDK
- A REDCap instance and an API token with appropriate rights

## Getting started

```csharp
using Redcap;
using Redcap.Models;

using var api = new RedcapApi(
    "https://your-redcap-instance/api/",
    "YOUR_API_TOKEN"
);

// Export records as JSON
var result = await api.ExportRecordsAsync(
    format: RedcapFormat.json
);
```

The token now belongs to the client instance rather than being passed to each API call.

## Common workflows

If you want to control HTTP behavior in tests or production, inject an `IRedcapTransport`:

```csharp
var api = new RedcapApi(
    "https://your-redcap-instance/api/",
    "YOUR_API_TOKEN",
    transport
);
```

Per-call timeouts and cancellation tokens are available on API methods:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

var users = await api.ExportUsersTypedAsync(cancellationToken: cts.Token);
```

The default transport also has its own fallback timeout for calls that do not pass a positive `timeOutSeconds` value:

```csharp
using Redcap.Api;

using var transport = new DefaultRedcapTransport(timeOutSeconds: 100);
using var api = new RedcapApi("https://your-redcap-instance/api/", "YOUR_API_TOKEN", transport);
```

To download a file from a REDCap file-upload field, provide a destination directory:

```csharp
var savedFileName = await api.ExportFileAsync(
    record: "1",
    field: "consent_pdf",
    eventName: "event_1_arm_1",
    filePath: "downloads"
);
```

For API failures, catch `RedcapApiException`; HTTP failures include the status code and raw response body when REDCap provides one:

```csharp
using Redcap.Exceptions;

try
{
    var projectInfo = await api.ExportProjectInfoTypedAsync();
}
catch (RedcapApiException ex)
{
    Console.WriteLine(ex.StatusCode);
    Console.WriteLine(ex.ResponseBody ?? ex.Message);
}
```

## Migration note

Version 2.x moved the REDCap token into `RedcapApi` construction:

- Before: `api.ExportRecordsAsync(token, ...)`
- Now: `new RedcapApi(url, token)` followed by `api.ExportRecordsAsync(...)`

If you inject a custom transport, the caller still owns that transport's lifetime.

## Building and testing

```bash
dotnet restore RedcapApi.slnx
dotnet build RedcapApi.slnx -c Release
dotnet test tests/RedcapApi.Tests/RedcapApi.Tests.csproj --verbosity minimal
```

End-to-end tests against a real REDCap instance are skipped by default. Set these environment variables to run them:

| Variable | Description |
|---|---|
| `REDCAP_E2E_URL` | Full URL to your REDCap API endpoint |
| `REDCAP_E2E_TOKEN` | API token |
| `REDCAP_E2E_RECORD_ID` | A valid record ID in your project |
| `REDCAP_E2E_FORM` | A valid instrument/form name |

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the endpoint workflow, transport-test expectations, and guidance for adding REDCap API surface area.

## Project layout

```
src/RedcapApi/          library source
tests/RedcapApi.Tests/  xUnit tests
```

## API reference

<details>
<summary>Show all methods</summary>

| Area | Methods |
|---|---|
| Records | `ExportRecordsAsync` `ExportRecordAsync` `ImportRecordsAsync` `DeleteRecordsAsync` `RenameRecordAsync` `GenerateNextRecordNameAsync` |
| Metadata | `ExportMetaDataAsync` `ImportMetaDataAsync` `ExportFieldNamesAsync` |
| Instruments | `ExportInstrumentsAsync` `ExportInstrumentMappingAsync` `ImportInstrumentMappingAsync` `ExportPDFInstrumentsAsync` |
| Events | `ExportEventsAsync` `ImportEventsAsync` `DeleteEventsAsync` |
| Arms | `ExportArmsAsync` `ImportArmsAsync` `DeleteArmsAsync` |
| Files | `ExportFileAsync` `ImportFileAsync` `DeleteFileAsync` |
| File repository | `ExportFilesFoldersFileRepositoryAsync` `ExportFileFileRepositoryAsync` `ImportFileRepositoryAsync` `DeleteFileRepositoryAsync` `CreateFolderFileRepositoryAsync` |
| Users | `ExportUsersAsync` `ImportUsersAsync` `DeleteUsersAsync` |
| User roles | `ExportUserRolesAsync` `ImportUserRolesAsync` `DeleteUserRolesAsync` `ExportUserRoleAssignmentAsync` `ImportUserRoleAssignmentAsync` |
| DAGs | `ExportDagsAsync` `ImportDagsAsync` `DeleteDagsAsync` `ExportUserDagAssignmentAsync` `ImportUserDagAssignmentAsync` `SwitchDagAsync` |
| Surveys | `ExportSurveyLinkAsync` `ExportSurveyParticipantsAsync` `ExportSurveyQueueLinkAsync` `ExportSurveyReturnCodeAsync` `ExportSurveyAccessCodeAsync` |
| Reports | `ExportReportsAsync` |
| Logging | `ExportLoggingAsync` |
| Project | `ExportProjectInfoAsync` `ImportProjectInfoAsync` `ExportProjectXmlAsync` `CreateProjectAsync` `ExportRedcapVersionAsync` |

</details>

---

## License

MIT — see [LICENSE.md](LICENSE.md).
