# Claude Handoff — Blevins / Family Safety

**Current state (2026-10-04):** `CTX-1.30` ACTIVE (`DEC-083`; [VER-CTX-1.30](../ai-context/planning/verification/VER-CTX-1.30.md) PASS). Phase 9 remains `COMPLETE / CLOSED` (`DEC-081`; [VER-PH-009](../ai-context/planning/verification/VER-PH-009.md)). Phase 9.5 is `CURRENT / IN PROGRESS` for only [WI-095-001](../ai-context/planning/stories/WI-095-001-event-invitations-and-rsvp.md) — Event Invitations and RSVP: APPROVED scope, **execution authorized NO**. Separate owner execution authorization is required; later capabilities remain separately gated. Prior confirmation work committed `02ebce3`; see [CURRENT-TASK.md](CURRENT-TASK.md). Historical Phase 9 closure banner remains in [HISTORY.md § Part D](HISTORY.md#part-d).

> ## PRIMARY VISUAL REFERENCES: `C:\Dev\blevins\claude-handoff\reference`
>
> These six mockup images are **the visual target** for the web app. Open them with the image reader before any UI work:
>
> | File | Target for |
> |---|---|
> | `reference/detail-shell-topbar.png` | Shared app shell (top bar + sidebar) |
> | `reference/panel1-main-home.png` | Home |
> | `reference/panel2-family-activity.png` | Family dashboard direction under `DEC-077` (original image highlights Home) |
> | `reference/panel3-travel-events.png` | Events; its Trip rows inform the Trips list |
> | `reference/panel4-safety-aware.png` | Safety dashboard direction under `DEC-079`; mockup-only actions are excluded |
> | `reference/full-mockup.png` | Overview of panels 1–4 |
>
> **Hierarchy:**
> 1. `claude-handoff\reference` = **primary visual target**
> 2. `ai-context` = **product, privacy, and functional authority**. Mockups never authorize functionality.
> 3. The running app = implementation starting point
> 4. Generated screenshots = evidence of prior builds, **not** design authority
>
> Mapping, KEEP / ALIGN / ADAPT / DROP, and gaps: [MOCKUP-ALIGNMENT.md](MOCKUP-ALIGNMENT.md).

Session and task guidance for Claude coding sessions working in this repository.

- **Project root:** `C:\Dev\blevins`
- **Product:** Family Safety, Coordination & Planning Application (local-first MVP; .NET 10 API + Angular 22 web client)
- **Canonical context:** `C:\Dev\blevins\ai-context`
- **Prepared:** 2026-10-01. Current guidance reconciled 2026-10-04 with CTX-1.30 verified activation (`DEC-083`). Phase 9 closure remains intact. Only WI-095-001 scope is approved, execution NO. Cleanup Waves 1–5 complete; no broad cleanup/reorganization authorized.

## Source of truth

```
claude-handoff  = session/task guidance (how to work, what to do now, where to look)
ai-context      = authoritative product and project context
```

**If anything in this handoff conflicts with `ai-context`, AI-CONTEXT WINS.** Report the conflict in your closeout so the handoff can be corrected. If two authoritative `ai-context` sources conflict with each other, stop and report `BLOCKED—CONTEXT CONFLICT`, citing both (rule from `ai-context/README.md`).

Authority order, from `ai-context/README.md` § Authority model:

1. `ai-context/README.md` — AI and context operating rules
2. Approved core contracts (`ai-context/core/*-contract.md`) — product, UX, privacy, safety truth
3. Approved workflows (`ai-context/workflows/`)
4. Approved stories / work items (`ai-context/planning/stories/`) — authorize bounded work only
5. Verification reports — evaluate intent, cannot redefine it
6. `project-status.md`, `requirements-index.md`, routing, skills, templates — operational support
7. UX mockups — **non-authoritative for behavior**. For *visual* direction, `claude-handoff/reference/` is primary; `ai-context/references/ux/` is secondary, for screens the primary set does not cover.
8. This handoff — orientation only

## Where to start

Read in this order. Stop reading once you have what you need.

1. `ai-context/README.md` and `ai-context/project-status.md` — the mandatory ai-context entry sequence still applies.
2. [CURRENT-TASK.md](CURRENT-TASK.md) — what the owner wants from this session.
3. [PROJECT-STATE.md](PROJECT-STATE.md) — short orientation on phase, completed work, open decisions.
4. [WORKING-RULES.md](WORKING-RULES.md) — what you may and may not change; stop conditions.
5. [MOCKUP-ALIGNMENT.md](MOCKUP-ALIGNMENT.md) — the KEEP / ALIGN / ADAPT / DROP method and reference index. **Authorized UI work only.**
6. [CONTEXT-ROUTING.md](CONTEXT-ROUTING.md) — which authoritative file answers which question. Use it instead of reading the whole `ai-context` tree.
7. [VALIDATION.md](VALIDATION.md) — commands and rendered-review expectations after changes.
8. [SESSION-CLOSEOUT.md](SESSION-CLOSEOUT.md) — the format for handing work back.

Before adding or moving Angular folders, read the client's [structure and extraction rule](../src/FamilySafety.Web/README.md#structure-and-extraction).

Do not load [HISTORY.md](HISTORY.md) for normal work. It keeps earlier checkpoints, closed work-item scopes, and old closeout templates word-for-word for audit only.

## Current phase (one line)

Phase 9.5 — MVP Feature Completion is `CURRENT / IN PROGRESS` under DEC-083; CTX-1.30 ACTIVE. Only WI-095-001 — Event Invitations and RSVP is APPROVED; execution NO. Phase 9 remains COMPLETE / CLOSED under DEC-081. See [CURRENT-TASK.md](CURRENT-TASK.md) for gates and [PROJECT-STATE.md](PROJECT-STATE.md) for orientation; earlier checkpoints remain in [HISTORY.md § Part D](HISTORY.md#part-d). Future authorized UI work follows the existing visual system.

## Workflow

**Server work (last used for WI-008-003, now `DONE / PASS`):**

1. Orient (entry files, this handoff, the story).
2. Implement the story's tasks within its authorized scope.
3. Validate with `VALIDATION.md` § 4, with the SQL gate on.
4. Write the work item's `VER` record.
5. Hand back using `SESSION-CLOSEOUT.md` (including its server-work items), with the work item left in `VERIFYING / OWNER REVIEW`.

**UI work (historical example: WI-008-004, now `DONE / PASS`):**

1. Orient.
2. Compare the mockup against the running implementation and write the comparison down **before** editing code.
3. Stop for owner review of the comparison when the prompt asks for it.
4. Implement within authorized scope.
5. Validate: tests, Playwright, build, rendered desktop and narrow review, and mockup comparison.
6. Hand back using `SESSION-CLOSEOUT.md`, with the work item left in `VERIFYING / OWNER REVIEW`.

## No-commit rule

**Do not commit. Do not push. Do not create or complete a pull request.** The owner commits manually. Leave every change uncommitted in the working tree and report the actual Git status.

## Relationship to `codex-handoff/`

The earlier Codex-targeted Home-page build brief is preserved at [ai-context/archive/codex-handoff/CODEX_PROMPT.md](../ai-context/archive/codex-handoff/CODEX_PROMPT.md). It is historical; its Next.js/Tailwind and mock-data instructions do not apply. Its six duplicate image copies were removed; the retained originals are in `claude-handoff/reference/`.
