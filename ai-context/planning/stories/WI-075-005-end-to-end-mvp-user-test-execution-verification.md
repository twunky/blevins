---
Status: DONE — OWNER/DEVELOPER-SUPPLIED HUMAN VALIDATION; DOCUMENTED EVIDENCE LIMITATIONS
Authority: Owner-approved PRP-026; Stage 1 execution authorization and owner acceptance of supplied human-validation evidence recorded in VER-WI-075-005
Context-Baseline: CTX-1.24
Version: 1.2
---

# WI-075-005 — End-to-End MVP User-Test Execution & Verification

## Metadata

- **Work Item ID:** `WI-075-005`
- **Status:** `DONE — PASS WITH OWNER-ACCEPTED EVIDENCE LIMITATIONS`
- **Phase:** `7.5 — End-to-End UX & User-Test Readiness`
- **Context baseline:** `CTX-1.24`
- **Proposal:** `PRP-026 — End-to-End MVP User-Test Scenario`, approved by the project owner; proposal approval and scope are recorded in the project-owner session.
- **Execution authorization:** `YES — Stage 1 technical preflight was separately authorized. The owner/developer reports that manual workflow validation was completed outside Codex observation; the current owner request authorizes documentation and closure assessment only.`
- **Work type:** Test / verification; no feature development.

## Objective

Determine whether two adults can use the current Development build through invitation, Family, Event, Plan, Trip, Trip Notes, and Home journeys without developer coaching. Run a clean technical preflight first, execute the approved human scenario only if preflight passes, capture lightweight evidence, classify findings, and assess Phase 7.5 exit criteria. This work item does not authorize fixes or phase closure.

## Required context manifest

| Context file or anchor | Why it is needed | Load timing |
|---|---|---|
| `ai-context/README.md`; `ai-context/project-status.md` | Work mode, operating authority, current phase and baseline | Entry |
| `ai-context/planning/roadmap.md#phase-75--end-to-end-ux--user-test-readiness`; approved `PRP-026` and its approval record | Phase 7.5 objective/exit criteria and bounded user-test scope | Before execution |
| `ai-context/planning/stories/WI-075-001-invited-adult-first-use-journey.md` through `WI-075-004-mvp-visual-design-interaction-refinement.md` and matching `VER-WI-075-001.md` through `VER-WI-075-004.md` | Current implemented behavior, acceptance boundaries, verification evidence, and known limitations | Before preflight and test |
| `docs/local-development.md`; `scripts/start-invitation-scenario.ps1` | Reproducible dedicated Development database, identities, invitation outbox, reset and startup behavior | Before preflight |
| `ai-context/workflows/events.md#req-evt-008`; `ai-context/workflows/plans-trips.md#req-plan-005`, `#req-plan-006`, `#req-trip-007`, `#req-trip-008`, `#req-trip-010`; relevant `REQ-PRIV-020/021/022` anchors | Existing Event edit, Plan audience/Notes, Trip audience/Notes, and item access expectations under test | Before test and verification |

## Context intentionally excluded

| Area | Why excluded |
|---|---|
| Safety, Sharing Sessions, Check-Ins, notifications, location sharing, and deferred Event functions | Explicitly excluded from required test success; no implementation or behavior change is authorized |
| Production authentication, registration, email delivery, hosting, and deployment | Development identities/outbox are test-only; these production choices remain outside scope |
| `OD-001` and final mobile navigation design | Remain open; this work checks usability at a narrow viewport without deciding navigation grouping |
| Bootstrap, archive, and unrelated historical verification | Current approved contracts, stories, reports, and Development instructions suffice |

## Preconditions and starting state

Before inviting human testers, reconfirm `CTX-1.24` is active, Phase 7.5 is `CURRENT`, `WI-075-001` through `WI-075-004` are `DONE` with passing verification, and `OD-001` remains open. Use the Development-only identity tooling and a **fresh dedicated invitation scenario database** created by `scripts/start-invitation-scenario.ps1`.

The starting state must have:

- Adult A as an existing Account/Profile, active Family member, and Family administrator.
- Adult B as an existing Account/Profile with no current or former membership in that Family.
- No Events, Plans, or Trips.

Do not use the ordinary `FamilySafety` database. Do not reuse a scenario in which B already accepted an invitation. The scenario script retains its uniquely named databases and does not drop them; use a fresh name/run for each clean session.

## Technical preflight

Run one clean end-to-end preflight against a fresh scenario database before human testing. Confirm:

1. The Development API and app start successfully using the documented ports and URL.
2. A is identified as the active Family administrator.
3. A can invite B and the Development invitation link appears in the in-memory outbox.
4. Switching to B visibly identifies B, clears A's prior account context, and B begins with no active Family.
5. B can open the invitation and understand the Family and inviter preview.
6. B can accept; membership refreshes and Family context loads in the same session.
7. Optional profile review can be saved or skipped and B reaches Home.
8. No stale A data, route dead end, or blocking error appears during this journey.

If any preflight check fails, stop before human testing. Preserve concise evidence, classify the finding, and report it for separate follow-up. Do not fix it in this work item.

## Human test personas and journey

Use two adult testers with the Development identities only:

- **Adult A:** existing Account/Profile; active Family member and administrator.
- **Adult B:** existing Account/Profile; initially outside the Family; invited by A.

After a passing preflight, conduct the PRP-026 scenario:

1. A invites B; B opens and accepts the invitation, then reaches Family and Home.
2. B views the Family directory and suggests a relationship with A; A reviews and accepts it.
3. A creates an Event visible and editable by B. B finds it, makes a currently permitted detail edit, and A confirms the change.
4. A creates a whole-Family Plan with a note. B finds and reads it, then explains who they believe can see it.
5. A creates a Trip shared with B. B opens it and adds a Trip Note. A sees the Note with its author.
6. A changes the Trip audience to exclude B and reviews the access-change preview before confirming. B loses access; A retains creator access.
7. Both return Home, assess the Family context and visible Event/Plan/Trip previews, and navigate back to relevant areas without coaching.

Do not treat Event RSVP, Needs, Updates, notifications, Check-Ins, Sharing Sessions, location sharing, Safety, production authentication, live production email, broader Family administration, or a final mobile navigation decision as required behavior. Record questions or assumptions about them as observations, not automatic defects.

## Tester-facing task script

Present these tasks one at a time, using the assigned Development identity for each task. Tell participants this is a Development build and the accounts are test identities. The Development identity switcher and invitation outbox are test scaffolding; do not ask participants to understand or judge that infrastructure. Do not explain which button to press, where a feature lives, or the exact navigation path unless intervention is required. Speaking aloud is optional. Do not repeat technical checks already covered by Playwright, Angular/Vitest, .NET/xUnit, or the technical Stage 1 evidence.

1. **A:** “You’re already part of this Family and help administer it. Invite the other adult to join.”
2. **B:** “Your family has invited you to join. Open the invitation and join the Family.”
3. **B:** “Take a look around the Family and suggest how you’re related to A.”
4. **A:** “Review B’s relationship suggestion and respond.”
5. **A:** “Create an Event that B can see and update. Later, check whether B’s change appears for you.”
6. **B:** “Find the Event and make a small change that seems appropriate.”
7. **A:** “Create a Plan for the Family and include a useful note.”
8. **B:** “Find the Plan and tell us who you think can see it.”
9. **A:** “Create a Trip for you and B to share.”
10. **B:** “Add a useful note to the Trip.”
11. **A:** “Change who can see the Trip. Review the change before confirming.”
12. **Both:** “Return to Home. Tell us what the app shows and where you would go next.”

## Facilitator instructions

- Use the passing Stage 1 scenario and the existing Development identities. Keep A and B sessions ready in separate browser profiles/windows or another clear arrangement that avoids signing one identity over the other. Set the desktop viewport to approximately 1440 px.
- Before each prompt, make the correct identity/session available without explaining Development identity infrastructure to participants. Confirm the assigned identity privately as facilitator.
- Give one task prompt at a time. Let participants choose their path and take their time. Do not point out controls, suggest destinations, or explain product terminology.
- Observe discoverability, comprehension, navigation, audience/privacy understanding, unassisted completion, meaningful hesitation/backtracking, and expected-but-missing behavior. Do not repeat technical verification or perform visual-regression analysis.
- Use fictional, non-identifying content. Avoid recording personal data. Capture a screenshot only when it helps document a meaningful finding; note viewport and A/B identity.
- If a task is blocked by a technical failure, stop that task, record what happened, and continue only if the remaining tasks can be completed without corrupting the scenario or evidence.

## Compact observation template

Record one row per task and participant. Complete the required fields; leave optional fields blank or write `None observed` when appropriate.

| Task | Participant A/B | Completed YES/NO | Without help YES/NO | Intervention, if any | Meaningful hesitation/backtracking | Privacy/audience misunderstanding, if any | Navigation/wording problem, if any | Unexpected/missing behavior | Important participant comment | Finding classification, if applicable |
|---|---|---|---|---|---|---|---|---|---|---|
|  |  |  |  |  |  |  |  |  |  |  |

Session header: date; scenario database name; desktop and narrow viewport widths; anonymized participant codes. For an intervention, note the task, why it was needed, the exact help given, and whether it was blocking or nonblocking. Classify meaningful findings using the existing A–G categories; do not force a classification when no finding occurred.

## Brief 390 px procedure

After the desktop journey, have at least one participant do only 2–3 representative actions at approximately 390 px. For example: navigate to a shared item, perform one meaningful action, then return Home. Choose the relevant item/action from the completed scenario so no extra setup or verification step is needed. Observe navigation comprehension, action discoverability, readability, and whether the flow feels usable. Do not repeat the full journey or perform visual-regression analysis. Technical responsive checks remain covered by the separately recorded technical evidence.

## Intervention rules

Do not help simply because a participant pauses, explores, takes a wrong turn, or tries an unexpected route. Intervene only if the participant explicitly asks for help, cannot reasonably continue, a technical failure blocks the task, or a privacy/personal-data concern requires stopping. Give the minimum help needed to continue. Record only meaningful interventions, including the task, reason, exact help, and effect on unassisted completion. Stop if consent is withdrawn or a personal-data concern cannot be handled safely.

## Evidence to return

Return the completed compact observation rows, session date, fresh scenario database name, viewport widths, anonymized participant codes, meaningful interventions, and any relevant redacted screenshots or notes. Include observed privacy/audience misunderstandings, significant hesitation/backtracking, navigation or wording problems, and expected-but-missing behavior. Keep participant evidence distinct from Playwright and other technical evidence. Do not infer participant results from automated checks.

## Acceptance criteria

1. **`AC-01 — Fresh scenario preflight`**: A documented fresh dedicated scenario starts with A as administrator, B as a nonmember, and no Events, Plans, or Trips. The ordinary Development database is not used.
2. **`AC-02 — Invitation handoff`**: A sends B an invitation and retrieves its link from the Development outbox; B opens the invitation and can identify the Family and inviter before responding.
3. **`AC-03 — Membership and identity isolation`**: B begins with no active Family. Switching A-to-B clears prior identity context, and B's acceptance refreshes active membership without a browser refresh or stale A data.
4. **`AC-04 — Family/Home handoff`**: B can accept, optionally review or skip the profile, and reach the correct Family context and Home in-session. No route dead end occurs.
5. **`AC-05 — Relationship workflow`**: B can find the existing relationship entry point, suggest a relationship with A, and A can review and accept it. Testers do not interpret that relationship as granting private-item access.
6. **`AC-06 — Event creation and edit`**: A creates an Event with B's currently supported view/edit access; B can find it and perform an allowed detail edit; A can see the saved change. No unfinished Event behavior is required.
7. **`AC-07 — Plan audience and note`**: A creates a whole-Family Plan with a note; B, as an active Family member, can find and read it and explain their understanding of its audience.
8. **`AC-08 — Trip creation and Notes`**: A creates a Trip shared with B; B can open it and add a Trip Note; A sees the Note and its author.
9. **`AC-09 — Trip audience removal`**: A reviews the Trip audience preview before confirming removal of B; B then loses Trip access and A retains creator access.
10. **`AC-10 — Return journey and Home`**: Both testers return Home and navigate back to the relevant areas without coaching. The current Family and available Event, Plan, and Trip previews are understandable and accurate to each tester's access.
11. **`AC-11 — Preflight stop rule`**: A blocking preflight failure stops human testing. The failure and evidence are recorded and classified; no fix is made under this story.
12. **`AC-12 — Intervention tracking`**: Each tester task is recorded as completed or not completed; every facilitator intervention records whether it occurred, the task affected, why it was needed, and whether it was blocking or nonblocking. A critical task requiring button/path coaching is not reported as unassisted.
13. **`AC-13 — Observation capture`**: For each major task, record relevant hesitation/backtracking, confusing wording, privacy/audience confusion, Family/identity context, navigation/form difficulty, unsupported-feature assumptions, and unexpected behavior.
14. **`AC-14 — Evidence record`**: The verification record includes run date, scenario database identifier, viewport, active Development identity, task result, intervention count, relevant screenshots/notes, blocking errors, and privacy/access observations. Avoid unnecessary personal information.
15. **`AC-15 — Desktop usability`**: At approximately 1440 px, technical browser review finds no clipped primary actions or blocking layout problems, and actual participants attempt the required invitation, Family, Event, Plan, Trip, Notes, audience-change, and Home journey. Report task usability only from their observed completion/interventions, not from layout review alone.
16. **`AC-16 — Mobile usability`**: At approximately 390 px, technical browser review finds no horizontal overflow, clipped primary actions, or unusable controls on invitation, Home, Family, Event, Plan, Trip, Trip Notes, audience preview, and return navigation. At least one actual participant attempts the narrow-screen tasks without needing the full journey repeated; report usability only from that human task evidence, not browser review alone.
17. **`AC-17 — Optional tablet check`**: If performed, the approximately 820 px spot check is recorded as optional evidence and does not substitute for desktop or mobile checks.
18. **`AC-18 — Privacy and access boundary`**: No cross-identity or item-audience data leak occurs; Home shows only currently authorized items; Family membership, relationship, traveler, and administrator status are not mistaken for general access to private items; the Trip access change takes effect as previewed; B loses access after removal while A retains creator access; and no privacy misunderstanding capable of exposing information is left unresolved.
19. **`AC-19 — Finding classification`**: Every finding is classified and severity recorded before follow-up is recommended: A bug; B UX clarity issue; C visual polish; D existing-feature wiring; E missing approved behavior; F new product requirement; or G deferred capability.
20. **`AC-20 — Deferred-feature boundary`**: No deferred capability is treated as required test success or automatically called a defect. Tester requests/assumptions about deferred items are recorded and classified appropriately.
21. **`AC-21 — Technical verification evidence`**: `VER-WI-075-005` records technical preflight, relevant regression/access behavior, identity switching, and desktop/mobile browser evidence separately from human test evidence. A regression check not run is identified as unverified; evidence is labeled automated, Codex-run/browser, or otherwise sourced; self-verification applies only to technical checks actually self-verified; and a build/test pass is not described as human validation.
22. **`AC-22 — Human evidence integrity and gate`**: Human-task criteria are marked `PASS` only when actual adult participants perform the scenario and the facilitator supplies corresponding observations/evidence. Codex may prepare instructions/templates, run technical checks, organize supplied results, classify findings, and write verification documentation; it must not simulate participants or invent task results, hesitation, confusion, screenshots, or human validation. After technical preflight alone, human criteria remain `PENDING HUMAN TEST`, the report remains `PARTIAL / PENDING HUMAN TEST`, and the work item must not be marked `DONE`.
23. **`AC-23 — Phase 7.5 exit assessment`**: After human evidence exists, the verification assesses every Phase 7.5 roadmap exit criterion and records `PASS` (criteria satisfied with evidence), `CONDITIONAL` (only nonblocking observations remain), or `FAIL` (blockers remain). If human evidence is absent, the assessment remains pending and cannot pass. Any blocker prevents an exit `PASS`. Phase 7.5 remains `CURRENT`; this work item does not close the phase.

## Observation and outcome rules

Record outcomes as:

- **PASS:** Required journey completes without developer intervention; no blocking errors or route dead ends; Family and identity context are correct; access boundaries hold; privacy meaning is understandable; desktop and required mobile paths are usable.
- **OBSERVATION:** Wording uncertainty, hesitation, minor layout issue, visual preference, or nonblocking friction.
- **BLOCKER:** Invitation failure; required area cannot be found; an implemented item cannot be created or used; stale Family/identity context; access leak; privacy misunderstanding capable of exposing information; dead-end route; failed audience change; or unusable mobile path.

For every finding, preserve relevant evidence, classify it using AC-19's A-G scheme, and recommend separate follow-up only where needed. Do not modify application code, tests, canonical requirements, or product behavior to resolve findings in this work item. A requested behavior not already approved remains a candidate new product requirement, not an implicit change.

## Viewport and evidence plan

- **Desktop:** approximately 1440 px; run the full scenario where practical.
- **Mobile participant check:** approximately 390 px; at least one participant performs only 2–3 representative actions, such as navigating to a shared item, performing one meaningful action, and returning Home. Observe comprehension, discoverability, readability, and perceived usability. Do not repeat the whole journey or perform visual-regression analysis. The technical responsive coverage is recorded separately.
- **Tablet:** approximately 820 px; optional spot check.

Capture screenshots only when useful to explain a meaningful finding. Record screenshot viewport and active identity, redact them as needed, and do not capture unnecessary personal information.

## Session duration

Plan for **35–45 minutes total**, including a brief introduction, all 12 one-at-a-time tasks, and the short 390 px subset. Allow approximately **50–60 minutes** if participants naturally discuss their thinking. Think-aloud commentary is optional. Do not plan a 75–90 minute session.

## Verification plan

Execute this work in five stages. A failed Stage 1 stops the process before participant testing. Stages 2–5 may continue only after the previous stage's gate is satisfied; a later session may resume from its recorded stage without reusing an accepted B scenario for a fresh run.

### Stage 1 — Technical preflight

Codex may run the fresh-database preflight and record its outcomes. Check every preflight item above. If any required item fails, stop before human testing; capture evidence, classify the issue, determine severity, and report it. No repair is in scope.

### Stage 2 — Human-test preparation

After a passing preflight, prepare the goal-oriented script, compact observation sheet, browser sessions, viewport, and fresh starting state for actual adult participants. Use the facilitator instructions and duration above. Preparation is not human evidence.

### Stage 3 — Human session

The two adult participants perform the approved 12-task scenario at the desktop viewport, one prompt at a time. At least one participant attempts only 2–3 representative actions at approximately 390 px; the full human journey is not repeated there. Use the compact observation template and intervention rules above. A task receiving path/button coaching is not reported as unassisted. If there are no actual participants, do not simulate the session; leave human criteria pending.

### Stage 4 — Results documentation

Organize facilitator-supplied evidence, record observations and blockers, classify each finding, and create the verification report. Keep technical evidence distinct from human evidence and distinguish automated checks, Codex-run/browser checks, facilitator-recorded human evidence, and unrun/unresolved criteria.

### Stage 5 — Phase 7.5 exit assessment

Only after human evidence is available, assess each roadmap exit criterion against the combined evidence. Record `PASS`, `CONDITIONAL`, or `FAIL` with rationale. A pending human session cannot produce a passing exit assessment. Any blocker prevents an exit `PASS`. Phase 7.5 stays `CURRENT` pending a separate closure review/decision.

Create `ai-context/planning/verification/VER-WI-075-005.md` after authorized execution, with these distinct sections:

1. **Section A — Technical Preflight Evidence:** fresh-database preflight; relevant regression/access checks; identity switching; browser/viewport checks. For each item state evidence source (automated, Codex-run/browser, or other), result, and environmental limits. Label self-verification only for technical checks actually self-verified.
2. **Section B — Human User-Test Evidence:** actual tester task outcomes; facilitator interventions and reasons; observations/confusion points; blockers; screenshots/notes with viewport and active identity. Identify facilitator-supplied evidence. If no session occurred, state `PENDING HUMAN TEST`; do not infer or claim human usability results.
3. **Finding register:** A-G classification, severity/outcome, evidence, and recommended separate follow-up. No fixes are performed here.
4. **Unresolved/unrun criteria:** list every unrun or pending check, including human criteria if participants were unavailable.
5. **Phase 7.5 exit assessment:** each roadmap exit criterion mapped to evidence and an overall `PASS`, `CONDITIONAL`, `FAIL`, or pending result. Phase closure remains a separate explicit review/decision.

## Tasks for future authorized execution

- [x] `TASK-001` — Reconfirmed `CTX-1.24`, Phase 7.5 status, WI-075-001 through WI-075-004 verification, `OD-001`, and the approved PRP-026 scope.
- [x] `TASK-002` — Complete and document the fresh dedicated Development scenario technical preflight. The Codex retry was blocked by restricted-environment SQL encryption capability. Combined **DEVELOPER-SUPPLIED EXTERNAL TECHNICAL EVIDENCE** reports the invitation journey and 390 px smoke passing and confirms active database `FamilySafety_Invitation_ddf3f406d9024bc1ac330e46513b5209` with zero Events, Plans, and Trips. See `VER-WI-075-005.md`; Codex did not execute these supplied checks.
- [x] `TASK-003` — Owner-accepted for this milestone based on the supplied manual validation. No preparation artifact or completed observation sheet was supplied; the record does not claim the approved script/evidence sheet was used.
- [x] `TASK-004` — The owner reports manually exercising the major WI-075-005 workflow areas and that the MVP workflow passed sufficiently for this milestone. Codex did not conduct or observe the session; participant count, identities, and per-task results were not supplied.
- [x] `TASK-005` — Recorded the supplied scope and result without inventing session metadata, participant observations, interventions, screenshots, or task-level details; unavailable details are listed as limitations.
- [x] `TASK-006` — Preserved historical findings and technical evidence, classified the current supplied outcome, and updated `VER-WI-075-005.md` with distinct technical and owner/developer-supplied human evidence.
- [x] `TASK-007` — Assessed each Phase 7.5 roadmap exit criterion. The phase remains `CURRENT` and is marked `READY FOR CLOSURE`; formal phase closure remains a separate owner decision.

## Exclusions and boundaries

- No application, test, or database implementation changes; no bug fixes or feature additions.
- No changes to canonical requirements, workflows, permissions, architecture, or `OD-001`.
- No execution of the human user test until this story receives separate approval and explicit execution authorization.
- No Phase 7.5 closure or Phase 8 work.
- No required success criteria for Event RSVP, Needs, Updates, notifications, Check-Ins, Sharing Sessions, location sharing, Safety, production authentication, production email, broader Family administration, or final mobile navigation design.
- No automatic conversion of an observation, deferred capability, or tester request into an approved requirement.

## Product requirement impact

No new product requirement is needed for this test/verification work. It evaluates existing implemented behavior and approved boundaries only. Any genuinely new desired behavior must be proposed separately.

## Traceability

This work item adds no product requirement. The following existing contracts/workflows and Phase 7.5 roadmap exit criteria define what the verification evaluates:

| Existing requirement or roadmap exit criterion | Evidence expected | Acceptance criteria |
|---|---|---|
| `REQ-UX-001/002/003/006/007/008/011/012` — Family-first use, Home, navigation, current Family, and responsive use | Technical route/viewport checks plus actual tester task completion and observations | `AC-04`, `AC-10`, `AC-15`–`AC-17`, `AC-22`–`AC-23` |
| `REQ-FAM-005/006/007/008/009` — Invitation-only entry, controlled provisioning, profile review, and relationships | Fresh invitation preflight; human invitation and relationship tasks; boundary observations | `AC-01`–`AC-05`, `AC-11`–`AC-14`, `AC-22` |
| `REQ-PRIV-005/006/011/013/014/020/021/022` — Relationship/admin limits, membership/profile privacy, and item-specific Event/Plan/Trip access | Identity-switch and audience checks; Home authorization; human understanding and access observations | `AC-03`, `AC-05`–`AC-10`, `AC-18`, `AC-21`–`AC-23` |
| `REQ-EVT-001/005/008/013` — Implemented Event details/audience/edit behavior; deferred coordination boundaries | Event create/edit journey and explicit exclusion of RSVP, Needs, Updates, and notifications | `AC-06`, `AC-20`–`AC-22` |
| `REQ-PLAN-001/005/006` — Lightweight Plan audience and Notes | Whole-Family Plan creation/read and audience-understanding observation | `AC-07`, `AC-18`, `AC-22` |
| `REQ-TRIP-001/007/008/010/012` — Trip audience, creator access, Notes, and audience-change preview/confirmation | B Note/A visibility, preview and access removal, creator retention | `AC-08`, `AC-09`, `AC-18`, `AC-21`–`AC-23` |
| Roadmap exit: coherent first-use and returning-user journeys | Invitation and Home handoff, identity isolation, Home return journey | `AC-02`–`AC-04`, `AC-10`, `AC-22`–`AC-23` |
| Roadmap exit: understandable invitation/join path | Fresh preflight plus actual adult invitation task and recorded interventions | `AC-02`–`AC-04`, `AC-12`, `AC-22`–`AC-23` |
| Roadmap exit: useful Home with empty and populated data | Fresh starting-state/low-data Home, populated previews, return journey, access scope | `AC-01`, `AC-10`, `AC-18`, `AC-22`–`AC-23` |
| Roadmap exit: understandable navigation and Family administration | Family/route tasks, return navigation, context clarity and intervention evidence | `AC-04`, `AC-10`, `AC-12`–`AC-16`, `AC-22`–`AC-23` |
| Roadmap exit: natural connection across implemented areas and useful empty states | End-to-end Family, Event, Plan, Trip, Notes and Home journey; existing empty state is included at fresh start | `AC-01`, `AC-05`–`AC-10`, `AC-15`–`AC-16`, `AC-22`–`AC-23` |
| Roadmap exit: passing end-to-end scenario completable without developer explanation | Actual adult task outcomes and interventions; human-evidence gate | `AC-12`, `AC-13`, `AC-22`–`AC-23` |
| Roadmap exit: preserved privacy, audience, administrator, and sharing boundaries | Identity isolation, item audience, Home access, Trip access change, and observed understanding | `AC-03`, `AC-05`, `AC-07`–`AC-09`, `AC-18`, `AC-21`–`AC-23` |

## Execution authorization

- **Approved proposal:** `PRP-026 — End-to-End MVP User-Test Scenario`
- **Story created:** `YES`
- **Execution authorized:** `YES — Stage 1 technical preflight and 2026-09-28 retry only`, by explicit owner authorization.
- **Authorized scope completed:** The original attempt stopped at the NU1900 startup blocker. After WI-075-006 passed, an authorized retry used a new fresh scenario and passed startup, clean fixture, invitation creation, outbox, and initial A-to-B isolation. Opening the generated invitation link reset the active Development identity to A and made B's invitation unavailable, so Stage 1 stopped at that blocker. See `VER-WI-075-005.md`.
- **Human validation evidence:** The owner reports that manual workflow validation was performed and passed sufficiently for the current MVP / Phase 7.5 milestone. This report does not claim that Codex observed participants or that the session followed every planned procedure.
- **Not performed by Codex:** Human-test preparation/session, application fixes, feature work, or formal phase closure.

## Story readiness

**DONE — PASS WITH OWNER-ACCEPTED EVIDENCE LIMITATIONS.** Stage 1 technical evidence and the owner/developer-supplied human validation collectively support the current MVP milestone. The owner reports that the manual workflow covered Family invitation/joining, Family navigation/context, relationships, Events, Plans and audience understanding, Trips and Trip Notes, Trip audience changes/access, Home return/navigation, general comprehension, privacy/audience behavior, and narrow/mobile usability; no current issue was considered blocking. Codex did not observe or conduct this human validation. The exact session date, participant count/identities, scenario database, viewport/action trace, task-by-task outcomes, intervention count/details, and screenshots/notes were not supplied. Those evidence limitations remain explicit and are accepted as non-blocking for this milestone by the owner; no missing details are inferred. Historical identity-reset and restricted-environment SQL failures remain preserved; subsequent startup/fixture corrections and WI-075-007 through WI-075-010 evidence are referenced in `VER-WI-075-005.md`. **Current phase disposition:** Phase 7.5 remains `CURRENT / IN PROGRESS`. The later owner decision defers broader formal usability validation until visual design, screen composition, and mockup alignment are substantially mature; it supersedes the earlier phase-closure readiness interpretation without changing this story's evidence or `DONE` status. `OD-001` remains open.
