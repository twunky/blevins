# Visual alignment and spacing — 2026-10-08

SELF-VERIFIED technical evidence using harness-owned synthetic data. This is a bounded presentation refinement on the approved dark design. It does not represent human usability validation. Existing rollout changes were preserved.

The profile's broad `.field:first-of-type` selector applied an 8px margin to the first field inside each contact pair while its neighbor received 16px. The selector now targets only the direct identity field. Shared paired fields use CSS subgrid to size label and control tracks from their actual content; longer labels cannot stagger controls. Profile help and validation messages share a separate feedback wrapper, preserving labels, descriptions and validation behavior. Birthday controls also share label tops, control tops and 44px heights at desktop/tablet widths.

Grouped fields now have one source of section spacing instead of adding both parent gaps and child margins. Trip date and optional-note pairs use shared tracks; mobile Trip rows have a measured 16px gap. Interior card heading content can shrink and wrap beside its icon instead of pushing the icon onto a separate line on phones. Home-specific styles and appearance are preserved.

Intermediate-width review found the two-column profile cards allowed the birthday fields to overflow at 1024px and 1100px. Profile cards now stay stacked below 1280px. Final checks at 1024/1100/1200/1280px confirm no horizontal overflow and keep the birthday controls within the card padding; see [profile-intermediate-widths.json](profile-intermediate-widths.json).

| Contact pair measurement | Before, 1440/768px | After, 1440/768px |
| --- | --- | --- |
| Email control top difference | 8px | 0px |
| Phone control top difference | 8px | 0px |
| Label top difference in each pair | 8px | 0px |
| Input/select height | 44.391px / 44px | 44px / 44px |

Raw dimensions are in `profile-before-*.json` and `profile-after-*.json`. At 390px, fields stack in reading order with feedback below its control. Stress checks use DOM-only long labels and helper text plus real client-side invalid-email feedback; no profile is saved.

| Representative evidence | Before | After |
| --- | --- | --- |
| Profile, desktop | [1440px](profile-before-1440.png) | [1440px](profile-after-1440.png) |
| Profile, tablet | [768px](profile-before-768.png) | [768px](profile-after-768.png) |
| Profile, phone | [390px](profile-before-390.png) | [390px](profile-after-390.png) |
| Home | [1440px](home-before-1440.png) | [1440px](home-after-1440.png) |
| Family mobile header | [390px](family-before-390.png) | [390px](family-after-390.png) |
| Event form | [1440px](event-create-before-1440.png) | [1440px](event-create-after-1440.png) |
| Trip mobile form | [390px](trip-create-before-390.png) | [390px](trip-create-after-390.png) |
| Safety mobile cards | [390px](safety-before-390.png) | [390px](safety-after-390.png) |
| Check-In mobile form | [390px](check-in-form-before-390.png) | [390px](check-in-form-after-390.png) |

Home comparisons also include 768px and 390px. Only this bounded set of 22 comparison screenshots is retained here. Home has zero differing pixels at 1440, 768 and 390px with matching synthetic fixtures, viewport and frozen clock; see [home-comparison.json](home-comparison.json).

Reviewed surfaces: shell and six-destination navigation, Home, Family, Family settings, Household and Shared Family Date drafts, profile, Events dashboard/all/create and detail with RSVP/Needs/Updates, Plans landing/list/create/detail, Trips list/create/detail/completion, Safety, Check-Ins, temporary sharing, Safety Circles, Trusted Contacts, Concerns, Incidents, and the generic unavailable state. Automated checks cover 23 surfaces at 1440/768/390px, visible controls, horizontal overflow, WCAG A/AA scans, navigation focus and reduced motion. Connected scenarios cover authorized/denied access, Family switching, error recovery and `/safety#check-ins`.

Validation: 61 focused Angular tests passed; the full frontend suite passed 420 tests in 27 files. The 14-check browser batch passed, including all 69 surface/viewport combinations. After the final profile-only breakpoint adjustment, all four alignment regressions passed at 1440/1024/768/390px, including the intermediate widths above. The final production build passed with existing budget warnings (initial 727.21kB vs 500kB warning budget; Home 5.79kB, Trips 5.71kB, Concerns 7.06kB vs 4kB component warning budget). No budgets were changed. `git diff --check` passed. No .NET test suite was run; the existing browser harness built its API once per run.

Commands: `npx ng test family-safety-web --watch=false --include=src/app/app.spec.ts --include=src/app/trips/trips.spec.ts --include=src/app/family-dashboard.spec.ts`; `npm test -- --watch=false`; `npm run build`. Browser runs used the existing isolated database harness and an ignored local config that reused the running Angular server. Specs: `visual-alignment`, `visual-design-rollout`, `connected-family-organization`, `connected-event-coordination`, `connected-plans-trips`, `connected-safety-care`, `home-pending-rsvp`, and `trip-completion`.

The first new tablet stress check compared viewport coordinates before and after Playwright scrolled the input into view. It was corrected to compare document coordinates; no product behavior was changed for that test failure. The harness retained its diagnostic database `FamilySafety_Invitation_E2E_c7b7288f79b24df3adbbc3fe956900e0` under `src/FamilySafety.Web/e2e/scenario-runs/20261008T043641-491f189f/manifest.json`, per the existing failed-run retention policy. Successful test databases were removed by their owning harness.

Files changed for this refinement:

| File | Reason |
| --- | --- |
| `src/FamilySafety.Web/src/app/app.scss` | Scope the profile margin override and correct the profile card breakpoint. |
| `src/FamilySafety.Web/src/app/app.html` | Group contact-email help and validation below its control. |
| `src/FamilySafety.Web/src/styles.scss` | Shared paired-field tracks, margins, heights and interior header wrapping. |
| `src/FamilySafety.Web/src/app/trips/trips.scss` | Remove double spacing and align paired Trip labels/controls. |
| `src/FamilySafety.Web/e2e/visual-alignment.spec.ts` | Regression coverage for wrapped labels, validation, control geometry and mobile gaps. |

No backend, schema, permissions, privacy defaults, state management, navigation destination, product copy, governance or human-testing evidence changes. No Git staging, commits or pushes.

The ordinary Development API was restored after the isolated browser runs (`/health`: `Healthy`), and the reused Angular server responds with HTTP 200 on port 4200. No unresolved presentation defect was found in the reviewed states. Existing build-budget warnings and the retained diagnostic test database above remain.
