## ADDED Requirements

### Requirement: Summary widgets live on Home only

The recovery-counter summary, open-goals reminder, word of the day, and daily faith passage SHALL be shown on Home and SHALL NOT be repeated at the top of other pages. Full management of each remains on its own page (Recovery, Therapy, Account). The faith passage and matched tip shown after saving or releasing a journal entry are unaffected.

#### Scenario: Write page has no summary widgets
- **WHEN** a signed-in, unlocked user with recovery counters, open goals, and a faith tradition opens Write
- **THEN** none of the summary widgets appear on the page

#### Scenario: Therapy and Account have no recovery summary
- **WHEN** a signed-in user with recovery counters opens Therapy or Account
- **THEN** the recovery-counter summary is not shown

#### Scenario: Home shows the summaries
- **WHEN** a signed-in, unlocked user opens Home
- **THEN** their recovery counters, open goals, faith passage (if a tradition is selected), and word of the day are shown

### Requirement: Write leads with the writing field

On the Write page the text box SHALL be the first form control, preceded only by the page heading and a single line of context. Naming a feeling (text input and Emotion Wheel) SHALL remain available as an optional step after the text box. Save, "Shout into the wind", and the post-save tip/passage behaviour SHALL be unchanged.

#### Scenario: Text box is above the fold on a phone
- **WHEN** a guest opens Write on a 375×812 viewport
- **THEN** the top of the text box is visible without scrolling

#### Scenario: Feeling remains optional and available
- **WHEN** the user writes an entry and names a feeling via the input or the wheel
- **THEN** the feeling is saved with the entry and used to match the tip exactly as before

### Requirement: Signed-in Home is a dashboard

For signed-in users, Home SHALL show a compact greeting with primary actions, then the summary widgets, then the feature list. The marketing sections ("A calm place to come back to yourself" feature cards and the "Your words are yours" privacy pitch) SHALL be shown only to signed-out visitors. The feature list SHALL still link to every feature.

#### Scenario: Signed-in user skips the pitch
- **WHEN** a signed-in user opens Home
- **THEN** the marketing feature cards and privacy pitch are not shown
- **AND** every feature is still reachable from the feature list

#### Scenario: Guest still sees the pitch
- **WHEN** a signed-out visitor opens Home
- **THEN** the marketing feature cards and privacy pitch are shown

### Requirement: One unlock prompt per page

When a signed-in user's vault is locked, a page SHALL show at most one prompt to unlock. On Home this SHALL be a single unlock card that opens the unlock flow in place; the individual summary widgets SHALL NOT each show their own unlock nudge. Pages whose content requires the vault keep their inline unlock flow as their content.

#### Scenario: Locked Home shows one prompt
- **WHEN** a signed-in user with recovery counters and goals opens Home while locked
- **THEN** exactly one unlock prompt is shown
- **AND** after unlocking, the summary widgets appear without a page reload

#### Scenario: Locked feature page shows its own gate only
- **WHEN** a signed-in user opens Therapy while locked
- **THEN** only the page's unlock flow is shown, with no additional widget nudges
