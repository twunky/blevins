# WI-005-003 - Adult Family Directory

## Metadata

- **Work Item ID:** `WI-005-003`
- **Status:** `DONE`
- **Phase:** Phase 5 - Family & Profiles
- **Context baseline:** `CTX-1.5`
- **Requirement references:**
  - `REQ-FAM-001`
  - `REQ-FAM-003`
  - `REQ-FAM-007`
  - `REQ-PRIV-005`
  - `REQ-PRIV-006`
  - `REQ-PRIV-011`
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
| `ai-context/planning/proposals/PRP-008-adult-family-directory.md` | Accepted directory decision, boundaries, and verification direction | Before execution and verification |
| `ai-context/core/product-contract.md#req-fam-001`, `#req-fam-003`, and `#req-fam-007` | Approved Family capability, exclusions, and adult profile field boundary | Before execution and verification |
| `ai-context/core/privacy-safety-contract.md#req-priv-005`, `#req-priv-006`, `#req-priv-011`, and `#req-priv-013` | Membership and field-level access rules | Before execution and verification |
| `ai-context/core/ux-contract.md#req-ux-003` and `#req-ux-006` | Progressive disclosure, filtering, and accessibility | Before client work and verification |
| `ai-context/core/domain-glossary.md#family-group` through `#profile-field-visibility` | Approved meaning and separation of Family Group, member, profile, and visibility | Before execution |
| Existing profile and membership source/tests directly affected by this item | Preserve verified boundaries and reuse existing authorization | During execution |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Event, plan/trip, sharing/check-in, and safety workflows | This item implements only adult Family member discovery and approved profile viewing |
| Bootstrap, archive, and historical verification reports | They are not normal working authority |
| Operator provisioning, invitation implementation details, and production identity/delivery | They are not changed by this item |
| Relationships, Household, Safety Circle, removal/rejoining, and retention | Detailed behavior is not approved |
| Minor and guardian behavior | Blocked by open decision `OD-003` |

## Goal

Implement and verify a minimal adult Family directory in which an active member can choose one of their active Family Groups, filter its active adult members by display name, select a member, and view only the profile fields already authorized for that viewer.

## Approved implementation decisions

1. Extend the existing modular-monolith profile/membership slice; add no service, provider, or database technology.
2. Add a current-member Family Group query that returns only group ID and name for groups where the current account has active membership.
3. Add a group-scoped adult directory query that returns only profile ID and display name for active adult members after revalidating the requester's active membership in that group.
4. Retrieve at most the configured group ceiling already enforced by membership. Apply trimmed, case-insensitive display-name filtering and predictable display-name ordering in the application layer, with profile ID as the stable tie-breaker. Do not add pagination in this validated 250-member slice.
5. Continue using the existing shared-profile operation for another member's on-demand details and the existing owner-profile operation when the selected profile is the current adult's own profile. Do not create a richer bulk profile response.
6. Return uniform unavailable responses for unknown, inactive, former, or unrelated group/member access without exposing private membership information.
7. Add the directory as a progressively disclosed section in the current Angular shell. Do not decide or implement the unresolved primary/mobile navigation grouping in `OD-001`.
8. Hide the group selector when exactly one active group is available, show it when more than one is available, and provide understandable zero-group, no-match, loading, error, and selected-member states.
9. Add no schema or migration unless execution discovers a concrete requirement; if it does, stop and return to Proposal Mode rather than expanding scope.

## Boundaries / non-goals

- No new profile fields or visibility modes.
- No relationship labels, family tree, Household, Safety Circle, trusted-contact, minor, or guardian behavior.
- No administrator-only data or override; no member management, removal, suspension, reactivation, rejoining, or role transfer.
- No invitations, operator provisioning, identity-provider changes, contact synchronization, or notifications.
- No public, global, or cross-Family-Group people discovery.
- No primary/mobile navigation decision, visual redesign, or unrelated Phase 6+ behavior.
- No database migration, new external dependency, paid service, hosting, or deployment work.

## Acceptance checks

1. `AC-01` - The current adult can query only Family Groups in which their membership is active; inactive, former, unrelated, unauthenticated, administrator, and operator status provide no bypass.
2. `AC-02` - A directory query requires active membership in the selected group and returns only active adult members of that group.
3. `AC-03` - Directory entries expose exactly profile ID and display name; they expose no optional profile values, sign-in identifiers, delivery emails, visibility settings, role assignments, capacity, or unrelated metadata.
4. `AC-04` - Display-name filtering is trimmed and case-insensitive; results are ordered predictably by display name with profile ID as a stable tie-breaker.
5. `AC-05` - Selecting another member loads details on demand through the existing shared-profile authorization, with unauthorized optional values omitted server-side.
6. `AC-06` - Selecting the current adult loads their owner profile through the existing owner boundary without changing edit or visibility behavior.
7. `AC-07` - Unknown, inactive, former, and unrelated access uses uniform unavailable responses and reveals no private group or membership information.
8. `AC-08` - One active group does not show an unnecessary selector; multiple active groups provide an explicit selector; zero active groups provides a useful private empty state.
9. `AC-09` - Changing the selected group clears stale member selection and details before loading the newly authorized directory.
10. `AC-10` - The Angular experience provides keyboard-operable native controls, visible labels, clear focus, and understandable loading, error, no-match, and selected-member states.
11. `AC-11` - The slice adds no relationship, Household, Safety Circle, minor/guardian, administrator override, member-management, public discovery, or primary-navigation behavior.
12. `AC-12` - No schema migration, external dependency, provider, hosting, deployment, or production-service choice is introduced.
13. `AC-13` - Automated tests cover authorization matrices, response shaping, group scoping, inactive/former exclusion, filtering, deterministic ordering, self/other detail selection, group switching, and accessible Angular interactions.
14. `AC-14` - .NET and Angular builds/tests, formatting, dependency vulnerability checks, scoped Development/Production runtime checks, and excluded-scope scans pass with evidence.
15. `AC-15` - A verification report maps every acceptance check and referenced requirement to evidence and labels verifier independence honestly.

## Tasks

- [x] `TASK-001` - Confirm `CTX-1.5`, proposal acceptance, work-item approval, execution authorization, and the verified profile/membership foundation.
- [x] `TASK-002` - Add minimal current-member group and group-scoped directory application contracts and authorization behavior.
- [x] `TASK-003` - Extend EF persistence queries without changing the schema and verify deterministic group/member scoping.
- [x] `TASK-004` - Add private HTTP/JSON operations with minimal response contracts and uniform unavailable shaping.
- [x] `TASK-005` - Add the accessible Angular directory, filtering, conditional selector, and on-demand detail experience without deciding navigation.
- [x] `TASK-006` - Add focused unit, authorization, API, persistence, regression, and Angular tests.
- [x] `TASK-007` - Run builds, tests, formatting, dependency advisories, runtime checks, and excluded-scope scans.
- [x] `TASK-008` - Produce `VER-WI-005-003` and update status only when verification supports completion.

## Risks and blockers

- **Membership-disclosure risk:** Directory access exposes who belongs to a private group. Mitigation: active same-group authorization on every query and minimal response shaping.
- **Bulk-data risk:** Returning optional profile data for every member would increase exposure. Mitigation: index-only results and on-demand existing profile authorization.
- **Stale-state risk:** A group switch could leave prior details visible. Mitigation: clear selected state before each scoped reload and test it.
- **Navigation ambiguity:** `OD-001` remains open. Mitigation: use the current shell without adding a primary navigation decision.
- **Open-decision boundary:** `OD-003` blocks every minor/guardian path; relationship, Household, Safety Circle, removal, and rejoining remain undefined and excluded.

No authoritative context conflict was detected. Because the item affects private membership visibility and authorization, `DEC-032` does not apply; explicit work-item approval and execution authorization are required.

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner
- **Authorized at:** 2026-09-25
- **Authorized scope:** All tasks and acceptance checks in `WI-005-003`; no work outside its boundaries

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| Proposal decision | `PRP-008 Option A` approved; recorded as `DEC-033` | `PASS` |
| Application implementation | Application, persistence, private API, and Angular adult directory | `PASS` |
| Automated and runtime verification | 42 .NET tests, 8 Angular tests, no-model-change check, builds, formatting, advisories, and runtime checks | `PASS` |

- **Verification report:** `VER-WI-005-003`
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `PASS`
