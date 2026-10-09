# Historical review artifacts

`artifacts/` retains human-review material outside runtime code. `reviews/` holds historical owner and design review images in work-item folders that preserve provenance. These files do not authorize product behavior.

The [relocation inventory](reviews/relocation-inventory.csv) records each image's original path, new path, and SHA-256 hash.

The Phase 9 sets retain complete owner-review captures for [Family (WI-009-007)](reviews/WI-009-007/), [Events (WI-009-008)](reviews/WI-009-008/), [Safety (WI-009-009)](reviews/WI-009-009/), and [First Use (WI-009-010)](reviews/WI-009-010/), including variants beyond the selected canonical evidence.

Canonical verification evidence remains in [`ai-context/planning/verification/evidence/`](../ai-context/planning/verification/evidence/). Design references remain in [`claude-handoff/reference/`](../claude-handoff/reference/) and [`ai-context/references/ux/`](../ai-context/references/ux/). Generated E2E captures stay in the ignored `src/FamilySafety.Web/e2e/captures/` path and are not canonical evidence.

October 8 rollout/alignment comparison bundles and one-off profile verification captures were removed after consolidation into [project status](../ai-context/project-status.md). Historical owner-review sets above remain pending provenance/privacy review. They are not current design targets. Generated E2E capture filenames mentioned in old verification records describe past runs; the transient outputs may be cleared and regenerated and are not retained evidence links.
