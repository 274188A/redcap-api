# Handoff — redcap-api P2 Testing Work

## Goal
Work through the remaining P2 items in `TODO.MD` on the `newtonsoft-json-migration` branch. All work is committed and pushed. Pick up with the next unchecked testing item.

## Branch
`newtonsoft-json-migration` — all 216 tests pass. Do not merge to `master` until the branch is ready.

## What Was Done This Session

### Commits (newest first)
- `83400ca` — P2: extract TestConstants; remove 4 duplicate `private const string Token` declarations across test files; created `tests/RedcapApi.Tests/TestConstants.cs` with `Token = "token123"` and `BaseUrl = "http://localhost/"`
- `0e1759f` — P1/P2: stale CI cleanup, NetAnalyzers, .editorconfig, global.json, build.ps1, CHANGELOG, Utils refactors (string.Join + ConcurrentDictionary cache), PayloadKey constants class across all 17 partial files

### TODO.MD items now checked off
- [x] Remove stale CI/packaging refs from CLAUDE.md
- [x] Fix dependency description in CLAUDE.md
- [x] Remove CI badges from README.md
- [x] Remove Newtonsoft.Json from Directory.Packages.props
- [x] Create CHANGELOG.md
- [x] Simplify ConvertArraytoString<T>() — string.Join
- [x] Cache reflection in GetDisplayName — ConcurrentDictionary
- [x] Introduce PayloadKey static constants class
- [x] Add .editorconfig
- [x] Add global.json
- [x] Add Microsoft.CodeAnalysis.NetAnalyzers
- [x] Add build.ps1
- [x] Extract TestConstants from test files

## Next Task (pick up here)

**P2 — Convert copy-paste overload tests to `[Theory]` + `[InlineData]`**

> `RedcapApiTransportTests.cs` has 152 `[Fact]` methods and zero `[Theory]` usages. Many tests for overload variants differ only in which optional arguments are omitted. Identify clusters of structurally identical tests (e.g., multiple `ExportArmsAsync` overloads that each assert `content=arm`) and consolidate them into `[Theory]` + `[InlineData]` or `[MemberData]`. Target ≥20% reduction in total `[Fact]` count without losing any assertion coverage.

### How to approach it
1. Read `tests/RedcapApi.Tests/RedcapApiTransportTests.cs` — it is ~2600 lines
2. Look for clusters of `[Fact]` methods with names like `ExportArmsAsync_DefaultOverload_*` / `ExportArmsAsync_ContentOverload_*` that assert the same payload keys with no variation
3. Collapse each cluster into a single `[Theory]` + `[InlineData]` (or `[MemberData]` if setup differs)
4. Target: reduce from ~152 `[Fact]`s to ≤121 (≥20% reduction)
5. Run `dotnet test` — all 216 must still pass
6. Commit and push

## After That

Remaining P2 items in order:
1. Add cancellation forwarding tests for all domain areas (Arms, DAGs, Events, FieldNames, FileRepository, Instruments, Logging, Metadata, Projects, RepeatingInstruments, Reports, Surveys, UserRoles, Version) — pattern in `CancellationTests.cs`
2. Add `ConcurrencyTests.cs`
3. Extend `FakeTransport` with `AllPayloads` history (`AllDictionaryPayloads`, `AllMultipartPayloads` lists)
4. Create `ValidationTests.cs` for guard-clause coverage
5. Add 429 and 503 to `HttpErrorTests.cs` + malformed JSON body test
6. Expand E2E test suite (ImportRecordsAsync, ExportUsersAsync/Typed, file round-trip, ExportSurveyLinkAsync)
7. RecordExportOptions / RecordImportOptions parameter objects

Then P1: merge `newtonsoft-json-migration` → `master` and delete the branch.

## Key Files
- `tests/RedcapApi.Tests/RedcapApiTransportTests.cs` — main transport test file (~2600 lines, 152 [Fact]s)
- `tests/RedcapApi.Tests/CancellationTests.cs` — cancellation pattern to replicate
- `tests/RedcapApi.Tests/TestConstants.cs` — shared Token + BaseUrl constants
- `TODO.MD` — authoritative backlog; tick items `[x]` as completed
- `src/RedcapApi/Api/PayloadKey.cs` — wire key constants
