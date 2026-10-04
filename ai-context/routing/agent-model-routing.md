# Agent and Model Routing

**Status:** APPROVED  
**Applies to:** AI Context Foundation work and later story workflows

## Agent roles

### Coordinator/Planner

- Read `README.md` and `project-status.md` first.
- Classify the request, mode, authority, impact, and required context.
- Detect missing decisions, stale baselines, and conflicts before proposing work.
- Create a proposal for high-impact or ambiguous work; otherwise create an approval-ready story.
- Never convert an unresolved product question into an assumed requirement.

### Executor

- Require an approved story or proposal plus explicit execution authorization.
- Confirm the baseline and `required_context` are current.
- Perform only the authorized tasks and collect evidence.
- Stop for conflicts, missing decisions, scope expansion, or stale context.
- Hand work to verification rather than declaring unsupported completion.

### Verifier

- Evaluate the story, canonical requirement sources, acceptance criteria, outputs, and evidence independently of the executor's conclusions.
- Map each requirement and criterion to evidence and `PASS`, `FAIL`, `BLOCKED`, or justified `NOT APPLICABLE`.
- Do not change requirements to make work pass.
- Disclose whether the pass is `INDEPENDENT` or `SELF-VERIFIED`.

### Context Curator

- Require an accepted decision before changing canonical intent.
- Update affected canonical documents, requirement links, status, and supersession references together.
- Prevent duplicate normative wording and stale routing links.
- Keep bootstrap and archive material out of normal context.

## Model routing

| Tier | Use |
|---|---|
| `ROUTINE` | Extraction, formatting, ID checks, status maintenance, and tightly bounded mechanical work. Use the lowest-cost capable model and low reasoning. |
| `STANDARD` | Normal planning, execution, review, and verification requiring judgment and completeness. |
| `DEEP` | Cross-contract analysis, privacy or safety impact, permissions, ambiguous defects, difficult verification, or broad context. Use stronger reasoning. |
| `ESCALATION` | A human or product decision is required. A model may organize evidence and options but must not decide autonomously. |

Do not permanently bind tiers to model names. Select the least expensive available model and reasoning level that meets the correctness bar. Decompose work when routine portions can safely use a lower tier.

## Escalation rules

Route to `DEEP` or `ESCALATION` when work includes:

- Privacy, consent, access, safety, or permission impact.
- A proposed change to an approved contract or multiple workflows.
- Ambiguous defects with more than one plausible intended behavior.
- Technical architecture decisions not yet authorized.
- Conflicting authoritative context.
- Missing product decisions or unsupported assumptions.
- Failed verification whose resolution would change intended behavior.
- Repeated unsuccessful attempts indicating that the current tier is inadequate.

`BLOCKED—CONTEXT CONFLICT` always requires resolution before execution continues.

## Independence rules

- Story verification is a separate pass after execution.
- Prefer a different agent, reviewer, or isolated context for verification.
- Give the verifier the story, canonical sources, outputs, and evidence—not the executor's private reasoning.
- For privacy, safety, permissions, or broad-impact work, verification uses a tier appropriate to the risk and must not be weaker merely to reduce cost.
- If a separate verifier is unavailable, label the report `SELF-VERIFIED` and disclose that limitation.
- The dedicated project verification phase aggregates and extends story verification; it does not replace it.
