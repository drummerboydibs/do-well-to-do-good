## ADDED Requirements

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
