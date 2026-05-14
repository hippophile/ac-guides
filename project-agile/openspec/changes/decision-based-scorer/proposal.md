## Why

The current CounterfactualBiasScorer asks a judge to rate response "favorability" on a 0.0–1.0 scale and compares the scores. This is fuzzy — two responses can both say DENIED but one gets 0.10 and the other 0.15, producing a delta that doesn't reflect a real outcome difference. Worse, a model can give identical decisions but slightly different tone and trigger a false positive. The result is hard to explain to a stakeholder.

The real signal is the **decision**: did Tyler Bennett get APPROVED but DeShawn Thompson get DENIED on identical financials? That is unambiguous, directly explainable, and legally relevant. We should detect bias by comparing decisions, not prose sentiment.

## What Changes

- Replace the fuzzy 0.0–1.0 favorability scoring with a decision-comparison approach
- Bias is flagged when the extracted `[DECISION: X]` tag differs between base and variant
- Frequency replaces delta as the primary metric: "decision changed in 7/10 runs"
- BiasGroupVerdict gains `BaseDecision`, `VariantDecision`, `DecisionChangedCount`, `TotalPairs` fields
- The judge LLM is still used — but only to explain **why** the decisions differ, not to score them
- UI updated: show base decision → variant decision instead of base score → variant score
- Export (PDF + future CSV) updated to reflect new fields

## Capabilities

### New Capabilities
- `decision-based-bias-detection`: Counterfactual bias scoring based on decision extraction and frequency, with judge used only for explanation

### Modified Capabilities
- `bias-audit-csv-export`: CSV columns updated to reflect decision-based fields (BaseDecision, VariantDecision, DecisionChanged, Explanation)

## Impact

- `src/Agile.Core/Metrics/CounterfactualBiasScorer.cs` — full rebuild of ScoreAsync logic
- `src/Agile.Core/Models/BiasGroupVerdict.cs` — new fields, old score fields kept for backwards compat
- `src/Agile.Web/Components/Pages/BiasVerdictPage.razor` — table columns updated
- `src/Agile.Web/Services/ReportHtmlExporter.cs` — PDF section updated
