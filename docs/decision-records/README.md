# Decision records

An ADR captures **why** a decision was made, at the time it was made, by whoever made it.
Code shows what was done; an ADR shows what else was considered and why it was rejected. That
is the part that is otherwise lost.

ADRs 0001–0007 are the **starter set** this engineering system ships with. They are decisions
already taken, on the evidence of an estate of 17 supplier services we analysed, and a new
adopter inherits them on day one. They are not sacred: an adopter may revisit any of them, but
the way to revisit one is to write a new ADR that supersedes it, never to edit it. Adopters
continue the sequence from **ADR-0008** onwards with their own decisions.

## Rules

1. **One decision per record.** If it needs "and", it is two ADRs.
2. **Immutable once accepted.** Do not edit an accepted ADR to reflect a change of mind —
   write a new one that supersedes it. The record of having been wrong is valuable.
3. **Numbered sequentially**, never reused: `ADR-NNNN-short-kebab-title.md`.
4. **Written before the standard changes**, not after. The ADR is the decision; the edited
   standard is its consequence.
5. **Options must be real.** Listing a straw man to justify a foregone conclusion makes the
   record worthless to the next reader.

## When to write one

- Any architectural choice with a plausible alternative
- Any change to a standard in `docs/`
- Any resolved escalation (see [human-intervention](../human-intervention.md))
- Any deliberate deviation from a standard
- Any business rule that constrains the design

Not for: routine implementation choices, obvious bug fixes, or anything with no defensible
alternative.

## Status values

| Status | Meaning |
| --- | --- |
| **Proposed** | Written, awaiting a human decision. Not yet binding. |
| **Accepted** | Decided. Binding. |
| **Superseded by ADR-NNNN** | Replaced. Kept for the historical record. |
| **Rejected** | Considered and declined. Kept so it is not re-proposed. |

## Index — the starter set

| ADR | Title | Status |
| --- | --- | --- |
| [0001](ADR-0001-shared-contracts-package.md) | Shared contracts package for client DTOs | **Proposed** — the adopter decides |
| [0002](ADR-0002-response-mapper-decomposition.md) | Response mappers must be decomposed into builders | Accepted |
| [0003](ADR-0003-canonical-feature-vocabulary.md) | Canonical feature vocabulary | Accepted |
| [0004](ADR-0004-fixture-based-testing.md) | Fixture-based testing as the default | Accepted |
| [0005](ADR-0005-no-new-service-by-copy.md) | New services are never created by copying an existing one | Accepted |
| [0006](ADR-0006-file-size-limit.md) | 400-line file limit | Accepted |
| [0007](ADR-0007-separate-engineering-system-repo.md) | The engineering system lives in its own repository | Accepted |

**Open on adoption:** ADR-0001 (shared contracts package) ships **Proposed**, not Accepted. It
depends on facts only the adopting organisation has — whether an internal package feed exists,
and who owns the contract — so it is the one starter decision that must be made rather than
inherited.

**Numbering:** 0001–0007 are reserved for the starter set and are never reused. The adopter's
first own decision is **ADR-0008**.

Keep this index in step with each ADR's own `Status:` line. It drifted once already in the
source estate, within hours of being written, which is exactly the failure mode this repository
exists to prevent.
