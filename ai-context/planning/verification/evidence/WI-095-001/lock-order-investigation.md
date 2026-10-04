# WI-095-001 evidence repair and lock-order investigation

Date: 2026-10-04. Implementing Codex agent; **SELF-VERIFIED**. HEAD at start: `b8af6798cada23ea4d7a48e02e7c1ebb5e9368de`; working tree was clean. CTX-1.30 ACTIVE, Phase 9.5 CURRENT / IN PROGRESS, WI-095-001 VERIFYING / OWNER REVIEW. No next item authorized.

**Classification: REPRODUCED DEFECT. Closure readiness: NOT READY.** Investigation stopped after the first coordinated SQL iteration returned deadlock error 1205. No product code, isolation, lock ordering or retries were changed. This supplemental finding blocks reliance on the earlier verification record's automated PASS conclusion. Owner visual acceptance and explicit closure remain pending.

## Source and lock order

All four mutations begin their own Serializable transaction, hold relevant locks until commit or rollback/disposal, and use a no-op `UPDATE FamilyGroups SET CapacityLimit = CapacityLimit` as the explicit Family lock. `AsNoTracking` affects EF tracking, not Serializable SQL locks.

| Actual path | Operations within transaction | Source |
|---|---|---|
| Event detail update | Begin; Family UPDATE; active membership SELECT; tracked Event/editor-authorization SELECT; update entity in memory; EF `SaveChangesAsync` issues Event UPDATE; commit. Returned view is read after commit. | [EfFamilyEventRepository.cs](../../../../../src/FamilySafety.Infrastructure/Persistence/EfFamilyEventRepository.cs), `UpdateAsync` lines 89–129; Family lock lines 218–223. |
| Invitation issue | Begin; Event SELECT resolving FamilyGroupId; Family UPDATE; canonical Event visibility SELECT; organizer membership/profile and invitee membership/audience reads; active invitation existence read; EF INSERT invitation; commit. | [EfEventInvitationRepository.cs](../../../../../src/FamilySafety.Infrastructure/Persistence/EfEventInvitationRepository.cs), `IssueAsync` lines 53–70; shared helper lines 119–134. |
| Invitation revoke | Begin; same Event SELECT then Family UPDATE and canonical visibility; organizer profile read; fresh invitation SELECT; validate active state/version; EF UPDATE invitation; commit. | Same repository, `RevokeAsync` lines 73–84, `FreshInvitationAsync` lines 110–117, save lines 104–107. |
| RSVP update | Begin; same Event SELECT then Family UPDATE and canonical visibility; active actor profile read; fresh invitation SELECT; validate ownership/state/version/status; EF UPDATE invitation; commit. | Same repository, `RespondAsync` lines 87–101; shared read/save helpers above. |

The opposing resource order is Event edit **Family → Event**, versus all invitation mutations **Event → Family**. The initial invitation Event read occurs inside Serializable, so its read locks remain while it waits for the Family UPDATE. Event editing holds its Family write lock while attempting its Event write. The resulting cycle prevents either transaction from completing until SQL Server chooses a victim. The focused run records actual SQL and transaction isolation; no native deadlock graph was recovered, so individual lock IDs/index names are not claimed.

## Focused reproduction

[Test source](../../../../../tests/FamilySafety.Foundation.Tests/EventInvitationPersistenceTests.cs), `EventEditAndRsvpDoNotDeadlockWhenTransactionsOverlap`, line 269, uses the existing migrated disposable real-SQL fixture and actual `EfFamilyEventRepository.UpdateAsync` / `EfEventInvitationRepository.RespondAsync` paths with separate contexts. The observer pauses Event editing after its Family UPDATE completes, then starts RSVP. After RSVP's Event lookup has completed and its Family UPDATE is about to execute, the observer releases Event editing. There are no lock hints, raw mutation SQL, retries or production behavior changes.

Three iterations were planned, stopping on first failure. **One executed; deadlock reproduced immediately.** Each iteration has a 30-second cancellation deadline and 20-second SQL command timeout. Native output shows Serializable on both paths, the Event UPDATE being issued, Event edit success, RSVP failure, victim SPID 60, and `SqlException.Number=1205` (state 51, class 13). This was a deadlock, not command timeout or deadline cancellation. Native result: 1 failed test, exit 1, 7.4058-second test run. The regression deliberately asserts both operations succeed and remains failing until the product defect is fixed.

Reproduce in PowerShell:

```powershell
$env:RUN_FAMILYSAFETY_SQL_TESTS = '1'
dotnet test tests/FamilySafety.Foundation.Tests --filter FullyQualifiedName~EventEditAndRsvpDoNotDeadlockWhenTransactionsOverlap --logger "console;verbosity=detailed"
```

Full native output: [sql-lock-order.txt](sql-lock-order.txt). Fixture cleanup runs `EnsureDeletedAsync` in `finally`, after the operation transactions/contexts are disposed. No broad test rerun followed the stop condition.

## Smallest proposed correction — requires separate owner authorization

Resolve the Event's Family lock target outside the invitation Serializable transaction as a non-authorizing lookup. Then begin Serializable, acquire the Family lock first, and re-read canonical Event visibility plus all current membership, role, invitation and version checks inside that transaction. Verify the re-read Event still belongs to the resolved Family before mutation. Apply through the common helper used by issue, revoke and RSVP; preserve existing isolation and conflict/privacy behavior. This removes the retained Event read lock while waiting for Family and aligns the order with Event editing. Do not substitute retries for consistent lock order.

Retain the failing coordinated regression and extend its conflict pairs to Event edit versus issue and revoke. After an authorized correction, require all iterations to pass without 1205, timeout or cancellation, then repeat focused eligibility/concurrency/privacy checks and the full SQL-gated suite. No correction has been implemented in this task.

## Evidence dispositions and validation

| Artifact / check | Disposition / actual result |
|---|---|
| `dotnet-tests-sql-final.txt` | Empty historical excerpt replaced by a new SQL-gated full run: 260 passed, 0 failed, 0 skipped; 260 named native outcomes retained. |
| `dotnet-tests-sql.txt` | Explicitly labelled SUPERSEDED / INVALID; points to the sole new authoritative full-run excerpt. It does not prove the earlier historical failure/counts. |
| `sql-focused.txt` | Empty excerpt replaced by a new explicit gate=1 run: 10 passed, 0 failed, 0 skipped. |
| `dotnet-build.txt` | Damaged transcript replaced by a fresh `dotnet build FamilySafety.slnx`: PASS, 0 warnings/errors. |
| `angular-full.txt` | Damaged transcript replaced by fresh `npm test -- --watch=false`: 328 tests / 17 files passed. |
| `type-template.txt` | Damaged transcript replaced by fresh `npx ngc -p tsconfig.app.json --noEmit`: exit 0. |
| `web-build.txt` | Damaged transcript replaced by fresh `npm run build`: exit 0; existing initial/Concern/Trip budget warnings retained. No budget changed. |
| `playwright-full.txt` | Damaged transcript replaced by selected exact intact output from the original run `20261004T223339-62920d07`, not a new run: 38 passed, exit 0, 14 databases created/removed, 0 retained. |
| `playwright-source-excerpt.json` | Original output timestamps, call/chunk identifiers, selected intact native lines and native completion exit status. Hash referenced by the recovered transcript. No truncated text was reconstructed. |
| `backend-focused.txt` | New focused domain/service/API native excerpt: 31 passed (7/12/12), 0 failed/skipped. |
| `sql-lock-order.txt` | New complete native investigation output: 1 failed, SQL deadlock 1205. |
| `cleanup-repair.txt` | New measured cleanup: identical starting/ending 20 database names; no task database remains; ports 4200/5080 free; no task API/test/browser process candidates. |
| `evidence-repair-checks.txt` | UTF-8/artifact health, raw-source hash/count checks, relative-reference checks, unchanged screenshots and whitespace validation. |

The successful full build, 260-test full run, 10-test SQL run and 31-test backend run preceded addition of the new failing regression. They do not establish that the current expanded suite passes. Fresh SQL transcripts identify gate=1, localhost/Windows integrated authentication, command, UTC times, native exit status, and raw UTF-8 source path/hash. Build/type/Angular transcripts directly capture native output. The recovered Playwright evidence is explicitly original-run evidence; prior screenshot and focused browser evidence was not changed.

## Verification bookkeeping stopped

`VER-WI-095-001.md`, acceptance criteria, story/context/roadmap status and owner gates were left unchanged because Part F requires RISK DISPROVED before reconciliation. Its stale implementation-uncommitted and retained-diagnostic database statements remain deferred corrections, and its historical .NET-failure claim is not proved by the invalid old excerpt. Current facts are recorded here and in replacement transcripts. Do not use the earlier automated PASS/owner-review next-action conclusion to bypass the reproduced defect. AC-21/24/25 owner acceptance remains incomplete; no closure or sibling work is authorized.

All changes are verification tests/evidence only, unstaged and uncommitted. The owner performs every commit.
