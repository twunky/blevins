# PRP-019 — Trip Creation, Viewing, and Audience Contributions

## Metadata

- **Proposal ID:** `PRP-019`
- **Status:** `SUPERSEDED — owner decisions recorded in DEC-057; see PRP-020 and candidate CTX-1.18`
- **Date:** `2026-09-27`
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.17`
- **Required context:**
  - `ai-context/README.md` and `ai-context/project-status.md` — operating rules and current phase checkpoint
  - `ai-context/planning/roadmap.md` — Phase 7 status
  - `ai-context/workflows/plans-trips.md#req-trip-001` through `#req-trip-006` — Trip definition, MVP information, workflow, and exclusions
  - `ai-context/core/privacy-safety-contract.md#req-priv-006`, `#req-priv-019`, and `#req-priv-020` — administrator boundary, private contacts, item audience choice, and deferred behavior
  - `ai-context/workflows/sharing-checkins.md#req-share-001` through `#req-share-004` — sharing authorization is separate from Trip context
  - `ai-context/planning/stories/WI-007-001-plan-audience-creation.md` and `WI-007-002-plan-notes-at-creation.md` — completed Plan patterns; not automatic Trip authority
- **Affected authority:** Trip workflow and item access only if separately approved and canonically updated; this proposal makes no canonical change
- **Model tier:** `DEEP` (Trip details and traveler/audience privacy)

## Request

Define the first bounded Phase 7 Trip work item for creating and viewing Trip details, using approved Trip capabilities without assuming that Plan or Event-specific behavior applies to Trips.

## Project-owner direction to carry into review

The project owner has given the following direction for this proposal. These are not yet canonical requirements or implementation authorization:

- Audience choices should be configurable through a small set of administration-managed presets rather than being permanently hard-coded. The responsible admin role and configuration scope still need definition.
- External access is not included in this version. Only people who are users in the application may view a Trip.
- The Trip start and end dates are both required. Date-confidence labels and additional date notes are deferred to a future version to keep this version simple.
- Anyone included in the Trip audience may view it. The creator alone may edit the Trip record.
- Audience members may add information to appropriate, non-sensitive areas, but must not change important Trip details such as dates. The contribution areas and editing rules need definition.
- Keep the first version simple and suitable for user feedback; defer advanced behavior until feedback supports it.

The remaining questions below distinguish these directions from decisions that still need definition.

## Current evidence and context

- `REQ-TRIP-001` distinguishes a structured Trip from a lightweight Plan and includes multi-family trips as a possible Trip type.
- `REQ-TRIP-002` lists Trip information: title, travelers, destinations, dates/times, departure, expected arrival, itinerary, accommodations, transportation notes, companions, emergency contacts, check-ins, and sharing preferences.
- `REQ-TRIP-003` requires MVP support for title, travelers, destinations, dates/times, accommodations, transportation notes, companions, expected arrival, check-ins, and sharing settings.
- `REQ-TRIP-004` places item-audience choice before check-in preference and a separate location-sharing decision.
- `REQ-TRIP-005` describes progress through arrival and expiration of any associated Sharing Session according to its authorized duration.
- `REQ-TRIP-006` excludes booking, expense splitting, itinerary optimization, recommendations, and complex document management.
- `REQ-PRIV-020` requires an explicit Trip audience choice. It lists whole applicable Family Group, specific people, or both, while leaving recipient eligibility, multiple-group behavior, later audience changes, and membership-change effects undecided.
- The completed Plan workflow supports a bounded group-or-selected-people creation choice. Its audience lifecycle rules are Plan-specific. Event edit permissions are Event-specific. Neither is automatically a Trip rule.
- A Trip may provide context for a Sharing Session, but does not authorize location sharing; authorization must remain separate.

## Impact

Trips combine potentially sensitive travel plans, itinerary details, traveler information, and sharing preferences. The initial creation and viewing slice should make item access explicit, avoid implying that traveler status grants access, and avoid starting location sharing or sending notifications as an incidental effect. Trip traveler eligibility and audience behavior need owner decisions before an implementation work item can be approved.

## Mock-up alignment review

The project-owner mock-up (`trip-mockup.png`, provided 2026-09-27) is a design reference for this proposal. It is not, by itself, approval to implement every depicted control or permission.

| Mock-up element | Fit with approved context / owner direction | Proposed treatment |
|---|---|---|
| Trips overview with Upcoming/Past/All tabs, filters, destination cards, traveler avatars | A Trip list and detail view fit the Trip concept. Filter behavior, status labels, cover images, and date grouping are not defined by current Trip requirements. | Include a simple Trips list and Trip detail view in the first slice. Defer custom filters, cover images, and Planning/Not started lifecycle labels until separately specified. |
| Basic details: title, description, start/end dates, destination, transportation/accommodation notes | These mostly align with `REQ-TRIP-002` and `REQ-TRIP-003`; owner direction makes both start and end date required. The mock does not show field-level requiredness for remaining details. | Propose required title, one destination, start date, and end date; transportation/accommodation notes and description optional. This is proposed field scope, pending confirmation. |
| Family-member selection and “Trip members only” audience | Consistent with system-only access if “Trip members” means accounts explicitly included in the Trip audience. The connection between travelers and viewers is not defined. | Keep audience selection explicit and admin-preset driven. Proposed simple model: people selected as Trip members are viewers; traveler/companion labels are separate descriptive data and do not grant access. Confirm before work-item approval. |
| Add Non-System Contact with relationship, phone, and email; invited contacts can add notes | Conflicts with the owner's direction that only people in the system may view. Contact information is private by default, and its Trip use is undecided (`REQ-PRIV-019`). | Exclude non-system contact creation/invitation and all external view/contribution access from this version. Revisit contact records separately. Do not turn a typed name into an account or permission. |
| Expected arrival plus separate start/end dates | Expected arrival is named in Trip requirements. Its relationship to required start/end dates and time zones is not defined. | Defer expected-arrival time from this first slice. Required start/end dates are sufficient for the first feedback version; revisit expected arrival with later Trip progress/check-in work. |
| “Check-in & Sharing” enable control | `REQ-TRIP-004` requires a check-in preference and a separate location-sharing decision. A combined checkbox risks conflating check-ins with location-sharing consent. The current first-slice recommendation defers both behaviors. | Exclude this control from the first create flow. Propose check-in and location-sharing decisions as later, separate work. |
| Review & Create confirmation step | A review step is consistent with a guided creation flow but is not a current requirement. | Treat as optional UX polish; a single create/save action is enough for the bounded slice unless review is accepted as a design requirement. |
| Trip Notes panel where members and invited contacts add updates | Supports the owner's direction for audience contributions, but external contacts are excluded. “Notes” may contain sensitive free text. | Recommend a Trip Updates area for in-system audience members only, with add-only member contributions and creator edit/removal capability. Confirm whether this is part of the first slice and whether members can edit/remove their own entries. |
| Trip Summary Edit control | Aligns with the creator-edit-only direction, but editing the audience after creation is explicitly deferred by `REQ-PRIV-020`. | Creator may edit Trip details after creation. Recommend audience fixed after creation; changing it requires explicit decision and canonical alignment with REQ-PRIV-020. |

The mock visually includes photos, status chips, traveler/friend language, a non-system contact invite, and a check-in/sharing toggle. These should not be treated as first-version commitments unless individually accepted in this proposal and aligned with the approved privacy and workflow context.

## Proposed first work-item boundary

**Goal:** Let an eligible in-system user create a basic Trip with required dates and an explicit audience, let only the creator edit its Trip details, and let included audience members view it. A contribution area is a separately confirmable first-slice option, not assumed by the mockup alone.

The proposed initial Trip record covers:

- Required title, start date, end date, and one destination
- Optional description, accommodation notes, and transportation notes
- Optional traveler/companion descriptions, separate from access
- Explicit Trip audience choice

This is a staged first slice, not completion of all MVP Trip capabilities. Expected arrival, check-in behavior, sharing preferences, and Sharing Session execution require separate decisions/work items before those parts of `REQ-TRIP-003` are implemented. The owner directed that audience members can view and contribute to appropriate areas while only the creator edits the Trip record. This proposal recommends an add-only audience updates area, but that is still for confirmation. The creator's ability to change audience after creation remains deferred by `REQ-PRIV-020`; this proposal recommends keeping the audience fixed after creation for the first version.

## Questions requiring project-owner direction

1. **Admin configuration authority:** Which role manages audience presets, and are they application-wide or per Family Group? The current context does not define administration of these presets. Recommendation: define presets as allowed audience-choice modes, not administrator-selected recipients; configuration cannot grant its administrator access to Trip contents or override an individual's audience. This needs a new canonical rule because no current contract defines such configuration.
2. **Preset choices and eligible viewers:** Which presets should admins enable: whole applicable Family Group, selected people from that group, or both? Does “people in the system” mean active adult members of one applicable Family Group or any registered app user? Recommendation: for this simple version, restrict viewers to active adult members of one applicable group, with the creator selecting either the whole group or specific members. External contacts and users outside that group do not view Trips. This is a Trip-specific decision, not an automatic extension of Plan rules.
3. **Contact and traveler records:** Should non-viewer contact names be added as travelers/companions, and should the creator be the only person allowed to add them? Recommendation: keep contact/traveler listing separate from audience membership; a saved name without an eligible app account cannot view the Trip.
4. **Audience contributions:** What specific area may audience members add to? Recommendation: a separate “Trip updates” area where audience members can add entries but cannot edit or remove them; the creator can edit/remove contributions and all Trip fields. Contributions inherit the Trip audience. Confirm whether this area belongs in the first slice. This requires a Trip-specific canonical workflow rule; Plan notes do not authorize it.
5. **Dates and times:** Start and end dates are both required, as directed. Should departure and arrival times be optional or omitted from this first version? Date-certainty labels and date notes are deferred. Recommendation: require the two dates, make times optional, and omit confidence values and date notes for the simple first version.
6. **Other field requiredness:** Which of title, travelers, destinations, accommodations, transportation notes, and companions are required? Recommendation: require a title and at least one destination; keep other details optional. This remains a proposal choice.
7. **Editing audience after creation:** Does “creator can edit anything” include changing the audience after creation? `REQ-PRIV-020` currently defers later audience changes. Recommendation: keep audience fixed after creation for this version. If you want audience editing, explicitly approve changing this deferred behavior and define safeguards before implementation.
8. **Audience lifecycle:** How should membership changes affect whole-group or selected-person Trip access? Recommendation: leave these effects outside the first work item and do not infer Plan lifecycle rules for Trips. Keep the initial audience to one applicable Family Group; multi-group handling remains out of scope.
## Requirements proposed for the first slice

Subject to resolving the questions above:

- Create a Trip from the approved Trip details and selected audience.
- List and show saved Trips only to viewers allowed by the approved initial audience rules.
- Allow viewing by included audience recipients; only the Trip creator can edit Trip details and creator-managed fields.
- Keep traveler/contact listing separate from access unless a person is separately and explicitly included in the Trip audience.
- Require a start and end date; do not add date-confidence status or date notes in this version.
- If approved, let audience members add updates only in a separate contribution area; do not let them modify dates or other creator-managed core details. The creator can edit or remove contributions.
- Keep the Trip audience separate from travelers/companions and from any Sharing Session audience.
- Do not start location sharing, create a Sharing Session, or send notifications as a consequence of creating or viewing a Trip.
- Keep existing Trips without optional information valid where fields are approved as optional.
- Do not introduce multi-group handling or membership-change effects. Post-creation audience changes remain excluded unless explicitly approved and aligned with `REQ-PRIV-020`.

## Acceptance-check sketch for a later work item

1. A member can create a Trip with required start and end dates, other approved core details, and an explicit audience choice.
2. Optional details follow the approved validation rules; invalid data is rejected consistently at the API boundary.
3. A permitted viewer can retrieve the saved Trip details; unrelated and unauthorized viewers receive no Trip data.
4. Traveler/companion/contact designation does not broaden Trip visibility; audience membership is separately recorded.
5. Only included audience members can view; only the creator can edit Trip details and creator-managed fields.
6. If approved, audience members can add updates only in a separate contribution area and cannot change dates or other creator-managed fields. The creator can edit or remove contributions.
7. Creation, viewing, and contributions do not start location sharing, create a Sharing Session, or send notifications.
8. Focused service/API, persistence, access-control, and Angular checks map every acceptance criterion to evidence. Run SQL-gated persistence tests and Angular production build in the supported owner environment; record verification separately.

These checks are a draft. They are not implementation requirements until the owner resolves the questions and approves a bounded work item.

## Non-goals

- Trip cancellation, deletion, completion, or arrival lifecycle.
- Check-in preference, check-in events, missed-check-in handling, or notifications.
- Location sharing, Sharing Session creation, or live-sharing expiration.
- Multi-group audience or traveler handling, or post-creation audience changes unless explicitly approved and aligned with `REQ-PRIV-020`.
- Selected-recipient membership-change behavior or creator-departure access behavior.
- Event editing permissions or Plan-specific membership lifecycle rules.
- Booking, expense splitting, advanced itinerary optimization, travel recommendations, complex document management.
- Minor/guardian functionality, which remains outside MVP under `DEC-053`.

## Conflicts, unknowns, and decisions required

- **Context updates required before implementation:** (1) update the privacy contract to define Trip audience eligibility, one-group scope, the creator's creator-only edit permission, and the fixed-after-creation audience rule; (2) update the Trips workflow with approved required fields and view/edit rules; (3) if configurable presets are retained, define who configures only the available audience modes and make clear that administration grants no Trip content access; and (4) if audience updates are included, define their audience and add/edit/remove permissions. Update requirement indexes and baseline/status references after accepting those canonical changes.
- **Conflicts / unresolved scope:** The current `REQ-PRIV-020` leaves Trip viewer eligibility, later audience changes, and membership lifecycle undecided. This proposal's recommended one-group eligible audience and fixed audience after creation would resolve some of those for Trips and therefore need explicit approval plus canonical context changes. The mockup cannot itself settle them.
- **Unknowns:** Admin role/configuration scope; exact audience preset choices and eligible viewers; whether traveler display is included; whether Trip Updates belongs in this first slice; contribution edit/removal rules; optional versus required details; and how much of the approved Trip MVP to stage into later work items.
- **Decision required:** Answer or revise the eight questions above. Then prepare a bounded WI-007-003 with final acceptance checks and separate execution authorization. No implementation is authorized by this proposal.

## Options

### Option A — Stage Trip details first (recommended)

- **Description:** First decide the Trip audience, traveler model, date rules, and contribution area, then implement creation/list/detail viewing, creator-only editing, and limited audience contributions. Create separate work items for check-ins and sharing.
- **Benefits:** Delivers useful Trip coordination without coupling creation to live location or unresolved check-in behavior. Keeps sensitive audience and traveler decisions visible.
- **Risks/tradeoffs:** Several later work items will be needed to cover the full MVP Trip capability in `REQ-TRIP-003`.

### Option B — Define and implement the full Trip lifecycle together

- **Description:** Resolve Trip fields, audience, editing, check-in, and Sharing Session behavior in a larger proposal before implementation.
- **Benefits:** Defines one coherent Trip experience and avoids interim partial capability.
- **Risks/tradeoffs:** Larger, higher-risk scope crossing item permissions, notifications, check-ins, and location sharing; requires decisions that are currently open, including the persistent-sharing question in `OD-002`.

## Recommendation

Choose Option A. Keep required dates simple (start and end), defer date certainty and notes, keep external access out, and configure a small set of audience-choice presets. The creator controls Trip details; included audience members view the Trip. Add a Trip Updates area only if its add/edit rules are explicitly approved. Keep the audience fixed after creation in this version, consistent with the current deferral in `REQ-PRIV-020`. Preserve the separate explicit location-sharing decision required by `REQ-TRIP-004`; do not combine Trip creation with a Sharing Session. Approval of this proposal will not authorize implementation; a separate WI approval and execution authorization will be required.

## Verification plan

After approval and canonical alignment, an authorized work item should verify required/optional fields, audience-filtered reads, persistence and compatibility, safe display, and the absence of sharing/notification side effects. Use focused .NET tests, SQL-gated persistence tests, Angular tests, and a production build. Record each result in a separate `SELF-VERIFIED` or independent report, and disclose any environment limitation.

## Decision and authorization

- **Decision:** `SUPERSEDED`
- **Decision ID:** `DEC-057`
- **Approved by:** Project owner
- **Approved at:** 2026-09-27
- **Execution authorized:** `NO`; this proposal is for review only.

## Supersession

The project owner approved Trip reconciliation decisions in `DEC-057`. This proposal's unresolved recommendations are superseded by `PRP-020` and candidate `CTX-1.18`. It did not authorize application implementation.
