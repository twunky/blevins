---
Status: APPROVED
Authority: CANONICAL — FUTURE TARGET ONLY
Target-Phase: Phase 11 — Pilot & Release Readiness
Decision: DEC-097; DEC-098
Version: 1.1
---

# Future Production Entry Contract

Owner-approved under [DEC-097](../decision-log.md#dec-097--approve-future-phase-11-production-entry-model), 2026-10-06. All requirements here are **APPROVED FUTURE PHASE 11 behavior, UNIMPLEMENTED**. They govern future production-entry planning, not current runtime behavior. CTX-1.39 remains the active MVP baseline; this future-target contract does not activate a successor CTX, start Phase 11, or authorize implementation or implementation work items. Phase 10 human usability work remains current; the functionally complete MVP and closed Phases 9.5/10.5 are preserved.

Normative future-entry wording lives here. The [requirements index](requirements-index.md#future-phase-11-production-entry-dec-097) traces the target separately from the active baseline. Existing resource-specific contracts remain authoritative outside the narrow replacements below.

[DEC-098](../decision-log.md#dec-098--approve-provider-neutral-identity-and-production-entry-architecture) approves the initial web identity architecture below. [WI-011-001](../planning/stories/WI-011-001-provider-neutral-identity-and-production-entry-architecture.md) records architecture planning and governance checks only. All production behavior remains UNIMPLEMENTED; no provider selection, runtime change or Phase 11 implementation authorization follows.

## Account entry

### REQ-PROD-015

An eligible adult may register an application Account without an invitation. Account creation, authentication and registration confer no Family membership or Family/resource access. Zero active Family memberships is a valid Account state. Minimum adult/profile eligibility must be satisfied before membership creation; the existing minimum profile contract remains the starting point, without introducing minor/guardian behavior.

## Family bootstrap and private joining

### REQ-FAM-019

An eligible verified adult may create a Family even if already a member of one or more Families. Creation atomically establishes the FamilyGroup, creator's active membership and initial Family administrator assignment. No self-invitation is required. Stable Family IDs establish identity; Family names need not be globally unique. Do not search for duplicate Families, automatically merge similarly named Families, impose a lifetime one-Family creation restriction, or require zero existing memberships. Future submission idempotency, rate limits and abuse controls address creation safety; their implementation remains future work.

Normal membership paths are creating a Family as its initial member/admin, or explicitly accepting a valid invitation to an existing Family. Existing-Family joining remains private and invitation-driven. No public Family search/directory, nearby discovery, or request-to-join by name is approved. Registration, authentication and possessing a URL never create membership.

## Adult invitation authority

### REQ-FAM-020

Every eligible active adult Family member may invite another adult by default. An ordinary adult member may issue invitations and view, resend or revoke their own Pending invitations. A Family administrator may additionally view Family-wide invitation management and revoke any Pending Family invitation. There is no initial configurable "Members may invite" setting.

Invitation permission is separate from administration. Ordinary inviters gain no member-removal, administrator-assignment, Family-settings or private-resource override authority. Checks use actual current Family membership and invitation ownership, never selected-Family context alone.

## Invitation lifecycle and delivery

### REQ-FAM-021

Preserve Pending, Accepted, Declined, Revoked and Expired states; seven-day expiry; explicit acceptance; single-use semantics; and nonreactivatable terminal states. Retain the existing one-Pending-invitation-per-normalized-recipient-email-and-Family rule. Resend revokes the prior invitation and issues a new seven-day invitation; it does not reactivate a terminal invitation. Acceptance-time capacity and duplicate-membership checks control; retain issue-time capacity feedback.

Acceptance revalidates inviter authority. If the inviter leaves the Family or otherwise loses invitation authority while an invitation is Pending, that invitation becomes permanently unusable. Later rejoining or regaining authority never revives it. Future implementation must preserve that loss across membership/authority changes, not merely check that the inviter is active again.

Design production email delivery together with this lifecycle, through a provider-neutral boundary with bounded reliable delivery/retry. Delivery status is separate from invitation state: delivery failure/success cannot accept, revive or extend an invitation. No email vendor is selected. Development outbox behavior remains current MVP behavior only.

## Acceptance and verified recipient security

### REQ-PRIV-035

Membership is created only after all of these hold at explicit acceptance:

- An authenticated local Account and required adult/minimum profile eligibility.
- A valid Pending invitation with expiry and current inviter-authority validation, including permanent invalidation on prior authority loss.
- Recipient reconciliation where applicable; the initial direct invitation model requires control of the matching verified normalized email address.
- Explicit recipient acceptance, configured capacity and duplicate-membership validation.

Opening a link is not acceptance. GET/navigation requests must never consume an invitation. Successful acceptance binds the local Account and atomically creates at most one active membership. Preserve strong opaque tokens, hash-only stored token material, single-use enforcement and nondisclosing unavailable behavior. Tokens/links carry no private Family data.

Initial direct invitations target a normalized email address. A typed email assertion, editable profile contact email, or invitation-token possession is insufficient to establish recipient control. Authentication email evidence and profile contact information remain separate, with no automatic copying/synchronization. Email is not stable Account identity.

For an authenticated-account mismatch, explain it without exposing additional Family data and allow account switching or require the inviter to reissue to the correct address. No mismatch bypass is approved. Authentication/registration/verification may continue back to the application-owned invitation, but must revalidate and require explicit acceptance after return.

## Multi-Family membership and former-member return

### REQ-FAM-022

One Account/AdultProfile may participate in multiple Families. Joining another Family reuses that Account/Profile and creates a separate membership; it neither duplicates the Account nor copies grants from another Family. Selected Family is presentation/navigation context only. APIs authorize actual resources using current membership, grants and domain-specific rules.

A former member may return only through a fresh valid invitation and explicit acceptance, with renewed membership provenance. Do not automatically restore old resource grants, Safety Circle membership, administrator assignment or stale membership-specific authority. Retained own-resource access already allowed by existing domain rules remains governed by those rules; this contract neither removes it nor turns it into renewed membership or recipient access.

## No-Family onboarding and Trusted Contacts

### REQ-UX-015

A valid Account with zero Families sees a bounded first-use state such as "You're not part of a Family yet." Offer Create a Family, continuation of a valid invitation if one exists, and review of the user's own profile/privacy. No public discovery or short invitation-code entry is approved initially; secure invitation links are sufficient.

Onboarding distinguishes "Invite someone to join this Family" from "Add a Trusted Contact". Trusted Contacts remain private owner-managed contacts, need no Account, are not Family memberships, are not automatically invited, and grant no authorization. Existing Trusted Contact ownership/privacy rules remain unchanged.

## Per-Family optional-profile privacy

### REQ-PRIV-036

Replace the MVP's broad optional-profile visibility behavior with per-Family sharing choices for production onboarding. For a newly joined Family, display name may be available under the minimum Family-member profile contract; every optional profile field starts private for that Family. Only the owner explicitly chooses which optional fields to share with that Family. Sharing in one Family never automatically exposes a field in another. This applies to each new membership, including a return, without duplicating the Account/Profile.

Preserve owner-only editing, server omission of unauthorized fields, and the absence of administrator/operator visibility overrides. Do not retrofit per-Family choices into the completed MVP during this task.

## Administrator continuity

### REQ-FAM-023

The production model supports more than one Family administrator. Before pilot readiness, establish bounded rules preventing a populated Family from unintentionally having no administrator. The preferred direction is co-administration, last-admin protection and an exceptional operator recovery path if required. Detailed departure/removal/recovery rules remain to be approved before pilot implementation/readiness; no broad administrator lifecycle is silently authorized here.

Operator recovery must not grant arbitrary access to protected Family content or broad support impersonation. Invitation authority alone grants neither administrator assignment nor removal authority. Administrator membership on a returning account is not automatically restored.

## Provider-neutral authentication and identity mapping

### REQ-PROD-016

The future architecture is:

```text
Identity Provider
    -> validated external identity
    -> ASP.NET Core authentication
    -> stable local Account mapping
    -> Family Safety domain/API authorization
    -> resource access
```

The identity provider answers "Who are you?" Family Safety answers "What are you allowed to do?" Family Safety keeps authoritative membership, administrator assignment, creator authority, explicit recipients, Concerns, Incidents, Sharing and resource-specific visibility rules. Do not encode dynamic Family authorization in provider roles/claims.

Map trusted external issuer + subject to a stable local Account. Email may support verification, invitation reconciliation and communication, but is not the Account primary identity. Never automatically merge Accounts because email, names or profile information match. Explicit Account linking requires proof of ownership of both identities.

No identity provider is selected. Required provider-neutral capabilities are:

- OIDC; OAuth authorization-code flow with PKCE; practical ASP.NET Core integration. REQ-PROD-017 makes PKCE mandatory for the initial web topology under DEC-098.
- Signup/registration, verified email, sign-in, recovery and stable subject identifiers.
- Secure sessions/logout, signing-key rotation and MFA support.
- Accessible web/mobile flows and continuation back to an application-owned invitation after registration/verification.
- Suitable pilot operational/support controls.

Evaluate passkeys, social login, multiple verified addresses and broader federation later; they are not initial required capabilities. DEC-098's requirements below approve the initial session, bootstrap, assurance and continuation policies and mandatory provider proof gate. Provider integration, detailed mechanics and all implementation still require later bounded authorization. These are not current MVP requirements.

## Initial web authentication topology

### REQ-PROD-017

Use same-origin Angular with the existing ASP.NET Core backend managing OIDC authentication and a Family Safety application session. The browser authenticates to Family Safety through a secure application session cookie. Do not use SPA-managed reusable provider tokens for the initial production web application. Do not introduce a separate BFF service without a future concrete need and decision.

Use standard ASP.NET Core authentication/OIDC handlers rather than a custom OIDC framework. Require OpenID Connect, OAuth authorization-code flow, PKCE, state, nonce, exact registered redirects, trusted issuer validation and audience/signature/lifetime validation. ASP.NET Core exchanges the authorization code. An ID token is never a browser API credential; provider tokens remain server-side when required.

This topology applies to the initial web application. Preserve a future native-client boundary for standards-based OAuth access tokens to the Family Safety API, resolving to the same local Account and domain authorization. No further mobile authentication design is approved here.

## Local identity, bootstrap and eligibility

### REQ-PROD-018

Map validated trusted Issuer + Subject to a stable local Family Safety Account. Neither email nor display name is a stable Account key. A future conceptual structure may include Account, ExternalIdentity / LoginIdentity, verified-contact evidence/snapshot and adult-eligibility attestation; this is not schema authorization.

Before creating a new local production Account/Profile require validated trusted issuer/subject, one usable primary verified email, a user-confirmed display name, explicit adult eligibility attestation and required approved application acknowledgments. Create the Account, external identity mapping, minimum AdultProfile, verified-contact evidence and attestation atomically. Optional profile data stays optional/private; never automatically copy authentication email into optional profile contact fields.

For the initial adult product/pilot, use verified email plus explicit adult attestation captured inside Family Safety after authentication. Verified email does not prove age. Retain the approved attestation wording/version and server timestamp. Final wording/legal threshold remains open. Stronger age assurance is a later policy decision unless deployment requirements demand it.

One trustworthy primary verified email is sufficient initially. Accept verification evidence only from validated provider evidence. Authentication-contact evidence stays separate from optional profile contact information; profile email must never reconcile invitations. A provider unable to supply sufficiently trustworthy verified-email evidence must demonstrate a bounded verification solution before selection.

Defer user-facing Account linking until multiple login methods create a concrete need. Never merge Accounts because emails, names or profile details match or look similar. Future linking requires proof of control of both identities and recent authentication under REQ-PROD-020. No administrator email-based linking shortcut is approved.

## Application sessions, cookies, CSRF and logout

### REQ-PROD-019

Use a revocable server-side Family Safety session with a secure HttpOnly application cookie. Initial pilot defaults are 30-minute idle expiration, 12-hour absolute expiration and no persistent "remember me". These defaults may later be tuned using operational/usability evidence. Background polling must not keep sessions alive indefinitely. Sliding renewal cannot override absolute expiry or recent-authentication requirements.

Require HTTPS, Secure, HttpOnly, a host-only cookie, Path=/, suitable SameSite behavior and session rotation. All unsafe cookie-authenticated mutations require antiforgery/CSRF validation. Angular's readable antiforgery request token is distinct from the HttpOnly authentication cookie. HttpOnly does not solve XSS; continue normal Angular/browser XSS protections.

Logout reliably revokes the Family Safety session, clears its cookie and selected-Family/account-scoped browser state, and propagates across application tabs. Attempt provider-session logout where supported; provider logout failure must not undo local logout. Global logout across every provider/device/application is not an initial requirement.

## MFA, recent authentication and recovery

### REQ-PROD-020

MFA is available to all users and required for any session acting as a Family administrator before pilot readiness. Require sufficient MFA assurance before self-service Family creation because it grants initial administration. Family creation remains application-owned and transactional: Family + membership + administrator assignment + idempotent result under REQ-FAM-019.

The provider owns factor enrollment, challenge and factor recovery. Family Safety determines whether current local authorization requires MFA; Family roles are never authoritative provider roles/claims.

Initial recent-authentication window: five minutes for assigning/removing administrators, destructive member/Family actions, future identity linking, and credential/MFA/verified-contact security changes. Use real provider reauthentication/assurance evidence. Refreshing the application cookie does not satisfy recent authentication. Ordinary viewing, coordination and invitation acceptance do not require universal step-up; applicable administrator-session assurance still holds.

The provider owns credentials, MFA factors and their recovery. Family Safety owns the stable Account, Profile, memberships, ownership, grants and application-session revocation. Recovery must return to the same stable issuer/subject mapping. A different Subject must not be repaired by matching email; require a bounded account-control/recovery process. Detailed administrator continuity/recovery rules remain a separate open decision under REQ-FAM-023.

## Invitation continuation and authenticated entry

### REQ-UX-017

Preserve invitation intent in application-owned server state through sign-in, registration and verification. Never put raw invitation tokens in provider metadata. Authentication return restores invitation REVIEW only; it never accepts membership. Revalidate under REQ-PRIV-035 and require explicit acceptance. Continuation must protect against tampering, replay, open redirect, account/session confusion, user switching, concurrent tabs and token leakage.

After successful authentication, a valid invitation continuation takes priority. Otherwise, zero Families shows REQ-UX-015's approved no-Family first use; one Family is selected and Home opens; multiple Families reuse a still-valid current-session selection or require explicit Family selection. A protected deep link must authorize the actual requested resource before presentation Family context changes. Selected Family remains navigation/presentation context only, never authorization.

After explicit invitation acceptance: reload authoritative memberships, select the newly joined Family, show a brief per-Family privacy review with optional fields private for that Family under REQ-PRIV-036, then continue to that Family Home. A failed membership reload must be retryable without repeating acceptance.

## Provider-neutral API and Development boundary

### REQ-PROD-021

ASP.NET Core authentication establishes the current local Account through a small request-user abstraction such as the existing CurrentAccount / ICurrentAccountAccessor boundary. Unauthenticated or expired sessions produce 401 / login flow; authenticated callers lacking resource authorization retain existing nondisclosing 404/unavailable behavior. Lists continue omitting unauthorized records.

Retain a Development-only authentication scheme. Production OIDC/cookie and Development authentication resolve to the same local Account abstraction; the domain must not know which scheme produced it. Production must never accept X-Development-Account, fallback fixture identities or Development identity switching. Development auth remains unchanged in this governance task.

Limit provider-specific integration to OIDC configuration/registration, claims normalization, verified-email extraction, assurance/reauthentication interpretation, registration/recovery/logout integration and provider revocation/security-event integration where available. Account identity, bootstrap, attestation, session records, invitation continuation, Family memberships, administrator assignments, profile privacy and resource authorization remain application-owned. Acceptable provider revocation latency remains open.

## Identity-provider proof gate

### REQ-PROD-022

No provider may be selected solely from documentation or marketing comparison. Each candidate must demonstrate an isolated proof of every required flow below, including relevant negative/error behavior. The proof itself requires later bounded authorization; it is not performed or authorized by this architecture task.

| Required proof | Required behavior / relevant failure evidence |
|---|---|
| Registration | Complete the eligible-adult registration path; show rejected/incomplete registration behavior |
| Verified email | Demonstrate trustworthy validated evidence, or a bounded verification solution; reject missing/unverified/untrusted evidence |
| Sign-in | Demonstrate the approved OIDC flow and validated return; reject invalid flow/identity evidence |
| Credential + MFA recovery | Demonstrate both recovery paths and stable identity; no email-based repair for changed Subject |
| Server-side Family Safety session | Demonstrate revocation, idle/absolute expiry and polling/renewal boundaries |
| ASP.NET Core integration | Demonstrate standard handlers, server-side exchange and secure cookie/CSRF behavior |
| Invitation continuation through auth | Restore review through sign-in/registration/verification; reject tampering/replay/confusion without automatic acceptance |
| Stable Issuer/Subject → Account mapping | Reuse the same local Account; reject email/name-based merging and invalid mapping/bootstrap |
| MFA/admin assurance | Demonstrate admin and creation gates, real recent authentication, and failed/insufficient assurance |
| Logout/session behavior | Demonstrate local revocation, cookie/state clearing and cross-tab logout even on provider-logout failure |

Only candidates passing this required-flow gate proceed to comparison of cost, operational burden, support, portability and hosting requirements. No identity provider or email delivery vendor is selected here.

## Minimum authentication telemetry

### REQ-PROD-023

Plan outcome categories for login; Account mapping/bootstrap failures; invitation continuation/reconciliation failures; MFA/reauthentication outcomes; and session expiry/revocation/logout. Never log passwords, secrets, authorization codes, provider tokens, session cookies, invitation tokens or full invitation URLs. This is planning only; no telemetry implementation is authorized.

## Bounded invitation retention

### REQ-PRIV-037

Production invitation lifecycle requires bounded retention, minimal retained metadata, token material removed as soon as safely possible, and review of backup/restore implications. Final retention periods require owner approval before production. **Exact durations remain OPEN**; the proposed 30-day / 90-day periods are not approved. Seven-day invitation expiry is separate from metadata retention. Existing Shared Date, Concern and Incident retention policies are unchanged and must not be imported as invitation retention defaults.

## Later invitation conveniences

### REQ-UX-016

QR invitations are future convenience scope. A QR must represent the same secure invitation URL/flow; scanning creates no membership and bypasses neither authentication, recipient reconciliation where applicable, nor explicit acceptance. No separate QR authorization semantics. General/multi-use Family invitation links are not approved initial production scope. SMS, short codes and general links need later justification/approval; listing them in the game plan authorizes no implementation.

## Current-rule reconciliation

These are targeted replacements for **future production planning only**, not global supersession of the active MVP requirements or historical decisions.

| Current MVP contract / historical assumption | Future Phase 11 replacement |
|---|---|
| REQ-FAM-005 / REQ-FAM-008; DEC-016/030: administrator-only normal invitation authority | REQ-FAM-019/020/021: private existing-Family joining, adult-member invitation authority and bounded management |
| REQ-FAM-006; DEC-018: controlled operator-provisioned first Family and self-invitation path | REQ-FAM-019: self-service atomic Family/member/admin bootstrap without a self-invitation |
| REQ-UX-011; DEC-064/080: no-Family invitation/operator guidance | REQ-UX-015: valid zero-Family Account, creation/invitation/profile actions |
| REQ-PRIV-013: broad optional-profile sharing across active Families | REQ-PRIV-036: per-Family choices, optional fields private on a new membership |
| REQ-PRIV-014 / REQ-FAM-008: delivery email without production proof of recipient control | REQ-PRIV-035: matching verified-email recipient reconciliation, preserving lifecycle security |
| REQ-PRIV-012: operator-bootstrap privacy boundary | REQ-FAM-023: possible bounded continuity recovery, preserving no protected-content override |

Operator provisioning is a historical/current MVP contract assumption, not a claim that a production operator workflow is implemented. Development seeded/scenario Families remain unchanged. Historical proposals, candidates, completed work items and verification records remain evidence of their original scope.

## Planning and unresolved choices

The [Phase 11 roadmap](../planning/roadmap.md#phase-11--pilot--release-readiness) owns the planned sequence and pilot gates. The following remain future bounded decisions and must not be resolved by this canonicalization:

- OD-006 exact invitation retention durations.
- Detailed administrator continuity/recovery rules.
- Final adult-attestation wording/legal threshold.
- Invitation abuse/rate-limit parameters.
- Acceptable identity-provider revocation latency.
- Identity provider selection.
- Email delivery vendor selection.

MFA/provider integration, recovery, eligibility enforcement and creation/delivery mechanics require later bounded design/proof within the approved architecture. Passkeys/social login/multiple verified addresses/federation and invitation conveniences remain optional later evaluation. These unresolved mechanics do not reopen the approved product model or authorize implementation.
