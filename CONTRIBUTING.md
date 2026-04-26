# Contributing

This repository is small on purpose: one library project, one test project, and a very explicit style for how REDCap endpoints are added and verified. The fastest way to make a good change here is to preserve that shape instead of inventing a new one.

## Repository layout

- `src/RedcapApi` contains the library that is packed as `RedcapAPI`.
- `tests/RedcapApi.Tests` contains xUnit tests for payload composition, transport behavior, and a small amount of end-to-end coverage.
- `RedcapApi.slnx` ties the two projects together.

Most API surface is split across matching partial files:

- Interfaces live in `src/RedcapApi/Interfaces/IRedcap.*.cs`.
- Implementations live in `src/RedcapApi/Api/RedcapApi.*.cs`.
- Shared client helpers live in `src/RedcapApi/Api/RedcapApi.cs`.
- The default HTTP implementation lives in `src/RedcapApi/Api/DefaultRedcapTransport.cs`.

## Daily commands

Run these from the repo root:

```bash
dotnet restore RedcapApi.slnx
dotnet build RedcapApi.slnx -c Release --no-restore
dotnet test tests/RedcapApi.Tests/RedcapApi.Tests.csproj --verbosity minimal
```

Useful variants:

```bash
dotnet test tests/RedcapApi.Tests/RedcapApi.Tests.csproj --filter "FullyQualifiedName~ExportDagsAsync_UsesExpectedPayload"
dotnet test tests/RedcapApi.Tests/RedcapApi.Tests.csproj --filter "Category!=E2E"
dotnet test tests/RedcapApi.Tests/RedcapApi.Tests.csproj --collect:"XPlat Code Coverage" --verbosity minimal
dotnet pack src/RedcapApi/RedcapApi.csproj -c Release -o artifacts
```

## Architecture rules

The client is intentionally transport-first.

- Public `RedcapApi` methods build REDCap request payloads and hand them to `IRedcapTransport`.
- Endpoint methods should not create `HttpClient` instances directly.
- `DefaultRedcapTransport` is the only built-in transport that knows about `HttpClient`, timeout wiring, and download behavior.
- `RedcapApi` owns the transport only when it creates the default transport itself. Injected transports remain caller-owned.
- If you need DI-friendly HTTP usage, prefer `DefaultRedcapTransport.FromHttpClient(...)`.

The shared payload helpers in `src/RedcapApi/Api/RedcapApi.cs` are part of the house style. Prefer them over hand-rolled dictionary setup:

- `ExecuteAsync(...)` for normal form posts
- `ExecuteMultipartAsync(...)` for multipart file requests
- `ExecuteDownloadAsync(...)` for download-to-disk endpoints
- `AddFormattedRequest(...)`, `AddActionRequest(...)`, `AddImportRequest(...)`
- `AddIndexedValues(...)`, `AddOptional(...)`, `AddData(...)`, `RequireItems(...)`

If you find yourself retyping `token`, `content`, `format`, `action`, `returnFormat`, or JSON serialization in a new method, stop and route it through the helpers instead.

## REDCap wire-contract conventions

The API contract matters more than internal elegance.

- Use the existing enums in `src/RedcapApi/Models` for wire values such as `content`, `format`, `action`, `returnFormat`, and similar flags.
- Those enums use `[Display(Name="...")]`; always write the payload value with `GetDisplayName()`.
- Keep payload key names exactly aligned with REDCap expectations and existing tests.
- Preserve current payload shapes for arrays, booleans, timestamps, and optional values unless you are intentionally changing behavior.

Tests in this repo pin the exact posted keys and values. If a payload looks slightly odd but has coverage around it, assume the current wire shape is deliberate until proven otherwise.

## Adding or changing an endpoint

Follow this sequence when extending the API:

1. Add or update the method on the relevant partial interface file under `src/RedcapApi/Interfaces`.
2. Implement it in the matching `src/RedcapApi/Api/RedcapApi.*.cs` file.
3. Build the payload with the shared helpers in `RedcapApi.cs`.
4. Reuse existing models and enums where possible instead of introducing ad hoc strings.
5. Add or update transport tests that pin the exact payload.
6. Update XML docs and README examples if the public behavior changed.

When the surrounding area already exposes multiple overloads, maintain parity unless there is a strong reason not to. This library has historically favored overload completeness, and the test suite assumes that style.

## Typed overload guidance

String-returning methods remain the compatibility baseline. Typed methods are additive.

- Add a typed overload when REDCap returns a stable JSON object or array shape that is genuinely useful to consumers.
- Keep the original string-returning method unless you are making a deliberate breaking change.
- Typed overloads should force JSON, deserialize with the existing Newtonsoft-based model layer, and throw `RedcapApiException` when deserialization fails or REDCap returns an empty payload.
- Skip typed overloads for endpoints that are inherently raw, file-based, highly dynamic, or not worth modeling.

## Validation guidance

Public endpoint validation should feel consistent to callers.

- Use `RequireItems(...)` for missing or empty public collections.
- Use `RedcapApiException` for public request-shape failures that a consumer can correct.
- Keep standard framework exceptions for lower-level guard clauses where that is already the local convention, such as constructor null checks or utility argument validation.

Do not silently coerce bad input into empty payload values.

## Testing strategy

Most changes should start with `RedcapApiTransportTests`.

- `FakeTransport` is the primary seam for endpoint work.
- Transport tests should assert the exact payload keys and values that would be posted.
- If you add a typed overload, also test successful deserialization and bad JSON failure behavior.
- If you add validation, include a test that proves the transport is not called on invalid input.

Use `LocalHttpServer` only when you need real HTTP behavior that `FakeTransport` cannot prove well:

- timeout and cancellation behavior
- HTTP status and response body handling
- request encoding, headers, or path parsing
- download and file-writing behavior

`RecordsTest` is the only end-to-end test class. It is skipped unless these environment variables are set:

- `REDCAP_E2E_URL`
- `REDCAP_E2E_TOKEN`
- `REDCAP_E2E_RECORD_ID`
- `REDCAP_E2E_FORM`

## Docs and release hygiene

If you change public behavior, also check the surrounding docs:

- README usage examples
- XML doc comments on the API surface
- package-facing assumptions such as disposal, timeout behavior, or typed overload availability

Before considering a change done, make sure the repo still builds cleanly in Release and the relevant tests pass.

## A good final checklist

- The endpoint lives in the right partial interface and partial implementation.
- Payload composition uses the shared helpers instead of bespoke dictionary boilerplate.
- Tests pin the wire contract and any new validation or typed behavior.
- Public docs still describe the code truthfully.
- `dotnet build` and `dotnet test` pass.
