---
Status: ACTIVATED — historical candidate record
Authority: Owner-approved PRP-028 decisions under DEC-067 and DEC-068
Context-Baseline: CTX-1.24
Version: 1.0-candidate
Decision: DEC-068
---

# Candidate CTX-1.25 — Phase 8 Concern Access and Retention

The owner approved PRP-028 fully under `DEC-067` and `DEC-068`. `CTX-1.25` is active after passing `VER-CTX-1.25`; `CTX-1.24` remains the approved historical baseline. This candidate records context changes only; it creates no implementation story or authorization.

## Canonical effect

- Add `REQ-PRIV-025` for Concern eligibility, audience, permissions, protected-data boundaries, provenance and corrections, resolution, inactivity freeze, retention, audit evidence, backup aging, restore behavior, and deployment-specific legal-review caveat.
- Add `REQ-SAFE-015` for the first Concern workflow's manual creation and lifecycle, including creator-departure and inactivity freeze.
- Clarify `REQ-PROD-012` that the first Concern slice has no automatic or manual Concern-to-Incident transition; any future transition requires separate design and approval.
- Add both new requirement IDs to the requirements index. Preserve `OD-004` as the separate Incident-history retention decision.
- Record PRP-028 `APPROVED`; preserve DEC-067 as the historical partial approval and DEC-068 as its completion.

## Approved policy summary

- Resolved product history: 180 days from resolution.
- Open Concern inactivity: freeze after 30 days without authorized mutation; reads and denied attempts do not reset it. No pre-freeze acknowledgement.
- Frozen history: currently eligible audience read-only; 180 days from freeze. No contributions, audience changes, correction/challenges, or resolution. Continuation requires a new Concern.
- Audit: minimal metadata, 180 days per event; no content/private source details; no user audit viewer or role-based Family/admin/operator audit access.
- Product expiry removes Concern content/history from active storage and access. Audit expires on its own clock.
- Backup copies age out within 30 days after active deletion; deployment docs describe mechanism and restores cannot reactivate already-expired data. No legal-hold engine.
- These are application defaults; qualified review applies before future deployment where legal obligations may apply.

No application code, database schema, migration, or implementation work item was created or authorized.
