# ADR-0003 — Canonical feature vocabulary

**Status:** Accepted · **Date:** 2026-09-05

## Context

Across an estate of 17 supplier services we analysed, the same concept carried different names
in different services (counts are services using that spelling):

| Concept | Variants |
| --- | --- |
| Issue a ticket | `IssueTicket` (10) · `Ticketing` (7) · `AirTicketingDetails` (5) |
| Cancel | `CancelBooking` (8) · `CancelPNR` (5) |
| Revalidate | `Revalidate` (13) · `ReValidate` (4) |
| Fare rules | `FareRule` (6) · `FareRules` (6) |
| Import PNR | `ImportPnr` (9) · `ImportPNR` (3) |
| Auth | `Auth` (9) · `Authenticate` (2) |
| Logging | `Loging` (**16**) · `Logging` (1) |

`Loging` — a misspelling — is in 16 of 17 services. A typo survived sixteen supposedly
independent projects, which is conclusive evidence that services are created by copying.

The cost is real: a developer moving between services cannot predict folder names, and
cross-service search misses results.

## Decision

Adopt the canonical vocabulary table in
[`docs/coding-standards.md §2`](../coding-standards.md). All new features and new services use
it. Acronyms follow .NET convention (`Pnr`, `Api`, `Id`).

Existing services are **not** renamed — namespace renames after release are expensive and
risky, and the benefit is developer convenience rather than correctness.

An adopter with services already in flight inherits this rule for new work and keeps its own
existing names; an adopter starting fresh gets the table for free.

## Consequences

- **Positive:** predictable navigation; cross-service search works; no eighteenth spelling.
- **Negative:** new services differ from old ones, so an existing estate stays mixed until the
  old services are retired.
- **Accepted trade-off:** consistency going forward beats consistency with a broken past.
