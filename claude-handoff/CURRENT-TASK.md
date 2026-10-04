# Current Task — Repository Cleanup Wave 1

## Current state and authorization

Phase 9 is `COMPLETE / CLOSED` under [DEC-081](../ai-context/decision-log.md), with all exits mapped in [VER-PH-009](../ai-context/planning/verification/VER-PH-009.md). `CTX-1.28` is active. Development user testing is `READY`; Phase 9.5 is `READY TO BEGIN`. **No product implementation work item is authorized.** Phase 9.5 requires separate scope and approval.

## Current maintenance activity

The owner approved repository cleanup Wave 1: reconcile current context and entry documentation only. Do not move or delete files, change code or tests, run tests or builds, commit, or push. Later cleanup waves are outside this authorization. Plan Location and the one-person Event remain separate product follow-ups; production authentication, hosting, and backup evidence remain separate predeployment work.

## Historical checkpoints — reference only

Everything below records earlier checkpoints and task briefs. In-progress labels, authorization statements, commands, and “next action” instructions below describe their dated historical state; **none authorizes work now**. Current authority is the two sections above and `ai-context/project-status.md`.

**Phase 9 is `CURRENT / IN PROGRESS`. `CTX-1.28` is active.** No implementation work item is currently authorized. Do not change `src/`, `tests/`, or `scripts/` until the owner selects and authorizes the next Phase 9 item (from `PRP-030`).

- **Most recent closure (2026-10-03):** `WI-009-010 — First-Use and Invitation Arrival Workflow`, `DONE / PASS` — `OWNER APPROVED` (`DEC-080`, O1–O11; `VER-WI-009-010`). `WI-009-005` to `WI-009-010` are all `DONE / PASS` — `OWNER APPROVED`.
- **Latest regression (`WI-009-010` closure):** Angular 269/269 (16 files); focused Playwright 3/3; one full `npm run test:e2e` with 30 passed, 0 failed, 0 skipped, and 10/10 databases removed; build passes with existing warnings only: `main` 597.68 kB, complete initial total 610.15 kB, estimated transfer 134.28 kB.
- **Development review invitations:** `wi009010-review-1440@example.invalid` and `wi009010-review-390@example.invalid` were sent and revoked in the shared Development database during the pre-approval review. They stay revoked; no supported cleanup path owns them.
- **Owner actions outstanding:** commit the uncommitted `WI-009-001` to `WI-009-010` changes and records, including `DEC-075` to `DEC-080` and `CTX-1.28`.

---

# Historical task record — WI-009-010 First-Use and Invitation Arrival Workflow (`DONE / PASS`)

> **Closed (2026-10-03): `DONE / PASS` — `OWNER APPROVED`.** AC-01 to AC-12 pass; see `VER-WI-009-010` and `DEC-080`. Implementation scope, O10 (keep "Continue to Home" wording), and O11 (template-level single Family-required message) are recorded there.

---

# Historical note — no work item authorized (before WI-009-010)

**Phase 9 — Integration & UX Refinement is `CURRENT / IN PROGRESS`** (`DEC-073`). **`CTX-1.28` is active.** `OD-001` is resolved; `OD-002` is open (Phase 9.5).

**No implementation work item is currently authorized.** Do not change `src/`, `tests/`, or `scripts/` until the owner selects and authorizes the next Phase 9 item (from `PRP-030`).

- **Most recent closures (2026-10-03):** `WI-009-005` to `WI-009-009`, all `DONE / PASS` — `OWNER APPROVED`. The latest is `WI-009-009 — Safety Dashboard Prototype Alignment` (`DEC-079`; `VER-WI-009-009`).
- **Latest regression (`WI-009-009` closure):**
  - Angular 244/244;
  - focused Safety dashboard and navigation Playwright passed;
  - one full `npm run test:e2e`: 28 passed, 0 failed, 0 skipped; 9/9 databases removed;
  - build passes, with existing warnings only: `main` 594.88 kB, complete initial total 607.36 kB, estimated transfer 133.77 kB.
- **Bundle terminology:** from `WI-009-009` on, records give `main`, the complete initial total, and the estimated transfer separately. The 594.55 kB "initial bundle" recorded at `WI-009-008` was `main`.
- **Owner actions outstanding:** commit the uncommitted `WI-009-001` to `WI-009-009` changes and records, including `DEC-075` to `DEC-079` and `CTX-1.28`.
- **Recorded follow-ups, unscheduled:**
  - the Plan create `Location` header (OD-I);
  - a possible Plans & Trips dashboard in a separate story (`DEC-078` D11);
  - whether Home's Safety card should also link to `/safety` (`WI-009-009` O11);
  - backup-aging and restore evidence before any deployment.

Test leftovers: run `.\scripts\test-artifacts.ps1` (read-only). No retained database remains after the 2026-10-03 `WI-009-009` closure cleanup.

---

# Historical task record — WI-009-009 Safety Dashboard Prototype Alignment (`DONE / PASS`)

> **Closed (2026-10-03): `DONE / PASS` — `OWNER APPROVED`.** AC-01 to AC-13 pass; see `VER-WI-009-009` and `DEC-079`.

- **What it is:** `/safety` is the read-only Safety dashboard after `references/ux/home-dashboard` panel ④: separate Open Concerns and Open Safety Incidents cards, a Recent Check-Ins placeholder, and an About Concerns and Incidents card; Back to Safety on `/concerns` and `/incidents`.
- **Component:** standalone, deferred `safety-dashboard.ts` / `.html` / `.scss`.
- **Evidence:** `ai-context/planning/verification/evidence/WI-009-009/`.

---

# Historical note — no work item authorized (before WI-009-009)


**Phase 9 — Integration & UX Refinement is `CURRENT / IN PROGRESS`** (`DEC-073`). **`CTX-1.28` is active.** `OD-001` is resolved; `OD-002` is open (Phase 9.5).

**No implementation work item is currently authorized.** Do not change `src/`, `tests/`, or `scripts/` until the owner selects and authorizes the next Phase 9 item (from `PRP-030`).

- **Most recent closures (2026-10-03):**
  - `WI-009-008 — Events Dashboard Prototype Alignment`: `DONE / PASS` — `OWNER APPROVED` (`DEC-078`; `VER-WI-009-008`).
  - `WI-009-007 — Family Dashboard Prototype Alignment`: `DONE / PASS` — `OWNER APPROVED` (`DEC-077`).
  - `WI-009-006 — Home Dashboard Prototype Alignment`: `DONE / PASS` — `OWNER APPROVED` (`DEC-076`).
  - `WI-009-005`: `DONE / PASS` — `OWNER APPROVED`.
- **Latest regression (`WI-009-008` closure):**
  - Angular 228/228;
  - focused Events Playwright passed;
  - one full `npm run test:e2e`: 24 passed, 0 failed, 0 skipped; 8/8 databases removed;
  - build passes, with existing warnings only (initial bundle 594.55 kB).
- **Owner actions outstanding:** commit the uncommitted `WI-009-001` to `WI-009-008` changes and records, including `DEC-075` to `DEC-078` and `CTX-1.28`.
- **Recorded follow-ups, unscheduled:**
  - the Plan create `Location` header (OD-I);
  - a possible Plans & Trips dashboard in a separate story (`DEC-078` D11);
  - backup-aging and restore evidence before any deployment.

Test leftovers: run `.\scripts\test-artifacts.ps1` (read-only). No retained database remains after the 2026-10-03 closure cleanup.

---

# Historical task record — WI-009-008 Events Dashboard Prototype Alignment (`DONE / PASS`)

> **Closed (2026-10-03): `DONE / PASS` — `OWNER APPROVED`.** AC-01 to AC-11 pass; see `VER-WI-009-008` and `DEC-078`.

- **What it is:** a presentation-only Events redesign after `references/ux/home-dashboard` panel ③:
  - `/events` is the dashboard: Upcoming Events, a read-only current-Family Trips card, six real Quick Links, and an RSVPs Needed placeholder.
  - `/events/new` holds the existing create form; `/events/all` holds the complete list with inline edit; `/events/:eventId` is unchanged except its "Events" title.
- **Styles:** `events-dashboard.scss` (new, about 3.4 kB compiled).
- **Evidence:** `ai-context/planning/verification/evidence/WI-009-008/`.

---

# Historical note — no work item authorized (before WI-009-008)


**Phase 9 — Integration & UX Refinement is `CURRENT / IN PROGRESS`** (`DEC-073`). **`CTX-1.28` is active.** `OD-001` is resolved; `OD-002` is open (Phase 9.5).

**No implementation work item is currently authorized.** Do not change `src/`, `tests/`, or `scripts/` until the owner selects and authorizes the next Phase 9 item (from `PRP-030`).

- **Most recent closures (2026-10-03):**
  - `WI-009-005`: `DONE / PASS` — `OWNER APPROVED`.
  - `WI-009-006 — Home Dashboard Prototype Alignment`: `DONE / PASS` — `OWNER APPROVED` (`DEC-076`).
  - `WI-009-007 — Family Dashboard Prototype Alignment`: `DONE / PASS` — `OWNER APPROVED` (`DEC-077`; `VER-WI-009-007`).
- **Latest regression (`WI-009-007` closure):**
  - Angular 215/215;
  - focused Family Playwright passed;
  - one full `npm run test:e2e`: 22 passed, 0 failed, 0 skipped; 7/7 databases removed;
  - build passes, with existing warnings only (initial bundle 583.51 kB).
- **Owner actions outstanding:** commit the uncommitted `WI-009-001` to `WI-009-007` changes and records, including `DEC-075`, `DEC-076`, `DEC-077`, and `CTX-1.28`.
- **Recorded follow-ups, unscheduled:**
  - the Plan create `Location` header (OD-I);
  - backup-aging and restore evidence before any deployment.

Test leftovers: run `.\scripts\test-artifacts.ps1` (read-only). No retained database remains after the 2026-10-03 closure cleanup.


---

# Historical task record — WI-009-007 Family Dashboard Prototype Alignment (`DONE / PASS`)

> **Closed (2026-10-03): `DONE / PASS` — `OWNER APPROVED`.** AC-01 to AC-10 pass; see `VER-WI-009-007` and `DEC-077`.

- **What it is:** a presentation-only Family redesign after `references/ux/home-dashboard` panel ②:
  - `/family` is the dashboard: members, member detail, Waiting for you, and placeholders for Upcoming birthdays, Recent Family Updates, and Recent Check-Ins.
  - `/family/settings` holds Family details, relationships, and the administrator-only Invite card.
  - Every member sees the gear, which is navigation only.
- **Owner decisions:** D1–D7 (`DEC-077`).
- **Styles:** `family.scss` (dashboard) and `family-settings.scss` (Settings, links, first-use).
- **Evidence:** `ai-context/planning/verification/evidence/WI-009-007/`.

---

# Historical task record — WI-009-006 Home Dashboard Prototype Alignment (`DONE / PASS`)

> **Closed (2026-10-03): `DONE / PASS` — `OWNER APPROVED`.** AC-01 to AC-10 pass; see `VER-WI-009-006` and `DEC-076`.

- **Origin:** the item was created to formalize Home redesign work started during the `WI-009-005` owner review.
- **What it is:** a presentation-only Home dashboard after `references/ux/home-dashboard` panel ①:
  - a time-of-day greeting, with the Family in the subtitle;
  - real Events, Trips (heading "Trips"), and Plans cards with compact empty states;
  - a compact Safety coordination card and a View Family action;
  - text-only placeholders for Recent Family Updates, My Tasks, and Active Sharing.
- **Not included:** any Phase 9.5 capability, and any API, schema, authorization, or Home-query change.
- **Evidence:** `ai-context/planning/verification/evidence/WI-009-006/`.

---

# Historical task record — WI-009-005 Refresh-Safe Item URLs and Authorize-First Deep Links (`DONE / PASS`)

> **Closed (2026-10-03): `DONE / PASS` — `OWNER APPROVED`.** AC-01 to AC-42 pass; see `VER-WI-009-005`.
>
> - `AC-16` carries the `DEC-076` note: its Home-presentation clause is superseded by `WI-009-006`, and its row links still hold.
> - The two Home link captures were recaptured under new names.
> - The three retained `item-links` diagnostic databases were removed by exact name.
>
> The brief below is kept for the record only; do not re-execute it.

**Phase 9 — Integration & UX Refinement is `CURRENT / IN PROGRESS`** (`DEC-073`). `PRP-030` is `APPROVED` (`DEC-074`). **`CTX-1.28` is active** (`DEC-075`; `REQ-UX-014` amended). `OD-001` is resolved; `OD-002` is open (Phase 9.5).

**Authorized:** `ai-context/planning/stories/WI-009-005-refresh-safe-item-urls-and-authorize-first-deep-links.md` is `APPROVED — EXECUTION AUTHORIZED` against `CTX-1.28` (project owner, 2026-10-02). No further routine approval is needed. The story is the scope authority; its **Owner decisions OD-A to OD-I override any looser wording**. Implementation has not started.

- **What:** `PRP-030` M3 client part and the M4 link part.
  - Item routes: `/events/:eventId`, `/plans/:planId`, `/trips/:tripId`, `/concerns/:concernId`; `/incidents/:incidentId` is kept.
  - Authorize-first Family switching: route → item API read → server authorization → Family decision → render. Never switch from the URL, navigation state, or list metadata.
  - The generic unavailable state per type, with the Family unchanged.
  - Semantic list and Home item links.
  - Concern route conversion; after create, navigate to `/concerns/{id}` without losing the create recovery states.
  - The Incident multi-Family refresh fix.
  - The "Switched to {name}" polite announcement.
  - An opt-in second-Family Development / E2E fixture (test support only).
- **Key rules:**
  - **OD-A:** an authorized Trip whose Family is not selectable renders with no switch. This is Trip only.
  - **OD-B:** refresh `/api/family-groups/mine` at most once. A non-Trip item whose Family is still absent shows the generic unavailable state.
  - **OD-C:** Event and Plan details stay in `App`. Stop and report an exact Event-card extraction if reuse fails.
  - **OD-I:** the Plan `Location` header is out of scope.
- **Order:**
  1. `TASK-001`: baseline.
  2. `TASK-002`: the fixture.
  3. `TASK-003`: the **live server-response precheck (`AC-31`)**, before significant client work. Stop if any type differs materially.
  4. The UI tasks.
  5. Verification: Angular tests; `item-links.spec.ts`; one full `npm run test:e2e`; `npm run build`; `dotnet build`.
  6. Evidence, then `VER-WI-009-005`.

  Hand back `VERIFYING / OWNER REVIEW`.
- **Do not:** change any production API, schema, authorization, or registration; raise any budget, including `concerns.scss` (stop and propose a split instead); commit or push.
- **Owner actions outstanding:** commit the uncommitted `WI-009-001`, cleanup-record, `WI-009-002`, `WI-009-003`, and `WI-009-004` changes, including the `WI-009-004` implementation and closure records.

Test leftovers: run `.\scripts\test-artifacts.ps1` (read-only). Its cleanup is dry run by default and needs `-Execute`; see `docs/local-development.md` § "Test artifacts: status and cleanup".

---

# Historical task record — WI-009-004 Event and Plan Single-Item Read Foundation (`DONE / PASS`)

> **Closed (2026-10-02): `DONE / PASS` — `OWNER APPROVED`.** All 22 acceptance checks pass; see `VER-WI-009-004`. The verification evidence:
>
> - the SQL-gated baseline: 191/191;
> - the final suite: 211/211, with 55 SQL-gated methods;
> - 161 list/item equivalence comparisons with 0 mismatches.
>
> The owner accepted as non-blocking:
>
> - the test-only stub state and `using` additions;
> - stub-based HTTP missing-versus-unauthorized equivalence;
> - the lack of a dedicated multi-item ordering test.
>
> The Plan create `Location` header stays a recorded, unscheduled follow-up. The brief below is kept for the record only; do not re-execute it.

- **Authority:** `ai-context/planning/stories/WI-009-004-event-and-plan-single-item-read-foundation.md`, `APPROVED — EXECUTION AUTHORIZED` against `CTX-1.27` (project owner, 2026-10-02). The story is the scope authority, and its **"Owner decisions (2026-10-02)" D1–D10 override any looser wording**.
- **What:** server only (`PRP-030` M3, server part; `REQ-PRIV-027`; `REQ-UX-014` steps 1 and 3; `DEC-074` D4).
  - **D1:** `GET /api/events/{eventId}` and `GET /api/plans/{planId}`. The item ID alone is the request; no `FamilyGroupId` in the route.
  - **D2:** every missing or unauthorized case returns `404` with an empty body. No `403`, no differing bodies, and no Family metadata or existence hints.
  - **D3/D4:** preserve the existing Event and Plan access rules exactly. Administrator status grants nothing.
  - **D5/D6:** one private access query per repository, shared by the Family-scoped list and the item read. If moving a list onto it changes any externally observable result, stop and report. List/item equivalence tests are required.
  - **D7:** do not reuse `GetCreatedEventAsync`, which forces `CanEdit = true`.
  - **D8:** return the existing `FamilyEventView` / `FamilyPlanView` unchanged; `FamilyGroupId` only, no Family name.
  - **D9/D10:** no change to the Plan create `Location` header, and no Event or Plan audit.
  - **First:** the SQL-gated .NET baseline (`TASK-001`, `AC-22`), recording total, SQL-gated, and pass/fail/skip counts, with proof that the SQL tests executed. If SQL tests cannot run, stop and report.
- **Not authorized:** any Angular or client change, item routes, Home links, Family switching, navigation or visual change; schema, migration, DbContext, or domain-behavior change; audit; new dependency; create or update change. Do not commit or push. This folder has no git repository.
- **Hand back** in `VERIFYING / OWNER REVIEW` with `ai-context/planning/verification/VER-WI-009-004.md` (22 acceptance checks). Owner review is required to close.

---

# Historical task record — WI-009-003 Six-Area Navigation and Safety Discoverability (`DONE / PASS`)

> **Closed (2026-10-02): `DONE / PASS` — `OWNER APPROVED`.** All 23 acceptance checks pass, with owner visual approval (AC-23) at 1440, 1024, 768, 600, and 390 px; see `VER-WI-009-003`. Five implementation deviations and the +12.77 kB initial-bundle growth (monitoring-only) were accepted as non-blocking. The post-review cleanup cleared only the transient `e2e/captures/` and `e2e/test-results/` folders; no database, run record, or evidence was removed. The brief below is kept for the record only; do not re-execute it.

**Phase 9 — Integration & UX Refinement is `CURRENT / IN PROGRESS`** (`DEC-073`). `PRP-030` is `APPROVED` (`DEC-074`). **`CTX-1.27` is active.** `OD-001` is resolved (`REQ-UX-013`); `OD-002` is open (Phase 9.5).

- **Authority:** `ai-context/planning/stories/WI-009-003-six-area-navigation-and-safety-discoverability.md`, `APPROVED — EXECUTION AUTHORIZED` against `CTX-1.27` (project owner, 2026-10-02). The story is the scope authority, and its **"Owner decisions (2026-10-02)" D1–D6 override any looser wording**. No further routine approval is needed. The stop conditions apply. Implementation has not started.
- **What:** Angular shell and navigation only.
  - Exactly six main-navigation destinations, in this order at every width: Home, Family, Events, Plans & Trips, Safety, Me / Settings.
  - **D5:** they are semantic `routerLink` links in a `nav` labeled "Main navigation", not buttons.
  - **D1/D6:** data-free landing pages `/plans-trips` (links to Plans and Trips) and `/safety` (links to Concerns and Safety Incidents). No counts, status, summaries, or Concern/Incident/Plan/Trip requests. `/plans`, `/trips`, `/concerns`, `/incidents…` keep working directly.
  - **D3:** one typed view → area table; `aria-current="page"` on an area's own page, `"true"` on its child routes; exactly one current item; never used for authorization.
  - **D2:** about 390 px → 3 rows × 2 columns, no overflow, 44 px targets, no "More" menu. Desktop sidebar unchanged (`DEC-070`).
  - **D4:** focus the page `h1` after user-initiated link navigation only; not on startup, refresh, back/forward, or internal state changes.
  - Tests: component tests, new `e2e/navigation.spec.ts` (1440 px and 390 px), locator-only updates in `trip-audience-removal.spec.ts` and `invitation-handoff.spec.ts`, one full `npm run test:e2e`, `npm run build`.
- **Not authorized:** server, API, authorization, schema, or dependency changes (no `@axe-core/playwright`); `REQ-UX-014` item URLs, single-item reads, Home item links, cross-Family deep links; Home hub changes; Concern, Incident, Plan, or Trip behavior changes; `App` extraction; budget increases; `OD-002`. Do not commit or push.
- **Hand back** in `VERIFYING / OWNER REVIEW` with `VER-WI-009-003` and evidence in `ai-context/planning/verification/evidence/WI-009-003/`. Owner visual approval is required to close.

### Start here

1. `ai-context/README.md`, then `ai-context/project-status.md`.
2. The WI-009-003 story in full, then only its "Required context manifest".
3. [WORKING-RULES.md](WORKING-RULES.md), [MOCKUP-ALIGNMENT.md](MOCKUP-ALIGNMENT.md), [VALIDATION.md](VALIDATION.md) §§ 1–3 and 5–7, [SESSION-CLOSEOUT.md](SESSION-CLOSEOUT.md) (UI closeout), and `reference/detail-shell-topbar.png`.

Test leftovers: run `.\scripts\test-artifacts.ps1` (read-only). Its cleanup is dry run by default and needs `-Execute`; see `docs/local-development.md` § "Test artifacts: status and cleanup".

---

# Historical task record — WI-009-002 Test Artifact Lifecycle and Cleanup (`DONE / PASS`)

> **Closed (2026-10-02): `DONE / PASS` — `OWNER APPROVED`.** All 20 acceptance checks pass; see `VER-WI-009-002`. The post-review cleanup removed the 9 Resolved run folders and the transient captures and test results; no database was deleted. The brief below is kept for the record only; do not re-execute it.

- **Authority:** `ai-context/planning/stories/WI-009-002-test-artifact-lifecycle-and-cleanup.md`, `APPROVED — EXECUTION AUTHORIZED` against `CTX-1.27` (project owner, 2026-10-02). The story is the scope authority, and its **"Owner decisions (2026-10-02)" D1–D10 override any looser wording**. No further routine approval is needed. The stop conditions apply.
- **What:** test infrastructure only.
  - `scripts/test-artifacts.ps1`: a read-only status report by default; destructive actions need an exact target and `-Execute`; every database drop is delegated to `remove-scenario-database.ps1`.
  - Run-record lifecycle; cited or uncertain runs fail closed.
  - `FAMILY_SAFETY_E2E_KEEP_RUN_FILES`.
  - Transient screenshots in `e2e/captures/`.
  - Fixed generated-file locations, and a safety-check script.
- **Not authorized:** product, API, schema, or fixture code; new dependencies; weakening any `WI-009-001` safeguard; deleting `FamilySafety`, PRE-EXISTING, Class D, or unknown-ownership databases; touching the root `trips` file (`UNCLASSIFIED / OWNER REVIEW REQUIRED`) or any existing canonical evidence. Do not commit or push.
- **Hand back** in `VERIFYING / OWNER REVIEW` with `VER-WI-009-002`. Follow the [VALIDATION.md](VALIDATION.md) test cadence: one full-suite run, no repeated reliability sequence.
- `WI-009-001 — Playwright Per-Spec Scenario Isolation` is **`DONE / PASS` — `OWNER APPROVED`** (2026-10-02; `VER-WI-009-001`). The one-time cleanup is complete (`ai-context/planning/verification/evidence/WI-009-001/database-cleanup-completion-2026-10-02.md`).

---

# Historical task record — WI-009-001 Playwright Per-Spec Scenario Isolation (`DONE / PASS`)

> **Closed (2026-10-02): `DONE / PASS` — `OWNER APPROVED`.** All 14 acceptance checks pass; see `VER-WI-009-001`. The brief below is kept for the record only; do not re-execute it.

**Phase 9 — Integration & UX Refinement is `CURRENT / IN PROGRESS`** (`DEC-073`). `PRP-030` is `APPROVED` (`DEC-074`). **`CTX-1.27` is active.** Phase 9.5 is `FUTURE`.

**Authority:** `ai-context/planning/stories/WI-009-001-playwright-per-spec-scenario-isolation.md`, `APPROVED — EXECUTION AUTHORIZED` against `CTX-1.27` (project owner, 2026-10-02). The story is the scope authority, and its **"Owner execution conditions"** section overrides any looser wording. **No further routine approval is needed.** Stop conditions still apply.

### What to build

Test infrastructure only:

- each spec file gets its own scenario database and its own API process, declared through one helper;
- the Angular dev server stays shared, and the API is built once and reused;
- `isInvitationOnlyRun` is removed;
- guarded cleanup goes in `scripts/remove-scenario-database.ps1`;
- `-NoBuild` is added to the scenario script;
- the mechanism is documented in `docs/local-development.md`;
- verification goes in `VER-WI-009-001`.

### Database deletion — only under ALL of these conditions

Automated deletion is authorized **only** for databases created by the current Playwright run:

1. exact pattern `^FamilySafety_Invitation_E2E_[A-Za-z0-9]+$`;
2. not `FamilySafety`;
3. recorded in the current run's ownership manifest and not PRE-EXISTING;
4. only after the corresponding spec passes;
5. failed-spec databases preserved;
6. fail closed if ownership cannot be proven;
7. no wildcard deletion;
8. no partial-prefix deletion;
9. never pre-existing databases;
10. log the exact name before dropping.

**If any safeguard fails, do not drop.** The script needs an explicit name and manifest path, rejects blank input, uses safe SQL with no embedded credentials (Windows integrated security, as the tests already do), and exits non-zero on unsafe input or failure.

### Order of work

1. **First,** before any implementation or cleanup, enumerate existing `FamilySafety*` test or scenario databases and record them as PRE-EXISTING. Delete nothing.
2. Then follow `TASK-001` to `TASK-007`.
3. When a spec fails: stop its API cleanly, keep its database, report its name, and do not retry cleanup.
4. For the API: controlled port 5080 must be free before start (fail if occupied; never stop an unknown process); wait for health before the spec runs; stop the process tree after the spec; free the port before the next spec; leave no orphaned `dotnet` processes.

### Do not

- change product, API, schema, migration, fixture (`DevelopmentFixtureRegistration`), identity, or outbox code;
- change the meaning of any existing assertion;
- add any package (`@axe-core/playwright` belongs to a later item);
- address the later Phase 9 findings (Event/Plan single-item reads, `DEC-033` admin display, axe, `OD-002`, root decomposition);
- delete `e2e-alt/` or the root build folders;
- mark the item `DONE / PASS` (hand back in `VERIFYING / OWNER REVIEW`);
- commit or push.

### Start here

1. `ai-context/README.md`, then `ai-context/project-status.md`.
2. The WI-009-001 story in full, then only its "Required context manifest".
3. [WORKING-RULES.md](WORKING-RULES.md), [VALIDATION.md](VALIDATION.md), [SESSION-CLOSEOUT.md](SESSION-CLOSEOUT.md).

---

# Historical task record — PRP-030 owner review (completed 2026-10-02)

`PRP-030` was drafted under `DEC-073` and approved under `DEC-074`, together with decisions D1–D4 and the `CTX-1.27` update (`VER-CTX-1.27`).


---

# Historical task record — Phase 8 closure (`DEC-072`)

Phase 8 — Safety Coordination was closed under `DEC-072` (2026-10-02). `VER-PH-008` records all 13 exit criteria as `PASS`, and `WI-008-001` to `WI-008-004` are `DONE / PASS`. The carried-forward follow-ups listed in `VER-PH-008` are classified in `PRP-030`.

---

# Historical task record — WI-008-004 Incident UI and Start-an-Incident Flow (`DONE / PASS`)

> **Closed (2026-10-02): `DONE / PASS` — `OWNER APPROVED`.** The owner's visual review passed; the accepted visual differences and implementation deviations are recorded in `VER-WI-008-004`. Evidence: 118/118 Angular tests, both Incident Playwright journeys, the independent regressions, and the build; captures are in `ai-context/planning/verification/evidence/WI-008-004/`. The brief below is kept for the record only; do not re-execute it.

**Authority:** `ai-context/planning/stories/WI-008-004-incident-ui-and-start-an-incident-flow.md`. The story is the scope authority. Its "Owner decisions" and "Execution authorization" sections record what the owner approved.

**Baseline:** `CTX-1.26`, active. `OD-001` and `OD-002` are open and must stay open; `OD-004` is resolved. Do not create `CTX-1.27`.

**Server contract:** the WI-008-003 Incident API (`src/FamilySafety.Api/Incidents/IncidentEndpoints.cs`, `src/FamilySafety.Application/Incidents/IncidentContracts.cs`) is **authoritative and read-only**. No server, API, schema, or migration change is authorized. A UI need the API cannot meet is a stop condition.

**What to build:** Angular UI only, exactly as the story defines it:

- the routes `/incidents`, `/incidents/new`, and `/incidents/:incidentId`, with no navigation destination;
- the Incident list, direct creation, and Start an Incident from a Concern (via router navigation state);
- Incident detail: header, lineage, People with access, handoff, timeline, findings, Unknowns, summaries, contact attempts, corrections and "Add another report", tasks, and resolution;
- the Frozen, Resolved, and unavailable states, including refresh-on-denial;
- the Home card retitled "Safety coordination", with "Open Incidents";
- component tests, the Playwright Incident journeys, rendered evidence in `ai-context/planning/verification/evidence/WI-008-004/`, and `VER-WI-008-004`.

### Start here

1. `ai-context/README.md`, then `ai-context/project-status.md`.
2. The WI-008-004 story in full. Then load only its "Required context manifest".
3. [WORKING-RULES.md](WORKING-RULES.md) (the WI-008-004 UI scope), [MOCKUP-ALIGNMENT.md](MOCKUP-ALIGNMENT.md), and the images in `claude-handoff/reference/`.
4. [VALIDATION.md](VALIDATION.md) §§ 1–3 and 5–7, and [SESSION-CLOSEOUT.md](SESSION-CLOSEOUT.md) (UI closeout).

### Do

- Work through `TASK-001` to `TASK-007` in the story, ticking each checkbox as it completes.
- Treat the approved design and the `UI-D1`–`UI-D4` decisions as settled. Reopen nothing unless you find a genuine canonical contradiction; that is a stop condition.
- Run the Angular tests, the Incident Playwright spec (and the Concern, invitation, and Trip-audience specs independently), and the production build. Record counts, budget sizes, and the initial bundle before and after.
- Write `VER-WI-008-004.md` and set the story to `VERIFYING / OWNER REVIEW`. Do not mark `DONE / PASS`.

### Do not

- change `.NET` code, API routes or contracts, permissions, schema, or migrations;
- add a navigation destination or resolve `OD-001` or `OD-002`;
- add notifications, location, Check-In, Sharing, maps, escalation, reopening, task editing or reassignment, or unrelated refactoring;
- raise budget thresholds, add screenshot golden baselines, or overwrite existing evidence;
- commit or push.

---

# Historical task record — WI-008-003 Safety Incident Domain and Access Foundation (`DONE / PASS`)

> **Closed (2026-10-01): `DONE / PASS` — `OWNER APPROVED`.** All 29 acceptance checks pass, with 191/191 tests passing with `RUN_FAMILYSAFETY_SQL_TESTS=1`. See `ai-context/planning/verification/VER-WI-008-003.md`. The brief below is kept for the record only; do not re-execute it.

**Authority:** `ai-context/planning/stories/WI-008-003-safety-incident-domain-and-access-foundation.md`. The story is the scope authority. Canonical rules: `REQ-SAFE-016` (`ai-context/workflows/safety.md`) and `REQ-PRIV-026` (`ai-context/core/privacy-safety-contract.md`).

**What to build:** the server-side Safety Incident foundation, exactly as the story defines it:

- domain, application service, and authorization;
- grants bound to `MembershipId` + `AccessVersion`, including the coordinator's;
- coordinator transfer, and freeze when the coordinator loses eligibility;
- non-authorizing `OriginatingConcernId` lineage;
- findings, contact attempts, tasks, corrections and challenges, and a read-time timeline;
- `Open` / `Frozen` / `Resolved`;
- 365-day retention and audit, with `DEC-069` denied summaries;
- maintenance and cleanup;
- one additive EF migration;
- the private API;
- SQL-gated tests;
- `VER-WI-008-003`.

**Not in scope:**

- Incident UI, or any `src/FamilySafety.Web` change;
- `WI-008-004`;
- notifications;
- location, Check-In, or Sharing;
- automatic escalation;
- `OD-002`;
- unrelated refactoring.

### Start here

1. `ai-context/README.md`, then `ai-context/project-status.md` (mandatory entry).
2. The WI-008-003 story, in full. Then load only its "Required context manifest".
3. The Concern code it names as patterns to reuse: `src/FamilySafety.Domain/Concerns/`, `src/FamilySafety.Application/Concerns/`, `src/FamilySafety.Infrastructure/Persistence/EfConcernRepository.cs`, the Concern mapping in `FamilySafetyDbContext.cs`, `src/FamilySafety.Api/Concerns/`, and `tests/FamilySafety.Foundation.Tests/Concern*Tests.cs`.
4. [WORKING-RULES.md](WORKING-RULES.md) (the WI-008-003 server scope), [VALIDATION.md](VALIDATION.md) § 4 (SQL gate), and [SESSION-CLOSEOUT.md](SESSION-CLOSEOUT.md) (server closeout).

`MOCKUP-ALIGNMENT.md` and `claude-handoff/reference/` are **not** needed for this task.

### Do

- Work through `TASK-001` to `TASK-007` in the story, ticking each checkbox as it completes.
- Treat the accepted design decisions in the story's Execution authorization section as settled. Reopen nothing unless you find a genuine canonical contradiction; that is a stop condition.
- Run `dotnet build` and the full `dotnet test` with `RUN_FAMILYSAFETY_SQL_TESTS=1`. Record the counts and proof that the Incident SQL tests ran.
- Write `ai-context/planning/verification/VER-WI-008-003.md` and set the story to `VERIFYING / OWNER REVIEW`.
- Do not commit or push.

---

# Historical task record — Match the Web App to the Reference Mockups

> **Status (2026-10-01): COMPLETE — owner visual approval recorded as `DEC-070`.** The site-wide alignment is implemented and approved. Evidence is in `ai-context/planning/verification/evidence/site-mockup-alignment/`. Do not rework the accepted differences listed in `DEC-070`. This section is kept as the method record for future visual work, such as `WI-008-004`. The decisions below are resolved, and its "Authorization" note refers only to that completed UI task.

**MAKE THE RUNNING WEB APPLICATION MATCH THE MOCKUPS IN `C:\Dev\blevins\claude-handoff\reference` AS CLOSELY AS PRACTICAL WHILE PRESERVING APPROVED PRODUCT BEHAVIOR.**

This is stronger than "polish the UI", "align generally with mockups", or "improve visual consistency". The files in `claude-handoff\reference` are the actual target. Success means an owner who puts a reference file next to the running screen can see they are the same design, apart from differences that are recorded and justified.

### Source hierarchy

| Rank | Source | Role |
|---|---|---|
| 1 | `claude-handoff\reference\*.png` | **Primary visual target** |
| 2 | `ai-context\` | Product, privacy, and functional authority. Decides what may exist and how it behaves. |
| 3 | The running application | Implementation starting point. It is not the target. |
| 4 | Generated screenshots under `ai-context\planning\verification\evidence\` and `src\FamilySafety.Web\*-review-*.png` | Evidence of current or prior implementation only. **Never a design authority.** |

The other mockups under `ai-context\references\ux\` are secondary. See `MOCKUP-ALIGNMENT.md` § Coverage gaps.

The mockups define visual composition only. They never authorize functionality. Anything a mockup shows that `ai-context` does not approve is DROP. Its visual pattern may still be reused for approved content (ADAPT).

### Start here

1. Open every file in `claude-handoff\reference` with the image reader. Do not rely on the descriptions alone.
2. Read `MOCKUP-ALIGNMENT.md`: inventory, screen mapping, KEEP / ALIGN / ADAPT / DROP, and the open owner decisions.
3. Open the running app (`docs/local-development.md`) and compare each mapped screen side by side at 1440 px.
4. Write the per-screen comparison table before editing code.

### Owner decisions (resolved 2026-10-01, `DEC-070`)

- **Palette:** blue reference palette.
- **Shell:** desktop sidebar plus slim top bar. Search, bell, and avatar menu are not built.
- **Navigation:** current destinations and order kept. No Messages or Safety destination. `OD-001` remains open, so the narrow navigation stays a wrapping row.

The original questions are kept below for the record.

#### Original questions

The references assume a design that differs from the current app in three shared ways. Earlier owner decisions held these fixed, but they were scoped to WI-008-002, which is now closed. **Ask the owner; do not decide silently.**

1. **Palette:** the references use a blue palette. The app uses evergreen tokens (owner-approved through Phase 7.5 and WI-008-002). Choose one: keep evergreen and match composition only, or move the shared tokens toward the reference blue, which restyles every screen.
2. **Shell:** `detail-shell-topbar.png` and every panel show a left sidebar plus a top bar (logo, search, bell, avatar). The app has a stacked header card with horizontal nav. A desktop sidebar/top-bar shell is a shared-shell change. Search, the notification bell, and the avatar menu are not approved functionality.
3. **Navigation destinations:** the references list Home, Events, Trips, Family, Safety, Messages, Plans, and Me / Settings. The app has Home, Family, Events, Plans, Trips, and Me / Settings; Concerns is reached from Home. Adding "Safety" or "Messages", or changing the destination count or order, touches `OD-001`, which is open. Messages is not an approved capability.

### Authorization

The handoff does not authorize implementation. Confirm with the owner which work item authorizes the alignment (for example, a new bounded UI alignment story or stories) and its scope before changing `src/`. Changes to owner-approved screens need owner visual re-approval.

### Previous task (closed)

`WI-008-002 — Concern MVP User Interface` is `DONE / PASS` with owner visual approval (2026-10-01). It was aligned to `ai-context\references\ux\safety\safety-coordination.png`, rendered in evergreen tokens with the shell unchanged. That approval does not exempt Concerns from the new primary reference: panel 4 (`panel4-safety-aware.png`) now informs the Concerns list/entry presentation, subject to the DROP list.
