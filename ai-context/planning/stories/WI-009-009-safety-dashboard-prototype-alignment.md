---
Status: DONE / PASS — OWNER APPROVED
Authority: Project-owner authorization (2026-10-03) after the panel ④ design review; owner decisions O1–O11 (DEC-079) and O12–O14 (owner visual review)
Context-Baseline: CTX-1.28
Version: 1.0
---

# WI-009-009 — Safety Dashboard Prototype Alignment

## Metadata

- **Work Item ID:** `WI-009-009`
- **Status:** `DONE / PASS` — `OWNER APPROVED` (owner review 2026-10-03; `VER-WI-009-009`; `DEC-079`). AC-01 to AC-13 `PASS`.
  - Status history:
    1. `IMPLEMENTED / OWNER VISUAL REVIEW`.
    2. Owner visual approval `PASS`.
    3. `VERIFYING / FINAL REGRESSION`.
    4. `DONE / PASS`.
  - All on 2026-10-03.
- **Phase:** `9 — Integration & UX Refinement`
- **Context baseline:** `CTX-1.28` (no requirement change; no `CTX-1.29`)
- **Planning basis:** the panel ④ design review (2026-10-03); `WI-009-006` / `DEC-076`, `WI-009-007` / `DEC-077`, `WI-009-008` / `DEC-078` (dashboards with honest placeholders); `DEC-070` visual direction; `DEC-079`.
- **Visual target:** `ai-context/references/ux/home-dashboard/home-dashboard-overview.png`, **panel ④ "Safety-Aware Dashboard"** only (larger copy: `claude-handoff/reference/panel4-safety-aware.png`). The newer `home-dashboard-redesign.png` in the same folder is **not** a design authority for this item (O1). UX mockups are non-authoritative design evidence.
- **Requirement references (preserved, not changed):** `REQ-UX-013` (Safety is a navigation grouping; Concerns and Incidents stay separate; reaching Safety grants no access and implies no item exists), `REQ-UX-014` (item URLs, authorize-first Family switching), `REQ-PRIV-025`, `REQ-PRIV-026`, `REQ-PRIV-027`, `REQ-SAFE-008` (MVP states Open / Frozen / Resolved only), `REQ-SAFE-015`, `REQ-SAFE-016` (neither a Concern nor an Incident proves danger or safety), `REQ-SHARE-006`–`008` (Check-In, not implemented).

## Goal

`/safety` becomes a calm, read-only Safety dashboard in the panel ④ composition, showing the Open Concerns and Open Safety Incidents the caller is already authorized to see, as two separate summaries. Concern and Incident workflows, routes, authorization, and privacy are unchanged.

## Owner decisions (2026-10-03)

- **O1 — Canonical panel:** panel ④ of `home-dashboard-overview.png`, the board used for panels ①–③. The redesign board is not authoritative here.
- **O2 — Data-bearing dashboard:** `/safety` replaces the data-free landing with a read-only dashboard: Open Concerns, Open Incidents, a Check-ins placeholder, and static guidance. Concerns and Incidents stay separate cards and workflows: no combined list, timeline, count, workflow, or create menu.
- **O3 — Governance:** `DEC-079`. It revisits `WI-009-003` D1 and `AC-15` for `/safety` only. `CTX-1.28` stays active; the owner's reading is that `REQ-UX-013` permits this dashboard without amendment.
- **O4 — Title and subtitle:** "Safety" / "Concerns and Safety Incidents shared with you." No monitoring, proactive-safety, all-clear, everyone-is-safe, or danger wording.
- **O5 — Multiple Families:** no selected Family required. All authorized Open items from the existing list reads. When the caller has more than one active Family, each row shows the `familyGroupName` already present in that authorized row. No server field added. Item links use the authorize-first behaviour of `REQ-UX-014`.
- **O6 — No create actions:** no Create a Concern, Start an Incident, create menu, or escalation shortcut on `/safety`. No `/concerns/new`.
- **O7 — Check-ins placeholder:** heading "Recent Check-Ins"; copy exactly "Check-ins shared with you will appear here when available." Static only.
- **O8 — Guidance card:** "About Concerns and Incidents", reusing the existing Concern and Incident descriptions and the boundary "Neither one proves danger or safety."
- **O9 — Back to Safety:** a semantic link to `/safety` on `/concerns` and `/incidents`. Navigation only; item and detail routing unchanged.
- **O10 — No Concern reason** on the dashboard.
- **O11 — Home Safety card:** out of scope; unchanged.

## Owner visual approval and final decisions (2026-10-03)

- **Owner visual approval: `PASS`.** Approved: `/safety` as the read-only Safety dashboard; the separate Open Concerns and Open Safety Incidents cards; the Recent Check-Ins placeholder; the About Concerns and Incidents card; no create or escalation actions; no combined timeline; no contact-attempt summary; no Safety Settings; no emergency controls; the calm visual treatment; multi-Family labels; Back to Safety links; the 1440 / 1024 / 768 / 390 layouts; the standalone component.
- **O12 — Row wording:** keep "Concern about {name}" and "Incident about {name}", using only the already-authorized subject display name. No reason, lineage, notes, or other subject information.
- **O13 — Additional open items:** "Additional open Concerns are available." and "Additional open Safety Incidents are available." View all provides navigation; the hidden count is never shown.
- **O14 — Check-Ins placeholder height:** the card may match the guidance card's height, but the note stays top-aligned at its natural size; it is not stretched or centred.
- **Cleanup caused by `DEC-079`:** the unreachable Safety branch of `group-landing.ts` is removed; the Plans & Trips landing is unchanged.

## Scope

- **Routes:** unchanged. `/safety` (Safety, `aria-current="page"`); `/concerns`, `/concerns/:concernId`, `/incidents`, `/incidents/new`, `/incidents/:incidentId` (Safety, `aria-current="true"`). No primary-navigation change.
- **Standalone component:** `src/app/safety-dashboard.ts`, `.html`, `.scss`, rendered from `app.html` inside `@defer (on immediate)`, like Incidents. It is a separate lazy chunk. No `App` refactor.
- **Open Concerns card:** `GET /api/concerns` (existing), `state === 'Open'` only, most recent activity first, up to 3 rows. A row shows initials, "Concern about {subject}" (link to `/concerns/:id`), the existing Open state as a text pill, "Created by you" or "You are a selected participant", the Family name when the caller has more than one Family, and the last activity. With more than 3: "More open Concerns are listed in Concerns." (no digit). View all goes to `/concerns`. Empty: "No open Concerns are shared with you."
- **Open Safety Incidents card:** `GET /api/incidents` (existing summaries), Open only, same ordering and limit. A row shows initials, "Incident about {subject}" (link to `/incidents/:id`), Open, the existing coordinator line, the Family name when needed, and the last activity. View all goes to `/incidents`. Empty: "No open Safety Incidents are shared with you." Static footnote: "Contact attempts, tasks, and timelines remain inside each Incident."
- **Recent Check-Ins:** the O7 placeholder in the Home/Events placeholder style. No request, name, avatar, time, count, or state.
- **About Concerns and Incidents:** the two existing descriptions and "Neither one proves danger or safety. Each is shared only with the people chosen for it."
- **Loading and errors:** each card loads and fails independently, with its own retry; a 401 shows the existing session-ended message.
- **Back to Safety:** in `app.html`, above the title on `/concerns` and on the `/incidents` list (not `/incidents/new` or item pages). It reuses the Back to Events link style.
- **Responsive layout:**
  - **≥ 64 rem (1440, 1024):** sidebar; 2 × 2 grid with equal columns: Open Concerns | Open Safety Incidents, then Recent Check-Ins | About.
  - **< 64 rem (768, 390):** one column in DOM order: Concerns, Incidents, Check-Ins, Guidance.
  - **< 44 rem (390):** View all becomes a full-width 44 px target below each card heading. Row title links have `min-height: 2.75rem` (at least 44 px, measured in Playwright). No horizontal overflow.
- **Styles:** calm blue tones only; no alarm red, amber, or verdict colours. Status is always text.

## Explicit exclusions

- Panel ④'s Recent Contact Attempts card, combined Safety Timeline and Today filter, "Monitoring" status, Contact / Call / Send message, Mark as Resolved on the dashboard, Safety Settings (button or route), expected check-in times, last known location, alarm-red surfaces; the redesign board's "Need urgent help? / Emergency options".
- Check-Ins, Trusted Contacts, Safety Circles, Sharing Sessions, notifications, location / `OD-002`, emergency response, messaging, activity feeds, and any Phase 9.5 capability.
- Any production API, endpoint, contract, schema, migration, authorization, or server change.
- Home, Family, Events, Plans & Trips, Concern and Incident component changes.

## Acceptance checks

1. `AC-01` The `/safety` 1440 composition follows panel ④ as adapted by O2–O10.
2. `AC-02` Only Open rows returned by `GET /api/concerns` and `GET /api/incidents` appear, in separate cards; Frozen and Resolved never appear; no combined list, count, or timeline.
3. `AC-03` No Concern reason, entry, contact attempt, task, or timeline content appears on `/safety`.
4. `AC-04` Empty states and all copy contain no safety verdict, monitoring, or emergency language.
5. `AC-05` No create, resolve, contact, settings, or emergency control on `/safety`.
6. `AC-06` `/safety` renders without a selected Family; Family names appear only for multi-Family callers and come from the authorized rows; item links use authorize-first switching.
7. `AC-07` Recent Check-Ins contains only its heading and the O7 sentence.
8. `AC-08` Back to Safety on `/concerns` and the `/incidents` list returns to `/safety` and focuses its `h1`; item pages are unchanged.
9. `AC-09` `aria-current` is `page` on `/safety` and `true` on the Concern and Incident pages.
10. `AC-10` 1440, 1024, 768, and 390 px follow the Scope layout with no horizontal overflow.
11. `AC-11` (after visual approval) Focused Angular specs updated and added; `group-landing.spec.ts` and the `/safety` assertions of `e2e/navigation.spec.ts` (WI-009-003 `AC-15`) updated as a documented, intentional meaning change; `npm run build` passes with no new budget warning.
12. `AC-12` (after visual approval) Final regression, including affected Playwright specs and a participant / non-participant / subject visibility journey.
13. `AC-13` Owner visual approval.

## Implementation record (2026-10-03)

- **Files:**
  - `src/FamilySafety.Web/src/app/safety-dashboard.ts`, `.html`, `.scss`: new.
  - `app/app.html`: `/safety` renders the deferred dashboard; the Plans & Trips landing is unchanged; Back to Safety on `/concerns` and the `/incidents` list.
  - `app/app.ts`: imports `SafetyDashboard`; the Safety subtitle.
  - `app/events-dashboard.scss`: the Back link rules also cover `.safety-back` / `.safety-back-link`.
  - `group-landing.ts`: its `safety` choice was left in place during visual review and removed in the testing pass (see Owner closure).
- **Build:** one `npm run build`. It passes with the existing warnings only: initial total 607.74 kB (`main` 595.27 kB, was 594.55 kB; estimated transfer 133.91 kB), `concerns.scss` 7.53 kB, `trips.scss` 5.79 kB. The dashboard is a lazy `safety-dashboard` chunk of 11.25 kB. `safety-dashboard.scss` raised no budget warning (3.2 kB source).
- **Not run (by owner direction):** Angular specs, Playwright, and regression. No tests were written or changed. Expected to need updates: `group-landing.spec.ts` (Safety branch), `e2e/navigation.spec.ts` (`/safety` request listener and data-free assertions), and any shell spec asserting the old Safety subtitle "Choose Concerns or Safety Incidents."
- **Review renders (transient, not canonical evidence):** `src/FamilySafety.Web/e2e/captures/safety-dashboard-review/`:
  - `safety-live-{1440,1024,768,390}`: real Development data (no Concerns or Incidents, so empty states).
  - `safety-mock-multi-{1440,1024,768,390}`, `safety-mock-single-1440`: browser-only injected sample data, labelled on the image; nothing was written to the database. The sample includes Resolved and Frozen rows and a sentinel Concern reason; none rendered.
  - `concerns-live-{1440,390}`, `incidents-live-{1440,390}`, `incidents-new-live-1440`.
  - `safety-contact-sheet.png`.
  - Checked during capture: zero horizontal overflow on every render; Safety `aria-current` `page` / `true`; Back to Safety lands on `/safety` with focus on the `h1`; none of All clear, Everyone is safe, Monitoring, Emergency, Create a Concern, Start an Incident, or Safety Settings appears in the dashboard.

## Remaining tasks

- [x] `TASK-001` Implement O1–O11.
- [x] `TASK-002` Review renders at 1440, 1024, 768, and 390, plus the contact sheet.
- [x] `TASK-003` Owner visual approval (`AC-13`): `PASS`, 2026-10-03, with O12–O14 applied.
- [x] `TASK-004` Testing pass: remove the unused landing Safety branch; update the affected specs; add dashboard coverage; run the focused specs and build.
- [x] `TASK-005` Final regression (`AC-12`), evidence, `VER-WI-009-009`, and closure.

## Owner closure (2026-10-03)

- **Decision:** `DONE / PASS` — `OWNER APPROVED` (`DEC-079`). Evidence is in `evidence/WI-009-009/`.
- **Verification** (see `VER-WI-009-009`):
  - Angular: focused 90/90; full suite 244/244, including the new `safety-dashboard.spec.ts` (14 tests).
  - Focused Playwright: the new `e2e/safety-dashboard.spec.ts` 4/4 (including the privacy regression); the updated `navigation` spec passing.
  - One full `npm run test:e2e`: 28 passed, 0 failed, 0 skipped; 9 of 9 databases removed.
  - Build passes with the existing warnings only. `main` 594.88 kB; complete initial total 607.36 kB; estimated transfer 133.77 kB. The 594.55 kB recorded at `WI-009-008` was the `main` bundle.
- **Deliberate `DEC-079` test supersessions:** `group-landing.spec.ts` (Safety landing cases removed), `e2e/navigation.spec.ts` (Safety steps and the `AC-15` request assertion), `shell.spec.ts` (the Safety half of the landing-link test).
- **No Phase 9.5 capability, and no production API, schema, or authorization change.**
