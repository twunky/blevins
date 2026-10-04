# Proposal — Event Editing Access

## Metadata

- **Proposal ID:** `PRP-015`
- **Status:** `ACCEPTED`
- **Date:** `2026-09-26`
- **Requested by:** Project owner direction in the Phase 6 discussion
- **Context baseline:** `CTX-1.12`
- **Required context:**
  - `ai-context/README.md` — decision and authorization rules
  - `ai-context/project-status.md` — active checkpoint
  - `ai-context/workflows/events.md#req-evt-001` and `#req-evt-005` — current Event scope and sequence
  - `ai-context/core/privacy-safety-contract.md#req-priv-005`, `#req-priv-006`, `#req-priv-011`, and `#req-priv-020` — privacy and audience constraints
  - `ai-context/planning/stories/WI-006-001-event-creation-and-audience.md` — completed creation slice boundaries
- **Affected authority:** Privacy-safety contract and Events workflow
- **Model tier:** `DEEP`

## Request

Allow an Event creator to designate family members who may edit event details, consistent with the creator’s earlier direction that event setup should control who can see and edit it.

## Current evidence and context

- `WI-006-001` implements Event creation and selected-member visibility. It explicitly excludes editing permissions and post-creation audience changes.
- `REQ-PRIV-020` says an Event audience choice governs sharing of that item and does not itself establish invitation/RSVP or other permissions.
- `REQ-PRIV-005`, `REQ-PRIV-006`, and `REQ-PRIV-011` require access to be explicit, separate from administrator status, and limited by active membership.
- The project owner has deferred multi-group handling and audience changes, and described the initial application as a small family group where rules can be simpler.

## Impact

This adds a new permission beyond viewing and changes the Events workflow and privacy contract. Editors could change event details seen by the selected audience. The proposal intentionally limits any future implementation to one applicable Family Group and the existing adult membership boundary. It does not authorize implementation or broaden the Event audience.

## Requirements

- At Event creation, the creator can assign each selected active adult family member either **Can view** or **Can edit** access.
- A person with edit access necessarily has view access; the creator always retains edit access.
- Only the creator and explicitly selected editors may change Event name, description, date, time, and location.
- Event audience selection and event editing access are distinct. Choosing someone as a viewer does not grant edit access.
- Only an active member of the same applicable Family Group can use an Event permission. Family Administrator status alone grants no access.
- Permission assignment is fixed after creation in this initial slice. No later audience or editor changes are included.

## Non-goals

- Multi-group or cross-group editors.
- Non-member friends, minors, or guardian behavior.
- Editing the audience or editor assignments after creation.
- Event deletion, cancellation, invitation delivery, RSVP, Needs/Tasks, claims, updates, notifications, or location sharing.
- Administrator override or a general-purpose role/permission system.

## Conflicts, unknowns, and decisions required

- **Conflicts:** Current `REQ-PRIV-020` addresses audience sharing only; it does not define edit permission. Canonical context must be updated before implementation.
- **Unknowns:** None required for this bounded proposal. Event deletion/cancellation and post-creation permission changes remain deferred.
- **Decision required:** Accept or reject the proposed per-person view/edit choice at creation, including the stated restrictions and canonical privacy/workflow update. Acceptance must explicitly authorize preparation of the canonical update and the bounded implementation work item; implementation will not begin until those artifacts are approved.

## Options

### Option A — Per-person view/edit choice at creation (recommended)

- **Description:** For each active adult in the one applicable group, assign view or edit. Editors automatically have view access. The creator retains edit access. Freeze assignments after creation.
- **Benefits:** Directly supports the creator’s stated need with a simple family-facing control; clearly separates viewing from editing.
- **Risks/tradeoffs:** Requires a privacy-contract update and server-side authorization on every edit; permission changes are deferred.

### Option B — Keep Events read-only after creation

- **Description:** Do not add collaborator editing; only the creator can edit or event details remain fixed.
- **Benefits:** No new shared-edit permission surface.
- **Risks/tradeoffs:** Does not meet the stated request to designate family members who can edit an event.

## Recommendation

Accept Option A. It reflects the project owner’s direction while keeping the first implementation simple: one group, active adults, creator-controlled access at creation, and no later permission changes. Event content edits must be authorized server-side for the creator or selected editor, after confirming the requester’s active group membership.

## Verification plan

If accepted, verify the canonical privacy/workflow update independently before implementation. The implementation work item should test creator edits, selected editor edits, view-only member rejection, unrelated group member rejection, inactive membership rejection, and the lack of administrator bypass. Verify the creation UI distinguishes view and edit access without implying invitations or RSVP state. Run the migration/model checks, focused API/persistence tests, the Angular suite, and production build. Disclose any unavailable SQL integration environment.

## Decision and authorization

- **Decision:** `ACCEPTED`
- **Decision ID:** `DEC-051`
- **Approved by:** Project owner
- **Approved at:** 2026-09-26
- **Execution authorized:** `YES, bounded to canonical context update and preparation of WI-006-002; implementation requires separate approval and authorization`
