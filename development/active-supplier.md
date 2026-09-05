# Active supplier

The single supplier currently being worked on. **One at a time.** When work switches, this file
is rewritten from the template below and the previous entry is summarised into
[`progress.md`](progress.md).

---

## Current: *none*

No supplier is in progress.

To start one: copy the template below over this section, fill it in, and create the supplier's
knowledge file from [`../knowledge/suppliers/_TEMPLATE.md`](../knowledge/suppliers/_TEMPLATE.md)
in the same commit.

---

## Template for the next entry

````markdown
## Current: <Supplier>

**Started:** YYYY-MM-DD · **Phase:** 1 understand | 2 research | 3 design | 4 implement | 5 verify | 6 close
**Repository:** <supplier-service-root>/<Supplier>-Air-Service
**Knowledge:** knowledge/suppliers/<supplier>.md
**Decision:** docs/decision-records/ADR-XXXX-<slug>.md (if this supplier was chosen by a recorded decision)

### Scope

Which features, in which order, and what is explicitly **out** of scope until a stated
condition is met. Name the condition — a contract, a credential, a schema — not "later".

### Progress

- [ ] Phase 1 — understand
- [ ] Phase 2 — research
- [ ] Phase 3 — design
- [ ] Phase 4 — implement
- [ ] Phase 5 — verify against the real API
- [ ] Phase 6 — close

**Verified:** <build and test result, or "not yet run">

| Piece | State |
| --- | --- |
| Repository, composition root, DI, middleware, config | |
| Shared envelope, constants, options, token cache | |
| Auth | |
| <Feature> — validator, model builder, decomposed mapper | |
| Everything else | |

State each piece as what has been *observed*, not what has been typed. "Written, never run
against the live API" and "scaffold; supplier DTOs are placeholders" are the honest states and
must be said out loud — a reader who mistakes either for "done" will build on sand.

### Open escalations

One row per blocker. The full escalation — Problem, Evidence, Impact, Possible options,
Recommended option, Decision required — is written in the format defined in
`docs/human-intervention.md` and pasted underneath this table.

| Question | Raised | Status |
| --- | --- | --- |

### Not attempted, deliberately

Actions that were within technical reach but outside the agent's authority — registering an
account, entering a password or payment detail, accepting a service agreement, or anything else
requiring a legal identity or a device only the account holder holds. See `../AGENTS.md` and
`../docs/permission-boundaries.md`.

### Assumptions in force

Anything being built on that is not yet confirmed. Each needs an owner and a resolution date.
Mark the load-bearing one and say what it costs if it is wrong — that is the one to check first
once the blocker clears.

| # | Assumption | Risk if wrong |
| --- | --- | --- |

### Next action

One concrete thing.
````
