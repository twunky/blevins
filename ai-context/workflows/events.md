---
Status: APPROVED
Authority: CANONICAL WORKFLOW
Context-Baseline: CTX-1.30
Version: 1.4
Source: ../bootstrap/product-ux-source.md; DEC-048; DEC-051; DEC-058; DEC-083
Constrained-By:
  - ../core/product-contract.md
  - ../core/ux-contract.md
  - ../core/privacy-safety-contract.md
---

# Events Workflow

Family Events are a core MVP capability under `REQ-PROD-010`. Item-audience selection is governed by `REQ-PRIV-020`; Event view/edit access is governed by `REQ-PRIV-021`.

## Event behavior

### REQ-EVT-001

Members must be able to create a family event with a name, description, date, time, location, and organizer; invite family members; and collect RSVP or participation status. In MVP, the creator is the organizer under `REQ-EVT-009`. Event audience, invitation, participation, and RSVP are distinct under `REQ-EVT-010`.

### REQ-EVT-002

An event may include notes and meaningful organizer updates. Updates should communicate relevant changes without creating a general-purpose event chat or social feed. For MVP, the Event description holds stable Event information and `REQ-EVT-012` governs time-ordered Updates; no separate generic Event Notes field is added.

### REQ-EVT-003

Event coordination must support simple tasks or responsibilities, food or supply needs, and members claiming responsibilities. `REQ-EVT-011` defines MVP eligibility and Need lifecycle.

### REQ-EVT-004

Food, supplies, equipment, help, transportation, accommodations, and similar coordination should preferably use one lightweight **Needs** concept rather than separate complicated systems. `REQ-EVT-011` makes the unified model and lightweight Food, Supplies, Help, Transport, and Other categories explicit for MVP, including setup and cleanup as coordination examples.

### REQ-EVT-005

The standard event flow is:

`Create Event → Basic Details → Choose item audience (per REQ-PRIV-020) → Invite Family → RSVP → Add Needs / Tasks → Members claim responsibilities → Organizer posts meaningful updates → Event occurs → Event completes`

Event creation must remain lightweight. This is a conceptual workflow, not a required multi-screen wizard. Choosing an Event audience does not send an invitation or create an RSVP.

### REQ-EVT-006

MVP Events exclude event chat, photo albums, advanced budgeting, ticketing, vendor management, complex room assignments, detailed equipment inventory, and social-media-style comments or reactions.

### REQ-EVT-007

An event may provide context for an optional Sharing Session. Sharing authorization, controls, and expiration are governed by `REQ-PRIV-001` through `REQ-PRIV-004`, `REQ-PRIV-023`, and the Sharing workflow. Event audience, invitation, RSVP, participation, organizer status, and Need responsibility do not authorize another person's Sharing Session data. Event Sharing integration requires a separately approved work item.

### REQ-EVT-008

At Event creation, the creator assigns each selected active adult member of the applicable Family Group either view or edit access under `REQ-PRIV-021`. The creator and selected editors may update the Event's name, description, date, time, and location. No other Event fields or permissions are editable in this initial slice. Audience and editor assignments are fixed after creation.

### REQ-EVT-009

For MVP, the Event creator is its sole organizer. Organizer responsibility is not transferred and there are no multiple organizers. The designated Event detail editors under `REQ-EVT-008` remain valid but do not thereby gain invitation or RSVP management, Need management, Update posting, or Sharing Session access. Each capability has its own explicit rule.

### REQ-EVT-010

Event audience means authorized Event visibility; an invitation is an explicit request to participate; a participant is an in-system person associated with Event participation; RSVP is that person's response to an active invitation. The organizer may invite only an in-system person who already has authorized Event visibility. An invitation never grants Event visibility. The organizer may revoke an invitation without removing Event visibility. Revocation makes the invitation inactive and its associated RSVP inactive and historical; further participation through it requires a new invitation. MVP invitations do not expire automatically, and Event date or time does not expire them. Event-completion handling for invitations and RSVP remains deferred.

An active invited in-system user controls and may change their own RSVP among Pending, Going, Maybe, and Not Going while the invitation is active. The organizer cannot RSVP on behalf of another adult. Non-system RSVP is deferred. RSVP does not change Event visibility or grant editing, Need management, Update posting, location, or Sharing Session access. Going status is not generally required for ordinary Event coordination; Need eligibility is defined separately by `REQ-EVT-011`.

### REQ-EVT-011

An Event Need is one lightweight coordination item with a required title, optional short detail, and optional category from Food, Supplies, Help, Transport, and Other. One person may be responsible for a Need in MVP. Structured inventory quantities, a due-time subsystem, multi-person responsibility, and complex reassignment are excluded.

A Need is Open when no one is responsible. An eligible invited in-system participant may voluntarily claim an Open Need, making it Claimed. The organizer may ask an eligible invited participant to take an Open Need, making it Assigned; Assigned is a request pending that person's explicit acceptance or decline, not forced consent, and the experience must communicate that pending acceptance. A Claimed or Assigned Need may become Completed. Release or decline returns it to Open. The organizer may create, edit, cancel/remove, assign, and mark a Need complete when appropriate. An eligible participant may claim an Open Need, release their own claim, accept/retain or decline/release their own assignment, and mark their own responsibility complete. Event detail editors do not automatically manage Needs; Need-manager delegation is deferred.

Claiming and organizer assignment require an active invitation, authorized Event visibility, eligible active membership, and RSVP Pending, Going, or Maybe. A person with RSVP Not Going may not take a new Need. When a responsible person changes RSVP to Not Going, or loses eligibility through invitation revocation, Event visibility loss, or eligible membership loss, any Need they hold as Claimed or Assigned automatically returns to Open without being completed or deleted. Prior responsibility, prior person, release reason, and release time remain in history/provenance. Later restored access or a new invitation does not restore responsibility automatically; normal claim or assignment is required. Need responsibility grants no broader Event or private-data access.

### REQ-EVT-012

MVP Event Updates are meaningful, time-ordered coordination announcements, not general chat. Only the organizer may post or edit their own Update. Each Update shows author and creation time; an edit changes the current visible text and records an updated time while retaining prior text for history/audit. The organizer may remove/hide their own Update from normal Event display; its record, author, timestamps, and prior revisions remain retained with the Event while the Event record exists. No public history or restore UI is required in MVP. Normal Event APIs must not disclose hidden Updates or old revisions, and retained history creates no alternate access path. A visible Update is available only to the current authorized Event audience; loss of Event access ends Update access. No separate Update audience, comments, replies, reactions, threads, or general-purpose chat are included. Future Event deletion/retention policy may define ultimate disposal of retained history; this requirement sets no broader retention policy.

### REQ-EVT-013

Event-context Check-Ins reuse the generic Check-In model rather than introducing Event-specific states. Event association and recipients require separate scope; a Check-In does not automatically start location sharing. Home may surface restrained, relevant Event attention such as RSVP requested, a Need assigned to the current user, a meaningful Event change, or an upcoming Event, rather than every open Need or action. Notifications are introduced with their owning capability and remain actionable and restrained under `REQ-NOTIFY-001`; this requirement approves no specific notification trigger or Event Sharing Session implementation.

### REQ-EVT-014

The first Event Invitations and Self RSVP slice is in-app only. It supplements `REQ-EVT-010`; current eligibility and named-data visibility are governed by `REQ-PRIV-028`. Event creation and audience selection create zero invitations. Only the organizer may issue or revoke invitations to eligible existing in-system adult Event viewers. The organizer may not invite themselves and receives no RSVP merely by creating the Event. At most one unrevoked invitation may exist for the same Event and invitee profile; temporary eligibility loss does not permit a duplicate.

A new invitation starts with RSVP Pending: a response has been requested, but creation is not an invitee response. The deliberate RSVP-updated timestamp (for example, `RsvpUpdatedAtUtc`) is absent until the invitee deliberately submits an RSVP. While currently eligible and unrevoked, the invited adult may freely choose or change their own RSVP among Pending, Going, Maybe, and Not Going, including deliberately selecting Pending. Revoked invitations cannot receive further RSVP changes.

Revoked invitations and their final RSVP remain inactive historical records retained for the parent Event record's lifecycle. A later re-invitation creates a new invitation record and ID. There is no independent RSVP revision-history table, public RSVP history, or separate invitation-retention timer. Future Event-level disposal/retention governs invitations unless a later explicit rule says otherwise; this does not define Event completion or a new parent retention policy. Retention supplies no alternate access path.

The existing Events dashboard RSVP/attention area is the discovery surface: it shows the signed-in adult's own currently eligible, active Pending invitations for the selected Family, each linking to the existing Event item URL. Event detail is the self-RSVP action surface and organizer invitation-management surface. Pending invitations remain discoverable after the Event date because dates do not expire invitations. This slice adds no Home attention, outbound email, push, SMS, notification infrastructure or center, preferences, or generic activity feed. `REQ-EVT-013` remains a future capability boundary rather than authorization for those surfaces.

## Completion

An event completes after it occurs and its active coordination needs are concluded. Invitation and RSVP handling at Event completion remains deferred under `REQ-EVT-010`. Retained Update history under `REQ-EVT-012` and invitation records under `REQ-EVT-014` follow the Event record lifecycle. No other archival or retention behavior is approved by this workflow.
