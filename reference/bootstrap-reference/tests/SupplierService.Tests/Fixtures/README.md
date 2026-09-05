# Fixtures

Captured supplier responses, replayed by the offline tests (ADR-0004).

## Rules

1. **Captured from a real sandbox call** — never hand-authored to match the mapper.
2. **Redacted before commit** — replace credential and PII *values*, preserve the *shape*.
3. **Named for the scenario**: `search-oneway.json`, `search-error.json`.
4. **Provenance recorded below.** A fixture whose origin is unknown is worth little once the
   supplier changes format.

## Inventory

| Fixture | Scenario | Captured from | Date |
| --- | --- | --- | --- |
| `search-oneway.json` | One-way, two offers, deliberately out of price order | **synthetic — template placeholder** | 2026-09-05 |
| `search-error.json` | Supplier domain rejection | **synthetic — template placeholder** | 2026-09-05 |

**Both fixtures here are synthetic placeholders** so the template's tests run out of the box.
They demonstrate the shape only. **Delete them and capture real ones** as the first task when
implementing a supplier — a synthetic fixture tests your understanding, not the supplier.
