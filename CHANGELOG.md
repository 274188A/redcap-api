# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

---

## [2.0.0] — 2026-04-26

### Breaking changes

#### Token moved to construction

The REDCap API token is no longer passed to every method call. It is now supplied once at construction time and stored on the client instance.

```csharp
// Before (1.x)
var result = await api.ExportRecordsAsync(token: "MY_TOKEN", format: RedcapFormat.json);

// After (2.x)
using var api = new RedcapApi("https://your-instance/api/", "MY_TOKEN");
var result = await api.ExportRecordsAsync(format: RedcapFormat.json);
```

#### Transport injection replaces static HTTP helpers

The `IRedcapTransport` interface is now the single seam between the API client and HTTP. Pass a transport at construction to control HTTP behavior in tests or production. The old static helpers in `Utils.cs` are no longer part of the public contract.

```csharp
// Inject a custom or fake transport
var api = new RedcapApi("https://your-instance/api/", token, myTransport);

// Use the default transport with an IHttpClientFactory-owned HttpClient
var transport = DefaultRedcapTransport.FromHttpClient(httpClient, timeOutSeconds: 60);
using var api = new RedcapApi("https://your-instance/api/", token, transport);
```

`DefaultRedcapTransport` is `IDisposable`; it disposes the `HttpClient` it owns unless constructed via `FromHttpClient`, in which case lifetime remains with the caller.

#### Self-signed certificate bypass moved to `BrokenCertificate`

The old `Utils.UseInsecureCertificate` static boolean is gone. To bypass TLS validation in development, pass a handler built with `BrokenCertificate.DangerousAcceptAnyServerCertificateValidator`:

```csharp
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        BrokenCertificate.DangerousAcceptAnyServerCertificateValidator
};
using var transport = new DefaultRedcapTransport(handler);
using var api = new RedcapApi("https://your-dev-instance/api/", token, transport);
```

#### JSON serialization migrated from Newtonsoft.Json to System.Text.Json

The `Newtonsoft.Json` package is no longer a dependency. All serialization now uses `System.Text.Json`. Typed export methods (e.g. `ExportUsersTypedAsync`, `ExportDagsTypedAsync`) return POCOs deserialized with `PropertyNameCaseInsensitive = true`. If you were relying on Newtonsoft-specific attributes (`[JsonProperty]`) on your own models passed to import methods, replace them with `System.Text.Json` equivalents (`[JsonPropertyName]`).

### Added

- `DefaultRedcapTransport.FromHttpClient(HttpClient, long)` — factory for DI-managed `HttpClient` lifetimes.
- `BrokenCertificate.DangerousAcceptAnyServerCertificateValidator` — explicit opt-in for skipping TLS validation.
- `RedcapApiException.StatusCode` and `RedcapApiException.ResponseBody` — HTTP failures now surface the status code and raw REDCap response body.
- Per-call `timeOutSeconds` and `CancellationToken` parameters on every public API method.
- Typed export variants (e.g. `ExportUsersTypedAsync<T>`, `ExportDagsTypedAsync`) that deserialize directly to POCOs.
- `ExportFileAsync` now saves downloaded files to a caller-supplied directory path and returns the saved filename, with a path-traversal guard on the resolved destination.

### Removed

- `Newtonsoft.Json` dependency — replace `[JsonProperty]` with `[JsonPropertyName]` on custom models.
- `Utils.UseInsecureCertificate` static field — see `BrokenCertificate` above.
- Per-call `token` parameters on all API methods — token is now supplied at construction.

---

## [1.x]

Earlier releases are not formally documented here. See the git log for history prior to 2.0.0.
