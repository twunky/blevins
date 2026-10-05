# WI-095-007 — Trusted Contacts

- **Status:** VERIFYING / OWNER REVIEW; **Phase:** 9.5 CURRENT / IN PROGRESS.
- **Baseline:** CTX-1.34 / DEC-088; **Tier:** DEEP.
- **Requirements:** REQ-FAM-014/015/016; REQ-PRIV-019/024/032.
- **Authorization:** Explicit owner decision, 2026-10-05, resolves the blocker, approves the narrow canonical update/successor baseline and authorizes implementation in this task. No standing auto-approval, Git action, PR, sibling work or phase transition.

## Required context

| Source | Purpose |
|---|---|
| ai-context/README.md; project-status.md; claude-handoff/CURRENT-TASK.md; WORKING-RULES.md; CONTEXT-ROUTING.md | Entry, authority, checkpoint |
| core/product-contract.md REQ-FAM-014–016; core/privacy-safety-contract.md REQ-PRIV-019/024/032; domain-glossary.md Trusted Contact; requirements-index.md; decision-log.md DEC-088 | Approved manual model, privacy and lifecycle |
| src/FamilySafety.Web/README.md; claude-handoff/MOCKUP-ALIGNMENT.md; VALIDATION.md; docs/local-development.md | Structure, existing presentation and guarded validation |
| ai-context/skills/maintain-context/SKILL.md; execute-story/SKILL.md; verify-story/SKILL.md | Narrow context update, execution and self-verification |

Excluded: bootstrap/archive, broad proposals, unrelated workflows and historical evidence. Only the prior Sharing Session production-build warning baseline may be consulted for comparison.

## Scope / non-goals

Private manual owner-profile contacts: list/read/add/edit/permanent removal. Display Name; Phone and/or Email; optional Relationship / Label. Owner-only explicit API, repository/service/entity, EF mapping/migration, Safety UI and focused privacy/data/lifecycle tests. No contact identity/Profile/Family link, discovery, invitations, verification, notifications, Safety Circles, automatic sharing, downstream access, address-book framework, account deletion, restore/history or unrelated Phase 9.5 work.

## Acceptance checks

1. **AC-01:** Current adult alone lists/reads/creates/edits/removes its contacts; no other user/admin/selected Family/known ID override; missing/denied IDs nondisclosing, responses no-store.
2. **AC-02:** Trimmed name and at least one valid phone/email required; both and optional label supported; only approved descriptive fields stored, no contact identity linking/discovery or downstream permission grants.
3. **AC-03:** Email case and phone formatting duplicates rejected on create/edit within owner, either method sufficient; names alone do not deduplicate; two owners may store the same contact. Database prevents concurrent duplicates; stale edit/delete rejected.
4. **AC-04:** Contacts survive Family selection/membership changes. Removal physically deletes, cannot be read/restored and retains no feature history; ownership FK follows profile deletion without implementing account deletion.
5. **AC-05:** Safety UI supports loading/empty/error/Retry, add/edit, field validation, success/error feedback and explicit removal confirmation; identity changes cancel reads/mutations and clear private state. Desktop/narrow presentation uses existing visual language without feature Family selectors/navigation redesign.
6. **AC-06:** Focused backend/API/SQL and Angular checks, then one full SQL-gated .NET regression, full Angular, production build, one relevant Playwright journey and diff check. Record actual results/limitations as SELF-VERIFIED; hand back VERIFYING / OWNER REVIEW.

## Stop conditions

Only a new material unresolved canonical/product/privacy conflict or unexplained required validation failure. The owner's eligibility, fields, duplicate identity, Family lifecycle, deletion and no-discovery decisions are resolved.

## Evidence

Backend/schema/API and Safety manual-contact workflow implemented. [VER-WI-095-007](../verification/VER-WI-095-007.md) records AC-01–06 PASS / SELF-VERIFIED: focused backend 14, focused Angular/Safety 23, full SQL-gated .NET 355, full Angular 382, production build and one browser journey. The initial full Angular run's existing invitation timeout passed isolated and on the justified full retry; no timeout changed. Existing build warning categories persist. Migration `20261005040757_ManualTrustedContacts`; no user database migration during validation. Owner review remains required; not COMPLETE, no next work item authorized. Owner performs all commits.
