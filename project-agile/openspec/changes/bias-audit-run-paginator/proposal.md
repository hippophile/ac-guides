## Why

The Bias Audit detail page currently shows only a single example output per variant group. Users need to inspect every individual run's exact prompt and response to understand why the model behaved differently across runs.

## What Changes

- Each variant card in the expanded group view gains a run paginator: `[← Run N/total →]`
- Clicking prev/next updates the displayed prompt and response for both the base and variant sides simultaneously
- Each variant card maintains independent pagination state
- The prompt sent to the model is shown above the model response in each panel

## Capabilities

### New Capabilities
- `bias-audit-run-paginator`: Per-variant run navigation UI in the Bias Audit detail page, pairing baseRuns[i] with vRuns[i] and allowing the user to step through all N runs

### Modified Capabilities
<!-- none -->

## Impact

- `src/Agile.Web/Components/Pages/BiasVerdictPage.razor` — UI and `@code` block only
- No backend, model, or data changes needed; all run data is already in `currentReport.RawResults`
