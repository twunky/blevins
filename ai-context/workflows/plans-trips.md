---
Status: APPROVED
Authority: CANONICAL WORKFLOW
Context-Baseline: CTX-1.29
Version: 1.7
Source: ../bootstrap/product-ux-source.md; DEC-048; DEC-054; DEC-055; DEC-056; DEC-057; DEC-060; DEC-082
Constrained-By:
  - ../core/product-contract.md
  - ../core/ux-contract.md
  - ../core/privacy-safety-contract.md
---

# Plans and Trips Workflows

Plans and Trips are distinct coordination concepts. Both remain useful without location sharing under `REQ-PRIV-001`. Item-audience selection is governed by `REQ-PRIV-020` and is separate from location-sharing decisions.

## Lightweight Plans

### REQ-PLAN-001

A Plan is a lightweight future activity. It may represent a short visit, outing, overnight stay, or similar activity and must remain less structured than a Trip.

### REQ-PLAN-002

A Plan may include activity or destination, timing, optional expected return, audience, notes, and an optional expected check-in.

### REQ-PLAN-003

The standard Plan flow is:

`Create Plan → Activity / destination → Timing → Optional expected return → Choose item audience (per REQ-PRIV-020) → Optional expected check-in → Optional Notes (per REQ-PLAN-006) → Save plan → Update if needed → Check in / complete`

### REQ-PLAN-004

A Plan may be updated, checked in against, and explicitly completed. Location sharing is optional and, when selected, is governed by the Sharing workflow.

### REQ-PLAN-005

For MVP Plan audience selection:

- The creator chooses either the one applicable Family Group as a whole or specific active adult members of that same Family Group. The creation flow does not offer a combined whole-group-plus-selected-people choice.
- The creator is not a selectable recipient and retains access to the Plan as its creator. Selecting recipients grants access to that Plan only.
- For the whole-group audience option, access follows current active adult membership: new active members gain access, ending active membership removes access, and restored active membership restores access. This dynamic rule does not apply to individually selected recipients.
- Multi-group audience selection, post-creation audience changes, and recipient access changes for individually selected recipients remain deferred under `REQ-PRIV-020`.
- If the creator leaves the applicable Family Group, the Plan record is retained and is not automatically deleted or transferred to another owner. This rule does not define post-departure read or edit permissions.
- Plan audience selection does not authorize location sharing, notifications, profile or Safety information, or a Sharing Session.
- A Plan note is part of the Plan and follows its existing audience. For a whole-group Plan, the note is visible to current active adult members, including members who become active in the group later.

### REQ-PLAN-006

Optional Plan notes at creation:

- The creator may enter an optional plain-text note when creating a Plan, after the optional expected check-in and before saving.
- A note may contain up to 1,000 characters. Blank or whitespace-only input is treated as no note.
- Preserve internal line breaks. Display note content as text; do not interpret it as markup.
- Show that the note follows the Plan audience. For a whole-group audience, explain that future active adult members may also see the Plan and its note.
- A note does not create access beyond the Plan's existing audience and access enforcement.
- Editing or removing a note after creation is not included in this requirement.

### REQ-PLAN-007

The Plan creator may edit or clear the existing optional Plan note after creation. Only the creator with current Plan access may write it; losing active membership in the Plan's Family also removes this write access. The same Plan `Notes` field is used, with the existing plain-text, 1,000-character, line-break, blank-as-absent, and Plan-audience read rules. No separate authored notes, note author metadata, revision history, or general Plan editing is introduced.

## Trips

### REQ-TRIP-001

A Trip is more structured than a Plan and may represent a vacation, road trip, international trip, relocation, multi-day visit, or multi-family trip.

### REQ-TRIP-002

A Trip may include a title, travelers, destinations, dates and times, departure, expected arrival, itinerary, accommodations, transportation notes, companions, emergency contacts, check-ins, and sharing preferences.

### REQ-TRIP-003

MVP Trips must support trip title, travelers, destinations, dates and times, accommodations, transportation notes, companions, expected arrival, check-ins, and sharing settings.

### REQ-TRIP-004

The standard Trip flow is:

`Create Trip → Travelers → Destination(s) → Important travel information → Expected arrival → Choose item audience (per REQ-PRIV-020) → Check-in preference → Separate location-sharing decision → Travel begins → Check-ins / updates → Arrival → Sharing expires → Trip completes`

### REQ-TRIP-005

Trip progress may include check-ins and updates through arrival. Any associated temporary Sharing Session expires according to its authorized duration, including arrival or trip completion where selected.

### REQ-TRIP-006

MVP Trips exclude travel booking, expense splitting, advanced itinerary optimization, travel recommendation engines, and complex document management.

### REQ-TRIP-007

For MVP Trip roles and editing:

- The account that creates the Trip is its creator and organizer. There is no separate permission-bearing Organizer role. The creator does not have to be a traveler.
- The creator retains access to the Trip as creator if their eligible Family Group membership later ends. Creator access is distinct from audience membership. The Trip is not transferred to another owner.
- The creator may edit Trip title, description, dates, primary destination, travelers, non-system contacts, expected arrival, transportation and accommodation notes, audience, and completion status.
- Traveler status alone grants no Trip visibility, Trip editing, Trip Notes access, or Sharing Session access. A traveler can view or contribute only when separately included in the Trip audience and granted the corresponding audience capability.
- A Trip viewer is an eligible in-system member included in the Trip audience. Audience membership grants Trip viewing and Trip Notes access as defined by `REQ-TRIP-010`; it does not grant core Trip editing.
- Changing the Trip audience is a creator-only capability and must be separately authorized for implementation. Each change is explicit, takes effect immediately, and must identify the access change before confirmation so information is not silently exposed to newly added viewers.

### REQ-TRIP-008

For MVP Trip creation and fields:

- Require a Trip title, one primary destination, start date, end date, at least one eligible traveler, and an explicit audience.
- The end date must not precede the start date.
- The creator does not need to be one of the travelers.
- Description, transportation notes, accommodation notes, non-system contacts, and expected arrival are optional.
- The initial Trip model supports one primary destination. Multiple destinations are deferred.
- Admin-configured audience modes are not part of MVP. The product-defined choices are one applicable Family Group or selected eligible people from that same group.

### REQ-TRIP-009

For MVP non-system Trip contacts:

- The creator may add informational contact records with required name and optional relationship/role, phone, and email. General contact Notes are not included.
- Contact details follow the Trip audience visibility.
- A non-system contact is not an application user and cannot log in through the contact record, view the Trip, receive Trip permissions, author Trip Notes, or participate in a Sharing Session. Contact records are informational only.

### REQ-TRIP-010

For MVP Trip Notes:

- Current authorized Trip audience members may read normally visible notes and add notes. The Trip creator may also read visible notes and add notes through creator authority for their own Trip, independently of Trip audience or eligible Family Group membership. This narrow creator exception continues while creator access to that Trip remains valid. It grants no Notes access to traveler-only users, administrators, non-system contacts, or other non-audience people.
- Every Notes read or mutation uses current Trip authorization under `REQ-TRIP-007` and `REQ-PRIV-022`. Notes follow the Trip audience without creating another access path to the Trip or unrelated information. Selected-person access, group audience snapshots, membership-loss revocation, and no automatic restoration on rejoin continue to apply. Traveler status and informational contact status alone grant no Notes access.
- An author with current Notes access may edit or soft-delete their own **ACTIVE** note. Each note preserves its author and created timestamp; editing preserves those values and records an updated timestamp. Author soft deletion changes **ACTIVE** to **AUTHOR_DELETED**, removes the note from normal display, and retains its original content, author, created timestamp, and updated timestamp where applicable. Preserve enough state and transition information to distinguish author deletion; normal author deletion does not hard-delete the record.
- The Trip creator may hide another author's **ACTIVE** note for moderation, changing it to **CREATOR_HIDDEN** and removing it from normal display. The original content, author, and timestamps remain retained. The creator may not rewrite another author's content. The original author may not edit or delete a creator-hidden note through normal Trip Notes actions. A creator-hidden note is immutable through normal Trip Notes behavior in MVP.
- **ACTIVE**, **AUTHOR_DELETED**, and **CREATOR_HIDDEN** are conceptual MVP states, not a prescribed database representation. Removed notes remain associated with the Trip while its record exists and are excluded from normal Trip Notes APIs and UI. Retained records create no alternate access path. This requirement does not define a global retention duration, Trip deletion, archive/disposal rules, audit-history UI, restore/recovery UI, or a general audit-log subsystem.
- Trip Notes are lightweight coordination text, rendered as plain text without executable markup. They are not chat. Replies, reactions, threads, a general activity feed, and a full revision-history UI are excluded from MVP. Notes grant no Sharing Session, location, Check-In, administrator override, or unrelated private-data access; Trip and Sharing Session audiences remain independent under `REQ-SHARE-009`.

### REQ-TRIP-011

For MVP expected arrival, Check-Ins, and Trip completion:

- Expected arrival is optional and distinct from Trip start date, end date, and completion status. A missed expected arrival represents uncertainty, not confirmed danger.
- Check-Ins remain optional, personal, and voluntary. A Trip creator cannot require another traveler to check in, provide current location, or start a Sharing Session. Existing Check-In rules remain authoritative.
- Only the Trip creator may mark a Trip complete. Passing the end date does not automatically complete the Trip or raise a Concern. Missing location does not automatically raise a Concern.
- Trip open/completion status does not itself start, restart, or extend a Sharing Session. Any session follows its own separately authorized sharing choices, duration, and stop controls.

### REQ-TRIP-012

The MVP Trip product flow is:

`Trips Overview → Create Trip → Trip Detail`

Creation includes required fields and explicit audience selection. The form may use sections or dialogs for travelers, contacts, and audience. A separate Review & Create screen is not required. Upcoming/Past/All filters, traveler avatars, destination cards, status chips, richer filtering, and quick actions such as Leaving Now, Delayed, or Arrived remain UX guidance/prototype concepts, not MVP product requirements.

The create flow does not itself start or authorize a Check-In or Sharing Session. Those remain optional, separately controlled capabilities under `REQ-TRIP-011` and the Sharing workflow.
