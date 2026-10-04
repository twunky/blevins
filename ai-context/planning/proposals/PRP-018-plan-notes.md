# PRP-018 — Optional Plan Notes at Creation

## Metadata

- **Proposal ID:** `PRP-018`
- **Status:** `ACCEPTED — CTX-1.17 active; WI-007-002 authorized`
- **Date:** `2026-09-26`
- **Requested by:** Project owner request for the next Phase 7 step
- **Context baseline:** `CTX-1.16`
- **Required context:**
  - `ai-context/README.md` and `ai-context/project-status.md` — operating rules and current checkpoint
  - `ai-context/planning/roadmap.md` — Phase 7 status
  - `ai-context/workflows/plans-trips.md#req-plan-002`, `#req-plan-003`, and `#req-plan-005` — approved Plan information, creation sequence, and audience behavior
  - `ai-context/core/privacy-safety-contract.md#req-priv-020` — Plan audience access is limited to the item
  - `ai-context/planning/stories/WI-007-001-plan-audience-creation.md` and `ai-context/planning/verification/VER-WI-007-001.md` — completed Plan creation and audience implementation boundary
- **Affected authority:** Plan creation workflow and Plan information requirements; no change to audience eligibility or access rules
- **Model tier:** `DEEP` (new potentially sensitive content visible to a Plan audience)

## Request

Define a bounded next step that adds the optional notes already contemplated by `REQ-PLAN-002` to Plan creation and viewing.

## Current evidence and context

- `REQ-PLAN-002` says a Plan may include notes. `REQ-PLAN-003` defines the creation flow but does not place a notes step or define note length and presentation.
- `WI-007-001` is complete: a Plan is created with an explicit whole-group or selected-people audience, and that audience controls who can view the Plan.
- `DEC-055` makes a whole-group Plan visible to current active adult members, including members who join after creation. Notes on such a Plan would therefore become visible to those future active members.
- Plan audience choice does not authorize location sharing, notification delivery, profile or Safety access, or a Sharing Session.

## Impact

Notes can contain sensitive free text. They would use the Plan's existing audience and appear to every person who can view that Plan. The creator should see this consequence before saving a whole-group Plan. Adding notes requires a small Plan workflow/context refinement and a bounded implementation work item after owner approval.

## Requirements proposed for approval

- Add an optional plain-text **Notes** field at creation, after optional expected check-in and before Save Plan. Update the canonical Plan flow to show this position.
- Limit notes to 1,000 characters. Treat blank or whitespace-only input as no note; preserve internal line breaks and render note text as text, not markup.
- Display the saved note only wherever the Plan is visible under its existing audience. Use the same server-side access check as other Plan details.
- State near the field that notes are visible to the chosen Plan audience. For the whole-group choice, explain that future active adult group members can also see the Plan and its notes.
- Leave note editing, later audience changes, notifications, check-in behavior, and Plan completion for separate work.

## Proposed bounded work item and acceptance checks

**Goal:** A creator can optionally add a note when creating a Plan, and permitted viewers can read it with the Plan.

1. The Plan form offers the optional Notes field in the approved sequence and explains who can see it.
2. The API validates and saves up to 1,000 characters; blank input saves no note. Existing Plans remain valid with no note.
3. The note is returned only through Plan access already authorized by the saved audience. Unselected, inactive, and out-of-group users gain no note access.
4. Note text is displayed safely as text, including internal line breaks, without enabling markup or scripts.
5. Focused verification covers creation, persistence, audience-limited read, blank/length validation, UI rendering, and no sharing or notification side effects. Record evidence in a separate self-verified or independent report.

## Non-goals

- Plan detail editing, note changes after creation, check-ins, completion, or deletion.
- Multi-group handling, later audience changes, or selected-recipient membership-change behavior.
- Creator post-departure read/edit access or ownership transfer.
- Trips, Events, Shared Family Dates, minor/guardian functions, and other workflows.
- Location sharing, Sharing Sessions, notifications, profile information, or Safety access.

## Conflicts, unknowns, and decisions required

- **Conflicts:** None if notes use the existing Plan audience and the canonical Plan flow is updated after approval.
- **Unknowns:** The approved context does not specify a character limit, exact input placement, or note presentation. The values above are proposals, not current requirements.
- **Decision required:** Accept or revise the optional plain-text note, 1,000-character limit, placement, visibility explanation, and bounded work-item checks. Canonical context and a separately authorized work item are required before implementation.

## Options

### Option A — Add optional notes at creation (recommended)

- **Description:** Add the scoped field and audience explanation above.
- **Benefits:** Completes a small part of approved Plan information without introducing edit or check-in permissions.
- **Risks/tradeoffs:** Free text can be sensitive; creators must understand that whole-group access follows current membership.

### Option B — Defer notes until Plan updates are defined

- **Description:** Keep the creation form as it is and revisit notes with Plan editing.
- **Benefits:** One later design could define both creation and editing.
- **Risks/tradeoffs:** The optional Plan information already named in `REQ-PLAN-002` remains unavailable until the larger edit-permission decisions are made.

## Recommendation

Choose Option A. It is a small next step grounded in the approved Plan information requirement. Make the whole-group visibility consequence clear in the creation form. Keep Plan editing and check-in/completion for later proposals.

## Verification plan

After approval and canonical alignment, verify every work-item acceptance check against application and test evidence. Run focused .NET and SQL persistence tests with an explicit test count, Angular component tests, TypeScript checks, and the production build. Record any environment limitation without treating an unrun test as a pass.

## Decision and authorization

- **Decision:** Option A accepted: optional plain-text Plan notes at creation, up to 1,000 characters, after expected check-in, displayed to the existing Plan audience with the whole-group membership notice.
- **Decision ID:** `DEC-056`
- **Approved by:** Project owner, “I choose option a for PRP-018.”
- **Approved at:** 2026-09-26
- **Execution authorized:** `YES` for candidate context preparation and drafting (prior scope), and separately for implementing and verifying `WI-007-002` (project owner, 2026-09-26). No other Phase 7 work is authorized by this approval.
