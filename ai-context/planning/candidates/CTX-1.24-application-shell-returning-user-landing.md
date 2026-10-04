---
Status: ACTIVATED — historical candidate record
Authority: Owner-approved Phase 7.5 direction and multi-Family current-context decision
Context-Baseline: CTX-1.23
Version: 1.0-candidate
Decision: DEC-064
---

# Candidate CTX-1.24 — Application Shell & Returning-User Landing

The project owner approved PRP-023 and the multi-Family current-context rule on 2026-09-28 and authorized a context/decision update only. `DEC-064` records the decision. `CTX-1.24` is active after passing `VER-CTX-1.24`; `CTX-1.23` remains an approved historical baseline.

## Canonical effect

- Clarify existing `REQ-UX-011`: automatically select a sole active Family; require explicit selection for multiple active Families using a clear shell-level selector; do not silently select the first returned membership.
- With no active Family, select none and retain the approved invitation/operator-provisioned entry path. Retain selected Family only for the current application/page session.
- Treat selection as presentation/application context only. It grants no access and changes no membership, Event/Plan/Trip audiences, profile visibility, administrator authority, Sharing permission, or Safety permission.
- On context change, clear prior Family-scoped client state and reload data authorized for the selected Family so prior Family directories, items, and forms are not visually retained.
- Preserve `OD-001`, production authentication deferment, controlled Family provisioning, and all existing authorization boundaries.
- Record PRP-023's approved routed shell direction: Home, Family, Events, Plans & Trips, and Me / Settings; distinct Plan and Trip routes; contextual administrator entry; Development-only identity switcher; and full Home/dashboard deferred.
- No new requirement ID is added. No application code or implementation story is created or authorized.
