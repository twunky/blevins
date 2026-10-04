---
name: execute-story
description: Execute an approved story within its authorized scope, preserve governing context, and collect evidence for a separate verification pass.
---

1. Read `../../README.md`, `../../project-status.md`, and the approved story.
2. Confirm explicit execution authorization, the story's context baseline, and its `required_context`.
3. If authority conflicts, required context is missing, or the baseline is stale, stop and mark the story `BLOCKED` or `NEEDS_REVIEW`.
4. Perform only the listed tasks. Do not add requirements or widen scope.
5. Record outputs, checks, limitations, and evidence against the acceptance criteria.
6. Move the story to `VERIFYING` and hand it to `verify-story`; do not self-declare `DONE` without a verification result.
