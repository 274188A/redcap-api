# TODO

This file is no longer an active backlog.

The hardening and cleanup pass that originally lived here is complete, and every item from that list has already been implemented.

## Completed areas

- Correctness and contract work:
  per-request timeout behavior, disposal ownership, fail-fast utility behavior, and doc/code alignment.
- Public API shape:
  cached REDCap version exposure, async naming cleanup, typed export overloads, and smaller interface groupings.
- Internal maintainability:
  shared payload-building helpers, common request-shape helpers, consistent endpoint validation, and centralized outbound serialization.
- Packaging and release quality:
  package metadata, deterministic build settings, source-link/symbol packaging, and warnings-as-errors.
- Tests and developer experience:
  expanded transport and HTTP behavior coverage, richer README examples, and a rewritten contributor guide.
- DI-friendly transport support:
  caller-owned `HttpClient` support through `DefaultRedcapTransport.FromHttpClient(...)`.

## Current state

- No open items remain in this file.
- If new work is needed, start a fresh backlog instead of extending this completed one.
- Prefer tracking future work by concrete outcome or issue rather than by historical cleanup phase.
