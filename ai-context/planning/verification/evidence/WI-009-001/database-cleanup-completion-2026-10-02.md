# Local Cleanup Completion Record — 2026-10-02

- **Authorization:** the project owner approved this one-time cleanup on 2026-10-02. It covered every Class C and both Class B databases, the seven listed build-output folders, and `src/FamilySafety.Web/e2e-alt/` if it was still empty. The authority is [database-cleanup-inventory-2026-10-02.md](database-cleanup-inventory-2026-10-02.md); no broader deletion set was derived.
- **Performed:** 2026-10-02, by a Claude Code session.
- **Work item:** `WI-009-001` stays `DONE / PASS` — `OWNER APPROVED`. It was not reopened.
- **Server:** `localhost` (default instance), Windows integrated security. No credentials were used or stored.

## Result

| Measure | Value |
|---|---|
| `FamilySafety*` databases before | **82** |
| Approved for deletion (64 Class C + 2 Class B) | **66** |
| Deleted and confirmed absent | **66** |
| Skipped | **0** |
| Protected (1 Class A + 15 Class D) | **16**, all still present |
| `FamilySafety*` databases after | **16** |
| Removed but not approved | **0** |
| New databases during cleanup | **0** |
| Folders deleted | **8** (7 build-output folders and the empty `e2e-alt/`) |
| Folders skipped | **0** |

Databases outside the `FamilySafety*` family were not part of this cleanup or of these counts.

## Pre-cleanup reconciliation

- The section A–D tables of the inventory were parsed by exact name: A = 1, B = 2, C = 64, D = 15, total 82, with no overlap or duplicates.
- `sys.databases` held exactly those 82 names, with no differences.
- Class D matched the owner's list of 15 exactly. Class A was exactly `FamilySafety`. Class C held 61 names in the exact `FamilySafety_Invitation_E2E_<32 hex>` format plus the three named test databases.
- Each of the seven folders still had the 5 files recorded in the inventory, none changed after 2026-09-28. `e2e-alt/` still held 0 files and 4 empty folders.

## Procedure and safety checks

**Class B (2).** Each was removed with `scripts/remove-scenario-database.ps1 -DatabaseName <name> -ManifestPath <its run manifest> -RemoveRetained`, applying all of the script's WI-009-001 checks. Both exited 0, logged `Dropping scenario database: <name>`, and confirmed the database was gone. Their manifest entries are now `removed`.

**Class C (64).** The guarded script correctly refuses these (they are PRE-EXISTING), so a **temporary one-off PowerShell procedure** was used. It is kept only in the session scratchpad, not in the repository, and no reusable tool was added. Its safeguards:

- **Input:** only the exact section C list from the inventory. Names were never discovered from the server, and no wildcard or prefix was used.
- **Before any drop, it would stop** unless:
  - there were exactly 64 unique names;
  - none of them was `FamilySafety` or one of the 15 Class D names (hard-blocked);
  - every name was either a 32-hex legacy E2E name or one of the three named Class C databases;
  - every name existed exactly once on the server.
- **For each name, it:**
  - confirmed the name existed exactly once and had no open sessions;
  - logged `Dropping database: <exact name>`;
  - ran `ALTER DATABASE … SET SINGLE_USER WITH ROLLBACK IMMEDIATE` and `DROP DATABASE` on a server-side `QUOTENAME` of a **parameter**;
  - confirmed the database was gone, and stopped at the first anomaly.

**Interruption, disclosed.** The first pass was ended after 2 of 64 drops by an operator error: the command that launched it cut off its output stream, which terminated the process. Both drops (`…48b57bae…`, `…f9f33bbd…`) had been logged and confirmed. A state check showed:

- no process left running;
- exactly 4 databases gone (the 2 Class B and those 2);
- all 16 protected databases present;
- no database left in single-user or any other abnormal state.

The procedure then resumed with the same safeguards on the remaining **62** names of the section C list (expected count 62). All 62 were dropped and confirmed absent, with exit 0.

**Folders.** Each of the seven folders was deleted by its exact name, only after re-checking that it still matched the inventory (5 files, `3rdpartylicenses.txt` present). `e2e-alt/` was deleted only after re-confirming it held 0 files and its 4 empty folders.

## Post-cleanup verification

- `sys.databases` holds **16** `FamilySafety*` databases, exactly the 16 protected names.
- None of the 66 approved names remains. No database outside the approved list disappeared, and none appeared.
- `FamilySafety` exists. All 15 Class D databases exist.
- The repository root now lists `ai-context/ claude-handoff/ codex-handoff/ docs/ scripts/ src/ tests/`, which is the earlier list minus exactly the seven build folders. `src/FamilySafety.Web/` still contains `dist/ e2e/ node_modules/ public/ src/`.
- Untouched: the Playwright harness files, the scripts, every `ai-context` record and evidence folder, the run manifests (`e2e/scenario-runs/`, 19 runs), and all source, test, and migration files.

## Databases preserved (16)

| Database | Class |
|---|---|
| `FamilySafety` | A — production/default |
| `FamilySafety_WI005001_Verification` | D |
| `FamilySafety_WI005002_Verification` | D |
| `FamilySafety_VisualReview` | D |
| `FamilySafety_Invitation_WI075_Browser_20260928` | D |
| `FamilySafety_Invitation_WI075004Visual` | D |
| `FamilySafety_Invitation_WI075006VerifyHost` | D |
| `FamilySafety_Invitation_Stage1Retry_20260928_235812_c7f2` | D |
| `FamilySafety_Invitation_79558d15516649aeb6758a9e2496b6b7` | D |
| `FamilySafety_Invitation_ddf3f406d9024bc1ac330e46513b5209` | D |
| `FamilySafety_Invitation_WI075005_CodexAttempt_20260929` | D |
| `FamilySafety_Invitation_ConcernReview20261001` | D |
| `FamilySafety_Invitation_ShellReview034731` | D |
| `FamilySafety_Invitation_SiteReview044319` | D |
| `FamilySafety_Invitation_SiteReview044420` | D |
| `FamilySafety_Invitation_Review_WI008004` | D |

## Databases deleted (66)

Listed in drop order, taken from the procedure and script logs.

| # | Database | Class | Method |
|---|---|---|---|
| 1 | `FamilySafety_Invitation_E2E_48b57bae6f7a440a9d88f5173e0640f7` | C | One-time exact-list procedure |
| 2 | `FamilySafety_Invitation_E2E_f9f33bbd0b374adb8f8447fd98ae6883` | C | One-time exact-list procedure |
| 3 | `FamilySafety_Invitation_E2E_e889b03f305a49d3b0736dad7f80bda1` | C | One-time exact-list procedure |
| 4 | `FamilySafety_Invitation_E2E_d08a62e70f804721befc2406454d6f32` | C | One-time exact-list procedure |
| 5 | `FamilySafety_Invitation_E2E_b0c8426153c3403596eb22a0bca0db63` | C | One-time exact-list procedure |
| 6 | `FamilySafety_Invitation_E2E_68f70d147de948d6a122d7ea17783ec6` | C | One-time exact-list procedure |
| 7 | `FamilySafety_Invitation_E2E_bcec962e1e6b4ecc856294459423e7c0` | C | One-time exact-list procedure |
| 8 | `FamilySafety_Invitation_E2E_de47349937e04c33af6db9b7fbd7def3` | C | One-time exact-list procedure |
| 9 | `FamilySafety_Invitation_E2E_a183380fc8d34f37960a1209a6a66f3d` | C | One-time exact-list procedure |
| 10 | `FamilySafety_Invitation_E2E_50fcac1962d04e05865ed9039e58e307` | C | One-time exact-list procedure |
| 11 | `FamilySafety_Invitation_E2E_c3b17466100143bd851da7012787d192` | C | One-time exact-list procedure |
| 12 | `FamilySafety_Invitation_E2E_30b65792562e421a9f58e4eaeae4d02a` | C | One-time exact-list procedure |
| 13 | `FamilySafety_Invitation_E2E_972ad0dfc6e449a1a47c445cb92d6dd7` | C | One-time exact-list procedure |
| 14 | `FamilySafety_Invitation_E2E_a30397b7632d43919bdb75522039564f` | C | One-time exact-list procedure |
| 15 | `FamilySafety_Invitation_E2E_4b5778faaf7447119bd828a3879914c4` | C | One-time exact-list procedure |
| 16 | `FamilySafety_Invitation_E2E_912cdd7570504747807487dd107332fd` | C | One-time exact-list procedure |
| 17 | `FamilySafety_Invitation_E2E_60b7ee89dcab4c4aadacb337485db6c3` | C | One-time exact-list procedure |
| 18 | `FamilySafety_Invitation_E2E_260198eff2df4f19ac5c4e9b73ec3dde` | C | One-time exact-list procedure |
| 19 | `FamilySafety_Invitation_E2E_711e59c089b74ced97c77f79c6876099` | C | One-time exact-list procedure |
| 20 | `FamilySafety_Invitation_E2E_706eab5e33fb41e1bde6ba60f6b65545` | C | One-time exact-list procedure |
| 21 | `FamilySafety_Invitation_E2E_b6b566eb0f5d4d4db2a65ac362e2368b` | C | One-time exact-list procedure |
| 22 | `FamilySafety_Invitation_E2E_4185eb0abf854c6c8e03a0d4ffa18e48` | C | One-time exact-list procedure |
| 23 | `FamilySafety_Invitation_E2E_6bb607462316461783cdc22178620d82` | C | One-time exact-list procedure |
| 24 | `FamilySafety_Invitation_E2E_0990f03717354b4aa47eadeb41a42e54` | C | One-time exact-list procedure |
| 25 | `FamilySafety_Invitation_E2E_b7d3f44e17c74950890d1d62bc6e0d09` | C | One-time exact-list procedure |
| 26 | `FamilySafety_Invitation_E2E_ea627978a0e544448f6913be016550b5` | C | One-time exact-list procedure |
| 27 | `FamilySafety_Invitation_E2E_13ce6011514748bda91d39fec91fa5ea` | C | One-time exact-list procedure |
| 28 | `FamilySafety_Invitation_E2E_11827fd5ef50441499b162fd01742932` | C | One-time exact-list procedure |
| 29 | `FamilySafety_Invitation_E2E_15cbd87b9af14fff999b19f2f1cbb479` | C | One-time exact-list procedure |
| 30 | `FamilySafety_Invitation_Test_9d0590e7073c48a287a158e4d937ef65` | C | One-time exact-list procedure |
| 31 | `FamilySafety_Invitation_E2E_17c47cace43942a2a92482affbe97dd8` | C | One-time exact-list procedure |
| 32 | `FamilySafety_Invitation_E2E_1b2b4c99f8b9407cb9f16d225aaf4a11` | C | One-time exact-list procedure |
| 33 | `FamilySafety_Invitation_E2E_bec34cd6852c40d3ad633f08d72c7672` | C | One-time exact-list procedure |
| 34 | `FamilySafety_Invitation_E2E_3e13fbaddb0e406fb34cf2fd9b9ea049` | C | One-time exact-list procedure |
| 35 | `FamilySafety_Invitation_E2E_22b4806ee2a548dca8cacf1a400cde36` | C | One-time exact-list procedure |
| 36 | `FamilySafety_Invitation_E2E_71b12134dff046d9867af76fcd0ae68c` | C | One-time exact-list procedure |
| 37 | `FamilySafety_Invitation_E2E_162839bccb2b42f7bf4f16c54e3a5771` | C | One-time exact-list procedure |
| 38 | `FamilySafety_Invitation_E2E_0a1de4a16f74435aa3292916fdf5a3b4` | C | One-time exact-list procedure |
| 39 | `FamilySafety_Invitation_E2E_7c125cd7105e4cd187c644b4384184fd` | C | One-time exact-list procedure |
| 40 | `FamilySafety_Invitation_E2E_c2bf0b1f5c6b4b939f5841a22def425e` | C | One-time exact-list procedure |
| 41 | `FamilySafety_Invitation_E2E_357f0cb27706460fb9cff4f671430a50` | C | One-time exact-list procedure |
| 42 | `FamilySafety_Invitation_E2E_33a2507135da4f44918da698fe123e09` | C | One-time exact-list procedure |
| 43 | `FamilySafety_Invitation_E2E_10210b3af7114593822826344598eb75` | C | One-time exact-list procedure |
| 44 | `FamilySafety_Invitation_E2E_dd5707a92ebd4b3eb5562fa70d057226` | C | One-time exact-list procedure |
| 45 | `FamilySafety_Invitation_E2E_06408c857b3146f0aa29214dea7993d1` | C | One-time exact-list procedure |
| 46 | `FamilySafety_Invitation_E2E_e715931c2af444db8b7b31a10b7be88f` | C | One-time exact-list procedure |
| 47 | `FamilySafety_Invitation_E2E_41f70946f14a46a892b9699fd4308788` | C | One-time exact-list procedure |
| 48 | `FamilySafety_Invitation_E2E_77bad682ddc547058d9b238c453ddf4c` | C | One-time exact-list procedure |
| 49 | `FamilySafety_Invitation_E2E_a6d6a7c7fab943af9757eef35edc8e81` | C | One-time exact-list procedure |
| 50 | `FamilySafety_WI005003_DirectoryTests` | C | One-time exact-list procedure |
| 51 | `FamilySafety_WI005002_ConcurrencyTests` | C | One-time exact-list procedure |
| 52 | `FamilySafety_Invitation_E2E_929c0ff1d97e4e08a8543f8a170cb784` | C | One-time exact-list procedure |
| 53 | `FamilySafety_Invitation_E2E_fcc7e99342d54d54879405c38aaa6f88` | C | One-time exact-list procedure |
| 54 | `FamilySafety_Invitation_E2E_787888ae581148a59e1f00f271a48b0a` | C | One-time exact-list procedure |
| 55 | `FamilySafety_Invitation_E2E_d1b879c917f34ef8a3849967ec25346a` | C | One-time exact-list procedure |
| 56 | `FamilySafety_Invitation_E2E_ab719bbbc8994476a9f9a61020d6ba32` | C | One-time exact-list procedure |
| 57 | `FamilySafety_Invitation_E2E_244ef79174674418b89c4bb0bc394a66` | C | One-time exact-list procedure |
| 58 | `FamilySafety_Invitation_E2E_fdcbfe3e59f64b63bbc3b30b606e92f1` | C | One-time exact-list procedure |
| 59 | `FamilySafety_Invitation_E2E_a999aedf33034b54b6ddbf3efeff60dc` | C | One-time exact-list procedure |
| 60 | `FamilySafety_Invitation_E2E_69258b4fa271423ea0d3bfe55955d6e5` | C | One-time exact-list procedure |
| 61 | `FamilySafety_Invitation_E2E_ea672ed2d1d44d6bbb9406526507e02f` | C | One-time exact-list procedure |
| 62 | `FamilySafety_Invitation_E2E_570abca5d6184ed2b46c57f2ed729b8a` | C | One-time exact-list procedure |
| 63 | `FamilySafety_Invitation_E2E_c50128dd5fe8424d90df20ac58528527` | C | One-time exact-list procedure |
| 64 | `FamilySafety_Invitation_E2E_01fb9f9635b140649b793fe2fe8a8b14` | C | One-time exact-list procedure |
| 65 | `FamilySafety_Invitation_E2E_408952ea95564bce9ef95c840e77f120` | B | `remove-scenario-database.ps1 -RemoveRetained` with its run manifest |
| 66 | `FamilySafety_Invitation_E2E_537e62aafa9f4633a4144dafaad77e6e` | B | `remove-scenario-database.ps1 -RemoveRetained` with its run manifest |

## Folders deleted (8)

`wi075003-build-1`, `wi075003-build-2`, `wi075003-build-3`, `wi075003-build-final`, `wi075003-build-verification`, `wi075004-build`, `wi075004-build-final` (repository root), and `src/FamilySafety.Web/e2e-alt/`.

## Related updates

- `scripts/pre-existing-scenario-databases.txt` now lists only the 16 remaining databases, as step 4 of the inventory's procedure requires, with a dated note. Its header's snapshot-1 count was corrected from a mistyped "77" to the actual 75.
- The two Class B manifest entries are marked `removed`.
- Every Class D database still needs an owner decision before any change.
- The two fixed-name SQL test databases (`FamilySafety_WI005003_DirectoryTests`, `FamilySafety_WI005002_ConcurrencyTests`) are recreated automatically the next time the SQL-gated .NET tests run.
