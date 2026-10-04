---
Status: APPROVED
Authority: CANONICAL — CTX-1.28
Context-Baseline: CTX-1.28
Version: 1.2
Source: ../bootstrap/product-ux-source.md; DEC-064; DEC-074; DEC-075
---

# UX Contract

This contract defines approved experience rules under `CTX-1.28`. `REQ-UX-011` was clarified under `CTX-1.24` and amended under `CTX-1.27`; `REQ-UX-013` and `REQ-UX-014` were added under `CTX-1.27`; `REQ-UX-014` was amended under `CTX-1.28` (authorized items with no selectable Family, and the one-time Family-list refresh).

## Experience posture

### REQ-UX-001

The experience must be family-first and must avoid making users feel tracked or monitored. Location is shown only in the context of an authorized Sharing Session, event, plan, trip, check-in, or safety workflow.

### REQ-UX-002

The experience must prioritize attention before activity and relevance over a universal activity feed, especially for large families.

### REQ-UX-003

Use progressive disclosure so routine family coordination remains simple while richer details appear only when the task requires them.

### REQ-UX-004

Safety experiences must use calm language. Confirmed facts, reports, assumptions, unknowns, and outdated information must be visually and textually distinguishable.

### REQ-UX-005

Stale information must not appear current. Time-sensitive information must expose enough freshness context for a user to understand whether it may be outdated.

### REQ-UX-006

The experience must support large-family information filtering and accessibility for older and less technical family members.

## Home

### REQ-UX-007

Home should answer, “What is happening with my family, and is there anything I need to do?” It must not be designed to answer, “Where is everybody right now?”

### REQ-UX-008

The recommended Home hierarchy is:

1. Needs My Attention
2. Upcoming Events
3. Current Trips / Plans
4. Relevant Family Updates
5. My Tasks
6. Active Safety Concern, when applicable
7. Active Sharing indicator for the current user

Attention items may include an RSVP, assigned task, expected check-in, event update, travel update, or safety concern.

## Sharing clarity

### REQ-UX-009

When sharing is active, the current user must be able to understand whether they are sharing, what they are sharing, who can see it, and when it will stop. Home must surface an active-sharing indicator when applicable.

## Mobile actions

### REQ-UX-010

Critical mobile actions should require minimal interaction, including check-in, RSVP, claiming a task, updating expected arrival, starting or stopping sharing, recording a contact attempt, adding a safety finding, and resolving a Concern.

## Navigation

### REQ-UX-011

The approved conceptual UX destinations are Home, Family, Events, Plans & Trips, Safety, and Me / Settings, matching the conceptual product areas in `product-contract.md#req-prod-007`. The approved MVP primary navigation for these destinations is defined by `REQ-UX-013`.

For the MVP application shell, automatically use the user's sole active Family membership as the current Family context. When the user has multiple active Family memberships, require an explicit selection in a clear shell-level Family selector; do not treat the first returned membership as the selected context. The only other way the current Family may change is the authorized item-link rule in `REQ-UX-014`. With no active Family membership, select no Family and use the approved invitation/operator-provisioned entry path. Retain the selection only for the current application/page session.

The selected Family is presentation/application context only. It grants no access and does not change membership, Event/Plan/Trip audiences, profile visibility, administrator authority, Sharing permissions, or Safety permissions. On a Family context change, clear prior Family-scoped client state and reload data authorized for the newly selected Family so prior Family directories, items, and forms are not visually retained. These rules do not change production authentication behavior.

`OD-001` is resolved by `DEC-074` through `REQ-UX-013`.

### REQ-UX-013

The MVP primary navigation has exactly six areas, in this order, at desktop and narrow widths: **Home**, **Family**, **Events**, **Plans & Trips**, **Safety**, and **Me / Settings**.

- **Plans & Trips** is a navigation grouping only. It gives clear access to Plans and to Trips. Plans and Trips remain separate product domains with their own routes, data, audiences, authorization, and lifecycle. The grouping must not merge their screens, APIs, records, or rules.
- **Safety** is a navigation grouping only. It gives clear access to Concerns and to Safety Incidents. Concerns and Incidents remain separate workflows with their own routes, access rules, lifecycle, and privacy (`REQ-SAFE-015`, `REQ-SAFE-016`, `REQ-PRIV-025`, `REQ-PRIV-026`). Reaching Safety grants no access to any Concern or Incident and must not imply that one exists.
- A grouping may use a simple landing page or an equivalent control that reaches both of its parts. Existing direct routes for Plans, Trips, Concerns, and Incidents remain available and refresh-safe.
- No other primary destination is added without a new decision. Messages, search, notifications, and location are not navigation areas.

For the MVP, the navigation model is adequately validated only when all of the following are true: owner visual review; rendered evidence at desktop and narrow widths; a keyboard and navigation accessibility review (logical order, visible focus, current destination announced); and automated end-to-end verification that every implemented primary area is reachable through main navigation at both widths. Automated accessibility scanning may supplement, but does not replace, the keyboard and manual review. A later formal usability study may revisit this model through a new decision.

### REQ-UX-014

Every Event, Plan, Trip, Concern, and Safety Incident that the application lists or shows on Home must be reachable through a refresh-safe item URL. A URL identifies an item; it never grants access. Server authorization of that specific item remains authoritative.

When a user opens an item URL:

1. The server first decides whether the current user may read that item, under the item's own access rules.
2. Only after the server authorizes the read may the application learn the item's Family. The application looks for that Family among the user's own active Family memberships (the same list used for Family selection under `REQ-UX-011`):
   - **Selectable Family.** If the item's Family is in that list and differs from the current Family (or no Family is selected), the application switches the current Family to the item's Family, clears and reloads Family-scoped state as `REQ-UX-011` requires, and shows the item. It may announce the change calmly with a polite status message, for example "Switched to {Family name}", using the name from the user's own Family list. There is no announcement when the Family does not change.
   - **List possibly stale.** If the item's Family is not in that list, the application refreshes the user's Family list once and checks again. It does not retry further.
   - **No selectable Family.** If the item's Family is still not in the list, the application never creates, guesses, or selects a Family context for it. It shows the authorized item without changing the current Family **only** where that domain's canonical access rules explicitly allow reading the item without active membership in its Family. Currently this is only the Trip creator's retained access after their Family membership ends (`privacy-safety-contract.md#req-priv-022`). For every other item, including Events, Plans, Concerns, and Incidents, the application shows the normal generic unavailable state of step 3 and keeps the current Family unchanged.
3. If the item does not exist or the user may not read it, the application shows the normal generic unavailable state, keeps the current Family unchanged, and reveals nothing about whether the item or its Family exists. An unavailable item never causes a Family switch or a switch announcement.

The application must never switch Family from the URL or client state alone, reveal a target Family before authorization, or treat current Family membership, or the client's Family list, as permission to read an item. The rule in step 2 for an item with no selectable Family adds no access: the server's item authorization stays the only authority. The privacy rule is `privacy-safety-contract.md#req-priv-027`.

## Visual direction

### REQ-UX-012

Initial visual prototypes are starting-point concepts, not pixel-perfect final designs. They establish a modern family-oriented web/mobile experience, clear dashboard hierarchy, visible Family Events, contextual trips and sharing, a structured but calm safety workspace, and responsive/mobile-friendly behavior. Layout may be refined while preserving this contract and the approved workflows.
