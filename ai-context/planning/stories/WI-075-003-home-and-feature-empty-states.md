---
Status: DONE
Authority: Owner-approved PRP-024 and explicit owner decisions on Home preview limits and Trip context
Context-Baseline: CTX-1.24
Version: 1.0
---

# WI-075-003 — Home and Feature Empty States

## Metadata

- **Work Item ID:** `WI-075-003`
- **Status:** `DONE`
- **Phase:** `7.5 — End-to-End UX & User-Test Readiness`
- **Context baseline:** `CTX-1.24`
- **Proposal:** `PRP-024 — Home and Feature Empty States`, approved by the project owner on 2026-09-28. Approval and the proposal text are recorded in the project-owner session; no proposal file was present in the repository when this work item was prepared.
- **Owner decisions:** Home previews show at most three items per area (Events, Plans, Trips). Home Trips are limited to the current Family context. Creator-accessible Trips outside that context remain available in Trips under existing rules and are not shown on Home in this slice. This is presentation-only and does not change Trip access.
- **Execution authorization:** `YES`
- **Model tier:** `DEEP` (privacy-sensitive presentation filtering and regression of existing access boundaries; no authorization or domain changes)

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md`; `ai-context/project-status.md` | Operating authority, phase, baseline, and completion checkpoint | Entry |
| `ai-context/planning/roadmap.md#phase-75--end-to-end-ux--user-test-readiness`; approved `PRP-024` and its owner decisions recorded in the 2026-09-28 approval session | Phase boundary, approved scope, preview cap, and current-Family Trip Home rule | Before execution |
| `ai-context/core/ux-contract.md#req-ux-001` through `#req-ux-003`, `#req-ux-006` through `#req-ux-008`, and `#req-ux-011` | Family-first, attention-first, progressive disclosure, Home purpose, and current Family shell behavior | Before execution and verification |
| `ai-context/core/product-contract.md#req-fam-005` through `#req-fam-009` | Invitation/operator-based membership, profile, and relationship boundaries | Before execution and verification |
| `ai-context/core/privacy-safety-contract.md#req-priv-006`, `#req-priv-011`, and `#req-priv-020` through `#req-priv-022` | Administrator boundary and authorized Event/Plan/Trip visibility, including Trip creator access | Before execution and verification |
| `ai-context/workflows/events.md#req-evt-001`, `#req-evt-005`, and `#req-evt-013` | Event details/purpose, existing flow, and explicit Home/Check-In/notification boundaries | Before execution and verification |
| `ai-context/workflows/plans-trips.md#req-plan-001`, `#req-plan-005`, `#req-plan-006`, `#req-trip-001`, `#req-trip-007`, and `#req-trip-010` | Plan and Trip purpose, audience, Notes, and creator access; preserve current behavior | Before execution and verification |
| `ai-context/planning/stories/WI-075-001-invited-adult-first-use-journey.md` and `ai-context/planning/verification/VER-WI-075-001.md` | Existing invitation handoff, minimal Home, and first-use behavior to preserve | Before execution |
| `ai-context/planning/stories/WI-075-002-returning-user-shell-landing.md` and `ai-context/planning/verification/VER-WI-075-002.md` | Routed shell, selected Family, route reuse, and current administration behavior to preserve | Before execution |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Safety workflow and requirements | Safety is explicitly excluded and receives no new Home behavior |
| Sharing and Check-In workflows | No sharing or check-in execution is implemented by this story |
| Bootstrap, archive, mockups, and unrelated historical verification | Not needed; approved contracts and current work records are authoritative |
| Production identity, account registration, and authentication provider decisions | No identity or registration behavior changes |

- **Pinned context dependency:** None.
- **Requirement references:** `REQ-UX-001/002/003/006/007/008/011`; `REQ-FAM-005/006/007/008/009`; `REQ-PRIV-005/006/011/013/014/020/021/022`; `REQ-EVT-001/005/013`; `REQ-PLAN-001/005/006`; `REQ-TRIP-001/007/010`.

## Objective

Make Home and the existing Family, Events, Plans, Trips, and Me / Settings areas understandable and useful when the current user has little or no visible data, using only existing authorized data and actions without changing domain behavior.

## Non-goals

- Full Home dashboard or general Needs My Attention system.
- Notifications, activity feed, RSVP, Event Needs, or Event Updates.
- Safety, Sharing Sessions, Check-Ins, or location-sharing behavior.
- Final mobile navigation decision or resolution of `OD-001`.
- New administrator capabilities or changes to authorization/access rules.
- Event, Plan, or Trip domain redesign, new APIs for richer Home cards, migrations, or production authentication.
- Self-service Family creation, discovery, or public join.
- Home display of creator-accessible Trips outside the selected current Family.

## Acceptance criteria

1. **`AC-01 — No active Family`**: After membership loading succeeds with zero active Families, Home explains that no active Family is available and that access begins through a Family administrator invitation or controlled setup. Valid invitation response remains available. Me / Settings remains reachable. No Create Family, Find Family, or public/self-service join action is offered.
2. **`AC-02 — Membership load failure`**: A membership request failure is presented as a load error with the existing retry path, not as a zero-Family or empty-content state. No Family-specific summary or create action is shown as if loading had succeeded.
3. **`AC-03 — Low-data Home`**: With a selected Family and no visible Events, Plans, or current-Family Trips, Home identifies the Family, explains that the app is ready for family coordination, and offers a small set of links/actions into existing areas. It has no blank dashboard cards and no false urgency.
4. **`AC-04 — Populated Home`**: Home shows a compact preview for each area (Events, Plans, Trips) that has qualifying data, and no empty dashboard card for an area without qualifying data. A preview contains currently authorized upcoming Events, currently accessible Plans, or Trips belonging to the selected current Family, respectively. Each preview links to its existing routed area. Items are drawn from existing authorized data; no new API or domain behavior is introduced solely to enrich Home. “Upcoming Events” uses the existing Event date/time fields: omit Events dated before the user's current local calendar date, then order by date/time ascending. Do not add time-zone conversion or a new Event status.
5. **`AC-05 — Preview limit and ordering`**: Home shows no more than three Events, three Plans, and three Trips in their respective areas. Order Events by date/time ascending; Plans by their existing date/time ascending; and current-Family Trips by start date ascending. Use a stable existing item identifier to break ties. Do not define or infer completion, past/future status, or lifecycle for Plans or Trips. When more authorized items are available, the section links to the full routed area.
6. **`AC-06 — Current-Family Trip boundary`**: Home previews only Trips associated with the current Family context. Creator-accessible Trips outside active/current Families do not appear on Home. Those Trips remain available in the Trips area according to existing creator-access rules; no access behavior changes.
7. **`AC-07 — Relationship action scope`**: A pending Family relationship suggestion addressed to the current user may appear as a clearly scoped Family action. When none is present, Home omits that action or uses neutral wording limited to the relationship suggestions actually checked. Home does not claim that no action exists elsewhere in the app.
8. **`AC-08 — Family no-context state`**: The Family destination gives the same invitation/operator guidance when there is no active Family and distinguishes membership loading errors from a successful zero-Family result. It offers no self-service Family creation, discovery, or join.
9. **`AC-09 — Family small directory and relationships`**: A one-member or small directory remains understandable; copy explains that administrators may invite adults without implying that the current user can do so unless existing administrator authorization allows it. Empty relationships are described as none currently visible/available in this Family; existing relationship suggestion behavior remains unchanged and relationship is not presented as permission.
10. **`AC-10 — Directory filter empty state`**: A filter with no matches clearly says no active adult members match the filter and offers a clear-filter or change-filter action. It does not imply that the Family has no other members.
11. **`AC-11 — Events empty state`**: When the authorized Event list is empty, the page briefly explains the purpose of Events and uses privacy-safe wording such as “No Events are currently shared with you in this Family.” Existing Create Event behavior is shown only when its current Family and eligible-audience requirements allow creation; otherwise the unmet existing requirement is explained without a dead-end action.
12. **`AC-12 — Plans empty state`**: When the authorized Plan list is empty, the page briefly explains a Plan and uses privacy-safe wording such as “No Plans are currently visible to you in this Family.” Create Plan is shown only where current behavior permits it; existing audience, date, notes, and creation behavior remain unchanged.
13. **`AC-13 — Trips empty state`**: When no Trip is available to the current user, the page explains that Trips organize travel information and may include existing Trip Notes for authorized viewers. It uses privacy-safe wording such as “No Trips are currently available to you.” Create Trip is shown only where existing authorization permits it. Creator access, audience, Notes, and audience-editing behavior remain unchanged.
14. **`AC-14 — No-context feature guidance`**: Events, Plans, and Trips do not use misleading self-service Family creation/join copy when no active Family exists. They use the consistent invitation/operator guidance, with valid invitation handling preserved.
15. **`AC-15 — Me / Settings introduction`**: Me / Settings may have concise introductory copy identifying own profile and current profile-field visibility choices. No account security, production identity, registration, or new profile fields are added.
16. **`AC-16 — Administrator explanation`**: Existing Family invitation controls may have concise contextual help explaining approved invitation behavior. Copy makes clear that administrator status does not grant private-data access. No new controls or authority are introduced.
17. **`AC-17 — Loading, error, and empty distinction`**: Home and each affected destination distinguish loading, failed load, and successful empty authorized results. A failed data request is never represented as proof that no item is visible or exists.
18. **`AC-18 — Authorized next actions`**: Each area offers at most one or two primary next actions. Home creation shortcuts appear only when the corresponding existing feature eligibility checks would allow the user to proceed; otherwise Home links to the area and the area explains its existing unmet requirement. Create/invite controls are displayed only when current eligibility and authorization permit them. Do not add a role, grant, or eligibility rule; existing server authorization remains authoritative.
19. **`AC-19 — Privacy-safe language`**: Empty-state copy describes the user's view using “shared with you,” “visible to you,” or “available to you” where appropriate. It neither asserts that hidden items exist nor that none exist, and contains no developer/internal terminology, surveillance language, unsupported capability claim, or false urgency.
20. **`AC-20 — Existing behavior and exclusions`**: Existing invitation responses, profile editing/visibility, Family directory/relationship permissions, Event/Plan/Trip audience and access behavior, Trip creator exception, Trip Notes access, administrator boundaries, and routed shell/current-Family behavior continue to pass regression checks. No notifications, activity feed, RSVP, Event Needs/Updates, Safety, Sharing Sessions, Check-Ins, new attention source, or domain redesign is introduced.

## Tasks

- [x] `TASK-001` — Reconfirmed CTX-1.24, approved PRP-024, owner preview decisions, and current implementation behavior; no canonical conflict or requirement gap was discovered.
- [x] `TASK-002` — Implemented compact Home hierarchy and previews from existing authorized Events, Plans, and selected-current-Family Trips, capped at three per area, with the scoped relationship action only.
- [x] `TASK-003` — Improved Family, Events, Plans, Trips, and Me / Settings explanatory/empty-state copy and retained existing actions according to current authorization and eligibility.
- [x] `TASK-004` — Preserved loading, errors, and successful empty results as distinct states; Home omits out-of-current-Family creator Trips.
- [x] `TASK-005` — Added/updated targeted UI checks and preserved existing Family, invitation, profile, Event, Plan, Trip, audience, and privacy behavior; no domain or persistence behavior was added.
- [x] `TASK-006` — Ran Angular checks, .NET/API regression checks and production build; documented the practical narrow-layout smoke limitation; created `VER-WI-075-003.md` mapping all acceptance criteria and traced requirements.

## Verification plan

- Test zero-Family Home and Family guidance, invitation response continuity, and Me / Settings access; separately test membership-load failure/retry.
- Test selected-Family low-data and populated Home states, maximum three items per preview, destination links, and relationship suggestion present/absent states.
- Test Trip preview scoping to the selected Family; verify an out-of-current-Family creator Trip is absent from Home and still available on Trips under existing rules.
- Test Family one/few-member, no-relationship, and no-directory-filter-match states, including clear/change filter action.
- Test Event, Plan, and Trip privacy-safe empty copy, concise purpose, current create eligibility, and no dead-end create actions.
- Regress profile and Me / Settings; contextual administrator invitation controls and no private-data implication; Event/Plan/Trip audiences, access, Trip Notes, and creator exception.
- Check loading/error/empty distinctions and verify no unsupported features are suggested by copy or controls.
- Run Angular tests, applicable .NET/API regression tests, production build, and a narrow-layout browser smoke check where practical.
- Record results in `ai-context/planning/verification/VER-WI-075-003.md`, mapping `AC-01`–`AC-20` and every traced requirement to evidence.

## Requirement traceability

| Requirement | Application in this story |
|---|---|
| `REQ-UX-001/002/003/006/007/008/011` | Family-first, attention-before-activity, progressive disclosure, accessible guidance, Home purpose, bounded implemented-content previews, and preservation of current routed Family context without resolving `OD-001` |
| `REQ-FAM-005/006/007/008/009` | Invitation-only membership, operator provisioning, existing profile/visibility, invitation lifecycle, and relationship behavior remain unchanged |
| `REQ-PRIV-005/006/011/013/014` | Relationships and administrator status do not imply private-data access; membership, profile visibility, and invitation paths remain accurate |
| `REQ-PRIV-020/021` | Event/Plan/Trip summaries and empty states use only authorized data and preserve existing audience and Event edit boundaries |
| `REQ-PRIV-022`; `REQ-TRIP-007/010` | Current-Family Home Trip previews and creator-accessible Trip exception/Notes remain under current Trip authorization; excluded creator Trips remain on Trips only |
| `REQ-EVT-001/005/013` | Existing Event details/purpose and current Event flow inform presentation; Home boundaries remain explicit and do not implement RSVP, Needs, Updates, Check-Ins, Sharing, or notifications |
| `REQ-PLAN-001/005/006` | Existing lightweight Plan purpose, audience, and Notes inform copy; Plan behavior remains unchanged |
| `REQ-TRIP-001/007/010` | Existing Trip purpose, creator/audience access, and Notes inform copy; Trip behavior remains unchanged |

No canonical requirement is added or changed. If implementation analysis discovers a genuine canonical gap or conflict, stop and return to proposal/context review before changing scope or requirements.

## Risks and blockers

- Home previews must consume only data already loaded/available under current authorization. If existing routes/components do not expose required data without new domain/API behavior, stop and report the gap rather than adding it implicitly.
- Current Family switching and in-flight loading must not leave stale Family data visible in a Home preview.
- Event preview selection uses the user's local calendar date only to apply the approved “upcoming” presentation. Event times remain as entered; no cross-time-zone interpretation is introduced.
- Existing Plans have no completion state in this story; show the currently accessible Plan results and order them by their existing date/time fields without inventing a lifecycle.
- Owner-approved PRP-024 approval and detailed text are present in the 2026-09-28 project-owner session rather than a repository proposal file; the approval message and decisions are the story authority for this slice.
- `OD-001` remains open. This story makes no final mobile navigation decision.

## Execution authorization

- **Approved story:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner, explicit pasted approval
- **Authorized at:** 2026-09-28
- **Authorized scope:** WI-075-003 and its 20 acceptance criteria under CTX-1.24 only

## Evidence

| Requirement or criterion | Evidence | Result |
|---|---|---|
| Story scope and traceability | Approved PRP-024, owner decisions, CTX-1.24 contracts/workflows, and WI-075-001/WI-075-002 behavior reviewed | `PASS` |
| `AC-01`–`AC-20` and all traced requirements | `ai-context/planning/verification/VER-WI-075-003.md` evidence matrix | `PASS` |
| Angular UI tests | 53 passed across 5 files | `PASS` |
| .NET/API regression | 125 passed, 0 failed; SQL-enabled run also passed 125/125 | `PASS` |
| Production build | Angular production build completed, 430.56 kB initial total | `PASS` |

## Verification

- **Verification report:** `ai-context/planning/verification/VER-WI-075-003.md`
- **Independence:** `SELF-VERIFIED`; independent verification unavailable.
- **Outcome:** `PASS`
- **Verified at:** 2026-09-29
- **Limitation:** Narrow-layout browser smoke not run because supported Development startup applies migrations and seeds a persistent SQL database; automated UI checks and production build passed.
