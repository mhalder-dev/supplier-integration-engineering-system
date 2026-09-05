# Environment configuration — template

**This is the only file that names a specific organisation, machine, or path.**

Everything in `docs/` and `.claude/` is deliberately generic so the system can be dropped into
any organisation unchanged. This file is the one thing each adopter writes.

`knowledge/` and `docs/decision-records/` are intentionally *not* generic — see
[Why the split](#why-the-split) below.

## How to use this file

1. Copy it to `config/environment.md`.
2. Replace every `<PLACEHOLDER>` with a real value. Each table carries a **How to find it**
   column — follow it rather than guessing.
3. Delete nothing structural. A row you cannot fill yet stays, marked `TODO`, so the gap is
   visible. An empty placeholder that silently ships reads as a fact and will be trusted.
4. Keep it current. This file is the map the skills navigate by; a stale path here fails the
   work silently, not loudly.

## Current installation

| Field | Value | How to find it |
| --- | --- | --- |
| Organisation | `<ORGANISATION>` | Your organisation or team name — used only for labelling. |
| **Aggregator root** | `<aggregator-root>` | The absolute path of the working copy of the platform that fans out to supplier services. |
| Aggregator remote | `<aggregator-git-remote-url>` | `git -C <aggregator-root> remote get-url origin` |
| Aggregator project · tests | `<AggregatorApiProject>` · tests `<AggregatorTestProject>` | The two project names in the aggregator's solution file: the API host and its test project. |
| Existing supplier services (read-only reference) | `<supplier-service-root>/<Supplier>-Air-Service` | The directory holding the supplier integrations already in production. Treat as read-only reference. |
| New supplier work | `<workspace-root>/<Supplier>-Air-Service` | Where new supplier services are cloned or created. May differ from the reference root. |
| This engineering system | `<workspace-root>/supplier-integration-engineering-system` | The working copy of this repository. |

## Aggregator wiring surface

The files a new supplier touches in the aggregator. **This list is the boundary** — a change
outside it needs approval ([`docs/permission-boundaries.md`](../docs/permission-boundaries.md)).

### Derive the list yourself — do not trust this table blind

The surface is whatever your aggregator already does for a supplier that works. Pick one
supplier already wired end-to-end and grep the aggregator for its name:

```bash
grep -ril "<ExistingSupplierName>" <aggregator-root>/ --include="*.cs" --include="*.json" --include="*.proto"
```

Every file that comes back is a file the next supplier will touch. Anything that does *not*
come back is outside the boundary. Run the grep before wiring, not after — it is the only
statement of the surface that cannot go stale.

Fill the table from that output:

| File | Change |
| --- | --- |
| `<Enums/SupplierCodeEnum.cs>` | add the supplier's code constant |
| `<Protos/<supplier>_search.proto>` | add, mirroring the existing envelope |
| `<Extensions/SupplierGrpcExtensions.cs>` | register the gRPC client |
| `<Extensions/SupplierConfigurationExtensions.cs>` | add the env-var mapping |
| `<Extensions/ServiceExtensions.cs>` | register the wiring service |
| `<Services/<Feature>/Suppliers/<Supplier><Feature>Service.cs>` | add, subclassing the existing base |
| `<Models/<Feature>/Supplier/Request/<Supplier>Service<Feature>Request.cs>` | add, mirroring the existing shape |
| `<Services/<Feature>/Supplier<Feature>Constants.cs>` | add this supplier's config-key constants — **required, the wiring service will not compile without them** |
| `<appsettings*.json>` (×`<N>` environments) | add this supplier's block — **contains live credentials, see permission-boundaries** |

**Reference implementation:** `<path to the shortest existing wiring service>` — pick the one
that is pure wiring and carries no business logic; in the estate this system was derived from
that file is 73 lines. Read it before writing a new one. If your shortest wiring service is
several hundred lines, that is a finding: logic has leaked into the aggregator.

### ⚠ The `nameof` trap

Most wiring services write the supplier code as `SupplierCode => nameof(SupplierCodeEnum.<X>)`.
That returns the constant's **name**, and it only works because for most suppliers the name
happens to equal the value — in an estate of 17 supplier services we analysed, 16 of 17.

It breaks for any IATA code that cannot be a C# identifier — a code beginning with a digit, for
example. One supplier we analysed has such a code and correctly uses the enum **value**, not
`nameof`.

`[Certain]` Copying the `nameof` line for a digit-leading code routes silently under the wrong
supplier code. **Use the value unless you have checked that name and value match.**

Find your own exception before you need it:

```bash
grep -rn "nameof(SupplierCodeEnum" <aggregator-root>/
```

Cross-check each hit against the enum's declared values.

## Client contract source files

Copied into a new supplier service at bootstrap; **never authored**. Locate them once, record
them here, and treat the paths as fixed.

| What | Path (relative to `<AggregatorApiProject>/`) | How to find it |
| --- | --- | --- |
| Search request | `<Models/Search/Client/Request/SearchRequestDTO.cs>` | The DTO the aggregator's own search endpoint binds. |
| Credentials | `<Models/.../SupplierCredentialItem.cs>` | The credential item type passed down to every supplier service. |
| Search response payload | `<Models/Search/Supplier/Response/SupplierSearchResponseDTO.cs>` | The response type every supplier maps *into*. |
| Per-supplier request shape | `<Models/Search/Supplier/Request/<Supplier>ServiceSearchRequest.cs>` | Mirror the shape of an existing supplier's file, byte for byte. |

Transport: record what your aggregator actually uses, per feature — e.g. **gRPC for Search**
(thin `payloadJson`/`responseJson` envelope), REST for the rest.

## Reference services by shape

Which existing service to study for which supplier shape. Referenced from the skill's Phase 1
step 3; keep it current as services improve.

Fill one row per integration shape you actually have. The **Why** column is the load-bearing
one: name the single thing that service does better than the others, so a reader knows what to
copy and what to ignore.

| Shape | Study | Why |
| --- | --- | --- |
| NDC / reference-based | `<Supplier>-Air-Service` | e.g. best mapper decomposition in the estate — the benchmark from the estate analysed was 11 files, 187-line maximum |
| SOAP over mTLS | `<Supplier>-Air-Service` | e.g. certificate handling, `XmlWriter` request building, best documentation |
| SOAP, session-based | `<Supplier>-Air-Service` | e.g. explicit session open/close and caching |
| REST/JSON, token auth | `<Supplier>-Air-Service` | e.g. **and its test suite** — the testing reference; the benchmark from the estate analysed was 205 tests |
| GDS | `<Supplier>-Air-Service`, `<Supplier>-Air-Service` | shared GDS request shapes |

**Caveat, and it matters:** expect that no service in your estate is good at everything. In the
estate this system was derived from, the two services with the best architecture had **zero
tests**, and the service with the best tests had **zero documentation**. Study each for the
thing it does well, and nothing else. Record the full picture in
`docs/decision-records/`
— and write the caveat down, because a reference service with an unrecorded weakness gets
copied wholesale.

## Local ports

One row per service you run locally. Ports must be unique across the estate.

| Service | Port | How to find it |
| --- | --- | --- |
| `<aggregator>` | `<port>` | The `applicationUrl` in the project's `Properties/launchSettings.json`, or the hosting configuration. |
| `<Supplier>-Air-Service` | `<port>` | Same, per supplier service. |

**Stop any service you started once testing is finished** — a later manual run otherwise hits
a port conflict.

## Build and test

```bash
<build-command>   # e.g. dotnet build <proj> -clp:ErrorsOnly --nologo
<test-command>    # e.g. dotnet test  <testproj> --nologo
```

All services target `<target-framework>` — record the single value here (find it in any
project's `<TargetFramework>` element). **Do not introduce a second target framework.**

## Why the split

| Layer | Generic? | Reason |
| --- | :-: | --- |
| `docs/` (standards) | **yes** | A rule that says "do what supplier X does" is unusable anywhere else, and unusable here once X is retired. Rules are stated as rules. |
| `.claude/skills/` | **yes** | The workflow is the same regardless of estate. |
| `config/` | no | The installation-specific pointers — this file. |
| `knowledge/` | no | Supplier facts are inherently supplier-specific. That is the point of the folder. |
| `docs/decision-records/` | no | An ADR is a historical record of a decision in its context. Stripping the evidence ("the mapper was 2,028 lines") would make it a worse record, not a more portable one. |

To port this system elsewhere: copy this template to `config/environment.md` and fill it in,
empty `knowledge/suppliers/`, and start a fresh ADR series. `docs/` and `.claude/` carry over
unchanged.
