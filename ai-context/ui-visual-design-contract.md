# Blevins UI Visual Design Contract

**Status:** Canonical visual standard, owner-requested 2026-10-08. Based on the current Activities redesign and Family alignment in the working tree, including uncommitted files. Scope is presentation only; this document neither changes the active product baseline nor authorizes implementation.

Read this before any UI work. The [UX contract](core/ux-contract.md), [product contract](core/product-contract.md), [privacy and safety contract](core/privacy-safety-contract.md), and applicable workflows continue to govern behavior, access, and capabilities. Their requirements take precedence over a visual example. This standard replaces older mockup palette/layout guidance for future visual work; it does not require retroactive restyling of unrelated pages.

## Design posture

Blevins should feel modern, premium, clean, minimal, consistent, readable, and easy to navigate: dark and cinematic with quiet depth, privacy-conscious and trustworthy. Tesla and Linear are inspiration; implemented Blevins values below are authoritative. Avoid generic administrative dashboards, excessive card nesting, decorative chrome, bright cyan outlines on every card, and glow. Use available space without crowding; choose layouts for the task rather than imposing the Family composition everywhere.

## Palette and surfaces

Reuse the semantic tokens in [styles.scss](../src/FamilySafety.Web/src/styles.scss). The following are existing values, not a new palette.

| Role | Token / implemented value |
|---|---|
| Base page | `--color-page: #000` |
| Navigation / top bar | `--color-shell: #0b0c0e` |
| Card / inset surface | `--color-surface: #111`; `--color-surface-soft: #17191c` |
| Primary text | `--color-text: #fff`; `--color-primary-strong: #fff` (white, despite its name) |
| Secondary / helper text | `--color-muted: #a5adb9`; `--color-secondary-text: #f1f4f8`; neutral text `#e1e7ee` |
| Borders | `--color-border: #ffffff14`; control border `--color-border-strong: #59616d` |
| Cyan / focus | `--color-primary`, `--color-accent`, `--color-focus`: `#00d4ff` |
| Primary hover / soft accent / on-primary text | `--color-primary-hover: #79e8ff`; `--color-primary-soft: #142126`; `--color-on-primary: #001418` |
| Secondary control / hover | `--color-secondary: #202328`; `--color-secondary-hover: #30363e`; hover border `#8795a6` |
| Success | `--color-success: #8de0b0`; `--color-success-bg: #13271c` |
| Warning / attention | `--color-warning: #f4cb89`; `--color-warning-bg`, `--color-attention-bg`: `#302519` |
| Error | `--color-error: #ffadb5`; `--color-error-bg: #321b20` |
| Neutral icon / avatar | `--color-icon: #bdc7d4`; `--color-neutral-bg: #24282d` |

The approved atmospheric treatment is **black beneath a subtle blue-black gradient**, not a flat-black replacement of the content surface:

- Activities and Family main: `radial-gradient(ellipse at 40% 0, #04141e 0, transparent 55%)`, in `activity-forms.scss` and `family.scss` respectively. Activities applies through the shell's Activities area class, including its Create/Edit routes; Family applies to the homepage.
- Family directory/summary cards: `linear-gradient(145deg, #11181e, #0b1014 80%)`.
- Family nested organization/date rows: `linear-gradient(120deg, #10171d, #0a1015)`; hover `#10202a`, border `rgb(0 212 255 / 30%)`.
- Family search/settings controls: `linear-gradient(145deg, #14191f, #0c1116)`; member hover `#0c2029`.
- Navigation hover: `#1a1d21`; selected: `#132328` with primary-hover text. Activity selected filters use `#06212a` with cyan border/text.

Activities rows use the surface tokens, not the Family gradient everywhere. Existing category badges use Event `#64e7ff` on `#082637`, Plan `#51e7a0` on `#072b1d`, Trip `#d895ff` on `#28123a`. These denote types, not success, danger, or permissions; retain their text/icon labels.

Use existing tokens first and the relevant scoped gradient where appropriate. Do not invent parallel colors or a second theme. State colors describe actual feedback; safety UI stays calm and must not infer danger from color.

## Typography

The global stack is `Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif`, with `font-synthesis: none`. Inter is a preferred family, not a guarantee that a font asset is loaded. Body/shell line-height is `1.55`.

| Element | Established pattern |
|---|---|
| Activities / Family h1 | `clamp(2rem, 3vw, 2.7rem)`; interior heading tracking `-0.04em`, line-height `1.2`; Family weight `650`, Activities inherits interior `550` |
| Other interior h1 | `clamp(1.75rem, 2.8vw, 2.25rem)`, weight `550` |
| Subtitle | Muted, line-height `1.65`, heading bottom margin `10px`; global subtitle max-width `52rem` |
| Family section title | `1.2rem`, weight `650`; Activities h2 `1.2rem` |
| Shared interior card title | `1.05rem`, weight `600`, tracking `-0.02em`, line-height `1.4` |
| Labels / legends | Text token, weight `650`; visible associated label above control |
| Supporting copy | Shared item meta `0.85rem` / `1.6`; Family summary copy `0.9rem`; card subtitle `0.8rem` / `1.55` |
| Helpers / errors | Global helper and field error `0.88rem`; helper line-height `1.5`; activity count `0.8rem` |
| Buttons | `0.875rem`, weight `600`, line-height `1.4` |
| Empty state | Muted supporting text, readable heading, useful next action; Activities empty copy line-height `1.6` |

Preserve this hierarchy and relevant scoped variants. Avoid arbitrary heading scales, all-caps decoration, and excessive emphasis. Long names/titles wrap; do not trade readability for a mockup's sample text.

## Shell, layout, and spacing

Reuse the shell in [app.html](../src/FamilySafety.Web/src/app/app.html) and global styles: top bar across the frame, left sidebar, main content; focused invitation/handoff views omit navigation. Desktop sidebar is `--sidebar-width: 13.5rem`; top bar minimum height `76px`, padding `0.6rem clamp(1rem, 2.5vw, 1.75rem)`. Navigation uses `28px 12px` padding and `6px` gaps. Main is centered with `min-width: 0` and `width: 100%`.

- Shared main max-width `1264px`, gutters `40px 32px`; Activities/Family max-width `1500px`, padding `28px`. Keep the scoped content gradient with these layouts.
- Page heading precedes content; action/search sits alongside where space permits. Activities/Family heading bottom gap is `24px`; ordinary interior headings use `32px`, `24px` at phone widths.
- Existing spacing tokens `--space-1` through `--space-6`: `0.25`, `0.5`, `0.75`, `1`, `1.5`, `2rem`. Common page/card gaps are `16–24px`.
- Activities tools/list with sidebar: `minmax(0, 2.5fr) minmax(16rem, 1fr)`, gap `1.25rem`; list gap `0.75rem`.
- Family desktop: `minmax(0, 2.1fr) minmax(280px, 1fr)`, gap `20px`. Directory comes first in DOM/mobile reading order; organization is placed left and directory right on desktop. Directory uses three member columns.
- Event/Plan Create grid: details/audience `1.45fr / 1fr`, gap `2rem`; audience inset padding `1rem`. Trip forms use their own sections; Edit forms need not duplicate the Create composition.

Avoid redundant panels and intermediary pages when an existing direct destination serves the task. Keep meaningful progressive disclosure; layout does not authorize workflow changes.

## Cards, rows, and icons

Shared `.card` uses surface, 1px border token, `--radius-lg: 1rem`, `--shadow-card: 0 12px 32px #0003`. Interior padding is `24px`, phone `20px`; Activities sidebar cards use `1rem`, Family phone cards `20px 16px`. Existing radii: small `0.5rem`, medium `0.75rem`, large `1rem`. Buttons use `10px`.

Shared row lists separate rows with a 1px border and `--space-3` top padding, omitting the first separator. Activities uses individual bordered rows: padding `1rem`, minimum height `7.5rem`, large radius; hover changes to soft surface and cyan border. Family summary rows use medium radius, `18px 16px` padding, `10px` list gaps. Nest a surface only for a distinct group, audience section, or selectable row; keep depth restrained. Preserve global visible focus on links and controls.

Use [AppIcon](../src/FamilySafety.Web/src/app/icon.ts), the existing inline SVG outline component, not another icon package. SVG is `20×20`, viewBox `0 0 24 24`, stroke `1.8`, round caps/joins, `currentColor`. Icons are decorative (`aria-hidden`, not focusable); the enclosing control supplies its name.

- Shared card icon container: `2.25rem` square, radius `0.6rem`; interior uses secondary fill and icon token.
- Family header icons: `40px` square, medium radius, cyan on `#06222c`, cyan border at 12% opacity; household symbols `48px` square. Activity chooser type tiles are `3.5rem` square.
- Align icon and label with flex/grid, no baseline hacks. Family settings is a `52px` square named link; search-clear is at least `44px`. Every icon-only action needs an accessible name on its button/link.

## Forms and actions

Reuse native inputs/selects/textarea and Angular form patterns. Global control minimum height `2.75rem` (44px at a 16px root), padding `0.55rem 0.75rem`, small radius, surface background, strong border, full width. Select options use soft surface and text tokens. Textareas start at `6rem`, resize vertically; the Activity audience textarea uses `7rem`. Activity search/filter controls are `3.25rem`; Family search is at least `52px`.

- `.field` uses an `8px` label/control gap and `16px` top spacing; paired grids use `16px` gaps. At `44rem+`, shared field grids align label/control/feedback tracks with subgrid so validation does not displace a neighbor.
- Required indicators accompany semantic required state; the decorative `*` may be hidden from assistive technology. Keep associated labels, field hints, actionable inline errors, and validation summaries/alerts. Connect errors with `aria-describedby` and invalid state as appropriate; never imply existing templates universally implement all associations.
- Native radio/checkbox accent is cyan; input size `1.2rem`, with labeled choice target at least `44px` in interior pages. Preserve legends and audience explanations.
- Primary action: solid cyan with on-primary text; secondary: secondary fill, strong border, light text; link action: quiet transparent treatment. Reuse `.button-link` for navigation and buttons for commands. Destructive actions use `.button--danger` and existing confirmations; stopping sharing remains an ordinary prominent privacy action.
- Event/Plan Create footer separates actions from fields with a border, `1rem` padding/margin, Cancel left and submit right. Edit uses existing Save/Cancel rows (Event/Plan save first); Trip Cancel is in the form header, submit below sections. No single action order or footer layout is implemented universally: preserve the relevant workflow's arrangement and destinations.
- Disable during pending writes, retain loading text such as “Saving…”, and prevent duplicate submissions. Existing disabled opacity: inputs `0.6`, buttons `0.55`, with `not-allowed` cursor; read-only input uses soft surface.

Prefer dedicated Create/Edit screens for substantial new workflows. Current type chooser and routes are examples, not authorization to relocate existing forms automatically. Keep small note/audience edits separate where the domain requires them.

## Navigation and interaction

Reuse [navigation.ts](../src/FamilySafety.Web/src/app/navigation.ts) and [app.routes.ts](../src/FamilySafety.Web/src/app/app.routes.ts), rather than defining parallel destination state. Nav rows have icons/labels, minimum height `48px`, `10px` radius; selected state uses the colors above plus `aria-current` (`page` for destination, `true` for its children). User navigation carries `focusHeadingState`; headings use `tabindex="-1"` for focus after navigation.

The approved primary destinations are Home, Family, Activities, Safety, Me / Settings, in that order at desktop and narrow widths, under [REQ-UX-018](core/ux-contract.md#req-ux-018). Activities groups Events, Plans, and Trips for navigation only; domain workflows and authorization remain separate. The owner reconciliation on 2026-10-08 supersedes the former six-destination rule in `REQ-UX-013`.

Activities shows labeled type filters with counts and `aria-pressed`, labeled search, and a date select; Family uses labeled search with named clear action. Rows/names that navigate are real router links with refresh-safe domain item URLs. Details expose authorized edit links. Create Cancel returns to Activities; Event/Plan/Trip Edit Cancel returns to the item's details. Back links name the destination. Preserve existing redirects, privacy-safe unavailable states, and server-authorized item reads before Family adoption; a link, selected nav state, or displayed Family never grants access.

Use textual `role="status"` for loading/success, `role="alert"` for errors, and a clear retry action for recoverable reads. Distinguish empty results, filtered-out results, incomplete counts, loading, error, and nondisclosing unavailable states. Do not replace them with decorative skeletons or infer hidden item existence.

## Responsive and accessibility review

Use actual source breakpoints, not a new breakpoint system. Rem-to-pixel equivalents below assume the default 16px root.

| Existing breakpoint | Behavior |
|---|---|
| Shell max `63.99rem` (just below 1024px) | Top bar, navigation, then main; nav three columns; shared main `32px 24px` |
| Shell max `43.99rem` (just below 704px) | Nav two columns; top bar `20px`; shared main `28px 16px`; Family selector stacks, profile name hides, interior action rows become full-width |
| Activities max `65rem` (1040px) | Tools/content become one column; sidebar becomes two columns |
| Activities max `40rem` (640px) | Filters two columns, sidebar one; date/badge/copy/chevron reflow |
| Create grid max `55rem` (880px); chooser max `50rem` (800px) | Details/audience stack; type chooser stacks respectively |
| Activities main max `44rem` (704px) | Main `24px 16px`; title/action stack |
| Family min `64rem` (1024px) | Two-column desktop arrangement; below it directory precedes organization |
| Family max `79.99rem` (just below 1280px) | Heading search/actions wrap to a new row |
| Family max `43.99rem`; max `40rem` | Summary rows/header reflow and compact card padding; main `24px 16px` at 640px or less |

Shared two-column field/landing grids begin at `44rem`; `.split-layout` at `68rem`. Preserve these only where relevant. Scoped Activities/Family gutters override shared shell gutters; a 768px Family main remains `28px`, while Activities main changes at 704px. Do not flatten the cascade into an invented universal measurement.

For future authorized UI changes, visually review **1440px desktop, 768px tablet, and 390px mobile**, plus affected breakpoint boundaries. Check no horizontal overflow, long names and labels, stacked content, usable forms/actions, readable hierarchy, accessible navigation, and touch targets of at least 44px for these patterns. No browser review is claimed by this documentation task.

Accessibility takes precedence over visual fidelity: preserve logical keyboard/DOM order, semantic controls, labels, screen-reader status/errors, and visible focus. Global focus is `3px solid var(--color-focus)` with `2px` offset (interior buttons/links `4px`; Family custom focus varies). Maintain WCAG AA contrast, do not use color alone, and honor reduced motion; global button/link transitions are `160ms` and disabled under `prefers-reduced-motion: reduce`. Verify any new animation similarly. Source inspection alone does not certify contrast or accessibility across all states.

## Practical references and known gaps

All source paths below are relative to this document; inspect the working tree before copying a pattern.

| Reference | Reuse |
|---|---|
| [Activities component](../src/FamilySafety.Web/src/app/activities/activities.ts), [template](../src/FamilySafety.Web/src/app/activities/activities.html), [styles](../src/FamilySafety.Web/src/app/activities/activities.scss) | Filters, authorized list projection, date/type rows, contextual sidebar, empty/loading/error/retry presentation |
| [Activity type chooser](../src/FamilySafety.Web/src/app/activities/activity-type-chooser.ts), [activity-forms.scss](../src/FamilySafety.Web/src/app/activity-forms.scss) | Type tiles, content gradient, Create grid, audience inset and footer |
| [app.html](../src/FamilySafety.Web/src/app/app.html), [app.ts](../src/FamilySafety.Web/src/app/app.ts), [events.scss](../src/FamilySafety.Web/src/app/events.scss), [events-dashboard.scss](../src/FamilySafety.Web/src/app/events-dashboard.scss), [plans.scss](../src/FamilySafety.Web/src/app/plans.scss) | Family homepage, Event/Plan Create and Edit, headings, state/authorization boundaries |
| [family.scss](../src/FamilySafety.Web/src/app/family.scss), [family-summary.scss](../src/FamilySafety.Web/src/app/family-summary.scss), [Households](../src/FamilySafety.Web/src/app/households/households.html), [Shared Family Dates](../src/FamilySafety.Web/src/app/family-dates/shared-family-dates.html) | Directory, search/settings, summary card/row gradients and responsive organization |
| [Trip template](../src/FamilySafety.Web/src/app/trips/trips.html), [styles](../src/FamilySafety.Web/src/app/trips/trips.scss), [component](../src/FamilySafety.Web/src/app/trips/trips.ts) | Dedicated Create/Edit sections, native choices, loading and cancellation |
| [Global styles](../src/FamilySafety.Web/src/styles.scss), [app.scss](../src/FamilySafety.Web/src/app/app.scss), [home-shell.scss](../src/FamilySafety.Web/src/app/home-shell.scss), [AppIcon](../src/FamilySafety.Web/src/app/icon.ts) | Tokens, shell, shared cards/controls/focus, scoped Home exceptions, outline icons |

Consistency means shared visual patterns, not identical pages. Family's two-column organization is not mandatory for forms or other workflows. Respect the [client structure rules](../src/FamilySafety.Web/README.md#structure-and-extraction); no new framework, icon library, broad tokens, folder reshuffle, or component refactor follows from this contract.

Known gaps to carry forward:

- Navigation governance is reconciled by `REQ-UX-018`, replacing `REQ-UX-013`. Older six-area shell comments are historical wording, not a requirement to restore separate Events and Plans & Trips sidebar destinations.
- [Mockup alignment](../claude-handoff/MOCKUP-ALIGNMENT.md) summarizes historical light-reference provenance and superseded mappings. This owner-requested contract governs current visual language; historical screenshots remain evidence, not authority for new capabilities.
- Heading weights, card padding, gradients, and Save/Cancel arrangements have deliberate scoped variants. No universal gradient token or shared action-footer component exists. Some accessibility error associations require case-by-case verification. These are not mandates to refactor.
- A later bounded change could centralize repeated atmospheric/Family gradient values if duplication becomes a maintenance problem. Reuse current styles now; do not build a design-system framework speculatively.

## Instructions for future AI workers

Before implementing or modifying UI:

1. Read this contract and the task's relevant canonical behavior/privacy requirements.
2. Inspect the relevant existing shared styles/components and their scoped overrides.
3. Reuse established colors, gradients, surfaces, typography, controls, icons, and spacing.
4. Preserve functionality, audience rules, authorization, and Family-state reset behavior.
5. Avoid parallel styles and unnecessary abstractions.
6. Make the smallest maintainable authorized change.
7. Verify visual language, responsive states, keyboard behavior, and accessibility with checks appropriate to that change.

If a mockup differs, follow an explicitly approved new direction within its scope; never silently replace the application's standard. Mockups do not authorize business capabilities or privacy behavior. This documentation task authorizes no runtime change, build, regression suite, or Playwright execution.

Reusable future-feature directive:

> Follow the Blevins canonical UI Visual Design Contract and existing Activities/Family design patterns. Reuse established backgrounds, gradients, typography, cards, icons, controls, layout spacing, and responsive conventions. Preserve existing functionality and privacy/authorization. Do not introduce an alternative visual style without explicit approval.
