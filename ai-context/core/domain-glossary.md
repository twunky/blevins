---
Status: APPROVED
Authority: CANONICAL — CTX-1.35
Context-Baseline: CTX-1.35
Version: 1.11
Source: ../bootstrap/product-ux-source.md; DEC-016; DEC-018; DEC-028; DEC-029; DEC-030; DEC-031; DEC-034; DEC-038; DEC-059; DEC-071; DEC-088; DEC-089
---

# Domain Glossary

These are approved nontechnical product concepts under `CTX-1.20`. This file does not define tables, schemas, payloads, services, persistence, or implementation entities.

## People and family

### Family Group

The private extended-family context coordinated by the product. Its operator-configured membership capacity follows `REQ-FAM-004`; membership is invitation-only under `REQ-FAM-005`; the first Family Group bootstrap follows `REQ-FAM-006`; and Family Administrators do not control the platform capacity limit.

### Family Member

A person represented within the Family Group. A family relationship does not by itself grant access to the person's private information.

### Adult Profile

The self-managed profile for an adult Family Member in the first Phase 5 slice. It contains a required display name and optional contact email, contact phone, and birthday month/day. It remains distinct from the person's Account, Family Group membership, administrator responsibility, relationships, Household, and Safety Circle.

### Profile Field Visibility

The first adult-profile slice supports only **Only me** and **Active Family Group** visibility for optional profile fields. Display name is visible to active members of the same Family Group. Visibility does not grant membership, administrator responsibility, operator access, Household access, relationship-based access, Safety Circle access, or public access.

### Family Relationship

A mutually confirmed, Family-Group-scoped descriptive connection between two active adult Family Members. The first slice supports the reciprocal label pairs **Spouse / partner** and **Spouse / partner**, **Parent** and **Adult child**, **Sibling** and **Sibling**, or **Other family** and **Other family**. A pending proposal is visible only to its participants; an accepted relationship is visible to active adults in the same Family Group. A Family Relationship grants no permissions or authority and remains distinct from Account, Adult Profile, membership, Family Administrator responsibility, Household, Safety Circle, location sharing, and guardian or caregiving status.

### Shared Family Date

A birthday or other important date shared within one Family Group. Under the first-slice rules, only its creator and specifically associated active adult members of that group can view it; the creator and associated members can edit date content, while only the active creator can delete it. The creator manages associations, and association grants no other information access or authority. Shared dates are distinct from Events. Birthday display follows the Adult Profile field's explicit visibility choice. V1 fields and single-date behavior follow `REQ-FAM-011` through `REQ-FAM-013`; the 30-day retention period follows `REQ-PRIV-018`. Audit, backup, and cleanup mechanics remain undecided.

### Family Administrator

A family member with responsibility for membership or family structure. A Family Administrator may issue or revoke a Family Group membership invitation. This responsibility does not automatically grant access to private information or membership in another person's Safety Circle.

### Account

A signed-in application identity. Having an Account does not by itself establish Family Group membership or access to Family Group data.

### Membership Invitation

A revocable, one-time invitation issued by a Family Administrator for a specific intended adult to join one Family Group. It is delivered by email through a provider-neutral boundary, expires after seven days, and grants no Family Group access until a signed-in adult explicitly accepts and active membership is created. The delivery email is not an Adult Profile contact field or authentication identifier. The first Family Administrator invitation follows the separate controlled bootstrap rule in `REQ-FAM-006`.

### Application Operator

The application-level operator responsible for platform configuration and controlled first Family Group provisioning. This is not a Family Group role. Operator responsibility does not grant routine access to private Family Group content or private user information.

### Household

A basic grouping of family members. Detailed household behavior is not defined by this foundation.

### Safety Circle

A private owner-managed, Family-scoped reusable recipient-selection preset under REQ-FAM-017 / REQ-PRIV-033. V1 members are eligible same-Family adult application profiles; Trusted Contacts and external contacts remain separate. Circle membership grants no protected access. Check-Ins and Sharing Sessions store their own confirmed explicit grants.

### Trusted Contact

An adult owner-managed trusted person/contact entry or designation. The person may be outside the Family Group. The first slice is a private manual contact record with no application identity or Family link, under REQ-FAM-016 / REQ-PRIV-032. It is distinct from the owner's profile contact fields and Safety Circle. Designation alone grants no protected product access or authority; future uses remain separately gated.

## Coordination

### Event

A family occasion or group activity with details, invitations, RSVP, updates, and optional coordination needs or tasks.

### Need

A simple event coordination concept for food, supplies, equipment, help, transportation, accommodations, or other responsibilities that a member may claim.

### Plan

A lightweight future activity with timing, audience, notes, and optional expected return or check-in. It is less structured than a Trip.

### Trip

A structured travel activity that may include travelers, destinations, travel timing, accommodations, transportation notes, companions, expected arrival, check-ins, and sharing preferences.

### Task

An assigned coordination responsibility. Event tasks support preparation or needs; incident tasks support coordinated investigation or response. Applicable tasks expose ownership, status, and result, with completion time where required by the workflow.

## Sharing and check-ins

### Location Sharing

Voluntary sharing of location-related information with an understood audience, purpose, and duration. It is not synonymous with tracking.

### Sharing Session

A temporary context that defines what is shared, who can see it, why it is shared, how frequently it updates, when it begins, and when it ends.

### Check-In

A fast status update such as **I'm okay**, **Arrived**, **Leaving now**, **Delayed**, or **Plans changed**. It may optionally include a note, voluntarily provided current location, or updated expected arrival.

### Expected Arrival

The time someone intends or is expected to reach a destination. Missing it creates uncertainty rather than confirming danger.

## Safety coordination

### Concern

A lightweight safety state used to review what is known after a missed expectation or unexpected communication gap, before or instead of creating a coordinated Safety Incident.

### Safety Incident

A structured family coordination workspace for a safety situation. It does not create official emergency classifications or bypass existing privacy authorization.

### Coordinator

The person coordinating a Safety Incident response.

### Participant

A person participating in an event, trip, Sharing Session, Concern review, or Safety Incident according to that context. Participation does not automatically grant private-data access.

### Finding

Information contributed during Concern or Safety Incident review. A finding carries its confidence category and relevant provenance.

### Contact Attempt

A recorded attempt to reach the person involved or another relevant contact, including its result when known.

### Timeline Entry

A meaningful chronological incident activity, such as confirmed contact, expected arrival, contact attempt, reported finding, task assignment, correction, state change, or resolution.

## Information confidence

### Confirmed

Directly verified information.

### Reported

Information provided by someone but not independently confirmed.

### Assumption

A hypothesis being considered.

### Unknown

A question for which no reliable answer currently exists. Unknown is meaningful information, not an empty field to be guessed through.

### Outdated

Information that was previously valid but is no longer current.

## Conceptual relationships and invariants

- A Family Group contains Family Members and may use Household grouping.
- A Family Member may select a Safety Circle.
- Events, Plans, Trips, and temporary activities may provide context for a Sharing Session.
- A Check-In may occur independently or within an Event, Plan, Trip, or Sharing Session.
- A missed expectation may lead to a Concern; a Concern may resolve, or its creator may deliberately start a separate Safety Incident from it. No access or information transfers automatically (`REQ-SAFE-016`).
- A Safety Incident may contain Participants, a Coordinator, Findings, Contact Attempts, Tasks, Timeline Entries, and a Resolution.
- Every safety Finding retains an applicable confidence category; Unknowns are preserved rather than inferred.
