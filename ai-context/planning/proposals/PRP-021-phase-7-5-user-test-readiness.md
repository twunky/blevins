---
Status: ACCEPTED
Authority: Approved roadmap-priority proposal; product requirements remain governed by existing canonical contracts
Context-Baseline: CTX-1.23
Version: 1.0
Decision: DEC-062
---

# PRP-021 — Phase 7.5 End-to-End UX & User-Test Readiness

## Metadata

- **Proposal ID:** `PRP-021`
- **Status:** `ACCEPTED`
- **Date:** `2026-09-27`
- **Requested by:** Project owner
- **Context baseline:** `CTX-1.22` at proposal approval; updated planning priority recorded in `CTX-1.23`
- **Affected authority:** Roadmap and project-status routing only; no product requirement changes
- **Decision:** `DEC-062`

## Request

Make end-to-end usability for early user testing the next implementation priority after Phase 7 is formally complete. Preserve completed phases and existing product/privacy contracts.

## Current evidence and context

- Phase 7 is current. `WI-007-001` through `WI-007-004` are complete. `WI-007-005` has passed final story review but remains unapproved and not execution-authorized; Phase 7 has not closed.
- `REQ-FAM-005/006` require invitation-only Family membership and controlled/operator provisioning of the first Family Group and first administrator. Public self-service Family creation is outside MVP.
- Production authentication-provider and account-registration architecture remain deferred.
- `REQ-UX-001/002/003/006/007/008/011/012` govern family-first presentation, attention-first Home, progressive disclosure, accessibility, conceptual destinations, and non-authoritative mockups.
- `REQ-PRIV-006` and other feature-specific contracts preserve administrator and audience privacy boundaries.

## Impact

Phase 7.5 is inserted after Phase 7 as a planned priority before Phase 8, without renumbering Phases 8–11. It coordinates the user experience around existing capabilities. It does not expand Safety behavior, change authentication architecture, create a Family self-service path, or grant administrators access to private information. Phase 9 remains planned for deeper integration and refinement after user feedback.

## Approved phase direction

### Objective

A new or returning user can enter the application, establish their approved Family context, understand their role, navigate the implemented product areas, perform ordinary family coordination workflows, and return to a useful Home experience without developer guidance.

### In-scope planning direction

- First-use and returning-user experience, including the distinction between Development identity and future real-user sign-in/sign-up expectations without selecting an authentication provider or architecture.
- Invitation-based entry to an already provisioned Family: accept invitation, establish/update profile, and understand Family context.
- Coherent navigation, page hierarchy, headings/actions, profile/account access, Family administration entry, return paths, and mobile-aware behavior; exact navigation structure remains a UX decision and does not resolve `OD-001`.
- Useful Home behavior for new users and Families with little or no data, preserving attention-first and family-first principles.
- Clear structural Family administration and useful empty states for Home, Family, Events, Plans, and Trips.
- Natural user journeys across implemented Family, Event, Plan, and Trip capabilities and one testable end-to-end scenario.
- Early user-test readiness, verified without developer explanation and without weakening privacy boundaries.

### Family provisioning and identity boundaries

- The first Family Group and first Family Administrator continue to use approved controlled/operator provisioning.
- Another adult may enter through an invitation, explicitly accept, establish/update a profile, and enter the Family context.
- Phase 7.5 may improve the visible journey but does not change provisioning authority.
- Public/self-service Family creation requires a separate product/privacy decision and canonical update.
- User-visible entry/onboarding may be defined, but no identity provider, authentication platform, or production account-registration architecture is selected.

### Administrator boundary

Administration may make approved structural responsibilities understandable, including membership and pending invitations. Administrator status remains structural and grants no automatic access to private profile information, Events, Trips, Sharing Sessions, location, Safety Circle data, or unrelated private data.

### Home and empty states

Home answers “What is happening, and what needs my attention?” It should offer understandable next actions rather than unexplained empty feature sections. It preserves attention before activity, family-first presentation, no surveillance-first design, contextual sharing, and progressive disclosure. No unfinished feature is invented to populate Home or an empty state.

### End-to-end scenario

Initial direction:

`Provisioned Family → invited user enters app → accepts invitation → profile established → Family context understood → Family area usable → Event / Plan / Trip created or viewed using implemented capabilities → collaboration where implemented → return to Home → Home reflects meaningful current information`

The exact scenario may be refined during Phase 7.5 planning.

### Exit criteria

- Coherent first-use and returning-user journeys exist.
- Family invitation/join path is understandable within the approved provisioning model.
- Home is useful with empty and populated data.
- Navigation and application shell are understandable.
- Family administration is understandable and respects privacy.
- Family, Event, Plan, and Trip areas connect naturally.
- Useful empty states and authorized next actions exist.
- At least one end-to-end user-test scenario passes.
- A user can complete the critical journey without developer explanation.
- Existing privacy, audience, administrator, and sharing boundaries remain intact.

## Phase relationships

- **Phase 7:** Remains current until formally closed under its existing completion and verification requirements. Phase 7.5 cannot begin before that closure.
- **Phase 8 — Safety Coordination:** Remains later with its existing scope. Phase 7.5 may prepare Home and the application shell for future approved Safety information but implements no new Safety capabilities.
- **Phase 9 — Integration & UX Refinement:** Remains planned for deeper integration, refinement informed by user feedback, accessibility refinement, performance, broader usability, and release-quality polish.
- Phases 10 and 11 remain present and retain their numbering and status.

## Mockups

UX mockups may guide visual direction, information hierarchy, workflow presentation, and page composition. They do not authorize product behavior, permissions, fields, navigation choices, or workflow states. Canonical context remains authoritative.

## Non-goals

- Self-service Family creation or changes to controlled first-Family provisioning.
- Production authentication architecture/provider selection or account-registration architecture.
- Expanded administrator permissions.
- New Safety, Check-In, Sharing Session, or location features.
- New Event, Plan, or Trip behavior added only for visual completeness.
- Resolving `OD-001` by implication.
- Complex administration, genealogy, minor/guardian behavior, chat/social features, or broad visual redesign unrelated to usability.
- Pixel-perfect mockup implementation.
- Closing Phase 7 or starting Phase 7.5 before Phase 7 closure.
- Creating or executing Phase 7.5 implementation stories as part of this decision.

## Requirement impact

No new requirement IDs are created. Existing UX, Family, privacy, Event, Plan, Trip, and Sharing requirements remain controlling. Any genuinely new product behavior discovered during Phase 7.5 planning follows `Proposal → Decision → Canonical Update → Story`.

## Verification plan

Verify phase order and status, Phase 7 closure gate, preservation of Phases 8–11, the Phase 8 and Phase 9 boundaries, Family provisioning and identity constraints, administrator privacy, Home UX principles, mockup authority, absence of new requirements, and absence of application-code or implementation-story changes. This is a context-only update; application tests are not required.

## Decision and authorization

- **Decision:** `ACCEPTED`
- **Decision ID:** `DEC-062`
- **Approved by:** Project owner, explicit pasted approval on 2026-09-27
- **Execution authorized:** Context update, verification, and conditional context-baseline activation only. No application implementation or Phase 7.5 implementation story is authorized.
