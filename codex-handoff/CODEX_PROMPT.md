# FamilyConnect: Home Page Build Spec (for Codex)

## 0. How to use this brief

You are building the **Home / Dashboard** page of a web app called **FamilyConnect** (tagline: *Together. Informed. Connected.*).

Reference images are in `./reference/`:

| File | What it is |
|---|---|
| `full-mockup.png` | The original 4-panel board. Overview only. Do not build from this alone, text is too small. |
| `panel1-main-home.png` | **The default Home screen. Build this first.** |
| `panel2-family-activity.png` | Variant: Family Activity & Coordination |
| `panel3-travel-events.png` | Variant: Travel & Events Summary |
| `panel4-safety-aware.png` | Variant: Safety-Aware Dashboard |
| `detail-shell-topbar.png` | Close-up of sidebar and top bar |

**Important:** the board shows **one Home page in four views**, not four unrelated pages. Panels 2-4 reuse the same shell (sidebar, top bar, card style) and the same components with different content. Build the shared shell and components once.

**Work in stages. Finish a stage, then stop and wait for my approval.** After each stage I will send a screenshot. Compare it against the matching reference image, list every difference, and fix them before moving on.

Before writing any code in Stage 1, reply with a short written description of what you see in `panel1-main-home.png` (layout, sections, colors) so I can correct any misreading.

---

## 1. Stack and conventions

- Use the stack already in this repo. If starting fresh: Next.js (App Router) + TypeScript + Tailwind CSS.
- Icons: `lucide-react` (outlined, 1.75px stroke).
- Font: Inter (fallback: system-ui, sans-serif).
- All data is **mock data** in `/lib/mock-data.ts`, typed with interfaces. No backend yet.
- Desktop-first at 1440px wide, but must be responsive (see section 7).
- Accessible: semantic landmarks (`nav`, `main`), visible focus rings, buttons are real `<button>`s, images have alt text, status is never conveyed by color alone (pills always contain text).
- Photos and avatars are placeholders. Do not try to recreate the photography. Use neutral gradient or initials avatars and soft-gradient image blocks (or `/public/placeholders/`).

---

## 2. Design tokens

Define these as Tailwind theme extensions or CSS variables. Values are approximations read from the mockup. Adjust only if I say so.

**Color**
| Token | Value | Use |
|---|---|---|
| `brand-700` | `#1E3A8A` | Page titles, card titles, nav text |
| `brand-600` | `#1D4ED8` | Links, icons, active nav text |
| `brand-500` | `#2563EB` | Primary buttons |
| `brand-50` | `#EFF6FF` | Active nav pill, secondary button fill, search bar fill |
| `bg-app` | `#F3F7FD` | Page background (very light blue-gray, subtle gradient allowed) |
| `surface` | `#FFFFFF` | Cards |
| `border` | `#E3EAF5` | Card and row borders |
| `text-body` | `#475569` | Secondary text (dates, locations, subtitles) |
| `text-muted` | `#94A3B8` | Timestamps, placeholder text |
| `alert-tint` | `#FEECEC` | Background of "Needs Your Attention" and "Active Concern" cards |
| `alert-text` | `#DC2626` | Alert card title and alert icon |

**Status pills** (rounded-full, ~12px semibold text, px-3 py-1)
| Status | Background | Text |
|---|---|---|
| Going | `#DCFCE7` | `#15803D` |
| Checked In | `#DCFCE7` | `#15803D` |
| Planning | `#DBEAFE` | `#1D4ED8` |
| Maybe | `#FEF3C7` | `#B45309` |
| Monitoring | `#FEF3C7` | `#B45309` |
| Not Going | `#FEE2E2` | `#B91C1C` |
| Considering | `#E5E7EB` | `#4B5563` |
| Delivered | `#DCFCE7` | `#15803D` |
| No answer | none (plain muted text) | `#94A3B8` |

**Shape and elevation**
- Cards: `rounded-2xl` (16px), 1px `border`, shadow `0 1px 2px rgba(30,58,138,.04), 0 4px 16px rgba(30,58,138,.06)`, padding 16-20px.
- Buttons: `rounded-lg` (8-10px), 36px tall, 14px semibold. Primary = solid brand-500 with white text. Secondary = brand-50 fill with brand-600 text. Outline = white with 1px border and brand-600 text.
- Thumbnails: `rounded-lg`, 64-72px tall, ~96px wide in lists.
- Avatars: circular, 36-44px.

**Type scale**
| Use | Size / weight |
|---|---|
| Page title ("Good morning, Alex!") | 28-30px / 700 / brand-700 |
| Page subtitle | 14px / 400 / text-body |
| Card title | 17-18px / 700 / brand-600 or brand-700 |
| Row title | 14-15px / 600 / brand-700 |
| Row meta (date, place) | 13px / 400 / text-body |
| Timestamp | 12px / 400 / text-muted |
| "View all" link | 13px / 600 / brand-600 |

**Tone:** calm, friendly, reassuring. Even alert states use a soft pink tint with red only on the title and icon. Never use harsh saturated red backgrounds, flashing, or alarming copy.

---

## 3. Shared shell (build once, used by every view)

**Browser frame in the mockup is decoration. Do not build it.**

**Top bar** (white, ~64px, bottom border)
- Left: logo (house-with-people mark in brand blue) + "FamilyConnect" wordmark, bold, brand-700.
- Center: search input, ~480px max, fully rounded, brand-50 fill, search icon left, placeholder "Search events, people, trips, locations..." (placeholder text varies slightly per view, see section 6).
- Right: notification bell with a small red dot badge (count `1`), then avatar (Alex) + "Alex" + chevron-down (dropdown menu).

**Left sidebar** (~220px, white or transparent over bg-app)
Items in this order, icon + label, 15px, 44px row height, rounded-xl:
1. Home 2. Events 3. Trips 4. Family 5. Safety 6. Messages 7. Plans 8. Me / Settings

Active item: brand-50 pill background, brand-600 bold text and icon. Inactive: brand-700 text, regular weight.
Suggested icons: Home, CalendarCheck, Plane, Users, ShieldCheck, MessageSquare, Plane/Map (Plans), User.

**Main content area:** max-width ~1100px, 24-32px padding, page header row (title and subtitle left, date right-aligned: "Tue, Jul 8, 2025").

---

## 4. Reusable components (create these first)

`AppShell`, `Sidebar`, `TopBar`, `PageHeader`, `Card`, `SectionHeader` (icon + title left, "View all" right), `StatusPill`, `Avatar`, `AvatarStack` (overlapping with "+2" overflow chip), `Thumbnail`, `ListRow`, `Button` (primary / secondary / outline), `CheckboxRow`, `Timeline`, `AlertCard`.

Every section in every view must be assembled from these. No one-off styling that duplicates what a component does.

---

## 5. Stages

### Stage 1: Shell and design system
Build the tokens, `AppShell`, `Sidebar`, `TopBar`, `PageHeader`, `Card`, `SectionHeader`, `StatusPill`, `Button`, `Avatar`. Render an empty Home page with the greeting header and two or three sample cards so I can check the styling. **Stop.**

### Stage 2: Panel 1, Main Home Dashboard (`/` route)
Match `panel1-main-home.png` exactly. Layout is a 12-column grid:

**Row 1** (two columns, roughly 55/45)
- **Needs Your Attention** (left): `alert-tint` card. Header: filled red circle with white "!" icon, title "Needs Your Attention" in `alert-text`, subtitle "2 items", chevron right. Inside, two white inset rows:
  1. Purple calendar icon, **"RSVP needed: Family Reunion"**, "Sat, Jul 12 - Lake House", primary button **Respond**
  2. Blue shield icon, **"Check-in reminder"**, "Jordan is overdue for a check-in", secondary button **View**
- **Upcoming Events** (right): header with calendar icon, "Upcoming Events", "View all". Two rows: thumbnail, title, date, location, status pill right-aligned.
  - Family Reunion / Sat, Jul 12, 2025 / Lake House / **Going**
  - Georgia Trip / Aug 14 – Aug 20, 2025 / Atlanta, GA / **Planning**

**Row 2** (three equal columns)
- **Current Trips**: rows with larger thumbnail (city skyline / beach), title, date range, location. Georgia Trip has a primary **View Trip** button, Beach Vacation (Dec 18 – Dec 26, 2025, Miami, FL) has a **Planning** pill.
- **Recent Family Updates**: avatar, bold one-line headline, quoted preview in muted text, timestamp right-aligned.
  - Emma checked in, "All good at home! 😊", 2h ago
  - Jordan added a note to Georgia Trip, "Here are the restaurant ideas!", 8h ago
  - Taylor uploaded 5 new photos, "Weekend at the lake!", 8h ago
- **My Tasks**: checkbox rows with title and a muted context line, working toggle state (checked = strikethrough and muted).
  - Respond to event RSVP / Family Reunion
  - Confirm trip details / Georgia Trip
  - Review safety check-in / Jordan
  - Add packing items / Beach Vacation

**Row 3** (full-width, short bar)
- **Active Sharing**: people icon, title, small muted sentence "5 family members are actively sharing location, updates, or trip details.", `AvatarStack` (3 avatars + "+2"), outline button **Manage Sharing**.

**Stop for review.**

### Stage 3: Panel 2, Family Activity (`/activity` route)
Match `panel2-family-activity.png`. Page title "Family Activity", subtitle "A snapshot of your family's latest activity and coordination." Two-by-two card grid:
- **Upcoming Birthdays**: Emma (Jul 14, in 6 days), Grandpa Joe (Jul 22, in 14 days), Taylor (Aug 3, in 26 days). Each has avatar, name, date line, **Send Note** outline button.
- **Recent Updates**: avatar rows with headline, quote, timestamp. The first row (Taylor, "Uploaded 5 new photos") shows a photo strip of 3 thumbnails plus a "+3" tile. Others: Emma checked in (4h ago), Grandma Lee added a note (6h ago).
- **Pending Invitations**: Family Reunion (Sat, Jul 12 · Lake House · Invited by Emma) and Jordan's Graduation Party (Sun, May 18, 2025 · Invited by Jordan). Each has stacked **Respond** (primary) and **Maybe** (outline) buttons.
- **Recent Check-ins**: Emma (Atlanta, GA, "All good at home! 😊", 2h ago), Jordan (At school, "Heading to study group.", 5h ago), Taylor (Riverside Park, "Great run this morning!", 9h ago). Each with a **Checked In** pill.

**Stop for review.**

### Stage 4: Panel 3, Travel & Events (`/events` route)
Match `panel3-travel-events.png`. Title "Travel & Events", subtitle "All your family events, trips, and RSVPs in one place.", primary **+ Create** button right of the header.
- **Upcoming Events** (left, tall): Family Reunion (Sat, Jul 12, 2025 · 12:00 PM · Lake House) **Going**; Maya's Birthday Dinner (Sat, Jul 26, 2025 · 6:00 PM · The Local Table) **Maybe**; Thanksgiving Gathering (Thu, Nov 27, 2025 · 1:00 PM · Grandma Lee's House) **Not Going**.
- **Active Trips** (right): Georgia Trip **Planning**; Beach Vacation (Dec 18 – Dec 26, 2025 · Miami, FL) **Planning**; Spring Ski Trip (Mar 7 – Mar 14, 2026 · Park City, UT) **Considering**.
- **RSVPs Needed** (bottom left): compact rows with colored calendar icon, title, date and place, small primary **Respond** button. Items: Family Reunion, Jordan's Graduation Party, Game Night (Sat, Aug 2, 2025 · Our House).
- **Quick Links** (bottom right): 2x3 grid of tiles, icon + label + one-line helper text: Create an Event, Plan a Trip, View Calendar, Family Members, Shared Lists, Travel Documents.

Sidebar active item for this view: **Events**.

**Stop for review.**

### Stage 5: Panel 4, Safety-Aware Dashboard (`/safety` route)
Match `panel4-safety-aware.png`. Title "Family Safety & Well-Being", subtitle "A calm, proactive view of your family's safety and check-ins.", outline **Safety Settings** button (gear icon) right of header.
- **Active Concern** (top left, `alert-tint` card): shield icon, title "Active Concern", amber **Monitoring** pill at right. Large avatar (Emma), headline **"Emma – Delayed Check-In"**, "Expected check-in: Mon, Jul 8 · 7:30 PM", "Haven't heard from Emma since 7:30 PM. Last known location: Atlanta, GA." Buttons: primary **Contact**, outline **Mark as Resolved**.
- **Recent Contact Attempts** (top right): list with icon, label, status, time. Call to Emma / No answer / 8:15 PM; Text to Emma / Delivered / 8:12 PM; Check-in reminder / Sent via app / 7:45 PM; Call to Emma / No answer / 7:32 PM.
- **Today's Check-ins** (bottom left): Jordan (Checked in · 5:20 PM, "At the library, all good!"), Taylor (Checked in · 3:15 PM, "Finished practice!"), Grandma Lee (Checked in · 1:42 PM, "Home and relaxing."), each with **Checked In** pill.
- **Safety Timeline** (bottom right): "Today" select dropdown. Vertical timeline with colored dots (red = expected check-in missed, blue = outreach, green = completed), time left, event title and detail right. 8:15 PM Call to Emma (no answer), 8:12 PM Text to Emma (delivered), 7:45 PM Check-in reminder sent, 7:30 PM Expected check-in time (no check-in received yet).

Sidebar active item for this view: **Safety**. (The mockup shows Events highlighted here, which looks like a mistake.)

**Stop for review.**

### Stage 6: Making the four views one Home
Decide with me how panels 2-4 surface from the Home route: either as tabs/segmented control on Home, or as the separate routes above linked from sidebar and "View all" links. Default if I don't answer: separate routes, Panel 1 stays at `/`.

---

## 6. Search placeholder per view
- Panel 1: "Search events, people, trips, locations..."
- Panel 2: "Search people, events, trips, or places..."
- Panel 3: "Search events, trips, destinations, people..."
- Panel 4: "Search people, locations, or safety updates..."

---

## 7. Responsive behavior
- **≥1280px:** layout as in the mockup.
- **768-1279px:** sidebar collapses to icons only (~72px). Three-column rows become two columns, then the third wraps below.
- **<768px:** sidebar becomes a bottom tab bar (Home, Events, Trips, Family, Safety, with the rest under "More"). All cards stack in one column, order: Needs Your Attention first, then Upcoming Events, Current Trips, Recent Updates, My Tasks, Active Sharing.

---

## 8. Known inconsistencies in the mockup (decisions made)
- Panel 1 flags **Jordan** as overdue for a check-in, while Panel 4 shows **Emma** as the delayed check-in. Use the mock data as written in each view, but keep the data source shared so this can be reconciled later.
- Panel 4 highlights **Events** in the sidebar. Use **Safety** instead.
- "Trips" and "Plans" both exist in the nav. Keep both for now, with Plans as a placeholder route.
- "Current Trips" in Panel 1 lists trips that start in the future. Keep the label as drawn.

---

## 9. Acceptance checklist (run this against every stage)
1. Side-by-side with the reference image, section order and grid proportions match.
2. All text content, dates, names, and statuses match the mockup.
3. Colors, radii, shadows, and type sizes use the tokens in section 2, with no ad-hoc hex values in components.
4. Status pills use the correct color per status.
5. Interactive states exist: hover, focus-visible, active, disabled. Checkboxes toggle. Dropdowns open.
6. No component is duplicated where a shared one should be used.
7. Layout holds at 1440px, 1024px, and 390px wide.
8. No console errors or TypeScript errors, and lint passes.

When done with a stage, give me: (a) a list of files created or changed, (b) the commands to run it, (c) anything you were unsure about and chose a default for.
