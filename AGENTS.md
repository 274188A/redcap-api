# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## Repository Layout

Two .NET 10 projects held together by `RedcapApi.slnx`:

- `src/RedcapApi` — the library (NuGet package `RedcapAPI`, version 2.0.0)
- `tests/RedcapApi.Tests` — xUnit test project (transport-level + a few E2E tests)

Both target `net10.0`. The library depends only on `Newtonsoft.Json` and `Serilog`.

## Common Commands

```bash
# restore / build / test the full solution
dotnet restore RedcapApi.slnx
dotnet build RedcapApi.slnx -c Release --no-restore
dotnet test tests/RedcapApi.Tests/RedcapApi.Tests.csproj --verbosity minimal

# run a single test by fully-qualified name
dotnet test tests/RedcapApi.Tests/RedcapApi.Tests.csproj --filter "FullyQualifiedName~ExportDagsAsync_UsesExpectedPayload"

# skip the E2E-tagged tests (they no-op unless env vars are set, but you can filter explicitly)
dotnet test tests/RedcapApi.Tests/RedcapApi.Tests.csproj --filter "Category!=E2E"

# tests with coverage
dotnet test tests/RedcapApi.Tests/RedcapApi.Tests.csproj --collect:"XPlat Code Coverage" --verbosity minimal

# pack the NuGet
dotnet pack src/RedcapApi/RedcapApi.csproj -c Release -o artifacts
```

## Architecture

### Transport abstraction is the testability seam

`RedcapApi` (in `src/RedcapApi/Api/RedcapApi.cs`, ~5,000 lines) implements `IRedcap` (in `src/RedcapApi/Interfaces/IRedcap.cs`, ~2,100 lines). Every public API method builds a `Dictionary<string, string>` payload (or `MultipartFormDataContent` for file uploads) and hands it to `IRedcapTransport`. The default transport (`DefaultRedcapTransport`) delegates to static helpers in `Utils.cs` that drive `HttpClient`.

This seam exists so tests can assert the composed payload without a real REDCap server. `RedcapApiTransportTests.cs` uses a `FakeTransport` that records `LastDictionaryPayload` / `LastMultipartPayload`; the majority of tests assert the exact keys and values the library would POST. When adding or modifying an endpoint, add/update a transport test that pins the payload shape — that is the house style here. There is also a `LocalHttpServer` helper (HttpListener on loopback) for tests that need to exercise the real transport end-to-end without a REDCap instance.

`RedcapApi` exposes three constructors: URL-only, URL + `IRedcapTransport`, and URL + `useInsecureCertificates` flag (which toggles a static `Utils.UseInsecureCertificate` used by the default transport when building its `HttpClient`). Prefer the transport-injection constructor in tests.

### Enums carry wire values via `[Display]` + `GetDisplayName()`

The REDCap API is form-encoded with lower-case string parameters (`content=record`, `format=json`, `action=import`, …). The codebase models these as C# enums in `src/RedcapApi/Models` (`Content`, `RedcapFormat`, `RedcapAction`, `RedcapReturnFormat`, `OverwriteBehavior`, etc.), each member decorated with `[Display(Name="…")]`. The extension `Utils.GetDisplayName(this Enum)` reads that attribute and is called whenever a payload value is written — e.g. `{ "content", Content.Arm.GetDisplayName() }`. When adding a new enum value, the `[Display]` name is the wire contract.

### Overload fan-out

`IRedcap` deliberately exposes many overloads per operation (varying which optional parameters are present, and whether `Content` is passed explicitly). When adding or changing an endpoint, check whether parity is needed across the existing overload set — tests in this repo explicitly cover overload parity and the 2.0.0 release notes call out "overload parity across API method variants" as a dimension of the coverage work.

### E2E tests

`RecordsTests.cs` is the only test class tagged `[Trait("Category", "E2E")]`. It silently returns early when `REDCAP_E2E_URL` / `REDCAP_E2E_TOKEN` are unset, so a default `dotnet test` run locally will pass without hitting any network. Use `REDCAP_E2E_URL`, `REDCAP_E2E_TOKEN`, `REDCAP_E2E_RECORD_ID`, `REDCAP_E2E_FORM` to actually exercise it.

## CI

Three pipelines exist and all must stay green:

- `.github/workflows/ci.yml` — `dotnet restore/build/test` on push/PR to `master` (uses .NET 10 SDK, Ubuntu).
- `.github/workflows/publish-github-packages.yml` — publishes the NuGet to GitHub Packages.
- `bitbucket-pipelines.yml` — build + test + pack against `mcr.microsoft.com/dotnet/sdk:10.0`. Note this file currently has the pipeline block duplicated; only the first copy runs, but don't let the duplication grow.
- `azure-pipelines.yml` / `.travis.yml` — older CI definitions still referenced by the README badge.
