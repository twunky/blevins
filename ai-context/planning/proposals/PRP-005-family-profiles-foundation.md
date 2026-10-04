# PRP-005 — Family & Profiles Foundation

## Metadata

- **Proposal ID:** `PRP-005`
- **Status:** `ACCEPTED`
- **Date:** 2026-09-25
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.3`
- **Required context:**
  - `ai-context/README.md`
  - `ai-context/project-status.md`
  - `ai-context/core/product-contract.md#req-fam-001` through `#req-fam-006`
  - `ai-context/core/privacy-safety-contract.md#req-priv-005` through `#req-priv-012`
  - `ai-context/core/domain-glossary.md#people-and-family`
  - `ai-context/core/ux-contract.md#req-ux-003` and `#req-ux-006`
  - `ai-context/decision-log.md#open-product-decisions`
  - `ai-context/decision-log.md#dec-021-authentication-deferred-and-application-authorization-owned-internally`
- **Affected authority:** Product, privacy/access, Family concepts, UX, and later Phase 5 work items
- **Model tier:** `DEEP`

## Request

Define the smallest safe, implementation-ready first slice of Phase 5 — Family & Profiles without guessing unresolved product behavior or using family/admin status as a private-data access shortcut.

## Current evidence and context

- `REQ-FAM-001` requires profiles, family relationships, household/basic grouping, contact information, privacy settings, and Safety Circle in the Family area.
- `REQ-FAM-002` permits shared dates and trusted contacts only under applicable privacy rules.
- `REQ-FAM-003` excludes genealogy, family-tree visualization, ancestry research, complex custom groups, and a family-history archive.
- `REQ-FAM-004` fixes operator-configured capacity targets at 50 planned, 100 default, and 250 validated ceiling.
- `REQ-FAM-005` requires private invitation-only membership and explicit acceptance.
- `REQ-FAM-006` requires controlled operator provisioning for the first Family Group and first administrator invitation.
- `REQ-PRIV-005` through `REQ-PRIV-008` prohibit family, administrator, Safety Circle, Concern, or incident context from becoming an access shortcut.
- `REQ-PRIV-011` separates signed-in identity, invitation state, active membership, administrator responsibility, and private-information access.
- `REQ-PRIV-012` prevents application-operator responsibility from granting routine private-content access.
- `DEC-021` keeps authentication replaceable and product authorization inside the application.
- The canonical source does not yet define exact profile fields, field visibility, relationship vocabulary, minimum household behavior, invitation channel/lifetime, sign-in/recovery, operator support procedures, or minor/guardian behavior.

## Impact

Family and Profile concepts become the identity and membership boundary used by later Events, Plans/Trips, Sharing, and Safety work. Incorrect early choices could create privacy leaks or encode family/admin relationships as authorization. These decisions are high-impact but still reversible before implementation begins.

## Requirements

- Preserve the distinction among Account, Family Member, active membership, Family Administrator responsibility, Membership Invitation, Application Operator, Household, and Safety Circle.
- Keep application authorization server-side and independent of authentication-provider claims except for signed-in account identity.
- Keep Family Group membership private, invitation-only, revocable before acceptance, and inactive until explicit acceptance.
- Enforce capacity through application-operator configuration rather than Family Administrator control.
- Use progressive disclosure and support older or less technical family members.
- Define every first-slice field and visibility rule explicitly before implementation.
- Block all minor/guardian behavior until `OD-003` is resolved.

## Non-goals

- Implementing Phase 5 code under this proposal.
- Choosing an authentication provider, invitation delivery provider, hosting, deployment, or production database edition.
- Defining minor/guardian behavior.
- Implementing Events, Plans/Trips, Sharing, Safety, notifications, or location behavior.
- Implementing genealogy, family-tree visualization, ancestry, complex custom groups, or family-history archives.
- Treating administrator, operator, relationship, household, or Safety Circle status as automatic private-data access.

## Conflicts, unknowns, and decisions required

- **Conflicts:** None detected among the loaded authoritative sources.
- **Unknowns:** Exact adult profile fields; per-field visibility and editing rules; relationship vocabulary and direction; minimum Household behavior; invitation channel and lifetime; sign-in/recovery; operator support process; minor/guardian policy.
- **Decision required:** Approve the recommended adult-first sequence, then explicitly define the minimum adult profile fields and visibility rules before creating an executable work item. Invitation, bootstrap, Household, relationships, and Safety Circle should receive later bounded work unless their missing decisions are first resolved.

## Options

### Option A — Adult self-profile foundation first

- **Description:** First define and implement an adult Account-to-Family-Member self-profile slice with explicit fields and visibility rules. Preserve membership and authorization boundaries, but defer invitation delivery, operator bootstrap mechanics, Household details, relationship taxonomy, Safety Circle behavior, and minors.
- **Benefits:** Smallest privacy-reviewable slice; establishes identity/profile boundaries without depending on unresolved invitation/provider behavior.
- **Risks/tradeoffs:** Does not yet create an end-to-end multi-member Family Group onboarding flow.

### Option B — Invitation and membership lifecycle first

- **Description:** Begin with Family Group creation, administrator invitations, acceptance, revocation, and active membership.
- **Benefits:** Moves toward multi-member onboarding sooner.
- **Risks/tradeoffs:** Not implementation-ready because invitation channel/lifetime, sign-in/recovery, operator support, and exact identity association remain undecided.

### Option C — Implement the full Family area together

- **Description:** Combine profiles, relationships, Household, contacts, privacy preferences, Safety Circle, invitations, membership, and bootstrap.
- **Benefits:** Broader Family-area coverage in one delivery.
- **Risks/tradeoffs:** Too large for the lightweight process and would force guesses across multiple privacy-sensitive unknowns.

## Recommendation

Choose **Option A**. First approve a minimal adult profile field/visibility contract, then create one lightweight implementation work item for that bounded slice. Keep the broader Phase 5 requirements visible but do not bundle them into the first execution.

## Verification plan

An approved implementation work item must independently map each selected field and operation to canonical requirements and demonstrate:

1. account identity does not imply Family Group membership;
2. family relationship and administrator responsibility do not grant private-data access;
3. only explicitly approved adult fields and visibility rules exist;
4. minor/guardian paths are absent or explicitly blocked;
5. server-side authorization denies cross-member access without a specific approved rule;
6. builds, automated tests, formatting, and dependency checks pass; and
7. self-verification is labeled if independent verification is unavailable.

## Decision and authorization

- **Decision:** `ACCEPTED` — Option A, adult self-profile foundation first
- **Decision ID:** `DEC-027`
- **Approved by:** Project owner
- **Approved at:** 2026-09-25
- **Execution authorized:** `NO`
