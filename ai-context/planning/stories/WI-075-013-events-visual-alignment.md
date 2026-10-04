---
Status: DONE / PASS
Authority: Project-owner execution authorization and OWNER VISUAL APPROVAL dated 2026-09-30
Context-Baseline: CTX-1.24
Version: 1.2
Execution-Authorization: YES
---

# WI-075-013 — Events Visual Alignment

## Metadata

- **Work Item ID:** `WI-075-013`
- **Status:** `DONE / PASS`
- **Phase:** `7.5 — End-to-End UX & User-Test Readiness`
- **Priority:** `P2 — Events screen-specific alignment`
- **Context baseline:** `CTX-1.24`
- **Source proposal:** `PRP-027 — Mockup Alignment & UI Gap Assessment` (`APPROVED` as planning basis)
- **Work type:** Behavior-neutral Events presentation alignment
- **Model tier:** `STANDARD`
- **Execution authorization:** `YES` â€” explicitly authorized by project owner on 2026-09-30.
- **Requirement references:** `REQ-UX-003`, `REQ-UX-006`, `REQ-UX-012`; `REQ-PRIV-020`, `REQ-PRIV-021`; `REQ-EVT-008`, `REQ-EVT-009`, and `REQ-EVT-010` through `REQ-EVT-012` as scope boundaries. All existing Event contracts remain authoritative. No canonical requirement change is proposed.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md`; `ai-context/project-status.md` | Operating authority, active baseline, phase, and status constraints | Entry |
| `ai-context/planning/roadmap.md#phase-75--end-to-end-ux--user-test-readiness` | Preserve Phase 7.5 status and distinguish planning from execution authorization | Before execution and verification |
| `ai-context/planning/proposals/PRP-027-mockup-alignment-ui-gap-assessment.md` | Approved reference authority, screen-specific alignment direction, and exclusions | Before execution |
| `ai-context/references/ux/README.md`; `ai-context/references/ux/events/events-workflow.md`; `ai-context/references/ux/events/events-coordination.md`; corresponding `.png` files | Event design direction and explicit boundary between illustration and approved functionality | Before execution and owner review |
| `ai-context/workflows/events.md#req-evt-008` through `#req-evt-012`; `ai-context/core/privacy-safety-contract.md#req-priv-020`; `#req-priv-021`; `ai-context/core/ux-contract.md#req-ux-003`; `#req-ux-006`; `#req-ux-012` | Preserve Event audience, view/edit access, creator/organizer boundaries, distinguish audience from invitation/participation, exclude unimplemented coordination capabilities, and maintain accessible, reference-informed presentation | Before execution and verification |
| `ai-context/planning/stories/WI-075-010-align-shared-shell-core-ui-patterns.md`; `ai-context/planning/verification/VER-WI-075-010.md` | Reuse completed shared tokens and patterns; avoid duplicating shared foundation | Before execution and verification |
| `src/FamilySafety.Web/src/app/app.html`; `app.ts`; `app.scss`; `family.scss`; `src/FamilySafety.Web/angular.json`; `src/FamilySafety.Web/src/app/app.spec.ts`; `home.spec.ts`; current `src/FamilySafety.Web/e2e` specs and config | Existing Event markup, component stylesheet registration, style budget, behavior selectors, and available regression coverage | Before execution and verification |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Plans and Trips screen-specific implementation and workflows | They are separate future alignment candidates and are not changed here. |
| Implementations of Event invitations/RSVP, Needs, Updates, Event-context Check-Ins, Sharing Sessions, or notifications | They are outside this presentation slice and remain subject to their existing canonical requirements and separately scoped work; mockups cannot authorize or expand them. |
| Backend, domain, API, persistence, database, and authentication implementation | No behavior or data changes are in scope. |
| Screenshot baselines and visual-regression tooling | Explicitly excluded by PRP-027 and this story request. |

- **Pinned context dependency:** `None`

## Objective

Make the implemented Events screen materially closer to the useful visual direction of `events-workflow.png` and `events-coordination.png`. Improve scanability, hierarchy, grouping, and coordination context across Event creation, audience selection, visible summaries, organizer context, eligible editing, and existing loading/error/empty states. Preserve current Event behavior, privacy, access, routes, data, wording, and accessible interaction semantics. References are visual guidance, not product specifications.

## Current implementation and concrete visual gaps

The Events view is a section of the root `app.html` component. It includes a create form, Event audience fieldset with per-member view/edit choices and optional filtering, an Events list, in-place editing for eligible users, and existing Family-membership loading, Event-list loading/load-error, no-visible-Events, no-active-Family, no-available-audience, and no-filter-match states. Creation/edit forms also have existing validation and request error/success feedback. Current privacy copy explains that Event sharing does not send an invitation or start location sharing.

Visible Event summaries currently render the name, date/time/location, description, organizer, and eligible edit action as a simple sequence of headings and paragraphs within list items. Their hierarchy is weak: the title, date/time, location, description, organizer context, and action have little separation. The creation form and overview also need a more deliberate relationship and hierarchy. Audience controls are grouped semantically in a fieldset, but their presentation and member rows can be easier to scan. In-place editing should read as part of the specific Event being edited. Existing low-data, loading, validation, request-error, and success states should have deliberate presentation while remaining distinct and truthful. The privacy/sharing explanation should remain visible and fit naturally into the Event composition.

These gaps are based on current source and the PRP-027 assessment; they do not claim a new independent rendered-runtime inspection. The execution story requires rendered desktop and narrow-width review before acceptance.

## Scope

- Improve Events page composition so creation/audience selection and the existing visible Event overview read as distinct, deliberately grouped parts of one screen, with a clear hierarchy and intentional relationship. The desktop layout should make the overview easy to scan as the primary read area and the create form a distinct task area; exact placement/columns remain design choices.
- Make each existing Event summary materially more scannable by distinguishing its title, date/time, location, description, organizer context, and eligible edit action using only currently available data.
- Improve grouping and scanability of current audience controls and member access choices while preserving their semantics and order.
- Visually connect an Event's in-place edit form to the Event being edited while preserving current behavior and accessible reading/focus order.
- Give existing Family-membership loading, Event-list loading/load-error, no-visible-Events, no-active-Family, no-available-audience, no-filter-match, create/edit validation or request-error, and success-feedback states clearer local presentation without conflating their meanings or adding retry actions.
- Keep the existing Event privacy/sharing explanation readable and integrated near the relevant composition.
- Adapt concise-summary and coordination-grouping concepts from the references only to implemented Event fields and actions.
- Reuse WI-075-010 shared tokens and primitives. Prefer an Events-scoped `events.scss` registered via `app.ts` stylesheet metadata; `app.ts` may change only to register the stylesheet. Keep Event composition out of substantial `app.scss` growth.

## KEEP / ALIGN / ADAPT / DROP

- **KEEP:** Event creation behavior and fields; audience assignment and eligibility; visible Event data; organizer context; edit eligibility and in-place edit behavior; current loading/error/empty/no-audience/no-match states; privacy/access rules; wording except minor presentation-only wrapping/grouping; routes; accessibility semantics; stable selectors.
- **ALIGN:** Visual hierarchy, grouping, surfaces/cards, spacing, typography, form/action hierarchy, audience presentation, summary/detail presentation, status presentation, information density, and screen composition.
- **ADAPT:** Reference concepts for concise Event summaries, coordination context, and grouping, limited to current fields, data, and actions.
- **DROP FROM THIS STORY:** No RSVP, invitations, Needs, Updates, Event tabs, calendar views, unsupported sharing controls, unsupported Event categories or lifecycle/status features, or other mockup-only capability is added or implied here. Existing canonical Event requirements for separately scoped invitation/RSVP, Needs, and Updates work remain in force; this story neither implements nor rejects them.

## Non-goals and guardrails

- No Event domain behavior, fields, audience eligibility, view/edit access, organizer rules, privacy rules, ordering/filtering semantics, routes, APIs, backend, database/schema, or authentication changes.
- No Plans, Trips, Home, Family, shell/navigation redesign, or resolution of `OD-001`.
- No new Event capability, data, state, workflow, wording with behavioral meaning, or mockup-derived requirement.
- No broad stylesheet cleanup, style-budget changes/suppression, relocation of shared styles to game budgets, duplicate shared controls/status/focus primitives, or component library.
- No Phase 8 work, formal participant testing, screenshot goldens, pixel assertions, visual-regression infrastructure, or brittle CSS-class/DOM-position/layout assertions.
- Preserve existing accessible selectors and interaction behavior. If the alignment appears to require behavior, access, or privacy changes, stop and return that as separate scope.

## Likely implementation files and styling approach

- `src/FamilySafety.Web/src/app/app.html` — Events-only composition markup/class hooks; preserve current IDs, accessible names, bindings, semantics, and stable selectors.
- `src/FamilySafety.Web/src/app/events.scss` — scoped Event composition rules using existing shared CSS tokens/primitives.
- `src/FamilySafety.Web/src/app/app.ts` — metadata-only registration of `events.scss` in `styleUrls`, if needed; no behavior changes.
- Existing focused verification documentation after separately authorized execution.

Do not add substantial Event layout rules to `app.scss`; avoid changing `family.scss`. Existing `.event-list` and `.event-list-section` rules in `app.scss` are also used by Plans, so do not move or change those shared hooks in a way that alters Plans. Add narrowly scoped Events-only hooks for the new composition in `app.html` and style them in `events.scss`; retain any existing shared styling they rely on. Because all registered styles apply to the root component template, keep new selectors Events-specific and verify they do not affect Plans. Reuse WI-075-010 shared form, surface, action, status, and focus patterns rather than duplicating them.

Current recorded stylesheet sizes: `app.scss` 7.62 kB, `family.scss` 3.74 kB, and `trips.scss` 4.29 kB. Angular `anyComponentStyle` warns at 4 kB and errors at 8 kB. Confirm actual baselines when execution begins. Keep `events.scss` below the 4 kB warning threshold if practical and record its final size and build budget output. Do not raise/suppress budgets or treat separate per-file accounting as permission for unnecessary aggregate style growth.

## Acceptance criteria

1. **AC-01 — Presentation-only boundary:** Changes are limited to Events presentation markup/styles and metadata-only stylesheet registration if needed, plus focused verification documentation. Routes, workflows, Event content/data, behavior, privacy/access, backend/domain/schema/API, and other screens remain unchanged.
2. **AC-02 — Material Events composition:** Rendered desktop Events presents the existing creation/audience area as a distinct task group and the visible Event overview as a deliberate, easily scanned primary read area. Their relationship is clear without requiring a particular side-by-side arrangement. The change is recognizable and materially improves composition; spacing-only, color-only, or superficial CSS changes do not satisfy this criterion. Exact dimensions and layout remain design choices.
3. **AC-03 — Event summary scanability:** Each existing visible Event summary clearly distinguishes its current title, date/time, location, description, organizer context, and eligible edit action. Long content wraps without clipping or obscuring controls. No new facts, categories, or statuses are introduced.
4. **AC-04 — Creation and audience grouping:** The current Event form fields, privacy explanation, audience fieldset, member access choices, optional filter, and create action form a coherent, scannable composition. Existing labels, choices, field order, meanings, and enabled states remain intact.
5. **AC-05 — In-place editing context:** The existing edit form is visually connected to the Event it edits and clearly distinguished from its read presentation. Existing fields, eligibility, save/cancel behavior, error handling, and accessible names remain unchanged.
6. **AC-06 — Low-data and request states:** Family-membership loading, Event-list loading/load-error, no-visible-Events, no-active-Family, no-available-audience, and no-filter-match states each remain truthful, distinct, and understandable with improved local grouping. Existing create/edit validation, request-error, pending, and success feedback remains legible and distinct. A load failure or unfinished request is never presented as empty data; no unsupported retry or action is added.
7. **AC-07 — Privacy and access meaning:** The current explanation that Event sharing does not send an invitation or start location sharing remains visible and readable in context. Audience/view/edit and organizer meaning are not weakened, broadened, or obscured.
8. **AC-08 — Behavior and stable selectors:** Creation, audience assignment/filtering, Event ordering, visible data, edit eligibility, in-place editing, routes, bindings, and current behavior remain unchanged. Existing accessible names and stable selectors are preserved; no behavior is added to facilitate styling.
9. **AC-09 — Reference adaptation boundary:** The two Event references inform grouping and concise summary presentation only. This story adds or implies no RSVP, invitations, Needs, Updates, tabs, calendar, sharing controls, unsupported categories/statuses, or other mockup-only feature. Existing canonical requirements for separate invitation/RSVP, Needs, and Updates work remain unchanged.
10. **AC-10 — Shared styles and budget:** Event-specific composition is scoped to `events.scss`; `app.ts` changes only to register it. New selectors are specific to the Events section and do not change Plans presentation. Existing WI-075-010 tokens and primitives are reused, with no substantial `app.scss` growth, broad cleanup, budget changes, warning suppression, relocation of existing shared list rules solely to change budget accounting, or duplicated shared primitives. Baseline/final stylesheet sizes and budget output are recorded.
11. **AC-11 — Accessibility:** Heading hierarchy, labels, fieldset/group semantics, list semantics, keyboard operation, visible focus, accessible action names, status/error announcements, privacy-copy readability, non-color-only meaning, usable control targets, contrast, and logical reading/focus order are preserved or improved. No full accessibility certification is claimed.
12. **AC-12 — Responsive rendered review:** Desktop and narrow/mobile-like review covers a populated list, longer title/description/location, create form, audience selection, edit state, Family-membership and Event-list loading, Event-list error, no visible Events, no active Family, no available audience, no filter match, and representative create/edit validation or request feedback. Narrow layouts may stack and wrap while preserving reading/focus order, usable actions, and avoiding horizontal overflow, overlap, clipping, or obscured controls. Navigation is unchanged.
13. **AC-13 — Verification, owner review, and status:** Relevant existing Angular/Vitest and Playwright regressions pass; Angular production build and stylesheet budget output are recorded. Focused accessibility checks and rendered desktop/narrow review are documented. Owner visual comparison against both Event references and approval of the final alignment are recorded. `CTX-1.24` remains active; Phase 7.5 remains current; PRP-027 remains approved; WI-075-005/010/011/012 remain done; `OD-001` remains open; Phase 8 remains future. Canonical requirements are unchanged, and this work does not close PRP-027 or Phase 7.5.

## Verification plan

- Render and review desktop and narrow/mobile-like layouts for all states listed in AC-12, including longer user-entered content. Preserve the distinction between Family-membership loading, Event-list loading, and each existing error/empty state.
- Perform focused checks for headings, labels, fieldset/list semantics, accessible action names, keyboard and visible focus, status/error announcements, privacy meaning, contrast, target usability, wrapping, overlap, clipping, and overflow. Do not claim full accessibility certification.
- Compare the result with `events-workflow.png` and `events-coordination.png`; record owner visual approval and any useful reference patterns intentionally not adopted.
- Run relevant existing Angular/Vitest tests, the full existing Playwright regression suite, and the Angular production build. Angular tests currently cover Event creation/audience filtering, in-place update, view-only edit restriction, empty and load-error behavior; the existing Playwright suite has no Event-specific flow (it covers invitation handoff and Trip audience removal). Treat Playwright as regression coverage, not as direct Event workflow coverage. Keep behavior-focused selectors stable; do not add class, DOM-position, pixel, coordinate, or layout-geometry assertions.
- Record starting/final stylesheet sizes and Angular budget warnings. Do not add screenshot goldens or visual-regression infrastructure.
- Formal participant usability testing is not required.

## Canonical requirement impact

No canonical requirement changes are proposed. Preserve `REQ-PRIV-020`, `REQ-PRIV-021`, `REQ-EVT-008`, and `REQ-EVT-009`, plus all other applicable Event requirements. `REQ-EVT-010` through `REQ-EVT-012` already define invitation/RSVP, Needs, and Updates capabilities; these remain separate from this visual story, and their exclusion here does not reject or defer the canonical requirements themselves. If implementation reveals a conflict or need for behavior/access change, stop and propose separate work; do not reinterpret requirements through mockups.

## Relationship to PRP-027 and project status

WI-075-010 completed the shared shell/core-pattern foundation; WI-075-011 completed Home visual alignment; WI-075-012 completed Family and first-use visual alignment. WI-075-013 is the next bounded, screen-specific PRP-027 slice. It does not combine Events with Plans or Trips, duplicate the shared foundation, close PRP-027 or Phase 7.5, resolve `OD-001`, or start Phase 8. Remaining Plans and Trips alignment remains separate future planning work.

Preserve `CTX-1.24` as active; Phase 7.5 as `CURRENT / IN PROGRESS`; PRP-027 as `APPROVED`; WI-075-005 and WI-075-010 through WI-075-012 as `DONE`; `OD-001` as `OPEN`; and Phase 8 as `FUTURE`.

## Planning-time implementation considerations

- Exact desktop grouping, card/surface emphasis, typography, spacing, and narrow-width stacking remain implementation choices for rendered review and owner comparison.
- The stylesheet registration was confirmed and implemented with `events.scss`; visual choices were resolved through implementation and recorded rendered review.
- The default Development fixture did not contain enough states for direct review. The required states were rendered with temporary browser-only response fixtures and the artifacts were removed; see `VER-WI-075-013.md`.
- No product or canonical decision is requested by these presentation choices.

## Pre-execution final story review

- **Review date:** `2026-09-29`
- **Review outcome:** `READY FOR EXECUTION APPROVAL`
- **Reviewed:** Current project status and roadmap; PRP-027; WI-075-010/011/012; Event workflow and applicable UX/privacy requirements; reference notes and both Event reference images; current root Event template/component styles, stylesheet metadata/budgets, Angular/Vitest specs, and existing Playwright coverage.
- **Current implementation finding:** Event creation/audience assignment/filtering, summary/edit display, role-restricted edit action, loading/error/empty states, and privacy explanation exist. Event summaries are plain heading/paragraph sequences; there is no Events-specific composition stylesheet. Existing Event styles are in `app.scss`, and some list classes are shared with Plans.
- **Test coverage finding:** Angular tests exercise Event creation/audience filtering, update, view-only restriction, empty state, and list-load error. The two existing Playwright journeys do not directly exercise Events; preserve and run them as suite-level regression only. No new Playwright scenario is required by this story.
- **Style-budget finding:** `App` currently registers `app.scss` and `family.scss` with `styleUrls`; `events.scss` is absent. Angular's application builder marks each component stylesheet output as `ng-component` and feeds each resource as an individual component-style budget asset. `anyComponentStyle` is evaluated per asset, not against a hidden sum of all component stylesheet files. The styles are still all delivered for the root component, so keep their aggregate reasonable. Use `events.scss`, avoid substantial `app.scss` growth, preserve Plans-shared list styling, and target under 4 kB when practical.
- **Corrections made:** Made composition floor explicit so spacing/color-only changes cannot pass; included both Family-membership and Event-list loading plus create/edit feedback states; clarified that Plans-shared list hooks must not be altered; recorded the existing Playwright coverage limitation and exact Angular Event coverage; confirmed `styleUrls` registration is already supported and the proposed Events stylesheet is not present; clarified that excluding invitation/RSVP, Needs, and Updates from this slice does not supersede their canonical requirements.
- **Execution disposition:** `DONE / PASS`. AC-01 through AC-13 passed; rendered evidence and owner visual approval are recorded in `VER-WI-075-013.md`.
- **Execution authorization:** `YES` — explicitly authorized by project owner on 2026-09-30.
- **Owner visual approval:** `OWNER VISUAL APPROVAL` — the project owner approved the WI-075-013 Events visual direction on 2026-09-30. No additional owner comments or observations are recorded.
