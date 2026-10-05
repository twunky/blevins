# Context Routing

Read [AI operating rules](../ai-context/README.md) and [project status](../ai-context/project-status.md) first. Then select only sources relevant to the requested task. An active authorized work item's required-context manifest controls execution loading; completed manifests are historical.

## Canonical sources by task

| If working on | Read |
|---|---|
| Events | [Events workflow](../ai-context/workflows/events.md); [privacy contract](../ai-context/core/privacy-safety-contract.md), especially REQ-PRIV-020/021/023/027/028/029. Invitation/self-RSVP: REQ-EVT-014; Needs: REQ-EVT-011. |
| Plans | [Plans & Trips workflow](../ai-context/workflows/plans-trips.md), REQ-PLAN-* including REQ-PLAN-007; relevant privacy anchors. |
| Trips | [Plans & Trips workflow](../ai-context/workflows/plans-trips.md), REQ-TRIP-*; relevant Trip privacy anchors. |
| Family, profiles, membership, administration | [Product contract](../ai-context/core/product-contract.md), REQ-FAM-*; [privacy contract](../ai-context/core/privacy-safety-contract.md), especially REQ-PRIV-011/012. |
| Safety, Concerns, Incidents | [Safety workflow](../ai-context/workflows/safety.md), especially REQ-SAFE-015/016; [privacy contract](../ai-context/core/privacy-safety-contract.md), especially REQ-PRIV-025/026; relevant product rules. |
| Sharing / Check-Ins | [Sharing & Check-Ins workflow](../ai-context/workflows/sharing-checkins.md) and relevant privacy anchors; OD-002 in the [full decision log](../ai-context/decision-log.md#open-product-decisions) when location scope is involved. |
| Navigation, deep links, UX, accessibility | [UX contract](../ai-context/core/ux-contract.md), especially REQ-UX-011/013/014; REQ-PRIV-027 in the privacy contract. |
| Requirement lookup / terminology | [Requirements index](../ai-context/core/requirements-index.md) / [domain glossary](../ai-context/core/domain-glossary.md). |
| Phase scope / approval rationale | [Roadmap](../ai-context/planning/roadmap.md); only relevant entries in the [full decision log](../ai-context/decision-log.md). planning/decision-log.md is a partial excerpt. |

## Implementation and operations

| If working on | Read / inspect |
|---|---|
| Angular structure | [Client README](../src/FamilySafety.Web/README.md#structure-and-extraction). |
| Authorized UI presentation | [MOCKUP-ALIGNMENT](MOCKUP-ALIGNMENT.md) and its mapped primary images in reference/; secondary ai-context/references/ux images only for uncovered screens. |
| Feature code | Relevant paths under src/FamilySafety.Api/, Application/, Domain/, Infrastructure/, and Web/src/app/; find files for the selected domain. |
| Local setup / test artifacts | [Local development](../docs/local-development.md); scripts/test-artifacts.ps1 for guarded artifact operations. |
| Validation / closeout | [VALIDATION](VALIDATION.md) / [SESSION-CLOSEOUT](SESSION-CLOSEOUT.md), as needed. |

## Historical routing — only on demand

Do not normally load HISTORY.md, bootstrap/archive, old candidates/proposals, completed work-item manifests, verification reports/evidence, screenshots, raw transcripts, or generated output. For a specific provenance, acceptance, or regression question, read only the relevant decision, completed work item, or verification record. [HISTORY](HISTORY.md) retains earlier handoff checkpoints; do not copy canonical history into it.
