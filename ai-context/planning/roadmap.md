# Roadmap

Approved phase status and remaining gates. The [project checkpoint](../project-status.md) owns the active baseline and current execution authorization. Completed phases cover their approved slices, not every MVP capability.

| Phase | Name | Status |
|---|---|---|
| Phase 1 | Product & UX Foundation | `COMPLETE` |
| Phase 2 | AI Context Foundation | `COMPLETE` |
| Phase 3 | Technical Architecture | `COMPLETE` |
| Phase 4 | MVP Foundation | `COMPLETE` |
| Phase 5 | Family & Profiles | `COMPLETE` |
| Phase 6 | Events | `COMPLETE` |
| Phase 7 | Plans, Trips & Sharing | `COMPLETE / CLOSED` |
| Phase 7.5 | End-to-End UX & User-Test Readiness | `COMPLETE / CLOSED` |
| Phase 8 | Safety Coordination | `COMPLETE / CLOSED` |
| Phase 9 | Integration & UX Refinement | `COMPLETE / CLOSED` |
| Phase 9.5 | MVP Feature Completion | `COMPLETE / CLOSED` under DEC-092 |
| Phase 10 | Testing & Verification / User-Testability Readiness | `CURRENT / IN PROGRESS` under DEC-093 |
| Phase 10.5 | MVP Functional Completion | `COMPLETE / CLOSED` under DEC-096 |
| Phase 11 | Pilot & Release Readiness | `FUTURE` |

## Phase 9.5 — MVP Feature Completion

COMPLETE / CLOSED under DEC-092, 2026-10-05. WI-095-001 through WI-095-010 are COMPLETE / PASS — OWNER APPROVED and constitute the completed implementation set. CTX-1.37 is the historical closure baseline; CTX-1.39 is now active for bounded Plan fact correction, preserving Trip completion. No Phase 9.5 work remains active; no WI-095-011 is required. Phase 10 is separately authorized below.

| Work item | Delivered capability |
|---|---|
| WI-095-001 | Event Invitations and RSVP |
| WI-095-002 | Event Needs |
| WI-095-003 | Event Updates |
| WI-095-004 | Shared Family Dates |
| WI-095-005 | Standalone Check-Ins |
| WI-095-006 | Temporary Sharing Sessions |
| WI-095-007 | Trusted Contacts |
| WI-095-008 | Safety Circles |
| WI-095-009 | Home Pending RSVP Attention |
| WI-095-010 | Household Grouping |

The owner-approved minimum and full deferral boundary are canonical in [REQ-PROD-014](../core/product-contract.md#req-prod-014). WI-095-005 satisfies standalone Check-Ins, WI-095-006 satisfies bounded temporary location sharing, and WI-095-009 satisfies the first notification/attention slice through live eligible own-Pending RSVP Home attention. Broader/contextual Sharing Sessions, broader notifications and additional Check-In integrations/reminders are FORMALLY DEFERRED, not implemented or closure gates. OD-002 remains resolved only for the bounded temporary MVP under DEC-087; indefinite sharing remains excluded.

DEC-092 supersedes the remaining Phase 9.5 gates under DEC-073 and DEC-090's notification-completion boundary only. Broader product intent and existing privacy/implementation permissions are preserved. General Plan editing/lifecycle/completion and Trip completion remain outside Phase 9.5 closure. DEC-094 now places only the owner-selected functional completion candidates in Phase 10.5; it does not reopen Phase 9.5 or authorize general Plan lifecycle work. Any future capability or additional Event RSVP scope requires separate owner-approved scope, canonical updates where needed and execution authorization.

## Phase 10 — Testing & Verification / User-Testability Readiness

CURRENT / IN PROGRESS under DEC-093. WI-010-001 through WI-010-003 automated/connected verification and readiness results remain valid. Their READY FOR FACILITATED TEST rows mean technically prepared for human testing; no P1–P5 session or human result exists. Under owner acceptance recorded in DEC-096, the adult usability gate is now eligible to resume because the bounded adult MVP is FUNCTIONALLY COMPLETE. WI-010-004 Stage A remains prepared; Stage B PREPARATION COMPLETE / READY FOR P1 under the owner request, 2026-10-06. Guide/blank observations reconciled for B-owned Plan correction and Trip completion; synthetic starting examples and mechanical preflight/fresh-scenario reset SELF-VERIFIED. No human session performed. Assess feature discovery, task completion, terminology, navigation, privacy expectations, coaching and actual participant misunderstandings. This human validation remains required before pilot/release readiness and Phase 10 closure. No minor profiles or feature expansion are authorized by user testing. The single [readiness matrix](phase-10-readiness.md) records critical journeys, reused coverage, honest limitations and session preparation.

First item: [WI-010-001 — Post-9.5 User-Testability Baseline](stories/WI-010-001-post-9-5-user-testability-baseline.md) COMPLETE / PASS — OWNER APPROVED, including bounded corrections to verified misleading presentation. [WI-010-002 — Connected Everyday Coordination Verification](stories/WI-010-002-connected-everyday-coordination-verification.md) COMPLETE / PASS — OWNER APPROVED, accepted 2026-10-05. [WI-010-003 — Connected Safety and Care Verification](stories/WI-010-003-connected-safety-care-verification.md) COMPLETE / PASS — OWNER APPROVED, accepted 2026-10-05; [verification](verification/VER-WI-010-003.md) COMPLETE / PASS, technical evidence SELF-VERIFIED. Affected journeys remain READY FOR FACILITATED TEST; human results are uncollected. [WI-010-004 — Facilitated Adult Usability Sessions](stories/WI-010-004-facilitated-adult-usability-sessions.md): APPROVED — Stage A PREPARATION COMPLETE; Stage B PREPARATION COMPLETE / READY FOR P1. Materials and B-owned Plan/Trip setup reconciled for WI-105-001/002; focused mechanical preflight/reset SELF-VERIFIED, with no human evidence. Minimum useful round 3 adults, target 5; synthetic Development only. No WI-010-005 authorized. CTX-1.39 remains active for the bounded Plan correction slice.

Exit gates: critical adult journeys understandable and completable without unreasonable coaching; no blocking defect in tested critical paths or identified unintended privacy/access disclosure; deferred/unsupported behavior clearly communicated; remaining issues classified as blocking, bounded follow-up, deferred or production-readiness. Actual human observations and owner acceptance are still needed; automated evidence alone does not close Phase 10. DEC-092 deferrals remain active.

## Phase 10.5 — MVP Functional Completion

COMPLETE / CLOSED under [DEC-096](../decision-log.md#dec-096--close-phase-105-and-accept-functional-mvp-completion), 2026-10-06. Established under [DEC-094](../decision-log.md#dec-094--establish-phase-105-mvp-functional-completion), between Phase 10 and Phase 11 using the decimal-phase precedent (DEC-062/073); historical phases retain their numbers and meanings. Phase 10 remains CURRENT / IN PROGRESS for human verification/readiness. The owner accepted the Final Functional MVP Readiness Review and determined the bounded adult MVP FUNCTIONALLY COMPLETE. This does not claim human usability validation or production/pilot readiness.

Selected Phase 10.5 scope is complete through [WI-105-001 — Explicit Creator-Only Trip Completion](stories/WI-105-001-explicit-creator-only-trip-completion.md) COMPLETE / PASS — OWNER APPROVED; [verification](verification/VER-WI-105-001.md) COMPLETE / PASS, technical evidence SELF-VERIFIED, accepted 2026-10-06; [WI-105-002 — Bounded Creator Plan Activity and Timing Correction](stories/WI-105-002-bounded-creator-plan-activity-timing-correction.md) COMPLETE / PASS — OWNER APPROVED; [verification](verification/VER-WI-105-002.md) COMPLETE / PASS, technical evidence SELF-VERIFIED, accepted 2026-10-06; and the Final Functional MVP Readiness Review OWNER ACCEPTED under DEC-096. No further Phase 10.5 implementation work or implementation WI is required. General Plan completion remains deferred until after human testing; no general Plan lifecycle is authorized.

Numbering follows `WI-075` / `WI-095` for decimal phases and DEC-093 for whole phases: Phase 10.5 uses `WI-105-###`. This roadmap owns phase scope and exit gates; no separate phase record or initial WI is required to establish it.

Phase 10.5 exit gates are satisfied: the selected implementation items are owner-approved complete, no known blocking functional defect remains, and the Final Functional MVP Readiness Review is owner accepted. WI-010-004 Stage B preparation is complete under the owner request, 2026-10-06, and READY FOR P1; the human work item remains open. Human sessions have not started and no observations exist. Phase 10 remains open until human validation and owner acceptance; Phase 11 remains FUTURE.

DEC-092's broader/contextual Sharing Sessions, broader notifications and additional Check-In integrations/reminders remain deferred. General Event completion and explicit Plan completion remain deferred until after human testing; Trip reopen/undo and child/minor/guardian support remain out of MVP. Production authentication/recovery, invitation delivery, provisioning, hosting/deployment, configuration/secrets, backup/restore, observability/support operations, physical-device GPS and browser/mobile background/suspension behavior remain later production/pilot dependencies. Phase 9.5 stays closed; Phase 11 retains its separate pilot/release boundary. CTX-1.39 remains active under DEC-095; CTX-1.37 remains the historical Phase 9.5 closure baseline. DEC-096 closes Phase 10.5; Phase 10 remains CURRENT / IN PROGRESS until human validation and owner acceptance.

## Later phases

Phase 11 — Pilot & Release Readiness remains FUTURE. Completion of functional MVP scope is followed by the deferred WI-010-004 facilitated adult usability gate before the MVP is considered ready for pilot/release readiness. Real signup/sign-in/recovery, production invitation delivery, hosting, provisioning, backup-aging/restore evidence (DEC-068, DEC-071) and physical-device location validation remain separate production/pilot dependencies. They do not block continued MVP feature development; Development verification and phase closure do not establish deployment readiness.

## Phase 9 — Integration & UX Refinement

COMPLETE / CLOSED under DEC-081. All approved exits passed; closure applies to integration and refinement of implemented workflows, not Phase 9.5 capability completion. Historical scope/exit criteria: [PRP-030](proposals/PRP-030-phase-9-scope-themes-and-exit-criteria.md). Closure mapping: [VER-PH-009](verification/VER-PH-009.md).

## Phase 8 — Safety Coordination

COMPLETE / CLOSED under DEC-072. Approved Concern and Incident workflows are implemented and verified. Historical scope/exit criteria: [PRP-028](proposals/PRP-028-phase-8-concern-access-and-lifecycle.md), DEC-067/068, and [VER-PH-008](verification/VER-PH-008.md). Canonical behavior remains in the Safety workflow and product/privacy contracts.

## Phase 7.5 — End-to-End UX & User-Test Readiness

COMPLETE / CLOSED under DEC-065; PRP-027 CLOSED / COMPLETED. Historical purpose and boundaries: [PRP-021](proposals/PRP-021-phase-7-5-user-test-readiness.md). Closure and accepted human-validation limitations: [VER-PH-075](verification/VER-PH-075.md).

## Earlier phases

Phases 1–7 remain complete. Phase 5 (DEC-050), Phase 6 (DEC-052), and Phase 7 (DEC-063) closed their approved slices; deferred Family, Event, Trip, Sharing, and Check-In capabilities were not implicitly completed. Closure records remain under planning/verification/ and details under planning/stories/ and the [full decision log](../decision-log.md).

These historical pointers are for targeted review, not default loading. Do not preload completed work items, verification evidence, candidates, or old checkpoint narrative.
