# ADR-0007 — The engineering system lives in its own repository

**Status:** Accepted · **Date:** 2026-09-05

## Context

When engineering knowledge lives inside the service repositories, it distributes itself
unevenly and then stops moving. In an estate of 17 supplier services we analysed:

- one service held 50 documents, 8 ADRs, a per-feature handbook and a feature-authoring
  playbook
- another held 56 documents
- the service with the **best test suite in the estate** held **0** documents
- a fourth held **0** documents
- only 2 of the 17 services carried an agent-instruction file at all

The best material in the estate — that feature-authoring playbook — was effectively an
engineering standard already. But it was written in one supplier's vocabulary (that supplier's
protocol dialect, SOAP, its own client abstraction) and lived inside that supplier's
repository, so no other service could adopt it.

## Options

1. **Per-service docs only** — status quo. Knowledge stays trapped; new services start blank.
2. **Docs in the aggregator repo** — couples standards to a deployable service and mixes two
   audiences.
3. **A dedicated engineering-system repository** — standards, workflow, knowledge, and
   decisions in one place, referenced by every service.

## Decision

Option 3. A dedicated repository at `<workspace-root>/supplier-integration-engineering-system`
— the repository you are reading. An adopter inherits this decision by cloning it, and should
keep it separate from the supplier services rather than folding it into one of them.

Division of responsibility:

| Lives here | Lives in the service repo |
| --- | --- |
| Standards that apply to every supplier | Supplier-specific implementation notes |
| The development workflow (skill) | That service's agent-instruction file, pointing here |
| Cross-supplier knowledge and assessment | Feature handbook for that service |
| Decisions affecting the standard | Decisions affecting only that service |
| Lessons learned across suppliers | — |

The supplier services under `<supplier-service-root>/` are **read-only reference** for this
repository. Engineering-system artefacts are never created inside that tree.

## Consequences

- **Positive:** one source of truth; new suppliers start from accumulated knowledge rather
  than a blank folder; standards are versioned and reviewable.
- **Negative:** two repositories to keep in sync; a standard can drift from what services
  actually do.
- **Mitigation:** each service's agent-instruction file points here rather than restating the
  rules. Restating them creates a second copy that silently goes stale — a failure one of the
  analysed services had already recorded in an ADR of its own.
