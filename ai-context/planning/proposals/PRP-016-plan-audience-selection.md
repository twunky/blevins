# PRP-016 — Plan Audience Selection

## Metadata

- **Proposal ID:** `PRP-016`
- **Status:** `ACCEPTED; CANONICAL CTX-1.15` (Plan-specific audience choice refined by `DEC-054`)
- **Date:** `2026-09-26`
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.16`
- **Required context:**
  - `ai-context/README.md` and `ai-context/project-status.md` — operating mode and current baseline
  - `ai-context/planning/roadmap.md` — Phase 7 is current for planning only
  - `ai-context/planning/proposals/PRP-014-item-audience-selection.md` and `ai-context/decision-log.md#dec-048` — accepted cross-item audience direction and deferred questions
  - `ai-context/workflows/plans-trips.md#req-plan-001` through `#req-plan-004` — approved Plan concept and creation sequence
  - `ai-context/workflows/sharing-checkins.md#req-share-004` and `ai-context/core/privacy-safety-contract.md#req-priv-002`, `#req-priv-004` through `#req-priv-006`, `#req-priv-020` — sharing separation and access boundaries
  - `ai-context/planning/stories/WI-006-001-event-creation-and-audience.md`, `WI-006-002-event-edit-access.md`, and `ai-context/planning/verification/VER-WI-006-001.md` — Phase 6 implementation reference, not authority for Plan behavior
  - `ai-context/core/product-contract.md#req-prod-013` and `ai-context/decision-log.md#dec-053` — minor/guardian functionality is outside MVP and reserved for future work
  - `ai-context/workflows/plans-trips.md#req-plan-005` and `ai-context/decision-log.md#dec-054` — approved Plan-specific audience choices and creator-departure retention
- **Affected authority:** Plan creation and item access UX within existing approved Plan audience requirements
- **Model tier:** `DEEP`

## Request

Define a Plan creation experience that follows the approved audience options and is informed by the completed Event audience implementation, without transferring Event-only permission behavior to Plans.

## Current evidence and context

- `REQ-PLAN-003` places “Choose item audience” after Plan timing and optional expected return, and before the optional expected check-in.
- `REQ-PRIV-020` requires explicit item audience selection. Under Plan-specific `REQ-PLAN-005`, the MVP choices are the one applicable Family Group as a whole or specific active adult members of that same group; a combined choice is omitted because it adds no recipients under the one-group boundary.
- In Phase 6, `WI-006-001` implemented explicit per-person selection of active adult members of one applicable Family Group for Events. `WI-006-002` separately implemented Event-specific view/edit assignments. The recipient boundary is now explicitly approved for Plans; Event edit permissions still do not establish Plan edit rights.
- Under `DEC-054`, retain the Plan record if its creator leaves the applicable group. Do not infer ownership transfer or post-departure read/edit behavior.
- The Plan creation flow includes expected check-in, but a Plan audience does not authorize location sharing or a Sharing Session.
- Trip audience choices are in approved workflow context, but Trip recipient eligibility and implementation have not been established in the reviewed Phase 6 evidence.

## Impact

This proposal affects who can access a Plan and adds an explicit audience choice to Plan creation. It is privacy-sensitive. Audience access must remain limited to the Plan and must not enable location sharing, notification delivery, profile access, safety information, or Sharing Session access. Event UI patterns may inform the selected-person picker, but Event view/edit semantics must not be presumed for Plans.

## Requirements

- Plan creation presents either the one applicable Family Group as a whole or specific active adult members of that same group. Do not present a combined choice.
- Place the choice in the position specified by `REQ-PLAN-003`.
- For specific-person selection, use an explicit, filterable person picker informed by Phase 6 Event UI. The creator is not a selectable recipient and retains access as creator.
- Retain the Plan record if its creator leaves the applicable Family Group. Do not automatically delete or transfer it; post-departure read/edit permissions remain undefined.
- The selected audience governs access to the Plan only.
- Do not add per-person view/edit permissions for Plans under this proposal. Plan edit rights require separate approved requirements if they are to be expanded beyond existing authorization.
- Preserve the existing separation between item audience, location sharing, notification delivery, and Sharing Sessions.

## Non-goals

- Changing Trips, Events, Shared Family Dates, or any other item type.
- Defining multi-group handling, later audience changes, or membership-change effects for individually selected recipients; these remain deferred by `REQ-PRIV-020`. `DEC-055` defines current active membership behavior for whole-group Plans only.
- Minor and guardian functionality, which is outside MVP under `REQ-PROD-013` and remains future work under `OD-003`.
- Adding Plan collaboration roles or a generalized permission model.
- Changing notification consent, location-sharing authorization, or any safety/privacy contract.
- Application implementation or architecture selection.

## Conflicts, unknowns, and decisions required

- **Conflicts:** None identified if `REQ-PRIV-020` and the Plan workflow remain controlling, and Event-specific edit permissions are not transferred.
- **Unknowns:** Post-departure read/edit permissions for a Plan whose creator leaves remain undefined. Minor and guardian functionality is excluded from MVP and remains future work; this proposal does not define it.
- **Decision required:** `DEC-055` additionally resolves current active membership behavior for whole-group Plan audiences. Keep multi-group handling, later audience changes, membership-change effects for individually selected recipients, and post-departure access details deferred unless a future decision is approved.

## Options

### Option 1 — Whole group or selected people (selected)

- **Description:** Present the one applicable Family Group as a whole or specific active adult people in that same group. Omit “both,” which is redundant under the one-group rule.
- **Benefits:** Gives the creator distinct broad or narrow access choices and reuses the established Event recipient boundary.
- **Risks/tradeoffs:** Multi-group audiences and post-creation audience management remain unavailable in the MVP.

### Option 2 — Event-style specific people only

- **Description:** Limit Plan creation to selecting specific active adult members of one applicable Family Group, following `WI-006-001`.
- **Benefits:** Closely matches the shipped Event audience picker and provides a narrower initial audience.
- **Risks/tradeoffs:** Narrows the audience options already approved in `REQ-PRIV-020`; would require an explicit change to the accepted Plan direction.

## Recommendation

Use Option 1, as approved under `DEC-054`. Reuse the Phase 6 Event picker only for its accessible, filterable explicit person-selection interaction. Do not reuse Event edit permissions. Retain the Plan record if its creator leaves, without implying ownership transfer or deciding post-departure access. Minor and guardian functionality remains outside MVP under `DEC-053` and is not defined here.

## Conceptual mock

```text
CREATE PLAN

Activity or destination
[ Visit the lake                         ]

Timing
[ Date             ]   [ Start time     ]

Expected return (optional)
[ Date             ]   [ Time           ]

Who can see this plan? *
( ) This Family Group
( ) Selected people

If “Selected people” is chosen:
  Filter family members
  [ Search by name                         ]

  [ ] Alex Morgan
  [ ] Jordan Lee
  [ ] Casey Rivera

Expected check-in (optional)
[ Date             ]   [ Time           ]

[ Save plan ]
```

This is a conceptual mock for review, not an approved final design or implementation specification. Names and available recipients are illustrative. Minor and guardian functionality is outside MVP and remains future work.

## Verification plan

For a future authorized work item, map each acceptance check to implementation and test evidence. Verify the audience step's placement in the Plan flow; explicit selection and persistence of one of the approved audience choices; access limited to that Plan and its selected audience; and the separation from location sharing, notifications, profiles, safety information, and Sharing Sessions. Verify all declared exclusions. Report independence and any environment limits; use `SELF-VERIFIED` if independent verification is unavailable.

## Decision and authorization

- **Decision:** Either the one applicable Family Group or specific active adult members of that group; no combined choice. Retain the Plan record if its creator leaves.
- **Decision ID:** `DEC-054`
- **Approved by:** Project owner
- **Approved at:** 2026-09-26
- **Execution authorized:** `NO`


