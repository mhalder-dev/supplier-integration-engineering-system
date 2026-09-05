# Supplier knowledge

One file per supplier, holding everything known about how that supplier's API actually
behaves. Start from [`_TEMPLATE.md`](_TEMPLATE.md).

This is **descriptive** knowledge — it records what is true, and it is updated freely as we
learn. Contrast with `docs/`, which is **prescriptive** and changes only by decision. See
[standards-governance](../../docs/standards-governance.md).

## This folder ships empty

It ships with exactly two files — this README and [`_TEMPLATE.md`](_TEMPLATE.md) — and no
supplier files at all. **Supplier knowledge never ships with the product.** It is specific to
one organisation's contracts, credentials, sandbox access and commercial terms, it is
frequently covered by a supplier non-disclosure agreement, and it is worthless — or actively
misleading — to anyone whose agreements with that supplier differ.

Populate it per installation:

1. Copy [`_TEMPLATE.md`](_TEMPLATE.md) to `<supplier>.md` — lower case, one file per supplier.
2. Fill it in **while the integration is being built**, not afterwards. A file written from
   memory at the end of a project is the file nobody trusts.
3. Add a row to the index below so the next reader can find it.

Write a supplier's file when someone actually works on that supplier. Writing one
speculatively, without touching the code or calling the API, produces documentation nobody has
verified — worse than none, because it reads as though someone had.

## Index

| File | Supplier | Role |
| --- | --- | --- |
| [_TEMPLATE.md](_TEMPLATE.md) | — | Template for a new supplier |

_Add one row per supplier file as it is written._

## The source-tag rule

Every claim about a supplier's API carries its source:

| Tag | Meaning |
| --- | --- |
| `[spec §x.y]` | Stated in the supplier's documentation |
| `[observed in <fixture>]` | Seen in a captured response |
| `[prod-verified]` | Confirmed against production behaviour |
| `[assumed]` | An assumption — must have an owner and a resolution path |

**An untagged claim is a defect.** The next reader cannot tell whether it was verified or
invented, which makes the whole file untrustworthy.

Where the spec and observed behaviour disagree, **observed wins**, and the disagreement is
recorded with a date. See [L-11](../lessons-learned.md) — this has already caused real bugs.
