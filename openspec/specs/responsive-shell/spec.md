# responsive-shell Specification

## Purpose
The layout shell around every page: content width tiers, mobile-first sizing, and what the header/footer chrome does at each breakpoint.
## Requirements
### Requirement: Content width tiers

The app SHALL use two content width tiers on large screens: a wide shell for chrome (header/footer) and dashboard/grid content, and a narrower reading column for prose and single-column forms so text keeps a comfortable measure. Layout SHALL remain mobile-first — on small screens content SHALL fill the viewport width within its padding.

#### Scenario: Dashboard content uses the wide shell
- **WHEN** a dashboard or grid page (e.g. Home, Resources) is viewed on a large screen
- **THEN** its content is constrained to the wide shell width (~900px) and centred
- **AND** the header and footer align to that same width

#### Scenario: Prose keeps a readable measure
- **WHEN** prose or a single-column form is viewed on a large screen
- **THEN** it is constrained to a narrower reading column (~720–768px) rather than the full shell width

#### Scenario: Mobile-first at small widths
- **WHEN** any page is viewed on a small screen
- **THEN** content fills the available width within its horizontal padding

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

