# PRP-020 — Trip Context Reconciliation

## Metadata

- **Proposal ID:** `PRP-020`
- **Status:** `ACCEPTED — canonical context activated as CTX-1.18`
- **Date:** `2026-09-27`
- **Requested by:** Project owner
- **Decision:** `DEC-057`
- **Previous baseline:** `CTX-1.17`
- **Context baseline:** `CTX-1.18`
- **Model tier:** `DEEP` (Trip privacy, permissions, and audience lifecycle)

## Purpose

Record the approved Trip roles, audience/privacy behavior, required and optional fields, informational contacts, collaborative Notes, expected-arrival/check-in boundaries, completion, and simple creation flow before Trip implementation planning. The recovered Product/UX handoff and `PRP-019` were review inputs; canonical authority comes from the approved decisions recorded in `DEC-057` and the context baseline after activation.

## Accepted owner direction

- Creator is organizer; no separate permission-bearing organizer role. Creator need not travel and retains access after losing group membership. No ownership transfer.
- Travelers are eligible existing in-system adult Family Group members, at least one required. Traveler status alone grants no Trip view/edit/Notes/Sharing Session access.
- The audience is exactly one applicable Family Group or selected eligible people from that group. No multi-group, combined audience, or non-system viewers.
- Family Group audience is a snapshot at audience selection. New members do not gain access to existing Trips; lost membership revokes access; rejoining does not restore it. Creator must explicitly add a person again or refresh/change the audience.
- Creator may explicitly change audience after creation through a separately authorized capability. Changes immediately enforce access and must identify access effects before confirmation. No Sharing Session access is granted.
- Audience modes are product-defined. Admin-configured presets are deferred.
- Creator can edit Trip fields and completion status. Audience members do not edit core fields.
- Non-system contacts are informational only: required name, optional role/relationship, phone, and email; contact details follow Trip audience. No login, permissions, Notes, or Sharing Session participation.
- Trip Notes are collaborative but not chat. Audience members read/add; author edits/deletes own; creator can hide another's note from normal display but cannot rewrite its content. Hidden notes are retained for history/audit. No restore UI or full revision-history feature is defined.
- Required fields: title, primary destination, start date, end date, at least one traveler, and audience. End date cannot precede start date. Description, transportation notes, accommodation notes, contacts, and expected arrival are optional. Multiple destinations deferred.
- Expected arrival is distinct from Trip dates and completion. Check-Ins remain personal and voluntary. Missed arrival is uncertainty. Creator explicitly marks completion; end date or missing location does not automatically complete a Trip or raise a Concern.
- Trip audience and Sharing Session audience are separate. Each person controls Sessions involving their own information.
- MVP create flow: Trips Overview → Create Trip → Trip Detail. Sections/dialogs are allowed. Review screen, filters, status chips, traveler avatars, and quick actions remain UX guidance/prototype concepts.

## Candidate context and verification

The proposed decisions are represented by `REQ-TRIP-007` through `REQ-TRIP-012`, `REQ-PRIV-022`, and `REQ-SHARE-009`. The cross-cutting privacy addition explicitly preserves Plan and Event behavior. See `planning/candidates/CTX-1.18-trip-reconciliation.md` for the candidate record and `planning/verification/VER-CTX-1.18.md` for consistency evidence. `CTX-1.18` was activated after the passing consistency verification.

## Authorization boundary

- **Context update:** Explicitly approved for preparation and consistency verification under `DEC-057`.
- **Trip application implementation:** Not authorized.
- **Trip implementation story/work item:** Not created or authorized by this proposal.
- **Plan/Event code and completed work:** Unchanged.

## PRP-019 disposition

`PRP-019` is superseded as a decision proposal by this approved direction. Its admin-configured audience presets are deferred; one-group audience, required title/destination/dates/traveler, traveler/viewer separation, optional expected arrival, informational contacts, revised Trip Notes, and creator audience changes are resolved by `DEC-057` and candidate context. It does not authorize implementation.
