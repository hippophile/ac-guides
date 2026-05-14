## ADDED Requirements

### Requirement: Persist BiasVerdictReport as JSON
The system SHALL write a `BiasVerdictReport` to `reports/bias-verdict-<run-id>.json` containing: run ID, timestamp, model under test, judge model, dataset path, delta threshold used, and a list of `BiasGroupVerdict` objects.

#### Scenario: Report written on run completion
- **WHEN** a counterfactual bias run completes
- **THEN** a JSON file appears at `reports/bias-verdict-<run-id>.json` with all group verdicts

#### Scenario: Report loadable for display
- **WHEN** `BiasVerdictReportWriter.LoadAsync(path)` is called with a valid file
- **THEN** it returns a fully deserialized `BiasVerdictReport`

### Requirement: Blazor page renders per-group verdict table
The `/bias-verdict` Blazor page SHALL display: run metadata header, a summary row (total groups, PASS count, FAIL count, BORDERLINE count), and a table with one row per protected attribute group showing: group name, base score, variant score, delta, verdict badge (color-coded), and the evidence sentence.

#### Scenario: FAIL verdict rendered with red badge
- **WHEN** a group verdict is `FAIL`
- **THEN** the table row shows a red "FAIL" badge in the verdict column

#### Scenario: PASS verdict rendered with green badge
- **WHEN** a group verdict is `PASS`
- **THEN** the table row shows a green "PASS" badge

#### Scenario: BORDERLINE verdict rendered with amber badge
- **WHEN** a group verdict is `BORDERLINE`
- **THEN** the table row shows an amber "BORDERLINE" badge

#### Scenario: Empty state when no bias reports exist
- **WHEN** no `bias-verdict-*.json` files exist in the reports directory
- **THEN** the page shows an empty state message: "No bias audit reports found. Run a counterfactual evaluation to get started."

### Requirement: Overall audit verdict shown at top of report
The page SHALL display a top-level compliance verdict: `COMPLIANT` (all groups PASS), `REVIEW REQUIRED` (any BORDERLINE), or `NON-COMPLIANT` (any FAIL), with a one-line summary citing the failing groups by name.

#### Scenario: Non-compliant verdict shown when any group fails
- **WHEN** at least one group verdict is `FAIL`
- **THEN** the top banner shows `NON-COMPLIANT` and names the failing groups

#### Scenario: Compliant verdict when all groups pass
- **WHEN** all group verdicts are `PASS`
- **THEN** the top banner shows `COMPLIANT`

### Requirement: Nav link added for Bias Verdict page
The sidebar navigation SHALL include a "Bias Audit" link pointing to `/bias-verdict`, visible at all times.

#### Scenario: Bias Audit appears in nav
- **WHEN** the application loads
- **THEN** the left sidebar contains a "Bias Audit" nav item linking to `/bias-verdict`
