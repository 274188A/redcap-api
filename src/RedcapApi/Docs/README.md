[![.NET](https://github.com/274188A/redcap-api/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/274188A/redcap-api/actions/workflows/ci.yml)

[![NuGet](https://img.shields.io/nuget/dt/RedcapApi.svg?style=for-the-badge)](https://www.nuget.org/packages/RedcapAPI) 
[![license](https://img.shields.io/github/license/mashape/apistatus.svg?style=for-the-badge)](https://github.com/274188A/redcap-api/blob/master/LICENSE.md)

# REDCap API Library for .NET
The REDCap Api Library for .NET provides the ability to interact with REDCap programmatically using various .NET languages(C#,F#,VB.NET);

## What's New in 3.0.0

Version 3.0.0 is a breaking-change release focused on correctness, security, and maintainability.

- Removed global static `UseInsecureCertificate` — callers inject their own `HttpMessageHandler` for custom TLS
- All endpoints now throw `RedcapApiException` on error instead of returning the exception message as a string
- Shared `HttpClient` per `DefaultRedcapTransport` instance (eliminates socket exhaustion)
- Full nullable annotations on the public surface
- `ExecuteAsync` / `ExecuteMultipartAsync` helpers eliminate per-method boilerplate
- Fixed `ReadAsFileAsync` path traversal vulnerability and inverted existence check
- Synced `IRedcap` interface defaults with implementation across all File Repository methods

__Prerequisites__
1.  Local REDCap instance installed (visit https://project-redcap.org)
2.  Create a new project with "Demographics" for the template
3.  Create an API token and set `REDCAP_DEMO_BASE_URI` / `REDCAP_DEMO_PROJECT_TOKEN` environment variables
4.  You may need to add a field type of `file_upload` to test the file upload API
5.  Build the solution, then run the tests

__Highlights__
* Export and import records, metadata, users, roles, DAGs, events, instruments, reports, and files
* Project export, project XML export, project settings import, and next record name generation
* Survey link, survey queue link, survey return code, survey participants, and survey access code support
* File repository create/list/export/import/delete support
* Repeating instruments/events import and export support
* Randomize record support

__Usage__:

1. `dotnet restore`
2. Add a reference to the package or project
3. Add `using Redcap;` namespace
4. Add `using Redcap.Models;` for convenience
5. The repository is organized as follows:

    - `src/RedcapApi` — library source
    - `tests/RedcapApi.Tests` — test project

__Sample / Example__
```csharp
using Newtonsoft.Json;
using Redcap;
using Redcap.Models;

var redcap_api = new RedcapApi("https://localhost/redcap/api/");
var result = await redcap_api.ExportRecordsAsync("YOUR_API_TOKEN");
Console.WriteLine(JsonConvert.DeserializeObject(result));
```

__Install__
```
dotnet add package RedcapAPI --version 3.0.0
```
