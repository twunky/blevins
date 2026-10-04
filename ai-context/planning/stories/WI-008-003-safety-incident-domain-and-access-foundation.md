---
Status: DONE
Authority: Project-owner approved and execution-authorized for WI-008-003 only (2026-10-01), against CTX-1.26; planning basis PRP-029 / DEC-071
Context-Baseline: CTX-1.26
Version: 1.2
---

# WI-008-003 — Safety Incident Domain and Access Foundation

## Metadata

- **Work Item ID:** `WI-008-003`
- **Status:** `DONE / PASS` — `OWNER APPROVED` (owner review 2026-10-01). The project owner approved this `DEEP` work item, authorized its execution against `CTX-1.26`, and accepted the completed implementation and verification on 2026-10-01. See `planning/verification/VER-WI-008-003.md` and [Owner review](#owner-review-2026-10-01).
- **Phase:** `8 — Safety Coordination`, `CURRENT / IN PROGRESS`
- **Context baseline:** `CTX-1.26`
- **Model tier:** `DEEP` — safety, privacy, authorization, history, and retention
- **Approved planning basis:** `PRP-029` (`APPROVED`), `DEC-071`, and `DEC-069` (denied-audit pattern). Canonical `REQ-SAFE-016` and `REQ-PRIV-026` control wherever PRP-029 wording differs.
- **Depends on:** `WI-008-001` (`DONE / PASS`), for the patterns reused and for Concern access used by lineage.
- **Followed by:** `WI-008-004 — Incident MVP user interface`. It is not created here.
- **Execution authorization:** `YES` — see [Execution authorization](#execution-authorization)

## Goal and story boundary

Establish one complete server-side Safety Incident foundation for one Family Group. It must be fully verifiable without any Incident UI and covers:

- manual creation, directly or deliberately from a Concern, with a non-authorizing lineage reference;
- explicit, membership-bound participant grants;
- one coordinator, with explicit handoff;
- attributed entries with provenance, corrections, and challenges;
- record-only contact attempts;
- simple tasks;
- a read-time timeline;
- coordinator-only resolution;
- `Open` / `Frozen` / `Resolved` with inactivity and coordinator-loss freeze;
- immediate access loss;
- 365-day product and audit retention, with request-time expiry and bounded cleanup;
- metadata-only audit with `DEC-069` denied summaries;
- EF persistence, including an additive migration;
- a private HTTP/JSON API;
- SQL-backed tests.

### One story, not split

This should remain **one work item**. The reasons:

- **Every mutation needs the same protections at once.** Each Incident mutation (entry, contact attempt, task action, participant change, handoff, resolution) needs:
  - the same grant check;
  - the same effective-state check, so it is not frozen or expired;
  - the same commit-time revalidation;
  - the same activity clock;
  - the same audit;
  - the same whole-aggregate expiry deletion.
- **A partial API would be unsafe.** If tasks or contact attempts were deferred, the first item would ship an API that persists sensitive history before its full aggregate, deletion scope, and lifecycle interplay were verified. That interplay includes task cancellation authority after a coordinator handoff, and Open tasks at freeze. A second item would then need its own additive migration and would reopen retention and cleanup.
- **The rules interlock.** Coordinator handoff, coordinator-loss freeze, and participant-management rules are verified together or not at all.
- **The Concern precedent fits this size.** WI-008-001 delivered the same kind of server-first foundation in one item. The Incident adds three record types: tasks, coordinator changes, and contact-attempt fields on entries. It reuses every proven mechanism unchanged.

**Contingency only.** If execution shows the item cannot be completed coherently, the smallest safe split is to move **tasks** into a follow-on server item with its own additive migration. That follow-on must be completed before WI-008-004 consumes the API. Do not split preemptively, and do not expose a partial API to a UI.

The UI is a separate interaction and accessibility surface and remains `WI-008-004`.

## Requirement traceability

| Canonical anchor | Applied in this item |
|---|---|
| `REQ-PROD-001`, `REQ-PROD-004`, `REQ-PROD-009`, `REQ-PROD-012` | Care without surveillance. Certainty distinctions. No automatic danger or dispatch. Manual creation only. The Start-an-Incident shortcut transfers no information or audience. |
| `REQ-PRIV-005` through `REQ-PRIV-008`, `REQ-PRIV-011`, `REQ-PRIV-012` | Relationship, Administrator, Operator, membership, and Incident status never bypass authorization. Membership end ends access. |
| `REQ-PRIV-009`, `REQ-PRIV-026` | Grants, permissions, nondisclosure, membership loss, protected-information boundary, lineage, 365-day retention, audit, backup, and restore. |
| `REQ-PRIV-019`, `REQ-PRIV-024` | Trusted Contact designation grants no Incident access. |
| `REQ-PRIV-025` | Consumed read-only, to evaluate the Concern creator's current access for create-from-Concern and lineage visibility. Concern behavior is not changed. |
| `REQ-SAFE-001` through `REQ-SAFE-004` | Uncertainty, the five categories, provenance, Unknowns, and append-only correction. |
| `REQ-SAFE-007` through `REQ-SAFE-014` | Incident workspace content, deferred labels, coordinator and participants, provenance, contact attempts, tasks, timeline, and resolution. They are consumed as amended and scoped by `REQ-SAFE-016`. Other-domain fields such as shared location and Trip data are excluded. |
| `REQ-SAFE-015` | Starting an Incident leaves the Concern unchanged. |
| `REQ-SAFE-016` | The complete MVP Incident workflow. |
| `REQ-NOTIFY-001` through `REQ-NOTIFY-003` | Not implemented. No notifications are sent. |

`requirements-index.md` confirms that `REQ-PRIV-026` and `REQ-SAFE-016` are approved in `CTX-1.26`.

## Required context manifest

| Context | Why | When |
|---|---|---|
| `ai-context/README.md`; `ai-context/project-status.md` | Operating rules and authorization checkpoint | Entry |
| `ai-context/workflows/safety.md#req-safe-016`; `ai-context/core/privacy-safety-contract.md#req-priv-026` | The controlling Incident workflow and privacy rules | Before execution and verification |
| `ai-context/core/privacy-safety-contract.md#req-safe-001`–`#req-safe-004`; `ai-context/workflows/safety.md#req-safe-007`–`#req-safe-014` | Provenance, categories, and the amended Incident workspace anchors | Before execution |
| `ai-context/core/privacy-safety-contract.md#req-priv-025` | The Concern access rule used for create-from-Concern and lineage visibility | Before execution |
| `ai-context/decision-log.md#dec-069`, `#dec-071` | Denied-summary shape and the owner's Incident decisions | Before execution |
| `ai-context/planning/stories/WI-008-001-concern-domain-and-access-foundation.md`; `VER-WI-008-001.md` | The proven server-first pattern and its evidence format | Before implementation design |
| Concern source: `src/FamilySafety.Domain/Concerns/`, `src/FamilySafety.Application/Concerns/`, `src/FamilySafety.Infrastructure/Persistence/EfConcernRepository.cs`, the Concern mapping in `FamilySafetyDbContext.cs`, `src/FamilySafety.Api/Concerns/`, `tests/FamilySafety.Foundation.Tests/Concern*Tests.cs` | Patterns to reuse (listed below) | Before implementation design |
| `ai-context/templates/verification.md` | `VER-WI-008-003` structure | Before verification |

## Context intentionally excluded

- Incident UI and the Angular application (`WI-008-004`).
- `claude-handoff/reference` mockups.
- Sharing/Check-In workflows. They are referenced only as boundaries; `OD-002` is open.
- Events, Plans, and Trips workflows.
- Safety Circle and Trusted Contact management.
- Bootstrap and archive material.
- Historical verification reports other than VER-WI-008-001.
- PRP-028's historical retention packet.

`OD-001` and `OD-002` remain open and do not block this item.

## Proven patterns to reuse (do not reinvent)

| Pattern | Where it lives today | Incident use |
|---|---|---|
| Grants bound to membership `Id` + `AccessVersion`, and `Matches(...)` | `ConcernParticipantGrant`; `EfConcernRepository.Matches` / `HasParticipantGrantAsync` | One grant type for **all** Incident access, including the coordinator. |
| Actual loss time from `FamilyGroupMembership.EndedAt` | `NormalizeLifecycle`, `RevokeLostParticipantsAsync` | Coordinator-loss freeze time and revocation audit time. |
| Serializable transaction plus Family Group row lock (`ExecuteUpdate` no-op on `FamilyGroups`) | `LockFamilyGroupAsync` in the Concern, Trip, and Event repositories | Every Incident read-with-normalization, command, and reconciliation step. |
| Request-time effective lifecycle with idempotent persisted normalization | `Concern.IsEffectivelyFrozen` / `EffectiveFreezeAt` / `NormalizeLifecycle` | Same, with reason `CoordinatorEligibilityLost`. |
| Nondisclosing `ConcernUnavailableException` → `404`; `401` for no account | `ConcernService`, `ConcernEndpoints` | `IncidentUnavailableException` → `404`. |
| Denied-summary upsert (`UPDLOCK, SERIALIZABLE`, unique index on actor + action + bucket) | `AddDeniedAuditAsync`, `ConcernAuditEvent.DeniedSummary`, migration `BoundConcernDeniedAuditSummaries` | A separate `IncidentAuditEvents` table with `IncidentReadDenied` / `IncidentWriteDenied`. |
| Audit table with no product foreign key; `ExecuteDelete` by age | `ConcernAuditEvents` mapping comment; `ReconcileAsync` | Same, at 365 days. |
| In-process reconciliation `BackgroundService` that runs immediately, then every 2 minutes | `ConcernMaintenanceService` | A separate `IncidentMaintenanceService` with the same shape. |
| Outdated handling: a new entry keeps the original category, with `IsOutdated` | `ConcernService.AddEntryAsync`, `ConcernEntry` | Same validation rules. |
| SQL tests gated by `RUN_FAMILYSAFETY_SQL_TESTS=1` with `ManualTimeProvider` | `ConcernPersistenceTests` | Same gate and helpers. |

Reuse means "follow the same mechanism." Copying small private helpers into the Incident repository is acceptable. **Do not** create a shared generic Safety base class, policy engine, or workflow framework. Do not refactor Concern code except for the narrow read-only access check described under [Concern lineage](#concern-lineage).

## Functional scope

### Creation

- There is one create command, under one Family Group. The request contains:
  - the subject profile ID;
  - a **reason**;
  - provenance for the reason: category, and source or explicit unknown source;
  - zero or more participant profile IDs;
  - an optional `OriginatingConcernId`.
- The creator must have an active adult membership in that group at commit.
- The subject must be an active adult member of the same group at commit. Self is allowed.
- Administrator status is irrelevant.
- The reason is persisted as the **first `Summary` entry**, attributed to the creator, with its provenance.
  - Canon requires every entry to carry a category, so the create command requires one. The server applies no default category.
  - Optional "concern began at" and "last contact" details are **not** separate create fields. Contributors add them as ordinary entries afterward.
- The creator receives a participant grant capturing their membership `Id` and `AccessVersion`, and that grant is the coordinator.
- Selected participants receive grants. The set must be distinct, eligible, and at most 250. The subject is included only if selected. An invalid set rejects the whole creation atomically, with no partial records.
- There is no other creation path. Nothing creates an Incident automatically.

### Concern lineage

- When `OriginatingConcernId` is supplied, the creation is rejected with the uniform unavailable result unless **all** of these hold:
  - the Concern exists and is not effectively expired;
  - the caller is that Concern's **creator** and currently holds valid Concern creator access under `REQ-PRIV-025` (the creator grant's membership ID and version still match an active membership);
  - the Concern belongs to the **same Family Group** as the new Incident.

  The Concern may be in any non-expired state, including Frozen or Resolved.
- The subject may differ from the Concern's subject. The UI prefill is a WI-008-004 concern; the server copies nothing.
- The Incident stores `OriginatingConcernId` as a plain column with **no foreign key** to `Concerns`. Concern expiry and deletion must neither cascade nor block, and the Incident's own expiry deletes the reference.
- Creating from a Concern must not:
  - change the Concern's state, `LastAuthorizedMutationAt`, participants, entries, freeze, or resolution;
  - write any Concern audit event or Concern denied summary;
  - copy any Concern data or participants into the Incident.
- **Visibility.** Incident detail returns `OriginatingConcernId` only when the caller is currently authorized to read **both** the Incident and that Concern, under `REQ-PRIV-025` effective lifecycle and expiry. Otherwise the field is `null`. The response must be **indistinguishable from an Incident with no lineage.** It is never returned in list items.
- **How to check Concern access.** Add one narrow, **read-only** Concern access query, for example `IConcernAccessQuery.CanReadAsync(accountId, concernId)` implemented beside `EfConcernRepository`.
  - It evaluates the Concern's creator or participant grant against current membership, and the Concern's *effective* freeze and expiry, exactly as Concern reads do.
  - It persists nothing and writes no Concern audit.
  - A second, creator-only variant (or a parameter) serves create-from-Concern.
  - This adds a query. It does not change Concern permissions or behavior, and existing Concern tests must still pass.
- Lineage never grants, extends, or implies access to either record.

### Participants and coordinator

- **Grants.** All Incident access, including the coordinator's, is through `IncidentParticipantGrant`. A grant is valid only while its captured membership row is active at the captured `AccessVersion`. There is one effective grant per profile per Incident.
- **Participant management.** While Open, the coordinator replaces the **non-coordinator** participant set.
  - The request lists every desired participant other than the coordinator.
  - The coordinator's grant is never affected by this command. **No API path removes the coordinator's grant.**
  - The whole set is revalidated at commit and applied atomically. Unchanged valid grants are kept; dropped ones get `RemovedAt`.
  - A new selection of someone whose old grant is stale creates a **new** grant. It never refreshes an old grant.
  - The subject is never added implicitly.
- **Coordinator handoff.** While Open, the coordinator transfers coordination to the holder of a **current valid participant grant** who is not the current coordinator.
  - The handoff takes effect immediately, in one transaction.
  - The former coordinator's grant remains as an ordinary participant grant while valid.
  - It is recorded as an `IncidentCoordinatorChange` row plus an audit event.
  - The creator has no authority beyond their current grant role. `CreatedBy*` is attribution only.
- **Coordinator-only powers:** participant management, handoff, `Summary` entries, creating tasks for others, cancelling any task, and resolution. Each also requires the coordinator's grant to be currently valid.
- **Coordinator reference.** Avoid a foreign-key cycle between the Incident and its grants that would complicate whole-aggregate deletion. Recommended: a `CoordinatorGrantId` column without a database foreign key, with integrity enforced in the domain, repository, and tests. A nullable foreign key cleared before deletion is acceptable if simpler in EF. Do not use an `IsCoordinator` flag with a unique filtered index; the update order during handoff can violate it transiently.

### Entries and provenance

`IncidentEntry` is one explicit, immutable-after-insert type.

| Field | Rule |
|---|---|
| `Kind` | `Observation`, `Unknown`, `Summary` (coordinator only), `ContactAttempt`, `TaskResult` (created only by task completion), `Correction`, or `Challenge`. |
| Contributor | Account ID and profile ID. |
| `RecordedAt` | Server time. |
| `EventAt` | Optional. |
| `Category` | `Confirmed`, `Reported`, `Assumption`, or `Unknown`. `Outdated` is expressed as in Concern: the request carries `Outdated` plus the original category, and the stored row keeps the original category with `IsOutdated = true`. |
| Source | A participant-safe source description, or `SourceUnknown`. Exactly one. `Confirmed` requires a source. |
| `RelatedEntryId` | For `Correction` and `Challenge`. |
| `RelatedTaskId` | For `TaskResult`. |
| `Explanation` | Required for `Correction` and `Challenge`. |
| Contact-attempt fields | Nullable, required only for `ContactAttempt`. See [Contact attempts](#contact-attempts). |

Additional rules:

- Mirror Concern validation: an `Unknown` entry uses the `Unknown` category, an `Observation` cannot be `Unknown`, and entries do not reference other entries except as allowed above.
- **No numeric scores.**
- A `Correction` targets an entry by the **same** contributor, of any kind.
- A `Challenge` targets another contributor's entry.
- Neither changes its target. There is no update or delete command for entries.

Use a separate `IncidentInformationCategory` enum, mirroring the canonical five values, rather than renaming or sharing `ConcernInformationCategory`. That keeps Concern code untouched.

### Contact attempts

Contact attempts are coordination records only. They are stored as `IncidentEntry` rows with `Kind = ContactAttempt`.

| Field | Rule |
|---|---|
| Attempted by | `AttemptedByDescription` is optional text. `null` means the contributor made the attempt. Otherwise it names the person the contributor is reporting about, normally with the `Reported` category. |
| `TargetLabel` | Required short text label. Not a phone, email, or address field. |
| `ContactMethod` | `Call`, `TextMessage`, `Email`, `InAppMessage`, `InPerson`, or `Other`. |
| `ContactOutcome` | `Reached`, `NoAnswer`, `LeftMessage`, `SentNoReplyYet`, `Other`, or `Unknown`. |
| Attempt time | `EventAt`, or `EventTimeUnknown = true`. Exactly one is required for this kind. |
| Notes | Optional; stored in `Text`. |
| Category and source | As for every entry. |

- The server never reads Profile contact fields, Trusted Contacts, or any other domain to fill these values.
- `Reached` changes no state.
- No outbound communication of any kind.

### Tasks

`IncidentTask` fields:

- `Description` (immutable);
- assignee account, profile, and `AssigneeGrantId`, the grant valid at assignment;
- `Status`: `Open`, `Done`, or `Cancelled`;
- `CreatedByAccountId` / `ProfileId` and `CreatedAt`;
- `CompletedAt`;
- `CancelledAt`, `CancelledByAccountId`, and `CancelReason`;
- a concurrency token.

The result is the `TaskResult` entry, linked through `IncidentEntry.RelatedTaskId`. The task stores **no** `ResultEntryId`, which avoids a foreign-key cycle.

| Action | Rule |
|---|---|
| Create | The coordinator may assign to any current valid grant holder, including themself. A participant may create a task only **assigned to themself**. The assignee's grant is validated at commit. |
| Complete | Only the assignee, while **the same `AssigneeGrantId` is still valid**, on an `Open` task in an `Open` Incident. Completion requires a result (text, category, source) and writes a `TaskResult` entry. It sets `Done` and `CompletedAt`. A task can be completed only once. |
| Cancel | The coordinator may cancel **any** `Open` task, with a reason. A participant may cancel **only a task they created that is assigned to themself**, with a reason. A participant may **not** cancel another participant's task. |
| Reassign or edit | No command exists. To change an assignment, cancel and create again. |
| After access loss | A task whose assignee lost access stays as-is. The coordinator may cancel it. A re-added assignee gets a new grant and cannot complete the old task. |
| At freeze or resolution | `Open` tasks become read-only, and the view marks them "not completed." The stored status is not rewritten. |

### Timeline (read-time projection)

The detail response includes a `Timeline` computed at read time from persisted records only. **No timeline table and no event sourcing.**

**Items:**

- Incident created;
- each entry (by kind);
- task created, completed, and cancelled;
- participant added or removed by the coordinator;
- participant access ended (system; shown **without a reason**, so membership status is not disclosed);
- coordinator handed off;
- Frozen (with reason);
- Resolved.

Each item carries its type, its times, the actor's display name or "System", the category where applicable, and the related entry or task IDs.

**Ordering:** `EventAt` when known, otherwise `RecordedAt`, then `RecordedAt`, then a fixed item-type precedence, then the record ID. This order is deterministic for equal timestamps. Both times are exposed; an item without a known event time is identifiable as such.

### Resolution

Resolution is coordinator-only, while Open. It is stored on the Incident.

**Required:**

- `ResolutionSummary`;
- `OutcomeText`, with an `OutcomeCategory` of `Confirmed`, `Reported`, `Assumption`, or `Unknown` (not `Outdated`), and a source or explicit unknown source. `Confirmed` requires a source.
- **exactly one** of `ContactRestoredAt` or `ContactRestoredTimeUnknown = true`.

**Optional:** `AssistanceProvided`, `UnresolvedFollowUp`, and `Lessons`.

**System-recorded:** `ResolvedAt` and resolved-by.

There are **no** preset verdict fields or enums such as Safe, Dangerous, or Emergency resolved. Resolution sets `Resolved`. There is no reopen command.

### Subject membership loss

- There is no state change, and other grants are unaffected.
- The subject's display identity continues to be shown, as Concern does.
- If the subject held a grant, it becomes invalid through the normal membership check.

## Lifecycle and time

| State | Reads | Mutations | Transition |
|---|---|---|---|
| `Open` | Valid grant holders | All rules above | Created manually. It freezes at the **earlier** of `LastAuthorizedMutationAt + 30 days` (reason `Inactivity`) and the coordinator grant's actual loss time, `EndedAt` (reason `CoordinatorEligibilityLost`). The coordinator may resolve first. |
| `Frozen` | Valid grant holders, read-only. A departed coordinator has no access. | None | Terminal. Expires 365 days after `FrozenAt`. |
| `Resolved` | Valid grant holders, read-only | None | Terminal and never Frozen. Expires 365 days after `ResolvedAt`. |

- **Authorized mutations** reset inactivity. They are: creation, participant replacement, handoff, every entry kind, task create/complete/cancel, and resolution.
- These do **not** reset inactivity: reads, denied attempts, lineage checks, audit writes, and maintenance.
- Use injected `TimeProvider` and UTC.
- Persist the state, transition time, and reason once. Derive `ExpiresAt` rather than storing a second deadline.
- Apply the **effective** lifecycle and expiry before authorization and response shaping on every request. Materialize a due freeze exactly once, with its system audit event. When both freeze causes have occurred, the earlier one controls the reason, time, and expiry clock.
- Freezing never resolves, notifies, escalates, creates anything, or implies safety or danger.

## Authorization matrix

All checks run server-side against the specific Incident, current membership, and the captured grant epoch. **None** of the following grants anything: subject status, Family membership alone, Family Administrator, Application Operator, relationship, Safety Circle, Trusted Contact, Concern participation, lineage, or Family-context selection.

| Action | Coordinator (valid grant), Open | Participant (valid grant), Open | Subject not selected / other member / Administrator / Concern participant | Former, removed, rejoined, stale-grant, other-group, or no-profile caller | Frozen or Resolved (valid grant) |
|---|---|---|---|---|---|
| Create (direct) | Allowed under creation rules | Allowed | Allowed if eligible | Denied | — |
| Create from Concern | Only if Concern creator with current access, same group | Same | Same | Denied | — |
| List / detail / timeline | Allowed | Allowed | Omitted / unavailable | Omitted / unavailable | Read-only before expiry |
| See lineage | Only if also currently authorized for the Concern | Same | — | — | Same |
| Observation / Unknown / contact attempt | Allowed | Allowed | Denied | Denied | Denied |
| `Summary` entry | Allowed | Denied | Denied | Denied | Denied |
| Correct own entry / challenge another's | Allowed | Allowed | Denied | Denied | Denied |
| Create task | Any valid assignee | Self-assigned only | Denied | Denied | Denied |
| Complete task | If assignee with same grant | If assignee with same grant | Denied | Denied | Denied |
| Cancel task | Any `Open` task | Own self-created, self-assigned only | Denied | Denied | Denied |
| Replace participants | Allowed (coordinator grant untouched) | Denied | Denied | Denied | Denied |
| Hand off coordinator | To a current valid participant | Denied | Denied | Denied | Denied |
| Resolve | Allowed | Denied | Denied | Denied | Denied |
| Reopen / edit / delete history / reassign task / read audit | No endpoint exists | — | — | — | — |

**Uniform unavailable result.** Every one of the following uses the existing `404` pattern and reveals no existence, subject, participants, or content:

- a missing Incident ID;
- an unauthorized Incident, including the unselected subject, an Administrator, or a Concern participant;
- an expired Incident, before or after cleanup;
- a removed participant;
- a caller whose membership was lost;
- a caller who rejoined;
- a stale grant;
- an unauthorized nested entry or task ID;
- a denied create or create-from-Concern;
- any coordinator-only command by a participant.

Request-shape validation that does not depend on the target may run first, as in Concern. Target-dependent validation runs only after authority is established. `401` is used when there is no current account.

## Membership loss and rejoin

| Event | Behavior |
|---|---|
| Participant loses membership | Denied immediately on the next request, because the grant no longer matches. Normalization or reconciliation records `RemovedAt`, the eligibility flag, and one `AccessRevoked` audit event at `EndedAt`. Prior entries keep their attribution. |
| Removal by the coordinator | Same access effect, with participant-change audit. |
| Rejoin | No restoration, ever. Only an explicit new selection by the coordinator while Open creates a new grant. |
| Coordinator loses membership | Effective freeze at `EndedAt`; coordinator revocation is audited once; no promotion. Rejoin neither restores nor unfreezes. |
| Subject loses membership | No effect on the Incident (see Functional scope). |

There is no production membership-end command today; `FamilyGroupMembership.End` is called only by tests. So detection stays request-time and reconciliation-based, exactly as in Concern. **Do not add a membership event bus or change join/leave semantics.**

## Audit and retention mechanism

- **Table.** `IncidentAuditEvent` is a separate table with the same shape as `ConcernAuditEvent`. It has **no foreign key** to Incident tables and expires at `OccurredAt + 365 days`.
- **Successful individual events** (metadata only):
  - `IncidentCreated`;
  - `ParticipantAdded`, `ParticipantRemoved`;
  - `CoordinatorTransferred` (from and to profile IDs);
  - `ObservationCreated`, `UnknownCreated`, `SummaryCreated`, `ContactAttemptRecorded`, `CorrectionCreated`, `ChallengeCreated`;
  - `TaskCreated`, `TaskCompleted`, `TaskCancelled` (task ID and assignee ID);
  - `Frozen` (system), `Resolved`;
  - `AccessRevoked` (system, affected profile, at the loss time).

  Each records the Incident ID, actor, action, time, authorization result, and affected profile, entry, or task ID only. **No text, target labels, notes, descriptions, resolution content, or source details.**
- **Denied attempts.** At most one summary per authenticated actor, category, and UTC hour, using the `DEC-069` `UPDLOCK, SERIALIZABLE` upsert against a unique index. The category is `IncidentReadDenied` (detail reads) or `IncidentWriteDenied` (every command, including denied creates). Each summary stores the first and last attempt and a count, and **no Incident ID**. The list endpoint never records a denial.
- **No audit endpoint or viewer.**
- **Reconciliation.** `IncidentMaintenanceService` runs at startup and then on the same interval as the Concern service. It:
  - finds candidate Incidents with a set-based query: Open past the inactivity deadline, grants whose membership no longer matches, or past expiry. It does not load every row;
  - processes each candidate in its own serializable, group-locked transaction, normalizing the lifecycle once and revoking lost grants once;
  - **deletes the whole expired aggregate**: entries, tasks, coordinator changes, grants, and the Incident including its lineage;
  - deletes audit rows older than 365 days.

  It must be idempotent and safe under concurrent or repeated runs.
- **Correctness never depends on cleanup having run.** Request-time checks make expired Incidents unavailable in list, detail, and commands immediately.
- **Backup and restore.** This item builds no backup infrastructure.
  - The request-time guards plus startup reconciliation are the application-level restore invariant.
  - Proof that expired Incident data ages out of ordinary backups within 30 days, and that a restore does not reactivate expired data, remains a **predeployment operational dependency**, shared with Concern. `VER-WI-008-003` must record this dependency and must not claim backup compliance.

## API scope (private, minimal)

| Route | Purpose |
|---|---|
| `GET /api/incidents` | Caller-accessible, unexpired Incidents as **summary** items: ID, Family Group, subject display name, state, coordinator display name, created time, last activity, and expiry. No lineage. |
| `GET /api/incidents/{incidentId}` | Detail: subject, coordinator, current participants, state and lifecycle times, resolution, tasks, entries, conditional `OriginatingConcernId`, and `Timeline`. |
| `POST /api/family-groups/{familyGroupId}/incidents` | Create, directly or with `OriginatingConcernId`. **One create route; no separate "from Concern" route.** |
| `PUT /api/incidents/{incidentId}/participants` | Replace the non-coordinator participant set. |
| `POST /api/incidents/{incidentId}/coordinator` | Hand off to a participant profile ID. |
| `POST /api/incidents/{incidentId}/entries` | `Observation`, `Unknown`, `Summary`, `ContactAttempt`, `Correction`, or `Challenge`, as one validated command. |
| `POST /api/incidents/{incidentId}/tasks` | Create a task. |
| `POST /api/incidents/{incidentId}/tasks/{taskId}/complete` | Complete with a result. |
| `POST /api/incidents/{incidentId}/tasks/{taskId}/cancel` | Cancel with a reason. |
| `POST /api/incidents/{incidentId}/resolve` | Resolve. |

Exact DTO names and field limits are implementation choices. Mirror the Concern limits: reason and explanation up to 1000 characters, text up to 2000, source up to 500. Suggested: task description up to 500, target label up to 200, cancel reason up to 1000.

Every command maps to the matrix and has API or SQL tests. Responses contain only audience-safe fields: no private profile fields, no contact details, and no other-domain data.

There are no audit, notification, escalation, location, Check-In, Sharing, export, or reopen endpoints. The existing authorized Family directory endpoint remains the participant-selection source; do not add discovery.

## Persistence and migration

- **New domain folder** `src/FamilySafety.Domain/Incidents/`:
  - `SafetyIncident`;
  - `IncidentParticipantGrant`;
  - `IncidentCoordinatorChange`;
  - `IncidentEntry`;
  - `IncidentTask`;
  - `IncidentAuditEvent`;
  - enums: `IncidentState`, `IncidentFreezeReason`, `IncidentEntryKind`, `IncidentInformationCategory`, `IncidentContactMethod`, `IncidentContactOutcome`, `IncidentTaskStatus`.

  There is no separate contact-attempt table; it is an entry kind. There is no separate resolution table; resolution fields live on `SafetyIncident`, as on `Concern`.
- **EF mapping** in `FamilySafetyDbContext`:
  - `Incidents`, `IncidentParticipantGrants`, `IncidentCoordinatorChanges`, `IncidentEntries`, `IncidentTasks`, and `IncidentAuditEvents` tables;
  - row-version tokens on `SafetyIncident` and `IncidentTask`;
  - a unique filtered index for one effective grant per (Incident, profile);
  - indexes for list lookup by account grant, entry and task by Incident, candidate reconciliation, and audit expiry;
  - a unique denied-summary index on (actor, action, bucket);
  - restrict or no-cascade where needed so whole-aggregate deletion is explicit;
  - no foreign key from audit, and no foreign key for `OriginatingConcernId`.
- **One additive migration and an updated model snapshot**, for example `SafetyIncidentDomainAndAccessFoundation`. It creates the new tables only; **existing tables are unchanged**.
- SQL-gated tests must apply the migration to a real SQL Server database, including one that already contains Concern data. Do not generate the migration during planning.

## Concurrency

Use the existing pattern. Every command and every normalizing read:

- runs in a `Serializable` transaction;
- takes the Family Group row lock first;
- loads the Incident;
- revalidates the caller's grant against current membership, the effective lifecycle, and expiry;
- mutates;
- writes audit in the same transaction;
- commits.

The row-version tokens on the Incident and task are a second guard. No distributed locks, queues, or external services.

| Race | Required outcome |
|---|---|
| Handoff vs. a coordinator-only command by the old coordinator | Exactly one effective coordinator. The command after handoff is denied. |
| Coordinator membership loss vs. a coordinator command | The command fails if the loss is visible at commit. The Incident freezes at `EndedAt`. |
| Participant removal or membership loss vs. that participant's write | No write commits after the revocation commits. |
| Concurrent complete/cancel, or double complete, on one task | One terminal transition; the other is denied. |
| Resolve vs. due inactivity freeze | Effective state wins. A resolve after the deadline is denied, and a Resolved Incident never becomes Frozen. |
| Expiry vs. any request | Expired means unavailable, before or after cleanup. |
| Concurrent reconciliation runs | No duplicate `Frozen` or `AccessRevoked` events, and no partial aggregate deletion. |

## Expected implementation locations after authorization

| Area | Likely work |
|---|---|
| `src/FamilySafety.Domain/Incidents/` | Types and invariants listed above. |
| `src/FamilySafety.Application/Incidents/` | Contracts and DTOs, `IIncidentService` / `IncidentService` (request validation, current account), `IIncidentRepository`, `IncidentUnavailableException`, `IncidentValidationException`. |
| `src/FamilySafety.Application/Concerns/` + `src/FamilySafety.Infrastructure/Persistence/` | **Only** the narrow read-only Concern access query and its implementation. No change to existing Concern behavior. |
| `src/FamilySafety.Infrastructure/Persistence/EfIncidentRepository.cs`; `FamilySafetyDbContext.cs`; `DependencyInjection.cs`; `Persistence/Migrations/` | Persistence, authorization, normalization, reconciliation, migration, and snapshot. |
| `src/FamilySafety.Api/Incidents/IncidentEndpoints.cs`, `IncidentMaintenanceService.cs`; `Program.cs` | Routes, DI, and hosted-service registration. |
| `tests/FamilySafety.Foundation.Tests/Incident*Tests.cs` | Domain, API, and SQL-gated persistence, access, lifecycle, audit, retention, and concurrency tests. |
| `ai-context/planning/verification/VER-WI-008-003.md` | Per-AC evidence. |

**No `src/FamilySafety.Web` changes.** These are likely locations, not a mandated class decomposition.

## Acceptance checks (29)

1. **`AC-01 — Direct creation`**: An active adult member creates an Incident in one Family Group about an active adult in that group, including themself. The required reason is persisted as the first `Summary` entry with its provenance. The creator holds the coordinator grant, which captures membership ID and version. Inactive, pending, nonmember, other-group, or no-profile callers, and ineligible or other-group subjects, receive the unavailable result with no partial record.
2. **`AC-02 — Explicit participants`**: Creation accepts zero or more distinct eligible adults, up to 250, with captured membership ID and version. The subject is absent unless selected. Any invalid participant rejects the whole creation atomically.
3. **`AC-03 — No implicit authority`**: Subject status, Family membership alone, Administrator, relationship, Concern participation, lineage, and other-domain roles grant no list, detail, or command access.
4. **`AC-04 — Authorized list/detail`**: Only callers with a currently valid grant see the Incident in the list and in detail and timeline, including retained Frozen and Resolved Incidents. List items are summaries without lineage. The list omits expired Incidents and Incidents whose access the caller has lost.
5. **`AC-05 — Nondisclosing denial`**: Missing, unauthorized, expired, removed, membership-lost, rejoined, stale-grant, unselected-subject, and unauthorized nested entry or task IDs all produce the same unavailable result, for both reads and commands.
6. **`AC-06 — Create from Concern`**: With `OriginatingConcernId`, creation succeeds only for that Concern's creator with current Concern access, in the same Family Group, before Concern expiry. It stores the ID and allows a different subject. Concern participants, a departed or rejoined Concern creator, an expired Concern, an other-group Concern, and a missing Concern all receive the unavailable result. The Concern's state, last-mutation time, participants, entries, and Concern audit are unchanged. No Concern data or participants are copied.
7. **`AC-07 — Lineage visibility`**: Detail returns `OriginatingConcernId` only to a caller currently authorized for both records. For every other caller, and for everyone after Concern expiry, the response is indistinguishable from an Incident without lineage. Lineage checks write no Concern audit or denied summary and grant no access.
8. **`AC-08 — Participant management`**: Only the coordinator, while Open, replaces the non-coordinator set. Eligibility is revalidated at commit and the change is atomic. Removed participants lose access immediately. The coordinator's grant cannot be removed by any request. Participants are denied.
9. **`AC-09 — Coordinator handoff`**: The coordinator hands off, while Open, to a current valid participant, recorded as a change row plus audit. The new coordinator's powers apply immediately. The former coordinator keeps only participant rights, and the creator has no residual authority. Handoff to self, a non-participant, a removed participant, or a stale-grant holder is denied.
10. **`AC-10 — Coordinator loss`**: Coordinator membership loss while Open makes the Incident effectively `Frozen` (`CoordinatorEligibilityLost`) at `EndedAt`. Revocation is audited once. There is no promotion and no resolution, and remaining valid participants are read-only. An earlier inactivity freeze is preserved. Rejoin neither restores nor unfreezes.
11. **`AC-11 — Participant loss and rejoin`**: Membership loss or removal immediately denies reads and writes, and stale writes fail. Rejoin never restores access; only a new explicit coordinator selection while Open creates a new grant. Prior attribution remains.
12. **`AC-12 — Subject membership loss`**: The Incident and other grants are unchanged and the subject's identity remains. A subject who was a participant loses access. No new access is created.
13. **`AC-13 — Entries and provenance`**: Valid grant holders append `Observation` and `Unknown` entries, and only the coordinator appends `Summary`. Each entry persists contributor, recorded time, optional event time, one of the five categories (with Outdated stored as a marker that preserves the original category), and a source or explicit unknown source. `Confirmed` without a source is rejected. No numeric score exists.
14. **`AC-14 — Corrections and challenges`**: A contributor appends an explained, linked correction to their own entry. Another participant appends an explained, linked challenge. Correcting another's entry and challenging one's own are denied. Targets are unchanged, and no update or delete path exists.
15. **`AC-15 — Contact attempts`**: A valid grant holder records a contact attempt with: attempted-by (self or described), target label, method and outcome from the fixed sets, attempt time or explicit Unknown (exactly one), optional notes, and category and source. Invalid values are rejected. No Profile, Trusted Contact, or other-domain contact data is read or stored. `Reached` changes no state, and no outbound communication occurs.
16. **`AC-16 — Task creation`**: The coordinator creates a task assigned to any current valid grant holder, including themself. A participant creates a task only for themself. The assignee's grant is captured and validated at commit, and no edit or reassignment command exists.
17. **`AC-17 — Task completion`**: Only the assignee, holding the same valid grant, completes an `Open` task in an `Open` Incident. A result with category and source is required and is persisted as a linked `TaskResult` entry, with `Done` and `CompletedAt`. A second completion is denied.
18. **`AC-18 — Task cancellation`**: The coordinator cancels any `Open` task with a reason. A participant cancels only a task they created that is assigned to themself. A participant's attempt to cancel another participant's or a coordinator-assigned task is denied. `Done` and `Cancelled` are terminal.
19. **`AC-19 — Timeline`**: Detail includes a read-time timeline of creation, entries, task created/completed/cancelled, participant additions and removals, access ended (without a reason), coordinator handoffs, freeze, and resolution. It is ordered by event time when known, otherwise recorded time, with a deterministic tie-break. Each item keeps its category, and both times are exposed. No timeline table exists.
20. **`AC-20 — Resolution`**: Only the coordinator resolves an Open Incident. A summary, an outcome with category and source (not `Outdated`), and exactly one of contact-restored time or explicit Unknown are required. Optional assistance, follow-up, and lessons are allowed. `ResolvedAt` and the resolver are system-recorded. No verdict field exists, and participants are denied.
21. **`AC-21 — Terminal states`**: `Frozen` and `Resolved` deny every mutation, including entries, tasks, participants, handoff, and resolution. No reopen path exists, and a Resolved Incident never becomes Frozen.
22. **`AC-22 — Inactivity freeze`**: At 30 days after the last authorized mutation, the Incident is effectively `Frozen` (`Inactivity`) with the deadline as its transition time, recorded with exactly one system event. Reads, denials, and lineage checks do not extend the deadline. Every listed mutation type does.
23. **`AC-23 — Commit-time authorization and concurrency`**: Each race in the concurrency table produces its required outcome. No stale or revoked actor's mutation commits.
24. **`AC-24 — Protected-domain boundary`**: Incident code reads no profile contact fields, Event, Plan, Trip, location, Check-In, Sharing, Safety Circle, or Trusted Contact data. Responses contain only the approved Incident fields and ordinary display names.
25. **`AC-25 — Product expiry`**: `Frozen` and `Resolved` Incidents become unavailable at 365 days from their transition time, in list, detail, and commands, **before** cleanup. Cleanup then deletes the complete aggregate (entries, tasks, coordinator changes, grants, and the Incident including lineage) and leaves Concern data untouched.
26. **`AC-26 — Audit evidence and retention`**: Each required successful action stores one metadata-only event. Denied reads and writes, including nonexistent IDs and denied creates, aggregate to at most one `IncidentReadDenied` / `IncidentWriteDenied` summary per actor and UTC hour, with first and last attempt and count, and no Incident ID. Freeze and revocation events are written once. No Incident content is stored. Audit expires at 365 days per event, independently of product deletion. No audit endpoint exists.
27. **`AC-27 — Restore, overdue, and reconciliation`**: An overdue or restored database never lists or serves expired Incidents, and never accepts writes to an effectively frozen Incident before reconciliation. Reconciliation uses injected time, selects candidates rather than all rows, is idempotent, and is safe when run repeatedly or concurrently.
28. **`AC-28 — Migration`**: One additive migration and updated snapshot create only the Incident tables and indexes. The migration applies cleanly to a SQL Server database containing existing Family, Trip, and Concern data, and existing tables are unchanged.
29. **`AC-29 — No escalation and no regression`**: No time passage, Concern action, Concern freeze, or Incident action creates an Incident, a danger score, a notification, or dispatch. Existing Concern behavior is unchanged. The full foundation test suite passes with the SQL gate on.

## Test and verification strategy

- **Domain tests:**
  - state machine and effective freeze;
  - earlier-cause selection;
  - 30-day and 365-day boundaries, using `TimeProvider`;
  - entry validation per kind (categories, Outdated, source, links, contact-attempt fields);
  - task transitions;
  - resolution field rules;
  - timeline ordering and tie-break.
- **API tests:** each route's status mapping (`200` / `201` / `401` / `404` / validation) and the nondisclosing equivalence of responses.
- **SQL-gated tests** (`RUN_FAMILYSAFETY_SQL_TESTS=1`, real SQL Server, migration applied), covering at minimum the scenarios in the owner's task list:
  - **Creation:** eligible, ineligible, self, Administrator.
  - **Visibility:** coordinator, participant, unselected subject, nonparticipant.
  - **Participants:** management, participant denied, removal, leave and rejoin.
  - **Coordinator:** handoff, old coordinator loses powers, cannot remove coordinator, coordinator-loss freeze.
  - **Lineage:** stored, no audience or data transfer, visible only with both accesses, hidden after Concern expiry, no Concern side effects.
  - **Findings:** provenance, Unknown, append-only correction, additive challenge.
  - **Contact attempts:** fixed sets, no contact-detail pull.
  - **Tasks:** every create, complete, and cancel rule, including a participant denied cancelling another's task.
  - **Lifecycle:** inactivity, coordinator loss, terminal states, no reopen.
  - **Resolution:** coordinator only; required fields; known or Unknown contact time.
  - **Retention:** expiry before cleanup, list exclusion, physical deletion, 365-day clocks.
  - **Audit:** every successful action, denied aggregation (actor, category, and hour separation; varying IDs; concurrent increments; no ID), independent expiry, no content.
  - **Concurrency:** each row of the race table.
  - **Migration:** applies over existing Concern data.
- **Regression:** `dotnet build` of the solution, and the full foundation test project with the SQL gate on. That includes Family, Event, Plan, Trip, and **Concern** suites.
- **SQL gate evidence.** The SQL-gated tests return early, and therefore report as passed, when `RUN_FAMILYSAFETY_SQL_TESTS` is not `1`. A run without the gate **cannot** establish `PASS` for any SQL-sensitive AC (AC-01 through AC-29, except pure domain checks). `VER-WI-008-003` must record:
  - the exact command, with the environment variable set;
  - total, passed, failed, and skipped counts;
  - confirmation that the Incident SQL tests executed, for example by listing their names in the run output.
- `VER-WI-008-003` maps every AC and canonical anchor to evidence and a result. It labels independence (`SELF-VERIFIED` unless independently reviewed) and records the backup and restore predeployment dependency.
- No UI rendering or accessibility review is required in this item.

## Explicit exclusions

- **UI:** any Incident UI; any Angular route, navigation item, screen, form, timeline UI, or Start-an-Incident button (all `WI-008-004`).
- **Notifications and external action:** push, SMS, or email; emergency dispatch; law-enforcement, EMS, or fire contact; external integrations.
- **Automation:** automatic Incident creation or escalation; danger, risk, or severity scoring.
- **Deferred labels:** the `REQ-SAFE-008` descriptive labels.
- **Other domains:** location, maps, Check-Ins, Sharing Sessions, Trips, Events, Plans, private profile data; Safety Circle or Trusted Contact management or access.
- **Concern changes:** Concern behavior changes, Concern-stored Incident references, and copying or selective transfer of Concern information.
- **Generic infrastructure:** generic Safety, workflow, role, relationship, retention, audit, or event-sourcing frameworks; a membership event bus; an audit viewer or role-based audit access.
- **Retention and backup:** configurable retention or legal hold; backup infrastructure.
- **Open decisions:** `OD-001` and `OD-002` resolution.

## Stop conditions during future execution

Stop and return for a separate decision or scope if implementation requires any of these:

- a change to canonical Incident permissions (`REQ-PRIV-026`, `REQ-SAFE-016`) or Concern permissions or behavior (`REQ-PRIV-025`, `REQ-SAFE-015`), beyond the narrow read-only access query;
- a change to Family membership join, leave, or eligibility semantics, or a new membership-end path or event mechanism;
- any `OD-002` behavior; location, Check-In, Sharing, or Trip access;
- notification or external-communication behavior;
- global or generic authorization architecture;
- backup infrastructure;
- a change to existing tables' schema, or to unrelated domains.

Also stop if a baseline later than `CTX-1.26`, or a conflicting canonical source, appears. Do not infer a permission from a convenient existing API or UI state. Do not broaden scope silently.

## Implementation choices left to execution (non-policy)

Each of these is bounded by the guidance above and none changes product behavior:

- the coordinator-reference shape (no foreign-key cycle);
- the Concern access query's placement and name;
- exact DTO and route names and input limits;
- index names;
- reconciliation batch size.

Record each choice in `VER-WI-008-003`.

## Execution tasks

- [x] `TASK-001` — Reconfirm `CTX-1.26`, owner work-item approval, and execution authorization.
- [x] `TASK-002` — Implement Incident domain types, invariants, and domain tests.
- [x] `TASK-003` — Implement application contracts, validation, the service, and the read-only Concern access query.
- [x] `TASK-004` — Implement EF mapping, the repository (authorization, normalization, concurrency, audit), the additive migration, and the snapshot.
- [x] `TASK-005` — Implement API endpoints, `IncidentMaintenanceService`, and registration.
- [x] `TASK-006` — Add API and SQL-gated tests; run the build and full regression with the SQL gate on.
- [x] `TASK-007` — Write `VER-WI-008-003` with per-AC evidence, gate-on test counts, implementation choices, and the predeployment dependency.

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner, explicit pasted approval and authorization
- **Authorized at:** 2026-10-01
- **Context baseline authorized against:** `CTX-1.26`
- **Authorized scope:** The implementation scope defined in this story:
  - Incident domain, application services, and authorization;
  - participant grants, coordinator transfer, and membership-loss handling;
  - Concern lineage, including the narrow read-only Concern access query;
  - findings and provenance, contact attempts, tasks, and corrections and challenges;
  - lifecycle, retention, audit, denied-audit aggregation, and maintenance/cleanup;
  - persistence, the additive EF migration and model snapshot, and the private API;
  - SQL-backed verification and `VER-WI-008-003`.

  This explicitly includes the .NET, API, permission, schema, and migration changes those items require, within this story's bounds.
- **Not authorized:**
  - Incident UI, and creation or implementation of `WI-008-004`;
  - notification integrations;
  - location, Check-In, or Sharing expansion;
  - automatic emergency escalation;
  - `OD-002` resolution;
  - unrelated refactoring.
- **Accepted design decisions:** The owner accepted this story's design as written. That includes:
  - one server-side foundation story;
  - `MembershipId` + `AccessVersion` grants, which the coordinator also holds;
  - the coordinator transfer model and the coordinator-loss freeze;
  - no automatic subject access;
  - the non-authorizing `OriginatingConcernId` lineage;
  - a creation reason that requires category and source;
  - record-only contact attempts;
  - the task rules, including that a participant may cancel only a task they created that is assigned to themself;
  - a read-time timeline, where access-ended items do not disclose why Family eligibility ended;
  - append-only corrections and challenges;
  - `Open` / `Frozen` / `Resolved`;
  - 365-day product and audit retention;
  - the `DEC-069` denied aggregation;
  - SQL-backed final verification.

  Do not reopen these during implementation unless a genuine canonical contradiction is discovered. A contradiction is a stop condition.
- **Handback:** On completion, set the story to `VERIFYING / OWNER REVIEW` with `VER-WI-008-003` written. Do not mark `DONE / PASS` without the owner's closure decision.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| Story traceability and bounded planning | `CTX-1.26`; `REQ-PRIV-026`, `REQ-SAFE-016`, and amended anchors; PRP-029; `DEC-069`, `DEC-071`; WI-008-001 and VER; current Concern source and tests reviewed 2026-10-01 | `PASS` (planning) |
| Owner approval and execution authorization | Explicit project-owner approval and authorization, 2026-10-01, against `CTX-1.26` | `AUTHORIZED` |
| Application behavior and tests | `dotnet build FamilySafety.slnx`: 0 warnings, 0 errors. `RUN_FAMILYSAFETY_SQL_TESTS=1` full suite: 191 total, 191 passed, 0 failed, 0 skipped. That includes 47 new Incident tests, 22 of them SQL-gated, each executed against a real database. Per-AC evidence is in `VER-WI-008-003`. | `PASS` (self-verified) |
| Migration | `20261002013148_SafetyIncidentDomainAndAccessFoundation`. It creates only the six Incident tables and their indexes. It was applied over a database with existing Family, Trip, and Concern data, with schema and data unchanged, and passed a down/up round trip. | `PASS` |
| Backup aging and restore operations | Not in scope; no operational evidence | `PENDING PREDEPLOYMENT` |

- **Verification report:** `ai-context/planning/verification/VER-WI-008-003.md`
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `DONE / PASS` — `OWNER APPROVED` (2026-10-01). All 29 acceptance checks pass.

## Implementation record (2026-10-01)

- **Scope delivered:** TASK-001 to TASK-007 as written. No acceptance check was changed. No `src/FamilySafety.Web` file changed. `WI-008-004` was not created.
- **Concern code:** unchanged. One new read-only interface (`Application/Concerns/IConcernAccessQuery.cs`) and its implementation (`Infrastructure/Persistence/EfConcernAccessQuery.cs`) were added. All 13 existing Concern tests pass.
- **Non-policy implementation choices:** recorded in `VER-WI-008-003` § Implementation choices. They cover:
  - the coordinator reference as a plain column with no FK;
  - query names, DTO names, and input limits;
  - index names and the reconciliation batch size;
  - two narrower readings: participant self-cancel requires the current grant, and a participant set that includes the coordinator is denied.
- **Hardening found during verification:** the repository now detaches tracked Incident rows after taking the Family Group lock. This means authorization never uses stale tracked state. Display names are projected so that profile contact columns are never read.
- **Stop conditions:** none triggered, and no canonical contradiction found.
- **Source control:** nothing committed or pushed. The workspace has no `.git`.

## Owner review (2026-10-01)

- **Result:** `PASS` — `OWNER APPROVED`. The project owner accepted the implementation and its verification evidence as complete. The work item is `DONE / PASS`.
- **Accepted as recorded:**
  - explicit membership-bound grants;
  - the coordinator model, handoff, and coordinator-loss freeze;
  - no automatic subject access, and non-authorizing Concern lineage;
  - findings and provenance, record-only contact attempts, and tasks;
  - append-only corrections and challenges, and the read-time timeline;
  - `Open` / `Frozen` / `Resolved` with coordinator-only resolution;
  - 365-day product and audit retention, and `DEC-069` denied aggregation;
  - request-time expiry and set-based maintenance;
  - the additive migration, the private API, and SQL-backed tests.
- **Narrow readings accepted** as consistent with `DEC-071` and this story. No separate decision record is needed:
  1. A participant may cancel a self-created task only while holding the same Incident grant used when the task was created.
  2. A participant-management request that includes the coordinator in the ordinary participant list is rejected with the uniform unavailable result.
- **Evidence preserved:**
  - `dotnet build FamilySafety.slnx`: 0 warnings, 0 errors.
  - `RUN_FAMILYSAFETY_SQL_TESTS=1` full suite: 191 total, 191 passed, 0 failed, 0 skipped. That includes 47 new Incident tests, with all 22 Incident SQL tests executed, and all 13 Concern tests passing.
  - Four consecutive gated Incident runs passed, and no test databases were left behind.
  - Migration `20261002013148_SafetyIncidentDomainAndAccessFoundation` is additive. It was verified over existing Family, Trip, and Concern data, including an upgrade/down/re-up round trip.
- **Closeout:** tests were not rerun; application code was not changed.
- **Open, non-blocking, outside this item:**
  - backup-aging and restore evidence (predeployment);
  - Playwright shared-database isolation;
  - Concern repository stale-tracking risk;
  - Concern display-name profile loading;
  - Concern maintenance reconciliation efficiency.
- **Next:** `WI-008-004 — Incident UI and Start-an-Incident flow`, to be planned. It has not been created.
