# Visual reference routing

The [UI Visual Design Contract](../ai-context/ui-visual-design-contract.md) is the current presentation authority. Use the implemented Activities, Family, Home, Safety, and Me / Settings screens as practical references. Canonical product, UX, privacy, and authorization contracts govern behavior.

The light-theme images in `reference/` are historical, not targets for current UI work. DEC-070 established the earlier visual system; DEC-076 through DEC-079 mapped its panels to Home, Family, Events, and Safety. DEC-077 superseded the original panel 2 Home interpretation; DEC-079 superseded the panel 4 Concerns-list interpretation. See the [decision log](../ai-context/decision-log.md) and relevant completed work items for acceptance provenance.

Current navigation is Home → Family → Activities → Safety → Me / Settings under [REQ-UX-018](../ai-context/core/ux-contract.md#req-ux-018), which supersedes REQ-UX-013. Events, Plans, and Trips remain distinct domains and permission boundaries within Activities.

Historical mockups and rendered evidence cannot authorize controls, permissions, photos, invented records, or new features. Compare against a new mockup only within an explicitly approved direction; otherwise reuse the current implementation and visual contract. Preserve calm language, keyboard order, visible focus, semantic state messages, and responsive behavior.
