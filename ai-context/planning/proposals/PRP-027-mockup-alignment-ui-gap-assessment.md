---
Status: CLOSED / COMPLETED — approved implementation purpose fulfilled
Authority: Project-owner closure authorization recorded in DEC-065; approved requirements and existing behavior remain authoritative
Context-Baseline: CTX-1.24
Version: 1.1
Execution-Authorization: NO
---

# PRP-027 — Mockup Alignment & UI Gap Assessment

## Metadata

- **Proposal ID:** `PRP-027`
- **Status:** `CLOSED / COMPLETED` — approved implementation purpose fulfilled under `DEC-065`; individual story history remains unchanged
- **Date:** `2026-09-29`
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.24` (active)
- **Phase:** Phase 7.5 — End-to-End UX & User-Test Readiness (`COMPLETE / CLOSED` under `DEC-065`)
- **Required context:** `project-status.md`; `planning/roadmap.md`; `planning/decision-log.md`; active UX and privacy/product contracts; PRP-023; WI-075-004 and VER-WI-075-004; WI-075-005 and VER-WI-075-005; WI-075-008 and VER-WI-075-008; UX reference inventory; Angular shared styles, shell, app and Trip templates/styles.
- **Affected authority:** Presentation direction and future implementation planning only. No canonical contract is amended.
- **Model tier:** `STANDARD`
- **Execution authorization:** `NO`

## Current closure disposition — 2026-09-29

PRP-027 is `CLOSED / COMPLETED` under owner-approved `DEC-065`. Its approved implementation purpose was fulfilled by the six completed slices: WI-075-010 shared shell/core patterns, WI-075-011 Home, WI-075-012 Family and first-use, WI-075-013 Events, WI-075-014 Plans, and WI-075-015 Trips. The useful mockup/reference direction was applied within existing approved behavior; privacy/access rules were preserved and unsupported mockup-only functionality was intentionally excluded. WI-075-005 remains DONE with accepted evidence limitations. Remaining spacing, density, responsive, and minor visual refinement is future polish, not unfinished PRP-027 scope. No further PRP-027 implementation slice is known. OD-001 remains OPEN; Phase 7.5 is closed and Phase 8 remains FUTURE. This status update changes no canonical requirement and does not authorize new work.

This current disposition supersedes the proposal-time recommendations below. Sections describing candidate priorities, groupings, and implementation order are retained as historical planning intent, not pending commitments.

## 1. Executive summary

The current Angular implementation already completed a broad visual refinement in WI-075-004. Its warm neutral background, evergreen/sage palette, centered page frame, grouped surfaces, visible active route, current-Family context, responsive forms, readable status patterns, keyboard focus, and action hierarchy express the approved family-centered direction. The verification report records visual smoke at 390, 820, and 1440 CSS-pixel widths and key contrast checks. The references should therefore be treated as directional inspiration, not as a pixel target or a reason to redesign stable workflows.

The clearest remaining alignment opportunity is consistency and emphasis across the existing shell and shared UI patterns: the references lean toward a persistent, recognizable navigation rail and strong, concise page identity, while the implementation uses a wrapping horizontal navigation row and page content whose consistency depends on shared SCSS conventions. A future pass should assess small, global refinements to shell hierarchy, spacing, cards, buttons, forms, and status/empty states, then address only substantiated screen-level gaps. It should preserve the implemented routes and the unresolved mobile grouping decision (`OD-001`).

Some mockups depict unapproved or deferred capabilities, including Safety, activity feeds, invitations/RSVP, location sharing, broad settings, member/group administration, and live coordination. Those concepts are `DROP` for this proposal’s implementation scope. No screen should gain functionality for visual parity. Screen-specific visual decisions should use the owner-directed review and verification approach below; the broader formal human usability gate remains deferred until design maturity.

**Original assessment recommendation (superseded by closure):** use this assessment to scope small, separately reviewed presentation stories. The owner has determined WI-075-005 is DONE and its functional/user validation is sufficient for the MVP milestone, while broader formal usability validation should wait until the UI/design is substantially more mature. Do not require a new participant test before each UI story. Keep behavior, privacy, accessibility, and current contracts as guardrails; use regression, stable Playwright flows, rendered browser review, accessibility checks, and owner visual review. Do not create screenshot baselines or visual-regression acceptance gates in this phase.

### Owner direction update — 2026-09-29

This update supersedes proposal wording that makes human observations a prerequisite for continuing screen-specific visual alignment. The owner defers the broader formal usability gate until visual design, screen composition, and mockup alignment are substantially complete; human validation remains in the longer-term plan. Continue presentation work without changing approved workflows: keep current functionality/privacy/accessibility, align hierarchy, spacing, surfaces, composition, forms, actions, typography, visual character, and screen organization where behavior remains intact; adapt references to the implemented product and drop unapproved mockup-only features. `WI-075-010` is DONE and consumed the shared-foundation work described in its story and verification: current shell hierarchy/active route/Family context and shared spacing, surfaces/cards, forms, buttons/actions, status/empty patterns, and focus treatment. It did not implement screen-specific composition alignment and did not accept this proposal. At the time of this owner direction, PRP-027 was approved as the planning basis for bounded stories; that planning status was later superseded by closure under `DEC-065`.

## 2. Mockup/reference inventory

All files below are reference-only under `ai-context/references/ux/`; their accompanying notes describe useful direction and exploratory elements. None is canonical.

| Reference | Available views | Coverage and caveat |
|---|---|---|
| `home-dashboard/home-dashboard-overview.png` and `.md` | Home dashboard, family activity, travel/events summary, safety-aware dashboard | Useful compact grouping and attention-first hierarchy. Activity feed, RSVP/Needs, quick links, active-sharing summary, and Safety are exploratory or deferred. |
| `family/family-profiles-overview.png` and `.md` | Directory, member profile, groups/households, Safety Circle/trusted contacts | Useful directory/profile separation and nearby privacy context. Several depicted fields and capabilities are outside current scope or privacy rules. |
| `events/events-coordination.png` and `.md` | Event overview, create, RSVP/Needs, Updates/invitations | Useful overview-to-detail and event-context grouping. RSVP, invitations, Needs, Updates, and related controls are not implemented/authorized here. |
| `events/events-workflow.png` and `.md` | Workflow progression and coordination concepts | A separate reference image and note are present. Treat its participation, audience, invitation, Sharing, and Check-In concepts as exploratory, not approved behavior. |
| `plans-trips/trips-sharing.png` and `.md` | Trip list, creation, detail, Sharing/Check-ins | Useful grouping of trip facts, travelers, audience, and optional information. The image’s Sharing, live location, and Check-In functionality is explicitly out of scope. No dedicated Plans mockup is present. |
| `admin-settings/admin-settings-overview.png` and `.md` | Family admin, membership/roles, privacy/sharing, notifications/access review | Useful separation of administration from personal privacy. Roles/permissions, broad access review, persistent location sharing, invitation history, and notification controls are unapproved. It is not a dedicated Me/profile mockup. |
| `safety/safety-coordination.png` and `.md` | Concern, incident, information/contact, resolution/history | Future Phase 8 reference only. Safety functionality is excluded from this visual alignment proposal. |

There is no dedicated mockup for the invitation/first-use journey, Plan list/detail/form, Trip Notes, Trip audience editing/recipient preview, current simplified Me/profile route, or the implemented shell’s multi-Family selection states. Shared visual direction may inform these areas, but no absent mockup is to be treated as a specification.

## 3. Current implementation summary

WI-075-004 (`DONE / PASS`) applied a small SCSS token vocabulary in `src/FamilySafety.Web/src/styles.scss`: evergreen primary/strong/soft, sage accent, warm-neutral page, white/soft surfaces, text/muted/border/status/focus colors, spacing steps, three radii, and a restrained shadow. Shared page, section, card/list, action, form, status, focus, shell, and responsive treatments are mainly in `app/app.scss`, with Trip-specific detail in `app/trips/trips.scss`. No component library or broad design-system framework was added.

The shell in `app/app.html` has a centered app frame; page title and current Family; explicit selector when there are multiple active Families; a wrapping horizontal navigation row with visible and programmatic active state; grouped Plans and Trips destinations; and separate Development-only controls. Existing routed views cover Home, Family, Events, Plans, Trips, and Me / Settings. Home has limited Event, Plan, and current-Family Trip previews and a low-data state. The current app renders approved existing content and actions only.

The Trip route has the most locally specialized styling: responsive form grids, fieldsets, list cards, detail/Notes areas, and nested audience editor and preview. WI-075-004 reports that existing tests and visual checks preserved workflow order, field sets, audience disclosure and authorization, Family switch isolation, and action semantics. At the time of this proposal assessment, WI-075-009 was `PROPOSED`. It was subsequently completed; its recorded audience-removal/access safeguards remain preserved by WI-075-015 verification. The earlier proposal-time caution is historical.

Evidence is prior-story source review and the WI-075-004 self-verification record; this proposal did not launch the application for a new independent visual inspection. The report covers desktop/tablet/mobile and key accessibility checks but notes self-verification, two style-budget warnings, and that a valid live invitation acceptance path was not repeated in the persistent Development fixture. No visual delta below should be read as a newly observed runtime defect.

## 4–5. Screen-by-screen comparison and decisions

Complexity and churn estimates describe a future presentation-only story, not authorization. “Behavior required: No” means no behavior change is recommended; any implementation must preserve existing user-visible behavior and server authorization.

| Screen / area | Current implementation and mockup direction | Decision and rationale | User impact | Complexity | UI churn risk | Behavior required? | Suggested story grouping |
|---|---|---|---|---|---|---|
| **1. App shell / global navigation** | Current: page header, Family context, horizontal wrapping navigation; current destination is visually and programmatically identified. Plans and Trips are grouped yet remain separate routes. References mostly show a persistent vertical rail, brand/search/account furniture, and more destinations. | **ADAPT** the mockup’s persistent, easy-to-scan destination affordance to the existing small MVP shell and responsive layout. Keep current routing, active state, Family selector, and Development separation. Assess shell spacing/visual grouping as a global refinement; do not copy search, notifications, Messages, Safety, or other unimplemented destinations. **DEFER** final mobile navigation form because `OD-001` remains open. | Clearer wayfinding and family context, especially on narrow screens, without expanding destinations. | Medium | Medium | No | Shared shell/navigation alignment |
| **2. Home** | Current: compact, constrained previews for Events, Plans, and current-Family Trips plus reassuring low-data state and destination actions. Mockup: multi-column attention, activity, RSVP/tasks, active sharing, Safety, and broad “family activity” dashboards. | **KEEP** current data scope, simplicity, and calm low-data presentation. **ADAPT** hierarchy, spacing, surface grouping, and composition of existing previews/empty state toward the reference where useful, reviewed in browser and by owner. **DROP** activity feed, broad “who is where” signals, Safety panel, RSVP/tasks/sharing concepts not approved for Home. | Makes the implemented Home more reference-aligned without implying monitoring or unapproved attention features. | Low | Low | No | Home visual alignment |
| **3. Family** | Current: directory, selected profile details, relationship area, and contextual structural administration separated into sections. Mockup: dense searchable/filterable directory, richer profile, group/household tabs, sensitive profile fields, Safety Circle and trusted contacts. | **KEEP** scannable directory and contextual sections. **ADAPT** concise member-card grouping and nearby privacy context where supported by existing fields. **DROP** minor data, extra profile fields, households/groups, Safety Circle, trusted contacts, and broader family management unless separately approved. | Easier scanning while retaining least disclosure and current permissions. | Low | Low | No | Family / first-use alignment |
| **4. Invitation / first-use flow** | Current: invitation preview and Accept/Decline, status/error/retry, optional profile review, and Family/Home handoff. No dedicated mockup; references do not define these steps. | **KEEP** working journey and statuses. **ADAPT** common page/surface/action treatment only; do not invent steps or make acceptance/handoff look like mockup onboarding. | Consistent presentation without burdening a first-time invitee or obscuring the invitation decision. | Low | Low | No | Family / first-use alignment |
| **5. Events** | Current: existing event create/list/detail/editor and audience controls with grouped form fields, cards, statuses, and empty treatment. Mockups emphasize rich overview cards then RSVP/Needs/Updates/invitations tabs. | **KEEP** current overview/create/detail grouping. **ADAPT** concise event summary and clearer local status/context where those statuses already exist. **DROP** RSVP, invitations, Needs, Updates, calendar, and other mockup-only event features from visual parity work. | More coherent scan of approved event data without promising coordination capability that does not exist. | Medium | Medium | No | Events/Plans shared list/detail patterns |
| **6. Plans** | Current: a real Plan workflow styled with shared forms/lists and intentionally lighter presentation than Trips. No Plans-specific reference image. | **KEEP** distinction from Trips and the current familiar shared patterns. **ADAPT** only shared list/card and form consistency; use Events/Trips references as general hierarchy inspiration, not as Plan specifications. | Supports recognition and consistency while preserving distinct concepts. | Low | Low | No | Events/Plans shared list/detail patterns |
| **7. Trips** | Current: responsive Trip fields, details, travelers, audience, optional information, list, and empty/status states. Mockup: image-led cards, filters, traveler chips, and grouping of basic/additional travel data, plus separate sharing controls. | **KEEP** readable sections, existing required/optional field distinctions, and visibility clarity. **ADAPT** summary grouping and optional detail disclosure only if it preserves existing field order and discoverability. Do not add imagery, filters, trip states, or sharing controls just to imitate the reference. | Easier scan of dense trip information while retaining clear audience meaning. | Medium | Medium | No | Trips visual alignment |
| **8. Trip details / Notes / audience editing** | Current: text-led detail, traveler/audience labels, creator-only complete-audience editor with preview/reconfirmation, Notes area, and access explanations. No mockup for Notes or audience replacement flow; Trip image shows only broad audience edit and Sharing. | **KEEP** precise access/status context, Notes placement, and current audience-review safeguards. **ADAPT** grouping/spacing of detail and editor panels if review finds scanning friction. **DROP** mockup implication that Trip visibility controls location-sharing permission; no location control. **DEFER** fine layout tuning until human task evidence is available. | Preserves trust around who can see a Trip, who can edit its audience, and who retains creator access. | Medium | Medium | No | Trips visual alignment; shared forms/cards cleanup |
| **9. Me / profile / settings** | Current: profile groups, field-level visibility controls, privacy help, Save action, and optional first-use continuation. No direct mockup; the admin/settings image is broad and depicts many settings not present. | **KEEP** explicit per-field visibility and local privacy help. **ADAPT** grouping and action consistency with shared form patterns. **DROP** broad security/account/notification/location settings and any profile fields shown only in reference imagery. | Keeps privacy decisions legible and personal settings distinct from administration. | Low | Low | No | Forms/buttons/shared cards cleanup |
| **10. Shared patterns** | Current: common tokens and global app styles; Trip-specific CSS duplicates some component treatments. References use repeated rounded surfaces, section headings, iconography, color/status badges, and dense cards, with substantial cross-mockup variation. | **ALIGN** a small set of existing patterns where actual routes diverge: page/section spacing, card padding, heading scale, action hierarchy, form field spacing, and empty/status surface treatment. Prefer shared token/selector changes. Keep semantic text and avoid relying on color/icon alone. **DEFER** icon system, imagery, or extensive type redesign absent evidence. | Consistency lowers learning effort and makes the product feel intentional across screens. | Medium | Medium | No | Shared forms/buttons/cards cleanup |

### A–O review coverage

For every screen above, the assessment considered composition, hierarchy, navigation, density/whitespace, card grouping, typography, palette/token use, primary/secondary actions, empty state, form consistency, status/context clarity, responsive needs, developer/admin-tool tone, human/family tone, and accessibility effects. Screen-specific findings are captured in the comparison rationale. The cross-screen conclusion is that current implementation already establishes the intended human/family tone; remaining opportunities are consistency and shell emphasis, rather than missing decoration or mockup features.

## 6. Shared cross-screen gaps

These are candidates to address once in shared styles and markup conventions, then apply selectively:

1. **Shell emphasis and responsive wayfinding:** the horizontal navigation and wrapping narrow-width controls are functional and were visually smoke-checked. A future refinement may improve visual grouping and touch scanning. Do not choose fixed bottom tabs or a final mobile destination count while `OD-001` is open.
2. **Card and section rhythm:** set and consistently apply existing page/section/card spacing, padding, border, and radius values. Review `app.scss` versus `trips.scss` duplication first; avoid a new component framework.
3. **Heading and page identity:** keep a stable page-title, section-title, and helper-text hierarchy across routed views. Avoid overloading pages with mockup subtitles or decorative chrome.
4. **Action hierarchy:** align primary commit, secondary, quiet/navigation, cancel, and destructive appearances across app and Trip styles. Keep text labels, current enabled conditions, and destructive meaning explicit.
5. **Forms:** align label/help/error spacing, grouped fields, checkboxes/radios, and responsive paired-field behavior. Trip styles have their own control rules; check actual differences before consolidating.
6. **Empty/loading/error/success patterns:** use consistent spacing and surface/border treatments, while keeping the current textual distinction among no data, no results, loading, and failure. Existing messages/actions remain authoritative.
7. **Token application:** favor current evergreen/sage/warm-neutral/status/focus tokens over literal mockup blue, one-off hex values, or screen-specific shadow/radius overrides.

These are assessment targets, not claims that every screen currently fails. A short implementation story should name concrete instances after review rather than mechanically restyling every component.

## 7. Token/component alignment findings

**Keep:** current semantic CSS custom properties for primary, accent, page/surfaces, text/muted text, border, success/warning/error, and focus; spacing/radii; restrained shadow; Inter/system stack; shared app stylesheet approach. This vocabulary supports the intended warm, calm direction and documented contrast. No new design system appears necessary.

**Apply more consistently:** shared tokens across Trip-specific and app-level controls/surfaces. In particular, verify Trip secondary button hover color, borders, card padding, headings, and statuses against common patterns before any change. Existing Trip styling includes some local declarations; retain them when they express a genuine Trip-specific need.

**Potential missing shared abstraction:** there is no reusable Angular component library for cards, page headers, form fields, or status callouts; current shared SCSS may be enough for the present app size. Only introduce a small shared class/mixin/token if it removes real duplication and remains readable. Do not create a generalized component/design system as part of this proposal.

**Style-budget note:** VER-WI-075-004 and VER-WI-075-008 record successful production builds with existing component-style budget warnings (`app.scss` and `trips.scss` above warning threshold, below failure threshold). A future shared-style cleanup could reduce duplication, but should not become a standalone performance project or cause churn solely to satisfy the warning.

## 8. Accessibility considerations

WI-075-004 reports semantic headings/labels, `aria-current` active navigation, visible 3 px keyboard focus, reviewed enabled-button heights of at least 44 px, no color-only status communication, and key contrast ratios meeting WCAG AA in reviewed combinations. Preserve those gains.

Any future alignment should:

- Preserve accessible names, labels, heading order, keyboard operation, visible focus, and programmatic active state.
- Keep status meaning in text and structure as well as color/icon; do not reproduce tiny mockup labels or low-contrast muted text without verification.
- Keep audience choices, privacy help, error association, and confirmation language visible and understandable at zoom/narrow widths.
- Treat decorative imagery as optional and non-essential; no information or action may be image-only.
- Review touch target usability and recheck actual color combinations whenever shared tokens change.

This proposal recommends behavior/accessibility regression checks for approved changes, not a claim of full accessibility certification.

## 9. Responsive considerations

Prior visual verification reports no document-level overflow across six destinations at 390×844, 820×1000, and 1440×1000. The shell navigation wraps on narrow screens; this should be observed with real task paths and longer labels, but is not a demonstrated failure. Trip forms and headings stack at narrow widths, and the Trip audience preview was reviewed at 390 px.

Flag for later review: navigation scanning/touch reach, multi-Family selector width, longer localized or user-entered content in cards, action wrapping in forms, audience recipient lists, and dense Family directories. Do not launch a breakpoint redesign project under PRP-027. Any obvious usability issue found while implementing an approved bounded story should be reported with evidence and separately scoped if it needs behavior or navigation decisions. Detailed responsive visual regression remains deferred.

## 10. Priorities

### P1 — High-value alignment

- **Shared shell hierarchy (presentation only):** clarify page identity, Family context emphasis, and current active destination within the existing shell and route set. A bounded presentation refinement that keeps the current horizontal navigation structure can be considered before human testing if independently approved; persistent-rail replacement and mobile composition changes are deferred while `OD-001` is open.
- **Shared pattern consistency pass:** identify and align concrete differences in section/card spacing, heading levels, action treatments, form control/help spacing, and statuses through current tokens/shared styles. Keep the scope to stable, observable inconsistencies; do not restyle screens simply for uniformity.
- Owner visual review and rendered/accessibility verification guide presentation decisions; participant usability testing is not a prerequisite for each UI story. Each story still requires separate approval and explicit execution authorization.

### P2 — Useful refinement

- **Home screen-specific alignment (recommended next):** bring the composition and visual grouping of existing Event/Plan/Trip previews and low-data state closer to the Home reference. Keep approved content scope/cap, current-Family Trip boundary, privacy-safe wording, and existing actions; do not claim user-discovery findings without evidence.
- **Family/first-use:** refine existing directory/profile grouping, privacy context, and invitation surface consistency using current data and journey.
- **Events/Plans list and form consistency:** shared summary/form alignment while preserving each workflow’s concepts and existing status.
- **Trips detail/readability:** adjust detail, Notes, and audience editor panel rhythm based on task observations; preserve creator-only editing and review/reconfirmation.

### P3 — Later polish

- Small typography/spacing/icon refinements that have low measured impact; optional surface decoration; build-warning reduction if it naturally follows from shared-style cleanup. No broad imagery or decorative rebuild.

### Deferred

- Final mobile navigation design (`OD-001`); detailed responsive exploration and visual regression; any change dependent on WI-075-005 outcomes; future Phase 9 feedback-informed deeper integration; any unresolved UI behavior or product decision; all mockup-only capabilities listed under exclusions.

## 11. Suggested story groupings

Candidate group names for later proposal/authorization only; no implementation stories are created here:

1. **Shared shell/navigation alignment** — independently implementable and verifiable against current route/active-state behavior, Family context, and Development separation. Limit to presentation of the current route set. Defer persistent-rail/mobile-composition decisions tied to `OD-001`.
2. **Shared forms, buttons, cards, and statuses foundation** — one cross-screen story for concrete token application, field/help/error spacing, action hierarchy, card/section rhythm, and empty/status surfaces. This is the shared-pattern foundation; screen stories should consume it rather than duplicate its CSS work.
3. **Home visual alignment** — independently verifiable on existing Home states and routes; existing content only, using owner visual review and rendered browser/accessibility checks, preserving approved preview scope/cap and privacy-safe attention wording.
4. **Family and first-use alignment** — related but two journeys. Keep one story only if it can remain bounded to existing directory/profile/admin/invitation patterns and one clear verification slice; otherwise split into Family presentation and invitation/first-use presentation. Neither may add data or steps.
5. **Events and Plans shared list/detail patterns** — common presentation work is sensible, but retain distinct Event and Plan form/list/detail semantics. Split if the joint work becomes too broad; do not repeat the shared foundation from group 2 or add RSVP/Needs/Updates.
6. **Trips visual alignment** — independently verifiable across existing list/create/detail/Notes/audience surfaces. Keep creator/access semantics and audience review intact; defer audience editor layout changes until WI-075-009 disposition and WI-075-005 observations are known.

**Coupling and boundaries:** Groups 1 and 2 form the shared foundation and should precede screen-level groups. Groups 3–6 should reference that foundation and change only evidenced local presentation. Group 2 overlaps every screen by design, so acceptance criteria must name shared selectors/patterns and explicitly exclude redoing those same changes in groups 3–6. Group 5 can be one implementation story only if verification remains small; group 4 may need to split based on independent journey acceptance checks. All six are behavior-neutral and locally verifiable through source review, accessibility checks, and existing behavior regression; none requires pixel assertions. No implementation story is created by this assessment.

## 11a. Proposed implementation order

1. After PRP-027 review/acceptance and separate story authorization, do the bounded shared shell presentation work that does not decide mobile grouping.
2. Establish the shared token/pattern foundation once (or combine it with step 1 only if scope and verification stay clear).
3. WI-075-010 completed the shared-foundation slice. Continue with Home screen-specific visual alignment, then Family/first-use, Events/Plans, and Trips as separately bounded, non-overlapping work. Use rendered review and owner visual review; reserve broader participant usability validation for when screen composition is substantially mature.
4. Leave P3 polish, detailed responsive refinements, and any OD-001-dependent navigation composition until layouts and product feedback settle.

Do not treat this order as implementation authorization or as a requirement to execute all candidate groups.

## 11b. Safe before vs. better after human testing

**SAFE BEFORE HUMAN TEST** (only through a separately approved, bounded story): correct a clear shared-token inconsistency; improve current shell title/active-route/Family-context hierarchy without changing routes or navigation grouping; align existing button, form-label/help/error, card, or status treatments when semantics and wording stay unchanged; fix an obvious accessibility defect such as inadequate focus visibility or contrast. Verify affected routes and current behavior.

**OWNER REVIEW / CAREFUL SCOPE:** Home content hierarchy/emphasis; Family or first-use grouping; Events/Plans composition; Trip detail/Notes/audience-editor layout; substantial spacing/type changes that shift hierarchy. These may proceed as visual-alignment proposals without participant-test prerequisites, but must preserve approved workflow, privacy, accessibility, and semantics and receive rendered browser and owner visual review. Trip audience layout must also preserve WI-075-009 safeguards. Broader formal participant usability validation remains deferred until UI/design maturity; any discovered accessibility or privacy issue should be separately prioritized and authorized.

**DEFERRED:** responsive breakpoint redesign, final mobile navigation composition, screenshot baselines/visual regression, and mockup-feature-dependent layouts.
## 12. Explicit exclusions

This proposal excludes:

- Any implementation now; any new product, workflow, domain, API, route, field, state, permission, validation, or persistence behavior.
- Safety functionality or Safety navigation/panels, incident/concern handling, maps, escalation, timelines, history, or export.
- Event invitations, RSVP, Needs, Updates, attendance, task assignment, or coordination statuses not currently implemented and approved.
- Activity feeds, notifications, messaging, location sharing, Sharing Sessions, Check-Ins, or live presence.
- Broad admin/role/permission/access-review dashboards, invitation history, member removal, household/group management, trusted contacts, Safety Circles, minor profiles, or unapproved profile data.
- Production authentication, self-service Family creation/discovery/join, final mobile destination grouping, or resolution of any open decision.
- Literal mockup reproduction, screenshot baselines, pixel-perfect checks, or detailed visual-regression testing.
- Canonical requirements or contract edits. No requirement change is proposed or needed for visual alignment.

## 13. Risks and churn considerations

- **Design maturity:** Formal broader usability validation is deferred by the owner until visual design and screen composition are substantially more mature. Keep screen changes bounded and reversible, preserve approved flows and privacy semantics, and use rendered browser/accessibility checks plus owner visual review.
- **Mockup scope inflation:** references combine implemented, future, and exploratory features. Copying their density or controls could misstate product capability, expose information, or undermine trust. Maintain explicit KEEP/ADAPT/DROP boundaries per story.
- **Navigation decision pressure:** a visual treatment could accidentally resolve `OD-001`. Keep grouping conceptual and preserve current route/actions pending an explicit decision.
- **Shared-style regressions:** a global SCSS change can affect forms and action hierarchy across all routes. Limit selector breadth and inspect each affected route; verify keyboard, contrast, and existing behavior.
- **Over-standardization:** current Trip-specific audience/Notes panels have distinct access meanings. Shared visual consistency must not flatten their context or make protected actions appear generally available.
- **Responsive churn:** breakpoint-level changes may create more layout work while page density/content is still evolving. Only address demonstrated usability issues in narrow stories.

Estimated UI churn for the recommended P1 items is **LOW to MEDIUM** if implemented through tokens/shared patterns with route-level spot checks. Churn becomes **HIGH** if a story imports the vertical rail, multi-destination dashboard, tabs/filters, card imagery, or global layout wholesale.

## 14. Relationship to WI-075-004

WI-075-004 is `DONE / PASS` under `CTX-1.24` and already delivered the first approved visual refinement: shared visual vocabulary, shell/active navigation, Family context, Home, existing feature areas, forms, cards, status/empty states, focus/contrast, and responsive presentation. This proposal is an assessment and possible follow-up direction; it does not reopen that completed story, assert its acceptance criteria failed, or authorize changes. Future work should identify a concrete residual gap and reference its evidence rather than duplicate WI-075-004.

PRP-025 itself is referenced in WI-075-004, but no PRP-025 proposal file is present in the repository. The approved scope and decisions relevant here are recoverable from the WI-075-004 story and verification report. This is a documentation limitation, not an authoritative conflict; no attempt is made to reconstruct an absent proposal as canonical truth.

## 15. Relationship to WI-075-005

WI-075-005 is `DONE` based on Stage 1 technical evidence and owner/developer-supplied validation accepted for the functional MVP milestone; documented evidence limitations remain as recorded in its verification report. The owner has deferred broader formal usability validation until UI/design maturity. This proposal does not reopen WI-075-005 or change its evidence. Do not require participant testing before each visual-alignment story. Use automated regressions, stable Playwright flows, rendered browser review, accessibility checks, and owner visual review during continued development. Keep formal human usability validation in the longer-term plan as a later maturity gate.

## 16. Relationship to Playwright strategy

WI-075-008 is `DONE / PASS`; its focused Playwright journey protects invitation, identity handoff, session context, and server authorization. It explicitly avoids styling/layout assertions, screenshots as approval evidence, and visual baselines. Continue the current division: Playwright for stable browser behavior and privacy outcomes; Vitest/xUnit for lower-layer behavior/domain/API rules. For future UI stories, use accessibility semantics and focused behavior regression as guardrails. Failure-only screenshots/traces may diagnose automated failures, but must not become visual approval tests. Detailed responsive/visual regression remains later work; do not create screenshot baselines under this proposal.

## 17. Canonical requirements

**No canonical requirements need to change.** `CTX-1.24` remains active; Phase 7.5 is now closed under `DEC-065`; `REQ-UX-001/002/003/006/007/008/011/012` and relevant Family/privacy/Event/Plan/Trip contracts remain authoritative. Mockups cannot amend them. Existing design language, hierarchy, and consistency can be refined within those constraints. Any future proposal that changes routes, navigation grouping, behavior, privacy, feature scope, or permissions must separately identify and resolve its governing decision/requirement before implementation.

## 18. Open design decisions

- `OD-001` final mobile navigation grouping remains open; no recommendation here resolves it.
- The amount and sequencing of residual visual refinement should be informed by WI-075-005 observations where practicable.
- Whether a shared visual pass should precede screen-specific stories depends on concrete inconsistency examples selected at story creation; do not create new components preemptively.
- No mockup is available for Plans, first-use, Trip Notes, or Trip audience editing; future decisions in these areas must be grounded in actual behavior and approved UX contracts.

These are presentation/planning questions. Product open decisions remain untouched.

## 19. Recommended next step — superseded by closure

The proposal-time recommendation to implement WI-075-013 next is historical and was superseded by completion of WI-075-013 Events, WI-075-014 Plans, and WI-075-015 Trips. Together with WI-075-010 shared foundation, WI-075-011 Home, and WI-075-012 Family/first-use, these exhaust the known PRP-027 implementation slices. No PRP-027 story remains proposed or future. The owner subsequently closed PRP-027 under `DEC-065`; see the current closure disposition above. No new polish story is implied by residual visual differences.

## Owner disposition — 2026-09-29

The project owner approves PRP-027 as the planning basis for the remaining Phase 7.5 visual/mockup-alignment work, to be divided into bounded work items. This disposition does not mean that PRP-027 work is complete, Phase 7.5 is complete, Phase 8 begins, mockup-only features are approved, OD-001 is resolved, or future stories are authorized for execution. Each story retains its own status and execution-authorization field. WI-075-010 remains DONE as the shared shell/core-pattern foundation.

## Decision and authorization

- **Historical decision:** `APPROVED` as planning basis for bounded Phase 7.5 visual-alignment work; superseded by the closure disposition above
- **Decision ID:** Owner disposition recorded here; no canonical product decision or requirement amendment
- **Approved by:** Project owner
- **Approved at:** 2026-09-29
- **Execution authorized:** `NO` — approval does not authorize any individual story or application implementation
- **Historical next story:** `WI-075-013 — Events Visual Alignment`; it is now `DONE / PASS`. WI-075-014 Plans and WI-075-015 Trips are also complete. This original owner disposition is retained as history; current PRP-027 closure is recorded in `DEC-065`.
