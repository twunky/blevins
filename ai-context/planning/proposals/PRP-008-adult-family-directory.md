# PRP-008 - Adult Family Directory

## Metadata

- **Proposal ID:** `PRP-008`
- **Status:** `ACCEPTED`
- **Date:** 2026-09-25
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.5`
- **Required context:**
  - `ai-context/README.md`
  - `ai-context/project-status.md`
  - `ai-context/core/product-contract.md#req-fam-001` through `#req-fam-003`
  - `ai-context/core/product-contract.md#req-fam-007` and `#req-fam-008`
  - `ai-context/core/privacy-safety-contract.md#req-priv-005`, `#req-priv-006`, `#req-priv-011`, and `#req-priv-013`
  - `ai-context/core/ux-contract.md#req-ux-003` and `#req-ux-006`
  - `ai-context/core/domain-glossary.md#family-group` through `#profile-field-visibility`
  - `ai-context/planning/stories/WI-005-001-adult-self-profile.md`
  - `ai-context/planning/stories/WI-005-002-adult-membership-invitations.md`
- **Context intentionally excluded:** Events, plans/trips, sharing/check-ins, safety workflows, bootstrap, archive, operator provisioning, minors/guardians, relationships, Household behavior, Safety Circle behavior, removal/rejoining, and retention policy
- **Affected authority:** Family-area behavior, profile access, privacy, and Phase 5 planning
- **Model tier:** `DEEP`

## Request

Define the smallest useful Family-area experience that lets an active adult member find other active adult members of the same Family Group and open the profile information already authorized for that viewer.

This proposal does not authorize implementation.

## Current evidence and context

- `REQ-FAM-001` requires member profiles in the Family area.
- `REQ-FAM-007` defines the only approved adult profile fields in the current slice.
- `REQ-PRIV-013` already defines exactly which profile values an active same-group member may receive and requires unauthorized values to be omitted server-side.
- `REQ-PRIV-005` and `REQ-PRIV-006` prohibit relationship or administrator status from becoming an access shortcut.
- `REQ-PRIV-011` requires active membership and ends access when membership ends.
- `REQ-UX-003` favors progressive disclosure, and `REQ-UX-006` requires large-family filtering and accessible operation.
- `WI-005-001` implemented owner and same-group shared-profile boundaries. `WI-005-002` implemented active adult membership creation.
- Detailed relationship, Household, Safety Circle, minor/guardian, member-removal, and former-member behavior is not approved.

## Impact

This slice makes the approved adult profile and membership foundations discoverable and useful without adding new profile data. It is privacy-sensitive because a directory can reveal membership and identifiers, so every query must be scoped to an active shared Family Group and every optional value must continue through the existing field-level authorization boundary.

## Requirements

- Show only active adult members of a Family Group in which the current account has active membership.
- Return a minimal directory index containing only stable profile identity needed for selection: profile ID and display name.
- Load optional shared profile values only after the user selects a member, using the existing server-side visibility rules.
- Never expose sign-in identifiers, invitation delivery addresses, visibility settings, administrator assignments, capacity, private optional values, inactive/former members, or unrelated groups.
- Provide case-insensitive display-name filtering suitable for the validated Family Group ceiling and an understandable empty state.
- Sort members predictably by display name with a stable tie-breaker.
- Use a Family Group selector only when the current adult has more than one active Family Group; do not create public group discovery.
- Revalidate authorization server-side for both directory results and profile detail access.
- Preserve keyboard operation, visible labels, focus clarity, and understandable loading/error states.

## Non-goals

- Adding or editing profile fields, photos, addresses, location, biography, pronouns, medical data, emergency data, or arbitrary custom fields.
- Defining or displaying relationship labels, family trees, Household membership, Safety Circle membership, trusted contacts, or minors/guardians.
- Administrator-only directory data, member management, removal, suspension, reactivation, rejoining, or role transfer.
- Invitations, operator provisioning, identity-provider behavior, production hosting, notifications, Events, Plans/Trips, Sharing, or Safety workflows.
- Public discovery, cross-family search, global people search, fuzzy identity matching, or external contact synchronization.

## Conflicts, unknowns, and decisions required

- **Conflicts:** None detected in the loaded authoritative context.
- **Unknowns:** The source does not prescribe a final visual layout or whether a person with multiple active Family Groups sees a selector or separate group sections.
- **Decision required:** Approve the minimal index-plus-on-demand-detail behavior and the conditional selector recommended below. Final visual styling may be refined within existing UX rules.

## Options

### Option A - Minimal directory with on-demand profile detail

- **Description:** Present a searchable, alphabetically ordered list of active adult display names for one active Family Group. Selecting a person loads only the profile fields authorized for the current viewer. If the current adult belongs to multiple active groups, show an explicit Family Group selector; otherwise hide it.
- **Benefits:** Leanest useful slice, least data exposure, reuses existing profile authorization, supports large families, and leaves undefined relationship/Household concepts untouched.
- **Risks/tradeoffs:** Requires one extra selection to see shared details and intentionally provides no family-structure visualization.

### Option B - Rich directory cards

- **Description:** Return authorized contact and birthday values for all visible members in the initial directory response and display them in cards.
- **Benefits:** Fewer interactions when scanning contact details.
- **Risks/tradeoffs:** Discloses more data at once, increases payload and visual noise, and works against progressive disclosure.

### Option C - Defer the directory until relationships and Households are defined

- **Description:** Do not expose a Family member list until richer family structure is approved.
- **Benefits:** A later unified design could combine several concepts.
- **Risks/tradeoffs:** Leaves completed membership and shared-profile behavior difficult to use and couples a simple approved capability to undefined higher-risk work.

## Recommendation

Approve **Option A**. It is the smallest coherent next Phase 5 slice, directly operationalizes approved requirements, minimizes private-data exposure, and avoids every currently undefined Family-area concept.

If accepted, create `WI-005-003` for the directory index, conditional active-group selector, on-demand shared-profile detail, Angular experience, and focused authorization/accessibility verification. No canonical baseline change appears necessary because the proposal selects a bounded implementation of existing approved requirements rather than changing them.

## Verification plan

- Unit and application tests for active/shared membership authorization, inactive/former/unrelated denial, deterministic ordering, and case-insensitive filtering.
- API tests proving the index exposes only profile ID and display name and detail responses omit unauthorized values.
- Persistence tests for multi-group scoping and current membership revalidation.
- Angular tests for one-group and multi-group behavior, filtering, selection, empty/loading/error states, labels, and keyboard-operable native controls.
- Regression tests for owner profile and invitation behavior.
- Build, formatting, dependency advisory, scope, Development runtime, and Production-safe runtime checks.
- Story-level verification mapping every approved criterion and requirement; label self-verification honestly if an independent verifier is unavailable.

## Decision and authorization

- **Decision:** `ACCEPTED`
- **Decision ID:** `DEC-033`
- **Approved by:** Project owner
- **Approved at:** 2026-09-25
- **Execution authorized:** `NO`
