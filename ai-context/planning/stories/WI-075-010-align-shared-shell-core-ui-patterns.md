---
Status: DONE — AC-01 through AC-21 PASS
Authority: Project-owner implementation authorization in current Codex request, 2026-09-29
Context-Baseline: CTX-1.24
Version: 1.0
---

# WI-075-010 — Align Shared Shell and Core UI Patterns

## Metadata

- **Work Item ID:** `WI-075-010`
- **Status:** `DONE — AC-01 through AC-21 PASS`
- **Phase:** `7.5 — End-to-End UX & User-Test Readiness`
- **Priority:** `P1 — Shared presentation foundation`
- **Context baseline:** `CTX-1.24`
- **Source proposal:** `PRP-027 — Mockup Alignment & UI Gap Assessment` (`PROPOSED`); this story adopts its bounded shared-shell/pattern direction and does not imply proposal acceptance or authorize execution.
- **Execution authorization:** `YES` — project-owner approval in current Codex request, 2026-09-29
- **Work type:** Presentation-only refinement of current Angular shell and shared UI styles.
- **Model tier:** `DEEP` (cross-screen UI hierarchy with privacy/accessibility meaning to preserve; no behavior changes authorized).
- **Requirement references:** `REQ-UX-001/002/003/006/011/012`; existing Family/profile, privacy, Event, Plan, Trip, and invitation contracts remain constraints, not change targets.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md`; `ai-context/project-status.md` | Operating authority, baseline, phase, and pending human-test checkpoint | Entry |
| `ai-context/planning/proposals/PRP-027-mockup-alignment-ui-gap-assessment.md` | Source direction, P1 boundaries, classifications, exclusions, and visual-testing boundary | Before execution |
| `ai-context/planning/roadmap.md#phase-75--end-to-end-ux--user-test-readiness`; `ai-context/planning/decision-log.md#open-product-decisions` | Phase boundary; preserve unresolved `OD-001` and no feature expansion | Before execution and verification |
| `ai-context/core/ux-contract.md#req-ux-001`, `#req-ux-002`, `#req-ux-003`, `#req-ux-006`, `#req-ux-011`, `#req-ux-012`; relevant privacy and product contract anchors for current Family/profile, administrator, and audience context | Family-first direction, accessibility, conceptual destinations, mockup authority, and existing privacy/access constraints | Before execution and verification |
| `ai-context/planning/stories/WI-075-004-mvp-visual-design-interaction-refinement.md`; `ai-context/planning/verification/VER-WI-075-004.md` | Existing completed visual foundation, approved scope, implementation patterns, evidence, and known limitations | Before execution |
| `ai-context/planning/stories/WI-075-005-end-to-end-mvp-user-test-execution-verification.md` | Maintain pending participant test and avoid changing flow, terminology, or screen composition | Before execution and verification |
| `ai-context/planning/stories/WI-075-008-playwright-foundation-invitation-identity-handoff.md`; `ai-context/planning/stories/WI-075-009-trip-audience-removal-access-enforcement.md` | Preserve stable accessible Playwright selectors; check WI-075-009 status before optional Trip-suite verification | Before execution and verification |
| `src/FamilySafety.Web/src/styles.scss`; `src/FamilySafety.Web/src/app/app.html`; `src/FamilySafety.Web/src/app/app.scss`; `src/FamilySafety.Web/src/app/trips/trips.html`; `src/FamilySafety.Web/src/app/trips/trips.scss`; current Angular tests/config | Existing token, shell, shared patterns, route markup, styles, and verification commands | Before execution and verification |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Mockup-only Safety, Sharing, Check-In, RSVP/Needs/Updates, broad admin/settings, and future feature workflows | They are outside implementation scope and cannot authorize UI or behavior. |
| Production identity, APIs, persistence, domain implementation, and unrelated historical work | This story changes presentation only. |
| Visual regression tooling and screenshot baseline material | Explicitly deferred by PRP-027 and WI-075-008. |

- **Pinned context dependency:** `None`

## Objective

Make the existing routed shell and core shared UI patterns feel consistent, warm, calm, family-centered, modern, and trustworthy by refining the current token/style foundation and applying it consistently. Improve hierarchy and visual consistency without changing routes, navigation composition, screen information architecture, wording, workflows, data, permissions, or product behavior.

This is a shared-foundation story. It should resolve confirmed inconsistencies once so later screen-specific stories can use the same patterns without duplicating CSS. PRP-027 and the mockups provide visual direction only; canonical contracts and current approved behavior remain authoritative.

## Scope — shared shell

- Clarify visual hierarchy among the existing application eyebrow/title, current Family context, current destination, and shell navigation.
- Apply consistent existing-token spacing/alignment to the shell, page frame, page headers, and content surfaces across current routes.
- Keep active-route presentation visible and programmatically conveyed through existing semantics.
- Make the selected Family context easy to identify while preserving its session-only, presentation-only behavior and current selector states.
- Improve the existing grouped shell treatment only as presentation; preserve the current route set and navigation structure.

## Scope — shared UI patterns

After reviewing the current routes, align confirmed shared-pattern inconsistencies in:

- page and section headers and their typography/spacing hierarchy;
- card/list surfaces, borders/radii, and padding;
- primary, secondary, quiet/navigation, cancel, and destructive action treatments;
- form control spacing, labels, helper text, and existing error placement;
- empty/loading/success/error/status surfaces, with meaning available in text and semantics;
- consistent use of the existing color, type, spacing, radius, focus, and shadow tokens;
- visible keyboard focus and usable action target sizing.

Prefer existing global tokens and styles in `styles.scss`/`app.scss`. Make minimal markup-class adjustments only when needed for consistent adoption. `trips.scss` may receive narrowly scoped normalization where it duplicates a shared control or surface treatment; this does not authorize Trip detail, Notes, or audience-editor layout changes. There is no separate shared component library to expand. No new component or component library is expected or authorized by this story. If execution shows a genuinely duplicated, stable semantic element that a component would simplify, stop and seek separate scope approval; do not add generic Card, PageHeader, FormField, or design-system component hierarchies here.

Patterns may be adopted by current Home, Family, Events, Plans, Trips, and Me/profile views. Adoption means shared presentation treatment only; no composition or content change.

## Non-goals

- Change route structure or add/remove/relabel destinations.
- Decide the number, grouping, or final composition of mobile navigation; `OD-001` remains open.
- Introduce or alter product behavior, data, fields, statuses, workflow order, validation, authorization, privacy, or domain/API behavior.
- Change Home composition, content priority, preview scope, or empty-state wording.
- Change Family information architecture, profile fields, relationships, or administrator meaning.
- Change Event, Plan, or Trip workflow or information architecture.
- Change Trip detail, Trip Notes, or Trip audience editor/preview structure.
- Change invitation/first-use terminology, steps, or wording.
- Add visual regression, screenshot baselines, pixel assertions, or layout-dependent Playwright assertions.
- Reproduce mockups literally or add any mockup-only feature.
- Modify canonical requirements, PRP-027, WI-075-005, WI-075-009, or project status as part of execution.

## Expected file impact

Likely presentation touchpoints are `src/FamilySafety.Web/src/styles.scss` for the existing token/global navigation layer; `src/FamilySafety.Web/src/app/app.scss` and, only where needed for minimal class adoption, `app.html` for shared shell/page styles; and possibly `src/FamilySafety.Web/src/app/trips/trips.scss` only to remove or normalize duplicated shared presentation rules without changing Trip-specific layouts. There is no reusable shared Angular component library. Do not modify `app.routes.ts`, application behavior, API/domain/database files, or introduce a component library. Existing test files should not need selector changes; the verification report is created during separately authorized verification, not in this proposal/story-creation change.

The current verification records identify component-style warnings for `app.scss` and `trips.scss` (7.44 kB and 5.55 kB against a 4 kB warning budget, below the 8 kB error budget). Confirm the actual baseline when execution begins, record the build output, and do not suppress warnings or make warning elimination a separate objective.
## Acceptance criteria

1. **`AC-01 — Existing design foundation`**: The shared styles use the existing semantic token vocabulary for colors, typography, spacing, radii, focus, and surfaces. Any proposed new token is justified by a confirmed repeated need and reuses the current visual direction; no one-off value is introduced where an existing token fits.
2. **`AC-02 — Shell hierarchy`**: At representative desktop width, the existing application label/title, current Family context (when present), page content, and navigation have consistent spacing and a clear hierarchy across Home, Family, Events, Plans, Trips, and Me/profile.
3. **`AC-03 — Active destination`**: The current route has a consistent visible active state that is distinguishable without color alone and retains its existing accessible/programmatic current-page semantics. Plans and Trips remain distinct routes.
4. **`AC-04 — Family context`**: Current Family identity and the existing multi-Family selector remain visually clear and accurately reflect existing states. Styling does not change selection, session retention, loading/error behavior, state clearing, or access.
5. **`AC-05 — Page frame and headers`**: Current routes use consistent content alignment/width and page-header treatment through shared styles; no individual screen is restructured to achieve visual uniformity.
6. **`AC-06 — Section and card patterns`**: Confirmed shared section headers, list/card surfaces, and padding follow a consistent token-based treatment where those shared patterns are used. Existing content, order, and screen composition remain unchanged.
7. **`AC-07 — Action hierarchy`**: Existing primary, secondary, navigation/quiet, cancel, and destructive actions use consistent visual styling. Do not move actions or change which action is primary in the workflow; if priority is ambiguous, leave it unchanged for later review. Names, enabled conditions, actions, and destructive meaning remain unchanged.
8. **`AC-08 — Forms and guidance`**: Existing form labels, controls, helper text, and validation/error presentation have consistent spacing where shared styles apply. Fields, text, validation, grouping, and workflow order remain unchanged.
9. **`AC-09 — Empty and status treatments`**: Existing empty, loading, success, error, and alert patterns use consistent shared spacing/surface treatment where applicable. State remains textually and semantically understandable; loading, failure, and empty states remain distinct. Use only current message/state categories; do not invent workflow states or categories.
10. **`AC-10 — Shared style reuse`**: Confirmed common styling is solved through existing shared tokens/styles or a clearly reusable existing pattern. No duplicate page-specific button/card/form rules are added where the shared treatment applies. Preserve legitimate feature-specific styling and context, such as the distinct Trip audience/Notes surfaces and structural-administration callout. Any necessary Trip-local rule remains narrow and does not restructure Trip surfaces.
11. **`AC-11 — Warm, calm direction`**: The result retains the current warm-neutral, evergreen/sage family-centered tone and does not become an enterprise/admin console or surveillance-oriented interface. Mockups are not copied literally.
12. **`AC-12 — Readability and contrast`**: Existing readable text is preserved. Any changed text/control/state color combinations are checked against their actual backgrounds for the relevant WCAG AA contrast role; no reviewed combination regresses below its applicable criterion.
13. **`AC-13 — Keyboard and semantics`**: Keyboard traversal and visible focus remain usable; headings, labels, roles, and active-route semantics remain appropriate. Focus is not removed or conveyed by color alone.
14. **`AC-14 — Target sizing`**: Existing interactive target sizing is preserved or improved, aiming for at least 44 × 44 CSS px where control type and layout allow. Reviewed primary actions and controls remain usable at representative desktop width and a narrow-width smoke size without clipping or overlap.
15. **`AC-15 — Accessible names and stable selectors`**: Accessible names, labels, and user-facing strings used by existing Playwright selectors remain unchanged. If a proposed visual adjustment appears to require a name/string change, stop and report it for separate review; do not silently alter the contract.
16. **`AC-16 — Behavior and scope preservation`**: No route, navigation destination/grouping, screen information architecture, visible feature capability, terminology, workflow, data, validation, permission, privacy rule, or product behavior changes. No Home/Family/Event/Plan/Trip/first-use restructuring is introduced.
17. **`AC-17 — Visual review`**: Perform and record a visual review at a representative desktop viewport across the six current destinations. Perform only a basic narrow-width smoke to catch obvious clipping, overlap, or unusable controls; detailed breakpoint refinement is outside scope. Do not require pixel-perfect comparison.
18. **`AC-18 — Existing browser behavior`**: The existing invitation/identity/access Playwright journey passes without layout assertions or selector rewrites. If WI-075-009 is `DONE` when execution begins, run its existing Trip audience/access journey unchanged and record the result; if not, record its status and do not add or substitute Trip browser tests in this story.
19. **`AC-19 — Existing lower-layer verification`**: The existing Angular/Vitest suite passes, and the Angular production build completes. Record test results and the starting/final component-style budget warnings; do not suppress existing warnings or leave a new/worsened warning unexplained. Warning elimination is not a goal unless it follows naturally from shared-style cleanup.
20. **`AC-20 — No visual-regression infrastructure`**: No screenshot baselines, screenshot approvals, pixel assertions, layout-dependent selectors, or visual-regression infrastructure are added or modified.
21. **`AC-21 — Implementation evidence`**: Record shared files changed, current screens adopting the shared styles, visual/accessibility checks performed, existing test/build outcomes, and any remaining screen-specific gaps. No claim of full accessibility certification is made.

## Tasks

- [x] `TASK-001` — Reconfirmed baseline, proposal direction, WI-075-005, OD-001, and WI-075-009; identified repeated control, focus, helper, and status styles while retaining feature-specific surfaces.
- [x] `TASK-002` — Consolidated shared control, focus, button hierarchy, help text, and status styles into the existing global stylesheet.
- [x] `TASK-003` — Applied global styles through the existing app root without changing templates, route set, content, wording, order, or interactions.
- [x] `TASK-004` — Static accessibility/style review and rendered desktop/narrow-width review complete; see `VER-WI-075-010`.
- [x] `TASK-005` — Existing Playwright journeys, Angular/Vitest suite, and production build completed; see `VER-WI-075-010`.
- [x] `TASK-006` — Reviewed changed-file scope and recorded verification evidence; no visual-regression assets or tests were changed.

## Verification plan

- Review the UI at a representative desktop width on Home, Family, Events, Plans, Trips, and Me/profile. Record the viewport and concise observations; no reference-image pixel matching is required.
- At a narrow width, smoke the shell/navigation, page frame, forms/actions, and statuses for obvious clipping, overlap, or unusable controls only. Do not redesign breakpoints or settle mobile navigation grouping.
- Check focus visibility, keyboard traversal, semantic headings/labels, current-route state without color alone, existing accessible names, key contrast combinations on rendered backgrounds, status meaning without color alone, and action target usability.
- Run the existing `npm run test:e2e` invitation/identity/access Playwright journey. Do not modify tests to depend on layout, DOM order, or pixels. Check WI-075-009 status at execution; if it is `DONE`, run its already-existing Trip audience/access journey without rewriting it. Do not add a substitute Trip E2E journey under this story.
- Run existing Angular/Vitest suite and the Angular production build; record command/results and component-style warning sizes against the pre-change build. Do not suppress warnings; explain any new or worsened warning.
- Record files changed, routes reviewed, findings, remaining screen-specific gaps, and any limitation. No screenshot baseline is an acceptance artifact; diagnostic failure captures remain governed by existing tooling only.

## Risks and blockers

- Shared styles affect multiple routes; overly broad selectors may unintentionally change screen composition. Keep selector scope explicit and review every affected destination.
- Human usability evidence is pending. This story is presentation-only and safe before WI-075-005 only while wording, information hierarchy, action placement/priority, and task flow remain stable; if review suggests structural or wording changes, stop and defer them to a separate story informed by participant evidence.
- `OD-001` remains open. No navigation count, grouping, or mobile composition choice may be made here.
- WI-075-009 was confirmed `DONE / PASS` at execution; its existing Trip audience/access journey was run unchanged under AC-18.
- If an accessible name, stable selector string, privacy explanation, audience meaning, or current Family/access presentation would need to change to achieve visual alignment, stop and request separate review rather than changing it within this story.
- PRP-027 remains `PROPOSED`; this work item creates a bounded candidate story only. Owner review/approval and separate execution authorization are still required.

## WI-075-005 relationship

WI-075-005 remains `PENDING HUMAN TEST`. This shared presentation story may proceed before participant testing only after its own approval and explicit execution authorization, and only if implementation remains within the behavior-neutral scope above. It does not change the human test script, task flow, terminology, screen composition, privacy comprehension, WI-075-005 status, or Phase 7.5 exit assessment. Any finding that points to those changes should be deferred for evidence-informed scope.

## Playwright relationship

Existing Playwright tests continue to protect stable behavior and authorization. Accessible selectors and names remain stable. The tests are not visual approvals; this story adds no screenshot baselines, pixel assertions, layout selectors, or visual-regression infrastructure.

## Execution authorization

- **Approved story:** `YES`
- **Authorized by:** Project owner in the current Codex request
- **Authorized at:** 2026-09-29
- **Authorized scope:** WI-075-010 presentation-only implementation and verification within its listed scope and AC.

## Evidence

| Requirement or criterion | Evidence | Result |
|---|---|---|
| `AC-01`–`AC-21` | `ai-context/planning/verification/VER-WI-075-010.md` | See per-criterion results |

## Verification

- **Verification report:** `ai-context/planning/verification/VER-WI-075-010.md`
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `PASS` — AC-01 through AC-21 passed; AC-17 rendered desktop and narrow-width review is recorded in `VER-WI-075-010`.
- **Verified at:** 2026-09-29


