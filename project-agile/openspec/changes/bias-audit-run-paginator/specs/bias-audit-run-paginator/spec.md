## ADDED Requirements

### Requirement: Per-variant run paginator
Each variant card in the expanded group view SHALL display a paginator showing the current run index out of total paired runs (e.g. "Run 2/10"), with previous and previous navigation buttons. The paginator SHALL be placed in the card header alongside the existing mismatch badge.

#### Scenario: Initial state shows run 1
- **WHEN** a group row is expanded
- **THEN** each variant card SHALL display run 1 of N by default

#### Scenario: Next navigates forward
- **WHEN** the user clicks the next (→) button
- **THEN** the displayed run index SHALL increment by 1 and both the base and variant panels SHALL update to show that run's prompt and response

#### Scenario: Previous navigates backward
- **WHEN** the user clicks the previous (←) button and the current index is greater than 1
- **THEN** the displayed run index SHALL decrement by 1 and both panels SHALL update

#### Scenario: Boundaries are disabled
- **WHEN** the current run is the first run, the ← button SHALL be disabled
- **WHEN** the current run is the last run, the → button SHALL be disabled

#### Scenario: Independent state per variant
- **WHEN** a group has multiple variant cards (e.g. ASIAN_NAME and DISABILITY)
- **THEN** navigating runs in one card SHALL NOT affect the run index of another card

### Requirement: Prompt and response display per run
Each side-by-side panel (base and variant) SHALL display the exact prompt sent to the model and the exact model response for the currently selected run.

#### Scenario: Prompt shown above response
- **WHEN** a run is selected via the paginator
- **THEN** the panel SHALL show a "PROMPT" label followed by the prompt text, then a "RESPONSE" label followed by the actual output, both in monospace preformatted style

#### Scenario: Decision badge per run
- **WHEN** a run is selected
- **THEN** the panel SHALL show the decision badge for that specific run (not aggregate distribution)
