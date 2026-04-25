# TODO

Working list for turning `redcap-api` into a genuinely excellent library, not just a functioning one.

## P0 Correctness And Contract

- [x] Make `timeOutSeconds` actually effective per request.
  - Today every public API method still accepts `timeOutSeconds`, but `DefaultRedcapTransport` sets `HttpClient.Timeout` once in its constructor and ignores the per-call value.
  - Files: `src/RedcapApi/Api/DefaultRedcapTransport.cs`, `src/RedcapApi/Api/RedcapApi.cs`, `src/RedcapApi/Utilities/Utils.cs`
  - Done: `DefaultRedcapTransport` now uses linked cancellation tokens with `CancelAfter`, and timeout behavior is covered through the public API in `CancellationTests`.

- [x] Define and implement a clear disposal model.
  - `DefaultRedcapTransport` owns `HttpClient` and is disposable, but `RedcapApi` itself does not expose disposal semantics.
  - Files: `src/RedcapApi/Api/RedcapApi.cs`, `src/RedcapApi/Api/DefaultRedcapTransport.cs`
  - Done: `RedcapApi` now implements `IDisposable`, disposes only internally-created transports, leaves injected transports under caller ownership, and is covered by disposal tests.

- [x] Stop swallowing utility errors and returning ambiguous defaults.
  - `ConvertArraytoString` catches exceptions, logs, and returns `string.Empty`, which can silently turn caller mistakes into malformed API payloads.
  - Files: `src/RedcapApi/Utilities/Utils.cs`
  - Done: array conversion helpers now throw on null/empty input, and tests assert the fail-fast behavior.

- [x] Audit the refactor for any remaining "public API says one thing, implementation does another" mismatches.
  - Focus: constructor token model, timeout behavior, disposal, docs, XML comments, README examples.
  - Goal: zero misleading surface area.
  - Done: aligned XML docs and README examples with constructor-owned tokens, disposal ownership, and actual timeout behavior, and added a short migration note for 2.x consumers.

## P1 Public API Design

- [x] Replace the mutable public `Version` field with a proper property or explicit cache strategy.
  - File: `src/RedcapApi/Api/RedcapApi.cs`
  - Goal: remove odd mutable-field semantics from the main client type.
  - Done: `Version` is now a read-only cached property populated by `ExportRedcapVersionAsync`, and transport tests lock that behavior down.

- [x] Review method naming and async conventions for consistency.
  - Current standouts: `RandomizeRecord`, `ExportRepeatingInstrumentsAndEvents`, and `ImportRepeatingInstrumentsAndEvents` all return `Task` but do not follow `Async` naming.
  - Files: `src/RedcapApi/Api/*.cs`, `src/RedcapApi/Interfaces/*.cs`
  - Goal: make the API feel native to modern .NET consumers.
  - Done: added `Async`-suffixed method names for the inconsistent task-returning members, kept obsolete compatibility shims for existing callers, and covered both paths with transport tests.

- [x] Review whether string-returning methods should expose richer typed results in key places.
  - Many methods return raw `string` even when the response shape is well known.
  - Goal: identify high-value typed overloads without breaking existing consumers unnecessarily.
  - Done: added `ExportProjectInfoTypedAsync`, `ExportDagsTypedAsync`, `ExportUserDagAssignmentTypedAsync`, `ExportUsersTypedAsync`, `ExportArmsTypedAsync`, `ExportEventsTypedAsync`, `ExportMetaDataTypedAsync`, `ExportInstrumentsTypedAsync`, `ExportInstrumentMappingTypedAsync`, `ExportUserRolesTypedAsync`, `ExportUserRoleAssignmentTypedAsync`, `ExportRepeatingInstrumentsAndEventsTypedAsync`, `ExportFieldNamesTypedAsync`, and `ExportLoggingTypedAsync`, using existing or newly-introduced models while preserving the original string-returning APIs. Left inherently raw/dynamic endpoints like reports, records, files, links, and XML/version/count responses as strings because they do not have a stable JSON object shape worth modeling here.

- [ ] Decide the long-term shape of the giant `IRedcap` surface.
  - Today the API is split across partial interfaces/files but still presents one very large client.
  - Files: `src/RedcapApi/Interfaces`, `src/RedcapApi/Api`
  - Goal: evaluate grouped clients or functional sub-interfaces for maintainability and discoverability.

## P1 Internal Design And Maintainability

- [ ] Reduce remaining payload-building duplication across partials.
  - Many methods still hand-roll the same `"token"`, `"content"`, `"format"`, `"returnFormat"` patterns.
  - Files: `src/RedcapApi/Api/*.cs`
  - Goal: keep methods small, consistent, and hard to get wrong.

- [ ] Introduce focused helper abstractions for common request shapes.
  - Examples: export requests, import requests, delete requests, file download requests.
  - Goal: fewer copy-paste variations and easier future endpoint additions.

- [ ] Normalize argument validation strategy across the library.
  - Some methods throw `RedcapApiException`, some rely on `ArgumentException`, some log and continue.
  - Goal: predictable consumer experience and simpler tests.

- [ ] Review serialization strategy intentionally.
  - Current implementation relies on `JsonConvert.SerializeObject(...)` in many import methods.
  - Goal: document or refactor toward one explicit serialization policy.

## P1 Packaging And Release Quality

- [x] Add explicit package metadata to the library project.
  - Missing or unclear in `RedcapApi.csproj`: package id/version/source link/repository URL/package tags/license/readme inclusion.
  - File: `src/RedcapApi/RedcapApi.csproj`
  - Goal: make the NuGet package polished and self-describing.
  - Done: added explicit package id/version, repository and project URLs, MIT license expression, tags, author/description, packed the root `README.md` into the NuGet package, and centralized dependency versions in `Directory.Packages.props`.

- [x] Add release-quality build settings.
  - Candidates: deterministic builds, SourceLink, symbol packages, XML docs validation, nullable/warning tightening.
  - Goal: more trustworthy packages and easier debugging for consumers.
  - Done: enabled deterministic/CI-friendly build metadata, portable PDBs, symbol package generation, repository/source embedding settings, and GitHub SourceLink in `RedcapApi.csproj`.

- [x] Decide whether warnings should be elevated.
  - File: project files / CI
  - Goal: keep quality regressions from creeping in quietly.
  - Done: the solution was already warning-clean in Release builds, so warnings are now treated as errors repo-wide via `Directory.Build.props`.

## P1 Test Quality

- [x] Expand tests around timeout behavior once fixed.
  - Goal: prove the contract instead of assuming it.
  - Done: `CancellationTests` covers forwarding caller cancellation tokens and verifies that a per-call timeout cancels a slow request through the public API.

- [x] Add tests around disposal/lifetime semantics once defined.
  - Goal: lock down ownership behavior.
  - Done: `CancellationTests` verifies injected transports remain caller-owned and internally-created transports reject calls after the client is disposed.

- [x] Add more targeted tests for helper edge cases.
  - Examples: array conversion failures, malformed filenames, empty payload inputs, header parsing in the local test server.
  - Files: `tests/RedcapApi.Tests`
  - Done: utility tests now cover array conversion failures, file-name safety, empty dictionary payloads, and local test server request header/path/charset parsing.

- [x] Keep the new `TcpListener` test server intentionally minimal and well-covered.
  - File: `tests/RedcapApi.Tests/LocalHttpServer.cs`
  - Goal: avoid growing a hidden second HTTP stack without tests.
  - Done: utility tests directly cover request capture, path/header/body parsing, charset decoding, configured response headers/content types, and multiple-request ordering.

## P2 Documentation And Developer Experience

- [x] Expand README usage examples beyond the happy path.
  - Examples: injected transport, file download, cancellation token usage, error handling, E2E environment variables.
  - File: `README.md`
  - Done: README now includes common workflow examples for injected transport, cancellation, default transport timeout, typed exports, file download, error handling, and E2E environment variables.

- [x] Add a migration note for the constructor-token breaking change.
  - Goal: help existing consumers move from per-call tokens to client-owned tokens cleanly.
  - Done: `README.md` now includes a 2.x migration note showing the move from per-call tokens to constructor-owned tokens.

- [x] Add contributor guidance for adding new endpoints.
  - House style already exists informally in tests; codify it in docs.
  - Goal: make future changes consistent without tribal knowledge.
  - Done: added `CONTRIBUTING.md` with endpoint workflow, transport-test expectations, overload parity guidance, and local HTTP server usage notes; linked it from `README.md`.

## P2 Modernization Candidates

- [ ] Evaluate moving from `Serilog` direct usage to `Microsoft.Extensions.Logging.Abstractions` or a library-neutral logging story.
- [ ] Evaluate `System.Text.Json` versus `Newtonsoft.Json` intentionally rather than by inertia.
- [ ] Evaluate `IHttpClientFactory` support for ASP.NET / DI-heavy consumers.

## Suggested Order

1. Correct timeout behavior.
2. Define disposal semantics.
3. Remove swallowed utility failures.
4. Add package metadata and stricter build quality settings.
5. Tackle API shape / typed responses / client partitioning.
