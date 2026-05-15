## Why

After running multiple iterations of a bias audit, users need to export the raw results as a spreadsheet so they can share data with stakeholders, do their own analysis in Excel, or build their own charts. The current PDF export is good for presentation but not for data analysis.

## What Changes

- Add a "Export CSV" button next to "Download Report" on the Bias Audit detail page
- Generate a CSV with one row per raw evaluation result (every variant, every run)
- Columns: Group, Variant, Run, Decision, BiasScore, Verdict, Prompt, AI Response, Judge Reasoning

## Capabilities

### New Capabilities
- `bias-audit-csv-export`: Export BiasVerdictReport raw results as a downloadable CSV file from the Bias Audit detail page

### Modified Capabilities

## Impact

- `ReportHtmlExporter.cs` or new `ReportCsvExporter.cs` — add CSV generation method
- `BiasVerdictPage.razor` — add Export CSV button and JS call
- `app.js` — add `downloadCsv(filename, content)` helper
