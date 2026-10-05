---
Status: APPROVED
Authority: CANONICAL
Context-Baseline: CTX-1.34
Version: 1.18
Source: ../bootstrap/product-ux-source.md; DEC-016; DEC-018; DEC-028; DEC-029; DEC-030; DEC-031; DEC-034; DEC-038; DEC-040; DEC-042; DEC-045; DEC-046; DEC-047; DEC-053; DEC-059; DEC-068; DEC-071; DEC-088
---

# Product Contract

This contract includes the narrow manual Trusted Contact clarification under `CTX-1.34` / `DEC-088`. Shared Family Dates v1 use a required title, description, and one specific calendar date; they do not recur automatically. Trusted Contacts remain owner-managed and private by default. Minor and guardian functionality is outside the MVP and reserved for future work under `DEC-053`; this does not define that future behavior.

## Identity and purpose

### REQ-PROD-001

The product is a private family coordination application for a large extended family. It must feel like a family application with strong safety capabilities and must not be positioned primarily as a location or tracking application.

### REQ-PROD-002

The product philosophy is **Connect, Coordinate, Care**:

- **Connect:** Family members, events, important dates, and useful family information.
- **Coordinate:** Events, trips, lightweight plans, responsibilities, food, supplies, transportation, accommodations, and check-ins.
- **Care:** Voluntary location sharing, expected arrivals, Safety Circles, Concerns, and coordinated family safety incidents.

### REQ-PROD-003

The product must help family members assemble and coordinate fragmented information when someone is unexpectedly unavailable or unreachable, including last confirmed status, expected activity and arrival, companions and contacts, contact attempts, actions taken, response coordination, and remaining unknowns.

### REQ-PROD-004

The product must preserve the difference between confirmed, reported, assumed, unknown, and outdated information rather than presenting all safety information as equally certain.

### REQ-PROD-005

The product must support geographically distributed and large families, reduce information overload, respect individual autonomy, and remain useful without continuous or precise location sharing.

### REQ-PROD-006

Everyday family usefulness is part of the safety strategy: normal coordination keeps relevant family, event, plan, trip, contact, and check-in information current.

## Product areas

### REQ-PROD-007

The conceptual product areas are Home, Family, Events, Plans & Trips, Safety, and Me / Settings. Location is contextual within Sharing Sessions, events, plans, trips, check-ins, and safety workflows; it is not a top-level product area.

## MVP capability boundary

### REQ-PROD-008

MVP includes these product capabilities:

- **Family:** Profiles, relationships, household or basic grouping, contacts, privacy preferences, and Safety Circle.
- **Home:** Needs My Attention, upcoming events, shared or current trips, relevant updates, user tasks, and appropriate safety-concern visibility.
- **Events:** Creation, invitations, RSVP, organizer, date/time/location, updates, simple tasks, food or supply needs, and claiming needs.
- **Plans & Trips:** Lightweight plans, structured trips, travelers, destinations, accommodations, transportation notes, companions, expected arrival, and check-ins.
- **Sharing:** Check-in sharing, temporary Sharing Sessions, periodic sharing concept, temporary real-time sharing, audience selection, expiration, stopping sharing, and event/trip/plan context.
- **Safety:** Safety Circle, Concern, Safety Incident, coordinator, participants, information confidence, unknowns, contact attempts, tasks, timeline, and resolution.
- **Notifications:** Event, task, check-in, trip, Concern, incident, and resolution notifications.

Detailed behavior is governed by the relevant contracts and workflow files.

### REQ-PROD-013

Minor and guardian functionality is outside the MVP and reserved for future work. No minor profile, guardian authority, minor invitation, or related behavior is approved for the MVP. Future work requires a separate product, privacy, and safety review before requirements or implementation are approved. `OD-003` remains open for that future definition.

### REQ-FAM-001

The Family area must support member profiles, family relationships, household or basic grouping, contact information, privacy settings, and Safety Circle in MVP.

### REQ-FAM-002

The Family area may present shared birthdays or important dates and trusted contacts. Information appears only when shared under the applicable privacy rules.

### REQ-FAM-003

Complete genealogy, family-tree visualization, ancestry research, complex custom groups, and a family-history archive are outside MVP.

### REQ-FAM-004

Family Group membership capacity must be controlled by an application-operator configuration rather than by individual Family Administrators. Use 50 members per Family Group as the initial planning target, 100 members as the default configured limit, and 250 members as the validated supported ceiling. Supporting more than 250 members in one Family Group requires later capacity validation before it is treated as supported.

### REQ-FAM-005

Family Group membership in MVP is private and invitation-only. A Family Administrator may issue and revoke a membership invitation for a specific intended person. Membership becomes active only after the recipient explicitly accepts the invitation. Public Family Group discovery, self-service join requests, and automatic membership are outside MVP.

### REQ-FAM-006

The first Family Group and first Family Administrator must be created through controlled application-operator provisioning. The application operator may provision the initial private Family Group and initiate the first Family Administrator invitation. The intended first administrator must still sign in and explicitly accept the invitation before membership becomes active. After activation, normal Family Administrator invitation rules apply. Public self-service Family Group creation is outside MVP.

## Adult profile first-slice contract

### REQ-FAM-007

The first adult self-profile slice must contain only these profile fields:

- **Display name:** required.
- **Contact email:** optional and separate from any authentication or sign-in email.
- **Contact phone:** optional.
- **Birthday month and day:** optional; birth year is not collected in this slice.

The first slice must not include profile photo, birth year, street address, precise or general location, biography, pronouns, medical details, emergency details, relationship labels, Household assignment, Safety Circle membership, or arbitrary custom fields. These exclusions apply to the first slice and do not decide whether a later approved MVP work item may add a field.

## Adult membership invitation contract

### REQ-FAM-008

For normal adult Family Group membership invitations:

- An active Family Administrator may invite a specific adult to that administrator's Family Group using an email address solely for invitation delivery.
- An invitation expires seven days after issue and is one-time, revocable, and non-reactivatable after it is declined, revoked, expired, or used.
- Only one pending invitation may exist for the same normalized recipient email and Family Group. Resending revokes the prior invitation and issues a new seven-day invitation.
- The adult recipient must be signed in, see the Family Group name and inviter display name, and explicitly accept or decline.
- Successful acceptance binds the invitation to the current Account and atomically creates at most one active membership only if the invitation is valid and configured Family Group capacity remains available.
- Capacity is checked when the invitation is issued for early feedback and again at acceptance; the acceptance-time check controls.
- Public discovery, join requests, automatic membership, delegated acceptance, and minor or guardian invitation paths are excluded.

This requirement does not define controlled first-administrator operator provisioning, production authentication, or a production email provider.

## Adult Family relationship contract

### REQ-FAM-009

The first adult Family Relationship slice must follow this contract:

- Either active adult may propose one relationship to another active adult in the same private Family Group.
- The proposer selects a structured label from the proposer's perspective. The supported reciprocal pairs are **Spouse / partner** and **Spouse / partner**, **Parent** and **Adult child**, **Sibling** and **Sibling**, or **Other family** and **Other family**.
- The recipient must explicitly accept or decline. The proposer may withdraw a pending proposal.
- Only one pending or accepted relationship may exist for the same adult pair and Family Group. Self-relationships, duplicate proposals, and contradictory simultaneous labels for the same pair are unavailable.
- An accepted relationship may be removed by either participant without the other participant's approval. Changing its label requires removing it and creating a new mutually confirmed proposal.
- Ending either participant's active membership makes the relationship unavailable in that Family Group. Persistence after removal, reactivation, or rejoining is not defined by this requirement.
- A Family Relationship remains separate from Account, Adult Profile, Family Group membership, Family Administrator responsibility, Household, Safety Circle, and all access-control decisions.

This slice excludes minors, guardians, dependents, legal or caregiving status, custom labels, genealogy, inferred relationships, multiple simultaneous labels for a pair, cross-group discovery, administrator moderation, dispute adjudication, and relationship-history screens.

## Shared Family Dates

### REQ-FAM-010

The first shared Family Dates slice must follow these rules:

- It covers shared birthdays and other important dates within one existing Family Group. Shared dates are distinct from Events.
- A date is visible only to its creator and active adult members of the same Family Group who are specifically associated with that date. Family Group membership alone does not make a date visible.
- The creator may associate only active adult members of that same Family Group. Only the creator may change the association list.
- The creator and associated active members may edit the date content. `REQ-FAM-011` through `REQ-FAM-013` define the required title, required description, and one specific calendar date including its year and without automatic recurrence.
- Only the creator may delete the date, and only while the creator is an active member of that Family Group.
- When an associated member's active membership ends, that person's access ends and the association is removed. Later active membership does not restore the association automatically.
- When the creator's active membership ends, the date becomes unavailable in normal Family views and is not transferred to another member or administrator. Retention and cleanup follow `REQ-PRIV-017`.
- Do not duplicate an Adult Profile birthday as a separate date record. A member's birthday may appear only when that existing profile field is explicitly shared with the active Family Group. Other important dates are separate shared-date entries.
- Shared-date rules are fixed for this first slice; Family Group configuration and a “main family group member” role are not included.

### REQ-FAM-011

For the first shared Family Dates slice, each date entry has a title, a description, and a date field. The title and date field are required. Description is required under `REQ-FAM-013`. Notes distinct from the description, attachments, notification settings, and other additional content fields are excluded from v1. The date value is one specific calendar date including its year; it does not recur automatically under `REQ-FAM-012`.

### REQ-FAM-012

In the first shared Family Dates slice, each entry represents one specific calendar date, including its year. Shared dates do not recur automatically; recurring-date behavior is excluded from v1. The product may later evolve toward standard calendar behavior, but that future direction does not add recurrence or other calendar capabilities to the current scope.

### REQ-FAM-013

The description field for each v1 Shared Family Date is required.

### REQ-FAM-014

Each adult manages their own Trusted Contact entries. Entries are private to that adult by default. Trusted Contacts remain distinct from the adult's own profile contact fields, Family membership, Family Relationships, and Safety Circle. A separate explicit choice and approved rule are required to share or designate an entry for a defined use. The manual first slice is defined by `REQ-FAM-016` and `REQ-PRIV-032`. Eligibility of people outside the Family Group follows `REQ-FAM-015`.

### REQ-FAM-015

A Trusted Contact may be a person who is not a Family Group member, such as a trusted friend, caregiver, neighbor, or other trusted person. Family Group membership is not a prerequisite for designation. The first slice requires no contact Account or Profile and uses only owner-entered information under `REQ-FAM-016`. A Trusted Contact remains distinct from the adult's own profile contact fields and from Safety Circle membership.

### REQ-FAM-016

Trusted Contacts first slice (DEC-088): an existing adult profile may own private manual contact records, independently of any Family membership. The current Account/Profile model has no separate inactive-profile state; an existing adult profile belonging to the current account is the owner eligibility check. The contact has no application identity or Family link, even if the actual person uses the application. No user/profile discovery, lookup, linking, invitation, acceptance or verification is provided.

Each record has a required trimmed plain-text Display Name, at least one valid Phone Number or Email Address, and an optional trimmed plain-text Relationship / Label. No other descriptive contact fields or authority flags are allowed. Email follows existing application validation and compares case-insensitively. Phone preserves its useful display value, supports international numbers and normalizes formatting for duplicate comparison without assuming a country for local numbers. Reasonable field lengths/validation use existing repository conventions.

The owner may list, add, edit these four fields and remove records in the existing Safety experience, without a feature Family selector. The resulting record must always have at least one valid contact method. A match on either normalized email or normalized phone rejects create/edit as a duplicate within the owner; exclude the edited record itself. Names alone are not identity, and different owners may store the same contact independently. Optimistic concurrency follows current CRUD conventions; no revision history is created.

Removal permanently deletes the record, with no tombstone, retained feature history, recycle bin, restore or retention window. Leaving/joining/changing selected Family does not remove, duplicate or expose records. Contacts follow owner-profile deletion through ownership cascade when a separately implemented account lifecycle actually deletes that profile; this slice implements no account deletion. External-contact eligibility loss does not apply. Operational logs/backups remain governed by existing infrastructure, not a new feature retention framework.

## Explicit non-goals

### REQ-PROD-009

The product must not provide or become:

- Surveillance, covert tracking, behavioral monitoring, or control of adult relatives
- A default family-wide live location map or default continuous family tracking
- Historical movement playback or movement anomaly detection
- Automatic danger scoring or automatic missing-person determination
- Automatic emergency-service dispatch
- A replacement for emergency services, law enforcement, medical professionals, or appropriate real-world assistance
- A medical diagnosis system
- An AI system that independently determines whether someone is in danger
- A general-purpose family chat, social-media feed, reactions, or comments
- A complete genealogy or ancestry product
- Family finances, payments, household chore management, or detailed household inventory
- Advanced medical records or property management
- A photo or video social platform
- Advanced travel booking
- Sophisticated event budgeting, ticketing, or vendor management

## Cross-area product invariants

### REQ-PROD-010

Family Events are a core product and MVP capability, not an auxiliary safety feature.

### REQ-PROD-011

Plans must remain lightweight, while Trips may support more structured travel information. Neither requires location sharing to remain useful.

### REQ-PROD-012

Concern and Safety Incident are separate product concepts. Concern supports review before escalation; a Safety Incident supports coordinated family response.

Concerns and Safety Incidents are always created manually. Nothing automatically creates an Incident, escalates a Concern into one, or moves a Concern toward one.

The Concern creator may use the deliberate manual **Start an Incident** action, which creates a separate new Incident. That action:

- prefills only the subject;
- transfers no Concern information and no Concern audience;
- leaves the Concern unchanged.

The Incident keeps only a non-authorizing lineage reference to the originating Concern. Detailed rules:

- Concern access and lifecycle: `../workflows/safety.md#req-safe-015` and `privacy-safety-contract.md#req-priv-025`.
- Incident workflow: `../workflows/safety.md#req-safe-016`.
- Incident access, lineage, and retention: `privacy-safety-contract.md#req-priv-026`.

## Scale decision reference

`OD-005` is resolved by `DEC-014`. See `../decision-log.md#resolved-product-decisions`.

Invitation-only membership is approved by `DEC-016`. Controlled first Family Group and first Family Administrator bootstrap is approved by `DEC-018`. The normal-adult invitation lifecycle is approved by `DEC-030`. The adult Family Relationship contract is approved by `DEC-034` and active under `CTX-1.6`. Shared Family Dates are approved by `DEC-038` and active under `CTX-1.7`; their fields, single-date semantics, and 30-day retention period are approved under `CTX-1.9` and `CTX-1.10`. Sign-in/recovery method, production email provider, operator support process, long-term invitation-history retention, and minor/guardian behavior remain unresolved.
