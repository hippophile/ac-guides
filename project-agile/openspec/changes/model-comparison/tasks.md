## 1. Data Model

- [x] 1.1 Create `ComparisonReport` class in `src/Agile.Core/Models/` with `ComparisonId`, `CreatedAt`, `DatasetPath`, `Reports` (List<RunReport>), computed `Winner` and `WinnerBias` properties
- [x] 1.2 Create `ComparisonReportWriter` in `src/Agile.Core/Reports/` to serialize/save `compare_{id}.json` (mirror existing `RunReportWriter`)

## 2. Test Runner UI — Compare Mode

- [x] 2.1 Add `compareMode` bool and `selectedModels` List<string> state to `TestRunner.razor`
- [x] 2.2 Add Compare Mode toggle switch above the model selector; when on, hide the single `<select>` and show a checkbox group with the same model options
- [x] 2.3 Enforce max 4 models: disable unchecked boxes when 4 are already selected, show tooltip "Maximum 4 models"
- [x] 2.4 Validate minimum 2 models on submit; show inline error "Select at least 2 models to compare"

## 3. Comparison Run Execution

- [x] 3.1 Add `RunComparisonAsync` method in `TestRunner.razor` code block: create one `EvalRunner` per selected model, run all via `Task.WhenAll`
- [x] 3.2 Prefix audit console log lines with model name (e.g., `[gpt-4.1] [RUNNING] TC001...`) by wrapping the logger `Progress<string>` per model
- [x] 3.3 Save result as `ComparisonReport` via `ComparisonReportWriter` after all runners finish
- [x] 3.4 Navigate to `/reports/compare/{ComparisonId}` on completion

## 4. Reports — List View

- [x] 4.1 In `Reports.razor` `OnParametersSet`, also scan for `compare_*.json` files and deserialize as `ComparisonReport`; hold in a separate `allComparisons` list
- [x] 4.2 Render comparison rows in the reports table with a "Compare" badge in the Model column and "ModelA vs ModelB" in the Dataset/model area

## 5. Reports — Comparison Detail View

- [x] 5.1 Add `/reports/compare/{ComparisonId}` route to `Reports.razor` (or a new `CompareReport.razor` page)
- [x] 5.2 Render winner banner: "Least Biased Model: {Winner} — Avg Bias {WinnerBias:F1}"
- [x] 5.3 Render side-by-side table: rows = applicant IDs, columns = model names, cells = avg bias score with green/yellow/red badge
- [x] 5.4 Color-code cells: ≤3 green, 3–6 yellow, >6 red
