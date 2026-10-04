# PRP-030 — Phase 9 Scope, Themes and Exit Criteria

**Closure disposition (2026-10-03):** Phase 9 is `COMPLETE / CLOSED` under [DEC-081](../../decision-log.md). [VER-PH-009](../verification/VER-PH-009.md) maps the final exit criteria, including approved substitutions and later story decisions that revised specific implementation or presentation details. This proposal remains historical governance, not current implementation authorization; its approved criteria below are preserved as written.

## Metadata

- **Proposal ID:** `PRP-030`
- **Status:** `APPROVED` (owner decisions D1–D4 under `DEC-074`, 2026-10-02; canonical rules in `CTX-1.27`)
- **Date:** 2026-10-02 (proposed); approved 2026-10-02
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.27` (active). Drafted against `CTX-1.26`.
- **Required context:**
  - `ai-context/README.md` and `project-status.md`: operating rules and current checkpoint.
  - `planning/roadmap.md` and `decision-log.md` (`DEC-062`, `DEC-064`, `DEC-070`, `DEC-072`, `DEC-073`, open decisions): the Phase 9 boundary, the Phase 9.5 assignment, the current visual system, and `OD-001` / `OD-002`.
  - `core/ux-contract.md` (`REQ-UX-002`, `003`, `006`, `007`, `008`, `010`, `011`): Home purpose and hierarchy, accessibility, conceptual navigation areas, and current Family context.
  - `core/product-contract.md` (conceptual product areas): the six conceptual areas behind `OD-001`.
  - `planning/verification/VER-PH-008.md` and `VER-WI-008-003.md` (follow-up sections only): the carried-forward technical follow-ups.
  - `src/FamilySafety.Web/src/app/` and `e2e/`: the current implementation the proposal is measured against (read-only).
- **Affected authority:** Phase 9 scope, themes, and exit criteria. The owner decisions on this proposal resolved `OD-001` and set the deep-link Family-context rule; those are canonical in `CTX-1.27` as `REQ-UX-011` (amended), `REQ-UX-013`, `REQ-UX-014`, and `REQ-PRIV-027`. Canonical wording controls wherever it differs from this proposal.
- **Model tier:** `STANDARD` for the phase plan. Individual items that touch navigation (`OD-001`), server reads, or authorization-adjacent routing route to `DEEP` in their own work items.

## Request

Define what Phase 9 — Integration & UX Refinement must accomplish, and how its completion will be measured, before any Phase 9 work item is created.

This proposal is the approved Phase 9 planning basis. It creates no work item by itself and authorizes no code, test, or schema change; each work item needs its own approval and execution authorization.

## Owner decisions (DEC-074, 2026-10-02)

- **D1 — approved:** this proposal's objective, themes, MUST / SHOULD / LATER classification, Phase 9 boundary, Phase 9.5 placement, exit criteria (as updated below), and the test-isolation-first approach.
- **D2 — `OD-001` resolved, Option A:** six primary areas — Home, Family, Events, Plans & Trips, Safety, Me / Settings — at desktop and narrow widths. "Plans & Trips" and "Safety" are navigation groupings only. The four-part validation standard is approved. Canonical: `REQ-UX-013`.
- **D3 — `@axe-core/playwright` approved** as a development-only test dependency. It supplements, and never replaces, keyboard, focus, semantic, owner visual, and targeted manual review. No other accessibility package and no runtime dependency.
- **D4 — deep-link Family context:** server authorization of the item first; only then switch the selected Family to the item's Family and show the item; otherwise the generic unavailable state with no Family change and no disclosure. Canonical: `REQ-UX-014`, `REQ-PRIV-027`.

## Phase boundary (from DEC-073)

- Phase 9 is **integration and UX refinement of what is already implemented**. It is not feature completion.
- The approved but unimplemented MVP capabilities are assigned to **Phase 9.5 — MVP Feature Completion**, after Phase 9 and before Phase 10 — Testing & Verification. They remain in the MVP. They are: Event RSVP, Event Needs, Event Updates, Check-In, Sharing Sessions, Trusted Contacts, Shared Family Dates, Safety Circles, Household grouping, and notifications.
- Phase 9 must not build, partially build, or visually imply any of those capabilities.

## Phase 9 objective

Make the implemented MVP work as one connected family application. A first-time invited adult and a returning member can:

- find every implemented area;
- move from Home to the specific item that needs their attention;
- move between related Family, Event, Plan, Trip, Concern, and Incident information without rediscovering it;
- use the application reliably at desktop and narrow widths;

and the project can rely on an end-to-end test suite that runs consistently in one pass.

## Current evidence

This summarizes the Phase 9 assessment of 2026-10-02 (static review of records and code; no rendered walkthrough was performed).

| Area | Current state |
|---|---|
| Navigation | Desktop sidebar: Home, Family, Events, Plans, Trips, Me / Settings. Narrow widths: the same buttons in a wrapping row. Concerns (`/concerns`) and Safety Incidents (`/incidents`) have **no navigation entry** and are reachable only from the Home "Safety coordination" card. `REQ-UX-011` and the product contract name six conceptual areas (Home, Family, Events, Plans & Trips, Safety, Me / Settings). The current six buttons are a different set: Plans and Trips are split and Safety is missing. |
| Item URLs | Only Incidents have item URLs (`/incidents/:incidentId`). Events, Plans, and Trips have none. A Concern opens only through router navigation state, so a page refresh loses it. |
| Home | Shows the top three Events, Plans, and Trips, but **the rows do not open anything**. Events are filtered to today and later; Plans and Trips are **not** filtered, so past items can appear. The Safety card is two static buttons and shows no state. "Needs your attention" always repeats a "View Family" row. |
| First use | After an invitation is accepted, profile review continues to **Family**, not Home. There is no first-run orientation beyond an empty-state card. A one-person Family cannot create an Event (at least one other adult is required), which reads as a dead end. |
| Family / admin | The directory API (`FamilyDirectoryEntry`) returns only profile ID and display name, so **administrator role cannot be shown without a server read change**. Invitations are a single email field with "Send or resend" and "Revoke"; revoking requires retyping the email; there is no invitation status list. The "administrator does not grant private access" message appears three times. |
| Consistency | Five create patterns (side form for Events and Plans; header button and panel for Trips; inline section for Concerns; a `/new` route for Incidents). Edit exists for Events (inline) and Trips (panel), not Plans. List and audience headings, button capitalization, loading copy, and secondary-button class names (`secondary-button` ×2 vs `button--secondary` ×46) vary. |
| Root component | `app.ts` (≈ 1,409 lines) and `app.html` (≈ 1,109 lines) still host Home, Family, Events, Plans, Me / Settings, and the invitation flow. Trips, Concerns, and Incidents are already separate components. |
| Tests | Playwright uses one scenario database per run (`e2e/playwright.config.ts`), shared by all four specs. The Concern journey adds Adult B, so `invitation-handoff` then fails its precondition in a full run. The config also branches on which spec files are named (`isInvitationOnlyRun`). Every spec passes on its own. No automated accessibility scanner is configured. |
| Safety list APIs | `GET /api/concerns` and `GET /api/incidents` already return `State` and `FamilyGroupId` for items the caller is authorized to see, so a Home Safety summary needs no server change. |
| Budgets | Initial bundle ≈ 536 kB (500 kB warning, 1 MB error). `concerns.scss` ≈ 7.53 kB compiled (8 kB error). `trips.scss` ≈ 5.8 kB (4 kB warning). |

## Themes

1. **Test reliability foundation.** Isolated, reproducible state for every Playwright spec, so cross-feature journeys can be trusted.
2. **Navigation and discoverability.** Resolve `OD-001` early and implement it, so every implemented area, including Safety, is reachable at desktop and narrow widths.
3. **Home as the application hub.** Rows open items, Safety reflects state the user is already authorized to see, filtering is consistent, and dead-end actions are removed.
4. **Item-level routing and cross-feature continuity.** Refresh-safe URLs for every item type, so Home, lists, and related features can link directly.
5. **First-use and returning-user refinement.** A deliberate landing after invitation, an understandable first Home, and a verified multi-Family return.
6. **UI terminology and pattern consistency.** One vocabulary and one set of patterns for create, edit, detail, audience, empty, unavailable, and error states.
7. **Accessibility and responsive verification.** Keyboard, screen-reader semantics, and narrow-width behavior verified across primary routes.
8. **Selected technical correctness follow-ups.** Only where needed for Phase 9 or for predeployment correctness.

## Candidate work

### MUST (needed for the Phase 9 objective)

| # | Item | Theme | Notes |
|---|---|---|---|
| M1 | Playwright full-suite isolation | 1 | First work item; see below. |
| M2 | Implement the six-area navigation (`REQ-UX-013`) | 2 | `OD-001` is resolved. Includes the Plans & Trips and Safety groupings, both parts reachable, at desktop and narrow widths, validated by the four-part standard. |
| M3 | Refresh-safe item URLs for Events, Plans, Trips, and Concerns, with the authorized deep-link Family switch (`REQ-UX-014`, `REQ-PRIV-027`) | 4 | Incidents already have item URLs. An unauthorized or missing item URL shows the generic unavailable state and leaves the Family unchanged. **Server finding:** Events and Plans have only Family-scoped list endpoints (`/api/family-groups/{familyGroupId}/events` and `/plans`), so they need additive single-item reads under their existing authorization, returning the same nondisclosing result for missing and unauthorized items. Trips (`GET /api/trips/{tripId}`), Concerns, and Incidents already have single-item reads; their nondisclosure must be confirmed in that work item. This makes M3 a server-and-UI item (`DEEP`). |
| M4 | Home rows open the specific item | 3 | Depends on M3. |
| M5 | Home Safety summary from existing authorized list APIs | 3 | For example, counts of Open Concerns and Incidents in the current Family. No new read path, no urgency language, no notifications. |
| M6 | Consistent past-item filtering on Home | 3 | Plans and Trips follow the same "today and later" rule as Events. A Trip in progress is treated as current. |
| M7 | Post-invitation landing and first-run orientation | 5 | Land on Home or a deliberate orientation step, not Family. |
| M8 | Terminology and pattern consistency pass | 6 | Driven by a written checklist (exit criterion 7). |
| M9 | Cross-feature end-to-end journeys | 1, 4, 5 | The invited-adult and multi-Family journeys in the exit criteria. |
| M10 | Accessibility verification across primary routes | 7 | `@axe-core/playwright` route checks (D3) plus keyboard, focus, and semantic review. |

### SHOULD (important, but not a Phase 9 exit requirement)

| # | Item | Notes |
|---|---|---|
| S1 | Extract routed feature components from `app.ts` / `app.html` where needed | Required in practice for M3 on Events and Plans; see the decomposition recommendation. |
| S2 | Concern repository stale-tracking fix | Apply the proven Incident pattern (`ForgetTrackedAggregate`). Also a **predeployment correctness item**: if not done in Phase 9, it must be done before deployment. Server change; needs its own work item. |
| S3 | Show administrators in the Family member directory | **Not available from existing data, and currently excluded by decision.** The directory contract has no role field, and `DEC-033` says the directory must not expose administrator assignments. This needs an owner decision revisiting `DEC-033` before any server read change. |
| S4 | Invitation-status proposal and administrator read path | Needs a new server read. Proposal first; do not implement silently. |
| S5 | Split `concerns.scss` before any further Concern UI work | Only if a Phase 9 item changes Concern UI. |
| S6 | Friendlier one-person-Family Event state | Copy and layout only; the "at least one other adult" rule is unchanged. |

### LATER / out of Phase 9

- All Phase 9.5 capabilities: Event RSVP, Needs, Updates; Check-In; Sharing Sessions; Trusted Contacts; Shared Family Dates; Safety Circles; Household grouping; notifications.
- `OD-002` (assigned to Phase 9.5 with Sharing Sessions and Check-In).
- Production sign-in and account registration; hosting; production database.
- Backup-aging and restore implementation and evidence (predeployment, Phase 11).
- Formal participant usability study.
- Minor and guardian functionality (`OD-003`, outside MVP).
- Member removal, administrator transfer, leaving a Family, self-service Family creation.
- "My Tasks" on Home, unless separately proposed.
- Generalized bundle or performance optimization.
- Me / Settings expansion, unless a Phase 9 continuity item needs it.
- Concern display-name projection and Concern reconciliation efficiency.

## OD-001 — navigation decision area (resolved: Option A)

**Resolved under `DEC-074` with Option A; canonical in `REQ-UX-013`.** The options below are kept as the decision record.

### Option A — six primary areas: merge Plans & Trips, add Safety

- **Destinations:** Home, Family, Events, Plans & Trips, Safety, Me / Settings, at every width.
- **Benefits:** Matches `REQ-UX-011` and the product contract's six conceptual areas exactly. Same set on desktop and mobile, so there is one mental model. Safety becomes a first-class area holding Concerns and Incidents.
- **Risks:** "Plans & Trips" needs a combined landing page or a clear two-tab page; Plans and Trips are separate records (`DEC-057`) and must stay separate in behavior. Six items is tight on a phone bottom bar.

### Option B — seven desktop destinations, smaller mobile set with "More"

- **Destinations:** Desktop: Home, Family, Events, Plans, Trips, Safety, Me / Settings. Mobile: Home, Events, Plans, Safety, More (Family, Trips, Me / Settings).
- **Benefits:** No change to the current Plans and Trips pages. Small mobile bar.
- **Risks:** Desktop and mobile differ, so some areas are one tap deeper on mobile. Seven destinations departs from the six conceptual areas; the mobile grouping must still be justified against `REQ-UX-010` (minimal-interaction mobile actions).

### Option C — six desktop areas (Option A), five on mobile

- **Destinations:** Desktop as Option A. Mobile: Home, Events, Plans & Trips, Safety, and Me (Family reached from Me and from Home).
- **Benefits:** Same areas as Option A, with a five-item mobile bar that fits narrow screens comfortably. Family is already prominent on Home.
- **Risks:** Family is one step deeper on mobile. Mobile and desktop differ slightly.

### Recommendation

**Option A**, or Option C if a six-item bar does not fit acceptably at 390 px. Both match the canonical six areas, give Safety a real entry, and avoid a "More" menu. Option B keeps more of the current layout but moves away from the canonical areas.

**Owner decision:** Option A, at desktop and narrow widths. The navigation work item must make the six areas fit at narrow width; switching to Option C would need a new decision.

### Validation standard for OD-001 in the MVP (approved)

The decision record constrained `OD-001` with "requires UX validation", and formal usability testing is deferred (Phase 7.5 owner direction). The owner approved the following, together, as sufficient MVP validation (`REQ-UX-013`):

1. **Owner visual review** of the chosen navigation at desktop (1440 px) and narrow (390 px) widths.
2. **Rendered evidence** at both widths for every destination, filed in the work item's evidence folder.
3. **Keyboard and screen-reader verification:** every destination is reachable by keyboard in a logical order, the active destination is announced (`aria-current`), and focus is visible.
4. **Playwright route and discoverability tests:** from Home, each implemented area (including Concerns and Incidents) is reached through main navigation alone at both widths.

None of these alone is enough; all four are required. `@axe-core/playwright` may supplement them but does not replace the keyboard and manual review. A formal participant study stays a later maturity gate and may revisit the decision.

## OD-002 placement

`OD-002` stays **out of Phase 9**. It belongs to Phase 9.5, alongside Sharing Sessions and Check-In, because persistent sharing can only be judged once temporary sharing exists. Phase 9 copy must continue not to imply that location sharing exists.

## First work item recommendation

**`WI-009-001 — Playwright Per-Spec Scenario Isolation`** is the first Phase 9 implementation work item. It was prepared after approval (`planning/stories/WI-009-001-playwright-per-spec-scenario-isolation.md`) and awaits owner execution authorization.

- **Why first:** the full suite has an ordering dependence today; Phase 9 depends on cross-feature end-to-end journeys; every later Phase 9 change is verified through this suite.
- **Intent:** every spec runs from isolated, reproducible state (for example, its own scenario database or a reset fixture), and the spec-specific `isInvitationOnlyRun` branching is removed.
- **Boundary:** test infrastructure only (`e2e/`, the scenario script, and fixtures). No application, API, or schema change. The safety checks on scenario database names must be kept.
- **Acceptance direction:** the full suite passes three consecutive times from a clean start; each spec still passes alone.
- It does not depend on the navigation work, so planning for M2 can proceed in parallel.
- `DEC-032` standing authorization does **not** apply; see the work item for the reasoning.

## Root component decomposition

Decompose `app.ts` / `app.html` **only where it directly enables Phase 9 work**, not for cleanliness alone.

- Extract a feature only when a Phase 9 item needs it: Events and Plans for item URLs (M3) and Home links (M4); Home for the hub work (M4–M7); Family when the Family/admin clarity work touches it.
- Extract **incrementally, one feature per work item or as the first task of the item that needs it**, with existing tests and Playwright journeys passing before and after.
- Lazy loading and a smaller initial bundle are welcome side effects, not goals.
- The shell, current-Family context (`DEC-064`), and invitation flow stay in the root until a work item needs to move them.
- No behavior, copy, or visual change as part of an extraction step.

## Home targets

- Event, Plan, and Trip rows open the specific item through its item URL (after M3).
- The Safety card reflects accessible Concern and Incident state using the existing list APIs only.
- Past Plans and Trips are filtered on the same rule as Events.
- A clearer first-run state that explains what each area is for and how to start.
- Remove redundant or dead-end actions, such as the always-present "View Family" row and the repeated "Open Events / Plans / Trips" buttons.
- No "My Tasks", activity feed, or notification behavior unless separately proposed.

## First-use targets

- After accepting an invitation and reviewing the profile, land on Home or a deliberate orientation step, not Family.
- The first Home view explains the product well enough for the member to continue without help.
- No new signup or authentication scope.
- The Development identity switcher remains a Development environment constraint, not a Phase 9 product feature. Journeys may use it as the test identity mechanism.

## Family and administration targets

Using existing data only:

- Reduce the repeated "administrator does not grant private access" wording to one clear statement.
- Improve page composition so the administrator panel is easy to find for administrators and absent for others (as today).

Needing a proposal or decision first:

- Showing which members are administrators (S3): `DEC-033` currently excludes administrator assignments from the directory, so this needs an owner decision revisiting it, then a server read.
- Listing invitation status (S4): needs a new administrator-only server read.

## Technical follow-ups

| Follow-up | Classification |
|---|---|
| Concern stale-tracking risk | Phase 9 SHOULD; **predeployment correctness item** |
| Concern display-name projection | Later technical debt |
| Concern reconciliation efficiency | Later; review before the pilot |
| Initial bundle warning | Monitor only |
| `concerns.scss` near the 8 kB error budget | Split only if a Phase 9 item changes Concern UI |
| `trips.scss` warning | Monitor only |

No budget threshold may be raised.

## Deployment readiness carry-forward

- Backup-aging and restore evidence (`DEC-068`, `DEC-071`) stays **outside Phase 9**. It is carried to the predeployment stage and should be an explicit Phase 11 — Pilot & Release Readiness requirement.
- Production authentication remains unresolved and outside Phase 9.
- Phase 9 completion will not mean deployment readiness.

## Phase 9 exit criteria (approved)

1. **Test reliability:** the full Playwright suite passes in one command, three consecutive times from a clean start. Each spec runs on isolated scenario state, results do not depend on spec order, and the configuration has no spec-specific branching.
2. **Navigation model:** the six-area navigation of `REQ-UX-013` (Home, Family, Events, Plans & Trips, Safety, Me / Settings) is implemented at 1440 px and 390 px and validated by all four parts of the approved standard: owner visual review, rendered evidence at both widths, keyboard and navigation review, and automated reachability tests.
3. **Groupings reach every part:** an automated test at both widths reaches Plans and Trips through Plans & Trips, and Concerns and Incidents through Safety, using main navigation alone. Direct routes for all four still load after a page refresh. Reaching Safety shows no Concern or Incident the user is not authorized to see.
4. **Item URLs:** every Event, Plan, Trip, Concern, and Incident that appears in a list or on Home opens through a URL that survives a page refresh (`REQ-UX-014`).
5. **Authorized cross-Family deep link:** an automated test opens an item URL belonging to the user's other Family and confirms that the item renders, the selected Family switches only after the server returns the item, and Family-scoped state from the previous Family is cleared.
6. **Generic unavailable behavior:** automated tests confirm that, for each of the five item types, a nonexistent item and an item from a Family the user does not belong to (or an item they are not in the audience of) give the same unavailable state, leave the selected Family unchanged, and show no Family name (`REQ-PRIV-027`). The server responses for both cases are indistinguishable.
7. **Invited-adult journey:** a Playwright journey runs invitation → accept → profile review → Home → open an existing item from Home → create a Plan, a Trip, and a Concern → start an Incident from that Concern, using main navigation and in-app links only, with no direct URL entry and no developer steps beyond selecting the Development identity.
8. **Returning multi-Family journey:** a Playwright journey verifies that a member of two Families must choose a Family, sees only that Family's items, and sees no prior-Family data after switching (`REQ-UX-011`).
9. **Home:** Home shows only current or upcoming Events, Plans, and Trips under one rule; its rows open the specific item; it summarizes Concern and Incident state from existing authorized APIs only; and it has no redundant or dead-end action. Covered by component tests.
10. **Consistency:** a written product-wide checklist (create, edit, detail, list headings, audience wording, empty, unavailable, and error states, button hierarchy and capitalization, date and time format) is filed with evidence, with every item done or explicitly accepted by the owner.
11. **No dead ends:** every primary page, grouping landing, and item detail has a visible way forward and a way back, recorded in the checklist.
12. **Accessibility:** `@axe-core/playwright` checks run on every primary route and grouping landing with no serious or critical violations (any accepted exception is recorded with a reason), **and** a manual review passes: keyboard-only completion of the journeys in criteria 7 and 8, visible focus, correct landmarks and headings, and `aria-current` on navigation.
13. **Owner visual review** passes at desktop and narrow widths for every changed screen.
14. **Authorization preserved:** the existing Concern, Incident, Trip-audience, and invitation journeys and the server denied-access tests still pass. No Phase 9 item adds an authorization path, and no grouping merges domain authorization, data, or lifecycle.
15. **Build:** the production build passes without raising any budget threshold, and `@axe-core/playwright` (if installed) is a development dependency only.

## Decisions (resolved)

- **D1 — PRP-030:** approved (`DEC-074`).
- **D2 — `OD-001`:** Option A and the four-part validation standard (`DEC-074`, `REQ-UX-013`).
- **D3 — accessibility tooling:** `@axe-core/playwright`, development-only (`DEC-074`).
- **D4 — cross-Family item URLs:** server authorization first, then Family switch; otherwise generic unavailable and no change (`DEC-074`, `REQ-UX-014`, `REQ-PRIV-027`).

## Conflicts and unknowns

- **Conflicts:** none between authoritative sources. `DEC-033` constrains S3 (see above); it is a recorded constraint, not a conflict.
- **Unknowns:** how the six areas fit at 390 px (resolved in the navigation work item under Option A); the exact single-item read design for Events and Plans (decided in the M3 work item).

## Verification plan

- Each Phase 9 work item carries its own acceptance criteria and verification report, as in earlier phases.
- A Phase 9 closure review (`VER-PH-009`) maps every approved exit criterion to evidence.
- Owner visual review for every UI change; `SELF-VERIFIED` labeling where no independent review is available.

## Decision and authorization

- **Decision:** `ACCEPTED`
- **Decision ID:** `DEC-074`
- **Approved by:** Project owner
- **Approved at:** 2026-10-02
- **Execution authorized:** `NO`. Approval of this proposal authorizes no code, test, dependency, or schema change. Each Phase 9 work item needs its own approval and execution authorization.
