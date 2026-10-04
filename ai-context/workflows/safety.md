---
Status: APPROVED
Authority: CANONICAL WORKFLOW — CTX-1.26
Context-Baseline: CTX-1.26
Version: 1.2
Source: ../bootstrap/product-ux-source.md; DEC-068; DEC-071
Constrained-By:
  - ../core/product-contract.md
  - ../core/ux-contract.md
  - ../core/privacy-safety-contract.md
---

# Concerns and Safety Incidents Workflow

This workflow supports family coordination. It does not define official emergency classifications and does not replace appropriate outside assistance.

## Concern

### REQ-SAFE-005

A Concern is a lightweight safety state used before or instead of a coordinated Safety Incident. It may arise from a missed expected arrival, missed expected check-in, or unexpected communication gap and exists so the family can review what is actually known before escalating unnecessarily.

### REQ-SAFE-006

The standard Concern flow is:

`Expectation missed → Personal reminder if appropriate → Trusted-contact awareness if configured → Family member raises Concern → Review authorized information → Record contact attempts → Identify unknowns → Resolve Concern`

A Concern may instead escalate to a coordinated Safety Incident. The review is constrained by `REQ-PRIV-008` and cannot unlock unauthorized information.

### REQ-SAFE-015

For the first Concern slice, an active adult manually raises a Concern about an active adult in one Family Group, including themself. The creator explicitly selects eligible adult participants; the subject has no automatic access. Selected participants may view authorized history and add attributed observations, unknowns, own-entry corrections, or separate challenges. The creator alone changes the participant list and resolves the Concern. Resolution records an attributed note/reason and time, ends coordination, and does not establish safety, danger, or verified facts. A Resolved Concern accepts no further contributions and cannot be reopened; a later concern uses a new record.

If the creator loses active membership while Open, the Concern becomes Frozen: the creator loses access, eligible participants retain read-only history, no ownership transfers, and the Concern is not Resolved. An Open Concern with no authorized mutation for 30 calendar days also becomes Frozen through an attributed system lifecycle action. Reads and denied attempts do not reset inactivity. Frozen Concerns do not accept contributions, participant changes, correction/challenges, or resolution. No automatic danger determination, Incident creation, external escalation, or notification occurs. Continue coordination through a new Concern.

Entry confidence, source, contributor, time/freshness, unknowns, and append-only correction history follow `REQ-SAFE-002` through `REQ-SAFE-004`. Concern permissions, protected-information boundaries, 180-day Resolved/Frozen product-history retention, 180-day audit retention, 30-day backup aging, and restore invariants are governed by `REQ-PRIV-025`. A Concern creator may deliberately start a separate Safety Incident from a Concern under `REQ-SAFE-016`. Doing so does not change the Concern's state, audience, history, or inactivity clock, and no Concern information or access transfers. The Concern rules above are otherwise unchanged.

## Safety Incident

### REQ-SAFE-007

A Safety Incident is a structured family coordination workspace. It may include the person involved, incident status, coordinator, participants, time the concern began, current summary, last confirmed contact, authorized last shared location, expected destination and arrival, trip information, travel companions, relevant contacts, contact attempts, findings, tasks, timeline, notes, and resolution. Attachments are deferred unless later approved.

Incident participation, coordination, or subject status does not itself make any of these items available. Items that depend on other domains appear only when they are independently authorized under their own rules and implemented by separately approved work. These include authorized last shared location, trip information, travel companions, expected destination and arrival taken from a Trip, and relevant contacts taken from profiles or Trusted Contacts. The MVP Incident defined in `REQ-SAFE-016` does not depend on those features. Participants may record such information only as their own attributed entries under `REQ-PRIV-026`.

### REQ-SAFE-008

Possible family coordination states are Concern Raised, Reviewing Information, Coordinating Response, Escalated Externally, Contact Re-established, Resolved, and Closed / Archived. These states must not be presented as official emergency classifications.

For the MVP, the approved Incident lifecycle is `Open`, `Frozen`, and `Resolved`, defined in `REQ-SAFE-016`. The labels above are deferred, non-MVP descriptive concepts and are not implemented in v1. They are represented in the MVP as follows:

| Label | MVP representation |
|---|---|
| Concern Raised | The separate Concern record |
| Reviewing Information, Coordinating Response | `Open` |
| Contact Re-established | Contact-attempt outcomes and the resolution |
| Escalated Externally | An attributed finding or task result about a family member's own outside contact |
| Resolved | `Resolved` |
| Closed / Archived | `Frozen` and retained history |

Any future descriptive label requires separate approval. It must not change access or lifecycle and must not be presented as an official emergency classification.

### REQ-SAFE-009

A Safety Incident may identify a coordinator and participants to organize the family response. Their participation does not expand their authorization to private information.

## Information and activity

### REQ-SAFE-010

Findings and other important incident information must use the confidence and provenance rules in `REQ-SAFE-002` through `REQ-SAFE-004`.

### REQ-SAFE-011

The workflow must support recording contact attempts and their known results so family members can see what has already been tried.

### REQ-SAFE-012

Incident tasks may include contacting the person, a travel companion, accommodation, destination family, or other relevant contact; verifying destinations or travel plans; gathering itinerary or relevant information; and coordinating appropriate outside assistance.

Tasks must show owner, status, result, and completion time so the family can reduce duplicate effort.

### REQ-SAFE-013

The incident timeline must preserve meaningful chronological activity, including confirmed contact, expected arrival, contact attempts, reported findings, assigned tasks, corrected information, incident state changes, and resolution. It is the family's authoritative chronological reconstruction of recorded activity, while each item retains its confidence classification.

## Resolution

### REQ-SAFE-014

Resolution must capture when contact was restored or resolution occurred, the outcome, a summary, assistance provided if applicable, unresolved follow-up, important corrections, and relevant lessons where appropriate.

The incident must transition to resolution rather than simply disappearing.

## MVP Safety Incident workflow

### REQ-SAFE-016

For the first Safety Incident slice:

**Concern and Incident.** A Concern coordinates review of uncertainty. A Safety Incident is deliberately started family safety coordination. Neither one proves danger or safety. No Incident is created, and no Concern moves toward one, because of:

- elapsed time;
- missed contact or a missed arrival;
- a missed or failed Check-In;
- stale information or unavailable location;
- Concern age or Concern freeze;
- the content or category of any entry.

**Creation.**

- Any active adult member of one Family Group may manually create an Incident about exactly one active adult subject in that same group. Self-Incident is allowed.
- Family Administrator status adds no Incident authority.
- An Incident may be created directly, or started deliberately from a Concern.
- Creation records a required short reason, in uncertainty-based language, as the first coordinator summary.

**Starting from a Concern.**

- The Concern creator may use a deliberate **Start an Incident** action when both of these are true:
  - they currently have access to that Concern under `REQ-PRIV-025`; and
  - they are eligible to create an Incident.
- Only the subject is prefilled, and it may be changed before submission.
- Concern entries, corrections, challenges, participants, sources, history, and private data are not copied. Concern participants are not preselected.
- The Concern's state, audience, history, and inactivity clock are unchanged.
- The new Incident stores the originating Concern ID only as the non-authorizing lineage reference defined in `REQ-PRIV-026`.
- Any future selective information transfer requires separately approved scope.

**Coordinator.**

- An Incident has exactly one coordinator. The creator is the initial coordinator.
- While the Incident is Open, the coordinator may explicitly hand off coordination to a current, eligible Incident participant. The handoff takes effect immediately.
- After a handoff, the former coordinator continues as an ordinary participant while their grant remains valid.
- The creator retains no special authority merely because they created the Incident.
- No co-coordinator or other role exists. The coordinator cannot be removed from the Incident and must hand off before leaving coordination.
- **Coordinator loss.** If the current coordinator loses eligibility while the Incident is Open:
  - coordinator access is revoked immediately;
  - the Incident becomes Frozen at the recorded loss time;
  - no participant is promoted, and the Incident is not marked Resolved;
  - remaining eligible participants may read retained history;
  - no further contributions, tasks, participant changes, or resolution occur in that Incident.

  To continue coordination, an eligible adult creates a new Incident.
- **Coordinator unavailable.** If the coordinator is unavailable but remains eligible, authority does not transfer automatically. Participants may continue the actions already allowed to participants, and the inactivity rule still applies.

**Lifecycle.** The MVP states are `Open`, `Frozen`, and `Resolved`.

- `Open` is active coordination.
- `Frozen` is a read-only coordination record.
- `Resolved` means coordination was ended explicitly by the coordinator.
- **Inactivity freeze.** An Open Incident with no authorized Incident mutation for 30 calendar days becomes Frozen through an attributed system lifecycle action.
  - These count as activity: creation, participant changes, coordinator handoff, entries, corrections, challenges, task creation/completion/cancellation, and resolution.
  - Reads and denied attempts do not reset inactivity.
- `Frozen` and `Resolved` are terminal and cannot be reopened in v1. A Resolved Incident never becomes Frozen.
- Freezing does not mean safe, dangerous, resolved, or verified. It does not notify, escalate, or create anything.

**Entries and provenance.**

- Entries are:
  - observations (findings);
  - explicit Unknowns;
  - coordinator summaries (coordinator only; append-only; the latest is shown as current);
  - contact attempts;
  - task results;
  - corrections;
  - challenges.
- Each entry preserves:
  - contributor;
  - source, or an explicitly unknown source;
  - recorded time;
  - event time when known, otherwise an explicit Unknown;
  - one category: `Confirmed`, `Reported`, `Assumption`, `Unknown`, or `Outdated`;
  - known/unknown status and freshness;
  - links to corrections and challenges.
- Numerical confidence, danger, or risk scores are prohibited.
- Time passing never changes a category. `Outdated` is applied by an appended attributed entry that preserves the original classification.
- A contributor corrects their own entry by appending an attributed correction that gives an explanation and references the original. Others may add attributed challenges or reports.
- Meaningful history is never silently replaced or deleted.

**Contact attempts.** A contact attempt is a record only. It records:

- the actor: the contributor, or another person named by the contributor;
- a text target label;
- a method: Call, Text message, Email, In-app message, In person, or Other;
- an outcome: Reached, No answer, Left message, Sent — no reply yet, Other, or Unknown;
- the attempt time, or Unknown;
- optional notes;
- its category and source.

A Reached outcome does not resolve the Incident or change its state. The application places no call and sends no SMS, email, or other message.

**Tasks.** A task has:

- an immutable description;
- one assignee: the coordinator or a current participant, eligible at assignment;
- a status of `Open`, `Done`, or `Cancelled`;
- created-by and created time;
- completion time, or cancellation time, actor, and reason, where applicable.

Rules:

- The coordinator may create tasks, assign them, and cancel any task.
- A participant may create a task assigned to themself and may cancel that self-created task with a reason.
- Only the assignee may complete a task. Completion requires an attributed result entry with a category and source.
- Tasks cannot be reassigned in v1. To change an assignment, cancel the task and create a new one.
- Status moves only forward.
- If an assignee loses access, the task remains and the coordinator may cancel it.
- When an Incident becomes Frozen or Resolved, its Open tasks become read-only and are shown as not completed.

This is not a general task-management capability.

**Timeline.** The timeline is a read-time chronological projection of the Incident's explicit records. It includes:

- creation, summaries, findings, and Unknowns;
- contact attempts;
- tasks created, completed, and cancelled;
- participant changes and coordinator handoffs;
- corrections and challenges;
- freeze and resolution.

Items are ordered by event time when known, otherwise by recorded time. Both times are shown, and each item keeps its classification. The timeline is not a separate store, and no event sourcing is required.

**Resolution.**

- Only the coordinator may resolve, and only while the Incident is Open.
- Resolution requires:
  - an attributed resolution summary;
  - an outcome statement with its category (`Unknown` is allowed);
  - the time contact was restored or resolution occurred, or an explicit Unknown;
  - the resolver and resolution timestamp, recorded by the system.
- Assistance provided, unresolved follow-up, and lessons are optional. Important corrections are presented from the Incident history.
- No preset verdict such as Safe, Dangerous, or Emergency resolved is used.
- Resolution means Incident coordination ended. It does not mean confirmed safe, danger ended, emergency resolved, or facts verified.
- A Resolved Incident accepts no contributions and cannot be reopened. A later situation uses a new Incident.

**Subject membership loss.** If the subject later loses Family membership:

- the Incident remains intact, and the subject's historical identity remains part of it;
- other authorized participants' access is not removed solely because the subject left;
- no new access is created.

If the subject was a participant, their own grant becomes invalid under the normal membership rule.

**Exclusions.** This slice includes none of the following:

- notifications, or automatic SMS or email escalation;
- emergency dispatch, or law-enforcement, EMS, or fire notification;
- automatic Incident creation, automated danger determination, risk scoring, or severity.

Future notifications require separately approved scope.

Incident access, the lineage reference, the protected-information boundary, retention, audit, backup aging, and restore behavior are governed by `REQ-PRIV-026`.

## Decision reference

`OD-004` (Incident-history retention) is resolved by `DEC-071`; see `REQ-PRIV-026` and `../decision-log.md#resolved-product-decisions`.
