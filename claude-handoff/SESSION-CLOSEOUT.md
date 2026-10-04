# Session Closeout

Hand work back with a single message. Be factual. If something was not run, say so. If something failed, include the output.

The WI-008 examples below are historical templates, not current work or authorization. Repository Cleanup Waves 1–5 are complete. Future work requires separate authorization. The repository has Git, and the owner performs commits; the remaining cleanup source-control step is `git rm --cached .claude/settings.local.json`, performed by the owner.

**Server work (last used for WI-008-003, now `DONE / PASS`).** Use this structure:

1. Summary.
2. AC-by-AC result table (AC-01 to AC-29), with evidence.
3. Files changed.
4. Migration name and what it creates.
5. `dotnet build` result.
6. `dotnet test` with `RUN_FAMILYSAFETY_SQL_TESTS=1`: command, counts, and proof the Incident SQL tests ran.
7. Implementation choices made (the story's non-policy list).
8. Canonical or handoff conflicts found.
9. Predeployment dependency (backup aging / restore evidence).
10. Source control (actual `git status`; no commit or push).
11. Status: `VERIFYING / OWNER REVIEW`.
12. Proposed PR title and description, as text.
13. Recommended follow-ups (not done).

**UI work — historical WI-008-002 example.** Use its structure only when a separately authorized UI work item calls for it; substitute that item's scope, evidence, and status.

```markdown
## Closeout — WI-008-002 Concern mockup alignment — <date>

### 1. Summary
<2–4 sentences: what changed and the overall fidelity judgment>

### 2. Visual changes
- <state/area>: <what changed, mockup cue it follows>

### 3. Functionality preserved
<Confirm each KEEP item still holds; note any spec/E2E locator updates and why>

### 4. Privacy preserved
<Confirm each guardrail: no automatic subject access, explicit participants,
creator vs participant separation, no Family/admin access, server authority,
append-only correction, additive challenge, Open/Frozen/Resolved semantics,
nondisclosing unavailable>

### 5. Files changed
| File | Change |
|---|---|

### 6. Tests
`npm test -- --watch=false` → <files / tests passed / failed>

### 7. Build
`npm run build` → <result>; budget warnings: <list with sizes, vs. baseline>

### 8. Playwright
`npx playwright test --config e2e/playwright.config.ts e2e/concern-workflow.spec.ts` → <result>
<other suites if run>

### 9. Rendered evidence
<list of new capture paths, desktop and 390 px; mark mocked/intercepted captures;
narrow overflow check result>

### 10. Mockup comparison
| State | Now matches | Still differs | Why |
|---|---|---|---|
Biggest remaining gaps: <1–3>

### 11. Accessibility review
<findings; manual/code-based>

### 12. Proposed PR title
<title>

### 13. Proposed PR description
<short description: purpose, visual changes, preserved behavior and privacy,
validation results, evidence location, owner visual approval still pending,
no server/API/schema changes>

### 14. Story notes / check-in summary
<dated text added to the WI-008-002 story and the VER-WI-008-002 follow-up section>

### 15. Source control
`git status` → <actual output>
**No commits, pushes, or PRs were made.** All changes are uncommitted.

### 16. Status
WI-008-002 remains `VERIFYING / OWNER REVIEW`; owner visual approval pending.

### 17. Handoff/context issues found
<conflicts between this handoff and ai-context, stale lines, or "none">

### 18. Recommended follow-ups (not done)
<out-of-scope ideas, if any>
```

## Rules

- Do not claim a test, build, or capture ran unless it ran in this session.
- Do not mark a work item `DONE / PASS` or write "approved" on the owner's behalf without its required approval.
- Keep the PR description suitable for the owner to paste. Keep the check-in summary to a few lines.
