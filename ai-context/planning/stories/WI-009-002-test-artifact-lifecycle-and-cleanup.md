---
Status: DONE / PASS — OWNER APPROVED
Authority: Project-owner approved and execution-authorized for WI-009-002 only (2026-10-02), against CTX-1.27, with the owner decisions recorded below; planning basis PRP-030 / DEC-074
Context-Baseline: CTX-1.27
Version: 1.0
---

# WI-009-002 — Test Artifact Lifecycle and Cleanup

## Metadata

- **Work Item ID:** `WI-009-002`
- **Status:** `DONE / PASS` — `OWNER APPROVED` (owner review `PASS`, 2026-10-02). All 20 acceptance checks `PASS` in `VER-WI-009-002`. Approved and execution-authorized by the project owner on 2026-10-02 against `CTX-1.27`. See [Owner decisions](#owner-decisions-2026-10-02) and [Execution authorization](#execution-authorization).
- **Phase:** `9 — Integration & UX Refinement`, `CURRENT / IN PROGRESS` (`DEC-073`)
- **Context baseline:** `CTX-1.27`
- **Approved planning basis:** `PRP-030` (`APPROVED`, `DEC-074`), theme 1 "Test reliability foundation". This is a follow-up to M1 (`WI-009-001`). It is not a new MUST item and adds no exit criterion.
- **Requirement references:** none changed. No product rule is touched.
- **Model tier:** `STANDARD`. Test infrastructure only, but it adds a manual database-removal entry point and deletes local files, so the safeguards need care.
- **Phase 9 exit criteria served:** 1 (test reliability), indirectly: routine runs leave no residue, and verification evidence stays stable.
- **Standing auto-approval:** `DEC-032` does **not** apply (see [DEC-032 review](#dec-032-review)). Explicit owner approval and execution authorization were given on 2026-10-02, so no further routine approval is needed to execute it.

## Goal

Finish the lifecycle of artifacts that automated verification creates:

- A normal successful test run cleans up after itself.
- A failed run keeps its diagnostic artifacts in one controlled, discoverable place.
- One read-only command shows every test artifact, and one explicit command removes only artifacts whose ownership is proven.
- The owner never again has to write SQL cleanup commands or search the repository for generated folders.

This is **not** a database redesign. `WI-009-001`'s isolation and deletion safeguards are kept exactly as they are.

## Current state (verified 2026-10-02)

### Databases

- `WI-009-001` works as approved. Each spec file gets a fresh `FamilySafety_Invitation_E2E_<hex>` database. A passing spec's database is dropped by `scripts/remove-scenario-database.ps1`. A failed spec's database is kept and marked `retained`. `FamilySafety`, PRE-EXISTING names, and anything not owned by the current run are refused.
- The local server now holds 16 `FamilySafety*` databases: `FamilySafety` and the 15 uncertain (Class D) databases, all on `scripts/pre-existing-scenario-databases.txt`. No retained Playwright database currently exists.
- The manual removal path exists (`remove-scenario-database.ps1 -RemoveRetained`), but the person must already know the database name and the exact manifest path. Nothing lists retained databases across runs. The only report is the console output of the run that created them.

### Run manifests (`src/FamilySafety.Web/e2e/scenario-runs/`)

- There are 19 run folders, about 15 MB in total, mostly per-spec API logs. They are ignored by a `.gitignore` that `global-setup.ts` writes. **Nothing ever removes them**, so a folder accumulates on every run, including fully successful runs.
- **The manifests no longer match the server.** Three entries say `retained`, but their databases are gone:
  - `…0c1229b7…` (run `20261002T062821-5fe3e530`) and `…f8800e98…` (run `20261002T062330-c886642c`) were kept with `FAMILY_SAFETY_E2E_KEEP_DATABASES=1` and later removed by hand.
  - `…5f538d45…` (run `20261002T063131-c166ad75`) was recorded, but the database was never created, because the run was interrupted (`VER-WI-009-001`).

  The cause: `remove-scenario-database.ps1` does not update the manifest after a manual drop. The two Class B entries were marked `removed` only by the one-time cleanup procedure.
- No manifest records when its run finished. A completed run cannot be told apart from a run that was killed before `global-teardown.ts` ran.
- Records cite 10 of the 19 run IDs, mainly `VER-WI-009-001` and the cleanup inventory. Those records contain the evidence themselves (counts, output, names). The manifests are local, ignored files and are not canonical evidence.

### Generated files and folders

| Location | Written by | Lifecycle today |
|---|---|---|
| `src/FamilySafety.Web/e2e/test-results/` | Playwright `outputDir` (screenshot only on failure, trace retained on failure) | Playwright empties it at the start of each run. Ignored. Currently holds 5 empty folders. |
| `src/FamilySafety.Web/e2e/scenario-runs/` | `WI-009-001` harness | Never cleaned (see above). |
| `src/FamilySafety.Web/dist/` | `npm run build` | Overwritten by each build. Ignored. |
| Repository root `wi075003-build-*`, `wi075004-build*` (deleted 2026-10-02) | Ad hoc `npm run build -- --output-path C:\Dev\blevins\<name>` during WI-075-003/004 verification (`VER-WI-075-004`) | No rule existed. They were removed by hand under the one-time cleanup. |
| `ai-context/planning/verification/evidence/site-mockup-alignment/concern-workflow/` (30 files) | `concern-workflow.spec.ts`, **on every run** | Overwritten. See [Screenshots](#screenshots-and-canonical-evidence). |
| `ai-context/planning/verification/evidence/WI-008-004/` (45 files) | `incident-workflow.spec.ts`, **on every run** | Overwritten. |

- **No rule says where verification builds go.** `VALIDATION.md` § 3 runs `npm run build`, which writes to `dist/`, but earlier sessions chose root-level output paths.
- **Unclassified root file.** The repository root contains `trips`, a 1.6 MB PNG image with no file extension, dated 2026-09-27. It is not source or build output, and it predates the cleanup. This item does **not** touch it; the owner should decide what it is. It shows why the status report should flag unexpected root entries.

### Screenshots and canonical evidence

- Two journeys write their screenshots straight into canonical evidence folders under `ai-context/`, so any routine regression run rewrites approved evidence. During `WI-008-004`, the Concern evidence had to be backed up, restored, and MD5-compared to prove it was unchanged (`evidence/WI-008-004/README.md`). The owner accepted this at `WI-009-001` closeout as a separate test-evidence follow-up (owner review item 5).

## Remaining lifecycle gaps

1. Retained databases cannot be listed across runs. Removal requires the person to know the name and the manifest path.
2. A manual removal leaves the manifest saying `retained`, so the records drift from the server.
3. Run folders accumulate forever, including after fully successful runs.
4. An interrupted run cannot be detected, and its entries stay `starting` or `running`. Neither the automatic nor the manual removal path accepts those statuses.
5. No rule exists for verification build output, and nothing reports stray generated folders.
6. Routine Playwright runs overwrite canonical evidence screenshots.
7. No single place shows what test residue exists.

## Owner decisions (2026-10-02)

Recorded with the owner's approval. They are part of the authorized scope and **override any looser wording elsewhere in this item**.

### D1 — Screenshots: option A approved

- Routine Playwright runs write transient screenshots to `src/FamilySafety.Web/e2e/captures/`.
- Routine test execution must **not** write to, overwrite, or delete anything under `ai-context/`.
- Approved screenshots may be copied into canonical evidence only during a story's verification or closeout, when explicitly needed, and the copy is recorded in that story's `VER` record.
- Existing screenshot assertions keep their meaning, including the 390 px `scrollWidth` checks.
- Existing historical screenshots are not deleted or changed.

### D2 — Cited run folders fail closed

- A run folder that `ai-context/`, a verification record, or documentation references is never removed automatically or by a bulk command. Removing it requires the exact `-RunId <run id>` **and** `-Execute`.
- A run counts as referenced if its run ID **or any database name it records** appears in any searched file. The search covers `ai-context/`, `claude-handoff/`, `codex-handoff/`, `docs/`, `scripts/`, and the root `README.md`.
- **If citation detection is uncertain, the run is protected.** For example, the search fails, a searched file cannot be read, or a searched location is missing. A text-search miss must never make an uncertain run removable without `-RunId`.

### D3 — `FAMILY_SAFETY_E2E_KEEP_RUN_FILES=1` approved

- **Default successful run:** teardown removes the run's disposable folder, but only after every database the run owns is confirmed gone.
- **Flag set:** the run folder and its diagnostic logs are kept.
- **Failed or interrupted run:** its run data is kept by default.
- The flag changes only whether run files are kept. It never changes database cleanup or any database safeguard.

### D4 — Interrupted runs are never deleted automatically

An interrupted database is never deleted because its process is gone, because the run is old, or because teardown did not finish. Neither the harness, the teardown, the status report, nor run-record cleanup ever deletes it. Instead, the tool:

1. reports the interrupted run;
2. shows the manifest ownership evidence;
3. shows whether its database still exists;
4. classifies it as `retained diagnostic / interrupted`;
5. prints an explicit cleanup command with the exact database name;
6. requires `-Execute` for deletion;
7. routes the deletion through the existing guarded `remove-scenario-database.ps1`.

If ownership cannot be proven, it does not delete.

### D5 — Manifest and server reconciliation

The status report distinguishes these states, each with its own label:

- retained database exists;
- manifest says retained, but the database is gone;
- manifest records the name, but creation never completed or the database never existed;
- successful run with no remaining database;
- interrupted run;
- unreadable or invalid manifest;
- database with unknown ownership.

Historical manifests are **not** rewritten to match the server. When a lifecycle state changes through an explicit action (an automatic drop, a manual removal, or marking an interrupted entry `retained` before its removal), the change is recorded as a new current state, and the earlier state is preserved in an appended `history` entry. Recorded fields are never deleted or overwritten silently.

### D6 — One entry point; delegated removal

- `scripts/test-artifacts.ps1` is the developer-facing entry point. Run without options, it is a **read-only status report**.
- Every destructive action requires an exact target selection **and** `-Execute`.
- None of these exists: a "delete all retained" option, wildcard cleanup, prefix-based database deletion, age-only deletion, or automatic deletion of a failed database. The status report never prints a wildcard or bulk database command.
- `test-artifacts.ps1` contains **no** database `DROP` logic. Every database removal goes through `remove-scenario-database.ps1`, with the manifest of the run that owns it, and all `WI-009-001` protections stay in force.

### D7 — Run-record cleanup

A run record may be removed only when **all** of these hold:

- no database it owns still exists (confirmed by a successful live query);
- the run is not active;
- the manifest is readable and internally valid;
- no reference, and no uncertain reference, protects it (D2);
- the conditions in [Run-manifest lifecycle](#3-run-manifest-lifecycle) are met.

A cited run additionally needs the exact `-RunId` and `-Execute`. Manifests still needed for ownership proof are never removed.

### D8 — Generated artifacts

- Approved locations: `src/FamilySafety.Web/e2e/captures/` for transient screenshots, and `src/FamilySafety.Web/dist/verification/<WI-id>/` for a separate verification build.
- Generated cleanup touches **only** the fixed directories this item approves (§ 5). It never deletes source, canonical `ai-context/` evidence, verification records, manifests still needed for ownership proof, or arbitrary root folders.

### D9 — The root `trips` file

- The repository-root file `trips` is **not** deleted or modified in `WI-009-002`.
- It is recorded as `UNCLASSIFIED / OWNER REVIEW REQUIRED`. It is outside this item until its origin and purpose are established.
- No cleanup command may target it. The status report lists it under unexpected root artifacts, with that label and no action.

### D10 — Test cadence

The `VALIDATION.md` policy applies:

- focused tests during implementation;
- the affected journeys and **one** complete-suite run at story completion, because shared harness behavior changes;
- repeated reliability runs only where an acceptance check specifically needs a targeted repetition.

`WI-009-001`'s extended multi-run sequence is not repeated.

## Proposed design

### 1. One developer entry point: `scripts/test-artifacts.ps1`

Windows PowerShell 5.1, the same `System.Data.SqlClient`, Windows integrated security, and `localhost` convention as the existing scripts. No module or package.

| Command | Effect |
|---|---|
| `.\scripts\test-artifacts.ps1` | **Read-only status report** (the default). Changes nothing. |
| `.\scripts\test-artifacts.ps1 -RemoveDatabase <name>[,<name>…]` | Dry run. Shows the ownership proof and the proposed action for each named database. |
| `… -RemoveDatabase <name>[,<name>…] -Execute` | Removes each named database through `remove-scenario-database.ps1 -RemoveRetained`, using its owning manifest. |
| `.\scripts\test-artifacts.ps1 -RemoveRunRecords` | Dry run. Lists the run folders that the [manifest cleanup rules](#3-run-manifest-lifecycle) allow removing. |
| `… -RemoveRunRecords [-RunId <id>[,<id>…]] -Execute` | Removes those run folders. Without `-RunId`, it removes only runs in the "Resolved" state: uncited, with a completed reference search. A cited run, or one whose citation state is uncertain, must be named with its exact `-RunId` (D2). |
| `.\scripts\test-artifacts.ps1 -ClearGenerated` | Dry run. Lists the contents of the fixed transient folders. |
| `… -ClearGenerated -Execute` | Empties only those fixed folders. |

Every destructive form needs `-Execute`. Without it, the script prints what it would do and exits 0. It refuses to act while a run is active (see [Safeguards](#destructive-operation-safeguards)). Exit codes match the existing script: 0 = done or dry run; 2 = refused; 3 = failure.

**The new script never drops a database itself.** It finds the owning manifest and delegates to `remove-scenario-database.ps1`, so all ten `WI-009-001` checks run unchanged. Nothing is duplicated, and nothing can weaken them.

### 2. Small changes to the existing harness

- **`remove-scenario-database.ps1`:** after a confirmed drop, it sets the entry's current `status` to `removed`, adds `removedAt` and `removedBy` (`automatic` or `manual`), and appends a `history` entry `{ at, from, to, by, note }` that preserves the previous status (D5). The write is atomic (temporary file and rename). If the drop succeeds but the manifest write fails, it exits with a new code, 4, and prints the name, so the condition is never silent. No existing check changes.
- **`global-teardown.ts`:** records `completedAt` in the manifest. Then, only if every database the run owns is `removed` **and** confirmed absent by a successful live query, and `FAMILY_SAFETY_E2E_KEEP_RUN_FILES` is not `1`, it deletes the run's own folder (D3). The folder path must be exactly `scenario-runs/<runId>`, with a `runId` that matches the generated format. A run with any failed, retained, or interrupted database keeps its folder. The console summary is printed before the deletion, as today. When teardown marks a still-`running` entry `retained`, as it does today, it now also appends a `history` entry.
- **`isolated-scenario.ts`:** the manifest types gain `completedAt`, `removedAt`, `removedBy`, and a per-entry `history` array. Older manifests without these fields stay valid and are read as they are. The file also gains a `captureDirectory(spec)` helper for transient screenshots. Isolation, ports, process handling, and deletion conditions are unchanged.

### 3. Run-manifest lifecycle

The status report gives every run folder exactly one state:

| State | Condition | Allowed action |
|---|---|---|
| **Active** | No `completedAt`, and a recorded API PID is alive with a command line naming that run's scenario (the existing teardown check), or port 5080 is in use | None. All destructive actions refuse while any run is active. |
| **Interrupted** | No `completedAt`, no live owned process, and at least one entry `starting` or `running` | Report as `retained diagnostic / interrupted` (D4). Its existing databases appear as removal candidates that need an exact name and `-Execute` (§ 4). **Keep the folder** while any of its databases exists. |
| **Holds retained databases** | At least one recorded database still exists on the server | **Keep.** The manifest is the ownership proof. |
| **Resolved, cited or citation uncertain** | Every recorded database is absent, and the run ID or one of its database names is referenced, **or** the reference search was uncertain (D2) | Protected. Removed only with the exact `-RunId` and `-Execute`. |
| **Resolved** | Every recorded database is absent, the manifest is valid, the run is not active, and the reference search completed without a match | Removable with `-RemoveRunRecords -Execute`. |
| **Unreadable or invalid** | Missing, unreadable, or malformed `manifest.json`, or internally inconsistent (for example, a duplicate database name, or an entry without a name or status) | **Keep.** Report it for the owner. Never removed by the tool, even with `-RunId`. |

**Database reconciliation labels (D5).** For each recorded database, the report compares the manifest with a live `sys.databases` query and shows exactly one of these:

| Label | Manifest | Server |
|---|---|---|
| `retained — exists` | `retained` or `cleanup-failed` | Present |
| `interrupted — exists` | `starting` or `running`, in an interrupted run | Present |
| `retained in manifest — absent on server` | `retained`, `cleanup-failed`, or `running`, with no `removedAt` | Absent. The tool does not guess why; removal outside the harness is one possibility. |
| `creation never completed — absent` | `starting` (the API never reported healthy) | Absent |
| `removed — absent` | `removed` | Absent (successful run with no remaining database) |
| `removed in manifest — PRESENT on server` | `removed` | Present. This is an anomaly: the tool reports it and offers no action. |

Server databases recorded in no manifest are reported under **unknown ownership**, or **protected** when they are `FamilySafety` or PRE-EXISTING. Neither group is ever offered for removal.

Rules:

- A manifest is never removed while any database it records still exists. "Absent" requires a successful live query. If SQL Server cannot be reached, every run counts as "holds retained databases", and nothing is removed.
- Successful runs remove their own folder automatically (§ 2, D3). The rules above handle older, failed, and interrupted runs.
- Run folders are local, ignored diagnostics, not canonical evidence. Canonical verification records live under `ai-context/` and are never touched. The citation check (D2) fails closed.
- The status report **never rewrites** a manifest. Stale records are reported as they are. A manifest changes only through an explicit, logged action, and the earlier state is kept in `history` (D5). Removing a resolved folder is what clears the drift.

### 4. Retained-database cleanup

A database is a **removal candidate** only when all of these hold:

1. It exists on the server.
2. Exactly one manifest under `scenario-runs/` records it. If two or more do, the report shows "ambiguous ownership" and offers no action.
3. That entry's status is `retained` or `cleanup-failed`, **or** the entry belongs to an **interrupted** run (status `starting` or `running`).
4. **Interrupted entries (D4):** the dry run shows the manifest evidence, whether the database exists, and the label `retained diagnostic / interrupted`. Only with the exact name **and** `-Execute`, and only after proving the run is inactive (no `completedAt`, no live owned process, port 5080 free), does the tool record the entry as `retained`, with the note `run interrupted; marked retained by test-artifacts.ps1` and a `history` entry keeping the original status. It then delegates the removal. If the delegated removal refuses or fails, the entry stays `retained` and the database stays.
5. `remove-scenario-database.ps1 -RemoveRetained` then applies its own checks: exact pattern, not `FamilySafety`, not PRE-EXISTING, not in the run's start snapshot, recorded exactly once, exists exactly once, no open sessions, and the exact name logged before the drop.

Databases are **selected only by exact name.** `-RemoveDatabase` rejects wildcard characters (`*`, `?`, `%`, `[`, `]`), and there is no "all retained" option. The status report prints a ready-to-copy command with the explicit names instead. Nothing automatic (harness, teardown, status report, or run-record cleanup) ever removes an interrupted or failed database.

### 5. Generated-folder strategy

| Folder | Purpose | Success | Failure | Cleanup |
|---|---|---|---|---|
| `src/FamilySafety.Web/e2e/test-results/` | Playwright failure screenshots and traces | Empty (nothing is written on success) | Kept until the next run, which Playwright empties | `-ClearGenerated` |
| `src/FamilySafety.Web/e2e/captures/<spec>/` (**new**) | Transient journey screenshots (see below) | Overwritten each run | Same | `-ClearGenerated` |
| `src/FamilySafety.Web/e2e/scenario-runs/<run id>/` | Ownership manifest and API logs | Removed automatically by teardown | Kept | `-RemoveRunRecords` (rules above) |
| `src/FamilySafety.Web/dist/` | `npm run build` | Overwritten each build | Same | Not touched by the tool. It is reported only. |
| `src/FamilySafety.Web/dist/verification/<WI-id>/` (**documented rule**) | A separate build copy, when a story needs one | Owner's choice | Same | `-ClearGenerated` |

- **Rule (documented in `VALIDATION.md` § 3 and `docs/local-development.md`):** use `npm run build`. If a separate output folder is genuinely needed, use `--output-path dist/verification/<WI-id>`. Never write build output to the repository root or under `ai-context/`.
- `-ClearGenerated` works only on the three fixed paths marked above. Each is resolved to a full path and must lie inside `src/FamilySafety.Web/`. It never recurses outside them, and it never touches `ai-context/`, `dist/` itself, `node_modules/`, or source.
- `captures/` gets its own `.gitignore`, written the same way `scenario-runs/` does, so no repository ignore file changes.
- **Unclassified root entries:** the status report compares the repository root with a fixed list of expected entries (source folders, records, configuration files, `.vs`, `.claude`, `.config`). Anything else is listed as `unclassified — review manually` and is **never** offered for deletion. Today it would list `trips`, labeled `UNCLASSIFIED / OWNER REVIEW REQUIRED` (D9). `-ClearGenerated` cannot reach any repository-root entry: its only targets are the fixed paths in the table above, each confirmed to lie inside `src/FamilySafety.Web/`.

### 6. Screenshots and canonical evidence — option A (approved, D1)

**Approved: A.** Routine runs write screenshots to `e2e/captures/<spec>/` (transient and ignored). Only during a story's closeout does the verifier copy the approved captures into that story's evidence folder, and the copy is recorded in the `VER` record.

Why A:

- A routine regression must not alter historical verification evidence. Today, every Concern or Incident run rewrites 75 approved files, and protecting them took a backup, a restore, and an MD5 check.
- The fix is small and belongs in this item. Each of the two specs changes only one line, its `evidenceDirectory` constant, which becomes `captureDirectory(...)`. No assertion changes. The 390 px `scrollWidth` checks stay.
- The canonical folders and their README files stay exactly as they are.

Option B (keep the current behavior) was not chosen. It would keep making every regression run a risk to approved evidence.

## Status report (example layout)

```text
TEST ARTIFACT STATUS  (read-only; nothing was changed)            2026-10-02 14:05
Server localhost · 16 FamilySafety* databases · 19 run records · port 5080 free

ACTIVE RUNS
  none

RUNS
  Successful, completed (no remaining database) ... 12
  Failed, holding retained databases ..............  1
  Interrupted ......................................  1
  Unreadable or invalid manifest ...................  0

RETAINED DATABASES (proven run ownership; removed only by exact name + -Execute)
  Run                       Database                                   Spec                Result  Label                              Created (age)     Action
  20261003T101500-ab12cd34  FamilySafety_Invitation_E2E_9f…e1           incident-workflow   failed  retained — exists                  2026-10-03 (2 d)  removable on request
  20261003T090000-77aa0011  FamilySafety_Invitation_E2E_3c…08           concern-workflow    pending retained diagnostic / interrupted  2026-10-03 (2 d)  removable on request

MANIFEST / SERVER MISMATCHES (reported only; manifests are not rewritten)
  20261002T062821-5fe3e530  …0c1229b7…  invitation-handoff  retained in manifest — absent on server
  20261002T063131-c166ad75  …5f538d45…  trip-audience-removal  retained in manifest — absent on server
  (none)                                                    removed in manifest — PRESENT on server

RUN RECORDS
  Resolved, removable .............................  9   (3.9 MB)
  Cited or citation uncertain (protected) ......... 10   (11.0 MB)   exact -RunId + -Execute only
  Holding databases (kept for ownership proof) ....  2
  Unreadable or invalid (kept) ....................  0

PROTECTED DATABASES (never removed)       FamilySafety + 15 PRE-EXISTING
UNKNOWN OWNERSHIP (no action offered)     none

GENERATED FOLDERS (fixed approved locations only)
  e2e/test-results          5 empty folders     clearable
  e2e/captures              0 files             clearable
  dist/verification         absent

UNEXPECTED REPOSITORY-ROOT ARTIFACTS (never deleted)
  trips   (file, 1.6 MB, 2026-09-27)   UNCLASSIFIED / OWNER REVIEW REQUIRED

SAFE NEXT STEPS (dry run first; add -Execute only for what you intend)
  .\scripts\test-artifacts.ps1 -RemoveDatabase FamilySafety_Invitation_E2E_9f…e1
  .\scripts\test-artifacts.ps1 -RemoveDatabase FamilySafety_Invitation_E2E_3c…08
  .\scripts\test-artifacts.ps1 -RemoveRunRecords
```

Full database names are printed. They are shortened here only for layout. Age is information only; it never decides an action.

## Destructive-operation safeguards

All `WI-009-001` safeguards stay in force unchanged. In addition:

1. **Dry run by default.** Every destructive form needs `-Execute`.
2. **Ownership, never pattern.** Candidates come only from manifest records reconciled with `sys.databases`. They are never discovered from a `FamilySafety_*` prefix, a wildcard, or age.
3. **Explicit selection.** Databases are named one by one, and wildcard characters are rejected. A cited or uncertain run folder needs its exact `-RunId`. No "all retained" switch exists, and no bulk or wildcard database command is ever printed.
4. **Single drop path.** `test-artifacts.ps1` contains no `DROP` logic. Every drop goes through `remove-scenario-database.ps1`, with all of its checks (D6).
5. **Always refused:** `FamilySafety`; PRE-EXISTING names; names in a run's start snapshot; names recorded in zero or several manifests; names whose manifest is missing, unreadable, malformed, or internally inconsistent; names not matching the exact pattern.
6. **No action while a run is active**, or if SQL Server cannot be queried. In both cases the tool fails closed.
7. **Interrupted runs are never deleted automatically (D4).** Removal needs the exact name, `-Execute`, proof that no owned process is alive, and the guarded removal path. Marking the entry `retained` is recorded with `history`.
8. **Citation fails closed (D2).** An uncertain reference search protects the run.
9. **File deletion is limited to fixed paths:** `scenario-runs/<runId>` folders whose `runId` matches the generated format, and the three transient folders. Each path is resolved and checked to lie inside `src/FamilySafety.Web/`. `ai-context/`, source, `dist/` itself, verification records, manifests still needed for ownership proof, the root `trips` file, and every other unclassified entry are never deleted.
10. **No automation.** Nothing is scheduled. The harness never deletes a failed or interrupted database. The only automatic deletions are the existing passing-spec drop and the new removal of a fully clean run's own folder (unless `KEEP_RUN_FILES` is set).
11. **No silent rewrites.** The status report is read-only. Manifest changes come only from explicit actions, and the earlier state is preserved in `history` (D5).
12. **Logged.** The exact name or path is printed before each deletion, and absence is confirmed afterwards.

## Boundaries / non-goals

- Test infrastructure and documentation only: `scripts/`, `src/FamilySafety.Web/e2e/` (harness, fixtures, and the two screenshot constants), `docs/`, and `claude-handoff/VALIDATION.md`.
- No product behavior, Angular product UI (`src/FamilySafety.Web/src`), API, domain, application, infrastructure, schema, migration, or `DevelopmentFixtureRegistration` change.
- No change to the meaning of any assertion. No change to scenario isolation, ports, workers, timeouts, or budgets.
- No weakening of any `WI-009-001` deletion condition. `scripts/pre-existing-scenario-databases.txt` is unchanged.
- No new npm, NuGet, or PowerShell module. Pester is not used.
- No deletion of any existing database during execution, beyond the databases this item's own verification runs create.
- No change to existing canonical evidence files or their README files.
- Out of scope: the 15 Class D databases; the root `trips` file (`UNCLASSIFIED / OWNER REVIEW REQUIRED`, D9; not deleted or modified); `.NET` SQL-gated test databases (they are reported as "unknown ownership" if orphaned, with no action); backup and restore; navigation; Phase 9 M2/M3 product work; general machine cleanup.

## Acceptance checks

Updated for the owner decisions. Each check names the decision it proves where one applies.

1. `AC-01` — **Successful cleanup still works.** A passing spec's database is dropped automatically and confirmed absent. Its manifest entry becomes `removed`, with `removedAt`, `removedBy: automatic`, and a `history` entry. `WI-009-001` behavior is otherwise unchanged.
2. `AC-02` — **Failed retention still works.** A controlled failure (a temporary failing spec, removed afterwards) keeps its database, marks it `retained`, and keeps its run folder and API log. A database kept with `FAMILY_SAFETY_E2E_KEEP_DATABASES=1` is also retained. The harness never deletes either one.
3. `AC-03` — **Read-only status.** The default `test-artifacts.ps1` lists every retained database from manifest ownership, with run ID, database, spec, result, retained reason or label, created date and age, and proposed action. It also shows the run groups, protected and unknown databases, run records, generated folders, and unexpected root artifacts. `sys.databases`, every manifest, and the file tree are identical before and after (hash comparison).
4. `AC-04` — **Dry run by default; `-Execute` required.** `-RemoveDatabase`, `-RemoveRunRecords` (with and without `-RunId`), and `-ClearGenerated` without `-Execute` each print the proposed action and change nothing.
5. `AC-05` — **Exact selection, delegated removal.** `-RemoveDatabase <exact name> -Execute` removes only that manifest-owned retained database, by calling `remove-scenario-database.ps1 -RemoveRetained` with its owning manifest. Other retained databases are untouched. Wildcard characters are rejected (exit 2). No "all retained" option exists. `test-artifacts.ps1` contains no `DROP` statement (code review).
6. `AC-06` — **Protected names are refused.** `FamilySafety`; a PRE-EXISTING name (for example `FamilySafety_VisualReview`), even with a hand-made manifest that claims it; and a name in a run's start snapshot are each refused by every command form (exit 2, still present).
7. `AC-07` — **Unproven ownership prevents deletion.** Each of these is refused (exit 2) with nothing dropped: a name with no manifest; a name recorded in two manifests; a missing manifest; an unreadable manifest; a malformed manifest; an internally inconsistent manifest.
8. `AC-08` — **Active or unreachable state is refused.** Destructive commands refuse while a run is active (port 5080 held, or a live owned API process) and when SQL Server cannot be queried.
9. `AC-09` — **Interrupted databases are never deleted automatically (D4).** After a controlled interruption, the status report shows the run as interrupted, with the manifest evidence, the database's existence, the `retained diagnostic / interrupted` label, and an exact-name command. Neither teardown, the next harness run, the status report, nor `-RemoveRunRecords` removes it. Only `-RemoveDatabase <exact name> -Execute` removes it, after proving the run inactive, through the guarded path. Its `history` keeps the original status.
10. `AC-10` — **Reconciliation states are distinct (D5).** Each label in [Run-manifest lifecycle](#3-run-manifest-lifecycle) is shown for a matching case, using real runs where available and temporary manifests otherwise. The status report modifies no manifest. The existing historical manifests are byte-identical before and after verification, except folders removed deliberately under AC-12.
11. `AC-11` — **`KEEP_RUN_FILES` (D3).** After a fully successful run, its `scenario-runs/<runId>/` folder is removed automatically. With `FAMILY_SAFETY_E2E_KEEP_RUN_FILES=1`, it is kept and its database is still dropped. A failed or interrupted run keeps its folder.
12. `AC-12` — **Run records fail closed (D2, D7).** `-RemoveRunRecords -Execute` removes only "Resolved" folders. It keeps:
    - folders whose databases still exist;
    - cited folders, matched by run ID or by database name;
    - folders whose citation search was uncertain (simulated with an unreadable or missing search location);
    - unreadable or invalid manifests;
    - active runs.

    A cited folder is removed only with its exact `-RunId` and `-Execute`.
13. `AC-13` — **Generated cleanup is limited (D8, D9).** `-ClearGenerated -Execute` empties only the three fixed transient paths. A crafted path outside them, or a repository-root entry, cannot be targeted. The root `trips` file is byte-identical before and after verification and is reported as `UNCLASSIFIED / OWNER REVIEW REQUIRED`.
14. `AC-14` — **Routine screenshots no longer overwrite canonical evidence (D1).** Concern and Incident runs write screenshots to `e2e/captures/`. The 75 files in `evidence/site-mockup-alignment/concern-workflow/` and `evidence/WI-008-004/` are byte-identical before and after the full suite run (SHA-256 recorded). Screenshot assertions are unchanged (diff review).
15. `AC-15` — No command of this item creates, changes, or deletes any file under `ai-context/`. Only the records this item writes deliberately change.
16. `AC-16` — **No product behavior changes.** No file under `src/FamilySafety.Api`, `src/FamilySafety.Application`, `src/FamilySafety.Domain`, `src/FamilySafety.Infrastructure`, or `src/FamilySafety.Web/src` changes. Assertion meaning is unchanged (diff review).
17. `AC-17` — **No new dependency:** no npm, NuGet, or PowerShell module, and no Pester.
18. `AC-18` — `docs/local-development.md` and `claude-handoff/VALIDATION.md` explain the routine workflow: see status, remove a retained or interrupted database, clear run records and generated folders, the cited-run rule, `KEEP_RUN_FILES`, the build-output rule, and the screenshot closeout copy.
19. `AC-19` — A repeatable safety script, `scripts/tests/check-test-artifacts-safety.ps1`, exercises the refusal cases of AC-04 to AC-08, AC-12, and AC-13. It uses temporary manifests and a temporary folder, drops nothing, touches no protected database or file, and passes.
20. `AC-20` — **The test cadence is preserved (D10).** Verification uses:
    - the safety script;
    - the targeted single-spec runs these checks need (keep flag, controlled failure, controlled interruption, one passing spec);
    - the Concern and Incident journeys;
    - **one** complete-suite run.

    No repeated reliability sequence is run.

## Tasks

- [x] `TASK-001` — Record a baseline: the database list, the `scenario-runs/` inventory with manifest hashes, SHA-256 hashes of the 75 canonical screenshot files, and the hash of the root `trips` file. Delete nothing.
- [x] `TASK-002` — `remove-scenario-database.ps1`: after a confirmed drop, record `removed`, `removedAt`, `removedBy`, and a `history` entry atomically; exit 4 if the drop succeeded but the manifest write failed. No other change.
- [x] `TASK-003` — Manifest types (`completedAt`, `removedAt`, `removedBy`, `history`) and `captureDirectory(...)` in `isolated-scenario.ts`. In `global-teardown.ts`: `completedAt`, removal of a fully clean run folder, `FAMILY_SAFETY_E2E_KEEP_RUN_FILES`, and `history` when it marks an entry `retained`.
- [x] `TASK-004` — `scripts/test-artifacts.ps1`:
  - the read-only status report with the D5 labels;
  - `-RemoveDatabase` (exact names, wildcard rejection, interrupted-entry handling under D4, delegation only);
  - `-RemoveRunRecords [-RunId]` with fail-closed citation (D2);
  - `-ClearGenerated` limited to the fixed paths;
  - dry run by default, `-Execute` for action, and the active-run and SQL-reachability guards.
- [x] `TASK-005` — Point the Concern and Incident screenshot constants at `captureDirectory(...)`. No assertion changes.
- [x] `TASK-006` — `scripts/tests/check-test-artifacts-safety.ps1`: the negative cases, using temporary manifests in a temporary folder, with no drops.
- [x] `TASK-007` — Documentation: `docs/local-development.md` and `claude-handoff/VALIDATION.md` §§ 2, 3, and 5.
- [x] `TASK-008` — Verify under the D10 cadence:
  - the safety script;
  - one spec with `KEEP_DATABASES=1`, then status, dry run, and `-Execute` removal;
  - one controlled failure (a temporary spec, removed afterwards);
  - one controlled interruption: stop the Playwright process during a spec, stop any leftover API only by its recorded PID, then run status, dry run, and `-Execute` removal;
  - one passing spec, with and without `KEEP_RUN_FILES`;
  - the Concern and Incident journeys;
  - one full suite with before and after hashes.

  Write `VER-WI-009-002` and set this item to `VERIFYING / OWNER REVIEW`. Do not mark it `DONE / PASS`.

## Expected files

| File | Change |
|---|---|
| `scripts/test-artifacts.ps1` | **New.** The status and cleanup entry point |
| `scripts/tests/check-test-artifacts-safety.ps1` | **New.** Repeatable negative checks |
| `scripts/remove-scenario-database.ps1` | Records `removed` in the manifest after a confirmed drop; exit code 4 |
| `src/FamilySafety.Web/e2e/global-teardown.ts` | `completedAt`; removes a fully clean run's folder |
| `src/FamilySafety.Web/e2e/global-setup.ts` | Writes the `captures/.gitignore` (may move into the helper) |
| `src/FamilySafety.Web/e2e/fixtures/isolated-scenario.ts` | Manifest fields; `captureDirectory(...)` |
| `src/FamilySafety.Web/e2e/concern-workflow.spec.ts`, `incident-workflow.spec.ts` | Screenshot directory constant only |
| `docs/local-development.md`, `claude-handoff/VALIDATION.md` | Workflow documentation |
| This item, `planning/verification/VER-WI-009-002.md` (new), `project-status.md` | Records |

Unchanged: `scripts/list-scenario-databases.ps1`, `scripts/start-invitation-scenario.ps1`, `scripts/pre-existing-scenario-databases.txt`, `playwright.config.ts`, the other two specs, all product code, and all existing evidence.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md` | Mandatory operating rules | Entry |
| `ai-context/project-status.md` | Continuation checkpoint | Entry |
| This work item | Scope, design, acceptance | Entry |
| `planning/stories/WI-009-001-playwright-per-spec-scenario-isolation.md` ("Owner execution conditions" and "Owner review") | The safeguards that must not weaken; the screenshot follow-up | Before execution |
| `planning/verification/evidence/WI-009-001/database-cleanup-completion-2026-10-02.md` | Current protected-database state | Before execution |
| `scripts/` (all four files) and `src/FamilySafety.Web/e2e/` (config, fixtures, setup and teardown, Concern and Incident specs) | The code to change | Before execution |
| `docs/local-development.md` § "Isolated Playwright scenarios", `claude-handoff/VALIDATION.md` | Documentation to extend; test cadence | Before execution |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Core contracts, workflows, `PRP-030` body | No product rule is touched. The theme 1 link is recorded above. |
| Phase 8 stories and verification | Only the two screenshot paths matter; the specs show them. |
| `MOCKUP-ALIGNMENT.md` and the reference images | No UI work. |
| The full `VER-WI-009-001` and the cleanup inventory | Their conclusions are summarized here. Load them only if a safeguard question arises. |
| Bootstrap and archive | Not normal context. |

## Stop conditions

Stop and report if:

- any `WI-009-001` deletion condition would have to be relaxed;
- a database cannot be removed without bypassing `remove-scenario-database.ps1`;
- a change is needed to product code, the API fixture, ports, workers, timeouts, or assertion meaning;
- an existing canonical evidence file would change;
- a new dependency appears necessary;
- an owner decision (D1–D10) cannot be met as written;
- the root `trips` file, a Class D database, or any other out-of-scope artifact would be touched.

## DEC-032 review

- **In favor:** `STANDARD`, local, test infrastructure, within `PRP-030` theme 1, with complete acceptance checks and no product change.
- **Against, and decisive:** it adds a developer command that **drops databases** and deletes local folders. Those actions are destructive and cannot be undone, which is `DEC-032`'s explicit carve-out. It also changes how verification evidence is produced.

**Conclusion:** `DEC-032` does not apply. Explicit owner approval and execution authorization were therefore required, and were given on 2026-10-02 (below).

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES` — `APPROVED — EXECUTION AUTHORIZED` against `CTX-1.27`
- **Authorized by:** Project owner
- **Authorized at:** 2026-10-02
- **Authorized scope:** exactly this work item's design, boundaries, 20 acceptance checks, and 8 tasks, as constrained by [Owner decisions](#owner-decisions-2026-10-02) D1–D10.
  - **Included:** a developer command that removes manifest-owned retained or interrupted databases, only by exact name with `-Execute` and only through `remove-scenario-database.ps1`; removal of run folders and fixed transient folders under the stated rules; automatic removal of a fully successful run's own folder; and removal of the databases and temporary files this item's own verification runs create.
  - **Excluded:** any product, API, schema, migration, or fixture-code change; any new dependency; any weakening of a `WI-009-001` safeguard; deletion of `FamilySafety`, PRE-EXISTING, Class D, or unknown-ownership databases; deletion or modification of the root `trips` file or of any existing canonical evidence.
  - **Process:** no further routine approval is needed, and the stop conditions still apply. Hand back in `VERIFYING / OWNER REVIEW`; do not mark `DONE / PASS`. Do not commit or push.
- **Implementation started:** `YES` (2026-10-02). Handed back in `VERIFYING / OWNER REVIEW`; closed `DONE / PASS` — `OWNER APPROVED` on 2026-10-02 (`VER-WI-009-002`).

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| AC-01 to AC-20 | `planning/verification/VER-WI-009-002.md` | `PASS` (owner review `PASS`, 2026-10-02) |

- **Verification report:** `planning/verification/VER-WI-009-002.md`
- **Independence:** `SELF-VERIFIED`, followed by project-owner review (`PASS`, 2026-10-02)
- **Outcome:** `DONE / PASS` — `OWNER APPROVED` (2026-10-02). 20/20 acceptance checks `PASS`. See [Owner review](#owner-review-2026-10-02).

## Owner review (2026-10-02)

- **Result:** `PASS`. The project owner accepted the implementation and verification in `VER-WI-009-002`, and the item is closed `DONE / PASS` — `OWNER APPROVED`.
- **Accepted implementation fixes:** the six corrections found during verification are part of the accepted implementation. They are the Windows PowerShell 5.1 status-parsing fix, the destructive-action dispatch fix, the ownership-lookup fix, unknown-option rejection, the manifest update after a drop, and the encoding fix.
- **Accepted small additions:** the `captureDirectory` re-export in `invitation-scenario.ts`, and the `runnerPid` manifest field.
- **Accepted non-blocking issues:** these do not reopen this item, and no work item is created for them.
  1. An orphaned API is detected safely through the port-5080 check, but the status report may not name the run that owns it.
  2. Citation detection may over-protect run folders.
- **Early manifest-drift evidence is preserved.** Verification run `20261002T225019-49bd4b30` dropped its database before the manifest-write fix, so its manifest still says `retained`. It is not rewritten. The final implementation contains the fix.
- **Post-review cleanup (2026-10-02):** done with `scripts/test-artifacts.ps1` only, as recorded in `VER-WI-009-002`. It removed the 9 run folders classified as Resolved and emptied the transient folders. No database was deleted, and the protected and interrupted run folders were kept.
