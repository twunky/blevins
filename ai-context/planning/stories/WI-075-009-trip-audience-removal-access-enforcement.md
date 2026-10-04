---
Status: DONE — AC-01 through AC-18 PASS
Authority: Project-owner implementation authorization, 2026-09-29; verification recorded in VER-WI-075-009
Context-Baseline: CTX-1.24
Version: 1.1
---

# WI-075-009 — Verify Trip Audience Removal and Access Enforcement

## Metadata

- **Work Item ID:** `WI-075-009`
- **Status:** `DONE — AC-01 through AC-18 PASS`
- **Phase:** `7.5 — End-to-End UX & User-Test Readiness`
- **Context baseline:** `CTX-1.24`
- **Execution authorization:** `YES — project-owner approval, 2026-09-29`
- **Work type:** Focused Playwright regression coverage with a bounded Development-only fixture extension.
- **Traceability:** Adds technical evidence relevant to `WI-075-005`; does not replace human participant evidence, change its status, or close Phase 7.5.

## Objective

Add one focused Playwright browser journey that verifies a Trip creator can remove one selected audience member while another remains authorized, and that server-side Trip and Trip Notes authorization follows the confirmed audience. Verify creator access remains independent of selected-recipient membership.

Reuse the WI-075-008 Playwright foundation, the existing isolated invitation scenario, and its startup lifecycle. Keep the Development fixture addition explicit and limited to Adult C. Do not build a generalized fixture framework.

## Required context

| File or anchor | Why it is needed |
|---|---|
| `ai-context/README.md`; `ai-context/project-status.md` | Operating authority, active baseline, and Phase 7.5 status |
| `ai-context/workflows/plans-trips.md#req-trip-007`; `#req-trip-008`; `#req-trip-010` | Creator access, allowed Trip audiences, current authorization, and Trip Note behavior |
| `ai-context/core/privacy-safety-contract.md#req-priv-020`; `#req-priv-022` | Item audience and server authorization boundaries |
| `ai-context/planning/stories/WI-075-008-playwright-foundation-invitation-identity-handoff.md`; `ai-context/planning/verification/VER-WI-075-008.md` | Existing Playwright configuration, selectors, scenario lifecycle, and verification conventions |
| `ai-context/planning/stories/WI-075-005-end-to-end-mvp-user-test-execution-verification.md`; `ai-context/planning/verification/VER-WI-075-005.md` | Keep automated technical evidence distinct from pending human evidence |
| `scripts/start-invitation-scenario.ps1` | Existing isolated Development database startup contract |
| `src/FamilySafety.Api/Development/DevelopmentFixtureRegistration.cs`; `CurrentAccountRegistration.cs` | Existing Development identities and invitation-scenario fixture to extend minimally |
| `src/FamilySafety.Web/e2e/fixtures/invitation-scenario.ts`; `invitation-handoff.spec.ts`; `playwright.config.ts` | Reusable scenario helpers and browser journey conventions |
| `src/FamilySafety.Web/src/app/trips/trips.html`; `trips.ts` | Current accessible Trip controls and audience change behavior |
| `src/FamilySafety.Api/Trips/FamilyTripEndpoints.cs`; `FamilyTripNoteEndpoints.cs` | Canonical HTTP results for Trip and Trip Notes authorization failures |

## Scope

- One Playwright spec for Trip creation, audience editing, and access enforcement.
- An opt-in Development-only Adult C account/profile and active membership in the existing scenario Family, enabled only for this Trip E2E run.
- Use the existing invitation flow as prerequisite setup when B has not yet joined. The Trip spec must run by itself against a fresh scenario and must not depend on a previous spec's mutations.
- A creates a Trip with selected audience B and C. A is the creator and is not a selected audience recipient.
- B and C are active Family members before the Trip is created. B and C are both eligible selected recipients.
- B retrieves the Trip and adds a Trip Note. A changes the selected audience to C only, reviews the preview, and confirms.
- Verify post-confirmation authorization with UI and server/API assertions for A, B, and C.
- Verify an already-rendered B page is not treated as a real-time invalidation requirement. After audience change, subsequent B requests are denied and a refresh/refetch removes the Trip from B's authorized list.

## Non-goals

- Creator-only audience support or any Trip contract change.
- Immediate clearing of a Trip already rendered in another browser session.
- SignalR, polling, push invalidation, cross-tab notifications, or real-time audience-change events.
- Events, Plans, relationship workflows, Trip completion, Check-Ins, Sharing Sessions, location, notifications, Safety, or unrelated authorization paths.
- Visual regression, screenshot baselines, CSS/layout selectors, responsive regression, or full Phase B automation.
- Page Objects or a generalized E2E/fixture framework.
- Changes to `WI-075-005`, its human criteria, Phase 7.5 status, or canonical requirements.

## Finalized audience and stale-view behavior

The initial selected audience is B and C. A's creator authority is separate from recipient selection. After A confirms removal of B, C remains an audience recipient and B is no longer authorized. The implementation's requirement for at least one selected recipient is preserved; the creator is not added to the recipient list.

Server authorization changes immediately at confirmation. Any subsequent B request for Trip details, Trip Notes, note creation, or the authorized Trip list must be evaluated under the current audience. An already-rendered view may remain in memory until B makes a new request or refreshes/navigates to trigger a fresh fetch. The test verifies that no post-revocation request retrieves protected data or successfully mutates it, and that a refreshed authorized list omits the Trip. It does not require push-based UI invalidation.

Previously created Trip Notes continue to follow Trip authorization. Once B is removed, B cannot retrieve or add Trip Notes. A, as Trip creator, can still read the existing visible note with B recorded as its author. C remains authorized to retrieve the Trip as a selected audience member.

## Scenario and fixture

Use a fresh, uniquely named `FamilySafety_Invitation_*` database through `scripts/start-invitation-scenario.ps1`. Keep A as Family administrator and preserve B's invitation-scenario starting state as a nonmember. Add one explicit Development Adult C account/profile and active membership in the same Family; expose C as a Development identity only as needed for test setup and API actor identification. Do not add C to ordinary or production fixtures.

The Trip spec must establish B's active membership itself, using the existing invitation scenario flow as setup if B is not already a member. Do not rely on Playwright test order. The fixture and test must assert that A, B, and C are active members before Trip creation and that A is the administrator. Record the effective scenario database with the existing annotation/attachment mechanism.

Adult C must be an explicit opt-in to the invitation scenario used by this Trip E2E test (for example, a narrowly scoped startup-script switch passed only by this Playwright config). Enforce that the opt-in is accepted only in the Development environment with a dedicated `FamilySafety_Invitation_*` database. Without that opt-in, the existing invitation scenario must retain its current A/B behavior and B's no-membership starting state. With the opt-in, B must still start without membership and follow the existing invitation handoff before the Trip workflow. Keep C's identity allowlist and identity listing scoped to the opted-in Development scenario. Add no authentication secret. No schema migration, generalized fixture mechanism, or ordinary database reset is in scope.

## Browser and authorization approach

- Use existing Chromium-only Playwright configuration, one worker/serial execution, fresh isolated scenario startup, API health and Angular readiness checks, scenario recording, and existing Playwright web-server process lifecycle/cleanup behavior. Do not enable parallel scenario orchestration or destructive database cleanup.
- Prefer `getByRole` and `getByLabel` with stable accessible names. Avoid CSS classes, DOM hierarchy, positional selectors, dimensions, spacing, visual snapshots, and screenshots as pass criteria.
- Use the browser to prove B can reach the Trip before removal, can add a Note, and no longer sees the Trip after a fresh authorized list fetch.
- Use API requests with the existing Development identity header to prove authorization for A, B, and C. Assert the current canonical unavailable response for denied detail and Notes endpoints (`404`), and a successful authorized Trip list response that excludes B's Trip.
- For B's post-revocation note creation, assert the denied response and verify as A that no new note was created.
- Record automated evidence separately from any human participant observations.

## Acceptance criteria

1. **`AC-01 — Isolated scenario and lifecycle`**: The focused spec runs against a fresh uniquely named `FamilySafety_Invitation_*` database started by the existing scenario script with the Trip-specific C opt-in. It records the effective name, verifies API health and Angular readiness, retains one-worker/serial execution and existing process cleanup behavior, and never targets the ordinary `FamilySafety` database. It does not add destructive database cleanup.
2. **`AC-02 — Bounded opt-in C fixture`**: With the Trip-specific opt-in, accepted only in the Development environment for a dedicated `FamilySafety_Invitation_*` database, the scenario includes Adult C as an explicit account/profile with active membership in the same Family as A and exposes C only as an opted-in Development identity. B still starts without membership. Without the opt-in, existing A/B identity and invitation-scenario behavior is unchanged. No production or ordinary database fixture behavior changes, no auth secret is introduced, and no generalized fixture system is added.
3. **`AC-03 — Active member setup`**: Before Trip creation, A is an active Family administrator, C is an active Family member, and B is an active Family member. The test establishes B's membership itself when using a fresh scenario and does not depend on another spec running first.
4. **`AC-04 — Trip creator and audience`**: A creates a valid Trip with selected audience recipients B and C. The returned Trip identifies A as creator and B/C as recipients; A is not included in the selected audience list.
5. **`AC-05 — Initial B and C access`**: B and C can retrieve the Trip under their own identities. B's browser can open and display authorized Trip details.
6. **`AC-06 — Authorized collaboration note`**: B adds a Trip Note while authorized. The saved note content and author identity are verified. A and B can retrieve the note while B remains in the audience.
7. **`AC-07 — Creator audience edit`**: A can open the audience editor for the created Trip and submit an audience change through the existing preview/reconfirmation flow.
8. **`AC-08 — Accurate preview`**: The preview identifies B as losing access, retains C as a recipient, and does not treat A's creator authority as a selected-recipient entry.
9. **`AC-09 — Confirmed audience state`**: A confirms the previewed change. The saved Trip audience contains C and no longer contains B. The confirmation succeeds using the preview token returned by the current server preview.
10. **`AC-10 — B list authorization`**: After confirmation, B's authorized Trip list request succeeds with no entry for the Trip.
11. **`AC-11 — B detail denial`**: After confirmation, B's direct Trip detail request returns the canonical unavailable result, currently HTTP `404`, without returning Trip details.
12. **`AC-12 — B Notes read denial`**: After confirmation, B's Trip Notes list request returns the canonical unavailable result, currently HTTP `404`, without returning note content.
13. **`AC-13 — B Notes mutation denial`**: After confirmation, B's request to create another Trip Note returns the canonical unavailable result, currently HTTP `404`. A subsequent authorized read confirms no post-revocation note was created.
14. **`AC-14 — Refreshed B browser state`**: When B navigates back to Trips or otherwise triggers a fresh authorized fetch, the Trip is absent and cannot be reopened from B's authorized list. The test does not require immediate cross-session clearing before a new server interaction.
15. **`AC-15 — A creator access retained`**: After B's removal, A's authorized list and direct detail request still return the Trip. A can read the previously created visible B-authored Trip Note, with B remaining its author. This demonstrates creator access independently of the selected recipient list.
16. **`AC-16 — C access retained`**: After B's removal, C's authorized list and direct detail request still return the Trip, and C remains in the saved audience.
17. **`AC-17 — Stable browser assertions`**: The browser test uses accessible role/label selectors and behavior assertions. It does not rely on CSS classes, DOM order, card position, pixel dimensions, exact spacing, responsive assumptions, or visual baselines.
18. **`AC-18 — Regression and evidence boundary`**: Verification records the scenario database, Playwright test counts, B denial results, A creator-access result, C audience-access result, Trip Note outcome, process cleanup, and known limitations. The new Trip spec, existing invitation Playwright spec, Angular/Vitest suite, SQL-enabled .NET/xUnit suite, and Angular production build pass. Evidence is labeled automated technical verification and does not claim human usability evidence or change `WI-075-005` status.

## Regression verification plan

Run and record:

- The focused Trip audience Playwright spec against a fresh opted-in invitation scenario.
- The existing WI-075-008 invitation Playwright spec against a fresh default invitation scenario, confirming its existing A/B behavior remains intact.
- The complete Playwright suite serially.
- Angular/Vitest tests.
- SQL-enabled .NET/xUnit tests.
- Angular production build.

Record test counts/results, effective scenario database names, API health and Angular readiness, B's post-revocation denials, A and C retained-access results, the B-authored note result, process cleanup, and known limitations. Do not delete scenario databases automatically.

## Expected implementation files

- `src/FamilySafety.Api/Development/DevelopmentFixtureRegistration.cs` — add explicit Adult C fixture data for the invitation scenario only.
- `src/FamilySafety.Api/Development/CurrentAccountRegistration.cs` — expose/allowlist C only for the opt-in scenario; preserve the default A/B identities.
- `scripts/start-invitation-scenario.ps1` — add a narrow opt-in used only by the Trip Playwright spec; default invocation remains unchanged.
- Relevant Development fixture/identity tests, if required by the established regression pattern.
- `src/FamilySafety.Web/e2e/trip-audience-removal.spec.ts` — focused browser and API journey.
- `src/FamilySafety.Web/e2e/playwright.config.ts` — include the second focused spec while retaining isolated startup and serial execution.
- `ai-context/planning/verification/VER-WI-075-009.md` — evidence and AC results after separately authorized execution.

No changes are authorized by this proposed story. In particular, do not implement the Playwright test, change application behavior, modify `WI-075-005`, or update canonical requirements until a later explicit execution authorization.

## Relationship to WI-075-005 and Phase 7.5

This work may provide repeatable technical evidence for the Trip audience, access revocation, and creator-retention portions of `WI-075-005`. It cannot establish human discoverability, comprehension, navigation, privacy understanding, hesitation, or usability. Keep all participant-dependent criteria `PENDING HUMAN TEST`. Keep `WI-075-005` not `DONE`, Phase 7.5 `CURRENT`, `CTX-1.24` unchanged, and `OD-001` open. This story does not authorize Phase 7.5 closure.

## Implementation result

**DONE — AC-01 through AC-18 PASS.** The opt-in Development Adult C fixture, focused Trip audience Playwright journey, and bounded fixture/identity tests are implemented. No production Trip behavior or canonical requirement changed. Verification is recorded in `ai-context/planning/verification/VER-WI-075-009.md`.

`WI-075-005` remains `PENDING HUMAN TEST`; its human criteria are not passed by this technical evidence. Phase 7.5 remains `CURRENT`, `CTX-1.24` is unchanged, and `OD-001` remains open.
