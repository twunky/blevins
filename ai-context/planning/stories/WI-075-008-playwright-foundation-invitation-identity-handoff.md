---
Status: DONE — AC-01 through AC-19 PASS
Authority: Project-owner implementation authorization, 2026-09-29; verification recorded in VER-WI-075-008
Context-Baseline: CTX-1.24
Version: 1.2
---

# WI-075-008 — Establish Playwright Foundation for Invitation Identity Handoff

## Metadata

- **Work Item ID:** `WI-075-008`
- **Status:** `DONE — AC-01 through AC-19 PASS`
- **Phase:** `7.5 — End-to-End UX & User-Test Readiness`
- **Context baseline:** `CTX-1.24`
- **Execution authorization:** `YES` — project-owner approval, 2026-09-29
- **Work type:** Browser E2E regression infrastructure and bounded Development invitation journey.
- **Traceability:** Provides reusable technical browser evidence for `WI-075-005`; does not change its requirements or human-test gate.
- **Model tier:** `STANDARD`

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md` | Mandatory operating rules | Entry |
| `ai-context/project-status.md` | Phase and related work-item status | Entry |
| `ai-context/planning/stories/WI-075-005-end-to-end-mvp-user-test-execution-verification.md` | Technical versus human evidence boundary | Before execution |
| `ai-context/planning/verification/VER-WI-075-005.md` | Existing evidence history and pending human stage | Before execution |
| `scripts/start-invitation-scenario.ps1` | Existing isolated scenario startup contract | Before execution |
| `src/FamilySafety.Web/src/app/development-session.ts` | Existing Development identity and outbox controls | Before execution |
| `src/FamilySafety.Web/src/app/app.html` | Accessible invitation, profile, and handoff controls | Before execution |

## Goal

Add a minimal Playwright Test foundation that runs a real browser against a fresh Development invitation scenario and verifies stable invitation, identity-handoff, and server-authorization behavior.

## Boundaries / non-goals

- No visual regression, screenshot-baseline approval, UX approval, or human usability verification.
- No relationships, Events, Plans, Trips, Trip Notes, or audience-change browser workflows; these are a later phase.
- No detailed responsive or mobile layout assertions while design is evolving.
- No browser matrix, CI integration, Page Object framework, or large custom harness.
- No test-only DOM identifiers when a suitable accessible selector exists.
- No automatic scenario-database deletion; cleanup policy remains deferred.
- Do not change `WI-075-005`, mark its human criteria PASS, redefine Phase 7.5, or modify canonical requirements/context.

## Acceptance criteria

1. **`AC-01 — Minimal Playwright setup`**: The Angular package includes `@playwright/test` as a development dependency, a `test:e2e` script, and a `playwright.config.ts` using Chromium only, the local Angular base URL, one worker/serial execution for this stateful journey, and a sensible timeout. No unnecessary reporters, plugins, browser projects, or production dependencies are added.
2. **`AC-02 — Runner operation`**: The documented developer command launches and runs the focused test headlessly in Chromium against the real Angular app, API, and scenario database without relying on Edge window state.
3. **`AC-03 — Fresh isolated scenario`**: Each run starts a uniquely named `FamilySafety_Invitation_*` scenario using `scripts/start-invitation-scenario.ps1`; the scenario name/database is recorded with run evidence.
4. **`AC-04 — Ordinary database isolation`**: The run passes its unique scenario name to the existing startup script, records the effective scenario database name from startup evidence, and verifies the `FamilySafety_Invitation_*` prefix. The API fixture's existing prefix guard remains enabled. Startup never targets or resets the ordinary `FamilySafety` database.
5. **`AC-05 — Readiness and clean state`**: The fixture waits for API `/health` and Angular readiness, and verifies through the app/API that the fresh scenario has no Events, Plans, or Trips before the journey.
6. **`AC-06 — Adult A baseline`**: The browser verifies Adult A is active, has the expected Family context, and is its administrator.
7. **`AC-07 — Invitation and outbox`**: A can invite the scenario recipient; the Development outbox exposes the generated invitation link, which the test opens.
8. **`AC-08 — Identity switch isolation`**: Switching A to B leaves B active, shows B has no active Family, and clears A’s Family/session-scoped UI state.
9. **`AC-09 — Reload identity persistence`**: Opening the invitation link through document navigation/reload keeps B active; A is not restored as the active identity.
10. **`AC-10 — No stale A context`**: While B is active, A’s Family context and other stale A-scoped state do not reappear before or after invitation response.
11. **`AC-11 — Invitation preview`**: The preview identifies the expected Family and inviter and exposes the Accept and Decline actions.
12. **`AC-12 — Acceptance refresh`**: Accept succeeds and B’s membership refreshes in the same browser session without a manual page reload; B remains active.
13. **`AC-13 — Profile and navigation handoff`**: Optional profile review is available; continuing without changes completes the Family and Home handoffs for B.
14. **`AC-14 — Server authorization`**: Assertions verify B has no Family access before acceptance, Development identity persistence alone grants no access, unauthorized Family/private requests receive the expected denial or empty result, and after acceptance B receives only the access authorized by membership. The test checks UI and server outcomes without relying on implementation internals.
15. **`AC-15 — Accessible stable selectors`**: Tests prefer accessible roles, labels, and stable user-facing text, using existing semantics such as the “Active test identity” label, “Show Development invitation links” button, “Open invitation” link, Family navigation button, “Recipient email” label, “Send or resend invitation” button, “Invitation response” region, Accept/Decline buttons, and “Continue to Family without changes” button where each is present. No test-only identifiers are added where a reasonable accessible selector exists; any necessary accessibility improvement changes real semantics.
16. **`AC-16 — No brittle visual assertions`**: Tests do not depend on CSS styling classes, DOM hierarchy/order, child indices, pixel positions, exact dimensions/spacing, card composition, or mobile navigation layout.
17. **`AC-17 — Failure diagnostics only`**: Traces and screenshots are retained on failure for debugging; there are no screenshot baselines or golden-image assertions.
18. **`AC-18 — Process lifecycle`**: API and Angular child processes stop cleanly after the run. A small lifecycle wrapper is permitted only if needed to reliably stop the Windows process tree.
19. **`AC-19 — Existing regressions`**: The Angular/Vitest suite, SQL-enabled .NET/xUnit suite, and Angular production build pass with the new tooling in place.

## Scenario and test structure

Keep the E2E runner inside the Angular package so its tests and config resolve the package-local Playwright dependency without a root package or module-resolution workaround:

```text
src/FamilySafety.Web/e2e/
  playwright.config.ts
  fixtures/
    invitation-scenario.ts
  invitation-handoff.spec.ts
```

Run the API scenario and Angular through Playwright server orchestration or the smallest equivalent fixture. Use serial execution for the shared scenario. Retain the scenario database on failure and leave automatic deletion deferred; report scenario names so accumulated databases can be identified for later manual handling.

## Test-layer boundary

Playwright complements the existing test layers; it does not replace or duplicate their full suites:

- **Angular/Vitest:** component and application behavior with mocked APIs.
- **.NET/xUnit:** backend, domain, and API behavior, including SQL-backed scenario coverage.
- **Playwright:** the browser-to-API-to-database integration across the real Angular app and a fresh local scenario.

Keep the Playwright journey focused on browser navigation, real identity switching/reload, end-to-end handoff, and observable authorization outcomes. Leave lower-layer unit, component, and API cases to their existing suites.

## Dependency and local-data safety

Playwright and its Chromium browser are development tooling only; they must not enter the production bundle or runtime. Do not embed authentication secrets or private fixture data in the tests. Use only the existing Development A/B fixture identities and Development-only outbox, with the server remaining authoritative for access.

## Selector and assertion policy

Inspect current accessible semantics before adding selector workarounds. Prefer `getByRole`, `getByLabel`, and stable user-facing text where it is part of the user contract. In particular, use the existing “Active test identity”, “Show Development invitation links”, “Open invitation”, Family navigation, recipient email, send invitation, invitation response, Accept/Decline, and “Continue to Family without changes” semantics where they remain appropriate.

Assert behavior, application state, and server authorization outcomes. Do not treat a persisted Development identity as authorization. The server remains authoritative.

## Verification plan

- Run the new Playwright E2E command against a newly named dedicated scenario database.
- Capture the database name, API health, Angular readiness, browser pass/fail, invitation and identity-handoff assertions, and privacy/access outcomes.
- Confirm the trace and screenshot artifact behavior on failure without creating visual approval tests.
- Confirm on Windows that the PowerShell-started API process tree and Angular process terminate after the run. Use Playwright server shutdown alone if reliable; add only a small lifecycle helper if verification shows a child survives.
- Run the existing Angular/Vitest suite, SQL-enabled .NET/xUnit suite, and Angular production build; record test counts, results, and known environment limitations.
- Do not run or claim a human usability test.

## Future browser-testing phases

- **Phase A — Now:** Development identity, invitation, and access contracts in this work item.
- **Phase B — Later:** Relationship workflow, Events, Plans, Trips, Trip Notes, and audience changes in separately scoped work.
- **Phase C — After UI stabilizes:** Responsive and visual regression coverage, including any screenshot baselines.

Phase B and Phase C are outside this work item.

## WI-075-005 relationship

This work may provide reusable technical evidence for invitation handoff, identity persistence, membership refresh, and privacy/access boundaries. It does not replace actual participant evidence, pass human criteria, change the WI-075-005 requirements, or redefine Phase 7.5. `WI-075-005` remains `PENDING HUMAN TEST`.

## Requirements

No new canonical requirement is needed. This work item defines test infrastructure and evidence boundaries for existing behavior.

## Execution authorization

- **Approved work item:** `YES`.
- **Execution authorized:** `YES`.
- **Authorized by:** Project owner, explicit implementation approval.
- **Authorized at:** 2026-09-29.
- **Authorized scope:** Implement WI-075-008, verify AC-01 through AC-19, and update its verification/status records. No Phase B/C testing, visual regression, CI integration, unrelated fixes, WI-075-005 human criteria, Phase closure, canonical requirement, or OD-001 changes.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| AC-01 through AC-19 | [`VER-WI-075-008.md`](../verification/VER-WI-075-008.md) | `PASS` |

- **Verification report:** [`VER-WI-075-008.md`](../verification/VER-WI-075-008.md)
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `PASS`

## Completion

The Playwright foundation and one invitation/identity/privacy browser journey are implemented and passing. The package-local E2E layout was selected so tests resolve the Angular package's development-only Playwright dependency directly. All 19 acceptance criteria pass; see [`VER-WI-075-008.md`](../verification/VER-WI-075-008.md). `WI-075-005` remains `PENDING HUMAN TEST`; Phase 7.5 remains `CURRENT`; `CTX-1.24` is unchanged; and `OD-001` remains open.
