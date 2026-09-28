## 1. Primitives & styles

- [x] 1.1 Add `.btn-sm` (≥44px), grow `.nav-ctl` to 2.75rem, cap `.sheet` width on `sm+`, add action-sheet item styles; move bottom-bar clearance to the layout root
- [x] 1.2 Create `Components/Sheet.razor` (dialog role, label, scrim, Escape, focus-in) and refactor `BottomNav` to use it
- [x] 1.3 Create `Components/ActionMenu.razor` (+ `MenuAction` record) — ⋯ button, sheet of actions, close-then-run, focus return
- [x] 1.4 Create `PageHeader`, `EncryptedBadge`, `EmptyState` components
- [x] 1.5 Create `UnlockCard` (VaultGate in a Sheet) and drop the per-widget unlock nudges from `SobrietyCounters` and `OpenGoalsReminder`

## 2. Page declutter

- [x] 2.1 Write: remove summary widgets; textarea first, optional feeling step after; trim intro copy
- [x] 2.2 Home: compact signed-in hero, UnlockCard + widgets, compact feature list; marketing sections guest-only
- [x] 2.3 My journal: PageHeader + badge, EmptyState, row actions → ActionMenu
- [x] 2.4 Sleep: PageHeader + badge, wake-ups/naps under "More details" (open when data exists), row actions → ActionMenu
- [x] 2.5 Sleep review: PageHeader + badge, 44px week/month arrows
- [x] 2.6 Recovery: PageHeader + badge, row actions → ActionMenu, add-counter form collapsed once counters exist, EmptyState
- [x] 2.7 Therapy: remove SobrietyCounters; PageHeader with "Log a session" toggle; goals first; goal/completed/session actions → ActionMenu; progress history + "Log progress" disclosure; goal drafts start empty
- [x] 2.8 Account: remove SobrietyCounters and redundant CTAs; nav editor in a disclosure; About section with footer content
- [x] 2.9 Footer hidden below `sm`; sweep remaining `!min-h-0` overrides to `.btn-sm` / full-height inputs

## 3. Verify & ship

- [x] 3.1 `dotnet build` and `dotnet test` pass
- [x] 3.2 Check guest and signed-in pages at 375×812 and desktop (light + dark): no horizontal scroll, Write textarea above the fold, menus/sheets keyboard-operable (signed-in verified in locked state; unlocked row menus/disclosures left for manual check)
- [x] 3.3 Archive the change on the branch and open one PR
