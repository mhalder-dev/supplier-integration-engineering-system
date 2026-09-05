# Human intervention

**Status:** active · **Applies to:** every agent and developer working in this system.

Escalation is a **feature of this system, not a failure of it**. An integration built on a
guess about supplier behaviour fails in production against real money and real passengers.
Stopping to ask is cheaper than any of the alternatives.

## When to stop

Stop and escalate whenever any of these is true:

| Trigger | Why guessing fails |
| --- | --- |
| Documentation is missing for a field or flow | Invented semantics silently corrupt bookings |
| Documentation contradicts itself | Picking a side without evidence is a coin flip |
| Observed API behaviour differs from the spec | Someone must decide which we build to |
| Sandbox credentials unavailable | Cannot verify anything; do not fabricate |
| Business rule unclear (markup, penalty, refund policy) | Commercial decision, not engineering |
| Several architecturally valid options exist | Sets precedent for future suppliers |
| A change would break an existing contract | Affects other services and the aggregator |
| The standard itself seems wrong | See [standards-governance](standards-governance.md) |
| You cannot determine correct behaviour safely | This is the catch-all; use it |

**Do not** escalate what you can determine yourself. Reading more of the spec, checking an
existing service, or capturing a sandbox response is investigation, not escalation. Escalate
after investigating, with the investigation attached.

## The escalation format

```
Problem:
  One sentence. What is blocked.

Evidence:
  What you checked and what you found. Cite specifics — spec section, file:line,
  captured response, supplier documentation URL. This is what makes the escalation
  answerable rather than a request to do your investigation.

Impact:
  What is blocked, and what is NOT blocked. State what you will continue with
  while waiting.

Possible options:
  1. <option> — consequence, cost, risk
  2. <option> — consequence, cost, risk
  3. <option> — consequence, cost, risk

Recommended option:
  Which one and why. Always give a recommendation. "You decide" wastes the
  investigation you just did.

Decision required:
  The single question needing an answer. One question, answerable in a sentence.
```

## A worked example

```
Problem:
  Cannot determine whether Revalidate must be called before Booking for supplier X.

Evidence:
  - Spec §4.2 p.31 shows Revalidate as "recommended", not required.
  - Spec §6.1 p.58 booking sample includes a PriceKey that only Revalidate returns.
  - Captured sandbox booking without a PriceKey: rejected, error PX-4021
    "missing price reference" (fixture: book-error-no-pricekey.json, 2026-09-05).
  - Five existing services call Revalidate first; two do not, and both of those
    only support one-way domestic.

Impact:
  Blocks the Booking slice. Search and Revalidate are unaffected and complete.
  Continuing with RetrieveBooking meanwhile.

Possible options:
  1. Always call Revalidate before Booking. Correct per observed behaviour;
     adds one supplier call and ~400ms to every booking.
  2. Call Revalidate only when no PriceKey is present. Faster; relies on the
     caller supplying a valid key, which we cannot verify.
  3. Ask the supplier to clarify. Authoritative; unblocks in days, not minutes.

Recommended option:
  1, plus 3 in parallel. The observed rejection is stronger evidence than the
  spec's "recommended", and 400ms on a booking is acceptable. Revisit if the
  supplier confirms option 2 is safe.

Decision required:
  Do we always call Revalidate before Booking for supplier X?
```

## After the decision

The decision is **not done when it is answered**. It is done when it is persisted.

1. Write an ADR in [`decision-records/`](decision-records/) — the decision, its context, the
   options considered, and the consequences.
2. If it is supplier behaviour, add it to `knowledge/suppliers/<supplier>.md`.
3. If it changes a standard, follow [standards-governance](standards-governance.md).
4. Link the ADR from the code if a reader would otherwise wonder why.
5. Continue.

A decision that exists only in a chat log will be re-litigated by the next developer, who
will reach a different conclusion. That is how seventeen services came to have seventeen
different `FlightResultDto`s.

## While waiting

Never idle, and never guess to keep moving:

- Do every part of the task that does not depend on the answer.
- Write the tests you already know are needed.
- Capture more fixtures.
- Document what you have learned so far.
- If the whole task is genuinely blocked, say so plainly and stop.

State clearly what you completed and what remains blocked. Scaling the work down is the
human's call, not yours.
