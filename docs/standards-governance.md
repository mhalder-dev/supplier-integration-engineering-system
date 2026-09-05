# Standards governance

**Status:** active · **Governs:** every document in `docs/`.

## Why this exists

A standard that changes whenever someone finds it inconvenient is not a standard. A standard
that never changes becomes wrong and gets ignored. This document is the middle path: change
is expected, but it is deliberate, recorded, and applied consistently.

The existing ecosystem shows what unmanaged change produces. Seventeen services, each
copy-pasted from a predecessor and then locally "improved", have produced:

- seventeen divergent copies of the client contract DTO
- sixteen near-identical copies of one utility class
- two spellings of `Revalidate`, two of `FareRule`, three names for ticketing
- one misspelling, `Loging`, propagated into sixteen of seventeen services

Nobody decided any of that. Each step was a small local improvement, and the aggregate is
incoherence. Silent drift is the failure mode this document prevents.

## The rule

> **Never change a standard silently.** Propose, decide, record, update, apply.

This applies to an agent mid-implementation *and* to a human who thinks the rule is obviously
wrong. "Obviously wrong" is a proposal, not a licence.

## The process

### 1. Identify
Name the specific rule and where it failed you. "The 400-line limit forces an awkward split
in generated SOAP DTOs" is actionable. "The standards are too rigid" is not.

### 2. Explain
Why is the current rule insufficient *here*? Distinguish two cases, because they have
different remedies:

- **The rule is wrong** → change the rule.
- **The rule is right but this case is exceptional** → document an exception; keep the rule.

Most proposals are the second. Reach for a rule change only when the exception would recur
across suppliers.

### 3. Propose
Use the escalation format from [human-intervention](human-intervention.md). Include:

- the current rule, quoted
- the proposed rule
- what breaks if adopted — specifically, which existing services would now be non-compliant
- migration: does existing code change, or does the rule apply only to new work?

### 4. Decide
A human decides. Not the agent, not the implementer alone. The decision is explicit —
silence is not approval.

### 5. Record
An ADR in [`decision-records/`](decision-records/), **before** the standard is edited. The
ADR is the primary artefact; the edited standard is its consequence. An ADR is immutable
once accepted — supersede it with a new one rather than rewriting history.

### 6. Update
Edit the standard. Reference the ADR from the changed section so a future reader finds the
reasoning without archaeology.

### 7. Apply
Consistently, going forward. Decide explicitly whether existing code is retrofitted:

- **New work only** — the default. Cheap, and the old code still works.
- **Retrofit on touch** — when a file is edited for another reason, bring it up to standard.
- **Retrofit now** — only when the old pattern is actively harmful (a security issue, a
  correctness bug).

Record which of the three was chosen. Ambiguity here is how half-migrations happen, and a
half-migrated codebase is worse than either end state.

## What does not require this process

- Fixing a typo or broken link in a document.
- Adding a supplier fact to `knowledge/suppliers/`.
- Adding a lesson to `knowledge/lessons-learned.md`.
- Updating `development/progress.md`.
- Adding an example that illustrates an existing rule without changing it.

Knowledge accumulates freely. **Rules change deliberately.** That asymmetry is intentional:
knowledge is descriptive and self-correcting, rules are prescriptive and coordinate people.

## Standard maturity

Each standard carries a status:

| Status | Meaning |
| --- | --- |
| **active** | Binding. Deviations need an ADR. |
| **provisional** | Believed right, not yet proven across suppliers. Deviations need a note, not an ADR. |
| **deprecated** | Superseded. The replacement is named. |

Rules derived from a single service's practice start **provisional** and become **active**
once a second supplier has been built under them. This system's own standards are currently
a mix — the proof-of-concept supplier is what promotes them.

## Reviewing this system

After each supplier is built under these standards, review:

- Which rules were followed without friction? (Keep.)
- Which were worked around? (They are wrong or unclear.)
- Which questions came up that no document answered? (A gap.)
- Which escalations were avoidable with better documentation? (A gap.)
- How much time went to work the system should have automated? (A tooling gap.)

Record the review in `knowledge/lessons-learned.md` and raise proposals from it. A system
that is never reviewed against reality becomes ceremony.
