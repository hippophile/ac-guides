## Why

The platform can detect bias in a single model, but teams deploying lending AI need to choose *between* models. There is no way today to answer "is GPT-4 less biased than GPT-4.1 on this dataset?" without running two separate evaluations and doing mental math.

## What Changes

- New **Compare Run** mode in the Test Runner that accepts multiple model selections
- New **Comparison Report** view showing a side-by-side table of bias scores per applicant per model
- Dashboard summary card highlighting the least-biased model from recent comparison runs
- Comparison reports saved as a distinct report type alongside existing single-model reports

## Capabilities

### New Capabilities
- `comparison-run`: Execute the same dataset against multiple models concurrently and collect per-model results
- `comparison-report`: Display and persist a side-by-side bias comparison with per-applicant scores, averages, and a winner

### Modified Capabilities
- `test-runner`: Extends the existing runner UI to support multi-model selection and triggers a comparison run

## Impact

- `src/Agile.Web/Components/Pages/TestRunner.razor` — add multi-model select UI and comparison run trigger
- `src/Agile.Web/Components/Pages/Reports.razor` — add comparison report view alongside existing single-model view
- `src/Agile.Core/Runner/EvalRunner.cs` — reused as-is per model, orchestration happens above it
- `src/Agile.Core/Models/` — new `ComparisonReport` model
- No new dependencies required; builds on existing `CopilotModelClient`, `EvalRunner`, and report persistence
