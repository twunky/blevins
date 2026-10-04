---
Status: DONE
Authority: Project-owner approval and explicit execution authorization for WI-075-004
Context-Baseline: CTX-1.24
Version: 1.2
---

# WI-075-004 — MVP Visual Design & Interaction Refinement

## Metadata

- **Work Item ID:** `WI-075-004`
- **Status:** `DONE`
- **Phase:** `7.5 — End-to-End UX & User-Test Readiness`
- **Context baseline:** `CTX-1.24`
- **Proposal:** `PRP-025 — MVP Visual Design & Interaction Refinement`, approved by the project owner for story creation in the current session. The design review and approval are in the session; no PRP-025 proposal file is present in the repository.
- **Execution authorization:** `YES`
- **Execution model tier:** `DEEP` (cross-area presentation with privacy-sensitive audience, Family-context, and first-use regressions)
- **Pinned context dependency:** None.
- **Requirement references:** `REQ-UX-001/002/003/006/007/008/011/012`; `REQ-FAM-005/006/007/008/009`; `REQ-PRIV-005/006/011/013/014/015/020/021/022`; `REQ-EVT-001/005/013`; `REQ-PLAN-001/005/006`; `REQ-TRIP-001/007/008/010/012`.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md`; `ai-context/project-status.md` | Operating authority, current phase/baseline, and execution checkpoint | Entry; before execution |
| Approved PRP-025 design review and project-owner story-creation approval in the current session; `ai-context/planning/roadmap.md#phase-75--end-to-end-ux--user-test-readiness` | Presentation direction, approved story boundary, and Phase 7.5 purpose | Before execution and verification |
| `ai-context/references/ux/README.md` and the seven area references under `home-dashboard/`, `family/`, `events/`, `plans-trips/`, `admin-settings/`, and `safety/` | Non-authoritative visual direction; apply KEEP / ADAPT / DROP against implemented behavior | Before visual work |
| `ai-context/core/ux-contract.md#req-ux-001`–`#req-ux-003`, `#req-ux-006`–`#req-ux-008`, `#req-ux-011`–`#req-ux-012` | Family-first posture, progressive disclosure, accessibility, Home hierarchy, shell context, and prototype status | Before execution and verification |
| `ai-context/core/product-contract.md#req-fam-005`–`#req-fam-009` | Invitation-only entry, controlled first Family, existing adult profile, invitation, and relationship boundaries | Before execution and verification |
| `ai-context/core/privacy-safety-contract.md#req-priv-005`–`#req-priv-006`, `#req-priv-011`, `#req-priv-013`–`#req-priv-015`, `#req-priv-020`–`#req-priv-022` | Existing membership, profile, administrator, relationship, item-audience, Event editor, and Trip creator/access boundaries | Before execution and verification |
| `ai-context/workflows/events.md#req-evt-001`, `#req-evt-005`, `#req-evt-013`; `ai-context/workflows/plans-trips.md#req-plan-001`, `#req-plan-005`–`#req-plan-006`, `#req-trip-001`, `#req-trip-007`–`#req-trip-008`, `#req-trip-010`, `#req-trip-012` | Existing Event/Plan/Trip structure and presentation limits; preserve audience, Notes, and creator behavior | Before execution and verification |
| `ai-context/planning/stories/WI-075-001-invited-adult-first-use-journey.md`; `WI-075-002-returning-user-shell-landing.md`; `WI-075-003-home-and-feature-empty-states.md` and their respective verification reports | Completed first-use, routed shell, Home/empty-state behavior and known visual-verification limits | Before execution |
| `src/FamilySafety.Web/src/app/app.html`, `app.scss`, `development-session.ts`, `trips/trips.html`, `trips/trips.scss`, and `src/styles.scss` | Current UI and styling to refine, including separate Development tools | Before execution |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Bootstrap and archived/superseded context | Historical extraction material does not govern this story |
| Safety, Sharing, and Check-In workflow detail | No capability from those areas is implemented; their visual references are usable only for generic calm presentation |
| Production identity/provider and operator-process decisions | This story does not select or change them |
| Unrelated Phase 5–7 historical implementation records | Current canonical rules and completed Phase 7.5 records define this presentation boundary |

## Goal

Give the working Phase 7.5 MVP a coherent, warm, calm, trustworthy, family-centered visual and interaction layer that is understandable for early user testing on desktop, tablet, and mobile. Refine presentation of existing routes, actions, forms, lists, details, empty states, and status messages without changing workflow order, data, validation, privacy, authorization, or domain behavior.

## Design-reference rule

Mockups under `ai-context/references/ux/` are **non-authoritative** design references for visual direction, layout, information hierarchy, interaction-pattern inspiration, and tone. **KEEP** compatible visual patterns, **ADAPT** useful hierarchy or interaction presentation to current workflows and accessibility, and **DROP** mockup-only capabilities or assumptions. Mockups do not approve workflows, labels, navigation, permissions, fields, states, actions, or domain behavior. The current implementation and canonical contracts govern. The references include no dedicated first-use mockup, so first-use presentation uses the shared visual language without inferring additional steps.

## Boundaries / non-goals

- Preserve current routes and destinations, session-only current Family selection, Family switch isolation, WI-075-003 Home preview count and current-Family Trip boundary, and the existing attention limitation. `OD-001` remains open; this story makes no final mobile-navigation grouping decision.
- Preserve first-use invitation response/handoff, optional profile review, existing Family directory/relationship actions, structural administrator invitations, Event audience/editor access, Plan audience and Notes, Trip creator exception, Trip audience replacement preview/reconfirmation, and Trip Notes permissions.
- Keep Event, Plan, and Trip concepts distinct. Do not remove required fields, alter validation or confirmation, hide a required step, or change the order or meaning of an approved workflow. Progressive disclosure is limited to existing behavior.
- Add no product functionality, APIs, domain rules, persistence changes, or broad architecture/design-system framework.
- Exclude Safety, Sharing Sessions, Check-Ins, notifications, RSVP, Event Needs, Event Updates, Event invitations, production authentication, self-service Family creation/join/discovery, new profile fields, and new Family or administrator permissions.
- Exclude pending-invitation overview/history, member removal, administrator transfer, broad permission management, and private-data access through administrator status. Do not reproduce reference mockups literally.

## Acceptance criteria

1. **`AC-01 — Shared visual vocabulary`**: A small set of reusable styles or tokens defines the page background, surfaces, text, muted text, primary/accent colors, status colors, spacing, typography levels, borders, radii, and restrained shadows. Existing Angular/SCSS remains the presentation stack; no large component framework is added. Key text, focus, and state combinations meet the contrast checks in the verification plan.
2. **`AC-02 — Shell hierarchy`**: Existing routed pages use a consistent centered content frame, recognizable page title, current Family context, and section hierarchy. The shell no longer presents every destination as part of one undifferentiated profile card. Existing routes and Family-context rules remain intact.
3. **`AC-03 — Active destination`**: The current routed destination is visibly identified and programmatically conveyed, including distinct Plan and Trip routes. Keyboard and narrow-screen users can understand the active destination. This presentation does not resolve `OD-001` or alter destination grouping/behavior.
4. **`AC-04 — Family context and Development tools`**: The selected Family and multiple-Family selector are clear in the shell, with zero/multiple/loading/error states still accurate. Development-only identity/outbox tools are visually separate and clearly labeled, retain their existing Development gating, and do not interrupt normal navigation or first-use presentation.
5. **`AC-05 — Home`**: Home gives the selected Family a clear visual anchor and consistently styled Event, Plan, and current-Family Trip previews and destination actions. Its low-data state remains reassuring and scannable. Preview ordering, maximum three per area, current-Family Trip filter, authorized data, and scoped attention wording remain as implemented in WI-075-003; no new Home capability appears.
6. **`AC-06 — Family`**: Family identity, member directory, selected member/profile details, relationships, and contextual administrator entry form distinct, scannable sections. Only currently authorized profile fields, relationship states, and actions are shown.
7. **`AC-07 — Events`**: The Event list/cards, Create Event entry, create/edit sections, visible details, actions, and empty state have consistent hierarchy. Required fields, validation, selected-person view/edit assignment, editor permissions, and existing eligibility remain unchanged. No RSVP, Needs, Updates, Event invitations, Sharing, or Check-In controls appear.
8. **`AC-08 — Plans`**: The Plan list/cards, creation fields, audience choice, Notes, actions, and empty state use the shared presentation patterns while remaining visibly lighter than Trips. Current Plan audience, Notes, required data, and workflow remain unchanged.
9. **`AC-09 — Trips`**: Trip list/cards, create/edit form, and detail organize title/destination/dates, travelers, audience, optional travel information, contacts, and Trip Notes into readable sections. The form retains existing required and optional fields, order, validation, and permitted actions. Traveler status and Trip visibility remain distinct.
10. **`AC-10 — Trip audience edit`**: The existing creator-only audience change, preview, gained/lost recipient display, reconfirmation, errors, and cancellation are easy to scan at desktop and narrow widths. The complete-replacement behavior, departed-creator restrictions, exact preview/reconfirmation rule, server authority, and non-creator disclosure limit remain unchanged; no location/Sharing/Check-In access is implied.
11. **`AC-11 — Me / Settings`**: Existing profile groups, per-field visibility controls, privacy help, Save action, and optional first-use continue path are readable and clearly grouped. Current fields, default visibility, ownership, and save behavior remain unchanged; no account-security or authentication settings appear.
12. **`AC-12 — First-use`**: Invitation preview, Accept/Decline actions, accepted-invitation handoff, optional profile review, and loading/error/retry states use the common page, surface, action, and status treatments. The exact invitation acceptance, membership activation, retry, and Family handoff logic remains unchanged.
13. **`AC-13 — Family administration`**: Existing invite/resend/revoke presentation is clearly tied to the administered current Family, and the structural-role explanation remains near those controls. Eligibility and permissions are unchanged; no invitation overview/history, member removal, administrator transfer, permission dashboard, or private-data access is introduced.
14. **`AC-14 — Action hierarchy`**: Across existing screens, the main commit or next action has a consistent primary treatment; secondary, quiet, cancel, and destructive actions are visually distinct and understandable in text. Navigation controls are distinguishable from form submission. Action names and enabled/disabled conditions retain current meaning.
15. **`AC-15 — Form clarity`**: Event, Plan, Trip, profile, relationship, and invitation forms use consistent labels, grouping, spacing, helper and validation placement, and responsive paired fields. Required fields, privacy choices, fieldsets, validation, and confirmation steps stay available and in their current workflow order. Existing error text remains associated with the relevant control or form.
16. **`AC-16 — Empty and status states`**: Existing no-data, filtered-no-results, loading, success, and error states use consistent visual patterns and accurate text. Loading failures remain distinct from successful empty states; privacy-safe wording and valid existing next actions are preserved. State is not conveyed by color alone.
17. **`AC-17 — Keyboard and readability`**: All existing interactive controls retain semantic names and labels, semantic heading structure, visible keyboard focus, logical traversal, readable text, and touch-friendly targets (aim for at least 44 × 44 CSS px where the control type and layout allow). Focus and status treatments work without inaccessible decorative interactions. Key color combinations meet WCAG AA contrast for their text/control role; no full accessibility certification is claimed.
18. **`AC-18 — Responsive presentation`**: At desktop, tablet, and approximately 390 px viewport width, the shell/navigation, Home, Family directory, Event and Plan forms, Event audience controls, Trip forms and audience editor/preview, Me/profile, invitation flow, cards, lists, and status states remain readable and usable. There is no document-level horizontal overflow, clipped primary action, or control that becomes unusable at those sizes. No final mobile destination decision is inferred.
19. **`AC-19 — Workflow regression`**: Existing route/history behavior, invitation/first-use, profile save, Family relationships, Event create/edit, Plan creation, Trip create/edit/Notes/audience preview-confirmation, and Development identity switching continue to work through the same steps and validations.
20. **`AC-20 — Privacy and authorization regression`**: Existing Family selection/switch isolation, profile field visibility, relationship visibility, admin limits, Event view/edit access, Plan and Trip audiences, current-Family Home Trip scope, Trip creator exception, Trip Notes access, and audience recipient disclosure continue to respect current API/server authorization. Styling and client presentation grant no access.
21. **`AC-21 — Deferred-capability boundary`**: Source/UI review finds no newly surfaced Safety, Sharing Session, Check-In, notification, RSVP, Event Need/Update, production-authentication, self-service Family creation, or new administrator capability; mockup-only features are absent.
22. **`AC-22 — Visual coherence for early user testing`**: A reviewer can identify the current Family, current destination, primary action, item summaries, private-field choices, and available next step across the existing core screens without developer explanation. The result looks intentional and coherent on desktop and mobile without requiring pixel-perfect mockup reproduction.

## Tasks for authorized execution

- [x] `TASK-001` — Reconfirmed `CTX-1.24`, PRP-025 approval, current implementation, and reference KEEP / ADAPT / DROP decisions; no new product/workflow decision was required.
- [x] `TASK-002` — Defined small shared visual tokens/styles and applied consistent page, section, surface, action, form, empty-state, and status patterns through the existing frontend stack.
- [x] `TASK-003` — Refined shell/current Family/navigation/Development-tool presentation and Home while preserving completed behavior.
- [x] `TASK-004` — Refined Family, Event, Plan, Trip, Me / Settings, invitation/first-use, and contextual Family administration presentation within existing actions and data.
- [x] `TASK-005` — Checked responsive and keyboard/contrast behavior and made only presentation fixes within scope.
- [x] `TASK-006` — Ran the bounded verification plan and mapped every `AC-01`–`AC-22` and each traced requirement in `VER-WI-075-004`.

## Verification plan

Story creation claimed no executed checks. Authorized execution completed this plan; the results for every acceptance criterion and requirement group are recorded in `ai-context/planning/verification/VER-WI-075-004.md` as self-verification.

1. Run the existing Angular test suite and add or adjust focused UI checks only where the presentation changes create a meaningful regression risk, especially route active state, Family switch isolation, Home preview scope, and audience/first-use actions.
2. Run the existing .NET/API regression suite with its SQL-enabled checks enabled. Report the exact result and whether SQL-backed tests ran; if the environment prevents the SQL-enabled run, record the cause and limitation explicitly rather than treating a non-SQL run as equivalent. Run a production Angular build and confirm Development-only tooling remains absent from the production path.
3. Perform visual smoke at desktop, tablet, and approximately 390 px mobile width for Home, Family, Events, Plans, Trips, Me / Settings, invitation/handoff, and contextual administration. Check no document-level horizontal overflow, clipped primary actions, touch target usability, and usable Event audience and Trip audience editor layouts. Record screenshots/viewport dimensions or equivalent inspectable evidence.
4. Traverse navigation, forms, audience choices, dialogs/panels if present, and first-use actions with the keyboard. Inspect focus visibility, semantic labels/headings, status text, and touch-target sizing. Check contrast for primary/muted text, links/actions, focused controls, success/warning/error states, and selected navigation against the actual rendered backgrounds.
5. Regress low-data and populated Home, Family directory/relationship states, Event list/create/edit, Plan list/create, Trip list/create/detail/Notes/audience preview/confirm including a stale or changed preview requiring refreshed review, Me/profile save and visibility, invitation accept/decline/membership refresh/profile review or skip/Family and Home handoff/retry, Family invitation administration, and Development identity/outbox separation.
6. Review current-Family switches, out-of-current-Family creator Trips, selected recipient disclosure, administrator eligibility, no-Family/error/empty distinctions, and deferred-feature absence against current authorization and workflow rules. Record any unverified scenario as a limitation rather than a pass.

## Requirement traceability

| Existing requirement | Application in this story | Acceptance criteria |
|---|---|---|
| `REQ-UX-001/002/003` | Family-first, calm, relevant presentation; simpler grouping without new attention, monitoring, or workflow behavior | `AC-01`, `AC-05`, `AC-09`, `AC-16`, `AC-22` |
| `REQ-UX-006/007/008` | Accessible, scannable large-Family content and restrained Home purpose/hierarchy limited to implemented data | `AC-05`, `AC-06`, `AC-17`, `AC-18`, `AC-22` |
| `REQ-UX-011/012` | Current Family shell, open mobile grouping, non-literal visual prototype direction, and responsive presentation | `AC-01`–`AC-05`, `AC-17`, `AC-18` |
| `REQ-FAM-005/006/008`; `REQ-PRIV-011/014` | Invitation-only entry, controlled first Family, invitation preview/response, and no pre-acceptance Family access | `AC-04`, `AC-12`, `AC-13`, `AC-19`–`AC-21` |
| `REQ-FAM-007`; `REQ-PRIV-013` | Current self-profile fields, visibility choices, owner-only edits, and authorized member details | `AC-06`, `AC-11`, `AC-15`, `AC-20` |
| `REQ-FAM-009`; `REQ-PRIV-005/015` | Existing relationship suggestion/acceptance presentation without implied permission | `AC-06`, `AC-15`, `AC-19`, `AC-20` |
| `REQ-PRIV-006` | Structural administration does not grant access to private user data | `AC-06`, `AC-13`, `AC-20`, `AC-21` |
| `REQ-EVT-001/005/013`; `REQ-PRIV-020/021` | Current Event overview/create/edit and audience/editor rules; no deferred coordination or Home attention behavior | `AC-05`, `AC-07`, `AC-15`, `AC-19`–`AC-21` |
| `REQ-PLAN-001/005/006`; `REQ-PRIV-020` | Lightweight Plan presentation, existing audience and Notes, no Trip merge | `AC-05`, `AC-08`, `AC-15`, `AC-19`, `AC-20` |
| `REQ-TRIP-001/007/008/010/012`; `REQ-PRIV-020/022` | Structured Trip fields, audience, creator exception, Notes, and audience change preview/reconfirmation | `AC-05`, `AC-09`, `AC-10`, `AC-15`, `AC-18`–`AC-20` |

These are existing requirements only. Styling needed no new product requirement. No genuine workflow, permission, or product decision was required during execution.

## Implementation notes and retained design choices

- Token values and selected-navigation styling were set to meet contrast and readability goals without changing routes or `OD-001`.
- Shared styles were used through the existing Angular/SCSS stack; no presentation framework or broad component layer was introduced.
- Trip optional details were visually grouped without adding collapse behavior, hiding required controls, or changing the workflow.
- Responsive, keyboard-focus, and key contrast checks were completed and are recorded in `VER-WI-075-004`.
- PRP-025 is approved in the owner session but has no repository proposal file. The approval/design review text remains the proposal authority for this approved work item.

## Execution authorization

- **Approved work item:** `YES`.
- **Execution authorized:** `YES`.
- **Authorized by:** Project owner, explicit WI-075-004 execution approval.
- **Authorized at:** 2026-09-29.
- **Authorized scope:** Execute and verify only WI-075-004 under CTX-1.24; preserve all existing workflows, privacy/authorization rules, routes, validation, and domain behavior.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| Story scope and traceability | PRP-025 design review, owner approval and execution authorization, `CTX-1.24`, current UI, and relevant contracts/workflows | `PASS` |
| Angular UI regression | `npm test -- --watch=false` | `PASS` — 53/53 |
| SQL-enabled .NET/API regression | `RUN_FAMILYSAFETY_SQL_TESTS=1`; `dotnet test FamilySafety.slnx --no-build --no-restore` | `PASS` — 125/125 |
| Production Angular build | Output at `C:\Dev\blevins\wi075004-build-final` | `PASS WITH WARNINGS` — component-style budget warnings documented in verification |
| Responsive/keyboard/contrast visual review | 390×844, 820×1000, and 1440×1000 browser smoke; focus and key palette measurements | `PASS` |
| `AC-01`–`AC-22` and all traced requirements | `ai-context/planning/verification/VER-WI-075-004.md` | `PASS` |

- **Verification report:** `VER-WI-075-004` — `SELF-VERIFIED`, `PASS`.
- **Independence:** Self-verification; independent verification was unavailable.
- **Outcome:** `PASS`.
- **Story readiness:** `DONE` under explicit execution authorization.
