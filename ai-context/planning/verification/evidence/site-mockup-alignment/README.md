# Site-wide mockup alignment — evidence

- **Decision:** `DEC-070` (owner visual approval, 2026-10-01)
- **Visual target:** `claude-handoff/reference/` (desktop mockups)
- **Owner visual review:** `PASS` — approved 2026-10-01
- **Scope:** Presentation only. No product behavior, permission, privacy, API, schema, or migration change. Navigation destinations are unchanged; `OD-001` remains open.
- **Verification independence:** `SELF-VERIFIED` (Claude), with explicit owner visual approval

## Contents

| Folder | What it shows |
|---|---|
| `screens/` | 13 screens and states at 1440 px (`-desktop`) and 390 px (`-narrow`): Home (populated, and empty for Adult B), Family (list and selected member), Events, Plans, Trips (list, detail, create), Concerns (list, detail, create), Me / Settings. Captured from an isolated scenario database seeded through the existing API with sample data. |
| `concern-workflow/` | 15 Concern states at both widths, captured by `e2e/concern-workflow.spec.ts`. Frozen, unavailable, and list-error states use browser response interception, as before. |

No narrow capture showed horizontal overflow.

## Validation at approval

| Check | Result |
|---|---|
| Angular component suite (`npm test -- --watch=false`) | 66/66 pass; no spec edits |
| Playwright `concern-workflow.spec.ts` (run alone) | Pass |
| Playwright `trip-audience-removal.spec.ts` (run alone) | Pass |
| Playwright `invitation-handoff.spec.ts` (run alone) | Pass |
| Production build (`npm run build`) | Pass. Non-blocking warnings: initial bundle ≈ 526 kB (500 kB budget); `concerns.scss` ≈ 7.53 kB and `trips.scss` ≈ 5.79 kB (4 kB warning / 8 kB error). `app.scss` fell below the 4 kB warning. No budgets changed. |

**Known separate issue:** in one full-suite Playwright run the specs share a scenario database. The Concern journey adds Adult B, so `invitation-handoff` can fail its precondition. Each journey passes when run independently. This is test infrastructure and is outside this work.

## Accepted differences from the mockups

These are recorded in `DEC-070` and are not blockers:

- no photo thumbnails where no photo data exists
- no search, bell, or avatar-menu functionality
- no Messages destination
- no RSVP / Respond / Maybe
- no activity feed, Tasks, or Active Sharing
- no check-ins, map, or Incident
- no unsupported Trip statuses
- Events and Plans create forms remain visible beside their lists
- Trips Create action stays in its current placement
- narrow/mobile navigation remains unresolved under `OD-001`
- the development identity banner remains Development-only
