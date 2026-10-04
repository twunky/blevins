---
Status: DONE / PASS
Authority: Project-owner execution authorization and OWNER VISUAL APPROVAL dated 2026-09-30
Context-Baseline: CTX-1.24
Version: 1.2
Execution-Authorization: YES
---

# WI-075-014 — Plans Visual Alignment

**Repository cleanup relocation note:** The four review captures listed in the execution record were originally under `src/FamilySafety.Web/`. They are retained, with filenames and bytes unchanged, in [artifacts/reviews/WI-075-014/](../../../artifacts/reviews/WI-075-014/). The execution record below preserves their original location.

## Metadata

- **Work Item ID:** `WI-075-014`
- **Status:** `DONE / PASS` — AC-01 through AC-12 pass; owner visual approval recorded.
- **Phase:** `7.5 — End-to-End UX & User-Test Readiness`
- **Priority:** `P2 — Plans screen-specific alignment`
- **Context baseline:** `CTX-1.24`
- **Source proposal:** `PRP-027 — Mockup Alignment & UI Gap Assessment` (`APPROVED` as planning basis)
- **Work type:** Behavior-neutral Plans presentation alignment
- **Model tier:** `STANDARD`
- **Execution authorization:** `YES` — explicitly authorized by the project owner on 2026-09-29 for this reviewed story and scope.
- **Requirement references:** `REQ-UX-003`, `REQ-UX-006`, `REQ-UX-012`; `REQ-PRIV-020`; `REQ-PLAN-001` through `REQ-PLAN-006` as implemented-scope boundaries. No canonical requirement change is proposed.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md`; `ai-context/project-status.md` | Operating rules, active baseline, phase, and status constraints | Entry |
| `ai-context/planning/roadmap.md#phase-75--end-to-end-ux--user-test-readiness` | Preserve Phase 7.5 status and distinguish planning from execution authorization | Before execution and verification |
| `ai-context/planning/proposals/PRP-027-mockup-alignment-ui-gap-assessment.md` | Approved presentation direction and mockup authority/boundaries | Before execution and verification |
| `ai-context/workflows/plans-trips.md#req-plan-001` through `#req-plan-006`; `ai-context/core/privacy-safety-contract.md#req-priv-020`; `ai-context/core/ux-contract.md#req-ux-003`, `#req-ux-006`, `#req-ux-012` | Preserve implemented Plan fields, audience and note visibility, access/privacy boundaries, and accessible presentation | Before execution and verification |
| `ai-context/planning/stories/WI-075-010-align-shared-shell-core-ui-patterns.md`; `ai-context/planning/verification/VER-WI-075-010.md`; `WI-075-011`/`012`/`013` stories and verification reports as needed | Reuse the completed shared visual foundation and established Home, Family, and Events language | Before execution and owner review |
| `ai-context/references/ux/README.md`; applicable Home, Family, and Events reference inventory entries and notes | Apply established design language while respecting reference-only boundaries; no Plans mockup exists | Before execution and owner review |
| `src/FamilySafety.Web/src/app/app.html`; `app.ts`; `app.scss`; `events.scss`; `src/FamilySafety.Web/angular.json`; `src/FamilySafety.Web/src/app/app.spec.ts`; current `src/FamilySafety.Web/e2e` specs and config | Current Plans markup/styles, stylesheet registration/budget, stable selectors, and available regression coverage | Before execution and verification |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| `ai-context/references/ux/plans-trips/trips-sharing.png` and `.md` as a Plans specification | They describe Trips and are not authority for Plan fields, labels, or behavior. No Plans-specific mockup exists. |
| Trips implementation and WI-075-009 access-removal implementation details | Trips is a separate future alignment candidate; no Trips changes are in scope. Consult only if a narrow shared-pattern question requires it, without expanding scope. |
| Backend, domain, API, persistence, database, schema, and authentication implementation | No behavior or data changes are in scope. |
| Phase 8 Safety implementation and future capabilities | Phase 8 remains future and is outside this presentation slice. |
| Screenshot baselines and visual-regression tooling | Explicitly excluded by PRP-027 and this story. |

- **Pinned context dependency:** `None`

## Objective

Make the implemented Plans screen materially clearer to scan by establishing a deliberate relationship between Plan creation and the visible Plan overview, strengthening the hierarchy of current Plan facts and optional details, and improving the presentation of audience, notes, creator context, and existing status states. Preserve all current Plan behavior, fields, privacy/access rules, routes, wording meaning, and accessibility semantics.

Plans has no dedicated mockup. Use the canonical Plans workflow, PRP-027, WI-075-010 shared patterns, established Home/Family/Events design language, and the UX reference inventory boundaries. Do not treat the Trips reference as a Plans specification.

## Current implementation and concrete visual gaps

Plans is a section in the root `app.html` component. Its current creation form includes activity or destination, date and time, optional expected return, audience selection, optional expected check-in, and optional Notes. It explains that Plan audience choices do not start location sharing or send notifications, and that notes follow the selected audience, including future active adult members for whole-group Plans. The overview lists visible Plans with primary timing, optional return/check-in, notes, and creator context. Existing family-group/list loading, no-family, list error, no-visible-Plans, validation, submission, and create success/error feedback are present. Plans has no detail route or post-creation editing UI.

The form and overview currently read as one broadly stacked section with limited Plans-specific composition. Plan summaries reuse Event-oriented `.event-list` presentation. Primary activity and timing, optional details, notes, audience meaning, and creator context need clearer grouping and hierarchy. Audience and note-visibility explanations must remain easy to find. Existing low-data and request states need deliberate local presentation while retaining their distinct meanings. A spacing-only or color-only pass is insufficient.

These findings are based on current source and prior PRP-027/ WI-075-013 review records; they do not claim a new rendered-runtime inspection. The execution story requires desktop and narrow rendered review before acceptance.

## Scope

- Give the existing Plan creation area and visible Plan overview a deliberate, clearly related composition. Exact desktop placement and narrow stacking remain implementation choices.
- Group the existing activity/destination and date/time inputs as primary Plan creation facts; give expected return and expected check-in appropriate optional-detail hierarchy without changing field order or behavior.
- Present the existing audience choices and recipient controls as a coherent, scannable group. Keep the audience explanation truthful and readily discoverable.
- Keep the note-visibility explanation readable near Notes and preserve its existing meaning.
- Make each visible Plan summary materially more scannable using only current fields: activity/destination, primary timing, optional return/check-in, note content, and creator context. Distinguish optional details and Notes from primary facts; give creator context secondary emphasis.
- Give existing family-group loading, Plan-list loading/error, no-visible-Plans/no-Family, form validation, submission, and create success/error feedback clearer local presentation without conflating states or adding actions.
- Use Plans-specific class hooks where needed. Preserve completed WI-075-013 Events presentation; do not change shared Event-oriented styles in ways that materially affect Events.
- Reuse WI-075-010 shared tokens and primitives. Prefer a dedicated `plans.scss` registered through `app.ts` stylesheet metadata if needed. `app.ts` may change only to register the stylesheet and must not change behavior.

## KEEP / ALIGN / ADAPT / DROP

- **KEEP:** Current Plan creation behavior and fields; audience choices, eligibility, and visibility; note visibility; creator context; current loading/error/empty and submission feedback states; routes; wording meaning; accessible semantics; stable selectors.
- **ALIGN:** Hierarchy, grouping, surfaces/cards, spacing, typography, form/action hierarchy, audience presentation, summary presentation, optional-detail and note presentation, status presentation, information density, and overall screen composition.
- **ADAPT:** Existing PRP-027 coordination patterns and shared visual language only to the currently implemented Plan behavior and data.
- **DROP FROM THIS STORY:** No Plan editing, Plan detail routes, post-creation audience changes, completion or check-in workflows, notifications, location sharing, new permissions, unsupported Trip functionality, or other mockup-only capability. The Trips reference is not a Plans specification.

## Non-goals and guardrails

- No Plan domain behavior, fields, audience eligibility, Plan visibility, note visibility, creator rules, routes, APIs, backend, database/schema, authentication, privacy, or authorization changes.
- No Events, Trips, Home, Family, shell/navigation redesign, or resolution of `OD-001`.
- No Plan edit workflow, detail route, audience-change workflow, check-in/completion behavior, notifications, live location, or unimplemented capability.
- No broad stylesheet cleanup, style-budget changes/suppression, unrelated changes to shared Event styles, relocation of shared styles solely to change budget accounting, duplicated shared controls/forms/status/focus primitives, or component library.
- No Phase 8 work, formal participant testing, screenshot goldens, pixel assertions, visual-regression infrastructure, or brittle CSS-class/DOM-position/layout assertions.
- Preserve current accessible names, bindings, and stable selectors. If alignment appears to require behavior/access changes or a shared-style change that materially affects Events, stop and identify separate scope.

## Likely implementation files and styling approach

- `src/FamilySafety.Web/src/app/app.html` — Plans-only composition markup and specific class hooks; preserve current IDs, accessible names, bindings, field order, semantics, and stable selectors.
- `src/FamilySafety.Web/src/app/plans.scss` — new Plans-specific composition rules using shared tokens/primitives.
- `src/FamilySafety.Web/src/app/app.ts` — metadata-only `styleUrls` registration of `plans.scss`, if needed; no behavior changes.
- `src/FamilySafety.Web/src/app/app.spec.ts` — only if focused behavior/accessibility-semantic regression assertions are needed; avoid layout-based assertions.
- Relevant existing Playwright regressions and focused verification documentation after separately authorized execution.

Keep substantial Plans-specific composition out of `app.scss`. Reuse WI-075-010 shared tokens/primitives and do not duplicate shared buttons, forms, status, or focus rules. Plans currently shares some Event-oriented root list styles; use Plans-specific hooks for its composition and preserve WI-075-013 Events unchanged. Do not move or alter shared styles merely to game budgets. If separation requires a shared-style change that materially affects Events, stop and report it before broadening scope.

The current recorded stylesheet baselines are `app.scss` 7.62 kB, `family.scss` 3.74 kB, `events.scss` 3,867 bytes, and `trips.scss` 4.29 kB. Angular `anyComponentStyle` warns at 4 kB and errors at 8 kB. Confirm the effective compiled baselines when execution begins. Reuse shared patterns, keep `plans.scss` below 4 kB if practical, and record final Angular budget output. Do not raise or suppress budgets.

## Acceptance criteria

1. **AC-01 — Presentation-only boundary:** Application changes are limited to Plans presentation markup/styles and metadata-only stylesheet registration if needed, plus focused verification documentation. Plan workflows, fields, data, behavior, privacy/access, routes, APIs, backend/domain/schema, authentication, and other screens remain unchanged.
2. **AC-02 — Material Plans composition:** Rendered desktop presents Plan creation and the visible Plan overview as deliberately grouped areas with a clear relationship and scan hierarchy. The result is a recognizable composition improvement; spacing-only or color-only changes do not satisfy this criterion. Exact placement remains a design choice.
3. **AC-03 — Creation form hierarchy:** Existing activity/destination and date/time read as primary Plan inputs; expected return and expected check-in read as optional details. Audience, Notes, submit action, labels, field order, validation, and enabled behavior remain intact.
4. **AC-04 — Audience and privacy explanations:** Existing audience choices and recipients are easy to scan. The audience explanation and note-visibility explanation remain readily discoverable, readable, and truthful, including the whole-group future-member note visibility rule. No visual treatment implies broader Family access or location/notification capability.
5. **AC-05 — Plan summary scanability:** Every existing visible summary clearly distinguishes its activity/destination and primary timing from available optional return/check-in details, note content, and secondary creator context. Long user-entered content wraps without clipping or obscuring content. No new facts or statuses are introduced.
6. **AC-06 — Notes and optional details:** Optional details are visually subordinate to primary Plan facts, while Notes are distinguishable from both primary facts and creator context. Existing note text remains plain text, retains line breaks as currently rendered, and follows the Plan audience without introducing independent note access.
7. **AC-07 — Low-data and request states:** Existing Family-group loading/no-Family, Plan-list loading/error/no-visible-Plans, validation, submitting, and create success/error states remain truthful, distinct, and understandable in their local context. A loading or failed request is never presented as empty data; no unsupported retry or action is added.
8. **AC-08 — Behavior and stable selectors:** Plan creation, field values/order, audience selection/filtering, ordering, visible data, status behavior, routes, bindings, and current behavior remain unchanged. Existing IDs, accessible names, and stable selectors are preserved; no behavior is added to facilitate styling.
9. **AC-09 — Reference and scope boundary:** The canonical workflow, PRP-027 direction, WI-075-010 patterns, established Home/Family/Events language, and UX inventory boundaries inform presentation. No Plans-specific mockup is claimed and the Trips reference is not used as a Plans specification. No excluded capability is added or implied.
10. **AC-10 — Stylesheet isolation and budget:** Plans-specific composition is scoped to `plans.scss` where practical; `app.ts` changes only to register the stylesheet. Shared tokens/primitives are reused. WI-075-013 Events presentation is preserved; no substantial `app.scss` growth, unrelated cleanup, budget change/suppression, budget-gaming relocation, or duplicated shared primitive is introduced. Baseline/final stylesheet and Angular budget output are recorded; `plans.scss` targets under 4 kB if practical.
11. **AC-11 — Accessibility:** Heading hierarchy, form labels and group semantics, list semantics, keyboard operation, visible focus, accessible action names, status/error announcements, explanation readability, non-color-only meaning, usable control targets, contrast, and logical reading/focus order are preserved or improved. This is a focused check, not full accessibility certification.
12. **AC-12 — Responsive review, verification, and owner approval:** Rendered desktop and narrow/mobile-like review covers a populated list; longer activity/destination; Plans with and without optional details; notes; creation form; audience selection and explanations; no-visible-Plans, loading, error, and representative submit feedback. Narrow layouts may stack/wrap while preserving logical reading/focus order, usable actions, and avoiding clipping or horizontal overflow. Relevant Angular/Vitest and existing Playwright regressions pass; Angular production build and budget output, focused accessibility checks, and rendered review are documented. The owner approves alignment against established PRP-027/Home/Family/Events visual language, not comparison to a nonexistent Plans mockup. Status remains CTX-1.24 active, Phase 7.5 current, PRP-027 approved, WI-075-005/010/011/012/013 done, `OD-001` open, Phase 8 future; this work does not close PRP-027 or Phase 7.5.

## Final acceptance status

| Criterion | Result |
|---|---|
| AC-01 through AC-11 | `PASS` — implementer verification recorded in `VER-WI-075-014.md` |
| AC-12 | `PASS` — implementer verification and project-owner `OWNER VISUAL APPROVAL` recorded in `VER-WI-075-014.md` |

## Tasks

- [x] `TASK-001` — Confirmed current Plans markup, existing fields/states/selectors, shared styles, Events isolation boundary, and approved workflow/privacy constraints.
- [x] `TASK-002` — Reorganized the Plans template using Plans-specific hooks while preserving current field order, bindings, accessible names, copy meaning, and workflow.
- [x] `TASK-003` — Added and registered `plans.scss`; reused established tokens and kept compiled component CSS under the Angular warning threshold.
- [x] `TASK-004` — Rendered populated, long-content, optional-detail, audience, empty, loading, error, and submit-feedback states; reviewed desktop/narrow layout, focus, labels, and Events isolation.
- [x] `TASK-005` — Ran Angular/Vitest, existing Playwright regression, and production build; recorded budget output and verification evidence.
- [x] `TASK-006` — Recorded the project owner's `OWNER VISUAL APPROVAL` for the retained Plans direction and closed the story without adding unprovided observations.

## Verification plan

- Render and review desktop and narrow/mobile-like layouts for every representative state listed in AC-12. Include Plans with long content and distinct combinations of optional return, check-in, and notes.
- Perform focused checks of headings, labels, fieldset/group/list semantics, accessible action names, keyboard and visible focus, status/error announcements, explanation readability, contrast, target usability, logical order, wrapping, clipping, and overflow. Do not claim full accessibility certification.
- Review Plans against established PRP-027 design language and completed Home, Family, and Events patterns. Record owner visual approval and useful patterns adopted; do not claim pixel comparison to a nonexistent Plans reference.
- Run relevant existing Angular/Vitest regression, the existing Playwright regression suite, and Angular production build. Current `app.spec.ts` includes Plan audience/creation and note rendering coverage; Playwright coverage is suite-level regression and is not a Plans-specific visual or workflow assertion. Do not add class, DOM-position, pixel, coordinate, or layout-geometry assertions.
- Record starting/final stylesheet sizes and Angular budget output. Keep `plans.scss` under 4 kB if practical; do not change or suppress budgets.
- Formal participant usability testing is not required.

## Canonical requirement impact

No canonical requirement changes are proposed. Preserve `REQ-PRIV-020` and `REQ-PLAN-001` through `REQ-PLAN-006`, plus applicable UX requirements. Existing rules remain authoritative: selected audience grants access only to that Plan; Plan audience does not authorize location, notifications, profile/Safety information, or a Sharing Session; Plan Notes follow the Plan's audience; whole-group Notes are visible to current active adults including future active members. If visual alignment appears to require behavior/access changes, stop and propose separate scope.

## Relationship to PRP-027 and project status

WI-075-010 completed the shared shell/core-pattern foundation; WI-075-011 Home and WI-075-012 Family/first-use visual alignment are done; WI-075-013 Events Visual Alignment is `DONE / PASS` with owner visual approval. WI-075-014 is the completed bounded screen-specific PRP-027 alignment slice for Plans. It does not duplicate the shared foundation, change other screens, close PRP-027 or Phase 7.5, resolve `OD-001`, or start Phase 8. Trips remains the separate remaining candidate and requires its own planning and authorization.

Preserve `CTX-1.24` as active; Phase 7.5 as `CURRENT / IN PROGRESS`; PRP-027 as `APPROVED`; WI-075-005 and WI-075-010 through WI-075-013 as `DONE`; `OD-001` as `OPEN`; Phase 8 as `FUTURE`.

## Planning-time implementation considerations

- Exact desktop relationship between creation and overview, card/surface emphasis, typography, spacing, and narrow-width stacking remain implementation choices for rendered review and owner comparison.
- Dedicated `plans.scss` registration is supported by the existing root component stylesheet metadata pattern; `app.ts` must remain metadata-only if changed.
- Existing Event-oriented list rules are shared in the root styles. Prefer Plans-specific hooks and preserve Events; avoid broad shared selector changes.
- No product or canonical decision is requested by these presentation choices.

## Pre-execution final story review

- **Review date:** `2026-09-29`
- **Review outcome:** `READY FOR EXECUTION APPROVAL`
- **Reviewed:** Current project status and roadmap; PRP-027; WI-075-010/011/012/013 and verification records; canonical Plans workflow and privacy/UX boundaries; UX reference inventory; current root Plans template, stylesheet registration and styles, Angular budget configuration, Angular/Vitest specs, and current Playwright coverage.
- **Current implementation finding:** Plan creation and visible summaries are implemented in the root App template. Creation includes audience and note explanations; summaries include timing, optional details, notes, and creator. Existing loading/error/empty and submission feedback states are present. No Plan detail route or post-creation editing UI exists.
- **Reference finding:** There is no dedicated Plans mockup. Use canonical workflow and completed shared/Home/Family/Events design language within the UX inventory boundary. Do not use the Trips reference as a Plan specification.
- **Visual-gap finding:** Form and overview read as one broadly stacked section; summaries reuse Event-oriented list styling; primary facts, optional details, notes, audience explanation, creator context, and low-data states need deliberate hierarchy and grouping.
- **Test coverage finding:** `app.spec.ts` covers Plan audience/creation and note presentation among its tests. Existing Playwright coverage is suite-level regression, with no dedicated Plans visual journey identified. Retain stable behavior selectors and do not add layout-based assertions.
- **Style-budget finding:** Recorded baselines are `app.scss` 7.62 kB, `family.scss` 3.74 kB, `events.scss` 3,867 bytes, and `trips.scss` 4.29 kB; Angular component-style warning/error budgets are 4/8 kB. Use Plans-scoped styles, avoid substantial `app.scss` growth, target `plans.scss` below warning if practical, and record effective budget output during execution.
- **Execution disposition:** `DONE / PASS` — all acceptance criteria are satisfied. Project-owner execution authorization was supplied on 2026-09-29 for WI-075-014 as reviewed.
- **Owner visual approval:** `OWNER VISUAL APPROVAL` — on 2026-09-30, the owner stated: “I approve the WI-075-014 Plans visual direction.” No additional owner observations were supplied.

## Execution record

- **Execution authorization:** `YES` — project owner, 2026-09-29, reviewed WI-075-014 scope.
- **Implementation files:** `src/FamilySafety.Web/src/app/app.html`; `app.ts` (stylesheet registration only); new `plans.scss`.
- **Status/context records:** This story; `ai-context/planning/verification/VER-WI-075-014.md`; `ai-context/project-status.md`; `ai-context/planning/roadmap.md`.
- **Rendered review captures:** `src/FamilySafety.Web/plans-review-populated-1440.png`; `plans-review-populated-390.png`; `plans-review-audience-1440.png`; `plans-review-audience-390.png`. Captures use temporary in-memory API fixtures and are retained only for owner visual review.
- **Acceptance criteria:** AC-01 through AC-12 `PASS`; AC-12 owner approval is classified and recorded in `VER-WI-075-014.md`.
- **Automated verification:** Angular/Vitest 5 files / 55 tests passed; existing Playwright regression 2 / 2 passed; Angular production build passed. The new `plans.scss` generated no budget warning. Existing `app.scss` and `trips.scss` warnings remain unchanged.
- **Verification report:** `ai-context/planning/verification/VER-WI-075-014.md`.
- **Work item disposition:** `DONE / PASS`. Phase 7.5 remains current; PRP-027 remains approved/open; no Trips story was created.
