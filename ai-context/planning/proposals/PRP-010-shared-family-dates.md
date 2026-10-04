# PRP-010 — Shared Family Dates

## Metadata

- **Proposal ID:** `PRP-010`
- **Status:** `ACCEPTED — RULE PACKAGE; CANONICAL CANDIDATE CTX-1.7`
- **Date:** `2026-09-26`
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.6`
- **Required context:**
  - `ai-context/README.md`
  - `ai-context/project-status.md`
  - `ai-context/core/product-contract.md#req-fam-001`, `#req-fam-002`, and `#req-fam-003`
  - `ai-context/core/privacy-safety-contract.md#req-priv-005`, `#req-priv-006`, and `#req-priv-011`
  - `ai-context/core/ux-contract.md#req-ux-003` and `#req-ux-006`
  - `ai-context/core/domain-glossary.md#family-group` and `#family-member`
- **Affected authority:** Family product behavior, shared-date visibility, participation, editing, and deletion rules
- **Model tier:** `DEEP`

## Request

Prepare a bounded Phase 5 proposal for shared birthdays or important dates. Trusted contacts, Events, Household/basic grouping, Safety Circle, and implementation are outside this proposal.

## Current evidence and context

- `REQ-FAM-002` permits the Family area to present shared birthdays or important dates when shared under applicable privacy rules.
- `REQ-FAM-001` includes Family capabilities but does not define shared-date behavior.
- `REQ-FAM-003` excludes genealogy, family-tree visualization, ancestry research, complex custom groups, and a family-history archive.
- `REQ-PRIV-005`, `REQ-PRIV-006`, and `REQ-PRIV-011` preserve private-data boundaries and require active membership for Family Group access.
- Existing Phase 5 work covers adult profiles, membership invitations, a directory, and adult Family Relationships. None defines shared-date visibility, editing, or lifecycle.
- The project owner proposed a simple collaboration rule: the creator and members specifically associated with an item can edit it; only its creator can delete it.
- The project owner clarified that associated members must be within the same Family Group, and creator-only deletion is allowed only while the creator is an active member.
- The project owner approved the fixed v1 rule package in `DEC-038` and approved baseline `CTX-1.7` in `DEC-039`. The shared-date rules are canonical. The project owner also accepted the limited-retention direction in `DEC-040`; its canonical amendment is a `CTX-1.8` candidate. Exact content fields and the retention duration remain open.

## Impact

Shared dates may reveal personal information. The proposal must define who can see a date, who can be associated with it, and how active Family Group membership constrains access. Creator departure also affects ownership and deletion, and must not be resolved through assumed administrator or operator powers.

## Proposed requirements

- Limit this proposal to shared birthdays or important dates within an existing Family Group.
- A person must be an active member of the relevant Family Group to be associated with a date or to exercise date-editing rights.
- Proposed editors are the date creator and active Family Group members specifically associated with that date. Other Family Group members do not receive editing rights merely from group membership.
- Only the creator may delete a date, and only while the creator remains an active member of the relevant Family Group.
- Do not infer that association or editing rights grant access to profile fields, membership, administrator responsibility, Household, Safety Circle, location, sharing, or safety information.
- Do not add a detailed role system in this slice; more granular roles may be considered separately.

## Non-goals

- Trusted contacts or contact-management behavior.
- Event creation or event participant/edit/delete behavior.
- Household/basic grouping, Safety Circle, or relationships.
- Minors, guardians, delegated authority, Family Administrator overrides, or Application Operator overrides.
- Notifications, recurrence, date formats, calendars, external calendar synchronization, or specific screen design unless separately defined and approved.
- Retention/audit policy, transfer of ownership, or post-membership removal behavior beyond the active-member rule stated above.

## Conflicts, unknowns, and decisions required

- **Conflicts:** None identified in the loaded approved requirements. The proposed collaboration rules add detail not currently present in canonical context and therefore require explicit approval before they can become authoritative.
- **Remaining unknowns:** Exact date-entry content fields and the finite retention duration remain undecided. These are not inferred by `DEC-038` or `DEC-040`.
- **Decision required:** Explicitly approve or reject candidate baseline `CTX-1.8`. Before implementation, separately define exact date-entry fields and retention duration and authorize a bounded work item.

## Options

### Option A — Small shared-date slice (recommended)

- **Description:** Define one shared-date capability for birthdays or important dates, using the proposed creator/associated-member edit rule and active-creator-only deletion. Resolve visibility, association management, content fields, and creator-departure behavior before implementation.
- **Benefits:** Keeps the scope to the smallest remaining Family capability and records the owner's requested editing boundary without expanding to contacts or events.
- **Risks/tradeoffs:** Several privacy and lifecycle choices remain open; implementation must wait for them.

### Option B — Shared dates and trusted contacts together

- **Description:** Define both shared dates and trusted contacts in one product slice.
- **Benefits:** Broader Family information capability in a single decision cycle.
- **Risks/tradeoffs:** Adds a separate sensitive-information model and more consent, visibility, and lifecycle decisions; it exceeds the currently selected Option A scope.

## Recommendation

Proceed with Option A as the proposal scope only. First resolve its open questions; then, if approved, update the canonical Family and privacy requirements before creating a separately authorized implementation work item. Keep trusted contacts deferred.

## Verification plan

Before any implementation is authorized, verify that the approved canonical update defines visibility, active membership, association, editing, deletion, and creator departure; preserves all existing privacy/authority boundaries; and does not add trusted contacts, Events, Household, Safety Circle, or other deferred behavior. Any later implementation work item must map each accepted rule to tests and evidence. Prefer independent verification; label self-verification honestly when independence is unavailable.

## Decision and authorization

- **Decision:** Shared dates only; v1 visibility/editing rules are active under `CTX-1.7`; limited-retention handling is accepted as a direction and proposed under `CTX-1.8`.
- **Decision ID:** `DEC-038`, `DEC-039`, `DEC-040` (direction recorded in `DEC-036`)
- **Approved by:** Explicit project-owner approval
- **Approved at:** 2026-09-26
- **Execution authorized:** `CTX-1.7` preparation, verification, and activation are complete. `CTX-1.8` retention amendment is candidate-only and pending approval. Application implementation is not authorized.
