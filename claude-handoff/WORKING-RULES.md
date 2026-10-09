# Working Rules

[AI operating rules](../ai-context/README.md) govern authority, conflicts, work modes, and DEC-032 limits. [Project status](../ai-context/project-status.md) owns current authorization; completed work items authorize no further implementation.

## Scope and authority

- Product scope expansion starts with proposal and owner approval. Change application code only within an approved, execution-authorized work item; honor its exclusions and stop conditions.
- Do not invent behavior, permissions, architecture, retention, or resolutions of open decisions. Canonical changes require their own authorization.
- If canonical sources conflict, stop with BLOCKED—CONTEXT CONFLICT and cite both. If the handoff conflicts with canonical context, follow canonical context and report it.
- Stop and report when work needs scope outside approval, a missing decision, or an unexplained validation failure.
- Keep architecture simple, readable, and developer-friendly. Prefer existing patterns; no broad cleanup, refactor, folder symmetry, or App decomposition without a concrete authorized need. [Client README](../src/FamilySafety.Web/README.md#structure-and-extraction) owns Angular structure rules.

## Privacy and presentation

- Server authorization is authoritative; client checks are display hints. Selected Family is not authorization.
- Preserve privacy boundaries, nondisclosing denied access, and separation between Family, Safety, Sharing, Check-In, location, Trip, and profile permissions. Load the relevant canonical contract/workflow via [routing](CONTEXT-ROUTING.md).
- No implicit administrator override, automatic escalation, danger determination, risk scoring, notification, or emergency dispatch.
- For UI work, follow the [UI Visual Design Contract](../ai-context/ui-visual-design-contract.md); unsupported mockup functionality remains excluded. Preserve required copy, calm tone, accessibility, and the approved visual system. Obtain owner visual approval where the item requires it.
- Do not raise build budget thresholds, add screenshot golden baselines, or overwrite/delete existing verification evidence without authorization.

## Lean validation

- Use focused validation during development. Run one meaningful broad regression near completion when integrated behavior, shared infrastructure, or material risk justifies it.
- Repeat full suites or regenerate evidence only for a failure, material change/risk, or an explicit acceptance/gate requirement.
- Documentation-only changes receive Markdown/reference checks, stale-state searches, and git diff --check. Match documentation and evidence to scope; do not create records just because they can be created.
- Acceptance checks still control completion. Report only checks actually performed and disclose self-verification. [VALIDATION](VALIDATION.md) gives commands and operational safeguards.

## Source control and cleanup

- Owner performs all Git commits. Do not stage, commit, push, or create/update/complete a PR. Leave changes reviewable and report actual Git status.
- Database/generated-artifact cleanup must use the existing guarded harness or scripts/test-artifacts.ps1 workflow; it is dry run by default and requires exact targets for execution. Never delete pre-existing or uncertain databases without owner authorization. See [local development](../docs/local-development.md) for operations.
- Do not mark items DONE/PASS or approved on the owner's behalf. Hand back for any required owner acceptance. [SESSION-CLOSEOUT](SESSION-CLOSEOUT.md) keeps reporting proportional.
