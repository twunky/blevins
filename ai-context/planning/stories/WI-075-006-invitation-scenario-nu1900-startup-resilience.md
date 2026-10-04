---
Status: DONE
Authority: Project-owner execution approval in the 2026-09-28 session
Context-Baseline: CTX-1.24
Version: 1.1
---

# WI-075-006 — Invitation Scenario NU1900 Startup Resilience

## Metadata

- **Work Item ID:** `WI-075-006`
- **Status:** `DONE`
- **Phase:** `7.5 — End-to-End UX & User-Test Readiness`
- **Context baseline:** `CTX-1.24`
- **Execution authorization:** `YES` — project-owner approval, 2026-09-28
- **Work type:** Development tooling fix; no application behavior change.
- **Traceability:** Unblocks the `WI-075-005` Stage 1 technical preflight blocked in [`VER-WI-075-005.md`](../verification/VER-WI-075-005.md).

## Goal

Allow the local Development invitation scenario startup script to continue when NuGet cannot retrieve vulnerability metadata and reports `NU1900`, while keeping the warning visible and preserving vulnerability auditing and the existing policy for actual vulnerability findings.

## Confirmed diagnosis

1. The development machine currently cannot complete TLS negotiation with `https://api.nuget.org/v3/index.json` for NuGet vulnerability metadata. DNS resolution and TCP connectivity succeeded; the TLS operation failed with `No credentials are available in the security package`.
2. `Directory.Build.props` sets `TreatWarningsAsErrors=true`.
3. As a result, NU1900 is promoted from a warning to a restore/build/startup failure.
4. Diagnostic restore and API build succeeded when `NU1900` alone was added to `WarningsNotAsErrors`.
5. NuGet auditing remained enabled during that diagnostic.
6. NU1901, NU1902, NU1903, and NU1904 were not suppressed by that setting.
7. This is a build/environment issue, not application behavior.

## Bounded change

Change only `scripts/start-invitation-scenario.ps1`. Scope `WarningsNotAsErrors=NU1900` to the script's `dotnet run` invocation using the process environment, the narrow mechanism verified by the diagnosis. Keep NU1900 in output as a warning. Do not change any repository-wide warning/audit policy, project files, application code, CI configuration, or canonical product context.

## Non-goals

- Repairing the machine's TLS/security-provider or network configuration.
- Disabling NuGet auditing or suppressing NU1901, NU1902, NU1903, or NU1904.
- Changing ordinary build behavior or CI behavior.
- Retrying the full `WI-075-005` preflight as part of this work item.
- Introducing a product requirement or changing application behavior.

## Acceptance criteria

1. **`AC-01 — Script-scoped setting`**: Only the Development invitation scenario startup script supplies `WarningsNotAsErrors=NU1900` to its `dotnet run`/build invocation; the setting does not alter shared repository configuration.
2. **`AC-02 — Audit remains enabled and visible`**: NuGet auditing remains enabled, and when vulnerability metadata is unavailable NU1900 is emitted as a warning in startup output rather than suppressed or promoted to an error.
3. **`AC-03 — Findings retain policy`**: The change does not add NU1901, NU1902, NU1903, or NU1904 to `WarningsNotAsErrors`, `NoWarn`, or another suppression mechanism; actual vulnerability findings remain subject to the existing policy.
4. **`AC-04 — Ordinary builds unchanged`**: A normal repository build outside this script retains the existing warnings-as-errors behavior.
5. **`AC-05 — CI unchanged`**: No CI configuration is changed, and CI behavior remains unchanged.
6. **`AC-06 — Scenario startup verified`**: With the TLS/feed problem present, the script shows NU1900 as a warning, the API starts, the invitation scenario database initializes, and the health endpoint succeeds.
7. **`AC-07 — Limitation recorded`**: The verification record states that the workaround does not repair TLS access to `api.nuget.org` and is intentionally limited to local invitation-scenario startup.

## Verification plan

- Run the invitation scenario startup script while the current TLS/feed failure is present; confirm NU1900 is visible as a warning, not an error.
- Confirm the API starts, the dedicated invitation scenario database initializes, and its health endpoint succeeds.
- Confirm a normal repository build outside this script retains strict warning behavior.
- Review the diff to confirm only `scripts/start-invitation-scenario.ps1` changed and no global warning/audit configuration or CI files changed.
- Do not retry the complete WI-075-005 preflight as part of this story; this work item removes only its Stage 1 startup blocker.

## Known limitation

This workaround does not repair TLS access to `api.nuget.org`, does not establish that the vulnerability feed is reachable, and must not be interpreted as resolving the underlying machine/network issue. It is intentionally scoped to local invitation-scenario startup; NU1900 remains visible and real vulnerability findings retain the existing policy.

## Completion

Implemented and self-verified on 2026-09-28. All seven acceptance criteria pass in [`VER-WI-075-006.md`](../verification/VER-WI-075-006.md). `WI-075-005` remains blocked until its Stage 1 preflight is explicitly retried.
