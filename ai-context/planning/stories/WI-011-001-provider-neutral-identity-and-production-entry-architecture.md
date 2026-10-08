# WI-011-001 — Provider-Neutral Identity and Production-Entry Architecture

## Metadata

- **Work Item ID:** `WI-011-001`
- **Status:** `APPROVED` — owner-approved architecture planning; governance canonicalization complete. This is not an implementation WI or a claim of production delivery.
- **Phase:** Phase 11 — Pilot & Release Readiness, FUTURE / planning only.
- **Context baseline:** `CTX-1.39` APPROVED / ACTIVE for the unchanged MVP. Future target: production-entry contract v1.1 under DEC-097/098, APPROVED / UNIMPLEMENTED; no successor CTX.
- **Requirement references:** REQ-PROD-015/016 and REQ-FAM-019–023, REQ-PRIV-035–037, REQ-UX-015/016 preserved; new architecture trace REQ-PROD-017–023 and REQ-UX-017.
- **Model tier:** `DEEP`

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| [AI operating rules](../../README.md) | Authority, future-target and work lifecycle rules | Entry |
| [Project status](../../project-status.md) | Current MVP/human-testing checkpoint | Entry |
| [DEC-097](../../decision-log.md#dec-097--approve-future-phase-11-production-entry-model) / [DEC-098](../../decision-log.md#dec-098--approve-provider-neutral-identity-and-production-entry-architecture) | Approved product model and explicit architecture/governance authorization | Before planning / verification |
| [Production-entry contract](../../core/production-entry-contract.md) | Single canonical future-target source | Before planning / verification |
| [Future requirements trace](../../core/requirements-index.md#future-phase-11-production-entry-dec-097) | Immutable IDs and separate target trace | Before planning / verification |
| [Phase 11 roadmap](../roadmap.md#phase-11--pilot--release-readiness) | Numbering, planned sequence and gates | Before planning / verification |
| [maintain-context skill](../../skills/maintain-context/SKILL.md) / [validation guidance](../../../claude-handoff/VALIDATION.md) | Canonical/reference synchronization and docs-only checks | Before canonicalization / verification |
| [CurrentAccount boundary](../../../src/FamilySafety.Application/Profiles/CurrentAccount.cs) | Confirm the existing request-user abstraction; read-only | If needed |

## Context intentionally not loaded

| Context file or area | Why it is excluded |
|---|---|
| Bootstrap/archive, old candidates/proposals and completed verification/evidence | Historical scope is preserved; no regression execution requested |
| Unrelated feature workflows, UI mockups and human-test fixtures | No feature/runtime or P1–P5 changes |
| Provider documentation/marketing, vendor configuration and package installation | No candidate evaluation/proof or selection is authorized |

## Goal

Canonicalize the owner's 23 approved identity/production-entry architecture directions as future Phase 11 planning, distinct from the current runtime baseline. Normative policy lives in the production-entry contract; this item records scope and verification.

## Boundaries / non-goals

- Documentation/governance only. No production authentication, schema, endpoints, API behavior, Angular changes, telemetry or provider installation/configuration.
- Preserve Development login/auth, current invitations, human-testing fixtures and P1–P5 readiness. No application tests, builds or browser validation.
- No provider proof is performed; no identity/email vendor selected; no Phase 11 implementation WI or slice authorized. No mobile design beyond the future token/local-Account boundary.
- Leave all seven bounded choices in the contract's [unresolved list](../../core/production-entry-contract.md#planning-and-unresolved-choices) open.
- Owner performs all Git commits. No staging, commit, push or PR.

## Acceptance checks

1. `AC-01` — All 23 owner-approved directions are canonical, with unique future-target requirement IDs and valid decision/index references.
2. `AC-02` — Topology, bootstrap, sessions/CSRF/logout, MFA/recent authentication, continuation/entry, provider proof and telemetry remain provider-neutral and application/domain ownership is explicit.
3. `AC-03` — All seven unresolved choices remain open; the ten-step implementation sequence is planning only, with no implementation WI or authorization.
4. `AC-04` — CTX-1.39, functional MVP completion, current Phase 10/WI-010-004 and closed Phase 10.5 remain unchanged; no runtime/fixtures/historical evidence change.
5. `AC-05` — Markdown/references, decision/requirement trace, current/future consistency, stale architecture searches and `git diff --check` pass using documentation checks only.

## Tasks

- [x] `TASK-001` — Confirm governance state, normal WI numbering and next decision ID.
- [x] `TASK-002` — Record DEC-098 and canonical future architecture/requirement trace.
- [x] `TASK-003` — Synchronize roadmap, status, overview and routing while preserving human-testing priority.
- [x] `TASK-004` — Complete documentation-only self-verification and record results below.

## Execution authorization

- **Approved work item:** `YES` — planning only, from the explicit owner architecture/governance request.
- **Execution authorized:** `YES` — documentation/governance canonicalization only; production implementation `NO`.
- **Authorized by:** Project owner, explicit approval of the 23 directions and canonicalization request, recorded as DEC-098; not DEC-032 auto-approval.
- **Authorized at:** 2026-10-06 (America/New_York).
- **Authorized scope:** Decision, future contract/index, planning item, roadmap and current-facing references. No provider proof/selection or implementation slice.

## Evidence / verification

Owner-direction trace (numbers refer to the explicit approval request):

| Owner directions | Canonical evidence |
|---|---|
| 1–2 browser topology / OIDC | [REQ-PROD-017](../../core/production-entry-contract.md#req-prod-017) |
| 3 session; 15 logout; 19 cookie/CSRF | [REQ-PROD-019](../../core/production-entry-contract.md#req-prod-019) |
| 4–8 Account, linking, bootstrap, adult eligibility, verified email | [REQ-PROD-018](../../core/production-entry-contract.md#req-prod-018) |
| 9–11 invitation continuation, zero/one/multi-Family entry, post-join | [REQ-UX-017](../../core/production-entry-contract.md#req-ux-017), preserving REQ-PRIV-035/036 |
| 12–14 creation assurance, MFA, recent authentication; 16 recovery | [REQ-PROD-020](../../core/production-entry-contract.md#req-prod-020), preserving REQ-FAM-019/023 |
| 17–18 API/Development auth; 20 provider-neutral boundary | [REQ-PROD-021](../../core/production-entry-contract.md#req-prod-021) |
| 21 provider proof gate | [REQ-PROD-022](../../core/production-entry-contract.md#req-prod-022) |
| 22 future native clients | [REQ-PROD-017](../../core/production-entry-contract.md#req-prod-017) |
| 23 telemetry | [REQ-PROD-023](../../core/production-entry-contract.md#req-prod-023) |

| Check | Evidence | Result |
|---|---|---|
| AC-01 / AC-02 | Manual review of all 23 directions against the trace above; eight new requirements, 20 unique canonical future definitions/index rows, 180 distinct requirement references and 98 decision references checked | PASS |
| AC-03 | All seven unresolved choices retained; ten roadmap stages in order; isolated ten-flow proof gate; only one Phase 11 planning item, no implementation authorization | PASS |
| AC-04 | Git diff confined to ten intended Markdown files; current checkpoint/open decisions, human-testing continuation, current MVP index rows and Phase 10/10.5 roadmap sections match HEAD; earlier decision-log text preserved | PASS |
| AC-05 | 216 local links/anchors resolved; Markdown fences/UTF-8 checked; no stale conditional PKCE or unresolved session-policy wording in future contract; `git diff --check` passed | PASS |

Checks performed 2026-10-06: documentation-only Node reference/consistency checker (`node "$env:TEMP\blevins-dec098-doc-check.cjs"` from the repository root); focused `rg` stale-architecture/current-state searches; `git diff --check`, `git diff --stat`, `git diff --numstat` and `git status --short`. Files use UTF-8/LF per `.editorconfig`; Git's existing `core.autocrlf=true` emits LF-to-CRLF notices, with no whitespace-check error. No .NET tests, Angular, Playwright or builds were run. Runtime, fixtures and human evidence remain unchanged.

- **Verification report:** This section and DEC-098's separate context verification; no application verification package.
- **Independence:** `SELF-VERIFIED`; no independent verifier or new human evidence.
- **Outcome:** `PASS` — documentation/governance only, SELF-VERIFIED. Architecture is owner approved; production behavior remains UNIMPLEMENTED. Work-item status remains APPROVED architecture planning, without a claim of production implementation or later owner acceptance of these checks.
