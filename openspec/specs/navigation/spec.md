# navigation Specification

## Purpose
How people move around the app: a fixed set of primary destinations (Today, Journal, Track, Get help, Account) and a create menu, presented as a five-slot bottom bar on phones and a header row on desktop, with tracker selection as the only customisation and redirects for old routes.
## Requirements
### Requirement: Private, cross-device persistence

The navigation layout SHALL be persisted so it applies instantly on every load and syncs across a signed-in user's devices without the server learning the layout. A plaintext copy SHALL be cached in browser local storage for immediate application (and as the only store for guests). For signed-in users, an envelope-encrypted copy SHALL be stored server-side. The two SHALL be reconciled last-write-wins when the encryption vault is unlocked. The server SHALL only ever hold ciphertext of the layout.

#### Scenario: Change applies instantly and survives reload
- **WHEN** a user edits the layout
- **THEN** the navigation updates immediately
- **AND** the layout is restored from local cache on the next load without requiring unlock

#### Scenario: Signed-in edits sync encrypted
- **WHEN** a signed-in user with an unlocked vault edits the layout
- **THEN** an encrypted copy is written to the user's `user_prefs` row
- **AND** the server stores only ciphertext of the layout

#### Scenario: Another device adopts the synced layout
- **WHEN** a user unlocks the vault on a second device whose stored layout is older
- **THEN** the newer layout from the server is adopted and cached locally

#### Scenario: Layout is unreadable to the server
- **WHEN** the layout is stored server-side
- **THEN** no plaintext ordering or visibility information is present in the stored value

### Requirement: Fixed primary destinations

The system SHALL present a fixed set of primary destinations: `Today` (the home route), `Journal`, `Track`, `Get help`, and `Account`, plus a `Create` control that opens the create menu. These destinations SHALL NOT be hidden or reordered. For signed-out visitors, `Account` SHALL be labelled `Sign in`. `Journal` SHALL open the entry editor directly for signed-out visitors, since they have no saved entries.

#### Scenario: Guest sees the same shape
- **WHEN** a signed-out visitor views the navigation
- **THEN** `Today`, `Journal`, `Create`, `Track`, `Get help`, and `Sign in` are shown

#### Scenario: Signed-in user sees Account
- **WHEN** a signed-in user views the navigation
- **THEN** the same destinations are shown and the last one is labelled `Account`

#### Scenario: Guest Journal opens the editor
- **WHEN** a signed-out visitor activates `Journal`
- **THEN** they land on the new-entry editor rather than an empty list

### Requirement: Five-slot responsive navigation

The navigation SHALL adapt to viewport width. On phone viewports it SHALL render as a fixed bottom bar with exactly five slots: `Today`, `Journal`, `Create` (centre), `Track`, and `Account`. There SHALL be no overflow ("More") sheet. `Get help` SHALL be shown as an icon button in the header. On wide viewports, `Today`, `Journal`, and `Track` SHALL render as a horizontal row in the header, followed by a `New` button that opens the create menu, `Get help`, and `Account`. A destination SHALL show as current for its own route and every route beneath it (e.g. `Track` for `/track/sleep/review`). Content SHALL clear the fixed bar.

#### Scenario: Phone shows the five-slot bar
- **WHEN** the viewport is below the small breakpoint
- **THEN** the bottom bar shows `Today`, `Journal`, `Create`, `Track`, and `Account`, with no `More` control
- **AND** a `Get help` icon is shown in the header

#### Scenario: Desktop shows the header row
- **WHEN** the viewport is at least the small breakpoint
- **THEN** the header shows `Today`, `Journal`, `Track`, a `New` button, `Get help`, and `Account`, and the bottom bar is hidden

#### Scenario: Nested routes highlight their section
- **WHEN** the user is on `/track/sleep/review` or `/journal/new`
- **THEN** `Track` or `Journal` respectively is marked as the current page

### Requirement: Tracker selection and order

Users SHALL be able to choose which trackers (Sleep, Recovery, Therapy) they use and in what order, from a "Your trackers" section on the Account page. The chosen order SHALL determine the Track tab order, the order of tracker cards on Today, and the order of tracker actions in the create menu. Hidden trackers SHALL be omitted from all three, but SHALL stay reachable by URL. A reset SHALL restore the default order with every tracker on. Layouts saved before this change SHALL keep their relative tracker order and hidden trackers. Entries for non-tracker pages SHALL be ignored.

#### Scenario: Reordering changes every tracker surface
- **WHEN** the user moves Therapy above Sleep
- **THEN** Therapy appears before Sleep in the Track tabs, on Today, and in the create menu

#### Scenario: Hiding a tracker removes it everywhere
- **WHEN** the user turns Recovery off
- **THEN** Recovery no longer appears in the Track tabs, on Today, or in the create menu

#### Scenario: Reset restores defaults
- **WHEN** the user chooses to reset
- **THEN** every tracker is on, in the default order

#### Scenario: Old layouts migrate
- **WHEN** a layout saved by an earlier version lists `write`, `therapy`, `resources`, `sleep` with `recovery` hidden
- **THEN** the trackers load in the order Therapy, Sleep, Recovery with Recovery hidden

### Requirement: Feature discovery

Signed-out visitors SHALL see every feature on the Home (Today) route as a card, and cards for authentication-gated features SHALL invite them to sign in. Signed-in users SHALL discover features through the Track tabs, the create menu, and a "Choose what you track" link on Today that opens the tracker settings.

#### Scenario: Guest sees feature cards
- **WHEN** a signed-out visitor scrolls Home
- **THEN** Journal, Sleep, Recovery, Therapy, and Get help appear as cards, with the gated ones linking to sign in

#### Scenario: Signed-in user can find hidden trackers
- **WHEN** a signed-in user who has turned off a tracker views Today
- **THEN** a "Choose what you track" link opens the tracker settings on the Account page

### Requirement: Legacy routes redirect

Routes from the previous navigation SHALL redirect, replacing the history entry, to their new location: `/entries` → `/journal`, `/sleep` → `/track/sleep`, `/sleep/review` → `/track/sleep/review`, `/recovery` → `/track/recovery`, `/therapy` → `/track/therapy`, `/resources` → `/help`. `/signin` SHALL keep working as the Account page.

#### Scenario: Old bookmark still works
- **WHEN** a user opens `/therapy`
- **THEN** they land on `/track/therapy`, and Back does not return to `/therapy`

