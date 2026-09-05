# CLAUDE.md — Supplier Integration Engineering System

This repository is the **governing standard** for flight-supplier integrations. When you
work on a supplier service, this repository decides *how*; the supplier's own repository is
only *where*.

## What this repo is (and is not)

- **Is**: standards, workflow, supplier knowledge, decisions, lessons.
- **Is not**: the supplier services themselves. Concrete paths for this installation are in
  `config/environment.md`, written from
  [`config/environment.example.md`](config/environment.example.md) — the only file that names
  machines, organisations, or directories.

**Never write engineering-system files into a supplier service tree**, and never modify an
existing supplier service unless the task explicitly says to.

## Operating mode

Operate as a senior engineer, not an order-taker: challenge a weak assumption before acting,
tag every claim with its confidence, lead with the uncomfortable finding, and hold a position
under push-back unless given new evidence.

Two additions specific to this repo:

- **Tag every claim about a supplier's API** with its source: `[spec §x.y]`, `[observed in
  <fixture>]`, `[prod-verified]`, or `[assumed]`. An untagged supplier claim is a defect.
- **Never invent supplier behaviour.** If the spec is silent and you have no captured
  response, that is an escalation (`docs/human-intervention.md`), not an inference.

## The workflow

Supplier work runs through the skill at
[`.claude/skills/supplier-development/SKILL.md`](.claude/skills/supplier-development/SKILL.md).
Invoke it rather than improvising. It routes to three sub-skills — bootstrap, feature, wiring — and carries the rules common to all three.

## Reading order before touching any supplier code

1. [`docs/architecture.md`](docs/architecture.md) — the shape of a service.
2. [`docs/supplier-patterns.md`](docs/supplier-patterns.md) — the shape of a feature.
3. [`docs/coding-standards.md`](docs/coding-standards.md) — the shape of the code.
4. [`knowledge/suppliers/<supplier>.md`](knowledge/suppliers/) — what we know about this one.
5. [`knowledge/lessons-learned.md`](knowledge/lessons-learned.md) — what already bit us.

Do not skip 5. Most of it was derived from analysing a real estate of supplier services or wasted weeks.

## The three rules that this system exists to enforce

### 1. Knowledge is persisted, not remembered
A discovery that lives only in a conversation is lost. The moment you learn a supplier
quirk, a spec-vs-production difference, or a business rule, write it to
`knowledge/suppliers/<supplier>.md`. Not at the end of the task — when you learn it.

### 2. The standard changes by decision, not by drift
If you find a better convention while implementing, **do not apply it silently**. Follow
[`docs/standards-governance.md`](docs/standards-governance.md): propose → human decides →
ADR recorded → standard updated → applied consistently. The failure mode this prevents is
seventeen supplier services each carrying its own "improved" convention — the observed state
of an estate of 17 supplier services we analysed.

### 3. Unknowns escalate
Missing docs, contradictory docs, ambiguous API behaviour, absent sandbox credentials,
unclear business rules, or several equally valid designs — all stop work and raise a
structured escalation. Guessing at supplier behaviour produces integrations that fail in
production against real money.

## Definition of done

A supplier feature is done when **all** of these hold. "It compiles" is not on the list.

- [ ] 0 build errors, 0 warnings (`dotnet build <proj> -clp:ErrorsOnly --nologo`, with
      `TreatWarningsAsErrors=true` so a warning cannot pass unnoticed)
- [ ] Validator at the request boundary, running before any field is read
- [ ] No magic strings or numbers — constants or config, per `docs/coding-standards.md`
- [ ] Thin service + thin mapper + dedicated builders; no file over 400 lines
- [ ] Logging wired (both client and supplier, request and response)
- [ ] Every mapped field traced to spec or to a captured fixture
- [ ] Tests written **and executed**, with output shown
- [ ] `knowledge/suppliers/<supplier>.md` updated with anything learned
- [ ] Any decision taken recorded as an ADR
- [ ] The real flow driven end to end and observed — not just compiled

## Environment

- Windows. PowerShell primary; Git Bash available. Do not mix their syntax.
- Native Windows tools do not understand POSIX-style `/c/...` paths — pass a Windows-style
  drive path instead.
- .NET 9 (`net9.0`) across every existing service. Do not introduce a second target.
- Services run locally on their own ports — see `config/environment.md`, written from
  [`config/environment.example.md`](config/environment.example.md).
  **Stop any service you started once testing is finished.**
