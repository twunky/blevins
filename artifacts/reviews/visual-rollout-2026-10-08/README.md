# Application visual rollout — 2026-10-08

**SELF-VERIFIED.** Tested UI version: `visual-rollout-2026-10-08`, working tree based on `7f31574`. This is implementation and technical verification under the owner's pasted authorization. It is not human usability approval or a change to CTX-1.39, Phase 10, or future Phase 11 scope.

The approved Home Dashboard's black background, charcoal surfaces, cyan accents, typography, shell, and interaction treatments now extend across the existing application. Interior pages use smaller headings and practical form layouts. No new pages or product features were added.

## Completed application areas

| Area | Surfaces covered |
|---|---|
| Family | Overview, directory/member details, relationships, membership/administration, Family settings, Households, Shared Family Dates |
| Account | Me / Settings, profile fields and visibility controls, existing invitation/profile-review states |
| Events | Dashboard, all-events list, creation/detail/editing, invitations/self-RSVP, Needs and Updates |
| Plans & Trips | Group landing, lists, creation/details, Plan activity/timing correction and Notes, Trip editing/audience/Notes/completion confirmation |
| Safety & care | Safety dashboard, Check-Ins, Safety Circles, Trusted Contacts, temporary location sharing, Concerns, Incidents and their existing coordination/timeline/task forms |
| Shared interface | Navigation, Family selector, cards, titles, buttons/links, fields, native date/time/select controls, status indicators, inline confirmations, loading/error/retry/empty/unavailable states |

## Shared styling and preservation

- `src/FamilySafety.Web/src/styles.scss` owns the reusable palette, shell, controls, focus, semantic feedback, and responsive interior-page patterns. Colors come from the approved implementation: page `#000`, cards `#111`, secondary surfaces `#17191c`, primary cyan `#00d4ff`, muted text `#a5adb9`.
- The duplicated shell/palette rules were consolidated. `home-shell.scss` now contains only the approved Home-specific link/focus/selector measurements; `home.scss` retains the Dashboard layout. Its only rollout-specific changes are the shared Family placeholder icon colors and comment.
- Feature styles replace hard-coded light colors with semantic tokens. Destructive operations get a distinct treatment; existing labels, confirmations, handlers, validation, and submitted values are preserved.
- No new framework, component library, dependency, API contract, migration, domain rule, permission, consent behavior, or fixture definition was introduced.
- Existing uncommitted work was preserved. `app.ts` and `home-pending-rsvp.spec.ts` remain as supplied at session start. Other pre-existing Home edits were retained while performing the authorized style consolidation.

The final Home comparison has **zero differing pixels** at both 1440px and 390px, with identical full-page dimensions. Browser time was fixed to the original reference's date/greeting for the final comparison; this does not alter application code. See [comparison results](dashboard-comparison.json).

Navigation remains the same six destinations. Existing tests verify Events → `/events`, Home Check-Ins → `/safety#check-ins`, keyboard focus/current-route semantics, direct loads and refreshes, Family selection, and authorized/denied item transitions. Connected journeys retain explicit recipients, owner-only contacts/presets, independent Concern/Incident audiences, sharing stop/expiry, and role restrictions.

## Visual evidence

| Surface | Desktop | Mobile |
|---|---|---|
| Approved Home reference | [Before](home-before-1440.png) · [After](home-after-1440.png) | [Before](home-before-390.png) · [After](home-after-390.png) |
| Family | [After](family-1440.png) · [Older light UI](family-before-1440.png) | [After/navigation](family-390.png) |
| Family settings | [1440px](family-settings-1440.png) | [390px](family-settings-390.png) |
| Events | [1440px](events-1440.png) | [390px](events-390.png) |
| Plans | [1440px](plans-1440.png) | [390px](plans-390.png) |
| Trip form | [1440px](trip-create-1440.png) | [390px](trip-create-390.png) |
| Safety | [1440px](safety-1440.png) | [390px](safety-390.png) |
| Profile/privacy form | [1440px](profile-1440.png) | [390px](profile-390.png) |
| Incident detail | [1440px](incident-detail-lineage-1440.png) | [390px](incident-detail-lineage-390.png) |
| Trip confirmation | [Desktop](trip-confirmation-desktop.png) | [390px](trip-confirmation-390.png) |

Additional representative evidence: [tablet Event form](event-create-768.png), [Check-In form](check-in-form-390.png), [sharing form](sharing-form-390.png), [Concern form](concern-create-390.png), [Incident form](incident-create-390.png), [completed Trip](trip-completed-390.png).

The Family before/after images use different fixture identities; they demonstrate the visual rollout rather than a pixel comparison. Home uses the same unchanged ordinary Development data. Other review screenshots use synthetic isolated scenarios. The complete 69-capture surface/draft sweep is in the ignored `src/FamilySafety.Web/e2e/captures/visual-design-rollout/` directory; populated journey captures remain in their existing spec directories. [Screenshot hashes](screenshots.sha256) identify the retained review files.

## Validation results

| Validation | Result |
|---|---|
| Production Angular build | PASS; final initial bundle 725.90 kB. Budget warnings remain for the initial bundle and Home, Trips, and Concerns component styles; no budget thresholds were changed. |
| Broad Angular regression | PASS — 27 files, 420 tests (`npm test -- --watch=false`) |
| Focused Family/Safety tests after refinement | PASS — 2 files, 28 tests (`npx ng test family-safety-web --watch=false --include=src/app/family-dashboard.spec.ts --include=src/app/safety/safety-dashboard.spec.ts`) |
| Existing navigation/connected journeys | PASS — Concern, Incident, Family organization, Event coordination, Plans/Trips, Plan correction, desktop/mobile/intermediate navigation |
| Final Safety and visual sweep | PASS — 6 tests, including permission recovery, freshness, stop/real server expiry and 23 surfaces/drafts at each of 1440, 768 and 390px |
| Home CTA/RSVP and Trip completion/editing checks | PASS — 2 tests |
| Playwright aggregate | All 22 unique selected checks ultimately passed. The first broad run had 19 passes and one Family-context readiness timeout in the sharing test; the focused rerun passed after adding a membership-load wait and selected-value assertion to its test helper. No application behavior or assertions were weakened. |
| Accessibility/responsiveness | No detected WCAG A/AA violations or horizontal overflow across the 69 final surface/viewport combinations. Keyboard navigation, visible focus, six-link order, primary touch targets, dark native controls and reduced-motion behavior checked. |
| Dashboard comparison | PASS — zero pixel differences at 1440px and 390px |
| `git diff --check` | PASS; Git reports normal LF/CRLF conversion notices |

E2E runs used the existing isolated SQL scenario harness on port 5080. Angular's already-running server on 4200 was reused through an ignored local config (`e2e/captures/visual-rollout/run.config.ts`); the usual tracked config remains unchanged. Successful scenarios were removed by the existing guarded harness. The ordinary Development API was restored with the original documented configuration, and Angular remains running at `http://localhost:4200`.

## Limits and Git status

No in-scope application area remains on the old light theme. Future production-entry screens/features remain outside this work. Automated Chromium checks and screenshot review do not certify all assistive technologies, other browsers, physical-device location behavior, or human understanding. No human usability approval is claimed; observations should identify this tested UI version.

The existing harness retained one synthetic diagnostic database from the initial failed sharing check: `FamilySafety_Invitation_E2E_1a7ab2bd48f847e7bb1ba6e267dc1add`. Its ownership/diagnostic record remains under `e2e/scenario-runs/20261008T035610-02380312/`. Retained failure cleanup was not retried automatically. Existing and human-testing databases were excluded from the automated test writes and cleanup.

Application edits are limited to Angular presentation, plus the focused test additions/readiness fix and this review evidence. Backend and canonical governance documents are unchanged. See [changed-file inventory](files-changed.txt) for the working-tree paths, including preserved starting edits. All changes are unstaged and uncommitted. No staging, commit, push, branch switch, or PR action was performed.
