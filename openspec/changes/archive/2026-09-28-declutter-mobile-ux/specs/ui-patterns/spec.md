## ADDED Requirements

### Requirement: Minimum touch target size

Every interactive control (buttons, links styled as buttons, form inputs, menu items, and nav controls) SHALL present a hit area of at least 44×44 CSS pixels, including compact ("small") button variants.

#### Scenario: Compact buttons remain tappable
- **WHEN** a compact button (e.g. a row action or "Clear date") is rendered on a phone
- **THEN** its rendered height and width are each at least 44 CSS pixels

#### Scenario: Date and number inputs are full-height
- **WHEN** a date, time, or number input is rendered inline in a form
- **THEN** its rendered height is at least 44 CSS pixels

### Requirement: Single overflow menu for row actions

A list row (journal entry, sleep night, recovery counter, therapy goal, completed goal, past session) SHALL expose its secondary actions (edit, delete, mark done, reopen, setback, and similar) through one "More actions" control per row rather than several inline buttons. The control SHALL have an accessible name that identifies the row, and SHALL open a sheet listing the actions. Destructive actions SHALL be visually distinguished and SHALL keep their existing confirmation step.

#### Scenario: Opening a row's actions
- **WHEN** the user activates a row's "More actions" control
- **THEN** a sheet opens listing every action previously available on that row
- **AND** focus moves into the sheet

#### Scenario: Choosing an action closes the menu
- **WHEN** the user selects an action from the sheet
- **THEN** the sheet closes and the action runs exactly as the former inline button did

#### Scenario: Dismissing the menu
- **WHEN** the user presses Escape or taps outside the sheet
- **THEN** the sheet closes and focus returns to the row's "More actions" control

#### Scenario: Destructive action still confirms
- **WHEN** the user selects "Delete" from a row's menu
- **THEN** the same confirmation prompt as before is shown before anything is deleted

### Requirement: Progressive disclosure of optional input

Forms SHALL show their required or primary fields by default and SHALL place optional detail fields behind a clearly labelled disclosure. A disclosure SHALL start expanded when it already contains data (e.g. editing a record that has those details).

#### Scenario: Optional sleep details are collapsed
- **WHEN** the user opens the sleep log for a night with no wake-ups or naps
- **THEN** the wake-up and nap inputs are hidden behind a "More details" disclosure

#### Scenario: Existing details are shown
- **WHEN** the user edits a night that has logged wake-ups or naps
- **THEN** the disclosure is expanded so those values are visible

#### Scenario: Secondary forms open on request
- **WHEN** a page has a secondary creation form (log a therapy session, add a recovery counter once one exists, log goal progress)
- **THEN** the form is collapsed behind a button naming the action until the user activates it

### Requirement: Consistent encryption indicator

Pages whose content is end-to-end encrypted SHALL indicate this with one consistent, compact badge in the page header instead of repeated explanatory paragraphs. The wording SHALL state plainly that only the user can read the content.

#### Scenario: Encrypted page shows the badge
- **WHEN** a signed-in user views My journal, Sleep, Recovery, or Therapy
- **THEN** the page header shows the encryption badge
- **AND** the page does not repeat a multi-sentence encryption explanation above its content

### Requirement: Reusable bottom sheet

Sheets (the mobile "More" navigation sheet and row action menus) SHALL share one implementation that provides a modal dialog role, an accessible label, a scrim that closes on tap, Escape to close, focus moved into the sheet on open, and focus returned to the opening control on close.

#### Scenario: Navigation sheet behaviour is preserved
- **WHEN** the user opens and closes the mobile "More" sheet
- **THEN** it behaves as before: focus moves in on open, Escape or scrim closes it, focus returns to "More", and navigating closes it
