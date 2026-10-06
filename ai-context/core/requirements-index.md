# Requirements Index

**Status:** APPROVED - `CTX-1.39`
**Purpose:** Traceability index only; normative wording lives at each canonical source.

DEC-092 / REQ-PROD-014 records Phase 9.5 closure scope. Existing product-intent anchors remain approved; formal deferral does not mean implementation. Prior row baselines retain requirement provenance; CTX-1.39 is the active aggregate baseline under DEC-095 for bounded Plan fact correction, preserving CTX-1.38 Trip completion; CTX-1.37 remains the historical Phase 9.5 closure baseline.

Phase 10 trace: [WI-010-001](../planning/stories/WI-010-001-post-9-5-user-testability-baseline.md) / [verification](../planning/verification/VER-WI-010-001.md) evaluates REQ-PROD-014, REQ-UX-006/008/011/014, REQ-PLAN-002 and REQ-SHARE-010 without changing them. DEC-093 authorizes testing and bounded presentation fixes; no new requirement or CTX baseline.

Phase 10.5 Plan trace: [WI-105-002](../planning/stories/WI-105-002-bounded-creator-plan-activity-timing-correction.md) implements REQ-PLAN-008 while preserving REQ-PLAN-005/007 and REQ-PRIV-011/020/027. DEC-095 records the owner-approved boundary.

## ID and lifecycle rules

- Format: `REQ-<NAMESPACE>-###`.
- IDs are immutable, never renumbered, and never reused.
- Meaning changes receive a new ID; superseded IDs link to their replacement.
- Editorial clarification may retain an ID only when meaning and verification expectations remain unchanged.
- Stories cite applicable IDs; verification maps each cited ID to evidence and a result.

Statuses are `PROPOSED`, `APPROVED`, `SUPERSEDED`, or `RETIRED`. Product/UX extraction requirements are approved under the baseline shown in each row. Existing AI operating and verification requirements remain approved independently of those baselines.

DEC-097's future-target requirements use status `APPROVED` with a separate **Phase 11 / UNIMPLEMENTED** target column, not an active CTX assignment. Current MVP IDs remain approved for their original scope; future replacements have new IDs and do not globally supersede MVP truth. See the [future-target trace](#future-phase-11-production-entry-dec-097).

## Namespaces

| Namespace | Area |
|---|---|
| `REQ-PROD-###` | Product |
| `REQ-UX-###` | UX |
| `REQ-PRIV-###` | Privacy, consent, and access |
| `REQ-FAM-###` | Family and profiles |
| `REQ-EVT-###` | Events |
| `REQ-PLAN-###` | Lightweight plans |
| `REQ-TRIP-###` | Trips |
| `REQ-SHARE-###` | Sharing and check-ins |
| `REQ-SAFE-###` | Concerns and safety incidents |
| `REQ-NOTIFY-###` | Notifications |
| `REQ-AI-###` | AI operating rules |
| `REQ-VERIFY-###` | Verification |

Do not use `REQ-SAFETY-*`.

## Product

| ID | Short label | Canonical location | Status | Introduced | Superseded by |
|---|---|---|---|---|---|
| `REQ-PROD-001` | Family coordination identity | `product-contract.md#req-prod-001` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PROD-002` | Connect / Coordinate / Care | `product-contract.md#req-prod-002` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PROD-003` | Unreachable-person coordination purpose | `product-contract.md#req-prod-003` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PROD-004` | Distinguish information certainty | `product-contract.md#req-prod-004` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PROD-005` | Large/distributed family usefulness | `product-contract.md#req-prod-005` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PROD-006` | Everyday usefulness supports safety | `product-contract.md#req-prod-006` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PROD-007` | Conceptual product areas | `product-contract.md#req-prod-007` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PROD-008` | Consolidated MVP capability boundary | `product-contract.md#req-prod-008` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PROD-009` | Explicit product non-goals | `product-contract.md#req-prod-009` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PROD-010` | Events are core | `product-contract.md#req-prod-010` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PROD-011` | Plans and Trips distinction | `product-contract.md#req-prod-011` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PROD-012` | Concern and Incident distinction; manual Start an Incident shortcut | `product-contract.md#req-prod-012` | `APPROVED` | `CTX-1.25`; amended `CTX-1.26` | — |
| `REQ-PROD-013` | Minor and guardian functionality outside MVP | `product-contract.md#req-prod-013` | `APPROVED` | `CTX-1.14` | — |
| `REQ-PROD-014` | Phase 9.5 implemented minimum and formally deferred future scope | `product-contract.md#req-prod-014` | `APPROVED` | `CTX-1.37` | DEC-092; WI-095-001–010 complete; no WI-095-011 required |

## UX

| ID | Short label | Canonical location | Status | Introduced | Superseded by |
|---|---|---|---|---|---|
| `REQ-UX-001` | Family-first, contextual location | `ux-contract.md#req-ux-001` | `APPROVED` | `CTX-1.0` | — |
| `REQ-UX-002` | Attention before activity | `ux-contract.md#req-ux-002` | `APPROVED` | `CTX-1.0` | — |
| `REQ-UX-003` | Progressive disclosure | `ux-contract.md#req-ux-003` | `APPROVED` | `CTX-1.0` | — |
| `REQ-UX-004` | Calm language and visible uncertainty | `ux-contract.md#req-ux-004` | `APPROVED` | `CTX-1.0` | — |
| `REQ-UX-005` | Information freshness | `ux-contract.md#req-ux-005` | `APPROVED` | `CTX-1.0` | — |
| `REQ-UX-006` | Filtering and accessibility | `ux-contract.md#req-ux-006` | `APPROVED` | `CTX-1.0` | — |
| `REQ-UX-007` | Home purpose | `ux-contract.md#req-ux-007` | `APPROVED` | `CTX-1.0` | — |
| `REQ-UX-008` | Home hierarchy | `ux-contract.md#req-ux-008` | `APPROVED` | `CTX-1.0` | — |
| `REQ-UX-009` | Active sharing clarity | `ux-contract.md#req-ux-009` | `APPROVED` | `CTX-1.0` | — |
| `REQ-UX-010` | Minimal-interaction mobile actions | `ux-contract.md#req-ux-010` | `APPROVED` | `CTX-1.0` | — |
| `REQ-UX-011` | Conceptual navigation areas and current Family context | `ux-contract.md#req-ux-011` | `APPROVED` | `CTX-1.0`; clarified `CTX-1.24`; amended `CTX-1.27` | — |
| `REQ-UX-012` | Visual direction | `ux-contract.md#req-ux-012` | `APPROVED` | `CTX-1.0` | — |
| `REQ-UX-013` | MVP six-area primary navigation and its validation standard (resolves `OD-001`) | `ux-contract.md#req-ux-013` | `APPROVED` | `CTX-1.27` | — |
| `REQ-UX-014` | Refresh-safe item URLs and authorized deep-link Family context; no fabricated Family context; one-time Family-list refresh | `ux-contract.md#req-ux-014` | `APPROVED` | `CTX-1.27`; amended `CTX-1.28` | — |

## Privacy, consent, and access

| ID | Short label | Canonical location | Status | Introduced | Superseded by |
|---|---|---|---|---|---|
| `REQ-PRIV-001` | Voluntary sharing and no-location usefulness | `privacy-safety-contract.md#req-priv-001` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PRIV-002` | User control of sharing dimensions | `privacy-safety-contract.md#req-priv-002` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PRIV-003` | Temporary access, expiration, and revocation | `privacy-safety-contract.md#req-priv-003` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PRIV-004` | Participation does not enable sharing | `privacy-safety-contract.md#req-priv-004` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PRIV-005` | Relationship does not imply permission | `privacy-safety-contract.md#req-priv-005` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PRIV-006` | Administration does not grant private access | `privacy-safety-contract.md#req-priv-006` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PRIV-007` | Individual selects Safety Circle | `privacy-safety-contract.md#req-priv-007` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PRIV-008` | Concern/Incident does not bypass privacy | `privacy-safety-contract.md#req-priv-008` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PRIV-009` | Stale and retained sensitive information | `privacy-safety-contract.md#req-priv-009` | `APPROVED` | `CTX-1.0`; amended `CTX-1.26` | — |
| `REQ-PRIV-010` | Anti-surveillance boundary | `privacy-safety-contract.md#req-priv-010` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PRIV-011` | Account and invitation access boundary | `privacy-safety-contract.md#req-priv-011` | `APPROVED` | `CTX-1.2` | — |
| `REQ-PRIV-012` | Operator bootstrap access boundary | `privacy-safety-contract.md#req-priv-012` | `APPROVED` | `CTX-1.3` | — |
| `REQ-PRIV-013` | Adult profile visibility and access contract | `privacy-safety-contract.md#req-priv-013` | `APPROVED` | `CTX-1.4` | — |
| `REQ-PRIV-014` | Adult membership invitation privacy contract | `privacy-safety-contract.md#req-priv-014` | `APPROVED` | `CTX-1.5` | — |
| `REQ-PRIV-015` | Adult Family Relationship consent and privacy | `privacy-safety-contract.md#req-priv-015` | `APPROVED` | `CTX-1.6` | — |
| `REQ-PRIV-016` | Shared Family Date visibility and access | `privacy-safety-contract.md#req-priv-016` | `APPROVED` | `CTX-1.7` | — |
| `REQ-PRIV-017` | Shared Family Date retention boundary | `privacy-safety-contract.md#req-priv-017` | `APPROVED` | `CTX-1.8` | — |
| `REQ-PRIV-018` | Shared Family Date 30-day retention period | `privacy-safety-contract.md#req-priv-018` | `APPROVED` | `CTX-1.9` | — |
| `REQ-PRIV-019` | Trusted Contacts private by default and explicit designation | `privacy-safety-contract.md#req-priv-019` | `APPROVED` | `CTX-1.11` | — |
| `REQ-PRIV-020` | Explicit audience choice for Events, Plans, and Trips | `privacy-safety-contract.md#req-priv-020` | `APPROVED` | `CTX-1.12` | — |
| `REQ-PRIV-021` | Event view and edit access | `privacy-safety-contract.md#req-priv-021` | `APPROVED` | `CTX-1.13` | — |
| `REQ-PRIV-022` | Trip audience privacy, lifecycle, and creator-controlled whole-audience replacement | `privacy-safety-contract.md#req-priv-022` | `APPROVED` | `CTX-1.18` | Clarified by `DEC-061` in `CTX-1.22` |
| `REQ-PRIV-023` | Event participation and private-data separation | `privacy-safety-contract.md#req-priv-023` | `APPROVED` | `CTX-1.19` | — |
| `REQ-PRIV-024` | Trusted Contact designation grants no unrelated access | `privacy-safety-contract.md#req-priv-024` | `APPROVED` | `CTX-1.20` | — |
| `REQ-PRIV-025` | Concern access, retention, audit, and backup privacy | `privacy-safety-contract.md#req-priv-025` | `APPROVED` | `CTX-1.25` | — |
| `REQ-PRIV-026` | Safety Incident access, lineage, retention, audit, and backup privacy | `privacy-safety-contract.md#req-priv-026` | `APPROVED` | `CTX-1.26` | — |
| `REQ-PRIV-027` | Item URLs, navigation, and selected Family are not authorization; nondisclosing unavailable result | `privacy-safety-contract.md#req-priv-027` | `APPROVED` | `CTX-1.27` | — |
| `REQ-PRIV-028` | Event invitation eligibility, same-profile restoration, and named RSVP privacy | `privacy-safety-contract.md#req-priv-028` | `APPROVED` | `CTX-1.30` | — |
| `REQ-PRIV-029` | Actor-aware Event Need responsibility identity disclosure | `privacy-safety-contract.md#req-priv-029` | `APPROVED` | `CTX-1.31` | — |

## Family and profiles

| ID | Short label | Canonical location | Status | Introduced | Superseded by |
|---|---|---|---|---|---|
| `REQ-FAM-001` | MVP Family capabilities | `product-contract.md#req-fam-001` | `APPROVED` | `CTX-1.0` | — |
| `REQ-FAM-002` | Shared dates and trusted contacts | `product-contract.md#req-fam-002` | `APPROVED` | `CTX-1.0` | — |
| `REQ-FAM-003` | Family MVP exclusions | `product-contract.md#req-fam-003` | `APPROVED` | `CTX-1.0` | — |
| `REQ-FAM-004` | Configurable Family Group capacity | `product-contract.md#req-fam-004` | `APPROVED` | `CTX-1.1` | — |
| `REQ-FAM-005` | Private invitation-only membership | `product-contract.md#req-fam-005` | `APPROVED` | `CTX-1.2` | — |
| `REQ-FAM-006` | Controlled first Family Group bootstrap | `product-contract.md#req-fam-006` | `APPROVED` | `CTX-1.3` | — |
| `REQ-FAM-007` | Minimal adult self-profile field contract | `product-contract.md#req-fam-007` | `APPROVED` | `CTX-1.4` | — |
| `REQ-FAM-008` | Adult membership invitation lifecycle | `product-contract.md#req-fam-008` | `APPROVED` | `CTX-1.5` | — |
| `REQ-FAM-009` | Adult Family Relationship lifecycle | `product-contract.md#req-fam-009` | `APPROVED` | `CTX-1.6` | — |
| `REQ-FAM-010` | Shared Family Dates | `product-contract.md#req-fam-010` | `APPROVED` | `CTX-1.7` | — |
| `REQ-FAM-011` | Shared Family Date title, description, and date fields | `product-contract.md#req-fam-011` | `APPROVED` | `CTX-1.9` | — |
| `REQ-FAM-012` | Shared Family Date single-date, non-recurring v1 behavior | `product-contract.md#req-fam-012` | `APPROVED` | `CTX-1.10` | — |
| `REQ-FAM-013` | Shared Family Date description is required | `product-contract.md#req-fam-013` | `APPROVED` | `CTX-1.10` | — |
| `REQ-FAM-014` | Adult-owned Trusted Contacts private by default | `product-contract.md#req-fam-014` | `APPROVED` | `CTX-1.11` | — |
| `REQ-FAM-015` | Nonmember Trusted Contact eligibility | `product-contract.md#req-fam-015` | `APPROVED` | `CTX-1.20` | — |

## Events

| ID | Short label | Canonical location | Status | Introduced | Superseded by |
|---|---|---|---|---|---|
| `REQ-EVT-001` | Event details, invitations, and RSVP | `../workflows/events.md#req-evt-001` | `APPROVED` | `CTX-1.0` | — |
| `REQ-EVT-002` | Meaningful event updates | `../workflows/events.md#req-evt-002` | `APPROVED` | `CTX-1.0` | — |
| `REQ-EVT-003` | Tasks, needs, and claiming | `../workflows/events.md#req-evt-003` | `APPROVED` | `CTX-1.0` | — |
| `REQ-EVT-004` | Unified Needs concept | `../workflows/events.md#req-evt-004` | `APPROVED` | `CTX-1.0` | — |
| `REQ-EVT-005` | Event workflow | `../workflows/events.md#req-evt-005` | `APPROVED` | `CTX-1.0` | — |
| `REQ-EVT-006` | Event MVP exclusions | `../workflows/events.md#req-evt-006` | `APPROVED` | `CTX-1.0` | — |
| `REQ-EVT-007` | Optional event Sharing Session | `../workflows/events.md#req-evt-007` | `APPROVED` | `CTX-1.0` | — |
| `REQ-EVT-008` | Event detail editing access | `../workflows/events.md#req-evt-008` | `APPROVED` | `CTX-1.13` | — |
| `REQ-EVT-009` | Event organizer boundary | `../workflows/events.md#req-evt-009` | `APPROVED` | `CTX-1.19` | — |
| `REQ-EVT-010` | Event invitation and RSVP lifecycle | `../workflows/events.md#req-evt-010` | `APPROVED` | `CTX-1.19` | — |
| `REQ-EVT-011` | Unified Event Needs and eligibility lifecycle | `../workflows/events.md#req-evt-011` | `APPROVED` | `CTX-1.19` | — |
| `REQ-EVT-012` | Meaningful Event Updates and retained history | `../workflows/events.md#req-evt-012` | `APPROVED` | `CTX-1.19` | — |
| `REQ-EVT-013` | Event Check-In, Home, and notification boundaries | `../workflows/events.md#req-evt-013` | `APPROVED` | `CTX-1.19` | DEC-090 / WI-095-009: live Home own-Pending RSVP attention; DEC-092 / REQ-PROD-014: phase minimum satisfied, broader capability formally deferred |
| `REQ-EVT-014` | First in-app invitation and self-RSVP lifecycle and discovery | `../workflows/events.md#req-evt-014` | `APPROVED` | `CTX-1.30` | — |

## Lightweight plans

| ID | Short label | Canonical location | Status | Introduced | Superseded by |
|---|---|---|---|---|---|
| `REQ-PLAN-001` | Lightweight Plan definition | `../workflows/plans-trips.md#req-plan-001` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PLAN-002` | Plan information | `../workflows/plans-trips.md#req-plan-002` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PLAN-003` | Plan workflow | `../workflows/plans-trips.md#req-plan-003` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PLAN-004` | Plan update, check-in, and completion | `../workflows/plans-trips.md#req-plan-004` | `APPROVED` | `CTX-1.0` | — |
| `REQ-PLAN-005` | Plan audience choices, current-group access, and creator-departure retention | `../workflows/plans-trips.md#req-plan-005` | `APPROVED` | `CTX-1.16` | — |
| `REQ-PLAN-006` | Optional Plan notes at creation | `../workflows/plans-trips.md#req-plan-006` | `APPROVED` | `CTX-1.17` | — |
| `REQ-PLAN-007` | Creator edit or clear of existing optional Plan note | `../workflows/plans-trips.md#req-plan-007` | `APPROVED` | `CTX-1.29` | — |
| `REQ-PLAN-008` | Bounded original-creator activity/timing correction with current stored-Family access | `../workflows/plans-trips.md#req-plan-008` | `APPROVED` | `CTX-1.39` | — |

## Trips

| ID | Short label | Canonical location | Status | Introduced | Superseded by |
|---|---|---|---|---|---|
| `REQ-TRIP-001` | Structured Trip definition | `../workflows/plans-trips.md#req-trip-001` | `APPROVED` | `CTX-1.0` | — |
| `REQ-TRIP-002` | Trip information | `../workflows/plans-trips.md#req-trip-002` | `APPROVED` | `CTX-1.0` | — |
| `REQ-TRIP-003` | MVP Trip capabilities | `../workflows/plans-trips.md#req-trip-003` | `APPROVED` | `CTX-1.0` | — |
| `REQ-TRIP-004` | Trip workflow | `../workflows/plans-trips.md#req-trip-004` | `APPROVED` | `CTX-1.0` | DEC-094 / CTX-1.38: explicit completion independent of arrival/Sharing |
| `REQ-TRIP-005` | Trip updates, arrival, and sharing expiration | `../workflows/plans-trips.md#req-trip-005` | `APPROVED` | `CTX-1.0` | DEC-094 / CTX-1.38: one-way status-only creator completion |
| `REQ-TRIP-006` | Trip MVP exclusions | `../workflows/plans-trips.md#req-trip-006` | `APPROVED` | `CTX-1.0` | — |
| `REQ-TRIP-007` | Trip roles and edit permissions | `../workflows/plans-trips.md#req-trip-007` | `APPROVED` | `CTX-1.18` | DEC-094 / CTX-1.38: one-way status-only creator completion |
| `REQ-TRIP-008` | Required and optional Trip fields | `../workflows/plans-trips.md#req-trip-008` | `APPROVED` | `CTX-1.18` | — |
| `REQ-TRIP-009` | Informational non-system Trip contacts | `../workflows/plans-trips.md#req-trip-009` | `APPROVED` | `CTX-1.18` | — |
| `REQ-TRIP-010` | Collaborative Trip Notes, creator access, and retained states | `../workflows/plans-trips.md#req-trip-010` | `APPROVED` | `CTX-1.18` | — |
| `REQ-TRIP-011` | Expected arrival, Check-Ins, and completion | `../workflows/plans-trips.md#req-trip-011` | `APPROVED` | `CTX-1.18` | DEC-094 / CTX-1.38: one-way status-only creator completion |
| `REQ-TRIP-012` | MVP Trip creation flow and UX boundary | `../workflows/plans-trips.md#req-trip-012` | `APPROVED` | `CTX-1.18` | DEC-094 / CTX-1.38: one-way status-only creator completion |

## Sharing and check-ins

| ID | Short label | Canonical location | Status | Introduced | Superseded by |
|---|---|---|---|---|---|
| `REQ-SHARE-001` | Sharing Session definition | `../workflows/sharing-checkins.md#req-share-001` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SHARE-002` | Sharing modes | `../workflows/sharing-checkins.md#req-share-002` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SHARE-003` | What/who/why/duration dimensions | `../workflows/sharing-checkins.md#req-share-003` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SHARE-004` | Context does not authorize sharing | `../workflows/sharing-checkins.md#req-share-004` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SHARE-005` | Sharing clarity and stop control | `../workflows/sharing-checkins.md#req-share-005` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SHARE-006` | Fast Check-In statuses | `../workflows/sharing-checkins.md#req-share-006` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SHARE-007` | Optional Check-In information | `../workflows/sharing-checkins.md#req-share-007` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SHARE-008` | Missed expected Check-In flow | `../workflows/sharing-checkins.md#req-share-008` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SHARE-009` | Trip and Sharing Session access separation | `../workflows/sharing-checkins.md#req-share-009` | `APPROVED` | `CTX-1.18` | — |

## Concerns and safety incidents

| ID | Short label | Canonical location | Status | Introduced | Superseded by |
|---|---|---|---|---|---|
| `REQ-SAFE-001` | Missed expectation is uncertainty | `privacy-safety-contract.md#req-safe-001` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SAFE-002` | Confidence categories | `privacy-safety-contract.md#req-safe-002` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SAFE-003` | Safety information provenance | `privacy-safety-contract.md#req-safe-003` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SAFE-004` | Unknowns and correction history | `privacy-safety-contract.md#req-safe-004` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SAFE-005` | Concern definition | `../workflows/safety.md#req-safe-005` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SAFE-006` | Concern workflow | `../workflows/safety.md#req-safe-006` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SAFE-007` | Safety Incident workspace; no unlock of other-domain fields | `../workflows/safety.md#req-safe-007` | `APPROVED` | `CTX-1.0`; amended `CTX-1.26` | — |
| `REQ-SAFE-008` | Incident coordination states; MVP lifecycle Open/Frozen/Resolved, other labels deferred | `../workflows/safety.md#req-safe-008` | `APPROVED` | `CTX-1.0`; amended `CTX-1.26` | — |
| `REQ-SAFE-009` | Coordinator and participants | `../workflows/safety.md#req-safe-009` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SAFE-010` | Findings use confidence rules | `../workflows/safety.md#req-safe-010` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SAFE-011` | Contact attempts | `../workflows/safety.md#req-safe-011` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SAFE-012` | Incident tasks | `../workflows/safety.md#req-safe-012` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SAFE-013` | Incident timeline | `../workflows/safety.md#req-safe-013` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SAFE-014` | Incident resolution | `../workflows/safety.md#req-safe-014` | `APPROVED` | `CTX-1.0` | — |
| `REQ-SAFE-015` | Concern lifecycle and inactivity freeze | `../workflows/safety.md#req-safe-015` | `APPROVED` | `CTX-1.25`; amended `CTX-1.26` | — |
| `REQ-SAFE-016` | MVP Safety Incident workflow | `../workflows/safety.md#req-safe-016` | `APPROVED` | `CTX-1.26` | — |

## Notifications

| ID | Short label | Canonical location | Status | Introduced | Superseded by |
|---|---|---|---|---|---|
| `REQ-NOTIFY-001` | Actionable and restrained notifications | `privacy-safety-contract.md#req-notify-001` | `APPROVED` | `CTX-1.0` | — |
| `REQ-NOTIFY-002` | Gradual notification escalation | `privacy-safety-contract.md#req-notify-002` | `APPROVED` | `CTX-1.0` | — |
| `REQ-NOTIFY-003` | Notification purposes | `privacy-safety-contract.md#req-notify-003` | `APPROVED` | `CTX-1.0` | — |

## AI operating rules

| ID | Short label | Canonical location | Status | Introduced | Superseded by |
|---|---|---|---|---|---|
| `REQ-AI-001` | Mandatory entry sequence | `../README.md#mandatory-entry-sequence` | `APPROVED` | Phase 2B pre-baseline | — |
| `REQ-AI-002` | Selective context loading | `../README.md#context-loading` | `APPROVED` | Phase 2B pre-baseline | — |
| `REQ-AI-003` | No unsupported inference | `../README.md#mandatory-ai-rules` | `APPROVED` | Phase 2B pre-baseline | — |
| `REQ-AI-004` | Authoritative conflict blocks work | `../README.md#authority-model` | `APPROVED` | Phase 2B pre-baseline | — |
| `REQ-AI-005` | Proposal and Execution separation | `../README.md#work-modes` | `APPROVED` | Phase 2B pre-baseline | — |
| `REQ-AI-006` | Bootstrap/archive excluded from routing | `../README.md#supersession-and-archive` | `APPROVED` | Phase 2B pre-baseline | — |
| `REQ-AI-007` | Least expensive capable model tier | `../routing/agent-model-routing.md#model-routing` | `APPROVED` | Phase 2B pre-baseline | — |
| `REQ-AI-008` | Technical architecture proposal boundary | `../README.md#architecture-boundary` | `APPROVED` | Phase 2B pre-baseline | — |
| `REQ-AI-009` | Best-practices and human-readable work standard | `../README.md#mandatory-ai-rules` | `APPROVED` | `DEC-023` | — |
| `REQ-AI-010` | Required context manifest discipline | `../README.md#context-loading` | `APPROVED` | `DEC-024` | — |
| `REQ-AI-011` | Lightweight personal-project work items | `../README.md#work-hierarchy-and-lifecycle` | `APPROVED` | `DEC-025` | — |
| `REQ-AI-012` | Bounded standing auto-approval | `../README.md#standing-bounded-auto-approval` | `APPROVED` | `DEC-032` | — |

## Verification

| ID | Short label | Canonical location | Status | Introduced | Superseded by |
|---|---|---|---|---|---|
| `REQ-VERIFY-001` | Story acceptance criteria | `../README.md#verification` | `APPROVED` | Phase 2B pre-baseline | — |
| `REQ-VERIFY-002` | Completion evidence | `../README.md#verification` | `APPROVED` | Phase 2B pre-baseline | — |
| `REQ-VERIFY-003` | Verification independence disclosure | `../README.md#verification` | `APPROVED` | Phase 2B pre-baseline | — |
| `REQ-VERIFY-004` | Project verification does not replace story verification | `../README.md#verification` | `APPROVED` | Phase 2B pre-baseline | — |



## Standalone Check-In first slice (CTX-1.32)

| ID | Short label | Canonical location | Status | Introduced | Superseded by |
|---|---|---|---|---|---|
| REQ-PRIV-030 | Explicit Check-In grants and membership loss | privacy-safety-contract.md#req-priv-030 | APPROVED | CTX-1.32 | — |
| REQ-SHARE-010 | Immutable standalone Check-In creation/history | ../workflows/sharing-checkins.md#req-share-010 | APPROVED | CTX-1.32 | — |

## Temporary Sharing Session first slice (CTX-1.33)

| Requirement | Summary | Canonical source | Status | Baseline | Supersession |
|---|---|---|---|---|---|
| REQ-PRIV-031 | Temporary own-location grants, expiration and latest-point privacy | privacy-safety-contract.md#req-priv-031 | APPROVED | CTX-1.33 | — |
| REQ-SHARE-011 | Bounded browser sharing workflow and Home clarity | ../workflows/sharing-checkins.md#req-share-011 | APPROVED | CTX-1.33 | — |

## Manual Trusted Contacts first slice (CTX-1.34)

| ID | Requirement | Canonical source | Status | Baseline | Supersedes |
|---|---|---|---|---|---|
| REQ-FAM-016 | Private manual contacts, fields, duplicates and personal lifecycle | product-contract.md#req-fam-016 | APPROVED | CTX-1.34 | — |
| REQ-PRIV-032 | Owner-only manual-contact privacy and zero downstream grants | privacy-safety-contract.md#req-priv-032 | APPROVED | CTX-1.34 | — |

DEC-088 clarifies REQ-FAM-014/015 and REQ-PRIV-019/024 for this first slice; broader future uses remain gated.

## Safety Circle first slice

| ID | Short label | Canonical location | Status | Introduced | Superseded by |
|---|---|---|---|---|---|
| REQ-FAM-017 | Owner Safety Circle CRUD | product-contract.md#req-fam-017 | APPROVED | CTX-1.35 | - |
| REQ-PRIV-033 | Circle privacy, provenance, deletion and target grants | privacy-safety-contract.md#req-priv-033 | APPROVED | CTX-1.35 | - |

## Household organization (CTX-1.36)

| ID | Short label | Canonical location | Status | Introduced | Superseded by |
|---|---|---|---|---|---|
| REQ-FAM-018 | Household organization, adult eligibility and lifecycle | product-contract.md#req-fam-018 | APPROVED | CTX-1.36 | - |
| REQ-PRIV-034 | Household current Family visibility/admin checks and zero protected authority | privacy-safety-contract.md#req-priv-034 | APPROVED | CTX-1.36 | - |

## Future Phase 11 production entry (DEC-097)

All rows are approved future product requirements, **UNIMPLEMENTED**, outside the active CTX-1.39 MVP baseline. Authority: [DEC-097](../decision-log.md#dec-097--approve-future-phase-11-production-entry-model). Canonical wording: [production-entry contract](production-entry-contract.md). No implementation WI or execution authorization.

| ID | Requirement | Canonical source | Status | Target / implementation | Current scope reconciled |
|---|---|---|---|---|---|
| REQ-PROD-015 | Open adult Account signup, zero-Family state, no implicit access | [production-entry-contract.md#req-prod-015](production-entry-contract.md#req-prod-015) | APPROVED | Phase 11 / UNIMPLEMENTED | Preserves REQ-PRIV-011 access boundary |
| REQ-FAM-019 | Self-service atomic Family bootstrap and private existing-Family joining | [production-entry-contract.md#req-fam-019](production-entry-contract.md#req-fam-019) | APPROVED | Phase 11 / UNIMPLEMENTED | Future replacement for REQ-FAM-005/006 bootstrap paths; capacity unchanged |
| REQ-FAM-020 | Adult-member invitation authority and bounded management | [production-entry-contract.md#req-fam-020](production-entry-contract.md#req-fam-020) | APPROVED | Phase 11 / UNIMPLEMENTED | Future replacement for REQ-FAM-005/008 admin-only authority |
| REQ-FAM-021 | Invitation lifecycle, permanent authority-loss invalidation and delivery | [production-entry-contract.md#req-fam-021](production-entry-contract.md#req-fam-021) | APPROVED | Phase 11 / UNIMPLEMENTED | REQ-FAM-008 lifecycle retained and extended for production |
| REQ-PRIV-035 | Explicit authenticated acceptance and matching verified-email control | [production-entry-contract.md#req-priv-035](production-entry-contract.md#req-priv-035) | APPROVED | Phase 11 / UNIMPLEMENTED | REQ-PRIV-014 / REQ-FAM-008 recipient boundary strengthened |
| REQ-FAM-022 | Shared Account/Profile, separate memberships, fresh-invitation return | [production-entry-contract.md#req-fam-022](production-entry-contract.md#req-fam-022) | APPROVED | Phase 11 / UNIMPLEMENTED | Preserves resource-specific grant/own-access rules |
| REQ-UX-015 | No-Family first use and Trusted Contact separation | [production-entry-contract.md#req-ux-015](production-entry-contract.md#req-ux-015) | APPROVED | Phase 11 / UNIMPLEMENTED | Future replacement for REQ-UX-011 entry guidance only |
| REQ-PRIV-036 | Per-Family optional-profile privacy, private on new membership | [production-entry-contract.md#req-priv-036](production-entry-contract.md#req-priv-036) | APPROVED | Phase 11 / UNIMPLEMENTED | Future replacement for REQ-PRIV-013 broad sharing |
| REQ-FAM-023 | Multiple administrators and bounded continuity/recovery | [production-entry-contract.md#req-fam-023](production-entry-contract.md#req-fam-023) | APPROVED | Phase 11 / UNIMPLEMENTED | Preserves REQ-PRIV-006/012 no protected-content override |
| REQ-PROD-016 | Provider-neutral identity mapping, capabilities, domain boundary and MFA | [production-entry-contract.md#req-prod-016](production-entry-contract.md#req-prod-016) | APPROVED | Phase 11 / UNIMPLEMENTED | Provider selection remains open; no MVP auth change |
| REQ-PRIV-037 | Bounded invitation retention with exact periods open | [production-entry-contract.md#req-priv-037](production-entry-contract.md#req-priv-037) | APPROVED | Phase 11 / UNIMPLEMENTED | OD-006; no 30-day / 90-day policy approved |
| REQ-UX-016 | Later QR convenience uses the same secure invitation semantics | [production-entry-contract.md#req-ux-016](production-entry-contract.md#req-ux-016) | APPROVED | Phase 11 later convenience / UNIMPLEMENTED | No initial general/multi-use links, SMS or short-code authorization |
