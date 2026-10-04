# WI-005-004 — Adult Family Relationships

## Metadata

- **Work item ID:** `WI-005-004`
- **Status:** `DONE`
- **Phase:** Phase 5 — Family & Profiles
- **Epic:** No epic defined; this is a lightweight personal-project work item.
- **Context baseline:** `CTX-1.6`
- **Required context manifest:**

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md` | Mandatory operating rules and authority | Entry |
| `ai-context/project-status.md` | Current phase, active baseline, and continuation checkpoint | Entry |
| `ai-context/core/product-contract.md#req-fam-009` | Approved adult relationship labels and lifecycle | Before execution |
| `ai-context/core/privacy-safety-contract.md#req-priv-015` | Consent, visibility, access, and administrator/operator boundaries | Before execution |
| `ai-context/core/domain-glossary.md#family-relationship` | Approved domain terminology and distinctions | Before execution |
| `ai-context/planning/proposals/PRP-009-adult-family-relationships.md` | Accepted rationale, UX language guardrails, and explicit exclusions | Before execution |
| `ai-context/core/ux-contract.md#req-ux-003` and `#req-ux-006` | Progressive disclosure, filtering, and accessibility expectations | Before client work and verification |
| `ai-context/planning/proposals/PRP-003-technical-architecture.md#proposed-architecture-direction` and `#required-architectural-invariants` | Existing modular-monolith boundaries and server-side contextual authorization | Before implementation |
| `ai-context/routing/agent-model-routing.md` | DEEP routing and verification independence guidance | Before execution and verification |

- **Context intentionally not loaded:** Events, plans/trips, sharing/check-ins, safety workflows, bootstrap source, archive, and unrelated historical reports; this work does not alter those areas.
- **Pinned context dependency:** None
- **Requirement references:** `REQ-FAM-009`, `REQ-PRIV-015`
- **Additional UX references:** `REQ-UX-003`, `REQ-UX-006`
- **Model tier:** `DEEP`

## Objective

Implement the approved mutually confirmed, adult-only Family Relationship capability for active adults in the same Family Group, preserving the approved reciprocal labels, lifecycle, visibility, consent, and no-permission boundaries.

## Non-goals

- Minors, guardians, dependents, caregiving, custody, or legal relationship status.
- Genealogy, family trees, inference, custom/free-text labels, or more than one relationship per pair and group.
- Household or Safety Circle behavior, relationship-based profile visibility, location/sharing/safety access, or administrator/operator overrides.
- Cross-group discovery, public visibility, inactive/former member views, relationship history/audit screens, notification behavior, or retention policy.
- Authentication, membership lifecycle, deployment, hosting, provider selection, architecture changes, or other product areas.
- Resolving UI behavior not already specified by approved UX context or `PRP-009`; if implementation requires such a decision, stop and report it.

## Acceptance criteria

1. `AC-01` — An active adult can propose a relationship to another active adult only within a Family Group where both have active membership; self-relationships and duplicate/contradictory pending or accepted relationships for the same pair and group are rejected or unavailable.
2. `AC-02` — The supported reciprocal label pairs are exactly spouse/partner ↔ spouse/partner, parent ↔ adult child, sibling ↔ sibling, and other family ↔ other family.
3. `AC-03` — A pending proposal is visible only to its participants. The recipient can accept or decline; the proposer can withdraw; only explicit acceptance makes the relationship visible to other active adults in that same Family Group.
4. `AC-04` — Either participant can remove an accepted relationship. Changing a label requires removal and a new mutually accepted proposal.
5. `AC-05` — Relationship information is unavailable to inactive/former members and unrelated groups, and ending either participant's active membership ends access within that group.
6. `AC-06` — Relationship state grants no profile visibility, membership, administrator responsibility, Household/Safety Circle membership, location, sharing, safety-workflow access, guardian authority, or other permission; Family Administrators and Application Operators cannot act for another adult.
7. `AC-07` — User-facing proposal, confirmation, and removal language is clear and accessible, identifies relationships as mutually confirmed descriptive context, and does not imply verified legal, biological, caregiving, guardian, emergency, or financial authority.
8. `AC-08` — Automated and manual verification evidence maps each acceptance criterion and both requirements to the implemented behavior; no unrelated scope or unsupported UX/technical choice is introduced.

## Tasks

- [x] `TASK-005-004-01` — Inspect existing application patterns and identify the smallest implementation surface; stop if the approved architecture or existing behavior conflicts with this scope.
- [x] `TASK-005-004-02` — Implement the approved relationship proposal, acceptance/decline, withdrawal, removal, reciprocal labels, and group visibility behavior within the existing application.
- [x] `TASK-005-004-03` — Add focused automated coverage for acceptance criteria and privacy/authorization boundaries using the repository's existing test patterns.
- [x] `TASK-005-004-04` — Record changed files, commands/checks, evidence, limitations, and complete self-verification.

## Risks and blockers

- Relationships are sensitive personal information; authorization must be enforced wherever relationship data is read or changed, not only through interface visibility.
- If the current application architecture cannot support the required behavior without an architecture change, stop and propose that change separately.
- Approved canonical UX detail does not specify exact screen placement, notifications, or visual design. Use existing approved patterns only where they already cover this behavior; otherwise report the missing decision before making a product choice.

## Execution authorization

- **Approved story:** `YES`
- **Authorized by:** Explicit project-owner approval of `CTX-1.6`, together with the prior message approving the resulting work item and authorizing execution within its approved scope.
- **Authorized at:** 2026-09-26
- **Authorized scope:** This work item only, under `CTX-1.6`, subject to its acceptance criteria and non-goals.

## Evidence

| Requirement or criterion | Evidence | Result |
|---|---|---|
| `REQ-FAM-009`; `AC-01`–`AC-05` | Domain, application, persistence, API, Angular, and automated test evidence in `VER-WI-005-004.md` | `PASS` |
| `REQ-PRIV-015`; `AC-03`, `AC-05`–`AC-07` | Group- and participant-scoped server queries/actions, UI copy, consent/removal tests in `VER-WI-005-004.md` | `PASS` |
| `REQ-UX-003`, `REQ-UX-006`; `AC-07` | Progressive disclosure, native labeled controls/status, participant-specific copy, and UI tests in `VER-WI-005-004.md` | `PASS` |
| `AC-08` | `VER-WI-005-004.md` maps all criteria and requirements; scope and limitations recorded | `PASS` |

## Verification

- **Verification report:** `ai-context/planning/verification/VER-WI-005-004.md`
- **Independence:** `SELF-VERIFIED` (no independent verifier available in this execution)
- **Outcome:** `PASS`
- **Verified at:** 2026-09-26
