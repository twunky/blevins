# Decision Log

This append-only log records semantic decisions. Canonical behavior changes are not operational until affected authoritative files are updated.

Entry fields: `ID`, `Status`, `Date`, `Decision`, `Rationale`, `Affected context`, `Supersedes`, and `Approved by`.

## DEC-001 — Mandatory entry point

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** Every AI session begins with `README.md` and `project-status.md`, then loads only task-relevant context.
- **Rationale:** Minimize token use while preserving continuity and governing rules.
- **Affected context:** `README.md`, `project-status.md`
- **Supersedes:** None
- **Approved by:** User approval in Phase 2A/2B

## DEC-002 — Authority and conflict handling

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** README governs AI/context behavior; approved core contracts govern product, UX, privacy, and safety truth; workflows govern workflow behavior under the contracts; stories and tasks authorize work without overriding higher authority. Conflicts block work.
- **Rationale:** Prevent silent reinterpretation and AI guessing.
- **Affected context:** All AI context
- **Supersedes:** None
- **Approved by:** User approval in Phase 2A/2B

## DEC-003 — Selective context loading

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** Do not load the context tree recursively. Use `required_context` manifests and exclude bootstrap, superseded, archived, and unrelated content from normal routing.
- **Rationale:** Reduce repeated tokens and context contamination.
- **Affected context:** `README.md`, templates, skills
- **Supersedes:** None
- **Approved by:** User approval in Phase 2A/2B

## DEC-004 — Requirement namespaces and traceability

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** Use the twelve namespaces defined in `core/requirements-index.md`; IDs are immutable and normative text has one canonical location.
- **Rationale:** Provide unambiguous traceability without duplicating requirements.
- **Affected context:** `core/requirements-index.md`, canonical contracts and workflows
- **Supersedes:** Earlier overlapping SAFE/SAFETY naming
- **Approved by:** User approval in Phase 2A/2B

## DEC-005 — Context baseline

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** Stories normally reference one approved context baseline, such as `CTX-1.0`, plus a small `required_context` manifest. Specific document versions are pinned only when necessary.
- **Rationale:** Preserve freshness checks without repeating the full version manifest in every story.
- **Affected context:** `README.md`, story template, project status
- **Supersedes:** Per-story duplication of every context document version
- **Approved by:** User approval in Phase 2A/2B

## DEC-006 — Proposal and Execution modes

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** Proposal Mode analyzes without application changes. Execution Mode performs only explicitly approved scope and returns to Proposal Mode when scope or authority changes.
- **Rationale:** Separate deliberation from authorized mutation.
- **Affected context:** `README.md`, skills, templates
- **Supersedes:** None
- **Approved by:** User approval in Phase 2A/2B

## DEC-007 — Agent and model routing

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** Use Coordinator/Planner, Executor, Verifier, and Context Curator roles with ROUTINE, STANDARD, DEEP, and ESCALATION capability tiers. Do not permanently bind tiers to model names.
- **Rationale:** Use the least expensive capable model while preserving judgment and independent review.
- **Affected context:** `routing/agent-model-routing.md`
- **Supersedes:** None
- **Approved by:** User approval in Phase 2A/2B

## DEC-008 — Four-skill limit

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** Create only `propose-work`, `execute-story`, `verify-story`, and `maintain-context`. A new skill requires evidence of a distinct repeatable workflow these cannot represent.
- **Rationale:** Keep skill discovery concise and avoid overlapping instructions.
- **Affected context:** `skills/`
- **Supersedes:** Proposed six-skill structure
- **Approved by:** User approval in Phase 2A/2B

## DEC-009 — Evidence-backed verification

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** Every story requires acceptance criteria and evidence. Independent verification is preferred; self-verification is labeled honestly. Project-level verification does not replace story-level verification.
- **Rationale:** Make completion auditable.
- **Affected context:** `README.md`, verification template and skill
- **Supersedes:** None
- **Approved by:** User approval in Phase 2A/2B

## DEC-010 — Temporary bootstrap source

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** `bootstrap/product-ux-source.md` is extraction evidence only. After verified canonical extraction and `CTX-1.0` approval, it has no authority, leaves normal routing, and may be archived.
- **Rationale:** Seed canonical context without creating a permanent competing authority.
- **Affected context:** `bootstrap/`, core contracts, workflows
- **Supersedes:** None
- **Approved by:** User approval in Phase 2A/2B

## DEC-011 — Technical architecture boundary

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** Technical architecture and application implementation do not begin during Phase 2B.
- **Rationale:** Preserve the approved phase boundary and avoid premature technology decisions.
- **Affected context:** All Phase 2 work
- **Supersedes:** None
- **Approved by:** User approval in Phase 2A/2B

## DEC-012 — Curated Product & UX bootstrap source

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** Use `bootstrap/product-ux-source.md` as the complete approved Product & UX extraction source for the proposed `CTX-1.0` baseline.
- **Rationale:** The curated source contains approved decisions while excluding brainstorming, superseded discussion, and technical architecture.
- **Affected context:** Core contracts, domain glossary, workflows, and requirements index
- **Supersedes:** The missing-source blocker recorded during initial Phase 2B setup
- **Approved by:** User execution authorization for canonical extraction

## DEC-013 — Approve CTX-1.0

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** Adopt the extracted core contracts, glossary, workflows, and requirement traceability as the active approved `CTX-1.0` baseline.
- **Rationale:** Canonical extraction completed, `VER-PH-002` passed its separate self-verification review, and the user explicitly approved the baseline.
- **Affected context:** All `CTX-1.0` canonical files
- **Supersedes:** Source-gated placeholder documents
- **Approved by:** Explicit user approval on 2026-09-24

## DEC-014 — Configurable Family Group capacity

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** Family Group membership capacity is controlled by application-operator configuration, not by Family Administrators. Use 50 members per Family Group as the initial planning target, 100 members as the default configured limit, and 250 members as the validated supported ceiling. Supporting more than 250 members in one Family Group requires later capacity validation.
- **Rationale:** Preserve dynamic configuration while giving architecture validation a concrete, bounded capacity target.
- **Affected context:** `core/product-contract.md`, `core/domain-glossary.md`, `core/requirements-index.md`, active context baseline
- **Supersedes:** `OD-005`
- **Approved by:** Explicit user approval of the recommended limit model and values

## DEC-015 — No microservices for the MVP

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** The MVP will not begin with microservices or separately deployed application services. Its Phase 3 proposal will use a modular-monolith direction unless a later, evidence-backed decision supersedes it.
- **Rationale:** The approved 50/100/250 Family Group capacity direction does not require distributed services, and a modular monolith reduces initial delivery, authorization, audit, operational, and long-term maintenance cost.
- **Affected context:** `planning/proposals/PRP-003-technical-architecture.md`, later Phase 3 architecture decisions
- **Supersedes:** None
- **Approved by:** Explicit user direction: “We dont need microservices now.”

## DEC-016 — Private invitation-only Family Group membership

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** MVP Family Group membership is invitation-only. A Family Administrator may issue and revoke an invitation for a specific intended person; membership activates only when that person explicitly accepts. Pending, expired, revoked, and already-used invitations grant no Family Group access. Public Family Group discovery, self-service join requests, and automatic membership are outside MVP.
- **Rationale:** Preserve the private-family posture and ensure that an account, family relationship, or administrative responsibility cannot become an accidental access path.
- **Affected context:** `core/product-contract.md`, `core/privacy-safety-contract.md`, `core/domain-glossary.md`, `core/requirements-index.md`, `planning/proposals/PRP-003-technical-architecture.md`
- **Supersedes:** None
- **Approved by:** Explicit user approval of the invitation-only membership direction

## DEC-017 — Private HTTP/JSON application API

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** The Angular client and C#/.NET modular-monolith application will use a private HTTP/JSON application API. It is not a microservice boundary, public developer API, partner API, or direct database interface. Protected operations enforce application authorization, privacy, Sharing Session, purpose, time-window, and audit rules server-side.
- **Rationale:** Maintain a clear, testable client/application boundary without adding distributed-system or public-integration scope.
- **Affected context:** `planning/proposals/PRP-003-technical-architecture.md`, later Phase 3 architecture decisions
- **Supersedes:** None
- **Approved by:** Explicit user approval of the private application API direction

## DEC-018 — Controlled first Family Group bootstrap

- **Status:** ACCEPTED
- **Date:** 2026-09-24
- **Decision:** The first Family Group and first Family Administrator will be created through controlled application-operator provisioning. The application operator may provision the initial private Family Group and initiate the first Family Administrator invitation. The intended first administrator must still sign in and explicitly accept before membership becomes active. Public self-service Family Group creation remains outside MVP.
- **Rationale:** The approved invitation-only membership model requires a first-administrator path without adding public Family Group discovery, self-service group creation, or operator access to private family content.
- **Affected context:** `core/product-contract.md`, `core/privacy-safety-contract.md`, `core/domain-glossary.md`, `core/requirements-index.md`, `planning/proposals/PRP-003-technical-architecture.md`
- **Supersedes:** None
- **Approved by:** Explicit user approval to continue with the controlled bootstrap recommendation

## DEC-019 — Local-first expandable architecture foundation

- **Status:** ACCEPTED
- **Date:** 2026-09-25
- **Decision:** The Phase 3 foundation direction is a local-first, expandable modular monolith using C#/.NET for the server, Angular for the responsive browser client, SQL Server as the development and MVP database candidate, a private HTTP/JSON application API, and no MVP microservices. Exact framework versions are deferred until implementation-decision time and must be verified against official support documentation, preferring the latest stable LTS or stable supported versions. Local development should use free or no-additional-cost tooling where legally permitted and practically sufficient. Production database cost must be verified before deployment.
- **Rationale:** This matches the project owner's existing skills, supports fast local progress, aligns with current official support patterns for .NET and Angular, avoids premature paid/provider commitments, and preserves future expansion through clear boundaries rather than early distributed-system complexity.
- **Affected context:** `planning/proposals/PRP-003-technical-architecture.md`, `project-status.md`
- **Supersedes:** None
- **Approved by:** Explicit user approval: “if that is the best pracftises and patterns for today, lets approve it”

## DEC-020 — Local development foundation and provider deferment

- **Status:** ACCEPTED
- **Date:** 2026-09-25
- **Decision:** The MVP foundation should support a mostly free local development workflow for the server, client, database candidate, and basic verification. Paid providers and production services are deferred until separate bounded decisions. Provider-specific behavior must stay behind replaceable boundaries for identity, notifications, mapping/location, file/media storage if later needed, background work, real-time delivery, hosting, deployment, and production database operation. Local-only substitutions must be explicit and must not hide production privacy, security, audit, retention, or reliability requirements.
- **Rationale:** This allows simple local progress now while preserving future expansion, cost control, and provider flexibility.
- **Affected context:** `planning/proposals/PRP-003-technical-architecture.md`, `project-status.md`
- **Supersedes:** None
- **Approved by:** Explicit user approval: “lets go with these recomendations and continue”

## DEC-021 — Authentication deferred and application authorization owned internally

- **Status:** ACCEPTED
- **Date:** 2026-09-25
- **Decision:** Identity-provider selection is deferred. Authentication must remain behind a replaceable boundary that establishes the signed-in account identity. Product authorization is owned by the C#/.NET application, not by the external identity provider, Angular client, database, or Family Administrator role. The application must evaluate Family Group membership, invitations, Safety Circle, Sharing Sessions, privacy settings, purpose, time windows, incident participation, and audit rules server-side. Local development may use a clearly labeled development authentication substitute only if it cannot be mistaken for production behavior and does not bypass application authorization.
- **Rationale:** The product's privacy and access rules are domain-specific and cannot safely be delegated to sign-in alone. Deferring the provider preserves local development speed and future provider flexibility without weakening authorization.
- **Affected context:** `planning/proposals/PRP-003-technical-architecture.md`, `project-status.md`
- **Supersedes:** None
- **Approved by:** User direction to continue after the `DEC-021` recommendation

## DEC-022 — Phase 3 architecture closure and Phase 4 readiness gates

- **Status:** ACCEPTED
- **Date:** 2026-09-25
- **Decision:** Phase 3 Technical Architecture is closed as an approved proposal with readiness gates for Phase 4 MVP Foundation. Phase 4 may begin only with separate execution authorization and an approved first story. The first Phase 4 story must preserve `CTX-1.3`, the accepted Phase 3 decisions, local-first development, replaceable provider boundaries, internal application authorization, and story-level verification. Phase 4 may select exact framework/tool versions only by verifying current official support documentation at the time of execution. It must not select paid providers, production hosting, production database licensing, deployment, public APIs, native mobile, microservices, or unresolved product behavior unless separately proposed and approved.
- **Rationale:** This provides a clean handoff from architecture proposal to implementation planning without accidentally starting implementation or guessing unresolved decisions.
- **Affected context:** `planning/proposals/PRP-003-technical-architecture.md`, `planning/roadmap.md`, `project-status.md`, `planning/verification/VER-PH-003.md`
- **Supersedes:** None
- **Approved by:** User direction: “ok, lets do it now”

## DEC-023 — Best-practices and human-readable work standard

- **Status:** ACCEPTED
- **Date:** 2026-09-25
- **Decision:** All project work must follow current best practices and established patterns appropriate to the selected technology, product risk, and project phase. All artifacts, proposals, stories, code, tests, and verification evidence must be human-readable, maintainable, and understandable by a future developer.
- **Rationale:** The project should stay functional, sustainable, and easy to reason about as it moves from context and proposals into implementation.
- **Affected context:** `README.md`, `core/requirements-index.md`, future proposals, stories, implementation, tests, and verification
- **Supersedes:** None
- **Approved by:** User direction: “as a rule... everything needs to follow best practices and patterns. It needs to be human readbale.”

## DEC-024 — Required context manifest discipline

- **Status:** ACCEPTED
- **Date:** 2026-09-25
- **Decision:** Each story must use a small, explicit `required_context` manifest that explains why each context file or anchor is needed and when it should be loaded. Stories must not load the whole `ai-context/` tree. Unneeded contracts, workflows, proposals, bootstrap files, archive files, and historical verification reports should be intentionally excluded. If broader context becomes necessary during work, the specific file and reason must be added to the story or reported before continuing.
- **Rationale:** Preserve memory and token efficiency while still giving each task enough authority and traceability to avoid guessing.
- **Affected context:** `README.md`, `templates/story.md`, `core/requirements-index.md`, `project-status.md`, future stories and verification
- **Supersedes:** None
- **Approved by:** User direction to make context and token usage efficient

## DEC-025 — Lightweight personal-project work items

- **Status:** ACCEPTED
- **Date:** 2026-09-25
- **Decision:** Phase 4 and later personal-project work should use lightweight Work Items by default instead of sprint-heavy story planning. A Work Item must still define one bounded goal, narrow required context with reasons, explicit non-goals, acceptance checks, execution authorization, and verification evidence. Formal stories may still be used when complexity, risk, or coordination warrants them.
- **Rationale:** Keep the project practical for a personal build while preserving the safeguards that prevent guessing, context overload, and unverifiable work.
- **Affected context:** `README.md`, `templates/work-item.md`, `planning/stories/`, `project-status.md`, future planning and verification
- **Supersedes:** Heavyweight sprint-style story process for routine personal-project work
- **Approved by:** User direction to use a lighter next step/item/process

## DEC-026 — Phase 4 closure and Phase 5 proposal entry

- **Status:** ACCEPTED
- **Date:** 2026-09-25
- **Decision:** Close Phase 4 — MVP Foundation after `WI-004-001` reached `DONE` with passing build, test, runtime, formatting, vulnerability, scope, and self-verification evidence. Advance Phase 5 — Family & Profiles to `CURRENT` in Proposal Mode. Phase 5 implementation remains unauthorized until a privacy- and permissions-aware proposal is approved, an implementation-ready work item is approved, and execution is separately authorized.
- **Rationale:** The approved local-first foundation is complete and verified. Family and profile implementation introduces unresolved field, visibility, identity, invitation, household, relationship, and minor/guardian questions that must not be guessed through.
- **Affected context:** `project-status.md`, `planning/roadmap.md`, `planning/verification/VER-PH-004.md`, `planning/proposals/PRP-005-family-profiles-foundation.md`
- **Supersedes:** Phase 4 `CURRENT` status
- **Approved by:** Project owner direction to close Phase 4 if complete and move to the next phase

## DEC-027 — Adult self-profile first Phase 5 sequence

- **Status:** ACCEPTED
- **Date:** 2026-09-25
- **Decision:** Use Option A from `PRP-005`: begin Phase 5 with a bounded adult Account-to-Family-Member self-profile foundation. Preserve separate Account, membership, Family Administrator, operator, Household, relationship, and Safety Circle concepts. Defer invitation delivery, operator bootstrap mechanics, Household behavior, relationship taxonomy, Safety Circle behavior, and all minor/guardian behavior to later decisions and work items.
- **Rationale:** This is the smallest privacy-reviewable Family/Profile slice and avoids forcing unresolved identity, invitation, household, relationship, or guardian behavior into the first implementation.
- **Affected context:** `planning/proposals/PRP-005-family-profiles-foundation.md`, `planning/proposals/PRP-006-adult-profile-contract.md`, `project-status.md`, later Phase 5 work items
- **Supersedes:** None
- **Approved by:** Explicit project-owner approval of Option A

## DEC-028 — Minimal adult profile field and visibility contract

- **Status:** ACCEPTED
- **Date:** 2026-09-25
- **Decision:** Accept Option A from `PRP-006`. The first adult self-profile slice contains a required display name and optional contact email, contact phone, and birthday month/day. Display name is visible only to active members of the same Family Group. Optional fields default to Only me and may be shared with the active Family Group only through an explicit per-field choice. The adult profile owner alone edits these values and visibility. Family Administrators and Application Operators receive no edit or visibility override. Authentication identifiers are not automatically exposed as profile contact fields; server responses omit unauthorized values; inactive/former membership ends access; public and other unapproved visibility modes are excluded; and minor/guardian paths remain unavailable.
- **Rationale:** This is the smallest useful profile contract that advances approved Family, contact, birthday, and privacy capabilities while minimizing personal-data collection and authorization surface.
- **Affected context:** `core/product-contract.md`, `core/privacy-safety-contract.md`, `core/domain-glossary.md`, `core/requirements-index.md`, `planning/proposals/PRP-006-adult-profile-contract.md`, candidate `CTX-1.4`
- **Supersedes:** None
- **Approved by:** Explicit project-owner approval of `PRP-006`

## DEC-029 — Approve CTX-1.4

- **Status:** ACCEPTED
- **Date:** 2026-09-25
- **Decision:** Approve and activate `CTX-1.4`. Promote `REQ-FAM-007` and `REQ-PRIV-013`, together with the Adult Profile and Profile Field Visibility glossary definitions, from candidate content to authoritative canonical context. This baseline approval does not authorize Phase 5 application implementation.
- **Rationale:** The adult-profile amendment was accepted in `DEC-028`, faithfully extracted, traced with immutable requirement IDs, and passed `SELF-VERIFIED` context verification in `VER-CTX-1.4`; the project owner then explicitly approved `CTX-1.4`.
- **Affected context:** `README.md`, `project-status.md`, `core/product-contract.md`, `core/privacy-safety-contract.md`, `core/domain-glossary.md`, `core/requirements-index.md`, `planning/roadmap.md`, `planning/verification/VER-CTX-1.4.md`
- **Supersedes:** `CTX-1.3` as the active aggregate context baseline; historical `CTX-1.3` records remain valid audit evidence
- **Approved by:** Explicit project-owner approval of `CTX-1.4`

## DEC-030 — Adult membership invitation lifecycle

- **Status:** ACCEPTED
- **Date:** 2026-09-25
- **Decision:** Accept Option A from `PRP-007`. MVP adult membership invitations use a provider-neutral email delivery boundary and an opaque one-time link that expires after seven days. A clearly labeled local outbox may be used only for Development; no production email provider is selected. Only one pending invitation may exist for the same normalized recipient email and Family Group; resending revokes the prior invitation and creates a new seven-day invitation. Declined, revoked, expired, and used invitations cannot be reactivated. The adult recipient must sign in, see the Family Group name and inviter display name, and explicitly accept or decline. Acceptance binds the invitation to the current account and atomically creates at most one active membership only while the invitation is valid and capacity remains available. Invitation delivery data remains separate from profile contact data and authentication identifiers. Tokens must be strong, stored only as hashes, disclose no private group data in links, and use uniform unavailable responses for invalid or terminal states. Long-term invitation-history retention, controlled first-administrator operator mechanics, production identity, and production delivery providers remain deferred.
- **Rationale:** This is the smallest familiar adult onboarding flow that works locally without paid providers while preserving replaceable production boundaries, explicit consent, application-owned authorization, capacity enforcement, and private-family access rules.
- **Affected context:** `planning/proposals/PRP-007-adult-membership-invitations.md`, `core/product-contract.md`, `core/privacy-safety-contract.md`, `core/domain-glossary.md`, `core/requirements-index.md`, candidate `CTX-1.5`
- **Supersedes:** None
- **Approved by:** Explicit project-owner approval of `PRP-007`

## DEC-031 — Approve CTX-1.5

- **Status:** ACCEPTED
- **Date:** 2026-09-25
- **Decision:** Approve and activate `CTX-1.5`. Promote `REQ-FAM-008`, `REQ-PRIV-014`, and the updated Membership Invitation glossary definition from candidate content to authoritative canonical context. This baseline approval does not authorize Phase 5 application implementation.
- **Rationale:** The adult invitation amendment was accepted in `DEC-030`, faithfully recorded with immutable requirement IDs, and passed `SELF-VERIFIED` candidate-context verification in `VER-CTX-1.5`; the project owner then explicitly approved `CTX-1.5`.
- **Affected context:** `README.md`, `project-status.md`, `core/product-contract.md`, `core/privacy-safety-contract.md`, `core/domain-glossary.md`, `core/requirements-index.md`, `planning/roadmap.md`, `planning/verification/VER-CTX-1.5.md`
- **Supersedes:** `CTX-1.4` as the active aggregate context baseline; historical `CTX-1.4` records remain valid audit evidence
- **Approved by:** Explicit project-owner approval of `CTX-1.5`

## DEC-032 — Bounded standing auto-approval for routine work

- **Status:** ACCEPTED
- **Date:** 2026-09-25
- **Decision:** Prospectively allow recommended options, low-impact proposals, and work items to be accepted automatically, with work-item approval and execution authorization combined when useful, only for `ROUTINE` or `STANDARD` work that is complete, unambiguous, local, reversible, and wholly within approved scope and authority. Each use must be recorded and verified. Explicit project-owner approval remains mandatory for `DEEP` or `ESCALATION` work; product/MVP or canonical changes; context baselines; privacy, safety, permissions, identity, retention, architecture, provider, cost, external, destructive, phase-transition, missing-decision, ambiguity, or conflict concerns. Standing authorization never relaxes no-guessing, authority, scope, or verification rules and never applies retroactively.
- **Rationale:** Reduce repetitive approval turns and context reloads for safe routine work while preserving deliberate human control over consequential decisions.
- **Affected context:** `README.md`, `core/requirements-index.md`, `project-status.md`, future proposals and work items
- **Supersedes:** Repeated separate approval turns for qualifying routine work only
- **Approved by:** Explicit project-owner approval of the bounded auto-approval recommendation

## DEC-033 — Minimal adult Family directory

- **Status:** ACCEPTED
- **Date:** 2026-09-25
- **Decision:** Accept Option A from `PRP-008`. The first adult Family directory presents a searchable, predictably ordered index containing only profile ID and display name for active adult members of a Family Group in which the current account has active membership. Selecting a member loads profile details on demand through the existing server-side profile-visibility rules. A Family Group selector appears only when the current adult belongs to multiple active groups. The directory must not expose private optional values, sign-in or invitation identifiers, visibility settings, administrator assignments, capacity, inactive/former members, unrelated groups, or undefined relationship, Household, Safety Circle, or minor/guardian behavior.
- **Rationale:** This is the smallest useful next Family-area slice. It operationalizes completed membership and profile foundations, minimizes private-data exposure, supports larger families, and avoids coupling simple member discovery to undefined higher-risk concepts.
- **Affected context:** `planning/proposals/PRP-008-adult-family-directory.md`, `planning/stories/WI-005-003-adult-family-directory.md`, `project-status.md`
- **Supersedes:** None
- **Approved by:** Explicit project-owner approval of `PRP-008 Option A`

## DEC-034 — Mutually confirmed adult Family relationships

- **Status:** ACCEPTED
- **Date:** 2026-09-26
- **Decision:** Accept Option A from `PRP-009`. An active adult may propose one structured Family Relationship to another active adult in the same Family Group using the reciprocal labels spouse/partner, parent/adult child, sibling, or other family. The recipient must explicitly accept before the relationship becomes visible to other active adults in that group. Pending proposals remain visible only to their participants. The proposer may withdraw a pending proposal; the recipient may accept or decline it; either participant may remove an accepted relationship; and changing a label requires removal followed by a new mutually confirmed proposal. Relationships grant no permissions, profile visibility, membership, administrator responsibility, Household or Safety Circle membership, location access, guardian authority, or other access. Family Administrators and Application Operators have no override over another adult's relationship.
- **Rationale:** This is the smallest shared adult relationship model that provides understandable family context while preserving mutual consent, adult autonomy, narrow group visibility, and the approved separation between descriptive relationships and authorization.
- **Affected context:** `planning/proposals/PRP-009-adult-family-relationships.md`, `core/product-contract.md`, `core/privacy-safety-contract.md`, `core/domain-glossary.md`, `core/requirements-index.md`, candidate `CTX-1.6`
- **Supersedes:** None
- **Approved by:** Explicit project-owner approval of the recommended option

## DEC-035 — Approve CTX-1.6

- **Status:** ACCEPTED
- **Date:** 2026-09-26
- **Decision:** Approve and activate `CTX-1.6`. Promote `REQ-FAM-009`, `REQ-PRIV-015`, and the Family Relationship glossary definition from candidate content to authoritative canonical context. This baseline approval does not independently authorize application implementation; any implementation must remain within an approved and authorized work item.
- **Rationale:** The adult Family Relationship amendment was accepted in `DEC-034`, recorded with immutable requirement IDs, and passed independent candidate-context verification in `VER-CTX-1.6`; the project owner explicitly approved `CTX-1.6`.
- **Affected context:** `README.md`, `project-status.md`, `core/product-contract.md`, `core/privacy-safety-contract.md`, `core/domain-glossary.md`, `core/requirements-index.md`, `planning/roadmap.md`, `planning/verification/VER-CTX-1.6.md`
- **Supersedes:** `CTX-1.5` as the active aggregate context baseline; historical `CTX-1.5` records remain valid audit evidence
- **Approved by:** Explicit project-owner approval of `CTX-1.6`

## DEC-036 — Shared Family Dates proposal direction

- **Status:** ACCEPTED — DIRECTION ONLY
- **Date:** 2026-09-26
- **Decision:** Accept Option A from `PRP-010` as the next Phase 5 proposal scope: shared birthdays or important dates only. Trusted contacts, Events, Household/basic grouping, Safety Circle, and implementation remain outside this decision. Record as proposed rules that only active members of the relevant Family Group may be associated with a date; the creator and members specifically associated with it may edit; and only the creator may delete while the creator remains active in that group.
- **Rationale:** A narrow shared-date scope is the smallest next capability under consideration and preserves the owner's preference for a simple creator/participant collaboration rule without expanding the role model.
- **Affected context:** `planning/proposals/PRP-010-shared-family-dates.md`, `project-status.md`
- **Open decisions:** Date visibility; who may create or change associations; edit-field scope; what happens when the creator leaves; how associations behave when members leave; and whether shared birthdays use profile birthday data or separate date entries.
- **Canonical effect:** None. The accepted rules remain proposal-level and are not authoritative product requirements until unresolved behavior is decided and canonical context is explicitly updated.
- **Execution authorization:** None. No implementation is authorized.
- **Approved by:** Explicit project-owner approval of `PRP-010` direction

## DEC-037 — Shared-date configuration decision framework

- **Status:** ACCEPTED — DECISION FRAMEWORK ONLY
- **Date:** 2026-09-26
- **Decision:** Accept `PRP-011` as the framework for deciding whether shared-date rules are fixed, Family Administrator-configured, or selected by each date creator. Do not create or infer a “main family group member” role. This decision does not select an option, appoint a settings manager, establish configurable settings, change creator/associated-member rights, update canonical requirements, or authorize implementation.
- **Rationale:** The configurability question merits explicit comparison, while its authority and privacy implications remain unresolved.
- **Affected context:** `planning/proposals/PRP-011-configurable-shared-date-rules.md`, `project-status.md`
- **Open decisions:** Configuration owner; configurable controls; default versus per-date scope; effect on existing dates; consent/notice; administrator turnover; and interaction with creator deletion/editing rules.
- **Canonical effect:** None. All configuration behavior remains non-authoritative until separately decided and incorporated into canonical context.
- **Execution authorization:** None.
- **Approved by:** Explicit project-owner approval of `PRP-011` as a proposal framework

## DEC-038 — Fixed v1 Shared Family Date rules

- **Status:** ACCEPTED
- **Date:** 2026-09-26
- **Decision:** Accept the fixed v1 shared-date rule package in `PRP-011`. Shared dates are visible only to their creator and specifically associated active adult members of the same Family Group. The creator alone manages associations, and only active members of that group may be associated. The creator and associated active members may edit date content; only the creator may delete while active. Ended membership ends access; associated-member associations are removed without automatic restoration. When the creator becomes inactive, the date is unavailable in normal Family views and is not transferred. A profile birthday appears only under its existing explicit Active Family Group visibility and is not duplicated as a separate date. No Family Group configuration or “main family group member” role is introduced in v1.
- **Rationale:** A fixed, narrow v1 is simpler and more privacy-preserving than adding configurable authority before family needs are known. Configurability can be considered later through a separate proposal.
- **Affected context:** `planning/proposals/PRP-010-shared-family-dates.md`, `planning/proposals/PRP-011-configurable-shared-date-rules.md`, `core/product-contract.md#req-fam-010`, `core/privacy-safety-contract.md#req-priv-016`, `core/domain-glossary.md#shared-family-date`, `core/requirements-index.md`, `project-status.md`, `planning/verification/VER-CTX-1.7.md`, `planning/verification/VER-CTX-1.7-ACTIVATION.md`
- **Open items:** Exact date-entry fields and retention/cleanup behavior remain undecided and must be resolved before implementation.
- **Canonical effect:** Requirements were proposed in `CTX-1.7` and became authoritative when `DEC-039` approved and activated the baseline.
- **Execution authorization:** Canonical preparation and verification were authorized; no application implementation.
- **Approved by:** Explicit project-owner approval of the recommended v1 package

## DEC-039 — Approve and activate CTX-1.7

- **Status:** ACCEPTED
- **Date:** 2026-09-26
- **Decision:** Approve and activate `CTX-1.7`. Promote `REQ-FAM-010`, `REQ-PRIV-016`, and the Shared Family Date glossary definition to authoritative canonical context. The fixed v1 rules in `DEC-038` are now active. This baseline approval does not authorize application implementation.
- **Rationale:** The project owner explicitly approved the shared-date candidate baseline after candidate context verification passed in `VER-CTX-1.7`.
- **Affected context:** `README.md`, `project-status.md`, `decision-log.md`, `core/product-contract.md`, `core/privacy-safety-contract.md`, `core/domain-glossary.md`, `core/requirements-index.md`, `planning/proposals/PRP-010-shared-family-dates.md`, `planning/proposals/PRP-011-configurable-shared-date-rules.md`, `planning/verification/VER-CTX-1.7-ACTIVATION.md`
- **Supersedes:** `CTX-1.6` as the active aggregate context baseline; historical `CTX-1.6` records remain valid audit evidence.
- **Open items:** Exact shared-date content fields and retention/cleanup behavior remain undecided and block implementation planning.
- **Approved by:** Explicit project-owner approval of `CTX-1.7`

## DEC-040 — Shared Family Date limited retention

- **Status:** ACCEPTED — RETENTION DIRECTION; DURATION OPEN
- **Date:** 2026-09-26
- **Decision:** For shared Family Date data only, loss of active membership by any associated member or creator deletion immediately removes the affected entry from normal Family views for all Family Group members. Retained copies are not available to Family Group members through normal product behavior. Retention must be for an explicitly approved finite period and the retained data must be deleted when that period ends. No indefinite retention is approved. This does not change visibility or retention for Adult Profile birthdays shown under existing profile-field rules.
- **Rationale:** Separate access revocation and normal-view deletion from bounded record retention, while keeping retained copies out of family-member reach and avoiding indefinite sensitive-data retention.
- **Affected context:** Candidate `CTX-1.8` amendment to `core/product-contract.md#req-fam-010`, `core/privacy-safety-contract.md#req-priv-017`, `core/requirements-index.md`, `project-status.md`, and `planning/verification/VER-CTX-1.8.md`
- **Open items:** Exact finite retention duration and associated audit/cleanup details are not decided and block implementation.
- **Canonical effect:** Approved and active under `CTX-1.8` after the project owner's explicit approval on 2026-09-26.
- **Execution authorization:** Candidate context preparation and verification only; no application implementation.
- **Approved by:** Explicit project-owner approval of the limited-retention recommendation

## DEC-041 — Approve and activate CTX-1.8

- **Status:** ACCEPTED
- **Date:** 2026-09-26
- **Decision:** Approve and activate context baseline `CTX-1.8`, including the shared Family Date retention boundary in `REQ-PRIV-017` and its related cross-reference in `REQ-FAM-010`.
- **Rationale:** The project owner explicitly approved the verified candidate baseline after reviewing `VER-CTX-1.8`.
- **Affected context:** `README.md`, `project-status.md`, `decision-log.md`, `core/product-contract.md`, `core/privacy-safety-contract.md`, `core/requirements-index.md`, and `planning/verification/VER-CTX-1.8-ACTIVATION.md`.
- **Open items:** Exact shared-date fields, finite retention duration, and any audit/cleanup details remain undecided and block implementation planning for those behaviors.
- **Canonical effect:** `CTX-1.8` is authoritative. `CTX-1.7` remains an approved historical baseline, superseded by `CTX-1.8` for current context.
- **Execution authorization:** Activate and verify the approved context baseline only; no application implementation.
- **Approved by:** Explicit project-owner approval of `CTX-1.8`

## DEC-042 — Shared Family Date fields and 30-day retention proposal

- **Status:** ACCEPTED RECOMMENDATION; CANONICAL CANDIDATE ONLY
- **Date:** 2026-09-26
- **Decision:** The project owner approved a shared-date title, description, and date field; the title and date are required, while description requiredness remains open. Notes distinct from the description, attachments, notifications, and other extra content are excluded from v1. Retain a shared-date record for 30 calendar days after it becomes unavailable in normal Family views, then delete the retained date data. The date value is a calendar date; whether the year is significant and whether a date recurs annually remain open. Audit records, technical backups, and cleanup mechanics are not defined by this decision.
- **Rationale:** A small date-oriented field set and short, explicit finite retention period provide a bounded v1 direction without inferring recurrence behavior or technical cleanup behavior.
- **Affected context:** Candidate `CTX-1.9` updates to `core/product-contract.md#req-fam-011`, `core/privacy-safety-contract.md#req-priv-018`, `core/requirements-index.md`, `project-status.md`, and `planning/verification/VER-CTX-1.9.md`.
- **Open items:** Description requiredness, year significance, annual recurrence, and audit/backup/cleanup implementation remain undecided and block implementation planning for the affected behavior.
- **Canonical effect:** The recommendation became authoritative under `CTX-1.9` after explicit approval and activation recorded in `DEC-043`.
- **Execution authorization:** Candidate context preparation and verification only; no application implementation.
- **Approved by:** Explicit project-owner approval of the recommendation

## DEC-043 — Approve and activate CTX-1.9

- **Status:** ACCEPTED
- **Date:** 2026-09-26
- **Decision:** Approve and activate context baseline `CTX-1.9`, including the shared-date name/date-value minimum and 30-calendar-day retention period.
- **Rationale:** The project owner explicitly approved `CTX-1.9` after reviewing its candidate verification.
- **Affected context:** `README.md`, `project-status.md`, `decision-log.md`, `core/product-contract.md`, `core/privacy-safety-contract.md`, `core/requirements-index.md`, `planning/proposals/PRP-012-shared-date-fields-and-retention.md`, and `planning/verification/VER-CTX-1.9-ACTIVATION.md`.
- **Open items:** The date value's calendar semantics (including year and annual recurrence) and the handling of audit records, backups, and cleanup remain to be clarified. No implementation is authorized.
- **Canonical effect:** `CTX-1.9` is authoritative. `CTX-1.8` remains an approved historical baseline, superseded for current context.
- **Execution authorization:** Activate and verify the approved context baseline only; no application implementation.
- **Approved by:** Explicit project-owner approval of `CTX-1.9`

## DEC-044 — Expiration metadata direction for audit, backup, and cleanup records

- **Status:** ACCEPTED DIRECTION; DETAILS OPEN
- **Date:** 2026-09-26
- **Decision:** The project owner directed that audit records, backup-related records/data, and cleanup-related records should carry an end/expiration value. This is metadata direction only: no scheduled deletion, movement, purge task, cleanup process, retention duration, or technical implementation is authorized or defined by this decision.
- **Rationale:** Record an intended end point without prematurely defining operations or implementation.
- **Affected context:** Future candidate context for applicable retention requirements; exact data categories, meaning and ownership of the end value, and duration remain unspecified.
- **Open items:** Clarify which records/data the end value applies to, whether one or separate values are intended, and how this direction relates to the approved 30-day shared-date rule. Do not infer or implement.
- **Canonical effect:** Decision direction only; no new requirement or baseline amendment until its scope is clarified.
- **Execution authorization:** None beyond recording this decision direction.
- **Approved by:** Explicit project-owner instruction

## DEC-045 — Single-date shared-date behavior for v1

- **Status:** ACCEPTED
- **Date:** 2026-09-26
- **Decision:** The project owner clarified that v1 shared dates represent a single specific calendar date including its year, with no automatic recurrence. A future direction toward standard-calendar behavior is noted, but it does not authorize recurring dates or other calendar capabilities in v1.
- **Rationale:** Keep the first slice narrow and avoid adding recurrence behavior while preserving the owner's longer-term product direction.
- **Affected context:** Candidate `CTX-1.10` update to `core/product-contract.md#req-fam-012`, `core/requirements-index.md`, `project-status.md`, and `planning/verification/VER-CTX-1.10.md`.
- **Open items:** Audit/backup expiration metadata scope and duration remain unspecified; cleanup behavior is deferred. No automated cleanup is defined.
- **Canonical effect:** Approved and active under `CTX-1.10` after explicit owner approval recorded in `DEC-046`.
- **Execution authorization:** Context baseline activation only; no application implementation.
- **Approved by:** Explicit project-owner clarification

## DEC-046 — Approve and activate CTX-1.10

- **Status:** ACCEPTED
- **Date:** 2026-09-26
- **Decision:** Approve and activate `CTX-1.10`, including one specific calendar date with year and no automatic recurrence for shared-date v1, and require the description field.
- **Rationale:** The project owner explicitly approved `CTX-1.10` and resolved description requiredness. Cleanup behavior remains intentionally deferred.
- **Affected context:** `README.md`, `project-status.md`, `decision-log.md`, `core/product-contract.md`, `core/requirements-index.md`, and `planning/verification/VER-CTX-1.10-ACTIVATION.md`.
- **Open items:** Audit/backup expiration metadata scope and duration remain undefined. Cleanup behavior is deferred to a later decision. No application implementation is authorized.
- **Canonical effect:** `CTX-1.10` is authoritative. `CTX-1.9` remains an approved historical baseline, superseded for current context.
- **Execution authorization:** Activate and verify the approved context baseline only; no application implementation.
- **Approved by:** Explicit project-owner approval of `CTX-1.10` and required description field

## DEC-047 — Accept Option A Trusted Contacts direction

- **Status:** ACCEPTED DIRECTION; CANONICAL CANDIDATE ONLY
- **Date:** 2026-09-26
- **Decision:** Accept Option A from `PRP-013`: each adult manages their own Trusted Contact entries; entries are private to that adult by default; any sharing or designation for a defined use is separate and explicit. Merely listing someone does not authorize notifications or grant them Family Group data access. Trusted Contacts remain distinct from profile contact fields and Safety Circle membership.
- **Rationale:** This follows the approved individual privacy and autonomy boundaries while keeping the capability separate from existing concepts.
- **Affected context:** Candidate `CTX-1.11` updates to `core/product-contract.md#req-fam-014`, `core/privacy-safety-contract.md#req-priv-019`, `core/requirements-index.md`, `project-status.md`, `planning/proposals/PRP-013-trusted-contacts.md`, and `planning/verification/VER-CTX-1.11.md`.
- **Open items:** Contact fields, whether non-members may be listed, sharing audiences, defined uses, contact consent expectations, access lifecycle, and retention remain unresolved.
- **Canonical effect:** Trusted Contacts direction became authoritative under `CTX-1.12` after explicit baseline approval in `DEC-049`.
- **Execution authorization:** Candidate context preparation and verification only; no application implementation.
- **Approved by:** Explicit project-owner approval of Option A

## DEC-048 — Accept explicit item-audience selection proposal

- **Status:** ACCEPTED; CANONICAL CANDIDATE ONLY
- **Date:** 2026-09-26
- **Decision:** Approve Option A in `PRP-014` for Events, Plans, and Trips: when creating one of these items, its creator explicitly selects whether to share it with the applicable Family Group as a whole, specific people, or both. This audience choice concerns the item only and does not authorize location sharing, notifications, or access to unrelated private data. Existing narrower feature-specific rules remain controlling. The project owner defers recipient eligibility, multiple-group behavior, later audience changes, and access after membership changes to a later phase or decision.
- **Rationale:** A consistent explicit audience choice supports intentional sharing while retaining existing item-specific privacy boundaries and deferring unnecessary policy detail.
- **Affected context:** Candidate `CTX-1.12` updates to `core/privacy-safety-contract.md#req-priv-020`, `core/requirements-index.md`, `workflows/events.md`, `workflows/plans-trips.md`, `planning/proposals/PRP-014-item-audience-selection.md`, `project-status.md`, and `planning/verification/VER-CTX-1.12.md`.
- **Open items:** Deferred questions remain unresolved and must be decided before implementing behavior that depends on them. Shared Family Dates are excluded under their narrower approved rules. Trusted Contacts remain a separate candidate scope.
- **Canonical effect:** The audience-selection rule became authoritative under `CTX-1.12` after explicit baseline approval in `DEC-049`.
- **Execution authorization:** Candidate context preparation and verification only; no application implementation.
- **Approved by:** Explicit project-owner approval of `PRP-014` Option A

## DEC-049 — Approve and activate CTX-1.12

- **Status:** ACCEPTED
- **Date:** 2026-09-26
- **Decision:** Approve and activate `CTX-1.12`, including the previously accepted Trusted Contacts Option A direction and explicit creator-selected audience for Events, Plans, and Trips.
- **Rationale:** The project owner explicitly approved the verified candidate baseline.
- **Affected context:** `README.md`, `project-status.md`, `decision-log.md`, product/privacy contracts, Events and Plans/Trips workflows, requirements index, `PRP-013`, `PRP-014`, and `planning/verification/VER-CTX-1.12-ACTIVATION.md`.
- **Open items:** Recipient eligibility, multiple-group behavior, later audience changes, and access after membership changes remain deferred. Trusted Contact fields, non-member contacts, audiences, uses, consent expectations, lifecycle, and retention remain open. No application implementation is authorized.
- **Canonical effect:** `CTX-1.12` is authoritative. `CTX-1.10` remains an approved historical baseline, superseded for current context.
- **Execution authorization:** Activate and verify the approved context baseline only; no application implementation.
- **Approved by:** Explicit project-owner approval of `CTX-1.12`

## DEC-050 — Close Phase 5 and advance to Phase 6

- **Status:** ACCEPTED
- **Date:** 2026-09-26
- **Decision:** Close Phase 5 — Family & Profiles and advance Phase 6 — Events to `CURRENT`. The four approved Phase 5 work items (`WI-005-001` through `WI-005-004`) are complete with passing work-item verification. Deferred Trusted Contact details, minor/guardian behavior, and other explicitly deferred areas are not unfinished Phase 5 work. Phase 6 begins with planning only; this decision does not authorize application implementation.
- **Rationale:** The project owner confirmed there is no additional Phase 5 work to add and directed that the phase be completed and work continue.
- **Affected context:** `project-status.md`, `planning/roadmap.md`, `planning/verification/VER-PH-005.md`
- **Supersedes:** Phase 5 `CURRENT` and Phase 6 `FUTURE` status.
- **Approved by:** Explicit project-owner direction: “I want to complete it and move on” and “Lets do this now and continue”.

## DEC-051 — Accept Event editing access proposal

- **Status:** ACCEPTED
- **Date:** 2026-09-26
- **Decision:** Accept Option A in `PRP-015`. When creating an Event, the creator assigns each selected active adult member of the one applicable Family Group either view or edit access. Edit access includes view access. The creator retains edit access. The creator and selected editors may update only the Event's name, description, date, time, and location. Access requires active membership; Family Administrator status alone grants no access. The initial implementation does not support changing the audience or editor assignments after creation.
- **Rationale:** This implements the owner's direction that an Event creator chooses who may see and edit the Event, with a simple rule suitable for the initial small family group.
- **Affected context:** `core/privacy-safety-contract.md#req-priv-021`, `core/requirements-index.md`, `workflows/events.md#req-evt-008`, `planning/proposals/PRP-015-event-edit-access.md`, `project-status.md`, `planning/verification/VER-CTX-1.13.md`, and `planning/verification/VER-CTX-1.13-ACTIVATION.md`.
- **Supersedes:** None.
- **Canonical effect:** The Event edit-access rules are active under `CTX-1.13` after candidate consistency verification and explicit owner approval recorded here.
- **Execution authorization:** Canonical context update and preparation of `WI-006-002` only. Event editing implementation requires its own explicit work-item approval and authorization.
- **Approved by:** Explicit project-owner approval of `PRP-015` on 2026-09-26.

## DEC-052 — Close Phase 6 and advance to Phase 7

- **Status:** ACCEPTED
- **Date:** 2026-09-26
- **Decision:** Close Phase 6 — Events and advance Phase 7 — Plans, Trips & Sharing to `CURRENT`. The approved Phase 6 work items `WI-006-001` and `WI-006-002` are `DONE` with passing work-item verification. The project owner reports the focused SQL-gated Event persistence suite passed from the Visual Studio terminal. Deferred scope and open decisions remain deferred and are not unfinished Phase 6 work. Phase 7 begins with planning only; this decision does not authorize application implementation.
- **Rationale:** The owner conditionally approved closure once all work was complete; `VER-PH-006` found closure readiness passes, and both approved work items are complete.
- **Affected context:** `project-status.md`, `planning/roadmap.md`, `planning/verification/VER-PH-006.md`
- **Supersedes:** Phase 6 `CURRENT` and Phase 7 `FUTURE` status.
- **Approved by:** Project-owner message: “I approve if everything is completed,” applied after the passing Phase 6 closure-readiness review.

## DEC-053 — Move minor and guardian functionality outside the MVP

- **Status:** ACCEPTED
- **Date:** 2026-09-26
- **Decision:** Minor and guardian functionality is outside the MVP and reserved for future work. The product owner wants this functionality considered further before it is defined. No minor profile, guardian authority, minor invitation, or related behavior is approved by this decision.
- **Rationale:** Minor and guardian functionality requires additional product, privacy, and safety consideration and should not expand the MVP before that work is complete.
- **Affected context:** `README.md`, candidate `CTX-1.14` update to `core/product-contract.md#req-prod-008` and `#req-prod-013`, `core/requirements-index.md`, `project-status.md`, `planning/proposals/PRP-016-plan-audience-selection.md`, `planning/stories/WI-007-001-plan-audience-creation.md`, and `planning/verification/VER-CTX-1.14.md`.
- **Open items:** `OD-003` remains open for future work to define minor and guardian functionality. This decision does not resolve specific-person eligibility for Plan audiences or multi-group, later audience-change, or membership-change behavior.
- **Canonical effect:** The MVP exclusion is active under `CTX-1.14` after consistency verification and approval recorded here.
- **Execution authorization:** Canonical context update and planning artifact alignment only; no application implementation.
- **Approved by:** Explicit project-owner direction: “the minor/guardian behavior needs to be moved out of this mvp and moved to future work.”

## DEC-054 — Approve bounded Plan audience choices and creator-departure retention

- **Status:** ACCEPTED
- **Date:** 2026-09-26
- **Decision:** For MVP Plans, the creator chooses either the one applicable Family Group as a whole or specific active adult members of that same group. Do not offer a combined “whole group and selected people” choice because it adds no recipients under the one-group rule. Exclude the creator from the specific-recipient picker; the creator has Plan access as creator. If the creator leaves the group, retain the Plan record; do not automatically delete it or transfer it to another owner.
- **Rationale:** This reuses the narrow, implemented Event recipient boundary while keeping the Plan choices meaningful and preserving the Plan record if its creator leaves.
- **Affected context:** Candidate `CTX-1.15` update to `workflows/plans-trips.md#req-plan-005`, `core/requirements-index.md`, `project-status.md`, `planning/proposals/PRP-016-plan-audience-selection.md`, `planning/proposals/PRP-017-plan-audience-lifecycle.md`, `planning/stories/WI-007-001-plan-audience-creation.md`, and `planning/verification/VER-CTX-1.15.md`; `README.md` active baseline reference.
- **Open items:** Multi-group Plan audiences, audience changes after creation, and recipient access after membership changes remain deferred under `REQ-PRIV-020`. This decision does not define Plan access or edit behavior after the creator leaves beyond retaining the record.
- **Canonical effect:** The Plan-specific audience choices and creator-departure retention rule are active under `CTX-1.15` after consistency verification and approval recorded here.
- **Execution authorization:** Canonical context update and planning artifacts only; no application implementation.
- **Approved by:** Explicit project-owner approval: “I approve this now,” following the recommendation to offer either whole-group or selected-people audience, remove the redundant combined option, and retain Plans when their creator leaves.

## DEC-055 — Use current active members for whole-group Plan audiences

- **Status:** ACCEPTED
- **Date:** 2026-09-26
- **Decision:** For a Plan shared with the whole applicable Family Group, audience access follows current active adult membership. A person whose membership becomes active in that group gains access to the Plan; ending active membership removes access; restored active membership restores access. This rule applies to the whole-group audience option only. Access changes for individually selected recipients remain deferred under `REQ-PRIV-020`.
- **Rationale:** A whole-group audience represents the currently active group and avoids requiring creators to revise a saved audience as group membership changes.
- **Affected context:** Candidate `CTX-1.16` update to `workflows/plans-trips.md#req-plan-005`, `core/requirements-index.md`, `project-status.md`, `planning/proposals/PRP-016-plan-audience-selection.md`, `planning/proposals/PRP-017-plan-audience-lifecycle.md`, `planning/stories/WI-007-001-plan-audience-creation.md`, and `planning/verification/VER-CTX-1.16.md`; `README.md` active baseline reference.
- **Open items:** Multi-group Plan audiences, post-creation audience changes, and membership-change behavior for individually selected recipients remain deferred under `REQ-PRIV-020`. Creator departure retains the Plan record under `DEC-054`; post-departure read/edit behavior remains undefined beyond whole-group access for current active members.
- **Canonical effect:** Whole-group Plan audience access follows current active adult membership under `CTX-1.16` after consistency verification and approval recorded here.
- **Execution authorization:** Canonical context update only; execution for `WI-007-001` was separately authorized by the project owner and remains limited to that work item.
- **Approved by:** Explicit project-owner direction: “I like option 1 - current group. We can define it further in the future if needed.”

## DEC-056 — Add optional Plan notes at creation

- **Status:** ACCEPTED — canonical context activated
- **Date:** 2026-09-26
- **Decision:** Add an optional plain-text Notes field to Plan creation, limited to 1,000 characters. Place it after the optional expected check-in and before saving. Blank or whitespace-only content means no note; preserve internal line breaks and display the content as text. A note is part of its Plan and follows that Plan's existing audience. The form must explain that audience, including that future active adult members can see notes on a whole-group Plan. Do not add note editing, audience changes, notifications, or Sharing Session behavior in this item.
- **Rationale:** This completes a narrow part of the Plan information already allowed by `REQ-PLAN-002`, while making the existing Plan audience rule clear for potentially sensitive free text.
- **Affected context:** Candidate `CTX-1.17` update to `workflows/plans-trips.md#req-plan-003`, new `#req-plan-006`, `#req-plan-005`, `core/requirements-index.md`, `project-status.md`, `PRP-018`, `planning/verification/VER-CTX-1.17.md`, and draft `WI-007-002`.
- **Open items:** None for the accepted notes scope. Plan edits, note changes after creation, check-ins, completion, multi-group handling, later audience changes, selected-recipient membership effects, creator post-departure access, and minor/guardian behavior remain outside this item.
- **Canonical effect:** `CTX-1.17` is active following context consistency verification and explicit owner activation on 2026-09-26.
- **Execution authorization:** Candidate context preparation, verification, canonical activation, and drafting `WI-007-002`. Application implementation and verification of `WI-007-002` were separately approved and authorized by the project owner on 2026-09-26.
- **Approved by:** Project owner, “I choose option a for PRP-018”; separately “I approve activation of CTX-1.17 and approve/authorize execution of WI-007-002.”

## DEC-057 — Reconcile MVP Trip roles, audience, privacy, and workflow

- **Status:** ACCEPTED — canonical context activated as `CTX-1.18`
- **Date:** 2026-09-27
- **Decision:** For MVP, the Trip creator is the organizer, need not be a traveler, retains creator access after loss of Family Group eligibility, and does not transfer Trip ownership. Require at least one eligible existing active adult in-system Family Group traveler; traveler status alone grants no Trip viewing, editing, Notes, or Sharing Session access. The Trip audience is exactly one applicable Family Group or selected eligible people from that same group, with no multiple groups, combined audience, or non-system viewers. Group audience membership is snapshotted when selected; new members do not gain access automatically, membership loss revokes access, and rejoining does not restore access. The creator may explicitly change audience after creation through a separately authorized capability; changes immediately enforce access and must disclose access effects before confirmation. Do not add admin-configured audience modes in MVP. Require title, one primary destination, start date, end date, at least one traveler, and audience; end date cannot precede start date. Description, transportation notes, accommodation notes, informational non-system contacts, and expected arrival are optional. Non-system contacts use required name and optional role/relationship, phone, and email, inherit Trip audience visibility, and receive no account or application permissions. Trip Notes are readable/addable by audience members; authors may edit/delete their own; creator may hide another person's note from normal display but may not rewrite it; hidden notes are retained for history/audit. Check-Ins are personal and voluntary; expected arrival is distinct from dates/completion; creator explicitly marks completion; passing end date or missing location does not automatically complete or raise a Concern. Trip audience and Sharing Session audience remain separate; each person controls Sessions involving their own information. Initial product flow is Trips Overview → Create Trip → Trip Detail. Review screen, filters, status chips, avatars, and quick actions remain guidance/prototype concepts.
- **Rationale:** This establishes the minimum Trip-specific role and access rules needed for safe MVP planning while preserving Plan and Event contracts and keeping the creation experience lightweight.
- **Affected context:** `workflows/plans-trips.md` (`REQ-TRIP-007` through `REQ-TRIP-012`), `core/privacy-safety-contract.md` (`REQ-PRIV-020` clarification and new `REQ-PRIV-022`), `workflows/sharing-checkins.md` (new `REQ-SHARE-009`), `core/requirements-index.md`, `planning/candidates/CTX-1.18-trip-reconciliation.md`, `planning/proposals/PRP-019-trip-creation-and-viewing.md`, new `planning/proposals/PRP-020-trip-context-reconciliation.md`, `project-status.md`, `planning/roadmap.md`, `README.md`, and `planning/verification/VER-CTX-1.18.md`.
- **Supersedes:** The unresolved/recommended Trip behavior in `PRP-019`; it does not supersede any existing Plan or Event requirement.
- **Open items:** Exact retention/storage implementation for creator-hidden Notes and restore/recovery UI are not defined here; restore UI is explicitly deferred. No ownership transfer is allowed. No admin audience configuration, multi-group, combined audience, non-system viewer, multiple destination, or minor/guardian behavior is introduced.
- **Canonical effect:** `CTX-1.18` is active after the passing consistency verification recorded in `VER-CTX-1.18` and explicit project-owner authorization.
- **Execution authorization:** Prepare and verify this canonical context update only. No Trip code, implementation story, migration, architecture work, or Phase 7 application execution is authorized.
- **Approved by:** Explicit project-owner approval and clarifications in the 2026-09-27 Trip context update authorization.

## DEC-058 — Reconcile Event MVP coordination and privacy boundaries

- **Status:** ACCEPTED — canonical context activated as `CTX-1.19` after verification
- **Date:** 2026-09-27
- **Decision:** For MVP, the Event creator is the sole organizer. Existing selected-person Event audience and designated five-field detail editors remain unchanged; editors gain no invitation, RSVP, Need, Update, or Sharing Session authority from detail-edit access. Event audience, invitation, participant, and RSVP are distinct. The organizer may invite only an already authorized Event viewer and may revoke an invitation without removing Event visibility; revocation makes the invitation and its RSVP inactive/historical. Invitations do not expire automatically, and Event-completion handling is deferred. An active invited in-system user controls and may change their own Pending, Going, Maybe, or Not Going RSVP. Use one lightweight Event Need model with one responsible person, Open, Claimed, Assigned, and Completed states. Assignment is an organizer request pending explicit acceptance, never forced consent. Eligible invited participants with Pending, Going, or Maybe RSVP may claim an Open Need or receive an assignment; Not Going prevents new responsibility. Not Going, invitation revocation, Event visibility loss, or eligible membership loss returns a person's Claimed or Assigned Need to Open without completion or deletion, preserving prior responsibility, reason, and time; renewed access does not restore responsibility. Only the organizer posts, edits, or hides meaningful Event Updates. Edits retain prior text and an updated timestamp; hidden records and revisions remain with the Event while it exists, are unavailable through normal Event APIs, and create no alternate access path. Visible Updates follow current Event audience. Description holds stable information; Updates are time-ordered announcements. Generic Check-Ins remain separate; Event participation, roles, RSVP, and audience do not authorize location or Sharing Session data. Phase 6 remains closed for its two approved implementation slices while Event invitations/RSVP, Needs, and Updates remain unimplemented MVP capabilities.
- **Rationale:** Define the remaining core Event coordination behavior without weakening consent, access, or completed Phase 6 boundaries.
- **Affected context:** `workflows/events.md` (`REQ-EVT-001` through `REQ-EVT-005`, `REQ-EVT-007`, new `REQ-EVT-009` through `REQ-EVT-013`), `core/privacy-safety-contract.md` (new `REQ-PRIV-023`), `core/requirements-index.md`, `planning/candidates/CTX-1.19-event-reconciliation.md`, `project-status.md`, `planning/roadmap.md`, `README.md`, and `planning/verification/VER-CTX-1.19.md`.
- **Supersedes:** None. `DEC-051`, `DEC-052`, `REQ-PRIV-020/021`, and `REQ-EVT-008` remain in force. Plan and Trip requirements are unchanged.
- **Deferred:** Optional Event end date/time, group or combined audience, multiple groups or organizers, non-system RSVP, Need-manager delegation, Event Sharing Session implementation, Event-specific Check-In states, notification triggers, mockup layout, Event completion handling for invitations/RSVP, and broader Event deletion/retention policy.
- **Canonical effect:** `CTX-1.19` is active after passing consistency verification in `VER-CTX-1.19` and explicit project-owner authorization to activate on a pass.
- **Execution authorization:** Canonical context update and verification only. No application code, implementation story, migration, Plan/Trip behavior change, or reopening of `WI-006-001` or `WI-006-002` is authorized.
- **Approved by:** Explicit project-owner Event reconciliation and final context-update authorization on 2026-09-27.

## DEC-059 — Reconcile Trusted Contact eligibility and Phase 5 closure scope

- **Status:** ACCEPTED — canonical context activated as `CTX-1.20` after verification
- **Date:** 2026-09-27
- **Decision:** A Trusted Contact may be a person outside the Family Group. Designation remains adult-owned and private by default and grants no Family Group, Event, Trip, Sharing Session, or safety-incident access or automatic Safety Circle membership. Trusted Contact and Safety Circle remain separate concepts. Correct stale Shared Family Date and Trusted Contact wording without changing previously approved Shared Family Date behavior. Phase 5 remains complete for its four approved implementation work items; other Family MVP capabilities require separate scope and authorization.
- **Rationale:** Reconcile recovered approved Family planning with current canonical wording while preserving individual privacy, feature-specific audiences, and completed implementation boundaries.
- **Affected context:** `core/product-contract.md` (`REQ-FAM-010`, `REQ-FAM-014`, new `REQ-FAM-015`), `core/privacy-safety-contract.md` (new `REQ-PRIV-024`), `core/domain-glossary.md`, `core/requirements-index.md`, `planning/candidates/CTX-1.20-phase-5-reconciliation.md`, `project-status.md`, `planning/roadmap.md`, `README.md`, and `planning/verification/VER-CTX-1.20.md`.
- **Open items:** Trusted Contact fields, Account expectations, invitation or acceptance, notification consent, audiences, uses, retention, Safety Circle integration, safety-incident access, UI, and implementation shape; Household and Safety Circle management; membership removal/rejoin and administrator transfer; future retention and minor/guardian behavior.
- **Compatibility:** `WI-005-001` through `WI-005-004`, CTX-1.19 Event rules, Plan and Trip audiences and Notes, and Sharing Session consent remain unchanged. No application code correction is required.
- **Canonical effect:** `CTX-1.20` is active after passing `VER-CTX-1.20` verification and the project owner's conditional activation authorization. `CTX-1.19` remains an approved historical baseline.
- **Execution authorization:** Canonical context update, verification, and conditional activation only. No application code or implementation story is authorized.
- **Approved by:** Explicit project-owner Phase 5 context reconciliation authorization on 2026-09-27.

## DEC-060 — Clarify MVP Trip Notes authority and retained states

- **Status:** ACCEPTED — canonical context activated as `CTX-1.21` after verification
- **Date:** 2026-09-27
- **Decision:** Current authorized Trip audience members may read visible Trip Notes and add Notes. The Trip creator may do so through narrow creator authority for their own Trip, independently of audience or eligible Family Group membership. An author with current Notes access may edit or soft-delete their own ACTIVE Note. Author deletion retains the original Note and provenance outside normal display. The creator may hide another author's ACTIVE Note; it remains retained and immutable through normal Notes actions, and the creator may not rewrite its content. The conceptual MVP states are ACTIVE, AUTHOR_DELETED, and CREATOR_HIDDEN. Removed Notes remain associated with the Trip while it exists, are excluded from normal APIs and UI, and create no alternate access path. Notes remain plain-text coordination rather than chat and grant no private Check-In, location, or Sharing Session access. The 1,000-character validation bound remains a work-item choice and is not canonical.
- **Rationale:** Make the owner-approved Notes access, deletion, moderation, and retention boundaries explicit before the first Notes implementation, without changing Trip audience or unrelated feature permissions.
- **Affected context:** `workflows/plans-trips.md#req-trip-010`, `core/requirements-index.md`, `planning/candidates/CTX-1.21-trip-notes-reconciliation.md`, `project-status.md`, `planning/roadmap.md`, `README.md` after verification, draft `WI-007-004` baseline references, and `planning/verification/VER-CTX-1.21.md`.
- **Compatibility:** `REQ-TRIP-007` through `REQ-TRIP-012` outside the clarified Notes semantics, `REQ-PRIV-022`, selected-person and group snapshot audiences, creator access after membership loss, Plan Notes, CTX-1.19 Events, CTX-1.20 Family/Profile behavior, and Sharing Session consent remain unchanged. `WI-007-003` remains `DONE` and requires no code change.
- **Deferred:** Database representation, global retention duration, Trip deletion, archive/disposal, audit-history and restore/recovery UI, and all other Phase 7 capabilities outside Notes.
- **Canonical effect:** `CTX-1.21` is active after passing `VER-CTX-1.21` and the project owner's conditional activation authorization. `CTX-1.20` remains an approved historical baseline.
- **Execution authorization:** Canonical context update, verification, and conditional activation only. `WI-007-004` remains `PROPOSED` and is not approved or authorized for execution. No application code or migration is authorized.
- **Approved by:** Explicit project-owner Trip Notes canonical reconciliation authorization on 2026-09-27.

## DEC-061 — Define MVP creator-controlled Trip audience replacement

- **Status:** ACCEPTED — canonical context activated as `CTX-1.22` after verification
- **Date:** 2026-09-27
- **Decision:** The Trip creator may deliberately replace the entire audience of their existing Trip with either the existing applicable Family Group or a complete Selected People set. Either mode switch is allowed. Group selection and refresh create a fresh eligible active-adult snapshot without individual overrides. Selected People requires distinct, currently eligible active-adult members of the applicable group and at least one eligible non-creator recipient. Invalid proposals leave the saved audience unchanged. Creator authority is independent and cannot be removed or transferred. A creator who has lost eligibility to select current group members may reduce the saved Trip audience using currently eligible saved recipients but may not expand it using new group information, refresh the Group snapshot, or regain directory/profile access. Preview identifies the resulting mode and access gains/losses within the same privacy boundary; authority and eligibility are revalidated at commit, and a stale preview cannot silently commit different access effects. Replacement atomically changes Trip and normal Notes access without changing existing Notes or granting retained removed Notes. Later Note mutations require current Trip access. Creator/non-creator detail disclosure remains narrow. No audience notification, Sharing Session/Check-In effect, dedicated audience history, or generalized ACL/audit capability is added.
- **Rationale:** Make post-creation Trip audience changes understandable and privacy-preserving while reusing the approved snapshot model and keeping the MVP small.
- **Affected context:** `core/privacy-safety-contract.md#req-priv-022`, `core/requirements-index.md`, `planning/candidates/CTX-1.22-trip-audience-editing-reconciliation.md`, `planning/verification/VER-CTX-1.22.md`, `project-status.md`, `planning/roadmap.md`, and `README.md` after passing verification and activation.
- **Compatibility:** `REQ-TRIP-007/008/010`, `REQ-PRIV-011/020`, `REQ-SHARE-009`, completed `WI-007-003/004`, dynamic Plan audiences, Event access, Family/Profile permissions, and sharing/check-in consent remain valid and unchanged. No completed application code correction is required.
- **Deferred:** WI-007-005 implementation approval; dedicated audience history; notifications; Trip completion; Check-In and Sharing Session execution; multi-group, combined, and non-system audiences.
- **Canonical effect:** `CTX-1.22` is active after passing `VER-CTX-1.22` and the project owner's conditional activation authorization. `CTX-1.21` remains an approved historical baseline.
- **Execution authorization:** Canonical context update, verification, and conditional activation only. No application code, migration, WI-007-005 approval, or WI-007-005 execution is authorized.
- **Approved by:** Explicit project-owner Trip audience-editing decisions and context-update authorization on 2026-09-27.

## DEC-062 — Prioritize End-to-End UX & User-Test Readiness after Phase 7

- **Status:** ACCEPTED — roadmap-priority context activated as `CTX-1.23` after verification
- **Date:** 2026-09-27
- **Decision:** Add Phase 7.5 — End-to-End UX & User-Test Readiness as the next implementation priority after Phase 7 is formally complete and before Phase 8. Phase 7 remains current until it satisfies its existing completion and verification requirements. Phase 7.5 makes existing application journeys coherent and usable for early user testing, including the approved invitation-based Family entry path, the application shell/navigation, useful attention-first Home and empty states, understandable structural Family administration, integration of implemented Family/Event/Plan/Trip workflows, and at least one end-to-end user-test scenario. Preserve operator-controlled first Family provisioning under `REQ-FAM-006`; public self-service Family creation is not introduced. Do not choose production authentication architecture, expand administrator permissions, implement Safety capabilities, or resolve `OD-001`. Phase 8 remains unchanged. Phase 9 — Integration & UX Refinement remains planned for deeper integration, feedback-informed refinement, accessibility, performance, broader usability, and release-quality polish. Phases 8–11 are not renumbered. No product requirement IDs are added or changed; mockups remain non-authoritative.
- **Rationale:** Make end-to-end product usability the explicit priority before major capability expansion, without weakening approved Family provisioning, privacy, UX, or Safety boundaries.
- **Affected context:** `planning/roadmap.md`, `project-status.md`, `README.md`, `planning/proposals/PRP-021-phase-7-5-user-test-readiness.md`, candidate `planning/candidates/CTX-1.23-phase-7-5-user-test-readiness.md`, and `planning/verification/VER-CTX-1.23.md`.
- **Compatibility:** `REQ-FAM-005/006`, `REQ-PRIV-006`, `REQ-UX-001/002/003/006/007/008/011/012`, current Phase 7 requirements, Phase 8 scope, and Phase 9 existence remain in force. No requirement index or product contract changed. `WI-007-005` remains proposed, unapproved, and not execution-authorized; Phase 7 remains current.
- **Canonical effect:** `CTX-1.23` is active after passing `VER-CTX-1.23`. `CTX-1.22` remains an approved historical baseline.
- **Execution authorization:** Context update, verification, and conditional activation only. No application code, migration, Phase 7 closure, Phase 7.5 implementation, or implementation story is authorized.
- **Approved by:** Explicit project-owner Phase 7.5 direction and context-update authorization on 2026-09-27.

## DEC-063 — Close Phase 7 and advance Phase 7.5

- **Status:** ACCEPTED — Phase 7 closure and Phase 7.5 status transition recorded
- **Date:** 2026-09-28
- **Decision:** Close Phase 7 — Plans, Trips & Sharing. Its five approved work items (`WI-007-001` through `WI-007-005`) are `DONE` with passing work-item verification, and the closure review returned `READY TO CLOSE`; closure evidence is recorded in `planning/verification/VER-PH-007.md`. Set Phase 7 to `COMPLETE / CLOSED` and Phase 7.5 — End-to-End UX & User-Test Readiness to `CURRENT`. Phase 8 remains future; Phases 9–11 retain their existing sequence and purpose. Phase 7.5 retains the purpose and boundaries approved in `DEC-062`: prepare the existing application for coherent end-to-end early user testing across first-use/return journeys, invitation-based Family entry, profile and Family context, Home/dashboard, navigation/application shell, Family administration, empty states, implemented Family/Event/Plan/Trip integration, and a user-test scenario. Preserve operator-controlled Family provisioning, no production authentication architecture selection, no administrator access to private data, no Safety implementation, and non-authoritative mockups. This status transition creates no Phase 7.5 implementation story and authorizes no application implementation.
- **Rationale:** The approved Phase 7 scope is complete and its closure review found no blockers; Phase 7.5 was already approved as the next priority after closure.
- **Affected context:** `README.md`, `project-status.md`, `planning/roadmap.md`, and `planning/verification/VER-PH-007.md`.
- **Compatibility:** Active baseline `CTX-1.23` remains in force. No product requirements, privacy rules, or Safety scope change. Phase 7 deferred work remains future work and is not represented as completed behavior. `DEC-062` remains the source for Phase 7.5 purpose and boundaries.
- **Deferred:** Trip completion; Check-In execution; Sharing Sessions; location-sharing execution; notifications; Safety functionality; selected-person Plan membership/rejoin lifecycle behavior; other separately documented membership lifecycle edge cases; and future scale/performance concerns remain separately scoped. None blocks closure of the approved Phase 7 work-item set. No standalone scale/performance concern is documented in the Phase 7 work items; later Phase 9 retains performance refinement in its approved purpose.
- **Canonical effect:** Phase 7 is `COMPLETE / CLOSED`; Phase 7.5 is `CURRENT`; Phase 8 remains future. `CTX-1.23` remains the active baseline because this is a phase-status transition only and changes no product requirements.
- **Approved by:** Explicit project-owner formal Phase 7 closure and context/status update authorization on 2026-09-28.

## DEC-064 — Define MVP current Family context in the application shell

- **Status:** ACCEPTED — canonical context activated as `CTX-1.24` after verification
- **Date:** 2026-09-28
- **Decision:** For MVP shell behavior, automatically select the user's only active Family membership. If there are multiple active memberships, require an explicit current-Family choice through a clear shell-level selector; never silently choose the first returned membership. If there is no active membership, select no Family and show the approved invitation/operator-provisioned no-Family path. Retain a chosen Family only for the current application/page session. Treat this selection as presentation/application context only: it changes no access, membership, audiences, profile visibility, administrator authority, Sharing permission, or Safety permission. When the selected Family changes, clear prior Family-scoped client state and reload data authorized for the new selection, without visually retaining the prior Family's directory, items, or forms.
- **Rationale:** Make returning-user context explicit for people belonging to more than one Family while preserving existing authorization boundaries and avoiding misleading stale Family data.
- **Affected context:** `core/ux-contract.md#req-ux-011`, `planning/proposals/PRP-023-application-shell-returning-user-landing.md`, `planning/candidates/CTX-1.24-application-shell-returning-user-landing.md`, `planning/verification/VER-CTX-1.24.md`, `README.md`, and `project-status.md`.
- **Requirement impact:** Clarifies existing `REQ-UX-011`; no new requirement ID is needed. `OD-001` remains open.
- **Compatibility:** Controlled/operator provisioning and invitation-only membership remain in force. The Family selection does not grant access or alter existing server authorization. Production authentication is unchanged. PRP-023's routed shell, conceptual destinations, separate Plan and Trip routes, contextual admin entry, Development-only identity switcher, and deferred full Home direction remain approved. No application code or implementation story is created.
- **Deferred:** Final mobile navigation grouping under `OD-001`; production authentication; self-service Family creation/discovery/join; new domain behavior; full Home/dashboard; and expanded administration remain outside this decision.
- **Canonical effect:** `CTX-1.24` is active after passing `VER-CTX-1.24`; `CTX-1.23` remains an approved historical baseline.
- **Execution authorization:** Context/decision update and verification only. No application implementation or implementation story is authorized.
- **Approved by:** Explicit project-owner approval and context/decision-update authorization on 2026-09-28.

## Open product decisions

These questions are intentionally unresolved and are not requirements.

| ID | Status | Decision required | Constraint |
|---|---|---|---|
| `OD-002` | `RESOLVED - bounded MVP` | Temporary only; no indefinite sharing in this first slice. | DEC-087 / REQ-PRIV-031: default one hour, maximum 24 hours, explicit audience/start/expiration and early stop. Future scope requires separate approval. |
| `OD-003` | `OPEN — FUTURE WORK` | Define minor and guardian functionality for a future phase. | Excluded from MVP by `DEC-053`; no behavior is defined or authorized. |

## Resolved product decisions

| ID | Status | Resolution | Decision record |
|---|---|---|---|
| `OD-001` | `RESOLVED` | MVP primary navigation has six areas at desktop and narrow widths: Home, Family, Events, Plans & Trips, Safety, Me / Settings. "Plans & Trips" and "Safety" are navigation groupings only; the underlying domains, routes, authorization, data, and lifecycle stay separate. Validated by owner visual review, desktop and narrow rendered evidence, keyboard/navigation review, and automated reachability tests together. | `DEC-074`; `REQ-UX-013` |
| `OD-004` | `RESOLVED` | Incident-history retention, as product defaults (not legal requirements). An Open Incident is retained while Open and freezes after 30 days without authorized activity. Frozen and Resolved product history is retained 365 days from freeze or resolution, then becomes unavailable immediately and is removed from active storage. Audit metadata is retained 365 days per event. Backups age out within 30 days of active deletion, and a restore never reactivates expired data. | `DEC-071`; `REQ-PRIV-026` |
| `OD-005` | `RESOLVED` | Operator-configured capacity; planning target 50, default limit 100, validated ceiling 250 members per Family Group. | `DEC-014` |
## DEC-065 — Close PRP-027 and Phase 7.5

- **Status:** ACCEPTED — PRP-027 and Phase 7.5 formally closed
- **Date:** 2026-09-29
- **Decision:** Close `PRP-027 — Mockup Alignment & UI Gap Assessment` as `CLOSED / COMPLETED` because its approved implementation purpose was fulfilled through WI-075-010 shared foundation, WI-075-011 Home, WI-075-012 Family and first-use, WI-075-013 Events, WI-075-014 Plans, and WI-075-015 Trips. Close Phase 7.5 — End-to-End UX & User-Test Readiness as `COMPLETE / CLOSED`; its exit areas are assessed as satisfied in `planning/verification/VER-PH-075.md`. Preserve WI-075-005 as `DONE` with its accepted owner/developer-supplied evidence limitations; another participant study is not required for closure. Keep `OD-001` `OPEN`; its final mobile-navigation grouping concerns later UX validation and does not block the completed Phase 7.5 scope. Preserve the useful reference direction while excluding unsupported mockup-only functionality and retaining approved privacy/access rules. Remaining spacing, density, responsive and visual refinements are future polish, not unfinished PRP-027 scope.
- **Rationale:** The completed readiness assessment found all known PRP-027 implementation slices and Phase 7.5 exit areas satisfied, with no MUST-FIX blocker. The project owner explicitly authorized both closures.
- **Affected context:** `README.md`, `project-status.md`, `planning/roadmap.md`, `planning/proposals/PRP-027-mockup-alignment-ui-gap-assessment.md`, `planning/verification/VER-PH-075.md`, and this decision log.
- **Compatibility:** `CTX-1.24` remains active. No canonical requirement, contract, implementation, or completed story history changes. `WI-075-005` remains DONE. `OD-001` remains OPEN. Phase 8 remains FUTURE; this decision does not start Phase 8 or authorize implementation work.
- **Deferred / non-blocking:** Broader formal usability validation, stylesheet budget warnings and future maintenance, the unrelated DevelopmentSession template TypeError, responsive polish, and minor visual refinement remain future non-blocking items. No new work item is created.
- **Canonical effect:** Phase 7.5 is `COMPLETE / CLOSED`; PRP-027 is `CLOSED / COMPLETED`; `CTX-1.24` remains active because this phase/proposal status closure changes no canonical requirements. Phase 8 remains `FUTURE`.
- **Approved by:** Explicit project-owner authorization to formally close PRP-027 and Phase 7.5 on 2026-09-29.

## DEC-066 — Enter Phase 8 Safety Coordination

- **Status:** ACCEPTED — Phase 8 `CURRENT / IN PROGRESS`
- **Date:** 2026-09-29
- **Decision:** The project owner authorizes the transition of Phase 8 — Safety Coordination from `FUTURE` to `CURRENT / IN PROGRESS`. Phase 7.5 remains `COMPLETE / CLOSED` and PRP-027 remains `CLOSED / COMPLETED`. Create `PRP-028 — Phase 8 Concern Access and Lifecycle Proposal` for owner review only. The proposal's permissions, lifecycle, retention approach, and proposed Phase 8 exit criteria remain unapproved.
- **Compatibility:** `CTX-1.24` remains the active baseline; the transition is status-only and changes no canonical requirements or Safety/privacy rules. `OD-001`, `OD-002`, and `OD-004` remain `OPEN`; `OD-003` remains outside MVP. Family membership and current-Family context do not grant Safety access.
- **Affected context:** `README.md`, `project-status.md`, `planning/roadmap.md`, `planning/decision-log.md`, and `planning/proposals/PRP-028-phase-8-concern-access-and-lifecycle.md`.
- **Execution authorization:** Governance/status updates and proposal creation only. No implementation work item, application code, migration, or Safety implementation is authorized.
- **Approved by:** Explicit project-owner Phase 8 entry and planning-proposal authorization on 2026-09-29.

## DEC-067 — Approve PRP-028 Concern rules and Phase 8 exit criteria direction

- **Status:** ACCEPTED — PRP-028 partial approval; retention pending
- **Date:** 2026-09-30
- **Decision:** Record the project owner's approval for PRP-028 of the Concern creation, subject visibility, participant rights, creator-only controls and resolution, membership-loss behavior, protected-information boundary, canonical provenance/confidence model, append-only corrections, resolved-Concern behavior, Concern/Incident separation, and exclusions for automatic Safety Circle/Trusted Contact access, location, Check-Ins, Sharing Sessions, notifications, and escalation. Approve the 13 Phase 8 exit-criteria areas in `planning/roadmap.md`, without requiring broad formal participant usability research unless separately approved.
- **Compatibility:** `CTX-1.24` remains active; no canonical product/privacy requirement has yet been amended. `OD-001`, `OD-002`, and `OD-004` remain open; `OD-003` remains outside MVP. `OD-004` remains separate for Incident-history retention.
- **Deferred:** Concern history retention, Open/Frozen expiration, audit evidence, and backup aging remain undecided. No duration is selected. PRP-028 remains `PROPOSED — RETENTION DECISION PENDING` and is not fully approved.
- **Execution authorization:** Update planning records and prepare the Concern-retention decision packet only. No implementation work item, code, migration, or Safety implementation is authorized.
- **Approved by:** Explicit project-owner decision dated 2026-09-30.

## DEC-068 — Approve complete PRP-028 Concern retention policy

- **Status:** ACCEPTED — PRP-028 approved in full; canonical context activated as `CTX-1.25` after verification
- **Date:** 2026-09-30
- **Decision:** Approve resolved Concern product-history retention for 180 days from resolution; automatic freeze of Open Concerns after 30 days without authorized mutation; Frozen history read-only for currently eligible participants and retained 180 days from freeze; minimal audit evidence retained 180 days per event for create, participant changes, contribution/correction/challenge, freeze, resolution, revocation, and denied read/write; product content removed from active storage/access at expiry; backup aging capped at 30 days after active deletion with restore prevention for already-expired data; and no legal-hold engine. Freeze does not resolve, determine danger/safety, verify/invalidate, create an Incident, or escalate. No pre-freeze acknowledgement is required. Audit content excludes Concern content and restricted-source details; no user audit viewer or automatic Family-member/admin/operator audit access is provided. Any future operational audit access policy requires separate approval.
- **Retention scope:** Resolved history starts its clock at resolution; Frozen history starts at freeze. At expiry, remove Concern history, participants, entries, unknowns, challenges, correction links, provenance, resolution, and freeze details from active product storage/access. Audit entries expire on their own per-event clocks. Deployment documentation must describe backup aging and restore behavior. These are application defaults; obtain qualified review before deployment if applicable legal obligations arise. No legal-hold engine is in scope.
- **Rationale:** The selected finite periods and explicit triggers define product, audit, and backup deletion behavior while preserving immediate access revocation and preventing stale Open Concerns from being mistaken as resolved or dangerous.
- **Affected context:** `core/privacy-safety-contract.md#req-priv-025`, `workflows/safety.md#req-safe-015`, `core/product-contract.md#req-prod-012`, `core/requirements-index.md`, `planning/proposals/PRP-028-phase-8-concern-access-and-lifecycle.md`, `planning/roadmap.md`, `README.md`, and `project-status.md`.
- **Compatibility:** `DEC-067` remains the historical partial approval; this decision completes it. `OD-004` remains open for Incident-history retention. Phase 8 stays `CURRENT / IN PROGRESS`; no implementation story, code, schema, or migration is authorized.
- **Canonical effect:** `CTX-1.25` is active after passing `VER-CTX-1.25`; `CTX-1.24` remains an approved historical baseline.
- **Approved by:** Explicit project-owner decision dated 2026-09-30.

## DEC-069 — Bound denied Concern audit volume with hourly summaries

- **Status:** ACCEPTED — implementation clarification for authorized `WI-008-001`
- **Date:** 2026-10-01
- **Decision:** For denied Concern access attempts by authenticated actors only, retain at most one summary per actor, explicit broad operation category (`ConcernReadDenied` or `ConcernWriteDenied`), and UTC hour bucket. Store the actor ID, category, denied result, bucket time, first and last attempt timestamps, count, and the existing 180-day retention clock. Do not retain attempted Concern IDs or other target identifiers in these summaries. Keep successful Concern action audit rows individual and unchanged. Do not add throttling, lockouts, IP identity, abuse monitoring, or generic audit/deduplication infrastructure.
- **Concurrency:** Use the simplest reliable database uniqueness and update-safe locking supported by the current SQL Server architecture. No distributed lock or external service is authorized.
- **Rationale:** Preserve bounded evidence of denied read/write activity while avoiding one indefinitely accumulating retained row per request and preventing caller-controlled target IDs from creating unbounded key cardinality.
- **Compatibility:** Product-visible Concern permissions, nondisclosing unavailable responses, authenticated actor identity, successful-action audit detail, and 180-day retention remain unchanged. The summary is internal and has no viewer endpoint. If a denied Concern request lacks a stable authenticated account ID, stop and request a separate decision rather than inventing anonymous identity.
- **Affected context:** `planning/stories/WI-008-001-concern-domain-and-access-foundation.md`, `planning/verification/VER-WI-008-001.md`, and this decision log.
- **Canonical effect:** This specifies the persistence representation for the already-approved minimal denied-attempt evidence. It changes no canonical product/privacy behavior and does not require a new context baseline; `CTX-1.25` remains active.
- **Approved by:** Explicit project-owner decision dated 2026-10-01.


## DEC-070 — Approve site-wide reference mockup alignment

- **Status:** ACCEPTED — owner visual approval of completed presentation-only work
- **Date:** 2026-10-01
- **Decision:** Adopt the visual direction in `claude-handoff/reference` across the web application, as implemented and owner-reviewed on 2026-10-01:
  - blue reference palette
  - desktop left sidebar and slim top bar
  - one page-title hierarchy per screen
  - light-blue content background with a white card system
  - mockup-style list rows and date tiles
  - shared spacing, typography, and surfaces
  - the implemented Home, Family, Events, Plans, Trips, Concerns, and Me / Settings compositions
  - the current responsive interpretation

  This supersedes the PRP-027 preference for the evergreen token palette and the stacked-header shell for future visual work.
- **Accepted differences (not to be reworked without a new request):**
  - no photo thumbnails where no photo data exists
  - no search, notification bell, or avatar-menu functionality
  - no Messages destination
  - no RSVP / Respond / Maybe
  - no activity feed, Tasks, or Active Sharing
  - no check-ins, map, or Incident
  - no unsupported Trip statuses
  - Events and Plans create forms remain visible beside their lists
  - Trips Create action stays in its current placement
  - narrow/mobile navigation remains unresolved under `OD-001`
  - the development identity banner remains Development-only
- **Evidence:** 66/66 Angular component tests pass. The Concern, Trip-audience, and invitation Playwright journeys each pass when run independently. The production build passes with existing non-blocking budget warnings: initial bundle ≈ 526 kB; `concerns.scss` ≈ 7.53 kB and `trips.scss` ≈ 5.79 kB, both under the 8 kB error budget; `app.scss` fell below the 4 kB warning. Rendered desktop/narrow evidence is in `planning/verification/evidence/site-mockup-alignment/` (see its `README.md`). The full-suite Playwright shared-database ordering issue remains a separate test-infrastructure concern.
- **Compatibility:** Presentation only. No product behavior, permissions, privacy rules, authorization, API, schema, or migration changed. Navigation destinations are unchanged, and `OD-001`, `OD-002`, and `OD-004` remain open. Approved WI-008-002 evidence is preserved unchanged.
- **Affected context:** `project-status.md`, `planning/roadmap.md`, `planning/verification/evidence/site-mockup-alignment/README.md`, and `claude-handoff/`.
- **Canonical effect:** Visual styling only; no canonical requirement changes and no new context baseline. `CTX-1.25` remains active.
- **Approved by:** Explicit project-owner visual approval dated 2026-10-01.

## DEC-071 — Approve PRP-029 Safety Incident workflow and resolve OD-004

- **Status:** ACCEPTED — PRP-029 approved; `OD-004` resolved; `CTX-1.26` active after verification
- **Date:** 2026-10-01
- **Decision:** Accept PRP-029's recommended Safety Incident model with the owner refinements below.
  - **Concern and Incident.** A Concern coordinates review of uncertainty. An Incident is deliberately started safety coordination. Neither proves danger. Nothing creates an Incident automatically, including elapsed time, missed contact or arrival, a failed Check-In, stale information, unavailable location, Concern age, or Concern freeze.
  - **Creation.** Any active adult Family member may manually create an Incident about exactly one active adult in the same Family Group. Self-Incident is allowed. Administrator status adds no authority. An Incident may be created directly or started deliberately from a Concern.
  - **Access.**
    - Access is by explicit per-Incident grants to active adults in the same group, bound to membership ID and `AccessVersion`.
    - The subject has no automatic access; the coordinator may add the subject explicitly.
    - Family membership, Administrator, Safety Circle, Trusted Contact, subject status, and Concern participation grant no access.
    - Rejoining never reactivates an old grant.
    - Denied access does not disclose existence.
  - **Coordinator.**
    - There is one coordinator, initially the creator, with explicit handoff to a current eligible participant while Open.
    - The former coordinator continues as an ordinary participant while eligible. The creator keeps no special authority.
    - No co-coordinators or role framework.
    - **Coordinator loss.** Unexpected loss of eligibility revokes coordinator access and freezes the Incident. There is no promotion and no resolution, and remaining eligible participants have read-only access.
    - **Coordinator unavailable.** A coordinator who is unavailable but still eligible triggers no transfer. Participants continue their allowed actions and the inactivity rule applies.
  - **Concern → Incident.**
    - The Concern creator may use a manual **Start an Incident** action while the current Concern rules permit them access.
    - Only the subject is prefilled. No entries, corrections, challenges, participants, sources, history, or private data are copied, and participants are not preselected.
    - **Lineage refinement.** The Incident stores the originating Concern ID as a non-authorizing lineage reference. Per the owner's stated preference, it is stored only on the Incident (Incident → originating Concern), and the Concern stores no Incident reference. Lineage grants no access, copies no audience, exposes no private information, makes neither record readable to the other's audience, copies nothing, and changes no Concern state.
    - No generic relationship framework.
  - **Lifecycle.** `Open`, `Frozen`, and `Resolved`. The additional `REQ-SAFE-008` labels are deferred, non-MVP concepts. Thirty days without authorized activity freezes an Open Incident. Freeze means neither safe, danger, resolved, nor verified. No reopening.
  - **Information.**
    - Reuse the Concern provenance model with the five canonical categories, without scores.
    - Entry kinds are observation, Unknown, coordinator summary, contact attempt, task result, correction, and challenge. Entries are append-only, with own-entry correction and others' challenges.
    - Contact attempts are record-only, with actor, target label, fixed method and outcome lists, time, and optional notes. No contact details are pulled from profiles, and there is no call, SMS, or email integration.
  - **Tasks.**
    - A task has a description, one assignee, a status of `Open`/`Done`/`Cancelled`, created-by and created-time, and a completion time.
    - The coordinator creates, assigns, and cancels tasks. Participants create self-assigned tasks.
    - Only the assignee completes a task, with an attributed, categorized result.
    - No reassignment.
    - Also carried from the PRP-029 default: a participant may cancel a task they created for themself.
  - **Timeline.** A read-time chronological projection of the explicit records, without event sourcing. Event time when known, otherwise recorded time.
  - **Resolution.**
    - Coordinator only.
    - Requires an attributed summary, an outcome with category, and the time contact was restored or an explicit Unknown. The system records the resolution timestamp.
    - No preset verdicts.
    - Resolution means coordination ended. It is final and cannot be reopened.
  - **Boundaries.**
    - No automatic location, Check-In, Sharing Session, live-map, or tracking access.
    - Safety Circle and Trusted Contacts get no access or authority, and Trusted Contacts are not participants in v1.
    - No notifications, dispatch, external escalation, automated danger determination, or risk scoring.
  - **Subject membership loss.** The Incident stays intact with the subject's historical identity. Other participants' access is not removed solely because the subject left, and no new access is created.
  - **`OD-004` resolution.** Product defaults, not legal requirements:
    - An Open Incident is retained while Open, with the 30-day inactivity freeze.
    - Frozen history is retained 365 days from freeze, and Resolved history 365 days from resolution.
    - At expiry, history becomes unavailable immediately; physical cleanup may follow.
    - Audit metadata is retained 365 days per event, using the Concern pattern. Denied reads and writes use the `DEC-069` hourly summary keyed by actor, `IncidentReadDenied`/`IncidentWriteDenied`, and UTC hour, with no attempted Incident ID. No Incident content is kept in audit, and there is no audit viewer.
    - Ordinary backups age out within 30 days after active deletion. A restore must not reactivate expired data.
    - Actual backup and restore evidence remains a predeployment dependency.
- **Rationale:** These choices complete the minimum approvable Incident model needed toward Phase 8 exit criterion 2. They reuse the verified Concern access, provenance, audit, and expiry patterns, keep access explicit and nondisclosing, and give Incident history a finite, longer window suited to its coordination and lessons value.
- **Requirement impact:**
  - Add `REQ-PRIV-026` and `REQ-SAFE-016`.
  - Amend `REQ-SAFE-007` (other-domain fields are not unlocked by Incident participation) and `REQ-SAFE-008` (MVP lifecycle; other labels deferred).
  - Amend `REQ-PROD-012` (the manual shortcut is permitted, with no automatic escalation, information transfer, or audience transfer).
  - Editorially update `REQ-PRIV-009`, `REQ-PRIV-025`, and `REQ-SAFE-015` to reference the resolved retention and the Incident rules.
  - Update the glossary invariant.
- **Affected context:** `core/privacy-safety-contract.md`, `core/product-contract.md`, `core/domain-glossary.md`, `core/requirements-index.md`, `workflows/safety.md`, `planning/proposals/PRP-029-phase-8-safety-incident-workflow-and-access.md`, `planning/candidates/CTX-1.26-phase-8-safety-incident-workflow-and-retention.md`, `planning/verification/VER-CTX-1.26.md`, `planning/roadmap.md`, `project-status.md`, `README.md`, and this decision log.
- **Compatibility:** Concern behavior under `REQ-PRIV-025` and `REQ-SAFE-015` is unchanged; starting an Incident does not alter a Concern. `OD-001` and `OD-002` remain open; `OD-003` remains outside MVP. Phase 8 remains `CURRENT / IN PROGRESS`, and the approved exit criteria are unchanged.
- **Canonical effect:** `CTX-1.26` is active after passing `VER-CTX-1.26`; `CTX-1.25` remains an approved historical baseline.
- **Execution authorization:** Decision recording, canonical context update, baseline creation, and verification only. No implementation story (`WI-008-003` is not created), application code, schema, migration, or UI is authorized.
- **Approved by:** Explicit project-owner decision dated 2026-10-01.

## DEC-072 — Close Phase 8 Safety Coordination

- **Status:** ACCEPTED — Phase 8 formally closed
- **Date:** 2026-10-02
- **Decision:** Close Phase 8 — Safety Coordination as `COMPLETE / CLOSED`. All 13 Phase 8 exit criteria approved under `DEC-067` are satisfied; `planning/verification/VER-PH-008.md` maps each one to its evidence. The Concern and Incident MVP Safety Coordination workflows are complete:
  - `PRP-028` and `PRP-029` are `APPROVED`;
  - `WI-008-001` to `WI-008-004` are `DONE / PASS`;
  - `DEC-067`, `DEC-068`, `DEC-069`, and `DEC-071` are in force, and `OD-004` is resolved;
  - the Concern workflow, the Incident server foundation, and the Incident UI are implemented and verified.

  `DEC-070` remains the approved product-wide visual direction. It is presentation only and is not a Phase 8 Safety product decision.
- **Rationale:** The closure assessment found every exit criterion supported by existing work-item verification, owner approvals, SQL-backed server tests, and independent Playwright journeys, with no closure blocker. The project owner explicitly approved closure.
- **Accepted caveats:** Most evidence is `SELF-VERIFIED`; governance does not require independent review for phase closure. Incident Frozen behavior is verified by server and database tests; its browser evidence is component-based and one mocked capture. Neither caveat blocks closure.
- **Deployment boundary:** This closure does not establish production or deployment readiness, backup compliance, or disaster-recovery compliance. Backup-aging and restore evidence is still required before any deployment.
- **Deferred / non-blocking follow-ups (carried forward):**
  - Open decisions `OD-001` (mobile navigation) and `OD-002` (location sharing). Neither is resolved here.
  - Backup-aging and restore evidence, before deployment.
  - Playwright full-suite shared-database isolation.
  - Concern repository stale-tracking, display-name loading efficiency, and reconciliation efficiency.
  - Monitoring of the existing bundle and style-budget warnings.

  These follow-ups do not reopen Phase 8.
- **Affected context:** `README.md`, `project-status.md`, `planning/roadmap.md`, `planning/verification/VER-PH-008.md`, and this decision log.
- **Compatibility:** `CTX-1.26` remains active. This status-only closure changes no canonical requirement, contract, or completed work-item record (precedent: `DEC-065`). `OD-003` remains outside MVP. Phase 9 — Integration & UX Refinement remains `FUTURE`.
- **Execution authorization:** Closure records and status updates only. This decision creates no work item and authorizes no application code, schema, or Phase 9 work. Any future change needs its own approved work item or a later phase.
- **Approved by:** Explicit project-owner decision dated 2026-10-02.

## DEC-073 — Enter Phase 9 Integration & UX Refinement

- **Status:** ACCEPTED — Phase 9 `CURRENT / IN PROGRESS`; Phase 9.5 added to the roadmap
- **Date:** 2026-10-02
- **Decision:**
  - Phase 8 — Safety Coordination remains `COMPLETE / CLOSED` under `DEC-072`.
  - Set Phase 9 — Integration & UX Refinement to `CURRENT / IN PROGRESS`. Phase 9 is integration and UX refinement of the implemented MVP. It is not feature completion.
  - The approved but unimplemented MVP capabilities are **not** cut from the MVP and are **not** part of Phase 9. They are assigned to a new **Phase 9.5 — MVP Feature Completion**, after Phase 9 and before Phase 10 — Testing & Verification. They are: Event RSVP, Event Needs, Event Updates, Check-In, Sharing Sessions, Trusted Contacts, Shared Family Dates, Safety Circles, Household grouping, and notifications. This decision defines no behavior for them.
  - `OD-002` is assigned to Phase 9.5, with Sharing Sessions and Check-In. It is not resolved.
  - Create `PRP-030 — Phase 9 Scope, Themes and Exit Criteria` for owner review. Its scope, themes, exit criteria, and `OD-001` options are not approved by this decision.
- **Numbering:** Phase 9.5 follows the Phase 7.5 precedent (`DEC-062`). Phases 10 and 11 keep their numbers, because `DEC-062`, `CTX-1.23`, and PRP-021 already record that Phases 8–11 retain their numbering. No historical record is renumbered.
- **Rationale:** The Phase 9 assessment found that the implemented areas work individually but not yet as one connected application, and that several approved MVP capabilities had no assigned phase. The owner chose to keep Phase 9 focused on integration and give feature completion its own phase.
- **Affected context:** `README.md`, `project-status.md`, `planning/roadmap.md`, `planning/proposals/PRP-030-phase-9-scope-themes-and-exit-criteria.md`, this decision log, and `claude-handoff/`.
- **Compatibility:** `CTX-1.26` remains active. This is a status and roadmap-sequence decision; no canonical requirement, contract, or workflow changes, so no new baseline is created (precedent: `DEC-063`, `DEC-066`). `OD-001` remains `OPEN` and is a Phase 9 decision area in `PRP-030`. `OD-002` remains `OPEN`. `OD-003` remains outside MVP. `OD-004` remains resolved.
- **Carried forward:** Backup-aging and restore evidence remains required before any deployment and is outside Phase 9. Production authentication remains unselected and outside Phase 9. Phase 9 completion will not establish deployment readiness.
- **Execution authorization:** Governance and status updates and `PRP-030` creation only. No work item (including `WI-009-001`), application code, test, schema, migration, or Phase 9.5 work is authorized.
- **Approved by:** Explicit project-owner roadmap decision and Phase 9 entry authorization dated 2026-10-02.

## DEC-074 — Approve PRP-030, resolve OD-001, set the deep-link Family-context rule, and activate CTX-1.27

- **Status:** ACCEPTED — `PRP-030` approved; `OD-001` resolved; `CTX-1.27` active after `VER-CTX-1.27`
- **Date:** 2026-10-02
- **Decision:**
  - **D1 — PRP-030:** approve `PRP-030 — Phase 9 Scope, Themes and Exit Criteria`: its objective, themes, MUST / SHOULD / LATER classification, Phase 9 boundary, the Phase 9.5 placement of unbuilt MVP capabilities (`DEC-073`), the exit criteria as updated for this decision, and the test-isolation-first approach.
  - **D2 — OD-001 resolved (Option A):** the MVP primary navigation has exactly six areas at desktop and narrow widths: Home, Family, Events, Plans & Trips, Safety, Me / Settings. "Plans & Trips" and "Safety" are navigation groupings only. Plans and Trips stay separate domains and routes; Concerns and Incidents stay separate Safety workflows and routes. The groupings merge no authorization, domain model, data, or lifecycle, and Safety navigation grants no access. A grouping may use a simple landing page that reaches both of its parts; direct routes stay refresh-safe.
  - **D2 — validation standard:** the navigation model is adequately validated for the MVP only when owner visual review, desktop and narrow rendered evidence, a keyboard and navigation accessibility review, and automated verification that every implemented primary area is reachable through main navigation are all satisfied. Automated accessibility scanning may supplement this but does not replace keyboard and manual review.
  - **D3 — accessibility scanner:** approve `@axe-core/playwright` as a **development-only test dependency** for Phase 9, for automated detection of common accessibility violations and route-level regression checks. It does not replace keyboard, focus, semantic, owner visual, or targeted manual accessibility review. No other accessibility package and no production or runtime dependency is approved.
  - **D4 — deep-link Family context:** when a user opens a refresh-safe item URL for an Event, Plan, Trip, Concern, or Incident, the server first establishes that the user may read that item. Only after authorization succeeds does the application learn the item's Family, switch the selected Family to it if different, render the item, and optionally announce the switch calmly. The application never switches Family from the URL or client state alone, never reveals the target Family before authorization, never infers authorization from Family membership, and never bypasses the item's own access rules. A nonexistent or unauthorized item shows the normal generic unavailable state, leaves the selected Family unchanged, and reveals nothing about the item or Family.
- **Canonical effect:** OD-001 resolution and the deep-link rule are normative UX and privacy behavior. `REQ-UX-011` previously stated that the final grouping was not approved and allowed the Family to change only by explicit selection, so canonical updates are required:
  - `REQ-UX-011` amended (conceptual destinations aligned with `REQ-PROD-007`; navigation delegated to `REQ-UX-013`; the authorized deep-link switch added as the only other way the selected Family may change);
  - `REQ-UX-013` added (six-area navigation and its validation standard);
  - `REQ-UX-014` added (refresh-safe item URLs and deep-link Family context);
  - `REQ-PRIV-027` added (item URLs, navigation, and selected Family are not authorization; nondisclosing unavailable result).

  `CTX-1.27` is active after `VER-CTX-1.27` passed. `CTX-1.26` becomes an approved historical baseline. The Phase 9.5 roadmap addition (`DEC-073`) did not and does not require a baseline.
- **Planning findings recorded in PRP-030:**
  - Events and Plans have only Family-scoped list endpoints. Item URLs under `REQ-UX-014` will need additive single-item server reads under the existing authorization in a later, separately approved work item. Trips, Concerns, and Incidents already have single-item reads.
  - `DEC-033` excludes administrator assignments from the Family directory. Showing administrators there (PRP-030 S3) needs a later owner decision revisiting `DEC-033`, not only a server read.
- **Compatibility:** Phase 9 remains `CURRENT / IN PROGRESS`; Phase 9.5 remains `FUTURE`. `OD-002` remains `OPEN` and assigned to Phase 9.5. `OD-003` remains outside MVP. `OD-004` remains resolved. No Concern, Incident, Event, Plan, Trip, membership, or administrator permission changes. Production authentication remains unselected.
- **Affected context:** `core/ux-contract.md`, `core/privacy-safety-contract.md`, `core/requirements-index.md`, `planning/candidates/CTX-1.27-phase-9-navigation-and-item-links.md`, `planning/verification/VER-CTX-1.27.md`, `planning/proposals/PRP-030-phase-9-scope-themes-and-exit-criteria.md`, `planning/stories/WI-009-001-playwright-per-spec-scenario-isolation.md`, `README.md`, `project-status.md`, `planning/roadmap.md`, this decision log, and `claude-handoff/`.
- **Execution authorization:** Decision records, the canonical context update and its verification, and preparation of `WI-009-001` only. No application code, test code, dependency installation, or work-item execution is authorized by this decision. `WI-009-001` requires its own execution authorization.
- **Approved by:** Explicit project-owner decisions D1–D4 dated 2026-10-02.

## DEC-075 — Resolve WI-009-005 decisions OD-A to OD-I, clarify REQ-UX-014, and activate CTX-1.28

- **Status:** ACCEPTED — `WI-009-005` approved and execution-authorized; `CTX-1.28` active after `VER-CTX-1.28`
- **Date:** 2026-10-02
- **Decision:**
  - **OD-A — Authorized Trip with no selectable Family:** a narrowly bounded Trip exception.
    - The Trip creator keeps Trip access after leaving the Trip's Family (`REQ-PRIV-022`). An authorized Trip may therefore reference a Family that is not in the caller's active Family list.
    - The Trip item read still happens first, and server authorization stays authoritative.
    - The application does not switch the selected Family and does not create a selectable Family. It renders the authorized Trip with the existing Trip behavior and keeps the current selection, if any.
    - The exception applies only where existing Trip authorization already allows the item after membership ends. There is no equivalent exception for Events, Plans, Concerns, or Incidents.
    - Trip access is not broadened. This clarifies `REQ-UX-014` / `DEC-074` D4; it is not an authorization change.
  - **OD-B — Stale or missing Family context:** after the server authorizes an item:
    1. wait for the caller's current Family list;
    2. look for the item's `FamilyGroupId`;
    3. if it is absent, refresh `/api/family-groups/mine` **once**;
    4. check again.

    If the Family is now present, switch normally if required. If it is still absent:
    - **Trip:** apply OD-A only where the existing Trip authorization legitimately allows reading without active membership.
    - **Event, Plan, Concern, Incident:** do not switch, do not render the item under an unrelated Family, and show the normal generic unavailable state with the selected Family unchanged.

    The client never retries repeatedly and never treats its Family list as authorization.
  - **OD-C — Event and Plan detail:** minimal routed detail views inside the current `App` structure.
    - No broad `app.ts` / `app.html` extraction. Existing presentation and behavior are reused.
    - The Event detail exposes only the caller's existing Event actions, including the existing Edit when `CanEdit` is true. The Plan detail is read-only.
    - If the Event presentation cannot be reused safely without a small extraction, the implementer stops and reports the exact proposed Event-card extraction.
  - **OD-D — Routes:** `/events/:eventId`, `/plans/:planId`, `/trips/:tripId`, and `/concerns/:concernId` are approved. The existing `/incidents/:incidentId` is kept. GUID parameter handling is repository-consistent, and existing list and grouping routes are unchanged.
  - **OD-E — Second-Family E2E fixture:** a narrowly scoped Development / E2E fixture capability for a second Family is approved, so cross-Family deep links can be proven end to end.
    - It is opt-in through the isolated Playwright scenario mechanism, enabled only for specs that request it, and not seeded for ordinary Development use. It is never active in production.
    - It makes no production API, domain, or schema change.
    - It uses deterministic IDs and state, and is reusable by the later Phase 9 multi-Family journey.
    - It adds no user-facing Family-creation workflow. It is test-fixture support only.
  - **OD-F — Cross-Family Concern and Incident open:** the visible behavior change required by `DEC-074` D4 is approved. Opening an authorized Concern or Incident whose Family differs from the selected Family:
    1. authorizes the item on the server;
    2. takes `FamilyGroupId` from the authorized response;
    3. locates that Family in the caller's Family list;
    4. switches;
    5. reloads Family-scoped context;
    6. renders.

    Never switch from the route, list metadata alone, navigation state, or a guess. The same principle applies to Event, Plan, and active-Family Trip deep links.
  - **OD-G — Concern creation destination:** after a Concern is successfully created, the application navigates to `/concerns/{concernId}`. This replaces in-place, state-only display of the created Concern. Current creation behavior, permissions, data, and lifecycle are preserved, and no additional information is copied or exposed.
  - **OD-H — Family-switch announcement:** a polite status announcement, "Switched to {Family name}", appears only when the application actually performs an automatic Family switch. The name comes from the caller's own Family list. There is no announcement for same-Family opens, unavailable items, or before authorization succeeds. No visual notification infrastructure is added.
  - **OD-I — Plan create `Location` header:** kept out of `WI-009-005`. It stays a recorded, unscheduled server/API cleanup follow-up.
- **Canonical effect:** OD-A and OD-B change normative text in `REQ-UX-014` step 2, which previously required a switch for every authorized item in a different Family. `REQ-UX-014` is amended in place; see `planning/candidates/CTX-1.28-item-link-family-context-clarification.md`. `REQ-PRIV-027`, `REQ-PRIV-022`, and `REQ-UX-011` are unchanged. OD-C to OD-I need no canonical change. `CTX-1.28` is active after `VER-CTX-1.28` passed; `CTX-1.27` becomes an approved historical baseline.
- **Compatibility:**
  - Phase 9 remains `CURRENT / IN PROGRESS`.
  - No Event, Plan, Trip, Concern, Incident, membership, or administrator permission changes.
  - `OD-002` remains `OPEN` (Phase 9.5); `OD-003` remains outside MVP.
- **Affected context:** `core/ux-contract.md`, `core/requirements-index.md`, `planning/candidates/CTX-1.28-item-link-family-context-clarification.md`, `planning/verification/VER-CTX-1.28.md`, `planning/stories/WI-009-005-refresh-safe-item-urls-and-authorize-first-deep-links.md`, `README.md`, `project-status.md`, `planning/roadmap.md`, this decision log, and `claude-handoff/`.
- **Execution authorization:** `WI-009-005` is `APPROVED — EXECUTION AUTHORIZED` against `CTX-1.28`, within the scope, decisions, acceptance checks, and stop conditions recorded in that work item, including the OD-E Development / E2E fixture. No further routine approval is needed. No commit or push is authorized.
- **Approved by:** Explicit project-owner decisions OD-A to OD-I dated 2026-10-02.

## DEC-076 — Approve the WI-009-006 Home dashboard prototype alignment and supersede the WI-009-005 AC-16 Home-presentation clause

- **Status:** ACCEPTED — owner visual approval of completed presentation-only Home work
- **Date:** 2026-10-03
- **Decision:**
  - **Work item:** `WI-009-006 — Home Dashboard Prototype Alignment` owns the Home dashboard redesign.
    - The redesign was started at the owner's direction during the `WI-009-005` owner visual review.
    - The story formalizes the existing implementation. It is not recreated.
  - **Owner visual approval: PASS** (2026-10-03) for the design shown in the review contact sheet, now held in `planning/verification/evidence/WI-009-006/`. The approved design:
    - places the dashboard grid after panel ① of `references/ux/home-dashboard`;
    - greets by time of day with the signed-in display name, with the Family name in the subtitle;
    - has a compact "View Family" action and a compact Needs Your Attention empty state;
    - shows real Upcoming Events, Trips (heading "Trips"), and Plans;
    - keeps a compact Safety coordination card, with Plans and Safety sharing a row;
    - reserves Recent Family Updates, My Tasks, and Active Sharing as text-only placeholders, with no badge;
    - removes the redundant low-data strip;
    - has responsive layouts at 1440, 1024, 768, and 390 px.
  - **DEC-070 accepted differences revisited by new owner request:**
    - "No activity feed, Tasks, or Active Sharing" now means no such *capability*. Honest, data-free placeholder cards are allowed on Home.
    - Every other `DEC-070` accepted difference stands.
  - **WI-009-005 `AC-16`, superseded in part:**
    - Still required: the Home Event, Plan, and Trip row links and their item navigation, semantic links, and authorization.
    - Superseded by `WI-009-006`: the clause requiring the Home Safety card, "View all", "Needs your attention", and the low-data card to keep the same text, button count, and composition.
    - `WI-009-005` is not failed by this intentional redesign. Its historical record keeps the original wording, with an explicit supersession note.
- **Canonical effect:** presentation only. No requirement, contract, privacy rule, authorization, API, schema, or migration changes, so there is no new context baseline. `CTX-1.28` remains active. Mockups remain non-authoritative design evidence.
- **Compatibility:**
  - No Phase 9.5 capability is implemented: activity feed, tasks, Sharing Sessions, RSVP, Check-In, and notifications are all absent.
  - `OD-001` remains resolved. `OD-002` remains `OPEN` (Phase 9.5).
  - Home data queries (Event, Plan, and Trip filtering and ordering) are unchanged.
- **Affected context:**
  - `planning/stories/WI-009-006-home-dashboard-prototype-alignment.md`, `planning/verification/VER-WI-009-006.md`, and `evidence/WI-009-006/`;
  - the `WI-009-005` story and `VER-WI-009-005` (the `AC-16` note and the Home evidence recapture);
  - `README.md`, `project-status.md`, `planning/roadmap.md`, and `claude-handoff/`.
- **Execution authorization:** `WI-009-006` final verification and closure records only. No further implementation work item is authorized. No commit or push is authorized.
- **Approved by:** Explicit project-owner visual approval dated 2026-10-03.

## DEC-077 — Approve the WI-009-007 Family dashboard and Family Settings split, and allow honest Family placeholders

- **Status:** ACCEPTED — owner visual approval of presentation-only Family work
- **Date:** 2026-10-03
- **Decision:**
  - **Work item:** `WI-009-007 — Family Dashboard Prototype Alignment` owns the Family redesign after panel ② "Family Activity and Coordination" of `references/ux/home-dashboard`.
  - **Owner visual approval: PASS** (2026-10-03) for the design in `planning/verification/evidence/WI-009-007/`.
  - **Dashboard and Settings split:**
    - `/family` is the Family coordination dashboard.
    - `/family/settings` is a new Family child page holding the existing management functions. Family stays the selected primary area on it (`aria-current="true"`).
    - The six primary destinations (`REQ-UX-013`) are unchanged.
  - **Owner decisions:**
    - **D1:** honest, data-free placeholders are allowed on the Family dashboard (Upcoming birthdays, Recent Family Updates, Recent Check-Ins). This extends `DEC-076`, which allowed placeholders on Home only. The `DEC-070` "no activity feed / Check-Ins" difference still means no *capability*.
    - **D2:** "Waiting for you" replaces "Pending Invitations". It shows only real actionable items: today, relationship suggestions sent to the signed-in adult.
    - **D3:** the page title is "Family".
    - **D4:** every member sees the Family settings gear. The gear is navigation only; Settings renders only the functions the member may use, and Invite stays administrator-only.
    - **D5:** a member tile shows a second line only for an accepted relationship between that member and the signed-in adult (that member's label). There is no fallback label and no administrator status.
    - **D6:** the date is hidden at narrow widths, as on Home.
    - **D7:** the placeholder copy is "Birthdays shared with your family will appear here.", "Family updates will appear here as activity is shared.", and "Recent family check-ins will appear here when available."
- **Canonical effect:** presentation and client routing only. No requirement, contract, privacy rule, authorization, API, schema, or migration changes, so there is no new context baseline. `CTX-1.28` remains active.
- **Compatibility:**
  - No Phase 9.5 capability: no Check-Ins, activity feed, Shared Family Dates (`REQ-FAM-010`), notifications, Tasks, Sharing Sessions, or RSVP.
  - `DEC-033` / `PRP-008` directory limits and `REQ-PRIV-013` to `REQ-PRIV-015` are unchanged. `OD-002` remains `OPEN`.
- **Affected context:**
  - `planning/stories/WI-009-007-family-dashboard-prototype-alignment.md`, `planning/verification/VER-WI-009-007.md`, and `evidence/WI-009-007/`;
  - `README.md`, `project-status.md`, `planning/roadmap.md`, and `claude-handoff/`.
- **Execution authorization:** `WI-009-007` verification and closure records only. No further implementation work item is authorized. No commit or push is authorized.
- **Approved by:** Explicit project-owner visual approval and decisions D1 to D7 dated 2026-10-03.

## DEC-078 — Approve the WI-009-008 Events dashboard direction and its route split, and allow an honest Events placeholder

- **Status:** ACCEPTED — owner design decisions D1–D11 after the panel ③ design review; owner visual approval `PASS` (2026-10-03); `WI-009-008` `DONE / PASS` (`VER-WI-009-008`)
- **Date:** 2026-10-03
- **Decision:**
  - **Work item:** `WI-009-008 — Events Dashboard Prototype Alignment` owns the Events redesign after panel ③ "Travel & Events Summary" of `references/ux/home-dashboard`. Execution is authorized by the owner.
  - **Owner decisions:**
    - **D1:** `/events` becomes the Events dashboard. The six primary destinations (`REQ-UX-013`) are unchanged; there is no "Travel & Events" area.
    - **D2:** the page title is "Events".
    - **D3:** a read-only **Trips** summary card on the Events dashboard, using Home's current-Family selection. Trips from any other Family, including retained creator Trips (`REQ-PRIV-022`), never appear. No Trip status or audience state.
    - **D4:** Event creation moves to `/events/new` with all existing behaviour and help text. This revisits the `DEC-070` accepted difference "Events and Plans create forms remain visible beside their lists" for **Events only**; Plans is unchanged.
    - **D5:** one header action, "Create event", to `/events/new`. No create menu covering more than one domain.
    - **D6:** the complete Event list, with past Events and inline edit, moves to `/events/all`.
    - **D7:** "RSVPs Needed" is a text-only placeholder ("Event invitations that need your response will appear here."). This extends the `DEC-076` / `DEC-077` placeholder allowance to the Events dashboard. RSVP stays a Phase 9.5 capability (`REQ-EVT-010`).
    - **D8:** six Quick Links, all real destinations: Create an event, All events, Trips, Plans, Family members, Your profile. No Calendar, Shared Lists, or Travel Documents tiles.
    - **D9:** no pill on Event rows.
    - **D10:** after a successful create, navigate to `/events/{eventId}`.
    - **D11:** `/plans-trips` is not redesigned in this item.
    - **Final (owner visual review, 2026-10-03):** the Event item page title changes from "Family events" to "Events" (terminology only). The deliberate panel ③ differences are accepted and not to be reopened: date tiles instead of photos; no RSVP or Trip status pills; "Trips" instead of "Active Trips"; the RSVPs placeholder; real Quick Links; no search, bell, avatar menu, or Messages; the six-area navigation; "Create event".
  - **Earlier guidance revisited:** the `claude-handoff/MOCKUP-ALIGNMENT.md` "DROP the combined Travel & Events page" treatment is revisited only in part. Events stays its own area and domain; the Trips card is a cross-area read-only summary, like Home. No screens, domains, APIs, records, or rules are merged. The `DEC-070` differences "no RSVP / Respond / Maybe" and "no unsupported Trip statuses" still stand.
- **Canonical effect:** presentation and client routing only. No requirement, contract, privacy rule, authorization, API, schema, or migration changes. `REQ-UX-013` already allows summaries that do not merge screens, and `REQ-UX-014` item URLs are unchanged, so there is no new context baseline. `CTX-1.28` remains active.
- **Compatibility:** no Phase 9.5 capability (no RSVP, invitations, Needs, Updates, Check-Ins, notifications, Calendar, Shared Lists, Travel Documents, Trip statuses, or location sharing). `OD-002` remains `OPEN`.
- **Affected context:** `planning/stories/WI-009-008-events-dashboard-prototype-alignment.md`, `planning/verification/VER-WI-009-008.md`, `evidence/WI-009-008/`, `README.md`, `project-status.md`, `planning/roadmap.md`, and `claude-handoff/`.
- **Execution authorization:** `WI-009-008` implementation, visual review, verification, and closure records. No further implementation work item is authorized. No commit or push is authorized.
- **Approved by:** Explicit project-owner decisions D1 to D11 and owner visual approval, dated 2026-10-03.

## DEC-079 — Approve the WI-009-009 Safety dashboard and revisit the data-free Safety landing (WI-009-003 D1 / AC-15) for /safety only

- **Status:** ACCEPTED — owner design decisions O1–O11 after the panel ④ design review; owner visual approval `PASS` (2026-10-03) with final decisions O12–O14; `WI-009-009` `DONE / PASS` (`VER-WI-009-009`)
- **Date:** 2026-10-03
- **Decision:**
  - **Work item:** `WI-009-009 — Safety Dashboard Prototype Alignment` owns the Safety redesign after panel ④ "Safety-Aware Dashboard" of `references/ux/home-dashboard/home-dashboard-overview.png`. The newer `home-dashboard-redesign.png` is not a design authority for this item.
  - **`/safety` may contain authorized read-only summaries.** It becomes a Safety dashboard showing the Open Concerns and Open Safety Incidents returned to the caller by the existing authorized list reads (`GET /api/concerns`, `GET /api/incidents`), a Recent Check-Ins placeholder, and static guidance. It requires no selected Family.
  - **WI-009-003 revisited for `/safety` only.** D1 ("does not fetch Concern or Incident data … looks the same whether or not the caller has any records") and `AC-15` no longer apply to `/safety`. They still apply to `/plans-trips` and to the navigation itself, which carries no count, badge, or state.
  - **Separate workflows.** Concerns and Incidents remain separate workflows with separate cards, routes, access rules, and lifecycles: no combined list, timeline, count, workflow, or create menu (`REQ-UX-013`).
  - **No disclosure to unauthorized callers.** Every row comes from the server's per-caller authorization. A caller sees only items already visible to them on `/concerns` and `/incidents`; empty states read "No open … are shared with you." and say nothing about whether other items exist. Summaries are minimal: no Concern reason, entry, contact attempt, task, or timeline content.
  - **Check-Ins placeholder only.** "Recent Check-Ins" with "Check-ins shared with you will appear here when available." No request, name, time, count, or state. Check-In remains a Phase 9.5 capability (`REQ-SHARE-006`–`008`). This extends the `DEC-076` / `DEC-077` / `DEC-078` placeholder allowance to Safety.
  - **No safety inference.** The dashboard never states or implies that anyone is safe, unsafe, monitored, or in danger (`REQ-SAFE-016`); only the MVP states Open / Frozen / Resolved are used (`REQ-SAFE-008`).
  - **No actions that escalate or imitate missing capability:** no create, resolve, contact, Safety Settings, or emergency controls on `/safety`.
  - **Final owner decisions (visual review, 2026-10-03):** O12 keeps "Concern about {name}" / "Incident about {name}" with the authorized subject display name only; O13 words additional rows "Additional open Concerns are available." / "Additional open Safety Incidents are available." with no count; O14 lets the Check-Ins card match its neighbour's height with its note top-aligned at natural size. The unreachable Safety branch of the group landing is removed as cleanup caused by this decision.
  - **Owner decisions O1–O11** are recorded in the work item (title "Safety", subtitle "Concerns and Safety Incidents shared with you."; Family names from authorized rows for multi-Family callers; Back to Safety on `/concerns` and `/incidents`; Home Safety card unchanged).
- **Canonical effect:** none. The owner's interpretation is that `REQ-UX-013` ("a simple landing page or an equivalent control that reaches both of its parts"; no merging; reaching Safety grants no access and implies no item exists) permits authorized, separate, read-only summaries without amendment. `CTX-1.28` remains active; no `CTX-1.29`. A canonical amendment is required only if implementation reveals a real requirement conflict.
- **Compatibility:** no API, endpoint, contract, schema, migration, authorization, or server change. No Phase 9.5 capability. `OD-002` remains `OPEN`.
- **Affected context:** `planning/stories/WI-009-009-safety-dashboard-prototype-alignment.md`, the transient review captures, `project-status.md`, `planning/roadmap.md`, and `claude-handoff/`. `claude-handoff/MOCKUP-ALIGNMENT.md` (panel ④ mapped to the Concerns list) is superseded for panel ④ by this decision.
- **Execution authorization:** `WI-009-009` implementation and visual review. Tests start only after owner visual approval. No commit or push is authorized.
- **Approved by:** Explicit project-owner decisions O1–O11 dated 2026-10-03.

## DEC-080 — Approve the WI-009-010 first-use and invitation arrival workflow

- **Status:** ACCEPTED — owner decisions O1–O9 after the first-use / onboarding review; owner review `PASS` (2026-10-03) with final decisions O10–O11
- **Date:** 2026-10-03
- **Decision:**
  - **Arrival on Home:** a successful invitation arrival (acceptance, the server-confirmed membership reload, selection of the accepted Family, then the optional profile review) now completes on `/home` through the router. A one-time polite announcement, "You joined {Family}.", names the Family from the caller's own membership list and does not survive a refresh.
  - **Planning basis:** this implements `PRP-030` M7 and its first-use targets (approved under `DEC-074`).
  - **Superseded in part:** the `PRP-022` / `WI-075-001` "Continue to Family" destination. `WI-075-001` is not failed; the rest of its behavior is preserved.
  - **Profile review:** remains optional and skippable.
  - **Exits:** every invitation state that cannot continue offers "Go to Home".
  - **No-Family guidance:** plain-language guidance with one real action, "Review your profile" (`/me`).
  - **First Family choice:** with no Family selected, choosing one keeps the current page.
  - **Administrator invite feedback:** renders in the Invite card; the administrator privacy statement appears once on Family settings.
  - **Boundaries:**
    - Family membership remains invitation-only (`REQ-FAM-005`), and the first Family remains operator-provisioned (`REQ-FAM-006`). No self-service Family creation, join request, search, or join code was introduced.
    - Invitation privacy and the uniform unavailable semantics (`REQ-FAM-008`) are unchanged.
    - No authentication, sign-up, provisioning, email delivery, or email-ownership capability was added.
  - **Out of scope:** the one-person-Family Event limitation (O8), and authentication and provisioning (O9).
- **Canonical effect:** none. `REQ-UX-011` (sole-Family auto-selection, session-only selection, "use the approved invitation/operator-provisioned entry path" with no Family) and `REQ-FAM-005`–`008` already support this workflow, so there is no new context baseline. `CTX-1.28` remains active.
- **Compatibility:** client presentation and navigation only. No API, endpoint, contract, schema, migration, authorization, or invitation-token change. No Phase 9.5 capability. `OD-002` remains `OPEN`.
- **Affected context:** `planning/stories/WI-009-010-first-use-and-invitation-arrival-workflow.md`, the transient review captures, `project-status.md`, `planning/roadmap.md`, and `claude-handoff/`.
- **Owner review (2026-10-03): `PASS`.** Approved: the `/home` destination, the one-time announcement, the optional profile review, the Go to Home exits, the no-Family guidance and `/me` link, the narrow first-choice behaviour, the Invite card feedback, the consolidated administrator wording, and 390 px behaviour. Final decisions:
  - **O10:** keep "Continue to Home" and "Continue to Home without changes".
  - **O11:** fix the duplicate no-Family messages at view/template level only: when the shared Family-required prompt shows its no-Family state, the Events, Plans, and Family-directory inner no-Family copy does not render. The global `.page-content` hiding rule is not changed, so the Home profile-load error is preserved.
- **Execution authorization:** `WI-009-010` implementation, owner review, then testing, final regression, and closure. No commit or push is authorized.
- **Approved by:** Explicit project-owner decisions O1–O9 and O10–O11, and the owner review, dated 2026-10-03.

## DEC-081 — Close Phase 9 Integration & UX Refinement

- **Status:** ACCEPTED — owner conditional closure gates satisfied; Phase 9 formally closed
- **Date:** 2026-10-03 (America/New_York; run identifiers use 2026-10-04 UTC)
- **Decision:** Close Phase 9 as `COMPLETE / CLOSED` and WI-009-011 as `DONE / PASS — OWNER APPROVED`. VER-WI-009-011 is `DONE / PASS`; VER-PH-009 is `PASS / PHASE CLOSURE VERIFIED`. All ten PRP-030 MUST outcomes and all 15 formal exits are satisfied, each mapped to one permitted classification in VER-PH-009.
- **Outcome reconciliation:**
  - **M5:** `PASS — owner-approved substitution`. Home retains lightweight Safety navigation; `/safety` is the authorized data-bearing dashboard. Separate Concern/Incident workflows and access remain. This replaces the original Home Safety-summary outcome without moving summaries onto Home.
  - **M6:** `PASS — direct evidence`. Events use current/future dates; Trips use end date >= local today without completion inference. Plans use expected-return date >= local today, falling back to start date when expected return is absent. Calendar-date comparison, current-Family scope, authorization, filtering before cap, existing sorting, three rows, links and approved Home design remain. No lifecycle/API/schema/domain change.
  - **M8:** `PASS — direct evidence`, filed six-area consistency checklist and preserved accepted domain/cosmetic differences.
  - **M9:** `PASS — direct evidence`, continuous invited-adult journey, returning multi-Family evidence and actual keyboard paths.
  - **M10:** `PASS — direct evidence`, dev-only axe at all 13 routes at 1440/390 plus Home states: 30 final scans, zero violations/incomplete, no suppression; actual keyboard-only invited/returning journeys. Two moderate landmark naming defects fixed without visual changes.
- **Final verification:** Home components 25/25 including eight new Plan tests; focused Plan browser 1/1 with six desktop/mobile renders; earlier full Angular 271/271 and unaffected evidence retained per owner instruction. One final production build PASS (main 597.77 kB, complete initial 610.24 kB, estimated transfer 134.28 kB), unchanged budgets. Exactly one executed final full Playwright run `20261004T013144-591feecb`: 34/34 PASS, 0 failed/skipped, 13/13 databases removed. Final health proves the original 18-database set unchanged, both ports free and zero API processes.
- **Evidence / caveats:** `planning/verification/VER-PH-009.md`, `VER-WI-009-011.md` and linked evidence. Verification is `SELF-VERIFIED`; actual agent-led keyboard review and existing owner visual approvals are distinct from automated tests. No new human usability study or full 279-test Angular run is claimed.
- **Accepted non-blocking follow-ups:** **IMPORTANT FOLLOW-UP — NOT A PHASE 9 BLOCKER:** Plan-create Location currently uses `/api/family-groups/{familyGroupId}/plans/{planId}`, while the item route is `/api/plans/{planId}`; unchanged here. One-person Event intentionally requires another active adult; clearer explanatory copy is later UX work. Existing bundle/style warning monitoring and earlier accepted technical follow-ups remain separate.
- **Phase 9.5 boundary:** `READY TO BEGIN` under the owner's latest conditional direction. This is readiness for separately scoped/approved work, not feature implementation authorization. OD-002 remains open; no location, RSVP/Needs/Updates, Check-In, Sharing Session, Trusted Contact, Shared Family Date, Safety Circle, Household or notification capability was implemented here.
- **Predeployment boundary:** Development user testing is `READY`; this closure does not certify deployment. Production authentication, hosting, backup/restore, production data and protected artifact cleanup remain outside this work.
- **Artifact boundary:** Only WI-009-011's exact scenario identifiers were cleaned with approved tooling. Historical/cited/interrupted records, protected and unknown databases, root `trips`, revoked WI-009-010 invitations and earlier canonical evidence remain.
- **Canonical effect / compatibility:** No canonical requirement changed. `CTX-1.28` remains ACTIVE; no CTX-1.29. D2 interprets summary relevance from existing Plan dates without introducing completion behavior.
- **Supersedes:** Phase 9 in-progress status under DEC-073 and WI-009-011's reported Plan/final-gate blockers. Original PRP-030 M5 Home placement is replaced by D1 above; no other contract or feature decision is overridden.
- **Affected context:** this decision log, README, project-status, planning/roadmap, WI-009-011, VER-WI-009-011, VER-PH-009 and synchronized claude-handoff current-state files.
- **Execution authorization:** WI-009-011 is complete. No implementation work item remains authorized. Next work requires the owner's scoped approval. No commit/push or Git initialization.
- **Approved by:** Explicit project-owner WI-009-011 authorization and conditional closure request, followed by the owner D2 Plan filtering decision and continuation instruction dated 2026-10-03. Created only after all remaining verification and artifact gates passed; this does not claim a later separate owner review.

## DEC-082 — Creator edit and clear of the existing optional Plan note

- **Status:** ACCEPTED — separately owner-authorized post-Phase-9 follow-up
- **Date:** 2026-10-04
- **Decision:** The Plan creator may edit or clear the Plan's existing optional note after creation while they retain current Plan access. Continue using `FamilyPlan.Notes`, with plain-text semantics, the 1,000-character maximum, and blank-as-absent behavior. The write is server-authorized for the active creator; existing Plan reads and audience rules do not change.
- **Rationale:** Let the creator correct or remove the one optional Plan note without broadening Plan editing or adding a separate Notes capability.
- **Canonical effect:** Add `REQ-PLAN-007` under `CTX-1.29`; `REQ-PLAN-006` remains the creation requirement and historical `DEC-056` remains intact. Phase 9 remains `COMPLETE / CLOSED` under `DEC-081`, and `VER-PH-009` remains its historical closure evidence.
- **Boundary:** No general Plan editing, multiple authored notes, note-specific author metadata, revision history, Tasks, Check-Ins, or other Phase 9.5 capability is approved here.
- **Affected context:** `workflows/plans-trips.md`, `core/requirements-index.md`, `planning/candidates/CTX-1.29-plan-note-edit.md`, `planning/verification/VER-CTX-1.29.md`, `README.md`, `project-status.md`, `planning/roadmap.md`, and current `claude-handoff` summaries.
- **Supersedes:** None.
- **Approved by:** Explicit project-owner implementation request dated 2026-10-04.

## DEC-083 — Approve the first Phase 9.5 Event Invitations and Self RSVP scope

- **Status:** ACCEPTED — owner-approved context/work-item scope; CTX-1.30 activated after verification
- **Date:** 2026-10-04
- **Decision:** Approve only `WI-095-001 — Event Invitations and RSVP` as Phase 9.5's first bounded item, in-app only. Outbound delivery and notification infrastructure are excluded. Discovery uses the existing Events dashboard RSVP/attention area for the caller's own active Pending invitations in the selected Family, including past-dated Events; actions use Event detail and the existing item URL.
- **Canonical decisions:**
  - Initial Pending requests a response; creation is not a response and leaves the deliberate RSVP-updated timestamp absent. Eligible invitees freely choose the four existing states for themselves (`REQ-EVT-014`).
  - Current membership and Event access control invitation use. Temporary loss suspends invitee reads/actions without revoking; the same profile regaining both may use an unrevoked record. Selected Family, Account similarity, new profiles, and invitation-ID knowledge grant nothing (`REQ-PRIV-028`).
  - The currently authorized organizer sees named invitation identity/person, active/revoked state and current RSVP and alone manages invitations. Eligible invitees see their own invitation/RSVP; other viewers/editors see no named data. Editor status gives no management power (`REQ-PRIV-028`; existing `REQ-EVT-009`–`010`).
  - No organizer self-invitation or self-RSVP from Event creation (`REQ-EVT-014`).
  - Revoked rows/final RSVP follow the parent Event lifecycle; re-invitation has a new ID. No independent revision table, public history, or retention timer (`REQ-EVT-014`).
  - Organizer access loss ends organizer authority without freezing independently eligible invitees' own RSVP (`REQ-PRIV-028`).
- **Architecture boundary:** Do not extract existing routed Event detail/edit from App. Permit only a small new `src/app/events/` participation boundary for invitation/RSVP presentation, client state, domain API calls consistent with existing frontend conventions, and focused specs/styles. It receives an already authorized Event from the existing App/item-route path. App retains dashboard ownership except minimum discovery integration, Event create/list/all/card/edit, `openRoutedItem`, authorize-first Family adoption, and selected-Family shell behavior. Fuller extraction may be reassessed for separately approved Needs/Updates growth; no standalone refactor.
- **Implementation intent:** One minimum invitation record (stable ID, Event ID, invitee profile, creation/revocation times, current RSVP, deliberate response time, narrow concurrency token); derive organizer from the Event creator rule. No participant abstraction, RSVP revision table, workflow engine, notification or Need data. Material reasons to revise this direction require stopping and reporting.
- **Canonical effect:** Add `REQ-EVT-014` and `REQ-PRIV-028` under candidate `CTX-1.30`, supplementing unchanged `REQ-EVT-010` and `REQ-PRIV-023`. The Completion paragraph recognizes approved invitation retention without resolving completion. Existing Event audience/access/edit/Need/Update rules remain. `CTX-1.29` becomes historical only after `VER-CTX-1.30` passes and explicit activation is recorded.
- **Phase boundary:** After verified activation, Phase 9.5 is `CURRENT / IN PROGRESS` for this item alone. Later DEC-073 capabilities require separate owner/work-item approval and execution gates; OD-002 remains OPEN. Phase 9 stays `COMPLETE / CLOSED`; DEC-081, WI-009-011, VER-WI-009-011, and VER-PH-009 remain unchanged.
- **Execution authorization:** Approved work item YES; product execution NO. This owner instruction authorizes canonical context/governance preparation and verified CTX activation only. No entities, EF mappings, migrations, endpoints/contracts, Angular changes, participation components, tests or RSVP implementation. A separate explicit owner execution instruction is required for WI-095-001. No Git staging, commit, push, or PR.
- **Activation (2026-10-04):** Candidate VER-CTX-1.30 PASS / SELF-VERIFIED recorded before explicit activation. CTX-1.30 is ACTIVE; current-state routing synchronized. WI-095-001 APPROVED, execution NO.
- **Compatibility:** Existing implemented behavior remains compatible and unchanged; new participation behavior is approved context, unimplemented. Implementation must re-evaluate this baseline if canonical rules change before execution.
- **Affected context:** `workflows/events.md`, `core/privacy-safety-contract.md`, `core/requirements-index.md`, `planning/stories/WI-095-001-event-invitations-and-rsvp.md`, `planning/candidates/CTX-1.30-event-invitations-and-rsvp.md`, `planning/verification/VER-CTX-1.30.md`, README, project-status, roadmap, and current handoff summaries.
- **Supersedes:** Only Phase 9.5 readiness/current-state routing once verified activation occurs. No historical closure or prior requirement is superseded.
- **Approved by:** Explicit owner APPROVED CONTEXT / GOVERNANCE PREPARATION instruction dated 2026-10-04.

## DEC-084 — Event Needs responsibility privacy and execution

- **Status/date:** ACCEPTED — explicit owner decision, 2026-10-04.
- **Canonical effect:** REQ-PRIV-029 records actor-aware responsibility identity disclosure; REQ-EVT-011 references it. CTX-1.31 succeeds CTX-1.30 after concise self-verification.
- **Authorization:** Create and execute WI-095-002 — Event Needs under existing canonical transitions/eligibility and the approved privacy rule. No Event Updates or sibling capability. Owner performs all Git commits.
- **Activation:** CTX-1.31 ACTIVE after VER-CTX-1.31 PASS / SELF-VERIFIED. WI-095-002 IN_PROGRESS, execution YES. Leave VERIFYING / OWNER REVIEW after implementation; no owner completion inferred.

## DEC-085 — Shared Family Dates execution and retention mechanics

- **Status/date:** ACCEPTED — explicit owner instruction, 2026-10-04.
- **Authorization:** Create and execute WI-095-004 under CTX-1.31 and REQ-FAM-010/011/012/013, REQ-PRIV-016/017/018. Immediate unavailability and 30-calendar-day duration are unchanged. No new baseline is needed for implementation mechanics.
- **Mechanics:** No user-facing restore or retained-data API. Reuse feature-specific hosted maintenance for permanent cleanup at expiry; retained storage never grants current access. Operational/backup restoration must recheck current Family and original membership/association authorization. Audit/backup retention policies remain separate predeployment work.
- **Scope/status:** Shared Family Dates only; owner commits; hand back VERIFYING / OWNER REVIEW. No sibling capability authorization.

## DEC-086 — Standalone Check-Ins and explicit Family-anchored grants

- **Date:** 2026-10-04; **Authority:** Project owner, original standalone scope and follow-up eligibility decision.
- **Decision:** Approve creation/read-only standalone Check-Ins, five existing statuses and optional plain-text note; no location or integrations, recipient editing, automatic expiry, delete/restore or safety inference.
- **Privacy:** Explicit active adult recipients in one Family where the author is active. Grants bind exact profiles and original memberships; either party's loss permanently invalidates sharing, with no rejoin/inheritance. Own history survives departure. REQ-PRIV-030 is normative.
- **Canonical effect:** Add REQ-PRIV-030 and REQ-SHARE-010; CTX-1.32 succeeds CTX-1.31 after VER-CTX-1.32 PASS / SELF-VERIFIED.
- **Authorization:** Create and execute WI-095-005 only. Phase 9.5 remains current; leave VERIFYING / OWNER REVIEW, no next item. Owner alone stages/commits/pushes; no PR.

## DEC-087 — Temporary Sharing Sessions, bounded MVP privacy and execution

- **Date/authority:** 2026-10-04, explicit project-owner Temporary Sharing Sessions prompt.
- **Decision:** Temporary self-initiated location sharing only, default one hour/maximum 24 hours, chosen end and early stop, fixed explicit same-Family active adult recipients, exact membership grants with permanent invalidation, author loss ends session. OD-002 resolved for this bounded MVP; indefinite sharing excluded.
- **Location:** Latest accepted point only, browser permission/active-UI limits, bounded minute cadence and honest freshness/failure states. Expiration denies access synchronously; feature cleanup removes raw coordinates. No history, restore, map provider rollout or sibling integrations.
- **Canonical effect:** REQ-PRIV-031 and REQ-SHARE-011; CTX-1.33 succeeds CTX-1.32 after separate context self-verification. Existing broader concepts remain gated and Check-In rules unchanged.
- **Authorization:** Create and execute WI-095-006 in this task, including minimum context amendments/baseline. Phase 9.5 stays current; final VERIFYING / OWNER REVIEW, no next item or owner completion. Owner alone performs all Git commits; no staging/push/PR.

## DEC-088 — Private manual Trusted Contacts first slice

- **Date/authority:** 2026-10-05, explicit owner decision resolving the Trusted Contact blocker and continuing implementation.
- **Decision:** Approve personal manual contact records with no contact Account/Profile/Family links or discovery. REQ-FAM-016 defines name, phone/email, optional descriptive label, owner-scoped duplicate create/edit rules, optimistic concurrency, Family independence and permanent deletion with no feature history/restore.
- **Privacy:** REQ-PRIV-032 defines owner-only server authorization, no administrator/selected-Family/known-ID override, nondisclosure/no-store and zero downstream access, notifications, membership or Safety Circle enrollment. Future integrations/linking remain separately gated.
- **Canonical effect:** Update REQ-FAM-014/015, REQ-PRIV-019/024 and glossary/index; add REQ-FAM-016 / REQ-PRIV-032. Earlier DEC-049/059 open questions are resolved only for the approved manual first slice. CTX-1.34 succeeds CTX-1.33 after separate VER-CTX-1.34 PASS / SELF-VERIFIED under this owner authorization.
- **Authorization:** Create and execute WI-095-007 in this task, including this minimum context successor. Phase 9.5 remains current; final VERIFYING / OWNER REVIEW, no next item or completion on owner's behalf. Owner performs Git commits; no staging/push/PR.

## DEC-089 - Safety Circle presets and owner-loss deletion

- **Status:** ACCEPTED - original owner instruction and follow-up owner-loss decision explicitly authorize WI-095-008 and minimum successor CTX-1.35, 2026-10-05.
- **Decision:** REQ-FAM-017 / REQ-PRIV-033 record private owner-only, one-Family presets with same-Family eligible adult profiles and zero protected authority. Ordinary loss permanently removes only the member row; owner loss permanently deletes circle/member rows, without retention/restore/transfer or automatic rejoin recreation. Optional Check-In/Sharing presets materialize confirmed explicit grants independently of later circle changes/deletion.
- **Scope:** Bounded CRUD, creation presets, focused verification and one final regression; no other item authorized. Owner performs Git commits. WI-095-008 ends VERIFYING / OWNER REVIEW.
- **Canonical effect:** CTX-1.35 succeeds CTX-1.34 after separate context verification; other permissions/lifecycles unchanged.

## DEC-090 - Home Pending RSVP attention first trigger

- **Status/date/authority:** ACCEPTED - explicit project-owner WI-095-009 continuation instruction, 2026-10-05.
- **Decision:** Pending Event RSVP is the first Home attention trigger. Reuse live currently eligible exact-profile own-Pending invitations, with selected Family scoping only after server authorization; include past-dated Events. Home discovers and links to existing Event detail for RSVP. Resolved/revoked/ineligible/invisible entries disappear on reread.
- **Boundary:** No notification storage/inbox/history, read/unread/dismiss/snooze, outbound push/email/SMS, preferences, generic infrastructure or other workflow attention. This does not complete Phase 9.5 notifications.
- **Later scope reconciliation:** DEC-092 supersedes only that Phase 9.5 notification-completion boundary: WI-095-009 satisfies the approved minimum attention slice; broader notifications are formally deferred future scope. Original implementation permissions remain unchanged.
- **Context effect:** Implements contemplated REQ-EVT-013 / REQ-UX-008 using unchanged REQ-EVT-010/014 and REQ-PRIV-028. Clarify current Home scope separately from WI-095-001's historical exclusion. CTX-1.35 remains ACTIVE: no new permission, lifecycle or baseline is required.
- **Authorization:** Create and execute WI-095-009 only; hand back VERIFYING / OWNER REVIEW. WI-095-010 unauthorized. Owner performs all Git commits; no staging/commit/push/PR.

## DEC-091 — Household organization and adult-only MVP

- **Status/date/authority:** ACCEPTED — explicit original WI-095-010 owner instruction and adult-only blocker-resolution decision, 2026-10-05.
- **Decision:** Household grouping follows REQ-FAM-018 / REQ-PRIV-034. Only current Family admins manage; all current active members view same-Family organization. Zero-or-one assignment per active adult member; unique normalized Family names; empty Households allowed; immediate assignment removal on membership loss; no automatic rejoin/replacement inheritance. Permanent deletion unassigns only. Households belong to the Family and survive creator/admin authority loss.
- **Boundary:** Organization/presentation only; zero feature authorization, audience, sharing, private contact visibility or admin privacy override. No address/residence/legal/custody/financial meaning. Current MVP supports existing active adults only; child grouping needs separately approved profile/membership/privacy/management rules under REQ-PROD-013. No minor-domain changes, generic groups, retention framework or feature integrations.
- **Context effect:** Activate minimum successor CTX-1.36 after VER-CTX-1.36 PASS; supersedes CTX-1.35 as active aggregate baseline without changing unrelated contracts.
- **Authorization:** Create/execute WI-095-010 only; hand back VERIFYING / OWNER REVIEW. WI-095-011 unauthorized; owner commits, no staging/commit/push/PR.

## DEC-092 — Phase 9.5 closure scope and formal deferrals

- **Status/date/authority:** ACCEPTED — explicit project-owner governance, deferral and conditional phase-closure instruction, 2026-10-05.
- **Decision:** WI-095-001 through WI-095-010 constitute the complete, owner-approved Phase 9.5 implementation set. WI-095-005 satisfies the standalone Check-In minimum; WI-095-006 satisfies the bounded temporary location-sharing minimum; WI-095-009 satisfies the first notification/attention slice through live eligible own-Pending RSVP Home attention. No WI-095-011 is required.
- **Deferrals:** Broader/contextual Sharing Sessions, broader notification functionality and additional Check-In integrations/reminders are formally deferred future scope, not implemented capabilities or Phase 9.5 closure gates. The exact canonical boundary is REQ-PROD-014; existing Sharing/Check-In anchors and REQ-NOTIFY-001–003 preserve future product intent/design principles. General Plan editing/lifecycle/completion and Trip completion remain outside Phase 9.5 closure, with no new phase assignment.
- **Supersession:** Reconcile DEC-073's remaining Phase 9.5 placement/gates and DEC-090's notification-completion boundary only. Do not change approved implementation behavior, privacy, audiences, authorization, duration, retention or lifecycle rules, or historical work-item/verification evidence.
- **Context and closure:** CTX-1.37 is APPROVED / ACTIVE, succeeding CTX-1.36 as the active aggregate baseline for REQ-PROD-014 and related scope references. Separate context checks passed; Phase 9.5 is COMPLETE / CLOSED: all ten implementation items are owner-approved, all remaining identified closure gates are formally deferred, and no active Phase 9.5 item remains. Closure does not certify deployment readiness.
- **Separate context verification:** PASS / SELF-VERIFIED, 2026-10-05. Reviewed owner instruction against canonical deferrals, unchanged implementation/privacy rules and completed work-item evidence. Markdown/local references and anchors (184), requirement/decision references (773), all ten canonical titles and owner-approved completion records, current baseline/phase/authorization consistency, future Phase 10/11 status and Git diff whitespace checks passed. Prior work-item/evidence records remain unchanged. No application tests/builds or new verification package.
- **Authorization:** Canonical reconciliation, successor baseline and phase closure only. No next phase or work item is authorized. No product code, application tests/builds, new verification package or Git staging/commit/push/PR; owner performs all Git commits.

## DEC-093 — Phase 10 facilitated Development testing and first readiness item

- **Status/date/authority:** ACCEPTED — explicit project-owner Phase 10 creation and execution instruction, 2026-10-05.
- **Decision:** Authorize Phase 10 — Testing & Verification / User-Testability Readiness and create/execute WI-010-001 — Post-9.5 User-Testability Baseline. Use facilitated local Development, isolated synthetic adult scenarios and later small adult usability sessions. Production hosting/authentication/delivery/provisioning are not prerequisites.
- **Numbering:** WI-010-001 follows the existing zero-padded whole-phase convention (WI-008 / WI-009); WI-075 / WI-095 are intervening decimal-phase identifiers. No WI-100 item is created.
- **Boundary:** Evaluate the implemented journeys and make bounded fixes for demonstrated blockers or misleading discovery/presentation. No new behavior, grants, broad redesign, notification triggers or deferred feature expansion. DEC-092 remains fully in force; production/pilot dependencies are tracked separately. No human usability evidence is inferred from automation.
- **Context compatibility:** CTX-1.37 remains APPROVED / ACTIVE. Phase/testing authorization changes no normative requirement, privacy rule, workflow or architecture; no successor CTX is needed. The roadmap owns phase exits; planning/phase-10-readiness.md owns the single concise matrix and future session preparation. Requirements trace uses existing anchors.
- **Authorization/status:** WI-010-001 COMPLETE / PASS — OWNER APPROVED; VER-WI-010-001 COMPLETE / PASS under explicit owner closure approval, 2026-10-05. Phase 10 remains CURRENT / IN PROGRESS. The later explicit owner continuation request, 2026-10-05, authorizes [WI-010-002 — Connected Everyday Coordination Verification](planning/stories/WI-010-002-connected-everyday-coordination-verification.md), VERIFYING / OWNER REVIEW, within the existing verification/bounded-fix boundary. No WI-010-003 authorized. Owner performs Git commits; no staging/commit/push/PR.
- **Supersedes:** DEC-092's no-next-phase/no-next-item authorization checkpoint only; Phase 9.5 closure, implemented minimum and all deferrals remain unchanged.
- **Separate context verification:** PASS / SELF-VERIFIED, 2026-10-05; consistency/reference/requirements-trace and unchanged-deferral checks recorded in VER-WI-010-001. Normative contracts/workflows and CTX-1.37 remain unchanged; owner accepted the recorded validation without rerunning application suites.
