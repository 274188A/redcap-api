[![.NET](https://github.com/274188A/redcap-api/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/274188A/redcap-api/actions/workflows/ci.yml)

[![NuGet](https://img.shields.io/nuget/dt/RedcapApi.svg?style=for-the-badge)](https://www.nuget.org/packages/RedcapAPI) 
[![license](https://img.shields.io/github/license/mashape/apistatus.svg?style=for-the-badge)](https://github.com/274188A/redcap-api/blob/master/LICENSE.md)

# REDCap API Library for .NET
The REDCap Api Library provides the ability to interact with REDCap programmatically using various .NET languages(C#,F#,VB.NET);

## What's New in 2.0.0

Version 2.0.0 is a cleanup and parity release focused on bringing the library in line with documented REDCap API behavior while modernizing the repository structure.

- Added `ExportSurveyAccessCodeAsync`
- Added interface coverage for randomization and user role mapping endpoints
- Added `combineCheckboxOptions` support for record export methods
- Added `delete_logging` support for record deletion methods
- Added ODM format support where REDCap supports it
- Expanded `Content` coverage for newer REDCap content values
- Fixed `ExportProjectXmlAsync` default content mapping
- Reorganized the repository into conventional `.NET` folders: `src` and `tests`

__Prerequisites__
1.  Local redcap instance installed (visit https://project-redcap.org) if you need to download files(assuming you have access)
2.  Create a new project with "Demographics" for the template; this gives you a basic project to work with.
3.  Create an api token for yourself, replace that with the tokens you see on the "RedcapApiTests.cs" files, and others
4.  You may need to add a field type of "file_upload" so that you can test the file upload interface of the API
5.  Build the solution and run the tests.

__Highlights__
* Export and import records, metadata, users, roles, DAGs, events, instruments, reports, and files
* Project export, project XML export, project settings import, and next record name generation
* Survey link, survey queue link, survey return code, survey participants, and survey access code support
* File repository create/list/export/import/delete support
* Repeating instruments/events import and export support
* Randomize record support

__Usage__:

1. dotnet restore
2. Add a reference to the package or project
3. Add "using Redcap" namespace
4. Add "using Redcap.Models" for convenience
5. Use the sample data dictionary in `src/RedcapApi/Docs` if you want a quick test project setup
6. The repository is organized as follows:

    - `src/RedcapApi` - library source
    - `tests/RedcapApi.Tests` - test project

7. Feel free to contribute

__Sample / Example__
```C# 

using Newtonsoft.Json;
using Redcap;

namespace RedcapApiDemo
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var apiToken = "3D57A7FA57C8A43F6C8803A84BB3957B";
            var redcap_api = new RedcapApi("http://localhost/redcap/api/");
            var result = await redcap_api.ExportRecordsAsync(apiToken);
            var records = JsonConvert.DeserializeObject(result);
            Console.WriteLine(records);
            Console.ReadLine();
            return 0;
        }
    }
}


```

__Install directly in Package Manager Console or Command Line Interface__
```C#
Package Manager

Install-Package RedcapAPI -Version 2.0.0

```

```C#
.NET CLI

dotnet add package RedcapAPI --version 2.0.0

 ```

```C#
Paket CLI

paket add RedcapAPI --version 2.0.0

```

## Testing and Coverage

The repository includes transport-focused unit tests that validate payload composition for API methods and overload variants without requiring a live REDCap instance.

Run tests:

```bash
dotnet test tests/RedcapApi.Tests/RedcapApi.Tests.csproj --verbosity minimal
```

Run tests with coverage:

```bash
dotnet test tests/RedcapApi.Tests/RedcapApi.Tests.csproj --collect:"XPlat Code Coverage" --verbosity minimal
```

Latest local baseline (2026-04-19):

- Test count: `128`
- Passing: `128`
- Failing: `0`
- Line coverage: `80.26%` (`2310/2878`)
- Branch coverage: `59.49%` (`445/748`)

Recent coverage work focused on:

- Overload parity across API method variants
- Multipart and dictionary payload branch paths
- File and file-repository request behavior
- Survey/report/record export optional parameter handling
