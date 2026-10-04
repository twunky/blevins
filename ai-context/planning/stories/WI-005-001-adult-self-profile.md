# WI-005-001 - Adult Self-Profile Vertical Slice

## Metadata

- **Work Item ID:** `WI-005-001`
- **Status:** `DONE`
- **Phase:** Phase 5 - Family & Profiles
- **Context baseline:** `CTX-1.4`
- **Requirement references:**
  - `REQ-FAM-007`
  - `REQ-PRIV-005`
  - `REQ-PRIV-006`
  - `REQ-PRIV-011`
  - `REQ-PRIV-012`
  - `REQ-PRIV-013`
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
| `ai-context/planning/proposals/PRP-005-family-profiles-foundation.md` | Approved adult-first sequence and boundaries | Before proposal and execution |
| `ai-context/planning/proposals/PRP-006-adult-profile-contract.md` | Approved field/visibility decision and implementation recommendations | Before proposal and execution |
| `ai-context/core/product-contract.md#req-fam-007` | Canonical first-slice field contract | Before execution and verification |
| `ai-context/core/privacy-safety-contract.md#req-priv-013` | Canonical ownership, visibility, filtering, and access rules | Before execution and verification |
| `ai-context/core/domain-glossary.md#adult-profile` and `#profile-field-visibility` | Approved meanings and concept boundaries | Before execution |
| `ai-context/core/ux-contract.md#req-ux-003` and `#req-ux-006` | Progressive disclosure and accessible, understandable UI | Before client work and verification |
| `ai-context/decision-log.md#dec-017-private-httpjson-application-api` | Approved private application API boundary | Before API work |
| `ai-context/decision-log.md#dec-019-local-first-expandable-architecture-foundation` through `#dec-025-lightweight-personal-project-work-items` | Approved stack, provider, authorization, readability, context, and process constraints | Before execution |
| Existing solution/project files and the specific server/client entry points changed by this work item | Preserve the verified Phase 4 foundation and dependency direction | During execution |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Event, plan/trip, sharing/check-in, and safety workflows | This work item implements only the adult self-profile slice |
| Bootstrap, archive, and historical verification reports | They are not normal working authority for this slice |
| Invitation delivery, operator bootstrap mechanics, Household, relationships, and Safety Circle behavior | Explicitly deferred by `DEC-027` |
| Minor and guardian behavior | Blocked by open decision `OD-003` |
| Hosting, deployment, production identity, and production database selection | Provider and production decisions remain deferred |

## Goal

Implement and verify one local, end-to-end adult self-profile vertical slice using the existing .NET and Angular foundation. An adult owner can view and update exactly the approved four profile fields and optional-field visibility, while the server returns to another account only the fields that account is authorized to see.

## Proposed implementation decisions

Approval of this work item approves these bounded technical choices for this slice only:

1. Use the latest stable, .NET 10-compatible EF Core 10 patch available at execution time with the SQL Server provider. Record and lock the selected package versions.
2. Keep repository-owned EF Core migrations in the Infrastructure project. Use the detected local SQL Server only for local development and verification; make no production database, licensing, hosting, backup, or retention decision.
3. Keep `Account`, `AdultProfile`, `FamilyGroupMembership`, and `FamilyAdministratorAssignment` as separate persistence concepts. A role, relationship, or membership record must never substitute for an explicit profile-visibility decision.
4. Add a replaceable current-account boundary owned by the application. A clearly labeled development adapter may supply a configured adult account only in the Development environment; application startup must reject that adapter outside Development.
5. Use test-only identities for authorization matrices. Do not add a user-facing identity switcher or simulate a production sign-in system.
6. Provide private HTTP/JSON resource operations for the current adult's profile and for an authorized same-group member's filtered profile view. Endpoint contracts must not expose database entities or unauthorized fields.
7. Apply server-side authorization and response shaping on every profile read. Client-side hiding is not an authorization control.
8. Keep optional-field visibility as explicit per-field values limited to `OnlyMe` and `ActiveFamilyGroup`. Display-name visibility remains fixed to active members of the same Family Group.
9. Use one development-only adult fixture for local manual use. Test fixtures cover owner, active same-group member, Family Administrator, inactive/former member, unrelated account, and Application Operator. Fixtures must not be available in Production.

## Input and validation decisions

These limits are technical safeguards for the first slice and do not add profile fields or identity claims:

| Field | Proposed handling |
|---|---|
| Display name | Required; trim surrounding whitespace; 1-100 Unicode characters after trimming; not unique |
| Contact email | Optional; trim surrounding whitespace; maximum 254 characters; validate as an email-shaped contact value; do not treat as verified or as a sign-in identifier |
| Contact phone | Optional; trim surrounding whitespace; maximum 32 characters; preserve user-entered formatting; do not treat as verified or normalize into an identity claim |
| Birthday month/day | Optional pair; both values are present or both absent; require a valid Gregorian month/day, including February 29; never collect or infer a birth year |

Blank optional text is stored as absent. Validation failures return restrained, field-specific errors without echoing unrelated private data.

## Boundaries / non-goals

- No production authentication provider, credentials, recovery, MFA, passkey, or invitation mechanism.
- No account registration, invitation acceptance, membership-management, administrator-management, or operator UI.
- No minors, guardians, delegated profile editing, or administrator/operator override.
- No profile photo, birth year, address/location, biography, pronouns, medical/emergency data, relationship labels, Household, Safety Circle, or custom fields.
- No public profile, discovery, broad family directory, selected-person visibility, link sharing, or cross-group search.
- No use of contact email or phone as a verified identity claim.
- No production hosting, deployment, database edition/license, backups, retention policy, or paid provider.
- No Events, Plans/Trips, Sharing, Safety, notification, or location functionality.
- No microservices, public API, generic repository framework, or speculative abstraction outside this slice.

## Acceptance checks

1. `AC-01` - The implementation contains only the four profile fields approved by `REQ-FAM-007`; excluded fields and minor/guardian paths are absent.
2. `AC-02` - Account, adult profile, Family Group membership, and administrator assignment remain separate concepts in code and persistence.
3. `AC-03` - A repository-owned migration creates only the persistence needed for this slice and applies successfully to a clean local SQL Server database.
4. `AC-04` - Optional fields default to `OnlyMe`; display name is available only to active members of the same Family Group; optional fields are returned to them only when explicitly marked `ActiveFamilyGroup`.
5. `AC-05` - Only the adult owner can update profile values and visibility. Family Administrator and Application Operator status provide no override.
6. `AC-06` - Inactive/former members, unrelated accounts, and accounts outside the relevant Family Group receive no private profile fields, and denial behavior does not reveal additional sensitive information.
7. `AC-07` - Server responses omit unauthorized fields before serialization; Angular does not receive fields merely to hide them.
8. `AC-08` - Authentication/sign-in identifiers are neither copied into profile contact fields nor returned as profile data automatically.
9. `AC-09` - The development current-account adapter and fixtures cannot activate outside the Development environment.
10. `AC-10` - The Angular experience lets the current adult edit the approved fields and make an explicit per-field visibility choice using accessible labels, keyboard operation, understandable validation, and progressive disclosure.
11. `AC-11` - Automated tests cover validation, defaults, owner updates, same-group filtering, administrator denial, operator denial, inactive-member denial, unrelated-account denial, sign-in/contact separation, production rejection of development identity, and migration behavior.
12. `AC-12` - .NET and Angular builds/tests, formatting checks, and dependency vulnerability checks pass with evidence recorded.
13. `AC-13` - A verification report maps every acceptance check and referenced canonical requirement to evidence and an honest result; self-verification is labeled.

## Tasks

- [x] `TASK-001` - Confirm `CTX-1.4`, work-item approval, separate execution authorization, and compatible stable package versions before changing application files.
- [x] `TASK-002` - Add the minimal domain/application concepts, validation, current-account boundary, and authorization policy for the approved adult profile slice.
- [x] `TASK-003` - Add EF Core SQL Server persistence configuration and a repository-owned initial profile-slice migration.
- [x] `TASK-004` - Add the Development-only current-account adapter and adult fixture with a production activation guard.
- [x] `TASK-005` - Add private HTTP/JSON profile operations with server-side authorization and field-filtered response contracts.
- [x] `TASK-006` - Add the accessible Angular adult-profile editor and explicit optional-field visibility controls.
- [x] `TASK-007` - Add unit, application, authorization, API, persistence/migration, and Angular tests required by the acceptance checks.
- [x] `TASK-008` - Run builds, tests, formatting, dependency vulnerability checks, clean-database migration verification, and scoped runtime checks.
- [x] `TASK-009` - Produce `VER-WI-005-001`, update the work item with evidence, and update project status only if verification supports it.

## Risks and blockers

- **Privacy/access risk:** A broad response model or client-only hiding could disclose private fields. Mitigation: explicit filtered response construction and negative authorization tests.
- **Development-identity risk:** A local adapter could be mistaken for production authentication. Mitigation: Development-only registration, visible labeling, and a startup rejection test outside Development.
- **Schema lock-in risk:** Combining account, profile, membership, or administrator state would make later authorization unsafe. Mitigation: separate persistence concepts and dependency tests.
- **Provider lock-in risk:** EF Core SQL Server details could leak into domain/application layers. Mitigation: keep provider and migrations in Infrastructure.
- **Open-decision boundary:** `OD-003` blocks every minor/guardian path. Profile deletion/retention, richer fields, identity provider, and invitation behavior remain deferred and must not be inferred.

No authoritative context conflict was detected. The project owner separately authorized execution on 2026-09-25.

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner
- **Authorized at:** 2026-09-25
- **Authorized scope:** All tasks and acceptance checks in `WI-005-001`; no work outside its boundaries

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| Work-item scope and traceability | Project owner approved `WI-005-001` on 2026-09-25 | `PASS` |
| Application implementation | Domain/application policy, EF Core SQL Server persistence and migration, Development-only identity/fixture, private API, and Angular editor completed | `PASS` |
| .NET build and tests | `dotnet build FamilySafety.slnx --no-restore`: 0 warnings/errors; `dotnet test FamilySafety.slnx --no-build --no-restore`: 18 passed | `PASS` |
| Angular build and tests | Production build passed; Vitest: 3 passed | `PASS` |
| Migration verification | Initial migration applied to clean `FamilySafety_WI005001_Verification`; expected tables/history present; no pending model changes | `PASS` |
| Runtime verification | Development health/profile GET/PUT passed; Production rejects enabled development identity and returns 401 when identity is unavailable | `PASS` |
| Formatting and dependency checks | .NET format and Prettier checks passed; NuGet found no vulnerable packages; npm production audit found 0 vulnerabilities | `PASS` |
| Scope verification | Excluded-feature, merge-marker, TODO, and FIXME scans passed | `PASS` |

- **Verification report:** `../verification/VER-WI-005-001.md`
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `PASS`; completion authorized by `VER-WI-005-001`

## Execution record

| Area | Selected or verified value | Basis |
|---|---|---|
| EF Core SQL Server | `10.0.12` | Latest stable EF Core 10 patch verified from official Microsoft/NuGet sources on 2026-09-25 |
| Migration | `20260926001919_InitialAdultProfile` | Repository-owned Infrastructure migration; clean SQL Server application verified |
| Development database | Local SQL Server default instance | Development and verification only; no production database decision |
| Current account | Replaceable application boundary with Development-only configured adapter | Production guard and HTTP 401 behavior verified |
