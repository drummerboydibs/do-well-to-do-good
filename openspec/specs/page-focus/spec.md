# page-focus Specification

## Purpose
Content-priority rules for each page: summary widgets live only on Home, each page leads with its primary task, signed-in Home is a dashboard rather than a pitch, and a locked vault shows at most one unlock prompt per page.
## Requirements
### Requirement: Summary widgets live on Home only

The tracker status cards (last night's sleep, recovery counters, open goals), "Your journey" stats, and the daily thought (word of the day plus faith passage) SHALL be shown on Today (the home route) and SHALL NOT be repeated at the top of other pages. Tracker status cards SHALL appear only for enabled trackers, in tracker order. Full management of each remains on its own page. The faith passage and matched tip shown after saving or releasing a journal entry are unaffected.

#### Scenario: Write page has no summary widgets
- **WHEN** a signed-in, unlocked user with recovery counters, open goals, and a faith tradition opens the new-entry editor
- **THEN** none of the summary widgets appear on the page

#### Scenario: Therapy and Account have no recovery summary
- **WHEN** a signed-in user with recovery counters opens Therapy or Account
- **THEN** the recovery-counter summary is not shown
- **AND** Account does not show the "Your journey" stats

#### Scenario: Home shows the summaries
- **WHEN** a signed-in, unlocked user with every tracker on opens Today
- **THEN** their tracker cards, "Your journey" stats, and daily thought (including the faith passage if a tradition is selected) are shown

#### Scenario: Today orders tracker cards
- **WHEN** a signed-in, unlocked user with tracker order Therapy, Sleep and Recovery hidden opens Today
- **THEN** the open-goals card appears before the sleep card, no recovery card is shown, and the stats and daily thought follow

### Requirement: Write leads with the writing field

On the Write page the text box SHALL be the first form control, preceded only by the page heading and a single line of context. Naming a feeling (text input and Emotion Wheel) SHALL remain available as an optional step after the text box. Save, "Shout into the wind", and the post-save tip/passage behaviour SHALL be unchanged.

#### Scenario: Text box is above the fold on a phone
- **WHEN** a guest opens Write on a 375×812 viewport
- **THEN** the top of the text box is visible without scrolling

#### Scenario: Feeling remains optional and available
- **WHEN** the user writes an entry and names a feeling via the input or the wheel
- **THEN** the feeling is saved with the entry and used to match the tip exactly as before

### Requirement: Signed-in Home is a dashboard

For signed-in users, Today SHALL show a greeting with today's date and a "Start writing" action, then the unlock card (when locked), then the first-run tracker question (when applicable), then a status card for each enabled tracker, then "Your journey" stats and the daily thought, ending with a "Choose what you track" link. The marketing sections and the feature-card list SHALL be shown only to signed-out visitors.

#### Scenario: Signed-in user skips the pitch
- **WHEN** a signed-in user opens Today
- **THEN** the marketing sections and feature cards are not shown
- **AND** the tracker cards, stats, and daily thought are shown

#### Scenario: Guest still sees the pitch
- **WHEN** a signed-out visitor opens Home
- **THEN** the marketing sections and feature cards are shown

### Requirement: One unlock prompt per page

When a signed-in user's vault is locked, a page SHALL show at most one prompt to unlock. On Home this SHALL be a single unlock card that opens the unlock flow in place; the individual summary widgets SHALL NOT each show their own unlock nudge. Pages whose content requires the vault keep their inline unlock flow as their content.

#### Scenario: Locked Home shows one prompt
- **WHEN** a signed-in user with recovery counters and goals opens Home while locked
- **THEN** exactly one unlock prompt is shown
- **AND** after unlocking, the summary widgets appear without a page reload

#### Scenario: Locked feature page shows its own gate only
- **WHEN** a signed-in user opens Therapy while locked
- **THEN** only the page's unlock flow is shown, with no additional widget nudges

