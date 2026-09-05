# tests/

This repository holds standards, not services, so there is no application code here to unit
test. This folder holds the **shared testing assets** that supplier test suites reuse.

The testing standard itself is [`docs/testing-strategy.md`](../docs/testing-strategy.md).

## What belongs here

| Path | Contents | Status |
| --- | --- | --- |
| `fixtures/` | Shared, supplier-agnostic fixtures — reference data samples, canonical client-DTO examples | empty |
| `schemas/` | JSON schemas for the client contract, asserted against by every supplier's contract tests | **not created** |
| `harness/` | Reusable test helpers (fixture loading, redaction checks, canonical XML comparison) | **not created** |

**Nothing here is populated yet.** The working fixture harness lives in the template at
`reference/bootstrap-reference/tests/` and is copied into each new service; this folder is for
assets genuinely shared *across* services, and none exist until a second service needs one.
See [`development/readiness.md`](../development/readiness.md), gap 6.

Supplier-specific fixtures live in that supplier's own test project
(`<Supplier>.AirAPI.Tests/Fixtures/`), **not here**. Only genuinely cross-supplier assets
belong in this folder.

## Contract schemas — the near-term priority

Until the shared contracts package exists
([ADR-0001](../docs/decision-records/ADR-0001-shared-contracts-package.md)), a committed JSON
schema for `FlightResultDto` is the cheapest defence against further drift. Every supplier's
contract test asserts its output against that schema, so divergence becomes a failing test
rather than an invisible difference discovered in production.

This is deliberately the lowest-cost half of ADR-0001: it needs no NuGet feed, no release
process, and no coordination — one service can adopt it today.

**Status:** not written yet, and blocked on a decision this repository cannot take.

**The open decision:** *which `FlightResultDto` shape is canonical?* A schema can only be
written once one shape is chosen, and the choice belongs to the team that owns the aggregator's
contract — in the estate we analysed the DTO existed in 17 copies with 17 different hashes and
property counts ranging from 4 to 23, so picking one unilaterally here would simply add an
eighteenth.

Carry it as a live question, not a footnote: add it to the open-questions table in
[`development/progress.md`](../development/progress.md) with a named owner and the date raised,
and write the schema in the commit that records the answer.

## Fixture redaction

Every fixture in this repository and in every supplier test project must be redacted before
commit: replace credential and PII **values**, preserve the **shape**. A fixture whose
structure was changed by redaction no longer tests the real wire format.

See [`docs/testing-strategy.md`](../docs/testing-strategy.md) and
[L-07](../knowledge/lessons-learned.md).
