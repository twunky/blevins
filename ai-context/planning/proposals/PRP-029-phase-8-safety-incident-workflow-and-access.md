# PRP-029 — Phase 8 Safety Incident Workflow and Access Proposal

## Metadata

- **Proposal ID:** `PRP-029`
- **Status:** `APPROVED` (owner-approved under `DEC-071` with the lineage refinement; canonicalized in `CTX-1.26`). No implementation authorized.
- **Date:** 2026-10-01 (proposed and approved)
- **Requested by:** Project owner
- **Context baseline:** Prepared against `CTX-1.25`. Approved rules are canonical in `CTX-1.26` (`REQ-PRIV-026`, `REQ-SAFE-016`, and amendments).
- **Required context:** `ai-context/README.md`; `project-status.md`; `planning/roadmap.md` (Phase 8 exit criteria); `decision-log.md` (`DEC-066` through `DEC-070`, `OD-001` through `OD-004`); `core/product-contract.md` (`REQ-PROD-003/004/008/012`); `core/privacy-safety-contract.md` (`REQ-PRIV-005` through `012`, `019`, `024`, `025`, `REQ-SAFE-001` through `004`, `REQ-NOTIFY-001` through `003`); `core/ux-contract.md` (`REQ-UX-001/004/005/008/010/011`); `core/domain-glossary.md` (Safety coordination terms); `workflows/safety.md` (`REQ-SAFE-005` through `015`); `workflows/sharing-checkins.md` (`REQ-SHARE-008/009`); `planning/proposals/PRP-028-phase-8-concern-access-and-lifecycle.md`; `planning/stories/WI-008-001-concern-domain-and-access-foundation.md` and `VER-WI-008-001` (membership-grant, nondisclosure, audit, expiry patterns); `WI-008-002` and `VER-WI-008-002` (Concern UI precedent); `planning/proposals/PRP-003-technical-architecture.md` (authorization and audit direction); `references/ux/safety/safety-coordination.md` and `claude-handoff/reference/panel4-safety-aware.png` (visual reference only).
- **Excluded context:** bootstrap source, archived material, Phase 2–7.5 verification reports, Events/Plans/Trips workflows (no Incident linkage is proposed), and `ai-context/skills/`.
- **Affected authority (on approval):** `workflows/safety.md`, `core/privacy-safety-contract.md`, `core/product-contract.md#req-prod-012`, `core/requirements-index.md`, `decision-log.md` (`OD-004`), and a new context baseline candidate. Owner approval and a canonical update are both required before any implementation story.
- **Model tier:** `DEEP` — safety, privacy, permissions, history, and retention.

## Request and authority boundary

Define the minimum product model for a manually created, coordinated **Safety Incident**, and prepare owner decisions for `OD-004` (Incident-history retention). This is a planning proposal. It is not an approved product rule, an implementation story, a schema, or execution authorization. No Incident code, migration, UI, or story is created by it.

Recommendations below are marked **Recommended**. Each one needed owner approval through the decision table in [§ 27](#27-owner-decisions-required).

## Owner-approved decisions (DEC-071)

On 2026-10-01 the project owner accepted every recommendation in this proposal, `D-01` through `D-19`, with one refinement and several confirmations. **Canonical wording now lives in `REQ-SAFE-016` and `REQ-PRIV-026`.** If the proposal body below differs from those requirements or `DEC-071`, the canonical requirements control.

| Area | Approved outcome |
|---|---|
| `D-08` Concern / Incident linkage — **refined** | The Incident stores the originating Concern ID as a **non-authorizing lineage reference**, as the owner preferred, Incident → originating Concern only. The Concern stores no Incident reference, which avoids changing the approved Concern persistence. The lineage grants no access, copies no audience, exposes no private information, makes neither record readable to the other's audience, copies nothing, and changes no Concern state. It is shown, if at all, only to a viewer currently authorized for both records. It is removed with the Incident at expiry. This replaces the "no stored link" recommendation in § 3. |
| `D-06` Coordinator loss | Confirmed Option A: freeze, with no promotion or resolution. A coordinator who is unavailable but still eligible triggers no transfer, and the inactivity rule applies. |
| `D-07`, `D-09` Start an Incident | Confirmed: Concern creator only, while the Concern rules permit access. Only the subject is prefilled. No copying or preselection. Future selective transfer needs separate scope. |
| `D-10` Tasks | Confirmed. A participant cancelling their own self-created task is carried from this proposal's default. |
| `D-11` Resolution | Confirmed. The system records the resolution timestamp. No preset verdicts such as Safe, Dangerous, or Emergency resolved. |
| `D-16` `REQ-SAFE-008` labels | Deferred as non-MVP concepts. The canonical text now records the MVP lifecycle `Open` / `Frozen` / `Resolved`. |
| Subject membership loss | Confirmed: the Incident stays intact, other participants' access is unaffected, and no new access is created. |
| `D-13`–`D-15` ★ `OD-004` | **Resolved.** 365 days from freeze or resolution for product history. Expired data is unavailable immediately; physical cleanup may follow. 365 days per event for audit, with `DEC-069`-style denied summaries (`IncidentReadDenied` / `IncidentWriteDenied`, no Incident ID). Backups age out within 30 days after active deletion, and a restore never reactivates expired data. These are product defaults, not legal requirements. |
| All other decisions | Accepted as recommended. |

`REQ-SAFE-007`, `REQ-SAFE-008`, and `REQ-PROD-012` were reconciled as planned in "Canonical tensions found" below.

## Repository state verified (2026-10-01)

| Expected state | Verified against | Result |
|---|---|---|
| `CTX-1.25` active | `README.md`, `project-status.md` | Confirmed |
| Phase 8 `CURRENT / IN PROGRESS` | `planning/roadmap.md`, `DEC-066` | Confirmed |
| PRP-028 `APPROVED` | PRP-028 metadata, `DEC-067`, `DEC-068` | Confirmed |
| WI-008-001 and WI-008-002 `DONE / PASS` | `project-status.md`, `VER-WI-008-001`, `VER-WI-008-002` | Confirmed |
| Site-wide mockup alignment approved | `DEC-070` | Confirmed |
| `OD-001`, `OD-002`, `OD-004` open | `decision-log.md#open-product-decisions` | Confirmed |
| No Incident implementation | Search of `src/` for "incident" in `.cs`, `.ts`, `.html` | Confirmed: no matches |
| No Incident story | `planning/stories/` | Confirmed: none |
| `PRP-029` unused | Search of `ai-context/` and `claude-handoff/` | Confirmed: no prior use. PRP-022 and PRP-024–026 were session-only proposals; PRP-029 is the next free number. |

## Current evidence and context

### What canonical context already says about Incidents

| Anchor | Approved content | Consequence for this proposal |
|---|---|---|
| `REQ-PROD-012` | Concern and Safety Incident are separate. A future Concern-to-Incident action "requires separate approved design." | This proposal is that design. It must stay manual. |
| `REQ-SAFE-006`, glossary | "A Concern may instead escalate to a coordinated Safety Incident." | A manual path from Concern to Incident is canonically anticipated. |
| `REQ-SAFE-007` | The Incident is a structured family coordination workspace. It *may* include the person involved, status, coordinator, participants, time the concern began, current summary, last confirmed contact, authorized last shared location, expected destination and arrival, trip information, travel companions, relevant contacts, contact attempts, findings, tasks, timeline, notes, and resolution. Attachments are deferred. | "May include" sets an upper bound, not a mandatory field list. Fields that depend on unimplemented domains (shared location, Trip linkage) can be deferred. |
| `REQ-SAFE-008` | "Possible family coordination states" are Concern Raised, Reviewing Information, Coordinating Response, Escalated Externally, Contact Re-established, Resolved, and Closed / Archived. They must not be presented as official emergency classifications. | "Possible" permits a smaller lifecycle. See [§ 7](#7-lifecycle) and decision `D-16`. |
| `REQ-SAFE-009` | An Incident *may* identify a coordinator and participants. Participation does not expand authorization to private information. | A coordinator concept is canonical. Its powers are undefined. |
| `REQ-SAFE-010` | Findings and other important Incident information use `REQ-SAFE-002` through `004` (categories, provenance, unknowns, correction history). | Reuse the approved Concern provenance model. |
| `REQ-SAFE-011` | The workflow **must** support recording contact attempts and their known results. | Contact attempts are required in v1. |
| `REQ-SAFE-012` | Tasks may cover contacting people, verifying plans, gathering information, and coordinating outside assistance. Tasks **must** show owner, status, result, and completion time. | Minimum task fields are fixed by canon. |
| `REQ-SAFE-013` | The timeline **must** preserve meaningful chronological activity. It is the family's authoritative chronological reconstruction, and each item keeps its confidence classification. | A timeline is required. Its storage shape is not. |
| `REQ-SAFE-014` | Resolution **must** capture when contact was restored or resolution occurred, the outcome, a summary, assistance provided if applicable, unresolved follow-up, important corrections, and relevant lessons where appropriate. The Incident transitions to resolution rather than disappearing. | Incident resolution needs more content than Concern resolution. |
| `REQ-PRIV-005/006/007/012` | Relationship, Family Administrator, Safety Circle, and Application Operator status grant no private-data access. | No role-based Incident access. |
| `REQ-PRIV-008` | Creating a Concern or Incident does not bypass privacy or unlock unauthorized information. | Incident access unlocks no other data. |
| `REQ-PRIV-009`, `OD-004` | Stale information must not appear current. Old sensitive information must not remain indefinitely. Incident-history retention is open. | This proposal must offer finite options. |
| `REQ-PRIV-019`, `REQ-PRIV-024` | Trusted Contact designation grants no safety-incident access and does not add someone to the Safety Circle. | Trusted Contacts get no Incident access. |
| `REQ-SAFE-001` | Missed arrival, missed check-in, communication gap, or missing location is uncertainty, not danger. | No automatic Incident creation. |
| `REQ-SHARE-008` | A missed check-in does not automatically create a Concern or Incident. | Same. |
| `REQ-NOTIFY-002/003` | Safety notifications should escalate outward gradually. Incident task assignment, major update, and resolution are possible notification purposes. | Notifications are future work and stay out of this slice. |
| `REQ-UX-004/005/010` | Calm language. Distinguishable categories. Visible freshness. Recording a contact attempt and adding a finding should take minimal interaction on mobile. | UI constraints for a later story. |
| PRP-003 principles 3, 4, 6, 8 | Participation is separate from permission. Safety records keep provenance. Notification eligibility is separate from disclosure. Sensitive safety actions are auditable, with retention subject to `OD-004`. | Audit is expected. Duration is open. |

### What the repository is silent on

- Who may create an Incident, see it, contribute, or control it.
- Coordinator powers, handoff, and loss.
- Incident lifecycle mechanics (as distinct from the "possible" coordination labels).
- What happens to an Incident when its **subject** loses Family membership. The Concern rules are also silent on this. The implemented Concern code checks subject eligibility only at creation and leaves the Concern unchanged afterward.
- Incident retention, audit duration, and backup aging (`OD-004`).
- Any legal or public-records retention obligation (none found).

### Reusable approved precedent

The Concern foundation (`REQ-PRIV-025`, `REQ-SAFE-015`, WI-008-001, `DEC-069`) already proves these patterns in code and tests:

- explicit per-record grants that capture the membership `Id` and `AccessVersion`, rechecked on every read and write;
- uniform nondisclosing `404` for missing, unauthorized, expired, and nested IDs;
- append-only corrections and separate challenges;
- `Open` / `Frozen` / `Resolved` with request-time effective state and a reconciliation sweep;
- metadata-only audit with hourly denied-attempt summaries;
- whole-aggregate expiry and independent audit clocks.

This proposal reuses them wherever they fit, and says so where it diverges.

## Canonical tensions found (not conflicts)

These do not block the proposal. Approval should resolve each one explicitly.

1. **`REQ-SAFE-008` state list vs. a minimal lifecycle.** The canon lists seven "possible" states. The proposal recommends a three-state lifecycle and defers the descriptive coordination labels (`D-16`). Because the canon says "possible," this is a scoping choice, not a contradiction. "Concern Raised" is now covered by the separate Concern record.
2. **`REQ-SAFE-007` fields that depend on unbuilt domains.** "Authorized last shared location," "trip information," and "travel companions" would need Sharing Sessions or Trip linkage. Sharing is unbuilt and `OD-002` is open. v1 records these only as attributed text findings. No linked fields are added.
3. **`REQ-PROD-012` wording.** It currently says the *first Concern slice* has no transition and a future one "requires separate approved design." If `D-07` is approved, `REQ-PROD-012` needs a clarifying amendment that allows the manual, subject-only transition proposed here.
4. **Subject membership loss** is silent for both Concern and Incident. The recommended Incident rule ([§ 18](#18-membership-loss-and-rejoin)) matches the Concern code's current behavior. The owner may want the same rule written down for Concern later. That is out of scope here.

No `BLOCKED—CONTEXT CONFLICT` condition was found.

---

## 1. Concern vs. Safety Incident

| | **Concern** (approved, implemented) | **Safety Incident** (proposed) |
|---|---|---|
| Purpose | Lightweight review of **uncertainty**: what is actually known, before or instead of escalating. | Structured **coordination** of a family response when someone deliberately decides coordinated action is warranted. |
| Typical trigger | Missed contact, missed arrival, stale or incomplete information, conflicting reports. | A person's deliberate judgment that the family should coordinate tasks and contact attempts. Never time, a missed expectation, or Concern age. |
| Authority | Creator | One explicit **coordinator** (initially the creator), with voluntary handoff. |
| Content | Observations, unknowns, corrections, challenges | All of the Concern content, plus structured contact attempts, tasks, coordinator summaries, and a richer resolution. |
| Timeline | Entry history | A chronological projection across all Incident records ([§ 11](#11-timeline)). |
| What it asserts | Nothing about danger | **Nothing about danger either.** An Incident means "the family is coordinating." It does not mean "someone is confirmed to be in danger." |

**The invariant holds: uncertainty is not confirmed danger.** None of the following may create an Incident, move a Concern toward one, or change any Incident state:

- elapsed time;
- missed contact, missed arrival, or a failed or missed check-in;
- unavailable or stale location or information;
- Concern age, inactivity, or freeze;
- any entry's content or category, including a `Confirmed` entry.

Every Incident is created by a deliberate, attributed human action. Its existence must not be presented as a danger finding. UI copy should say "coordinating" and never "emergency," "danger," or "missing."

## 2. Incident creation

| Question | Recommendation | Rationale |
|---|---|---|
| 1. Who may create | **Any active adult member** of one Family Group, about an active adult in that same group. Same rule as Concern. | Consistent with the approved Concern rule. Relationship or Safety Circle gating is impossible today (Safety Circle is unbuilt) and would block real coordination. |
| 2. Subject required | **Yes — exactly one subject**, an active adult member of the same group at commit. | `REQ-SAFE-007` centers on "the person involved." One subject keeps access, retention, and UI simple. Other people (companions, for example) appear only as text in findings. |
| 3. Self-Incident | **Allowed.** | Consistent with self-Concern. A person may coordinate help for their own situation (for example, stranded travel). No added privacy risk. |
| 4. Creator must be an active adult member | **Yes**, checked at commit against an active membership row. Pending invitees, former members, other-group members, and operators are denied. | Same as `REQ-PRIV-025`. Minors are outside MVP (`DEC-053`). |
| 5. Administrator changes authority | **No.** Family Administrator status adds nothing to create, view, control, or audit. | `REQ-PRIV-006`. |
| 6. Direct creation | **Allowed.** An Incident does not require a prior Concern. | `REQ-SAFE-005` says a Concern is used "before or instead of" an Incident, not that one is a prerequisite. Requiring one would add friction in genuine situations. |
| 7. Creation from a Concern | **Allowed as a manual convenience entry point** ([§ 3](#3-concern--incident-transition)). | `REQ-SAFE-006` anticipates escalation. |
| 8. Manual only | **Yes, always.** No system, timer, rule, or category ever creates an Incident. | `REQ-SAFE-001`, `REQ-SHARE-008`, `REQ-PROD-012`. |

**Required at creation:** subject; a short **reason** in calm, uncertainty-based language (it becomes the first coordinator summary); zero or more explicitly selected participants ([§ 4](#4-access-and-participants)).

**Optional at creation:** "concern began at" (or explicit *Unknown*) and "last confirmed contact at" (or explicit *Unknown*). Each carries a confidence category and source, and is stored as an ordinary attributed entry, not as a bare header field. This covers the `REQ-SAFE-007` items "time the concern began" and "last confirmed contact" without inventing unsourced facts.

Creation fails atomically, with the nondisclosing unavailable result, if the creator or subject is ineligible or any selected participant is ineligible.

## 3. Concern → Incident transition

**Recommended model: "Start an Incident" is a manual shortcut, not a transformation.**

| Question | Recommendation |
|---|---|
| Can a Concern lead to a new Incident? | Yes. Concern detail offers a **Start an Incident** action that opens the normal Incident creation form with **only the subject prefilled**. |
| Who may use it | **The Concern creator only**, while they still have Concern access (Open, Frozen by inactivity, or Resolved, before expiry). Any eligible adult, including Concern participants, can still create an Incident directly ([§ 2](#2-incident-creation)). The shortcut is creator-only because the creator controls the Concern lifecycle, and this reduces duplicate Incidents for the same situation. |
| Does the Concern remain intact? | **Yes. It is completely unchanged.** It is not frozen, resolved, locked, or annotated by the system. |
| What happens to the Concern | Its state is whatever its creator chooses. The UI may *suggest* that the creator resolve it with a note such as "continued as an Incident," but the system does not do it. Concern inactivity freeze and expiry continue on the Concern's own clocks. |
| Is an Incident ID or link stored on the Concern? | **No** (recommended, `D-08`). No link in either direction. *Superseded by `DEC-071`: the Incident stores a non-authorizing originating-Concern lineage reference, and the Concern stores nothing. See "Owner-approved decisions" above.* |
| Is information copied automatically? | **No.** Only the subject identity is prefilled, and the creator can change it before submitting. |
| Does participant access transfer? | **No.** Concern participants are **not** preselected or suggested. The Incident creator selects Incident participants explicitly from current eligible members. |

**Why no stored link (`D-08`)** *(historical recommendation; the owner chose the Incident → Concern lineage reference under `DEC-071`, with the disclosure and retention safeguards in `REQ-PRIV-026`):*

- A link visible on the Concern would disclose an Incident's existence to Concern participants who are not Incident participants.
- A link visible on the Incident would point to a Concern that some Incident participants cannot access, and would leave a dangling reference when the Concern expires at 180 days.
- Either link ties two different retention clocks together.

The creator can still mention the connection in free text if they choose.

**Alternative (`D-08` option B):** a one-way, non-navigable "Started from a Concern" marker on the Incident, shown only to Incident participants, carrying no Concern ID or content, and kept even after the Concern expires. It adds little value. Not recommended for v1.

### Information-transfer rules (`D-09`)

| Item | Recommended v1 behavior |
|---|---|
| Subject identity | Prefilled; editable before submit. |
| Concern reason, entries, unknowns, corrections, challenges | **Not copied.** |
| Concern participants | **Not copied, preselected, or suggested.** |
| Concern resolution or freeze details | **Not copied.** |
| Contributor's own knowledge | The Incident creator and participants may **re-record** what they independently know as new, attributed Incident entries. These are new entries with new recorded times, not copies. |

**Possible later option (not recommended for v1):** the acting user explicitly selects **their own** Concern entries to copy. Each becomes a new Incident entry attributed to them, keeping its original category, event time, and source, and labeled "carried from an earlier Concern." Other contributors' entries can **never** be copied, because that would disclose another person's contribution to an audience they did not choose.

## 4. Access and participants

### Principles

- Access is explicit and per-Incident. No one gets access because they are Family, Administrator, subject, Safety Circle, Trusted Contact, operator, related, or were on an earlier Concern.
- Every read and write is rechecked server-side against the Incident's grants and current membership. The client is never the authority.
- Participation grants nothing outside the Incident (`REQ-PRIV-008`, `REQ-SAFE-009`).

### Who can be a participant

**Only currently active adult members of the Incident's Family Group**, selected explicitly by the coordinator. The subject may be selected (see [§ 5](#5-subject-visibility)). The coordinator's access is separate from the participant list.

### Actor and action matrix (recommended)

"Open" means the Incident is Open. All rights require a currently valid grant (membership `Id` + `AccessVersion` captured at grant time, still active).

| Action | Coordinator | Participant | Subject (not selected) | Other active member, incl. Administrator, Safety Circle member, original Concern participants | Former member, pending invitee, other group, operator, Trusted Contact |
|---|---|---|---|---|---|
| Create a separate Incident | Allowed | Allowed | Allowed | Allowed | Denied |
| List / view / timeline | Allowed | Allowed | **Denied, nondisclosing** | **Denied, nondisclosing** | **Denied, nondisclosing** |
| Add finding or unknown (Open) | Allowed | Allowed | Denied | Denied | Denied |
| Record contact attempt (Open) | Allowed | Allowed | Denied | Denied | Denied |
| Post coordinator summary (Open) | Allowed | Denied | Denied | Denied | Denied |
| Correct own entry, append-only (Open) | Allowed | Allowed | Denied | Denied | Denied |
| Challenge another's entry, additive (Open) | Allowed | Allowed | Denied | Denied | Denied |
| Create a task assigned to anyone eligible (Open) | Allowed | Denied | Denied | Denied | Denied |
| Create a task assigned to self (Open) | Allowed | **Allowed** (`D-10`) | Denied | Denied | Denied |
| Complete own assigned task with result (Open) | Allowed | Allowed | Denied | Denied | Denied |
| Cancel a task, with reason (Open) | Allowed (any task) | Own self-created tasks only | Denied | Denied | Denied |
| Add or remove participants (Open) | Allowed, eligibility revalidated | Denied | Denied | Denied | Denied |
| Hand off coordinator role (Open) | Allowed, to a current participant | Denied | Denied | Denied | Denied |
| Resolve (Open) | Allowed, with required resolution content | Denied | Denied | Denied | Denied |
| Reopen | Denied for everyone in v1 | | | | |
| Any write when Frozen or Resolved | Denied | Denied | Denied | Denied | Denied |
| Read when Frozen or Resolved, before expiry | Allowed if still eligible (a departed coordinator is not) | Allowed if still eligible | Denied | Denied | Denied |
| View audit evidence | Denied | Denied | Denied | Denied | Denied (no audit viewer) |

Participants see the subject's ordinary directory display name, the coordinator, and the current participant identities. They need these to assign tasks and avoid duplicate effort. This matches the Concern detail, which shows its participant list.

## 5. Subject visibility

| Option | Description | Transparency | Privacy / safety | Misuse risk | Coordination |
|---|---|---|---|---|---|
| **S1 — No automatic access; explicit selection allowed (Recommended)** | Subject sees nothing unless the coordinator explicitly adds them as a participant. | Lower by default | Protects sensitive discussion (for example, a health or personal crisis) from reaching the subject at a fragile moment. Consistent with the approved Concern rule. | An Incident can be coordinated without the subject knowing. Mitigated by: no location, Check-In, or Sharing unlock; only voluntarily recorded information; finite retention; full audit. | Coordinator adds the subject once contact is restored. |
| S2 — Automatic view-only | Subject can always read. | High | Exposes every contributor's reports and contact attempts to the subject, which may chill honest reporting. | Lower covert-coordination risk | Subject sees work in progress. |
| S3 — Automatic participant | Subject can read and contribute. | High | Same as S2, plus subject entries mixed into coordination. | Lower | Awkward when the subject is the person being sought. |
| S4 — Explicit only, no subject participation ever | Subject can never be added. | Lowest | Strong | Highest | Cannot include the subject after contact is restored. Not useful. |

**Recommendation: S1.** It keeps one rule across Concern and Incident, matches the least-disclosure posture, and allows the coordinator to include the subject deliberately.

**Misuse consideration to surface to the owner.** An Incident could be used to coordinate locating an adult who chose to be unreachable. The product's protections are:

- no access to location, Check-Ins, or Sharing Sessions;
- participants can record only information they independently know;
- no external contact or automation;
- finite retention and audit.

The proposal does not add subject notification, because notifications are out of scope. A future notification proposal should consider telling the subject when an Incident about them is resolved or when they are added.

## 6. Coordinator model

**Recommended: one explicit coordinator, with no role engine.**

- The **creator is the initial coordinator.** There is exactly one coordinator at any time.
- **Handoff:** the current coordinator may hand off to a **current, eligible participant**. Handoff takes effect immediately, with no acceptance step in v1, because notifications are out of scope and a pending acceptance could stall coordination. The former coordinator becomes an ordinary participant and keeps their existing grant. The handoff is an attributed timeline item and an audit event.
- **Creator has no residual authority** after handing off. "Creator" stays as attribution only.
- The coordinator **cannot be removed** by anyone and cannot remove themself without first handing off.
- **No co-coordinators, deputies, or role hierarchy.**

### Coordinator loss and unavailability (`D-06`)

| Situation | Recommended behavior |
|---|---|
| Coordinator loses active Family membership (membership `Id`/`AccessVersion` no longer valid) while Open | **Freeze immediately** at the recorded loss time, with reason `CoordinatorEligibilityLost`. The coordinator loses all access. Remaining eligible participants keep **read-only** access. No automatic transfer. Not Resolved. Continue through a new Incident. This mirrors the approved Concern creator-loss rule. |
| Coordinator is unavailable but still a member (for example, unreachable or ill) | **No takeover in v1.** Participants keep contributing, because the Incident stays Open, but cannot add participants, assign tasks to others, or resolve. If no authorized mutation occurs for the inactivity threshold, the Incident freezes ([§ 7](#7-lifecycle)). Prevention is voluntary handoff. |
| Coordinator rejoins later | No restored access. A Frozen Incident stays Frozen. |
| Coordinator removed as a participant | Not possible; handoff comes first. |

**Alternatives considered for `D-06`:**

- **Option B, coordinator-vacant.** On coordinator loss, the Incident stays Open. Participants may still contribute, but no one may change participants, assign tasks, or resolve. It then freezes through inactivity. This keeps contributions flowing for longer coordination, but leaves an Incident that cannot be resolved.
- **Option C, pre-designated backup coordinator.** The coordinator explicitly names one participant as backup, who becomes coordinator automatically on loss. This is more resilient, but it adds a second role and an automatic authority change.
- **Option D, participant claim.** Any participant may claim a vacant coordinator role. This is an automatic authority grant and is rejected.

**Recommendation: Option A (freeze).** It is the simplest and matches approved Concern behavior. Creating a new Incident is a deliberate, explicit act.

## 7. Lifecycle

**Recommended persisted states:** `Open`, `Frozen`, `Resolved`. These are the same names as Concern, so the UI and code share one vocabulary. None implies danger or safety.

| State | Meaning | Allowed |
|---|---|---|
| `Open` | Coordination is happening. | Everything in the [§ 4](#4-access-and-participants) matrix |
| `Frozen` | Coordination stopped **without** resolution: coordinator lost eligibility, or inactivity. Not safe, not dangerous, not verified. | Read only, for still-eligible coordinator and participants. Retention clock runs from freeze. |
| `Resolved` | The coordinator ended coordination with attributed resolution content ([§ 16](#16-resolution)). Not "confirmed safe." | Read only. Retention clock runs from resolution. |

**Transitions**

- Creation → `Open`
- `Open` → `Resolved`: coordinator action
- `Open` → `Frozen`: coordinator eligibility lost, or inactivity threshold reached
- No other transitions. **No reopen.** `Frozen` and `Resolved` are terminal. `Resolved` never becomes `Frozen`.

**Inactivity freeze (`D-12`):** the recommended threshold is **30 calendar days** without an authorized mutation, the same as Concern. Creation, participant changes, coordinator handoff, entries, corrections, challenges, task create/complete/cancel, and resolution count as activity. Reads and denied attempts do not. The freeze is an attributed system action. It never resolves, creates anything, notifies, or escalates.

- **Alternative:** 60 or 90 days, for longer-lived coordination. The tradeoff is longer exposure of an abandoned Open Incident. Thirty days of *zero* recorded activity is already long for an active coordination. Any single entry resets the clock.

**Expiry:** after the `OD-004` retention period ([§ 19](#19-od-004--incident-history-retention)), the whole Incident becomes unavailable and is removed from active storage. Expiry is not a lifecycle state shown to users. An expired Incident reads exactly like a nonexistent one.

**`REQ-SAFE-008` coordination labels (`D-16`):**

- **Recommended:** defer them in v1. The mapping is:
  - "Coordinating Response" ≈ `Open`;
  - "Resolved" = `Resolved`;
  - "Closed / Archived" ≈ `Frozen`, or post-resolution retention;
  - "Concern Raised" is covered by the separate Concern record;
  - "Contact Re-established" is captured in the resolution and in contact-attempt outcomes;
  - "Escalated Externally" is recorded as an attributed finding or task result (for example, "Called the county non-emergency line, 8:05 PM").
- **Alternative:** a coordinator-set, purely descriptive "coordination stage" label drawn from a subset of `REQ-SAFE-008`. It would have no access or lifecycle effect, and every change would appear in the timeline. This adds UI and copy surface, plus some risk that "Escalated Externally" reads as an official classification.

**No severity, priority, or risk levels** in any state.

## 8. Findings and information model

**Reuse the approved Concern entry model.** Every important Incident entry carries:

| Field | Rule |
|---|---|
| Contributor | The attributed participant or coordinator. The system for system actions. |
| Recorded time | Server time of the write. |
| Event time | When the observed thing happened, if known; otherwise explicitly *Unknown*. Shown separately from recorded time. |
| Category | Exactly one of `Confirmed`, `Reported`, `Assumption`, `Unknown`, `Outdated`. **No numeric scores.** `Confirmed` is reserved for directly verified information with an identified basis. |
| Source | As described by the contributor, or explicitly *unknown source*. Must not disclose a restricted underlying record to others. |
| Known / unknown | `Unknown` items are first-class entries, not blanks. |
| Freshness | Shown from event and recorded time. Time passing never changes a category automatically. `Outdated` is applied by an appended attributed entry that keeps the original category, as in Concern. |
| Correction relationship | Optional link to an earlier entry (correction, challenge, task result). |

**Recommended entry kinds** (one entry model, typed):

| Kind | Who | Notes |
|---|---|---|
| `Observation` (finding) | Coordinator, participants | Same as Concern. |
| `Unknown` | Coordinator, participants | Same as Concern. |
| `ContactAttempt` | Coordinator, participants | Extra structured fields ([§ 9](#9-contact-attempts)). |
| `Summary` | Coordinator only | The "current summary" from `REQ-SAFE-007`. Append-only: the latest is shown as current, with its time. Earlier summaries remain in history. The creation reason is the first summary. |
| `TaskResult` | Task assignee | Created when a task is completed. Linked to the task. Has a category and source like any finding. |
| `Correction` | Original contributor only | Append-only. Linked to the corrected entry. |
| `Challenge` | Any other contributor | Additive. Linked to the challenged entry. |

**Separate aggregate, shared vocabulary.** Incidents should be implemented as a separate aggregate with the same rules, sharing the category enum and provenance validation where that is a clean extraction. Do not generalize the Concern tables into one "safety record" engine. The two have different authority, lifecycle, tasks, and retention, and the approved Concern implementation should not be destabilized.

**Protected-information boundary** (same as `REQ-PRIV-025`): Incident records copy and link no profile-private field, contact detail, Event, Plan, Trip, location, Check-In, Sharing Session, Safety Circle, or Trusted Contact record. A contributor may manually report what they independently know, as their own attributed entry.

## 9. Contact attempts

**Product meaning:** an attributed **record** that someone tried to reach a person or organization, and what happened. It is coordination memory that reduces duplicate effort (`REQ-SAFE-011`). **The app makes no calls, sends nothing, and integrates with no telephony, SMS, or email.**

| Field | Rule |
|---|---|
| Attempted by | The contributor (default), or another named person as reported by the contributor. If another person, category is normally `Reported`. |
| Target | A short **text label** such as "Jordan," "Hotel front desk," or "Taylor (travel companion)." **Not** a phone number or email field. Never auto-filled from Profile contact fields or Trusted Contact entries. |
| Method | Small fixed set: `Call`, `Text message`, `Email`, `In-app message`, `In person`, `Other`. |
| Attempted at | Event time, or *Unknown*. |
| Outcome | Small fixed set: `Reached`, `No answer`, `Left message`, `Sent — no reply yet`, `Other`, `Unknown`. `Reached` does **not** resolve the Incident or set any state. |
| Notes | Optional free text. |
| Category, source, recorded time | As for every entry. |

**Privacy boundary.** Contact details are not stored as structured data and are not shown from other domains. The UI should advise contributors not to paste other people's private phone numbers or addresses into free text. The server does not attempt detection.

**Corrections.** Append-only, by the original contributor. Others may challenge.

## 10. Tasks

**Minimal model** (`REQ-SAFE-012`):

| Field | Rule |
|---|---|
| Description | Required short text. **Immutable** after creation. To change it, cancel and create a new task. |
| Assignee (owner) | Exactly one: the coordinator or a current participant, eligible at assignment. |
| Status | `Open`, `Done`, `Cancelled`. No "in progress" state in v1. |
| Created by / at | Attributed. |
| Completed at / result | On `Done`: completion time plus a required `TaskResult` entry with category and source. |
| Cancelled at / by / reason | On `Cancelled`: short required reason. |

**Authority (`D-10`, recommended):**

- The coordinator creates tasks for any eligible person, and cancels any task.
- A participant may create a task **assigned to themself** (for example, "I'll call the hotel"), and may cancel that self-created task with a reason. This directly reduces duplicate effort.
- Only the assignee completes a task. The coordinator cannot complete on someone's behalf. If needed, the coordinator cancels with a reason and records a finding.
- **No reassignment in v1.** Cancel and recreate. This keeps task history to three timestamps and avoids a task-change log.
- If an assignee loses access, the task stays as-is and shows its assignee. The coordinator may cancel it.
- At Incident resolution or freeze, Open tasks become read-only and display as "Not completed." Their stored status is not rewritten.

**Explicitly not included:** due dates, priorities, checklists, comments, subtasks, notifications, cross-Incident task lists, and "My Tasks" on Home. Home task surfacing (`REQ-UX-008`) is future UI work.

## 11. Timeline

**Recommended: explicit domain records plus a read-time chronological projection. No event sourcing.**

The timeline is computed on read from the Incident's own records:

| Timeline item | Source record |
|---|---|
| Incident created | Incident |
| Summary, finding, unknown, contact attempt, task result | Entries |
| Correction / challenge | Entries (with link) |
| Task created / completed / cancelled | Task timestamps |
| Participant added / removed | Participant grant rows (added/removed times) |
| Coordinator handed off | Coordinator-change rows |
| Frozen (with reason) / Resolved | Incident lifecycle fields |

**Ordering (`D-18`, recommended):** order by **event time when known, otherwise recorded time**. Every item shows both times, and items without a known event time are labeled as such. This best fits `REQ-SAFE-013`'s "chronological reconstruction." The alternative is pure recorded-time order: simpler, but a late-reported 6:00 PM call would appear after 9:00 PM items.

Each item keeps its confidence category. Corrections are shown next to the corrected item, and the original is not hidden. The timeline is not a separate store, so it cannot drift from the records it summarizes, and it expires with them.

## 12. Corrections and history

| Information | Rule |
|---|---|
| Findings, unknowns, summaries, contact attempts, task results | **Append-only.** The original contributor corrects their own entry with an attributed, linked `Correction` that gives an explanation. Others add a separate `Challenge`. Nothing meaningful is overwritten or deleted. `Outdated` is an appended marker. |
| Coordinator summary | New summary entries supersede the old as "current," and the old remain in history. |
| Task description | Immutable. Cancel and recreate. |
| Task status | Forward-only: `Open` → `Done` or `Cancelled`. No undo. Errors are corrected through the linked `TaskResult` entry. |
| Participant list | Grant rows record added and removed times. No rewriting. |
| Coordinator | Coordinator-change rows. No rewriting. |
| Resolution | Final. No correction after resolution in v1, because a Resolved Incident accepts no writes. Important corrections made *before* resolution are shown in the resolved view from history. |

Append-only behavior is **not** required for harmless presentation state such as UI filters or collapsed sections. No such state is persisted server-side.

## 13. Location, Check-In, and Sharing boundary

- Incident participation, coordination, or subject status grants **no** access to location, Check-Ins, Sharing Sessions, background tracking, live maps, or movement history. `REQ-PRIV-008`, `REQ-PRIV-010`, and `REQ-SHARE-009` stay controlling.
- An Incident does not start, extend, widen, or change any Sharing Session, and does not request location.
- `REQ-SAFE-007`'s "authorized last shared location" is **deferred**. It is not a field in v1, and it may only be designed after Sharing exists and a separate rule defines it.
- A participant who independently saw shared information may record it as their own attributed finding, with source and category, for example "Jordan's 5:42 PM check-in said 'Arrived' (Reported, from my Family view)." That text is a **manual report, not a link**. The Incident stores no Check-In ID, coordinates, or map.
- **Re-disclosure caution:** manually reposting someone else's voluntarily shared location to the Incident audience widens that audience. This is already permitted for Concern under `REQ-PRIV-025`. The proposal keeps the same rule, and recommends that the UI remind contributors to share only what is needed and to avoid precise addresses. This proposal does **not** resolve `OD-002`.

## 14. Safety Circle, Trusted Contacts, and other actors

| Actor | Recommended v1 treatment |
|---|---|
| Safety Circle (unbuilt) | **No automatic access, no special eligibility, no notification role.** A Safety Circle member who is an active adult in the group may be selected like anyone else. A future rule could make Safety Circle members *suggested* participants or *notification-eligible* (`REQ-NOTIFY-002`). Not proposed now. |
| Trusted Contacts (unbuilt) | **No access and not eligible as participants** in v1. `REQ-PRIV-024` already forbids access by designation. People outside the Family appear only as text targets in contact attempts. |
| Family Administrator | No added authority (`REQ-PRIV-006`). |
| Application Operator | No product access (`REQ-PRIV-012`). Operational audit access needs a separate policy. |
| Non-Family people, external responders (police, EMS, hotel staff) | Never participants and no access. They are recorded only as text in contact attempts, findings, or task results. |
| Former members, pending invitees, other Family Groups | Denied, nondisclosing. |

## 15. Notifications and external escalation

**Not in scope, and not authorized.** The Incident slice has:

- no automatic Incident creation;
- no automated danger determination, risk scoring, or severity;
- no emergency dispatch;
- no automatic police, EMS, or fire contact;
- no SMS, email, push, or in-app notification delivery;
- no outbound integration with any external service.

Participants discover an Incident by an authorized in-app read, as with Concerns. Contacting outside assistance is a human action outside the app, recorded afterward as a finding or task result.

**Future work, separately proposed:** restrained Incident notifications (task assignment, participant added, major update, resolution) under `REQ-NOTIFY-001` through `003`, with notification eligibility kept separate from disclosure (PRP-003 principle 6). Optional subject notification is part of that future work.

## 16. Resolution

**Meaning:** coordination in this Incident has ended. It does **not** mean confirmed safe, danger ended, emergency resolved, or facts verified. The UI should say "Coordination ended," not "Safe."

| Element | Rule (`REQ-SAFE-014`) |
|---|---|
| Who | **Coordinator only**, while Open. |
| Required | **Resolution summary** (attributed text: why coordination is ending). **Outcome statement** (attributed text) with a **confidence category**; `Unknown` is allowed, for example "Coordination ended; outcome not confirmed." **Contact restored / resolution occurred at**: a time, or explicit *Unknown*. |
| Optional | Assistance provided; unresolved follow-up; lessons. All plain text. |
| Important corrections | Not re-entered. The resolved view lists the corrections and challenges already in history. |
| System | Resolved-by and resolved-at. |
| After | Read-only for still-eligible audience. Open tasks display as "Not completed." Retention clock starts. |
| Reopen | **Not allowed in v1.** A later situation uses a new Incident. This is cleaner for retention, audit, and audience. |

There are no preset outcome choices such as "Safe" or "Found," which would read as verdicts.

## 17. Nondisclosing access

Reuse the WI-008-001 behavior exactly. Each of the following returns the **same unavailable response** (the existing API `404` pattern), and the list **omits** the Incident:

- a nonexistent Incident ID;
- an existing Incident the caller has no grant for, including the unselected subject, an Administrator, and the original Concern's participants;
- an expired Incident, whether or not cleanup has run;
- a removed participant;
- a caller who lost membership;
- a stale grant: same profile, but a different membership `Id` or `AccessVersion`;
- an unauthorized nested entry or task ID.

Other rules:

- Commands use the same result.
- Validation errors are returned only after caller authority and target visibility are established.
- Create with an ineligible subject or participant returns an unavailable result without revealing private subject data.
- Authentication failure keeps the existing `401`.

## 18. Membership loss and rejoin

Reuse the Concern membership `Id` + `AccessVersion` grant principle. Do not add another epoch framework.

| Event | Behavior |
|---|---|
| Participant loses Family membership | Immediate denial of all reads and writes. Stale in-flight writes are rejected at commit. Their entries stay attributed for the remaining audience until expiry. Their Open tasks stay visible with their name, and the coordinator may cancel. |
| Participant removed by coordinator | Same as above. A timeline item and audit event are recorded. |
| Participant rejoins later | **No automatic restoration.** While Open, the coordinator may explicitly re-add them, which creates a new grant on the new membership `Id`/`AccessVersion`. |
| Coordinator loses membership | Freeze ([§ 6](#6-coordinator-model)). No transfer. No restoration on rejoin. |
| **Subject** loses membership | **No effect on the Incident** (recommended, `D-02`). It stays Open under its coordinator, and the subject's ordinary display name as recorded continues to show. If the subject was a participant, they lose access like any participant. Rationale: coordination may matter most when someone has left the group, and Incident access comes from grants, not subject status. This matches the implemented Concern behavior. |
| Subject rejoins | Nothing changes automatically. |
| Membership-end races | Serialize or otherwise protect membership-end and Incident writes so no write succeeds after revocation commits. Same requirement as WI-008-001. |

## 19. OD-004 — Incident-history retention

### Repository review

| Source | Finding |
|---|---|
| `OD-004` | Open. "A policy is required before production use; old sensitive information must not remain indefinitely merely because it was once useful." |
| `REQ-PRIV-009` | Stale must not appear current. No indefinite retention merely for usefulness. |
| `REQ-SAFE-004`, `REQ-SAFE-014` | Meaningful history is preserved rather than silently erased. The Incident "transitions to resolution rather than simply disappearing." Lessons are part of resolution. This argues for a meaningful, but finite, post-resolution window. |
| `REQ-PRIV-025` / `DEC-068` | Concern: 30-day inactivity freeze; 180-day Resolved/Frozen history; 180-day audit; 30-day backup aging; restore must not revive expired data; application defaults, not legal advice; no legal-hold engine. |
| `REQ-PRIV-018` | 30 days for Shared Family Dates only. Must not be copied by analogy. |
| PRP-003 | Safety audit retention subject to `OD-004`. |
| `DEC-069` | Denied-attempt hourly summaries, no target IDs, retained on the audit clock. |
| WI-008-001 / VER | In-process reconciliation sweep and request-time expiry guards proven. Backup-aging and restore operations evidence is still a **predeployment dependency**. |
| Legal / public records | **No requirement found.** This is a private family application, not a records system. |

### Data classes

| Class | Recommended treatment |
|---|---|
| **Open Incident** | Retained while Open. Never deleted or resolved because of age alone. Bounded by the inactivity freeze (`D-12`). |
| **Frozen Incident history** | Read-only for still-eligible audience. Deleted at the end of the retention period, counted from the freeze time. |
| **Resolved Incident history** | Read-only for still-eligible audience. Deleted at the end of the retention period, counted from the resolution time. |
| **Deletion scope at expiry** | The whole aggregate: Incident, participant grants and history, coordinator changes, all entries (findings, unknowns, summaries, contact attempts, task results, corrections, challenges), tasks, provenance, and freeze and resolution content. Nothing detached survives through a secondary path. |
| **Audit evidence** | Metadata only, on its own per-event clock ([§ 20](#20-audit)). |
| **Backups** | Age out within a finite window after active deletion. |
| **Restore** | Must not make expired Incident data available. Apply request-time guards immediately and rerun reconciliation on startup or after restore (the Concern invariant). |
| **Related Concern** | Unaffected. Independent clocks; no link to break. |

### Product-history options (Resolved and Frozen)

These durations are **product-policy options only**. They are not legal requirements or existing rules.

| Option | Duration | Privacy / minimization | Family usefulness | Sensitive exposure | Operational complexity |
|---|---:|---|---|---|---|
| **A — Shorter** | 180 days | Strongest. Identical to Concern. | Covers follow-up after the immediate situation. Lessons may be gone before the next similar season or trip. | Lowest | **Lowest**: one constant shared with Concern, so families learn one rule. |
| **B — Moderate (Recommended)** | 365 days | Still clearly finite | Covers a full annual cycle (holidays, recurring trips), so "what we learned last time" is still available. Reflects the canon's emphasis on preserved history and lessons. | Moderate: a year of reports about a person's crisis | Low: same mechanism, different constant. |
| **C — Longer** | 2 years (730 days) | Weakest of the three | Longest reference value | Highest: long-lived records about real crises, and more risk if accounts or backups are compromised | Low mechanically, but higher governance burden |

**Recommendation: Option B, 365 days**, for both Resolved (from resolution) and Frozen (from freeze). An Incident is rarer and more consequential than a Concern, and its resolution explicitly includes lessons. One year preserves that value while staying finite. If the owner prefers a single rule across Safety records, Option A is the clean alternative.

No per-Family override, no indefinite retention, and no user-triggered early deletion in v1. Early deletion would be a separate privacy feature.

### Audit retention options

| Option | Duration | Tradeoff |
|---|---:|---|
| Shorter | 180 days | Matches Concern audit, but would expire evidence before the Incident content it describes (under Option B). |
| **Same as product history (Recommended)** | 365 days per event, if Option B | Same principle the owner chose for Concern: audit lifetime equals product lifetime. Simple to explain. |
| Longer | 2 years | Keeps actor-to-Incident activity after content is gone. No accountability need for this has been identified. |

Denied-attempt summaries follow the `DEC-069` pattern (actor, broad read/write category, UTC hour; no target ID) on the same Incident audit clock.

### Backup aging and restore (`D-15`)

**Recommended:** the same as Concern. Backups age out **within 30 days** after active deletion, and the deployment documentation describes the mechanism. On restore, request-time expiry guards apply immediately and reconciliation reruns. No legal-hold engine.

This remains a **predeployment operational dependency**, shared with Concern. It is not a blocker to implementation planning.

### Legal caveat

These are application defaults. They are not legal, records-management, or regulatory advice. If a future deployment carries legal, contractual, or regulatory obligations, it requires qualified review before deployment. That review does not silently change these defaults.

## 20. Audit

**Reuse the Concern audit pattern. Store metadata only.**

| Event | Metadata (no content) |
|---|---|
| Incident created | Incident ID, actor, timestamp, result |
| Participant added or removed | Plus the affected participant ID |
| Coordinator handed off | Plus the from and to participant IDs |
| Entry created (by kind: finding, unknown, summary, contact attempt, task result, correction, challenge) | Plus entry ID and kind |
| Task created, completed, or cancelled | Plus task ID and assignee ID |
| Frozen | System actor, reason code |
| Resolved | Actor |
| Access revocation (membership loss detected) | Affected person ID |
| Denied sensitive read or write | `DEC-069` hourly summary: actor, `IncidentReadDenied` / `IncidentWriteDenied`, bucket, first and last time, count. **No Incident ID.** |

- Successful reads are not audited, matching Concern.
- Audit never copies entry text, contact-attempt targets or notes, task descriptions, resolution text, or protected source details.
- Audit rows have no cascading foreign key to the Incident, so they survive product deletion until their own expiry.
- **No audit viewer** and no role-based audit access. Any operational audit access needs a separate approved policy.

## 21. Minimum domain and data concepts (not a schema)

| Concept | Purpose |
|---|---|
| `SafetyIncident` | Aggregate root: Family Group, subject profile, creator (attribution), current coordinator and its grant reference, state, created / last-authorized-mutation / frozen (time + reason) / resolved timestamps, resolution content, concurrency token. Expiry is derived from frozen or resolved time. |
| `IncidentParticipantGrant` | Profile plus captured membership `Id` and `AccessVersion`; added and removed times; added by. One effective grant per person. The coordinator also has a grant. |
| `IncidentCoordinatorChange` | From, to, at, by. Small and append-only. |
| `IncidentEntry` | One typed entry model: `Observation`, `Unknown`, `Summary`, `ContactAttempt`, `TaskResult`, `Correction`, `Challenge`. Common provenance fields, nullable contact-attempt fields, and an optional related-entry or task link. |
| `IncidentTask` | Description, assignee, created by/at, status, completed at, cancelled at/by/reason, result entry link. |
| `IncidentAuditEvent` and denied summary | As in [§ 20](#20-audit). |

Resolution content can live on `SafetyIncident`, as it does for Concern, or in a 1:1 record if the field count makes that clearer. That is an implementation choice.

**Not proposed:** a generic workflow or state engine; a roles or permissions framework; event sourcing; a generic task system; a shared "SafetyRecord" base table merging Concern and Incident; a notification platform; a retention or legal-hold engine.

## 22. UI direction (for a later story; no UI in this step)

- Follow the approved `DEC-070` system: blue palette, sidebar and slim top bar, card and list language, calm status badges.
- **Entry points:** the existing Concerns screen and the Home Concern card may present Incidents alongside Concerns (for example, a "Safety coordination" area with Concern and Incident lists). **Do not add a navigation destination.** That would touch `OD-001`.
- Use a separate component and stylesheet for Incidents. `concerns.scss` (≈ 7.53 kB) has little budget headroom, and budgets must not be raised.

**Reference mockup triage** (`safety-coordination.png` panels 2–4 and `panel4-safety-aware.png`), at a high level:

- **KEEP**
  - timeline card;
  - tasks card with owner and status;
  - participants card;
  - resolution summary and timeline;
  - category legend.
- **ADAPT**
  - status pill → calm `Open` / `Frozen` / `Resolved`;
  - "Incident Status" dropdown → read-only state (or a `D-16` label if approved);
  - "Key Takeaways" → optional Lessons;
  - "Preserved Incident History" → a retention notice with the expiry date;
  - "Information at a Glance" category counts → optional.
- **DROP**
  - map and "Last Known Information" location card;
  - View on Map;
  - Share;
  - Export;
  - More Actions;
  - "Convert to Incident" as a one-step conversion;
  - "Safety team notified";
  - red "Missed" / "Now 2h 30m late" urgency;
  - subject age, school, or photo details;
  - call and message buttons on people;
  - Companions "Manage";
  - Trip, Hotel, and Travel panels pulled from other domains;
  - "Jordan is safe and accounted for" as a system verdict headline (it may only appear as the coordinator's attributed outcome text);
  - "+ Add" for non-Family people.

## 23. Representative scenarios

Assume active adults A, B, C, D in Family Group F, and Concern K created earlier by A.

| # | Scenario | Result under the recommendations |
|---|---|---|
| 1 | A manually creates an Incident about B | Allowed: A and B are active adults in F. Required reason recorded as first `Summary`. A is coordinator. State `Open`. Audit: created. No notification. |
| 2 | A explicitly selects C as participant | C's grant captures C's membership `Id`/`AccessVersion`. Timeline: "C added by A." Audit: participant added. |
| 3 | B is not selected | B's list omits the Incident, and B's direct request returns the uniform unavailable result. B's subject status grants nothing. D (an Administrator) gets the same result. |
| 4 | C adds a finding | `Observation`, `Reported`, source "B's roommate by phone," event time 6:30 PM, recorded 8:10 PM. Shown with both times and category. Not a danger finding. |
| 5 | C records a contact attempt | `ContactAttempt`: by C, target "B," method `Call`, at 8:15 PM, outcome `No answer`, category `Confirmed` (C's own action). No call is placed by the app. No phone number stored. |
| 6 | A creates and assigns a task to C | "Call the hotel front desk," assignee C, `Open`. Timeline: task created. Audit: task created with assignee ID. |
| 7 | C completes the task | C records the result "Hotel confirms B checked in at 7:50 PM" (`Reported`, source "Hotel front desk by phone"). Task `Done` with completion time. Timeline shows both. |
| 8 | A corrects or challenges | A's own earlier summary said "last contact 5 PM." A appends a `Correction` ("last contact was 4:30 PM, per my call log") linked to the original, which stays visible. For C's entry, A can only add a `Challenge`. A cannot edit C's text. |
| 9 | C loses Family membership | C's next read or write is denied, and an in-flight write is rejected at commit. C's entries stay attributed to C for A and others. Audit: access revocation. |
| 10 | C rejoins F | C's new membership does not match the stored grant, so access is still denied. Only if A explicitly re-adds C while Open does C get a new grant. |
| 11 | Coordinator A loses eligibility | The Incident freezes at the recorded loss time (`CoordinatorEligibilityLost`). A is denied. Remaining participants have read-only access. No transfer and no resolution. A new Incident is needed to continue. *(Under `D-06` Option B it would remain Open without a coordinator.)* |
| 12 | Resolution | Coordinator records summary "Coordination ended: B reached by phone and is at the hotel," outcome category `Confirmed` (direct call), contact restored at 9:48 PM, optional lessons. State `Resolved`. Read-only. Retention clock starts (365 days if Option B). No reopen. |
| 13 | Unauthorized request | D, a former member, another group's member, or an operator requests the Incident ID: same unavailable response as a nonexistent ID. Hourly denied summary records no Incident ID. |
| 14 | Incident started from Concern K | A uses **Start an Incident** on K. The form opens with B prefilled, and nothing else. A submits. A new Incident exists. K is unchanged and still `Open`. A may separately resolve K with a note. No link is stored. |
| 15 | No Concern access transfers | K's participants (say C and D) are not preselected or suggested and get no Incident access. A must select them explicitly. K's entries are not copied. A, C, and D may re-record what they know as new Incident entries. |
| 16 | Inactivity | No authorized mutation for 30 days. Effective state becomes `Frozen` (`Inactivity`) at the deadline, recorded once as a system action. Not resolved, not a danger finding, no escalation. |
| 17 | Subject B leaves F | The Incident continues under its coordinator, with B's display name as recorded. If B had been a participant, B loses access. |

## 24. Phase 8 exit-criteria mapping

| # | Criterion (`DEC-067`) | Status after Concern | How this proposal enables the Incident portion |
|---|---|---|---|
| 1 | Approved and verified Concern workflow | **Satisfied** (WI-008-001, WI-008-002) | — |
| 2 | Approved and verified Incident workflow | **Not satisfied** | Defines the approvable rule set. Approval → canonical update → server and UI work items → verification. |
| 3 | Provenance and uncertainty preserved | Satisfied for Concern | § 8, § 9, § 12, § 16: same categories and provenance; `Unknown` outcomes allowed. |
| 4 | Explicit authorization boundaries | Satisfied for Concern | § 4 matrix and § 14. |
| 5 | Denied-access paths tested | Satisfied for Concern | § 17 enumerates the denial cases for tests. |
| 6 | Correction and history behavior verified | Satisfied for Concern | § 12. |
| 7 | Resolution behavior verified | Satisfied for Concern | § 16. |
| 8 | Privacy boundaries preserved | Satisfied for Concern | § 3, § 13, § 14. |
| 9 | No automatic privacy override | Satisfied for Concern | No role-, subject-, or Concern-based access; no location unlock. |
| 10 | No automatic danger determination | Satisfied for Concern | § 1, § 7, § 16. |
| 11 | No automatic emergency dispatch | Satisfied for Concern | § 15. |
| 12 | End-to-end verification | Satisfied for Concern (isolated Playwright run) | A future Incident journey. Full-suite database isolation should be fixed before or alongside it. |
| 13 | Relevant accessibility verification | Satisfied for Concern | A future Incident UI story. |

**No change to the approved exit criteria is needed.** No contradiction was found. Criterion 2 is the only unsatisfied criterion. Criteria 3–13 must also be demonstrated for the Incident once it is implemented.

## 25. Explicit exclusions

- implementation code, migrations, schema, API;
- Incident UI;
- an implementation story;
- automatic Incident creation or escalation;
- Concern-to-Incident conversion that copies data or access;
- emergency dispatch or police, EMS, or fire contact;
- live location, maps, covert tracking, background tracking, movement history;
- Check-In or Sharing Session access or changes;
- automatic risk, danger, or severity scoring;
- a notification platform, or any SMS, email, or push delivery;
- external-service integrations;
- a legal-hold system;
- a generic workflow or state engine;
- a generalized roles framework;
- generic task management;
- event sourcing;
- an audit viewer;
- export or sharing of Incidents;
- attachments (deferred by `REQ-SAFE-007`);
- Trip, Event, or Plan linkage;
- Safety Circle or Trusted Contact management or access;
- non-Family participants;
- minors and guardians (`OD-003`);
- a new navigation destination (`OD-001`);
- resolving `OD-002`.

## 26. Verification plan

**Before approval:** map each chosen decision to its canonical anchor, and confirm that no rule grants profile, location, Check-In, Sharing, Event, Plan, Trip, Safety Circle, or Trusted Contact access by implication.

**After approval and canonical update,** a server work item should test:

- creation eligibility;
- explicit grants;
- no implicit authority (Administrator, subject, Safety Circle, Concern participant);
- the nondisclosing denial set in § 17;
- stale-grant and rejoin behavior;
- coordinator handoff and coordinator loss;
- append-only entries;
- contact-attempt and task authority;
- task lifecycle;
- timeline ordering;
- resolution content;
- terminal states;
- inactivity and retention boundaries with injected time;
- expired reads before cleanup;
- whole-aggregate deletion;
- independent audit clocks;
- denied-summary aggregation;
- the restore and overdue guard;
- no Concern side effects;
- no danger, notification, or escalation side effects.

A UI work item should add rendered desktop and narrow review, focused accessibility checks, and an Incident Playwright journey. Label verification `SELF-VERIFIED` where independent review is unavailable.

## 27. Owner decisions required

*Historical decision packet. All decisions were answered under `DEC-071`; see "Owner-approved decisions" at the top.*

Reply by decision ID. **Recommended** is the proposal's default. Decisions marked ★ are retention decisions that resolve `OD-004`.

| ID | Decision | Recommended | Alternatives |
|---|---|---|---|
| `D-01` | Incident creator eligibility | Any active adult member, about an active adult in the same group; Administrator adds nothing | Restrict to a narrower group (not recommended; Safety Circle is unbuilt) |
| `D-02` | Subject rules | Exactly one active adult subject; self-Incident allowed; subject membership loss does not change the Incident | Disallow self-Incident; allow multiple subjects; freeze on subject loss |
| `D-03` | Subject visibility | **S1:** no automatic access; coordinator may explicitly add the subject | S2 auto view-only; S3 auto participant |
| `D-04` | Participant powers | § 4 matrix: view, findings, unknowns, contact attempts, own corrections, challenges, self-assigned tasks, completing own tasks | Narrower (no self-created tasks) or broader (participants manage participants) |
| `D-05` | Coordinator model | One coordinator; creator first; immediate handoff to a current participant; no acceptance step; creator keeps no residual authority | Handoff requiring acceptance; co-coordinators |
| `D-06` | Coordinator-loss behavior | **A:** freeze, no transfer | B: coordinator-vacant Open; C: pre-designated backup |
| `D-07` | Concern → Incident entry | Manual **Start an Incident** shortcut for the Concern creator only; Concern unchanged; direct creation always available | Any Concern participant may use it; no shortcut at all |
| `D-08` | Concern / Incident linkage | No stored link in either direction | One-way, non-navigable "Started from a Concern" marker on the Incident |
| `D-09` | Information transfer | Subject only; nothing copied; no participants preselected or suggested | Later: user copies their **own** selected entries, attributed and labeled |
| `D-10` | Task authority | Coordinator creates, assigns, and cancels any task; participant creates self-assigned tasks; only assignee completes; no reassignment | Coordinator-only creation; coordinator may complete on behalf |
| `D-11` | Resolution authority and content | Coordinator only; required summary, outcome with category, and contact-restored time (or Unknown); optional assistance, follow-up, lessons; no reopen | Allow reopen; allow any participant to resolve |
| `D-12` | Inactivity freeze threshold | 30 days without authorized mutation (same as Concern) | 60 or 90 days |
| `D-13` ★ | Resolved and Frozen Incident history retention | **B: 365 days** from resolution or freeze | A: 180 days; C: 2 years |
| `D-14` ★ | Incident audit retention | Same as product history (365 days per event if B); `DEC-069` denied summaries on the same clock | 180 days; longer |
| `D-15` ★ | Backup aging and restore | Within 30 days of active deletion; restore reapplies expiry; deployment docs describe the mechanism; no legal hold | Different finite window |
| `D-16` | `REQ-SAFE-008` coordination labels | Defer in v1 (lifecycle `Open` / `Frozen` / `Resolved` only) | Coordinator-set descriptive stage label, no access effect |
| `D-17` | Safety Circle and Trusted Contacts | No automatic access, no special eligibility, no notification role in v1; Trusted Contacts not eligible | Safety Circle as suggested participants (needs Safety Circle first) |
| `D-18` | Timeline order | Event time when known, otherwise recorded time; both always shown | Recorded time only |
| `D-19` | Contact-attempt fields | Text target label; fixed method and outcome sets; no structured contact details | Free-form only |

## 28. Genuine blockers to first Incident implementation planning

*Status after `DEC-071`: items 1–3 are complete. Decisions were answered, `OD-004` is resolved, and `CTX-1.26` is active after `VER-CTX-1.26`. The tracked non-blockers below are unchanged.*

1. **Owner decisions `D-01` through `D-19`**, at minimum `D-01`–`D-15`. `D-16` through `D-19` can take the recommended defaults if the owner accepts them.
2. **`OD-004` resolution** through `D-13`, `D-14`, and `D-15`. Incident stores sensitive history from its first write, so retention must be decided before a server story, as it was for Concern.
3. **Canonical update and new baseline (expected `CTX-1.26`):**
   - new Incident privacy and access requirement (expected `REQ-PRIV-026`);
   - new Incident lifecycle requirement (expected `REQ-SAFE-016`);
   - clarification of `REQ-PROD-012` for the manual shortcut;
   - a note on `REQ-SAFE-008` scoping;
   - `OD-004` marked resolved;
   - requirements index update;
   - a `VER-CTX` record.
   Stories reference canonical IDs, not proposals.

**Not blockers to planning,** but tracked:

- backup-aging and restore operations evidence (predeployment, shared with Concern);
- Playwright full-suite database isolation (fix before or within the Incident UI story);
- `concerns.scss` budget headroom (use a separate Incident stylesheet).

## 29. Recommended path after approval (non-authorizing)

Mirror the Concern delivery split:

1. canonical update to `CTX-1.26`;
2. **WI-008-003 — Incident domain, access, and retention foundation**: server, persistence including migration, API, audit, reconciliation, tests;
3. **WI-008-004 — Incident MVP user interface**, including the Concern **Start an Incident** shortcut if `D-07` is approved.

Each needs its own story and explicit execution authorization. This proposal creates neither.

## Decision and authorization

- **Decision:** `APPROVED`, with the `D-08` lineage refinement.
- **Decision ID:** `DEC-071`. It approves PRP-029 and resolves `OD-004`. Canonical context is active at `CTX-1.26`.
- **Approved by:** Project owner
- **Approved at:** 2026-10-01
- **Execution authorized:** `NO`. `DEC-071` authorizes the decision record, canonical update, baseline, and verification only. No implementation story (`WI-008-003` is not created), code, schema, migration, or UI is authorized. `OD-001` and `OD-002` remain open; `OD-003` remains outside MVP.
