# Family Safety, Coordination & Planning Application

**Current context (2026-10-04):** `CTX-1.30` is ACTIVE under `DEC-083` after [VER-CTX-1.30](planning/verification/VER-CTX-1.30.md) PASS. Phase 9 remains `COMPLETE / CLOSED` under `DEC-081` and [VER-PH-009](planning/verification/VER-PH-009.md), with historical closure baseline `CTX-1.28`. Phase 9.5 is `CURRENT / IN PROGRESS` for only [WI-095-001](planning/stories/WI-095-001-event-invitations-and-rsvp.md) — Event Invitations and RSVP (`APPROVED`; **execution authorized: NO**). Canonical context/work-item preparation is complete; product execution requires a separate explicit owner instruction. Later Phase 9.5 capabilities require separate owner/work-item gates. `CTX-1.29` remains the historical Plan-note amendment.

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

**Historical checkpoint routing:** Earlier Phase 9 entry and work-item states are historical, not current authorization. The closure decision is [DEC-081](decision-log.md), and [VER-PH-009](planning/verification/VER-PH-009.md) records the final exit mapping. Use `project-status.md` and the roadmap for current status. Pin individual document versions only when a specific dependency requires it.

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

