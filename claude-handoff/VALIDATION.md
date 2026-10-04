# Validation

Run the sections that match the current work item before handing back. Record exact commands, counts, and warnings. Commands and paths come from the root `README.md`, `VER-WI-008-001.md`, `VER-WI-008-002.md`, and `e2e/playwright.config.ts`.

- **Server work (for example WI-008-003, now `DONE / PASS`):** § 4 is required, with the SQL gate on. Sections 1–3 and 5–6 are not required, because WI-008-003 makes no Web changes. State that explicitly rather than implying they ran.
- **UI work:** §§ 1–3 and 5–6; § 4 only if server code was touched.

## Test cadence

A work item's own acceptance checks always take precedence over this guidance; never run less than the story requires.

- **During implementation:** run the focused unit, component, or integration tests for the area being changed, and only the directly affected Playwright journeys, as needed.
- **Story completion:** run the required affected Playwright regressions. Run the complete Playwright suite (`npm run test:e2e`) **once** when the story changes integrated product behavior or shared infrastructure.
- **Infrastructure, phase, or release gates:** repeated full-suite reliability runs (for example default order, reversed order, and three consecutive passes) are used only when a work item or gate specifically requires them. That repeated sequence was specific to `WI-009-001` proving the harness; it is not the default for every story.

## 1. Angular component tests (includes Concern tests)

```powershell
Set-Location C:\Dev\blevins\src\FamilySafety.Web
npm test -- --watch=false
```

Baseline before alignment: **6 files, 66 tests passed.** Report the new count. If you updated `concerns.spec.ts` for markup changes, say which assertions changed and confirm that behavior coverage was kept: audience, two-step create, correction/challenge linkage, Outdated mapping, creator-only controls, Frozen/Resolved read-only, and unavailable handling.

## 2. Playwright journeys (isolated per-spec scenarios, `WI-009-001`)

```powershell
Set-Location C:\Dev\blevins\src\FamilySafety.Web
npx playwright test --config e2e/playwright.config.ts e2e/concern-workflow.spec.ts   # one affected journey
npm run test:e2e                                                                      # complete suite
```

**Ports `4200` and `5080` must be free.** Stop any dev servers you started first. If another process holds `5080`, setup fails; the harness never stops a process it did not start.

How the harness works (details in `docs/local-development.md` § "Isolated Playwright scenarios"):

- **Isolated scenario per spec file.** Each spec declares its scenario with `useIsolatedScenario({ tripAudienceAdult })` and gets its own fresh `FamilySafety_Invitation_E2E_<hex>` database and its own API process on `:5080`. Results do not depend on which specs run or in what order. A new spec must call the helper once at the top of the file.
- **Shared Angular server.** One `ng serve` on `:4200` serves the whole run. The API is built once per run, and each spec starts it with `-NoBuild`.
- **Safe cleanup on success.** After a spec file passes, its API is stopped, the port is confirmed free, and `scripts/remove-scenario-database.ps1` drops that one database, owned by the current run, and confirms it is gone.
- **Retention on failure.** A failed spec's database is kept, marked `retained` in `e2e/scenario-runs/<run id>/manifest.json`, and printed with its removal command. Cleanup is never retried automatically. `FAMILY_SAFETY_E2E_KEEP_DATABASES=1` keeps every database.
- **Run files (`WI-009-002`).** A fully successful run deletes its own `scenario-runs/<run id>/` folder after its databases are confirmed gone. Failed and interrupted runs keep theirs. `FAMILY_SAFETY_E2E_KEEP_RUN_FILES=1` keeps a successful run's folder and API logs; it never changes database cleanup.
- **Screenshots (`WI-009-002`).** The Concern and Incident journeys write to `src/FamilySafety.Web/e2e/captures/<spec>/` (ignored, overwritten each run). Routine runs never write under `ai-context/`.
- **Pre-existing protection.** Databases listed in `scripts/pre-existing-scenario-databases.txt`, or present when the run started, are never removed. Do not delete databases by name pattern, by hand, or with ad hoc SQL.
- **Leftovers: use `.\scripts\test-artifacts.ps1`** from the repository root. Without options it is a read-only status report of retained and interrupted databases, manifest/server mismatches, run records, generated folders, and unexpected root entries. Removal is a dry run unless `-Execute` is added: `-RemoveDatabase <exact name>` (delegates to `remove-scenario-database.ps1`; interrupted entries are recorded `retained` first), `-RemoveRunRecords` (only uncited "Resolved" folders; a cited or protected folder needs its exact `-RunId`), and `-ClearGenerated` (only `e2e/test-results/`, `e2e/captures/`, `dist/verification/`). Details: `docs/local-development.md` § "Test artifacts: status and cleanup".
- **What to record:** the command, pass/fail counts, any `RETAINED DATABASE` line, the end-of-run summary (`… created, … removed, … retained` and `Port 5080 free: true. FamilySafety.Api processes remaining: 0.`), and whether the run files were removed or kept.

Which journeys to run follows the [Test cadence](#test-cadence) above. For example, the Concern journey is: Adult A creates a Concern about B and selects C; B sees only empty or unavailable; C contributes; A resolves; C sees read-only history.

`FAMILY_SAFETY_E2E_SPEC_ORDER` (a comma-separated list of spec names) forces a file order. Use it only when a work item requires an order-independence check.

## 3. Production build

```powershell
Set-Location C:\Dev\blevins\src\FamilySafety.Web
npm run build
```

**Build output location (`WI-009-002`).** Use `npm run build`, which writes `dist/`. If a story needs a separate build copy, use `npm run build -- --output-path dist/verification/<WI-id>`. Never write build output to the repository root or under `ai-context/`.

Must succeed. Report all budget warnings with sizes. Baseline: `concerns.scss` 6.93 kB / 4.00 kB warning; initial bundle 522.11 kB / 500 kB; existing `app.scss` and `trips.scss` warnings. Do not change thresholds. Note whether `concerns.scss` grew or shrank.

## 4. .NET build and tests (required for server work such as WI-008-003)

```powershell
Set-Location C:\Dev\blevins
dotnet build FamilySafety.slnx
$env:RUN_FAMILYSAFETY_SQL_TESTS = '1'
dotnet test FamilySafety.slnx --no-build
```

- **SQL gate.** SQL-gated tests (for example `ConcernPersistenceTests` and the new Incident persistence tests) **return early and report as passed** unless `RUN_FAMILYSAFETY_SQL_TESTS=1`. A run without the gate cannot establish `PASS` for SQL-sensitive acceptance checks.
- **Test databases.** The tests connect to the local default SQL Server instance (`Server=localhost`, Windows authentication). They create a disposable database per test, apply all EF migrations, and delete it afterward.
- **Record the evidence.** Write down the exact command with the variable set, total/passed/failed/skipped counts, and evidence that the Incident SQL tests executed (for example, their names in the run output or `--logger "console;verbosity=normal"`). Reference: VER-WI-008-001 recorded 144 passed, 0 failed, 0 skipped with the gate on.
- **Regression.** The full foundation suite, including the Family, Event, Plan, Trip, and Concern tests, must pass.

For UI-only work that did not touch server code, state "No server changes; .NET suite not rerun" rather than implying it ran.

## 5. Rendered review — desktop and narrow

Capture **every** Concern state at **1440 px** and **390 px** width:

- list populated, list empty
- create (with audience selection visible)
- Open detail (header, participants, history with at least one correction and one challenge)
- Observation / Unknown composer, correction composer, challenge composer
- creator participant management and resolve form
- Frozen, Resolved
- unavailable, generic list error, loading if practical
- the Home Concern entry card

Rules:

- Use the running development scenario for real states. Frozen and generic error may use browser response interception shaped from real API responses, as before. Label those captures as mocked in the VER record.
- At 390 px, assert `document.documentElement.scrollWidth <= 390` (no horizontal overflow).
- Save new captures to `ai-context/planning/verification/evidence/WI-008-002/` with a distinguishing suffix (for example `concern-open-desktop-aligned.png`). Do not overwrite the earlier evidence.
- **Playwright journey screenshots (`WI-009-002`).** Routine runs write them to `src/FamilySafety.Web/e2e/captures/<spec>/`, never to `ai-context/`. Copy approved captures into the story's evidence folder only during that story's verification or closeout, when needed, and record the copied files and their SHA-256 in the `VER` record. Never overwrite existing canonical evidence.
- The Development test-identity banner will appear in captures. That is expected; note it.

## 6. Accessibility review (focused)

- Heading order is logical after restructuring (one page `h1`, sections/cards in order).
- Every control is labeled; participant and category choices use fieldset/legend.
- Validation messages are associated with fields; `role="status"` / `role="alert"` announcements still fire.
- Keyboard: everything is reachable and operable, focus is visible (3 px focus style), and focus lands logically after submit, open/close composer, and unavailable routing.
- Status and category are never conveyed by color alone; pills contain text.
- Contrast holds for any new token combinations; enabled buttons are about 44 px or taller.
- Narrow reading order matches visual order, and timeline/history stays single-column and readable.

No automated accessibility scanner is configured. State that the review is manual or code-based.

## 7. Mockup comparison (required — not just "it renders")

Rendered evidence must show fidelity, not only that pages load. For each state, compare the new capture against its mapped file in **`claude-handoff/reference/`** (mapping in `MOCKUP-ALIGNMENT.md`), not against earlier evidence, and report:

| State | **Now matches the mockup** | **Still differs** | **Why** |
|---|---|---|---|

Valid "why" reasons:

- canonical behavior or copy requirement (cite the AC or REQ)
- DROP item (unsupported functionality)
- owner decision (palette, shell, navigation), citing which one
- no API data
- accessibility
- deferred for owner direction

Also report an overall judgment: is the running UI now **materially** closer to the mockup? Name the three biggest remaining gaps.
