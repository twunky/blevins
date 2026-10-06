# FamilySafety.Web

Angular browser client for the Family Safety, Coordination & Planning application.

The client provides Home, Family and Family Settings, Events, Plans & Trips, Safety, and Me / Settings. It includes the implemented invitation, Concern, and Safety Incident screens within those areas.

In `src/app`, `app.ts` and `app.html` still implement Home, Family, Events, Plans, Me / Settings, and first use. Standalone feature code lives in `concerns/`, `incidents/`, `trips/`, `safety/`, and `plans-trips/`; navigation and cross-feature item routing remain at the app root. `events/event-participation` owns WI-095-001 invitation/self-RSVP UI and protected requests; `events/event-needs` owns WI-095-002 Need UI and protected requests, refreshing after participation changes. `events/event-updates` owns WI-095-003 visible organizer Update UI and protected requests; hidden text and revisions never enter its normal projection. App supplies an already authorized Event and retains Event create/detail/edit, dashboard discovery, and authorize-first routing/Family adoption.

WI-095-009 reuses `/api/family-groups/{familyId}/event-invitations/mine` for live Home attention as well as Events discovery. Home displays only the server-authorized current profile's eligible unrevoked Pending invitations, including past dates, and links to Event detail for RSVP. Family switches clear/cancel pending reads; returning/reloading Home reads current state. No notification persistence, inbox or outbound delivery is added.

`check-ins/check-ins` owns WI-095-005 standalone creation and sent/shared history in Safety. App supplies creation Family context and current profile; every history projection uses server-authorized author/recipient access, independently of selected Family. Status/note/recipients are fixed at creation.

`sharing-sessions/sharing-sessions` owns WI-095-006 temporary own-location sharing in Safety and the Home active-sharing indicator. Family/profile inputs reset drafts, protected reads and browser watches. Explicit fixed recipients, a chosen expiration within 24 hours and stop use server authorization. Browser geolocation is permission-based and collected only while the sharing UI is active; author may re-enable updates after returning. Updates are limited to one minute, recipient reads refresh every 15 seconds, and local expiration removes displayed points without waiting for a read. No coordinates are persisted in browser storage or kept as history.

## Structure and extraction

WI-105-002 keeps bounded Plan activity/timing correction in the existing App detail. The action requires the server's authorized creator hint plus current creator/active stored-Family membership display checks. A prefilled seven-field form submits a complete replacement with its captured Guid token to `/api/plans/{planId}/activity-timing`; conflict requires reload/review. Notes remain a separate form/request. Success updates current detail/list state; normal Home/list loads read current facts using existing date-based relevance. Expected Check-In remains a recorded planned time only.

`households/households` owns WI-095-010 Family Settings organization reads/forms, server-derived management controls and versioned create/rename/assign/move/unassign/delete requests. It cancels pending requests and clears organization/drafts/confirmations on Family or identity changes. Existing active adult members may view their Family's structure; current Family administrators manage it. Households supply no feature audience, grants, profile contact access or administrative override.

`trusted-contacts/trusted-contacts` owns WI-095-007 private manual contact list/forms and owner-only requests in Safety. App supplies only the current owner profile; there is no feature Family input/selector, contact profile lookup/link or downstream permission grant. Identity changes cancel pending requests and discard contacts/drafts. Add/edit use owner-scoped duplicate validation and optimistic concurrency; confirmed removal permanently deletes the record.

`family-dates/shared-family-dates` owns WI-095-004 date list/forms, protected requests, server-derived actions and context reset. App supplies the selected Family/profile and retains the rest of the Family experience. Profile birthdays are not copied into date entries.

This is the current rule for organizing `src/app`:

- **A folder is a real boundary.** Create a folder only when it holds a genuine ownership or implementation boundary, such as `concerns/`, `incidents/`, `trips/`, `safety/`, or `plans-trips/`. Do not create feature folders for symmetry.
- **No dumping grounds.** Do not create `shared/`, `core/`, `common/`, `utils/`, or `features/` folders. Keep a cross-feature helper at the app root under a name that says what it does, as `navigation.ts`, `item-route.ts`, and `date-format.ts` do.
- **`App` concentration is not a defect by itself.** Home, Family, Family Settings, Events, Plans, Me / Settings, and first use stay in `App` until there is a concrete reason to move them.
- **Extract incrementally.** Move a feature out of `App` as part of an authorized work item, when that feature's growth creates a real boundary: for example, its own state, API calls, and tests that no longer depend on `App`'s shared Family-scoped state. Do not run broad `App` decomposition as a cleanup project.
- **Family-scoped state in `App`.** If you add Family-scoped state to `App`, also reset it in `clearFamilyScopedState()` so a Family switch never shows another Family's data.

See the [repository README](../../README.md) for the project overview, [local development](../../docs/local-development.md) for setup and test-artifact guidance, and [AI context](../../ai-context/README.md) for authoritative product and project rules.

`safety-circles/safety-circles` owns WI-095-008 private owner-only Family presets and member management. `circle-recipient-preset` fetches fresh authorized members only when optionally requested in Check-In/Temporary Sharing creation; users review adjustable checkboxes and submit profile IDs to unchanged explicit-grant APIs. Identity/Family changes discard data and cancel reads. Owner membership loss deletes the circle, ordinary member loss removes only that member; rejoin does not restore either.
