# WI-005-002 - Adult Membership Invitations

## Metadata

- **Work Item ID:** `WI-005-002`
- **Status:** `DONE`
- **Phase:** Phase 5 - Family & Profiles
- **Context baseline:** `CTX-1.5`
- **Requirement references:**
  - `REQ-FAM-004`
  - `REQ-FAM-005`
  - `REQ-FAM-008`
  - `REQ-PRIV-005`
  - `REQ-PRIV-006`
  - `REQ-PRIV-011`
  - `REQ-PRIV-012`
  - `REQ-PRIV-014`
  - `REQ-UX-003`
  - `REQ-UX-006`
  - `REQ-AI-003`
  - `REQ-AI-009`
  - `REQ-AI-010`
  - `REQ-AI-011`
  - `REQ-VERIFY-001`
  - `REQ-VERIFY-002`
  - `REQ-VERIFY-003`
- **Model tier:** `DEEP`

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md` | Mandatory operating, authority, execution, and verification rules | Entry |
| `ai-context/project-status.md` | Active phase, baseline, blockers, and authorization state | Entry |
| `ai-context/planning/proposals/PRP-007-adult-membership-invitations.md` | Accepted invitation decision, boundaries, and verification direction | Before proposal and execution |
| `ai-context/core/product-contract.md#req-fam-004`, `#req-fam-005`, and `#req-fam-008` | Capacity, invitation-only membership, and adult lifecycle requirements | Before execution and verification |
| `ai-context/core/privacy-safety-contract.md#req-priv-005`, `#req-priv-006`, `#req-priv-011`, `#req-priv-012`, and `#req-priv-014` | Authorization, data separation, token, and no-pre-acceptance-access rules | Before execution and verification |
| `ai-context/core/domain-glossary.md#family-administrator` through `#application-operator` | Approved meanings and separation of account, invitation, membership, administrator, and operator | Before execution |
| `ai-context/core/ux-contract.md#req-ux-003` and `#req-ux-006` | Progressive disclosure and accessible, understandable interactions | Before client work and verification |
| `ai-context/decision-log.md#dec-017-private-httpjson-application-api`, `#dec-020-local-development-foundation-and-provider-deferment`, and `#dec-021-authentication-deferred-and-application-authorization-owned-internally` | Private API, local-first provider boundary, and application-owned authorization | Before execution |
| Existing profile-slice source, migration, and tests directly affected by this work item | Preserve the verified modular-monolith boundaries and extend existing concepts safely | During execution |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Event, plan/trip, sharing/check-in, and safety workflows | This work item implements only normal adult Family Group invitations and membership acceptance |
| Bootstrap, archive, and historical verification reports | They are not normal working authority for this slice |
| Controlled first-Family-Group and first-administrator operator mechanics | The operator process remains deferred |
| Account registration, production identity, recovery, and production email delivery | Provider and onboarding decisions remain deferred |
| Active-member removal, administrator transfer, rejoining after former membership, and invitation-history cleanup | Product behavior or retention is not yet approved |
| Household, relationships, Safety Circle, and profile deletion/retention | Separate Phase 5 decisions are required |
| Minor and guardian behavior | Blocked by open decision `OD-003` |

## Goal

Implement and verify one local, end-to-end normal-adult Family Group invitation slice. An active Family Administrator can send or resend a seven-day email invitation through a replaceable delivery boundary or revoke it by recipient email. A signed-in adult with an existing Account and Adult Profile can view the approved invitation confirmation and explicitly accept or decline. Only valid acceptance creates active membership.

## Proposed implementation decisions

Approval of this work item approves these bounded technical choices for this slice only:

1. Extend the existing .NET modular monolith, EF Core SQL Server persistence, private HTTP/JSON API, Angular client, and Development-only identity substitute. Add no microservice or production provider.
2. Extend `FamilyGroup` with a required human-readable name and operator-controlled capacity limit. Use 100 for the Development fixture, reject values outside 1-250, and provide no Family Administrator or operator configuration UI in this slice.
3. Add a distinct persisted Membership Invitation lifecycle with Pending, Accepted, Declined, Revoked, and Expired states; issue/expiry/terminal timestamps; target Family Group; issuing administrator membership; recipient delivery email and normalized comparison value; token hash; and accepted Account reference when applicable. Do not expose persistence entities as API contracts.
4. Generate at least 256 bits of cryptographically secure token entropy. Deliver the raw token once through an application-owned delivery interface, persist only its cryptographic hash, compare hashes without exposing the raw token, and use `TimeProvider` for deterministic seven-day expiration tests.
5. Keep this first flow limited to a signed-in adult who already has an Account and Adult Profile and has never held membership in the target Family Group. Do not create accounts or profiles from invitation data, and do not define former-member rejoining.
6. Provide an authorized query for the current account's actively administered Family Groups so the administrator can choose the target group. Do not add a general family-group directory or public discovery.
7. Do not add an invitation directory. The administrator chooses an administered group and supplies the recipient email to send/resend or revoke. Sending again for the same normalized email and group atomically revokes the prior pending invitation and issues a new one.
8. Use a client-side URL fragment for the raw invitation token so ordinary browser requests and server access logs do not receive it from the invitation link. The Angular client sends the token in a private JSON request body for preview, accept, or decline.
9. Preview returns only the Family Group name and inviter display name after the signed-in account and token pass the approved checks. Invalid, expired, revoked, declined, used, or otherwise unavailable invitations return one uniform unavailable result.
10. Acceptance runs in a database transaction, revalidates the token, invitation state, current Account and Adult Profile, absence of current or former membership, and capacity, then consumes the invitation and creates at most one active membership. Database constraints and concurrency tests must prevent duplicate membership, double acceptance, and capacity overflow.
11. Add a Development-only in-memory local outbox behind the delivery interface and a clearly labeled Development-only inspection endpoint. Startup must reject this outbox or its endpoint outside Development. Production email delivery remains unselected.
12. Add only the Angular experiences needed to choose an administered group, send/resend or revoke by email, and preview/accept/decline a token-bearing invitation. Use clear labels, keyboard-operable native controls, restrained errors, and progressive disclosure.

## Input and validation decisions

| Input | Proposed handling |
|---|---|
| Family Group name | Required; trim surrounding whitespace; 1-100 Unicode characters; Development fixture uses `Development Family` |
| Capacity limit | Required integer from 1 through 250; Development fixture uses 100; no user-facing editor |
| Recipient email | Required; trim surrounding whitespace; maximum 254 characters; validate as an email-shaped delivery value; use deterministic case-insensitive normalization only for duplicate comparison; do not treat it as verified identity or profile contact data |
| Invitation token | At least 256 bits of secure random entropy; URL-safe representation; raw value exists only for delivery and request processing; persist only its hash |
| Expiration | Exactly seven days from issue using an injected clock |

Validation failures must be field-specific where safe. Authorization, token, terminal-state, and unavailable-resource failures must avoid revealing unrelated private data.

## Boundaries / non-goals

- No production authentication provider, account registration, credentials, recovery, MFA, passkeys, or verified-email matching.
- No production email provider, paid service, deliverability operations, or production notification infrastructure.
- No controlled first-Family-Group provisioning, first-administrator invitation tooling, or operator UI/process.
- No invitation directory, invitation-history screen, cleanup/purge job, or long-term retention decision.
- No active-member removal, membership reactivation, former-member rejoining, administrator assignment/transfer, or delegated acceptance.
- No minors, guardians, Household, relationships, Safety Circle, Events, Plans/Trips, Sharing, Safety, location, or unrelated notifications.
- No public discovery, join requests, automatic membership, public API, microservices, hosting, deployment, or production database decision.
- No copying recipient email into Account sign-in identifiers or Adult Profile contact fields.

## Acceptance checks

1. `AC-01` - Only an active Family Administrator of the selected Family Group can issue/resend or revoke its invitation; unrelated members, inactive members, non-administrators, and Application Operators receive no override.
2. `AC-02` - The administrator can query only Family Groups they actively administer, choose one, and send/resend or revoke by validated recipient email without a public or general group directory.
3. `AC-03` - An issued invitation is pending, one-time, revocable, expires exactly seven days after issue, and uses at least 256 bits of secure token entropy with only its hash persisted.
4. `AC-04` - At most one pending invitation exists for a normalized recipient email and Family Group. Resend atomically revokes the previous invitation, and its former token becomes unavailable.
5. `AC-05` - Pending invitations grant no Family Group or profile access. Invitation possession, sign-in, administrator responsibility, and operator responsibility remain insufficient without accepted active membership.
6. `AC-06` - A signed-in existing adult can preview only the Family Group name and inviter display name for an available invitation and can explicitly accept or decline it.
7. `AC-07` - Invalid, expired, revoked, declined, used, wrong-account-state, and otherwise unavailable invitations use a uniform response and reveal no additional Family Group, inviter, recipient, or lifecycle information.
8. `AC-08` - Acceptance requires a valid pending invitation, an existing Account and Adult Profile, no current or former membership in the target group, and available configured capacity; it atomically consumes the invitation and creates exactly one active membership.
9. `AC-09` - Database constraints and concurrent tests prevent duplicate membership, double acceptance, multiple pending invitations for the same normalized email/group, and acceptance beyond configured capacity.
10. `AC-10` - Capacity is checked at issue time for early feedback and transactionally at acceptance; the acceptance-time check controls. Family Administrators cannot read or change capacity configuration through this slice.
11. `AC-11` - Recipient delivery email remains separate from Account and Adult Profile data, is never copied automatically, and is not returned to unrelated accounts or exposed in invitation links.
12. `AC-12` - The raw token is placed only in the client-side URL fragment for the delivered link, is sent to the private API only in a JSON body, is not persisted, and is absent from normal server access-log URLs.
13. `AC-13` - The local outbox and its inspection endpoint work only in Development; enabling either outside Development rejects startup. No production delivery provider is added.
14. `AC-14` - The Angular experience provides accessible, keyboard-operable send/resend, revoke, preview, accept, decline, loading, success, unavailable, validation, and capacity-conflict states using progressive disclosure.
15. `AC-15` - The repository-owned migration adds only the Family Group name/capacity and invitation persistence needed for this slice and applies successfully to a clean local SQL Server database with no pending model changes.
16. `AC-16` - Automated tests cover authorization matrices, normalization, expiration boundaries, resend/revoke/decline/used states, token storage and URL handling, data separation, first-time adult prerequisites, capacity, concurrency/idempotency, Development guards, API response shaping, and Angular interactions.
17. `AC-17` - .NET and Angular builds/tests, formatting checks, dependency vulnerability checks, clean-database migration verification, and scoped Development/Production runtime checks pass with evidence recorded.
18. `AC-18` - Scope scans confirm that production identity/email, operator bootstrap, minors/guardians, former-member rejoining, invitation retention cleanup, and unrelated product areas were not implemented.
19. `AC-19` - A verification report maps every acceptance check and referenced requirement to evidence and an honest result; self-verification is labeled.

## Tasks

- [x] `TASK-001` - Confirm `CTX-1.5`, work-item approval, separate execution authorization, and the existing verified foundation before application changes.
- [x] `TASK-002` - Add the minimal Family Group name/capacity and Membership Invitation domain/application lifecycle, delivery, clock, authorization, and error contracts.
- [x] `TASK-003` - Add EF Core mappings, constraints, indexes, concurrency handling, and the repository-owned migration.
- [x] `TASK-004` - Add the Development-only in-memory local outbox, inspection endpoint, recipient fixture, and non-Development startup guards.
- [x] `TASK-005` - Add the private administrator-group, invitation issue/resend/revoke, and recipient preview/accept/decline HTTP/JSON operations with server-side authorization and response shaping.
- [x] `TASK-006` - Add the accessible Angular administrator and recipient invitation experiences with fragment-based token handling.
- [x] `TASK-007` - Add unit, application, authorization, API, persistence/migration, concurrency, Development-guard, and Angular tests required by the acceptance checks.
- [x] `TASK-008` - Run builds, tests, formatting, dependency vulnerability checks, clean-database migration verification, scoped runtime checks, and excluded-scope scans.
- [x] `TASK-009` - Produce `VER-WI-005-002`, update this work item with evidence, and update project status only if verification supports it.

## Risks and blockers

- **Bearer-token risk:** Anyone possessing the link could attempt acceptance. Mitigation: high-entropy single-use token, hash-only persistence, short lifetime, signed-in explicit acceptance, uniform unavailable responses, and later production identity assurance at the replaceable boundary.
- **Concurrency risk:** Simultaneous acceptance could create duplicate membership or exceed capacity. Mitigation: database constraints, transactional acceptance, concurrency control, and parallel acceptance tests.
- **Delivery-data risk:** Recipient email could be mistaken for verified identity or profile contact information. Mitigation: separate contracts/storage, no automatic copying, and negative exposure tests.
- **Development-tooling risk:** The local outbox exposes raw links by design. Mitigation: explicit Development-only registration, startup rejection elsewhere, and no production implementation.
- **Existing-account boundary:** This slice requires an existing Account and Adult Profile. Registration/profile onboarding remains deferred and must not be inferred.
- **Open-decision boundary:** `OD-003` blocks every minor/guardian path. Former-member rejoining, operator bootstrap mechanics, and invitation-history retention also remain outside scope.

No authoritative context conflict was detected. The project owner approved this work item and separately authorized execution on 2026-09-25.

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner
- **Authorized at:** 2026-09-25
- **Authorized scope:** All tasks and acceptance checks in `WI-005-002`; no work outside its boundaries

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| Work-item scope and traceability | Project owner approved and authorized `WI-005-002` on 2026-09-25 | `PASS` |
| Application implementation | Domain, application, persistence, API, Development delivery, and Angular invitation flow | `PASS` |
| Automated and runtime verification | 36 .NET tests, 5 Angular tests, migration, runtime, formatting, builds, and advisory scans | `PASS` |

- **Verification report:** `VER-WI-005-002`
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `PASS`
