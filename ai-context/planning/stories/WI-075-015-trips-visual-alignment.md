---
Status: DONE — PASS
Authority: Project-owner execution authorization supplied 2026-09-29
Context-Baseline: CTX-1.24
Version: 1.1
Execution-Authorization: YES
---

# WI-075-015 — Trips Visual Alignment

## Metadata

- **Work Item ID:** `WI-075-015`
- **Status:** `DONE` — all acceptance criteria passed and owner visual approval recorded.
- **Phase:** `7.5 — End-to-End UX & User-Test Readiness` (`CURRENT`)
- **Priority:** `P2 — remaining Trips screen-specific PRP-027 alignment`
- **Context baseline:** `CTX-1.24`
- **Source proposal:** `PRP-027 — Mockup Alignment & UI Gap Assessment` (`APPROVED` as planning basis)
- **Work type:** Behavior-neutral Trips presentation alignment
- **Model tier:** `DEEP` for the privacy/access review; presentation implementation remains bounded by this story.
- **Execution authorization:** `YES` — the owner explicitly authorized WI-075-015 execution on 2026-09-29.
- **Requirement references:** `REQ-UX-003`, `REQ-UX-006`, `REQ-UX-012`; `REQ-TRIP-007` through `REQ-TRIP-010`; `REQ-PRIV-020`, `REQ-PRIV-022`. No canonical change is proposed.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md`; `ai-context/project-status.md` | Operating authority, active baseline, and current status | Entry |
| `ai-context/planning/roadmap.md#phase-75--end-to-end-ux--user-test-readiness`; `ai-context/planning/proposals/PRP-027-mockup-alignment-ui-gap-assessment.md` | Phase and approved bounded visual direction | Before execution and verification |
| `ai-context/workflows/plans-trips.md#req-trip-007` through `#req-trip-010`; `ai-context/core/privacy-safety-contract.md#req-priv-020`, `#req-priv-022`; `ai-context/core/ux-contract.md#req-ux-003`, `#req-ux-006`, `#req-ux-012` | Creator, traveler, audience, Notes, privacy, and accessible presentation rules | Before execution and verification |
| `ai-context/planning/stories/WI-075-009-trip-audience-removal-access-enforcement.md`; `ai-context/planning/verification/VER-WI-075-009.md` | Exact removal, retained-access, Notes, and stale-view safeguards and regression evidence | Before execution and verification |
| `ai-context/planning/stories/WI-075-010-align-shared-shell-core-ui-patterns.md`; `ai-context/planning/verification/VER-WI-075-010.md`; `WI-075-014` and its verification as needed | Reuse completed shared visual language and avoid repeating Plans work | Before execution and owner review |
| `ai-context/references/ux/README.md`; `ai-context/references/ux/plans-trips/trips-sharing.png` and `.md` | Trip grouping reference and its non-authoritative boundary | Before execution and owner review |
| `src/FamilySafety.Web/src/app/trips/trips.html`, `trips.ts`, `trips.scss`, `trips.spec.ts`; `src/FamilySafety.Web/e2e/trip-audience-removal.spec.ts`; `src/FamilySafety.Web/angular.json`; shared `src/styles.scss` and `src/app/app.scss` | Current UI, responsive rules, style budget, shared primitives, and stable behavior coverage | Before execution and verification |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Other screen mockups and unrelated screen-specific stories | This is a Trips-only presentation slice; WI-075-010 supplies shared foundations. |
| Backend, domain, API, persistence, database, and authentication implementation | No behavior, authorization, data, or schema change is in scope; inspect a specific backend path only if verification reveals a contract concern. |
| Phase 8 Safety and future Sharing/Check-In implementation | Separate future work; reference imagery cannot authorize it. |
| Screenshot baselines and visual-regression tooling | Excluded by PRP-027; rendered review is human-inspected evidence. |

- **Pinned context dependency:** `None`.

## Objective and current implementation

Make existing Trips materially easier to scan and use across overview, creation, editing, detail, audience change, and Trip Notes. Strengthen hierarchy, grouping, information density, action placement, and request-state presentation while preserving approved behavior, privacy, access, accessible interaction, and the current route.

The current template has a current-Family list and a separate retained-creator list; create/edit forms; primary Trip facts, travelers, audience context, optional travel information, and Notes on detail; a creator-only audience editor with server preview and reconfirmation; and existing empty, loading, error, and busy states. List cards are text-led; detail information is largely a sequence of paragraphs and lists; the long form and nested audience preview have limited visual hierarchy. Notes and optional information have useful sections but need clearer local rhythm. The existing preview has distinct styling and must keep its access consequences prominent. These are source-based planning findings, not claims from a new runtime capture. Desktop and narrow rendered review is required for acceptance.

## Scope and KEEP / ALIGN / ADAPT / DROP

- **KEEP:** All current Trip fields, eligibility and validation, form order, plain-text rendering, current-Family isolation, retained creator access, creator-only core/audience editing, viewer disclosure limit, audience preview and confirmation, Notes capability controls and provenance, optional travel data meaning, routes, labels, accessible names, status meaning, and shared design tokens.
- **ALIGN:** Existing list/card hierarchy; detail sequencing and grouping of primary facts, travelers, audience, optional information, and Notes; form sections, spacing, typography, actions, and status surfaces; audience editor and preview reading order; desktop and narrow composition.
- **ADAPT:** The Trips reference's concise overview and grouped basic/additional facts using current data and behavior. Its Sharing/Check-In separation informs truthful privacy wording only; no sharing control is added.
- **DROP FROM THIS STORY:** Reference-only image cards, filters, avatar chips, new Trip statuses, travel fields, sharing modes, location controls, Check-Ins, Sharing Sessions, quick updates, new audience semantics, and permissions.

Make a material composition change, not a spacing-only or color-only pass. Keep optional details discoverable and retain Notes within the Trip access context. Existing shared shell and other screen presentations remain outside scope.

Keep this as one story because overview, form, detail, Notes, and audience review are states of the same Trips component and share one rendered verification journey. Separating the access-sensitive panel would obscure whether its context and removal consequences remain clear alongside detail and Notes. If implementation discovers a required authorization, request-state, or cross-screen change, stop and propose that separately rather than expanding this visual story.

## WI-075-009 and canonical privacy/access invariants

- A creator retains own-Trip access independently of selected-recipient and eligible Family Group membership; creator authority is not an entry in the selected-recipient list and does not restore group membership or directory access. Creator-only edit and audience controls remain creator-only.
- The creator chooses exactly one Trip audience mode: the Trip's applicable Family Group snapshot or Selected People from that group. An audience change replaces the complete saved recipient set; Group refresh takes a fresh eligible snapshot without individual exceptions. Selected People requires at least one eligible non-creator recipient. Traveler and informational-contact status do not grant Trip or Notes access.
- A departed creator may only reduce the Trip's saved eligible recipient set as allowed by `REQ-PRIV-022`; the UI must not expose the current Family directory or offer expansion operations they cannot perform.
- Before confirmation, the server-authoritative preview must show resulting mode and recipients plus who gains and loses access. The creator confirms that preview with its token. If access effects change before commit, show a refreshed preview and require another confirmation; never silently commit a changed effect.
- WI-075-009 verified A creating with B and C selected, then previewing and confirming removal of B while C remains. A is creator, not a selected recipient. After confirmation, B's authorized list omits the Trip; B's subsequent detail GET, Notes GET, and Note POST return canonical `404`; no post-removal Note is created. A retains list/detail access and can read the original B-authored visible Note with B attribution/content intact. C retains list/detail access and remains the selected recipient.
- Existing Notes retain content, provenance, and state after recipient removal. Removal revokes the person's access; it does not delete Notes they authored or change their attribution. Current Trip authorization governs subsequent Notes reads and mutations. Only authorized visible `ACTIVE` Notes appear normally; the creator may hide another author's active Note but may not rewrite it. A removed recipient gains no access through old authorship. Audience-removal copy and layout must not imply deletion of the removed person's authored Notes.
- A non-creator viewer sees approved audience mode/context, not the Selected People identity list. Audience presentation must not suggest Trip access includes another person's location, separately protected Check-In, Sharing Session, notification, or broader Family access.
- An already-rendered removed-recipient page may persist in memory until a new request or refresh. Subsequent requests must be denied, and a refreshed/refetched authorized list omits the Trip. No real-time push invalidation is required by this story.

## Non-goals and guardrails

- No Trip domain behavior, fields, validation, audience eligibility, access, Notes state/provenance, routes, API, backend, schema, persistence, authentication, or new request behavior solely to create a visual effect.
- No new audience confirmation shortcut, hidden removal consequence, creator/recipient authority blur, or disclosure of recipient identities to non-creators.
- No Family, Home, Events, Plans, shell/navigation redesign, `OD-001` resolution, Phase 8 work, location/Sharing/Check-In controls, notifications, or unsupported travel features.
- No broad shared-style cleanup, budget change/suppression, relocation solely to alter budget reporting, component library, screenshot baselines, pixel/layout assertions, or mandatory formal participant test.
- If rendered review reveals a needed functional/access change, stop and propose separate scope. Existing request feedback may be grouped or styled; adding a new loading/success state requires a separately justified behavior review and must not be assumed by this story.

## Likely implementation files and styling approach

- `src/FamilySafety.Web/src/app/trips/trips.html` — Trips-only composition and class hooks; preserve controls, names, IDs, bindings, conditionals, and meaningful reading order.
- `src/FamilySafety.Web/src/app/trips/trips.scss` — replace or consolidate local layout rules using current tokens and shared primitives. Keep audience preview and Notes visually distinct. Avoid unnecessary growth.
- `src/FamilySafety.Web/src/app/trips/trips.ts` — only if a tightly bounded presentation need can be met without changing access or request semantics; otherwise leave unchanged.
- `src/FamilySafety.Web/src/app/trips/trips.spec.ts` and Trip Playwright spec — verification inputs; add or change behavior assertions only for a concrete regression risk, not for layout.

The current recorded compiled `trips.scss` is 4.29 kB, above Angular's 4 kB component-style warning and below its 8 kB error. The raw SCSS file is 5,128 bytes; these are different measures. Existing Trip fieldsets, grid, list buttons, audience editor/preview, Notes, and breakpoints are useful; no large obsolete block was identified. Refine these rules first. The local `.trip-help` typography and radio/checkbox accent overlap shared `styles.scss` treatment; remove or consolidate a touched declaration only after checking the effective cascade and rendered result. The local `.trip-error` spacing, list button, preview, Notes, and responsive rules have Trips-specific purposes and are not blanket cleanup targets. Add narrow Trips-only layout hooks where necessary, avoid material compiled-size growth, and explain any increase against the baseline. Record baseline and final compiled sizes/build output. Do not split the stylesheet unless a genuine independent component boundary emerges; splitting merely for budget accounting is not a design improvement.

## Acceptance criteria

1. **AC-01 — Presentation boundary:** Only Trips presentation and focused verification change. Existing fields, routes, bindings, ordering, validation, request behavior, privacy/access, backend/API/schema, and other screens remain unchanged. No reference-only capability is added or implied.
2. **AC-02 — Material overview composition:** Rendered desktop and narrow Trips overview clearly distinguishes current-Family and retained-creator sections. Current cards give title, destination, and dates a scannable hierarchy without inventing statuses, imagery, filtering, or new data. Long content wraps.
3. **AC-03 — Detail hierarchy:** Primary title, destination, dates, creator context, and description are easy to scan; travelers, audience, optional travel details, contacts, and Notes have deliberate grouping and order. Existing facts remain readable and discoverable.
4. **AC-04 — Create/edit forms:** Existing required/optional inputs, traveler and audience explanations, arrival time-zone guidance, contacts, field order, validation, cancel, and save behavior remain intact. Form sections and actions are visually coherent at desktop and narrow widths. Audience editing remains separate from core Trip editing.
5. **AC-05 — Audience display and retained access:** Creator and recipient views preserve their different disclosures and controls. Only the creator sees selected recipient identities and Change Audience; recipient viewers see mode/context only. Retained creator Trips remain reachable with the explanation that this does not restore Family membership or directory access.
6. **AC-06 — Audience editor:** The complete-replacement choice, Group snapshot meaning, Selected People minimum, and departed-creator reduction limit remain readable. Current eligibility, mode choices, control names, and available-member disclosure remain unchanged.
7. **AC-07 — Preview and reconfirmation:** Resulting mode and recipients, gains, and losses have an unambiguous reading order; losses/removal consequences remain prominent in text, not color alone. Removing a recipient is described as revoking their Trip and Notes access, not deleting their authored Notes. Preview and Confirm remain separate actions. Stale preview produces a refreshed preview requiring another confirmation, with no visual shortcut or obscured warning.
8. **AC-08 — Notes:** Notes remain inside the authorized Trip context with readable author/time/content and plain-text line breaks. Empty, edit, add, delete, and creator-hide presentation preserves current capability gates, provenance, and server-authorized outcomes; no chat/thread/history capability is implied.
9. **AC-09 — Low-data and feedback states:** Existing list loading/error/retry/empty states, unavailable detail, form validation/save busy/error, audience preview/confirm busy/error, and Notes empty/error/busy feedback remain distinct, truthful, and legible. A failed or pending request is not presented as successful or empty data. No unsupported action is added.
10. **AC-10 — Privacy meaning:** Trip audience, traveler, Family membership, informational contacts, location sharing, Check-In, and Sharing Session remain clearly separate. Presentation cannot imply broader access or weaken removal consequences. WI-075-009 A/B/C safeguards and stale-view limit remain invariant.
11. **AC-11 — Responsive and accessibility:** Rendered desktop and narrow/mobile-like review covers populated/long/empty/error/loading and create/edit/detail/Notes/audience states, including creator and recipient presentations, removal, and reconfirmation. Content and controls wrap without clipping, overlap, or horizontal overflow; removal consequences and confirmation remain visible and usable. Headings, labels, fieldsets, list semantics, accessible names, focus/keyboard order, status/error announcements, privacy explanations near their controls, usable targets, contrast, and non-color-only meaning are preserved or improved. No full accessibility certification is claimed.
12. **AC-12 — Style budget and isolation:** Trips styling reuses WI-075-010 tokens/primitives. Baseline/final compiled size and Angular budget output are recorded. No warning suppression, budget increase, broad cleanup, component library, or budget-gaming split occurs; other screens remain visually unaffected.
13. **AC-13 — Regression:** Relevant Trips Vitest tests and the WI-075-009 Trip audience Playwright scenario pass, along with the existing Playwright regression suite and Angular production build. Existing accessible behavior selectors remain stable; no CSS-position, dimension, pixel, or screenshot baseline assertions are added for visual layout. Add an automated test only for a concrete uncovered behavior risk.
14. **AC-14 — Rendered evidence and owner review:** Desktop/narrow captures and focused accessibility observations are recorded for the representative states below. The owner reviews the final Trips alignment against the reference's compatible grouping and approves the visual result. Verification records applicable status without closing PRP-027 or Phase 7.5, changing CTX-1.24, resolving `OD-001`, or beginning Phase 8.

## Responsive review states

At desktop and narrow/mobile-like widths, inspect: populated list; retained creator list; long title/description/contacts; no Trips in current Family and overall empty state; list loading/error/retry; create form with both audience modes; edit form; detail with travelers, audience, and optional information; Notes empty/populated/edit/actions/error/busy; audience editor in Group and Selected People modes plus departed-creator reduction; preview with gains/losses; stale-preview reconfirmation; removal confirmation; unavailable detail; save and note feedback. Temporary browser-only fixtures may expose rare states if needed; do not treat fixtures as product behavior or retain them as application code.

## Tasks and verification plan

- [x] `TASK-001` — Reconfirm CTX-1.24, current Trips implementation, WI-075-009 behavior, and initial compiled style budget at execution start.
- [x] `TASK-002` — Implement Trips-only markup/style composition within the scope and privacy guardrails.
- [x] `TASK-003` — Render and inspect the representative desktop/narrow states and perform focused accessibility checks.
- [x] `TASK-004` — Run existing behavior regressions and production build; record style budget and any limitations.
- [x] `TASK-005` — Owner visual approval and criterion-level verification are recorded in `VER-WI-075-015`.

Verification must compare with `trips-sharing.png` only for compatible hierarchy/grouping. Inspect labels, fieldsets, headings, focus, keyboard order, announcements, contrast, target usability, long-text wrapping, and access meaning. The existing Trips Vitest spec covers Family isolation, retained creator access, creation payload, viewer disclosure, safe text, core edit boundary, preview/reconfirmation, departed-creator limits, validation, and Notes actions. WI-075-009 Playwright covers A/B/C creation, removal, Notes, server denial, and retained access; it has no visual or responsive assertions. Existing .NET Trip tests protect service/API/persistence behavior. Record these as behavior evidence, not human visual evidence. Formal participant usability testing is not a prerequisite for this story.

## Canonical requirement impact and status

No canonical requirement change is proposed. `REQ-TRIP-007` through `REQ-TRIP-010`, `REQ-PRIV-020`, `REQ-PRIV-022`, and applicable UX requirements remain authoritative. The reference cannot grant location, Sharing, Check-In, or broader audience access. A discovered need for new behavior or changed access requires a separate proposal.

This was the remaining known screen-specific PRP-027 candidate after WI-075-010 through WI-075-014. WI-075-015 is now DONE / PASS. WI-075-005 remains DONE with its documented evidence limitations. `CTX-1.24` stays active; PRP-027 approved; Phase 7.5 current; `OD-001` open; Phase 8 future. This story completion does not close PRP-027 or Phase 7.5.

## Pre-execution final story review

- **Review outcome:** `READY FOR EXECUTION APPROVAL` — the story remains `PROPOSED` and execution authorization remains `NO` until separately granted.
- **Reviewed:** Current Trips template/component/SCSS and eleven Trips Vitest cases; WI-075-009 story, verification, and Trip Playwright scenario; relevant Trip API/Notes tests and invitation Playwright regression; PRP-027, WI-075-010 through WI-075-014, status/roadmap, canonical Trip/privacy/UX rules, and the Trips reference with its authority warning.
- **Boundary decision:** One Trips story is reasonable because all visual states share the Trips component and audience/Notes context. Any discovered behavior, authorization, or unrelated shared-style need leaves this scope.
- **Corrections:** Made authored-Note preservation explicit in removal presentation, strengthened narrow-width review of removal/reconfirmation and creator/recipient views, and named safe stylesheet cleanup candidates and limits.
- **Coverage decision:** Existing behavior tests plus rendered desktop/narrow review are sufficient for presentation-only work. Add no layout test; add a behavior test only for a concrete uncovered regression introduced during execution.
- **Recommended execution tier:** `DEEP` with high reasoning effort for privacy-sensitive template review and verification. Keep edits local and use the established shared tokens; do not use this tier to expand scope.
- **Remaining implementation choices:** Exact surface composition, spacing, and stacking are resolved by rendered review and owner comparison. Current request states may be styled or grouped; new loading/success behavior is outside this approval-ready visual scope. `OD-001` remains open.

## Execution authorization and evidence

- **Approved work item:** `YES` — project owner authorized execution of the reviewed v1.1 story.
- **Execution authorized:** `YES`.
- **Authorized by / at:** Project owner, 2026-09-29.
- **Authorized scope:** WI-075-015 Trips-only presentation and verification, per execution request.
- **Verification report:** `ai-context/planning/verification/VER-WI-075-015.md`.
- **Outcome:** `DONE / PASS` — all criteria passed; project owner supplied `OWNER VISUAL APPROVAL` for AC-14 on 2026-09-29.
