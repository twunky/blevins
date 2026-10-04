---
Status: ACCEPTED
Authority: Approved Phase 7.5 proposal; existing canonical requirements remain controlling
Context-Baseline: CTX-1.24
Version: 1.0
Decision: DEC-064
---

# PRP-023 — Application Shell & Returning-User Landing

## Objective

Make the existing application usable for a returning Family member through a persistent routed shell, clear current Family context, navigation to implemented areas, and a Home landing without repeating invitation/profile onboarding.

## Approved direction

- Resolve the current identity and active Family membership before landing the returning user on Home.
- Automatically select the sole active Family. When multiple active Families exist, require an explicit choice in a clear shell-level Family selector; do not silently use the first returned membership. With no active Family, select none and use the invitation/operator-provisioned entry path.
- Retain the selected Family for the current application/page session only. A Family switch clears Family-scoped client state and reloads authorized data for the new context. Selection grants no access and changes no membership, audiences, visibility, administrator authority, Sharing permission, or Safety permission.
- Use a persistent routed shell with Home, Family, Events, Plans & Trips, and Me / Settings conceptual destinations. Keep Plan and Trip workflows distinct routes even though they are grouped conceptually.
- Keep Family administration contextual and administrator-only, reusing existing controls.
- Keep the Development identity switcher visibly Development-only, preserve active identity labeling and state clearing, and never show it in production.
- Preserve the minimal WI-075-001 Home; full Home/dashboard remains a separate later Phase 7.5 slice.
- Preserve the open final mobile navigation decision in `OD-001`.

## Boundaries

This proposal authorizes no application implementation and creates no implementation story. It does not authorize domain behavior changes, production authentication changes, self-service Family creation/discovery/join, expanded Family administration, Event/Plan/Trip redesign, Safety, Sharing, Check-Ins, or notifications. Existing contracts continue to govern permissions and data access.

## Requirement impact

`DEC-064` clarifies the existing `REQ-UX-011` in `core/ux-contract.md`. No new requirement ID is needed. `CTX-1.24` records the approved context update.

## Recommended next step

Create a bounded implementation work item for the returning-user shell and landing. Reference `CTX-1.24`; include explicit multiple-Family selection and switching acceptance checks, existing-area route integration, no-Family behavior, and Development identity-switcher preservation. Obtain separate execution authorization before implementation.
