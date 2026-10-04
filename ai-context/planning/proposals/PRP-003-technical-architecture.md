---
Proposal-ID: PRP-003
Status: ACCEPTED
Date: 2026-09-24
Requested-By: User
Context-Baseline: CTX-1.3
Model-Tier: DEEP
Execution-Authorized: NO
---

# Phase 3 — Technical Architecture Proposal

## Request

Define a lean, privacy-first technical architecture direction for the approved MVP. This proposal is a decision artifact only. It creates no application, infrastructure, account, vendor selection, or implementation work.

## Required context

- `../../README.md`
- `../../project-status.md`
- `../../core/product-contract.md`
- `../../core/ux-contract.md`
- `../../core/privacy-safety-contract.md`
- `../../core/domain-glossary.md`
- `../../workflows/events.md`
- `../../workflows/plans-trips.md`
- `../../workflows/sharing-checkins.md`
- `../../workflows/safety.md`
- `../../decision-log.md#open-product-decisions`
- `../../decision-log.md#resolved-product-decisions`

## Current evidence and constraints

- The product is a private family coordination application with safety capabilities, not a tracking product (`REQ-PROD-001` through `REQ-PROD-006`).
- Family relationships, Family Administrator responsibility, and workflow participation never imply access to private information (`REQ-PRIV-004` through `REQ-PRIV-008`).
- Sharing is voluntary, explicit, contextual, time-bound where appropriate, revocable, and useful only as an independent product capability (`REQ-PRIV-001` through `REQ-PRIV-003`; `REQ-SHARE-001` through `REQ-SHARE-005`).
- Safety facts need confidence, provenance, freshness, corrections, and preserved unknowns (`REQ-PROD-004`, `REQ-SAFE-001` through `REQ-SAFE-004`, `REQ-SAFE-010` through `REQ-SAFE-014`).
- The initial planning target is 50 members per Family Group; the default configured limit is 100; the validated supported ceiling is 250 (`REQ-FAM-004`).
- The product must provide a modern responsive web/mobile experience, but no native-mobile delivery requirement is approved (`REQ-UX-012`).
- The project owner is experienced with C#, .NET Core, Angular, and SQL Server, and prioritizes a prompt MVP while minimizing long-run total cost.
- The project owner prefers a mostly free local development path now, with an architecture that can expand later when needs and budget are clearer.
- Exact framework versions, provider choices, licensing choices, and paid services are intentionally unknown at this proposal stage.
- Controlled first Family Group bootstrap through application-operator provisioning is approved by `DEC-018`, `REQ-FAM-006`, and `REQ-PRIV-012`.

## Proposed architecture direction

### Accepted direction: local-first modular .NET monolith, Angular client, SQL Server candidate

Begin the MVP as one deployable modular monolith with a C#/.NET server application, an Angular responsive browser client, and one relational transactional data boundary. Use SQL Server as the preferred development and MVP database candidate. Deliver the approved web/mobile experience through the responsive Angular client first.

This reuses the project's existing strengths, avoids the cost of a new language or client framework, and keeps the first release simple enough to build and verify locally. It also protects the boundaries that are expensive and unsafe to retrofit: consent, access decisions, time limits, auditability, information confidence, and safety history.

The proposal does **not** select a .NET or Angular version, hosting provider, identity provider, deployment method, notification provider, mapping provider, production SQL Server edition, or any public third-party integration. The private internal application API direction is accepted by `DEC-017`; this local-first expandable architecture direction is accepted by `DEC-019`.

### Cost posture

- Prefer one application, one relational database, and the smallest number of managed dependencies needed for the approved MVP. Do not start with independently deployed services.
- Optimize for total cost of ownership: the project's established .NET, Angular, and SQL Server knowledge reduces delivery time, maintenance risk, and outside-support cost.
- Prefer free local development tooling where legally permitted and practically sufficient.
- Use SQL Server's development tooling only within its permitted development use. A local development choice is not a production licensing decision.
- Before production deployment, compare the actual recurring cost of the viable SQL Server licensing/hosting option with a supported relational alternative. Keep SQL Server when its verified recurring cost preserves the familiarity advantage; otherwise bring an explicit database-alternative proposal for approval. Do not silently switch data platforms.
- The 50-member planning target, 100-member default limit, and 250-member validated ceiling do not justify microservices or an enterprise-scale database deployment at MVP.

### Version and support posture

- Use the latest stable long-term-support version available at the time each implementation foundation decision is made.
- Verify supported versions from official vendor documentation before pinning any version.
- Prefer LTS versions over short-term, preview, release-candidate, or experimental versions.
- Do not hard-code exact framework versions in this proposal because version support changes over time.
- Record chosen versions later in a bounded implementation foundation story, with the date and source used to verify support.

### Expansion posture

The architecture should be simple now and expandable later by preserving clear boundaries, not by starting with distributed complexity. Expansion means the MVP should keep these concerns separable inside the modular monolith:

- UI client vs. application API.
- Authentication boundary vs. application authorization policy.
- Family/membership rules vs. private-data access rules.
- Sharing/check-in behavior vs. safety incident behavior.
- Notification decision logic vs. notification delivery provider.
- Mapping/location provider boundary vs. approved location-sharing policy.
- Transactional data boundary vs. later reporting, analytics, archive, or integration needs.

Future expansion may include provider swaps, stronger deployment topology, background processing, real-time updates, mobile clients, reporting, or extracting a module into a separate service. Those are later decisions and must be justified by evidence rather than assumed at MVP.

## Local development foundation and provider deferment direction

### Accepted direction: local first, providers deferred, boundaries preserved

The MVP foundation should be able to run locally with free or no-additional-cost tooling where practical. Local development must cover the server, responsive client, SQL Server database candidate, and basic verification workflow without requiring paid cloud services. This direction is accepted by `DEC-020`.

Provider selection is deferred. The architecture should not choose hosting, identity, email/SMS, notification, mapping/location, file/media storage, background job, real-time, deployment, production database, or monitoring providers until those choices have a concrete requirement, cost/security review, and implementation story.

### Provider boundary rules

| Provider area | Local-first posture | Later decision trigger |
|---|---|---|
| Identity/authentication | Use a replaceable authentication boundary for local development and testing. | Select when account, recovery, invitation acceptance, and production security requirements are ready. |
| Notifications | Keep notification decision logic inside the application; defer delivery provider selection. | Select when the first notification story needs actual delivery. |
| Mapping/location | Keep location-sharing policy separate from any map/geocoding provider. | Select only when an approved UI/workflow needs provider-backed map or location features. |
| File/media storage | Do not introduce unless an approved feature requires it. | Select only after a specific file/media requirement exists. |
| Background work | Treat reminders, expiry, and scheduled work as an internal capability boundary first. | Select a scheduler/queue only when local or simple in-process behavior is insufficient. |
| Real-time delivery | Keep real-time as optional and supplementary to the private API. | Select transport only after a timely-update story proves the need. |
| Hosting/deployment | Keep local development independent of paid hosting. | Select before production/pilot readiness, using cost, security, operability, and audit criteria. |
| Production database | Use SQL Server as the local/MVP candidate without final production commitment. | Select after the production database cost gate and backup/retention needs are known. |

### Provider selection criteria

When a provider decision becomes necessary, evaluate it against:

- total recurring cost at expected MVP scale;
- free or low-cost local development support;
- current support status and security update posture;
- ability to enforce server-side authorization, audit, privacy, and retention requirements;
- data export/portability and provider replacement effort;
- operational simplicity for the project owner;
- compatibility with the approved C#/.NET and Angular direction;
- ability to defer advanced features until actually needed.

### Decision status

| Decision | Accepted direction | Remaining boundary |
|---|---|---|
| Local development foundation | Accepted by `DEC-020`: run locally with mostly free/no-additional-cost tooling where practical. | Exact setup and versions remain later implementation-foundation decisions. |
| Provider deferment | Accepted by `DEC-020`: defer paid/production providers until specific requirements and cost/security review exist. | No provider is selected by this proposal. |
| Replaceable provider boundaries | Accepted by `DEC-020`: isolate provider-specific behavior behind replaceable boundaries. | No adapters, code, interfaces, or infrastructure are implemented here. |

## Primary implementation stack direction

### Accepted direction: familiar, local-first stack as the MVP default

C#/.NET is accepted for the server application, Angular is accepted for the responsive browser client, and SQL Server is accepted as the development and MVP database candidate. The implementation foundation should use the latest stable LTS or stable supported versions available when that work begins, verified from official sources. This is a primary architecture direction, not implementation authorization and not final production hosting, licensing, provider, or exact-version approval.

This recommendation is based on the project owner's stated familiarity with C#/.NET Core, Angular, SQL Server, and related technologies, plus the desire to get the MVP running promptly while keeping long-run cost low. The cheapest long-run path is not always the cheapest hosting line item; for this project, using a familiar stack reduces delivery risk, outside-support cost, and maintenance friction.

### Stack decision boundaries

| Area | Proposed direction | Not decided yet |
|---|---|---|
| Server | C#/.NET modular monolith using latest stable LTS at selection time | Exact .NET version, hosting model, deployment pattern, identity provider |
| Client | Angular responsive browser client using latest stable supported version at selection time | Exact Angular version, component library, native mobile approach |
| Data | SQL Server as development and MVP database candidate | Production SQL Server edition, hosting cost, backup/retention plan, alternative database decision |
| Integration boundary | Private HTTP/JSON application API accepted by `DEC-017` | Endpoint conventions, API versioning convention, real-time transport |
| Services | No microservices for MVP accepted by `DEC-015` | Later service extraction criteria after evidence |

### Local development posture

The MVP foundation should be able to run locally using free or no-additional-cost tooling where practical. Local development should support the server, client, database candidate, and basic verification workflow without requiring paid cloud services. Any local-only substitution must be explicit and must not hide production requirements, privacy rules, or security obligations.

### Production cost gate

Before production deployment, the architecture must verify the actual recurring cost of the viable SQL Server path against at least one supported relational alternative. If SQL Server's production cost materially undermines the long-run cost goal, the project must return to Proposal Mode with an explicit database alternative. Until then, SQL Server remains the familiar candidate, not an irreversible production commitment.

### Stack decision status

| Decision | Accepted direction | Remaining boundary |
|---|---|---|
| Primary implementation stack | C#/.NET, Angular, and SQL Server candidate are accepted by `DEC-019` as the Phase 3 MVP architecture direction. | Exact versions, providers, deployment, and implementation remain unselected. |
| Version support posture | Latest stable LTS or stable supported versions will be selected at implementation-decision time and verified from official sources. | No exact version is pinned by this proposal. |
| Local development posture | MVP foundation should prefer a mostly free local development setup. | Local-only substitutions must not hide production requirements, privacy rules, or security obligations. |
| Production database cost gate | Production SQL Server cost must be verified before deployment against at least one supported relational alternative. | No production database edition, license, host, or alternative is selected yet. |

### Logical modules

The following are ownership boundaries, not services, tables, APIs, or implementation schemas:

| Module | Owns | Must not imply |
|---|---|---|
| Family and profiles | Family Groups, membership, relationships, household grouping, contact and privacy preferences | Access to a member's private data (`REQ-PRIV-005`, `REQ-PRIV-006`) |
| Events and coordination | Events, invitations, RSVPs, Needs, claims, and meaningful updates | A general chat or social feed |
| Plans and trips | Lightweight Plans, structured Trips, expected arrivals, trip coordination | Required location sharing |
| Sharing and check-ins | Sharing Session consent, scope, audience, start/end, revocation, check-ins, and freshness | Covert, default-wide, or historical tracking |
| Safety coordination | Concerns, Incidents, findings, contact attempts, tasks, timeline, and resolution | Emergency classification or access bypass |
| Notifications | Purposeful, audience-aware notification decisions and delivery requests | A right to view the underlying private information |
| Access and audit | Authentication integration boundary, policy evaluation, consent history, and access/activity audit | A product role that automatically sees all data |

### Required architectural invariants

1. **Policy enforcement is server-side and contextual.** Every private-data request must be evaluated against the requesting member, the requested data, the relevant Family Group, the authorized audience, the purpose/context, and the time window. A UI control or membership check alone is never sufficient.
2. **Consent is enforceable state.** Starting, stopping, revoking, and expiring a Sharing Session changes effective access immediately. Every read of location-related data must re-evaluate current authorization and expiry.
3. **Privacy is separate from relationship and participation.** The system models relationship, administrative responsibility, event/trip/incident participation, and permission as distinct concepts. No one of them is a shortcut for another.
4. **Safety information retains provenance.** Findings, contact attempts, confidence classifications, timestamps/freshness, corrections, state transitions, and resolution history must be preserved as recorded evidence. Unknown is representable information, not a missing value to infer.
5. **Time-sensitive data has visible freshness.** Check-ins, expected arrivals, location-related information, and incident facts carry enough recorded time context to support `Confirmed`, `Reported`, `Assumption`, `Unknown`, and `Outdated` presentation.
6. **Notification eligibility is separate from data disclosure.** The system may determine that a recipient should receive a restrained notification without exposing more than that recipient is authorized to view. Safety notifications follow the approved gradual escalation intent.
7. **Limits are configurable by the application operator.** Family Group capacity enforcement reads the approved operator configuration; Family Administrators do not change that configuration.
8. **Sensitive access is auditable.** Consent changes, Sharing Session lifecycle changes, safety-record changes, and access to sensitive location or safety information require an auditable event trail. Retention duration remains subject to `OD-004`.

### Interaction patterns

- Use request/response interactions for normal family coordination.
- Use an asynchronous work boundary for reminders, expiry, periodic sharing updates, notification delivery, and other time-triggered work. This is a capability boundary only; no queue, scheduler, or provider is selected here.
- Use a real-time update capability only where an approved experience needs timely sharing, check-in, or incident updates. It must apply the same authorization, expiry, and audit rules as normal reads.
- Put location, notification, identity, mapping/geocoding, file, and external-assistance integrations behind replaceable adapters. No particular external service is selected by this proposal.
- Keep provider-specific code behind replaceable boundaries so local/free development choices can later be replaced by managed production services when justified.

## Identity and Family Group access proposal

### Accepted direction: private, invitation-only membership

Use a managed identity boundary for sign-in and account recovery, but keep application authorization in the modular .NET application. A signed-in account is not automatically a Family Group member and receives no Family Group data merely by having an account.

For the MVP, the approved private membership model is:

1. A Family Administrator may issue an invitation to a specific intended person for one Family Group.
2. An invitation is single-use, time-limited, revocable, and does not expose Family Group details before acceptance.
3. The recipient signs in or creates an account, then explicitly accepts the invitation before membership becomes active.
4. Membership removal immediately ends access to that Family Group's data; it does not erase safety or audit history outside the separately required retention decision.
5. No public Family Group directory, family-name search, self-service join request, or automatic family membership exists in MVP.
6. Family membership, Family Administrator responsibility, Safety Circle membership, event/trip/incident participation, and individual privacy permissions remain separate authorization inputs.

This respects the approved private-family purpose and prevents an account, a relationship, or a Family Administrator role from becoming an access shortcut. The proposal does not select an identity provider, sign-in method, recovery method, invitation channel, or invitation lifetime.

### Accepted direction: authentication provider deferred, application authorization owned internally

Authentication and authorization must stay deliberately separate. Authentication establishes the signed-in account identity through a replaceable boundary. Authorization remains owned by the C#/.NET application because Family Group membership, invitations, Safety Circle, Sharing Sessions, privacy, purpose, time windows, incident participation, and audit rules are product-specific. This direction is accepted by `DEC-021`.

Identity-provider selection is deferred until implementation planning has enough detail to evaluate sign-in, recovery, invitation acceptance, local development, cost, and production security. Local development may use a clearly labeled local/developer authentication substitute only if it cannot be mistaken for production behavior and does not bypass application authorization rules.

This recommendation does not choose an identity provider, credential method, recovery method, MFA/passkey policy, invitation channel, invitation lifetime, or minor/guardian behavior.

### Authentication and authorization decision status

| Decision | Accepted direction | Remaining boundary |
|---|---|---|
| Authentication provider | Deferred by `DEC-021`; authentication remains replaceable. | No provider, sign-in method, recovery method, MFA/passkey policy, invitation channel, or invitation lifetime is selected. |
| Application authorization | Accepted by `DEC-021`; the application owns product authorization rules server-side. | No implementation policy engine, schema, endpoint, or code is selected. |
| Local development authentication | A clearly labeled development substitute may be used only if it does not bypass application authorization. | Exact local setup remains a later implementation-foundation decision. |

### Required identity/access boundaries

| Boundary | Proposed rule | Approved constraint it protects |
|---|---|---|
| Authentication vs. authorization | Sign-in proves an account identity; application policy decides what that account may access. | `REQ-PRIV-005` through `REQ-PRIV-008` |
| Account vs. Family Member record | A signed-in account is not assumed to equal every profile or represented Family Member. Any association must be explicit. | No automatic private-data access |
| Membership vs. private data | Active Family Group membership permits only the non-private access approved by policy; it is not a blanket grant. | `REQ-PRIV-005`, `REQ-PRIV-006` |
| Family Administrator vs. privacy | A Family Administrator may manage membership-related operations but receives no automatic Safety Circle, location, or other private-data access. | `REQ-PRIV-006`, `REQ-PRIV-007` |
| Invitation vs. access | A pending, expired, revoked, or already-used invitation grants no Family Group data access. | Private-family posture under `REQ-PROD-001` |
| Minor/guardian behavior | No guardian, dependent, or minor-specific access rule is inferred. | `OD-003` |

### Alternatives considered

#### Option A — Invitation-only membership (recommended)

- **Benefits:** Fits a private Family Group; minimizes accidental disclosure and public discovery; gives a clear audit point for access; aligns with Family Administrator membership responsibility.
- **Tradeoffs:** Requires a bootstrap path for the first Family Administrator and an invitation/recovery experience.

#### Option B — Open account registration with self-service join requests

- **Benefits:** Lower invitation friction.
- **Tradeoffs:** Adds membership-discovery, approval, impersonation, and privacy risks that are not supported by the current product baseline.

#### Option C — Operator-only manual membership activation

- **Benefits:** Strong central control.
- **Tradeoffs:** Creates ongoing operator work and conflicts with the approved Family Administrator responsibility for membership or family structure.

### Remaining decisions

| Decision | Proposal recommendation | Why it remains pending |
|---|---|---|
| Membership model | Invitation-only membership is accepted by `DEC-016`. | Canonical requirements are `REQ-FAM-005` and `REQ-PRIV-011`. |
| First Family Administrator | Controlled application-operator provisioning is accepted by `DEC-018`. | Implementation details remain unselected. |
| Invitation issuer | Permit a Family Administrator to issue and revoke membership invitations. | Membership responsibility is approved, but invitation mechanics are not. |
| Identity method | Identity-provider selection is deferred by `DEC-021`. | No approved identity provider, credential policy, recovery method, MFA/passkey policy, invitation channel, or invitation lifetime exists. |
| Authentication boundary | Accepted by `DEC-021`: keep authentication replaceable and authorization inside the application. | No implementation mechanism is selected. |
| `OD-003` minor/guardian behavior | Keep minor/guardian-specific onboarding and permissions out of MVP implementation until approved. | The product policy is explicitly unresolved. |

## Internal API and application-boundary proposal

### Accepted direction: private HTTP/JSON application API

Use a private HTTP/JSON application API between the Angular client and the C#/.NET application. It is an internal application boundary, not a separate microservice and not a public partner/developer API. It allows the responsive client to evolve independently while keeping policy enforcement, audit, and transactional rules in one application. This direction is accepted by `DEC-017`.

The API should use resource-oriented reads for approved data and explicit commands for consequential actions such as accepting an invitation, changing membership, starting or stopping sharing, recording a Check-In, raising or resolving a Concern, changing an incident state, or recording a safety finding. It must not expose direct database access, generic administrative endpoints, or endpoints that bypass application policy.

### API boundary rules

| Rule | Proposal |
|---|---|
| Caller authentication | Every non-public request establishes the signed-in account before application processing. The identity provider remains unselected. |
| Authorization | The application evaluates Family Group, membership, audience, privacy, Sharing Session, purpose, and time-window rules for every protected read and mutation. The Angular client never makes the final access decision. |
| Data minimization | A response contains only data the caller is authorized to receive; notification eligibility never grants access to the notification's underlying private data. |
| Sensitive actions | Sharing lifecycle, membership changes, safety findings, contact attempts, incident transitions, and resolution are explicit commands that create an auditable application event. |
| Time-sensitive data | The API returns recorded timestamps/freshness and confidence/provenance information needed to honor `Confirmed`, `Reported`, `Assumption`, `Unknown`, and `Outdated`. |
| Failure behavior | A denied or unavailable record must not disclose whether sensitive data exists beyond what the caller is already authorized to know. |
| Real-time updates | Any later real-time channel is supplementary and must apply the same authentication, authorization, expiry, and audit rules. It does not replace the API boundary. |
| External exposure | No public developer API, partner API, or direct third-party database/API access is in MVP unless separately proposed and approved. |

### Alternatives considered

#### Option A — Private HTTP/JSON application API (recommended)

- **Benefits:** Natural fit for Angular and .NET; explicit server-side policy boundary; easy to test and audit; preserves a future integration boundary without adding a separate service.
- **Tradeoffs:** Requires disciplined endpoint/contract design; the application must avoid leaking data through overly broad response shapes.
- **Status:** `ACCEPTED` by `DEC-017`.

#### Option B — Graph/query-first API from the MVP

- **Benefits:** Flexible client data selection.
- **Tradeoffs:** Adds policy, query-depth, response-shape, and data-disclosure complexity before the approved MVP behavior is validated.

#### Option C — Public API from the MVP

- **Benefits:** Earlier external integration potential.
- **Tradeoffs:** Expands authorization, support, versioning, security, and privacy scope without an approved product requirement.

### Remaining API decisions

| Decision | Proposal recommendation | Why it remains pending |
|---|---|---|
| Internal API style | Use Option A: private HTTP/JSON API with resource reads and explicit commands. | It is the leanest fit for the approved Angular/.NET direction. |
| API versioning and endpoint conventions | Define only after an approved application boundary and first story set exist. | No endpoint or public compatibility requirement exists yet. |
| Real-time transport | Choose later only after prioritizing the approved timely-update experiences. | The product requires certain timely updates but not a particular protocol. |
| External/public APIs | Exclude from MVP. | No approved external integration requirement exists. |

## Family Group bootstrap direction

### Accepted direction: controlled operator provisioning for the first administrator

The first Family Group and its first Family Administrator use a controlled bootstrap path because the approved invitation-only model has no administrator who can issue the first invitation. The application operator may provision the initial Family Group and initiate the first Family Administrator invitation. The intended first administrator must still sign in and explicitly accept before membership becomes active. After that, normal Family Administrator invitation rules apply. This direction is accepted by `DEC-018`.

This uses the already-approved application-operator boundary for Family Group capacity without granting the operator routine access to private Family Group content. It also avoids adding public Family Group creation, discovery, or self-service onboarding before those policies are approved.

### Boundaries and unresolved details

- The operator's capacity-configuration authority does not imply routine visibility into private Family Group information.
- The bootstrap process must be auditable, but exact audit-retention behavior remains constrained by `OD-004`.
- No sign-in provider, recovery method, invitation channel, invitation lifetime, operator support process, or Family Group creation UI is selected by this proposal.
- `OD-003` continues to block minor/guardian-specific enrollment or access rules.

### Alternatives considered

#### Option A — Controlled operator provisioning (recommended)

- **Benefits:** Resolves the first-administrator deadlock; supports private invitation-only membership; avoids public enrollment scope.
- **Tradeoffs:** Requires a controlled operator process before ordinary Family Administrator self-service can begin.
- **Status:** `ACCEPTED` by `DEC-018`.

#### Option B — Public self-service Family Group creation

- **Benefits:** Lower initial setup friction.
- **Tradeoffs:** Creates unapproved public onboarding, abuse-prevention, discovery, support, and privacy scope.

### Bootstrap decision status

| Decision | Accepted direction | Remaining boundary |
|---|---|---|
| First Family Group / administrator bootstrap | Option A is accepted by `DEC-018`: controlled application-operator provisioning, followed by the first administrator's explicit acceptance. | No product decision remains pending for the bootstrap path; implementation details remain unselected. |

## Alternatives considered

### Option A — C#/.NET modular monolith, Angular client, SQL Server candidate (recommended)

- **Benefits:** Reuses existing skills; low initial delivery friction; one coherent policy and audit boundary; easier end-to-end verification; suitable for the approved 50/100/250 Family Group capacity direction; leaves a path to extract a module only when evidence warrants it.
- **Tradeoffs:** A single deployment needs clear module discipline; an individual hot path cannot be independently scaled without later change; production SQL Server cost and licensing must be validated before deployment.

### Option B — Same .NET/Angular direction, but split into separate services from the first release

- **Benefits:** Independent deployment and scaling boundaries from day one.
- **Tradeoffs:** More identity, authorization, audit, operational, and cross-service consistency risk before product behavior is proven. It is disproportionate to the approved MVP and family-size target.
- **Status:** `REJECTED FOR MVP` by `DEC-015`.

### Option C — Native-mobile-first delivery

- **Benefits:** Can later provide deeper device integration.
- **Tradeoffs:** Adds multiple clients, release processes, and permission surfaces before the approved responsive web/mobile experience has been validated. Native delivery is not currently an approved requirement.

## Phase 4 MVP Foundation readiness gates

### Accepted direction: Phase 3 closed, Phase 4 requires separate execution authorization

Phase 3 Technical Architecture is closed as an approved proposal by `DEC-022`. This closure does not authorize implementation. Phase 4 MVP Foundation may begin only after a separate execution request and an approved first story with acceptance criteria and verification evidence.

### Required Phase 4 gates

| Gate | Requirement before Phase 4 execution |
|---|---|
| Baseline | Use active product/UX baseline `CTX-1.3` unless a later approved context update supersedes it. |
| Architecture decisions | Preserve `DEC-015`, `DEC-017`, `DEC-018`, `DEC-019`, `DEC-020`, `DEC-021`, and `DEC-022`. |
| Story authorization | Create an approved Phase 4 MVP Foundation story before code, scaffolding, infrastructure, or tool installation. |
| Exact versions | Select exact .NET, Angular, SQL Server/development database tooling, Node/npm, and related versions only after checking current official support documentation at execution time. |
| Local development | Keep the first implementation foundation local-first and mostly free/no-additional-cost where practical. |
| Provider boundaries | Keep identity, notifications, mapping/location, file/media, background work, real-time, hosting, deployment, and production database providers replaceable and deferred unless separately approved. |
| Authorization | Keep product authorization in the C#/.NET application and evaluate it server-side. Do not rely on identity provider, client, database, Family Administrator status, relationship, or context participation as an access shortcut. |
| Privacy and safety | Preserve all approved privacy, consent, sharing, safety, confidence, freshness, and audit invariants. |
| Open decisions | Do not resolve `OD-001`, `OD-002`, `OD-003`, or `OD-004` implicitly. Related implementation must be blocked or scoped around them. |
| Verification | Every Phase 4 story must include acceptance criteria and evidence; self-verification must be labeled honestly. |

### Phase 4 non-authorizations

Phase 3 closure does not authorize:

- application code, scaffolding, commits, deployments, accounts, or infrastructure;
- exact framework or tool versions before current support verification;
- paid providers, production hosting, production database licensing, or deployment;
- microservices, native mobile, public APIs, or external integrations;
- product-scope expansion or resolution of open product decisions;
- schemas, endpoints, domain implementation models, or provider adapters outside an approved story.

## Decisions still required before implementation planning

| Decision | Proposal recommendation | Reason it needs explicit approval |
|---|---|---|
| Architecture style | Accepted by `DEC-015`, `DEC-019`, and `DEC-022`: use a modular monolith; separate services are rejected for MVP. | Later service extraction requires evidence and a separate decision. |
| First delivery surface | Accepted by `DEC-019` and `DEC-022`: responsive browser client first; reassess native clients after MVP evidence. | The UX contract requires responsive behavior, not a native application. |
| Primary implementation stack | Accepted by `DEC-019`: C#/.NET for the server, Angular for the responsive browser client, and SQL Server as the development/MVP database candidate. | Exact versions and implementation choices remain later decisions. |
| Version support posture | Accepted by `DEC-019`: use latest stable LTS or stable supported framework versions at implementation-decision time, verified against official support documentation. | Exact versions change over time and should not be guessed in this proposal. |
| Local-first development posture | Accepted by `DEC-019`: prefer free/no-additional-cost local development tools where practical, while keeping production needs visible. | It supports quick progress without locking the future production architecture to local-only assumptions. |
| Local development foundation and provider deferment | Accepted by `DEC-020`: local development should be mostly free where practical, paid providers are deferred, and provider-specific behavior stays replaceable. | Exact local setup, provider choices, and production services remain later decisions. |
| Production database cost gate | Accepted by `DEC-019`: verify the production SQL Server licensing and hosting path before deployment. If it materially exceeds a supported relational alternative, submit that alternative as a separate proposal. | Familiarity reduces total cost, but production licensing and hosting may change the long-run comparison. |
| Provider choices | Select providers only in a later bounded decision/story using security support, auditability, cost, and team-operability criteria. | No hosting, identity provider, or external provider constraint has been approved. |
| Authentication and membership policy | Accepted by `DEC-021`: defer identity-provider selection and keep product authorization owned by the application. | Identity provider, sign-in, recovery, invitation channel, invitation lifetime, MFA/passkey policy, and minor/guardian behavior remain unselected. |
| Internal application API | Accepted by `DEC-017`: private HTTP/JSON API with resource reads and explicit commands. | Endpoint conventions and real-time transport remain later decisions. |
| First Family Group / administrator bootstrap | Accepted by `DEC-018`. | No implementation details are authorized by this acceptance. |
| `OD-002` indefinite sharing | Keep it out of MVP unless separately approved. | Temporary/contextual sharing is the approved default. |
| `OD-003` minors/guardians | Block related implementation until policy is approved. | A family relationship cannot be used to infer guardian access. |
| `OD-004` retention | Define retention, deletion, legal/audit, and incident-history behavior before production use. | Sensitive data may not remain indefinitely merely because it was once useful. |
| `OD-001` mobile navigation | Resolve during UX validation, independently of the architecture. | It is an approved UX open decision. |
| Phase 4 MVP Foundation readiness gates | Accepted by `DEC-022`: Phase 4 requires a separate approved story and execution authorization. | Implementation has not started. |

## Non-goals

- Selecting exact versions, SQL Server production licensing, hosting, authentication provider, cloud, deployment system, notification provider, mapping provider, public API, or vendor.
- Creating schemas, endpoints, infrastructure, code, accounts, or implementation tasks.
- Adding persistent tracking, historical movement playback, automatic danger scoring, automatic emergency dispatch, or any excluded capability.
- Resolving the four open product decisions.

## Verification plan for a later approved architecture decision

An independent Phase 3 reviewer should verify that any subsequent architecture decision:

1. maps each architecture constraint to its canonical requirement(s);
2. demonstrates policy denial for family relationship, Family Administrator status, context participation, expired sharing, revoked sharing, and incident creation alone;
3. demonstrates provenance, confidence, correction, freshness, and unknown handling for safety records;
4. demonstrates that notification routing does not disclose unauthorized underlying data;
5. documents how the operator-configured 50/100/250 capacity requirement will be validated;
6. identifies the unresolved `OD-001` through `OD-004` as decision gates rather than implicit behavior; and
7. confirms that the design contains no prohibited surveillance or emergency-response behavior.

## Decision and authorization

- **Decision:** `ACCEPTED`
- **Accepted:** `DEC-015` no microservices for MVP; `DEC-017` private HTTP/JSON application API; `DEC-018` controlled first Family Group bootstrap; `DEC-019` local-first expandable C#/.NET, Angular, SQL Server-candidate foundation with LTS/stable support posture and production database cost gate; `DEC-020` local development foundation and provider deferment; `DEC-021` authentication provider deferment and internal application authorization ownership; `DEC-022` Phase 3 closure and Phase 4 readiness gates.
- **Decision requested:** None for Phase 3. Next work is Phase 4 MVP Foundation planning/execution only after separate authorization and an approved story.
- **Execution authorized:** `NO`
- **If continuing:** Create the first Phase 4 MVP Foundation story; do not begin implementation without separate execution authorization.
