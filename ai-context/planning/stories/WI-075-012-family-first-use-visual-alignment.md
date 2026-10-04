---
Status: DONE / PASS
Authority: Project-owner execution authorization and OWNER VISUAL APPROVAL recorded 2026-09-29
Context-Baseline: CTX-1.24
Version: 1.2
Execution-Authorization: YES
---

# WI-075-012 — Family and First-Use Visual Alignment

## Metadata

- **Work Item ID:** `WI-075-012`
- **Status:** `DONE / PASS`
- **Phase:** `7.5 — End-to-End UX & User-Test Readiness` (`CURRENT`)
- **Priority:** `P2 — Family and first-use screen-specific alignment`
- **Context baseline:** `CTX-1.24`
- **Source proposal:** `PRP-027 — Mockup Alignment & UI Gap Assessment` (`APPROVED` as the planning basis)
- **Work type:** Behavior-neutral presentation refinement
- **Model tier:** `STANDARD`
- **Execution authorization:** `YES` — explicitly granted by the project owner on 2026-09-29 for this reviewed story.
- **Requirement references:** `REQ-UX-001`, `REQ-UX-003`, `REQ-UX-006`, `REQ-UX-011`, `REQ-UX-012`; `REQ-FAM-005` through `REQ-FAM-009`; `REQ-PRIV-006`, `REQ-PRIV-011`, `REQ-PRIV-013` through `REQ-PRIV-015`. These requirements constrain current membership, profile fields/visibility, adult invitations, relationship actions, consent, and access; they are not change targets.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md`; `ai-context/project-status.md` | Operating authority, phase/baseline status, and work authorization boundary | Entry |
| `ai-context/planning/proposals/PRP-027-mockup-alignment-ui-gap-assessment.md` | Approved visual-alignment basis, KEEP/ALIGN/ADAPT/DROP boundary, and verification limits | Before execution and verification |
| `ai-context/planning/roadmap.md#phase-75--end-to-end-ux--user-test-readiness`; `ai-context/planning/decision-log.md#open-product-decisions` | Preserve Phase 7.5 status and `OD-001`; keep Phase 8 future | Before execution and verification |
| `ai-context/core/ux-contract.md#req-ux-001`, `#req-ux-003`, `#req-ux-006`, `#req-ux-011`, `#req-ux-012`; `ai-context/core/product-contract.md#req-fam-005` through `#req-fam-009`; `ai-context/core/privacy-safety-contract.md#req-priv-006`, `#req-priv-011`, `#req-priv-013`, `#req-priv-014`, `#req-priv-015` | Family-first and accessible presentation, invitation-only membership, approved profile fields, invitation/relationship workflows, Family-context behavior, and privacy/access boundaries | Before execution and verification |
| `ai-context/references/ux/README.md`; `ai-context/references/ux/family/family-profiles-overview.md` and `.png`; `ai-context/references/ux/admin-settings/admin-settings-overview.md` and `.png` | Primary and secondary visual direction and explicit non-authoritative/exploratory boundaries | Before execution and owner review |
| `ai-context/planning/stories/WI-075-010-align-shared-shell-core-ui-patterns.md`; `ai-context/planning/verification/VER-WI-075-010.md`; `ai-context/planning/stories/WI-075-011-home-visual-alignment.md`; `ai-context/planning/verification/VER-WI-075-011.md` | Reuse completed shared patterns, preserve adjacent Home behavior, and honor the recorded style-budget constraint | Before execution |
| `src/FamilySafety.Web/src/app/app.html`; `app.scss`; `app.ts`; `styles.scss`; `app.spec.ts`; `first-use.spec.ts`; existing invitation-handoff Playwright spec and Angular build configuration | Current Family/first-use markup, root component style registration, shared tokens, stable selectors, relevant behavior regressions, and actual style budget application | Before execution and verification |

## Context intentionally not loaded

| Context or capability | Why it is excluded |
|---|---|
| Safety, location, check-in, trusted-contact, and Safety Circle workflows | Not part of this presentation slice; no such capability is authorized |
| Groups/households, minor profiles, additional profile fields, invitation history, or broader administration | Exploratory or unapproved mockup concepts |
| Backend/domain/database/authentication implementation | Presentation only; behavior and data contracts do not change |
| Events, Plans, Trips, and Home screen-specific implementation | They are outside this story; only the existing Home-to-Family suggestion entry remains a regression constraint |
| Screenshot goldens and visual-regression infrastructure | Explicitly excluded by PRP-027 and the request |

- **Pinned context dependency:** `None`

## Objective

Make the implemented Family and first-use presentation easier to scan and understand, and materially closer to the useful visual direction of the approved Family references. Preserve all current content, workflows, data, privacy/access rules, navigation, and accessibility semantics. Treat references as inspiration, not specifications.

## Current implementation and visual gaps

The Family experience is rendered in the root `app.html` template. The Family view has a membership/context statement, active-adult directory with name filtering and selection, selected profile details, relationship proposal controls and accepted/incoming-pending/outgoing-pending states, and conditional administrator invitation controls. Profile details may include email, phone, and birthday only when supplied for sharing; the existing no-optional-details message is also rendered. Privacy explanations are present near directory, relationship, and administrator areas.

The first-use path in the same root component includes invitation preview with Accept/Decline, status and error/retry handling, optional profile review, and an accepted-invitation handoff. These use the shared generic focused-state surface. The UI also presents current Family identity, while actual selection remains in the shell and obeys `CTX-1.24`.

The implementation already has working behavior and shared visual primitives. Specific presentation opportunities are: directory entries read as simple full-width buttons; selected member details need stronger distinction from the directory; optional profile details and the no-details state can be grouped more clearly; accepted, incoming-pending, and outgoing-pending relationships need clearer visual differentiation beyond shared card treatment; directory, profile, relationships, and administration need a more deliberate hierarchy; and invitation preview/profile review/handoff can have clearer surfaces while retaining the current steps and wording. These are planning observations from source review and the prior PRP-027 assessment, not claims of a new independent runtime inspection or defect.

The Family UI and Home UI share the root template and `app.scss`. The last recorded build reports `app.scss` at **7.62 kB** against a **4 kB warning** and **8 kB error** threshold. This leaves about **0.38 kB** before the error threshold. Current Angular configuration sets the `anyComponentStyle` budget to 4 kB warning / 8 kB error. In the installed `@angular/build` application builder, component stylesheet resources are separately bundled and marked as `ng-component`; the budget calculator evaluates each emitted component-style asset separately. A second file registered via component `styleUrls` therefore has its own per-stylesheet budget measurement rather than being concatenated into the `app.scss` measurement. Both styles are still delivered/applied for the same root component, so the sum of their sizes is not independently constrained by this budget.

## Scope

- Improve Family-directory entry grouping and scanning while retaining name filtering, current list semantics, member selection, order, and actions.
- Make selected-member presentation visually distinct from the directory and group only the profile fields already returned by current behavior, including the existing no-optional-details state.
- Visually distinguish accepted, incoming-pending, and outgoing-pending relationship states using existing text/actions and non-color cues. Keep proposal, accept, decline, withdraw, removal, and confirmation behavior intact.
- Clarify the hierarchy among Family identity/privacy context, directory, selected profile, relationships, and existing structural-administration controls.
- Present the existing administrator invitation controls clearly, preserving the current role eligibility, explanatory privacy text, actions, and placement within the Family view.
- Give the existing invitation preview, profile-review step, and accepted-invitation handoff a clearer visual composition without adding, removing, reordering, or renaming steps or actions.
- Retain all current information and action order in the accessible reading sequence. Any visual layout reordering must not reorder focus/reading order.
- Reuse WI-075-010 tokens and shared surfaces, controls, button, status, and focus primitives. Keep shared-style changes minimal.

## KEEP / ALIGN / ADAPT / DROP

- **KEEP:** Directory and selection behavior; current profile fields and sharing/privacy rules; all relationship states/actions; invitation and profile-review workflows; accepted-invitation handoff; Family context behavior; administrator eligibility/authority; navigation; accessibility semantics; stable selectors.
- **ALIGN:** Visual hierarchy, grouping, spacing, surfaces/cards, forms/actions, typography, relationship/status presentation, screen composition, and information density.
- **ADAPT:** Directory/profile separation, compact member grouping, nearby privacy context, relationship grouping, and invitation/profile-review surfaces, using only implemented data and actions.
- **DROP:** Minor profiles, extra profile fields, households/groups, Safety Circle, trusted contacts, broader family management, invitation history, roles/permissions management, Safety/location/check-ins, and all other unapproved mockup capabilities.

## Non-goals and guardrails

- No changes to Family-context selection/retention/state clearing, profile fields/visibility, privacy logic, relationship or invitation logic, administrator permissions, routes, APIs, domain behavior, database schema, or authentication.
- No new onboarding step, invitation capability, state, copy, data, or feature.
- No shell/navigation redesign; `OD-001` remains open.
- No Home, Events, Plans, or Trips redesign. Keep the existing Home relationship-suggestion handoff behavior unchanged.
- No Safety, location, check-in, or sharing feature.
- No component library, broad unrelated stylesheet cleanup, style-budget increase, or warning suppression.
- No screenshot goldens, pixel assertions, CSS-class selectors, DOM-position selectors, or brittle layout assertions.
- No formal participant usability testing requirement.
- If achieving the presentation goal requires a behavior, privacy, authorization, data, wording, or workflow change, stop and propose separate scope.

## Expected file impact and styling approach

Likely files:

- `src/FamilySafety.Web/src/app/app.html` — narrowly scoped presentation markup/class hooks while preserving labels, text, IDs, accessible names, bindings, and order.
- `src/FamilySafety.Web/src/app/app.scss` — retain as-is except for a directly necessary, very small shared-style correction reviewed separately; do not add Family composition rules here or disturb Home styling.
- `src/FamilySafety.Web/src/app/family.scss` — approved home for Family and first-use composition rules, using only Family/first-use-specific class hooks and existing shared CSS custom-property tokens/primitives.
- `src/FamilySafety.Web/src/app/app.ts` — change component metadata from singular `styleUrl: './app.scss'` to `styleUrls: ['./app.scss', './family.scss']` to register the additional encapsulated component stylesheet; no behavior changes.
- Existing Angular/Vitest tests and existing invitation-handoff Playwright tests are regression targets; selector changes are not expected or permitted merely to accommodate styling.

The builder was inspected during final story review. Its `anyComponentStyle` threshold is applied to each emitted component CSS asset separately, so the current 7.62 kB `app.scss` warning remains but is not made worse by Family composition additions; `family.scss` will have its own size check. Keep `family.scss` below the 4 kB warning threshold where practicable, do not approach the 8 kB error threshold, and avoid treating the separate measurement as permission to grow aggregate root-component CSS without restraint. Angular's root-component style encapsulation applies to both registered component stylesheets. Do not raise/suppress budgets, move shared primitives, or perform broad style refactoring. Record the starting and final sizes/warnings for both styles in verification. This small `styleUrls` metadata change and new screen-specific stylesheet are in scope.

## Acceptance criteria

1. **`AC-01 — Presentation-only boundary`:** Changes are limited to Family/first-use markup and styling, the root component's stylesheet registration metadata needed for `family.scss`, and focused verification documentation. Routes, workflows, content/data contracts, profile fields, privacy/access, navigation, backend/domain/schema, and product behavior remain unchanged.
2. **`AC-02 — Material Family composition`:** Rendered desktop Family presents the existing Family context, member directory, selected profile, relationships, and conditional administration as a deliberately grouped hierarchy. Owner review confirms a recognizable composition improvement over a sequence of similarly weighted sections; exact dimensions and columns remain design choices.
3. **`AC-03 — Directory scanability`:** Existing active adult members and name-filter controls form a clear, scannable directory. Each member remains operable by keyboard and pointer; member names, filtering behavior, list semantics, result order, and stable accessible names remain unchanged.
4. **`AC-04 — Selected profile distinction`:** Selecting a member presents their existing optional details in a clearly distinct profile surface, visually related to the directory. Only currently available fields render; no field or value is invented or disclosed more broadly.
5. **`AC-05 — Low-data profile and directory`:** Empty directory, no filter matches, a single-member directory, and a member with no optional shared details remain truthful and understandable. Existing explanatory content/actions remain present with improved grouping where useful; empty, no-match, and load/error states are not conflated.
6. **`AC-06 — Relationship-state differentiation`:** Accepted, incoming-pending, and outgoing-pending relationships are visually distinguishable through hierarchy, labels, grouping, or other non-color cues while retaining existing textual meaning and state-specific actions. No urgency, authority, or new relationship meaning is implied.
7. **`AC-07 — Relationship guidance and privacy`:** Existing guidance about suggestion visibility and the fact that relationships do not change access remains readable near the relationship controls/states. Text, visibility rules, and actions are unchanged.
8. **`AC-08 — Family identity and privacy context`:** Current Family identity and existing membership/access explanation remain clear within the Family view. Styling does not change selector state, selection, session retention, access, or Family-scoped data clearing/reload behavior.
9. **`AC-09 — Structural administration`:** Existing invitation controls read as a distinct structural-administration area with privacy explanation nearby. Eligibility, enabled states, accessible names, invitation actions, and the rule that administrator status grants no private-data access remain unchanged.
10. **`AC-10 — First-use surfaces`:** Existing invitation preview, profile review, and accepted-invitation handoff each receive a coherent, legible presentation appropriate to their current content. Current wording, step sequence, actions, error/status/retry states, and handoff behavior remain unchanged; no onboarding chrome implies extra steps.
11. **`AC-11 — Shared style reuse and budget`:** Existing WI-075-010 semantic tokens and shared control/surface/action/status/focus patterns are reused. Family-specific composition is kept out of substantial `app.scss` growth; any separate stylesheet is scoped and its budget accounting is confirmed. No budget is raised/suppressed and no broad cleanup is introduced. Any unavoidable budget warning change is recorded and explained.
12. **`AC-12 — Accessibility and stable selectors`:** Heading hierarchy, labels, list/form semantics, accessible names, keyboard traversal, visible focus, textual status meaning, non-color-only relationship distinctions, nearby privacy information, target usability, relevant contrast, and stable selectors are preserved or improved. No full accessibility certification is claimed.
13. **`AC-13 — Responsive rendered review`:** Rendered desktop and narrow/mobile-like reviews cover populated directory, selected profile, no optional details, incoming/outgoing pending and accepted relationship states, invitation preview, profile review, and relevant empty/no-match states. Narrow layouts may stack/wrap in reading order; there is no clipping, overlap, horizontal overflow, or obscured control. No navigation redesign is introduced.
14. **`AC-14 — Regression, owner review, and status`:** Relevant existing Angular/Vitest tests, existing Playwright regression, and Angular production build are run and recorded. Owner visual review against the Family references is recorded. No canonical requirement changes; `CTX-1.24` remains active, Phase 7.5 remains current, PRP-027 remains approved as planning basis, WI-075-005/010/011 remain done, `OD-001` remains open, and Phase 8 remains future. This story's completion would not itself close PRP-027 or Phase 7.5.

## Verification plan

- Review rendered Family at representative desktop and narrow/mobile-like widths, using actual Family data and relevant populated/low-data states. Inspect current Family context, directory and selected profile, no optional details, all existing relationship states, and conditional administrator area.
- Review existing invitation preview, profile review, and accepted-invitation handoff surfaces at both widths. Preserve current wording, sequence, actions, statuses, errors, and retry.
- Perform focused checks of heading order, labels, list semantics, accessible names, keyboard traversal, visible focus, textual status, relationship state without color alone, privacy context placement, relevant contrast, target usability, wrapping, and overflow. Do not claim full accessibility certification.
- Obtain and record owner visual review against the primary Family reference and secondary admin/settings reference; references guide presentation and do not authorize depicted capabilities.
- Run relevant existing Angular/Vitest regression, existing Playwright regression without selector/layout assertions, and the Angular production build. Record commands/outcomes and style-budget starting/final sizes. Do not add screenshot goldens or pixel/layout assertions.
- Record modified files, rendered states/viewports, accessibility checks, regression/build outcomes, owner decision, remaining limitations, and status preservation in a verification report only after separately authorized execution.

## Risks and unresolved implementation decisions

- The root Angular component owns the broad `app.html` view set. Keep Family/first-use selectors explicitly scoped and verify that the second stylesheet remains an independent component-style output with no unintended style leakage during the production build/rendered review.
- A composition change must remain recognizable rather than collapsing into superficial spacing-only edits, while preserving information/focus order and stable selectors.
- Relationship status has privacy meaning. Distinguish states visually without exposing pending suggestions to anyone currently unauthorized or implying that acceptance grants access/authority.
- Do not infer a design direction from exploratory fields/features in either reference. Only current rendered fields and approved actions are in scope.
- Precise card arrangement, typography within the existing hierarchy, local spacing, and surface emphasis remain implementation decisions for rendered review and owner comparison. They do not require a product decision while the behavior boundary is respected.
- Avoid growing `app.scss` for Family composition. The approved approach is `family.scss` registered as a second encapsulated root-component stylesheet; do not refactor unrelated app styles if an implementation detail needs adjustment.

## PRP-027 relationship and project status

This was the next screen-specific PRP-027 slice after `WI-075-010` shared shell/core patterns and `WI-075-011` Home alignment. It addresses Family and first-use gaps identified by PRP-027 without implementing mockup-only features. `PRP-027` remains `APPROVED` as planning basis. Execution was explicitly authorized and owner visual approval recorded by the project owner on 2026-09-29. This completion does not close PRP-027 or Phase 7.5, or authorize another story.

## Final story review

- **Review date:** `2026-09-29`
- **Review outcome:** `READY FOR EXECUTION APPROVAL`
- **Reviewed:** Current root template/component metadata/styles, relevant Family and first-use Angular specs, invitation-handoff Playwright scenario, production Angular builder configuration/source, project status/roadmap, PRP-027, WI-075-010/011, canonical privacy/UX anchors, and primary/secondary UX reference notes/images.
- **Styling finding:** The Angular application builder emits each component stylesheet resource as a component-style CSS asset; the configured `anyComponentStyle` budget checks each such asset independently. `family.scss` registered with `styleUrls: ['./app.scss', './family.scss']` is a clean, supported approach and will not be added to the `app.scss` asset-size measurement. Both styles apply to the same component and their aggregate is not separately bounded, so keep `family.scss` compact (target below 4 kB) and do not use this split to relocate or grow shared styles. No broad structural refactor is needed.
- **Corrections made:** Replaced conditional styling language with the specific approved `family.scss`/`styleUrls` approach; recorded per-asset budget behavior and aggregate-size caveat; limited `app.scss` changes to a directly necessary, separately reviewed shared correction; clarified expected `app.ts` metadata-only impact; added the canonical adult membership, profile privacy, invitation, and relationship requirement anchors to the manifest.
- **Review state at that time:** `READY FOR EXECUTION APPROVAL`; authorization was subsequently granted as recorded below.
- **Execution routing:** `STANDARD` for the bounded presentation work; escalate if privacy/access, behavior, or cross-contract issues are discovered.
- **Approval at final story review:** `NOT RECORDED` then; subsequently granted for execution on 2026-09-29.
- **Execution authorization at final story review:** `NO` then; now `YES`.

## Execution and verification checkpoint

- **Execution authorization:** Explicit project-owner request dated 2026-09-29, limited to this reviewed story.
- **Implementation:** Family/first-use presentation wrappers and classes in `app.html`; scoped `family.scss`; metadata-only `styleUrls` registration in `app.ts`. No behavior, backend, domain, schema, authentication, canonical requirement, or shared-budget change.
- **Verification:** `VER-WI-075-012` records rendered desktop/narrow review, low-data states, focused accessibility, 55 passing Angular/Vitest tests, 2 passing existing Playwright scenarios on isolated ports, production build, and stylesheet sizes.
- **Current status:** `DONE / PASS`. AC-01 through AC-14 pass. Verification: `ai-context/planning/verification/VER-WI-075-012.md`.

## Owner visual approval and closure

- **Date:** `2026-09-29`
- **Evidence classification:** `OWNER VISUAL APPROVAL`
- **Owner decision:** “I approve the WI-075-012 Family and first-use visual direction.” The owner authorized AC-02 PASS and the owner-review portion of AC-14 PASS.
- **Rendered review / comparison:** Performed by Codex and recorded in `VER-WI-075-012` with the retained desktop/narrow captures.
- **Additional owner observations:** None recorded or inferred.
- **Closure:** AC-01 through AC-14 PASS; `VER-WI-075-012` PASS; WI-075-012 DONE / PASS.

