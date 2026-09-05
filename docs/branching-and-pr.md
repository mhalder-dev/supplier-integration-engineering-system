# Branching and pull requests

**Status:** active · **Applies to:** supplier services and the aggregator.

A supplier feature is almost always **two repositories**: the supplier service that does the
translation, and the aggregator that routes to it. They branch and merge independently but ship
together.

## Branch names

| Repository | Branch |
| --- | --- |
| Supplier service | `feature/<feature-name>` |
| Aggregator | `feature/<supplier-name>/<feature-name>` |

Feature names use the **canonical vocabulary** from
[`coding-standards.md §2`](coding-standards.md) — `feature/search`, `feature/issue-ticket`,
`feature/cancel-booking`. Not `Ticketing`, not `CancelPNR`.

The aggregator branch carries the supplier name because one aggregator branch touches only one
supplier, and several suppliers may be in flight at once:

```
supplier repo:    feature/search
aggregator repo:  feature/<supplier-name>/search
```

## Always branch from updated staging

**Before creating any branch**, refresh from the remote. Branching from a stale local `staging`
is how a feature arrives already conflicting.

```bash
git fetch origin
git checkout staging
git pull origin staging
git checkout -b feature/<feature-name>
```

For the aggregator, identically, with `feature/<supplier-name>/<feature-name>`.

**Never branch from another feature branch.** If your work genuinely depends on unmerged work,
that is an escalation — it usually means the two features should be one.

## One feature per branch

A branch delivers one feature end to end: supplier DTOs, request builder, service, mapper,
tests, and the aggregator wiring that reaches it. Not half a feature, and not three.

If you find something unrelated that needs fixing, note it and leave it. A branch that fixes
"just one more thing" is a branch nobody can review.

## Commits

- Scoped to the change. The message says **what changed and why**, not "wip" or "fixes".
- Author is the current git user. **No AI attribution trailers** — no `Co-Authored-By`, no
  generated-by footer.
- Commit when a piece works, not once at the end. A branch with one enormous commit cannot be
  bisected.

## Pull requests

**Target: `staging`.** Both repositories.

Open with `gh`:

```bash
gh pr create --base staging --title "<Feature>: <supplier>" --body "..."
```

### The PR body must state

1. **What was built** — the feature and its scope
2. **Test results** — actual output, not "tests pass"
3. **What was verified against the real API**, and what was not
4. **Assumptions** still in force
5. **The paired PR** — link the supplier PR from the aggregator PR and vice versa
6. **Anything that needed human approval** and the ADR recording it

### The paired-PR rule

The two PRs reference each other and **merge together**. An aggregator PR merged without its
supplier PR routes traffic to a service that cannot answer; the reverse ships a supplier
nothing calls.

State the merge order explicitly in both bodies. Usually supplier first — a deployed supplier
service that nothing calls is harmless; a caller with no service is not.

## What must be true before opening a PR

From the definition of done in `CLAUDE.md`:

- [ ] 0 build errors, 0 warnings
- [ ] Tests written **and executed**, output pasted in the PR
- [ ] The real flow driven and observed — not just compiled
- [ ] `knowledge/suppliers/<supplier>.md` updated with anything learned
- [ ] Any decision recorded as an ADR
- [ ] No credentials in the diff — check the diff, not just your intent
- [ ] Nothing in the aggregator diff outside the wiring surface
      ([permission-boundaries](permission-boundaries.md))

**Review your own aggregator diff against the wiring surface before opening the PR.** That is
the diff most likely to contain something that needed approval and did not get it.

## Merging

PR to `staging`, merged directly once approved. No long-lived integration branch.

After merge, delete the feature branch in both repositories.

## When something is blocked mid-branch

Do not leave a half-feature on a branch indefinitely and do not merge it to "unblock". Push the
branch, record the blockage in
[`development/active-supplier.md`](../development/active-supplier.md) with the escalation, and
say plainly what is done and what is not.

A branch that is honestly parked is recoverable. A half-feature merged into `staging` is not.
