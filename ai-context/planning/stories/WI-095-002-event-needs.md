# WI-095-002 — Event Needs

- **Status:** COMPLETE / PASS — OWNER APPROVED (2026-10-04); Phase 9.5 CURRENT / IN PROGRESS.
- **Baseline/tier:** CTX-1.31 / DEEP.
- **Requirements:** REQ-EVT-003/004/009/010/011/014; REQ-PRIV-011/021/023/027/028/029.
- **Authorization:** Approved work item YES; execution YES. Explicit owner Event Needs request and responsibility-privacy decision, 2026-10-04 (DEC-084). Owner commits only.

## Required context

Entry README/status and current handoff/rules/routing; Events workflow (Need lifecycle and invitation dependency); privacy contract (listed anchors); client README and mockup alignment (existing presentation/boundary); VALIDATION (focused checks, one final regression). Relevant Event invitation/visibility implementation and tests supply conventions. History, old evidence, transcripts, unrelated workflows are excluded.

## Scope and acceptance

1. Organizer alone creates, edits title/detail/category, removes, assigns Open Needs and completes held Needs. Editors/admins gain no management authority.
2. Active invited, currently authorized members with Pending/Going/Maybe may claim Open Needs; owners release claims, accept/decline assignments and complete own responsibility. Exactly Open/Claimed/Assigned/Completed; one responsible person.
3. Not Going, revocation, visibility loss and membership loss release Claimed/Assigned Needs to Open, retaining prior state/person, reason/time. Restoration never restores responsibility; Completed remains completed.
4. Every read/write rechecks server authority; actor-aware projections restrict identities/history under REQ-PRIV-029. Selected Family supplies no authority. Conflicting transitions cannot overwrite one another; preserve Family-before-Event order.
5. Event-detail Needs UI supplies loading/empty/error/Retry and action feedback, management and personal actions with pending-assignment wording. Focused tests plus one final SQL-gated .NET/Angular/build/relevant Playwright/diff regression.

## Non-goals and stop conditions

No Updates, notifications, chat, generic tasks/history, Home changes, Check-Ins, Sharing, location, Plans/Trips, Event completion, audience editing or broad extraction. Stop only for a different material canonical conflict, product/privacy decision or unavoidable broad architecture change.

## Verification

COMPLETE / PASS — owner accepted implementation and final validation on 2026-10-04, without rerunning checks. Technical evidence below remains SELF-VERIFIED. [Verification closure](../verification/VER-WI-095-002.md). No next item is authorized.

| Acceptance / requirements | Evidence | Result |
|---|---|---|
| 1 — REQ-EVT-003/004/009; REQ-PRIV-011/021 | `EventNeedPersistenceTests.ManagementEligibilityValidationAndTransitionsAreBounded`, endpoint tests and organizer browser create/edit/assign/complete/remove. | PASS |
| 2 — REQ-EVT-010/011/014 | Domain transitions; SQL eligibility and lifecycle checks; browser claim/release, assignment decline/accept and completion. Invitation/RSVP remains authoritative. | PASS |
| 3 — REQ-EVT-011 | Eight SQL loss/held-state cases inspect persistence before any Need read, including release reason/time/person/state, unchanged Completed responsibility and no restoration; browser Not Going/revocation release. | PASS |
| 4 — REQ-PRIV-023/027/028/029 | SQL actor projections and serialized payload assertions cover organizer/self/eligible viewer/editor/admin, unauthorized administrator and released/completed/pending history. API account/404/409 checks; competing claims and deterministic Family-lock overlap with Event edit/RSVP loss. No cross-capability grants added. | PASS |
| 5 — UI / regression | Component loading/empty/Retry, versioned actions, context cancellation, conflict/access-loss recovery; integrated keyboard claim, axe and narrow overflow checks. Final regression below. | PASS |

- **Focused:** `dotnet test tests/FamilySafety.Foundation.Tests/FamilySafety.Foundation.Tests.csproj --filter FullyQualifiedName~EventNeed` with SQL gate: 36/36 (2 domain, 20 API/service, 14 SQL). Need Angular 11/11; existing Event dashboard/item-route 53/53; initial Need Playwright journey 1/1. Focused development failures were resolved before the final regression.
- **One final regression:** `RUN_FAMILYSAFETY_SQL_TESTS=1; dotnet test FamilySafety.slnx`: 299/299; `npm test -- --watch=false`: 339/339, 18 files; `npm run build`: PASS. Final relevant Playwright: 5/5 across `event-needs`, `events-dashboard`, `item-links`, filtered to `organizer management|direct entry, privacy|Family switching clears|transient Event|editing an Event`. Final minor presentation refinements also received a fresh focused Need component run, 11/11. `git diff --check`: PASS.
- **Build warnings:** Initial 660.25 kB versus 500 kB warning (prior 638.96 kB); existing Concern 7.01 kB and Trip 5.17 kB versus 4 kB warnings. Thresholds unchanged; Need styles produce no warning. Existing Playwright NO_COLOR/FORCE_COLOR notice remains.
- **SQL/artifacts:** Migration `20261005003108_EventNeeds` preserves existing participation, supports rollback/reapplication, and adds Needs, responsibility changes, constraints/indexes and three atomic loss triggers. Disposable SQL tests clean their databases. Final browser harness confirmed 3/3 databases removed, 0 retained, port 5080 free and no API process left. A transient manifest-replacement warning during cleanup was followed by confirmed database removal and successful manifest/run cleanup; no harness changes were made.
- **UI evidence:** Two Need captures under ignored `src/FamilySafety.Web/e2e/captures/event-needs/` (organizer 1440, participant 390), visually inspected; shared card/button/typography patterns retained. No specific Need mockup exists. Axe reports zero violations on those states; keyboard claim passed. Technical checks remain self-verification; owner accepted the implementation and validation. This is not production/deployment certification. The final Playwright run is relevant selection, not the full suite.

Owner performs all Git commits. Closure authorizes no further implementation.
