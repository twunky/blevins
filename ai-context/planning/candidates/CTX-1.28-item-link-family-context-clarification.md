---
Status: ACTIVATED — historical candidate record
Authority: Owner decisions OD-A and OD-B on WI-009-005 (2026-10-02)
Context-Baseline: CTX-1.27
Version: 1.0-candidate
Decision: DEC-075
---

# Candidate CTX-1.28 — Item-Link Family Context Clarification

On 2026-10-02 the project owner resolved `OD-A` to `OD-I` for `WI-009-005 — Refresh-Safe Item URLs and Authorize-First Deep Links`. `DEC-075` records these decisions. Two of them, `OD-A` and `OD-B`, change normative canonical text. `CTX-1.28` is active after passing `VER-CTX-1.28`. `CTX-1.27` remains an approved historical baseline.

## Why a new baseline is needed

The baseline is not created because `WI-009-005` exists. It is needed because the owner's decisions conflict with the active wording of `REQ-UX-014` step 2. That step says that after authorization, if the item's Family "differs from the current Family (or no Family is selected), the application switches the current Family to the item's Family". The step has no exception.

- **`OD-A`:** the Trip creator keeps Trip access after their Family membership ends (`REQ-PRIV-022`). An authorized Trip can therefore belong to a Family the user cannot select under `REQ-UX-011`. The owner decided that the application must not switch, and must not create a Family context, in that case. That contradicts the unconditional "switches".
- **`OD-B`:** the owner set a fail-safe rule for an authorized item whose Family is missing from the client's Family list. The client refreshes the list once; if the Family is still absent, it shows the generic unavailable state for non-Trip items. This is a new normative UX rule that `REQ-UX-014` did not state.

The remaining decisions (`OD-C` to `OD-I`) are implementation, scope, test-fixture, or presentation choices within the existing requirements. They need no canonical change. `OD-H` (the announcement) is already allowed by `REQ-UX-014` ("may announce"); the amendment only states the polite status pattern and that there is no announcement without a Family change.

## Canonical effect

- **`REQ-UX-014` amended in place.** Following the `DEC-064` and `DEC-074` precedent, it is a clarification with the same subject, so the ID is not replaced. Step 2 now:
  - matches the item's Family against the user's own active Family list;
  - switches only to a selectable Family;
  - refreshes the list once if the Family is absent;
  - never creates or guesses a Family context;
  - renders without a Family change only where the domain's canonical access rules explicitly allow access without active membership. Currently that is only Trip creator retention under `REQ-PRIV-022`. Every other item shows the generic unavailable state.

  Step 3 states that an unavailable item never causes a switch or an announcement. The final paragraph adds that the client's Family list is not permission and that the no-selectable-Family rule adds no access.
- **`core/requirements-index.md`:** status `CTX-1.28`; the `REQ-UX-014` row is marked amended under `CTX-1.28`.
- **`core/ux-contract.md` header:** `CTX-1.28`, version 1.2, source adds `DEC-075`.
- **Unchanged:**
  - `REQ-PRIV-027`: it already requires authorization first, no switch from the URL or client state, and identical missing and unauthorized results. It does not require a switch for every authorized item.
  - `REQ-PRIV-022`: Trip creator retention is cited, not changed.
  - `REQ-UX-011`: the item-link rule is still the only other way the Family may change.
  - Every Event, Plan, Trip, Concern, Incident, membership, and administrator access rule.
  - `OD-002` stays open (Phase 9.5).
- No application code, test code, fixture, or dependency is changed by this baseline.
