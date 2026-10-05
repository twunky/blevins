# WI-095-005 — Standalone Check-Ins

- **Status:** VERIFYING / OWNER REVIEW
- **Phase:** 9.5 — MVP Feature Completion
- **Context baseline:** CTX-1.32
- **Requirements:** REQ-SHARE-006/007/008/009/010; REQ-PRIV-002/005/006/011/013/030; REQ-FAM-005/007/008; REQ-SAFE-001; REQ-UX-010/013/014.
- **Model tier:** DEEP

## Required context

| Source | Purpose |
|---|---|
| README.md; project-status.md; current handoff/routing | Authority and checkpoint |
| workflows/sharing-checkins.md, REQ-SHARE-006–010 | Status, optional note, immutable standalone slice |
| core/privacy-safety-contract.md, requirements above | Explicit grants, membership loss, own history, nondisclosure |
| core/product-contract.md, REQ-FAM-005/007/008; core/domain-glossary.md, Check-In | Adult membership/profile and independent Check-In |
| core/ux-contract.md, REQ-UX-010/013/014; Web README; handoff MOCKUP-ALIGNMENT/VALIDATION | Existing navigation, component boundary, calm accessible presentation, lean validation |

Excluded: bootstrap, HISTORY, old evidence/screenshots, unrelated completed manifests, location/Sharing Session workflows.

## Goal and approved decisions

Create/read immutable, self-authored standalone status records with an optional plain-text note and explicitly chosen active adult recipients in one Family where the author is active. Capture exact profiles and membership grants; invalidate sharing permanently on either party's eligibility loss. Own history survives author departure. No invented expiry, editing, deletion, restore, or recipient replacement.

## Boundaries

No location, Event/Plan/Trip association, Sharing Sessions, OD-002 resolution, default Family audience, Safety Circle/Trusted Contact integration, notification/reminder, Concern/Incident creation, safety inference, chat/comments/attachments, generic feed/sharing/retention framework, or navigation redesign.

## Acceptance checks

1. AC-01 — Own creation uses the authenticated profile, five canonical statuses, optional plain-text note, and explicit same-Family eligible recipients; invalid status/ineligible choices reject atomically.
2. AC-02 — Mine/received/detail queries authorize before projection. Missing and denied detail are nondisclosing; known IDs, administrator status and selected Family grant nothing. Recipient identities are not disclosed to other recipients.
3. AC-03 — Either party's membership loss ends recipient access permanently; rejoin/replacement profile cannot inherit it. Author retains own history and stored provenance grants no access.
4. AC-04 — Existing Safety area provides create, explicit choices, sent/shared history, loading/empty/error/Retry/feedback and usable narrow layout. No unauthorized edit/delete/share actions.
5. AC-05 — Migration and focused domain/API/SQL/Angular/journey checks prove the above. One final SQL-gated .NET regression, Angular regression, production build, relevant Playwright and diff check; disclose self-verification.

## Tasks

- Record narrowly scoped DEC-086 and CTX-1.32 privacy baseline.
- Implement domain, service, repository/query projections, EF/migration and API.
- Implement standalone Check-In component in Safety, retaining existing visual patterns.
- Validate and leave VERIFYING / OWNER REVIEW with concise evidence.

## Execution authorization

- **Approved work item:** YES
- **Execution authorized:** YES
- **Authorized by:** Project owner's original standalone Check-In prompt and subsequent recipient-eligibility decision, 2026-10-04.
- **Authorized scope:** This item only; owner performs all Git commits. No next item authorized.

## Stop conditions

Only a new material canonical status, privacy, retention or mutation conflict, or unavoidable excluded dependency. The approved membership rule is resolved.

## Verification

[VER-WI-095-005](../verification/VER-WI-095-005.md) records AC-01–05 and requirement mappings: PASS / SELF-VERIFIED. Focused backend 6, Angular 19 and E2E 1 passed; one final SQL-gated .NET regression 332, Angular 366 and relevant E2E 5 passed; production build and final context/diff checks passed. Existing build warning categories remain; no thresholds raised. No retained test database or API process. Owner review/acceptance remains pending; no completion or next-item authorization inferred.
