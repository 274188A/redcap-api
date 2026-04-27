# TODO — RedcapApi .NET Library

Authoritative improvement backlog for the `redcap-api` .NET 10 library. Items are ordered highest to lowest priority. Every item is directly actionable: a developer should be able to pick it up, know exactly which file(s) to change, and understand why.

Priority bands:
- **P1** — Blocking / must be done before any other work lands on `master`
- **P2** — Important / high-value quality improvements
- **P3** — Nice-to-have / developer-experience polish
- **P4** — Future / research-stage, no immediate urgency

---

## P1 — Critical / Blocking

### Branch Merge

- [x] **Merge `newtonsoft-json-migration` → `master`**
  All 216 tests pass. Nothing else should land until `master` reflects the current working state. Run `git checkout master && git merge --no-ff newtonsoft-json-migration`, verify `dotnet test` still passes, then delete the feature branch.

---

### Remove Stale CI/Packaging References

- [x] **Remove the "CI" section from `CLAUDE.md`**
  `CLAUDE.md` references `.github/workflows/ci.yml`, `.github/workflows/publish-github-packages.yml`, `bitbucket-pipelines.yml`, `azure-pipelines.yml`, and `.travis.yml`. None of these files exist or will ever exist in this repo. Delete the entire "CI" section to eliminate confusion for contributors.

- [x] **Fix the dependency description in `CLAUDE.md`**
  The "Repository Layout" section currently reads "The library depends only on Newtonsoft.Json and Serilog." The migration is complete; the library no longer references Newtonsoft.Json. Update to: "The library depends only on Serilog; JSON serialization uses `System.Text.Json` from the .NET runtime."

- [x] **Remove stale CI badge references from `README.md`**
  The README header contains Azure Pipelines and Travis CI badge `img` links that reference non-existent pipelines. Remove them entirely.

- [x] **Remove the `Newtonsoft.Json` version pin from `Directory.Packages.props`**
  `<PackageVersion Include="Newtonsoft.Json" Version="13.0.4" />` remains in the central package manifest even though no project references it after the migration. Delete the entry.

---

### Documentation

- [x] **Create `CHANGELOG.md` with a v1.x → v2.x migration guide**
  Users upgrading have no consolidated reference for breaking changes. Create `CHANGELOG.md` at the repo root (use [Keep a Changelog](https://keepachangelog.com) format) with a `## [2.0.0]` section covering: token moved to constructor; `IRedcapTransport` injection pattern; `DefaultRedcapTransport` lifetime and `FromHttpClient`; `BrokenCertificate` replacing the old static `Utils.UseInsecureCertificate` flag; `System.Text.Json` replacing Newtonsoft.Json.

---

## P2 — Important / High-Value Quality

### Code Quality

- [x] **Simplify `ConvertArraytoString<T>()` in `src/RedcapApi/Utilities/Utils.cs`**
  The method uses a manual `StringBuilder` loop that appends a trailing comma on every element and calls `TrimEnd(',')`. Replace the entire loop body with `return string.Join(",", inputArray);`. Existing tests in `UtilitiesTests.cs` cover the behavior; verify they still pass.

- [x] **Cache reflection results in `GetDisplayName(this Enum)` in `src/RedcapApi/Utilities/Utils.cs`**
  `GetDisplayName` calls `GetType().GetMember(...)` and `GetCustomAttribute<DisplayAttribute>()` on every invocation — once per payload key per API call. Introduce a `static readonly ConcurrentDictionary<(Type, string), string> _displayNameCache` and populate it on first access for each `(type, memberName)` pair. Public signature is unchanged; no test changes required.

- [x] **Introduce a `PayloadKey` static constants class**
  Literal strings such as `"content"`, `"format"`, `"action"`, `"token"`, `"records"`, `"returnFormat"`, `"data"`, and `"type"` appear as dictionary keys throughout `src/RedcapApi/Api/RedcapApi.cs` and its partial files. Create `src/RedcapApi/Api/PayloadKey.cs` with `public static class PayloadKey` holding `public const string Content = "content";` etc. Replace all raw strings in `AddContent`, `AddFormattedRequest`, `AddActionRequest`, `AddImportRequest`, `CreatePayload`, and related helpers.

- [x] **Add `RecordExportOptions` / `RecordImportOptions` parameter objects**
  `ExportRecordsAsync` in `src/RedcapApi/Api/RedcapApi.Records.cs` has 21 parameters. Add non-breaking overloads `ExportRecordsAsync(RecordExportOptions options, CancellationToken cancellationToken = default)` and the equivalent for import, where the options objects carry all current optional parameters as properties with identical defaults. Keep all existing overloads intact — this is purely additive. Document in CONTRIBUTING.md that new endpoints with more than ~8 optional parameters should prefer options objects.

---

### Testing

- [x] **Extract `TestConstants` from test files**
  The string `"token123"` is declared as `private const string Token` independently in every test class. Create `tests/RedcapApi.Tests/TestConstants.cs` with `internal static class TestConstants { public const string Token = "token123"; ... }` and replace all duplicate declarations. Add any other repeated literals (localhost base URL, common record IDs) as named constants here.

- [x] **Convert copy-paste overload tests to `[Theory]` + `[InlineData]`**
  `RedcapApiTransportTests.cs` has 152 `[Fact]` methods and zero `[Theory]` usages. Many tests for overload variants differ only in which optional arguments are omitted. Identify clusters of structurally identical tests (e.g., multiple `ExportArmsAsync` overloads that each assert `content=arm`) and consolidate them into `[Theory]` + `[InlineData]` or `[MemberData]`. Target ≥20% reduction in total `[Fact]` count without losing any assertion coverage.

- [x] **Add cancellation forwarding tests for all domain areas**
  `CancellationTests.cs` verifies `CancellationToken` forwarding for Records, Files, and Users only. Every remaining domain (Arms, DAGs, Events, FieldNames, FileRepository, Instruments, Logging, Metadata, Projects, RepeatingInstruments, Reports, Surveys, UserRoles, Version) needs at least one `_ForwardsCancellationTokenToTransport` test. The pattern already exists in `CancellationTests.cs`.

- [x] **Add `ConcurrencyTests.cs`**
  There are no tests that verify the transport and API instance are safe under concurrent load. Add a test class that issues 20–50 parallel calls against a `LocalHttpServer` using a single shared `RedcapApi` instance and asserts all calls complete without exceptions or data mixing. This documents — and guards — the thread-safety invariant of `DefaultRedcapTransport`.

- [x] **Extend `FakeTransport` with `AllPayloads` history**
  `FakeTransport` (bottom of `RedcapApiTransportTests.cs`) only stores `LastDictionaryPayload` / `LastMultipartPayload` / `LastDownloadDestinationPath` — the most recent call only. Multi-step workflows that call multiple API methods sequentially cannot assert on earlier calls. Add `public List<Dictionary<string, string>> AllDictionaryPayloads { get; } = new();` and `public List<MultipartFormDataContent> AllMultipartPayloads { get; } = new();`, appending to them on each call alongside updating the `Last*` properties.

- [x] **Create `ValidationTests.cs` for guard-clause coverage**
  There is no dedicated test class for input-validation behavior. Create `tests/RedcapApi.Tests/ValidationTests.cs` and add tests verifying: `ArgumentNullException` from the constructor when `token` is null; `ArgumentException` when `token` is empty; `RedcapApiException` from `RequireItems(...)` when a required collection is empty (e.g., `DeleteArmsAsync(Array.Empty<RedcapArm>())`). Use `FakeTransport` and assert `LastDictionaryPayload == null` to confirm the transport is never invoked.

- [x] **Add 429 and 503 error simulation in `HttpErrorTests.cs`**
  `HttpErrorTests.cs` tests only 401, 403, 400, and 500. Add `[InlineData(429)]` and `[InlineData(503)]` to the existing `[Theory]` so those status codes are verified to produce `RedcapApiException`. Also add a test for HTTP 200 with a malformed JSON body that verifies typed overloads (e.g., `ExportDagsTypedAsync`) throw `RedcapApiException` with a meaningful message rather than an unhandled `JsonException`.

### Developer Experience

- [x] **Add `.editorconfig` at the repo root**
  Without one, C# formatting rules are IDE-dependent with no tooling enforcement. Create `.editorconfig` with at minimum: `indent_style = space`, `indent_size = 4`, `end_of_line = lf`, `charset = utf-8-bom`, and the standard `[*.cs]` C# analyzer severity settings that align with `TreatWarningsAsErrors = true` already in `Directory.Build.props`.

- [x] **Add `global.json` to pin the .NET 10 SDK version**
  Different team members may use different .NET 10 preview/stable SDK builds. Add `global.json` at the repo root with `{ "sdk": { "version": "10.0.xxx", "rollForward": "latestPatch" } }` where `xxx` matches the team's current toolchain (`dotnet --version`). This prevents build surprises when a new preview SDK ships.

- [x] **Add `Microsoft.CodeAnalysis.NetAnalyzers`**
  The repo enforces `TreatWarningsAsErrors` but has no Roslyn analyzer package beyond the compiler. Add `<PackageVersion Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="9.0.0" />` to `Directory.Packages.props` and `<PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" PrivateAssets="all" />` to a shared `<ItemGroup>` in `Directory.Build.props`. Set baseline severities in `.editorconfig`.

- [x] **Add `build.ps1` for one-command dev setup**
  Contributors currently assemble `dotnet` commands from CONTRIBUTING.md manually. Add `build.ps1` at the repo root with named targets: `Restore`, `Build`, `Test`, `TestE2E`. A `./build.ps1 -Target Test` invocation becomes the canonical "does everything pass?" command.

---

## P3 — Nice-to-Have

- [x] **Clean up or remove `bitbucket-pipelines.yml`**
  CLAUDE.md notes the pipeline block is duplicated (only the first copy runs). Since this repo has no CI, either delete the file entirely or leave exactly one block with a comment. Do not let the duplication silently grow.

- [x] **Document the "no CI / no automated publish" decision in `CONTRIBUTING.md`**
  Add a short note so future contributors do not add CI pipelines or NuGet publish workflows. Saves future sessions from rediscovering the decision.

---

## P4 — Future / Research

- [x] **Stryker.NET mutation testing**
  Run Stryker.NET against `src/RedcapApi` to measure mutation score for `Utils.cs`, `DefaultRedcapTransport.cs`, and the Execute helpers in `RedcapApi.cs`. A score below 70% in core logic suggests the test suite has blind spots that `[Theory]` consolidation alone will not catch. Add `stryker-config.json` and a `./build.ps1 -Target Mutate` target when approved. Runs locally — no CI required. Current baseline: `65.35%` mutation score.

- [ ] **DocFX API documentation site**
  The library has thorough XML doc comments on all public surfaces. Add a `docs/` folder with a `docfx.json` targeting `src/RedcapApi` and instructions in CONTRIBUTING.md for generating the site locally (`docfx docs/docfx.json --serve`).

- [ ] **BenchmarkDotNet performance baselines**
  Add a `benchmarks/RedcapApi.Benchmarks` project measuring: `GetDisplayName` with and without the reflection cache; `ConvertArraytoString<T>` for various array sizes; `JsonSerializer.Serialize` / `Deserialize` for representative model payloads. Establish baselines before any performance-motivated refactoring.

- [ ] **ArchUnitNET architectural constraint tests**
  Add an `ArchitectureTests.cs` test class enforcing: classes in `Redcap.Api` must not reference the test project; `DefaultRedcapTransport` must only be instantiated through its constructors or `FromHttpClient`; `Utils.cs` must not have cyclic references back into `Redcap.Api`. Record current violations as known exceptions until resolved.

---

## Deferred Until Last

- [ ] **Expand the E2E test suite**
  Only one E2E test exists (`ExportRecordAsync`). Add `[SkippableFact]` tests — guarded by the same `REDCAP_E2E_*` env-var pattern and `[Trait("Category", "E2E")]` attribute — for: `ImportRecordsAsync`, `ExportUsersAsync` / `ExportUsersTypedAsync`, an `ImportFileAsync` + `ExportFileAsync` round-trip, and `ExportSurveyLinkAsync`.
