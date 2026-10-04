# PRP-011 — Configurable Shared-Date Rules

## Metadata

- **Proposal ID:** `PRP-011`
- **Status:** `ACCEPTED — V1 RULE PACKAGE; CONFIGURABILITY DEFERRED`
- **Date:** `2026-09-26`
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.6`
- **Related proposal:** `PRP-010` — Shared Family Dates (`ACCEPTED — DIRECTION ONLY`)
- **Required context:**
  - `ai-context/README.md`
  - `ai-context/project-status.md`
  - `ai-context/planning/proposals/PRP-010-shared-family-dates.md`
  - `ai-context/decision-log.md#dec-036-shared-family-dates-proposal-direction`
  - `ai-context/core/product-contract.md#req-fam-001`, `#req-fam-002`, and `#req-fam-003`
  - `ai-context/core/privacy-safety-contract.md#req-priv-005`, `#req-priv-006`, and `#req-priv-011`
  - `ai-context/core/domain-glossary.md#family-group` and `#family-administrator`
- **Affected authority:** Family shared-date visibility, association, editing, deletion, and group-level settings
- **Model tier:** `DEEP`

## Request

Evaluate whether shared-date rules should be configurable by a Family-level manager instead of being fully fixed in product behavior. This proposal is limited to shared dates. It does not authorize settings for Events, trusted contacts, Household, or Safety Circle.

## Current evidence and context

- `REQ-FAM-002` allows shared birthdays or important dates when shared under applicable privacy rules; it does not define who configures those rules.
- `REQ-PRIV-005` and `REQ-PRIV-006` prohibit relationship or administrative status from implying access to private information.
- `REQ-PRIV-011` requires active membership for Family Group data access.
- `DEC-036` accepts a shared-dates-only direction and records as proposal-level rules that the creator and specifically associated active Family Group members may edit, and only the creator may delete while active. It does not establish visibility or a configurable policy manager.
- “Family Administrator” is an existing defined role. “Main family group member” is not a defined product role or authority.
- Group-level configuration could change who sees or edits sensitive dates. A manager's ability to change settings must not be assumed to grant access to dates or override the creator's control.

## Impact

Configuration could let families choose policies that suit their preferences, but it also adds an authority boundary and can broaden access to personal dates. The design must distinguish setting group defaults from changing the visibility or permissions of an individual date. It must also define how a policy change affects existing dates and what happens when the settings manager becomes inactive.

## Proposed requirements

- Keep this decision limited to shared dates in one Family Group.
- Any configurable policy must remain bounded to active members of that same Family Group.
- A group setting must not, by itself, grant its manager visibility into a date or private profile fields.
- No Family Administrator or other manager override is approved by this proposal; the authority and limits require an explicit decision.
- Preserve the `PRP-010` creator/associated-member editing and creator-only active-member deletion rules unless the project owner explicitly changes them.
- Do not add a new “main family member” role without defining its appointment, authority, and lifecycle.

## Recommended v1 rule package

To avoid forcing a configuration/role system into the first slice, this proposal recommends one simple rule set for all Family Groups. These remain proposals until explicitly approved and added to canonical context:

1. **Configuration:** No per-group rule settings in v1. Revisit configurability later through a separate proposal, using observed needs rather than predicting every family preference now.
2. **Visibility:** A shared date is visible only to its creator and the active Family Group members specifically associated with that date. Group membership alone does not make the date visible.
3. **Association:** The creator associates only active members of that same Family Group. Only the creator manages the association list; association does not grant any unrelated profile or Family Group access.
4. **Editing:** The creator and associated active members may edit the date's approved content. The association list is controlled separately by the creator. The exact minimal content fields must be stated in the canonical update before implementation.
5. **Deletion:** Only the creator may delete the date, and only while the creator is an active member of that Family Group, as already proposed in `DEC-036`.
6. **Membership ending:** When an associated member becomes inactive, that member immediately loses date visibility and editing access. The association is removed; later active membership does not restore it automatically.
7. **Membership ending/deletion retention:** Affected date entries become unavailable in normal Family views; retained copies are inaccessible to Family Group members, may be kept only for a finite approved period, and must then be deleted. The finite-retention principle is in candidate `CTX-1.8`; the exact period and cleanup details remain open.
8. **Birthdays:** Do not duplicate the existing profile birthday as a second date record. Show a member's birthday only where that profile field is already explicitly shared with the active Family Group. Other important dates are separate shared-date entries.

These defaults intentionally favor clear, narrow sharing over group-wide visibility or a new manager role. They do not define recurrence, notifications, audit access, or member rejoining behavior beyond the access rules stated above.

## Non-goals

- Configuring Events, trusted contacts, Household, Safety Circle, or other features.
- Granting administrative access to private date content or unrelated profile data.
- Introducing a new role, role hierarchy, permissions framework, or organization-wide settings model.
- Defining date fields, recurrence, notifications, retention, or visual layout except where needed to decide the configuration boundary.
- Canonical contract changes or application implementation.

## Conflicts, unknowns, and decisions required

- **Conflicts:** No conflict with active canonical requirements was found. The project owner later approved fixed Option A in `DEC-038`, which resolves the proposal-level shared-date rule question; configurable policies and a “main family group member” role remain excluded from v1.
- **Unknowns:** The exact date content fields and finite retention duration are not defined. The “main family group member” role remains undefined and is not included in v1.
- **Decision required:** None for the approved v1 visibility/editing package. The project owner accepted a limited-retention direction in `DEC-040`; its privacy-contract amendment is proposed in `CTX-1.8`. Exact content fields and retention duration require explicit decisions before implementation. Configurable group rules remain a possible later proposal, not a v1 capability.

## Options

### Option A — Fixed shared-date rules for the first slice (recommended)

- **Description:** Resolve `PRP-010`'s open questions once and apply the same approved rules to dates in all Family Groups. Do not add group configuration in the first slice.
- **Benefits:** Simplest behavior; avoids a new authority role and reduces risk that one member changes another person's privacy unexpectedly.
- **Risks/tradeoffs:** Families cannot tailor rules to their preferences; a later change may require another product decision and migration of existing records.

### Option B — Family Administrator selects group-wide rules

- **Description:** An active Family Administrator configures a bounded set of shared-date rules for their Family Group. The proposal does not yet specify which controls or how changes affect existing dates.
- **Benefits:** One clear existing role controls a consistent group policy; avoids inventing the undefined “main family member” role.
- **Risks/tradeoffs:** Family Administrator responsibility currently does not grant private-data access. Group policy could still change other members' effective date visibility or editing rights, so consent, notice, setting limits, administrator turnover, and interaction with creator-only deletion must be decided.

### Option C — Each date creator chooses within fixed product limits

- **Description:** The creator selects per-date settings from a product-defined set; associated members remain constrained by the approved association and active-membership rules.
- **Benefits:** Keeps decisions close to the date and creator; no group manager needs authority over every date.
- **Risks/tradeoffs:** Different dates may behave differently, reducing consistency; creators still need clear choices and boundaries.

## Recommendation

Option A and the fixed v1 rule package above were approved by the project owner in `DEC-038` and activated as canonical `CTX-1.7` in `DEC-039`. Configurability is deferred; a future proposal must explicitly define its manager, controls, consent, and effect on existing dates. The project owner accepted a finite-retention direction in `DEC-040`; candidate `CTX-1.8` records the rule while leaving the duration open. Exact date fields also remain open.

## Verification plan

Before any implementation work item is created, verify that approved canonical context explicitly defines the configuration owner, setting choices, scope, effect on existing dates, consent/notice behavior, administrator turnover, and relationship to creator/associated-member rights. Confirm that configuration does not grant access to date contents or private profile data by implication and remains within active Family Group membership. A later implementation must test allowed and denied configurations and record evidence for every criterion.

## Decision and authorization

- **Decision:** Option A and the fixed v1 rule package are accepted; configurability is deferred; the rules are active under `CTX-1.7`.
- **Decision ID:** `DEC-038`, `DEC-039`, `DEC-040` (decision framework recorded in `DEC-037`)
- **Approved by:** Explicit project-owner approval of the recommended rule package
- **Approved at:** 2026-09-26
- **Execution authorized:** Fixed v1 rules are active under `CTX-1.7`; limited-retention amendment is candidate-only under `CTX-1.8`. Application implementation is not authorized.
