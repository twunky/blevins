# Working Rules

## Source control

- **Do not commit.**
- **Do not push.**
- **Do not create, update, or complete a pull request.**
- Leave all changes uncommitted for the owner to review and commit manually. Provide a proposed PR title and description in the closeout as text only.
- The workspace has a Git repository. Report its actual status; the owner performs commits.

## Standing rule: scope comes from the current approved work item

**Only change application code when the currently approved, execution-authorized work item explicitly authorizes it.** This covers .NET code, API routes and contracts, permissions and authorization, database schema, EF migrations, and Angular code. Stay within that work item's stated scope, exclusions, and stop conditions.

Confirm the authorizing work item and its scope before editing `src/` or `tests/`. If no current work item authorizes a change, do not make it; ask the owner.

### Current approved work item: WI-095-001; execution authorized YES

The separate 2026-10-04 owner instruction authorizes exactly WI-095-001 under active CTX-1.30. Its bounded in-app invitation/self-RSVP implementation is VERIFYING / OWNER REVIEW. The work item and VER-WI-095-001 record exact evidence and pending owner visual review/closure. Phase 9.5 remains CURRENT / IN PROGRESS; all later items separately gated. Phase 9 closed; OD-002 OPEN. Cleanup Waves 1-5 authorize no broad cleanup. Owner performs all commits; no staging, commit, push or PR.

Closed work-item scopes and exclusions (WI-009-010 back to WI-008-003) are kept word-for-word in [HISTORY.md § Part B](HISTORY.md#part-b). They authorize nothing.

### Databases and generated folders

**Databases and generated folders:** do not drop any database or delete any folder outside the Playwright harness's own guarded per-run cleanup and `scripts/test-artifacts.ps1` (`WI-009-002`, `DONE / PASS`). That script's report is read-only; its cleanup is dry run by default, by exact name or run ID, with `-Execute`. The one-time owner-approved cleanup is complete (`ai-context/planning/verification/evidence/WI-009-001/database-cleanup-completion-2026-10-02.md`); the remaining `FamilySafety` and 15 uncertain databases must not be touched without a new owner authorization. `scripts/remove-scenario-database.ps1` is the only path that drops a database; `test-artifacts.ps1` delegates to it, and it refuses every PRE-EXISTING database.

### Reusable UI / mockup-alignment scope (from WI-008-002 and the DEC-070 site alignment — complete)

The rules below applied to the completed presentation-only UI work. Reuse them only when a future approved **UI** work item, such as `WI-008-004`, authorizes UI changes:

| Usually in scope for UI alignment | Notes |
|---|---|
| Angular templates and component styles for the screens named by the UI work item (`src/FamilySafety.Web/src/app/**`) | Restructure freely for presentation. Keep behavior, API calls, bindings, and required copy. |
| Shared styles (`src/styles.scss`, `src/app/app.scss`) and shell markup (`app.html`) | Only as far as the owner's palette, shell, and navigation decisions allow. |
| Component specs and Playwright specs | Only to follow changed markup or locators. Assertions keep the same meaning. |
| The work item's story and VER record, and its evidence folder | Notes and evidence only. Do not change acceptance criteria. |

For **presentation-only UI work**, do not change .NET code, API routes, contracts, permissions, schema, or migrations unless that UI work item explicitly authorizes it.

## For a separately authorized product work item

The permissions below describe the usual product work-item workflow and require a separately authorized work item. Completed repository cleanup authorizes no further work.

- Edit the files the current authorized work item allows (see above).
- Run `dotnet build`, `dotnet test`, Angular/Vitest tests, Playwright, and the production build.
- Start the API and Angular dev server locally (see `docs/local-development.md`).
- Inspect diffs, Git status, and file listings.
- Update the current work item's story (task checkboxes, implementation record) and create its VER record.

## You must not (any work item)

- Change code outside the current authorized work item's scope, or expand that scope silently.
- Edit canonical contracts, workflows, requirements, or decisions. Update `project-status.md` / `roadmap.md` only for status changes the owner authorizes.
- Resolve or imply a resolution of `OD-002`. `OD-001` is resolved by `DEC-074` (`REQ-UX-013`) and `OD-004` by `DEC-071`; do not reopen either.
- Change navigation destinations, count, or order outside an authorized navigation work item. The approved model is `REQ-UX-013`.
- Add mockup-only functionality, or fetch data the approved endpoints do not return.
- Raise Angular budget thresholds or add screenshot golden baselines.
- Overwrite or delete existing evidence PNGs. Write new files with distinct names.
- Mark any work item `DONE / PASS` yourself. Hand back in `VERIFYING / OWNER REVIEW`. UI items additionally require explicit owner visual approval.
- Rework the approved visual system or the accepted differences in `DEC-070` without a new owner request.

## Product and privacy invariants

- Server authorization is authoritative. Client-side role checks are display hints only; never let them grant or imply access.
- Family membership, Family Administrator, relationship, Safety Circle, Trusted Contact, subject status, Concern participation, and lineage never grant Concern or Incident access by themselves.
- Denied access must not disclose whether a Concern or Incident exists.
- No automatic Incident creation, escalation, danger determination, risk scoring, notification, or dispatch.
- No access to location, Check-In, Sharing, Trip, or private profile data through Safety records.
- Preserve every guardrail in `MOCKUP-ALIGNMENT.md` § Privacy guardrails for UI work.
- Keep tone calm and neutral: no urgency, lateness, danger, safety verdicts, or accusation.

## Stop conditions

Stop, do not guess, and report back when:

- `ai-context` sources conflict (`BLOCKED—CONTEXT CONFLICT`, citing both), or this handoff conflicts with `ai-context` (follow `ai-context` and report it).
- The work would need a change outside the current work item's authorized scope.
- **Server work:** any stop condition in the authorized story's own stop-conditions section. For example, WI-008-003's "Stop conditions during future execution" included a change to canonical Incident or Concern permissions, membership semantics, `OD-002` behavior, notifications, location/Check-In/Sharing, generic authorization architecture, backup infrastructure, existing tables' schema, or unrelated domains.
- Tests fail for reasons you cannot attribute to your changes.
- **UI work:**
  - a visual change would require a behavior, copy-requirement, API, or permission change;
  - a mockup idea needs data the API does not return;
  - fidelity requires an undecided palette, shell, or navigation change;
  - a screen has no primary reference and you would have to invent a target.

## Scope discipline

- No silent scope expansion. If something outside scope looks worth doing, list it under "Recommended follow-ups" in the closeout.
- For UI work, small shared visual tweaks for consistency are allowed; a site-wide redesign is not. **Owner visual approval is required.**

## Angular structure rule

The current Angular structure strategy is in [src/FamilySafety.Web/README.md § Structure and extraction](../src/FamilySafety.Web/README.md#structure-and-extraction). In short:

- A folder under `src/app` represents a genuine ownership or implementation boundary. Do not create folders for symmetry, and do not create `shared/`, `core/`, `common/`, `utils/`, or `features/` dumping grounds.
- Concentration in `App` is not a defect by itself.
- Extract incrementally, inside an authorized work item, when feature growth creates a real boundary. Do not propose broad `App` decomposition as cleanup.

## Practical notes for Claude sessions

- The environment is Windows. PowerShell is the primary shell, and Git Bash is also available. Use absolute paths such as `C:\Dev\blevins\...`. Python is not installed.
- SQL-gated .NET tests run only when `RUN_FAMILYSAFETY_SQL_TESTS=1`. Without it, they return early and report as passed. See `VALIDATION.md` § 4.
- Use the file-read tool to view PNG mockups and evidence directly.
- Playwright runs headless Chromium through the project config. Do not add capture scripts to the repo unless asked.
- Prefer editing existing files over creating new ones. Match the surrounding C#, Angular, and SCSS style.
