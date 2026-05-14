## ADDED Requirements

### Requirement: CSV Generation
The system SHALL provide a `ReportCsvExporter.GenerateBiasVerdictCsv(BiasVerdictReport)` static method that returns a UTF-8 CSV string with BOM. Each row represents one `EvaluationResult` from `RawResults`. Columns: Group, Variant, Decision, BiasScore, Verdict, JudgeReasoning, Prompt, AIResponse. All fields MUST be RFC 4180 quoted.

### Requirement: Export Button
The Bias Audit detail page SHALL display an "Export CSV" button alongside the existing "Download Report" button. The button SHALL be disabled with tooltip "No raw data" when `RawResults` is empty.

### Requirement: Client Download
Clicking "Export CSV" SHALL trigger a browser file download named `agile-bias-{runId}-{date}.csv` via a `downloadCsv(filename, content)` JS function using a Blob with `text/csv` MIME type.
