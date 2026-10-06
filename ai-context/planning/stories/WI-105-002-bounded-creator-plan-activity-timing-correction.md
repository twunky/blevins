# WI-105-002 — Bounded Creator Plan Activity and Timing Correction

## Metadata / authorization

- **Status:** COMPLETE / PASS — OWNER APPROVED, 2026-10-06.
- **Phase:** 10.5 — MVP Functional Completion.
- **Context baseline:** CTX-1.39, successor to the verified entry checkpoint CTX-1.38.
- **Requirements:** REQ-PLAN-002/005/007/008; REQ-PRIV-011/020/027; REQ-PROD-014.
- **Model tier:** DEEP.
- **Approved / execution authorized:** YES — explicit owner request, 2026-10-06, including narrow canonical reconciliation under DEC-095. Owner performs Git commits; leave changes unstaged; no PR.

## Required context manifest

| Source | Purpose / timing |
|---|---|
| `ai-context/README.md`, `ai-context/project-status.md` | Operating authority and checkpoint / entry |
| `ai-context/workflows/plans-trips.md`, Plan anchors only | Creation, audience, separate Notes, bounded correction / execution and verification |
| `ai-context/core/privacy-safety-contract.md`, REQ-PRIV-011/020/027 | Current access, no admin override, nondisclosure / execution and verification |
| `ai-context/core/requirements-index.md`, Plan rows; `ai-context/decision-log.md`, DEC-082/092/094/095 | Trace and owner authority / context update and verification |
| `ai-context/planning/roadmap.md`, Phase 10.5 | Placement and deferred human gate / entry and closeout |
| `claude-handoff/WORKING-RULES.md`, `CONTEXT-ROUTING.md`, `VALIDATION.md`, `MOCKUP-ALIGNMENT.md`; Web README | Scope, existing UI patterns and lean checks / execution |

Excluded: archives/bootstrap, unrelated workflows, historical verification except a concrete regression question. Inspect existing Plan code/tests and Guid/EF concurrency patterns only as needed.

## Goal and bounded contract

Correct ordinary current Plan facts on the existing Plan without creating a conflicting second Plan. One atomic replacement contains exactly `ActivityOrDestination`, `Date`, `Time`, `ExpectedReturnDate`, `ExpectedReturnTime`, `ExpectedCheckInDate`, `ExpectedCheckInTime`, plus a required concurrency token. Reuse creation validation, activity maximum 200, required activity/date/time and both-or-neither optional timing pairs. No automatic timing shifts.

The server checks exact original creator, current Plan access and active membership in the stored Plan Family at acceptance. No retained authority after departure, administrator override, selected-Family/known-ID/audience/previous-membership grant. Missing and unauthorized remain nondisclosing. Audience mode/selected rows, owner, Family and Notes are outside the request and unchanged. Whole-Family visibility stays dynamic. Notes retain their separate mutation. Expected Check-In remains descriptive only; no Check-In, reminder, notification, Concern, Incident or safety inference.

Small Guid/EF fact-concurrency token; stale writes return 409. Fact correction and Notes cannot overwrite each other. Current corrected values are canonical on detail/list/Home rereads, without history or global client state.

## Non-goals

No audience replacement, completion/reopen/archive/delete, transfer/reassignment, Notes rewrite, history/audit, acknowledgements, notifications, scheduled/contextual Check-Ins/reminders, location/sharing expansion or generic lifecycle/concurrency framework. DEC-092 intact. WI-010-004 Stage B remains DEFERRED — MVP-END HUMAN VALIDATION. No next work item is created or authorized.

## Acceptance checks / exit criteria

1. **AC-01:** Eligible creator corrects all seven fields; optional timings set/change/clear; required/length/date/time and partial pairs rejected.
2. **AC-02:** Server denies departed creator, audience viewer, admin, unrelated selected Family and known ID using current nondisclosure.
3. **AC-03:** Audience/recipient rows, Notes, creator and Family preserved; dynamic whole-Family access unchanged; authorized readers see current facts.
4. **AC-04:** Required current token succeeds and advances, stale token conflicts atomically, independent Notes writes preserved.
5. **AC-05:** Existing detail has eligible-only prefilled correction form, Save/Cancel, validation, clear/loading/error/conflict feedback; Notes separate and Check-In copy descriptive.
6. **AC-06:** Detail/list/Home use corrected facts and existing relevance; focused integrated journey, desktop/narrow presentation checks.
7. **AC-07:** Safe smallest migration, focused backend/SQL/Angular/Playwright, one final SQL-gated .NET regression, Angular suite/build, relevant Playwright set, context/reference and whitespace checks pass; canonical owner decision matches implementation.

## Tasks / evidence

- [x] Canonical activation and separate context verification.
- [x] Domain/schema/API and focused authorization/boundary/concurrency coverage.
- [x] Existing detail form and focused Angular/integrated coverage.
- [x] Final validation and SELF-VERIFIED report; hand back VERIFYING / OWNER REVIEW.

Evidence: [VER-WI-105-002](../verification/VER-WI-105-002.md) COMPLETE / PASS; technical evidence SELF-VERIFIED and accepted by the owner, 2026-10-06. WI-105-002 is closed. Next activity: Final Functional MVP Readiness Review, separately authorized; no implementation work item is authorized.
