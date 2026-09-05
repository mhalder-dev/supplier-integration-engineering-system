---
name: supplier-bootstrap
description: Create a new supplier integration service from scratch - repository, solution file, project, composition root, appsettings for every environment, DI registration, Swagger, middleware, logging, gRPC scaffolding, and the client contract DTOs. Use when asked to start, create, set up, scaffold, or bootstrap a new supplier (e.g. "develop a new supplier", "create the supplier project", "start a new supplier service from scratch"). Produces a building, testable, feature-less service ready for the first feature. Does NOT implement any feature - use supplier-feature for that.
---

# Bootstrap a supplier service

Creates the base of a new supplier integration: everything that is the same for every supplier,
and nothing that is specific to one. When this finishes, the service **builds, runs, and passes
its tests** — with no features implemented.

**Nothing here is invented.** The base mirrors what already exists across the estate; the client
DTOs are copied from the aggregator. If you find yourself designing, stop — that is a signal you
are in the wrong skill.

## Prerequisites

1. **Supplier name**, correctly spelled. It becomes namespaces that are permanent. Confirm the
   spelling with the human if there is any doubt — the estate already ships two repositories
   with a typo in the name, frozen forever.
2. **Supplier short code** — the airline code or agreed abbreviation. Goes in the aggregator's
   supplier-code registry.
3. **Protocol** — REST/JSON, SOAP/XML, or NDC. Changes the HTTP layer only.
4. Paths from `config/environment.md` (copy it from [`config/environment.example.md`](../../../config/environment.example.md) on first install).

Read before starting: [`docs/architecture.md`](../../../docs/architecture.md),
[`docs/coding-standards.md`](../../../docs/coding-standards.md),
[`contracts/README.md`](../../../contracts/README.md).

## Step 1 — Confirm scope, then branch

Restate: supplier name, short code, protocol, repository location. Get the name confirmed
before creating anything.

```bash
cd "<new-work-root>"
mkdir "<Supplier>-Air-Service" && cd "<Supplier>-Air-Service"
git init -b staging
```

**`-b staging` matters.** `git init` alone leaves you on `master`, and every later step —
`supplier-feature` step 1 (`git checkout staging`), and `gh pr create --base staging` — assumes
`staging` exists.

**A remote is required before the first feature.** `supplier-feature` begins with
`git fetch origin`, which fails outright on a repository with no remote. Creating the remote is a
human action (it needs an account and an organisation):

```
Problem:          <Supplier>-Air-Service has no git remote.
Impact:           BLOCKED: supplier-feature step 1, and every PR. NOT blocked: bootstrap itself.
Decision required: Create the remote under which organisation, and who does it?
```

Raise that escalation at the end of bootstrap if no remote exists. Do not skip it and hope —
the failure surfaces later, mid-feature, where it is more disruptive.

Naming, consistent with the estate:

| Thing | Form |
| --- | --- |
| Repository | `<Supplier>-Air-Service` |
| Project | `<Supplier>.AirAPI` |
| Namespace root | `<Supplier>.AirAPI` |
| Test project | `<Supplier>.AirAPI.Tests` |

## Step 2 — Solution and project

`.slnx` — the modern format, opens in Rider and Visual Studio:

```xml
<Solution>
  <Project Path="<Supplier>.AirAPI/<Supplier>.AirAPI.csproj" />
  <Project Path="<Supplier>.AirAPI.Tests/<Supplier>.AirAPI.Tests.csproj" />
</Solution>
```

`.csproj` — `net9.0`, nullable enabled, implicit usings, **`TreatWarningsAsErrors`**. Package
versions are pinned and must match the estate; take them from an existing service rather than
from memory — they drift otherwise.

Baseline packages: FluentValidation (+ DI extensions), Swashbuckle, Serilog.AspNetCore, Polly,
Grpc.AspNetCore + Google.Protobuf + Grpc.Tools, plus the storage clients the estate uses.

**Do not add a package the estate does not already use** without approval
([permission-boundaries](../../../docs/permission-boundaries.md)).

## Step 3 — Configuration

Five files, matching every other service: `appsettings.json`,
`.Development`, `.Staging`, `.Uat`, `.Production`.

They hold **endpoints, timeouts, storage settings, and log configuration**. They hold **no
credentials** — those arrive per request in the `supplier` block of the client contract.

Bind with `IOptions<T>`; never read `IConfiguration` from a feature class.

## Step 4 — Composition root

`Program.cs` composes and does not configure. Target ~30 lines:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
builder.Services.AddGrpc();
builder.Services.AddCorsPolicy();
builder.Services.AddSupplierSwagger();
builder.Services.AddSupplierConfiguration(builder.Configuration);
builder.Services.ConfigureServices(builder.Configuration);
builder.Services.AddValidators();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseRouting();
app.UseCors("AllowAll");
app.UseSupplierSwagger();
app.MapEndpoints();
app.Run();
```

Detail goes in `Extensions/`: `ServiceConfiguration`, `LoggingExtensions`, `SwaggerExtensions`,
`ValidatorExtensions`, `KestrelExtensions`.

**Swagger must be properly configured** — title, version, and every endpoint visible. It is how
a human drives the service by hand before the aggregator is wired.

## Step 5 — Shared layer

The pieces every feature depends on:

```
Shared/
  Constants/   ServiceMessages, ServiceStatusCodes, ApiNames, SupplierConstants
  DTOs/        BaseResponse envelope
  Http/        ResponseBuilder  (the ONLY place an envelope is shaped)
  Services/    SupplierServiceBase, ITokenCache + TokenCache
  Options/     SupplierOptions
Middleware/    GlobalExceptionMiddleware, RequestLoggingMiddleware
```

`ApiNames` gets a member per feature, using the canonical vocabulary. No feature name is ever
spelled inline.

**Logging is non-blocking**: `ILogService` → queue → background worker → blob. Wire the pipeline
now; features only add their four log points.

## Step 6 — Client contract DTOs

**Copy, do not author.** Take the definitions from the aggregator — paths in
[`contracts/client-contract.md`](../../../contracts/client-contract.md) — into:

```
Features/<Feature>/DTOs/Client/    the client-contract types for that feature —
                                   for Search, the flight result and its nested types
```

Then, in the same commit, note in `knowledge/suppliers/<supplier>.md` the date and aggregator
commit they were taken from.

**These files are read-only from here on.** Changing one is a contract change and needs approval
([permission-boundaries](../../../docs/permission-boundaries.md)).

## Step 7 — gRPC scaffolding

Search reaches the service over gRPC. Add the proto, mirroring the estate's envelope exactly —
only the namespace and service name change:

```
Protos/search.proto      option csharp_namespace = "<supplier>.SearchGrpc";
                         service <Supplier>SearchService { rpc Search (...) returns (...); }
Grpc/<Supplier>SearchGrpcService.cs
```

The gRPC service is a **thin adapter**: deserialise `payloadJson`, call the same feature service
the REST endpoint calls, serialise the result. No logic of its own, and never a second code path.

Register it in `Program.cs` when Search is implemented, not before.

## Step 8 — Endpoints and test project

`Endpoints.cs` with a route group and a `/status` endpoint. Feature routes get added by the
feature skill.

Test project mirroring the estate: xUnit, `Microsoft.NET.Test.Sdk`, coverlet, a
`ProjectReference` to the service, a `Fixtures/` folder copied to output, and a `FixtureLoader`.
See [`docs/testing-strategy.md`](../../../docs/testing-strategy.md).

## Step 9 — Repository hygiene

- `.gitignore` covering `bin/`, `obj/`, **`LocalTests/`, `*.pfx`, `*.pem`, `credentials*.json`,
  `raw/`**
- `CLAUDE.md` that **points at this engineering system rather than restating it**, plus this
  supplier's specifics and current state
- `README.md` — build and test commands, layout, status
- `Dockerfile` and pipeline config, matching the estate

## Step 10 — Verify, then commit

Non-negotiable — do not report success without this output:

```bash
dotnet build <Supplier>.AirAPI.Tests/<Supplier>.AirAPI.Tests.csproj -clp:ErrorsOnly --nologo
dotnet test  <Supplier>.AirAPI.Tests/<Supplier>.AirAPI.Tests.csproj --nologo
```

**Show the output.** Then run the service and hit `/status` and Swagger — bootstrap is about the
base actually working, and compiling is not working.

Commit on `staging` (this is repository creation, not a feature), then create
`knowledge/suppliers/<supplier>.md` from the template and record what is known so far.

## Definition of done

- [ ] Repository exists at the correct path, git initialised
- [ ] `.slnx` opens in Rider/Visual Studio
- [ ] `net9.0`, nullable, `TreatWarningsAsErrors`
- [ ] Five appsettings files, `IOptions<T>` bound, **no credentials**
- [ ] `Program.cs` ~30 lines; detail in `Extensions/`
- [ ] Swagger reachable and correct
- [ ] Shared constants, envelope, `ResponseBuilder`, base service, token cache
- [ ] Non-blocking logging pipeline wired
- [ ] Client DTOs **copied from the aggregator**, provenance recorded
- [ ] gRPC proto + thin adapter present
- [ ] Test project builds and runs
- [ ] `.gitignore` covers credentials and `LocalTests/`
- [ ] `CLAUDE.md` points at the engineering system, does not restate it
- [ ] **Build and test output shown**
- [ ] Service runs; `/status` and Swagger verified
- [ ] Knowledge file created

## What this skill does NOT do

No features. No supplier DTOs. No request builders, mappers, or feature services. No aggregator
changes.

Next: [`supplier-feature`](../supplier-feature/SKILL.md) for the first feature, usually Auth
then Search.

## Escalate

- Supplier name or short code ambiguous
- Protocol unclear
- A needed package is not already used in the estate
- The aggregator's client DTOs cannot be located or are inconsistent between services

Format: [`docs/human-intervention.md`](../../../docs/human-intervention.md).
