# RedcapApi Documentation

RedcapApi is a .NET client for the REDCap API. This docset is generated locally with DocFX from the XML documentation comments already present in the library source.

## What is here

- API reference generated from `src/RedcapApi/RedcapApi.csproj`
- Namespace and type pages based on the public surface of the package

## Build locally

From the repository root:

```bash
dotnet tool restore --configfile NuGet.Config
dotnet restore RedcapApi.slnx --configfile NuGet.Config
dotnet docfx docs/docfx.json --serve
```

DocFX writes the generated website to `docs/_site/` and serves it locally for preview.
