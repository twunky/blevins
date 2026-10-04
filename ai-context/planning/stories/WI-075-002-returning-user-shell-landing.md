---
Status: DONE
Authority: Owner-approved execution of `WI-075-002` on 2026-09-28; `VER-WI-075-002` SELF-VERIFIED PASS
Context-Baseline: CTX-1.24
Version: 1.0
---

# WI-075-002 — Returning-User Shell and Landing

## Metadata

- **Work Item ID:** `WI-075-002` (`075` denotes Phase 7.5)
- **Status:** `DONE`
- **Phase:** `7.5 — End-to-End UX & User-Test Readiness`
- **Context baseline:** `CTX-1.24`
- **Proposal:** `PRP-023 — Application Shell & Returning-User Landing`, approved under `DEC-064`
- **Execution authorization:** `YES` — owner approval, 2026-09-28
- **Model tier:** `DEEP` (application routing, user-scoped client state, existing authorization boundaries, and invitation continuity)

## Objective

A returning user can enter the application, resolve an authorized current Family context, land on Home, navigate to the existing major product areas, and return Home through a persistent routed shell without repeating invitation/profile onboarding or using the old long-page navigation model.

## Requirement traceability

- `REQ-UX-001/002/003/006/007/011/012` — family-first, attention-first, progressive disclosure, accessible navigation, bounded Home, approved destinations, and responsive presentation. `REQ-UX-011` includes the `CTX-1.24` current-Family selection and switching rules; final mobile grouping remains open under `OD-001`.
- `REQ-FAM-005/006` — invitation-only membership and controlled/operator provisioning; no self-service creation or discovery.
- `REQ-FAM-007/008/009` — existing adult profile, invitation, and relationship behavior retained where exposed by the routed shell.
- `REQ-PRIV-006/011/013/014/020/021/022` — administrator privacy, membership boundary, profile and invitation privacy, and existing item-specific Event/Plan/Trip audience boundaries.
- Existing implemented slices remain the feature contracts for the reused screens: `WI-005-001` through `WI-005-004`, `WI-006-001` and `WI-006-002`, and `WI-007-001` through `WI-007-005`.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md`; `ai-context/project-status.md` | Operating rules, active baseline, and phase checkpoint | Entry |
| `ai-context/planning/roadmap.md#phase-75--end-to-end-ux--user-test-readiness`; `ai-context/planning/proposals/PRP-021-phase-7-5-user-test-readiness.md`; `ai-context/planning/proposals/PRP-023-application-shell-returning-user-landing.md`; `ai-context/decision-log.md#dec-064` | Phase boundaries, approved shell scope, and current-Family decision | Before execution |
| `ai-context/core/ux-contract.md#req-ux-001` through `#req-ux-003`, `#req-ux-006`, `#req-ux-007`, `#req-ux-011`, and `#req-ux-012` | UX destinations, current Family context, bounded Home, responsiveness, and open mobile decision | Before execution and verification |
| `ai-context/core/product-contract.md#req-fam-005` through `#req-fam-009` | Membership, provisioning, profile, invitation, and relationship boundaries | Before execution and verification |
| `ai-context/core/privacy-safety-contract.md#req-priv-006`, `#req-priv-011`, `#req-priv-013`, `#req-priv-014`, and `#req-priv-020` through `#req-priv-022` | Existing administrator, membership, profile, invitation, and item-audience privacy | Before execution and verification |
| `ai-context/workflows/events.md`; `ai-context/workflows/plans-trips.md` | Existing Event, Plan, and Trip behavior to expose without redesign | Before execution and verification |
| `ai-context/planning/stories/WI-075-001-invited-adult-first-use-journey.md`; `ai-context/planning/verification/VER-WI-075-001.md` | Invitation handoff, minimal Home, and Development identity regression baseline | Before execution and verification |
| `ai-context/planning/stories/WI-005-001-adult-self-profile.md` through `WI-005-004-adult-family-relationships.md`; `WI-006-001-event-creation-and-audience.md`; `WI-006-002-event-edit-access.md`; `WI-007-001-plan-audience-creation.md` through `WI-007-005-creator-controlled-trip-audience-editing.md`; associated verification reports | Completed feature contracts and regression coverage for screens relocated into routes | Before execution and verification |
| `src/FamilySafety.Web/src/app/app.routes.ts`, `app.ts`, `app.html`, `development-session.ts`, and `trips/` components; `docs/local-development.md` | Current route, workspace, Home, feature, identity-switcher implementation and repeatable Development setup | Before execution |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Safety, Sharing Session, Check-In, notification workflows and implementations | Explicitly outside this shell/routing slice; no new capabilities are authorized |
| Production identity-provider, registration, hosting, and deployment options | Production authentication and account registration remain deferred |
| Bootstrap, archive, superseded candidates, and non-authoritative mockups | Not needed to implement the approved current contract and scope |

- **Pinned context dependency:** None.

## Approved story boundary

1. Add a simple Angular routed shell for `/`, `/home`, `/family`, `/events`, `/plans`, `/trips`, and `/me`. Redirect `/` to `/home`. Add detail routes only when an existing detail state supports them without new domain behavior.
2. Make Home the returning-user landing after current identity and Family memberships resolve. Do not show invitation/profile onboarding to an ordinary returning user. Recognize a valid invitation entry before a generic Home redirect can discard its token, and preserve the existing focused invitation response and first-use handoff.
3. Resolve current Family context according to `DEC-064`: auto-select exactly one active Family; require explicit shell selection for multiple active Families; select none for zero active Families. Do not use response order as an implicit choice. Keep the selected context only for the current application/page session.
4. Display the current Family context where applicable. Treat it only as presentation/application context; server authorization remains authoritative and selection changes no permissions, membership, audiences, profile visibility, administrator authority, Sharing, or Safety behavior.
5. On Family change, clear or reinitialize prior Family-scoped directory/member and relationship data; Event, Plan, and Trip lists, details, forms, and selections; contextual administrator state; and stale in-flight results. Then load data authorized for the newly selected Family. Do not display the previous Family's data or forms while the new context resolves or fails. Preserve truly user-global state, such as the user's own profile, without carrying Family-scoped selections across the switch.
6. Reuse existing Family directory and relationship controls, Event UI, Plan UI, Trip UI, and own-profile/privacy controls at their routed destinations. Keep Plans and Trips conceptually grouped in shell navigation while retaining their separate `/plans` and `/trips` routes and distinct behavior. Preserve existing item-specific Trip creator access after membership ends; current Family selection must not grant or revoke that access or expose the former Family's directory.
7. Keep existing structural invitation administration contextual to Family and expose its entry only when the current user's existing administrator eligibility applies to the current Family. Administrator status grants no private-data access.
8. Preserve the Development identity switcher as clearly labeled Development tooling, visibly show the selected test identity, and retain prior-user workspace and Family-context clearing/re-resolution on switch. Production builds/configuration must not expose the tooling.
9. Preserve the current minimal Home and approved no-Family Home path. Do not add self-service Family creation, public discovery, or self-service join.
10. Use an interim responsive shell while preserving `OD-001`. Do not finalize mobile destination count/grouping.

## Non-goals

- Full Home/dashboard, Needs My Attention system, complex summaries, or activity feed.
- Final mobile navigation decision or resolution of `OD-001`.
- Production authentication, account registration, or identity settings.
- Self-service Family creation, Family discovery, or join requests.
- Pending-invitation overview, administrator transfer, member removal, or expanded administrator permissions.
- Event redesign, RSVP, Event Needs, or Event Updates.
- Plan or Trip redesign, audience-rule changes, or changes to Trip Notes/audience editing.
- Safety, Sharing Sessions, Check-Ins, or notifications.
- New domain behavior or permissions.

## Acceptance criteria

1. **`AC-01 — Returning-user landing`**: After the existing identity/session and membership data resolve, a returning user lands on Home and is not sent through invitation response or profile review again.
2. **`AC-02 — One active Family`**: When exactly one active Family is returned, it becomes the current context automatically and Home identifies it.
3. **`AC-03 — Multiple active Families`**: When more than one active Family is returned, none is silently selected by response order. The shell presents a clear selector, and Family-scoped content waits for the user's explicit selection. After selection, Home and routed areas use that Family.
4. **`AC-04 — No active Family`**: When no active Family is returned, no current Family is selected and Home shows the approved invitation/operator-provisioned guidance. A failed membership load is not presented as no active Family. No self-service creation, public discovery, or join action is offered. A valid invitation link can still enter the existing response flow.
5. **`AC-05 — Context boundary and lifetime`**: The current Family is visibly identified where applicable and its selection lasts only for the current application/page session. Selection grants no access and changes no membership, audience, profile visibility, administrator, Sharing, or Safety permission. Server authorization remains authoritative.
6. **`AC-06 — Family switch isolation`**: Switching current Family clears or reinitializes the prior Family's directory/member and relationship data; Event, Plan, and Trip lists, details, forms, and selections; and contextual administrator state before loading the new Family's authorized data. Stale responses or load failures cannot visually restore prior Family content. The user's own profile remains available without retaining prior Family-scoped selections.
7. **`AC-07 — Shell navigation`**: Persistent shell navigation exposes Home, Family, Events, Plans & Trips, and Me / Settings and reaches their explicit routes. `/` opens `/home`; routes work by direct navigation and browser history without turning the former long workspace into the normal navigation model.
8. **`AC-08 — Return to Home`**: Each routed product area has a clear way to return Home, and return navigation does not repeat first-use onboarding or discard the active current Family selection within the current application/page session.
9. **`AC-09 — Family area`**: `/family` exposes existing Family context, directory, and relationship controls for the selected Family, subject to current server authorization and existing profile visibility.
10. **`AC-10 — Event area`**: `/events` exposes the existing Event slice for the selected context and preserves existing Event behavior and item-specific access.
11. **`AC-11 — Plan area`**: `/plans` exposes existing Plan behavior and access rules for the selected context without changes to Plan audiences or workflow.
12. **`AC-12 — Trip area`**: `/trips` exposes existing authorized Trip overview/detail, creation, Notes, and audience-editing behavior without changing audience rules or workflow. The shell preserves the existing narrow creator access to an own Trip after Family membership ends, without treating it as active Family membership or granting directory access.
13. **`AC-13 — Plans and Trips remain distinct`**: Shell navigation groups Plans & Trips conceptually, while `/plans` and `/trips` remain separate routes and workflows. This does not resolve `OD-001`.
14. **`AC-14 — Me / Settings`**: `/me` exposes the existing self-profile and privacy controls with the same fields, visibility choices, validation, and save behavior.
15. **`AC-15 — Contextual administrator entry`**: Existing structural invitation controls are reachable from Family only when existing authorization says the user administers the currently selected Family. No admin entry or behavior exposes private data or adds authority.
16. **`AC-16 — Invitation continuity`**: A valid invitation link is recognized before a generic route redirect can lose its token. The focused preview, accept/decline, post-accept membership and Family-context reload/retry, optional profile review/skip, Family handoff, and minimal Home handoff continue to work. The accepted Family remains the explicit first-use handoff context; a later normal return with multiple active Families requires the shell selector. Ordinary returning-user navigation does not enter invitation flow without a valid invitation.
17. **`AC-17 — Development identity isolation`**: Development identity switching remains clearly marked and shows the active identity. Switching destroys/clears previous-user UI state, clears/re-resolves current Family selection, and loads the selected identity's data. Production builds/configuration expose no Development switcher or outbox tools.
18. **`AC-18 — Scope and responsive boundary`**: Narrow-layout smoke checks show the interim shell remains usable; the work does not resolve `OD-001`, implement full Home/dashboard behavior, or introduce new domain permissions or capabilities.

## Tasks

- [x] `TASK-001` — Reconfirm `CTX-1.24`, PRP-023, DEC-064, and current source behavior before execution; stop if a genuine canonical conflict or new requirement is found.
- [x] `TASK-002` — Add explicit routes and a persistent, responsive shell while preserving valid invitation URL handling.
- [x] `TASK-003` — Implement returning-user Home landing and zero/one/multiple-Family context resolution with session-only explicit selection.
- [x] `TASK-004` — Reuse and route existing Family, Event, Plan, Trip, and Me/profile UI; add contextual existing administrator entry.
- [x] `TASK-005` — Ensure Family changes clear old Family-scoped UI state and stale reads before/while loading the new authorized context.
- [x] `TASK-006` — Preserve Development identity switching and invitation first-use flow under the routed shell.
- [x] `TASK-007` — Add/update targeted route, shell, state-isolation, feature-regression, invitation, Development-guard, and responsive checks.
- [x] `TASK-008` — Run applicable Angular tests, .NET/API regressions, production build, and scoped checks; create `VER-WI-075-002` mapping every acceptance criterion and traced requirement to evidence.

## Verification plan

- Add Angular route/navigation tests for direct routes, root redirect, Home return paths, and browser history.
- Exercise returning-user zero-, one-, and multiple-Family cases, including explicit selection, visible current context, session-only lifetime, absence of implicit first-result selection, and a failed membership load that must not appear as the zero-Family state.
- Exercise Family switching while directory/relationships, Event/Plan/Trip lists/details/forms, administrator controls, and requests are active; confirm old scoped content and stale responses do not remain visible, the selected Family's authorized data reloads, and own-profile state remains separate.
- Regress existing Me/profile, Family directory/relationship, and current-Family administrator invitation behavior.
- Regress Event, Plan, and Trip routes and existing access/audience behavior, including Trip Notes, audience editing, and the creator's narrow own-Trip access after membership ends; do not change feature behavior.
- Regress WI-075-001 valid-invitation entry before Home redirect, preview, accept/decline, membership/context reload and retry, and first-use handoffs with the router active; on a later return with multiple Families, verify explicit shell selection.
- Test Development identity switching, active identity labeling, prior state clearing, Family-context re-resolution, and production exclusion of Development tools.
- Perform practical responsive/narrow-layout smoke checks without selecting final mobile grouping.
- Run applicable .NET/API regression tests and Angular tests, then a production build. Record commands/results and map every `AC-01`–`AC-18` and traced requirement in `VER-WI-075-002`.

## Risks and unresolved decisions

- **`OD-001` remains open.** The shell must remain responsive and usable without deciding the final number or grouping of mobile destinations.
- Existing screens currently share one workspace and several feature forms have their own Family selectors. Execution must reconcile them with the approved shell current-Family context without creating competing context choices or weakening server authorization.
- Route details should be added only where current UI/state supports them; new detail workflows are outside scope.
- Production authentication and session mechanisms remain unchanged. The returning-user behavior uses the identity/session source available in the existing application.
- No other product ambiguity is known from the approved proposal and current context. If execution discovers one, stop and return to proposal/context review rather than deciding it in code.

## Execution authorization

- **Approved story:** `YES`
- **Authorized by:** Project owner
- **Authorized at:** 2026-09-28
- **Authorized scope:** Execute `WI-075-002` under `CTX-1.24`, with its 18 acceptance criteria and bounded verification.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| Story scope against PRP-023 and DEC-064 | `CTX-1.24`, `PRP-023`, and `VER-CTX-1.24` reviewed during story preparation | `PASS` |
| Acceptance criteria and requirement traceability | `VER-WI-075-002` maps `AC-01`–`AC-18` and every traced requirement | `PASS` |
| Application implementation and tests | 41 Angular tests; 125 SQL-enabled .NET tests; production build; 390 px browser smoke | `PASS` |

- **Verification report:** `planning/verification/VER-WI-075-002.md`
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `PASS`; work item `DONE`
