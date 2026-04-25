# Contributing

## Adding Or Changing API Endpoints

The main test seam is `IRedcapTransport`. Public API methods should build the REDCap form payload, then hand it to the transport. The default transport owns the actual HTTP work; endpoint methods should not create `HttpClient` instances directly.

When adding or changing an endpoint:

1. Add the public method to the relevant partial `IRedcap.*.cs` file and implement it in the matching `RedcapApi.*.cs` file.
2. Build payload values with model enums and `GetDisplayName()` for REDCap wire values such as `content`, `format`, `action`, and `returnFormat`.
3. Preserve overload parity where the surrounding API already exposes multiple variants.
4. Add or update `RedcapApiTransportTests` coverage with `FakeTransport` so the exact posted keys and values are pinned without a live REDCap server.
5. Use `LocalHttpServer` only when the real HTTP transport behavior matters, such as response errors, file downloads, timeout behavior, or request encoding.
6. Keep typed overloads additive. Existing string-returning APIs should continue to work unless a breaking change is intentional and documented.

Before opening a change, run:

```bash
dotnet test tests/RedcapApi.Tests/RedcapApi.Tests.csproj --verbosity minimal
```

End-to-end tests in `RecordsTests` are skipped unless the `REDCAP_E2E_*` environment variables are set.
