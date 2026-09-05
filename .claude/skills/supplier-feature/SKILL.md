---
name: supplier-feature
description: Implement ONE feature end-to-end in an existing supplier service - supplier DTOs, validator, request builder, orchestrating service with logging and error handling, decomposed response mapper, endpoint, DI, tests, and a live verification run. Use when asked to add or implement a named feature for an existing supplier (e.g. "add search to the supplier", "implement booking for this supplier", "add refund to an existing supplier", "now do issue ticket"). Assumes the project already exists - use supplier-bootstrap first if it does not. Treats the client contract as fixed and escalates rather than changing it.
---

# Implement one feature

One feature, end to end, on its own branch. Search, then Revalidate, then Booking, then
IssueTicket, then the rest — **one at a time**, each verified before the next starts.

The order matters: each feature teaches you something about the supplier that the next one
needs. Doing three at once means learning nothing until all three are broken.

## The rule that shapes everything here

> **The client side is fixed. Only the supplier side moves.**

You are writing a translation layer. The client request is an input you read; the client
response is an output shape you fill. Neither is yours to change. If the supplier returns
something with no home, that is an escalation — see
[`docs/permission-boundaries.md`](../../../docs/permission-boundaries.md).

## Before starting

Read: [`contracts/client-contract.md`](../../../contracts/client-contract.md),
[`docs/supplier-patterns.md`](../../../docs/supplier-patterns.md),
[`docs/coding-standards.md`](../../../docs/coding-standards.md),
`knowledge/suppliers/<supplier>.md`.

Confirm the feature name against the **canonical vocabulary**
([`coding-standards.md §2`](../../../docs/coding-standards.md)) — `IssueTicket` not `Ticketing`,
`CancelBooking` not `CancelPNR`.

## Step 1 — Branch

```bash
git fetch origin && git checkout staging && git pull origin staging
git checkout -b feature/<feature-name>
```

Always from freshly-pulled `staging`. See
[`docs/branching-and-pr.md`](../../../docs/branching-and-pr.md).

## Step 2 — Read the client contract for this feature

**Start here, not at the supplier.** The client contract defines what you receive and what you
must produce; the supplier work is then a known translation problem rather than an open one.

Write down, before touching the supplier:

- the client request fields available to build from
- **every field the client response requires** — this is your mapping checklist
- which client fields have no obvious supplier source (candidates for escalation)

## Step 3 — Study the supplier's API for this feature

From documentation and captured responses. Never from memory or from another supplier.

Establish: endpoint and method · request shape and required fields · response shape ·
error format and codes · limits · whether it is idempotent (decides retry policy).

**Every claim gets a source tag** — `[spec §x]`, `[observed in <fixture>]`, `[prod-verified]`,
`[assumed]`. An `[assumed]` on supplier behaviour must become an escalation, not code.

Write findings into `knowledge/suppliers/<supplier>.md` **now**, not at the end.

## Step 4 — Escalate the gaps before writing code

Two kinds:

1. **Unknown supplier behaviour** — documentation missing, contradictory, or ambiguous.
2. **Client fields with no supplier source, or supplier data with no client home.**

Raise them together, in one batch, before implementation. Discovering them mid-implementation
costs a rewrite; discovering them after a PR costs a revert.

Format: [`docs/human-intervention.md`](../../../docs/human-intervention.md).

## Step 5 — Supplier DTOs

```
Features/<Feature>/DTOs/Supplier/Request/
Features/<Feature>/DTOs/Supplier/Response/
```

- Model the supplier's **actual wire shape**, not a tidied version of it.
- **Every field nullable** until a captured response proves otherwise. A spec saying "mandatory"
  is not proof.
- Naming: `<Supplier><Feature>Request` / `<Supplier><Feature>Response`.
- **These never appear in a client-facing signature.** The moment one does, the boundary is gone.

## Step 6 — Validator

```
Features/<Feature>/Validator/<ClientRequest>Validator.cs
```

`AbstractValidator<T>`, auto-registered by assembly scan. Null-guard the request **and its
nested objects** before any field is read — the upstream request is untrusted.

## Step 7 — Request builder

```
Features/<Feature>/ModelBuilder/I<Feature>RequestModelBuilder.cs + impl
```

Client request → supplier request. **Only** that: no network calls, no response mapping, no
state. Dates in the supplier's format, codes resolved through constants, nothing inline.

## Step 8 — The service

```
Features/<Feature>/Services/I<Feature>Service.cs + <Feature>Service.cs
```

**Orchestration only.** No payload building, no field mapping. The sequence is the same for
every feature:

1. `ResponseBuilder.Init<TResponse>(request?.TrackingId ?? "")`
2. **Validate first, before reading any field.** Invalid → `ValidationError`, return.
3. Log `Client<Feature>Request`
4. Build the supplier request · log `Supplier<Feature>Request`
5. Call the supplier via the **named** `IHttpClientFactory` client, with the token cache
6. Log `Supplier<Feature>Response`
7. Guard: null response → `NoSupplierResponse`; supplier error → `SupplierError` with **their**
   message preserved; missing payload → the feature's specific message
8. Map · log `Client<Feature>Response` · return `Success`
9. `catch` → `UnhandledException` with the message. **Never swallow.**

**Retry policy is per feature.** Safe for reads. Never on `IssueTicket`, `Refund`, or
`VoidTicket` — a timeout there usually means it *succeeded* and the response was lost. Retrieve
and check state instead (lesson L-06).

The service never sets `IsSuccess` / `StatusCode` / `Message` by hand — `ResponseBuilder` owns
the envelope.

## Step 9 — Response mapper, decomposed

**The highest-risk step.** This is where every god class in the estate formed — thirteen of
seventeen services had a mapper over 800 lines, the largest 2,028.

**Name the builders before writing any of them:**

```
Features/<Feature>/Mappers/
  <Feature>ResponseMapper.cs     THIN orchestrator, ~50 lines, no field mapping
  <Feature>Assembler.cs          composes builder output into the client DTO
  SegmentBuilder.cs              one output concern
  FareBuilder.cs                 one output concern
  BaggageBuilder.cs              one output concern
  PenaltyBuilder.cs              one output concern
  ...one file per concern
```

Rules: stateless builders are `static` · **no file over 400 lines** · if the orchestrator passes
~80 lines a concern has leaked into it, extract it · raw supplier DTOs stay inside this layer.

Work the **client response checklist from step 2** field by field. Every field either maps from
a named supplier field, or is explicitly recorded as unavailable in the knowledge file. No field
is left silently empty.

## Step 10 — Endpoint, DI, and gRPC

- Route in `Endpoints.cs` — **one line**: call the service, return the result
- Register service, builder, mapper, and mapper's assembler in `ServiceConfiguration.cs`
- Add the four `LogType` members and the log-routing arm
- Add the `ApiNames` member
- For Search: map the gRPC service — the **same** feature service, never a second path

## Step 11 — Tests

Per [`docs/testing-strategy.md`](../../../docs/testing-strategy.md). Minimum for this feature:

| Layer | Tests |
| --- | --- |
| **Mapper** | happy path (every client field); empty results → empty list not null; supplier error; absent optional fields; money currency + per-passenger vs total; timezone offsets |
| **Builder** | required fields present; dates in supplier format; codes resolved |
| **Validator** | one per rule, plus null request and null nested objects |
| **Service** | validation short-circuits before any network call; null response → correct envelope; supplier fault not reported as success; **irreversible operations never retried**; all four log points fire |

Fixtures are **captured from the real sandbox and redacted** — shape preserved, values replaced.
A hand-written fixture tests your understanding of the supplier, not the supplier.

## Step 12 — Run it and watch it work

```bash
dotnet build <proj> -clp:ErrorsOnly --nologo
dotnet test  <testproj> --nologo
```

**Show the output.** Then start the service and drive the real flow — Swagger or `LocalTests/` —
against the sandbox, and confirm the feature actually returns what it should.

For Search: **are results actually coming back, correctly mapped?** That is the question this
step answers. Compiling is not working; passing unit tests against fixtures is not working
either.

Capture the responses you see as new fixtures. **Stop the service when you are done.**

If it fails, lead with that. Never report a success you did not observe.

## Step 13 — Knowledge and commit

Update `knowledge/suppliers/<supplier>.md`: the feature's endpoint and behaviour, quirks,
spec-versus-production differences (with dates), error codes, limits, fixtures captured,
remaining open questions.

Commit with a scoped message. No AI attribution trailers.

## Step 14 — Aggregator wiring

The feature is not usable until the aggregator can reach it. Switch to
[`aggregator-wiring`](../aggregator-wiring/SKILL.md).

**Do not open the supplier PR alone** — the two PRs pair and merge together
([`branching-and-pr.md`](../../../docs/branching-and-pr.md)).

## Definition of done

- [ ] 0 build errors, 0 warnings
- [ ] Validator at the boundary, before any field is read
- [ ] No magic strings or numbers
- [ ] Thin service · thin mapper · dedicated builders · **no file over 400 lines**
- [ ] Supplier DTOs never in a client-facing signature
- [ ] **Client contract unchanged** — or changed only with an approving ADR
- [ ] Four log points wired
- [ ] Retry policy correct — **none** on irreversible operations
- [ ] Every client field mapped or explicitly recorded as unavailable
- [ ] Tests written **and executed**, output shown
- [ ] **Real flow driven and observed**, service stopped afterwards
- [ ] Fixtures captured and redacted
- [ ] Knowledge file updated
- [ ] Decisions recorded as ADRs

## Escalate immediately

- Documentation missing, contradictory, or ambiguous
- Observed behaviour differs from the spec
- **A client field has no supplier source, or supplier data has no client home**
- Sandbox credentials unavailable
- A business rule (markup, penalty, refund window) is unclear
- Several valid designs exist
- You cannot safely determine correct behaviour

Never invent supplier behaviour to keep moving.
