# ADR-0006 — 400-line file limit

**Status:** Accepted · **Date:** 2026-09-05

## Context

File sizes across an estate of 17 supplier services we analysed are bimodal. Medians are
healthy (29–48 lines); the tail is not. Twenty-five files exceed 1,000 lines and eight exceed
1,500, topping out at 2,028.

Two services in that estate demonstrate the limit is achievable on real supplier APIs:

| Service | Files | Max file | Files > 500 |
| --- | --: | --: | --: |
| Reference service A | 159 | 447 | 0 |
| Reference service B | 282 | 464 | 0 |

Reference service B is a SOAP integration with 18 features and hand-built XML. It holds every
one of its 282 files under 470 lines. This is not a toy service clearing a low bar.

## Decision

**400 lines per file.** Also: 300 per class, 40 per method, cyclomatic complexity 10, and 6
constructor parameters.

Exceeding a limit is not forbidden — it requires a comment explaining why decomposition would
be worse, reviewed in the pull request.

**Exempt:** DTOs generated from a supplier's WSDL or schema. They are not maintained by hand,
so the readability argument does not apply. Hand-written mappers are never exempt.

## Consequences

- **Positive:** files stay reviewable and testable; the god mapper cannot form gradually.
- **Negative:** more files; occasional friction where a genuinely cohesive unit runs long.
- **Note:** 400 is a judgement call, not a derived constant. It sits comfortably above what
  the two best services actually produce (447 and 464 max, medians near 30), so it constrains
  the tail without harassing normal code.

`[Opinion]` The specific number matters less than having one. An unbounded file grows.
