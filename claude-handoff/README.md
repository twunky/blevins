# Claude Handoff — Blevins / Family Safety

This directory provides session guidance. [ai-context](../ai-context/README.md) is authoritative; this handoff cannot authorize product behavior or implementation. If the handoff conflicts with canonical context, follow canonical context and report it. Conflicting authoritative sources block work under the AI operating rules.

## Where to start

1. Read [AI operating rules](../ai-context/README.md) and [project status](../ai-context/project-status.md).
2. Read [WORKING-RULES](WORKING-RULES.md); current authority is already in project status.
3. Use [CONTEXT-ROUTING](CONTEXT-ROUTING.md) only to select sources for the requested task; load the authorized item's required-context manifest when one exists.

Read [PROJECT-STATE](PROJECT-STATE.md) only for architecture orientation, [VALIDATION](VALIDATION.md) when choosing checks, and [SESSION-CLOSEOUT](SESSION-CLOSEOUT.md) when handing back work.

For authorized UI work, read the [UI Visual Design Contract](../ai-context/ui-visual-design-contract.md) and inspect the current screen implementation. [Historical reference routing](MOCKUP-ALIGNMENT.md) is needed only for provenance questions.

Before adding or moving Angular folders, read the [client structure rule](../src/FamilySafety.Web/README.md#structure-and-extraction).

[HISTORY](HISTORY.md), archives, completed verification, and old task briefs are retained for specific historical questions and are excluded from normal loading. The archived Codex brief's Next.js/Tailwind instructions do not apply.
