---
Status: ACTIVATED — historical candidate record
Authority: Historical candidate; active canonical wording is in the referenced contracts and workflows
Context-Baseline: CTX-1.17
Version: 1.0-candidate
Decision: DEC-057
---

# Candidate CTX-1.18 — Trip reconciliation

This historical candidate records the Trip-specific context update approved by the project owner and activated as `CTX-1.18` after passing consistency verification. It adds no admin-configured audience subsystem and does not authorize Trip application implementation. Active normative wording is in `workflows/plans-trips.md`, `core/privacy-safety-contract.md`, and `workflows/sharing-checkins.md`.

## Proposed Trip workflow additions

- `REQ-TRIP-007`: Creator is organizer; creator need not be a traveler; traveler, viewer, and contributor permissions are separate; traveler status grants no access by itself; creator access persists after creator membership loss; no ownership transfer; audience editing is separately authorized.
- `REQ-TRIP-008`: Require title, one primary destination, start/end dates, at least one eligible traveler, and explicit audience. End date cannot precede start date. Other approved fields are optional. Audience modes are product-defined; admin configuration and multiple destinations are deferred.
- `REQ-TRIP-009`: Non-system contacts are informational only, with required name and optional role/relationship, phone, and email; contact details follow Trip audience visibility.
- `REQ-TRIP-010`: Audience members read/add Trip Notes; authors edit/delete their own notes; creator may hide another author's note from normal display but cannot rewrite it. Hidden notes remain retained for history/audit. No restore UI or full revision history is defined.
- `REQ-TRIP-011`: Expected arrival is optional; Check-Ins are personal and voluntary; only creator marks completion; dates/missing location do not automatically complete Trips or raise Concerns; Trip status does not itself authorize Sharing Sessions.
- `REQ-TRIP-012`: MVP flow is Trips Overview → Create Trip → Trip Detail. A separate review screen and rich filters/status/quick actions remain UX guidance or prototype ideas.

## Proposed privacy addition

- `REQ-PRIV-022`: Trip audience is exactly one applicable Family Group or selected eligible active adult members from that group. Group audience is snapshotted at selection. New members do not gain access automatically; membership loss revokes audience access; rejoining does not restore it automatically. Creator retains access even after creator membership loss and no ownership transfer occurs. Creator may explicitly change audience via a separately authorized capability with immediate enforcement and disclosure of access changes. Trip visibility does not grant Sharing Session or separately protected Check-In data access. Plan and Event rules are unchanged.
- `REQ-PRIV-011` clarification: Creator's own-Trip access after membership loss is a narrow `REQ-PRIV-022` exception; it restores no Family Group membership or access to other group data.

## Proposed Sharing workflow addition

- `REQ-SHARE-009`: Trip context/audience does not authorize another person's Check-In or Sharing Session data. Each person controls Sessions involving their own information.
- The Trip creation flow does not itself initiate or authorize a Check-In or Sharing Session.

## Boundaries

- No admin-configured audience presets, multi-group handling, combined group-plus-selected audience, non-system viewer access, or multiple destinations.
- No Trip implementation stories, code, or application changes are authorized by this context update.
- Existing `REQ-PLAN-005`, `REQ-PLAN-006`, Event requirements, and completed Plan/Event work remain unchanged.
