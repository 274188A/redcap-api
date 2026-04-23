# RedcapAPI

A .NET library for interacting with the [REDCap](https://www.project-redcap.org/) API.

## Installation

```
dotnet add package RedcapAPI
```

## Usage

```csharp
var api = new Redcap.RedcapApi("https://your-redcap-instance/api/", token);
var records = await api.ExportRecordsAsync(...);
```

## License

See [LICENSE.md](LICENSE.md).
