# WI-095-010 — Household Grouping

- **Status:** COMPLETE / PASS — OWNER APPROVED, 2026-10-05. Verification COMPLETE / PASS.
- **Tier / baseline:** DEEP / CTX-1.36 (narrow successor to CTX-1.35 under DEC-091).
- **Goal:** Organize existing active adult members within a Family, with read-only member visibility and current Family-administrator management.
- **Governing sources:** REQ-FAM-001/018, REQ-PROD-013, REQ-PRIV-011/013/034; DEC-073/091.

## Required context

- `core/product-contract.md#req-fam-018`: organization, eligibility and lifecycle; `#req-prod-013`: minors deferred.
- `core/privacy-safety-contract.md#req-priv-034`: current Family membership/admin checks and zero downstream authority; `#req-priv-011`/`#req-priv-013`: membership and display-name privacy.
- `core/domain-glossary.md#household`: descriptive meaning only.
- `claude-handoff/VALIDATION.md` and `src/FamilySafety.Web/README.md#structure-and-extraction`: focused checks and existing UI ownership patterns.

Excluded: unrelated workflows, bootstrap/archive, prior verification packages, generic group/concurrency/retention frameworks, minor profiles, addresses, guardian/custody/legal/financial relationships, notification and feature-audience integrations.

## Acceptance checks

1. Current active same-Family members see names, active member assignments and unassigned members only; nonmembers and selected Family/known IDs grant nothing.
2. Every create/rename/assignment/move/unassign/delete rechecks existing current Family-admin authority. Households have no creator ownership; loss of creating admin authority or membership leaves the Household manageable by another current admin.
3. Short plain-text names normalize uniquely within each Family; equal names across Families and empty Households are allowed.
4. Active adult same-Family members have zero or one Household; assignment, move and unassign work; cross-Family/inactive/nonmembers are rejected. Stale rename/move/unassign/delete cannot overwrite newer organization.
5. Membership loss removes assignments immediately; rejoin and replacement profiles inherit nothing. Deletion is permanent, unassigns members, preserves Family membership and all unrelated feature data/access.
6. Existing Family Settings presents read-only member organization and admin CRUD using current loading/empty/error/Retry/confirmation/feedback patterns, with Family/identity reset and desktop/narrow usability.
7. Focused backend/API/SQL and Angular checks, then ONE final SQL-gated .NET regression, full Angular, production build, relevant Playwright journey and diff check. Verification is SELF-VERIFIED; no owner acceptance inferred.

## Authorization / handback

Owner accepted WI-095-010 on 2026-10-05. Verification: [VER-WI-095-010](../verification/VER-WI-095-010.md) COMPLETE / PASS. No next work item is authorized. Owner performs all Git commits; no staging, commit, push, or PR by agents.
