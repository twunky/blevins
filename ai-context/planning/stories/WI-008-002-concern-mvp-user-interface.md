# WI-008-002 — Concern MVP User Interface

## Metadata

- **Status:** `DONE / PASS` — all 26 acceptance criteria passed; `OWNER VISUAL APPROVAL` supplied by the project owner on 2026-10-01 (`VER-WI-008-002`)
- **Context baseline:** `CTX-1.25` (active)
- **Phase:** Phase 8 — Safety Coordination (`CURRENT / IN PROGRESS`)
- **Depends on:** `WI-008-001` (`DONE / PASS`; `VER-WI-008-001`)
- **Authority:** `PRP-028` (`APPROVED` under `DEC-067` and `DEC-068`); `DEC-069` is server-side audit implementation behavior only
- **Execution authorization:** `YES` — limited to this Concern UI story
- **Work mode:** Authorized Angular UI implementation and verification only
- **Model tier:** `DEEP` — safety, privacy, authorization presentation, and user history

## Required context

| Source | Why it is required |
|---|---|
| `ai-context/project-status.md`; `ai-context/planning/roadmap.md` | Confirm active baseline, Phase 8 state, OD decisions, and WI-008-001 status |
| `ai-context/planning/candidates/CTX-1.25-phase-8-concern-access-and-retention.md`; `ai-context/planning/proposals/PRP-028-phase-8-concern-access-and-lifecycle.md`; `ai-context/decision-log.md` (`DEC-067` through `DEC-069`) | Approved Concern policy, retention, Phase 8 direction, and implementation clarification |
| `ai-context/planning/stories/WI-008-001-concern-domain-and-access-foundation.md`; `ai-context/planning/verification/VER-WI-008-001.md` | Server contract, completed scope, evidence, and known deployment dependency |
| `ai-context/core/product-contract.md` (`REQ-PROD-004`, `REQ-PROD-007` through `012`); `ai-context/core/privacy-safety-contract.md` (`REQ-PRIV-005` through `008`, `011`, `025`) | Product boundaries, permissions, protected data, and privacy behavior |
| `ai-context/core/ux-contract.md` (`REQ-UX-001`, `003` through `008`, `010` through `012`); `ai-context/workflows/safety.md` (`REQ-SAFE-001` through `006`, `015`) | Calm presentation, accessibility, uncertainty, provenance, and Concern lifecycle |
| `ai-context/core/requirements-index.md` | Requirement traceability; no new requirement is proposed |
| `src/FamilySafety.Api/Concerns/ConcernEndpoints.cs`; `src/FamilySafety.Application/Concerns/ConcernContracts.cs`; `src/FamilySafety.Application/Concerns/ConcernService.cs` | Exact API routes, request/response shapes, validation, and unavailable behavior |
| `src/FamilySafety.Api/Directory/FamilyDirectoryEndpoints.cs`; `src/FamilySafety.Application/Directory/FamilyDirectoryService.cs`; `src/FamilySafety.Api/Profiles/AdultProfileEndpoints.cs` | Existing active-adult directory and own-profile sources for explicit selectors |
| `src/FamilySafety.Web/src/app/app.routes.ts`; `app.ts`; `app.html`; `trips/` and current Home/Family/Events/Plans styles and templates | Existing Angular route, shell, responsive, form, card, and state patterns |

## Goal

Provide a minimal functional Concern workflow in the existing Angular application. An authorized user can discover only server-returned Concerns, create a Concern with an explicitly selected audience, review authorized detail/history, contribute observations and explicit Unknowns, append a correction to their own entry, append a challenge to another entry, manage participants as creator while Open, and resolve as creator while Open.

The UI consumes the verified private API and its server-side authorization. Client state may guide which controls are shown; it is never an access-control boundary. This story does not redesign server permissions.

## Story-size decision

Keep list, create, detail, contribution, correction/challenge, participant management, and resolution in **one coherent story**. The flows share the same authorized aggregate, state model, and history presentation; separating list/create/detail would create artificial dependencies and leave no usable Concern workflow at intermediate completion. The scope is bounded to one feature area and the six current API actions, with one integration journey and screen-state coverage.

## Exact scope

- Add an Angular Concern list and detail workflow backed by the existing API.
- Add creation with same-Family adult subject selection, self-Concern support, required reason/context, and an explicit optional participant audience.
- Add entry contribution for Observation and explicit Unknown, including category, text, source or explicitly unknown source, and optional event time.
- Add append-only own-entry correction and distinct attributed challenge actions with required explanation and target linkage.
- Add whole-audience participant replacement for the creator while Open and explicit creator resolution with a required note.
- Render the Open, Frozen, and Resolved states and their read/write consequences.
- Handle server validation, unauthorized session, nondisclosing unavailable responses, refresh, stale access, loading, and empty states.
- Reuse the established shell, forms, buttons, cards, status surfaces, typography, spacing, and calm Phase 7.5 styling patterns.
- Keep the current desktop navigation unchanged. Add a `/concerns` route and a clear Home entry point to it; do not add a new global/mobile destination or decide `OD-001`.
- Keep selectors limited to the existing current-profile endpoint and active adult Family directory endpoint; do not request profile-private fields.

### Expected Angular implementation surface

- Add a focused standalone Concern feature component under `src/FamilySafety.Web/src/app/concerns/` (component, template, scoped stylesheet, and component spec) plus a small typed API service/model module for the existing Concern and directory calls.
- Extend `app.routes.ts` and the existing `App` shell's view/title/Home entry in `app.ts` and `app.html`; reuse existing Home card styles where practical. Keep feature behavior and presentation out of the already broad shell component.
- Add one Playwright spec under `src/FamilySafety.Web/e2e/`. Reuse current isolated invitation scenario and optional Adult C fixture; a .NET fixture or API change is not expected.
- Exact file names and whether the Home link is a button or route link may follow current Angular conventions during authorized implementation. Avoid a generalized Safety framework or unrelated shell refactor.

## Explicit exclusions

- Any .NET/API permission redesign, new endpoint, server/schema/migration/persistence change, or server authorization shortcut.
- Incident screens, Concern-to-Incident conversion/escalation, dispatch, emergency contacts, notifications, timers-to-Incident, risk scoring, or danger status.
- Trip, Event, Plan, location, Check-In, Sharing Session, Safety Circle, Trusted Contact, or private profile detail joins, indicators, convenience links, maps, or location status.
- Audit UI, denied-attempt counters, admin/operator audit access, or audit data retrieval.
- Global shell redesign, mobile navigation redesign, final mobile destination count/grouping, or resolution of `OD-001`, `OD-002`, or `OD-004`.
- A new Safety design system, mockup-only functionality, screenshot golden baselines, or a formal participant study.
- Backup aging / restore-operation evidence. This remains a predeployment dependency from WI-008-001 and is not a Concern UI blocker.

## Navigation and route

- Route: `/concerns`, handled through the current `App` shell/routing pattern.
- Discovery: a small Concern coordination entry/card on Home routes to `/concerns`; the route remains directly addressable and has a meaningful page heading.
- Do not add a seventh global navigation destination or a mobile navigation treatment. This keeps OD-001 unresolved while making the workflow discoverable in the current desktop/routing architecture.
- The existing shell may require current Family selection before rendering feature content when multiple groups exist. That selection is not authorization; creation uses it only to select a Family Group and load its member directory. Once rendered, do not filter the Concern list by selected Family: `/api/concerns` returns authorized Concerns across the account's eligible groups.

## List/dashboard

- Render only results from `GET /api/concerns`; never synthesize a Concern from Family membership, Home summaries, local cache, or guessed IDs.
- Show subject display name, lifecycle state, `CreatedAt`, `LastAuthorizedMutationAt` labeled “Last activity”, and creator/participant relationship where useful. Preserve the API's newest-created-first ordering; do not relabel a recent entry time as general Concern activity.
- Make Open (coordination available), Frozen (read-only; coordination ended), and Resolved (read-only; record ended) visually and textually distinct. Do not use color alone.
- Sort by a simple, documented recent-activity ordering using returned timestamps; no unsupported filters, counts, urgency ranking, or hidden existence hints.
- Empty state: “No Concerns are currently available to you.” Explain that a Concern is shared only with people explicitly selected, without implying a subject can see it.
- A stale list item that later returns unavailable is removed after refresh and never used to infer whether it still exists.

## Creation and audience

- Select one active adult subject from `/api/family-groups/{familyGroupId}/members`; allow the current adult as an explicit self-Concern subject using `/api/profile` identity. Avoid duplicate self choices if the directory includes the current adult.
- Require a concise reason/context (`CreateConcernRequest.Reason`, maximum 1,000 characters). This is the creation-time context supported by the API.
- The creator has creator access while eligible; the API represents this separately from selected participant grants. Let the creator intentionally select zero or more additional eligible adults; do not preselect the subject. Selection of the subject as participant, if offered, must be an explicit checkbox action.
- Before submit, state plainly: “Only you and the people you select can access this Concern. The person it is about is not included unless you select them. Family membership or administrator status alone does not provide access.”
- Do not show membership IDs, access versions, authorization internals, or eligibility implementation details. Directory IDs are only submission keys.
- Initial sourced information is not atomic with creation. After successful creation, an optional initial Observation/Unknown can be submitted through the entry API. If that second request fails, retain the created Concern, state that the Concern was created but the entry was not added, and offer to continue to the Concern or retry only the entry. Do not claim creation failed or retry creation automatically. Disable create submission while its request is in flight; after an ambiguous network result, refresh/confirm the authorized list before offering a deliberate new create.
- If no eligible additional participant is returned, explain that the Concern can be created with the creator as its only selected participant. Do not silently broaden the audience.

## Detail, history, and entry presentation

Group the screen into: subject and creation context; state and lifecycle timing; selected participants; chronological entries/history; actions available in the current state; and resolution information when resolved. Keep state and audience understandable without an emergency-console layout.

Each entry presentation uses returned `ConcernEntryView` fields: contributor, source description or explicit “Source unknown” according to `SourceUnknown`, recorded time, optional event time, text, kind, category, freshness marker, explanation, and linked target. Keep recorded time distinct from event time. Preserve all authorized history in chronological order; for linked correction/challenge, provide a visible and screen-reader-understandable relationship to the original entry, with a meaningful link/action to navigate to it.

Categories are information/provenance labels, not danger scores: Confirmed, Reported, Assumption, Unknown, and Outdated. Use brief neutral helper copy that categories describe the information's basis/freshness and do not confirm danger or safety. Confirmed must not visually imply objective system verification; its recorded source remains visible.

Outdated is a freshness concept. The request uses `Category=Outdated` with `OriginalCategory`; the server persists the original value in `ConcernEntryView.Category` and returns freshness separately as `IsOutdated`. Present both, e.g. “Reported · Outdated” or “Confirmed · Outdated”. When creating an Outdated entry, request an original classification of Confirmed, Reported, or Assumption; the current API rejects Unknown or Outdated as its original classification. Do not replace the original classification with an Outdated-only badge or create a sixth category.

## Corrections and challenges

- Correction is offered only for the current user's own entries, using `ContributorProfileId` compared with the current `/api/profile` `ProfileId` as a presentation hint. It appends a `Correction` linked by `RelatedEntryId`; the original remains visible and unchanged. Server account-based authorship validation is authoritative.
- Challenge is a distinct action on another contributor's entry. Label it neutrally, e.g. “Add a different perspective”; explain that it creates a separate attributed entry and does not edit the original.
- Both flows use `AddConcernEntryRequest`: collect entry text (1–2,000 characters), category, source description or explicit unknown source, optional event time, target entry, and required explanation (1–1,000 characters). Confirmed requires a described verification source. Preserve author/time and linked history. No ordinary edit/delete UI is allowed.

## State and controls

- **Open:** authorized participants can view, contribute Observation/Unknown, correct their own entry, and challenge another. Creator may additionally replace the complete participant set and resolve. Derive display affordances from the returned creator/participant/state data plus current profile identity, but treat API results as authoritative. Handle server 404/unavailable on any action without revealing why.
- **Frozen:** show “Coordination in this Concern has ended. This record is read-only. Start a new Concern if coordination needs to continue.” No writes, participant changes, or resolution. Do not explain a private membership loss or call the record resolved/safe/danger-ended. If the API supplies a general approved freeze reason, present it only in neutral general terms; do not expose membership details.
- **Resolved:** show resolution time and attributed note. State once, clearly, that resolution means coordination on this record ended and does not establish that the subject was safe, in danger, or that information was verified. Read-only; no reopening.
- No UI behavior may presume permissions from Family Administrator status, creator/participant state stored only in browser memory, or a prior successful request.

## Unavailable/access loss and empty states

- `GET /api/concerns/{id}` and all protected writes map `ConcernUnavailableException` to 404, intentionally nondisclosing. Show a generic “This Concern is unavailable” state, with a link back to the current list; do not distinguish nonexistent, unauthorized, expired, membership loss, rejoin, or stale grant.
- After a nondisclosing unavailable response, clear cached detail and refresh authorized list; remove stale item if it no longer appears. Do not reveal former participants, current existence, or private access reasons.
- 401: use existing signed-out/session handling; do not present as a Concern-specific access denial.
- 400 validation: map field keys to field-level, UI-owned neutral messages and preserve safe draft text where practical; do not display raw backend exception text. Generic network/server errors provide retry without replaying a potentially successful create/write blindly.
- Include empty states for no accessible Concerns, no entries, no eligible additional participants, Frozen, Resolved, and unavailable/error. All copy remains neutral and concise.

## Responsive and accessibility requirements

- Follow existing shell and feature styles at normal desktop and narrow/mobile widths; stack cards, detail groups, forms, and actions in reading order with no horizontal overflow. Keep narrow history in a readable single column rather than dense multi-column layout.
- Use semantic page/section headings, labeled controls, fieldsets/legends for participant and category choices, associated validation descriptions, keyboard-operable selectors/actions, visible focus, meaningful button names, and status/error announcements (`role="status"` / `role="alert"` where appropriate).
- Use sufficient contrast and non-color state/category cues. Preserve logical focus after submit, validation, refresh, unavailable routing, and opening/closing forms.
- Do not prioritize screenshot similarity over accessible interaction.

## UI action → API mapping

| UI action | API action / endpoint | Required server authorization | Expected error / unavailable handling |
|---|---|---|---|
| Load authorized list | `GET /api/concerns` | Current authenticated account; server returns only currently visible, eligible records in newest-created-first order | `401` existing session flow; network error with retry; empty list is not an authorization diagnosis |
| Load detail/history | `GET /api/concerns/{concernId}` | Current explicit creator/participant grant, current eligibility, nonexpired record | `404` means generic unavailable; clear stale detail and refresh list; never distinguish reasons |
| Load subject/participant options | `GET /api/family-groups/{familyGroupId}/members`; `GET /api/profile` | Existing active Family directory/current-profile rules | `401` session handling; directory `404`/error shown as unavailable choices; server revalidates all submitted IDs |
| Create Concern | `POST /api/family-groups/{familyGroupId}/concerns` with `CreateConcernRequest` | Active eligible adult; same-group eligible subject; creator/audience eligibility rechecked by server | `400` field errors; `401`; nondisclosing `404`; network ambiguity must not cause blind duplicate retry |
| Add initial Observation or explicit Unknown | `POST /api/concerns/{id}/entries` with `AddConcernEntryRequest` | Current authorized participant and Open state; server validates type/category/source and current rights | `400` field errors; `404` generic unavailable/state no longer writable; keep created Concern and allow safe retry after refresh |
| Contribute Observation / Unknown | `POST /api/concerns/{id}/entries` | Current authorized participant; Open; valid provenance/category | `400` field errors; `404` generic unavailable/read-only; refresh detail |
| Correct own entry | Same entries endpoint with `Kind=Correction`, target, entry text/category/source, and required `Explanation` | Current participant; Open; target belongs to current account; append-only | `400` validation/ownership rule; `404` unavailable; retain original and draft, show neutral error |
| Challenge another entry | Same entries endpoint with `Kind=Challenge`, target, entry text/category/source, and required `Explanation` | Current participant; Open; target belongs to another account; additive, not overwrite | `400` validation; `404` unavailable; retain target unchanged in presentation |
| Replace participant audience | `PUT /api/concerns/{id}/participants` with complete `ParticipantProfileIds` set | Current creator, Open, every chosen person currently eligible; server revalidates at commit | `400` invalid/ineligible set; `404` generic unavailable/read-only/not creator; refresh audience and explain selection must be reviewed again |
| Resolve | `POST /api/concerns/{id}/resolve` with required `Note` | Current creator, Open | `400` required note; `404` generic unavailable/read-only/not creator; refresh before further action |

### API fit and gaps

- No blocker requires an API change for the MVP workflow. List/detail/create/audience replacement/entries/resolution and required response fields exist.
- Active eligible adult choices can use the existing active Family directory (`ProfileId`, `DisplayName`) plus own `/api/profile`; the directory endpoint is not a Concern grant and server-side Concern eligibility validation remains authoritative.
- Creation supports a required `Reason`, not an atomic initial provenance entry. An initial Observation/Unknown is a second entries request after creation, with explicit partial-success handling. This small UI composition is within the story; no WI-008-001 reopening is recommended.
- The API does not return explicit `canContribute`/`canManageParticipants` capability flags. Use only simple display hints: compare `CreatorProfileId` to current `ProfileId`; otherwise check whether current `ProfileId` appears in returned `Participants`; require `State=Open` for write controls; compare entry `ContributorProfileId` to current `ProfileId` to offer correction/challenge actions. Do not copy membership/version or commit-time authorization rules into Angular. If these fields prove insufficient, stop and propose a separate capability/API story rather than guessing or changing permissions here.
- `GET /api/concerns` returns full authorized Concern views and orders by `CreatedAt` descending. Preserve that ordering for MVP; show `CreatedAt` and `LastAuthorizedMutationAt` (“Last activity”). Do not calculate list recency from latest entry alone or require a summary endpoint.
- The detail response does not return `OriginalCategory`; this is not a gap because the repository projects the request's preserved original category into response `Category` and returns the freshness marker in `IsOutdated`.
- `AddConcernEntryRequest` requires valid `Kind`, `Category`, `Text`, and either `SourceDescription` or `SourceUnknown=true`; corrections/challenges also require `RelatedEntryId` and `Explanation`. Map these fields in each composer.

## Angular/component verification strategy

Add focused component/service coverage for:

- Authorized list rendering, API newest-created-first ordering, `CreatedAt` and `LastAuthorizedMutationAt` labels, creator/participant labels, and no-visible-Concern state.
- Subject/self selection, active adult participant selection, explicit nonselection of subject by default, audience explanation, no eligible additional participants, and create validation.
- Required reason; in-flight duplicate-submit protection; optional initial entry's two-request partial-success behavior and no duplicate create retry.
- Lifecycle rendering and action visibility; API 404 after stale controls; creator-only participant/resolution affordances; participant restrictions.
- Observation versus explicit Unknown; all five categories; source known/unknown, recorded/event time; both original category and Outdated freshness.
- Append-only correction relationship/author/time/explanation and challenge as a separate linked contribution that leaves target unchanged; both send valid text/category/source fields.
- Frozen and Resolved read-only behavior and resolution attribution/copy.
- Generic unavailable/access-loss handling, stale-list refresh/removal, 401, validation/network errors, empty entries, and focus/status announcements.

Tests prove UI integration/presentation. Do not duplicate WI-008-001's deep SQL authorization matrix; its server tests remain authoritative for access enforcement and commit-time checks.

## Browser / E2E strategy

Add one stable Playwright journey using the existing Development setup: run the isolated invitation scenario with `-IncludeTripAudienceAdult` so Adult C is an active member; have Adult A invite Adult B and Adult B accept through the existing invitation flow; then A creates a Concern about B, explicitly selects C, and the UI confirms B was not selected and explains the audience. B cannot retrieve the Concern and receives only generic unavailable behavior; C opens and contributes; A resolves with a note; C refreshes and sees retained read-only history and resolution time/note. Current invitation and Trip audience E2E scenarios establish these identity, invitation, and optional C-fixture mechanisms; this journey needs no new .NET fixture. Assertions must not infer denial cause from the UI.

Cover access-loss browser behavior only if stable fixtures/API can revoke an existing participant during the journey. It is optional; nondisclosure, stale-detail clearing, and list refresh remain component/API integration coverage if fixture churn would make E2E brittle. Keep deep server policy tests in the .NET suite.

## Rendered visual verification

For future authorized implementation, capture/review normal desktop and narrow/mobile widths for: empty list, populated list, create form/audience selection, Open detail, corrected/challenged history, Frozen, Resolved, unavailable/error. Check reading/focus order, wrapping, overflow, control sizing, state distinction without color, and the existing calm Family-centered visual language. Owner visual approval is the final visual acceptance checkpoint. Do not add screenshot golden baselines without separate approval.

## Acceptance criteria (26)

1. **Route and discovery:** `/concerns` is reachable from a concise Home entry point; global/mobile navigation and `OD-001` remain unchanged.
2. **Authorized list:** only `GET /api/concerns` results render in server newest-created-first order; subject/state, `CreatedAt`, `LastAuthorizedMutationAt` (“Last activity”), and optional creator/participant context are shown.
3. **Lifecycle summary:** Open, Frozen, and Resolved are textually and non-color-only distinguishable; no urgency score or danger/safety inference appears.
4. **Empty list:** no-visible-Concern copy is neutral and does not disclose whether any inaccessible record exists.
5. **Subject choices:** subject options come from active adult directory/current profile only; self-Concern is supported; no private profile fields are fetched.
6. **Explicit participant audience:** creator access is explained separately from selected participant grants; extra participants are intentionally selected; subject is never preselected or automatically granted access; zero extra participants is supported.
7. **Audience copy:** selected-only access, no automatic subject access, and no Family/admin implicit permission are stated plainly before create.
8. **Create validation:** required reason/context and supported field limits are validated; server validation is mapped accessibly.
9. **Initial information and duplicate protection:** create submission is disabled in flight; any optional initial entry is separate after create; partial success identifies the created Concern and failed entry, offers continuation or entry-only retry, and never automatically recreates the Concern.
10. **Detail structure:** subject/context, lifecycle, audience, chronological history, allowed controls, and resolution information are grouped clearly.
11. **Entry provenance:** contributor, source/unknown-source, recorded time, optional event time, kind/category, freshness, explanation, and linked relation are shown when returned.
12. **Category meaning:** Confirmed, Reported, Assumption, Unknown, and Outdated are presented as provenance/freshness labels, never danger scores or verified truth by mere entry.
13. **Outdated freshness:** original category and outdated state are both visible; the form collects an allowed original classification and maps it to the request; no sixth canonical category is introduced.
14. **Observation / Unknown contribution:** Open authorized participants can submit the supported entry types with required text/source/category validation.
15. **Correction:** own-entry correction is offered using stable profile identifiers as a UI hint and appends a linked contribution with required provenance, explanation, author, and time; original remains visible and unchanged; the link to the original is understandable to screen-reader users.
16. **Challenge:** challenge is distinct and neutral, appends an attributed linked contribution with required provenance, and does not appear to edit or accuse another person or imply the original is false; the target relationship is understandable to screen-reader users.
17. **Open participant controls:** participant contribution/correction/challenge controls match the supported workflow; creator-only controls are not shown as participant actions.
18. **Participant management:** creator Open control replaces the complete audience intentionally; server validation/unavailable outcomes are surfaced without claiming client role state grants rights.
19. **Resolution:** creator Open control requires a note; resolved record shows attributed note/time and the coordination-ended distinction once.
20. **Frozen:** history is read-only; copy says coordination ended and a new Concern is needed to continue; no membership-loss details are exposed.
21. **Resolved:** history is read-only and does not imply safety, danger, verification, or reopening.
22. **Unavailable/access loss:** 404 is generic; stale detail is cleared and authorized list refreshed; no reason distinction or existence hint is shown.
23. **Empty/error states:** no entries, no eligible extra participants, unavailable, loading, validation, and network states are useful, neutral, retry-safe, and do not expose raw backend error text.
24. **Protected-domain boundary:** no audit, Incident/escalation, location, Check-In, Sharing, Safety Circle, Trusted Contact, Trip, Event, Plan, or private-profile data appears.
25. **Accessibility and responsive behavior:** semantic headings, labels, fieldsets, keyboard, visible focus, announcements, non-color cues, logical focus, and usable desktop/narrow layouts pass focused review.
26. **Authorization boundary:** affordances use only simple response/profile comparisons; every request remains subject to server authorization; no client-side role logic grants access or distinguishes nondisclosing failure causes.

## Implementation Notes / Check-in — 2026-10-01

- Implemented the authorized Concern Angular workflow on the existing WI-008-001 API; no server/API permissions, schema, migrations, Incident work, or OD-001 changes.
- Angular suite: 66 tests passed. Isolated Adult A/B/C Playwright workflow: passed. Production build: passed with Angular bundle/style budget warnings recorded in `planning/verification/VER-WI-008-002.md`.
- Desktop and narrow rendered evidence is in `planning/verification/evidence/WI-008-002/`.
- Owner feedback follow-up: removed the duplicate, nonfunctional visible `Plans & Trips` heading and its outlined wrapper; Plans and Trips now use the same peer-button styling as all navigation destinations. Links are unchanged; Angular dev server rebuilt and reloaded successfully.
- Status is `VERIFYING / OWNER REVIEW`. Do not mark `DONE / PASS` until the owner explicitly approves the visual review.

## Implementation Notes / Mockup alignment — 2026-10-01

- Owner-directed visual alignment of the Concern UI toward `references/ux/safety/safety-coordination.png` (panel 1 composition, panel 4 timeline/outcome styling) and the Events create-form pattern, rendered in the existing evergreen tokens. Shell/navigation unchanged except the Concern page title text (`Concern coordination` → `Concerns`) to remove the duplicated in-page title.
- Layout: intro row with the create action; two-column card grid for the list; standalone create view with details left and access right; detail header card with subject initials, reason, Created/Last activity and state; history timeline beside People-with-access and Contribute cards (stacks on narrow); Resolved outcome card.
- Owner decisions applied: one timeline (no duplicated Observation/Unknown summary cards); resolution shown as "Resolved by {creator}" because only the creator can resolve (`workflows/safety.md`); resolution note is revealed by "Resolve this Concern…" instead of always visible; captures saved to a new evidence subfolder.
- Behavior, API calls, required copy, and authorization hints are unchanged. Small presentation-flow changes: focus moves to newly opened create/composer headings and the resolution note; after an ambiguous create finds no match, the list view is shown and the draft is kept.
- Validation: 66 Angular tests pass (no spec changes); Concern Playwright journey passes and now also submits a correction and a challenge, opens participant management and the resolve form, and captures unavailable and Home states; production build passes with `concerns.scss` at 6.92 kB (was 6.93 kB). Status remains `VERIFYING / OWNER REVIEW`.
- Refinement pass (2026-10-01): single-message unavailable state; non-wrapping compact timeline times; bounded shared-shell spacing and title-size reduction (header 261 → 216 px desktop, 312 → 295 px narrow, on all screens). Tests, Concern E2E, and build pass; details in `VER-WI-008-002.md`. Status at that time remained `VERIFYING / OWNER REVIEW`.

## Closure — 2026-10-01

- **Outcome:** `DONE / PASS`. All 26 acceptance criteria passed. The project owner supplied `OWNER VISUAL APPROVAL` for the Concern visual direction on 2026-10-01, completing AC-25.
- **Accepted intentional differences from the mockups:** calm evergreen colors; initials instead of photos or icons; no Known/Unknown summary cards; no Incident, Suggested Steps, contact log, map, or other mockup-only functionality.
- **Separate follow-up areas (not created as stories):**
  - Shared-shell mockup alignment: single top-bar layout versus the stacked header rows, and page titles repeated in shell and content. `OD-001` remains `OPEN`.
  - Playwright full-suite database isolation.
- Closure changed documentation only; no application code changed and no tests were rerun. Evidence and build warnings are recorded in `VER-WI-008-002.md`.

## Open UI decisions

- Final compact card grouping and whether “Last activity” needs a short explanation can be settled during implementation; use `LastAuthorizedMutationAt` and preserve API creation order.
- Whether the participant editor is an inline section or a focused dialog is an interaction choice; preserve complete-audience confirmation and accessibility.
- Exact selector composition for the original category when marking an entry Outdated can be settled visually; request mapping and allowed original categories are fixed by current API validation.

## Blockers and dependencies

- **Genuine blocker:** none for creating this planning story or implementing against the existing API.
- **Dependency:** WI-008-001 remains `DONE / PASS`; API integration must preserve its private routes and nondisclosing 404 behavior.
- **Not a UI blocker:** backup aging/restore-operation evidence remains required before deployment per `VER-WI-008-001`.
- `OD-001`, `OD-002`, `OD-004` remain `OPEN`; `OD-003` remains outside MVP.

## Execution authorization

- **Planning story recorded:** `YES`.
- **Execution authorized:** `YES` — explicit project-owner authorization dated 2026-10-01, limited to WI-008-002.
- **Application implementation authorized:** `YES` — Angular Concern UI only.
- **Not authorized:** .NET/API permission or capability changes, migrations/schema changes, Incident work, OD decisions, commit/push/PR, or subsequent Phase 8 work.

