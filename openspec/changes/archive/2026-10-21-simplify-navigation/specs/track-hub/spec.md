## ADDED Requirements

### Requirement: Tracker tabs

Every tracker page (Sleep, Sleep review, Recovery, Therapy) SHALL show a tab row at the top listing the user's enabled trackers in their chosen order, whenever two or more trackers are enabled (with one, there is nothing to switch between). The tab for the current tracker SHALL be marked current. A tracker the user has turned off SHALL NOT appear as a tab, but its page SHALL still render when reached by URL.

#### Scenario: Switching trackers
- **WHEN** a signed-in user on `/track/sleep` activates the Recovery tab
- **THEN** they land on `/track/recovery` and the Recovery tab is marked current

#### Scenario: Sleep review belongs to the Sleep tab
- **WHEN** the user is on `/track/sleep/review`
- **THEN** the Sleep tab is marked current

### Requirement: Track landing

Opening `/track` SHALL send a signed-in user to their first enabled tracker, replacing the history entry. If every tracker is off, it SHALL explain this and link to the tracker settings. A signed-out visitor SHALL see what each tracker does and an invitation to sign in.

#### Scenario: Lands on the first tracker
- **WHEN** a signed-in user whose tracker order starts with Therapy opens `/track`
- **THEN** they land on `/track/therapy`

#### Scenario: All trackers off
- **WHEN** a signed-in user with every tracker turned off opens `/track`
- **THEN** a message explains that no trackers are on and links to choose them

#### Scenario: Guest sees what's inside
- **WHEN** a signed-out visitor opens `/track`
- **THEN** Sleep, Recovery, and Therapy are described with an invitation to sign in
