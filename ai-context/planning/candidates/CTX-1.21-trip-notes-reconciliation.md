---
Status: ACTIVATED — historical candidate record
Authority: Historical candidate; active normative wording is in the affected canonical workflow
Context-Baseline: CTX-1.20
Version: 1.0-candidate
Decision: DEC-060
---

# Candidate CTX-1.21 — Trip Notes authority and retained states

The project owner approved this bounded canonical clarification on 2026-09-27. It amends `REQ-TRIP-010` to make the approved Trip Notes access and state semantics explicit before any Notes implementation. `CTX-1.21` was activated after passing `VER-CTX-1.21`; `CTX-1.20` is an approved historical baseline.

## Canonical clarification

- Current authorized Trip audience members may read active Notes and add Notes. The creator may do the same through narrow own-Trip creator authority even outside the audience or after losing eligible Family Group membership. The exception does not extend to travelers, administrators, non-system contacts, or other non-audience people.
- An author with current Notes access may edit or soft-delete their own ACTIVE Note. Edits preserve author and creation time and record updated time. Soft deletion produces AUTHOR_DELETED, excludes the Note from normal display, and retains its content and provenance.
- The creator may hide another author's ACTIVE Note for moderation, producing CREATOR_HIDDEN. The original content and provenance remain retained; the creator cannot rewrite content, and the author cannot edit or delete the hidden Note through normal Notes actions. It is immutable through normal Trip Notes behavior.
- ACTIVE, AUTHOR_DELETED, and CREATOR_HIDDEN are conceptual MVP states. Removed Notes remain with the Trip while its record exists, are excluded from normal APIs and UI, and create no alternate access path. Database representation, global retention, Trip deletion, archive/disposal, history UI, and restore UI remain undefined.
- Notes are plain-text coordination, not chat. They grant no Sharing Session, location, Check-In, administrator override, or unrelated private-data access. The proposed 1,000-character limit stays at `WI-007-004` level, subject to separate work-item approval.

## Requirement and compatibility boundaries

`REQ-TRIP-010` retains its ID; no new privacy requirement is introduced. `REQ-PRIV-022` and `REQ-SHARE-009` remain the Trip audience and private-sharing boundaries. Selected-person audiences, group snapshots, membership-loss revocation, no automatic rejoin restoration, and independent creator access remain unchanged. Completed `WI-007-003` is unaffected. Plan Notes, CTX-1.19 Events, CTX-1.20 Family/Profile rules, and Sharing Session consent remain unchanged.

## Authorization boundary

This candidate authorizes context maintenance and verification only. It does not approve or execute `WI-007-004`, change application code, create a migration, or close Phase 7.
