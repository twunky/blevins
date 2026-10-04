# PRP-017 — Plan Audience Eligibility and Lifecycle

## Metadata

- **Proposal ID:** `PRP-017`
- **Status:** `PARTIALLY ACCEPTED — DEC-054; remaining recommendations deferred`
- **Date:** `2026-09-26`
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.16`
- **Required context:**
  - `ai-context/README.md` and `ai-context/project-status.md` — proposal-only mode and current baseline
  - `ai-context/planning/proposals/PRP-014-item-audience-selection.md` and `ai-context/decision-log.md#dec-048` — approved audience choices and deferred questions
  - `ai-context/planning/proposals/PRP-016-plan-audience-selection.md` — selected Plan audience options and boundaries
  - `ai-context/planning/stories/WI-006-001-event-creation-and-audience.md` and `ai-context/planning/verification/VER-WI-006-001.md` — established Event recipient precedent
  - `ai-context/core/privacy-safety-contract.md#req-priv-005`, `#req-priv-006`, `#req-priv-011`, and `#req-priv-020` — access boundaries, membership, and explicitly deferred audience lifecycle behavior
  - `ai-context/core/product-contract.md#req-prod-013` — minor and guardian functionality is outside MVP
- **Affected authority:** Plan audience eligibility and lifecycle. Any accepted direction that changes canonical scope requires a decision and context update.
- **Model tier:** `DEEP`

## Request

Recommend reasonable defaults for the remaining Plan audience questions, with a narrow MVP approach and deeper audience management left for post-MVP review.

## Current evidence and context

- `REQ-PRIV-020` approves explicit audience selection for Events, Plans, and Trips. It names the applicable Family Group as a whole, specific people, or both as the audience choices.
- `REQ-PRIV-020` explicitly defers recipient eligibility, multiple-group behavior, later audience changes, and effects of membership changes.
- The owner selected all three audience choices for Plans under `PRP-016`.
- The Phase 6 Event slice selects specific active adult members of one applicable Family Group. It is a useful precedent, but its recipient rule is Event-specific until accepted for Plans.
- Minor and guardian functionality is outside MVP under `DEC-053` and `REQ-PROD-013`.
- Location sharing, notifications, and Sharing Sessions require independent authorization; item audience does not authorize them.

## Impact

These choices determine who can see a Plan and whether access can change after creation or membership changes. They are privacy and access decisions. A narrow MVP can follow the proven one-group Event pattern while making clear that future group aggregation and audience management are not yet supported.

## Recommendations and accepted direction

### MVP recommendation

- **Specific-person eligibility — accepted:** Allow selection of other active adult members of the one applicable Family Group. Exclude the creator from the recipient picker; the creator retains access as creator. Do not include people outside the active group. Minor and guardian functionality remains outside the MVP.
- **Whole-group option — accepted:** “This Family Group” means the one applicable Family Group as a whole.
- **Combined option — accepted for exclusion:** Do not offer “This Family Group and selected people” in MVP because selected people are limited to the same applicable group, making the combined choice redundant.
- **Multiple groups — deferred:** Multi-group Plan audience handling remains out of scope. This proposal does not decide how a creator who belongs to multiple groups chooses or combines groups.
- **Audience changes after creation — deferred:** Post-creation audience changes remain out of scope under `REQ-PRIV-020`. This proposal does not decide whether a later version should permit or prohibit changes.
- **Whole-group audience membership — accepted by `DEC-055`:** Access follows current active adult membership; joining gains access, ending active membership removes access, and restored active membership restores access.`r`n- **Individually selected recipient membership changes — deferred:** Access after a selected person leaves or rejoins remains undecided under `REQ-PRIV-020`.
- **Creator departure — partially decided:** Retain the Plan record if the creator leaves the applicable group. Do not automatically delete or transfer it to another owner. Post-departure read and edit behavior remains undefined.

### Post-MVP direction

- Revisit multi-group Plans, including whether group audiences combine as a union and how duplicate people are represented.
- Consider whether to permit creator-managed audience changes, and what confirmation or history would be needed.
- Define behavior when a recipient leaves and later rejoins, post-departure read/edit access for retained Plans, and whether any access can be restored.
- Review these choices across Plans, Trips, and Events for consistency without assuming a universal audience system.

## Non-goals

- Approving or implementing any of these recommendations in this proposal.
- Changing Events, Trips, Shared Family Dates, or `REQ-PRIV-020` without a separate decision.
- Adding minors, guardians, non-members, or cross-group recipients to MVP.
- Defining Plan collaborative editing permissions, ownership transfer, record retention, or notifications.
- Authorizing location sharing, notifications, or Sharing Sessions.
- Changing multi-group handling, later audience changes, or membership-change rules in any other feature.

## Conflicts, unknowns, and decisions required

- **Conflicts:** None identified. Plan-specific `REQ-PLAN-005` bounds this decision to Plans and does not change `REQ-PRIV-020` for other item types.
- **Unknowns:** Post-departure read/edit access for a retained Plan; handling recipients whose active membership ends or is restored; and how creators with multiple groups work with the one-group Plan boundary.
- **Decision required:** No further decision is needed for the audience choices, recipient eligibility, or record retention covered by `DEC-054`. `DEC-055` resolves whole-group access as a current membership audience. Multi-group handling, later changes, selected-recipient membership effects, and post-departure access remain deferred.

## Options

### Option A — Apply the narrow Event-style MVP rule (recommended)

- **Description:** Use one applicable Family Group and active adult recipients. Do not include post-creation audience editing or recipient membership-change behavior in the MVP implementation; defer those policies for a later decision.
- **Benefits:** Small, understandable, and aligned with the proven Event access pattern and conservative privacy defaults.
- **Risks/tradeoffs:** This is a Plan-specific policy choice that needs explicit approval. It does not resolve all-group plus selected-people semantics or creator departure.

### Option B — Defer all unresolved audience behavior

- **Description:** Keep Plan audience implementation blocked until recipient eligibility, multi-group use, post-creation changes, and membership lifecycle have all been decided together.
- **Benefits:** Avoids partial policy choices.
- **Risks/tradeoffs:** Delays Plan audience work even where a narrow MVP rule could be defined independently.

## Recommendation

The owner accepted the one-group active-adult recipient rule, the two non-combined audience choices, and Plan record retention after creator departure under `DEC-054`. Keep cross-group handling, post-creation audience editing, recipient membership changes, and post-departure read/edit behavior deferred.

## Verification plan

After owner decisions are recorded and canonical context is updated, verify the work item maps each accepted recipient and lifecycle rule to explicit acceptance checks. For later implementation, test selected, unselected, inactive, and out-of-group users; creator access; membership termination and rejoin behavior if approved; and the absence of post-creation audience mutation. Verify audience grants no location, notification, profile, Safety, or Sharing Session access. Report evidence per criterion and label self-verification if independent review is unavailable.

## Decision and authorization

- **Decision:** Specific active adult recipients from one applicable group; whole group or selected people only; combined option excluded; retain record if creator leaves. Remaining lifecycle recommendations deferred.
- **Decision ID:** `DEC-054`
- **Approved by:** Project owner
- **Approved at:** 2026-09-26
- **Execution authorized:** `NO`


