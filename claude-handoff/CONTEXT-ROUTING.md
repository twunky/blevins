# Context Routing

Use this to load only what a question needs. All paths are relative to `C:\Dev\blevins`. For requirement IDs, load the named section instead of the whole file when you can.

## Visual target (read first for any UI work)

| Question | Read |
|---|---|
| **What should the app look like?** | **`claude-handoff/reference/*.png` — the primary visual target.** Open the images directly. |
| Which reference maps to which screen, and what to KEEP / ALIGN / ADAPT / DROP | `claude-handoff/MOCKUP-ALIGNMENT.md` |
| Screens with no primary reference | Secondary layout hints in `ai-context/references/ux/` (`events/`, `plans-trips/`, `family/`, `safety/`, `admin-settings/`, `home-dashboard/`). Use them only where the primary set has no coverage. |
| What the app looked like before (comparison only, **not** a target) | `ai-context/planning/verification/evidence/**`, `src/FamilySafety.Web/*-review-*.png` |

## Authoritative project context

| Question / topic | Read |
|---|---|
| AI operating rules, authority, work modes, auto-approval limits | `ai-context/README.md` |
| Current state, next action, open decisions | `ai-context/project-status.md` |
| Phase status, Phase 9 boundary, Phase 9.5 assignment, and Phase 8 exit criteria | `ai-context/planning/roadmap.md` |
| Phase 9 scope, themes, and exit criteria (approved), `OD-001` resolution, deep-link rule | `ai-context/planning/proposals/PRP-030-phase-9-scope-themes-and-exit-criteria.md`, `ai-context/decision-log.md` → `DEC-073`, `DEC-074`; canonical `REQ-UX-011`, `REQ-UX-013`, `REQ-UX-014` (`ai-context/core/ux-contract.md`) and `REQ-PRIV-027` |
| Current work item (Playwright isolation) | `ai-context/planning/stories/WI-009-001-playwright-per-spec-scenario-isolation.md` |
| Product behavior, boundaries, permissions | `ai-context/core/product-contract.md` (`REQ-PROD-004`, `REQ-PROD-007`–`012` for Concern) |
| Privacy and safety rules, protected data, Concern and Incident access and retention | `ai-context/core/privacy-safety-contract.md` (`REQ-PRIV-005`–`008`, `011`, `025`, `026`) |
| UX rules: calm presentation, accessibility, uncertainty, provenance | `ai-context/core/ux-contract.md` (`REQ-UX-001`, `003`–`008`, `010`–`012`) |
| Concern and Incident workflow, categories, correction, resolution | `ai-context/workflows/safety.md` (`REQ-SAFE-001`–`016`; Concern `015`, Incident `016`) |
| Requirement ID lookup and traceability | `ai-context/core/requirements-index.md` |
| Terms (Concern, Participant, Frozen, …) | `ai-context/core/domain-glossary.md` |
| Check-In / Sharing separation (only if a mockup idea touches it) | `ai-context/workflows/sharing-checkins.md` |

## Concern decisions and records

| Topic | Read |
|---|---|
| Approved Concern access and lifecycle policy | `ai-context/planning/proposals/PRP-028-phase-8-concern-access-and-lifecycle.md` |
| Approval decisions | `ai-context/decision-log.md` → `DEC-066`, `DEC-067`, `DEC-068`, `DEC-069` (this is the full log; `planning/decision-log.md` is a partial excerpt) |
| Baseline change record | `ai-context/planning/candidates/CTX-1.25-phase-8-concern-access-and-retention.md` |
| Approved Incident policy and its baseline record | `ai-context/planning/proposals/PRP-029-phase-8-safety-incident-workflow-and-access.md`, `DEC-071`, `ai-context/planning/candidates/CTX-1.26-phase-8-safety-incident-workflow-and-retention.md`, `VER-CTX-1.26` |
| Server implementation and its evidence | `ai-context/planning/stories/WI-008-001-concern-domain-and-access-foundation.md`, `ai-context/planning/verification/VER-WI-008-001.md` |
| Incident server foundation and its evidence (`DONE / PASS`, owner approved; the API that WI-008-004 consumes) | `ai-context/planning/stories/WI-008-003-safety-incident-domain-and-access-foundation.md`, `ai-context/planning/verification/VER-WI-008-003.md` |
| Concern UI implementation (`DONE / PASS`) | `ai-context/planning/stories/WI-008-002-concern-mvp-user-interface.md`, `ai-context/planning/verification/VER-WI-008-002.md` |

## Visual-alignment precedent

| Topic | Read |
|---|---|
| Mockup gap assessment, token policy, KEEP/ADAPT/DROP origin | `ai-context/planning/proposals/PRP-027-mockup-alignment-ui-gap-assessment.md` |
| Worked examples of the KEEP/ALIGN/ADAPT/DROP method | `ai-context/planning/stories/WI-075-011-home-visual-alignment.md` through `WI-075-015-trips-visual-alignment.md` (read one or two, not all) |
| How earlier alignment work was verified and owner-approved | `ai-context/planning/verification/VER-WI-075-013.md` or `VER-WI-075-015.md` |
| Older mockups and their notes (secondary visual hints) | `ai-context/references/ux/`. The primary target is `claude-handoff/reference/`. |

## Code

| Area | Location |
|---|---|
| Concern feature (component, template, styles, spec, API client) | `src/FamilySafety.Web/src/app/concerns/` — `concerns.ts`, `concerns.html`, `concerns.scss`, `concerns.spec.ts`, `concern-api.ts` |
| Shell, routes, Home entry card | `src/FamilySafety.Web/src/app/app.html`, `app.ts`, `app.routes.ts`, `app.scss` |
| Design tokens (colors, radii, shadow) and global styles | `src/FamilySafety.Web/src/styles.scss` |
| Other screen styles for consistency reference | `src/FamilySafety.Web/src/app/family.scss`, `events.scss`, `plans.scss`, `trips/` |
| Concern E2E journey | `src/FamilySafety.Web/e2e/concern-workflow.spec.ts`; config `e2e/playwright.config.ts`; fixtures `e2e/fixtures/` |
| Concern API contract (read-only for this task) | `src/FamilySafety.Api/Concerns/ConcernEndpoints.cs`, `src/FamilySafety.Application/Concerns/ConcernContracts.cs`, `ConcernService.cs` |
| Running locally (API + Angular, dev identities, scenario script) | `docs/local-development.md`, `scripts/start-invitation-scenario.ps1`, root `README.md` |

## Do not load unless specifically needed

- `ai-context/bootstrap/` — extraction evidence only, not canonical
- Old `VER-CTX-1.x` reports and Phase 2–7 verification reports
- `events.md` and `plans-trips.md` workflows and unrelated proposals (PRP-003 to PRP-023)
- `ai-context/skills/` and `templates/`, unless you are creating a new work item or verification report
- `src/FamilySafety.Web/node_modules`, `dist`, and the `wi075*-build*` folders at the repo root (build output)
