# WI-010-004 — Facilitated Adult Usability Sessions

- **Status:** IN_PROGRESS — Stage A PREPARED / AWAITING FACILITATED HUMAN EXECUTION; Stage B NOT STARTED.
- **Phase / baseline:** Phase 10 CURRENT / IN PROGRESS; CTX-1.37 APPROVED / ACTIVE.
- **Purpose / tier:** DEEP for observing privacy understanding; gather actual adult discovery, understanding and task-completion evidence without unreasonable coaching.
- **Authority:** Explicit owner continuation request, 2026-10-05, following clean manual commit `b14aab6`. Creation and Stage A preparation authorized; owner conducts sessions and returns observations. This agent run must stop after Stage A. Stage B analysis awaits actual owner-supplied human evidence. No WI-010-005 authorized.

## Required context

| Source | Reason / timing |
|---|---|
| ai-context/README.md; project-status.md; planning/roadmap.md; planning/phase-10-readiness.md | Entry: authority, current phase and single readiness matrix |
| claude-handoff/CURRENT-TASK.md; WORKING-RULES.md; CONTEXT-ROUTING.md; VALIDATION.md; ai-context/skills/execute-story/SKILL.md; maintain-context/SKILL.md | Before preparation: scope, established lifecycle, lean checks and status maintenance |
| planning/stories/WI-010-002-connected-everyday-coordination-verification.md; WI-010-003-connected-safety-care-verification.md; planning/verification/VER-WI-010-002.md; VER-WI-010-003.md | Before preparation: required checkpoint and reusable scenario evidence/limits |
| core/product-contract.md: REQ-PROD-014, REQ-FAM-014–018; core/privacy-safety-contract.md: REQ-PRIV-019/024–027/030–034; core/ux-contract.md: REQ-UX-009/011/013/014 | Before preparation/analysis: deferrals, contacts/presets/organization and actual access model |
| workflows/plans-trips.md: supported Notes and deferred lifecycle; workflows/sharing-checkins.md: REQ-SHARE-006/010/011; workflows/safety.md: REQ-SAFE-015/016 | Before preparation/analysis: supported task boundaries |
| docs/local-development.md: invited-adult scenario; scripts/start-invitation-scenario.ps1; connected-event-coordination, connected-family-organization, connected-plans-trips and connected-safety-care specs | Before preparation: existing synthetic fixture and concrete setup conventions |
| planning/verification/evidence/WI-010-004/facilitator-guide.md; observations.md | Session execution / later analysis: one script/task set and one first-round record |

Excluded: bootstrap/archive, unrelated historical verification, production providers, minors, broad application regression and product changes. Existing specs are setup/evidence references, never human participants.

## Boundaries

Minimum useful first round: 3 adults; target: 5, identified only as P1–P5. Use isolated Development accounts and synthetic data; no participant identifiers, real relationships, contact details, locations, travel or Safety information in evidence. Sessions last approximately 30–45 minutes. Owner performs every Git commit; no staging, commit, push or PR.

DEC-092 remains intact: no broader/contextual Sharing Sessions, notifications, additional Check-In integrations/reminders or Plan/Trip/Event completion. Production authentication, signup/recovery, invitation delivery, hosting, provisioning, backup/restore, physical GPS and background tracking remain outside this round. No fixes or feature expansion are authorized by an observed expectation alone.

## Two stages and acceptance checks

### Stage A — PREPARE (this run only)

- [x] **AC-A1:** Checkpoint confirmed: clean tree, CTX-1.37 active, WI-010-001–003 owner-approved, Phase 9.5 closed, no existing WI-010-004 or later authorization.
- [x] **AC-A2:** One neutral facilitator script, seven bounded tasks, 30–45 minute plan and anonymous adult model prepared in the [guide](../verification/evidence/WI-010-004/facilitator-guide.md).
- [x] **AC-A3:** One blank [first-round record](../verification/evidence/WI-010-004/observations.md) captures outcome, coaching, issues/comments and privacy expectations without invented observations or success estimates.
- [x] **AC-A4:** Existing Development scenario inspected and reused; facilitator preflight/reset checklist covers Event invitation, organization, existing Plan/Trip Notes, private preset/contact and synthetic location. No application/test/setup code change required. Runtime setup is a facilitator pre-session action, not claimed executed here.
- [x] **AC-A5:** Practical readiness signals and deferred/production distinctions defined; current routing synchronized. Stage A documentation checks recorded below; human usability remains uncollected.

### Stage B — EXECUTE / ANALYZE (owner sessions pending)

- [ ] **AC-B1:** Owner conducts at least 3 adult sessions (target 5), returns actual anonymous task observations, records every recovery hint separately and distinguishes unattempted tasks from failed tasks.
- [ ] **AC-B2:** Analyze only returned human evidence; trace findings to participant/task and classify blocking, bounded follow-up, deferred or production dependency. Record privacy misunderstandings even where implementation is correct. No statistical thresholds from this small round.
- [ ] **AC-B3:** Update the existing readiness matrix from actual findings; owner decides acceptance and separately authorizes any fixes/later work. Human evidence and owner review are required before overall verification/completion or Phase 10 closure.

## Stage A evidence / handoff

Preparation is SELF-VERIFIED; overall WI outcome remains PENDING. No human task outcomes, participant observations, completion rates or usability conclusions exist. Normal status remains IN_PROGRESS; the owner explicitly forbids VERIFYING / OWNER REVIEW until human evidence is collected, overriding the execute-story skill's ordinary end-of-execution transition for this staged item.

Stage A validation (2026-10-05): SELF-VERIFIED documentation checks passed across 7 changed/new Markdown files: 59 local links, including 6 anchors, resolved; no trailing whitespace, replacement characters or unbalanced code fences. Current-state checks confirmed active CTX-1.37, awaiting human execution, no stale current authorization, no WI-010-005 file and empty staging. The existing readiness matrix table is identical to HEAD. `git diff --check` passed; new-file whitespace was checked separately. Scenario suitability was confirmed by inspecting the existing script, local instructions and connected specs; no database/browser preflight was executed or claimed. No application tests/builds or browser participant simulation were performed. Stage A stops here; await owner-facilitated human execution and returned observations.
