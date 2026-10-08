# Household member management repair — 2026-10-08

SELF-VERIFIED technical correction, explicitly authorized by the owner's pasted request. Bounded repair of WI-095-010 within CTX-1.39; no canonical rule changes, phase transition, facilitated session, or inferred owner acceptance.

## Finding and approved behavior

Existing domain, application service, repository, endpoints and Angular component already support create, rename, assign, move, unassign and delete. Family Settings contains `app-households`. Its original member dropdown saved immediately on selection, with no explicit Add/Move action; removal was labeled “Unassign member.” The correction addresses discoverability and deliberate saving, not missing backend capability.

Sources: [WI-095-010](../../ai-context/planning/stories/WI-095-010-household-grouping.md), [REQ-FAM-018](../../ai-context/core/product-contract.md#req-fam-018), [REQ-PRIV-034](../../ai-context/core/privacy-safety-contract.md#req-priv-034), and DEC-091. Only existing active adult members of the same Family are eligible, with zero or one Household per member per Family. Current Family admins manage; current members view. Membership loss clears assignment; rejoin/replacement inherits nothing. Empty Households persist. Permanent Household deletion unassigns members without ending Family membership. Grouping grants no protected-resource access.

## Correction and workflow

Open **Family → Family settings (gear) → Households**. Create a Household or find its named row. Its **Manage Members** section lists eligible existing Family members. Choose someone, then click **Add Member**. Selecting someone labeled “from [Household]” shows the source/destination and changes the action to **Move Member**. Current members have **Remove from Household** actions. Named success feedback appears after saving; server state reloads, and selections clear. Selection alone sends no mutation. Read-only members have no management controls.

Existing server authorization, eligibility, transaction locks, nondisclosing 404s, no-store headers and source/target version checks remain unchanged. No backend change, migration, invitation flow, downstream permission or dependency was added. The management panel uses existing dark theme tokens and shared form/button styles, with stacked actions at narrow widths and unique accessible region names per Household.

## Verification

| Check | Actual evidence/result |
|---|---|
| Focused Angular | `npm test -- --watch=false --include=src/app/households/households.spec.ts --include=src/app/family-dashboard.spec.ts`: 26 passed; repeated after the accessible-name correction, 26 passed. Covers explicit save, busy state, fresh projection/selection reset, source/target tokens, remove/delete, read-only state, error/Retry and Family/identity cancellation. |
| SQL-backed Household tests | `RUN_FAMILYSAFETY_SQL_TESTS=1`; `dotnet test FamilySafety.slnx --artifacts-path C:/Dev/blevins/tests/FamilySafety.Foundation.Tests/obj/household-verification --filter FullyQualifiedName~HouseholdTests --logger 'console;verbosity=minimal'`: 7 passed, 0 skipped. Exercises current admin/member authorization, cross-Family/inactive/nonmember rejection, constraints/concurrency, membership loss/rejoin/replacement and unchanged protected-feature data/access. |
| One broader frontend regression | `npm test -- --watch=false`: 422 passed in 27 files. Completed before the final region-label correction; focused tests and production build repeated afterward. No broad backend regression, since no backend behavior changed. |
| Final production build | `npm run build`: PASS. Initial bundle 729.96 kB versus 500 kB warning budget (latest visual-alignment baseline 727.21 kB). Existing style warnings: Concerns 7.06 kB, Trips 5.71 kB, Home 5.79 kB versus 4 kB. No thresholds changed. |
| Real browser journey | Updated `e2e/households.spec.ts`, run through a temporary port-isolated copy/config: `npx playwright test --config e2e/playwright.household-verification.temp.config.ts e2e/households.temp.spec.ts`: 1 passed. Creates two Households; verifies selection alone does not assign, explicit keyboard Add, named feedback, persistence across page reload, reviewed move, keyboard removal and persisted unassignment, reassignment, member read-only view, confirmed deletion, unchanged Family membership, 390px overflow and three zero-violation axe scans. |
| Visual review | Inspected real isolated-data captures `src/FamilySafety.Web/e2e/captures/households/admin-1440.png` and `admin-390.png`; also captured `member-390.png`. No overlapping controls or horizontal overflow in exercised layouts; dark cards, inputs and buttons preserved. These are automated technical checks, not human usability evidence. |
| Worktree | `git diff --check`: PASS. Initially clean; only this correction's files changed. Nothing staged, committed, pushed, branched or submitted as a PR. |

## Isolation and diagnostic artifacts

The running API (port 5080, PID 16128) and Angular server (4200, PID 26688) were preserved. Initial read-only Playwright inspection confirmed the ordinary running Family Settings showed admin creation and its existing unassigned members. Because the browser-control connector had no available browser, the explicitly requested Playwright tooling supplied browser verification.

The initial standard .NET build failed on DLL locks held by that API. Verification then used an isolated artifacts directory under the test project's ignored `obj/`. Temporary harness copies reused the existing manifest and guarded cleanup logic with API 5180, Angular 4300 and a matching proxy; teardown excluded pre-existing API process IDs. No tracked harness configuration was changed. Temporary scripts/config/spec/fixture copies were removed after testing. No existing databases or Phase 10 fixtures were reset or overwritten.

The first browser run found duplicate “Manage Members” region names. Fixed by including the Household name in each accessible label, without suppressing axe checks. Its database remains retained under failed-run policy: `FamilySafety_Invitation_E2E_aa55041dc8fa41c98326ee42aa901e62`, manifest `src/FamilySafety.Web/e2e/scenario-runs/20261008T051112-6092f001/manifest.json`. It is this repair's synthetic diagnostic data, not a Phase 10 fixture. The successful rerun's database `FamilySafety_Invitation_E2E_315d8ad1860d4aab9c33bd6ff515e0a1` and run files were removed and confirmed absent by the existing guarded harness; ports 4300/5180 are free. No unresolved Household product defect was found within the exercised scope.
