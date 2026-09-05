# Progress

Running record of what this system has done and what state it is in. Newest first.

Current readiness: [`readiness.md`](readiness.md).
Supplier currently in flight: [`active-supplier.md`](active-supplier.md).

**This log starts empty.** Add an entry whenever a phase closes, a supplier is delivered, a
standard changes, or a finding is significant enough that the next person needs it. Copy the
entry template to the **top** of the log — newest first. Never rewrite a past entry; mark it
*(superseded — see the entry above)* instead, so the record of what was believed at the time
survives.

An index that is not updated in the same commit as the thing it indexes will be wrong. Update
this file in the commit that changes the state it describes, not afterwards.

---

## Log

*(no entries yet)*

---

## Entry template

````markdown
## YYYY-MM-DD — <short title: what changed>

### Done

What was actually built or decided, with the verification attached — "0 errors, 0 warnings,
N tests passing", not "should build". A claim with no observed evidence behind it does not
belong in this section.

### Findings

Numbered. Each is a mechanism plus what it costs the next person. Anything reusable is also
written into `knowledge/lessons-learned.md` in the same commit, with the lesson identifier
quoted here.

### Blocked

What cannot proceed, and the escalation it is recorded under in `active-supplier.md`.

### Not done — deliberately

Scope that was declined, and on whose decision. Recording it stops it being re-litigated
silently, and stops a later reader mistaking a decision for an oversight.
````

---

## Open questions carried forward

Questions raised by the work that only a human owner can answer. A question leaves this table
by being answered in place — struck through, with the answer and its date — never by deletion.

| # | Question | Owner | Raised |
| --- | --- | --- | --- |
| | | | |

---

## Assumptions on record

Anything the work is built on that has been stated rather than verified, because verifying it
needs access or knowledge outside the work. State the risk if it is wrong. An assumption that
is load-bearing enough to invalidate a deliverable is not an assumption — it is a blocker, and
belongs in the escalation in [`active-supplier.md`](active-supplier.md).

| # | Assumption | Risk if wrong |
| --- | --- | --- |
| | | |
