# Readiness

**Last audited:** 2026-09-06

What a developer can rely on, and what they cannot. Written honestly because a system that
overstates its own completeness is worse than one with no documentation — the reader trusts it
and gets burned.

**Short answer: usable, not finished.** A developer can start a REST/JSON supplier today and
will be well supported. A SOAP supplier, or anything past Search, is thinner. **No supplier has
been taken end-to-end against a live API using this system** — read the *Not proven* section
before you plan around it.

## Proven

Verified by execution, not assertion. Each row was re-run at the audit date above.

| | Evidence |
| --- | --- |
| The template builds | `dotnet build` → 0 errors, 0 warnings, `TreatWarningsAsErrors=true` |
| The template's tests pass | `dotnet test` → **20 passed, 0 failed, 0 skipped** |
| A service can be created from it | One supplier service scaffolded from the template — 0 errors, 0 warnings, 20 tests passing. Scaffold and auth only; **never run against a live API** |
| The 400-line limit is achievable | 33 C# files; largest: **121 lines** |
| The mapper decomposition works | Search mapper: 5 builders behind one interface, largest **44 lines**, each independently tested |
| Fixture-based testing works | 10 mapper tests run offline with no network and no credentials |

## Not proven

**No supplier has been built end-to-end against a live API using this system.** Everything
below is designed and reasoned but unvalidated by contact with reality:

| Claim | Status |
| --- | --- |
| The workflow survives a real integration | **Untested.** The one attempt reached Phase 4 and stopped: supplier sandbox credentials needed a legal identity and a human-held OTP device. |
| Fixture capture and redaction is practical | **Untested.** Both committed fixtures are synthetic. |
| The escalation loop works | Raised once; **no human has answered one yet.** |
| The standards hold under delivery pressure | Untested — that is when standards actually fail. |

The credential wall is not an accident of one supplier. Airline and GDS sandboxes routinely
require a signed agreement under a legal identity and a device-bound second factor, so **plan
for a human to obtain credentials before Phase 4**, not during it.

## Gaps — what is genuinely missing

Ranked by how likely they are to block a real developer.

### 1. No write-path example (highest risk)
The template demonstrates **Search only** — a read path.

The write-path rules are the **highest-risk rules in the system**: never retry `IssueTicket`,
never retry `Refund`, retrieve-and-check after an ambiguous timeout. They are documented in
`docs/supplier-patterns.md §2` but have **no worked example in code**.

A developer implementing Booking or IssueTicket has prose, not a pattern to copy. That is
exactly the situation in which people improvise.

### 2. REST/JSON only
The template has no SOAP scaffold — no `XmlWriter` request building, no envelope handling, no
canonicalised-XML test helper. Roughly half of real airline and GDS APIs are SOAP. Those
developers get the architecture but no starting point.

### 3. `ILogService` is a stub
`SupplierServiceBase.LogAsync` writes to `ILogger` instead of a real queue → background worker
→ blob pipeline. Every service must currently build its own — which is precisely the
duplication [ADR-0001](../docs/decision-records/ADR-0001-shared-contracts-package.md) exists to
stop.

### 4. `IReferenceData` is absent
Airport/airline/aircraft lookup with timezone resolution. The most-duplicated component in the
estate of 17 supplier services we analysed, and the template does not supply one. A developer
will copy an existing service's — the exact habit
[ADR-0005](../docs/decision-records/ADR-0005-no-new-service-by-copy.md) bans.

### 5. The shared contracts package does not exist
ADR-0001 is still **Proposed**. Until it is decided, every new service defines its own
`FlightResultDto` and the drift continues — in the estate we analysed that DTO existed in 17
copies with 17 different hashes and property counts ranging from 4 to 23. The template's copy
is marked with a warning comment, which is a mitigation, not a fix.

### 6. `tests/schemas/` and `tests/harness/` are promised but empty
[`tests/README.md`](../tests/README.md) describes a contract-test JSON schema and shared harness
helpers. Neither exists. The schema is blocked on deciding which of the divergent DTO variants
is canonical — an aggregator-team decision, not one to take unilaterally.

## Coverage by supplier shape

| Shape | Standards | Template | Worked example |
| --- | :-: | :-: | :-: |
| REST/JSON, token auth | ✅ | ✅ | ✅ Search |
| REST/JSON, write path | ✅ | ⚠️ structure only | ❌ |
| SOAP/XML | ✅ | ❌ | ❌ |
| SOAP + mTLS | ✅ | ❌ | ❌ |
| NDC / reference-based | ✅ | ⚠️ pattern documented | ❌ |
| Session-based (GDS) | ⚠️ mentioned | ❌ | ❌ |

## Can a new developer actually start?

Walked through as a fresh developer would, on a clean adoption of this repository:

| Step | Works? |
| --- | :-: |
| Understand what this repo is → `README.md` | ✅ |
| Find the standards → `docs/` | ✅ |
| Record the installation's paths and ports → copy [`config/environment.example.md`](../config/environment.example.md) to `config/environment.md` | ✅ template shipped, adopter fills it in |
| Know which reference to study → `reference/bootstrap-reference/` | ✅ |
| Follow the workflow → `.claude/skills/supplier-development/SKILL.md` | ✅ |
| Scaffold a new service → `reference/bootstrap-reference/` | ✅ REST only |
| Write and run tests → template harness | ✅ |
| Handle an unknown → [`docs/human-intervention.md`](../docs/human-intervention.md) | ✅ |
| Record a decision → [`docs/decision-records/ADR-template.md`](../docs/decision-records/ADR-template.md) | ✅ |
| Implement a **SOAP** supplier | ⚠️ standards only, no scaffold |
| Implement **Booking / IssueTicket** | ⚠️ standards only, no example |
| See a completed supplier end-to-end | ❌ none exists |

## What would close the gaps

In priority order:

1. **A write-path slice in the template** (Booking + IssueTicket), demonstrating the no-retry
   rule and the retrieve-and-check-after-timeout pattern. Highest value; the rules it
   demonstrates carry real money.
2. **A SOAP variant of the template** — roughly half of suppliers need it.
3. **Decide ADR-0001**, then extract `ILogService`, `IReferenceData`, and the client contracts
   into a shared package. Closes gaps 3, 4, 5, and 6 together.
4. **Take one supplier end-to-end against a live API** — the only thing that converts
   "designed" into "proven", and the only way the workflow, the fixture-capture rules and the
   escalation loop get tested.

## Maintaining this file

Re-audit after each supplier is built, and before any release of this system. The checks are
mechanical:

```bash
# 1. the template still builds and its tests still pass
dotnet test reference/bootstrap-reference/tests/*/*.csproj --nologo

# 2. no broken relative links in any .md
# 3. ADR statuses quoted here still match each ADR's own Status: line
# 4. portability: no organisation, supplier, host, credential or absolute path
#    has leaked into docs/, contracts/, .claude/, install/ or reference/ —
#    config/environment.md is the only file permitted to name an installation
```

This file drifting out of date would be the same failure it was created to catch.
