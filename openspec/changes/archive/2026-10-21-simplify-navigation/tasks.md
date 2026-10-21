## 1. Navigation model

- [x] 1.1 Split `NavCatalog` into `Primary` destinations and `Trackers`; add a shared prefix-based `IsCurrent` helper; add heart/plus/track icons
- [x] 1.2 Narrow `NavPrefsService` to trackers (drop `BottomBarSlots`), add `IsCustomized`, `Synced`, `SetTrackersAsync`, `ConfirmAsync`
- [x] 1.3 Update `NavPrefsServiceTests` for the tracker catalog, legacy-layout migration, and the new methods' pure helpers

## 2. Shell & routes

- [x] 2.1 `CreateMenu` component (bar + button variants) with tracker-ordered actions
- [x] 2.2 New `BottomNav` (Today · Journal · + · Track · Account) and desktop `NavMenu` (row + New + Get help + Account); header Get-help icon on phones
- [x] 2.3 Move routes: `/journal` list, `/journal/new` editor, `/track/*`, `/help`, `/account`; add `LegacyRedirects` and the `/track` landing
- [x] 2.4 `TrackTabs` on Sleep, Sleep review, Recovery, Therapy; update every internal link to the new routes
- [x] 2.5 Query-flag deep links: Therapy `?log=1` / `?goal=1`, Recovery `?add=1`

## 3. Today & Account

- [x] 3.1 `JourneyStats` (moved from Account), `DailyThought` (replaces DefinitionOfDay + FaithPassageOfDay), `LastNightCard`
- [x] 3.2 Teaching empty states for the Today tracker cards (`ShowEmpty`)
- [x] 3.3 `OnboardingCard` (first-run tracker question)
- [x] 3.4 Rebuild signed-in Today; guest Home explore cards from the new catalog
- [x] 3.5 Account: "Your trackers" editor (opens via `?trackers=1`), stats removed

## 4. Verify & ship

- [x] 4.1 `dotnet build` and `dotnet test` pass
- [x] 4.2 Browser check at 375×812 and desktop: bar, create menu, Track tabs/landing, redirects, Today (guest + signed-in locked), no horizontal scroll (unlocked-only views — tracker cards with data, onboarding card — left for manual check)
- [x] 4.3 Archive on the branch; update PR #58
