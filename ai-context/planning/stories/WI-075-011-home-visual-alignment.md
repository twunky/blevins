---
Status: DONE — AC-01 through AC-14 PASS
Authority: Project-owner implementation authorization and final owner visual approval, 2026-09-29
Context-Baseline: CTX-1.24
Version: 1.1
Execution-Authorization: YES
---

# WI-075-011 — Home Visual Alignment

## Metadata

- **Work Item ID:** `WI-075-011`
- **Status:** `DONE — AC-01 through AC-14 PASS`
- **Phase:** `7.5 — End-to-End UX & User-Test Readiness`
- **Priority:** `P2 — Home screen-specific alignment`
- **Context baseline:** `CTX-1.24`
- **Source proposal:** `PRP-027 — Mockup Alignment & UI Gap Assessment` (`APPROVED` as the planning basis; this story separately approved and authorized for execution)
- **Work type:** Home-specific, behavior-neutral presentation refinement
- **Model tier:** `STANDARD`
- **Requirement references:** `REQ-UX-001`, `REQ-UX-002`, `REQ-UX-003`, `REQ-UX-006`, `REQ-UX-007`, `REQ-UX-008`, `REQ-UX-011`, `REQ-UX-012`; `REQ-PRIV-006`, `REQ-PRIV-020` through `REQ-PRIV-022` constrain access and presentation. Current Home, Family-context, Event, Plan, and Trip behavior remains authoritative.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md`; `ai-context/project-status.md` | Operating authority, phase, baseline, and current status constraints | Entry |
| `ai-context/planning/proposals/PRP-027-mockup-alignment-ui-gap-assessment.md` | Approved design-alignment basis and KEEP/ADAPT/DROP and verification boundaries | Before execution |
| `ai-context/planning/roadmap.md#phase-75--end-to-end-ux--user-test-readiness`; `ai-context/planning/decision-log.md#open-product-decisions` | Keep Phase 7.5 current and OD-001 open | Before execution |
| `ai-context/core/ux-contract.md#req-ux-001` through `#req-ux-003`, `#req-ux-006` through `#req-ux-008`, `#req-ux-011`, `#req-ux-012`; `ai-context/core/privacy-safety-contract.md#req-priv-006`, `#req-priv-020` through `#req-priv-022` | Family-first Home hierarchy, Family context, reference authority, accessibility, and authorized preview boundaries | Before execution and verification |
| `ai-context/references/ux/home-dashboard/home-dashboard-overview.png` and `.md` | Visual reference and its explicit exploratory/unapproved boundary | Before execution and owner review |
| `ai-context/planning/stories/WI-075-003-home-and-feature-empty-states.md`; `ai-context/planning/verification/VER-WI-075-003.md` | Preserve approved Home data scope, order/caps, low-data state, and existing behavior | Before execution and verification |
| `ai-context/planning/stories/WI-075-010-align-shared-shell-core-ui-patterns.md`; `ai-context/planning/verification/VER-WI-075-010.md` | Shared patterns are complete; consume them without duplicating shared CSS | Before execution and verification |
| `src/FamilySafety.Web/src/app/app.html`; `src/FamilySafety.Web/src/app/app.scss`; `src/FamilySafety.Web/src/app/app.ts`; `src/FamilySafety.Web/src/styles.scss`; `src/FamilySafety.Web/src/app/home.spec.ts`; `src/FamilySafety.Web/src/app/app.spec.ts`; `src/FamilySafety.Web/e2e/invitation-handoff.spec.ts` | Current Home markup, presentation, data-selection behavior, shared tokens, and existing behavior selectors/regression | Before execution and verification |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Safety, RSVP/Needs/Updates, activity-feed, Sharing Session, Check-In, and notification workflows | The mockup depicts exploratory or unapproved Home features; none is in this story. |
| Family, Events, Plans, and Trips screen-specific styles/workflows beyond the Home preview contracts | No redesign of those destinations is in scope. |
| Backend, domain, database, API, and authentication implementation | Presentation only; no source or behavior changes are authorized. |
| Screenshot baselines and visual-regression tooling | Explicitly excluded by PRP-027. |

- **Pinned context dependency:** `None`

## Objective

Bring the implemented Home composition materially closer to the useful visual direction of `home-dashboard-overview.png` by improving hierarchy, spacing, grouping, and the presentation of existing Event, Plan, Trip, and low-data content. Treat the mockup as inspiration rather than a specification. Preserve every existing Home data boundary, action, route, state, privacy meaning, and accessibility contract.

## Current implementation and concrete gaps

The current rendered implementation uses the established shell and shared surface system. Home starts with a welcome heading, explanatory copy, and a prominent View Family action; relationship suggestion messaging/actions follow in the same welcome area. Existing Event, Plan, and Trip previews then appear as similarly treated sections, each with a heading, a capped list of compact text rows, and a destination button. At desktop the rows can form a grid; at narrow widths they stack. With no visible items, a warm low-data panel explains that state and presents three destination actions. Loading and errors remain separately represented.

The reference's useful direction is a more deliberate dashboard composition: concise page identity/supporting text, clear visual priority among useful existing summaries, and compact grouped preview surfaces. Compared with that direction, the current Home gives the welcome/Family action more visual weight than the existing coordination summaries. Each populated preview section spans the page as a similar sequential band, even though the preview rows can form a grid. Each row renders its title and all date/time or destination facts as one inline sentence. The low-data state is understandable, but its next actions read as a separate group of equal buttons rather than as part of a composed Home overview. The pending relationship suggestion is the only currently implemented attention item on Home; its existing message and Family action can receive a clearer, restrained place without inventing a general alert feed or urgency state.

The mockup's desktop grouping, compact summary cards, stronger section identity, and alignment of section actions are useful. Its blue palette, imagery, badges, counts, RSVP/activity/task/sharing/Safety panels, and rail are not Home requirements. Keep the shell's existing `h1` Home and Family context; do not add a second dashboard page title. The current Home order is welcome/context, any relationship suggestion, Events, Plans, then Trips. Preserve that logical and keyboard reading order while improving the visual grouping.

These are presentation gaps inferred from current markup/styles and the recorded rendered review in `VER-WI-075-010`; no new independent live visual inspection is claimed in this planning record.

## Scope

- Refine Home-only composition and vertical rhythm, keeping the existing application shell and routes intact.
- Make a recognizable Home composition change: a concise welcome/context area followed by a related overview of existing Event, Plan, and Trip preview panels. At desktop width the preview panels should read as one deliberately arranged overview rather than three identical full-width stacked bands; at narrow width preserve their logical reading order. Exact columns, dimensions, and coordinates are implementation choices.
- Give the existing Event, Plan, and Trip summaries a coherent, scannable grouping and stronger internal hierarchy using their currently available preview fields. Separate item titles from existing supporting facts in the visual treatment, and associate each destination action with its own section.
- Improve how existing headings, supporting text, summary facts, and existing destination actions relate visually.
- Keep the welcome/Family context clear without allowing it to overwhelm the coordination summaries. When a relationship suggestion exists, make its existing text and Review in Family action easy to notice before the routine previews; do not create a generic Needs My Attention panel or new status.
- Compose the low-data presentation as the same Home experience, with its reassurance and existing next-step actions easy to scan. When only some preview types have data, show those populated panels without empty placeholder panels.
- Reuse `styles.scss` tokens and WI-075-010 shared controls, surfaces, buttons, status, focus, and spacing patterns. Any truly Home-specific layout remains scoped to Home selectors in `app.scss`; do not copy shared declarations.

## Non-goals and guardrails

- No shell or navigation redesign; do not settle `OD-001`.
- No Family, Events, Plans, or Trips destination redesign.
- No changed preview sources, filters, order/caps, Family boundary, dates, data fields, route/action behavior, loading/error semantics, or current-family behavior.
- No new product behavior, backend/domain/schema/API changes, component library, architecture changes, visual-regression infrastructure, screenshot goldens, or pixel/layout assertions.
- No formal participant usability study for this story.

## Acceptance criteria

1. `AC-01 — Home-only scope`: Changes are limited to Home-specific composition/presentation plus necessary focused evidence documentation; the shell, navigation, other destinations, and product behavior remain unchanged.
2. `AC-02 — Material Home composition`: On a populated desktop Home, the existing Event, Plan, and Trip sections form a deliberate, related overview with clear section identity rather than three visually identical full-width bands. The welcome/context area remains concise; Event, Plan, and Trip logical reading order is preserved. Owner review confirms a noticeable improvement toward the reference without relying on exact columns, dimensions, or pixels.
3. `AC-03 — Preview hierarchy`: In each existing preview panel, item title and available date/time/destination facts are visually distinct, compact, and scannable; its existing View action is visibly associated with that panel. Long user-provided text wraps without clipping or obscuring actions. No imagery, badges, counts, status labels, or new facts are fabricated.
4. `AC-04 — Welcome and real attention`: The current welcome, Family context, View Family action, and conditional relationship-suggestion message/action remain present and understandable. A pending suggestion, when present, is easier to find ahead of routine previews without a new attention category or implied urgency; when absent, no attention placeholder appears.
5. `AC-05 — Existing actions`: Existing View Events, View Plans, View Trips, View Family, Review in Family, and low-data Open Events/Plans/Trips actions retain their meaning, destinations, enablement, accessible names, and stable test selectors.
6. `AC-06 — Preview behavior`: Existing data sources, visibility/access rules, current-Family Trip boundary, maximum of three items per preview, ordering, date/time values, and conditional rendering are preserved.
7. `AC-07 — State clarity`: Loading, per-area error, no-Family/multiple-Family states, relationship suggestions, and the approved low-data state remain distinct and truthful; a failed/unfinished load is never presented as an empty Home.
8. `AC-08 — Low and mixed data`: When all preview sources successfully return no visible items, the approved reassuring copy and all three existing destination actions remain visible in a coherent Home surface. When only some sources have visible items, show only their populated panels and preserve truthful loading/error messages for other sources; do not insert empty placeholders or mockup-only content.
9. `AC-09 — Shared pattern reuse`: Home consumes WI-075-010 shared tokens and core patterns. Home-specific panel geometry may use those tokens; no new duplicate shared button/control/focus/status primitives or unrelated shared-style changes are introduced. Any genuine shared-foundation defect is identified explicitly for review.
10. `AC-10 — Accessibility`: Semantic heading order, landmarks, list semantics, labels, live status/error roles, keyboard order, visible focus, accessible names, and non-color-only meaning remain intact; new text/surface combinations meet applicable contrast expectations.
11. `AC-11 — Responsive usability`: At representative desktop and narrow widths, Home content and actions remain readable and operable with no horizontal overflow, overlap, clipped content, or inaccessible controls. This is a focused Home usability check, not a breakpoint redesign.
12. `AC-12 — Regression and build`: Existing relevant Angular/Vitest and Playwright behavior regressions pass, and Angular production build completes. Keep existing role/name browser selectors stable; add no Playwright CSS-class, DOM-position, pixel, or layout assertions and no screenshot baseline/golden test.
13. `AC-13 — Visual sign-off`: Record rendered desktop and narrow-width review of populated, low-data, and useful mixed/long-content Home states; accessibility checks; and owner visual comparison with the reference. Record the concrete composition and hierarchy improvements and accepted deviations. Do not require a formal participant study.
14. `AC-14 — Status and contracts`: WI-075-005 and WI-075-010 remain DONE; Phase 7.5 remains CURRENT; OD-001 remains open; canonical requirements remain unchanged; WI-075-011 completion does not itself close PRP-027, Phase 7.5, or authorize another story.

## KEEP / ALIGN / ADAPT / DROP

- **KEEP:** Existing Family welcome/context; Event, Plan, and current-Family Trip previews; existing preview ordering and cap; current visible data and privacy/access boundaries; existing relationship suggestions; approved low-data copy/actions; loading/error distinctions; routes and interaction behavior.
- **ALIGN:** Home heading/supporting-copy hierarchy, whitespace/rhythm, preview grouping, summary title/detail/action hierarchy, and the low-data surface with the warm, calm, family-centered design language and shared patterns from WI-075-010.
- **ADAPT:** The reference's concise identity, compact grouped summaries, and conditional attention emphasis only as they map to currently implemented Home content. Preserve the current shell title, actual content, existing Event/Plan/Trip reading order, and established actions.
- **DROP:** Safety/concern panels, activity feeds/updates, birthday notes, RSVP/Needs/tasks, check-ins, notifications, live location/presence, Sharing Sessions, quick links/calendar, new statuses, imagery-dependent content, search, and any information with no current approved data source. Do not add these for visual parity.

## Likely implementation files

- `src/FamilySafety.Web/src/app/app.html` — Home-only markup/grouping and semantic presentation.
- `src/FamilySafety.Web/src/app/app.scss` — Home-scoped layout and presentation, reusing existing shared styles/tokens.
- `src/FamilySafety.Web/src/app/home.spec.ts` and `src/FamilySafety.Web/src/app/app.spec.ts` — existing regression coverage to run; update only where a behavior-preserving semantic markup change requires it. Prefer headings/roles/labels or stable section IDs for any focused behavior assertion. No new layout, DOM-position, or pixel checks.
- `src/FamilySafety.Web/src/styles.scss` — inspect for shared patterns/tokens; changes are out of scope unless a specific missing shared foundation is demonstrated and separately reviewed as a small extension.

`src/FamilySafety.Web/src/app/app.ts` is read-only context for the current preview filters, order/caps, and Family boundary. No TypeScript/data-fetch change is expected. If implementation appears to require one, stop and return for story-scope review rather than changing behavior under this presentation story.

## Shared patterns reused from WI-075-010

Use semantic color, spacing, surface, border, radius, and focus tokens from `styles.scss`; the existing shared button/action classes; shared app-root control, focus, help/status treatments; and existing page/card/section conventions. Keep feature-specific grouping local to Home markup/styles. Do not repeat or globally redefine those shared primitives.

## Accessibility guardrails

Preserve the page's existing landmark and accessible name relationships, heading semantics/IDs, list semantics, all status/alert roles, visible focus, keyboard order, textual distinctions among loading/error/low-data states, and current action names/targets. Any new semantic grouping must have a meaningful accessible heading/label. Avoid color/icon-only meaning and verify contrast on actual rendered surfaces. Do not change stable Playwright selectors or accessibility labels for visual convenience.

## Responsive boundary

Review rendered Home at representative desktop and narrow widths, including longer preview text and action wrapping. Correct Home-specific overflow or unusable controls discovered in these checks within the story. Do not redesign global breakpoints, navigation, multi-Family selector behavior, or create a final responsive design system; report issues that require those decisions separately.

## Tasks

- [x] `TASK-001` — Confirmed the approved baseline, current Home conditions and preview behavior, reference boundaries, and existing Home/browser selectors before editing.
- [x] `TASK-002` — Refined Home-only markup and scoped styles to create the stated welcome/overview hierarchy and distinguish preview title, facts, and section action without changing content or behavior.
- [x] `TASK-003` — Reviewed conditional relationship suggestion, populated, mixed, loading/error, no-context, and low-data presentation for truthful hierarchy and preserved actions.
- [x] `TASK-004` — Completed desktop and narrow rendered review plus focused accessibility checks, including long content and wrapped actions; owner supplied final visual approval against the reference (OWNER VISUAL APPROVAL).
- [x] `TASK-005` — Ran relevant existing Angular/Vitest, Playwright, and production-build regression; recorded warnings and the isolated-port runner limitation in the verification report.

## Verification plan

- Render and review Home with representative populated previews and the approved low-data state at desktop and narrow widths; include a mixed-data state and inspect longer titles/details and action wrapping. Record how the new composition differs from the existing full-width preview bands and inline item rows.
- Compare the result with `home-dashboard-overview.png` for hierarchy, grouping, spacing, compactness, and family-centered tone, while checking every visual choice against the implemented product and explicit DROP list.
- Owner visual review against the reference; capture concise observations and any accepted deviations in verification evidence.
- Accessibility review: keyboard traversal and focus, heading/landmark/list semantics, accessible names, status/error announcements, non-color-only meaning, contrast on rendered surfaces, and usable targets.
- Run relevant existing Angular/Vitest regression, including Home and app/shell tests as applicable; run the existing invitation-handoff Playwright journey that reaches Home, plus other existing journeys affected by any shared-style change. Keep browser selectors based on roles/names and behavior; do not make them depend on CSS classes, DOM position, or layout.
- Run the Angular production build and record outcomes/warnings. Do not add screenshot baselines, golden tests, pixel comparisons, or exact layout assertions.
- Record changed files, states/viewports reviewed, regression commands/results, owner review outcome, remaining limitations, and explicit canonical/status impact.

## Risks and blockers

- Greater visual emphasis can imply new priority or urgency. Do not invent labels, counts, alerts, freshness, attention states, or data.
- Home summaries share content but have distinct Event/Plan/Trip meanings. Retain labels and avoid conflating concepts.
- Current-Family Trip preview behavior is a privacy/context boundary; presentation changes cannot broaden it or imply all creator-retained Trips belong to the selected Family.
- Home resides in the shared app template and stylesheet. Scope selectors and markup changes tightly to Home and inspect for effects on other routes.
- If a confirmed shared-pattern defect from WI-075-010 is found, document it and determine whether it merits a separate small foundation extension before changing shared CSS.

## WI-075-005, WI-075-010, and PRP-027 relationships

WI-075-005 remains `DONE`; broader formal human usability validation remains deferred until visual design is more mature. WI-075-010 remains `DONE` and provides the shared foundation consumed here. This story addresses only the Home-specific screen composition gap identified by PRP-027. Completing it does not mean all remaining PRP-027 alignment work, Phase 7.5, or any subsequent story is complete or authorized.

## Final story review — 2026-09-29

**Result:** `READY FOR EXECUTION APPROVAL`. The current template, styles, preview-selection code, Home tests, and existing invitation-handoff Playwright selector were compared with the reference and canonical Home/privacy rules. The initial story was too open-ended about the intended visual change. This revision requires a visibly different desktop overview composition, distinct preview title/fact/action hierarchy, a restrained place for the only implemented attention item, and coherent low/mixed-data presentation. It preserves current content, reading order, actions, access, and data behavior. No canonical contradiction or blocking product decision was found.

**Implementation decisions left open:** The precise responsive panel arrangement, typography scale within the existing hierarchy, and Home-specific spacing/surface details should be resolved by rendered review and owner visual comparison. They do not require a new product decision. If the design requires changing preview sources, Family access, shell/navigation, behavior, or shared foundations beyond a small reviewed extension, stop and rescope.

**Recommended execution routing at review:** `STANDARD` tier with medium reasoning for the bounded Angular presentation change and verification. Escalate to `DEEP` if a privacy/access or cross-contract effect is discovered. The later owner request explicitly authorized this story's execution.

## Execution authorization

- **Approved story:** `YES` — WI-075-011 scope only
- **Authorized by:** Project owner in the current Codex request
- **Authorized at:** 2026-09-29
- **Authorized scope:** Home-specific presentation implementation and verification within AC-01–AC-14; no other PRP-027 story or application feature

## Evidence

| Requirement or criterion | Evidence | Result |
|---|---|---|
| `AC-01`–`AC-14` | `ai-context/planning/verification/VER-WI-075-011.md` | See per-criterion results |

## Verification

- **Verification report:** `ai-context/planning/verification/VER-WI-075-011.md`
- **Independence:** `SELF-VERIFIED` implementation/rendered/technical checks; final owner visual acceptance recorded separately
- **Outcome:** `PASS` — AC-01 through AC-14 passed; AC-02 and AC-13 are classified `OWNER VISUAL APPROVAL`.
- **Verified at:** 2026-09-29
