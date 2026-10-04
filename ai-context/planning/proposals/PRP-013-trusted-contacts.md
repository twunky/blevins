# PRP-013 — Trusted Contacts

## Metadata

- **Proposal ID:** `PRP-013`
- **Status:** `ACCEPTED OPTION A; ACTIVE UNDER CTX-1.12`
- **Date:** 2026-09-26
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.10`
- **Required context:**
  - `README.md` and `project-status.md` — operating rules and current phase
  - `core/product-contract.md#req-fam-001`, `#req-fam-002`, and `#req-fam-007` — Family scope, trusted contacts, and existing self-profile contact fields
  - `core/privacy-safety-contract.md#req-priv-005`, `#req-priv-006`, `#req-priv-007`, `#req-priv-008`, `#req-priv-011`, and `#req-priv-013` — access, administrator, Safety Circle, and profile privacy boundaries
  - `workflows/sharing-checkins.md#req-share-008` — approved missed-check-in flow referencing designated trusted contacts
- **Affected authority:** Phase 5 Family capability and privacy rules for Trusted Contacts
- **Model tier:** `DEEP`

## Request

Define a narrow adult Trusted Contacts capability that satisfies the approved Family-area scope without conflating trusted contacts with profile contact fields, Family membership, Safety Circle, or automatic notification permission.

## Current evidence and context

- `REQ-FAM-001` includes contact information in the Family area.
- `REQ-FAM-002` permits trusted contacts only under applicable privacy rules; it does not define fields, ownership, visibility, or lifecycle.
- `REQ-FAM-007` defines an adult's own optional contact email and phone. These are profile fields and must remain distinct from information about another person.
- `REQ-PRIV-005` and `REQ-PRIV-006` say family relationship or administrative responsibility does not imply private-data access.
- `REQ-PRIV-007` establishes that Safety Circle membership is individually selected; it is not automatically granted by Family Administrator status.
- `REQ-SHARE-008` permits optional notification of designated trusted contacts after a missed expected check-in, according to preferences. The designation, permission, and detail-sharing rules are not defined.
- No detailed Trusted Contacts contract or workflow exists in the active baseline.

## Impact

Trusted Contacts may include personal contact information about someone other than the account holder. Visibility, use, and notification behavior therefore need explicit owner control. This proposal does not authorize a contact to access Family Group data or receive information merely because they are listed.

## Proposed requirements

- Define Trusted Contacts as a concept separate from an adult's own profile contact fields, Family membership, Family Relationships, and Safety Circle.
- Decide who owns and maintains contact entries, whether a contact may be outside the Family Group, and who may see each entry.
- Decide whether a listed contact is only a directory entry or may also be designated for a particular notification/use; listing alone should not be treated as notification authorization.
- Define the smallest approved contact fields and the entry's create, edit, remove, and access-ending behavior.
- Preserve active-membership and private-data boundaries; administrator status must not create contact visibility or override owner choices.

## Non-goals

- Implementing contact storage, screens, notifications, or delivery.
- Changing adult self-profile contact email/phone behavior.
- Replacing or expanding Safety Circle behavior.
- Defining minor/guardian or delegated authority.
- Defining general retention, audit, backup, or cleanup operations.

## Conflicts, unknowns, and decisions required

- **Conflicts:** None identified in the loaded approved context. The current workflow references trusted-contact notification according to preferences, but does not define the preference or authorization model.
- **Unknowns:** Ownership; family-member versus external contacts; visibility; required fields; whether contacts can be designated for a use or notification; consent/permission expectations; lifecycle and retention.
- **Decision required:** Review the options and choose a bounded v1 ownership, visibility, field, and use model before canonical requirements or an implementation work item are prepared.

## Options

### Option A — Personal contacts with explicit use (recommended)

- **Description:** Each adult manages their own Trusted Contact entries. Entries are private to that adult by default. Explicit sharing or designation is a separate choice for a defined use; merely being listed grants no Family access and does not automatically authorize notifications. Whether contacts may be non-members and the exact available audiences/uses must be decided.
- **Benefits:** Aligns with individual control, limits unnecessary disclosure, and keeps the contact directory distinct from Safety Circle and profile fields.
- **Risks/tradeoffs:** Requires explicit decisions about sharing and any notification use; does not create a universally visible Family directory.

### Option B — Family Group contact directory

- **Description:** Contact entries are maintained for use across a Family Group under explicitly selected visibility and editing rules.
- **Benefits:** Easier for members to find shared contact information.
- **Risks/tradeoffs:** Broadens disclosure of third-party information and requires group ownership, edit, consent, and former-member access rules not established in the current context.

## Recommendation

Choose Option A for v1. Keep entries owner-managed and private by default; require a separate explicit designation for any defined sharing or notification use. Treat whether non-members may be listed and which fields are supported as explicit decisions, not assumptions. Do not equate a Trusted Contact with a Safety Circle member.

## Verification plan

After the owner selects an option, compare the approved rule package with `REQ-FAM-002`, `REQ-PRIV-005` through `REQ-PRIV-008`, `REQ-PRIV-011`, `REQ-PRIV-013`, and `REQ-SHARE-008`. Verify that each visibility/use rule is explicit, no administrator or relationship override is introduced, and listing a contact does not implicitly authorize data access or notification. Record unresolved choices as blockers rather than filling them in.

## Decision and authorization

- **Decision:** Option A accepted and active under `CTX-1.12`.
- **Decision ID:** `DEC-047`
- **Approved by:** Project owner
- **Execution authorized:** Context baseline activation only; no application implementation.
- **Baseline:** `CTX-1.12` is approved and active under `DEC-049`.
