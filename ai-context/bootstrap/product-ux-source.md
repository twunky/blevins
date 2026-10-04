---
Status: SOURCE-ONLY — EXTRACTION COMPLETE
Authority: NONE — EXCLUDED FROM NORMAL ROUTING
Canonical after extraction: NO
Purpose: Approved Product & UX Foundation source for canonical extraction
---

# Approved Product & UX Foundation

This is the complete curated source approved for AI Context Foundation v1.0 extraction. It excludes brainstorming, superseded discussion, and technical architecture.

Extraction is complete and `CTX-1.0` is approved. This file is retained only as source evidence, has no canonical authority, and must not enter normal context routing.

## Product identity

The product is a private family coordination application for a large extended family.

Its long-term philosophy is:

### Connect

- Family members
- Family events
- Important dates
- Useful family information

### Coordinate

- Family events
- Trips
- Lightweight plans
- Responsibilities
- Food, supplies, transportation, and accommodations
- Check-ins

### Care

- Voluntary location sharing
- Expected arrivals
- Safety Circles
- Concerns
- Coordinated family safety incidents

The application must feel like a family application with strong safety capabilities. It is not primarily a tracking application.

## Core product purpose

When a family member becomes unexpectedly unavailable or unreachable, important information is often fragmented across calls, texts, emails, calendars, travel confirmations, individual relatives, and memory.

The product should help the family answer:

- Where was the person last confirmed to be?
- Where were they supposed to be, where were they going, and when were they expected to arrive or check in?
- Who were they traveling with, and who might know more?
- What information is confirmed, reported, assumed, unknown, or outdated?
- Who has tried contacting them, and what actions have been taken?
- Who is coordinating the response?
- What should the family investigate next?

The application does not replace emergency services, law enforcement, medical professionals, or appropriate real-world assistance.

## Product principles

1. There is no default family-wide live location map.
2. Use the term **location sharing**, not **location tracking**.
3. Location sharing is voluntary.
4. The application must remain useful to someone who never shares precise location.
5. Users control what they share, who receives it, why it is shared, and how long it remains shared.
6. Temporary and contextual sharing is preferred.
7. Sharing Sessions are a core concept and may be associated with trips, events, plans, and temporary activities.
8. Joining the family, a trip, or an event does not automatically enable location sharing.
9. Family relationship does not automatically equal permission.
10. Administrative responsibility does not automatically grant access to private information.
11. Safety Circle membership is selected by the individual.
12. A missed expected arrival or check-in represents uncertainty, not confirmed danger.
13. Concern and Safety Incident are separate concepts. A Concern allows review before escalation.
14. Creating a Safety Incident does not automatically bypass privacy settings.
15. Important safety information must distinguish Confirmed, Reported, Assumption, Unknown, and Outdated.
16. Important safety information should preserve its source, timestamp or freshness, contributor, and confidence classification.
17. Corrections should not silently erase relevant incident history.
18. Safety notifications should escalate gradually rather than notifying the entire family by default.
19. Family Events are a core product capability.
20. Everyday family usefulness matters because normal usage keeps useful safety information current.
21. Privacy, consent, trust, and autonomy are foundational requirements, not optional features.

## Goals

- Improve family situational awareness and normal-life coordination.
- Improve coordination when someone becomes unexpectedly unreachable.
- Preserve the distinction between known facts and uncertain information.
- Respect individual autonomy.
- Make safety preparation part of ordinary family activity.
- Reduce information overload for large families.
- Support geographically distributed family members.
- Remain useful without continuous location sharing.

## Product non-goals

The product is not:

- A surveillance system, covert tracking software, or behavioral monitoring platform
- A default family-wide live location map
- A tool for controlling adult relatives
- A replacement for emergency services or law enforcement
- An emergency dispatch or medical diagnosis system
- An automatic missing-person determination system
- A social media platform or general-purpose family chat replacement
- A genealogy platform in MVP
- A family financial-management system in MVP
- An AI system that independently determines whether someone is in danger

## Conceptual product areas

- Home
- Family
- Events
- Plans & Trips
- Safety
- Me / Settings

Location is not a top-level application area. It appears contextually within Sharing Sessions, trips, plans, events, check-ins, and safety workflows.

## Family

The Family area may include family members, profiles, relationships, households or basic grouping, contact information, birthdays or important dates when shared, trusted contacts, and Safety Circle.

Relationship and permission are separate concepts. Family administrators may manage membership or structure without automatically receiving private user information.

### MVP Family scope

Included:

- Member profile
- Family relationship
- Household or basic grouping
- Contact information
- Privacy settings
- Safety Circle

Excluded from MVP:

- Complete genealogy or family tree visualization
- Ancestry research
- Complex custom groups
- Family-history archive

## Family Events

Family Events are a core MVP area. Examples include birthdays, weddings, reunions, holidays, graduations, cookouts, memorials, parties, sporting events, family gatherings, camping or fishing outings, and group activities.

Event information may include name, description, date, time, location, organizer, invited members, RSVP or status, notes, and updates. Coordination may include tasks, food, supplies, equipment, transportation, and accommodations.

### MVP Events scope

Included:

- Create event
- Invite members and RSVP
- Date, time, and location
- Organizer and event updates
- Simple tasks and responsibilities
- Food or supply needs
- Claiming responsibilities

Excluded from MVP:

- Event chat or photo albums
- Advanced budgeting
- Ticketing or vendor management
- Complex room assignments
- Detailed equipment inventory
- Social-media-style comments or reactions

Event participation does not automatically enable location sharing. Optional event Sharing Sessions may be supported.

### Event workflow

`Create Event → Basic Details → Invite Family → RSVP → Add Needs / Tasks → Members claim responsibilities → Organizer posts meaningful updates → Event occurs → Event completes`

Event creation should remain lightweight. Food, supplies, and help or tasks should preferably use one simple **Needs** concept rather than multiple complicated systems.

## Plans

A Plan is a lightweight future activity, such as driving to a relative's home, fishing, staying with family, or taking the children camping.

A plan may include activity or destination, timing, expected return, audience, notes, and an optional expected check-in. A plan must remain useful without location sharing.

### Plan workflow

`Create Plan → Activity / destination → Timing → Optional expected return → Audience → Optional expected check-in → Save plan → Update if needed → Check in / complete`

## Trips

Trips are more structured than Plans. Examples include vacations, road trips, international travel, relocations, multi-day visits, and multi-family trips.

Trips may include travelers, destinations, departure, expected arrival, itinerary, accommodations, transportation notes, companions, emergency contacts, check-ins, and sharing preferences.

### MVP Trip scope

Included:

- Trip title, travelers, and destinations
- Dates and times
- Accommodations and transportation notes
- Companions
- Expected arrival
- Check-ins and sharing settings

Excluded from MVP:

- Travel booking or expense splitting
- Advanced itinerary optimization
- Travel recommendation engines
- Complex document management

### Trip workflow

`Create Trip → Travelers → Destination(s) → Important travel information → Expected arrival → Check-in preference → Sharing decision → Travel begins → Check-ins / updates → Arrival → Sharing expires → Trip completes`

## Sharing Sessions

A Sharing Session is a temporary context for voluntary sharing. It combines what is shared, who can see it, why it is shared, how frequently it updates, when it begins, and when it ends.

Possible sharing modes:

1. No location sharing
2. Check-in only
3. Periodic location sharing
4. Temporary real-time location sharing
5. Trip-based sharing
6. Event-based sharing

Sharing dimensions remain separate:

- **What:** Status, check-ins, approximate or current location, periodic location, temporary live location, destination, or itinerary
- **Who:** Safety Circle, selected family, trip participants, or event participants
- **Why:** Trip, event, plan, or temporary activity
- **How long:** Until arrival, trip end, event end, a fixed duration, a selected time, or manual stop

The user should always be able to understand:

- Am I sharing?
- What am I sharing?
- Who can see it?
- When will it stop?

Users must be able to stop active sharing. Persistent or indefinite sharing remains an open product decision.

## Check-ins

Check-ins should be fast and simple. Examples include **I'm okay**, **Arrived**, **Leaving now**, **Delayed**, and **Plans changed**.

Optional information may include a note, voluntarily included current location, or updated expected arrival. A missed check-in does not automatically indicate danger.

### Expected check-in flow

`Expected check-in missed → Reminder to the individual → Optionally notify designated trusted contacts according to preferences → Family may raise a Concern if appropriate`

## Safety Circle

A Safety Circle is a user-selected set of trusted people. Possible members include a spouse, parent, adult child, sibling, trusted friend, or caregiver.

Being a Family Administrator does not automatically place someone in another person's Safety Circle. Access should reflect explicit user choices.

## Concern

Concern is a lightweight safety state before a coordinated incident. Examples include a missed expected arrival, missed expected check-in, or unexpected communication gap.

It allows the family to review what is actually known before escalating unnecessarily.

### Concern workflow

`Expectation missed → Personal reminder if appropriate → Trusted-contact awareness if configured → Family member raises Concern → Review authorized information → Record contact attempts → Identify unknowns → Resolve Concern`

Alternatively, the Concern may escalate to a coordinated Safety Incident.

## Safety Incident

A Safety Incident is a structured family coordination workspace. It may include:

- Person involved, incident status, coordinator, and participants
- Time the concern began and current summary
- Last confirmed contact and last shared location, if authorized
- Expected destination and arrival
- Trip information, travel companions, and relevant contacts
- Contact attempts, findings, tasks, timeline, and notes
- Attachments later, if approved
- Resolution

Creating an incident does not unlock information that was not already authorized.

Possible conceptual states are Concern Raised, Reviewing Information, Coordinating Response, Escalated Externally, Contact Re-established, Resolved, and Closed / Archived. These are family coordination states, not official emergency classifications.

## Information confidence

- **Confirmed:** Directly verified.
- **Reported:** Provided by someone but not independently confirmed.
- **Assumption:** A hypothesis being considered.
- **Unknown:** No reliable answer currently exists.
- **Outdated:** Previously valid information that is no longer current.

Important incident information should show its source, contributor, timestamp or freshness, and confidence category. Unknowns are first-class information. Corrections should preserve useful historical context rather than silently replacing prior information.

## Incident tasks

Incident tasks may include calling or texting the person, contacting a travel companion or accommodation, verifying a destination or travel plans, gathering an itinerary or other information, contacting family at the destination, and coordinating appropriate outside assistance.

Tasks should show owner, status, result, and completion time. Task coordination should reduce duplicate family effort.

## Incident timeline

The timeline should contain meaningful chronological activity such as confirmed contact, expected arrival, contact attempts, reported findings, assigned tasks, corrected information, incident state changes, and resolution.

The timeline becomes the family's authoritative chronological reconstruction.

## Incident resolution

Resolution should capture the time contact was restored or resolution occurred, outcome, summary, assistance provided if applicable, unresolved follow-up, important corrections, and relevant lessons if appropriate.

The incident should transition to resolution rather than simply disappearing.

## Privacy and trust

Privacy, consent, trust, and autonomy are foundational.

- Information sharing is voluntary where appropriate.
- Users control audiences.
- Location sharing is independent from trip or event participation.
- Temporary access should expire when appropriate.
- Users may revoke future sharing.
- Administrative authority does not imply private-data access.
- Relationship does not imply permission.
- An incident does not automatically override privacy.
- Lack of location data does not imply something is wrong.
- Stale information must not appear current.
- Old sensitive information should not remain indefinitely merely because it was once useful.

The UX must avoid making users feel monitored.

## Notifications

Notifications should be actionable and restrained. Useful types may include event invitation or RSVP, event task assignment, event change, check-in reminder, missed expected check-in according to preferences, trip departure or arrival update, Concern update, incident task assignment, major incident update, and resolution.

Notifications should generally escalate outward gradually:

`Individual → Selected trusted people → Incident participants → Broader family only when intentionally appropriate`

Large families should not receive every minor update.

## Home and dashboard UX

The Home experience should answer:

> What is happening with my family, and is there anything I need to do?

It should not answer:

> Where is everybody right now?

Recommended hierarchy:

1. Needs My Attention
2. Upcoming Events
3. Current Trips / Plans
4. Relevant Family Updates
5. My Tasks
6. Active Safety Concern, when applicable
7. Active Sharing indicator for the current user

Attention items may include an RSVP, assigned task, expected check-in, event update, travel update, or safety concern. The dashboard should prioritize relevance over a universal activity feed.

## Primary UX areas

- Home
- Events
- Plans
- Family
- Safety
- Me / Settings

Mobile navigation may eventually use either five or six primary destinations. That decision remains open for UX validation.

## Core UX principles

- Family-first experience
- Attention before activity
- Progressive disclosure
- Sharing must always be understandable
- Location is contextual
- Calm safety language
- Facts must look different from uncertainty
- Mobile-first critical actions
- Large-family information filtering
- Accessibility for older and less technical family members

Important mobile actions should require minimal interaction: check in, RSVP, claim a task, update expected arrival, start or stop sharing, record a contact attempt, add a safety finding, and resolve a Concern.

## Consolidated MVP scope

### Family

- Profiles, relationships, household or basic grouping, contacts, privacy preferences, and Safety Circle

### Home

- Needs Attention, upcoming events, shared or current trips, relevant updates, user tasks, and safety concern visibility when appropriate

### Events

- Create event, invite, RSVP, organizer, location/date/time, updates, simple tasks, food or supply needs, and claiming needs

### Plans & Trips

- Lightweight plans, structured trips, travelers, destinations, accommodations, transportation notes, companions, expected arrival, and check-ins

### Sharing

- Check-in sharing, temporary Sharing Sessions, periodic sharing concept, temporary real-time sharing, audience selection, expiration, stop sharing, and event/trip/plan context

### Safety

- Safety Circle, Concern, Safety Incident, coordinator, participants, information confidence, unknowns, contact attempts, tasks, timeline, and resolution

### Notifications

- Event, task, check-in, trip, Concern, incident, and resolution notifications

## Explicit MVP exclusions

- Default continuous family tracking or a family-wide live map
- Historical movement playback, behavioral surveillance, or movement anomaly detection
- Automatic danger scoring, missing-person determination, or emergency-service dispatch
- General-purpose family chat, social-media feed, reactions, or comments
- Complete genealogy or ancestry research
- Family finances or payments
- Household chore management or detailed household inventory
- Advanced medical records or property management
- Photo or video social platform
- Advanced travel booking
- Sophisticated event budgeting
- Ticketing or vendor management

## Terminology

Preferred terms:

- Location Sharing
- Sharing Session
- Check-In
- Expected Arrival
- Safety Circle
- Concern
- Safety Incident
- Confirmed
- Reported
- Assumption
- Unknown
- Outdated
- Event
- Plan
- Trip

Avoid surveillance-oriented terms such as **tracking**, **monitoring**, **target**, and **subject**. Do not use **Missing Person Alert** unless genuinely appropriate.

## Approved phase status

| Phase | Name | Status |
|---|---|---|
| Phase 1 | Product & UX Foundation | `COMPLETE` |
| Phase 2 | AI Context Foundation | `CURRENT` |
| Phase 3 | Technical Architecture | `NEXT` |
| Phase 4 | MVP Foundation | `FUTURE` |
| Phase 5 | Family & Profiles | `FUTURE` |
| Phase 6 | Events | `FUTURE` |
| Phase 7 | Plans, Trips & Sharing | `FUTURE` |
| Phase 8 | Safety Coordination | `FUTURE` |
| Phase 9 | Integration & UX Refinement | `FUTURE` |
| Phase 10 | Testing & Verification | `FUTURE` |
| Phase 11 | Pilot & Release Readiness | `FUTURE` |

## Open decisions

These are not approved requirements and must not be resolved without explicit approval.

### OD-001 — Mobile primary navigation

Determine whether mobile uses five or six primary destinations.

### OD-002 — Persistent location sharing

Determine whether persistent or indefinite location sharing belongs in MVP.

### OD-003 — Minor and guardian behavior

Define an explicit product policy before implementation.

### OD-004 — Incident history retention

Decide the retention policy before production use.

### OD-005 — Initial family scale

Establish the expected initial size and practical upper range before architecture validation.

## Visual and UX direction

Initial visual prototypes are approved only as starting-point concepts. They establish:

- A modern, family-oriented web and mobile experience
- Clear dashboard hierarchy
- Family events as a major visible feature
- Trips and sharing as contextual features
- A structured but calm safety workspace
- Responsive and mobile-friendly behavior

They are not pixel-perfect final designs. Future implementation may refine layout while preserving approved UX contracts and workflows.
