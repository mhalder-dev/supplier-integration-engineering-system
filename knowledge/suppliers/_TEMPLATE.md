# <Supplier name>

**Service:** `<Supplier>-Air-Service` · **Protocol:** REST/JSON | SOAP/XML | NDC
**Status:** research | in development | live · **Last updated:** YYYY-MM-DD

Every claim below carries a source tag: `[spec §x.y]`, `[observed in <fixture>]`,
`[prod-verified]`, or `[assumed]`. **An untagged claim is a defect** — it cannot be trusted or
re-verified by the next reader.

## Summary

Two or three sentences. What this supplier is, what we sell through it, anything structurally
unusual about them.

## Access

| | |
| --- | --- |
| Developer portal | <url> |
| API documentation | <url> |
| Sandbox base URL | <url> |
| Production base URL | <url> |
| Credentials held by | <person or system — **never the values**> |
| Onboarding requirement | e.g. self-service / IATA agency contract / partner agreement |

## Authentication

- **Mechanism:** token endpoint | per-request credentials | mTLS | session
- **Token lifetime:** <value> `[source]`
- **Caching:** how, and the safety margin used
- **Gotchas:** anything that has actually bitten us

## Features

| Feature | Endpoint | Implemented | Notes |
| --- | --- | :-: | --- |
| Search | | | |
| Revalidate | | | |
| Booking | | | |
| IssueTicket | | | |
| CancelBooking | | | |
| Refund | | | |
| RetrieveBooking | | | |

## Request and response behaviour

What the wire actually looks like, especially where it surprises. Structure per feature.

### Search
- Request shape and required fields `[source]`
- Response structure — flat list, grouped, reference-based? `[source]`
- How offers map to our `FlightResultDto`
- Pagination or result limits `[source]`

### Booking
- What creates a PNR, what it returns, whether payment is involved `[source]`
- Idempotency: does the supplier support a key? `[source]`

### IssueTicket
- **Irreversible.** Document exactly what a timeout means here `[source]`
- How to determine actual state after an ambiguous response

## Error handling

| Code | Meaning | Retryable | Notes |
| --- | --- | :-: | --- |

Distinguish transport failures from domain rejections. A domain rejection re-sent is still a
rejection.

## Limits

Passengers, segments, request size, rate limits, session lifetime. `[source]` each.

## Spec-versus-production differences

The highest-value section in this document. Every case where the documentation and the real
API disagree.

| Field / behaviour | Spec says | Production does | Verified | Date |
| --- | --- | --- | --- | --- |

**Production wins.** Record it here so the next developer does not "fix" the code back to the
spec. See lesson L-11.

## Known limitations

What this supplier cannot do, or does badly. Include things we chose not to implement and why.

## Fixtures

| Fixture | Scenario | Captured from | Date |
| --- | --- | --- | --- |

All fixtures redacted before commit. Shape preserved, values replaced.

## Open questions

| Question | Status | Owner | Raised |
| --- | --- | --- | --- |

Escalations that have not been answered yet. See
[human-intervention](../../docs/human-intervention.md).

## Decisions

ADRs that concern this supplier, linked with a one-line summary.

## Change log

| Date | Change |
| --- | --- |

Supplier APIs change without notice. Date every discovery so a stale entry is recognisable.
