# WI-009-011 — Phase 9 Exit Criteria Verification

## Metadata

- **Work Item ID:** `WI-009-011`
- **Status:** `DONE / PASS — OWNER APPROVED` — explicit conditional closure satisfied after all gates passed (2026-10-03; DEC-081).
- **Phase:** 9 — Integration & UX Refinement
- **Context baseline:** `CTX-1.28`, unchanged
- **Model tier:** `DEEP`
- **Requirement references:** `REQ-UX-002`, `REQ-UX-003`, `REQ-UX-005`, `REQ-UX-006`, `REQ-UX-011`, `REQ-UX-013`, `REQ-UX-014`, `REQ-PRIV-027`, `REQ-PLAN-001`–`006`, `REQ-TRIP-008`, `REQ-TRIP-011`. UX-005 is the freshness anchor; UX-003 is progressive disclosure.

## Required context manifest

| Context | Why | Timing |
|---|---|---|
| `ai-context/README.md`, `project-status.md` | Operating authority and current state | Entry |
| `planning/proposals/PRP-030-phase-9-scope-themes-and-exit-criteria.md`, `planning/roadmap.md` | M1–M10 and 15 phase exits | Before execution/verification |
| `decision-log.md`, DEC-074–080 | Approved navigation, item access, dashboard and first-use supersessions | Before execution |
| `core/ux-contract.md`, `core/privacy-safety-contract.md` (listed anchors) | Freshness, accessibility, context and access boundaries | Before execution |
| `workflows/plans-trips.md`, existing Plan/Trip DTOs | Determine filtering from existing timing semantics | Before D2 |
| WI-009-005–010 and their VER records | Preserve approved behavior and use existing evidence | Before verification |
| `claude-handoff/WORKING-RULES.md`, `VALIDATION.md`, work-item/verification templates | Scope, test order, artifact safeguards and reporting | Before execution |

## Goal

Complete the bounded remaining Phase 9 outcome and verification gaps, with an explicit M1–M10 and exit-criteria reconciliation in `VER-PH-009`.

## Owner decisions and execution authorization (2026-10-03)

The owner's supplied request explicitly approves creation and completion of this item, including conditional phase closure after all exits pass. `DEC-032` standing approval is not used.

- **D1:** M5 is satisfied by Home's lightweight Safety navigation and the authorized `/safety` dashboard. This is an explicit owner-approved outcome substitution; do not move summaries to Home.
- **D2:** M6 is not waived. Owner clarification approves `(ExpectedReturnDate ?? StartDate) >= local today` at calendar-date level. Keep overnight Plans through expected-return day; exclude past-start Plans without a return. Filter before cap, preserve Family/authorization/start-date-time sorting/links, and never infer completion. Trip end-date freshness and Event filtering remain unchanged. No API/schema/domain change.
- **D3:** File the product-wide consistency/no-dead-end checklist. Do not change accepted cosmetic differences. Report additional product findings before implementing them.
- **D4:** Add one focused continuous invited-adult cross-feature E2E journey using the existing scenario-isolation mechanism; avoid duplicating the whole regression suite.
- **D5:** Add the PRP-030-approved `@axe-core/playwright` development dependency and primary-route checks, with no serious/critical suppression. Perform and record a keyboard-only walkthrough. Straightforward accessibility defects may be fixed; broad redesign/product changes require reporting first.
- **Closure:** only after verification passes, record the owner closure decision, close Phase 9 and this item, synchronize current-state records, retain CTX-1.28, and leave no implementation authorized.

## Boundaries / non-goals

No dashboard redesign, API/schema change, new lifecycle state, budget increase, production identity/hosting, backup/restore certification, or Phase 9.5 capability. Preserve Event audience requirements. Leave the Plan-create Location header unchanged as **IMPORTANT FOLLOW-UP — NOT A PHASE 9 BLOCKER**. Do not touch protected/unknown databases, historical records, root `trips`, revoked WI-009-010 invitations, or prior canonical evidence. No commit/push.

## Acceptance checks

1. **AC-01:** D1 substitution is explicit in phase evidence and closure decision.
2. **AC-02:** D2 filtering uses approved existing semantics; current/upcoming, past/ended, current-Family isolation, row limits, links and empty states have focused tests and rendered evidence.
3. **AC-03:** Product-wide D3 checklist covers all owner-listed concerns, with actual findings and accepted differences distinguished.
4. **AC-04:** Scenario-isolated D4 continuous invited-adult journey passes using in-app navigation through all six areas and an authorized refresh-safe item.
5. **AC-05:** D5 axe checks cover all 13 named routes with zero serious/critical violations, and all other findings are classified.
6. **AC-06:** Keyboard-only manual journey records actual observed navigation, selector, invitation, settings, creation, confirmation, item and Back interactions, visible/logical focus and route focus; no pointer input is needed.
7. **AC-07:** Focused verification, affected regressions and full Angular pass; one final production build passes with unchanged budgets and corrected bundle terminology.
8. **AC-08:** Exactly one final full Playwright run passes; exact owned-artifact cleanup and final health are documented.
9. **AC-09:** `VER-PH-009` maps M1–M10 and each of the 15 exits to the owner's permitted result classifications; work-item VER links to it.
10. **AC-10:** Only after all gates pass, closure DEC and synchronized records show Phase 9 closed, WI-009-011 owner-approved, CTX-1.28 active, Development testing ready, Phase 9.5 READY TO BEGIN (latest owner instruction) and no implementation work item authorized.

## Tasks

- [x] TASK-001 Read governing context, record authority and inspect timing semantics.
- [x] TASK-002 Resolve Plan cutoff ambiguity; implement minimal approved Home filtering and focused component coverage.
- [x] TASK-003 Add development-only axe route checks; classify/fix permitted accessibility findings.
- [x] TASK-004 Add continuous cross-feature journey and render Trip filtering evidence (Plan renders remain in TASK-002).
- [x] TASK-005 Perform keyboard-only walkthrough and file consistency checklist.
- [x] TASK-006 Run affected checks, full Angular, one final build, exactly one final full Playwright; record artifact health.
- [x] TASK-007 Verify exits and complete conditional owner-approved closure records.

## Evidence / verification

- **Report:** `planning/verification/VER-WI-009-011.md` — DONE / PASS, linking to `VER-PH-009.md` — PASS / PHASE CLOSURE VERIFIED. All ten acceptance criteria, M1–M10 and all 15 exits pass.
- **Independence:** `SELF-VERIFIED`; owner execution and conditional closure authorization are explicit, not an inferred independent review.
- **Outcome:** D1–D5 pass. Plan expected-return/start fallback passes Home 25/25 (eight new cases), focused Plan browser 1/1 and six desktop/mobile renders. Prior affected Angular/E2E and full Angular 271/271 retained per owner instruction. One final production build PASS: main 597.77 kB, complete initial 610.24 kB, estimated transfer 134.28 kB, unchanged budgets. Sole final full Playwright PASS 34/34, 0 failed/skipped, 13/13 DBs removed. Final axe 30 scans zero violations/incomplete; actual keyboard evidence retained. Across the story all 24 owned DBs removed, original 18-name baseline unchanged, exact folder cleanup complete, both ports free and no API process. DEC-081 closes Phase 9; CTX-1.28 ACTIVE, Development READY, Phase 9.5 READY TO BEGIN, no implementation item authorized. No out-of-scope feature or commit/push.
