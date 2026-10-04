# WI-095-001 rendered and accessibility review

2026-10-04; SELF-VERIFIED by the implementing agent. Owner visual approval remains pending. These are development scenario captures, with the expected test-identity banner. No production rendering/deployment claim.

29 images copied without overwriting prior evidence from `src/FamilySafety.Web/e2e/captures/event-invitations/`; [SHA-256 manifest](screenshot-sha256.txt). Filenames ending 1440/390 indicate viewport width. Fourteen state pairs were scanned with axe (28 scans, zero violations, no suppression) and checked for horizontal overflow. The held loading read has a separate 390 px capture and was not axe-scanned while blocked.

## Evidence and provenance

| State | Captures / provenance |
|---|---|
| Organizer empty, eligible choices | screenshots/organizer-empty-{1440,390}.png; real API |
| Organizer Pending and Going | screenshots/organizer-pending-{1440,390}.png; organizer-going-{1440,390}.png; real API |
| Organizer revoke confirmation / retained state | screenshots/revoke-confirmation-{1440,390}.png; organizer-revoked-{1440,390}.png; real API |
| Own Pending dashboard | screenshots/pending-dashboard-{1440,390}.png; real API; past Event date |
| Invitee Pending / Going | screenshots/invitee-pending-{1440,390}.png; invitee-going-{1440,390}.png; real API |
| Invitee revoked / no action | screenshots/invitee-revoked-{1440,390}.png; real API, Event access retained |
| Editor without invitation | screenshots/editor-no-invitation-{1440,390}.png; real API; existing detail edit still available |
| Unauthorized Event | screenshots/unauthorized-event-{1440,390}.png; real API; generic unavailable, unchanged Family, no component |
| Organizer / invitee read failure | screenshots/organizer-read-error-{1440,390}.png; invitee-read-error-{1440,390}.png; intercepted 500 read only, subsequent Retry uses real API |
| Invitee loading | screenshots/invitee-loading-390.png; explicitly held/intercepted protected read |
| Invitee conflict | screenshots/invitee-conflict-{1440,390}.png; real 409 after a separate real API response advanced Version; Refresh restores current Maybe |

## Visual comparison

Primary dashboard target: `claude-handoff/reference/panel3-travel-events.png`, under DEC-070/078 and DEC-083's bounded participation additions. Event-detail participation has no primary mockup; it uses the established shared visual system. There is no narrow reference image; 390 px is derived from desktop composition and canonical accessibility rules.

| State | Matches / adaptation | Difference and reason |
|---|---|---|
| Events own-Pending dashboard | Existing blue shell, white rounded cards, four-card arrangement, icon/heading rhythm, compact named/date rows and an actionable Event link. The formerly empty RSVP area now displays real own-Pending data. | Event name links to the existing detail route rather than a separate Respond workflow (REQ-EVT-014/UX-014). No photos, location metadata or public RSVP badges; no source photo data and actor-specific privacy. |
| Organizer detail | Same card, type, row, button, border and focus language; clear eligible viewer and retained invitation sections. | No primary detail reference. Separate participation card preserves App/detail/edit ownership (DEC-083). Active/Revoked and current RSVP are visible only to organizer (REQ-PRIV-028). |
| Invitee detail | Clear heading, current RSVP, four labeled radio choices in a fieldset, primary save button and calm success status. Mobile radio labels wrap without overflow. | No primary detail reference. Pending has explicit initial-no-response explanation; deliberate submission controls the timestamp (REQ-EVT-014). No organizer proxy controls. |
| Confirmation / revoked | Inline consequential-action confirmation matches existing local confirmation pattern. Final RSVP stays as textual history; re-invite is a new action. | No public history or automatic expiry. Revocation does not remove Event visibility (REQ-EVT-014). |
| Editor / unavailable | Existing Event editing remains independently available. No named participation data/actions for an uninvited editor. Unauthorized Event uses existing generic unavailable surface. | Canonical privacy takes precedence over mockup-only universal status/participation (REQ-PRIV-027/028). |
| Loading, read failure, conflict | Shared status/error/secondary Retry styling, readable mobile wrapping and clear distinct states. | Read failures are explicitly mocked. Conflict is real and disables writes until authorized Refresh; no mutation replay (AC-18/19). |

The dashboard is materially closer to panel3 in the RSVP region: real invitation rows replace the placeholder while existing page composition remains. Three largest retained gaps: no photos/thumbnails beyond existing date tiles; no global search/bell/notification UI; Quick Links use existing authorized routes rather than unbuilt calendar/lists/documents. These are approved scope/data constraints, not defects to expand in this work item.

## Accessibility inspection

- Existing single page h1 and Event h2 retained; participation h2 with organizer subsections h3. Semantic section naming and own-RSVP fieldset/legend used.
- Native radio labels, text-only status meaning, real buttons/links and shared visible focus styling; role=status for loading/success and role=alert for errors.
- Browser exercised Enter for Invite and confirmed revocation, Space for Going, then Enter for RSVP submission. This is actual keyboard operation of new controls; it is not claimed as a human keyboard-only traversal/usability study.
- Axe found zero violations in the 28 paired state scans, including the full page/shared shell. No exclusions/suppressions added. Overflow checks passed at 1440 and 390 px.
- Agent inspected organizer/read-error/confirmation/history, own Pending/conflict, editor/no-invitation, dashboard and unavailable captures. Owner visual review remains the required final human gate; no approval inferred from these automated or agent checks.
