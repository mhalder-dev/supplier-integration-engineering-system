# Bootstrap reference implementation

A worked example of what
[`supplier-bootstrap`](../../.claude/skills/supplier-bootstrap/SKILL.md) produces. It exists to
be **read**, and to be **checked against** — a generated project should look like this.

**Do not copy this folder to start a supplier.** Run the bootstrap skill. It names the
repository, project, namespaces and `RootNamespace` for the supplier and copies the real client
DTOs from the aggregator, neither of which a static folder can do — the client DTOs in here are
illustrations, and a service built on them will not deserialise. See
[`../README.md`](../README.md).

Both routes serve the same rule,
[ADR-0005](../../docs/decision-records/ADR-0005-no-new-service-by-copy.md): **new services are
created from the template, never by copying a working service.**

Copying a working service is how the estate we analysed acquired 16 copies of the misspelled
folder `Loging`, 16 divergent copies of `General.cs`, and an 827-line mapper duplicated across
three services with a 22-line diff. This reference exists so that stops.

## Verified state

Last built 2026-09-05:

```
dotnet build   -> 0 errors, 0 warnings (TreatWarningsAsErrors=true)
dotnet test    -> 20 passed, 0 failed
largest file   -> 121 lines (limit 400)
```

## What it gives you

| | |
| --- | --- |
| Composition root | `Program.cs` — 23 lines, detail delegated to `Extensions/` |
| Routing | `Endpoints.cs` — one line per route |
| DI | `Extensions/ServiceConfiguration.cs`, grouped by feature |
| Config | `IOptions<SupplierOptions>` bound from `appsettings.*.json` |
| HTTP | Named `IHttpClientFactory` clients — never `new HttpClient()` |
| Errors | `GlobalExceptionMiddleware` + `ResponseBuilder` envelope discipline |
| Auth | `TokenCache` — per-key gates, double-checked locking, early expiry |
| Constants | `ServiceMessages`, `ServiceStatusCodes`, `ApiNames` — no magic values |
| **A complete Search slice** | validator → model builder → service → **decomposed mapper** |
| Tests | xUnit + fixture harness + 20 passing tests |

### The Search slice is the point

It is a worked example of every standard at once. The mapper is deliberately split:

```
Mappers/
  SearchResponseMapper.cs      28 lines  — thin orchestrator, no field mapping
  FlightResultAssembler.cs     44 lines  — composes builder output
  SegmentBuilder.cs            23 lines  — one concern
  FareBuilder.cs               26 lines  — one concern
  BaggageBuilder.cs            14 lines  — one concern
```

The equivalent class in the estate we analysed is **2,028 lines**. Add a concern by adding a
builder, never by growing the orchestrator.

## Using it

Run the bootstrap skill to create the service, then read this folder to check what came out.

The names here are deliberately neutral — this folder is `bootstrap-reference`, and the project
inside it is `src/SupplierService`. A generated service carries the supplier's names in those
positions instead: repository `<Supplier>-Air-Service`, project and namespace root
`<Supplier>.AirAPI`, test project `<Supplier>.AirAPI.Tests`. Compare shape, not names.

**Spell the supplier's name correctly.** Namespace and folder typos become permanent — the
estate we analysed froze the same misspelling into two project folder names, `…-Air-Servcie`.

What the skill deliberately leaves for you, in order:

1. **Fill in `appsettings.json`** — `Name`, `ShortCode`, `BaseUrl`, `TokenUrl`. Credentials
   never go here; they arrive per request or from a gitignored local file.
2. **Replace the supplier DTOs** in `Features/Search/DTOs/Supplier/` with the real wire shape.
3. **Implement `SearchService.CallSupplierAsync`** — it throws `NotImplementedException` by
   design. Wire the named `HttpClient` and `ITokenCache`.
4. **Delete the placeholder fixtures** and capture real ones. See
   [`tests/.../Fixtures/README.md`](tests/SupplierService.Tests/Fixtures/README.md).
5. **Add features** by copying the `Features/Search/` folder shape and using the canonical
   names from [`docs/coding-standards.md §2`](../../docs/coding-standards.md).

## Deliberate stubs

Three things are intentionally unfinished, because they need a real supplier:

| Stub | Where | Replace with |
| --- | --- | --- |
| `CallSupplierAsync` throws | `Features/Search/Services/SearchService.cs` | Named `HttpClient` + `ITokenCache` |
| `LogAsync` writes to `ILogger` | `Shared/Services/SupplierServiceBase.cs` | The real `ILogService` → queue → worker → blob |
| `IsInternational` heuristic | `Mappers/FlightResultAssembler.cs` | Country lookup from reference data |

Each is commented at the site. **Do not ship them as they are.**

## What it deliberately omits

- **gRPC** — add only if the platform needs it for this supplier.
- **A real `ILogService`** — the queue/worker/blob pipeline is infrastructure that should
  eventually be a shared package, not re-templated per service (ADR-0001).
- **`IReferenceData`** — airport/airline/aircraft lookup is a shared concern, not a per-service
  one; in the estate we analysed it is `General.cs` copied 16 times, which ADR-0001 is meant to
  fix. Build it once behind a single interface, or put your own existing lookup behind that
  interface if you already have one. Either way, one implementation shared by every service —
  never a fresh copy per service.

These omissions are choices, not oversights. Adding them per service is what produced the
duplication the assessment measured.
