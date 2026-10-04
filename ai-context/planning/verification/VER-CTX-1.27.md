# VER-CTX-1.27 — Phase 9 Navigation and Item Links context update

## Metadata

- **Verification ID:** `VER-CTX-1.27`
- **Subject:** `DEC-074` — resolution of `OD-001`, the deep-link Family-context rule, and approval of `PRP-030`
- **Date:** `2026-10-02`
- **Verifier:** Agent preparing the context update
- **Independence:** `SELF-VERIFIED`
- **Context baseline during review:** `CTX-1.26` active; `CTX-1.27` candidate
- **Approval:** Explicit project-owner decisions D1–D4, 2026-10-02; `DEC-074`
- **Sources:** `core/ux-contract.md` (`REQ-UX-011`, `013`, `014`); `core/privacy-safety-contract.md` (`REQ-PRIV-025`, `026`, `027`); `core/product-contract.md#req-prod-007`; `core/requirements-index.md`; `decision-log.md` (`DEC-033`, `DEC-064`, `DEC-073`, `DEC-074`, open and resolved decisions); `planning/proposals/PRP-030-phase-9-scope-themes-and-exit-criteria.md`; `planning/candidates/CTX-1.27-phase-9-navigation-and-item-links.md`.

## Scope

Verify the canonical context update requested by the owner. This is context-only; application tests are not required and were not run.

## Evidence matrix

| # | Required check | Evidence | Result |
|---|---|---|---|
| 1 | Six-area navigation is canonical at desktop and narrow widths | `REQ-UX-013` lists Home, Family, Events, Plans & Trips, Safety, Me / Settings, in order, at both widths. | `PASS` |
| 2 | Navigation areas match the conceptual product areas | `REQ-UX-011` and `REQ-UX-013` match `REQ-PROD-007` exactly; `REQ-PROD-007` is unchanged. | `PASS` |
| 3 | Plans & Trips is a grouping only | `REQ-UX-013` keeps separate domains, routes, data, audiences, authorization, and lifecycle, and forbids merging screens, APIs, records, or rules. | `PASS` |
| 4 | Safety is a grouping only and grants no access | `REQ-UX-013` keeps Concerns and Incidents separate under `REQ-SAFE-015/016` and `REQ-PRIV-025/026`; reaching Safety grants no access and implies no existence. | `PASS` |
| 5 | Direct routes stay refresh-safe | `REQ-UX-013` (grouping) and `REQ-UX-014` (item URLs). | `PASS` |
| 6 | Validation standard recorded with all four parts | `REQ-UX-013`: owner visual review, desktop and narrow rendered evidence, keyboard/navigation review, automated reachability. Scanner supplements but does not replace manual review. | `PASS` |
| 7 | `OD-001` resolved and no longer described as open | `decision-log.md` lists `OD-001` under resolved decisions (`DEC-074`, `REQ-UX-013`). `REQ-UX-011` no longer says the grouping is unapproved. Search of `core/` and `workflows/` finds no remaining "`OD-001` remains open" statement. | `PASS` |
| 8 | Server authorization precedes any Family switch | `REQ-UX-014` steps 1–2; `REQ-PRIV-027` first and second bullets. | `PASS` |
| 9 | No switch from URL or client state alone; no Family disclosure before authorization | `REQ-UX-014` final paragraph; `REQ-PRIV-027` second bullet. | `PASS` |
| 10 | Membership is not read permission | `REQ-UX-014` final paragraph; `REQ-PRIV-027` first bullet; `REQ-UX-011` unchanged grants-no-access rule. | `PASS` |
| 11 | Unauthorized and nonexistent items are indistinguishable; Family unchanged | `REQ-UX-014` step 3; `REQ-PRIV-027` third bullet. Consistent with the existing Concern and Incident nondisclosure rules. | `PASS` |
| 12 | No new access path | `REQ-PRIV-027` fourth bullet; no audience, participant, membership, or administrator rule changed. | `PASS` |
| 13 | Existing multi-Family rules preserved | `REQ-UX-011` keeps sole-Family auto-selection, explicit multi-Family selection, no-Family path, session-only retention, and clear-and-reload; the deep-link switch reuses clear-and-reload. | `PASS` |
| 14 | `OD-002` remains open and outside Phase 9 | Open decisions table; `workflows/sharing-checkins.md` unchanged. | `PASS` |
| 15 | Requirement ID rules followed | New meaning uses new IDs (`REQ-UX-013`, `REQ-UX-014`, `REQ-PRIV-027`). `REQ-UX-011` is amended in place following the `DEC-064` and `DEC-071` precedent, and its index row records the amendment. No ID was renumbered or reused. | `PASS` |
| 16 | Index and contract headers consistent | Index status `CTX-1.27`; UX contract header `CTX-1.27`, version 1.1 (its header previously still showed `CTX-1.3` despite the `CTX-1.24` clarification; corrected); privacy contract header `CTX-1.27`, version 1.20. | `PASS` |
| 17 | No application code, test, or dependency changed | Changes are limited to `ai-context/` and `claude-handoff/`. `@axe-core/playwright` is approved but not installed. | `PASS` |
| 18 | Phase 9.5 addition did not cause this baseline | The candidate record states the baseline is driven by the canonical changes only; `DEC-073` needed none. | `PASS` |

## Findings

- `DEC-074` is the next decision after `DEC-073`; `CTX-1.27` is the next baseline after `CTX-1.26`; `REQ-UX-013`, `REQ-UX-014`, and `REQ-PRIV-027` were unused.
- No authoritative source conflicts with the new rules. `DEC-033` (directory excludes administrator assignments) is unaffected by this baseline but constrains PRP-030 item S3; that is recorded in PRP-030.
- Implementing `REQ-UX-014` for Events and Plans will need additive single-item server reads; this is a planning finding, not a context conflict.

## Limitations

- Self-verification by the agent that prepared the update.
- Application tests were not run because no application behavior or code changed.

## Outcome

- **Result:** `PASS`.
- **Completion authorized:** `YES` — activate `CTX-1.27`.
- **Reason:** all 18 checks pass and the update matches the owner's decisions without changing any existing access rule.
