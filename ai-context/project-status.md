# Project Status

**As of:** 2026-10-08. This file owns the current checkpoint; the [roadmap](planning/roadmap.md) owns phase status and remaining gates.

## Continuation checkpoint

| Field | Current value |
|---|---|
| Active baseline | CTX-1.39 APPROVED / ACTIVE under [DEC-095](decision-log.md#dec-095--bounded-creator-plan-activity-and-timing-correction), succeeding CTX-1.38 for bounded Plan fact correction. Separate context checks: [VER-WI-105-002](planning/verification/VER-WI-105-002.md). Prior baselines remain historical. |
| Current phase | Phase 10 — Testing & Verification / User-Testability Readiness — CURRENT / IN PROGRESS under DEC-093. |
| Functional MVP status | FUNCTIONALLY COMPLETE by owner acceptance of the Final Functional MVP Readiness Review, recorded under [DEC-096](decision-log.md#dec-096--close-phase-105-and-accept-functional-mvp-completion). Phase 10.5 is COMPLETE / CLOSED; no further implementation WI is required. This is not human validation or production/pilot readiness. |
| Previous phase | Phase 9.5 — MVP Feature Completion — COMPLETE / CLOSED under DEC-092; Phase 9 remains closed under DEC-081. |
| Completed items | WI-095-001–010, WI-010-001–003, and WI-105-001/002 are COMPLETE / PASS — OWNER APPROVED. Acceptance and technical verification remain in their work items and VER records; technical self-verification is not human validation. |
| Current authorized work item | [WI-010-004](planning/stories/WI-010-004-facilitated-adult-usability-sessions.md) remains APPROVED / open. Stage B preparation explicitly authorized by owner, 2026-10-06, now PREPARATION COMPLETE / READY FOR P1. Materials reconciled, B-owned Plan/Trip starting examples prepared, mechanical preflight/reset SELF-VERIFIED. No human session/result or product fix; no WI-010-005 authorized. |
| Next authorized action | Owner conducts actual P1 using the prepared guide and synthetic scenario after the in-tab location/startup check. Agent preparation stopped before P1; human execution/analysis remains pending. Phase 11 FUTURE. Owner performs all Git commits. |

## Open decisions

- OD-002 - resolved for bounded temporary-only MVP by DEC-087; indefinite sharing excluded.
- OD-003 — Minor/guardian functionality; outside MVP under DEC-053.
- OD-006 — Exact production invitation retention periods OPEN under [DEC-097](decision-log.md#dec-097--approve-future-phase-11-production-entry-model); bounded retention/minimal metadata required, no exact 30-day / 90-day policy approved.

Open decisions are not requirements. Resolved decisions and their canonical references are in the [full decision log](decision-log.md#open-product-decisions).

## Dependencies

[DEC-097](decision-log.md#dec-097--approve-future-phase-11-production-entry-model) and [DEC-098](decision-log.md#dec-098--approve-provider-neutral-identity-and-production-entry-architecture) record the owner-approved [future production-entry contract](core/production-entry-contract.md), v1.1, APPROVED / UNIMPLEMENTED for Phase 11. [WI-011-001 — Provider-Neutral Identity and Production-Entry Architecture](planning/stories/WI-011-001-provider-neutral-identity-and-production-entry-architecture.md) is APPROVED architecture planning only; governance canonicalization is complete with documentation self-verification recorded in the item. CTX-1.39 remains active; no successor CTX or implementation WI is created and no implementation or provider proof is authorized. The [Phase 11 game plan](planning/roadmap.md#phase-11--pilot--release-readiness) records ten planned implementation stages. Identity/email vendors, OD-006 exact invitation retention, detailed administrator continuity/recovery, final adult-attestation wording/legal threshold, invitation abuse/rate-limit parameters and acceptable provider revocation latency remain future bounded decisions. Provider/MFA integration and other production mechanics need later design/proof within the approved architecture. Current MVP, completed phase status and WI-010-004 human-testing checkpoint above are unchanged.

Production authentication, hosting, and backup-aging/restore evidence remain separate predeployment work. Development verification and phase closure do not certify deployment readiness. See DEC-068 and DEC-071 in the full decision log.

## Context for the next action

Read [AI operating rules](README.md), this checkpoint, and the requested item's required context. For the pending human session, use [WI-010-004](planning/stories/WI-010-004-facilitated-adult-usability-sessions.md), its [facilitator guide](planning/verification/evidence/WI-010-004/facilitator-guide.md), blank [observations](planning/verification/evidence/WI-010-004/observations.md), and [readiness matrix](planning/phase-10-readiness.md). Stage A/B preparation is complete; P1–P5 and human acceptance remain pending. The owner must confirm Sensors `0 0` and the exact synthetic scenario/startup before P1.

DEC-092 deferrals remain intact: general Event completion and explicit Plan completion await human testing; Trip reopen/undo and minor/guardian support remain outside MVP. No WI-095-011 or WI-010-005 is authorized, and no Phase 10.5 implementation remains. Historical work items and VER records are loaded only for a specific provenance or regression question.

## Current UI milestone — 2026-10-08

The owner-requested modernization is functionally complete in the working tree. The [UI Visual Design Contract](ui-visual-design-contract.md) and implemented Activities, Family, Home, Safety, and Me / Settings screens define current presentation. [REQ-UX-018](core/ux-contract.md#req-ux-018) supersedes REQ-UX-013: Home → Family → Activities → Safety → Me / Settings. Events, Plans, and Trips retain their separate domain and authorization contracts. [Family implementation notes](../docs/family-homepage-redesign.md) retain the authorized member-profile read/adoption safeguards and redesign verification limits.

Earlier rollout/alignment verification was SELF-VERIFIED: 420 Angular tests, selected connected browser journeys, 23 surfaces at 1440/768/390px, no detected overflow or axe violations in exercised states, and passing production builds with existing initial-bundle/Home/Trips/Concerns budget warnings. Profile field alignment and intermediate-width overflow were corrected; reusable tests remain. These results predate the later Activities/Family changes and do not certify the final working tree or human usability. Disposable comparison bundles were removed; no test or build was rerun for cleanup. [Household repair evidence](../artifacts/reviews/household-member-management-2026-10-08.md) retains its authorization checks and diagnostic provenance.

Failed-scenario data and ownership manifests remain untouched. October 8 diagnostic runs retained by the harness:

| Run | Synthetic database | Origin |
|---|---|---|
| `20261008T035610-02380312` | `FamilySafety_Invitation_E2E_1a7ab2bd48f847e7bb1ba6e267dc1add` | Initial rollout sharing-readiness failure; focused rerun passed. |
| `20261008T043641-491f189f` | `FamilySafety_Invitation_E2E_c7b7288f79b24df3adbbc3fe956900e0` | Alignment stress-check coordinate error; corrected check passed. |
| `20261008T051112-6092f001` | `FamilySafety_Invitation_E2E_aa55041dc8fa41c98326ee42aa901e62` | Household duplicate region-name failure; label correction and rerun passed. |

Ownership records are under `src/FamilySafety.Web/e2e/scenario-runs/<run>/`. This inventory records historical retention, not a fresh database-state check or deletion authorization. Ordinary Development and human-testing data remain protected.
