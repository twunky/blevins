---
Status: DONE / PASS — OWNER APPROVED
Authority: Project-owner approved and execution-authorized for WI-008-004 only (2026-10-01), against CTX-1.26; planning basis PRP-029 / DEC-071; consumes the WI-008-003 private API unchanged
Context-Baseline: CTX-1.26
Version: 1.1
---

# WI-008-004 — Incident UI and Start-an-Incident Flow

## Metadata

- **Work Item ID:** `WI-008-004`
- **Status:** `DONE / PASS` — `OWNER APPROVED` (owner visual review `PASS`, 2026-10-02). Implemented and verified against `CTX-1.26`; see `VER-WI-008-004`. It was approved and execution-authorized on 2026-10-01, with the UI decisions recorded in [Owner decisions](#owner-decisions--start-an-incident). See [Execution authorization](#execution-authorization).
- **Phase:** `8 — Safety Coordination`, `CURRENT / IN PROGRESS`
- **Context baseline:** `CTX-1.26`
- **Model tier:** `DEEP`. The work presents safety information, privacy boundaries, explicit access, and permanent history.
- **Approved planning basis:** `PRP-029` (`APPROVED`), § 22 "UI direction"; `DEC-071`; `DEC-070` (visual system). Canonical `REQ-SAFE-016` and `REQ-PRIV-026` control wherever wording differs.
- **Depends on:** `WI-008-003` (`DONE / PASS`, owner approved; `VER-WI-008-003`) for the Incident API. `WI-008-002` (`DONE / PASS`) for the Concern UI that gains the Start-an-Incident entry point.
- **Standing auto-approval:** `DEC-032` does **not** apply. This is `DEEP` work with privacy and safety presentation. Explicit owner approval and execution authorization were given on 2026-10-01, and no further routine approval is needed to execute it. Owner visual approval is still required at closure.
- **Phase 8 exit criteria served:** 2 (verified Incident workflow), 3, 4, 5, 6, 7, 8, 12, and 13. With this item verified, every Phase 8 exit criterion has implemented and verified evidence; phase closure remains a separate owner decision.

## Objective

Deliver the minimum complete Safety Incident workflow in the existing Angular application, on top of the verified WI-008-003 private API, so that an authorized adult can:

- find the Incidents shared with them;
- create an Incident directly, or start one deliberately from a Concern they created;
- read the Incident's header, people with access, timeline, tasks, and resolution;
- contribute findings, Unknowns, contact attempts, corrections, and challenges;
- work with tasks under the server's task rules;
- as coordinator, manage participants, hand off coordination, post summaries, and resolve;
- see `Frozen` and `Resolved` Incidents as read-only records;
- meet one generic unavailable state whenever the server says no.

The server remains the sole authorization authority. The UI uses returned data only to decide which controls to show.

## Story-size decision

Keep this as **one story**. The reasons:

- **The screens share one aggregate.** List, create, and detail all read the same `IncidentView`. Every contribution form posts to the same entries route and redraws the same timeline. Splitting them would leave no usable Incident workflow at the intermediate point. Phase 8 exit criterion 2 needs the whole journey verified end to end.
- **The rules interlock in the UI just as they do on the server.** Coordinator handoff changes which controls appear for participant management, tasks, summaries, and resolution. Freeze and resolution remove every control at once. These are verified together or not at all.
- **The Concern precedent fits this size.** WI-008-002 delivered list, create, detail, contribution, correction, challenge, participants, and resolution as one item. This item adds contact attempts, tasks, handoff, the timeline projection, and one Concern entry point. All of them reuse the same patterns.

**Contingency only.** If execution shows the item cannot be completed coherently, the smallest safe split is to move **tasks** (create, complete, cancel) into a follow-on UI item. Everything else, including the Start-an-Incident flow and the Playwright journey without its task steps, remains here. Do not split preemptively.

## Requirement traceability

| Canonical anchor | Applied in this item |
|---|---|
| `REQ-SAFE-016` | The complete MVP Incident workflow presented in the UI: creation, Start an Incident, coordinator, lifecycle, entries and provenance, contact attempts, tasks, timeline, resolution, subject membership loss. |
| `REQ-PRIV-026` | Explicit access, no automatic subject access, nondisclosing unavailable state, lineage visible only to viewers authorized for both records, no protected-domain data, no audit UI. |
| `REQ-PROD-004`, `REQ-PROD-009`, `REQ-PROD-012` | Manual creation only. The Start-an-Incident shortcut transfers no information or audience. No automatic danger, escalation, or dispatch. |
| `REQ-PRIV-005` through `REQ-PRIV-008`, `REQ-PRIV-011` | Administrator, relationship, and membership status never imply access in copy or controls. |
| `REQ-SAFE-001` through `REQ-SAFE-004`; `REQ-SAFE-010` | The five categories, provenance, first-class Unknowns, and append-only correction. |
| `REQ-SAFE-007`, `REQ-SAFE-008` (amended) | Incident content limited to the MVP fields. Only `Open` / `Frozen` / `Resolved` appear, with no deferred labels. |
| `REQ-SAFE-009`, `REQ-SAFE-011` through `REQ-SAFE-014` | Coordinator and participants, contact attempts, tasks (owner, status, result, completion time), timeline, and resolution content. |
| `REQ-SAFE-015`, `REQ-PRIV-025` | The Concern is unchanged by Start an Incident. Concern access rules are consumed, not changed. |
| `REQ-UX-001`, `REQ-UX-003` through `REQ-UX-006`, `REQ-UX-010` | Calm, family-first presentation; progressive disclosure; distinguishable categories; freshness; accessibility for less technical adults. |
| `REQ-UX-011` | The shell's current-Family selection is used to choose the Family Group for direct creation. It grants nothing. Navigation destinations are unchanged (`OD-001`). |
| `REQ-NOTIFY-001` through `REQ-NOTIFY-003` | Not implemented. No notifications, counts, or badges. |

`requirements-index.md` confirms that `REQ-SAFE-016` and `REQ-PRIV-026` are approved in `CTX-1.26`. This story proposes no new requirement.

## Required context manifest

| Context | Why | When |
|---|---|---|
| `ai-context/README.md`; `ai-context/project-status.md` | Operating rules and the authorization checkpoint | Entry |
| This story | Scope authority | Entry |
| `ai-context/workflows/safety.md#req-safe-016`; `ai-context/core/privacy-safety-contract.md#req-priv-026` | The controlling Incident workflow and access rules | Before execution and verification |
| `ai-context/core/ux-contract.md` (`REQ-UX-001`, `003`–`006`, `010`, `011`) | Calm presentation, categories, freshness, accessibility | Before execution |
| `ai-context/planning/stories/WI-008-003-safety-incident-domain-and-access-foundation.md` § "Authorization matrix", § "API scope"; `VER-WI-008-003.md` § "Implementation choices" | The server rules the UI must respect, including the two accepted narrow readings | Before execution |
| `src/FamilySafety.Api/Incidents/IncidentEndpoints.cs`; `src/FamilySafety.Application/Incidents/IncidentContracts.cs`; `IncidentService.cs` (validation only) | Exact routes, request and response shapes, field limits, validation keys | Before execution |
| `ai-context/planning/stories/WI-008-002-concern-mvp-user-interface.md`; `src/FamilySafety.Web/src/app/concerns/` | The approved Concern UI pattern to reuse, and the component that gains the entry point | Before execution |
| `src/FamilySafety.Web/src/app/app.routes.ts`, `app.ts`, `app.html`, `src/styles.scss` | Route-to-view pattern, shell title, Home card, shared visual tokens and primitives | Before execution |
| `claude-handoff/reference/*.png`; `claude-handoff/MOCKUP-ALIGNMENT.md`; `ai-context/decision-log.md#dec-070` | Primary visual target and the KEEP / ALIGN / ADAPT / DROP method | Before execution |
| `ai-context/references/ux/safety/safety-coordination.png` (panels 2 and 4 only) | Secondary layout hints for the Incident workspace and resolution, per PRP-029 § 22 triage | Before execution |
| `src/FamilySafety.Web/e2e/concern-workflow.spec.ts`; `e2e/fixtures/`; `e2e/playwright.config.ts` | Isolated scenario, Adult A/B/C identities, invitation setup, capture helper | Before E2E work |
| `claude-handoff/VALIDATION.md` §§ 1–3 and 5–7 | Validation commands and the rendered-review method | Before verification |
| `ai-context/templates/verification.md` | `VER-WI-008-004` structure | Before verification |

## Context intentionally excluded

- `.NET` persistence, migration, and SQL test internals beyond the contract files above. The server is complete and is not changed here.
- `PRP-029` sections other than § 22, and `PRP-028`. Their outcomes are canonical in `CTX-1.26`.
- Sharing, Check-In, location, Trip, Event, and Plan workflows, except as boundaries.
- Safety Circle and Trusted Contact material.
- Bootstrap, archive, and historical verification other than `VER-WI-008-002` and `VER-WI-008-003`.

## Scope

### In scope

- A typed Angular API client and models for all ten WI-008-003 Incident routes.
- Incident routes inside the existing `App` route-to-view pattern: list, create, and detail.
- The Incident list, direct creation, and creation started from a Concern.
- Incident detail: header, current summary, lineage reference, people with access, timeline, contribution forms, tasks, and resolution.
- Coordinator controls: participant management, coordinator handoff, summary updates, task assignment and cancellation, resolution.
- Participant controls: findings, Unknowns, contact attempts, own-entry corrections, challenges, self-assigned tasks, completing assigned tasks, cancelling own self-assigned tasks.
- `Open`, `Frozen`, and `Resolved` presentation; the retention notice; the generic unavailable state.
- A narrow Concern UI change: the **Start an Incident** action on Concern detail, and the ability to open a specific Concern when arriving from an Incident's lineage reference.
- A narrow Home change: an entry point to the Incident list from the existing Concern coordination card.
- Responsive layout, accessibility, component tests, one Playwright journey file, rendered evidence, and `VER-WI-008-004`.

### Explicitly excluded

- Server, API, permission, schema, or migration changes, and new endpoints. If a genuine contract defect is found, stop (see [Stop conditions](#stop-conditions)).
- A new permanent navigation destination, a changed destination count or order, or any mobile navigation treatment. `OD-001` stays open.
- `OD-002` behavior.
- Notifications, badges, counts, push, SMS, email, or calling; Call, Text, or Email buttons; any outbound communication.
- Emergency dispatch, external escalation, or "notify the safety team".
- Live or last-known location, maps, Check-In, Sharing Sessions, Trip, Event, or Plan data.
- Risk, danger, or severity scoring; urgency colors; "late" or "missed" timers.
- The deferred `REQ-SAFE-008` labels (Reviewing Information, Coordinating Response, and so on).
- A generic workflow engine, a generic task system, a shared Safety framework, or a refactor of the Concern component beyond the entry point.
- An audit viewer or denied-attempt display.
- Incident reopening, entry edit or delete, task edit or reassignment.
- Copying or selecting Concern entries into an Incident.
- Export, Share, "More actions", photos, subject age or school, and other DROP items listed below.
- Playwright full-suite database isolation. It remains a separate follow-up area; this item's journey runs independently.
- Backup-aging and restore evidence. It remains a predeployment dependency and does not block UI work.

## UI route plan

| Route | Screen | Notes |
|---|---|---|
| `/incidents` | Incident list | Directly addressable. Page title from the shell. |
| `/incidents/new` | Direct creation | Uses the shell's current Family. If no Family is selected, show the existing "choose a Family" guidance and disable submit. |
| `/incidents/new`, with router navigation state from Concern detail | Creation started from a Concern | See [Start an Incident from a Concern](#start-an-incident-from-a-concern). |
| `/incidents/:incidentId` | Incident detail | Deep-linkable. The ID is an opaque GUID. A `404` shows the generic unavailable state. |

Rules:

- All three routes resolve to the `incidents` view in the existing `App` shell. **No navigation button is added.** On these routes no destination is marked current, as on `/concerns` today.
- Moving between list, create, and detail must not reload the shell or refetch the profile and Family context. Use a single route definition (for example, a `UrlMatcher`) or an equivalent approach, and verify it.
- **No Concern content is placed in any URL.** The Start-an-Incident handoff uses router navigation state, which is not part of the URL. It carries only the Concern ID, its Family Group ID, and its subject profile ID. If that state is absent, as after a browser reload, `/incidents/new` falls back to direct creation with no lineage. That fallback is safe, because lineage is optional and non-authorizing.
- The lineage action back to a Concern navigates to `/concerns` with navigation state carrying the Concern ID. The Concern component opens that Concern through its existing `GET` and existing unavailable handling. No new Concern route is added.
- The page title for the `incidents` view is **"Safety Incidents"**, with a one-line subtitle such as "Coordinate a deliberate family response with the people you choose." There is exactly one `h1`, owned by the shell.

## Screen and state plan

### Incident list (`/incidents`)

**Data:** `GET /api/incidents` only. The response is `IncidentSummaryView[]`, ordered newest-created first by the server.

**Each row shows:**

- subject initials tile and subject display name;
- state as a text pill: `Open`, `Frozen`, or `Resolved`;
- "Coordinated by {name}", or "You coordinate this Incident" when `coordinatorProfileId` matches the current profile;
- Family Group name, only when the account has more than one active Family;
- "Created {date}" and "Last activity {date}" (`lastAuthorizedMutationAt`);
- for `Frozen` and `Resolved`: "Read-only · kept until {expiresAt}".

**Not shown:**

- The **reason**. `IncidentSummaryView` has no reason field. Do not fetch each detail to fill it, and do not request an API change. This is a recorded, non-blocking API fit note.
- **Lineage.** The list response never carries it, by design.
- Counts, urgency ordering, or anything that hints at Incidents the caller cannot see.

**Grouping:** two simple groups, with no filter controls. The first is **"Open coordination"**. The second is **"Ended coordination (read-only)"**, which holds `Frozen` and `Resolved` together and distinguishes them by their pills. Within each group, keep the server order. An empty group is omitted. This is simpler than filters and keeps the active work first.

**Header actions:** "Start an Incident" (primary, to `/incidents/new`, shown when a current Family is selected) and "Refresh list" (quiet).

**States:**

- **Loading:** `role="status"` "Loading Incidents…".
- **Empty:** "No Incidents are currently shared with you." with "An Incident is shared only with its coordinator and the people they add." Do not imply that the subject, the Family, or an Administrator can see an Incident.
- **Error:** neutral message with "Retry loading Incidents". A `401` shows the existing session message.

### Direct creation (`/incidents/new`)

The fields map one-to-one to `CreateIncidentRequest`.

| Field | Control | Rules |
|---|---|---|
| Subject | `select` "Who is this Incident about?" | Options from `GET /api/family-groups/{id}/members` plus the current profile as "{name} (you)", as in Concern. Required. Self is allowed. |
| Reason | `textarea` "Why are you starting this Incident?" | Required, 1–1,000 characters. Helper: "Describe what is uncertain, in your own words. This becomes the first coordinator summary." |
| Reason category | `fieldset` of radios: Confirmed, Reported, Assumption, Unknown | Required. Outdated is not offered; the API rejects it here. One-line helper per category. |
| Reason source | "Source description" text input, or the "Source is unknown" checkbox | Exactly one, up to 500 characters. Confirmed requires a described source, so the checkbox is disabled while Confirmed is selected. |
| People with access | `fieldset` of checkboxes | Active adults from the members response, **excluding the current user**. The API rejects a set that contains the creator. **None are preselected. The subject is never preselected.** Zero selections are allowed. |

**Privacy copy.** Shown beside "People with access", before submit, in calm wording:

> You will coordinate this Incident. Only you and the people you add can see it.
> **Choosing someone as the subject does not share the Incident with them.** If they should see it, add them under People with access.
> Family membership or administrator status alone does not give anyone access.

When the selected subject is another person and is not checked, add a short inline note next to their checkbox: "(subject, not added)". When checked: "(subject, added)".

**Submit:** "Create Incident". It is disabled while the request is in flight, while members are loading, and when no Family is selected.

**Outcomes:**

- **`201`:** navigate to `/incidents/{id}`. Focus the detail heading and announce "Incident created." When no participants were selected, show a short note in the People card: "Only you have access. Add people when you are ready."
- **`400`:** map the `ValidationProblem` keys (`Reason`, `ReasonCategory`, `ReasonSourceDescription`, `ParticipantProfileIds`, `SubjectProfileId`) to field-level, UI-owned messages. Never show raw server text. Keep the draft.
- **`404`:** "The Incident could not be created with the selected information. Refresh your Family members and try again." This is the Concern precedent, and it gives no reason.
- **Network or ambiguous failure:** do not retry. Refresh the list, and look for a match on subject, coordinator = me, and `createdAt` no earlier than 60 seconds before submit, as Concern does without the reason field. Offer "Review the list before creating another."

There is **no** optional initial entry at creation. The reason is the first summary entry. Further information is added on detail. This avoids the Concern two-request partial-success path.

### Start an Incident from a Concern

**Entry point:** on Concern detail, in the side column below "People with Concern access", a small card titled **"Separate Incident"**. It holds:

- one line of text: "If this needs coordinated family follow-up, you can start a separate Incident. Nothing here is copied or shared.";
- a **secondary** button: **"Start an Incident…"**.

**Visibility:** shown only when the current profile is the Concern's creator (`creatorProfileId` matches), in **any** returned state (`Open`, `Frozen`, or `Resolved`). This follows `REQ-SAFE-016`: the creator may start one while they currently have Concern access, and the server decides. Participants never see it. The button is a display hint; the server's create-from-Concern check is authoritative.

**Action:** navigate to `/incidents/new` with navigation state `{ originatingConcernId, familyGroupId, subjectProfileId }`.

**Creation form differences:**

- **Notice at the top**, in an information surface, not a warning:
  > **Starting a separate Incident**
  > This creates a new Incident. It does not change the Concern.
  > Nothing from the Concern is copied: no entries, history, sources, or people.
  > The Concern's participants are not added, and they will not see this Incident unless you add them.
  > Choosing someone as the subject does not share the Incident with them.

  The fourth line was added by the owner's `UI-D2` approval. The same message also stays beside "People with access".
- **Family:** the Concern's Family Group. Show its name. Members load for that group. This may differ from the shell's current Family; lineage requires the same group.
- **Subject:** prefilled with the Concern's subject and **changeable** before submit, as `REQ-SAFE-016` requires. If the Concern's subject is not among the active members returned, leave the subject unselected and show the normal "Choose a person" prompt.
- **Reason, category, source:** empty. Nothing is copied from the Concern.
- **People with access:** empty. No Concern participant is preselected, suggested, or marked.
- **Request:** the normal create request with `originatingConcernId` set.

**After `201`:** navigate to the new Incident's detail, as for direct creation. The Concern is unchanged.

**After `404`:** the same generic creation message as direct creation. Do not explain whether the Concern, its access, or the subject caused it.

### Incident detail (`/incidents/:incidentId`)

Use cards and sections, **not tabs**. Composition at desktop width, a main column plus a side column, mirrors the approved Concern detail:

**1. Header card** (full width)

- subject initials tile and the heading `h2` "Incident about {subject}";
- state pill (`Open` / `Frozen` / `Resolved`) and one-line state description;
- **Reason for starting:** the first `Summary` entry's text, with its category and source;
- **Current summary:** the latest `Summary` entry, with author and time, shown only when it is newer than the reason (canonical: "the latest is shown as current");
- "Coordinator: {name}" with "(you)" where applicable;
- "Started by {createdByName}" and "Created {date} · Last activity {date}";
- the lineage reference, when present (see [Lineage display](#lineage-display));
- for `Frozen` and `Resolved`, the read-only notice and the retention line.

**2. Resolution card** (full width, `Resolved` only). See [Resolved state](#resolved-state).

**3. Main column**

- **Timeline** card: the full read-time timeline. See [Timeline and history](#timeline-and-history).
- The active composer, when one is open: finding, Unknown, contact attempt, summary, correction, or challenge.

**4. Side column**

- **People with access** card, with coordinator controls. See [Participants](#participants).
- **Contribute** card (`Open`, and the current profile is a participant): action buttons for each allowed contribution.
- **Tasks** card. See [Incident tasks](#incident-tasks).
- **Coordination** card (`Open`, coordinator only): "Hand off coordination…" and "Resolve this Incident…".

At narrow width, the header, resolution, timeline, People, Contribute, Tasks, and Coordination cards stack in that reading order.

**Display hints.** All are display hints, never access control. They use only returned data and the current `/api/profile` `profileId`:

- `isCoordinator`: `coordinatorProfileId` matches.
- `isParticipant`: the profile appears in `participants`. The list includes the coordinator, flagged with `isCoordinator`.
- `canWrite`: `state === 'Open'` and `isParticipant`.

### Timeline and history

The timeline is the single chronological history. It is rendered from `timeline`, in **server order**, which is event time when known, otherwise recorded time, with the server's deterministic tie-break. Do not re-sort. Each item looks up its entry in `entries` by `entryId` or its task in `tasks` by `taskId`.

**Each item shows:**

- a **time column**. Primary: the event time when known. Otherwise, the recorded time, marked "Event time unknown".
- **both times** in the item meta: "Event time {t}" or "Event time unknown", then "Recorded {t}". `REQ-SAFE-016` requires both times to be shown.
- a **type label in text**: Finding, Unknown, Coordinator summary, Contact attempt, Task result, Correction, Another report, Task added, Task completed, Task cancelled, Person added, Person removed, Access ended, Coordination handed off, Coordination stopped, Resolved.
- the actor, or "System".
- for entry items: category pill text, with Outdated shown as "{original} · Outdated"; text; "Source: {description}" or "Source unknown"; explanation; and the related-entry link.
- a marker dot by **item type**, never by danger or category. No red.

**Item-specific text:**

- **Participant access ended:** "{name}'s access ended". **No reason**, because the server deliberately provides none.
- **Coordinator handoff:** "{from} handed coordination to {to}".
- **Frozen:** "Coordination stopped. This record is now read-only." The freeze reason is **not** displayed, following the WI-008-002 Concern precedent. Inactivity and coordinator access loss are both shown with the same neutral text, so nothing about anyone's membership is disclosed.
- **Resolved:** "{name} resolved this Incident".

The list is `<ol aria-label="Incident timeline, oldest first">`, and each time is a `<time datetime>`. A correction or challenge links to its target with descriptive text: "Correction to {contributor}'s entry from {time}: view original". The target item has a stable `id`. The original is never visually altered beyond showing that a related entry exists.

### Findings and entries (composers)

All composers post `AddIncidentEntryRequest` to `POST /api/incidents/{id}/entries`. Only one composer is open at a time. Opening one moves focus to its heading.

| Composer | Who sees it | `Kind` | Category choices | Fields |
|---|---|---|---|---|
| **Add a finding** | `canWrite` | `Observation` | Confirmed, Reported, Assumption, Outdated (Outdated then requires "Earlier classification": Confirmed, Reported, or Assumption) | Information (required, ≤ 2,000); source or "Source is unknown"; event time (optional) |
| **Record an Unknown** | `canWrite` | `Unknown` | Fixed to Unknown, with the help text "An Unknown records that no reliable answer is available yet." | What is unknown (required, ≤ 2,000); source or unknown; event time (optional) |
| **Post a summary update** | `canWrite` and `isCoordinator` | `Summary` | Confirmed, Reported, Assumption, Unknown, Outdated (with earlier classification) | Summary (required, ≤ 2,000); source or unknown; event time (optional) |
| **Add a correction** (own entry) | `canWrite`, own entry | `Correction` | As for a finding | Corrected information; explanation (required, ≤ 1,000); source or unknown; event time (optional). The target is shown as a quotation. |
| **Add another report** (another person's entry) | `canWrite`, another contributor's entry | `Challenge` | As for a finding | As for a correction |

**Category rules:**

- There is no numeric confidence anywhere.
- Outdated is freshness, not a replacement category. The form sends `category: Outdated` plus `originalCategory`. The stored entry returns the original category with `isOutdated: true`, and it is displayed as "Reported · Outdated".
- Confirmed requires a described source, so "Source is unknown" is disabled while Confirmed is selected.
- A brief, persistent legend in the Timeline card explains: "Categories describe how information is known. They do not say whether anyone is safe."

**Event time:** when the optional event time is left blank, send `eventTimeUnknown: true`. The entry is then shown as "Event time unknown", matching the canonical "event time when known, otherwise an explicit Unknown".

**Labels.** The neutral challenge label "Add another report" and the kind label "Another report" reuse the owner-approved Concern wording (WI-008-002 AC-16). "Challenge" stays the internal and API name. Corrections use "Add a correction", as in Concern.

**No Edit or Delete action exists anywhere.** Correction and challenge help text says: "This adds a separate entry linked to the original. The original stays as it was."

### Contact attempts

"Record a contact attempt" is a `canWrite` composer posting `Kind = ContactAttempt`. It is a record only.

| Field | Control | API field |
|---|---|---|
| Who attempted contact | Radios: "I did" (default) or "Someone else", which reveals a text input "Who attempted contact" (≤ 200) | `attemptedByDescription` (`null` for "I did") |
| Who they tried to reach | Text input "Person or place contacted" (required, ≤ 200). Helper: "A name or short label. Do not enter phone numbers or addresses." | `targetLabel` |
| Method | `select`: Call, Text message, Email, In-app message, In person, Other | `contactMethod` |
| Outcome | `select`: Reached, No answer, Left message, Sent — no reply yet, Other, Unknown | `contactOutcome` |
| When | `datetime-local`, **or** the "Time unknown" checkbox. Exactly one is required, and the form says so. | `eventAt` / `eventTimeUnknown` |
| Notes | `textarea`, optional (≤ 2,000) | `text` |
| Category | Confirmed, Reported, Assumption, Unknown. "Someone else" defaults the selection to Reported. | `category` |
| Source | Description, or "Source is unknown" | `sourceDescription` / `sourceUnknown` |

Rules:

- **No profile, Trusted Contact, or directory contact details are shown, suggested, or prefilled.** The target is free text.
- **No Call, Text, Email, or "Contact" action exists**, on people or on attempts. The method values are descriptions of what a person already did.
- A **Reached** outcome changes nothing else. No state change and no "safe" wording.
- In the timeline, a contact attempt reads: "{attempted by} tried to reach {target} · {method} · {outcome}", followed by the notes, both times, category, and source.

### Incident tasks

The Tasks card lists every task from `tasks`, in server order (created time).

**Each task shows:**

- the description;
- "Assigned to {name}" with "(you)";
- a status pill in text: `Open`, `Done`, `Cancelled`, or **"Not completed"** when `notCompleted` is true (an Open task in a Frozen or Resolved Incident);
- "Added by {name} · {time}";
- when Done: "Completed {time}" and a "View result" link to the `TaskResult` entry in the timeline (`resultEntryId`);
- when Cancelled: "Cancelled by {name} · {time}" and the reason.

**Actions.** These are display hints; the server enforces each rule.

| Action | Shown when | Form | API |
|---|---|---|---|
| **Add a task** (coordinator) | `canWrite` and `isCoordinator` | Description (required, ≤ 500); "Assign to" `select` of current participants, including the coordinator as "{name} (you)" | `POST …/tasks` |
| **Add a task for yourself** (participant) | `canWrite` and not the coordinator | Description (required, ≤ 500); static text "Assigned to you" | `POST …/tasks` with own `profileId` |
| **Complete…** | `canWrite`, task `Open`, and the current profile is the assignee | Result (required, ≤ 2,000); category: Confirmed, Reported, Assumption, Unknown; source or unknown; optional event time | `POST …/tasks/{taskId}/complete` |
| **Cancel task…** | `canWrite`, task `Open`, and either the current profile is the coordinator, **or** the current profile both created the task and is its assignee | Reason (required, ≤ 1,000) | `POST …/tasks/{taskId}/cancel` |

- No edit, reassign, priority, due date, checkbox-to-complete, or filtering. The task card is not a to-do list.
- Fixed helper text in the card: "Tasks can't be edited or reassigned. To change one, cancel it and add a new task."
- A participant's cancel can be denied for an old-grant task under the accepted narrow reading. That is handled by the [refresh-on-denial rule](#unavailable-and-nondisclosure); the UI does not try to predict it.

### Participants

The card is titled **"People with access"**, never "Family" or "Members".

**List:**

- the coordinator first, labeled "Coordinator";
- then each current participant, labeled "Participant";
- where the subject is a participant: "Participant · subject, added explicitly".
- When the subject is not a participant and is not the coordinator, show the persistent note: "{subject} is the subject of this Incident and does not have access unless added."
- Former participants appear only as timeline items ("Person removed" / "Access ended"), never in this list.

**Manage people** (coordinator, `Open`): an inline editor in the same card.

- A `fieldset` with the legend "People with access", containing checkboxes for active adults from `GET /api/family-groups/{familyGroupId}/members`, **excluding the coordinator**. The coordinator is not offered and is never sent; the API rejects a set that contains the coordinator.
- Current non-coordinator participants who appear in the members response are prechecked.
- Copy: "Saving replaces this list. People you remove lose access right away. What they already added stays in the history. Someone removed and added again gets new access; old access is never restored."
- **"Save people with access"** sends the full set through `PUT …/participants`. **Cancel** discards the edit.
- The coordinator has no remove control anywhere. A line explains: "The coordinator can't be removed. To step back, hand off coordination first."

### Coordinator handoff

"Hand off coordination…" is shown in the Coordination card when `Open` and `isCoordinator`. It runs in two inline steps within that card.

1. **Choose.** A `fieldset` with the legend "Hand off coordination to", holding radios for the **current participants other than the coordinator**. When there are none, show "Add a person under People with access before handing off" and no radios. Explanatory copy:
   > The person you choose becomes the only coordinator, right away. They will manage people with access, tasks, summaries, and resolution.
   > You will stay on this Incident as a participant. There is no shared coordination.
2. **Confirm.** A heading "Hand off to {name}?", focused on reveal. A primary **"Hand off coordination"** button and a secondary "Go back" button. The step is cancellable with Escape.

On success, the response redraws the page. Coordinator-only controls disappear for the former coordinator, the header shows the new coordinator, and `role="status"` announces "{name} now coordinates this Incident."

The wording is plain and calm. There are no warnings, red surfaces, or "are you sure?" alarm styling.

### Frozen state

- Pill: `Frozen`. State description: "Read-only record".
- Notice in the header, neutral information styling:
  > Coordination in this Incident is no longer editable. Its history stays available to people with access until {expiresAt date}, then it is removed.
  > To continue coordinating, start a new Incident.
- **Every mutation control is absent.** That covers Contribute, Coordination, Manage people, task actions, and correction and challenge actions. Absent is preferred to disabled, so there is no implied "you lack permission".
- Open tasks show "Not completed".
- Copy never says or implies safe, resolved, danger, or verified, and never gives the freeze reason.

### Resolved state

- Pill: `Resolved`. State description: "Coordination ended".
- A **Resolution** card, all fields from `IncidentResolutionView`:
  - "Resolution summary": `summary`;
  - "Outcome": `outcomeText`, with the category pill and "Source: …" or "Source unknown";
  - "Contact restored or resolution time": `contactRestoredAt`, or **"Unknown"** when `contactRestoredTimeUnknown`;
  - when present: "Assistance provided", "Follow-up still open", and "Lessons";
  - "Resolved by {resolvedByName} · {resolvedAt}".
- One explanatory line, shown once:
  > Resolution means coordination ended. It does not confirm that anyone is safe or that information was verified.
- **No preset verdict labels.** No "Safe", "Dangerous", "Emergency resolved", "All clear", or check-mark "success" heading. Any such wording can appear only inside the coordinator's own attributed outcome text.
- Retention line: "Kept until {expiresAt date}, then removed."
- Read-only, as for Frozen. **No reopen action.** The timeline stays visible below the resolution, and important corrections are visible there.

### Resolution flow

"Resolve this Incident…" is shown in the Coordination card when `Open` and `isCoordinator`. It reveals an inline form, and focus moves to its heading.

| Field | Control | API field |
|---|---|---|
| Resolution summary | `textarea`, required, ≤ 2,000 | `summary` |
| Outcome | `textarea`, required, ≤ 2,000. Helper: "What is known about how this ended, in your own words." | `outcomeText` |
| Outcome category | `fieldset` of radios: Confirmed, Reported, Assumption, Unknown | `outcomeCategory` |
| Outcome source | Description, or "Source is unknown" (disabled for Confirmed) | `outcomeSourceDescription` / `outcomeSourceUnknown` |
| When contact was restored or this was resolved | `datetime-local` **or** the "Unknown" checkbox; exactly one | `contactRestoredAt` / `contactRestoredTimeUnknown` |
| Assistance provided | `textarea`, optional, ≤ 2,000 | `assistanceProvided` |
| Follow-up still open | `textarea`, optional, ≤ 2,000 | `unresolvedFollowUp` |
| Lessons | `textarea`, optional, ≤ 2,000 | `lessons` |

- Text before the final button: "Resolving ends coordination for everyone with access. It can't be reopened. Resolving does not mean anyone is confirmed safe."
- The final action is an explicit primary button: **"Resolve Incident"**. "Cancel" closes the form and keeps the draft in memory for the session.
- On success, the page redraws as Resolved, focus moves to the Resolution heading, and `role="status"` announces "Incident resolved."

### Lineage display

- When the detail response has a non-null `originatingConcernId`, the header shows one subtle line: **"Started from a Concern"**, with a quiet link-style button **"Open the Concern"**. That button navigates to `/concerns` with state carrying the Concern ID.
- The line shows **nothing else**: no Concern subject, reason, state, or dates.
- When `originatingConcernId` is `null`, show **nothing**. No placeholder, no "not available" text, and no layout gap that differs from an Incident started directly. The server returns `null` for anyone not currently authorized for both records, and that response is indistinguishable from an Incident with no lineage.
- If the Concern turns out to be unavailable when opened, the existing Concern unavailable state appears. That state gives no reason.

### Unavailable and nondisclosure

- **One generic state** for `404` on detail: the heading "This Incident is unavailable", the text "Only Incidents currently shared with you can be opened.", and "Return to Incidents". It covers a nonexistent Incident, an unauthorized caller, expiry, a former participant, a stale grant, and a subject without access. Nothing distinguishes them. The pattern reuses the Concern unavailable card.
- **Refresh-on-denial for writes.** When any write returns `404`:
  1. close the open form, keeping its draft in memory;
  2. refetch the detail.
  - If the detail loads, redraw it and show the neutral message: "That change could not be made. This Incident has been refreshed with its current details." This covers a handoff race, an effective freeze, a removed assignee, and a denied old-grant cancel, without saying which.
  - If the detail returns `404`, show the generic unavailable state and refresh the list.

  This refines the Concern behavior, which goes straight to unavailable. It is needed because the Incident has more actor-specific rules, such as handoff and task grants, under which a denied write does not mean the caller lost access. It reveals nothing beyond what the caller can already read.
- **`401`** uses the existing session message on every route. **`400`** maps to field-level messages. **Other errors** give a neutral retry message that never replays a write automatically.
- After a `404`, stale list rows disappear on the next list refresh. The UI never infers whether a missing row still exists.

### Home entry point

**Recommendation: core, minimal, and static.** Without a Home entry point, an Incident participant who did not create the Incident has no way to find it other than a pasted URL, so this is required for the workflow, not optional.

- Retitle the existing Home card from "Concern coordination" to **"Safety coordination"**. The text becomes: "Review a Concern or Incident shared with you, or start a new one."
- The card holds two secondary buttons: **"Open Concerns"** (existing) and **"Open Incidents"** (new, to `/incidents`).
- The card loads no Incident data. It shows no counts, badges, "new" markers, or presence hints, and adds no notification semantics.
- **Optional, not in this story:** showing a count of Open Incidents on Home. It would need an extra Home request and a decision on attention semantics (`REQ-UX-008`). If wanted, it is a later item.

## Owner decisions — Start an Incident

Canon already answers some of the questions raised for this flow:

- **Subject locked or changeable:** answered by canon. `REQ-SAFE-016` says "Only the subject is prefilled, and it may be changed before submission." Prefilled and changeable; no owner decision is needed.
- **Who sees the action:** answered by canon. Only the Concern creator, while they have Concern access (`REQ-SAFE-016`, `DEC-071`).
- **What transfers:** answered by canon. Nothing but the subject prefill.

The remaining genuine UI choices are below. **All four were approved by the project owner on 2026-10-01**, as recorded in the "Owner decision" lines after the table. The subject-prefill rule is canonical and must not be reopened.

| ID | Decision | Recommendation | Alternatives |
|---|---|---|---|
| `UI-D1` | Where "Start an Incident" appears on Concern detail | A small **"Separate Incident"** card in the Concern side column, with a **secondary** "Start an Incident…" button, shown to the Concern creator in every returned state. It is deliberate and discoverable, without competing with the Concern's own primary actions or implying escalation. | A primary header action (too prominent; it reads as escalation). An overflow or "More" menu (no such pattern exists in the app, and it hides a deliberate action). |
| `UI-D2` | Confirmation step before creation | **No separate dialog.** The Incident creation form *is* the deliberate step. It opens with the "Starting a separate Incident" notice stating that a new Incident is created, nothing is copied, Concern participants are not added, and the Concern is unchanged. "Create Incident" is the explicit confirmation. | A modal interstitial before the form (an extra click with no new information). An acknowledgement checkbox (adds friction to a calm flow). |
| `UI-D3` | After creation | **Go directly to the new Incident's detail**, announce "Incident created", and show the "Only you have access. Add people when you are ready." note. The next task, adding people, happens there. The Concern is unchanged and is reachable through the lineage link. | Stay on the Concern with a success link (leaves the new Incident with no people and an extra step). |
| `UI-D4` | Whether the UI suggests resolving the Concern after an Incident starts | **No suggestion** in this story. PRP-029 permits a suggestion but does not require one, and staying silent keeps the Concern visibly untouched. | A dismissible hint on the Concern ("You may want to resolve this Concern"). |

**Owner decisions (2026-10-01):**

- **`UI-D1` — APPROVED.** A small "Separate Incident" card in the Concern detail side column, with a secondary "Start an Incident…" action. It is not the primary Concern action and not in an overflow menu.
- **`UI-D2` — APPROVED.** No separate confirmation dialog. The Incident create page is the deliberate confirmation step. It shows the calm notice that this creates a separate Incident, Concern information is not copied, Concern participants are not transferred, and choosing a subject does not share the Incident with them.
- **`UI-D3` — APPROVED.** After successful creation, navigate directly to the new Incident's detail screen.
- **`UI-D4` — APPROVED.** After creation, do not suggest resolving, freezing, or otherwise changing the Concern. The Concern remains independent and unchanged.

**Related presentation decisions, approved by the owner on 2026-10-01:**

- **Home entry point — APPROVED.** Retitle the existing Home card to "Safety coordination" and add "Open Incidents" as the Incident-list action. No counts, badges, notification semantics, or alert indicators.
- **Page title — APPROVED.** "Safety Incidents", in the existing `DEC-070` visual hierarchy.
- **Routes — APPROVED.** `/incidents`, `/incidents/new`, and `/incidents/:incidentId`. No top-level navigation destination is added, and `OD-001` remains open.
- **Start-from-Concern navigation state — APPROVED.** Router navigation state may carry the originating Concern ID, Family Group, and subject. Sensitive Concern information never goes into query strings. If the state is lost on reload, fall back to ordinary direct creation. Never reconstruct or expose the originating Concern from untrusted client state.
- **List, detail, participants, coordinator, entries, contact attempts, tasks, Frozen and Resolved, unavailable and save-denial, responsive, and accessibility plans — APPROVED as written.** The list uses only current API fields, and no API change is requested for the reason.

## Visual direction

**Authority.** `claude-handoff/reference/*.png` is the primary target, with the `DEC-070` system:

- blue palette;
- desktop sidebar and slim top bar, unchanged;
- one page title;
- light-blue content background with white cards;
- card header pattern of icon, title, and optional action;
- list rows with initials tiles;
- text pills;
- the shared spacing and type scale in `src/styles.scss`.

The Incident screens must look like siblings of the approved Concern screens.

| Region | Reference | Treatment |
|---|---|---|
| Incident list rows | `panel4-safety-aware.png` "Active Concern" card; `panel3` list rows | **ADAPT:** initials tile, subject, state pill, coordinator and time meta. Calm tones; no pink alarm tint. |
| Detail header | `safety-coordination.png` panel 2 header (secondary) | **ADAPT:** subject tile, title, state pill, reason and current summary. **DROP:** Share, More Actions, age, school, photo, Incident number. |
| Timeline | `panel4` "Safety Timeline"; `safety-coordination.png` panels 2 and 4 | **KEEP / ALIGN:** time column, dots, title and sub-line. **ADAPT:** dots by item type; category pills in text. **DROP:** red "Missed" and "late" states, "Today" filter, "Safety team notified". |
| Tasks card | `safety-coordination.png` panel 2 "Tasks & Coordination" | **ADAPT:** description, assignee, status pill. **DROP:** checkbox-to-complete and "In progress" / "Pending" statuses; only `Open` / `Done` / `Cancelled` / "Not completed" exist. |
| People with access | Panel 2 "Coordinators & Family" | **ADAPT:** stacked rows with Coordinator / Participant labels. **DROP:** "+ Add" for non-Family people, call and message icons, "Family member" labels. |
| Resolution | Panel 4 "Final Outcome", "Key Takeaways", "Preserved Incident History" | **ADAPT:** attributed summary and outcome; Lessons as optional text; the retention line. **DROP:** "Jordan is safe and accounted for" as a system headline, the green success check, Export, "View" rows. |
| Category legend | Panel 3 "Information at a Glance" | **ADAPT (optional):** a one-line text legend. Category counts are optional and not required. |
| Contact attempts | `panel4` "Recent Contact Attempts" | **ADAPT:** attempts appear as timeline items with method and outcome text. **DROP:** phone and chat icons implying actions, "Delivered" (not an approved outcome), "Check-in reminder". |
| Everything else in panel 4 and panels 2–4 | — | **DROP:** map, Last Known Information, Check-ins, Safety Settings, "Monitoring", Contact button, Suggested Next Steps, Convert to Incident, Companions, Travel & Accommodation, Information & Contacts. |

**Styling rules:**

- Create a **separate Incident feature folder and stylesheets**. Do not extend `concerns.scss`, which is about 7.53 kB against the 8 kB error budget.
- Split the feature into a small number of focused components (for example list, create, detail, timeline, tasks), so that each component stylesheet stays under the 4 kB warning where practical and **must** stay under the 8 kB error budget.
- Reuse global primitives from `styles.scss` before adding local rules.
- **Do not raise any budget threshold.**
- The initial bundle already exceeds its 500 kB warning (about 526 kB). Load the Incident feature lazily (for example with `@defer` on the `incidents` view, or a lazily loaded component), so that the initial bundle does not grow materially. Report the before and after sizes.
- The Concern entry-point card reuses existing Concern classes. `concerns.scss` must not grow by more than a few hundred bytes, and must stay under the 8 kB error budget.
- No red or pink alarm surfaces. State and category are conveyed by text first; color is secondary.

## Responsive behavior

There is no narrow mockup. Use the approved `DEC-070` responsive interpretation.

- No horizontal overflow at 390 px (`document.documentElement.scrollWidth <= 390`).
- Detail stacks in reading order: header, resolution, timeline, People, Contribute, Tasks, Coordination.
- The timeline stays a single column. At narrow width, the time moves above the item body rather than sitting in a cramped side column.
- List rows, task cards, and people rows stack their meta lines.
- Forms are single-column at narrow width, and fieldset options wrap.
- Action buttons wrap or stack full-width and keep a target of about 44 px or more.
- The current-Family selector and the shell behave as today. Narrow navigation remains the existing wrapping row (`OD-001`).

## Accessibility

- **One `h1`** (the shell title). Detail and create use `h2`, and cards use `h3`. Heading order is logical.
- **Labels.** Every input has a real `<label>`. Grouped choices use `fieldset` and `legend`: categories, source, people with access, handoff target, attempted-by, and time or unknown.
- **Validation.** Field errors are associated with `aria-describedby`, and invalid fields carry `aria-invalid`. A form-level summary uses `role="alert"`.
- **Status announcements.** `role="status"` (polite) is used for loading, "Incident created", saved contributions, handoff, and resolution. `role="alert"` is used for errors.
- **Keyboard.** Every control is reachable and operable. Inline confirmation steps support Escape to go back. If the implementation uses a native `<dialog>` for any confirmation, it must trap focus, label itself, and return focus to the trigger.
- **Visible focus.** The existing 3 px focus style.
- **Focus management.** Focus moves to the detail heading after create, to a composer or confirmation heading on open, and back to the triggering control (or the Resolution heading) after submit or cancel. On unavailable, focus moves to the unavailable heading.
- **Non-color cues.** State, task status, and category are always text pills. Timeline dots are decorative (`aria-hidden`).
- **Timeline semantics.** An ordered list with an accessible name; `<time datetime>` for both times, with full dates for assistive technology; related-entry links whose text names the relationship.
- **Contrast.** Any new token pairing meets WCAG AA.
- Narrow reading order matches visual order.

## UI action → API mapping

All routes are from `IncidentEndpoints.cs`. Enum values are serialized as strings (`JsonStringEnumConverter`).

| UI action | Route | Request (from `IncidentContracts.cs`) | Display hint (server enforces) | Error handling |
|---|---|---|---|---|
| Load list | `GET /api/incidents` | — | — | `401` session; error with retry |
| Load detail | `GET /api/incidents/{id}` | — | — | `404` generic unavailable |
| Load people choices | `GET /api/family-groups/{familyGroupId}/members`; `GET /api/profile` | — | — | Error: choices unavailable, submit disabled |
| Create (direct) | `POST /api/family-groups/{familyGroupId}/incidents` | `CreateIncidentRequest(subjectProfileId, reason, reasonCategory, reasonSourceDescription, reasonSourceUnknown, participantProfileIds)` | Current Family selected | `400` fields; `404` generic create message; network: no retry, refresh list |
| Start from Concern | Same route | The same request plus `originatingConcernId` | Concern creator | As above |
| Replace people | `PUT /api/incidents/{id}/participants` | `ReplaceIncidentParticipantsRequest(participantProfileIds)`, coordinator excluded | Coordinator, `Open` | `400` fields; `404` refresh-on-denial |
| Hand off | `POST /api/incidents/{id}/coordinator` | `TransferIncidentCoordinatorRequest(participantProfileId)` | Coordinator, `Open`, target is a current participant | `404` refresh-on-denial |
| Finding / Unknown / Summary / Correction / Challenge | `POST /api/incidents/{id}/entries` | `AddIncidentEntryRequest(kind, category, text, sourceDescription, sourceUnknown, eventAt, eventTimeUnknown, relatedEntryId, explanation, originalCategory)` | `canWrite`; Summary needs the coordinator; Correction needs own entry; Challenge needs another person's entry | `400` fields; `404` refresh-on-denial |
| Contact attempt | Same route | `kind: ContactAttempt` plus `attemptedByDescription, targetLabel, contactMethod, contactOutcome`, and exactly one of `eventAt` / `eventTimeUnknown`; `text` optional | `canWrite` | As above |
| Add task | `POST /api/incidents/{id}/tasks` | `CreateIncidentTaskRequest(description, assigneeProfileId)` | Coordinator: any current participant; participant: self only | `400`; `404` refresh-on-denial |
| Complete task | `POST /api/incidents/{id}/tasks/{taskId}/complete` | `CompleteIncidentTaskRequest(resultText, category, sourceDescription, sourceUnknown, eventAt)` | Assignee, task `Open` | As above |
| Cancel task | `POST /api/incidents/{id}/tasks/{taskId}/cancel` | `CancelIncidentTaskRequest(reason)` | Coordinator: any `Open` task; participant: own self-assigned task | As above |
| Resolve | `POST /api/incidents/{id}/resolve` | `ResolveIncidentRequest(summary, outcomeText, outcomeCategory, outcomeSourceDescription, outcomeSourceUnknown, contactRestoredAt, contactRestoredTimeUnknown, assistanceProvided, unresolvedFollowUp, lessons)` | Coordinator, `Open` | As above |
| Open lineage Concern | Navigate to `/concerns` with state; then `GET /api/concerns/{id}` | — | `originatingConcernId` is non-null | Existing Concern unavailable state |

Every write returns the full `IncidentView`. Redraw from the response, and never patch local state optimistically.

### API fit notes (non-blocking)

1. **The list has no reason.** `IncidentSummaryView` omits it. The list shows subject, state, coordinator, Family, and times instead. No API change is requested.
2. **No capability flags.** As with Concern, controls use simple profile comparisons. The two accepted narrow server readings (old-grant self-cancel; a coordinator in the participant set) are handled by refresh-on-denial and by excluding the coordinator from people choices.
3. **No Concern detail route.** The lineage link uses navigation state. No new Concern route or Concern API is added.
4. **Freeze reason is returned** (`freezeReason`, timeline `freezeReason`), but it is **not displayed**, following the Concern precedent and the nondisclosure intent of `REQ-PRIV-026`.

If execution finds that a returned field is insufficient for an acceptance criterion, stop. Do not guess, and do not change the API (see [Stop conditions](#stop-conditions)).

## Expected implementation surface (after authorization)

| Area | Likely change |
|---|---|
| `src/FamilySafety.Web/src/app/incidents/` (new) | Typed API client and models; focused standalone components (list, create, detail, timeline, tasks, or a similar split) with scoped stylesheets; component specs. |
| `src/FamilySafety.Web/src/app/concerns/concerns.ts`, `.html`, `.spec.ts` | **Only:** the "Separate Incident" card and its navigation, opening a Concern from navigation state, and the tests for both. No change to Concern behavior, API calls, or permissions. |
| `src/FamilySafety.Web/src/app/app.routes.ts`, `app.ts`, `app.html` | Incident routes, the `incidents` view, title and subtitle, lazy rendering of the feature, and the Home card retitle with its "Open Incidents" button. **No navigation button.** |
| `src/FamilySafety.Web/src/app/home.spec.ts` | Assertions for the Home card change. |
| `src/FamilySafety.Web/e2e/incident-workflow.spec.ts` (new); optionally a small shared helper under `e2e/fixtures/` | The Playwright journeys below. |
| `ai-context/planning/verification/evidence/WI-008-004/` (new) | Rendered evidence and a `README.md` index. |
| This story; `ai-context/planning/verification/VER-WI-008-004.md` (new) | Task checkboxes, implementation record, per-AC evidence. |

No `.NET`, API, schema, migration, or global-shell structural change is expected or authorized.

## Acceptance criteria (36)

**Routes and entry**

1. **`AC-01 — Routes without a destination`**: `/incidents`, `/incidents/new`, and `/incidents/{id}` render inside the existing shell with the single `h1` "Safety Incidents". No navigation button is added, and destination count and order are unchanged (`OD-001`). Moving between the three routes does not reload the shell.
2. **`AC-02 — Home entry`**: The Home card reads "Safety coordination", offers "Open Concerns" and "Open Incidents", loads no Incident data, and shows no count or badge.

**List**

3. **`AC-03 — Authorized list`**: Only `GET /api/incidents` results render, in server order within the "Open coordination" and "Ended coordination (read-only)" groups. Each row shows subject, a state pill, coordinator (or "You coordinate this Incident"), created and last-activity times, and, for ended Incidents, the read-only and kept-until line. No lineage and no reason appear.
4. **`AC-04 — Empty and error list`**: The empty state says no Incidents are currently shared with the user and that Incidents are shared only with the coordinator and the people they add. It implies no subject, Family, or Administrator access. Loading and error states are announced, and retry works.

**Creation**

5. **`AC-05 — Create fields`**: The create form collects subject (with self allowed), reason (1–1,000), reason category (Confirmed, Reported, Assumption, or Unknown; no Outdated), and reason source or explicit unknown source. Confirmed cannot be submitted with an unknown source. Limits and required fields are validated accessibly, and server `400` keys map to field messages without raw server text.
6. **`AC-06 — Explicit people at creation`**: People choices come only from the members endpoint, exclude the current user, and have none preselected, including the subject. Zero selections are allowed and the resulting request matches the selection.
7. **`AC-07 — Subject is not shared`**: Before submit, the form states that choosing someone as the subject does not share the Incident with them, that only the user and the people they add have access, and that membership or administrator status grants nothing. The subject's checkbox shows "(subject, not added)" or "(subject, added)".
8. **`AC-08 — Create outcomes`**: Submit is disabled in flight. `201` navigates to the new detail with focus and an announcement. `404` shows the generic creation message. A network failure never retries automatically and checks the refreshed list for a match before offering another attempt.

**Start from Concern**

9. **`AC-09 — Entry point visibility`**: Concern detail shows the "Separate Incident" card and "Start an Incident…" only to the Concern creator, in `Open`, `Frozen`, and `Resolved`. Participants never see it.
10. **`AC-10 — Nothing transferred`**: The from-Concern form shows the "Starting a separate Incident" notice. It prefills only the subject (changeable), uses the Concern's Family Group, leaves reason, category, and source empty, preselects no people, and marks no Concern participant. The request carries `originatingConcernId` and nothing from the Concern's content.
11. **`AC-11 — No URL content and safe fallback`**: No Concern data, including its ID, appears in the URL. Reloading `/incidents/new` without navigation state yields plain direct creation with no lineage.
12. **`AC-12 — Concern unchanged`**: After Start an Incident, the Concern's state, last activity, participants, and entries are unchanged (verified through the API), and the new Incident's audience contains only the people explicitly added.

**Detail**

13. **`AC-13 — Detail composition`**: Detail shows the header (subject, state, reason with provenance, current summary when newer, coordinator, started-by, times), People with access, Timeline, Tasks, and, when allowed, the Contribute and Coordination cards, as cards with no tabs, in the stated reading order.
14. **`AC-14 — Lineage shown only when returned`**: With a non-null `originatingConcernId`, the header shows only "Started from a Concern" and "Open the Concern", which opens that Concern. With `null`, no lineage text, placeholder, or layout difference appears.

**Timeline and contributions**

15. **`AC-15 — Timeline`**: Every timeline item type renders in server order with a text type label, actor or "System", event time or "Event time unknown", and recorded time. Entry items show category, Outdated, source or unknown source, text, and explanation. Access-ended and Frozen items give no reason. Markers are by type and never red or danger-coded.
16. **`AC-16 — Findings and Unknowns`**: Participants with `canWrite` add a finding (Confirmed, Reported, Assumption, or Outdated with an earlier classification) and an Unknown (fixed category), with required text and source or explicit unknown. A blank event time is sent as `eventTimeUnknown`. No numeric confidence exists.
17. **`AC-17 — Outdated is freshness`**: An Outdated submission sends `category: Outdated` with `originalCategory`. The result displays as "{original} · Outdated", and no sixth category appears.
18. **`AC-18 — Coordinator summary`**: Only the coordinator sees "Post a summary update". It posts `Summary`, and the header's current summary updates to the latest summary.
19. **`AC-19 — Correction`**: "Add a correction" appears only on the current user's own entries. It requires an explanation and appends a linked `Correction`. The original stays unchanged and visible, and the relationship is understandable to screen readers.
20. **`AC-20 — Challenge`**: "Add another report" appears only on other contributors' entries. It requires an explanation and appends a linked `Challenge` with neutral wording. No Edit or Delete action exists for any entry.
21. **`AC-21 — Contact attempts`**: The contact-attempt form records attempted-by (self or described), a free-text target label with the no-phone-or-address hint, method and outcome from the fixed lists, exactly one of time or "Time unknown", optional notes, and category and source. No contact details are prefilled, and no Call, Text, Email, or Contact action exists anywhere. A Reached outcome changes nothing else.

**Tasks**

22. **`AC-22 — Task display`**: Each task shows description, assignee, a status pill (`Open`, `Done`, `Cancelled`, or "Not completed"), creator and time, completion time with a link to its result, or cancellation actor, time, and reason. The no-edit-or-reassign helper text is shown.
23. **`AC-23 — Task creation`**: The coordinator assigns to any current participant, including themself. A participant can create a task only for themself, with no assignee choice.
24. **`AC-24 — Task completion and cancellation`**: "Complete…" appears only for the assignee on an Open task and requires a result with category and source. "Cancel task…" appears for the coordinator on any Open task, and for a participant only on a task they created and are assigned. It requires a reason. No edit or reassign control exists.

**People and coordination**

25. **`AC-25 — People with access`**: The card lists the coordinator and current participants with their roles, marks the subject only when explicitly added, and otherwise states that the subject has no access unless added. Former participants appear only in the timeline.
26. **`AC-26 — People management`**: Only the coordinator, while Open, edits the list. The coordinator is never offered or sent. Saving replaces the whole set. The copy explains immediate access loss, kept history, and no restoration on re-adding. No control removes the coordinator.
27. **`AC-27 — Coordinator handoff`**: Only the coordinator, while Open, chooses from current participants other than themself, reads calm copy that authority transfers and that they stay a participant with no co-coordination, and confirms explicitly. After success, coordinator controls move to the new coordinator and the change is announced.

**Lifecycle**

28. **`AC-28 — Frozen`**: Frozen shows the neutral read-only notice with its kept-until date and the "start a new Incident" guidance. All mutation controls are absent, and Open tasks show "Not completed". No safe, resolved, danger, verified, or freeze-reason wording appears.
29. **`AC-29 — Resolution flow`**: Only the coordinator, while Open, sees "Resolve this Incident…". It requires a summary, an outcome, an outcome category (not Outdated), a source or unknown, and exactly one of contact-restored time or "Unknown". It allows optional assistance, follow-up, and lessons, and ends with an explicit "Resolve Incident" action after the coordination-ended copy.
30. **`AC-30 — Resolved`**: Resolved shows the attributed resolution with every returned field, "Unknown" for an unknown time, resolver and time, the one coordination-ended line, and the kept-until line. It shows no preset verdict label and no reopen action, and it is read-only.

**Access and boundaries**

31. **`AC-31 — Generic unavailable`**: A `404` on detail shows the single "This Incident is unavailable" state, whatever the cause. A `404` on a write refetches the detail. If the detail loads, it is redrawn with the neutral "could not be made" message. If not, the unavailable state shows and the list refreshes. No cause is ever distinguished.
32. **`AC-32 — Display hints only`**: Controls derive only from returned fields compared with the current `profileId` and the state. Every action remains subject to the server, and no client role logic grants access or explains a denial.
33. **`AC-33 — Protected-domain boundary`**: No audit, notification, location, map, Check-In, Sharing, Trip, Event, Plan, Safety Circle, Trusted Contact, or private-profile data, or control implying them, appears. No DROP item from the visual triage is built.

**Quality**

34. **`AC-34 — Responsive`**: At 1440 px and 390 px every captured state renders without horizontal overflow (`scrollWidth <= 390` at narrow width), stacks in reading order, keeps the timeline single-column at narrow width, and keeps forms and actions usable.
35. **`AC-35 — Accessibility`**: The focused review passes for one `h1` and logical headings, labels, fieldsets, associated validation, keyboard operation including Escape on confirmations, visible focus, focus management, status and alert announcements, non-color cues, timeline semantics, and contrast.
36. **`AC-36 — Regression, budgets, and owner visual approval`**: All Angular tests pass, including the unchanged Concern behavior tests. The Concern, invitation, and Trip-audience Playwright journeys each still pass when run independently. The production build passes with no new budget error and no raised threshold, and reports the before and after initial-bundle and stylesheet sizes. The owner gives `OWNER VISUAL APPROVAL`.

## Component-test plan (Angular / Vitest)

**Rules:**

- Use **accessible selectors only**: `getByRole`, `getByLabelText`, and `getByText`, or Angular `By.css` limited to roles, labels, and `data-testid` where no accessible name exists.
- **No** CSS-class assertions, DOM-order assumptions beyond the semantic `ol` order, or screenshot golden tests.
- Mock `HttpClient` with `HttpTestingController`, using responses shaped from `IncidentContracts.cs`.

| Spec | Covers |
|---|---|
| API client | Each route, method, and body shape; string enums. |
| List | Server order within the two groups; "You coordinate"; times; kept-until line; no reason and no lineage; empty, error, and `401`; refresh. |
| Create (direct) | Subject choices with self; no preselected people; the coordinator excluded from people choices; subject-not-shared copy and the subject checkbox note; category without Outdated; Confirmed disables unknown source; field limits; `400` mapping; in-flight disable; `201` navigation and focus; `404` message; ambiguous-network list check. |
| Create (from Concern) | The notice; subject prefilled and changeable; Concern Family used; nothing else prefilled; `originatingConcernId` sent; missing state falls back to direct creation. |
| Concern entry point (in `concerns.spec.ts`) | The card for the creator in all three states and not for a participant; navigation state contents; opening a Concern from navigation state, including the existing unavailable path. The existing Concern assertions keep their meaning. |
| Detail and header | Reason with provenance; current summary only when newer; coordinator; lineage present and lineage absent (with no placeholder text). |
| Timeline | Each item type's label, actor, and both times; "Event time unknown"; Outdated text; access-ended and Frozen with no reason; related-entry links; contact-attempt rendering. |
| Composers | Finding, Unknown, Summary (coordinator only), correction (own only), challenge (others only), contact attempt (exactly-one time rule, attempted-by toggle, fixed lists, no contact actions); request bodies; `400` mapping; focus. |
| Tasks | Display of every status including "Not completed"; coordinator create with assignee choice; participant self-only create; complete visibility and body; cancel visibility rules for coordinator and participant; no edit or reassign controls. |
| People and handoff | Role labels; subject note; editor excludes the coordinator; full-set body; no coordinator remove control; handoff choices exclude self; confirm step; Escape; redraw and announcement after success. |
| Lifecycle | Frozen: no mutation controls, notice, no reason. Resolved: every resolution field, Unknown time, no verdict text, no reopen. Resolution form: required fields, exactly-one time rule, explicit final action. |
| Unavailable | Detail `404`; write `404` followed by a detail `200` (refresh message); write `404` followed by a detail `404` (unavailable and list refresh); `401` on every route. |
| Home (in `home.spec.ts`) | Retitled card and both buttons; no Incident request from Home. |

## Playwright / E2E plan

**Location and run.** New file `src/FamilySafety.Web/e2e/incident-workflow.spec.ts`, using the existing isolated scenario (`-IncludeTripAudienceAdult`, Adults A, B, and C, invitation setup as in `concern-workflow.spec.ts`). Run it independently:

```powershell
Set-Location C:\Dev\blevins\src\FamilySafety.Web
npx playwright test --config e2e/playwright.config.ts e2e/incident-workflow.spec.ts
```

**Selectors and assertions:**

- Use accessible locators (`getByRole`, `getByLabel`). No CSS-class or DOM-order selectors.
- Access assertions check **server-enforced** outcomes: list contents and `404`s, through the UI and, where useful, `apiJson` with the identity header. They never check UI hiding alone.
- Assertions never infer a denial's cause from the UI.

**Journey 1 — direct Incident.** Adult A coordinates, B is the subject, and C is a participant.

1. A creates an Incident about B with no people selected. The subject-not-shared copy is visible. (Steps 1 and 2 of the owner's list.)
2. As B: `/incidents` shows the empty state, and `/incidents/{id}` shows the generic unavailable state. The API returns `404` for B.
3. A adds C under People with access. C now lists and opens the Incident. (Step 3.)
4. C adds a finding. A records a contact attempt with "Time unknown" and a Reached outcome; the state stays `Open`. (Steps 4 and 5.)
5. A creates a task assigned to C. C completes it with a result, and the task shows `Done` with a result link. (Steps 6 and 7.)
6. A adds a correction to A's own contact attempt. C adds another report on A's correction or finding. Both originals stay unchanged. (Step 8.)
7. A hands off coordination to C with explicit confirmation. A's coordinator controls disappear, and C's appear. (Step 9.)
8. C resolves with an Unknown contact-restored time. (Step 10.)
9. As A and as C: the Resolution card is shown, no mutation control exists, and there is no reopen action. (Step 11.)
10. As A: `/incidents/{random GUID}` shows the same generic unavailable state as B saw in step 2. (Step 12.)

**Journey 2 — Start an Incident from a Concern.** Adult A creates the Concern, and C is a Concern participant.

1. Set up a Concern by A about B with C selected, through the API or the UI. Record its API view as the "before" snapshot.
2. A opens the Concern and chooses "Start an Incident…". The notice is visible, B is prefilled, and **no people are checked, including C**. A creates the Incident. (Step 13.)
3. As C: the new Incident is **not** in C's list, and its URL is unavailable, so the Concern audience was not transferred. The Concern's API view equals the "before" snapshot, so its history is unchanged and nothing was copied. The Incident timeline holds only the creation and reason entries. (Step 14.)
4. As A: the header shows "Started from a Concern". "Open the Concern" opens the same Concern. (Step 15.)
5. A adds C and B to the Incident. As C, who holds both accesses, the lineage is shown. As B, who holds Incident access only, **no lineage text appears**, and the API's `originatingConcernId` is `null` for B. (Step 16.)

**Frozen** is not reachable in E2E without 30 days of time travel or a membership-end path, and neither exists. Frozen is covered by component tests and by a rendered capture that uses response interception shaped from a real detail response. That capture is labeled "mocked" in `VER-WI-008-004`, as in WI-008-002.

**Full suite.** Also run the Concern, invitation, and Trip-audience specs independently and record the results. The known full-suite shared-database ordering issue remains a separate follow-up. If this item's spec makes it worse, record that; do not fix it here.

## Visual-evidence plan

Captures go to the **new** folder `ai-context/planning/verification/evidence/WI-008-004/`, with a `README.md` index. Use distinct file names such as `incident-list-desktop.png`. **Do not overwrite** `WI-008-002/` or `site-mockup-alignment/` evidence. The Concern entry-point capture is saved here, not in those folders.

Capture at **1440 px** and **390 px** wherever practical:

1. Incident list: populated (both groups) and empty.
2. Direct create, showing people selection and the subject-not-shared copy.
3. Create from Concern, showing the notice; plus the Concern detail with the "Separate Incident" card.
4. Detail `Open` as coordinator: header with lineage, timeline with a finding, Unknown, contact attempt, correction, and another report.
5. Detail `Open` as a participant, without coordinator controls.
6. People management editor, and the handoff choose and confirm steps.
7. Composers: finding, contact attempt, and task create; tasks in `Open`, `Done`, and `Cancelled`.
8. Resolution form, and the `Resolved` detail.
9. `Frozen` detail (mocked, labeled).
10. Unavailable state.
11. The Home "Safety coordination" card.

Report the mockup comparison table from `VALIDATION.md` § 7 (now matches, still differs, why) against `panel4-safety-aware.png` and the secondary `safety-coordination.png` panels 2 and 4, with an overall judgment and the three biggest remaining gaps. The Development identity banner appearing in captures is expected; note it.

## Verification plan

`VER-WI-008-004` records:

1. **Angular tests:** `npm test -- --watch=false`. Baseline 66 tests in 6 files, or the current count at execution start. Record the new count and list changed Concern and Home assertions, confirming that their meaning is preserved.
2. **Playwright:** the Incident spec, plus the Concern, invitation, and Trip-audience specs, each run independently. Record the commands and results.
3. **Production build:** `npm run build`. Record every budget warning with sizes, the initial bundle before and after, and each new stylesheet's size. Confirm there is no new error and no threshold change.
4. **.NET:** "No server changes; .NET suite not rerun", unless server code was touched, in which case that is a stop condition.
5. **Rendered review:** the evidence list above, with the narrow overflow assertion.
6. **Accessibility review:** manual or code-based, per `VALIDATION.md` § 6. No automated scanner is configured.
7. **Mockup comparison:** per `VALIDATION.md` § 7.
8. **Per-AC table:** every AC and canonical anchor mapped to evidence and a result, labeled `SELF-VERIFIED` unless independently reviewed.
9. **Handback:** set the story to `VERIFYING / OWNER REVIEW`. **Do not mark `DONE / PASS`** without the owner's closure decision and `OWNER VISUAL APPROVAL`.

## Stop conditions

Stop, do not guess, and report back if execution would require:

- any `.NET`, API, permission, schema, or migration change, a new endpoint, or a changed response field, including to fill a gap such as the list reason;
- a change to Concern behavior, Concern API calls, or Concern permissions beyond the entry point and opening a Concern from navigation state;
- a navigation destination, a destination count or order change, or a mobile navigation treatment (`OD-001`);
- any `OD-002`, location, Check-In, Sharing, Trip, notification, or external-communication behavior;
- displaying data the API does not return, or inferring a permission from a convenient client state;
- raising a budget threshold, or a component stylesheet over the 8 kB error budget that cannot be split;
- showing a denial's cause, or distinguishing unavailable reasons;
- an owner-approved screen changing beyond the Concern entry-point card and the Home card change named here.

Also stop if a baseline later than `CTX-1.26`, or a conflicting canonical source, appears (`BLOCKED—CONTEXT CONFLICT`).

## Implementation choices left to execution (non-policy)

Each is bounded by this story and changes no product behavior:

- the exact component split and file names;
- the route-matcher technique;
- the lazy-loading technique;
- whether inline confirmations use a native `<dialog>` (with full focus handling) or inline panels;
- exact helper-copy wording within the meanings fixed above;
- exact field error wording.

Record each choice in `VER-WI-008-004`.

## Execution tasks

- [x] `TASK-001` — Reconfirm `CTX-1.26`, owner approval of this story, the `UI-D1`–`UI-D4` answers, and execution authorization. Record the starting test count and build sizes.
- [x] `TASK-002` — Add the Incident API client and models, routes, the `incidents` view, title, and lazy rendering. Add the Home card change.
- [x] `TASK-003` — Build the list and creation (direct and from Concern), and the Concern entry point and open-from-state.
- [x] `TASK-004` — Build detail: header, lineage, people and management, handoff, timeline, and composers including contact attempts.
- [x] `TASK-005` — Build tasks, the resolution flow, Frozen and Resolved presentation, and unavailable with refresh-on-denial.
- [x] `TASK-006` — Add component tests, and the Playwright journeys with evidence capture.
- [x] `TASK-007` — Run validation (tests, Playwright, build, rendered and accessibility review, mockup comparison). Write `VER-WI-008-004`, and set `VERIFYING / OWNER REVIEW`.

## Blockers and dependencies

- **Genuine blockers:** none. The API supports every planned screen.
- **Dependency:** WI-008-003 `DONE / PASS`. Its routes and nondisclosing `404` behavior are consumed unchanged.
- **Owner input before execution:** received 2026-10-01. The story is approved, `UI-D1`–`UI-D4` are approved, and execution is authorized. Owner visual approval is required at closure.
- **Not blockers:** backup-aging and restore evidence (predeployment); Playwright full-suite isolation (separate follow-up); the list lacking a reason (recorded fit note).
- `OD-001` and `OD-002` remain open. `OD-003` remains outside MVP. `OD-004` is resolved.

## Execution authorization

- **Planning story recorded:** `YES` (2026-10-01)
- **Approved story:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner, by explicit pasted approval and authorization
- **Authorized at:** 2026-10-01
- **Context baseline authorized against:** `CTX-1.26`
- **Authorized scope:**
  - Angular Incident routes and views (`/incidents`, `/incidents/new`, `/incidents/:incidentId`);
  - the Incident list, creation, and detail;
  - Start an Incident from a Concern, including the narrow Concern UI changes named in this story;
  - the Home "Safety coordination" entry point;
  - Incident API integration against the WI-008-003 contract, unchanged;
  - participant management and coordinator handoff;
  - findings, Unknowns, and coordinator summaries;
  - contact attempts;
  - tasks;
  - corrections and "Add another report";
  - resolution;
  - the Frozen, Resolved, and unavailable states, including refresh-on-denial;
  - responsive styling and accessibility;
  - component tests, the Playwright Incident journeys, rendered evidence, and `VER-WI-008-004`.
- **Not authorized:**
  - server, API, or schema changes, and migrations;
  - resolving `OD-001` or `OD-002`;
  - notification integrations;
  - location, Check-In, Sharing Sessions, or maps;
  - emergency escalation;
  - Incident reopening;
  - unrelated refactoring.
- **Authoritative contract:** the WI-008-003 server contract (`IncidentEndpoints.cs`, `IncidentContracts.cs`) is authoritative. A UI need it cannot meet is a [stop condition](#stop-conditions), not a reason to change the server.
- **Accepted design:** the story as written, plus the [owner decisions](#owner-decisions--start-an-incident). Do not reopen them during implementation unless a genuine canonical contradiction is discovered; that is a stop condition.
- **Handback:** On completion, set the story to `VERIFYING / OWNER REVIEW` with `VER-WI-008-004` written. Do not mark `DONE / PASS` without the owner's closure decision and `OWNER VISUAL APPROVAL`. Do not commit or push.

## Evidence

| Check | Evidence | Result |
|---|---|---|
| Story traceability and bounded planning | `CTX-1.26`; `REQ-SAFE-016`, `REQ-PRIV-026`; `PRP-029` § 22; `DEC-070`, `DEC-071`; WI-008-002 and WI-008-003 stories and VER records; `IncidentEndpoints.cs`, `IncidentContracts.cs`, `IncidentService.cs`; Concern Angular source; shell, routes, and Home source; reference mockups reviewed 2026-10-01 | `PASS` (planning) |
| Owner approval and execution authorization | Explicit project-owner approval and authorization, 2026-10-01, against `CTX-1.26`, with `UI-D1`–`UI-D4` approved | `AUTHORIZED` |
| Implementation | Angular only: `src/app/incidents/` (new); narrow changes to `concerns.ts/.html/.spec.ts`, `app.routes.ts`, `app.ts`, `app.html`, `app.scss` (+1 selector), and `home.spec.ts`; `e2e/incident-workflow.spec.ts` (new); one locator in `e2e/concern-workflow.spec.ts`. No server, API, schema, or migration change. | `DONE` (2026-10-02) |
| Validation | Angular 118/118 (8 files); Incident Playwright 2/2; Concern, invitation, and Trip-audience journeys each 1/1, run on their own; production build passes with no new budget warning; 22 states captured at 1440 px and 390 px in `evidence/WI-008-004/` with no horizontal overflow | `PASS` (`SELF-VERIFIED`) |

## Verification

- **Verification report:** `ai-context/planning/verification/VER-WI-008-004.md`
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `PASS`. All 36 acceptance criteria pass. AC-36 `OWNER VISUAL APPROVAL` was given on 2026-10-02. The story is `DONE / PASS` — `OWNER APPROVED`.

## Implementation record (2026-10-02)

- **Implementation choices and interpretations:** recorded in `VER-WI-008-004` (Implementation choices; Deviations and interpretations). In brief:
  - one matcher route serves all three Incident URLs;
  - `@defer` lazy loading;
  - inline confirmations with Escape;
  - recorded-time-only meta for server action items in the timeline;
  - a one-line Home locator update in `concern-workflow.spec.ts`;
  - the initial bundle grew about 10 kB (defer runtime).
- **Contract:** no server or API contract gap was found.
- **Frozen evidence:** component tests plus one mocked capture.

## Owner closure (2026-10-02)

- **Owner visual review:** `PASS`. The project owner gave `OWNER VISUAL APPROVAL` on 2026-10-02 for the Incident UI as implemented. The approval covers the Safety coordination Home card and Open Incidents entry point; the Safety Incidents page title; the list; direct creation; Start an Incident from a Concern; detail; People with access; coordinator handoff; findings, Unknowns, and summaries; corrections and "Add another report"; contact attempts; tasks; Frozen and Resolved presentation; the resolution flow; lineage display; unavailable and save-denial behavior; responsive layout; the current accessibility implementation; and the `DEC-070` visual direction.
- **Accepted visual differences (non-blocking; no further work):**
  - one chronological timeline instead of separate contact-attempt and check-in cards;
  - People shown as rows rather than avatar strips;
  - initials rather than photos;
  - no mockup-only functionality;
  - Frozen evidenced by component tests and a mocked capture, not by forcing a real Frozen state.
- **Accepted implementation deviations (non-blocking):**
  1. Timeline actions without a separate event time show only the recorded time.
  2. The Home locator in the existing Concern Playwright spec was updated for the intentional Home card rename.
  3. The initial bundle grew from 526.47 kB to 536.44 kB. It was already over the warning before this work. The Incident feature is a separate lazy chunk of about 138 kB. No Incident stylesheet warns, and no budget was raised.
  4. Navigation state may survive a reload of the same browser entry. A fresh `/incidents/new` without state falls back to direct creation. The state holds IDs only, and authorization stays server-enforced.
- **Closeout:** status reconciliation only. No application code changed, and tests and the build were not rerun. The recorded evidence in `VER-WI-008-004` stands.
