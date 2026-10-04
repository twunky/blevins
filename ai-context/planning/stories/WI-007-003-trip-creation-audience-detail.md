# WI-007-003 — Trip Creation, Audience, and Detail Viewing

## Metadata

- **Work Item ID:** `WI-007-003`
- **Status:** `DONE`
- **Phase:** Phase 7 — Plans, Trips & Sharing
- **Epic:** MVP Trips
- **Context baseline:** `CTX-1.18` — approved and active
- **Pinned context dependency:** None beyond active `CTX-1.18`
- **Model tier:** `DEEP`

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md` | Work mode, authority, scope, and verification rules | Entry |
| `ai-context/project-status.md` | Active phase, baseline, current objective, and blockers | Entry |
| `ai-context/planning/roadmap.md#phase-7` | Confirms Phase 7 status and completed Plan work | Before proposal and execution |
| `ai-context/planning/proposals/PRP-020-trip-context-reconciliation.md` and `ai-context/decision-log.md#dec-057` | Accepted Trip decisions and implementation authorization boundary | Before proposal and execution |
| `ai-context/planning/verification/VER-CTX-1.18.md` | Confirms the Trip context is active, internally consistent, and compatible with Plan/Event behavior | Before execution and verification |
| `ai-context/workflows/plans-trips.md#req-trip-001` through `#req-trip-009`, `#req-trip-011`, and `#req-trip-012` | Trip definition, fields, roles, contacts, workflow, editing, and non-goals | Before execution and verification |
| `ai-context/workflows/plans-trips.md#req-trip-010` | Boundary only: Trip Notes and moderation are explicitly excluded from this item | Before execution and verification |
| `ai-context/core/privacy-safety-contract.md#req-priv-004` through `#req-priv-006`, `#req-priv-011`, `#req-priv-020`, and `#req-priv-022` | Location-sharing separation, permission boundaries, creator exception, and Trip audience lifecycle | Before execution and verification |
| `ai-context/workflows/sharing-checkins.md#req-share-004` and `#req-share-009` | Prevent Trip access from granting Sharing Session or another person's Check-In access | Before execution and verification |
| `ai-context/core/product-contract.md#req-prod-013` | Keeps minor and guardian behavior outside the MVP | Before execution and verification |
| `ai-context/skills/execute-story/SKILL.md` | Requires authorization, bounded execution, and evidence collection | Before execution |
| `ai-context/skills/verify-story/SKILL.md` and `ai-context/templates/verification.md` | Governs the separate verification pass and evidence matrix | Before verification |
| Existing Event and Plan application entry points named in Implementation Areas | Reuse established project patterns without importing Event or Plan product rules | During execution |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Safety coordination workflows | This item creates no Concern, Safety Incident, escalation, or emergency behavior. |
| Trip Notes implementation details | Notes and moderation require a later work item. `REQ-TRIP-010` is loaded only as a boundary. |
| Sharing Session and Check-In execution beyond `REQ-SHARE-004` and `REQ-SHARE-009` | This item must preserve their separation but does not implement them. |
| Plan proposals, Plan lifecycle decisions, and Event access rules | Plan and Event rules do not define Trip behavior. Existing code may be inspected only for technical patterns. |
| Bootstrap, archive, superseded proposals including `PRP-019`, and unrelated historical verification | `CTX-1.18`, `DEC-057`, and `PRP-020` are the active authority. |
| Minor and guardian design material | The capability is outside MVP under `REQ-PROD-013`. |

## Objective

Implement the smallest useful Trip foundation that lets an eligible creator create a Trip with travelers, an explicit initial audience, and optional informational contacts; lets authorized people view its details; and lets only the creator edit the approved core details while preserving the original audience.

## User and business value

- Families can record and share a structured Trip without enabling location sharing or other safety capabilities.
- Travelers, viewers, contacts, and the creator remain distinct so access is explicit and privacy preserving.
- The resulting Trip aggregate, API, persistence, and UI form a stable base for separately approved Notes, audience editing, Check-Ins, Sharing Sessions, and completion work.

## Requirement references and traceability

| Requirement | Application in this work item |
|---|---|
| `REQ-TRIP-001`–`REQ-TRIP-003` | Establish a structured MVP Trip and the supported information subset. |
| `REQ-TRIP-004` | Implement only the creation portion through initial audience selection; later travel, Check-In, sharing, arrival, and completion steps remain excluded. |
| `REQ-TRIP-005` | Boundary only; no progress updates, arrival action, or Sharing Session expiration is implemented. |
| `REQ-TRIP-006` | Preserve Trip MVP exclusions. |
| `REQ-TRIP-007` | Enforce creator, traveler, viewer, and editor distinctions; preserve creator access after membership loss. |
| `REQ-TRIP-008` | Enforce required/optional fields, date order, one destination, traveler minimum, and product-defined audience modes. |
| `REQ-TRIP-009` | Implement informational contacts and their permission limits. |
| `REQ-TRIP-010` | Boundary only; do not implement Trip Notes or moderation. |
| `REQ-TRIP-011` | Store/display optional expected arrival only; do not add Check-In execution, completion, automatic Concern, or sharing behavior. |
| `REQ-TRIP-012` | Implement the minimal Overview → Create → Detail flow without requiring a review screen. |
| `REQ-PRIV-004`–`REQ-PRIV-006` | Trip participation, relationship, and administration do not imply private-data or location access. |
| `REQ-PRIV-011` | Preserve the narrow Trip creator exception without restoring other Family Group access. |
| `REQ-PRIV-020` | Require an explicit item audience while deferring to the narrower Trip rules. |
| `REQ-PRIV-022` | Enforce one-group or selected-people audience, snapshots, membership revocation, no automatic restoration, and creator access. |
| `REQ-SHARE-004`, `REQ-SHARE-009` | Keep Trip visibility separate from Sharing Session and another person's Check-In data. |
| `REQ-PROD-013` | Limit eligible people to the approved adult MVP model; introduce no minor or guardian behavior. |

## Functional scope

### Create Trip

The creator can create a Trip in one applicable Family Group with:

**Required**

- Trip title
- One primary destination
- Start date
- End date
- At least one eligible traveler
- One explicit initial audience mode and its required recipient selection

**Optional**

- Description
- Transportation notes
- Accommodation notes
- Expected arrival
- Zero or more informational non-system contacts

The create operation persists the Trip, travelers, initial audience snapshot, and contacts as one consistent operation. A partial Trip must not remain when creation fails.

### Travelers

- Each traveler is an existing active adult member of the Trip's applicable Family Group when selected.
- At least one traveler is required after duplicate identifiers are normalized or rejected.
- The creator may be included as a traveler but is not required to be one.
- Traveler status does not grant Trip visibility, editing, Trip Notes, Check-In data, or Sharing Session data.

### Initial audience

- The creator selects exactly one product-defined mode: `FamilyGroup` or `SelectedPeople`.
- `FamilyGroup` stores a snapshot of the eligible active adult recipients at creation.
- `SelectedPeople` stores only the eligible active adult members explicitly selected from that same group.
- Multiple groups, a combined group-plus-selected-people audience, non-system viewers, and admin-configured audience modes are rejected or unavailable.
- The creator's access is an independent creator privilege and does not depend on inclusion in the stored audience snapshot.
- Post-creation audience editing is absent from the API and UI in this item.

### Non-system contacts

- The creator may add, edit, or remove informational contacts while creating or editing the Trip.
- Each contact requires a non-blank name and may include relationship/role, phone, and email.
- Contacts are returned only as part of an authorized Trip view.
- A contact record creates no account, invitation, login, Trip access, note access, Check-In access, or Sharing Session participation.

### View Trip details

- Provide a minimal Trips Overview from which an authorized user can enter Create Trip and open a visible Trip.
- Provide a Trip Detail view containing title, primary destination, start/end dates, creator, travelers, and audience context with role-specific disclosure:
  - The creator sees the exact audience selection; Selected People includes recipient identities, and group mode includes the selected Family Group.
  - A non-creator viewer sees the audience mode and, for group mode, the Family Group label. For Selected People, show a simple label such as “Shared with selected people”; do not return or display recipient identities.
- Show description, transportation notes, accommodation notes, expected arrival, and contacts only when present.
- The detail response contains Trip data only. It contains no Trip Notes, Sharing Session data, or another person's private Check-In data.

### Expected Arrival

- Expected Arrival is optional and, when supplied, represents a date and time with explicit time-zone context.
- The UI presents the time-zone context clearly enough for the creator and authorized viewers to understand the intended arrival time.
- Expected Arrival remains distinct from Trip Start Date, Trip End Date, Trip completion, Check-In, and Sharing Session.
- This story defines semantic behavior and does not prescribe database storage representation.

### Edit Trip details

- Only the creator can edit title, description, dates, primary destination, travelers, contacts, transportation notes, accommodation notes, and expected arrival.
- Every edit revalidates required fields, date order, traveler count, traveler eligibility, and contact names.
- An ordinary Trip edit cannot replace, add to, remove from, refresh, or otherwise mutate the initial audience.
- A viewer or traveler receives no core edit right unless they are also the creator.
- The creator's own-Trip view and edit capability remains available after their eligible group membership ends, without restoring access to any other Family Group data.

## Validation rules

1. Title and primary destination must contain non-whitespace text.
2. Start date and end date are required, and end date cannot precede start date.
3. At least one distinct, eligible traveler is required on create and after every edit.
4. Every traveler must be an existing active adult member of the one applicable Family Group at the time they are added.
5. Audience mode must be exactly `FamilyGroup` or `SelectedPeople`.
6. A group audience must reference one valid applicable Family Group and accepts no separately supplied recipient list.
7. A Selected People audience must include at least one distinct eligible active adult recipient from that same group; creator access does not satisfy this minimum.
8. Title and primary destination are trimmed, non-empty, and length-bounded: title 100 characters and destination 200 characters, matching existing project conventions.
9. Description, transportation notes, and accommodation notes are optional plain text, limited to 1,000 characters each, matching the existing Plan Notes bound. Render as text and do not interpret markup.
10. Each non-system contact must have a trimmed, non-empty name (100-character limit, matching profile display names). Relationship/role is optional (100-character limit). Phone is optional, limited to 32 characters, and must preserve practical international formats without US-only parsing or normalization. Email is optional; when supplied, apply the existing 254-character maximum and project email-validation conventions (`EmailAddressAttribute` in .NET and Angular `Validators.email`).
11. Optional blank text values are normalized consistently and do not create misleading empty detail sections.
12. Server validation is authoritative. The UI mirrors the same user-correctable validation and displays field-specific errors.
13. Edit input contains no audience mutation. Supplied or over-posted audience changes are rejected or excluded by the request contract; they must never change stored access.

## Privacy and access criteria

- Creation requires an authenticated account with an eligible active adult profile in the applicable Family Group.
- A Trip can be read only by its creator or by a current eligible audience recipient.
- A saved audience recipient who loses eligible membership loses access immediately.
- Rejoining later does not restore access from the old snapshot.
- A person joining the group after a group-audience snapshot was created receives no access to that Trip.
- The creator retains access to their own Trip after membership loss. That exception grants no Family Group directory, profile, Plan, Event, location, Check-In, Sharing Session, or other group-data access.
- Traveler status alone and a non-system contact record grant no read or edit access.
- Unauthorized access must not reveal Trip contents or sensitive contact details. API behavior should follow the project's existing unavailable-resource pattern so it does not disclose whether an inaccessible Trip exists.
- Family Administrator status alone grants no access.
- Plan and Event audience behavior remains unchanged.

## Acceptance criteria

1. **`AC-01 — Successful creation`**: An eligible creator can create and persist a Trip containing every required field, at least one eligible traveler, one valid initial audience, and any supplied optional fields; the UI navigates to the created Trip Detail view.
2. **`AC-02 — Required-field validation`**: Missing or blank title/destination, missing dates, no traveler, or missing/invalid audience produces field-specific validation and persists no partial Trip.
3. **`AC-03 — Date validation`**: End date earlier than start date is rejected on create and edit; equal dates are accepted.
4. **`AC-04 — Traveler eligibility`**: Inactive, out-of-group, nonexistent, duplicate, minor/guardian, guest, or non-system-contact identifiers cannot satisfy the traveler requirement.
5. **`AC-05 — Creator/traveler independence`**: Creation succeeds when the creator is an eligible traveler and when the creator is not a traveler; neither choice changes creator access.
6. **`AC-06 — Group audience snapshot`**: A group audience references exactly one valid applicable Family Group and grants view access to the eligible active adult recipient snapshot at creation; later group joiners receive no access.
7. **`AC-07 — Selected-people audience`**: Selected People requires at least one distinct eligible recipient from the same applicable Family Group; creator access does not satisfy the minimum. Access is granted only to saved recipients plus creator privilege; unselected group members receive no access.
8. **`AC-08 — Audience context disclosure`**: The creator sees the exact audience selection, including selected recipient identities for Selected People or the chosen Family Group for group mode. A non-creator viewer sees the audience mode and group label for group mode; for Selected People the viewer sees only a generic label and receives no recipient identity list.
9. **`AC-09 — Unauthorized and traveler-only denial`**: An outsider, administrator without audience access, unselected group member, traveler who is not an audience member, and non-system contact cannot retrieve or edit the Trip.
10. **`AC-10 — Membership lifecycle and creator exception`**: Ending audience-member eligibility revokes access, rejoining does not restore it, and ending creator membership preserves creator-only access to view and edit that Trip without granting other group data.
11. **`AC-11 — Informational contacts`**: Optional contacts persist with a required trimmed name and optional relationship/role, phone, and email; authorized viewers can read them, while contacts receive no application capability and unauthorized responses disclose no contact data.
12. **`AC-12 — Optional fields and Expected Arrival`**: Description, transportation notes, accommodation notes, Expected Arrival, and contacts may be omitted; when supplied they persist and render on authorized Trip Detail. Expected Arrival contains a date, time, and explicit time-zone context that is clearly presented, and remains separate from Trip dates, completion, Check-In, and Sharing Session.
13. **`AC-13 — Text and contact validation`**: Title (100 characters), primary destination (200), optional plain-text Description/Transportation Notes/Accommodation Notes (1,000 each), contact name (100), relationship/role (100), phone (32), and email (254) follow the stated limits. Title, destination, and contact name are trimmed and non-empty. Phone accepts practical international formatting without US-only validation. Optional email follows existing project validation.
14. **`AC-14 — Creator editing`**: The creator can update every approved core field, travelers, and contacts; required-field, date, traveler, audience-invariant, and contact validation still applies. Non-creators cannot perform these edits.
15. **`AC-15 — Audience unchanged during ordinary edit`**: Core Trip edits preserve the exact audience mode and stored recipient snapshot. No audience-edit control or separately callable audience-mutation endpoint is introduced.
16. **`AC-16 — Detail presentation`**: Authorized viewers see the approved Trip fields, creator, travelers, contacts when present, and only the role-appropriate audience context; missing optional values do not create confusing empty content.
17. **`AC-17 — Sharing and Check-In separation`**: Trip create, view, and edit responses and UI expose no Sharing Session state, location data, or another person's protected Check-In data and start no such capability.
18. **`AC-18 — No automatic or deferred behavior`**: Creating, viewing, or editing a Trip does not complete it, change status automatically, raise a Concern, send an invitation, start a Sharing Session, require a Check-In, or create a Trip Updates/Notes feed.
19. **`AC-19 — Scope and regression`**: No Trip Notes, moderation, audience editing, multiple destinations, guest accounts, quick actions, advanced filters, or review screen is introduced; focused Plan regression proves `WI-007-001` and `WI-007-002` behavior is unchanged.
20. **`AC-20 — Evidence-backed completion`**: Verification maps every cited requirement and acceptance criterion to concrete evidence and a result, including SQL-gated persistence/access evidence and UI/build evidence.

## Out of scope / non-goals

- Post-creation audience editing, refresh, or replacement
- Trip Notes, note authoring, note editing/deletion, moderation, or retention mechanics
- Check-In creation, reminders, execution, display, or escalation
- Sharing Session creation, execution, location display, management, or expiration
- Trip completion and automatic status transitions
- Automatic Concern, Safety Incident, emergency, or notification behavior
- Multiple destinations, itinerary optimization, booking, expenses, or document management
- Admin-configured audience presets
- Multiple Family Groups or combined group-plus-selected-people audiences
- Invitations, login, guest accounts, or permissions for non-system contacts
- Trip chat, reactions, replies, or Trip Updates feed
- Separate Review & Create screen
- Advanced filters, destination cards, traveler avatars, status chips, and quick actions
- Ownership transfer
- Minor and guardian behavior
- Changes to Plan or Event behavior

## Dependencies

- Active and verified `CTX-1.18`, `DEC-057`, and `PRP-020`.
- Existing authenticated current-account accessor and active adult Family Group membership/profile data.
- Existing Family directory lookup pattern for eligible member selection.
- Existing .NET 10, EF Core/SQL Server, minimal API, Angular 22, xUnit, Vitest, and build/test conventions.
- Explicit owner approval of this work item and separate execution authorization because the item changes privacy and permissions and is `DEEP`.

## Assumptions

- One Trip belongs to one applicable Family Group in this MVP slice.
- Start and end are calendar dates as stated by `REQ-TRIP-008`; no start/end times are introduced by this item.
- The initial audience snapshot is persisted independently from current whole-group membership so later joiners cannot gain access.
- Stored audience membership must retain a permanent revocation effect after membership loss so rejoining does not restore prior access.
- The creator privilege is evaluated from the Trip's creator identity and is not implemented as a synthetic audience recipient.
- Existing unavailable-resource API behavior is used for inaccessible Trips rather than revealing existence through permission-specific detail.
- Technical constraints such as safe text lengths, trimming, and database column sizes must be explicit and consistent across UI, API, domain, and persistence before execution, but they must not redefine product meaning.

## Proposed implementation areas likely affected

The exact names may be adjusted during execution to match established conventions. Likely areas are:

| Area | Likely files or additions |
|---|---|
| Domain | New `src/FamilySafety.Domain/Trips/` aggregate and value/entity types for Trip, audience recipients, travelers, and informational contacts |
| Application | New `src/FamilySafety.Application/Trips/` contracts, service, repository interface, validation/error types, create/read/update operations |
| Infrastructure | New EF Trip repository; updates to `FamilySafetyDbContext.cs`, infrastructure dependency registration, and an additive EF migration/model snapshot |
| API | New `src/FamilySafety.Api/Trips/FamilyTripEndpoints.cs`; service and route registration in `src/FamilySafety.Api/Program.cs` |
| Web | Trip interfaces, state, create/edit forms, audience/traveler/contact selection, overview/detail behavior in the existing Angular application files unless a bounded component split is justified during implementation |
| .NET tests | New focused `FamilyTripServiceTests`, `FamilyTripInfrastructureTests`, `FamilyTripPersistenceTests`, and `FamilyTripApiTests` using the current xUnit patterns |
| Web tests | Focused additions to the existing Angular Vitest suite, or narrowly split Trip specs if implementation structure warrants it |
| Planning evidence | Status/evidence updates to this work item and a separate `VER-WI-007-003.md` after authorized execution |

## Tasks

- [x] **`TASK-001` — Execution readiness:** Confirm explicit approval and execution authorization, active `CTX-1.18`, and the required context manifest before implementation begins.
- [x] **`TASK-002` — Domain/application foundation:** Implement the Trip aggregate, contracts, validation, repository/service boundary, and creator-only update behavior.
- [x] **`TASK-003` — Persistence and access:** Add the additive schema and repository behavior for Trip, travelers, initial audience snapshot/lifecycle, contacts, authorized detail reads, and creator exception.
- [x] **`TASK-004` — API:** Add create, authorized overview/detail read, and creator-only core edit endpoints without an audience mutation operation.
- [x] **`TASK-005` — Web experience:** Add the minimal Overview → Create → Detail flow, creator editing, eligibility selection, contacts, validation, and privacy-safe detail rendering.
- [x] **`TASK-006` — Focused tests:** Add domain/service, persistence, API, authorization, UI, and Plan regression coverage listed below.
- [x] **`TASK-007` — Separate verification:** Move to `VERIFYING`, run the approved checks, and produce `VER-WI-007-003` using `verify-story`.

## Proposed test plan

### Domain and application behavior

- Required title, destination, dates, traveler, and audience validation.
- End-before-start rejection and same-day acceptance.
- Optional fields omitted and supplied.
- Contact name trimming and length; optional relationship/role and phone accepted; optional email follows existing validation.
- Text normalization and limits for title, destination, description, transportation notes, and accommodation notes; plain-text values render without markup interpretation.
- Duplicate/empty traveler and audience identifiers rejected or normalized without weakening minimum counts.
- Creator allowed both inside and outside traveler collection.
- Update preserves Trip identity, creator, Family Group, and audience.
- Edit revalidates the remaining traveler set and required fields.

### Repository and SQL persistence

- Atomic persistence of Trip, travelers, audience snapshot, and contacts.
- Group audience snapshot contains only eligible active adult recipients at creation.
- New group member cannot read an existing group-audience Trip.
- Selected audience allows only saved recipients.
- Audience member membership loss revokes access; reactivation/rejoin does not restore it.
- Creator can still read and edit own Trip after membership loss; unrelated group data stays unavailable.
- Traveler-only, administrator-only, outsider, and non-system contact access is denied.
- Contact information is returned only through an authorized Trip projection.
- Core edit updates approved fields and leaves audience rows and mode unchanged.
- Creator and non-creator detail projections apply the distinct recipient-identity disclosure rules.
- Expected Arrival requires date, time, and explicit time-zone context and is presented clearly without prescribing storage representation.
- Additive migration applies to a clean SQL test database and round-trips all required/optional values.

### API authorization and contracts

- Authenticated eligible creator receives success/created response for a valid request.
- Unauthenticated request is rejected using the current authentication convention.
- Validation failures return field-specific validation responses.
- Authorized creator and audience viewer can retrieve detail; unauthorized identities receive the current unavailable-resource response with no body leakage.
- Only creator can update core fields; viewer/traveler/administrator/outsider updates are denied.
- No create/read/update contract exposes Notes, Check-In, location, or Sharing Session properties.
- Over-posted or otherwise attempted audience changes cannot alter the saved audience.

### Angular UI

- Create form renders all required and optional fields in a clear, usable order.
- Client validation covers required fields, date order, Selected People minimum, and the specified length/email rules while server errors remain displayable.
- Traveler selection uses eligible directory members and supports creator selected or not selected.
- Audience control supports only Family Group or Selected People and cannot combine modes.
- Selected People requires at least one eligible selection; group mode references exactly one applicable Family Group.
- Optional contacts can be added/removed and validate required name.
- Successful save navigates to Trip Detail.
- Trip Detail renders required values and only present optional sections.
- Trip Detail applies creator/viewer audience disclosure rules and presents Expected Arrival's time zone clearly.
- Creator sees core edit capability; non-creator viewer does not.
- Ordinary edit sends no audience mutation and preserves the displayed access context.
- No Notes, sharing, Check-In, completion, quick-action, or review-screen UI is present.

### Regression

- Existing Family Plan service, infrastructure, API, SQL persistence, and Angular tests continue to pass.
- Existing Event and Family/Profile tests continue to pass as part of the full .NET suite.
- Explicit assertions confirm Trip snapshot audience logic did not replace dynamic whole-group Plan behavior.

## Verification plan

1. Create `VER-WI-007-003` from `ai-context/templates/verification.md` and declare verifier independence before evaluation.
2. Map every requirement reference and `AC-01` through `AC-20` to code/test/build evidence with `PASS`, `FAIL`, `BLOCKED`, or justified `NOT APPLICABLE`.
3. Run the focused and full .NET suites using the existing `FamilySafety.Foundation.Tests` project.
4. Run SQL-gated `FamilyTripPersistenceTests` with `RUN_FAMILYSAFETY_SQL_TESTS=1`; record the final test summary and any environment limitations. A skipped SQL gate is not passing persistence evidence.
5. Run the Angular Vitest suite and Angular production build from `src/FamilySafety.Web`.
6. Record the additive migration inspection/application evidence and confirm the current model snapshot matches the intended Trip schema.
7. Record targeted negative-access evidence for traveler-only, unauthorized, former audience member, rejoined member, and creator-after-membership-loss cases.
8. Record Plan regression evidence, including SQL-gated Plan persistence behavior when the environment supports it.
9. Update this work item to `DONE` only after the separate verification outcome is `PASS`. If the same agent verifies it, label the result `SELF-VERIFIED`.

Suggested commands, subject to the execution environment and existing project instructions:

```powershell
dotnet test tests/FamilySafety.Foundation.Tests/FamilySafety.Foundation.Tests.csproj --no-restore -p:NuGetAudit=false

$env:RUN_FAMILYSAFETY_SQL_TESTS = '1'
dotnet test tests/FamilySafety.Foundation.Tests/FamilySafety.Foundation.Tests.csproj --no-restore -p:NuGetAudit=false --filter FullyQualifiedName~FamilyTripPersistenceTests

Set-Location src/FamilySafety.Web
$env:NG_CLI_ANALYTICS = 'false'
npm test -- --watch=false
npm run build
```

## Definition of Done

- The work item has explicit owner approval and execution authorization under active `CTX-1.18`.
- The semantic Expected Arrival, audience disclosure, selected-recipient minimum, and item-level validation bounds in this work item are implemented as stated. Any new product ambiguity or conflict stops execution for review.
- All in-scope domain, application, persistence, API, and UI behavior is implemented and human-readable.
- An additive migration represents the approved Trip data and applies successfully in the SQL-gated test flow.
- Every acceptance criterion and cited requirement has recorded verification evidence.
- Focused Trip tests, the full .NET suite, Angular tests, Angular production build, and required Plan regressions pass.
- `VER-WI-007-003` reports `PASS`; limitations are disclosed; the work item is then marked `DONE`.
- No listed out-of-scope behavior or canonical context change is introduced.

## Risks and blockers

- **Privacy/access complexity:** Trip group audiences are snapshots, unlike the completed Plan whole-group behavior. Reusing Plan access logic would violate `REQ-PRIV-022`.
- **Membership lifecycle:** Revocation must survive a later rejoin. Checking only current membership at read time is insufficient unless persistence also records permanent removal from the original snapshot.
- **Creator exception:** Existing group-scoped repository patterns may require active membership before any read. Trip creator access needs a narrow own-Trip path that does not expose other group data.
- **Aggregate consistency:** Travelers, audience recipients, and contacts should save atomically to avoid partially authorized records.
- **Sensitive contact data:** Phone/email must never appear in unauthorized list/detail/error responses or logs.
- **Over-posting:** Core edit contracts must not bind or mutate audience data before the separate audience-edit work item.
- **Scope pressure:** Notes, completion, check-ins, sharing, quick actions, and audience changes are adjacent but require later authorization.
- **Verification environment:** SQL-gated tests may require the project owner's Visual Studio terminal as in prior work; skipped tests do not establish persistence compliance.

## Remaining ambiguity

- **Expected Arrival storage representation:** Product behavior is resolved as a date and time with explicit time-zone context. Existing architecture contracts do not prescribe its storage representation. Implementation may choose a representation that preserves the approved semantic value and clear display; no product ambiguity remains.
- **Numeric limits without a direct precedent:** The draft proposes 1,000 characters for each optional Trip plain-text field, matching Plan Notes, and 100 for contact relationship/role, matching display-name conventions. These are item-level validation bounds for this work item, not new cross-project canonical standards.
- **Phone format:** Reuse the existing 32-character maximum. Preserve practical international formats; no stricter phone parsing convention was found or is proposed.

No product ambiguity remains. The owner-approved Expected Arrival semantics, audience disclosure, selected-recipient minimum, and validation conventions are fully stated here.

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner
- **Authorized at:** 2026-09-27
- **Authorized scope:** Implement this bounded Trip creation, initial audience, detail, and creator core edit work item under active `CTX-1.18`; exclude all listed out-of-scope work.

## Evidence

| Requirement or criterion | Evidence | Result |
|---|---|---|
| Traceability to active Trip baseline | `CTX-1.18`, `DEC-057`, `PRP-020`, and `VER-WI-007-003` | `PASS` |
| Application behavior and all 20 acceptance criteria | `VER-WI-007-003` evidence matrix | `PASS` |
| Persistence, API, UI, regression, and build checks | SQL-gated full .NET suite: 99 passed; Angular suite: 18 passed; Angular production build passed; migration/model check passed | `PASS` |

## Verification

- **Verification report:** `ai-context/planning/verification/VER-WI-007-003.md`
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `PASS`
- **Verified at:** 2026-09-27
