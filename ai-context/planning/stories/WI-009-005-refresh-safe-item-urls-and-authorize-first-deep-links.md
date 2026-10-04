---
Status: DONE / PASS — OWNER APPROVED
Authority: Project-owner approved and execution-authorized for WI-009-005 only (2026-10-02), against CTX-1.28, with owner decisions OD-A to OD-I (DEC-075); planning basis PRP-030 M3 (client part) and M4 (Home rows open the item) / DEC-074 D4
Context-Baseline: CTX-1.28
Version: 1.0
---

# WI-009-005 — Refresh-Safe Item URLs and Authorize-First Deep Links

## Metadata

- **Work Item ID:** `WI-009-005`
- **Status:** `DONE / PASS` — `OWNER APPROVED` (owner review 2026-10-03; `VER-WI-009-005`). AC-01 to AC-42 `PASS`. `AC-16` carries the `DEC-076` supersession note for its obsolete Home-presentation wording; see [Owner closure](#owner-closure-2026-10-03). Implemented and self-verified 2026-10-03. Approved and execution-authorized by the project owner on 2026-10-02 against `CTX-1.28`.
  - Prepared `PROPOSED` on 2026-10-02.
  - The owner resolved `OD-A` to `OD-I` the same day (`DEC-075`); see [Owner decisions](#owner-decisions-2026-10-02).
  - `OD-A` and `OD-B` amended `REQ-UX-014`, which is canonical in `CTX-1.28` (`VER-CTX-1.28` `PASS`).
  - No further routine approval is needed. The stop conditions still apply.
- **Phase:** `9 — Integration & UX Refinement`, `CURRENT / IN PROGRESS` (`DEC-073`)
- **Context baseline:** `CTX-1.28`
- **Approved planning basis:** `PRP-030` (`APPROVED`, `DEC-074`):
  - **M3, client part:** refresh-safe item URLs and the authorized deep-link Family switch. The server part was `WI-009-004`.
  - **M4, link part only:** Home rows open the specific item. Home filtering, the Safety summary, and dead-end cleanup are not included (M5–M7).
  - Theme 4.
- **Requirement references:**
  - `REQ-UX-014` (as amended in `CTX-1.28`): implemented here on the client, steps 1–3.
  - `REQ-PRIV-027`: preserved and verified. The URL, the navigation area, the selected Family, and the client's Family list are never authorization.
  - `REQ-UX-011`: the authorized item link is the only other way the selected Family may change. On a change, Family-scoped state is cleared and reloaded.
  - `REQ-UX-013`: six-area navigation, preserved. Item routes map to their area.
  - `REQ-PRIV-022`: Trip creator retention, cited by OD-A and unchanged.
  - `DEC-074` D4 and `DEC-075`.
  - Preserved unchanged: Event access (`REQ-PRIV-020`, `REQ-PRIV-021`, `REQ-PRIV-023`); Plan access (`REQ-PLAN-005`, `REQ-PLAN-006`); Trip access (`REQ-PRIV-022`); Concern access (`REQ-SAFE-015`, `REQ-PRIV-025`); Incident access (`REQ-SAFE-016`, `REQ-PRIV-026`).
- **Model tier:** `DEEP`. Authorization-adjacent routing that changes when the selected Family changes, across private items in several Families.
- **Phase 9 exit criteria served:**
  - 4 (item URLs), in full;
  - 5 (authorized cross-Family deep link), in full;
  - 6 (generic unavailable, client and live API, all five types), in full;
  - 9, only the "rows open the specific item" part;
  - 11, every item detail has a way back;
  - 13 and 14, for the changed screens.
- **Standing auto-approval:** `DEC-032` does **not** apply (see [DEC-032 review](#dec-032-review)). Explicit owner approval and execution authorization were given on 2026-10-02.

## Goal

Every Event, Plan, Trip, Concern, and Safety Incident that the application lists or shows on Home opens through its own URL. The URL works on direct entry, after a browser refresh, through back/forward, and without any prior navigation state.

Opening an item URL always follows `DEC-074` D4 as amended by `DEC-075`:

```text
ROUTE (item ID only)
  → ITEM API READ  (GET /api/{events|plans|trips|concerns|incidents}/{id})
  → SERVER AUTHORIZATION (the item's own rules)
  → 200 AUTHORIZED RESPONSE (includes FamilyGroupId)
      → FAMILY CONTEXT DECISION (caller's own Family list; one refresh if absent)
          → in list, same Family           : no change, no announcement
          → in list, different/none        : switch, clear + reload, announce "Switched to {name}"
          → not in list, Trip (REQ-PRIV-022): no switch, no announcement (OD-A)
          → not in list, any other type     : generic unavailable, Family unchanged (OD-B)
      → RENDER
  → 404 / invalid ID
      → generic unavailable for that type; Family unchanged; no announcement; nothing disclosed
```

Never `ROUTE → SWITCH FAMILY → TRY AUTHORIZATION`. No server API is changed and no authorization path is added.

## Owner decisions (2026-10-02)

These decisions are authoritative (`DEC-075`). They override any looser wording elsewhere in this item.

### OD-A — Authorized Trip with no selectable Family: approved, Trip only

- The Trip creator keeps access after leaving the Trip's Family (`REQ-PRIV-022`). An authorized Trip may therefore reference a Family that is not in the caller's active Family list.
- The Trip item read still happens first, and server authorization stays authoritative.
- The application does **not** switch the selected Family and does **not** create a selectable Family. It renders the authorized Trip with the existing Trip behavior, and the current selection, if any, is preserved. There is no announcement.
- **Only** where existing Trip authorization already allows the item after membership ends. There is **no** equivalent exception for Events, Plans, Concerns, or Incidents.
- Trip access is not broadened. Canonical in `REQ-UX-014` step 2 (`CTX-1.28`).

### OD-B — Stale or missing Family context: approved

After the server authorizes an item:

1. wait for the caller's current Family list;
2. look for the authorized `FamilyGroupId`;
3. if it is absent, refresh `/api/family-groups/mine` **once**;
4. check again.

If the Family is now present, switch normally if required. If it is still absent:

- **Trip:** apply OD-A only if the existing Trip authorization legitimately allows reading without active membership. That is shown by the server's `200` for a Trip whose Family is not in the list. The client makes no access judgment of its own.
- **Event, Plan, Concern, Incident:** do not switch, do not render the item under an unrelated Family, show the normal generic unavailable state, and leave the selected Family unchanged.

The client never retries repeatedly, and never treats its Family list as authorization. If the Family-list request itself fails, it shows the existing membership error and **Retry**, with no render and no switch. That is an infrastructure error, not an access decision.

### OD-C — Event and Plan detail: approved, inside `App`

- Minimal routed Event and Plan detail views inside the current `App` structure. No broad `app.ts` / `app.html` extraction. Existing presentation and behavior are reused (shared `<ng-template>`).
- The Event detail exposes only the caller's existing Event actions, including the existing **Edit event details** when `canEdit` is true. The Plan detail is read-only.
- **If the Event presentation cannot be reused safely without a small extraction, stop and report the exact proposed Event-card extraction.** Do not turn it into a broad root-component refactor.

### OD-D — Routes: approved

- `/events/:eventId`, `/plans/:planId`, `/trips/:tripId`, `/concerns/:concernId`. The existing `/incidents/:incidentId` is kept.
- GUID handling is repository-consistent: the same GUID pattern as `readOrigin`, moved to a shared helper.
- Existing list and grouping routes are unchanged.

### OD-E — Second-Family E2E fixture: approved, test support only

- A Development / E2E fixture capability seeds a second Family, so cross-Family deep links are proven end to end.
- It is opt-in through the existing isolated Playwright scenario mechanism, and enabled only for specs that request it. It is not seeded for ordinary Development use, and never active in production.
- No production API, endpoint, domain, schema, or production-registration change.
- Deterministic IDs and state, reusable by the later Phase 9 multi-Family journey (M9).
- No user-facing "create a second Family" workflow.

### OD-F — Cross-Family Concern and Incident open: approved

- Opening an authorized Concern or Incident whose Family differs from the selected Family:
  1. authorize the item on the server;
  2. take `FamilyGroupId` from the authorized response;
  3. locate that Family in the caller's Family list;
  4. switch;
  5. reload Family-scoped context;
  6. render.
- Never switch from the route, list metadata alone, navigation state, or a guess. The same applies to Event, Plan, and active-Family Trip deep links.

### OD-G — Concern creation destination: approved

- After a Concern is **successfully created**, navigate to `/concerns/{concernId}`.
- Current creation behavior, permissions, data, and lifecycle are preserved. Nothing additional is copied or exposed.
- **Implementation constraint, from code review.** Creation is a two-step flow in `Concerns`: create, then an optional initial entry. It has in-component recovery states: the initial-entry failure panel with "Continue to Concern" and **Retry**, and the ambiguous-create check. These must not be lost. Navigate when the created Concern is confirmed and no recovery panel is pending. That is one of:
  - directly after a fully successful create;
  - after a successful retry of the initial entry;
  - when the user chooses **Continue to Concern**;
  - after an ambiguous create resolves to a match.

  A shared `concerns` route matcher (the `incidentsMatcher` precedent), so the shell is not re-created, is an acceptable alternative. If neither works without changing creation behavior, stop and report.

### OD-H — Family-switch announcement: approved

- The polite status message "Switched to {Family name}." appears only when the application actually performs an automatic Family switch. The name comes from the caller's own Family list.
- No announcement on same-Family opens, on unavailable items, on OD-A Trips, or before authorization succeeds.
- Use an always-present polite live region. No visual notification infrastructure.

### OD-I — Plan create `Location` header: out of scope

- Not changed in this item. It stays a recorded, unscheduled server/API cleanup follow-up (`WI-009-004` D9).

## Current implementation (verified 2026-10-02, static review)

### Item routes and reads

| Type | Item read (server) | Response Family fields | Item URL today | How the detail opens today |
|---|---|---|---|---|
| Event | `GET /api/events/{eventId}` | `familyGroupId` | None | No detail. List card in `app.html` with inline **Edit event details** when `canEdit` |
| Plan | `GET /api/plans/{planId}` | `familyGroupId` | None | No detail. List card; Plans have no actions |
| Trip | `GET /api/trips/{tripId}` | `familyGroupId`, `familyGroupName` | None | `Trips.openDetail(id)` sets in-component state; a refresh loses it; an error shows an alert and reloads the list |
| Concern | `GET /api/concerns/{concernId}` (denied-read audit) | `familyGroupId`, `familyGroupName` | None | List rows call `openConcern(id)`; the Incident lineage link uses navigation state `openConcernId` |
| Incident | `GET /api/incidents/{incidentId}` (denied-read audit) | `familyGroupId`, `familyGroupName` | `/incidents/:incidentId` | `Incidents` reads the URL on `NavigationEnd`. **No Family switch.** Hidden behind "Choose your current Family above" when a member of several Families refreshes with no selection, because `.page-content` is `[hidden]` without a selected Family |

### Selected-Family mechanism

- **`CurrentFamilyContext`:** an in-memory root signal; session-only and lost on refresh.
- **`App.loadMemberships()`:** runs on every `App` creation (every area route). It reads `/api/family-groups/mine`, auto-selects a sole Family, keeps a prior choice if it is still listed, and otherwise selects none. Then it clears and reloads Family-scoped state.
- **`App.switchCurrentFamily()`:** validates against the list, clears and reloads. It redirects to `/home` when moving from no selection, so **it is not reused as-is for item links**.
- **`app-trips`:** re-keyed on `currentFamilyId()`, and not rendered when no Family is selected, except for zero-Family users.

### Lists and Home

| Surface | Today |
|---|---|
| Events and Plans lists | Card headings, no links |
| Trips list and retained-creator list | `button.trip-list-button` |
| Concerns list | `button.concern-card` with `aria-label="Open Concern about …, {state}"`; covers all the caller's Families |
| Incidents list | `button.incident-row` with `aria-label="Open Incident about …"`; covers all the caller's Families |
| Home Events, Trips, and Plans cards | Rows with plain text that open nothing |
| Home "Safety coordination" | Two static buttons; **no item rows**; out of scope |

### Fixtures and budgets

- **Fixtures:** the Playwright scenario seeds one Family. Adult A is its administrator, B has no membership, and C (opt-in) is a member. Family creation is operator-only (`REQ-FAM-006`), hence OD-E.
- **Budgets:** initial bundle 549.21 kB; `concerns.scss` about 7.5 kB compiled against the 8 kB error budget; `trips.scss` about 6.3 kB (warning).

## Design

### 1. Routes

| URL | Renders | Area / `aria-current` |
|---|---|---|
| `/events/:eventId` | Event detail (new, minimal, in `App`) | Events, `"true"` |
| `/plans/:planId` | Plan detail (new, minimal, in `App`) | Plans & Trips, `"true"` |
| `/trips/:tripId` | Existing Trip detail in `Trips` | Plans & Trips, `"true"` |
| `/concerns/:concernId` | Existing Concern detail in `Concerns` | Safety, `"true"` |
| `/incidents/:incidentId` | Existing Incident detail (route unchanged) | Safety, `"true"` |

- **Separate path routes** for the four new URLs, following the majority pattern. List ↔ item navigation re-creates `App`, as area changes already do, so `WI-009-003` D4 focus works unchanged. For Concerns, the OD-G matcher alternative is allowed.
- **Invalid IDs:** a non-GUID ID shows the generic unavailable state for that type **without** an API call.
- **Navigation mapping:** add item views to `navigation.ts` (illustrative names `eventItem`, `planItem`, `tripItem`, `concernItem`) in the typed `areaForView` table.
- **Page `h1`:** stays the area title ("Family events", "Plans", "Trips", "Concerns", "Safety Incidents"). The item name is the card's `h2`, shown only after authorization.

### 2. Authorize-first Family decision (one method in `App`)

```ts
// App, illustrative only
/** REQ-UX-014 step 2. Call only with the FamilyGroupId of an item the server has just returned (200). */
protected adoptAuthorizedItemFamily(familyGroupId: string, kind: ItemKind): Promise<ItemFamilyOutcome>
// 'current'     : already selected; nothing changes; no announcement
// 'switched'    : selection changed; Family-scoped state cleared and reloaded; announcement
// 'retained'    : Trip only, Family still not in the list after one refresh (OD-A); no switch, no announcement
// 'unavailable' : non-Trip, Family still not in the list after one refresh (OD-B); show the generic unavailable state
```

- It waits for the Family list, matches only against `activeFamilyGroups()`, refreshes the list at most once (OD-B), and switches through the existing `clearFamilyScopedState()` / `loadSelectedFamily()` path **without** the `/home` redirect.
- If the list request fails, the existing membership error and **Retry** appear, with no render.
- It is passed as an `@Input()` function to `Trips`, `Concerns`, and `Incidents` → `IncidentDetail`. `App` calls it directly for Events and Plans.
- **Rendering order:** a host sets its visible item only after the promise resolves to `'current'`, `'switched'`, or `'retained'`. No Family-scoped request for the target Family is sent before the item `200`.
- **One read per open:** each component keeps its own item read. This preserves the Concern and Incident denied-read audit counts.
- **Concurrency:** a membership change after the item read is not re-checked on the client. The server re-checks every later read and write, and existing error states apply.

### 3. Generic unavailable state

| Type | Heading (`h2`, focused) | Text | Way back |
|---|---|---|---|
| Event | This Event is unavailable | Only Events currently shared with you can be opened. | Return to Events |
| Plan | This Plan is unavailable | Only Plans currently shared with you can be opened. | Return to Plans |
| Trip | This Trip is unavailable | Only Trips currently shared with you can be opened. | Return to Trips |
| Concern | This Concern is unavailable | Only Concerns currently shared with you can be opened. | Return to Concerns (existing card) |
| Incident | This Incident is unavailable | Only Incidents currently shared with you can be opened. | Return to Incidents (existing card) |

- The same card for a missing item, an unauthorized item, an invalid ID, an ended membership, and the OD-B "still absent" case. Raw API distinctions are not exposed.
- Event, Plan, and Trip use a new presentational `item-unavailable.ts`. Concern and Incident keep their existing cards.
- The card renders whether or not a Family is selected. On item routes the shell does not show the "Choose your current Family above" prompt in place of the item or the card.
- The Family selection, the top bar, and the selector are unchanged. There is no announcement.
- `401` and other errors keep each type's existing messages. They are not the unavailable card, and they never switch the Family.

### 4. Event detail (`/events/:eventId`)

- The existing Event card content (name, date and time, location, description, organizer) and a "Back to Events" link.
- **Edit event details** only when `canEdit`, using the existing edit form, methods, and `PUT`. The detail shows the updated Event after a save.
- The card body moves into an `<ng-template>` shared by the list and the detail, with no duplication. The list keeps its inline edit.
- No new action, no audience list, and no RSVP, Needs, or Updates.

### 5. Plan detail (`/plans/:planId`)

- The existing Plan card content (activity, date and time, expected return and check-in, Notes, creator) and a "Back to Plans" link.
- Read-only, through a shared `<ng-template>`.

### 6. Trip (`/trips/:tripId`)

- `Trips` gains `@Input() tripId`. The existing detail opens via read → adopt → `selectedTrip.set`. On `404` or `'unavailable'`, the item-unavailable card appears instead of today's alert plus list. On `'retained'` (OD-A), the existing detail renders, with no switch.
- The list and retained-creator rows become `routerLink` anchors. **Back to Trips** becomes a link.
- Create, edit, audience change, Notes, and retained handling are unchanged. A Trip still shows in place after create or edit.
- On the item route, `app-trips` renders while no Family is selected. A second Trip read after re-keying on a switch is acceptable (no Trip audit). Removing the re-key on the item view is also acceptable. Record which was chosen.

### 7. Concern (`/concerns/:concernId`)

- `Concerns` gains `@Input() concernId`. `openConcern(id)` does read → adopt → `selected.set`.
- The `openConcernId` navigation-state path is **removed**.
- List rows become `routerLink` anchors, keeping the existing `aria-label` and classes. **Back to Concerns** becomes a link to `/concerns`.
- After a successful create, navigate to `/concerns/{id}` (OD-G, including its constraint).
- **Start an Incident…** is unchanged. It carries only `{ originatingConcernId, familyGroupId, subjectProfileId }`.
- Contributions, participants, resolution, history, the write-time unavailable handling, and all permissions and lifecycle are unchanged.
- **Styles:** the `button` → `a` reset goes in a token-only `.item-link` utility in `styles.scss`. `concerns.scss` must not grow. If Concern style additions would cross the budget, stop and propose the smallest safe stylesheet split.

### 8. Incident (`/incidents/:incidentId`)

- The route and `Incidents` URL handling are unchanged.
- `IncidentDetail` calls `adoptAuthorizedItemFamily(…, 'incident')` after its initial read succeeds and before it renders.
- `IncidentList` rows become `routerLink` anchors, keeping the existing `aria-label` and classes.
- The lineage link **Open the Concern** becomes a link to `/concerns/{id}`. `Incidents.openConcern` and its state are removed.
- **Refresh gap fix:** on `/incidents/:incidentId` the item host renders while no Family is selected, so the item read authorizes first and the Family decision follows. Incident access is unchanged.
- Create (`/incidents/new`, Concern origin), coordination, people, tasks, timeline, resolution, the existing unavailable card, and the `initial` path are unchanged.

### 9. List and Home links

| Surface | Link | Accessible name |
|---|---|---|
| Events list | `/events/{id}` on the card heading | Event name |
| Plans list | `/plans/{id}` on the card heading | Activity or destination |
| Trips list and retained-creator list | `/trips/{id}` (row) | Existing row text |
| Concerns list | `/concerns/{id}` (row) | Existing `aria-label` |
| Incidents list | `/incidents/{id}` (row) | Existing `aria-label` |
| Home "Upcoming Events" rows | `/events/{id}` | Event name |
| Home "Trips in {Family}" rows | `/trips/{id}` | Trip title |
| Home "Plans you can see" rows | `/plans/{id}` | Activity or destination |

- All are semantic `routerLink` anchors carrying `focusHeadingState` (`WI-009-003` D4 unchanged).
- Existing actions such as **Edit event details** stay separate from the detail link.
- The Home Safety card, "View all", "Needs your attention", and the low-data card are unchanged. No new summaries or Safety feeds.

### 10. Root component

- No extraction (OD-C).
- The expected `App` growth is about 80–120 lines: route parameters, two item reads, the adopt method, the status region, and host conditions.
- New files are limited to `item-route.ts` (`ItemKind`, `ItemFamilyOutcome`, the GUID check) and `item-unavailable.ts`.
- If the Event card cannot be reused without duplication, stop and report the exact Event-card extraction (OD-C).

### 11. Second-Family fixture (OD-E)

- **`scripts/start-invitation-scenario.ps1`:** a new switch (illustrative `-IncludeSecondFamily`) sets a Development fixture setting (illustrative `DevelopmentFixtures__SecondFamilyScenario`). It is off by default.
- **`src/FamilySafety.Api/Development/DevelopmentFixtureRegistration.cs`:** when the setting is on, and only in Development (guarded like the existing Trip-audience scenario), seed "Development Second Family" with fixed GUIDs.
  - Adult A is an active member, so A belongs to two Families.
  - At least one other seeded adult is an active member, so Events and selected-audience items can be created there.
  - At least one seeded adult is **not** a member, for the other-Family unauthorized case.
  - Administrator assignment is set only as existing domain rules require. Membership is created through the existing fixture helper.
- **`e2e/fixtures/isolated-scenario.ts`:** a `secondFamily` scenario option, passed to the script only for specs that request it.
- Items in the second Family are created per test through the existing APIs, as the Development identity. The fixture seeds no items.
- **Not allowed:** a new endpoint, a production registration change, a schema or migration change, a domain change, or any effect when the option is off. Existing specs must run exactly as before.

### 12. Accessibility

- Semantic `a[href]` links with the existing 3 px `:focus-visible` outline. The link text or the existing `aria-label` names the item.
- `WI-009-003` D4 heading focus is unchanged. Nothing moves on refresh, direct entry, back/forward, or internal state changes.
- The unavailable heading is focused (`tabindex="-1"`). There is one card, with no extra alert.
- "Switched to {name}." goes in an always-present polite live region (OD-H).
- Each item page has one `h1`, and the item `h2` is in a headed card. No `@axe-core/playwright` (M10).

## Scope

### In scope

- The four item routes, the item views in `navigation.ts`, and route-parameter handling in `App`.
- `adoptAuthorizedItemFamily`, the status region, and the item-route host conditions.
- Event and Plan detail views (shared templates; the existing Event edit).
- `item-route.ts` and `item-unavailable.ts`.
- `Trips`, `Concerns`, and `Incidents` changes as designed, including the Concern create navigation (OD-G) and the lineage link.
- List and Home row links, and the `.item-link` utility in `styles.scss`.
- The OD-E second-Family fixture (script, Development fixture, isolated-scenario option).
- Component tests, the new Playwright spec, locator-only updates to existing specs, rendered evidence, `VER-WI-009-005`, and the closure context sync.

### Explicitly excluded

- Any API endpoint, contract, response field, status code, authorization rule, audit behavior, schema, migration, or production registration change. This includes the Plan `Location` header (OD-I).
- New item response fields, including a Family name on Event or Plan.
- Any change to Event, Plan, Trip, Concern, or Incident permissions, audience, participants, lifecycle, or create/edit semantics. No Plan edit, no Event delete, and no new detail actions.
- Home past-item filtering (M6), the Safety summary (M5), removal of "View all" or "Needs your attention" (M4 cleanup or M7), and first-run changes (M7).
- Extracting Events, Plans, Home, or Family from `App`, beyond the OD-C stop-and-report path.
- Splitting `concerns.scss` (S5), unless the budget stop condition is reported and approved. The M8 terminology pass. `@axe-core/playwright` (M10).
- Navigation changes, `OD-001`, `OD-002`, and every Phase 9.5 capability.
- Persisting the selected Family across refresh; changes to `REQ-UX-011` session-only selection.
- A user-facing Family-creation workflow.
- Raising any budget threshold, and any new dependency.

## Acceptance checks

**Direct entry and refresh** (each without prior navigation state; Playwright unless noted)

1. `AC-01` — **Event direct URL.** Typing `/events/{id}` in a fresh tab renders the authorized Event detail. It has exactly the list card's fields, and **Edit event details** only when `canEdit`. Editing from the detail uses the existing form and `PUT`, and shows the updated Event.
2. `AC-02` — **Event refresh.** Refreshing on `/events/{id}` renders the same Event.
3. `AC-03` — **Plan direct URL.** `/plans/{id}` renders the authorized Plan detail: the Plan card fields, read-only, with no actions.
4. `AC-04` — **Plan refresh.** Refreshing on `/plans/{id}` renders the same Plan.
5. `AC-05` — **Trip direct URL.** `/trips/{id}` renders the existing Trip detail, with audience, Notes, and edit behavior unchanged. **Back to Trips** is a link to `/trips`.
6. `AC-06` — **Trip refresh.** Refreshing on `/trips/{id}` renders the same Trip.
7. `AC-07` — **Concern direct URL.** `/concerns/{id}` renders the existing Concern detail, with history, participants, contributions, and resolution unchanged. No code path opens a Concern through navigation state (code review).
8. `AC-08` — **Concern refresh.** Refreshing on `/concerns/{id}` renders the same Concern.
9. `AC-09` — **Incident direct URL.** `/incidents/{id}` renders the existing Incident detail.
10. `AC-10` — **Incident refresh without a preselected Family.** A member of two Families (OD-E fixture) refreshes `/incidents/{id}` with no Family selected. The Incident renders after authorization, with its Family selected per `REQ-UX-014`, and **not** the "Choose your current Family" prompt. During loading there is a neutral loading state and no context prompt.

**Links**

11. `AC-11` — **Event list links.** Each Events list card has an `a[href="/events/{id}"]` named by the Event. **Edit event details** remains a separate button.
12. `AC-12` — **Plan list links.** Each Plans list card has an `a[href="/plans/{id}"]` named by the Plan.
13. `AC-13` — **Trip list links.** Each Trips row and retained-creator row is an `a[href="/trips/{id}"]`.
14. `AC-14` — **Concern list links.** Each Concerns row is an `a[href="/concerns/{id}"]`, keeping its existing accessible name.
15. `AC-15` — **Incident list links.** Each Incidents row is an `a[href="/incidents/{id}"]`, keeping its existing accessible name.
16. `AC-16` — **Home row links.**
    - Each existing Home Event, Plan, and Trip row has an `a[href]` to its item URL, named by the item.
    - The Safety card, "View all", "Needs your attention", and the low-data card are unchanged: the same text and button count (component test).
    - No new Home summary or Safety item feed.

    > **Supersession note (2026-10-03, `DEC-076`).** The original wording above is kept for the record.
    >
    > - **Still required, still passing:**
    >   - the Home Event, Plan, and Trip rows link to `/events/:id`, `/plans/:id`, and `/trips/:id` and are named by the item;
    >   - item navigation from Home works, including focus and Back/Forward;
    >   - links are semantic `a[href]`;
    >   - no authorization behavior changed;
    >   - no Safety item feed or Home summary data was added.
    > - **Superseded by `WI-009-006 — Home Dashboard Prototype Alignment`** (owner visual approval 2026-10-03):
    >   - the second bullet's requirement that the Safety card, "View all", "Needs your attention", and the low-data card keep the same text, button count, and composition;
    >   - the old low-data Home composition;
    >   - the old Safety and Home card presentation.
    >
    > The intentional redesign does not fail this criterion.

**Authorized Family context**

17. `AC-17` — **Cross-Family Event switches Family.** A is in two Families with Family 1 selected, and opens an authorized Event of Family 2 by URL. Then:
    - the selected Family, the top-bar name, and the selector stay Family 1 **until the item read returns `200`**. Playwright holds the response with `page.route`; a component test asserts before and after `flush`;
    - they then switch to Family 2;
    - Family 1's lists, directory, and form values are cleared and Family 2's are reloaded;
    - no Family 2-scoped request is sent before the `200`;
    - "Switched to {Family 2 name}." is announced once.
18. `AC-18` — **Cross-Family Plan switches Family.** As `AC-17`, for a Plan.
19. `AC-19` — **Cross-Family Concern switches Family.** As `AC-17`, for a Concern opened by URL **and** from the Concerns list row (OD-F).
20. `AC-20` — **Cross-Family Incident switches Family.** As `AC-17`, for an Incident opened by URL **and** from the Incidents list row (OD-F).
21. `AC-21` — **Active-Family Trip and same-Family opens.**
    - An authorized Trip of the non-selected active Family switches as in `AC-17`.
    - For every type, an authorized item in the already selected Family renders with no Family change, no Family-scoped reload, and no announcement (component tests; Playwright for at least one type).
22. `AC-22` — **Trip creator without active Family (OD-A).** For an authorized Trip (`200`) whose Family is still absent from the caller's list after one refresh:
    - the Trip renders with the existing detail;
    - the selected Family (or "none") is unchanged;
    - there is no announcement;
    - no Family is created or listed in the selector.

    The same situation for an Event, Plan, Concern, or Incident gives the generic unavailable state (see `AC-26`). Component tests; Playwright is not required, because no product path ends a membership in the scenario.
23. `AC-23` — **A missing item never switches Family.** For each type, a nonexistent GUID and a non-GUID ID (with no API request) leave the selected Family (including "none"), the top bar, and the selector unchanged, with no announcement and no Family-scoped reload.
24. `AC-24` — **An unauthorized item never switches Family.** For each type, an existing item the user may not read leaves everything unchanged as in `AC-23`. Cases:
    - not in the audience or participants;
    - an item of a Family the user does not belong to (OD-E);
    - an ended membership, where the scenario allows.
25. `AC-25` — **The same user-visible unavailable state.**
    - For each type, the missing, invalid-ID, unauthorized, and OD-B cases render the same card: the same heading, text, back link, and focus. The DOM differs only in the URL.
    - The page contains no name or ID of a Family the user does not belong to, and the item name does not appear before authorization.
26. `AC-26` — **Stale Family list (OD-B).** Component tests:
    - an authorized item whose Family is absent triggers **exactly one** `/api/family-groups/mine` refresh;
    - if the Family is then present, the normal switch happens;
    - if it is still absent, the non-Trip types show the generic unavailable state with the Family unchanged, and the Trip follows OD-A;
    - no second refresh happens;
    - a failed Family-list request shows the existing membership error and **Retry**, with no render and no switch;
    - a Family list still loading when the item returns is awaited, not treated as absent.

**Concern create, Concern → Incident, navigation**

27. `AC-27` — **Concern creation navigates to its route (OD-G).**
    - A successful create ends at `/concerns/{newId}`, showing the created Concern. A refresh there renders it.
    - With an initial entry that fails, the recovery panel ("Continue to Concern" / **Retry**) still works. Navigation happens only once the Concern is confirmed and no recovery is pending.
    - The ambiguous-create check still works.
    - Created data and permissions are unchanged.
28. `AC-28` — **Concern → Incident is correct.**
    - From `/concerns/{id}`, **Start an Incident…** opens `/incidents/new` with the subject and Family prefilled, exactly as today.
    - Navigation state carries only the three IDs. No Concern content, entries, or participants are copied or transferred.
    - Incident authorization is unchanged. Creating it opens `/incidents/{newId}`.
    - **Open the Concern** in the Incident links to `/concerns/{id}` and is authorized independently: an Incident participant without Concern access gets the Concern unavailable card.
29. `AC-29` — **Six-area selected navigation is correct.**
    - Event item → Events; Plan and Trip items → Plans & Trips; Concern and Incident items → Safety. Each with `aria-current="true"`.
    - Exactly one destination is current; the six areas, their order, and the landings are unchanged.
    - Table-driven `areaForView` test plus a routed shell test.
30. `AC-30` — **No authorization or privacy behavior changes.** Diff review:
    - No product file under `src/FamilySafety.Api`, `Application`, `Domain`, or `Infrastructure` changes. The only server-project change is the opt-in Development fixture (`AC-34`).
    - Deep links use only item-ID reads.
    - The client makes no access decision. The adopt method only selects among Families already returned by `/api/family-groups/mine`.
    - No Family name is taken from an item response.
    - Concern and Incident denied-read audit behavior is unchanged: one read per open.

**Retained checks**

31. `AC-31` — **Live server-response precheck (before significant client work).** For each of the five types, Playwright API requests (Development identity header) for a missing ID and for an unauthorized existing ID give the same status (`404`), an empty body, and the same `Content-Type` and `Content-Length`. **Any material difference is a stop condition.** It must not be masked in the client.
32. `AC-32` — **Back and forward.** For each type:
    - list → item → Back returns to the list, and Forward returns to the item, with the correct content and area and no focus jump on `popstate`;
    - Home → item → Back returns to Home;
    - Back after a cross-Family switch keeps the new selection. There is no automatic switch back, because `REQ-UX-011` session selection is unchanged.
33. `AC-33` — **Announcement semantics (OD-H).** "Switched to {name}." appears in an always-present polite live region only after an actual automatic switch, with the name from the caller's Family list. A component test returns an item whose own `familyGroupName` differs, and asserts that the list's name is used. It never appears for same-Family opens, unavailable items, OD-A Trips, or before authorization. It does not move focus.
34. `AC-34` — **Fixture boundary (OD-E).** Diff and run review:
    - the second Family is seeded only when the scenario option is requested;
    - it is Development-only, with the same guard pattern as the Trip-audience scenario;
    - it uses fixed GUIDs;
    - there is no endpoint, schema, migration, domain, or production-registration change;
    - existing specs run without it and pass unchanged.
35. `AC-35` — **Existing behavior preserved.**
    - `concern-workflow`, `incident-workflow`, `trip-audience-removal` (including its denied and unavailable steps), `invitation-handoff`, and `navigation` pass, with only `button` → `link` locator changes for item rows.
    - Existing Angular specs pass; every changed assertion or setup is listed in the VER with its reason.
    - Event edit, Trip audience and Notes, Concern lifecycle, and Incident coordination are unchanged.
36. `AC-36` — **Accessibility review** (keyboard and code review, recorded in the VER):
    - item links are reachable and operable by keyboard at 1440 and 390 px, with visible focus;
    - focus follows `WI-009-003` D4;
    - the unavailable heading receives focus;
    - the live region is polite;
    - each item page has one `h1`, and the item `h2` is in a headed card.
37. `AC-37` — **Focused Playwright spec passes:** `e2e/item-links.spec.ts`, at 1440 and 390 px, with `useIsolatedScenario` and the OD-E option.
    - It covers `AC-01`–`AC-21`, `AC-23`–`AC-25`, `AC-27`, `AC-28`, `AC-31`, and `AC-32`.
    - Captures go only to `e2e/captures/item-links/`.
38. `AC-38` — **Angular tests pass**, including the new item-route, adopt (`AC-17`–`AC-26`), announcement, unavailable, link, Home, and Concern-create tests.
39. `AC-39` — **One full Playwright suite passes** (`npm run test:e2e`) once at completion. The end-of-run summary shows every created database removed and port 5080 free. No repeated reliability sequence.
40. `AC-40` — **Production build passes** (`npm run build`).
    - No budget threshold changes and no new dependency.
    - `concerns.scss` compiled size does not grow; otherwise stop and propose the smallest split.
    - The initial bundle and warnings are reported before and after.
    - With OD-E: `dotnet build` passes with no new warnings, and existing .NET tests are unaffected. The VER states which tests ran.
41. `AC-41` — **Rendered evidence and artifact policy.**
    - Desktop and 390 px captures go into `ai-context/planning/verification/evidence/WI-009-005/` only during verification, with SHA-256 recorded and nothing overwritten.
    - The captures show: each item detail, each type's unavailable card, a cross-Family switch with its announcement, and Home rows with links.
    - Routine runs write nothing under `ai-context/`.
    - Leftovers are handled only through `scripts/test-artifacts.ps1`.
42. `AC-42` — **Owner visual approval** at desktop and narrow widths. The item cannot close without it.

## Tasks

- [x] `TASK-001` — **Baseline.**
  1. Run the Angular tests and `npm run build`. Record the counts, the bundle, warnings, and the compiled size of `concerns.scss`.
  2. Confirm ports 4200 and 5080 are free and that `scripts/test-artifacts.ps1` reports no leftovers.
  3. Run the Concern, Incident, and Trip specs once as the regression baseline.
- [x] `TASK-002` — **Second-Family fixture (OD-E, § 11).** Add the script switch, the Development fixture option, and the `isolated-scenario.ts` option. Run `dotnet build`. Confirm the existing specs are unaffected.
- [x] `TASK-003` — **Live server-response precheck (`AC-31`).** Write the API-level part of `item-links.spec.ts` and run it **before** the UI work. If any type differs materially, **stop and report**.
- [x] `TASK-004` — **Routes and navigation.** Add the four routes, the item views in `navigation.ts`, `areaForView`, and their tests. Add route-parameter reading in `App` and `item-route.ts`.
- [x] `TASK-005` — **Family decision and shell.** Add `adoptAuthorizedItemFamily` (§ 2, OD-A, OD-B), the polite status region (OD-H), the item-route host conditions, and `item-unavailable.ts`. Write component tests for `AC-17`–`AC-26` and `AC-33`.
- [x] `TASK-006` — **Event detail.** Add the shared template, the detail, read → adopt → render, edit from the detail, and the list heading link. Tests. Apply the OD-C stop rule if needed.
- [x] `TASK-007` — **Plan detail.** The same for Plans, read-only. Tests.
- [x] `TASK-008` — **Trips.** The `tripId` input, read → adopt (including `'retained'`), the unavailable card, links for rows and Back, and the host condition. Update `trips.spec.ts`.
- [x] `TASK-009` — **Concerns.** The `concernId` input, read → adopt, removal of `openConcernId`, links for rows and Back, the create navigation (OD-G and its constraint), and the `.item-link` utility. Update `concerns.spec.ts`. Check the `concerns.scss` size.
- [x] `TASK-010` — **Incidents.** Adopt in `IncidentDetail`, row links, the lineage link to `/concerns/{id}`, and the refresh host condition. Update `incidents.spec.ts` and `incident-detail.spec.ts`.
- [x] `TASK-011` — **Home links.** Add the row links and update `home.spec.ts` (`AC-16`).
- [x] `TASK-012` — **Playwright.**
  1. Complete `item-links.spec.ts` (`AC-37`).
  2. Make the locator-only updates in `concern-workflow.spec.ts`, `incident-workflow.spec.ts`, and `trip-audience-removal.spec.ts`.
- [x] `TASK-013` — **Verification runs.**
  1. Angular tests.
  2. The focused specs during work.
  3. One full `npm run test:e2e` at completion.
  4. `npm run build`.
  5. `dotnet build` (fixture).
- [x] `TASK-014` — **Evidence and reviews.** Rendered captures (`AC-41`) and the accessibility review (`AC-36`).
- [x] `TASK-015` — **Record and hand back.** (Step 1 done 2026-10-03: `VER-WI-009-005` written; status `VERIFYING / OWNER REVIEW`. Step 2 done 2026-10-03 at owner closure.)
  1. Write `VER-WI-009-005.md` (`SELF-VERIFIED`) and set this item to `VERIFYING / OWNER REVIEW`. Do not mark it `DONE`.
  2. **At owner closure only:** update `project-status.md`, `roadmap.md`, the README checkpoint, and the `claude-handoff` state files.

## Expected files

| File | Change |
|---|---|
| `src/FamilySafety.Web/src/app/app.routes.ts` | Four item routes |
| `src/FamilySafety.Web/src/app/navigation.ts`, `navigation.spec.ts` | Item views and area mapping |
| `src/FamilySafety.Web/src/app/item-route.ts` (+ spec) | **New** |
| `src/FamilySafety.Web/src/app/item-unavailable.ts` (+ spec) | **New** |
| `src/FamilySafety.Web/src/app/app.ts`, `app.html` | Route params, Event/Plan item reads and details, adopt method, status region, host conditions, list and Home links |
| `src/FamilySafety.Web/src/styles.scss` | `.item-link` reset; status region (token-only) |
| `src/FamilySafety.Web/src/app/trips/trips.ts`, `trips.html`, `trips.spec.ts` | Item input, adopt, unavailable card, links |
| `src/FamilySafety.Web/src/app/concerns/concerns.ts`, `concerns.html`, `concerns.spec.ts` | Item input, adopt, state path removed, links, create navigation |
| `src/FamilySafety.Web/src/app/incidents/incidents.ts`, `incident-detail.ts`, `incident-detail.html`, `incident-list.html`, `incidents.spec.ts`, `incident-detail.spec.ts` | Adopt, links, lineage link |
| `src/FamilySafety.Web/src/app/app.spec.ts`, `home.spec.ts`, `shell.spec.ts` | New and updated tests |
| `src/FamilySafety.Web/e2e/item-links.spec.ts` | **New** |
| `src/FamilySafety.Web/e2e/concern-workflow.spec.ts`, `incident-workflow.spec.ts`, `trip-audience-removal.spec.ts` | Locator only |
| `scripts/start-invitation-scenario.ps1`, `src/FamilySafety.Api/Development/DevelopmentFixtureRegistration.cs`, `src/FamilySafety.Web/e2e/fixtures/isolated-scenario.ts` | Opt-in second-Family fixture (OD-E) |
| `docs/local-development.md` | One line on the new scenario option, if that section lists options |
| `ai-context/planning/verification/evidence/WI-009-005/`, `ai-context/planning/verification/VER-WI-009-005.md` | **New** |
| This work item | Task checkboxes and implementation record |

**Not expected:** `concerns.scss` growth; `app.scss`, `angular.json`, `package.json`, `playwright.config.ts`; any API endpoint, application service, domain, infrastructure, `DbContext`, migration, `Program.cs`, or production registration file. Needing one is a stop condition. Selector-only edits in `trips.scss` or `incident-list.scss` for `a` rows are allowed if their net size does not grow, and must be recorded.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md`, `project-status.md` | Mandatory operating rules and checkpoint | Entry |
| This work item, especially [Owner decisions](#owner-decisions-2026-10-02) | Scope authority | Entry |
| `core/ux-contract.md#req-ux-014` (as amended in `CTX-1.28`), `#req-ux-011`, `#req-ux-013` | Deep-link rule, OD-A and OD-B, Family change, area mapping | Before execution |
| `core/privacy-safety-contract.md#req-priv-027`, `#req-priv-022` | Nondisclosure; Trip creator retention | Before execution |
| `decision-log.md#dec-074` (D4), `#dec-075` | The flow and these decisions | Before execution |
| `planning/verification/VER-WI-009-004.md` (§ "For the later `REQ-UX-014` client item") | Event and Plan read contract | Before execution |
| `planning/stories/WI-009-003-…md` (D3, D4, D5) | Area mapping, heading focus, link semantics | Before execution |
| `claude-handoff/WORKING-RULES.md`, `VALIDATION.md`, `SESSION-CLOSEOUT.md` | Execution rules, test cadence, artifact policy | Before execution |
| `docs/local-development.md` § "Isolated Playwright scenarios" | New spec and the fixture option | Before execution |
| The source, spec, script, and fixture files in [Expected files](#expected-files) | Implementation surface | Before execution |

## Context intentionally not loaded

| Context | Why |
|---|---|
| `workflows/safety.md`, `REQ-SAFE-015/016`, `REQ-PRIV-025/026` in detail | Concern and Incident behavior is preserved; only opening changes |
| Phase 8 stories and VER records | Closed history |
| `REQ-EVT-010` to `REQ-EVT-013` | Phase 9.5; must not appear on the Event detail |
| `claude-handoff/reference/` mockups | The detail views reuse existing cards |
| Bootstrap, archive, Phase 5–7.5 records | Not relevant |

## Stop conditions

Stop and report, without guessing, if execution would require:

- any API endpoint, contract, response-field, status-code, audit, schema, migration, or production-registration change, **including** a material missing-versus-unauthorized difference found by `AC-31`;
- any change to Event, Plan, Trip, Concern, or Incident authorization, audience, participants, lifecycle, or create/edit semantics;
- switching Family from the URL, navigation state, list metadata, or before a `200`; taking a Family name from an item response; or retrying the Family list more than once;
- extending the OD-A exception beyond Trips, or applying it where the server did not return `200`;
- a new item data field solely for routing;
- duplicating the Event edit logic or markup, or an `App` change well beyond § 10. Report the exact Event-card extraction (OD-C);
- a broad `App` rewrite or extraction of Events, Plans, Home, or Family;
- Concern create navigation that cannot preserve the recovery and ambiguous-create behavior (OD-G);
- Concern style additions that would grow `concerns.scss` past its budget. Propose the smallest safe split; never raise the budget;
- a budget threshold change or a new dependency;
- a fixture change outside the OD-E boundary (§ 11, `AC-34`);
- any `OD-002` or Phase 9.5 capability;
- a change in meaning to an existing journey assertion beyond the item-row `button` → `link` locators;
- test failures not attributable to this item.

## Risks

| Risk | Mitigation |
|---|---|
| A switch or render before authorization, through a race with the membership load | Adopt waits for the list; hosts render after the promise; ordering tests `AC-17`–`AC-20` |
| A target Family name leaks (Trip, Concern, and Incident responses carry `familyGroupName`) | The name comes only from the caller's list; nothing renders before `200`; `AC-25`, `AC-33` |
| Multi-Family refresh shows the context prompt | Item-route host conditions; `AC-10` |
| OD-G navigation drops the Concern create recovery state | The OD-G constraint, or the shared matcher; `AC-27`; stop condition |
| A second Trip read after a switch (re-keying) | Acceptable (no Trip audit), or drop the re-key on the item view; recorded |
| `concerns.scss` crosses its budget | Anchor reset in `styles.scss`; `AC-40`; stop condition |
| The fixture leaks into ordinary runs | Opt-in, off by default, Development-guarded; `AC-34` |
| Existing specs depend on row buttons | Locator-only changes, listed (`TASK-012`) |
| `App` grows | Bounded estimate and the OD-C stop rule |

## Blockers and dependencies

- **Blockers:** none.
- **Depends on:** `WI-009-004` (done); `WI-009-001` and `WI-009-002` harness and artifact lifecycle (done); `WI-009-003` navigation (done); `CTX-1.28` (active).
- **Later items that depend on this:**
  - M4 cleanup, M5, and M6 (Home);
  - M9 (journeys open items from Home; the multi-Family journey reuses the OD-E fixture);
  - M10 (axe on item routes).

## DEC-032 review

- **In favor:** client-side; reversible; no new access path.
- **Against, and decisive:** `DEEP` tier; it changes when the selected Family changes; it handles cross-Family private items; it needed owner decisions and a canonical clarification; it requires owner visual approval.

**Conclusion:** `DEC-032` does not apply. Explicit owner approval and execution authorization were given on 2026-10-02 (below).

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES` — `APPROVED — EXECUTION AUTHORIZED` against `CTX-1.28`
- **Authorized by:** Project owner
- **Authorized at:** 2026-10-02 (`DEC-075`)
- **Authorized scope:** exactly this work item's design, [Owner decisions](#owner-decisions-2026-10-02) OD-A to OD-I, 42 acceptance checks, and 15 tasks.
  - **Included:**
    - Angular routes, shell, Event and Plan detail views, and the Trip, Concern, and Incident changes;
    - list and Home links and the `styles.scss` utility;
    - component tests and the new Playwright spec;
    - locator-only changes to existing specs;
    - the OD-E Development / E2E fixture (script, Development fixture, isolated-scenario option), as test support only;
    - rendered evidence and the verification record.
  - **Excluded:** everything under [Explicitly excluded](#explicitly-excluded), including any production API, schema, authorization, or registration change, the Plan `Location` header, `App` extraction beyond the OD-C stop-and-report path, `@axe-core/playwright`, and `OD-002`.
  - **Process:**
    - No further routine approval is needed; the stop conditions still apply.
    - Run `AC-31` before significant client work.
    - Hand back in `VERIFYING / OWNER REVIEW`; do not mark `DONE / PASS`.
    - Do not commit or push.
- **Implementation started:** `YES` (2026-10-02; completed and self-verified 2026-10-03)

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| `AC-01`–`AC-41` | `VER-WI-009-005.md` | `PASS` (self-verified; `AC-16` with the `DEC-076` supersession note) |
| `AC-42` | Owner visual approval | `PASS` (2026-10-03) |

- **Verification report:** `planning/verification/VER-WI-009-005.md`
- **Independence:** `SELF-VERIFIED` expected, plus owner visual review
- **Outcome:** `DONE / PASS` — `OWNER APPROVED` (2026-10-03)

## Owner closure (2026-10-03)

- **Decision:** `DONE / PASS` — `OWNER APPROVED`. Owner review date: 2026-10-03.
- **AC-42 owner visual approval:** `PASS`.
- **AC-16:** `PASS`. Its row-link requirement still holds and was re-proven. Its Home-presentation clause (the same Safety card, "View all", "Needs your attention", and low-data card text and button count) is **superseded** by `WI-009-006 — Home Dashboard Prototype Alignment` (`DEC-076`), an owner-approved Home redesign made during this item's owner review. The intentional redesign does not fail `AC-16`. The original wording is kept above, with its note.
- **Final regression (shared with `WI-009-006`, 2026-10-03):**
  - Angular: 206/206.
  - Focused Playwright (`item-links`, `navigation`, `invitation-handoff`, `concern-workflow`, `incident-workflow`): 19/19.
  - One full `npm run test:e2e`: 20 passed, 0 failed, 0 skipped. 6 scenario databases were created and 6 removed, with 0 retained.
  - `npm run build`: passes with the existing warnings only.

  See `VER-WI-009-005` § Final regression and owner closure.
- **Home evidence recaptured:** `home-row-links-1440-recaptured-2026-10-03.png` and `home-row-links-390-recaptured-2026-10-03.png`, added beside the original captures. The originals are kept as the historical record.
- **Carried forward, unscheduled:** the Plan create `Location` header (OD-I).
