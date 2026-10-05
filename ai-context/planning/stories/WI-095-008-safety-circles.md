# WI-095-008 - Safety Circles

- **Status:** COMPLETE / PASS — OWNER APPROVED (2026-10-05)
- **Phase:** 9.5 CURRENT / IN PROGRESS
- **Context baseline:** CTX-1.35 (DEC-089)
- **Requirements:** REQ-FAM-017; REQ-PRIV-033; REQ-PRIV-030/031; REQ-SHARE-010/011; REQ-PRIV-005/006/007/011/012/013; REQ-PROD-013.
- **Model tier:** DEEP

## Required context

| Source | Purpose |
|---|---|
| README.md; project-status.md | Authority/checkpoint |
| core/product-contract.md#req-fam-017, #req-prod-013 | Bounded adult-only CRUD |
| core/privacy-safety-contract.md#req-priv-033, #req-priv-030, #req-priv-031; #req-priv-005 through #req-priv-013 | Privacy, lifecycle, explicit grants and eligibility |
| workflows/sharing-checkins.md#req-share-010, #req-share-011 | Creation-only integration |
| core/domain-glossary.md#safety-circle; decision-log.md#dec-089 | Terminology and owner decision |
| ../claude-handoff/WORKING-RULES.md, VALIDATION.md, MOCKUP-ALIGNMENT.md; ../src/FamilySafety.Web/README.md | Implementation/UI/validation conventions |

Excluded: broad phase audit, bootstrap/archive, unrelated workflows and evidence packages. Relevant Safety workflow/privacy anchors were checked for independent Concern/Incident boundaries; neither feature changes.

## Goal and non-goals

Owner-only Family presets with optional Check-In/Temporary Sharing creation selectors. No live audience, Trusted Contacts/external/child/cross-Family members, notifications, location automation, other product integrations, generic groups/ACL/retention or unrelated work.

## Acceptance checks

1. AC-01: Owner CRUD, scoped normalized names, plain-text validation, concurrency/confirmed deletion; nonowner/admin/known-ID/selected-Family nondisclosure.
2. AC-02: Existing same-Family active adult profiles only; reject ineligible/cross-Family/Trusted Contact IDs. Zero inherited authority.
3. AC-03: Member loss permanently removes only membership; rejoin cannot restore; explicit re-add works. Owner loss deletes circle/member rows; rejoin/admin cannot restore/take over.
4. AC-04: Both creation flows optionally populate currently eligible recipients, visibly allow adjustment and save normal explicit grants. Later circle changes/deletion leave target grants unchanged, subject to target membership-loss/expiration.
5. AC-05: Angular loading/empty/error/Retry/create/rename/member/delete/presets/context reset; one isolated browser journey at desktop/narrow widths.
6. AC-06: Focused SQL/API/Angular checks, one full SQL-gated .NET + Angular + production build + relevant Playwright regression and diff check; no staging/commit/PR.

## Tasks and authorization

- [x] Canonical boundary and separate CTX verification.
- [x] Backend/schema/API and focused privacy/lifecycle coverage.
- [x] Safety UI and presets with focused Angular/browser coverage.
- [x] Final regression and owner-review handoff.

Explicit project-owner authorization: original WI-095-008 instruction plus owner-loss decision, 2026-10-05. No standing auto-approval needed. Owner accepted WI-095-008 on 2026-10-05. Next work item: NONE AUTHORIZED.

## Evidence

Verification: [VER-WI-095-008](../verification/VER-WI-095-008.md). Verification COMPLETE / PASS; owner accepted implementation and validation on 2026-10-05. Next work item: NONE AUTHORIZED.
