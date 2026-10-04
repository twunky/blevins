# PRP-028 — Phase 8 Concern Access and Lifecycle Proposal

## Metadata

- **Proposal ID:** `PRP-028`
- **Status:** `APPROVED` (owner-approved in full under `DEC-068`; canonicalized in `CTX-1.25`)
- **Date:** 2026-09-30
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.25` (active; approved requirements are canonicalized)
- **Required context:** `ai-context/README.md`; `project-status.md`; `planning/roadmap.md`; `decision-log.md` (`DEC-066`, `OD-001` through `OD-004`); `core/product-contract.md` (`REQ-PROD-003/004/008/009/012`, `REQ-FAM-005`); `core/privacy-safety-contract.md` (`REQ-PRIV-001` through `011`, `019`, `024`, `REQ-SAFE-001` through `004`); `core/ux-contract.md` (`REQ-UX-001/004/005/006/010/011`); `workflows/safety.md` (`REQ-SAFE-005` through `014`); `workflows/sharing-checkins.md` (`REQ-SHARE-008/009`); `planning/proposals/PRP-003-technical-architecture.md` (authorization, provenance, audit direction); `references/ux/safety/safety-coordination.md` (reference boundary only).
- **Affected authority:** Proposed Phase 8 Concern access and lifecycle rules; owner approval and canonical update required before implementation.
- **Model tier:** `DEEP` — safety, privacy, permissions, history, and retention.

## Request and authority boundary

Define a minimum viable, manually raised Concern for one adult actor and one Family Group. This is a planning proposal, not an approved product rule, implementation work item, schema, or execution authorization. The owner authorized Phase 8 entry under `DEC-066`, not the choices below. No Safety implementation is active.

## Current evidence and context

- `REQ-SAFE-001` says a missed expectation, communication gap, or unavailable location is uncertainty, not confirmed danger. `REQ-SAFE-002` through `004` require confidence categories, provenance, unknowns, and meaningful correction history.
- `REQ-SAFE-005/006` define Concern as a lightweight review before or instead of an Incident. A Family member may raise one in the standard flow, but exact eligibility, audience, mutation rights, and lifecycle are not specified.
- `REQ-PROD-012` separates Concern from Safety Incident. `REQ-PRIV-008` forbids either from unlocking otherwise unauthorized information. Relationship, administrator, operator, Safety Circle, Event, and Trip status do not supply a Concern permission.
- `REQ-PRIV-009` forbids stale information appearing current and indefinite retention merely because data was once useful. `OD-004` concerns incident-history retention and remains open; no Concern-history duration is defined either.
- The approved architecture direction keeps private-data authorization server-side, separates participation from permission, and calls for auditable sensitive actions. It does not approve a Concern entity or persistence schema.
- The Safety UX reference is inspiration only. Its maps, escalation actions, location displays, export, sharing controls, navigation, buttons, and retention treatment are not requirements.

## Owner-approved Concern rules (DEC-067; completed by DEC-068)

The project owner accepted the following non-retention recommendation package on 2026-09-30 under `DEC-067`. The complete retention package was approved under `DEC-068`; together these decisions fully approve PRP-028 as a planning basis. They do not authorize implementation.

- Any active adult in the Family Group may manually raise a Concern about an active adult in that group. Self-Concern is allowed. A non-self Concern requires a distinct adult subject. Family Administrator status grants no additional authority.
- The subject receives no automatic access. The creator may explicitly include the subject under the same eligibility and access rules as another participant. If not selected, the subject has no access and denied reads reveal no Concern existence.
- The creator controls participant selection and changes. Explicitly selected, currently eligible adult participants may view authorized history and contribute observations, explicit unknowns, own-entry append-only corrections, and separate attributed challenges. They cannot overwrite meaningful entries, resolve, or change participants.
- Only the creator may resolve an Open Concern, with an attributed note/reason and resolution time. Resolution means coordination ended; it does not itself mean safe, in danger, verified, or emergency resolved.
- If the creator loses active Family membership while Open, revoke the creator's access immediately and freeze the Concern. Remaining eligible participants may read retained authorized history but may not contribute, change participants, or resolve. Do not transfer ownership or mark Resolved. Continued coordination requires a new Concern.
- If another participant loses eligibility, immediately deny future reads and writes; do not restore access on rejoin; preserve historical authorship for the remaining authorized audience subject to retention; reject stale writes.
- Concern access grants no access to protected profile, Event, Trip, location, Check-In, Sharing Session, or other records. Do not copy or link protected records. A contributor may manually report information they are independently authorized to know. Provenance must not disclose an underlying restricted source to other participants.
- Use canonical confidence categories `Confirmed`, `Reported`, `Assumption`, `Unknown`, and `Outdated`, without scores. Preserve contributor, source or explicitly unknown source, recorded time, event time when known, category, known/unknown state, and freshness where applicable. Outdated must not erase the original classification/history.
- Corrections are append-only and identify contributor, time, explanation, and the original entry. Contributors correct their own entries; others may challenge or add a separately attributed report. Do not silently overwrite or delete another contributor's meaningful history.
- Resolved Concerns accept no more contributions, are not reopened in v1, preserve the attributed resolution/reason, and remain readable only to the authorized audience during the approved retention period. Later concerns use a new record.
- The first Concern slice has no Incident transition, notifications, location/Check-In/Sharing dependency, Safety Circle or Trusted Contact access, or automated escalation/danger scoring. Future manual Concern-to-Incident behavior needs separate design and approval.

## Approved Concern retention policy (DEC-068)

- Resolved Concern product history is retained for 180 days from resolution. Open Concerns with no authorized mutation for 30 days automatically become Frozen; reads and denied attempts do not reset inactivity. Freeze is not resolution, a safety/danger finding, verification, Incident creation, or escalation. No pre-freeze acknowledgement is required.
- Frozen Concerns are read-only for currently eligible participants and retained for 180 days from freeze. No contribution, audience change, correction/challenge, or creator resolution is allowed; continuation requires a new Concern.
- Membership/access revocation remains immediate. Rejoining does not restore access; historical attribution remains during retention.
- Minimal audit metadata is retained 180 days per event for create, participant changes, contribution/correction/challenge, freeze, resolution, revocation, and denied read/write. No Concern content or private source details are copied. No user audit viewer or automatic Family-member/admin/operator audit access is provided; any future operational access policy requires separate approval.
- At expiry, Concern product content and history are removed from active product storage/access, including participant history, entries, unknowns, challenges, correction links, provenance, resolution, and freeze information. Audit expires on its independent per-event clock.
- Backup copies age out within 30 days after active-store deletion. Deployment documentation must identify the mechanism, and restore must not reactivate already-expired data. No legal-hold engine is required.
- These are application defaults, not legal/records advice. Qualified review is required before any future deployment with applicable legal obligations. `OD-004` remains open for Incident history.

## Bounded model rationale and remaining options

### Scope and lifecycle

An active adult member of one Family Group may manually raise a Concern about an active adult member of that same group, including themself. The creator records a short uncertainty-based reason without asserting danger. The Concern begins **Open**, may receive authorized entries and corrections, and may be explicitly **Resolved** by its creator with an outcome summary and time. Resolved means coordination on this Concern has ended; it does not verify that the subject was safe, contacted, or endangered unless an independently supported entry says so. There is no automatic creation, escalation, expiry-as-resolution, or danger classification. No reopening in the first slice: a later distinct Concern would need its own deliberate action and cannot silently reuse the prior audience or facts.

At creation, the creator explicitly chooses zero or more other *currently active adult members of that same Family Group* as Concern participants. The subject is not automatically a participant merely by being named; the creator may explicitly select the subject. The creator remains an authorized participant only while their membership remains active. The creator may change the participant list; every change revalidates eligibility and applies immediately to future access. Participant notification delivery is outside this slice; selected participants discover the Concern only through authorized in-app reads.

The subject reference may use only the same-group member identity already available under ordinary Family-directory rules. Creating a Concern copies or links no profile contact field, private note, location, Check-In, Sharing Session, Trip, Event, or other protected record. A contributor may manually report information only if independently authorized to know it, as their own attributed contribution. Provenance presented to participants must not expose restricted source details they cannot otherwise access. These limits do not define cross-domain aggregation.

### Approved actor/action matrix

The matrix records the approved first-slice access policy. Every read and write is rechecked server-side against the specific Concern audience and active membership. Subject identity, Safety Circle, relationship, administrator/operator role, and item participation do not add rights.

| Actor | Create | Discover/view and history | Contribute | Correct own entry | Correct another's entry | Change participants | Resolve | Reopen |
|---|---|---|---|---|---|---|---|---|
| Active adult A in group, as creator | ALLOW about active adult in same group or self | ALLOW while A remains active | ALLOW while Open | ALLOW by append-only correction while authorized | DENY overwrite; ALLOW separate attributed challenge/clarification | ALLOW with eligibility recheck | ALLOW with attributed note and time while Open | DENY in v1 |
| Explicitly selected active adult participant C (subject B may be selected) | ALLOW a separate Concern under same create rule | ALLOW this Concern while C remains eligible | ALLOW while Open | ALLOW append-only correction of C's entry | DENY overwrite; ALLOW separate attributed challenge/clarification | DENY | DENY | DENY |
| Subject B, not separately selected | ALLOW a separate Concern under same create rule | DENY this Concern | DENY | DENY | DENY | DENY | DENY | DENY |
| Other active member, including Family Administrator or Safety Circle member | ALLOW a separate Concern under same create rule | DENY this Concern | DENY | DENY | DENY | DENY | DENY | DENY |
| Inactive/former member, pending invitee, unrelated-group member, operator | DENY for this Family Concern | DENY | DENY | DENY | DENY | DENY | DENY | DENY |

The create permission follows the canonical Family-member Concern flow and the owner-approved active-adult/same-group rule. The owner-approved selected-person view and contribution rules are in force for this proposal. A denied list/detail/history request must not reveal that a Concern exists, its subject, participants, or entries. Client visibility is never authorization. Authoritative write checks occur at commit, including membership changes between display and submission.

### Information, provenance, uncertainty, and corrections

Each important entry should carry an attributed contributor, recorded time, the source as described by that contributor or explicitly unknown source, a distinct event/observation time when known, and one canonical category: **Confirmed, Reported, Assumption, Unknown, or Outdated**. The interface must distinguish recorded time from asserted event time and show when a time is unknown. **Confirmed** is reserved for directly verified information with an identified basis; a contributor's assertion or a missed expectation does not become Confirmed merely by entry. Time passing does not silently transform a Report into a verified danger finding. Outdated is a freshness concept and must not erase the original classification or history.

Unknown is an explicit recorded item or field, not a blank interpreted as a negative fact. Examples such as “no recent contact,” “expected arrival missed,” or “location unavailable” remain observations with source/time and uncertainty; none proves danger. The first slice contains no location read, map, or location-derived status.

Corrections append a new attributed, timed entry that points to the earlier entry and states what is corrected and why. The earlier entry remains in authorized history with its original confidence and source, and current views clearly identify the correction. A contributor may correct their own entry; another participant may append a separate challenge or additional report but cannot impersonate, edit, or delete the original contributor's record. This preserves `REQ-SAFE-004` without silent history mutation.

Normal Concern detail should disclose only the subject's ordinary same-group display identity, creator, selected participant identities, state, visible entries with their source/contributor/confidence/freshness, corrections, and resolution. It must omit profile-private fields, contact information, Safety Circle or Trusted Contact entries, location, Check-In and Sharing Session data, and unrelated Event/Plan/Trip data unless separately authorized under a later explicit rule. The user interface should use calm language and distinguish unknown, stale, reported, and confirmed information.

### Membership and access loss

Ending any actor's active membership immediately ends their Concern list/detail/history and mutation access. A later rejoin does not restore access automatically. Their prior entries remain attributed within the Concern's authorized history while the applicable retention policy permits; access loss must not rewrite the record. A selected participant who loses access cannot finish a stale in-flight contribution after revocation. Creator authority is **not** proposed to persist after membership loss (unlike the narrow Trip-specific exception in `REQ-PRIV-022`).

If the creator loses active membership while a Concern remains Open, freeze it. Remaining eligible participants may read authorized retained history during the approved 180-day period but may not contribute, change participants, or resolve. Do not transfer ownership or mark it Resolved. A new Concern is required to continue coordination.

### Resolution, retained history, and audit

Resolution is a deliberate creator action with an attributed note/reason and resolution time. It preserves entries, unknowns, corrections, and state change in authorized history; it does not delete or convert uncertainty into a confirmed outcome. Authorized participants may read resolved history only during the owner-approved finite retention period. Resolved Concerns accept no contributions and cannot be reopened in v1; a new concern creates a new record. Normal views must remove access immediately upon membership loss regardless of stored history.

Audit evidence records only minimal metadata for creation, participant changes, contribution/correction/challenge, freeze, resolution, access revocation, and denied read/write attempts, retained for 180 days per event. It excludes Concern content and restricted-source details. There is no user audit viewer and no automatic Family member, administrator, or operator access; future operational access requires separate approval. Allowed reads are not included in the required event set. Reopening is excluded.

`OD-004` remains open for **Incident** history. This proposal does not set its policy. Concern product history, Open/Frozen behavior, audit evidence, backup aging, and restore behavior are approved under `DEC-068` and canonicalized in `CTX-1.25`. No period is inferred from Shared Family Dates or any other feature.

### Concern versus Incident

A Concern stays a lightweight uncertainty review with selected participants, entries, and explicit resolution. It has no incident coordinator, incident task system, incident timeline workspace, or official emergency status. The first Concern slice has no Concern-to-Incident transition; any future transition requires separate design and approval. No missed arrival, lack of contact, stale location, Concern entry, or passage of time automatically creates an Incident or determines danger. This proposal does not design or implement escalation.

## Representative scenario review

| Scenario | Proposed result and privacy check |
|---|---|
| A raises a Concern about B | A must be an active adult in B's one Family Group. B's ordinary directory identity may be referenced; B's private data and Concern access do not follow automatically. |
| A selects C; C contributes “B's arrival was reported late” | C must be an eligible selected participant at read and commit. Entry is `Reported` with source, contributor, and times; not a danger finding. |
| C later corrects the arrival time | Append a linked correction; preserve the original and mark the current interpretation. Another participant cannot rewrite C's entry. |
| Contact status remains unknown | Record `Unknown` explicitly, with source/time context where possible. Absence of a Check-In or location is not proof of danger. |
| Earlier information becomes stale | Show observation and recorded times and `Outdated`/freshness treatment; do not present it as current or silently erase it. |
| C loses membership; later rejoins | Recheck at each operation; deny list/detail/history/write after loss. No automatic restoration on rejoin; retain C's attribution only under an approved history policy. |
| A loses membership while Open | Deny A immediately and freeze the Concern. Remaining eligible participants retain read-only history; no transfer, contribution, participant changes, or resolution. |
| A resolves without Incident | Require a sourced outcome summary; preserve authorized history subject to a finite approved policy. No automatic notification or Incident creation. |
| Unselected member, administrator, operator, or cross-group caller requests detail or submits an entry | Deny without disclosing Concern existence/content; do not accept stale or unauthorized writes. |

## Alternatives considered

### Option A — Selected active adults (approved)

- **Description:** Creator and explicitly selected active same-group adults can read; selected participants can contribute; creator resolves. Subject and group do not receive automatic access.
- **Benefits:** Smallest useful collaborative Concern, bounded disclosure, explicit audience, testable denied paths, no dependency on Safety Circle or notifications.
- **Tradeoffs:** Creator departure freezes coordination; a Concern is not visible to every family member by default.

### Option B — Group-wide Concern (not selected)

- **Description:** Every active adult in the Family Group can discover and read the Concern.
- **Benefits:** Broad awareness without participant selection.
- **Risks:** Discloses sensitive concern existence and reports widely, conflates group membership with Safety access, and conflicts with the least-disclosure posture unless separately and explicitly approved.

The owner approved **Option A** under `DEC-067`. Option B was not selected. Option A provides collaboration without using location, Check-Ins, Sharing Sessions, Safety Circle, Trusted Contacts, or notifications as prerequisites.

## Minimum likely domain/data concepts — not a schema

- A **Concern** identity scoped to one Family Group, subject adult, creator, state, creation and resolution metadata.
- Explicit **Concern audience/participant** identities, separate from Family membership and Safety Circle.
- Attributed **Concern entries** for observations, reports, explicit unknowns, contact attempts if approved for this slice, and corrections linked to earlier entries; each important entry needs source, contributor, confidence, and time/freshness.
- An explicit **resolution** record and append-only **activity/audit evidence** with controlled access and an approved finite retention rule.

These are conceptual needs only. No entity names, table structure, migration, API shape, or broader audit platform is selected. A first implementation can use the existing .NET application, private API, and SQL patterns once the rules are approved.

## Exclusions and open decisions

No automatic Concern creation or escalation; Incident workspace; coordinator/tasks; location collection, map, tracking, continuous or indefinite sharing (`OD-002` remains `OPEN`); Check-In execution; Sharing Session management; Safety Circle or Trusted Contact management or automatic access; push, SMS, email, or external notifications; emergency dispatch; law-enforcement contact; danger scoring; minor/guardian behavior; export; mockup-defined controls; production authentication/provider changes. `OD-001` remains `OPEN` and does not block this proposal. `OD-003` remains outside MVP. `OD-004` remains `OPEN`.

## Owner-approved Phase 8 exit criteria

The project owner approved this phase-gate direction on 2026-09-30 under `DEC-067`. Phase 8 exit requires, at an appropriate level:

1. Approved **and verified** Concern workflow.
2. Approved **and verified** Incident workflow.
3. Provenance and uncertainty preserved.
4. Explicit authorization boundaries.
5. Denied-access paths tested.
6. Correction and history behavior verified.
7. Resolution behavior verified.
8. Privacy boundaries preserved.
9. No automatic privacy override.
10. No automatic danger determination.
11. No automatic emergency dispatch.
12. End-to-end verification for implemented, approved workflows.
13. Relevant accessibility verification.

These criteria do not require a broad formal participant usability study unless separately approved. They define phase gates, not authorization for any implementation.

## Decision and authorization

- **Decision:** `APPROVED` in full under `DEC-067` and `DEC-068`.
- **Decision ID:** `DEC-068` completes the retention decisions and approves PRP-028; canonical context is active at `CTX-1.25`.
- **Approved by:** Project owner, 2026-09-30.
- **Execution authorized:** `NO`. No implementation story, code, schema, or migration is authorized by this proposal decision.

The retention packet below records the options considered before the owner's decision. Its recommendations and pending-choice language are historical and superseded by the approved policy above and `DEC-068`.

## Verification plan

Before approval, map the chosen rules to each cited canonical requirement and verify that no proposal grants profile, Safety Circle, location, Check-In, Sharing Session, Event, or Trip access by implication. After approval and context update, a bounded implementation item should test allowed and denied API reads/writes, cross-group isolation, member loss/rejoin, stale writes, unknown/stale/provenance display, append-only correction, resolution/history, and retention behavior. Include rendered and focused accessibility review. Record self-verification as such if independent review is unavailable.

## Historical owner-decision packet (superseded by DEC-068)

The following alternatives and questions preserve the packet as it appeared before the owner decision. They are historical; the approved answers are stated above and in `DEC-068`. Do not treat the old recommendation/options as current policy.

### Existing repository rules reviewed

- `REQ-PRIV-009` says stale information must not appear current and old sensitive information must not remain indefinitely merely because it was once useful. Its specific Incident-history policy remains open under `OD-004`.
- `REQ-SAFE-004` requires meaningful incident correction history to be preserved rather than silently erased. It does not set Concern duration.
- `REQ-PRIV-017/018` set a finite 30-calendar-day rule **only for Shared Family Dates**; their wording expressly does not define retention for other data, audit, or technical backups. That duration must not be copied into Concern policy by analogy.
- Trip Notes and Event Updates preserve some history while the parent record exists, but their canonical text explicitly does not establish a general retention duration or audit subsystem.
- The architecture proposal calls for auditable sensitive access/actions and refers Safety retention to `OD-004`; it does not set a Concern or audit duration.
- No repository legal/public-records retention requirement, general audit-evidence period, Concern deletion rule, or backup-aging maximum was found.

### Data classes and decisions required

| Class | Current/recommended treatment | Owner decision before implementation |
|---|---|---|
| **A. Active/Open Concern** | Keep available to its authorized audience while actively coordinated. Do not resolve or delete merely because time passed. To avoid indefinite sensitive data, choose a finite inactivity threshold that moves a stale Open Concern to Frozen; never label it Resolved. | Inactivity threshold; whether creator receives an in-product review action before freeze; start of retention clock after freeze. No push/email reminder is included. |
| **B. Frozen Concern** | Read-only history for remaining authorized participants. Freeze immediately on creator membership loss; do not transfer ownership. Also freeze after the owner-selected inactivity threshold if adopted. | Finite period from freeze until product-history deletion; whether participants retain read during that period (recommended yes, eligibility still enforced). |
| **C. Resolved Concern product history** | Read-only to authorized audience for a finite period measured from resolution. | Select one duration option below and confirm the trigger/deletion scope. |
| **D. Security/audit evidence** | Minimal metadata only: actor, Concern identifier, action, timestamp, authorization result; include creation, participant additions/removals, contributions/corrections (references only), resolution, access revocation, and denied sensitive access attempts. Do not duplicate entry text, source details, or private records. Audit access itself follows a restricted operational boundary. | Confirm event set, who may inspect it, finite retention option, and deletion/de-identification when its period expires. No broader audit platform is proposed. |
| **E. Database backup copies** | Expired/deleted Concern content must not remain indefinitely in backups. Product deletion may not erase every backup copy instantly. | Set and document a finite maximum backup aging window; define restore handling so already-expired deletions are reapplied before restored data is available. No backup duration is selected here. |
| **F. Legal/public-records retention** | No such product requirement appears in the reviewed repository. This is a private family application decision, not assumed to be a government records system. | No blocker to choosing an application default. If a future deployment creates applicable legal duties, qualified review is an external deployment concern before that deployment. It does not silently change the product default. |

### Resolved-history product policy options

The durations below are **product-policy options only**, not legal requirements or existing repository rules. The period starts when a Concern becomes Resolved. If the same period is selected for Frozen history, its start would be the freeze time.

| Option | Candidate duration | Rationale and tradeoff |
|---|---:|---|
| **A — Short** | 30 calendar days | Strong minimization and a brief near-term reference window. It may be too short for families to revisit a correction or outcome after the immediate situation. This is not Shared Family Dates' rule; the number is only a candidate here. |
| **B — Moderate (recommended)** | 180 calendar days | Retains recent coordination context for several months while placing a clear limit on sensitive history. It still retains reports about people for a meaningful period, so access and deletion must be explicit. |
| **C — Longer finite** | 2 years | Supports longer-term reference across seasonal or recurring family contexts, at the cost of substantially longer exposure and storage of sensitive reports. |

**Recommendation:** Option B, 180 days, as a balanced product default. This is a recommendation for the owner to accept or replace; no duration has been chosen or approved.

At period expiry, the product record, entries, correction links, participant association/history, and resolution note become unavailable in normal product access and are deleted from the active product record. Do not leave detached correction text or identifying participant history through a secondary path. Related audit metadata follows its own approved period and must not expose content. Backup copies age out under the separately stated finite backup window; restoration must not make expired Concern content available again.

### Open and Frozen lifecycle recommendation

Do not automatically resolve or delete an active Open Concern based only on age. Also do not let inactive Open or Frozen sensitive data remain indefinitely. Recommended simple rule: after a finite owner-selected period with no activity, automatically mark an Open Concern **Frozen / inactive** (read-only, not resolved); start its finite history-retention clock at freeze. Creator membership loss freezes immediately under the already approved rule. Frozen history remains visible only to still-eligible participants until expiry, then follows the same deletion outcome as resolved history. The inactivity threshold and Frozen retention period are owner choices; deletion never means danger determination or emergency closure.

### Audit-evidence retention options

Audit metadata should not copy Concern text and should not outlive product history without a clear security/accountability reason. Candidate options are **30 days** (shorter, minimizes actor-linked access logs but may leave little time to investigate delayed access concerns), **the same period selected for product history** (recommended; simple, keeps reference and content lifetimes aligned), or **365 days** (longer finite accountability window but retains actor-to-Concern activity after the Concern content is gone). Recommendation: same finite period as selected product history, with only the minimal metadata listed above. The owner must select the duration and audit readers. No regulatory requirement is claimed.

### Exact remaining owner choices

1. Select resolved and Frozen Concern history duration: Option A (30 days), Option B (180 days), Option C (2 years), or another finite product period.
2. Select the no-activity threshold that freezes an Open Concern; confirm whether an in-product review action is needed before it freezes. A stale Concern is never silently resolved.
3. Confirm whether Frozen history remains readable to still-eligible participants during its retention period (recommended yes).
4. Select audit evidence retention: shorter finite period, same period as product history (recommended), or longer finite period; confirm the minimal audit events and who may inspect them.
5. Set a finite maximum backup aging window and confirm restore behavior that prevents expired Concern data from reappearing.

At the time this packet was prepared, no option had yet been selected. `DEC-068` subsequently selected and approved the retention policy, completed PRP-028 approval, and activated `CTX-1.25` after verification. `OD-004` remains `OPEN` for Incident history and is unaffected. No implementation is authorized here.
