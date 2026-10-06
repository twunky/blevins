# WI-095-009 - Home Pending RSVP Attention

- **Status:** COMPLETE / PASS — OWNER APPROVED (2026-10-05)
- **Phase:** 9.5 CURRENT / IN PROGRESS
- **Context baseline:** CTX-1.35 remains ACTIVE; DEC-090 records bounded implementation scope, with no new permission or lifecycle rule.
- **Requirements:** REQ-EVT-013/010/014; REQ-UX-008/011/014; REQ-PRIV-011/020/021/023/027/028; REQ-NOTIFY-001.
- **Model tier:** DEEP (privacy); explicit owner authorization, no standing auto-approval.

## Required context

| Source | Purpose |
|---|---|
| ../../README.md; ../../project-status.md; ../../../claude-handoff/CURRENT-TASK.md, WORKING-RULES.md, CONTEXT-ROUTING.md | Checkpoint and authority |
| ../../workflows/events.md#req-evt-013, #req-evt-010, #req-evt-014 | Attention, existing RSVP and past-date rules |
| ../../core/ux-contract.md#req-ux-008, #req-ux-011, #req-ux-014 | Home hierarchy, Family reset and authorize-first links |
| ../../core/privacy-safety-contract.md#req-priv-028, #req-priv-011, #req-priv-020, #req-priv-021, #req-priv-023, #req-priv-027, #req-notify-001 | Exact-profile live eligibility, privacy and restrained attention |
| ../../core/requirements-index.md; ../../decision-log.md#dec-090; ../roadmap.md | Traceability and bounded scope |
| WI-095-001-event-invitations-and-rsvp.md (invitation/discovery criteria only) | Preserve existing authoritative query and lifecycle |
| ../../../src/FamilySafety.Web/README.md; ../../../claude-handoff/MOCKUP-ALIGNMENT.md, ../../../claude-handoff/reference/panel1-main-home.png, ../../../claude-handoff/VALIDATION.md; ../../../docs/local-development.md | Existing structure, presentation and guarded tests |
| Relevant invitation query/service/API, App/Home and invitation tests | Reuse existing authorization; focused regression |

Excluded: bootstrap/archive, unrelated workflows, historical evidence bundles and broader Phase 9.5 audit.

## Goal / scope

Reuse the live own-Pending invitation query in the existing Home attention card. Show Event title, date/time, RSVP needed and existing Event-detail link for the selected Family. Event detail retains RSVP mutations. Preserve relationship attention and honest loading/empty/error/Retry states.

## Acceptance checks

1. AC-01: Only the exact signed-in eligible invitee's unrevoked Pending invitations with current Event visibility appear, scoped to selected Family after authorization; past-dated invitations remain eligible. (REQ-EVT-010/013/014; REQ-PRIV-011/020/021/027/028)
2. AC-02: Live reread removes Going/Maybe/Not Going, revoked, membership-ineligible and visibility-lost entries. Other profiles/viewers/organizers/admins and Family selection get no override; minimum projection contains no other RSVP or invitation-management information. (REQ-PRIV-023/028)
3. AC-03: Existing compact Home attention displays loading, populated, no-attention and retryable error honestly, coexists with relationship suggestions, and links to existing Event detail without RSVP mutation controls. Family/identity changes clear stale state and refreshes remove resolved items. (REQ-UX-008/011/014; REQ-EVT-013; REQ-NOTIFY-001)
4. AC-04: Focused backend/API/SQL and Angular checks plus one isolated Pending → Home → Event RSVP → Home journey; proportional desktop/narrow accessibility/layout review. One final SQL-gated .NET, full Angular, production build, relevant Playwright and diff check.
5. AC-05: Minimal context scope decision; CTX-1.35 stays active, Phase 9.5 stays in progress, closure requires explicit owner approval, WI-095-010 unauthorized; no Git staging/commit/push/PR.

## Non-goals / stop conditions

No notification persistence/inbox/history, read/unread, dismissal/snooze, outbound push/email/SMS, preferences or generic infrastructure. No Needs/Check-In/Sharing attention, Event completion/expiry, Household grouping, contextual Sharing or unrelated features. Stop only for a new material canonical conflict, unsafe authorization or required excluded scope; the owner's trigger/live-state/past-date/Family decisions are settled.

## Tasks / authorization

- [x] Reuse invitation API and implement compact Home attention.
- [x] Focused tests and one final regression; concise self-verification and owner-review handoff.

Approved work item YES; execution authorized YES by the owner's explicit pasted continuation request on 2026-10-05, including this first Home attention trigger. Owner performs all Git commits. No next work item authorized.

## Evidence

Verification: [VER-WI-095-009](../verification/VER-WI-095-009.md) COMPLETE / PASS. Owner accepted the implementation, presentation, recorded validation and existing build-budget warning categories on 2026-10-05. SELF-VERIFIED limitation remains. Next work item: NONE AUTHORIZED.
