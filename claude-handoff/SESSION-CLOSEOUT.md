# Session Closeout

Hand work back with a single message. Be factual. If something was not run, say so. If something failed, include the output.

Future work requires separate authorization. The repository has Git, and the owner performs commits: report the actual `git status`, and do not stage, commit, push, or open a pull request. The WI-008-003 server and WI-008-002 UI templates this structure came from are kept word-for-word in [HISTORY.md § Part C](HISTORY.md#part-c).

## Closeout structure

Use the sections that apply to the authorized work item, in this order, and substitute that item's ID, scope, evidence, and status.

1. **Summary**, in 2–4 sentences.
2. **Acceptance checks:** one row per acceptance criterion, with its result and evidence.
3. **Files changed:** a table of file and change.
4. **Server work only:**
   - migration name and what it creates, or "none";
   - `dotnet build` result;
   - `dotnet test` command and counts, stating whether `RUN_FAMILYSAFETY_SQL_TESTS=1` was set and proving that the SQL-gated tests ran.
5. **UI work only:**
   - visual changes, and each KEEP item and privacy guardrail confirmed as preserved;
   - `npm test -- --watch=false` counts;
   - `npm run build` result, with budget warnings and sizes against the previous baseline;
   - the affected Playwright specs and their results;
   - rendered evidence at desktop and 390 px, marking mocked or intercepted captures;
   - mockup comparison and accessibility review.
6. **Implementation choices** that the story leaves open.
7. **Conflicts found** in canonical context or this handoff, or "none".
8. **Predeployment dependencies** affected, if any (for example, backup aging and restore evidence).
9. **Source control:** the actual `git status` output, and a statement that no commit, push, or pull request was made.
10. **Status:** `VERIFYING / OWNER REVIEW`. UI work also needs explicit owner visual approval.
11. **Proposed PR title and description**, as text the owner can paste.
12. **Recommended follow-ups (not done).**

## Rules

- Do not claim a test, build, or capture ran unless it ran in this session.
- Do not mark a work item `DONE / PASS` or write "approved" on the owner's behalf without its required approval.
- Keep the PR description suitable for the owner to paste. Keep the check-in summary to a few lines.
