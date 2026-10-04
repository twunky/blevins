---
Status: DONE
Authority: Project-owner approved and explicitly execution-authorized in the 2026-09-28 session; verification passed
Context-Baseline: CTX-1.23
Version: 1.0-draft
---

# WI-007-005 — Creator-Controlled Trip Audience Editing

## Metadata

- **Work Item ID:** `WI-007-005`
- **Status:** `DONE`
- **Phase:** `7 — Plans, Trips & Sharing`
- **Context baseline:** `CTX-1.23`; Trip audience rules from `CTX-1.22` / `DEC-061` remain in force
- **Execution authorization:** `YES`
- **Model tier:** `DEEP`

## Objective

Allow the Trip creator to deliberately replace the Trip's complete audience while preserving the approved Trip privacy, creator, audience-snapshot, and Notes rules. This is one whole-audience replacement operation, not an incremental access-control system.

## User and business value

Creators can correct or narrow who can see a Trip as plans change, and can deliberately grant access to eligible family members with a clear preview. Existing viewers lose access promptly when removed; audience changes do not silently expose Trip or Notes information or grant access to unrelated private data.

## Requirement traceability

- `REQ-PRIV-022` — authoritative Trip audience modes, replacement, eligibility, creator-after-departure limits, preview, revalidation, immediate access effects, disclosure, and exclusions.
- `REQ-TRIP-007` — creator-only audience editing and role boundaries.
- `REQ-TRIP-010` — current Notes permissions, retained note states, and Notes authorization.
- `REQ-SHARE-009` — Trip audience grants no Sharing Session or another person's Check-In access.
- `REQ-PRIV-020` — item audience rule; Trip-specific behavior remains governed by `REQ-PRIV-022`.
- `REQ-PLAN-005` — Plan audience behavior remains unchanged.
- `REQ-PRIV-021` — Event access behavior remains unchanged.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md` | Authority, execution, privacy, and verification rules | Entry |
| `ai-context/project-status.md` | Current phase, baseline, completed work, and authorization checkpoint | Entry |
| `ai-context/planning/roadmap.md` | Confirm Phase 7 remains current and scope is bounded | Before execution |
| `ai-context/core/privacy-safety-contract.md#REQ-PRIV-022` | Canonical Trip audience replacement, eligibility, preview, revocation, and creator exception | Before execution and verification |
| `ai-context/workflows/plans-trips.md#REQ-TRIP-007` and `#REQ-TRIP-010` | Trip creator/viewer permissions and Notes interaction | Before execution and verification |
| `ai-context/workflows/sharing-checkins.md#REQ-SHARE-009` | Preserve separation from Sharing Sessions and Check-Ins | Before execution and verification |
| `ai-context/core/privacy-safety-contract.md#REQ-PRIV-020`, `#REQ-PRIV-021`; `ai-context/workflows/plans-trips.md#REQ-PLAN-005` | Confirm Plan and Event access behavior is not changed | Before execution and verification |
| `ai-context/planning/stories/WI-007-003-trip-creation-audience-detail.md` and `ai-context/planning/verification/VER-WI-007-003.md` | Preserve completed Trip creation, initial access, and detail behavior | Before execution and verification |
| `ai-context/planning/stories/WI-007-004-trip-notes-collaboration.md` and `ai-context/planning/verification/VER-WI-007-004.md` | Preserve Notes states, provenance, and completed collaboration behavior | Before execution and verification |
| `ai-context/planning/verification/VER-CTX-1.22.md` | Confirm the active audience-editing reconciliation was verified; not a substitute for canonical rules | Before execution |
| `ai-context/templates/work-item.md` and `ai-context/templates/verification.md` | Work-item and verification structure | Before execution and verification |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Safety coordination, Concern, and Incident workflows | No safety workflow behavior is in scope; missed arrival/escalation is unchanged |
| Bootstrap and archived context | Superseded extraction/history; active canonical requirements are sufficient |
| Phase 5/6 implementation stories and unrelated verification | No changes to Family/Profile or Event behavior are authorized |
| Deployment, hosting, and external-provider context | No infrastructure or provider decision is needed for the bounded story |

## Functional scope

Implement one explicit, creator-authorized whole-audience replacement for an existing Trip:

- The resulting audience is exactly one mode: the Trip's existing applicable Family Group or Selected People from that same group.
- The creator can switch between modes. Choosing Family Group, including refreshing while already in that mode, creates a fresh snapshot of currently eligible active adult members and replaces the prior snapshot.
- Selected People replaces the complete recipient set with distinct, currently eligible active adult members of the Trip's applicable Family Group. At least one eligible non-creator recipient is required.
- The proposed audience is previewed, validated again by the server at commit, and applied atomically. Invalid or stale proposals leave the existing audience unchanged.
- A creator who has lost eligibility to access the applicable Family Group may only make permitted reductions based on currently eligible recipients already represented in the saved audience.
- Committed access changes take effect immediately for Trip and normal Notes access. Creator access remains separate and is never transferred or removed.

## Permission model

- Only the Trip creator may initiate and confirm an audience replacement.
- Creator authority remains independent of audience membership and Family Group eligibility, subject to the canonical reduction-only limits after membership/eligibility loss.
- Travelers, viewers, Family Administrators, and non-system contacts receive no audience-edit permission from their status.
- Audience editing does not change Trip core-field permissions, traveler status, Sharing Session permissions, Check-In permissions, or location-sharing consent.

## Audience replacement and eligibility model

### Family Group mode

- Exactly one applicable Family Group is used; audience editing cannot change the Trip's Family Group.
- Selecting or refreshing Family Group replaces the recipient snapshot with currently eligible active adult members at that time.
- No individual additions, exclusions, partial snapshots, or hidden overrides are supported.
- Later joiners do not gain access automatically. Membership loss revokes access; rejoining does not silently restore it.

### Selected People mode

- The proposal replaces the complete saved recipient set.
- Recipients must be distinct, currently eligible active adult members of the Trip's same applicable Family Group.
- At least one eligible non-creator recipient is required; creator access does not satisfy this minimum.
- All proposed recipients are validated together. If any recipient is invalid or stale, reject the complete change and preserve the existing audience.

### Creator after Family Group eligibility loss

The creator retains own-Trip creator access but may only reduce access using currently eligible people already represented in that Trip's saved audience. The creator may remove current recipients, switch Group to Selected People using a valid nonempty subset of those recipients, or shrink a Selected People set. They cannot discover current Group members, add newly eligible people, refresh the Group snapshot, switch Selected People to a newly generated Group audience, or regain Family Group directory or unrelated profile access. Ownership transfer is not included.

## Preview and confirmation

- Before confirmation, the creator is shown the resulting audience mode and which recipients will gain or lose access.
- For a creator who has left the eligible Group, preview may disclose only saved, currently eligible recipients needed for a permitted reduction; it must not reveal the current Group directory.
- The server revalidates creator authority and recipient eligibility at commit.
- If the commit-time access effects differ from the confirmed preview, the operation does not silently commit; a refreshed preview and confirmation are required.
- The precise API, persistence, and concurrency mechanisms are implementation design, provided observable behavior meets `REQ-PRIV-022`.

## Immediate access and Trip Notes interaction

- Audience replacement is one atomic access change.
- Removed recipients immediately lose Trip list/detail and normal Trip Notes read, add, edit, and delete access. Their existing Notes, content, provenance, and state remain unchanged and inaccessible to them.
- Newly authorized recipients gain normal Trip access and visible `ACTIVE` Notes only. They do not receive retained `AUTHOR_DELETED` or `CREATOR_HIDDEN` Notes.
- Every Notes mutation checks current Trip authorization when the write is accepted. A removed recipient's stale client state cannot authorize a later successful mutation after revocation commits.
- Creator and non-creator audience disclosure remains unchanged: the creator may see the exact audience and selected recipient identities; a non-creator does not receive the full Selected People identity list.

## Sharing, Check-In, and location separation

Audience editing does not start, stop, or change a Sharing Session; grant location or another person's Check-In access; or alter sharing consent. Trip visibility and traveler status remain separate from Sharing Session authorization under `REQ-SHARE-009`.

## Validation rules

- Resulting audience mode is exactly Family Group or Selected People.
- Family Group refers to the Trip's existing applicable group and uses a fresh eligible active-adult snapshot.
- Selected People is a complete, duplicate-free set of eligible active adult members from that group with at least one non-creator recipient.
- Creator-after-departure proposals use only currently eligible recipients in the saved audience and obey reduction-only limits.
- Authority, eligibility, and the confirmed access effect are revalidated at commit. Invalid/stale proposals fail without partial changes.

## API and persistence expectations

- Provide a bounded preview/confirmation behavior that enforces the canonical server-side rules; UI checks are not the authorization boundary.
- Audience replacement is persisted as one atomic whole-audience change. A failed validation or stale confirmation leaves the previous saved audience and access effects unchanged.
- Enforce current Trip authorization in Trip and Notes reads/mutations after commit, including requests based on stale client state.
- Do not prescribe a particular endpoint layout, concurrency token, transaction implementation, or schema/migration. Add a migration only if implementation design demonstrates it is required.
- Do not add dedicated audience-history UI, generalized ACL/audit frameworks, or special retained audience-change records solely for this capability. Existing ordinary modification metadata conventions still apply.

## UI expectations

- Only the creator is offered audience-edit controls.
- The creator can select exactly one approved audience mode and receives a clear gains/losses preview before confirming.
- Group mode exposes no individual include/exclude controls.
- Selected People mode requires at least one eligible non-creator recipient.
- A departed creator is not shown a current Group directory or controls that would expand access beyond the saved eligible audience.
- Stale preview or server-side revalidation failures require the creator to refresh and confirm the changed access effects.
- Non-creator Trip detail continues to show only the approved audience mode/context, not the full Selected People identity list.
- No audience-change notifications or unrelated capability controls are introduced.

## Acceptance checks

1. **`AC-01 — Creator-only replacement`**: Only the Trip creator can preview, confirm, or execute an audience change; traveler, viewer, administrator, contact, and outsider status grants no such authority.
2. **`AC-02 — Creator access persists`**: Audience replacement cannot remove creator access or transfer Trip ownership, including when the creator has lost eligible Family Group membership.
3. **`AC-03 — Exactly one mode`**: Every committed audience is exactly one applicable Family Group or Selected People mode; combined modes, multiple groups, and non-system viewers are rejected.
4. **`AC-04 — Group to Selected People`**: The creator can replace a Group audience with a complete valid Selected People set from that same applicable group.
5. **`AC-05 — Selected People to Group`**: An eligible creator can replace a Selected People audience with a fresh Group snapshot from that same applicable group.
6. **`AC-06 — Fresh Group snapshot`**: Selecting Family Group creates a snapshot of currently eligible active adult members; later joiners do not gain access automatically.
7. **`AC-07 — Group refresh replacement`**: Refreshing Group mode replaces the previous saved snapshot with a fresh current eligible snapshot; prior access is not retained as an override.
8. **`AC-08 — No Group overrides`**: Group mode offers and accepts no individual additions, exclusions, partial snapshots, or hidden overrides.
9. **`AC-09 — Complete Selected People set`**: Selected People replaces the entire previous set; it is not an additive include/remove ACL operation.
10. **`AC-10 — Minimum recipient`**: Selected People requires at least one distinct eligible non-creator recipient; creator access alone is insufficient.
11. **`AC-11 — Recipient eligibility`**: Every Selected People recipient is currently eligible, active, adult, and in the Trip's applicable Family Group at commit.
12. **`AC-12 — Atomic invalid rejection`**: Any invalid, duplicate, or stale recipient rejects the whole proposal and leaves the saved audience and access unchanged.
13. **`AC-13 — Gains/losses preview`**: Before confirmation, the creator sees the resulting mode and people who will gain or lose Trip access, within the departed-creator disclosure boundary.
14. **`AC-14 — Stale preview reconfirmation`**: If current authority, eligibility, or access effects change after preview, the stale proposal does not silently commit; refreshed effects and confirmation are required.
15. **`AC-15 — Departed creator reduction-only`**: A creator who lost eligible Group membership can remove recipients, switch Group to Selected People using a nonempty subset of currently eligible people already represented in the saved audience, or shrink a Selected People set to such a subset. No other audience expansion or replacement is permitted after departure.
16. **`AC-16 — No directory discovery after departure`**: A departed creator cannot discover new Group members, add newly eligible people, refresh a Group snapshot, switch Selected People to a new Group snapshot, or regain directory/profile access through audience editing.
17. **`AC-17 — Trip audience lifecycle`**: Once replacement commits, removed recipients immediately lose Trip list/detail access. An audience member who loses eligible Family Group membership also loses Trip access; rejoining does not automatically restore it.
18. **`AC-18 — Immediate Notes revocation`**: Once replacement commits, removed recipients immediately lose normal Notes read/add/edit/delete access. Membership-loss revocation also removes audience-based Notes access; creator access remains governed by its separate creator authority.
19. **`AC-19 — Existing Notes preserved`**: Audience replacement leaves existing Note content, author, timestamps, provenance, and state unchanged; removed Notes remain inaccessible to removed recipients.
20. **`AC-20 — New viewers see ACTIVE Notes only`**: Newly authorized recipients can read visible `ACTIVE` Notes but cannot retrieve `AUTHOR_DELETED` or `CREATOR_HIDDEN` Notes through normal APIs or UI.
21. **`AC-21 — Current authorization for Note mutations`**: Each Note mutation uses current Trip authorization when the write is accepted. A write based on stale client authorization fails if audience or membership revocation committed before acceptance.
22. **`AC-22 — Role-based audience disclosure`**: Creator can see the exact resulting audience and selected identities; non-creator viewers do not receive the full Selected People identity list.
23. **`AC-23 — Sharing/Check-In/location unchanged`**: Audience editing neither exposes another person's location or protected Check-In data nor grants, starts, stops, or changes Sharing Session access or consent.
24. **`AC-24 — No notifications`**: Committing an audience change does not send audience-change notifications.
25. **`AC-25 — WI-007-003 regression`**: Completed Trip creation, initial audience, detail viewing, creator core editing, and privacy behavior remain valid and unchanged outside this separately scoped audience operation. Ordinary core edits continue to preserve the saved audience.
26. **`AC-26 — WI-007-004 regression`**: Completed Trip Notes permissions, states, provenance, and normal display behavior remain valid, with only the current audience access effect specified in `REQ-PRIV-022` applied.
27. **`AC-27 — Plan/Event/Family/Sharing regression`**: Plan audience behavior, Event permissions, Family/Profile access, and Sharing/Check-In rules remain unchanged; no behavior is imported across these boundaries.
28. **`AC-28 — No adjacent capability`**: No audience-history UI, generalized ACL/audit framework, ownership transfer, organizer role, notification, location, Check-In, Sharing Session, Trip completion, or other excluded feature is added.
29. **`AC-29 — Evidence mapping`**: `VER-WI-007-005` maps every applicable requirement and `AC-01` through `AC-28` to concrete evidence and a result; persistence evidence cannot rely on skipped SQL-gated tests.

## Boundaries and explicit exclusions

- Trip completion or status workflow.
- Check-In execution, Sharing Session execution, location behavior, or sharing consent changes.
- Notifications caused by audience changes.
- Ownership transfer, multiple organizers, or organizer permissions.
- Non-system viewers, guest accounts, or non-system contact permissions.
- Multiple Family Groups, combined Group plus Selected People modes, or multiple concurrent audiences.
- Group-mode individual overrides or incremental ACL behavior.
- Plan or Event audience editing, or changes to Family/Profile membership behavior.
- Implementing Family Group membership removal/rejoin behavior; this item consumes existing eligibility/membership state only.
- Dedicated audience-history UI, generalized ACL framework, generalized audit framework, or new retained audience-change records solely for this feature.

## Dependencies

- Active `CTX-1.23`; Trip audience rules from `CTX-1.22` and `DEC-061` remain in force.
- Completed `WI-007-003` Trip creation/audience/detail foundation and `WI-007-004` Trip Notes behavior.
- Existing identity, Family Group membership, and eligibility sources used by Trip access.
- Explicit project-owner approval and separate execution authorization for this `DEEP` privacy/permission work item.

## Assumptions

- The Trip's applicable Family Group is fixed as defined by current Trip data and cannot be changed through this operation.
- Existing membership and eligibility data is authoritative at preview and commit; the implementation must not invent eligibility rules beyond `REQ-PRIV-022`.
- API shape, storage schema, migration need, transaction strategy, and concurrency mechanism remain implementation decisions bounded by observable canonical behavior.
- No other product behavior is inferred beyond `CTX-1.22` and the cited active requirements.

## Likely implementation areas

Names are illustrative; execution must follow established project patterns and verify actual paths before editing.

| Area | Likely impact |
|---|---|
| Domain/application | Trip audience replacement and preview/confirmation contracts, complete-set validation, creator and departed-creator policy |
| Persistence/repository | Atomic whole-audience replacement, fresh Group snapshots, saved-recipient reduction limits, current access checks; migration only if required |
| API | Creator-authorized preview/confirm operation, commit-time revalidation, stale-preview response, privacy-safe projections |
| Web | Trip Detail creator audience controls, mode selection, gain/loss preview, reconfirmation on stale state, departed-creator restrictions |
| Tests | Focused domain/service, SQL persistence, API authorization, audience access, stale-state, Notes interaction, Angular UI, and regression tests |
| Verification | `ai-context/planning/verification/VER-WI-007-005.md` mapping requirements and acceptance criteria to evidence |

## Test plan

- **Domain/application:** Whole-set replacement; one-mode constraint; group snapshot/refresh; no Group overrides; Selected People distinct/minimum/eligibility validation; creator versus non-creator authority; departed-creator reduction-only rules; invalid/stale proposal atomic rejection; access-diff calculation.
- **Persistence / SQL:** Atomic audience replacement; old recipient removal and new recipient access; fresh Group snapshot behavior; no later-joiner access; membership-loss and rejoin effects; departed creator reduction boundary; creator access retention; current Trip/Notes authorization after commit; note state/content/provenance retained; newly authorized recipients see only `ACTIVE`; migration only if implementation introduces one. Run SQL-gated persistence tests; a skipped gate is not passing evidence.
- **API:** Unauthorized preview/confirm rejected; creator success; server-side revalidation; invalid/stale proposal leaves old audience unchanged; access-change preview does not disclose Group directory to departed creator; non-creator disclosure omits Selected People identities; inaccessible Trip responses follow current privacy-safe patterns.
- **UI:** Only creator controls; exactly one mode; Group mode has no individual overrides; Selected People minimum and eligible selection; gain/loss preview; departed creator cannot discover/add recipients; stale preview requires refresh/reconfirmation; role-appropriate detail disclosure; no notification, Sharing, Check-In, location, completion, or history UI.
- **Regression:** Focused WI-007-003 and WI-007-004 Trip behavior; Plan audience/Notes behavior; Event, Family/Profile, and Sharing/Check-In tests. Existing project test/build patterns only; do not introduce a test framework.

## Verification plan

1. Before execution, reconfirm the `CTX-1.22` / `DEC-061` Trip audience rules remain in force under active `CTX-1.23`, and this work item has explicit owner approval and execution authorization.
2. Create `VER-WI-007-005` using `ai-context/templates/verification.md`; map `REQ-PRIV-022`, `REQ-TRIP-007`, `REQ-TRIP-010`, `REQ-SHARE-009`, `REQ-PRIV-020`, `REQ-PRIV-021`, and `REQ-PLAN-005` plus each `AC-01`–`AC-29` to evidence and a result.
3. Run focused domain/application, API, UI, and SQL-gated persistence checks using existing repository patterns. A skipped SQL persistence gate is not a pass.
4. Run the applicable broader .NET and Angular suites/build needed to demonstrate Plan, Trip, Event, Family/Profile, and Sharing/Check-In regression.
5. Record explicit negative-access and departed-creator evidence, including post-commit Notes mutation rejection and retained-note state behavior.
6. Disclose self-verification, unavailable environments, or other evidence limits. Mark this item `DONE` only after all required evidence passes and no scope or context conflict remains.

## Definition of Done

- The project owner explicitly approves and authorizes execution of this `DEEP` work item under active `CTX-1.23`, with the `CTX-1.22` / `DEC-061` Trip audience rules unchanged.
- The complete audience replacement, preview/reconfirmation, eligibility, departed-creator, immediate revocation, Notes, and disclosure behavior matches the acceptance checks and cited requirements.
- Any required persistence change is atomic and verified; no migration is presumed.
- Every cited requirement and acceptance criterion has concrete verification evidence.
- SQL-gated persistence evidence passes; applicable .NET and Angular regression suites/build pass.
- No excluded capability or unrelated Plan, Event, Family/Profile, Sharing, or Check-In behavior is introduced.
- A separate `VER-WI-007-005` records the evidence and outcome. The work item is marked `DONE` only after verification passes.

## Implementation risks

- Access may be checked from stale audience or membership data; commit-time authority, eligibility, and preview-effect validation must be authoritative.
- A departed creator's permitted reductions must not create a Group directory or profile-data access path.
- Partial persistence could leave both old and new recipients with inconsistent access; replacement must be atomic.
- Trip access changes must apply to Notes reads and writes, including concurrent/stale-client mutation attempts, without changing retained Note content or states.
- Reusing Plan's dynamic whole-Group access model would violate Trip snapshot rules.
- UI-only authorization could leak Trip or selected-recipient identities; enforcement must be server-side.

## Remaining ambiguity

No product ambiguity remains within this bounded story under `CTX-1.22`. Endpoint shape, data representation, transaction and concurrency mechanics, and whether an additive migration is needed are implementation design choices, not product decisions; they must preserve the canonical observable behavior and existing project conventions.

## Tasks

- [x] `TASK-001` — Confirm active baseline, explicit owner approval, and execution authorization before implementation.
- [x] `TASK-002` — Implement creator-authorized audience preview and whole-audience replacement.
- [x] `TASK-003` — Enforce eligibility, departed-creator reduction-only behavior, commit revalidation, and atomic access effects.
- [x] `TASK-004` — Add creator-only UI and privacy-safe gain/loss preview with stale confirmation handling.
- [x] `TASK-005` — Add focused tests and required Trip/Plan/Event/Family/Sharing regressions using existing patterns.
- [x] `TASK-006` — Complete self-verification and produce `VER-WI-007-005`.

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner
- **Authorized at:** 2026-09-28
- **Authorized scope:** WI-007-005 only; no subsequent Phase 7 work item or Phase 7.5 work.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| Final story review against active audience requirements | Reviewed against `CTX-1.22`, `DEC-061`, `REQ-PRIV-022`, `REQ-TRIP-007/010`, `REQ-SHARE-009`, `REQ-PRIV-020/021`, and `REQ-PLAN-005`; AC wording clarified without changing scope | `PASS` |
| Application behavior | Preview/confirm persistence, API, creator UI, stale handling, departed-creator boundary, immediate Trip/Notes access effects implemented | `PASS` |
| SQL-gated persistence | Full suite with `RUN_FAMILYSAFETY_SQL_TESTS=1`: 111 passed, 0 failed, 0 skipped | `PASS` |
| .NET suite | Full `FamilySafety.Foundation.Tests`: 111 passed, 0 failed | `PASS` |
| Angular | Unit suite: 22 passed; production build generated | `PASS` |
| Work-item verification | `VER-WI-007-005`, all requirements and AC-01–AC-29 mapped | `PASS` |

- **Verification report:** `ai-context/planning/verification/VER-WI-007-005.md`
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `PASS`

