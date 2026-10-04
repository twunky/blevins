---
Status: ACTIVATED — owner-approved amendment
Authority: DEC-083 (2026-10-04)
Context-Baseline: CTX-1.29
Decision: DEC-083
---

# CTX-1.30 — Event Invitations and Self RSVP

## Candidate scope

Successor to `CTX-1.29`. Owner-approved source: the 2026-10-04 APPROVED CONTEXT / GOVERNANCE PREPARATION instruction, recorded by [DEC-083](../../decision-log.md#dec-083--approve-the-first-phase-95-event-invitations-and-self-rsvp-scope).

- [REQ-EVT-014](../../workflows/events.md#req-evt-014) owns initial Pending semantics, no self-invite, invitation history/re-invitation, and in-app discovery. `REQ-EVT-010` is unchanged.
- [REQ-PRIV-028](../../core/privacy-safety-contract.md#req-priv-028) owns current eligibility, same-profile restoration, named visibility, and organizer access loss. `REQ-PRIV-023` is unchanged.
- The [requirements index](../../core/requirements-index.md) routes the two new IDs. Existing meanings receive no silent rewrite.
- [WI-095-001](../stories/WI-095-001-event-invitations-and-rsvp.md) records the single first approved scope, architecture/data intent, acceptance checks, and future verification plan. Approved work item YES; execution NO.

No sibling Phase 9.5 capability or product implementation is authorized. Phase 9 closure and its CTX-1.28 evidence remain intact; the post-Phase-9 CTX-1.29 amendment remains historical and compatible.

## Verification and activation gate

[VER-CTX-1.30](../verification/VER-CTX-1.30.md) must PASS before activation. Verify requirement/decision/link traceability, privacy and Event consistency, unchanged Phase 9 history, excluded sibling capabilities, work-item readiness, and current-state routing. Candidate approval is not application execution approval.

**Activation (2026-10-04):** Following VER-CTX-1.30 candidate PASS / SELF-VERIFIED, explicitly activate CTX-1.30 under the owner-approved governance instruction and DEC-083. README, status, roadmap and current handoff pointers are synchronized. CTX-1.29 is historical; Phase 9 closure remains unchanged. Phase 9.5 is CURRENT / IN PROGRESS only for WI-095-001 scope APPROVED, execution authorized NO. Product implementation and sibling capabilities remain separately gated.
