## ADDED Requirements

### Requirement: Compare Mode toggle
The Test Runner SHALL include a "Compare Mode" toggle switch above the model selector. When off, the existing single-model dropdown is shown. When on, it is replaced with a checkbox group.

#### Scenario: Toggle switches UI
- **WHEN** Compare Mode toggle is switched on
- **THEN** the single model `<select>` is hidden and a checkbox list appears with the same model options

#### Scenario: Toggle reverts selection
- **WHEN** Compare Mode is switched off after models were checked
- **THEN** the selection resets to the default single model and checked state is cleared

### Requirement: Start button routes correctly
When Compare Mode is active, the "Start Evaluation" button SHALL trigger the comparison run path instead of the single-model run path, and on completion SHALL navigate to `/reports/compare/{id}`.

#### Scenario: Navigation after compare run
- **WHEN** a comparison run completes
- **THEN** the browser navigates to `/reports/compare/{ComparisonId}`
