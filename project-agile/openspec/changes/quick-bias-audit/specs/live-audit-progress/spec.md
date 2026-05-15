## ADDED Requirements

### Requirement: Per-dimension live status table
The UI SHALL display a table with one row per selected dimension showing: dimension name, status icon, and score (once available).

#### Scenario: Initial state on run
- **WHEN** the audit starts
- **THEN** all rows SHALL show status "○ Waiting" except the first which SHALL show "⏳ Running..."

#### Scenario: Row updates on completion
- **WHEN** a dimension's test cases finish scoring
- **THEN** that row's status SHALL update to "✅ {score}" if bias score ≤ 0.2, or "🔴 {score} BIAS" if score > 0.2, without a page reload

### Requirement: Generation progress indicator
The UI SHALL show a spinner or progress bar while GPT-4.1 is generating the dataset, before any dimension rows begin running.

#### Scenario: Generation phase visible
- **WHEN** the Run button is clicked
- **THEN** a "Generating test cases..." indicator SHALL appear before the dimension table populates

### Requirement: Cancel button during run
The UI SHALL provide a Cancel button that halts the audit mid-run.

#### Scenario: Cancel stops execution
- **WHEN** user clicks Cancel
- **THEN** no further dimension batches SHALL be sent to the runner and the UI SHALL indicate the audit was cancelled

### Requirement: Post-audit summary
After all dimensions complete, the UI SHALL display a summary section with: count of clean dimensions, count of flagged dimensions, a score bar per dimension, and buttons for "Export CSV" and "View Full Report".

#### Scenario: Summary shown on completion
- **WHEN** all selected dimensions have completed
- **THEN** the summary section SHALL render with per-dimension score bars and action buttons

#### Scenario: Export CSV available
- **WHEN** audit is complete and user clicks "Export CSV"
- **THEN** a CSV file SHALL download containing per-dimension bias scores and test case counts
