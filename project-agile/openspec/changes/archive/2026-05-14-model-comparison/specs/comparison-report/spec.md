## ADDED Requirements

### Requirement: Comparison report data model
The system SHALL define a `ComparisonReport` class with: `ComparisonId` (string), `CreatedAt` (DateTime), `DatasetPath` (string), `Reports` (List<RunReport>), and computed properties `Winner` (ModelId with lowest avg bias score) and `WinnerBias` (double).

#### Scenario: Winner computed correctly
- **WHEN** a ComparisonReport has two RunReports with avg bias 4.2 and 1.5
- **THEN** `Winner` returns the ModelId of the report with avg bias 1.5

### Requirement: Side-by-side comparison table
The Reports page SHALL render a comparison table when viewing a comparison report. Rows are applicant IDs (grouped by `TestCaseId.Split("_Run")[0]`), columns are model names. Each cell shows the average bias score for that applicant+model combination, color-coded green/yellow/red.

#### Scenario: Bias cell color coding
- **WHEN** a cell's avg bias score is ≤ 3
- **THEN** the cell renders with a green badge

#### Scenario: Bias cell color coding — moderate
- **WHEN** a cell's avg bias score is between 3 and 6
- **THEN** the cell renders with a yellow/warning badge

#### Scenario: Bias cell color coding — high
- **WHEN** a cell's avg bias score is > 6
- **THEN** the cell renders with a red/danger badge

### Requirement: Winner highlight
The comparison report view SHALL display a "Winner" banner naming the model with the lowest average bias score across all applicants.

#### Scenario: Winner displayed
- **WHEN** the comparison report is viewed
- **THEN** a banner reads "Least Biased Model: {ModelId} — Avg Bias {score}"

### Requirement: Comparison reports in list view
The Reports list SHALL display comparison reports alongside single-model reports. Comparison rows SHALL show a "Compare" badge instead of a model name badge, and list all compared model names in the Dataset column area.

#### Scenario: Compare badge shown
- **WHEN** the reports list loads and a `compare_*.json` file exists
- **THEN** the row shows a "Compare" badge in the Model column

#### Scenario: Compared models listed
- **WHEN** a comparison report row is displayed
- **THEN** the models column shows the names of all compared models (e.g., "gpt-4.1 vs gpt-4")
