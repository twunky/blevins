# Project Status

**As of:** 2026-10-05. This file owns the current checkpoint; the [roadmap](planning/roadmap.md) owns phase status and remaining gates.

## Continuation checkpoint

| Field | Current value |
|---|---|
| Active baseline | CTX-1.37 APPROVED / ACTIVE under [DEC-092](decision-log.md#dec-092--phase-95-closure-scope-and-formal-deferrals); separate context verification recorded there. CTX-1.36 remains an approved historical baseline. |
| Current phase | Phase 10 — Testing & Verification / User-Testability Readiness — CURRENT / IN PROGRESS under DEC-093. |
| Previous phase | Phase 9.5 — MVP Feature Completion — COMPLETE / CLOSED under DEC-092; Phase 9 remains closed under DEC-081. |
| Completed items | [WI-010-001 — Post-9.5 User-Testability Baseline](planning/stories/WI-010-001-post-9-5-user-testability-baseline.md) COMPLETE / PASS — OWNER APPROVED; [verification](planning/verification/VER-WI-010-001.md) COMPLETE / PASS, accepted 2026-10-05. [WI-010-002 — Connected Everyday Coordination Verification](planning/stories/WI-010-002-connected-everyday-coordination-verification.md) COMPLETE / PASS — OWNER APPROVED; [verification](planning/verification/VER-WI-010-002.md) COMPLETE / PASS, accepted 2026-10-05. [WI-010-003 — Connected Safety and Care Verification](planning/stories/WI-010-003-connected-safety-care-verification.md) COMPLETE / PASS — OWNER APPROVED; [verification](planning/verification/VER-WI-010-003.md) COMPLETE / PASS, accepted 2026-10-05. WI-095-001 through WI-095-010 remain COMPLETE / PASS — OWNER APPROVED. |
| Current authorized work item | [WI-010-004 — Facilitated Adult Usability Sessions](planning/stories/WI-010-004-facilitated-adult-usability-sessions.md) IN_PROGRESS — Stage A PREPARED / AWAITING FACILITATED HUMAN EXECUTION under explicit owner request, 2026-10-05. This agent run ends after Stage A; no product changes or WI-010-005 authorized. |
| Next owner action | Conduct the first synthetic adult session using the [facilitator guide](planning/verification/evidence/WI-010-004/facilitator-guide.md), record anonymous actual observations in the [first-round record](planning/verification/evidence/WI-010-004/observations.md), and return evidence for Stage B. Owner performs all Git commits. |

## Open decisions

- OD-002 - resolved for bounded temporary-only MVP by DEC-087; indefinite sharing excluded.
- OD-003 — Minor/guardian functionality; outside MVP under DEC-053.

Open decisions are not requirements. Resolved decisions and their canonical references are in the [full decision log](decision-log.md#open-product-decisions).

## Dependencies

Production authentication, hosting, and backup-aging/restore evidence remain separate predeployment work. Development verification and phase closure do not certify deployment readiness. See DEC-068 and DEC-071 in the full decision log.

## Required context for the next action

Read [AI operating rules](README.md) and this checkpoint first, then WI-010-004 and its narrow required context. CTX-1.37 is active; Phase 10 remains CURRENT / IN PROGRESS with [one readiness matrix](planning/phase-10-readiness.md). WI-010-001, WI-010-002 and WI-010-003 are COMPLETE / PASS — OWNER APPROVED. Connected everyday-coordination and affected Safety/care journeys remain READY FOR FACILITATED TEST. WI-010-004 Stage A is prepared; Stage B awaits actual owner-supplied human observations. Human usability results have not yet been collected; WI-010-004 is not VERIFYING or COMPLETE. [REQ-PROD-014](core/product-contract.md#req-prod-014) and DEC-092 deferrals remain unchanged. No WI-095-011 or WI-010-005 is authorized.

Completed implementation and testing details remain in their work items and verification records, including [VER-WI-095-001](planning/verification/VER-WI-095-001.md). Read historical records only for a specific provenance or regression question, not as default session context.
