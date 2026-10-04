---
Status: DONE / PASS — OWNER APPROVED
Authority: Project-owner authorization (2026-10-03) after the Family design review and Claude previews; owner decisions D1–D4
Context-Baseline: CTX-1.28
Version: 1.0
---

# WI-009-007 — Family Dashboard Prototype Alignment

## Metadata

- **Work Item ID:** `WI-009-007`
- **Status:** `DONE / PASS` — `OWNER APPROVED` (owner review 2026-10-03; `VER-WI-009-007`; `DEC-077`). AC-01 to AC-10 `PASS`.
  - Status history:
    1. `IMPLEMENTED / OWNER VISUAL REVIEW`.
    2. Owner visual approval `PASS`.
    3. `VERIFYING / FINAL REGRESSION`.
    4. `DONE / PASS`.
  - All on 2026-10-03.
- **Phase:** `9 — Integration & UX Refinement`
- **Context baseline:** `CTX-1.28` (no requirement change)
- **Planning basis:** `WI-009-006` / `DEC-076` (Home dashboard; honest placeholders), `DEC-070` visual direction.
- **Visual target:** `ai-context/references/ux/home-dashboard/home-dashboard-overview.png`, **panel ② "Family Activity and Coordination"** only (larger copy: `claude-handoff/reference/panel2-family-activity.png`). The owner-reviewed Claude previews ("Family Dashboard Previews", six artboards) are the immediate implementation reference. UX mockups are non-authoritative design evidence.
- **Requirement references (preserved, not changed):** `REQ-UX-011`, `REQ-UX-013`, `REQ-PRIV-013`, `REQ-PRIV-014`, `REQ-PRIV-015`, `REQ-PRIV-027` (navigation is not authorization), `DEC-033` / `PRP-008` (directory shows profile ID and display name only; no administrator assignments or capacity).

## Goal

`/family` becomes a day-to-day Family coordination dashboard in the panel ② composition. Family management moves to `/family/settings`. No existing Family function is lost, and authorization and privacy behavior are unchanged.

## Owner decisions (2026-10-03)

- **D1 — Placeholders: approved.** Polished static placeholders for valuable panel ② sections that are not implemented. They have no fake data, no controls, and no requests, they don't imply the capability exists, and they don't mention phases or work items.
- **D2 — "Waiting for you": approved**, in place of "Pending Invitations". It is the dashboard's actionable area. It shows only real actionable items supported today, with a compact empty state otherwise.
- **D3 — Page title "Family": approved.** No administrative title.
- **D4 — Settings gear: approved, with an authorization constraint.** Every member sees the gear (accessible name "Family settings", keyboard reachable, navigates to `/family/settings`). The gear is navigation only. Settings shows each member only the functions they may currently use.

## Final owner decisions (2026-10-03)

- **D5:** keep the tile relationship note, shown only for an accepted relationship between that member and the signed-in adult. There is no fallback label (the "You" line was removed) and no administrator status.
- **D6:** the date is hidden at about 390 px, as on Home.
- **D7:** placeholder copy:
  - "Birthdays shared with your family will appear here."
  - "Family updates will appear here as activity is shared."
  - "Recent family check-ins will appear here when available."

## Scope

- **Routes:**
  - `/family` is the dashboard.
  - `/family/settings` is new (its own route definition, view `familySettings`). Family stays the selected primary area on both: `aria-current="page"` on `/family` and `"true"` on Settings.
- **Dashboard:**
  - **Header:** title "Family", the Family name in the subtitle, the date (hidden below 44 rem by the shell, as on Home), and the gear.
  - **Members card:** the existing filter, compact member tiles (initials avatar and name), a second line reading "You" or the member's side of an accepted relationship with the signed-in adult, and the selection state.
  - **Member detail:** existing fields only, plus accepted relationships that include the member. These are already visible to every active adult.
  - **2 × 2 coordination grid:**
    - Waiting for you: incoming relationship suggestions with Accept and Decline, a "Manage" link to Settings, and the empty state "Nothing is waiting for you right now."
    - Upcoming birthdays: placeholder.
    - Recent Family Updates: placeholder.
    - Recent Check-Ins: placeholder.
- **Settings:**
  - Back to Family link.
  - **Family details** (read-only): Family name and membership status, the existing membership and administrator privacy statements, and a link to the profile sharing choices (`/me`).
  - **Invite an adult family member:** unchanged, and rendered only when `canAdministerCurrentFamily()`.
  - **Family relationships:**
    - suggest a relationship;
    - grouped lists for Waiting for you, Sent by you (withdraw), and Accepted relationships (remove with confirmation, for the two adults only);
    - the existing empty state.
- **Responsive layout:**
  - **≥ 80 rem:** members 2fr and detail 1fr side by side.
  - **64–80 rem:** sidebar, members full width, detail as a horizontal panel.
  - **44–64 rem:** top navigation, with members and detail stacked.
  - **Coordination grid:** two columns from 44 rem.
  - **Below 44 rem:** one column, with the gear beside the heading.
  - No horizontal overflow.

### Birthdays decision

Birthday month and day exist only on an individual shared profile (`/api/family-members/{id}/profile`), subject to each member's visibility choice. There is no Family-level birthday or date listing. Shared Family Dates (`REQ-FAM-010`) is approved but not implemented. Building an "upcoming" list would mean reading every member's profile and implementing `REQ-FAM-010` behavior without its rules, so the card stays a placeholder.

### Waiting for you decision

The only currently implemented actionable Family items for the signed-in adult are relationship suggestions sent to them. Invitation responses happen only through the invitation link, and there is no endpoint for listing invitations.

## Explicit exclusions

- Check-Ins, activity feed, notifications, Tasks, Sharing Sessions, location, presence, RSVP, Household grouping, Safety Circles, and any Phase 9.5 feature.
- Any API, endpoint, schema, migration, authorization, or server change, and any new request. The dashboard uses only the existing Family, directory, profile, relationships, and administered-Family reads.
- Showing administrator assignments in the directory, or any private or hidden member information.
- Birthday or date aggregation.
- Home changes, and any shell or navigation change beyond the new Family child route.

## Acceptance checks

1. `AC-01` The `/family` 1440 composition matches panel ② as adapted in the approved previews.
2. `AC-02` Every existing Family function remains reachable:
   - filter and clear the directory;
   - select a member and view their details;
   - suggest, accept, decline, withdraw, and remove a relationship;
   - send or resend and revoke an invitation (administrator only).
3. `AC-03` The gear is visible to every member, has the accessible name "Family settings", is keyboard reachable, and moves focus to the Settings `h1`. Back to Family returns to `/family` the same way.
4. `AC-04` A member who is not an administrator sees no Invite section on Settings. Server authorization is unchanged.
5. `AC-05` Placeholders contain only their heading and sentence: no button, link, input, image, list, digit, or badge, and no request.
6. `AC-06` Waiting for you shows only incoming relationship suggestions, or the one-line empty state.
7. `AC-07` 1440, 1024, 768, and 390 px follow the Scope layout with no horizontal overflow.
8. `AC-08` (after visual approval) Focused Angular specs updated and passing. `npm run build` passes with no new budget warning.
9. `AC-09` (after visual approval) Final regression, including affected Playwright specs.
10. `AC-10` Owner visual approval.

## Implementation record (2026-10-03)

- **Files:**
  - `src/FamilySafety.Web/src/app/app.routes.ts`: `family/settings` route.
  - `app/navigation.ts`: `familySettings` view, area `family`, path `family/settings`.
  - `app/icon.ts`: decorative `settings`, `info`, and `back` icons.
  - `app/app.ts`:
    - titles "Family" and "Family settings" and their subtitles;
    - `family-settings.scss` added to `styleUrls`;
    - `incomingRelationshipSuggestions`, `outgoingRelationshipSuggestions`, `acceptedRelationships`, `acceptedRelationshipsFor()`, and `memberTileNote()`, which are presentation groupings of data already loaded.
  - `app/app.html`:
    - the shell page header (gear on `/family`; Back to Family on Settings);
    - the Family section, split into the dashboard and Settings.
  - `app/family.scss`: dashboard styles.
  - `app/family-settings.scss`: new; the Settings, link, and first-use styles, split to stay under the 4 kB component budget.
- **Build:** `npm run build` passes. Only the existing warnings remain (initial bundle, `trips.scss`, `concerns.scss`).
- **Not run (by owner direction):** Angular specs, Playwright, and full regression. No tests were written or changed. Existing specs that assert the old Family title ("Your family"), the old context panel, or the single relationship list are expected to need updates in the testing pass.
- **Review renders (transient, not canonical evidence):** `src/FamilySafety.Web/e2e/captures/family-dashboard-review/`:
  - `family-live-*` and `family-settings-live-*`: real Development data. Fixture identity A is the Family administrator; B is an active member who is not an administrator.
  - `family-mock-*` and `family-settings-mock-*`: identity A, with browser-mocked sample relationships for review only.
  - `family-contact-sheet.png`.

## Remaining tasks

- [x] `TASK-001` Implement the dashboard and Settings split (D1–D4).
- [x] `TASK-002` Review renders at 1440, 1024, 768, and 390, plus the contact sheet.
- [x] `TASK-003` Owner visual approval (`AC-10`): `PASS`, 2026-10-03 (`DEC-077`), with D5 to D7 applied.
- [x] `TASK-004` Testing pass: update the affected specs, add Family dashboard and Settings coverage (including the boundary for members who are not administrators), and run the focused specs and build.
- [x] `TASK-005` Final regression (`AC-09`), evidence, `VER-WI-009-007`, and closure.

## Owner closure (2026-10-03)

- **Decision:** `DONE / PASS` — `OWNER APPROVED` (`DEC-077`). Evidence is in `evidence/WI-009-007/`.
- **Verification:**
  - Angular: 215/215, including the new `family-dashboard.spec.ts` (9 tests).
  - Focused Playwright: the new `e2e/family-dashboard.spec.ts` 2/2, with the updated `navigation` and `invitation-handoff` specs 8/8.
  - One full `npm run test:e2e`: 22 passed, 0 failed, 0 skipped; 7 of 7 databases removed.
  - Build passes with the existing warnings only; initial bundle 583.51 kB.
  - See `VER-WI-009-007`.
- **No Phase 9.5 capability, and no production API, schema, or authorization change.**
