---
Status: ACTIVATED — historical candidate record
Authority: Historical proposal; active canonical wording is in `workflows/plans-trips.md`
Context-Baseline: CTX-1.16
Version: 1.0-candidate
Decision: DEC-056
---

# Candidate CTX-1.17 — Plan Notes at Creation

This document contains proposed amendments to `workflows/plans-trips.md` for review and activation. Until the project owner activates `CTX-1.17`, the approved workflow remains unchanged at `CTX-1.16`.

## Proposed replacement for REQ-PLAN-003

The standard Plan flow is:

`Create Plan → Activity / destination → Timing → Optional expected return → Choose item audience (per REQ-PRIV-020) → Optional expected check-in → Optional Notes (per REQ-PLAN-006) → Save plan → Update if needed → Check in / complete`

## Proposed addition to REQ-PLAN-005

- A Plan note is part of the Plan and follows its existing audience. For a whole-group Plan, the note is visible to current active adult members, including members who become active in the group later.

## Proposed new REQ-PLAN-006

Optional Plan notes at creation:

- The creator may enter an optional plain-text note when creating a Plan, after the optional expected check-in and before saving.
- A note may contain up to 1,000 characters. Blank or whitespace-only input is treated as no note.
- Preserve internal line breaks. Display note content as text; do not interpret it as markup.
- Show that the note follows the Plan audience. For a whole-group audience, explain that future active adult members may also see the Plan and its note.
- A note does not create access beyond the Plan's existing audience and access enforcement.
- Editing or removing a note after creation is not included in this requirement.

## Candidate boundaries

These amendments do not define post-creation audience changes, selected-recipient membership-change behavior, creator post-departure permissions, multi-group handling, notifications, location sharing, Sharing Sessions, check-in behavior, or minor/guardian behavior.
