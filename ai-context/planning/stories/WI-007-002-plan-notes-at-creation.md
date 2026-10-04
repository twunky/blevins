# WI-007-002 — Optional Plan Notes at Creation

## Metadata

- **Work Item ID:** `WI-007-002`
- **Status:** `DONE`
- **Phase:** Phase 7 — Plans, Trips & Sharing
- **Context baseline:** `CTX-1.17` — approved and active
- **Requirement references:**
  - `REQ-PLAN-002`
  - `REQ-PLAN-003`
  - `REQ-PLAN-005`
  - `REQ-PLAN-006`
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
| `ai-context/README.md` and `ai-context/project-status.md` | Work modes, authority, active baseline, and execution checkpoint | Entry |
| `ai-context/planning/proposals/PRP-018-plan-notes.md` and `ai-context/decision-log.md#dec-056` | Accepted notes direction and execution boundary | Before execution |
| `ai-context/planning/verification/VER-CTX-1.17.md` | Confirms the activated context amendment was consistency-verified | Before execution |
| `ai-context/workflows/plans-trips.md#req-plan-002`, `#req-plan-003`, `#req-plan-005`, and `#req-plan-006` | Active Plan information, audience, and notes rules under baseline CTX-1.17 | Before execution |
| `ai-context/planning/candidates/CTX-1.17-plan-notes.md` | Historical candidate record; not normative after activation | Excluded from execution context |
| `ai-context/core/privacy-safety-contract.md#req-priv-002`, `#req-priv-004`, `#req-priv-005`, `#req-priv-006`, and `#req-priv-020` | Item access and sharing boundaries | Before execution |
| `ai-context/workflows/sharing-checkins.md#req-share-004` | A Plan or note does not authorize a Sharing Session | Before execution |
| `ai-context/planning/stories/WI-007-001-plan-audience-creation.md` and `ai-context/planning/verification/VER-WI-007-001.md` | Completed Plan audience behavior and implementation patterns | During execution |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Trip and Event workflows | Notes are added to Plans only. |
| Safety workflows and records | Notes grant no Safety access. |
| Minor and guardian requirements | Functionality is outside MVP under `DEC-053`. |
| Post-creation Plan editing, membership lifecycle, and unrelated historical verification | Explicitly outside this item. |

## Goal

Allow a creator to add an optional note when creating a Plan, then show the saved note to people who can already view that Plan.

## Boundaries / non-goals

- Add an optional plain-text Notes field, maximum 1,000 characters, after expected check-in and before Save Plan.
- Blank or whitespace-only input means no note. Preserve internal line breaks and display text safely without interpreting markup.
- Explain note visibility next to the field. Notes follow the Plan's existing audience and access enforcement. Whole-group Plans' notes are visible to future active adult group members under `DEC-055`.
- Implement creation and read display only. Do not allow note edits after creation.
- Do not change audience selection, Plan editing, check-in, completion, notification delivery, location sharing, or Sharing Sessions.
- Do not add multi-group behavior, later audience changes, selected-recipient membership-change behavior, creator post-departure permissions, minors, or guardians.
- Keep existing Plan records valid with no note.

## Acceptance checks

1. `AC-01` — Plan creation displays the Notes field after expected check-in and before Save Plan, in the order specified by `REQ-PLAN-003`.
2. `AC-02` — Notes are optional, plain text, limited to 1,000 characters, omit blank/whitespace-only content, and preserve internal line breaks.
3. `AC-03` — The form explains that notes use the selected Plan audience and that future active adult members can view notes on a whole-group Plan.
4. `AC-04` — Saved notes persist and are returned only through Plan reads authorized by the existing audience; existing Plans without notes remain readable.
5. `AC-05` — Note content renders as text and does not authorize location sharing, notifications, profile or Safety access, or a Sharing Session.
6. `AC-06` — Focused verification maps every applicable requirement and acceptance check to evidence without defining deferred behavior.

## Tasks

- [x] `TASK-001` — Implement the note field, API validation, persistence, and audience-limited display.
- [x] `TASK-002` — Add focused application, SQL persistence, API, and Angular coverage for the acceptance checks.
- [x] `TASK-003` — Verify the work item and record evidence in a separate verification report.

## Risks and blockers

- **Candidate context:** `CTX-1.17` was explicitly activated and its normative wording is in the canonical Plan workflow.
- **Authorization:** The owner explicitly approved and authorized implementation of this work item on 2026-09-26.
- **Privacy:** Free text may include sensitive information. The UI and server must enforce the Plan's existing audience; no independent note audience is introduced.
- **Deferred:** All exclusions above remain outside this work item.

## Execution authorization

- **Approved work item:** `YES` — explicit project-owner approval, 2026-09-26
- **Execution authorized:** `YES` — explicit project-owner authorization, 2026-09-26
- **Authorized scope:** Implement and verify the acceptance checks in this work item under active `CTX-1.17`.
- **Implementation authorization:** Does not include multi-group handling, later audience changes, selected-recipient membership-change behavior, minor/guardian functionality, Plan editing, or any other listed non-goal.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| Proposal decision traceability | `PRP-018` and `DEC-056` | `PASS` |
| Activated context | `VER-CTX-1.17.md`; canonical workflow and requirements index | `PASS` |
| .NET Foundation suite | Owner terminal reports 85 passed, 0 failed, 0 skipped with `RUN_FAMILYSAFETY_SQL_TESTS='1'` previously set in the terminal session | `PASS` |
| Angular component/unit tests | 13 tests passed, including notes order, request body, audience notice, and safe multiline rendering | `PASS` |
| SQL Plan persistence tests | Included in owner-reported successful SQL-gated suite run | `PASS` |
| Angular production build | Owner Visual Studio terminal screenshot shows `Application bundle generation complete` | `PASS` |

- **Verification report:** `../verification/VER-WI-007-002.md`
- **Independence:** `SELF-VERIFIED`; owner-terminal test and production-build results recorded.
- **Outcome:** `DONE`; all acceptance checks pass. Migration rollout in a deployed environment is outside this work item.
