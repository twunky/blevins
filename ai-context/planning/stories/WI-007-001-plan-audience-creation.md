# WI-007-001 — Plan Audience Selection at Creation

## Metadata

- **Work Item ID:** `WI-007-001`
- **Status:** `DONE`
- **Phase:** Phase 7 — Plans, Trips & Sharing
- **Context baseline:** `CTX-1.16`
- **Requirement references:**
  - `REQ-PLAN-001`
  - `REQ-PLAN-002`
  - `REQ-PLAN-003`
  - `REQ-PLAN-004`
  - `REQ-PLAN-005`
  - `REQ-PRIV-002`
  - `REQ-PRIV-004`
  - `REQ-PRIV-005`
  - `REQ-PRIV-006`
  - `REQ-PRIV-020`
  - `REQ-SHARE-004`
- **Model tier:** `DEEP`

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md` | Work modes, authority, scope, and verification rules | Entry |
| `ai-context/project-status.md` | Phase 7 planning checkpoint and active baseline | Entry |
| `ai-context/planning/roadmap.md` | Confirms Phase 7 planning status | Before execution |
| `ai-context/planning/proposals/PRP-014-item-audience-selection.md` and `ai-context/decision-log.md#dec-048` | Approved audience direction and explicitly deferred questions | Before execution |
| `ai-context/planning/proposals/PRP-016-plan-audience-selection.md` | Project-owner-selected Plan audience choices and Plan-specific boundaries | Before execution |
| `ai-context/planning/proposals/PRP-017-plan-audience-lifecycle.md` and `ai-context/decision-log.md#dec-054` and `#dec-055` | Eligibility, audience choices, creator-departure retention, and whole-group current-membership behavior | Before execution |
| `ai-context/core/product-contract.md#req-prod-013` and `ai-context/decision-log.md#dec-053` | Confirms minor/guardian functionality is future work and outside the MVP | Before execution |
| `ai-context/workflows/plans-trips.md#req-plan-001` through `#req-plan-005` | Plan definition, creation sequence, and lifecycle boundary | Before execution |
| `ai-context/core/privacy-safety-contract.md#req-priv-002`, `#req-priv-004` through `#req-priv-006`, and `#req-priv-020` | Explicit item sharing, separation from location sharing, and access boundaries | Before execution |
| `ai-context/workflows/sharing-checkins.md#req-share-004` | Confirms a Plan context does not authorize a Sharing Session | Before execution |
| Existing Plan-related application entry points | Identify implementation and verification surfaces while preserving project patterns | During execution |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Trips workflow details | This item covers Plans only. |
| Event implementation and `REQ-PRIV-021` | Phase 6 Event work is complete; Event edit permissions do not define Plan permissions. |
| Shared Family Dates | Their narrower visibility rules are separate and unchanged. |
| Safety workflows and records | Plan audience grants no Safety access. |
| Notification behavior | Item audience does not authorize notification delivery. |
| Bootstrap, archive, and unrelated historical verification | Not needed under active `CTX-1.16`. |

## Goal

Add an explicit audience choice during Plan creation and make the saved choice govern access to that Plan, within approved Plan and privacy requirements.

## Boundaries / non-goals

- Cover Plans only. Trips, Events, Shared Family Dates, and other item types are outside this item.
- Plan creation offers either the one applicable Family Group as a whole or specific active adult members of that same group. The combined “both” choice is excluded because it is redundant under this boundary.
- The creator is not selectable as a recipient and retains access as creator.
- If the creator leaves the applicable Family Group, retain the Plan record; do not automatically delete or transfer it. Post-departure read/edit behavior is not defined by this item.
- Minor and guardian functionality is outside the MVP and is not defined by this item.
- Do not transfer Event-specific per-member view/edit assignments to Plans. Plan editing permissions are not expanded by this item.
- Do not infer access from family relationship, participation, Family Administrator status, or prior sharing.
- Audience choice controls access to the Plan only. It does not authorize location sharing, notifications, profile access, Safety information, or Sharing Session access.
- Whole-group audience access follows current active adult membership under `DEC-055`. Do not define membership-change effects for individually selected recipients, multi-group handling, or later audience changes; these remain deferred under `REQ-PRIV-020`.
- Do not broaden Shared Family Date visibility or change its separate privacy contract.
- No technical architecture, provider, or implementation-domain design is selected by this item.

## Acceptance checks

1. `AC-01` — Plan creation includes an explicit audience choice in the position defined by `REQ-PLAN-003`.
2. `AC-02` — The creator can make only audience choices supported by the approved Plan and item-audience requirements; no unsupported recipient rule is inferred.
3. `AC-03` — The saved audience governs access to that Plan, and access is not granted solely by relationship, participation, or administrator status.
4. `AC-04` — Plan audience access is confined to the Plan and does not enable location sharing, notifications, profile or Safety access, or a Sharing Session.
5. `AC-05` — Whole-group access tracks current active adult membership (new membership grants access; ending removes it; restored active membership restores it). Multi-group handling, later audience changes, membership-change effects for individually selected recipients, and unrelated workflows remain outside the implementation.
6. `AC-06` — A Plan record is not automatically deleted or transferred when its creator leaves the applicable Family Group; post-departure read/edit behavior is not inferred.
7. `AC-07` — Focused verification maps each applicable requirement and acceptance check to evidence and a result; verification findings do not redefine requirements.

## Tasks

- [x] `TASK-001` — After explicit execution authorization, inspect only the application entry points needed for Plan creation and access enforcement.
- [x] `TASK-002` — Implement Plan audience selection and persist/enforce the selected Plan audience within the approved scope.
- [x] `TASK-003` — Retain Plan records when their creator leaves the applicable Family Group, without adding ownership transfer or post-departure permission behavior.
- [x] `TASK-004` — Add focused checks for creation, persistence, audience-limited access, retention, and privacy boundaries.
- [x] `TASK-005` — Verify the approved acceptance checks and record evidence in a separate verification report.

## Risks and blockers

- **Resolved:** `DEC-055` explicitly approves dynamic current-membership access for whole-group Plan audiences.
- **Deferred:** Multi-group Plan audiences, post-creation audience changes, membership-change effects for individually selected recipients, and post-departure read/edit behavior are not defined by `DEC-054` or this work item. Do not infer them.
- **Verification:** SQL-backed Plan persistence/access tests passed in an unrestricted test run, and the Angular production build succeeded. See `planning/verification/VER-WI-007-001.md`.
- Minor and guardian functionality is separately excluded from MVP under `REQ-PROD-013`.
- This is a `DEEP` privacy/access work item. `DEC-032` does not auto-approve it.
- The project owner explicitly approved and authorized execution of this bounded work item. Implementation and completion verification are complete.

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner
- **Authorized at:** 2026-09-26
- **Authorized scope:** `WI-007-001` goal, boundaries, acceptance checks, and tasks under `CTX-1.16`; implementation scope includes the whole-group current-membership behavior approved in `DEC-055`.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| Work-item scope and traceability | Approved context baseline `CTX-1.16`, `REQ-PLAN-001` through `REQ-PLAN-005`, `REQ-PRIV-020`, `REQ-PROD-013`, and this work item | `PASS` |
| Plan audience and creator-retention requirements | `DEC-054`, `REQ-PLAN-005`, and this work item | `PASS` |
| Application behavior | Plan model, API, persistence, audience enforcement, and UI are implemented; all three SQL-backed Plan persistence tests and focused non-SQL tests pass. | `PASS` |
| Build and tests | .NET build passes; all 81 ungated .NET tests pass; Angular TypeScript check and 13 component tests pass; Angular production build succeeds. | `PASS` |
| Acceptance checks | `VER-WI-007-001.md` maps every AC to passing evidence. | `PASS` |

- **Verification report:** `planning/verification/VER-WI-007-001.md`
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `DONE`; `VER-WI-007-001` is `SELF-VERIFIED`, `PASS`.



