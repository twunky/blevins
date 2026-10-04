# Claude Handoff — Blevins / Family Safety

**Phase 9 closure (2026-10-03):** WI-009-011 is `DONE / PASS — OWNER APPROVED`; VER-WI-009-011 is `DONE / PASS`; [VER-PH-009](../ai-context/planning/verification/VER-PH-009.md) is `PASS / PHASE CLOSURE VERIFIED`, all 15 exits PASS. DEC-081 records explicit conditional owner closure after Plan filtering, focused verification, one final build, sole final full Playwright 34/34 and artifact health passed. Phase 9 `COMPLETE / CLOSED`; CTX-1.28 ACTIVE; Development user testing READY; Phase 9.5 READY TO BEGIN. No implementation work item remains authorized; no new CTX, out-of-scope feature, commit or push.

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
- **Prepared:** 2026-10-01. Current guidance reconciled after Phase 9 closure (`DEC-081`; `VER-PH-009`) against active baseline `CTX-1.28`. Repository cleanup Wave 1 is documentation maintenance only; no product implementation item is authorized.

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

## Current phase (one line)

Phase 9 — Integration & UX Refinement is `COMPLETE / CLOSED` (`DEC-081`, 2026-10-03). WI-009-011 is DONE / PASS — OWNER APPROVED; all 15 exits pass. CTX-1.28 ACTIVE; Development testing READY; Phase 9.5 READY TO BEGIN. No implementation work item is authorized (see [CURRENT-TASK.md](CURRENT-TASK.md)). Earlier closed items remain:

- `WI-009-011 — Phase 9 Exit Criteria Verification` (`DEC-081`): approved Plan/Trip Home freshness, explicit Safety substitution, consistency/journey/axe/keyboard evidence, final build/full regression and guarded artifact cleanup.
- `WI-009-005 — Refresh-Safe Item URLs and Authorize-First Deep Links`;
- `WI-009-007 — Family Dashboard Prototype Alignment` (`DEC-077`): the `/family` dashboard and `/family/settings` split, presentation only.
- `WI-009-008 — Events Dashboard Prototype Alignment` (`DEC-078`): the `/events` dashboard, `/events/new`, and `/events/all`, presentation and client routing only.
- `WI-009-009 — Safety Dashboard Prototype Alignment` (`DEC-079`): the read-only `/safety` dashboard with separate Open Concerns and Open Safety Incidents summaries, a Recent Check-Ins placeholder, and Back to Safety links; presentation and client composition only.
- `WI-009-010 — First-Use and Invitation Arrival Workflow` (`DEC-080`): invitation arrival on `/home` with a one-time join announcement, Go to Home exits, plain no-Family guidance with one Family-required message per page, a first Family choice that keeps the page, and Invite feedback in the Invite card; client only.
- `WI-009-006 — Home Dashboard Prototype Alignment` (`DEC-076`). It is a presentation-only Home redesign that supersedes the `WI-009-005` `AC-16` Home-presentation clause. `CTX-1.28` amends `REQ-UX-014` (no fabricated Family context; one-time Family-list refresh). `WI-009-001 — Playwright Per-Spec Scenario Isolation`, `WI-009-002 — Test Artifact Lifecycle and Cleanup`, `WI-009-003 — Six-Area Navigation and Safety Discoverability`, and `WI-009-004 — Event and Plan Single-Item Read Foundation` are `DONE / PASS` — `OWNER APPROVED` (2026-10-02). `PRP-030` is `APPROVED` (`DEC-074`), `OD-001` is resolved (six-area navigation, `REQ-UX-013`), and `CTX-1.28` is active (`DEC-075`). Unbuilt approved MVP capabilities belong to Phase 9.5 — MVP Feature Completion, not Phase 9. Phase 8 history: Phase 8 — Safety Coordination is `COMPLETE / CLOSED` (`DEC-072`, 2026-10-02; `VER-PH-008`). `WI-008-002` is `DONE / PASS`. The site-wide mockup alignment to `claude-handoff\reference` is **complete and owner-approved** (`DEC-070`: blue palette, sidebar + slim top bar, card system). The Safety Incident workflow is approved (`PRP-029`, `DEC-071`, `OD-004` resolved) and canonical in `CTX-1.26`. `WI-008-003 — Safety Incident Domain and Access Foundation` is `DONE / PASS`, owner approved 2026-10-01; the Incident server foundation is complete. `WI-008-004 — Incident UI and Start-an-Incident Flow` is `DONE / PASS` — `OWNER APPROVED` (2026-10-02). See [CURRENT-TASK.md](CURRENT-TASK.md). Any future UI work should follow the approved visual system in `src/FamilySafety.Web/src/styles.scss`.

## Workflow

**Server work (historical: WI-008-003, now `DONE / PASS`):**

1. Orient (entry files, this handoff, the story).
2. Implement the story's tasks within its authorized scope.
3. Validate with `VALIDATION.md` § 4, with the SQL gate on.
4. Write `VER-WI-008-003`.
5. Hand back using the server format in `SESSION-CLOSEOUT.md`, with the work item left in `VERIFYING / OWNER REVIEW`.

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
