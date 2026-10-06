# WI-010-002 — Connected Everyday Coordination Verification

- **Status:** COMPLETE / PASS — OWNER APPROVED
- **Phase / baseline:** Phase 10 CURRENT / IN PROGRESS; CTX-1.37 APPROVED / ACTIVE.
- **Purpose:** Prepare implemented adult everyday coordination for facilitated local Development testing by verifying integration boundaries, not rebuilding Phase 9.5 feature verification.
- **Authority:** Explicit owner creation/execution authorization, 2026-10-05 continuation request. Owner performs every Git commit; no staging, commit, push or PR. No WI-010-003 authorized.
- **Model tier:** DEEP for privacy-boundary verification; no new privacy decision authorized.

## Required context

| Source | Reason |
|---|---|
| ai-context/README.md; project-status.md; planning/roadmap.md; planning/phase-10-readiness.md | Rules, checkpoint, phase gates and single readiness matrix |
| claude-handoff/CURRENT-TASK.md; WORKING-RULES.md; CONTEXT-ROUTING.md; VALIDATION.md | Current authorization, routing and lean test/artifact safeguards |
| core/product-contract.md: REQ-FAM-005/007/008/010–013/018, REQ-PROD-013/014 | Adult invitation/profile, Dates/Households and deferrals |
| core/privacy-safety-contract.md: REQ-PRIV-011/012/014–018/020–023/027–029/034 | Server authorization, resource-first deep links, Date/Household and Event disclosure boundaries |
| core/ux-contract.md: REQ-UX-008/011/013/014; workflows/events.md; workflows/plans-trips.md | Home discovery, navigation/accessibility, RSVP/Needs/Updates and supported Plan/Trip behavior |
| src/FamilySafety.Web/README.md; docs/local-development.md (isolated Playwright scenarios) | Existing frontend and synthetic adult harness conventions |
| planning/verification/VER-WI-010-001.md (build results only, if warnings occur) | Targeted comparison with the owner-approved build-warning baseline |

Excluded: bootstrap/archive, unrelated Safety/Sharing workflows, production providers, broad historical reports. Existing feature specs are reused as regression evidence, not scope authority.

## Journey boundary and allowed fixes

Three coherent browser journeys: invitation → optional profile review → Home → Event discovery/RSVP/Needs/Updates → Home; Family → shared Dates → Settings/Households → Family selection and authorized/denied resource transitions; Home/navigation → Plan/Trip creation/detail/Notes → correct domain/Home returns.

Only demonstrated broken navigation, stale/misleading wording, empty/loading/error/retry presentation, selected-Family presentation, straightforward accessibility or blocking small layout issues may be corrected, with focused regressions. Material product/privacy decisions stop execution.

Non-goals: new behavior, authorization, domain states, notification triggers, sharing modes, broad refactoring/redesign or harness overhaul; Plan/Trip/Event completion, contextual Sharing/Check-Ins, minors, production authentication/delivery/hosting. DEC-092 remains in force.

## Acceptance / exit checks

1. **AC-01:** Representative synthetic adult invitation/profile/Home and Event coordination journey completes; RSVP resolution is visible on Home return/reload.
2. **AC-02:** Dates and Households fit Family navigation; admin/member capabilities, unassigned members and Family switch isolation remain coherent.
3. **AC-03:** Authorized resource deep links reconcile Family only after authorization; denied resource IDs disclose no item/Family data or change selected Family. Selection/Household grouping grants no permission.
4. **AC-04:** Plans/Trips are discoverable and current create/view/Notes actions work; deferred lifecycle behavior is not misleadingly offered.
5. **AC-05:** Connected paths include keyboard activation/focus, axe scans and 390px overflow checks, plus proportionate loading/empty/error recovery evidence.
6. **AC-06:** No blocking defect remains in tested paths; findings/limitations are classified in the existing readiness matrix. Frontend: one full Angular suite, production build, relevant Playwright set, git diff --check. Backend regression only if backend code changes.

## Evidence / handoff

Connected technical checks PASS / SELF-VERIFIED; [VER-WI-010-002](../verification/VER-WI-010-002.md) maps acceptance checks, inherited validation, findings and continuation checks. Three connected journeys plus five reused privacy/keyboard checks passed; 403 Angular tests and production build passed. No application defect or code fix was demonstrated. Existing second-Family roles and inline Trip creation required test/session-prompt corrections only.

Owner accepted implementation and recorded validation, 2026-10-05. Technical evidence remains SELF-VERIFIED; human usability evidence remains uncollected. Phase 10 remains CURRENT / IN PROGRESS. Next work item: NONE AUTHORIZED; no WI-010-003 authorized.
