## Why

The primary navigation was a single flat row of pill links that wrapped awkwardly on desktop and stacked into several rows on phones, eating vertical space. On large screens the content column also maxed out around 720px while the header/footer spanned wider, so the app felt narrow and misaligned. Different users lean on different features — someone in recovery wants that page front-and-centre; someone focused on sleep and journaling doesn't want therapy tools in the way — but everyone got the same fixed nav.

## What Changes

- Replace the flat nav row with a responsive pattern: a horizontal pill row on desktop and a fixed **bottom tab bar** on phones (`Home` + the first few pages + a `More` sheet for the rest).
- Introduce a single navigation catalog as the source of truth for the desktop row, mobile bottom bar, Home feature cards, and the Account editor.
- Let signed-in users **choose which pages appear and in what order**; the order drives both the bottom-bar tabs and the desktop row. `Home` and `Account` stay fixed.
- Persist the layout **hybrid**: a plaintext copy in this browser's local storage for instant application on every load (and for guests), plus an envelope-encrypted copy synced across devices via `user_prefs.nav_payload`. Reconciled last-write-wins on vault unlock. **BREAKING** (data): adds a `nav_payload` column to `user_prefs` and makes `payload` nullable.
- Add an **Explore** section on Home with a card for every feature so users can discover the whole app by scrolling; auth-gated cards prompt guests to sign in.
- Widen the app to a ~900px shell on large screens for chrome and dashboard/grid content, while keeping prose and forms at a comfortable reading measure (~720–768px).

## Capabilities

### New Capabilities
- `navigation`: The set of primary destinations, how they are presented responsively (desktop row vs. mobile bottom bar + More sheet), how users customize which pages show and their order, how that layout is persisted and synced privately, and how Home surfaces every feature for discovery.
- `responsive-shell`: The width tiers for page content — a wide shell for chrome and dashboard/grid layouts, and a narrower reading column for prose and forms.

### Modified Capabilities
<!-- None — openspec/specs/ has no existing capabilities yet. -->

## Impact

- **Database**: `user_prefs` gains `nav_payload text` (encrypted, nullable, length-checked); `payload` becomes nullable so a nav-only prefs row is valid. Governed by existing per-user RLS.
- **New code**: `Models/NavItems.cs` (catalog), `Services/NavPrefsService.cs` (hybrid persistence + sync), `Components/NavIcon.razor`, `Layout/BottomNav.razor`.
- **Modified code**: `Layout/NavMenu.razor`, `Layout/MainLayout.razor`, `Pages/Home.razor`, `Pages/Signin.razor`, `Program.cs`, `Styles/tailwind.css`.
- **Privacy**: tab order is never legible to the server (encrypted); the accepted trade-off is a plaintext copy in the user's own browser storage (same as the theme preference).
