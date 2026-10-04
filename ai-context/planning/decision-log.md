## DEC-065 — Close PRP-027 and Phase 7.5

- **Status:** ACCEPTED — PRP-027 and Phase 7.5 formally closed
- **Date:** 2026-09-29
- **Decision:** Close `PRP-027 — Mockup Alignment & UI Gap Assessment` as `CLOSED / COMPLETED` because its approved implementation purpose was fulfilled through WI-075-010 shared foundation, WI-075-011 Home, WI-075-012 Family and first-use, WI-075-013 Events, WI-075-014 Plans, and WI-075-015 Trips. Close Phase 7.5 — End-to-End UX & User-Test Readiness as `COMPLETE / CLOSED`; its exit areas are assessed as satisfied in `planning/verification/VER-PH-075.md`. Preserve WI-075-005 as `DONE` with its accepted owner/developer-supplied evidence limitations; another participant study is not required for closure. Keep `OD-001` `OPEN`; its final mobile-navigation grouping concerns later UX validation and does not block the completed Phase 7.5 scope. Preserve the useful reference direction while excluding unsupported mockup-only functionality and retaining approved privacy/access rules. Remaining spacing, density, responsive and visual refinements are future polish, not unfinished PRP-027 scope.
- **Rationale:** The completed readiness assessment found all known PRP-027 implementation slices and Phase 7.5 exit areas satisfied, with no MUST-FIX blocker. The project owner explicitly authorized both closures.
- **Affected context:** `README.md`, `project-status.md`, `planning/roadmap.md`, `planning/proposals/PRP-027-mockup-alignment-ui-gap-assessment.md`, `planning/verification/VER-PH-075.md`, and this decision log.
- **Compatibility:** `CTX-1.24` remains active. No canonical requirement, contract, implementation, or completed story history changes. `WI-075-005` remains DONE. `OD-001` remains OPEN. Phase 8 remains FUTURE; this decision does not start Phase 8 or authorize implementation work.
- **Deferred / non-blocking:** Broader formal usability validation, stylesheet budget warnings and future maintenance, the unrelated DevelopmentSession template TypeError, responsive polish, and minor visual refinement remain future non-blocking items. No new work item is created.
- **Canonical effect:** Phase 7.5 is `COMPLETE / CLOSED`; PRP-027 is `CLOSED / COMPLETED`; `CTX-1.24` remains active because this phase/proposal status closure changes no canonical requirements. Phase 8 remains `FUTURE`.
- **Approved by:** Explicit project-owner authorization to formally close PRP-027 and Phase 7.5 on 2026-09-29.

## DEC-066 — Enter Phase 8 Safety Coordination

- **Status:** ACCEPTED — Phase 8 transition only
- **Date:** 2026-09-29
- **Decision:** Set Phase 8 — Safety Coordination to `CURRENT / IN PROGRESS`. Preserve Phase 7.5 as `COMPLETE / CLOSED` under `DEC-065` and PRP-027 as `CLOSED / COMPLETED`. Create `PRP-028 — Phase 8 Concern Access and Lifecycle Proposal` for owner review; its proposed permissions, lifecycle, retention, and phase exit criteria are not approved by this transition.
- **Rationale:** The owner explicitly authorized Phase 8 entry after the Phase 7.5 closure and readiness assessment. The first planning question is a narrow, manually raised Concern with explicit privacy and uncertainty boundaries.
- **Affected context:** `README.md`, `project-status.md`, `planning/roadmap.md`, `planning/proposals/PRP-028-phase-8-concern-access-and-lifecycle.md`, and this decision log.
- **Compatibility:** `CTX-1.24` remains active. This is a status-only transition and adds no canonical requirement. `OD-001`, `OD-002`, and `OD-004` remain `OPEN`; `OD-003` remains outside MVP. Existing Safety, privacy, Family, membership, and authorization rules remain controlling.
- **Deferred:** Phase 8 exit criteria require separate owner approval. Concern actor/access, lifecycle, creator departure, and Concern-history retention require owner decisions and any needed canonical update before an implementation work item. Incident scope and `OD-004` are not decided here.
- **Execution authorization:** Governance/status update and `PRP-028` proposal creation only. No implementation story, code, migration, or Safety implementation is authorized.
- **Approved by:** Explicit project-owner Phase 8 entry and planning-proposal authorization on 2026-09-29.

## DEC-067 — Approve PRP-028 Concern rules and Phase 8 exit criteria direction

- **Status:** ACCEPTED — PRP-028 partial approval; retention pending
- **Date:** 2026-09-30
- **Decision:** Approve for PRP-028 the owner-specified Concern creation eligibility; no automatic subject access with creator-selected subject participation; selected participant read/contribution/correction/challenge rights and limits; creator control of participant selection/change and resolution; creator-only attributed resolution; freeze/no-transfer behavior on creator membership loss; immediate access revocation without authorship erasure for other membership loss; no access or copied/linked data from protected domains; canonical provenance/confidence categories and append-only corrections; resolved Concern behavior; no Concern-to-Incident transition in the first slice; no automatic Safety Circle/Trusted Contact access, location/Check-In/Sharing dependency, notifications, or escalation. Approve the 13 clarified Phase 8 exit-criteria areas recorded in PRP-028 and the roadmap; broad formal participant usability research is not required absent a separate approval.
- **Rationale:** These owner choices bound the Concern capability and phase gates while preserving canonical privacy and uncertainty rules.
- **Affected context:** `planning/proposals/PRP-028-phase-8-concern-access-and-lifecycle.md`, `planning/roadmap.md`, `project-status.md`, `README.md`, and this decision log.
- **Compatibility:** `CTX-1.24` remains active; no canonical product or privacy requirement has yet been amended. `OD-001`, `OD-002`, and `OD-004` remain open; `OD-003` remains outside MVP. `OD-004` remains separate and concerns Incident history.
- **Deferred:** Concern product-history retention; Open/Frozen expiration; audit-evidence scope and duration; backup-aging and restore behavior. No duration is selected. PRP-028 remains `PROPOSED — RETENTION DECISION PENDING` and is not fully approved.
- **Execution authorization:** Proposal and roadmap updates plus preparation of a separate Concern-retention decision packet only. No implementation story, application code, migration, or Safety implementation is authorized.
- **Approved by:** Explicit project-owner decision dated 2026-09-30.

## DEC-068 — Approve complete PRP-028 Concern retention policy

- **Status:** ACCEPTED — PRP-028 approved in full; `CTX-1.25` active after verification
- **Date:** 2026-09-30
- **Decision:** Resolved history: 180 days from resolution. Open inactivity: freeze automatically after 30 days without authorized mutation; no pre-freeze acknowledgement. Frozen: eligible-audience read-only, 180 days from freeze, no contributions/audience changes/corrections/challenges/creator resolution; continue in a new Concern. Freeze is not resolution, a safety/danger finding, verification/invalidation, Incident creation, or escalation. Audit: minimum metadata, 180 days per event for create, participant changes, contribution/correction/challenge, freeze, resolution, revocation, and denied read/write; no Concern content/restricted source details, no user audit viewer, and no automatic Family-member/admin/operator access. Expiry removes all Concern product history from active storage/access; audit expires independently. Backups age out within 30 days of active deletion and restore must not reactivate expired data. No legal-hold engine; these are application defaults, with qualified review before any deployment having applicable legal obligations.
- **Compatibility:** Completes `DEC-067`; `OD-004` remains open for Incident history. No implementation story, code, schema, or migration is authorized.
- **Affected context:** `planning/proposals/PRP-028-phase-8-concern-access-and-lifecycle.md`, `planning/roadmap.md`, `README.md`, `project-status.md`, canonical contracts/workflow, and verification `VER-CTX-1.25.md`.
- **Approved by:** Explicit project-owner decision dated 2026-09-30.

