# Architecture standard

**Status:** active · **Governs:** every `<Supplier>-Air-Service` · **Changes via:** [standards-governance](standards-governance.md)

## 1. What a supplier service is

A single-supplier translation microservice. It has exactly one job:

> Accept the aggregator's request, translate it to one supplier's API, translate that
> supplier's response back into the aggregator's shared client DTOs, and log both sides.

It owns no business policy, no pricing rules, no cross-supplier logic. Those belong to the
aggregator. A supplier service that starts making commercial decisions has
exceeded its remit.

## 2. Vertical slices — confirmed standard

Organise by **feature**, not by technical layer. Every one of the 17 analysed services does
this, and it is the one convention the ecosystem got unambiguously right.

```
Features/<Feature>/
  DTOs/Client/{Request,Response}/     aggregator contract — the only DTOs that cross out
  DTOs/Supplier/{Request,Response}/   supplier wire shape — must NEVER leak outward
  Validator/                          <Request>Validator : AbstractValidator<T>
  ModelBuilder/                       client request  -> supplier request
  Mappers/                            supplier response -> client response
  Services/                           I<F>Service + thin orchestrator
  Constants/                          feature-local protocol constants (optional)
```

**Why it works:** everything needed to change one supplier operation is in one folder, and
a feature can be added without touching another feature.

**The rule that makes it work:** supplier DTOs never escape the mapper layer. The instant a
`<Supplier>CreatePnrResponse` appears in a client-facing signature, the boundary is gone and
the aggregator is coupled to a supplier's wire format.

## 3. The pipeline — the real architectural unit

Every feature is the same five-stage pipeline. Learn it once, then apply it unchanged to
every feature of every supplier service.

```
Endpoint  ->  Validator  ->  ModelBuilder  ->  Supplier API  ->  Mapper  ->  Client DTO
                  |               |                                 |
                  |               +--- log supplier request         +--- log supplier response
                  +--- log client request                                log client response
```

| Stage | Owns | Must not |
| --- | --- | --- |
| **Endpoint** | Routing, DI resolution | Contain logic. One line: call the service, return the result. |
| **Validator** | Rejecting malformed input | Know anything about the supplier |
| **ModelBuilder** | Building the supplier request | Call the network, or map responses |
| **Service** | Orchestration, logging, error envelope | Build payloads or map fields |
| **Mapper** | Supplier response → client DTO | Call the network, or mutate state |

**Validate before reading any field.** The upstream request is untrusted. Every field access
before validation is a potential null-reference in production.

## 4. Response-mapper decomposition — the most important rule here

This is where the ecosystem fails hardest and where the standard is strictest.

**Measured evidence.** Two services doing the same job — mapping a search response to the
client DTO — differed by a factor of ten in file count and eleven in largest file (11 files /
187 lines vs 1 file / 2028 lines). The decomposed one was unit-testable; the other was not.
See [ADR-0002](decision-records/ADR-0002-response-mapper-decomposition.md) for the full
measurements.

### The required shape

```
Mappers/
  <F>ResponseMapper.cs      THIN orchestrator — composes builders, ~50 lines
  ReferenceData.cs          airport/airline/aircraft lookup, injected once
  SegmentBuilder.cs         one output concern
  FareBuilder.cs            one output concern
  BaggageBuilder.cs         one output concern
  PenaltyBuilder.cs         one output concern
  <F>Assembler.cs           assembles builder outputs into the client DTO
```

**One builder per output concern, one concern per file.** A builder that is stateless is
`static`. The orchestrator holds no mapping logic — if it grows past ~80 lines, a concern
has leaked into it.

**Hard limit: no file over 400 lines.** Not a guideline. It is demonstrably achievable on real
supplier APIs, including SOAP ones — see [ADR-0006](decision-records/ADR-0006-file-size-limit.md).

### Why the god mapper happens

It is never written as 2000 lines. It starts at 200, and every new field, fare type, and
edge case is appended because appending is cheaper than extracting. The decomposition must
exist **from the first commit** — retrofitting it costs a week per service.

## 5. Composition root

`Program.cs` composes, it does not configure. Around 30 lines is the target:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.ConfigureLogging();
builder.Services.AddSupplierConfiguration(builder.Configuration);
builder.Services.ConfigureServices(builder.Configuration);
var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.MapEndpoints();
app.Run();
```

Detail lives in `Extensions/*.cs` — one extension per concern (logging, Kestrel, swagger,
validators, DI). Routes live in `Endpoints.cs`. DI lives in
`Extensions/ServiceConfiguration.cs`.

## 6. Cross-cutting concerns

| Concern | Standard |
| --- | --- |
| **Errors** | One `GlobalExceptionMiddleware`. Services return an error envelope; they do not throw to the caller. |
| **Logging** | Non-blocking: `ILogService` → in-memory queue → `BackgroundWorker` → blob. Never block a request on log I/O. |
| **HTTP** | Named `IHttpClientFactory` clients. Never `new HttpClient()` — see lesson L-04. |
| **Config** | `IOptions<T>` bound from `appsettings.*.json`. Never hardcode an environment-varying value. |
| **Resilience** | Polly, on the named client. Timeout always; retry only on idempotent reads. **Never retry booking or ticketing.** |

## 7. Async discipline

- `async`/`await` end to end. `CancellationToken` accepted and passed through.
- **Never `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`.** Blocking on async work from a
  request thread risks thread-pool starvation — see lesson L-03 for a real instance.
- Constructors do no I/O. Async initialisation belongs in a factory or a hosted service.

## 8. Anti-patterns — do not reproduce these

| Anti-pattern | Why it is banned |
| --- | --- |
| God response mapper — the whole response mapped in one file, in breach of the 400-line limit | Untestable, unreviewable, a merge-conflict magnet |
| Client contract DTO defined per service | Copies drift silently — see [ADR-0001](decision-records/ADR-0001-shared-contracts-package.md) |
| Utility class copy-pasted between services | A fix to one is a fix to one |
| Whole mapper copy-pasted between services | Multiplies every latent bug |
| Config hardcoded instead of `IOptions<T>` | Cannot vary by environment without a rebuild |
| Unused dependency shipped to production | Unvetted supply-chain surface for zero benefit |
| Certificate or credential committed to git | Committed means compromised |

Each of these was measured in a real ecosystem before being banned; the evidence is in the
decision records and in `knowledge/`.
