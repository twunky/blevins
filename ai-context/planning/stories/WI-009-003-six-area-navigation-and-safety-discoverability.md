---
Status: DONE / PASS — OWNER APPROVED
Authority: Project-owner approved and execution-authorized for WI-009-003 only (2026-10-02), against CTX-1.27, with owner decisions D1–D6 recorded below; planning basis PRP-030 M2 / DEC-074 D2
Context-Baseline: CTX-1.27
Version: 1.0
---

# WI-009-003 — Six-Area Navigation and Safety Discoverability

## Metadata

- **Work Item ID:** `WI-009-003`
- **Status:** `DONE / PASS` — `OWNER APPROVED` (owner review 2026-10-02). Approved and execution-authorized by the project owner on 2026-10-02 against `CTX-1.27`; implemented and self-verified the same day; AC-23 owner visual approval `PASS` on 2026-10-02. See [Owner closure](#owner-closure-2026-10-02), [Implementation record](#implementation-record-2026-10-02), and `VER-WI-009-003`.
- **Phase:** `9 — Integration & UX Refinement`, `CURRENT / IN PROGRESS` (`DEC-073`)
- **Context baseline:** `CTX-1.27`
- **Approved planning basis:** `PRP-030` (`APPROVED`, `DEC-074`), MUST item **M2** — implement the six-area navigation. Theme 2 "Navigation and discoverability".
- **Requirement references:** `REQ-UX-013` (implemented here); `REQ-UX-011` (current-Family context, preserved); `REQ-PRIV-027` (navigation is not authorization, preserved); `REQ-UX-003` and `REQ-UX-010` (accessibility and narrow-width use, as verification targets). No requirement is changed.
- **Model tier:** `DEEP`. `PRP-030` routes navigation (`OD-001`) work to `DEEP`. The code change is small; the tier reflects that it implements a just-resolved product decision on the shared shell.
- **Phase 9 exit criteria served:** 2 (navigation model) in full; 3 (groupings reach every part) in full; 13 (owner visual review) and 14 (authorization preserved) for the changed screens; contributes to 11 (grouping landings have a way forward and back) and 12 (manual `aria-current` and focus review on navigation). Exit criterion 12's `@axe-core/playwright` scan is **not** part of this item (M10).
- **Standing auto-approval:** `DEC-032` does **not** apply (see [DEC-032 review](#dec-032-review)). Explicit owner approval and execution authorization were given on 2026-10-02, so no further routine approval is needed.

## Goal

Every implemented area of the application is reachable from one six-item main navigation — **Home, Family, Events, Plans & Trips, Safety, Me / Settings** — at desktop and at 390 px, built from semantic router links, with a clear current-area indication, keyboard access, and no change to any authorization, data, route, or lifecycle behavior.

Concerns and Safety Incidents, which today are reachable only from the Home "Safety coordination" card, become discoverable through **Safety**. Plans and Trips are reached through **Plans & Trips**.

## Owner decisions (2026-10-02)

These decisions are authoritative. They override any looser wording elsewhere in this item.

### D1 — Grouping landing pages: approved

- `/plans-trips` is the **Plans & Trips** landing; `/safety` is the **Safety** landing.
- They are navigation landing pages only. They must not become product domains.
- **Plans & Trips landing:** two clear destinations, Plans and Trips. No counts, summaries, combined data, recent-item feeds, audience information, or cross-domain lifecycle behavior. Plans and Trips keep their independent routes, APIs, authorization, audiences, and behavior.
- **Safety landing:** two clear destinations, Concerns and Safety Incidents. It does not fetch Concern or Incident data to render, and shows no counts and no open/closed state. It implies nothing exists and reveals no private Safety information. It looks the same whether or not the caller has any Concern or Incident records.
- Both landings use the existing approved cards and spacing.

### D2 — Narrow navigation: approved

- At approximately 390 px, the six destinations display as a **3 row × 2 column grid** in the existing top-of-content position.
- No horizontal overflow; targets at least 44 × 44 px; readable labels; visible selected state; visible keyboard focus; no hover dependency.
- No "More" menu and no hidden seventh destination.
- Same logical order as desktop: Home, Family, Events, Plans & Trips, Safety, Me / Settings, read row by row (Home | Family / Events | Plans & Trips / Safety | Me / Settings).
- Between 390 px and the desktop breakpoint, any layout that keeps all six visible in this order, without overflow, is acceptable (for example the same 2-column grid, or a 3-column or single row where it fits). Desktop (≥ 64 rem) keeps the approved sidebar.

### D3 — Current-area semantics: approved

- On an area's own page, that destination has `aria-current="page"`: `/home`, `/family`, `/events`, `/plans-trips`, `/safety`, `/me`.
- On a child route within an area, the area's destination has `aria-current="true"`:
  - `/plans`, `/trips` → Plans & Trips;
  - `/concerns`, `/incidents`, `/incidents/new`, `/incidents/:incidentId` → Safety.
- Exactly one primary destination is current, and it is also visually selected.
- The mapping is UI/navigation state only and never participates in authorization.

### D4 — Focus after navigation: approved

- After **user-initiated SPA navigation** through a main-navigation link or a landing-page link, focus moves to the destination page's primary `h1`.
- The `h1` is made programmatically focusable (`tabindex="-1"`) without changing its visual presentation; focus behavior stays visible and appropriate.
- No focus movement on initial application startup, on a browser refresh, or on internal state changes (data loads, Family switch, Incident sub-screen changes), and never repeatedly.
- Use a simple, explicit user-navigation signal, not a broad global router side effect. Recommended mechanism: the main-navigation and landing links pass router navigation state (for example `[state]="{ focusHeading: true }"`). After the new view renders, `App` focuses the `h1` once only when that flag is present **and** the navigation trigger is `imperative` **and** it is not the router's initial navigation. Angular restores history state on refresh and on back/forward, so the trigger and initial-navigation checks are required.
- Back/forward stays natural: `popstate` navigations do not move focus.

### D5 — Semantic navigation links: approved (changes the proposal)

- The six primary destinations are **links** with real URLs, using Angular `routerLink`, inside a `nav` landmark labeled "Main navigation". They may keep the approved navigation styling.
- The landing-page choices are also `routerLink` links.
- No JavaScript click handlers where `routerLink` is sufficient. `aria-current` is bound from the D3 mapping rather than `routerLinkActive`, so that `"page"` and `"true"` are set correctly.
- Other existing in-page buttons that call `navigate()` (for example the Home cards) are outside this item and stay as they are.
- **Test consequence:** Playwright locators that find the six nav items by `button` role must find them by `link` role. That covers `trip-audience-removal.spec.ts` (Trips, approved below) and `invitation-handoff.spec.ts` (`Family` at lines 95 and 97, `Home` at line 159). These are locator-only changes, mechanically required by D5, with no assertion change.

### D6 — Routes: approved

- `/plans-trips` (Plans & Trips) and `/safety` (Safety) are added.
- `/plans`, `/trips`, `/concerns`, `/incidents`, `/incidents/new`, and `/incidents/:incidentId` remain and continue to work directly and after refresh.

### Further owner direction

- **Order:** exactly Home, Family, Events, Plans & Trips, Safety, Me / Settings, at every width.
- **Mapping:** the small, explicit typed view → area mapping is approved. Do not build a generalized navigation framework.
- **Visual:** preserve `DEC-070`. Desktop: approved blue sidebar, slim top bar, existing proportions. Narrow: the D2 grid in the current blue visual language. Do not redesign the application.
- **No `@axe-core/playwright`** in this item.
- **`trip-audience-removal.spec.ts`:** a locator-only change is approved so that Trips is reached through Plans & Trips → Trips. Its behavioral assertions must not be weakened or altered.
- **Routing boundary:** no `REQ-UX-014` item routing, Event or Plan single-item reads, item-detail APIs, Home item links, cross-Family deep-link switching, or Concern item URL changes.
- **No `App` extraction** for cleanup; only the minimal shell and view changes this item needs.

## Current implementation (verified 2026-10-02, static review)

### Shell structure

- `DevelopmentSession` (`src/app/development-session.ts`) is the bootstrapped root; it hosts the router outlet.
- Every primary route renders the same routed component, `App` (`src/app/app.ts` 1,409 lines, `app.html` 1,109 lines). `App` contains the shell: the slim top bar (brand, current Family, Family selector, profile chip), the sidebar `<nav aria-label="Main navigation">`, and `<main>` with one `h1`.
- Each route has its own route configuration, so moving between areas **re-creates `App`** and reloads profile and membership data (`shell.spec.ts` relies on this). The Incident routes share one matcher-based configuration, so moving within `/incidents…` reuses the instance.
- Which area renders is a `view` signal: `'home' | 'profile' | 'family' | 'events' | 'plans' | 'trips' | 'concerns' | 'incidents' | 'invitation' | 'handoff'`. `ngOnInit` sets it from the route path; `navigate(view)` sets it and calls `router.navigateByUrl`, guarded so component tests without a router still switch views locally.
- Home, Family, Events, Plans, and Me / Settings are template sections inside `app.html`. Trips (`app-trips`), Concerns (`app-concerns`), and Incidents (`app-incidents`, `@defer`) are separate components rendered by `App`.

### Routes (`src/app/app.routes.ts`)

| URL | Area today | Notes |
|---|---|---|
| `/` | redirects to `/home` | Kept empty so `#invite=` links survive |
| `/home`, `/family`, `/events`, `/me` | own nav button | `/me` ↔ `view = 'profile'` |
| `/plans` | own nav button | Plan list and create form in `app.html`; no item URL |
| `/trips` | own nav button | `Trips` component; Trip detail is in-component state; no item URL |
| `/concerns` | **no nav entry** | List and detail in `Concerns`; a Concern opens through router navigation state (`openConcernId`), not a URL |
| `/incidents`, `/incidents/new`, `/incidents/:incidentId` | **no nav entry** | `incidentsMatcher`; `Incidents` reads the URL on `NavigationEnd` |
| `**` | redirects to `/home` | |

All of these are path routes, so all are refresh-safe today at the list level. No route uses a guard or resolver; authorization is entirely server-side.

### Navigation

- **Desktop (≥ 64 rem / 1024 px):** a sidebar grid of six `<button class="nav-button">` items: Home, Family, Events, **Plans**, **Trips**, Me / Settings. Each has a decorative `app-icon` and a text label.
- **Narrow (< 64 rem):** the same six buttons as a wrapping flex row under the top bar (`flex: 1 1 9rem`). At 390 px this happens to wrap to three rows of two. There is no hidden menu and no bottom bar.
- **Selected state:** `.is-active` (blue tint, heavier weight) and `aria-current="page"` when `view()` equals the button's view exactly. On `/concerns` and `/incidents` no item is current (asserted by `home.spec.ts`).
- **Hidden in focused flows:** the nav is not rendered for `invitation` and `handoff`.
- **Keyboard and focus:** native buttons in DOM order; global `:focus-visible` 3 px outline (`styles.scss`, which already covers `a`); min height 2.75 rem. No skip link. Because `App` is re-created on each navigation, focus returns to the document start after a nav activation.
- **Hover:** a hover tint only; nothing depends on hover.

### Tests that touch navigation

| File | Dependency on today's nav |
|---|---|
| `src/app/shell.spec.ts` | Routes to each existing area and asserts `h1` and the `aria-current="page"` label |
| `src/app/home.spec.ts` | Asserts the nav labels are exactly `Home, Family, Events, Plans, Trips, Me / Settings` (via `nav button`) and that `/incidents` has no current item |
| `src/app/first-use.spec.ts` | Clicks the `Home` and `Plans` nav **buttons** through a local `click()` helper, with `provideRouter([])` (no routes) |
| `src/app/app.spec.ts` | `provideRouter([])`; check during execution for nav usage |
| `e2e/trip-audience-removal.spec.ts` | `getByRole('button', { name: 'Trips', exact: true })` at lines 134, 190, 220, 297 |
| `e2e/invitation-handoff.spec.ts` | `getByRole('button', { name: 'Family' / 'Home', exact: true })` at lines 95, 97, 159 |
| `e2e/concern-workflow.spec.ts`, `e2e/incident-workflow.spec.ts` | Use `page.goto(...)` and the Home card buttons; do not use the nav |

Because the nav becomes `routerLink` links, component specs that click nav items need a router with the real `routes` (as `shell.spec.ts` and `home.spec.ts` already use) or must assert on the link `href` instead of clicking. A spec using `provideRouter([])` cannot follow a nav link. Changing that test setup is in scope; the behavior each test checks must stay the same.

## Design

### 1. One explicit navigation definition

Add `src/app/navigation.ts`, a small, framework-free module that is the single place a developer reads or changes the navigation:

```ts
export type PrimaryArea = 'home' | 'family' | 'events' | 'plansTrips' | 'safety' | 'me';

/** The six REQ-UX-013 destinations, in the required order. */
export const primaryDestinations: readonly PrimaryDestination[] = [
  { area: 'home',       label: 'Home',          icon: 'home',     path: '/home',        view: 'home' },
  { area: 'family',     label: 'Family',        icon: 'users',    path: '/family',      view: 'family' },
  { area: 'events',     label: 'Events',        icon: 'calendar', path: '/events',      view: 'events' },
  { area: 'plansTrips', label: 'Plans & Trips', icon: 'plane',    path: '/plans-trips', view: 'plansTrips' },
  { area: 'safety',     label: 'Safety',        icon: 'shield',   path: '/safety',      view: 'safety' },
  { area: 'me',         label: 'Me / Settings', icon: 'user',     path: '/me',          view: 'profile' },
];

/** Which primary area is current for each view. UI state only; never used for authorization (REQ-PRIV-027). */
export const areaForView: Record<NavigableView, PrimaryArea> = {
  home: 'home', family: 'family', events: 'events',
  plansTrips: 'plansTrips', plans: 'plansTrips', trips: 'plansTrips',
  safety: 'safety', concerns: 'safety', incidents: 'safety',
  profile: 'me',
};
```

(Shape is illustrative; names may be adjusted in execution. The rule is one table, typed so a new view cannot be added without an area. No generalized framework.)

- `app.html` renders the nav with one `@for` over `primaryDestinations`: each item is `<a [routerLink]="path" [state]="{ focusHeading: true }">` with the icon and label, styled with the existing nav-item styles.
- Existing icons are reused (`plane`, `shield`); no icon is added.

### 2. Grouping landing pages (D1, D6)

Two new path routes, rendered by `App` like the others:

| URL | `view` | Page title (`h1`) | Content |
|---|---|---|---|
| `/plans-trips` | `plansTrips` | Plans & Trips | Two cards: **Plans** (existing Plans subtitle) with a link to `/plans`; **Trips** (existing Trips subtitle) with a link to `/trips` |
| `/safety` | `safety` | Safety | Two cards: **Concerns** (existing Concerns subtitle) with a link to `/concerns`; **Safety Incidents** (existing Incidents subtitle) with a link to `/incidents` |

- A small presentational component (`src/app/group-landing.ts`) renders the two cards with `routerLink` links (carrying the D4 focus state). It makes **no HTTP request**, reads no Family-scoped state, and shows no count, status, badge, or "nothing here" message. Its output is identical for every user.
- Link text names the destination clearly (for example "Open Plans", "Open Trips", "Open Concerns", "Open Safety Incidents"); exact wording is settled in execution and checked in owner review.
- The landings render whether or not a Family is selected, because they show no Family data. The child pages keep their current Family-context behavior unchanged.
- Card copy reuses existing approved subtitles, stays calm, and must not suggest that any Concern or Incident exists.
- The Home "Safety coordination" card and its buttons are **unchanged** (Home hub work is M4–M7).

### 3. Selected area (D3)

- `App` gets `currentArea = computed(() => areaForView[view()])` (undefined for `invitation` and `handoff`, where the nav is hidden).
- Each nav link gets `.is-active` when its area is current, and `[attr.aria-current]` = `'page'` when `view()` equals the destination's own view, `'true'` when only the area matches, otherwise `null`.
- Selected state is derived only from the route-driven `view`, never from data, membership, or permissions.

### 4. Narrow layout (D2)

- Edit only the shell navigation rules in `src/styles.scss` (the "Narrow" block).
- At about 390 px (`< 44 rem`): `display: grid; grid-template-columns: repeat(2, minmax(0, 1fr))`, which with six items gives three rows of two. Each link is at least 44 px tall, centered, with icon and label; labels may wrap inside their cell.
- 44–63.99 rem: keep all six visible in order without overflow (implementer's choice within D2).
- `document.documentElement.scrollWidth <= 390` at 390 px on every primary page and both landings.
- Desktop sidebar unchanged. Update the stale "OD-001 unchanged" comment in that block.
- Nav links need the existing nav-item look (`.nav-button` styles currently target `app-root .nav-button`, so applying them to an `a` element works if the class is kept or renamed consistently); add `text-decoration: none` and keep the 3 px `:focus-visible` outline.

### 5. Focus after navigation (D4)

See D4. Implement it in `App` after the view renders (for example `afterNextRender` in `ngOnInit`), reading `router.lastSuccessfulNavigation()` once: focus the `h1` only if `extras.state?.focusHeading` is true, `trigger === 'imperative'`, and the navigation is not the initial one (`previousNavigation` is not null). Do not subscribe to router events for this. For `/incidents` (shared instance), the nav or landing link from another area creates a new `App`, so the same check applies; Incident sub-screen changes are not focused.

### 6. Unchanged

- `navigate()` remains for existing in-page buttons and component tests without routes.
- No change to `Concerns`, `Incidents`, `Trips`, or the Plans section, their API calls, their navigation-state handoffs, or their unavailable states.

## Scope

### In scope

- `navigation.ts` with the six destinations and the view → area table.
- Data-driven main navigation in `app.html` using `routerLink` links, selected state, and D3 `aria-current`.
- `/plans-trips` and `/safety` routes and their data-free landing views (`group-landing.ts`).
- `App` changes needed for the new views: the `view` union, page titles and subtitles, `ngOnInit` route-to-view mapping, `navigate()` path mapping, `currentArea`, D4 focus handling, and the router imports.
- Narrow navigation layout in the shell section of `styles.scss`.
- Component tests, and updated navigation assertions and test setup in existing component specs.
- A focused Playwright navigation spec; locator-only updates in `trip-audience-removal.spec.ts` and `invitation-handoff.spec.ts` (D5).
- Rendered evidence, `VER-WI-009-003`, and the closeout context sync.

### Explicitly excluded

- Item-level URLs for Events, Plans, Trips, or Concerns; Event or Plan single-item server reads; new item-detail APIs; any `REQ-UX-014` work (M3).
- Cross-Family deep-link switching.
- Home changes: Home row links, Safety counts or summaries, past-item filtering, removing or converting Home buttons, first-run redesign (M4–M7).
- Any server, API, contract, authorization, schema, or migration change.
- Any change to Concern or Incident behavior, access, lifecycle, item URLs, or `concerns.scss`.
- Plan or Trip permission, audience, or lifecycle changes.
- Family administration changes.
- Extraction of Home, Family, Events, Plans, or Me / Settings from `App`.
- `@axe-core/playwright` (M10), Messages, notifications, search, Tasks, Sharing, Check-In, a "More" menu, or any admin-only primary destination.
- `OD-002` and all Phase 9.5 capabilities. `OD-001` is not reopened.
- Raising any budget threshold; any new dependency.

## Acceptance checks

1. `AC-01` — **Exactly six destinations, as links.** On every shell page at 1440 px and 390 px, `nav[aria-label="Main navigation"]` contains exactly six links (`a[href]`, role `link`) labeled, in order, Home, Family, Events, Plans & Trips, Safety, Me / Settings, with `href`s `/home`, `/family`, `/events`, `/plans-trips`, `/safety`, `/me`. The nav contains no buttons and no Plans, Trips, Concerns, or Incidents item. (Component test + Playwright.)
2. `AC-02` — **Home reachable** through main navigation at both widths; `h1` and selected area are correct.
3. `AC-03` — **Family reachable** likewise.
4. `AC-04` — **Events reachable** likewise.
5. `AC-05` — **Plans and Trips through Plans & Trips.** At both widths, following the Plans & Trips link shows `/plans-trips` with two links, and each opens `/plans` and `/trips` respectively, using main navigation and the landing alone.
6. `AC-06` — **Concerns and Incidents through Safety.** At both widths, following the Safety link shows `/safety` with two links, and each opens `/concerns` and `/incidents` respectively, using main navigation and the landing alone.
7. `AC-07` — **Me / Settings reachable** likewise.
8. `AC-08` — **Selected-area mapping (D3).** For every route — `/home`, `/family`, `/events`, `/plans-trips`, `/plans`, `/trips`, `/safety`, `/concerns`, `/incidents`, `/incidents/new`, `/incidents/<id>`, `/me` — exactly one nav link is visually selected and carries `aria-current`; it is the mapped area; the value is `page` on the area's own page and `true` on a child route. Covered by a table-driven unit test of `areaForView` and a routed component test.
9. `AC-09` — **Desktop visual direction.** At 1440 px the sidebar, slim top bar, single page title, light-blue content background, and white cards match `claude-handoff/reference/detail-shell-topbar.png` and the approved `DEC-070` shell; only the destination list and the two landing pages are new, and the landings use existing cards and spacing.
10. `AC-10` — **Narrow layout (D2).** At 390 px the six destinations form a 3 row × 2 column grid in the required order, all visible without scrolling the navigation or opening anything; each target is at least 44 × 44 px; labels are fully legible; and `document.documentElement.scrollWidth <= 390` on every primary page and both landings. No "More" menu or hidden destination exists at any width.
11. `AC-11` — **Keyboard.** At both widths, Tab reaches the six links in order after the top-bar controls; Enter follows each; landing links are reachable and operable by keyboard; focus is visible on every link (3 px focus style); nothing depends on hover.
12. `AC-12` — **Focus after user navigation (D4).** After following a main-navigation link or a landing link, focus is on the new page's `h1`, which has no visual change other than the focus indication. No focus movement occurs on initial load, on browser refresh, on back/forward, or on internal state changes; focus moves once per navigation. (Component test for the in-app and initial-load cases; Playwright for in-app, refresh, and back.)
13. `AC-13` — **Accessible current-area semantics.** The nav is a `nav` landmark labeled "Main navigation"; each link's accessible name equals its visible label (icons stay `aria-hidden`); `aria-current` follows D3; each landing has exactly one `h1`, and its two links sit in headed cards. Recorded as a manual and code-based review.
14. `AC-14` — **No authorization change.** No file under `src/FamilySafety.Api`, `Application`, `Domain`, or `Infrastructure`, and no API client or HTTP call (`concern-api.ts`, `incident-api.ts`, HTTP calls in `app.ts` and `trips.ts`) is changed. The existing Concern, Incident, Trip-audience, and invitation Playwright journeys pass, including their denied and unavailable steps.
15. `AC-15` — **Landings are data-free; no private Safety state.** Rendering `/safety` or `/plans-trips`, and rendering the nav on any page, makes no request to `/api/concerns`, `/api/incidents`, `/api/trips`, or any Plan endpoint beyond what the shell already loads on every page (component test with `HttpTestingController`; Playwright request listener on `/safety`). Neither landing nor the nav contains a count, digit, status, badge, item, audience, or empty/"nothing shared" text, and `/safety` renders identically for a user with and without Concerns or Incidents.
16. `AC-16` — **Direct routes still work (D6).** `/plans`, `/trips`, `/concerns`, `/incidents`, and `/incidents/new` load directly and after a browser refresh with the correct page and selected area; `/plans-trips` and `/safety` do too. Unknown paths still redirect to `/home`; `#invite=` entry is unchanged.
17. `AC-17` — **Groupings merge nothing.** Plans and Trips keep their own pages, create flows, and API calls; Concerns and Incidents keep theirs. The landing pages are navigation only (code review of the diff).
18. `AC-18` — **Focused Playwright navigation spec passes** (`e2e/navigation.spec.ts`) at 1440 px and 390 px, using `useIsolatedScenario`, covering `AC-01`–`AC-08`, `AC-10`–`AC-12`, `AC-15` (request listener), and `AC-16`, with captures written only to `e2e/captures/navigation/`.
19. `AC-19` — **Angular tests pass**, including the new navigation and landing tests and the updated `shell`, `home`, and `first-use` specs. Every changed assertion or test setup is listed in `VER-WI-009-003` with its reason; only navigation-label, link-role, and current-area expectations change meaning, as this item intends.
20. `AC-20` — **One full Playwright suite passes** (`npm run test:e2e`) once at story completion; the end-of-run summary shows all created databases removed and port 5080 free. The `trip-audience-removal` and `invitation-handoff` changes are locator-only, and their behavioral assertions are unchanged. No repeated reliability sequence.
21. `AC-21` — **Production build passes** (`npm run build`) with no budget threshold changed and no new dependency; the initial bundle and the existing warnings are reported before and after.
22. `AC-22` — **Test-artifact policy and rendered evidence.** Routine runs write nothing under `ai-context/`. Desktop (1440 px) and narrow (390 px) captures of every primary destination, both landings, and one child page per grouping (Plans, Trips, Concerns, Incidents) showing the selected area are copied into `ai-context/planning/verification/evidence/WI-009-003/` only during verification, with SHA-256 recorded and no existing evidence overwritten. Leftovers, if any, are handled only with `scripts/test-artifacts.ps1`.
23. `AC-23` — **Owner visual approval** at desktop and narrow widths. The item cannot close without it.

`AC-09`–`AC-13`, `AC-22`, and `AC-23` together, with `AC-01`–`AC-08` and `AC-18` as the automated reachability tests, satisfy the four-part `REQ-UX-013` validation standard.

## Tasks

- [x] `TASK-001` — **Baseline.** Run the Angular tests and the production build; record counts, initial bundle, and warnings. Confirm ports 4200/5080 are free and `scripts/test-artifacts.ps1` reports no leftovers.
- [x] `TASK-002` — **Navigation definition.** Add `navigation.ts` (destinations, `PrimaryArea`, `areaForView`) and `navigation.spec.ts` (order, labels, paths, complete mapping).
- [x] `TASK-003` — **Routes and views.** Add `/plans-trips` and `/safety` to `app.routes.ts`; extend the `view` union, `ngOnInit` mapping, `navigate()` path mapping, `pageTitle`, and `pageSubtitle` in `app.ts`; add `currentArea`.
- [x] `TASK-004` — **Landing pages.** Add `group-landing.ts` with `routerLink` links and render it for the two landing views in `app.html`, outside the Family-context prompt; component test including the no-request assertion (`AC-15`).
- [x] `TASK-005` — **Desktop and narrow navigation.** Replace the six hand-written buttons with `routerLink` links from `primaryDestinations`, with D3 `aria-current`; update the shell nav rules in `styles.scss` for D2 (link styling, 3 × 2 grid at 390 px). Check 1440, 1024, 768, and 390 px.
- [x] `TASK-006` — **Focus (D4).** Focus the `h1` after user-initiated navigation only; component tests for in-app navigation (focused) and initial load (not focused).
- [x] `TASK-007` — **Update existing tests.** `shell.spec.ts` (new destinations, link role, child-route mapping), `home.spec.ts` (nav labels via links; `/incidents` now selects Safety with `aria-current="true"`), `first-use.spec.ts` (reach Plans through Plans & Trips using a routed setup, or assert link targets), `app.spec.ts` if affected; Playwright locator-only: `trip-audience-removal.spec.ts` (link Plans & Trips → link Trips) and `invitation-handoff.spec.ts` (`Family` and `Home` by `link` role).
- [x] `TASK-008` — **Playwright navigation spec.** `e2e/navigation.spec.ts`: for each width, from `/home`, reach all six areas and all four children through the nav and landings alone; assert `h1`, selected area, `aria-current`, and `href`s; keyboard traversal and activation; focus on `h1` after link navigation and not after refresh or back; overflow check; the 3 × 2 grid at 390 px; direct load and refresh of the four child routes and two landings; no Safety API request from `/safety`. Captures to `e2e/captures/navigation/`.
- [x] `TASK-009` — **Verification runs.** Angular tests; the navigation, trip-audience, and invitation specs during work; one full `npm run test:e2e` at completion; `npm run build`.
- [x] `TASK-010` — **Rendered evidence and reviews.** Copy approved captures into `evidence/WI-009-003/` with a `README.md` and SHA-256; mockup comparison table (`VALIDATION.md` § 7); keyboard and screen-reader review (`VALIDATION.md` § 6, manual or code-based).
- [x] `TASK-011` — **Record and hand back.** Write `VER-WI-009-003.md`; set this item to `VERIFYING / OWNER REVIEW`. Update the implementation record here. *(Done 2026-10-02. The closure-time updates below wait for owner closure.)* At owner closure only: update `project-status.md`, `roadmap.md`, `ai-context/README.md`, and the `claude-handoff` state files.

## Expected files

| File | Change |
|---|---|
| `src/FamilySafety.Web/src/app/navigation.ts` | **New** — destinations and area table |
| `src/FamilySafety.Web/src/app/navigation.spec.ts` | **New** |
| `src/FamilySafety.Web/src/app/group-landing.ts` | **New** — static two-link landing (inline template; no component stylesheet expected) |
| `src/FamilySafety.Web/src/app/group-landing.spec.ts` | **New** (may be folded into `shell.spec.ts`) |
| `src/FamilySafety.Web/src/app/app.routes.ts` | Two routes |
| `src/FamilySafety.Web/src/app/app.ts` | View union, titles, mapping, `currentArea`, D4 focus, `RouterLink` import |
| `src/FamilySafety.Web/src/app/app.html` | Data-driven link nav; landing rendering; context-prompt condition excludes landings; `h1` `tabindex="-1"` |
| `src/FamilySafety.Web/src/styles.scss` | Shell nav rules (link styling, narrow grid) |
| `src/FamilySafety.Web/src/app/shell.spec.ts`, `home.spec.ts`, `first-use.spec.ts` (and `app.spec.ts` if affected) | Navigation expectations and test setup |
| `src/FamilySafety.Web/e2e/navigation.spec.ts` | **New** |
| `src/FamilySafety.Web/e2e/trip-audience-removal.spec.ts`, `e2e/invitation-handoff.spec.ts` | Locator only |
| `ai-context/planning/verification/evidence/WI-009-003/` | **New** — captures and `README.md` |
| `ai-context/planning/verification/VER-WI-009-003.md` | **New** |
| This work item | Task checkboxes and implementation record |

Not expected: `app.scss`, any feature component (`trips/`, `concerns/`, `incidents/`), `angular.json`, `package.json`, `e2e/fixtures/`, `playwright.config.ts`, `scripts/`, or any .NET project. Needing any of them is a stop condition, except a one-line selector adjustment in `app.scss` if the nav-item class moves, which must be recorded.

## Test plan summary

- **Unit / component (Vitest):** `areaForView` completeness; six links in order with `href`s; routed shell test for every route in `AC-08`; landing content and no-request assertion; D4 focus; updated existing specs.
- **Playwright (focused):** `e2e/navigation.spec.ts`, `trip-audience-removal.spec.ts`, and `invitation-handoff.spec.ts` during implementation.
- **Playwright (full):** `npm run test:e2e` once at completion, because the shared shell changes.
- **Build:** `npm run build`.
- **Not run:** .NET build and tests (no server change; state this in the VER record). No axe scan (M10).

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md`, `project-status.md` | Mandatory operating rules and checkpoint | Entry |
| This work item, especially [Owner decisions](#owner-decisions-2026-10-02) | Scope authority | Entry |
| `core/ux-contract.md#req-ux-013`, `#req-ux-011` | The navigation rule and validation standard; current-Family behavior to preserve | Before execution |
| `core/privacy-safety-contract.md#req-priv-027` | Navigation is not authorization | Before execution |
| `decision-log.md#dec-074`, `#dec-070` | OD-001 resolution; approved visual system | Before execution |
| `claude-handoff/WORKING-RULES.md`, `VALIDATION.md`, `SESSION-CLOSEOUT.md` | Execution rules, test cadence, artifact policy, closeout | Before execution |
| `claude-handoff/reference/detail-shell-topbar.png`, `panel4-safety-aware.png` | Shell and Safety-entry visual reference | Before execution |
| `src/FamilySafety.Web/src/app/` shell files and the specs listed above | Implementation surface | Before execution |
| `docs/local-development.md` § "Isolated Playwright scenarios" | Writing the new spec with `useIsolatedScenario` | If needed |
| `planning/proposals/PRP-030-…md` (M2, OD-001 section, exit criteria 2, 3, 11–14) | Background for the validation standard | If needed |

## Context intentionally not loaded

| Context | Why |
|---|---|
| `workflows/safety.md`, `REQ-SAFE-015/016`, `REQ-PRIV-025/026` in detail | No Concern or Incident behavior changes; their rules are preserved by not touching those components |
| Phase 8 stories and VER records | Closed history; only needed if Concern/Incident code is touched (it is not) |
| `REQ-UX-014` item-URL design, Event/Plan server contracts | M3, excluded |
| Bootstrap, archive, Phase 5–7.5 records | Not relevant |

## Stop conditions

Stop and report, without guessing, if execution would require:

- a new or changed API contract, server read, or authorization path;
- any change to Concern or Incident access, data loading, lifecycle, item URLs, or components beyond being navigated to;
- any Plan or Trip permission, audience, or data change;
- item-level URLs, single-item reads, Home item links, or cross-Family deep-link behavior;
- a navigation model other than the six areas in this order at every width (reopening `OD-001`), adding any destination, or a "More" menu;
- anything touching `OD-002` or Phase 9.5 capabilities;
- raising a budget threshold, or any new dependency (including `@axe-core/playwright`);
- extracting Home, Family, Events, Plans, or Me / Settings from `App`;
- a change to the meaning of an existing journey assertion, beyond the navigation, link-role, and current-area expectations listed in `TASK-007`;
- D4 focus behavior that can only be achieved through a global router side effect;
- test failures not attributable to this item.

## Risks

| Risk | Mitigation |
|---|---|
| Existing tests depend on nav **buttons** (`Plans`, `Trips`, `Family`, `Home`) | Known and listed (`TASK-007`); Playwright changes are locator only; component-test setup moves to the real routes where clicks are needed |
| `routerLink` in component specs configured with `provideRouter([])` | Use `provideRouter(routes)` or assert `href`s; record each setup change |
| Narrow labels wrap awkwardly or targets shrink | D2 grid with an explicit 44 px minimum; checked at 390 px and intermediate widths |
| Two-step access to Plans, Trips, Concerns, Incidents | Landings are minimal; Home shortcuts remain; D4 keeps keyboard cost low; owner review judges it |
| Safety landing copy implies something exists | Static, identical for all users; `AC-15` and owner review |
| D4 focus fires on refresh or back/forward because Angular restores history state | Require the state flag **and** an `imperative` trigger **and** a non-initial navigation; tested in `AC-12` |
| Initial bundle grows | Expected to be negligible (`RouterLink` is already in the router package); reported in `AC-21` |
| Confusing `currentArea` with access | Comment in `navigation.ts`; `AC-14`, `AC-15`, `AC-17` |

## Blockers and dependencies

- **Blockers:** none. `WI-009-001` and `WI-009-002` are `DONE`; the harness and artifact workflow are in place.
- **Later items that depend on this:** M10 (axe on primary routes and the two landings), M7 (first-run orientation may reference the six areas).

## DEC-032 review

- **In favor:** local, reversible, Angular-only, no privacy or permission effect, complete acceptance checks.
- **Against, and decisive:** `PRP-030` routes navigation work to `DEEP`; it implements a newly resolved product decision on the shared shell; it requires owner visual approval; and D1–D6 are owner UX choices.

**Conclusion:** `DEC-032` does not apply. Explicit owner approval and execution authorization were required, and were given on 2026-10-02 (below).

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES` — `APPROVED — EXECUTION AUTHORIZED` against `CTX-1.27`
- **Authorized by:** Project owner
- **Authorized at:** 2026-10-02
- **Authorized scope:** exactly this work item's design, [Owner decisions](#owner-decisions-2026-10-02) D1–D6 and the further owner direction, 23 acceptance checks, and 11 tasks.
  - **Included:** Angular shell, routes, navigation definition, landing component, shell styles in `styles.scss`, component tests and their setup, the new Playwright navigation spec, locator-only changes in `trip-audience-removal.spec.ts` and `invitation-handoff.spec.ts`, rendered evidence, and the verification record.
  - **Excluded:** everything under [Explicitly excluded](#explicitly-excluded), including any server, API, authorization, schema, or dependency change; `REQ-UX-014` item routing; Home hub changes; `App` extraction; `@axe-core/playwright`; `OD-002`.
  - **Process:** no further routine approval is needed; the stop conditions still apply. Hand back in `VERIFYING / OWNER REVIEW`; do not mark `DONE / PASS`. Do not commit or push.
- **Implementation started:** `YES` (2026-10-02); handed back `VERIFYING / OWNER REVIEW` the same day.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| AC-01 – AC-22 | `VER-WI-009-003.md`; `evidence/WI-009-003/` | `PASS` (self-verified; owner accepted) |
| AC-23 owner visual approval | Owner visual review, 2026-10-02 | `PASS` |

- **Verification report:** `planning/verification/VER-WI-009-003.md` (`DONE / PASS` — `OWNER APPROVED`)
- **Independence:** `SELF-VERIFIED` expected, plus owner visual review
- **Outcome:** `DONE / PASS` — `OWNER APPROVED` (2026-10-02). AC-01–AC-23 `PASS`.

## Implementation record (2026-10-02)

- **Result:** implemented within the approved boundary and handed back `VERIFYING / OWNER REVIEW`. Full evidence is in `planning/verification/VER-WI-009-003.md` and `planning/verification/evidence/WI-009-003/`. Independence: `SELF-VERIFIED`.
- **Baseline (TASK-001):** 8 files and 118 tests passed. Initial bundle 536.44 kB, with 3 existing budget warnings. Ports 4200 and 5080 free. No retained databases.
- **Final results:**
  - Angular: 10 files, 152 tests passed.
  - Focused Playwright (navigation, trip-audience, invitation): 9/9.
  - One full `npm run test:e2e`: 12/12. 5 databases created, 5 removed, 0 retained; port 5080 free.
  - `npm run build`: succeeds with the same 3 warnings. Initial bundle 549.21 kB (+12.77 kB, mostly `RouterLink`). No threshold change and no new dependency.
- **Files:**
  - New: `navigation.ts`, `navigation.spec.ts`, `group-landing.ts`, `group-landing.spec.ts`, `e2e/navigation.spec.ts`.
  - Modified: `app.routes.ts`, `app.ts`, `app.html`, `styles.scss`, `shell.spec.ts`, `home.spec.ts`, `first-use.spec.ts`, `app.spec.ts`.
  - Locator only: `e2e/trip-audience-removal.spec.ts`, `e2e/invitation-handoff.spec.ts`.
  - No server, API, schema, authorization, feature-component, `app.scss`, configuration, or fixture change.
- **Deviations (recorded in the VER):**
  1. D4 reads the creating navigation when the router constructs `App`, rather than `lastSuccessfulNavigation()` in `afterNextRender`. The conditions are unchanged.
  2. Small token-only landing and `h1` focus rules were added to `styles.scss`.
  3. The `page-content` wrapper is hidden on the landings, as it is on Home.
  4. An intermediate-width Playwright test was added.
  5. Between 44 rem and 64 rem the links form 2 rows × 3 columns (D2 allows this).
- **Closed:** owner visual approval (AC-23) `PASS` on 2026-10-02; see [Owner closure](#owner-closure-2026-10-02). The TASK-011 closure-time updates (`project-status.md`, `roadmap.md`, `ai-context/README.md`, `claude-handoff` state files) were made at closure.

## Owner closure (2026-10-02)

- **Result:** `DONE / PASS` — `OWNER APPROVED`. Owner review result: `PASS`, dated 2026-10-02.
- **AC-23 — `PASS`.** The owner accepted the rendered implementation at 1440, 1024, 768, 600, and 390 px, and the six-area structure in this order: Home, Family, Events, Plans & Trips, Safety, Me / Settings. Also accepted: the desktop sidebar, slim top bar, 3 × 2 narrow navigation grid, Plans & Trips landing, Safety landing, semantic navigation links, current-area styling, and heading focus behavior.
- **Accepted evidence:** AC-01–AC-22 `PASS` on self-verification; Angular 152 passed; navigation Playwright 7/7; Trip-audience and invitation Playwright passed; full suite 12/12; production build passed; no API, server, or schema change; no authorization behavior change; no existing evidence overwritten; rendered evidence at the approved widths.
- **Accepted deviations (non-blocking; not to be reopened):**
  1. The focus logic reads the router navigation available when `App` is created, rather than `lastSuccessfulNavigation()`.
  2. Small landing and focus styles were added to `styles.scss`.
  3. The page-content wrapper is hidden on the two grouping landings, as on Home.
  4. An intermediate-width navigation test was added.
  5. Between about 704 and 1024 px the navigation uses 2 rows × 3 columns.
- **Bundle growth:** initial bundle 536.44 kB → 549.21 kB (+12.77 kB) accepted as non-blocking and **monitoring-only**, unless later Phase 9 work shows a real performance issue. No budget threshold was raised; the build passes. No optimization work is created from this item.
- **Accepted non-blocking issues (no new work item):** Plans, Trips, Concerns, and Incidents need one grouping step from the main navigation; the Home shortcuts remain; transient captures stay removable through `scripts/test-artifacts.ps1`; the pre-existing interrupted run record with no database stays untouched.
- **Tests:** not rerun for closeout.
- **Post-review cleanup:** `scripts/test-artifacts.ps1` (read-only report, then a dry run, then `-ClearGenerated -Execute`) cleared only the transient `e2e/captures/` (97 files, 11.9 MB) and `e2e/test-results/` (45 B) folders. No database, run record, interrupted run record, canonical evidence, or the root `trips` file was touched; `evidence/WI-009-003/` is intact (24 files).
- **Source control:** nothing committed or pushed; all changes are uncommitted for owner review.
