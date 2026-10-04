# Working Rules

## Source control

- **Do not commit.**
- **Do not push.**
- **Do not create, update, or complete a pull request.**
- Leave all changes uncommitted for the owner to review and commit manually. Provide a proposed PR title and description in the closeout as text only.
- The workspace has a Git repository. Report its actual status; the owner performs commits.

## Standing rule: scope comes from the current approved work item

**Only change application code when the currently approved, execution-authorized work item explicitly authorizes it.** This covers .NET code, API routes and contracts, permissions and authorization, database schema, EF migrations, and Angular code. Stay within that work item's stated scope, exclusions, and stop conditions.

Confirm the authorizing work item and its scope before editing `src/` or `tests/`. If no current work item authorizes a change, do not make it; ask the owner.

### Current authorized work item: none

Phase 9 is `COMPLETE / CLOSED` under `DEC-081`; Phase 9.5 is `READY TO BEGIN` but no product implementation work item is authorized. The current approved activity is repository cleanup Wave 1, limited to current context and documentation reconciliation. Do not change application code, tests, or scripts under this authorization. The owner performs commits; do not commit or push.

### Historical: WI-009-010 — First-Use and Invitation Arrival Workflow — `DONE / PASS`, owner approved 2026-10-03

`ai-context/planning/stories/WI-009-010-first-use-and-invitation-arrival-workflow.md` is closed (`VER-WI-009-010`; `DEC-080`, decisions O1–O11). It authorizes nothing further.

### Historical: WI-009-009 — Safety Dashboard Prototype Alignment — `DONE / PASS`, owner approved 2026-10-03

`ai-context/planning/stories/WI-009-009-safety-dashboard-prototype-alignment.md` is closed (`VER-WI-009-009`; `DEC-079`, decisions O1–O14). It authorizes nothing further.

- **Scope:**
  - the read-only `/safety` dashboard (panel ④), a standalone deferred component;
  - Back to Safety links;
  - removal of the unreachable Safety branch of the group landing;
  - the deliberate `DEC-079` test supersessions.
- **Not changed:** API, schema, authorization, or server. No Phase 9.5 capability.

### Historical: WI-009-008 — Events Dashboard Prototype Alignment — `DONE / PASS`, owner approved 2026-10-03

`ai-context/planning/stories/WI-009-008-events-dashboard-prototype-alignment.md` is closed (`VER-WI-009-008`; `DEC-078`, decisions D1–D11 and the item-page terminology decision). It authorizes nothing further.

- **Scope:** presentation and client routing only: the `/events` dashboard (panel ③), `/events/new`, `/events/all`, the current-Family Trips card, six real Quick Links, the RSVPs Needed placeholder, and the "Events" item-page title.
- **Not changed:** API, schema, authorization, or server. No Phase 9.5 capability.

### Historical: WI-009-007 — Family Dashboard Prototype Alignment — `DONE / PASS`, owner approved 2026-10-03

`ai-context/planning/stories/WI-009-007-family-dashboard-prototype-alignment.md` is closed (`VER-WI-009-007`; `DEC-077`, decisions D1–D7). It authorizes nothing further.

- **Scope:** presentation and client routing only:
  - the `/family` dashboard (prototype panel ②);
  - the `/family/settings` management page;
  - the gear visible to all members, as navigation only;
  - the administrator-only Invite card;
  - honest Family placeholders.
- **Not changed:** API, schema, authorization, or server.
- **Phase 9.5:** no capability.

### Historical: WI-009-006 — Home Dashboard Prototype Alignment — `DONE / PASS`, owner approved 2026-10-03

`ai-context/planning/stories/WI-009-006-home-dashboard-prototype-alignment.md` is closed (`VER-WI-009-006`; `DEC-076`). It authorizes nothing further. It formalized a presentation-only Home redesign started during the `WI-009-005` owner review:

- the Home grid, greeting, and persistent cards;
- text-only placeholders for Recent Family Updates, My Tasks, and Active Sharing;
- the compact Safety card and the View Family action.

It made no API, schema, authorization, Home-query, or Phase 9.5 change. Under `DEC-076`, the `DEC-070` accepted difference "no activity feed, Tasks, or Active Sharing" now means no *capability*; honest, data-free Home placeholders are allowed.

### Historical: WI-009-005 — Refresh-Safe Item URLs and Authorize-First Deep Links — `DONE / PASS`, owner approved 2026-10-03

`ai-context/planning/stories/WI-009-005-refresh-safe-item-urls-and-authorize-first-deep-links.md` is closed (`VER-WI-009-005`). It authorizes nothing further. Its `AC-16` Home-presentation clause is superseded by `WI-009-006` (`DEC-076`). Its scope, kept for reference, was `APPROVED — EXECUTION AUTHORIZED` against `CTX-1.28` (project owner, 2026-10-02; `DEC-075`), with the story as the scope authority and its "Owner decisions (2026-10-02)" OD-A to OD-I overriding any looser wording.

- **Scope:** the client side of `REQ-UX-014`:
  - item routes `/events/:eventId`, `/plans/:planId`, `/trips/:tripId`, `/concerns/:concernId` (`/incidents/:incidentId` kept);
  - authorize-first Family switching (OD-A Trip exception; OD-B one-time Family-list refresh);
  - the generic unavailable state;
  - list and Home item links;
  - the Concern route conversion and create navigation (OD-G);
  - the "Switched to {name}" announcement (OD-H);
  - tests, the new `e2e/item-links.spec.ts`, and evidence;
  - the opt-in second-Family Development / E2E fixture (OD-E), as test support only.
- **Not authorized:** any production API, endpoint, contract, authorization, audit, schema, migration, or production-registration change; the Plan create `Location` header (OD-I); broad `App` extraction; raising a budget; new dependencies; `@axe-core/playwright`; `OD-002`; Phase 9.5 features.
- **Process:** run the live server-response precheck (`AC-31`) before significant client work. Stop on any story stop condition. Hand back `VERIFYING / OWNER REVIEW`. Do not commit or push.

### Historical: WI-009-004 (server only) — `DONE / PASS`, owner approved 2026-10-02

`ai-context/planning/stories/WI-009-004-event-and-plan-single-item-read-foundation.md` is closed (`VER-WI-009-004`). It authorizes nothing further.

- **Scope:** the two item-read endpoints; one shared visibility query in each of `EfFamilyEventRepository` and `EfFamilyPlanRepository`; their service and repository methods; and Event and Plan tests.
- **Not touched:** client, schema, migration, `DbContext`, domain, dependency, audit, create/update behavior, and the Plan create `Location` header.
- **Handoff correction:** while WI-009-004 was active, the heading above still read "none", which was stale. It is accurate again now that WI-009-004 is closed.

**Databases and generated folders:** do not drop any database or delete any folder outside the Playwright harness's own guarded per-run cleanup and `scripts/test-artifacts.ps1` (`WI-009-002`, `DONE / PASS`). That script's report is read-only; its cleanup is dry run by default, by exact name or run ID, with `-Execute`. The one-time owner-approved cleanup is complete (`ai-context/planning/verification/evidence/WI-009-001/database-cleanup-completion-2026-10-02.md`); the remaining `FamilySafety` and 15 uncertain databases must not be touched without a new owner authorization. `scripts/remove-scenario-database.ps1` is the only path that drops a database; `test-artifacts.ps1` delegates to it, and it refuses every PRE-EXISTING database.

### Historical: WI-009-003 (Angular shell and navigation only) — `DONE / PASS`, owner approved 2026-10-02

`ai-context/planning/stories/WI-009-003-six-area-navigation-and-safety-discoverability.md` is closed (`VER-WI-009-003`). It authorizes nothing further. Its scope, kept for reference:

| In scope for WI-009-003 | Notes |
|---|---|
| `src/FamilySafety.Web/src/app/navigation.ts`, `group-landing.ts` (new) and their specs | Six destinations; typed view → area table; data-free landings |
| `src/FamilySafety.Web/src/app/app.routes.ts`, `app.ts`, `app.html` | `/plans-trips` and `/safety`; `routerLink` nav; `aria-current` `page`/`true`; `h1` focus after user navigation. **No extraction.** |
| `src/FamilySafety.Web/src/styles.scss` (shell nav rules only) | Link styling; 3 × 2 grid at about 390 px |
| `shell.spec.ts`, `home.spec.ts`, `first-use.spec.ts` (and `app.spec.ts` if affected) | Navigation expectations and test setup only |
| `e2e/navigation.spec.ts` (new); `e2e/trip-audience-removal.spec.ts`, `e2e/invitation-handoff.spec.ts` | New spec; the two existing specs **locator only** (nav items are now links) |
| `evidence/WI-009-003/`, `VER-WI-009-003.md`, the story's checkboxes and implementation record | Do not change acceptance criteria |

WI-009-003 does **not** authorize: server, API, authorization, schema, or migration changes; `REQ-UX-014` item URLs, single-item reads, Home item links, or cross-Family deep links; Home hub changes; any change to Concern, Incident, Plan, or Trip behavior or components; `App` extraction; new dependencies (including `@axe-core/playwright`); budget increases; `OD-002`; a "More" menu or any extra destination.

### Historical: WI-009-002 (test infrastructure only) — `DONE / PASS`, owner approved 2026-10-02

`ai-context/planning/stories/WI-009-002-test-artifact-lifecycle-and-cleanup.md` is closed (`VER-WI-009-002`). It added `scripts/test-artifacts.ps1`, `scripts/tests/check-test-artifacts-safety.ps1`, the run-record lifecycle (`FAMILY_SAFETY_E2E_KEEP_RUN_FILES`), and transient screenshots in `e2e/captures/`. It authorizes nothing further.

### Historical: WI-009-001 (test infrastructure only) — `DONE / PASS`, owner approved 2026-10-02

`ai-context/planning/stories/WI-009-001-playwright-per-spec-scenario-isolation.md` was `APPROVED — EXECUTION AUTHORIZED` against `CTX-1.27` and is now closed. Its scope, kept for reference:

| In scope for WI-009-001 | Notes |
|---|---|
| `src/FamilySafety.Web/e2e/playwright.config.ts`, `e2e/fixtures/`, new global setup and teardown | Per-spec scenario helper, ownership manifest, Angular-only `webServer`; remove `isInvitationOnlyRun` |
| `src/FamilySafety.Web/e2e/*.spec.ts` (4) | Scenario declaration only. **No assertion changes.** |
| `scripts/start-invitation-scenario.ps1` | `-NoBuild` switch only |
| `scripts/remove-scenario-database.ps1` (new) | Guarded removal: all ten deletion conditions; fail closed; non-zero exit; no credentials |
| `docs/local-development.md` | How specs obtain isolated scenarios; cleanup and retention |
| The WI-009-001 story (task checkboxes, implementation record) and `VER-WI-009-001.md` | Do not change acceptance criteria |

**Database deletion** is authorized only for databases the current Playwright run created, after the spec passes, when every safeguard holds. Enumerate and record PRE-EXISTING databases first, and never delete them. If any safeguard fails, do not drop.

WI-009-001 does **not** authorize:

- any change under `src/FamilySafety.Api`, `Application`, `Domain`, `Infrastructure`, or `src/FamilySafety.Web/src`;
- fixture, identity, or outbox code changes;
- schema or migration changes;
- new packages (including `@axe-core/playwright`);
- the later Phase 9 findings;
- deleting `e2e-alt/` or the root build folders.

### Historical: WI-008-004 (Angular UI only) — `DONE / PASS`, owner approved 2026-10-02

`ai-context/planning/stories/WI-008-004-incident-ui-and-start-an-incident-flow.md` was `APPROVED — EXECUTION AUTHORIZED` against `CTX-1.26` (owner, 2026-10-01) and is now closed. Its scope, kept for reference, explicitly authorized changes to:

| In scope for WI-008-004 | Notes |
|---|---|
| `src/FamilySafety.Web/src/app/incidents/` (new) | Incident API client and models, focused components, scoped stylesheets, specs |
| `src/FamilySafety.Web/src/app/concerns/concerns.ts`, `.html`, `.spec.ts` | **Only** the "Separate Incident" card with "Start an Incident…", and opening a Concern from navigation state. No change to Concern behavior, API calls, or permissions. |
| `src/FamilySafety.Web/src/app/app.routes.ts`, `app.ts`, `app.html`, `home.spec.ts` | Incident routes, the `incidents` view titled "Safety Incidents", lazy rendering, and the Home card retitled "Safety coordination" with "Open Incidents". **No navigation button.** |
| `src/FamilySafety.Web/e2e/incident-workflow.spec.ts` (new); optionally a small helper in `e2e/fixtures/` | Playwright Incident journeys |
| `ai-context/planning/verification/evidence/WI-008-004/` (new) | Rendered evidence and its `README.md` |
| The WI-008-004 story (task checkboxes, implementation record) and `VER-WI-008-004.md` | Do not change acceptance criteria |

The WI-008-003 server contract is authoritative. WI-008-004 does **not** authorize any of the following:

- `.NET`, API, permission, or schema changes, or migrations;
- resolving `OD-001` or `OD-002`, or adding a navigation destination;
- notification integrations, location, Check-In, Sharing Sessions, or maps;
- emergency escalation or Incident reopening;
- unrelated refactoring.

Its approved decisions (`UI-D1`–`UI-D4`, the Home entry point, the page title, the routes, and the navigation-state handoff) are recorded in the story's "Owner decisions" section. Do not reopen them unless you find a genuine canonical contradiction; that is a stop condition. The UI rules in the historical section below apply.

### Historical: WI-008-003 (server-side) — `DONE / PASS`, owner approved 2026-10-01

`ai-context/planning/stories/WI-008-003-safety-incident-domain-and-access-foundation.md` was `APPROVED — EXECUTION AUTHORIZED` against `CTX-1.26` and is now closed. Its scope, kept for reference, explicitly authorized changes to:

| In scope for WI-008-003 | Notes |
|---|---|
| `src/FamilySafety.Domain/Incidents/` (new) | Incident domain types and invariants |
| `src/FamilySafety.Application/Incidents/` (new) | Contracts, validation, service, repository interface |
| One narrow **read-only** Concern access query (`Application/Concerns` interface + `Infrastructure/Persistence` implementation) | For create-from-Concern and lineage visibility only. **No change to existing Concern behavior or permissions.** |
| `src/FamilySafety.Infrastructure/Persistence/` (`EfIncidentRepository.cs`, `FamilySafetyDbContext.cs`, `DependencyInjection.cs`, `Migrations/`) | Mapping, authorization, concurrency, audit, reconciliation, **one additive migration plus snapshot** |
| `src/FamilySafety.Api/Incidents/` (new) and `Program.cs` | Private API routes and the maintenance hosted service |
| `tests/FamilySafety.Foundation.Tests/Incident*Tests.cs` (new) | Domain, API, and SQL-gated tests |
| The WI-008-003 story (task checkboxes, implementation record) and `VER-WI-008-003.md` | Do not change acceptance criteria |

WI-008-003 does **not** authorize any of the following:

- Incident UI or any `src/FamilySafety.Web` change;
- creating or implementing `WI-008-004`;
- notification integrations;
- location, Check-In, or Sharing expansion;
- automatic emergency escalation;
- `OD-002` resolution;
- changes to existing tables' schema;
- changes to Family membership semantics;
- unrelated refactoring.

Its accepted design decisions are listed in the story's Execution authorization section. Do not reopen them unless you find a genuine canonical contradiction; that is a stop condition.

### Historical: UI / mockup-alignment scope (WI-008-002 and the DEC-070 site alignment — complete)

The rules below applied to the completed presentation-only UI work. Reuse them only when a future approved **UI** work item, such as `WI-008-004`, authorizes UI changes:

| Usually in scope for UI alignment | Notes |
|---|---|
| Angular templates and component styles for the screens named by the UI work item (`src/FamilySafety.Web/src/app/**`) | Restructure freely for presentation. Keep behavior, API calls, bindings, and required copy. |
| Shared styles (`src/styles.scss`, `src/app/app.scss`) and shell markup (`app.html`) | Only as far as the owner's palette, shell, and navigation decisions allow. |
| Component specs and Playwright specs | Only to follow changed markup or locators. Assertions keep the same meaning. |
| The work item's story and VER record, and its evidence folder | Notes and evidence only. Do not change acceptance criteria. |

For **presentation-only UI work**, do not change .NET code, API routes, contracts, permissions, schema, or migrations unless that UI work item explicitly authorizes it.

## For a separately authorized product work item

The permissions below describe the usual product work-item workflow. They do not expand the current documentation-only Wave 1 authorization.

- Edit the files the current authorized work item allows (see above).
- Run `dotnet build`, `dotnet test`, Angular/Vitest tests, Playwright, and the production build.
- Start the API and Angular dev server locally (see `docs/local-development.md`).
- Inspect diffs, Git status, and file listings.
- Update the current work item's story (task checkboxes, implementation record) and create its VER record.

## You must not (any work item)

- Change code outside the current authorized work item's scope, or expand that scope silently.
- Edit canonical contracts, workflows, requirements, or decisions. Update `project-status.md` / `roadmap.md` only for status changes the owner authorizes.
- Resolve or imply a resolution of `OD-002`. `OD-001` is resolved by `DEC-074` (`REQ-UX-013`) and `OD-004` by `DEC-071`; do not reopen either.
- Change navigation destinations, count, or order outside an authorized navigation work item. The approved model is `REQ-UX-013`.
- Add mockup-only functionality, or fetch data the approved endpoints do not return.
- Raise Angular budget thresholds or add screenshot golden baselines.
- Overwrite or delete existing evidence PNGs. Write new files with distinct names.
- Mark any work item `DONE / PASS` yourself. Hand back in `VERIFYING / OWNER REVIEW`. UI items additionally require explicit owner visual approval.
- Rework the approved visual system or the accepted differences in `DEC-070` without a new owner request.

## Product and privacy invariants

- Server authorization is authoritative. Client-side role checks are display hints only; never let them grant or imply access.
- Family membership, Family Administrator, relationship, Safety Circle, Trusted Contact, subject status, Concern participation, and lineage never grant Concern or Incident access by themselves.
- Denied access must not disclose whether a Concern or Incident exists.
- No automatic Incident creation, escalation, danger determination, risk scoring, notification, or dispatch.
- No access to location, Check-In, Sharing, Trip, or private profile data through Safety records.
- Preserve every guardrail in `MOCKUP-ALIGNMENT.md` § Privacy guardrails for UI work.
- Keep tone calm and neutral: no urgency, lateness, danger, safety verdicts, or accusation.

## Stop conditions

Stop, do not guess, and report back when:

- `ai-context` sources conflict (`BLOCKED—CONTEXT CONFLICT`, citing both), or this handoff conflicts with `ai-context` (follow `ai-context` and report it).
- The work would need a change outside the current work item's authorized scope.
- **Server work (WI-008-003):** any stop condition in the story's "Stop conditions during future execution" section. These include a change to canonical Incident or Concern permissions, membership semantics, `OD-002` behavior, notifications, location/Check-In/Sharing, generic authorization architecture, backup infrastructure, existing tables' schema, or unrelated domains.
- Tests fail for reasons you cannot attribute to your changes.
- **UI work:**
  - a visual change would require a behavior, copy-requirement, API, or permission change;
  - a mockup idea needs data the API does not return;
  - fidelity requires an undecided palette, shell, or navigation change;
  - a screen has no primary reference and you would have to invent a target.

## Scope discipline

- No silent scope expansion. If something outside scope looks worth doing, list it under "Recommended follow-ups" in the closeout.
- For UI work, small shared visual tweaks for consistency are allowed; a site-wide redesign is not. **Owner visual approval is required.**

## Practical notes for Claude sessions

- The environment is Windows. PowerShell is the primary shell, and Git Bash is also available. Use absolute paths such as `C:\Dev\blevins\...`. Python is not installed.
- SQL-gated .NET tests run only when `RUN_FAMILYSAFETY_SQL_TESTS=1`. Without it, they return early and report as passed. See `VALIDATION.md` § 4.
- Use the file-read tool to view PNG mockups and evidence directly.
- Playwright runs headless Chromium through the project config. Do not add capture scripts to the repo unless asked.
- Prefer editing existing files over creating new ones. Match the surrounding C#, Angular, and SCSS style.
