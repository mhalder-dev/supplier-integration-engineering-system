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
[`client-contract.md`](README.md).

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

---

## What a client contract is

Four things, and only four:

| # | Object | Question it answers |
| --- | --- | --- |
| 1 | **Request envelope** | What outer shape does the aggregator send per feature? |
| 2 | **Credentials object** | How do supplier credentials and settings arrive? |
| 3 | **Response envelope** | What outer shape must every response carry back? |
| 4 | **Result payload** | What does the feature's actual data look like? |

Capture those four and a supplier service can be built against them. Miss one and it will be
invented locally, which is exactly how an estate ends up with as many contracts as suppliers.

## Where to find it

The aggregator is the source of truth — it is the **consumer**, so its definition is the one
that matters. In order of usefulness:

1. **The aggregator's request/response model types** for one feature. These are the contract.
2. **The aggregator's wiring for a supplier that already works end-to-end.** A working caller
   is a better oracle than any document: it shows what is actually populated, not what is
   declared.
3. **An existing supplier service.** Useful for orientation, **never** authoritative. Where an
   existing service and the aggregator disagree, the aggregator wins and the difference is
   drift to report, not to replicate.

Record where you looked. Every entry below should be traceable back to a file in the
aggregator so any detail can be re-checked.

## The transport question

Answer this before capturing shapes, because it decides what "the contract" even is.

| Ask | Why it matters |
| --- | --- |
| Which features go over **gRPC**, which over **REST**? | High-volume features (typically search) are often gRPC; the rest REST. |
| If gRPC: is the envelope **thin or typed**? | A thin envelope carries a serialised JSON string, so the real contract is the JSON. A typed envelope makes the proto itself the contract, and every field change is a proto change. |
| Is there a **REST fallback** for a gRPC feature? | If so, both channels must carry the identical payload. |

A thin envelope looks like this — a correlation id and a JSON string in, a JSON string out:

```protobuf
syntax = "proto3";
option csharp_namespace = "<YourNamespace>.<Feature>Grpc";

message <Feature>Request {
  string trackingId  = 1;
  string payloadJson = 2;
}

message <Feature>Response {
  string responseJson = 1;
}

service <Feature>Service {
  rpc <Feature> (<Feature>Request) returns (<Feature>Response);
}
```

**Mirror whatever your aggregator already has, exactly.** Do not add fields to the envelope:
anything present in gRPC but not in the JSON makes the two channels diverge, and the divergence
will not show up until the fallback path is exercised in production.

Record the answer here:

| Feature | Channel | Envelope | Source |
| --- | --- | --- | --- |
| _e.g. Search_ | _gRPC_ | _thin, JSON payload_ | _`<path in aggregator>`_ |
| _all others_ | _REST_ | _JSON over HTTP POST_ | _`<path in aggregator>`_ |

## 1 — Request envelope

Find the type the aggregator serialises when it calls a supplier service. Expect an outer object
with three parts: a **correlation id**, the **feature request**, and the **credentials object**.

```jsonc
{
  "<correlationId>":     "string",   // echo it in every log and every response
  "<feature>Request":    { },        // the aggregator's feature request
  "<credentialsField>":  { }         // credentials + supplier settings
}
```

Then record the feature request's own fields:

| Field | Type | Notes |
| --- | --- | --- |
| | | |

Things to capture that are easy to miss, and expensive to miss:

- **Exact spelling, including mistakes.** A misspelled property name is part of the contract.
  Do not "fix" it — a corrected name silently stops binding.
- **Defaults.** A field that defaults to a value (a maximum result count, a default number of
  stops) is a constraint on your mapping, not a suggestion. Respect it.
- **Normalisation the aggregator already applies.** If the aggregator's own setters clear,
  clamp, or sanitise a field, the supplier service receives the *result* and must not
  re-implement it — but also must not assume a collection is populated just because a related
  count is non-zero. Note each such behaviour where you find it.
- **Nullability and collection emptiness**, separately. `null` and `[]` are different contracts.

## 2 — Credentials object

Find the type carrying per-supplier credentials and settings. Record it the same way:

| Field | Type | Notes |
| --- | --- | --- |
| | | |

Three rules apply to whatever you find:

- **Credentials arrive per request.** They are never read from the supplier service's own
  configuration and never committed to its repository.
- **Never log them.** Not at debug level, not in a request dump, not "temporarily".
- **Expect supplier-specific fields on a shared type.** Some fields will be meaningful to one
  supplier and ignored by the rest. That is existing reality — use what applies to you and
  **do not add more**.

## 3 — Response envelope

Find the base type every feature response subclasses. Expect a success flag, a status code, a
message, an error, and the echoed correlation id.

| Concept | Your field name | **Type — copy it exactly** | Notes |
| --- | --- | --- | --- |
| Success flag | | | |
| Status code | | | **Check whether it is a string or an int. Do not assume.** |
| Message | | | Human-readable |
| Error | | | Populated on failure |
| Correlation id | | | Echo the request's |

> **The status code's type is the single most common contract bug.** If the aggregator declares
> it as a string and a service returns a number — or the reverse — the response will not
> deserialise, and the failure surfaces as an empty result rather than an error. Read the
> declaration; do not infer the type from the values you see in a sample payload.

Record the same for the message and error fields: an `error` typed as a string is not the same
contract as an `error` typed as an object.

## 4 — Result payload

The feature's actual data — for search, the flight results. This is the largest type, the one
most likely to tempt a supplier into adding a field, and therefore the one that drifts first.

**Take it from the aggregator when bootstrapping, and copy it verbatim.** Do not re-type it,
do not tidy it, do not drop fields that look unused. Record the source path here:

```
<aggregator>/<path to the feature's result payload type>
```

If existing supplier services disagree with the aggregator on this type — extra flags, a raw
supplier-JSON passthrough, a headers collection, a passenger-requirement flag present in exactly
one service — take the **aggregator's** definition and record the difference as drift. Drift is
reported, not replicated.

## How to record what you find

- **Point at code; do not re-author it.** This folder is a *record*. The runtime source of
  truth is the aggregator's code. Every table above should carry the source path beside it.
- **A duplicated contract drifts.** The moment this file becomes a second definition that
  someone edits instead of the code, it is worse than nothing. Keep it thin enough that
  re-verifying is cheaper than trusting it.
- **Tag certainty.** Mark each entry as verified against code, or inferred. An inferred type is
  a bug waiting for its first non-happy-path response.
- **Date it.** Put the verification date at the top of your captured contract, and record the
  aggregator's location in your own environment file, copied from
  [`config/environment.example.md`](../config/environment.example.md) — the one file that names
  your installation.
- **Note what you dropped.** A supplier field with no home in the contract goes in that
  supplier's knowledge file under `knowledge/suppliers/`, with the reasoning.

## Feature naming

Whatever your aggregator calls its features, pick one canonical name per feature and use it
everywhere — type names, folders, endpoints, logs. The canonical vocabulary and the banned
variants: [`docs/coding-standards.md §2`](../docs/coding-standards.md).

## Whatever you capture becomes read-only input

Once recorded:

1. **At bootstrap**, the client DTOs are generated into `Features/<Feature>/DTOs/Client/` from
   this contract. They are not authored by hand.
2. **During a feature**, the request builder reads *from* the client request and the mapper
   writes *into* the client response. Both treat the client side as fixed.
3. **The envelope is set only by the shared response builder.** Services and mappers never
   assign the success flag, status code, or message by hand — see
   [`docs/coding-standards.md §5`](../docs/coding-standards.md).
4. **They are never edited to fit a supplier.** Only the supplier DTOs move. If the contract
   genuinely cannot hold what a supplier returns, escalate — see
   [`README.md`](README.md) and
   [`docs/permission-boundaries.md`](../docs/permission-boundaries.md).

## Re-verification

Before relying on a detail recorded here, confirm it against the aggregator. Keep a runnable
checklist beside your captured contract, so re-verification is one command per object:

```bash
# request shape
cat "<aggregator>/<path to the feature request type>"
# credentials
cat "<aggregator>/<path to the credentials type>"
# response envelope
cat "<aggregator>/<path to the response envelope base type>"
# result payload
cat "<aggregator>/<path to the result payload type>"
```

Re-verify whenever the aggregator changes, and before every new supplier bootstrap.

**Target state:** a shared package so there is exactly one definition and this folder becomes
its documentation, not a copy of it. See
[ADR-0001](../docs/decision-records/ADR-0001-shared-contracts-package.md).
