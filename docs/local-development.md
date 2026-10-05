# Run the application locally

These instructions describe the current local Development configuration. They do not configure production services.

## Prerequisites

- Windows with the .NET 10 SDK. The repository pins SDK `10.0.401` in `global.json`.
- Node.js 24 LTS and npm 12 for the Angular client.
- A running local SQL Server default instance reachable at `localhost`, with Windows Integrated Authentication enabled.
- Permission for the Windows account running the API to create the `FamilySafety` database if necessary and apply its migrations.

Docker is not required; the repository has no Dockerfile or Compose setup.

## Database and development configuration

The API reads `src/FamilySafety.Api/appsettings.Development.json` when `ASPNETCORE_ENVIRONMENT` is `Development`. Its default connection string is:

```text
Server=localhost;Database=FamilySafety;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

If SQL Server uses another instance or credentials, override the connection string in the API terminal before starting it. For example, for a local SQL Express instance:

```powershell
$env:ConnectionStrings__FamilySafety = 'Server=.\SQLEXPRESS;Database=FamilySafety;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true'
```

In Development, the API's fixture initializer automatically applies pending EF Core migrations at startup, then seeds a `Development Family` group, a development adult account/profile, a second adult account/profile, active memberships for both, and an administrator assignment for the development adult. It does not seed Trips, Plans, or Events. The first start may therefore create/update the local database and add these fixture records.

The default development current-account adapter selects the seeded development adult automatically. The opt-in invitation scenario below adds a clearly labeled Development account switcher. The Development identity, fixtures, switcher, and invitation outbox are guarded so they cannot be enabled outside the Development environment; they are local conveniences, not production authentication or invitation delivery.

No extra application environment variables are required for the defaults below. `ConnectionStrings__FamilySafety` is the supported override when the SQL Server connection differs.

## Start from VS Code

Open two PowerShell terminals in the repository root, `C:\Dev\blevins`.

### Terminal 1: API

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = 'http://localhost:5080'
dotnet run --project .\src\FamilySafety.Api\FamilySafety.Api.csproj --no-launch-profile
```

Wait for startup, including migration and fixture initialization. Check the API at [http://localhost:5080/health](http://localhost:5080/health).

The Angular development proxy in `src/FamilySafety.Web/proxy.conf.json` targets port `5080`. The API launch profiles use ports `5162` and `7299`, so use the command above when running the client with its current proxy configuration.

### Terminal 2: Angular

```powershell
Set-Location C:\Dev\blevins\src\FamilySafety.Web
npm start
```

Open [http://localhost:4200](http://localhost:4200). The Angular dev server proxies `/api` calls to the API at port `5080`.

If `npm start` reports that packages are missing, `npm ci` restores packages from the committed lock file. Do not run an install or upgrade if you have not approved dependency changes for your session.

## Screens and features available

The current UI includes:

- Home dashboard, six-area navigation, and refresh-safe item links.
- Adult profile display and editing, including profile-field visibility.
- Family dashboard, Family Settings, adult relationships, and invitations.
- Events dashboard, creation, list/detail viewing, and authorized detail editing.
- Plan creation and viewing with audience selection and optional notes; the Plan's creator can edit or clear its note from the Plan page.
- Trip creation and viewing, creator editing of core Trip details, and collaborative Trip Notes.
- Safety dashboard and separate private Concern and Safety Incident workflows; standalone Check-Ins with optional plain-text note and explicit same-Family recipients. Check-Ins have immutable sent/shared history, no location or safety inference.

New local databases start with the development Family Group and two adult members but no sample Events, Plans, or Trips. Create records in the UI to explore those features.

Phase 9 is `COMPLETE / CLOSED` under `DEC-081`; Phase 9.5 is `CURRENT / IN PROGRESS`; WI-095-005 is the bounded standalone Check-In item under CTX-1.32. Creator-controlled Trip audience editing is available. Trip Check-In and Sharing Session execution are not available in this local build.

## Invited-adult scenario (WI-075-001)

Stop any API already using port 5080. From the project root in a new PowerShell terminal, run:

```powershell
.\scripts\start-invitation-scenario.ps1
```

This starts the API in Development using a new isolated `FamilySafety_Invitation_<unique name>` database. It applies the existing migrations and seeds Adult A as the active Family administrator and Adult B as an existing Account/Profile with no current or former membership. It leaves the ordinary `FamilySafety` database alone. No new schema or migration is introduced by this scenario.

Start Angular with `npm start` as above and open **http://localhost:4200** in one tab. Use that host consistently so opening an outbox link remains in the same page session.

1. Confirm **Active test identity: Adult A — Family administrator** in the Development banner.
2. Open **Family**, then choose the **Family settings** gear. In **Invite an adult family member**, enter `development-recipient@example.invalid` as recipient email and choose **Send or resend invitation**.
3. Choose **Show Development invitation links** to inspect the in-memory outbox. This is local test delivery, not a sent production email.
4. Select **Adult B — invited adult**. The previous workspace and its pending reads are discarded. Confirm Home says there is no active Family.
5. Choose **Show Development invitation links** again, then **Open invitation** for the message. Switching clears the prior outbox display as well as user data.
6. Confirm the Family and inviter names, then choose **Accept invitation**. Membership and dependent Family data reload automatically. If loading fails after acceptance, use **Retry loading family**; do not accept the consumed token again.
7. Review the existing profile and privacy choices. Save changes if desired, or choose **Continue to Home without changes**. Review is optional and does not gate membership.
8. Confirm arrival on **Home** with the one-time joined-Family announcement. Open **Family** to confirm the directory includes A and B, then open **Family settings** to confirm the existing relationship controls are available.

For a fresh test, stop the scenario API and run the script again without a scenario name. Each run gets a fresh database; the script never drops a database. To resume an existing scenario, use `-ScenarioName <the name printed by the script>`. B remains a member after acceptance in that database. The outbox is in memory and is cleared by API restart; resend as A if a pending link is needed. A new scenario can also exercise Decline and used/unavailable links.

The script accepts `-Server <SQL instance>` and sets process-local Development flags. Close its terminal before starting ordinary Development work, or use a fresh terminal. Scenario databases are disposable test artifacts but are retained by this script; any later cleanup should target only the explicitly identified scenario database.

The switcher uses a request-scoped `X-Development-Account` header, limited to the two fixture Account IDs. It is opt-in through `DevelopmentIdentity:AllowSwitching`, with a Development-only identities endpoint. The browser shows it only in a Development build when that endpoint is available. Invalid identity selections do not fall back to a privileged identity. Production ignores this header and cannot enable the adapter/switcher/fixtures/outbox. This mechanism makes no production identity or recipient-email ownership claim.

Home is the current dashboard. The six-area navigation and item routes approved in Phase 9 are available at desktop and narrow widths. See `ai-context/project-status.md` for the current scope and remaining follow-ups.

## Isolated Playwright scenarios (WI-009-001)

Run the end-to-end suite from `src/FamilySafety.Web` with ports `4200` and `5080` free:

```powershell
npm run test:e2e                                                        # all specs
npx playwright test --config e2e/playwright.config.ts e2e/<name>.spec.ts  # one spec
```

**How it works.** Each spec file gets its own fresh database and its own API process, so results do not depend on which specs run or in what order:

1. `e2e/global-setup.ts` records every `FamilySafety*` database that already exists, writes the run's ownership manifest to `e2e/scenario-runs/<run id>/manifest.json`, and builds the API once.
2. For each spec file, `useIsolatedScenario(...)` (`e2e/fixtures/isolated-scenario.ts`) checks that port `5080` is free, records a new `FamilySafety_Invitation_E2E_<32 hex characters>` name in the manifest, starts the API with `scripts/start-invitation-scenario.ps1 -NoBuild`, and waits for `/health`. The API migrates and seeds the new database at startup. If another process holds port `5080`, setup fails; the harness never stops a process it did not start.
3. After the file's tests, the API process tree is stopped and the port is confirmed free before anything else happens.
4. The Angular dev server is started once by Playwright and shared by all specs.

**Declaring a scenario in a new spec.** Call the helper once at the top of the file, after the imports:

```ts
import { recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({ tripAudienceAdult: true });
```

Adults A (administrator) and B (an account with no membership) are always seeded. `tripAudienceAdult: true` also seeds Adult C as an active Family member. `secondFamily: true` (WI-009-005; off by default) also seeds "Development Second Family" with fixed IDs, where A and B are active members and C is not; it seeds no items. Tests in one file share that file's scenario. Call `recordScenario(test.info())` in a test to attach the database name to the report.

**Cleanup.** When every test in a spec file passes, its database is dropped by `scripts/remove-scenario-database.ps1` and confirmed gone. That script drops exactly one database and only when all of these hold: the name matches `^FamilySafety_Invitation_E2E_[A-Za-z0-9]+$`, it is not `FamilySafety`, it is not in `scripts/pre-existing-scenario-databases.txt` or the run's start snapshot, the run manifest records it exactly once, and the spec passed. It logs the exact name before dropping and exits non-zero (nothing dropped) if any check fails. It uses Windows integrated security and stores no credentials.

**Retention.** When a spec fails, its API is stopped but its database is kept, marked `retained` in the manifest, and printed in the output with the command to remove it. Cleanup is never retried automatically. Set `$env:FAMILY_SAFETY_E2E_KEEP_DATABASES = '1'` to keep every database from a run.

**Run files.** A fully successful run deletes its own `e2e/scenario-runs/<run id>/` folder at the end, but only after every database it created is recorded `removed` and confirmed absent by a live query. A failed or interrupted run keeps its folder (the manifest is the ownership proof, and the `*.api.log` files are the diagnostics). Set `$env:FAMILY_SAFETY_E2E_KEEP_RUN_FILES = '1'` to keep a successful run's folder and logs too; the flag never changes database cleanup.

**Screenshots.** The Concern and Incident journeys write their screenshots to `e2e/captures/<spec>/`, which is ignored by source control and overwritten by each run. Routine runs never write under `ai-context/`. When a story's verification needs rendered evidence, copy the approved captures into that story's evidence folder during closeout and record the copy (files and SHA-256) in its `VER` record.

## Test artifacts: status and cleanup (WI-009-002)

One read-only command shows every artifact that test runs leave behind. Run it from the repository root:

```powershell
.\scripts\test-artifacts.ps1
```

It changes nothing. It lists active and interrupted runs, retained databases (with run, spec, result, label, age, and whether removal is allowed), manifest/server mismatches, run records by state, protected (`FamilySafety` and PRE-EXISTING) and unknown-ownership databases, the generated folders, unexpected repository-root entries (the root `trips` file is listed as `UNCLASSIFIED / OWNER REVIEW REQUIRED` and is never a target), and copy-ready next steps.

Every destructive form is a dry run unless `-Execute` is added. Each refuses (exit 2, nothing changed) while a run is active (port `5080` in use or a live runner or API process recorded by a manifest) or when SQL Server cannot be queried.

| Task | Command (dry run first; add `-Execute` to act) |
|---|---|
| Remove a retained or interrupted database | `.\scripts\test-artifacts.ps1 -RemoveDatabase FamilySafety_Invitation_E2E_<hex>` |
| Remove resolved run folders | `.\scripts\test-artifacts.ps1 -RemoveRunRecords` |
| Remove one cited or protected run folder | `.\scripts\test-artifacts.ps1 -RemoveRunRecords -RunId <run id>` |
| Empty the transient folders | `.\scripts\test-artifacts.ps1 -ClearGenerated` |

- **Databases** are chosen by exact name only. Wildcards (`* ? % [ ]`) are rejected, and there is no "all" option. The tool never drops a database itself: it finds the one manifest that records the name and calls `remove-scenario-database.ps1 -RemoveRetained` with it, so every WI-009-001 check still applies. `FamilySafety`, PRE-EXISTING names, names in a run's start snapshot, and names with no, several, or an unreadable owning manifest are always refused. After a drop, the manifest entry becomes `removed` with `removedAt`, `removedBy`, and a `history` entry; the earlier status is kept.
- **Interrupted runs** (no `completedAt` and no live runner or API process) are never cleaned up automatically. The status report labels their databases `retained diagnostic / interrupted` and prints the exact-name command. With `-Execute`, the entry is first recorded as `retained` (kept in `history`) and then removed through the guarded script.
- **Run folders** are removed without `-RunId` only when they are "Resolved": every database they record is confirmed absent, the manifest is valid, the run is not active, and no project file cites the run. A run counts as cited when its run ID or one of its database names appears anywhere in `ai-context/`, `claude-handoff/`, `codex-handoff/`, `docs/`, `scripts/`, or the root `README.md`. If that search is uncertain (a location is missing or a file cannot be read), the run is protected. A cited or protected folder is removed only with its exact `-RunId` and `-Execute`. A folder whose database still exists, or whose manifest is unreadable or invalid, is never removed.
- **Generated folders.** `-ClearGenerated` empties only `src/FamilySafety.Web/e2e/test-results/`, `e2e/captures/` (keeping its `.gitignore`), and `dist/verification/`, after confirming each lies inside `src/FamilySafety.Web/` and contains no junction or symbolic link. It never touches `dist/` itself, source, `ai-context/`, or anything at the repository root.
- **Build output.** Use `npm run build` (output in `dist/`). If a story needs a separate build copy, use `npm run build -- --output-path dist/verification/<WI-id>`. Never write build output to the repository root or under `ai-context/`.

`scripts/tests/check-test-artifacts-safety.ps1` re-runs the tool's refusal cases against temporary folders. It drops nothing and needs port `5080` free.

**Spec order (verification only).** `$env:FAMILY_SAFETY_E2E_SPEC_ORDER = 'trip-audience-removal,invitation-handoff,incident-workflow,concern-workflow'` runs the listed files in that order. It does not change any scenario.

## Troubleshooting

- **API startup fails connecting to SQL Server:** Confirm that SQL Server is running and reachable at `localhost`, and that the current Windows account can create/update the `FamilySafety` database. Set `ConnectionStrings__FamilySafety` to the correct local instance if needed.
- **The browser loads but API-backed content fails:** Confirm the API is listening at `http://localhost:5080` and `/health` responds. The proxy target is fixed to that address in `proxy.conf.json`.
- **No Trips, Plans, or Events appear:** The development fixture intentionally creates family/account records only; create a record through the UI.
