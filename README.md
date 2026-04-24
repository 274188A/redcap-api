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

var api = new RedcapApi("https://your-redcap-instance/api/");

// Export records as JSON
var result = await api.ExportRecordsAsync(
    token: "YOUR_API_TOKEN",
    format: RedcapFormat.json
);
```

The constructor also accepts an `IRedcapTransport` for testing or custom HTTP behaviour.

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
