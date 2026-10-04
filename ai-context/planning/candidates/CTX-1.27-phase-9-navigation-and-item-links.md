---
Status: ACTIVATED — historical candidate record
Authority: Owner decisions D1–D4 on PRP-030 (resolution of OD-001 and the deep-link Family-context rule)
Context-Baseline: CTX-1.26
Version: 1.0-candidate
Decision: DEC-074
---

# Candidate CTX-1.27 — Phase 9 Navigation and Item Links

The project owner approved `PRP-030`, resolved `OD-001` with Option A, approved the navigation validation standard, approved `@axe-core/playwright` as a development-only test dependency, and set the deep-link Family-context rule on 2026-10-02. `DEC-074` records these decisions. `CTX-1.27` is active after passing `VER-CTX-1.27`; `CTX-1.26` remains an approved historical baseline.

## Why a new baseline is needed

`DEC-073` (Phase 9 entry and the Phase 9.5 roadmap addition) changed only phase status and sequence, so it needed no baseline. These decisions are different: they change normative canonical text.

- `REQ-UX-011` stated that the final grouping of navigation destinations "is not yet approved" and that `OD-001` was open. Resolving `OD-001` contradicts that text.
- `REQ-UX-011` allowed the selected Family to change only by explicit selection. The authorized deep-link switch is a second, new way, which the contract must state.
- Item URLs and their nondisclosure behavior constrain server and client behavior across five domains, which the privacy contract did not yet state for Events, Plans, and Trips.

## Canonical effect

- **`REQ-UX-011` amended:** conceptual destinations aligned with `REQ-PROD-007` (Home, Family, Events, Plans & Trips, Safety, Me / Settings); navigation now defined by `REQ-UX-013`; the authorized item-link rule in `REQ-UX-014` is the only other way the selected Family may change. The existing multi-Family, no-Family, session-only, and grants-no-access rules are unchanged.
- **`REQ-UX-013` added:** six-area primary navigation at desktop and narrow widths; "Plans & Trips" and "Safety" are navigation groupings only; direct routes stay refresh-safe; the four-part MVP validation standard.
- **`REQ-UX-014` added:** refresh-safe item URLs for Events, Plans, Trips, Concerns, and Incidents; server authorization first; Family switch only after authorization; generic unavailable state otherwise.
- **`REQ-PRIV-027` added:** item URLs, navigation, and the selected Family are not authorization; nothing about an item or its Family is disclosed before authorization; nonexistent and unauthorized items look the same.
- **`core/requirements-index.md`:** status `CTX-1.27`; the `REQ-UX-011` row amended; three rows added.
- **`decision-log.md`:** `OD-001` moved to resolved decisions.
- **Unchanged:** `REQ-PROD-007` already names the six conceptual areas. No Concern, Incident, Event, Plan, Trip, Family, membership, or administrator rule changes. `OD-002` stays open (Phase 9.5). The `@axe-core/playwright` approval is a tooling decision recorded in `DEC-074`, not a product requirement.
- No application code, test code, or dependency is changed by this baseline.
