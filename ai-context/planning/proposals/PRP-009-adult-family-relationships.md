# PRP-009 - Adult Family Relationships

## Metadata

- **Proposal ID:** `PRP-009`
- **Status:** `ACCEPTED`
- **Date:** 2026-09-26
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.5`
- **Required context:**
  - `ai-context/README.md`
  - `ai-context/project-status.md`
  - `ai-context/core/product-contract.md#req-fam-001` through `#req-fam-003`
  - `ai-context/core/product-contract.md#req-fam-007`
  - `ai-context/core/privacy-safety-contract.md#req-priv-005` and `#req-priv-006`
  - `ai-context/core/ux-contract.md#req-ux-003` and `#req-ux-006`
  - `ai-context/core/domain-glossary.md#family-group` through `#profile-field-visibility`
  - `ai-context/planning/proposals/PRP-008-adult-family-directory.md`
- **Context intentionally excluded:** Events, plans/trips, sharing/check-ins, safety workflows, bootstrap, archive, operator provisioning, invitations, Household behavior, Safety Circle behavior, member removal/rejoining, retention policy, and minors/guardians
- **Affected authority:** Product Family behavior, privacy and consent, terminology, requirements, and later Phase 5 work
- **Model tier:** `DEEP`

## Request

Define the smallest adult-only Family relationship capability that supports understandable family context without creating permissions, administrator control over personal relationships, genealogy features, or minor/guardian behavior.

This proposal does not authorize canonical changes or application implementation.

## Current evidence and context

- `REQ-FAM-001` requires family relationships in MVP.
- `REQ-FAM-003` excludes complete genealogy, family-tree visualization, ancestry research, and complex custom groups.
- `REQ-FAM-007` deliberately excluded relationship labels from the first adult profile slice without deciding whether a later slice may add them.
- `REQ-PRIV-005` states that a family relationship does not imply permission to access information.
- `REQ-PRIV-006` states that Family Administrator responsibility does not grant access to private information.
- The glossary keeps Adult Profile, membership, administrator responsibility, relationships, Household, and Safety Circle distinct.
- `PRP-008` and `WI-005-003` established a minimal active-adult directory but intentionally added no relationship behavior.
- The approved source does not define relationship taxonomy, direction, consent, editing authority, visibility, conflict handling, or lifecycle.

## Impact

Relationship data is personal and can be sensitive or disputed. A relationship label could also be mistaken for an access rule, guardian authority, Household membership, or Safety Circle trust. The first slice therefore needs explicit consent, narrow visibility, neutral language, and hard separation from permissions.

## Requirements

- Limit the first slice to active adults within the same private Family Group.
- Keep relationship state separate from Account, Adult Profile, membership, Family Administrator, Household, Safety Circle, and every access-control decision.
- Require both adults to consent before a relationship becomes visible to other active members of that Family Group.
- Give neither Family Administrators nor Application Operators an override to create, accept, edit, or remove another adult's relationship.
- Use a small structured label set with clear reciprocal meaning and one general family fallback.
- Prevent self-relationships, duplicate active/pending relationships for the same pair and group, and contradictory labels for the same pair.
- Allow either participant to remove an accepted relationship without the other participant's approval.
- Keep pending proposals visible only to the two participants and grant no permissions or private-data access.
- Ensure ending active membership makes the relationship unavailable in that Family Group. Removal/rejoining persistence remains outside this slice because those membership workflows are not approved.
- Use clear, accessible confirmation and removal language without implying legal, biological, caregiving, guardian, or emergency authority.

## Non-goals

- Minors, guardians, dependents, parental consent, custody, legal status, or delegated account management.
- Genealogy, family-tree visualization, ancestry, lineage, generations, or relationship inference.
- Arbitrary custom labels, free-text relationship descriptions, multiple simultaneous labels for one pair, or complex relationship taxonomy.
- Household membership, co-residence, addresses, shared finances, or household permissions.
- Safety Circle membership, caregiver authority, trusted-contact permissions, emergency access, location sharing, or safety-workflow access.
- Relationship-based profile visibility, automatic notifications outside the in-application proposal state, public visibility, or cross-group discovery.
- Family Administrator moderation, forced assignment, dispute adjudication, or relationship-history/audit screens.
- Member removal, reactivation, former-member rejoining, production identity, hosting, or deployment.

## Conflicts, unknowns, and decisions required

- **Conflicts:** None detected in the loaded authoritative context.
- **Unknowns:** The approved source does not select a relationship taxonomy, consent model, visibility audience, reciprocal-label behavior, or editing lifecycle.
- **Decision required:** Select one bounded relationship model. Approval of an option will require canonical requirements and glossary updates, candidate-baseline verification, and explicit baseline approval before implementation planning.

## Options

### Option A - Mutually confirmed structured relationships

- **Description:** Either active adult proposes one relationship to another active adult in the same Family Group. The proposer selects a label from their perspective. The recipient sees the reciprocal meaning and explicitly accepts or declines. Only accepted relationships are visible to other active adults in that Family Group.
- **Initial labels:**
  - **Spouse / partner** ↔ **Spouse / partner**
  - **Parent** ↔ **Adult child**
  - **Sibling** ↔ **Sibling**
  - **Other family** ↔ **Other family**
- **Lifecycle:** One pending or accepted relationship may exist for the same adult pair and Family Group. The proposer may withdraw a pending proposal; the recipient may accept or decline; either participant may remove an accepted relationship. Changing a label requires removing it and creating a newly confirmed proposal.
- **Visibility:** Pending state is visible only to its participants. An accepted relationship is visible only to active adult members of that Family Group. Terminal proposals and removed relationships are absent from normal product views; long-term audit retention is not decided here.
- **Permissions:** Relationship state grants no profile visibility, membership, administrator responsibility, Household membership, Safety Circle membership, location access, guardian authority, or other permission.
- **Benefits:** Consent protects both adults; reciprocal labels are understandable; the set covers common adult family context without becoming genealogy; group-scoped visibility fits the private Family directory.
- **Risks/tradeoffs:** Confirmation adds a step, the initial taxonomy is intentionally limited, and “Other family” is less descriptive than custom labels.

### Option B - Private personal labels

- **Description:** Each adult privately labels other active members for their own reference. Labels are never shared and require no confirmation.
- **Benefits:** Simplest consent and conflict model; no one can label another person publicly.
- **Risks/tradeoffs:** Does not create shared family context, can produce inconsistent private interpretations, and provides limited value to later coordination experiences.

### Option C - Family Administrator-managed relationships

- **Description:** A Family Administrator assigns structured relationship labels for members of that Family Group.
- **Benefits:** Centralized setup and fewer confirmation steps.
- **Risks/tradeoffs:** Lets an administrator assert personal information for other adults, creates correction/dispute problems, and risks confusing administrative responsibility with private-data authority.

## Recommendation

Approve **Option A**.

It provides useful shared family context while requiring both adults' consent, preserving the approved rule that relationships grant no permissions, and avoiding administrator overreach, genealogy, custom taxonomy, Household behavior, Safety Circle behavior, and minors.

If accepted:

1. Record the decision.
2. Add new canonical relationship and privacy requirements using the next immutable IDs.
3. Add a concise `Family Relationship` glossary definition.
4. Prepare and independently check candidate baseline `CTX-1.6`.
5. Stop for explicit project-owner approval of `CTX-1.6`.
6. Only after baseline approval, create an approval-ready implementation work item.

## Verification plan

- Verify every new canonical statement against the accepted option and existing privacy invariants.
- Confirm relationship state never changes authorization, profile visibility, membership, administrator responsibility, Household, Safety Circle, sharing, or safety access.
- Confirm the taxonomy is reciprocal, adult-only, group-scoped, and free of minor/guardian implications.
- Confirm proposal, acceptance, decline, withdrawal, removal, duplicate prevention, and unavailable-member boundaries are explicit.
- Confirm excluded genealogy, custom-label, administrator-control, Household, Safety Circle, and minor/guardian behaviors remain absent.
- Produce a separate `SELF-VERIFIED` candidate-context report if no independent verifier is available; do not activate `CTX-1.6` automatically.

## Decision and authorization

- **Decision:** `OPTION A ACCEPTED`
- **Decision ID:** `DEC-034`
- **Approved by:** Explicit project-owner approval
- **Approved at:** 2026-09-26
- **Execution authorized:** `NO`

Approval authorizes the candidate canonical amendment and its verification only. No implementation work item exists yet, and application execution remains blocked until `CTX-1.6` is explicitly approved and a bounded work item is created.
