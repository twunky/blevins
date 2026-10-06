# WI-010-003 — Connected Safety and Care Verification

- **Status:** COMPLETE / PASS — OWNER APPROVED (2026-10-05)
- **Phase / baseline:** Phase 10 CURRENT / IN PROGRESS; CTX-1.37 APPROVED / ACTIVE.
- **Authority:** Explicit owner creation/execution authorization in the continuation request, confirmed 2026-10-05 after clean manual commit `11083d3`. Owner performs all commits; no staging, commit, push or PR. No WI-010-004 authorized.
- **Purpose / tier:** DEEP, verification and user-testability hardening of implemented adult Safety/care boundaries, not feature expansion.

## Required context

| Source | Reason |
|---|---|
| ai-context/README.md; project-status.md; planning/roadmap.md; planning/phase-10-readiness.md | Authority, checkpoint, single readiness matrix and phase gates |
| claude-handoff/CURRENT-TASK.md; WORKING-RULES.md; CONTEXT-ROUTING.md; VALIDATION.md; ai-context/skills/execute-story, verify-story and maintain-context/SKILL.md | Execution, self-verification, current-state maintenance and artifact safeguards |
| core/privacy-safety-contract.md: REQ-PRIV-019/024–027/030–033, REQ-SAFE-001, REQ-PRIV-003/006/009/010 | Explicit grants, nondisclosure, independent domains, membership loss and location limits |
| core/product-contract.md: REQ-FAM-014–017, REQ-PROD-014; core/ux-contract.md: REQ-UX-009/011/013/014 | Manual contacts, presets, deferrals, clear sharing and accessible navigation |
| workflows/sharing-checkins.md: REQ-SHARE-006/010/011; workflows/safety.md: REQ-SAFE-015/016 | Implemented status/location and manual Concern/Incident workflows |
| src/FamilySafety.Web/README.md; docs/local-development.md: isolated Playwright scenarios; current domain specs | Existing component ownership and isolated synthetic adult harness |
| planning/verification/VER-WI-095-005 through VER-WI-095-008; VER-WI-010-001 (only relevant inherited checks/build warnings) | Reuse feature privacy regressions and identify limits without repeating whole suites |

Excluded: bootstrap/archive, unrelated coordination workflows, broad historical reports, production providers and minor profiles.

## Verification boundary and fix authority

Use three coherent journeys: Safety Circle → reviewed standalone Check-In → recipient/nonrecipient and unchanged grants; private Trusted Contact → Family switching → manual Concern/Incident discovery and independent audiences; preset → bounded temporary sharing → permission/unavailability/freshness → recipient/nonrecipient → stop/expiry and Home return. Use existing isolated adult identities and real Development API/SQL; browser geolocation may be mocked and must be labeled.

Fix only demonstrated broken links, misleading copy, loading/error/retry, narrow accessibility/layout or Family-context presentation defects. Record each defect, apply a bounded fix and focused regression. Stop for a material product/privacy decision, reporting blocker, source and recommended resolution.

Non-goals: new workflows, audiences, authorization, escalation, notifications, contact integration, location history, maps, background guarantees, broad redesign/refactor, minors, authentication/deployment. DEC-092 deferrals remain in force; Phase 9.5 stays closed.

## Acceptance / exit checks

1. **AC-01:** Safety navigation, owner circle management and reviewed explicit Check-In recipients work; recipient can read, nonrecipient/admin cannot, and later preset changes leave target grants intact. Preserve self-authorship and inherited membership-loss rules.
2. **AC-02:** Owner creates/edits/removes private manual contacts; contacts remain personal across Families and independent of recipient choices, membership and authorization.
3. **AC-03:** Explicit fixed temporary audience and bounded duration work; permission denial/unavailability, latest point and freshness are clear. Recipient access ends at stop/expiry; nonrecipient/admin has no override. No history or background guarantee.
4. **AC-04:** Manual Concern/Incident discovery and supported creation/actions remain separate from status/location/contact/preset activity. Independent audiences and denied links disclose no protected content or selected-Family change.
5. **AC-05:** Proportionate keyboard activation/focus, axe and 390px checks cover connected paths and Family-scoped preset/draft isolation. Selected Family grants no protected access.
6. **AC-06:** No blocking demonstrated defect remains; affected rows are READY FOR FACILITATED TEST with human/location limits explicit. One final Angular regression, production build, relevant Playwright set and diff check; full SQL-gated .NET regression only if backend product code changes.

## Evidence / handoff

Technical verification PASS / SELF-VERIFIED; [VER-WI-010-003](../verification/VER-WI-010-003.md) maps acceptance and inherited feature coverage. Three connected journeys plus three reused privacy/location checks passed; a final strengthened 390px anchor journey also passed. One demonstrated broken Safety Check-In link was fixed with focused regression coverage. Final Angular 403/403 and production build passed with unchanged warning categories. Human usability remains uncollected; physical-device accuracy, browser background behavior, OS suspension and real GPS reliability remain production/pilot dependencies.

Owner reviewed and accepted WI-010-003 and its recorded validation on 2026-10-05. WI-010-003 is COMPLETE / PASS — OWNER APPROVED. Technical evidence remains SELF-VERIFIED; human usability remains uncollected. No WI-010-004 is authorized. CTX-1.37 and DEC-092 remain unchanged. Owner alone performs Git commits.
