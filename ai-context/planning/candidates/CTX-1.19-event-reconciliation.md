---
Status: ACTIVATED — historical candidate record
Authority: Historical candidate; active wording is in the referenced canonical contracts and workflow
Context-Baseline: CTX-1.18
Version: 1.0-candidate
Decision: DEC-058
---

# Candidate CTX-1.19 — Event reconciliation

This historical candidate records the project owner's approved Event MVP direction, activated as `CTX-1.19` after passing `VER-CTX-1.19`. It preserves `DEC-051`, `DEC-052`, `WI-006-001`, `WI-006-002`, the implemented selected-person audience, and the five-field Event detail-editor permission. It authorizes no application code, implementation work item, or Plan/Trip change.

## Event workflow refinement

- `REQ-EVT-001` through `REQ-EVT-005` and `REQ-EVT-007` are clarified without superseding their approved MVP capabilities.
- `REQ-EVT-009` makes the creator the sole MVP organizer and keeps designated detail editors separate from other Event capabilities.
- `REQ-EVT-010` separates Event audience, invitation, participant, and RSVP. An invitation targets only an already authorized viewer, remains active until revoked, and never grants visibility. Revocation leaves visibility intact while making the invitation and its RSVP inactive/historical. Active invited in-system users control Pending, Going, Maybe, or Not Going responses. Event-completion handling remains deferred.
- `REQ-EVT-011` defines one lightweight Need model, Open/Claimed/Assigned/Completed states, consent-based assignment, one responsible person, eligible invitation/RSVP boundaries, and automatic return to Open after Not Going or eligibility loss with prior responsibility preserved. Regaining access does not restore responsibility.
- `REQ-EVT-012` limits meaningful Updates to the organizer. Edits retain prior text, hidden Updates and revisions remain with the Event while it exists, and normal viewers receive neither hidden Updates nor old revisions. Visible Updates follow current Event access; no history or restore UI is required.
- `REQ-EVT-013` reuses generic Check-Ins and preserves restrained Home/notification direction without authorizing specific triggers or Event Sharing Session implementation.

## Privacy boundary

- `REQ-PRIV-023` prevents Event roles, invitation, RSVP, and Need responsibility from granting unrelated private-data, location, or Sharing Session access. It bars hidden Update/history exposure through normal Event access and creates no Plan or Trip rule.

## Boundaries

- No optional end date/time, group or combined Event audience, multiple organizers, non-system RSVP, Event Sharing Session implementation, Event-specific Check-In states, notification triggers, mockup layout, or broader retention/deletion policy is added.
- Phase 6 remains complete only for its two approved implementation slices; invitations/RSVP, Needs, and Updates remain unimplemented MVP capabilities requiring separately authorized work.
