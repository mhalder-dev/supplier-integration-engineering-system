# ADR-0002 — Response mappers must be decomposed into builders

**Status:** Accepted · **Date:** 2026-09-05

## Context

Response mappers are the largest files in a supplier estate by a wide margin. Across an estate
of 17 supplier services we analysed, the five biggest files were all response mappers, topping
out at 2,028 lines. Thirteen of the seventeen had a mapper over 800 lines. Twenty-five files
across the estate exceeded 1,000 lines.

They are untestable in isolation, unreviewable in a pull request, and merge-conflict magnets.

Crucially, this is **not** forced by supplier complexity. Two of the services did the same
job — map a search response to `FlightResultDto`:

| | Decomposed service | Monolithic service |
| --- | --- | --- |
| Files | 11 | 1 |
| Largest file | 187 lines | 2,028 lines |
| Testable in isolation | yes | no |

The decomposed service's supplier protocol (NDC, reference-based) is arguably *harder* than
the monolithic one's.

## Decision

Every response mapper is decomposed into:

- a **thin orchestrator** (`<F>ResponseMapper`) that composes and holds no mapping logic
- an **assembler** that builds the client DTO from builder outputs
- **one builder per output concern**, one concern per file (segments, fares, baggage,
  penalties, filters, branded fares…)
- a **`ReferenceData`** lookup object injected once rather than threaded through parameters

Stateless builders are `static`. The orchestrator stays under ~80 lines; past that, a concern
has leaked into it.

**This applies from the first commit.** Retrofitting decomposition costs roughly a week per
service, which is why it never happens.

## Consequences

- **Positive:** each concern independently unit-testable; parallel work without conflicts;
  reviewable diffs; a new fare type is a new builder, not another 200 lines in a god class.
- **Negative:** more files, and a small up-front design cost naming the builders.
- **Neutral:** total line count is similar (~1,100 vs 2,028 for the compared pair) — this buys
  structure, not brevity.

Generated DTOs (from a WSDL) are exempt. Hand-written mappers never are.
