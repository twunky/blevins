# WI-095-006 — Temporary Sharing Sessions

- **Status:** COMPLETE / PASS — OWNER APPROVED (2026-10-04); **Phase:** 9.5 CURRENT / IN PROGRESS.
- **Baseline:** CTX-1.33 / DEC-087; **Tier:** DEEP.
- **Requirements:** REQ-SHARE-001–005/011; REQ-PRIV-001–006/009–012/031; REQ-UX-009/010/011/013. REQ-PRIV-030 supplies the consistent stale-grant precedent only.
- **Authorization:** Explicit project-owner prompt, 2026-10-04, approves this work item, narrow canonical amendments/successor baseline and implementation in this task. No standing auto-approval, Git action, PR, sibling item or phase transition.

## Required context

| Source | Purpose |
|---|---|
| ai-context/README.md; project-status.md; claude-handoff/CURRENT-TASK.md; WORKING-RULES.md; CONTEXT-ROUTING.md | Entry, authority and checkpoint |
| workflows/sharing-checkins.md REQ-SHARE-001–005/011; core/privacy-safety-contract.md REQ-PRIV-001–012/030/031; core/product-contract.md REQ-FAM-001–008 | Consent, eligibility, audience and lifecycle |
| core/ux-contract.md REQ-UX-009/010/011/013; decision-log.md DEC-087 / OD-002; core/requirements-index.md; planning/roadmap.md | Clarity, navigation and bounded decision traceability |
| src/FamilySafety.Web/README.md; claude-handoff/MOCKUP-ALIGNMENT.md; VALIDATION.md; docs/local-development.md | Existing visual language, structure and guarded SQL/browser validation |
| ai-context/skills/execute-story/SKILL.md; maintain-context/SKILL.md; verify-story/SKILL.md | Execution, context maintenance and self-verification |

Old verification packages, bootstrap/archive, unrelated completed manifests and workflows are excluded.

Validation exception: only VER-WI-095-005's production-build warning line was consulted to compare the prior initial bundle and unchanged stylesheet budgets.

## Scope and approved decisions

Self-initiated adult sharing, anchored to one Family, with explicitly selected active adult recipients fixed at creation. Default one hour, chosen expiration up to 24 hours, no extension; author may stop early. Exact original profile/membership grants permanently lose access on recipient membership loss. Author membership loss ends the whole session. Browser permission controls actual availability; modest updates replace one current point. Server access expires immediately, independently of physical cleanup. Minimal metadata only after end, no archive UI. Safety workflow and Home indicator use existing presentation; no new mapping dependency.

## Non-goals

Indefinite/proxy sharing, native/background guarantees, location history/playback, mid-session audience editing, Event/Plan/Trip/Trusted Contact/Safety Circle sharing, notifications/alerts/geofences/SOS, safety inference/escalation, generic tracking/retention/concurrency frameworks, restore, proprietary map rollout or other Phase 9.5 work.

## Acceptance checks

1. **AC-01:** Active adult author creates only their own bounded session with explicit same-Family eligible recipients; invalid duration/recipient rejected.
2. **AC-02:** Author or exact valid recipient alone reads active coordinates; known ID, selected Family, administrators and replacement profiles grant nothing; denied/missing reads are nondisclosing. Recipients do not receive audience identities.
3. **AC-03:** Recipient membership loss invalidates forever, including rejoin; author loss ends all access. Family-first protected writes preserve concurrency.
4. **AC-04:** Stop/expiration immediately deny coordinates and subsequent updates; feature cleanup removes raw points. Stale versions cannot revive sessions.
5. **AC-05:** Only author submits valid, fresh timestamped coordinates; latest point replaces prior point; server/client cadence is bounded to one minute.
6. **AC-06:** Deterministic browser/component tests cover creation, permission, failures, updates, recipient latest point and stop/access loss. UI shows audience, end time, stale freshness and never-available state; Home indicates active sharing. Desktop/narrow controls follow existing visual language.
7. **AC-07:** Focused checks then one final full SQL-gated .NET suite, Angular suite, production build, relevant Playwright journey and diff check. Record migration and limitations, label SELF-VERIFIED, hand back VERIFYING / OWNER REVIEW.

## Stop conditions

Stop only for a new material unresolved canonical/product/privacy conflict, mandatory location-history/irreconcilable retention rule, or substantial new browser/location platform decision. Report exact source, blocker and recommended resolution.

## Evidence

Implemented backend/schema/API, Safety workflow and Home active-sharing indicator. [VER-WI-095-006](../verification/VER-WI-095-006.md) records AC-01–07 PASS / SELF-VERIFIED, focused checks, final SQL-gated .NET 341/341, Angular 373/373, production build, one integrated Playwright journey and diff/context checks. Build warning categories persist; initial bundle grew to 687.53 kB, thresholds unchanged. Owner accepted the implementation and recorded validation on 2026-10-04. Targeted bundle check found no accidental dependency/import; closure changed documentation only. Owner performs all Git commits; no next item authorized.
