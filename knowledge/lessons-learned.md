# Lessons learned

Things that cost us something. Each entry: what happened, why it happened, what to do
differently. Add to this file whenever a bug, incident, or wasted afternoon teaches something
that would not be obvious to the next developer.

Lessons L-01 to L-10 are derived from a 2026-09-05 analysis of an estate of 17 supplier
services, not from incidents witnessed live. They are tagged with their evidence.

---

## L-01 — Copying a service copies its mistakes, permanently

**Evidence** `[Certain]`: Across the 17 services analysed, the misspelt feature folder `Loging`
(one 'g') exists in **16 of the 17**. Only one service spells it `Logging`.
Similarly: two service directories end `-Air-Servcie` instead of `-Air-Service`, a feature
folder reads `RetrieveBookiongDetails`, and a git branch is named `stagng`.

**Why:** New services are created by copying the previous one. A typo made once in 2024 is
now in sixteen production namespaces, where renaming it is expensive enough that nobody will.

**Do instead:** Create new services from a template ([ADR-0005](../docs/decision-records/ADR-0005-no-new-service-by-copy.md)).
Study existing services for patterns; never copy their files. Spell-check folder and project
names before the first commit — they become permanent within a day.

---

## L-02 — A duplicated contract will drift, and nobody will notice

**Evidence** `[Certain]`: `FlightResultDto` exists in all 17 services with **17 different MD5
hashes**. Property counts range 4 to 23. `SupplierResponseJson` is in 7 of 15 comparable
copies; `Headers` in 1; `IsPassportRequired` in 1.

**Why:** Each service owns its own copy, so each team adds what it needs. No mechanism ever
compares them. The aggregator absorbs the inconsistency by being defensive per supplier.

**Do instead:** One definition in a shared package
([ADR-0001](../docs/decision-records/ADR-0001-shared-contracts-package.md)). Until that
exists, assert the output against a committed JSON schema so drift fails a test instead of
accumulating silently.

---

## L-03 — Sync-over-async in a constructor is a latent deadlock

**Evidence** `[Certain]`: In one service we analysed, a reference-data loader
(`Features/Search/Mappers/Shop/ReferenceData.cs`) calls `.GetAwaiter().GetResult()` three times
in its constructor, and is registered as a singleton.

**Why:** Loading reference data feels like setup, and constructors cannot be async, so blocking
looks like the only option. On the ASP.NET Core thread pool, blocking on async work from a
request thread risks starvation, and it makes the first request pay the entire load cost.

**Do instead:** No I/O in constructors. Use an async factory, or preload in an
`IHostedService` at startup. If a singleton must be warm before first use, warm it explicitly
at startup where the cost is visible.

---

## L-04 — `new HttpClient()` exhausts sockets

**Evidence** `[Certain]`: In one supplier service we analysed, `SearchService` constructs its own
`HttpClient` (to attach a client certificate) rather than using the named `IHttpClientFactory`
client already registered in DI. Documented as a known rough edge in that service's own
`CLAUDE.md`.

**Why:** Certificate handling seems to require a custom handler, and building one locally is
the shortest path.

**Do instead:** Register a named client and configure its handler in DI —
`IHttpClientFactory` supports client certificates via
`ConfigurePrimaryHttpMessageHandler`. Certificates are not a reason to bypass the factory.

---

## L-05 — An unused dependency is still an attack surface

**Evidence** `[Certain]`: a lookalike package is referenced by 5 production services, always on the
line directly after a well-known resilience library, and there is **no `using Poly;` anywhere** in 281,000 lines.
It was an unmaintained hobby package whose name differed from the intended one by a single letter.

**Why:** An autocomplete misfire, then copied into four more services along with the project
file.

**Do instead:** Read the package identity — author, downloads, repository, last update —
before adding it. Audit project files for references nothing imports. Justify every dependency
in the pull request, and record the removal of an unjustified one as a decision record.

---

## L-06 — Retrying an irreversible operation duplicates it

**Evidence** `[Likely]` — inferred from the domain and from retry policies not being
differentiated per feature in the services reviewed. This is a known failure mode in ticketing
integrations generally.

**Why:** Resilience policies get applied globally because that is how the library documentation
demonstrates them. A timeout on IssueTicket usually means the ticket *was* issued and the
response was lost — retrying issues a second one.

**Do instead:** Retry policy is per feature, never global. Safe left of Booking; **never** on
IssueTicket, Refund, or Void. On an ambiguous timeout, retrieve the booking and determine
actual state before acting.

---

## L-07 — A credential committed to git is compromised

**Evidence** `[Certain]`: In one supplier service we analysed, `cert.pfx` (2,661 bytes) is
tracked in git at `<supplier-service-root>/<Supplier>-Air-Service/Shared/Config/cert.pfx`.

**Why:** The certificate is needed for local development, and committing it is the easiest way
to share it.

**Do instead:** Never commit certificates or credentials. `.gitignore` must cover `*.pfx`,
`*.pem`, `*.key`, `credentials*.json`, and `LocalTests/`. Distribute secrets out of band.
Once committed, a secret is compromised — removing it from HEAD is not enough; it must be
rotated.

---

## L-08 — The god mapper is never written; it accumulates

**Evidence** `[Certain]`: 13 of the 17 services have a response mapper over 800 lines; the
largest is 2,028. One service does the same job in 11 files with a 187-line maximum.

**Why:** Nobody writes a 2,000-line file. They write 200, and then every new fare type, edge
case, and supplier quirk is appended, because appending is always cheaper than extracting.

**Do instead:** Decompose from the first commit
([ADR-0002](../docs/decision-records/ADR-0002-response-mapper-decomposition.md)). Name the
builders during design, before writing any of them. Retrofitting costs about a week per
service, which is why it never happens.

---

## L-09 — "We cannot test this" is usually false

**Evidence** `[Certain]`: 12 of the 17 services have no tests, including both architectural
reference services. One service has 205 tests that need no network access, built on fixtures
captured from the live staging API.

**Why:** Supplier integrations feel untestable because they depend on an external system. But
the risky part — mapping — is pure: response in, DTO out.

**Do instead:** Capture real responses, redact, commit as fixtures, test mappers offline
([ADR-0004](../docs/decision-records/ADR-0004-fixture-based-testing.md)). Capture the fixture
during your first successful sandbox call, when you have the response in front of you anyway.

---

## L-10 — Documentation and testing do not travel together

**Evidence** `[Certain]`: One service has 50 documents, 8 ADRs, and zero tests. Another has 205
tests and zero documents. The service with the best architecture in the estate has neither.

**Why:** Each is a habit held by a person, not by the system. Whoever built the tested service
tests; whoever built the documented service documents. Nothing transmits either habit to the
next service.

**Do instead:** This is precisely why the definition of done in `CLAUDE.md` requires both, and
why they are checklist items rather than aspirations. A system, not a person, has to carry the
habit.

---

## L-11 — The spec is not what production does

**Evidence** `[Certain, prod-verified]`, from one supplier service's handbook:
- Spec shows `OriginDestinationRefNumber` as a 0-based index; production one-way responses tag
  it `"1"`. Code that assumed the index broke.
- Spec describes 2 penalty buckets (`<1`/`>1` hours); production uses 3.

**Why:** Specs describe intent; production reflects years of accumulated behaviour, and airline
APIs are old.

**Do instead:** Verify every mapping assumption against a captured response, not the spec
alone. When they disagree, **the captured response wins** — and record the discrepancy in
`knowledge/suppliers/<supplier>.md` with the date, because the next developer will otherwise
"fix" your code back to the spec.

---

## L-12 — Windows cannot do mTLS with curl

**Evidence** `[Certain, prod-verified]`, from one certificate-authenticated integration: `curl`
on Windows uses Schannel, which will not present a client certificate. Attempts to test a
certificate-authenticated endpoint with curl fail in a way that looks like a server-side
rejection.

**Do instead:** Drive certificate-authenticated calls from PowerShell with `X509Certificate2`
and `Invoke-WebRequest -Certificate`. Hours have been lost debugging the wrong end of this.

---

## L-13 — An index drifts from what it indexes within hours

**Evidence** `[Certain]`: On the day this repository was created, three files already described
a state that had changed: `README.md` said the PoC was "awaiting the supplier-selection
decision" after the supplier had been selected and scaffolded; `development/progress.md` said
"no PoC supplier implemented" and "the service template does not exist yet" after both existed;
the ADR index listed the newest ADR as "Proposed — needs a human decision" while that ADR's own
`Status:` line said Accepted.

Elapsed time from written to wrong: **under six hours.**

**Why:** An index is a *second* copy of a fact. The primary copy (the ADR's own status, the
actual repo state) changes, and nothing forces the copy to follow. Everyone updates the thing
they are working on; nobody re-reads the summary that mentions it.

**Do instead:** Update the index **in the same commit** as the thing it indexes — treat a stale
index as a broken build, not a tidying task. Where an index is avoidable, avoid it: link to the
source of truth rather than restating its state. Re-audit on a schedule; the checks are
mechanical (see `development/readiness.md`).

This lesson was produced by the system auditing itself. That is the intended behaviour, but
note that the drift happened *anyway*, in a repository whose entire purpose is preventing drift.
Discipline does not come from having written the rule down.

---

## L-14 — A shared config file can be green-lit for editing and full of secrets at the same time

**Evidence** `[Certain, verified 2026-09-06]`: the aggregator's git-tracked `appsettings.json`
contains a MongoDB connection string with credentials, a JWT signing key, an Azure Blob account
key and a 185-character SAS URL, and **five suppliers' passwords**. Five environment variants of
the file are tracked.

Meanwhile `docs/permission-boundaries.md` listed `appsettings*.json` under **green — proceed
without asking**, and `docs/branching-and-pr.md` instructs developers to paste diffs into PR
descriptions. Both were written by this system.

**Why:** the permission model reasoned about *what a developer adds* — a supplier settings block,
which is genuinely harmless. It did not reason about *what the file already contains*. A diff
hunk carries surrounding context lines, so a small correct addition can expose an unrelated
secret sitting three lines above it.

**Do instead:** when granting edit permission on a shared config file, **open it first** and
check what is already there. Permission to add is not permission to disclose. Where a file holds
live credentials, say so at the point of permission, not in a general security section nobody
reads at the moment they need it.

This was found by the system auditing itself before any developer was onboarded — which is the
intended behaviour. It was still wrong for a day.

---

## Template for new lessons

```markdown
## L-NN — <one-line lesson, stated as the takeaway>

**Evidence** `[Certain|Likely|Guessing]`: what was observed, where, when.

**Why:** the mechanism — why a reasonable person did this.

**Do instead:** the specific corrective action.
```
