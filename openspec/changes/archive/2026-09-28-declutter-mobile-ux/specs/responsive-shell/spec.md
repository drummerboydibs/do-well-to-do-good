## ADDED Requirements

### Requirement: Footer hidden on phones

Below the small breakpoint the site footer SHALL NOT be rendered, so page content ends just above the bottom navigation bar. The footer's content (privacy line, credits, and the "Suggest a feature" link) SHALL remain reachable from an About section on the Account / Sign-in page. Page content SHALL still clear the fixed bottom bar.

#### Scenario: Phone has no footer
- **WHEN** any page is viewed below the small breakpoint
- **THEN** the footer is not shown
- **AND** the last content on the page is not obscured by the bottom bar

#### Scenario: Footer links remain reachable
- **WHEN** a user on a phone opens the Account / Sign-in page
- **THEN** an About section offers the "Suggest a feature" link and the credits

#### Scenario: Desktop keeps the footer
- **WHEN** a page is viewed at or above the small breakpoint
- **THEN** the footer is shown as before
