# Project State — Orientation Summary

Snapshot reconciled after the **2026-10-03 Phase 9 closure**. This is a summary for orientation only. The authoritative state is `ai-context/project-status.md` and `ai-context/planning/roadmap.md`; re-check them at the start of each session, because this file can go stale.

## Baseline and phase

| Field | Value |
|---|---|
| Active context baseline | `CTX-1.28` — `APPROVED` (`DEC-075`: `REQ-UX-014` amended — an authorized item whose Family is not selectable never creates a Family context; only Trip creator retention under `REQ-PRIV-022` renders without a switch; one-time Family-list refresh. `CTX-1.27` historical) |
| Previous phase | Phase 8 — Safety Coordination — `COMPLETE / CLOSED` (`DEC-072`, 2026-10-02; `VER-PH-008`). Phase 7.5 closed earlier (`DEC-065`). |
| Current phase | Phase 9 — Integration & UX Refinement — `COMPLETE / CLOSED` (`DEC-081`; `VER-PH-009`, all 15 exits PASS) |
| Phase 9 planning basis | `PRP-030 — Phase 9 Scope, Themes and Exit Criteria` — `APPROVED` (`DEC-074`; 15 exit criteria) |
| Next phase | Phase 9.5 — MVP Feature Completion — `READY TO BEGIN` (separately scoped approval required; `OD-002` remains open), then Phases 10 and 11, numbering unchanged |
| Phase 8 planning basis | `PRP-028` — `APPROVED` (`DEC-067`, `DEC-068`) |
| Incident planning basis | `PRP-029` — `APPROVED` (`DEC-071`); canonical as `REQ-SAFE-016` and `REQ-PRIV-026`. `OD-004` resolved. |
| **Current implementation objective** | **None authorized.** Repository cleanup Wave 1 is documentation maintenance only. See [CURRENT-TASK.md](CURRENT-TASK.md). |
| Home dashboard | **Complete.** `WI-009-006 — Home Dashboard Prototype Alignment` is `DONE / PASS` — `OWNER APPROVED` (2026-10-03; `VER-WI-009-006`; `DEC-076`). <ul><li>Presentation-only Home after `references/ux/home-dashboard` panel ①:<ul><li>a time-of-day greeting, with the Family in the subtitle;</li><li>real Events, Trips, and Plans cards with compact empty states;</li><li>a compact Safety coordination card and a View Family action;</li><li>text-only placeholders for Recent Family Updates, My Tasks, and Active Sharing.</li></ul></li><li>Styles are in `src/FamilySafety.Web/src/app/home.scss`.</li><li>No Phase 9.5 capability.</li></ul> |
| Family dashboard | **Complete.** `WI-009-007 — Family Dashboard Prototype Alignment` is `DONE / PASS` — `OWNER APPROVED` (2026-10-03; `VER-WI-009-007`; `DEC-077`). <ul><li>`/family` is the coordination dashboard after panel ②:<ul><li>members and member detail;</li><li>Waiting for you;</li><li>text-only placeholders for birthdays, updates, and check-ins.</li></ul></li><li>`/family/settings` holds Family details, relationships, and the administrator-only Invite card. The gear is visible to all members and is navigation only.</li><li>Styles are in `family.scss` and `family-settings.scss`.</li><li>No Phase 9.5 capability.</li></ul> |
| Safety dashboard | **Complete.** `WI-009-009 — Safety Dashboard Prototype Alignment` is `DONE / PASS` — `OWNER APPROVED` (2026-10-03; `VER-WI-009-009`; `DEC-079`). <ul><li>`/safety` is a read-only dashboard after panel ④: separate Open Concerns and Open Safety Incidents summaries (existing list reads, Open only, no reason or workspace detail), a Recent Check-Ins placeholder, and an About Concerns and Incidents card.</li><li>No create, resolve, contact, settings, or emergency control; no selected Family needed; Family labels for multi-Family callers.</li><li>Back to Safety on `/concerns` and `/incidents`. It revisits `WI-009-003` D1 / `AC-15` for `/safety` only.</li><li>Standalone deferred component `safety-dashboard.ts`. No Phase 9.5 capability.</li></ul> |
| First use and invitation arrival | **Complete.** `WI-009-010 — First-Use and Invitation Arrival Workflow` is `DONE / PASS` — `OWNER APPROVED` (2026-10-03; `VER-WI-009-010`; `DEC-080`, O1–O11). <ul><li>Invitation arrival ends on `/home` with a one-time "You joined {Family}." announcement; the profile review stays optional.</li><li>Unavailable, capacity, and declined invitations offer Go to Home; messages and privacy semantics unchanged.</li><li>No-Family Home and the shared Family-required prompt give plain guidance and a Review your profile link; one Family-required message per page.</li><li>A first Family choice keeps the page; Invite feedback sits in the Invite card.</li><li>Client only; no API, schema, authorization, Family-creation, or authentication change.</li></ul> |
| Item URLs and deep links | **Complete.** `WI-009-005 — Refresh-Safe Item URLs and Authorize-First Deep Links` is `DONE / PASS` — `OWNER APPROVED` (2026-10-03; `VER-WI-009-005`). <ul><li>It covers item routes, authorize-first Family switching (OD-A to OD-I), the generic unavailable state, list and Home item links, Concern URL conversion, and the opt-in second-Family E2E fixture.</li><li>Its `AC-16` Home-presentation clause is superseded by `WI-009-006` (`DEC-076`).</li></ul> |
| Event and Plan item reads | **Complete.** `WI-009-004 — Event and Plan Single-Item Read Foundation` is `DONE / PASS` — `OWNER APPROVED` (2026-10-02; `VER-WI-009-004`). It adds `GET /api/events/{eventId}` and `GET /api/plans/{planId}` under the unchanged access rules: one shared visibility query per repository, used by both the list and the item read. Responses are the existing `FamilyEventView` / `FamilyPlanView`. Every missing or unauthorized case is a uniform `404` with an empty body. Recorded follow-up, unscheduled: the Plan create `Location` header. |
| Six-area navigation | **Complete.** `WI-009-003 — Six-Area Navigation and Safety Discoverability` is `DONE / PASS` — `OWNER APPROVED` (2026-10-02; `VER-WI-009-003`). Six `routerLink` destinations (Home, Family, Events, Plans & Trips, Safety, Me / Settings); `/plans-trips` remains a data-free landing. `/safety` was later replaced by the data-bearing dashboard under `DEC-079`. `aria-current` is `page`/`true`; the 390 px navigation uses a 3 × 2 grid; heading focus follows user navigation. Navigation is defined in `src/FamilySafety.Web/src/app/navigation.ts`. |
| Test artifact lifecycle | **Complete.** `WI-009-002 — Test Artifact Lifecycle and Cleanup` is `DONE / PASS` — `OWNER APPROVED` (2026-10-02; `VER-WI-009-002`). `.\scripts\test-artifacts.ps1` is the read-only status report; its cleanup is dry run by default, by exact name or run ID, with `-Execute`. |
| Test isolation | **Complete.** `WI-009-001 — Playwright Per-Spec Scenario Isolation` is `DONE / PASS` — `OWNER APPROVED` (2026-10-02; `VER-WI-009-001`). Each spec gets its own scenario database and API; successful-spec databases are removed by guarded cleanup, failed ones are retained. |
| Incident implementation / UI | **Complete.** The server foundation is `WI-008-003` (`DONE / PASS`; additive migration `20261002013148_SafetyIncidentDomainAndAccessFoundation`). The Angular Incident UI and Start an Incident from a Concern are `WI-008-004` (`DONE / PASS` — `OWNER APPROVED` 2026-10-02; `VER-WI-008-004`). |

## Historical completed work for orientation

- **Phase 7.5 visual alignment (PRP-027, now closed):** shared shell/core patterns (WI-075-010), Home (011), Family/first-use (012), Events (013), Plans (014), Trips (015). All `DONE / PASS` with owner visual approval. These established the card/form/button/status patterns and the KEEP/ALIGN/ADAPT/DROP method. Their original evergreen token palette was later replaced by the `DEC-070` blue reference visual system (see below), which is the current visual system for all screens (`src/FamilySafety.Web/src/styles.scss`).
- **WI-008-001 — Concern Domain and Access Foundation:** `DONE / PASS`. Server-side Concern model, private API, authorization, lifecycle, retention, and nondisclosing 404 behavior. Denied-attempt audit handling clarified by `DEC-069`. The server is the sole authorization authority.
- **WI-008-002 — Concern MVP User Interface:** `DONE / PASS` with owner visual approval (2026-10-01), first aligned in evergreen tokens and then restyled to the blue system under `DEC-070`. It includes small shared-shell spacing and title-size reductions. Evidence is in `evidence/WI-008-002/mockup-alignment/`.
- **WI-008-003 — Safety Incident Domain and Access Foundation:** `DONE / PASS`, owner approved 2026-10-01. It delivers the server-side Incident model, private API, explicit grants, coordinator handoff and coordinator-loss freeze, non-authorizing Concern lineage, tasks, timeline, 365-day retention and audit, `DEC-069` denied summaries, and one additive migration. Evidence: `VER-WI-008-003`, 191/191 tests with the SQL gate on. Known non-blocking follow-ups (not stories): Concern repository stale-tracking, display-name profile loading, and reconciliation efficiency.
- **WI-008-004 — Incident UI and Start-an-Incident Flow:** `DONE / PASS` — `OWNER APPROVED` (2026-10-02). It delivers the `/incidents` routes (no navigation destination), the Home "Safety coordination" card, the list, creation (direct and from a Concern), detail, people and handoff, entries and contact attempts, tasks, Frozen, Resolved, and unavailable states. Evidence: `VER-WI-008-004`, 118/118 Angular tests, two Incident Playwright journeys, and `evidence/WI-008-004/`.
- **Recommended follow-up areas (not stories):**
  - ~~Shared-shell mockup alignment~~: done in the site-wide alignment (`DEC-070`).
- **Site-wide reference mockup alignment:** owner-approved 2026-10-01 (`DEC-070`). It covers the blue palette, desktop sidebar and slim top bar, single page titles, and card system on all screens. Presentation only. Evidence is in `ai-context/planning/verification/evidence/site-mockup-alignment/`.
  - ~~Playwright full-suite database isolation~~: done in `WI-009-001` (`DONE / PASS`).
  - Concern and Incident Playwright specs no longer rewrite evidence screenshots: they write to `src/FamilySafety.Web/e2e/captures/` (`WI-009-002`, D1 option A; `DONE / PASS`).

## Not authorized

- Any application code, test, or script change: no product implementation work item is currently authorized. `WI-009-001` through `WI-009-011` and WI-008-001 through WI-008-004 are closed. Repository cleanup Wave 1 is documentation maintenance only.
- Any database or folder deletion beyond the Playwright harness's own guarded per-run cleanup and the exact-name or exact-run-ID, `-Execute`-only, manifest-owned workflow of `scripts/test-artifacts.ps1` (`WI-009-002`). The one-time owner-approved cleanup is complete (`ai-context/planning/verification/evidence/WI-009-001/database-cleanup-completion-2026-10-02.md`). The 15 remaining uncertain databases need an owner decision before any change.
- Any production server, API, permission, schema, or migration change, or any client change, without a newly authorized work item.
- Any change to existing Concern behavior, Family membership semantics, or existing tables' schema.
- Check-In, Sharing Sessions, location, maps, notifications, emergency actions, risk/danger scoring, audit UI, `OD-002` resolution, unrelated refactoring.

## Product decisions (OD-002 and OD-003 open; do not resolve or imply)

- `OD-001` — **RESOLVED** by `DEC-074` (`REQ-UX-013`): six areas — Home, Family, Events, Plans & Trips, Safety, Me / Settings. Navigation groupings only. Implement only under an authorized navigation work item.
- `OD-002` — Persistent or indefinite location sharing in MVP → assigned to Phase 9.5 (`DEC-073`); outside Phase 9
- `OD-003` — Minor/guardian functionality — outside MVP (`DEC-053`)

`OD-004` (Incident-history retention) is **resolved** by `DEC-071`: 365-day history and audit retention, and 30-day backup aging (`REQ-PRIV-026`).

## Dependencies and environment facts

- **Predeployment dependency:** backup-aging and restore-operation evidence (`DEC-068`, `DEC-071`) is still required before any deployment. Phase 8 closure does not establish deployment readiness or backup compliance.
- **Source control:** `C:\Dev\blevins` has a Git repository. The owner performs commits; report the actual `git status` result in closeouts.
- **Build budget warnings already exist:**
  - `concerns.scss` ≈ 7.53 kB and `trips.scss` ≈ 5.79 kB, against a 4 kB warning and 8 kB error component budget. `concerns.scss` has little headroom.
  - `app.scss` is now under 4 kB, because shared shell and primitive styles live in global `src/styles.scss`.
  - At the Phase 9 closure build (`VER-PH-009`), `main` was 597.77 kB, complete initial total 610.24 kB, estimated transfer 134.28 kB, against a 500 kB warning budget. Earlier WI-009-007 through WI-009-010 figures are historical baselines, not the closure result. `home.scss`, `family.scss`, and `family-settings.scss` were each under 4 kB. The Incident feature was a separate lazy chunk of about 138 kB.
  - `trips.scss` also warns.
  - Do not raise thresholds.

## Known inconsistencies in ai-context

- `ai-context/planning/decision-log.md` is a partial excerpt (DEC-065 to DEC-068). The full log through Phase 9 closure (`DEC-081`) is `ai-context/decision-log.md`.
- The old build-output folders (`wi075003-build-*`, `wi075004-build*`) and the empty `src/FamilySafety.Web/e2e-alt/` folder were deleted on 2026-10-02 under owner authorization (see the cleanup completion record).
- The stale Phase 8 sentences in `project-status.md` and `roadmap.md` were corrected at WI-008-002 closure.
