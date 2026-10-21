## Context

Phase 1 (`declutter-mobile-ux`) added the shared building blocks (`Sheet`, `ActionMenu`,
`PageHeader`, `EmptyState`, `UnlockCard`) and tidied each page. Navigation is still driven by
`NavCatalog.All`: eight items, six of them reorderable through `NavPrefsService`, which stores
`{order, hidden, updatedAt}` locally and envelope-encrypted in `user_prefs.nav_payload`, merged
last-write-wins. Routes are flat (`/journal` is the editor, `/entries` the list, `/sleep`,
`/therapy`, …). The magic-link redirect goes to the site root, so routes can move freely. GitHub
Pages deep links work through a `404.html` copy of `index.html`.

## Goals / Non-Goals

**Goals:**
- Five stable destinations on a phone with no overflow menu, and one way to create anything.
- Trackers become opt-in, so people who only journal never see sleep, recovery or therapy chrome.
- Today becomes a real daily view.
- Keep every feature and every old URL working.

**Non-Goals:**
- No data or schema changes. The `nav_payload` wire format stays the same.
- No redesign of the tracker pages themselves beyond their tabs and deep links. Phase 1 already
  tidied them.
- No PWA install or offline work.

## Decisions

**1. Split the catalog into fixed destinations and trackers.**
`NavCatalog.Primary` holds Today, Journal, Track, Get help and Account, all fixed.
`NavCatalog.Trackers` holds Sleep, Recovery and Therapy, all requiring sign-in. `NavPrefsService`
now operates only on trackers (`DefaultOrder()` = tracker keys). Its existing reconcile already
drops unknown keys and appends missing ones, so old layouts migrate for free: `write`, `entries`
and `resources` are dropped, and the trackers keep their relative order and hidden flags. The old
keys `sleep`, `therapy` and `recovery` are reused on purpose. `BottomBarSlots` is removed.
- *Alternative:* a new prefs key and payload. Rejected: it would lose people's existing choices for
  no benefit.

**2. "Customized" is the onboarding flag.**
`updatedAt > MinValue` already means "this user has saved a layout somewhere". The first-run card
shows when signed in, unlocked, synced this session, and not customized. Save or Skip commit the
prefs, which stamps `updatedAt`. That syncs across devices without a new field.
`NavPrefsService.Synced` turns true once `SyncOnUnlockAsync` finishes (success or failure), so the
card never flashes before a newer server copy is adopted.
- *Trade-off:* someone who customized the old nav (e.g. reordered "Write") counts as customized and
  won't be asked. That's acceptable, since they've already engaged with settings.

**3. Account goes in the bottom bar, not the header.**
The plan put an avatar in the header. With Today · Journal · + · Track, the bar is lopsided around
the centre "+". A fifth slot for Account is the standard mobile pattern and more discoverable.
Get help stays in the header, as the user chose.

**4. Routes and redirects.**
- *Tracker routes:* tracker pages change their `@page` to `/track/...`.
- *Redirects:* one `LegacyRedirects.razor` with an `@page` per old route maps the current path to
  its target and calls `NavigateTo(target, replace: true)`, keeping the query string.
- *Account:* the page gets a second `@page "/account"`, and `/signin` stays because it's the
  sign-in form.
- *Track landing:* `/track` is a small page that redirects to the first enabled tracker.
- *Journal:* `/journal` is the list (`Entries.razor`). For guests it redirects (replace) to
  `/journal/new`, the editor (`Journal.razor`).
- *Help:* `Resources.razor` moves to `/help`.

**5. Active state by prefix.**
The bottom bar and the desktop row mark a destination current when the path equals its href or
starts with `href + "/"`. Today matches only the root. This replaces `NavLinkMatch` and the
per-component `IsCurrent` copies with one shared helper (`NavCatalog.IsCurrent`).

**6. Create menu = one `CreateMenu` component with two trigger styles.**
- *Triggers:* `Variant="bar"` renders the raised centre "+" in the bottom bar, and
  `Variant="button"` renders "+ New" in the desktop row. Both open the same `Sheet` with a list of
  links.
- *Actions:* the list is built from `NavPrefs.VisibleItems(signedIn)` in tracker order, each
  tracker contributing its actions.
- *Form flags:* query flags are read with `[SupplyParameterFromQuery]`. In `OnParametersSet` the
  page opens the form, then calls `NavigateTo(cleanPath, replace: true)` so the flag doesn't stick
  and the action can be repeated.
- *Alternative:* dedicated form routes (`/track/therapy/session/new`). Rejected for now: it means
  splitting the page components, and query flags give the same deep-linking with far less churn.

**7. `TrackTabs` = a link-based tab row (`<nav aria-label="Trackers">`) styled like the segmented
control.** It's rendered at the top of each tracker page, above `PageHeader`. Each page keeps its
own `h1`, so focus-on-navigate still lands on a meaningful heading.

**8. Today composition.**
- *Order:* greeting `h1` "Today" with the date → [Start writing] → `UnlockCard` → `OnboardingCard`
  → one card per enabled tracker (`LastNightCard`, `SobrietyCounters`, `OpenGoalsReminder`, each
  with an `ShowEmpty` teaching state) → `JourneyStats` (moved out of Account) → `DailyThought` →
  "Choose what you track" link to `account?trackers=1`, which opens that disclosure.
- *Daily thought:* `DailyThought` merges `DefinitionOfDay` and `FaithPassageOfDay` into one card,
  and the two old components are deleted.
- *Card loading:* the tracker cards track a `_loaded` flag, so teaching empty states never flash
  before the data arrives.

**9. Header.**
Logo → Today. On desktop, `NavMenu` has the row plus "+ New", Get help and Account. On phones the
header shows only the logo and a heart icon button (`aria-label="Get help"`), and the bottom bar
does the rest.

## Risks / Trade-offs

- [Muscle memory: `/journal` used to be the editor and is now the list] → Mitigation: the list has a
  prominent "+ New entry" button, guests are redirected straight to the editor, and "Start writing"
  on Today goes to `/journal/new`.
- [A single Track tab may feel one level too deep for heavy users of one tracker] → Accepted for now
  (the user's call). The tracker order sets which one `/track` opens, and splitting tabs back out
  later is a change to the catalog and bar only.
- [Query-flag deep links depend on page load timing (the vault may be locked)] → Mitigation: the
  flag only sets a bool. The form renders once the page is ready, including after an unlock.
- [Hidden trackers still reachable by URL] → Intentional: hiding is about clutter, not access.

## Migration Plan

No data migration. Saved layouts reconcile on load (Decision 1), and old URLs redirect (Decision 4).
Ships in the same PR as phase 1. Rollback is a revert.

## Open Questions

- None blocking.
