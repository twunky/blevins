---
Status: ACTIVATED — historical candidate record
Authority: Historical candidate; active wording is in the referenced canonical contracts and glossary
Context-Baseline: CTX-1.19
Version: 1.0-candidate
Decision: DEC-059
---

# Candidate CTX-1.20 — Phase 5 Family and Profiles reconciliation

The project owner approved this bounded context update on 2026-09-27, and it was activated after passing `VER-CTX-1.20`. It clarifies previously approved Trusted Contact eligibility and privacy boundaries, corrects stale Shared Family Date wording, and explains Phase 5 closure scope. It authorizes no application code or implementation work item.

## Canonical changes

- `REQ-FAM-010` now points to `REQ-FAM-011` through `REQ-FAM-013` for the required title, required description, and one specific year-bearing date without automatic recurrence. Stale Shared Family Date summaries in the product-contract reference section and glossary are corrected to match the already approved fields, date semantics, and 30-day retention period. Shared Family Date behavior does not change.
- `REQ-FAM-014` retains adult ownership, default privacy, and separation from profile contact fields and Safety Circle. Its stale candidate label and nonmember uncertainty are removed. The misplaced Shared Family Date paragraph is removed.
- New `REQ-FAM-015` permits a Trusted Contact outside the Family Group without placing that person in Family membership or the directory. Account expectations and implementation details remain undecided.
- New `REQ-PRIV-024` states that Trusted Contact designation alone grants no Family Group, Event, Trip, Sharing Session, or safety-incident access and no automatic Safety Circle membership.
- The domain glossary distinguishes Trusted Contact from Safety Circle without adding behavior.

## Closure and compatibility

Phase 5 remains `COMPLETE` under `DEC-050` for the four approved implementation slices, `WI-005-001` through `WI-005-004`, all `DONE`. Remaining Family MVP capabilities need separate scope and authorization. CTX-1.19 Event rules, Plan audiences and Notes, Trip audiences and Notes, and Sharing Session consent rules are unchanged.

## Explicitly deferred

Trusted Contact fields, Account expectations, invitation or acceptance, notification consent, audiences, uses, retention, Safety Circle integration, safety-incident access, UI, and v1 implementation shape; Household and Safety Circle management; membership removal/rejoin and administrator transfer; future retention and minor/guardian policy. No implementation is authorized.
