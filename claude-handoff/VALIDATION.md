# Validation

Validation depth follows the authorized item's acceptance checks, change scope, and risk. Do not run checks unrelated to the change merely to fill a template.

## Test cadence

- During implementation, use focused unit/component/integration tests and affected journeys as needed.
- Near feature completion, run one meaningful full regression when integrated behavior or shared infrastructure is affected. Major features may need broad regression; documentation, CSS tweaks, and narrow fixes generally need focused checks only.
- Repeat a full suite, build, capture, or evidence cycle only after a material change, failure, unresolved risk, or explicit acceptance/gate requirement. Harness order/reliability experiments are not routine feature checks.
- Documentation-only work: check Markdown/local references, search changed current-facing files for stale state, and run git diff --check. No application tests, builds, screenshots, or axe scans.
- Required acceptance checks still apply. Record actual commands/results and meaningful limitations; do not imply unrun tests passed or repeatedly regenerate evidence.

## Commands — select only what applies

From src/FamilySafety.Web:

~~~powershell
npm test -- --watch=false
npx playwright test --config e2e/playwright.config.ts e2e/<affected-spec>.spec.ts
npm run test:e2e
npm run build
~~~

From the repository root, for relevant server validation:

~~~powershell
dotnet build FamilySafety.slnx
$env:RUN_FAMILYSAFETY_SQL_TESTS = '1'
dotnet test FamilySafety.slnx --no-build
~~~

SQL-gated tests return early and can report passed without RUN_FAMILYSAFETY_SQL_TESTS=1. SQL-sensitive acceptance requires the gate and evidence that the tests executed. Tests use disposable databases on local SQL Server; never substitute or reset a user database.

## Operational safeguards

- Playwright uses the existing per-spec isolated harness. Ports 4200 and 5080 must be free; stop only processes you started. Failed scenario databases are retained. Do not delete databases by pattern or ad hoc SQL.
- Harness setup, retention variables, cleanup and leftover reporting are documented in [local development](../docs/local-development.md). Use scripts/test-artifacts.ps1 for guarded cleanup; report any retained databases or cleanup failure.
- Routine captures stay under Web/e2e/captures/; builds stay under Web/dist/. Preserve canonical evidence. Copy captures into a work item's evidence folder only when required, with distinct names and the required provenance/hash.
- Report actual build warnings; compare with the relevant baseline and do not raise thresholds.

## UI review when warranted

Inspect affected states at desktop and narrow widths. Check overflow, labels, heading order, keyboard/focus, announcements, and non-color status cues as relevant. For visual alignment, compare against the mapped [primary reference](MOCKUP-ALIGNMENT.md), explaining material differences. Capture only evidence needed for acceptance; exhaustive screenshots, transcripts, and per-state comparison tables are not default requirements.

Label mocked/intercepted states. Distinguish automated accessibility results from actual manual/keyboard review, and do not infer owner visual approval.
