---
Status: APPROVED
Authority: CANONICAL WORKFLOW — CTX-1.3
Context-Baseline: CTX-1.32
Version: 1.2
Source: ../bootstrap/product-ux-source.md; DEC-057; DEC-086
Constrained-By:
  - ../core/product-contract.md
  - ../core/ux-contract.md
  - ../core/privacy-safety-contract.md
---

# Sharing and Check-ins Workflows

All sharing behavior is constrained by the voluntary consent, audience, duration, and privacy rules in `../core/privacy-safety-contract.md`.

## Sharing Sessions

### REQ-SHARE-001

A Sharing Session is a temporary context that combines what is shared, who can see it, why it is shared, how frequently it updates, when it begins, and when it ends.

### REQ-SHARE-002

Supported sharing modes are:

1. No location sharing
2. Check-in only
3. Periodic location sharing
4. Temporary real-time location sharing
5. Trip-based sharing
6. Event-based sharing

### REQ-SHARE-003

Sharing dimensions remain independently understandable and selectable:

- **What:** Status, check-ins, approximate or current location, periodic location, temporary live location, destination, or itinerary.
- **Who:** Safety Circle, selected family, trip participants, or event participants.
- **Why:** Trip, event, plan, or temporary activity.
- **How long:** Until arrival, trip end, event end, a fixed duration, a selected time, or manual stop.

### REQ-SHARE-004

Events, Plans, Trips, and temporary activities may provide context for a Sharing Session. Authorization is governed by `REQ-PRIV-004`.

### REQ-SHARE-005

Active Sharing Sessions must expose the clarity required by `REQ-UX-009` and must allow the user to stop sharing under `REQ-PRIV-003`.

`OD-002` remains open. See `../decision-log.md#open-product-decisions`.

## Check-ins

### REQ-SHARE-006

A Check-In must be fast and simple and may communicate **I'm okay**, **Arrived**, **Leaving now**, **Delayed**, or **Plans changed**.

### REQ-SHARE-007

A Check-In may optionally include a note, a voluntarily included current location, or an updated expected arrival.

### REQ-SHARE-008

The standard missed expected check-in flow is:

`Expected check-in missed → Reminder to the individual → Optionally notify designated trusted contacts according to preferences → Family may raise a Concern if appropriate`

This flow must preserve the uncertainty rule in `REQ-SAFE-001`; it does not automatically create a Concern or Safety Incident.

### REQ-SHARE-009

Trip context does not grant Sharing Session or another person's Check-In access. A Trip audience member receives only the Trip information allowed by the Trip audience rules; access to another person's location, Check-In information, or Sharing Session data requires separate authorization under the applicable privacy and sharing rules. Each person controls Sharing Sessions involving their own information. A Trip creator cannot start or manage a Sharing Session for another person's information merely by creating, organizing, or completing the Trip.

### REQ-SHARE-010

The first standalone Check-In slice is self-authored creation and read-only history. It uses exactly the five statuses in REQ-SHARE-006 and an optional plain-text note under REQ-SHARE-007. Status, note and explicitly chosen recipients are fixed at creation; post-creation editing, re-sharing, deletion and restore are excluded. Historical records remain stored without an invented expiry; their availability follows REQ-PRIV-030. No location, expected-arrival update, Event/Plan/Trip association, Sharing Session dependency, reminder, notification or automatic Concern/Incident/safety inference is included. A status is the author's report, not verified safety.
