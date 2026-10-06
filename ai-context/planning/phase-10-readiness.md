# Phase 10 — Testing & Verification / User-Testability Readiness

CURRENT / IN PROGRESS under DEC-093, 2026-10-05. Baseline CTX-1.37 remains APPROVED / ACTIVE. First item: [WI-010-001](stories/WI-010-001-post-9-5-user-testability-baseline.md) COMPLETE / PASS — OWNER APPROVED; [verification](verification/VER-WI-010-001.md) COMPLETE / PASS. Next work item: NONE AUTHORIZED. No second item authorized.

## Boundary and exits

Facilitated LOCAL DEVELOPMENT testing uses existing isolated scenarios, synthetic adult identities and data, and small adult sessions when human validation is appropriate. Evaluate implemented end-to-end continuity, discovery, understandable workflows, privacy-preserving transitions, mobile/narrow use, keyboard/accessibility and Family context. Correct only demonstrated blockers or misleading copy/navigation/loading/empty/error presentation within existing behavior. No child/minor profiles or real sensitive family data are needed.

Success requires understandable critical adult journeys, completion without unreasonable coaching, no remaining blocking functional defects in tested paths, no identified unintended privacy/access disclosure, clear unsupported/deferred behavior, and classification of remaining issues as blocking, bounded follow-up, deferred or production-readiness. Phase closure requires owner acceptance and actual human observations where appropriate; this initial baseline does not satisfy those exits by itself.

## Readiness matrix

READY FOR FACILITATED TEST means implemented behavior has reusable automated evidence and can be attempted locally; it is not human usability validation or production certification. Coverage below is retained from existing specs/work-item evidence, not a claim that every spec was rerun for Phase 10. All rows still need facilitated adult observations.

| Critical journey / implemented task | Classification | Existing coverage / session focus |
|---|---|---|
| Invitation acceptance → optional profile review → Home arrival | READY FOR FACILITATED TEST | invitation-handoff, first-use-arrival, phase9-journey; discover Development outbox/identity handoff, skip or save review |
| Family selection/context and authorized item transitions | READY FOR FACILITATED TEST | navigation, item-links, events-dashboard; switch Families without prior data or permission assumptions |
| Family Settings, relationships and administrator/member behavior | READY FOR FACILITATED TEST | family-dashboard; find settings, propose/respond, distinguish admin from private access |
| Household organization | READY FOR FACILITATED TEST | households; admin assign/move/unassign/delete, member read-only; organization grants no access |
| Shared Family Dates | READY FOR FACILITATED TEST | shared-family-dates; create/read/edit/delete with authorized visibility; birthday summary remains unsupported |
| Event discovery/detail → own RSVP | READY FOR FACILITATED TEST | events-dashboard, event-invitations, home-pending-rsvp; discover Pending, respond, return Home |
| Event Needs and Event Updates | READY FOR FACILITATED TEST | event-needs, event-updates; organizer versus viewer actions, claim/release, visible Updates only |
| Plans: creation/audience/detail and creator note edit/clear | READY FOR FACILITATED TEST | phase9-journey, item-links, phase9-home-filtering; expectation is recorded timing only, no scheduled Check-In/reminder |
| Trips: creation/audience/detail/Notes and creator audience changes | READY FOR FACILITATED TEST | phase9-journey, item-links, trip-audience-removal; traveler status grants no access |
| Standalone Check-In: explicit recipients, sent/shared history | READY FOR FACILITATED TEST | standalone-check-ins; find Safety, distinguish author report from verified safety and Family-wide feed |
| Safety Circle preset → review explicit Check-In/Sharing recipients | READY FOR FACILITATED TEST | safety-circles; preset supplies choices, never inherited access |
| Trusted Contacts: private manual owner records | READY FOR FACILITATED TEST | trusted-contacts; personal across Families, no linked identity or automatic outreach |
| Temporary Sharing: explicit recipients, chosen end, permission denied/unavailable, latest point, Home indication, stop/expiry | READY FOR FACILITATED TEST | temporary-sharing-sessions; mocked geolocation browser evidence, SQL/component stop/expiry evidence in VER-WI-095-006; inspect freshness and active-UI limits |
| Concern: explicit participants, contributions/corrections/challenges, resolution | READY FOR FACILITATED TEST | concern-workflow, item-links; no automatic subject access or danger inference |
| Incident: explicit access, contributions/tasks/handoff/resolution, manual start from Concern | READY FOR FACILITATED TEST | incident-workflow, item-links; no automatic escalation or permission/content transfer |
| Desktop/narrow, keyboard and navigation accessibility across journeys | READY FOR FACILITATED TEST | navigation, phase9-accessibility and domain specs; prior evidence plus focused Phase 10 changed-path checks; observe real coaching and navigation confusion |
| Universal Home attention, Family activity/Check-In/birthday summaries, automated Plan Check-In/reminders, general Plan/Trip/Event completion | DEFERRED BY DESIGN | DEC-092 / REQ-PROD-014 and existing implementation boundaries; do not imply these exist |
| Real signup/sign-in/recovery, production invitations, hosting/provisioning, backup/restore readiness, physical-device location | PRODUCTION DEPENDENCY | Later production/pilot work; does not block facilitated local testing |

No blocking defect is established by this initial coverage inspection; that is not proof every critical path is defect-free. Newly demonstrated issues should change the affected row to BLOCKED BY DEFECT or NEEDS CLARIFICATION, with a brief reason and proposed bounded follow-up. New product behavior/privacy policy requires separate owner scope.

## Facilitated adult sessions (prepared, not performed)

Use [local scenario instructions](../../docs/local-development.md#invited-adult-scenario-wi-075-001), an exact isolated scenario name, localhost:4200 and the existing Development identity/outbox controls. Facilitate identity switching explicitly; do not present it as production sign-in. Invite another synthetic adult into the scenario before recipient/audience tasks. A separate multi-Family session can use the existing script's `-IncludeTripAudienceAdult -IncludeSecondFamily` switches (the latter seeds B as a member of the second Family, so use the default scenario for the no-Family invitation entry task). The automated equivalent is the fixture's `tripAudienceAdult` / `secondFamily` options. Keep scenario names and harness ownership manifests; use existing guarded cleanup only for exact owned artifacts. Manual scenario databases are retained, not automatically eligible for harness deletion.

Give task goals before navigation hints: join and reach Home; choose a Family/find settings and dates; respond to an Event and coordinate Needs/Updates; share a Plan/Trip; send a Check-In using a reviewed preset; manage a private contact; start/stop temporary sharing and explain who can see it; record and resolve a Concern/Incident. Use short groups of tasks across sessions, including narrow width and keyboard paths. Ask participants to explain audience, Family context, expected Check-In timing and location freshness/expiry in their own words. Synthetic or mocked browser location is sufficient here; device permission behavior remains a later physical-device check.

Record only task discovery, completion, misunderstandings, navigation confusion, privacy expectations and coaching required, plus issue classification (blocking / bounded follow-up / deferred / production-readiness). No participants, timings, success rates or human results are claimed now. A future separately authorized item may record owner/participant observations and decide bounded fixes.
