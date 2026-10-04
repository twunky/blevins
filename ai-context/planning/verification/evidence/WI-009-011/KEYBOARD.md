# Live keyboard-only verification

Date: 2026-10-03 (America/New_York; host records use 2026-10-04 UTC). Reviewer: implementing agent, SELF-VERIFIED. Result: PASS for the supported keyboard paths. This was an interactive agent-led check in the Codex in-app browser, separate from the Playwright spec and from a human participant study. The owner usability walkthrough remains a separate readiness exercise.

## Method and ownership

Used the computer-use skill and `cua_repl` browser control. Browser input consisted of Tab, Shift+Tab, Enter, Space, arrows, Escape and text typing at the current focus. No pointer actions, element-targeted focus jumps, DOM mutation, form fill APIs or direct route entry after the initial Home load. A small helper repeated Tab/Shift+Tab and inspected the actual focused accessibility element; it never assigned focus. Native date/time fields were completed with keyboard segments. Route/state observations used fresh accessibility snapshots, with screenshots for visible focus and narrow layout.

The database was `FamilySafety_Invitation_E2E_2750336d367a405b9840a84489058f23`, owned by WI-009-011 focused axe run `20261004T003701-5e709e5e`. Its retained seed had A administrator, B without membership and C active. It was resumed through `start-invitation-scenario.ps1 -NoBuild`, then the same database was resumed with the existing `-IncludeSecondFamily` fixture for the returning-member check. No shared/protected/unknown database was used. Browser and both owned server sessions were stopped after review; viewport override was reset.

A browser reload began the fresh returning-member session; browser refresh is a separate browser operation, not a claimed keyboard accelerator. `Ctrl+R` did not visibly reload the in-app browser, so that attempt is not counted as refresh evidence. Automated journey/item tests provide the item-refresh evidence.

## Observed invited-adult path

| Step | Keyboard interaction and actual outcome |
|---|---|
| Administrator / Family | Tab from Development identity through outbox and navigation to Family; Enter. Family h1 gained visible focus. Tab to named Settings gear, Enter; Settings h1 gained focus. |
| Invite | Tab to recipient email, type `development-recipient@example.invalid`, Tab/Enter Send. Card reported Invitation sent; next Tab reached Revoke. No external delivery; Development outbox only. |
| Identity / no-Family Home | Reverse tab order back to identity selector; Down selected B, Escape closed it. Home showed invitation-only guidance and own-profile review; no Family context or self-service creation. |
| Invitation | Tab/Enter outbox toggle; Tab/Enter Open invitation. Tab reached Accept; Tab reached Decline; Shift+Tab/Enter accepted. Main navigation was absent in the invitation preview. |
| Optional profile | Review showed active membership and optional changes. Keyboard traversed privacy controls and returned to Continue to Home without changes; Enter. Home showed joined notice, selected Development Family and focused greeting h1. |
| Existing Home item | Tab/Shift+Tab reached Upcoming Plan link. Enter opened `/plans/{id}` with item content and heading focus. Tab/Enter Back to Plans returned to list/create page. |
| Plan create | Typed Keyboard review Plan; native keyboard segments entered 2026-10-05 and 09:00; Space selected This Family Group; Tab/Enter submitted. Success status and specific Plan row appeared. No lifecycle change was inferred. |
| Event create | Reverse-tab main Events, Enter; Tab/Enter header Create event. Filled title, location, description, native date/time segments; arrows chose A Can view; Enter submitted. Event appeared at `/events/{id}` with focused h1 and created status. |
| Grouping / Trip create | Reverse-tab Plans & Trips, Enter. Two grouping links reachable. Open Trips → Create Trip; typed title/destination/start/end; Space selected B traveler and Family audience. Enter submitted; details and recipient list appeared. |
| Confirmation | Keyboard Change Audience → Selected people; Space removed C in this isolated Trip; Preview audience change. Preview explicitly listed nobody gaining and C losing access, including retained-note consequence. Confirm was the next focusable control, with a visible focus ring. Enter confirmed; recipient list updated. This is an inline preview/confirmation, not an invented modal. Back to Trips returned to list. |
| Concern | Main Safety → View all Concerns → Create a Concern. Subject A selected with arrows; reason typed; creator-only audience kept; Enter Create Concern. Own item URL appeared with explicit subject-not-automatically-shared wording. |
| Separate Incident | Tab/Enter Start an Incident from Concern. Origin banner said no history/people copied. Typed separate reason, selected Reported, typed source; Enter Create Incident. Item URL showed Reported/source, coordinator B and Concern lineage. |
| Safety item / returns | Keyboard Back to Incidents → Back to Safety. Safety showed B's authorized Concern/Incident only; Tab/Enter specific Incident row opened its item route with h1 focus. |
| Me / member / Settings / Home | Main Me showed B's own profile. Family member button showed only A's shared profile fields; ordinary-member Settings had no Invite controls. Back to Family worked. Main Home showed the newly created Event/Plan/Trip links. |

## Observed returning multi-Family path at 390 px

1. Fresh browser load after the isolated second-Family fixture: no Family selected; Home asked for choice and showed no previous summary rows.
2. Tab reached Current Family selector in header order. Its focus ring was visible. Down/Escape selected Development Family, keeping `/home` and loading B's Event/Plan/Trip summaries.
3. Down/Escape switched to Development Second Family. All first-Family Home item rows disappeared; honest empty summaries named the second Family where appropriate. Focus stayed on selector.
4. Tab/Enter reached Family; h1 focus was visible at 390. Settings gear and Back to Family both worked with keyboard and ordinary-member permissions.
5. Main Events showed second-Family empty summaries. Reverse-tab selector, Up/Escape changed to first Family, keeping `/events` and rendering first-Family Event/Trip rows.
6. Main Plans & Trips, Safety and Me were reachable in the narrow two-column nav. Safety retains its deliberate cross-authorized-Family behavior; it is not a current-Family-only list.
7. Me Save profile with unchanged values reported Profile saved. Main Home regained focused h1. The viewport was reset and the temporary tab closed.

## Focus, semantics and limits

- Observed visible focus on Family/Home headings, Family selector and confirmation control; keyboard focus followed the visible order, including native date/time subcontrols. No keyboard trap or required pointer action.
- Main/grouping/fresh-shell navigation moves focus to h1; existing matched Concern/Incident internal route transitions do not force shell h1 focus (WI-009-005 accepted policy). New local composers focus their own h2. Removed/temporarily disabled controls can expose body focus while tab progression resumes at the prior position; this did not block completion. No unapproved focus redesign was introduced.
- Landmarks/headings and labels were reviewed in the live tree and all-route axe scans. aria-current semantics are proven by existing navigation/component checks, not inferred from the browser's active-link styling.
- The live full creation path used the default desktop viewport; the returning/six-area path used explicit 390×844. Automated route renders cover 1440×1000 and 390×844. This record does not claim a human owner visual review or a participant study.
