# ADR-0004 — Fixture-based testing as the default

**Status:** Accepted · **Date:** 2026-09-05

## Context

In an estate of 17 supplier services we analysed, twelve had no tests at all. Of the five that
did, four were token suites (10–29 tests) and two of those were copy-pasted duplicates of each
other. Both of the estate's architectural reference services had **zero** tests.

The standard justification is that supplier integrations cannot be tested without calling the
supplier. One service in the estate disproved it: 205 tests, no network, built on JSON fixtures
captured from the supplier's live staging API. Its own source documented the technique — the
fixtures were exercised against responses captured from the live staging API, so they carried
the real wire format rather than hand-written approximations.

## Decision

Fixture-based testing is the default for all supplier work.

- Capture real responses from a sandbox or staging environment.
- **Redact** credentials and PII before committing — replace values, preserve shape.
- Commit fixtures to the test project, copied to output.
- Test mappers, builders, and validators against them with no network.
- Record each fixture's provenance and date in `Fixtures/README.md`.

**Hand-written fixtures are not acceptable as a substitute.** A fixture you invented tests
your understanding of the supplier, not the supplier.

Five offline layers (mapper, builder, validator, service, contract) are required per feature.
Live sandbox calls are a sixth, manual, gated layer whose main purpose is producing new
fixtures.

## Consequences

- **Positive:** fast, deterministic, credential-free CI; supplier format changes are caught by
  a diff against the old fixture; testing no longer depends on sandbox availability.
- **Negative:** fixtures must be captured before tests can be written, so the first sandbox
  call becomes a prerequisite; fixtures need maintenance when suppliers change format.
- **Risk:** an un-redacted fixture leaks credentials. Mitigated by the redaction rule and by
  reviewing every fixture in its pull request.
