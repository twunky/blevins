# WI-095-001 evidence repair and lock-order investigation

Date: 2026-10-04. Implementing Codex agent; **SELF-VERIFIED**. HEAD at start: `b8af6798cada23ea4d7a48e02e7c1ebb5e9368de`; working tree was clean. CTX-1.30 ACTIVE, Phase 9.5 CURRENT / IN PROGRESS, WI-095-001 VERIFYING / OWNER REVIEW. No next item authorized.

**Resolution: REPRODUCED AND FIXED.** The initial real-SQL reproduction returned deadlock 1205 (retained in [sql-lock-order.txt](sql-lock-order.txt)). The owner then authorized the bounded Family-first correction. The shared invitation helper now uses the same Family → Event order as Event editing; Serializable isolation, authorization checks and Version concurrency remain. Current coordinated SQL coverage passes issue, revoke and RSVP, three iterations each. Owner visual acceptance and explicit WI closure remain pending.

## Source and lock order

All four mutations begin their own Serializable transaction, hold relevant locks until commit or rollback/disposal, and use a no-op `UPDATE FamilyGroups SET CapacityLimit = CapacityLimit` as the explicit Family lock. `AsNoTracking` affects EF tracking, not Serializable SQL locks.

| Actual path | Operations within transaction | Source |
|---|---|---|
| Event detail update | Begin; Family UPDATE; active membership SELECT; tracked Event/editor-authorization SELECT; update entity in memory; EF `SaveChangesAsync` issues Event UPDATE; commit. Returned view is read after commit. | [EfFamilyEventRepository.cs](../../../../../src/FamilySafety.Infrastructure/Persistence/EfFamilyEventRepository.cs), `UpdateAsync` lines 89–129; Family lock lines 218–223. |
| Invitation issue (before correction) | Begin; Event SELECT resolving FamilyGroupId; Family UPDATE; canonical Event visibility SELECT; organizer membership/profile and invitee membership/audience reads; active invitation existence read; EF INSERT invitation; commit. | [EfEventInvitationRepository.cs](../../../../../src/FamilySafety.Infrastructure/Persistence/EfEventInvitationRepository.cs), `IssueAsync`; shared mutation helper. |
| Invitation revoke (before correction) | Begin; same Event SELECT then Family UPDATE and canonical visibility; organizer profile read; fresh invitation SELECT; validate active state/version; EF UPDATE invitation; commit. | Same repository, `RevokeAsync`, `FreshInvitationAsync`, save helper. |
| RSVP update (before correction) | Begin; same Event SELECT then Family UPDATE and canonical visibility; active actor profile read; fresh invitation SELECT; validate ownership/state/version/status; EF UPDATE invitation; commit. | Same repository, `RespondAsync`; shared read/save helpers. |

The original opposing resource order was Event edit **Family → Event**, versus all invitation mutations **Event → Family**. The invitation Event lookup retained locks while the mutation waited on Family; Event editing held Family while requesting its Event update. SQL Server chose the RSVP transaction as victim. The correction resolves the target Family before the transaction, acquires its lock first inside Serializable, then re-reads and authorizes the Event and verifies the Family still matches. Individual lock IDs/index names were not captured.

## Focused reproduction

[Test source](../../../../../tests/FamilySafety.Foundation.Tests/EventInvitationPersistenceTests.cs), `EventEditAndInvitationMutationDoNotDeadlockWhenTransactionsOverlap`, uses the existing migrated disposable real-SQL fixture and actual `EfFamilyEventRepository.UpdateAsync` plus invitation repository paths with separate contexts. For issue, revoke and RSVP, the observer pauses Event editing after its Family UPDATE completes, then coordinates the mutation's Family UPDATE. Each path runs three iterations with 30-second cancellation deadlines and 20-second SQL command timeouts. There are no lock hints, raw mutation SQL or retries.

Original test `EventEditAndRsvpDoNotDeadlockWhenTransactionsOverlap` reproduced a deadlock in the first iteration: SQL error 1205, Event edit success, RSVP victim. After the shared ordering correction, **3/3 SQL test cases pass**, covering **9/9 coordinated overlaps** across issue, revoke and RSVP. Each mutation preserves its ordinary state/version behavior and the regression asserts both operations succeed.

Reproduce in PowerShell:

```powershell
$env:RUN_FAMILYSAFETY_SQL_TESTS = '1'
dotnet test tests/FamilySafety.Foundation.Tests --no-restore --filter FullyQualifiedName~EventEditAndInvitationMutationDoNotDeadlockWhenTransactionsOverlap --logger "console;verbosity=normal"
```

The initial failure output is in [sql-lock-order.txt](sql-lock-order.txt). Fixture cleanup runs `EnsureDeletedAsync` in `finally`, after operation transactions/contexts are disposed.

## Applied correction

All three mutations resolve only the Family target before the Serializable transaction. The shared helper then locks Family, re-reads canonical Event visibility, checks the Event still belongs to the locked Family, and returns it for the existing in-transaction membership, role, exact-profile, invitation, eligibility and Version checks. The target lookup grants no authority. Isolation and conflict/privacy behavior remain unchanged.

The regression retains all three mutation pairs. No generic retry, transaction framework, schema change or unrelated behavior was added.

## Evidence dispositions and validation

| Artifact / check | Disposition / actual result |
|---|---|
| `dotnet-tests-sql-final.txt` | Sole authoritative concise final SQL-gated full run: solution build plus 263 passed, 0 failed, 0 skipped; named native pass rows retained. |
| `dotnet-tests-sql.txt` | Explicitly labelled SUPERSEDED / INVALID; points to the sole new authoritative full-run excerpt. It does not prove the earlier historical failure/counts. |
| `sql-focused.txt` | Earlier 10/10 focused run; it predates the lock-order regression. The current 13/13 focused result is included in the sole final full-gate transcript. |
| `dotnet-build.txt` | Earlier fresh `dotnet build FamilySafety.slnx`: PASS, 0 warnings/errors. The final full-gate run also compiled the corrected solution. |
| `angular-full.txt` | Damaged transcript replaced by fresh `npm test -- --watch=false`: 328 tests / 17 files passed. |
| `type-template.txt` | Damaged transcript replaced by fresh `npx ngc -p tsconfig.app.json --noEmit`: exit 0. |
| `web-build.txt` | Damaged transcript replaced by fresh `npm run build`: exit 0; existing initial/Concern/Trip budget warnings retained. No budget changed. |
| `playwright-full.txt` | Damaged transcript replaced by selected exact intact output from the original run `20261004T223339-62920d07`, not a new run: 38 passed, exit 0, 14 databases created/removed, 0 retained. |
| `playwright-source-excerpt.json` | Original output timestamps, call/chunk identifiers, selected intact native lines and native completion exit status. Hash referenced by the recovered transcript. No truncated text was reconstructed. |
| `backend-focused.txt` | New focused domain/service/API native excerpt: 31 passed (7/12/12), 0 failed/skipped. |
| `sql-lock-order.txt` | Original native regression failure, SQL deadlock 1205, retained to document discovery. |

The final `dotnet test FamilySafety.slnx` run built the solution and passed 263/263 with `RUN_FAMILYSAFETY_SQL_TESTS=1`. Focused invitation persistence passed 13/13; service/API passed 24/24; the coordinated lock-order cases passed 9/9 overlaps. SQL fixtures cleaned their disposable databases normally. The full console also logged SQL error 4060 from background maintenance during an existing negative Development fixture startup test; that named test passed. This is outside the changed invitation path and was not modified or investigated further. Frontend code and evidence were not rerun or changed for this backend correction.

## Verification bookkeeping stopped

`VER-WI-095-001.md` now records the discovered and corrected lock order, the passing mutation-specific SQL regression and the final full gate. Acceptance criteria, context/roadmap status and owner gates were not changed. AC-21/24/25 owner acceptance remains incomplete; WI-095-001 remains VERIFYING / OWNER REVIEW, and no sibling work is authorized.

This correction changes the invitation repository, its focused SQL regression and the two verification records only. Changes remain unstaged and uncommitted; the owner performs every commit.
