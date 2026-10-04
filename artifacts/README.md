# Historical review artifacts

`artifacts/` retains human-review material outside runtime code. `reviews/` holds historical owner and design review images in work-item folders that preserve provenance. These files do not authorize product behavior.

The [relocation inventory](reviews/relocation-inventory.csv) records each image's original path, new path, and SHA-256 hash.

Canonical verification evidence remains in [`ai-context/planning/verification/evidence/`](../ai-context/planning/verification/evidence/). Design references remain in [`claude-handoff/reference/`](../claude-handoff/reference/) and [`ai-context/references/ux/`](../ai-context/references/ux/). Generated E2E captures stay in the ignored `src/FamilySafety.Web/e2e/captures/` path and are not canonical evidence.
