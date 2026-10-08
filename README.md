# Family Safety, Coordination & Planning

Local-first MVP for private family coordination and safety, with a .NET API and Angular web client.

## Current application

The application includes Home, Family and Family Settings, Events, Plans & Trips, Safety, and Me / Settings. It supports adult profiles, family relationships and invitations, Event and Plan coordination, Trips and Trip Notes, and private Concern and Safety Incident workflows. Approved capabilities that are not yet implemented remain outside the current application.

**Project state:** The bounded adult MVP is FUNCTIONALLY COMPLETE under [DEC-096](ai-context/decision-log.md#dec-096--close-phase-105-and-accept-functional-mvp-completion). Phases 9.5/10.5 are COMPLETE / CLOSED; Phase 10 remains CURRENT / IN PROGRESS for human usability validation. CTX-1.39 is active, WI-010-004 Stage B is PREPARATION COMPLETE / READY FOR P1, and no human session/result or next implementation WI is claimed. Existing deferrals remain intact. See [project status](ai-context/project-status.md) and [AI context](ai-context/README.md) for the current checkpoint and authority.

**Future production:** [DEC-097](ai-context/decision-log.md#dec-097--approve-future-phase-11-production-entry-model) and [DEC-098](ai-context/decision-log.md#dec-098--approve-provider-neutral-identity-and-production-entry-architecture) approve the [Phase 11 production-entry contract](ai-context/core/production-entry-contract.md), v1.1, and same-origin Angular / ASP.NET Core OIDC with server-side application sessions. [WI-011-001](ai-context/planning/stories/WI-011-001-provider-neutral-identity-and-production-entry-architecture.md) is APPROVED architecture planning only: production behavior UNIMPLEMENTED, no provider selected and no implementation authorized. CTX-1.39 and current MVP runtime behavior remain unchanged.

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

The API host is in `src/FamilySafety.Api/`; the Angular client is in `src/FamilySafety.Web/`. Product and privacy authority lives in `ai-context/`, not in this overview. Before adding or moving Angular folders, read the client's [structure and extraction rule](src/FamilySafety.Web/README.md#structure-and-extraction).

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

- Production authentication implementation and identity provider selection (future architecture approved under DEC-098)
- Production database edition/licensing and operations
- Notifications, maps/location providers, file storage, background jobs, and real-time services
- Hosting, deployment, infrastructure, monitoring, and production operations
- Unresolved product decisions recorded in `ai-context/project-status.md`

Development-only identity, fixture, and invitation-outbox adapters must not be used as production authentication or delivery behavior. Do not infer deferred choices through local-development defaults.
