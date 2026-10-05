# Mockup Alignment

> **Current use:** The site-wide visual system was approved under `DEC-070`. Later owner decisions map panel ① to Home (`DEC-076`), ② to Family (`DEC-077`), ③ to Events (`DEC-078`), and ④ to the data-bearing Safety dashboard (`DEC-079`). Phase 9 is closed under `DEC-081`; [project status](../ai-context/project-status.md) owns current Phase 9.5 authorization. The original DEC-070 comparison examples below record the earlier mapping and are not current implementation instructions.

## The visual target

**THE FILES IN `C:\Dev\blevins\claude-handoff\reference` DEFINE THE VISUAL TARGET.**

```
claude-handoff\reference   = WHAT IT SHOULD LOOK LIKE
ai-context                 = WHAT MAY EXIST AND HOW IT BEHAVES
running app                = WHERE WE START
generated screenshots      = WHAT WAS BUILT BEFORE (evidence, not design)
```

- Do not treat rendered verification evidence (`ai-context\planning\verification\evidence\**`) or historical owner-review images (`artifacts\reviews\**`) as equivalent to the mockups. Those images show what was built or reviewed, not what is wanted.
- Compare a screen with its approved reference only within a separately authorized UI work item. Later owner decisions and canonical rules control any difference.
- The mockups do **not** authorize functionality. Canonical behavior remains in `ai-context`.
- The earlier Codex brief is [archived](../ai-context/archive/codex-handoff/CODEX_PROMPT.md). Its former `codex-handoff\reference\` copies were byte-for-byte duplicates; use the retained `claude-handoff\reference\` images.

## Reference inventory

All six files are **desktop** (about 1,400 px wide) and show one product, "FamilyConnect", in a single visual language. There is **no mobile or narrow mockup**. Narrow layouts must be derived from the desktop compositions.

| File | Represents | Layout and composition | Visual patterns and components |
|---|---|---|---|
| `full-mockup.png` | Overview board titled "Home / Dashboard": four views (1 Main Home, 2 Family Activity, 3 Travel & Events, 4 Safety-Aware) | 2 × 2 board of the panels below. Overview only; text is too small to build from. | Same as panels 1–4 |
| `detail-shell-topbar.png` | **Shared app shell** close-up | Top bar: logo and "FamilyConnect" wordmark, centered rounded search field, bell with red badge, avatar, name, and chevron. Left sidebar: icon + label rows, with the active row as a soft pill. Content area: large greeting title, subtitle, and right-aligned date. | Light blue-gray page; white surfaces; bold dark-blue titles; soft active pill |
| `panel1-main-home.png` | **Home** (default dashboard) | Title row "Good morning, Alex!" + subtitle + date. Row 1 (about 55/45): "Needs Your Attention" alert card (pink tint, red icon, inset rows with action buttons) and "Upcoming Events" (thumbnail rows + status pills). Row 2, three equal cards: "Current Trips" (larger thumbnails, View Trip / status), "Recent Family Updates" (avatar feed with timestamps), "My Tasks" (checkbox list). Row 3: full-width "Active Sharing" bar (avatar stack + outline button). | Card header = icon + title + "View all"; rounded-2xl cards; pill statuses; thumbnails; avatars; primary/secondary/outline buttons |
| `panel2-family-activity.png` | "Family Activity" — a **Home variant** (the sidebar highlights Home) | Title + subtitle + date. 2 × 2 cards: "Upcoming Birthdays" (avatar, date, Send Note), "Recent Updates" (activity feed with photo strip), "Pending Invitations" (thumbnail, Respond / Maybe), "Recent Check-ins" (avatar, place, Checked In pill). | Avatar list rows; stacked button pairs; status pills |
| `panel3-travel-events.png` | "Travel & Events" — **Events** (the sidebar highlights Events), combining Events and Trips summaries | Title + subtitle + primary "+ Create" at the top right. Row 1: "Upcoming Events" (thumbnail, date · time, place, RSVP pill) and "Active Trips" (thumbnail, dates, place, status pill). Row 2: "RSVPs Needed" (compact rows + Respond) and "Quick Links" (2 × 3 icon tiles). | Thumbnail list rows with status pills; icon tiles; primary header action |
| `panel4-safety-aware.png` | "Family Safety & Well-Being" — a **Safety** dashboard (the sidebar wrongly highlights Events) | Title + subtitle + outline "Safety Settings" at the top right. Row 1: "Active Concern" (pink tint, avatar, headline, expected time, Contact / Mark as Resolved, Monitoring pill) and "Recent Contact Attempts" list. Row 2: "Today's Check-ins" (avatar rows + Checked In pills) and "Safety Timeline" (time column, colored dots, Today dropdown). | Alert-tint card; timeline with time column and dots; icon list rows |

## Current approved screen mapping

| Panel | Current screen | Decision |
|---|---|---|
| ① `panel1-main-home.png` | Home dashboard | `DEC-076` |
| ② `panel2-family-activity.png` | Family dashboard and Family Settings split; the image itself highlights Home | `DEC-077` |
| ③ `panel3-travel-events.png` | Events dashboard, with a read-only Trips summary | `DEC-078` |
| ④ `panel4-safety-aware.png` | Read-only, data-bearing Safety dashboard with Open Concern and Open Safety Incident summaries and a Check-Ins placeholder | `DEC-079` |

The approved destinations and behavior are in `ai-context`, the completed work items, and their decisions. Mockup-only controls do not become features.

## Original screen mapping before Phase 9 dashboard decisions — historical

The table and DEC-070 alignment examples below describe the pre-Phase 9 interpretation. In particular, the original panel ② Home-pattern and panel ④ Concerns-list mappings were later revised by `DEC-077` and `DEC-079`.

| Reference | Running screen | Fit |
|---|---|---|
| `detail-shell-topbar.png` | Shared shell on **every** screen (`app.html` header and nav, `app.scss`, `styles.scss`) | Clean for composition. Search, bell, and avatar menu are not approved functionality, and the sidebar destinations differ from the app's. Needs owner decisions (see `CURRENT-TASK.md`). |
| `panel1-main-home.png` | **Home** | **Clean.** Primary Home target. |
| `panel2-family-activity.png` | **Home** (variant) — *not* the Family screen | **Ambiguous.** The title says "Family Activity", but the sidebar marks Home, and the content (birthdays, activity feed, invitations, check-ins) is not the Family screen's content (directory, profile, relationships, invitations). Use it as visual language for card and avatar-row patterns. It is **not** a Family screen layout. |
| `panel3-travel-events.png` | **Events** (primary); its "Active Trips" card informs the **Trips** list rows | **Partial.** The app keeps Events and Trips as separate screens with create forms; the reference is a combined summary. The list-row and header-action patterns transfer; the combined page does not. |
| `panel4-safety-aware.png` | **Concerns** (list / entry view) | **Partial.** Only the "Active Concern" card and the timeline styling map to approved Concern content. Most of the panel is Check-In and contact-log functionality, which is DROP. |
| `full-mockup.png` | Overview of the four panels | Use the individual panel files instead. |

### Coverage gaps (no primary reference)

Plans (list/create), Trip create/detail/notes/audience editor, Family directory/profile/relationships/invitations, Events create/detail/edit, Concern create/detail/history, Me / Settings, invitation/first-use screens, and all narrow/mobile layouts.

For these, apply the **shared visual language** of the primary references: shell, card, card-header, list-row, pill, button, spacing, and type scale. The older mockups in `ai-context\references\ux\` (`events/`, `plans-trips/`, `family/`, `safety/`, `admin-settings/`, `home-dashboard/`) may be consulted as **secondary** layout hints for these screens only. Where they conflict with the primary references' visual language, the primary references win. If a screen has no reference at all, say so in the comparison. Do not invent a target.

## KEEP / ALIGN / ADAPT / DROP — original DEC-070 examples

This method remains useful for approved UI work, but the screen-specific examples below are historical. `DEC-076`–`DEC-079` allow honest data-free placeholders and revised dashboard compositions without implementing the underlying Phase 9.5 capabilities.

| Category | Meaning |
|---|---|
| **KEEP** | Approved functionality shown in the mockup and already implemented. Keep the behavior; restyle it to the reference. |
| **ALIGN** | Existing functionality whose layout or composition should change to match the mockup. |
| **ADAPT** | Visual structure from the mockup reused with currently approved functionality or data substituted. |
| **DROP** | Mockup-only functionality that is not approved. Do not build it or imply it. |

**Approved but not implemented** capabilities (Event invitations/RSVP, Needs, Updates, Shared Family Dates/birthdays, Trusted Contacts) remain outside this cleanup. The historical DROP examples below do not override later owner-approved, data-free placeholders; they never authorize the underlying capability.

### Shell — `detail-shell-topbar.png`

- **ALIGN** (pending the owner shell decision): top-bar + sidebar composition, logo/wordmark placement, active-destination pill, page title + subtitle + right-aligned date row.
- **ADAPT:** the sidebar lists the app's **current** destinations in their current order, with Concerns reached as today (no new destination without an owner decision; `OD-001`). The avatar/name slot may show the current profile name and link to Me / Settings. The current-Family indicator and multi-Family selector must remain visible and usable.
- **DROP:** search, the notification bell and badge, the avatar dropdown menu, the Messages destination, and the browser chrome (decoration in the image).

### Home — `panel1-main-home.png` (+ `panel2` patterns)

- **KEEP:** Upcoming Events, Plans, and Trips previews; the Concern entry card; the relationship-suggestion attention item; the empty/low-data states.
- **ALIGN:**
  - Greeting title row with subtitle and date.
  - Row 1: attention card on the left, Upcoming Events on the right.
  - Row 2: three-card grid.
  - Card header pattern (icon + title + "View all" link to the existing route).
  - Event and Trip rows with an image-or-tile slot and the date/place meta.
- **ADAPT:**
  - "Needs Your Attention" holds only real attention items, such as the relationship suggestion; use a calm tint, not alarm red.
  - "Current Trips" holds the existing Trip previews.
  - The Row 2 slots hold Plans and the Concern entry instead of Updates/Tasks.
  - Thumbnails become neutral tiles or initials, because no images exist.
  - Status pills appear only for statuses the app actually has.
- **DROP:** RSVP "Respond" and Going/Planning statuses, Check-in reminder, Recent Family Updates feed, My Tasks, Active Sharing, birthdays, Send Note, Pending Invitations as RSVP, Recent Check-ins, and photos.

### Events (+ Trips list) — `panel3-travel-events.png`

- **KEEP:** Event list, create, detail, and edit; Trip list and create; audience selection; Notes.
- **ALIGN:** title + subtitle + primary create action at the top right; card with header and "View all"; thumbnail-style list rows with date · time and place; card rhythm.
- **ADAPT:** the "+ Create" header action maps to the existing create flow of the screen being viewed; Trip rows on the Trips screen reuse the "Active Trips" row pattern; thumbnails become tiles.
- **DROP:** RSVP pills (Going/Maybe/Not Going), RSVPs Needed, Quick Links (calendar, shared lists, travel documents), the Trip statuses Planning/Considering, and the combined Travel & Events page.

### Concerns / Safety — `panel4-safety-aware.png`

- **KEEP:** everything approved in WI-008-002: list, create, detail, history, correction/challenge, participants, resolution, Open/Frozen/Resolved, and the generic unavailable state.
- **ALIGN:** dashboard-style title row with header action; a prominent card for Open Concerns in the list; timeline styling with a time column and dots.
- **ADAPT:**
  - "Active Concern" card → an Open Concern summary card (subject initials, reason, last activity, state pill, open/view action) in calm tones.
  - "Mark as Resolved" → only the existing creator resolve flow on detail.
  - The timeline shows Concern entries, with markers by entry kind, never by danger.
- **DROP:** Contact, Recent Contact Attempts, Today's Check-ins, Check-in reminders, Safety Settings, "Monitoring" status, expected check-in times, last known location, Incident, and alarm-red styling.

## Original mockup exclusions — historical DEC-070 list

Later decisions approved selected presentation elements, placeholders, and the Safety Incident implementation. Use current context and `DEC-076`–`DEC-081` for today's scope; the list below records what the original mockup did not authorize by itself.

Search; notifications and badge; the Messages destination; the avatar menu; RSVP/Respond/Maybe/Going statuses; RSVPs Needed; Pending Invitations as RSVP; Send Note and birthdays; activity/updates feeds and photo uploads; My Tasks and task checklists; Active Sharing and Manage Sharing; live or last-known location; Check-ins, check-in reminders, and Recent Check-ins; contact attempts and Contact/Call/Text actions; Safety Settings; "Monitoring" and other safety statuses; Quick Links (calendar, shared lists, travel documents); Trip statuses (Planning/Considering); Safety Incident and escalation; Suggested Steps; maps; emergency actions; danger or risk scoring; new permissions or roles.

## Privacy and product guardrails no visual choice may weaken

- Server authorization is authoritative; client hints only decide which controls are shown.
- Family membership alone grants no access to Concerns or audience-limited items. Administrator status grants no private-data or Concern access override.
- Events, Plans, and Trips keep their selected-audience semantics. A card or "View all" must never show items the API did not return.
- Concerns: no automatic subject access; explicit participant selection; creator access separate from participant access; append-only corrections; additive, attributed challenges; Open/Frozen/Resolved semantics; nondisclosing unavailable state.
- Calm tone. No alarm-red surfaces, urgency, or danger inference for Concerns.
- Accessibility: status is never conveyed by color alone; real buttons and labels; visible focus; usable at 390 px.

## Original per-screen comparison template — historical example

| Screen | Reference + region | Current implementation | KEEP / ALIGN / ADAPT / DROP | Planned change |
|---|---|---|---|---|
| Shell (desktop) | `detail-shell-topbar.png` | | | |
| Shell (narrow) | none — derive | | | |
| Home | `panel1` | | | |
| Family | none — shared language (+ `panel2` patterns) | | | |
| Events | `panel3` | | | |
| Plans | none — shared language | | | |
| Trips | `panel3` "Active Trips" rows | | | |
| Concerns list | `panel4` | | | |
| Concern detail/create | none — shared language (WI-008-002 approved layout) | | | |
| Me / Settings | none — shared language | | | |

Cover composition, cards, sections, whitespace, typography, hierarchy, forms, buttons, status surfaces, audience controls, history, desktop and narrow layout, density, and rhythm. After implementation, report **what now matches, what still differs, and why**.

## Known reference inconsistencies

- `panel4-safety-aware.png` highlights **Events** in the sidebar; it is a Safety view.
- `panel2-family-activity.png` is titled "Family Activity" but highlights **Home**.
- Panel 1 flags Jordan as overdue, while panel 4 shows Emma. This is sample data; it has no meaning.
- The sidebar lists both Trips and Plans, plus Safety and Messages. The destination set is governed by `REQ-UX-013` (`DEC-074`), which resolved `OD-001`, not by the image.
- "Current Trips" lists future trips. The label is illustrative only.
