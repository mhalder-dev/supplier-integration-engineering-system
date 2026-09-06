# Testing strategy

**Status:** active · **Changes via:** [standards-governance](standards-governance.md)

## The problem this solves

Supplier integrations are widely treated as untestable, on the grounds that you cannot test one
without calling the supplier. In a real 17-service ecosystem this produced 12 services with no
tests at all, including both services considered architectural references.

The justification is false. The risky part of a supplier integration is *mapping*, and mapping
is pure. See [ADR-0004](decision-records/ADR-0004-fixture-based-testing.md).

## The core technique: captured fixtures

Capture real responses from the supplier's staging or sandbox environment, redact them, and
commit them as JSON fixtures. Replay them against the mappers offline.

This is the standard, and it is what makes the rest of the pyramid possible: given a real
captured response, mapping is fully testable with no network, no credentials, and no
flakiness.

**Hand-written fixtures are not a substitute.** A fixture you invented tests your
understanding of the supplier, not the supplier. Capture real traffic.

### Fixture rules

1. **Captured from a real sandbox/staging call**, never hand-authored to match your mapper.
2. **Redacted before commit** — strip tokens, passwords, certificates, PII, agency ids.
   Redaction must not change the *shape*: replace values, keep keys and structure.
3. **Named for the scenario**: `search-oneway.json`, `search-roundtrip-intl.json`,
   `book-error-duplicate-pnr.json`.
4. **Committed** to the test project, copied to output:
   ```xml
   <Content Include="Fixtures\*.json" CopyToOutputDirectory="PreserveNewest" />
   ```
5. **Dated and sourced** in a `Fixtures/README.md`: which environment, which date, which
   request produced it. A fixture whose provenance is unknown is worth little when the
   supplier changes their format.

## The test pyramid

| Layer | What | Network | When |
| --- | --- | --- | --- |
| **1. Mapper tests** | Supplier response fixture → client DTO | none | every feature, always |
| **2. Builder tests** | Client request → supplier request payload | none | every feature, always |
| **3. Validator tests** | Malformed input rejected at the boundary | none | every feature, always |
| **4. Service tests** | Orchestration, error envelopes, retries | mocked | every feature |
| **5. Contract tests** | Client DTO matches the aggregator's contract | none | every feature |
| **6. Live sandbox** | Real call to the supplier's test environment | real | manual, gated |

Layers 1–5 run in CI on every commit. Layer 6 is manual and never runs in CI.

### Layer 1 — mapper tests (highest value)

For every feature, at minimum:

- happy path maps every field the client DTO declares
- empty result set produces an empty list, not a null and not a crash
- supplier error response produces a failed envelope with the supplier's message preserved
- optional/absent fields do not throw
- one-way vs round-trip vs multi-city, where supported
- money: currency preserved, per-passenger vs total not confused
- timezones: local time converted with the right offset

```csharp
[Fact]
public async Task Maps_every_offer_to_a_flight()
{
    var result = await MapAsync("search-oneway.json");

    Assert.True(result.IsSuccess);
    Assert.Equal(7, result.Flights.Count);
    Assert.All(result.Flights, f => Assert.NotEmpty(f.FlightId));
}
```

### Layer 2 — builder tests

Assert the built payload against the supplier's spec: required elements present, element
order correct where the supplier is strict, dates in the supplier's format, passenger type
codes correct. For SOAP, compare canonicalised XML rather than raw strings — whitespace
differences are not defects.

### Layer 3 — validator tests

One test per rule, plus: null request, null nested objects, empty collections. This layer is
cheap and catches the null-reference class of production failure.

### Layer 4 — service tests

Mock the HTTP boundary (`HttpMessageHandler` or the client interface). Assert: validation
failure short-circuits before any network call; a null supplier response yields the right
error envelope; a supplier fault is not reported as success; **irreversible operations are
never retried**; all four log points fire.

### Layer 5 — contract tests

Assert the produced client DTO matches the aggregator's contract. This is what stops each service
quietly growing its own variant of the shared DTO. Until a shared contracts package exists
([ADR-0001](decision-records/ADR-0001-shared-contracts-package.md)), assert against a committed
JSON schema.

### Layer 6 — live sandbox (gated, manual)

Real calls to the supplier's test environment, driven from `LocalTests/` (gitignored).

- Legitimate sandbox credentials only, supplied by a human.
- **Never** production credentials. **Never** fabricated credentials.
- Read-only flows (search, retrieve) freely; write flows (book, ticket) only against a
  sandbox that is explicitly documented as safe.
- Every response captured here becomes a Layer-1 fixture. That is the main reason to run it.

## What "tested" means

A feature is tested when Layers 1–5 pass **and the output has been shown**. "It compiles" is
not tested. "The tests exist" is not tested. The definition of done requires executed tests
with visible results.

If a layer was skipped, the completion report says which and why. An honest gap is
manageable; a silent one is not.

## Running tests

```bash
dotnet test <Supplier>.AirAPI.Tests/<Supplier>.AirAPI.Tests.csproj --nologo
```

Standard test project layout:

```xml
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="coverlet.collector" Version="6.0.2" />
```

Naming: `<Thing>Tests.cs`, methods named as sentences —
`Maps_every_offer_to_a_flight`, `Rejects_request_with_no_segments`.

## Coverage

`[Opinion]` No percentage target. A coverage number is easy to reach with tests that assert
nothing. The requirement is behavioural: every feature has all five offline layers, and every
production bug gains a regression test before it is closed.

## When a supplier changes their format

1. Capture the new response as a **new** fixture; keep the old one.
2. Both must pass, or the old one is deleted with a note saying the supplier retired it.
3. Record the change in `knowledge/suppliers/<supplier>.md` with the date.

This is how the knowledge base stays true over time rather than describing an API that no
longer exists.

## Contract schemas — the near-term priority

Until the shared contracts package exists
([ADR-0001](../docs/decision-records/ADR-0001-shared-contracts-package.md)), a committed JSON
schema for `FlightResultDto` is the cheapest defence against further drift. Every supplier's
contract test asserts its output against that schema, so divergence becomes a failing test
rather than an invisible difference discovered in production.

This is deliberately the lowest-cost half of ADR-0001: it needs no NuGet feed, no release
process, and no coordination — one service can adopt it today.

**Status:** not written yet, and blocked on a decision this repository cannot take.

**The open decision:** *which `FlightResultDto` shape is canonical?* A schema can only be
written once one shape is chosen, and the choice belongs to the team that owns the aggregator's
contract — in the estate we analysed the DTO existed in 17 copies with 17 different hashes and
property counts ranging from 4 to 23, so picking one unilaterally here would simply add an
eighteenth.

Carry it as a live question, not a footnote: add it to the open-questions table in
[`development/progress.md`](../development/progress.md) with a named owner and the date raised,
and write the schema in the commit that records the answer.

## Fixture redaction

Every fixture in this repository and in every supplier test project must be redacted before
commit: replace credential and PII **values**, preserve the **shape**. A fixture whose
structure was changed by redaction no longer tests the real wire format.

See [`docs/testing-strategy.md`](../docs/testing-strategy.md) and
[L-07](../knowledge/lessons-learned.md).
