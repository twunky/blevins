# Family Safety, Coordination & Planning

Local-first MVP for private family coordination and safety.

## Current application

The repository contains working .NET API and Angular UI slices for adult profiles, family directory and relationships, invitations, Events, Plans, and Trips. Phase 7 work completed so far includes Plan audience selection and notes, Trip creation/audience/detail viewing, and collaborative Trip Notes. Check-Ins, Sharing Session execution, and post-creation Trip audience editing are not currently implemented.

For the exact local setup, development database behavior, seeded accounts, and current screens, see [Local development](docs/local-development.md).

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
