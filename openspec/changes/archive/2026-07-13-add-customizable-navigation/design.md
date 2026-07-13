## Context

Navigation was a single hard-coded flat row rendered in `NavMenu.razor`, duplicated implicitly by the Home "features" copy. It wrapped poorly on desktop and consumed vertical space on phones, and the content column was capped narrower than the header/footer. The app is a zero-knowledge Blazor WASM PWA talking directly to Supabase (PostgREST) with no server of our own; sensitive user data is AES-GCM encrypted in the browser, and the encryption key (DEK) lives only in tab memory and is wiped on every load (`crypto.js`). Preferences already have two precedents: `ThemeService` (localStorage, applied pre-paint) and `FaithService` (envelope-encrypted, synced in the single `user_prefs` row).

## Goals / Non-Goals

**Goals:**
- One source of truth for every nav surface (desktop row, mobile bottom bar, Home cards, Account editor).
- Responsive Option-A pattern: desktop row + mobile bottom tab bar with a `More` sheet.
- Per-user choice of which pages show and their order, prioritising the bottom-bar tabs.
- Cross-device sync where the server never learns the layout, applied instantly on every load.
- A wider shell on large screens without harming reading measure. Accessible customization.

**Non-Goals:**
- A true "open the app to this page" default-route setting (nav prioritisation only for now).
- Drag-and-drop reordering (accessibility cost outweighs benefit at this scale).
- Reordering or hiding `Home`/`Account`.
- Widening prose columns to the full shell width.

## Decisions

- **Single `NavCatalog`** (`Models/NavItems.cs`) of `NavItem` records (key, href, labels, icon, blurb, requires-auth, fixed). Every surface derives from it, so adding a page lights up nav, bottom bar, Home cards, and the editor at once. Alternative — per-surface lists — was rejected as drift-prone (the pre-existing duplication is exactly what caused the Sleep item to differ between deployed and source).

- **Hybrid persistence** (`NavPrefsService`): localStorage plaintext cache + envelope-encrypted `user_prefs.nav_payload`, reconciled last-write-wins via a timestamp embedded in the payload. Rationale: nav is global chrome shown before unlock, but the DEK is wiped each load, so a pure-encrypted store (like Faith) would show default order until the user unlocks that session. Local cache gives instant application; the encrypted server copy gives private cross-device sync. Alternatives considered: pure-encrypted (rejected — poor UX for always-on chrome); localStorage-only (rejected — no cross-device sync); auto-unlocking the vault (rejected — breaks zero-knowledge).

- **Separate `nav_payload` column** rather than merging into Faith's `payload`. Avoids a read-modify-write race between two independent services on one JSON blob; PostgREST upsert with `resolution=merge-duplicates` writes only the provided columns, so Faith and Nav never clobber each other. Required making `payload` nullable so a nav-only row is valid.

- **↑/↓ + show-hide checkbox editor** in the Account page, reusing the existing signed-in settings surface next to Appearance/Faith. Keyboard-operable and screen-reader friendly.

- **Two width tiers** via a `.shell` component class (~900px) for chrome and dashboards; prose/forms keep `max-w-3xl`/`max-w-md`. Reading research (~66ch ideal) means widening prose to 900px would hurt legibility, so only the shell widens.

## Risks / Trade-offs

- Local plaintext cache exposes tab order to anyone with access to the user's own browser storage → accepted (matches the theme preference; the stated threat model is the server/DBA, which encryption addresses).
- New device shows default order until first unlock that session → inherent to zero-knowledge; documented in the Account copy and mitigated by caching after first sync.
- Last-write-wins can drop a concurrent edit from another device → acceptable for a single-user, low-frequency preference; no merge UI warranted.
- Bottom bar fits a limited number of tabs → overflow handled by the `More` sheet.

## Migration Plan

1. Apply migration `add_nav_payload_to_user_prefs`: add nullable, length-checked `nav_payload`; drop NOT NULL on `payload`. Additive and backward-compatible; existing rows and Faith writes are unaffected.
2. Ship client. Existing users start from defaults; any local customization syncs up on next unlock.
3. Rollback: `alter table user_prefs drop column nav_payload;` (the nullable `payload` change is harmless to leave in place).

## Open Questions

- Should a future iteration add a real "default landing page" route setting distinct from nav order?
- Should the bottom-bar tab count adapt to viewport width instead of a fixed count?
