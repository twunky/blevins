# Family Safety, Coordination & Planning

Local-first MVP for private family coordination and safety, with a .NET API and Angular web client.

## Current application

The application includes Home, Family and Family Settings, Events, Plans & Trips, Safety, and Me / Settings. It supports adult profiles, family relationships and invitations, Event and Plan coordination, Trips and Trip Notes, and private Concern and Safety Incident workflows. Approved capabilities that are not yet implemented remain outside the current application.

**Project state:** Phase 9 is `COMPLETE / CLOSED` under [DEC-081](ai-context/decision-log.md) and [VER-PH-009](ai-context/planning/verification/VER-PH-009.md). `CTX-1.28` is active; development user testing is ready. Phase 9.5 is `READY TO BEGIN`, but no product implementation item is authorized. See [project status](ai-context/project-status.md) for the current checkpoint and [AI context](ai-context/README.md) for authority and working rules.

For local setup, development database behavior, seeded accounts, and current screens, see [Local development](docs/local-development.md).

## Technology baseline

| Area | Selected baseline |
|---|---|
| Server | .NET SDK 10.0.401 / ASP.NET Core 10 (`global.json`) |
| Web client | Angular 22.2 |
| Client runtime | Node.js 24 LTS with npm 12 |
| Database | SQL Server for local development |

The server is a modular monolith. Provider selection, production database edition/licensing, hosting, deployment, and other production services remain deferred.

## Repository structure

```text
src/
  FamilySafety.Api/             ASP.NET Core API host
  FamilySafety.Application/     Use-case boundary
  FamilySafety.Domain/          Domain model
  FamilySafety.Infrastructure/  EF Core and SQL Server persistence
  FamilySafety.Web/             Angular browser client
tests/
  FamilySafety.Foundation.Tests/ .NET tests
ai-context/                     Approved project context and work records
docs/                            Developer guides
```

The API host is in `src/FamilySafety.Api/`; the Angular client is in `src/FamilySafety.Web/`. Product and privacy authority lives in `ai-context/`, not in this overview.

## Restore, build, and test

From the repository root:

```powershell
dotnet restore FamilySafety.slnx
dotnet build FamilySafety.slnx --no-restore
dotnet test FamilySafety.slnx --no-build

Set-Location src/FamilySafety.Web
npm ci
npm run build
npm test -- --watch=false
```

`npm ci` restores packages from the lock file. It is not needed when dependencies are already installed.

## Deferred decisions

- Production authentication and identity provider
- Production database edition/licensing and operations
- Notifications, maps/location providers, file storage, background jobs, and real-time services
- Hosting, deployment, infrastructure, monitoring, and production operations
- Unresolved product decisions recorded in `ai-context/project-status.md`

Development-only identity, fixture, and invitation-outbox adapters must not be used as production authentication or delivery behavior. Do not infer deferred choices through local-development defaults.
