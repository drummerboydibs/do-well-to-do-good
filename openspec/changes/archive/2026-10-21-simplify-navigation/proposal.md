## Why

Phase 1 tidied each page, but the app's shape is still a list of eight peer destinations. On a
phone, half of them hide behind "More". Writing and reading the journal are separate places, the
trackers each live on their own page, and adding something new means first finding the right page.
Phases 2 and 3 of the mobile-first refactor give the app a small, stable shape: a day view, a
journal, a tracker hub, and one place to create anything. They also help each person switch on only
the trackers they actually use.

## What Changes

- **BREAKING (routes):** a new information architecture. Old URLs redirect to their new homes.
  - **Today** (`/`): the signed-in dashboard. Guests still get the landing page.
  - **Journal** (`/journal`): merges "Write" and "My journal". It lists saved entries, and a new
    entry is written at `/journal/new`. Guests go straight to the editor.
  - **Track** (`/track`): one hub with Sleep, Recovery and Therapy tabs (`/track/sleep`,
    `/track/sleep/review`, `/track/recovery`, `/track/therapy`). Only the trackers you use are shown.
  - **Get help** (`/help`): Resources and crisis lines, always one tap away from a heart icon in the
    header.
  - **Account** (`/account`; `/signin` still works).
  - Redirects: `/entries`, `/sleep`, `/sleep/review`, `/recovery`, `/therapy` and `/resources` go to
    their new homes.
- **Bottom bar (phones):** Today · Journal · **+** · Track · Account. Five fixed slots, so the "More"
  sheet goes away. Desktop gets the same destinations in its top row, plus a "New" button, Get help
  and Account.
- **Create menu (+):** a single sheet listing every "add" action: new journal entry, log sleep, log
  a therapy session, add a goal, add a recovery counter. Tracker actions follow the trackers you
  use. Each opens the right form directly through a link or query string (`?log=1`, `?goal=1`,
  `?add=1`).
- **BREAKING (customization):** the navigation editor becomes **"Your trackers"**. You choose which
  of Sleep, Recovery and Therapy you use and in what order. The order drives the Track tabs, the
  Today cards and the create menu. It uses the same encrypted, cross-device storage, and existing
  saved layouts carry over their tracker order and hidden state.
- **Today dashboard:**
  - A greeting and date, one "Start writing" action, and the single unlock card.
  - A status card for each tracker you use: last night's sleep, recovery streaks, open goals.
  - "Your journey" stats, moved from Account.
  - One combined "Daily thought" card (word of the day plus faith passage).
  - A link to choose your trackers.
- **First-run question:** "What would you like support with?" appears once on Today and switches
  trackers on or off.
- **Teaching empty states:** an empty tracker card on Today or an empty list says exactly what to
  tap to begin.

## Capabilities

### New Capabilities
- `track-hub`: the Track destination: tabs for each tracker you use, a redirect to the first one,
  and the guest and "all trackers off" states.
- `quick-create`: the create menu and the deep links that open each form directly.
- `onboarding`: the first-run tracker question and teaching empty states.

### Modified Capabilities
- `navigation`: primary destinations, responsive presentation (five-slot bar, no "More" sheet,
  header help), customization narrowed to trackers, legacy redirects, and feature discovery.
- `page-focus`: the signed-in Today dashboard replaces "Signed-in Home is a dashboard", and the
  summary widgets requirement now covers Today's per-tracker cards.

## Impact

- **Code:** `Models/NavItems.cs` (catalog split into primary destinations and trackers),
  `Services/NavPrefsService.cs` (trackers only, plus a "customized" flag), `Layout/*` (new bar,
  header and desktop row), all pages' `@page` routes, `Pages/Home.razor`, `Pages/Signin.razor`, and
  new components (`CreateMenu`, `TrackTabs`, `LastNightCard`, `JourneyStats`, `DailyThought`,
  `OnboardingCard`) plus a legacy-redirect page.
- **Tests:** `NavPrefsServiceTests` is updated for the tracker catalog and saved-layout migration.
- **Data:** no schema change. `user_prefs.nav_payload` keeps its wire format, and unknown keys from
  older layouts are dropped on load, as before.
