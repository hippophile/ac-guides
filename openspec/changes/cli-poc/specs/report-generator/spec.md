## ADDED Requirements

### Requirement: Write JSON run report
The system SHALL serialise a `RunReport` to a JSON file at `reports/<run-id>.json` after every completed evaluation run.

#### Scenario: JSON report created
- **WHEN** `EvalRunner.RunAsync` completes
- **THEN** a valid JSON file exists at `reports/<run-id>.json` containing all `EvaluationResult` entries

### Requirement: Render HTML report from JSON
The system SHALL render a `RunReport` to an HTML file using a Scriban template, saving it at `reports/<run-id>.html`.

#### Scenario: HTML report generated
- **WHEN** `HtmlReportRenderer.RenderAsync` is called with a `RunReport`
- **THEN** an HTML file exists at `reports/<run-id>.html`

#### Scenario: HTML contains summary table
- **WHEN** the HTML report is opened
- **THEN** it contains a summary table with: run ID, model, date, total tests, passed, failed, average faithfulness, average relevancy

### Requirement: Report includes per-result detail
The HTML report SHALL include a detail section per test case showing: test ID, category, prompt (truncated), actual output (truncated), faithfulness score, relevancy score, bias score, pass/fail.

#### Scenario: Per-result rows present
- **WHEN** the HTML report contains 20 test cases
- **THEN** the detail table has 20 rows

### Requirement: Load JSON report from disk
The system SHALL deserialise a `RunReport` from a JSON file given a run ID or file path.

#### Scenario: Load saved report
- **WHEN** `RunReportWriter.LoadAsync("reports/<run-id>.json")` is called
- **THEN** a fully populated `RunReport` is returned
