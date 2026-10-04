---
Status: DONE
Authority: PRP-022 approved by project owner in the 2026-09-28 session; execution explicitly authorized in the 2026-09-28 session
Context-Baseline: CTX-1.23
Version: 1.0
---

# WI-075-001 — Invited Adult First-Use Journey

## Metadata

- **Work Item ID:** `WI-075-001` (`075` denotes Phase 7.5)
- **Status:** `DONE`
- **Phase:** `7.5 — End-to-End UX & User-Test Readiness`
- **Context baseline:** `CTX-1.23`
- **Proposal:** `PRP-022 — Invited Adult Entry into an Already Provisioned Family`, approved with owner decisions in the 2026-09-28 session; proposal text and approval are in that session, not a repository proposal file
- **Execution authorization:** `YES`
- **Model tier:** `DEEP` (invitation, identity-test boundary, and privacy regression)

## Goal

An administrator of an already provisioned Family can invite an existing adult Account/Profile that has never belonged to that Family. The invited adult can respond, gain active membership through explicit acceptance, review or skip profile changes, understand Family context, and reach a useful low-data Home entry point in one session without a browser refresh. A Development-only two-identity scenario makes the full path repeatable.

## Requirement traceability

- `REQ-FAM-005/006` — invitation-only membership and controlled first-Family provisioning.
- `REQ-FAM-007/008` — approved adult profile fields and normal-adult invitation lifecycle.
- `REQ-FAM-009` — existing adult Family Relationship entry points; relationship grants no permission.
- `REQ-PRIV-011/013/014` — membership access boundary, profile field privacy, invitation token/preview/acceptance privacy, and Development outbox boundary.
- `REQ-UX-001/002/003/006/007/008/011` — family-first presentation, attention-first and low-data Home, progressive disclosure, accessibility, and conceptual destinations without deciding mobile grouping.
- `REQ-PRIV-020/021/022` — preserve existing item-specific Event, Plan, and Trip access; Family membership is not universal item access.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md`; `ai-context/project-status.md` | Authority, mode, active baseline and phase | Entry |
| `ai-context/planning/roadmap.md#phase-75--end-to-end-ux--user-test-readiness`; `ai-context/planning/proposals/PRP-021-phase-7-5-user-test-readiness.md` | Phase boundary and approved first-use direction | Before execution |
| Approved `PRP-022` and owner decisions in the 2026-09-28 session | Specific scope and skippable-profile/Development-fixture decisions | Before execution |
| `ai-context/core/product-contract.md#req-fam-005` through `#req-fam-009` | Provisioning, invitation, profile and relationship rules | Before execution and verification |
| `ai-context/core/privacy-safety-contract.md#req-priv-011`, `#req-priv-013`, `#req-priv-014`, `#req-priv-020` through `#req-priv-022` | Access and disclosure boundaries | Before execution and verification |
| `ai-context/core/ux-contract.md#req-ux-001` through `#req-ux-003`, `#req-ux-006` through `#req-ux-008`, `#req-ux-011` | First-use and bounded Home presentation | Before execution and verification |
| `ai-context/planning/stories/WI-005-001-adult-self-profile.md` through `WI-005-004-adult-family-relationships.md` | Completed Family/Profile behavior to integrate, not redefine | Before execution |
| `docs/local-development.md` and existing Development fixture/identity/outbox source | Current two-identity test constraints | Before execution |

## Context intentionally not loaded

| Area | Why excluded |
|---|---|
| Safety, Sharing Session, Check-In, notification, and detailed Event/Plan/Trip workflows | No new behavior in those areas; use only targeted access regression evidence |
| Bootstrap, archive, mockups, and historical verification | Not authority for this bounded journey |
| Production identity, registration, email-provider, hosting, and deployment options | Remain undecided and outside this work item |

## Approved story behavior

1. Present a focused invitation response using the existing token flow. After authorized preview, show only Family name, inviter display name, Accept, and Decline. Use the existing uniform unavailable handling and existing capacity-conflict handling. Before acceptance, disclose no Family data beyond the approved preview context.
2. On successful acceptance, reload `/api/family-groups/mine` in the same session, reconcile/select the accepted Family using server-confirmed membership, and load dependent Family directory and relationship context. Clear the consumed token. If the reload fails, show a retry for **membership/context loading**, never an instruction to accept the used token again. Do not require a browser refresh.
3. Present a recommended, skippable review of the current adult profile: required display name; optional contact email, contact phone, and birthday month/day; and existing per-field visibility choices. The owner may save changes or continue without changes. Profile review is not a membership or Family-entry gate. Invitation delivery email is not copied into profile or sign-in data.
4. Hand off to the active Family identity, the user's own membership context, active adult directory, and existing relationship entry points. Preserve server-enforced profile visibility and the distinction between relationship and permission.
5. Offer a minimal low-data Home destination with Family welcome/context, a truthful no-attention message limited to information this destination actually checks, one clear next action, and links to currently available Family/Event/Plan/Trip areas. Show only data the current user may access. This is not the complete Home hierarchy or a shell redesign.
6. Add a dedicated Development invitation scenario with A as active Family administrator and B as an existing Account/Profile with **no current or former membership** in that Family. Provide Development-only switching between these allowlisted accounts, clearly display the selected test identity, clear prior-identity client state when switching, and reload data for the newly selected identity. Reuse the Development invitation outbox. Make the scenario repeatable with an isolated local test database or an explicit reset/reseed mechanism; do not silently delete ordinary Development data. Reject or disable all scenario and switcher mechanisms outside Development. They do not select or alter production authentication or production authorization.

## Boundaries / non-goals

- Preserve controlled/operator first-Family provisioning and the existing invitation lifecycle, token security, acceptance eligibility, capacity checks, and uniform unavailable response.
- The current implementation does not verify that the signed-in Account owns the invitation delivery email. Do not claim or add that verification in this story.
- Do not create Accounts or profiles from an invitation; B already has both. Do not add a new profile field or mandatory profile-completion gate.
- No public discovery, self-service Family creation, join request, production authentication, registration, or email delivery.
- No administrator transfer, member removal, expanded administrator authority, or pending-invitation administration overview.
- No full application-shell/navigation or Home/dashboard redesign; do not resolve `OD-001`.
- No Event, Plan, or Trip UX redesign; no RSVP, Event Needs, or Event Updates; no Safety, Sharing Session, Check-In, or notification work.

## Acceptance checks

1. **`AC-01 — Focused preview`**: An eligible signed-in B opening a valid link sees the Family name and inviter display name in a focused response with explicit Accept and Decline; opening/preview alone grants no Family access.
2. **`AC-02 — Acceptance and decline`**: Accept invokes the existing explicit acceptance operation and creates active membership only on success; Decline invokes the existing decline operation and creates no membership.
3. **`AC-03 — Unavailable and capacity states`**: Invalid, expired, revoked, declined, used, and otherwise ineligible invitations receive one non-disclosing unavailable presentation; an acceptance-time capacity conflict uses the existing capacity response. No recipient-email ownership claim is shown.
4. **`AC-04 — Same-session transition`**: After acceptance, the client reloads active memberships, selects the accepted Family from the authoritative result, and loads its dependent Family context without a browser refresh. The no-Family view does not remain stale.
5. **`AC-05 — Failed handoff retry`**: If post-accept membership or Family-context loading fails, B can retry that loading; the used invitation is not resubmitted or represented as pending.
6. **`AC-06 — Profile review`**: B sees their existing display name, approved optional fields, and current per-field visibility settings, may save using existing owner-only profile rules, and may continue without editing. No new field or automatic invitation-email copy appears.
7. **`AC-07 — Family handoff`**: The selected Family name and B's active membership context are clear; its active adult directory loads, existing relationship entry points remain available, and only server-authorized shared profile values appear.
8. **`AC-08 — Low-data Home`**: B can reach a minimal Home view showing Family context, a no-attention message qualified to the information actually checked when appropriate, a clear next action, and links to available areas; it does not expose unavailable or unauthorized Event/Plan/Trip content or imply a full dashboard.
9. **`AC-09 — Development scenario isolation`**: In the dedicated scenario A is an active administrator, B starts with an Account/Profile and no current or former Family membership, and the account switcher selects only allowlisted Development identities. The active test identity is visible; switching clears prior-identity client state and reloads data under the newly selected identity. The scenario/switcher cannot activate outside Development or change production identity/authorization behavior.
10. **`AC-10 — Outbox journey`**: A sends an invitation, a tester retrieves its link from the Development outbox, switches to B, previews and accepts, then completes profile, Family, directory, and Home handoffs without refreshing the browser. The scenario can be repeated through its documented isolated database or reset/reseed path.
11. **`AC-11 — Boundary regression`**: Existing invitation lifecycle, owner-only profile editing and visibility, Family directory authorization, relationship/administrator separation, and Event/Plan/Trip item-specific access remain enforced. No self-service Family creation or production identity behavior is introduced.

## Tasks

- [x] `TASK-001` — Reconfirm active baseline, owner-approved proposal decisions, and source behavior before execution; stop for a genuine canonical conflict or new requirement.
- [x] `TASK-002` — Implement focused invitation response and same-session membership/context reload with retry.
- [x] `TASK-003` — Implement skippable profile, Family, and minimal Home handoffs using existing fields and permissions.
- [x] `TASK-004` — Implement and document the isolated Development A/B scenario, account switching, and outbox procedure.
- [x] `TASK-005` — Add targeted application/service, API, UI, and Development-guard tests; run the complete scenario and relevant regressions.
- [x] `TASK-006` — Run production build and scoped checks; create `VER-WI-075-001` mapping every acceptance check and traced requirement to evidence, with independent verification preferred or `SELF-VERIFIED` clearly labeled.

## Verification plan

- Exercise invitation lifecycle and acceptance through application/service and API tests, including unavailable and capacity responses and the absence of pre-acceptance Family access.
- Exercise the response, acceptance/decline, membership reload, failed-load retry, skippable profile, Family, and Home transitions in UI tests without browser refresh.
- Test Development identity selection, visible identity labeling, client-state clearing on switch, and non-Development rejection/disablement; verify B's initial nonmember state and repeatable outbox scenario.
- Recheck owner-only profile edits, field omission/visibility, directory authorization, and existing relationship behavior. Run targeted Event/Plan/Trip access regression where membership changes could affect the handoff; do not expand those features.
- Run application and production builds. Record commands/results and scope scans in future `VER-WI-075-001`, mapping `AC-01`–`AC-11` and every requirement listed above to evidence.

## Risks and unresolved decisions

- **No blocking product decision is known.** The owner decided that profile review is recommended and skippable and approved the bounded Development approach.
- Implementation may choose an isolated Development database or explicit reset/reseed path, provided it is repeatable and does not silently destroy ordinary local data.
- If authoritative Family reconciliation requires a small accept-response identifier, the implementation may propose that wiring within existing membership semantics. Any new access or identity semantics require a separate proposal and decision.
- Recipient-email ownership verification and production identity remain deliberately unresolved outside this story.

## Execution authorization

- **Approved work item:** `YES`
- **Execution authorized:** `YES`
- **Authorized by:** Project owner, explicit execution approval
- **Authorized at:** 2026-09-28
- **Authorized scope:** WI-075-001 only, all 11 acceptance checks

## Evidence / verification

| Check | Evidence | Result |
|---|---|---|
| `AC-01`–`AC-11` and all traced requirements | [VER-WI-075-001](../verification/VER-WI-075-001.md): SQL-enabled .NET tests, Angular tests, release builds, and browser scenario | `PASS` |

- **Verification report:** [VER-WI-075-001](../verification/VER-WI-075-001.md)
- **Independence:** `SELF-VERIFIED`
- **Outcome:** `PASS`

## Completion record

Completed 2026-09-28. The accept response returns the committed Family ID; a keyed workspace and request cancellation isolate Development identities; a dedicated database seeds B without membership. All 11 acceptance checks pass in `VER-WI-075-001`. `CTX-1.23` remains active and Phase 7.5 remains current. No next work item is authorized by this completion.
