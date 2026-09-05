# Permission boundaries

**Status:** active · **Applies to:** every agent and developer building a supplier integration.

The system is allowed to write a lot of code without asking. It is **not** allowed to change the
shape of the aggregator or the contract the aggregator owns. This document draws that line
precisely, because "use judgement" is not a boundary.

## The principle

> **The client contract is an input, not a design space.**

A supplier service exists to translate one supplier's API into a contract that already exists.
The contract is owned by the aggregator and consumed by every other supplier service. Changing
it to suit one supplier breaks that symmetry — and it is how a shared DTO ends up existing in
seventeen divergent copies (lesson L-02).

If a supplier returns something the contract has no home for, **that is a conversation, not a
commit.**

## Green — proceed without asking

### In the supplier service (its own repository)

| | |
| --- | --- |
| Everything under `Features/<Feature>/DTOs/Supplier/` | The supplier's wire shape is yours to model |
| Request builders, response mappers, mapper builders | The translation layer is the whole job |
| Validators | |
| Feature services, interfaces, DI registration | |
| Constants, protocol enums, code-list resolvers | |
| Extensions, middleware, `Program.cs`, `appsettings.*` | |
| gRPC service + `.proto` for this supplier | Mirroring the established envelope |
| Tests and fixtures | |
| That service's own `CLAUDE.md` and docs | |

### In the aggregator

Only the **wiring surface** — the code that makes an existing capability reach a new supplier:

| Wiring point | Typical location | Change permitted |
| --- | --- | --- |
| Supplier code registry | `<aggregator-root>/<supplier-code-enum>` | Add the supplier's code constant |
| Transport contract for this supplier | `<aggregator-root>/<protos>/<supplier>_<feature>.proto` | Add, mirroring the existing envelope exactly |
| Transport client registration | `<aggregator-root>/<supplier-transport-extensions>` | Register the supplier's client |
| Configuration mapping | `<aggregator-root>/<supplier-configuration-extensions>` | Add the env-var mapping |
| Service registration | `<aggregator-root>/<service-registration-extensions>` | Register the supplier's wiring service |
| Per-supplier wiring service | `<aggregator-root>/<feature>/Suppliers/<Supplier><Feature>Service` | Add, subclassing the existing base |
| Per-supplier request model | `<aggregator-root>/<feature>/<Supplier>ServiceRequest` | Add, mirroring the existing per-supplier shape |
| Environment configuration | `<aggregator-root>/<app-settings>` | Add this supplier's settings block |

The placeholders are deliberate: what matters is the **role**, not the filename. Record your own
aggregator's actual paths once, in your `config/environment.md` (written from
[`config/environment.example.md`](../config/environment.example.md)), and the list above stays
true whatever they are called.

**The test for "is this wiring?"** — you are adding a supplier to a list that already exists,
copying a shape that is already there. If you are creating a *new kind of thing*, it is not
wiring.

### ⚠ Check what a shared config file already contains before treating it as safe to edit

Green permission is permission to **add**. It is never permission to **disclose**. A configuration
file can be entirely safe for you to append a block to *and* full of other people's secrets at the
same time, and the permission model above reasons only about what you add.

`[Certain]` In one estate we analysed, the aggregator's git-tracked environment configuration held
a database connection string with credentials, a token-signing key, an object-storage account key
and a 185-character signed URL, and **five suppliers' passwords** — across five tracked variants
of the file.

So, on any shared config file:

- **Open it and read it before you edit it.** Establish whether it holds live credentials first,
  not after.
- Never paste the file, or a diff of it, into a PR description, a chat, or a ticket.
- Never copy it to a scratch file, a fixture, or anywhere outside its repository.
- Never add a credential of your own to it — supplier credentials arrive per request.

**Before opening a PR, look at the actual diff hunk, not just your intent.** A diff carries
surrounding context lines, so a small, correct addition can expose an unrelated secret sitting
three lines above it.

Where a shared file is known to hold live credentials, say so at the point where permission is
granted — not in a general security section nobody reads at the moment they need it. See
[L-14](../knowledge/lessons-learned.md).

## Red — stop and ask, every time

### Anything that changes the client contract

- Adding a field to a client-side DTO
- Removing, renaming, or retyping an existing field
- Changing the meaning of a field
- Adding a new client-facing DTO
- Changing an endpoint's request or response shape

**Even if the supplier genuinely returns data with nowhere to go.** Especially then — that is
the case where the contract may actually need to change, which is exactly why a human decides.

### Anything that adds behaviour to the aggregator

- Business logic of any kind — pricing, markup, filtering, ranking, penalty rules
- A new abstraction, base class, or interface
- Changing how existing suppliers behave
- A new endpoint
- Changing the fan-out, aggregation, or dispatch logic
- Anything in the aggregator's shared or common code, whatever it is called locally

**The aggregator is not where supplier problems get solved.** If a supplier's response cannot be
made to fit by mapping alone, that is a finding to escalate, not a special case to add upstream.

### Anything structural

- A new NuGet dependency
- A change to a standard in `docs/` (see [standards-governance](standards-governance.md))
- A new shared/common component
- Changing the branch or PR workflow

## The grey area, resolved

**"The current DTO is not enough for my supplier."**

This is the situation the boundary exists for, and the honest answer is that it has three
possible resolutions — and you do not get to pick:

1. **Map into what exists.** Usually possible. Try this first and show your working.
2. **The data is genuinely not needed.** Drop it, and record what was dropped in
   `knowledge/suppliers/<supplier>.md` so nobody later assumes it was never available.
3. **The contract must change.** A real outcome, and a human decision, because it affects every
   supplier and the aggregator.

Escalate with all three assessed. See [human-intervention](human-intervention.md).

**Never** resolve it by quietly adding a field, stuffing data into an unrelated one, or
serialising a blob into a string field. Those look like progress and are not.

## How to escalate a contract change

Use the standard format, with these specifics filled in:

```
Problem:
  <Supplier> returns <data> which the client contract has no field for.

Evidence:
  - Where it appears in the supplier's response [observed in <fixture>]
  - What it means, per their documentation [spec section]
  - Why options 1 and 2 above do not work — specifically

Impact:
  What is lost if we drop it. Who else would benefit if the contract gained it.

Possible options:
  1. Map into <existing field> - what is lost
  2. Drop it - what is lost
  3. Add <field> to the contract - who must change, and what it costs

Recommended option:
  With reasoning.

Decision required:
  Do we add <field> to the client contract?
```

Once decided, record it as an ADR **before** any code changes.

## Why this is strict

In an estate of 17 supplier services we analysed, every one of them was allowed to shape the
contract a little, and the result is seventeen copies of one DTO with property counts from 4 to
23 and no two identical. Nobody decided that. Each step was a small, locally reasonable change.

The boundary is strict because the failure is gradual.
