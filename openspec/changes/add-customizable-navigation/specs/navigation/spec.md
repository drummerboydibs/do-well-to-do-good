## ADDED Requirements

### Requirement: Primary navigation destinations

The system SHALL present a defined set of primary destinations drawn from a single navigation catalog. `Home` and `Account` SHALL be fixed structural anchors that always appear and cannot be hidden or reordered. Destinations marked as requiring authentication SHALL appear only to signed-in users.

#### Scenario: Guest sees only public destinations
- **WHEN** a signed-out visitor views the navigation
- **THEN** only `Home`, public destinations (e.g. Write, Resources), and the `Sign in` entry are shown
- **AND** authentication-gated destinations (My journal, Sleep, Therapy, Recovery) are omitted

#### Scenario: Signed-in user sees gated destinations
- **WHEN** a signed-in user views the navigation
- **THEN** the authentication-gated destinations are shown alongside the public ones
- **AND** the `Sign in` entry is labelled `Account`

### Requirement: Responsive presentation

The navigation SHALL adapt to viewport width. On wide (desktop) viewports it SHALL render as a horizontal row. On narrow (phone) viewports it SHALL render as a fixed bottom tab bar showing `Home`, the first few visible destinations as tabs, and a `More` control; the remaining destinations plus `Account` SHALL be reachable from a `More` sheet. Content SHALL clear the fixed bar so nothing is obscured.

#### Scenario: Desktop shows a horizontal row
- **WHEN** the viewport is at least the small breakpoint
- **THEN** the primary destinations render as a horizontal pill row and the bottom bar is not shown

#### Scenario: Phone shows a bottom tab bar
- **WHEN** the viewport is below the small breakpoint
- **THEN** a fixed bottom bar shows `Home`, up to the configured number of visible destinations, and a `More` control
- **AND** the desktop row is hidden

#### Scenario: More sheet reveals the remainder
- **WHEN** the user activates `More` on the bottom bar
- **THEN** a sheet opens listing the destinations that did not fit as tabs, plus `Account`
- **AND** navigating to any item or dismissing the sheet closes it

### Requirement: User-customizable layout

Signed-in users SHALL be able to choose which reorderable destinations appear and in what order. The chosen order SHALL determine both the desktop row sequence and which destinations become bottom-bar tabs versus `More` overflow. A reset SHALL restore the default order with all destinations visible.

#### Scenario: Reordering changes both presentations
- **WHEN** the user moves a destination earlier in the order
- **THEN** it appears earlier in the desktop row and is more likely to be a bottom-bar tab

#### Scenario: Hiding removes a destination from navigation
- **WHEN** the user hides a reorderable destination
- **THEN** it no longer appears in the desktop row, bottom bar, or More sheet

#### Scenario: Structural anchors are protected
- **WHEN** the user opens the layout editor
- **THEN** `Home` and `Account` are not offered for hiding or reordering

#### Scenario: Reset restores defaults
- **WHEN** the user chooses to reset the layout
- **THEN** the default order is restored and every reorderable destination is visible

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

### Requirement: Feature discovery on Home

The Home page SHALL surface every feature as a card so users can discover the whole app by scrolling. Cards for authentication-gated features SHALL, for signed-out visitors, invite them to sign in rather than link into a gated page.

#### Scenario: Signed-in user can jump to any feature
- **WHEN** a signed-in user scrolls Home
- **THEN** each feature is shown as a card linking to that feature

#### Scenario: Guest is invited to unlock gated features
- **WHEN** a signed-out visitor scrolls Home
- **THEN** cards for gated features indicate they require signing in and link to sign in
