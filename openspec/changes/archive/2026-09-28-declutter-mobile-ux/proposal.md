## Why

The app's features have grown faster than its layout, and on a phone it now feels cluttered. The same
summary widgets (recovery streaks, open goals, word of the day, faith passage) repeat at the top of
up to four pages. On Write, the text box starts below the first screen. Signed-in Home still shows
the marketing pitch, and rows carry two to four small ghost buttons each (54 places shrink buttons
below a comfortable touch size). This change is phases 0 and 1 of a mobile-first UX refactor. It
tidies each page in place without changing navigation, so it can ship before the larger navigation
redesign (phase 2).

## What Changes

- **Shared UI building blocks**: a reusable bottom `Sheet` (moved out of `BottomNav`), an `ActionMenu`
  ("⋯" button → sheet of row actions), `PageHeader`, `EmptyState`, and an `EncryptedBadge` that
  replaces the per-page encryption paragraphs.
- **Touch targets**: every interactive control is at least 44×44 CSS px. A new `.btn-sm` size
  replaces the `!min-h-0 …` overrides.
- **One home per widget**: recovery counters, open goals, word of the day and the daily faith passage
  appear only on Home. They're removed from Write, Therapy and Account.
- **Write leads with writing**: the text box comes first and the feeling field follows as an optional
  step. Intro copy shrinks to one line.
- **Signed-in Home is a dashboard, not a pitch**: a compact greeting and primary actions, then the
  status widgets, then a compact feature list. The "Features" and "Privacy" marketing sections show
  to guests only.
- **Row actions in one menu**: Edit / Delete / Mark done / Reopen / "I had a setback" move into a
  single ⋯ menu per row on My journal, Sleep, Recovery and Therapy.
- **Progressive disclosure**: optional inputs sit behind a disclosure. That covers wake-ups and naps
  on Sleep, goal drafts and the log-session form on Therapy, the add-counter form on Recovery once
  counters exist, per-goal progress logging, and the nav editor on Account.
- **One unlock prompt per page**: the widgets' separate "Unlock to see…" nudges become a single
  unlock card on Home. The unlock card opens the existing vault flow in a sheet.
- **Footer on phones**: the site footer is hidden below the small breakpoint. Its content ("Suggest a
  feature", credits, privacy line) moves to an About section on the Account/Sign-in page.
- No functionality is removed. Every action, field and page stays reachable. Navigation structure is
  unchanged.

## Capabilities

### New Capabilities
- `ui-patterns`: shared interaction rules: minimum touch targets, a single overflow menu for row
  actions, progressive disclosure for optional fields, reusable sheets, and a consistent encryption
  indicator.
- `page-focus`: content-priority rules per page: summary widgets live only on Home, each page leads
  with its primary task, signed-in Home omits marketing, and there's one unlock prompt per page.

### Modified Capabilities
- `responsive-shell`: adds a requirement that the footer is not shown on phones and its links stay
  reachable from the Account page.

## Impact

- **Code**: `Layout/MainLayout.razor`, `Layout/BottomNav.razor`, all pages under `Pages/`, the
  summary widgets under `Components/`, new components (`Sheet`, `ActionMenu`, `PageHeader`,
  `EmptyState`, `EncryptedBadge`, `UnlockCard`), and `Styles/tailwind.css`.
- **No data, API, schema or encryption changes.** Nav preferences, routes and stored payloads are
  untouched.
- **Accessibility**: the sheets keep the existing focus-trap/return-focus behaviour and Escape to
  close, and the ⋯ menus get accessible names per row.
