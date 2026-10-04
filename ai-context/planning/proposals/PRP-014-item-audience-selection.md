# PRP-014 — Explicit audience selection for shareable items

## Metadata

- **Proposal ID:** `PRP-014`
- **Status:** `ACCEPTED; CANONICAL CTX-1.12`
- **Date:** `2026-09-26`
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.12` (approved and active under `DEC-049`)
- **Required context:**
  - `README.md` and `project-status.md` — operating rules and current phase
  - `core/product-contract.md#req-fam-002` — shared dates and trusted contacts
  - `core/privacy-safety-contract.md#req-priv-001` through `#req-priv-008`, `#req-priv-016`, and `#req-priv-017` — sharing consent, access boundaries, and shared-date exception
  - `workflows/events.md#req-evt-001`, `#req-evt-005`, and `#req-evt-007` — event invitations and sharing context
  - `workflows/plans-trips.md#req-plan-002`, `#req-plan-003`, `#req-trip-002`, `#req-trip-003`, and `#req-trip-004` — plan/trip audience and sharing settings
  - `workflows/sharing-checkins.md#req-share-001` through `#req-share-005` — independently authorized Sharing Session audiences
- **Affected authority:** Cross-feature visibility/audience choice for user-created coordination items
- **Model tier:** `DEEP`

## Request

Consider a consistent rule requiring the person who creates a shareable item to choose its audience, using whole Family Group(s), specific people, or both.

## Current evidence and context

- The project owner proposed explicit audience selection when adding an event, travel item, or similar shareable content, with whole-group and specific-person choices.
- `REQ-EVT-001` requires events to invite family members; `REQ-EVT-005` includes an Invite Family step but does not define a required audience-selection control.
- `REQ-PLAN-002` already identifies an audience as possible Plan information; `REQ-TRIP-002` and `REQ-TRIP-003` include sharing settings.
- Sharing Sessions already require independent audience selection under `REQ-SHARE-001` and `REQ-SHARE-003`; item audience must not silently authorize location sharing.
- Shared Family Dates have narrower, explicitly approved visibility under `REQ-FAM-010` / `REQ-PRIV-016`, as amended by `CTX-1.10`. A broad family audience cannot override that contract without a separate decision.
- The approved privacy rules say family relationship, membership administration, or participation alone does not grant access; intentional selection must remain explicit and bounded by the relevant feature contract.

## Impact

This rule would affect who can view a user-created event, Plan, Trip, or other explicitly included item. It intersects with Family Group membership and item-specific invitation/participant rules. It must stay separate from location-sharing authorization, trusted-contact designation, notification consent, and existing narrower visibility rules.

## Proposed requirements

- For each included item type, require its creator to make an explicit audience choice when creating the item; do not silently infer an audience from family relationships, participation, or prior sharing.
- Offer whole eligible Family Group(s), specific eligible people, or both, only where the item's own contract supports those audiences.
- Preserve stricter feature-specific visibility and privacy rules. This proposal does not broaden Shared Family Date visibility or authorize access to profile fields, Sharing Sessions, Safety Circle information, or safety records.
- Do not treat audience selection as authorization for location sharing or notifications; those retain their own consent and preference rules.
- Decide whether audiences may include people outside the creator's active Family Group(s), and how audience changes affect an existing item.

## Non-goals

- Changing the already approved Shared Family Date audience rules.
- Selecting recipients for Trusted Contact records; `PRP-013`/candidate `CTX-1.11` governs that separate concept.
- Changing Family Group membership, invitations, roles, or administrator authority.
- Authorizing location sharing, notification delivery, or access to private profile, Safety Circle, Concern, or Safety Incident information.
- Defining technical implementation or a universal audience model where item-specific contracts differ.

## Conflicts, unknowns, and decisions required

- **Conflicts:** None if existing feature-specific rules remain controlling. Applying “whole family” to Shared Family Dates would conflict with the approved creator-and-associated-members rule; Shared Family Dates are therefore excluded unless separately reconsidered.
- **Unknowns:** Exact included item types; how to select among multiple Family Groups; whether individual recipients must be active members of an eligible group; whether external people are permitted; whether group and person selections combine as a union; whether audience choice can be edited later and by whom; empty/private audience behavior; and what happens when membership ends.
- **Decision:** The project owner approved Option A for Events, Plans, and Trips. Recipient eligibility, multiple-group behavior, later audience changes, and access after membership changes are explicitly deferred to a later phase or decision. These open items do not authorize assumptions for implementation.

## Options

### Option A — Explicit per-item audience (recommended)

- **Description:** At creation, the creator selects whole eligible Family Group(s), specific eligible people, or both, from audiences the item-specific contract permits. The saved item records that explicit choice. Existing narrower rules remain in force; Sharing Sessions and notifications require their own authorization. Initially consider Events, Plans, and Trips; keep Shared Family Dates excluded under their specific contract.
- **Benefits:** Matches the owner's requested choice, avoids silent broad sharing, and can be applied consistently without collapsing feature-specific rules.
- **Risks/tradeoffs:** Requires decisions about eligible recipients, multiple groups, update rights, and membership changes. Requiring selection may add a step to item creation.

### Option B — Feature-specific audiences only

- **Description:** Continue defining visibility independently for Events, Plans, Trips, and other items, without a shared cross-feature rule.
- **Benefits:** Avoids premature generalization and allows each feature to fit its workflow.
- **Risks/tradeoffs:** Repeats privacy and audience decisions across features; users may encounter inconsistent sharing choices.

## Recommendation

Choose Option A as a reusable product rule, initially for Events, Plans, and Trips, with the explicit exclusions above. Keep “whole group and/or selected people” as the proposed audience choices, but resolve recipient eligibility, multiple-group behavior, later audience edits, and membership-loss effects before implementation. Do not apply it to Shared Family Dates or location/notification sharing without separate decisions.

## Verification plan

After the owner selects the scope, compare each affected workflow against its existing audience/invitation steps and privacy requirements. Verify that whole-group and specific-person choices cannot exceed item-specific authorization; Shared Family Date visibility is unchanged; Family membership or administrator status does not independently grant private access; and audience selection does not activate location sharing or notifications.

## Decision and authorization

- **Decision:** Option A accepted; candidate context prepared under `CTX-1.12`.
- **Decision ID:** `DEC-048`
- **Approved by:** Project owner
- **Execution authorized:** Context baseline activation only; no application implementation.
- **Baseline:** `CTX-1.12` is approved and active under `DEC-049`.
