# Current Task — WI-095-001 Scope Approved; Execution Not Authorized

## Current state and authorization

`CTX-1.30` is ACTIVE under DEC-083 after [VER-CTX-1.30](../ai-context/planning/verification/VER-CTX-1.30.md) PASS. Phase 9.5 is CURRENT / IN PROGRESS for only [WI-095-001 — Event Invitations and RSVP](../ai-context/planning/stories/WI-095-001-event-invitations-and-rsvp.md), APPROVED scope with **execution authorized NO**. Context/governance preparation is complete; do not edit application code, migrations, endpoints, UI or tests until separate explicit owner execution authorization. Later Phase 9.5 items require separate gates; OD-002 OPEN. Phase 9 remains COMPLETE / CLOSED under DEC-081 and VER-PH-009, unchanged.

## First approved scope

In-app organizer invitations and invitee self-RSVP; Events dashboard own active Pending discovery and existing Event detail actions. No Home/outbound notifications, Needs or Updates. Use the WI manifest, acceptance checks, approved single-record data intent, small new participation boundary, exclusions and stop conditions. No existing App detail/edit extraction. Implementation and future WI verification remain PENDING. Next action is owner review and separate execution authorization, not automatic implementation.

## Completed post-Phase-9 follow-ups

Each was separately owner-authorized and is committed by the owner in `9140349` (2026-10-04). None changes the Phase 9 closure or starts Phase 9.5.

- Plan note edit/clear by the Plan creator (`DEC-082` / `REQ-PLAN-007`; `CTX-1.29`).
- The Plan create `Location` header now points to `/api/plans/{planId}`.
- The one-person Event explanation on Event creation; Event audience eligibility is unchanged.
- Trip Notes loading, empty, failure, and Retry states for the existing Notes read (`SELF-VERIFIED`: focused Angular tests, the full Angular suite, Angular compilation, and the production web build). No requirement or baseline change.

The later post-Phase-9 confirmation work is committed by the owner in `02ebce3`; the tree was clean before this governance task.

## Current maintenance activity

Repository Cleanup Waves 1–5 are `COMPLETE`. No additional broad repository cleanup or reorganization is authorized. `.claude/settings.local.json` is ignored by `.gitignore` and is no longer tracked, so no source-control step remains for it. Production authentication, hosting, and backup evidence remain separate predeployment work.

An owner-approved documentation and AI-routing correction (2026-10-04) updated current-state text and moved historical handoff narrative word-for-word into [HISTORY.md](HISTORY.md). It is documentation only and changes no requirement, behavior, or phase scope. The owner reviews and commits it.

## History

Earlier checkpoints and task briefs, from the Phase 8 Incident work through Phase 9 closure, are in [HISTORY.md § Part A](HISTORY.md#part-a). They are kept word-for-word and authorize nothing.
