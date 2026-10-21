## ADDED Requirements

### Requirement: First-run tracker question

A signed-in user with an unlocked vault who has never saved a tracker choice (on any device) SHALL see a "What would you like support with?" card on Today. The card lists Sleep, Recovery, and Therapy, all on by default. Saving SHALL store the choice using the tracker preferences, and "Skip for now" SHALL keep every tracker on. Either way the card SHALL NOT appear again. The card SHALL wait until the encrypted preferences have been synced on unlock, so a choice made on another device is respected.

#### Scenario: New user chooses trackers
- **WHEN** a new user unticks Recovery and saves
- **THEN** Recovery is hidden from Track, Today, and the create menu, and the card disappears

#### Scenario: Skipping keeps everything on
- **WHEN** the user chooses "Skip for now"
- **THEN** all trackers stay on and the card does not appear again

#### Scenario: Choice made elsewhere is respected
- **WHEN** a user who chose trackers on another device unlocks on this one
- **THEN** the card is not shown

### Requirement: Teaching empty states

When a tracker the user has turned on has no data yet, its Today card SHALL say what it does and offer a direct action to begin (e.g. "Add a counter", "Add a goal", "Log last night"). The Journal list's empty state SHALL offer to write the first entry.

#### Scenario: Empty recovery tracker on Today
- **WHEN** a user with Recovery on and no counters views Today
- **THEN** the Recovery card explains counters and links to add one

#### Scenario: No sleep logged yet
- **WHEN** a user with Sleep on has not logged last night
- **THEN** the Sleep card offers to log last night
