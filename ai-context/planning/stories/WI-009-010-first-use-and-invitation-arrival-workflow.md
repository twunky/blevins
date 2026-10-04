---
Status: DONE / PASS — OWNER APPROVED
Authority: Project-owner authorization (2026-10-03) after the first-use / onboarding review; owner decisions O1–O9 (DEC-080)
Context-Baseline: CTX-1.28
Version: 1.0
---

# WI-009-010 — First-Use and Invitation Arrival Workflow

## Metadata

- **Work Item ID:** `WI-009-010`
- **Status:** `DONE / PASS` — `OWNER APPROVED` (2026-10-03; owner review `PASS`; O10–O11). Verification: `ai-context/planning/verification/VER-WI-009-010.md`.
- **Phase:** `9 — Integration & UX Refinement`
- **Context baseline:** `CTX-1.28` (no requirement change; no `CTX-1.29`)
- **Planning basis:**
  - the first-use / onboarding review (2026-10-03);
  - `PRP-030` M7 and its first-use targets (approved under `DEC-074`): after accepting an invitation and reviewing the profile, land on Home, not Family;
  - `DEC-080`.
- **Supersedes in part:** the `PRP-022` / `WI-075-001` "Continue to Family" destination. The rest of `WI-075-001` (focused preview, explicit Accept / Decline, uniform unavailable handling, same-session handoff with retry, skippable profile review) is preserved.
- **Requirement references (preserved, not changed):**
  - `REQ-FAM-005` (invitation-only membership);
  - `REQ-FAM-006` (operator-provisioned first Family; no self-service creation);
  - `REQ-FAM-007` (profile fields);
  - `REQ-FAM-008` (invitation lifecycle and preview);
  - `REQ-UX-011` (session-only Family selection; sole Family auto-selected);
  - `REQ-UX-013`, `REQ-UX-014`, `REQ-PRIV-027`.

## Goal

A person arriving through a Family invitation is never stranded, and finishes on the Home dashboard with a one-time confirmation. A person with no Family sees plain guidance and one real action. Choosing a first Family keeps the current page. Administrator invitation feedback appears next to the action. Membership stays invitation-only, and nothing is added on the server.

## Invitation-only MVP boundary

There is no Family creation, join request, Family search, join code, sign-up, sign-in, password, production authentication, email delivery, email-ownership check, invitation status list, or operator provisioning UI. Accounts and profiles are not created by the application.

## Owner decisions (2026-10-03)

- **O1 — Successful invitation destination:** after acceptance, the server-confirmed membership reload, selection of the accepted Family, and the optional profile review, navigate through the router to `/home`. Never `/family`, and never Family content shown while the URL is `/`.
- **O2 — Join confirmation:** a one-time polite announcement on Home: "You joined {Family}."
  - It uses the navigation-state pattern of the Event-create message: it appears once and never after refresh or back/forward.
  - It is not a dashboard card.
  - The Family name comes from the caller's own server-returned membership list, and only when that Family is the selected one. It never comes from invitation data.
- **O3 — Profile review:** kept: optional, skippable, part of the handoff, the user's own profile only. Both actions go to `/home`.
- **O4 — No-Family Home:**
  - Copy: "You’re not part of a Family yet. Ask a Family administrator to send you an invitation. While you wait, you can review your profile and sharing choices."
  - One action: "Review your profile", linking to `/me`.
  - No Create Family, request access, search, join code, or invitation action.
- **O5 — Invitation error exit:** "Go to Home" whenever the invitation flow cannot continue: unavailable (all server-collapsed outcomes), capacity failure, and declined. The existing messages are unchanged.
- **O6 — First Family selection:** with no Family selected, choosing one keeps the current page, which reloads for that Family.
  - This removes the old redirect to Home for that case only.
  - Switching between Families when one is already selected behaved this way before and is unchanged.
  - Invitation completion still goes to `/home` (O1).
- **O7 — Administrator invite feedback:**
  - "Invitation sent." and "Invitation revoked." (and invite errors) render inside the Invite card, under its buttons, so they are visible at 390 px.
  - The administrator privacy statement appears once on Family settings, in Family details.
- **O8 — One-person Family Event limitation:** out of scope; Event creation and audience behavior are unchanged.
- **O9 — Authentication and provisioning:** out of scope.

## Scope (as implemented)

- **Invitation success:** preview → Accept → server confirms → the membership list reloads → the accepted Family is selected → profile review. Then "Continue to Home" or "Continue to Home without changes" → `/home` (router, with focus on the Home `h1`) → "You joined {Family}." in the existing polite status line (`p.family-switch-status`, used by `REQ-UX-014` switch announcements).
- **Invitation decline:** unchanged result "Invitation declined." with "Go to Home".
- **Invitation unavailable:** unchanged "This invitation is unavailable." plus "Go to Home" (navigation only; no Retry). Capacity failure keeps its own message plus "Go to Home". The Accept and Decline buttons stay as before.
- **Shared Family-context prompt** (areas that need a Family): "This area needs an active Family. Ask a Family administrator to send you an invitation." plus "Review your profile" linking to `/me`. The multi-Family "Choose your current Family above…" prompt and the load-error prompt are unchanged.
- **Me / Settings:** unchanged; it is only the destination of the new links.
- **Routes:** unchanged. No onboarding route, wizard, or new component.
- **Architecture:** the existing `App` root (`app.ts`, `app.html`, `app.scss`); no extraction.

## Duplicate no-Family messages (O11, resolved)

The owner chose a narrow fix (O11). When the shared prompt shows its no-Family state, the Events and Plans inner no-Family copy does not render and the Family directory section is hidden (`sharedNoFamilyPromptShown`). The global `.page-content` rule is unchanged, so the Home profile-load error is preserved. See `VER-WI-009-010`. The original finding follows.

### Original finding

The review found three inner no-Family messages (Events, Plans, Family directory) and judged them unreachable behind the shared prompt. Implementation showed they **are reachable**:

- `.page-content { display: grid }` overrides the browser's `[hidden]` rule, so the `page-content` container's `[hidden]` binding never hides it.
- On `/family`, `/events`, and `/plans` with no Family, the prompt and the inner message both show.

All three were restored exactly, with their original copy. This is a pre-existing CSS behaviour, reported for an owner decision; it is not changed here.

## Explicit exclusions

- Family creation, authentication, sign-up, provisioning, email delivery or verification, invitation status lists.
- Any server, API, endpoint, contract, schema, migration, authorization, or invitation-token change.
- Event creation or audience changes (O8).
- Any Home, Family, Events, Safety, or Me / Settings redesign beyond the items above.
- An `App` refactor.

## Acceptance checks

1. `AC-01` After acceptance and either profile-review action, the browser is at `/home` with the accepted Family selected; it never shows Family content at `/`.
2. `AC-02` "You joined {Family}." shows once on that arrival. It is absent after refresh or back/forward, and absent if the selected Family is not the accepted one. The name comes only from the caller's membership list.
3. `AC-03` The profile review stays optional; saving still uses the existing owner-only rules.
4. `AC-04` No-Family Home shows the O4 copy and one "Review your profile" link to `/me`, with no other action.
5. `AC-05` Unavailable, capacity, and declined invitation states each offer "Go to Home", with unchanged messages and no disclosed reason.
6. `AC-06` The shared prompt shows the concise copy and the `/me` link.
7. `AC-07` From no selection, choosing a Family on `/events` or `/family` stays on that page and loads that Family's data; switching between Families is unchanged.
8. `AC-08` Invite feedback renders inside the Invite card, only on Family settings, and is in view at 390 px after Send and Revoke; the administrator privacy statement appears once.
9. `AC-09` 1440 and 390 px: no horizontal overflow; the invitation screen stays focused (no main navigation); all actions are reachable.
10. `AC-10` (after owner review) Angular specs updated and added; `npm run build` passes with no new warning.
11. `AC-11` (after owner review) Focused Playwright (`invitation-handoff` lands on Home, plus terminal-state exits) and one full regression.
12. `AC-12` Owner approval.

## Implementation record (2026-10-03)

- **Files:**
  - `src/FamilySafety.Web/src/app/app.ts`:
    - `continueToHome()` replaces `continueToFamily()`;
    - `announceJoinedFamily()`, `arrivedFromInvitation()`, and `pendingJoinedFamilyId` (consumed at the first membership load);
    - `switchCurrentFamily()` no longer redirects to Home after a first choice.
  - `app/app.html`:
    - the invitation "Go to Home" condition;
    - the profile-review button labels and actions;
    - the no-Family Home card;
    - the shared-prompt copy and link;
    - invite feedback moved into the Invite card (Family settings only), and the top-of-page invitation messages removed;
    - the Invite card help consolidated.
  - `app/app.scss`: layout for the no-Family card and the prompt link.
- **Build:** `npm run build` passes with the existing warnings only. `main` 595.94 kB, complete initial total 608.41 kB, estimated transfer 134.02 kB. No new stylesheet warning.
- **Not run (by owner direction):** Angular specs, Playwright, regression. No tests written or changed. Expected updates:
  - `first-use.spec.ts` (button labels; the Home destination; the "Go to Home" exits);
  - `home.spec.ts` (no-Family copy);
  - `e2e/invitation-handoff.spec.ts` (no-Family text, button labels, Home arrival);
  - `shell.spec.ts` (Invite card wording around "Administering Family A", which is kept);
  - any spec asserting the old redirect after a first Family choice.
- **Review renders (transient):** `src/FamilySafety.Web/e2e/captures/first-use-review/`, with `first-use-contact-sheet.png`.
  - Real Development data: unavailable invitation (a made-up token; real server 404), Family settings Send then Revoke for `wi009010-review-{1440,390}@example.invalid` (each left revoked), Home, Me.
  - Browser-only controlled responses, labelled on each image: an empty Family list (no-Family states), invitation preview, accept (pointing at the existing Development Family, so nothing is written), capacity 409, decline, and a two-Family list (O6).
  - Checked during capture:
    - URLs `/home` after the "Go to Home" exit and after arrival;
    - "You joined Development Family." shown on arrival and absent after refresh;
    - focus on the Home `h1`;
    - invite feedback inside the viewport at 1440 and 390;
    - one administrator privacy statement;
    - O6 stays on `/events` and `/family`;
    - zero horizontal overflow on every render.

## Remaining tasks

- [x] `TASK-001` Implement O1–O7.
- [x] `TASK-002` Review renders at 1440 and 390, plus the contact sheet.
- [x] `TASK-003` Owner review (`AC-12`): `PASS` 2026-10-03; O10 keep the Continue to Home wording; O11 narrow duplicate fix.
- [x] `TASK-004` Testing pass: Angular 81/81 focused, 269/269 full; focused Playwright 3/3; build PASS.
- [x] `TASK-005` Full regression 30/30, evidence (14 PNGs), `VER-WI-009-010`, closure.
