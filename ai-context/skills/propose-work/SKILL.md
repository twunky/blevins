---
name: propose-work
description: Analyze a requested change and create a bounded proposal or approval-ready story when intent, impact, or authorization must be established before execution.
---

1. Read `../../README.md` and `../../project-status.md`.
2. Identify the mode, baseline, affected authority, relevant requirement IDs, and smallest `required_context`; load only that context.
3. Report missing, stale, ambiguous, or conflicting information. For authoritative conflicts, stop with `BLOCKED—CONTEXT CONFLICT`.
4. Use `../../templates/proposal.md` for high-impact, ambiguous, privacy/safety, permissions, architecture, cross-contract, or not-yet-authorized work.
5. Use `../../templates/story.md` only when intent and scope are already decision-complete.
6. Do not invent behavior, expand scope, or treat a proposal as execution authorization.
