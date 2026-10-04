---
Status: APPROVED
Authority: CANONICAL
Context-Baseline: CTX-1.27
Version: 1.20
Source: ../bootstrap/product-ux-source.md; DEC-016; DEC-018; DEC-028; DEC-029; DEC-030; DEC-031; DEC-034; DEC-038; DEC-040; DEC-042; DEC-047; DEC-048; DEC-051; DEC-057; DEC-058; DEC-059; DEC-061; DEC-068; DEC-071; DEC-074
---

# Privacy and Safety Contract

Privacy, consent, trust, and autonomy are foundational approved product requirements through `CTX-1.27`.

## Consent and sharing control

### REQ-PRIV-001

Location sharing is voluntary. The application must remain useful to a person who never shares precise location.

### REQ-PRIV-002

Users control what they share, who receives it, why it is shared, and how long it remains shared.

### REQ-PRIV-003

Temporary and contextual sharing is preferred. Temporary access should expire when appropriate, and users must be able to stop active sharing and revoke future sharing.

### REQ-PRIV-004

Joining a family, event, plan, or trip does not automatically enable location sharing. Location sharing remains an independent choice.

### REQ-PRIV-005

Family relationship does not imply permission to access information.

### REQ-PRIV-006

Administrative responsibility or the ability to manage family membership or structure does not grant access to private user information.

### REQ-PRIV-007

A Safety Circle is selected by the individual. Family Administrator status does not automatically add someone to another person's Safety Circle or grant Safety Circle access.

### REQ-PRIV-008

Creating a Concern or Safety Incident does not bypass privacy settings or unlock information that was not already authorized.

### REQ-PRIV-011

Signing in or receiving an invitation does not itself grant Family Group data access. A pending, expired, revoked, or already-used membership invitation grants no Family Group access. Ending active membership ends access to that Family Group's data. Family membership and Family Administrator responsibility remain separate from access to private information.

The Trip creator's continued access to their own Trip after membership ends is the narrow feature-specific exception defined by `REQ-PRIV-022`. It does not restore Family Group membership or access to other Family Group data.

### REQ-PRIV-012

Application-operator authority to configure Family Group capacity or perform controlled first Family Group bootstrap does not grant routine access to private Family Group content, Safety Circle information, location-related information, safety findings, or other private user information.

## Adult profile privacy and access

### REQ-PRIV-013

For the first adult self-profile slice:

- Display name is visible only to active members of the same Family Group.
- Contact email, contact phone, and birthday month/day default to **Only me** and may be shared with the active Family Group only through an explicit per-field choice.
- There is no public, link-based, operator-wide, administrator-wide, Household, relationship-based, or selected-person visibility.
- Only the adult profile owner may edit profile values or their visibility. A Family Administrator or Application Operator has no edit or visibility override.
- Authentication and sign-in identifiers must not be copied into or exposed as profile contact fields automatically.
- A user may remove an optional value or return its visibility to **Only me**.
- Server responses must omit unauthorized profile field values rather than relying on the client to hide disclosed data.
- Ending active Family Group membership ends access to that Family Group's shared profile information.
- Minor profiles and guardian actions are unavailable in this slice.

## Adult membership invitation privacy

### REQ-PRIV-014

For normal adult membership invitations:

- Receiving, opening, or viewing an invitation grants no Family Group data access. Access begins only after valid explicit acceptance creates active membership.
- The invitation link uses an opaque, cryptographically strong, single-use token. Only a token hash may be stored, and the link must contain no private Family Group data.
- Invalid, expired, revoked, declined, used, and otherwise unavailable invitations use a uniform unavailable response that does not reveal additional private information.
- The recipient email is invitation-delivery data. It must remain separate from Adult Profile contact fields and authentication identifiers and must not be copied or synchronized into either automatically.
- Acceptance requires a signed-in Account and explicit confirmation. Sign-in, invitation possession, Family Administrator status, and Application Operator responsibility remain insufficient by themselves to grant private Family Group access.
- A Development-only local outbox may stand in for email delivery during local work but must not activate outside the Development environment. No production delivery provider is selected by this requirement.

Only lifecycle security and audit metadata needed to enforce these rules is authorized. Long-term invitation-history retention requires a later decision before production use.

## Adult Family relationship privacy

### REQ-PRIV-015

For the first adult Family Relationship slice:

- A pending proposal is visible only to its two participants and grants no permission or private-data access.
- A relationship becomes visible to other active adults in the same Family Group only after both participants have explicitly consented through proposal and acceptance.
- An accepted relationship is not public, cross-group, or visible to inactive or former members. Ending active membership ends access to the relationship in that group.
- Relationship state grants no profile-field visibility, Family Group membership, Family Administrator responsibility, Household membership, Safety Circle membership, location access, sharing access, safety-workflow access, guardian authority, or other authorization.
- Family Administrators and Application Operators may not create, accept, change, or remove a relationship for another adult.
- Invalid, unavailable, inactive-member, and unrelated-group access must not disclose the relationship or additional private information.
- Declined or withdrawn proposals and removed relationships are absent from normal product views. This requirement does not decide long-term audit retention.
- Relationship labels describe mutually confirmed application context only. They must not imply verified legal, biological, caregiving, custody, guardian, emergency, or financial authority.

## Shared Family Date privacy

### REQ-PRIV-016

For the first shared Family Dates slice:

- A date is visible only to its creator and active adult members specifically associated with it in the same Family Group. Other group members and all unrelated groups receive no date access.
- Only the creator may add or remove associated members, and only active adult members of the same Family Group may be associated.
- The creator and associated active members may edit date content. Only the active creator may delete the date.
- Ending an associated member's active membership ends their access and removes their association. Access does not return automatically if active membership is later restored.
- Ending the creator's active membership makes the date unavailable in normal Family views. No administrator or other member receives an override or ownership transfer.
- Family Administrator or Application Operator status does not grant date visibility, editing, deletion, or configuration authority.
- Shared-date association, viewing, or editing grants no other profile access, membership, administrative responsibility, Household or Safety Circle membership, location/sharing access, safety-workflow access, or other authorization.
- An Adult Profile birthday is shown only under that field's existing explicit **Active Family Group** visibility. Shared-date behavior does not copy or broaden profile-field visibility.
- No group-level policy configuration or “main family group member” authority is defined for this slice.
- Retention handling follows `REQ-PRIV-017`. No exact retention duration or audit policy is set by this requirement.

## Shared Family Date retention

### REQ-PRIV-017

For shared Family Date data:

- When any associated member's active membership ends, or the creator deletes a date, the affected shared-date entry becomes unavailable in normal Family views for all Family Group members immediately.
- A retained copy may remain only for an explicitly approved, finite retention period. It is unavailable to Family Group members through normal product behavior during that period.
- When the creator leaves, the date is unavailable in normal Family views as required by `REQ-FAM-010`; retained data is not transferred to another member or administrator.
- At the end of the approved retention period, the retained date data must be deleted. The duration and any audit/cleanup details must be explicitly decided before implementation; do not infer them.
- Retaining a date does not extend membership, restore access, or grant a Family Administrator or Application Operator access to it.
- This requirement concerns shared Family Date data only. It does not define retention for other product data.
- This requirement does not change retention or sharing rules for an Adult Profile birthday that is displayed under `REQ-FAM-010` and `REQ-PRIV-013`.

### REQ-PRIV-018

For shared Family Date data that becomes unavailable under `REQ-PRIV-017`, the finite retention period is 30 calendar days, beginning when the entry first becomes unavailable in normal Family views. At the end of that period, the retained date data must be deleted. This duration applies only to shared Family Date data and does not define retention for other product data. Audit records, technical backups, and cleanup mechanics remain undecided and must not be inferred from this requirement.

### REQ-PRIV-019

Trusted Contact entries are private to the adult who maintains them by default. Sharing an entry or designating it for a defined use requires a separate explicit choice by that adult. Listing someone as a Trusted Contact does not itself grant that person access to Family Group data or authorize notifications. The specific fields, permitted audiences, and permitted uses remain undecided.

### REQ-PRIV-020

For an Event, Plan, or Trip that supports audience sharing, its creator must explicitly choose who the item is shared with when creating it. The proposed choices are the applicable Family Group as a whole, specific people, or both. This choice governs sharing of that item only. It does not itself authorize location sharing, notification delivery, access to profile or safety information, or event invitation/RSVP status. Existing narrower feature-specific privacy rules remain controlling. Recipient eligibility, multiple-group behavior, later audience changes, and the effect of membership changes are deferred and are not decided by this requirement, except where an approved feature-specific requirement defines them. `REQ-PRIV-022` defines the MVP Trip audience and lifecycle; it does not change Plan or Event rules.

### REQ-PRIV-021

For an Event, its creator must explicitly assign each selected active adult member of the one applicable Family Group either view access or edit access at creation. Edit access includes view access. The creator retains edit access. Only the creator and explicitly selected editors may update the Event details allowed by the Events workflow. Active Family Group membership is required to use Event access; Family Administrator status alone grants no access. In the initial implementation, the audience and editor assignments cannot be changed after creation. This Event-specific edit permission is separate from the item audience choice in `REQ-PRIV-020` and grants no access to other private data or capabilities.

### REQ-PRIV-022

For MVP Trip audience privacy and lifecycle:

- The creator explicitly chooses exactly one applicable Family Group or selected eligible active adult members from that same group. Multiple groups, a combined group-plus-selected-people audience, and non-system viewers are not supported. The creator always retains Trip access as creator, independently of audience membership and eligible group membership. The Trip is not transferred to another owner.
- For a Family Group audience, the recipient set is a snapshot of eligible active adult members at the time that audience is selected. For a selected-people audience, the recipient set is the explicitly selected eligible people. New group members do not automatically gain access to existing Trips.
- When an audience member loses eligible membership, their Trip access is revoked. Rejoining does not automatically restore access. The creator must explicitly add that person again or refresh/change the audience.
- Only the Trip creator may explicitly change its audience after creation. For MVP, one deliberate change replaces the entire saved audience with exactly one resulting mode: the Trip's existing applicable Family Group or Selected People from that group. The creator may switch in either direction. Audience editing cannot change the Trip's Family Group, remove creator authority, transfer ownership, or add another organizer. It has no multiple concurrent modes, partial Group overrides, or hidden individual access exceptions.
- Selecting Family Group, including a refresh while already in Group mode, replaces the saved recipient snapshot with a fresh snapshot of currently eligible active adult members of the applicable group. Group mode does not allow individual additions or exclusions; a creator who wants individual exceptions must choose Selected People. Later joiners, membership loss, and rejoining continue to follow the snapshot rules above.
- Selecting Selected People replaces the complete saved recipient set. Every recipient must be distinct, currently eligible, and an active adult member of the Trip's applicable Family Group. At least one eligible non-creator recipient is required after editing; independent creator access does not satisfy this minimum. The server validates the full replacement set at commit. An invalid or stale recipient rejects the entire change and leaves the existing audience unchanged.
- A creator who is no longer eligible to view or select current Family Group members retains own-Trip creator access and may reduce the audience using only currently eligible recipients already represented in that Trip's saved audience. They may switch Group to Selected People using a nonempty eligible subset of those recipients or replace a Selected People set with a smaller nonempty eligible subset. They may not discover newly eligible members, add recipients absent from that saved audience, refresh a Group snapshot, switch Selected People to a newly generated Group snapshot, or regain Family Group directory or unrelated profile access through Trip editing. Creator authority persists when these expansion operations are unavailable.
- Before confirmation, the creator must understand the resulting mode and who will gain or lose Trip access. The server is authoritative for creator permission, the current and proposed audience, recipient eligibility, and the gained and removed recipients. A preview for a departed creator may use only the Trip's saved currently eligible recipients needed for a permitted reduction; it must not reveal the current Family Group directory. At commit, authority and eligibility are revalidated. If a change would make the committed access effects differ from the confirmed preview, it must not silently commit; a refreshed preview and confirmation are required. This does not prescribe API or storage mechanics.
- Audience replacement takes effect atomically as one access change. Removed recipients immediately lose Trip list/detail and normal Trip Notes read, add, edit, and delete access. Their existing Notes retain content, provenance, and state. Newly authorized recipients gain normal Trip access and access to visible **ACTIVE** Notes under `REQ-TRIP-010`, but not retained **AUTHOR_DELETED** or **CREATOR_HIDDEN** Notes. Every Note mutation uses current Trip authorization when its write is accepted; after revocation commits, a removed recipient cannot complete a later mutation using stale client state. This does not prescribe locking, transaction, or concurrency-token mechanics.
- After a change, the creator may see the exact resulting audience and selected recipient identities. An authorized non-creator viewer sees only the approved audience mode/context and does not receive the Selected People identity list. Audience editing does not broaden detail disclosure.
- Audience changes in this MVP capability do not send notifications, start or stop Sharing Sessions, change a Sharing Session audience, grant location or another person's Check-In access, or alter sharing consent. No dedicated audience-change history, user-facing history, generalized audit subsystem, or special retained change records are required solely for this capability; ordinary modification metadata required by existing persistence conventions is unaffected. Future audit behavior requires separate scope.
- Trip audience membership, traveler status, and Sharing Session audience are separate authorization concerns. Trip visibility does not grant another person's location, separately protected Check-In information, or Sharing Session data. Each person controls Sharing Sessions involving their own information under the existing consent rules.
- These Trip-specific rules do not alter Plan behavior under `REQ-PLAN-005` or Event access under `REQ-PRIV-021`.

### REQ-PRIV-023

For Events, audience visibility, invitation, participant or RSVP status, and Need responsibility are distinct and do not inherently grant Event detail editing, invitation or RSVP management, Need management, Update posting, location access, Sharing Session data, or unrelated private-data access. The organizer has only the Event-specific powers expressly stated in `REQ-EVT-009` through `REQ-EVT-012`; organizer status grants no override of another person's private data or sharing choice. The only Event detail editors are those authorized by `REQ-PRIV-021`. An Event invitation may be issued only to an already authorized Event viewer and never grants Event visibility; revocation does not itself remove that visibility. Visible Event Updates follow current Event audience access. Hidden Updates and prior Update revisions are unavailable through normal Event views and APIs, and retained history must not create an alternate access path. Event context does not authorize a Sharing Session for another person; each person controls their own sharing under `REQ-PRIV-001` through `REQ-PRIV-004`. Family Administrator status remains no override. This Event-specific rule does not alter Plan or Trip access.

### REQ-PRIV-024

Trusted Contact entries remain owner-managed and private by default under `REQ-PRIV-019`. Designating a person as a Trusted Contact does not itself grant that person Family Group, Event, Trip, Sharing Session, or safety-incident access, and does not automatically add that person to the owner's Safety Circle. Any future use or sharing of the Trusted Contact entry requires its own approved rule and the adult owner's separate explicit choice. Trusted Contact and Safety Circle remain distinct concepts.

### REQ-PRIV-025

For the first Concern slice:

- Any active adult member of one Family Group may manually create a Concern about an active adult in that same group. Self-Concern is allowed; otherwise the subject must be a distinct adult in that group. Family Administrator, Application Operator, relationship, and Safety Circle status add no Concern authority.
- The creator explicitly selects the Concern participants from currently active adult members of that same Family Group. The subject receives no access solely by being named, but the creator may explicitly select the subject. A nonparticipant receives no Concern list, detail, history, or existence disclosure. A denied read must not reveal whether the Concern exists.
- The creator retains Concern access while their Family membership remains active, independently of the selected participant list. The creator may explicitly add or remove other participants; each change revalidates current eligibility and changes their future access immediately. A person whose access ended does not regain it through rejoining alone; the creator must explicitly select them again.
- The creator alone may add/remove participants and resolve an Open Concern. Every participant change revalidates eligibility and affects subsequent access. Selected eligible adult participants may read authorized history; contribute observations and explicit unknowns; correct their own entries by appending an attributed correction; and add a distinct attributed challenge/report about another entry. Participants cannot overwrite or delete another person's meaningful history, change participants, or resolve. No one may reopen a Resolved Concern in this slice.
- Concern participation grants no access to private profile information, Event, Plan, Trip, location, Check-In, Sharing Session, Safety Circle, Trusted Contact, or other protected records. Concern records do not copy or link those records. A participant may manually report information they are independently authorized to know. Provenance shown to another participant must not disclose an underlying source or record that participant cannot access independently.
- A Concern entry preserves contributor, source or explicitly unknown source, recorded time, event time when known, confidence category, known/unknown status, and freshness where applicable under `REQ-SAFE-002` and `REQ-SAFE-003`. The only confidence categories are `Confirmed`, `Reported`, `Assumption`, `Unknown`, and `Outdated`; numerical danger/confidence scores are prohibited. Outdated information must not erase the original classification/history. Corrections append contributor, time, explanation, and reference to the original; meaningful prior entries are not silently overwritten or deleted.
- Resolution requires an attributed note/reason and resolution time. It means coordination ended; it does not itself mean confirmed safe, confirmed danger, verified facts, or emergency resolution. Resolved Concerns accept no further contributions and remain readable only to the authorized audience during their product-history retention period.
- Ending an actor's active Family membership or other Concern eligibility immediately ends their future Concern reads and writes. Rejoining does not automatically restore access. Prior contributions retain their authorship for the remaining authorized audience until the Concern expires. Stale client writes after revocation are denied.
- If the creator loses active Family membership while the Concern is Open, the Concern immediately becomes Frozen. The creator loses access; remaining eligible participants may read retained history but may not contribute, change participants, or resolve. Ownership is not transferred and the record is not marked Resolved. Frozen does not mean safe, in danger, or verified. Continued coordination requires a new Concern.
- An Open Concern with no authorized Concern mutation for 30 calendar days automatically becomes Frozen. Creation, participant changes, contributions, corrections, challenges, or resolution count as activity; reads and denied attempts do not reset this period. Freezing is an attributed system lifecycle action with its time and reason. It does not mark the Concern Resolved, imply safety/danger, verify or invalidate information, create an Incident, or trigger escalation. No pre-freeze acknowledgment is required.
- Resolved Concern product history is retained for 180 days from resolution time. Frozen Concern product history is retained for 180 days from freeze time. At expiry, the Concern and its product history, including participant history, entries, unknowns, challenges, corrections/links, Concern provenance, resolution, and frozen-state information, are removed from active product storage/access. No per-Family override or indefinite retention is provided.
- Minimal Concern audit evidence is retained for 180 days from each event. It records as needed the Concern and actor identifiers, action, timestamp, authorization result, and affected participant identifier for participant changes. It includes creation, participant changes, contribution and correction/challenge creation, freeze, resolution, eligibility/access revocation, and denied sensitive reads/writes. It must not copy Concern text, private profile or location data, or protected source content. There is no audit viewer in the first Concern slice; participants, Family Administrators, and Application Operators gain no audit access from their roles. Any future operational/security audit access requires a separate approved policy.
- Concern data removed from active product storage must age out of ordinary backup retention within no more than 30 days. Deployment/operations documentation must describe the backup-aging mechanism. A restore must not make Concern data that had already expired normally available again; the implementation must reapply current retention state after restore. These are application defaults, not legal/public-records requirements. A future deployment-specific legal, contractual, regulatory, or records-management obligation requires qualified review before that deployment. It does not silently alter these defaults.

These Concern-specific rules do not define Safety Incident access or retention, which are governed by `REQ-PRIV-026`. They do not authorize location, Check-In, Sharing Session, notification, Safety Circle, or Trusted Contact capabilities. Starting an Incident from a Concern changes none of the Concern rules above.

### REQ-PRIV-026

For the first Safety Incident slice (workflow in `REQ-SAFE-016`):

**Authority.**

- Any active adult member of one Family Group may manually create an Incident about one active adult in that same group, including themself.
- None of the following adds Incident authority: Family Administrator, Application Operator, relationship, Safety Circle, or Trusted Contact status.

**Explicit grants.**

- Incident access exists only through explicit, per-Incident grants. The coordinator holds a grant. The coordinator explicitly selects participants from currently active adult members of the same Family Group.
- Each grant binds to the membership ID and `AccessVersion` captured when it was granted. Every read and write rechecks it against current membership.
- None of the following grants Incident access: Family membership alone, Family Administrator or Application Operator status, Safety Circle membership, Trusted Contact designation, relationship, being the subject, or participation in a related Concern.
- The subject receives no automatic access. The coordinator may explicitly add the subject under the same eligibility and access rules as any other participant.
- Trusted Contacts and other people who are not active adult members of the group are not Incident participants in this slice.

**Permissions.**

- The coordinator alone may:
  - add or remove participants (eligibility is revalidated on each change);
  - hand off coordination;
  - post coordinator summaries;
  - create tasks for other people;
  - cancel any task;
  - resolve the Incident.
- Participants may, while the Incident is Open:
  - read authorized history;
  - add observations, explicit Unknowns, and contact attempts;
  - correct their own entries by appending;
  - add challenges;
  - create a task assigned to themself, and cancel a task they created;
  - complete a task assigned to them.
- No one may overwrite or delete another person's meaningful history.
- Authorized viewers see:
  - the subject's ordinary same-group display identity;
  - the coordinator;
  - the current participant identities;
  - the authorized history.

**Nondisclosure.**

- A denied read must not reveal whether an Incident exists, or its subject, participants, or content.
- The same unavailable result is used for each of these:
  - a nonexistent Incident;
  - an existing but unauthorized Incident, including one where the caller is the unselected subject, an Administrator, or a participant of a related Concern;
  - an expired Incident, before or after physical cleanup;
  - a removed participant;
  - a caller whose membership ended;
  - a stale grant (a different membership ID or `AccessVersion`);
  - an unauthorized nested entry or task.
- Lists omit every Incident the caller cannot currently access.
- Server authorization is authoritative. Writes are revalidated when they are accepted.

**Membership and access loss.**

- Ending an actor's active membership or Incident eligibility immediately ends their future Incident reads and writes. Stale writes are denied.
- Rejoining the Family never reactivates an old Incident grant. While the Incident is Open, the coordinator may explicitly add the person again, which creates a new grant.
- Prior contributions keep their authorship for the remaining authorized audience until the Incident expires.
- Coordinator loss and subject membership loss follow `REQ-SAFE-016`. A subject's departure does not by itself remove other participants' access or create new access.

**Protected information.**

- Incident participation, coordination, or subject status grants no access to: private profile information, Events, Plans, Trips, location, Check-Ins, Sharing Sessions, live maps, background tracking, Safety Circle, Trusted Contact, or other protected records.
- Incident records do not copy or link those records.
- A participant may manually report information they are independently authorized to know, as their own attributed entry. Provenance shown to others must not disclose an underlying source or record they cannot access independently.
- Contact attempts store no structured phone, email, or address details. They are never populated from profiles or Trusted Contact entries merely because an Incident exists.
- This rule does not resolve `OD-002`.

**Concern lineage.**

- An Incident started from a Concern stores the originating Concern ID solely as a non-authorizing lineage reference for traceability. In this slice, the Concern stores no Incident reference.
- The lineage reference must not:
  - grant access or copy an audience;
  - expose private information;
  - make the Incident readable to Concern participants, or the Concern readable to Incident participants;
  - copy entries or history;
  - change the Concern's state or retention.
- Any product display of the lineage is limited to a viewer who is currently authorized for both records. For anyone else, it must not reveal whether the other record exists, its state, or its content.
- The reference is removed with the Incident at expiry. After the Concern expires, the reference must not disclose anything about it.

**Retention.**

- An Open Incident is retained while Open and freezes after 30 calendar days without authorized Incident activity, under `REQ-SAFE-016`.
- Frozen Incident product history is retained for 365 days from the freeze time.
- Resolved Incident product history is retained for 365 days from the resolution time.
- During those periods, history is read-only and only currently eligible viewers may read it.
- At expiry, the Incident becomes unavailable to all normal product access immediately, even before physical cleanup. The whole Incident is then removed from active product storage. This includes:
  - participant and coordinator history;
  - entries, Unknowns, summaries, and contact attempts;
  - task results, corrections, challenges, and tasks;
  - provenance and the lineage reference;
  - freeze and resolution details.
- No per-Family override or indefinite retention is provided.

**Audit.**

- Minimal Incident audit metadata is retained for 365 days from each event, independently of product-history deletion.
- It covers these events:
  - creation;
  - participant changes and coordinator handoff;
  - entry creation, by kind;
  - task creation, completion, and cancellation;
  - freeze and resolution;
  - access revocation;
  - denied sensitive reads and writes.
- As needed, it records these identifiers and fields:
  - Incident and actor identifiers;
  - action and timestamp;
  - authorization result;
  - affected participant or assignee identifiers;
  - entry or task identifiers.
- **Denied-attempt summaries.** Denied attempts by authenticated actors are stored as at most one summary per actor, broad category, and UTC hour. The category is `IncidentReadDenied` or `IncidentWriteDenied`. Each summary holds the first and last attempt times and a count, and no attempted Incident identifier.
- Audit must not copy Incident text, contact-attempt targets or notes, task descriptions, resolution content, private profile or location data, or protected source content.
- There is no product-facing audit viewer. Participants, coordinators, Family Administrators, and Application Operators gain no audit access from their roles. Any future operational audit access requires a separate approved policy.

**Backup and restore.**

- Incident data removed from active product storage must age out of ordinary backup retention within no more than 30 days. Deployment and operations documentation must describe the backup-aging mechanism.
- A restore must not make already-expired Incident data normally available. The implementation must reapply current retention state after a restore.
- These are application and deployment defaults, not legal or public-records requirements. A future deployment-specific legal, contractual, regulatory, or records-management obligation requires qualified review before that deployment. It does not silently alter these defaults.

## Item links and Family context

### REQ-PRIV-027

An item URL, a navigation area, and the selected Family are never authorization. For any Event, Plan, Trip, Concern, or Safety Incident:

- The server decides whether the current user may read the specific item, under that item's own access rules, before anything about the item is disclosed. Current Family membership, the selected Family, a navigation grouping, or a URL does not by itself grant or imply read access.
- The item's Family, name, existence, or any other attribute is disclosed to the client only after that authorization succeeds. The client must not switch the selected Family, or show a Family name, based on the URL or client state alone.
- A nonexistent item and an item the user may not read produce the same unavailable result. It must not reveal whether the item exists, which Family it belongs to, or whether that Family exists. The selected Family does not change.
- This adds no access path. Each domain's existing audience, participant, and access rules stay authoritative, including `REQ-PRIV-025` and `REQ-PRIV-026`.

## Safety information invariants

### REQ-SAFE-001

A missed expected arrival, missed check-in, communication gap, or lack of location data represents uncertainty and must not be treated as confirmation that someone is in danger.

### REQ-SAFE-002

Important safety information must be classified as one of:

- **Confirmed:** Directly verified.
- **Reported:** Provided by someone but not independently confirmed.
- **Assumption:** A hypothesis being considered.
- **Unknown:** No reliable answer currently exists.
- **Outdated:** Previously valid information that is no longer current.

### REQ-SAFE-003

Important safety information must expose its source, contributor, timestamp or freshness, and confidence category.

### REQ-SAFE-004

Unknowns are first-class information. Corrections must preserve useful incident history rather than silently erasing or replacing relevant prior information.

### REQ-PRIV-009

Stale information must not appear current. Old sensitive information must not remain indefinitely merely because it was once useful. Concern history retention is defined by `REQ-PRIV-025`; Safety Incident history retention is defined by `REQ-PRIV-026`, which resolves `OD-004`.

## Anti-surveillance boundaries

### REQ-PRIV-010

The product must not provide a default family-wide live location map, covert tracking, behavioral surveillance, historical movement playback, or movement anomaly detection. Use **location sharing**, not **location tracking**, in product language.

## Notifications

### REQ-NOTIFY-001

Notifications must be actionable and restrained so large families do not receive every minor update.

### REQ-NOTIFY-002

Safety-related notifications should generally escalate outward gradually: the individual, then selected trusted people, then incident participants, and then the broader family only when intentionally appropriate.

### REQ-NOTIFY-003

Supported notification purposes may include event invitation or RSVP, event task assignment, event change, check-in reminder, a missed expected check-in according to preferences, trip departure or arrival update, Concern update, incident task assignment, major incident update, and resolution.

## Open decision references

- `OD-002`
- `OD-003`

See `../decision-log.md#open-product-decisions`. These questions are not approved requirements. `OD-004` is resolved by `DEC-071` through `REQ-PRIV-026`.
