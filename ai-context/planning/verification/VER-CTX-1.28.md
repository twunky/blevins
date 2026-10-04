# VER-CTX-1.28 — Item-Link Family Context Clarification

## Metadata

- **Verification ID:** `VER-CTX-1.28`
- **Subject:** `DEC-075` — the owner's resolution of `OD-A` to `OD-I` for `WI-009-005`, and the `REQ-UX-014` amendment
- **Date:** `2026-10-02`
- **Verifier:** Agent preparing the context update
- **Independence:** `SELF-VERIFIED`
- **Context baseline during review:** `CTX-1.27` active; `CTX-1.28` candidate
- **Approval:** Explicit project-owner decisions OD-A to OD-I, 2026-10-02; `DEC-075`
- **Sources:**
  - `core/ux-contract.md` (`REQ-UX-011`, `REQ-UX-013`, `REQ-UX-014`);
  - `core/privacy-safety-contract.md` (`REQ-PRIV-022`, `REQ-PRIV-025`, `REQ-PRIV-026`, `REQ-PRIV-027`);
  - `core/requirements-index.md`;
  - `decision-log.md` (`DEC-064`, `DEC-074`, `DEC-075`);
  - `planning/candidates/CTX-1.28-item-link-family-context-clarification.md`;
  - `planning/stories/WI-009-005-refresh-safe-item-urls-and-authorize-first-deep-links.md`.

## Scope

Verify the canonical context update required by the owner's decisions. This is context-only, so no application tests were run.

## Evidence matrix

| # | Required check | Evidence | Result |
|---|---|---|---|
| 1 | A baseline is needed only because canonical text changes | The previous `REQ-UX-014` step 2 required a switch for every authorized item in a different Family. OD-A forbids that for a Trip whose Family is not selectable, and OD-B adds a new fail-safe rule. The candidate record states that `WI-009-005`'s existence is not the reason. | `PASS` |
| 2 | Server item authorization always comes first | `REQ-UX-014` step 1 is unchanged; step 2 still begins "Only after the server authorizes the read". | `PASS` |
| 3 | A selectable Family switches after authorization when needed | `REQ-UX-014` step 2, "Selectable Family", keeps the switch and the clear-and-reload rule of `REQ-UX-011`. | `PASS` |
| 4 | No fabricated Family context | Step 2, "No selectable Family": the application "never creates, guesses, or selects a Family context". | `PASS` |
| 5 | Rendering without a switch only where domain rules explicitly allow it; currently only the Trip creator | Step 2 names `REQ-PRIV-022` creator retention as the only current case. Events, Plans, Concerns, and Incidents get the unavailable state. `REQ-PRIV-022` (privacy contract) states that the creator retains Trip access independently of group membership. `REQ-PRIV-026` binds Incident grants to current membership; Event and Plan access require active membership (`REQ-PRIV-021`, `REQ-PLAN-005`); `REQ-PRIV-025` ties Concern access to active membership. | `PASS` |
| 6 | One-time Family-list refresh, no repeated retry (OD-B) | Step 2, "List possibly stale": refresh once; "It does not retry further." | `PASS` |
| 7 | Unavailable items never switch or announce | Step 3, final sentence. | `PASS` |
| 8 | Announcement only on an actual switch, with the name from the caller's own list (OD-H) | Step 2, "Selectable Family": polite status message, name from the user's own list, no announcement when the Family does not change. Consistent with `REQ-PRIV-027` (no Family name before authorization). | `PASS` |
| 9 | No new access path | The final paragraph says the client's Family list is not permission and that the no-selectable-Family rule adds no access. No access rule in `REQ-PRIV-020` to `REQ-PRIV-027` or the workflows changed. | `PASS` |
| 10 | `REQ-PRIV-027` is consistent without change | It requires authorization before disclosure, no switch from the URL or client state, and identical missing and unauthorized results. It never required a switch for every authorized item. | `PASS` |
| 11 | `REQ-UX-011` is consistent without change | The item-link rule remains the only other way the Family may change. The OD-A case changes nothing, which is compatible. | `PASS` |
| 12 | No remaining canonical text requires an unconditional switch | Search of `core/` and `workflows/` for switch rules finds only the amended `REQ-UX-014`. | `PASS` |
| 13 | Requirement ID rules | Amended in place: same subject, a clarification following the `DEC-064` and `DEC-074` precedent. The index row records "amended `CTX-1.28`". No ID was renumbered or reused. | `PASS` |
| 14 | Headers and index consistent | Index status `CTX-1.28`; UX contract header `CTX-1.28`, version 1.2, source adds `DEC-075`. The privacy contract is unchanged and stays at `CTX-1.27`, version 1.20. | `PASS` |
| 15 | OD-C to OD-I need no canonical change | They are implementation, scope, fixture, or presentation choices within the existing requirements. They are recorded in `DEC-075` and `WI-009-005`. | `PASS` |
| 16 | No application code, test, fixture, or dependency changed | Changes are limited to `ai-context/` and `claude-handoff/`. | `PASS` |

## Findings

- `DEC-075` is the next decision after `DEC-074`, and `CTX-1.28` is the next baseline after `CTX-1.27`.
- No authoritative source conflicts with the amended rule.
- The amended step 2 is general ("where that domain's canonical access rules explicitly allow…"). A future domain rule permitting access without membership would apply only through its own canonical decision.

## Limitations

- Self-verification by the agent that prepared the update.
- No application tests were run, because no application behavior or code changed.

## Outcome

- **Result:** `PASS`.
- **Completion authorized:** `YES` — activate `CTX-1.28`.
- **Reason:** all 16 checks pass. The update matches the owner's decisions without changing any access rule.
