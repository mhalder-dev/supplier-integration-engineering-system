# Supplier patterns

**Status:** active · **Purpose:** the recurring shapes every supplier integration takes.

Supplier APIs differ enormously in protocol (REST/JSON, SOAP/XML, NDC) and in vocabulary.
They differ far less in *shape*. This document captures the shapes so each new integration
is a variation on a known theme rather than a fresh invention.

## 1. The booking funnel

Almost every supplier exposes some version of this sequence:

```
Search  ->  Revalidate  ->  Booking (hold)  ->  IssueTicket  ->  [ Cancel | Refund | Void ]
   |            |                 |                   |
 many         one offer        PNR created      ticket numbers
 offers       re-priced        money not taken   money taken
```

The transitions matter more than the steps:

- **Search to Revalidate**: prices in search results are *indicative*. Revalidate is what the
  supplier will actually honour. Never book from a search price.
- **Revalidate to Booking**: creates a PNR, usually without payment. Recoverable.
- **Booking to IssueTicket**: **the irreversible step.** Money moves. Never auto-retry.
- After ticketing, only Cancel/Refund/Void apply, each with supplier-specific time windows
  and penalties.

**Consequence for retry policy:** retry is safe left of Booking, unsafe at Booking, and
forbidden at IssueTicket. Encode this per feature, never globally.

## 2. Feature archetypes

Six shapes cover every feature in the analysed ecosystem. Feature names used below are the
canonical ones from [`coding-standards.md §2`](coding-standards.md); an archetype never
licenses a name outside that table.

### A. Read-many (Search)
Many offers out. The heaviest mapping in the service — this is where god mappers form.
Requires full builder decomposition from the first commit.
**Pipeline:** validator → model builder → client → mapper (decomposed) → list of results.

### B. Read-one (RetrieveBooking, FareRule)
One entity out. Simple mapping. Safe to retry.

### C. Re-price (Revalidate, BrandedFare)
Takes a search result, asks the supplier to confirm it. Often **cache-read**: some suppliers
let you re-read a cached search rather than re-calling. Those features legitimately have
**no ModelBuilder and no supplier request** — that is correct, not a missing piece.

### D. Write-reversible (Booking, SeatMap where selecting a seat reserves it)
Creates or amends supplier state that can be undone — a booking held before ticketing, a seat
reserved, an itinerary changed while no money has moved. Log the full request and response.
Retry only with an idempotency key the supplier honours; if it has none, do not retry.

### E. Write-irreversible (IssueTicket, Refund, VoidTicket)
Money moves. **Never retry automatically.** Log everything. On an ambiguous timeout, do not
re-issue — retrieve the booking and determine actual state first. See lesson L-06.

### F. Session/auth (Auth, plus the token or session cache that supports it)
Not a business feature; infrastructure. Covered below.

## 3. Authentication

Four patterns seen; pick by what the supplier offers.

| Pattern | Notes |
| --- | --- |
| Token endpoint + expiry | Most common. Cache it — see below. |
| Per-request credentials in the payload | No caching possible; the credential travels with every call. |
| mTLS client certificate | Certificate must be loadable on the host; see the Windows note below. |
| Stateful session token | Needs explicit open/close and a session cache. |

### Token caching — the required shape

A token endpoint called on every request will get you rate-limited. The required shape:

```csharp
private static readonly ConcurrentDictionary<string, CachedToken> Tokens = new();
private static readonly ConcurrentDictionary<string, SemaphoreSlim> Gates = new();

// 1. fast path: usable cached token -> return it
// 2. take a per-credential-key gate (not a global lock)
// 3. re-check inside the gate (double-checked locking)
// 4. fetch, cache with expiry, release
```

Three properties that matter and are easy to get wrong:

1. **Key by credential set**, not globally — one service handles multiple agency accounts.
2. **Gate per key**, not one global lock — otherwise account A's fetch blocks account B.
3. **Re-check inside the gate** — otherwise every concurrent caller fetches anyway.

**Known limitation** `[Certain]`: a `static` dictionary is process-local. Tokens are lost on
restart and not shared between instances. Acceptable at one instance per supplier; if these
services are ever scaled horizontally this needs a distributed cache. Tracked as an open
question in `development/progress.md`.

**Expiry safety margin**: expire the cached token 60–120s before the supplier's stated
expiry. A token that expires in flight produces a confusing mid-transaction 401.

## 4. Protocol handling

### REST / JSON
Default. `System.Text.Json` with `PropertyNameCaseInsensitive = true` — suppliers are
inconsistent about casing and will change it without telling you.

### SOAP / XML
Many airline and GDS APIs are SOAP. Two viable approaches:

- **Generated from WSDL** — fast to start, produces very large generated DTOs (1000+ lines is
  normal). Acceptable *because generated*; exempt from size limits.
- **Hand-built with `XmlWriter`** — full control, no generated bloat, but every element is
  hand-verified against the spec.

`[Opinion]` Prefer generated DTOs for reading and hand-built XML for writing: reading
benefits from the supplier's own schema, writing benefits from control over element order,
which several SOAP suppliers enforce strictly.

**Windows gotcha** `[Certain, prod-verified]`: `curl` cannot perform an mTLS handshake on
Windows — Schannel will not present the client certificate. Drive certificate auth from
PowerShell with `X509Certificate2` and `Invoke-WebRequest -Certificate`.

### NDC
Reference-based: the response carries a data map and offers refer into it by id rather than
embedding data. **Do not thread that map through twenty method parameters.** Build a
`ReferenceData` lookup object once and inject it into the builders. Threading it manually is
the single most reliable way to produce an unmaintainable NDC mapper.

## 5. Reference data

Airport, airline, and aircraft lookups are needed by every supplier for name resolution and
timezone conversion. This is the single most commonly duplicated thing across supplier
services.

**Standard:** a single injected `IReferenceData` per service, loaded once, keyed
case-insensitively. **Load it asynchronously** — see lesson L-03 for what happens otherwise.

**Target state:** one shared package. See [ADR-0001](decision-records/ADR-0001-shared-contracts-package.md).

## 6. Timezone handling

Suppliers return local times without offsets, almost universally. Converting them requires
the airport's timezone, which comes from reference data.

- Convert to an explicit offset (`yyyy-MM-ddTHH:mm:sszzz`) at the mapper boundary.
- Never assume the server's local timezone. These services run in containers whose timezone
  is not the airport's.
- An unknown timezone is a **logged warning**, not a silent fallback. Swallowing it in an empty
  `catch` hides bad reference data indefinitely.

## 7. Money

- `decimal` only. Never `double` or `float`.
- Always carry the currency code with the amount. A bare number is a bug waiting for a
  multi-currency supplier.
- Never round in the mapper. Preserve what the supplier sent; rounding is a presentation
  decision that belongs upstream.
- Per-passenger versus total is a classic defect source: suppliers routinely return both in one
  response, under names that do not make the difference obvious. Name **our** DTO fields so the
  distinction is impossible to miss.

## 8. Logging

Four log points per feature, always:

```
ClientXRequest  ->  SupplierXRequest  ->  SupplierXResponse  ->  ClientXResponse
```

Both sides of both boundaries. When a supplier disputes behaviour, the raw supplier
request/response pair is the only evidence that settles it. Logging is asynchronous (queue
plus background worker) so it never blocks a request, and credentials are redacted before the
payload is queued.
