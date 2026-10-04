# D2 Plan Home filtering — owner decision and focused evidence

Owner continuation decision: keep a Plan through ExpectedReturnDate, falling back to StartDate when no expected return exists. Compare calendar dates with local today using the existing `localDateString` convention. The API calls the start date `date` on `FamilyPlanView`.

Implementation: in `homePlans()`, require current Family and `(plan.expectedReturnDate ?? plan.date).slice(0, 10) >= today`, then retain the existing start-date/time/ID sort and three-row cap. No lifecycle inference, API/schema/domain change, clock-time cutoff or timezone abstraction. Events/Trips predicates, Home design and authorization remain unchanged.

| Requested case | Focused component evidence | Rendered browser evidence |
|---|---|---|
| Future start appears | `future-start` included | today-future 1440/390 |
| Start today appears | `starts-today` included even at 00:00 | today-future and overnight-capped 1440/390 |
| Yesterday start, return tomorrow | `overnight-return-tomorrow` included | overnight-capped 1440/390 |
| Yesterday start, return today | `overnight-return-today` included even with return time 00:00 | overnight-capped 1440/390 |
| Return yesterday excluded | `returned-yesterday` excluded | absent in all six renders; remains accessible on `/plans` |
| Yesterday start, no return excluded | `past-start-no-return` excluded | absent in all six renders; remains accessible on `/plans` |
| Current-Family isolation | other-Family current Plan rejected | preserved existing isolated Family behavior; no new API access path |
| Filtering before cap | three stale rows and other-Family row do not consume slots | overnight pair + today fill three rows; future fourth is capped |
| Existing sort/item links | start-date/time order, exact hrefs | exact hrefs asserted; overnight item refresh + Back to Plans |
| Empty-state behavior | exact existing copy, Open Plans navigates via Angular Router | past-only empty at both widths; Open Plans lists both expired items |

`npm test -- --watch=false --include=src/app/home.spec.ts`: **25/25 PASS**, 4.25 s. Eight new tests; original Event/Trip assertions remain. The first attempt had 24/25 because the test incorrectly inspected browser `location.pathname` under Angular's mock router; changed to `TestBed.inject(Router).url`, keeping the navigation assertion. Both logs retained.

`npx playwright test --config e2e/playwright.config.ts e2e/phase9-home-filtering.spec.ts`: **1/1 PASS**, 5.0 s (35.3 s including harness), run `20261004T012930-e595465c`. Its one database was automatically removed. Six screenshots reviewed; approved desktop grid and mobile stacking remain with no document overflow. No new human participant or owner visual-review session is claimed.

The earlier full Angular 271/271 remains evidence for the unchanged code. Per owner instruction, the directly affected Home suite was rerun after the Plan rule; no new full 279-test run is claimed. The one final production build and final full Playwright are documented in VER-PH-009.
