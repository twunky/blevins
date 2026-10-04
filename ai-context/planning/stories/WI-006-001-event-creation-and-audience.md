# WI-006-001 — Event Creation and Explicit Audience

## Metadata

- **Work Item ID:** `WI-006-001`
- **Status:** `DONE`
- **Phase:** Phase 6 — Events
- **Context baseline:** `CTX-1.12`
- **Requirement references:**
  - `REQ-EVT-001`
  - `REQ-EVT-005`
  - `REQ-PRIV-005`
  - `REQ-PRIV-006`
  - `REQ-PRIV-011`
  - `REQ-PRIV-020`
  - `REQ-UX-003`
  - `REQ-UX-006`
- **Model tier:** `DEEP`

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md` | Work modes, authorization, authority, and verification rules | Entry |
| `ai-context/project-status.md` | Phase 6 checkpoint and active baseline | Entry |
| `ai-context/planning/roadmap.md` | Confirms Phase 6 is current | Before execution |
| `ai-context/workflows/events.md#req-evt-001` and `#req-evt-005` | Event creation scope and standard flow | Before execution |
| `ai-context/core/privacy-safety-contract.md#req-priv-005`, `#req-priv-006`, `#req-priv-011`, and `#req-priv-020` | Access boundaries, active membership, and explicit item audience | Before execution |
| `ai-context/core/ux-contract.md#req-ux-003` and `#req-ux-006` | Progressive disclosure and accessible family workflows | Before execution |
| Existing application files directly needed for the approved Event slice | Preserve established project patterns and identify affected entry points | During execution, after authorization |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Plans, Trips, and Sharing workflows | This item covers Events only; location sharing remains independently authorized. |
| Safety workflows and data | Event creation does not grant access to safety information. |
| Trusted Contact details | Separate deferred concept unrelated to Event audiences. |
| Bootstrap, archive, and historical verification material | Not needed under active `CTX-1.12`. |

## Goal

Provide a bounded Event creation slice in which an adult member explicitly selects the Event audience from active adult members of one applicable Family Group, consistent with `CTX-1.12`.

## Boundaries / non-goals

- Cover Events only; do not implement Plans, Trips, Sharing Sessions, or notification behavior.
- Keep the first slice to one applicable Family Group. Multiple-group selection is deferred.
- For this first slice, the creator selects one or more specific active adult members of the applicable Family Group. Group-wide and combined audiences remain outside this work item.
- People outside the active Family Group, including non-member friends, are excluded from this item.
- Exclude minors and guardian behavior under `OD-003`.
- Audience membership grants only access to this Event; it does not grant profile, Safety Circle, location, or other private-data access.
- This item does not define or grant collaborative edit permissions. Any permitted update behavior must follow the existing Event contract and be confirmed in the approved task breakdown; changes to role/access rules require a separate decision.
- Audience changes after creation remain deferred. If a selected member's active Family Group membership ends, their Event access ends under `REQ-PRIV-011`; do not add reactivation, transfer, or other membership-transition behavior.
- Event invitation delivery and RSVP/participation are outside this first audience-visibility slice. Event audience selection must not imply invitation or RSVP state.
- Date and time are captured as entered for the Event; this slice has no timezone setting or automatic timezone conversion.
- No event chat, social feed, or expanded MVP scope.
- No technical architecture or provider selection beyond previously approved project architecture.

## Acceptance checks

1. `AC-01` — An active adult member can create an Event with the required event details from `REQ-EVT-001`.
2. `AC-02` — During creation, the creator explicitly chooses one or more other active adult members of one applicable Family Group as the audience.
3. `AC-03` — Only the creator and selected audience can access the Event; access does not arise from administrator status, unrelated membership, RSVP state, or invitation delivery alone.
4. `AC-04` — Event audience access does not grant access to profile fields, safety information, location sharing, notifications, or other items.
5. `AC-05` — The creation steps in the approved Event sequence remain lightweight and use accessible labeled controls and feedback. Invitation, RSVP, Needs/Tasks, updates, and completion are explicitly outside this creation slice.
6. `AC-06` — The implementation and verification demonstrate that excluded areas remain out of scope; each criterion maps to evidence in a separate verification report.

## Tasks

- [x] `TASK-001` — Review and approve this bounded work item and execution authorization.
- [x] `TASK-002` — Inspect only the application entry points needed for the approved Event slice; no scope adjustment was required.
- [x] `TASK-003` — Implement Event creation and explicit audience selection within the approved boundaries.
- [x] `TASK-004` — Add focused tests for audience enforcement, access boundaries, and the Event creation flow.
- [x] `TASK-005` — Run the authorized verification checks and record evidence without expanding scope.
- [x] `TASK-006` — Produce `VER-WI-006-001` and update status based on recorded evidence.

## Risks and blockers

- `REQ-PRIV-020` leaves recipient eligibility and membership-change effects deferred. This proposal bounds initial selection to active adult members of one applicable Family Group based on current owner direction; membership-change handling remains out of scope and must not be guessed.
- This `DEEP` privacy/access work requires explicit project-owner approval and execution authorization. `DEC-032` does not auto-approve it.

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner direction to begin Phase 6 after review of this proposed bounded item
- **Authorized at:** 2026-09-26
- **Authorized scope:** Only the goal, boundaries, and acceptance checks above; Event creation and explicit audience selection for active adult members of one applicable Family Group. Edit permissions, non-member friends, multiple-group handling, post-creation audience changes, membership-change effects, location sharing, notifications, and unrelated features are excluded.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| Work-item scope and traceability | This approved work item and active canonical requirements | `PASS` |
| Application behavior | Event UI, API, service, repository, EF model and migration inspected; .NET and Angular tests/build passed | `PASS` |
| Acceptance checks | `VER-WI-006-001` | `PASS` (self-verified; local SQL Server test limitation documented) |

- **Verification report:** `ai-context/planning/verification/VER-WI-006-001.md`
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `PASS` (SQL Server persistence execution unavailable in the local environment; limitation documented in the verification report)
