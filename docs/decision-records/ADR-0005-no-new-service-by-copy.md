# ADR-0005 — New services are never created by copying an existing one

**Status:** Accepted · **Date:** 2026-09-05

## Context

In an estate of 17 supplier services we analysed, every duplication finding traced to one
habit: creating a new supplier service by copying the last one.

The evidence is unambiguous:

- `Loging` (a misspelling of `Logging`) in 16 of the 17 services
- `General.cs` in 16 near-identical copies
- one mapper file present in 3 services — a 22-line diff on 827 lines, purely renames
- test suites copy-pasted verbatim between two unrelated supplier services
- scaffold leftovers from two *other* suppliers still sitting in a third supplier's code
- project folder names frozen with the same typo in two services: `…-Air-Servcie`

Copying transmits the previous supplier's *accidents* along with its structure, and it
transmits them faster than anyone removes them.

## Decision

New supplier services are created from a **template**, never by copying a working service.

The template contains structure and no supplier logic: `Program.cs`, extensions, middleware,
DI skeleton, logging, config binding, a test project with the fixture harness, `.gitignore`,
and the canonical feature folder layout.

Studying an existing service to learn a *pattern* is expected and encouraged — see the skill's
Phase 1. Copying its *files* is not.

## Consequences

- **Positive:** no inherited typos, no dead scaffold, no divergent utility copies.
- **Negative:** the template must be built and maintained; without it this ADR is
  unenforceable.
- **Dependency:** the template is the adopter's to build, against its own stack and hosting
  conventions. Building it is the first task of the first supplier integration, and that first
  integration is what proves it. Until the template exists, this ADR states an intent rather
  than a rule.
