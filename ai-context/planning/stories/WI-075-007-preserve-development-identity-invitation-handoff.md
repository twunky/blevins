---
Status: DONE — AC-01 through AC-07 PASS; AC-07 live browser evidence developer-supplied
Authority: Owner-approved execution, 2026-09-29; completion evidence recorded in VER-WI-075-007
Context-Baseline: CTX-1.24
Version: 1.0
---

# WI-075-007 — Preserve Development Identity Across Invitation Link Handoff

## Metadata

- **Work Item ID:** `WI-075-007`
- **Status:** `DONE — AC-01 through AC-07 PASS`
- **Phase:** `7.5 — End-to-End UX & User-Test Readiness`
- **Context baseline:** `CTX-1.24`
- **Execution authorization:** `YES` — project-owner approval, 2026-09-29
- **Work type:** Bounded Development identity/session isolation bug fix.
- **Traceability:** Blocked `WI-075-005` Stage 1 and blocker evidence in [`VER-WI-075-005.md`](../verification/VER-WI-075-005.md); first-use behavior in [`WI-075-001`](WI-075-001-invited-adult-first-use-journey.md); shell and identity behavior in [`WI-075-002`](WI-075-002-returning-user-shell-landing.md); current Family context behavior in `CTX-1.24`.

## Confirmed failure and cause

In fresh scenario database `FamilySafety_Invitation_Stage1Retry_20260928_235812_c7f2`, Adult A was the initial Development identity. Switching to Adult B succeeded, and B correctly had no active Family. Opening the invitation URL reset the active identity to A, restored A's Family context, and made the invitation unavailable. Stage 1 stopped at this `A — Bug`, `BLOCKER` finding; acceptance was not reached.

The inspected implementation confirms the cause: the selected Development identity is held in an in-memory signal. Opening the outbox invitation through a normal anchor causes a document navigation and app reinitialization. Development session bootstrap then unconditionally selects the default fixture identity, Adult A, after loading the available identities. Invitation preview and Family state consequently load under A. The retry evidence records the observed identity reset and unavailable invitation.

## Goal

In Development, preserve an explicitly selected identity across invitation URL navigation and page reload according to the approved Development-session design. If B is selected, B remains active; A and A-scoped Family state do not return; and a valid invitation preview is evaluated for B. Existing production-facing invitation behavior and authorization remain unchanged.

## Bounded scope

Inspect and, after separate execution authorization, change only the Development identity/session initialization and the minimum related client state handling needed to meet this story. Expected implementation area is `src/FamilySafety.Web/src/app/development-session.ts`, with focused tests in the Development/session, shell, and first-use Angular test areas. Add or update `VER-WI-075-007.md` for evidence.

Do not change invitation authorization, Family membership rules, acceptance semantics, profile review, Home handoff, production authentication, unrelated shell behavior, canonical requirements, `OD-001`, `WI-075-005`, or `WI-075-006`. Do not create a new session architecture or alter product behavior beyond retaining the explicit Development identity through the handoff.

### Persistence and bootstrap boundary

The implementation may choose the smallest practical browser mechanism for the selected Development identity; the story does not require a particular storage API. Its lifetime is limited to the current browser tab/session so it can survive the invitation link's document reload, but it does not become a durable account preference or shared cross-tab state. If browser storage is used, store only the selected Development identity identifier, and only for Development behavior. Do not persist Family selection, Family-scoped lists/details/forms, private item data, or profile data. After the allowlisted Development identities load, restore a stored identity only if its identifier is present in that list; otherwise ignore or clear it and use the normal fixture default. Identity switching updates the selected Development identity and continues to clear/reload all prior identity-scoped client state. This identifier is test tooling and is never authentication or authorization evidence.

## Acceptance criteria

1. **`AC-01 — Preserve selected identity`**: After explicitly selecting B, opening a valid invitation URL or refreshing that route keeps B active for the current Development session. After allowlisted identities load, a valid explicitly selected Development identity takes precedence over the fixture default; an absent or invalid stored identifier safely falls back to the normal fixture default.
2. **`AC-02 — No prior-identity state`**: During and after the handoff, A is not selected automatically and no A Family context, Family-scoped data, or stale A session data appears while B is active. Switching identities still clears prior identity-scoped client state before loading the new identity.
3. **`AC-03 — Invitation preview and response`**: A valid link opened as B loads the existing B-eligible invitation preview, with Accept and Decline available when otherwise valid. Routing or preserving the Development identity grants no invitation or Family access; existing authorization, membership, and acceptance/decline semantics remain authoritative.
4. **`AC-04 — Acceptance and membership refresh`**: Accepting the valid invitation follows the existing acceptance flow and refreshes B's authoritative membership in the same session. Decline remains available and follows the existing decline flow without creating membership.
5. **`AC-05 — Family and Home handoff`**: After successful acceptance and membership refresh, the existing profile review, Family, and Home handoffs work for B without a browser refresh and without carrying stale state from A.
6. **`AC-06 — Development and production boundary`**: Identity persistence remains Development-only test tooling. Production identity and invitation behavior are unchanged; Development identity, relationship, member, or administrator status does not grant private-data authority.
7. **`AC-07 — Regression and privacy evidence`**: Focused Angular tests and relevant existing invitation regressions pass. A fresh isolated scenario verifies A baseline, A-to-B switch, B's no-Family state, invitation URL navigation, preview, acceptance, membership refresh, profile review, Family/Home handoff, and absence of cross-identity data. Where persistence is used, tests verify only an allowlisted Development identity identifier is retained, an unknown identifier is ignored or cleared, and no Family or private data is persisted. A relevant API regression is run if investigation shows server-facing behavior was touched.

## Verification plan

- Add focused Angular coverage for identity bootstrap precedence, valid and invalid persisted identity handling when applicable, reload/direct invitation URL handling, and A-to-B/B-to-A state clearing; run relevant existing invitation and shell regressions.
- If implementation touches server-facing behavior, run the relevant .NET/API regression without changing authorization rules.
- Use a fresh dedicated invitation scenario database. Verify A's baseline, switch A to B, confirm B has no Family, open the generated invitation URL, and verify B remains active while A's state stays absent.
- For a valid invitation, verify preview, Accept/Decline availability, successful acceptance, same-session membership refresh, optional profile review, and Family/Home handoff. Check that invitation routing alone grants no access and no stale Family selection or data crosses identities.
- Run an approximately 390 px invitation-path smoke where applicable. Do not run the full human test or resume `WI-075-005` as part of this work item.
- Record each acceptance result and evidence in `VER-WI-075-007.md`, labeled `SELF-VERIFIED` unless independently verified.

## Privacy and isolation boundary

Development identity selection is a test-session selector only. The server remains authoritative for invitation eligibility, membership, and access. Switching identities clears prior identity-scoped client state; the application must not render A's Family context or private/session data while B is selected. Invitation URL routing must not grant access. Relationship, member, and administrator status does not change private-data authority.

## Canonical requirements

No canonical product requirement change is expected. The story traces to the approved Development first-use and shell behavior and the existing `CTX-1.24` Family-context rules. Any implementation finding that requires new or ambiguous product behavior must stop for clarification rather than expanding this story.

## Execution status

The Development identity change and focused Angular regressions are passing. Developer-supplied external evidence confirms a successful unrestricted SQL/scenario run and Angular production build; earlier Codex restricted-environment SQL/TLS and build failures remain historical. Developer-supplied live browser evidence confirms B stays selected across invitation-link reload, A Family state does not return, invitation preview and acceptance work, membership refreshes without reload, profile skip and Family/Home handoffs work, no stale A data appears, and the 390 px smoke passes. AC-01 through AC-07 pass and this story is `DONE`; see [`VER-WI-075-007.md`](../verification/VER-WI-075-007.md). `WI-075-005` remains blocked and was not retried; its Stage 1 retry requires separate authorization.
