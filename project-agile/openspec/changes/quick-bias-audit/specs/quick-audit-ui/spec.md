## ADDED Requirements

### Requirement: Role description input
The page SHALL provide a single text input where the user enters a plain-language description of the AI role being audited (e.g., "loan writer-admin").

#### Scenario: Input accepted
- **WHEN** user types any non-empty text into the role description field
- **THEN** the Run button becomes enabled

#### Scenario: Empty input blocked
- **WHEN** the role description field is empty
- **THEN** the Run button SHALL remain disabled

### Requirement: Bias dimension checkboxes
The page SHALL display a checkbox for each of the 11 supported bias dimensions. All checkboxes SHALL be checked by default.

#### Scenario: Select all / Clear all
- **WHEN** user clicks "Select All"
- **THEN** all dimension checkboxes SHALL be checked
- **WHEN** user clicks "Clear All"
- **THEN** all dimension checkboxes SHALL be unchecked

#### Scenario: At least one dimension required
- **WHEN** all checkboxes are unchecked
- **THEN** the Run button SHALL be disabled

### Requirement: Run button triggers audit
The page SHALL have a prominent "Generate & Run Bias Audit" button that, when clicked, starts the quick audit flow.

#### Scenario: Button triggers generation
- **WHEN** user clicks the Run button with a non-empty role description and at least one dimension selected
- **THEN** the UI SHALL transition to the live progress view

### Requirement: Sidebar navigation entry
The Blazor sidebar SHALL include a "Quick Audit" nav item linking to `/quick-audit`.

#### Scenario: Nav item visible
- **WHEN** the user opens the application
- **THEN** "Quick Audit" SHALL appear in the sidebar navigation
