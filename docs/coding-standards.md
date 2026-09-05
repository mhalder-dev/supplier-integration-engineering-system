# Coding standards

**Status:** active · **Scope:** C# / .NET 9 supplier services · **Changes via:** [standards-governance](standards-governance.md)

Rules here are enforceable in review. Each carries its justification; a rule you cannot
justify is a rule you should propose removing.

## 1. Size limits

| Unit | Limit | Rationale |
| --- | --- | --- |
| File | **400 lines** | Demonstrably achievable: a 282-file SOAP service held every file under 470 lines. |
| Class | **300 lines** | Beyond this it has more than one reason to change. |
| Method | **40 lines** | Beyond this it needs a name for its parts. |
| Cyclomatic complexity | **10** | Beyond this it needs tests you will not write. |
| Constructor params | **6** | Beyond this the class does too much. |

Exceeding a limit is not forbidden — it requires a comment explaining why decomposition is
worse, reviewed as part of the PR. Generated DTOs (a SOAP envelope from a supplier's WSDL)
are exempt; hand-written mappers never are.

## 2. Naming

**Use the canonical feature vocabulary.** Without one, the same concept acquires several names
(`IssueTicket` *and* `Ticketing`; `CancelBooking` *and* `CancelPNR`; `Revalidate` *and*
`ReValidate`), a developer moving between services cannot predict folder names, and
cross-service search silently misses results. See
[ADR-0003](decision-records/ADR-0003-canonical-feature-vocabulary.md) for the measured case
that produced this rule — including a misspelling that propagated into sixteen services.

| Concept | Canonical | Never |
| --- | --- | --- |
| Flight search | `Search` | — |
| Re-price / confirm availability | `Revalidate` | `ReValidate`, `Reprice` |
| Branded fares / fare families | `BrandedFare` | `BrandedFares`, `GetBrandedFare`, `FareTag` |
| Create a booking | `Booking` | `CreatePNR`, `Book` |
| Issue a ticket | `IssueTicket` | `Ticketing`, `AirTicketingDetails` |
| Cancel a booking | `CancelBooking` | `CancelPNR`, `CancelPnr` |
| Retrieve a booking | `RetrieveBooking` | `GetBooking`, `RetrieveBookingDetails`, `LoadBooking` |
| Import an external PNR | `ImportPnr` | `ImportPNR` |
| Fare rules | `FareRule` | `FareRules` |
| Seat map / seat selection | `SeatMap` | `SeatMaps`, `Seats`, `SeatSelection` |
| Refund | `Refund` | — |
| Void a ticket | `VoidTicket` | `VoidFlight` |
| Logging | `Logging` | **`Loging`** |
| Authentication | `Auth` | `Authenticate` |

Acronyms follow .NET convention: `Pnr` not `PNR`, `Api` not `API`, `Id` not `ID`.

**Project and folder names must be spelled correctly.** A typo in a namespace is effectively
permanent — renaming after release is expensive enough that nobody does it. Spell-check before
the first commit; treat it as a review gate.

Types: `I<Feature>Service`, `<Feature>Service`, `<Feature>RequestModelBuilderService`,
`<Feature>ResponseMapper`, `<Feature>RequestValidator`, `<Concern>Builder`.

## 3. No magic values

Never inline a string or number that carries meaning.

```csharp
// Wrong
if (response.StatusCode == 409) return Error(response, "PNR already ticketed");

// Right
if (response.StatusCode == ServiceStatusCodes.Conflict)
    return Error(response, ServiceMessages.AlreadyTicketed);
```

Where a value belongs:

| Kind of value | Home |
| --- | --- |
| Varies by environment (URLs, credentials, timeouts) | `IOptions<T>` from `appsettings.*.json` |
| Fixed by the supplier's protocol (version, namespaces, code lists) | `Features/<F>/Constants/<F>Protocol.cs` |
| Shared across features (status codes, messages, journey types) | `Shared/Constants/` |
| Supplier identity (short code, name) | `SupplierConstants` — never inline `"XX"` |
| Used exactly once, locally | `private const` in the owning class |

## 4. Nullability

`<Nullable>enable</Nullable>` — already on in every service. Do not suppress it.

- Supplier responses are **hostile**. Every field is nullable until a captured response proves
  otherwise; a spec saying "mandatory" is not proof.
- `!` (null-forgiving) requires a comment saying what guarantees non-null.
- Prefer explicit defaults on client DTOs (`= string.Empty`, `= new()`) so consumers never
  face a null collection.

## 5. Error handling

**Services return envelopes; they do not throw at the caller.**

```csharp
try
{
    var validation = await validator.ValidateAsync(request, ct);
    if (!validation.IsValid)
        return ResponseBuilder.ValidationError(response, validation);

    var supplierResponse = await client.SendAsync(payload, ct);
    if (supplierResponse is null)
        return await ErrorAsync(response, ServiceMessages.NoSupplierResponse);

    return await SuccessAsync(mapper.Map(supplierResponse, request), ...);
}
catch (Exception ex)
{
    return await ErrorAsync(response, ServiceMessages.UnhandledException, ex.Message);
}
```

- **Never swallow**: `catch { }` and `catch { /* ignore */ }` are defects. If a failure is
  genuinely acceptable, log it at debug and comment why.
- Never catch a specific exception only to `throw ex;` — that erases the stack trace. Use
  `throw;`.
- Distinguish **transport failure** (retryable) from **domain rejection** (never retryable —
  the supplier said no, and asking again says no louder).

## 6. Dependency injection

- Constructor injection only. No service locator, no `IServiceProvider` in a feature class.
- Primary constructors are preferred: `public sealed class Foo(IBar bar) : IFoo`.
- Lifetimes: `Scoped` for anything touching a request; `Singleton` only for genuinely
  immutable state; `Transient` rarely. A `Singleton` that holds request state is a
  cross-request data leak.
- Register in `Extensions/ServiceConfiguration.cs`, grouped by feature with a comment header.

## 7. Async

- `async`/`await` throughout. Suffix async methods `Async`.
- Accept and propagate `CancellationToken`.
- **Never** `.Result`, `.Wait()`, `.GetAwaiter().GetResult()`.
- No `async void` except event handlers (there are none here).
- No I/O in constructors.

## 8. Immutability

- `sealed` by default. Unseal only for a designed inheritance point.
- `record` for DTOs that are genuinely values; `class` with init-only properties otherwise.
- Static builders for stateless mapping work — they cannot accumulate state by accident.

## 9. Comments

Comment the **why**, never the what.

```csharp
// Wrong:  // loop through segments
// Right:  // Prod tags one-way itineraries "1" while the spec shows "0", so group by the
//         // discriminator's value rather than assuming a 0-based index.
```

Every non-obvious supplier behaviour gets a comment **and** an entry in
`knowledge/suppliers/<supplier>.md`. The comment explains the line; the knowledge file
explains the supplier.

## 10. Dependencies

- Justify every new package in the PR description.
- Pin exact versions; keep them aligned across services.
- **Read the package identity before adding it** — author, download count, repository, last
  update. An autocomplete misfire can silently add a lookalike package that nothing imports;
  see lesson L-05 for a real instance across five production services.
- Remove a dependency when its last usage goes.

## 11. Security

- No credentials in source, config, tests, or fixtures. Redact fixtures before committing.
- No certificates (`.pfx`, `.pem`, `.key`) in git. Committed means compromised — see L-07.
- `.gitignore` must cover `LocalTests/`, `**/credentials*.json`, `*.pfx`, `*.pem`, `raw/`.
- Log supplier payloads to blob storage, not to stdout, and redact credential fields on the
  way in.
