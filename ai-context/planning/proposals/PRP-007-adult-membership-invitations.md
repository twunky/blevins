# PRP-007 - Adult Membership Invitations

## Metadata

- **Proposal ID:** `PRP-007`
- **Status:** `ACCEPTED`
- **Date:** 2026-09-25
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.4`
- **Required context:**
  - `ai-context/README.md`
  - `ai-context/project-status.md`
  - `ai-context/planning/proposals/PRP-005-family-profiles-foundation.md`
  - `ai-context/core/product-contract.md#req-fam-004` through `#req-fam-006`
  - `ai-context/core/privacy-safety-contract.md#req-priv-005`, `#req-priv-006`, `#req-priv-011`, and `#req-priv-012`
  - `ai-context/core/domain-glossary.md#family-administrator` through `#application-operator`
  - `ai-context/core/ux-contract.md#req-ux-003` and `#req-ux-006`
  - `ai-context/decision-log.md#dec-016-private-invitation-only-family-group-membership`
  - `ai-context/decision-log.md#dec-018-controlled-first-family-group-bootstrap`
  - `ai-context/decision-log.md#dec-021-authentication-deferred-and-application-authorization-owned-internally`
- **Context intentionally excluded:** Events, plans/trips, sharing, safety, minors/guardians, Household, relationships, Safety Circle, bootstrap source, archive, and unrelated verification history
- **Affected authority:** Product membership behavior, privacy/access, Family terminology, and later Phase 5 work
- **Model tier:** `DEEP`

## Request

Define the smallest safe adult invitation and Family Group membership lifecycle that can run locally now, remain independent of a production identity or email provider, and expand later without weakening privacy or authorization.

This proposal does not authorize implementation.

## Current evidence and context

- `REQ-FAM-005` requires private, invitation-only membership for a specific intended person, explicit acceptance, and administrator revocation.
- `REQ-FAM-006` defines a separate controlled path for the first Family Group and first Family Administrator.
- `REQ-FAM-004` requires application-configured capacity enforcement.
- `REQ-PRIV-011` separates signed-in identity, invitation state, active membership, administrator responsibility, and access to Family Group data.
- `REQ-PRIV-005`, `REQ-PRIV-006`, and `REQ-PRIV-012` prohibit relationship, administrator, or operator status from becoming a private-data access shortcut.
- `DEC-021` keeps authentication replaceable and requires application-owned, server-side authorization.
- `WI-005-001` established separate Account, Family Group membership, and Family Administrator concepts, but did not implement invitations or production identity.
- Invitation delivery channel, lifetime, identity association, duplicate behavior, and operator support remain undecided.

## Impact

Invitation acceptance is the first operation that turns an outside signed-in account into an active Family Group member. An unsafe or ambiguous design could disclose membership, bind the wrong account, exceed capacity, or grant access before acceptance. The decision is privacy- and permissions-sensitive but can remain provider-neutral.

## Requirements

- Support adults only; `OD-003` continues to block every minor or guardian path.
- Keep invitation, signed-in account, active membership, and administrator assignment as separate states.
- Grant no Family Group access before successful explicit acceptance.
- Allow an active Family Administrator to issue and revoke invitations for that administrator's Family Group.
- Enforce the configured capacity when issuing and accepting invitations without letting an administrator change the limit.
- Prevent public discovery, join requests, automatic membership, and disclosure of whether unrelated private groups or invitations exist.
- Keep recipient delivery data separate from adult profile contact fields and sign-in identifiers; do not copy or synchronize them implicitly.
- Keep production identity and delivery providers replaceable and undecided.
- Make terminal and error states understandable without exposing private information.

## Non-goals

- Production authentication, account registration, recovery, MFA, passkeys, or identity-provider selection.
- Production email-provider selection, deliverability operations, templates beyond the minimum invitation content, or paid services.
- Controlled first-Family-Group provisioning or first-administrator operator tooling.
- Removing active members, transferring administrator responsibility, account deletion, profile deletion, or data-retention policy.
- Minors, guardians, delegated acceptance, Household, relationships, Safety Circle, Events, Plans/Trips, Sharing, Safety, or notifications beyond delivering the invitation itself.

## Conflicts, unknowns, and decisions required

- **Conflicts:** None detected in the loaded authoritative context.
- **Unknowns:** Delivery channel, expiration period, binding of an invitation to the intended adult, resend/duplicate behavior, post-expiration behavior, and how much terminal invitation metadata to retain.
- **Decision required:** Select an invitation contract before canonical context or an executable work item is created.

## Options

### Option A - Provider-neutral email invitation with a one-time link

- **Description:** A Family Administrator enters an adult recipient's email address solely for invitation delivery. The application creates one opaque, single-use invitation for one Family Group, expiring after seven days. Local development uses a clearly labeled local outbox; a production email provider remains deferred behind a replaceable delivery boundary. The recipient follows the link, signs in through the replaceable identity boundary, sees the Family Group name and inviting administrator's display name, and explicitly accepts or declines. Successful acceptance atomically consumes the invitation and creates one active membership for the signed-in account.
- **Duplicate and resend rule:** Only one pending invitation may exist for the same normalized recipient email and Family Group. Resend revokes the prior token and issues a new seven-day invitation. A declined, revoked, expired, or used token cannot be reactivated; the administrator must issue a new invitation.
- **Security and privacy rule:** Store only a hash of the invitation token, use a cryptographically strong token, never place private group data in the link, and return a uniform unavailable response for invalid, expired, revoked, used, or unauthorized invitations. The raw recipient email remains invitation-delivery data, not profile or sign-in data.
- **Capacity rule:** Check configured capacity at issue time for early feedback and again transactionally at acceptance. The acceptance-time check controls.
- **Benefits:** Familiar experience; simple local implementation; no paid provider needed now; production delivery remains replaceable; clear one-time and expiration semantics.
- **Risks/tradeoffs:** Email delivery and link possession are part of recipient assurance. A production identity policy may later add stronger verified-address matching. Seven days and retention behavior are product decisions that require approval.

### Option B - Administrator-transmitted invitation code

- **Description:** Generate a one-time code that the administrator communicates outside the application. The signed-in recipient enters the code and explicitly accepts.
- **Benefits:** No delivery channel or provider is needed.
- **Risks/tradeoffs:** Encourages insecure manual handling, creates more support friction for less technical users, and provides weaker assurance that the accepting account is the intended adult.

### Option C - Wait for production identity and delivery-provider decisions

- **Description:** Defer invitation implementation until production authentication and email delivery are selected.
- **Benefits:** Identity matching and delivery can be designed together.
- **Risks/tradeoffs:** Blocks the next meaningful multi-member Family Group slice and is unnecessary if boundaries remain replaceable.

## Recommendation

Approve **Option A**, with these bounded decisions:

1. Email is the MVP invitation delivery channel, but the provider is not selected.
2. Local development uses a non-production local outbox; no paid service is required.
3. Invitations expire after seven days and are one-time, revocable, and non-reactivatable.
4. Resending replaces any existing pending invitation for the same normalized recipient email and Family Group.
5. The accepting adult must be signed in and must explicitly accept after seeing the Family Group name and inviter display name.
6. Acceptance binds the invitation to the current account and atomically creates membership only if the invitation is valid and capacity remains available.
7. Invitation delivery data stays separate from profile contact data and authentication identifiers.
8. Store security/audit metadata needed to enforce the lifecycle, but defer any long-term invitation-history retention rule. A later production-readiness decision must define retention before production use.

This is the leanest path that works locally and preserves expansion. It deliberately does not claim that a future production identity provider's email assertion is already available. If stronger verified-address matching is later required, it can be added at the identity boundary before production without moving membership authorization out of the application.

## Proposed follow-on sequence

If this proposal is approved:

1. Update canonical product, privacy, glossary, requirement-index, and decision context; create a candidate context baseline for explicit owner approval.
2. After that baseline is approved, create one lightweight work item for normal adult Family Administrator invitation, local outbox delivery, acceptance/decline, revocation, expiration, capacity enforcement, and server-side authorization.
3. Obtain separate work-item approval and execution authorization.
4. Keep controlled first-administrator operator provisioning in a later proposal because its operator process remains undecided.

## Verification plan

An eventual work item must provide evidence that:

1. no invitation state grants Family Group access before successful acceptance;
2. only an active administrator of the target group can issue, resend, or revoke;
3. tokens are strong, hashed at rest, single-use, expire after seven days, and fail uniformly after terminal states;
4. acceptance requires a signed-in account, explicit confirmation, a valid invitation, and available capacity, and creates at most one membership atomically;
5. concurrent or repeated acceptance cannot create duplicate membership or exceed capacity;
6. recipient delivery data is not copied into profile or authentication data and is not exposed to unrelated accounts;
7. administrator, operator, relationship, invitation, or sign-in status creates no unrelated private-data access;
8. local outbox behavior cannot activate outside Development;
9. minor/guardian and first-administrator operator paths remain absent;
10. accessible UI states, automated authorization/lifecycle tests, builds, formatting, migrations, and dependency checks pass; and
11. a separate verification report maps every acceptance check and requirement to evidence, labeled `SELF-VERIFIED` if independent verification is unavailable.

## Decision and authorization

- **Decision:** `ACCEPTED` — Option A
- **Decision ID:** `DEC-030`
- **Approved by:** Project owner
- **Approved at:** 2026-09-25
- **Execution authorized:** `NO`
