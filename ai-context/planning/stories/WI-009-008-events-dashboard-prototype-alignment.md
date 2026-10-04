---
Status: DONE / PASS — OWNER APPROVED
Authority: Project-owner authorization (2026-10-03) after the panel ③ design review; owner decisions D1–D11 (DEC-078)
Context-Baseline: CTX-1.28
Version: 1.0
---

# WI-009-008 — Events Dashboard Prototype Alignment

## Metadata

**Repository cleanup note:** Historical owner-review captures originally under `src/FamilySafety.Web/e2e/captures/events-dashboard-review/` were later retained in [artifacts/reviews/WI-009-008/](../../../artifacts/reviews/WI-009-008/) with filenames and bytes unchanged. Canonical verification evidence remains separate.

- **Work Item ID:** `WI-009-008`
- **Status:** `DONE / PASS` — `OWNER APPROVED` (owner review 2026-10-03; `VER-WI-009-008`; `DEC-078`). AC-01 to AC-11 `PASS`.
  - Status history:
    1. `IMPLEMENTED / OWNER VISUAL REVIEW`.
    2. Owner visual approval `PASS`.
    3. `VERIFYING / FINAL REGRESSION`.
    4. `DONE / PASS`.
  - All on 2026-10-03.
- **Phase:** `9 — Integration & UX Refinement`
- **Context baseline:** `CTX-1.28` (no requirement change)
- **Planning basis:** the panel ③ design review (2026-10-03); `WI-009-006` / `DEC-076` and `WI-009-007` / `DEC-077` (dashboards with honest placeholders); `DEC-070` visual direction; `DEC-078`.
- **Visual target:** `ai-context/references/ux/home-dashboard/home-dashboard-overview.png`, **panel ③ "Travel & Events Summary"** only (larger copy: `claude-handoff/reference/panel3-travel-events.png`). UX mockups are non-authoritative design evidence.
- **Requirement references (preserved, not changed):** `REQ-UX-013` (six areas; summaries do not merge screens), `REQ-UX-014` (item URLs), `REQ-PRIV-022` (Trip audience and creator retention), `REQ-PRIV-027` (navigation is not authorization), `REQ-EVT-008` (Event detail editing), `REQ-EVT-010` (RSVP, not implemented).

## Goal

`/events` becomes an Events dashboard in the panel ③ composition. Event creation and the complete Event list move to their own Events child pages. No existing Event function is lost, and authorization and privacy behaviour are unchanged.

## Owner decisions (2026-10-03)

- **D1 — Events dashboard:** `/events` is the dashboard. The six primary destinations are unchanged; no "Travel & Events" destination.
- **D2 — Title:** "Events", with a subtitle naming the selected Family.
- **D3 — Trips card:** read-only, heading "Trips", using Home's current-Family selection (`homeTrips()`). Retained creator Trips and Trips from any other Family never appear. Rows link to `/trips/:tripId`; View all goes to `/trips`. No Trip status or audience state.
- **D4 — Create route:** creation moves to `/events/new` with every existing field, validation, help text, audience filter, per-person No access / Can view / Can edit choice, the state where nobody is selectable, error handling, and the statement that Event sharing does not send an invitation or start location sharing. This revisits the `DEC-070` accepted difference for Events only.
- **D5 — Header action:** one primary "Create event" link to `/events/new`. No create menu covering more than one domain.
- **D6 — Full list:** `/events/all` holds the complete list: every Event the caller can see in the current Family, including past Events, the count, descriptions, "Organized by", inline edit where `canEdit`, and the loading, no-Family, load-error, and empty states. "Back to Events" link.
- **D7 — RSVPs Needed:** a static placeholder. Copy: "Event invitations that need your response will appear here." No count, button, RSVP state, fake item, digit, or request.
- **D8 — Quick Links:** Create an event (`/events/new`), All events (`/events/all`), Trips (`/trips`), Plans (`/plans`), Family members (`/family`), Your profile (`/me`). Real destinations only.
- **D9 — Event row pill:** none.
- **D10 — After create:** navigate to `/events/{eventId}`; the success message shows on the item page.
- **D11 — Plans & Trips:** `/plans-trips` is not redesigned here.

## Owner visual approval and final decision (2026-10-03)

- **Owner visual approval: `PASS`.** Approved: `/events` (dashboard), `/events/new`, `/events/all`, `/events/:eventId`, the Upcoming Events summary, the current-Family Trips summary, the RSVPs Needed placeholder, the six Quick Links, and the 1440 / 1024 / 768 / 390 layouts.
- **Accepted deliberate differences from panel ③** (not to be reopened): date tiles instead of photos; no RSVP pills; no Trip status pills; "Trips" instead of "Active Trips"; the RSVPs placeholder instead of Respond rows; real Quick Links in place of unsupported destinations; no search, bell, avatar menu, or Messages; the six-area navigation; "Create event" instead of a generic "Create".
- **Final decision — item-page terminology:** the Event item page title changes from "Family events" to "Events". It is terminology only; the Event name, details, authorization, editing, routing, and privacy are unchanged. The Events area now reads "Events" on `/events`, `/events/:eventId`, and as the area of `/events/new` ("Create event") and `/events/all` ("All events").

## Scope

- **Routes** (`app.routes.ts`; `events/new` and `events/all` are declared before `events/:eventId`):
  - `/events`: dashboard (view `events`, `aria-current="page"`).
  - `/events/new`: Create event (view `eventCreate`, Events area, `aria-current="true"`).
  - `/events/all`: All events (view `eventsAll`, Events area, `aria-current="true"`).
  - `/events/:eventId`: unchanged item page (Events area, `aria-current="true"`).
- **Dashboard:**
  - **Header:** title "Events", the subtitle "Upcoming events and trips shared with you in {Family}.", and the "Create event" link.
  - **Upcoming Events:** `dashboardEvents()`, the same upcoming filter and order as Home, for up to 5 rows. Each row has a date tile, a linked name, date · time, and location. View all goes to `/events/all`. The empty state is compact and says past Events stay in All events.
  - **Trips:** `homeTrips()` (3 rows, current Family only). The existing `/api/trips` read now also loads on the Events dashboard.
  - **Quick Links:** six semantic links in a tile grid, inside a `nav` labelled "Quick Links".
  - **RSVPs Needed:** a placeholder reusing the Home placeholder note style.
- **Create page:** the existing form, unchanged; Back to Events. On success it navigates to `/events/{id}` with the focus-heading state. The item page shows "Event created and shared with the selected family members." only on that navigation, never on refresh or back/forward. Without a router (unit tests), the previous in-place reset and message remain.
- **Responsive layout:**
  - **≥ 80 rem:** sidebar. A 12-column 2 × 2 grid: Upcoming Events 7 and Trips 5; RSVPs 5 and Quick Links 7. Quick Links in 3 columns.
  - **64–80 rem:** the same 2 × 2 with tighter gaps; Quick Links in 2 columns.
  - **44–64 rem:** top navigation. Upcoming Events full width; Trips and RSVPs side by side; Quick Links full width in 3 columns.
  - **Below 44 rem:** one column in the order Upcoming Events, Trips, Quick Links, RSVPs Needed. The title and the compact "Create event" share the first row, with the subtitle below. Quick Links in 2 columns, title only.
  - Create and All events pages: single cards, maximum width 60 rem.
  - No horizontal overflow.
- **Styles:** new scoped `events-dashboard.scss` (dashboard, header action, Back link, Quick Links). No budget raised.

## Explicit exclusions

- Event RSVP, invitations, Needs, Updates, Check-Ins, notifications, Calendar, Shared Lists, Travel Documents, Trip statuses, location or sharing, and any Phase 9.5 capability.
- Any production API, endpoint, schema, migration, authorization, or server change. The dashboard uses only the existing Event list and `/api/trips` reads.
- Home, Family, `/plans-trips`, Plans, and Trips page changes.
- A create menu covering more than one domain; an Organizer pill.

## Acceptance checks

1. `AC-01` The `/events` 1440 composition follows panel ③ as adapted by D1–D9.
2. `AC-02` Every existing Event function remains reachable: create (all fields, audience filter and access choices, the state where nobody is selectable, errors), the full list including past Events, inline edit, and the item page.
3. `AC-03` `/events/new` and `/events/all` are not treated as item IDs. Navigation shows Events with `aria-current="page"` on `/events` and `"true"` on the child and item pages.
4. `AC-04` The Trips card shows only Trips of the selected Family; a retained creator Trip never appears.
5. `AC-05` RSVPs Needed contains only its heading and the D7 sentence: no button, link, input, image, list, digit, badge, or request.
6. `AC-06` All six Quick Links are semantic, keyboard-reachable links to implemented routes.
7. `AC-07` After a successful create, the browser is at `/events/{id}` with the success message; refresh shows no message.
8. `AC-08` 1440, 1024, 768, and 390 px follow the Scope layout with no horizontal overflow.
9. `AC-09` (after visual approval) Focused Angular specs updated and passing; `npm run build` passes with no new budget warning.
10. `AC-10` (after visual approval) Final regression, including affected Playwright specs.
11. `AC-11` Owner visual approval.

## Implementation record (2026-10-03)

- **Files:**
  - `src/FamilySafety.Web/src/app/app.routes.ts`: the `events/new` and `events/all` routes, before `events/:eventId`.
  - `app/navigation.ts`: the `eventCreate` and `eventsAll` views, the Events area, and their paths.
  - `app/app.ts`:
    - titles and subtitles;
    - `dashboardEvents()`, which shares the Home upcoming selection through `upcomingEvents()`;
    - the Trips read on the Events dashboard;
    - the D8 `eventQuickLinks`;
    - D10 post-create navigation and the one-time success message;
    - `events-dashboard.scss` in `styleUrls`.
  - `app/app.html`:
    - the Events header action;
    - Back to Events on the child pages;
    - the dashboard;
    - the full list and the create form moved unchanged into the `eventsAll` and `eventCreate` pages.
  - `app/events-dashboard.scss`: new.
- **Build:** one `npm run build`. It passes with only the existing warnings: the initial bundle at 594.24 kB (was 583.51 kB), `trips.scss`, and `concerns.scss`. A selector fix after that build moved `events-dashboard.scss` from 4.0 to 4.5 kB of source. A Sass estimate puts it at about 3.6 kB compiled, under the 4 kB warning. The next build in the testing pass confirms it.
- **Not run (by owner direction):** Angular specs, Playwright, and regression. No tests were written or changed. Specs that assert the old Events title ("Family events"), the old subtitle, or the create form on `/events` are expected to need updates.
- **Review renders (transient, not canonical evidence):** `src/FamilySafety.Web/e2e/captures/events-dashboard-review/`:
  - `events-live-*`, `events-new-live-*`, `events-all-live-*`: real Development data (the Development Family has no Events or Trips, so these show empty states).
  - `events-mock-*`, `events-all-mock-*`, `events-post-create-mock-1440`: browser-only injected sample data, labelled on the image; nothing was written to the database. The mock Trip list includes a Trip from another Family, and it did not render.
  - `events-contact-sheet.png`.
  - Checked during capture: zero horizontal overflow on every render. Events shows `aria-current="page"` on `/events` and `"true"` on `/events/new`. The Create event link moves focus to the `h1`. The mocked create lands on `/events/aaaaaaaa-…-008` with the success message.

## Remaining tasks

- [x] `TASK-001` Implement D1–D11.
- [x] `TASK-002` Review renders at 1440, 1024, 768, and 390, plus the contact sheet.
- [x] `TASK-003` Owner visual approval (`AC-11`): `PASS`, 2026-10-03, with the item-page terminology decision applied.
- [x] `TASK-004` Testing pass: update the affected specs; add Events dashboard, child-route, Trips-filter, placeholder, Quick Links, and post-create coverage; run the focused specs and build.
- [x] `TASK-005` Final regression (`AC-10`), evidence, `VER-WI-009-008`, and closure.

## Owner closure (2026-10-03)

- **Decision:** `DONE / PASS` — `OWNER APPROVED` (`DEC-078`). Evidence is in `evidence/WI-009-008/`.
- **Verification:**
  - Angular: 228/228, including the new `events-dashboard.spec.ts` (11 tests).
  - Focused Playwright: the new `e2e/events-dashboard.spec.ts` 2/2, with the updated `item-links` and `navigation` specs passing.
  - One full `npm run test:e2e`: 24 passed, 0 failed, 0 skipped; 8 of 8 databases removed.
  - Build passes with the existing warnings only; initial bundle 594.55 kB; `events-dashboard.scss` compiles to 3.36 kB (under 4 kB).
  - See `VER-WI-009-008`.
- **No Phase 9.5 capability, and no production API, schema, or authorization change.**
