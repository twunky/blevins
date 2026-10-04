# WI-008-004 rendered evidence — Incident UI and Start-an-Incident Flow

Captured 2026-10-02 by `src/FamilySafety.Web/e2e/incident-workflow.spec.ts` against an isolated `FamilySafety_Invitation_E2E_*` scenario database (real API on `:5080`, Angular dev server on `:4200`, headless Chromium).

- Every state is captured at **1440 px** (`*-1440.png`) and **390 px** (`*-390.png`), full page.
- Before each 390 px capture the spec asserts `document.documentElement.scrollWidth <= 390`. All 22 states passed, so there is no horizontal overflow.
- The yellow **Development test identities** banner at the top is the local test-identity switcher. It is expected and is not product UI.
- Only `incident-frozen-mocked-*` is **mocked**. Frozen cannot be reached in E2E without 30 days of inactivity or coordinator membership loss. The capture intercepts the real detail response for the same Incident and sets `state: Frozen` (with a `freezeReason`, which the UI must not show). Every other capture is a real server state.
- These files do not replace any `WI-008-002/` or `site-mockup-alignment/` evidence. The Concern regression run rewrote `site-mockup-alignment/concern-workflow/`. Those files were backed up first and then restored; an MD5 comparison of all 57 files confirmed they are identical to before the run.

| File stem | State | Journey / actor |
|---|---|---|
| `incident-list-empty` | List, empty state | J1, Adult A |
| `incident-create-direct` | Direct create: no people checked, subject marked "(subject, not added)", subject-not-shared copy | J1, A |
| `incident-unavailable` | Generic "This Incident is unavailable": the subject has no automatic access | J1, Adult B |
| `incident-people-editor` | People with access editor (coordinator not offered) | J1, A |
| `incident-list-open` | List, "Open coordination" group | J1, Adult C |
| `incident-finding-composer` | Add a finding composer | J1, C |
| `incident-detail-participant` | Open detail as a participant: no Coordination card and no summary action | J1, C |
| `incident-contact-composer` | Contact attempt: Time unknown, Reached, no-phone-or-address hint | J1, A |
| `incident-task-create` | Coordinator task creation with an assignee choice | J1, A |
| `incident-report-composer` | "Add another report" on another person's correction | J1, C |
| `incident-detail-coordinator` | Open detail as coordinator. Timeline: summary, finding, contact attempt, Unknown, tasks, task result, correction, another report. Tasks Open, Done, and Cancelled | J1, A |
| `incident-handoff-choose` / `incident-handoff-confirm` | Two-step coordinator handoff | J1, A |
| `incident-resolve-form` | Resolution form with Unknown contact-restored time and the can't-reopen copy | J1, C (new coordinator) |
| `incident-resolved` | Resolved, read-only, as a participant: Resolution card, "Not completed" task, no controls | J1, A |
| `incident-list-ended` | List, "Ended coordination (read-only)" group, with the kept-until date | J1, A |
| `incident-frozen-mocked` | Frozen read-only notice, kept-until date, no freeze reason (**mocked**) | J1, A |
| `home-safety-coordination` | Home "Safety coordination" card with Open Concerns and Open Incidents | J1, A |
| `concern-separate-incident-card` | Concern detail "Separate Incident" card (creator) | J2, A |
| `incident-create-from-concern` | Create from a Concern: the notice, subject prefilled, nothing else filled, no people checked | J2, A |
| `incident-detail-lineage` | Detail with "Started from a Concern" and "Open the Concern" | J2, A |
| `incident-detail-no-lineage` | The same Incident seen by a participant without Concern access: no lineage text | J2, B |
