# WI-009-003 rendered evidence — Six-Area Navigation and Safety Discoverability

Captured 2026-10-02 by `src/FamilySafety.Web/e2e/navigation.spec.ts`, during the single full-suite run (`npm run test:e2e`, run `20261002T235519-6cabe351`, 12/12 passed). The spec ran against its own isolated `FamilySafety_Invitation_E2E_*` scenario database: real API on `:5080`, Angular dev server on `:4200`, headless Chromium, signed in as Adult A, the default development identity.

- The spec wrote these files to `src/FamilySafety.Web/e2e/captures/navigation/`, which is transient and ignored. They were copied here only during verification and renamed from `-desktop` / `-narrow` to `-1440` / `-390`. This folder is new, so no existing evidence was overwritten.
- Each page was reached **through the main navigation or a landing link only** (no `page.goto`), except the initial `/home` load. Each capture shows the selected area.
- Every 390 px capture is preceded by the check `document.documentElement.scrollWidth <= 390`. All pages passed, so there is no horizontal overflow.
- In desktop captures the page `h1` has focus after the link was clicked. It shows no ring because the click came from a pointer; the keyboard test checks that the ring appears after keyboard navigation.
- The yellow **Development test identities** banner is the local test-identity switcher. It is expected in captures and is not product UI.
- All states are real; nothing is mocked. Adult A has no Events, Plans, Trips, Concerns, or Incidents in this fresh scenario, so the child pages show their existing empty states.

| File stem | Page | Selected area (`aria-current`) | Widths |
|---|---|---|---|
| `home` | `/home` (initial load) | Home (`page`) | 1440, 390, plus 1024, 768, 600 |
| `family` | `/family` | Family (`page`) | 1440, 390 |
| `events` | `/events` | Events (`page`) | 1440, 390 |
| `plans-trips-landing` | `/plans-trips` landing: Plans and Trips cards | Plans & Trips (`page`) | 1440, 390 |
| `plans` | `/plans`, reached via the landing's "Open Plans" | Plans & Trips (`true`) | 1440, 390 |
| `trips` | `/trips`, reached via the landing's "Open Trips" | Plans & Trips (`true`) | 1440, 390 |
| `safety-landing` | `/safety` landing: Concerns and Safety Incidents cards | Safety (`page`) | 1440, 390 |
| `concerns` | `/concerns`, reached via "Open Concerns" | Safety (`true`) | 1440, 390 |
| `incidents` | `/incidents`, reached via "Open Safety Incidents" | Safety (`true`) | 1440, 390 |
| `me` | `/me` | Me / Settings (`page`) | 1440, 390 |

`home-1024`, `home-768`, and `home-600` come from the intermediate-width test (D2). At 1024 px (64 rem) the desktop sidebar is used. At 768 px the six links form 2 rows × 3 columns. Below 44 rem (704 px), at 600 px and 390 px, they form 3 rows × 2 columns.

## SHA-256

```text
1e2ef19888c75fc2fcde9e9da990c672eb55594c987268c5e6b82818bcfae139  concerns-1440.png
31cbefe633b382cb1317ad3a010917e54f3173fed677b80aa7617413db8d3edf  concerns-390.png
1e81e6ebc504a2764867ca1cbe96669432321bb4de0c6d35529566c50c5855ab  events-1440.png
b3b59e204c7bc77ae3f99094a5a60ddb6e4de1331692ba2603787dbf19834fb8  events-390.png
449926a711be1e73f3318f943d25938e8ed55da7b467d650d05432aa92b033ac  family-1440.png
a2962147fb1470b84a58648ade59bcdb9006fd23ec520013dafd33792ea3c09d  family-390.png
43319d6a95778e71c68d3ab060b2dc4a21d732a84d8ac3775291610e59a1552b  home-1024.png
e36af69ee346105b9d7d599fb7f6fd9bd366f62d390780961845180c6ceb186b  home-1440.png
ca8f7055439815db3a173c9bd6a6d5381310e6c6886d207ba71afc90ba659639  home-390.png
878f1e1df701fa825ad7dc7309ccf8e188db688dd9c7e055f9c4f11d3bf36306  home-600.png
7efd5f9b63b49ee0df9f5277a7fcad7c6493f6efd5d9ddcba1088cc07637abfe  home-768.png
4bb78db0d91ac31f5ef87a9a2089fdfa252e40955c27317c1aa3cf583bec9e50  incidents-1440.png
ec7412f7a73a3577df4454b4a86e11889eefd99e14ddabdcf9adc27c215686a8  incidents-390.png
47dd7860cc437b97343bccb3416e25a3da156e75254f273634bef4946eb121e1  me-1440.png
595727d4875e620a2d347894aa1c85edaedd4afc5a92634457c3337b0512804a  me-390.png
c6ddd05aea9a74f01bcf6d6e4b17a7d7d404f35d4392005788424359e6ae35ac  plans-1440.png
c5ace13a961c548fd746875ed39c80669078ba4c4783772694805bc4c8f2b48e  plans-390.png
6ae5d18ed541c94a42ccfe3a5773eff547438c72e827244ece923f14305b4909  plans-trips-landing-1440.png
75bd709fb760f9d98e0a03cfc50590888f02d9ec150feb3e5711cd376a02a5d8  plans-trips-landing-390.png
ecf1710a4b6727e19c7ae97da277f3cd146926b9207bc20fd287b20684dd24e9  safety-landing-1440.png
4ceeb4848615d9d6103f8fd843fd12a06406cece0000a3cc005926f4ec48983d  safety-landing-390.png
790424fc2840e73d6eb9dade62130293bef983e19a35e829ef3ed3160e4cb25f  trips-1440.png
0eac94de88fbb015b10127080c46eb90c67a669dc6f5f0b924bf8a1bc194f3ce  trips-390.png
```
