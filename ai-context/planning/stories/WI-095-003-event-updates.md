# WI-095-003 — Event Updates

- **Status:** COMPLETE / PASS — OWNER APPROVED (2026-10-04); Phase 9.5 CURRENT / IN PROGRESS.
- **Baseline/tier:** CTX-1.31 / DEEP.
- **Requirements:** REQ-EVT-002/006/009/012; REQ-PRIV-021/023/027.
- **Authorization:** Work item and execution explicitly authorized by the owner's Event Updates request, 2026-10-04. Owner performs all Git commits; no staging, commits, push or PR.

## Required context

| Source | Purpose |
|---|---|
| ai-context/README.md; project-status.md; claude-handoff/CURRENT-TASK.md, WORKING-RULES.md, CONTEXT-ROUTING.md | Authority and checkpoint |
| workflows/events.md, especially REQ-EVT-012; core/privacy-safety-contract.md, REQ-PRIV-021/023/027 | Organizer authorship, current audience, retained revisions and nondisclosing denial |
| src/FamilySafety.Web/README.md; claude-handoff/MOCKUP-ALIGNMENT.md, VALIDATION.md; docs/local-development.md | Existing ownership, presentation and guarded validation |
| Current Event access, Needs repository/API/component and focused tests | Reuse access and Family-first locking conventions; preserve App routing |

Excluded: unrelated workflows, bootstrap/archive, historical proposals and broad verification evidence.

## Scope and non-goals

Implement Event-local plain-text Updates: authorized organizer create/edit/hide; current visible list ordered by creation time with author and created/edited times. Retain hidden records and prior text with Event-local edit provenance. Normal APIs exclude hidden Updates and all revisions for every actor; no history/restore endpoint or UI is required. Use the existing Event short-detail convention of 1,000 characters. Add narrow version protection and an Event-specific component under `src/app/events/`.

No comments, reactions, chat, attachments, rich text, generic feed/audit framework, notifications, Home work, Check-Ins, Sharing, location, Event completion/audience changes, Plans/Trips or broad Event/App refactor. No new CTX or next-item authorization.

## Acceptance checks

1. **AC-01:** Organizer with current Event access can create/edit/hide their own Update; editors, administrators, invitees, participants and outsiders gain no write authority.
2. **AC-02:** Current Event viewers receive ordered current visible text, author and creation/updated times only. Hidden content and prior text never enter normal API projections; access loss and known IDs supply no alternate path.
3. **AC-03:** Edits retain prior text and minimum edit provenance; hide retains the record, author, timestamps and revisions. Stale edit/hide cannot overwrite newer state; Family-first locking is preserved.
4. **AC-04:** Event detail supports loading, empty/list, Retry, organizer post/edit/hide confirmation, read-only viewers and conflict/error refresh. Event/profile/Family changes clear and cancel stale component state; authorize-first routing remains intact.
5. **AC-05:** Focused backend/SQL, Angular and integrated journey checks pass; one final .NET SQL-gated regression, Angular suite, production build, relevant Playwright run and diff check are reported with limitations.

## Stop conditions

Stop for a material unresolved product/privacy decision, canonical conflict, unexplained validation failure, stale baseline or required work outside this scope. Owner acceptance and closure recorded 2026-10-04.

## Verification

2026-10-04 — implementation and proportional validation finished; PASS / SELF-VERIFIED in [VER-WI-095-003](../verification/VER-WI-095-003.md). Owner accepted implementation and validation and approved closure on 2026-10-04. Verification is COMPLETE / PASS. No next item authorized; owner performs all Git commits.
