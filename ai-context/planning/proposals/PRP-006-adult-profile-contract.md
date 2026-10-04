# PRP-006 — Minimal Adult Profile Contract

## Metadata

- **Proposal ID:** `PRP-006`
- **Status:** `ACCEPTED`
- **Date:** 2026-09-25
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.3`
- **Required context:**
  - `ai-context/README.md`
  - `ai-context/project-status.md`
  - `ai-context/planning/proposals/PRP-005-family-profiles-foundation.md`
  - `ai-context/core/product-contract.md#req-fam-001` and `#req-fam-002`
  - `ai-context/core/privacy-safety-contract.md#req-priv-005`, `#req-priv-006`, `#req-priv-011`, and `#req-priv-012`
  - `ai-context/core/domain-glossary.md#people-and-family`
  - `ai-context/core/ux-contract.md#req-ux-003` and `#req-ux-006`
  - `ai-context/decision-log.md#dec-027-adult-self-profile-first-phase-5-sequence`
- **Affected authority:** Product profile behavior, privacy/access rules, UX, and first Phase 5 implementation work item
- **Model tier:** `DEEP`

## Request

Approve the smallest useful adult self-profile field set and visibility/edit rules so the first Phase 5 implementation work item can be created without inventing private-data behavior.

## Current evidence and context

- `DEC-027` approves an adult self-profile foundation before broader Family functionality.
- `REQ-FAM-001` requires member profiles, contact information, and privacy settings.
- `REQ-FAM-002` permits shared birthdays and trusted contacts only under applicable privacy rules.
- `REQ-PRIV-005` and `REQ-PRIV-006` prevent family relationship and administrator responsibility from granting private-information access.
- `REQ-PRIV-011` separates Account identity, invitation, membership, administrator responsibility, and private-information access.
- The approved source does not define exact profile fields or visibility defaults.

## Impact

This contract determines the first personal information stored and displayed by the application. Defaults must minimize disclosure while still making active Family Group membership understandable and useful.

## Requirements

- The first slice applies only to adults.
- Account identity, Family Member profile, Family Group membership, and administrator responsibility remain separate concepts.
- An adult edits only their own profile in this slice.
- Family Administrators cannot edit another adult's profile or override profile visibility.
- Application Operators have no routine profile-content access.
- Authentication/sign-in identifiers are not automatically copied into or exposed as profile contact fields.
- Ending active Family Group membership ends access to that Family Group's shared profile information.

## Recommended field contract

| Field | Required | Default visibility | Allowed visibility | Recommendation |
|---|---|---|---|---|
| Display name | Yes | Active Family Group | Active Family Group | Required so members can identify one another; no public visibility. |
| Contact email | No | Only me | Only me or Active Family Group | Separate from any sign-in email; sharing requires an explicit choice. |
| Contact phone | No | Only me | Only me or Active Family Group | Sharing requires an explicit choice; no administrator override. |
| Birthday month and day | No | Only me | Only me or Active Family Group | Supports shared birthdays without collecting a birth year in the first slice. |

Do not include profile photo, birth year, street address, precise/general location, biography, pronouns, medical details, emergency details, relationship labels, Household assignment, Safety Circle membership, or arbitrary custom fields in the first slice. Their exclusion from this slice does not decide whether they may appear in a later approved MVP work item.

## Recommended behavior

1. Display name is visible only to active members of the same Family Group.
2. Optional fields begin private and are shared with the active Family Group only through an explicit per-field choice.
3. There is no public, link-based, operator-wide, administrator-wide, Household, relationship-based, or selected-person visibility in this slice.
4. A user can remove an optional value or return its visibility to **Only me**.
5. API responses omit unauthorized field values; the client is not responsible for hiding data the server already disclosed.
6. Authorization tests must cover a different member, Family Administrator, inactive/former member, unrelated account, and Application Operator.
7. Minor profiles and guardian actions are unavailable rather than approximated.

## Non-goals

- Implementing code under this proposal.
- Choosing authentication, email/SMS, storage, hosting, or other providers.
- Defining account recovery, invitations, relationships, Household behavior, Safety Circle behavior, profile deletion/retention, or minor/guardian behavior.
- Using contact fields as verified identity claims.
- Adding public profiles, discovery, search outside the active Family Group, or custom fields.

## Conflicts, unknowns, and decisions required

- **Conflicts:** None detected among the loaded authoritative sources.
- **Unknowns:** Profile deletion/retention; whether later slices need a photo, birth year, time zone, address, biography, pronouns, emergency details, selected-person sharing, or profile management assistance.
- **Decision required:** Approve or revise the four-field matrix and the seven recommended behaviors. Approval establishes the product/privacy contract for the first work item but does not authorize implementation.

## Options

### Option A — Minimal four-field contract

- **Description:** Adopt the field matrix and behavior above.
- **Benefits:** Useful identity/contact foundation with restrained collection, explicit sharing, and a small authorization surface.
- **Risks/tradeoffs:** Richer profiles and selected-person sharing require later decisions.

### Option B — Display name only

- **Description:** Store only a display name in the first slice.
- **Benefits:** Smallest possible personal-data surface.
- **Risks/tradeoffs:** Does not yet satisfy the approved contact-information direction or enable shared birthdays.

### Option C — Rich profile now

- **Description:** Add photo, full birth date, address/location, biography, relationships, Household, and additional fields immediately.
- **Benefits:** More complete profile presentation.
- **Risks/tradeoffs:** Collects more personal information and forces several currently unresolved privacy/product decisions.

## Recommendation

Choose **Option A**. It is the leanest first slice that advances approved profile, contact, birthday, and privacy capabilities without creating a broad personal-data record or access model.

Before execution, create a lightweight work item that separately recommends and obtains approval for:

- recording the accepted field/visibility decision in canonical product, privacy, glossary, and requirement context and independently verifying the resulting context baseline;
- EF Core 10 with the existing local SQL Server development candidate and repository-owned migrations;
- a replaceable current-account boundary with a clearly development-only identity adapter;
- separate persistence concepts for Account, Family Member profile, Family Group membership, and administrator responsibility;
- server-side field filtering and authorization-denial tests; and
- a development-only adult fixture that cannot be enabled in production.

These are implementation recommendations, not approved product behavior or execution authorization.

## Verification plan

The later implementation must demonstrate:

1. only the four approved fields exist in the first slice;
2. optional fields default to **Only me**;
3. only the profile owner can edit values and visibility;
4. same-group members receive only fields explicitly shared with the Family Group;
5. administrators, operators, unrelated accounts, and inactive members receive no extra access;
6. sign-in identifiers are not exposed as contact fields automatically;
7. minor/guardian paths are unavailable;
8. API, persistence, client, migration, authorization, and boundary tests pass; and
9. formatting, builds, dependency vulnerability checks, and honest verification evidence pass.

## Decision and authorization

- **Decision:** `ACCEPTED` — Option A, minimal four-field contract and seven recommended behaviors
- **Decision ID:** `DEC-028`
- **Approved by:** Project owner
- **Approved at:** 2026-09-25
- **Execution authorized:** `NO`
