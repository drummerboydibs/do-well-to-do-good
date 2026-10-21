## MODIFIED Requirements

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

### Requirement: Signed-in Home is a dashboard

For signed-in users, Today SHALL show a greeting with today's date and a "Start writing" action, then the unlock card (when locked), then the first-run tracker question (when applicable), then a status card for each enabled tracker, then "Your journey" stats and the daily thought, ending with a "Choose what you track" link. The marketing sections and the feature-card list SHALL be shown only to signed-out visitors.

#### Scenario: Signed-in user skips the pitch
- **WHEN** a signed-in user opens Today
- **THEN** the marketing sections and feature cards are not shown
- **AND** the tracker cards, stats, and daily thought are shown

#### Scenario: Guest still sees the pitch
- **WHEN** a signed-out visitor opens Home
- **THEN** the marketing sections and feature cards are shown
