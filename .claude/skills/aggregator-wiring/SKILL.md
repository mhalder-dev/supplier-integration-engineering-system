---
name: aggregator-wiring
description: Wire a supplier feature into the aggregator so it can route to a supplier service - supplier code registry, gRPC proto and client registration, configuration mapping, the per-supplier wiring service, and appsettings. Use after a supplier-side feature is complete and needs connecting (e.g. "wire a supplier's search into the aggregator", "connect a supplier's booking to the aggregator"). STRICTLY LIMITED to wiring - adding business logic, new abstractions, or new client DTOs in the aggregator requires human approval and is refused by this skill.
---

# Wire a supplier into the aggregator

The aggregator fans out to every supplier service. Adding a supplier means **extending lists
that already exist** — not adding capability.

## The constraint, stated first

> **You may only add a supplier to patterns that already exist. You may not add behaviour.**

The aggregator is shared by every supplier. A change made here to solve one supplier's problem
affects all of them. So the rule is narrow on purpose:

| Allowed | Refused without human approval |
| --- | --- |
| Add the supplier to an existing registry | Any business logic — pricing, filtering, ranking, penalties |
| Add a proto mirroring the existing envelope | A new abstraction, base class, or interface |
| Register a gRPC client alongside the others | Changing how existing suppliers behave |
| Add a wiring service subclassing the existing base | A new endpoint |
| Add a per-supplier request DTO mirroring the existing shape | **Adding a field to a client DTO** |
| Add this supplier's appsettings block | Changing fan-out, aggregation, or dispatch |
| | Anything in `Services/Shared/` |

**The test:** you are adding an entry to a list that exists, copying a shape that exists. If you
are creating a *new kind of thing*, stop and escalate.

Full boundary: [`docs/permission-boundaries.md`](../../../docs/permission-boundaries.md).

## Prerequisites

- The supplier-side feature is **complete, tested, and observed working**
- Supplier short code agreed and in `knowledge/suppliers/<supplier>.md`
- Aggregator path from `config/environment.md` (copy it from [`config/environment.example.md`](../../../config/environment.example.md) on first install)

## Step 1 — Branch from updated staging

```bash
cd "<aggregator-path>"
git fetch origin && git checkout staging && git pull origin staging
git checkout -b feature/<supplier-name>/<feature-name>
```

Note the aggregator's branch form includes the supplier name:
`feature/<supplier-name>/search`. The supplier repo's does not: `feature/search`.

## Step 2 — Read an existing supplier's wiring first

**Before writing anything, read a recent supplier's wiring end to end.** It is the
specification. Copy its shape exactly; deviation here is not creativity, it is drift.

Find every file that mentions an existing supplier:

```bash
grep -rln "<ExistingSupplier>" --include="*.cs" --include="*.json" --include="*.proto" . | grep -v /obj/
```

That list **is** the wiring surface. If your change touches a file that is not in it, you are
outside the boundary.

## Step 3 — The wiring surface

Typically these, and only these:

### 1. Supplier code registry
`Enums/SupplierCodeEnum.cs` — one constant, with the supplier's code and a comment:

```csharp
public const string XX = "XX";  // <Supplier Name> (IATA: XX)
```

**Record whether the constant's name and its value are identical.** They can only be identical
when the code is a legal C# identifier. A code that begins with a digit is not, so its constant
has to be named something else — and every later step that reaches for the name instead of the
value will then be routing under the wrong code. Carry this fact into item 6 below.

### 2. Proto
`Protos/<supplier>_search.proto` — mirror the existing envelope exactly. Only the namespace and
service name change. **Do not add fields**; the contract is the JSON inside, not the envelope.

### 3. gRPC client registration
`Extensions/SupplierGrpcExtensions.cs` — one registration alongside the others, using the same
defaults helper.

### 4. Configuration mapping
`Extensions/SupplierConfigurationExtensions.cs` — add the environment-variable entry to the
supplier map, matching the naming convention exactly.

### 5. Service registration
`Extensions/ServiceExtensions.cs` — register the wiring service alongside the others.

### 6. The wiring service
`Services/<Feature>/Suppliers/<Supplier><Feature>Service.cs`

Subclass the existing base. **Pure wiring** — build the request envelope, call, hand back. The
reference implementation is ~73 lines and contains no logic:

```csharp
public class <Supplier>FlightSearchService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogService logService,
    <Supplier>SearchService.<Supplier>SearchServiceClient client
) : FlightSearchServiceBase(httpClientFactory, configuration, logService)
{
    public override string SupplierCode => SupplierCodeEnum.<CODE>;  // the VALUE, not the name

    // REST path: build the request model, delegate to the base helper.
    // gRPC path: same model, serialised into the envelope, logged, sent.
}
```

> **Use the constant's value, not `nameof`.** You will see
> `SupplierCode => nameof(SupplierCodeEnum.<CODE>)` in existing services. `nameof` returns the
> **name of the constant**, not the code it holds. That is only correct while the two happen to
> be identical. When the supplier's IATA code cannot be a C# identifier — any code beginning with
> a digit — the constant must be named something else, and `nameof` then returns that identifier
> instead of the real code. Nothing fails to compile and nothing throws: the request is simply
> dispatched, logged and reconciled under the **wrong supplier code**, silently. Reference
> `SupplierCodeEnum.<CODE>` directly unless you have opened the registry and confirmed that the
> name and the value are character-for-character the same.

**If you find yourself writing an `if`, a calculation, or a transformation here, stop.** That
logic belongs in the supplier service, or it needs approval.

### 7. Per-supplier request DTO
`Models/<Feature>/Supplier/Request/<Supplier>Service<Feature>Request.cs` — mirror the existing
shape, which is normally just `TrackingId`, the client request, and the credentials:

```csharp
public class <Supplier>ServiceSearchRequest
{
    public string TrackingId { get; set; } = string.Empty;
    public SearchRequestDTO SearchRequest { get; set; } = new();
    public SupplierCredentialItem Supplier { get; set; } = new();
}
```

**This is allowed because it mirrors an existing per-supplier shape.** Adding a *field* the
other suppliers do not have is not — that is a contract change.

### 8. Settings
`appsettings.json` and every environment variant — this supplier's block, matching the existing
structure. **Endpoints and identifiers only. No credentials.**

> **These files are shared, and they may already hold live secrets.** Every supplier already
> wired is configured in the same file, and one of those blocks may contain a real key, password,
> connection string or token. So:
>
> - **Open and read the whole file before editing it.** Never write blind, never overwrite, never
>   regenerate it from a template — you would destroy another supplier's working configuration.
> - **Treat anything you read there as a live credential**, whatever it is named.
> - **Never paste the file, an excerpt of it, or its diff into a PR body, a chat message, a
>   ticket, or a log.** Refer to the keys by name only.
> - If you find a credential committed in a file you touched, do not quote it and do not "clean
>   it up" in passing — escalate it as a secret-exposure finding.

## Step 4 — Build and verify the pair

```bash
dotnet build <aggregator>.csproj -clp:ErrorsOnly --nologo
dotnet test  <aggregator>.Tests.csproj --nologo
```

Then **run both services together** — the supplier service and the aggregator — and drive the
flow through the aggregator, not directly at the supplier. That is the path production uses, and
it is the only way to know the wiring works.

Confirm results actually come back through the aggregator, correctly shaped. **Show the output.**
**Stop both services afterwards.**

## Step 5 — Review your own diff against the boundary

Before opening the PR:

```bash
git diff staging --stat
```

Every changed file must be on the wiring surface from step 2. Anything else is a finding — either
revert it, or escalate it and get an ADR before proceeding.

This is the single most valuable check in this skill. The aggregator diff is where unapproved
changes hide.

## Step 6 — Paired PR

```bash
gh pr create --base staging --title "Wire <Supplier> <Feature>" --body "..."
```

The body links the supplier PR, states test results and what was verified live, and confirms
the diff is wiring-only. **The two PRs merge together** — supplier first.

> **Nothing from a configuration file goes in the PR body.** The settings files you touched are
> shared and may contain live secrets belonging to other suppliers. Do not paste the file, a
> snippet of it, its diff, or a "before/after" block into the PR description, a review comment, a
> ticket, or a chat message. Name the file and the keys you added — that is enough for a
> reviewer, who reads the diff in the PR itself. The same applies to any log or console output
> you attach as evidence: check it for keys, tokens and passwords before pasting it.

See [`docs/branching-and-pr.md`](../../../docs/branching-and-pr.md).

## Definition of done

- [ ] Branched from freshly-pulled `staging`, correct branch name
- [ ] Supplier code added to the registry
- [ ] Proto mirrors the existing envelope, no added fields
- [ ] gRPC client registered alongside the others
- [ ] Config mapping and service registration added
- [ ] Wiring service contains **no logic**
- [ ] `SupplierCode` returns the constant's **value**, not `nameof` — unless the registry was
      opened and the name confirmed identical to the value
- [ ] Per-supplier request DTO mirrors the existing shape
- [ ] appsettings **read in full before editing**, updated for every environment, **no credentials**
- [ ] No configuration file, excerpt or diff pasted into the PR body, a ticket, or chat
- [ ] 0 build errors, 0 warnings; tests pass, output shown
- [ ] **Both services run together and the flow verified through the aggregator**
- [ ] `git diff staging --stat` reviewed — **every file on the wiring surface**
- [ ] PR opened against `staging`, paired with the supplier PR

## Refuse and escalate

Stop and raise it rather than proceeding, if the wiring seems to require:

- business logic of any kind in the aggregator
- a new field on a client DTO
- a new abstraction, base class, or endpoint
- changing shared code or how another supplier behaves
- a supplier whose response cannot be handled without upstream changes

That last one is the important one. **A supplier that does not fit is a finding about the
supplier, not a licence to change the aggregator.** Escalate with the three options — map into
what exists, drop the data, or change the contract — assessed. Format:
[`docs/human-intervention.md`](../../../docs/human-intervention.md).
