## ADDED Requirements

### Requirement: Create menu

A `Create` control, available on every page, SHALL open a sheet listing the actions for adding something new: "New journal entry" always, then, for signed-in users, the actions for each enabled tracker in tracker order: "Log sleep" (Sleep); "Log a therapy session" and "Add a goal" (Therapy); "Add a recovery counter" (Recovery). Signed-out visitors SHALL see "New journal entry" and an invitation to sign in to track sleep, recovery, and therapy. The sheet SHALL follow the shared bottom-sheet behaviour.

#### Scenario: Signed-in user sees tracker actions
- **WHEN** a signed-in user with all trackers on opens the create menu
- **THEN** it lists New journal entry, Log sleep, Add a recovery counter, Log a therapy session, and Add a goal, with tracker actions grouped in tracker order

#### Scenario: Hidden trackers are left out
- **WHEN** a signed-in user with Recovery turned off opens the create menu
- **THEN** "Add a recovery counter" is not listed

#### Scenario: Guest sees the journal action
- **WHEN** a signed-out visitor opens the create menu
- **THEN** "New journal entry" is listed with an invitation to sign in for the trackers

### Requirement: Actions open their form directly

Each create-menu action SHALL take the user straight to a form that's ready to fill in. "New journal entry" opens `/journal/new`, and "Log sleep" opens `/track/sleep` (where the log form leads). "Log a therapy session" (`/track/therapy?log=1`), "Add a goal" (`/track/therapy?goal=1`), and "Add a recovery counter" (`/track/recovery?add=1`) open the corresponding form expanded. The query flag SHALL then be removed from the address without adding a history entry, so the action can be repeated.

#### Scenario: Log a session from anywhere
- **WHEN** the user chooses "Log a therapy session" from the create menu on Today
- **THEN** the Therapy page opens with the session form expanded and the address reads `/track/therapy`

#### Scenario: Repeating an action
- **WHEN** the user is on `/track/recovery` and chooses "Add a recovery counter" again
- **THEN** the add-counter form opens
