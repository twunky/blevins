# WI-004-001 — Local MVP Foundation

## Metadata

- **Work Item ID:** `WI-004-001`
- **Status:** `DONE`
- **Phase:** Phase 4 — MVP Foundation
- **Context baseline:** `CTX-1.3`
- **Requirement references:**
  - `REQ-AI-003`
  - `REQ-AI-009`
  - `REQ-AI-010`
  - `REQ-AI-011`
  - `REQ-VERIFY-001`
  - `REQ-VERIFY-002`
  - `REQ-VERIFY-003`
- **Model tier:** `STANDARD`

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md` | Mandatory operating rules, work modes, context loading, verification, and lightweight work-item lifecycle | Entry |
| `ai-context/project-status.md` | Current phase, active baseline, blockers, and next action | Entry |
| `ai-context/planning/proposals/PRP-003-technical-architecture.md#phase-4-mvp-foundation-readiness-gates` | Phase 4 gates and non-authorizations | Before execution |
| `ai-context/decision-log.md#dec-019-local-first-expandable-architecture-foundation` | Local-first .NET/Angular/SQL Server candidate direction | Before execution |
| `ai-context/decision-log.md#dec-020-local-development-foundation-and-provider-deferment` | Provider deferment and replaceable boundary rule | Before execution |
| `ai-context/decision-log.md#dec-021-authentication-deferred-and-application-authorization-owned-internally` | Authentication deferment and internal authorization boundary | Before execution |
| `ai-context/decision-log.md#dec-022-phase-3-architecture-closure-and-phase-4-readiness-gates` | Phase 4 separate execution authorization and readiness rules | Before execution |
| `ai-context/decision-log.md#dec-023-best-practices-and-human-readable-work-standard` | Best-practices and human-readable standard | Before execution |
| `ai-context/decision-log.md#dec-024-required-context-manifest-discipline` | Narrow context-loading rule | Before execution |
| `ai-context/decision-log.md#dec-025-lightweight-personal-project-work-items` | Lightweight personal-project process | Before execution |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| `ai-context/bootstrap/` | Bootstrap source is extraction evidence only and not normal context. |
| `ai-context/workflows/` | This work item creates the technical foundation only; no product workflow behavior is implemented. |
| `ai-context/core/product-contract.md` | No product capability behavior is implemented in this work item. |
| `ai-context/core/ux-contract.md` | No user-facing UX behavior is implemented in this work item. |
| `ai-context/core/privacy-safety-contract.md` | No privacy/safety workflow behavior is implemented in this work item; architecture authorization rules remain from `DEC-021`. |
| Historical verification reports except `VER-PH-003` if needed | Prior reports are not needed unless a verification question arises. |

## Goal

Prepare the local MVP foundation execution scope without starting implementation. The next execution step, if separately authorized, will create a local-first application foundation consistent with the approved Phase 3 architecture.

## Boundaries / non-goals

- Do not create application code under this proposed work item until execution is separately authorized.
- Do not implement product features, screens, workflows, schemas, endpoints, authentication provider integration, hosting, deployment, or production database setup.
- Do not select paid providers or production services.
- Do not resolve `OD-001`, `OD-002`, `OD-003`, or `OD-004`.
- Do not load unrelated contracts or workflows unless a specific blocker requires them.

## Acceptance checks

1. `AC-01` — The work item has a narrow required-context manifest with a reason for each file or anchor.
2. `AC-02` — The work item preserves `CTX-1.3` and accepted decisions `DEC-019` through `DEC-025`.
3. `AC-03` — The work item clearly separates approval from execution authorization.
4. `AC-04` — The work item requires exact versions to be selected only after checking current official support documentation at execution time.
5. `AC-05` — The work item excludes product feature implementation, providers, hosting, deployment, and production database choices.
6. `AC-06` — A .NET 10 LTS modular-monolith solution foundation builds and its automated tests pass locally.
7. `AC-07` — An Angular 22 responsive web-client foundation builds and its automated tests pass locally on a supported Node 24 LTS runtime.
8. `AC-08` — The scaffold contains no product features, domain models, schemas, business endpoints, provider integrations, or unresolved product decisions.
9. `AC-09` — Human-readable local setup documentation records the selected versions, project boundaries, commands, and deferred decisions.

## Tasks

- [x] `TASK-001` — Review this proposed work item.
- [x] `TASK-002` — Approve, revise, or reject this work item.
- [x] `TASK-003` — If approved, separately authorize execution before any application code or scaffolding.
- [x] `TASK-004` — Verify current supported framework and local-tool versions from official sources.
- [x] `TASK-005` — Create the bounded .NET modular-monolith solution foundation without product implementation.
- [x] `TASK-006` — Create the bounded Angular responsive web-client foundation without product implementation.
- [x] `TASK-007` — Add human-readable local setup guidance and shared quality settings.
- [x] `TASK-008` — Build and test the server and client foundations and record evidence.
- [x] `TASK-009` — Move the work item to verification and perform a separate verification pass.

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner
- **Authorized at:** 2026-09-25
- **Authorized scope:** Create and verify the local-first MVP application foundation described by this work item, including current supported version selection and scaffolding, while preserving all listed boundaries and non-goals.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| Narrow context manifest | Required context and exclusion tables above | `PASS` |
| Explicit implementation authorization | Execution authorization section records project-owner authorization on 2026-09-25 | `PASS` |
| Phase 3 decisions preserved | References to `DEC-019` through `DEC-025` | `PASS` |
| No product scope expansion | Boundaries / non-goals section | `PASS` |
| Current supported versions | Official .NET, Angular, Node.js, and SQL Server documentation checked on 2026-09-25; installed/package versions recorded below | `PASS` |
| .NET build | `dotnet build FamilySafety.slnx --no-restore`: 0 warnings, 0 errors | `PASS` |
| .NET tests | `dotnet test FamilySafety.slnx --no-build`: 3 passed, 0 failed | `PASS` |
| Angular production build | `npm run build`: production bundle generated successfully | `PASS` |
| Angular tests | `npm test -- --watch=false`: 2 passed, 0 failed | `PASS` |
| API runtime health | `GET http://127.0.0.1:5080/health`: HTTP 200, body `Healthy` | `PASS` |
| Scope scan | No `WeatherForecast`, `Hello World`, `TODO`, or `FIXME` content outside generated dependencies/outputs | `PASS` |
| Formatting | .NET Format verification and Angular Prettier check | `PASS` |
| Dependency vulnerability metadata | NuGet: no vulnerable packages; npm production dependencies: 0 vulnerabilities | `PASS` |

- **Verification report:** `../verification/VER-WI-004-001.md`
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `PASS`; completion authorized by `VER-WI-004-001`

## Execution record

| Area | Selected or detected version | Basis |
|---|---|---|
| .NET SDK | `10.0.401` | .NET 10 is the current active LTS line; installed and pinned by `global.json`. |
| Angular CLI/core | `22.2.0` | Current stable Angular release; Angular 22 is in active support and transitions to LTS. |
| Node.js | `24.16.0` | Node 24 is LTS and satisfies Angular 22's supported `^24.15.0` range. |
| npm | `12.0.1` | Existing local package manager; recorded in the Angular package metadata and lock file. |
| TypeScript | `6.0.3` locked | Angular 22 supports TypeScript `>=6.0.0 <6.1.0`. |
| Local SQL Server | SQL Server 2019 Developer `15.0.2000.5` | Detected local service only; no application connection, schema, production edition, or licensing decision was introduced. |

Official lifecycle and compatibility sources checked on 2026-09-25:

- <https://dotnet.microsoft.com/en-us/platform/support/policy>
- <https://angular.dev/reference/releases>
- <https://angular.dev/reference/versions>
- <https://nodejs.org/en/about/previous-releases>
- <https://learn.microsoft.com/en-us/sql/sql-server/editions-and-components-of-sql-server-latest>
