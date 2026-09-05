# Supplier Integration Engineering System

An engineering system for building flight-supplier integrations to one enforced standard: the
standards themselves, a prompt-driven workflow that applies them, the knowledge accumulated about
each supplier, and the decision record behind every rule.

This repository does not contain supplier services. It contains the system that governs them. Drop
it beside your integration repositories and it works — nothing in it is tied to a particular
organisation, supplier, or machine.

## The problem it solves

In most organisations the knowledge of how to build a supplier integration lives in individual
developers' heads and is transmitted by copy-pasting the last supplier. That mechanism fails
quietly, and it is measurable. In an estate of 17 supplier services we analysed (~281,000 lines of
C#):

- one shared client DTO existed as **17 divergent copies**, no two with the same hash, property
  counts ranging from **4 to 23**
- a misspelled feature folder had propagated into **16 of the 17 services**
- one response mapper had accumulated to **2,028 lines** in a single file; another service did the
  same job in 11 files with a 187-line maximum
- a lookalike package added by a single autocomplete misfire was carried into **five production
  services**, imported by nothing
- credentials sat in git-tracked configuration

Nobody decided any of that. Every individual step was small and locally reasonable. This system
exists so the small steps are never needed.

## Who it is for

- Teams running an **aggregator** that fans out to multiple airline or consolidator APIs.
- Anyone adding the second, fifth, or twentieth supplier and finding that each one is built
  differently from the last.
- Teams using AI coding agents on integration work, who need the agent bounded by a written
  standard rather than by whatever it inferred from the last file it read.

It assumes .NET and C#, a per-supplier service, and a shared client contract owned upstream. The
standards, workflow and lessons transfer to other stacks; the scaffolding does not.

## What you get

| | |
| --- | --- |
| **Standards** | Architecture, coding standards, recurring supplier patterns, testing strategy, branch/PR rules — each traced to evidence, or marked as opinion where it is opinion |
| **A workflow** | Four skills that carry the procedure: route, bootstrap, feature, wiring |
| **A permission model** | An explicit green/red boundary for what an agent or developer may change without asking |
| **A knowledge structure** | Template and rules for per-supplier files, plus fourteen lessons derived from analysing a real 17-service estate |
| **Decision records** | Why each rule exists, so it can be argued with rather than obeyed |
| **A verified template** | A bootstrap reference service that builds clean with 20 passing tests |
| **An escalation format** | A structured way to stop on an unknown instead of guessing at it |

## Install

Requires Windows with PowerShell, and Claude Code (or any agent that reads `AGENTS.md`).

```bash
git clone <this-repo-url> <workspace-root>/supplier-integration-engineering-system
```

```powershell
cd <workspace-root>/supplier-integration-engineering-system
.\install\install.ps1
```

The installer links the skills into `~/.claude/skills/` with directory junctions — no
administrator rights needed, and `git pull` here updates them everywhere. Without this step a
developer working inside a *supplier* repository cannot see the skills at all, which is the
single most common setup failure.

Verify: open Claude Code in any folder, type `/`, and four commands should be offered.

```
/supplier-development    route me to the right stage
/supplier-bootstrap      create a new supplier service
/supplier-feature        implement one feature end to end
/aggregator-wiring       wire a feature into the aggregator
```

Then copy [`config/environment.example.md`](config/environment.example.md) to
`config/environment.md` and fill in your own paths and ports. That is the only file expected to
name machines, organisations, or directories — everything else stays portable.

Full setup, including what to read and in what order:
[`docs/developer-onboarding.md`](docs/developer-onboarding.md).

## Using it

The system is **prompt-driven**. A developer says what they want; the skill carries the procedure:

```
"create the <Supplier> supplier project"      ->  supplier-bootstrap
"add search to <Supplier>"                    ->  supplier-feature
"wire <Supplier> search into the aggregator"  ->  aggregator-wiring
```

Features are built **one at a time**, each verified against the real API before the next starts:
Auth → Search → Revalidate → Booking → IssueTicket → the rest.

### The rule that shapes everything

**The client contract is fixed.** The aggregator owns it and every supplier service shares it. A
supplier service translates *into* it and may not change it. If a supplier returns something with
no home, that is an escalation, not a commit — see
[`docs/permission-boundaries.md`](docs/permission-boundaries.md).

The same applies in the aggregator: **wiring only.** You may add a supplier to lists that already
exist; you may not add business logic, abstractions, or DTO fields.

## Start here

| You want to… | Read |
| --- | --- |
| **Set up your machine (do this first)** | [`docs/developer-onboarding.md`](docs/developer-onboarding.md) |
| **Build a supplier — start here** | [`.claude/skills/supplier-development/SKILL.md`](.claude/skills/supplier-development/SKILL.md) |
| Create a new supplier project | [`.claude/skills/supplier-bootstrap/SKILL.md`](.claude/skills/supplier-bootstrap/SKILL.md) |
| Implement one feature | [`.claude/skills/supplier-feature/SKILL.md`](.claude/skills/supplier-feature/SKILL.md) |
| Wire a feature into the aggregator | [`.claude/skills/aggregator-wiring/SKILL.md`](.claude/skills/aggregator-wiring/SKILL.md) |
| **Know what you may NOT change** | [`docs/permission-boundaries.md`](docs/permission-boundaries.md) |
| Know the client contract | [`contracts/client-contract.md`](contracts/client-contract.md) |
| Know branch and PR rules | [`docs/branching-and-pr.md`](docs/branching-and-pr.md) |
| Know how a service is structured | [`docs/architecture.md`](docs/architecture.md) |
| Know how to write the code | [`docs/coding-standards.md`](docs/coding-standards.md) |
| Know the recurring shapes (search, book, ticket…) | [`docs/supplier-patterns.md`](docs/supplier-patterns.md) |
| Know what "tested" means here | [`docs/testing-strategy.md`](docs/testing-strategy.md) |
| Escalate something you cannot decide | [`docs/human-intervention.md`](docs/human-intervention.md) |
| Change a standard | [`docs/standards-governance.md`](docs/standards-governance.md) |
| Know why a rule exists | [`docs/decision-records/`](docs/decision-records/) |
| Know how a specific supplier behaves | [`knowledge/suppliers/`](knowledge/suppliers/) |
| Know what went wrong before | [`knowledge/lessons-learned.md`](knowledge/lessons-learned.md) |
| Know what is being worked on now | [`development/active-supplier.md`](development/active-supplier.md) |
| Find the paths/ports for **this** installation | [`config/environment.example.md`](config/environment.example.md) |
| See what bootstrap produces | [`reference/bootstrap-reference/`](reference/bootstrap-reference/) |

Agents of any kind — Claude Code, Cursor, Copilot, Codex — start at [`AGENTS.md`](AGENTS.md).
Claude Code additionally reads [`CLAUDE.md`](CLAUDE.md).

## What it is built on

Seventeen existing supplier services (~281,000 lines of C#) were analysed before a single standard
was written. Every rule in `docs/` traces to evidence in that analysis, recorded in
the decision records in `docs/decision-records/`.
Where a rule is opinion rather than evidence, it says so.

The analysis found **no single exemplary service**. Three different services were best at three
different things — one at architecture, one at documentation, one at testing — and none was good
at all three. The two held up internally as reference implementations both had zero tests.

The standard is therefore a **synthesis**, not a copy of any one service. That is the whole reason
this repository exists.

## Non-negotiables

1. Knowledge lives in this repository, not in a conversation. If it is only in a chat log, it is
   lost.
2. The standard changes by proposal and decision, never silently. See
   [`docs/standards-governance.md`](docs/standards-governance.md).
3. Unknowns get escalated, never guessed. See
   [`docs/human-intervention.md`](docs/human-intervention.md).
4. Credentials are never fabricated, never committed, never taken from production.

## Limitations

**Usable, not finished.** Read [`development/readiness.md`](development/readiness.md) before
relying on this system; it lists what is proven and what is not. In summary:

- **Complete:** standards, workflow, permission model, knowledge structure, decision records.
- **Complete and verified:** the service template — it builds clean and its 20 tests pass.
- **Thinner than it looks:** the write path (Booking, IssueTicket) has documented rules but **no
  worked example in code**. SOAP suppliers get the standards but no scaffold.
- **Never yet proven:** no supplier has been taken end-to-end against a live API under this
  system. The proof-of-concept is in progress and blocked on sandbox credentials.
- **Stack-bound:** the scaffolding assumes .NET 9 and C#. The standards and lessons are portable;
  the templates are not.
- **Windows-bound installer:** the skills installer is PowerShell and uses directory junctions.
  Linking the skills by hand on another OS is a two-line job, but it is not scripted here.
- **Not a substitute for supplier documentation.** The system enforces how you integrate; it
  cannot tell you what a supplier's API does. That still requires their spec and a sandbox.

A system that oversells itself gets trusted in exactly the places it should not be.
