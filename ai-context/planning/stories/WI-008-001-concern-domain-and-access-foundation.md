---
Status: DONE
Authority: Project-owner approved and execution-authorized for WI-008-001 only; implementation and verification complete
Context-Baseline: CTX-1.25
Version: 1.1
---

# WI-008-001 — Concern Domain and Access Foundation

## Metadata

- **Work Item ID:** `WI-008-001` (Phase 8 uses the existing three-digit phase convention: `WI-007-...` for Phase 7 and `WI-075-...` for Phase 7.5).
- **Status:** `DONE / PASS`
- **Phase:** `8 — Safety Coordination`, `CURRENT / IN PROGRESS`
- **Context baseline:** `CTX-1.25`
- **Model tier:** `DEEP` — safety, privacy, authorization, history, and retention
- **Approved planning basis:** `PRP-028`, `DEC-067`, `DEC-068`, `DEC-069`.
- **Execution authorization:** `YES` (authorized 2026-09-30; resumed under `DEC-069` on 2026-10-01 and completed)

## Goal and story boundary

Establish one complete server-side Concern foundation for one Family Group: manual creation, an explicit adult audience, attributed entries and corrections/challenges, creator controls, Open/Frozen/Resolved behavior, immediate access loss, minimal audit evidence, expiry enforcement, persistence, and a private HTTP/JSON API. It must be testable without a Safety UI. This is a bounded Concern workflow, not the complete Phase 8 Safety experience.

**Recommended split:** two work items, by delivery boundary. This first item owns the domain, EF persistence **including its additive migration**, application authorization, API, minimal audit/retention mechanisms, and focused tests. A later separately proposed item may add a simple functional Concern UI against this API using shared UI patterns. Domain, authorization, API, audit, and lifecycle belong together here because a smaller server slice would expose sensitive Concern data before its access, history, and expiry rules could be verified together. The UI adds a distinct interaction and accessibility surface and is not needed to prove this server foundation. Neither item implements an Incident.

## Requirement traceability

| Canonical anchor | Applied in this item |
|---|---|
| `REQ-PROD-001`, `REQ-PROD-004`, `REQ-PROD-009`, `REQ-PROD-012` | Care without surveillance; uncertainty; no automatic danger/dispatch; Concern remains distinct from Incident. |
| `REQ-PRIV-005` through `REQ-PRIV-008`, `REQ-PRIV-011`, `REQ-PRIV-012` | Membership, relationship, admin, operator, and Concern status never bypass private-data authorization. |
| `REQ-PRIV-009`, `REQ-PRIV-025` | Freshness, audience, access loss, audit, freeze, expiry, backup/restore constraints. |
| `REQ-SAFE-001` through `REQ-SAFE-006`, `REQ-SAFE-015` | Uncertainty, provenance, unknowns, append-only corrections, manual Concern workflow and lifecycle. `REQ-SAFE-006` is consumed only at the manual Concern/review/unknown/resolution points; its reminder, Trusted Contact, Check-In, and Incident steps are outside this item. |

The `requirements-index.md` confirms these anchors are approved in `CTX-1.25`. `DEC-067` approved the access rules and `DEC-068` completed retention policy approval. Canonical `REQ-PRIV-025` and `REQ-SAFE-015` control where historical PRP-028 wording differs.

## Required context manifest

| Context | Why | When |
|---|---|---|
| `ai-context/README.md`; `ai-context/project-status.md` | Operating rules and authorization checkpoint | Entry |
| `ai-context/core/product-contract.md#req-prod-012`; `ai-context/core/privacy-safety-contract.md#req-priv-025` | Concern/Incident boundary and complete privacy policy | Before execution and verification |
| `ai-context/core/privacy-safety-contract.md#req-safe-001` through `#req-safe-004`; `ai-context/workflows/safety.md#req-safe-015` | Confidence, unknowns, correction, lifecycle | Before execution and verification |
| `ai-context/core/requirements-index.md`; `ai-context/planning/roadmap.md#phase-8--safety-coordination` | Traceability and phase exit criteria | Before execution |
| `ai-context/planning/proposals/PRP-028-phase-8-concern-access-and-lifecycle.md`; `ai-context/decision-log.md#dec-067`; `#dec-068` | Approved rationale and decision history; canonical rules take precedence | Before execution |
| `ai-context/planning/proposals/PRP-003-technical-architecture.md`; current Family/Trip application, API, EF, and tests | Existing internal API, server policy, SQL, and membership patterns | Before implementation design |
| `ai-context/templates/work-item.md`; `ai-context/templates/verification.md` | Record structure and evidence format | Before execution and verification |

## Context intentionally excluded

Incident implementation, Sharing/Check-In execution, Safety Circle and Trusted Contact management, Phase 7.5 visual reference files, bootstrap/archive records, and unrelated historical verifications do not determine this Concern slice. `OD-004` remains open for **Incident** history only; `OD-001` and `OD-002` remain open and do not block this story.

## Functional scope

- Create a Concern manually for exactly one Family Group. Creator and subject must each have an active adult membership in that group at commit. Self-Concern is valid. Capture a short, uncertainty-based creation reason. Subject identification uses only ordinary same-group directory identity; no private profile fields are copied.
- Creator explicitly selects zero or more distinct eligible active adult participants. Creator access is separate from this set. Subject is included only when explicitly selected. Selected participants may read authorized Concern history and contribute while Open.
- Support attributed observations/reports and explicit unknown entries. This does not read Check-Ins, contacts, location, or another domain; a separate structured contact-attempt workflow is outside this foundation.
- Support an append-only correction of one's own entry and a separate attributed challenge/report of another entry, each linked to its target and supplied with an explanation. Preserve the original and all prior links; do not overwrite or delete meaningful history. A challenge does not silently reclassify its target.
- Creator alone may add/remove participants and resolve while Open. Validate the entire proposed participant set at commit and apply it atomically. Removal immediately ends access; later re-addition is explicit. Resolution requires an attributed note/reason and timestamp and cannot be reopened in v1.
- Enforce Open, Frozen, and Resolved on every read and mutation. Open inactivity at 30 calendar days and creator eligibility loss freeze the Concern. Frozen/Resolved are read-only to still-authorized viewers until expiry. Continuing coordination uses a new Concern.
- Retain product history for 180 days from freeze or resolution, then deny all normal reads and remove the whole Concern product aggregate from active storage. Retain minimal audit metadata independently for 180 days per event. Include a small deterministic reconciliation/deletion mechanism in this item so expiry is enforceable, not merely a future schema intention.

## Proposed domain and persistence design

Use explicit types in the existing `Domain` / `Application` / `Infrastructure` / `Api` projects, without an Incident model, generic policy engine, workflow framework, or event sourcing:

| Concept | Minimum fields / role |
|---|---|
| `Concern` | ID, FamilyGroupId, creator Account/Profile ID plus the granting membership ID and `AccessVersion`, subject ProfileId, creation reason/time, `Open`/`Frozen`/`Resolved` state, last authorized mutation time, frozen time/reason, resolved time/actor/note, concurrency token. Expiry derives from the recorded frozen/resolved time. |
| `ConcernParticipant` | ConcernId, participant Account/Profile ID plus the granting membership ID and `AccessVersion`, selection/removal times or active grant state. Retain participant-change history within the Concern until expiry. One currently effective grant per participant. |
| `ConcernEntry` | ID, ConcernId, kind (`Observation`, `Unknown`, `Correction`, `Challenge`), contributor identity, recorded time, optional event time, text, source state (`Declared` or `Unknown`) and participant-safe source description where allowed, one of the five approved categories, optional freshness/outdated marker, optional target entry ID, correction/challenge explanation. Immutable after insert. An outdated marker or linked follow-up never rewrites an earlier category. |
| `ConcernAuditEvent` | Successful actions retain individual minimal events with ConcernId when appropriate. Denied access uses at most one summary per authenticated actor, broad read/write category, and UTC hour, storing no attempted Concern ID and only first/last attempt timestamps and count. No Concern content, source detail, private profile/location data, or user-facing read endpoint. |

The resolution and freeze metadata can live on `Concern`; separate tables are unnecessary unless an immediate EF constraint requires them. Participant changes can use grant rows or compact change records rather than a generic ACL. A correction or challenge is another `ConcernEntry` with a typed link; no separate correction entity or event-sourcing framework is needed. Audit rows remain independent of product-history deletion, without a cascading foreign key to `Concern`, so their own 180-day clocks work. Use relational constraints/indexes for group/Concern lookup, target relationships, active grants, audit expiry, and deterministic cleanup. **The authorized implementation of this work item must include the additive EF migration and updated model snapshot**, in line with existing persistence practice. None is created in this review.

The current `FamilyGroupMembership` already has `Id`, `IsActive`, and `AccessVersion`; `End()` increments `AccessVersion`, and Trip recipients already capture it. **Do not add another epoch framework.** Capture membership `Id` and `AccessVersion` for both the creator and each participant when granted. On every read/write require that same membership row to be active at the captured version. `Id` distinguishes a replacement membership row; `AccessVersion` distinguishes a later activation of the same row from uninterrupted membership. A newly explicit participant selection may capture the new version; it never silently refreshes an old grant. Creator grant never refreshes. Membership authorization is checked against current database state for every operation. A protected mutation and its audit row commit atomically; failed access must create no product mutation. Serialize or otherwise protect membership-end, audience-change, and Concern-write commits so an in-flight write cannot succeed after revocation commits.

The current membership model does not record when `End()` occurred. To make creator-loss freeze time and its 180-day retention clock accurate, this item may add a minimal membership-ended timestamp or an equivalent same-transaction timestamp supplied by the membership-end path; use the actual loss time, not an arbitrarily later sweep time. This is supporting lifecycle metadata, not a change to who may join or leave. If the current or future membership-end path cannot supply a reliable loss time without changing unrelated membership policy, stop execution and seek separate scope rather than guessing a freeze time.

## Lifecycle and time

| State | Authorized behavior | Transition |
|---|---|---|
| `Open` | Creator/selected eligible adults read and add entries. Creator may change participants or resolve. | Created manually. Freeze at `LastAuthorizedMutationAt + 30 calendar days` or creator eligibility loss, whichever occurs first; creator may explicitly resolve before freeze. |
| `Frozen` | Still-eligible creator/selected participants read only, except a departed creator has no access. | Attributed system freeze with timestamp and reason (`Inactivity` or `CreatorEligibilityLost`); no reopening or resolving. Product expires 180 days from freeze. |
| `Resolved` | Still-eligible creator/selected participants read only. | Creator resolution with note/reason and time; no reopening. Product expires 180 days from resolution. |

Creation, participant changes, contributions, corrections, challenges, and resolution are authorized mutations. Reads, denied attempts, audit writes, maintenance, and freeze itself do not reset inactivity. Store UTC instants and evaluate the approved calendar-day boundaries consistently through injected `TimeProvider`; do not infer a danger state from time. Persist the explicit lifecycle state and its transition timestamp/reason once; derive expiry from `FrozenAt` or `ResolvedAt` rather than storing a second mutable deadline. If a request arrives after a deadline but before physical reconciliation, use the **effective** frozen/expired state before authorization or response shaping. Materialize a due freeze exactly once with its system audit event. A resolved Concern never changes to Frozen. Inactivity freeze time is the 30-day deadline; creator-loss freeze time is the actual recorded loss time. If both have occurred before reconciliation, the earlier transition controls the reason, time, and expiry clock.

## Authorization matrix

All checks occur in the .NET application against the specific Concern, current membership and captured grant epoch. The UI is never the authority. A subject gains no right from subject status; Family Administrator, operator, relationship, Safety Circle, Event/Plan/Trip involvement, and Family context selection grant no right.

| Action | Active creator, while Open | Explicit active participant, while Open | Subject not selected / other member | Former/ineligible actor | Frozen / Resolved |
|---|---|---|---|---|---|
| Create in one group | Allowed for own new Concern about eligible adult | Allowed for own new Concern | Allowed for own new Concern if eligible | Denied | Separate new Concern only |
| List, detail, history | Allowed while creator grant valid | Allowed while participant grant valid | Omitted / nondisclosing denial | Denied | Read only for still-valid grants before expiry |
| Add observation or unknown | Allowed | Allowed | Denied | Denied | Denied |
| Correct own entry | Allowed, append only | Allowed, append only | Denied | Denied | Denied |
| Challenge another entry | Allowed, additive | Allowed, additive | Denied | Denied | Denied |
| Change participants | Allowed; revalidate all grants | Denied | Denied | Denied | Denied |
| Resolve with reason | Allowed | Denied | Denied | Denied | Denied |

For a known but unauthorized Concern ID, missing ID, expired ID, or unauthorized nested entry ID, return the same nondisclosing unavailable result (use the existing API `404` pattern) for both reads and commands. A caller without create eligibility or an eligible same-group subject gets an unavailable create result without exposing private subject data. List returns only currently authorized, unexpired Open/Frozen/Resolved Concerns and omits all others. Authentication failure may use the existing `401` behavior. Invalid content is reported only after caller authority and target visibility are established; response body and lookup path must not reveal subject, participants, entries, or restricted provenance. Revalidate at write acceptance, including stale requests after participant removal or membership loss.

## Membership loss and history

- Creator loss while Open makes effective state Frozen immediately, revokes creator access, records a single system freeze and access-revocation audit event at the actual loss time, and leaves eligible participants read-only. There is no transfer or automatic resolution. Later creator rejoin cannot restore the old creator grant or unfreeze. A write racing membership end must recheck the membership row/version at commit.
- Selected participant loss immediately denies future read and write; the participant grant is invalidated by current membership and its stored membership ID/version. Stale writes fail. Rejoin alone does not restore it. Only a new explicit creator selection while Open can grant access to a new membership ID/version.
- Participant removal has the same immediate access effect, and the participant-change/revocation audit events are retained. Existing contributions keep their original authorship for authorized viewers until aggregate expiry. Eligibility changes affect access, not attribution.
- Existing membership-end operations may not currently emit Concern-specific events. The first story must reconcile and persist required revocation audit evidence once, using current membership/version and stored grants, without adding a generic membership event bus. Request-time enforcement remains authoritative even before reconciliation.

## Provenance, uncertainty, corrections

Only `Confirmed`, `Reported`, `Assumption`, `Unknown`, and `Outdated` are accepted confidence categories. `Unknown` is explicit, not an empty value or negative finding. Preserve original category and source when later information is marked outdated or challenged. Separate `RecordedAt` from optional `EventAt`; show unknown event time as unknown. Freshness must be explicit where applicable and must not silently label an old report current. `Confirmed` requires the contributor to identify a direct verification basis; the system does not independently verify the claim. Do not store numeric confidence or danger scores.

Source is a contributor-declared, participant-safe description or explicitly unknown. Do not add foreign keys or automatic reads into protected domains. Validate and shape structured provenance to prevent restricted source IDs/URLs/details from being emitted as metadata to another participant; manually entered narrative remains attributed user content and is not a new authorization grant. Avoid copying protected domain fields into Concern API responses. Corrections link to an entry by the same contributor, carry explanation, and append a new entry. Challenges link to another entry, carry their own attribution and explanation, and do not alter the target. The authorized history retains original, correction, and challenge in chronological order with the relationships clear.

## Audit and retention mechanism

Minimal audit persistence is **in this story**, not deferred: creation, participant add/remove, contribution, correction/challenge, freeze, resolution, access revocation, and denied sensitive reads/writes require evidence now. Successful actions retain their individual minimal event. Denied reads/writes, including nonexistent IDs, use the `DEC-069` actor/category/UTC-hour summary with first/last timestamps and attempt count; no attempted Concern ID, request body, Concern text, or protected source detail is retained. The database enforces one summary per key and the summary expires on the existing 180-day audit clock. No audit viewer or role-based audit access is added.

Request-time effective-state and expiry checks are authoritative for every list/detail/read/write; they require no freeze endpoint. A small in-process periodic reconciliation path is needed for persisted state normalization, once-only system freeze/revocation audit events, and physical product/audit deletion when no user request arrives. Keep it idempotent, batched, concurrency-safe, and testable with `TimeProvider`; use a bounded hosted process if needed, without a queue or scheduler platform. An overdue, restarted, or restored database must not serve stale Open or expired content while reconciliation catches up. Document the production need for a running maintenance host and health/operations checks before deployment. On startup or after restore, apply request-time guards immediately and run reconciliation; this is the application-level restore invariant.

The data model and active-store deletion mechanism are required now because this item will persist sensitive Concern history. Deployment backup retention configuration and an operations document proving ordinary backups age out within 30 days of active deletion belong to a later predeployment operational work item; this story must record that dependency and must not claim backup compliance from application tests alone. No generic retention engine, legal-hold engine, or Incident (`OD-004`) retention work is included.

## API scope

Use private resource reads and explicit commands consistent with existing minimal API routes: list caller-accessible Concerns; create under one Family Group; detail/history; add observation/unknown; append correction; append challenge; replace selected participant set; resolve. Return only audience-safe fields. A small eligible-adult selection source may reuse the existing authorized Family directory endpoint; do not add Family discovery or private profile exposure. Exact route names and DTO layout are implementation choices, but every command must map to the authorization matrix and have API tests. No audit, Incident, escalation, location, Check-In, or notification endpoints.

Freeze, expiry, and maintenance are server-controlled; they require no caller command endpoint. Do not multiply endpoints just to represent entry variants if one explicit, validated entry command is clearer. Normal detail may include authorized history rather than a separate history route.

## UI decision

**Option A: domain + persistence + API + tests.** No functional Concern UI in this first item. API integration tests make the increment coherent and verifiable; a subsequent minimal UI item can cover create, choose subject/participants, list/detail, add/unknown, correct/challenge, and creator resolve using existing shell/forms and accessibility patterns. No visual polish or formal participant usability study is required for this foundation. Phase 8 end-to-end and accessibility exit criteria remain to be satisfied by later implemented UI/workflows; this item alone does not close Phase 8.

After this item passes, a later UI story may assume stable private API contracts, server-enforced audience/commands, nondisclosing unavailable results, persisted history/provenance, and server-computed lifecycle/expiry. It must still design accessible user interactions and cannot infer Incident, notification, or protected-domain access from this foundation.

## Expected implementation locations after authorization

| Project/file area | Likely work |
|---|---|
| `src/FamilySafety.Domain/Concerns/` | Explicit Concern, participant, entry, state/category types and invariants. |
| `src/FamilySafety.Application/Concerns/` | Requests/views, server authorization and workflow service/repository contracts. |
| `src/FamilySafety.Domain/Profiles/FamilyGroupMembership.cs` and its EF mapping, if needed | Minimal actual membership-end timestamp for creator-loss freeze/audit timing; preserve existing eligibility semantics. |
| `src/FamilySafety.Infrastructure/Persistence/FamilySafetyDbContext.cs`; `EfConcernRepository.cs`; `DependencyInjection.cs`; `Persistence/Migrations/` | EF mapping, constrained transactions, audit/expiry queries, additive migration and updated snapshot, maintenance persistence. |
| `src/FamilySafety.Api/Concerns/ConcernEndpoints.cs`; `Program.cs` | Private API routes, DI and bounded maintenance registration. |
| `tests/FamilySafety.Foundation.Tests/` | Domain, API, SQL-gated access/persistence, lifecycle, audit, and retention evidence. |
| `ai-context/planning/verification/VER-WI-008-001.md` | Final per-AC evidence and operational dependency record. |

No `FamilySafety.Web` changes are planned in this item. These are likely locations, not a mandated class decomposition. Reuse current account accessor, Family directory eligibility, EF SQL patterns, and `TimeProvider` where suitable.

## Acceptance checks (26)

1. **`AC-01 — Creation eligibility`**: Any active adult member can manually create in one Family Group about an active adult in that group, including themself; inactive/nonmember/pending/cross-group actors or subjects fail with no partial record.
2. **`AC-02 — Explicit audience`**: Creation accepts zero or more distinct eligible adults selected by the creator; subject is absent unless selected, creator access is separate, and invalid recipients reject atomically.
3. **`AC-03 — No implicit authority`**: Administrator, operator, relationship, Safety Circle, subject, and other-domain roles grant no Concern access or control.
4. **`AC-04 — Authorized list/detail`**: Only the current creator and selected active participants with matching membership ID/version see list/detail/history, including retained Frozen/Resolved history; the list omits expired Concerns and every Concern for which the caller has lost access.
5. **`AC-05 — Nondisclosing denial`**: Unauthorized, missing, expired, and unselected-subject detail/history/nested reads use the same unavailable response without Concern existence or content disclosure.
6. **`AC-06 — Authorized entries`**: Creator and selected participants can append attributed observation/report and explicit unknown entries while Open; entry metadata includes contributor, recorded time, optional event time, category, freshness, and safe source/unknown source.
7. **`AC-07 — Confidence semantics`**: Only the five approved categories are persisted; unknown and outdated are explicit, previous classification/history remains visible, and missing/stale information never creates a danger finding.
8. **`AC-08 — Own correction`**: An authorized contributor can append an explained, linked correction to their own entry; original entry and provenance remain unchanged.
9. **`AC-09 — Additive challenge`**: An authorized participant can append an attributed challenge/report to another entry; cannot overwrite or delete that entry.
10. **`AC-10 — History relationships`**: Authorized reads show originals, corrections, challenges, contributors, timestamps, and links without silent replacement.
11. **`AC-11 — Creator participant control`**: Creator alone changes the whole selected set while Open, with commit-time eligibility recheck, atomic update, immediate access effects, and audit evidence.
12. **`AC-12 — Participant restrictions`**: Participants cannot change participants, resolve, correct another contributor's entry, or mutate history in place.
13. **`AC-13 — Creator resolution`**: Creator alone resolves Open with attributed nonempty note/reason and time; no safety/danger/verification assertion is inferred.
14. **`AC-14 — Terminal states`**: Frozen and Resolved deny contribution, correction, challenge, participant changes, and resolution; Resolved cannot reopen.
15. **`AC-15 — Inactivity freeze`**: At 30 calendar days since last authorized mutation, effective Open becomes Frozen with the deadline as its persisted transition time and exactly one attributed system event; reads/denials do not extend the deadline.
16. **`AC-16 — Creator loss`**: Creator membership ID/version loss while Open immediately revokes creator and freezes at the recorded loss time; eligible participants retain read-only access; no transfer or automatic resolution. If inactivity already froze it earlier, preserve that earlier transition.
17. **`AC-17 — Participant loss`**: Membership/access epoch loss or participant removal immediately denies future reads and writes; stale writes fail; prior attribution remains for authorized viewers.
18. **`AC-18 — Rejoin`**: Rejoining does not restore creator or participant grants; only a fresh explicit selection while Open can grant a participant new access.
19. **`AC-19 — Current authorization at commit`**: Concurrent/stale membership or audience changes cannot let a formerly authorized mutation commit after revocation.
20. **`AC-20 — Protected-domain boundary`**: Concern APIs have no protected-domain links/reads or private profile fields, and provenance responses cannot disclose restricted structured source detail.
21. **`AC-21 — Product expiry`**: Frozen/Resolved histories become unavailable at 180 days from their recorded transition, even before cleanup; maintenance deletes the complete product aggregate from active storage, including entries, relationships, participants, provenance, resolution/freeze details.
22. **`AC-22 — Audit evidence`**: Each required successful action keeps its individual metadata-only event. Denied sensitive read/write attempts, including nonexistent IDs, aggregate into at most one summary per authenticated actor, broad read/write category, and UTC hour, containing first/last attempt time and count but no attempted Concern ID. Freeze/revocation events occur once; no Concern text or protected source fields are persisted and no viewer endpoint exists.
23. **`AC-23 — Audit expiry`**: Audit rows expire independently 180 days per event; product deletion neither extends nor prematurely removes unexpired audit evidence.
24. **`AC-24 — Restore/overdue guard`**: An overdue or restored database never lists or serves expired history, presents an effectively Frozen Concern as writable, or accepts its write before reconciliation completes.
25. **`AC-25 — Deterministic lifecycle`**: Time-dependent behavior uses injected time; persisted state/reason/time agree with effective state after idempotent reconciliation, including when loss and inactivity overlap. Repeated/concurrent sweeps do not duplicate freeze/revocation audit events.
26. **`AC-26 — No escalation`**: No Incident record/transition, auto-creation, danger score, notification, dispatch, or protected-domain access is caused by a Concern action or passage of time.

## Test and verification strategy

Use focused domain tests for state, append-only relationships, and injected-time boundaries; API integration tests for each authorization-matrix action and nondisclosing responses; SQL-gated tests for persisted membership ID/version grants, transaction/concurrency, audit, expiry, and membership-loss behavior. Cover active/inactive/nonmember/cross-group creation, eligible/ineligible/self subject, zero participants, selected participant and unselected subject visibility, admin without override, creator add/remove, stale write, rejoin, own correction, other-entry overwrite denial, additive challenge, Unknown and provenance persistence, creator/participant resolution rights, Frozen/Resolved read-only behavior, exact 30/180-day boundaries, overlapping freeze causes, expired reads before cleanup, physical cleanup, independent audit clocks, denied read/write hourly aggregation (including actor/category/hour separation, varying IDs, content minimization, retention, and concurrent increments), repeated/concurrent reconciliation, and no Incident/danger side effect. Run the .NET build and relevant existing Family/Event/Plan/Trip regressions during authorized execution. `VER-WI-008-001` must map every acceptance check and canonical anchor to evidence/result, state whether verification was independent or self-verified, show that SQL-gated tests actually ran rather than skipped, and name backup-aging/restore operations documentation as an unverified predeployment dependency. No UI rendering/accessibility review is required in this first item.

## Explicit exclusions

Safety Incident and Concern-to-Incident escalation; location, maps, Check-Ins, Sharing Sessions, Safety Circle or Trusted Contact management; notifications, SMS/email, emergency dispatch, law-enforcement integration; automatic danger determination, escalation, Concern creation, or AI scoring; mobile navigation or broad UX redesign; Event/Plan/Trip/private-profile data joins; generic workflow/policy/event-sourcing/retention systems; audit UI or role-based audit access; configurable retention or legal hold; backup infrastructure; `OD-001`, `OD-002`, and `OD-004` resolution; full Phase 8 UI/Incident experience.

## Implementation record and remaining deployment risks

- Route/DTO names, input limits, EF mappings, SQL locking/concurrency, membership-end timestamp, sweep interval, and source-description validation were selected and implemented in the application and migration. See `VER-WI-008-001.md` and the Concern source.
- Membership eligibility and join/leave policy remain unchanged. `FamilyGroupMembership.EndedAt` provides the actual end instant for Concern freeze/revocation timing.
- A periodic in-process sweep depends on a running application. Deployment must provide a reliable maintenance host and backup/restore operations evidence before production use. This is a deployment gate, not a blocker to creating this planning story.
- Manual participant text can contain information a contributor chose to report. The application must not itself fetch or expose protected records or restricted structured provenance, and must make attribution clear.

**Execution clarification:** The project owner supplied a narrowly scoped denied-audit-volume decision on 2026-10-01, recorded as `DEC-069`. It authorizes only Concern-specific hourly aggregation of denied attempts. This changes neither product-visible authorization nor 180-day retention and does not authorize rate limiting or broader security monitoring. The implementation uses a database uniqueness constraint and SQL Server update/key-range locking for the bucket row.

## Stop conditions during future execution

Stop and return for separate scope/decision if implementation requires a change to canonical Concern permissions; Incident behavior; location, Check-In, or Sharing rules; membership eligibility/join/leave semantics beyond the narrow timing metadata above; global authorization architecture; backup infrastructure; or unrelated domain behavior. A new baseline or conflicting canonical source also requires review before continuing. Do not infer permission from a convenient existing API or UI state.

## Execution tasks

- [x] `TASK-001` — Confirm baseline, owner work-item approval, and separate execution authorization.
- [x] `TASK-002` — Implement explicit Concern domain, server authorization, and API contracts.
- [x] `TASK-003` — Add EF persistence, additive migration/model snapshot, membership safeguards/timing, and concurrency protection.
- [x] `TASK-004` — Implement minimal metadata audit and deterministic freeze/expiry/revocation reconciliation.
- [x] `TASK-005` — Add focused domain, API, and SQL-gated integration tests; run required regressions/build.
- [x] `TASK-006` — Write `VER-WI-008-001` with per-AC evidence and operational dependency.

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner, explicit pasted authorization
- **Authorized at:** 2026-09-30
- **Authorized scope:** WI-008-001 server-side Concern foundation only; no UI or follow-on story.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| Story traceability and bounded planning | `CTX-1.25`, canonical anchors, PRP-028, DEC-067/068/069, roadmap, architecture, and membership implementation reviewed | `PASS` |
| Application behavior/tests | Domain, repository, API, migration, audit aggregation, and focused tests implemented; full SQL-gated suite passed | `PASS` |
| Implementation checkpoint | Server foundation and all acceptance checks complete; operational backup/restore evidence remains a predeployment dependency | `DONE` |

- **Verification report:** [`ai-context/planning/verification/VER-WI-008-001.md`](../verification/VER-WI-008-001.md)
- **Independence:** Self-verified; no independent review claimed
- **Outcome:** `DONE / PASS`
