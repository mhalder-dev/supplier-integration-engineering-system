---
name: supplier-development
description: Entry point for any supplier integration work. Routes to the right sub-skill - bootstrap a new supplier project, implement one feature end-to-end, or wire a feature into the aggregator - and carries the rules that apply across all three. Use when the task names a supplier or a supplier feature (e.g. "develop a new supplier", "add refund to an existing supplier", "wire a supplier's search into the aggregator"). Enforces the fixed client contract, escalation of unknowns, branch and PR rules, and continuous knowledge capture.
---

# Supplier development

Supplier integration runs in three stages. This skill decides which one applies and hands off.
It also carries the rules common to all three — read those even when going straight to a
sub-skill.

## Route

| The task is… | Go to |
| --- | --- |
| "Create / start / set up the X supplier" — no project yet | [`supplier-bootstrap`](../supplier-bootstrap/SKILL.md) |
| "Add / implement `<feature>` for X" — project exists | [`supplier-feature`](../supplier-feature/SKILL.md) |
| "Wire X into the aggregator" — supplier feature done | [`aggregator-wiring`](../aggregator-wiring/SKILL.md) |
| "Develop the X supplier" — from nothing, all of it | bootstrap → feature → wiring, **one feature at a time** |

"Develop supplier X" end to end is:

```
bootstrap  →  Auth  →  Search  →  wire Search  →  Revalidate  →  wire  →  Booking  →  …
                 └── each feature: implement, test, run, verify, PR ──┘
```

**Never run features in parallel.** Each teaches you something about the supplier the next one
needs, and a failure is unattributable when three are in flight.

## Rules that apply to every stage

### 1. The client contract is fixed

The aggregator owns it; every supplier shares it. **A supplier service may not change it** — not
a new field, not a rename, not a widened type.

If a supplier returns something with no home, assess three options — map into what exists, drop
it (recording what was dropped), or change the contract — and **escalate**. A human decides, and
the decision becomes an ADR before any code changes.

[`contracts/README.md`](../../../contracts/README.md) ·
[`docs/permission-boundaries.md`](../../../docs/permission-boundaries.md)

### 2. The aggregator takes wiring only

You may add a supplier to lists that already exist. You may **not** add business logic,
abstractions, endpoints, or client DTO fields. A supplier that does not fit is a finding about
the supplier — not a licence to change shared code.

### 3. Unknowns escalate, never get invented

Missing or contradictory documentation, ambiguous behaviour, absent credentials, unclear business
rules, several valid designs — all stop work and raise a structured escalation.

**Never invent supplier behaviour to keep momentum.** An integration built on a guess fails in
production against real money.
[`docs/human-intervention.md`](../../../docs/human-intervention.md)

### 4. Knowledge is persisted as it is learned

The moment you learn a quirk, a spec-versus-production difference, or an error code's real
meaning, write it to `knowledge/suppliers/<supplier>.md` — **now, not at the end**. Knowledge
deferred to the end of a task is lost when the task is interrupted.

Every supplier claim carries a source tag: `[spec §x]`, `[observed in <fixture>]`,
`[prod-verified]`, `[assumed]`. An untagged claim is a defect.

### 5. Branches and PRs

| Repository | Branch |
| --- | --- |
| Supplier service | `feature/<feature-name>` |
| Aggregator | `feature/<supplier-name>/<feature-name>` |

Always branch from freshly-pulled `staging`. PR to `staging`. The supplier and aggregator PRs
**pair and merge together** — supplier first.
[`docs/branching-and-pr.md`](../../../docs/branching-and-pr.md)

### 6. Standards change by decision, not by drift

Found a better convention mid-implementation? **Do not apply it silently.** Propose → human
decides → ADR → standard updated → applied consistently.
[`docs/standards-governance.md`](../../../docs/standards-governance.md)

### 7. Done means observed, not compiled

Tests written **and executed**, output shown. The real flow driven against the sandbox and seen
to work. Services stopped afterwards.

If a test failed, lead with that. If a step was skipped, say so. **Never report a success you did
not observe.**

## Reading order

1. [`CLAUDE.md`](../../../CLAUDE.md) — operating rules
2. [`contracts/client-contract.md`](../../../contracts/client-contract.md) — **what is fixed**
3. [`docs/architecture.md`](../../../docs/architecture.md) — service shape
4. [`docs/supplier-patterns.md`](../../../docs/supplier-patterns.md) — feature shapes
5. [`docs/coding-standards.md`](../../../docs/coding-standards.md) — code rules
6. [`docs/permission-boundaries.md`](../../../docs/permission-boundaries.md) — what needs approval
7. [`knowledge/lessons-learned.md`](../../../knowledge/lessons-learned.md) — what already bit us
8. `knowledge/suppliers/<supplier>.md` — this supplier

Paths for this installation live in `config/environment.md`. The system ships with
[`config/environment.example.md`](../../../config/environment.example.md) — on install, copy it
to `config/environment.md` and fill in the real paths.

## Where things live

- **Existing supplier services** — read-only reference. Study the patterns; do not copy files,
  and do not modify them unless the task says to.
- **A new supplier** — its own repository.
- **Engineering-system artefacts** (standards, skills, knowledge) — here, never inside a supplier
  service tree.

## Starting from nothing

1. Confirm supplier name, short code, protocol. **Get the spelling confirmed** — namespaces are
   permanent.
2. Research the API and sandbox access. Escalate if credentials are unavailable.
3. `supplier-bootstrap` — the base, verified building and running.
4. `supplier-feature` for **Auth** — everything depends on it.
5. `supplier-feature` for **Search** — heaviest mapping, surfaces most quirks early.
6. `aggregator-wiring` for Search. Paired PRs.
7. Repeat 5–6 per feature: Revalidate → Booking → IssueTicket → the rest.
8. Keep `development/active-supplier.md` and `development/progress.md` current throughout.

## Hard limits

- Never fabricate credentials, commit them, or use production credentials.
- Never bypass authentication or reverse-engineer a restricted system.
- Never invent supplier behaviour.
- Never change the client contract without an approving ADR.
- Never add logic to the aggregator.
- Never silently change a standard.
- Never report a success you did not observe.
