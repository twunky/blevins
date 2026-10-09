# Project State — Orientation

Current baseline, phase, authorization, and next action live in [project status](../ai-context/project-status.md). This file describes stable implementation orientation only.

- Local-first MVP: .NET 10 API and Angular 22 client. Setup and test-artifact operations: [local development](../docs/local-development.md).
- Navigation: Home, Family, Activities, Safety, Me / Settings (REQ-UX-018 supersedes REQ-UX-013). Events, Plans, and Trips remain distinct domains and permission boundaries within Activities.
- Implemented areas include invitation-based Family entry and administration, Events with selected-person audience and in-app invitation/self-RSVP, Plans, Trips and Trip Notes, Concerns, and Safety Incidents. Phase closure covers approved slices, not every MVP capability; remaining work is in the [roadmap](../ai-context/planning/roadmap.md).
- Server authorization controls access. Selected Family is presentation context, and administrator or relationship status does not grant private-data access. Read the relevant canonical privacy/workflow rules through [routing](CONTEXT-ROUTING.md).
- Angular retains several screens in App, with standalone boundaries for growing features. [Client README](../src/FamilySafety.Web/README.md#structure-and-extraction) owns structure and extraction rules.
- The current dark/cyan visual system is defined by the [UI Visual Design Contract](../ai-context/ui-visual-design-contract.md) and implemented in src/FamilySafety.Web/src/styles.scss and scoped screen styles. Historical mockups cannot authorize behavior.

Production authentication, hosting, and backup/restore readiness remain separate from Development verification. Existing build warnings must be assessed against the actual work-item baseline; do not raise thresholds to hide them.
