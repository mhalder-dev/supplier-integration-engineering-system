# Reference implementations

> ### ⚠ The client DTOs in here are FABRICATED
>
> `[Certain, verified against a real aggregator contract]` The client-side DTOs in
> `bootstrap-reference` were authored as illustrations. They **do not match a real aggregator
> contract** — the one they were checked against declares `StatusCode` as a **string** where the
> reference uses `int`, and the reference omits fields the real contract carries.
>
> **A service built by copying these cannot be deserialised by the aggregator.**
>
> They exist to show the *structure*. The bootstrap skill copies the real types from the
> aggregator — see [`contracts/`](../contracts/). Never take the client DTOs from here.

**These are not the deliverable.** The deliverable is the skills in `.claude/skills/` — they
generate a project and drive features. This folder holds worked examples for those skills to
point at, and for a human to check output against.

| Folder | What it is |
| --- | --- |
| [`bootstrap-reference/`](bootstrap-reference/) | What [`supplier-bootstrap`](../.claude/skills/supplier-bootstrap/SKILL.md) produces, with one worked Search slice showing the decomposed mapper. Verified building, 20 tests passing. |

## How to use it

**Do not copy this folder to start a supplier.** Run the bootstrap skill — it generates a
correctly named, correctly configured project and copies the client DTOs from the aggregator,
which a static copy cannot do.

Use this instead to:

- see what the output should look like before running the skill
- check a generated project against a known-good shape
- read the decomposed mapper as a concrete example of the rule in
  [ADR-0002](../docs/decision-records/ADR-0002-response-mapper-decomposition.md)

## The mapper example

`bootstrap-reference/src/SupplierService/Features/Search/Mappers/` is the part worth reading:

```
SearchResponseMapper.cs    28 lines   orchestrator - no field mapping
FlightResultAssembler.cs   44 lines   composes builder output
SegmentBuilder.cs          23 lines   one concern
FareBuilder.cs             26 lines   one concern
BaggageBuilder.cs          14 lines   one concern
```

Five files. The equivalent class in the estate we analysed is **2,028 lines**. Adding branded fares
here means adding `BrandedFareBuilder.cs` — nothing grows.

## Known limitation

The client DTOs in this reference are **placeholders**, not the aggregator's real contract. A
static folder cannot stay in step with a contract that lives in another repository. The bootstrap
skill copies the real ones; see [`contracts/`](../contracts/).
