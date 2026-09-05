# The client contract

**This folder is an input to every supplier service. It is not a design space.**

The client contract is what the aggregator sends to a supplier service and what it expects
back. It is identical across every supplier — that symmetry is the entire reason the aggregator
can fan out to many services and treat the results uniformly.

## The rule

> **A supplier service may not change the client contract. Ever. Without exception.**

Not to add a field. Not to rename one. Not to "just widen a type". If a supplier returns
something the contract has no home for, that is an escalation — see
[`docs/permission-boundaries.md`](../docs/permission-boundaries.md).

## Why this is absolute

In an estate of 17 supplier services we analysed, the contract existed as 17 independent copies
and **all 17 differed** — property counts ranged from 4 to 23, and no two files were identical.
Optional-looking extras appeared in a handful of copies each; several fields existed in exactly
one service.

Nobody decided that. Each service was allowed to shape the contract slightly, each change was
locally reasonable, and the aggregate was a contract with no single definition. The aggregator
then has to be defensive per supplier — which defeats the point of having a contract at all.

This folder exists so that the next supplier added does not become the next variant.

## What is in the contract

Your aggregator defines it — four objects: the **request envelope**, the **credentials object**,
the **response envelope**, and the **result payload**.

Do not restate them here from memory or from another organisation's example. Extract them from
your own aggregator and record them, method and traps included:
[`client-contract.md`](client-contract.md).

Two things that folder will not let you skip, because both are silent failures:

- **A duplicated contract drifts.** This folder is a *record* that points at code, never a
  second definition someone edits instead of the code.
- **A status field's type matters.** Copy it exactly as declared; never infer it from sample
  values. A mismatch does not throw a clear error — it deserialises to nothing.

## How a supplier service uses it

1. **At bootstrap**, the client DTOs are generated into
   `Features/<Feature>/DTOs/Client/` from this contract. They are not authored.
2. **During a feature**, the request builder reads *from* the client request and the mapper
   writes *into* the client response. Both treat the client side as fixed.
3. **They are never edited** to fit a supplier. Only the supplier DTOs move.

## If the contract really is insufficient

It sometimes genuinely is. The process:

1. Try to map into what exists — show your working.
2. Consider whether the data is actually needed. Record what was dropped in
   `knowledge/suppliers/<supplier>.md`.
3. If neither works, escalate with all three options assessed.

A human decides. The decision becomes an ADR. Then the contract changes **here first**, and
propagates — it never changes in one supplier service and leaks outward.

## Keeping this in step with reality

This folder documents the contract as observed in the aggregator on the date recorded in your
captured contract. It is a *record*, not the runtime source of truth — the code is.

**Target state:** a shared package so there is exactly one definition and this folder becomes
its documentation. See
[ADR-0001](../docs/decision-records/ADR-0001-shared-contracts-package.md), still open.

Until then: **re-verify against the aggregator before relying on a detail here**, and update
this folder when the aggregator's contract changes.
