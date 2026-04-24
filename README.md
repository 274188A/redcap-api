# redcap-api

A .NET 10 library for the [REDCap](https://www.project-redcap.org/) REST API. Clone it, reference it directly, use it however you like.

## Requirements

- .NET 10 SDK
- A REDCap instance and an API token with appropriate rights

## Getting started

```csharp
using Redcap;

var api = new RedcapApi("https://your-redcap-instance/api/");

// Export records as JSON
var result = await api.ExportRecordsAsync(
    token: "YOUR_API_TOKEN",
    format: RedcapFormat.json
);
```

The constructor also accepts an `IRedcapTransport` for testing or custom HTTP behaviour.

## What's covered

The library wraps the full REDCap API surface:

| Area | Methods |
|---|---|
| Records | `ExportRecordsAsync`, `ExportRecordAsync`, `ImportRecordsAsync`, `DeleteRecordsAsync`, `RenameRecordAsync`, `GenerateNextRecordNameAsync` |
| Metadata | `ExportMetaDataAsync`, `ImportMetaDataAsync`, `ExportFieldNamesAsync` |
| Instruments | `ExportInstrumentsAsync`, `ExportInstrumentMappingAsync`, `ImportInstrumentMappingAsync`, `ExportPDFInstrumentsAsync` |
| Events | `ExportEventsAsync`, `ImportEventsAsync`, `DeleteEventsAsync` |
| Arms | `ExportArmsAsync`, `ImportArmsAsync`, `DeleteArmsAsync` |
| Files | `ExportFileAsync`, `ImportFileAsync`, `DeleteFileAsync` |
| File repository | `ExportFilesFoldersFileRepositoryAsync`, `ExportFileFileRepositoryAsync`, `ImportFileRepositoryAsync`, `DeleteFileRepositoryAsync`, `CreateFolderFileRepositoryAsync` |
| Users | `ExportUsersAsync`, `ImportUsersAsync`, `DeleteUsersAsync` |
| User roles | `ExportUserRolesAsync`, `ImportUserRolesAsync`, `DeleteUserRolesAsync`, `ExportUserRoleAssignmentAsync`, `ImportUserRoleAssignmentAsync` |
| DAGs | `ExportDagsAsync`, `ImportDagsAsync`, `DeleteDagsAsync`, `ExportUserDagAssignmentAsync`, `ImportUserDagAssignmentAsync`, `SwitchDagAsync` |
| Surveys | `ExportSurveyLinkAsync`, `ExportSurveyParticipantsAsync`, `ExportSurveyQueueLinkAsync`, `ExportSurveyReturnCodeAsync`, `ExportSurveyAccessCodeAsync` |
| Reports | `ExportReportsAsync` |
| Logging | `ExportLoggingAsync` |
| Project | `ExportProjectInfoAsync`, `ImportProjectInfoAsync`, `ExportProjectXmlAsync`, `CreateProjectAsync`, `ExportRedcapVersionAsync` |

## Building and testing

```bash
dotnet restore RedcapApi.slnx
dotnet build RedcapApi.slnx -c Release
dotnet test tests/RedcapApi.Tests/RedcapApi.Tests.csproj --verbosity minimal
```

End-to-end tests against a real REDCap instance are skipped by default. Set these environment variables to run them:

```
REDCAP_E2E_URL=https://your-redcap-instance/api/
REDCAP_E2E_TOKEN=your_token
REDCAP_E2E_RECORD_ID=1
REDCAP_E2E_FORM=your_form
```

## Project layout

```
src/RedcapApi/          library source
tests/RedcapApi.Tests/  xUnit tests
```

## License

MIT — see [LICENSE.md](LICENSE.md).
