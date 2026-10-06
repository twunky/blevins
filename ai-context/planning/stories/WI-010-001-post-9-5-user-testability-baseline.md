# WI-010-001 — Post-9.5 User-Testability Baseline

- **Status:** COMPLETE / PASS — OWNER APPROVED (2026-10-05)
- **Phase:** 10 — Testing & Verification / User-Testability Readiness
- **Context baseline:** CTX-1.37 APPROVED / ACTIVE; DEC-093 changes phase/testing authorization only.
- **Requirements:** REQ-PROD-014; REQ-UX-006/008/011/014; REQ-PLAN-002; REQ-SHARE-010.
- **Model tier:** STANDARD; explicit owner authorization covers phase transition and bounded presentation fixes.

## Required context

| Source | Why needed |
|---|---|
| ../../README.md; ../../project-status.md; ../../../claude-handoff/CURRENT-TASK.md, WORKING-RULES.md, CONTEXT-ROUTING.md | Entry checkpoint and authorization |
| ../roadmap.md; ../../decision-log.md (DEC-092/093); ../phase-10-readiness.md | Phase scope, deferrals, testing boundary |
| ../../core/product-contract.md#req-prod-014; ../../core/requirements-index.md | Implemented minimum and requirements trace |
| ../../core/ux-contract.md (REQ-UX-006/008/011/014); ../../workflows/plans-trips.md#req-plan-002; ../../workflows/sharing-checkins.md#req-share-010 | Honest discovery, accessibility, existing optional expectation and standalone Check-In limits |
| ../../../src/FamilySafety.Web/README.md; ../../../claude-handoff/VALIDATION.md, MOCKUP-ALIGNMENT.md; ../../../docs/local-development.md | Structure, presentation and isolated test operations |
| App template/attention summary and affected Angular/browser tests; existing e2e spec inventory; VER-WI-095-006/010 (targeted evidence only) | Confirm cited issues, reuse coverage, location caveats and prior warning baseline |

Excluded: architecture review, bootstrap/archive, full historical evidence bundles, new feature/privacy design and production provisioning. Load another canonical source only if a demonstrated defect requires it.

## Goal and boundaries

Establish one compact post-9.5 facilitated-local-test baseline and improve actual discovery/expectation wording. Evaluate existing adult journeys, without new API calls, grants, behavior, notifications, reminders, lifecycle/completion features or broad redesign. DEC-092 remains in force. Synthetic adult data only; human sessions are future evidence, never inferred from automation.

## Acceptance checks

1. **AC-01:** Confirm the clean CTX-1.37 / closed Phase 9.5 checkpoint, authorize Phase 10 and WI-010-001 only, and synchronize current routing and requirements trace (REQ-PROD-014).
2. **AC-02:** Record critical journeys, useful readiness classifications, existing coverage, human tasks/observations and separate production dependencies in one concise matrix (REQ-UX-006/011/014).
3. **AC-03:** Correct only verified misleading Home attention, Home/Family discovery and expected-Check-In presentation. Preserve existing authorized destinations, no protected data in placeholders, no new feature requests, no automated-reminder implication (REQ-UX-008/011/014; REQ-PLAN-002; REQ-SHARE-010).
4. **AC-04:** Focused affected checks, one Angular regression, one relevant isolated integrated Playwright pass, production build and documentation/diff checks. Check changed discovery paths at 1440/390 with keyboard activation and accessibility scans; reuse historical privacy/expiry coverage. Backend regression not required absent backend changes.
5. **AC-05:** Report SELF-VERIFIED evidence and limitations; leave VERIFYING / OWNER REVIEW, no second item authorized, no staging/commit/push/PR.

## Tasks / execution authorization

- [x] Verify checkpoint; establish minimum phase authorization and this item.
- [x] Correct verified presentation/discovery issues in existing UI.
- [x] Establish matrix, focused integrated continuity and proportional verification.

Approved work item YES; execution authorized YES by the owner's explicit Phase 10 instruction dated 2026-10-05. Authorization includes creation and implementation in this task; no separate approval turn or standing auto-approval is needed. Stop for a material canonical conflict, new permission/behavior requirement or governance ambiguity.

## Evidence

Verification: [VER-WI-010-001](../verification/VER-WI-010-001.md), COMPLETE / PASS. Owner accepted the implementation, presentation, readiness baseline and recorded validation on 2026-10-05. Technical evidence remains SELF-VERIFIED; no human usability results are claimed. Next work item: NONE AUTHORIZED. Owner alone performs all Git commits.
