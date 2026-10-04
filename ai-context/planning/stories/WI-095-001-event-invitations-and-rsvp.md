# WI-095-001 — Event Invitations and RSVP

## Metadata

- **Work Item ID:** `WI-095-001`
- **Status:** `VERIFYING / OWNER REVIEW` — implementation execution explicitly authorized by owner on 2026-10-04; owner visual approval and explicit closure pending
- **Phase:** Phase 9.5 — MVP Feature Completion; first approved item only
- **Context baseline:** `CTX-1.30` (execution depends on verified activation)
- **Requirement references:** `REQ-EVT-001`, `REQ-EVT-005`, `REQ-EVT-008`, `REQ-EVT-009`, `REQ-EVT-010`, `REQ-EVT-014`; `REQ-PRIV-011`, `REQ-PRIV-020`, `REQ-PRIV-021`, `REQ-PRIV-023`, `REQ-PRIV-027`, `REQ-PRIV-028`; `REQ-UX-011`, `REQ-UX-014`; `REQ-VERIFY-001`, `REQ-VERIFY-002`, `REQ-VERIFY-003`.
- **Boundary/dependency references:** `REQ-EVT-006`, `REQ-EVT-007`, `REQ-EVT-011`, `REQ-EVT-012`, `REQ-EVT-013` constrain scope; citing them does not authorize their implementation.
- **Decision references:** `DEC-083` (this scope and architecture), `DEC-058` (settled Event rules), `DEC-074`/`DEC-075` (authorize-first item routing), `DEC-078` (existing Events dashboard), `DEC-081` (closed Phase 9), `DEC-073` (phase numbering and later capabilities only).
- **Model tier:** `DEEP` — privacy, persistence, concurrency, and end-to-end ownership require explicit owner execution authorization.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| [README](../../README.md), [project status](../../project-status.md) | Operating rules, active checkpoint and authorization | Entry |
| [Events workflow](../../workflows/events.md) | Audience, organizer, invite/RSVP lifecycle and sibling exclusions | Before proposal / execution / verification |
| [Privacy contract](../../core/privacy-safety-contract.md#req-priv-028), also REQ-PRIV-011/020/021/023/027 | Current eligibility, named visibility, nondisclosure | Before proposal / execution / verification |
| [UX contract](../../core/ux-contract.md), REQ-UX-011/014 | Family presentation and authorize-first item routes | Before execution / verification |
| [Requirements index](../../core/requirements-index.md) | Requirement trace and canonical locations | Before execution / verification |
| [Decision log](../../decision-log.md), DEC-083/058/074/075/078/081 | Approved scope, retained rules and history | Before execution / verification |
| [Roadmap](../roadmap.md), [CTX candidate](../candidates/CTX-1.30-event-invitations-and-rsvp.md), [context verification](../verification/VER-CTX-1.30.md) | Activation and first-item-only gate | Before execution |
| [Web README](../../../src/FamilySafety.Web/README.md) | Current frontend/API/test conventions | Before execution |
| [Working rules](../../../claude-handoff/WORKING-RULES.md), [mockup alignment](../../../claude-handoff/MOCKUP-ALIGNMENT.md) and its named primary images | Implementation permissions and existing visual system | Before execution / UI verification |
| Current Event code across Domain/Application/Infrastructure/API/Angular/tests/E2E | Confirm current conventions and bounded design before editing; inspect only relevant paths | Before execution |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Bootstrap/archive and handoff HISTORY | Historical material is not current execution authority |
| Needs/Updates implementation designs; Sharing, Safety, Plan/Trip implementation | No sibling implementation or unrelated domain work |
| Full Phase 9 evidence bundle | Closed; load only specific item-route/dashboard evidence if needed to preserve behavior |

## Goal

After separate execution authorization, deliver the smallest useful end-to-end in-app Event invitation and self-RSVP experience: eligible organizers invite/revoke, invited adults discover their Pending invitation on Events and respond on Event detail, with server-enforced current access and retained minimal provenance.

## Boundaries / non-goals

- No Event Needs or Updates; no non-system invitees, minors/guardians, organizer transfer/multiple organizers, organizer self-invite or self-RSVP, Event completion/expiry, or post-creation audience/editor changes.
- No Home attention, email, push, SMS, notification infrastructure/center/preferences, generic feed, comments/chat, location, Check-Ins, Sharing Sessions, Trusted Contacts, Shared Family Dates, Safety Circles, Household grouping, Plan/Trip changes, or unrelated Phase 9.5 feature. OD-002 remains OPEN.
- No participant abstraction, RSVP revision-history table, independent retention timer, generic workflow engine, notification or Need data.
- No standalone Event refactor or visual redesign. Existing App detail/edit/create/list/card/routing/shell ownership is preserved under DEC-083.
- This task records intent only. None of the following implementation tasks may start without separate explicit owner execution authorization. No Git staging/commit/push or PR.

## Approved implementation intent

Use one minimum invitation record: stable invitation ID, Event ID, invitee profile identity, created time, revoked time (absent while unrevoked), current RSVP, deliberate RSVP-updated time (absent at creation), and a narrow version/concurrency token. Organizer identity derives from the Event creator rule. Do not store eligibility as durable invitation authority: current membership and canonical Event access remain authoritative. Keep revoked rows; later re-invitation gets a new ID. Preserve a single unrevoked record even during temporary eligibility loss.

Future backend work follows existing Domain/Application/Infrastructure/API boundaries with narrow invitation reads/issue/revoke and self-RSVP contracts, persistence mapping and an additive migration only after authorization. Reads project only actor-permitted fields; writes recheck current Event, organizer/self identity, membership, invitation and version. Protect duplicate issue and revoke-versus-RSVP races transactionally; stale writes cannot overwrite a later response or reactivate a revoked invitation. Exact DTO/route/file names follow inspected repository conventions; a material design change stops execution for owner review.

A small NEW `src/app/events/` participation boundary may own invitation/RSVP presentation, state, conventional domain API calls and focused specs/styles. Input is an already authorized Event from App/item-route. App keeps the Events dashboard except minimum discovery integration, creation/list/all/card/edit, `openRoutedItem`, authorize-first Family adoption, and selected-Family shell. Do not move existing detail/edit code or tests merely to create this boundary. Clear/reset participation state on item, identity or Family transitions; stale responses must not populate the new context.

Later Needs may consume invitation ID/profile, unrevoked status/current RSVP, creation/revocation/response times, and live membership/Event authorization. REQ-EVT-011 separately owns Need eligibility and responsibility/release provenance. This slice creates no Need state; same-profile restoration or re-invitation must never imply automatic restoration of future Need responsibility.

## Acceptance checks

The checks below govern implementation. [VER-WI-095-001](../verification/VER-WI-095-001.md) maps actual evidence; context verification alone does not satisfy them. Owner visual approval and explicit closure remain pending.

1. `AC-01` — Creating an Event and selecting its audience create zero invitations and no organizer RSVP; existing five-field detail editing and fixed audience/editor permissions remain. (`REQ-EVT-005/008/014`, `REQ-PRIV-020/021`)
2. `AC-02` — Only the currently authorized sole organizer with active Family membership can issue invitations, from Event detail, to existing active adult profiles with current canonical Event visibility; self-invite and ineligible/non-system targets are rejected server-side without expanding audience. (`REQ-EVT-009/010/014`, `REQ-PRIV-011/028`)
3. `AC-03` — At most one unrevoked invitation exists per Event/profile, including suspended eligibility. Repeated and concurrent issue attempts cannot create duplicates; re-invitation after revocation creates a new ID. (`REQ-EVT-014`)
4. `AC-04` — Non-organizer viewers, detail editors and Family Administrators cannot issue/revoke; client-selected Family and forged creator/actor fields grant no authority. (`REQ-EVT-009`, `REQ-PRIV-021/023/027/028`)
5. `AC-05` — Creation stores Pending and no deliberate RSVP-updated time; deliberate invitee submission sets that timestamp, including submission of Pending. (`REQ-EVT-014`)
6. `AC-06` — An independently eligible invited adult can RSVP only for their own unrevoked invitation. No organizer proxy RSVP, editor override, guessed ID, different/new profile or Account similarity bypasses this rule. (`REQ-EVT-010/014`, `REQ-PRIV-028`)
7. `AC-07` — While eligible and unrevoked, each of Pending, Going, Maybe and Not Going can transition freely to each approved state; invalid values are rejected. Response changes grant no audience/edit/Need/Update/private-data access. (`REQ-EVT-010/014`, `REQ-PRIV-023`)
8. `AC-08` — The authorized organizer can revoke without removing Event visibility. The row/final RSVP remain historical, no further RSVP write succeeds, and repeated revoke cannot alter final provenance or reactivate it. (`REQ-EVT-010/014`, `REQ-PRIV-028`)
9. `AC-09` — Revoked invitation identity/final RSVP follow the parent Event lifecycle. No RSVP revision table, public history UI or separate timer is introduced; no Event completion or disposal rule is invented. (`REQ-EVT-014`)
10. `AC-10` — Loss of active membership denies the invitee invitation/RSVP reads and writes while retaining the unrevoked record and response unchanged; it does not auto-revoke. (`REQ-PRIV-011/028`)
11. `AC-11` — Event visibility loss likewise denies invitee reads/writes without deleting or auto-revoking. Invitations and known IDs never grant Event access. (`REQ-EVT-010`, `REQ-PRIV-028`)
12. `AC-12` — The same profile regaining both active membership and Event visibility can again use the same unrevoked record. Regaining only one condition, selecting a Family, using a similar Account or a new profile fails; a revoked record remains inactive. (`REQ-PRIV-028`)
13. `AC-13` — Organizer access/membership loss denies organizer reads/issue/revoke, while independently eligible invitees can continue their own RSVP. Client creator state cannot restore authority; no blanket freeze. (`REQ-PRIV-028`)
14. `AC-14` — Eligible organizer projections contain identity, invited person, active/revoked state and current RSVP. Eligible invitee projections contain only their own records/RSVP. Other viewers/editors receive no named invitation/RSVP data, including through counts, list/detail/error payloads; an independently invited editor has only self visibility. (`REQ-PRIV-028`)
15. `AC-15` — Existing Events dashboard RSVP/attention shows only the caller's own currently eligible active Pending invitations for the selected Family. Past-dated Pending entries remain. Going/Maybe/Not Going, revoked/ineligible and other-Family invitations are excluded; no Home or outbound discovery surface is added. (`REQ-EVT-010/014`, `REQ-UX-011`)
16. `AC-16` — Each discovery entry opens the existing Event item URL; authorized Event detail hosts clear self-RSVP controls/status and organizer-only invite/list/revoke controls. Changing RSVP refreshes relevant detail/discovery state without changing audience/edit permissions. (`REQ-EVT-014`, `REQ-PRIV-028`, `REQ-UX-014`)
17. `AC-17` — Requested Event authorization occurs before Family presentation adoption; missing and unauthorized Event/invitation access remains nondisclosing using current uniform unavailable conventions. Refresh/deep links and direct API calls cannot bypass server access; a selected Family or unselectable Family creates no authority. (`REQ-PRIV-027/028`, `REQ-UX-014`)
18. `AC-18` — Stale/concurrent RSVP and revoke writes are protected by a narrow version token and current authorization at write time. Tests prove no lost response update, stale reactivation, duplicate active invite, or successful write after a competing revoke/eligibility loss; conflict recovery reloads authorized state without leaking data. (`REQ-EVT-014`, `REQ-PRIV-028`; DEC-083 implementation intent)
19. `AC-19` — Reads and actions have distinct loading, empty, failure and Retry states using current patterns; failed loads never look like empty success. Pending writes prevent duplicate submission; Retry reloads authorized current state and does not silently replay a mutation. (`REQ-EVT-014`, `REQ-PRIV-028`; existing UX contract)
20. `AC-20` — Switching Event, Family or identity clears/reloads participation state; delayed responses cannot display the prior person's/Family's invitation data. Existing routed Event editing, shell and authorize-first adoption remain. (`REQ-UX-011/014`, `REQ-PRIV-027/028`; DEC-083)
21. `AC-21` — Existing visual system is retained; keyboard-operable labeled controls, visible focus, announced loading/errors/status, accessible RSVP grouping and non-color-only states work at desktop and 390 px without horizontal overflow. Focused rendered/accessibility review covers organizer, invitee, viewer/editor, unavailable and read/action states. (UX contract; `REQ-EVT-014`, `REQ-VERIFY-001`)
22. `AC-22` — Implementation stays within DEC-083's new participation boundary, with authorized Event input. No existing detail/edit extraction or broad App rewrite; dashboard integration is minimal. No sibling data/behavior is implemented. (DEC-083; `REQ-EVT-006/007/011/012/013` exclusions)
23. `AC-23` — Focused .NET authorization/lifecycle/concurrency tests and SQL-backed persistence tests verify constraints, timestamps, retained revoked rows, new re-invite ID and transaction races; focused Angular specs and Playwright journeys cover discovery, all self responses, organizer controls, denial, recovery and Family/item transitions. (`REQ-VERIFY-001/002`, all implementation requirements above)
24. `AC-24` — After focused verification and owner visual review, run one final full .NET regression with SQL gate, one full Angular suite, production build and one final full Playwright run under current isolation/artifact rules. Record actual results, budgets, database/port/process health and any owner-accepted limitations; do not repeat broad runs without a concrete failure or gate. (`REQ-VERIFY-002/003`)
25. `AC-25` — A future VER-WI-095-001 maps every AC and implementation requirement to evidence, declares independence, records owner review and excludes unexecuted claims. DONE requires passing gates and explicit closure; context PASS alone is insufficient. (`REQ-VERIFY-001/002/003`)

## Tasks

- [x] `TASK-001` — Obtain explicit owner execution authorization after verified CTX activation; re-read checkpoint and inspect current relevant code/conventions.
- [x] `TASK-002` — Implement the bounded invitation domain, actor-specific contracts, server access, persistence/additive migration and concurrency constraints; focused .NET/SQL verification.
- [x] `TASK-003` — Add new participation presentation/state and minimal Events dashboard discovery integration, preserving App ownership; focused Angular verification.
- [ ] `TASK-004` — Verify focused end-to-end privacy/lifecycle/recovery, desktop/narrow render and accessibility; obtain owner visual review.
- [x] `TASK-005` — Run required final regressions, verify artifact health, prepare AC/requirement evidence and propose closure for owner review. Leave Git changes uncommitted.

## Stop conditions / dependencies

- STOP before application work until explicit owner execution authorization and activated CTX-1.30 are recorded. DEEP work cannot use standing routine auto-approval.
- STOP and report a material conflict with canonical access/retention/lifecycle rules or a repository reason to revise the approved single-record identity/history/concurrency direction. Do not invent a membership rejoin feature or alternate identity mapping to exercise same-profile return; use controlled test fixtures if current product has no restoration flow.
- STOP if eligibility, actor visibility, nondisclosure or atomic stale-write protection cannot be enforced; do not weaken authorization or silently freeze all invitees.
- STOP if implementation requires broad App/detail/edit extraction, audience editing, notifications, Needs/Updates or another excluded capability. These need separate owner/work-item approval.
- Re-evaluate changed baseline compatibility before execution. Existing Event authorization and item-route/dashboard foundations are dependencies; Phase 9 closure stays intact. Parent Event ultimate disposal remains a future decision, not a blocker to retaining rows while the parent exists.

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner's explicit APPROVED IMPLEMENTATION continuation instruction, supplied as Pasted text.txt in the new chat.
- **Authorized at:** 2026-10-04
- **Authorized scope:** Implement exactly WI-095-001 under active CTX-1.30: invitation model, additive migration, service/repository/API, small new participation component, selected-Family own-Pending discovery, focused and final verification, and current-context reconciliation. Needs, Updates, notifications, Home attention and every sibling capability remain excluded. No staging, commits, push or PR; owner performs all commits. Prior governance-only authorization is superseded for execution by this explicit instruction; canonical product decisions remain unchanged.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| Context readiness | [VER-CTX-1.30](../verification/VER-CTX-1.30.md) | Separate context verification; not implementation evidence |
| AC-01 through AC-23 | [VER-WI-095-001](../verification/VER-WI-095-001.md): bounded implementation, focused backend/API/SQL/Angular/E2E, rendered/axe review | See individual evidence/results |
| AC-24 / AC-25 | Same record: actual final gates, independence and review-order disclosure | Owner visual review/explicit closure PENDING |

- **Verification report:** [VER-WI-095-001](../verification/VER-WI-095-001.md).
- **Independence:** `SELF-VERIFIED` — the implementing agent also tested/reviewed this implementation. No independent or owner visual approval is inferred.
- **Outcome:** Implemented; `VERIFYING / OWNER REVIEW`. TASK-004 retains its owner visual-review requirement; final automated gates, evidence and closure proposal are recorded under TASK-005. Actual results and the latest-instruction validation order are disclosed in VER-WI-095-001; no owner approval or closure is inferred. No next item is authorized.

## Implementation record (2026-10-04)

- One `FamilyEventInvitation` entity/table; Guid identity/Event/profile/version, UTC creation/revocation/response times, current four-state RSVP. Organizer derives from Event creator. Pending has no initial response timestamp; new invitations after revocation get new IDs.
- Additive migration `20261004220426_EventInvitationsAndRsvp`: filtered active uniqueness, parent-history and own-Pending query indexes, restricted FKs and four-state SQL check. Existing Event/audience schema untouched.
- Actor-specific application/repository/API operations use canonical Event visibility, exact active membership/profile identity, serializable transactions and the existing Family-lock convention. GUID concurrency token changes on every response/revocation; stale writes return nondisclosing conflict after current access checks.
- New `src/app/events/event-participation` owns protected participation reads/actions/state/UI. App retains existing create/detail/edit/list/routing/Family-adoption ownership and adds only dashboard own-Pending discovery/reset.
- Focused evidence: 7 domain, 12 application, 12 API, 10 SQL invitation tests; 30 participation/dashboard Angular tests and 35 existing item-route tests. Two new integrated invitation journeys plus the two existing dashboard journeys verified. Exact final results and initial failures are in VER-WI-095-001.
- Only regression-test adaptation outside Event code: the Incident additive-migration test now explicitly targets its named Incident migration instead of assuming it is forever the latest migration. No Incident behavior or migration changed.
