---
Status: ACTIVATED — historical candidate record
Authority: Owner-approved PRP-029 decisions under DEC-071
Context-Baseline: CTX-1.25
Version: 1.0-candidate
Decision: DEC-071
---

# Candidate CTX-1.26 — Phase 8 Safety Incident Workflow and Retention

The owner approved PRP-029 under `DEC-071`, with the lineage refinement, and resolved `OD-004`. `CTX-1.26` is active after passing `VER-CTX-1.26`; `CTX-1.25` remains the approved historical baseline. This candidate records context changes only. It creates no implementation story or authorization.

## Canonical effect

| File | Change |
|---|---|
| `workflows/safety.md` (v1.2) | Add `REQ-SAFE-016`, the MVP Safety Incident workflow. Amend `REQ-SAFE-007`: Incident participation does not unlock other-domain fields, and the MVP does not depend on them. Amend `REQ-SAFE-008`: the MVP lifecycle is `Open` / `Frozen` / `Resolved`, and the earlier labels are kept as deferred, non-MVP concepts with their MVP representation. Editorially update `REQ-SAFE-015`: starting an Incident leaves the Concern unchanged. Replace the `OD-004` open reference with a resolved reference. |
| `core/privacy-safety-contract.md` (v1.19) | Add `REQ-PRIV-026`: Incident authority, explicit membership-bound grants, permissions, nondisclosure, membership loss, protected-information boundary, non-authorizing Concern lineage, 365-day retention, 365-day audit with denied hourly summaries, 30-day backup aging, restore invariant, and legal caveat. Editorially update `REQ-PRIV-009` and the closing note of `REQ-PRIV-025`. Remove `OD-004` from the open-decision references. |
| `core/product-contract.md` (v1.17) | Amend `REQ-PROD-012`: manual creation only; the manual Start an Incident shortcut is permitted, with no automatic escalation, information transfer, or audience transfer, and a non-authorizing lineage reference. |
| `core/domain-glossary.md` (v1.9) | Update the Concern → Incident invariant from "escalate" to a deliberately started separate Incident, with no automatic transfer. |
| `core/requirements-index.md` | Mark the index `CTX-1.26`. Add `REQ-PRIV-026` and `REQ-SAFE-016`. Mark `REQ-PROD-012`, `REQ-PRIV-009`, `REQ-SAFE-007`, `REQ-SAFE-008`, and `REQ-SAFE-015` as amended in `CTX-1.26`. |
| `decision-log.md` | Add `DEC-071`. Move `OD-004` to Resolved product decisions. |
| `planning/proposals/PRP-029-…` | Status `APPROVED`. Owner-approved decisions recorded. The superseded no-link recommendation is marked historical. |

## Approved policy summary

| Area | Policy |
|---|---|
| Creation | Manual only, by any active adult member, about one active adult in the same group. Self is allowed. Direct creation, or Start an Incident from a Concern by the Concern creator. |
| Access | Explicit per-Incident grants bound to membership ID and `AccessVersion`. No automatic access from subject status, Family, Administrator, Safety Circle, Trusted Contact, or Concern participation. Rejoin never reactivates a grant. Nondisclosing denial. |
| Coordinator | One coordinator; creator first; explicit handoff to a current participant. Loss of eligibility → Frozen, no promotion. |
| Lifecycle | `Open` / `Frozen` / `Resolved`. 30-day inactivity freeze. No reopen. |
| Information | Concern provenance model, five categories, no scores, append-only correction and challenge. Record-only contact attempts. Simple tasks without reassignment. Read-time timeline projection. |
| Resolution | Coordinator only. Required summary, categorized outcome, and contact-restored time or Unknown. No preset verdicts. Final. |
| Lineage | The Incident stores the originating Concern ID only. Non-authorizing; never discloses the other record. |
| Retention (`OD-004`) | 365 days from freeze or resolution. Unavailable immediately at expiry, then removed from active storage. |
| Audit | 365 days per event. Metadata only. Denied hourly summaries (`IncidentReadDenied` / `IncidentWriteDenied`, no Incident ID). No viewer. |
| Backup | Aged out within 30 days of active deletion. Restore never reactivates expired data. Operational evidence is predeployment. |

These are application defaults, not legal requirements. Qualified review applies before a future deployment with legal obligations.

No application code, database schema, migration, UI, or implementation work item was created or authorized.
