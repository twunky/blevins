# WI-095-004 — Shared Family Dates

- **Status:** VERIFYING / OWNER REVIEW
- **Phase / baseline:** Phase 9.5; CTX-1.31 ACTIVE (unchanged).
- **Requirements:** REQ-FAM-010/011/012/013; REQ-PRIV-016/017/018.
- **Model tier:** DEEP (association privacy and retention).

## Required context

Entry: README, project-status, CURRENT-TASK, WORKING-RULES, CONTEXT-ROUTING. Before execution/verification: product contract Shared Family Dates anchors; privacy contract Shared Family Date anchors; DEC-038/040/042/045/046/085; repository execute-story/maintain-context/verify-story skills; work-item template; current Family code and client structure rules; MOCKUP-ALIGNMENT and VALIDATION for existing presentation/checks. Roadmap supplies phase/numbering. Local development supplies guarded SQL/E2E harness conventions.

Excluded: historical verification, HISTORY, old proposals/screenshots, unrelated Phase 9.5 capabilities. Exception: only the build-warning line in VER-WI-095-003 is consulted to compare the final regression's budget warnings. No broad proposal or audit.

## Goal and bounded scope

Implement specifically associated Family date entries: required title/description/calendar date with year, list, create, content edit, creator-only association changes/deletion, server visibility, immediate whole-entry unavailability on creator/associated membership loss, and 30-calendar-day cleanup. Profile birthdays keep existing explicit field sharing and are never copied into date records.

DEC-085 records the owner's implementation decision: no user restore; narrow feature maintenance reuses current hosted-service conventions; no new audit/backup policy or generic retention framework. CTX-1.31 product/privacy behavior remains unchanged.

## Acceptance checks

1. AC-01: Active adult members create; only creator/associated active members read/edit content. Creator alone changes associations/deletes; administrator/operator/selected Family/known ID grant no override. Unauthorized responses disclose no date/associated identities.
2. AC-02: Required title/description/date survive SQL/API/UI as one date with year and no time-zone shift or recurrence. Associations are explicitly selected active adults in one Family.
3. AC-03: Any associated membership loss or creator loss immediately hides the whole entry; ended associations are removed, rejoining/operational row restoration cannot reauthorize old membership grants.
4. AC-04: Delete is confirmed inline; unavailable storage is excluded from normal API; first unavailability starts 30 days, then feature maintenance permanently removes expired row/associations. No restore.
5. AC-05: Version conflicts protect edits/deletion. Family UI covers loading/empty/list/create/edit/confirmation/Retry/server actions and context reset; integrated authorized-viewer lifecycle and denied identity verified at narrow width.
6. AC-06: Focused checks and one final .NET SQL-gated regression, full Angular, production build, relevant Playwright journey, diff check; results labeled SELF-VERIFIED.

## Authorization / stop conditions

Approved work item YES; execution YES, explicit owner prompt on 2026-10-04 (not DEC-032 auto-approval). Owner commits. Stop only for material canonical conflict/missing product or privacy decision, unrelated uncommitted work, or major cleanup architecture requirement.

Non-goals: recurring dates, reminders, notifications, calendar sync/framework, copied profile birthdays, Events changes, Check-Ins, Sharing Sessions, location, Trusted Contacts, Safety Circles, Household, Plan/Trip lifecycle, broad Family extraction, trash/restore/admin restore, generic retention/audit framework.

## Evidence / handback

Implementation and proportional SELF-VERIFIED validation are complete; [VER-WI-095-004](../verification/VER-WI-095-004.md) records the acceptance mapping and actual results. Leave VERIFYING / OWNER REVIEW; completion is NOT authorized and no next capability is authorized.
