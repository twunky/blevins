---
Status: DONE / PASS — OWNER APPROVED
Authority: Project-owner approved and execution-authorized for WI-009-001 only (2026-10-02), against CTX-1.27; planning basis PRP-030 / DEC-074
Context-Baseline: CTX-1.27
Version: 1.0
---

# WI-009-001 — Playwright Per-Spec Scenario Isolation

## Metadata

- **Work Item ID:** `WI-009-001`
- **Status:** `DONE / PASS` — `OWNER APPROVED` (owner review `PASS`, 2026-10-02; `VER-WI-009-001`). Approved and execution-authorized by the project owner on 2026-10-02 against `CTX-1.27`. See [Execution authorization](#execution-authorization) and [Owner execution conditions](#owner-execution-conditions).
- **Phase:** `9 — Integration & UX Refinement`, `CURRENT / IN PROGRESS` (`DEC-073`)
- **Context baseline:** `CTX-1.27`
- **Approved planning basis:** `PRP-030` (`APPROVED`, `DEC-074`): theme 1 "Test reliability foundation", MUST item M1, and the "First work item recommendation".
- **Requirement references:** none changed. The item protects the end-to-end evidence used for `REQ-PRIV-025`, `REQ-PRIV-026`, the Trip audience rules, and the invitation path; it alters none of them.
- **Model tier:** `STANDARD`. Test infrastructure only, but it adds automated database deletion, so its safeguards need care.
- **Phase 9 exit criteria served:** 1 (test reliability). It is also the base for criteria 3, 5–8, and 12, which need new journeys.
- **Standing auto-approval:** `DEC-032` does **not** apply (see [DEC-032 review](#dec-032-review)). Explicit owner approval and execution authorization were given on 2026-10-02, so no further routine approval is needed to execute it.

## Goal

Make the Playwright suite deterministic and independently runnable, so that Phase 9 cross-feature verification can rely on one complete suite run: every spec starts from its own fresh, known scenario state, and the result never depends on which specs run or in what order.

## Current state (verified 2026-10-02)

- `e2e/playwright.config.ts` creates **one** database name per run (`FamilySafety_Invitation_E2E_<guid>`) and starts **one** API through `webServer` with `scripts/start-invitation-scenario.ps1`. All four specs share it.
- `concern-workflow.spec.ts` makes Adult B a Family member. `invitation-handoff.spec.ts` requires B to have **no** membership, so it fails when it runs after the Concern spec. Each spec passes when run alone.
- `isInvitationOnlyRun` inspects `process.argv` for spec file names to decide whether to add Adult C (`-IncludeTripAudienceAdult`). The invitation journey needs no Adult C; the Concern, Incident, and Trip-audience journeys need Adult C.
- The API listens on fixed port 5080, and the Angular dev server proxies `/api` to it (`proxy.conf.json`). `workers: 1`, `fullyParallel: false`.
- The API migrates and seeds the scenario at startup (`DevelopmentFixtureRegistration`, Development only, guarded to `FamilySafety_Invitation_*` databases).
- **No cleanup exists.** Every run leaves a `FamilySafety_Invitation_E2E_*` database on the local SQL Server. The current safeguards are the name checks in `playwright.config.ts`, `e2e/fixtures/invitation-scenario.ts`, the scenario script, and the API fixture guard.
- `src/FamilySafety.Web/e2e-alt/` contains only empty folders. It is a cleanup candidate and is not used by this item.

## Isolation design

Prefer per-spec database isolation over conditional reset logic.

1. **One scenario per spec file.** Each spec file declares the scenario it needs (for example, with or without Adult C) through one shared helper, such as `useIsolatedScenario({ tripAudienceAdult: true })`. The helper registers file-level `beforeAll` / `afterAll` hooks. Tests inside one file share that file's scenario, as today.
2. **Fresh database and API per spec file.** In `beforeAll`, the helper generates a new name matching `^FamilySafety_Invitation_E2E_[A-Za-z0-9]+$`, records it in a run manifest, starts the API through the scenario script with that name and the declared options, and waits for `http://localhost:5080/health`. The existing API fixture initializer migrates and seeds the new database.
3. **The Angular dev server stays global.** It remains the single `webServer` entry on port 4200 and keeps proxying to port 5080. The API entry leaves `webServer`.
4. **Build once.** A global setup builds the API once; per-spec starts use the scenario script with a new `-NoBuild` switch (`dotnet run --no-build`), so each start does not rebuild.
5. **Clean start and stop.** Before starting, the helper confirms port 5080 is free and fails the spec setup if another process holds it. In `afterAll`, it stops the API process tree (on Windows, `taskkill /PID <pid> /T /F` or an equivalent) and confirms port 5080 is free before the next spec starts.
6. **Guarded cleanup.** After the API stops:
   - if the spec file passed, a new `scripts/remove-scenario-database.ps1` drops that database;
   - if it failed, the database is kept for diagnosis, and its name and the removal command are printed;
   - setting `FAMILY_SAFETY_E2E_KEEP_DATABASES=1` keeps all of them.
7. **Deletion safeguards** (all required in `remove-scenario-database.ps1`, and repeated in the TypeScript caller):
   - The name must match `^FamilySafety_Invitation_E2E_[A-Za-z0-9]+$` exactly. Blank input, `FamilySafety`, and any other name are refused.
   - Only names recorded in the **current run's** ownership manifest, and absent from the PRE-EXISTING list, are dropped automatically. The script requires the name and the manifest path explicitly and fails closed if ownership cannot be proven. Older databases are never deleted by pattern; at most, global teardown lists them.
   - The script logs the exact database name before dropping it and returns a non-zero exit code on unsafe input or failure.
   - No credentials are embedded; it uses Windows integrated security, as the tests already do.
   - The server defaults to `localhost`, as in the scenario script.
   - The name is passed as data and quoted with `QUOTENAME`, never concatenated into SQL.
   - It uses the `System.Data.SqlClient` built into Windows PowerShell 5.1, so no new module or package is needed.
   - It runs `ALTER DATABASE … SET SINGLE_USER WITH ROLLBACK IMMEDIATE` and then `DROP DATABASE`, only after the API for that database has stopped.
8. **No spec-specific configuration.** `isInvitationOnlyRun` and every `process.argv` check are removed. `FAMILY_SAFETY_E2E_DATABASE` stops being one global value; the fixture exposes the current spec's database name instead. The existing scenario annotation (`recordScenario`) keeps working.
9. **Reusable by new specs.** A future Phase 9 spec gets an isolated scenario by calling the helper once at the top of the file. This is documented.

Workers stay at 1, because port 5080 is fixed. Parallel execution is not a goal of this item.

## Owner execution conditions

Recorded with the owner's approval on 2026-10-02. They are part of the authorized scope and **override any looser wording elsewhere in this item**.

### Approved architecture

- Each spec file gets its own isolated scenario database and its own API process.
- The Angular dev server stays shared. The API build may be reused between specs.
- Each spec declares its scenario needs explicitly. `isInvitationOnlyRun` is removed.
- Existing privacy and authorization assertions stay unchanged. Product behavior is not altered to make the suite pass.

### Pre-existing databases

- Before any implementation or cleanup, enumerate every existing `FamilySafety*` test or scenario database on the local server and record the list as **PRE-EXISTING**, in `VER-WI-009-001` and in a file the cleanup logic reads.
- Pre-existing databases are never deleted automatically, even if their names match the pattern.

### Database deletion — authorized only when ALL conditions hold

Automated cleanup is authorized **only** for databases created by the current Playwright run, and only when every condition below is true:

1. The name matches the exact approved pattern `^FamilySafety_Invitation_E2E_[A-Za-z0-9]+$`.
2. The name is not `FamilySafety`.
3. The current run created it: the name is recorded in the current run's ownership manifest and is not in the PRE-EXISTING list.
4. Cleanup runs only after the corresponding spec **passes**.
5. A failed spec's database is preserved for diagnosis.
6. The logic **fails closed**: if ownership cannot be proven (missing or unreadable manifest, name absent, name pre-existing, any doubt), it does not drop.
7. No wildcard deletion.
8. No deletion based only on a partial prefix.
9. No deletion of pre-existing scenario databases.
10. The script logs exactly which database it intends to drop before dropping it.

**If any safeguard fails, the database is not dropped.**

### Removal script requirements (`scripts/remove-scenario-database.ps1`)

- Requires an explicit database name and the ownership manifest path. Blank or null input is rejected.
- Validates the exact pattern; rejects `FamilySafety` and other production or default names.
- Rejects names not recorded as owned by the run in the given manifest.
- Uses safe SQL execution: the name is validated, quoted with `QUOTENAME`, and checked against `sys.databases`. It is never concatenated unchecked into SQL.
- Embeds no credentials. It uses the same local SQL convention as the tests (`Server=localhost`, Windows integrated security / `Trusted_Connection=True`).
- Returns a non-zero exit code on any unsafe input or failed deletion.

### Failed-spec behavior

- Stop the spec's API process cleanly.
- Preserve its database, and mark it retained in the manifest.
- Report the database name in the test output and in `VER-WI-009-001`.
- Do not retry destructive cleanup automatically. A later manual removal goes through the same script, with the manifest of the run that created it.
- Otherwise follow the test runner's normal behavior.

### API process management

- Each spec's API uses only its isolated database.
- It runs on the controlled port 5080. Before starting, confirm the port is free; if another process holds it, fail the spec setup rather than stop an unknown process.
- It must be confirmed ready (health check) before the spec proceeds.
- It stops cleanly after the spec (process tree), and the port is confirmed free before the next spec begins.
- No orphaned `dotnet` or API processes remain after the run.

### Dependencies

No new package of any kind. `@axe-core/playwright` belongs to a later Phase 9 accessibility item.

### Known later Phase 9 findings — out of scope here

Recorded so they are not lost; this item does not address them:

1. Events and Plans need single-item server reads for refresh-safe deep links.
2. Showing administrators in Family conflicts with `DEC-033` and needs a separate decision or proposal.
3. `@axe-core/playwright` belongs to a later accessibility item.
4. `OD-002` remains outside Phase 9.
5. Root-component decomposition is a later Phase 9 item.

## Boundaries / non-goals

- Test infrastructure only: `src/FamilySafety.Web/e2e/`, `scripts/`, and documentation.
- No change to product behavior, API, authorization, schema, migrations, `DevelopmentFixtureRegistration`, or the Development identity and outbox guards. If isolation turns out to need such a change, stop and report it.
- No change to the meaning of any existing assertion. Specs may change only how they obtain their scenario (imports, helper call, database name source).
- No new npm or NuGet dependency. `@axe-core/playwright` is approved under `DEC-074` but belongs to a later accessibility item.
- No weakening of database-name checks or deletion safeguards, and no pattern-wide deletion of old databases.
- No new journeys, no parallel workers, no CI configuration.
- Do not delete `e2e-alt/` or the root build-output folders.

## Acceptance checks

1. `AC-01` — Each existing spec passes when run on its own: `concern-workflow`, `incident-workflow`, `invitation-handoff`, and `trip-audience-removal`.
2. `AC-02` — The complete suite passes with one command (`npm run test:e2e`).
3. `AC-03` — The complete suite passes three consecutive times from a clean start, with the counts recorded.
4. `AC-04` — Spec order does not affect results. At least one run puts `concern-workflow` before `invitation-handoff` (the order that fails today), and one run uses the reverse order; both pass.
5. `AC-05` — After a fully successful run, no `FamilySafety_Invitation_E2E_*` database created by that run remains, and every PRE-EXISTING database is still present. This is shown by a query of `sys.databases` before and after.
6. `AC-06` — Database-name safety checks are still enforced. Negative checks show that the removal script refuses, with a non-zero exit and no drop: blank input; `FamilySafety`; a name outside the pattern; a partial prefix; a pattern-matching name not in the current run's manifest; a PRE-EXISTING name; and a missing or unreadable manifest. A positive check shows it logs the exact name before dropping. The API fixture guard is unchanged.
7. `AC-07` — No production application behavior changes. No file under `src/FamilySafety.Api`, `src/FamilySafety.Application`, `src/FamilySafety.Domain`, `src/FamilySafety.Infrastructure`, or `src/FamilySafety.Web/src` changes, and the .NET and Angular unit test counts are unchanged.
8. `AC-08` — Authorization and privacy tests are not weakened. Every existing assertion in the four specs is unchanged in meaning (diff review recorded), and the denied-access and nondisclosure steps still run and pass.
9. `AC-09` — Documentation (`docs/local-development.md`, and a short comment in the helper) explains how a future spec obtains an isolated scenario, what the options are, how cleanup and retention work, and how to remove a retained database manually.
10. `AC-10` — `isInvitationOnlyRun` and all `process.argv`-based spec detection are gone from the Playwright configuration.
11. `AC-11` — When a spec fails (shown with a deliberately failing temporary spec or an equivalent controlled failure that is removed afterwards), its API process stops, its database is kept and marked retained, its name is reported in the output and in `VER-WI-009-001`, and no automatic cleanup retry happens. `FAMILY_SAFETY_E2E_KEEP_DATABASES=1` keeps all databases.
12. `AC-12` — Between spec files, the previous API process tree has exited and port 5080 is free before the next scenario starts. No orphaned `dotnet` processes remain after the run.
13. `AC-13` — No new npm or NuGet dependency, and no Angular budget or Playwright timeout is raised to make the suite pass.
14. `AC-14` — `VER-WI-009-001` records the commands, counts, timings for all three runs, the before and after database queries, the negative safety checks, and any limitation.

## Tasks

- [x] `TASK-001` — Before any implementation or cleanup, enumerate every existing `FamilySafety*` test or scenario database and record the list as **PRE-EXISTING** (in `VER-WI-009-001` and a file the cleanup logic reads); delete nothing. Then record a baseline: run each spec alone and the full suite once; record the known failure and the timings. Re-enumerate afterwards and add the databases these baseline runs create to the PRE-EXISTING list, because the current harness does not clean them up.
- [x] `TASK-002` — Add `-NoBuild` to `scripts/start-invitation-scenario.ps1`, with no other behavior change. Add `scripts/remove-scenario-database.ps1` with the safeguards above and its negative checks.
- [x] `TASK-003` — Add the isolated-scenario helper under `e2e/fixtures/`: name generation, run manifest, API start and health wait, process-tree stop, port check, guarded cleanup, retention on failure, and exposing the database name to `recordScenario`.
- [x] `TASK-004` — Update `e2e/playwright.config.ts`: global setup (build the API once, create the run manifest), global teardown (report retained databases), Angular-only `webServer`, and removal of `isInvitationOnlyRun`.
- [x] `TASK-005` — Update the four specs to declare their scenario through the helper (Adult C for Concern, Incident, and Trip-audience; none for invitation). No assertion changes.
- [x] `TASK-006` — Document the mechanism (`docs/local-development.md`).
- [x] `TASK-007` — Verify AC-01 to AC-13, including both orderings, three consecutive full runs, the negative deletion checks, and the controlled failure. Write `VER-WI-009-001` and set this item to `VERIFYING / OWNER REVIEW`.

## Expected files

| File | Change |
|---|---|
| `src/FamilySafety.Web/e2e/playwright.config.ts` | Global setup and teardown; Angular-only `webServer`; branching removed |
| `src/FamilySafety.Web/e2e/fixtures/invitation-scenario.ts` | Per-spec database name instead of the global environment value |
| `src/FamilySafety.Web/e2e/fixtures/isolated-scenario.ts` (new; name may differ) | The helper |
| `src/FamilySafety.Web/e2e/global-setup.ts`, `global-teardown.ts` (new; may be merged) | Build once; manifest; retained-database report |
| `src/FamilySafety.Web/e2e/*.spec.ts` (4) | Scenario declaration only |
| `scripts/start-invitation-scenario.ps1` | `-NoBuild` switch |
| `scripts/remove-scenario-database.ps1` (new) | Guarded removal |
| `docs/local-development.md` | How to use isolated scenarios |
| This story and `planning/verification/VER-WI-009-001.md` (new) | Records |

Projects affected: the Playwright end-to-end project inside `src/FamilySafety.Web` (not the Angular application), the repository `scripts/`, and `docs/`. No .NET project changes.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md` | Mandatory operating rules | Entry |
| `ai-context/project-status.md` | Continuation checkpoint | Entry |
| This work item | Scope, design, acceptance | Entry |
| `planning/proposals/PRP-030-phase-9-scope-themes-and-exit-criteria.md` (M1, exit criterion 1) | Why the item exists and what it serves | Before execution |
| `src/FamilySafety.Web/e2e/` (config, fixtures, four specs) | The code to change | Before execution |
| `scripts/start-invitation-scenario.ps1`, `src/FamilySafety.Api/Development/DevelopmentFixtureRegistration.cs` (read-only) | How a scenario starts, migrates, and seeds; the guards to preserve | Before execution |
| `docs/local-development.md` | Existing scenario documentation to extend | Before execution |
| `claude-handoff/VALIDATION.md` | Test and build commands | Before verification |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Core contracts and workflows | No product rule is touched; assertions keep their meaning. |
| Phase 8 stories and proposals | The specs themselves are the reference for what they check. |
| `MOCKUP-ALIGNMENT.md` and the reference images | No UI work. |
| Bootstrap and archived material | Not normal context. |

## Stop conditions

Stop and report back if:

- isolation would need a change to API, fixture, identity, or outbox code, or to any product behavior;
- an existing assertion would have to change meaning to pass;
- a safe automated database removal cannot be achieved with the safeguards above;
- the suite cannot pass reliably without raising timeouts or budgets;
- a spec turns out to depend on state created by another spec.

## DEC-032 review

`DEC-032` allows standing approval only for `ROUTINE` or `STANDARD` work that is complete, unambiguous, local, reversible, and wholly within approved scope. It excludes, among others, **destructive** actions.

- In favor: this is `STANDARD` test infrastructure, local, within the approved PRP-030 scope, with complete acceptance checks, and it changes no product behavior.
- Against, and decisive: the item **introduces automated `DROP DATABASE` on the developer's SQL Server**, where none exists today. Deleting a database is destructive and not reversible. That falls in `DEC-032`'s explicit carve-out, and `README.md` likewise requires explicit owner approval for destructive or difficult-to-reverse actions.
- Also relevant: the item changes the harness that runs the Concern, Incident, Trip-audience, and invitation privacy journeys, so a mistake could hide a privacy regression. That argues for deliberate owner sign-off.

**Conclusion:** `DEC-032` does not apply. Explicit owner approval and execution authorization were therefore required, and were given on 2026-10-02 (below).

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES` — `APPROVED — EXECUTION AUTHORIZED` against `CTX-1.27`
- **Authorized by:** Project owner
- **Authorized at:** 2026-10-02
- **Authorized scope:** exactly this work item's design, boundaries, 14 acceptance checks, and tasks, as constrained by [Owner execution conditions](#owner-execution-conditions). This explicitly includes automated removal of databases **created by the current Playwright run**, only when all ten deletion conditions hold; if any safeguard fails, the database is not dropped. It excludes any product, API, schema, or fixture-code change, any new package, deletion of PRE-EXISTING databases, and the later Phase 9 findings listed there. No further routine approval is needed; stop conditions still apply. Hand back in `VERIFYING / OWNER REVIEW`; do not mark `DONE / PASS`.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| AC-01 to AC-14 | `planning/verification/VER-WI-009-001.md` | `PASS` — owner approved 2026-10-02 |

- **Verification report:** `planning/verification/VER-WI-009-001.md`
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `DONE / PASS` — `OWNER APPROVED` (2026-10-02). All 14 acceptance checks pass.

## Implementation record (2026-10-02)

Implemented as designed; details and evidence are in `VER-WI-009-001`. Small differences from the "Expected files" table, all inside the authorized test-infrastructure scope:

- `scripts/list-scenario-databases.ps1` (new, read-only) lists `FamilySafety*` databases for the run-start snapshot and the name-collision check, because the Node side has no SQL driver and none may be added.
- `scripts/pre-existing-scenario-databases.txt` (new) is the PRE-EXISTING file the cleanup logic reads (80 names). Each run also records its own start snapshot in its manifest.
- Run manifests and per-spec API logs live in `src/FamilySafety.Web/e2e/scenario-runs/<run id>/`, which writes its own `.gitignore`; no repository ignore file was changed.
- `remove-scenario-database.ps1 -RemoveRetained` is the deliberate manual path for a retained database; automatic cleanup cannot remove a `retained` or `cleanup-failed` database.
- `FAMILY_SAFETY_E2E_SPEC_ORDER` (verification only) runs listed files in a fixed order through chained Playwright projects.

Retained diagnostic databases (not removed; their removal needs separate owner authorization): `FamilySafety_Invitation_E2E_408952ea95564bce9ef95c840e77f120` (controlled failure) and `FamilySafety_Invitation_E2E_537e62aafa9f4633a4144dafaad77e6e` (one unexplained Angular connection refusal in the first full run). Nothing was committed or pushed.

## Owner review (2026-10-02)

- **Result:** `PASS`. The project owner accepted the implementation and `VER-WI-009-001` and closed the item as `DONE / PASS` — `OWNER APPROVED`.
- **Accepted as non-blocking (the item is not reopened for these):**
  1. one transient Angular connection refusal in an early full run, not reproduced in nine later runs;
  2. the Windows PID-reuse condition, which the harness now handles without stopping an unrelated process;
  3. retained-database removal originally possible without `-RemoveRetained`, corrected before handoff;
  4. the original over-broad leftover-process check, corrected before handoff;
  5. the Concern and Incident specs still rewrite their evidence screenshots when run — a separate test-evidence follow-up.
- **Retained databases:** the two diagnostic databases above are preserved. Their removal, and any cleanup of older databases, needs separate owner authorization.
- **Closeout:** the verification sequence was not rerun for closeout.
