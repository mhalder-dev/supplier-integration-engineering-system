# ADR-0001 — Shared contracts package for client DTOs

**Status:** Proposed — the adopting organisation decides
**Date:** 2026-09-05
**Deciders:** _pending — this is the one starter decision that cannot be inherited_

## Context

`FlightResultDto` is the contract the aggregator consumes from every supplier service. In the
estate of 17 supplier services we analysed it was defined **independently in all 17**, and
every copy differed:

- all 17 copies had different MD5 hashes
- property counts ranged from 4 to 23
- `SupplierResponseJson` existed in 7 of 15 comparable copies
- `IsGetBrandedFareCall` in 6, `FareTag` in 3
- `Headers` in exactly 1 service
- `IsPassportRequired`, `IsEmergencyContactRequired`, `IsSeatSelectionRequired` in exactly 1
  service

The same applied to `Utilities/General.cs`, which existed as 16 near-identical copies at
436–456 lines.

Consequences of that arrangement:

1. The aggregator receives a structurally different payload from each supplier and must be
   defensive per supplier.
2. A contract change means 17 coordinated edits, so in practice it is made in one or two
   services and the rest silently diverge further.
3. A bug fixed in one copy of `General.cs` is fixed in exactly one service.
4. There is no mechanical way to tell whether a supplier's output is contract-compliant.

The estate had **no shared library at all** — zero cross-service project references.

Any organisation running more than a couple of supplier services will reach the same fork, so
the decision ships here with the options already worked out.

## Decision drivers

- Contract integrity across supplier boundaries
- Cost of change (17 repos versus one package)
- Deployment coupling — a shared package means coordinated releases
- Team autonomy — one supplier team should not be blocked by another

## Options

### 1. Do nothing
Keep 17 copies. **Cost:** zero now, and the drift continues to compound. Every new supplier
adds an eighteenth variant. **Rejected** — this is the status quo that produced the problem.

### 2. Shared NuGet package for client contracts only
`<Org>.Supplier.Contracts` containing `FlightResultDto` and its dependent types.
Versioned, semver, published to an internal feed.
**Pro:** one definition; contract changes are visible and reviewable; consumers upgrade
deliberately. **Con:** requires an internal NuGet feed and a release process; a breaking
change needs coordination.

### 3. Shared package for contracts **and** utilities
As 2, plus `General.cs`, reference data, and common helpers.
**Pro:** kills the largest duplication as well. **Con:** a much wider blast radius; utilities
change far more often than contracts, so services would be forced to upgrade constantly.

### 4. Git submodule / shared source
Link a shared source folder into each service.
**Pro:** no package infrastructure. **Con:** submodules are a well-known operational
nuisance, and it does not solve versioning — it just moves the drift to "which commit is
this service pinned to".

### 5. Contract tests without a shared package
Keep the copies, add a committed JSON schema every service asserts against.
**Pro:** no coupling; catches drift immediately; can be adopted per service today.
**Con:** does not remove the duplication, only detects divergence.

## Recommendation

**Option 2 now, option 5 immediately, option 3 deferred.**

- **Option 5 first** because it is cheap, needs no infrastructure, and can be adopted by one
  service this week. It converts an invisible problem into a failing test.
- **Option 2 next** because contracts are the highest-value, lowest-churn thing to share.
  Contracts change rarely, so forced upgrades are infrequent.
- **Option 3 deferred** because utilities churn; coupling every supplier service to a
  fast-moving shared utility package trades one problem for another. Revisit once option 2 has
  proven the release process.

Existing services are **not** retrofitted as part of this decision. New suppliers consume the
package; existing ones migrate when touched. See
[standards-governance §7](../standards-governance.md).

## Decision required from the adopter

1. Do you create `<Org>.Supplier.Contracts`?
2. Is there an internal NuGet feed, or does one need standing up?
3. Who owns the contract — the aggregator team or the supplier teams?
4. Do you adopt the JSON-schema contract test now, independently of 1–3?

Question 4 has no dependencies and no infrastructure cost. Answering it "yes" on day one is
the recommendation above, whatever is decided about 1–3.

## Consequences if accepted

- **Positive:** one contract definition; drift becomes a build failure; new suppliers cannot
  invent an eighteenth variant.
- **Negative:** release coordination for breaking changes; internal feed to maintain.
- **Risk:** the package becomes a dumping ground. Mitigate by keeping it to contracts only,
  which is why option 3 is deferred rather than adopted.

## Consequences if rejected

Drift continues and compounds. This ADR stays as the record of the choice, and the finding
above stands unaddressed.
