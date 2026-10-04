# Family Safety, Coordination & Planning Application

**Phase 9 closure (2026-10-03):** WI-009-011 is `DONE / PASS — OWNER APPROVED`; VER-WI-009-011 is `DONE / PASS`; [VER-PH-009](planning/verification/VER-PH-009.md) is `PASS / PHASE CLOSURE VERIFIED`, all 15 exits PASS. DEC-081 records explicit conditional owner closure after Plan filtering, focused verification, one final build, sole final full Playwright 34/34 and artifact health passed. Phase 9 `COMPLETE / CLOSED`; CTX-1.28 ACTIVE; Development user testing READY; Phase 9.5 READY TO BEGIN. No implementation work item remains authorized; no new CTX, out-of-scope feature, commit or push.

AI Context Foundation v1.0 operating guide.

## Mandatory entry sequence

Every AI work session must:

1. Read this file.
2. Read `project-status.md`.
3. Load only the files and requirement anchors named by the active work item's `required_context`.
4. Confirm the work mode, authority, context baseline, and blockers before acting.

Do not recursively read the entire `ai-context/` tree. Bootstrap, archived, superseded, and unrelated material is not normal working context.

## Mandatory AI rules

- Do not invent product behavior, permissions, user roles, requirements, architecture, dependencies, or scope.
- UX mockups under `references/ux` are non-authoritative design evidence. They may guide presentation and workflow discussion but must not override canonical contracts, requirements, privacy rules, or approved work items.
- Report missing, ambiguous, contradictory, or outdated information explicitly.
- If authoritative sources conflict, stop and report `BLOCKED—CONTEXT CONFLICT`, citing each conflicting source.
- All work must follow current best practices and established patterns appropriate to the selected technology, product risk, and project phase.
- All project artifacts, proposals, stories, code, tests, and verification evidence must be human-readable, maintainable, and understandable by a future developer.
- Use the least expensive capable model and reasoning level that preserves correctness.
- High-impact privacy, safety, permissions, ambiguous defects, architecture, and cross-contract work routes to `DEEP` or `ESCALATION`.
- Approved contracts and workflows cannot be overridden by stories, tasks, skills, templates, or status notes.
- A story or work item is not complete without acceptance criteria and verification evidence.
- Independent verification is preferred. Self-verification must be labeled `SELF-VERIFIED`.

## Authority model

1. This `README.md` governs AI and context operating behavior.
2. Approved core contracts govern product, UX, privacy, and safety truth.
3. Approved workflow documents govern workflow behavior while remaining constrained by the contracts.
4. Approved stories and tasks authorize bounded work but cannot override contracts or workflows.
5. Verification evaluates approved intent but cannot redefine it.
6. `project-status.md`, `requirements-index.md`, routing files, skills, and templates support operation but are not substitutes for canonical product truth.
7. Bootstrap, superseded, and archived content is non-authoritative normal context.

A decision that changes canonical context is not operational until the affected canonical files and references are updated.

## Context loading

After the mandatory entry files, select context by task:

| Work | Load next |
|---|---|
| Discuss or analyze a change | Relevant contracts, workflows, requirements, and proposal template |
| Create or refine a story or work item | Approved proposal if present, roadmap context, relevant canonical sources, and the matching template |
| Execute a story or work item | Approved story or work item, its `context_baseline`, its `required_context`, and `execute-story` skill |
| Verify work | Story or work item, canonical requirement sources, evidence, and `verify-story` skill |
| Maintain context | Accepted decision, affected canonical files, requirement index, and `maintain-context` skill |
| Audit history | Only the specifically referenced bootstrap or archived material |

**Historical context through WI-009-010 (current checkpoint superseded by DEC-081):** `WI-009-010 — First-Use and Invitation Arrival Workflow`, `DONE / PASS` — `OWNER APPROVED` (2026-10-03; `VER-WI-009-010`; `DEC-080`); no implementation work item is currently authorized. The active approved baseline is `CTX-1.28` (item-link Family context clarification of `REQ-UX-014`, `DEC-075`; `CTX-1.27`, Phase 9 navigation and item links under `DEC-074`, and earlier baselines are historical). Stories normally reference this baseline plus a small `required_context` manifest. Pin individual document versions only when a specific dependency requires it. Phase 7 — Plans, Trips & Sharing is closed. Phase 7.5 — End-to-End UX & User-Test Readiness is `COMPLETE / CLOSED` under `DEC-065`; Phase 8 — Safety Coordination is `COMPLETE / CLOSED` under `DEC-072` (2026-10-02; `VER-PH-008`). It entered under `DEC-066`. Phase 9 — Integration & UX Refinement is `CURRENT / IN PROGRESS` under `DEC-073` (2026-10-02); its planning basis, `PRP-030`, is `APPROVED` under `DEC-074`, which also resolves `OD-001` (six-area navigation, `REQ-UX-013`) and sets the deep-link Family-context rule (`REQ-UX-014`, `REQ-PRIV-027`). The first Phase 9 work item, `WI-009-001 — Playwright Per-Spec Scenario Isolation`, is `DONE / PASS` — `OWNER APPROVED` (2026-10-02; `VER-WI-009-001`). `WI-009-002 — Test Artifact Lifecycle and Cleanup` is `DONE / PASS` — `OWNER APPROVED` (2026-10-02; `planning/verification/VER-WI-009-002.md`). `WI-009-003 — Six-Area Navigation and Safety Discoverability` (`PRP-030` M2) is `DONE / PASS` — `OWNER APPROVED` (2026-10-02; `planning/verification/VER-WI-009-003.md`). `WI-009-004 — Event and Plan Single-Item Read Foundation` (`PRP-030` M3, server part) is `DONE / PASS` — `OWNER APPROVED` (2026-10-02; `planning/verification/VER-WI-009-004.md`). It adds `GET /api/events/{eventId}` and `GET /api/plans/{planId}` under the unchanged access rules. `WI-009-005 — Refresh-Safe Item URLs and Authorize-First Deep Links` (`PRP-030` M3 client part) is `DONE / PASS` — `OWNER APPROVED` (2026-10-03; `planning/verification/VER-WI-009-005.md`; `DEC-075`). `WI-009-006 — Home Dashboard Prototype Alignment` is `DONE / PASS` — `OWNER APPROVED` (2026-10-03; `planning/verification/VER-WI-009-006.md`; `DEC-076`). It is a presentation-only Home redesign that supersedes the `WI-009-005` `AC-16` Home-presentation clause and implements no Phase 9.5 capability. `WI-009-007 — Family Dashboard Prototype Alignment` is `DONE / PASS` — `OWNER APPROVED` (2026-10-03; `planning/verification/VER-WI-009-007.md`; `DEC-077`). It is the presentation-only `/family` dashboard and `/family/settings` split, with honest Family placeholders and no Phase 9.5 capability. `WI-009-008 — Events Dashboard Prototype Alignment` is `DONE / PASS` — `OWNER APPROVED` (2026-10-03; `planning/verification/VER-WI-009-008.md`; `DEC-078`). It is the presentation-only `/events` dashboard with `/events/new` and `/events/all`, a current-Family Trips summary, real Quick Links, an RSVPs Needed placeholder, and no Phase 9.5 capability. `WI-009-009 — Safety Dashboard Prototype Alignment` is `DONE / PASS` — `OWNER APPROVED` (2026-10-03; `VER-WI-009-009`; `DEC-079`, owner decisions O1–O14). `/safety` is a read-only Safety dashboard: separate Open Concerns and Open Safety Incidents summaries from the existing authorized list reads, a Recent Check-Ins placeholder, and static guidance, with Back to Safety on `/concerns` and `/incidents`. Presentation and client composition only; no Phase 9.5 capability and no API, schema, or authorization change. No implementation work item is currently authorized. The approved but unimplemented MVP capabilities are assigned to Phase 9.5 — MVP Feature Completion (`FUTURE`). PRP-028 is `APPROVED` under `DEC-067` and `DEC-068`; Concern rules and retention are canonical in `CTX-1.25`. WI-008-001 is `DONE / PASS`; its denied-audit persistence clarification is `DEC-069`. WI-008-002 Concern MVP User Interface is `DONE / PASS` with owner visual approval. `PRP-029` (Safety Incident workflow and access) is `APPROVED` under `DEC-071`, which also resolves `OD-004`. Its Incident rules are canonical in `CTX-1.26` (`REQ-SAFE-016`, `REQ-PRIV-026`). `WI-008-003` (Safety Incident server foundation) is `DONE / PASS`, owner approved on 2026-10-01 (`VER-WI-008-003`). `WI-008-004` (Incident UI and Start-an-Incident Flow) is `DONE / PASS` — `OWNER APPROVED` (owner visual review 2026-10-02; `VER-WI-008-004`). See `project-status.md`, `planning/stories/WI-008-002-concern-mvp-user-interface.md`, and `planning/verification/VER-WI-008-002.md`. `DEC-064` clarifies existing `REQ-UX-011` for current Family selection in the MVP shell; no new requirement ID is added.

Each story or work item's `required_context` must be intentionally small and explain why each file or anchor is needed. Do not include a contract, workflow, proposal, bootstrap file, archive file, or prior verification report merely because it exists. If broader context becomes necessary during work, add the specific file and reason to the story or work item, or report the need before continuing.

Each story or work item should also name explicitly excluded context when useful, especially workflows, contracts, bootstrap, archive, and historical verification that are not needed for the task.

If the active baseline changes before execution or verification, set affected work to `NEEDS_REVIEW` until compatibility is confirmed.

## Work modes

### PROPOSAL

Analyze and prepare a bounded recommendation. Do not make application changes or perform external actions. Contract changes, privacy or safety impacts, cross-workflow changes, ambiguity, or newly discovered scope begin here unless explicitly authorized otherwise.

### EXECUTION

Perform only the approved and explicitly authorized scope. Stop and return to Proposal Mode when execution discovers a conflict, missing decision, stale baseline, or required scope outside the approval.

Execution authorization never permits an agent to redefine contracts or workflows.

### Standing bounded auto-approval

`DEC-032` grants prospective standing authorization to reduce approval turns for routine work. A recommendation, low-impact proposal, or work item may be accepted automatically, and work-item approval may be combined with execution authorization, only when every condition below is true:

- the work is `ROUTINE` or `STANDARD`;
- it stays wholly within approved contracts, workflows, baseline, phase, and requested objective;
- intent, scope, acceptance checks, and non-goals are complete and unambiguous;
- the action is local, reversible, and creates no material external, financial, privacy, safety, permission, identity, or data-retention effect; and
- normal verification evidence will still be produced.

The work record must cite `DEC-032` and state exactly what was auto-approved and authorized. Standing authorization is prospective and never retroactively approves an existing item.

Explicit project-owner approval remains required for `DEEP` or `ESCALATION` work; product or MVP scope; canonical product, UX, privacy, safety, or workflow changes; context baselines; privacy, safety, permissions, identity, retention, or ambiguous defects; architecture or provider selection; paid services or material cost; external communications or external-system writes; destructive or difficult-to-reverse actions; phase closure or transition; and any missing, stale, ambiguous, or conflicting decision.

No-guessing, authority, conflict-blocking, scope, and verification rules always remain in force.

## Work hierarchy and lifecycle

`Phase → Epic when useful → Proposal when required → Story or Work Item → Task → Verification`

For this personal project, use lightweight Work Items by default instead of sprint-heavy story planning. A Work Item still needs a goal, narrow required context, boundaries, acceptance checks, execution authorization, and verification evidence.

When `DEC-032` applies, the work item may record standing authorization instead of waiting for a separate approval turn. Otherwise, approval and execution remain explicit checkpoints and may be combined in one project-owner message.

`Discussion → Proposal when needed → Decision → Canonical Context Update when needed → Story or Work Item → Task → Execution → Verification → Status Update`

Normal story/work-item states are:

`DRAFT → PROPOSED → APPROVED → IN_PROGRESS → VERIFYING → DONE`

Additional states are `BLOCKED`, `NEEDS_REVIEW`, and `SUPERSEDED`.

## Verification

- Every story or work item has explicit acceptance criteria.
- Each criterion and referenced requirement maps to evidence and a result.
- The verifier must not change requirements to make work pass.
- The later project verification phase supplements rather than replaces story/work-item-level verification.
- If independent verification is unavailable, label the report `SELF-VERIFIED` and disclose the limitation.

## Supersession and archive

Superseded IDs are never reused. Superseded material may be retained for audit but is excluded from normal routing. `archive/` is created only when first needed. The bootstrap source is extraction evidence only and must leave normal routing after `CTX-1.0` approval.

## Architecture boundary

Phase 3 may analyze and propose technical architecture only when authorized. Accepted Phase 3 decisions apply only when recorded in `decision-log.md` and reflected in the relevant planning/context files. No framework version, database production choice, hosting, authentication provider, deployment, infrastructure, application code, or implementation-domain design is authorized until explicitly approved. A proposal never authorizes implementation.

