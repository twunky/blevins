---
Status: DONE / PASS — OWNER APPROVED
Authority: Project-owner approved and execution-authorized for WI-009-004 only (2026-10-02), against CTX-1.27, with owner decisions D1–D10 recorded below; planning basis PRP-030 M3 (server part) / DEC-074 D4
Context-Baseline: CTX-1.27
Version: 1.0
---

# WI-009-004 — Event and Plan Single-Item Read Foundation

## Metadata

- **Work Item ID:** `WI-009-004`
- **Status:** `DONE / PASS` — `OWNER APPROVED` (owner review 2026-10-02). All 22 acceptance checks pass; see the [Owner closure](#owner-closure-2026-10-02) and `planning/verification/VER-WI-009-004.md`. It was `APPROVED — EXECUTION AUTHORIZED` against `CTX-1.27` (project owner, 2026-10-02). The item authorizes nothing further. The [Owner decisions](#owner-decisions-2026-10-02) override any looser wording in this item.
- **Phase:** `9 — Integration & UX Refinement`, `CURRENT / IN PROGRESS` (`DEC-073`)
- **Context baseline:** `CTX-1.27`
- **Approved planning basis:** `PRP-030` (`APPROVED`, `DEC-074`), MUST item **M3**, server part only: the "Server finding" that Events and Plans need additive single-item reads under their existing authorization, with the same nondisclosing result for missing and unauthorized items. Theme 4 "Item-level routing and cross-feature continuity".
- **Requirement references:**
  - `REQ-PRIV-027`: implemented on the server for Events and Plans. Its rule that the item is authorized before the client learns its Family is what this item provides.
  - `REQ-UX-014`: this item adds the server foundation for steps 1 and 3. Step 2 (the client Family switch) is **not** implemented here.
  - `DEC-074` D4: deep-link Family-context flow (server part only).
  - Preserved unchanged: `REQ-PRIV-020`, `REQ-PRIV-021`, `REQ-PRIV-023`, `REQ-EVT-008`, `REQ-EVT-009` (Event access); `REQ-PLAN-005`, `REQ-PLAN-006` (Plan audience and notes).
  - No requirement is changed.
- **Model tier:** `DEEP`. `PRP-030` routes server reads and authorization-adjacent routing to `DEEP`. The code change is small. The tier reflects that it adds a read path to private Family items.
- **Phase 9 exit criteria served:** 6 (generic unavailable: "the server responses for both cases are indistinguishable") for Events and Plans, server half; 14 (authorization preserved; no new authorization path). It is a prerequisite for 4 (item URLs) and 5 (authorized cross-Family deep link), which the later client item delivers.
- **Standing auto-approval:** `DEC-032` does **not** apply. This is `DEEP` work that exposes private items through a new endpoint, so it has a privacy and permissions effect. Explicit owner approval and execution authorization were given on 2026-10-02.

## Goal

An authenticated user can read **one** Event or **one** Plan by its identifier alone, without naming a Family:

- `GET /api/events/{eventId}`
- `GET /api/plans/{planId}`

The server finds the item and applies **the same visibility rule the Family-scoped list already uses**. It returns the item only when that rule allows it. The response includes the item's `FamilyGroupId`, so the later client deep-link flow can switch the selected Family **after** authorization (`DEC-074` D4).

A missing item and an unauthorized item give the same response: `404 Not Found` with an empty body. This matches the existing Trip, Concern, and Incident item reads.

Nothing else changes: no client change, no change to the list APIs' behavior, no change to who may see or edit an Event or Plan.

## Owner decisions (2026-10-02)

The project owner approved this item and authorized its execution against `CTX-1.27` with the following decisions. They control wherever the rest of this item is less strict.

### D1 — Endpoints: approved

`GET /api/events/{eventId}` and `GET /api/plans/{planId}`. The item ID alone is the request. `FamilyGroupId` is **not** part of the route. The server finds the item and authorizes the caller before any Family context is exposed.

### D2 — Uniform unavailable response: approved

`404` with an **empty body** for every externally unavailable case, including:

- a missing item;
- an empty or invalid ID, where the current API conventions give the same outcome (`Guid.Empty` in the service; a non-GUID fails the `:guid` route constraint);
- a caller not in the audience;
- a Family Administrator not otherwise authorized;
- a caller whose membership has ended;
- a creator who has left the Family;
- a caller who belongs to another Family;
- a caller whose membership was restored, or who rejoined, but who does not satisfy the existing access rule.

Missing and unauthorized stay indistinguishable. No `403`, no differing error bodies, no Family metadata, and no existence hints. `REQ-PRIV-027` controls.

### D3 — Event access rule: preserve exactly

The caller has an **active membership in the Event's Family** **and** is its **creator** or a **selected audience member**. Administrator status grants no access by itself. `CanEdit` stays: creator, or selected editor. Do not broaden or reinterpret Event access.

### D4 — Plan access rule: preserve exactly

The caller has an **active membership in the Plan's Family** **and** is its **creator**, **or** the Plan is shared with the **whole Family**, **or** the caller was **individually selected**. A creator who leaves the Family loses access. Administrator status grants no access by itself. Do not broaden or reinterpret Plan access.

### D5 — Authorization reuse: approved, behavior first

One private, reusable access query per repository, used by both the Family-scoped list and the single-item read. No generic policy framework. Do not change behavior just to reduce duplication. **If moving a list onto the shared query changes any externally observable result, stop and report before continuing.** The Design § 4 fallback may be used only with the owner's agreement after that report.

### D6 — List-equivalence proof: required

For every relevant test user and item combination, the single-item result equals that item as returned by the Family-scoped list when authorized. If the list excludes the item, the item read is unavailable. This is the key regression guard against authorization drift (`AC-05`, `AC-10`).

### D7 — `GetCreatedEventAsync`: do not reuse

It hard-codes `CanEdit: true` and assumes creator or editor semantics, so it must not serve normal item reads. Use the read projection with the existing `CanEdit` rule.

### D8 — Family context: approved

Reuse `FamilyEventView` and `FamilyPlanView` unchanged. `FamilyGroupId` is sufficient; the client resolves an already-authorized `FamilyGroupId` through its existing Family list. Do not add a Family name, audience lists, member details, or routing-only private metadata.

### D9 — Plan create `Location` header: unchanged

Do not change it in this item. It stays a recorded [follow-up](#follow-ups). Do not broaden the API-contract changes.

### D10 — Audit: none added

No denied-read audit for Events or Plans. The canonical denied-read audit rules apply to Concerns and Incidents only. If implementation finds a contrary canonical requirement, stop and report.

### Further owner direction

- **No client work:** no Angular item routes, detail-page changes, Home item links, selected-Family switching, Concern URL work, Trip or Incident route changes, visual changes, or navigation changes.
- **No schema or domain change:** no schema, migration, Event or Plan lifecycle, audience, administrator, or create/edit change. A domain entity may change only if an existing projection mechanically requires it with no behavior change. None is expected, and no DbContext change is expected.
- **SQL baseline first:** before implementation, run the .NET baseline with `RUN_FAMILYSAFETY_SQL_TESTS=1`, prove the SQL-backed tests executed rather than returning early, and record the total tests, the SQL-gated test count, and pass/fail/skip counts (`TASK-001`, `AC-22`). If SQL tests cannot run, stop and report before claiming verification.
- **Do not commit or push.**

## Current implementation (verified 2026-10-02, static review)

### Events

| Layer | Current state |
|---|---|
| Endpoints | `src/FamilySafety.Api/Events/FamilyEventEndpoints.cs`. The group is `/api/family-groups/{familyGroupId:guid}/events`, with `GET` (list), `POST` (create), and `PUT /{eventId}` (update). There is **no single-item `GET`**. `CurrentAccountUnavailableException` → `401`; `FamilyEventUnavailableException` → `404` (empty body). |
| Service | `FamilyEventService.GetVisibleAsync(familyGroupId)`: `Guid.Empty` → unavailable; requires the current account; a `null` repository result → unavailable. |
| Repository | `EfFamilyEventRepository.GetVisibleAsync(accountId, familyGroupId)`. (1) It finds the caller's **active** membership in that Family (`GetActiveProfileIdAsync`). If there is none, it returns `null`, which becomes `404`. (2) It lists Events in that Family where the caller's profile is the **creator** or a row in **`FamilyEventAudienceMembers`**. (3) `CanEdit` = creator, or an audience row with `CanEdit`. |
| Read model | `FamilyEventView(EventId, FamilyGroupId, Name, Description, Date, Time, Location, OrganizerProfileId, OrganizerDisplayName, CanEdit)`. It contains no audience list, no editor list, and no Family name. |
| Domain | `FamilyEvent` (with `FamilyGroupId` and `CreatorProfileId`) and `FamilyEventAudienceMember(EventId, ProfileId, CanEdit)`. |
| Caution | `GetCreatedEventAsync` (used after create and update) hard-codes `CanEdit: true`. It is correct only for those callers and **must not** be reused for the item read. |

**Event visibility rule, as implemented:** the caller has an active membership in the Event's Family **and** is either its creator or a selected audience member. An ended membership removes access, and a restored one restores it, because the audience row persists. Family Administrator status grants nothing (`REQ-PRIV-021`).

### Plans

| Layer | Current state |
|---|---|
| Endpoints | `src/FamilySafety.Api/Plans/FamilyPlanEndpoints.cs`. The group is `/api/family-groups/{familyGroupId:guid}/plans`, with `GET` (list) and `POST` (create). There is **no single-item `GET`**. Status mapping is the same as for Events. Note: `POST` returns a `Location` of `/api/family-groups/{id}/plans/{planId}`, a URL that does not exist today. |
| Service | `FamilyPlanService.GetVisibleAsync(familyGroupId)`: same guards as Events. |
| Repository | `EfFamilyPlanRepository.GetVisibleAsync(accountId, familyGroupId)`. It checks for an active membership (none → `null`). It then lists Plans in that Family where the caller is the **creator**, **or** `AudienceType == FamilyGroup`, **or** the caller has a row in **`FamilyPlanAudienceMembers`**. |
| Read model | `FamilyPlanView(PlanId, FamilyGroupId, ActivityOrDestination, Date, Time, ExpectedReturn…, ExpectedCheckIn…, AudienceType, CreatorProfileId, CreatorDisplayName, Notes)`. It contains no recipient list and no Family name. |
| Domain | `FamilyPlan` (with `AudienceType`: `FamilyGroup` or `SelectedPeople`) and `FamilyPlanAudienceMember(PlanId, ProfileId)`. |

**Plan visibility rule, as implemented:** the caller has an active membership in the Plan's Family **and** is its creator, **or** the Plan is whole-group, **or** the caller was individually selected. A creator who has left the Family loses access, and the Plan stays visible to current members (`REQ-PLAN-005`). Family Administrator status grants nothing.

**The rules differ:** Plans have a dynamic whole-group audience and no edit concept; Events have only selected people plus a per-person `CanEdit`. Each item read keeps its own domain's rule.

### Precedent: existing item reads

| Item | Item read | Missing / unauthorized | Family context in response |
|---|---|---|---|
| Trip | `GET /api/trips/{tripId}` | `404`, empty body | `FamilyGroupId` and `FamilyGroupName` |
| Concern | `GET /api/concerns/{concernId}` | `404`, empty body; denied-read audit summary (`DEC-069`) | `FamilyGroupId` |
| Incident | `GET /api/incidents/{incidentId}` | `404`, empty body; denied-read audit summary (`REQ-PRIV-026`) | `FamilyGroupId` |

All three item reads are top-level (no Family in the path), resolve the Family on the server, and use one unavailable result.

### Audit

Denied-read audit is required **only** for Concerns (`REQ-PRIV-025`, `DEC-069`) and Incidents (`REQ-PRIV-026`). No Event or Plan requirement asks for read or denied-read audit, and the Event and Plan code records none. `REQ-PRIV-022` (Trips) explicitly declines a generalized audit subsystem. **This item adds no audit or logging.** Adding it would be new privacy behavior and needs its own decision.

## Design

### 1. Endpoints

| Endpoint | Success | Every other case |
|---|---|---|
| `GET /api/events/{eventId:guid}` | `200` + `FamilyEventView` | `404`, empty body |
| `GET /api/plans/{planId:guid}` | `200` + `FamilyPlanView` | `404`, empty body |

- **Top-level paths follow the Trip, Concern, and Incident convention.** A Family-scoped path such as `/api/family-groups/{f}/events/{id}` would make the client name a Family before the item is authorized. That is the opposite of `DEC-074` D4.
- Each endpoint is mapped in the **existing** `FamilyEventEndpoints` / `FamilyPlanEndpoints` class, next to the list, with the same `try` / `catch` mapping. No new endpoint class is added.
- `401` is returned only when there is no current account (the existing `CurrentAccountUnavailableException` mapping). That check happens before any item lookup, so it reveals nothing about the item.
- A non-GUID identifier does not match the `:guid` route constraint, so it gets the framework's `404`, as it does for Trips, Concerns, and Incidents. `Guid.Empty` is rejected in the service as unavailable.

### 2. One unavailable result

Missing, unauthorized, `Guid.Empty`, caller with no active membership in the item's Family, caller who left the Family, and a Family Administrator outside the audience all produce **`404` with an empty body**, the same status, headers, and body. The service throws the existing `FamilyEventUnavailableException` / `FamilyPlanUnavailableException`. The repository returns `null` for every one of these cases and never tells them apart.

- No `403`, no problem-details body, no "no access" wording, no Family identifier, and no different error message.
- **Same query for every case:** the item read is **one** database query. It does not first look up the item and then check access, so a missing item and an unauthorized item run the same SQL. This avoids an extra round trip that would only happen when the item exists.

### 3. Family context in the response

- The response is the **existing** `FamilyEventView` / `FamilyPlanView`, **unchanged**. Both already carry `FamilyGroupId`, which is all the client needs to switch Family after authorization.
- **No `FamilyGroupName` is added.** The client already has the names of the caller's own active Families from `GET /api/family-groups/mine`, and an authorized item is always in one of those Families. Adding the name would change a list DTO as well, for no gain. This differs from the Trip view, which already includes the name; the difference is intentional and harmless.
- No audience list, editor list, recipient list, member data, or administrator data is added.
- The item response for a caller is **identical** to that item's entry in the same caller's Family-scoped list, including `CanEdit` for Events.

### 4. Authorization reuse: one visibility query per domain

Each repository gets **one private query** holding its existing visibility rule. Both the list and the new item read use it, so there is only one definition of Event visibility and one of Plan visibility.

```csharp
// EfFamilyEventRepository, illustrative only
// The caller's active Family memberships joined to the Events visible to them in that Family.
// This is the only place Event read visibility is defined. The list and the item read both use it.
private IQueryable<FamilyEventView> QueryVisibleEvents(Guid accountId) =>
    from membership in dbContext.FamilyGroupMemberships.AsNoTracking()
    where membership.AccountId == accountId && membership.IsActive
    join familyEvent in dbContext.FamilyEvents.AsNoTracking()
        on membership.FamilyGroupId equals familyEvent.FamilyGroupId
    join creator in dbContext.AdultProfiles.AsNoTracking()
        on familyEvent.CreatorProfileId equals creator.Id
    where familyEvent.CreatorProfileId == membership.AdultProfileId
        || dbContext.FamilyEventAudienceMembers.Any(audienceMember =>
            audienceMember.EventId == familyEvent.Id
            && audienceMember.ProfileId == membership.AdultProfileId)
    select new FamilyEventView(/* same fields and CanEdit rule as today */);
```

- **List (`GetVisibleAsync`):** it keeps its existing active-membership pre-check, so a non-member still gets `null` → `404` and a member with nothing visible still gets an empty list. It then uses `QueryVisibleEvents(accountId).Where(view => view.FamilyGroupId == familyGroupId)` with the same ordering. The API contract and behavior do not change.
- **Item (`GetAsync`):** `QueryVisibleEvents(accountId).Where(view => view.EventId == eventId).SingleOrDefaultAsync()`.
- **Plans:** the same shape, as `QueryVisiblePlans(accountId)`, with the creator, whole-group, or selected-recipient rule and the existing `ToView` mapping.
- The query shape is plain LINQ inside each repository. **No shared policy class, no generic authorization framework, no expression-composition library, and no new package.** Events and Plans keep separate queries because their rules differ.
- If EF Core cannot translate the projection directly into the record constructor, keep today's anonymous-type-then-map pattern. Either way, the rule is written once per repository.
- **Fallback if the list refactor proves risky:** leave the list query as it is and add the item query beside it, with a code comment pointing at the list predicate. The equivalence tests in `AC-05` and `AC-10` then guard against drift. Use this only if the shared query changes list SQL behavior or ordering. Record it in the VER as a deviation.

### 5. Layers touched

| Layer | Event | Plan |
|---|---|---|
| Repository interface | `Task<FamilyEventView?> GetAsync(Guid accountId, Guid eventId, CancellationToken)` | `Task<FamilyPlanView?> GetAsync(Guid accountId, Guid planId, CancellationToken)` |
| Service interface and service | `GetAsync(eventId)`: `Guid.Empty` → unavailable; require the current account; `null` → unavailable | the same |
| Endpoint | `MapGet("/api/events/{eventId:guid}", GetAsync)` | `MapGet("/api/plans/{planId:guid}", GetAsync)` |

No domain, DbContext, migration, DI registration, or `Program.cs` change is expected. The existing `Map…Endpoints` calls already wire each endpoint class.

## Scope

### In scope

- `GET /api/events/{eventId}` and `GET /api/plans/{planId}` as designed above.
- A shared private visibility query in each of the Event and Plan repositories, used by both the list and the item read.
- Service and repository methods for the item reads.
- Tests: service, API, and SQL-backed persistence tests, including list/item equivalence.
- The verification record and context sync at closure.

### Explicitly excluded

- **All client work:** Angular item routes, item detail pages, Home item links, Concern URL conversion, Trip or Incident route changes, selected-Family switching, navigation changes, visual changes, and cross-feature transitions. These belong to the later `REQ-UX-014` client item.
- Changing the list endpoints' paths, request or response shapes, status codes, or ordering.
- Changing Event or Plan create or update behavior, validation, or responses, **including** the `Location` headers returned by `POST` (see [Follow-ups](#follow-ups)).
- Any change to audience semantics, edit access, administrator authority, membership rules, or privacy boundaries.
- Adding `FamilyGroupName`, audience lists, or any other field to `FamilyEventView` or `FamilyPlanView`.
- Read or denied-read audit or logging for Events or Plans.
- Trip, Concern, and Incident item reads. Their nondisclosure is confirmed in the later client item, as `PRP-030` M3 says.
- Schema migrations, new packages, generalized authorization or policy frameworks.
- Playwright or browser work. This is server-only, and no client calls the new endpoints yet.
- Event RSVP, Needs, Updates, or any Phase 9.5 capability.

## Acceptance checks

1. `AC-01` — **Authorized Event read.** `GET /api/events/{id}` returns `200` with the Event for its creator, for a selected viewer (`canEdit: false`), and for a selected editor (`canEdit: true`). (SQL persistence + API tests.)
2. `AC-02` — **Event unavailable is uniform.** Each of the following returns `404` with an empty body: a random nonexistent ID; `Guid.Empty`; an active member of the Event's Family who is not in its audience; a Family Administrator of that Family who is not in its audience; a selected audience member whose membership has ended; the creator after their membership in the Event's Family has ended; an active member whose membership was ended and restored but who was never the creator or in the audience; and a user who belongs only to a different Family. Repository: `null` for every case. API: the status, body, and `Content-Type`/`Content-Length` headers are identical for the "missing" and "unauthorized" cases.
3. `AC-03` — **Event cross-Family read.** A user who is an active member of two Families, and is in the audience of an Event in Family B, can read it by ID alone, and the response's `familyGroupId` is Family B. No Family identifier is sent in the request.
4. `AC-04` — **Event membership changes.** A selected member whose membership ended gets `404`. When the membership is restored, the same member can read the Event again, matching the list.
5. `AC-05` — **Event visibility equals the list.** In a fixture with several callers and Events across two Families, for every caller and every Event, `GetAsync` returns a view **equal** (record equality, including `CanEdit`) to that Event's entry in `GetVisibleAsync(caller, event.FamilyGroupId)`, and returns `null` exactly when the Event is absent from that list or the list is `null`.
6. `AC-06` — **Authorized Plan read.** `GET /api/plans/{id}` returns `200` with the Plan, including `notes`, for: the creator; any active member when the audience is `FamilyGroup`, including a member who joined after the Plan was created; and a selected recipient when the audience is `SelectedPeople`.
7. `AC-07` — **Plan unavailable is uniform.** `404` with an empty body for: a nonexistent ID; `Guid.Empty`; an active member who is not selected on a `SelectedPeople` Plan; a Family Administrator who is not selected on a `SelectedPeople` Plan; an ended member of a whole-group Plan's Family; the creator after leaving the Family; an active member whose membership was ended and restored but who is not selected on a `SelectedPeople` Plan; and a user only in another Family. Repository returns `null`; the API responses for "missing" and "unauthorized" are identical.
8. `AC-08` — **Plan cross-Family read.** As `AC-03`, for a Plan in the caller's second Family.
9. `AC-09` — **Plan membership and creator departure.** After the creator leaves, the Plan stays readable by a current member of a whole-group Plan and is unavailable to the departed creator. An ended member who is restored can read it again. Each result matches the list.
10. `AC-10` — **Plan visibility equals the list.** As `AC-05`, for Plans (whole-group and selected-people), with record equality including `Notes`.
11. `AC-11` — **Minimum Family context, no extra data.** The JSON property set of each item response is exactly the property set of the same type's list entries: Events `eventId, familyGroupId, name, description, date, time, location, organizerProfileId, organizerDisplayName, canEdit`; Plans `planId, familyGroupId, activityOrDestination, date, time, expectedReturnDate, expectedReturnTime, expectedCheckInDate, expectedCheckInTime, audienceType, creatorProfileId, creatorDisplayName, notes`. There is no Family name and no audience, editor, recipient, or member list. `FamilyEventView` and `FamilyPlanView` are unchanged in the diff. `familyGroupId` appears only in a `200` response. Every unavailable response has an empty body (`AC-02`, `AC-07`), so no Family identifier is exposed before authorization.
12. `AC-12` — **No new authorization path.** Each repository defines Event (or Plan) read visibility in **one** place, used by both the list and the item read (or, under the recorded fallback, the equivalence tests in `AC-05`/`AC-10` pass). No new policy class, framework, package, or audience rule is added. Administrators gain nothing. Verified by diff review.
13. `AC-13` — **Lists unchanged.** `GET /api/family-groups/{id}/events` and `/plans` keep their paths, response shapes, ordering, and status codes. A non-member still gets `404`; a member with nothing visible still gets `200 []`. All existing Event and Plan list persistence tests pass unmodified.
14. `AC-14` — **Create and update unchanged.** The create, update, and validation code paths and their `Location` headers are unchanged in the diff. All existing Event and Plan service, API, and persistence tests pass with no assertion changed. A test stub that only gains the new interface method is the one permitted change to an existing test file's setup.
15. `AC-15` — **Service guards.** For both services, `GetAsync(Guid.Empty)` throws the domain's unavailable exception without calling the repository; a missing current account throws `CurrentAccountUnavailableException` (→ `401`); a `null` repository result throws the unavailable exception; the current account ID and the item ID are passed to the repository unchanged.
16. `AC-16` — **No client or UI change.** No file under `src/FamilySafety.Web/` changes.
17. `AC-17` — **No schema or dependency change.** No migration, DbContext model change, `.csproj` package change, or `package.json` change.
18. `AC-18` — **Build and tests pass with SQL.** `dotnet build` succeeds with no new warnings, and the full `dotnet test` passes with `RUN_FAMILYSAFETY_SQL_TESTS=1`. The VER record gives the counts before and after **and evidence that the new SQL persistence tests actually executed** (they return early and report as passed without the gate; `claude-handoff/VALIDATION.md` § 4).
19. `AC-19` — **Regression.** Every pre-existing .NET test still passes, including the Trip, Concern, and Incident denied-access tests. The Angular unit tests and Playwright are not required, because no client file changes (`AC-16`). The VER record states this.
20. `AC-20` — **Records updated.** `VER-WI-009-004.md` maps every check to evidence. At owner closure: this item, `project-status.md`, `roadmap.md`, the README checkpoint paragraph, and `claude-handoff` state files are updated, and the two new endpoints are recorded for the later client item. There is no separate API reference document in the repository, so the endpoints are documented in this item and the VER record.
21. `AC-21` — **Owner review.** The item cannot close without owner review of `VER-WI-009-004`.
22. `AC-22` — **SQL baseline before implementation.** Before any code change, `dotnet test` runs with `RUN_FAMILYSAFETY_SQL_TESTS=1`. The VER record gives:
    - the exact command;
    - total, passed, failed, and skipped counts;
    - the number of SQL-gated test methods, counted in source by their gate check;
    - evidence that they executed, such as their names with per-test durations at normal verbosity, compared with an ungated run in which they return almost instantly.

    The same evidence is recorded again after implementation (`AC-18`).

## Tasks

- [x] `TASK-001` — **SQL baseline (`AC-22`).** Before any code change:
  1. Run `dotnet build FamilySafety.slnx`.
  2. With `RUN_FAMILYSAFETY_SQL_TESTS=1`, run `dotnet test FamilySafety.slnx --no-build --logger "console;verbosity=normal"` (`claude-handoff/VALIDATION.md` § 4).
  3. Record the total, passed, failed, and skipped counts and the SQL-gated test-method count from source.
  4. Record proof that the gated tests executed: their names and per-test durations, compared with an ungated run.

  If SQL tests cannot run in this environment, stop and report (see [Stop conditions](#stop-conditions)).
- [x] `TASK-002` — **Event visibility query.** In `EfFamilyEventRepository`, add `QueryVisibleEvents(accountId)` with today's predicate and `CanEdit` rule. Move `GetVisibleAsync` onto it, keeping the membership pre-check and ordering. Run the existing Event persistence tests with the SQL gate: they must pass unchanged. Any change in an externally observable list result is a stop condition (D5).
- [x] `TASK-003` — **Event item read.** `IFamilyEventRepository.GetAsync`, `EfFamilyEventRepository.GetAsync` (one query), `IFamilyEventService.GetAsync`, `FamilyEventService.GetAsync` (guards), and `MapGet("/api/events/{eventId:guid}")` in `FamilyEventEndpoints`.
- [x] `TASK-004` — **Plan visibility query.** As `TASK-002`, with `QueryVisiblePlans(accountId)` in `EfFamilyPlanRepository`. Existing Plan persistence tests must pass unchanged with the SQL gate; D5 applies.
- [x] `TASK-005` — **Plan item read.** As `TASK-003`, with `MapGet("/api/plans/{planId:guid}")` in `FamilyPlanEndpoints`.
- [x] `TASK-006` — **Service tests.** Add `GetAsync` guard tests (`AC-15`) to `FamilyEventServiceTests` and `FamilyPlanServiceTests`. Stub repositories gain the new method only.
- [x] `TASK-007` — **API tests.** In `FamilyEventApiTests` and `FamilyPlanApiTests`: `200` with the exact property set (`AC-11`); `404` with an empty body for unavailable; identical responses for two unavailable IDs; `401` with no current account; non-GUID → `404`; existing list route still answers. Stub services gain the new method only.
- [x] `TASK-008` — **SQL persistence tests (Events).** In `FamilyEventPersistenceTests`: the authorized and unavailable matrix (`AC-01`, `AC-02`, administrator included), cross-Family (`AC-03`), end and restore (`AC-04`), and the list/item equivalence sweep (`AC-05`).
- [x] `TASK-009` — **SQL persistence tests (Plans).** In `FamilyPlanPersistenceTests`: `AC-06` to `AC-10`, including the newcomer, creator-departure, and administrator cases.
- [x] `TASK-010` — **Regression and diff review.** Full `dotnet build` and `dotnet test` with the SQL gate. Diff review for `AC-11` to `AC-14`, `AC-16`, and `AC-17`.
- [x] `TASK-011` — **Record and hand back.** Write `VER-WI-009-004.md` (`SELF-VERIFIED` unless independent review is available); set this item to `VERIFYING / OWNER REVIEW`. At owner closure only: update `project-status.md`, `roadmap.md`, the README checkpoint, and `claude-handoff` state files.

## Expected files

| File | Change |
|---|---|
| `src/FamilySafety.Api/Events/FamilyEventEndpoints.cs` | Add the item `GET` |
| `src/FamilySafety.Api/Plans/FamilyPlanEndpoints.cs` | Add the item `GET` |
| `src/FamilySafety.Application/Events/IFamilyEventService.cs`, `FamilyEventService.cs`, `IFamilyEventRepository.cs` | Add `GetAsync` |
| `src/FamilySafety.Application/Plans/IFamilyPlanService.cs`, `FamilyPlanService.cs`, `IFamilyPlanRepository.cs` | Add `GetAsync` |
| `src/FamilySafety.Infrastructure/Persistence/EfFamilyEventRepository.cs` | Shared visibility query; list uses it; item read |
| `src/FamilySafety.Infrastructure/Persistence/EfFamilyPlanRepository.cs` | The same |
| `tests/FamilySafety.Foundation.Tests/FamilyEventServiceTests.cs`, `FamilyEventApiTests.cs`, `FamilyEventPersistenceTests.cs` | New tests; stubs gain the method |
| `tests/FamilySafety.Foundation.Tests/FamilyPlanServiceTests.cs`, `FamilyPlanApiTests.cs`, `FamilyPlanPersistenceTests.cs` | The same |
| `ai-context/planning/verification/VER-WI-009-004.md` | **New** |
| This work item | Task checkboxes and implementation record |

Not expected: any `FamilySafety.Domain` file, `FamilySafetyDbContext.cs`, migrations, `DependencyInjection.cs`, `Program.cs`, `*.csproj`, anything under `src/FamilySafety.Web/`, `e2e/`, or `scripts/`. Needing any of them is a stop condition.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md`, `project-status.md` | Mandatory operating rules and checkpoint | Entry |
| This work item | Scope authority | Entry |
| `core/privacy-safety-contract.md#req-priv-027` | Nondisclosing unavailable result; authorization before disclosure | Before execution |
| `core/ux-contract.md#req-ux-014`, `decision-log.md#dec-074` (D4) | Why the read is top-level and returns `FamilyGroupId` | Before execution |
| `core/privacy-safety-contract.md#req-priv-020`, `#req-priv-021`, `#req-priv-023` | Event access rules to preserve | Before execution |
| `workflows/plans-trips.md#req-plan-005`, `#req-plan-006` | Plan audience and notes rules to preserve | Before execution |
| `claude-handoff/WORKING-RULES.md`, `VALIDATION.md` § 4, `SESSION-CLOSEOUT.md` | Test cadence, SQL gate, closeout | Before execution |
| The source and test files in [Expected files](#expected-files); `FamilyTripEndpoints.cs` and `EfFamilyTripRepository.GetAsync` as the item-read precedent | Implementation surface | Before execution |

## Context intentionally not loaded

| Context | Why |
|---|---|
| `workflows/safety.md`, `REQ-SAFE-015/016`, `REQ-PRIV-025/026` in detail | Concern and Incident reads are not touched; their audit pattern is not reused, because Events and Plans have no audit requirement |
| `REQ-EVT-010` to `REQ-EVT-013` (RSVP, Needs, Updates) | Phase 9.5; not implemented and not exposed |
| `REQ-PRIV-022`, Trip workflow | Trip read already exists; not changed |
| Angular app, `e2e/`, `claude-handoff/reference/` | No client work |
| Phase 5–8 stories and VER records, bootstrap, archive | Not relevant |

## Stop conditions

Stop and report, without guessing, if execution would require:

- any change to Event or Plan audience semantics, edit access, membership rules, or administrator authority;
- an item result that differs from the caller's list result for the same item;
- moving a list onto the shared access query changing any externally observable list result (D5);
- a canonical requirement for Event or Plan read or denied-read audit (D10);
- distinguishing missing from unauthorized in status, body, headers, or message;
- adding any field to `FamilyEventView` or `FamilyPlanView`, or returning Family names, audience, or member data;
- new audit, logging, or other privacy behavior;
- a schema migration or DbContext model change;
- any client, routing, or Family-switching change;
- a generalized authorization or policy framework, or any new package;
- changing list, create, or update behavior or responses (including `Location` headers);
- SQL-backed tests that cannot be run with `RUN_FAMILYSAFETY_SQL_TESTS=1` in the execution environment. Without them, `AC-01`–`AC-10` cannot be shown to `PASS`.

## Risks

| Risk | Mitigation |
|---|---|
| The list refactor changes list behavior (ordering, the `null`-versus-empty distinction, `CanEdit`). | Keep the membership pre-check. Existing list tests must pass unchanged before the item read is added (`TASK-002`, `TASK-004`). Fallback in Design § 4. |
| Item and list visibility drift apart later. | One shared query per repository, plus the equivalence sweeps in `AC-05` and `AC-10`. |
| A mistaken reuse of `GetCreatedEventAsync`, which hard-codes `CanEdit: true`. | Called out in the design; `AC-01` checks that a viewer gets `canEdit: false`. |
| SQL-gated tests silently "pass" without the gate. | `AC-18` requires proof that they executed. |
| EF Core translation of the membership-join query differs from today's SQL. | Persistence tests run against SQL Server, not an in-memory provider. |
| A timing difference between missing and unauthorized. | One query in both cases (Design § 2). Residual timing variance is not tested and is accepted as below the MVP threat model. |

## Follow-ups

These are recorded only. Neither is in scope.

- **Plan `POST` `Location` header** points at `/api/family-groups/{id}/plans/{planId}`, which does not exist. After this item, `/api/plans/{planId}` will exist. Changing the header is a create-response change, so it is excluded here. The owner may approve a one-line correction in a later item. Event `POST` returns the list URL; that is unchanged.
- **Trip, Concern, and Incident nondisclosure confirmation** stays with the later client item, as `PRP-030` M3 says.

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner
- **Authorized at:** 2026-10-02
- **Authorized scope:** WI-009-004 only, against `CTX-1.27`, as bounded by this item and its [Owner decisions](#owner-decisions-2026-10-02):
  - the two item-read endpoints;
  - the shared access query in the Event and Plan repositories;
  - their service and repository methods;
  - the tests;
  - the verification and closure records.

  Not authorized: any client, schema, migration, domain-behavior, audit, dependency, or create/update change, and any commit or push.

## Implementation record (2026-10-02)

- **Baseline first (`TASK-001`):** before any code change, `RUN_FAMILYSAFETY_SQL_TESTS=1 dotnet test FamilySafety.slnx --no-build --logger "console;verbosity=normal"` gave 191 total, 191 passed, 0 failed, and 0 skipped. Source has 51 SQL-gated methods, each taking 0.8–6 s gated and under 1–20 ms ungated (suite: 29.5 s against 3.4 s).
- **Design as written.** `QueryVisibleEvents(accountId)` and `QueryVisiblePlans(accountId)` are the single read-visibility definitions. Both lists moved onto them, keeping the membership pre-check and ordering keys, and the six existing list tests passed unmodified, so the Design § 4 fallback was not needed. Each item read is one query against the shared query. Each query projects into a private query-side row class so that EF Core can filter and order on it. `GetCreatedEventAsync` is not used by the item read.
- **Endpoints:** `GET /api/events/{eventId:guid}` and `GET /api/plans/{planId:guid}` return the unchanged `FamilyEventView` / `FamilyPlanView` (`200`), `404` with an empty body for every unavailable case, and `401` without a current account.
- **Tests:** 20 new tests: 8 service, 8 API, and 4 SQL persistence, including list/item equivalence sweeps (161 comparisons, 0 mismatches). After implementation: build with 0 warnings; 211/211 passed with the SQL gate, and the 4 new SQL tests ran 2–5 s gated against ≤ 1 ms ungated.
- **Unchanged:** no client, schema, migration, `DbContext`, domain, dependency, audit, create/update, or `Location` header change.
- **Deviations:** the test stubs gained the small state their new method reads, plus `Reset()` lines in the API stubs. Each change only adds lines; no existing line changed. See the VER record.

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| `AC-01`–`AC-19`, `AC-22` | `VER-WI-009-004.md` | `PASS` (`SELF-VERIFIED`) |
| `AC-20` | `VER-WI-009-004.md`; this record; closure-time status sync of `project-status.md`, `roadmap.md`, the README checkpoint, and the `claude-handoff` state files (2026-10-02) | `PASS` |
| `AC-21` | Owner review 2026-10-02: `PASS` | `PASS` |

- **Verification report:** `planning/verification/VER-WI-009-004.md`
- **Independence:** `SELF-VERIFIED`; owner reviewed
- **Outcome:** `DONE / PASS` — `OWNER APPROVED`

## Owner closure (2026-10-02)

The project owner reviewed `VER-WI-009-004` on 2026-10-02. The result is `PASS`, and the owner accepted the item as implemented. `AC-21` is `PASS`, and `AC-20` is complete with the closure-time status sync. All 22 acceptance checks are `PASS`. No tests were rerun for closeout; the accepted evidence is the recorded verification run.

- **Accepted evidence:**
  - the SQL-gated baseline: 191/191;
  - proof that the SQL-backed tests executed;
  - the final suite: 211/211, with 55 SQL-gated methods;
  - the targeted Event and Plan tests;
  - 161 list/item equivalence comparisons with 0 mismatches;
  - both item endpoints;
  - unchanged Event and Plan list, create, and update behavior;
  - no client, schema, migration, dependency, audit-policy, or authorization-expansion change.
- **Accepted deviations (non-blocking, test-only):**
  1. The test stubs gained the minimum state that the new `GetAsync` methods need, including the `Reset()` cleanup in the API stubs.
  2. Two `using` directives were added to each API test file.
- **Accepted non-blocking issues** (no work item created):
  1. HTTP-level missing-versus-unauthorized equivalence is exercised through the existing stub-service API-test pattern. The SQL tests and the common `null` → `404` path prove the end-to-end repository and service behavior.
  2. No dedicated multi-item ordering regression test exists. The ordering keys are unchanged, and the existing list tests pass.
- **Follow-up kept, not scheduled:** the Plan create `Location` header ([Follow-ups](#follow-ups)). No work item was created for it.
- **For the next item:** `GET /api/events/{eventId}` and `GET /api/plans/{planId}` are available to the later `REQ-UX-014` client item, alongside the existing Trip, Concern, and Incident item reads.
