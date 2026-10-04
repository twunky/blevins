# WI-007-004 — Trip Notes Collaboration

## Metadata

- **Work Item ID:** `WI-007-004`
- **Status:** `DONE`
- **Phase:** Phase 7 — Plans, Trips & Sharing
- **Context baseline:** `CTX-1.21` — approved and active; `REQ-TRIP-010` clarification verified by `VER-CTX-1.21`
- **Requirement references:** `REQ-TRIP-007`, `REQ-TRIP-009`, `REQ-TRIP-010`, `REQ-PRIV-004`–`REQ-PRIV-006`, `REQ-PRIV-011`, `REQ-PRIV-022`, `REQ-SHARE-009`; `REQ-PLAN-005/006` for regression only
- **Model tier:** `DEEP`
- **Execution gate:** Project owner approved and authorized execution on 2026-09-27 under active `CTX-1.21`.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md` | Authority, work-mode, and verification rules | Entry |
| `ai-context/project-status.md` | Active baseline and Phase 7 checkpoint | Entry |
| `ai-context/planning/roadmap.md` | Phase 7 status | Before execution |
| `ai-context/workflows/plans-trips.md#req-trip-007`, `#req-trip-009`, `#req-trip-010` | Creator, audience, contact, and Notes permissions | Before execution and verification |
| `ai-context/core/privacy-safety-contract.md#req-priv-004` through `#req-priv-006`, `#req-priv-011`, `#req-priv-022` | Trip access, membership lifecycle, and privacy boundaries | Before execution and verification |
| `ai-context/workflows/sharing-checkins.md#req-share-009` | No Check-In or Sharing Session access from Trip Notes | Before execution and verification |
| `ai-context/planning/stories/WI-007-003-trip-creation-audience-detail.md` and its verification report | Existing Trip access implementation and known limitations | During execution |
| `ai-context/templates/verification.md` and `ai-context/skills/verify-story/SKILL.md` | Evidence report structure and completion rules | Before verification |
| `ai-context/skills/execute-story/SKILL.md` | Approval and execution boundaries | Before execution |

## Requirement traceability

| Requirement | Application in this work item |
|---|---|
| `REQ-TRIP-010` | Notes read/add, author edit and soft deletion, creator moderation, three conceptual states, retained provenance, safe content, and exclusions. |
| `REQ-TRIP-007` | Separate creator, traveler, audience, and core Trip editing roles; traveler status alone grants no Notes access. |
| `REQ-TRIP-009` | Informational non-system contacts have no Notes access. |
| `REQ-PRIV-004`–`REQ-PRIV-006` | Trip participation, relationship, and administration do not imply location or private-data access. |
| `REQ-PRIV-011`, `REQ-PRIV-022` | Notes follow current Trip authorization, snapshot recipients, membership-loss/rejoin behavior, and the narrow creator exception. |
| `REQ-SHARE-009` | Notes access stays separate from another person's Check-In, location, and Sharing Session data. |
| `REQ-PLAN-005/006` | Regression only; Plan audience and Plan Notes rules remain unchanged. |

## Context intentionally not loaded

| Area | Reason |
|---|---|
| Safety coordination and incident workflows | Notes do not trigger a Concern, Incident, or escalation. |
| Sharing Session and Check-In execution details | Only the access boundary is relevant. |
| Historical Trip prototypes and superseded proposals | Active `CTX-1.21` controls this draft; earlier Trip reconciliation remains historical context. |
| Plan implementation decisions | Plan behavior is a regression boundary, not a source of Trip Notes permissions. |

## Objective and value

Add a lightweight collaborative Notes section to existing Trips. Authorized people can record useful travel coordination information with an identifiable author and timestamps. Authors manage their own active notes; the Trip creator can hide another author's note without changing its content. Notes remain part of the Trip and do not become chat or an activity feed.

This gives families a shared place for practical travel details while preserving each Trip's audience and contributor accountability.

## Owner decisions incorporated into this draft

1. The Trip creator may read and add Notes through creator authority independent of audience membership, including after eligible Family Group membership ends. This exception grants no other group-data access.
2. Author deletion is a soft delete: the note disappears from normal display but its original record, author, creation timestamp, and updated timestamp when present remain retained. Record deletion state/time as appropriate to the implementation.
3. After creator moderation hides another person's note, normal display excludes it, and the original retained note is immutable through normal Trip Notes actions. Its author cannot then edit or delete it. The creator cannot rewrite it.
4. MVP Notes have exactly three states: `ACTIVE`, `AUTHOR_DELETED`, and `CREATOR_HIDDEN`. No restore/recovery UI or additional moderation state is introduced by this item.

These decisions have been explicitly supplied by the project owner and are represented in active `REQ-TRIP-010` under `CTX-1.21`. The bounded implementation is complete.

### Canonical clarification activated before execution

Active `ai-context/workflows/plans-trips.md#req-trip-010` now contains the following meaning, without a new requirement ID:

- The Trip creator may read and add Trip Notes through creator authority independent of audience or Family Group membership; this privilege is limited to their own Trip and does not extend to travelers, administrators, or other non-audience people.
- Author deletion is a soft delete. It removes the note from normal display while retaining the original record and its provenance, including the author, creation time, and updated time when present.
- Creator hiding removes another author's note from normal display and retains an immutable moderation record. The author cannot edit or delete a creator-hidden note through normal Trip Notes actions, and the creator cannot rewrite its content.
- MVP states are `ACTIVE`, `AUTHOR_DELETED`, and `CREATOR_HIDDEN`; no restore/recovery UI is defined.

`DEC-060` records the owner decision, and `VER-CTX-1.21` verifies its activation. `REQ-PRIV-022` already establishes independent creator Trip access, so no privacy requirement change or new requirement ID was needed. This story file is not itself canonical authority.

## Functional scope and permissions

- A current eligible Trip audience member may read active Notes and add Notes. A traveler only receives these capabilities if separately authorized as an audience member.
- The creator may read active Notes and add Notes through own-Trip creator authority even when not an audience recipient or active Family Group member.
- The author of an `ACTIVE` note may edit its content or soft-delete it. The author identity and creation timestamp remain unchanged by an edit.
- The creator may hide another author's `ACTIVE` note. The moderation action has no content field and cannot rewrite or delete the retained content.
- `AUTHOR_DELETED` and `CREATOR_HIDDEN` notes are excluded from every normal Notes API response and UI view, including direct note lookup if one exists. Neither state has a normal edit, delete, hide, or restore action in this item.
- Notes do not create Trip access. Authorization is reevaluated against the Trip on every read and write, including after audience eligibility ends. The creator exception applies only to that creator's own Trip.
- Non-system contacts, unauthorized travelers, administrators without Trip access, outsiders, people with only a Family relationship, and group members who are not in the Trip audience receive no Notes capability.
- Visible Notes show author, created time, and updated time when edited. Content is plain text; no chat behavior is introduced.

| Actor | Read active Notes | Add | Edit active note | Soft-delete active note | Hide another author's active note |
|---|---:|---:|---:|---:|---:|
| Current Trip audience member | Yes | Yes | Own only | Own only | No |
| Trip creator, including after membership loss | Yes | Yes | Own only | Own only | Yes |
| Traveler without audience access | No | No | No | No | No |
| Non-system contact | No | No | No | No | No |
| Other user, including administrator without audience access, relationship-only user, or unselected group member | No | No | No | No | No |

## State model

| State | Normal display | Retention | Normal permitted transition |
|---|---|---|---|
| `ACTIVE` | Visible to authorized creator/audience | Record retained | Author edit; author soft-delete to `AUTHOR_DELETED`; creator may hide another author's note to `CREATOR_HIDDEN` |
| `AUTHOR_DELETED` | Hidden | Original record and provenance retained | No normal action or restore/recovery in this item |
| `CREATOR_HIDDEN` | Hidden | Original immutable moderation record retained | No normal action or restore/recovery in this item |

Retained removed Notes remain associated with the Trip while its record exists. The precise storage shape, global retention duration, and later Trip disposal rules are not set by this story. It requires retained records and no ordinary hard delete; it does not introduce an audit/history UI or a full revision log.

## Validation rules

- Note content is required, non-whitespace plain text. Trim surrounding whitespace while preserving meaningful internal line breaks.
- Enforce the owner-approved 1,000-character item-level maximum. This is a `WI-007-004` validation choice, not canonical Trip product behavior, an import of Plan permissions, or a cross-project standard.
- Render content as text and never interpret markup. Server validation is authoritative; the UI mirrors the bound and displays server errors.
- Author ID, Trip ID association, creation/update times, and state are server-controlled. Client-supplied values cannot impersonate an author or bypass state transitions.

## Acceptance checks

1. **`AC-01 — Audience read`**: A current authorized Trip audience member can read normally visible `ACTIVE` Notes on a Trip they may view.
2. **`AC-02 — Audience add`**: A current authorized Trip audience member can add a Note. The server assigns its Trip, authenticated author, creation timestamp, and initial `ACTIVE` state.
3. **`AC-03 — Creator Notes access`**: The creator can read visible Notes and add Notes through own-Trip creator authority, whether or not included in the audience and even after eligible Family Group membership ends. This grants no other group-data access.
4. **`AC-04 — Own edit`**: An author with current Notes access can edit only their own `ACTIVE` Note. The author and creation timestamp remain unchanged; the updated timestamp records the edit.
5. **`AC-05 — Own soft delete`**: An author with current Notes access can change only their own `ACTIVE` Note to `AUTHOR_DELETED`. Normal display excludes it, while original content, author, created time, applicable updated time, and information distinguishing author deletion remain retained. No ordinary hard delete occurs.
6. **`AC-06 — Other author denied`**: An audience member cannot edit or delete another author's Note. The creator cannot use an author action to edit or soft-delete another author's Note.
7. **`AC-07 — No creator rewrite`**: No creator action, including over-posting, can rewrite another author's Note content.
8. **`AC-08 — Creator hide`**: The creator can change another author's `ACTIVE` Note to `CREATOR_HIDDEN` without changing its original content or provenance.
9. **`AC-09 — Hidden-note immutability`**: After creator hiding, the author cannot edit or delete the Note through normal UI or API; the creator cannot rewrite it. Repeated or stale mutations leave the retained record unchanged.
10. **`AC-10 — Author-deleted display`**: `AUTHOR_DELETED` Notes are absent from the normal Trip Notes list and UI.
11. **`AC-11 — Creator-hidden display`**: `CREATOR_HIDDEN` Notes are absent from the normal Trip Notes list and UI.
12. **`AC-12 — Retained provenance`**: `AUTHOR_DELETED` and `CREATOR_HIDDEN` remain distinct, associated with the Trip while it exists, and retain original content, author, created time, applicable updated time, and enough transition information to distinguish deletion from moderation. Normal display identifies active-note author, creation time, and edit time when present.
13. **`AC-13 — Traveler-only denial`**: Traveler status alone grants no Notes read or mutation capability.
14. **`AC-14 — Contact denial`**: A non-system Trip contact record grants no Notes read or mutation capability.
15. **`AC-15 — Administrator denial`**: Family Administrator status alone grants no Notes read or mutation capability.
16. **`AC-16 — Relationship/group-only denial`**: Family relationship alone and Family Group membership without Trip audience access grant no Notes access; an unrelated user is denied under the existing unavailable-resource pattern.
17. **`AC-17 — Audience lifecycle`**: Selected-person access and group recipient snapshots remain controlling. Losing eligible membership revokes audience Notes access; rejoining or joining the group later does not silently grant it. Creator authority remains separate and valid for the creator's own Trip.
18. **`AC-18 — Cross-Trip denial`**: A Note identifier from one Trip cannot be used through another Trip to read or mutate it.
19. **`AC-19 — No retained-record retrieval`**: Neither removed state is returned by any normal Notes list, detail, or direct lookup API, including to the former author or creator. Retained records create no alternate Trip access path; no restore/recovery UI or endpoint is introduced.
20. **`AC-20 — Private-data separation`**: Notes access grants no Sharing Session, location, Check-In, administrator override, or unrelated private-data access.
21. **`AC-21 — Content validation`**: Required non-whitespace content is limited to 1,000 characters as this work item's validation rule. Valid internal line breaks survive; content renders as plain text without executable markup, with usable UI and server errors for invalid input.
22. **`AC-22 — No chat or feed`**: No replies, reactions, threads, chat behavior, general activity feed, Notes notifications, full revision-history UI, or restore/recovery UI is introduced.
23. **`AC-23 — Regression`**: Completed Plan audience and Plan Notes behavior and `WI-007-003` Trip creation, initial audience, detail, and creator-edit behavior remain unchanged.
24. **`AC-24 — Evidence`**: A separate verification report maps every criterion and cited requirement to concrete evidence, including SQL-gated retained-state and negative-access tests, API/UI checks, and regression/build results.

## Out of scope

- Post-creation Trip audience editing
- Check-In execution, Sharing Session execution, location data, and Trip completion
- Note restore/recovery UI, audit browsing UI, or a full revision-history feature
- Notifications for Notes
- Non-system contact authoring or access
- Chat, replies, reactions, typing/presence, or general activity feed
- Hard deletion through normal author or creator actions
- New moderation states or ownership transfer
- Changes to completed Plan behavior

## Likely implementation areas and persistence considerations

| Area | Likely impact |
|---|---|
| Domain and application | New Trip Note entity/state, request and view contracts, validation, and note service/repository interfaces under `src/FamilySafety.Domain/Trips/` and `src/FamilySafety.Application/Trips/` |
| SQL persistence | Additive Trip Notes table and EF migration; note author/Trip links, content, timestamps, state, and optional transition times; authorized queries reuse the Trip access boundary |
| API | Bounded nested Notes routes under `/api/trips/{tripId}/notes`; separate author edit/soft-delete and creator hide operations; existing unauthenticated/unavailable/validation patterns |
| Angular | Small Notes area in Trip Detail with visible-note list, add form, own-note actions, creator hide action, provenance and safe plain-text display |
| Tests | Focused xUnit service, SQL persistence, API authorization, and Angular component tests, plus full-suite regression |

Creator-hidden and author-deleted records must remain in persistence but be omitted from ordinary list/detail projections. Every mutation checks both the current Trip permission and the note's author/state; UI controls are not the security boundary. The exact EF mapping and timestamp representation follow existing architecture conventions during implementation.

## Tasks

- [x] **`TASK-001` — Execution readiness:** Confirm explicit work-item approval and execution authorization under active `CTX-1.21` and load the bounded context manifest.
- [x] **`TASK-002` — Domain/application:** Add the Note state and contracts, content validation, and role- and state-aware operations.
- [x] **`TASK-003` — Persistence:** Add an additive migration and authorized Note queries/mutations that retain removed records while excluding them from normal responses.
- [x] **`TASK-004` — API:** Add bounded list/add/author edit/author soft-delete/creator hide routes with current Trip authorization and unavailable-resource behavior.
- [x] **`TASK-005` — UI:** Add the Trip Detail Notes section, provenance, safe text rendering, and only the actions each actor may take.
- [x] **`TASK-006` — Tests:** Cover state transitions, negative access, SQL retention, API, UI, and Plan/Trip regression without implementing adjacent Phase 7 capabilities.
- [x] **`TASK-007` — Verification:** Produce `VER-WI-007-004`; mark this item `DONE` only after the required checks pass.

## Test plan

- **Domain/application:** content validation, state transitions, actor-specific permissions, immutable author/created time, updated time on edit, and no action on creator-hidden notes.
- **SQL:** create/edit round trip; soft-delete and hide retention and provenance; every normal-query exclusion path; selected-person and group snapshot access; audience membership loss and rejoin; creator after departure; traveler/admin/contact/relationship-only/unselected-group/outsider denial; cross-Trip ID denial; migration application.
- **API:** 401/unavailable/validation patterns; creator and audience routes; non-author denials; over-posting and stale/concurrent mutation attempts; no removed content in any normal list, detail, or direct lookup response.
- **Angular:** role-appropriate controls, note creation/edit/soft-delete/hide, provenance, safe text and line-break display, and absence of chat/feed/recovery UI.
- **Regression:** SQL-gated full .NET suite, focused Plan persistence checks, Angular suite, and production build.

## Verification plan and Definition of Done

1. Confirm the active `CTX-1.21` clarification of `REQ-TRIP-010` remains compatible with this story before execution.
2. Obtain explicit owner approval and execution authorization for this `DEEP` work item.
3. Implement only the bounded behavior above and generate an additive migration.
4. Run focused tests, the full SQL-gated .NET suite, Angular tests, and production build. A skipped SQL gate is not passing persistence evidence.
5. Produce `VER-WI-007-004` with each AC and requirement mapped to `PASS`, `FAIL`, `BLOCKED`, or justified `NOT APPLICABLE`. Label self-verification when applicable.
6. Mark the work item `DONE` only after verification passes and material limitations are recorded.

## Risks and remaining ambiguity

- **Canonical alignment:** `CTX-1.21` passed `VER-CTX-1.21` and is active. Creator access, author soft deletion, and immutable creator-hidden Notes are now canonical; the remaining gate is separate work-item approval and execution authorization.
- **Privacy:** A Notes endpoint must not bypass the Trip audience snapshot or broaden the creator's own-Trip exception to other group data.
- **Retention:** Records must be retained as approved, but long-term retention duration, archival, and recovery UI are not specified here. They are outside this slice; no duration is inferred.
- **Concurrency:** Mutations must not overwrite a note after it becomes hidden or deleted. Exact locking/version mechanism is an implementation choice.

There is no remaining product ambiguity within the approved MVP Notes behavior. The owner-approved 1,000-character bound is an item-level validation choice for this work item and is not a canonical Trip requirement.

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner, explicit pasted execution authorization
- **Authorized at:** 2026-09-27
- **Authorized scope:** `WI-007-004` only, under `CTX-1.21`.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| Story alignment with active Notes behavior | `REQ-TRIP-010`, `REQ-PRIV-022`, `DEC-060`, and `VER-CTX-1.21` | `PASS — CONTEXT ONLY` |
| Application behavior and acceptance checks | `VER-WI-007-004`, 24 mapped checks | `PASS` |
| SQL, API, UI, regression, and build checks | 105 SQL-gated .NET tests, 20 Angular tests, production build | `PASS` |

- **Verification report:** `VER-WI-007-004`
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `PASS`
