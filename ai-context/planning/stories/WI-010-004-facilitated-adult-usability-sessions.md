# WI-010-004 — Facilitated Adult Usability Sessions

- **Status:** APPROVED — Stage A PREPARATION COMPLETE; Stage B PREPARATION COMPLETE / READY FOR P1; human execution/analysis remains pending. Work item remains open.
- **Phase / baseline:** Phase 10 CURRENT / IN PROGRESS; Stage A was prepared under CTX-1.37. Current baseline CTX-1.39 remains ACTIVE.
- **Purpose / tier:** DEEP for observing privacy understanding; gather actual adult discovery, understanding and task-completion evidence without unreasonable coaching.
- **Authority:** Stage A creation/preparation authorized by the explicit owner continuation request, 2026-10-05, following clean manual commit `b14aab6`. Stage B was deferred pending functional completion; owner accepted the Final Functional MVP Readiness Review and closed Phase 10.5 under DEC-096. Owner explicitly authorized Stage B preparation/resumption on 2026-10-06, including mechanical setup/preflight and a stop before P1. This preparation is complete; human execution remains pending. No participant sessions or results exist. No WI-010-005 authorized.

## Required context

| Source | Reason / timing |
|---|---|
| ai-context/README.md; project-status.md; planning/roadmap.md; planning/phase-10-readiness.md | Entry: authority, current phase and single readiness matrix |
| claude-handoff/CURRENT-TASK.md; WORKING-RULES.md; CONTEXT-ROUTING.md; VALIDATION.md; ai-context/skills/execute-story/SKILL.md; maintain-context/SKILL.md | Before preparation: scope, established lifecycle, lean checks and status maintenance |
| planning/stories/WI-010-002-connected-everyday-coordination-verification.md; WI-010-003-connected-safety-care-verification.md; planning/verification/VER-WI-010-002.md; VER-WI-010-003.md | Before preparation: required checkpoint and reusable scenario evidence/limits |
| core/product-contract.md: REQ-PROD-014, REQ-FAM-014–018; core/privacy-safety-contract.md: REQ-PRIV-019/024–027/030–034; core/ux-contract.md: REQ-UX-009/011/013/014 | Before preparation/analysis: deferrals, contacts/presets/organization and actual access model |
| workflows/plans-trips.md: REQ-PLAN-008 correction, REQ-TRIP-011/012 completion and separate Notes; WI-105-001/002 accepted behavior; workflows/sharing-checkins.md: REQ-SHARE-006/010/011; workflows/safety.md: REQ-SAFE-015/016 | Before preparation/analysis: supported task boundaries |
| docs/local-development.md: invited-adult scenario; scripts/start-invitation-scenario.ps1; connected-event-coordination, connected-family-organization, connected-plans-trips and connected-safety-care specs | Before preparation: existing synthetic fixture and concrete setup conventions |
| planning/verification/evidence/WI-010-004/facilitator-guide.md; observations.md | Session execution / later analysis: one script/task set and one first-round record |

Excluded: bootstrap/archive, unrelated historical verification, production providers, minors, broad application regression and product changes. Existing specs are setup/evidence references, never human participants.

## Boundaries

Minimum useful first round: 3 adults; target: 5, identified only as P1–P5. Use isolated Development accounts and synthetic data; no participant identifiers, real relationships, contact details, locations, travel or Safety information in evidence. Sessions last approximately 30–45 minutes. Owner performs every Git commit; no staging, commit, push or PR.

DEC-092 remains intact: no broader/contextual Sharing Sessions, broader notifications or additional Check-In integrations/reminders. General Event completion and explicit Plan completion remain deferred until after human testing; Trip reopen/undo and child/minor/guardian support remain out of MVP. Production authentication, signup/recovery, invitation delivery, hosting, provisioning, backup/restore, physical GPS and background tracking remain outside this round. No fixes or feature expansion are authorized by an observed expectation alone.

## Two stages and acceptance checks

### Stage A — PREPARE (historical preparation)

- [x] **AC-A1:** Checkpoint confirmed: clean tree, CTX-1.37 active, WI-010-001–003 owner-approved, Phase 9.5 closed, no existing WI-010-004 or later authorization.
- [x] **AC-A2:** One neutral facilitator script, seven bounded tasks, 30–45 minute plan and anonymous adult model prepared in the [guide](../verification/evidence/WI-010-004/facilitator-guide.md).
- [x] **AC-A3:** One blank [first-round record](../verification/evidence/WI-010-004/observations.md) captures outcome, coaching, issues/comments and privacy expectations without invented observations or success estimates.
- [x] **AC-A4:** Existing Development scenario inspected and reused; facilitator preflight/reset checklist covers Event invitation, organization, existing Plan/Trip Notes, private preset/contact and synthetic location. No application/test/setup code change required. Runtime setup is a facilitator pre-session action, not claimed executed here.
- [x] **AC-A5:** Practical readiness signals and deferred/production distinctions defined; current routing synchronized. Stage A documentation checks recorded below; human usability remains uncollected.

### Stage B — PREPARED / READY FOR P1; EXECUTE / ANALYZE pending

**Current status:** PREPARATION COMPLETE / READY FOR P1 under the explicit owner preparation request, 2026-10-06. CTX-1.39 active; Phase 10.5 closed under DEC-096. Seven-task guide reconciled for participant-owned Plan fact correction and Trip completion; blank observation cues added. Synthetic B-owned starting examples and mechanical preflight/reset are recorded in the guide. Stop before any actual human session; no outcome, finding or usability pass exists. Retain APPROVED as the valid lifecycle state, not VERIFYING/COMPLETE/PASS/OWNER APPROVED.

- [ ] **AC-B1:** Owner conducts at least 3 adult sessions (target 5), returns actual anonymous task observations, records every recovery hint separately and distinguishes unattempted tasks from failed tasks.
- [ ] **AC-B2:** Analyze only returned human evidence; trace findings to participant/task and classify blocking, bounded follow-up, deferred or production dependency. Record privacy misunderstandings even where implementation is correct. No statistical thresholds from this small round.
- [ ] **AC-B3:** Update the existing readiness matrix from actual findings; owner decides acceptance and separately authorizes any fixes/later work. Human evidence and owner review are required before overall verification/completion or Phase 10 closure.

## Preparation evidence / handoff

Stage A preparation remains historical. Stage B preparation is SELF-VERIFIED; no human task outcomes, participant observations, completion rates or usability conclusions exist. The guide covers correction discovery, facts versus Notes, descriptive Check-In timing and deliberate one-way Trip completion, Home exclusion and continued list/detail editing/Notes. B is the original creator of both synthetic examples. The reset uses a fresh existing Development scenario, never Trip reopen or database mutation. Both rehearsal and ready starting databases are retained; exact names and restart instructions are in the guide. No P1–P5 session has begun.
Stage A validation (2026-10-05): SELF-VERIFIED documentation checks passed across 7 changed/new Markdown files: 59 local links, including 6 anchors, resolved; no trailing whitespace, replacement characters or unbalanced code fences. The Stage A checkpoint checks confirmed active CTX-1.37, then-awaiting human execution, no stale current authorization, no WI-010-005 file and empty staging. The existing readiness matrix table was identical to HEAD. `git diff --check` passed; new-file whitespace was checked separately. Scenario suitability was confirmed by inspecting the existing script, local instructions and connected specs; no database/browser preflight was executed or claimed. No application tests/builds or browser participant simulation were performed. Stage A materials are preserved for the MVP-end validation gate.

Stage B preparation validation (2026-10-06): SELF-VERIFIED focused mechanical setup checks and documentation/reference, stale-current-wording, WI/readiness consistency and `git diff --check`. No application regression suite or production build; no product changes. Human AC-B1–B3 remain unchecked.
