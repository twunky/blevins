---
Status: DONE / PASS — OWNER APPROVED
Authority: Project-owner direction during the WI-009-005 owner visual review (2026-10-03), owner decisions OD-1 to OD-6, and owner visual approval recorded as DEC-076 (2026-10-03)
Context-Baseline: CTX-1.28
Version: 1.0
---

# WI-009-006 — Home Dashboard Prototype Alignment

## Metadata

- **Work Item ID:** `WI-009-006`
- **Status:** `DONE / PASS` — `OWNER APPROVED` (owner review 2026-10-03; `VER-WI-009-006`). AC-01 to AC-10 `PASS`.
  - Status history:
    1. `IMPLEMENTED / OWNER VISUAL REVIEW`.
    2. Owner visual approval `PASS` (`DEC-076`).
    3. `VERIFYING / FINAL REGRESSION`.
    4. `DONE / PASS`.
  - All on 2026-10-03.
- **Phase:** `9 — Integration & UX Refinement`, `CURRENT / IN PROGRESS` (`DEC-073`)
- **Context baseline:** `CTX-1.28` (no requirement change)
- **Planning basis:** `PRP-030` theme "Home as the application hub"; `DEC-070` visual direction (blue palette, sidebar shell, white cards).
- **Visual target:** `ai-context/references/ux/home-dashboard/home-dashboard-overview.png`, **panel ① "Main Home Dashboard"** only (larger copy: `claude-handoff/reference/panel1-main-home.png`). Panels ②–④ are out of scope. UX mockups are non-authoritative design evidence (`ai-context/README.md`).
- **Requirement references:** none changed. `REQ-UX-011` (current-Family context), `REQ-UX-013` (six-area navigation), `REQ-UX-014` (item links), and `REQ-PRIV-027` (navigation is not authorization) are preserved.

## Origin — formalized, not recreated

This redesign was initiated by the project owner during the owner visual review of `WI-009-005 — Refresh-Safe Item URLs and Authorize-First Deep Links` (`VERIFYING / OWNER REVIEW`). The owner judged the Home page too sparse compared with the approved prototype and directed a prototype-aligned Home.

The work **materially exceeds** `WI-009-005`'s Home scope, which was only the Home row links (`AC-16`). It is therefore recorded here as its own Phase 9 UI item. The implementation already in the working tree is **adopted as this item's implementation**; it is not to be redone. `WI-009-005` remains `VERIFYING / OWNER REVIEW` and claims none of this work.

It also reopens, by new owner request, these `DEC-070` accepted differences, **as placeholders only**: "no activity feed, Tasks, or Active Sharing". It does the same for the matching `MOCKUP-ALIGNMENT.md` DROP entries. No capability is added.

## Goal

Home reads as a family coordination dashboard that matches the panel ① composition. The information architecture stays visible with little data. Real cards show only real data. Planned capabilities have honest places reserved for them.

## Scope

- **Prototype-aligned Home grid (desktop ≥ 80 rem, 12 columns):**
  - Row 1: Needs Your Attention (7) | Upcoming Events (5).
  - Row 2: Trips | Recent Family Updates | My Tasks (4 each).
  - Row 3: Plans you can see (7) | Safety coordination (5). This is the intentional adaptation for implemented features that the prototype lacks.
  - Row 4: Active Sharing, a full-width slim bar.
- **Persistent cards with compact empty states:**
  - Attention, Events, Trips, and Plans always render.
  - Loading and error states are shown inside each card.
  - Each empty card has one honest line and a real area button (Open Events / Open Trips / Open Plans).
  - The combined low-data strip is removed.
- **Future-feature placeholders:**
  - Recent Family Updates, My Tasks, and Active Sharing appear as muted cards, each with one sentence saying the feature will appear later.
  - There is no badge, no data, no request, and no action.
- **Compact Safety treatment:** the "Safety coordination" heading, text, and "Open Concerns" / "Open Incidents" buttons are unchanged in behavior, shown in a smaller card with a soft body panel.
- **Compact Family access:**
  - The large Family row is removed from the Attention card.
  - A "View Family" button sits in the page header beside the date.
  - The Family name stays in the subtitle.
- **Personalized greeting:**
  - "Good morning / afternoon / evening, {displayName}!" uses local time and the already-loaded own profile (`/api/profile`, loaded at startup); no additional request.
  - Without a profile it falls back to "Good morning!" (etc.).
  - Subtitle: "Here's what's happening with {Family} today."
- **Event rows** add the existing `location` field.
- **Responsive layout:**
  - 44–80 rem: two columns (Attention | Events; Trips | Plans; Safety bar; Updates | Tasks; Sharing bar).
  - Below 44 rem: one column in reading-priority order, with placeholders last.
  - No horizontal overflow.
- **Owner visual approval** at 1440, 1024, 768, and 390 px.

## Explicit exclusions

- activity-feed or updates backend;
- tasks functionality (no task API, state, checkboxes, or assignment);
- Sharing Sessions, Active Sharing data, avatars, or location state;
- Event RSVP and Check-Ins;
- notifications;
- `OD-002` (not resolved or implied);
- any Phase 9.5 feature implementation;
- any API, schema, migration, authorization, or route change;
- any change to Trip, Plan, or Event filtering or ordering (the Home queries are unchanged);
- shell or navigation changes.

## Owner decisions (2026-10-03)

- **OD-1 — Recording:** this is its own work item. It does not count as `WI-009-005` work. `WI-009-005` stays `VERIFYING / OWNER REVIEW`.
- **OD-2 — Trips heading:** "Trips". Not "Current Trips", because the Home query still includes ended Trips. Trip filtering is not changed.
- **OD-3 — Plans placement:** Plans stays in row 3 with Safety. My Tasks keeps its row 2 place.
- **OD-4 — Greeting:** prototype-style time-of-day greeting with the signed-in display name, from existing client state only. The Family stays in the subtitle. The date and View Family stay near the header.
- **OD-5 — Placeholder labels:** no visible "Coming later" badge. The copy itself says the feature is not yet available, with no phase, roadmap, or engineering terms.
- **OD-6 — Attention whitespace:** the outer card may match the Events card height. The inner white list sizes to its rows and stays at the top, so spare height shows the pink card background.

## Acceptance checks

1. `AC-01` Desktop composition matches the Scope grid at 1440 px.
2. `AC-02` Attention, Events, Trips, Plans, and Safety always render once a Family is selected. Each empty state is one line plus a real area button. The low-data strip is gone.
3. `AC-03` The placeholders show the approved sentences only. They contain no button, link, input, image, list item, avatar, digit, or badge, and Home makes no request beyond the existing profile, Family, Events, Plans, relationships, and Trips reads.
4. `AC-04` Item links from `WI-009-005` are unchanged (`/events/:id`, `/plans/:id`, `/trips/:id`; names and `href`s).
5. `AC-05` Greeting and subtitle follow OD-4, with no new request.
6. `AC-06` One attention item: the white list is no taller than its row. Zero items: one compact "Nothing needs your attention right now." row.
7. `AC-07` 1024, 768, and 390 px follow the Scope responsive layout, with no horizontal overflow and View Family usable.
8. `AC-08` Focused Angular specs pass; `npm run build` passes with no new budget warning.
9. `AC-09` Final regression (later, after visual approval): one full `npm run test:e2e`, including the updated `navigation.spec.ts` and `invitation-handoff.spec.ts`.
10. `AC-10` Owner visual approval.

## Implementation record (2026-10-03)

- **Files:**
  - `src/FamilySafety.Web/src/app/app.html`: Home block only.
  - `app/home.scss`: new; the Home styles, moved out of `app.scss`.
  - `app/app.scss`: Home rules removed.
  - `app/app.ts`:
    - `home.scss` added to `styleUrls`;
    - `homeGreeting`;
    - Home title and subtitle;
    - unused `homeHasVisibleItems()` removed.
  - `app/icon.ts`: four decorative icons (`check`, `message`, `tasks`, `share`).
- **Tests:**
  - `app/home.spec.ts`: low-data test rewritten; placeholder test added; button order updated.
  - `app/shell.spec.ts` and `app/first-use.spec.ts`: the Family-name check moves from "Welcome to {Family}" to the subtitle.
  - `e2e/navigation.spec.ts` and `e2e/invitation-handoff.spec.ts`: Home `h1` expectation moves to the greeting pattern plus the subtitle. These were edited but **not yet run**; they wait for `AC-09`.
- **Focused results:** `home`, `shell`, `first-use`, `app` specs: 62/62 pass. `npm run build` passes. Initial bundle 563.83 kB (WI-009-005) → 569.44 kB. Only the existing warnings remain (initial bundle, `trips.scss`, `concerns.scss`); `home.scss` is under 4 kB.
- **Design-review renders:** `src/FamilySafety.Web/e2e/captures/home-dashboard-review/` (transient, ignored by source control):
  - `home-live-*` (real Development data);
  - `home-populated-mock-*` (browser-mocked sample data, review only);
  - `home-contact-sheet.png`.

  These are not canonical evidence. Approved captures are copied into `evidence/WI-009-006/` at closeout.
- **Not changed:** API, schema, authorization, routes, navigation, Home data queries, and the Safety or Family behavior.

## Effect on WI-009-005 (to resolve at its closure)

- `AC-16`, second bullet ("the Safety card, 'View all', 'Needs your attention', and the low-data card are unchanged: the same text and button count") is **superseded** by this item: the low-data card is removed, the attention heading and text changed, and the button order changed. The first bullet (row links) still holds and is still tested.
- Several `VER-WI-009-005` statements are now stale:
  - "Home buttons are unchanged";
  - "No new section";
  - "`navigation.spec.ts` … passes unchanged".
- Evidence to recapture after this item's visual approval: `evidence/WI-009-005/home-row-links-1440.png` and `home-row-links-390.png` (Home layout changed; the Trips heading changed from "Trips in {Family}" to "Trips"). No other WI-009-005 capture shows Home.
- Specs to re-run then: `e2e/item-links.spec.ts` (its Home locators use section IDs, which are kept), `navigation.spec.ts`, `invitation-handoff.spec.ts`, `concern-workflow.spec.ts`, and `incident-workflow.spec.ts`. The last two rely on the "Safety coordination" heading and the "Open Incidents" button, which are kept.

## Remaining tasks

- [x] `TASK-001` Implement the Home redesign (adopted from the owner-review session).
- [x] `TASK-002` Apply OD-2 to OD-6; focused tests; build; review renders.
- [x] `TASK-003` Owner visual approval (`AC-10`): `PASS`, 2026-10-03.
- [x] `TASK-004` Record the approval: `DEC-076`, plus the status and handoff updates.
- [x] `TASK-005` Final regression (`AC-09`) and the `WI-009-005` recapture. Approved captures copied into `evidence/WI-009-006/`. `VER-WI-009-006` written.

## Owner closure (2026-10-03)

- **Decision:** `DONE / PASS` — `OWNER APPROVED`. Owner review date: 2026-10-03.
- **Visual approval (`AC-10`):** `PASS`, for the design in `evidence/WI-009-006/home-contact-sheet.png` (`DEC-076`).
- **Final regression (`AC-09`):** Angular 206/206; focused Playwright 19/19; one full `npm run test:e2e` with 20 passed, 0 failed, and 0 skipped; build passes with existing warnings only. See `VER-WI-009-006`.
- **Effect on `WI-009-005`:** the `AC-16` Home-presentation clause is superseded and noted in that story and its VER. The two Home link captures were recaptured under new names. `WI-009-005` closed `DONE / PASS` the same day.
- **No Phase 9.5 capability and no production API, schema, or authorization change.**
