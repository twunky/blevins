---
Status: ACTIVATED — historical candidate record
Authority: Historical candidate after activation; normative wording belongs to the canonical privacy contract
Context-Baseline: CTX-1.21
Version: 1.0-candidate
Decision: DEC-061
---

# Candidate CTX-1.22 — Creator-controlled Trip audience replacement

The project owner approved this bounded Trip audience-editing clarification on 2026-09-27 and authorized a context-only update with conditional activation after verification. It clarifies existing `REQ-PRIV-022` without a new requirement ID. `CTX-1.22` is active after passing `VER-CTX-1.22`; `CTX-1.21` is historical.

## Proposed canonical effect

- Only the creator can replace the whole audience of an existing Trip. The resulting mode is the Trip's existing applicable Family Group or Selected People from that group; switching in either direction is allowed.
- Group selection or refresh builds a fresh currently eligible active-adult recipient snapshot. Group mode has no individual additions, exclusions, partial snapshots, or hidden overrides.
- Selected People replaces the complete set with distinct, currently eligible active-adult members of the applicable group and at least one eligible non-creator recipient. The full proposal is validated and saved as one change or rejected unchanged.
- Creator access remains separate and cannot be removed or transferred. A creator who has lost eligibility to select current group members may make only permitted reductions using currently eligible people already represented in that Trip's saved audience. No directory or unrelated profile access is restored.
- The creator sees the proposed mode and gains/losses before confirmation. Server authority and eligibility are rechecked at commit; a stale preview cannot silently commit different access effects. Preview disclosure respects the departed-creator boundary.
- Removed recipients immediately lose Trip and normal Notes access while existing Notes remain intact. New recipients see only the Trip and visible ACTIVE Notes. Note mutations after committed revocation need current Trip access.
- Creator and non-creator Trip detail disclosure remains role-appropriate. No notification, Sharing Session, Check-In, location-consent, dedicated audience-history, or generalized ACL/audit behavior is introduced.

## Compatibility and authorization boundary

`REQ-TRIP-007/008/010`, `REQ-PRIV-011/020`, and `REQ-SHARE-009` remain substantively unchanged. Completed `WI-007-003` and `WI-007-004` remain valid and `DONE`; this candidate requires no completed-code correction. Plan dynamic whole-group behavior, Event access, Family/Profile permissions, and sharing consent remain unchanged. `WI-007-005` remains unapproved and unimplemented. This candidate authorizes no application code, migration, or Phase 7 feature execution.
