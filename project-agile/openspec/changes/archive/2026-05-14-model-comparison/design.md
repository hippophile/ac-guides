## Context

The platform runs `EvalRunner` against a single `IModelClient` and saves a `RunReport` JSON. The UI has one model selector in `TestRunner.razor` and the Reports page renders a single report. The goal is to extend this to N models with minimal new infrastructure — reuse everything, orchestrate above it.

Current data flow:
```
TestRunner → EvalRunner(client) → RunReport → reports/{id}.json
```

Target data flow:
```
TestRunner → [EvalRunner(client1), EvalRunner(client2), ...] → ComparisonReport → reports/compare_{id}.json
```

## Goals / Non-Goals

**Goals:**
- Multi-model selection in the existing Test Runner UI
- Run each model's evaluation concurrently (not sequentially)
- Produce a `ComparisonReport` with per-model `RunReport` results embedded
- Side-by-side table in Reports: applicant rows × model columns, bias score cells
- Surface winner (lowest avg bias) clearly
- Save comparison reports alongside single-model reports; list view distinguishes them

**Non-Goals:**
- Comparing runs across different datasets
- Comparing historical single-model runs (only fresh compare runs)
- PDF export (existing placeholder, separate effort)
- More than 4 models at once (UI constraint, keeps table readable)

## Decisions

**1. `ComparisonReport` wraps existing `RunReport`s**

Rather than a new flat schema, `ComparisonReport` holds a `List<RunReport>` — one per model. This means all existing report rendering logic works unchanged for each model's slice. The comparison view just reads across the list.

Alternative considered: flatten into a single result list with a `ModelId` discriminator. Rejected — would require rewriting ThresholdEvaluator, BiasScorer, and all report rendering.

**2. Run models concurrently via `Task.WhenAll`**

Each `EvalRunner` is independent. Fire them all at once, await all. Concurrency within each run is already controlled by `MaxConcurrency`.

Alternative considered: sequential runs. Rejected — doubles/triples wall time for no benefit.

**3. Comparison reports saved as `compare_{id}.json`**

Prefix distinguishes them from single-model reports in the file listing. The Reports list view checks for the prefix to render a "Compare" badge instead of a model badge.

Alternative considered: separate `/reports/compare/` directory. Rejected — overcomplicates the file discovery logic already in the razor pages.

**4. Multi-model selector: checkboxes over a multi-select dropdown**

Checkboxes are clearer for 2-4 models. The existing `<select>` becomes a checkbox group when "Compare Mode" is toggled.

## Risks / Trade-offs

- **Copilot CLI concurrency**: Running 4 models × 4 concurrency = 16 simultaneous sessions on one CLI process. The CLI has handled this in testing but may throttle. → Mitigation: cap compare mode at 4 models and document it.
- **Report list clutter**: Compare reports appear alongside single-model reports. The prefix badge differentiates them but the list grows faster. → Acceptable for now; pagination is a separate concern.
- **Long runtime**: 4 models × 4 iterations × 5 test cases = 80 API calls. Could take 2-3 minutes. → Show per-model progress bars in the live feed.

## Open Questions

- Should the comparison report show a per-model Audit Console log, or one combined log? (Lean toward combined, prefixed with model name)
- Should "winner" be determined purely by avg bias score, or weighted by consistency rate too?
