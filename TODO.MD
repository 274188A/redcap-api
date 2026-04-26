# TODO

This backlog tracks the next reliability pass for the REDCap client library.

## Goal

Make download, stream, form-encoding, and JSON behavior trustworthy end to end, then reduce repeated request and typed-response plumbing.

## 1. File download contract

- [x] Add `LocalHttpServer` coverage for `ExportFileAsync` saving a returned file to `filePath`.
- [x] Assert the export-file payload still includes `token`, `content=file`, `action=export`, `record`, `field`, `event`, and optional `repeat_instance`.
- [x] Assert `filePath` is never posted to REDCap; it is only a local save destination.
- [x] Route `ExportFileAsync` through the transport download path instead of the normal string-response path.
- [x] Keep or deliberately change the current behavior that creates a missing destination directory.

## 2. Download filename handling

- [x] Update `Utils.DownloadFileAsync` to prefer `Content-Disposition` `filename*`, then `filename`.
- [x] Preserve server-provided file extensions instead of blindly appending `.pdf`.
- [x] Add tests for quoted filenames, filenames with existing extensions, and non-PDF downloads.
- [x] Add tests for missing filename headers.
- [x] Keep the existing path-traversal guard in `ReadAsFileAsync` covered by tests.
- [x] Make PDF instrument exports use a predictable fallback name when the server does not send one.

## 3. Stream lifetime

- [x] Decide whether `GetStreamContentAsync` should return a readable buffered stream or a response-owning download object.
- [x] Replace the current disposed-stream behavior with the chosen contract.
- [x] Update `GetStreamContentAsync_ReturnsResponseStream` to assert the returned stream can read `stream-body`.
- [x] Keep non-success status coverage that preserves status code and response body.

## 4. Form encoding

- [x] Change `CustomFormUrlEncodedContent` to produce UTF-8 bytes, or replace it with the framework form encoder if behavior matches.
- [x] Add a request-capture test with non-ASCII values such as `cafe` with an accent.
- [x] Keep coverage for spaces as `+`, reserved characters, and empty dictionaries.

## 5. JSON serialization

- [x] Replace `Newtonsoft.Json` usage with `System.Text.Json`.
- [x] Preserve REDCap wire names for typed models, especially snake_case fields and existing import payload shapes.
- [x] Add serialization/deserialization tests around representative export and import models before removing the package reference.
- [x] Centralize JSON options so typed exports, imports, and helper methods use the same settings.
- [x] Remove the `Newtonsoft.Json` package reference once behavior is covered by tests.

## 6. Architecture cleanup

- [x] Add a shared typed-response helper for JSON deserialization and consistent `RedcapApiException` messages.
- [ ] Add multipart payload helpers for common fields such as `token`, `content`, `action`, `returnFormat`, and `repeat_instance`.
- [ ] Review cancellation and timeout behavior after the download work, especially whether caller cancellation should stay as `OperationCanceledException`.
- [ ] Consider focused tests for caller-owned `HttpClient` behavior in `DefaultRedcapTransport.FromHttpClient(...)`.

## Verification

- [ ] Run `dotnet test tests/RedcapApi.Tests/RedcapApi.Tests.csproj --no-restore --verbosity minimal`.
- [ ] Run `dotnet pack src/RedcapApi/RedcapApi.csproj -c Release -o artifacts` before publishing.
