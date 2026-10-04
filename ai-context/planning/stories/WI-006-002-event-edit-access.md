# WI-006-002 — Event Editing Access

## Metadata

- **Work Item ID:** `WI-006-002`
- **Status:** `DONE`
- **Phase:** Phase 6 — Events
- **Context baseline:** `CTX-1.13`
- **Requirement references:**
  - `REQ-EVT-008`
  - `REQ-PRIV-005`
  - `REQ-PRIV-006`
  - `REQ-PRIV-011`
  - `REQ-PRIV-021`
  - `REQ-UX-003`
  - `REQ-UX-006`
- **Model tier:** `DEEP`

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md` | Work modes, authority, explicit authorization, and verification rules | Entry |
| `ai-context/project-status.md` | Phase 6 checkpoint and active baseline | Entry |
| `ai-context/planning/roadmap.md` | Confirms Phase 6 is current | Before execution |
| `ai-context/planning/proposals/PRP-015-event-edit-access.md` and `ai-context/decision-log.md#dec-051` | Approved direction and exact permission boundaries | Before execution |
| `ai-context/workflows/events.md#req-evt-008` | Event fields that may be edited and fixed-at-creation permissions | Before execution |
| `ai-context/core/privacy-safety-contract.md#req-priv-005`, `#req-priv-006`, `#req-priv-011`, and `#req-priv-021` | Explicit edit grants, active membership, and no administrator bypass | Before execution |
| `ai-context/core/ux-contract.md#req-ux-003` and `#req-ux-006` | Lightweight, accessible view/edit controls | Before execution |
| Existing Event API, application, persistence, and UI entry points | Preserve project patterns and identify implementation/test surfaces | During execution, after authorization |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Plans, Trips, and Sharing workflows | This item covers Event access only. |
| Safety workflows and data | Event editing grants no Safety access. |
| Notifications, invitations, and RSVP | Separate deferred workflow steps; not required to implement Event detail editing. |
| Bootstrap, archive, and historical verification material | Not needed under active `CTX-1.13`. |

## Goal

Let an Event creator assign active adult members of one applicable Family Group view or edit access at creation, and allow only the creator or selected editors to update the Event's name, description, date, time, and location.

## Boundaries / non-goals

- Implement only Events and only one applicable Family Group per Event.
- At Event creation, each selected active adult member receives either view or edit access. Edit access includes view access; the creator retains edit access.
- Require active membership on Event reads and edits. Family Administrator status alone grants no access.
- Only name, description, date, time, and location may be edited. Do not edit creator identity, group, audience, or editor assignments.
- Audience and editor assignments remain fixed after creation in this initial slice.
- No event deletion/cancellation, invitations, RSVP, Needs/Tasks, claims, updates, notifications, location sharing, or unrelated features.
- No non-member friends, minors, guardians, multiple-group handling, or generalized role system.

## Acceptance checks

1. `AC-01` — Event creation lets the creator assign each selected active adult member either view or edit access; selecting edit also grants view, and the creator retains edit access.
2. `AC-02` — An active selected editor can update Event name, description, date, time, and location; the creator can update those same fields.
3. `AC-03` — A view-only member, an unrelated group member, an inactive member, and an administrator without an explicit edit assignment cannot update Event details.
4. `AC-04` — Every Event read or edit enforces active membership in the applicable Family Group; permission data does not grant access to any other private feature.
5. `AC-05` — The initial flow cannot change the Event audience or editor assignments after creation; no route or control implements those changes.
6. `AC-06` — The view/edit choice uses accessible labeled controls and clear feedback, and remains a lightweight extension of the existing Event flow.
7. `AC-07` — Focused API/service/persistence and UI tests map the permission and field boundaries above; the EF model/migration and application build verify successfully.

## Tasks

- [x] `TASK-001` — Review and approve this bounded work item and explicitly authorize execution.
- [x] `TASK-002` — Inspect the Event creation/read paths and identify minimal schema, API, and UI changes.
- [x] `TASK-003` — Persist the creator's per-member view/edit assignment at creation and enforce creator/editor-only detail updates server-side.
- [x] `TASK-004` — Add the accessible per-member access choice during Event creation and event edit controls for authorized users.
- [x] `TASK-005` — Add focused tests for authorized edits, rejected edits, active membership, and immutable permission assignment.
- [x] `TASK-006` — Run verification checks and create `VER-WI-006-002`; the project owner confirmed the gated SQL persistence suite passes from the Visual Studio terminal.

## Risks and blockers

- This is a `DEEP` privacy/permissions item. `DEC-032` does not auto-approve it; explicit project-owner approval and execution authorization are required.
- The Codex sandbox cannot run the SQL persistence tests under the user's Windows identity; the project owner confirmed the focused tests pass from Visual Studio. Keep encryption enabled.

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner
- **Authorized at:** 2026-09-26
- **Authorized scope:** Only the goal, boundaries, acceptance checks, and tasks in this work item: per-person view/edit choice at Event creation for active adult members of one applicable Family Group, and creator/editor updates to name, description, date, time, and location. No post-creation audience/editor changes or other Event workflows.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| Approved requirements and work-item scope | `DEC-051`, `REQ-PRIV-021`, `REQ-EVT-008`, and this authorized item | `PASS` |
| Application behavior | Implementation and API/service/UI tests; project owner reports the SQL-gated persistence tests pass from the Visual Studio terminal | `PASS` |
| Acceptance checks | `VER-WI-006-002` | `PASS` |

- **Verification report:** `ai-context/planning/verification/VER-WI-006-002.md`
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `DONE` (`VER-WI-006-002` passed; live SQL persistence tests were reported successful by the project owner)
